namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal partial class UpdateTranslator
    {
        /// <summary>
        /// Class validating relationship cardinality constraints. Only reasons about constraints that can be inferred
        /// by examining change requests from the store.
        /// (no attempt is made to ensure consistency of the store subsequently, since this would require pulling in all
        /// values from the store).
        /// </summary>
        private class RelationshipConstraintValidator
        {
            #region Constructor

            internal RelationshipConstraintValidator()
            {
                m_existingRelationships =
                    new Dictionary<DirectionalRelationship, DirectionalRelationship>(EqualityComparer<DirectionalRelationship>.Default);
                m_impliedRelationships =
                    new Dictionary<DirectionalRelationship, IEntityStateEntry>(EqualityComparer<DirectionalRelationship>.Default);
                m_referencingRelationshipSets = new Dictionary<EntitySet, List<AssociationSet>>(EqualityComparer<EntitySet>.Default);
            }

            #endregion

            #region Fields

            /// <summary>
            /// Relationships registered in the validator.
            /// </summary>
            private readonly Dictionary<DirectionalRelationship, DirectionalRelationship> m_existingRelationships;

            /// <summary>
            /// Relationships the validator determines are required based on registered entities.
            /// </summary>
            private readonly Dictionary<DirectionalRelationship, IEntityStateEntry> m_impliedRelationships;

            /// <summary>
            /// Cache used to store relationship sets with ends bound to entity sets.
            /// </summary>
            private readonly Dictionary<EntitySet, List<AssociationSet>> m_referencingRelationshipSets;

            #endregion

            #region Methods

            /// <summary>
            /// Add an entity to be tracked by the validator. Requires that the input describes an entity.
            /// </summary>
            /// <param name="stateEntry">State entry for the entity being tracked.</param>
            internal void RegisterEntity(IEntityStateEntry stateEntry)
            {
                Contract.Requires(stateEntry != null);

                if (EntityState.Added == stateEntry.State
                    || EntityState.Deleted == stateEntry.State)
                {
                    // We only track added and deleted entities because modifications to entities do not affect
                    // cardinality constraints. Relationships are based on end keys, and it is not
                    // possible to modify key values.
                    Debug.Assert(null != (object)stateEntry.EntityKey, "entity state entry must have an entity key");
                    var entityKey = stateEntry.EntityKey;
                    var entitySet = (EntitySet)stateEntry.EntitySet;
                    var entityType = EntityState.Added == stateEntry.State
                                         ? GetEntityType(stateEntry.CurrentValues)
                                         : GetEntityType(stateEntry.OriginalValues);

                    // figure out relationship set ends that are associated with this entity set
                    foreach (var associationSet in GetReferencingAssocationSets(entitySet))
                    {
                        // describe unidirectional relationships in which the added entity is the "destination"
                        var ends = associationSet.AssociationSetEnds;
                        foreach (var fromEnd in ends)
                        {
                            foreach (var toEnd in ends)
                            {
                                // end to itself does not describe an interesting relationship subpart
                                if (ReferenceEquals(
                                    toEnd.CorrespondingAssociationEndMember,
                                    fromEnd.CorrespondingAssociationEndMember))
                                {
                                    continue;
                                }

                                // skip ends that don't target the current entity set
                                if (!toEnd.EntitySet.EdmEquals(entitySet))
                                {
                                    continue;
                                }

                                // skip ends that aren't required
                                if (0 == MetadataHelper.GetLowerBoundOfMultiplicity(
                                    fromEnd.CorrespondingAssociationEndMember.RelationshipMultiplicity))
                                {
                                    continue;
                                }

                                // skip ends that don't target the current entity type
                                if (!MetadataHelper.GetEntityTypeForEnd(toEnd.CorrespondingAssociationEndMember)
                                         .IsAssignableFrom(entityType))
                                {
                                    continue;
                                }

                                // register the relationship so that we know it's required
                                var relationship = new DirectionalRelationship(
                                    entityKey, fromEnd.CorrespondingAssociationEndMember,
                                    toEnd.CorrespondingAssociationEndMember, associationSet, stateEntry);
                                m_impliedRelationships.Add(relationship, stateEntry);
                            }
                        }
                    }
                }
            }

            // requires: input is an IExtendedDataRecord representing an entity
            // returns: entity type for the given record
            private static EntityType GetEntityType(DbDataRecord dbDataRecord)
            {
                var extendedRecord = dbDataRecord as IExtendedDataRecord;
                Debug.Assert(extendedRecord != null);

                Debug.Assert(BuiltInTypeKind.EntityType == extendedRecord.DataRecordInfo.RecordType.EdmType.BuiltInTypeKind);
                return (EntityType)extendedRecord.DataRecordInfo.RecordType.EdmType;
            }

            /// <summary>
            /// Add a relationship to be tracked by the validator.
            /// </summary>
            /// <param name="associationSet">Relationship set to which the given record belongs.</param>
            /// <param name="record">Relationship record. Must conform to the type of the relationship set.</param>
            /// <param name="stateEntry">State entry for the relationship being tracked</param>
            internal void RegisterAssociation(AssociationSet associationSet, IExtendedDataRecord record, IEntityStateEntry stateEntry)
            {
                Contract.Requires(associationSet != null);
                Contract.Requires(record != null);
                Contract.Requires(stateEntry != null);

                Debug.Assert(associationSet.ElementType.Equals(record.DataRecordInfo.RecordType.EdmType));

                // retrieve the ends of the relationship
                var endNameToKeyMap = new Dictionary<string, EntityKey>(
                    StringComparer.Ordinal);
                foreach (var field in record.DataRecordInfo.FieldMetadata)
                {
                    var endName = field.FieldType.Name;
                    var entityKey = (EntityKey)record.GetValue(field.Ordinal);
                    endNameToKeyMap.Add(endName, entityKey);
                }

                // register each unidirectional relationship subpart in the relationship instance
                var ends = associationSet.AssociationSetEnds;
                foreach (var fromEnd in ends)
                {
                    foreach (var toEnd in ends)
                    {
                        // end to itself does not describe an interesting relationship subpart
                        if (ReferenceEquals(toEnd.CorrespondingAssociationEndMember, fromEnd.CorrespondingAssociationEndMember))
                        {
                            continue;
                        }

                        var toEntityKey = endNameToKeyMap[toEnd.CorrespondingAssociationEndMember.Name];
                        var relationship = new DirectionalRelationship(
                            toEntityKey, fromEnd.CorrespondingAssociationEndMember,
                            toEnd.CorrespondingAssociationEndMember, associationSet, stateEntry);
                        AddExistingRelationship(relationship);
                    }
                }
            }

            /// <summary>
            /// Validates cardinality constraints for all added entities/relationships.
            /// </summary>
            internal void ValidateConstraints()
            {
                // ensure all expected relationships exist
                foreach (var expected in m_impliedRelationships)
                {
                    var expectedRelationship = expected.Key;
                    var stateEntry = expected.Value;

                    // determine actual end cardinality
                    var count = GetDirectionalRelationshipCountDelta(expectedRelationship);

                    if (EntityState.Deleted
                        == stateEntry.State)
                    {
                        // our cardinality expectations are reversed for delete (cardinality of 1 indicates
                        // we want -1 operation total)
                        count = -count;
                    }

                    // determine expected cardinality
                    var minimumCount = MetadataHelper.GetLowerBoundOfMultiplicity(expectedRelationship.FromEnd.RelationshipMultiplicity);
                    var maximumCountDeclared =
                        MetadataHelper.GetUpperBoundOfMultiplicity(expectedRelationship.FromEnd.RelationshipMultiplicity);
                    var maximumCount = maximumCountDeclared.HasValue ? maximumCountDeclared.Value : count; // negative value
                    // indicates unlimited cardinality

                    if (count < minimumCount
                        || count > maximumCount)
                    {
                        // We could in theory "fix" the cardinality constraint violation by introducing surrogates,
                        // but we risk doing work on behalf of the user they don't want performed (e.g., deleting an
                        // entity or relationship the user has intentionally left untouched).
                        throw EntityUtil.UpdateRelationshipCardinalityConstraintViolation(
                            expectedRelationship.AssociationSet.Name, minimumCount, maximumCountDeclared,
                            TypeHelpers.GetFullName(
                                expectedRelationship.ToEntityKey.EntityContainerName, expectedRelationship.ToEntityKey.EntitySetName),
                            count, expectedRelationship.FromEnd.Name,
                            stateEntry);
                    }
                }

                // ensure actual relationships have required ends
                foreach (var actualRelationship in m_existingRelationships.Keys)
                {
                    int addedCount;
                    int deletedCount;
                    actualRelationship.GetCountsInEquivalenceSet(out addedCount, out deletedCount);
                    var absoluteCount = Math.Abs(addedCount - deletedCount);
                    var minimumCount = MetadataHelper.GetLowerBoundOfMultiplicity(actualRelationship.FromEnd.RelationshipMultiplicity);
                    var maximumCount = MetadataHelper.GetUpperBoundOfMultiplicity(actualRelationship.FromEnd.RelationshipMultiplicity);

                    // Check that we haven't inserted or deleted too many relationships
                    if (maximumCount.HasValue)
                    {
                        var violationType = default(EntityState?);
                        var violationCount = default(int?);
                        if (addedCount > maximumCount.Value)
                        {
                            violationType = EntityState.Added;
                            violationCount = addedCount;
                        }
                        else if (deletedCount > maximumCount.Value)
                        {
                            violationType = EntityState.Deleted;
                            violationCount = deletedCount;
                        }
                        if (violationType.HasValue)
                        {
                            throw new UpdateException(Strings.Update_RelationshipCardinalityViolation(
                                maximumCount.Value,
                                violationType.Value, actualRelationship.AssociationSet.ElementType.FullName,
                                actualRelationship.FromEnd.Name, actualRelationship.ToEnd.Name, violationCount.Value), null, actualRelationship.GetEquivalenceSet().Select(reln => reln.StateEntry).Cast<ObjectStateEntry>().Distinct());
                        }
                    }

                    // We care about the case where there is a relationship but no entity when
                    // the relationship and entity map to the same table. If there is a relationship
                    // with 1..1 cardinality to the entity and the relationship is being added or deleted,
                    // it is required that the entity is also added or deleted.
                    if (1 == absoluteCount && 1 == minimumCount
                        && 1 == maximumCount) // 1..1 relationship being added/deleted
                    {
                        var isAdd = addedCount > deletedCount;

                        // Ensure the entity is also being added or deleted
                        IEntityStateEntry entityEntry;

                        // Identify the following error conditions:
                        // - the entity is not being modified at all
                        // - the entity is being modified, but not in the way we expect (it's not being added or deleted)
                        if (!m_impliedRelationships.TryGetValue(actualRelationship, out entityEntry) ||
                            (isAdd && EntityState.Added != entityEntry.State)
                            ||
                            (!isAdd && EntityState.Deleted != entityEntry.State))
                        {
                            var message = Strings.Update_MissingRequiredEntity(actualRelationship.AssociationSet.Name, actualRelationship.StateEntry.State, actualRelationship.ToEnd.Name);
                            throw EntityUtil.Update(message, null, actualRelationship.StateEntry);
                        }
                    }
                }
            }

            /// <summary>
            /// Determines the net change in relationship count.
            /// For instance, if the directional relationship is added 2 times and deleted 3, the return value is -1.
            /// </summary>
            private int GetDirectionalRelationshipCountDelta(DirectionalRelationship expectedRelationship)
            {
                // lookup up existing relationship from expected relationship
                DirectionalRelationship existingRelationship;
                if (m_existingRelationships.TryGetValue(expectedRelationship, out existingRelationship))
                {
                    int addedCount;
                    int deletedCount;
                    existingRelationship.GetCountsInEquivalenceSet(out addedCount, out deletedCount);
                    return addedCount - deletedCount;
                }
                else
                {
                    // no modifications to the relationship... return 0 (no net change)
                    return 0;
                }
            }

            private void AddExistingRelationship(DirectionalRelationship relationship)
            {
                DirectionalRelationship existingRelationship;
                if (m_existingRelationships.TryGetValue(relationship, out existingRelationship))
                {
                    existingRelationship.AddToEquivalenceSet(relationship);
                }
                else
                {
                    m_existingRelationships.Add(relationship, relationship);
                }
            }

            /// <summary>
            /// Determine which relationship sets reference the given entity set.
            /// </summary>
            /// <param name="entitySet">Entity set for which to identify relationships</param>
            /// <returns>Relationship sets referencing the given entity set</returns>
            private IEnumerable<AssociationSet> GetReferencingAssocationSets(EntitySet entitySet)
            {
                List<AssociationSet> relationshipSets;

                // check if this information is cached
                if (!m_referencingRelationshipSets.TryGetValue(entitySet, out relationshipSets))
                {
                    relationshipSets = new List<AssociationSet>();

                    // relationship sets must live in the same container as the entity sets they reference
                    var container = entitySet.EntityContainer;
                    foreach (var extent in container.BaseEntitySets)
                    {
                        var associationSet = extent as AssociationSet;

                        if (null != associationSet
                            && !associationSet.ElementType.IsForeignKey)
                        {
                            foreach (var end in associationSet.AssociationSetEnds)
                            {
                                if (end.EntitySet.Equals(entitySet))
                                {
                                    relationshipSets.Add(associationSet);
                                    break;
                                }
                            }
                        }
                    }

                    // add referencing relationship information to the cache
                    m_referencingRelationshipSets.Add(entitySet, relationshipSets);
                }

                return relationshipSets;
            }

            #endregion

            #region Nested types

            /// <summary>
            /// An instance of an actual or expected relationship. This class describes one direction
            /// of the relationship. 
            /// </summary>
            private class DirectionalRelationship : IEquatable<DirectionalRelationship>
            {
                /// <summary>
                /// Entity key for the entity being referenced by the relationship.
                /// </summary>
                internal readonly EntityKey ToEntityKey;

                /// <summary>
                /// Name of the end referencing the entity key.
                /// </summary>
                internal readonly AssociationEndMember FromEnd;

                /// <summary>
                /// Name of the end the entity key references.
                /// </summary>
                internal readonly AssociationEndMember ToEnd;

                /// <summary>
                /// State entry containing this relationship.
                /// </summary>
                internal readonly IEntityStateEntry StateEntry;

                /// <summary>
                /// Reference to the relationship set.
                /// </summary>
                internal readonly AssociationSet AssociationSet;

                /// <summary>
                /// Reference to next 'equivalent' relationship in circular linked list.
                /// </summary>
                private DirectionalRelationship _equivalenceSetLinkedListNext;

                private readonly int _hashCode;

                internal DirectionalRelationship(
                    EntityKey toEntityKey, AssociationEndMember fromEnd, AssociationEndMember toEnd, AssociationSet associationSet,
                    IEntityStateEntry stateEntry)
                {
                    Contract.Requires(toEntityKey != null);
                    Contract.Requires(fromEnd != null);
                    Contract.Requires(toEnd != null);
                    Contract.Requires(associationSet != null);
                    Contract.Requires(stateEntry != null);

                    ToEntityKey = toEntityKey;
                    FromEnd = fromEnd;
                    ToEnd = toEnd;
                    AssociationSet = associationSet;
                    StateEntry = stateEntry;
                    _equivalenceSetLinkedListNext = this;

                    _hashCode = toEntityKey.GetHashCode() ^
                                fromEnd.GetHashCode() ^
                                toEnd.GetHashCode() ^
                                associationSet.GetHashCode();
                }

                /// <summary>
                /// Requires: 'other' must refer to the same relationship metadata and the same target entity and
                /// must not already be a part of an equivalent set.
                /// Adds the given relationship to linked list containing all equivalent relationship instances
                /// for this relationship (e.g. all orders associated with a specific customer)
                /// </summary>
                internal void AddToEquivalenceSet(DirectionalRelationship other)
                {
                    Debug.Assert(null != other, "other must not be null");
                    Debug.Assert(Equals(other), "other must be another instance of the same relationship target");
                    Debug.Assert(
                        ReferenceEquals(other._equivalenceSetLinkedListNext, other), "other must not be part of an equivalence set yet");
                    var currentSuccessor = _equivalenceSetLinkedListNext;
                    _equivalenceSetLinkedListNext = other;
                    other._equivalenceSetLinkedListNext = currentSuccessor;
                }

                /// <summary>
                /// Returns all relationships in equivalence set.
                /// </summary>
                internal IEnumerable<DirectionalRelationship> GetEquivalenceSet()
                {
                    // yield everything in circular linked list
                    var current = this;
                    do
                    {
                        yield return current;
                        current = current._equivalenceSetLinkedListNext;
                    }
                    while (!ReferenceEquals(current, this));
                }

                /// <summary>
                /// Determines the number of add and delete operations contained in this equivalence set.
                /// </summary>
                internal void GetCountsInEquivalenceSet(out int addedCount, out int deletedCount)
                {
                    addedCount = 0;
                    deletedCount = 0;
                    // yield everything in circular linked list
                    var current = this;
                    do
                    {
                        if (current.StateEntry.State
                            == EntityState.Added)
                        {
                            addedCount++;
                        }
                        else if (current.StateEntry.State
                                 == EntityState.Deleted)
                        {
                            deletedCount++;
                        }
                        current = current._equivalenceSetLinkedListNext;
                    }
                    while (!ReferenceEquals(current, this));
                }

                public override int GetHashCode()
                {
                    return _hashCode;
                }

                public bool Equals(DirectionalRelationship other)
                {
                    if (ReferenceEquals(this, other))
                    {
                        return true;
                    }
                    if (null == other)
                    {
                        return false;
                    }
                    if (ToEntityKey != other.ToEntityKey)
                    {
                        return false;
                    }
                    if (AssociationSet != other.AssociationSet)
                    {
                        return false;
                    }
                    if (ToEnd != other.ToEnd)
                    {
                        return false;
                    }
                    if (FromEnd != other.FromEnd)
                    {
                        return false;
                    }
                    return true;
                }

                public override bool Equals(object obj)
                {
                    Debug.Fail("use only typed Equals method");
                    return Equals(obj as DirectionalRelationship);
                }

                public override string ToString()
                {
                    return String.Format(
                        CultureInfo.InvariantCulture, "{0}.{1}-->{2}: {3}",
                        AssociationSet.Name, FromEnd.Name, ToEnd.Name,
                        StringUtil.BuildDelimitedList(ToEntityKey.EntityKeyValues, null, null));
                }
            }

            #endregion
        }
    }
}
