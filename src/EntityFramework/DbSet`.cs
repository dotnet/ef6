namespace System.Data.Entity
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.Linq;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     A DbSet represents the collection of all entities in the context, or that can be queried from the
    ///     database, of a given type.  DbSet objects are created from a DbContext using the DbContext.Set method.
    /// </summary>
    /// <remarks>
    ///     Note that DbSet does not support MEST (Multiple Entity Sets per Type) meaning that there is always a
    ///     one-to-one correlation between a type and a set.
    /// </remarks>
    /// <typeparam name = "TEntity">The type that defines the set.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "Name is intentional")]
    public class DbSet<TEntity> : DbQuery<TEntity>, IDbSet<TEntity>, IInternalSetAdapter
        where TEntity : class
    {
        #region Fields and constructors

        private readonly InternalSet<TEntity> _internalSet;

        /// <summary>
        ///     Creates a new set that will be backed by the given <see cref = "InternalSet{T}" />.
        /// </summary>
        /// <param name = "internalSet">The internal set.</param>
        internal DbSet(InternalSet<TEntity> internalSet)
            : base(internalSet)
        {
            Contract.Requires(internalSet != null);

            _internalSet = internalSet;
        }

        #endregion

        #region Find

        /// <summary>
        ///     Finds an entity with the given primary key values.
        ///     If an entity with the given primary key values exists in the context, then it is
        ///     returned immediately without making a request to the store.  Otherwise, a request
        ///     is made to the store for an entity with the given primary key values and this entity,
        ///     if found, is attached to the context and returned.  If no entity is found in the
        ///     context or the store, then null is returned.
        /// </summary>
        /// <remarks>
        ///     The ordering of composite key values is as defined in the EDM, which is in turn as defined in
        ///     the designer, by the Code First fluent API, or by the DataMember attribute.
        /// </remarks>
        /// <param name = "keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>The entity found, or null.</returns>
        /// <exception cref = "InvalidOperationException">Thrown if multiple entities exist in the context with the primary key values given.</exception>
        /// <exception cref = "InvalidOperationException">Thrown if the type of entity is not part of the data model for this context.</exception>
        /// <exception cref = "InvalidOperationException">Thrown if the types of the key values do not match the types of the key values for the entity type to be found.</exception>
        /// <exception cref = "InvalidOperationException">Thrown if the context has been disposed.</exception>
        public TEntity Find(params object[] keyValues)
        {
            return _internalSet.Find(keyValues);
        }

        /// <summary>
        ///     An asynchronous version of Find, which
        ///     finds an entity with the given primary key values.
        ///     If an entity with the given primary key values exists in the context, then it is
        ///     returned immediately without making a request to the store.  Otherwise, a request
        ///     is made to the store for an entity with the given primary key values and this entity,
        ///     if found, is attached to the context and returned.  If no entity is found in the
        ///     context or the store, then null is returned.
        /// </summary>
        /// <remarks>
        ///     The ordering of composite key values is as defined in the EDM, which is in turn as defined in
        ///     the designer, by the Code First fluent API, or by the DataMember attribute.
        /// </remarks>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name = "keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>A Task containing the entity found, or null.</returns>
        /// <exception cref = "InvalidOperationException">Thrown if multiple entities exist in the context with the primary key values given.</exception>
        /// <exception cref = "InvalidOperationException">Thrown if the type of entity is not part of the data model for this context.</exception>
        /// <exception cref = "InvalidOperationException">Thrown if the types of the key values do not match the types of the key values for the entity type to be found.</exception>
        /// <exception cref = "InvalidOperationException">Thrown if the context has been disposed.</exception>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "keyValues")]
        public Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Data binding/local view

        /// <summary>
        ///     Gets an <see cref = "ObservableCollection{T}" /> that represents a local view of all Added, Unchanged,
        ///     and Modified entities in this set.  This local view will stay in sync as entities are added or
        ///     removed from the context.  Likewise, entities added to or removed from the local view will automatically
        ///     be added to or removed from the context.
        /// </summary>
        /// <remarks>
        ///     This property can be used for data binding by populating the set with data, for example by using the Load
        ///     extension method, and then binding to the local data through this property.  For WPF bind to this property
        ///     directly.  For Windows Forms bind to the result of calling ToBindingList on this property
        /// </remarks>
        /// <value>The local view.</value>
        public ObservableCollection<TEntity> Local
        {
            get { return _internalSet.Local; }
        }

        #endregion

        #region Attach/Add/Remove

        /// <summary>
        ///     Attaches the given entity to the context underlying the set.  That is, the entity is placed
        ///     into the context in the Unchanged state, just as if it had been read from the database.
        /// </summary>
        /// <param name = "entity">The entity to attach.</param>
        /// <returns>The entity.</returns>
        /// <remarks>
        ///     Attach is used to repopulate a context with an entity that is known to already exist in the database.
        ///     SaveChanges will therefore not attempt to insert an attached entity into the database because
        ///     it is assumed to already be there.
        ///     Note that entities that are already in the context in some other state will have their state set
        ///     to Unchanged.  Attach is a no-op if the entity is already in the context in the Unchanged state.
        /// </remarks>
        public TEntity Attach(TEntity entity)
        {
            _internalSet.Attach(entity);
            return entity;
        }

        /// <summary>
        ///     Adds the given entity to the context underlying the set in the Added state such that it will
        ///     be inserted into the database when SaveChanges is called.
        /// </summary>
        /// <param name = "entity">The entity to add.</param>
        /// <returns>The entity.</returns>
        /// <remarks>
        ///     Note that entities that are already in the context in some other state will have their state set
        ///     to Added.  Add is a no-op if the entity is already in the context in the Added state.
        /// </remarks>
        public TEntity Add(TEntity entity)
        {
            _internalSet.Add(entity);
            return entity;
        }

        /// <summary>
        ///     Marks the given entity as Deleted such that it will be deleted from the database when SaveChanges
        ///     is called.  Note that the entity must exist in the context in some other state before this method
        ///     is called.
        /// </summary>
        /// <param name = "entity">The entity to remove.</param>
        /// <returns>The entity.</returns>
        /// <remarks>
        ///     Note that if the entity exists in the context in the Added state, then this method
        ///     will cause it to be detached from the context.  This is because an Added entity is assumed not to
        ///     exist in the database such that trying to delete it does not make sense.
        /// </remarks>
        public TEntity Remove(TEntity entity)
        {
            _internalSet.Remove(entity);
            return entity;
        }

        #endregion

        #region Create

        /// <summary>
        ///     Creates a new instance of an entity for the type of this set.
        ///     Note that this instance is NOT added or attached to the set.
        ///     The instance returned will be a proxy if the underlying context is configured to create
        ///     proxies and the entity type meets the requirements for creating a proxy.
        /// </summary>
        /// <returns>The entity instance, which may be a proxy.</returns>
        public TEntity Create()
        {
            return _internalSet.Create();
        }

        /// <summary>
        ///     Creates a new instance of an entity for the type of this set or for a type derived
        ///     from the type of this set.
        ///     Note that this instance is NOT added or attached to the set.
        ///     The instance returned will be a proxy if the underlying context is configured to create
        ///     proxies and the entity type meets the requirements for creating a proxy.
        /// </summary>
        /// <typeparam name = "TDerivedEntity">The type of entity to create.</typeparam>
        /// <returns> The entity instance, which may be a proxy. </returns>
        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, TEntity
        {
            return (TDerivedEntity)_internalSet.Create(typeof(TDerivedEntity));
        }

        #endregion

        #region Conversion to non-generic

        /// <summary>
        ///     Returns the equivalent non-generic <see cref = "DbSet" /> object.
        /// </summary>
        /// <returns>The non-generic set object.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates",
            Justification = "Intentionally just implicit to reduce API clutter.")]
        public static implicit operator DbSet(DbSet<TEntity> entry)
        {
            Contract.Requires(entry != null);

            return (DbSet)entry._internalSet.InternalContext.Set(entry._internalSet.ElementType);
        }

        #endregion

        #region IInternalSetAdapter

        /// <summary>
        ///     The internal IQueryable that is backing this DbQuery
        /// </summary>
        IInternalSet IInternalSetAdapter.InternalSet
        {
            get { return _internalSet; }
        }

        #endregion

        #region SQL queries

        /// <summary>
        ///     Creates a raw SQL query that will return entities in this set.  By default, the
        ///     entities returned are tracked by the context; this can be changed by calling
        ///     AsNoTracking on the <see cref = "DbSqlQuery{TEntity}" /> returned.
        ///     Note that the entities returned are always of the type for this set and never of
        ///     a derived type.  If the table or tables queried may contain data for other entity
        ///     types, then the SQL query must be written appropriately to ensure that only entities of
        ///     the correct type are returned.
        /// </summary>
        /// <param name = "sql">The SQL query string.</param>
        /// <param name = "parameters">The parameters to apply to the SQL query string.</param>
        /// <returns>A <see cref = "DbSqlQuery{TEntity}" /> object that will execute the query when it is enumerated.</returns>
        public DbSqlQuery<TEntity> SqlQuery(string sql, params object[] parameters)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));
            Contract.Requires(parameters != null);

            return new DbSqlQuery<TEntity>(new InternalSqlSetQuery(_internalSet, sql, false, parameters));
        }

        #endregion

        #region Hidden Object methods

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }

        #endregion
    }
}
