// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.Linq
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     A wrapping query provider that performs expression transformation and then delegates
    ///     to the <see cref="ObjectQuery" /> provider.  The <see cref="IQueryable" /> objects returned
    ///     are always instances of <see cref="DbQuery{TResult}" />. This provider is associated with
    ///     generic <see cref="DbQuery{T}" /> objects.
    /// </summary>
    internal class DbQueryProvider : IQueryProvider
#if !NET40
        , IDbAsyncQueryProvider
#endif
    {
        #region Fields and constructors

        private readonly InternalContext _internalContext;
        private readonly ObjectQueryProvider _provider;

        /// <summary>
        ///     Creates a provider that wraps the given provider.
        /// </summary>
        /// <param name="provider"> The provider to wrap. </param>
        public DbQueryProvider(InternalContext internalContext, ObjectQueryProvider provider)
        {
            Contract.Requires(internalContext != null);
            Contract.Requires(provider != null);

            _internalContext = internalContext;
            _provider = provider;
        }

        #endregion

        #region IQueryProvider Members

        /// <summary>
        ///     Performs expression replacement and then delegates to the wrapped provider before wrapping
        ///     the returned <see cref="ObjectQuery" /> as a <see cref="DbQuery{T}" />.
        /// </summary>
        public virtual IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var objectQuery = CreateObjectQuery(expression);

            // If the ElementType is different than the generic type then we need to use the ElementType
            // for the underlying type because then we can support covariance at the IQueryable level. That
            // is, it is possible to create IQueryable<object>.
            if (typeof(TElement)
                != ((IQueryable)objectQuery).ElementType)
            {
                return (IQueryable<TElement>)CreateQuery(objectQuery);
            }

            return new DbQuery<TElement>(new InternalQuery<TElement>(_internalContext, objectQuery));
        }

        /// <summary>
        ///     Performs expression replacement and then delegates to the wrapped provider before wrapping
        ///     the returned <see cref="ObjectQuery" /> as a <see cref="DbQuery{T}" /> where T is determined
        ///     from the element type of the ObjectQuery.
        /// </summary>
        public virtual IQueryable CreateQuery(Expression expression)
        {
            return CreateQuery(CreateObjectQuery(expression));
        }

        /// <summary>
        ///     By default, calls the same method on the wrapped provider.
        /// </summary>
        public virtual TResult Execute<TResult>(Expression expression)
        {
            _internalContext.Initialize();

            return ((IQueryProvider)_provider).Execute<TResult>(expression);
        }

        /// <summary>
        ///     By default, calls the same method on the wrapped provider.
        /// </summary>
        public virtual object Execute(Expression expression)
        {
            _internalContext.Initialize();

            return ((IQueryProvider)_provider).Execute(expression);
        }

        #endregion

        #region IDbAsyncQueryProvider Members

#if !NET40

        /// <summary>
        ///     By default, calls the same method on the wrapped provider.
        /// </summary>
        Task<TResult> IDbAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            _internalContext.Initialize();

            return ((IDbAsyncQueryProvider)_provider).ExecuteAsync<TResult>(expression, cancellationToken);
        }

        /// <summary>
        ///     By default, calls the same method on the wrapped provider.
        /// </summary>
        Task<object> IDbAsyncQueryProvider.ExecuteAsync(Expression expression, CancellationToken cancellationToken)
        {
            _internalContext.Initialize();

            return ((IDbAsyncQueryProvider)_provider).ExecuteAsync(expression, cancellationToken);
        }

#endif

        #endregion

        #region Helpers

        /// <summary>
        ///     Creates an appropriate generic IQueryable using Reflection and the underlying ElementType of
        ///     the given ObjectQuery.
        /// </summary>
        private IQueryable CreateQuery(ObjectQuery objectQuery)
        {
            var internalQuery = CreateInternalQuery(objectQuery);

            var genericDbQueryType = typeof(DbQuery<>).MakeGenericType(internalQuery.ElementType);
            var constructor =
                genericDbQueryType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single();
            return (IQueryable)constructor.Invoke(new object[] { internalQuery });
        }

        /// <summary>
        ///     Performs expression replacement and then delegates to the wrapped provider to create an
        ///     <see cref="ObjectQuery" />.
        /// </summary>
        protected ObjectQuery CreateObjectQuery(Expression expression)
        {
            Contract.Requires(expression != null);

            expression = new DbQueryVisitor().Visit(expression);

            return (ObjectQuery)((IQueryProvider)_provider).CreateQuery(expression);
        }

        /// <summary>
        ///     Wraps the given <see cref="ObjectQuery" /> as a <see cref="InternalQuery{T}" /> where T is determined
        ///     from the element type of the ObjectQuery.
        /// </summary>
        protected IInternalQuery CreateInternalQuery(ObjectQuery objectQuery)
        {
            Contract.Requires(objectQuery != null);

            var genericInternalQueryType = typeof(InternalQuery<>).MakeGenericType(
                ((IQueryable)objectQuery).ElementType);
            var constructor = genericInternalQueryType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public, null,
                new[] { typeof(InternalContext), typeof(ObjectQuery) }, null);
            return (IInternalQuery)constructor.Invoke(new object[] { _internalContext, objectQuery });
        }

        /// <summary>
        ///     Gets the internal context.
        /// </summary>
        /// <value> The internal context. </value>
        public InternalContext InternalContext
        {
            get { return _internalContext; }
        }

        #endregion
    }
}
