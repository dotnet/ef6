namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq.Expressions;

    /// <summary>
    /// implementation of ObjectStateManager class
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class ObjectStateManager : IEntityStateManager
    {
        // This is the initial capacity used for lists of entries.  We use this rather than the default because
        // perf testing showed we were almost always increasing the capacity which can be quite a slow operation.
        internal const int InitialListSize = 16;

        private bool _isDisposed;
        private InternalObjectStateManager _internalObjectStateManager;

        // delegate for notifying changes in collection
        private CollectionChangeEventHandler onObjectStateManagerChangedDelegate;
        private CollectionChangeEventHandler onEntityDeletedDelegate;

        /// <summary>
        /// ObjectStateManager constructor.
        /// </summary>
        /// <param name="metadataWorkspace"></param>
        [CLSCompliant(false)]
        public ObjectStateManager(MetadataWorkspace metadataWorkspace)
            : this(new InternalObjectStateManager(metadataWorkspace))
        {
            Contract.Requires(metadataWorkspace != null);
        }

        internal ObjectStateManager(InternalObjectStateManager internalObjectStateManager)
        {
            _internalObjectStateManager = internalObjectStateManager;
            _internalObjectStateManager.ObjectStateManagerWrapper = this;
        }

        #region Internal Properties for ObjectStateEntry change tracking

        internal object ChangingObject
        {
            get { return _internalObjectStateManager.ChangingObject; }
            set { _internalObjectStateManager.ChangingObject = value; }
        }

        internal string ChangingEntityMember
        {
            get { return _internalObjectStateManager.ChangingEntityMember; }
            set { _internalObjectStateManager.ChangingEntityMember = value; }
        }

        internal string ChangingMember
        {
            get { return _internalObjectStateManager.ChangingMember; }
            set { _internalObjectStateManager.ChangingMember = value; }
        }

        internal EntityState ChangingState
        {
            get { return _internalObjectStateManager.ChangingState; }
            set { _internalObjectStateManager.ChangingState = value; }
        }

        internal bool SaveOriginalValues
        {
            get { return _internalObjectStateManager.SaveOriginalValues; }
            set { _internalObjectStateManager.SaveOriginalValues = value; }
        }

        internal object ChangingOldValue
        {
            get { return _internalObjectStateManager.ChangingOldValue; }
            set { _internalObjectStateManager.ChangingOldValue = value; }
        }

        // Used by ObjectStateEntry to determine if it's safe to set a value
        // on a non-null IEntity.EntityKey property
        internal bool InRelationshipFixup
        {
            get { return _internalObjectStateManager.InRelationshipFixup; }
        }

        internal ComplexTypeMaterializer ComplexTypeMaterializer
        {
            get { return _internalObjectStateManager.ComplexTypeMaterializer; }
        }

        #endregion

        internal TransactionManager TransactionManager
        {
            get { return _internalObjectStateManager.TransactionManager; }
        }

        internal virtual EntityWrapperFactory EntityWrapperFactory
        {
            get { return _internalObjectStateManager.EntityWrapperFactory; }
        }

        /// <summary>
        /// MetadataWorkspace property
        /// </summary>
        /// <returns>MetadataWorkspace</returns>
        [CLSCompliant(false)]
        public MetadataWorkspace MetadataWorkspace
        {
            get { return _internalObjectStateManager.MetadataWorkspace; }
        }

        /// <summary>
        /// Flag that is set when we are processing an FK setter for a full proxy.
        /// This is used to determine whether or not we will attempt to call out into FK
        /// setters and null references during fixup.
        /// The value of this property is either null if the code is not executing an
        /// FK setter, or points to the entity on which the FK setter has been called.
        /// </summary>
        internal object EntityInvokingFKSetter
        {
            get { return _internalObjectStateManager.EntityInvokingFKSetter; }
            set { _internalObjectStateManager.EntityInvokingFKSetter = value; }
        }

        #region events ObjectStateManagerChanged / EntityDeleted

        /// <summary>
        /// Event to notify changes in the collection.
        /// </summary>
        public event CollectionChangeEventHandler ObjectStateManagerChanged
        {
            add { onObjectStateManagerChangedDelegate += value; }
            remove { onObjectStateManagerChangedDelegate -= value; }
        }

        internal event CollectionChangeEventHandler EntityDeleted
        {
            add { onEntityDeletedDelegate += value; }
            remove { onEntityDeletedDelegate -= value; }
        }

        internal void OnObjectStateManagerChanged(CollectionChangeAction action, object entity)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            if (onObjectStateManagerChangedDelegate != null)
            {
                onObjectStateManagerChangedDelegate(this, new CollectionChangeEventArgs(action, entity));
            }
        }

        internal void OnEntityDeleted(CollectionChangeAction action, object entity)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            if (onEntityDeletedDelegate != null)
            {
                onEntityDeletedDelegate(this, new CollectionChangeEventArgs(action, entity));
            }
        }

        #endregion

        /// <summary>
        /// Adds an object stub to the cache.
        /// </summary>
        /// <param name="entityKey">the key of the object to add</param>
        /// <param name="entitySet">the entity set of the given object</param>
        /// 
        internal EntityEntry AddKeyEntry(EntityKey entityKey, EntitySet entitySet)
        {
            Debug.Assert((object)entityKey != null, "entityKey cannot be null.");
            Debug.Assert(entitySet != null, "entitySet must be non-null.");

            return _internalObjectStateManager.AddKeyEntry(entityKey, entitySet);
        }

        /// <summary>
        /// Adds an object to the ObjectStateManager.
        /// </summary>
        /// <param name="dataObject">the object to add</param>
        /// <param name="entitySet">the entity set of the given object</param>
        /// <param name="argumentName">Name of the argument passed to a public method, for use in exceptions.</param>
        /// <param name="isAdded">Indicates whether the entity is added or unchanged.</param>
        internal EntityEntry AddEntry(
            IEntityWrapper wrappedObject, EntityKey passedKey, EntitySet entitySet, string argumentName, bool isAdded)
        {
            Debug.Assert(wrappedObject != null, "entity wrapper cannot be null.");
            Debug.Assert(wrappedObject.Entity != null, "entity cannot be null.");
            Debug.Assert(wrappedObject.Context != null, "the context should be already set");
            Debug.Assert(entitySet != null, "entitySet must be non-null.");
            // shadowValues is allowed to be null
            Debug.Assert(argumentName != null, "argumentName cannot be null.");

            return _internalObjectStateManager.AddEntry(wrappedObject, passedKey, entitySet, argumentName, isAdded);
        }

        internal void FixupReferencesByForeignKeys(EntityEntry newEntry, bool replaceAddedRefs = false)
        {
            _internalObjectStateManager.FixupReferencesByForeignKeys(newEntry, replaceAddedRefs);
        }

        /// <summary>
        /// Adds an entry to the index of foreign keys that reference entities that we don't yet know about.
        /// </summary>
        /// <param name="foreignKey">The foreign key found in the entry</param>
        /// <param name="entry">The entry that contains the foreign key that was found</param>
        internal void AddEntryContainingForeignKeyToIndex(EntityKey foreignKey, EntityEntry entry)
        {
            _internalObjectStateManager.AddEntryContainingForeignKeyToIndex(foreignKey, entry);
        }

        [Conditional("DEBUG")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Bug in FxCop rule")]
        internal void AssertEntryDoesNotExistInForeignKeyIndex(EntityEntry entry)
        {
            _internalObjectStateManager.AssertEntryDoesNotExistInForeignKeyIndex(entry);
        }

        [Conditional("DEBUG")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults",
            Justification = "This method is compiled only when the compilation symbol DEBUG is defined")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Bug in FxCop rule")]
        internal void AssertAllForeignKeyIndexEntriesAreValid()
        {
            _internalObjectStateManager.AssertAllForeignKeyIndexEntriesAreValid();
        }

        /// <summary>
        /// Removes an entry to the index of foreign keys that reference entities that we don't yet know about.
        /// This is typically done when the entity is detached from the context.
        /// </summary>
        /// <param name="foreignKey">The foreign key found in the entry</param>
        /// <param name="entry">The entry that contains the foreign key that was found</param>
        internal void RemoveEntryFromForeignKeyIndex(EntityKey foreignKey, EntityEntry entry)
        {
            _internalObjectStateManager.RemoveEntryFromForeignKeyIndex(foreignKey, entry);
        }

        /// <summary>
        /// Removes the foreign key from the index of those keys that have been found in entries
        /// but for which it was not possible to do fixup because the entity that the foreign key
        /// referenced was not in the state manager.
        /// </summary>
        /// <param name="foreignKey">The key to lookup and remove</param>
        internal void RemoveForeignKeyFromIndex(EntityKey foreignKey)
        {
            _internalObjectStateManager.RemoveForeignKeyFromIndex(foreignKey);
        }

        /// <summary>
        /// Gets all state entries that contain the given foreign key for which we have not performed
        /// fixup because the state manager did not contain the entity to which the foreign key pointed.
        /// </summary>
        /// <param name="foreignKey">The key to lookup</param>
        /// <returns>The state entries that contain the key</returns>
        internal IEnumerable<EntityEntry> GetNonFixedupEntriesContainingForeignKey(EntityKey foreignKey)
        {
            return _internalObjectStateManager.GetNonFixedupEntriesContainingForeignKey(foreignKey);
        }

        /// <summary>
        /// Adds to index of currently tracked entities that have FK values that are conceptually
        /// null but not actually null because the FK properties are not nullable.
        /// If this index is non-empty in AcceptAllChanges or SaveChanges, then we throw.
        /// If AcceptChanges is called on an entity and that entity is in the index, then
        /// we will throw.
        /// Note that the index is keyed by EntityEntry reference because it's only ever used
        /// when we have the EntityEntry and this makes it slightly faster than using key lookup.
        /// </summary>
        internal void RememberEntryWithConceptualNull(EntityEntry entry)
        {
            _internalObjectStateManager.RememberEntryWithConceptualNull(entry);
        }

        /// <summary>
        /// Checks whether or not there is some entry in the context that has any conceptually but not
        /// actually null FK values.
        /// </summary>
        internal bool SomeEntryWithConceptualNullExists()
        {
            return _internalObjectStateManager.SomeEntryWithConceptualNullExists();
        }

        /// <summary>
        /// Checks whether the given entry has conceptually but not actually null FK values.
        /// </summary>
        internal bool EntryHasConceptualNull(EntityEntry entry)
        {
            return _internalObjectStateManager.EntryHasConceptualNull(entry);
        }

        /// <summary>
        /// Stops keeping track of an entity with conceptual nulls because the FK values have been
        /// really set or because the entity is leaving the context or becoming deleted.
        /// </summary>
        internal void ForgetEntryWithConceptualNull(EntityEntry entry, bool resetAllKeys)
        {
            _internalObjectStateManager.ForgetEntryWithConceptualNull(entry, resetAllKeys);
        }

        // devnote: see comment to SQLBU 555615 in ObjectContext.AttachSingleObject()
        internal void PromoteKeyEntryInitialization(
            ObjectContext contextToAttach,
            EntityEntry keyEntry,
            IEntityWrapper wrappedEntity,
            bool replacingEntry)
        {
            Debug.Assert(keyEntry != null, "keyEntry must be non-null.");
            Debug.Assert(wrappedEntity != null, "entity cannot be null.");
            // shadowValues is allowed to be null

            _internalObjectStateManager.PromoteKeyEntryInitialization(contextToAttach, keyEntry, wrappedEntity, replacingEntry);
        }

        /// <summary>
        /// Upgrades an entity key entry in the cache to a a regular entity
        /// </summary>
        /// <param name="keyEntry">the key entry that exists in the state manager</param>
        /// <param name="entity">the object to add</param>
        /// <param name="replacingEntry">True if this promoted key entry is replacing an existing detached entry</param>
        /// <param name="setIsLoaded">Tells whether we should allow the IsLoaded flag to be set to true for RelatedEnds</param>
        internal void PromoteKeyEntry(
            EntityEntry keyEntry,
            IEntityWrapper wrappedEntity,
            bool replacingEntry,
            bool setIsLoaded,
            bool keyEntryInitialized)
        {
            Debug.Assert(keyEntry != null, "keyEntry must be non-null.");
            Debug.Assert(wrappedEntity != null, "entity wrapper cannot be null.");
            Debug.Assert(wrappedEntity.Entity != null, "entity cannot be null.");
            Debug.Assert(wrappedEntity.Context != null, "the context should be already set");

            _internalObjectStateManager.PromoteKeyEntry(keyEntry, wrappedEntity, replacingEntry, setIsLoaded, keyEntryInitialized);
        }

        internal void TrackPromotedRelationship(RelatedEnd relatedEnd, IEntityWrapper wrappedEntity)
        {
            Debug.Assert(relatedEnd != null);
            Debug.Assert(wrappedEntity != null);
            Debug.Assert(wrappedEntity.Entity != null);
            Debug.Assert(
                TransactionManager.IsAttachTracking || TransactionManager.IsAddTracking,
                "This method should be called only from ObjectContext.AttachTo/AddObject (indirectly)");
 
            _internalObjectStateManager.TrackPromotedRelationship(relatedEnd, wrappedEntity);
        }

        internal void DegradePromotedRelationships()
        {
            Debug.Assert(
                TransactionManager.IsAttachTracking || TransactionManager.IsAddTracking,
                "This method should be called only from the cleanup code");

            _internalObjectStateManager.DegradePromotedRelationships();
        }

        /// <summary>
        /// Performs non-generic collection or reference fixup between two entities
        /// This method should only be used in scenarios where we are automatically hooking up relationships for
        /// the user, and not in cases where they are manually setting relationships.
        /// </summary>
        /// <param name="mergeOption">The MergeOption to use to decide how to resolve EntityReference conflicts</param>
        /// <param name="sourceEntity">The entity instance on the source side of the relationship</param>
        /// <param name="sourceMember">The AssociationEndMember that contains the metadata for the source entity</param>
        /// <param name="targetEntity">The entity instance on the source side of the relationship</param>
        /// <param name="targetMember">The AssociationEndMember that contains the metadata for the target entity</param>
        /// <param name="setIsLoaded">Tells whether we should allow the IsLoaded flag to be set to true for RelatedEnds</param>
        /// <param name="relationshipAlreadyExists">Whether or not the relationship entry already exists in the cache for these entities</param>
        /// <param name="inKeyEntryPromotion">Whether this method is used in key entry promotion</param>
        internal static void AddEntityToCollectionOrReference(
            MergeOption mergeOption,
            IEntityWrapper wrappedSource,
            AssociationEndMember sourceMember,
            IEntityWrapper wrappedTarget,
            AssociationEndMember targetMember,
            bool setIsLoaded,
            bool relationshipAlreadyExists,
            bool inKeyEntryPromotion)
        {
            // Call GetRelatedEnd to retrieve the related end on the source entity that points to the target entity
            var relatedEnd = wrappedSource.RelationshipManager.GetRelatedEndInternal(sourceMember.DeclaringType.FullName, targetMember.Name);

            // EntityReference can only have one value
            if (targetMember.RelationshipMultiplicity != RelationshipMultiplicity.Many)
            {
                Debug.Assert(
                    relatedEnd is EntityReference, "If end is not Many multiplicity, then the RelatedEnd should be an EntityReference.");
                var relatedReference = (EntityReference)relatedEnd;

                switch (mergeOption)
                {
                    case MergeOption.NoTracking:
                        // if using NoTracking, we have no way of determining identity resolution.
                        // Throw an exception saying the EntityReference is already populated and to try using
                        // a different MergeOption
                        Debug.Assert(
                            relatedEnd.IsEmpty(),
                            "This can occur when objects are loaded using a NoTracking merge option. Try using a different merge option when loading objects.");
                        break;
                    case MergeOption.AppendOnly:
                        // SQLBU 551031
                        // In key entry promotion case, detect that sourceEntity is already related to some entity in the context,
                        // so it cannot be related to another entity being attached (relation 1-1).
                        // Without this check we would throw exception from RelatedEnd.Add() but the exception message couldn't
                        // properly describe what has happened.
                        if (inKeyEntryPromotion &&
                            !relatedReference.IsEmpty()
                            &&
                            !ReferenceEquals(relatedReference.ReferenceValue.Entity, wrappedTarget.Entity))
                        {
                            throw new InvalidOperationException(Strings.ObjectStateManager_EntityConflictsWithKeyEntry);
                        }
                        break;

                    case MergeOption.PreserveChanges:
                    case MergeOption.OverwriteChanges:
                        // Retrieve the current target entity and the relationship
                        var currentWrappedTarget = relatedReference.ReferenceValue;

                        // currentWrappedTarget may already be correct because we may already have done FK fixup as part of
                        // accepting changes in the overwrite code.
                        if (currentWrappedTarget != null && currentWrappedTarget.Entity != null && currentWrappedTarget != wrappedTarget)
                        {
                            // The source entity is already related to a different target, so before we hook it up to the new target,
                            // disconnect the existing related ends and detach the relationship entry
                            var relationshipEntry = relatedEnd.FindRelationshipEntryInObjectStateManager(currentWrappedTarget);
                            Debug.Assert(
                                relationshipEntry != null || relatedEnd.IsForeignKey,
                                "Could not find relationship entry for LAT relationship.");

                            relatedEnd.RemoveAll();

                            if (relationshipEntry != null)
                            {
                                Debug.Assert(relationshipEntry != null, "Could not find relationship entry.");
                                // If the relationship was Added prior to the above RemoveAll, it will have already been detached
                                // If it was Unchanged, it is now Deleted and should be detached
                                // It should never have been Deleted before now, because we just got currentTargetEntity from the related end
                                if (relationshipEntry.State == EntityState.Deleted)
                                {
                                    relationshipEntry.AcceptChanges();
                                }

                                Debug.Assert(relationshipEntry.State == EntityState.Detached, "relationshipEntry should be Detached");
                            }
                        }
                        break;
                }
            }

            RelatedEnd targetRelatedEnd = null;
            if (mergeOption == MergeOption.NoTracking)
            {
                targetRelatedEnd = relatedEnd.GetOtherEndOfRelationship(wrappedTarget);
                if (targetRelatedEnd.IsLoaded)
                {
                    // The EntityCollection has already been loaded as part of the query and adding additional
                    // entities would cause duplicate entries
                    throw new InvalidOperationException(Strings.Collections_CannotFillTryDifferentMergeOption(targetRelatedEnd.SourceRoleName, targetRelatedEnd.RelationshipName));
                }
            }

            // we may have already retrieved the target end above, but if not, just get it now
            if (targetRelatedEnd == null)
            {
                targetRelatedEnd = relatedEnd.GetOtherEndOfRelationship(wrappedTarget);
            }

            // Add the target entity
            relatedEnd.Add(
                wrappedTarget,
                applyConstraints: true,
                addRelationshipAsUnchanged: true,
                relationshipAlreadyExists: relationshipAlreadyExists,
                allowModifyingOtherEndOfRelationship: true,
                forceForeignKeyChanges: true);

            Debug.Assert(
                !(inKeyEntryPromotion && wrappedSource.Context == null),
                "sourceEntity has been just attached to the context in PromoteKeyEntry, so Context shouldn't be null");
            Debug.Assert(
                !(inKeyEntryPromotion &&
                  wrappedSource.Context.ObjectStateManager.TransactionManager.IsAttachTracking &&
                  (setIsLoaded || mergeOption == MergeOption.NoTracking)),
                "This verifies that UpdateRelatedEnd is a no-op in a keyEntryPromotion case when the method is called indirectly from ObjectContext.AttachTo");

            // If either end is an EntityReference, we may need to set IsLoaded or the DetachedEntityKey
            UpdateRelatedEnd(relatedEnd, wrappedTarget, setIsLoaded, mergeOption);
            UpdateRelatedEnd(targetRelatedEnd, wrappedSource, setIsLoaded, mergeOption);

            // In case the method was called from ObjectContext.AttachTo, we have to track relationships which were "promoted"
            // Tracked relationships are used in recovery code of AttachTo.
            if (inKeyEntryPromotion && wrappedSource.Context.ObjectStateManager.TransactionManager.IsAttachTracking)
            {
                wrappedSource.Context.ObjectStateManager.TrackPromotedRelationship(relatedEnd, wrappedTarget);
                wrappedSource.Context.ObjectStateManager.TrackPromotedRelationship(targetRelatedEnd, wrappedSource);
            }
        }

        // devnote: This method should only be used in scenarios where we are automatically hooking up relationships for
        // the user, and not in cases where they are manually setting relationships.
        private static void UpdateRelatedEnd(
            RelatedEnd relatedEnd, IEntityWrapper wrappedRelatedEntity, bool setIsLoaded, MergeOption mergeOption)
        {
            var endMember = (AssociationEndMember)(relatedEnd.ToEndMember);

            if ((endMember.RelationshipMultiplicity == RelationshipMultiplicity.One ||
                 endMember.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne))
            {
                if (setIsLoaded)
                {
                    relatedEnd.SetIsLoaded(true);
                }

                // else we just want to leave IsLoaded alone, not set it to false
                // In NoTracking cases, we want to enable the EntityReference.EntityKey property, so we have to set the key
                if (mergeOption == MergeOption.NoTracking)
                {
                    var targetKey = wrappedRelatedEntity.EntityKey;
                    if ((object)targetKey == null)
                    {
                        throw Error.EntityKey_UnexpectedNull();
                    }

                    // since endMember is not Many, relatedEnd must be an EntityReference
                    ((EntityReference)relatedEnd).DetachedEntityKey = targetKey;
                }
            }
        }

        /// <summary>
        /// Updates the relationships between a given source entity and a collection of target entities.
        /// Used for full span and related end Load methods, where the following may be true:
        /// (a) both sides of each relationship are always full entities and not stubs
        /// (b) there could be multiple entities to process at once
        /// (c) NoTracking queries are possible.
        /// Not used for relationship span because although some of the logic is similar, the above are not true.
        /// </summary>
        /// <param name="context">ObjectContext to use to look up existing relationships. Using the context here instead of ObjectStateManager because for NoTracking queries
        /// we shouldn't even touch the state manager at all, so we don't want to access it until we know we are not using NoTracking.</param>
        /// <param name="mergeOption">MergeOption to use when updating existing relationships</param>
        /// <param name="associationSet">AssociationSet for the relationships</param>
        /// <param name="sourceMember">Role of sourceEntity in associationSet</param>
        /// <param name="sourceKey">EntityKey for sourceEntity</param>
        /// <param name="sourceEntity">Source entity in the relationship</param>
        /// <param name="targetMember">Role of each targetEntity in associationSet</param>
        /// <param name="targetEntities">List of target entities to use to create relationships with sourceEntity</param>
        /// <param name="setIsLoaded">Tells whether we should allow the IsLoaded flag to be set to true for RelatedEnds</param>
        internal virtual int UpdateRelationships(
            ObjectContext context, MergeOption mergeOption, AssociationSet associationSet, AssociationEndMember sourceMember,
            IEntityWrapper wrappedSource, AssociationEndMember targetMember, IList targets, bool setIsLoaded)
        {
            return _internalObjectStateManager.UpdateRelationships(context, mergeOption, associationSet, sourceMember, wrappedSource, targetMember, targets, setIsLoaded);
        }

        /// <summary>
        /// Removes relationships if necessary when a query determines that the source entity has no relationships on the server
        /// </summary>
        /// <param name="context">ObjectContext that contains the client relationships</param>
        /// <param name="mergeOption">MergeOption to use when updating existing relationships</param>
        /// <param name="associationSet">AssociationSet for the incoming relationship</param>
        /// <param name="sourceKey">EntityKey of the source entity in the relationship</param>
        /// <param name="sourceMember">Role of the source entity in the relationship</param>
        internal virtual void RemoveRelationships(MergeOption mergeOption, AssociationSet associationSet,
            EntityKey sourceKey, AssociationEndMember sourceMember)
        {
            Debug.Assert(
                mergeOption == MergeOption.PreserveChanges || mergeOption == MergeOption.OverwriteChanges, "Unexpected MergeOption");

            _internalObjectStateManager.RemoveRelationships(mergeOption, associationSet, sourceKey, sourceMember);
        }

        /// <summary>
        /// Tries to updates one or more existing relationships for an entity, based on a given MergeOption and a target entity. 
        /// </summary>
        /// <param name="context">ObjectContext to use to look up existing relationships for sourceEntity</param>
        /// <param name="mergeOption">MergeOption to use when updating existing relationships</param>
        /// <param name="associationSet">AssociationSet for the relationship we are looking for</param>
        /// <param name="sourceMember">AssociationEndMember for the source role of the relationship</param>
        /// <param name="sourceKey">EntityKey for the source entity in the relationship (passed here so we don't have to look it up again)</param>
        /// <param name="sourceEntity">Source entity in the relationship</param>
        /// <param name="targetMember">AssociationEndMember for the target role of the relationship</param>
        /// <param name="targetKey">EntityKey for the target entity in the relationship</param>    
        /// <param name="setIsLoaded">Tells whether we should allow the IsLoaded flag to be set to true for RelatedEnds</param>
        /// <param name="newEntryState">[out] EntityState to be used for in scenarios where we need to add a new relationship after this method has returned</param>  
        /// <returns>
        /// true if an existing relationship is found and updated, and no further action is needed
        /// false if either no relationship was found, or if one was found and updated, but a new one still needs to be added
        /// </returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static bool TryUpdateExistingRelationships(
            ObjectContext context, MergeOption mergeOption, AssociationSet associationSet, AssociationEndMember sourceMember,
            EntityKey sourceKey, IEntityWrapper wrappedSource, AssociationEndMember targetMember, EntityKey targetKey, bool setIsLoaded,
            out EntityState newEntryState)
        {
            Debug.Assert(mergeOption != MergeOption.NoTracking, "Existing relationships should not be updated with NoTracking");

            // New relationships are always added as Unchanged except in specific scenarios. If there are multiple relationships being updated, and
            // at least one of those requests the new relationship to be Deleted, it should always be added as Deleted, even if there are other
            // relationships being updated that don't specify a state. Adding as Unchanged is just the default unless a scenario needs it to be Deleted to 
            // achieve a particular result.
            newEntryState = EntityState.Unchanged;
            // FK full span for tracked entities is handled entirely by FK fix up in the state manager.
            // Therefore, if the relationship is a FK, we just return indicating that nothing is to be done.
            if (associationSet.ElementType.IsForeignKey)
            {
                return true;
            }
            // Unless we find a case below where we explicitly do not want a new relationship, we should always add one to match the server.
            var needNewRelationship = true;

            var manager = context.ObjectStateManager;
            List<RelationshipEntry> entriesToDetach = null;
            List<RelationshipEntry> entriesToUpdate = null;
            foreach (var relationshipEntry in manager.FindRelationshipsByKey(sourceKey))
            {
                // We only care about relationships for the same AssociationSet and where the source entity is in the same role as it is in the incoming relationship.
                if (relationshipEntry.IsSameAssociationSetAndRole(associationSet, sourceMember, sourceKey))
                {
                    // If the other end of this relationship matches our current target entity, this relationship entry matches the server
                    if (targetKey == relationshipEntry.RelationshipWrapper.GetOtherEntityKey(sourceKey))
                    {
                        if (entriesToUpdate == null)
                        {
                            // Initial capacity is set to avoid an almost immediate resizing, which was causing a perf hit.
                            entriesToUpdate = new List<RelationshipEntry>(InitialListSize);
                        }
                        entriesToUpdate.Add(relationshipEntry);
                    }
                    else
                    {
                        // We found an existing relationship where the reference side is different on the server than what the client has.

                        // This relationship is between the same source entity and a different target, so we may need to take steps to fix up the 
                        // relationship to ensure that the client state is correct based on the requested MergeOption. 
                        // The only scenario we care about here is one where the target member has zero or one multiplicity (0..1 or 1..1), because those
                        // are the only cases where it is meaningful to say that the relationship is different on the server and the client. In scenarios
                        // where the target member has a many (*) multiplicity, it is possible to have multiple relationships between the source key
                        // and other entities, and we don't want to touch those here.
                        switch (targetMember.RelationshipMultiplicity)
                        {
                            case RelationshipMultiplicity.One:
                            case RelationshipMultiplicity.ZeroOrOne:
                                switch (mergeOption)
                                {
                                    case MergeOption.AppendOnly:
                                        if (relationshipEntry.State
                                            != EntityState.Deleted)
                                        {
                                            Debug.Assert(
                                                relationshipEntry.State == EntityState.Added
                                                || relationshipEntry.State == EntityState.Unchanged, "Unexpected relationshipEntry state");
                                            needNewRelationship = false; // adding a new relationship would conflict with the existing one
                                        }
                                        break;
                                    case MergeOption.OverwriteChanges:
                                        if (entriesToDetach == null)
                                        {
                                            // Initial capacity is set to avoid an almost immediate resizing, which was causing a perf hit.
                                            entriesToDetach = new List<RelationshipEntry>(InitialListSize);
                                        }
                                        entriesToDetach.Add(relationshipEntry);
                                        break;
                                    case MergeOption.PreserveChanges:
                                        switch (relationshipEntry.State)
                                        {
                                            case EntityState.Added:
                                                newEntryState = EntityState.Deleted;
                                                break;
                                            case EntityState.Unchanged:
                                                if (entriesToDetach == null)
                                                {
                                                    // Initial capacity is set to avoid an almost immediate resizing, which was causing a perf hit.
                                                    entriesToDetach = new List<RelationshipEntry>(InitialListSize);
                                                }
                                                entriesToDetach.Add(relationshipEntry);
                                                break;
                                            case EntityState.Deleted:
                                                newEntryState = EntityState.Deleted;
                                                if (entriesToDetach == null)
                                                {
                                                    // Initial capacity is set to avoid an almost immediate resizing, which was causing a perf hit.
                                                    entriesToDetach = new List<RelationshipEntry>(InitialListSize);
                                                }
                                                entriesToDetach.Add(relationshipEntry);
                                                break;
                                            default:
                                                Debug.Assert(false, "Unexpected relationship entry state");
                                                break;
                                        }
                                        break;
                                    default:
                                        Debug.Assert(false, "Unexpected MergeOption");
                                        break;
                                }
                                break;
                            case RelationshipMultiplicity.Many:
                                // do nothing because its okay for this source entity to have multiple different targets, so there is nothing for us to fixup
                                break;
                            default:
                                Debug.Assert(false, "Unexpected targetMember.RelationshipMultiplicity");
                                break;
                        }
                    }
                }
            }

            // Detach all of the entries that we have collected above
            if (entriesToDetach != null)
            {
                foreach (var entryToDetach in entriesToDetach)
                {
                    // the entry may have already been detached by another operation. If not, detach it now.
                    if (entryToDetach.State != EntityState.Detached)
                    {
                        RemoveRelatedEndsAndDetachRelationship(entryToDetach, setIsLoaded);
                    }
                }
            }

            // Update all of the matching entries that we have collectioned above
            if (entriesToUpdate != null)
            {
                foreach (var relationshipEntry in entriesToUpdate)
                {
                    // Don't need new relationship to be added to match the server, since we already have a match
                    needNewRelationship = false;

                    // We have an existing relationship entry that matches exactly to the incoming relationship from the server, but
                    // we may need to update it on the client based on the MergeOption and the state of the relationship entry.
                    switch (mergeOption)
                    {
                        case MergeOption.AppendOnly:
                            // AppendOnly and NoTracking shouldn't affect existing relationships, so do nothing
                            break;
                        case MergeOption.OverwriteChanges:
                            if (relationshipEntry.State == EntityState.Added)
                            {
                                relationshipEntry.AcceptChanges();
                            }
                            else if (relationshipEntry.State == EntityState.Deleted)
                            {
                                // targetEntry should always exist in this scenario because it would have
                                // at least been created when the relationship entry was created
                                var targetEntry = manager.GetEntityEntry(targetKey);

                                // If the target entity is deleted, we don't want to bring the relationship entry back.                            
                                if (targetEntry.State != EntityState.Deleted)
                                {
                                    // If the targetEntry is a KeyEntry, there are no ends to fix up.
                                    if (!targetEntry.IsKeyEntry)
                                    {
                                        AddEntityToCollectionOrReference(
                                            mergeOption,
                                            wrappedSource,
                                            sourceMember,
                                            targetEntry.WrappedEntity,
                                            targetMember,
                                            setIsLoaded: setIsLoaded,
                                            relationshipAlreadyExists: true,
                                            inKeyEntryPromotion: false);
                                    }
                                    relationshipEntry.RevertDelete();
                                }
                            }
                            // else it's already Unchanged so we don't need to do anything
                            break;
                        case MergeOption.PreserveChanges:
                            if (relationshipEntry.State == EntityState.Added)
                            {
                                // The client now matches the server, so just move the relationship to unchanged.
                                // If we don't do this and left the state Added, we will get a concurrency exception when trying to save
                                relationshipEntry.AcceptChanges();
                            }
                            // else if it's already Unchanged we don't need to do anything
                            // else if it's Deleted we want to preserve that state so do nothing
                            break;
                        default:
                            Debug.Assert(false, "Unexpected MergeOption");
                            break;
                    }
                }
            }

            return !needNewRelationship;
        }

        // Helper method to disconnect two related ends and detach their associated relationship entry
        internal static void RemoveRelatedEndsAndDetachRelationship(RelationshipEntry relationshipToRemove, bool setIsLoaded)
        {
            // If we are allowed to set the IsLoaded flag, then we can consider unloading these relationships
            if (setIsLoaded)
            {
                // If the relationship needs to be deleted, then we should unload the related ends
                UnloadReferenceRelatedEnds(relationshipToRemove);
            }

            // Delete the relationship entry and disconnect the related ends
            if (relationshipToRemove.State != EntityState.Deleted)
            {
                relationshipToRemove.Delete();
            }

            // Detach the relationship entry
            // Entries that were in the Added state prior to the Delete above will have already been Detached
            if (relationshipToRemove.State
                != EntityState.Detached)
            {
                relationshipToRemove.AcceptChanges();
            }
        }

        private static void UnloadReferenceRelatedEnds(RelationshipEntry relationshipEntry)
        {
            //Find two ends of the relationship
            var cache = relationshipEntry.ObjectStateManager;
            var endMembers = relationshipEntry.RelationshipWrapper.AssociationEndMembers;

            UnloadReferenceRelatedEnds(cache, relationshipEntry, relationshipEntry.RelationshipWrapper.GetEntityKey(0), endMembers[1].Name);
            UnloadReferenceRelatedEnds(cache, relationshipEntry, relationshipEntry.RelationshipWrapper.GetEntityKey(1), endMembers[0].Name);
        }

        private static void UnloadReferenceRelatedEnds(
            ObjectStateManager cache, RelationshipEntry relationshipEntry, EntityKey sourceEntityKey, string targetRoleName)
        {
            var entry = cache.GetEntityEntry(sourceEntityKey);

            if (entry.WrappedEntity.Entity != null)
            {
                var reference =
                    entry.WrappedEntity.RelationshipManager.GetRelatedEndInternal(
                        ((AssociationSet)relationshipEntry.EntitySet).ElementType.FullName, targetRoleName) as EntityReference;
                if (reference != null)
                {
                    reference.SetIsLoaded(false);
                }
            }
        }

        /// <summary>
        /// Attach entity in unchanged state (skip Added state, don't create temp key)
        /// It is equal (but faster) to call AddEntry(); AcceptChanges().
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entitySet"></param>
        internal EntityEntry AttachEntry(EntityKey entityKey, IEntityWrapper wrappedObject, EntitySet entitySet)
        {
            Debug.Assert(wrappedObject != null, "entity wrapper cannot be null.");
            Debug.Assert(wrappedObject.Entity != null, "entity cannot be null.");
            Debug.Assert(wrappedObject.Context != null, "the context should be already set");
            Debug.Assert(entitySet != null, "entitySet must be non-null.");
            Debug.Assert(entityKey != null, "argumentName cannot be null.");

            return _internalObjectStateManager.AttachEntry(entityKey, wrappedObject, entitySet);
        }

        internal RelationshipEntry AddNewRelation(RelationshipWrapper wrapper, EntityState desiredState)
        {
            Debug.Assert(null == FindRelationship(wrapper), "relationship should not exist, caller verifies");

            return _internalObjectStateManager.AddNewRelation(wrapper, desiredState);
        }

        internal RelationshipEntry AddRelation(RelationshipWrapper wrapper, EntityState desiredState)
        {
            Debug.Assert(
                EntityState.Added == desiredState || // result entry should be added or left alone
                EntityState.Unchanged == desiredState || // result entry should be that state
                EntityState.Deleted == desiredState, // result entry should be in that state
                "unexpected state");

            return _internalObjectStateManager.AddRelation(wrapper, desiredState);
        }

        internal RelationshipEntry FindRelationship(
            RelationshipSet relationshipSet,
            KeyValuePair<string, EntityKey> roleAndKey1,
            KeyValuePair<string, EntityKey> roleAndKey2)
        {
            return _internalObjectStateManager.FindRelationship(relationshipSet, roleAndKey1, roleAndKey2);
        }

        internal RelationshipEntry FindRelationship(RelationshipWrapper relationshipWrapper)
        {
            return _internalObjectStateManager.FindRelationship(relationshipWrapper);
        }

        /// <summary>
        /// DeleteRelationship
        /// </summary>
        /// <returns>The deleted entry</returns>
        internal RelationshipEntry DeleteRelationship(
            RelationshipSet relationshipSet,
            KeyValuePair<string, EntityKey> roleAndKey1,
            KeyValuePair<string, EntityKey> roleAndKey2)
        {
            return _internalObjectStateManager.DeleteRelationship(relationshipSet, roleAndKey1, roleAndKey2);
        }

        /// <summary>
        /// DeleteKeyEntry
        /// </summary>
        internal void DeleteKeyEntry(EntityEntry keyEntry)
        {
            _internalObjectStateManager.DeleteKeyEntry(keyEntry);
        }

        /// <summary>
        /// Finds all relationships with the given key at one end.
        /// </summary>
        internal RelationshipEntry[] CopyOfRelationshipsByKey(EntityKey key)
        {
            return FindRelationshipsByKey(key).ToArray();
        }

        /// <summary>
        /// Finds all relationships with the given key at one end.
        /// Do not use the list to add elements
        /// </summary>
        internal EntityEntry.RelationshipEndEnumerable FindRelationshipsByKey(EntityKey key)
        {
            return new EntityEntry.RelationshipEndEnumerable(FindEntityEntry(key));
        }

        IEnumerable<IEntityStateEntry> IEntityStateManager.FindRelationshipsByKey(EntityKey key)
        {
            return FindRelationshipsByKey(key);
        }

        /// <summary>
        /// Returns all CacheEntries in the given state.
        /// </summary>
        /// <exception cref="ArgumentException">if EntityState.Detached flag is set in state</exception>
        public IEnumerable<ObjectStateEntry> GetObjectStateEntries(EntityState state)
        {
            return _internalObjectStateManager.GetObjectStateEntries(state);
        }

        /// <summary>
        /// Returns all CacheEntries in the given state.
        /// </summary>
        /// <exception cref="ArgumentException">if EntityState.Detached flag is set in state</exception>
        IEnumerable<IEntityStateEntry> IEntityStateManager.GetEntityStateEntries(EntityState state)
        {
            Debug.Assert((EntityState.Detached & state) == 0, "Cannot get state entries for detached entities");
            foreach (var stateEntry in _internalObjectStateManager.GetObjectStateEntriesInternal(state))
            {
                yield return stateEntry;
            }
        }

        internal int GetObjectStateEntriesCount(EntityState state)
        {
            return _internalObjectStateManager.GetObjectStateEntriesCount(state);
        }

        /// <summary>
        /// Performs key-fixup on the given entry, by creating a (permanent) EntityKey
        /// based on the current key values within the associated entity and fixing up
        /// all associated relationship entries.
        /// </summary>
        /// <remarks>
        /// Will promote EntityEntry.IsKeyEntry and leave in _unchangedStore
        /// otherwise will move EntityEntry from _addedStore to _unchangedStore.
        /// </remarks>
        internal void FixupKey(EntityEntry entry)
        {
            Debug.Assert(entry != null, "entry should not be null.");
            Debug.Assert(entry.State == EntityState.Added, "Cannot do key fixup for an entry not in the Added state.");
            Debug.Assert(entry.Entity != null, "must have entity, can't be entity stub in added state");

            _internalObjectStateManager.FixupKey(entry);
        }

        /// <summary>
        /// Replaces permanent EntityKey with a temporary key.  Used in N-Tier API.
        /// </summary>
        internal void ReplaceKeyWithTemporaryKey(EntityEntry entry)
        {
            Debug.Assert(entry != null, "entry should not be null.");
            Debug.Assert(entry.State != EntityState.Added, "Cannot replace key with a temporary key if the entry is in Added state.");
            Debug.Assert(!entry.IsKeyEntry, "Cannot replace a key of a KeyEntry");

            _internalObjectStateManager.ReplaceKeyWithTemporaryKey(entry);
        }

        /// <summary>
        /// Finds an ObjectStateEntry for the given entity and changes its state to the new state.
        /// The operation does not trigger cascade deletion.
        /// The operation may change state of adjacent relationships.
        /// </summary>
        /// <param name="entity">entity which state should be changed</param>
        /// <param name="entityState">new state of the entity</param>
        /// <returns>entry associated with entity</returns>
        public ObjectStateEntry ChangeObjectState(object entity, EntityState entityState)
        {
            Contract.Requires(entity != null);
            EntityUtil.CheckValidStateForChangeEntityState(entityState);

            return _internalObjectStateManager.ChangeObjectState(entity, entityState);
        }

        /// <summary>
        /// Changes state of a relationship between two entities. 
        /// </summary>
        /// <remarks>
        /// Both entities must be already tracked by the ObjectContext.
        /// </remarks>
        /// <param name="sourceEntity">The instance of the source entity or the EntityKey of the source entity</param>
        /// <param name="targetEntity">The instance of the target entity or the EntityKey of the target entity</param>
        /// <param name="navigationProperty">The name of the navigation property on the source entity</param>
        /// <param name="relationshipState">The requested state of the relationship</param>
        /// <returns>The ObjectStateEntry for changed relationship</returns>
        public ObjectStateEntry ChangeRelationshipState(
            object sourceEntity,
            object targetEntity,
            string navigationProperty,
            EntityState relationshipState)
        {
            return _internalObjectStateManager.ChangeRelationshipState(sourceEntity, targetEntity, navigationProperty, relationshipState);
        }

        /// <summary>
        /// Changes state of a relationship between two entities.
        /// </summary>
        /// <remarks>
        /// Both entities must be already tracked by the ObjectContext.
        /// </remarks>
        /// <param name="sourceEntity">The instance of the source entity or the EntityKey of the source entity</param>
        /// <param name="targetEntity">The instance of the target entity or the EntityKey of the target entity</param>
        /// <param name="navigationPropertySelector">A LINQ expression specifying the navigation property</param>
        /// <param name="relationshipState">The requested state of the relationship</param>
        /// <returns>The ObjectStateEntry for changed relationship</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ObjectStateEntry ChangeRelationshipState<TEntity>(
            TEntity sourceEntity,
            object targetEntity,
            Expression<Func<TEntity, object>> navigationPropertySelector,
            EntityState relationshipState) where TEntity : class
        {
            return _internalObjectStateManager.ChangeRelationshipState<TEntity>(sourceEntity, targetEntity, navigationPropertySelector, relationshipState);
        }

        /// <summary>
        /// Changes state of a relationship between two entities.
        /// </summary>
        /// <remarks>
        /// Both entities must be already tracked by the ObjectContext.
        /// </remarks>
        /// <param name="sourceEntity">The instance of the source entity or the EntityKey of the source entity</param>
        /// <param name="targetEntity">The instance of the target entity or the EntityKey of the target entity</param>
        /// <param name="relationshipName">The name of relationship</param>
        /// <param name="targetRoleName">The target role name of the relationship</param>
        /// <param name="relationshipState">The requested state of the relationship</param>
        /// <returns>The ObjectStateEntry for changed relationship</returns>
        public ObjectStateEntry ChangeRelationshipState(
            object sourceEntity,
            object targetEntity,
            string relationshipName,
            string targetRoleName,
            EntityState relationshipState)
        {
            return _internalObjectStateManager.ChangeRelationshipState(sourceEntity, targetEntity, relationshipName, targetRoleName, relationshipState);
        }

        /// <summary>
        /// Retrieve the corresponding IEntityStateEntry for the given EntityKey.
        /// </summary>
        /// <exception cref="ArgumentNullException">if key is null</exception>
        /// <exception cref="ArgumentException">if key is not found</exception>
        IEntityStateEntry IEntityStateManager.GetEntityStateEntry(EntityKey key)
        {
            return GetEntityEntry(key);
        }

        /// <summary>
        /// Retrieve the corresponding ObjectStateEntry for the given EntityKey.
        /// </summary>
        /// <exception cref="ArgumentNullException">if key is null</exception>
        /// <exception cref="ArgumentException">if key is not found</exception>
        public ObjectStateEntry GetObjectStateEntry(EntityKey key)
        {
            return _internalObjectStateManager.GetObjectStateEntry(key);
        }

        internal EntityEntry GetEntityEntry(EntityKey key)
        {
            return _internalObjectStateManager.GetEntityEntry(key); 
        }

        /// <summary>
        /// Given an entity, of type object, return the corresponding ObjectStateEntry.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>The corresponding ObjectStateEntry for this object.</returns>
        public ObjectStateEntry GetObjectStateEntry(object entity)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            return _internalObjectStateManager.GetObjectStateEntry(entity);
        }

        internal EntityEntry GetEntityEntry(object entity)
        {
            Debug.Assert(entity != null, "entity is null");
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");

            return _internalObjectStateManager.GetEntityEntry(entity);
        }

        /// <summary>
        /// Retrieve the corresponding ObjectStateEntry for the given object.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entry"></param>
        /// <returns>true if the corresponding ObjectStateEntry was found</returns>
        public bool TryGetObjectStateEntry(object entity, out ObjectStateEntry entry)
        {
            Contract.Requires(entity != null);
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            
            return _internalObjectStateManager.TryGetObjectStateEntry(entity, out entry);
        }

        /// <summary>
        /// Retrieve the corresponding IEntityStateEntry for the given EntityKey.
        /// </summary>
        /// <returns>true if the corresponding IEntityStateEntry was found</returns>
        /// <exception cref="ArgumentNullException">if key is null</exception>
        bool IEntityStateManager.TryGetEntityStateEntry(EntityKey key, out IEntityStateEntry entry)
        {
            // Because the passed in IEntityStateEntry reference isn't necessarily an
            // ObjectStateEntry, we have to declare our own local copy, use it for the outparam of
            // TryGetObjectStateEntry, and then set it onto our outparam if we successfully find
            // something (at that point we know we can cast to IEntityStateEntry), but we just can't
            // cast in the other direction.
            ObjectStateEntry objectStateEntry;
            var result = TryGetObjectStateEntry(key, out objectStateEntry);
            entry = objectStateEntry;
            return result;
        }

        /// <summary>
        /// Given a key that represents an entity on the dependent side of a FK, this method attempts to return the key of the
        /// entity on the principal side of the FK.  If the two entities both exist in the context, then the primary key of
        /// the principal entity is found and returned.  If the principal entity does not exist in the context, then a key
        /// for it is built up from the foreign key values contained in the dependent entity.
        /// </summary>
        /// <param name="dependentKey">The key of the dependent entity</param>
        /// <param name="principalRole">The role indicating the FK to navigate</param>
        /// <param name="principalKey">Set to the principal key or null on return</param>
        /// <returns>True if the principal key was found or built; false if it could not be found or built</returns>
        bool IEntityStateManager.TryGetReferenceKey(EntityKey dependentKey, AssociationEndMember principalRole, out EntityKey principalKey)
        {
            EntityEntry dependentEntry;
            if (!TryGetEntityEntry(dependentKey, out dependentEntry))
            {
                principalKey = null;
                return false;
            }

            return dependentEntry.TryGetReferenceKey(principalRole, out principalKey);
        }

        /// <summary>
        /// Retrieve the corresponding ObjectStateEntry for the given EntityKey.
        /// </summary>
        /// <returns>true if the corresponding ObjectStateEntry was found</returns>
        /// <exception cref="ArgumentNullException">if key is null</exception>
        public bool TryGetObjectStateEntry(EntityKey key, out ObjectStateEntry entry)
        {
            return _internalObjectStateManager.TryGetObjectStateEntry(key, out entry);
        }

        internal bool TryGetEntityEntry(EntityKey key, out EntityEntry entry)
        {
            Contract.Requires(key != null);

            return _internalObjectStateManager.TryGetEntityEntry(key, out entry);
        }

        internal EntityEntry FindEntityEntry(EntityKey key)
        {
            return _internalObjectStateManager.FindEntityEntry(key);
        }

        /// <summary>
        /// Retrieve the corresponding EntityEntry for the given entity.
        /// Returns null if key is unavailable or passed entity is null.
        /// </summary>
        internal EntityEntry FindEntityEntry(object entity)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            Debug.Assert(!(entity is EntityKey), "Object is a EntityKey instead of raw entity.");

            return _internalObjectStateManager.FindEntityEntry(entity);
        }

        /// <summary>
        /// Gets a RelationshipManager for the given entity.  For entities that implement IEntityWithRelationships,
        /// the RelationshipManager is obtained through that interface.  For other types of entity, the RelationshipManager
        /// that is being tracked internally is returned.  This means that a RelationshipManager for an entity that
        /// does not implement IEntityWithRelationships can only be obtained if the entity is being tracked by the
        /// ObjectStateManager.
        /// Note that all code generated entities that inherit from EntityObject automatically implement IEntityWithRelationships.
        /// </summary>
        /// <param name="entity">The entity for which to return a RelationshipManager</param>
        /// <returns>The RelationshipManager</returns>
        /// <exception cref="InvalidOperationException">The entity does not implement IEntityWithRelationships and is not tracked by this ObjectStateManager</exception>
        public RelationshipManager GetRelationshipManager(object entity)
        {
            return _internalObjectStateManager.GetRelationshipManager(entity);
        }

        /// <summary>
        /// Gets a RelationshipManager for the given entity.  For entities that implement IEntityWithRelationships,
        /// the RelationshipManager is obtained through that interface.  For other types of entity, the RelationshipManager
        /// that is being tracked internally is returned.  This means that a RelationshipManager for an entity that
        /// does not implement IEntityWithRelationships can only be obtained if the entity is being tracked by the
        /// ObjectStateManager.
        /// Note that all code generated entities that inherit from EntityObject automatically implement IEntityWithRelationships.
        /// </summary>
        /// <param name="entity">The entity for which to return a RelationshipManager</param>
        /// <param name="relationshipManager">The RelationshipManager, or null if none was found</param>
        /// <returns>True if a RelationshipManager was found; false if The entity does not implement IEntityWithRelationships and is not tracked by this ObjectStateManager</returns>
        public bool TryGetRelationshipManager(object entity, out RelationshipManager relationshipManager)
        {
            Contract.Requires(entity != null);

            return _internalObjectStateManager.TryGetRelationshipManager(entity, out relationshipManager);
        }

        internal void ChangeState(RelationshipEntry entry, EntityState oldState, EntityState newState)
        {
            _internalObjectStateManager.ChangeState(entry, oldState, newState);
        }

        internal void ChangeState(EntityEntry entry, EntityState oldState, EntityState newState)
        {
            _internalObjectStateManager.ChangeState(entry, oldState, newState);
        }

        internal void RemoveEntryFromKeylessStore(IEntityWrapper wrappedEntity)
        {
            _internalObjectStateManager.RemoveEntryFromKeylessStore(wrappedEntity);
        }

        /// <summary>
        /// If a corresponding StateManagerTypeMetadata exists, it is returned.
        /// Otherwise, a StateManagerTypeMetadata is created and cached.
        /// </summary>
        internal StateManagerTypeMetadata GetOrAddStateManagerTypeMetadata(Type entityType, EntitySet entitySet)
        {
            Debug.Assert(entityType != null, "entityType cannot be null.");
            Debug.Assert(entitySet != null, "must have entitySet to correctly qualify Type");

            return _internalObjectStateManager.GetOrAddStateManagerTypeMetadata(entityType, entitySet);
        }

        /// <summary>
        /// If a corresponding StateManagerTypeMetadata exists, it is returned.
        /// Otherwise, a StateManagerTypeMetadata is created and cached.
        /// </summary>
        internal StateManagerTypeMetadata GetOrAddStateManagerTypeMetadata(EdmType edmType)
        {
            Debug.Assert(edmType != null, "edmType cannot be null.");
            Debug.Assert(
                Helper.IsEntityType(edmType) ||
                Helper.IsComplexType(edmType),
                "only expecting ComplexType or EntityType");

            return _internalObjectStateManager.GetOrAddStateManagerTypeMetadata(edmType);
        }

        /// <summary>
        /// Mark the ObjectStateManager as disposed
        /// </summary>
        internal void Dispose()
        {
            _internalObjectStateManager.Dispose();
            _isDisposed = true;
        }

        internal bool IsDisposed
        {
            get { return _isDisposed; }
        }

        /// <summary>
        /// For every tracked entity which doesn't implement IEntityWithChangeTracker detect changes in the entity's property values
        /// and marks appropriate ObjectStateEntry as Modified.
        /// For every tracked entity which doesn't implement IEntityWithRelationships detect changes in its relationships.
        /// 
        /// The method is used internally by ObjectContext.SaveChanges() but can be also used if user wants to detect changes 
        /// and have ObjectStateEntries in appropriate state before the SaveChanges() method is called.
        /// </summary>
        internal void DetectChanges()
        {
            _internalObjectStateManager.DetectChanges();
        }

        internal EntityKey GetPermanentKey(IEntityWrapper entityFrom, RelatedEnd relatedEndFrom, IEntityWrapper entityTo)
        {
            return _internalObjectStateManager.GetPermanentKey(entityFrom, relatedEndFrom, entityTo);
        }

        internal EntityKey CreateEntityKey(EntitySet entitySet, object entity)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            Debug.Assert(entitySet != null, "null entitySet");
            Debug.Assert(entity != null, "null entity");

            return _internalObjectStateManager.CreateEntityKey(entitySet, entity);
        }
    }
}
