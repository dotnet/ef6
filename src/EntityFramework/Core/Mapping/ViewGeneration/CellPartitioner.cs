// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping.ViewGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.Update.Internal;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Validation;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Text;
    using CellGroup = System.Data.Entity.Core.Common.Utils.Set<Structures.Cell>;

    // This class is responsible for partitioning cells into groups of cells
    // that are related and for which view generation needs to be done together
    internal class CellPartitioner : InternalBase
    {
        #region Constructor

        // effects: Creates a partitioner for cells with extra information
        // about foreign key constraints
        internal CellPartitioner(IEnumerable<Cell> cells, IEnumerable<ForeignConstraint> foreignKeyConstraints)
        {
            m_foreignKeyConstraints = foreignKeyConstraints;
            m_cells = cells;
        }

        #endregion

        #region Fields

        private readonly IEnumerable<Cell> m_cells;
        private readonly IEnumerable<ForeignConstraint> m_foreignKeyConstraints;

        #endregion

        #region Available Methods

        // effects: Given a list of cells, segments them into multiple
        // "groups" such that view generation (including validation) of one
        // group can be done independently of another group. Returns the
        // groups as a list (uses the foreign key information as well)
        internal List<CellGroup> GroupRelatedCells()
        {
            // If two cells share the same C or S, we place them in the same group
            // For each cell, determine the Cis and Sis that it refers
            // to. For every Ci (Si), keep track of the cells that Ci is
            // contained in. At the end, run through the Cis and Sis and do a
            // "connected components" algorithm to determine partitions

            // Now form a graph between different cells -- then compute the connected
            // components in it
            var graph = new UndirectedGraph<Cell>(EqualityComparer<Cell>.Default);

            var alreadyAddedCells = new List<Cell>();
            // For each extent, add an edge between it and all previously
            // added extents with which it overlaps

            foreach (var cell in m_cells)
            {
                graph.AddVertex(cell);
                // Add an edge from this cell to the already added cells
                var firstCExtent = cell.CQuery.Extent;
                var firstSExtent = cell.SQuery.Extent;
                foreach (var existingCell in alreadyAddedCells)
                {
                    var secondCExtent = existingCell.CQuery.Extent;
                    var secondSExtent = existingCell.SQuery.Extent;

                    // Add an edge between cell and existingCell if
                    // * They have the same C or S extent
                    // * They are linked via a foreign key between the S extents
                    // * They are linked via a relationship
                    var sameExtent = secondCExtent.Equals(firstCExtent) || secondSExtent.Equals(firstSExtent);
                    var linkViaForeignKey = OverlapViaForeignKeys(cell, existingCell);
                    var linkViaRelationship = AreCellsConnectedViaRelationship(cell, existingCell);

                    if (sameExtent || linkViaForeignKey || linkViaRelationship)
                    {
                        graph.AddEdge(existingCell, cell);
                    }
                }
                alreadyAddedCells.Add(cell);
            }

            // Now determine the connected components of this graph
            var result = GenerateConnectedComponents(graph);
            return result;
        }

        #endregion

        #region Private Methods

        // effects: Returns true iff cell1 is an extent at the end of cell2's
        // relationship set or vice versa
        private static bool AreCellsConnectedViaRelationship(Cell cell1, Cell cell2)
        {
            var cRelationSet1 = cell1.CQuery.Extent as AssociationSet;
            var cRelationSet2 = cell2.CQuery.Extent as AssociationSet;
            if (cRelationSet1 != null
                && MetadataHelper.IsExtentAtSomeRelationshipEnd(cRelationSet1, cell2.CQuery.Extent))
            {
                return true;
            }
            if (cRelationSet2 != null
                && MetadataHelper.IsExtentAtSomeRelationshipEnd(cRelationSet2, cell1.CQuery.Extent))
            {
                return true;
            }
            return false;
        }

        // effects: Given a graph of cell groups, returns a list of cellgroup
        // such that each cellgroup contains all the cells that are in the
        // same connected component
        private static List<CellGroup> GenerateConnectedComponents(UndirectedGraph<Cell> graph)
        {
            var groupMap = graph.GenerateConnectedComponents();

            // Run through the list of groups and generate the merged groups
            var result = new List<CellGroup>();
            foreach (var setNum in groupMap.Keys)
            {
                var cellsInComponent = groupMap.ListForKey(setNum);
                var component = new CellGroup(cellsInComponent);
                result.Add(component);
            }
            return result;
        }

        // effects: Returns true iff there is a foreign key constraint
        // between cell1 and cell2's S extents
        private bool OverlapViaForeignKeys(Cell cell1, Cell cell2)
        {
            var sExtent1 = cell1.SQuery.Extent;
            var sExtent2 = cell2.SQuery.Extent;

            foreach (var constraint in m_foreignKeyConstraints)
            {
                if (sExtent1.Equals(constraint.ParentTable) && sExtent2.Equals(constraint.ChildTable)
                    ||
                    sExtent2.Equals(constraint.ParentTable) && sExtent1.Equals(constraint.ChildTable))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        internal override void ToCompactString(StringBuilder builder)
        {
            Cell.CellsToBuilder(builder, m_cells);
        }
    }
}
