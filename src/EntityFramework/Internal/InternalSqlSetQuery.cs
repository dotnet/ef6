// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal.Linq;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Represents a raw SQL query against the context for entities in an entity set.
    /// </summary>
    internal class InternalSqlSetQuery : InternalSqlQuery
    {
        #region Constructors and fields

        private readonly IInternalSet _set;
        private readonly bool _isNoTracking;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InternalSqlSetQuery" /> class.
        /// </summary>
        /// <param name="set"> The set. </param>
        /// <param name="sql"> The SQL. </param>
        /// <param name="isNoTracking"> if set to <c>true</c> then the entities will not be tracked. </param>
        /// <param name="parameters"> The parameters. </param>
        internal InternalSqlSetQuery(IInternalSet set, string sql, bool isNoTracking, object[] parameters)
            : base(sql, parameters)
        {
            Contract.Requires(set != null);

            _set = set;
            _isNoTracking = isNoTracking;
        }

        #endregion

        #region AsNoTracking

        /// <summary>
        ///     If the query is would track entities, then this method returns a new query that will
        ///     not track entities.
        /// </summary>
        /// <returns> A no-tracking query. </returns>
        public override InternalSqlQuery AsNoTracking()
        {
            return new InternalSqlSetQuery(_set, Sql, isNoTracking: true, parameters: Parameters);
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is set to track entities or not.
        /// </summary>
        /// <value> <c>true</c> if this instance is no-tracking; otherwise, <c>false</c> . </value>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public bool IsNoTracking
        {
            get { return _isNoTracking; }
        }

        #endregion

        #region IEnumerable implementation

        /// <summary>
        ///     Returns an <see cref="IEnumerator" /> which when enumerated will execute the given SQL query against the database
        ///     materializing entities into the entity set that backs this set.
        /// </summary>
        /// <returns> The query results. </returns>
        public override IEnumerator GetEnumerator()
        {
            return _set.ExecuteSqlQuery(Sql, _isNoTracking, Parameters);
        }

        #endregion

        #region IDbAsyncEnumerable implementation

        /// <summary>
        ///     Returns an <see cref="IDbAsyncEnumerator" /> which when enumerated will execute the given SQL query against the database
        ///     materializing entities into the entity set that backs this set.
        /// </summary>
        /// <returns> The query results. </returns>
        public override IDbAsyncEnumerator GetAsyncEnumerator()
        {
            return _set.ExecuteSqlQueryAsync(Sql, _isNoTracking, Parameters);
        }

        #endregion
    }
}
