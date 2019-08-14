// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
    using System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    // This class represents the nodes that reside at the leaves of the tree
    internal class LeafCellTreeNode : CellTreeNode
    {
        // effects: Encapsulate the cell wrapper in the node
        internal LeafCellTreeNode(ViewgenContext context, LeftCellWrapper cellWrapper)
            : base(context)
        {
            m_cellWrapper = cellWrapper;
            cellWrapper.AssertHasUniqueCell();
            m_rightFragmentQuery = FragmentQuery.Create(
                cellWrapper.OriginalCellNumberString,
                cellWrapper.CreateRoleBoolean(),
                cellWrapper.RightCellQuery);
        }

        internal LeafCellTreeNode(ViewgenContext context, LeftCellWrapper cellWrapper, FragmentQuery rightFragmentQuery)
            : base(context)
        {
            m_cellWrapper = cellWrapper;
            m_rightFragmentQuery = rightFragmentQuery;
        }

        internal static readonly IEqualityComparer<LeafCellTreeNode> EqualityComparer = new LeafCellTreeNodeComparer();

        // The cell at the leaf level
        private readonly LeftCellWrapper m_cellWrapper;
        private readonly FragmentQuery m_rightFragmentQuery;

        internal LeftCellWrapper LeftCellWrapper
        {
            get { return m_cellWrapper; }
        }

        internal override MemberDomainMap RightDomainMap
        {
            get { return m_cellWrapper.RightDomainMap; }
        }

        // effects: See CellTreeNode.FragmentQuery
        internal override FragmentQuery LeftFragmentQuery
        {
            get { return m_cellWrapper.FragmentQuery; }
        }

        internal override FragmentQuery RightFragmentQuery
        {
            get
            {
                Debug.Assert(m_rightFragmentQuery != null, "Unassigned right fragment query");
                return m_rightFragmentQuery;
            }
        }

        // effects: See CellTreeNode.Attributes
        internal override Set<MemberPath> Attributes
        {
            get { return m_cellWrapper.Attributes; }
        }

        // effects: See CellTreeNode.Children
        internal override List<CellTreeNode> Children
        {
            get { return new List<CellTreeNode>(); }
        }

        // effects: See CellTreeNode.OpType
        internal override CellTreeOpType OpType
        {
            get { return CellTreeOpType.Leaf; }
        }

        internal override int NumProjectedSlots
        {
            get { return LeftCellWrapper.RightCellQuery.NumProjectedSlots; }
        }

        internal override int NumBoolSlots
        {
            get { return LeftCellWrapper.RightCellQuery.NumBoolVars; }
        }

        internal override TOutput Accept<TInput, TOutput>(CellTreeVisitor<TInput, TOutput> visitor, TInput param)
        {
            return visitor.VisitLeaf(this, param);
        }

        internal override TOutput Accept<TInput, TOutput>(SimpleCellTreeVisitor<TInput, TOutput> visitor, TInput param)
        {
            return visitor.VisitLeaf(this, param);
        }

        internal override bool IsProjectedSlot(int slot)
        {
            var cellQuery = LeftCellWrapper.RightCellQuery;
            if (IsBoolSlot(slot))
            {
                return cellQuery.GetBoolVar(SlotToBoolIndex(slot)) != null;
            }
            else
            {
                return cellQuery.ProjectedSlotAt(slot) != null;
            }
        }

        internal override CqlBlock ToCqlBlock(
            bool[] requiredSlots, CqlIdentifiers identifiers, ref int blockAliasNum, ref List<WithRelationship> withRelationships)
        {
            // Get the projected slots and the boolean expressions
            var totalSlots = requiredSlots.Length;
            var cellQuery = LeftCellWrapper.RightCellQuery;

            var projectedSlots = new SlotInfo[totalSlots];
            Debug.Assert(
                cellQuery.NumProjectedSlots + cellQuery.NumBoolVars == totalSlots,
                "Wrong number of projected slots in node");

            Debug.Assert(
                cellQuery.NumProjectedSlots == ProjectedSlotMap.Count,
                "Different number of slots in cell query and what we have mappings for");
            // Add the regular fields
            for (var i = 0; i < cellQuery.NumProjectedSlots; i++)
            {
                var slot = cellQuery.ProjectedSlotAt(i);
                // If the slot is not null, we will project it
                // For extents, we say that all requiredlots are the only the
                // ones that are CLR non-null. Recall that "real" nulls are
                // handled by having a CellConstant.Null in ConstantSlot
                if (requiredSlots[i]
                    && slot == null)
                {
                    var memberPath = ProjectedSlotMap[i];
                    var defaultValue =
                        new ConstantProjectedSlot(Domain.GetDefaultValueForMemberPath(memberPath, GetLeaves(), ViewgenContext.Config));
                    cellQuery.FixMissingSlotAsDefaultConstant(i, defaultValue);
                    slot = defaultValue;
                }
                var slotInfo = new SlotInfo(
                    requiredSlots[i], slot != null,
                    slot, ProjectedSlotMap[i]);
                projectedSlots[i] = slotInfo;
            }

            // Add the boolean fields
            for (var boolNum = 0; boolNum < cellQuery.NumBoolVars; boolNum++)
            {
                var expr = cellQuery.GetBoolVar(boolNum);
                BooleanProjectedSlot boolSlot;
                if (expr != null)
                {
                    boolSlot = new BooleanProjectedSlot(expr, identifiers, boolNum);
                }
                else
                {
                    boolSlot = new BooleanProjectedSlot(BoolExpression.False, identifiers, boolNum);
                }
                var slotIndex = BoolIndexToSlot(boolNum);
                var slotInfo = new SlotInfo(
                    requiredSlots[slotIndex], expr != null,
                    boolSlot, null);
                projectedSlots[slotIndex] = slotInfo;
            }

            // See if we are generating a query view and whether there are any collocated foreign keys for which
            // we have to add With statements.
            IEnumerable<SlotInfo> totalProjectedSlots = projectedSlots;
            if ((cellQuery.Extent.EntityContainer.DataSpace == DataSpace.SSpace)
                && (m_cellWrapper.LeftExtent.BuiltInTypeKind == BuiltInTypeKind.EntitySet))
            {
                var associationSetMaps =
                    ViewgenContext.EntityContainerMapping.GetRelationshipSetMappingsFor(m_cellWrapper.LeftExtent, cellQuery.Extent);
                var foreignKeySlots = new List<SlotInfo>();
                foreach (var collocatedAssociationSetMap in associationSetMaps)
                {
                    WithRelationship withRelationship;
                    if (TryGetWithRelationship(
                        collocatedAssociationSetMap, m_cellWrapper.LeftExtent, cellQuery.SourceExtentMemberPath, ref foreignKeySlots,
                        out withRelationship))
                    {
                        withRelationships.Add(withRelationship);
                        totalProjectedSlots = projectedSlots.Concat(foreignKeySlots);
                    }
                }
            }
            var result = new ExtentCqlBlock(
                cellQuery.Extent, cellQuery.SelectDistinctFlag, totalProjectedSlots.ToArray(),
                cellQuery.WhereClause, identifiers, ++blockAliasNum);
            return result;
        }

        private static bool TryGetWithRelationship(
            AssociationSetMapping collocatedAssociationSetMap,
            EntitySetBase thisExtent,
            MemberPath sRootNode,
            ref List<SlotInfo> foreignKeySlots,
            out WithRelationship withRelationship)
        {
            DebugCheck.NotNull(foreignKeySlots);
            withRelationship = null;

            //Get the map for foreign key end
            var foreignKeyEndMap = GetForeignKeyEndMapFromAssociationMap(collocatedAssociationSetMap);
            if (foreignKeyEndMap == null
                || foreignKeyEndMap.AssociationEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many)
            {
                return false;
            }

            var toEnd = (AssociationEndMember)foreignKeyEndMap.AssociationEnd;
            var fromEnd = MetadataHelper.GetOtherAssociationEnd(toEnd);
            var toEndEntityType = (EntityType)((RefType)(toEnd.TypeUsage.EdmType)).ElementType;
            var fromEndEntityType = (EntityType)(((RefType)fromEnd.TypeUsage.EdmType).ElementType);

            // Get the member path for AssociationSet
            var associationSet = (AssociationSet)collocatedAssociationSetMap.Set;
            var prefix = new MemberPath(associationSet, toEnd);

            // Collect the member paths for edm scalar properties that belong to the target entity key.
            // These will be used as part of WITH RELATIONSHIP.
            // Get the key properties from edm type since the query parser depends on the order of key members
            var propertyMaps = foreignKeyEndMap.PropertyMappings.Cast<ScalarPropertyMapping>();
            var toEndEntityKeyMemberPaths = new List<MemberPath>();
            foreach (EdmProperty edmProperty in toEndEntityType.KeyMembers)
            {
                var scalarPropertyMaps = propertyMaps.Where(propMap => (propMap.Property.Equals(edmProperty)));
                Debug.Assert(scalarPropertyMaps.Count() == 1, "Can't Map the same column multiple times in the same end");
                var scalarPropertyMap = scalarPropertyMaps.First();

                // Create SlotInfo for Foreign Key member that needs to be projected.
                var sSlot = new MemberProjectedSlot(new MemberPath(sRootNode, scalarPropertyMap.Column));
                var endMemberKeyPath = new MemberPath(prefix, edmProperty);
                toEndEntityKeyMemberPaths.Add(endMemberKeyPath);
                foreignKeySlots.Add(new SlotInfo(true, true, sSlot, endMemberKeyPath));
            }

            // Parent assignable from child: Ensures they are in the same hierarchy.
            if (thisExtent.ElementType.IsAssignableFrom(fromEndEntityType))
            {
                // Now create the WITH RELATIONSHIP with all the needed info.
                withRelationship = new WithRelationship(
                    associationSet, fromEnd, fromEndEntityType, toEnd, toEndEntityType, toEndEntityKeyMemberPaths);
                return true;
            }
            else
            {
                return false;
            }
        }

        //Gets the end that is not mapped to the primary key of the table
        private static EndPropertyMapping GetForeignKeyEndMapFromAssociationMap(
            AssociationSetMapping collocatedAssociationSetMap)
        {
            var mapFragment = collocatedAssociationSetMap.TypeMappings.First().MappingFragments.First();
            var storeEntitySet = (collocatedAssociationSetMap.StoreEntitySet);
            IEnumerable<EdmMember> keyProperties = storeEntitySet.ElementType.KeyMembers;
            //Find the end that's mapped to primary key
            foreach (EndPropertyMapping endMap in mapFragment.PropertyMappings)
            {
                var endStoreMembers = endMap.StoreProperties;
                if (endStoreMembers.SequenceEqual(keyProperties, EqualityComparer<EdmMember>.Default))
                {
                    //Return the map for the other end since that is the foreign key end
                    var otherEnds = mapFragment.PropertyMappings.OfType<EndPropertyMapping>().Where(eMap => (!eMap.Equals(endMap)));
                    Debug.Assert(otherEnds.Count() == 1);
                    return otherEnds.First();
                }
            }
            //This is probably defensive, but there should be no problem in falling back on the 
            //AssociationSetMap if collocated foreign key is not found for some reason.
            return null;
        }

        // effects: See CellTreeNode.ToString
        internal override void ToCompactString(StringBuilder stringBuilder)
        {
            m_cellWrapper.ToCompactString(stringBuilder);
        }

        // A comparer that equates leaf nodes if the wrapper is the same
        private class LeafCellTreeNodeComparer : IEqualityComparer<LeafCellTreeNode>
        {
            public bool Equals(LeafCellTreeNode left, LeafCellTreeNode right)
            {
                // Quick check with references
                if (ReferenceEquals(left, right))
                {
                    // Gets the Null and Undefined case as well
                    return true;
                }
                // One of them is non-null at least
                if (left == null
                    || right == null)
                {
                    return false;
                }
                // Both are non-null at this point
                return left.m_cellWrapper.Equals(right.m_cellWrapper);
            }

            public int GetHashCode(LeafCellTreeNode node)
            {
                return node.m_cellWrapper.GetHashCode();
            }
        }
    }
}
