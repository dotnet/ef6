namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using CompiledQueryParameters =
        System.Collections.ObjectModel.ReadOnlyCollection<Collections.Generic.KeyValuePair<ObjectParameter, ELinq.QueryParameterExpression>>
        ;

    /// <summary>
    /// Represents the 'compiled' form of all elements (query + result assembly) required to execute a specific <see cref="ObjectQuery"/>
    /// </summary>
    internal class ObjectQueryExecutionPlan
    {
        internal readonly DbCommandDefinition CommandDefinition;
        internal readonly ShaperFactory ResultShaperFactory;
        internal readonly TypeUsage ResultType;
        internal readonly MergeOption MergeOption;
        internal readonly CompiledQueryParameters CompiledQueryParameters;

        /// <summary>If the query yields entities from a single entity set, the value is stored here.</summary>
        private readonly EntitySet _singleEntitySet;

        /// <summary>
        /// For testing purposes only. For anything else call <see cref="Prepare"/>.
        /// </summary>
        protected ObjectQueryExecutionPlan(
            DbCommandDefinition commandDefinition, ShaperFactory resultShaperFactory, TypeUsage resultType, MergeOption mergeOption,
            EntitySet singleEntitySet, CompiledQueryParameters compiledQueryParameters)
        {
            CommandDefinition = commandDefinition;
            ResultShaperFactory = resultShaperFactory;
            ResultType = resultType;
            MergeOption = mergeOption;
            _singleEntitySet = singleEntitySet;
            CompiledQueryParameters = compiledQueryParameters;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal static ObjectQueryExecutionPlan Prepare(
            ObjectContext context, DbQueryCommandTree tree, Type elementType, MergeOption mergeOption, Span span,
            CompiledQueryParameters compiledQueryParameters, AliasGenerator aliasGenerator)
        {
            var treeResultType = tree.Query.ResultType;

            // Rewrite this tree for Span?
            DbExpression spannedQuery = null;
            SpanIndex spanInfo;
            if (ObjectSpanRewriter.TryRewrite(tree, span, mergeOption, aliasGenerator, out spannedQuery, out spanInfo))
            {
                tree = DbQueryCommandTree.FromValidExpression(tree.MetadataWorkspace, tree.DataSpace, spannedQuery);
            }
            else
            {
                spanInfo = null;
            }

            var connection = context.Connection;
            DbCommandDefinition definition = null;

            // The connection is required to get to the CommandDefinition builder.
            if (connection == null)
            {
                throw new InvalidOperationException(Strings.ObjectQuery_InvalidConnection);
            }

            var services = DbProviderServices.GetProviderServices(connection);

            try
            {
                definition = services.CreateCommandDefinition(tree);
            }
            catch (EntityCommandCompilationException)
            {
                // If we're running against EntityCommand, we probably already caught the providers'
                // exception and wrapped it, we don't want to do that again, so we'll just rethrow
                // here instead.
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

            if (definition == null)
            {
                throw new NotSupportedException(Strings.ADP_ProviderDoesNotSupportCommandTrees);
            }

            var entityDefinition = (EntityCommandDefinition)definition;
            var cacheManager = context.Perspective.MetadataWorkspace.GetQueryCacheManager();

            var shaperFactory = ShaperFactory.Create(
                elementType, cacheManager, entityDefinition.CreateColumnMap(null),
                context.MetadataWorkspace, spanInfo, mergeOption, false);

            // attempt to determine entity information for this query (e.g. which entity type and which entity set)

            EntitySet singleEntitySet = null;

            if (treeResultType.EdmType.BuiltInTypeKind
                == BuiltInTypeKind.CollectionType)
            {
                // determine if the entity set is unambiguous given the entity type
                if (null != entityDefinition.EntitySets)
                {
                    foreach (var entitySet in entityDefinition.EntitySets)
                    {
                        if (null != entitySet)
                        {
                            if (entitySet.ElementType.IsAssignableFrom(((CollectionType)treeResultType.EdmType).TypeUsage.EdmType))
                            {
                                if (singleEntitySet == null)
                                {
                                    // found a single match
                                    singleEntitySet = entitySet;
                                }
                                else
                                {
                                    // there's more than one matching entity set
                                    singleEntitySet = null;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return new ObjectQueryExecutionPlan(
                definition, shaperFactory, treeResultType, mergeOption, singleEntitySet, compiledQueryParameters);
        }

        internal string ToTraceString()
        {
            var traceString = string.Empty;
            var entityCommandDef = CommandDefinition as EntityCommandDefinition;
            if (entityCommandDef != null)
            {
                traceString = entityCommandDef.ToTraceString();
            }
            return traceString;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal virtual ObjectResult<TResultType> Execute<TResultType>(ObjectContext context, ObjectParameterCollection parameterValues)
        {
            DbDataReader storeReader = null;
            try
            {
                // create entity command (just do this to snarf store command)
                var commandDefinition = (EntityCommandDefinition)CommandDefinition;
                var entityCommand = new EntityCommand((EntityConnection)context.Connection, commandDefinition);

                // pass through parameters and timeout values
                if (context.CommandTimeout.HasValue)
                {
                    entityCommand.CommandTimeout = context.CommandTimeout.Value;
                }

                if (parameterValues != null)
                {
                    foreach (var parameter in parameterValues)
                    {
                        var index = entityCommand.Parameters.IndexOf(parameter.Name);

                        if (index != -1)
                        {
                            entityCommand.Parameters[index].Value = parameter.Value ?? DBNull.Value;
                        }
                    }
                }

                // acquire store reader
                storeReader = commandDefinition.ExecuteStoreCommands(entityCommand, CommandBehavior.Default);

                var shaperFactory = (ShaperFactory<TResultType>)ResultShaperFactory;
                var shaper = shaperFactory.Create(storeReader, context, context.MetadataWorkspace, MergeOption, true);

                // create materializer delegate
                TypeUsage resultItemEdmType;

                if (ResultType.EdmType.BuiltInTypeKind
                    == BuiltInTypeKind.CollectionType)
                {
                    resultItemEdmType = ((CollectionType)ResultType.EdmType).TypeUsage;
                }
                else
                {
                    resultItemEdmType = ResultType;
                }

                return new ObjectResult<TResultType>(shaper, _singleEntitySet, resultItemEdmType);
            }
            catch (Exception)
            {
                if (null != storeReader)
                {
                    // Note: The caller is responsible for disposing reader if creating
                    // the enumerator fails.
                    storeReader.Dispose();
                }
                throw;
            }
        }

        internal static ObjectResult<TResultType> ExecuteCommandTree<TResultType>(
            ObjectContext context, DbQueryCommandTree query, MergeOption mergeOption)
        {
            Debug.Assert(context != null, "ObjectContext cannot be null");
            Debug.Assert(query != null, "Command tree cannot be null");

            var execPlan = Prepare(context, query, typeof(TResultType), mergeOption, null, null, DbExpressionBuilder.AliasGenerator);
            return execPlan.Execute<TResultType>(context, null);
        }
    }
}
