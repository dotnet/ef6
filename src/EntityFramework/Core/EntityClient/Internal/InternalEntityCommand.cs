namespace System.Data.Entity.Core.EntityClient.Internal
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.EntitySql;
    using System.Data.Entity.Core.Common.QueryCache;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// See comments on <see cref="EntityCommand"/> class.
    /// </summary>
    internal class InternalEntityCommand
    {
        #region Fields

        private const int InvalidCloseCount = -1;

        private bool _designTimeVisible;
        private string _esqlCommandText;
        private EntityConnection _connection;
        private DbCommandTree _preparedCommandTree;
        private readonly EntityParameterCollection _parameters;
        private int? _commandTimeout;
        private CommandType _commandType;
        private EntityTransaction _transaction;
        private UpdateRowSource _updatedRowSource;
        private EntityCommandDefinition _commandDefinition;
        private bool _isCommandDefinitionBased;
        private DbCommandTree _commandTreeSetByUser;
        private DbDataReader _dataReader;
        private bool _enableQueryPlanCaching;
        private DbCommand _storeProviderCommand;
        private EntityDataReaderFactory _entityDataReaderFactory;

        #endregion

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public InternalEntityCommand(EntityDataReaderFactory factory = null)
        {
            // Initalize the member field with proper default values
            _designTimeVisible = true;
            _commandType = CommandType.Text;
            _updatedRowSource = UpdateRowSource.Both;
            _parameters = new EntityParameterCollection();

            // Future Enhancement: (See SQLPT #300004256) At some point it would be  
            // really nice to read defaults from a global configuration, but we're not 
            // doing that today.  
            _enableQueryPlanCaching = true;

            _entityDataReaderFactory = factory ?? new EntityDataReaderFactory();
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public InternalEntityCommand(string statement, EntityDataReaderFactory factory = null)
            : this(factory)
        {
            // Assign other member fields from the parameters
            _esqlCommandText = statement;
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public InternalEntityCommand(string statement, EntityConnection connection, EntityDataReaderFactory factory = null)
            : this(statement, factory)
        {
            // Assign other member fields from the parameters
            _connection = connection;
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public InternalEntityCommand(string statement, EntityConnection connection, EntityTransaction transaction, EntityDataReaderFactory factory = null)
            : this(statement, connection, factory)
        {
            // Assign other member fields from the parameters
            _transaction = transaction;
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        internal InternalEntityCommand(EntityCommandDefinition commandDefinition, EntityDataReaderFactory factory = null)
            : this(factory)
        {
            // Assign other member fields from the parameters
            _commandDefinition = commandDefinition;
            _parameters = new EntityParameterCollection();

            // Make copies of the parameters
            foreach (var parameter in commandDefinition.Parameters)
            {
                _parameters.Add(parameter.Clone());
            }

            // Reset the dirty flag that was set to true when the parameters were added so that it won't say
            // it's dirty to start with
            _parameters.ResetIsDirty();

            // Track the fact that this command was created from and represents an already prepared command definition
            _isCommandDefinitionBased = true;
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        internal InternalEntityCommand(EntityConnection connection, EntityCommandDefinition entityCommandDefinition, EntityDataReaderFactory factory = null)
            : this(entityCommandDefinition, factory)
        {
            _connection = connection;
        }

        /// <summary>
        /// Wrapper on the parent class, for accessing its protected members (via proxy method) 
        /// or when the parent class is a parameter to another method/constructor
        /// </summary>
        internal EntityCommand EntityCommandWrapper { get; set; }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public virtual EntityConnection Connection
        {
            get { return _connection; }
            set
            {
                ThrowIfDataReaderIsOpen();
                if (_connection != value)
                {
                    if (null != _connection)
                    {
                        Unprepare();
                    }
                    _connection = value;

                    _transaction = null;
                }
            }
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public virtual string CommandText
        {
            get
            {
                // If the user set the command tree previously, then we cannot retrieve the command text
                if (_commandTreeSetByUser != null)
                {
                    throw new InvalidOperationException(Strings.EntityClient_CannotGetCommandText);
                }

                return _esqlCommandText ?? "";
            }
            set
            {
                ThrowIfDataReaderIsOpen();

                // If the user set the command tree previously, then we cannot set the command text
                if (_commandTreeSetByUser != null)
                {
                    throw new InvalidOperationException(Strings.EntityClient_CannotSetCommandText);
                }

                if (_esqlCommandText != value)
                {
                    _esqlCommandText = value;

                    // Wipe out any preparation work we have done
                    Unprepare();

                    // If the user-defined command text or tree has been set (even to null or empty),
                    // then this command can no longer be considered command definition-based
                    _isCommandDefinitionBased = false;
                }
            }
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public virtual DbCommandTree CommandTree
        {
            get
            {
                // If the user set the command text previously, then we cannot retrieve the command tree
                if (!string.IsNullOrEmpty(_esqlCommandText))
                {
                    throw new InvalidOperationException(Strings.EntityClient_CannotGetCommandTree);
                }

                return _commandTreeSetByUser;
            }
            set
            {
                ThrowIfDataReaderIsOpen();

                // If the user set the command text previously, then we cannot set the command tree
                if (!string.IsNullOrEmpty(_esqlCommandText))
                {
                    throw new InvalidOperationException(Strings.EntityClient_CannotSetCommandTree);
                }

                // If the command type is not Text, CommandTree cannot be set
                if (CommandType.Text != CommandType)
                {
                    throw new InvalidOperationException(Strings.ADP_InternalProviderError((int)EntityUtil.InternalErrorCode.CommandTreeOnStoredProcedureEntityCommand));
                }

                if (_commandTreeSetByUser != value)
                {
                    _commandTreeSetByUser = value;

                    // Wipe out any preparation work we have done
                    Unprepare();

                    // If the user-defined command text or tree has been set (even to null or empty),
                    // then this command can no longer be considered command definition-based
                    _isCommandDefinitionBased = false;
                }
            }
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public virtual int CommandTimeout
        {
            get
            {
                // Returns the timeout value if it has been set
                if (_commandTimeout != null)
                {
                    return _commandTimeout.Value;
                }

                // Create a provider command object just so we can ask the default timeout
                if (_connection != null
                    && _connection.StoreProviderFactory != null)
                {
                    var storeCommand = _connection.StoreProviderFactory.CreateCommand();
                    if (storeCommand != null)
                    {
                        return storeCommand.CommandTimeout;
                    }
                }

                return 0;
            }
            set
            {
                ThrowIfDataReaderIsOpen();
                _commandTimeout = value;
            }
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public virtual CommandType CommandType
        {
            get { return _commandType; }
            set
            {
                ThrowIfDataReaderIsOpen();

                // For now, command type other than Text is not supported
                if (value != CommandType.Text
                    && value != CommandType.StoredProcedure)
                {
                    throw new NotSupportedException(Strings.EntityClient_UnsupportedCommandType);
                }

                _commandType = value;
            }
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public virtual EntityParameterCollection Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public virtual EntityTransaction Transaction
        {
            get 
            {
                // SQLBU 496829
                return _transaction; 
            }
            set
            {
                ThrowIfDataReaderIsOpen();
                _transaction = value;
            }
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public virtual UpdateRowSource UpdatedRowSource
        {
            get { return _updatedRowSource; }
            set
            {
                ThrowIfDataReaderIsOpen();
                _updatedRowSource = value;
            }
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public virtual bool DesignTimeVisible
        {
            get { return _designTimeVisible; }
            set
            {
                ThrowIfDataReaderIsOpen();
                _designTimeVisible = value;
                TypeDescriptor.Refresh(this.EntityCommandWrapper);
            }
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public virtual bool EnablePlanCaching
        {
            get { return _enableQueryPlanCaching; }
            set
            {
                ThrowIfDataReaderIsOpen();
                _enableQueryPlanCaching = value;
            }
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public virtual EntityDataReader ExecuteReader(CommandBehavior behavior)
        {
            Prepare(); // prepare the query first
            var reader = _entityDataReaderFactory.CreateEntityDataReader(
                this.EntityCommandWrapper, 
                _commandDefinition.Execute(this.EntityCommandWrapper, behavior),
                behavior);

            _dataReader = reader;
            return reader;
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public virtual Task<EntityDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public virtual int ExecuteNonQuery()
        {
            return ExecuteScalar(
                reader =>
                    {
                        // consume reader before checking records affected
                        CommandHelper.ConsumeReader(reader);
                        return reader.RecordsAffected;
                    });
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public virtual Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public virtual object ExecuteScalar()
        {
            return ExecuteScalar(
                reader =>
                    {
                        var result = reader.Read() ? reader.GetValue(0) : null;
                        // consume reader before retrieving parameters
                        CommandHelper.ConsumeReader(reader);
                        return result;
                    });
        }

        /// <summary>
        /// Executes a reader and retrieves a scalar value using the given resultSelector delegate
        /// </summary>
        private T_Result ExecuteScalar<T_Result>(Func<DbDataReader, T_Result> resultSelector)
        {
            T_Result result;
            using (var reader = ExecuteReader(CommandBehavior.SequentialAccess))
            {
                result = resultSelector(reader);
            }

            return result;
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        internal virtual void Unprepare()
        {
            _commandDefinition = null;
            _preparedCommandTree = null;

            // Clear the dirty flag on the parameters and parameter collection
            _parameters.ResetIsDirty();
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        public virtual void Prepare()
        {
            ThrowIfDataReaderIsOpen();
            CheckIfReadyToPrepare();

            InnerPrepare();
        }

        /// <summary>
        /// Creates a prepared version of this command without regard to the current connection state.
        /// Called by both <see cref="Prepare"/> and <see cref="ToTraceString"/>.
        /// </summary>
        private void InnerPrepare()
        {
            // Unprepare if the parameters have changed to force a reprepare
            if (_parameters.IsDirty)
            {
                Unprepare();
            }

            _commandDefinition = GetCommandDefinition();
            Debug.Assert(null != _commandDefinition, "_commandDefinition cannot be null");
        }

        /// <summary>
        /// Ensures we have the command tree, either the user passed us the tree, or an eSQL statement that we need to parse
        /// </summary>
        private void MakeCommandTree()
        {
            // We must have a connection before we come here
            Debug.Assert(_connection != null);

            // Do the work only if we don't have a command tree yet
            if (_preparedCommandTree == null)
            {
                DbCommandTree resultTree = null;
                if (_commandTreeSetByUser != null)
                {
                    resultTree = _commandTreeSetByUser;
                }
                else if (CommandType.Text == CommandType)
                {
                    if (!string.IsNullOrEmpty(_esqlCommandText))
                    {
                        // The perspective to be used for the query compilation
                        Perspective perspective = new ModelPerspective(_connection.GetMetadataWorkspace());

                        // get a dictionary of names and typeusage from entity parameter collection
                        var queryParams = GetParameterTypeUsage();

                        resultTree = CqlQuery.Compile(
                            _esqlCommandText,
                            perspective,
                            null /*parser option - use default*/,
                            queryParams.Select(paramInfo => paramInfo.Value.Parameter(paramInfo.Key))).CommandTree;
                    }
                    else
                    {
                        // We have no command text, no command tree, so throw an exception
                        if (_isCommandDefinitionBased)
                        {
                            // This command was based on a prepared command definition and has no command text,
                            // so reprepare is not possible. To create a new command with different parameters
                            // requires creating a new entity command definition and calling it's CreateCommand method.
                            throw new InvalidOperationException(Strings.EntityClient_CannotReprepareCommandDefinitionBasedCommand);
                        }
                        else
                        {
                            throw new InvalidOperationException(Strings.EntityClient_NoCommandText);
                        }
                    }
                }
                else if (CommandType.StoredProcedure == CommandType)
                {
                    // get a dictionary of names and typeusage from entity parameter collection
                    IEnumerable<KeyValuePair<string, TypeUsage>> queryParams = GetParameterTypeUsage();
                    var function = DetermineFunctionImport();
                    resultTree = new DbFunctionCommandTree(Connection.GetMetadataWorkspace(), DataSpace.CSpace, function, null, queryParams);
                }

                // After everything is good and succeeded, assign the result to our field
                _preparedCommandTree = resultTree;
            }
        }

        // requires: this must be a StoreProcedure command
        // effects: determines the EntityContainer function import referenced by this.CommandText
        private EdmFunction DetermineFunctionImport()
        {
            Debug.Assert(CommandType.StoredProcedure == CommandType);

            if (string.IsNullOrEmpty(CommandText) || string.IsNullOrEmpty(CommandText.Trim()))
            {
                throw new InvalidOperationException(Strings.EntityClient_FunctionImportEmptyCommandText);
            }

            var workspace = _connection.GetMetadataWorkspace();

            // parse the command text
            string containerName;
            string functionImportName;
            string defaultContainerName = null; // no default container in EntityCommand
            CommandHelper.ParseFunctionImportCommandText(CommandText, defaultContainerName, out containerName, out functionImportName);

            return CommandHelper.FindFunctionImport(_connection.GetMetadataWorkspace(), containerName, functionImportName);
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        internal virtual EntityCommandDefinition GetCommandDefinition()
        {
            var entityCommandDefinition = _commandDefinition;

            // Construct the command definition using no special options;
            if (null == entityCommandDefinition)
            {
                // check if the _commandDefinition is in cache
                if (!TryGetEntityCommandDefinitionFromQueryCache(out entityCommandDefinition))
                {
                    // if not, construct the command definition using no special options;
                    entityCommandDefinition = CreateCommandDefinition();
                }

                _commandDefinition = entityCommandDefinition;
            }

            return entityCommandDefinition;
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        [Browsable(false)]
        public virtual string ToTraceString()
        {
            CheckConnectionPresent();

            InnerPrepare();

            var commandDefinition = _commandDefinition;
            if (null != commandDefinition)
            {
                return commandDefinition.ToTraceString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets an entitycommanddefinition from cache if a match is found for the given cache key.
        /// </summary>
        /// <param name="entityCommandDefinition">out param. returns the entitycommanddefinition for a given cache key</param>
        /// <returns>true if a match is found in cache, false otherwise</returns>
        private bool TryGetEntityCommandDefinitionFromQueryCache(out EntityCommandDefinition entityCommandDefinition)
        {
            Debug.Assert(null != _connection, "Connection must not be null at this point");
            entityCommandDefinition = null;

            // if EnableQueryCaching is false, then just return to force the CommandDefinition to be created
            if (!_enableQueryPlanCaching || string.IsNullOrEmpty(_esqlCommandText))
            {
                return false;
            }

            // Create cache key
            var queryCacheKey = new EntityClientCacheKey(this.EntityCommandWrapper);

            // Try cache lookup
            var queryCacheManager = _connection.GetMetadataWorkspace().GetQueryCacheManager();
            Debug.Assert(null != queryCacheManager, "QuerycacheManager instance cannot be null");
            if (!queryCacheManager.TryCacheLookup(queryCacheKey, out entityCommandDefinition))
            {
                // if not, construct the command definition using no special options;
                entityCommandDefinition = CreateCommandDefinition();

                // add to the cache
                QueryCacheEntry outQueryCacheEntry = null;
                if (queryCacheManager.TryLookupAndAdd(new QueryCacheEntry(queryCacheKey, entityCommandDefinition), out outQueryCacheEntry))
                {
                    entityCommandDefinition = (EntityCommandDefinition)outQueryCacheEntry.GetTarget();
                }
            }

            Debug.Assert(null != entityCommandDefinition, "out entityCommandDefinition must not be null");

            return true;
        }

        /// <summary>
        /// Creates a commandDefinition for the command, using the options specified.  
        /// 
        /// Note: This method must not be side-effecting of the command
        /// </summary>
        /// <returns>the command definition</returns>
        private EntityCommandDefinition CreateCommandDefinition()
        {
            MakeCommandTree();

            // Always check the CQT metadata against the connection metadata (internally, CQT already
            // validates metadata consistency)
            if (!_preparedCommandTree.MetadataWorkspace.IsMetadataWorkspaceCSCompatible(Connection.GetMetadataWorkspace()))
            {
                throw new InvalidOperationException(Strings.EntityClient_CommandTreeMetadataIncompatible);
            }

            var result = EntityProviderServices.CreateCommandDefinition(_connection.StoreProviderFactory, _preparedCommandTree);
            return result;
        }

        private void CheckConnectionPresent()
        {
            if (_connection == null)
            {
                throw new InvalidOperationException(Strings.EntityClient_NoConnectionForCommand);
            }
        }

        /// <summary>
        /// Checking the integrity of this command object to see if it's ready to be prepared or executed
        /// </summary>
        private void CheckIfReadyToPrepare()
        {
            // Check that we have a connection
            CheckConnectionPresent();

            if (_connection.StoreProviderFactory == null || _connection.StoreConnection == null)
            {
                throw new InvalidOperationException(Strings.EntityClient_ConnectionStringNeededBeforeOperation);
            }

            // Make sure the connection is not closed or broken
            if (_connection.State == ConnectionState.Closed || _connection.State == ConnectionState.Broken)
            {
                var message = Strings.EntityClient_ExecutingOnClosedConnection(
                    _connection.State == ConnectionState.Closed
                        ? Strings.EntityClient_ConnectionStateClosed
                        : Strings.EntityClient_ConnectionStateBroken);
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Checking if the command is still tied to a data reader, if so, then the reader must still be open and we throw
        /// </summary>
        private void ThrowIfDataReaderIsOpen()
        {
            if (_dataReader != null)
            {
                throw new InvalidOperationException(Strings.EntityClient_DataReaderIsStillOpen);
            }
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        internal virtual Dictionary<string, TypeUsage> GetParameterTypeUsage()
        {
            Debug.Assert(null != _parameters, "_parameters must not be null");
            
            // Extract type metadata objects from the parameters to be used by CqlQuery.Compile
            var queryParams = new Dictionary<string, TypeUsage>(_parameters.Count);
            foreach (EntityParameter parameter in _parameters)
            {
                // Validate that the parameter name has the format: A character followed by alphanumerics or
                // underscores
                var parameterName = parameter.ParameterName;
                if (string.IsNullOrEmpty(parameterName))
                {
                    throw new InvalidOperationException(Strings.EntityClient_EmptyParameterName);
                }

                // Check each parameter to make sure it's an input parameter, currently EntityCommand doesn't support
                // anything else
                if (CommandType == CommandType.Text && parameter.Direction != ParameterDirection.Input)
                {
                    throw new InvalidOperationException(Strings.EntityClient_InvalidParameterDirection(parameter.ParameterName));
                }

                // Checking that we can deduce the type from the parameter if the type is not set
                if (parameter.EdmType == null && parameter.DbType == DbType.Object
                    && (parameter.Value == null || parameter.Value is DBNull))
                {
                    throw new InvalidOperationException(Strings.EntityClient_UnknownParameterType(parameterName));
                }

                // Validate that the parameter has an appropriate type and value
                // Any failures in GetTypeUsage will be surfaced as exceptions to the user
                TypeUsage typeUsage = null;
                typeUsage = parameter.GetTypeUsage();

                // Add the query parameter, add the same time detect if this parameter has the same name of a previous parameter
                try
                {
                    queryParams.Add(parameterName, typeUsage);
                }
                catch (ArgumentException e)
                {
                    throw new InvalidOperationException(Strings.EntityClient_DuplicateParameterNames(parameter.ParameterName), e);
                }
            }

            return queryParams;
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        internal virtual void NotifyDataReaderClosing()
        {
            // Disassociating the data reader with this command
            _dataReader = null;

            if (null != _storeProviderCommand)
            {
                CommandHelper.SetEntityParameterValues(this.EntityCommandWrapper, _storeProviderCommand, _connection);
                _storeProviderCommand = null;
            }
            if (this.EntityCommandWrapper.IsNotNullOnDataReaderClosingEvent())
            {
                this.EntityCommandWrapper.InvokeOnDataReaderClosingEvent(this.EntityCommandWrapper, new EventArgs());
            }
        }

        /// <summary>
        /// See comments on <see cref="EntityCommand"/> class.
        /// </summary>
        internal virtual void SetStoreProviderCommand(DbCommand storeProviderCommand)
        {
            _storeProviderCommand = storeProviderCommand;
        }

        /// <summary>
        /// Class for test purposes only, used to abstract the creation of <see cref="EntityDataReader"/> object.
        /// </summary>
        internal class EntityDataReaderFactory
        {
            internal virtual EntityDataReader CreateEntityDataReader(EntityCommand entityCommand, DbDataReader storeDataReader, CommandBehavior behavior)
            {
                return new EntityDataReader(entityCommand, storeDataReader, behavior);
            }
        }
    }
}
