// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    internal sealed class RelationshipEntry : ObjectStateEntry
    {
        internal RelationshipWrapper _relationshipWrapper;

        internal EntityKey Key0
        {
            get { return RelationshipWrapper.Key0; }
        }

        internal EntityKey Key1
        {
            get { return RelationshipWrapper.Key1; }
        }

        internal override BitArray ModifiedProperties
        {
            get { return null; }
        }

        #region Linked list of related relationships

        #endregion

        #region Constructors

        internal RelationshipEntry(ObjectStateManager cache, EntityState state, RelationshipWrapper relationshipWrapper)
            : base(cache, null, state)
        {
            DebugCheck.NotNull(relationshipWrapper);
            Debug.Assert(
                EntityState.Added == state ||
                EntityState.Unchanged == state ||
                EntityState.Deleted == state,
                "invalid EntityState");

            base._entitySet = relationshipWrapper.AssociationSet;
            _relationshipWrapper = relationshipWrapper;
        }

        #endregion

        #region Public members

        /// <summary>
        ///     API to accept the current values as original values and  mark the entity as Unchanged.
        /// </summary>
        /// <param> </param>
        /// <returns> </returns>
        public override bool IsRelationship
        {
            get
            {
                ValidateState();
                return true;
            }
        }

        public override void AcceptChanges()
        {
            ValidateState();

            switch (State)
            {
                case EntityState.Deleted:
                    DeleteUnnecessaryKeyEntries();
                    // Current entry could be already detached if this is relationship entry and if one end of relationship was a KeyEntry
                    if (_cache != null)
                    {
                        _cache.ChangeState(this, EntityState.Deleted, EntityState.Detached);
                    }
                    break;
                case EntityState.Added:
                    _cache.ChangeState(this, EntityState.Added, EntityState.Unchanged);
                    State = EntityState.Unchanged;
                    break;
                case EntityState.Modified:
                    Debug.Assert(false, "RelationshipEntry cannot be in Modified state");
                    break;
                case EntityState.Unchanged:
                    break;
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

        public override IEnumerable<string> GetModifiedProperties()
        {
            ValidateState();
            yield break;
        }

        public override void SetModified()
        {
            ValidateState();
            throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationState);
        }

        public override object Entity
        {
            get
            {
                ValidateState();
                return null;
            }
        }

        public override EntityKey EntityKey
        {
            get
            {
                ValidateState();
                return null;
            }
            internal set
            {
                // no-op for entires other than EntityEntry
                Debug.Assert(false, "EntityKey setter shouldn't be called for RelationshipEntry");
            }
        }

        /// <summary>
        ///     Marks specified property as modified.
        /// </summary>
        /// <param name="propertyName"> This API recognizes the names in terms of OSpace </param>
        /// <exception cref="InvalidOperationException">If State is not Modified or Unchanged</exception>
        public override void SetModifiedProperty(string propertyName)
        {
            ValidateState();

            throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationState);
        }

        /// <summary>
        ///     Throws since the method has no meaning for relationship entries.
        /// </summary>
        public override void RejectPropertyChanges(string propertyName)
        {
            ValidateState();

            throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationState);
        }

        /// <summary>
        ///     Throws since the method has no meaning for relationship entries.
        /// </summary>
        public override bool IsPropertyChanged(string propertyName)
        {
            ValidateState();

            throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationState);
        }

        /// <summary>
        ///     Original values
        /// </summary>
        /// <param> </param>
        /// <returns> DbDataRecord </returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public override DbDataRecord OriginalValues
        {
            get
            {
                ValidateState();
                if (State == EntityState.Added)
                {
                    throw new InvalidOperationException(Strings.ObjectStateEntry_OriginalValuesDoesNotExist);
                }

                return new ObjectStateEntryDbDataRecord(this);
            }
        }

        public override OriginalValueRecord GetUpdatableOriginalValues()
        {
            throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationValues);
        }

        /// <summary>
        ///     Current values
        /// </summary>
        /// <param> </param>
        /// <returns> DbUpdatableDataRecord </returns>
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

                return new ObjectStateEntryDbUpdatableDataRecord(this);
            }
        }

        public override RelationshipManager RelationshipManager
        {
            get { throw new InvalidOperationException(Strings.ObjectStateEntry_RelationshipAndKeyEntriesDoNotHaveRelationshipManagers); }
        }

        public override void ChangeState(EntityState state)
        {
            EntityUtil.CheckValidStateForChangeRelationshipState(state, "state");

            if (State == EntityState.Detached
                && state == EntityState.Detached)
            {
                return;
            }

            ValidateState();

            if (RelationshipWrapper.Key0 == Key0)
            {
                ObjectStateManager.ChangeRelationshipState(
                    Key0, Key1,
                    RelationshipWrapper.AssociationSet.ElementType.FullName,
                    RelationshipWrapper.AssociationEndMembers[1].Name,
                    state);
            }
            else
            {
                Debug.Assert(RelationshipWrapper.Key0 == Key1, "invalid relationship");
                ObjectStateManager.ChangeRelationshipState(
                    Key0, Key1,
                    RelationshipWrapper.AssociationSet.ElementType.FullName,
                    RelationshipWrapper.AssociationEndMembers[0].Name,
                    state);
            }
        }

        public override void ApplyCurrentValues(object currentEntity)
        {
            Check.NotNull(currentEntity, "currentEntity");

            throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationValues);
        }

        public override void ApplyOriginalValues(object originalEntity)
        {
            Check.NotNull(originalEntity, "originalEntity");

            throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationValues);
        }

        #endregion

        #region ObjectStateEntry members

        internal override bool IsKeyEntry
        {
            get { return false; }
        }

        internal override int GetFieldCount(StateManagerTypeMetadata metadata)
        {
            return _relationshipWrapper.AssociationEndMembers.Count;
        }

        /// <summary>
        ///     Reuse or create a new (Entity)DataRecordInfo.
        /// </summary>
        internal override DataRecordInfo GetDataRecordInfo(StateManagerTypeMetadata metadata, object userObject)
        {
            //Dev Note: RelationshipType always has default facets. Thus its safe to construct a TypeUsage from EdmType
            return new DataRecordInfo(TypeUsage.Create(((RelationshipSet)EntitySet).ElementType));
        }

        internal override void SetModifiedAll()
        {
            ValidateState();
            throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationState);
        }

        internal override Type GetFieldType(int ordinal, StateManagerTypeMetadata metadata)
        {
            // 'metadata' is used for ComplexTypes in EntityEntry

            return typeof(EntityKey); // this is given By Design
        }

        internal override string GetCLayerName(int ordinal, StateManagerTypeMetadata metadata)
        {
            ValidateRelationshipRange(ordinal);
            return _relationshipWrapper.AssociationEndMembers[ordinal].Name;
        }

        internal override int GetOrdinalforCLayerName(string name, StateManagerTypeMetadata metadata)
        {
            AssociationEndMember endMember;
            var endMembers = _relationshipWrapper.AssociationEndMembers;
            if (endMembers.TryGetValue(name, false, out endMember))
            {
                return endMembers.IndexOf(endMember);
            }
            return -1;
        }

        internal override void RevertDelete()
        {
            State = EntityState.Unchanged;
            _cache.ChangeState(this, EntityState.Deleted, State);
        }

        /// <summary>
        ///     Used to report that a scalar entity property is about to change
        ///     The current value of the specified property is cached when this method is called.
        /// </summary>
        /// <param name="entityMemberName"> The name of the entity property that is changing </param>
        internal override void EntityMemberChanging(string entityMemberName)
        {
            throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationValues);
        }

        /// <summary>
        ///     Used to report that a scalar entity property has been changed
        ///     The property value that was cached during EntityMemberChanging is now
        ///     added to OriginalValues
        /// </summary>
        /// <param name="entityMemberName"> The name of the entity property that has changing </param>
        internal override void EntityMemberChanged(string entityMemberName)
        {
            throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationValues);
        }

        /// <summary>
        ///     Used to report that a complex property is about to change
        ///     The current value of the specified property is cached when this method is called.
        /// </summary>
        /// <param name="entityMemberName"> The name of the top-level entity property that is changing </param>
        /// <param name="complexObject"> The complex object that contains the property that is changing </param>
        /// <param name="complexObjectMemberName"> The name of the property that is changing on complexObject </param>
        internal override void EntityComplexMemberChanging(string entityMemberName, object complexObject, string complexObjectMemberName)
        {
            DebugCheck.NotEmpty(entityMemberName);
            DebugCheck.NotNull(complexObject);
            DebugCheck.NotEmpty(complexObjectMemberName);

            throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationValues);
        }

        /// <summary>
        ///     Used to report that a complex property has been changed
        ///     The property value that was cached during EntityMemberChanging is now added to OriginalValues
        /// </summary>
        /// <param name="entityMemberName"> The name of the top-level entity property that has changed </param>
        /// <param name="complexObject"> The complex object that contains the property that changed </param>
        /// <param name="complexObjectMemberName"> The name of the property that changed on complexObject </param>
        internal override void EntityComplexMemberChanged(string entityMemberName, object complexObject, string complexObjectMemberName)
        {
            DebugCheck.NotEmpty(entityMemberName);
            DebugCheck.NotNull(complexObject);
            DebugCheck.NotEmpty(complexObjectMemberName);

            throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationValues);
        }

        #endregion

        // Helper method to determine if the specified entityKey is in the given role and AssociationSet in this relationship entry
        internal bool IsSameAssociationSetAndRole(
            AssociationSet associationSet, AssociationEndMember associationMember, EntityKey entityKey)
        {
            Debug.Assert(
                associationSet.ElementType.AssociationEndMembers[0].Name == associationMember.Name ||
                associationSet.ElementType.AssociationEndMembers[1].Name == associationMember.Name,
                "Expected associationMember to be one of the ends of the specified associationSet.");

            if (!ReferenceEquals(_entitySet, associationSet))
            {
                return false;
            }

            // Find the end of the relationship that corresponds to the associationMember and see if it matches the EntityKey we are looking for
            if (_relationshipWrapper.AssociationSet.ElementType.AssociationEndMembers[0].Name
                == associationMember.Name)
            {
                return entityKey == Key0;
            }
            else
            {
                return entityKey == Key1;
            }
        }

        private object GetCurrentRelationValue(int ordinal, bool throwException)
        {
            ValidateRelationshipRange(ordinal);
            ValidateState();
            if (State == EntityState.Deleted && throwException)
            {
                throw new InvalidOperationException(Strings.ObjectStateEntry_CurrentValuesDoesNotExist);
            }
            return _relationshipWrapper.GetEntityKey(ordinal);
        }

        private static void ValidateRelationshipRange(int ordinal)
        {
            if (unchecked(1u < (uint)ordinal))
            {
                throw new ArgumentOutOfRangeException("ordinal");
            }
        }

        internal object GetCurrentRelationValue(int ordinal)
        {
            return GetCurrentRelationValue(ordinal, true);
        }

        internal RelationshipWrapper RelationshipWrapper
        {
            get { return _relationshipWrapper; }
            set
            {
                DebugCheck.NotNull(value);
                _relationshipWrapper = value;
            }
        }

        internal override void Reset()
        {
            _relationshipWrapper = null;

            base.Reset();
        }

        /// <summary>
        ///     Update one of the ends of the relationship
        /// </summary>
        internal void ChangeRelatedEnd(EntityKey oldKey, EntityKey newKey)
        {
            if (oldKey.Equals(Key0))
            {
                if (oldKey.Equals(Key1))
                {
                    // self-reference
                    RelationshipWrapper = new RelationshipWrapper(RelationshipWrapper.AssociationSet, newKey);
                }
                else
                {
                    RelationshipWrapper = new RelationshipWrapper(RelationshipWrapper, 0, newKey);
                }
            }
            else
            {
                RelationshipWrapper = new RelationshipWrapper(RelationshipWrapper, 1, newKey);
            }
        }

        internal void DeleteUnnecessaryKeyEntries()
        {
            // We need to check to see if the ends of the relationship are key entries.
            // If they are, and nothing else refers to them then the key entry should be removed.
            for (var i = 0; i < 2; i++)
            {
                var entityKey = GetCurrentRelationValue(i, false) as EntityKey;
                var relatedEntry = _cache.GetEntityEntry(entityKey);
                if (relatedEntry.IsKeyEntry)
                {
                    var foundRelationship = false;
                    // count the number of relationships this key entry is part of
                    // if there aren't any, then the relationship should be deleted
                    foreach (var relationshipEntry in _cache.FindRelationshipsByKey(entityKey))
                    {
                        // only count relationships that are not the one we are currently deleting (i.e. this)
                        if (relationshipEntry != this)
                        {
                            foundRelationship = true;
                            break;
                        }
                    }
                    if (!foundRelationship)
                    {
                        // Nothing is refering to this key entry, so it should be removed from the cache
                        _cache.DeleteKeyEntry(relatedEntry);
                        // We assume that only one end of relationship can be a key entry,
                        // so we can break the loop
                        break;
                    }
                }
            }
        }

        //"doFixup" equals to False is called from EntityCollection & Ref code only
        internal void Delete(bool doFixup)
        {
            ValidateState();

            if (doFixup)
            {
                if (State != EntityState.Deleted) //for deleted ObjectStateEntry its a no-op
                {
                    //Find two ends of the relationship
                    var entry1 = _cache.GetEntityEntry((EntityKey)GetCurrentRelationValue(0));
                    var wrappedEntity1 = entry1.WrappedEntity;
                    var entry2 = _cache.GetEntityEntry((EntityKey)GetCurrentRelationValue(1));
                    var wrappedEntity2 = entry2.WrappedEntity;

                    // If one end of the relationship is a KeyEntry, entity1 or entity2 is null.
                    // It is not possible that both ends of relationship are KeyEntries.
                    if (wrappedEntity1.Entity != null
                        && wrappedEntity2.Entity != null)
                    {
                        // Obtain the ro role name and relationship name
                        // We don't create a full NavigationRelationship here because that would require looking up
                        // additional information like property names that we don't need.
                        var endMembers = _relationshipWrapper.AssociationEndMembers;
                        var toRole = endMembers[1].Name;
                        var relationshipName = ((AssociationSet)_entitySet).ElementType.FullName;
                        wrappedEntity1.RelationshipManager.RemoveEntity(toRole, relationshipName, wrappedEntity2);
                    }
                    else
                    {
                        // One end of relationship is a KeyEntry, figure out which one is the real entity and get its RelationshipManager
                        // so we can update the DetachedEntityKey on the EntityReference associated with this relationship
                        EntityKey targetKey = null;
                        RelationshipManager relationshipManager = null;
                        if (wrappedEntity1.Entity == null)
                        {
                            targetKey = entry1.EntityKey;
                            relationshipManager = wrappedEntity2.RelationshipManager;
                        }
                        else
                        {
                            targetKey = entry2.EntityKey;
                            relationshipManager = wrappedEntity1.RelationshipManager;
                        }
                        Debug.Assert(relationshipManager != null, "Entity wrapper returned a null RelationshipManager");

                        // Clear the detachedEntityKey as well. In cases where we have to fix up the detachedEntityKey, we will not always be able to detect
                        // if we have *only* a Deleted relationship for a given entity/relationship/role, so clearing this here will ensure that
                        // even if no other relationships are added, the key value will still be correct and we won't accidentally pick up an old value.

                        // devnote: Since we know the target end of this relationship is a key entry, it has to be a reference, so just cast
                        var targetMember = RelationshipWrapper.GetAssociationEndMember(targetKey);
                        var entityReference =
                            (EntityReference)
                            relationshipManager.GetRelatedEndInternal(targetMember.DeclaringType.FullName, targetMember.Name);
                        entityReference.DetachedEntityKey = null;

                        // Now update the state
                        if (State == EntityState.Added)
                        {
                            // Remove key entry if necessary
                            DeleteUnnecessaryKeyEntries();
                            // Remove relationship entry
                            // devnote: Using this method instead of just changing the state because the entry
                            //          may have already been detached along with the key entry above. However,
                            //          if there were other relationships using the key, it would not have been deleted.
                            DetachRelationshipEntry();
                        }
                        else
                        {
                            // Non-added entries should be deleted
                            _cache.ChangeState(this, State, EntityState.Deleted);
                            State = EntityState.Deleted;
                        }
                    }
                }
            }
            else
            {
                switch (State)
                {
                    case EntityState.Added:
                        // Remove key entry if necessary
                        DeleteUnnecessaryKeyEntries();
                        // Remove relationship entry
                        // devnote: Using this method instead of just changing the state because the entry
                        //          may have already been detached along with the key entry above. However,
                        //          if there were other relationships using the key, it would not have been deleted.
                        DetachRelationshipEntry();
                        break;
                    case EntityState.Modified:
                        Debug.Assert(false, "RelationshipEntry cannot be in Modified state");
                        break;
                    case EntityState.Unchanged:
                        _cache.ChangeState(this, EntityState.Unchanged, EntityState.Deleted);
                        State = EntityState.Deleted;
                        break;
                        //case DataRowState.Deleted:  no-op
                }
            }
        }

        internal object GetOriginalRelationValue(int ordinal)
        {
            return GetCurrentRelationValue(ordinal, false);
        }

        internal void DetachRelationshipEntry()
        {
            // no-op if already detached
            if (_cache != null)
            {
                _cache.ChangeState(this, State, EntityState.Detached);
            }
        }

        internal void ChangeRelationshipState(EntityEntry targetEntry, RelatedEnd relatedEnd, EntityState requestedState)
        {
            Debug.Assert(requestedState != EntityState.Modified, "Invalid requested state for relationsihp");
            Debug.Assert(State != EntityState.Modified, "Invalid initial state for relationsihp");

            var initialState = State;

            switch (initialState)
            {
                case EntityState.Added:
                    switch (requestedState)
                    {
                        case EntityState.Added:
                            // no-op
                            break;
                        case EntityState.Unchanged:
                            AcceptChanges();
                            break;
                        case EntityState.Deleted:
                            AcceptChanges();
                            // cascade deletion is not performed because TransactionManager.IsLocalPublicAPI == true
                            Delete();
                            break;
                        case EntityState.Detached:
                            // cascade deletion is not performed because TransactionManager.IsLocalPublicAPI == true
                            Delete();
                            break;
                        default:
                            Debug.Assert(false, "Invalid requested state");
                            break;
                    }
                    break;
                case EntityState.Unchanged:
                    switch (requestedState)
                    {
                        case EntityState.Added:
                            ObjectStateManager.ChangeState(this, EntityState.Unchanged, EntityState.Added);
                            State = EntityState.Added;
                            break;
                        case EntityState.Unchanged:
                            //no-op
                            break;
                        case EntityState.Deleted:
                            // cascade deletion is not performed because TransactionManager.IsLocalPublicAPI == true
                            Delete();
                            break;
                        case EntityState.Detached:
                            // cascade deletion is not performed because TransactionManager.IsLocalPublicAPI == true
                            Delete();
                            AcceptChanges();
                            break;
                        default:
                            Debug.Assert(false, "Invalid requested state");
                            break;
                    }
                    break;
                case EntityState.Deleted:
                    switch (requestedState)
                    {
                        case EntityState.Added:
                            relatedEnd.Add(
                                targetEntry.WrappedEntity,
                                applyConstraints: true,
                                addRelationshipAsUnchanged: false,
                                relationshipAlreadyExists: true,
                                allowModifyingOtherEndOfRelationship: false,
                                forceForeignKeyChanges: true);
                            ObjectStateManager.ChangeState(this, EntityState.Deleted, EntityState.Added);
                            State = EntityState.Added;
                            break;
                        case EntityState.Unchanged:
                            relatedEnd.Add(
                                targetEntry.WrappedEntity,
                                applyConstraints: true,
                                addRelationshipAsUnchanged: false,
                                relationshipAlreadyExists: true,
                                allowModifyingOtherEndOfRelationship: false,
                                forceForeignKeyChanges: true);
                            ObjectStateManager.ChangeState(this, EntityState.Deleted, EntityState.Unchanged);
                            State = EntityState.Unchanged;
                            break;
                        case EntityState.Deleted:
                            // no-op
                            break;
                        case EntityState.Detached:
                            AcceptChanges();
                            break;
                        default:
                            Debug.Assert(false, "Invalid requested state");
                            break;
                    }
                    break;
                default:
                    Debug.Assert(false, "Invalid entry state");
                    break;
            }
        }

        #region RelationshipEnds as singly-linked list

        internal RelationshipEntry GetNextRelationshipEnd(EntityKey entityKey)
        {
            DebugCheck.NotNull((object)entityKey);
            Debug.Assert(entityKey.Equals(Key0) || entityKey.Equals(Key1), "EntityKey mismatch");
            return (entityKey.Equals(Key0) ? NextKey0 : NextKey1);
        }

        internal void SetNextRelationshipEnd(EntityKey entityKey, RelationshipEntry nextEnd)
        {
            DebugCheck.NotNull((object)entityKey);
            Debug.Assert(entityKey.Equals(Key0) || entityKey.Equals(Key1), "EntityKey mismatch");
            if (entityKey.Equals(Key0))
            {
                NextKey0 = nextEnd;
            }
            else
            {
                NextKey1 = nextEnd;
            }
        }

        /// <summary>
        ///     Use when EntityEntry.EntityKey == this.Wrapper.Key0
        /// </summary>
        internal RelationshipEntry NextKey0 { get; set; }

        /// <summary>
        ///     Use when EntityEntry.EntityKey == this.Wrapper.Key1
        /// </summary>
        internal RelationshipEntry NextKey1 { get; set; }

        #endregion
    }
}
