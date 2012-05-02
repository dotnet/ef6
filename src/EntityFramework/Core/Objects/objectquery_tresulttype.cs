namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///   This class implements strongly-typed queries at the object-layer through
    ///   Entity SQL text and query-building helper methods. 
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public partial class ObjectQuery<T> : ObjectQuery, IEnumerable<T>, IQueryable<T>, IOrderedQueryable<T>, IListSource, IDbAsyncEnumerable<T>
    {
        internal ObjectQuery(ObjectQueryState queryState)
            : base(queryState)
        {
        }

        #region Public Methods

        /// <summary>
        ///   This method allows explicit query evaluation with a specified merge
        ///   option which will override the merge option property.
        /// </summary>
        /// <param name="mergeOption">
        ///   The MergeOption to use when executing the query.
        /// </param>
        /// <returns>
        ///   An enumerable for the ObjectQuery results.
        /// </returns>
        public new ObjectResult<T> Execute(MergeOption mergeOption)
        {
            EntityUtil.CheckArgumentMergeOption(mergeOption);
            return GetResults(mergeOption);
        }

        /// <summary>
        ///   An asynchronous version of Execute, which
        ///   allows explicit query evaluation with a specified merge
        ///   option which will override the merge option property.
        /// </summary>
        /// <param name="mergeOption">
        ///   The MergeOption to use when executing the query.
        /// </param>
        /// <returns>
        ///   A Task containing an enumerable for the ObjectQuery results.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<ObjectResult<T>> ExecuteAsync(MergeOption mergeOption)
        {
            return ExecuteAsync(mergeOption, CancellationToken.None);
        }

        /// <summary>
        ///   An asynchronous version of Execute, which
        ///   allows explicit query evaluation with a specified merge
        ///   option which will override the merge option property.
        /// </summary>
        /// <param name="mergeOption">
        ///   The MergeOption to use when executing the query.
        /// </param>
        /// <param name="cancellationToken">
        ///   The token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        ///   A Task containing an enumerable for the ObjectQuery results.
        /// </returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken"),
        SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "mergeOption")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task<ObjectResult<T>> ExecuteAsync(MergeOption mergeOption, CancellationToken cancellationToken)
        {
            EntityUtil.CheckArgumentMergeOption(mergeOption);

            throw new NotImplementedException();
        }

        /// <summary>
        ///   Adds a path to the set of navigation property span paths included in the results of this query
        /// </summary>
        /// <param name="path">The new span path</param>
        /// <returns>A new ObjectQuery that includes the specified span path</returns>
        public ObjectQuery<T> Include(string path)
        {
            EntityUtil.CheckStringArgument(path, "path");
            return new ObjectQuery<T>(QueryState.Include(this, path));
        }

        #endregion

        #region IEnumerable<T> implementation

        /// <summary>
        ///   These methods are the "executors" for the query. They can be called
        ///   directly, or indirectly (by foreach'ing through the query, for example).
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            var disposableEnumerable = GetResults(null);
            try
            {
                var result = disposableEnumerable.GetEnumerator();
                return result;
            }
            catch
            {
                // if there is a problem creating the enumerator, we should dispose
                // the enumerable (if there is no problem, the enumerator will take 
                // care of the dispose)
                disposableEnumerable.Dispose();
                throw;
            }
        }

        #endregion

        #region IDbAsyncEnumerable<T> implementation

        /// <summary>
        /// Gets an enumerator that can be used to asynchronously enumerate the sequence. 
        /// </summary>
        /// <returns>Enumerator for asynchronous enumeration over the sequence.</returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IDbAsyncEnumerator<T> IDbAsyncEnumerable<T>.GetAsyncEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ObjectQuery Overrides

        internal override IEnumerator GetEnumeratorInternal()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }

        internal override IList GetIListSourceListInternal()
        {
            return ((IListSource)GetResults(null)).GetList();
        }

        internal override ObjectResult ExecuteInternal(MergeOption mergeOption)
        {
            return GetResults(mergeOption);
        }

        /// <summary>
        /// Retrieves the LINQ expression that backs this ObjectQuery for external consumption.
        /// It is important that the work to wrap the expression in an appropriate MergeAs call
        /// takes place in this method and NOT in ObjectQueryState.TryGetExpression which allows
        /// the unmodified expression (that does not include the MergeOption-preserving MergeAs call)
        /// to be retrieved and processed by the ELinq ExpressionConverter.
        /// </summary>
        /// <returns>
        ///   The LINQ expression for this ObjectQuery, wrapped in a MergeOption-preserving call
        ///   to the MergeAs method if the ObjectQuery.MergeOption property has been set.
        /// </returns>
        internal override Expression GetExpression()
        {
            // If this ObjectQuery is not backed by a LINQ Expression (it is an ESQL query),
            // then create a ConstantExpression that uses this ObjectQuery as its value.
            Expression retExpr;
            if (!QueryState.TryGetExpression(out retExpr))
            {
                retExpr = Expression.Constant(this);
            }

            var objectQueryType = typeof(ObjectQuery<T>);
            if (QueryState.UserSpecifiedMergeOption.HasValue)
            {
                var mergeAsMethod = objectQueryType.GetMethod("MergeAs", BindingFlags.Instance | BindingFlags.NonPublic);
                Debug.Assert(mergeAsMethod != null, "Could not retrieve ObjectQuery<T>.MergeAs method using reflection?");
                retExpr = TypeSystem.EnsureType(retExpr, objectQueryType);
                retExpr = Expression.Call(retExpr, mergeAsMethod, Expression.Constant(QueryState.UserSpecifiedMergeOption.Value));
            }

            if (null != QueryState.Span)
            {
                var includeSpanMethod = objectQueryType.GetMethod("IncludeSpan", BindingFlags.Instance | BindingFlags.NonPublic);
                Debug.Assert(includeSpanMethod != null, "Could not retrieve ObjectQuery<T>.IncludeSpan method using reflection?");
                retExpr = TypeSystem.EnsureType(retExpr, objectQueryType);
                retExpr = Expression.Call(retExpr, includeSpanMethod, Expression.Constant(QueryState.Span));
            }

            return retExpr;
        }

        // Intended for use only in the MethodCallExpression produced for inline queries.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "mergeOption")]
        internal ObjectQuery<T> MergeAs(MergeOption mergeOption)
        {
            throw new InvalidOperationException(Strings.ELinq_MethodNotDirectlyCallable);
        }

        // Intended for use only in the MethodCallExpression produced for inline queries.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "span")]
        internal ObjectQuery<T> IncludeSpan(Span span)
        {
            throw new InvalidOperationException(Strings.ELinq_MethodNotDirectlyCallable);
        }

        #endregion

        #region Private Methods

        private ObjectResult<T> GetResults(MergeOption? forMergeOption)
        {
            QueryState.ObjectContext.EnsureConnection();

            try
            {
                var execPlan = QueryState.GetExecutionPlan(forMergeOption);
                return execPlan.Execute<T>(QueryState.ObjectContext, QueryState.Parameters);
            }
            catch
            {
                QueryState.ObjectContext.ReleaseConnection();
                throw;
            }
        }

        #endregion
    }
}
