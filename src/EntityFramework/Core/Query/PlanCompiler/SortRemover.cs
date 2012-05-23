namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;

    /// <summary>
    /// Removes all sort nodes from the given command except for the top most one 
    /// (the child of the root PhysicalProjectOp node) if any
    /// </summary>
    internal class SortRemover : BasicOpVisitorOfNode
    {
        #region Private members

        private readonly Command m_command;

        /// <summary>
        /// The only sort node that should not be removed, if any
        /// </summary>
        private readonly Node m_topMostSort;

        /// <summary>
        /// Keeps track of changed nodes to allow to only recompute node info when needed.
        /// </summary>
        private readonly HashSet<Node> changedNodes = new HashSet<Node>();

        #endregion

        #region Constructor

        private SortRemover(Command command, Node topMostSort)
        {
            m_command = command;
            m_topMostSort = topMostSort;
        }

        #endregion

        #region Entry point

        internal static void Process(Command command)
        {
            Node topMostSort;
            if (command.Root.Child0 != null
                && command.Root.Child0.Op.OpType == OpType.Sort)
            {
                topMostSort = command.Root.Child0;
            }
            else
            {
                topMostSort = null;
            }
            var sortRemover = new SortRemover(command, topMostSort);
            command.Root = sortRemover.VisitNode(command.Root);
        }

        #endregion

        #region Visitor Helpers

        /// <summary>
        /// Iterates over all children.
        /// If any of the children changes, update the node info.
        /// This is safe to do because the only way a child can change is 
        /// if it is a sort node that needs to be removed. The nodes whose children have
        /// chagnged also get tracked.
        /// </summary>
        /// <param name="n">The current node</param>
        protected override void VisitChildren(Node n)
        {
            var anyChanged = false;
            for (var i = 0; i < n.Children.Count; i++)
            {
                var originalChild = n.Children[i];
                n.Children[i] = VisitNode(n.Children[i]);
                if (!ReferenceEquals(originalChild, n.Children[i])
                    || changedNodes.Contains(originalChild))
                {
                    anyChanged = true;
                }
            }
            if (anyChanged)
            {
                m_command.RecomputeNodeInfo(n);
                changedNodes.Add(n);
            }
        }

        #endregion

        #region Visitors

        /// <summary>
        /// If the given node is not the top most SortOp node remove it. 
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(SortOp op, Node n)
        {
            VisitChildren(n);
            Node result;

            if (ReferenceEquals(n, m_topMostSort))
            {
                result = n;
            }
            else
            {
                result = n.Child0;
            }
            return result;
        }

        #endregion

        #region

        #endregion
    }
}
