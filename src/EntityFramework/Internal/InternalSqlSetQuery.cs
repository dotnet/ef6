// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal.Linq;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    // <summary>
    // Represents a raw SQL query against the context for entities in an entity set.
    // </summary>
    internal class InternalSqlSetQuery : InternalSqlQuery
    {
        #region Constructors and fields

        private readonly IInternalSet _set;
        private readonly bool _isNoTracking;

        // <summary>
        // Initializes a new instance of the <see cref="InternalSqlSetQuery" /> class.
        // </summary>
        // <param name="set"> The set. </param>
        // <param name="sql"> The SQL. </param>
        // <param name="isNoTracking">
        // If set to <c>true</c> then the entities will not be tracked.
        // </param>
        // <param name="parameters"> The parameters. </param>
        internal InternalSqlSetQuery(IInternalSet set, string sql, bool isNoTracking, object[] parameters) : this(set, sql, isNoTracking, /*streaming:*/ null, parameters) {}

        private InternalSqlSetQuery(IInternalSet set, string sql, bool isNoTracking, bool? streaming, object[] parameters)
            : base(sql, streaming, parameters)
        {
            DebugCheck.NotNull(set);

            _set = set;
            _isNoTracking = isNoTracking;
        }

        #endregion

        #region AsNoTracking

        // <inheritdoc />
        public override InternalSqlQuery AsNoTracking()
        {
            return _isNoTracking
                       ? this
                       : new InternalSqlSetQuery(_set, Sql, isNoTracking: true, streaming: Streaming, parameters: Parameters);
        }

        // <summary>
        // Gets a value indicating whether this instance is set to track entities or not.
        // </summary>
        // <value>
        // <c>true</c> if this instance is no-tracking; otherwise, <c>false</c> .
        // </value>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public bool IsNoTracking
        {
            get { return _isNoTracking; }
        }

        #endregion

        #region AsStreaming

        // <inheritdoc />
        public override InternalSqlQuery AsStreaming()
        {
            return Streaming.HasValue && Streaming.Value
                       ? this
                       : new InternalSqlSetQuery(_set, Sql, isNoTracking: _isNoTracking, streaming: true, parameters: Parameters);
        }

        #endregion

        #region IEnumerable implementation

        // <summary>
        // Returns an <see cref="IEnumerator" /> which when enumerated will execute the given SQL query against the database
        // materializing entities into the entity set that backs this set.
        // </summary>
        // <returns> The query results. </returns>
        public override IEnumerator GetEnumerator()
        {
            return _set.ExecuteSqlQuery(Sql, _isNoTracking, Streaming, Parameters);
        }

        #endregion

        #region IDbAsyncEnumerable implementation

#if !NET40

        // <summary>
        // Returns an <see cref="IDbAsyncEnumerator" /> which when enumerated will execute the given SQL query against the database
        // materializing entities into the entity set that backs this set.
        // </summary>
        // <returns> The query results. </returns>
        public override IDbAsyncEnumerator GetAsyncEnumerator()
        {
            return _set.ExecuteSqlQueryAsync(Sql, _isNoTracking, Streaming, Parameters);
        }

#endif

        #endregion
    }
}
