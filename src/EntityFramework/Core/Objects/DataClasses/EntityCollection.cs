// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Collection of entities modeling a particular EDM construct
    /// which can either be all entities of a particular type or
    /// entities participating in a particular relationship.
    /// </summary>
    /// <typeparam name="TEntity">The type of entities in this collection.</typeparam>
    [Serializable]
    public class EntityCollection<TEntity> : RelatedEnd, ICollection<TEntity>, IListSource
        where TEntity : class
    {
        // ------
        // Fields
        // ------
        // The following field is serialized.  Adding or removing a serialized field is considered
        // a breaking change.  This includes changing the field type or field name of existing
        // serialized fields. If you need to make this kind of change, it may be possible, but it
        // will require some custom serialization/deserialization code.
        // Note that this field should no longer be used directly.  Instead, use the _wrappedRelatedEntities
        // field.  This field is retained only for compatibility with the serialization format introduced in v1.
        private HashSet<TEntity> _relatedEntities;

        [NonSerialized]
        private CollectionChangeEventHandler _onAssociationChangedforObjectView;

        [NonSerialized]
        private Dictionary<TEntity, IEntityWrapper> _wrappedRelatedEntities;

        // ------------
        // Constructors
        // ------------

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.Objects.DataClasses.EntityCollection`1" /> class.
        /// </summary>
        public EntityCollection()
        {
        }

        internal EntityCollection(IEntityWrapper wrappedOwner, RelationshipNavigation navigation, IRelationshipFixer relationshipFixer)
            : base(wrappedOwner, navigation, relationshipFixer)
        {
        }

        // ---------
        // Events
        // ---------

        // <summary>
        // internal Event to notify changes in the collection.
        // </summary>
        // Dev notes -2
        // following statement is valid on current existing CLR: 
        // lets say Customer is an Entity, Array[Customer] is not Array[Entity]; it is not supported
        // to do the work around we have to use a non-Generic interface/class so we can pass the EntityCollection<T>
        // around safely (as RelatedEnd) without losing it.
        // Dev notes -3 
        // this event is only used for internal purposes, to make sure views are updated before we fire public AssociationChanged event
        internal override event CollectionChangeEventHandler AssociationChangedForObjectView
        {
            add { _onAssociationChangedforObjectView += value; }
            remove { _onAssociationChangedforObjectView -= value; }
        }

        // ---------
        // Properties
        // ---------
        private Dictionary<TEntity, IEntityWrapper> WrappedRelatedEntities
        {
            get
            {
                if (null == _wrappedRelatedEntities)
                {
                    _wrappedRelatedEntities = new Dictionary<TEntity, IEntityWrapper>(ObjectReferenceEqualityComparer.Default);
                }
                return _wrappedRelatedEntities;
            }
        }

        // ----------------------
        // ICollection Properties
        // ----------------------

        /// <summary>Gets the number of objects that are contained in the collection.</summary>
        /// <returns>
        /// The number of elements that are contained in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.DataClasses.EntityCollection`1" />
        /// .
        /// </returns>
        public int Count
        {
            get
            {
                DeferredLoad();
                return CountInternal;
            }
        }

        internal int CountInternal
        {
            get
            {
                // count should not cause allocation
                return ((null != _wrappedRelatedEntities) ? _wrappedRelatedEntities.Count : 0);
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.DataClasses.EntityCollection`1" />
        /// is read-only.
        /// </summary>
        /// <returns>Always returns false.</returns>
        public bool IsReadOnly
        {
            get { return false; }
        }

        // ----------------------
        // IListSource  Properties
        // ----------------------
        /// <summary>
        /// IListSource.ContainsListCollection implementation. Always returns false.
        /// This means that the IList we return is the one which contains our actual data,
        /// it is not a list of collections.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        bool IListSource.ContainsListCollection
        {
            get { return false; }
        }

        // -------
        // Methods
        // -------

        internal override void OnAssociationChanged(CollectionChangeAction collectionChangeAction, object entity)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            if (!_suppressEvents)
            {
                if (_onAssociationChangedforObjectView != null)
                {
                    _onAssociationChangedforObjectView(this, (new CollectionChangeEventArgs(collectionChangeAction, entity)));
                }
                if (_onAssociationChanged != null)
                {
                    _onAssociationChanged(this, (new CollectionChangeEventArgs(collectionChangeAction, entity)));
                }
            }
        }

        // ----------------------
        // IListSource  method
        // ----------------------
        /// <summary>
        /// Returns the collection as an <see cref="T:System.Collections.IList" /> used for data binding.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IList" /> of entity objects.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IList IListSource.GetList()
        {
            EntityType rootEntityType = null;
            if (WrappedOwner.Entity != null)
            {
                EntitySet singleEntitySet = null;

                // if the collection is attached, we can use metadata information; otherwise, it is unavailable
                if (null != RelationshipSet)
                {
                    singleEntitySet = ((AssociationSet)RelationshipSet).AssociationSetEnds[ToEndMember.Name].EntitySet;
                    var associationEndType = (EntityType)((RefType)(ToEndMember).TypeUsage.EdmType).ElementType;
                    var entitySetType = singleEntitySet.ElementType;

                    // the type is constrained to be either the entitySet.ElementType or the end member type, whichever is most derived
                    if (associationEndType.IsAssignableFrom(entitySetType))
                    {
                        // entity set exposes a subtype of the association
                        rootEntityType = entitySetType;
                    }
                    else
                    {
                        // use the end type otherwise
                        rootEntityType = associationEndType;
                    }
                }
            }

            return ObjectViewFactory.CreateViewForEntityCollection(rootEntityType, this);
        }

        /// <summary>Loads related objects into the collection, using the specified merge option.</summary>
        /// <param name="mergeOption">
        /// Specifies how the objects in this collection should be merged with the objects that might have been returned from previous queries against the same
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        /// .
        /// </param>
        public override void Load(MergeOption mergeOption)
        {
            CheckOwnerNull();

            //Pass in null to indicate the CreateSourceQuery method should be used.
            Load(null, mergeOption);
            // do not fire the AssociationChanged event here,
            // once it is fired in one level deeper, (at Internal void Load(IEnumerable<T>)), you don't need to add the event at other
            // API that call (Internal void Load(IEnumerable<T>))
        }

#if !NET40

        /// <inheritdoc />
        public override Task LoadAsync(MergeOption mergeOption, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CheckOwnerNull();

            //Pass in null to indicate the CreateSourceQuery method should be used.
            return LoadAsync(null, mergeOption, cancellationToken);
            // do not fire the AssociationChanged event here,
            // once it is fired in one level deeper, (at Internal void Load(IEnumerable<T>)), you don't need to add the event at other
            // API that call (Internal void Load(IEnumerable<T>))
        }

#endif

        /// <summary>Defines relationships between an object and a collection of related objects in an object context.</summary>
        /// <remarks>
        /// Loads related entities into the local collection. If the collection is already filled
        /// or partially filled, merges existing entities with the given entities. The given
        /// entities are not assumed to be the complete set of related entities.
        /// Owner and all entities passed in must be in Unchanged or Modified state. We allow
        /// deleted elements only when the state manager is already tracking the relationship
        /// instance.
        /// </remarks>
        /// <param name="entities">Collection of objects in the object context that are related to the source object.</param>
        /// <exception cref="T:System.ArgumentNullException"> entities  collection is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// The source object or an object in the  entities  collection is null or is not in an
        /// <see
        ///     cref="F:System.Data.Entity.EntityState.Unchanged" />
        /// or <see cref="F:System.Data.Entity.EntityState.Modified" /> state.-or-The relationship cannot be defined based on the EDM metadata. This can occur when the association in the conceptual schema does not support a relationship between the two types.
        /// </exception>
        public void Attach(IEnumerable<TEntity> entities)
        {
            Check.NotNull(entities, "entities");
            CheckOwnerNull();
            IList<IEntityWrapper> wrappedEntities = new List<IEntityWrapper>();
            foreach (var entity in entities)
            {
                wrappedEntities.Add(EntityWrapperFactory.WrapEntityUsingContext(entity, ObjectContext));
            }
            Attach(wrappedEntities, true);
        }

        /// <summary>Defines a relationship between two attached objects in an object context.</summary>
        /// <param name="entity">The object being attached.</param>
        /// <exception cref="T:System.ArgumentNullException">When the  entity  is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// When the  entity  cannot be related to the source object. This can occur when the association in the conceptual schema does not support a relationship between the two types.-or-When either object is null or is not in an
        /// <see
        ///     cref="F:System.Data.Entity.EntityState.Unchanged" />
        /// or <see cref="F:System.Data.Entity.EntityState.Modified" /> state.
        /// </exception>
        public void Attach(TEntity entity)
        {
            Check.NotNull(entity, "entity");
            Attach(new[] { EntityWrapperFactory.WrapEntityUsingContext(entity, ObjectContext) }, false);
        }

        // <summary>
        // Requires: collection is null or contains related entities.
        // Loads related entities into the local collection.
        // </summary>
        // <param name="collection"> If null, retrieves entities from the server through a query; otherwise, loads the given collection </param>
        internal virtual void Load(List<IEntityWrapper> collection, MergeOption mergeOption)
        {
            // Validate that the Load is possible
            bool hasResults;
            var sourceQuery = ValidateLoad<TEntity>(mergeOption, "EntityCollection", out hasResults);

            // we do not want any Add or Remove event to be fired during Merge, we will fire a Refresh event at the end if everything is successful
            _suppressEvents = true;
            try
            {
                if (collection == null)
                {
                    IEnumerable<TEntity> refreshedValues;
                    if (hasResults)
                    {
                        refreshedValues = sourceQuery.Execute(sourceQuery.MergeOption);
                    }
                    else
                    {
                        refreshedValues = Enumerable.Empty<TEntity>();
                    }

                    Merge(refreshedValues, mergeOption, true /*setIsLoaded*/);
                }
                else
                {
                    Merge<TEntity>(collection, mergeOption, true /*setIsLoaded*/);
                }
            }
            finally
            {
                _suppressEvents = false;
            }
            // fire the AssociationChange with Refresh
            OnAssociationChanged(CollectionChangeAction.Refresh, null);
        }

#if !NET40

        internal virtual async Task LoadAsync(List<IEntityWrapper> collection, MergeOption mergeOption, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Validate that the Load is possible
            bool hasResults;
            var sourceQuery = ValidateLoad<TEntity>(mergeOption, "EntityCollection", out hasResults);

            // we do not want any Add or Remove event to be fired during Merge, we will fire a Refresh event at the end if everything is successful
            _suppressEvents = true;
            try
            {
                if (collection == null)
                {
                    IEnumerable<TEntity> refreshedValues;
                    if (hasResults)
                    {
                        var queryResult =
                            await
                            sourceQuery.ExecuteAsync(sourceQuery.MergeOption, cancellationToken).ConfigureAwait(
                                continueOnCapturedContext: false);
                        refreshedValues = await queryResult.ToListAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                    }
                    else
                    {
                        refreshedValues = Enumerable.Empty<TEntity>();
                    }

                    Merge(refreshedValues, mergeOption, true /*setIsLoaded*/);
                }
                else
                {
                    Merge<TEntity>(collection, mergeOption, true /*setIsLoaded*/);
                }
            }
            finally
            {
                _suppressEvents = false;
            }
            // fire the AssociationChange with Refresh
            OnAssociationChanged(CollectionChangeAction.Refresh, null);
        }

#endif

        /// <summary>Adds an object to the collection.</summary>
        /// <param name="item">
        /// An object to add to the collection.  entity  must implement
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.DataClasses.IEntityWithRelationships" />
        /// .
        /// </param>
        /// <exception cref="T:System.ArgumentNullException"> entity  is null.</exception>
        public void Add(TEntity item)
        {
            Check.NotNull(item, "item");

            Add(EntityWrapperFactory.WrapEntityUsingContext(item, ObjectContext));
        }

        // <summary>
        // Add the item to the underlying collection
        // </summary>
        internal override void DisconnectedAdd(IEntityWrapper wrappedEntity)
        {
            DebugCheck.NotNull(wrappedEntity);

            // Validate that the incoming entity is also detached
            if (null != wrappedEntity.Context
                && wrappedEntity.MergeOption != MergeOption.NoTracking)
            {
                throw new InvalidOperationException(Strings.RelatedEnd_UnableToAddEntity);
            }

            VerifyType(wrappedEntity);

            // Add the entity to local collection without doing any fixup
            AddToCache(wrappedEntity, /* applyConstraints */ false);
            OnAssociationChanged(CollectionChangeAction.Add, wrappedEntity.Entity);
        }

        // <summary>
        // Remove the item from the underlying collection
        // </summary>
        internal override bool DisconnectedRemove(IEntityWrapper wrappedEntity)
        {
            DebugCheck.NotNull(wrappedEntity);

            // Validate that the incoming entity is also detached
            if (null != wrappedEntity.Context
                && wrappedEntity.MergeOption != MergeOption.NoTracking)
            {
                throw new InvalidOperationException(Strings.RelatedEnd_UnableToRemoveEntity);
            }

            // Remove the entity to local collection without doing any fixup
            var result = RemoveFromCache(wrappedEntity, /* resetIsLoaded*/ false, /*preserveForeignKey*/ false);
            OnAssociationChanged(CollectionChangeAction.Remove, wrappedEntity.Entity);
            return result;
        }

        /// <summary>Removes an object from the collection and marks the relationship for deletion.</summary>
        /// <returns>true if item was successfully removed; otherwise, false. </returns>
        /// <param name="item">The object to remove from the collection.</param>
        /// <exception cref="T:System.ArgumentNullException"> entity  object is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The  entity  object is not attached to the same object context.-or-The  entity  object does not have a valid relationship manager.</exception>
        public bool Remove(TEntity item)
        {
            Check.NotNull(item, "item");

            DeferredLoad();
            return RemoveInternal(item);
        }

        internal bool RemoveInternal(TEntity entity)
        {
            return Remove(EntityWrapperFactory.WrapEntityUsingContext(entity, ObjectContext), /*preserveForeignKey*/false);
        }

        internal override void Include(bool addRelationshipAsUnchanged, bool doAttach)
        {
            if (null != _wrappedRelatedEntities
                && null != ObjectContext)
            {
                var wrappedRelatedEntities = new List<IEntityWrapper>(_wrappedRelatedEntities.Values);
                foreach (var wrappedEntity in wrappedRelatedEntities)
                {
                    // Sometimes with mixed POCO and IPOCO, you can get different instances of IEntityWrappers stored in the IPOCO related ends
                    // These should be replaced by the IEntityWrapper that is stored in the context
                    var identityWrapper = EntityWrapperFactory.WrapEntityUsingContext(wrappedEntity.Entity, WrappedOwner.Context);
                    if (identityWrapper != wrappedEntity)
                    {
                        _wrappedRelatedEntities[(TEntity)identityWrapper.Entity] = identityWrapper;
                    }
                    IncludeEntity(identityWrapper, addRelationshipAsUnchanged, doAttach);
                }
            }
        }

        internal override void Exclude()
        {
            if (null != _wrappedRelatedEntities
                && null != ObjectContext)
            {
                if (!IsForeignKey)
                {
                    foreach (var wrappedEntity in _wrappedRelatedEntities.Values)
                    {
                        ExcludeEntity(wrappedEntity);
                    }
                }
                else
                {
                    var tm = ObjectContext.ObjectStateManager.TransactionManager;
                    Debug.Assert(
                        tm.IsAddTracking || tm.IsAttachTracking,
                        "Exclude being called while not part of attach/add rollback--PromotedEntityKeyRefs will be null.");
                    var values = new List<IEntityWrapper>(_wrappedRelatedEntities.Values);
                    foreach (var wrappedEntity in values)
                    {
                        var otherEnd = GetOtherEndOfRelationship(wrappedEntity) as EntityReference;
                        Debug.Assert(otherEnd != null, "Other end of FK from a collection should be a reference.");
                        var doFullRemove = tm.PopulatedEntityReferences.Contains(otherEnd);
                        var doRelatedEndRemove = tm.AlignedEntityReferences.Contains(otherEnd);
                        if (doFullRemove || doRelatedEndRemove)
                        {
                            // Remove the related ends and mark the relationship as deleted, but don't propagate the changes to the target entity itself
                            otherEnd.Remove(
                                otherEnd.CachedValue,
                                doFixup: doFullRemove,
                                deleteEntity: false,
                                deleteOwner: false,
                                applyReferentialConstraints: false,
                                preserveForeignKey: true);
                            // Since this has been processed, remove it from the list
                            if (doFullRemove)
                            {
                                tm.PopulatedEntityReferences.Remove(otherEnd);
                            }
                            else
                            {
                                tm.AlignedEntityReferences.Remove(otherEnd);
                            }
                        }
                        else
                        {
                            ExcludeEntity(wrappedEntity);
                        }
                    }
                }
            }
        }

        internal override void ClearCollectionOrRef(IEntityWrapper wrappedEntity, RelationshipNavigation navigation, bool doCascadeDelete)
        {
            if (null != _wrappedRelatedEntities)
            {
                //copy into list because changing collection member is not allowed during enumeration.
                // If possible avoid copying into list.
                var tempCopy = new List<IEntityWrapper>(_wrappedRelatedEntities.Values);
                foreach (var wrappedCurrent in tempCopy)
                {
                    // Following condition checks if we have already visited this graph node. If its true then
                    // we should not do fixup because that would cause circular loop
                    if ((wrappedEntity.Entity == wrappedCurrent.Entity)
                        && (navigation.Equals(RelationshipNavigation)))
                    {
                        Remove(
                            wrappedCurrent, /*fixup*/false, /*deleteEntity*/false, /*deleteOwner*/false, /*applyReferentialConstraints*/
                            false, /*preserveForeignKey*/false);
                    }
                    else
                    {
                        Remove(
                            wrappedCurrent, /*fixup*/true, doCascadeDelete, /*deleteOwner*/false, /*applyReferentialConstraints*/false,
                            /*preserveForeignKey*/false);
                    }
                }
                Debug.Assert(
                    _wrappedRelatedEntities.Count == 0, "After removing all related entities local collection count should be zero");
            }
        }

        internal override void ClearWrappedValues()
        {
            if (_wrappedRelatedEntities != null)
            {
                _wrappedRelatedEntities.Clear();
            }
            if (_relatedEntities != null)
            {
                _relatedEntities.Clear();
            }
        }

        // <returns> True if the verify succeeded, False if the Add should no-op </returns>
        internal override bool VerifyEntityForAdd(IEntityWrapper wrappedEntity, bool relationshipAlreadyExists)
        {
            DebugCheck.NotNull(wrappedEntity);

            if (!relationshipAlreadyExists
                && ContainsEntity(wrappedEntity))
            {
                return false;
            }

            VerifyType(wrappedEntity);

            return true;
        }

        internal override bool CanSetEntityType(IEntityWrapper wrappedEntity)
        {
            DebugCheck.NotNull(wrappedEntity);

            return wrappedEntity.Entity is TEntity;
        }

        internal override void VerifyType(IEntityWrapper wrappedEntity)
        {
            DebugCheck.NotNull(wrappedEntity);

            if (!CanSetEntityType(wrappedEntity))
            {
                throw new InvalidOperationException(
                    Strings.RelatedEnd_InvalidContainedType_Collection(wrappedEntity.Entity.GetType().FullName, typeof(TEntity).FullName));
            }
        }

        // <summary>
        // Remove from the RelatedEnd
        // </summary>
        internal override bool RemoveFromLocalCache(IEntityWrapper wrappedEntity, bool resetIsLoaded, bool preserveForeignKey)
        {
            DebugCheck.NotNull(wrappedEntity);

            if (_wrappedRelatedEntities != null
                && _wrappedRelatedEntities.Remove((TEntity)wrappedEntity.Entity))
            {
                if (resetIsLoaded)
                {
                    _isLoaded = false;
                }
                return true;
            }
            return false;
        }

        // <summary>
        // Remove from the POCO collection
        // </summary>
        internal override bool RemoveFromObjectCache(IEntityWrapper wrappedEntity)
        {
            DebugCheck.NotNull(wrappedEntity);

            // For POCO entities - remove the object from the CLR collection
            if (TargetAccessor.HasProperty) // Null if the navigation does not exist in this direction
            {
                return WrappedOwner.CollectionRemove(this, wrappedEntity.Entity);
            }

            return false;
        }

        internal override void RetrieveReferentialConstraintProperties(
            Dictionary<string, KeyValuePair<object, IntBox>> properties, HashSet<object> visited)
        {
            // Since there are no RI Constraints which has a collection as a To/Child role,
            // this method is no-op.
        }

        internal override bool IsEmpty()
        {
            return _wrappedRelatedEntities == null || (_wrappedRelatedEntities.Count == 0);
        }

        internal override void VerifyMultiplicityConstraintsForAdd(bool applyConstraints)
        {
            // no-op
        }

        // Update IsLoaded flag if necessary
        // This method is called when Clear() was called on the other end of relationship (if the other end is EntityCollection)
        // or when Value property of the other end was set to null (if the other end is EntityReference).
        // This method is used only when NoTracking option was used.
        internal override void OnRelatedEndClear()
        {
            // If other end of relationship was cleared, it means that this collection is also no longer loaded
            _isLoaded = false;
        }

        internal override bool ContainsEntity(IEntityWrapper wrappedEntity)
        {
            DebugCheck.NotNull(wrappedEntity);

            // Using operator 'as' instead of () allows calling ContainsEntity
            // with entity of different type than TEntity.
            var entity = wrappedEntity.Entity as TEntity;
            return _wrappedRelatedEntities == null ? false : _wrappedRelatedEntities.ContainsKey(entity);
        }

        // -------------------
        // ICollection Methods
        // -------------------

        /// <summary>Returns an enumerator that is used to iterate through the objects in the collection.</summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> that iterates through the set of values cached by
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.DataClasses.EntityCollection`1" />
        /// .
        /// </returns>
        public new IEnumerator<TEntity> GetEnumerator()
        {
            DeferredLoad();
            return WrappedRelatedEntities.Keys.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that is used to iterate through the set of values cached by
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.DataClasses.EntityCollection`1" />
        /// .
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> that iterates through the set of values cached by
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.DataClasses.EntityCollection`1" />
        /// .
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            DeferredLoad();
            return WrappedRelatedEntities.Keys.GetEnumerator();
        }

        internal override IEnumerable GetInternalEnumerable()
        {
            return WrappedRelatedEntities.Keys;
        }

        internal override IEnumerable<IEntityWrapper> GetWrappedEntities()
        {
            return WrappedRelatedEntities.Values;
        }

        /// <summary>Removes all entities from the collection. </summary>
        public void Clear()
        {
            DeferredLoad();
            if (WrappedOwner.Entity != null)
            {
                var shouldFireEvent = (CountInternal > 0);
                if (null != _wrappedRelatedEntities)
                {
                    var affectedEntities = new List<IEntityWrapper>(_wrappedRelatedEntities.Values);

                    try
                    {
                        _suppressEvents = true;

                        foreach (var wrappedEntity in affectedEntities)
                        {
                            // Remove Entity
                            Remove(wrappedEntity, false);

                            if (UsingNoTracking)
                            {
                                // The other end of relationship can be the EntityReference or EntityCollection
                                // If the other end is EntityReference, its IsLoaded property should be set to FALSE
                                var relatedEnd = GetOtherEndOfRelationship(wrappedEntity);
                                relatedEnd.OnRelatedEndClear();
                            }
                        }
                        Debug.Assert(_wrappedRelatedEntities.Count == 0);
                    }
                    finally
                    {
                        _suppressEvents = false;
                    }

                    if (UsingNoTracking)
                    {
                        _isLoaded = false;
                    }
                }

                if (shouldFireEvent)
                {
                    OnAssociationChanged(CollectionChangeAction.Refresh, null);
                }
            }
            else
            {
                // Disconnected Clear should be dispatched to the internal collection
                if (_wrappedRelatedEntities != null)
                {
                    _wrappedRelatedEntities.Clear();
                }
            }
        }

        /// <summary>Determines whether a specific object exists in the collection.</summary>
        /// <returns>
        /// true if the object is found in the <see cref="T:System.Data.Entity.Core.Objects.DataClasses.EntityCollection`1" />; otherwise, false.
        /// </returns>
        /// <param name="item">
        /// The object to locate in the <see cref="T:System.Data.Entity.Core.Objects.DataClasses.EntityCollection`1" />.
        /// </param>
        public bool Contains(TEntity item)
        {
            DeferredLoad();
            return _wrappedRelatedEntities == null ? false : _wrappedRelatedEntities.ContainsKey(item);
        }

        /// <summary>Copies all the contents of the collection to an array, starting at the specified index of the target array.</summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
        public void CopyTo(TEntity[] array, int arrayIndex)
        {
            DeferredLoad();
            WrappedRelatedEntities.Keys.CopyTo(array, arrayIndex);
        }

        internal virtual void BulkDeleteAll(List<object> list)
        {
            if (list.Count > 0)
            {
                _suppressEvents = true;
                try
                {
                    foreach (var entity in list)
                    {
                        // Remove Entity
                        RemoveInternal(entity as TEntity);
                    }
                }
                finally
                {
                    _suppressEvents = false;
                }
                OnAssociationChanged(CollectionChangeAction.Refresh, null);
            }
        }

        internal override bool CheckIfNavigationPropertyContainsEntity(IEntityWrapper wrapper)
        {
            Debug.Assert(RelationshipNavigation != null, "null RelationshipNavigation");

            // If the navigation property doesn't exist (e.g. unidirectional prop), then it can't contain the entity.
            if (!TargetAccessor.HasProperty)
            {
                return false;
            }

            var value = WrappedOwner.GetNavigationPropertyValue(this);

            if (value != null)
            {
                // It would be good to be able to always use ICollection<T>.Contains here. The problem
                // is if the entity has overridden Equals/GetHashcode such that it makes use of the
                // primary key value then this will break when an Added object with an Identity key that
                // is contained in a navigation collection has its primary key set after it is saved.
                // Therefore, we only use this optimization if we know for sure that the nav prop is
                // using reference equality or if neither Equals or GetHashCode are overridden.

                var collection = value as ICollection<TEntity>;
                if (collection == null)
                {
                    throw new EntityException(
                        Strings.ObjectStateEntry_UnableToEnumerateCollection(
                            TargetAccessor.PropertyName, WrappedOwner.Entity.GetType().FullName));
                }

                var hashSet = value as HashSet<TEntity>;
                if (!wrapper.OverridesEqualsOrGetHashCode
                    || (hashSet != null
                        && hashSet.Comparer is ObjectReferenceEqualityComparer))
                {
                    return collection.Contains((TEntity)wrapper.Entity);
                }

                return collection.Any(o => ReferenceEquals(o, wrapper.Entity));
            }
            return false;
        }

        internal override void VerifyNavigationPropertyForAdd(IEntityWrapper wrapper)
        {
            // no-op
        }

        // This method is required to maintain compatibility with the v1 binary serialization format. 
        // In particular, it takes the dictionary of wrapped entities and creates a hash set of
        // raw entities that will be serialized.
        // Note that this is only expected to work for non-POCO entities, since serialization of POCO
        // entities will not result in serialization of the RelationshipManager or its related objects.
        /// <summary>Used internally to serialize entity objects.</summary>
        /// <param name="context">The streaming context.</param>
        [SuppressMessage("Microsoft.Usage", "CA2238:ImplementSerializationMethodsCorrectly")]
        [OnSerializing]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnSerializing(StreamingContext context)
        {
            if (!(WrappedOwner.Entity is IEntityWithRelationships))
            {
                throw new InvalidOperationException(Strings.RelatedEnd_CannotSerialize("EntityCollection"));
            }
            _relatedEntities = _wrappedRelatedEntities == null ? null : new HashSet<TEntity>(_wrappedRelatedEntities.Keys, ObjectReferenceEqualityComparer.Default);
        }

        // This method is required to maintain compatibility with the v1 binary serialization format. 
        // In particular, it takes the _relatedEntities HashSet and recreates the dictionary of wrapped
        // entities from it.  This is because the dictionary is not serialized.
        // Note that this is only expected to work for non-POCO entities, since serialization of POCO
        // entities will not result in serialization of the RelationshipManager or its related objects.
        /// <summary>Used internally to deserialize entity objects.</summary>
        /// <param name="context">The streaming context.</param>
        [OnDeserialized]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [SuppressMessage("Microsoft.Usage", "CA2238:ImplementSerializationMethodsCorrectly")]
        public void OnCollectionDeserialized(StreamingContext context)
        {
            if (_relatedEntities != null)
            {
                // We need to call this here so that the hash set will be fully constructed
                // ready for access.  Normally, this would happen later in the process.
                _relatedEntities.OnDeserialization(null);
                _wrappedRelatedEntities = new Dictionary<TEntity, IEntityWrapper>(ObjectReferenceEqualityComparer.Default);
                foreach (var entity in _relatedEntities)
                {
                    _wrappedRelatedEntities.Add(entity, EntityWrapperFactory.WrapEntityUsingContext(entity, ObjectContext));
                }
            }
        }

        // Identical code is in EntityReference, but this can't be moved to the base class because it relies on the
        // knowledge of the generic type, and the base class isn't generic
        /// <summary>Returns an object query that, when it is executed, returns the same set of objects that exists in the current collection. </summary>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" /> that represents the entity collection.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">
        /// When the object is in an <see cref="F:System.Data.Entity.EntityState.Added" /> state 
        /// or when the object is in a
        /// <see cref="F:System.Data.Entity.EntityState.Detached" /> state with a
        /// <see cref="T:System.Data.Entity.Core.Objects.MergeOption" /> other than
        /// <see cref="F:System.Data.Entity.Core.Objects.MergeOption.NoTracking" />.
        /// </exception>
        public ObjectQuery<TEntity> CreateSourceQuery()
        {
            CheckOwnerNull();
            bool hasResults;
            return CreateSourceQuery<TEntity>(DefaultMergeOption, out hasResults);
        }

        internal override IEnumerable CreateSourceQueryInternal()
        {
            return CreateSourceQuery();
        }

        //End identical code

        #region Add

        internal override void AddToLocalCache(IEntityWrapper wrappedEntity, bool applyConstraints)
        {
            DebugCheck.NotNull(wrappedEntity);

            WrappedRelatedEntities[(TEntity)wrappedEntity.Entity] = wrappedEntity;
        }

        internal override void AddToObjectCache(IEntityWrapper wrappedEntity)
        {
            DebugCheck.NotNull(wrappedEntity);

            // For POCO entities - add the object to the CLR collection
            if (TargetAccessor.HasProperty) // Null if the navigation does not exist in this direction
            {
                WrappedOwner.CollectionAdd(this, wrappedEntity.Entity);
            }
        }

        #endregion
    }
}
