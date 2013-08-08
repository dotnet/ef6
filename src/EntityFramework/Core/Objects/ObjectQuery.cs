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
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// This class implements untyped queries at the object-layer.
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
        /// The underlying implementation of this ObjectQuery as provided by a concrete subclass
        /// of ObjectQueryImplementation. Implementations currently exist for Entity-SQL- and Linq-to-Entities-based ObjectQueries.
        /// </summary>
        private readonly ObjectQueryState _state;

        /// <summary>
        /// The result type of the query - 'TResultType' expressed as an O-Space type usage. Cached here and
        /// only instantiated if the <see cref="GetResultType" /> method is called.
        /// </summary>
        private TypeUsage _resultType;

        /// <summary>
        /// Every instance of ObjectQuery get a unique instance of the provider. This helps propagate state information
        /// using the provider through LINQ operators.
        /// </summary>
        private ObjectQueryProvider _provider;

        #endregion

        #region Internal Constructors

        // --------------------
        // Internal Constructors
        // --------------------

        /// <summary>
        /// The common constructor.
        /// </summary>
        /// <param name="queryState"> The underlying implementation of this ObjectQuery </param>
        /// <returns> A new ObjectQuery instance. </returns>
        internal ObjectQuery(ObjectQueryState queryState)
        {
            DebugCheck.NotNull(queryState);

            // Set the query state.
            _state = queryState;
        }

        /// <summary>
        /// For testing.
        /// </summary>
        internal ObjectQuery()
        {
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// Gets an untyped instantiation of the underlying ObjectQueryState that implements this ObjectQuery.
        /// </summary>
        internal ObjectQueryState QueryState
        {
            get { return _state; }
        }

        /// <summary>
        /// Gets the <see cref="ObjectQueryProvider" /> associated with this query instance.
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

        /// <summary>Returns the command text for the query.</summary>
        /// <returns>A string value.</returns>
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

        /// <summary>Gets the object context associated with this object query.</summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Objects.ObjectContext" /> associated with this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" />
        /// instance.
        /// </returns>
        public ObjectContext Context
        {
            get { return _state.ObjectContext; }
        }

        /// <summary>Gets or sets how objects returned from a query are added to the object context. </summary>
        /// <returns>
        /// The query <see cref="T:System.Data.Entity.Core.Objects.MergeOption" />.
        /// </returns>
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
        /// Whether the query is streaming or buffering
        /// </summary>
        public bool Streaming
        {
            get { return _state.EffectiveStreamingBehaviour; }
            set { _state.UserSpecifiedStreamingBehaviour = value; }
        }

        /// <summary>Gets the parameter collection for this object query.</summary>
        /// <returns>
        /// The parameter collection for this <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" />.
        /// </returns>
        public ObjectParameterCollection Parameters
        {
            get { return _state.EnsureParameters(); }
        }

        /// <summary>Gets or sets a value that indicates whether the query plan should be cached.</summary>
        /// <returns>A value that indicates whether the query plan should be cached.</returns>
        public bool EnablePlanCaching
        {
            get { return _state.PlanCachingEnabled; }

            set { _state.PlanCachingEnabled = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>Returns the commands to execute against the data source.</summary>
        /// <returns>A string that represents the commands that the query executes against the data source.</returns>
        [Browsable(false)]
        public string ToTraceString()
        {
            return _state.GetExecutionPlan(null).ToTraceString();
        }

        /// <summary>Returns information about the result type of the query.</summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.TypeUsage" /> value that contains information about the result type of the query.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public TypeUsage GetResultType()
        {
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

        /// <summary>Executes the untyped object query with the specified merge option.</summary>
        /// <param name="mergeOption">
        /// The <see cref="T:System.Data.Entity.Core.Objects.MergeOption" /> to use when executing the query. 
        /// The default is <see cref="F:System.Data.Entity.Core.Objects.MergeOption.AppendOnly" />.
        /// </param>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Objects.ObjectResult`1" /> that contains a collection of entity objects returned by the query.
        /// </returns>
        public ObjectResult Execute(MergeOption mergeOption)
        {
            EntityUtil.CheckArgumentMergeOption(mergeOption);
            return ExecuteInternal(mergeOption);
        }

#if !NET40

        /// <summary>
        /// Asynchronously executes the untyped object query with the specified merge option.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="mergeOption">
        /// The <see cref="T:System.Data.Entity.Core.Objects.MergeOption" /> to use when executing the query. 
        /// The default is <see cref="F:System.Data.Entity.Core.Objects.MergeOption.AppendOnly" />.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains an an <see cref="T:System.Data.Entity.Core.Objects.ObjectResult`1" /> 
        /// that contains a collection of entity objects returned by the query.
        /// </returns>
        public Task<ObjectResult> ExecuteAsync(MergeOption mergeOption)
        {
            return ExecuteAsync(mergeOption, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously executes the untyped object query with the specified merge option.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="mergeOption">
        /// The <see cref="T:System.Data.Entity.Core.Objects.MergeOption" /> to use when executing the query. 
        /// The default is <see cref="F:System.Data.Entity.Core.Objects.MergeOption.AppendOnly" />.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains an an <see cref="T:System.Data.Entity.Core.Objects.ObjectResult`1" /> 
        /// that contains a collection of entity objects returned by the query.
        /// </returns>
        public Task<ObjectResult> ExecuteAsync(MergeOption mergeOption, CancellationToken cancellationToken)
        {
            EntityUtil.CheckArgumentMergeOption(mergeOption);
            return ExecuteInternalAsync(mergeOption, cancellationToken);
        }

#endif

        #region IListSource implementation

        /// <summary>
        /// Returns the collection as an <see cref="T:System.Collections.IList" /> used for data binding.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IList" /> of entity objects.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IList IListSource.GetList()
        {
            return GetIListSourceListInternal();
        }

        #endregion

        #region IQueryable implementation

        /// <summary>
        /// Gets the result element type for this query instance.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        Type IQueryable.ElementType
        {
            get { return _state.ElementType; }
        }

        /// <summary>
        /// Gets the expression describing this query. For queries built using
        /// LINQ builder patterns, returns a full LINQ expression tree; otherwise,
        /// returns a constant expression wrapping this query. Note that the
        /// default expression is not cached. This allows us to differentiate
        /// between LINQ and Entity-SQL queries.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        Expression IQueryable.Expression
        {
            get { return GetExpression(); }
        }

        /// <summary>
        /// Gets the <see cref="IQueryProvider" /> associated with this query instance.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IQueryProvider IQueryable.Provider
        {
            get { return ObjectQueryProvider; }
        }

        #endregion

        #region IEnumerable implementation

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        #endregion

        #region IDbAsyncEnumerable<T> implementation

#if !NET40

        /// <summary>
        /// Returns an <see cref="IDbAsyncEnumerator" /> which when enumerated will execute the given SQL query against the database.
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
