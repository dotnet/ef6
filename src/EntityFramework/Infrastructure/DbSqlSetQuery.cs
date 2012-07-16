namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Internal;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Represents a SQL query for entities that is created from a <see cref = "DbContext" /> 
    ///     and is executed using the connection from that context.
    ///     Instances of this class are obtained from the <see cref = "DbSet" /> instance for the 
    ///     entity type. The query is not executed when this object is created; it is executed
    ///     each time it is enumerated, for example by using foreach.
    ///     SQL queries for non-entities are created using the <see cref = "DbContext.Database" />.
    ///     See <see cref = "DbSqlQuery{TEntity}" /> for a generic version of this class.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    public class DbSqlSetQuery : DbSqlQuery
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref = "DbSqlSetQuery" /> class.
        /// </summary>
        /// <param name = "internalQuery">The internal query.</param>
        internal DbSqlSetQuery(InternalSqlQuery internalQuery)
            : base(internalQuery)
        {
        }

        #region AsNoTracking

        /// <summary>
        ///     Returns a new query where the results of the query will not be tracked by the associated
        ///     <see cref = "DbContext" />.
        /// </summary>
        /// <returns>A new query with no-tracking applied.</returns>
        public DbSqlSetQuery AsNoTracking()
        {
            return new DbSqlSetQuery(InternalQuery.AsNoTracking());
        }

        #endregion
    }
}
