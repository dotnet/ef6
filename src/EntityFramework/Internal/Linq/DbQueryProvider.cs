// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.Linq
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    // <summary>
    // A wrapping query provider that performs expression transformation and then delegates
    // to the <see cref="ObjectQuery" /> provider.  The <see cref="IQueryable" /> objects returned
    // are always instances of <see cref="DbQuery{TResult}" />. This provider is associated with
    // generic <see cref="DbQuery{T}" /> objects.
    // </summary>
    internal class DbQueryProvider : IQueryProvider
#if !NET40
, IDbAsyncQueryProvider
#endif
    {
        #region Fields and constructors

        private readonly InternalContext _internalContext;
        private readonly IInternalQuery _internalQuery;

        // <summary>
        // Creates a provider that wraps the given provider.
        // </summary>
        // <param name="internalQuery"> The internal query to wrap. </param>
        public DbQueryProvider(InternalContext internalContext, IInternalQuery internalQuery)
        {
            DebugCheck.NotNull(internalContext);
            DebugCheck.NotNull(internalQuery);

            _internalContext = internalContext;
            _internalQuery = internalQuery;
        }

        #endregion

        #region IQueryProvider Members

        // <summary>
        // Performs expression replacement and then delegates to the wrapped provider before wrapping
        // the returned <see cref="ObjectQuery" /> as a <see cref="DbQuery{T}" />.
        // </summary>
        public virtual IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            Check.NotNull(expression, "expression");

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

        // <summary>
        // Performs expression replacement and then delegates to the wrapped provider before wrapping
        // the returned <see cref="ObjectQuery" /> as a <see cref="DbQuery{T}" /> where T is determined
        // from the element type of the ObjectQuery.
        // </summary>
        public virtual IQueryable CreateQuery(Expression expression)
        {
            Check.NotNull(expression, "expression");

            return CreateQuery(CreateObjectQuery(expression));
        }

        // <summary>
        // By default, calls the same method on the wrapped provider.
        // </summary>
        public virtual TResult Execute<TResult>(Expression expression)
        {
            Check.NotNull(expression, "expression");

            _internalContext.Initialize();

            return ((IQueryProvider)_internalQuery.ObjectQueryProvider).Execute<TResult>(expression);
        }

        // <summary>
        // By default, calls the same method on the wrapped provider.
        // </summary>
        public virtual object Execute(Expression expression)
        {
            Check.NotNull(expression, "expression");

            _internalContext.Initialize();

            return ((IQueryProvider)_internalQuery.ObjectQueryProvider).Execute(expression);
        }

        #endregion

        #region IDbAsyncQueryProvider Members

#if !NET40

        // <summary>
        // By default, calls the same method on the wrapped provider.
        // </summary>
        Task<TResult> IDbAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            Check.NotNull(expression, "expression");

            _internalContext.Initialize();

            return ((IDbAsyncQueryProvider)_internalQuery.ObjectQueryProvider).ExecuteAsync<TResult>(expression, cancellationToken);
        }

        // <summary>
        // By default, calls the same method on the wrapped provider.
        // </summary>
        Task<object> IDbAsyncQueryProvider.ExecuteAsync(Expression expression, CancellationToken cancellationToken)
        {
            Check.NotNull(expression, "expression");

            _internalContext.Initialize();

            return ((IDbAsyncQueryProvider)_internalQuery.ObjectQueryProvider).ExecuteAsync(expression, cancellationToken);
        }

#endif

        #endregion

        #region Helpers

        // <summary>
        // Creates an appropriate generic IQueryable using Reflection and the underlying ElementType of
        // the given ObjectQuery.
        // </summary>
        private IQueryable CreateQuery(ObjectQuery objectQuery)
        {
            var internalQuery = CreateInternalQuery(objectQuery);

            var genericDbQueryType = typeof(DbQuery<>).MakeGenericType(internalQuery.ElementType);
            var constructor =
                genericDbQueryType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single();
            return (IQueryable)constructor.Invoke(new object[] { internalQuery });
        }

        // <summary>
        // Performs expression replacement and then delegates to the wrapped provider to create an
        // <see cref="ObjectQuery" />.
        // </summary>
        protected ObjectQuery CreateObjectQuery(Expression expression)
        {
            DebugCheck.NotNull(expression);

            expression = new DbQueryVisitor().Visit(expression);

            return (ObjectQuery)((IQueryProvider)_internalQuery.ObjectQueryProvider).CreateQuery(expression);
        }

        // <summary>
        // Wraps the given <see cref="ObjectQuery" /> as a <see cref="InternalQuery{T}" /> where T is determined
        // from the element type of the ObjectQuery.
        // </summary>
        protected IInternalQuery CreateInternalQuery(ObjectQuery objectQuery)
        {
            DebugCheck.NotNull(objectQuery);

            var genericInternalQueryType = typeof(InternalQuery<>).MakeGenericType(
                ((IQueryable)objectQuery).ElementType);
            var constructor = genericInternalQueryType.GetDeclaredConstructor(typeof(InternalContext), typeof(ObjectQuery));
            return (IInternalQuery)constructor.Invoke(new object[] { _internalContext, objectQuery });
        }

        // <summary>
        // Gets the internal context.
        // </summary>
        // <value> The internal context. </value>
        public InternalContext InternalContext
        {
            get { return _internalContext; }
        }

        #endregion
    }
}
