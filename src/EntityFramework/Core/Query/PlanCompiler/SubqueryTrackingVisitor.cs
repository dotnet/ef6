// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The SubqueryTracking Visitor serves as a base class for the visitors that may turn
    /// scalar subqueryies into outer-apply subqueries.
    /// </summary>
    internal abstract class SubqueryTrackingVisitor : BasicOpVisitorOfNode
    {
        #region Private State

        protected readonly PlanCompiler m_compilerState;

        protected Command m_command
        {
            get { return m_compilerState.Command; }
        }

        // nested subquery tracking
        protected readonly Stack<Node> m_ancestors = new Stack<Node>();
        private readonly Dictionary<Node, List<Node>> m_nodeSubqueries = new Dictionary<Node, List<Node>>();

        #endregion

        #region Constructor

        protected SubqueryTrackingVisitor(PlanCompiler planCompilerState)
        {
            m_compilerState = planCompilerState;
        }

        #endregion

        #region Subquery Handling

        /// <summary>
        /// Adds a subquery to the list of subqueries for the relOpNode
        /// </summary>
        /// <param name="relOpNode"> the RelOp node </param>
        /// <param name="subquery"> the subquery </param>
        protected void AddSubqueryToRelOpNode(Node relOpNode, Node subquery)
        {
            List<Node> nestedSubqueries;

            // Create an entry in the map if there isn't one already
            if (!m_nodeSubqueries.TryGetValue(relOpNode, out nestedSubqueries))
            {
                nestedSubqueries = new List<Node>();
                m_nodeSubqueries[relOpNode] = nestedSubqueries;
            }
            // add this subquery to the list of currently tracked subqueries
            nestedSubqueries.Add(subquery);
        }

        /// <summary>
        /// Add a subquery to the "parent" relop node
        /// </summary>
        /// <param name="outputVar"> the output var to be used - at the current location - in lieu of the subquery </param>
        /// <param name="subquery"> the subquery to move </param>
        /// <returns> a var ref node for the var returned from the subquery </returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        protected Node AddSubqueryToParentRelOp(Var outputVar, Node subquery)
        {
            var ancestor = FindRelOpAncestor();
            PlanCompiler.Assert(ancestor != null, "no ancestors found?");
            AddSubqueryToRelOpNode(ancestor, subquery);

            subquery = m_command.CreateNode(m_command.CreateVarRefOp(outputVar));
            return subquery;
        }

        /// <summary>
        /// Find the first RelOp node that is in my ancestral path.
        /// If I see a PhysicalOp, then I don't have a RelOp parent
        /// </summary>
        /// <returns> the first RelOp node </returns>
        protected Node FindRelOpAncestor()
        {
            foreach (var n in m_ancestors)
            {
                if (n.Op.IsRelOp)
                {
                    return n;
                }
                else if (n.Op.IsPhysicalOp)
                {
                    return null;
                }
            }
            return null;
        }

        #endregion

        #region Visitor Helpers

        /// <summary>
        /// Extends the base class implementation of VisitChildren.
        /// Wraps the call to visitchildren() by first adding the current node
        /// to the stack of "ancestors", and then popping back the node at the end
        /// </summary>
        /// <param name="n"> Current node </param>
        protected override void VisitChildren(Node n)
        {
            // Push the current node onto the stack
            m_ancestors.Push(n);

            for (var i = 0; i < n.Children.Count; i++)
            {
                n.Children[i] = VisitNode(n.Children[i]);
            }

            m_ancestors.Pop();
        }

        #endregion

        #region Visitor Methods

        #region RelOps

        /// <summary>
        /// Augments a node with a number of OuterApply's - one for each subquery
        /// If S1, S2, ... are the list of subqueries for the node, and D is the
        /// original (driver) input, we convert D into
        /// OuterApply(OuterApply(D, S1), S2), ...
        /// </summary>
        /// <param name="input"> the input (driver) node </param>
        /// <param name="subqueries"> List of subqueries </param>
        /// <param name="inputFirst"> should the input node be first in the apply chain, or the last? </param>
        /// <returns> The resulting node tree </returns>
        private Node AugmentWithSubqueries(Node input, List<Node> subqueries, bool inputFirst)
        {
            Node newNode;
            int subqueriesStartPos;

            if (inputFirst)
            {
                newNode = input;
                subqueriesStartPos = 0;
            }
            else
            {
                newNode = subqueries[0];
                subqueriesStartPos = 1;
            }
            for (var i = subqueriesStartPos; i < subqueries.Count; i++)
            {
                var op = m_command.CreateOuterApplyOp();
                newNode = m_command.CreateNode(op, newNode, subqueries[i]);
            }
            if (!inputFirst)
            {
                // The driver node uses a cross apply to ensure that no results are produced
                // for an empty driver.
                newNode = m_command.CreateNode(m_command.CreateCrossApplyOp(), newNode, input);
            }

            // We may need to perform join elimination
            m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.JoinElimination);
            return newNode;
        }

        /// <summary>
        /// Default processing for RelOps.
        /// - First, we mark the current node as its own ancestor (so that any
        /// subqueries that we detect internally will be added to this node's list)
        /// - then, visit each child
        /// - finally, accumulate all nested subqueries.
        /// - if the current RelOp has only one input, then add the nested subqueries via
        /// Outer apply nodes to this input.
        /// The interesting RelOps are
        /// Project, Filter, GroupBy, Sort,
        /// Should we break this out into separate functions instead?
        /// </summary>
        /// <param name="op"> Current RelOp </param>
        /// <param name="n"> Node to process </param>
        /// <returns> Current subtree </returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "VisitRelOpDefault")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        protected override Node VisitRelOpDefault(RelOp op, Node n)
        {
            VisitChildren(n); // visit all my children first

            // Then identify all the subqueries that have shown up as part of my node
            // Create Apply Nodes for each of these.
            List<Node> nestedSubqueries;
            if (m_nodeSubqueries.TryGetValue(n, out nestedSubqueries)
                && nestedSubqueries.Count > 0)
            {
                // Validate - this must only apply to the following nodes
                PlanCompiler.Assert(
                    n.Op.OpType == OpType.Project || n.Op.OpType == OpType.Filter ||
                    n.Op.OpType == OpType.GroupBy || n.Op.OpType == OpType.GroupByInto,
                    "VisitRelOpDefault: Unexpected op?" + n.Op.OpType);

                var newInputNode = AugmentWithSubqueries(n.Child0, nestedSubqueries, true);
                // Now make this the new input child
                n.Child0 = newInputNode;
            }

            return n;
        }

        /// <summary>
        /// Processing for all JoinOps
        /// </summary>
        /// <param name="n"> Current subtree </param>
        /// <returns> Whether the node was modified </returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "JoinOp")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        protected bool ProcessJoinOp(Node n)
        {
            VisitChildren(n); // visit all my children first

            // then check to see if we have any nested subqueries. This can only 
            // occur in the join condition. 
            // What we'll do in this case is to convert the join condition - "p" into
            //    p -> Exists(Filter(SingleRowTableOp, p))
            // We will then move the subqueries into an outerApply on the SingleRowTable
            List<Node> nestedSubqueries;
            if (!m_nodeSubqueries.TryGetValue(n, out nestedSubqueries))
            {
                return false;
            }

            PlanCompiler.Assert(
                n.Op.OpType == OpType.InnerJoin ||
                n.Op.OpType == OpType.LeftOuterJoin ||
                n.Op.OpType == OpType.FullOuterJoin, "unexpected op?");
            PlanCompiler.Assert(n.HasChild2, "missing second child to JoinOp?");
            var joinCondition = n.Child2;

            var inputNode = m_command.CreateNode(m_command.CreateSingleRowTableOp());
            inputNode = AugmentWithSubqueries(inputNode, nestedSubqueries, true);
            var filterNode = m_command.CreateNode(m_command.CreateFilterOp(), inputNode, joinCondition);
            var existsNode = m_command.CreateNode(m_command.CreateExistsOp(), filterNode);

            n.Child2 = existsNode;
            return true;
        }

        /// <summary>
        /// Visitor for UnnestOp. If the child has any subqueries, we need to convert this
        /// into an
        /// OuterApply(S, Unnest)
        /// unlike the other cases where the OuterApply will appear as the input of the node
        /// </summary>
        /// <param name="op"> the unnestOp </param>
        /// <param name="n"> current subtree </param>
        /// <returns> modified subtree </returns>
        public override Node Visit(UnnestOp op, Node n)
        {
            VisitChildren(n); // visit all my children first

            List<Node> nestedSubqueries;
            if (m_nodeSubqueries.TryGetValue(n, out nestedSubqueries))
            {
                // We pass 'inputFirst = false' since the subqueries contribute to the driver in the unnest,
                // they are not generated by the unnest.
                var newNode = AugmentWithSubqueries(n, nestedSubqueries, false /* inputFirst */);
                return newNode;
            }
            else
            {
                return n;
            }
        }

        #endregion

        #endregion
    }
}
