// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Maintains object state and identity management for entity type instances and relationship instances.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class ObjectStateManager : IEntityStateManager
    {
        // This is the initial capacity used for lists of entries.  We use this rather than the default because
        // perf testing showed we were almost always increasing the capacity which can be quite a slow operation.
        private const int InitialListSize = 16;

        // dictionaries (one for each entity state) that store cache entries that represent entities
        // these are only non-null when there is an entity in respective state, must always check for null before using
        private Dictionary<EntityKey, EntityEntry> _addedEntityStore;
        private Dictionary<EntityKey, EntityEntry> _modifiedEntityStore;
        private Dictionary<EntityKey, EntityEntry> _deletedEntityStore;
        private Dictionary<EntityKey, EntityEntry> _unchangedEntityStore;
        private Dictionary<object, EntityEntry> _keylessEntityStore;

        // dictionaries (one for each entity state) that store cache entries that represent relationships
        // these are only non-null when there is an relationship in respective state, must always check for null before using
        private Dictionary<RelationshipWrapper, RelationshipEntry> _addedRelationshipStore;
        private Dictionary<RelationshipWrapper, RelationshipEntry> _deletedRelationshipStore;
        private Dictionary<RelationshipWrapper, RelationshipEntry> _unchangedRelationshipStore;

        // mapping from EdmType or EntitySetQualifiedType to StateManagerTypeMetadata
        private readonly Dictionary<EdmType, StateManagerTypeMetadata> _metadataStore;
        private readonly Dictionary<EntitySetQualifiedType, StateManagerTypeMetadata> _metadataMapping;

        private readonly MetadataWorkspace _metadataWorkspace;

        // delegate for notifying changes in collection
        private CollectionChangeEventHandler onObjectStateManagerChangedDelegate;
        private CollectionChangeEventHandler onEntityDeletedDelegate;

        // Flag to indicate if we are in the middle of relationship fixup.
        // This is set and cleared only during ResetEntityKey, because only in that case
        // do we allow setting a value on a non-null EntityKey property
        private bool _inRelationshipFixup;

        private bool _isDisposed;

        // materializer instance that can be used to create complex types with just a metadata workspace
        private ComplexTypeMaterializer _complexTypeMaterializer;

        private readonly Dictionary<EntityKey, HashSet<Tuple<EntityReference, EntityEntry>>> _danglingForeignKeys =
            new Dictionary<EntityKey, HashSet<Tuple<EntityReference, EntityEntry>>>();

        private HashSet<EntityEntry> _entriesWithConceptualNulls;

        private readonly EntityWrapperFactory _entityWrapperFactory;

        #region Private Fields for ObjectStateEntry change tracking

        private bool _detectChangesNeeded;

        #endregion

        internal ObjectStateManager()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.Objects.ObjectStateManager" /> class.
        /// </summary>
        /// <param name="metadataWorkspace">
        /// The <see cref="T:System.Data.Entity.Core.Metadata.Edm.MetadataWorkspace" />, which supplies mapping and metadata information.
        /// </param>
        public ObjectStateManager(MetadataWorkspace metadataWorkspace)
        {
            Check.NotNull(metadataWorkspace, "metadataWorkspace");
            _metadataWorkspace = metadataWorkspace;

            _metadataStore = new Dictionary<EdmType, StateManagerTypeMetadata>();
            _metadataMapping = new Dictionary<EntitySetQualifiedType, StateManagerTypeMetadata>(EntitySetQualifiedType.EqualityComparer);
            _isDisposed = false;
            _entityWrapperFactory = new EntityWrapperFactory();
            TransactionManager = new TransactionManager();
        }

        #region Internal Properties for ObjectStateEntry change tracking

        internal virtual object ChangingObject { get; set; }

        internal virtual string ChangingEntityMember { get; set; }

        internal virtual string ChangingMember { get; set; }

        internal virtual EntityState ChangingState { get; set; }

        internal virtual bool SaveOriginalValues { get; set; }

        internal virtual object ChangingOldValue { get; set; }

        // Used by ObjectStateEntry to determine if it's safe to set a value
        // on a non-null IEntity.EntityKey property
        internal virtual bool InRelationshipFixup
        {
            get { return _inRelationshipFixup; }
        }

        internal virtual ComplexTypeMaterializer ComplexTypeMaterializer
        {
            get
            {
                if (_complexTypeMaterializer == null)
                {
                    _complexTypeMaterializer = new ComplexTypeMaterializer(MetadataWorkspace);
                }
                return _complexTypeMaterializer;
            }
        }

        #endregion

        internal virtual TransactionManager TransactionManager { get; private set; }

        internal virtual EntityWrapperFactory EntityWrapperFactory
        {
            get { return _entityWrapperFactory; }
        }

        /// <summary>
        /// Gets the <see cref="T:System.Data.Entity.Core.Metadata.Edm.MetadataWorkspace" /> associated with this state manager.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Metadata.Edm.MetadataWorkspace" /> associated with this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectStateManager" />
        /// .
        /// </returns>
        public virtual MetadataWorkspace MetadataWorkspace
        {
            get { return _metadataWorkspace; }
        }

        #region events ObjectStateManagerChanged / EntityDeleted

        /// <summary>Occurs when entities are added to or removed from the state manager.</summary>
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

        internal virtual void OnObjectStateManagerChanged(CollectionChangeAction action, object entity)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            if (onObjectStateManagerChangedDelegate != null)
            {
                onObjectStateManagerChangedDelegate(this, new CollectionChangeEventArgs(action, entity));
            }
        }

        private void OnEntityDeleted(CollectionChangeAction action, object entity)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            if (onEntityDeletedDelegate != null)
            {
                onEntityDeletedDelegate(this, new CollectionChangeEventArgs(action, entity));
            }
        }

        #endregion

        // <summary>
        // Adds an object stub to the cache.
        // </summary>
        // <param name="entityKey"> the key of the object to add </param>
        // <param name="entitySet"> the entity set of the given object </param>
        internal virtual EntityEntry AddKeyEntry(EntityKey entityKey, EntitySet entitySet)
        {
            DebugCheck.NotNull(entityKey);
            DebugCheck.NotNull(entitySet);

            // We need to determine if an equivalent entry already exists;
            // this is illegal in certain cases.
            var entry = FindEntityEntry(entityKey);
            if (entry != null)
            {
                throw new InvalidOperationException(
                    Strings.ObjectStateManager_ObjectStateManagerContainsThisEntityKey(entitySet.ElementType.Name));
            }

            return InternalAddEntityEntry(entityKey, entitySet);
        }

        internal EntityEntry GetOrAddKeyEntry(EntityKey entityKey, EntitySet entitySet)
        {
            DebugCheck.NotNull(entityKey);
            DebugCheck.NotNull(entitySet);

            EntityEntry entry;
            if (TryGetEntityEntry(entityKey, out entry))
            {
                return entry;
            }

            return InternalAddEntityEntry(entityKey, entitySet);
        }

        private EntityEntry InternalAddEntityEntry(EntityKey entityKey, EntitySet entitySet)
        {
            // Get a StateManagerTypeMetadata for the entity type.
            var typeMetadata = GetOrAddStateManagerTypeMetadata(entitySet.ElementType);

            // Create a cache entry.
            var entry = new EntityEntry(entityKey, entitySet, this, typeMetadata);

            // A new entity is being added.
            AddEntityEntryToDictionary(entry, entry.State);

            return entry;
        }

        // <summary>
        // Validates that the proxy type being attached to the context matches the proxy type
        // that would be generated for the given CLR type for the currently loaded metadata.
        // This prevents a proxy for one set of metadata being incorrectly loaded into a context
        // which has different metadata.
        // </summary>
        private void ValidateProxyType(IEntityWrapper wrappedEntity)
        {
            var identityType = wrappedEntity.IdentityType;
            var actualType = wrappedEntity.Entity.GetType();
            if (identityType != actualType)
            {
                var entityType = MetadataWorkspace.GetItem<ClrEntityType>(identityType.FullNameWithNesting(), DataSpace.OSpace);
                var proxyTypeInfo = EntityProxyFactory.GetProxyType(entityType, MetadataWorkspace);
                if (proxyTypeInfo == null
                    || proxyTypeInfo.ProxyType != actualType)
                {
                    throw new InvalidOperationException(Strings.EntityProxyTypeInfo_DuplicateOSpaceType(identityType.FullName));
                }
            }
        }

        // <summary>
        // Adds an object to the ObjectStateManager.
        // </summary>
        // <param name="wrappedObject"> the object to add </param>
        // <param name="entitySet"> the entity set of the given object </param>
        // <param name="argumentName"> Name of the argument passed to a public method, for use in exceptions. </param>
        // <param name="isAdded"> Indicates whether the entity is added or unchanged. </param>
        internal virtual EntityEntry AddEntry(
            IEntityWrapper wrappedObject, EntityKey passedKey, EntitySet entitySet, string argumentName, bool isAdded)
        {
            DebugCheck.NotNull(wrappedObject);
            DebugCheck.NotNull(wrappedObject.Entity);
            DebugCheck.NotNull(wrappedObject.Context);
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(argumentName);

            var entityKey = passedKey;

            // Get a StateManagerTypeMetadata for the entity type.
            var typeMetadata = GetOrAddStateManagerTypeMetadata(wrappedObject.IdentityType, entitySet);

            ValidateProxyType(wrappedObject);

            // dataObject's type should match to type that can be contained by the entityset
            var entityEdmType = typeMetadata.CdmMetadata.EdmType;
            //OC Mapping will make sure that non-abstract type in O space is always mapped to a non-abstract type in C space
            Debug.Assert(!entityEdmType.Abstract, "non-abstract type in O space is mapped to abstract type in C space");
            if ((isAdded)
                && !entitySet.ElementType.IsAssignableFrom(entityEdmType))
            {
                throw new ArgumentException(
                    Strings.ObjectStateManager_EntityTypeDoesnotMatchtoEntitySetType(
                        wrappedObject.Entity.GetType().Name, TypeHelpers.GetFullName(entitySet.EntityContainer.Name, entitySet.Name)),
                    argumentName);
            }

            EntityKey dataObjectEntityKey = null;
            if (isAdded)
            {
                dataObjectEntityKey = wrappedObject.GetEntityKeyFromEntity();
            }
            else
            {
                dataObjectEntityKey = wrappedObject.EntityKey;
            }
#if DEBUG
            if ((object)dataObjectEntityKey != null
                && (object)entityKey != null)
            {
                Debug.Assert(dataObjectEntityKey == entityKey, "The passed key and the key on dataObject must match.");
            }
#endif
            if (null != (object)dataObjectEntityKey)
            {
                entityKey = dataObjectEntityKey;
                // These two checks verify that entityWithKey.EntityKey implemented by the user on a (I)POCO entity returns what it was given.
                if ((object)entityKey == null)
                {
                    throw new InvalidOperationException(Strings.EntityKey_UnexpectedNull);
                }
                if (wrappedObject.EntityKey != entityKey)
                {
                    throw new InvalidOperationException(Strings.EntityKey_DoesntMatchKeyOnEntity(wrappedObject.Entity.GetType().FullName));
                }
            }

            if ((object)entityKey != null
                && !entityKey.IsTemporary
                && !isAdded)
            {
                // If the entity already has a permanent key, and we were invoked
                // from the materializer, check that the key is correct.  We don't check
                // for temporary keys because temporary keys don't contain values.
                CheckKeyMatchesEntity(wrappedObject, entityKey, entitySet, /*forAttach*/ false);
            }

            // We need to determine if an equivalent entry already exists; this is illegal
            // in certain cases.
            EntityEntry existingEntry;
            if ((isAdded)
                &&
                ((dataObjectEntityKey == null && (null != (existingEntry = FindEntityEntry(wrappedObject.Entity)))) ||
                 (dataObjectEntityKey != null && (null != (existingEntry = FindEntityEntry(dataObjectEntityKey))))))
            {
                if (existingEntry.Entity
                    != wrappedObject.Entity)
                {
                    throw new InvalidOperationException(
                        Strings.ObjectStateManager_ObjectStateManagerContainsThisEntityKey(wrappedObject.IdentityType.FullName));
                }
                // key does exist but entity is the same, it is being re-added ;
                // no-op when Add(entity)
                // NOTE we don't want to re-add entities in other then Added state
                if (existingEntry.State
                    != EntityState.Added) // (state == DataRowState.Unchanged && state == DataRowState.Modified)
                {
                    throw new InvalidOperationException(
                        Strings.ObjectStateManager_DoesnotAllowToReAddUnchangedOrModifiedOrDeletedEntity(existingEntry.State));
                }

                // no-op
                return null;
            }
            else
            {
                // Neither entityWithKey.EntityKey nor the passed entityKey were non-null, or
                // If the entity doesn't already exist in the state manager
                // and we intend to put the entity in the Added state (i.e.,
                // AddEntry() was invoked from ObjectContext.AddObject()),
                // the entity's key must be set to a new temp key.
                if ((object)entityKey == null
                    || (isAdded && !entityKey.IsTemporary))
                {
                    // If the entity does not have a key, create and add a temporary key.
                    entityKey = new EntityKey(entitySet);
                    wrappedObject.EntityKey = entityKey;
                }

                if (!wrappedObject.OwnsRelationshipManager)
                {
                    // When a POCO instance is added or attached, we need to ignore the contents 
                    // of the RelationshipManager as it is out-of-date with the POCO nav props
                    wrappedObject.RelationshipManager.ClearRelatedEndWrappers();
                }

                // Create a cache entry.
                var newEntry = new EntityEntry(
                    wrappedObject, entityKey, entitySet, this, typeMetadata, isAdded ? EntityState.Added : EntityState.Unchanged);

                //Verify that the entityKey is set correctly--also checks entry.EK and entity.EK internally
                Debug.Assert(entityKey == newEntry.EntityKey, "The key on the new entry was not set correctly");

                // ObjectMaterializer will have already determined the existing entry doesn't exist
                Debug.Assert(null == FindEntityEntry(entityKey), "should not have existing entry");

                // A new entity is being added.
                newEntry.AttachObjectStateManagerToEntity();
                AddEntityEntryToDictionary(newEntry, newEntry.State);

                // fire ColectionChanged event  only when a new entity is added to cache
                OnObjectStateManagerChanged(CollectionChangeAction.Add, newEntry.Entity);

                // When adding, we do this in AddSingleObject since we don't want to do it before the context is attached.
                if (!isAdded)
                {
                    FixupReferencesByForeignKeys(newEntry);
                }

                return newEntry;
            }
        }

        internal virtual void FixupReferencesByForeignKeys(EntityEntry newEntry, bool replaceAddedRefs = false)
        {
            // Perf optimization to avoid all this work if the entity doesn't participate in any FK relationships
            if (!((EntitySet)newEntry.EntitySet).HasForeignKeyRelationships)
            {
                return;
            }

            // Look at the foreign keys contained in this entry and perform fixup to the entities that
            // they reference, or add the key and this entry to the index of foreign keys that reference
            // entities that we don't yet know about.
            newEntry.FixupReferencesByForeignKeys(replaceAddedRefs);
            // Lookup the key for this entry and find all other entries that reference this entry using
            // foreign keys.  Perform fixup between the two entries.
            foreach (var foundEntry in GetNonFixedupEntriesContainingForeignKey(newEntry.EntityKey))
            {
                foundEntry.FixupReferencesByForeignKeys(replaceAddedRefs: false, restrictTo: newEntry.EntitySet);
            }
            // Once we have done fixup for this entry we don't need the entries in the index anymore
            RemoveForeignKeyFromIndex(newEntry.EntityKey);
        }

        // <summary>
        // Adds an entry to the index of foreign keys that reference entities that we don't yet know about.
        // </summary>
        // <param name="foreignKey"> The foreign key found in the entry </param>
        // <param name="entry"> The entry that contains the foreign key that was found </param>
        internal virtual void AddEntryContainingForeignKeyToIndex(EntityReference relatedEnd, EntityKey foreignKey, EntityEntry entry)
        {
            HashSet<Tuple<EntityReference, EntityEntry>> danglingEntries;
            if (!_danglingForeignKeys.TryGetValue(foreignKey, out danglingEntries))
            {
                danglingEntries = new HashSet<Tuple<EntityReference, EntityEntry>>();
                _danglingForeignKeys.Add(foreignKey, danglingEntries);
            }
            Debug.Assert(entry.ObjectStateManager != null, "Attempt to add detached state entry to dangling keys");
            danglingEntries.Add(Tuple.Create(relatedEnd, entry));
        }

        [Conditional("DEBUG")]
        internal virtual void AssertEntryDoesNotExistInForeignKeyIndex(EntityEntry entry)
        {
            foreach (var dFkEntry in _danglingForeignKeys.SelectMany(kv => kv.Value))
            {
                if (!(dFkEntry.Item2.State == EntityState.Detached || entry.State == EntityState.Detached))
                {
                    Debug.Assert(
                        dFkEntry.Item2.EntityKey == null || entry.EntityKey == null ||
                        (dFkEntry.Item2.EntityKey != entry.EntityKey && dFkEntry.Item2 != entry),
                        string.Format(
                            CultureInfo.InvariantCulture, "The entry references {0} equal. dFkEntry={1}, entry={2}",
                            dFkEntry.Item2 == entry ? "are" : "are not", dFkEntry.Item2.EntityKey.ConcatKeyValue(), entry.EntityKey.ConcatKeyValue()));
                }
            }
        }

        [Conditional("DEBUG")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults",
            Justification = "This method is compiled only when the compilation symbol DEBUG is defined")]
        internal virtual void AssertAllForeignKeyIndexEntriesAreValid()
        {
            // These checks are most useful when running the test suite where the number of entities is generally very
            // small. However, when running a debug build with many entities this code can cause significant perf issues,
            // so we disable it to avoid the perf issues. See CodePlex 1724.
            if (GetMaxEntityEntriesForDetectChanges() > 100)
            {
                return;
            }

            var validEntries = new HashSet<ObjectStateEntry>(GetObjectStateEntriesInternal(~EntityState.Detached));
            foreach (var entry in _danglingForeignKeys.SelectMany(kv => kv.Value))
            {
                Debug.Assert(entry.Item2._cache != null, "found an entry in the _danglingForeignKeys collection that has been nulled out");
                Debug.Assert(
                    validEntries.Contains(entry.Item2),
                    "The entry in the dangling foreign key store is no longer in the ObjectStateManager. Key="
                    +
                    (entry.Item2.State == EntityState.Detached ? "detached" : entry.Item2.EntityKey != null ? "null" : entry.Item2.EntityKey.ConcatKeyValue()));
                Debug.Assert(
                    entry.Item2.State == EntityState.Detached || !ForeignKeyFactory.IsConceptualNullKey(entry.Item2.EntityKey),
                    "Found an entry with conceptual null Key=" + entry.Item2.EntityKey.ConcatKeyValue());
            }
        }

        // <summary>
        // Removes an entry to the index of foreign keys that reference entities that we don't yet know about.
        // This is typically done when the entity is detached from the context.
        // </summary>
        // <param name="foreignKey"> The foreign key found in the entry </param>
        // <param name="entry"> The entry that contains the foreign key that was found </param>
        internal virtual void RemoveEntryFromForeignKeyIndex(EntityReference relatedEnd, EntityKey foreignKey, EntityEntry entry)
        {
            HashSet<Tuple<EntityReference, EntityEntry>> danglingEntries;
            if (_danglingForeignKeys.TryGetValue(foreignKey, out danglingEntries))
            {
                danglingEntries.Remove(Tuple.Create(relatedEnd, entry));
            }
        }

        // <summary>
        // Removes the foreign key from the index of those keys that have been found in entries
        // but for which it was not possible to do fixup because the entity that the foreign key
        // referenced was not in the state manager.
        // </summary>
        // <param name="foreignKey"> The key to lookup and remove </param>
        internal virtual void RemoveForeignKeyFromIndex(EntityKey foreignKey)
        {
            _danglingForeignKeys.Remove(foreignKey);
        }

        // <summary>
        // Gets all state entries that contain the given foreign key for which we have not performed
        // fixup because the state manager did not contain the entity to which the foreign key pointed.
        // </summary>
        // <param name="foreignKey"> The key to lookup </param>
        // <returns> The state entries that contain the key </returns>
        internal virtual IEnumerable<EntityEntry> GetNonFixedupEntriesContainingForeignKey(EntityKey foreignKey)
        {
            HashSet<Tuple<EntityReference, EntityEntry>> foundEntries;
            if (_danglingForeignKeys.TryGetValue(foreignKey, out foundEntries))
            {
                // these entries will be updated by the code consuming them, so 
                // create a stable container to iterate over.
                return foundEntries.Select(e => e.Item2).ToList();
            }
            return Enumerable.Empty<EntityEntry>();
        }

        // <summary>
        // Adds to index of currently tracked entities that have FK values that are conceptually
        // null but not actually null because the FK properties are not nullable.
        // If this index is non-empty in AcceptAllChanges or SaveChanges, then we throw.
        // If AcceptChanges is called on an entity and that entity is in the index, then
        // we will throw.
        // Note that the index is keyed by EntityEntry reference because it's only ever used
        // when we have the EntityEntry and this makes it slightly faster than using key lookup.
        // </summary>
        internal virtual void RememberEntryWithConceptualNull(EntityEntry entry)
        {
            if (_entriesWithConceptualNulls == null)
            {
                _entriesWithConceptualNulls = new HashSet<EntityEntry>();
            }
            _entriesWithConceptualNulls.Add(entry);
        }

        // <summary>
        // Checks whether or not there is some entry in the context that has any conceptually but not
        // actually null FK values.
        // </summary>
        internal virtual bool SomeEntryWithConceptualNullExists()
        {
            return _entriesWithConceptualNulls != null && _entriesWithConceptualNulls.Count != 0;
        }

        // <summary>
        // Checks whether the given entry has conceptually but not actually null FK values.
        // </summary>
        internal virtual bool EntryHasConceptualNull(EntityEntry entry)
        {
            return _entriesWithConceptualNulls != null && _entriesWithConceptualNulls.Contains(entry);
        }

        // <summary>
        // Stops keeping track of an entity with conceptual nulls because the FK values have been
        // really set or because the entity is leaving the context or becoming deleted.
        // </summary>
        internal virtual void ForgetEntryWithConceptualNull(EntityEntry entry, bool resetAllKeys)
        {
            if (!entry.IsKeyEntry
                && _entriesWithConceptualNulls != null
                && _entriesWithConceptualNulls.Remove(entry))
            {
                if (entry.RelationshipManager.HasRelationships)
                {
                    foreach (var end in entry.RelationshipManager.Relationships)
                    {
                        var reference = end as EntityReference;
                        if (reference != null
                            && ForeignKeyFactory.IsConceptualNullKey(reference.CachedForeignKey))
                        {
                            if (resetAllKeys)
                            {
                                reference.SetCachedForeignKey(null, null);
                            }
                            else
                            {
                                // This means that we thought we could remove because one FK was no longer conceptually
                                // null, but in fact we have to add the entry back because another FK is still conceptually null
                                _entriesWithConceptualNulls.Add(entry);
                                break;
                            }
                        }
                    }
                }
            }
        }

        // devnote: see comment to SQLBU 555615 in ObjectContext.AttachSingleObject()
        internal virtual void PromoteKeyEntryInitialization(
            ObjectContext contextToAttach,
            EntityEntry keyEntry,
            IEntityWrapper wrappedEntity,
            bool replacingEntry)
        {
            DebugCheck.NotNull(keyEntry);
            DebugCheck.NotNull(wrappedEntity);

            // Future Enhancement: Fixup already has this information, don't rediscover it
            var typeMetadata = GetOrAddStateManagerTypeMetadata(wrappedEntity.IdentityType, (EntitySet)keyEntry.EntitySet);
            ValidateProxyType(wrappedEntity);
            keyEntry.PromoteKeyEntry(wrappedEntity, typeMetadata);
            AddEntryToKeylessStore(keyEntry);

            if (replacingEntry)
            {
                // if we are replacing an existing entry, then clean the entity's change tracker
                // so that it can be reset to this newly promoted entry
                wrappedEntity.SetChangeTracker(null);
            }
            // A new entity is being added.
            wrappedEntity.SetChangeTracker(keyEntry);

            if (contextToAttach != null)
            {
                // The ObjectContext needs to be attached to the wrapper here because we need it to be attached to
                // RelatedEnds for the snapshot change tracking that happens in TakeSnapshot. However, it
                // cannot be attached in ObjectContext.AttachSingleObject before calling this method because this
                // would attach it to RelatedEnds before SetChangeTracker is called, thereby breaking a legacy
                // case for entities derived from EntityObject--see AttachSingleObject for details.
                wrappedEntity.AttachContext(contextToAttach, (EntitySet)keyEntry.EntitySet, MergeOption.AppendOnly);
            }

            wrappedEntity.TakeSnapshot(keyEntry);

            OnObjectStateManagerChanged(CollectionChangeAction.Add, keyEntry.Entity);
        }

        // <summary>
        // Upgrades an entity key entry in the cache to a regular entity
        // </summary>
        // <param name="keyEntry"> the key entry that exists in the state manager </param>
        // <param name="wrappedEntity"> the object to add </param>
        // <param name="replacingEntry"> True if this promoted key entry is replacing an existing detached entry </param>
        // <param name="setIsLoaded"> Tells whether we should allow the IsLoaded flag to be set to true for RelatedEnds </param>
        internal virtual void PromoteKeyEntry(
            EntityEntry keyEntry,
            IEntityWrapper wrappedEntity,
            bool replacingEntry,
            bool setIsLoaded,
            bool keyEntryInitialized)
        {
            DebugCheck.NotNull(keyEntry);
            DebugCheck.NotNull(wrappedEntity);
            DebugCheck.NotNull(wrappedEntity.Entity);
            DebugCheck.NotNull(wrappedEntity.Context);

            if (!keyEntryInitialized)
            {
                // We pass null as the context here because, as asserted above, the context is already attached
                // to the wrapper when it comes down this path.
                PromoteKeyEntryInitialization(null, keyEntry, wrappedEntity, replacingEntry);
            }

            var doCleanup = true;
            try
            {
                // We don't need to worry about the KeyEntry <-- Relationship --> KeyEntry because a key entry must
                // reference a non-key entry. Fix up their other side of the relationship.
                // Get all the relationships that currently exist for this key entry
                foreach (var relationshipEntry in CopyOfRelationshipsByKey(keyEntry.EntityKey))
                {
                    if (relationshipEntry.State
                        != EntityState.Deleted)
                    {
                        // Find the association ends that correspond to the source and target
                        var sourceMember = keyEntry.GetAssociationEndMember(relationshipEntry);
                        var targetMember = MetadataHelper.GetOtherAssociationEnd(sourceMember);

                        // Find the other end of the relationship
                        var targetEntry = keyEntry.GetOtherEndOfRelationship(relationshipEntry);

                        // Here we are promoting based on a non-db retrieval so we use Append rules
                        AddEntityToCollectionOrReference(
                            MergeOption.AppendOnly,
                            wrappedEntity,
                            sourceMember,
                            targetEntry.WrappedEntity,
                            targetMember,
                            /*setIsLoaded*/ setIsLoaded,
                            /*relationshipAlreadyExists*/ true,
                            /*inKeyEntryPromotion*/ true);
                    }
                }
                FixupReferencesByForeignKeys(keyEntry);
                doCleanup = false;
            }
            finally
            {
                if (doCleanup)
                {
                    keyEntry.DetachObjectStateManagerFromEntity();
                    RemoveEntryFromKeylessStore(wrappedEntity);
                    keyEntry.DegradeEntry();
                }
            }

            if (TransactionManager.IsAttachTracking)
            {
                TransactionManager.PromotedKeyEntries.Add(wrappedEntity.Entity, keyEntry);
            }
        }

        internal virtual void TrackPromotedRelationship(RelatedEnd relatedEnd, IEntityWrapper wrappedEntity)
        {
            DebugCheck.NotNull(relatedEnd);
            DebugCheck.NotNull(wrappedEntity);
            Debug.Assert(wrappedEntity.Entity != null);
            Debug.Assert(
                TransactionManager.IsAttachTracking || TransactionManager.IsAddTracking,
                "This method should be called only from ObjectContext.AttachTo/AddObject (indirectly)");

            IList<IEntityWrapper> entities;
            if (!TransactionManager.PromotedRelationships.TryGetValue(relatedEnd, out entities))
            {
                entities = new List<IEntityWrapper>();
                TransactionManager.PromotedRelationships.Add(relatedEnd, entities);
            }
            entities.Add(wrappedEntity);
        }

        internal virtual void DegradePromotedRelationships()
        {
            Debug.Assert(
                TransactionManager.IsAttachTracking || TransactionManager.IsAddTracking,
                "This method should be called only from the cleanup code");

            foreach (var pair in TransactionManager.PromotedRelationships)
            {
                foreach (var wrappedEntity in pair.Value)
                {
                    if (pair.Key.RemoveFromCache(wrappedEntity, /*resetIsLoaded*/ false, /*preserveForeignKey*/ false))
                    {
                        pair.Key.OnAssociationChanged(CollectionChangeAction.Remove, wrappedEntity.Entity);
                    }
                }
            }
        }

        // <summary>
        // Performs non-generic collection or reference fixup between two entities
        // This method should only be used in scenarios where we are automatically hooking up relationships for
        // the user, and not in cases where they are manually setting relationships.
        // </summary>
        // <param name="mergeOption"> The MergeOption to use to decide how to resolve EntityReference conflicts </param>
        // <param name="wrappedSource"> The entity instance on the source side of the relationship </param>
        // <param name="sourceMember"> The AssociationEndMember that contains the metadata for the source entity </param>
        // <param name="wrappedTarget"> The entity instance on the source side of the relationship </param>
        // <param name="targetMember"> The AssociationEndMember that contains the metadata for the target entity </param>
        // <param name="setIsLoaded"> Tells whether we should allow the IsLoaded flag to be set to true for RelatedEnds </param>
        // <param name="relationshipAlreadyExists"> Whether or not the relationship entry already exists in the cache for these entities </param>
        // <param name="inKeyEntryPromotion"> Whether this method is used in key entry promotion </param>
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
            if (targetMember.RelationshipMultiplicity
                != RelationshipMultiplicity.Many)
            {
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
                        if (inKeyEntryPromotion
                            &&
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
                        if (currentWrappedTarget != null
                            && currentWrappedTarget.Entity != null
                            && currentWrappedTarget != wrappedTarget)
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
                                if (relationshipEntry.State
                                    == EntityState.Deleted)
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
                    throw new InvalidOperationException(
                        Strings.Collections_CannotFillTryDifferentMergeOption(
                            targetRelatedEnd.SourceRoleName, targetRelatedEnd.RelationshipName));
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
                    relatedEnd.IsLoaded = true;
                }
                // else we just want to leave IsLoaded alone, not set it to false

                // In NoTracking cases, we want to enable the EntityReference.EntityKey property, so we have to set the key
                if (mergeOption == MergeOption.NoTracking)
                {
                    var targetKey = wrappedRelatedEntity.EntityKey;
                    if ((object)targetKey == null)
                    {
                        throw new InvalidOperationException(Strings.EntityKey_UnexpectedNull);
                    }

                    // since endMember is not Many, relatedEnd must be an EntityReference
                    ((EntityReference)relatedEnd).DetachedEntityKey = targetKey;
                }
            }
        }

        // <summary>
        // Updates the relationships between a given source entity and a collection of target entities.
        // Used for full span and related end Load methods, where the following may be true:
        // (a) both sides of each relationship are always full entities and not stubs
        // (b) there could be multiple entities to process at once
        // (c) NoTracking queries are possible.
        // Not used for relationship span because although some of the logic is similar, the above are not true.
        // </summary>
        // <param name="context"> ObjectContext to use to look up existing relationships. Using the context here instead of ObjectStateManager because for NoTracking queries we shouldn't even touch the state manager at all, so we don't want to access it until we know we are not using NoTracking. </param>
        // <param name="mergeOption"> MergeOption to use when updating existing relationships </param>
        // <param name="associationSet"> AssociationSet for the relationships </param>
        // <param name="sourceMember"> Role of sourceEntity in associationSet </param>
        // <param name="wrappedSource"> Source entity in the relationship </param>
        // <param name="targetMember"> Role of each targetEntity in associationSet </param>
        // <param name="targets"> List of target entities to use to create relationships with sourceEntity </param>
        // <param name="setIsLoaded"> Tells whether we should allow the IsLoaded flag to be set to true for RelatedEnds </param>
        internal virtual int UpdateRelationships(
            ObjectContext context, MergeOption mergeOption, AssociationSet associationSet, AssociationEndMember sourceMember,
            IEntityWrapper wrappedSource, AssociationEndMember targetMember, IList targets, bool setIsLoaded)
        {
            var count = 0;
            var sourceKey = wrappedSource.EntityKey;

            context.ObjectStateManager.TransactionManager.BeginGraphUpdate();
            try
            {
                if (targets != null)
                {
                    if (mergeOption == MergeOption.NoTracking)
                    {
                        var relatedEnd = wrappedSource.RelationshipManager.GetRelatedEndInternal(
                            sourceMember.DeclaringType.FullName, targetMember.Name);
                        if (!relatedEnd.IsEmpty())
                        {
                            // The RelatedEnd has already been filled as part of the query and adding additional
                            // entities would cause duplicate entries
                            throw new InvalidOperationException(
                                Strings.Collections_CannotFillTryDifferentMergeOption(
                                    relatedEnd.SourceRoleName, relatedEnd.RelationshipName));
                        }
                    }

                    var sourceKeyRelationshipsLazy = new Lazy<ILookup<EntityKey, RelationshipEntry>>(() => GetRelationshipLookup(context.ObjectStateManager, associationSet, sourceMember, sourceKey));

                    foreach (var someTarget in targets)
                    {
                        var wrappedTarget = someTarget as IEntityWrapper;
                        if (wrappedTarget == null)
                        {
                            wrappedTarget = EntityWrapperFactory.WrapEntityUsingContext(someTarget, context);
                        }
                        count++;

                        // If there is an existing relationship entry, update it based on its current state and the MergeOption, otherwise add a new one            
                        EntityState newEntryState;
                        if (mergeOption == MergeOption.NoTracking)
                        {
                            // For NoTracking, we shouldn't touch the state manager, so no need to look for existing relationships to handle, just connect the two entities.
                            // We don't care if the relationship already exists in the state manager or not, so just pass relationshipAlreadyExists=true so it won't look for it
                            AddEntityToCollectionOrReference(
                                MergeOption.NoTracking,
                                wrappedSource,
                                sourceMember,
                                wrappedTarget,
                                targetMember,
                                setIsLoaded,
                                /*relationshipAlreadyExists*/ true,
                                /*inKeyEntryPromotion*/ false);
                        }
                        else
                        {
                            var manager = context.ObjectStateManager;
                            var targetKey = wrappedTarget.EntityKey;
                            

                            if (
                                !TryUpdateExistingRelationships(
                                    context, mergeOption, associationSet, sourceMember, sourceKeyRelationshipsLazy.Value, wrappedSource, targetMember, targetKey,
                                    setIsLoaded, out newEntryState))
                            {
                                var needNewRelationship = true;
                                switch (sourceMember.RelationshipMultiplicity)
                                {
                                    case RelationshipMultiplicity.ZeroOrOne:
                                    case RelationshipMultiplicity.One:
                                        // The other end of the relationship might already be related to something else, in which case we need to fix it up.
                                        // devnote1: In some cases we can let relationship span do this, but there are cases, like EntityCollection.Attach, where there is no query
                                        //           and thus no relationship span to help us. So, for now, this is redundant because relationship span will make another pass over these
                                        //           entities, but unless I add a flag or something to indicate when I have to do it and when I don't, this is necessary.
                                        // devnote2: The target and source arguments are intentionally reversed in the following call, because we already know there isn't a relationship
                                        //           between the two entities we are current processing, but we want to see if there is one between the target and another source
                                        var targetKeyRelationships = GetRelationshipLookup(context.ObjectStateManager, associationSet, targetMember, targetKey);

                                        needNewRelationship =
                                            !TryUpdateExistingRelationships(
                                                context, mergeOption, associationSet, targetMember,
                                                targetKeyRelationships, wrappedTarget, sourceMember, sourceKey, setIsLoaded, out newEntryState);
                                        break;
                                    case RelationshipMultiplicity.Many:
                                        // we always need a new relationship with Many-To-Many, if there was no exact match between these two entities, so do nothing                                
                                        break;
                                    default:
                                        Debug.Assert(false, "Unexpected sourceMember.RelationshipMultiplicity");
                                        break;
                                }
                                if (needNewRelationship)
                                {
                                    if (newEntryState != EntityState.Deleted)
                                    {
                                        AddEntityToCollectionOrReference(
                                            mergeOption,
                                            wrappedSource,
                                            sourceMember,
                                            wrappedTarget,
                                            targetMember,
                                            setIsLoaded,
                                            /*relationshipAlreadyExists*/ false,
                                            /*inKeyEntryPromotion*/ false);
                                    }
                                    else
                                    {
                                        // Add a Deleted relationship between the source entity and the target entity
                                        // No end fixup is necessary since the relationship is Deleted
                                        var wrapper = new RelationshipWrapper(
                                            associationSet, sourceMember.Name, sourceKey, targetMember.Name, targetKey);
                                        manager.AddNewRelation(wrapper, EntityState.Deleted);
                                    }
                                }
                                // else there is nothing else for us to do, the relationship has been handled already
                            }
                            // else there is nothing else for us to do, the relationship has been handled already
                        }
                    }
                }
                if (count == 0)
                {
                    // If we didn't put anything into the collection, then at least make sure that it is empty
                    // rather than null.
                    EnsureCollectionNotNull(sourceMember, wrappedSource, targetMember);
                }
            }
            finally
            {
                context.ObjectStateManager.TransactionManager.EndGraphUpdate();
            }
            return count;
            // devnote: Don't set IsLoaded on the target related end here -- the caller can do this more efficiently than we can here in some cases.
        }

        internal static ILookup<EntityKey, RelationshipEntry> GetRelationshipLookup(ObjectStateManager manager, AssociationSet associationSet, AssociationEndMember sourceMember, EntityKey sourceKey)
        {
            var relationshipEntries = new List<RelationshipEntry>();

            foreach (var relationshipEntry in manager.FindRelationshipsByKey(sourceKey))
            {
                if (relationshipEntry.IsSameAssociationSetAndRole(associationSet, sourceMember, sourceKey))
                    relationshipEntries.Add(relationshipEntry);
            }

            return relationshipEntries.ToLookup(r => r.RelationshipWrapper.GetOtherEntityKey(sourceKey));
        }

        // Checks if the target end is a collection and, if so, ensures that it is not
        // null by creating an empty collection if necessary.
        private static void EnsureCollectionNotNull(
            AssociationEndMember sourceMember, IEntityWrapper wrappedSource, AssociationEndMember targetMember)
        {
            var relatedEnd = wrappedSource.RelationshipManager.GetRelatedEndInternal(sourceMember.DeclaringType.FullName, targetMember.Name);
            var endMember = (AssociationEndMember)(relatedEnd.ToEndMember);
            if (endMember != null
                && endMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)
            {
                if (relatedEnd.TargetAccessor.HasProperty)
                {
                    wrappedSource.EnsureCollectionNotNull(relatedEnd);
                }
            }
        }

        // <summary>
        // Removes relationships if necessary when a query determines that the source entity has no relationships on the server
        // </summary>
        // <param name="mergeOption"> MergeOption to use when updating existing relationships </param>
        // <param name="associationSet"> AssociationSet for the incoming relationship </param>
        // <param name="sourceKey"> EntityKey of the source entity in the relationship </param>
        // <param name="sourceMember"> Role of the source entity in the relationship </param>
        internal virtual void RemoveRelationships(
            MergeOption mergeOption, AssociationSet associationSet,
            EntityKey sourceKey, AssociationEndMember sourceMember)
        {
            Debug.Assert(
                mergeOption == MergeOption.PreserveChanges || mergeOption == MergeOption.OverwriteChanges, "Unexpected MergeOption");
            // Initial capacity is set to avoid an almost immediate resizing, which was causing a perf hit.
            var deletedRelationships = new List<RelationshipEntry>(InitialListSize);

            // This entity has no related entities on the server for the given associationset and role. If it has related
            // entities on the client, we may need to update those relationships, depending on the MergeOption
            if (mergeOption == MergeOption.OverwriteChanges)
            {
                foreach (var relationshipEntry in FindRelationshipsByKey(sourceKey))
                {
                    // We only care about the relationships that match the incoming associationset and role for the source entity
                    if (relationshipEntry.IsSameAssociationSetAndRole(associationSet, sourceMember, sourceKey))
                    {
                        deletedRelationships.Add(relationshipEntry);
                    }
                }
            }
            else if (mergeOption == MergeOption.PreserveChanges)
            {
                // Leave any Added relationships for this entity, but remove Unchanged and Deleted ones
                foreach (var relationshipEntry in FindRelationshipsByKey(sourceKey))
                {
                    // We only care about the relationships that match the incoming associationset and role for the source entity
                    if (relationshipEntry.IsSameAssociationSetAndRole(associationSet, sourceMember, sourceKey)
                        &&
                        relationshipEntry.State != EntityState.Added)
                    {
                        deletedRelationships.Add(relationshipEntry);
                    }
                }
            }
            // else we do nothing. We never expect any other states here, and already Assert this condition at the top of the method

            foreach (var deletedEntry in deletedRelationships)
            {
                RemoveRelatedEndsAndDetachRelationship(deletedEntry, true);
            }
        }

        // <summary>
        // Tries to updates one or more existing relationships for an entity, based on a given MergeOption and a target entity.
        // </summary>
        // <param name="context"> ObjectContext to use to look up existing relationships for sourceEntity </param>
        // <param name="mergeOption"> MergeOption to use when updating existing relationships </param>
        // <param name="associationSet"> AssociationSet for the relationship we are looking for </param>
        // <param name="sourceMember"> AssociationEndMember for the source role of the relationship </param>
        // <param name="relationshipLookup"> Lookup for the source entity's relationships to find matching relationship entries by target key (passed here for performance reasons) </param>
        // <param name="wrappedSource"> Source entity in the relationship </param>
        // <param name="targetMember"> AssociationEndMember for the target role of the relationship </param>
        // <param name="targetKey"> EntityKey for the target entity in the relationship </param>
        // <param name="setIsLoaded"> Tells whether we should allow the IsLoaded flag to be set to true for RelatedEnds </param>
        // <param name="newEntryState"> [out] EntityState to be used for in scenarios where we need to add a new relationship after this method has returned </param>
        // <returns> true if an existing relationship is found and updated, and no further action is needed false if either no relationship was found, or if one was found and updated, but a new one still needs to be added </returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static bool TryUpdateExistingRelationships(
            ObjectContext context, MergeOption mergeOption, AssociationSet associationSet, AssociationEndMember sourceMember,
            ILookup<EntityKey, RelationshipEntry> relationshipLookup, IEntityWrapper wrappedSource, AssociationEndMember targetMember, EntityKey targetKey, bool setIsLoaded,
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

            foreach (var relationshipEntry in relationshipLookup[targetKey])
            {
                // We only care about relationships for the same AssociationSet and where the source entity is in the same role as it is in the incoming relationship.
                // If the other end of this relationship matches our current target entity, this relationship entry matches the server
                if (entriesToUpdate == null)
                {
                    // Initial capacity is set to avoid an almost immediate resizing, which was causing a perf hit.
                    entriesToUpdate = new List<RelationshipEntry>(InitialListSize);
                }
                entriesToUpdate.Add(relationshipEntry);
            }



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
                    foreach (var relationshipEntry in relationshipLookup.Where(g => g.Key != targetKey).SelectMany(re => re))
                    {
                        // We found an existing relationship where the reference side is different on the server than what the client has.
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
                    }
                    break;
                case RelationshipMultiplicity.Many:
                    // do nothing because its okay for this source entity to have multiple different targets, so there is nothing for us to fixup
                    break;
                default:
                    Debug.Assert(false, "Unexpected targetMember.RelationshipMultiplicity");
                    break;
            }

            // Detach all of the entries that we have collected above
            if (entriesToDetach != null)
            {
                foreach (var entryToDetach in entriesToDetach)
                {
                    // the entry may have already been detached by another operation. If not, detach it now.
                    if (entryToDetach.State
                        != EntityState.Detached)
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
                            if (relationshipEntry.State
                                == EntityState.Added)
                            {
                                relationshipEntry.AcceptChanges();
                            }
                            else if (relationshipEntry.State
                                     == EntityState.Deleted)
                            {
                                // targetEntry should always exist in this scenario because it would have
                                // at least been created when the relationship entry was created
                                var targetEntry = manager.GetEntityEntry(targetKey);

                                // If the target entity is deleted, we don't want to bring the relationship entry back.                            
                                if (targetEntry.State
                                    != EntityState.Deleted)
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
                                            /*setIsLoaded*/ setIsLoaded,
                                            /*relationshipAlreadyExists*/ true,
                                            /*inKeyEntryPromotion*/ false);
                                    }
                                    relationshipEntry.RevertDelete();
                                }
                            }
                            // else it's already Unchanged so we don't need to do anything
                            break;
                        case MergeOption.PreserveChanges:
                            if (relationshipEntry.State
                                == EntityState.Added)
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
            if (relationshipToRemove.State
                != EntityState.Deleted)
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
                    reference.IsLoaded = false;
                }
            }
        }

        // <summary>
        // Attach entity in unchanged state (skip Added state, don't create temp key)
        // It is equal (but faster) to call AddEntry(); AcceptChanges().
        // </summary>
        internal virtual EntityEntry AttachEntry(EntityKey entityKey, IEntityWrapper wrappedObject, EntitySet entitySet)
        {
            DebugCheck.NotNull(wrappedObject);
            DebugCheck.NotNull(wrappedObject.Entity);
            DebugCheck.NotNull(wrappedObject.Context);
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(entityKey);

            // Get a StateManagerTypeMetadata for the entity type.
            var typeMetadata = GetOrAddStateManagerTypeMetadata(wrappedObject.IdentityType, entitySet);

            ValidateProxyType(wrappedObject);

            CheckKeyMatchesEntity(wrappedObject, entityKey, entitySet, /*forAttach*/ true);

            if (!wrappedObject.OwnsRelationshipManager)
            {
                // When a POCO instance is added or attached, we need to ignore the contents 
                // of the RelationshipManager as it is out-of-date with the POCO nav props
                wrappedObject.RelationshipManager.ClearRelatedEndWrappers();
            }

            // Create a cache entry.
            var newEntry = new EntityEntry(wrappedObject, entityKey, entitySet, this, typeMetadata, EntityState.Unchanged);

            // The property EntityKey on newEntry validates that the entry and the entity on the entry have the same key.
            Debug.Assert(entityKey == newEntry.EntityKey, "newEntry.EntityKey should match entityKey");

            // A entity is being attached.
            newEntry.AttachObjectStateManagerToEntity();
            AddEntityEntryToDictionary(newEntry, newEntry.State);

            // fire ColectionChanged event only when a new entity is added to cache
            OnObjectStateManagerChanged(CollectionChangeAction.Add, newEntry.Entity);

            return newEntry;
        }

        // <summary>
        // Checks that the EntityKey attached to the given entity
        // appropriately matches the given entity.
        // </summary>
        // <param name="wrappedEntity"> The entity whose key must be verified </param>
        // <param name="entitySetForType"> The entity set corresponding to the type of the given entity. </param>
        // <param name="forAttach"> If true, then the exception message will reflect a bad key to attach, otherwise it will reflect a general inconsistency </param>
        private void CheckKeyMatchesEntity(IEntityWrapper wrappedEntity, EntityKey entityKey, EntitySet entitySetForType, bool forAttach)
        {
            DebugCheck.NotNull(wrappedEntity);
            DebugCheck.NotNull(wrappedEntity.Entity);

            DebugCheck.NotNull((object)entityKey);
            Debug.Assert(
                !entityKey.IsTemporary, "Verifying a temporary EntityKey doesn't make sense because the key doesn't contain any values.");
            DebugCheck.NotNull(entitySetForType);

            var entitySetForKey = entityKey.GetEntitySet(MetadataWorkspace);
            if (entitySetForKey == null)
            {
                throw new InvalidOperationException(Strings.ObjectStateManager_InvalidKey);
            }

            // Checks that the entity's key matches its type.
            Debug.Assert(
                entitySetForType.Name == entitySetForKey.Name &&
                entitySetForType.EntityContainer.Name == entitySetForKey.EntityContainer.Name,
                "The object cannot be attached because its EntityType belongs to a different EntitySet than the one specified in its key.");

            // Verify that the entity key contains the correct members for the entity set
            entityKey.ValidateEntityKey(_metadataWorkspace, entitySetForKey);

            // Checks that the key values in the entity match the key values
            // within its EntityKey.
            var typeMetadata = GetOrAddStateManagerTypeMetadata(wrappedEntity.IdentityType, entitySetForType);
            for (var i = 0; i < entitySetForKey.ElementType.KeyMembers.Count; ++i)
            {
                var keyField = entitySetForKey.ElementType.KeyMembers[i];
                var ordinal = typeMetadata.GetOrdinalforCLayerMemberName(keyField.Name);
                if (ordinal < 0)
                {
                    throw new InvalidOperationException(Strings.ObjectStateManager_InvalidKey);
                }

                var entityValue = typeMetadata.Member(ordinal).GetValue(wrappedEntity.Entity);
                var keyValue = entityKey.FindValueByName(keyField.Name);

                // Use EntityKey.ValueComparer to perform the correct equality comparison for entity key values.
                if (!ByValueEqualityComparer.Default.Equals(entityValue, keyValue))
                {
                    throw new InvalidOperationException(
                        forAttach
                            ? Strings.ObjectStateManager_KeyPropertyDoesntMatchValueInKeyForAttach
                            : Strings.ObjectStateManager_KeyPropertyDoesntMatchValueInKey);
                }
            }
        }

        internal virtual RelationshipEntry AddNewRelation(RelationshipWrapper wrapper, EntityState desiredState)
        {
            Debug.Assert(null == FindRelationship(wrapper), "relationship should not exist, caller verifies");

            var entry = new RelationshipEntry(this, desiredState, wrapper);
            AddRelationshipEntryToDictionary(entry, desiredState);
            AddRelationshipToLookup(entry);
            return entry;
        }

        internal virtual RelationshipEntry AddRelation(RelationshipWrapper wrapper, EntityState desiredState)
        {
            Debug.Assert(
                EntityState.Added == desiredState || // result entry should be added or left alone
                EntityState.Unchanged == desiredState || // result entry should be that state
                EntityState.Deleted == desiredState, // result entry should be in that state
                "unexpected state");

            var entry = FindRelationship(wrapper);
            Debug.Assert(null == entry || (EntityState.Modified != entry.State), "relationship should never be modified");

            if (entry == null)
            {
                entry = AddNewRelation(wrapper, desiredState);
            }
            else if (EntityState.Deleted
                     != entry.State)
            {
                // you can have a deleted and non-deleted relation between two entities
                // SQL BU DT 449757: for now no-op in case if it exists. ideally need to throw
                if (EntityState.Unchanged == desiredState)
                {
                    entry.AcceptChanges();
                }
                else if (EntityState.Deleted == desiredState)
                {
                    entry.AcceptChanges();
                    entry.Delete(false);
                }
                // else Added and leave entry alone
            }
            else if (EntityState.Deleted != desiredState)
            {
                Debug.Assert(EntityState.Deleted == entry.State, "should be deleted state");
                entry.RevertDelete();
            }
            // else entry already Deleted or if desired state is Added then left alone

            Debug.Assert(
                desiredState == entry.State ||
                EntityState.Added == desiredState,
                "unexpected end state");

            return entry;
        }

        // <summary>
        // Adds the given relationship cache entry to the mapping from each of its endpoint keys.
        // </summary>
        private void AddRelationshipToLookup(RelationshipEntry relationship)
        {
            DebugCheck.NotNull(relationship);

            AddRelationshipEndToLookup(relationship.RelationshipWrapper.Key0, relationship);
            if (!relationship.RelationshipWrapper.Key0.Equals(relationship.RelationshipWrapper.Key1))
            {
                AddRelationshipEndToLookup(relationship.RelationshipWrapper.Key1, relationship);
            }
        }

        // <summary>
        // Adds the given relationship cache entry to the mapping from the given endpoint key.
        // </summary>
        private void AddRelationshipEndToLookup(EntityKey key, RelationshipEntry relationship)
        {
            Debug.Assert(null != FindEntityEntry(key), "EntityEntry doesn't exist");

            var entry = GetEntityEntry(key);
            Debug.Assert(key.Equals(entry.EntityKey), "EntityKey mismatch");
            entry.AddRelationshipEnd(relationship);
        }

        // <summary>
        // Deletes the given relationship cache entry from the mapping from each of its endpoint keys.
        // </summary>
        private void DeleteRelationshipFromLookup(RelationshipEntry relationship)
        {
            // The relationship is stored in the lookup indexed by both keys, so we need to remove it twice.
            DeleteRelationshipEndFromLookup(relationship.RelationshipWrapper.Key0, relationship);
            if (!relationship.RelationshipWrapper.Key0.Equals(relationship.RelationshipWrapper.Key1))
            {
                DeleteRelationshipEndFromLookup(relationship.RelationshipWrapper.Key1, relationship);
            }
        }

        // <summary>
        // Deletes the given relationship cache entry from the mapping from the given endpoint key.
        // </summary>
        private void DeleteRelationshipEndFromLookup(EntityKey key, RelationshipEntry relationship)
        {
            Debug.Assert(relationship.State != EntityState.Detached, "Cannot remove a detached cache entry.");
            Debug.Assert(null != FindEntityEntry(key), "EntityEntry doesn't exist");

            var entry = GetEntityEntry(key);
            Debug.Assert(key.Equals(entry.EntityKey), "EntityKey mismatch");
            entry.RemoveRelationshipEnd(relationship);
        }

        internal virtual RelationshipEntry FindRelationship(
            RelationshipSet relationshipSet,
            KeyValuePair<string, EntityKey> roleAndKey1,
            KeyValuePair<string, EntityKey> roleAndKey2)
        {
            if ((null == (object)roleAndKey1.Value)
                || (null == (object)roleAndKey2.Value))
            {
                return null;
            }
            return FindRelationship(new RelationshipWrapper((AssociationSet)relationshipSet, roleAndKey1, roleAndKey2));
        }

        internal virtual RelationshipEntry FindRelationship(RelationshipWrapper relationshipWrapper)
        {
            RelationshipEntry entry = null;
            var result = (((null != _unchangedRelationshipStore) && _unchangedRelationshipStore.TryGetValue(relationshipWrapper, out entry))
                          ||
                          ((null != _deletedRelationshipStore) && _deletedRelationshipStore.TryGetValue(relationshipWrapper, out entry)) ||
                          ((null != _addedRelationshipStore) && _addedRelationshipStore.TryGetValue(relationshipWrapper, out entry)));
            Debug.Assert(result == (null != entry), "found null entry");
            return entry;
        }

        // <summary>
        // DeleteRelationship
        // </summary>
        // <returns> The deleted entry </returns>
        internal virtual RelationshipEntry DeleteRelationship(
            RelationshipSet relationshipSet,
            KeyValuePair<string, EntityKey> roleAndKey1,
            KeyValuePair<string, EntityKey> roleAndKey2)
        {
            var entry = FindRelationship(relationshipSet, roleAndKey1, roleAndKey2);
            if (entry != null)
            {
                entry.Delete( /*doFixup*/ false);
            }
            return entry;
        }

        // <summary>
        // DeleteKeyEntry
        // </summary>
        internal virtual void DeleteKeyEntry(EntityEntry keyEntry)
        {
            if (keyEntry != null
                && keyEntry.IsKeyEntry)
            {
                ChangeState(keyEntry, keyEntry.State, EntityState.Detached);
            }
        }

        // <summary>
        // Finds all relationships with the given key at one end.
        // </summary>
        internal virtual RelationshipEntry[] CopyOfRelationshipsByKey(EntityKey key)
        {
            return FindRelationshipsByKey(key).ToArray();
        }

        // <summary>
        // Finds all relationships with the given key at one end.
        // Do not use the list to add elements
        // </summary>
        internal virtual EntityEntry.RelationshipEndEnumerable FindRelationshipsByKey(EntityKey key)
        {
            return new EntityEntry.RelationshipEndEnumerable(FindEntityEntry(key));
        }

        IEnumerable<IEntityStateEntry> IEntityStateManager.FindRelationshipsByKey(EntityKey key)
        {
            return FindRelationshipsByKey(key);
        }

        //Verify that all entities in the _keylessEntityStore are also in the other dictionaries.
        //Verify that all the entries in the _keylessEntityStore don't implement IEntityWithKey.
        //Verify that there no entries in the other dictionaries that don't implement IEntityWithKey and aren't in _keylessEntityStore
        [Conditional("DEBUG")]
        private void ValidateKeylessEntityStore()
        {
            // The normal case these days is for all entities to be in the keyless store,
            // so we do a quick check whether the count of the keyless store is the same as the
            // count of the other stores and if so we abort the other checks so that running
            // the debug build is not slowed down too much--see CodePlex 1724

            Dictionary<EntityKey, EntityEntry>[] stores =
                {
                    _unchangedEntityStore, _modifiedEntityStore, _addedEntityStore,
                    _deletedEntityStore
                };

            if (_keylessEntityStore != null
                && _keylessEntityStore.Count == stores.Sum(s => s == null ? 0 : s.Count))
            {
                return;
            }

            // Future Enhancement : Check each entry in _keylessEntityStore to make sure it has a corresponding entry in one of the other stores.
            if (null != _keylessEntityStore)
            {
                foreach (var entry in _keylessEntityStore.Values)
                {
                    Debug.Assert(!(entry.Entity is IEntityWithKey), "_keylessEntityStore contains an entry that implement IEntityWithKey");
                    EntityEntry entrya;
                    var result = false;
                    if (null != _addedEntityStore)
                    {
                        result = _addedEntityStore.TryGetValue(entry.EntityKey, out entrya);
                    }
                    if (null != _modifiedEntityStore)
                    {
                        result |= _modifiedEntityStore.TryGetValue(entry.EntityKey, out entrya);
                    }
                    if (null != _deletedEntityStore)
                    {
                        result |= _deletedEntityStore.TryGetValue(entry.EntityKey, out entrya);
                    }
                    if (null != _unchangedEntityStore)
                    {
                        result |= _unchangedEntityStore.TryGetValue(entry.EntityKey, out entrya);
                    }
                    Debug.Assert(result, "entry in _keylessEntityStore doesn't exist in one of the other stores");
                }
            }

            //Check each entry in the other stores to make sure that each non-IEntityWithKey entry is also in _keylessEntityStore
            foreach (var store in stores)
            {
                if (null != store)
                {
                    foreach (var entry in store.Values)
                    {
                        if (null != entry.Entity
                            && //Skip span stub key entry
                            !(entry.Entity is IEntityWithKey))
                        {
                            EntityEntry keylessEntry;
                            Debug.Assert(null != _keylessEntityStore, "There should be a store that keyless entries are in");
                            if (_keylessEntityStore.TryGetValue(entry.Entity, out keylessEntry))
                            {
                                Debug.Assert(ReferenceEquals(entry, keylessEntry), "keylessEntry and entry from stores do not match");
                            }
                            else
                            {
                                Debug.Assert(
                                    false,
                                    "The entry containing an entity not implementing IEntityWithKey is not in the _keylessEntityStore");
                            }
                        }
                    }
                }
            }
        }

        // <summary>
        // Find the ObjectStateEntry from _keylessEntityStore for an entity that doesn't implement IEntityWithKey.
        // </summary>
        private bool TryGetEntryFromKeylessStore(object entity, out EntityEntry entryRef)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            Debug.Assert(!(entity is IEntityWithKey));

            ValidateKeylessEntityStore();
            entryRef = null;
            if (entity == null)
            {
                return false;
            }
            if (null != _keylessEntityStore)
            {
                if (_keylessEntityStore.TryGetValue(entity, out entryRef))
                {
                    return true;
                }
            }

            entryRef = null;
            return false;
        }

        /// <summary>
        /// Returns a collection of <see cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" /> objects for objects or relationships with the given state.
        /// </summary>
        /// <returns>
        /// A collection of <see cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" /> objects in the given
        /// <see
        ///     cref="T:System.Data.Entity.EntityState" />
        /// .
        /// </returns>
        /// <param name="state">
        /// An <see cref="T:System.Data.Entity.EntityState" /> used to filter the returned
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" />
        /// objects.
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        /// When  state  is <see cref="F:System.Data.Entity.EntityState.Detached" />.
        /// </exception>
        public virtual IEnumerable<ObjectStateEntry> GetObjectStateEntries(EntityState state)
        {
            if ((EntityState.Detached & state) != 0)
            {
                throw new ArgumentException(Strings.ObjectStateManager_DetachedObjectStateEntriesDoesNotExistInObjectStateManager);
            }
            return GetObjectStateEntriesInternal(state);
        }

        // <summary>
        // Returns all CacheEntries in the given state.
        // </summary>
        // <exception cref="ArgumentException">if EntityState.Detached flag is set in state</exception>
        IEnumerable<IEntityStateEntry> IEntityStateManager.GetEntityStateEntries(EntityState state)
        {
            Debug.Assert((EntityState.Detached & state) == 0, "Cannot get state entries for detached entities");
            foreach (var stateEntry in GetObjectStateEntriesInternal(state))
            {
                yield return stateEntry;
            }
        }

        internal virtual bool HasChanges()
        {
            return (_addedRelationshipStore != null && _addedRelationshipStore.Count > 0)
                || (_addedEntityStore != null && _addedEntityStore.Count > 0)
                || (_modifiedEntityStore != null && _modifiedEntityStore.Count > 0)
                || (_deletedRelationshipStore != null && _deletedRelationshipStore.Count > 0)
                || (_deletedEntityStore != null && _deletedEntityStore.Count > 0);
        }

        internal virtual int GetObjectStateEntriesCount(EntityState state)
        {
            var size = 0;
            if ((EntityState.Added & state) != 0)
            {
                size += ((null != _addedRelationshipStore) ? _addedRelationshipStore.Count : 0);
                size += ((null != _addedEntityStore) ? _addedEntityStore.Count : 0);
            }
            if ((EntityState.Modified & state) != 0)
            {
                size += ((null != _modifiedEntityStore) ? _modifiedEntityStore.Count : 0);
            }
            if ((EntityState.Deleted & state) != 0)
            {
                size += ((null != _deletedRelationshipStore) ? _deletedRelationshipStore.Count : 0);
                size += ((null != _deletedEntityStore) ? _deletedEntityStore.Count : 0);
            }
            if ((EntityState.Unchanged & state) != 0)
            {
                size += ((null != _unchangedRelationshipStore) ? _unchangedRelationshipStore.Count : 0);
                size += ((null != _unchangedEntityStore) ? _unchangedEntityStore.Count : 0);
            }
            return size;
        }

        private int GetMaxEntityEntriesForDetectChanges()
        {
            var size = 0;
            if (_addedEntityStore != null)
            {
                size += _addedEntityStore.Count;
            }
            if (_modifiedEntityStore != null)
            {
                size += _modifiedEntityStore.Count;
            }
            if (_deletedEntityStore != null)
            {
                size += _deletedEntityStore.Count;
            }
            if (_unchangedEntityStore != null)
            {
                size += _unchangedEntityStore.Count;
            }
            return size;
        }

        internal virtual IEnumerable<ObjectStateEntry> GetObjectStateEntriesInternal(EntityState state)
        {
            Debug.Assert((EntityState.Detached & state) == 0, "Cannot get state entries for detached entities");

            var size = GetObjectStateEntriesCount(state);
            var entries = new ObjectStateEntry[size];

            size = 0; // size is now used as an offset
            if (((EntityState.Added & state) != 0)
                && (null != _addedRelationshipStore))
            {
                foreach (var e in _addedRelationshipStore)
                {
                    entries[size++] = e.Value;
                }
            }
            if (((EntityState.Deleted & state) != 0)
                && (null != _deletedRelationshipStore))
            {
                foreach (var e in _deletedRelationshipStore)
                {
                    entries[size++] = e.Value;
                }
            }
            if (((EntityState.Unchanged & state) != 0)
                && (null != _unchangedRelationshipStore))
            {
                foreach (var e in _unchangedRelationshipStore)
                {
                    entries[size++] = e.Value;
                }
            }
            if (((EntityState.Added & state) != 0)
                && (null != _addedEntityStore))
            {
                foreach (var e in _addedEntityStore)
                {
                    entries[size++] = e.Value;
                }
            }
            if (((EntityState.Modified & state) != 0)
                && (null != _modifiedEntityStore))
            {
                foreach (var e in _modifiedEntityStore)
                {
                    entries[size++] = e.Value;
                }
            }
            if (((EntityState.Deleted & state) != 0)
                && (null != _deletedEntityStore))
            {
                foreach (var e in _deletedEntityStore)
                {
                    entries[size++] = e.Value;
                }
            }
            if (((EntityState.Unchanged & state) != 0)
                && (null != _unchangedEntityStore))
            {
                foreach (var e in _unchangedEntityStore)
                {
                    entries[size++] = e.Value;
                }
            }
            return entries;
        }

        private IList<EntityEntry> GetEntityEntriesForDetectChanges()
        {
            // This flag is set whenever an entity that may need snapshot change tracking
            // becomes tracked by the context.  Entities that may need snapshot change tracking
            // are those for which any of the following are true:
            // a) Entity does not implement IEntityWithRelationships
            // b) Entity does not implement IEntityWithChangeTracker
            // b) Entity has a complex property.
            if (!_detectChangesNeeded)
            {
                return null;
            }

            List<EntityEntry> entries = null; // Will be lazy initialized if needed.
            GetEntityEntriesForDetectChanges(_addedEntityStore, ref entries);
            GetEntityEntriesForDetectChanges(_modifiedEntityStore, ref entries);
            GetEntityEntriesForDetectChanges(_deletedEntityStore, ref entries);
            GetEntityEntriesForDetectChanges(_unchangedEntityStore, ref entries);

            // If the flag was set, but we don't find anything to do, then reset the flag again
            // since it means that there were some entities that needed DetectChanges, but now they
            // have been detached.
            if (entries == null)
            {
                _detectChangesNeeded = false;
            }

            return entries;
        }

        private void GetEntityEntriesForDetectChanges(Dictionary<EntityKey, EntityEntry> entityStore, ref List<EntityEntry> entries)
        {
            if (entityStore != null)
            {
                foreach (var entry in entityStore.Values)
                {
                    if (entry.RequiresAnyChangeTracking)
                    {
                        if (entries == null)
                        {
                            entries = new List<EntityEntry>(GetMaxEntityEntriesForDetectChanges());
                        }
                        entries.Add(entry);
                    }
                }
            }
        }

        #region temporary (added state) to permanent (deleted, modified, unchanged state) EntityKey fixup

        // <summary>
        // Performs key-fixup on the given entry, by creating a (permanent) EntityKey
        // based on the current key values within the associated entity and fixing up
        // all associated relationship entries.
        // </summary>
        // <remarks>
        // Will promote EntityEntry.IsKeyEntry and leave in _unchangedStore
        // otherwise will move EntityEntry from _addedStore to _unchangedStore.
        // </remarks>
        internal virtual void FixupKey(EntityEntry entry)
        {
            DebugCheck.NotNull(entry);
            Debug.Assert(entry.State == EntityState.Added, "Cannot do key fixup for an entry not in the Added state.");
            DebugCheck.NotNull(entry.Entity);

            var oldKey = entry.EntityKey;
            Debug.Assert(entry == _addedEntityStore[oldKey], "not the same EntityEntry");
            Debug.Assert((object)oldKey != null, "Cannot fixup a cache entry with a null key.");
            Debug.Assert(oldKey.IsTemporary, "Cannot fixup an entry with a non-temporary key.");
            Debug.Assert(null != _addedEntityStore, "missing added store");

            var entitySet = (EntitySet)entry.EntitySet;
            var performFkSteps = entitySet.HasForeignKeyRelationships;
            var performNonFkSteps = entitySet.HasIndependentRelationships;

            if (performFkSteps)
            {
                // Do fixup based on reference first for added objects.
                // This must be done before creating a new key or the key will have old values.
                entry.FixupForeignKeysByReference();
            }

            EntityKey newKey;
            try
            {
                // Construct an EntityKey based on the current, fixed-up values of the entry.
                newKey = new EntityKey((EntitySet)entry.EntitySet, entry.CurrentValues);
            }
            catch (ArgumentException ex)
            {
                // ArgumentException is not the best choice here but anything else would be a breaking change.
                throw new ArgumentException(Strings.ObjectStateManager_ChangeStateFromAddedWithNullKeyIsInvalid, ex);
            }

            var existingEntry = FindEntityEntry(newKey);
            if (existingEntry != null)
            {
                if (!existingEntry.IsKeyEntry)
                {
                    // If the fixed-up key conflicts with an existing entry, we throw.
                    throw new InvalidOperationException(
                        Strings.ObjectStateManager_CannotFixUpKeyToExistingValues(entry.WrappedEntity.IdentityType.FullName));
                }
                newKey = existingEntry.EntityKey; // reuse existing reference
            }

            RelationshipEntry[] relationshipEnds = null;
            if (performNonFkSteps)
            {
                // remove the relationships based on the temporary key
                relationshipEnds = entry.GetRelationshipEnds().ToArray();
                foreach (var relationshipEntry in relationshipEnds)
                {
                    RemoveObjectStateEntryFromDictionary(relationshipEntry, relationshipEntry.State);
                }
            }

            // Remove ObjectStateEntry with old Key and add it back or promote with new key.
            RemoveObjectStateEntryFromDictionary(entry, EntityState.Added);

            // This is the only scenario where we are allowed to set the EntityKey if it's already non-null
            // If entry.EntityKey is IEntityWithKey, user code will be called
            ResetEntityKey(entry, newKey);

            if (performNonFkSteps)
            {
                // Fixup all relationships for which this key was a participant.
                entry.UpdateRelationshipEnds(oldKey, existingEntry);

                // add all the relationships back on the new entity key
                foreach (var relationshipEntry in relationshipEnds)
                {
                    AddRelationshipEntryToDictionary(relationshipEntry, relationshipEntry.State);
                }
            }

            // Now promote the key entry to a full entry by adding entities to the related ends
            if (existingEntry != null)
            {
                // two ObjectStateEntry exist for same newKey, the entity stub must exist in unchanged state
                Debug.Assert(existingEntry.State == EntityState.Unchanged, "entity stub must be in unchanged state");
                Debug.Assert(existingEntry.IsKeyEntry, "existing entry must be a key entry to promote");
                Debug.Assert(ReferenceEquals(newKey, existingEntry.EntityKey), "should be same key reference");
                PromoteKeyEntry(existingEntry, entry.WrappedEntity, true, /*setIsLoaded*/ false, /*keyEntryInitialized*/ false);

                // leave the entity stub in the unchanged state
                // the existing entity stub wins
                entry = existingEntry;
            }
            else
            {
                // change the state to "Unchanged"
                AddEntityEntryToDictionary(entry, EntityState.Unchanged);
            }

            if (performFkSteps)
            {
                FixupReferencesByForeignKeys(entry);
            }

            Debug.Assert((null == _addedEntityStore) || !_addedEntityStore.ContainsKey(oldKey), "EntityEntry exists with OldKey");
            Debug.Assert(
                (null != _unchangedEntityStore) && _unchangedEntityStore.ContainsKey(newKey), "EntityEntry does not exist with NewKey");

            // FEATURE_CHANGE: once we support equality constraints (SQL PT DB 300002154), do recursive fixup.
        }

        // <summary>
        // Replaces permanent EntityKey with a temporary key.  Used in N-Tier API.
        // </summary>
        internal virtual void ReplaceKeyWithTemporaryKey(EntityEntry entry)
        {
            DebugCheck.NotNull(entry);
            Debug.Assert(entry.State != EntityState.Added, "Cannot replace key with a temporary key if the entry is in Added state.");
            Debug.Assert(!entry.IsKeyEntry, "Cannot replace a key of a KeyEntry");

            var oldKey = entry.EntityKey;
            Debug.Assert(!oldKey.IsTemporary, "Entity is not in the Added state but has a temporary key.");

            // Construct an temporary EntityKey.
            var newKey = new EntityKey(entry.EntitySet);

            Debug.Assert(FindEntityEntry(newKey) == null, "no entry should exist with the new temporary key");

            // remove the relationships based on the permanent key
            var relationshipEnds = entry.GetRelationshipEnds().ToArray();
            foreach (var relationshipEntry in relationshipEnds)
            {
                RemoveObjectStateEntryFromDictionary(relationshipEntry, relationshipEntry.State);
            }

            // Remove ObjectStateEntry with old Key and add it back or promote with new key.
            RemoveObjectStateEntryFromDictionary(entry, entry.State);

            // This is the only scenario where we are allowed to set the EntityKey if it's already non-null
            // If entry.EntityKey is IEntityWithKey, user code will be called
            ResetEntityKey(entry, newKey);

            // Fixup all relationships for which this key was a participant.
            entry.UpdateRelationshipEnds(oldKey, null); // null PromotedEntry

            // add all the relationships back on the new entity key
            foreach (var relationshipEntry in relationshipEnds)
            {
                AddRelationshipEntryToDictionary(relationshipEntry, relationshipEntry.State);
            }

            AddEntityEntryToDictionary(entry, EntityState.Added);
        }

        // <summary>
        // Resets the EntityKey for this entry.  This method is called
        // as part of temporary key fixup and permanent key un-fixup. This method is necessary because it is the only
        // scenario where we allow a new value to be set on a non-null EntityKey. This
        // is the only place where we should be setting and clearing _inRelationshipFixup.
        // </summary>
        private void ResetEntityKey(EntityEntry entry, EntityKey value)
        {
            DebugCheck.NotNull((object)entry.EntityKey);
            Debug.Assert(!_inRelationshipFixup, "already _inRelationshipFixup");
            Debug.Assert(!entry.EntityKey.Equals(value), "the keys should not be equal");

            var entityKey = entry.WrappedEntity.EntityKey;
            if (entityKey == null
                || value.Equals(entityKey))
            {
                throw new InvalidOperationException(Strings.ObjectStateManager_AcceptChangesEntityKeyIsNotValid);
            }
            try
            {
                _inRelationshipFixup = true;
                entry.WrappedEntity.EntityKey = value; // user will have control
                var wrappedEntity = entry.WrappedEntity;
                if (wrappedEntity.EntityKey != value)
                {
                    throw new InvalidOperationException(Strings.EntityKey_DoesntMatchKeyOnEntity(wrappedEntity.Entity.GetType().FullName));
                }
            }
            finally
            {
                _inRelationshipFixup = false;
            }

            // Keeping the entity and entry keys in sync.
            entry.EntityKey = value;

            //Internally, entry.EntityKey asserts that entry._entityKey and entityWithKey.EntityKey are equal.
            Debug.Assert(value == entry.EntityKey, "The new key was not set onto the entry correctly");
        }

        #endregion

        /// <summary>
        /// Changes state of the <see cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" /> for a specific object to the specified  entityState .
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" /> for the supplied  entity .
        /// </returns>
        /// <param name="entity">The object for which the state must be changed.</param>
        /// <param name="entityState">The new state of the object.</param>
        /// <exception cref="T:System.ArgumentNullException">When  entity  is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// When the object is not detached and does not have an entry in the state manager
        /// or when you try to change the state to <see cref="F:System.Data.Entity.EntityState.Detached" />
        /// from any other <see cref="T:System.Data.Entity.EntityState" />
        /// or when  state  is not a valid <see cref="T:System.Data.Entity.EntityState" /> value.
        /// </exception>
        public virtual ObjectStateEntry ChangeObjectState(object entity, EntityState entityState)
        {
            Check.NotNull(entity, "entity");
            EntityUtil.CheckValidStateForChangeEntityState(entityState);

            EntityEntry entry = null;

            TransactionManager.BeginLocalPublicAPI();
            try
            {
                var key = entity as EntityKey;
                entry = (key != null)
                            ? FindEntityEntry(key)
                            : FindEntityEntry(entity);

                if (entry == null)
                {
                    if (entityState == EntityState.Detached)
                    {
                        return null; // No-op
                    }
                    throw new InvalidOperationException(Strings.ObjectStateManager_NoEntryExistsForObject(entity.GetType().FullName));
                }

                entry.ChangeObjectState(entityState);
            }
            finally
            {
                TransactionManager.EndLocalPublicAPI();
            }

            return entry;
        }

        /// <summary>Changes the state of the relationship between two entity objects that is specified based on the two related objects and the name of the navigation property.</summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" /> for the relationship that was changed.
        /// </returns>
        /// <param name="sourceEntity">
        /// The object instance or <see cref="T:System.Data.Entity.Core.EntityKey" /> of the source entity at one end of the relationship.
        /// </param>
        /// <param name="targetEntity">
        /// The object instance or <see cref="T:System.Data.Entity.Core.EntityKey" /> of the target entity at the other end of the relationship.
        /// </param>
        /// <param name="navigationProperty">The name of the navigation property on  source  that returns the specified  target .</param>
        /// <param name="relationshipState">
        /// The requested <see cref="T:System.Data.Entity.EntityState" /> of the specified relationship.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">When  source  or  target  is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// When trying to change the state of the relationship to a state other than 
        ///     <see cref="F:System.Data.Entity.EntityState.Deleted" /> or <see cref="F:System.Data.Entity.EntityState.Detached" /> 
        /// when either  source  or  target  is in a <see cref="F:System.Data.Entity.EntityState.Deleted" /> state 
        /// or when you try to change the state of the relationship to a state other than
        /// <see cref="F:System.Data.Entity.EntityState.Added" /> or <see cref="F:System.Data.Entity.EntityState.Detached" />
        /// when either  source  or  target  is in an <see ref="F:System.Data.Entity.EntityState.Added" /> state 
        /// or when  state  is not a valid <see cref="T:System.Data.Entity.EntityState" /> value
        /// </exception>
        public virtual ObjectStateEntry ChangeRelationshipState(
            object sourceEntity,
            object targetEntity,
            string navigationProperty,
            EntityState relationshipState)
        {
            EntityEntry sourceEntry;
            EntityEntry targetEntry;

            VerifyParametersForChangeRelationshipState(sourceEntity, targetEntity, out sourceEntry, out targetEntry);
            Check.NotEmpty(navigationProperty, "navigationProperty");

            var relatedEnd = sourceEntry.WrappedEntity.RelationshipManager.GetRelatedEnd(navigationProperty);

            return ChangeRelationshipState(sourceEntry, targetEntry, relatedEnd, relationshipState);
        }

        /// <summary>Changes the state of the relationship between two entity objects that is specified based on the two related objects and a LINQ expression that defines the navigation property.</summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" /> for the relationship that was changed.
        /// </returns>
        /// <param name="sourceEntity">
        /// The object instance or <see cref="T:System.Data.Entity.Core.EntityKey" /> of the source entity at one end of the relationship.
        /// </param>
        /// <param name="targetEntity">
        /// The object instance or <see cref="T:System.Data.Entity.Core.EntityKey" /> of the target entity at the other end of the relationship.
        /// </param>
        /// <param name="navigationPropertySelector">A LINQ expression that selects the navigation property on  source  that returns the specified  target .</param>
        /// <param name="relationshipState">
        /// The requested <see cref="T:System.Data.Entity.EntityState" /> of the specified relationship.
        /// </param>
        /// <typeparam name="TEntity">The entity type of the  source  object.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">When  source ,  target , or  selector  is null.</exception>
        /// <exception cref="T:System.ArgumentException"> selector  is malformed or cannot return a navigation property.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// When you try to change the state of the relationship to a state other than
        /// <see cref="F:System.Data.Entity.EntityState.Deleted" />  or <see cref="F:System.Data.Entity.EntityState.Detached" /> 
        /// when either  source  or  target  is in a
        /// <see cref="F:System.Data.Entity.EntityState.Deleted" /> state
        /// or when you try to change the state of the relationship to a state other than
        /// <see  cref="F:System.Data.Entity.EntityState.Added" />  or <see  cref="F:System.Data.Entity.EntityState.Detached" />
        /// when either  source  or  target  is in an <see cref="F:System.Data.Entity.EntityState.Added" /> state 
        /// or when  state  is not a valid <see cref="T:System.Data.Entity.EntityState" /> value.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public virtual ObjectStateEntry ChangeRelationshipState<TEntity>(
            TEntity sourceEntity,
            object targetEntity,
            Expression<Func<TEntity, object>> navigationPropertySelector,
            EntityState relationshipState) where TEntity : class
        {
            EntityEntry sourceEntry;
            EntityEntry targetEntry;

            VerifyParametersForChangeRelationshipState(sourceEntity, targetEntity, out sourceEntry, out targetEntry);

            // We used to throw an ArgumentException if the expression contained a Convert.  Now we remove the convert,
            // but if we still need to throw, then we should still throw an ArgumentException to avoid a breaking change.
            // Therefore, we keep track of whether or not we removed the convert.
            bool removedConvert;
            var navigationProperty = ObjectContext.ParsePropertySelectorExpression(navigationPropertySelector, out removedConvert);
            var relatedEnd = sourceEntry.WrappedEntity.RelationshipManager.GetRelatedEnd(
                navigationProperty, throwArgumentException: removedConvert);

            return ChangeRelationshipState(sourceEntry, targetEntry, relatedEnd, relationshipState);
        }

        /// <summary>Changes the state of the relationship between two entity objects that is specified based on the two related objects and the properties of the relationship.</summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" /> for the relationship that was changed.
        /// </returns>
        /// <param name="sourceEntity">
        /// The object instance or <see cref="T:System.Data.Entity.Core.EntityKey" /> of the source entity at one end of the relationship.
        /// </param>
        /// <param name="targetEntity">
        /// The object instance or <see cref="T:System.Data.Entity.Core.EntityKey" /> of the target entity at the other end of the relationship.
        /// </param>
        /// <param name="relationshipName">The name of the relationship.</param>
        /// <param name="targetRoleName">The role name at the  target  end of the relationship.</param>
        /// <param name="relationshipState">
        /// The requested <see cref="T:System.Data.Entity.EntityState" /> of the specified relationship.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">When  source  or  target  is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// When you try to change the state of the relationship to a state other than
        /// <see cref="F:System.Data.Entity.EntityState.Deleted" /> or <see cref="F:System.Data.Entity.EntityState.Detached" /> 
        /// when either  source  or  target  is in a <see cref="F:System.Data.Entity.EntityState.Deleted" /> state 
        /// or when you try to change the state of the relationship to a state other than
        /// <see cref="F:System.Data.Entity.EntityState.Added" /> or <see cref="F:System.Data.Entity.EntityState.Detached" />
        /// when either  source  or  target  is in an
        /// <see cref="F:System.Data.Entity.EntityState.Added" /> state 
        /// or when  state  is not a valid  <see cref="T:System.Data.Entity.EntityState" /> value.
        /// </exception>
        public virtual ObjectStateEntry ChangeRelationshipState(
            object sourceEntity,
            object targetEntity,
            string relationshipName,
            string targetRoleName,
            EntityState relationshipState)
        {
            EntityEntry sourceEntry;
            EntityEntry targetEntry;

            VerifyParametersForChangeRelationshipState(sourceEntity, targetEntity, out sourceEntry, out targetEntry);

            var relatedEnd = sourceEntry.WrappedEntity.RelationshipManager.GetRelatedEndInternal(relationshipName, targetRoleName);

            return ChangeRelationshipState(sourceEntry, targetEntry, relatedEnd, relationshipState);
        }

        private ObjectStateEntry ChangeRelationshipState(
            EntityEntry sourceEntry,
            EntityEntry targetEntry,
            RelatedEnd relatedEnd,
            EntityState relationshipState)
        {
            VerifyInitialStateForChangeRelationshipState(sourceEntry, targetEntry, relatedEnd, relationshipState);

            var relationshipWrapper = new RelationshipWrapper(
                (AssociationSet)relatedEnd.RelationshipSet,
                new KeyValuePair<string, EntityKey>(relatedEnd.SourceRoleName, sourceEntry.EntityKey),
                new KeyValuePair<string, EntityKey>(relatedEnd.TargetRoleName, targetEntry.EntityKey));

            var relationshipEntry = FindRelationship(relationshipWrapper);

            if (relationshipEntry == null
                && relationshipState == EntityState.Detached)
            {
                // no-op
                return null;
            }

            TransactionManager.BeginLocalPublicAPI();
            try
            {
                if (relationshipEntry != null)
                {
                    relationshipEntry.ChangeRelationshipState(targetEntry, relatedEnd, relationshipState);
                }
                else
                {
                    relationshipEntry = CreateRelationship(targetEntry, relatedEnd, relationshipWrapper, relationshipState);
                }
            }
            finally
            {
                TransactionManager.EndLocalPublicAPI();
            }

            Debug.Assert(
                relationshipState != EntityState.Detached || relationshipEntry.State == EntityState.Detached, "state should be detached");
            return relationshipState == EntityState.Detached ? null : relationshipEntry;
        }

        private void VerifyParametersForChangeRelationshipState(
            object sourceEntity, object targetEntity, out EntityEntry sourceEntry, out EntityEntry targetEntry)
        {
            DebugCheck.NotNull(sourceEntity);
            DebugCheck.NotNull(targetEntity);

            sourceEntry = GetEntityEntryByObjectOrEntityKey(sourceEntity);
            targetEntry = GetEntityEntryByObjectOrEntityKey(targetEntity);
        }

        private static void VerifyInitialStateForChangeRelationshipState(
            EntityEntry sourceEntry, EntityEntry targetEntry, RelatedEnd relatedEnd, EntityState relationshipState)
        {
            relatedEnd.VerifyType(targetEntry.WrappedEntity);

            if (relatedEnd.IsForeignKey)
            {
                throw new NotSupportedException(Strings.ObjectStateManager_ChangeRelationshipStateNotSupportedForForeignKeyAssociations);
            }

            EntityUtil.CheckValidStateForChangeRelationshipState(relationshipState, "relationshipState");

            if ((sourceEntry.State == EntityState.Deleted || targetEntry.State == EntityState.Deleted)
                &&
                (relationshipState != EntityState.Deleted && relationshipState != EntityState.Detached))
            {
                throw new InvalidOperationException(Strings.ObjectStateManager_CannotChangeRelationshipStateEntityDeleted);
            }

            if ((sourceEntry.State == EntityState.Added || targetEntry.State == EntityState.Added)
                &&
                (relationshipState != EntityState.Added && relationshipState != EntityState.Detached))
            {
                throw new InvalidOperationException(Strings.ObjectStateManager_CannotChangeRelationshipStateEntityAdded);
            }
        }

        private RelationshipEntry CreateRelationship(
            EntityEntry targetEntry, RelatedEnd relatedEnd, RelationshipWrapper relationshipWrapper, EntityState requestedState)
        {
            Debug.Assert(requestedState != EntityState.Modified, "relationship cannot be in Modified state");

            RelationshipEntry relationshipEntry = null;

            switch (requestedState)
            {
                case EntityState.Added:
                    relatedEnd.Add(
                        targetEntry.WrappedEntity,
                        applyConstraints: true,
                        addRelationshipAsUnchanged: false,
                        relationshipAlreadyExists: false,
                        allowModifyingOtherEndOfRelationship: false,
                        forceForeignKeyChanges: true);
                    relationshipEntry = FindRelationship(relationshipWrapper);
                    Debug.Assert(relationshipEntry != null, "null relationshipEntry");
                    break;
                case EntityState.Unchanged:
                    relatedEnd.Add(
                        targetEntry.WrappedEntity,
                        applyConstraints: true,
                        addRelationshipAsUnchanged: false,
                        relationshipAlreadyExists: false,
                        allowModifyingOtherEndOfRelationship: false,
                        forceForeignKeyChanges: true);
                    relationshipEntry = FindRelationship(relationshipWrapper);
                    relationshipEntry.AcceptChanges();
                    break;
                case EntityState.Deleted:
                    relationshipEntry = AddNewRelation(relationshipWrapper, EntityState.Deleted);
                    break;
                case EntityState.Detached:
                    // no-op
                    break;
                default:
                    Debug.Assert(false, "Invalid requested state");
                    break;
            }

            return relationshipEntry;
        }

        private EntityEntry GetEntityEntryByObjectOrEntityKey(object o)
        {
            var key = o as EntityKey;
            var entry = (key != null)
                            ? FindEntityEntry(key)
                            : FindEntityEntry(o);

            if (entry == null)
            {
                throw new InvalidOperationException(Strings.ObjectStateManager_NoEntryExistsForObject(o.GetType().FullName));
            }

            if (entry.IsKeyEntry)
            {
                throw new InvalidOperationException(Strings.ObjectStateManager_CannotChangeRelationshipStateKeyEntry);
            }

            return entry;
        }

        // <summary>
        // Retrieve the corresponding IEntityStateEntry for the given EntityKey.
        // </summary>
        // <exception cref="ArgumentNullException">if key is null</exception>
        // <exception cref="ArgumentException">if key is not found</exception>
        IEntityStateEntry IEntityStateManager.GetEntityStateEntry(EntityKey key)
        {
            return GetEntityEntry(key);
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" /> for the object or relationship entry with the specified key.
        /// </summary>
        /// <returns>
        /// The corresponding <see cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" /> for the given
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKey" />
        /// .
        /// </returns>
        /// <param name="key">
        /// The <see cref="T:System.Data.Entity.Core.EntityKey" />.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">When  key  is null.</exception>
        /// <exception cref="T:System.ArgumentException">When the specified  key  cannot be found in the state manager.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// No entity with the specified <see cref="T:System.Data.Entity.Core.EntityKey" /> exists in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectStateManager" />
        /// .
        /// </exception>
        public virtual ObjectStateEntry GetObjectStateEntry(EntityKey key)
        {
            ObjectStateEntry entry;
            if (!TryGetObjectStateEntry(key, out entry))
            {
                throw new InvalidOperationException(Strings.ObjectStateManager_NoEntryExistForEntityKey);
            }
            return entry;
        }

        internal virtual EntityEntry GetEntityEntry(EntityKey key)
        {
            EntityEntry entry;
            if (!TryGetEntityEntry(key, out entry))
            {
                throw new InvalidOperationException(Strings.ObjectStateManager_NoEntryExistForEntityKey);
            }
            return entry;
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" /> for the specified object.
        /// </summary>
        /// <returns>
        /// The corresponding <see cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" /> for the given
        /// <see
        ///     cref="T:System.Object" />
        /// .
        /// </returns>
        /// <param name="entity">
        /// The <see cref="T:System.Object" /> to which the retrieved
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" />
        /// belongs.
        /// </param>
        /// <exception cref="T:System.InvalidOperationException">
        /// No entity for the specified <see cref="T:System.Object" /> exists in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectStateManager" />
        /// .
        /// </exception>
        public virtual ObjectStateEntry GetObjectStateEntry(object entity)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            ObjectStateEntry entry;
            if (!TryGetObjectStateEntry(entity, out entry))
            {
                throw new InvalidOperationException(Strings.ObjectStateManager_NoEntryExistsForObject(entity.GetType().FullName));
            }
            return entry;
        }

        internal virtual EntityEntry GetEntityEntry(object entity)
        {
            DebugCheck.NotNull(entity);
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");

            var entry = FindEntityEntry(entity);
            if (entry == null)
            {
                throw new InvalidOperationException(Strings.ObjectStateManager_NoEntryExistsForObject(entity.GetType().FullName));
            }
            return entry;
        }

        /// <summary>
        /// Tries to retrieve the corresponding <see cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" /> for the specified
        /// <see
        ///     cref="T:System.Object" />
        /// .
        /// </summary>
        /// <returns>
        /// A Boolean value that is true if there is a corresponding
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" />
        /// for the given object; otherwise, false.
        /// </returns>
        /// <param name="entity">
        /// The <see cref="T:System.Object" /> to which the retrieved
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" />
        /// belongs.
        /// </param>
        /// <param name="entry">
        /// When this method returns, contains the <see cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" /> for the given
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKey" />
        /// This parameter is passed uninitialized.
        /// </param>
        public virtual bool TryGetObjectStateEntry(object entity, out ObjectStateEntry entry)
        {
            Check.NotNull(entity, "entity");
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            entry = null;

            var entityKey = entity as EntityKey;
            if (entityKey != null)
            {
                return TryGetObjectStateEntry(entityKey, out entry);
            }
            else
            {
                entry = FindEntityEntry(entity);
            }

            return entry != null;
        }

        // <summary>
        // Retrieve the corresponding IEntityStateEntry for the given EntityKey.
        // </summary>
        // <returns> true if the corresponding IEntityStateEntry was found </returns>
        // <exception cref="ArgumentNullException">if key is null</exception>
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

        // <summary>
        // Given a key that represents an entity on the dependent side of a FK, this method attempts to return the key of the
        // entity on the principal side of the FK.  If the two entities both exist in the context, then the primary key of
        // the principal entity is found and returned.  If the principal entity does not exist in the context, then a key
        // for it is built up from the foreign key values contained in the dependent entity.
        // </summary>
        // <param name="dependentKey"> The key of the dependent entity </param>
        // <param name="principalRole"> The role indicating the FK to navigate </param>
        // <param name="principalKey"> Set to the principal key or null on return </param>
        // <returns> True if the principal key was found or built; false if it could not be found or built </returns>
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
        /// Tries to retrieve the corresponding <see cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" /> for the object or relationship with the specified
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKey" />
        /// .
        /// </summary>
        /// <returns>
        /// A Boolean value that is true if there is a corresponding
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" />
        /// for the given
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKey" />
        /// ; otherwise, false.
        /// </returns>
        /// <param name="key">
        /// The given <see cref="T:System.Data.Entity.Core.EntityKey" />.
        /// </param>
        /// <param name="entry">
        /// When this method returns, contains an <see cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" /> for the given
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKey" />
        /// This parameter is passed uninitialized.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">A null (Nothing in Visual Basic) value is provided for  key .</exception>
        public virtual bool TryGetObjectStateEntry(EntityKey key, out ObjectStateEntry entry)
        {
            bool result;
            EntityEntry entityEntry;
            result = TryGetEntityEntry(key, out entityEntry);
            entry = entityEntry;
            return result;
        }

        internal virtual bool TryGetEntityEntry(EntityKey key, out EntityEntry entry)
        {
            DebugCheck.NotNull(key);

            entry = null; // must set before checking for null key
            bool result;
            if (key.IsTemporary)
            {
                // only temporary keys exist in the added state
                result = ((null != _addedEntityStore) && _addedEntityStore.TryGetValue(key, out entry));
            }
            else
            {
                // temporary keys do not exist in the unchanged, modified, deleted states.
                result = (((null != _unchangedEntityStore) && _unchangedEntityStore.TryGetValue(key, out entry)) ||
                          ((null != _modifiedEntityStore) && _modifiedEntityStore.TryGetValue(key, out entry)) ||
                          ((null != _deletedEntityStore) && _deletedEntityStore.TryGetValue(key, out entry)));
            }
            Debug.Assert(result == (null != entry), "result and entry mismatch");
            return result;
        }

        internal virtual EntityEntry FindEntityEntry(EntityKey key)
        {
            EntityEntry entry = null;
            if (null != (object)key)
            {
                TryGetEntityEntry(key, out entry);
            }
            return entry;
        }

        // <summary>
        // Retrieve the corresponding EntityEntry for the given entity.
        // Returns null if key is unavailable or passed entity is null.
        // </summary>
        internal virtual EntityEntry FindEntityEntry(object entity)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            Debug.Assert(!(entity is EntityKey), "Object is a EntityKey instead of raw entity.");
            EntityEntry entry = null;
            var entityWithKey = entity as IEntityWithKey;

            if (entityWithKey != null)
            {
                var entityEntityKey = entityWithKey.EntityKey;
                if (null != (object)entityEntityKey)
                {
                    TryGetEntityEntry(entityEntityKey, out entry);
                }
            }
            else
            {
                TryGetEntryFromKeylessStore(entity, out entry);
            }

            // If entity is detached, then entry.Entity won't have the same object reference.
            // This can happen if the same entity is loaded with, then without, tracking
            // SQL BU Defect Tracking 520058
            if (entry != null
                && !ReferenceEquals(entity, entry.Entity))
            {
                entry = null;
            }

            return entry;
        }

        /// <summary>
        /// Returns the <see cref="T:System.Data.Entity.Core.Objects.DataClasses.RelationshipManager" /> that is used by the specified object.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Objects.DataClasses.RelationshipManager" /> for the specified object.
        /// </returns>
        /// <param name="entity">
        /// The object for which to return the <see cref="T:System.Data.Entity.Core.Objects.DataClasses.RelationshipManager" />.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The entity does not implement IEntityWithRelationships and is not tracked by this ObjectStateManager
        /// </exception>
        public virtual RelationshipManager GetRelationshipManager(object entity)
        {
            RelationshipManager rm;
            if (!TryGetRelationshipManager(entity, out rm))
            {
                throw new InvalidOperationException(Strings.ObjectStateManager_CannotGetRelationshipManagerForDetachedPocoEntity);
            }
            return rm;
        }

        /// <summary>
        /// Returns the <see cref="T:System.Data.Entity.Core.Objects.DataClasses.RelationshipManager" /> that is used by the specified object.
        /// </summary>
        /// <returns>
        /// true if a <see cref="T:System.Data.Entity.Core.Objects.DataClasses.RelationshipManager" /> instance was returned for the supplied  entity ; otherwise false.
        /// </returns>
        /// <param name="entity">
        /// The object for which to return the <see cref="T:System.Data.Entity.Core.Objects.DataClasses.RelationshipManager" />.
        /// </param>
        /// <param name="relationshipManager">
        /// When this method returns, contains the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Objects.DataClasses.RelationshipManager" />
        /// for the  entity .
        /// </param>
        public virtual bool TryGetRelationshipManager(object entity, out RelationshipManager relationshipManager)
        {
            Check.NotNull(entity, "entity");
            var withRelationships = entity as IEntityWithRelationships;
            if (withRelationships != null)
            {
                relationshipManager = withRelationships.RelationshipManager;
                if (relationshipManager == null)
                {
                    throw new InvalidOperationException(Strings.RelationshipManager_UnexpectedNull);
                }
                if (relationshipManager.WrappedOwner.Entity != entity)
                {
                    throw new InvalidOperationException(Strings.RelationshipManager_InvalidRelationshipManagerOwner);
                }
            }
            else
            {
                var wrappedEntity = EntityWrapperFactory.WrapEntityUsingStateManager(entity, this);
                if (wrappedEntity.Context == null)
                {
                    relationshipManager = null;
                    return false;
                }
                relationshipManager = wrappedEntity.RelationshipManager;
            }
            return true;
        }

        internal virtual void ChangeState(RelationshipEntry entry, EntityState oldState, EntityState newState)
        {
            if (newState == EntityState.Detached)
            {
                // If we're transitioning to detached, completely remove all traces of the entry.
                DeleteRelationshipFromLookup(entry);

                // delay removal until RelationshipEnds is done
                RemoveObjectStateEntryFromDictionary(entry, oldState);

                entry.Reset();
            }
            else
            {
                RemoveObjectStateEntryFromDictionary(entry, oldState);

                // If we're transitioning to something other than detached, add the
                // entry to the appropriate dictionary.
                AddRelationshipEntryToDictionary(entry, newState);
            }

            // do not fire event for relationship
        }

        internal virtual void ChangeState(EntityEntry entry, EntityState oldState, EntityState newState)
        {
            var fireEvent = !entry.IsKeyEntry;
            if (newState == EntityState.Detached)
            {
                // If we're transitioning to detached, completely remove all traces of the entry.

                // SQLBU 508278: Object State Manager should not allow "dangling" relationships to stay in the state manager.
                // Remove potential dangling relationships
                Debug.Assert((object)entry.EntityKey != null, "attached entry must have a key");
                foreach (var relationshipEntry in CopyOfRelationshipsByKey(entry.EntityKey))
                {
                    ChangeState(relationshipEntry, relationshipEntry.State, EntityState.Detached);
                }

                // delay removal until RelationshipEnds is done
                RemoveObjectStateEntryFromDictionary(entry, oldState);

                var wrappedEntity = entry.WrappedEntity; // we have to cache the entity before detaching it totally so we can fire event
                entry.Reset();
                // Prevent firing two events for removal from the context during rollback.
                if (fireEvent
                    && wrappedEntity.Entity != null
                    && !TransactionManager.IsAttachTracking)
                {
                    // first notify the view
                    OnEntityDeleted(CollectionChangeAction.Remove, wrappedEntity.Entity);
                    OnObjectStateManagerChanged(CollectionChangeAction.Remove, wrappedEntity.Entity);
                }
            }
            else
            {
                RemoveObjectStateEntryFromDictionary(entry, oldState);

                // If we're transitioning to something other than detached, add the
                // entry to the appropriate dictionary.
                AddEntityEntryToDictionary(entry, newState);
            }

            if (newState == EntityState.Deleted)
            {
                entry.RemoveFromForeignKeyIndex();
                ForgetEntryWithConceptualNull(entry, resetAllKeys: true);
                if (fireEvent)
                {
                    // fire collectionChanged event only when an entity is being deleted (this includes deleting an added entity which becomes detached)
                    OnEntityDeleted(CollectionChangeAction.Remove, entry.Entity);
                    OnObjectStateManagerChanged(CollectionChangeAction.Remove, entry.Entity);
                }
            }
        }

        private void AddRelationshipEntryToDictionary(RelationshipEntry entry, EntityState state)
        {
            Debug.Assert(entry.IsRelationship, "expecting IsRelationship");
            Debug.Assert(null != entry.RelationshipWrapper, "null RelationshipWrapper");

            Dictionary<RelationshipWrapper, RelationshipEntry> dictionaryToAdd = null;
            switch (state)
            {
                case EntityState.Unchanged:
                    if (null == _unchangedRelationshipStore)
                    {
                        _unchangedRelationshipStore = new Dictionary<RelationshipWrapper, RelationshipEntry>();
                    }
                    dictionaryToAdd = _unchangedRelationshipStore;
                    break;
                case EntityState.Added:
                    if (null == _addedRelationshipStore)
                    {
                        _addedRelationshipStore = new Dictionary<RelationshipWrapper, RelationshipEntry>();
                    }
                    dictionaryToAdd = _addedRelationshipStore;
                    break;
                case EntityState.Deleted:
                    if (null == _deletedRelationshipStore)
                    {
                        _deletedRelationshipStore = new Dictionary<RelationshipWrapper, RelationshipEntry>();
                    }
                    dictionaryToAdd = _deletedRelationshipStore;
                    break;
                default:
                    Debug.Assert(false, "Invalid state.");
                    break;
            }
            Debug.Assert(dictionaryToAdd != null, "Couldn't find the correct relationship dictionary based on entity state.");
            dictionaryToAdd.Add(entry.RelationshipWrapper, entry);
        }

        private void AddEntityEntryToDictionary(EntityEntry entry, EntityState state)
        {
            DebugCheck.NotNull((object)entry.EntityKey);

            if (entry.RequiresAnyChangeTracking)
            {
                _detectChangesNeeded = true;
            }

            Dictionary<EntityKey, EntityEntry> dictionaryToAdd = null;
            switch (state)
            {
                case EntityState.Unchanged:
                    if (null == _unchangedEntityStore)
                    {
                        _unchangedEntityStore = new Dictionary<EntityKey, EntityEntry>();
                    }
                    dictionaryToAdd = _unchangedEntityStore;
                    Debug.Assert(!entry.EntityKey.IsTemporary, "adding temporary entity key into Unchanged state");
                    break;
                case EntityState.Added:
                    if (null == _addedEntityStore)
                    {
                        _addedEntityStore = new Dictionary<EntityKey, EntityEntry>();
                    }
                    dictionaryToAdd = _addedEntityStore;
                    Debug.Assert(entry.EntityKey.IsTemporary, "adding non-temporary entity key into Added state");
                    break;
                case EntityState.Deleted:
                    if (null == _deletedEntityStore)
                    {
                        _deletedEntityStore = new Dictionary<EntityKey, EntityEntry>();
                    }
                    dictionaryToAdd = _deletedEntityStore;
                    Debug.Assert(!entry.EntityKey.IsTemporary, "adding temporary entity key into Deleted state");
                    break;
                case EntityState.Modified:
                    if (null == _modifiedEntityStore)
                    {
                        _modifiedEntityStore = new Dictionary<EntityKey, EntityEntry>();
                    }
                    dictionaryToAdd = _modifiedEntityStore;
                    Debug.Assert(!entry.EntityKey.IsTemporary, "adding temporary entity key into Modified state");
                    break;
                default:
                    Debug.Assert(false, "Invalid state.");
                    break;
            }
            Debug.Assert(dictionaryToAdd != null, "Couldn't find the correct entity dictionary based on entity state.");
            dictionaryToAdd.Add(entry.EntityKey, entry);
            AddEntryToKeylessStore(entry);
        }

        private void AddEntryToKeylessStore(EntityEntry entry)
        {
            // Add an entry that doesn't implement IEntityWithKey to the keyless lookup.
            // It is used to lookup ObjectStateEntries when all we have is an entity reference.
            if (null != entry.Entity
                && !(entry.Entity is IEntityWithKey))
            {
                if (null == _keylessEntityStore)
                {
                    _keylessEntityStore = new Dictionary<object, EntityEntry>(ObjectReferenceEqualityComparer.Default);
                }
                if (!_keylessEntityStore.ContainsKey(entry.Entity))
                {
                    _keylessEntityStore.Add(entry.Entity, entry);
                }
            }
        }

        // <summary>
        // Removes the given cache entry from the appropriate dictionary, based on
        // the given state and whether or not the entry represents a relationship.
        // </summary>
        private void RemoveObjectStateEntryFromDictionary(RelationshipEntry entry, EntityState state)
        {
            // Determine the appropriate dictionary from which to remove the entry.
            Dictionary<RelationshipWrapper, RelationshipEntry> dictionaryContainingEntry = null;
            switch (state)
            {
                case EntityState.Unchanged:
                    dictionaryContainingEntry = _unchangedRelationshipStore;
                    break;
                case EntityState.Added:
                    dictionaryContainingEntry = _addedRelationshipStore;
                    break;
                case EntityState.Deleted:
                    dictionaryContainingEntry = _deletedRelationshipStore;
                    break;
                default:
                    Debug.Assert(false, "Invalid state.");
                    break;
            }
            Debug.Assert(dictionaryContainingEntry != null, "Couldn't find the correct relationship dictionary based on entity state.");

            var result = dictionaryContainingEntry.Remove(entry.RelationshipWrapper);
            Debug.Assert(result, "The correct relationship dictionary based on entity state doesn't contain the entry.");

            if (0 == dictionaryContainingEntry.Count)
            {
                // reduce unused dictionary capacity
                switch (state)
                {
                    case EntityState.Unchanged:
                        _unchangedRelationshipStore = null;
                        break;
                    case EntityState.Added:
                        _addedRelationshipStore = null;
                        break;
                    case EntityState.Deleted:
                        _deletedRelationshipStore = null;
                        break;
                }
            }
        }

        // <summary>
        // Removes the given cache entry from the appropriate dictionary, based on
        // the given state and whether or not the entry represents a relationship.
        // </summary>
        private void RemoveObjectStateEntryFromDictionary(EntityEntry entry, EntityState state)
        {
            Dictionary<EntityKey, EntityEntry> dictionaryContainingEntry = null;
            switch (state)
            {
                case EntityState.Unchanged:
                    dictionaryContainingEntry = _unchangedEntityStore;
                    break;
                case EntityState.Added:
                    dictionaryContainingEntry = _addedEntityStore;
                    break;
                case EntityState.Deleted:
                    dictionaryContainingEntry = _deletedEntityStore;
                    break;
                case EntityState.Modified:
                    dictionaryContainingEntry = _modifiedEntityStore;
                    break;
                default:
                    Debug.Assert(false, "Invalid state.");
                    break;
            }
            Debug.Assert(dictionaryContainingEntry != null, "Couldn't find the correct entity dictionary based on entity state.");

            var result = dictionaryContainingEntry.Remove(entry.EntityKey);
            Debug.Assert(result, "The correct entity dictionary based on entity state doesn't contain the entry.");
            RemoveEntryFromKeylessStore(entry.WrappedEntity);

            if (0 == dictionaryContainingEntry.Count)
            {
                // reduce unused dictionary capacity
                switch (state)
                {
                    case EntityState.Unchanged:
                        _unchangedEntityStore = null;
                        break;
                    case EntityState.Added:
                        _addedEntityStore = null;
                        break;
                    case EntityState.Deleted:
                        _deletedEntityStore = null;
                        break;
                    case EntityState.Modified:
                        _modifiedEntityStore = null;
                        break;
                }
            }
        }

        internal virtual void RemoveEntryFromKeylessStore(IEntityWrapper wrappedEntity)
        {
            // Remove and entry from the store containing entities not implementing IEntityWithKey
            if (null != wrappedEntity
                && null != wrappedEntity.Entity
                && !(wrappedEntity.Entity is IEntityWithKey))
            {
                _keylessEntityStore.Remove(wrappedEntity.Entity);
            }
        }

        // <summary>
        // If a corresponding StateManagerTypeMetadata exists, it is returned.
        // Otherwise, a StateManagerTypeMetadata is created and cached.
        // </summary>
        internal virtual StateManagerTypeMetadata GetOrAddStateManagerTypeMetadata(Type entityType, EntitySet entitySet)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(entitySet);

            StateManagerTypeMetadata typeMetadata;
            if (!_metadataMapping.TryGetValue(new EntitySetQualifiedType(entityType, entitySet), out typeMetadata))
            {
                // GetMap doesn't have a mechanism to qualify identity with EntityContainerName
                // This is unimportant until each EntityContainer can have its own ObjectTypeMapping.
                typeMetadata = AddStateManagerTypeMetadata(
                    entitySet, (ObjectTypeMapping)
                               MetadataWorkspace.GetMap(entityType.FullNameWithNesting(), DataSpace.OSpace, DataSpace.OCSpace));
            }
            return typeMetadata;
        }

        // <summary>
        // If a corresponding StateManagerTypeMetadata exists, it is returned.
        // Otherwise, a StateManagerTypeMetadata is created and cached.
        // </summary>
        internal virtual StateManagerTypeMetadata GetOrAddStateManagerTypeMetadata(EdmType edmType)
        {
            DebugCheck.NotNull(edmType);
            Debug.Assert(
                Helper.IsEntityType(edmType) ||
                Helper.IsComplexType(edmType),
                "only expecting ComplexType or EntityType");

            StateManagerTypeMetadata typeMetadata;
            if (!_metadataStore.TryGetValue(edmType, out typeMetadata))
            {
                typeMetadata = AddStateManagerTypeMetadata(
                    edmType, (ObjectTypeMapping)
                             MetadataWorkspace.GetMap(edmType, DataSpace.OCSpace));
            }
            return typeMetadata;
        }

        // <summary>
        // Creates an instance of StateManagerTypeMetadata from the given EdmType and ObjectMapping,
        // and stores it in the metadata cache.  The new instance is returned.
        // </summary>
        private StateManagerTypeMetadata AddStateManagerTypeMetadata(EntitySet entitySet, ObjectTypeMapping mapping)
        {
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(mapping);

            var edmType = mapping.EdmType;
            Debug.Assert(
                Helper.IsEntityType(edmType) ||
                Helper.IsComplexType(edmType),
                "not Entity or complex type");

            StateManagerTypeMetadata typeMetadata;
            if (!_metadataStore.TryGetValue(edmType, out typeMetadata))
            {
                typeMetadata = new StateManagerTypeMetadata(edmType, mapping);
                _metadataStore.Add(edmType, typeMetadata);
            }

            var entitySetQualifiedType = new EntitySetQualifiedType(mapping.ClrType.ClrType, entitySet);
            if (!_metadataMapping.ContainsKey(entitySetQualifiedType))
            {
                _metadataMapping.Add(entitySetQualifiedType, typeMetadata);
            }
            else
            {
                throw new InvalidOperationException(
                    Strings.Mapping_CannotMapCLRTypeMultipleTimes(typeMetadata.CdmMetadata.EdmType.FullName));
            }
            return typeMetadata;
        }

        private StateManagerTypeMetadata AddStateManagerTypeMetadata(EdmType edmType, ObjectTypeMapping mapping)
        {
            DebugCheck.NotNull(edmType);
            Debug.Assert(
                Helper.IsEntityType(edmType) ||
                Helper.IsComplexType(edmType),
                "not Entity or complex type");

            var typeMetadata = new StateManagerTypeMetadata(edmType, mapping);
            _metadataStore.Add(edmType, typeMetadata);
            return typeMetadata;
        }

        // <summary>
        // Mark the ObjectStateManager as disposed
        // </summary>
        internal virtual void Dispose()
        {
            _isDisposed = true;
        }

        internal virtual bool IsDisposed
        {
            get { return _isDisposed; }
        }

        // <summary>
        // For every tracked entity which doesn't implement IEntityWithChangeTracker detect changes in the entity's property values
        // and marks appropriate ObjectStateEntry as Modified.
        // For every tracked entity which doesn't implement IEntityWithRelationships detect changes in its relationships.
        // The method is used internally by ObjectContext.SaveChanges() but can be also used if user wants to detect changes
        // and have ObjectStateEntries in appropriate state before the SaveChanges() method is called.
        // </summary>
        internal virtual void DetectChanges()
        {
            var entries = GetEntityEntriesForDetectChanges();
            if (entries == null)
            {
                return;
            }

            if (TransactionManager.BeginDetectChanges())
            {
                try
                {
                    // Populate TransactionManager.DeletedRelationshipsByGraph and TransactionManager.AddedRelationshipsByGraph
                    DetectChangesInNavigationProperties(entries);

                    // Populate TransactionManager.ChangedForeignKeys
                    DetectChangesInScalarAndComplexProperties(entries);

                    // Populate TransactionManager.DeletedRelationshipsByForeignKey and TransactionManager.AddedRelationshipsByForeignKey
                    DetectChangesInForeignKeys(entries);

                    // Detect conflicts between changes to FK and navigation properties
                    DetectConflicts(entries);

                    // Update graph and FKs
                    TransactionManager.BeginAlignChanges();
                    AlignChangesInRelationships(entries);
                }
                finally
                {
                    TransactionManager.EndAlignChanges();
                    TransactionManager.EndDetectChanges();
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void DetectConflicts(IList<EntityEntry> entries)
        {
            var tm = TransactionManager;
            foreach (var entry in entries)
            {
                //NOTE: DetectChangesInNavigationProperties will have created two navigation changes
                //      even if the user has only made a single change in there graph, this means we
                //      only need to check for conflicts on the local end of the relationship.

                //Find all relationships being added for this entity
                Dictionary<RelatedEnd, HashSet<IEntityWrapper>> addedRelationshipsByGraph;
                tm.AddedRelationshipsByGraph.TryGetValue(entry.WrappedEntity, out addedRelationshipsByGraph);
                Dictionary<RelatedEnd, HashSet<EntityKey>> addedRelationshipsByForeignKey;
                tm.AddedRelationshipsByForeignKey.TryGetValue(entry.WrappedEntity, out addedRelationshipsByForeignKey);

                //Ensure new graph relationships do not involve a Deleted Entity
                if (addedRelationshipsByGraph != null
                    && addedRelationshipsByGraph.Count > 0)
                {
                    if (entry.State
                        == EntityState.Deleted)
                    {
                        throw new InvalidOperationException(Strings.RelatedEnd_UnableToAddRelationshipWithDeletedEntity);
                    }
                }

                //Check for conflicting FK changes and changes to PKs
                if (addedRelationshipsByForeignKey != null)
                {
                    foreach (var pair in addedRelationshipsByForeignKey)
                    {
                        //Ensure persisted dependents of identifying FK relationships are not being re-parented
                        if (entry.State == EntityState.Unchanged
                            || entry.State == EntityState.Modified)
                        {
                            if (pair.Key.IsDependentEndOfReferentialConstraint(true)
                                && pair.Value.Count > 0)
                            {
                                throw new InvalidOperationException(Strings.EntityReference_CannotChangeReferentialConstraintProperty);
                            }
                        }

                        //Make sure each EntityReference only has one FK change 
                        //(It's possible to have more than one in an identifying 1:1/0..1 relationship 
                        // when two dependent FKs are set to match one principal)
                        var reference = pair.Key as EntityReference;
                        if (reference != null)
                        {
                            if (pair.Value.Count > 1)
                            {
                                throw new InvalidOperationException(
                                    Strings.ObjectStateManager_ConflictingChangesOfRelationshipDetected(
                                        pair.Key.RelationshipNavigation.To,
                                        pair.Key.RelationshipNavigation.RelationshipName));
                            }
                        }
                    }
                }

                //Check for conflicting reference changes and changes that will change a PK
                if (addedRelationshipsByGraph != null)
                {
                    // Retrieve key values from related entities
                    var properties = new Dictionary<string, KeyValuePair<object, IntBox>>();

                    foreach (var pair in addedRelationshipsByGraph)
                    {
                        //Ensure persisted dependents of identifying FK relationships are not being re-parented
                        if (pair.Key.IsForeignKey
                            && (entry.State == EntityState.Unchanged || entry.State == EntityState.Modified))
                        {
                            //Any reference change is invalid because it is not possible to have a persisted 
                            //principal that matches the dependents key without the reference already being set
                            if (pair.Key.IsDependentEndOfReferentialConstraint(true)
                                && pair.Value.Count > 0)
                            {
                                throw new InvalidOperationException(Strings.EntityReference_CannotChangeReferentialConstraintProperty);
                            }
                        }

                        //Check that each EntityReference only has one reference change
                        //AND that the change agrees with the FK change if present
                        var reference = pair.Key as EntityReference;
                        if (reference != null)
                        {
                            if (pair.Value.Count > 1)
                            {
                                throw new InvalidOperationException(
                                    Strings.ObjectStateManager_ConflictingChangesOfRelationshipDetected(
                                        pair.Key.RelationshipNavigation.To,
                                        pair.Key.RelationshipNavigation.RelationshipName));
                            }
                            else if (pair.Value.Count == 1)
                            {
                                //We know there is a max of one FK change as we checked this already
                                var addedEntity = pair.Value.First();

                                //See if there is also a new FK for this RelatedEnd
                                HashSet<EntityKey> newFks = null;
                                if (addedRelationshipsByForeignKey != null)
                                {
                                    addedRelationshipsByForeignKey.TryGetValue(pair.Key, out newFks);
                                }
                                else
                                {
                                    // Try the principal key dictionary to see if there is a conflict on the principal side
                                    Dictionary<RelatedEnd, HashSet<EntityKey>> addedRelationshipsByPrincipalKey;
                                    if (tm.AddedRelationshipsByPrincipalKey.TryGetValue(
                                        entry.WrappedEntity, out addedRelationshipsByPrincipalKey))
                                    {
                                        addedRelationshipsByPrincipalKey.TryGetValue(pair.Key, out newFks);
                                    }
                                }

                                if (newFks != null
                                    && newFks.Count > 0)
                                {
                                    //Make sure the FK change is consistent with the Reference change
                                    //The following call sometimes creates permanent key of Added entity
                                    var addedKey = GetPermanentKey(entry.WrappedEntity, reference, addedEntity);

                                    if (addedKey != newFks.First())
                                    {
                                        throw new InvalidOperationException(
                                            Strings.ObjectStateManager_ConflictingChangesOfRelationshipDetected(
                                                reference.RelationshipNavigation.To,
                                                reference.RelationshipNavigation.RelationshipName));
                                    }
                                }
                                else
                                {
                                    //If there is no added FK relationship but there is a deleted one then it means
                                    //the FK has been nulled and this will always conflict with an added reference
                                    Dictionary<RelatedEnd, HashSet<EntityKey>> deletedRelationshipsByForeignKey;
                                    if (tm.DeletedRelationshipsByForeignKey.TryGetValue(
                                        entry.WrappedEntity, out deletedRelationshipsByForeignKey))
                                    {
                                        HashSet<EntityKey> removedKeys;
                                        if (deletedRelationshipsByForeignKey.TryGetValue(pair.Key, out removedKeys))
                                        {
                                            if (removedKeys.Count > 0)
                                            {
                                                throw new InvalidOperationException(
                                                    Strings.ObjectStateManager_ConflictingChangesOfRelationshipDetected(
                                                        reference.RelationshipNavigation.To,
                                                        reference.RelationshipNavigation.RelationshipName));
                                            }
                                        }
                                    }
                                }

                                // For each change to the graph, validate that the entity will not have conflicting 
                                //   RI constrained property values
                                // The related entity is detached or added, these are valid cases 
                                //   so do not consider their changes in conflict
                                var relatedEntry = FindEntityEntry(addedEntity.Entity);
                                if (relatedEntry != null 
                                    && (relatedEntry.State == EntityState.Unchanged
                                    || relatedEntry.State == EntityState.Modified))
                                {
                                    var retrievedProperties = new Dictionary<string, KeyValuePair<object, IntBox>>();
                                    relatedEntry.GetOtherKeyProperties(retrievedProperties);
                                    // Merge retrievedProperties into the main list of properties
                                    foreach (var constraint in ((AssociationType)reference.RelationMetadata).ReferentialConstraints)
                                    {
                                        if (constraint.ToRole == reference.FromEndMember)
                                        {
                                            for (var i = 0; i < constraint.FromProperties.Count; ++i)
                                            {
                                                EntityEntry.AddOrIncreaseCounter(
                                                    constraint,
                                                    properties,
                                                    constraint.ToProperties[i].Name,
                                                    retrievedProperties[constraint.FromProperties[i].Name].Key);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal virtual EntityKey GetPermanentKey(IEntityWrapper entityFrom, RelatedEnd relatedEndFrom, IEntityWrapper entityTo)
        {
            EntityKey entityKey = null;
            if (entityTo.ObjectStateEntry != null)
            {
                entityKey = entityTo.ObjectStateEntry.EntityKey;
            }
            if (entityKey == null
                || entityKey.IsTemporary)
            {
                entityKey = CreateEntityKey(GetEntitySetOfOtherEnd(entityFrom, relatedEndFrom), entityTo.Entity);
            }
            return entityKey;
        }

        private static EntitySet GetEntitySetOfOtherEnd(IEntityWrapper entity, RelatedEnd relatedEnd)
        {
            var associationSet = (AssociationSet)relatedEnd.RelationshipSet;

            var entitySet = associationSet.AssociationSetEnds[0].EntitySet;
            if (entitySet.Name
                != entity.EntityKey.EntitySetName)
            {
                return entitySet;
            }
            else
            {
                return associationSet.AssociationSetEnds[1].EntitySet;
            }
        }

        private static void DetectChangesInForeignKeys(IList<EntityEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added
                    || entry.State == EntityState.Modified)
                {
                    entry.DetectChangesInForeignKeys();
                }
            }
        }

        private void AlignChangesInRelationships(IList<EntityEntry> entries)
        {
            PerformDelete(entries);
            PerformAdd(entries);
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void PerformAdd(IList<EntityEntry> entries)
        {
            var tm = TransactionManager;

            foreach (var entry in entries)
            {
                if (entry.State != EntityState.Detached
                    &&
                    !entry.IsKeyEntry) // Still need to check this here because entries may have been demoted
                {
                    foreach (var relatedEnd in entry.WrappedEntity.RelationshipManager.Relationships)
                    {
                        // find EntityKey of objects added to relatedEnd by changes of FKs

                        HashSet<EntityKey> entityKeysOfAddedObjects = null;

                        Dictionary<RelatedEnd, HashSet<EntityKey>> addedRelationshipsByForeignKey;
                        if (relatedEnd is EntityReference
                            &&
                            tm.AddedRelationshipsByForeignKey.TryGetValue(entry.WrappedEntity, out addedRelationshipsByForeignKey))
                        {
                            addedRelationshipsByForeignKey.TryGetValue(relatedEnd, out entityKeysOfAddedObjects);
                        }

                        // find IEntityWrappers of objects added to relatedEnd by changes to navigation property

                        Dictionary<RelatedEnd, HashSet<IEntityWrapper>> addedRelationshipsByGraph;
                        HashSet<IEntityWrapper> entitiesToAdd = null;
                        if (tm.AddedRelationshipsByGraph.TryGetValue(entry.WrappedEntity, out addedRelationshipsByGraph))
                        {
                            addedRelationshipsByGraph.TryGetValue(relatedEnd, out entitiesToAdd);
                        }

                        // merge the 2 sets into one (destroys entitiesToAdd)

                        // Perform Add of FK or FK + Reference changes
                        if (entityKeysOfAddedObjects != null)
                        {
                            EntityEntry relatedEntry;

                            foreach (var entityKeyOfAddedObjects in entityKeysOfAddedObjects)
                            {
                                // we are interested only in tracked non-Added entities
                                if (TryGetEntityEntry(entityKeyOfAddedObjects, out relatedEntry)
                                    &&
                                    relatedEntry.WrappedEntity.Entity != null)
                                {
                                    entitiesToAdd = entitiesToAdd != null ? entitiesToAdd : new HashSet<IEntityWrapper>();
                                    // if the change comes only from the FK and the FK is to a deleted entity
                                    // then we do not do fixup to align to that entity so do not add those
                                    // implementation note: we do not need to check for contains because if it's there we don't need to add it
                                    if (relatedEntry.State
                                        != EntityState.Deleted)
                                    {
                                        // Remove it from the list of entities to add by reference because it will be added now
                                        entitiesToAdd.Remove(relatedEntry.WrappedEntity);

                                        PerformAdd(entry.WrappedEntity, relatedEnd, relatedEntry.WrappedEntity, true);
                                    }
                                }
                                else
                                {
                                    // Need to update the CFK and dangling FK references even if there is no related entity
                                    var reference = relatedEnd as EntityReference;
                                    Debug.Assert(reference != null);
                                    entry.FixupEntityReferenceByForeignKey(reference);
                                }
                            }
                        }

                        // Perform Add for Reference changes
                        if (entitiesToAdd != null)
                        {
                            foreach (var entityToAdd in entitiesToAdd)
                            {
                                PerformAdd(entry.WrappedEntity, relatedEnd, entityToAdd, false);
                            }
                        }
                    }
                }
            }
        }

        private void PerformAdd(IEntityWrapper wrappedOwner, RelatedEnd relatedEnd, IEntityWrapper entityToAdd, bool isForeignKeyChange)
        {
            Debug.Assert(wrappedOwner == relatedEnd.WrappedOwner, "entry.WrappedEntity is not the same as relatedEnd.WrappedOwner?");

            relatedEnd.ValidateStateForAdd(relatedEnd.WrappedOwner);
            relatedEnd.ValidateStateForAdd(entityToAdd);

            // We need to determine if adding entityToAdd is going to cause reparenting
            // if relatedEnd is a principal then
            //   Get the target relatedEnd on entityToAdd to check if we are in this situation
            // if relatedEnd is a dependent then
            //   Check 
            if (relatedEnd.IsPrincipalEndOfReferentialConstraint())
            {
                var targetReference = relatedEnd.GetOtherEndOfRelationship(entityToAdd) as EntityReference;
                if (targetReference != null
                    && IsReparentingReference(entityToAdd, targetReference))
                {
                    TransactionManager.EntityBeingReparented =
                        targetReference.GetDependentEndOfReferentialConstraint(targetReference.ReferenceValue.Entity);
                }
            }
            else if (relatedEnd.IsDependentEndOfReferentialConstraint(checkIdentifying: false))
            {
                var reference = relatedEnd as EntityReference;
                if (reference != null
                    && IsReparentingReference(wrappedOwner, reference))
                {
                    TransactionManager.EntityBeingReparented =
                        reference.GetDependentEndOfReferentialConstraint(reference.ReferenceValue.Entity);
                }
            }
            try
            {
                relatedEnd.Add(
                    entityToAdd,
                    applyConstraints: false,
                    addRelationshipAsUnchanged: false,
                    relationshipAlreadyExists: false,
                    allowModifyingOtherEndOfRelationship: true,
                    forceForeignKeyChanges: !isForeignKeyChange);
            }
            finally
            {
                TransactionManager.EntityBeingReparented = null;
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void PerformDelete(IList<EntityEntry> entries)
        {
            var tm = TransactionManager;

            foreach (var entry in entries)
            {
                if (entry.State != EntityState.Detached
                    &&
                    entry.State != EntityState.Deleted
                    &&
                    !entry.IsKeyEntry) // Still need to check this here because entries may have been demoted
                {
                    foreach (var relatedEnd in entry.WrappedEntity.RelationshipManager.Relationships)
                    {
                        // find EntityKey of objects deleted from relatedEnd by changes of FKs

                        HashSet<EntityKey> entityKeysOfDeletedObjects = null;
                        var reference = relatedEnd as EntityReference;

                        Dictionary<RelatedEnd, HashSet<EntityKey>> deletedRelationshipsByForeignKey;
                        if (reference != null
                            &&
                            tm.DeletedRelationshipsByForeignKey.TryGetValue(entry.WrappedEntity, out deletedRelationshipsByForeignKey))
                        {
                            deletedRelationshipsByForeignKey.TryGetValue(reference, out entityKeysOfDeletedObjects);
                        }

                        // find IEntityWrappers of objects deleted from relatedEnd by changes to navigation property

                        Dictionary<RelatedEnd, HashSet<IEntityWrapper>> deletedRelationshipsByGraph;
                        HashSet<IEntityWrapper> entitiesToDelete = null;
                        if (tm.DeletedRelationshipsByGraph.TryGetValue(entry.WrappedEntity, out deletedRelationshipsByGraph))
                        {
                            deletedRelationshipsByGraph.TryGetValue(relatedEnd, out entitiesToDelete);
                        }

                        // Perform the deletes:
                        // 1. FK only OR combined FK/Ref changes (same change to both FK and reference)
                        if (entityKeysOfDeletedObjects != null)
                        {
                            foreach (var key in entityKeysOfDeletedObjects)
                            {
                                EntityEntry relatedEntry;
                                IEntityWrapper relatedEntity = null;
                                if (TryGetEntityEntry(key, out relatedEntry)
                                    &&
                                    relatedEntry.WrappedEntity.Entity != null)
                                {
                                    relatedEntity = relatedEntry.WrappedEntity;
                                }
                                else
                                {
                                    // The relatedEntity may be added, and we only have a permanent key 
                                    //  so look at the permanent key of the reference to decide
                                    if (reference != null
                                        &&
                                        reference.ReferenceValue != NullEntityWrapper.NullWrapper
                                        &&
                                        reference.ReferenceValue.EntityKey.IsTemporary
                                        &&
                                        TryGetEntityEntry(reference.ReferenceValue.EntityKey, out relatedEntry)
                                        &&
                                        relatedEntry.WrappedEntity.Entity != null)
                                    {
                                        var permanentRelatedKey = new EntityKey(
                                            (EntitySet)relatedEntry.EntitySet, relatedEntry.CurrentValues);
                                        if (key == permanentRelatedKey)
                                        {
                                            relatedEntity = relatedEntry.WrappedEntity;
                                        }
                                    }
                                }

                                if (relatedEntity != null)
                                {
                                    entitiesToDelete = entitiesToDelete != null ? entitiesToDelete : new HashSet<IEntityWrapper>();
                                    // if the reference also changed, we will remove that now
                                    // if only the FK changed, it will not be in the list entitiesToDelete and 
                                    //  so we should preserve the FK value
                                    // if the reference is being set to null, (was a delete, but not an add) 
                                    //  then we need to preserve the FK values regardless
                                    var preserveForeignKey = ShouldPreserveForeignKeyForDependent(
                                        entry.WrappedEntity, relatedEnd, relatedEntity, entitiesToDelete);
                                    // No need to also do a graph remove of the same value
                                    entitiesToDelete.Remove(relatedEntity);
                                    if (reference != null
                                        && IsReparentingReference(entry.WrappedEntity, reference))
                                    {
                                        TransactionManager.EntityBeingReparented =
                                            reference.GetDependentEndOfReferentialConstraint(reference.ReferenceValue.Entity);
                                    }
                                    try
                                    {
                                        relatedEnd.Remove(relatedEntity, preserveForeignKey);
                                    }
                                    finally
                                    {
                                        TransactionManager.EntityBeingReparented = null;
                                    }
                                    // stop trying to remove something, if the owner was detached or deleted because of RIC/cascade delete
                                    if (entry.State == EntityState.Detached
                                        || entry.State == EntityState.Deleted
                                        || entry.IsKeyEntry)
                                    {
                                        break;
                                    }
                                }
                                if (reference != null
                                    &&
                                    reference.IsForeignKey
                                    &&
                                    reference.IsDependentEndOfReferentialConstraint(checkIdentifying: false))
                                {
                                    // Ensure that the cached FK value on the reference is in sync because it is possible that we
                                    // didn't take any actions above that would cause this to be set.
                                    reference.SetCachedForeignKey(ForeignKeyFactory.CreateKeyFromForeignKeyValues(entry, reference), entry);
                                }
                            }
                        }

                        // 2. Changes to the reference only
                        if (entitiesToDelete != null)
                        {
                            foreach (var entityToDelete in entitiesToDelete)
                            {
                                var preserveForeignKey = ShouldPreserveForeignKeyForPrincipal(
                                    entry.WrappedEntity, relatedEnd, entityToDelete, entitiesToDelete);
                                if (reference != null
                                    && IsReparentingReference(entry.WrappedEntity, reference))
                                {
                                    TransactionManager.EntityBeingReparented =
                                        reference.GetDependentEndOfReferentialConstraint(reference.ReferenceValue.Entity);
                                }
                                try
                                {
                                    relatedEnd.Remove(entityToDelete, preserveForeignKey);
                                }
                                finally
                                {
                                    TransactionManager.EntityBeingReparented = null;
                                }

                                // stop trying to remove something, if the owner was detached or deleted because of RIC/cascade delete
                                if (entry.State == EntityState.Detached
                                    || entry.State == EntityState.Deleted
                                    || entry.IsKeyEntry)
                                {
                                    break;
                                }
                            }
                        }

                        // skip the remaining relatedEnds if the owner was detached or deleted because of RIC/cascade delete
                        if (entry.State == EntityState.Detached
                            || entry.State == EntityState.Deleted
                            || entry.IsKeyEntry)
                        {
                            break;
                        }
                    }
                }
            }
        }

        private bool ShouldPreserveForeignKeyForPrincipal(
            IEntityWrapper entity, RelatedEnd relatedEnd, IEntityWrapper relatedEntity,
            HashSet<IEntityWrapper> entitiesToDelete)
        {
            var preserveForeignKey = false;
            if (relatedEnd.IsForeignKey)
            {
                var otherEnd = relatedEnd.GetOtherEndOfRelationship(relatedEntity);
                if (otherEnd.IsDependentEndOfReferentialConstraint(false))
                {
                    // Check the changes being applied to the dependent end
                    HashSet<EntityKey> entityKeysOfDeletedObjects = null;
                    Dictionary<RelatedEnd, HashSet<EntityKey>> deletedRelationshipsByForeignKey;
                    Dictionary<RelatedEnd, HashSet<IEntityWrapper>> deletedRelationshipsByGraph;
                    // There must be a foreign key and graph change on the dependent side to know if we need to preserve the FK
                    if (TransactionManager.DeletedRelationshipsByForeignKey.TryGetValue(relatedEntity, out deletedRelationshipsByForeignKey)
                        &&
                        deletedRelationshipsByForeignKey.TryGetValue(otherEnd, out entityKeysOfDeletedObjects)
                        &&
                        entityKeysOfDeletedObjects.Count > 0
                        &&
                        TransactionManager.DeletedRelationshipsByGraph.TryGetValue(relatedEntity, out deletedRelationshipsByGraph)
                        &&
                        deletedRelationshipsByGraph.TryGetValue(otherEnd, out entitiesToDelete))
                    {
                        preserveForeignKey = ShouldPreserveForeignKeyForDependent(relatedEntity, otherEnd, entity, entitiesToDelete);
                    }
                }
            }
            return preserveForeignKey;
        }

        private bool ShouldPreserveForeignKeyForDependent(
            IEntityWrapper entity, RelatedEnd relatedEnd, IEntityWrapper relatedEntity,
            HashSet<IEntityWrapper> entitiesToDelete)
        {
            var hasReferenceRemove = entitiesToDelete.Contains(relatedEntity);
            return (!hasReferenceRemove ||
                    hasReferenceRemove && !HasAddedReference(entity, relatedEnd as EntityReference));
        }

        private bool HasAddedReference(IEntityWrapper wrappedOwner, EntityReference reference)
        {
            Dictionary<RelatedEnd, HashSet<IEntityWrapper>> addedRelationshipsByGraph;
            HashSet<IEntityWrapper> entitiesToAdd = null;
            if (reference != null
                &&
                TransactionManager.AddedRelationshipsByGraph.TryGetValue(wrappedOwner, out addedRelationshipsByGraph)
                &&
                addedRelationshipsByGraph.TryGetValue(reference, out entitiesToAdd)
                &&
                entitiesToAdd.Count > 0)
            {
                return true;
            }
            return false;
        }

        private bool IsReparentingReference(IEntityWrapper wrappedEntity, EntityReference reference)
        {
            var tm = TransactionManager;
            if (reference.IsPrincipalEndOfReferentialConstraint())
            {
                // need to find the dependent and make sure that it is being reparented
                wrappedEntity = reference.ReferenceValue;
                reference = wrappedEntity.Entity == null
                                ? null
                                : reference.GetOtherEndOfRelationship(wrappedEntity) as EntityReference;
            }

            if (wrappedEntity.Entity != null
                && reference != null)
            {
                HashSet<EntityKey> entityKeysOfAddedObjects = null;
                Dictionary<RelatedEnd, HashSet<EntityKey>> addedRelationshipsByForeignKey;
                if (tm.AddedRelationshipsByForeignKey.TryGetValue(wrappedEntity, out addedRelationshipsByForeignKey)
                    &&
                    addedRelationshipsByForeignKey.TryGetValue(reference, out entityKeysOfAddedObjects)
                    &&
                    entityKeysOfAddedObjects.Count > 0)
                {
                    return true;
                }

                Dictionary<RelatedEnd, HashSet<IEntityWrapper>> addedRelationshipsByGraph;
                HashSet<IEntityWrapper> entitiesToAdd = null;
                if (tm.AddedRelationshipsByGraph.TryGetValue(wrappedEntity, out addedRelationshipsByGraph)
                    &&
                    addedRelationshipsByGraph.TryGetValue(reference, out entitiesToAdd)
                    &&
                    entitiesToAdd.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static void DetectChangesInNavigationProperties(IList<EntityEntry> entries)
        {
            // Detect changes in navigation properties
            // (populates this.TransactionManager.DeletedRelationships and this.TransactionManager.AddedRelationships)
            foreach (var entry in entries)
            {
                Debug.Assert(!entry.IsKeyEntry, "List should be filtered before it gets to this method.");
                if (entry.WrappedEntity.RequiresRelationshipChangeTracking)
                {
                    entry.DetectChangesInRelationshipsOfSingleEntity();
                }
            }
        }

        private static void DetectChangesInScalarAndComplexProperties(IList<EntityEntry> entries)
        {
            foreach (var entry in entries)
            {
                Debug.Assert(!entry.IsKeyEntry, "List should be filtered before it gets to this method.");

                if (entry.State
                    != EntityState.Added)
                {
                    if (entry.RequiresScalarChangeTracking
                        || entry.RequiresComplexChangeTracking)
                    {
                        entry.DetectChangesInProperties(!entry.RequiresScalarChangeTracking);
                    }
                }
            }
        }

        internal virtual EntityKey CreateEntityKey(EntitySet entitySet, object entity)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(entity);

            // Creates an EntityKey based on the values in the entity and the given EntitySet
            var keyMembers = entitySet.ElementType.KeyMembers;
            var typeMetadata = GetOrAddStateManagerTypeMetadata(EntityUtil.GetEntityIdentityType(entity.GetType()), entitySet);
            var keyValues = new object[keyMembers.Count];

            for (var i = 0; i < keyMembers.Count; ++i)
            {
                var keyName = keyMembers[i].Name;
                var ordinal = typeMetadata.GetOrdinalforCLayerMemberName(keyName);
                if (ordinal < 0)
                {
                    throw new ArgumentException(
                        Strings.ObjectStateManager_EntityTypeDoesnotMatchtoEntitySetType(entity.GetType().FullName, entitySet.Name),
                        "entity");
                }

                keyValues[i] = typeMetadata.Member(ordinal).GetValue(entity);
                if (keyValues[i] == null)
                {
                    throw new InvalidOperationException(Strings.EntityKey_NullKeyValue(keyName, entitySet.ElementType.Name));
                }
            }

            if (keyValues.Length == 1)
            {
                return new EntityKey(entitySet, keyValues[0]);
            }
            else
            {
                return new EntityKey(entitySet, keyValues);
            }
        }

        // <summary>
        // Flag that is set when we are processing an FK setter for a full proxy.
        // This is used to determine whether or not we will attempt to call out into FK
        // setters and null references during fixup.
        // The value of this property is either null if the code is not executing an
        // FK setter, or points to the entity on which the FK setter has been called.
        // </summary>
        internal virtual object EntityInvokingFKSetter { get; set; }
    }
}
