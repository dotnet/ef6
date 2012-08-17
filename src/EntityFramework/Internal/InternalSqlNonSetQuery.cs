// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Represents a raw SQL query against the context for any type where the results are never
    ///     associated with an entity set and are never tracked.
    /// </summary>
    internal class InternalSqlNonSetQuery : InternalSqlQuery
    {
        #region Constructors and fields

        private readonly InternalContext _internalContext;
        private readonly Type _elementType;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InternalSqlNonSetQuery" /> class.
        /// </summary>
        /// <param name="internalContext"> The internal context. </param>
        /// <param name="elementType"> Type of the element. </param>
        /// <param name="sql"> The SQL. </param>
        /// <param name="parameters"> The parameters. </param>
        internal InternalSqlNonSetQuery(
            InternalContext internalContext, Type elementType, string sql, object[] parameters)
            : base(sql, parameters)
        {
            Contract.Requires(internalContext != null);
            Contract.Requires(elementType != null);

            _internalContext = internalContext;
            _elementType = elementType;
        }

        #endregion

        #region AsNoTracking

        /// <summary>
        ///     Returns this query since it can never be a tracking query.
        /// </summary>
        /// <returns> This instance. </returns>
        public override InternalSqlQuery AsNoTracking()
        {
            return this;
        }

        #endregion

        #region IEnumerable implementation

        /// <summary>
        ///     Returns an <see cref="IEnumerator" /> which when enumerated will execute the given SQL query against the
        ///     database backing this context. The results are not materialized as entities or tracked.
        /// </summary>
        /// <returns> The query results. </returns>
        public override IEnumerator GetEnumerator()
        {
            return _internalContext.ExecuteSqlQuery(_elementType, Sql, Parameters);
        }

        #endregion

        #region IDbAsyncEnumerable implementation

#if !NET40

        /// <summary>
        ///     Returns an <see cref="IDbAsyncEnumerator" /> which when enumerated will execute the given SQL query against the
        ///     database backing this context. The results are not materialized as entities or tracked.
        /// </summary>
        /// <returns> The query results. </returns>
        public override IDbAsyncEnumerator GetAsyncEnumerator()
        {
            return _internalContext.ExecuteSqlQueryAsync(_elementType, Sql, Parameters);
        }

#endif

        #endregion
    }
}
