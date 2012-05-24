namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents an "edge" in the join graph.
    /// A JoinEdge is a directed equijoin between the left and the right table. The equijoin
    /// columns are represented by the LeftVars and the RightVars properties
    /// </summary>
    internal class JoinEdge
    {
        #region private state

        private readonly AugmentedTableNode m_left;
        private readonly AugmentedTableNode m_right;
        private readonly AugmentedJoinNode m_joinNode;
        private readonly List<ColumnVar> m_leftVars;
        private readonly List<ColumnVar> m_rightVars;

        #endregion

        #region constructors

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="left">the left table</param>
        /// <param name="right">the right table</param>
        /// <param name="joinNode">the owner join node</param>
        /// <param name="joinKind">the Join Kind</param>
        /// <param name="leftVars">list of equijoin columns of the left table</param>
        /// <param name="rightVars">equijoin columns of the right table</param>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private JoinEdge(
            AugmentedTableNode left, AugmentedTableNode right,
            AugmentedJoinNode joinNode, JoinKind joinKind,
            List<ColumnVar> leftVars, List<ColumnVar> rightVars)
        {
            m_left = left;
            m_right = right;
            JoinKind = joinKind;
            m_joinNode = joinNode;
            m_leftVars = leftVars;
            m_rightVars = rightVars;
            PlanCompiler.Assert(m_leftVars.Count == m_rightVars.Count, "Count mismatch: " + m_leftVars.Count + "," + m_rightVars.Count);
        }

        #endregion

        #region public apis

        /// <summary>
        /// The left table
        /// </summary>
        internal AugmentedTableNode Left
        {
            get { return m_left; }
        }

        /// <summary>
        /// The right table of the join
        /// </summary>
        internal AugmentedTableNode Right
        {
            get { return m_right; }
        }

        /// <summary>
        /// The underlying join node, may be null
        /// </summary>     
        internal AugmentedJoinNode JoinNode
        {
            get { return m_joinNode; }
        }

        /// <summary>
        /// The join kind
        /// </summary>
        internal JoinKind JoinKind { get; set; }

        /// <summary>
        /// Equijoin columns of the left table
        /// </summary>
        internal List<ColumnVar> LeftVars
        {
            get { return m_leftVars; }
        }

        /// <summary>
        /// Equijoin columns of the right table
        /// </summary>
        internal List<ColumnVar> RightVars
        {
            get { return m_rightVars; }
        }

        /// <summary>
        /// Is this join edge useless?
        /// </summary>
        internal bool IsEliminated
        {
            get { return Left.IsEliminated || Right.IsEliminated; }
        }

        /// <summary>
        /// Factory method
        /// </summary>
        /// <param name="left">left table</param>
        /// <param name="right">right table</param>
        /// <param name="joinNode">the owner join node</param>
        /// <param name="leftVar">equijoin column of the left table</param>
        /// <param name="rightVar">equijoin column of the right table</param>
        /// <returns>the new join edge</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal static JoinEdge CreateJoinEdge(
            AugmentedTableNode left, AugmentedTableNode right,
            AugmentedJoinNode joinNode,
            ColumnVar leftVar, ColumnVar rightVar)
        {
            var leftVars = new List<ColumnVar>();
            var rightVars = new List<ColumnVar>();
            leftVars.Add(leftVar);
            rightVars.Add(rightVar);

            var joinOpType = joinNode.Node.Op.OpType;
            PlanCompiler.Assert(
                (joinOpType == OpType.LeftOuterJoin || joinOpType == OpType.InnerJoin),
                "Unexpected join type for join edge: " + joinOpType);

            var joinKind = joinOpType == OpType.LeftOuterJoin ? JoinKind.LeftOuter : JoinKind.Inner;

            var joinEdge = new JoinEdge(left, right, joinNode, joinKind, leftVars, rightVars);
            return joinEdge;
        }

        /// <summary>
        /// Creates a transitively generated join edge
        /// </summary>
        /// <param name="left">the left table</param>
        /// <param name="right">the right table</param>
        /// <param name="joinKind">the join kind</param>
        /// <param name="leftVars">left equijoin vars</param>
        /// <param name="rightVars">right equijoin vars</param>
        /// <returns>the join edge</returns>
        internal static JoinEdge CreateTransitiveJoinEdge(
            AugmentedTableNode left, AugmentedTableNode right, JoinKind joinKind,
            List<ColumnVar> leftVars, List<ColumnVar> rightVars)
        {
            var joinEdge = new JoinEdge(left, right, null, joinKind, leftVars, rightVars);
            return joinEdge;
        }

        /// <summary>
        /// Add a new "equi-join" condition to this edge
        /// </summary>
        /// <param name="joinNode">join node producing this condition</param>
        /// <param name="leftVar">the left-side column</param>
        /// <param name="rightVar">the right-side column</param>
        /// <returns>true, if this condition can be added</returns>
        internal bool AddCondition(AugmentedJoinNode joinNode, ColumnVar leftVar, ColumnVar rightVar)
        {
            if (joinNode != m_joinNode)
            {
                return false;
            }
            m_leftVars.Add(leftVar);
            m_rightVars.Add(rightVar);
            return true;
        }

        #endregion
    }
}
