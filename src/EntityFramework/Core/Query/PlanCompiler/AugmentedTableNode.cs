// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Data.Entity.Core.Query.InternalTrees;

    /// <summary>
    /// Additional information for a "Table" node
    /// AugmentedTableNode - the augmentedTableNode is a subclass of AugmentedNode,
    /// and represents a ScanTable node. In addition to the information above, this
    /// class keeps track of all join edges that this node participates in,
    /// whether this table has been eliminated, and finally, how high in the tree
    /// this node is visible
    /// </summary>
    internal sealed class AugmentedTableNode : AugmentedNode
    {
        #region private state

        private readonly Table m_table;

        // The replacement table 
        private AugmentedTableNode m_replacementTable;

        // Is this table being moved
        private int m_newLocationId;

        // List of columns of this table that are nullable (and must have nulls pruned out)

        #endregion

        #region constructors

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="id"> node id </param>
        /// <param name="node"> scan table node </param>
        internal AugmentedTableNode(int id, Node node)
            : base(id, node)
        {
            var scanTableOp = (ScanTableOp)node.Op;
            m_table = scanTableOp.Table;
            LastVisibleId = id;
            m_replacementTable = this;
            m_newLocationId = id;
        }

        #endregion

        #region public properties

        /// <summary>
        /// The Table
        /// </summary>
        internal Table Table
        {
            get { return m_table; }
        }

        /// <summary>
        /// The highest node (id) at which this table is visible
        /// </summary>
        internal int LastVisibleId { get; set; }

        /// <summary>
        /// Has this table been eliminated
        /// </summary>
        internal bool IsEliminated
        {
            get { return m_replacementTable != this; }
        }

        /// <summary>
        /// The replacement table (if any) for this table
        /// </summary>
        internal AugmentedTableNode ReplacementTable
        {
            get { return m_replacementTable; }
            set { m_replacementTable = value; }
        }

        /// <summary>
        /// New location for this table
        /// </summary>
        internal int NewLocationId
        {
            get { return m_newLocationId; }
            set { m_newLocationId = value; }
        }

        /// <summary>
        /// Has this table "moved" ?
        /// </summary>
        internal bool IsMoved
        {
            get { return m_newLocationId != Id; }
        }

        /// <summary>
        /// Get the list of nullable columns (that require special handling)
        /// </summary>
        internal VarVec NullableColumns { get; set; }

        #endregion
    }
}
