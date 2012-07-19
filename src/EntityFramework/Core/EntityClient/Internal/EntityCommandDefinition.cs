namespace System.Data.Entity.Core.EntityClient.Internal
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Data.Entity.Core.Query.PlanCompiler;
    using System.Data.Entity.Core.Query.ResultAssembly;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class EntityCommandDefinition : DbCommandDefinition
    {
        #region internal state

        /// <summary>
        /// nested store command definitions
        /// </summary>
        private readonly List<DbCommandDefinition> _mappedCommandDefinitions;

        /// <summary>
        /// generates column map for the store result reader
        /// </summary>
        private readonly IColumnMapGenerator[] _columnMapGenerators;

        /// <summary>
        /// list of the parameters that the resulting command should have
        /// </summary>
        private readonly ReadOnlyCollection<EntityParameter> _parameters;

        /// <summary>
        /// Set of entity sets exposed in the command.
        /// </summary>
        private readonly Set<EntitySet> _entitySets;

        private readonly BridgeDataReaderFactory _bridgeDataReaderFactory;

        #endregion

        #region constructors

        /// <summary>
        /// Creates a new instance of <see cref="EntityCommandDefinition"/>.
        /// </summary>
        /// <exception cref="EntityCommandCompilationException">Cannot prepare the command definition for execution; consult the InnerException for more information.</exception>
        /// <exception cref="NotSupportedException">The ADO.NET Data Provider you are using does not support CommandTrees.</exception>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal EntityCommandDefinition(
            DbProviderFactory storeProviderFactory, DbCommandTree commandTree,
            BridgeDataReaderFactory bridgeDataReaderFactory = null)
        {
            Contract.Requires(storeProviderFactory != null);
            Contract.Requires(commandTree != null);

            _bridgeDataReaderFactory = bridgeDataReaderFactory ?? new BridgeDataReaderFactory();

            var storeProviderServices = storeProviderFactory.GetProviderServices();

            try
            {
                if (DbCommandTreeKind.Query
                    == commandTree.CommandTreeKind)
                {
                    // Next compile the plan for the command tree
                    var mappedCommandList = new List<ProviderCommandInfo>();
                    ColumnMap columnMap;
                    int columnCount;
                    PlanCompiler.Compile(commandTree, out mappedCommandList, out columnMap, out columnCount, out _entitySets);
                    _columnMapGenerators = new IColumnMapGenerator[] { new ConstantColumnMapGenerator(columnMap, columnCount) };
                    // Note: we presume that the first item in the ProviderCommandInfo is the root node;
                    Debug.Assert(mappedCommandList.Count > 0, "empty providerCommandInfo collection and no exception?");
                    // this shouldn't ever happen.

                    // Then, generate the store commands from the resulting command tree(s)
                    _mappedCommandDefinitions = new List<DbCommandDefinition>(mappedCommandList.Count);

                    foreach (var providerCommandInfo in mappedCommandList)
                    {
                        var providerCommandDefinition = storeProviderServices.CreateCommandDefinition(providerCommandInfo.CommandTree);

                        if (null == providerCommandDefinition)
                        {
                            throw new ProviderIncompatibleException(Strings.ProviderReturnedNullForCreateCommandDefinition);
                        }

                        _mappedCommandDefinitions.Add(providerCommandDefinition);
                    }
                }
                else
                {
                    Contract.Assert(
                        DbCommandTreeKind.Function == commandTree.CommandTreeKind, "only query and function command trees are supported");
                    var entityCommandTree = (DbFunctionCommandTree)commandTree;

                    // Retrieve mapping and metadata information for the function import.
                    var mapping = GetTargetFunctionMapping(entityCommandTree);
                    IList<FunctionParameter> returnParameters = entityCommandTree.EdmFunction.ReturnParameters;
                    var resultSetCount = returnParameters.Count > 1 ? returnParameters.Count : 1;
                    _columnMapGenerators = new IColumnMapGenerator[resultSetCount];
                    var storeResultType = DetermineStoreResultType(mapping, 0, out _columnMapGenerators[0]);
                    for (var i = 1; i < resultSetCount; i++)
                    {
                        DetermineStoreResultType(mapping, i, out _columnMapGenerators[i]);
                    }

                    // Copy over parameters (this happens through a more indirect route in the plan compiler, but
                    // it happens nonetheless)
                    var providerParameters = new List<KeyValuePair<string, TypeUsage>>();
                    foreach (var parameter in entityCommandTree.Parameters)
                    {
                        providerParameters.Add(parameter);
                    }

                    // Construct store command tree usage.
                    var providerCommandTree = new DbFunctionCommandTree(
                        entityCommandTree.MetadataWorkspace, DataSpace.SSpace,
                        mapping.TargetFunction, storeResultType, providerParameters);

                    var storeCommandDefinition = storeProviderServices.CreateCommandDefinition(providerCommandTree);
                    _mappedCommandDefinitions = new List<DbCommandDefinition>(1)
                        {
                            storeCommandDefinition
                        };

                    var firstResultEntitySet = mapping.FunctionImport.EntitySets.FirstOrDefault();
                    if (firstResultEntitySet != null)
                    {
                        _entitySets = new Set<EntitySet>();
                        _entitySets.Add(mapping.FunctionImport.EntitySets.FirstOrDefault());
                        _entitySets.MakeReadOnly();
                    }
                }

                // Finally, build a list of the parameters that the resulting command should have;
                var parameterList = new List<EntityParameter>();

                foreach (var queryParameter in commandTree.Parameters)
                {
                    var parameter = CreateEntityParameterFromQueryParameter(queryParameter);
                    parameterList.Add(parameter);
                }

                _parameters = new ReadOnlyCollection<EntityParameter>(parameterList);
            }
            catch (EntityCommandCompilationException)
            {
                // No need to re-wrap EntityCommandCompilationException
                throw;
            }
            catch (Exception e)
            {
                // we should not be wrapping all exceptions
                if (e.IsCatchableExceptionType())
                {
                    // we don't wan't folks to have to know all the various types of exceptions that can 
                    // occur, so we just rethrow a CommandDefinitionException and make whatever we caught  
                    // the inner exception of it.
                    throw new EntityCommandCompilationException(Strings.EntityClient_CommandDefinitionPreparationFailed, e);
                }

                throw;
            }
        }

        /// <summary>
        /// Constructor for testing/mocking purposes.
        /// </summary>
        protected EntityCommandDefinition(BridgeDataReaderFactory factory = null, List<DbCommandDefinition> mappedCommandDefinitions = null)
        {
            _bridgeDataReaderFactory = factory ?? new BridgeDataReaderFactory();
            _mappedCommandDefinitions = mappedCommandDefinitions;
        }

        /// <summary>
        /// Determines the store type for a function import.
        /// </summary>
        private static TypeUsage DetermineStoreResultType(
            FunctionImportMappingNonComposable mapping, int resultSetIndex, out IColumnMapGenerator columnMapGenerator)
        {
            // Determine column maps and infer result types for the mapped function. There are four varieties:
            // Collection(Entity)
            // Collection(PrimitiveType)
            // Collection(ComplexType)
            // No result type
            TypeUsage storeResultType;
            {
                StructuralType baseStructuralType;
                var functionImport = mapping.FunctionImport;

                // Collection(Entity) or Collection(ComplexType)
                if (MetadataHelper.TryGetFunctionImportReturnType(functionImport, resultSetIndex, out baseStructuralType))
                {
                    ValidateEdmResultType(baseStructuralType, functionImport);

                    //Note: Defensive check for historic reasons, we expect functionImport.EntitySets.Count > resultSetIndex 
                    var entitySet = functionImport.EntitySets.Count > resultSetIndex ? functionImport.EntitySets[resultSetIndex] : null;

                    columnMapGenerator = new FunctionColumnMapGenerator(mapping, resultSetIndex, entitySet, baseStructuralType);

                    // We don't actually know the return type for the stored procedure, but we can infer
                    // one based on the mapping (i.e.: a column for every property of the mapped types
                    // and for all discriminator columns)
                    storeResultType = mapping.GetExpectedTargetResultType(resultSetIndex);
                }

                    // Collection(PrimitiveType)
                else
                {
                    var returnParameter = MetadataHelper.GetReturnParameter(functionImport, resultSetIndex);
                    if (returnParameter != null
                        && returnParameter.TypeUsage != null)
                    {
                        // Get metadata description of the return type 
                        storeResultType = returnParameter.TypeUsage;
                        Debug.Assert(
                            storeResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType,
                            "FunctionImport currently supports only collection result type");
                        var elementType = ((CollectionType)storeResultType.EdmType).TypeUsage;
                        Debug.Assert(
                            Helper.IsScalarType(elementType.EdmType),
                            "FunctionImport supports only Collection(Entity), Collection(Enum) and Collection(Primitive)");

                        // Build collection column map where the first column of the store result is assumed
                        // to contain the primitive type values.
                        var scalarColumnMap = new ScalarColumnMap(elementType, string.Empty, 0, 0);
                        var collectionColumnMap = new SimpleCollectionColumnMap(
                            storeResultType,
                            string.Empty, scalarColumnMap, null, null);
                        columnMapGenerator = new ConstantColumnMapGenerator(collectionColumnMap, 1);
                    }

                        // No result type
                    else
                    {
                        storeResultType = null;
                        columnMapGenerator = new ConstantColumnMapGenerator(null, 0);
                    }
                }
            }
            return storeResultType;
        }

        /// <summary>
        /// Handles the following negative scenarios
        /// Nested ComplexType Property in ComplexType
        /// </summary>
        /// <param name="resultType"></param>
        private static void ValidateEdmResultType(EdmType resultType, EdmFunction functionImport)
        {
            if (Helper.IsComplexType(resultType))
            {
                var complexType = resultType as ComplexType;
                Debug.Assert(null != complexType, "we should have a complex type here");

                foreach (var property in complexType.Properties)
                {
                    if (property.TypeUsage.EdmType.BuiltInTypeKind
                        == BuiltInTypeKind.ComplexType)
                    {
                        throw new NotSupportedException(
                            Strings.ComplexTypeAsReturnTypeAndNestedComplexProperty(
                                property.Name, complexType.Name, functionImport.FullName));
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves mapping for the given C-Space functionCommandTree
        /// </summary>
        private static FunctionImportMappingNonComposable GetTargetFunctionMapping(DbFunctionCommandTree functionCommandTree)
        {
            Debug.Assert(functionCommandTree.DataSpace == DataSpace.CSpace, "map from CSpace->SSpace function");
            Debug.Assert(functionCommandTree != null, "null functionCommandTree");
            Debug.Assert(!functionCommandTree.EdmFunction.IsComposableAttribute, "functionCommandTree.EdmFunction must be non-composable.");

            // Find mapped store function.
            FunctionImportMapping targetFunctionMapping;
            if (
                !functionCommandTree.MetadataWorkspace.TryGetFunctionImportMapping(
                    functionCommandTree.EdmFunction, out targetFunctionMapping))
            {
                throw new InvalidOperationException(Strings.EntityClient_UnmappedFunctionImport(functionCommandTree.EdmFunction.FullName));
            }
            return (FunctionImportMappingNonComposable)targetFunctionMapping;
        }

        #endregion

        #region properties

        /// <summary>
        /// Property to expose the known parameters for the query, so the Command objects 
        /// constructor can poplulate it's parameter collection from.
        /// </summary>
        internal virtual IEnumerable<EntityParameter> Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// Set of entity sets exposed in the command.
        /// </summary>
        internal virtual Set<EntitySet> EntitySets
        {
            get { return _entitySets; }
        }

        /// <summary>
        /// Create a DbCommand object from the definition, that can be executed
        /// </summary>
        /// <returns></returns>
        public override DbCommand CreateCommand()
        {
            return new EntityCommand(this);
        }

        #endregion

        #region internal methods

        /// <summary>
        /// Creates ColumnMap for result assembly using the given reader.
        /// </summary>
        internal ColumnMap CreateColumnMap(DbDataReader storeDataReader)
        {
            return CreateColumnMap(storeDataReader, 0);
        }

        /// <summary>
        /// Creates ColumnMap for result assembly using the given reader's resultSetIndexth result set.
        /// </summary>
        internal virtual ColumnMap CreateColumnMap(DbDataReader storeDataReader, int resultSetIndex)
        {
            return _columnMapGenerators[resultSetIndex].CreateColumnMap(storeDataReader);
        }

        /// <summary>
        /// Constructs a EntityParameter from a CQT parameter.
        /// </summary>
        /// <param name="queryParameter"></param>
        /// <returns></returns>
        private static EntityParameter CreateEntityParameterFromQueryParameter(KeyValuePair<string, TypeUsage> queryParameter)
        {
            // We really can't have a parameter here that isn't a scalar type...
            Debug.Assert(TypeSemantics.IsScalarType(queryParameter.Value), "Non-scalar type used as query parameter type");

            var result = new EntityParameter();
            result.ParameterName = queryParameter.Key;

            PopulateParameterFromTypeUsage(result, queryParameter.Value, isOutParam: false);

            return result;
        }

        internal static void PopulateParameterFromTypeUsage(EntityParameter parameter, TypeUsage type, bool isOutParam)
        {
            // type can be null here if the type provided by the user is not a known model type
            if (type != null)
            {
                PrimitiveTypeKind primitiveTypeKind;

                if (Helper.IsEnumType(type.EdmType))
                {
                    type = TypeUsage.Create(Helper.GetUnderlyingEdmTypeForEnumType(type.EdmType));
                }
                else if (Helper.IsSpatialType(type, out primitiveTypeKind))
                {
                    parameter.EdmType = EdmProviderManifest.Instance.GetPrimitiveType(primitiveTypeKind);
                }
            }

            DbCommandDefinition.PopulateParameterFromTypeUsage(parameter, type, isOutParam);
        }

        /// <summary>
        /// Internal execute method -- copies command information from the map command 
        /// to the command objects, executes them, and builds the result assembly 
        /// structures needed to return the data reader
        /// </summary>
        /// <exception cref="InvalidOperationException">behavior must specify CommandBehavior.SequentialAccess</exception>
        /// <exception cref="InvalidOperationException">input parameters in the entityCommand.Parameters collection must have non-null values.</exception>
        internal virtual DbDataReader Execute(EntityCommand entityCommand, CommandBehavior behavior)
        {
            if (CommandBehavior.SequentialAccess
                != (behavior & CommandBehavior.SequentialAccess))
            {
                throw new InvalidOperationException(Strings.ADP_MustUseSequentialAccess);
            }

            var storeDataReader = ExecuteStoreCommands(entityCommand, behavior);
            DbDataReader result = null;

            // If we actually executed something, then go ahead and construct a bridge
            // data reader for it.
            if (null != storeDataReader)
            {
                try
                {
                    var columnMap = CreateColumnMap(storeDataReader, 0);
                    if (null == columnMap)
                    {
                        // For a query with no result type (and therefore no column map), consume the reader.
                        // When the user requests Metadata for this reader, we return nothing.
                        CommandHelper.ConsumeReader(storeDataReader);
                        result = storeDataReader;
                    }
                    else
                    {
                        var metadataWorkspace = entityCommand.Connection.GetMetadataWorkspace();
                        var nextResultColumnMaps = GetNextResultColumnMaps(storeDataReader);
                        result = _bridgeDataReaderFactory.Create(
                            storeDataReader, columnMap, metadataWorkspace, nextResultColumnMaps);
                    }
                }
                catch
                {
                    // dispose of store reader if there is an error creating the BridgeDataReader
                    storeDataReader.Dispose();
                    throw;
                }
            }

            return result;
        }

        /// <summary>
        /// Internal execute method -- Asynchronously copies command information from the map command 
        /// to the command objects, executes them, and builds the result assembly 
        /// structures needed to return the data reader
        /// </summary>
        /// <exception cref="InvalidOperationException">behavior must specify CommandBehavior.SequentialAccess</exception>
        /// <exception cref="InvalidOperationException">input parameters in the entityCommand.Parameters collection must have non-null values.</exception>
        internal virtual async Task<DbDataReader> ExecuteAsync(
            EntityCommand entityCommand, CommandBehavior behavior, CancellationToken cancellationToken)
        {
            if (CommandBehavior.SequentialAccess
                != (behavior & CommandBehavior.SequentialAccess))
            {
                throw new InvalidOperationException(Strings.ADP_MustUseSequentialAccess);
            }

            var storeDataReader = await ExecuteStoreCommandsAsync(entityCommand, behavior, cancellationToken);
            DbDataReader result = null;

            // If we actually executed something, then go ahead and construct a bridge
            // data reader for it.
            if (null != storeDataReader)
            {
                try
                {
                    var columnMap = CreateColumnMap(storeDataReader, 0);
                    if (null == columnMap)
                    {
                        // For a query with no result type (and therefore no column map), consume the reader.
                        // When the user requests Metadata for this reader, we return nothing.
                        await CommandHelper.ConsumeReaderAsync(storeDataReader, cancellationToken);
                        result = storeDataReader;
                    }
                    else
                    {
                        var metadataWorkspace = entityCommand.Connection.GetMetadataWorkspace();
                        var nextResultColumnMaps = GetNextResultColumnMaps(storeDataReader);
                        result = _bridgeDataReaderFactory.Create(
                            storeDataReader, columnMap, metadataWorkspace, nextResultColumnMaps);
                    }
                }
                catch
                {
                    // dispose of store reader if there is an error creating the BridgeDataReader
                    storeDataReader.Dispose();
                    throw;
                }
            }

            return result;
        }

        private IEnumerable<ColumnMap> GetNextResultColumnMaps(DbDataReader storeDataReader)
        {
            for (var i = 1; i < _columnMapGenerators.Length; ++i)
            {
                yield return CreateColumnMap(storeDataReader, i);
            }
        }

        /// <summary>
        /// Execute the store commands, and return IteratorSources for each one
        /// </summary>
        internal virtual DbDataReader ExecuteStoreCommands(EntityCommand entityCommand, CommandBehavior behavior)
        {
            var storeProviderCommand = PrepareEntityCommandBeforeExecution(entityCommand);

            DbDataReader reader = null;
            try
            {
                reader = storeProviderCommand.ExecuteReader(behavior & ~CommandBehavior.SequentialAccess);
            }
            catch (Exception e)
            {
                // we should not be wrapping all exceptions
                if (e.IsCatchableExceptionType())
                {
                    // we don't wan't folks to have to know all the various types of exceptions that can 
                    // occur, so we just rethrow a CommandDefinitionException and make whatever we caught  
                    // the inner exception of it.
                    throw new EntityCommandExecutionException(Strings.EntityClient_CommandDefinitionExecutionFailed, e);
                }

                throw;
            }

            return reader;
        }

        /// <summary>
        /// Execute the store commands, and return IteratorSources for each one
        /// </summary>
        internal virtual async Task<DbDataReader> ExecuteStoreCommandsAsync(
            EntityCommand entityCommand, CommandBehavior behavior, CancellationToken cancellationToken)
        {
            var storeProviderCommand = PrepareEntityCommandBeforeExecution(entityCommand);

            DbDataReader reader = null;
            try
            {
                reader = await storeProviderCommand.ExecuteReaderAsync(behavior & ~CommandBehavior.SequentialAccess, cancellationToken);
            }
            catch (Exception e)
            {
                // we should not be wrapping all exceptions
                if (e.IsCatchableExceptionType())
                {
                    // we don't wan't folks to have to know all the various types of exceptions that can 
                    // occur, so we just rethrow a CommandDefinitionException and make whatever we caught  
                    // the inner exception of it.
                    throw new EntityCommandExecutionException(Strings.EntityClient_CommandDefinitionExecutionFailed, e);
                }

                throw;
            }

            return reader;
        }

        private DbCommand PrepareEntityCommandBeforeExecution(EntityCommand entityCommand)
        {
            if (1 != _mappedCommandDefinitions.Count)
            {
                throw new NotSupportedException("MARS");
            }

            var entityTransaction = entityCommand.ValidateAndGetEntityTransaction();
            var definition = _mappedCommandDefinitions[0];
            var storeProviderCommand = definition.CreateCommand();

            CommandHelper.SetStoreProviderCommandState(entityCommand, entityTransaction, storeProviderCommand);

            // Copy over the values from the map command to the store command; we 
            // assume that they were not renamed by either the plan compiler or SQL 
            // Generation.
            //
            // Note that this pretty much presumes that named parameters are supported
            // by the store provider, but it might work if we don't reorder/reuse
            // parameters.
            //
            // Note also that the store provider may choose to add parameters to thier
            // command object for some things; we'll only copy over the values for
            // parameters that we find in the EntityCommands parameters collection, so 
            // we won't damage anything the store provider did.

            var hasOutputParameters = false;
            if (storeProviderCommand.Parameters != null) // SQLBUDT 519066
            {
                var storeProviderServices = entityCommand.Connection.StoreProviderFactory.GetProviderServices();

                foreach (DbParameter storeParameter in storeProviderCommand.Parameters)
                {
                    // I could just use the string indexer, but then if I didn't find it the
                    // consumer would get some ParameterNotFound exeception message and that
                    // wouldn't be very meaningful.  Instead, I use the IndexOf method and
                    // if I don't find it, it's not a big deal (The store provider must
                    // have added it).
                    var parameterOrdinal = entityCommand.Parameters.IndexOf(storeParameter.ParameterName);
                    if (-1 != parameterOrdinal)
                    {
                        var entityParameter = entityCommand.Parameters[parameterOrdinal];

                        SyncParameterProperties(entityParameter, storeParameter, storeProviderServices);

                        if (storeParameter.Direction
                            != ParameterDirection.Input)
                        {
                            hasOutputParameters = true;
                        }
                    }
                }
            }

            // If the EntityCommand has output parameters, we must synchronize parameter values when
            // the reader is closed. Tell the EntityCommand about the store command so that it knows
            // where to pull those values from.
            if (hasOutputParameters)
            {
                entityCommand.SetStoreProviderCommand(storeProviderCommand);
            }

            return storeProviderCommand;
        }

        /// <summary>
        /// Updates storeParameter size, precision and scale properties from user provided parameter properties.
        /// </summary>
        /// <param name="entityParameter"></param>
        /// <param name="storeParameter"></param>
        private static void SyncParameterProperties(
            EntityParameter entityParameter, DbParameter storeParameter, DbProviderServices storeProviderServices)
        {
            IDbDataParameter dbDataParameter = storeParameter;

            // DBType is not currently syncable; it's part of the cache key anyway; this is because we can't guarantee
            // that the store provider will honor it -- (SqlClient doesn't...)
            //if (entityParameter.IsDbTypeSpecified)
            //{
            //    storeParameter.DbType = entityParameter.DbType;
            //}

            // Give the store provider the opportunity to set the value before any parameter state has been copied from
            // the EntityParameter.
            var parameterTypeUsage = TypeHelpers.GetPrimitiveTypeUsageForScalar(entityParameter.GetTypeUsage());
            storeProviderServices.SetParameterValue(storeParameter, parameterTypeUsage, entityParameter.Value);

            // Override the store provider parameter state with any explicitly specified values from the EntityParameter.
            if (entityParameter.IsDirectionSpecified)
            {
                storeParameter.Direction = entityParameter.Direction;
            }

            if (entityParameter.IsIsNullableSpecified)
            {
                storeParameter.IsNullable = entityParameter.IsNullable;
            }

            if (entityParameter.IsSizeSpecified)
            {
                storeParameter.Size = entityParameter.Size;
            }

            if (entityParameter.IsPrecisionSpecified)
            {
                dbDataParameter.Precision = entityParameter.Precision;
            }

            if (entityParameter.IsScaleSpecified)
            {
                dbDataParameter.Scale = entityParameter.Scale;
            }
        }

        /// <summary>
        /// Return the string used by EntityCommand and ObjectQuery<T> ToTraceString"/>
        /// </summary>
        /// <returns></returns>
        internal virtual string ToTraceString()
        {
            if (_mappedCommandDefinitions != null)
            {
                if (_mappedCommandDefinitions.Count == 1)
                {
                    // Gosh it sure would be nice if I could just get the inner commandText, but
                    // that would require more public surface area on DbCommandDefinition, or
                    // me to know about the inner object...
                    return _mappedCommandDefinitions[0].CreateCommand().CommandText;
                }
                else
                {
                    var sb = new StringBuilder();
                    foreach (var commandDefinition in _mappedCommandDefinitions)
                    {
                        var mappedCommand = commandDefinition.CreateCommand();
                        sb.Append(mappedCommand.CommandText);
                    }

                    return sb.ToString();
                }
            }

            return string.Empty;
        }

        #endregion

        #region nested types

        /// <summary>
        /// Generates a column map given a data reader.
        /// </summary>
        private interface IColumnMapGenerator
        {
            /// <summary>
            /// Given a data reader, returns column map.
            /// </summary>
            /// <param name="reader">Data reader.</param>
            /// <returns>Column map.</returns>
            ColumnMap CreateColumnMap(DbDataReader reader);
        }

        /// <summary>
        /// IColumnMapGenerator wrapping a constant instance of a column map (invariant with respect
        /// to the given DbDataReader)
        /// </summary>
        private sealed class ConstantColumnMapGenerator : IColumnMapGenerator
        {
            private readonly ColumnMap _columnMap;
            private readonly int _fieldsRequired;

            internal ConstantColumnMapGenerator(ColumnMap columnMap, int fieldsRequired)
            {
                _columnMap = columnMap;
                _fieldsRequired = fieldsRequired;
            }

            ColumnMap IColumnMapGenerator.CreateColumnMap(DbDataReader reader)
            {
                if (null != reader
                    && reader.FieldCount < _fieldsRequired)
                {
                    throw new EntityCommandExecutionException(Strings.EntityClient_TooFewColumns);
                }

                return _columnMap;
            }
        }

        /// <summary>
        /// Generates column maps for a non-composable function mapping.
        /// </summary>
        private sealed class FunctionColumnMapGenerator : IColumnMapGenerator
        {
            private readonly FunctionImportMappingNonComposable _mapping;
            private readonly EntitySet _entitySet;
            private readonly StructuralType _baseStructuralType;
            private readonly int _resultSetIndex;

            internal FunctionColumnMapGenerator(
                FunctionImportMappingNonComposable mapping, int resultSetIndex, EntitySet entitySet, StructuralType baseStructuralType)
            {
                _mapping = mapping;
                _entitySet = entitySet;
                _baseStructuralType = baseStructuralType;
                _resultSetIndex = resultSetIndex;
            }

            ColumnMap IColumnMapGenerator.CreateColumnMap(DbDataReader reader)
            {
                return ColumnMapFactory.CreateFunctionImportStructuralTypeColumnMap(
                    reader, _mapping, _resultSetIndex, _entitySet, _baseStructuralType);
            }
        }

        #endregion
    }
}
