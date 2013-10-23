// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    ///     Represents an entity set and the association sets incident to it,
    ///     which can be collapsed if the requirements are satisfied.
    ///     Internal for test purposes only.
    /// </summary>
    internal class CollapsibleEntityAssociationSets
    {
        private readonly EntitySet _entitySet;
        private readonly List<AssociationSet> _associationSets = new List<AssociationSet>(2);

        public CollapsibleEntityAssociationSets(EntitySet entitySet)
        {
            Debug.Assert(entitySet != null, "entitySet != null");
            _entitySet = entitySet;
        }

        public EntitySet EntitySet
        {
            get { return _entitySet; }
        }

        public List<AssociationSet> AssociationSets
        {
            get { return _associationSets; }
        }

        private bool MeetsRequirementsForCollapsing
        {
            get
            {
                if (_associationSets.Count != 2)
                {
                    return false;
                }

                var constraints0 = _associationSets[0].ElementType.ReferentialConstraints;
                var constraints1 = _associationSets[1].ElementType.ReferentialConstraints;

                if (constraints0.Count != 1
                    || constraints1.Count != 1)
                {
                    return false;
                }

                var constraint0 = constraints0[0];
                var constraint1 = constraints1[0];

                if (!IsEntityDependentSideOfBothAssociations(_entitySet, constraint0, constraint1))
                {
                    return false;
                }

                if (!IsAtLeastOneColumnOfBothDependentRelationshipColumnSetsNonNullable(constraint0, constraint1))
                {
                    return false;
                }

                if (!AreAllEntityColumnsMappedAsToColumns(_entitySet, constraint0, constraint1))
                {
                    return false;
                }

                if (IsAtLeastOneColumnFkInBothAssociations(constraint0, constraint1))
                {
                    return false;
                }

                return true;
            }
        }

        public static ICollection<CollapsibleEntityAssociationSets> CreateCollapsibleItems(
            IEnumerable<EntitySetBase> storeSets,
            out IEnumerable<AssociationSet> associationSetsFromNonCollapsibleItems)
        {
            var collapsibleItems = CreateCollapsingCandidates(storeSets);
            var associationSets = new HashSet<AssociationSet>();

            foreach (var item in collapsibleItems.Values.ToList())
            {
                if (!item.MeetsRequirementsForCollapsing)
                {
                    collapsibleItems.Remove(item.EntitySet);
                    associationSets.UnionWith(item.AssociationSets);
                }
            }

            foreach (var item in collapsibleItems.Values.ToList())
            {
                foreach (var set in item.AssociationSets)
                {
                    Debug.Assert(set.AssociationSetEnds.Count == 2);

                    CollapsibleEntityAssociationSets item0, item1;
                    var entitySet0 = set.AssociationSetEnds[0].EntitySet;
                    var entitySet1 = set.AssociationSetEnds[1].EntitySet;

                    // Eliminate the ends of the association if both are candidates,
                    // because we don't know which entity to collapse.
                    if (collapsibleItems.TryGetValue(entitySet0, out item0)
                        && collapsibleItems.TryGetValue(entitySet1, out item1))
                    {
                        collapsibleItems.Remove(item0.EntitySet);
                        collapsibleItems.Remove(item1.EntitySet);
                        associationSets.UnionWith(item0.AssociationSets);
                        associationSets.UnionWith(item1.AssociationSets);
                    }
                }
            }

            associationSetsFromNonCollapsibleItems = associationSets.Where(
                set => !set.AssociationSetEnds.Any(
                    end => collapsibleItems.ContainsKey(end.EntitySet)));

            return collapsibleItems.Values;
        }

        public AssociationSetEndDetails GetStoreAssociationSetEnd(int index)
        {
            Debug.Assert((index & 0xFFFE) == 0, "index can only be 0 or 1");
            Debug.Assert(AssociationSets.Count == 2);

            var definingSet = AssociationSets[index];
            var multiplicitySet = AssociationSets[(index + 1) % 2];

            // for a situation like this (CD is CascadeDelete)
            // 
            // --------  CD   --------  CD   --------
            // | A    |1 <-  1| AtoB |* <-  1|  B   |  
            // |      |-------|      |-------|      | 
            // |      |       |      |       |      |
            // --------       --------       --------
            // 
            // You get
            // --------  CD   --------
            // |  A   |* <-  1|  B   |
            // |      |-------|      |
            // |      |       |      |
            // --------       --------
            // 
            // Notice that the new "link table association" muliplicities are opposite of what is comming into the original link table
            // this seems counter intuitive at first, but makes sense when you think all the way through it
            //
            // CascadeDelete Behavior (we can assume the runtime will always delete cascade 
            // to the link table from the outside tables (it actually doesn't, but that is a bug))
            //  Store               Effective
            //  A -> AToB <- B      None
            //  A <- AToB <- B      <-
            //  A -> AToB -> B      ->
            //  A <- AToB -> B      None
            //  A <- AToB    B      <-
            //  A    AToB -> B      ->
            //  A -> AToB    B      None
            //  A    AToB <- B      None
            //  
            //  Other CascadeDelete rules
            //  1. Can't have a delete from a Many multiplicity end
            //  2. Can't have a delete on both ends
            //

            var associationSetEnd = GetAssociationSetEnd(definingSet, true);
            var multiplicityAssociationSetEnd = GetAssociationSetEnd(multiplicitySet, false);

            var multiplicity = multiplicityAssociationSetEnd.CorrespondingAssociationEndMember.RelationshipMultiplicity;
            var deleteBehavior = OperationAction.None;

            if (multiplicity != RelationshipMultiplicity.Many)
            {
                var otherEndBehavior =
                    GetAssociationSetEnd(definingSet, false).CorrespondingAssociationEndMember.DeleteBehavior;

                if (otherEndBehavior == OperationAction.None)
                {
                    // Since the other end does not have an operation it means that only one end could possibly have an operation.
                    deleteBehavior = multiplicityAssociationSetEnd.CorrespondingAssociationEndMember.DeleteBehavior;
                }
            }

            return new AssociationSetEndDetails(associationSetEnd, multiplicity, deleteBehavior);
        }

        private static bool IsEntityDependentSideOfBothAssociations(
            EntitySet storeEntitySet,
            ReferentialConstraint constraint0,
            ReferentialConstraint constraint1)
        {
            return ((RefType)constraint0.ToRole.TypeUsage.EdmType).ElementType == storeEntitySet.ElementType
                   && ((RefType)constraint1.ToRole.TypeUsage.EdmType).ElementType == storeEntitySet.ElementType;
        }

        private static bool IsAtLeastOneColumnOfBothDependentRelationshipColumnSetsNonNullable(
            ReferentialConstraint constraint0,
            ReferentialConstraint constraint1)
        {
            return constraint0.ToProperties.Any(p => !p.Nullable)
                   && constraint1.ToProperties.Any(p => !p.Nullable);
        }

        private static bool AreAllEntityColumnsMappedAsToColumns(
            EntitySet storeEntitySet,
            ReferentialConstraint constraint0,
            ReferentialConstraint constraint1)
        {
            var names = new HashSet<string>(constraint0.ToProperties.Select(p => p.Name));
            names.UnionWith(constraint1.ToProperties.Select(p => p.Name));
            return names.Count == storeEntitySet.ElementType.Properties.Count;
        }

        private static bool IsAtLeastOneColumnFkInBothAssociations(
            ReferentialConstraint constraint0,
            ReferentialConstraint constraint1)
        {
            return constraint1.ToProperties.Any(p => constraint0.ToProperties.Contains(p));
        }

        private static IDictionary<EntitySet, CollapsibleEntityAssociationSets> CreateCollapsingCandidates(
            IEnumerable<EntitySetBase> storeSets)
        {
            var candidates = new Dictionary<EntitySet, CollapsibleEntityAssociationSets>();
            var associationSets = new List<AssociationSet>();

            foreach (var set in storeSets)
            {
                switch (set.BuiltInTypeKind)
                {
                    case BuiltInTypeKind.AssociationSet:
                        associationSets.Add((AssociationSet)set);
                        break;

                    case BuiltInTypeKind.EntitySet:
                        var entitySet = (EntitySet)set;
                        candidates.Add(entitySet, new CollapsibleEntityAssociationSets(entitySet));
                        break;

                    default:
                        throw new InvalidOperationException(
                            String.Format(
                                CultureInfo.InvariantCulture,
                                Resources_VersioningFacade.ModelGeneration_UnGeneratableType,
                                set.BuiltInTypeKind));
                }
            }

            // Add the association sets to the corresponding collapsing items.
            foreach (var set in associationSets)
            {
                foreach (var setEnd in set.AssociationSetEnds)
                {
                    CollapsibleEntityAssociationSets item;
                    if (candidates.TryGetValue(setEnd.EntitySet, out item))
                    {
                        item.AssociationSets.Add(set);
                    }
                }
            }

            return candidates;
        }

        private static AssociationSetEnd GetAssociationSetEnd(AssociationSet associationSet, bool setEndIsSource)
        {
            var setEnds = associationSet.AssociationSetEnds;
            var constraints = associationSet.ElementType.ReferentialConstraints;

            Debug.Assert(setEnds.Count == 2, "setEnds.Count == 2");
            Debug.Assert(constraints.Count == 1, "constraints.Count == 1");

            var endMemberIsSource = setEnds[0].CorrespondingAssociationEndMember == constraints[0].FromRole;
            return setEnds[endMemberIsSource == setEndIsSource ? 0 : 1];
        }
    }
}
