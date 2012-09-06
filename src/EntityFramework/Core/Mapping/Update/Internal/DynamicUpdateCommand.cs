// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Spatial;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using IEntityStateEntry = System.Data.Entity.Core.IEntityStateEntry;

    internal class DynamicUpdateCommand : UpdateCommand
    {
        private readonly ModificationOperator _operator;
        private readonly TableChangeProcessor _processor;
        private readonly List<KeyValuePair<int, DbSetClause>> _inputIdentifiers;
        private readonly Dictionary<int, string> _outputIdentifiers;
        private readonly DbModificationCommandTree _modificationCommandTree;

        internal DynamicUpdateCommand(
            TableChangeProcessor processor, UpdateTranslator translator,
            ModificationOperator modificationOperator, PropagatorResult originalValues, PropagatorResult currentValues,
            DbModificationCommandTree tree, Dictionary<int, string> outputIdentifiers)
            : base(translator, originalValues, currentValues)
        {
            Contract.Requires(processor != null);
            Contract.Requires(translator != null);
            Contract.Requires(tree != null);

            _processor = processor;
            _operator = modificationOperator;
            _modificationCommandTree = tree;
            _outputIdentifiers = outputIdentifiers; // may be null (not all commands have output identifiers)

            // initialize identifier information (supports lateral propagation of server gen values)
            if (ModificationOperator.Insert == modificationOperator
                || ModificationOperator.Update == modificationOperator)
            {
                const int capacity = 2; // "average" number of identifiers per row
                _inputIdentifiers = new List<KeyValuePair<int, DbSetClause>>(capacity);

                foreach (var member in
                    Helper.PairEnumerations(
                        TypeHelpers.GetAllStructuralMembers(CurrentValues.StructuralType),
                        CurrentValues.GetMemberValues()))
                {
                    DbSetClause setter;
                    var identifier = member.Value.Identifier;

                    if (PropagatorResult.NullIdentifier != identifier
                        &&
                        TryGetSetterExpression(tree, member.Key, modificationOperator, out setter)) // can find corresponding setter
                    {
                        foreach (var principal in translator.KeyManager.GetPrincipals(identifier))
                        {
                            _inputIdentifiers.Add(new KeyValuePair<int, DbSetClause>(principal, setter));
                        }
                    }
                }
            }
        }

        // effects: try to find setter expression for the given member
        // requires: command tree must be an insert or update tree (since other DML trees hnabve 
        private static bool TryGetSetterExpression(
            DbModificationCommandTree tree, EdmMember member, ModificationOperator op, out DbSetClause setter)
        {
            Debug.Assert(op == ModificationOperator.Insert || op == ModificationOperator.Update, "only inserts and updates have setters");
            IEnumerable<DbModificationClause> clauses;
            if (ModificationOperator.Insert == op)
            {
                clauses = ((DbInsertCommandTree)tree).SetClauses;
            }
            else
            {
                clauses = ((DbUpdateCommandTree)tree).SetClauses;
            }
            foreach (DbSetClause setClause in clauses)
            {
                // check if this is the correct setter
                if (((DbPropertyExpression)setClause.Property).Property.EdmEquals(member))
                {
                    setter = setClause;
                    return true;
                }
            }

            // no match found
            setter = null;
            return false;
        }

        /// <summary>
        ///     See comments in <see cref="UpdateCommand" />.
        /// </summary>
        internal override long Execute(
            Dictionary<int, object> identifierValues,
            List<KeyValuePair<PropagatorResult, object>> generatedValues,
            IDbCommandInterceptor commandInterceptor)
        {
            // Compile command
            using (var command = CreateCommand(identifierValues))
            {
                var connection = Translator.Connection;
                // configure command to use the connection and transaction for this session
                command.Transaction = ((null == connection.CurrentTransaction)
                                           ? null
                                           : connection.CurrentTransaction.StoreTransaction);
                command.Connection = connection.StoreConnection;
                if (Translator.CommandTimeout.HasValue)
                {
                    command.CommandTimeout = Translator.CommandTimeout.Value;
                }

                // Execute the query
                int rowsAffected;
                if (_modificationCommandTree.HasReader)
                {
                    // retrieve server gen results
                    rowsAffected = 0;
                    using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
                    {
                        if (reader.Read())
                        {
                            rowsAffected++;

                            var members = TypeHelpers.GetAllStructuralMembers(CurrentValues.StructuralType);

                            for (var ordinal = 0; ordinal < reader.FieldCount; ordinal++)
                            {
                                // column name of result corresponds to column name of table
                                var columnName = reader.GetName(ordinal);
                                var member = members[columnName];
                                object value;
                                if (Helper.IsSpatialType(member.TypeUsage)
                                    && !reader.IsDBNull(ordinal))
                                {
                                    value = SpatialHelpers.GetSpatialValue(Translator.MetadataWorkspace, reader, member.TypeUsage, ordinal);
                                }
                                else
                                {
                                    value = reader.GetValue(ordinal);
                                }

                                // retrieve result which includes the context for back-propagation
                                var columnOrdinal = members.IndexOf(member);
                                var result = CurrentValues.GetMemberValue(columnOrdinal);

                                // register for back-propagation
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
                        // executing the command can be intercepted
                        CommandHelper.ConsumeReader(reader);
                    }
                }
                else
                {
                    // We currently only intercept commands on this code path.

                    var executeCommand = true;

                    if (commandInterceptor != null)
                    {
                        executeCommand = commandInterceptor.Intercept(command);
                    }

                    rowsAffected = executeCommand ? command.ExecuteNonQuery() : 1;
                }

                return rowsAffected;
            }
        }

#if !NET40

        /// <summary>
        ///     See comments in <see cref="UpdateCommand" />.
        /// </summary>
        internal override async Task<long> ExecuteAsync(
            Dictionary<int, object> identifierValues,
            List<KeyValuePair<PropagatorResult, object>> generatedValues, CancellationToken cancellationToken)
        {
            // Compile command
            using (var command = CreateCommand(identifierValues))
            {
                var connection = Translator.Connection;
                // configure command to use the connection and transaction for this session
                command.Transaction = ((null == connection.CurrentTransaction)
                                           ? null
                                           : connection.CurrentTransaction.StoreTransaction);
                command.Connection = connection.StoreConnection;
                if (Translator.CommandTimeout.HasValue)
                {
                    command.CommandTimeout = Translator.CommandTimeout.Value;
                }

                // Execute the query
                int rowsAffected;
                if (_modificationCommandTree.HasReader)
                {
                    // retrieve server gen results
                    rowsAffected = 0;
                    using (
                        var reader =
                            await
                            command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken).ConfigureAwait(
                                continueOnCapturedContext: false))
                    {
                        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                        {
                            rowsAffected++;

                            var members = TypeHelpers.GetAllStructuralMembers(CurrentValues.StructuralType);

                            for (var ordinal = 0; ordinal < reader.FieldCount; ordinal++)
                            {
                                // column name of result corresponds to column name of table
                                var columnName = reader.GetName(ordinal);
                                var member = members[columnName];
                                object value;
                                if (Helper.IsSpatialType(member.TypeUsage)
                                    &&
                                    !await reader.IsDBNullAsync(ordinal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
                                {
                                    value =
                                        await
                                        SpatialHelpers.GetSpatialValueAsync(
                                            Translator.MetadataWorkspace, reader, member.TypeUsage, ordinal, cancellationToken).
                                            ConfigureAwait(continueOnCapturedContext: false);
                                }
                                else
                                {
                                    value =
                                        await
                                        reader.GetFieldValueAsync<object>(ordinal, cancellationToken).ConfigureAwait(
                                            continueOnCapturedContext: false);
                                }

                                // retrieve result which includes the context for back-propagation
                                var columnOrdinal = members.IndexOf(member);
                                var result = CurrentValues.GetMemberValue(columnOrdinal);

                                // register for back-propagation
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
                        // executing the command can be intercepted
                        await CommandHelper.ConsumeReaderAsync(reader, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                    }
                }
                else
                {
                    rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                }

                return rowsAffected;
            }
        }

#endif

        /// <summary>
        ///     Gets DB command definition encapsulating store logic for this command.
        /// </summary>
        protected virtual DbCommand CreateCommand(Dictionary<int, object> identifierValues)
        {
            var commandTree = _modificationCommandTree;

            // check if any server gen identifiers need to be set
            if (null != _inputIdentifiers)
            {
                var modifiedClauses = new Dictionary<DbSetClause, DbSetClause>();
                for (var idx = 0; idx < _inputIdentifiers.Count; idx++)
                {
                    var inputIdentifier = _inputIdentifiers[idx];

                    object value;
                    if (identifierValues.TryGetValue(inputIdentifier.Key, out value))
                    {
                        // reset the value of the identifier
                        var newClause = new DbSetClause(inputIdentifier.Value.Property, DbExpressionBuilder.Constant(value));
                        modifiedClauses[inputIdentifier.Value] = newClause;
                        _inputIdentifiers[idx] = new KeyValuePair<int, DbSetClause>(inputIdentifier.Key, newClause);
                    }
                }
                commandTree = RebuildCommandTree(commandTree, modifiedClauses);
            }

            return Translator.CreateCommand(commandTree);
        }

        private static DbModificationCommandTree RebuildCommandTree(
            DbModificationCommandTree originalTree, Dictionary<DbSetClause, DbSetClause> clauseMappings)
        {
            if (clauseMappings.Count == 0)
            {
                return originalTree;
            }

            DbModificationCommandTree result;
            Debug.Assert(
                originalTree.CommandTreeKind == DbCommandTreeKind.Insert || originalTree.CommandTreeKind == DbCommandTreeKind.Update,
                "Set clauses specified for a modification tree that is not an update or insert tree?");
            if (originalTree.CommandTreeKind
                == DbCommandTreeKind.Insert)
            {
                var insertTree = (DbInsertCommandTree)originalTree;
                result = new DbInsertCommandTree(
                    insertTree.MetadataWorkspace, insertTree.DataSpace,
                    insertTree.Target, ReplaceClauses(insertTree.SetClauses, clauseMappings).AsReadOnly(), insertTree.Returning);
            }
            else
            {
                var updateTree = (DbUpdateCommandTree)originalTree;
                result = new DbUpdateCommandTree(
                    updateTree.MetadataWorkspace, updateTree.DataSpace,
                    updateTree.Target, updateTree.Predicate, ReplaceClauses(updateTree.SetClauses, clauseMappings).AsReadOnly(),
                    updateTree.Returning);
            }

            return result;
        }

        /// <summary>
        ///     Creates a new list of modification clauses with the specified remapped clauses replaced.
        /// </summary>
        private static List<DbModificationClause> ReplaceClauses(
            IList<DbModificationClause> originalClauses, Dictionary<DbSetClause, DbSetClause> mappings)
        {
            var result = new List<DbModificationClause>(originalClauses.Count);
            for (var idx = 0; idx < originalClauses.Count; idx++)
            {
                DbSetClause replacementClause;
                if (mappings.TryGetValue((DbSetClause)originalClauses[idx], out replacementClause))
                {
                    result.Add(replacementClause);
                }
                else
                {
                    result.Add(originalClauses[idx]);
                }
            }
            return result;
        }

        internal ModificationOperator Operator
        {
            get { return _operator; }
        }

        internal override EntitySet Table
        {
            get { return _processor.Table; }
        }

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
            get { return UpdateCommandKind.Dynamic; }
        }

        internal override IList<IEntityStateEntry> GetStateEntries(UpdateTranslator translator)
        {
            var stateEntries = new List<IEntityStateEntry>(2);
            if (null != OriginalValues)
            {
                foreach (var stateEntry in SourceInterpreter.GetAllStateEntries(
                    OriginalValues, translator, Table))
                {
                    stateEntries.Add(stateEntry);
                }
            }

            if (null != CurrentValues)
            {
                foreach (var stateEntry in SourceInterpreter.GetAllStateEntries(
                    CurrentValues, translator, Table))
                {
                    stateEntries.Add(stateEntry);
                }
            }
            return stateEntries;
        }

        internal override int CompareToType(UpdateCommand otherCommand)
        {
            Debug.Assert(!ReferenceEquals(this, otherCommand), "caller is supposed to ensure otherCommand is different reference");

            var other = (DynamicUpdateCommand)otherCommand;

            // order by operation type
            var result = (int)Operator - (int)other.Operator;
            if (0 != result)
            {
                return result;
            }

            // order by Container.Table
            result = StringComparer.Ordinal.Compare(_processor.Table.Name, other._processor.Table.Name);
            if (0 != result)
            {
                return result;
            }
            result = StringComparer.Ordinal.Compare(_processor.Table.EntityContainer.Name, other._processor.Table.EntityContainer.Name);
            if (0 != result)
            {
                return result;
            }

            // order by table key
            var thisResult = (Operator == ModificationOperator.Delete ? OriginalValues : CurrentValues);
            var otherResult = (other.Operator == ModificationOperator.Delete ? other.OriginalValues : other.CurrentValues);
            for (var i = 0; i < _processor.KeyOrdinals.Length; i++)
            {
                var keyOrdinal = _processor.KeyOrdinals[i];
                var thisValue = thisResult.GetMemberValue(keyOrdinal).GetSimpleValue();
                var otherValue = otherResult.GetMemberValue(keyOrdinal).GetSimpleValue();
                result = ByValueComparer.Default.Compare(thisValue, otherValue);
                if (0 != result)
                {
                    return result;
                }
            }

            // If the result is still zero, it means key values are all the same. Switch to synthetic identifiers
            // to differentiate.
            for (var i = 0; i < _processor.KeyOrdinals.Length; i++)
            {
                var keyOrdinal = _processor.KeyOrdinals[i];
                var thisValue = thisResult.GetMemberValue(keyOrdinal).Identifier;
                var otherValue = otherResult.GetMemberValue(keyOrdinal).Identifier;
                result = thisValue - otherValue;
                if (0 != result)
                {
                    return result;
                }
            }

            return result;
        }
    }
}
