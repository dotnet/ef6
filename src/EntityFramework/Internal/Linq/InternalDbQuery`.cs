// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.Linq
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     An instance of this internal class is created whenever an instance of the public <see cref="DbQuery" />
    ///     class is needed. This allows the public surface to be non-generic, while the runtime type created
    ///     still implements <see cref="IQueryable{T}" />.
    /// </summary>
    /// <typeparam name="TElement"> The type of the element. </typeparam>
    internal class InternalDbQuery<TElement> : DbQuery, IOrderedQueryable<TElement>
#if !NET40
                                               , IDbAsyncEnumerable<TElement>
#endif
    {
        #region Fields and constructors

        // Handles the underlying ObjectQuery that backs the query.
        private readonly IInternalQuery<TElement> _internalQuery;

        /// <summary>
        ///     Creates a new query that will be backed by the given internal query object.
        /// </summary>
        /// <param name="internalQuery"> The backing query. </param>
        public InternalDbQuery(IInternalQuery<TElement> internalQuery)
        {
            DebugCheck.NotNull(internalQuery);

            _internalQuery = internalQuery;
        }

        #endregion

        #region Implementation of abstract methods defined on DbQuery

        /// <inheritdoc />
        internal override IInternalQuery InternalQuery
        {
            get { return _internalQuery; }
        }

        /// <inheritdoc />
        public override DbQuery Include(string path)
        {
            // We need this because the Code Contract gets compiled out in the release build even though
            // this method is effectively on the public surface because it overrides the abstract method on DbSet.
            Check.NotEmpty(path, "path");

            return new InternalDbQuery<TElement>(_internalQuery.Include(path));
        }

        /// <inheritdoc />
        public override DbQuery AsNoTracking()
        {
            return new InternalDbQuery<TElement>(_internalQuery.AsNoTracking());
        }

        /// <inheritdoc />
        public override DbQuery AsStreaming()
        {
            return new InternalDbQuery<TElement>(_internalQuery.AsStreaming());
        }

        internal override IInternalQuery GetInternalQueryWithCheck(string memberName)
        {
            return _internalQuery;
        }

        #endregion

        #region IEnumerable implementation

        /// <summary>
        ///     Returns an <see cref="IEnumerator{TEntity}" /> which when enumerated will execute the query against the database.
        /// </summary>
        /// <returns> An enumerator for the query </returns>
        public IEnumerator<TElement> GetEnumerator()
        {
            return _internalQuery.GetEnumerator();
        }

        #endregion

        #region IDbAsyncEnumerable implementation

#if !NET40

        /// <summary>
        ///     Returns an <see cref="IDbAsyncEnumerator{TEntity}" /> which when enumerated will execute the query against the database.
        /// </summary>
        /// <returns> An enumerator for the query </returns>
        public IDbAsyncEnumerator<TElement> GetAsyncEnumerator()
        {
            return _internalQuery.GetAsyncEnumerator();
        }

#endif

        #endregion
    }
}
