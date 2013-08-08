// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Additional information for a node.
    /// AugmentedNode - this is the base class for all annotations. This class
    /// wraps a Node, an id for the node (where the "id" is assigned in DFS order),
    /// and a list of children. All Nodes that are neither joins, nor scanTables
    /// are represented by this class
    /// </summary>
    internal class AugmentedNode
    {
        #region private state

        private readonly int m_id;
        private readonly Node m_node;
        protected AugmentedNode m_parent;
        private readonly List<AugmentedNode> m_children;
        private readonly List<JoinEdge> m_joinEdges = new List<JoinEdge>();

        #endregion

        #region constructors

        /// <summary>
        /// basic constructor
        /// </summary>
        /// <param name="id"> Id for this node </param>
        /// <param name="node"> current node </param>
        internal AugmentedNode(int id, Node node)
            : this(id, node, new List<AugmentedNode>())
        {
        }

        /// <summary>
        /// Yet another constructor
        /// </summary>
        /// <param name="id"> Id for this node </param>
        /// <param name="node"> current node </param>
        /// <param name="children"> list of children </param>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal AugmentedNode(int id, Node node, List<AugmentedNode> children)
        {
            m_id = id;
            m_node = node;
            m_children = children;
            PlanCompiler.Assert(children != null, "null children (gasp!)");
            foreach (var chi in m_children)
            {
                chi.m_parent = this;
            }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Id of this node
        /// </summary>
        internal int Id
        {
            get { return m_id; }
        }

        /// <summary>
        /// The node
        /// </summary>
        internal Node Node
        {
            get { return m_node; }
        }

        /// <summary>
        /// Parent node
        /// </summary>
        internal AugmentedNode Parent
        {
            get { return m_parent; }
        }

        /// <summary>
        /// List of children
        /// </summary>
        internal List<AugmentedNode> Children
        {
            get { return m_children; }
        }

        /// <summary>
        /// List of directed edges in which:
        /// - If this is an AugmentedTableNode, it is the "left" table
        /// - If it is an AugumentedJoinNode, it is the join on which the edge is based
        /// </summary>
        internal List<JoinEdge> JoinEdges
        {
            get { return m_joinEdges; }
        }

        #endregion
    }
}
