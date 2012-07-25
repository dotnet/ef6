// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Internal.Linq
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    ///     An InternalQuery underlies every instance of DbSet and DbQuery.  It acts to lazily initialize a InternalContext as well
    ///     as an ObjectQuery and EntitySet the first time that it is used.  The InternalQuery also acts to expose necessary
    ///     information to other parts of the design in a controlled manner without adding a lot of internal methods and
    ///     properties to the DbSet and DbQuery classes themselves.
    /// </summary>
    /// <typeparam name = "TElement">The type of entity to query for.</typeparam>
    internal class InternalQuery<TElement> : IInternalQuery<TElement>
    {
        #region Fields and constructors and initalization

        private readonly InternalContext _internalContext;
        private ObjectQuery<TElement> _objectQuery;

        /// <summary>
        ///     Creates a new query that will be backed by the given InternalContext.
        /// </summary>
        /// <param name = "internalContext">The backing context.</param>
        public InternalQuery(InternalContext context)
        {
            Contract.Requires(context != null);

            _internalContext = context;
        }

        /// <summary>
        ///     Creates a new internal query based on the information in an existing query together with
        ///     a new underlying ObjectQuery.
        /// </summary>
        public InternalQuery(InternalContext internalContext, ObjectQuery objectQuery)
        {
            _internalContext = internalContext;
            _objectQuery = (ObjectQuery<TElement>)objectQuery;
        }

        /// <summary>
        ///     Resets the query to its uninitialized state so that it will be re-lazy initialized the next
        ///     time it is used.  This allows the ObjectContext backing a DbContext to be switched out.
        /// </summary>
        public virtual void ResetQuery()
        {
            _objectQuery = null;
        }

        #endregion

        #region Underlying context

        /// <summary>
        ///     The underlying InternalContext.
        /// </summary>
        public virtual InternalContext InternalContext
        {
            get { return _internalContext; }
        }

        #endregion

        #region Include

        /// <summary>
        ///     Updates the underlying ObjectQuery with the given include path.
        /// </summary>
        /// <param name = "path">The include path.</param>
        /// <returns>A new query containing the defined include path.</returns>
        public virtual IInternalQuery<TElement> Include(string path)
        {
            return new InternalQuery<TElement>(_internalContext, _objectQuery.Include(path));
        }

        #endregion

        #region AsNoTracking

        /// <summary>
        ///     Returns a new query where the entities returned will not be cached in the <see cref = "DbContext" />.
        /// </summary>
        /// <returns> A new query with NoTracking applied.</returns>
        public virtual IInternalQuery<TElement> AsNoTracking()
        {
            return new InternalQuery<TElement>(
                _internalContext, (ObjectQuery)DbHelpers.CreateNoTrackingQuery(_objectQuery));
        }

        #endregion

        #region Query properties

        /// <summary>
        ///     The underlying ObjectQuery.
        /// </summary>
        public virtual ObjectQuery<TElement> ObjectQuery
        {
            get
            {
                Contract.Assert(_objectQuery != null, "InternalQuery should have been initialized.");

                return _objectQuery;
            }
        }

        /// <summary>
        ///     The underlying ObjectQuery.
        /// </summary>
        ObjectQuery IInternalQuery.ObjectQuery
        {
            get { return ObjectQuery; }
        }

        #endregion

        #region Initialization

        /// <summary>
        ///     Performs lazy initialization of the underlying ObjectContext, ObjectQuery, and EntitySet objects
        ///     so that the query can be used.
        /// </summary>
        protected void InitializeQuery(ObjectQuery<TElement> objectQuery)
        {
            Contract.Assert(_objectQuery == null, "InternalQuery should not be initialized twice.");

            _objectQuery = objectQuery;
        }

        #endregion

        #region ToString

        /// <summary>
        ///     Returns a <see cref = "System.String" /> representation of the underlying query, equivalent
        ///     to ToTraceString on ObjectQuery.
        /// </summary>
        /// <returns>
        ///     The query string.
        /// </returns>
        public override string ToString()
        {
            Contract.Assert(_objectQuery != null, "InternalQuery should have been initialized.");

            return _objectQuery.ToTraceString();
        }

        #endregion

        #region IQueryable

        /// <summary>
        ///     The LINQ query expression.
        /// </summary>
        public virtual Expression Expression
        {
            get
            {
                Contract.Assert(_objectQuery != null, "InternalQuery should have been initialized.");

                return ((IQueryable)_objectQuery).Expression;
            }
        }

        /// <summary>
        ///     The LINQ query provider for the underlying <see cref = "ObjectQuery" />.
        /// </summary>
        public virtual ObjectQueryProvider ObjectQueryProvider
        {
            get
            {
                Contract.Assert(_objectQuery != null, "InternalQuery should have been initialized.");

                return _objectQuery.Provider;
            }
        }

        /// <summary>
        ///     The IQueryable element type.
        /// </summary>
        public Type ElementType
        {
            get { return typeof(TElement); }
        }

        #endregion

        #region IEnumerable

        /// <summary>
        ///     Returns an <see cref="IEnumerator{TElement}"/> which when enumerated will execute the query against the database.
        /// </summary>
        /// <returns>The query results.</returns>
        public virtual IEnumerator<TElement> GetEnumerator()
        {
            Contract.Assert(_objectQuery != null, "InternalQuery should have been initialized.");

            InternalContext.Initialize();

            return ((IEnumerable<TElement>)_objectQuery).GetEnumerator();
        }

        /// <summary>
        ///     Returns an <see cref="IEnumerator{TElement}"/> which when enumerated will execute the query against the database.
        /// </summary>
        /// <returns>The query results.</returns>
        IEnumerator IInternalQuery.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IDbAsyncEnumerable

        /// <summary>
        ///     Returns an <see cref="IDbAsyncEnumerator{TElement}"/> which when enumerated will execute the query against the database.
        /// </summary>
        /// <returns>The query results.</returns>
        public virtual IDbAsyncEnumerator<TElement> GetAsyncEnumerator()
        {
            Contract.Assert(_objectQuery != null, "InternalQuery should have been initialized.");

            InternalContext.Initialize();

            return ((IDbAsyncEnumerable<TElement>)_objectQuery).GetAsyncEnumerator();
        }

        /// <summary>
        ///     Returns an <see cref="IDbAsyncEnumerator"/> which when enumerated will execute the query against the database.
        /// </summary>
        /// <returns>The query results.</returns>
        IDbAsyncEnumerator IInternalQuery.GetAsyncEnumerator()
        {
            return GetAsyncEnumerator();
        }

        #endregion
    }
}
