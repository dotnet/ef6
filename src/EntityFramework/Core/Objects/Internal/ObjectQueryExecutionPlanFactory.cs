// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    internal class ObjectQueryExecutionPlanFactory
    {
        private readonly Translator _translator;

        public ObjectQueryExecutionPlanFactory(Translator translator = null)
        {
            _translator = translator ?? new Translator();
        }

        public virtual ObjectQueryExecutionPlan Prepare(
            ObjectContext context, DbQueryCommandTree tree, Type elementType, MergeOption mergeOption, bool streaming, Span span,
            IEnumerable<Tuple<ObjectParameter, QueryParameterExpression>> compiledQueryParameters, AliasGenerator aliasGenerator)
        {
            var treeResultType = tree.Query.ResultType;

            // Rewrite this tree for Span?
            DbExpression spannedQuery;
            SpanIndex spanInfo;
            if (ObjectSpanRewriter.TryRewrite(tree, span, mergeOption, aliasGenerator, out spannedQuery, out spanInfo))
            {
                tree = DbQueryCommandTree.FromValidExpression(
                    tree.MetadataWorkspace, tree.DataSpace, spannedQuery, tree.UseDatabaseNullSemantics, tree.DisableFilterOverProjectionSimplificationForCustomFunctions);
            }
            else
            {
                spanInfo = null;
            }

            var entityDefinition = CreateCommandDefinition(context, tree);

            var shaperFactory = Translator.TranslateColumnMap(
                _translator, elementType, entityDefinition.CreateColumnMap(null),
                context.MetadataWorkspace, spanInfo, mergeOption, streaming, false);

            // attempt to determine entity information for this query (e.g. which entity type and which entity set)

            EntitySet singleEntitySet = null;

            // determine if the entity set is unambiguous given the entity type
            if (treeResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType
                && entityDefinition.EntitySets != null)
            {
                foreach (var entitySet in entityDefinition.EntitySets)
                {
                    if (entitySet != null
                        && entitySet.ElementType.IsAssignableFrom(((CollectionType)treeResultType.EdmType).TypeUsage.EdmType))
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

            return new ObjectQueryExecutionPlan(
                entityDefinition, shaperFactory, treeResultType, mergeOption, streaming, singleEntitySet, compiledQueryParameters);
        }

        private static EntityCommandDefinition CreateCommandDefinition(ObjectContext context, DbQueryCommandTree tree)
        {
            var connection = context.Connection;

            // The connection is required to get to the CommandDefinition builder.
            if (connection == null)
            {
                throw new InvalidOperationException(Strings.ObjectQuery_InvalidConnection);
            }

            var services = DbProviderServices.GetProviderServices(connection);

            DbCommandDefinition definition;
            try
            {
                definition = services.CreateCommandDefinition(tree, context.InterceptionContext);
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
                    // we don't want folks to have to know all the various types of exceptions that can 
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

            return (EntityCommandDefinition)definition;
        }
    }
}
