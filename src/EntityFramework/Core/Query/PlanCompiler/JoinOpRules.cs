// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Transformation rules for JoinOps
    /// </summary>
    internal static class JoinOpRules
    {
        #region JoinOverProject

        internal static readonly PatternMatchRule Rule_CrossJoinOverProject1 =
            new PatternMatchRule(
                new Node(
                    CrossJoinOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(
                        ProjectOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern))),
                ProcessJoinOverProject);

        internal static readonly PatternMatchRule Rule_CrossJoinOverProject2 =
            new PatternMatchRule(
                new Node(
                    CrossJoinOp.Pattern,
                    new Node(
                        ProjectOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern)),
                ProcessJoinOverProject);

        internal static readonly PatternMatchRule Rule_InnerJoinOverProject1 =
            new PatternMatchRule(
                new Node(
                    InnerJoinOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(
                        ProjectOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern)),
                ProcessJoinOverProject);

        internal static readonly PatternMatchRule Rule_InnerJoinOverProject2 =
            new PatternMatchRule(
                new Node(
                    InnerJoinOp.Pattern,
                    new Node(
                        ProjectOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern),
                    new Node(LeafOp.Pattern)),
                ProcessJoinOverProject);

        internal static readonly PatternMatchRule Rule_OuterJoinOverProject2 =
            new PatternMatchRule(
                new Node(
                    LeftOuterJoinOp.Pattern,
                    new Node(
                        ProjectOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern),
                    new Node(LeafOp.Pattern)),
                ProcessJoinOverProject);

        /// <summary>
        /// CrossJoin(Project(A), B) => Project(CrossJoin(A, B), modifiedvars)
        /// InnerJoin(Project(A), B, p) => Project(InnerJoin(A, B, p'), modifiedvars)
        /// LeftOuterJoin(Project(A), B, p) => Project(LeftOuterJoin(A, B, p'), modifiedvars)
        /// </summary>
        /// <param name="context"> Rule processing context </param>
        /// <param name="joinNode"> Current JoinOp tree to process </param>
        /// <param name="newNode"> Transformed subtree </param>
        /// <returns> transformation status </returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "non-LeftOuterJoin")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private static bool ProcessJoinOverProject(RuleProcessingContext context, Node joinNode, out Node newNode)
        {
            newNode = joinNode;

            var trc = (TransformationRulesContext)context;
            var command = trc.Command;

            var joinConditionNode = joinNode.HasChild2 ? joinNode.Child2 : null;
            var varRefMap = new Dictionary<Var, int>();
            if (joinConditionNode != null
                && !trc.IsScalarOpTree(joinConditionNode, varRefMap))
            {
                return false;
            }

            Node newJoinNode;
            Node newProjectNode;

            // Now locate the ProjectOps
            var newVarSet = command.CreateVarVec();
            var varDefNodes = new List<Node>();

            //
            // Try and handle "project" on both sides only if we're not dealing with 
            // an LOJ. 
            //
            if ((joinNode.Op.OpType != OpType.LeftOuterJoin)
                &&
                (joinNode.Child0.Op.OpType == OpType.Project)
                &&
                (joinNode.Child1.Op.OpType == OpType.Project))
            {
                var projectOp1 = (ProjectOp)joinNode.Child0.Op;
                var projectOp2 = (ProjectOp)joinNode.Child1.Op;

                var varMap1 = trc.GetVarMap(joinNode.Child0.Child1, varRefMap);
                var varMap2 = trc.GetVarMap(joinNode.Child1.Child1, varRefMap);
                if (varMap1 == null
                    || varMap2 == null)
                {
                    return false;
                }

                if (joinConditionNode != null)
                {
                    joinConditionNode = trc.ReMap(joinConditionNode, varMap1);
                    joinConditionNode = trc.ReMap(joinConditionNode, varMap2);
                    newJoinNode = context.Command.CreateNode(joinNode.Op, joinNode.Child0.Child0, joinNode.Child1.Child0, joinConditionNode);
                }
                else
                {
                    newJoinNode = context.Command.CreateNode(joinNode.Op, joinNode.Child0.Child0, joinNode.Child1.Child0);
                }

                newVarSet.InitFrom(projectOp1.Outputs);
                foreach (var v in projectOp2.Outputs)
                {
                    newVarSet.Set(v);
                }
                var newProjectOp = command.CreateProjectOp(newVarSet);
                varDefNodes.AddRange(joinNode.Child0.Child1.Children);
                varDefNodes.AddRange(joinNode.Child1.Child1.Children);
                var varDefListNode = command.CreateNode(
                    command.CreateVarDefListOp(),
                    varDefNodes);
                newProjectNode = command.CreateNode(
                    newProjectOp,
                    newJoinNode, varDefListNode);
                newNode = newProjectNode;
                return true;
            }

            var projectNodeIdx = -1;
            var otherNodeIdx = -1;
            if (joinNode.Child0.Op.OpType
                == OpType.Project)
            {
                projectNodeIdx = 0;
                otherNodeIdx = 1;
            }
            else
            {
                PlanCompiler.Assert(joinNode.Op.OpType != OpType.LeftOuterJoin, "unexpected non-LeftOuterJoin");
                projectNodeIdx = 1;
                otherNodeIdx = 0;
            }
            var projectNode = joinNode.Children[projectNodeIdx];

            var projectOp = projectNode.Op as ProjectOp;
            var varMap = trc.GetVarMap(projectNode.Child1, varRefMap);
            if (varMap == null)
            {
                return false;
            }
            var otherChildInfo = command.GetExtendedNodeInfo(joinNode.Children[otherNodeIdx]);
            var vec = command.CreateVarVec(projectOp.Outputs);
            vec.Or(otherChildInfo.Definitions);
            projectOp.Outputs.InitFrom(vec);
            if (joinConditionNode != null)
            {
                joinConditionNode = trc.ReMap(joinConditionNode, varMap);
                joinNode.Child2 = joinConditionNode;
            }
            joinNode.Children[projectNodeIdx] = projectNode.Child0; // bypass the projectOp
            context.Command.RecomputeNodeInfo(joinNode);

            newNode = context.Command.CreateNode(projectOp, joinNode, projectNode.Child1);
            return true;
        }

        #endregion

        #region JoinOverFilter

        internal static readonly PatternMatchRule Rule_CrossJoinOverFilter1 =
            new PatternMatchRule(
                new Node(
                    CrossJoinOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(
                        FilterOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern))),
                ProcessJoinOverFilter);

        internal static readonly PatternMatchRule Rule_CrossJoinOverFilter2 =
            new PatternMatchRule(
                new Node(
                    CrossJoinOp.Pattern,
                    new Node(
                        FilterOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern)),
                ProcessJoinOverFilter);

        internal static readonly PatternMatchRule Rule_InnerJoinOverFilter1 =
            new PatternMatchRule(
                new Node(
                    InnerJoinOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(
                        FilterOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern)),
                ProcessJoinOverFilter);

        internal static readonly PatternMatchRule Rule_InnerJoinOverFilter2 =
            new PatternMatchRule(
                new Node(
                    InnerJoinOp.Pattern,
                    new Node(
                        FilterOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern),
                    new Node(LeafOp.Pattern)),
                ProcessJoinOverFilter);

        internal static readonly PatternMatchRule Rule_OuterJoinOverFilter2 =
            new PatternMatchRule(
                new Node(
                    LeftOuterJoinOp.Pattern,
                    new Node(
                        FilterOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern),
                    new Node(LeafOp.Pattern)),
                ProcessJoinOverFilter);

        /// <summary>
        /// CrossJoin(Filter(A,p), B) => Filter(CrossJoin(A, B), p)
        /// CrossJoin(A, Filter(B,p)) => Filter(CrossJoin(A, B), p)
        /// InnerJoin(Filter(A,p), B, c) => Filter(InnerJoin(A, B, c), p)
        /// InnerJoin(A, Filter(B,p), c) => Filter(InnerJoin(A, B, c), p)
        /// LeftOuterJoin(Filter(A,p), B, c) => Filter(LeftOuterJoin(A, B, c), p)
        /// Note that the predicate on the right table in a left-outer-join cannot be pulled
        /// up above the join.
        /// </summary>
        /// <param name="context"> Rule processing context </param>
        /// <param name="joinNode"> Current JoinOp tree to process </param>
        /// <param name="newNode"> transformed subtree </param>
        /// <returns> transformation status </returns>
        private static bool ProcessJoinOverFilter(RuleProcessingContext context, Node joinNode, out Node newNode)
        {
            newNode = joinNode;
            var trc = (TransformationRulesContext)context;
            var command = trc.Command;

            Node predicateNode = null;
            var newLeftInput = joinNode.Child0;
            // get the predicate from the first filter
            if (joinNode.Child0.Op.OpType
                == OpType.Filter)
            {
                predicateNode = joinNode.Child0.Child1;
                newLeftInput = joinNode.Child0.Child0; // bypass the filter
            }

            // get the predicate from the second filter
            var newRightInput = joinNode.Child1;
            if (joinNode.Child1.Op.OpType == OpType.Filter
                && joinNode.Op.OpType != OpType.LeftOuterJoin)
            {
                if (predicateNode == null)
                {
                    predicateNode = joinNode.Child1.Child1;
                }
                else
                {
                    predicateNode = command.CreateNode(
                        command.CreateConditionalOp(OpType.And),
                        predicateNode, joinNode.Child1.Child1);
                }
                newRightInput = joinNode.Child1.Child0; // bypass the filter
            }

            // No optimizations to perform if we can't locate the appropriate predicate
            if (predicateNode == null)
            {
                return false;
            }

            //
            // Create a new join node with the new inputs
            //
            Node newJoinNode;
            if (joinNode.Op.OpType
                == OpType.CrossJoin)
            {
                newJoinNode = command.CreateNode(joinNode.Op, newLeftInput, newRightInput);
            }
            else
            {
                newJoinNode = command.CreateNode(joinNode.Op, newLeftInput, newRightInput, joinNode.Child2);
            }

            //
            // create a new filterOp with the combined predicates, and with the 
            // newjoinNode as the input
            //
            var newFilterOp = command.CreateFilterOp();
            newNode = command.CreateNode(newFilterOp, newJoinNode, predicateNode);

            //
            // Mark this subtree so that we don't try to push filters down again
            // 
            trc.SuppressFilterPushdown(newNode);
            return true;
        }

        #endregion

        #region Join over SingleRowTable

        internal static readonly PatternMatchRule Rule_CrossJoinOverSingleRowTable1 =
            new PatternMatchRule(
                new Node(
                    CrossJoinOp.Pattern,
                    new Node(SingleRowTableOp.Pattern),
                    new Node(LeafOp.Pattern)),
                ProcessJoinOverSingleRowTable);

        internal static readonly PatternMatchRule Rule_CrossJoinOverSingleRowTable2 =
            new PatternMatchRule(
                new Node(
                    CrossJoinOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(SingleRowTableOp.Pattern)),
                ProcessJoinOverSingleRowTable);

        internal static readonly PatternMatchRule Rule_LeftOuterJoinOverSingleRowTable =
            new PatternMatchRule(
                new Node(
                    LeftOuterJoinOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(SingleRowTableOp.Pattern),
                    new Node(LeafOp.Pattern)),
                ProcessJoinOverSingleRowTable);

        /// <summary>
        /// Convert a CrossJoin(SingleRowTable, X) or CrossJoin(X, SingleRowTable) or LeftOuterJoin(X, SingleRowTable)
        /// into just "X"
        /// </summary>
        /// <param name="context"> rule processing context </param>
        /// <param name="joinNode"> the join node </param>
        /// <param name="newNode"> transformed subtree </param>
        /// <returns> transformation status </returns>
        private static bool ProcessJoinOverSingleRowTable(RuleProcessingContext context, Node joinNode, out Node newNode)
        {
            newNode = joinNode;

            if (joinNode.Child0.Op.OpType
                == OpType.SingleRowTable)
            {
                newNode = joinNode.Child1;
            }
            else
            {
                newNode = joinNode.Child0;
            }
            return true;
        }

        #endregion

        #region Misc

        #endregion

        #region All JoinOp Rules

        internal static readonly Rule[] Rules = new Rule[]
            {
                Rule_CrossJoinOverProject1,
                Rule_CrossJoinOverProject2,
                Rule_InnerJoinOverProject1,
                Rule_InnerJoinOverProject2,
                Rule_OuterJoinOverProject2,
                Rule_CrossJoinOverFilter1,
                Rule_CrossJoinOverFilter2,
                Rule_InnerJoinOverFilter1,
                Rule_InnerJoinOverFilter2,
                Rule_OuterJoinOverFilter2,
                Rule_CrossJoinOverSingleRowTable1,
                Rule_CrossJoinOverSingleRowTable2,
                Rule_LeftOuterJoinOverSingleRowTable,
            };

        #endregion
    }
}
