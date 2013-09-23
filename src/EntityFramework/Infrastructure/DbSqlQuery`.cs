// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.ComponentModel;
    using System.Data.Entity.Internal;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a SQL query for entities that is created from a <see cref="DbContext" />
    /// and is executed using the connection from that context.
    /// Instances of this class are obtained from the <see cref="DbSet{TEntity}" /> instance for the
    /// entity type. The query is not executed when this object is created; it is executed
    /// each time it is enumerated, for example by using foreach.
    /// SQL queries for non-entities are created using <see cref="Database.SqlQuery{TElement}(string, object[])" />.
    /// See <see cref="DbSqlQuery" /> for a non-generic version of this class.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class DbSqlQuery<TEntity> : DbRawSqlQuery<TEntity>
        where TEntity : class
    {
        // <summary>
        // Initializes a new instance of the <see cref="DbSqlQuery" /> class.
        // </summary>
        // <param name="internalQuery"> The internal query. </param>
        internal DbSqlQuery(InternalSqlQuery internalQuery)
            : base(internalQuery)
        {
        }

        /// <summary>
        /// Creates an instance of a <see cref="DbSqlQuery{TEntity}" /> when called from the constructor of a derived
        /// type that will be used as a test double for <see cref="DbSet{TEntity}.SqlQuery"/>. Methods and properties
        /// that will be used by the test double must be implemented by the test double except AsNoTracking and
        /// AsStreaming where the default implementation is a no-op.
        /// </summary>
        protected DbSqlQuery()
            : this(null)
        {
        }

        #region AsNoTracking

        /// <summary>
        /// Returns a new query where the entities returned will not be cached in the <see cref="DbContext" />.
        /// </summary>
        /// <returns> A new query with NoTracking applied. </returns>
        public virtual DbSqlQuery<TEntity> AsNoTracking()
        {
            return InternalQuery == null ? this: new DbSqlQuery<TEntity>(InternalQuery.AsNoTracking());
        }

        #endregion

        #region AsStreaming

        /// <summary>
        /// Returns a new query that will stream the results instead of buffering.
        /// </summary>
        /// <returns> A new query with AsStreaming applied. </returns>
        public new virtual DbSqlQuery<TEntity> AsStreaming()
        {
            return InternalQuery == null ? this : new DbSqlQuery<TEntity>(InternalQuery.AsStreaming());
        }

        #endregion

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
