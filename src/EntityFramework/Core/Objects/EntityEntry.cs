// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal sealed class EntityEntry : ObjectStateEntry
    {
        private StateManagerTypeMetadata _cacheTypeMetadata;
        private EntityKey _entityKey; // !null if IsKeyEntry or Entity
        private IEntityWrapper _wrappedEntity; // Contains null entity if IsKeyEntry

        // entity entry change tracking
        private BitArray _modifiedFields; // only and always exists if state is Modified or after Delete() on Modified
        private List<StateManagerValue> _originalValues; // only exists if _modifiedFields has a true-bit

        // The _originalComplexObjects should always contain references to the values of complex objects which are "original" 
        // at the moment of calling GetComplexObjectSnapshot().  They are used to get original scalar values from _originalValues
        // and to check if complex object instance was changed.
        private Dictionary<object, Dictionary<int, object>> _originalComplexObjects; // used for POCO Complex Objects change tracking

        private bool _requiresComplexChangeTracking;
        private bool _requiresScalarChangeTracking;
        private bool _requiresAnyChangeTracking;

        #region RelationshipEnd fields

        // <summary>
        // Singlely-linked list of RelationshipEntry.
        // One of the ends in the RelationshipEntry must equal this.EntityKey
        // </summary>
        private RelationshipEntry _headRelationshipEnds;

        // <summary>
        // Number of RelationshipEntry in the _relationshipEnds list.
        // </summary>
        private int _countRelationshipEnds;

        #endregion

        #region Constructors

        // <summary>
        // For testing purposes only.
        // </summary>
        internal EntityEntry()
            : base(new ObjectStateManager(), null, EntityState.Unchanged)
        {
        }

        // <summary>
        // For testing purposes only.
        // </summary>
        internal EntityEntry(ObjectStateManager stateManager)
            : base(stateManager, null, EntityState.Unchanged)
        {
        }

        internal EntityEntry(
            IEntityWrapper wrappedEntity, EntityKey entityKey, EntitySet entitySet, ObjectStateManager cache,
            StateManagerTypeMetadata typeMetadata, EntityState state)
            : base(cache, entitySet, state)
        {
            DebugCheck.NotNull(wrappedEntity);
            DebugCheck.NotNull(wrappedEntity.Entity);
            DebugCheck.NotNull(typeMetadata);
            DebugCheck.NotNull(entitySet);
            Debug.Assert((entityKey == null) || (entityKey.EntitySetName == entitySet.Name), "different entitySet");

            _wrappedEntity = wrappedEntity;
            _cacheTypeMetadata = typeMetadata;
            _entityKey = entityKey;

            wrappedEntity.ObjectStateEntry = this;

            SetChangeTrackingFlags();
        }

        // <summary>
        // Looks at the type of entity represented by this entry and sets flags defining the type of
        // change tracking that will be needed. The three main types are:
        // - Pure POCO objects or non-change-tracking proxies which need DetectChanges for everything.
        // - Entities derived from EntityObject which don't need DetectChanges at all.
        // - Change tracking proxies, which only need DetectChanges for complex properties.
        // </summary>
        private void SetChangeTrackingFlags()
        {
            _requiresScalarChangeTracking = Entity != null && !(Entity is IEntityWithChangeTracker);

            _requiresComplexChangeTracking = Entity != null &&
                                             (_requiresScalarChangeTracking ||
                                              (WrappedEntity.IdentityType != Entity.GetType() &&
                                               _cacheTypeMetadata.Members.Any(m => m.IsComplex)));

            _requiresAnyChangeTracking = Entity != null &&
                                         (!(Entity is IEntityWithRelationships) ||
                                          _requiresComplexChangeTracking ||
                                          _requiresScalarChangeTracking);
        }

        internal EntityEntry(EntityKey entityKey, EntitySet entitySet, ObjectStateManager cache, StateManagerTypeMetadata typeMetadata)
            : base(cache, entitySet, EntityState.Unchanged)
        {
            DebugCheck.NotNull(entityKey);
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(typeMetadata);
            Debug.Assert(entityKey.EntitySetName == entitySet.Name, "different entitySet");

            _wrappedEntity = NullEntityWrapper.NullWrapper;
            _entityKey = entityKey;
            _cacheTypeMetadata = typeMetadata;

            SetChangeTrackingFlags();
        }

        #endregion

        #region Public members

        public override bool IsRelationship
        {
            get
            {
                ValidateState();
                return false;
            }
        }

        public override object Entity
        {
            get
            {
                ValidateState();
                return _wrappedEntity.Entity;
            }
        }

        // <summary>
        // The EntityKey associated with the ObjectStateEntry
        // </summary>
        public override EntityKey EntityKey
        {
            get
            {
                ValidateState();
                return _entityKey;
            }
            internal set { _entityKey = value; }
        }

        internal IEnumerable<Tuple<AssociationSet, ReferentialConstraint>> ForeignKeyDependents
        {
            get
            {
                foreach (var foreignKey in ((EntitySet)EntitySet).ForeignKeyDependents)
                {
                    var constraint = foreignKey.Item2;
                    var dependentType = MetadataHelper.GetEntityTypeForEnd((AssociationEndMember)constraint.ToRole);
                    if (dependentType.IsAssignableFrom(_cacheTypeMetadata.DataRecordInfo.RecordType.EdmType))
                    {
                        yield return foreignKey;
                    }
                }
            }
        }

        internal IEnumerable<Tuple<AssociationSet, ReferentialConstraint>> ForeignKeyPrincipals
        {
            get
            {
                foreach (var foreignKey in ((EntitySet)EntitySet).ForeignKeyPrincipals)
                {
                    var constraint = foreignKey.Item2;
                    var dependentType = MetadataHelper.GetEntityTypeForEnd((AssociationEndMember)constraint.FromRole);
                    if (dependentType.IsAssignableFrom(_cacheTypeMetadata.DataRecordInfo.RecordType.EdmType))
                    {
                        yield return foreignKey;
                    }
                }
            }
        }

        public override IEnumerable<string> GetModifiedProperties()
        {
            ValidateState();
            if (EntityState.Modified == State
                && _modifiedFields != null)
            {
                Debug.Assert(null != _modifiedFields, "null fields");
                for (var i = 0; i < _modifiedFields.Length; i++)
                {
                    if (_modifiedFields[i])
                    {
                        yield return (GetCLayerName(i, _cacheTypeMetadata));
                    }
                }
            }
        }

        // <summary>
        // Marks specified property as modified.
        // </summary>
        // <param name="propertyName"> This API recognizes the names in terms of OSpace </param>
        // <exception cref="InvalidOperationException">If State is not Modified or Unchanged</exception>
        public override void SetModifiedProperty(string propertyName)
        {
            // We need this because the Code Contract gets compiled out in the release build even though
            // this method is effectively on the public surface because it overrides the abstract method on ObjectStateEntry.
            // Using a CodeContractsFor class doesn't work in this case.
            Check.NotEmpty(propertyName, "propertyName");

            var ordinal = ValidateAndGetOrdinalForProperty(propertyName, "SetModifiedProperty");

            Debug.Assert(
                State == EntityState.Unchanged || State == EntityState.Modified, "ValidateAndGetOrdinalForProperty should have thrown.");

            if (EntityState.Unchanged == State)
            {
                State = EntityState.Modified;
                _cache.ChangeState(this, EntityState.Unchanged, State);
            }

            SetModifiedPropertyInternal(ordinal);
        }

        internal void SetModifiedPropertyInternal(int ordinal)
        {
            if (null == _modifiedFields)
            {
                _modifiedFields = new BitArray(GetFieldCount(_cacheTypeMetadata));
            }

            _modifiedFields[ordinal] = true;
        }

        private int ValidateAndGetOrdinalForProperty(string propertyName, string methodName)
        {
            DebugCheck.NotNull(propertyName);

            // Throw for detached entities
            ValidateState();

            if (IsKeyEntry)
            {
                throw new InvalidOperationException(Strings.ObjectStateEntry_CannotModifyKeyEntryState);
            }

            var ordinal = _cacheTypeMetadata.GetOrdinalforOLayerMemberName(propertyName);
            if (ordinal == -1)
            {
                throw new ArgumentException(Strings.ObjectStateEntry_SetModifiedOnInvalidProperty(propertyName));
            }

            if (State == EntityState.Added
                || State == EntityState.Deleted)
            {
                // Threw for detached above; this throws for Added or Deleted entities
                throw new InvalidOperationException(Strings.ObjectStateEntry_SetModifiedStates(methodName));
            }

            return ordinal;
        }

        // <summary>
        // Rejects any changes made to the property with the given name since the property was last loaded,
        // attached, saved, or changes were accepted. The orginal value of the property is stored and the
        // property will no longer be marked as modified.
        // </summary>
        // <remarks>
        // If the result is that no properties of the entity are marked as modified, then the entity will
        // be marked as Unchanged.
        // Changes to properties can only rejected for entities that are in the Modified or Unchanged state.
        // Calling this method for entities in other states (Added, Deleted, or Detached) will result in
        // an exception being thrown.
        // Rejecting changes to properties of an Unchanged entity or unchanged properties of a Modifed
        // is a no-op.
        // </remarks>
        // <param name="propertyName"> The name of the property to change. </param>
        public override void RejectPropertyChanges(string propertyName)
        {
            // We need this because the Code Contract gets compiled out in the release build even though
            // this method is effectively on the public surface because it overrides the abstract method on ObjectStateEntry.
            // Using a CodeContractsFor class doesn't work in this case.
            Check.NotEmpty(propertyName, "propertyName");

            var ordinal = ValidateAndGetOrdinalForProperty(propertyName, "RejectPropertyChanges");

            if (State == EntityState.Unchanged)
            {
                // No-op for unchanged entities since all properties must be unchanged.
                return;
            }

            Debug.Assert(State == EntityState.Modified, "Should have handled all other states above.");

            if (_modifiedFields != null
                && _modifiedFields[ordinal])
            {
                // Reject the change by setting the current value to the original value
                DetectChangesInComplexProperties();
                var originalValue = GetOriginalEntityValue(
                    _cacheTypeMetadata, ordinal, _wrappedEntity.Entity, ObjectStateValueRecord.OriginalReadonly);
                SetCurrentEntityValue(_cacheTypeMetadata, ordinal, _wrappedEntity.Entity, originalValue);
                _modifiedFields[ordinal] = false;

                // Check if any properties remain modified. If any are modified, then we leave the entity state as Modified and we are done.
                for (var i = 0; i < _modifiedFields.Length; i++)
                {
                    if (_modifiedFields[i])
                    {
                        return;
                    }
                }

                // No properties are modified so change the state of the entity to Unchanged.
                ChangeObjectState(EntityState.Unchanged);
            }
        }

        // <summary>
        // Original values
        // </summary>
        // <returns> DbDataRecord </returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public override DbDataRecord OriginalValues
        {
            get { return InternalGetOriginalValues(true /*readOnly*/); }
        }

        // <summary>
        // Gets a version of the OriginalValues property that can be updated
        // </summary>
        public override OriginalValueRecord GetUpdatableOriginalValues()
        {
            return (OriginalValueRecord)InternalGetOriginalValues(false /*readOnly*/);
        }

        private DbDataRecord InternalGetOriginalValues(bool readOnly)
        {
            ValidateState();
            if (State == EntityState.Added)
            {
                throw new InvalidOperationException(Strings.ObjectStateEntry_OriginalValuesDoesNotExist);
            }

            if (IsKeyEntry)
            {
                throw new InvalidOperationException(Strings.ObjectStateEntry_CannotAccessKeyEntryValues);
            }
            else
            {
                DetectChangesInComplexProperties();

                if (readOnly)
                {
                    return new ObjectStateEntryDbDataRecord(this, _cacheTypeMetadata, _wrappedEntity.Entity);
                }
                else
                {
                    return new ObjectStateEntryOriginalDbUpdatableDataRecord_Public(
                        this, _cacheTypeMetadata, _wrappedEntity.Entity, s_EntityRoot);
                }
            }
        }

        private void DetectChangesInComplexProperties()
        {
            if (RequiresScalarChangeTracking)
            {
                // POCO: the snapshot of complex objects has to be updated 
                // without chaning state of the entry or marking properties as modified.
                // The IsOriginalValuesGetter is used in EntityMemberChanged to skip the state transition.
                // The snapshot has to be updated in case the complex object instance was changed (not only scalar values).
                ObjectStateManager.TransactionManager.BeginOriginalValuesGetter();
                try
                {
                    // Process only complex objects. The method will not change the state of the entry.
                    DetectChangesInProperties(true /*detectOnlyComplexProperties*/);
                }
                finally
                {
                    ObjectStateManager.TransactionManager.EndOriginalValuesGetter();
                }
            }
        }

        // <summary>
        // Current values
        // </summary>
        // <returns> DbUpdatableDataRecord </returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public override CurrentValueRecord CurrentValues
        {
            get
            {
                ValidateState();
                if (State == EntityState.Deleted)
                {
                    throw new InvalidOperationException(Strings.ObjectStateEntry_CurrentValuesDoesNotExist);
                }

                if (IsKeyEntry)
                {
                    throw new InvalidOperationException(Strings.ObjectStateEntry_CannotAccessKeyEntryValues);
                }
                else
                {
                    return new ObjectStateEntryDbUpdatableDataRecord(this, _cacheTypeMetadata, _wrappedEntity.Entity);
                }
            }
        }

        public override void Delete()
        {
            // doFixup flag is used for Cache and Collection & Ref consistency
            // When some entity is deleted if "doFixup" is true then Delete method
            // calls the Collection & Ref code to do the necessary fix-ups.
            // "doFixup" equals to False is only called from EntityCollection & Ref code
            Delete( /*doFixup*/true);
        }

        // <summary>
        // API to accept the current values as original values and  mark the entity as Unchanged.
        // </summary>
        public override void AcceptChanges()
        {
            ValidateState();

            if (ObjectStateManager.EntryHasConceptualNull(this))
            {
                throw new InvalidOperationException(Strings.ObjectContext_CommitWithConceptualNull);
            }

            Debug.Assert(!IsKeyEntry || State == EntityState.Unchanged, "Key ObjectStateEntries must always be unchanged.");

            switch (State)
            {
                case EntityState.Deleted:
                    CascadeAcceptChanges();
                    // Current entry could be already detached if this is relationship entry and if one end of relationship was a KeyEntry
                    if (_cache != null)
                    {
                        _cache.ChangeState(this, EntityState.Deleted, EntityState.Detached);
                    }
                    break;
                case EntityState.Added:
                    // If this entry represents an entity, perform key fixup.
                    Debug.Assert(Entity != null, "Non-relationship entries should have a non-null entity.");
                    Debug.Assert((object)_entityKey != null, "All entities in the state manager should have a non-null EntityKey.");
                    Debug.Assert(_entityKey.IsTemporary, "All entities in the Added state should have a temporary EntityKey.");

                    // Retrieve referential constraint properties from Principal entities (possibly recursively)
                    // and check referential constraint properties in the Dependent entities (1 level only)
                    // We have to do this before fixing up keys to preserve v1 behavior around when stubs are promoted.
                    // However, we can't check FKs until after fixup, which happens after key fixup.  Therefore,
                    // we keep track of whether or not we need to go check again after fixup.  Also, checking for independent associations
                    // happens using RelationshipEntries, while checking for constraints in FKs has to use the graph.
                    var skippedFKs = RetrieveAndCheckReferentialConstraintValuesInAcceptChanges();

                    _cache.FixupKey(this);

                    _modifiedFields = null;
                    _originalValues = null;
                    _originalComplexObjects = null;
                    State = EntityState.Unchanged;

                    if (skippedFKs)
                    {
                        // If we skipped checking constraints on any FK relationships above, then
                        // do it now on the fixuped RelatedEnds.
                        RelationshipManager.CheckReferentialConstraintProperties(this);
                    }

                    _wrappedEntity.TakeSnapshot(this);

                    break;
                case EntityState.Modified:
                    _cache.ChangeState(this, EntityState.Modified, EntityState.Unchanged);
                    _modifiedFields = null;
                    _originalValues = null;
                    _originalComplexObjects = null;
                    State = EntityState.Unchanged;
                    _cache.FixupReferencesByForeignKeys(this);

                    // Need to check constraints here because fixup could have got us into an invalid state
                    RelationshipManager.CheckReferentialConstraintProperties(this);
                    _wrappedEntity.TakeSnapshot(this);

                    break;
                case EntityState.Unchanged:
                    break;
            }
        }

        public override void SetModified()
        {
            ValidateState();

            if (IsKeyEntry)
            {
                throw new InvalidOperationException(Strings.ObjectStateEntry_CannotModifyKeyEntryState);
            }
            else
            {
                if (EntityState.Unchanged == State)
                {
                    State = EntityState.Modified;
                    _cache.ChangeState(this, EntityState.Unchanged, State);
                }
                else if (EntityState.Modified != State)
                {
                    throw new InvalidOperationException(Strings.ObjectStateEntry_SetModifiedStates("SetModified"));
                }
            }
        }

        public override RelationshipManager RelationshipManager
        {
            get
            {
                ValidateState();
                if (IsKeyEntry)
                {
                    throw new InvalidOperationException(Strings.ObjectStateEntry_RelationshipAndKeyEntriesDoNotHaveRelationshipManagers);
                }
                if (WrappedEntity.Entity == null)
                {
                    throw new InvalidOperationException(Strings.ObjectStateManager_CannotGetRelationshipManagerForDetachedPocoEntity);
                }
                return WrappedEntity.RelationshipManager;
            }
        }

        internal override BitArray ModifiedProperties
        {
            get { return _modifiedFields; }
        }

        // <summary>
        // Changes state of the entry to the specified <paramref name="state" />
        // </summary>
        // <param name="state"> The requested state </param>
        public override void ChangeState(EntityState state)
        {
            EntityUtil.CheckValidStateForChangeEntityState(state);

            if (State == EntityState.Detached
                && state == EntityState.Detached)
            {
                return;
            }

            ValidateState();

            // store a referece to the cache because this.ObjectStatemanager will be null if the requested state is Detached
            var osm = ObjectStateManager;
            osm.TransactionManager.BeginLocalPublicAPI();
            try
            {
                ChangeObjectState(state);
            }
            finally
            {
                osm.TransactionManager.EndLocalPublicAPI();
            }
        }

        // <summary>
        // Apply modified properties to the original object.
        // </summary>
        // <param name="currentEntity"> object with modified properties </param>
        public override void ApplyCurrentValues(object currentEntity)
        {
            Check.NotNull(currentEntity, "currentEntity");

            ValidateState();

            if (IsKeyEntry)
            {
                throw new InvalidOperationException(Strings.ObjectStateEntry_CannotAccessKeyEntryValues);
            }

            var wrappedEntity = ObjectStateManager.EntityWrapperFactory.WrapEntityUsingStateManager(currentEntity, ObjectStateManager);

            ApplyCurrentValuesInternal(wrappedEntity);
        }

        // <summary>
        // Apply original values to the entity.
        // </summary>
        // <param name="originalEntity"> The object with original values </param>
        public override void ApplyOriginalValues(object originalEntity)
        {
            Check.NotNull(originalEntity, "originalEntity");

            ValidateState();

            if (IsKeyEntry)
            {
                throw new InvalidOperationException(Strings.ObjectStateEntry_CannotAccessKeyEntryValues);
            }

            var wrappedEntity = ObjectStateManager.EntityWrapperFactory.WrapEntityUsingStateManager(originalEntity, ObjectStateManager);

            ApplyOriginalValuesInternal(wrappedEntity);
        }

        #endregion // Public members

        #region RelationshipEnd methods

        // <summary>
        // Add a RelationshipEntry (one of its ends must equal this.EntityKey)
        // </summary>
        internal void AddRelationshipEnd(RelationshipEntry item)
        {
            DebugCheck.NotNull(item);
            DebugCheck.NotNull(item.RelationshipWrapper);
            Debug.Assert(0 <= _countRelationshipEnds, "negative _relationshipEndCount");
            Debug.Assert(
                EntityKey.Equals(item.RelationshipWrapper.Key0) || EntityKey.Equals(item.RelationshipWrapper.Key1),
                "entity key doesn't match");

#if DEBUG
            for (var current = _headRelationshipEnds;
                 null != current;
                 current = current.GetNextRelationshipEnd(EntityKey))
            {
                Debug.Assert(!ReferenceEquals(item, current), "RelationshipEntry already in list");
                Debug.Assert(!item.RelationshipWrapper.Equals(current.RelationshipWrapper), "RelationshipWrapper already in list");
            }
#endif
            // the item will become the head of the list
            // i.e. you walk the list in reverse order of items being added
            item.SetNextRelationshipEnd(EntityKey, _headRelationshipEnds);
            _headRelationshipEnds = item;
            _countRelationshipEnds++;

            Debug.Assert(_countRelationshipEnds == (new RelationshipEndEnumerable(this)).ToArray().Length, "different count");
        }

        // <summary>
        // Determines if a given relationship entry is present in the list of entries
        // </summary>
        // <param name="item"> The entry to look for </param>
        // <returns> True of the relationship end is found </returns>
        internal bool ContainsRelationshipEnd(RelationshipEntry item)
        {
            for (var current = _headRelationshipEnds;
                 null != current;
                 current = current.GetNextRelationshipEnd(EntityKey))
            {
                if (ReferenceEquals(current, item))
                {
                    return true;
                }
            }
            return false;
        }

        // <summary>
        // Remove a RelationshipEntry (one of its ends must equal this.EntityKey)
        // </summary>
        internal void RemoveRelationshipEnd(RelationshipEntry item)
        {
            DebugCheck.NotNull(item);
            DebugCheck.NotNull(item.RelationshipWrapper);
            Debug.Assert(1 <= _countRelationshipEnds, "negative _relationshipEndCount");
            Debug.Assert(
                EntityKey.Equals(item.RelationshipWrapper.Key0) || EntityKey.Equals(item.RelationshipWrapper.Key1),
                "entity key doesn't match");

            // walk the singly-linked list, remembering the previous node so we can remove the current node
            var current = _headRelationshipEnds;
            RelationshipEntry previous = null;
            var previousIsKey0 = false;
            while (null != current)
            {
                // short-circuit if the key matches either candidate by reference
                var currentIsKey0 = ReferenceEquals(EntityKey, current.Key0) ||
                                    (!ReferenceEquals(EntityKey, current.Key1) && EntityKey.Equals(current.Key0));
                if (ReferenceEquals(item, current))
                {
                    RelationshipEntry next;
                    if (currentIsKey0)
                    {
                        // if this.EntityKey matches Key0, NextKey0 is the next element in the lsit
                        Debug.Assert(EntityKey.Equals(current.RelationshipWrapper.Key0), "entity key didn't match");
                        next = current.NextKey0;
                        current.NextKey0 = null;
                    }
                    else
                    {
                        // if this.EntityKey matches Key1, NextKey1 is the next element in the lsit
                        Debug.Assert(EntityKey.Equals(current.RelationshipWrapper.Key1), "entity key didn't match");
                        next = current.NextKey1;
                        current.NextKey1 = null;
                    }
                    if (null == previous)
                    {
                        _headRelationshipEnds = next;
                    }
                    else if (previousIsKey0)
                    {
                        previous.NextKey0 = next;
                    }
                    else
                    {
                        previous.NextKey1 = next;
                    }
                    --_countRelationshipEnds;

                    Debug.Assert(_countRelationshipEnds == (new RelationshipEndEnumerable(this)).ToArray().Length, "different count");
                    return;
                }
                Debug.Assert(
                    !item.RelationshipWrapper.Equals(current.RelationshipWrapper), "same wrapper, different RelationshipEntry instances");

                previous = current;
                current = currentIsKey0 ? current.NextKey0 : current.NextKey1;
                previousIsKey0 = currentIsKey0;
            }
            Debug.Assert(false, "didn't remove a RelationshipEntry");
        }

        // <summary>
        // Update one of the ends for the related RelationshipEntry
        // </summary>
        // <param name="oldKey"> the EntityKey the relationship should currently have </param>
        // <param name="promotedEntry"> if promoting entity stub to full entity </param>
        internal void UpdateRelationshipEnds(EntityKey oldKey, EntityEntry promotedEntry)
        {
            DebugCheck.NotNull(oldKey);
            Debug.Assert(!ReferenceEquals(this, promotedEntry), "shouldn't be same reference");

            // traverse the list to update one of the ends in the relationship entry
            var count = 0;
            var next = _headRelationshipEnds;
            while (null != next)
            {
                // get the next relationship end before we change the key of current relationship end
                var current = next;
                next = next.GetNextRelationshipEnd(oldKey);

                // update the RelationshipEntry from the temporary key to real key
                current.ChangeRelatedEnd(oldKey, EntityKey);

                // If we have a promoted entry, copy the relationship entries to the promoted entry
                // only if the promoted entry doesn't already know about that particular relationship entry
                // This can be the case with self referencing entities
                if (null != promotedEntry
                    && !promotedEntry.ContainsRelationshipEnd(current))
                {
                    // all relationship ends moved to new promotedEntry
                    promotedEntry.AddRelationshipEnd(current);
                }
                ++count;
            }
            Debug.Assert(count == _countRelationshipEnds, "didn't traverse all relationships");
            if (null != promotedEntry)
            {
                // cleanup existing (dead) entry to reduce confusion
                _headRelationshipEnds = null;
                _countRelationshipEnds = 0;
            }
        }

        #region Enumerable and Enumerator

        internal RelationshipEndEnumerable GetRelationshipEnds()
        {
            return new RelationshipEndEnumerable(this);
        }

        // <summary>
        // An enumerable so that EntityEntry doesn't implement it
        // </summary>
        internal struct RelationshipEndEnumerable : IEnumerable<RelationshipEntry>, IEnumerable<IEntityStateEntry>
        {
            internal static readonly RelationshipEntry[] EmptyRelationshipEntryArray = new RelationshipEntry[0];
            private readonly EntityEntry _entityEntry;

            internal RelationshipEndEnumerable(EntityEntry entityEntry)
            {
                // its okay if entityEntry is null
                _entityEntry = entityEntry;
            }

            public RelationshipEndEnumerator GetEnumerator()
            {
                return new RelationshipEndEnumerator(_entityEntry);
            }

            IEnumerator<IEntityStateEntry> IEnumerable<IEntityStateEntry>.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator<RelationshipEntry> IEnumerable<RelationshipEntry>.GetEnumerator()
            {
                Debug.Assert(false, "dead code, don't box the RelationshipEndEnumerable");
                return GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                Debug.Assert(false, "dead code, don't box the RelationshipEndEnumerable");
                return GetEnumerator();
            }

            // <summary>
            // Convert the singly-linked list into an Array
            // </summary>
            internal RelationshipEntry[] ToArray()
            {
                RelationshipEntry[] list = null;
                if ((null != _entityEntry)
                    && (0 < _entityEntry._countRelationshipEnds))
                {
                    var relationshipEnd = _entityEntry._headRelationshipEnds;
                    list = new RelationshipEntry[_entityEntry._countRelationshipEnds];
                    for (var i = 0; i < list.Length; ++i)
                    {
                        Debug.Assert(null != relationshipEnd, "count larger than list");
                        Debug.Assert(
                            _entityEntry.EntityKey.Equals(relationshipEnd.Key0) || _entityEntry.EntityKey.Equals(relationshipEnd.Key1),
                            "entity key mismatch");
                        list[i] = relationshipEnd;

                        relationshipEnd = relationshipEnd.GetNextRelationshipEnd(_entityEntry.EntityKey);
                    }
                    Debug.Assert(null == relationshipEnd, "count smaller than list");
                }
                return list ?? EmptyRelationshipEntryArray;
            }
        }

        // <summary>
        // An enumerator to walk the RelationshipEntry linked-list
        // </summary>
        internal struct RelationshipEndEnumerator : IEnumerator<RelationshipEntry>, IEnumerator<IEntityStateEntry>
        {
            private readonly EntityEntry _entityEntry;
            private RelationshipEntry _current;

            internal RelationshipEndEnumerator(EntityEntry entityEntry)
            {
                _entityEntry = entityEntry;
                _current = null;
            }

            public RelationshipEntry Current
            {
                get { return _current; }
            }

            IEntityStateEntry IEnumerator<IEntityStateEntry>.Current
            {
                get { return _current; }
            }

            object IEnumerator.Current
            {
                get
                {
                    Debug.Assert(false, "dead code, don't box the RelationshipEndEnumerator");
                    return _current;
                }
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (null != _entityEntry)
                {
                    if (null == _current)
                    {
                        _current = _entityEntry._headRelationshipEnds;
                    }
                    else
                    {
                        _current = _current.GetNextRelationshipEnd(_entityEntry.EntityKey);
                    }
                }
                return (null != _current);
            }

            public void Reset()
            {
                Debug.Assert(false, "not implemented");
            }
        }

        #endregion

        #endregion

        #region ObjectStateEntry members

        internal override bool IsKeyEntry
        {
            get { return null == _wrappedEntity.Entity; }
        }

        // <summary>
        // Reuse or create a new (Entity)DataRecordInfo.
        // </summary>
        internal override DataRecordInfo GetDataRecordInfo(StateManagerTypeMetadata metadata, object userObject)
        {
            if (Helper.IsEntityType(metadata.CdmMetadata.EdmType)
                && (null != (object)_entityKey))
            {
                // is EntityType with null EntityKey when constructing new EntityKey during ObjectStateManager.Add
                // always need a new EntityRecordInfo instance for the different key (reusing DataRecordInfo's FieldMetadata).
                return new EntityRecordInfo(metadata.DataRecordInfo, _entityKey, (EntitySet)EntitySet);
            }
            else
            {
                // ObjectContext.AttachTo uses CurrentValueRecord to build EntityKey for EntityType
                // so the Entity doesn't have an EntityKey yet
                return metadata.DataRecordInfo;
            }
        }

        internal override void Reset()
        {
            Debug.Assert(_cache != null, "Cannot Reset an entity that is not currently attached to a context.");
            RemoveFromForeignKeyIndex();
            _cache.ForgetEntryWithConceptualNull(this, resetAllKeys: true);

            DetachObjectStateManagerFromEntity();

            _wrappedEntity = NullEntityWrapper.NullWrapper;
            _entityKey = null;
            _modifiedFields = null;
            _originalValues = null;
            _originalComplexObjects = null;

            SetChangeTrackingFlags();

            base.Reset();
        }

        internal override Type GetFieldType(int ordinal, StateManagerTypeMetadata metadata)
        {
            // 'metadata' is used for ComplexTypes

            return metadata.GetFieldType(ordinal);
        }

        internal override string GetCLayerName(int ordinal, StateManagerTypeMetadata metadata)
        {
            return metadata.CLayerMemberName(ordinal);
        }

        internal override int GetOrdinalforCLayerName(string name, StateManagerTypeMetadata metadata)
        {
            return metadata.GetOrdinalforCLayerMemberName(name);
        }

        internal override void RevertDelete()
        {
            // just change the state from deleted, to last state.
            State = (_modifiedFields == null) ? EntityState.Unchanged : EntityState.Modified;
            _cache.ChangeState(this, EntityState.Deleted, State);
        }

        internal override int GetFieldCount(StateManagerTypeMetadata metadata)
        {
            return metadata.FieldCount;
        }

        private void CascadeAcceptChanges()
        {
            foreach (var entry in _cache.CopyOfRelationshipsByKey(EntityKey))
            {
                // CascadeAcceptChanges is only called on Entity ObjectStateEntry when it is
                // in deleted state. Entity is in deleted state therefore for all related Relationship
                // cache entries only valid state is Deleted.
                Debug.Assert(entry.State == EntityState.Deleted, "Relationship ObjectStateEntry should be in deleted state");
                entry.AcceptChanges();
            }
        }

        internal override void SetModifiedAll()
        {
            Debug.Assert(!IsKeyEntry, "SetModifiedAll called on a KeyEntry");
            Debug.Assert(State == EntityState.Modified, "SetModifiedAll called when not modified");

            ValidateState();
            if (null == _modifiedFields)
            {
                _modifiedFields = new BitArray(GetFieldCount(_cacheTypeMetadata));
            }
            _modifiedFields.SetAll(true);
        }

        // <summary>
        // Used to report that a scalar entity property is about to change
        // The current value of the specified property is cached when this method is called.
        // </summary>
        // <param name="entityMemberName"> The name of the entity property that is changing </param>
        internal override void EntityMemberChanging(string entityMemberName)
        {
            if (IsKeyEntry)
            {
                throw new InvalidOperationException(Strings.ObjectStateEntry_CannotAccessKeyEntryValues);
            }
            EntityMemberChanging(entityMemberName, null, null);
        }

        // <summary>
        // Used to report that a scalar entity property has been changed
        // The property value that was cached during EntityMemberChanging is now
        // added to OriginalValues
        // </summary>
        // <param name="entityMemberName"> The name of the entity property that has changing </param>
        internal override void EntityMemberChanged(string entityMemberName)
        {
            if (IsKeyEntry)
            {
                throw new InvalidOperationException(Strings.ObjectStateEntry_CannotAccessKeyEntryValues);
            }
            EntityMemberChanged(entityMemberName, null, null);
        }

        // <summary>
        // Used to report that a complex property is about to change
        // The current value of the specified property is cached when this method is called.
        // </summary>
        // <param name="entityMemberName"> The name of the top-level entity property that is changing </param>
        // <param name="complexObject"> The complex object that contains the property that is changing </param>
        // <param name="complexObjectMemberName"> The name of the property that is changing on complexObject </param>
        internal override void EntityComplexMemberChanging(string entityMemberName, object complexObject, string complexObjectMemberName)
        {
            DebugCheck.NotEmpty(entityMemberName);
            DebugCheck.NotNull(complexObject);
            DebugCheck.NotEmpty(complexObjectMemberName);

            if (IsKeyEntry)
            {
                throw new InvalidOperationException(Strings.ObjectStateEntry_CannotAccessKeyEntryValues);
            }
            EntityMemberChanging(entityMemberName, complexObject, complexObjectMemberName);
        }

        // <summary>
        // Used to report that a complex property has been changed
        // The property value that was cached during EntityMemberChanging is now added to OriginalValues
        // </summary>
        // <param name="entityMemberName"> The name of the top-level entity property that has changed </param>
        // <param name="complexObject"> The complex object that contains the property that changed </param>
        // <param name="complexObjectMemberName"> The name of the property that changed on complexObject </param>
        internal override void EntityComplexMemberChanged(string entityMemberName, object complexObject, string complexObjectMemberName)
        {
            DebugCheck.NotEmpty(entityMemberName);
            DebugCheck.NotNull(complexObject);
            DebugCheck.NotEmpty(complexObjectMemberName);

            if (IsKeyEntry)
            {
                throw new InvalidOperationException(Strings.ObjectStateEntry_CannotAccessKeyEntryValues);
            }
            EntityMemberChanged(entityMemberName, complexObject, complexObjectMemberName);
        }

        #endregion

        internal IEntityWrapper WrappedEntity
        {
            get { return _wrappedEntity; }
        }

        // <summary>
        // Method called to complete the change tracking process on an entity property. The original property value
        // is now saved in the original values record if there is not already an entry in the record for this property.
        // The parameters to this method must have the same values as the parameter values passed to the last call to
        // EntityValueChanging on this ObjectStateEntry.
        // All inputs are in OSpace.
        // </summary>
        // <param name="entityMemberName"> Name of the top-level entity property that has changed </param>
        // <param name="complexObject"> If entityMemberName refers to a complex property, this is the complex object that contains the change. Otherwise this is null. </param>
        // <param name="complexObjectMemberName"> If entityMemberName refers to a complex property, this is the name of the property that has changed on complexObject. Otherwise this is null. </param>
        private void EntityMemberChanged(string entityMemberName, object complexObject, string complexObjectMemberName)
        {
            string changingMemberName;
            StateManagerTypeMetadata typeMetadata;
            object changingObject;

            // Get the metadata for the property that is changing, and verify that it is valid to change it for this entry
            // If something fails, we will clear out our cached values in the finally block, and require the user to submit another Changing notification
            try
            {
                var changingOrdinal = GetAndValidateChangeMemberInfo(
                    entityMemberName, complexObject, complexObjectMemberName,
                    out typeMetadata, out changingMemberName, out changingObject);

                // if EntityKey is changing and is in a valid scenario for it to change, no further action is needed
                if (changingOrdinal == -2)
                {
                    return;
                }

                // Verify that the inputs to this call match the values we have cached
                if (changingObject != _cache.ChangingObject
                    || changingMemberName != _cache.ChangingMember
                    || entityMemberName != _cache.ChangingEntityMember)
                {
                    throw new InvalidOperationException(Strings.ObjectStateEntry_EntityMemberChangedWithoutEntityMemberChanging);
                }

                // check the state after the other values because if the other cached values have not been set and are null, it is more
                // intuitive to the user to get an error that specifically points to that as the problem, and in that case, the state will
                // also not be matched, so if we checked this first, it would cause a confusing error to be thrown.
                if (State != _cache.ChangingState)
                {
                    throw new InvalidOperationException(
                        Strings.ObjectStateEntry_ChangedInDifferentStateFromChanging(_cache.ChangingState, State));
                }

                var oldValue = _cache.ChangingOldValue;
                object newValue = null;
                StateManagerMemberMetadata memberMetadata = null;
                if (_cache.SaveOriginalValues)
                {
                    memberMetadata = typeMetadata.Member(changingOrdinal);
                    // Expand only non-null complex type values
                    if (memberMetadata.IsComplex
                        && oldValue != null)
                    {
                        newValue = memberMetadata.GetValue(changingObject);

                        ExpandComplexTypeAndAddValues(memberMetadata, oldValue, newValue, false);
                    }
                    else
                    {
                        AddOriginalValueAt(-1, memberMetadata, changingObject, oldValue);
                    }
                }

                // if the property is a Foreign Key, let's clear out the appropriate EntityReference
                // UNLESS we are applying FK changes as part of DetectChanges where we don't want to 
                // start changing references yet. If we are in the Align stage of DetectChanges, this is ok.
                var transManager = ObjectStateManager.TransactionManager;
                List<Pair<string, string>> relationships;
                if (complexObject == null
                    && // check if property is a top-level property
                    (transManager.IsAlignChanges || !transManager.IsDetectChanges)
                    && IsPropertyAForeignKey(entityMemberName, out relationships))
                {
                    foreach (var relationship in relationships)
                    {
                        var relationshipName = relationship.First;
                        var targetRoleName = relationship.Second;

                        var relatedEnd = WrappedEntity.RelationshipManager.GetRelatedEndInternal(relationshipName, targetRoleName);
                        Debug.Assert(relatedEnd != null, "relatedEnd should exist if property is a foreign key");
                        var reference = relatedEnd as EntityReference;
                        Debug.Assert(reference != null, "relatedEnd should be an EntityReference");

                        // Allow updating of other relationships that this FK property participates in except that
                        // if we're doing fixup by references as part of AcceptChanges then don't allow a ref to 
                        // be changed.
                        if (!transManager.IsFixupByReference)
                        {
                            if (memberMetadata == null)
                            {
                                memberMetadata = typeMetadata.Member(changingOrdinal);
                            }
                            if (newValue == null)
                            {
                                newValue = memberMetadata.GetValue(changingObject);
                            }

                            var hasConceptualNullFk = ForeignKeyFactory.IsConceptualNullKey(reference.CachedForeignKey);
                            if (!ByValueEqualityComparer.Default.Equals(oldValue, newValue) || hasConceptualNullFk)
                            {
                                FixupEntityReferenceByForeignKey(reference);
                            }
                        }
                    }
                }

                // POCO: The state of the entry is not changed if the EntityMemberChanged method 
                // was called from ObjectStateEntry.OriginalValues property.
                // The OriginalValues uses EntityMemberChanging/EntityMemberChanged to update snapshot of complex object in case
                // complex object was changed (not a scalar value).
                if (_cache != null
                    && !_cache.TransactionManager.IsOriginalValuesGetter)
                {
                    var initialState = State;
                    if (State != EntityState.Added)
                    {
                        State = EntityState.Modified;
                    }
                    if (State == EntityState.Modified)
                    {
                        SetModifiedProperty(entityMemberName);
                    }
                    if (initialState != State)
                    {
                        _cache.ChangeState(this, initialState, State);
                    }
                }
            }
            finally
            {
                Debug.Assert(_cache != null, "Unexpected null state manager.");
                SetCachedChangingValues(null, null, null, EntityState.Detached, null);
            }
        }

        // helper method used to set value of property
        internal void SetCurrentEntityValue(string memberName, object newValue)
        {
            var ordinal = _cacheTypeMetadata.GetOrdinalforOLayerMemberName(memberName);
            SetCurrentEntityValue(_cacheTypeMetadata, ordinal, _wrappedEntity.Entity, newValue);
        }

        internal void SetOriginalEntityValue(StateManagerTypeMetadata metadata, int ordinal, object userObject, object newValue)
        {
            ValidateState();
            if (State == EntityState.Added)
            {
                throw new InvalidOperationException(Strings.ObjectStateEntry_OriginalValuesDoesNotExist);
            }

            var initialState = State;

            // Update original values list
            var memberMetadata = metadata.Member(ordinal);
            var originalValueIndex = FindOriginalValueIndex(memberMetadata, userObject);

            if (memberMetadata.IsComplex)
            {
                if (originalValueIndex >= 0)
                {
                    _originalValues.RemoveAt(originalValueIndex);
                }

                var oldOriginalValue = memberMetadata.GetValue(userObject); // the actual value
                if (oldOriginalValue == null)
                {
                    throw new InvalidOperationException(Strings.ComplexObject_NullableComplexTypesNotSupported(memberMetadata.CLayerName));
                }

                var newValueRecord = newValue as IExtendedDataRecord;
                if (newValueRecord != null)
                {
                    // Requires materialization
                    newValue = _cache.ComplexTypeMaterializer.CreateComplex(newValueRecord, newValueRecord.DataRecordInfo, null);
                }

                // We only store scalar properties values in original values, so no need to search the list
                // if the property being set is complex. Just get the value as an OSpace object.
                ExpandComplexTypeAndAddValues(memberMetadata, oldOriginalValue, newValue, true);
            }
            else
            {
                AddOriginalValueAt(originalValueIndex, memberMetadata, userObject, newValue);
            }

            if (initialState == EntityState.Unchanged)
            {
                State = EntityState.Modified;
            }
        }

        // <summary>
        // Method called to start the change tracking process on an entity property. The current property value is cached at
        // this stage in preparation for later storage in the original values record. Multiple successful calls to this method
        // will overwrite the cached values.
        // All inputs are in OSpace.
        // </summary>
        // <param name="entityMemberName"> Name of the top-level entity property that is changing </param>
        // <param name="complexObject"> If entityMemberName refers to a complex property, this is the complex object that contains the change. Otherwise this is null. </param>
        // <param name="complexObjectMemberName"> If entityMemberName refers to a complex property, this is the name of the property that is changing on complexObject. Otherwise this is null. </param>
        private void EntityMemberChanging(string entityMemberName, object complexObject, string complexObjectMemberName)
        {
            string changingMemberName;
            StateManagerTypeMetadata typeMetadata;
            object changingObject;

            // Get the metadata for the property that is changing, and verify that it is valid to change it for this entry
            var changingOrdinal = GetAndValidateChangeMemberInfo(
                entityMemberName, complexObject, complexObjectMemberName,
                out typeMetadata, out changingMemberName, out changingObject);

            // if EntityKey is changing and is in a valid scenario for it to change, no further action is needed
            if (changingOrdinal == -2)
            {
                return;
            }

            Debug.Assert(changingOrdinal != -1, "Expected GetAndValidateChangeMemberInfo to throw for a invalid property name");

            // Cache the current value for later storage in original values. If we are not in a state where we should update
            // the original values, we don't even need to bother saving the current value here. However, we will still cache
            // the other data regarding the change, so that we always require matching Changing and Changed calls, regardless of the state.
            var memberMetadata = typeMetadata.Member(changingOrdinal);

            // POCO
            // Entities which don't implement IEntityWithChangeTracker entity can already have original values even in the Unchanged state.
            _cache.SaveOriginalValues = (State == EntityState.Unchanged || State == EntityState.Modified) &&
                                        FindOriginalValueIndex(memberMetadata, changingObject) == -1;

            // devnote: Not using GetCurrentEntityValue here because change tracking can only be done on OSpace members,
            //          so we don't need to worry about shadow state, and we don't want a CSpace representation of complex objects
            var oldValue = memberMetadata.GetValue(changingObject);

            Debug.Assert(State != EntityState.Detached, "Change tracking should not happen on detached entities.");
            SetCachedChangingValues(entityMemberName, changingObject, changingMemberName, State, oldValue);
        }

        // helper method used to get value of property
        internal object GetOriginalEntityValue(string memberName)
        {
            var ordinal = _cacheTypeMetadata.GetOrdinalforOLayerMemberName(memberName);
            return GetOriginalEntityValue(_cacheTypeMetadata, ordinal, _wrappedEntity.Entity, ObjectStateValueRecord.OriginalReadonly);
        }

        internal object GetOriginalEntityValue(
            StateManagerTypeMetadata metadata, int ordinal, object userObject, ObjectStateValueRecord updatableRecord)
        {
            Debug.Assert(
                updatableRecord != ObjectStateValueRecord.OriginalUpdatablePublic,
                "OriginalUpdatablePublic records must preserve complex type information, use the overload that takes parentEntityPropertyIndex");
            return GetOriginalEntityValue(metadata, ordinal, userObject, updatableRecord, s_EntityRoot);
        }

        internal object GetOriginalEntityValue(
            StateManagerTypeMetadata metadata, int ordinal, object userObject, ObjectStateValueRecord updatableRecord,
            int parentEntityPropertyIndex)
        {
            ValidateState();
            return GetOriginalEntityValue(metadata, metadata.Member(ordinal), ordinal, userObject, updatableRecord, parentEntityPropertyIndex);
        }

        internal object GetOriginalEntityValue(
            StateManagerTypeMetadata metadata, StateManagerMemberMetadata memberMetadata, 
            int ordinal, object userObject, ObjectStateValueRecord updatableRecord, int parentEntityPropertyIndex)
        {
            // if original value is stored, then use it, otherwise use the current value from the entity
            var originalValueIndex = FindOriginalValueIndex(memberMetadata, userObject);
            if (originalValueIndex >= 0)
            {
                // If the object is null, return DBNull.Value to be consistent with GetCurrentEntityValue
                return _originalValues[originalValueIndex].OriginalValue ?? DBNull.Value;
            }
            return GetCurrentEntityValue(metadata, ordinal, userObject, updatableRecord, parentEntityPropertyIndex);
        }

        internal object GetCurrentEntityValue(
            StateManagerTypeMetadata metadata, int ordinal, object userObject, ObjectStateValueRecord updatableRecord)
        {
            Debug.Assert(
                updatableRecord != ObjectStateValueRecord.OriginalUpdatablePublic,
                "OriginalUpdatablePublic records must preserve complex type information, use the overload that takes parentEntityPropertyIndex");
            return GetCurrentEntityValue(metadata, ordinal, userObject, updatableRecord, s_EntityRoot);
        }

        internal object GetCurrentEntityValue(
            StateManagerTypeMetadata metadata, int ordinal, object userObject, ObjectStateValueRecord updatableRecord,
            int parentEntityPropertyIndex)
        {
            ValidateState();

            object retValue = null;
            var member = metadata.Member(ordinal);
            Debug.Assert(null != member, "didn't throw ArgumentOutOfRangeException");

            retValue = member.GetValue(userObject);

            // Wrap the value in a record if it is a non-null complex type
            if (member.IsComplex
                && retValue != null)
            {
                // need to get the new StateManagerTypeMetadata for nested /complext member
                switch (updatableRecord)
                {
                    case ObjectStateValueRecord.OriginalReadonly:
                        retValue = new ObjectStateEntryDbDataRecord(
                            this,
                            _cache.GetOrAddStateManagerTypeMetadata(member.CdmMetadata.TypeUsage.EdmType), retValue);
                        break;
                    case ObjectStateValueRecord.CurrentUpdatable:
                        retValue = new ObjectStateEntryDbUpdatableDataRecord(
                            this,
                            _cache.GetOrAddStateManagerTypeMetadata(member.CdmMetadata.TypeUsage.EdmType), retValue);
                        break;
                    case ObjectStateValueRecord.OriginalUpdatableInternal:
                        retValue = new ObjectStateEntryOriginalDbUpdatableDataRecord_Internal(
                            this,
                            _cache.GetOrAddStateManagerTypeMetadata(member.CdmMetadata.TypeUsage.EdmType), retValue);
                        break;
                    case ObjectStateValueRecord.OriginalUpdatablePublic:
                        retValue = new ObjectStateEntryOriginalDbUpdatableDataRecord_Public(
                            this,
                            _cache.GetOrAddStateManagerTypeMetadata(member.CdmMetadata.TypeUsage.EdmType), retValue,
                            parentEntityPropertyIndex);
                        break;
                    default:
                        Debug.Assert(false, "shouldn't happen");
                        break;
                }
                // we need to pass the top level ordinal
            }
            return retValue ?? DBNull.Value;
        }

        internal int FindOriginalValueIndex(StateManagerMemberMetadata metadata, object instance)
        {
            if (_originalValues != null)
            {
                for (var i = 0; i < _originalValues.Count; i++)
                {
                    if (ReferenceEquals(_originalValues[i].UserObject, instance)
                        && ReferenceEquals(_originalValues[i].MemberMetadata, metadata))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        // Get AssociationEndMember of current entry of given relationship
        // Relationship must be related to the current entry.
        internal AssociationEndMember GetAssociationEndMember(RelationshipEntry relationshipEntry)
        {
            Debug.Assert(EntityKey != null, "entry should have a not null EntityKey");

            ValidateState();

            var endMember = relationshipEntry.RelationshipWrapper.GetAssociationEndMember(EntityKey);
            Debug.Assert(null != endMember, "should be one of the ends of the relationship");
            return endMember;
        }

        // Get entry which is on the other end of given relationship.
        // Relationship must be related to the current entry.
        internal EntityEntry GetOtherEndOfRelationship(RelationshipEntry relationshipEntry)
        {
            Debug.Assert(EntityKey != null, "entry should have a not null EntityKey");

            return _cache.GetEntityEntry(relationshipEntry.RelationshipWrapper.GetOtherEntityKey(EntityKey));
        }

        // <summary>
        // Helper method to recursively expand a complex object's values down to scalars for storage in the original values record.
        // This method is used when a whole complex object is set on its parent object, instead of just setting
        // individual scalar values on that object.
        // </summary>
        // <param name="memberMetadata"> metadata for the complex property being expanded on the parent where the parent can be an entity or another complex object </param>
        // <param name="oldComplexObject"> Old value of the complex property. Scalar values from this object are stored in the original values record </param>
        // <param name="newComplexObject"> New value of the complex property. This object reference is used in the original value record and is associated with the scalar values for the same property on the oldComplexObject </param>
        // <param name="useOldComplexObject"> Whether or not to use the existing complex object in the original values or to use the original value that is already present </param>
        internal void ExpandComplexTypeAndAddValues(
            StateManagerMemberMetadata memberMetadata, object oldComplexObject, object newComplexObject, bool useOldComplexObject)
        {
            Debug.Assert(memberMetadata.IsComplex, "Cannot expand non-complex objects");
            if (newComplexObject == null)
            {
                throw new InvalidOperationException(Strings.ComplexObject_NullableComplexTypesNotSupported(memberMetadata.CLayerName));
            }
            Debug.Assert(
                oldComplexObject == null || (oldComplexObject.GetType() == newComplexObject.GetType()),
                "Cannot replace a complex object with an object of a different type, unless the original one was null");

            var typeMetadata = _cache.GetOrAddStateManagerTypeMetadata(memberMetadata.CdmMetadata.TypeUsage.EdmType);
            for (var ordinal = 0; ordinal < typeMetadata.FieldCount; ordinal++)
            {
                var complexMemberMetadata = typeMetadata.Member(ordinal);
                if (complexMemberMetadata.IsComplex)
                {
                    object oldComplexMemberValue = null;
                    if (oldComplexObject != null)
                    {
                        oldComplexMemberValue = complexMemberMetadata.GetValue(oldComplexObject);

                        if (oldComplexMemberValue == null)
                        {
                            var orignalValueIndex = FindOriginalValueIndex(complexMemberMetadata, oldComplexObject);
                            if (orignalValueIndex >= 0)
                            {
                                _originalValues.RemoveAt(orignalValueIndex);
                            }
                        }
                    }
                    ExpandComplexTypeAndAddValues(
                        complexMemberMetadata, oldComplexMemberValue, complexMemberMetadata.GetValue(newComplexObject), useOldComplexObject);
                }
                else
                {
                    object originalValue;
                    var complexObject = newComplexObject;
                    var originalValueIndex = -1;

                    if (useOldComplexObject)
                    {
                        // Set the original values using the existing current value object
                        // complexObject --> the existing complex object
                        // originalValue --> the new value to set for this member
                        originalValue = complexMemberMetadata.GetValue(newComplexObject);
                        complexObject = oldComplexObject;
                    }
                    else
                    {
                        if (oldComplexObject != null)
                        {
                            originalValue = complexMemberMetadata.GetValue(oldComplexObject);
                            originalValueIndex = FindOriginalValueIndex(complexMemberMetadata, oldComplexObject);
                            if (originalValueIndex >= 0)
                            {
                                originalValue = _originalValues[originalValueIndex].OriginalValue;
                            }
                            else
                            {
                                Debug.Assert(
                                    Entity is IEntityWithChangeTracker, "for POCO objects the snapshot should contain all original values");
                            }
                        }
                        else
                        {
                            originalValue = complexMemberMetadata.GetValue(newComplexObject);
                        }
                    }

                    // Add the new entry. The userObject will reference the new complex object that is currently being set.
                    // If the value was in the list previously, we will still use the old value with the new object reference.
                    // That will ensure that we preserve the old value while still maintaining the link to the
                    // existing complex object that is attached to the entity or parent complex object. If an entry is already
                    // in the list this means that it was either explicitly set by the user or the entire complex type was previously
                    // set and expanded down to the individual properties.  In either case we do the same thing.
                    AddOriginalValueAt(originalValueIndex, complexMemberMetadata, complexObject, originalValue);
                }
            }
        }

        // <summary>
        // Helper method to validate that the property names being reported as changing/changed are valid for this entity and that
        // the entity is in a valid state for the change request. Also determines if this is a change on a complex object, and
        // returns the appropriate metadata and object to be used for the rest of the changing and changed operations.
        // </summary>
        // <param name="entityMemberName"> Top-level entity property name </param>
        // <param name="complexObject"> Complex object that contains the change, null if the change is on a top-level entity property </param>
        // <param name="complexObjectMemberName"> Name of the property that is changing on the complexObject, null for top-level entity properties </param>
        // <param name="typeMetadata"> Metadata for the type that contains the change, either for the entity itself or for the complex object </param>
        // <param name="changingMemberName"> Property name that is actually changing -- either entityMemberName for entities or complexObjectMemberName for complex objects </param>
        // <param name="changingObject"> Object reference that contains the change, either the entity or complex object as appropriate for the requested change </param>
        // <returns> Ordinal of the property that is changing, or -2 if the EntityKey is changing in a valid scenario. This is relative to the returned typeMetadata. Throws exceptions if the requested property name(s) are invalid for this entity. </returns>
        internal int GetAndValidateChangeMemberInfo(
            string entityMemberName, object complexObject, string complexObjectMemberName,
            out StateManagerTypeMetadata typeMetadata, out string changingMemberName, out object changingObject)
        {
            Check.NotNull(entityMemberName, "entityMemberName");

            typeMetadata = null;
            changingMemberName = null;
            changingObject = null;

            // complexObject and complexObjectMemberName are allowed to be null here for change tracking on top-level entity properties

            ValidateState();

            var changingOrdinal = _cacheTypeMetadata.GetOrdinalforOLayerMemberName(entityMemberName);
            if (changingOrdinal == -1)
            {
                if (entityMemberName == StructuralObject.EntityKeyPropertyName)
                {
                    // Setting EntityKey property is only allowed from here when we are in the middle of relationship fixup.
                    if (!_cache.InRelationshipFixup)
                    {
                        throw new InvalidOperationException(Strings.ObjectStateEntry_CantSetEntityKey);
                    }
                    else
                    {
                        // If we are in fixup, there is nothing more to do here with EntityKey, so just
                        // clear the saved changing values and return. This will ensure that we behave
                        // the same with the change notifications on EntityKey as with other properties.
                        // I.e. we still don't allow the following:
                        //     EntityMemberChanging("Property1")
                        //     EntityMemberChanging("EntityKey")
                        //     EntityMemberChanged("EntityKey")
                        //     EntityMemberChanged("Property1")
                        Debug.Assert(State != EntityState.Detached, "Change tracking should not happen on detached entities.");
                        SetCachedChangingValues(null, null, null, State, null);
                        return -2;
                    }
                }
                else
                {
                    throw new ArgumentException(Strings.ObjectStateEntry_ChangeOnUnmappedProperty(entityMemberName));
                }
            }
            else
            {
                StateManagerTypeMetadata tmpTypeMetadata;
                string tmpChangingMemberName;
                object tmpChangingObject;

                // entityMemberName is a confirmed valid property on the Entity, but if this is a complex type we also need to validate its property
                if (complexObject != null)
                {
                    // a complex object was provided, but the top-level Entity property is not complex
                    if (!_cacheTypeMetadata.Member(changingOrdinal).IsComplex)
                    {
                        throw new ArgumentException(Strings.ComplexObject_ComplexChangeRequestedOnScalarProperty(entityMemberName));
                    }

                    tmpTypeMetadata = _cache.GetOrAddStateManagerTypeMetadata(complexObject.GetType(), (EntitySet)EntitySet);
                    changingOrdinal = tmpTypeMetadata.GetOrdinalforOLayerMemberName(complexObjectMemberName);
                    if (changingOrdinal == -1)
                    {
                        throw new ArgumentException(Strings.ObjectStateEntry_ChangeOnUnmappedComplexProperty(complexObjectMemberName));
                    }

                    tmpChangingMemberName = complexObjectMemberName;
                    tmpChangingObject = complexObject;
                }
                else
                {
                    tmpTypeMetadata = _cacheTypeMetadata;
                    tmpChangingMemberName = entityMemberName;
                    tmpChangingObject = Entity;
                    if (WrappedEntity.IdentityType != Entity.GetType()
                        && // Is a proxy
                        Entity is IEntityWithChangeTracker
                        && // Is a full proxy
                        IsPropertyAForeignKey(entityMemberName)) // Property is part of FK
                    {
                        // Set a flag so that we don't try to set FK properties while already in a setter.
                        _cache.EntityInvokingFKSetter = WrappedEntity.Entity;
                    }
                }

                VerifyEntityValueIsEditable(tmpTypeMetadata, changingOrdinal, tmpChangingMemberName);

                typeMetadata = tmpTypeMetadata;
                changingMemberName = tmpChangingMemberName;
                changingObject = tmpChangingObject;
                return changingOrdinal;
            }
        }

        // <summary>
        // Helper method to set the information needed for the change tracking cache. Ensures that all of these values get set together.
        // </summary>
        private void SetCachedChangingValues(
            string entityMemberName, object changingObject, string changingMember, EntityState changingState, object oldValue)
        {
            _cache.ChangingEntityMember = entityMemberName;
            _cache.ChangingObject = changingObject;
            _cache.ChangingMember = changingMember;
            _cache.ChangingState = changingState;
            _cache.ChangingOldValue = oldValue;
            if (changingState == EntityState.Detached)
            {
                _cache.SaveOriginalValues = false;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal OriginalValueRecord EditableOriginalValues
        {
            get
            {
                Debug.Assert(!IsKeyEntry, "should not edit original key entry");
                Debug.Assert(
                    EntityState.Modified == State ||
                    EntityState.Deleted == State ||
                    EntityState.Unchanged == State, "only expecting Modified or Deleted state");

                return new ObjectStateEntryOriginalDbUpdatableDataRecord_Internal(this, _cacheTypeMetadata, _wrappedEntity.Entity);
            }
        }

        internal void DetachObjectStateManagerFromEntity()
        {
            // This method can be called on relationship entries where there is no entity
            if (!IsKeyEntry) // _wrappedEntity.Entity is not null.
            {
                _wrappedEntity.SetChangeTracker(null);
                _wrappedEntity.DetachContext();

                if (!_cache.TransactionManager.IsAttachTracking
                    ||
                    _cache.TransactionManager.OriginalMergeOption != MergeOption.NoTracking)
                {
                    // If AttachTo() failed while attaching graph retrieved with NoTracking option,
                    // we don't want to reset the EntityKey

                    //Entry's this._entityKey is set to null at the caller, maintaining consistency between entityWithKey.EntityKey and this.EntityKey
                    _wrappedEntity.EntityKey = null;
                }
            }
        }

        // This method is used for entities which don't implement IEntityWithChangeTracker to store orignal values of properties
        // which are later used to detect changes in properties
        internal void TakeSnapshot(bool onlySnapshotComplexProperties)
        {
            Debug.Assert(!IsKeyEntry);

            if (State != EntityState.Added)
            {
                var metadata = _cacheTypeMetadata;

                var fieldCount = GetFieldCount(metadata);
                object currentValue;

                for (var ordinal = 0; ordinal < fieldCount; ordinal++)
                {
                    var member = metadata.Member(ordinal);
                    if (member.IsComplex)
                    {
                        // memberValue is a complex object
                        currentValue = member.GetValue(_wrappedEntity.Entity);
                        AddComplexObjectSnapshot(Entity, ordinal, currentValue);
                        TakeSnapshotOfComplexType(member, currentValue);
                    }
                    else if (!onlySnapshotComplexProperties)
                    {
                        currentValue = member.GetValue(_wrappedEntity.Entity);
                        AddOriginalValueAt(-1, member, _wrappedEntity.Entity, currentValue);
                    }
                }
            }

            TakeSnapshotOfForeignKeys();
        }

        internal void TakeSnapshotOfForeignKeys()
        {
            Dictionary<RelatedEnd, HashSet<EntityKey>> keys;
            FindRelatedEntityKeysByForeignKeys(out keys, useOriginalValues: false);
            if (keys != null)
            {
                foreach (var pair in keys)
                {
                    var reference = pair.Key as EntityReference;
                    Debug.Assert(reference != null, "EntityReference expected");
                    Debug.Assert(pair.Value.Count == 1, "Unexpected number of keys");

                    if (!ForeignKeyFactory.IsConceptualNullKey(reference.CachedForeignKey))
                    {
                        reference.SetCachedForeignKey(pair.Value.First(), this);
                    }
                }
            }
        }

        private void TakeSnapshotOfComplexType(StateManagerMemberMetadata member, object complexValue)
        {
            Debug.Assert(member.IsComplex, "Cannot expand non-complex objects");

            // Skip null values
            if (complexValue == null)
            {
                return;
            }

            var typeMetadata = _cache.GetOrAddStateManagerTypeMetadata(member.CdmMetadata.TypeUsage.EdmType);
            for (var ordinal = 0; ordinal < typeMetadata.FieldCount; ordinal++)
            {
                var complexMember = typeMetadata.Member(ordinal);
                var currentValue = complexMember.GetValue(complexValue);
                if (complexMember.IsComplex)
                {
                    // Recursive call for nested complex types
                    // For POCO objects we have to store a reference to the original complex object
                    AddComplexObjectSnapshot(complexValue, ordinal, currentValue);
                    TakeSnapshotOfComplexType(complexMember, currentValue);
                }
                else
                {
                    if (FindOriginalValueIndex(complexMember, complexValue) == -1)
                    {
                        AddOriginalValueAt(-1, complexMember, complexValue, currentValue);
                    }
                }
            }
        }

        private void AddComplexObjectSnapshot(object userObject, int ordinal, object complexObject)
        {
            DebugCheck.NotNull(userObject);
            Debug.Assert(ordinal >= 0);

            if (complexObject == null)
            {
                return;
            }

            // Verify if the same complex object is not used multiple times.
            CheckForDuplicateComplexObjects(complexObject);

            if (_originalComplexObjects == null)
            {
                _originalComplexObjects = new Dictionary<object, Dictionary<int, object>>(ObjectReferenceEqualityComparer.Default);
            }
            Dictionary<int, object> ordinal2complexObject;
            if (!_originalComplexObjects.TryGetValue(userObject, out ordinal2complexObject))
            {
                ordinal2complexObject = new Dictionary<int, object>();
                _originalComplexObjects.Add(userObject, ordinal2complexObject);
            }

            Debug.Assert(!ordinal2complexObject.ContainsKey(ordinal), "shouldn't contain this ordinal yet");
            ordinal2complexObject.Add(ordinal, complexObject);
        }

        private void CheckForDuplicateComplexObjects(object complexObject)
        {
            if (_originalComplexObjects == null
                || complexObject == null)
            {
                return;
            }

            foreach (var ordinal2complexObject in _originalComplexObjects.Values)
            {
                foreach (var oldComplexObject in ordinal2complexObject.Values)
                {
                    if (ReferenceEquals(complexObject, oldComplexObject))
                    {
                        throw new InvalidOperationException(
                            Strings.ObjectStateEntry_ComplexObjectUsedMultipleTimes(
                                Entity.GetType().FullName, complexObject.GetType().FullName));
                    }
                }
            }
        }

        // <summary>
        // Uses DetectChanges to determine whether or not the current value of the property with the given
        // name is different from its original value. Note that this may be different from the property being
        // marked as modified since a property which has not changed can still be marked as modified.
        // </summary>
        // <remarks>
        // For complex properties, a new instance of the complex object which has all the same property
        // values as the original instance is not considered to be different by this method.
        // </remarks>
        // <param name="propertyName"> The name of the property. </param>
        // <returns> True if the property has changed; false otherwise. </returns>
        public override bool IsPropertyChanged(string propertyName)
        {
            // We need this because the Code Contract gets compiled out in the release build even though
            // this method is effectively on the public surface because it overrides the abstract method on ObjectStateEntry.
            // Using a CodeContractsFor class doesn't work in this case.
            Check.NotEmpty(propertyName, "propertyName");

            return DetectChangesInProperty(
                ValidateAndGetOrdinalForProperty(propertyName, "IsPropertyChanged"),
                detectOnlyComplexProperties: false, detectOnly: true);
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "originalValueFound",
            Justification = "Used in the debug build")]
        private bool DetectChangesInProperty(int ordinal, bool detectOnlyComplexProperties, bool detectOnly)
        {
            var changeDetected = false;
            var member = _cacheTypeMetadata.Member(ordinal);
            var currentValue = member.GetValue(_wrappedEntity.Entity);
            if (member.IsComplex)
            {
                if (State != EntityState.Deleted)
                {
                    var oldComplexValue = GetComplexObjectSnapshot(Entity, ordinal);
                    var complexObjectInstanceChanged = DetectChangesInComplexType(
                        member, member, currentValue, oldComplexValue, ref changeDetected, detectOnly);
                    if (complexObjectInstanceChanged)
                    {
                        // instance of complex object was changed

                        // Before updating the snapshot verify if the same complex object is not used multiple times.
                        CheckForDuplicateComplexObjects(currentValue);

                        if (!detectOnly)
                        {
                            // equivalent of EntityObject.ReportPropertyChanging()
                            ((IEntityChangeTracker)this).EntityMemberChanging(member.CLayerName);

                            Debug.Assert(
                                _cache.SaveOriginalValues,
                                "complex object instance was changed so the SaveOriginalValues flag should be set to true");

                            // Since the EntityMemberChanging method is called AFTER the complex object was changed, it means that
                            // the EntityMemberChanging method was unable to find the real oldValue.  
                            // The real old value is stored for POCO objects in _originalComplexObjects dictionary.
                            // The cached changing oldValue has to be updated with the real oldValue.
                            _cache.ChangingOldValue = oldComplexValue;

                            // equivalent of EntityObject.ReportPropertyChanged()
                            ((IEntityChangeTracker)this).EntityMemberChanged(member.CLayerName);
                        }

                        // The _originalComplexObjects should always contain references to the values of complex objects which are "original" 
                        // at the moment of calling GetComplexObjectSnapshot().  They are used to get original scalar values from _originalValues.
                        UpdateComplexObjectSnapshot(member, Entity, ordinal, currentValue);

                        if (!changeDetected)
                        {
                            // If we haven't already detected a change then we need to check the properties of the complex
                            // object to see if there are any changes so that IsPropertyChanged will not skip reporting the
                            // change just because the object reference has changed.
                            DetectChangesInComplexType(member, member, currentValue, oldComplexValue, ref changeDetected, detectOnly);
                        }
                    }
                }
            }
            else if (!detectOnlyComplexProperties)
            {
                var originalValueIndex = FindOriginalValueIndex(member, _wrappedEntity.Entity);

                if (originalValueIndex < 0)
                {
                    // This must be a change-tracking proxy or EntityObject entity, which means we are not keeping track
                    // of original values and have no way of knowing if the value is actually modified or just marked
                    // as modified. Therefore, we assume that if the property was marked as modified then it is modified.
                    return GetModifiedProperties().Contains(member.CLayerName);
                }

                var originalValue = _originalValues[originalValueIndex].OriginalValue;

                if (!Equals(currentValue, originalValue))
                {
                    changeDetected = true;

                    // Key property - throw if the actual byte values have changed, otherwise ignore the change
                    if (member.IsPartOfKey)
                    {
                        if (!ByValueEqualityComparer.Default.Equals(currentValue, originalValue))
                        {
                            throw new InvalidOperationException(Strings.ObjectStateEntry_CannotModifyKeyProperty(member.CLayerName));
                        }
                    }
                    else
                    {
                        if (State != EntityState.Deleted
                            && !detectOnly)
                        {
                            // equivalent of EntityObject.ReportPropertyChanging()
                            ((IEntityChangeTracker)this).EntityMemberChanging(member.CLayerName);

                            // equivalent of EntityObject.ReportPropertyChanged()
                            ((IEntityChangeTracker)this).EntityMemberChanged(member.CLayerName);
                        }
                    }
                }
            }

            return changeDetected;
        }

        // This method uses original values stored in the ObjectStateEntry to detect changes in values of entity's properties
        internal void DetectChangesInProperties(bool detectOnlyComplexProperties)
        {
            Debug.Assert(!IsKeyEntry, "Entry should be an EntityEntry");
            Debug.Assert(State != EntityState.Added, "This method should not be called for entries in Added state");

            var fieldCount = GetFieldCount(_cacheTypeMetadata);
            for (var i = 0; i < fieldCount; i++)
            {
                DetectChangesInProperty(i, detectOnlyComplexProperties, detectOnly: false);
            }
        }

        private bool DetectChangesInComplexType(
            StateManagerMemberMetadata topLevelMember,
            StateManagerMemberMetadata complexMember,
            object complexValue,
            object oldComplexValue,
            ref bool changeDetected,
            bool detectOnly)
        {
            Debug.Assert(complexMember.IsComplex, "Cannot expand non-complex objects");

            if (complexValue == null)
            {
                // If the values are just null, do not detect this as a change
                if (oldComplexValue == null)
                {
                    return false;
                }
                throw new InvalidOperationException(Strings.ComplexObject_NullableComplexTypesNotSupported(complexMember.CLayerName));
            }

            if (!ReferenceEquals(oldComplexValue, complexValue))
            {
                // Complex object instance was changed.  The calling method will update the snapshot of this object.
                return true;
            }

            Debug.Assert(oldComplexValue != null, "original complex type value should not be null at this point");

            var metadata = _cache.GetOrAddStateManagerTypeMetadata(complexMember.CdmMetadata.TypeUsage.EdmType);
            for (var ordinal = 0; ordinal < GetFieldCount(metadata); ordinal++)
            {
                var member = metadata.Member(ordinal);
                object currentValue = null;
                currentValue = member.GetValue(complexValue);
                if (member.IsComplex)
                {
                    if (State != EntityState.Deleted)
                    {
                        var oldNestedComplexValue = GetComplexObjectSnapshot(complexValue, ordinal);
                        var complexObjectInstanceChanged = DetectChangesInComplexType(
                            topLevelMember, member, currentValue, oldNestedComplexValue, ref changeDetected, detectOnly);
                        if (complexObjectInstanceChanged)
                        {
                            // instance of complex object was changed

                            // Before updating the snapshot verify if the same complex object is not used multiple times.
                            CheckForDuplicateComplexObjects(currentValue);

                            if (!detectOnly)
                            {
                                // equivalent of EntityObject.ReportComplexPropertyChanging()
                                ((IEntityChangeTracker)this).EntityComplexMemberChanging(
                                    topLevelMember.CLayerName, complexValue, member.CLayerName);

                                // Since the EntityComplexMemberChanging method is called AFTER the complex object was changed, it means that
                                // the EntityComplexMemberChanging method was unable to find real oldValue.  
                                // The real old value is stored for POCO objects in _originalComplexObjects dictionary.
                                // The cached changing oldValue has to be updated with the real oldValue.
                                _cache.ChangingOldValue = oldNestedComplexValue;

                                // equivalent of EntityObject.ReportComplexPropertyChanged()
                                ((IEntityChangeTracker)this).EntityComplexMemberChanged(
                                    topLevelMember.CLayerName, complexValue, member.CLayerName);
                            }
                            // The _originalComplexObjects should always contain references to the values of complex objects which are "original" 
                            // at the moment of calling GetComplexObjectSnapshot().  They are used to get original scalar values from _originalValues.
                            UpdateComplexObjectSnapshot(member, complexValue, ordinal, currentValue);

                            if (!changeDetected)
                            {
                                DetectChangesInComplexType(
                                    topLevelMember, member, currentValue, oldNestedComplexValue, ref changeDetected, detectOnly);
                            }
                        }
                    }
                }
                else
                {
                    var originalValueIndex = FindOriginalValueIndex(member, complexValue);
                    var originalValue = originalValueIndex == -1 ? null : _originalValues[originalValueIndex].OriginalValue;

                    // originalValueFound will be false if the complex value was initially null since then its original
                    // values will always be null, in which case all original scalar properties of the complex value are
                    // considered null.
                    if (!Equals(currentValue, originalValue))
                    {
                        changeDetected = true;

                        Debug.Assert(!member.IsPartOfKey, "Found member of complex type that is part of a key");

                        if (!detectOnly)
                        {
                            // equivalent of EntityObject.ReportComplexPropertyChanging()
                            ((IEntityChangeTracker)this).EntityComplexMemberChanging(
                                topLevelMember.CLayerName, complexValue, member.CLayerName);

                            // equivalent of EntityObject.ReportComplexPropertyChanged()
                            ((IEntityChangeTracker)this).EntityComplexMemberChanged(
                                topLevelMember.CLayerName, complexValue, member.CLayerName);
                        }
                    }
                }
            }

            // Scalar value in a complex object was changed
            return false;
        }

        private object GetComplexObjectSnapshot(object parentObject, int parentOrdinal)
        {
            object oldComplexObject = null;
            if (_originalComplexObjects != null)
            {
                Dictionary<int, object> ordinal2complexObject;
                if (_originalComplexObjects.TryGetValue(parentObject, out ordinal2complexObject))
                {
                    ordinal2complexObject.TryGetValue(parentOrdinal, out oldComplexObject);
                }
            }
            return oldComplexObject;
        }

        // The _originalComplexObjects should always contain references to the values of complex objects which are "original" 
        // at the moment of calling GetComplexObjectSnapshot().  They are used to get original scalar values from _originalValues
        // and to check if complex object instance was changed.
        // This method should be called after EntityMemberChanged in POCO case.
        internal void UpdateComplexObjectSnapshot(StateManagerMemberMetadata member, object userObject, int ordinal, object currentValue)
        {
            var requiresAdd = true;
            if (_originalComplexObjects != null)
            {
                Dictionary<int, object> ordinal2complexObject;
                if (_originalComplexObjects.TryGetValue(userObject, out ordinal2complexObject))
                {
                    Debug.Assert(ordinal2complexObject != null, "value should already exists");

                    object oldValue;
                    ordinal2complexObject.TryGetValue(ordinal, out oldValue);
                    // oldValue may be null if the complex object was attached with a null value
                    ordinal2complexObject[ordinal] = currentValue;

                    // check nested complex objects (if they exist)
                    if (oldValue != null
                        && _originalComplexObjects.TryGetValue(oldValue, out ordinal2complexObject))
                    {
                        _originalComplexObjects.Remove(oldValue);
                        _originalComplexObjects.Add(currentValue, ordinal2complexObject);

                        var typeMetadata = _cache.GetOrAddStateManagerTypeMetadata(member.CdmMetadata.TypeUsage.EdmType);
                        for (var i = 0; i < typeMetadata.FieldCount; i++)
                        {
                            var complexMember = typeMetadata.Member(i);
                            if (complexMember.IsComplex)
                            {
                                var nestedValue = complexMember.GetValue(currentValue);
                                // Recursive call for nested complex objects
                                UpdateComplexObjectSnapshot(complexMember, currentValue, i, nestedValue);
                            }
                        }
                    }
                    requiresAdd = false;
                }
            }
            if (requiresAdd)
            {
                AddComplexObjectSnapshot(userObject, ordinal, currentValue);
            }
        }

        // <summary>
        // Processes each dependent end of an FK relationship in this entity and determines if a nav
        // prop is set to a principal.  If it is, and if the principal is Unchanged or Modified,
        // then the primary key value is taken from the principal and used to fixup the FK value.
        // This is called during AddObject so that references set from the added object will take
        // precedence over FK values such that there is no need for the user to set FK values
        // explicitly.  If a conflict in the FK value is encountered due to an overlapping FK
        // that is tied to two different PK values, then an exception is thrown.
        // Note that references to objects that are not yet tracked by the context are ignored, since
        // they will ultimately be brought into the context as Added objects, at which point we would
        // have skipped them anyway because the are not Unchanged or Modified.
        // </summary>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "Only cast twice in debug mode.")]
        internal void FixupFKValuesFromNonAddedReferences()
        {
            Debug.Assert(EntitySet is EntitySet, "Expect entity entries to have true entity sets.");
            if (!((EntitySet)EntitySet).HasForeignKeyRelationships)
            {
                return;
            }

            // Keep track of all FK values that have already been set so that we can detect conflicts.
            var changedFKs = new Dictionary<int, object>();
            foreach (var dependent in ForeignKeyDependents)
            {
                var reference =
                    RelationshipManager.GetRelatedEndInternal(dependent.Item1.ElementType.FullName, dependent.Item2.FromRole.Name) as
                    EntityReference;
                Debug.Assert(reference != null, "Expected reference to exist and be an entity reference (not collection)");

                if (reference.TargetAccessor.HasProperty)
                {
                    var principal = WrappedEntity.GetNavigationPropertyValue(reference);
                    if (principal != null)
                    {
                        ObjectStateEntry principalEntry;
                        if (_cache.TryGetObjectStateEntry(principal, out principalEntry)
                            && (principalEntry.State == EntityState.Modified || principalEntry.State == EntityState.Unchanged))
                        {
                            Debug.Assert(
                                principalEntry is EntityEntry,
                                "Existing entry for an entity must be an EntityEntry, not a RelationshipEntry");
                            reference.UpdateForeignKeyValues(
                                WrappedEntity, ((EntityEntry)principalEntry).WrappedEntity, changedFKs, forceChange: false);
                        }
                    }
                }
            }
        }

        // Method used for entities which don't implement IEntityWithRelationships
        internal void TakeSnapshotOfRelationships()
        {
            Debug.Assert(_wrappedEntity != null, "wrapped entity shouldn't be null");
            Debug.Assert(
                !(_wrappedEntity.Entity is IEntityWithRelationships),
                "this method should be called only for entities which don't implement IEntityWithRelationships");

            var rm = _wrappedEntity.RelationshipManager;

            var metadata = _cacheTypeMetadata;

            var navigationProperties =
                (metadata.CdmMetadata.EdmType as EntityType).NavigationProperties;

            foreach (var n in navigationProperties)
            {
                var relatedEnd = rm.GetRelatedEndInternal(n.RelationshipType.FullName, n.ToEndMember.Name);
                var val = WrappedEntity.GetNavigationPropertyValue(relatedEnd);

                if (val != null)
                {
                    if (n.ToEndMember.RelationshipMultiplicity
                        == RelationshipMultiplicity.Many)
                    {
                        // Collection
                        var collection = val as IEnumerable;
                        if (collection == null)
                        {
                            throw new EntityException(
                                Strings.ObjectStateEntry_UnableToEnumerateCollection(n.Name, Entity.GetType().FullName));
                        }

                        foreach (var o in collection)
                        {
                            // Skip nulls in collections
                            if (o != null)
                            {
                                TakeSnapshotOfSingleRelationship(relatedEnd, n, o);
                            }
                        }
                    }
                    else
                    {
                        // Reference
                        TakeSnapshotOfSingleRelationship(relatedEnd, n, val);
                    }
                }
            }
        }

        private void TakeSnapshotOfSingleRelationship(RelatedEnd relatedEnd, NavigationProperty n, object o)
        {
            // Related entity can be already attached, so find the existing entry
            var relatedEntry = ObjectStateManager.FindEntityEntry(o);
            IEntityWrapper relatedWrapper;

            if (relatedEntry != null)
            {
                Debug.Assert(
                    ObjectStateManager.TransactionManager.IsAddTracking ||
                    ObjectStateManager.TransactionManager.IsAttachTracking, "Should be inside Attach or Add");

                //relatedEntry.VerifyOrUpdateRelatedEnd(n, this._wrappedEntity);
                relatedWrapper = relatedEntry._wrappedEntity;

                // In case of unidirectional relationships, it is possible that the other end of relationship was already added
                // to the context but its relationship manager doesn't contain proper related end with the current entity.
                // In OSM we treat all relationships as bidirectional so the related end has to be updated.
                var otherRelatedEnd = relatedWrapper.RelationshipManager.GetRelatedEndInternal(
                    n.RelationshipType.FullName, n.FromEndMember.Name);
                if (!otherRelatedEnd.ContainsEntity(_wrappedEntity))
                {
                    Debug.Assert(relatedWrapper.ObjectStateEntry != null, "Expected related entity to be tracked in snapshot code.");
                    if (relatedWrapper.ObjectStateEntry.State
                        == EntityState.Deleted)
                    {
                        throw Error.RelatedEnd_UnableToAddRelationshipWithDeletedEntity();
                    }
                    if (ObjectStateManager.TransactionManager.IsAttachTracking
                        && (State & (EntityState.Modified | EntityState.Unchanged)) != 0
                        && (relatedWrapper.ObjectStateEntry.State & (EntityState.Modified | EntityState.Unchanged)) != 0)
                    {
                        EntityEntry principalEntry = null;
                        EntityEntry dependentEntry = null;
                        if (relatedEnd.IsDependentEndOfReferentialConstraint(checkIdentifying: false))
                        {
                            principalEntry = relatedWrapper.ObjectStateEntry;
                            dependentEntry = this;
                        }
                        else if (otherRelatedEnd.IsDependentEndOfReferentialConstraint(checkIdentifying: false))
                        {
                            principalEntry = this;
                            dependentEntry = relatedWrapper.ObjectStateEntry;
                        }
                        if (principalEntry != null)
                        {
                            var constraint = ((AssociationType)relatedEnd.RelationMetadata).ReferentialConstraints[0];
                            if (!RelatedEnd.VerifyRIConstraintsWithRelatedEntry(
                                    constraint, dependentEntry.GetCurrentEntityValue, principalEntry.EntityKey))
                            {
                                throw new InvalidOperationException(constraint.BuildConstraintExceptionMessage());
                            }
                        }
                    }
                    // Keep track of the fact that we aligned the related end here so that we can undo
                    // it in rollback without wiping the already existing nav properties.
                    var otherEndAsRef = otherRelatedEnd as EntityReference;
                    if (otherEndAsRef != null
                        && otherEndAsRef.NavigationPropertyIsNullOrMissing())
                    {
                        ObjectStateManager.TransactionManager.AlignedEntityReferences.Add(otherEndAsRef);
                    }
                    otherRelatedEnd.AddToLocalCache(_wrappedEntity, applyConstraints: true);
                    otherRelatedEnd.OnAssociationChanged(CollectionChangeAction.Add, _wrappedEntity.Entity);
                }
            }
            else
            {
                if (!ObjectStateManager.TransactionManager.WrappedEntities.TryGetValue(o, out relatedWrapper))
                {
                    relatedWrapper = ObjectStateManager.EntityWrapperFactory.WrapEntityUsingStateManager(o, ObjectStateManager);
                }
            }

            if (!relatedEnd.ContainsEntity(relatedWrapper))
            {
                relatedEnd.AddToLocalCache(relatedWrapper, true);
                relatedEnd.OnAssociationChanged(CollectionChangeAction.Add, relatedWrapper.Entity);
            }
        }

        internal void DetectChangesInRelationshipsOfSingleEntity()
        {
            Debug.Assert(!IsKeyEntry, "Entry should be an EntityEntry");
            Debug.Assert(!(Entity is IEntityWithRelationships), "Entity shouldn't implement IEntityWithRelationships");

            var metadata = _cacheTypeMetadata;

            var navigationProperties =
                (metadata.CdmMetadata.EdmType as EntityType).NavigationProperties;

            foreach (var n in navigationProperties)
            {
                var relatedEnd = WrappedEntity.RelationshipManager.GetRelatedEndInternal(n.RelationshipType.FullName, n.ToEndMember.Name);
                Debug.Assert(relatedEnd != null, "relatedEnd is null");

                var val = WrappedEntity.GetNavigationPropertyValue(relatedEnd);

                var current = new HashSet<object>(ObjectReferenceEqualityComparer.Default);
                if (val != null)
                {
                    if (n.ToEndMember.RelationshipMultiplicity
                        == RelationshipMultiplicity.Many)
                    {
                        // Collection
                        var collection = val as IEnumerable;
                        if (collection == null)
                        {
                            throw new EntityException(
                                Strings.ObjectStateEntry_UnableToEnumerateCollection(n.Name, Entity.GetType().FullName));
                        }
                        foreach (var o in collection)
                        {
                            // Skip nulls in collections
                            if (o != null)
                            {
                                current.Add(o);
                            }
                        }
                    }
                    else
                    {
                        // Reference
                        current.Add(val);
                    }
                }

                // find deleted entities
                foreach (var o in relatedEnd.GetInternalEnumerable())
                {
                    if (!current.Contains(o))
                    {
                        AddRelationshipDetectedByGraph(
                            ObjectStateManager.TransactionManager.DeletedRelationshipsByGraph, o, relatedEnd, verifyForAdd: false);
                    }
                    else
                    {
                        current.Remove(o);
                    }
                }

                // "current" contains now only added entities
                foreach (var o in current)
                {
                    AddRelationshipDetectedByGraph(
                        ObjectStateManager.TransactionManager.AddedRelationshipsByGraph, o, relatedEnd, verifyForAdd: true);
                }
            }
        }

        private void AddRelationshipDetectedByGraph(
            Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<IEntityWrapper>>> relationships,
            object relatedObject,
            RelatedEnd relatedEndFrom,
            bool verifyForAdd)
        {
            var relatedWrapper = ObjectStateManager.EntityWrapperFactory.WrapEntityUsingStateManager(relatedObject, ObjectStateManager);

            AddDetectedRelationship(relationships, relatedWrapper, relatedEndFrom);

            var relatedEndTo = relatedEndFrom.GetOtherEndOfRelationship(relatedWrapper);

            if (verifyForAdd
                && relatedEndTo is EntityReference
                && ObjectStateManager.FindEntityEntry(relatedObject) == null)
            {
                // If the relatedObject is not tracked by the context, let's detect it before OSM.PerformAdd to avoid
                // making RelatedEnd.Add() more complicated (it would have to know when the values in relatedEndTo can be overriden, and when not
                relatedEndTo.VerifyNavigationPropertyForAdd(_wrappedEntity);
            }

            AddDetectedRelationship(relationships, _wrappedEntity, relatedEndTo);
        }

        private void AddRelationshipDetectedByForeignKey(
            Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>> relationships,
            Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>> principalRelationships,
            EntityKey relatedKey,
            EntityEntry relatedEntry,
            RelatedEnd relatedEndFrom)
        {
            Debug.Assert(!relatedKey.IsTemporary, "the relatedKey was created by a method which returns only permaanent keys");
            AddDetectedRelationship(relationships, relatedKey, relatedEndFrom);

            if (relatedEntry != null)
            {
                var relatedWrapper = relatedEntry.WrappedEntity;

                var relatedEndTo = relatedEndFrom.GetOtherEndOfRelationship(relatedWrapper);

                var permanentKeyOwner = ObjectStateManager.GetPermanentKey(relatedEntry.WrappedEntity, relatedEndTo, WrappedEntity);
                AddDetectedRelationship(principalRelationships, permanentKeyOwner, relatedEndTo);
            }
        }

        // <summary>
        // Designed to be used by Change Detection methods to insert
        // Added/Deleted relationships into <see cref="TransactionManager" />
        // Creates new entries in the dictionaries if required
        // </summary>
        // <typeparam name="T"> IEntityWrapper or EntityKey </typeparam>
        // <param name="relationships"> The set of detected relationships to add this entry to </param>
        // <param name="relatedObject"> The entity the relationship points to </param>
        // <param name="relatedEnd"> The related end the relationship originates from </param>
        private static void AddDetectedRelationship<T>(
            Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<T>>> relationships,
            T relatedObject,
            RelatedEnd relatedEnd)
        {
            // Update info about changes to this/from side of the relationship
            Dictionary<RelatedEnd, HashSet<T>> alreadyDetectedRelationshipsFrom;
            if (!relationships.TryGetValue(relatedEnd.WrappedOwner, out alreadyDetectedRelationshipsFrom))
            {
                alreadyDetectedRelationshipsFrom = new Dictionary<RelatedEnd, HashSet<T>>();
                relationships.Add(relatedEnd.WrappedOwner, alreadyDetectedRelationshipsFrom);
            }

            HashSet<T> objectsInRelatedEnd;
            if (!alreadyDetectedRelationshipsFrom.TryGetValue(relatedEnd, out objectsInRelatedEnd))
            {
                objectsInRelatedEnd = new HashSet<T>();
                alreadyDetectedRelationshipsFrom.Add(relatedEnd, objectsInRelatedEnd);
            }
            else
            {
                if (relatedEnd is EntityReference)
                {
                    Debug.Assert(objectsInRelatedEnd.Count() == 1, "unexpected number of entities for EntityReference");
                    var existingRelatedObject = objectsInRelatedEnd.First();
                    if (!Equals(existingRelatedObject, relatedObject))
                    {
                        throw new InvalidOperationException(
                            Strings.EntityReference_CannotAddMoreThanOneEntityToEntityReference(
                                relatedEnd.RelationshipNavigation.To, relatedEnd.RelationshipNavigation.RelationshipName));
                    }
                }
            }

            objectsInRelatedEnd.Add(relatedObject);
        }

        // <summary>
        // Detaches an entry and create in its place key entry if necessary
        // Removes relationships with another key entries and removes these key entries if necessary
        // </summary>
        internal void Detach()
        {
            ValidateState();

            Debug.Assert(!IsKeyEntry);

            var createKeyEntry = false;

            var relationshipManager = _wrappedEntity.RelationshipManager;
            Debug.Assert(relationshipManager != null, "Entity wrapper returned a null RelationshipManager");
            // Key entry should be created only when current entity is not in Added state
            // and if the entity is a "OneToOne" or "ZeroToOne" end of some existing relationship.
            createKeyEntry =
                State != EntityState.Added &&
                IsOneEndOfSomeRelationship();

            _cache.TransactionManager.BeginDetaching();
            try
            {
                // Remove current entity from collections/references (on both ends of relationship)
                // Relationship entries are removed from ObjectStateManager if current entity is in Added state
                // or if current entity is a "Many" end of the relationship.
                // NOTE In this step only relationship entries which have normal entity on the other end
                //      can be detached.
                // NOTE In this step no Deleted relationship entries are detached.
                relationshipManager.DetachEntityFromRelationships(State);
            }
            finally
            {
                _cache.TransactionManager.EndDetaching();
            }

            // Remove relationship entries which has a key entry on the other end.
            // If the key entry does not have any other relationship, it is removed from Object State Manager.
            // NOTE Relationship entries which have a normal entity on the other end are detached only if the relationship state is Deleted.
            DetachRelationshipsEntries(relationshipManager);

            var existingWrappedEntity = _wrappedEntity;
            var key = _entityKey;
            var state = State;

            if (createKeyEntry)
            {
                DegradeEntry();
            }
            else
            {
                // If entity is in state different than Added state, entityKey should not be set to null
                // EntityKey is set to null in
                //    ObjectStateManger.ChangeState() ->
                //    ObjectStateEntry.Reset() ->
                //    ObjectStateEntry.DetachObjectStateManagerFromEntity()

                // Store data required to restore the entity key if needed.
                _wrappedEntity.ObjectStateEntry = null;

                _cache.ChangeState(this, State, EntityState.Detached);
            }

            // In case the detach event modifies the key.
            if (state != EntityState.Added)
            {
                existingWrappedEntity.EntityKey = key;
            }
        }

        //"doFixup" equals to False is called from EntityCollection & Ref code only
        internal void Delete(bool doFixup)
        {
            ValidateState();

            if (IsKeyEntry)
            {
                throw new InvalidOperationException(Strings.ObjectStateEntry_CannotDeleteOnKeyEntry);
            }

            if (doFixup && State != EntityState.Deleted)
            {
                RelationshipManager.NullAllFKsInDependentsForWhichThisIsThePrincipal();
                NullAllForeignKeys(); // May set conceptual nulls which will later be removed
                FixupRelationships();
            }

            switch (State)
            {
                case EntityState.Added:
                    Debug.Assert(
                        EntityState.Added == State,
                        "Expected ObjectStateEntry state is Added; make sure FixupRelationship did not corrupt cache entry state");

                    _cache.ChangeState(this, EntityState.Added, EntityState.Detached);

                    Debug.Assert(null == _modifiedFields, "There should not be any modified fields");

                    break;
                case EntityState.Modified:
                    if (!doFixup)
                    {
                        // Even when we are not doing relationship fixup at the collection level, if the entry is not a relationship
                        // we need to check to see if there are relationships that are referencing keys that should be removed
                        // this mainly occurs in cascade delete scenarios
                        DeleteRelationshipsThatReferenceKeys(null, null);
                    }
                    Debug.Assert(
                        EntityState.Modified == State,
                        "Expected ObjectStateEntry state is Modified; make sure FixupRelationship did not corrupt cache entry state");
                    _cache.ChangeState(this, EntityState.Modified, EntityState.Deleted);
                    State = EntityState.Deleted;

                    break;
                case EntityState.Unchanged:
                    if (!doFixup)
                    {
                        // Even when we are not doing relationship fixup at the collection level, if the entry is not a relationship
                        // we need to check to see if there are relationships that are referencing keys that should be removed
                        // this mainly occurs in cascade delete scenarios
                        DeleteRelationshipsThatReferenceKeys(null, null);
                    }
                    Debug.Assert(State == EntityState.Unchanged, "Unexpected state");
                    Debug.Assert(
                        EntityState.Unchanged == State,
                        "Expected ObjectStateEntry state is Unchanged; make sure FixupRelationship did not corrupt cache entry state");
                    _cache.ChangeState(this, EntityState.Unchanged, EntityState.Deleted);
                    Debug.Assert(null == _modifiedFields, "There should not be any modified fields");
                    State = EntityState.Deleted;

                    break;
                case EntityState.Deleted:
                    // no-op
                    break;
            }
        }

        // <summary>
        // Nulls all FK values in this entity, or sets conceptual nulls if they are not nullable.
        // </summary>
        private void NullAllForeignKeys()
        {
            foreach (var dependent in ForeignKeyDependents)
            {
                var relatedEnd = WrappedEntity.RelationshipManager.GetRelatedEndInternal(
                    dependent.Item1.ElementType.FullName, dependent.Item2.FromRole.Name) as EntityReference;
                Debug.Assert(relatedEnd != null, "Expected non-null EntityReference to principal.");
                relatedEnd.NullAllForeignKeys();
            }
        }

        private bool IsOneEndOfSomeRelationship()
        {
            foreach (var relationshipEntry in _cache.FindRelationshipsByKey(EntityKey))
            {
                var multiplicity = GetAssociationEndMember(relationshipEntry).RelationshipMultiplicity;
                if (multiplicity == RelationshipMultiplicity.One
                    ||
                    multiplicity == RelationshipMultiplicity.ZeroOrOne)
                {
                    var targetKey = relationshipEntry.RelationshipWrapper.GetOtherEntityKey(EntityKey);
                    var relatedEntry = _cache.GetEntityEntry(targetKey);
                    // Relationships with KeyEntries don't count.
                    if (!relatedEntry.IsKeyEntry)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // Detaches related relationship entries if other ends of these relationships are key entries.
        // Detaches also related relationship entries if the entry is in Deleted state and the multiplicity is Many.
        // Key entry from the other side of the relationship is removed if is not related to other entries.
        private void DetachRelationshipsEntries(RelationshipManager relationshipManager)
        {
            DebugCheck.NotNull(relationshipManager);
            Debug.Assert(!IsKeyEntry, "Should only be detaching relationships with key entries if the source is not a key entry");

            foreach (var relationshipEntry in _cache.CopyOfRelationshipsByKey(EntityKey))
            {
                // Get state entry for other side of the relationship
                var targetKey = relationshipEntry.RelationshipWrapper.GetOtherEntityKey(EntityKey);
                Debug.Assert(targetKey != null, "EntityKey not on either side of relationship as expected");

                var relatedEntry = _cache.GetEntityEntry(targetKey);
                if (relatedEntry.IsKeyEntry)
                {
                    // This must be an EntityReference, so set the DetachedEntityKey if the relationship is currently Added or Unchanged  
                    // devnote: This assumes that we are in the middle of detaching the entity associated with this state entry, because
                    //          we don't always want to preserve the EntityKey for every detached relationship, if the source entity itself isn't being detached
                    if (relationshipEntry.State
                        != EntityState.Deleted)
                    {
                        var targetMember = relationshipEntry.RelationshipWrapper.GetAssociationEndMember(targetKey);
                        // devnote: Since we know the target end of this relationship is a key entry, it has to be a reference, so just cast
                        var entityReference =
                            (EntityReference)
                            relationshipManager.GetRelatedEndInternal(targetMember.DeclaringType.FullName, targetMember.Name);
                        entityReference.DetachedEntityKey = targetKey;
                    }
                    // else do nothing -- we can't null out the key for Deleted state, because there could be other relationships with this same source in a different state

                    // Remove key entry if necessary
                    relationshipEntry.DeleteUnnecessaryKeyEntries();
                    // Remove relationship entry
                    relationshipEntry.DetachRelationshipEntry();
                }
                else
                {
                    // Detach deleted relationships
                    if (relationshipEntry.State
                        == EntityState.Deleted)
                    {
                        var multiplicity = GetAssociationEndMember(relationshipEntry).RelationshipMultiplicity;
                        if (multiplicity == RelationshipMultiplicity.Many)
                        {
                            relationshipEntry.DetachRelationshipEntry();
                        }
                    }
                }
            }
        }

        private void FixupRelationships()
        {
            var relationshipManager = _wrappedEntity.RelationshipManager;
            Debug.Assert(relationshipManager != null, "Entity wrapper returned a null RelationshipManager");
            relationshipManager.RemoveEntityFromRelationships();
            DeleteRelationshipsThatReferenceKeys(null, null);
        }

        // <summary>
        // see if there are any relationship entries that point to key entries
        // if there are, remove the relationship entry
        // This is called when one of the ends of a relationship is being removed
        // </summary>
        // <param name="relationshipSet"> An option relationshipSet; deletes only relationships that are part of this set </param>
        internal void DeleteRelationshipsThatReferenceKeys(RelationshipSet relationshipSet, RelationshipEndMember endMember)
        {
            if (State != EntityState.Detached)
            {
                // devnote: Need to use a copy of the relationships list because we may be deleting Added
                //          relationships, which will be removed from the list while we are still iterating
                foreach (var relationshipEntry in _cache.CopyOfRelationshipsByKey(EntityKey))
                {
                    // Only delete the relationship entry if it is not already deleted (in which case we cannot access its values)
                    // and when the given (optionally) relationshipSet matches the one in teh relationship entry
                    if ((relationshipEntry.State != EntityState.Deleted)
                        &&
                        (relationshipSet == null || relationshipSet == relationshipEntry.EntitySet))
                    {
                        var otherEnd = GetOtherEndOfRelationship(relationshipEntry);
                        if (endMember == null
                            || endMember == otherEnd.GetAssociationEndMember(relationshipEntry))
                        {
                            for (var i = 0; i < 2; i++)
                            {
                                var entityKey = relationshipEntry.GetCurrentRelationValue(i) as EntityKey;
                                if ((object)entityKey != null)
                                {
                                    var relatedEntry = _cache.GetEntityEntry(entityKey);
                                    if (relatedEntry.IsKeyEntry)
                                    {
                                        // remove the relationshipEntry
                                        relationshipEntry.Delete(false);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Retrieve referential constraint properties from Principal entities (possibly recursively)
        // and check referential constraint properties in the Dependent entities (1 level only)
        // This code does not check the constraints on FKs because that work is instead done by
        // the FK fixup code that is also called from AcceptChanges.
        // Returns true if any FK relationships were skipped so that they can be checked again after fixup
        private bool RetrieveAndCheckReferentialConstraintValuesInAcceptChanges()
        {
            var relationshipManager = _wrappedEntity.RelationshipManager;
            Debug.Assert(relationshipManager != null, "Entity wrapper returned a null RelationshipManager");
            // Find key property names which are part of referential integrity constraints
            List<string> propertiesToRetrieve; // names of properties which should be retrieved from Principal entities
            bool propertiesToCheckExist; // true iff there are properties which should be checked in dependent entities

            // Get RI property names from metadata
            var skippedFKs = relationshipManager.FindNamesOfReferentialConstraintProperties(
                out propertiesToRetrieve, out propertiesToCheckExist, skipFK: true);

            // Do not try to retrieve RI properties if entity doesn't participate in any RI Constraints
            if (propertiesToRetrieve != null)
            {
                // Retrieve key values from related entities
                Dictionary<string, KeyValuePair<object, IntBox>> properties;

                // Create HashSet to store references to already visited entities, used to detect circular references
                var visited = new HashSet<object>();

                relationshipManager.RetrieveReferentialConstraintProperties(out properties, visited, includeOwnValues: false);

                // Update properties
                foreach (var pair in properties)
                {
                    SetCurrentEntityValue(pair.Key /*name*/, pair.Value.Key /*value*/);
                }
            }

            if (propertiesToCheckExist)
            {
                // Compare properties of current entity with properties of the dependent entities
                CheckReferentialConstraintPropertiesInDependents();
            }
            return skippedFKs;
        }

        internal void RetrieveReferentialConstraintPropertiesFromKeyEntries(Dictionary<string, KeyValuePair<object, IntBox>> properties)
        {
            string thisRole;
            AssociationSet association;

            // Iterate through related relationship entries
            foreach (var relationshipEntry in _cache.FindRelationshipsByKey(EntityKey))
            {
                var otherEnd = GetOtherEndOfRelationship(relationshipEntry);

                // We only try to retrieve properties from key entries
                if (otherEnd.IsKeyEntry)
                {
                    association = (AssociationSet)relationshipEntry.EntitySet;
                    Debug.Assert(association != null, "relationship is not an association");

                    // Iterate through referential constraints of the association of the relationship
                    // NOTE PERFORMANCE This collection in current stack can have 0 or 1 elements
                    foreach (var constraint in association.ElementType.ReferentialConstraints)
                    {
                        thisRole = GetAssociationEndMember(relationshipEntry).Name;

                        // Check if curent entry is a dependent end of the referential constraint
                        if (constraint.ToRole.Name == thisRole)
                        {
                            Debug.Assert(!otherEnd.EntityKey.IsTemporary, "key of key entry can't be temporary");
                            IList<EntityKeyMember> otherEndKeyValues = otherEnd.EntityKey.EntityKeyValues;
                            Debug.Assert(otherEndKeyValues != null, "key entry must have key values");

                            // NOTE PERFORMANCE Number of key properties is supposed to be "small"
                            foreach (var pair in otherEndKeyValues)
                            {
                                for (var i = 0; i < constraint.FromProperties.Count; ++i)
                                {
                                    if (constraint.FromProperties[i].Name == pair.Key)
                                    {
                                        AddOrIncreaseCounter(constraint, properties, constraint.ToProperties[i].Name, pair.Value);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static void AddOrIncreaseCounter(
            ReferentialConstraint constraint,
            Dictionary<string, KeyValuePair<object, IntBox>> properties,
            string propertyName, 
            object propertyValue)
        {
            DebugCheck.NotNull(constraint);
            DebugCheck.NotNull(properties);
            DebugCheck.NotNull(propertyName);
            DebugCheck.NotNull(propertyValue);

            if (properties.ContainsKey(propertyName))
            {
                // If this property already exists in the dictionary, check if value is the same then increase the counter
                var valueCounterPair = properties[propertyName];

                if (!ByValueEqualityComparer.Default.Equals(valueCounterPair.Key, propertyValue))
                {
                    throw new InvalidOperationException(constraint.BuildConstraintExceptionMessage());
                }

                valueCounterPair.Value.Value = valueCounterPair.Value.Value + 1;
            }
            else
            {
                // If property doesn't exist in the dictionary - add new entry with pair<value, counter>
                properties[propertyName] = new KeyValuePair<object, IntBox>(propertyValue, new IntBox(1));
            }
        }

        // Check if related dependent entities contain proper property values
        // Only entities in Unchanged and Modified state are checked (including KeyEntries)
        private void CheckReferentialConstraintPropertiesInDependents()
        {
            string thisRole;
            AssociationSet association;

            // Iterate through related relationship entries
            foreach (var relationshipEntry in _cache.FindRelationshipsByKey(EntityKey))
            {
                var otherEnd = GetOtherEndOfRelationship(relationshipEntry);

                // We only check entries which are in Unchanged or Modified state
                // (including KeyEntries which are always in Unchanged State)
                if (otherEnd.State == EntityState.Unchanged
                    || otherEnd.State == EntityState.Modified)
                {
                    association = (AssociationSet)relationshipEntry.EntitySet;
                    Debug.Assert(association != null, "relationship is not an association");

                    // Iterate through referential constraints of the association of the relationship
                    // NOTE PERFORMANCE This collection in current stack can have 0 or 1 elements
                    foreach (var constraint in association.ElementType.ReferentialConstraints)
                    {
                        thisRole = GetAssociationEndMember(relationshipEntry).Name;

                        // Check if curent entry is a principal end of the referential constraint
                        if (constraint.FromRole.Name == thisRole)
                        {
                            Debug.Assert(!otherEnd.EntityKey.IsTemporary, "key of Unchanged or Modified entry can't be temporary");
                            IList<EntityKeyMember> otherEndKeyValues = otherEnd.EntityKey.EntityKeyValues;
                            // NOTE PERFORMANCE Number of key properties is supposed to be "small"
                            foreach (var pair in otherEndKeyValues)
                            {
                                for (var i = 0; i < constraint.FromProperties.Count; ++i)
                                {
                                    if (constraint.ToProperties[i].Name == pair.Key)
                                    {
                                        if (!ByValueEqualityComparer.Default.Equals(
                                            GetCurrentEntityValue(constraint.FromProperties[i].Name), pair.Value))
                                        {
                                            throw new InvalidOperationException(constraint.BuildConstraintExceptionMessage());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void PromoteKeyEntry(IEntityWrapper wrappedEntity, StateManagerTypeMetadata typeMetadata)
        {
            DebugCheck.NotNull(wrappedEntity);
            DebugCheck.NotNull(wrappedEntity.Entity);
            DebugCheck.NotNull(typeMetadata);
            Debug.Assert(IsKeyEntry, "ObjectStateEntry should be a key.");

            _wrappedEntity = wrappedEntity;
            _wrappedEntity.ObjectStateEntry = this;

            // Allow updating of cached metadata because the actual entity might be a derived type
            _cacheTypeMetadata = typeMetadata;

            SetChangeTrackingFlags();
        }

        // <summary>
        // Turns this entry into a key entry (SPAN stub).
        // </summary>
        internal void DegradeEntry()
        {
            Debug.Assert(!IsKeyEntry);
            Debug.Assert((object)_entityKey != null);

            _entityKey = EntityKey; //Performs validation.

            RemoveFromForeignKeyIndex();

            _wrappedEntity.SetChangeTracker(null);

            _modifiedFields = null;
            _originalValues = null;
            _originalComplexObjects = null;

            // we don't want temporary keys to exist outside of the context
            if (State == EntityState.Added)
            {
                _wrappedEntity.EntityKey = null;
                _entityKey = null;
            }

            if (State != EntityState.Unchanged)
            {
                _cache.ChangeState(this, State, EntityState.Unchanged);
                State = EntityState.Unchanged;
            }

            _cache.RemoveEntryFromKeylessStore(_wrappedEntity);
            _wrappedEntity.DetachContext();
            _wrappedEntity.ObjectStateEntry = null;

            var degradedEntity = _wrappedEntity.Entity;
            _wrappedEntity = NullEntityWrapper.NullWrapper;

            SetChangeTrackingFlags();

            _cache.OnObjectStateManagerChanged(CollectionChangeAction.Remove, degradedEntity);

            Debug.Assert(IsKeyEntry);
        }

        internal void AttachObjectStateManagerToEntity()
        {
            // This method should only be called in cases where we really have an entity to attach to
            Debug.Assert(_wrappedEntity.Entity != null, "Cannot attach a null entity to the state manager");
            _wrappedEntity.SetChangeTracker(this);
            _wrappedEntity.TakeSnapshot(this);
        }

        // Get values of key properties which doesn't already exist in passed in 'properties'
        internal void GetOtherKeyProperties(Dictionary<string, KeyValuePair<object, IntBox>> properties)
        {
            DebugCheck.NotNull(properties);
            DebugCheck.NotNull(_cacheTypeMetadata);
            DebugCheck.NotNull(_cacheTypeMetadata.DataRecordInfo);
            DebugCheck.NotNull(_cacheTypeMetadata.DataRecordInfo.RecordType);

            var entityType = _cacheTypeMetadata.DataRecordInfo.RecordType.EdmType as EntityType;
            Debug.Assert(entityType != null, "EntityType == null");

            foreach (var member in entityType.KeyMembers)
            {
                if (!properties.ContainsKey(member.Name))
                {
                    properties[member.Name] = new KeyValuePair<object, IntBox>(GetCurrentEntityValue(member.Name), new IntBox(1));
                }
            }
        }

        internal void AddOriginalValueAt(int index, StateManagerMemberMetadata memberMetadata, object userObject, object value)
        {
            var stateManagerValue = new StateManagerValue(memberMetadata, userObject, value);
            
            if (index >= 0)
            {
                _originalValues[index] = stateManagerValue;
            }
            else
            {
                if (_originalValues == null)
                {
                    _originalValues = new List<StateManagerValue>();
                }
                _originalValues.Add(stateManagerValue);
            }
        }

        internal void CompareKeyProperties(object changed)
        {
            DebugCheck.NotNull(changed);
            Debug.Assert(!IsKeyEntry);

            var metadata = _cacheTypeMetadata;

            var fieldCount = GetFieldCount(metadata);
            object currentValueNew;
            object currentValueOld;

            for (var i = 0; i < fieldCount; i++)
            {
                var member = metadata.Member(i);
                if (member.IsPartOfKey)
                {
                    Debug.Assert(!member.IsComplex);

                    currentValueNew = member.GetValue(changed);
                    currentValueOld = member.GetValue(_wrappedEntity.Entity);

                    if (!ByValueEqualityComparer.Default.Equals(currentValueNew, currentValueOld))
                    {
                        throw new InvalidOperationException(Strings.ObjectStateEntry_CannotModifyKeyProperty(member.CLayerName));
                    }
                }
            }
        }

        // helper method used to get value of property
        internal object GetCurrentEntityValue(string memberName)
        {
            var ordinal = _cacheTypeMetadata.GetOrdinalforOLayerMemberName(memberName);
            return GetCurrentEntityValue(_cacheTypeMetadata, ordinal, _wrappedEntity.Entity, ObjectStateValueRecord.CurrentUpdatable);
        }

        // <summary>
        // Verifies that the property with the given ordinal is editable.
        // </summary>
        // <exception cref="InvalidOperationException">the property is not editable</exception>
        internal void VerifyEntityValueIsEditable(StateManagerTypeMetadata typeMetadata, int ordinal, string memberName)
        {
            DebugCheck.NotNull(typeMetadata);

            if (State == EntityState.Deleted)
            {
                throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyDetachedDeletedEntries);
            }

            var member = typeMetadata.Member(ordinal);

            Debug.Assert(member != null, "Member shouldn't be null.");

            // Key fields are only editable if the entry is the Added state.
            if (member.IsPartOfKey
                && State != EntityState.Added)
            {
                throw new InvalidOperationException(Strings.ObjectStateEntry_CannotModifyKeyProperty(memberName));
            }
        }

        // This API are mainly for DbDataRecord implementations to get and set the values
        // also for loadoptions, setoldvalue will be used.
        // we should handle just for C-space, we will not recieve a call from O-space for set
        // We will not also return any value in term of O-Layer. all set and gets for us is in terms of C-layer.
        // the only O-layer interaction we have is through delegates from entity.
        internal void SetCurrentEntityValue(StateManagerTypeMetadata metadata, int ordinal, object userObject, object newValue)
        {
            // required to validate state because entity could be detatched from this context and added to another context
            // and we want this to fail instead of setting the value which would redirect to the other context
            ValidateState();

            var member = metadata.Member(ordinal);
            Debug.Assert(member != null, "StateManagerMemberMetadata was not found for the given ordinal.");

            if (member.IsComplex)
            {
                if (newValue == null
                    || newValue == DBNull.Value)
                {
                    throw new InvalidOperationException(Strings.ComplexObject_NullableComplexTypesNotSupported(member.CLayerName));
                }

                var newValueRecord = newValue as IExtendedDataRecord;
                if (newValueRecord == null)
                {
                    throw new ArgumentException(Strings.ObjectStateEntry_InvalidTypeForComplexTypeProperty, "newValue");
                }

                newValue = _cache.ComplexTypeMaterializer.CreateComplex(newValueRecord, newValueRecord.DataRecordInfo, null);
            }

            _wrappedEntity.SetCurrentValue(this, member, ordinal, userObject, newValue);
        }

        private void TransitionRelationshipsForAdd()
        {
            foreach (var relationshipEntry in _cache.CopyOfRelationshipsByKey(EntityKey))
            {
                // Unchanged -> Added
                if (relationshipEntry.State
                    == EntityState.Unchanged)
                {
                    ObjectStateManager.ChangeState(relationshipEntry, EntityState.Unchanged, EntityState.Added);
                    relationshipEntry.State = EntityState.Added;
                }
                // Deleted -> Detached
                else if (relationshipEntry.State
                         == EntityState.Deleted)
                {
                    // Remove key entry if necessary
                    relationshipEntry.DeleteUnnecessaryKeyEntries();
                    // Remove relationship entry
                    relationshipEntry.DetachRelationshipEntry();
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Conditional("DEBUG")]
        private void VerifyIsNotRelated()
        {
            Debug.Assert(!IsKeyEntry, "shouldn't be called for a key entry");

            WrappedEntity.RelationshipManager.VerifyIsNotRelated();
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal void ChangeObjectState(EntityState requestedState)
        {
            if (IsKeyEntry)
            {
                if (requestedState == EntityState.Unchanged)
                {
                    return; // No-op
                }
                throw new InvalidOperationException(Strings.ObjectStateEntry_CannotModifyKeyEntryState);
            }

            switch (State)
            {
                case EntityState.Added:
                    switch (requestedState)
                    {
                        case EntityState.Added:
                            // Relationship fixup: Unchanged -> Added,  Deleted -> Detached
                            TransitionRelationshipsForAdd();
                            break;
                        case EntityState.Unchanged:
                            // Relationship fixup: none
                            AcceptChanges();
                            break;
                        case EntityState.Modified:
                            // Relationship fixup: none
                            AcceptChanges();
                            SetModified();
                            SetModifiedAll();
                            break;
                        case EntityState.Deleted:
                            // Need to forget conceptual nulls so that AcceptChanges does not throw.
                            // Note that there should always be no conceptual nulls left when we get into the Deleted state.
                            _cache.ForgetEntryWithConceptualNull(this, resetAllKeys: true);
                            // Relationship fixup: Added -> Detached, Unchanged -> Deleted
                            AcceptChanges();
                            // NOTE: OSM.TransactionManager.IsLocalPublicAPI == true so cascade delete and RIC are disabled
                            Delete(true);
                            break;
                        case EntityState.Detached:
                            // Relationship fixup: * -> Detached
                            Detach();
                            break;
                        default:
                            throw new ArgumentException(Strings.ObjectContext_InvalidEntityState, "requestedState");
                    }
                    break;
                case EntityState.Unchanged:
                    switch (requestedState)
                    {
                        case EntityState.Added:
                            ObjectStateManager.ReplaceKeyWithTemporaryKey(this);
                            _modifiedFields = null;
                            _originalValues = null;
                            _originalComplexObjects = null;
                            State = EntityState.Added;
                            // Relationship fixup: Unchanged -> Added,  Deleted -> Detached
                            TransitionRelationshipsForAdd();
                            break;
                        case EntityState.Unchanged:
                            // Relationship fixup: none
                            break;
                        case EntityState.Modified:
                            // Relationship fixup: none
                            SetModified();
                            SetModifiedAll();
                            break;
                        case EntityState.Deleted:
                            // Relationship fixup: Added -> Detached,  Unchanged -> Deleted
                            // NOTE: OSM.TransactionManager.IsLocalPublicAPI == true so cascade delete and RIC are disabled
                            Delete(true);
                            break;
                        case EntityState.Detached:
                            // Relationship fixup: * -> Detached
                            Detach();
                            break;
                        default:
                            throw new ArgumentException(Strings.ObjectContext_InvalidEntityState, "requestedState");
                    }
                    break;
                case EntityState.Modified:
                    switch (requestedState)
                    {
                        case EntityState.Added:
                            ObjectStateManager.ReplaceKeyWithTemporaryKey(this);
                            _modifiedFields = null;
                            _originalValues = null;
                            _originalComplexObjects = null;
                            State = EntityState.Added;
                            // Relationship fixup: Unchanged -> Added,  Deleted -> Detached
                            TransitionRelationshipsForAdd();
                            break;
                        case EntityState.Unchanged:
                            AcceptChanges();
                            // Relationship fixup: none
                            break;
                        case EntityState.Modified:
                            // Relationship fixup: none
                            SetModified();
                            SetModifiedAll();
                            break;
                        case EntityState.Deleted:
                            // Relationship fixup: Added -> Detached,  Unchanged -> Deleted
                            // NOTE: OSM.TransactionManager.IsLocalPublicAPI == true so cascade delete and RIC are disabled
                            Delete(true);
                            break;
                        case EntityState.Detached:
                            // Relationship fixup: * -> Detached
                            Detach();
                            break;
                        default:
                            throw new ArgumentException(Strings.ObjectContext_InvalidEntityState, "requestedState");
                    }
                    break;
                case EntityState.Deleted:
                    switch (requestedState)
                    {
                        case EntityState.Added:
                            // Throw if the entry has some not-Deleted relationships
                            VerifyIsNotRelated();
                            TransitionRelationshipsForAdd();
                            ObjectStateManager.ReplaceKeyWithTemporaryKey(this);
                            _modifiedFields = null;
                            _originalValues = null;
                            _originalComplexObjects = null;
                            State = EntityState.Added;
                            _cache.FixupReferencesByForeignKeys(this); // Make sure refs based on FK values are set
                            _cache.OnObjectStateManagerChanged(CollectionChangeAction.Add, Entity);
                            break;
                        case EntityState.Unchanged:
                            // Throw if the entry has some not-Deleted relationship
                            VerifyIsNotRelated();
                            _modifiedFields = null;
                            _originalValues = null;
                            _originalComplexObjects = null;

                            ObjectStateManager.ChangeState(this, EntityState.Deleted, EntityState.Unchanged);
                            State = EntityState.Unchanged;

                            _wrappedEntity.TakeSnapshot(this); // refresh snapshot

                            _cache.FixupReferencesByForeignKeys(this); // Make sure refs based on FK values are set
                            _cache.OnObjectStateManagerChanged(CollectionChangeAction.Add, Entity);

                            // Relationship fixup: none
                            break;
                        case EntityState.Modified:
                            // Throw if the entry has some not-Deleted relationship
                            VerifyIsNotRelated();
                            // Relationship fixup: none
                            ObjectStateManager.ChangeState(this, EntityState.Deleted, EntityState.Modified);
                            State = EntityState.Modified;
                            SetModifiedAll();

                            _cache.FixupReferencesByForeignKeys(this); // Make sure refs based on FK values are set
                            _cache.OnObjectStateManagerChanged(CollectionChangeAction.Add, Entity);

                            break;
                        case EntityState.Deleted:
                            // No-op
                            break;
                        case EntityState.Detached:
                            // Relationship fixup: * -> Detached
                            Detach();
                            break;
                        default:
                            throw new ArgumentException(Strings.ObjectContext_InvalidEntityState, "requestedState");
                    }
                    break;
                case EntityState.Detached:
                    Debug.Fail("detached entry");
                    break;
            }
        }

        internal void UpdateOriginalValues(object entity)
        {
            Debug.Assert(EntityState.Added != State, "Cannot change original values of an entity in the Added state");

            var oldState = State;

            UpdateRecordWithSetModified(entity, EditableOriginalValues);

            if (oldState == EntityState.Unchanged
                && State == EntityState.Modified)
            {
                // The UpdateRecord changes state but doesn't update ObjectStateManager's dictionaries.
                ObjectStateManager.ChangeState(this, oldState, EntityState.Modified);
            }
        }

        internal void UpdateRecordWithoutSetModified(object value, DbUpdatableDataRecord current)
        {
            UpdateRecord(value, current, UpdateRecordBehavior.WithoutSetModified, s_EntityRoot);
        }

        internal void UpdateRecordWithSetModified(object value, DbUpdatableDataRecord current)
        {
            UpdateRecord(value, current, UpdateRecordBehavior.WithSetModified, s_EntityRoot);
        }

        private enum UpdateRecordBehavior
        {
            WithoutSetModified,
            WithSetModified
        }

        internal const int s_EntityRoot = -1;

        private void UpdateRecord(object value, DbUpdatableDataRecord current, UpdateRecordBehavior behavior, int propertyIndex)
        {
            DebugCheck.NotNull(value);
            DebugCheck.NotNull(current);
            Debug.Assert(!(value is IEntityWrapper));
            Debug.Assert(
                propertyIndex == s_EntityRoot ||
                propertyIndex >= 0, "Unexpected index. Use -1 if the passed value is an entity, not a complex type object");

            // get Metadata for type
            var typeMetadata = current._metadata;
            var recordInfo = typeMetadata.DataRecordInfo;

            foreach (var field in recordInfo.FieldMetadata)
            {
                var index = field.Ordinal;

                var member = typeMetadata.Member(index);
                var fieldValue = member.GetValue(value) ?? DBNull.Value;

                if (Helper.IsComplexType(field.FieldType.TypeUsage.EdmType))
                {
                    var existing = current.GetValue(index);
                    // Ensure that the existing ComplexType value is not null. This is not supported.
                    if (existing == DBNull.Value)
                    {
                        throw new InvalidOperationException(Strings.ComplexObject_NullableComplexTypesNotSupported(field.FieldType.Name));
                    }
                    else if (fieldValue != DBNull.Value)
                    {
                        // There is both an IExtendedDataRecord and an existing CurrentValueRecord

                        // This part is different than Shaper.UpdateRecord - we have to remember the name of property on the entity (for complex types)
                        // For property of a complex type the rootCLayerName is CLayerName of the complex property on the entity.
                        UpdateRecord(
                            fieldValue, (DbUpdatableDataRecord)existing,
                            behavior,
                            propertyIndex == s_EntityRoot ? index : propertyIndex);
                    }
                }
                else
                {
                    Debug.Assert(Helper.IsScalarType(field.FieldType.TypeUsage.EdmType), "Expected primitive or enum type.");

                    // Set the new value if it doesn't match the existing value or if the field is modified, not a primary key, and
                    // this entity has a conceptual null, since setting the field may then clear the conceptual null--see 640443.
                    if (HasRecordValueChanged(current, index, fieldValue)
                        && !member.IsPartOfKey)
                    {
                        current.SetValue(index, fieldValue);

                        if (behavior == UpdateRecordBehavior.WithSetModified)
                        {
                            // This part is different than Shaper.UpdateRecord - we have to mark the field as modified.
                            // For property of a complex type the rootCLayerName is CLayerName of the complex property on the entity.
                            SetModifiedPropertyInternal(propertyIndex == s_EntityRoot ? index : propertyIndex);
                        }
                    }
                }
            }
        }

        internal bool HasRecordValueChanged(DbDataRecord record, int propertyIndex, object newFieldValue)
        {
            var existing = record.GetValue(propertyIndex);
            return (existing != newFieldValue) &&
                   ((DBNull.Value == newFieldValue) ||
                    (DBNull.Value == existing) ||
                    (!ByValueEqualityComparer.Default.Equals(existing, newFieldValue))) ||
                   (_cache.EntryHasConceptualNull(this) && _modifiedFields != null && _modifiedFields[propertyIndex]);
        }

        internal void ApplyCurrentValuesInternal(IEntityWrapper wrappedCurrentEntity)
        {
            DebugCheck.NotNull(wrappedCurrentEntity);
            Debug.Assert(!IsKeyEntry, "Cannot apply values to a key KeyEntry.");

            if (State != EntityState.Modified
                && State != EntityState.Unchanged)
            {
                throw new InvalidOperationException(Strings.ObjectContext_EntityMustBeUnchangedOrModified(State.ToString()));
            }

            if (WrappedEntity.IdentityType != wrappedCurrentEntity.IdentityType)
            {
                throw new ArgumentException(
                    Strings.ObjectContext_EntitiesHaveDifferentType(
                        Entity.GetType().FullName, wrappedCurrentEntity.Entity.GetType().FullName));
            }

            CompareKeyProperties(wrappedCurrentEntity.Entity);

            UpdateCurrentValueRecord(wrappedCurrentEntity.Entity);
        }

        internal void UpdateCurrentValueRecord(object value)
        {
            Debug.Assert(!(value is IEntityWrapper));
            _wrappedEntity.UpdateCurrentValueRecord(value, this);
        }

        internal void ApplyOriginalValuesInternal(IEntityWrapper wrappedOriginalEntity)
        {
            DebugCheck.NotNull(wrappedOriginalEntity);
            Debug.Assert(!IsKeyEntry, "Cannot apply values to a key KeyEntry.");

            if (State != EntityState.Modified
                && State != EntityState.Unchanged
                && State != EntityState.Deleted)
            {
                throw new InvalidOperationException(Strings.ObjectContext_EntityMustBeUnchangedOrModifiedOrDeleted(State.ToString()));
            }

            if (WrappedEntity.IdentityType != wrappedOriginalEntity.IdentityType)
            {
                throw new ArgumentException(
                    Strings.ObjectContext_EntitiesHaveDifferentType(
                        Entity.GetType().FullName, wrappedOriginalEntity.Entity.GetType().FullName));
            }

            CompareKeyProperties(wrappedOriginalEntity.Entity);

            // The ObjectStateEntry.UpdateModifiedFields uses a variation of Shaper.UpdateRecord method 
            // which additionaly marks properties as modified as necessary.
            UpdateOriginalValues(wrappedOriginalEntity.Entity);
        }

        // <summary>
        // For each FK contained in this entry, the entry is removed from the index maintained by
        // the ObjectStateManager for that key.
        // </summary>
        internal void RemoveFromForeignKeyIndex()
        {
            if (!IsKeyEntry)
            {
                foreach (var relatedEnd in FindFKRelatedEnds())
                {
                    foreach (var foreignKey in relatedEnd.GetAllKeyValues())
                    {
                        _cache.RemoveEntryFromForeignKeyIndex(relatedEnd, foreignKey, this);
                    }
                }
                _cache.AssertEntryDoesNotExistInForeignKeyIndex(this);
            }
        }

        // <summary>
        // Looks at the foreign keys contained in this entry and performs fixup to the entities that
        // they reference, or adds the key and this entry to the index of foreign keys that reference
        // entities that we don't yet know about.
        // </summary>
        internal void FixupReferencesByForeignKeys(bool replaceAddedRefs, EntitySetBase restrictTo = null)
        {
            Debug.Assert(_cache != null, "Attempt to fixup detached entity entry");
            _cache.TransactionManager.BeginGraphUpdate();
            var setIsLoaded = !(_cache.TransactionManager.IsAttachTracking || _cache.TransactionManager.IsAddTracking);
            try
            {
                foreach (var dependent in ForeignKeyDependents
                    .Where(t => restrictTo == null
                        || t.Item1.SourceSet.Identity == restrictTo.Identity
                        || t.Item1.TargetSet.Identity == restrictTo.Identity))
                {
                    var relatedEnd = WrappedEntity.RelationshipManager.GetRelatedEndInternal(
                        dependent.Item1.ElementType, (AssociationEndMember)dependent.Item2.FromRole) as EntityReference;

                    Debug.Assert(relatedEnd != null, "Expected non-null EntityReference to principal.");

                    // Prevent fixup using values that are effectively null but aren't nullable.
                    if (!ForeignKeyFactory.IsConceptualNullKey(relatedEnd.CachedForeignKey))
                    {
                        FixupEntityReferenceToPrincipal(relatedEnd, null, setIsLoaded, replaceAddedRefs);
                    }
                }
            }
            finally
            {
                _cache.TransactionManager.EndGraphUpdate();
            }
        }

        internal void FixupEntityReferenceByForeignKey(EntityReference reference)
        {
            // The FK is changing, so the reference is no longer loaded from the store, even if we do fixup
            reference.IsLoaded = false;

            // Remove the existing CachedForeignKey
            var hasConceptualNullFk = ForeignKeyFactory.IsConceptualNullKey(reference.CachedForeignKey);
            if (hasConceptualNullFk)
            {
                ObjectStateManager.ForgetEntryWithConceptualNull(this, resetAllKeys: false);
            }

            var existingPrincipal = reference.ReferenceValue;
            var foreignKey = ForeignKeyFactory.CreateKeyFromForeignKeyValues(this, reference);

            // Check if the new FK matches the key of the entity already at the principal end.
            // If it does, then don't change the ref.
            bool needToSetRef;
            if ((object)foreignKey == null
                || existingPrincipal.Entity == null)
            {
                needToSetRef = true;
            }
            else
            {
                var existingPrincipalKey = existingPrincipal.EntityKey;
                var existingPrincipalEntry = existingPrincipal.ObjectStateEntry;
                // existingPrincipalKey may be null if this fixup code is being called in the middle of
                // adding an object.  This can happen when using change tracking proxies with fixup.
                if ((existingPrincipalKey == null || existingPrincipalKey.IsTemporary)
                    && existingPrincipalEntry != null)
                {
                    // Build a temporary non-temp key for the added entity so we can see if it matches the new FK
                    existingPrincipalKey = new EntityKey((EntitySet)existingPrincipalEntry.EntitySet, existingPrincipalEntry.CurrentValues);
                }

                // If existingPrincipalKey is still a temp key here, then the equality check will fail
                needToSetRef = !foreignKey.Equals(existingPrincipalKey);
            }

            if (_cache.TransactionManager.RelationshipBeingUpdated != reference)
            {
                if (needToSetRef)
                {
                    _cache.TransactionManager.BeginGraphUpdate();
                    // Keep track of this entity so that we don't try to delete/detach the entity while we're
                    // working with it.  This allows the FK to be set to some value without that entity being detached.
                    // However, if the FK is being set to null, then for an identifying relationship we will detach.
                    if ((object)foreignKey != null)
                    {
                        _cache.TransactionManager.EntityBeingReparented = Entity;
                    }
                    try
                    {
                        FixupEntityReferenceToPrincipal(reference, foreignKey, setIsLoaded: false, replaceExistingRef: true);
                    }
                    finally
                    {
                        Debug.Assert(_cache != null, "Unexpected null state manager.");
                        _cache.TransactionManager.EntityBeingReparented = null;
                        _cache.TransactionManager.EndGraphUpdate();
                    }
                }
            }
            else
            {
                // We only want to update the CachedForeignKey and not touch the EntityReference.Value/EntityKey
                FixupEntityReferenceToPrincipal(reference, foreignKey, setIsLoaded: false, replaceExistingRef: false);
            }
        }

        // <summary>
        // Given a RelatedEnd that represents a FK from this dependent entity to the principal entity of the
        // relationship, this method fixes up references between the two entities.
        // </summary>
        // <param name="relatedEnd"> Represents a FK relationship to a principal </param>
        // <param name="foreignKey"> The foreign key, if it has already been computed </param>
        // <param name="setIsLoaded"> If true, then the IsLoaded flag for the relationship is set </param>
        // <param name="replaceExistingRef"> If true, then any existing references will be replaced </param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal void FixupEntityReferenceToPrincipal(
            EntityReference relatedEnd, EntityKey foreignKey, bool setIsLoaded, bool replaceExistingRef)
        {
            DebugCheck.NotNull(relatedEnd);
            if (foreignKey == null)
            {
                foreignKey = ForeignKeyFactory.CreateKeyFromForeignKeyValues(this, relatedEnd);
            }
            // Note that if we're not changing FKs directly, but rather as a result of fixup after a ref has changed,
            // and if the entity currently being pointed to is Added, then we shouldn't clobber it, because a ref to
            // an Added entity wins in this case.
            var canModifyReference = _cache.TransactionManager.RelationshipBeingUpdated != relatedEnd &&
                                     (!_cache.TransactionManager.IsForeignKeyUpdate ||
                                      relatedEnd.ReferenceValue.ObjectStateEntry == null ||
                                      relatedEnd.ReferenceValue.ObjectStateEntry.State != EntityState.Added);

            // Note that the code below has evolved to what it is now and could possibly be refactored to
            // simplify the logic.
            relatedEnd.SetCachedForeignKey(foreignKey, this);
            ObjectStateManager.ForgetEntryWithConceptualNull(this, resetAllKeys: false);
            if (foreignKey != null) // Implies no value is null or CreateKeyFromForeignKeyValues would have returned null
            {
                // Lookup key in OSM.  If found, then we can do fixup.  If not, then need to add to index
                // Should not overwrite a reference at this point since this might cause the graph to
                // be shredded.  This allows us to correctly detect key violations or RIC violations later.
                EntityEntry principalEntry;
                if (_cache.TryGetEntityEntry(foreignKey, out principalEntry)
                    &&
                    !principalEntry.IsKeyEntry
                    &&
                    principalEntry.State != EntityState.Deleted
                    &&
                    (replaceExistingRef || WillNotRefSteal(relatedEnd, principalEntry.WrappedEntity))
                    && relatedEnd.CanSetEntityType(principalEntry.WrappedEntity))
                {
                    if (canModifyReference)
                    {
                        // We add both sides to the promoted EntityKeyRefs collection because it could be the dependent or
                        // the principal or both that are being added.  Having extra members in this index doesn't hurt.
                        if (_cache.TransactionManager.PopulatedEntityReferences != null)
                        {
                            Debug.Assert(
                                _cache.TransactionManager.IsAddTracking || _cache.TransactionManager.IsAttachTracking,
                                "PromotedEntityKeyRefs is non-null while not tracking add or attach");
                            _cache.TransactionManager.PopulatedEntityReferences.Add(relatedEnd);
                        }

                        // Set the EntityKey on the RelatedEnd--this will cause the reference to be set and fixup to happen.
                        relatedEnd.SetEntityKey(foreignKey, forceFixup: true);

                        if (_cache.TransactionManager.PopulatedEntityReferences != null)
                        {
                            var otherEnd = relatedEnd.GetOtherEndOfRelationship(principalEntry.WrappedEntity) as EntityReference;
                            if (otherEnd != null)
                            {
                                _cache.TransactionManager.PopulatedEntityReferences.Add(otherEnd);
                            }
                        }
                    }
                    if (setIsLoaded && principalEntry.State != EntityState.Added)
                    {
                        relatedEnd.IsLoaded = true;
                    }
                }
                else
                {
                    // Add an entry to the index for later fixup
                    _cache.AddEntryContainingForeignKeyToIndex(relatedEnd, foreignKey, this);
                    if (canModifyReference
                        && replaceExistingRef
                        && relatedEnd.ReferenceValue.Entity != null)
                    {
                        relatedEnd.ReferenceValue = NullEntityWrapper.NullWrapper;
                    }
                }
            }
            else if (canModifyReference)
            {
                if (replaceExistingRef && (relatedEnd.ReferenceValue.Entity != null || relatedEnd.EntityKey != null))
                {
                    relatedEnd.ReferenceValue = NullEntityWrapper.NullWrapper;
                }
                if (setIsLoaded)
                {
                    // This is the case where a query comes from the database with a null FK value.
                    // We know that there is no related entity in the database and therefore the entity on the
                    // other end of the relationship is as loaded as it is possible to be.  Therefore, we
                    // set the IsLoaded flag so that if a user asks we will tell them that (based on last known
                    // state of the database) there is no need to do a load.
                    relatedEnd.IsLoaded = true;
                }
            }
        }

        // <summary>
        // Determins whether or not setting a reference will cause implicit ref stealing as part of FK fixup.
        // If it would, then an exception is thrown.  If it would not and we can safely overwrite the existing
        // value, then true is returned.  If it would not but we should not overwrite the existing value,
        // then false is returned.
        // </summary>
        private static bool WillNotRefSteal(EntityReference refToPrincipal, IEntityWrapper wrappedPrincipal)
        {
            var dependentEnd = refToPrincipal.GetOtherEndOfRelationship(wrappedPrincipal);
            var refToDependent = dependentEnd as EntityReference;
            if ((refToPrincipal.ReferenceValue.Entity == null && refToPrincipal.NavigationPropertyIsNullOrMissing())
                &&
                (refToDependent == null
                 || (refToDependent.ReferenceValue.Entity == null && refToDependent.NavigationPropertyIsNullOrMissing())))
            {
                // Return true if the ref to principal is null and it's not 1:1 or it is 1:1 and the ref to dependent is also null.
                return true;
            }
            else if (refToDependent != null
                     &&
                     (ReferenceEquals(refToDependent.ReferenceValue.Entity, refToPrincipal.WrappedOwner.Entity) ||
                      refToDependent.CheckIfNavigationPropertyContainsEntity(refToPrincipal.WrappedOwner)))
            {
                return true;
            }
            else if (refToDependent == null
                     ||
                     ReferenceEquals(refToPrincipal.ReferenceValue.Entity, wrappedPrincipal.Entity)
                     ||
                     refToPrincipal.CheckIfNavigationPropertyContainsEntity(wrappedPrincipal))
            {
                // Return false if the ref to principal is non-null and it's not 1:1
                return false;
            }
            else
            {
                // Else it is 1:1 and one side or the other is non-null => reference steal!
                throw new InvalidOperationException(
                    Strings.EntityReference_CannotAddMoreThanOneEntityToEntityReference(
                        refToDependent.RelationshipNavigation.To, refToDependent.RelationshipNavigation.RelationshipName));
            }
        }

        // <summary>
        // Given that this entry represents an entity on the dependent side of a FK, this method attempts to return the key of the
        // entity on the principal side of the FK.  If the two entities both exist in the context, then the primary key of
        // the principal entity is found and returned.  If the principal entity does not exist in the context, then a key
        // for it is built up from the foreign key values contained in the dependent entity.
        // </summary>
        // <param name="principalRole"> The role indicating the FK to navigate </param>
        // <param name="principalKey"> Set to the principal key or null on return </param>
        // <returns> True if the principal key was found or built; false if it could not be found or built </returns>
        internal bool TryGetReferenceKey(AssociationEndMember principalRole, out EntityKey principalKey)
        {
            var relatedEnd = RelationshipManager.GetRelatedEnd(principalRole.DeclaringType.FullName, principalRole.Name) as EntityReference;
            Debug.Assert(relatedEnd != null, "Expected there to be a non null EntityReference to the principal");
            if (relatedEnd.CachedValue.Entity == null
                || relatedEnd.CachedValue.ObjectStateEntry == null)
            {
                principalKey = null;
                return false;
            }
            principalKey = relatedEnd.EntityKey ?? relatedEnd.CachedValue.ObjectStateEntry.EntityKey;
            return principalKey != null;
        }

        // <summary>
        // Performs fixuyup of foreign keys based on referencesd between objects.  This should only be called
        // for Added objects since this is the only time that references take precedence over FKs in fixup.
        // </summary>
        internal void FixupForeignKeysByReference()
        {
            Debug.Assert(_cache != null, "Attempt to fixup detached entity entry");
            _cache.TransactionManager.BeginFixupKeysByReference();
            try
            {
                FixupForeignKeysByReference(null);
            }
            finally
            {
                _cache.TransactionManager.EndFixupKeysByReference();
            }
        }

        // <summary>
        // Fixup the FKs by the current reference values
        // Do this in the order of fixing up values from the principal ends first, and then propogate those values to the dependents
        // </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void FixupForeignKeysByReference(List<EntityEntry> visited)
        {
            var entitySet = EntitySet as EntitySet;

            // Perf optimization to avoid all this work if the entity doesn't participate in any FK relationships
            if (!entitySet.HasForeignKeyRelationships)
            {
                return;
            }

            foreach (var dependent in ForeignKeyDependents)
            {
                // Added dependent.  Make sure we traverse all the way to the top-most principal before beginging fixup.
                var reference =
                    RelationshipManager.GetRelatedEndInternal(dependent.Item1.ElementType.FullName, dependent.Item2.FromRole.Name) as
                    EntityReference;
                Debug.Assert(reference != null, "Expected reference to exist and be an entity reference (not collection)");
                var existingPrincipal = reference.ReferenceValue;
                if (existingPrincipal.Entity != null)
                {
                    var principalEntry = existingPrincipal.ObjectStateEntry;
                    bool? isOneToMany = null;
                    if (principalEntry != null
                        && principalEntry.State == EntityState.Added
                        &&
                        (principalEntry != this
                         || (isOneToMany = reference.GetOtherEndOfRelationship(existingPrincipal) is EntityReference).Value))
                    {
                        visited = visited ?? new List<EntityEntry>();
                        if (visited.Contains(this))
                        {
                            if (!isOneToMany.HasValue)
                            {
                                isOneToMany = reference.GetOtherEndOfRelationship(existingPrincipal) is EntityReference;
                            }
                            if (isOneToMany.Value)
                            {
                                // Cycles in constraints are dissallowed except for 1:* self references
                                throw new InvalidOperationException(
                                    Strings.RelationshipManager_CircularRelationshipsWithReferentialConstraints);
                            }
                        }
                        else
                        {
                            visited.Add(this);
                            principalEntry.FixupForeignKeysByReference(visited);
                            visited.Remove(this);
                        }
                    }
                    // "forceChange" is false because we don't want to actually set the property values
                    // here if they are aready set to the same thing--we don't want the events and setting
                    // the modified flag is irrelavent during AcceptChanges.
                    reference.UpdateForeignKeyValues(WrappedEntity, existingPrincipal, changedFKs: null, forceChange: false);
                }
                else
                {
                    var principalKey = reference.EntityKey;
                    if (principalKey != null
                        && !principalKey.IsTemporary)
                    {
                        reference.UpdateForeignKeyValues(WrappedEntity, principalKey);
                    }
                }
            }

            foreach (var principal in ForeignKeyPrincipals)
            {
                // Added prinipal end.  Fixup FKs on all dependents.
                // This is necessary because of the case where a PK in an added entity is changed after it and its dependnents
                // are added to the context--see bug 628752.
                var fkOverlapsPk = false; // Set to true if we find out that the FK overlaps the dependent PK
                var dependentPropsChecked = false; // Set to true once we have checked whether or not the FK overlaps the PK
                var principalEnd = RelationshipManager.GetRelatedEndInternal(
                    principal.Item1.ElementType.FullName, principal.Item2.ToRole.Name);
                foreach (var dependent in principalEnd.GetWrappedEntities())
                {
                    var dependentEntry = dependent.ObjectStateEntry;
                    Debug.Assert(dependentEntry != null, "Should have fully tracked graph at this point.");
                    if (dependentEntry.State != EntityState.Added
                        && !dependentPropsChecked)
                    {
                        dependentPropsChecked = true;
                        foreach (var dependentProp in principal.Item2.ToProperties)
                        {
                            var dependentOrdinal = dependentEntry._cacheTypeMetadata.GetOrdinalforOLayerMemberName(dependentProp.Name);
                            var member = dependentEntry._cacheTypeMetadata.Member(dependentOrdinal);
                            if (member.IsPartOfKey)
                            {
                                // If the FK overlpas the PK then we can't set it for non-Added entities.
                                // In this situation we just continue with the next one and if the conflict
                                // may then be flagged later as a RIC check.
                                fkOverlapsPk = true;
                                break;
                            }
                        }
                    }
                    // This code relies on the fact that a dependent referenced to an Added principal must be either Added or
                    // Modified since we cannpt trust thestate of the principal PK and therefore the dependent FK must also
                    // be considered not completely trusted--it may need to be updated.
                    if (dependentEntry.State == EntityState.Added
                        || (dependentEntry.State == EntityState.Modified && !fkOverlapsPk))
                    {
                        var principalRef = principalEnd.GetOtherEndOfRelationship(dependent) as EntityReference;
                        Debug.Assert(principalRef != null, "Expected reference to exist and be an entity reference (not collection)");
                        // "forceChange" is false because we don't want to actually set the property values
                        // here if they are aready set to the same thing--we don't want the events and setting
                        // the modified flag is irrelavent during AcceptChanges.
                        principalRef.UpdateForeignKeyValues(dependent, WrappedEntity, changedFKs: null, forceChange: false);
                    }
                }
            }
        }

        private bool IsPropertyAForeignKey(string propertyName)
        {
            foreach (var dependent in ForeignKeyDependents)
            {
                foreach (var property in dependent.Item2.ToProperties)
                {
                    if (property.Name == propertyName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsPropertyAForeignKey(string propertyName, out List<Pair<string, string>> relationships)
        {
            relationships = null;

            foreach (var dependent in ForeignKeyDependents)
            {
                foreach (var property in dependent.Item2.ToProperties)
                {
                    if (property.Name == propertyName)
                    {
                        if (relationships == null)
                        {
                            relationships = new List<Pair<string, string>>();
                        }
                        relationships.Add(new Pair<string, string>(dependent.Item1.ElementType.FullName, dependent.Item2.FromRole.Name));
                        break;
                    }
                }
            }

            return relationships != null;
        }

        internal void FindRelatedEntityKeysByForeignKeys(
            out Dictionary<RelatedEnd, HashSet<EntityKey>> relatedEntities,
            bool useOriginalValues)
        {
            relatedEntities = null;

            foreach (var dependent in ForeignKeyDependents)
            {
                var associationSet = dependent.Item1;
                var constraint = dependent.Item2;
                // Get association end members for the dependent and the principal ends
                var dependentId = constraint.ToRole.Identity;
                var setEnds = associationSet.AssociationSetEnds;
                Debug.Assert(associationSet.AssociationSetEnds.Count == 2, "Expected an association set with only two ends.");
                AssociationEndMember principalEnd;
                if (setEnds[0].CorrespondingAssociationEndMember.Identity == dependentId)
                {
                    principalEnd = setEnds[1].CorrespondingAssociationEndMember;
                }
                else
                {
                    principalEnd = setEnds[0].CorrespondingAssociationEndMember;
                }

                var principalEntitySet = MetadataHelper.GetEntitySetAtEnd(associationSet, principalEnd);
                var foreignKey = ForeignKeyFactory.CreateKeyFromForeignKeyValues(this, constraint, principalEntitySet, useOriginalValues);
                if (foreignKey != null) // Implies no value is null or CreateKeyFromForeignKeyValues would have returned null
                {
                    var reference = RelationshipManager.GetRelatedEndInternal(
                        associationSet.ElementType, (AssociationEndMember)constraint.FromRole) as EntityReference;

                    // only for deleted relationships the hashset can have > 1 elements
                    HashSet<EntityKey> entityKeys;
                    relatedEntities = relatedEntities != null ? relatedEntities : new Dictionary<RelatedEnd, HashSet<EntityKey>>();
                    if (!relatedEntities.TryGetValue(reference, out entityKeys))
                    {
                        entityKeys = new HashSet<EntityKey>();
                        relatedEntities.Add(reference, entityKeys);
                    }
                    entityKeys.Add(foreignKey);
                }
            }
        }

        // <summary>
        // Returns a list of all RelatedEnds for this entity
        // that are the dependent end of an FK Association
        // </summary>
        internal IEnumerable<EntityReference> FindFKRelatedEnds()
        {
            var relatedEnds = new HashSet<EntityReference>();

            foreach (var dependent in ForeignKeyDependents)
            {
                var reference = RelationshipManager.GetRelatedEndInternal(
                    dependent.Item1.ElementType.FullName, dependent.Item2.FromRole.Name) as EntityReference;
                relatedEnds.Add(reference);
            }
            return relatedEnds;
        }

        // <summary>
        // Identifies any changes in FK's and creates entries in;
        // - TransactionManager.AddedRelationshipsByForeignKey
        // - TransactionManager.DeletedRelationshipsByForeignKey
        // If the FK change will result in fix-up then two entries
        // are added to TransactionManager.AddedRelationshipsByForeignKey
        // (one for each direction of the new realtionship)
        // </summary>
        internal void DetectChangesInForeignKeys()
        {
            //DetectChangesInProperties should already have marked this entity as dirty
            Debug.Assert(State == EntityState.Added || State == EntityState.Modified, "unexpected state");

            //We are going to be adding data to the TransactionManager
            var tm = ObjectStateManager.TransactionManager;

            foreach (var entityReference in FindFKRelatedEnds())
            {
                var currentKey = ForeignKeyFactory.CreateKeyFromForeignKeyValues(this, entityReference);
                var originalKey = entityReference.CachedForeignKey;
                var originalKeyIsConceptualNull = ForeignKeyFactory.IsConceptualNullKey(originalKey);

                //If both keys are null there is nothing to check
                if (originalKey != null
                    || currentKey != null)
                {
                    if (originalKey == null)
                    {
                        //If original is null then we are just adding a relationship
                        EntityEntry entry;
                        ObjectStateManager.TryGetEntityEntry(currentKey, out entry);
                        AddRelationshipDetectedByForeignKey(
                            tm.AddedRelationshipsByForeignKey, tm.AddedRelationshipsByPrincipalKey, currentKey, entry, entityReference);
                    }
                    else if (currentKey == null)
                    {
                        //If current is null we are just deleting a relationship
                        Debug.Assert(!originalKeyIsConceptualNull, "If FK is nullable there shouldn't be a conceptual null set");
                        AddDetectedRelationship(tm.DeletedRelationshipsByForeignKey, originalKey, entityReference);
                    }
                    //If there is a Conceptual Null set we need to check if the current values
                    //are different from the values when the Conceptual Null was created
                    else if (!currentKey.Equals(originalKey)
                             && (!originalKeyIsConceptualNull || ForeignKeyFactory.IsConceptualNullKeyChanged(originalKey, currentKey)))
                    {
                        //If keys don't match then we are always adding
                        EntityEntry entry;
                        ObjectStateManager.TryGetEntityEntry(currentKey, out entry);
                        AddRelationshipDetectedByForeignKey(
                            tm.AddedRelationshipsByForeignKey, tm.AddedRelationshipsByPrincipalKey, currentKey, entry, entityReference);

                        //And if the original key wasn't a conceptual null we are also deleting
                        if (!originalKeyIsConceptualNull)
                        {
                            AddDetectedRelationship(tm.DeletedRelationshipsByForeignKey, originalKey, entityReference);
                        }
                    }
                }
            }
        }

        // <summary>
        // True if the underlying entity is not capable of tracking changes to complex types such that
        // DetectChanges is required to do this.
        // </summary>
        internal bool RequiresComplexChangeTracking
        {
            get { return _requiresComplexChangeTracking; }
        }

        // <summary>
        // True if the underlying entity is not capable of tracking changes to scalars such that
        // DetectChanges is required to do this.
        // </summary>
        internal bool RequiresScalarChangeTracking
        {
            get { return _requiresScalarChangeTracking; }
        }

        // <summary>
        // True if the underlying entity is not capable of performing full change tracking such that
        // it must be considered by at least some parts of DetectChanges.
        // </summary>
        internal bool RequiresAnyChangeTracking
        {
            get { return _requiresAnyChangeTracking; }
        }
    }
}
