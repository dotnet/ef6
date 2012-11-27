// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics.CodeAnalysis;

    internal delegate bool TryGetValue(Node key, out Node value);

    /// <summary>
    ///     The Aggregate Pushdown feature tries to identify function aggregates defined over a
    ///     group aggregate and push their definitions in the group by into node corresponding to
    ///     the group aggregate.
    /// </summary>
    internal class AggregatePushdown
    {
        #region Private fields

        private readonly Command m_command;
        private TryGetValue m_tryGetParent;

        #endregion

        #region Private Constructor

        private AggregatePushdown(Command command)
        {
            m_command = command;
        }

        #endregion

        #region 'Public' Surface

        /// <summary>
        ///     Apply Aggregate Pushdown over the tree in the given plan complier state.
        /// </summary>
        /// <param name="planCompilerState"> </param>
        internal static void Process(PlanCompiler planCompilerState)
        {
            var aggregatePushdown = new AggregatePushdown(planCompilerState.Command);
            aggregatePushdown.Process();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     The main driver
        /// </summary>
        private void Process()
        {
            var groupAggregateVarInfos = GroupAggregateRefComputingVisitor.Process(m_command, out m_tryGetParent);
            foreach (var groupAggregateVarInfo in groupAggregateVarInfos)
            {
                if (groupAggregateVarInfo.HasCandidateAggregateNodes)
                {
                    foreach (var candidate in groupAggregateVarInfo.CandidateAggregateNodes)
                    {
                        TryProcessCandidate(candidate, groupAggregateVarInfo);
                    }
                }
            }
        }

        /// <summary>
        ///     Try to push the given function aggregate candidate to the corresponding group into node.
        ///     The candidate can be pushed if all ancestors of the group into node up to the least common
        ///     ancestor between the group into node and the function aggregate have one of the following node op types:
        ///     Project
        ///     Filter
        ///     ConstraintSortOp
        /// </summary>
        /// <param name="command"> </param>
        /// <param name="candidate"> </param>
        /// <param name="groupAggregateVarInfo"> </param>
        /// <param name="m_childToParent"> </param>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "GroupByInto")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private void TryProcessCandidate(
            KeyValuePair<Node, Node> candidate,
            GroupAggregateVarInfo groupAggregateVarInfo)
        {
            IList<Node> functionAncestors;
            IList<Node> groupByAncestors;
            var definingGroupNode = groupAggregateVarInfo.DefiningGroupNode;
            FindPathsToLeastCommonAncestor(candidate.Key, definingGroupNode, out functionAncestors, out groupByAncestors);

            //Check whether all ancestors of the GroupByInto node are of type that we support propagating through
            if (!AreAllNodesSupportedForPropagation(groupByAncestors))
            {
                return;
            }

            //Add the function to the group by node
            var definingGroupOp = (GroupByIntoOp)definingGroupNode.Op;
            PlanCompiler.Assert(definingGroupOp.Inputs.Count == 1, "There should be one input var to GroupByInto at this stage");
            var inputVar = definingGroupOp.Inputs.First;
            var functionOp = (FunctionOp)candidate.Key.Op;

            //
            // Remap the template from referencing the groupAggregate var to reference the input to
            // the group by into
            //
            var argumentNode = OpCopier.Copy(m_command, candidate.Value);
            var dictionary = new Dictionary<Var, Var>(1);
            dictionary.Add(groupAggregateVarInfo.GroupAggregateVar, inputVar);
            var remapper = new VarRemapper(m_command, dictionary);
            remapper.RemapSubtree(argumentNode);

            var newFunctionDefiningNode = m_command.CreateNode(
                m_command.CreateAggregateOp(functionOp.Function, false),
                argumentNode);

            Var newFunctionVar;
            var varDefNode = m_command.CreateVarDefNode(newFunctionDefiningNode, out newFunctionVar);

            // Add the new aggregate to the list of aggregates
            definingGroupNode.Child2.Children.Add(varDefNode);
            var groupByOp = (GroupByIntoOp)definingGroupNode.Op;
            groupByOp.Outputs.Set(newFunctionVar);

            //Propagate the new var throught the ancestors of the GroupByInto
            for (var i = 0; i < groupByAncestors.Count; i++)
            {
                var groupByAncestor = groupByAncestors[i];
                if (groupByAncestor.Op.OpType
                    == OpType.Project)
                {
                    var ancestorProjectOp = (ProjectOp)groupByAncestor.Op;
                    ancestorProjectOp.Outputs.Set(newFunctionVar);
                }
            }

            //Update the functionNode
            candidate.Key.Op = m_command.CreateVarRefOp(newFunctionVar);
            candidate.Key.Children.Clear();
        }

        /// <summary>
        ///     Check whether all nodes in the given list of nodes are of types
        ///     that we know how to propagate an aggregate through
        /// </summary>
        /// <param name="nodes"> </param>
        /// <returns> </returns>
        private static bool AreAllNodesSupportedForPropagation(IList<Node> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.Op.OpType != OpType.Project
                    && node.Op.OpType != OpType.Filter
                    && node.Op.OpType != OpType.ConstrainedSort
                    )
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///     Finds the paths from each of node1 and node2 to their least common ancestor
        /// </summary>
        /// <param name="node1"> </param>
        /// <param name="node2"> </param>
        /// <param name="ancestors1"> </param>
        /// <param name="ancestors2"> </param>
        private void FindPathsToLeastCommonAncestor(Node node1, Node node2, out IList<Node> ancestors1, out IList<Node> ancestors2)
        {
            ancestors1 = FindAncestors(node1);
            ancestors2 = FindAncestors(node2);

            var currentIndex1 = ancestors1.Count - 1;
            var currentIndex2 = ancestors2.Count - 1;
            while (ancestors1[currentIndex1]
                   == ancestors2[currentIndex2])
            {
                currentIndex1--;
                currentIndex2--;
            }

            for (var i = ancestors1.Count - 1; i > currentIndex1; i--)
            {
                ancestors1.RemoveAt(i);
            }
            for (var i = ancestors2.Count - 1; i > currentIndex2; i--)
            {
                ancestors2.RemoveAt(i);
            }
        }

        /// <summary>
        ///     Finds all ancestors of the given node.
        /// </summary>
        /// <param name="node"> </param>
        /// <returns> An ordered list of the all the ancestors of the given node starting from the immediate parent to the root of the tree </returns>
        private IList<Node> FindAncestors(Node node)
        {
            var ancestors = new List<Node>();
            var currentNode = node;
            Node ancestor;
            while (m_tryGetParent(currentNode, out ancestor))
            {
                ancestors.Add(ancestor);
                currentNode = ancestor;
            }
            return ancestors;
        }

        #endregion
    }
}
