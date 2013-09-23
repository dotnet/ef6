// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    internal class ViewgenContext : InternalBase
    {
        private readonly ConfigViewGenerator m_config;
        private readonly ViewTarget m_viewTarget;

        // Extent for which the view is being generated
        private readonly EntitySetBase m_extent;

        // Different maps for members
        private readonly MemberMaps m_memberMaps;
        private readonly EdmItemCollection m_edmItemCollection;
        private readonly EntityContainerMapping m_entityContainerMapping;

        // The normalized cells that are created
        private List<LeftCellWrapper> m_cellWrappers;

        // Implicit constraints between members in queries based on schema. E.g., p.Addr IS NOT NULL <=> p IS OF Customer
        private readonly FragmentQueryProcessor m_leftFragmentQP;

        // In addition to constraints for each right extent contains constraints due to associations
        private readonly FragmentQueryProcessor m_rightFragmentQP;

        private readonly CqlIdentifiers m_identifiers;

        // Maps (left) queries to their rewritings in terms of views
        private readonly Dictionary<FragmentQuery, Tile<FragmentQuery>> m_rewritingCache;

        internal ViewgenContext(
            ViewTarget viewTarget, EntitySetBase extent, IList<Cell> extentCells,
            CqlIdentifiers identifiers, ConfigViewGenerator config, MemberDomainMap queryDomainMap,
            MemberDomainMap updateDomainMap, EntityContainerMapping entityContainerMapping)
        {
            foreach (var cell in extentCells)
            {
                Debug.Assert(extent.Equals(cell.GetLeftQuery(viewTarget).Extent));
                Debug.Assert(cell.CQuery.NumProjectedSlots == cell.SQuery.NumProjectedSlots);
            }

            m_extent = extent;
            m_viewTarget = viewTarget;
            m_config = config;
            m_edmItemCollection = entityContainerMapping.StorageMappingItemCollection.EdmItemCollection;
            m_entityContainerMapping = entityContainerMapping;
            m_identifiers = identifiers;

            // create a copy of updateDomainMap so generation of query views later on is not affected
            // it is modified in QueryRewriter.AdjustMemberDomainsForUpdateViews
            updateDomainMap = updateDomainMap.MakeCopy();

            // Create a signature generator that handles all the
            // multiconstant work and generating the signatures
            var domainMap = viewTarget == ViewTarget.QueryView ? queryDomainMap : updateDomainMap;

            m_memberMaps = new MemberMaps(
                viewTarget, MemberProjectionIndex.Create(extent, m_edmItemCollection), queryDomainMap, updateDomainMap);

            // Create left fragment KB: includes constraints for the extent to be constructed
            var leftKB = new FragmentQueryKBChaseSupport();
            leftKB.CreateVariableConstraints(extent, domainMap, m_edmItemCollection);
            m_leftFragmentQP = new FragmentQueryProcessor(leftKB);
            m_rewritingCache = new Dictionary<FragmentQuery, Tile<FragmentQuery>>(
                FragmentQuery.GetEqualityComparer(m_leftFragmentQP));

            // Now using the signatures, create new cells such that
            // "extent's" query (C or S) is described in terms of multiconstants
            if (!CreateLeftCellWrappers(extentCells, viewTarget))
            {
                return;
            }

            // Create right fragment KB: includes constraints for all extents and association roles of right queries
            var rightKB = new FragmentQueryKBChaseSupport();
            var rightDomainMap = viewTarget == ViewTarget.QueryView ? updateDomainMap : queryDomainMap;
            foreach (var leftCellWrapper in m_cellWrappers)
            {
                var rightExtent = leftCellWrapper.RightExtent;
                rightKB.CreateVariableConstraints(rightExtent, rightDomainMap, m_edmItemCollection);
                rightKB.CreateAssociationConstraints(rightExtent, rightDomainMap, m_edmItemCollection);
            }

            if (m_viewTarget == ViewTarget.UpdateView)
            {
                CreateConstraintsForForeignKeyAssociationsAffectingThisWrapper(rightKB, rightDomainMap);
            }

            m_rightFragmentQP = new FragmentQueryProcessor(rightKB);

            // Check for concurrency control tokens
            if (m_viewTarget == ViewTarget.QueryView)
            {
                CheckConcurrencyControlTokens();
            }
            // For backward compatibility -
            // order wrappers by increasing domain size, decreasing number of attributes
            m_cellWrappers.Sort(LeftCellWrapper.Comparer);
        }

        // <summary>
        // Find the Foreign Key Associations that relate EntitySets used in these left cell wrappers and
        // add any equivalence facts between sets implied by 1:1 associations.
        // We can collect other implication facts but we don't have a scenario that needs them( yet ).
        // </summary>
        private void CreateConstraintsForForeignKeyAssociationsAffectingThisWrapper(
            FragmentQueryKB rightKB, MemberDomainMap rightDomainMap)
        {
            var oneToOneForeignKeyAssociationSetsForThisWrapper
                = new OneToOneFkAssociationsForEntitiesFilter()
                    .Filter(
                        m_cellWrappers.Select(it => it.RightExtent).OfType<EntitySet>().Select(it => it.ElementType).ToList(),
                        m_entityContainerMapping.EdmEntityContainer.BaseEntitySets.OfType<AssociationSet>());

            // Collect the facts for the foreign key association sets that are 1:1 and affecting this wrapper
            foreach (var assocSet in oneToOneForeignKeyAssociationSetsForThisWrapper)
            {
                rightKB.CreateEquivalenceConstraintForOneToOneForeignKeyAssociation(assocSet, rightDomainMap);
            }
        }

        internal class OneToOneFkAssociationsForEntitiesFilter
        {
            public virtual IEnumerable<AssociationSet> Filter(
                IList<EntityType> entityTypes, IEnumerable<AssociationSet> associationSets)
            {
                DebugCheck.NotNull(entityTypes);
                DebugCheck.NotNull(associationSets);

                return associationSets
                    .Where(
                        a => a.ElementType.IsForeignKey
                             && a.ElementType.AssociationEndMembers
                                 .All(
                                     aem => (aem.RelationshipMultiplicity == RelationshipMultiplicity.One)
                                            && entityTypes.Contains(aem.GetEntityType())));
            }
        }

        internal ViewTarget ViewTarget
        {
            get { return m_viewTarget; }
        }

        internal MemberMaps MemberMaps
        {
            get { return m_memberMaps; }
        }

        // effects: Returns the extent for which the cells have been normalized
        internal EntitySetBase Extent
        {
            get { return m_extent; }
        }

        internal ConfigViewGenerator Config
        {
            get { return m_config; }
        }

        internal CqlIdentifiers CqlIdentifiers
        {
            get { return m_identifiers; }
        }

        internal EdmItemCollection EdmItemCollection
        {
            get { return m_edmItemCollection; }
        }

        internal FragmentQueryProcessor LeftFragmentQP
        {
            get { return m_leftFragmentQP; }
        }

        internal FragmentQueryProcessor RightFragmentQP
        {
            get { return m_rightFragmentQP; }
        }

        // effects: Returns all wrappers that were originally relevant for
        // this extent
        internal List<LeftCellWrapper> AllWrappersForExtent
        {
            get { return m_cellWrappers; }
        }

        internal EntityContainerMapping EntityContainerMapping
        {
            get { return m_entityContainerMapping; }
        }

        // effects: Returns the cached rewriting of (left) queries in terms of views, if any
        internal bool TryGetCachedRewriting(FragmentQuery query, out Tile<FragmentQuery> rewriting)
        {
            return m_rewritingCache.TryGetValue(query, out rewriting);
        }

        // effects: Records the cached rewriting of (left) queries in terms of views
        internal void SetCachedRewriting(FragmentQuery query, Tile<FragmentQuery> rewriting)
        {
            m_rewritingCache[query] = rewriting;
        }

        // <summary>
        // Checks:
        // 1) Concurrency token is not defined in this Extent's ElementTypes' derived types
        // 2) Members with concurrency token should not have conditions specified
        // </summary>
        private void CheckConcurrencyControlTokens()
        {
            Debug.Assert(m_viewTarget == ViewTarget.QueryView);
            // Get the token fields for this extent

            var extentType = m_extent.ElementType;
            var tokenMembers = MetadataHelper.GetConcurrencyMembersForTypeHierarchy(extentType, m_edmItemCollection);
            var tokenPaths = new Set<MemberPath>(MemberPath.EqualityComparer);
            foreach (var tokenMember in tokenMembers)
            {
                if (!tokenMember.DeclaringType.IsAssignableFrom(extentType))
                {
                    var message = Strings.ViewGen_Concurrency_Derived_Class(tokenMember.Name, tokenMember.DeclaringType.Name, m_extent);
                    var record = new ErrorLog.Record(ViewGenErrorCode.ConcurrencyDerivedClass, message, m_cellWrappers, String.Empty);
                    ExceptionHelpers.ThrowMappingException(record, m_config);
                }
                tokenPaths.Add(new MemberPath(m_extent, tokenMember));
            }

            if (tokenMembers.Count > 0)
            {
                foreach (var wrapper in m_cellWrappers)
                {
                    var conditionMembers = new Set<MemberPath>(
                        wrapper.OnlyInputCell.CQuery.WhereClause.MemberRestrictions.Select(oneOf => oneOf.RestrictedMemberSlot.MemberPath),
                        MemberPath.EqualityComparer);
                    conditionMembers.Intersect(tokenPaths);
                    if (conditionMembers.Count > 0)
                    {
                        // There is a condition on concurrency tokens. Throw an exception.
                        var builder = new StringBuilder();
                        builder.AppendLine(
                            Strings.ViewGen_Concurrency_Invalid_Condition(
                                MemberPath.PropertiesToUserString(conditionMembers, false), m_extent.Name));
                        var record = new ErrorLog.Record(
                            ViewGenErrorCode.ConcurrencyTokenHasCondition, builder.ToString(), new[] { wrapper }, String.Empty);
                        ExceptionHelpers.ThrowMappingException(record, m_config);
                    }
                }
            }
        }

        // effects: Given the cells for the extent (extentCells) along with
        // the signatures (multiconstants + needed attributes) for this extent, generates
        // the left cell wrappers for it extent (viewTarget indicates whether
        // the view is for querying or update purposes
        // Modifies m_cellWrappers to contain this list
        private bool CreateLeftCellWrappers(IList<Cell> extentCells, ViewTarget viewTarget)
        {
            var alignedCells = AlignFields(extentCells, m_memberMaps.ProjectedSlotMap, viewTarget);
            Debug.Assert(alignedCells.Count == extentCells.Count, "Cell counts disagree");

            // Go through all the cells and create cell wrappers that can be used for generating the view
            m_cellWrappers = new List<LeftCellWrapper>();

            for (var i = 0; i < alignedCells.Count; i++)
            {
                var alignedCell = alignedCells[i];
                var left = alignedCell.GetLeftQuery(viewTarget);
                var right = alignedCell.GetRightQuery(viewTarget);

                // Obtain the non-null projected slots into attributes
                var attributes = left.GetNonNullSlots();

                var fromVariable = BoolExpression.CreateLiteral(
                    new CellIdBoolean(m_identifiers, extentCells[i].CellNumber), m_memberMaps.LeftDomainMap);
                var leftFragmentQuery = FragmentQuery.Create(fromVariable, left);

                if (viewTarget == ViewTarget.UpdateView)
                {
                    leftFragmentQuery = m_leftFragmentQP.CreateDerivedViewBySelectingConstantAttributes(leftFragmentQuery)
                                        ?? leftFragmentQuery;
                }

                var leftWrapper = new LeftCellWrapper(
                    m_viewTarget, attributes, leftFragmentQuery, left, right, m_memberMaps,
                    extentCells[i]);
                m_cellWrappers.Add(leftWrapper);
            }
            return true;
        }

        // effects: Align the fields of each cell in mapping using projectedSlotMap that has a mapping 
        // for each member of this extent to the slot number of that member in the projected slots
        // example:
        //    input:  Proj[A,B,"5"] = Proj[F,"7",G]
        //            Proj[C,B]     = Proj[H,I]
        //   output:  m_projectedSlotMap: A -> 0, B -> 1, C -> 2
        //            Proj[A,B,null] = Proj[F,"7",null]
        //            Proj[null,B,C] = Proj[null,I,H]
        private static List<Cell> AlignFields(
            IEnumerable<Cell> cells, MemberProjectionIndex projectedSlotMap,
            ViewTarget viewTarget)
        {
            var outputCells = new List<Cell>();

            // Determine the aligned field for each cell
            // The new cells have ProjectedSlotMap.Count number of fields
            foreach (var cell in cells)
            {
                // If isQueryView is true, we need to consider the C side of
                // the cells; otherwise, we look at the S side. Note that we
                // CANNOT use cell.LeftQuery since that is determined by
                // cell's isQueryView

                // The query for which we are constructing the extent
                var mainQuery = cell.GetLeftQuery(viewTarget);
                var otherQuery = cell.GetRightQuery(viewTarget);

                CellQuery newMainQuery;
                CellQuery newOtherQuery;
                // Create both queries where the projected slot map is used
                // to determine the order of the fields of the mainquery (of
                // course, the otherQuery's fields are aligned automatically)
                mainQuery.CreateFieldAlignedCellQueries(
                    otherQuery, projectedSlotMap,
                    out newMainQuery, out newOtherQuery);

                var outputCell = viewTarget == ViewTarget.QueryView
                                     ? Cell.CreateCS(newMainQuery, newOtherQuery, cell.CellLabel, cell.CellNumber)
                                     : Cell.CreateCS(newOtherQuery, newMainQuery, cell.CellLabel, cell.CellNumber);
                outputCells.Add(outputCell);
            }
            return outputCells;
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            LeftCellWrapper.WrappersToStringBuilder(builder, m_cellWrappers, "Left Celll Wrappers");
        }
    }
}
