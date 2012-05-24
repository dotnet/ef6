namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;

    /// <summary>
    /// Additional information for a JoinNode 
    ///    AugmentedJoinNode - represents all joins (cross-joins, leftouter, fullouter
    ///        and innerjoins). This class represents a number of column equijoin conditions
    ///        via the LeftVars and RightVars properties, and also keeps track of additional
    ///        (non-equijoin column) join predicates
    ///
    /// </summary>
    internal sealed class AugmentedJoinNode : AugmentedNode
    {
        #region private state

        private readonly List<ColumnVar> m_leftVars;
        private readonly List<ColumnVar> m_rightVars;
        private readonly Node m_otherPredicate;

        #endregion

        #region constructors

        /// <summary>
        /// basic constructor
        /// </summary>
        /// <param name="id">current node id</param>
        /// <param name="node">the join node</param>
        /// <param name="leftChild">left side of the join (innerJoin, LOJ and FOJ only)</param>
        /// <param name="rightChild">right side of the join</param>
        /// <param name="leftVars">left-side equijoin vars</param>
        /// <param name="rightVars">right-side equijoin vars</param>
        /// <param name="otherPredicate">any remaining predicate</param>
        internal AugmentedJoinNode(
            int id, Node node,
            AugmentedNode leftChild, AugmentedNode rightChild,
            List<ColumnVar> leftVars, List<ColumnVar> rightVars,
            Node otherPredicate)
            : this(id, node, new List<AugmentedNode>(new[] { leftChild, rightChild }))
        {
            m_otherPredicate = otherPredicate;
            m_rightVars = rightVars;
            m_leftVars = leftVars;
        }

        /// <summary>
        /// Yet another constructor - used for crossjoins
        /// </summary>
        /// <param name="id">node id</param>
        /// <param name="node">current node</param>
        /// <param name="children">list of children</param>
        internal AugmentedJoinNode(int id, Node node, List<AugmentedNode> children)
            : base(id, node, children)
        {
            m_leftVars = new List<ColumnVar>();
            m_rightVars = new List<ColumnVar>();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Non-equijoin predicate
        /// </summary>
        internal Node OtherPredicate
        {
            get { return m_otherPredicate; }
        }

        /// <summary>
        /// Equijoin columns of the left side
        /// </summary>
        internal List<ColumnVar> LeftVars
        {
            get { return m_leftVars; }
        }

        /// <summary>
        /// Equijoin columns of the right side
        /// </summary>
        internal List<ColumnVar> RightVars
        {
            get { return m_rightVars; }
        }

        #endregion

        #region private methods

        #endregion
    }
}
