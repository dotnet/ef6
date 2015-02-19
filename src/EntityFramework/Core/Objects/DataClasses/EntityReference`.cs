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
    using System.Xml.Serialization;

    /// <summary>
    /// Models a relationship end with multiplicity 1.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity being referenced.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [DataContract]
    [Serializable]
    public class EntityReference<TEntity> : EntityReference
        where TEntity : class
    {
        // ------
        // Fields
        // ------

        // The following fields are serialized.  Adding or removing a serialized field is considered
        // a breaking change.  This includes changing the field type or field name of existing
        // serialized fields. If you need to make this kind of change, it may be possible, but it
        // will require some custom serialization/deserialization code.
        // Note that this field should no longer be used directly.  Instead, use the _wrappedCachedValue
        // field.  This field is retained only for compatibility with the serialization format introduced in v1.
        private TEntity _cachedValue;

        [NonSerialized]
        private IEntityWrapper _wrappedCachedValue;

        // ------------
        // Constructors
        // ------------

        /// <summary>
        /// Creates a new instance of <see cref="T:System.Data.Entity.Core.Objects.DataClasses.EntityReference`1" />.
        /// </summary>
        /// <remarks>
        /// The default constructor is required for some serialization scenarios. It should not be used to
        /// create new EntityReferences. Use the GetRelatedReference or GetRelatedEnd methods on the RelationshipManager
        /// class instead.
        /// </remarks>
        public EntityReference()
        {
            _wrappedCachedValue = NullEntityWrapper.NullWrapper;
        }

        internal EntityReference(IEntityWrapper wrappedOwner, RelationshipNavigation navigation, IRelationshipFixer relationshipFixer)
            : base(wrappedOwner, navigation, relationshipFixer)
        {
            _wrappedCachedValue = NullEntityWrapper.NullWrapper;
        }

        // ----------
        // Properties
        // ----------

        /// <summary>
        /// Gets or sets the related object returned by this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.DataClasses.EntityReference`1" />
        /// .
        /// </summary>
        /// <returns>
        /// The object returned by this <see cref="T:System.Data.Entity.Core.Objects.DataClasses.EntityReference`1" />.
        /// </returns>
        [SoapIgnore]
        [XmlIgnore]
        public TEntity Value
        {
            get
            {
                DeferredLoad();
                return (TEntity)ReferenceValue.Entity;
            }
            set { ReferenceValue = EntityWrapperFactory.WrapEntityUsingContext(value, ObjectContext); }
        }

        internal override IEntityWrapper CachedValue
        {
            get { return _wrappedCachedValue; }
        }

        internal override IEntityWrapper ReferenceValue
        {
            get
            {
                CheckOwnerNull();
                return _wrappedCachedValue;
            }
            set
            {
                CheckOwnerNull();
                //setting to same value is a no-op (SQL BU DT # 446320)
                //setting to null is a special case because then we will also clear out any Added/Unchanged relationships with key entries, so we can't no-op if Value is null
                if (value.Entity != null
                    && value.Entity == _wrappedCachedValue.Entity)
                {
                    return;
                }

                if (null != value.Entity)
                {
                    // Note that this is only done for the case where we are not setting the ref to null because
                    // clearing a ref is okay--it will cause the dependent to become deleted/detached.
                    ValidateOwnerWithRIConstraints(
                        value, value == NullEntityWrapper.NullWrapper ? null : value.EntityKey, checkBothEnds: true);
                    var context = ObjectContext ?? value.Context;
                    if (context != null)
                    {
                        context.ObjectStateManager.TransactionManager.EntityBeingReparented =
                            GetDependentEndOfReferentialConstraint(value.Entity);
                    }
                    try
                    {
                        Add(value, /*applyConstraints*/false);
                    }
                    finally
                    {
                        if (context != null)
                        {
                            context.ObjectStateManager.TransactionManager.EntityBeingReparented = null;
                        }
                    }
                }
                else
                {
                    if (UsingNoTracking)
                    {
                        if (_wrappedCachedValue.Entity != null)
                        {
                            // The other end of relationship can be the EntityReference or EntityCollection
                            // If the other end is EntityReference, its IsLoaded property should be set to FALSE
                            var relatedEnd = GetOtherEndOfRelationship(_wrappedCachedValue);
                            relatedEnd.OnRelatedEndClear();
                        }

                        _isLoaded = false;
                    }
                    else
                    {
                        if (ObjectContext != null
                            && ObjectContext.ContextOptions.UseConsistentNullReferenceBehavior)
                        {
                            AttemptToNullFKsOnRefOrKeySetToNull();
                        }
                    }

                    ClearCollectionOrRef(null, null, false);
                }
            }
        }

        // -------
        // Methods
        // -------

        /// <summary>
        /// Loads the related object for this <see cref="T:System.Data.Entity.Core.Objects.DataClasses.EntityReference`1" /> with the specified merge option.
        /// </summary>
        /// <param name="mergeOption">
        /// Specifies how the object should be returned if it already exists in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        /// .
        /// </param>
        /// <exception cref="T:System.InvalidOperationException">
        /// The source of the <see cref="T:System.Data.Entity.Core.Objects.DataClasses.EntityReference`1" /> is null 
        /// or a query returned more than one related end 
        /// or a query returned zero related ends, and one related end was expected.
        /// </exception>
        public override void Load(MergeOption mergeOption)
        {
            CheckOwnerNull();

            // Validate that the Load is possible
            bool hasResults;
            var sourceQuery = ValidateLoad<TEntity>(mergeOption, "EntityReference", out hasResults);

            _suppressEvents = true; // we do not want any event during the bulk operation
            try
            {
                IList<TEntity> refreshedValue = null;
                if (hasResults)
                {
                    // Only issue a query if we know it can produce results (in the case of FK, there may not be any 
                    // results).
                    var objectResult = sourceQuery.Execute(sourceQuery.MergeOption);
                    refreshedValue = objectResult.ToList();
                }

                HandleRefreshedValue(mergeOption, refreshedValue);
            }
            finally
            {
                _suppressEvents = false;
            }
            // fire the AssociationChange with Refresh
            OnAssociationChanged(CollectionChangeAction.Refresh, null);
        }

#if !NET40

        /// <inheritdoc />
        public override async Task LoadAsync(MergeOption mergeOption, CancellationToken cancellationToken)
        {
            CheckOwnerNull();

            cancellationToken.ThrowIfCancellationRequested();

            // Validate that the Load is possible
            bool hasResults;
            var sourceQuery = ValidateLoad<TEntity>(mergeOption, "EntityReference", out hasResults);

            _suppressEvents = true; // we do not want any event during the bulk operation
            try
            {
                IList<TEntity> refreshedValue = null;
                if (hasResults)
                {
                    // Only issue a query if we know it can produce results (in the case of FK, there may not be any 
                    // results).
                    var objectResult =
                        await
                        sourceQuery.ExecuteAsync(sourceQuery.MergeOption, cancellationToken).WithCurrentCulture();
                    refreshedValue = await objectResult.ToListAsync(cancellationToken).WithCurrentCulture();
                }

                HandleRefreshedValue(mergeOption, refreshedValue);
            }
            finally
            {
                _suppressEvents = false;
            }
            // fire the AssociationChange with Refresh
            OnAssociationChanged(CollectionChangeAction.Refresh, null);
        }

#endif

        private void HandleRefreshedValue(MergeOption mergeOption, IList<TEntity> refreshedValue)
        {
            if (null == refreshedValue
                || !refreshedValue.Any())
            {
                if (!((AssociationType)RelationMetadata).IsForeignKey
                    && ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.One)
                {
                    //query returned zero related end; one related end was expected.
                    throw Error.EntityReference_LessThanExpectedRelatedEntitiesFound();
                }
                else if (mergeOption == MergeOption.OverwriteChanges
                         || mergeOption == MergeOption.PreserveChanges)
                {
                    // This entity is not related to anything in this AssociationSet and Role on the server.
                    // If there is an existing _cachedValue, we may need to clear it out, based on the MergeOption
                    var sourceKey = WrappedOwner.EntityKey;
                    if ((object)sourceKey == null)
                    {
                        throw Error.EntityKey_UnexpectedNull();
                    }
                    ObjectContext.ObjectStateManager.RemoveRelationships(
                        mergeOption, (AssociationSet)RelationshipSet, sourceKey, (AssociationEndMember)FromEndMember);
                }
                // else this is NoTracking or AppendOnly, and no entity was retrieved by the Load, so there's nothing extra to do

                // Since we have no value and are not doing a merge, the last step is to set IsLoaded to true
                _isLoaded = true;
            }
            else if (refreshedValue.Count() == 1)
            {
                Merge(refreshedValue, mergeOption, true /*setIsLoaded*/);
            }
            else
            {
                // More than 1 result, which is non-recoverable data inconsistency
                throw Error.EntityReference_MoreThanExpectedRelatedEntitiesFound();
            }
        }

        // <summary>
        // This operation is not allowed if the owner is null
        // </summary>
        internal override IEnumerable GetInternalEnumerable()
        {
            // This shouldn't be converted to an iterator method because then the check for a null owner
            // will not throw until the enumerator is advanced
            CheckOwnerNull();

            if (ReferenceValue.Entity != null)
            {
                return new[] { ReferenceValue.Entity };
            }
            else
            {
                return Enumerable.Empty<object>();
            }
        }

        internal override IEnumerable<IEntityWrapper> GetWrappedEntities()
        {
            return _wrappedCachedValue.Entity == null ? new IEntityWrapper[0] : new[] { _wrappedCachedValue };
        }

        /// <summary>Creates a many-to-one or one-to-one relationship between two objects in the object context.</summary>
        /// <param name="entity">The object being attached.</param>
        /// <exception cref="T:System.ArgumentNullException">When the  entity  is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">When the  entity  cannot be related to the current related end. This can occur when the association in the conceptual schema does not support a relationship between the two types.</exception>
        public void Attach(TEntity entity)
        {
            Check.NotNull(entity, "entity");

            CheckOwnerNull();
            Attach(new[] { EntityWrapperFactory.WrapEntityUsingContext(entity, ObjectContext) }, false);
        }

        internal override void Include(bool addRelationshipAsUnchanged, bool doAttach)
        {
            Debug.Assert(ObjectContext != null, "Should not be trying to add entities to state manager if context is null");

            // If we have an actual value or a key for this reference, add it to the context
            if (null != _wrappedCachedValue.Entity)
            {
                // Sometimes with mixed POCO and IPOCO, you can get different instances of IEntityWrappers stored in the IPOCO related ends
                // These should be replaced by the IEntityWrapper that is stored in the context
                var identityWrapper = EntityWrapperFactory.WrapEntityUsingContext(_wrappedCachedValue.Entity, WrappedOwner.Context);
                if (identityWrapper != _wrappedCachedValue)
                {
                    _wrappedCachedValue = identityWrapper;
                }
                IncludeEntity(_wrappedCachedValue, addRelationshipAsUnchanged, doAttach);
            }
            else if (DetachedEntityKey != null)
            {
                IncludeEntityKey(doAttach);
            }
            // else there is nothing to add for this relationship
        }

        private void IncludeEntityKey(bool doAttach)
        {
            var manager = ObjectContext.ObjectStateManager;

            var addNewRelationship = false;
            var addKeyEntry = false;
            var existingEntry = manager.FindEntityEntry(DetachedEntityKey);
            if (existingEntry == null)
            {
                // add new key entry and create a relationship with it                
                addKeyEntry = true;
                addNewRelationship = true;
            }
            else
            {
                if (existingEntry.IsKeyEntry)
                {
                    // We have an existing key entry, so just need to add a relationship with it

                    // We know the target end of this relationship is 1..1 or 0..1 since it is a reference, so if the source end is also not Many, we have a 1-to-1
                    if (FromEndMember.RelationshipMultiplicity
                        != RelationshipMultiplicity.Many)
                    {
                        // before we add a new relationship to this key entry, make sure it's not already related to something else
                        // We have to explicitly do this here because there are no other checks to make sure a key entry in a 1-to-1 doesn't end up in two of the same relationship
                        foreach (var relationshipEntry in ObjectContext.ObjectStateManager.FindRelationshipsByKey(DetachedEntityKey))
                        {
                            // only care about relationships in the same AssociationSet and where the key is playing the same role that it plays in this EntityReference                            
                            if (relationshipEntry.IsSameAssociationSetAndRole(
                                (AssociationSet)RelationshipSet, (AssociationEndMember)ToEndMember, DetachedEntityKey)
                                &&
                                relationshipEntry.State != EntityState.Deleted)
                            {
                                throw new InvalidOperationException(Strings.ObjectStateManager_EntityConflictsWithKeyEntry);
                            }
                        }
                    }

                    addNewRelationship = true;
                }
                else
                {
                    var wrappedTarget = existingEntry.WrappedEntity;

                    // Verify that the target entity is in a valid state for adding a relationship
                    if (existingEntry.State
                        == EntityState.Deleted)
                    {
                        throw new InvalidOperationException(Strings.RelatedEnd_UnableToAddRelationshipWithDeletedEntity);
                    }

                    // We know the target end of this relationship is 1..1 or 0..1 since it is a reference, so if the source end is also not Many, we have a 1-to-1
                    var relatedEnd = wrappedTarget.RelationshipManager.GetRelatedEndInternal(RelationshipName, RelationshipNavigation.From);
                    if (FromEndMember.RelationshipMultiplicity != RelationshipMultiplicity.Many
                        && !relatedEnd.IsEmpty())
                    {
                        // Make sure the target entity is not already related to something else.
                        // devnote: The call to Add below does *not* do this check for the fixup case, so if it's not done here, no failure will occur
                        //          and existing relationships may be deleted unexpectedly. RelatedEnd.Include should not remove existing relationships, only add new ones.
                        throw new InvalidOperationException(Strings.ObjectStateManager_EntityConflictsWithKeyEntry);
                    }

                    // We have an existing entity with the same key, just hook up the related ends
                    Add(
                        wrappedTarget,
                        applyConstraints: true,
                        addRelationshipAsUnchanged: doAttach,
                        relationshipAlreadyExists: false,
                        allowModifyingOtherEndOfRelationship: true,
                        forceForeignKeyChanges: true);

                    // add to the list of promoted key references so we can cleanup if a failure occurs later
                    manager.TransactionManager.PopulatedEntityReferences.Add(this);
                }
            }

            // For FKs, don't create a key entry and don't create a relationship
            if (addNewRelationship && !IsForeignKey)
            {
                // devnote: If we add any validation here, it needs to go here before adding the key entry,
                //          otherwise we have to clean up that entry if the validation fails

                if (addKeyEntry)
                {
                    var targetEntitySet = DetachedEntityKey.GetEntitySet(ObjectContext.MetadataWorkspace);
                    manager.AddKeyEntry(DetachedEntityKey, targetEntitySet);
                }

                var ownerKey = WrappedOwner.EntityKey;
                if ((object)ownerKey == null)
                {
                    throw Error.EntityKey_UnexpectedNull();
                }
                var wrapper = new RelationshipWrapper(
                    (AssociationSet)RelationshipSet,
                    RelationshipNavigation.From, ownerKey, RelationshipNavigation.To, DetachedEntityKey);
                manager.AddNewRelation(wrapper, doAttach ? EntityState.Unchanged : EntityState.Added);
            }
        }

        internal override void Exclude()
        {
            Debug.Assert(ObjectContext != null, "Should not be trying to remove entities from state manager if context is null");

            if (null != _wrappedCachedValue.Entity)
            {
                // It is possible that _cachedValue was originally null in this graph, but was only set
                // while the graph was being added, if the DetachedEntityKey matched its key. In that case,
                // we only want to clear _cachedValue and delete the relationship entry, but not remove the entity
                // itself from the context.
                var transManager = ObjectContext.ObjectStateManager.TransactionManager;
                var doFullRemove = transManager.PopulatedEntityReferences.Contains(this);
                var doRelatedEndRemove = transManager.AlignedEntityReferences.Contains(this);
                // For POCO, if the entity is undergoing snapshot for the first time, then in this step we actually
                // need to really exclude it rather than just disconnecting it.  If we don't, then it has the potential
                // to remain in the context at the end of the rollback process.
                if ((transManager.ProcessedEntities == null || !transManager.ProcessedEntities.Contains(_wrappedCachedValue))
                    &&
                    (doFullRemove || doRelatedEndRemove))
                {
                    // Retrieve the relationship entry before _cachedValue is set to null during Remove
                    var relationshipEntry = IsForeignKey ? null : FindRelationshipEntryInObjectStateManager(_wrappedCachedValue);
                    Debug.Assert(
                        IsForeignKey || relationshipEntry != null,
                        "Should have been able to find a valid relationship since _cachedValue is non-null");

                    // Remove the related ends and mark the relationship as deleted, but don't propagate the changes to the target entity itself
                    Remove(
                        _wrappedCachedValue,
                        doFixup: doFullRemove,
                        deleteEntity: false,
                        deleteOwner: false,
                        applyReferentialConstraints: false,
                        preserveForeignKey: true);

                    // The relationship will now either be detached (if it was previously in the Added state), or Deleted (if it was previously Unchanged)
                    // If it's Deleted, we need to AcceptChanges to get rid of it completely                    
                    if (relationshipEntry != null
                        && relationshipEntry.State != EntityState.Detached)
                    {
                        relationshipEntry.AcceptChanges();
                    }

                    // Since this has been processed, remove it from the list
                    if (doFullRemove)
                    {
                        transManager.PopulatedEntityReferences.Remove(this);
                    }
                    else
                    {
                        transManager.AlignedEntityReferences.Remove(this);
                    }
                }
                else
                {
                    ExcludeEntity(_wrappedCachedValue);
                }
            }
            else if (DetachedEntityKey != null)
            {
                // there may still be relationship entries with stubs that need to be removed
                // this works whether we just added the key entry along with the relationship or if it was already existing
                ExcludeEntityKey();
            }
            // else there is nothing to remove for this relationship
        }

        private void ExcludeEntityKey()
        {
            var ownerKey = WrappedOwner.EntityKey;

            var relationshipEntry = ObjectContext.ObjectStateManager.FindRelationship(
                RelationshipSet,
                new KeyValuePair<string, EntityKey>(RelationshipNavigation.From, ownerKey),
                new KeyValuePair<string, EntityKey>(RelationshipNavigation.To, DetachedEntityKey));

            // we may have failed in adding the graph before we actually added this relationship, so make sure we actually found one
            if (relationshipEntry != null)
            {
                relationshipEntry.Delete( /*doFixup*/ false);
                // If entry was Added before, it is now Detached, otherwise AcceptChanges to detach it
                if (relationshipEntry.State
                    != EntityState.Detached)
                {
                    relationshipEntry.AcceptChanges();
                }
            }
        }

        internal override void ClearCollectionOrRef(IEntityWrapper wrappedEntity, RelationshipNavigation navigation, bool doCascadeDelete)
        {
            if (wrappedEntity == null)
            {
                wrappedEntity = NullEntityWrapper.NullWrapper;
            }
            if (null != _wrappedCachedValue.Entity)
            {
                // Following condition checks if we have already visited this graph node. If its true then
                // we should not do fixup because that would cause circular loop
                if ((wrappedEntity.Entity == _wrappedCachedValue.Entity)
                    && (navigation.Equals(RelationshipNavigation)))
                {
                    Remove(
                        _wrappedCachedValue, /*fixup*/false, /*deleteEntity*/false, /*deleteOwner*/false, /*applyReferentialConstraints*/
                        false, /*preserveForeignKey*/false);
                }
                else
                {
                    Remove(
                        _wrappedCachedValue, /*fixup*/true, doCascadeDelete, /*deleteOwner*/false, /*applyReferentialConstraints*/true,
                        /*preserveForeignKey*/false);
                }
            }
            else
            {
                // this entity reference could be replacing a relationship that points to a key entry
                // we need to search relationships on the Owner entity to see if this is true, and if so remove the relationship entry
                if (WrappedOwner.Entity != null
                    && WrappedOwner.Context != null
                    && !UsingNoTracking)
                {
                    var ownerEntry = WrappedOwner.Context.ObjectStateManager.GetEntityEntry(WrappedOwner.Entity);
                    ownerEntry.DeleteRelationshipsThatReferenceKeys(RelationshipSet, ToEndMember);
                }
            }

            // If we have an Owner, clear the DetachedEntityKey.
            // If we do not have an owner, retain the key so that we can resolve the difference when the entity is attached to a context
            if (WrappedOwner.Entity != null)
            {
                // Clear the detachedEntityKey as well. In cases where we have to fix up the detachedEntityKey, we will not always be able to detect
                // if we have *only* a Deleted relationship for a given entity/relationship/role, so clearing this here will ensure that
                // even if no other relationships are added, the key value will still be correct.
                DetachedEntityKey = null;
            }
        }

        internal override void ClearWrappedValues()
        {
            _cachedValue = null;
            _wrappedCachedValue = NullEntityWrapper.NullWrapper;
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
                    Strings.RelatedEnd_InvalidContainedType_Reference(wrappedEntity.Entity.GetType().FullName, typeof(TEntity).FullName));
            }
        }

        // <summary>
        // Disconnected adds are not supported for an EntityReference so we should report this as an error.
        // </summary>
        // <param name="wrappedEntity"> The entity to add to the related end in a disconnected state. </param>
        internal override void DisconnectedAdd(IEntityWrapper wrappedEntity)
        {
            DebugCheck.NotNull(wrappedEntity);

            CheckOwnerNull();
        }

        // <summary>
        // Disconnected removes are not supported for an EntityReference so we should report this as an error.
        // </summary>
        // <param name="wrappedEntity"> The entity to remove from the related end in a disconnected state. </param>
        internal override bool DisconnectedRemove(IEntityWrapper wrappedEntity)
        {
            DebugCheck.NotNull(wrappedEntity);

            CheckOwnerNull();
            return false;
        }

        // <summary>
        // Remove from the RelatedEnd
        // </summary>
        internal override bool RemoveFromLocalCache(IEntityWrapper wrappedEntity, bool resetIsLoaded, bool preserveForeignKey)
        {
            DebugCheck.NotNull(wrappedEntity);
            Debug.Assert(
                null == _wrappedCachedValue.Entity || wrappedEntity.Entity == _wrappedCachedValue.Entity,
                "The specified object is not a part of this relationship.");

            _wrappedCachedValue = NullEntityWrapper.NullWrapper;
            _cachedValue = null;

            if (resetIsLoaded)
            {
                _isLoaded = false;
            }

            // This code sets nullable FK properties on a dependent end to null when a relationship has been nulled.
            if (ObjectContext != null
                && IsForeignKey
                && !preserveForeignKey)
            {
                NullAllForeignKeys();
            }
            return true;
        }

        // <summary>
        // Remove from the POCO collection
        // </summary>
        internal override bool RemoveFromObjectCache(IEntityWrapper wrappedEntity)
        {
            DebugCheck.NotNull(wrappedEntity);

            // For POCO entities - clear the CLR reference
            if (TargetAccessor.HasProperty)
            {
                WrappedOwner.RemoveNavigationPropertyValue(this, wrappedEntity.Entity);
            }

            return true;
        }

        // Method used to retrieve properties from principal entities.
        // NOTE: 'properties' list is modified in this method and may already contains some properties.
        internal override void RetrieveReferentialConstraintProperties(
            Dictionary<string, KeyValuePair<object, IntBox>> properties, HashSet<object> visited)
        {
            DebugCheck.NotNull(properties);

            if (_wrappedCachedValue.Entity != null)
            {
                // Dictionary< propertyName, <propertyValue, counter>>
                Dictionary<string, KeyValuePair<object, IntBox>> retrievedProperties;

                // PERFORMANCE: ReferentialConstraints collection in typical scenario is very small (1-3 elements)
                foreach (var constraint in ((AssociationType)RelationMetadata).ReferentialConstraints)
                {
                    if (constraint.ToRole == FromEndMember)
                    {
                        // Detect circular references
                        if (visited.Contains(_wrappedCachedValue))
                        {
                            throw new InvalidOperationException(Strings.RelationshipManager_CircularRelationshipsWithReferentialConstraints);
                        }
                        visited.Add(_wrappedCachedValue);

                        _wrappedCachedValue.RelationshipManager.RetrieveReferentialConstraintProperties(
                            out retrievedProperties, visited, includeOwnValues: true);

                        Debug.Assert(retrievedProperties != null);
                        Debug.Assert(
                            constraint.FromProperties.Count == constraint.ToProperties.Count,
                            "Referential constraints From/To properties list have different size");

                        // Following loop rewrites properties from "retrievedProperties" into "properties".
                        // At the same time, property's name is translated from name from principal end into name from dependent end:
                        // Example: Client<C_ID> - Order<O_ID, Client_ID>   
                        //          Client is principal end, Order is dependent end, Client.C_ID == Order.Client_ID
                        // Input : retrievedProperties = { "C_ID" = 123 }
                        // Output: properties = { "Client_ID" = 123 }

                        // NOTE order of properties in collections constraint.From/ToProperties is important
                        for (var i = 0; i < constraint.FromProperties.Count; ++i)
                        {
                            EntityEntry.AddOrIncreaseCounter(
                                constraint,
                                properties,
                                constraint.ToProperties[i].Name,
                                retrievedProperties[constraint.FromProperties[i].Name].Key);
                        }
                    }
                }
            }
        }

        internal override bool IsEmpty()
        {
            return _wrappedCachedValue.Entity == null;
        }

        internal override void VerifyMultiplicityConstraintsForAdd(bool applyConstraints)
        {
            if (applyConstraints && !IsEmpty())
            {
                throw new InvalidOperationException(
                    Strings.EntityReference_CannotAddMoreThanOneEntityToEntityReference(
                        RelationshipNavigation.To, RelationshipNavigation.RelationshipName));
            }
        }

        // Update IsLoaded flag if necessary
        // This method is called when Clear() was called on the other end of relationship (if the other end is EntityCollection)
        // or when Value property of the other end was set to null (if the other end is EntityReference).
        // This method is used only when NoTracking option was used.
        internal override void OnRelatedEndClear()
        {
            // If other end of relationship was loaded, it mean that this end was also cleared.
            _isLoaded = false;
        }

        internal override bool ContainsEntity(IEntityWrapper wrappedEntity)
        {
            DebugCheck.NotNull(wrappedEntity);

            return _wrappedCachedValue.Entity != null && _wrappedCachedValue.Entity == wrappedEntity.Entity;
        }

        // Identical code is in EntityCollection, but this can't be moved to the base class because it relies on the
        // knowledge of the generic type, and the base class isn't generic
        /// <summary>Creates an equivalent object query that returns the related object.</summary>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Objects.ObjectQuery`1" /> that returns the related object.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">
        /// When the object is in an <see cref="F:System.Data.Entity.EntityState.Added" /> state 
        /// or when the object is in a <see cref="F:System.Data.Entity.EntityState.Detached" />
        /// state with a <see cref="P:System.Data.Entity.Core.Objects.ObjectQuery.MergeOption" />
        /// other than <see cref="F:System.Data.Entity.Core.Objects.MergeOption.NoTracking" />.
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

        // <summary>
        // Take any values in the incoming RelatedEnd and sets them onto the values
        // that currently exist in this RelatedEnd
        // </summary>
        internal void InitializeWithValue(RelatedEnd relatedEnd)
        {
            Debug.Assert(_wrappedCachedValue.Entity == null, "The EntityReference already has a value.");
            var reference = relatedEnd as EntityReference<TEntity>;
            if (reference != null
                && reference._wrappedCachedValue.Entity != null)
            {
                _wrappedCachedValue = reference._wrappedCachedValue;
                _cachedValue = (TEntity)_wrappedCachedValue.Entity;
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

            return ReferenceEquals(value, wrapper.Entity);
        }

        internal override void VerifyNavigationPropertyForAdd(IEntityWrapper wrapper)
        {
            if (TargetAccessor.HasProperty)
            {
                var value = WrappedOwner.GetNavigationPropertyValue(this);
                if (!ReferenceEquals(null, value)
                    && !ReferenceEquals(value, wrapper.Entity))
                {
                    throw new InvalidOperationException(
                        Strings.EntityReference_CannotAddMoreThanOneEntityToEntityReference(
                            RelationshipNavigation.To, RelationshipNavigation.RelationshipName));
                }
            }
        }

        // This method is required to maintain compatibility with the v1 binary serialization format. 
        // In particular, it recreates a entity wrapper from the serialized cached value.
        // Note that this is only expected to work for non-POCO entities, since serialization of POCO
        // entities will not result in serialization of the RelationshipManager or its related objects.
        /// <summary>This method is used internally to serialize related entity objects.</summary>
        /// <param name="context">The serialized stream.</param>
        [OnDeserialized]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [SuppressMessage("Microsoft.Usage", "CA2238:ImplementSerializationMethodsCorrectly")]
        public void OnRefDeserialized(StreamingContext context)
        {
            _wrappedCachedValue = EntityWrapperFactory.WrapEntityUsingContext(_cachedValue, ObjectContext);
        }

        /// <summary>This method is used internally to serialize related entity objects.</summary>
        /// <param name="context">The serialized stream.</param>
        [OnSerializing]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [SuppressMessage("Microsoft.Usage", "CA2238:ImplementSerializationMethodsCorrectly")]
        public void OnSerializing(StreamingContext context)
        {
            if (!(WrappedOwner.Entity is IEntityWithRelationships))
            {
                throw new InvalidOperationException(Strings.RelatedEnd_CannotSerialize("EntityReference"));
            }
        }

        #region Add

        // <summary>
        // AddToLocalEnd is used by both APIs a) RelatedEnd.Add b) Value property setter.
        // ApplyConstraints is true in case of RelatedEnd.Add because one cannot add entity to ref it its already set
        // however applyConstraints is false in case of Value property setter because value can be set to a new value
        // even if its non null.
        // </summary>
        internal override void AddToLocalCache(IEntityWrapper wrappedEntity, bool applyConstraints)
        {
            DebugCheck.NotNull(wrappedEntity);

            if (wrappedEntity != _wrappedCachedValue)
            {
                var tm = ObjectContext != null ? ObjectContext.ObjectStateManager.TransactionManager : null;
                if (applyConstraints && null != _wrappedCachedValue.Entity)
                {
                    // The idea here is that we want to throw for constraint violations in things that we are bringing in,
                    // but not when replacing references of things already in the context.  Therefore, if the the thing that
                    // we're replacing is in ProcessedEntities it means we're bringing it in and we should throw.
                    if (tm == null
                        || tm.ProcessedEntities == null
                        || tm.ProcessedEntities.Contains(_wrappedCachedValue))
                    {
                        throw new InvalidOperationException(
                            Strings.EntityReference_CannotAddMoreThanOneEntityToEntityReference(
                                RelationshipNavigation.To, RelationshipNavigation.RelationshipName));
                    }
                }
                if (tm != null
                    && wrappedEntity.Entity != null)
                {
                    // Setting this flag will prevent the FK from being temporarily set to null while changing
                    // it from one value to the next.
                    tm.BeginRelatedEndAdd();
                }
                try
                {
                    ClearCollectionOrRef(null, null, false);
                    _wrappedCachedValue = wrappedEntity;
                    _cachedValue = (TEntity)wrappedEntity.Entity;
                }
                finally
                {
                    if (tm != null
                        && tm.IsRelatedEndAdd)
                    {
                        tm.EndRelatedEndAdd();
                    }
                }
            }
        }

        internal override void AddToObjectCache(IEntityWrapper wrappedEntity)
        {
            DebugCheck.NotNull(wrappedEntity);

            // For POCO entities - set the CLR reference
            if (TargetAccessor.HasProperty)
            {
                WrappedOwner.SetNavigationPropertyValue(this, wrappedEntity.Entity);
            }
        }

        #endregion
    }
}
