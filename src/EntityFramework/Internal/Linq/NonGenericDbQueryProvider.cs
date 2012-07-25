// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Internal.Linq
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    ///     A wrapping query provider that performs expression transformation and then delegates
    ///     to the <see cref = "ObjectQuery" /> provider.  The <see cref = "IQueryable" /> objects returned
    ///     are instances of <see cref = "DbQuery{TResult}" /> when the generic CreateQuery method is
    ///     used and are instances of <see cref = "DbQuery" /> when the non-generic CreateQuery method
    ///     is used.  This provider is associated with non-generic <see cref = "DbQuery" /> objects.
    /// </summary>
    internal class NonGenericDbQueryProvider : DbQueryProvider
    {
        #region Fields and constructors

        /// <summary>
        ///     Creates a provider that wraps the given provider.
        /// </summary>
        /// <param name = "provider">The provider to wrap.</param>
        public NonGenericDbQueryProvider(InternalContext internalContext, ObjectQueryProvider provider)
            : base(internalContext, provider)
        {
        }

        #endregion

        #region IQueryProvider Members

        /// <summary>
        ///     Performs expression replacement and then delegates to the wrapped provider before wrapping
        ///     the returned <see cref = "ObjectQuery" /> as a <see cref = "DbQuery" />.
        /// </summary>
        public override IQueryable<TElement> CreateQuery<TElement>(Expression expression)
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

            return new InternalDbQuery<TElement>(new InternalQuery<TElement>(InternalContext, objectQuery));
        }

        /// <summary>
        ///     Delegates to the wrapped provider except returns instances of <see cref = "DbQuery" />.
        /// </summary>
        public override IQueryable CreateQuery(Expression expression)
        {
            return CreateQuery(CreateObjectQuery(expression));
        }

        /// <summary>
        ///     Creates an appropriate generic IQueryable using Reflection and the underlying ElementType of
        ///     the given ObjectQuery.
        /// </summary>
        private IQueryable CreateQuery(ObjectQuery objectQuery)
        {
            var internalQuery = CreateInternalQuery(objectQuery);

            var genericDbQueryType = typeof(InternalDbQuery<>).MakeGenericType(internalQuery.ElementType);
            var constructor = genericDbQueryType.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Single();
            return (IQueryable)constructor.Invoke(new object[] { internalQuery });
        }

        #endregion
    }
}
