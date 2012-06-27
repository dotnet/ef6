namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    internal class ObjectQueryExecutionPlanFactory
    {
        private readonly Translator _translator;

        public ObjectQueryExecutionPlanFactory(Translator translator = null)
        {
            _translator = translator ?? new Translator();
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public virtual ObjectQueryExecutionPlan Prepare(
            ObjectContext context, DbQueryCommandTree tree, Type elementType, MergeOption mergeOption, Span span,
            IEnumerable<Tuple<ObjectParameter, QueryParameterExpression>> compiledQueryParameters, AliasGenerator aliasGenerator)
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

            var shaperFactory = Translator.TranslateColumnMap(
                _translator,
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

        public ObjectResult<TResultType> ExecuteCommandTree<TResultType>(
            ObjectContext context, DbQueryCommandTree query, MergeOption mergeOption)
        {
            Contract.Requires(context != null);
            Contract.Requires(query != null);

            var execPlan = Prepare(context, query, typeof(TResultType), mergeOption, null, null, DbExpressionBuilder.AliasGenerator);
            return execPlan.Execute<TResultType>(context, null);
        }
    }
}
