namespace System.Data.Entity.Internal
{
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Represents a raw SQL query against the context that may be for entities in an entity set
    ///     or for some other non-entity element type.
    /// </summary>
    internal abstract class InternalSqlQuery : IEnumerable, IDbAsyncEnumerable, IListSource
    {
        #region Constructors and fields

        private readonly string _sql;
        private readonly object[] _parameters;

        /// <summary>
        ///     Initializes a new instance of the <see cref = "InternalSqlQuery" /> class.
        /// </summary>
        /// <param name = "sql">The SQL.</param>
        /// <param name = "parameters">The parameters.</param>
        internal InternalSqlQuery(string sql, object[] parameters)
        {
            Contract.Requires(sql != null);
            Contract.Requires(parameters != null);

            _sql = sql;
            _parameters = parameters;
        }

        #endregion

        #region Access to the SQL string and parameters

        /// <summary>
        ///     Gets the SQL query string,
        /// </summary>
        /// <value>The SQL query.</value>
        public string Sql
        {
            get { return _sql; }
        }

        /// <summary>
        ///     Gets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public object[] Parameters
        {
            get { return _parameters; }
        }

        #endregion

        #region AsNoTracking

        /// <summary>
        ///     If the query is tracking entities, then this method returns a new query that will
        ///     not track entities.
        /// </summary>
        /// <returns>A no-tracking query.</returns>
        public abstract InternalSqlQuery AsNoTracking();

        #endregion

        #region IEnumerable implementation

        /// <summary>
        ///     Returns an <see cref="IEnumerator"/> which when enumerated will execute the given SQL query against the database.
        /// </summary>
        /// <returns>The query results.</returns>
        public abstract IEnumerator GetEnumerator();

        #endregion

        #region IDbAsyncEnumerable implementation

        /// <summary>
        ///     Returns an <see cref="IDbAsyncEnumerator"/> which when enumerated will execute the given SQL query against the database.
        /// </summary>
        /// <returns>The query results.</returns>
        public abstract IDbAsyncEnumerator GetAsyncEnumerator();

        #endregion

        #region IListSource implementation

        /// <summary>
        ///     Returns <c>false</c>.
        /// </summary>
        /// <returns><c>false</c>.</returns>
        public bool ContainsListCollection
        {
            get { return false; }
        }

        /// <summary>
        ///     Throws an exception indicating that binding directly to a store query is not supported.
        /// </summary>
        /// <returns>
        ///     Never returns; always throws.
        /// </returns>
        public IList GetList()
        {
            throw Error.DbQuery_BindingToDbQueryNotSupported();
        }

        #endregion

        #region ToString

        /// <summary>
        ///     Returns a <see cref = "System.String" /> that contains the SQL string that was set
        ///     when the query was created.  The parameters are not included.
        /// </summary>
        /// <returns>
        ///     A <see cref = "System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Sql;
        }

        #endregion
    }
}
