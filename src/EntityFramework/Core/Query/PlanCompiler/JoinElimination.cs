// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;

    /// <summary>
    ///     The JoinElimination module is intended to do just that - eliminate unnecessary joins. 
    ///     This module deals with the following kinds of joins
    ///     * Self-joins: The join can be eliminated, and either of the table instances can be 
    ///     used instead
    ///     * Implied self-joins: Same as above
    ///     * PK-FK joins: (More generally, UniqueKey-FK joins): Eliminate the join, and use just the FK table, if no 
    ///     column of the PK table is used (other than the join condition)
    ///     * PK-PK joins: Eliminate the right side table, if we have a left-outer join
    /// </summary>
    internal class JoinElimination : BasicOpVisitorOfNode
    {
        #region private state

        private readonly PlanCompiler m_compilerState;

        private Command Command
        {
            get { return m_compilerState.Command; }
        }

        private ConstraintManager ConstraintManager
        {
            get { return m_compilerState.ConstraintManager; }
        }

        private readonly Dictionary<Node, Node> m_joinGraphUnnecessaryMap = new Dictionary<Node, Node>();
        private readonly VarRemapper m_varRemapper;
        private bool m_treeModified;
        private readonly VarRefManager m_varRefManager;

        #endregion

        #region constructors

        private JoinElimination(PlanCompiler compilerState)
        {
            m_compilerState = compilerState;
            m_varRemapper = new VarRemapper(m_compilerState.Command);
            m_varRefManager = new VarRefManager(m_compilerState.Command);
        }

        #endregion

        #region public surface

        internal static bool Process(PlanCompiler compilerState)
        {
            var je = new JoinElimination(compilerState);
            je.Process();
            return je.m_treeModified;
        }

        #endregion

        #region private methods

        /// <summary>
        ///     Invokes the visitor
        /// </summary>
        private void Process()
        {
            Command.Root = VisitNode(Command.Root);
        }

        #region JoinHelpers

        #region Building JoinGraphs

        /// <summary>
        ///     Do we need to build a join graph for this node - returns false, if we've already
        ///     processed this
        /// </summary>
        /// <param name="joinNode"> </param>
        /// <returns> </returns>
        private bool NeedsJoinGraph(Node joinNode)
        {
            return !m_joinGraphUnnecessaryMap.ContainsKey(joinNode);
        }

        /// <summary>
        ///     Do the real processing of the join graph.
        /// </summary>
        /// <param name="joinNode"> current join node </param>
        /// <returns> modified join node </returns>
        private Node ProcessJoinGraph(Node joinNode)
        {
            // Build the join graph
            var joinGraph = new JoinGraph(Command, ConstraintManager, m_varRefManager, joinNode);

            // Get the transformed node tree
            VarMap remappedVars;
            Dictionary<Node, Node> processedNodes;
            var newNode = joinGraph.DoJoinElimination(out remappedVars, out processedNodes);

            // Get the set of vars that need to be renamed
            foreach (var kv in remappedVars)
            {
                m_varRemapper.AddMapping(kv.Key, kv.Value);
            }
            // get the set of nodes that have already been processed
            foreach (var n in processedNodes.Keys)
            {
                m_joinGraphUnnecessaryMap[n] = n;
            }

            return newNode;
        }

        /// <summary>
        ///     Default handler for a node. Simply visits the children, then handles any var
        ///     remapping, and then recomputes the node info
        /// </summary>
        /// <param name="n"> </param>
        /// <returns> </returns>
        private Node VisitDefaultForAllNodes(Node n)
        {
            VisitChildren(n);
            m_varRemapper.RemapNode(n);
            Command.RecomputeNodeInfo(n);
            return n;
        }

        #endregion

        #endregion

        #region Visitor overrides

        /// <summary>
        ///     Invokes default handling for a node and adds the child-parent tracking info to the VarRefManager.
        /// </summary>
        /// <param name="n"> </param>
        /// <returns> </returns>
        protected override Node VisitDefault(Node n)
        {
            m_varRefManager.AddChildren(n);
            return VisitDefaultForAllNodes(n);
        }

        #region RelOps

        #region JoinOps

        /// <summary>
        ///     Build a join graph for this node for this node if necessary, and process it
        /// </summary>
        /// <param name="op"> current join op </param>
        /// <param name="joinNode"> current join node </param>
        /// <returns> </returns>
        protected override Node VisitJoinOp(JoinBaseOp op, Node joinNode)
        {
            Node newNode;

            // Build and process a join graph if necessary
            if (NeedsJoinGraph(joinNode))
            {
                newNode = ProcessJoinGraph(joinNode);
                if (newNode != joinNode)
                {
                    m_treeModified = true;
                }
            }
            else
            {
                newNode = joinNode;
            }

            // Now do the default processing (ie) visit the children, compute the nodeinfo etc.
            return VisitDefaultForAllNodes(newNode);
        }

        #endregion

        #endregion

        #endregion

        #endregion
    }
}
