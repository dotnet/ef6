// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.EntitySql;
    using System.Data.Entity.Core.Common.QueryCache;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;

    // <summary>
    // ObjectQueryState based on Entity-SQL query text.
    // </summary>
    internal sealed class EntitySqlQueryState : ObjectQueryState
    {
        // <summary>
        // The Entity-SQL text that defines the query.
        // </summary>
        // <remarks>
        // It is important that this field is readonly for consistency reasons wrt <see cref="_queryExpression" />.
        // If this field becomes read-write, then write should be allowed only when <see cref="_queryExpression" /> is null,
        // or there should be a mechanism keeping both fields consistent.
        // </remarks>
        private readonly string _queryText;

        // <summary>
        // Optional <see cref="DbExpression" /> that defines the query. Must be semantically equal to the
        // <see
        //     cref="_queryText" />
        // .
        // </summary>
        // <remarks>
        // It is important that this field is readonly for consistency reasons wrt <see cref="_queryText" />.
        // If this field becomes read-write, then there should be a mechanism keeping both fields consistent.
        // </remarks>
        private readonly DbExpression _queryExpression;

        // <summary>
        // Can a Limit subclause be appended to the text of this query?
        // </summary>
        private readonly bool _allowsLimit;

        private readonly ObjectQueryExecutionPlanFactory _objectQueryExecutionPlanFactory;

        // <summary>
        // Initializes a new query EntitySqlQueryState instance.
        // </summary>
        // <param name="commandText"> The Entity-SQL text of the query </param>
        // <param name="context"> The ObjectContext containing the metadata workspace the query was built against, the connection on which to execute the query, and the cache to store the results in. Must not be null. </param>
        internal EntitySqlQueryState(
            Type elementType, string commandText, bool allowsLimit, ObjectContext context, ObjectParameterCollection parameters, Span span)
            : this(elementType, commandText, /*expression*/ null, allowsLimit, context, parameters, span)
        {
        }

        // <summary>
        // Initializes a new query EntitySqlQueryState instance.
        // </summary>
        // <param name="commandText"> The Entity-SQL text of the query </param>
        // <param name="expression">
        // Optional <see cref="DbExpression" /> that defines the query. Must be semantically equal to the
        // <paramref name="commandText" />.
        // </param>
        // <param name="context"> The ObjectContext containing the metadata workspace the query was built against, the connection on which to execute the query, and the cache to store the results in. Must not be null. </param>
        internal EntitySqlQueryState(
            Type elementType, string commandText, DbExpression expression, bool allowsLimit, ObjectContext context,
            ObjectParameterCollection parameters, Span span,
            ObjectQueryExecutionPlanFactory objectQueryExecutionPlanFactory = null)
            : base(elementType, context, parameters, span)
        {
            Check.NotEmpty(commandText, "commandText");

            _queryText = commandText;
            _queryExpression = expression;
            _allowsLimit = allowsLimit;
            _objectQueryExecutionPlanFactory = objectQueryExecutionPlanFactory ?? new ObjectQueryExecutionPlanFactory();
        }

        // <summary>
        // Determines whether or not the current query is a 'Skip' or 'Sort' operation
        // and so would allow a 'Limit' clause to be appended to the current query text.
        // </summary>
        // <returns>
        // <c>True</c> if the current query is a Skip or Sort expression, or a Project expression with a Skip or Sort expression input.
        // </returns>
        internal bool AllowsLimitSubclause
        {
            get { return _allowsLimit; }
        }

        // <summary>
        // Always returns the Entity-SQL text of the implemented ObjectQuery.
        // </summary>
        // <param name="commandText"> Always set to the Entity-SQL text of this ObjectQuery. </param>
        // <returns>
        // Always returns <c>true</c> .
        // </returns>
        internal override bool TryGetCommandText(out string commandText)
        {
            commandText = _queryText;
            return true;
        }

        internal override bool TryGetExpression(out Expression expression)
        {
            expression = null;
            return false;
        }

        protected override TypeUsage GetResultType()
        {
            var query = Parse();
            return query.ResultType;
        }

        internal override ObjectQueryState Include<TElementType>(ObjectQuery<TElementType> sourceQuery, string includePath)
        {
            ObjectQueryState retState = new EntitySqlQueryState(
                ElementType, _queryText, _queryExpression, _allowsLimit, ObjectContext, ObjectParameterCollection.DeepCopy(Parameters),
                Span.IncludeIn(Span, includePath));
            ApplySettingsTo(retState);
            return retState;
        }

        internal override ObjectQueryExecutionPlan GetExecutionPlan(MergeOption? forMergeOption)
        {
            // Determine the required merge option, with the following precedence:
            // 1. The merge option specified to Execute(MergeOption) as forMergeOption.
            // 2. The merge option set via ObjectQuery.MergeOption.
            // 3. The global default merge option.
            var mergeOption = EnsureMergeOption(forMergeOption, UserSpecifiedMergeOption);

            // If a cached plan is present, then it can be reused if it has the required merge option
            // (since span and parameters cannot change between executions). However, if the cached
            // plan does not have the required merge option we proceed as if it were not present.
            var plan = _cachedPlan;
            if (plan != null)
            {
                if (plan.MergeOption == mergeOption)
                {
                    return plan;
                }
                else
                {
                    plan = null;
                }
            }

            // There is no cached plan (or it was cleared), so the execution plan must be retrieved from
            // the global query cache (if plan caching is enabled) or rebuilt for the required merge option.
            QueryCacheManager cacheManager = null;
            EntitySqlQueryCacheKey cacheKey = null;
            if (PlanCachingEnabled)
            {
                // Create a new cache key that reflects the current state of the Parameters collection
                // and the Span object (if any), and uses the specified merge option.
                cacheKey = new EntitySqlQueryCacheKey(
                    ObjectContext.DefaultContainerName,
                    _queryText,
                    (null == Parameters ? 0 : Parameters.Count),
                    (null == Parameters ? null : Parameters.GetCacheKey()),
                    (null == Span ? null : Span.GetCacheKey()),
                    mergeOption,
                    EffectiveStreamingBehaviour,
                    ElementType);

                cacheManager = ObjectContext.MetadataWorkspace.GetQueryCacheManager();
                ObjectQueryExecutionPlan executionPlan = null;
                if (cacheManager.TryCacheLookup(cacheKey, out executionPlan))
                {
                    plan = executionPlan;
                }
            }

            if (plan == null)
            {
                // Either caching is not enabled or the execution plan was not found in the cache
                var queryExpression = Parse();
                Debug.Assert(queryExpression != null, "EntitySqlQueryState.Parse returned null expression?");
                var tree = DbQueryCommandTree.FromValidExpression(ObjectContext.MetadataWorkspace, DataSpace.CSpace, queryExpression);
                plan = _objectQueryExecutionPlanFactory.Prepare(
                    ObjectContext, tree, ElementType, mergeOption, EffectiveStreamingBehaviour, Span, null,
                    DbExpressionBuilder.AliasGenerator);

                // If caching is enabled then update the cache now.
                // Note: the logic is the same as in ELinqQueryState.
                if (cacheKey != null)
                {
                    var newEntry = new QueryCacheEntry(cacheKey, plan);
                    QueryCacheEntry foundEntry = null;
                    if (cacheManager.TryLookupAndAdd(newEntry, out foundEntry))
                    {
                        // If TryLookupAndAdd returns 'true' then the entry was already present in the cache when the attempt to add was made.
                        // In this case the existing execution plan should be used.
                        plan = (ObjectQueryExecutionPlan)foundEntry.GetTarget();
                    }
                }
            }

            if (Parameters != null)
            {
                Parameters.SetReadOnly(true);
            }

            // Update the cached plan with the newly retrieved/prepared plan
            _cachedPlan = plan;

            // Return the execution plan
            return plan;
        }

        internal DbExpression Parse()
        {
            if (_queryExpression != null)
            {
                return _queryExpression;
            }

            List<DbParameterReferenceExpression> parameters = null;
            if (Parameters != null)
            {
                parameters = new List<DbParameterReferenceExpression>(Parameters.Count);
                foreach (var parameter in Parameters)
                {
                    var typeUsage = parameter.TypeUsage;
                    if (null == typeUsage)
                    {
                        // Since ObjectParameters do not allow users to specify 'facets', make 
                        // sure that the parameter TypeUsage is not populated with the provider
                        // default facet values.
                        ObjectContext.Perspective.TryGetTypeByName(
                            parameter.MappableType.FullNameWithNesting(),
                            false /* bIgnoreCase */,
                            out typeUsage);
                    }

                    Debug.Assert(typeUsage != null, "typeUsage != null");

                    parameters.Add(typeUsage.Parameter(parameter.Name));
                }
            }

            var lambda =
                CqlQuery.CompileQueryCommandLambda(
                    _queryText, // Command Text
                    ObjectContext.Perspective, // Perspective
                    null, // Parser options - null indicates 'use default'
                    parameters, // Parameters
                    null // Variables
                    );

            Debug.Assert(lambda.Variables == null || lambda.Variables.Count == 0, "lambda.Variables must be empty");

            return lambda.Body;
        }
    }
}
