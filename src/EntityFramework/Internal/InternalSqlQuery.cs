// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Represents a raw SQL query against the context that may be for entities in an entity set
    /// or for some other non-entity element type.
    /// </summary>
    internal abstract class InternalSqlQuery : IEnumerable
#if !NET40
                                               , IDbAsyncEnumerable
#endif
    {
        #region Constructors and fields

        private readonly string _sql;
        private readonly object[] _parameters;
        private readonly bool? _streaming;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalSqlQuery" /> class.
        /// </summary>
        /// <param name="sql"> The SQL. </param>
        /// <param name="streaming"> Whether the query is streaming or buffering. </param>
        /// <param name="parameters"> The parameters. </param>
        internal InternalSqlQuery(string sql, bool? streaming, object[] parameters)
        {
            DebugCheck.NotNull(sql);
            DebugCheck.NotNull(parameters);

            _sql = sql;
            _parameters = parameters;
            _streaming = streaming;
        }

        #endregion

        #region Access to the SQL string and parameters

        /// <summary>
        /// Gets the SQL query string,
        /// </summary>
        /// <value> The SQL query. </value>
        public string Sql
        {
            get { return _sql; }
        }

        /// <summary>
        /// Get the query streaming behavior.
        /// </summary>
        /// <value>
        /// <c>true</c> if the query is streaming;
        /// <c>false</c> if the query is buffering
        /// </value>
        internal bool? Streaming
        {
            get { return _streaming; }
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value> The parameters. </value>
        public object[] Parameters
        {
            get { return _parameters; }
        }

        #endregion

        #region AsNoTracking

        /// <summary>
        /// If the query is tracking entities, then this method returns a new query that will
        /// not track entities.
        /// </summary>
        /// <returns> A no-tracking query. </returns>
        public abstract InternalSqlQuery AsNoTracking();

        #endregion

        #region AsStreaming

        /// <summary>
        /// If the query is buffering, then this method returns a new query that will stream
        /// the results instead.
        /// </summary>
        /// <returns> A streaming query. </returns>
        public abstract InternalSqlQuery AsStreaming();

        #endregion

        #region IEnumerable implementation

        /// <summary>
        /// Returns an <see cref="IEnumerator" /> which when enumerated will execute the given SQL query against the database.
        /// </summary>
        /// <returns> The query results. </returns>
        public abstract IEnumerator GetEnumerator();

        #endregion

        #region IDbAsyncEnumerable implementation

#if !NET40
        /// <summary>
        /// Returns an <see cref="IDbAsyncEnumerator" /> which when enumerated will execute the given SQL query against the database.
        /// </summary>
        /// <returns> The query results. </returns>
        public abstract IDbAsyncEnumerator GetAsyncEnumerator();

#endif

        #endregion

        #region ToString

        /// <summary>
        /// Returns a <see cref="System.String" /> that contains the SQL string that was set
        /// when the query was created.  The parameters are not included.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Sql;
        }

        #endregion
    }
}
