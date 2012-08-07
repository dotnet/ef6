// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     An IDbSet represents the collection of all entities in the context, or that can be queried from the
    ///     database, of a given type.  DbSet is a concrete implementation of IDbSet.
    /// </summary>
    /// <typeparam name="TEntity"> The type that defines the set. </typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "Name is intentional")]
    [ContractClass(typeof(IDbSetContracts<>))]
    public interface IDbSet<TEntity> : IQueryable<TEntity>
        where TEntity : class
    {
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
        TEntity Find(params object[] keyValues);

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
        /// <param name="cancellationToken"> The token to monitor for cancellation requests. </param>
        /// <param name="keyValues"> The values of the primary key for the entity to be found. </param>
        /// <returns> A Task containing the entity found, or null. </returns>
        Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues);

        /// <summary>
        ///     Adds the given entity to the context underlying the set in the Added state such that it will
        ///     be inserted into the database when SaveChanges is called.
        /// </summary>
        /// <param name="entity"> The entity to add. </param>
        /// <returns> The entity. </returns>
        /// <remarks>
        ///     Note that entities that are already in the context in some other state will have their state set
        ///     to Added.  Add is a no-op if the entity is already in the context in the Added state.
        /// </remarks>
        TEntity Add(TEntity entity);

        /// <summary>
        ///     Marks the given entity as Deleted such that it will be deleted from the database when SaveChanges
        ///     is called.  Note that the entity must exist in the context in some other state before this method
        ///     is called.
        /// </summary>
        /// <param name="entity"> The entity to remove. </param>
        /// <returns> The entity. </returns>
        /// <remarks>
        ///     Note that if the entity exists in the context in the Added state, then this method
        ///     will cause it to be detached from the context.  This is because an Added entity is assumed not to
        ///     exist in the database such that trying to delete it does not make sense.
        /// </remarks>
        TEntity Remove(TEntity entity);

        /// <summary>
        ///     Attaches the given entity to the context underlying the set.  That is, the entity is placed
        ///     into the context in the Unchanged state, just as if it had been read from the database.
        /// </summary>
        /// <param name="entity"> The entity to attach. </param>
        /// <returns> The entity. </returns>
        /// <remarks>
        ///     Attach is used to repopulate a context with an entity that is known to already exist in the database.
        ///     SaveChanges will therefore not attempt to insert an attached entity into the database because
        ///     it is assumed to already be there.
        ///     Note that entities that are already in the context in some other state will have their state set
        ///     to Unchanged.  Attach is a no-op if the entity is already in the context in the Unchanged state.
        /// </remarks>
        TEntity Attach(TEntity entity);

        /// <summary>
        ///     Gets an <see cref="ObservableCollection{T}" /> that represents a local view of all Added, Unchanged,
        ///     and Modified entities in this set.  This local view will stay in sync as entities are added or
        ///     removed from the context.  Likewise, entities added to or removed from the local view will automatically
        ///     be added to or removed from the context.
        /// </summary>
        /// <remarks>
        ///     This property can be used for data binding by populating the set with data, for example by using the Load
        ///     extension method, and then binding to the local data through this property.  For WPF bind to this property
        ///     directly.  For Windows Forms bind to the result of calling ToBindingList on this property
        /// </remarks>
        /// <value> The local view. </value>
        ObservableCollection<TEntity> Local { get; }

        /// <summary>
        ///     Creates a new instance of an entity for the type of this set.
        ///     Note that this instance is NOT added or attached to the set.
        ///     The instance returned will be a proxy if the underlying context is configured to create
        ///     proxies and the entity type meets the requirements for creating a proxy.
        /// </summary>
        /// <returns> The entity instance, which may be a proxy. </returns>
        TEntity Create();

        /// <summary>
        ///     Creates a new instance of an entity for the type of this set or for a type derived
        ///     from the type of this set.
        ///     Note that this instance is NOT added or attached to the set.
        ///     The instance returned will be a proxy if the underlying context is configured to create
        ///     proxies and the entity type meets the requirements for creating a proxy.
        /// </summary>
        /// <typeparam name="TDerivedEntity"> The type of entity to create. </typeparam>
        /// <returns> The entity instance, which may be a proxy. </returns>
        TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, TEntity;
    }

    #region Interface Member Contracts

    [ContractClassFor(typeof(IDbSet<>))]
    internal abstract class IDbSetContracts<TEntity> : IDbSet<TEntity>
        where TEntity : class
    {
        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        Expression IQueryable.Expression
        {
            get { throw new NotImplementedException(); }
        }

        Type IQueryable.ElementType
        {
            get { throw new NotImplementedException(); }
        }

        IQueryProvider IQueryable.Provider
        {
            get { throw new NotImplementedException(); }
        }

        TEntity IDbSet<TEntity>.Find(params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        Task<TEntity> IDbSet<TEntity>.FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        TEntity IDbSet<TEntity>.Add(TEntity entity)
        {
            Contract.Requires(entity != null);

            throw new NotImplementedException();
        }

        TEntity IDbSet<TEntity>.Remove(TEntity entity)
        {
            Contract.Requires(entity != null);

            throw new NotImplementedException();
        }

        TEntity IDbSet<TEntity>.Attach(TEntity entity)
        {
            Contract.Requires(entity != null);

            throw new NotImplementedException();
        }

        ObservableCollection<TEntity> IDbSet<TEntity>.Local
        {
            get { throw new NotImplementedException(); }
        }

        TEntity IDbSet<TEntity>.Create()
        {
            throw new NotImplementedException();
        }

        TDerivedEntity IDbSet<TEntity>.Create<TDerivedEntity>()
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
