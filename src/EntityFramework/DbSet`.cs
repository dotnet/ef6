// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.Linq;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
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
    /// <typeparam name="TEntity"> The type that defines the set. </typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "Name is intentional")]
    public class DbSet<TEntity> : DbQuery<TEntity>, IDbSet<TEntity>, IInternalSetAdapter
        where TEntity : class
    {
        #region Fields and constructors

        private readonly InternalSet<TEntity> _internalSet;

        /// <summary>
        ///     Creates a new set that will be backed by the given <see cref="InternalSet{T}" />.
        /// </summary>
        /// <param name="internalSet"> The internal set. </param>
        internal DbSet(InternalSet<TEntity> internalSet)
            : base(internalSet)
        {
            DebugCheck.NotNull(internalSet);

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
        /// <param name="keyValues"> The values of the primary key for the entity to be found. </param>
        /// <returns> The entity found, or null. </returns>
        /// <exception cref="InvalidOperationException">Thrown if multiple entities exist in the context with the primary key values given.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the type of entity is not part of the data model for this context.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the types of the key values do not match the types of the key values for the entity type to be found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the context has been disposed.</exception>
        public TEntity Find(params object[] keyValues)
        {
            return _internalSet.Find(keyValues);
        }

#if !NET40

        /// <summary>
        ///     Asynchronously finds an entity with the given primary key values.
        ///     If an entity with the given primary key values exists in the context, then it is
        ///     returned immediately without making a request to the store.  Otherwise, a request
        ///     is made to the store for an entity with the given primary key values and this entity,
        ///     if found, is attached to the context and returned.  If no entity is found in the
        ///     context or the store, then null is returned.
        /// </summary>
        /// <remarks>
        ///     The ordering of composite key values is as defined in the EDM, which is in turn as defined in
        ///     the designer, by the Code First fluent API, or by the DataMember attribute.
        ///     Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///     that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <param name="keyValues"> The values of the primary key for the entity to be found. </param>
        /// <returns> A task that represents the asynchronous find operation. The task result contains the entity found, or null. </returns>
        /// <exception cref="InvalidOperationException">Thrown if multiple entities exist in the context with the primary key values given.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the type of entity is not part of the data model for this context.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the types of the key values do not match the types of the key values for the entity type to be found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the context has been disposed.</exception>
        public Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return _internalSet.FindAsync(cancellationToken, keyValues);
        }

#endif

        #endregion

        #region Data binding/local view

        /// <inheritdoc/>
        public DbLocalView<TEntity> Local
        {
            get { return _internalSet.Local; }
        }

        #endregion

        #region Attach/Add/Remove

        /// <inheritdoc/>
        public TEntity Attach(TEntity entity)
        {
            Check.NotNull(entity, "entity");

            _internalSet.Attach(entity);
            return entity;
        }

        /// <inheritdoc/>
        public TEntity Add(TEntity entity)
        {
            Check.NotNull(entity, "entity");

            _internalSet.Add(entity);
            return entity;
        }

        /// <summary>
        ///     Adds the given collection of entities into context underlying the set with each entity being put into
        ///     the Added state such that it will be inserted into the database when SaveChanges is called.
        /// </summary>
        /// <param name="entities">The collection of entities to add.</param>
        /// <returns>
        ///     The collection of entities.
        /// </returns>
        /// <remarks>
        ///     Note that if <see cref="DbContextConfiguration.AutoDetectChangesEnabled" /> is set to true (which is
        ///     the default), then DetectChanges will be called once before adding any entities and will not be called
        ///     again. This means that in some situations AddRange may perform significantly better than calling
        ///     Add multiple times would do.
        ///     Note that entities that are already in the context in some other state will have their state set to
        ///     Added.  AddRange is a no-op for entities that are already in the context in the Added state.
        /// </remarks>
        public IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities)
        {
            Check.NotNull(entities, "entities");

            _internalSet.AddRange(entities);
            return entities;
        }

        /// <inheritdoc/>
        public TEntity Remove(TEntity entity)
        {
            Check.NotNull(entity, "entity");

            _internalSet.Remove(entity);
            return entity;
        }

        /// <summary>
        ///     Remove the given collection of entities from context underlying the set with each entity being put into
        ///     the Deleted state such that it will be deleted from the database when SaveChanges is called.
        /// </summary>
        /// <param name="entities">The collection of entities to delete.</param>
        /// <returns>
        ///     The collection of entities.
        /// </returns>
        /// <remarks>
        ///     Note that if <see cref="DbContextConfiguration.AutoDetectChangesEnabled" /> is set to true (which is
        ///     the default), then DetectChanges will be called once before delete any entities and will not be called
        ///     again. This means that in some situations RemoveRange may perform significantly better than calling
        ///     Remove multiple times would do.
        ///     Note that if any entity exists in the context in the Added state, then this method
        ///     will cause it to be detached from the context.  This is because an Added entity is assumed not to
        ///     exist in the database such that trying to delete it does not make sense.
        /// </remarks>
        public IEnumerable<TEntity> RemoveRange(IEnumerable<TEntity> entities)
        {
            Check.NotNull(entities, "entities");

            _internalSet.RemoveRange(entities);
            return entities;
        }

        #endregion

        #region Create

        /// <inheritdoc/>
        public TEntity Create()
        {
            return _internalSet.Create();
        }
        /// <inheritdoc/>
        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, TEntity
        {
            return (TDerivedEntity)_internalSet.Create(typeof(TDerivedEntity));
        }

        #endregion

        #region Conversion to non-generic

        /// <summary>
        ///     Returns the equivalent non-generic <see cref="DbSet" /> object.
        /// </summary>
        /// <returns> The non-generic set object. </returns>
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates",
            Justification = "Intentionally just implicit to reduce API clutter.")]
        public static implicit operator DbSet(DbSet<TEntity> entry)
        {
            Check.NotNull(entry, "entry");

            return (DbSet)entry._internalSet.InternalContext.Set(entry._internalSet.ElementType);
        }

        #endregion

        #region IInternalSetAdapter

        /// <summary>
        ///     Gets the underlying internal set.
        /// </summary>
        /// <value> The internal set. </value>
        IInternalSet IInternalSetAdapter.InternalSet
        {
            get { return _internalSet; }
        }

        #endregion

        #region SQL queries

        /// <summary>
        ///     Creates a raw SQL query that will return entities in this set.  By default, the
        ///     entities returned are tracked by the context; this can be changed by calling
        ///     AsNoTracking on the <see cref="DbSqlQuery{TEntity}" /> returned.
        ///     Note that the entities returned are always of the type for this set and never of
        ///     a derived type.  If the table or tables queried may contain data for other entity
        ///     types, then the SQL query must be written appropriately to ensure that only entities of
        ///     the correct type are returned.
        /// </summary>
        /// <param name="sql"> The SQL query string. </param>
        /// <param name="parameters"> The parameters to apply to the SQL query string. </param>
        /// <returns>
        ///     A <see cref="DbSqlQuery{TEntity}" /> object that will execute the query when it is enumerated.
        /// </returns>
        public DbSqlQuery<TEntity> SqlQuery(string sql, params object[] parameters)
        {
            Check.NotEmpty(sql, "sql");
            Check.NotNull(parameters, "parameters");

            return
                new DbSqlQuery<TEntity>(
                    new InternalSqlSetQuery(_internalSet, sql, /*isNoTracking:*/ false, /*streaming:*/ false, parameters));
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
