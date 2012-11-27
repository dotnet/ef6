// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using IEntityStateEntry = System.Data.Entity.Core.IEntityStateEntry;

    /// <summary>
    ///     Aggregates information about a modification command delegated to a store function.
    /// </summary>
    internal class FunctionUpdateCommand : UpdateCommand
    {
        #region Constructors

        /// <summary>
        ///     Initialize a new function command. Initializes the command object.
        /// </summary>
        /// <param name="functionMapping"> Function mapping metadata </param>
        /// <param name="translator"> Translator </param>
        /// <param name="stateEntries"> State entries handled by this operation. </param>
        /// <param name="stateEntry"> 'Root' state entry being handled by this function. </param>
        internal FunctionUpdateCommand(
            StorageModificationFunctionMapping functionMapping, UpdateTranslator translator,
            ReadOnlyCollection<IEntityStateEntry> stateEntries, ExtractedStateEntry stateEntry)
            : this(translator, stateEntries, stateEntry,
                translator.GenerateCommandDefinition(functionMapping).CreateCommand())
        {
            DebugCheck.NotNull(functionMapping);
            DebugCheck.NotNull(translator);
            DebugCheck.NotNull(stateEntries);
        }

        protected FunctionUpdateCommand(
            UpdateTranslator translator, ReadOnlyCollection<IEntityStateEntry> stateEntries, ExtractedStateEntry stateEntry,
            DbCommand dbCommand)
            : base(translator, stateEntry.Original, stateEntry.Current)
        {
            // populate the main state entry for error reporting
            _stateEntries = stateEntries;

            _dbCommand = dbCommand;
        }

        #endregion

        #region Fields

        private readonly ReadOnlyCollection<IEntityStateEntry> _stateEntries;

        /// <summary>
        ///     Gets the store command wrapped by this command.
        /// </summary>
        private readonly DbCommand _dbCommand;

        /// <summary>
        ///     Gets map from identifiers (key component proxies) to parameters holding the actual
        ///     key values. Supports propagation of identifier values (fixup for server-gen keys)
        /// </summary>
        private List<KeyValuePair<int, DbParameter>> _inputIdentifiers;

        /// <summary>
        ///     Gets map from identifiers (key component proxies) to column names producing the actual
        ///     key values. Supports propagation of identifier values (fixup for server-gen keys)
        /// </summary>
        private Dictionary<int, string> _outputIdentifiers;

        /// <summary>
        ///     Gets a reference to the rows affected output parameter for the stored procedure. May be null.
        /// </summary>
        private DbParameter _rowsAffectedParameter;

        #endregion

        #region Properties

        /// <summary>
        ///     Pairs for column names and propagator results (so that we can associate reader results with
        ///     the source records for server generated values).
        /// </summary>
        protected virtual List<KeyValuePair<string, PropagatorResult>> ResultColumns { get; set; }

        internal override IEnumerable<int> InputIdentifiers
        {
            get
            {
                if (null == _inputIdentifiers)
                {
                    yield break;
                }
                else
                {
                    foreach (var inputIdentifier in _inputIdentifiers)
                    {
                        yield return inputIdentifier.Key;
                    }
                }
            }
        }

        internal override IEnumerable<int> OutputIdentifiers
        {
            get
            {
                if (null == _outputIdentifiers)
                {
                    return Enumerable.Empty<int>();
                }
                return _outputIdentifiers.Keys;
            }
        }

        internal override UpdateCommandKind Kind
        {
            get { return UpdateCommandKind.Function; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets state entries contributing to this function. Supports error reporting.
        /// </summary>
        internal override IList<IEntityStateEntry> GetStateEntries(UpdateTranslator translator)
        {
            return _stateEntries;
        }

        // Adds and register a DbParameter to the current command.
        internal void SetParameterValue(
            PropagatorResult result,
            StorageModificationFunctionParameterBinding parameterBinding, UpdateTranslator translator)
        {
            // retrieve DbParameter
            var parameter = _dbCommand.Parameters[parameterBinding.Parameter.Name];
            var parameterType = parameterBinding.Parameter.TypeUsage;
            var parameterValue = translator.KeyManager.GetPrincipalValue(result);
            translator.SetParameterValue(parameter, parameterType, parameterValue);

            // if the parameter corresponds to an identifier (key component), remember this fact in case
            // it's important for dependency ordering (e.g., output the identifier before creating it)
            var identifier = result.Identifier;
            if (PropagatorResult.NullIdentifier != identifier)
            {
                const int initialSize = 2; // expect on average less than two input identifiers per command
                if (null == _inputIdentifiers)
                {
                    _inputIdentifiers = new List<KeyValuePair<int, DbParameter>>(initialSize);
                }
                foreach (var principal in translator.KeyManager.GetPrincipals(identifier))
                {
                    _inputIdentifiers.Add(new KeyValuePair<int, DbParameter>(principal, parameter));
                }
            }
        }

        // Adds and registers a DbParameter taking the number of rows affected
        internal void RegisterRowsAffectedParameter(FunctionParameter rowsAffectedParameter)
        {
            if (null != rowsAffectedParameter)
            {
                Debug.Assert(
                    rowsAffectedParameter.Mode == ParameterMode.Out || rowsAffectedParameter.Mode == ParameterMode.InOut,
                    "when loading mapping metadata, we check that the parameter is an out parameter");
                _rowsAffectedParameter = _dbCommand.Parameters[rowsAffectedParameter.Name];
            }
        }

        // Adds a result column binding from a column name (from the result set for the function) to
        // a propagator result (which contains the context necessary to back-propagate the result).
        // If the result is an identifier, binds the 
        internal void AddResultColumn(UpdateTranslator translator, String columnName, PropagatorResult result)
        {
            const int initializeSize = 2; // expect on average less than two result columns per command
            if (null == ResultColumns)
            {
                ResultColumns = new List<KeyValuePair<string, PropagatorResult>>(initializeSize);
            }
            ResultColumns.Add(new KeyValuePair<string, PropagatorResult>(columnName, result));

            var identifier = result.Identifier;
            if (PropagatorResult.NullIdentifier != identifier)
            {
                if (translator.KeyManager.HasPrincipals(identifier))
                {
                    throw new InvalidOperationException(Strings.Update_GeneratedDependent(columnName));
                }

                // register output identifier to enable fix-up and dependency tracking
                AddOutputIdentifier(columnName, identifier);
            }
        }

        // Indicate that a column in the command result set (specified by 'columnName') produces the
        // value for a key component (specified by 'identifier')
        private void AddOutputIdentifier(String columnName, int identifier)
        {
            const int initialSize = 2; // expect on average less than two identifier output per command
            if (null == _outputIdentifiers)
            {
                _outputIdentifiers = new Dictionary<int, string>(initialSize);
            }
            _outputIdentifiers[identifier] = columnName;
        }

        /// <summary>
        ///     Sets all identifier input values (to support propagation of identifier values across relationship
        ///     boundaries).
        /// </summary>
        /// <param name="identifierValues"> Input values to set. </param>
        internal virtual void SetInputIdentifiers(Dictionary<int, object> identifierValues)
        {
            if (null != _inputIdentifiers)
            {
                foreach (var inputIdentifier in _inputIdentifiers)
                {
                    object value;
                    if (identifierValues.TryGetValue(inputIdentifier.Key, out value))
                    {
                        // set the actual value for the identifier if it has been produced by some
                        // other command
                        inputIdentifier.Value.Value = value;
                    }
                }
            }
        }

        /// <summary>
        ///     See comments in <see cref="UpdateCommand" />.
        /// </summary>
        internal override long Execute(
            Dictionary<int, object> identifierValues,
            List<KeyValuePair<PropagatorResult, object>> generatedValues,
            IDbCommandInterceptor commandInterceptor)
        {
            var connection = Translator.Connection;
            // configure command to use the connection and transaction for this session
            _dbCommand.Transaction = ((null == connection.CurrentTransaction)
                                          ? null
                                          : connection.CurrentTransaction.StoreTransaction);
            _dbCommand.Connection = connection.StoreConnection;
            if (Translator.CommandTimeout.HasValue)
            {
                _dbCommand.CommandTimeout = Translator.CommandTimeout.Value;
            }

            SetInputIdentifiers(identifierValues);

            // Execute the query
            long rowsAffected;
            if (null != ResultColumns)
            {
                // If there are result columns, read the server gen results
                rowsAffected = 0;
                var members = TypeHelpers.GetAllStructuralMembers(CurrentValues.StructuralType);
                using (var reader = _dbCommand.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    // Retrieve only the first row from the first result set
                    if (reader.Read())
                    {
                        rowsAffected++;

                        foreach (var resultColumn in ResultColumns
                            .Select(r => new KeyValuePair<int, PropagatorResult>(GetColumnOrdinal(Translator, reader, r.Key), r.Value))
                            .OrderBy(r => r.Key)) // order by column ordinal to avoid breaking SequentialAccess readers
                        {
                            var columnOrdinal = resultColumn.Key;
                            var columnType = members[resultColumn.Value.RecordOrdinal].TypeUsage;
                            object value;

                            if (Helper.IsSpatialType(columnType)
                                && !reader.IsDBNull(columnOrdinal))
                            {
                                value = SpatialHelpers.GetSpatialValue(Translator.MetadataWorkspace, reader, columnType, columnOrdinal);
                            }
                            else
                            {
                                value = reader.GetValue(columnOrdinal);
                            }

                            // register for back-propagation
                            var result = resultColumn.Value;
                            generatedValues.Add(new KeyValuePair<PropagatorResult, object>(result, value));

                            // register identifier if it exists
                            var identifier = result.Identifier;
                            if (PropagatorResult.NullIdentifier != identifier)
                            {
                                identifierValues.Add(identifier, value);
                            }
                        }
                    }

                    // Consume the current reader (and subsequent result sets) so that any errors
                    // executing the function can be intercepted
                    CommandHelper.ConsumeReader(reader);
                }
            }
            else
            {
                rowsAffected = _dbCommand.ExecuteNonQuery();
            }

            return GetRowsAffected(rowsAffected, Translator);
        }

#if !NET40

        /// <summary>
        ///     See comments in <see cref="UpdateCommand" />.
        /// </summary>
        internal override async Task<long> ExecuteAsync(
            Dictionary<int, object> identifierValues,
            List<KeyValuePair<PropagatorResult, object>> generatedValues, CancellationToken cancellationToken)
        {
            var connection = Translator.Connection;
            // configure command to use the connection and transaction for this session
            _dbCommand.Transaction = ((null == connection.CurrentTransaction)
                                          ? null
                                          : connection.CurrentTransaction.StoreTransaction);
            _dbCommand.Connection = connection.StoreConnection;
            if (Translator.CommandTimeout.HasValue)
            {
                _dbCommand.CommandTimeout = Translator.CommandTimeout.Value;
            }

            SetInputIdentifiers(identifierValues);

            // Execute the query
            long rowsAffected;
            if (null != ResultColumns)
            {
                // If there are result columns, read the server gen results
                rowsAffected = 0;
                var members = TypeHelpers.GetAllStructuralMembers(CurrentValues.StructuralType);
                using (
                    var reader =
                        await
                        _dbCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken).ConfigureAwait(
                            continueOnCapturedContext: false))
                {
                    // Retrieve only the first row from the first result set
                    if (await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                    {
                        rowsAffected++;

                        foreach (var resultColumn in ResultColumns
                            .Select(r => new KeyValuePair<int, PropagatorResult>(GetColumnOrdinal(Translator, reader, r.Key), r.Value))
                            .OrderBy(r => r.Key)) // order by column ordinal to avoid breaking SequentialAccess readers
                        {
                            var columnOrdinal = resultColumn.Key;
                            var columnType = members[resultColumn.Value.RecordOrdinal].TypeUsage;
                            object value;

                            if (Helper.IsSpatialType(columnType)
                                &&
                                !await
                                 reader.IsDBNullAsync(columnOrdinal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                            {
                                value =
                                    await
                                    SpatialHelpers.GetSpatialValueAsync(
                                        Translator.MetadataWorkspace, reader, columnType, columnOrdinal, cancellationToken).ConfigureAwait(
                                            continueOnCapturedContext: false);
                            }
                            else
                            {
                                value =
                                    await
                                    reader.GetFieldValueAsync<object>(columnOrdinal, cancellationToken).ConfigureAwait(
                                        continueOnCapturedContext: false);
                            }

                            // register for back-propagation
                            var result = resultColumn.Value;
                            generatedValues.Add(new KeyValuePair<PropagatorResult, object>(result, value));

                            // register identifier if it exists
                            var identifier = result.Identifier;
                            if (PropagatorResult.NullIdentifier != identifier)
                            {
                                identifierValues.Add(identifier, value);
                            }
                        }
                    }

                    // Consume the current reader (and subsequent result sets) so that any errors
                    // executing the function can be intercepted
                    await CommandHelper.ConsumeReaderAsync(reader, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                }
            }
            else
            {
                rowsAffected = await _dbCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }

            return GetRowsAffected(rowsAffected, Translator);
        }

#endif

        protected virtual long GetRowsAffected(long rowsAffected, UpdateTranslator translator)
        {
            // if an explicit rows affected parameter exists, use this value instead
            if (null != _rowsAffectedParameter)
            {
                // by design, negative row counts indicate failure iff. an explicit rows
                // affected parameter is used
                if (DBNull.Value.Equals(_rowsAffectedParameter.Value))
                {
                    rowsAffected = 0;
                }
                else
                {
                    try
                    {
                        rowsAffected = Convert.ToInt64(_rowsAffectedParameter.Value, CultureInfo.InvariantCulture);
                    }
                    catch (Exception e)
                    {
                        if (e.RequiresContext())
                        {
                            // wrap the exception
                            throw new UpdateException(
                                Strings.Update_UnableToConvertRowsAffectedParameter(
                                    _rowsAffectedParameter.ParameterName, typeof(Int64).FullName),
                                e, GetStateEntries(translator).Cast<ObjectStateEntry>().Distinct());
                        }
                        throw;
                    }
                }
            }

            return rowsAffected;
        }

        private int GetColumnOrdinal(UpdateTranslator translator, DbDataReader reader, string columnName)
        {
            int columnOrdinal;
            try
            {
                columnOrdinal = reader.GetOrdinal(columnName);
            }
            catch (IndexOutOfRangeException)
            {
                throw new UpdateException(
                    Strings.Update_MissingResultColumn(columnName), null, GetStateEntries(translator).Cast<ObjectStateEntry>().Distinct());
            }
            return columnOrdinal;
        }

        /// <summary>
        ///     Gets modification operator corresponding to the given entity state.
        /// </summary>
        private static ModificationOperator GetModificationOperator(EntityState state)
        {
            switch (state)
            {
                case EntityState.Modified:
                case EntityState.Unchanged:
                    // unchanged entities correspond to updates (consider the case where
                    // the entity is not being modified but a collocated relationship is)
                    return ModificationOperator.Update;

                case EntityState.Added:
                    return ModificationOperator.Insert;

                case EntityState.Deleted:
                    return ModificationOperator.Delete;

                default:
                    Debug.Fail("unexpected entity state " + state);
                    return default(ModificationOperator);
            }
        }

        internal override int CompareToType(UpdateCommand otherCommand)
        {
            Debug.Assert(!ReferenceEquals(this, otherCommand), "caller should ensure other command is different");

            var other = (FunctionUpdateCommand)otherCommand;

            // first state entry is the 'main' state entry for the command (see ctor)
            var thisParent = _stateEntries[0];
            var otherParent = other._stateEntries[0];

            // order by operator
            var result = (int)GetModificationOperator(thisParent.State) -
                         (int)GetModificationOperator(otherParent.State);
            if (0 != result)
            {
                return result;
            }

            // order by entity set
            result = StringComparer.Ordinal.Compare(thisParent.EntitySet.Name, otherParent.EntitySet.Name);
            if (0 != result)
            {
                return result;
            }
            result = StringComparer.Ordinal.Compare(thisParent.EntitySet.EntityContainer.Name, otherParent.EntitySet.EntityContainer.Name);
            if (0 != result)
            {
                return result;
            }

            // order by key values
            var thisInputIdentifierCount = (null == _inputIdentifiers ? 0 : _inputIdentifiers.Count);
            var otherInputIdentifierCount = (null == other._inputIdentifiers ? 0 : other._inputIdentifiers.Count);
            result = thisInputIdentifierCount - otherInputIdentifierCount;
            if (0 != result)
            {
                return result;
            }
            for (var i = 0; i < thisInputIdentifierCount; i++)
            {
                var thisParameter = _inputIdentifiers[i].Value;
                var otherParameter = other._inputIdentifiers[i].Value;
                result = ByValueComparer.Default.Compare(thisParameter.Value, otherParameter.Value);
                if (0 != result)
                {
                    return result;
                }
            }

            // If the result is still zero, it means key values are all the same. Switch to synthetic identifiers
            // to differentiate.
            for (var i = 0; i < thisInputIdentifierCount; i++)
            {
                var thisIdentifier = _inputIdentifiers[i].Key;
                var otherIdentifier = other._inputIdentifiers[i].Key;
                result = thisIdentifier - otherIdentifier;
                if (0 != result)
                {
                    return result;
                }
            }

            return result;
        }

        #endregion
    }
}
