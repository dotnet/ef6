// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Transformation rules for FilterOps
    /// </summary>
    internal static class FilterOpRules
    {
        #region Helpers

        /// <summary>
        ///     Split up a predicate into 2 parts - the pushdown and the non-pushdown predicate.
        ///     If the filter node has no external references *and* the "columns" parameter is null,
        ///     then the entire predicate can be pushed down
        ///     We then compute the set of valid column references - if the "columns" parameter
        ///     is non-null, this set is used. Otherwise, we get the definitions of the
        ///     input relop node of the filterOp, and use that.
        ///     We use this list of valid column references to identify which parts of the filter
        ///     predicate can be pushed down - only those parts of the predicate that do not
        ///     reference anything beyond these columns are considered for pushdown. The rest are
        ///     stuffed into the nonPushdownPredicate output parameter
        /// </summary>
        /// <param name="command"> Command object </param>
        /// <param name="filterNode"> the FilterOp subtree </param>
        /// <param name="columns"> (Optional) List of columns to consider for "pushdown" </param>
        /// <param name="nonPushdownPredicateNode"> (output) Part of the predicate that cannot be pushed down </param>
        /// <returns> part of the predicate that can be pushed down </returns>
        private static Node GetPushdownPredicate(Command command, Node filterNode, VarVec columns, out Node nonPushdownPredicateNode)
        {
            var pushdownPredicateNode = filterNode.Child1;
            nonPushdownPredicateNode = null;
            var filterNodeInfo = command.GetExtendedNodeInfo(filterNode);
            if (columns == null
                && filterNodeInfo.ExternalReferences.IsEmpty)
            {
                return pushdownPredicateNode;
            }

            if (columns == null)
            {
                var inputNodeInfo = command.GetExtendedNodeInfo(filterNode.Child0);
                columns = inputNodeInfo.Definitions;
            }

            var predicate = new Predicate(command, pushdownPredicateNode);
            Predicate nonPushdownPredicate;
            predicate = predicate.GetSingleTablePredicates(columns, out nonPushdownPredicate);
            pushdownPredicateNode = predicate.BuildAndTree();
            nonPushdownPredicateNode = nonPushdownPredicate.BuildAndTree();
            return pushdownPredicateNode;
        }

        #endregion

        #region FilterOverFilter

        internal static readonly PatternMatchRule Rule_FilterOverFilter =
            new PatternMatchRule(
                new Node(
                    FilterOp.Pattern,
                    new Node(
                        FilterOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern)),
                ProcessFilterOverFilter);

        /// <summary>
        ///     Convert Filter(Filter(X, p1), p2) => Filter(X, (p1 and p2))
        /// </summary>
        /// <param name="context"> rule processing context </param>
        /// <param name="filterNode"> FilterOp node </param>
        /// <param name="newNode"> modified subtree </param>
        /// <returns> transformed subtree </returns>
        private static bool ProcessFilterOverFilter(RuleProcessingContext context, Node filterNode, out Node newNode)
        {
            var newAndNode = context.Command.CreateNode(
                context.Command.CreateConditionalOp(OpType.And),
                filterNode.Child0.Child1, filterNode.Child1);

            newNode = context.Command.CreateNode(context.Command.CreateFilterOp(), filterNode.Child0.Child0, newAndNode);
            return true;
        }

        #endregion

        #region FilterOverProject

        internal static readonly PatternMatchRule Rule_FilterOverProject =
            new PatternMatchRule(
                new Node(
                    FilterOp.Pattern,
                    new Node(
                        ProjectOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern)),
                ProcessFilterOverProject);

        /// <summary>
        ///     Convert Filter(Project(X, ...), p) => Project(Filter(X, p'), ...)
        /// </summary>
        /// <param name="context"> Rule processing context </param>
        /// <param name="filterNode"> FilterOp subtree </param>
        /// <param name="newNode"> modified subtree </param>
        /// <returns> transformed subtree </returns>
        private static bool ProcessFilterOverProject(RuleProcessingContext context, Node filterNode, out Node newNode)
        {
            newNode = filterNode;
            var predicateNode = filterNode.Child1;

            //
            // If the filter is a constant predicate, then don't push the filter below the
            // project
            //
            if (predicateNode.Op.OpType
                == OpType.ConstantPredicate)
            {
                // There's a different rule to process this case. Simply return
                return false;
            }

            var trc = (TransformationRulesContext)context;
            //
            // check to see that this is a simple predicate
            //
            var varRefMap = new Dictionary<Var, int>();
            if (!trc.IsScalarOpTree(predicateNode, varRefMap))
            {
                return false;
            }
            //
            // check to see if all expressions in the project can be inlined
            //
            var projectNode = filterNode.Child0;
            var varMap = trc.GetVarMap(projectNode.Child1, varRefMap);
            if (varMap == null)
            {
                return false;
            }

            //
            // Try to remap the predicate in terms of the definitions of the Vars
            //
            var remappedPredicateNode = trc.ReMap(predicateNode, varMap);

            //
            // Now push the filter below the project
            //
            var newFilterNode = trc.Command.CreateNode(trc.Command.CreateFilterOp(), projectNode.Child0, remappedPredicateNode);
            var newProjectNode = trc.Command.CreateNode(projectNode.Op, newFilterNode, projectNode.Child1);

            newNode = newProjectNode;
            return true;
        }

        #endregion

        #region FilterOverSetOp

        internal static readonly PatternMatchRule Rule_FilterOverUnionAll =
            new PatternMatchRule(
                new Node(
                    FilterOp.Pattern,
                    new Node(
                        UnionAllOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern)),
                ProcessFilterOverSetOp);

        internal static readonly PatternMatchRule Rule_FilterOverIntersect =
            new PatternMatchRule(
                new Node(
                    FilterOp.Pattern,
                    new Node(
                        IntersectOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern)),
                ProcessFilterOverSetOp);

        internal static readonly PatternMatchRule Rule_FilterOverExcept =
            new PatternMatchRule(
                new Node(
                    FilterOp.Pattern,
                    new Node(
                        ExceptOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern)),
                ProcessFilterOverSetOp);

        /// <summary>
        ///     Transform Filter(UnionAll(X1, X2), p) => UnionAll(Filter(X1, p1), Filter(X, p2))
        ///     Filter(Intersect(X1, X2), p) => Intersect(Filter(X1, p1), Filter(X2, p2))
        ///     Filter(Except(X1, X2), p) => Except(Filter(X1, p1), X2)
        ///     where p1 and p2 are the "mapped" versions of the predicate "p" for each branch
        /// </summary>
        /// <param name="context"> Rule processing context </param>
        /// <param name="filterNode"> FilterOp subtree </param>
        /// <param name="newNode"> modified subtree </param>
        /// <returns> true, if successful transformation </returns>
        private static bool ProcessFilterOverSetOp(RuleProcessingContext context, Node filterNode, out Node newNode)
        {
            newNode = filterNode;
            var trc = (TransformationRulesContext)context;

            //
            // Identify parts of the filter predicate that can be pushed down, and parts that
            // cannot be. If nothing can be pushed down, then return
            // 
            Node nonPushdownPredicate;
            var pushdownPredicate = GetPushdownPredicate(trc.Command, filterNode, null, out nonPushdownPredicate);
            if (pushdownPredicate == null)
            {
                return false;
            }
            // Handle only simple predicates
            if (!trc.IsScalarOpTree(pushdownPredicate))
            {
                return false;
            }

            //
            // Now push the predicate (the part that can be pushed down) into each of the
            // branches (as appropriate)
            // 
            var setOpNode = filterNode.Child0;
            var setOp = (SetOp)setOpNode.Op;
            var newSetOpChildren = new List<Node>();
            var branchId = 0;
            foreach (var varMap in setOp.VarMap)
            {
                // For exceptOp, the filter should only be pushed below the zeroth child
                if (setOp.OpType == OpType.Except
                    && branchId == 1)
                {
                    newSetOpChildren.Add(setOpNode.Child1);
                    break;
                }

                var remapMap = new Dictionary<Var, Node>();
                foreach (var kv in varMap)
                {
                    var varRefNode = trc.Command.CreateNode(trc.Command.CreateVarRefOp(kv.Value));
                    remapMap.Add(kv.Key, varRefNode);
                }

                //
                // Now fix up the predicate.
                // Make a copy of the predicate first - except if we're dealing with the last
                // branch, in which case, we can simply reuse the predicate
                //
                var predicateNode = pushdownPredicate;
                if (branchId == 0
                    && filterNode.Op.OpType != OpType.Except)
                {
                    predicateNode = trc.Copy(predicateNode);
                }
                var newPredicateNode = trc.ReMap(predicateNode, remapMap);
                trc.Command.RecomputeNodeInfo(newPredicateNode);

                // create a new filter node below the setOp child
                var newFilterNode = trc.Command.CreateNode(
                    trc.Command.CreateFilterOp(),
                    setOpNode.Children[branchId],
                    newPredicateNode);
                newSetOpChildren.Add(newFilterNode);

                branchId++;
            }
            var newSetOpNode = trc.Command.CreateNode(setOpNode.Op, newSetOpChildren);

            //
            // We've now pushed down the relevant parts of the filter below the SetOps
            // We may still however some predicates left over - create a new filter node
            // to account for that
            // 
            if (nonPushdownPredicate != null)
            {
                newNode = trc.Command.CreateNode(trc.Command.CreateFilterOp(), newSetOpNode, nonPushdownPredicate);
            }
            else
            {
                newNode = newSetOpNode;
            }
            return true;
        }

        #endregion

        #region FilterOverDistinct

        internal static readonly PatternMatchRule Rule_FilterOverDistinct =
            new PatternMatchRule(
                new Node(
                    FilterOp.Pattern,
                    new Node(
                        DistinctOp.Pattern,
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern)),
                ProcessFilterOverDistinct);

        /// <summary>
        ///     Transforms Filter(Distinct(x), p) => Filter(Distinct(Filter(X, p1), p2)
        ///     where p2 is the part of the filter that can be pushed down, while p1 represents
        ///     any external references
        /// </summary>
        /// <param name="context"> Rule processing context </param>
        /// <param name="filterNode"> FilterOp subtree </param>
        /// <param name="newNode"> modified subtree </param>
        /// <returns> Transformation status </returns>
        private static bool ProcessFilterOverDistinct(RuleProcessingContext context, Node filterNode, out Node newNode)
        {
            newNode = filterNode;
            //
            // Split up the filter predicate into two parts - the part that can be pushed down
            // and the part that can't. If there is no part that can be pushed down, simply return
            // 
            Node nonPushdownPredicate;
            var pushdownPredicate = GetPushdownPredicate(context.Command, filterNode, null, out nonPushdownPredicate);
            if (pushdownPredicate == null)
            {
                return false;
            }

            //
            // Create a new filter node below the current distinct node for the predicate
            // that can be pushed down - create a new distinct node as well
            // 
            var distinctNode = filterNode.Child0;
            var pushdownFilterNode = context.Command.CreateNode(context.Command.CreateFilterOp(), distinctNode.Child0, pushdownPredicate);
            var newDistinctNode = context.Command.CreateNode(distinctNode.Op, pushdownFilterNode);

            //
            // If we have a predicate part that cannot be pushed down, build up a new 
            // filter node above the new Distinct op that we just created
            // 
            if (nonPushdownPredicate != null)
            {
                newNode = context.Command.CreateNode(context.Command.CreateFilterOp(), newDistinctNode, nonPushdownPredicate);
            }
            else
            {
                newNode = newDistinctNode;
            }
            return true;
        }

        #endregion

        #region FilterOverGroupBy

        internal static readonly PatternMatchRule Rule_FilterOverGroupBy =
            new PatternMatchRule(
                new Node(
                    FilterOp.Pattern,
                    new Node(
                        GroupByOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern)),
                ProcessFilterOverGroupBy);

        /// <summary>
        ///     Transforms Filter(GroupBy(X, k1.., a1...), p) =>
        ///     Filter(GroupBy(Filter(X, p1'), k1..., a1...), p2)
        ///     p1 and p2 represent the parts of p that can and cannot be pushed down
        ///     respectively - specifically, p1 must only reference the key columns from
        ///     the GroupByOp.
        ///     "p1'" is the mapped version of "p1",
        /// </summary>
        /// <param name="context"> Rule processing context </param>
        /// <param name="filterNode"> Current FilterOp subtree </param>
        /// <param name="newNode"> modified subtree </param>
        /// <returns> Transformation status </returns>
        private static bool ProcessFilterOverGroupBy(RuleProcessingContext context, Node filterNode, out Node newNode)
        {
            newNode = filterNode;
            var groupByNode = filterNode.Child0;
            var groupByOp = (GroupByOp)groupByNode.Op;
            var trc = (TransformationRulesContext)context;

            // Check to see that we have a simple predicate
            var varRefMap = new Dictionary<Var, int>();
            if (!trc.IsScalarOpTree(filterNode.Child1, varRefMap))
            {
                return false;
            }

            // 
            // Split up the predicate into two parts - the part that can be pushed down below
            // the groupByOp (specifically, the part that only refers to keys of the groupByOp),
            // and the part that cannot be pushed below
            // If nothing can be pushed below, quit now
            // 
            Node nonPushdownPredicate;
            var pushdownPredicate = GetPushdownPredicate(context.Command, filterNode, groupByOp.Keys, out nonPushdownPredicate);
            if (pushdownPredicate == null)
            {
                return false;
            }

            //
            // We need to push the filter down; but we need to remap the predicate, so
            // that any references to variables defined locally by the groupBy are fixed up
            // Make sure that the predicate is not too complex to remap
            //
            var varMap = trc.GetVarMap(groupByNode.Child1, varRefMap);
            if (varMap == null)
            {
                return false; // complex expressions
            }
            var remappedPushdownPredicate = trc.ReMap(pushdownPredicate, varMap);

            //
            // Push the filter below the groupBy now
            //
            var subFilterNode = trc.Command.CreateNode(trc.Command.CreateFilterOp(), groupByNode.Child0, remappedPushdownPredicate);
            var newGroupByNode = trc.Command.CreateNode(groupByNode.Op, subFilterNode, groupByNode.Child1, groupByNode.Child2);

            //
            // If there was any part of the original predicate that could not be pushed down,
            // create a new filterOp node above the new groupBy node to represent that 
            // predicate
            //
            if (nonPushdownPredicate == null)
            {
                newNode = newGroupByNode;
            }
            else
            {
                newNode = trc.Command.CreateNode(trc.Command.CreateFilterOp(), newGroupByNode, nonPushdownPredicate);
            }
            return true;
        }

        #endregion

        #region FilterOverJoin

        internal static readonly PatternMatchRule Rule_FilterOverCrossJoin =
            new PatternMatchRule(
                new Node(
                    FilterOp.Pattern,
                    new Node(
                        CrossJoinOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern)),
                ProcessFilterOverJoin);

        internal static readonly PatternMatchRule Rule_FilterOverInnerJoin =
            new PatternMatchRule(
                new Node(
                    FilterOp.Pattern,
                    new Node(
                        InnerJoinOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern)),
                ProcessFilterOverJoin);

        internal static readonly PatternMatchRule Rule_FilterOverLeftOuterJoin =
            new PatternMatchRule(
                new Node(
                    FilterOp.Pattern,
                    new Node(
                        LeftOuterJoinOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern)),
                ProcessFilterOverJoin);

        /// <summary>
        ///     Transform Filter()
        /// </summary>
        /// <param name="context"> Rule Processing context </param>
        /// <param name="filterNode"> Current FilterOp subtree </param>
        /// <param name="newNode"> Modified subtree </param>
        /// <returns> Transformation status </returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "non-InnerJoin")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private static bool ProcessFilterOverJoin(RuleProcessingContext context, Node filterNode, out Node newNode)
        {
            newNode = filterNode;
            var trc = (TransformationRulesContext)context;

            //
            // Have we shut off filter pushdown for this node? Return
            //
            if (trc.IsFilterPushdownSuppressed(filterNode))
            {
                return false;
            }

            var joinNode = filterNode.Child0;
            var joinOp = joinNode.Op;
            var leftInputNode = joinNode.Child0;
            var rightInputNode = joinNode.Child1;
            var command = trc.Command;
            var needsTransformation = false;

            //
            // If we're dealing with an outer-join, first check to see if the current 
            // predicate preserves nulls for the right table. 
            // If it doesn't then we can convert the outer join into an inner join,
            // and then continue with the rest of our processing here
            // 
            var rightTableNodeInfo = command.GetExtendedNodeInfo(rightInputNode);
            var predicate = new Predicate(command, filterNode.Child1);
            if (joinOp.OpType
                == OpType.LeftOuterJoin)
            {
                if (!predicate.PreservesNulls(rightTableNodeInfo.Definitions, true))
                {
                    joinOp = command.CreateInnerJoinOp();
                    needsTransformation = true;
                }
            }
            var leftTableInfo = command.GetExtendedNodeInfo(leftInputNode);

            //
            // Check to see if the predicate contains any "single-table-filters". In those
            // cases, we could simply push that filter down to the child. 
            // We can do this for inner joins and cross joins - for both inputs.
            // For left-outer joins, however, we can only do this for the left-side input
            // Further note that we only want to do the pushdown if it will help us - if 
            // the join input is a ScanTable (or some other cases), then it doesn't help us.
            // 
            Node leftSingleTablePredicateNode = null;
            if (leftInputNode.Op.OpType
                != OpType.ScanTable)
            {
                var leftSingleTablePredicates = predicate.GetSingleTablePredicates(leftTableInfo.Definitions, out predicate);
                leftSingleTablePredicateNode = leftSingleTablePredicates.BuildAndTree();
            }

            Node rightSingleTablePredicateNode = null;
            if ((rightInputNode.Op.OpType != OpType.ScanTable)
                &&
                (joinOp.OpType != OpType.LeftOuterJoin))
            {
                var rightSingleTablePredicates = predicate.GetSingleTablePredicates(rightTableNodeInfo.Definitions, out predicate);
                rightSingleTablePredicateNode = rightSingleTablePredicates.BuildAndTree();
            }

            //
            // Now check to see if the predicate contains some "join predicates". We can
            // add these to the existing join predicate (if any). 
            // We can only do this for inner joins and cross joins - not for LOJs
            //
            Node newJoinPredicateNode = null;
            if (joinOp.OpType == OpType.CrossJoin
                || joinOp.OpType == OpType.InnerJoin)
            {
                var joinPredicate = predicate.GetJoinPredicates(leftTableInfo.Definitions, rightTableNodeInfo.Definitions, out predicate);
                newJoinPredicateNode = joinPredicate.BuildAndTree();
            }

            //
            // Now for the dirty work. We've identified some predicates that could be pushed
            // into the left table, some predicates that could be pushed into the right table
            // and some that could become join predicates. 
            // 
            if (leftSingleTablePredicateNode != null)
            {
                leftInputNode = command.CreateNode(command.CreateFilterOp(), leftInputNode, leftSingleTablePredicateNode);
                needsTransformation = true;
            }
            if (rightSingleTablePredicateNode != null)
            {
                rightInputNode = command.CreateNode(command.CreateFilterOp(), rightInputNode, rightSingleTablePredicateNode);
                needsTransformation = true;
            }

            // Identify the new join predicate
            if (newJoinPredicateNode != null)
            {
                needsTransformation = true;
                if (joinOp.OpType
                    == OpType.CrossJoin)
                {
                    joinOp = command.CreateInnerJoinOp();
                }
                else
                {
                    PlanCompiler.Assert(joinOp.OpType == OpType.InnerJoin, "unexpected non-InnerJoin?");
                    newJoinPredicateNode = PlanCompilerUtil.CombinePredicates(joinNode.Child2, newJoinPredicateNode, command);
                }
            }
            else
            {
                newJoinPredicateNode = (joinOp.OpType == OpType.CrossJoin) ? null : joinNode.Child2;
            }

            // 
            // If nothing has changed, then just return the current node. Otherwise, 
            // we will loop forever
            //
            if (!needsTransformation)
            {
                return false;
            }

            Node newJoinNode;
            // 
            // Finally build up a new join node
            // 
            if (joinOp.OpType
                == OpType.CrossJoin)
            {
                newJoinNode = command.CreateNode(joinOp, leftInputNode, rightInputNode);
            }
            else
            {
                newJoinNode = command.CreateNode(joinOp, leftInputNode, rightInputNode, newJoinPredicateNode);
            }

            //
            // Build up a new filterNode above this join node. But only if we have a filter left
            // 
            var newFilterPredicateNode = predicate.BuildAndTree();
            if (newFilterPredicateNode == null)
            {
                newNode = newJoinNode;
            }
            else
            {
                newNode = command.CreateNode(command.CreateFilterOp(), newJoinNode, newFilterPredicateNode);
            }
            return true;
        }

        #endregion

        #region Filter over OuterApply

        internal static readonly PatternMatchRule Rule_FilterOverOuterApply =
            new PatternMatchRule(
                new Node(
                    FilterOp.Pattern,
                    new Node(
                        OuterApplyOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern)),
                ProcessFilterOverOuterApply);

        /// <summary>
        ///     Convert Filter(OuterApply(X,Y), p) into
        ///     Filter(CrossApply(X,Y), p)
        ///     if "p" is not null-preserving for Y (ie) "p" does not preserve null values from Y
        /// </summary>
        /// <param name="context"> Rule processing context </param>
        /// <param name="filterNode"> Filter node </param>
        /// <param name="newNode"> modified subtree </param>
        /// <returns> transformation status </returns>
        private static bool ProcessFilterOverOuterApply(RuleProcessingContext context, Node filterNode, out Node newNode)
        {
            newNode = filterNode;
            var applyNode = filterNode.Child0;
            var applyOp = applyNode.Op;
            var applyRightInputNode = applyNode.Child1;
            var trc = (TransformationRulesContext)context;
            var command = trc.Command;

            //
            // Check to see if the current predicate preserves nulls for the right table. 
            // If it doesn't then we can convert the outer apply into a cross-apply,
            // 
            var rightTableNodeInfo = command.GetExtendedNodeInfo(applyRightInputNode);
            var predicate = new Predicate(command, filterNode.Child1);
            if (!predicate.PreservesNulls(rightTableNodeInfo.Definitions, true))
            {
                var newApplyNode = command.CreateNode(command.CreateCrossApplyOp(), applyNode.Child0, applyRightInputNode);
                var newFilterNode = command.CreateNode(command.CreateFilterOp(), newApplyNode, filterNode.Child1);
                newNode = newFilterNode;
                return true;
            }

            return false;
        }

        #endregion

        #region FilterWithConstantPredicate

        internal static readonly PatternMatchRule Rule_FilterWithConstantPredicate =
            new PatternMatchRule(
                new Node(
                    FilterOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(ConstantPredicateOp.Pattern)),
                ProcessFilterWithConstantPredicate);

        /// <summary>
        ///     Convert
        ///     Filter(X, true)  => X
        ///     Filter(X, false) => Project(Filter(SingleRowTableOp, ...), false)
        ///     where ... represent variables that are equivalent to the table columns
        /// </summary>
        /// <param name="context"> Rule processing context </param>
        /// <param name="n"> Current subtree </param>
        /// <param name="newNode"> modified subtree </param>
        /// <returns> transformation status </returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private static bool ProcessFilterWithConstantPredicate(RuleProcessingContext context, Node n, out Node newNode)
        {
            newNode = n;
            var predOp = (ConstantPredicateOp)n.Child1.Op;

            // If we're dealing with a "true" predicate, then simply return the RelOp
            // input to the filter
            if (predOp.IsTrue)
            {
                newNode = n.Child0;
                return true;
            }

            PlanCompiler.Assert(predOp.IsFalse, "unexpected non-false predicate?");
            // We're dealing with a "false" predicate, then we can get rid of the 
            // input, and replace it with a dummy project

            //
            // If the input is already a singlerowtableOp, then there's nothing 
            // further to do
            //
            if (n.Child0.Op.OpType == OpType.SingleRowTable
                ||
                (n.Child0.Op.OpType == OpType.Project &&
                 n.Child0.Child0.Op.OpType == OpType.SingleRowTable))
            {
                return false;
            }

            var trc = (TransformationRulesContext)context;
            var childNodeInfo = trc.Command.GetExtendedNodeInfo(n.Child0);
            var varDefNodeList = new List<Node>();
            var newVars = trc.Command.CreateVarVec();
            foreach (var v in childNodeInfo.Definitions)
            {
                var nullConst = trc.Command.CreateNullOp(v.Type);
                var constNode = trc.Command.CreateNode(nullConst);
                Var computedVar;
                var varDefNode = trc.Command.CreateVarDefNode(constNode, out computedVar);
                trc.AddVarMapping(v, computedVar);
                newVars.Set(computedVar);
                varDefNodeList.Add(varDefNode);
            }
            // If no vars have been selected out, add a dummy var
            if (newVars.IsEmpty)
            {
                var nullConst = trc.Command.CreateNullOp(trc.Command.BooleanType);
                var constNode = trc.Command.CreateNode(nullConst);
                Var computedVar;
                var varDefNode = trc.Command.CreateVarDefNode(constNode, out computedVar);
                newVars.Set(computedVar);
                varDefNodeList.Add(varDefNode);
            }

            var singleRowTableNode = trc.Command.CreateNode(trc.Command.CreateSingleRowTableOp());
            n.Child0 = singleRowTableNode;

            var varDefListNode = trc.Command.CreateNode(trc.Command.CreateVarDefListOp(), varDefNodeList);
            var projectOp = trc.Command.CreateProjectOp(newVars);
            var projectNode = trc.Command.CreateNode(projectOp, n, varDefListNode);

            projectNode.Child0 = n;
            newNode = projectNode;
            return true;
        }

        #endregion

        #region All FilterOp Rules

        internal static readonly Rule[] Rules = new Rule[]
                                                    {
                                                        Rule_FilterWithConstantPredicate,
                                                        Rule_FilterOverCrossJoin,
                                                        Rule_FilterOverDistinct,
                                                        Rule_FilterOverExcept,
                                                        Rule_FilterOverFilter,
                                                        Rule_FilterOverGroupBy,
                                                        Rule_FilterOverInnerJoin,
                                                        Rule_FilterOverIntersect,
                                                        Rule_FilterOverLeftOuterJoin,
                                                        Rule_FilterOverProject,
                                                        Rule_FilterOverUnionAll,
                                                        Rule_FilterOverOuterApply,
                                                    };

        #endregion
    }
}
