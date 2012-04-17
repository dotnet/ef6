namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Models a relationship end with multiplicity 1.
    /// </summary>
    [DataContract]
    [Serializable]
    public abstract class EntityReference : RelatedEnd
    {
        // ------
        // Fields
        // ------

        // The following fields are serialized.  Adding or removing a serialized field is considered
        // a breaking change.  This includes changing the field type or field name of existing
        // serialized fields. If you need to make this kind of change, it may be possible, but it
        // will require some custom serialization/deserialization code.

        // The following field is valid only for detached EntityReferences, see EntityKey property for more details.
        private EntityKey _detachedEntityKey;

        // The following field is used to cache the FK value to the principal for FK relationships.
        // It is okay to not serialize this field because it is only used when the entity is tracked.
        // For a detached entity it can always be null and cause no problems.
        [NonSerialized]
        private EntityKey _cachedForeignKey;

        // ------------
        // Constructors
        // ------------

        /// <summary>
        /// The default constructor is required for some serialization scenarios. It should not be used to 
        /// create new EntityReferences. Use the GetRelatedReference or GetRelatedEnd methods on the RelationshipManager
        /// class instead.
        /// </summary>
        internal EntityReference()
        {
        }

        internal EntityReference(IEntityWrapper wrappedOwner, RelationshipNavigation navigation, IRelationshipFixer relationshipFixer)
            : base(wrappedOwner, navigation, relationshipFixer)
        {
        }

        /// <summary>
        /// Returns the EntityKey of the target entity associated with this EntityReference.
        /// 
        /// Is non-null in the following scenarios:
        /// (a) Entities are tracked by a context and an Unchanged or Added client-side relationships exists for this EntityReference's owner with the
        ///     same RelationshipName and source role. This relationship could have been created explicitly by the user (e.g. by setting
        ///     the EntityReference.Value, setting this property directly, or by calling EntityCollection.Add) or automatically through span queries.
        /// (b) If the EntityKey was non-null before detaching an entity from the context, it will still be non-null after detaching, until any operation
        ///     occurs that would set it to null, as described below.
        /// (c) Entities are detached and the EntityKey is explicitly set to non-null by the user.
        /// (d) Entity graph was created using a NoTracking query with full span
        /// 
        /// Is null in the following scenarios:
        /// (a) Entities are tracked by a context but there is no Unchanged or Added client-side relationship for this EntityReference's owner with the
        ///     same RelationshipName and source role.
        /// (b) Entities are tracked by a context and a relationship exists, but the target entity has a temporary key (i.e. it is Added) or the key
        ///     is one of the special keys
        /// (c) Entities are detached and the relationship was explicitly created by the user.
        /// </summary>   
        [DataMember]
        public EntityKey EntityKey
        {
            // This is the only scenario where it is valid to have a null Owner, so don't check it
            get
            {
                if (ObjectContext != null
                    && !UsingNoTracking)
                {
                    Debug.Assert(WrappedOwner.Entity != null, "Unexpected null Owner on EntityReference attached to a context");

                    EntityKey attachedKey = null;

                    // If this EntityReference contains an entity, look up the key on that object
                    if (CachedValue.Entity != null)
                    {
                        // While processing an attach the owner may have a context while the target does not.  This means
                        // that the target may gave an entity but not yet have an attached entity key.
                        attachedKey = CachedValue.EntityKey;
                        if (attachedKey != null
                            && !IsValidEntityKeyType(attachedKey))
                        {
                            // don't return temporary or special keys from this property
                            attachedKey = null;
                        }
                    }
                    else
                    {
                        if (IsForeignKey)
                        {
                            // For dependent ends, return the value of the cached foreign key if it is not conceptually null
                            if (IsDependentEndOfReferentialConstraint(false)
                                && _cachedForeignKey != null)
                            {
                                if (!ForeignKeyFactory.IsConceptualNullKey(_cachedForeignKey))
                                {
                                    attachedKey = _cachedForeignKey;
                                }
                            }
                            else
                            {
                                // Principal ends or ends that haven't been fixed up yet (i.e during Add/Attach) should use the DetachedEntityKey value
                                // that contains the last known value that was set
                                attachedKey = DetachedEntityKey;
                            }
                        }
                        else
                        {
                            // There could still be an Added or Unchanged relationship with a stub entry
                            var ownerKey = WrappedOwner.EntityKey;
                            foreach (var relationshipEntry in ObjectContext.ObjectStateManager.FindRelationshipsByKey(ownerKey))
                            {
                                // We only care about the relationships that match the AssociationSet and source role for the owner of this EntityReference
                                if (relationshipEntry.State != EntityState.Deleted
                                    &&
                                    relationshipEntry.IsSameAssociationSetAndRole(
                                        (AssociationSet)RelationshipSet, (AssociationEndMember)FromEndProperty, ownerKey))
                                {
                                    Debug.Assert(
                                        attachedKey == null,
                                        "Found more than one non-Deleted relationship for the same AssociationSet and source role");
                                    attachedKey = relationshipEntry.RelationshipWrapper.GetOtherEntityKey(ownerKey);
                                    // key should never be temporary or special since it came from a key entry
                                }
                            }
                        }
                    }
                    Debug.Assert(
                        attachedKey == null || IsValidEntityKeyType(attachedKey),
                        "Unexpected temporary or special key");
                    return attachedKey;
                }
                else
                {
                    return DetachedEntityKey;
                }
            }
            set { SetEntityKey(value, forceFixup: false); }
        }

        internal void SetEntityKey(EntityKey value, bool forceFixup)
        {
            if (value != null && value == EntityKey
                && (ReferenceValue.Entity != null || (ReferenceValue.Entity == null && !forceFixup)))
            {
                // "no-op" -- this is not really no-op in the attached case, because at a minimum we have to do a key lookup,
                // worst case we have to review all relationships for the owner entity
                // However, if we don't do this, we can get into a scenario where we are setting the key to the same thing it's already set to
                // and this could have side effects, especially with RI constraints and cascade delete. We don't want to delete something
                // and then add it back, if that deleting could have additional unexpected effects. Don't bother doing this check if value is
                // null, because EntityKey could be null even if there are Added/Unchanged relationships, if the target entity has a temporary key.
                // In that case, we still need to delete that existing relationship, so it's not a no-op
                return;
            }

            if (ObjectContext != null
                && !UsingNoTracking)
            {
                Debug.Assert(WrappedOwner.Entity != null, "Unexpected null Owner on EntityReference attached to a context");

                // null is a valid value for the EntityKey, but temporary and special keys are not    
                // devnote: Can't check this on detached references because this property could be set to a temp key during deserialization,
                //          if the key hasn't finished deserializing yet.
                if (value != null
                    && !IsValidEntityKeyType(value))
                {
                    throw new ArgumentException(Strings.EntityReference_CannotSetSpecialKeys, "value");
                }

                if (value == null)
                {
                    if (AttemptToNullFKsOnRefOrKeySetToNull())
                    {
                        DetachedEntityKey = null;
                    }
                    else
                    {
                        ReferenceValue = EntityWrapperFactory.NullWrapper;
                    }
                }
                else
                {
                    // Verify that the key has the right EntitySet for this RelationshipSet
                    var targetEntitySet = value.GetEntitySet(ObjectContext.MetadataWorkspace);
                    CheckRelationEntitySet(targetEntitySet);
                    value.ValidateEntityKey(ObjectContext.MetadataWorkspace, targetEntitySet, true /*isArgumentException */, "value");

                    var manager = ObjectContext.ObjectStateManager;

                    // If we already have an entry with this key, we just need to create a relationship with it
                    var addNewRelationship = false;
                    // If we don't already have any matching entries for this key, we'll have to create a new entry
                    var addKeyEntry = false;
                    var targetEntry = manager.FindEntityEntry(value);
                    if (targetEntry != null)
                    {
                        // If it's not a key entry, just use the entity to set this reference's Value
                        if (!targetEntry.IsKeyEntry)
                        {
                            // Delegate to the Value property to clear any existing relationship
                            // and to add the new one. This will fire the appropriate events and
                            // ensure that the related ends are connected.

                            // It has to be a TEntity since we already verified that the EntitySet is correct above
                            ReferenceValue = targetEntry.WrappedEntity;
                        }
                        else
                        {
                            // if the existing entry is a key entry, we just need to
                            // add a new relationship between the source entity and that key
                            addNewRelationship = true;
                        }
                    }
                    else
                    {
                        // no entry exists, so we'll need to add a key along with the relationship
                        addKeyEntry = !IsForeignKey;
                        addNewRelationship = true;
                    }

                    if (addNewRelationship)
                    {
                        var ownerKey = ValidateOwnerWithRIConstraints(
                            targetEntry == null ? null : targetEntry.WrappedEntity, value, checkBothEnds: true);

                        // Verify that the owner is in a valid state for adding a relationship
                        ValidateStateForAdd(WrappedOwner);

                        if (addKeyEntry)
                        {
                            manager.AddKeyEntry(value, targetEntitySet);
                        }

                        // First, clear any existing relationships
                        manager.TransactionManager.EntityBeingReparented = WrappedOwner.Entity;
                        try
                        {
                            ClearCollectionOrRef(null, null, /*doCascadeDelete*/ false);
                        }
                        finally
                        {
                            manager.TransactionManager.EntityBeingReparented = null;
                        }

                        // Then add the new one
                        if (IsForeignKey)
                        {
                            DetachedEntityKey = value;
                            // Update the FK values in this entity
                            if (IsDependentEndOfReferentialConstraint(false))
                            {
                                UpdateForeignKeyValues(WrappedOwner, value);
                            }
                        }
                        else
                        {
                            var wrapper = new RelationshipWrapper(
                                (AssociationSet)RelationshipSet, RelationshipNavigation.From, ownerKey, RelationshipNavigation.To, value);
                            // Add the relationship in the unchanged state if
                            var relationshipState = EntityState.Added;

                            // If this is an unchanged/modified dependent end of a relationship and we are allowing the EntityKey to be set
                            // create the relationship in the Unchanged state because the state must "match" the dependent end state
                            if (!ownerKey.IsTemporary
                                && IsDependentEndOfReferentialConstraint(false))
                            {
                                relationshipState = EntityState.Unchanged;
                            }
                            manager.AddNewRelation(wrapper, relationshipState);
                        }
                    }
                }
            }
            else
            {
                // Just set the field for detached object -- during Attach/Add we will make sure this value
                // is not in conflict if the EntityReference contains a real entity. We cannot always determine the
                // EntityKey for any real entity in the detached state, so we don't bother to do it here.
                DetachedEntityKey = value;
            }
        }

        /// <summary>
        /// This method is called when either the EntityKey or the Value property is set to null when it is
        /// already null. For an FK association of a tracked entity the method will attempt to null FKs
        /// thereby deleting the relationship. This may result in conceptual nulls being set.
        /// </summary>
        internal bool AttemptToNullFKsOnRefOrKeySetToNull()
        {
            if (ReferenceValue.Entity == null &&
                WrappedOwner.Entity != null &&
                WrappedOwner.Context != null &&
                !UsingNoTracking &&
                IsForeignKey)
            {
                // For identifying relationships, we throw, since we cannot set primary key values to null, unless
                // the entity is in the Added state.
                if (WrappedOwner.ObjectStateEntry.State != EntityState.Added
                    &&
                    IsDependentEndOfReferentialConstraint(checkIdentifying: true))
                {
                    throw new InvalidOperationException(Strings.EntityReference_CannotChangeReferentialConstraintProperty);
                }

                // For unloaded FK relationships in the context we attempt to null FK values here, which will
                // delete the relationship.
                RemoveFromLocalCache(EntityWrapperFactory.NullWrapper, resetIsLoaded: true, preserveForeignKey: false);

                return true;
            }
            return false;
        }

        internal EntityKey AttachedEntityKey
        {
            get
            {
                Debug.Assert(
                    ObjectContext != null && !UsingNoTracking,
                    "Should only need to access AttachedEntityKey property on attached EntityReferences");
                return EntityKey;
            }
        }

        internal EntityKey DetachedEntityKey
        {
            get { return _detachedEntityKey; }
            set { _detachedEntityKey = value; }
        }

        internal EntityKey CachedForeignKey
        {
            get { return EntityKey ?? _cachedForeignKey; }
        }

        internal void SetCachedForeignKey(EntityKey newForeignKey, EntityEntry source)
        {
            if (ObjectContext != null && ObjectContext.ObjectStateManager != null && // are we attached?
                source != null && // do we have an entry?
                _cachedForeignKey != null && !ForeignKeyFactory.IsConceptualNullKey(_cachedForeignKey) // do we have an fk?
                && _cachedForeignKey != newForeignKey) // is the FK different from the one that we already have?
            {
                ObjectContext.ObjectStateManager.RemoveEntryFromForeignKeyIndex(_cachedForeignKey, source);
            }
            _cachedForeignKey = newForeignKey;
        }

        internal IEnumerable<EntityKey> GetAllKeyValues()
        {
            if (EntityKey != null)
            {
                yield return EntityKey;
            }

            if (_cachedForeignKey != null)
            {
                yield return _cachedForeignKey;
            }

            if (_detachedEntityKey != null)
            {
                yield return _detachedEntityKey;
            }
        }

        internal abstract IEntityWrapper CachedValue { get; }

        internal abstract IEntityWrapper ReferenceValue { get; set; }

        internal EntityKey ValidateOwnerWithRIConstraints(IEntityWrapper targetEntity, EntityKey targetEntityKey, bool checkBothEnds)
        {
            var ownerKey = WrappedOwner.EntityKey;

            // Check if Referential Constraints are violated
            if ((object)ownerKey != null &&
                !ownerKey.IsTemporary
                &&
                IsDependentEndOfReferentialConstraint(checkIdentifying: true))
            {
                Debug.Assert(CachedForeignKey != null || EntityKey == null, "CachedForeignKey should not be null if EntityKey is not null.");
                ValidateSettingRIConstraints(
                    targetEntity,
                    targetEntityKey == null,
                    (CachedForeignKey != null && CachedForeignKey != targetEntityKey));
            }
            else if (checkBothEnds && targetEntity != null
                     && targetEntity.Entity != null)
            {
                var otherEnd = GetOtherEndOfRelationship(targetEntity) as EntityReference;
                if (otherEnd != null)
                {
                    otherEnd.ValidateOwnerWithRIConstraints(WrappedOwner, ownerKey, checkBothEnds: false);
                }
            }

            return ownerKey;
        }

        internal void ValidateSettingRIConstraints(IEntityWrapper targetEntity, bool settingToNull, bool changingForeignKeyValue)
        {
            var isNoTracking = targetEntity != null && targetEntity.MergeOption == MergeOption.NoTracking;

            if (settingToNull || // setting the principle to null
                changingForeignKeyValue
                || // existing key does not match incoming key
                (targetEntity != null &&
                 !isNoTracking &&
                 (targetEntity.ObjectStateEntry == null || // setting to a detached principle
                  (EntityKey == null && targetEntity.ObjectStateEntry.State == EntityState.Deleted || // setting to a deleted principle
                   (CachedForeignKey == null && targetEntity.ObjectStateEntry.State == EntityState.Added)))))
                // setting to an added principle
            {
                throw new InvalidOperationException(Strings.EntityReference_CannotChangeReferentialConstraintProperty);
            }
        }

        /// <summary>
        /// EntityReferences can only deferred load if they are empty
        /// </summary>
        internal override bool CanDeferredLoad
        {
            get { return IsEmpty(); }
        }

        /// <summary>
        /// Takes key values from the given principal entity and transfers them to the foreign key properties
        /// of the dependant entry.  This method requires a context, but does not require that either
        /// entity is in the context.  This allows it to work in NoTracking cases where we have the context
        /// but we're not tracked by that context.
        /// </summary>
        /// <param name="dependentEntity">The entity into which foreign key values will be written</param>
        /// <param name="principalEntity">The entity from which key values will be obtained</param>
        /// <param name="changedFKs">If non-null, then keeps track of FKs that have already been set such that an exception can be thrown if we find conflicting values</param>
        /// <param name="forceChange">If true, then the property setter is called even if FK values already match,
        ///                           which causes the FK properties to be marked as modified.</param>
        internal void UpdateForeignKeyValues(
            IEntityWrapper dependentEntity, IEntityWrapper principalEntity, Dictionary<int, object> changedFKs, bool forceChange)
        {
            Debug.Assert(dependentEntity.Entity != null, "dependentEntity.Entity == null");
            Debug.Assert(principalEntity.Entity != null, "principalEntity.Entity == null");
            Debug.Assert(IsForeignKey, "cannot update foreign key values if the relationship is not a FK");
            var constraint = ((AssociationType)RelationMetadata).ReferentialConstraints[0];
            Debug.Assert(constraint != null, "null constraint");

            var isUnchangedDependent = (object)WrappedOwner.EntityKey != null &&
                                       !WrappedOwner.EntityKey.IsTemporary &&
                                       IsDependentEndOfReferentialConstraint(checkIdentifying: true);

            var stateManager = ObjectContext.ObjectStateManager;
            stateManager.TransactionManager.BeginForeignKeyUpdate(this);
            try
            {
                var principalEntitySet = ((AssociationSet)RelationshipSet).AssociationSetEnds[ToEndMember.Name].EntitySet;
                var principalTypeMetadata = stateManager.GetOrAddStateManagerTypeMetadata(principalEntity.IdentityType, principalEntitySet);

                var dependentEntitySet = ((AssociationSet)RelationshipSet).AssociationSetEnds[FromEndProperty.Name].EntitySet;
                var dependentTypeMetadata = stateManager.GetOrAddStateManagerTypeMetadata(dependentEntity.IdentityType, dependentEntitySet);

                var principalProps = constraint.FromProperties;
                var numValues = principalProps.Count;
                string[] keyNames = null;
                object[] values = null;
                if (numValues > 1)
                {
                    keyNames = principalEntitySet.ElementType.KeyMemberNames;
                    values = new object[numValues];
                }
                for (var i = 0; i < numValues; i++)
                {
                    var principalOrdinal = principalTypeMetadata.GetOrdinalforOLayerMemberName(principalProps[i].Name);
                    var value = principalTypeMetadata.Member(principalOrdinal).GetValue(principalEntity.Entity);
                    var dependentOrdinal = dependentTypeMetadata.GetOrdinalforOLayerMemberName(constraint.ToProperties[i].Name);
                    var valueChanging =
                        !ByValueEqualityComparer.Default.Equals(
                            dependentTypeMetadata.Member(dependentOrdinal).GetValue(dependentEntity.Entity), value);
                    if (forceChange || valueChanging)
                    {
                        if (isUnchangedDependent)
                        {
                            ValidateSettingRIConstraints(
                                principalEntity, settingToNull: value == null, changingForeignKeyValue: valueChanging);
                        }
                        // If we're tracking FK values that have already been set, then compare the value we are about to set
                        // to the value we previously set for this ordinal, if such a value exists.  If they don't match then
                        // it means that we got conflicting FK values from two different PKs and we should throw.
                        if (changedFKs != null)
                        {
                            object previouslySetValue;
                            if (changedFKs.TryGetValue(dependentOrdinal, out previouslySetValue))
                            {
                                if (!ByValueEqualityComparer.Default.Equals(previouslySetValue, value))
                                {
                                    throw new InvalidOperationException(Strings.Update_ReferentialConstraintIntegrityViolation);
                                }
                            }
                            else
                            {
                                changedFKs[dependentOrdinal] = value;
                            }
                        }
                        dependentEntity.SetCurrentValue(
                            dependentEntity.ObjectStateEntry,
                            dependentTypeMetadata.Member(dependentOrdinal),
                            -1,
                            dependentEntity.Entity,
                            value);
                    }

                    if (numValues > 1)
                    {
                        var keyIndex = Array.IndexOf(keyNames, principalProps[i].Name);
                        Debug.Assert(keyIndex >= 0 && keyIndex < numValues, "Could not find constraint prop name in entity set key names");
                        values[keyIndex] = value;
                    }
                    else
                    {
                        SetCachedForeignKey(new EntityKey(principalEntitySet, value), dependentEntity.ObjectStateEntry);
                    }
                }

                if (numValues > 1)
                {
                    SetCachedForeignKey(new EntityKey(principalEntitySet, values), dependentEntity.ObjectStateEntry);
                }
                if (WrappedOwner.ObjectStateEntry != null)
                {
                    stateManager.ForgetEntryWithConceptualNull(WrappedOwner.ObjectStateEntry, resetAllKeys: false);
                }
            }
            finally
            {
                stateManager.TransactionManager.EndForeignKeyUpdate();
            }
        }

        /// <summary>
        /// Takes key values from the given principal key and transfers them to the foreign key properties
        /// of the dependant entry.  This method requires a context, but does not require that either
        /// entity or key is in the context.  This allows it to work in NoTracking cases where we have the context
        /// but we're not tracked by that context.
        /// </summary>
        /// <param name="dependentEntity">The entity into which foreign key values will be written</param>
        /// <param name="principalEntity">The key from which key values will be obtained</param>
        internal void UpdateForeignKeyValues(IEntityWrapper dependentEntity, EntityKey principalKey)
        {
            Debug.Assert(dependentEntity.Entity != null, "dependentEntity.Entity == null");
            Debug.Assert(principalKey != null, "principalKey == null");
            Debug.Assert(!principalKey.IsTemporary, "Cannot update from a temp key");
            Debug.Assert(IsForeignKey, "cannot update foreign key values if the relationship is not a FK");
            var constraint = ((AssociationType)RelationMetadata).ReferentialConstraints[0];
            Debug.Assert(constraint != null, "null constraint");

            var stateManager = ObjectContext.ObjectStateManager;
            stateManager.TransactionManager.BeginForeignKeyUpdate(this);
            try
            {
                var dependentEntitySet = ((AssociationSet)RelationshipSet).AssociationSetEnds[FromEndProperty.Name].EntitySet;
                var dependentTypeMetadata = stateManager.GetOrAddStateManagerTypeMetadata(dependentEntity.IdentityType, dependentEntitySet);

                for (var i = 0; i < constraint.FromProperties.Count; i++)
                {
                    var value = principalKey.FindValueByName(constraint.FromProperties[i].Name);
                    var dependentOrdinal = dependentTypeMetadata.GetOrdinalforOLayerMemberName(constraint.ToProperties[i].Name);
                    var currentValue = dependentTypeMetadata.Member(dependentOrdinal).GetValue(dependentEntity.Entity);
                    if (!ByValueEqualityComparer.Default.Equals(currentValue, value))
                    {
                        dependentEntity.SetCurrentValue(
                            dependentEntity.ObjectStateEntry,
                            dependentTypeMetadata.Member(dependentOrdinal),
                            -1,
                            dependentEntity.Entity,
                            value);
                    }
                }

                SetCachedForeignKey(principalKey, dependentEntity.ObjectStateEntry);
                if (WrappedOwner.ObjectStateEntry != null)
                {
                    stateManager.ForgetEntryWithConceptualNull(WrappedOwner.ObjectStateEntry, resetAllKeys: false);
                }
            }
            finally
            {
                stateManager.TransactionManager.EndForeignKeyUpdate();
            }
        }

        internal object GetDependentEndOfReferentialConstraint(object relatedValue)
        {
            return IsDependentEndOfReferentialConstraint(checkIdentifying: false)
                       ? WrappedOwner.Entity
                       : relatedValue;
        }

        internal bool NavigationPropertyIsNullOrMissing()
        {
            Debug.Assert(RelationshipNavigation != null, "null RelationshipNavigation");

            return !TargetAccessor.HasProperty || WrappedOwner.GetNavigationPropertyValue(this) == null;
        }

        /// <summary>
        /// Attempts to null all FKs associated with the dependent end of this relationship on this entity.
        /// This may result in setting conceptual nulls if the FK is not nullable.
        /// </summary>
        internal void NullAllForeignKeys()
        {
            Debug.Assert(ObjectContext != null, "Nulling FKs only works when attached.");
            Debug.Assert(IsForeignKey, "Cannot null FKs for independent associations.");

            var stateManager = ObjectContext.ObjectStateManager;
            var entry = WrappedOwner.ObjectStateEntry;
            var transManager = stateManager.TransactionManager;
            if (!transManager.IsGraphUpdate && !transManager.IsAttachTracking
                && !transManager.IsRelatedEndAdd)
            {
                var constraint = ((AssociationType)RelationMetadata).ReferentialConstraints.Single();
                if (TargetRoleName == constraint.FromRole.Name) // Only do this on the dependent end
                {
                    if (transManager.IsDetaching)
                    {
                        // If the principal is being detached, then the dependent must be added back to the
                        // dangling keys index.
                        // Perf note: The dependent currently gets added when it is being detached and is then
                        // removed again later in the process.  The code could be optimized to prevent this.
                        Debug.Assert(entry != null, "State entry must exist while detaching.");
                        var foreignKey = ForeignKeyFactory.CreateKeyFromForeignKeyValues(entry, this);
                        if (foreignKey != null)
                        {
                            stateManager.AddEntryContainingForeignKeyToIndex(foreignKey, entry);
                        }
                    }
                    else if (!ReferenceEquals(stateManager.EntityInvokingFKSetter, WrappedOwner.Entity)
                             && !transManager.IsForeignKeyUpdate)
                    {
                        transManager.BeginForeignKeyUpdate(this);
                        try
                        {
                            var unableToNull = true;
                            var canSetModifiedProps = entry != null
                                                      && (entry.State == EntityState.Modified || entry.State == EntityState.Unchanged);
                            var dependentEntitySet = ((AssociationSet)RelationshipSet).AssociationSetEnds[FromEndProperty.Name].EntitySet;
                            var dependentTypeMetadata = stateManager.GetOrAddStateManagerTypeMetadata(
                                WrappedOwner.IdentityType, dependentEntitySet);

                            for (var i = 0; i < constraint.FromProperties.Count; i++)
                            {
                                var propertyName = constraint.ToProperties[i].Name;
                                var dependentOrdinal = dependentTypeMetadata.GetOrdinalforOLayerMemberName(propertyName);
                                var member = dependentTypeMetadata.Member(dependentOrdinal);

                                // This is a check for nullability in o-space. However, o-space nullability is not the
                                // same as nullability of the underlying type. In particular, one difference is that when
                                // attribute-based mapping is used then a property can be marked as not nullable in o-space
                                // even when the underlying CLR type is nullable. For such a case, we treat the property
                                // as if it were not nullable (since that's what we have shipped) even though we could
                                // technically set it to null.
                                if (member.ClrMetadata.Nullable)
                                {
                                    // Only set the value to null if it is not already null.
                                    if (member.GetValue(WrappedOwner.Entity) != null)
                                    {
                                        WrappedOwner.SetCurrentValue(
                                            WrappedOwner.ObjectStateEntry,
                                            dependentTypeMetadata.Member(dependentOrdinal),
                                            -1,
                                            WrappedOwner.Entity,
                                            null);
                                    }
                                    else
                                    {
                                        // Given that the current value is null, this next check confirms that the original
                                        // value is also null.  If it isn't, then we must make sure that the entity is marked
                                        // as modified.
                                        // This case can happen because fixup in the entity can set the FK to null while processing
                                        // a RelatedEnd operation.  This will be detected by DetectChanges, but when performing
                                        // RelatedEnd operations the user is not required to call DetectChanges.
                                        if (canSetModifiedProps
                                            && WrappedOwner.ObjectStateEntry.OriginalValues.GetValue(dependentOrdinal) != null)
                                        {
                                            entry.SetModifiedProperty(propertyName);
                                        }
                                    }
                                    unableToNull = false;
                                }
                                else if (canSetModifiedProps)
                                {
                                    entry.SetModifiedProperty(propertyName);
                                }
                            }
                            if (unableToNull)
                            {
                                // We were unable to null out the FK because all FK properties were non-nullable.
                                // We need to keep track of this state so that we treat the FK as null even though
                                // we were not able to null it.  This prevents the FK from being used for fixup and
                                // also causes an exception to be thrown if an attempt is made to commit in this state.

                                //We should only set a conceptual null if the entity is tracked
                                if (entry != null)
                                {
                                    //The CachedForeignKey may be null if we are putting
                                    //back a Conceptual Null as part of roll back
                                    var realKey = CachedForeignKey;
                                    if (realKey == null)
                                    {
                                        realKey = ForeignKeyFactory.CreateKeyFromForeignKeyValues(entry, this);
                                    }

                                    // Note that the realKey can still be null here for a situation where the key is marked not nullable
                                    // in o-space and yet the underlying type is nullable and the entity has been added or attached with a null
                                    // value for the property. This will cause SaveChanges to throw unless the entity is marked
                                    // as deleted before SaveChanges is called, in which case we don't want to set a conceptual
                                    // null here as the call might very well succeed in the database since, unless the FK is
                                    // a concurrency token, the value we have for it is not used at all for the delete.
                                    if (realKey != null)
                                    {
                                        SetCachedForeignKey(ForeignKeyFactory.CreateConceptualNullKey(realKey), entry);
                                        stateManager.RememberEntryWithConceptualNull(entry);
                                    }
                                }
                            }
                            else
                            {
                                SetCachedForeignKey(null, entry);
                            }
                        }
                        finally
                        {
                            transManager.EndForeignKeyUpdate();
                        }
                    }
                }
            }
        }
    }
}
