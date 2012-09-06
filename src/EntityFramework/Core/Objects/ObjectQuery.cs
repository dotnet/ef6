// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     This class implements untyped queries at the object-layer.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public abstract class ObjectQuery : IEnumerable, IOrderedQueryable, IListSource
#if !NET40
                                        , IDbAsyncEnumerable
#endif
    {
        #region Private Instance Members

        // -----------------
        // Instance Fields
        // -----------------

        /// <summary>
        ///     The underlying implementation of this ObjectQuery as provided by a concrete subclass
        ///     of ObjectQueryImplementation. Implementations currently exist for Entity-SQL- and Linq-to-Entities-based ObjectQueries.
        /// </summary>
        private readonly ObjectQueryState _state;

        /// <summary>
        ///     The result type of the query - 'TResultType' expressed as an O-Space type usage. Cached here and
        ///     only instantiated if the <see cref="GetResultType" /> method is called.
        /// </summary>
        private TypeUsage _resultType;

        /// <summary>
        ///     Every instance of ObjectQuery get a unique instance of the provider. This helps propagate state information
        ///     using the provider through LINQ operators.
        /// </summary>
        private ObjectQueryProvider _provider;

        #endregion

        #region Internal Constructors

        // --------------------
        // Internal Constructors
        // --------------------

        /// <summary>
        ///     The common constructor.
        /// </summary>
        /// <param name="queryState"> The underlying implementation of this ObjectQuery </param>
        /// <returns> A new ObjectQuery instance. </returns>
        internal ObjectQuery(ObjectQueryState queryState)
        {
            Debug.Assert(queryState != null, "ObjectQuery state cannot be null");

            // Set the query state.
            _state = queryState;
        }

        #endregion

        #region Internal Properties

        /// <summary>
        ///     Gets an untyped instantiation of the underlying ObjectQueryState that implements this ObjectQuery.
        /// </summary>
        internal ObjectQueryState QueryState
        {
            get { return _state; }
        }

        /// <summary>
        ///     Gets the <see cref="ObjectQueryProvider" /> associated with this query instance.
        /// </summary>
        internal virtual ObjectQueryProvider ObjectQueryProvider
        {
            get
            {
                if (_provider == null)
                {
                    _provider = new ObjectQueryProvider(this);
                }
                return _provider;
            }
        }

        #endregion

        #region Public Properties

        #region IListSource implementation

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        bool IListSource.ContainsListCollection
        {
            // this means that the IList we return is the one which contains our actual data, it is not a collection
            get { return false; }
        }

        #endregion

        /// <summary>
        ///     Gets the Command Text (if any) for this ObjectQuery.
        /// </summary>
        public string CommandText
        {
            get
            {
                string commandText;
                if (!_state.TryGetCommandText(out commandText))
                {
                    return String.Empty;
                }

                Debug.Assert(!string.IsNullOrEmpty(commandText), "Invalid Command Text returned");
                return commandText;
            }
        }

        /// <summary>
        ///     The context for the query, which includes the connection, cache and
        ///     metadata. Note that only the connection property is mutable and must be
        ///     set before a query can be executed.
        /// </summary>
        public ObjectContext Context
        {
            get { return _state.ObjectContext; }
        }

        /// <summary>
        ///     Allows optional control over how queried results interact with the object state manager.
        /// </summary>
        public MergeOption MergeOption
        {
            get { return _state.EffectiveMergeOption; }

            set
            {
                EntityUtil.CheckArgumentMergeOption(value);
                _state.UserSpecifiedMergeOption = value;
            }
        }

        /// <summary>
        ///     The parameter collection for this query.
        /// </summary>
        public ObjectParameterCollection Parameters
        {
            get { return _state.EnsureParameters(); }
        }

        /// <summary>
        ///     Defines if the query plan should be cached.
        /// </summary>
        public bool EnablePlanCaching
        {
            get { return _state.PlanCachingEnabled; }

            set { _state.PlanCachingEnabled = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Get the provider-specific command text used to execute this query
        /// </summary>
        /// <returns> </returns>
        [Browsable(false)]
        public string ToTraceString()
        {
            return _state.GetExecutionPlan(null).ToTraceString();
        }

        /// <summary>
        ///     This method returns information about the result type of the ObjectQuery.
        /// </summary>
        /// <returns> The TypeMetadata that describes the shape of the query results. </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public TypeUsage GetResultType()
        {
            Context.EnsureMetadata();
            if (null == _resultType)
            {
                // Retrieve the result type from the implementation, in terms of C-Space.
                var cSpaceQueryResultType = _state.ResultType;

                // Determine the 'TResultType' equivalent type usage based on the mapped O-Space type.
                // If the result type of the query is a collection[something], then
                // extract out the 'something' (element type) and use that. This
                // is the equivalent of saying the result type is T, rather than
                // IEnumerable<T>, which aligns with users' expectations.
                TypeUsage tResultType;
                if (!TypeHelpers.TryGetCollectionElementType(cSpaceQueryResultType, out tResultType))
                {
                    tResultType = cSpaceQueryResultType;
                }

                // Map the C-space result type to O-space.
                tResultType = _state.ObjectContext.Perspective.MetadataWorkspace.GetOSpaceTypeUsage(tResultType);
                if (null == tResultType)
                {
                    throw new InvalidOperationException(Strings.ObjectQuery_UnableToMapResultType);
                }

                _resultType = tResultType;
            }

            return _resultType;
        }

        /// <summary>
        ///     This method allows explicit query evaluation with a specified merge
        ///     option which will override the merge option property.
        /// </summary>
        /// <param name="mergeOption"> The MergeOption to use when executing the query. </param>
        /// <returns> An enumerable for the ObjectQuery results. </returns>
        public ObjectResult Execute(MergeOption mergeOption)
        {
            EntityUtil.CheckArgumentMergeOption(mergeOption);
            return ExecuteInternal(mergeOption);
        }

#if !NET40

        /// <summary>
        ///     An asynchronous version of Execute, which
        ///     allows explicit query evaluation with a specified merge
        ///     option which will override the merge option property.
        /// </summary>
        /// <param name="mergeOption"> The MergeOption to use when executing the query. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task containing an enumerable for the ObjectQuery results. </returns>
        public Task<ObjectResult> ExecuteAsync(MergeOption mergeOption)
        {
            return ExecuteAsync(mergeOption, CancellationToken.None);
        }

        /// <summary>
        ///     An asynchronous version of Execute, which
        ///     allows explicit query evaluation with a specified merge
        ///     option which will override the merge option property.
        /// </summary>
        /// <param name="mergeOption"> The MergeOption to use when executing the query. </param>
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <returns> A Task containing an enumerable for the ObjectQuery results. </returns>
        public Task<ObjectResult> ExecuteAsync(MergeOption mergeOption, CancellationToken cancellationToken)
        {
            EntityUtil.CheckArgumentMergeOption(mergeOption);
            return ExecuteInternalAsync(mergeOption, cancellationToken);
        }

#endif

        #region IListSource implementation

        /// <summary>
        ///     IListSource.GetList implementation
        /// </summary>
        /// <returns> IList interface over the data to bind </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IList IListSource.GetList()
        {
            return GetIListSourceListInternal();
        }

        #endregion

        #region IQueryable implementation

        /// <summary>
        ///     Gets the result element type for this query instance.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        Type IQueryable.ElementType
        {
            get { return _state.ElementType; }
        }

        /// <summary>
        ///     Gets the expression describing this query. For queries built using
        ///     LINQ builder patterns, returns a full LINQ expression tree; otherwise,
        ///     returns a constant expression wrapping this query. Note that the
        ///     default expression is not cached. This allows us to differentiate
        ///     between LINQ and Entity-SQL queries.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        Expression IQueryable.Expression
        {
            get { return GetExpression(); }
        }

        /// <summary>
        ///     Gets the <see cref="IQueryProvider" /> associated with this query instance.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IQueryProvider IQueryable.Provider
        {
            get { return ObjectQueryProvider; }
        }

        #endregion

        #region IEnumerable implementation

        /// <summary>
        ///     Returns an <see cref="IEnumerator" /> which when enumerated will execute the given SQL query against the database.
        /// </summary>
        /// <returns> The query results. </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        #endregion

        #region IDbAsyncEnumerable<T> implementation

#if !NET40

        /// <summary>
        ///     Returns an <see cref="IDbAsyncEnumerator" /> which when enumerated will execute the given SQL query against the database.
        /// </summary>
        /// <returns> The query results. </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetAsyncEnumeratorInternal();
        }

#endif

        #endregion

        #endregion

        #region Internal Methods

        internal abstract Expression GetExpression();
        internal abstract IEnumerator GetEnumeratorInternal();

#if !NET40

        internal abstract IDbAsyncEnumerator GetAsyncEnumeratorInternal();
        internal abstract Task<ObjectResult> ExecuteInternalAsync(MergeOption mergeOption, CancellationToken cancellationToken);

#endif

        internal abstract IList GetIListSourceListInternal();
        internal abstract ObjectResult ExecuteInternal(MergeOption mergeOption);

        #endregion
    }
}
