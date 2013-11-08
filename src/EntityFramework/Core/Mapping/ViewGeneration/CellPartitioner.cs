// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.Update.Internal;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Validation;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using System.Text;
    using CellGroup = System.Data.Entity.Core.Common.Utils.Set<Structures.Cell>;

    // This class is responsible for partitioning cells into groups of cells
    // that are related and for which view generation needs to be done together
    internal class CellPartitioner : InternalBase
    {
        // effects: Creates a partitioner for cells with extra information
        // about foreign key constraints
        internal CellPartitioner(IEnumerable<Cell> cells, IEnumerable<ForeignConstraint> foreignKeyConstraints)
        {
            m_foreignKeyConstraints = foreignKeyConstraints;
            m_cells = cells;
        }

        private readonly IEnumerable<Cell> m_cells;
        private readonly IEnumerable<ForeignConstraint> m_foreignKeyConstraints;

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

            var extentGraph = new UndirectedGraph<EntitySetBase>(EqualityComparer<EntitySetBase>.Default);
            var extentToCell = new Dictionary<EntitySetBase, Set<Cell>>(EqualityComparer<EntitySetBase>.Default);

            foreach (var cell in m_cells)
            {
                foreach (var extent in new[] { cell.CQuery.Extent, cell.SQuery.Extent })
                {
                    Set<Cell> cellsWithExtent;
                    if (!extentToCell.TryGetValue(extent, out cellsWithExtent))
                    {
                        extentToCell[extent] = cellsWithExtent = new Set<Cell>();
                    }
                    cellsWithExtent.Add(cell);
                    extentGraph.AddVertex(extent);
                }
                extentGraph.AddEdge(cell.CQuery.Extent, cell.SQuery.Extent);

                var associationSetExtent = cell.CQuery.Extent as AssociationSet;
                if (associationSetExtent != null)
                {
                    if (associationSetExtent.ElementType.IsManyToMany())
                    {
                        foreach (var end in associationSetExtent.AssociationSetEnds)
                        {
                            extentGraph.AddEdge(end.EntitySet, associationSetExtent);
                        }
                    }
                    else
                    {
                        EntitySetBase prev = null;
                        foreach (var end in associationSetExtent.AssociationSetEnds)
                        {
                            if (prev != null)
                            {
                                extentGraph.AddEdge(prev, end.EntitySet);
                            }
                            prev = end.EntitySet;
                        }
                    }
                }
            }

            foreach (var fk in m_foreignKeyConstraints)
            {
                extentGraph.AddEdge(fk.ChildTable, fk.ParentTable);
            }

            var groupMap = extentGraph.GenerateConnectedComponents();
            var result = new List<CellGroup>();
            foreach (var setNum in groupMap.Keys)
            {
                var cellSets = groupMap.ListForKey(setNum).Select(e => extentToCell[e]);
                var component = new CellGroup();
                foreach (var cellSet in cellSets)
                {
                    component.AddRange(cellSet);
                }

                result.Add(component);
            }

            return result;
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            Cell.CellsToBuilder(builder, m_cells);
        }
    }
}
