// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Common.QueryCache;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Models a Linq to Entities ObjectQuery
    /// </summary>
    internal class ELinqQueryState : ObjectQueryState
    {
        #region Private State

        private readonly Expression _expression;
        private Func<bool> _recompileRequired;
        private IEnumerable<Tuple<ObjectParameter, QueryParameterExpression>> _linqParameters;
        private bool _useCSharpNullComparisonBehavior;
        private readonly ObjectQueryExecutionPlanFactory _objectQueryExecutionPlanFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new <see cref="ELinqQueryState" /> instance based on the specified Linq Expression
        /// against the specified ObjectContext.
        /// </summary>
        /// <param name="elementType"> The element type of the implemented ObjectQuery, as a CLR type. </param>
        /// <param name="context"> The ObjectContext with which the implemented ObjectQuery is associated. </param>
        /// <param name="expression"> The Linq Expression that defines this query. </param>
        internal ELinqQueryState(
            Type elementType, ObjectContext context, Expression expression,
            ObjectQueryExecutionPlanFactory objectQueryExecutionPlanFactory = null)
            : base(elementType, context, null, null)
        {
            //
            // Initialize the LINQ expression, which is passed in via
            // public APIs on ObjectQuery and must be checked here
            // (the base class performs similar checks on the ObjectContext and MergeOption arguments).
            //
            DebugCheck.NotNull(expression);
            // closure bindings and initializers are explicitly allowed to be null

            _expression = expression;
            _useCSharpNullComparisonBehavior = context.ContextOptions.UseCSharpNullComparisonBehavior;
            _objectQueryExecutionPlanFactory = objectQueryExecutionPlanFactory ?? new ObjectQueryExecutionPlanFactory();
        }

        /// <summary>
        /// Constructs a new <see cref="ELinqQueryState" /> instance based on the specified Linq Expression,
        /// copying the state information from the specified ObjectQuery.
        /// </summary>
        /// <param name="elementType"> The element type of the implemented ObjectQuery, as a CLR type. </param>
        /// <param name="query"> The ObjectQuery from which the state information should be copied. </param>
        /// <param name="expression"> The Linq Expression that defines this query. </param>
        internal ELinqQueryState(
            Type elementType, ObjectQuery query, Expression expression,
            ObjectQueryExecutionPlanFactory objectQueryExecutionPlanFactory = null)
            : base(elementType, query)
        {
            DebugCheck.NotNull(expression);
            _expression = expression;
            _objectQueryExecutionPlanFactory = objectQueryExecutionPlanFactory ?? new ObjectQueryExecutionPlanFactory();
        }

        #endregion

        #region ObjectQueryState overrides

        protected override TypeUsage GetResultType()
        {
            // Since this method is only called once, on demand, a full conversion pass
            // is performed to produce the DbExpression and return its result type. 
            // This does not affect any cached execution plan or closure bindings that may be present.
            var converter = CreateExpressionConverter();
            return converter.Convert().ResultType;
        }

        internal override ObjectQueryExecutionPlan GetExecutionPlan(MergeOption? forMergeOption)
        {
            Debug.Assert(Span == null, "Include span specified on compiled LINQ-based ObjectQuery instead of within the expression tree?");

            // If this query has already been prepared, its current execution plan may no longer be valid.
            var plan = _cachedPlan;
            if (plan != null)
            {
                // Was a merge option specified in the call to Execute(MergeOption) or set via ObjectQuery.MergeOption?
                var explicitMergeOption = GetMergeOption(forMergeOption, UserSpecifiedMergeOption);

                // If a merge option was explicitly specified, and it does not match the plan's merge option, then the plan is no longer valid.
                // If the context flag UseCSharpNullComparisonBehavior was modified, then the plan is no longer valid.
                if ((explicitMergeOption.HasValue &&
                     explicitMergeOption.Value != plan.MergeOption)
                    || _recompileRequired()
                    || ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior != _useCSharpNullComparisonBehavior)
                {
                    plan = null;
                }
            }

            // The plan may have been invalidated above, or this query may never have been prepared.
            if (plan == null)
            {
                // Reset internal state
                _recompileRequired = null;
                ResetParameters();

                // Translate LINQ expression to a DbExpression
                var converter = CreateExpressionConverter();
                var queryExpression = converter.Convert();

                // This delegate tells us when a part of the expression tree has changed requiring a recompile.
                _recompileRequired = converter.RecompileRequired;

                // Determine the merge option, with the following precedence:
                // 1. A merge option was specified explicitly as the argument to Execute(MergeOption).
                // 2. The user has set the MergeOption property on the ObjectQuery instance.
                // 3. A merge option has been extracted from the 'root' query and propagated to the root of the expression tree.
                // 4. The global default merge option.
                var mergeOption = EnsureMergeOption(
                    forMergeOption,
                    UserSpecifiedMergeOption,
                    converter.PropagatedMergeOption);

                _useCSharpNullComparisonBehavior = ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior;

                // If parameters were aggregated from referenced (non-LINQ) ObjectQuery instances then add them to the parameters collection
                _linqParameters = converter.GetParameters();
                if (_linqParameters != null
                    && _linqParameters.Any())
                {
                    var currentParams = EnsureParameters();
                    currentParams.SetReadOnly(false);
                    foreach (var pair in _linqParameters)
                    {
                        // Note that it is safe to add the parameter directly only
                        // because parameters are cloned before they are added to the
                        // converter's parameter collection, or they came from this
                        // instance's parameter collection in the first place.
                        var convertedParam = pair.Item1;
                        currentParams.Add(convertedParam);
                    }
                    currentParams.SetReadOnly(true);
                }

                // Try retrieving the execution plan from the global query cache (if plan caching is enabled).
                QueryCacheManager cacheManager = null;
                LinqQueryCacheKey cacheKey = null;
                if (PlanCachingEnabled && !_recompileRequired())
                {
                    // Create a new cache key that reflects the current state of the Parameters collection
                    // and the Span object (if any), and uses the specified merge option.
                    string expressionKey;
                    if (ExpressionKeyGen.TryGenerateKey(queryExpression, out expressionKey))
                    {
                        cacheKey = new LinqQueryCacheKey(
                            expressionKey,
                            (null == Parameters ? 0 : Parameters.Count),
                            (null == Parameters ? null : Parameters.GetCacheKey()),
                            (null == converter.PropagatedSpan ? null : converter.PropagatedSpan.GetCacheKey()),
                            mergeOption,
                            EffectiveStreamingBehaviour,
                            _useCSharpNullComparisonBehavior,
                            ElementType);

                        cacheManager = ObjectContext.MetadataWorkspace.GetQueryCacheManager();
                        ObjectQueryExecutionPlan executionPlan = null;
                        if (cacheManager.TryCacheLookup(cacheKey, out executionPlan))
                        {
                            plan = executionPlan;
                        }
                    }
                }

                // If execution plan wasn't retrieved from the cache, build a new one and cache it.
                if (plan == null)
                {
                    var tree = DbQueryCommandTree.FromValidExpression(ObjectContext.MetadataWorkspace, DataSpace.CSpace, queryExpression);
                    plan = _objectQueryExecutionPlanFactory.Prepare(
                        ObjectContext, tree, ElementType, mergeOption, EffectiveStreamingBehaviour, converter.PropagatedSpan, null,
                        converter.AliasGenerator);

                    // If caching is enabled then update the cache now.
                    // Note: the logic is the same as in EntitySqlQueryState.
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

                // Remember the current plan in the local cache, so that we don't have to recalc the key and look into the global cache
                // if the same instance of query gets executed more than once.
                _cachedPlan = plan;
            }

            // Evaluate parameter values for the query.
            if (_linqParameters != null)
            {
                foreach (var pair in _linqParameters)
                {
                    var parameter = pair.Item1;
                    var parameterExpression = pair.Item2;
                    if (null != parameterExpression)
                    {
                        parameter.Value = parameterExpression.EvaluateParameter(null);
                    }
                }
            }

            return plan;
        }

        /// <summary>
        /// Returns a new ObjectQueryState instance with the specified navigation property path specified as an Include span.
        /// For eLINQ queries the Include operation is modelled as a method call expression applied to the source ObectQuery,
        /// so the <see cref="Span" /> property is always <c>null</c> on the returned instance.
        /// </summary>
        /// <typeparam name="TElementType"> The element type of the resulting query </typeparam>
        /// <param name="sourceQuery"> The ObjectQuery on which Include was called; required to build the new method call expression </param>
        /// <param name="includePath"> The new Include path </param>
        /// <returns> A new ObjectQueryState instance that incorporates the Include path, in this case a new method call expression </returns>
        internal override ObjectQueryState Include<TElementType>(ObjectQuery<TElementType> sourceQuery, string includePath)
        {
            var includeMethod = GetIncludeMethod(sourceQuery);
            Debug.Assert(includeMethod != null, "Unable to find ObjectQuery.Include method?");

            Expression includeCall = Expression.Call(
                Expression.Constant(sourceQuery), includeMethod, new Expression[] { Expression.Constant(includePath, typeof(string)) });
            ObjectQueryState retState = new ELinqQueryState(ElementType, ObjectContext, includeCall);
            ApplySettingsTo(retState);
            return retState;
        }

        internal static MethodInfo GetIncludeMethod<TElementType>(ObjectQuery<TElementType> sourceQuery)
        {
            return sourceQuery.GetType().GetDeclaredMethod("Include");
        }

        /// <summary>
        /// eLINQ queries do not have command text. This method always returns <c>false</c>.
        /// </summary>
        /// <param name="commandText">
        /// Always set to <c>null</c>
        /// </param>
        /// <returns>
        /// Always returns <c>false</c>
        /// </returns>
        internal override bool TryGetCommandText(out string commandText)
        {
            commandText = null;
            return false;
        }

        /// <summary>
        /// Gets the LINQ Expression that defines this query for external (of ObjectQueryState) use.
        /// Note that the <see cref="Expression" /> property is used, which is overridden by compiled eLINQ
        /// queries to produce an Expression tree where parameter references have been replaced with constants.
        /// </summary>
        /// <param name="expression"> The LINQ expression that describes this query </param>
        /// <returns>
        /// Always returns <c>true</c>
        /// </returns>
        internal override bool TryGetExpression(out Expression expression)
        {
            expression = Expression;
            return true;
        }

        #endregion

        internal virtual Expression Expression
        {
            get { return _expression; }
        }

        protected virtual ExpressionConverter CreateExpressionConverter()
        {
            var funcletizer = Funcletizer.CreateQueryFuncletizer(ObjectContext);
            return new ExpressionConverter(funcletizer, _expression);
        }

        private void ResetParameters()
        {
            if (Parameters != null)
            {
                var wasLocked = ((ICollection<ObjectParameter>)Parameters).IsReadOnly;
                if (wasLocked)
                {
                    Parameters.SetReadOnly(false);
                }
                Parameters.Clear();
                if (wasLocked)
                {
                    Parameters.SetReadOnly(true);
                }
            }
            _linqParameters = null;
        }
    }
}
