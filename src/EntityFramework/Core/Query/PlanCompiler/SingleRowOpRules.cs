// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Data.Entity.Core.Query.InternalTrees;

    /// <summary>
    /// Rules for SingleRowOp
    /// </summary>
    internal static class SingleRowOpRules
    {
        internal static readonly PatternMatchRule Rule_SingleRowOpOverAnything =
            new PatternMatchRule(
                new Node(
                    SingleRowOp.Pattern,
                    new Node(LeafOp.Pattern)),
                ProcessSingleRowOpOverAnything);

        /// <summary>
        /// Convert a 
        ///    SingleRowOp(X) => X
        /// if X produces at most one row
        /// </summary>
        /// <param name="context">Rule Processing context</param>
        /// <param name="singleRowNode">Current subtree</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>Transformation status</returns>
        private static bool ProcessSingleRowOpOverAnything(RuleProcessingContext context, Node singleRowNode, out Node newNode)
        {
            newNode = singleRowNode;
            var trc = (TransformationRulesContext)context;
            var childNodeInfo = context.Command.GetExtendedNodeInfo(singleRowNode.Child0);

            // If the input to this Op can produce at most one row, then we don't need the
            // singleRowOp - simply return the input
            if (childNodeInfo.MaxRows
                <= RowCount.One)
            {
                newNode = singleRowNode.Child0;
                return true;
            }

            //
            // if the current node is a FilterOp, then try and determine if the FilterOp
            // produces one row at most
            //
            if (singleRowNode.Child0.Op.OpType
                == OpType.Filter)
            {
                var predicate = new Predicate(context.Command, singleRowNode.Child0.Child1);
                if (predicate.SatisfiesKey(childNodeInfo.Keys.KeyVars, childNodeInfo.Definitions))
                {
                    childNodeInfo.MaxRows = RowCount.One;
                    newNode = singleRowNode.Child0;
                    return true;
                }
            }

            // we couldn't do anything
            return false;
        }

        internal static readonly PatternMatchRule Rule_SingleRowOpOverProject =
            new PatternMatchRule(
                new Node(
                    SingleRowOp.Pattern,
                    new Node(
                        ProjectOp.Pattern,
                        new Node(LeafOp.Pattern), new Node(LeafOp.Pattern))),
                ProcessSingleRowOpOverProject);

        /// <summary>
        /// Convert 
        ///    SingleRowOp(Project) => Project(SingleRowOp)
        /// </summary>
        /// <param name="context">Rule Processing context</param>
        /// <param name="singleRowNode">current subtree</param>
        /// <param name="newNode">transformeed subtree</param>
        /// <returns>transformation status</returns>
        private static bool ProcessSingleRowOpOverProject(RuleProcessingContext context, Node singleRowNode, out Node newNode)
        {
            newNode = singleRowNode;
            var projectNode = singleRowNode.Child0;
            var projectNodeInput = projectNode.Child0;

            // Simply push the SingleRowOp below the ProjectOp
            singleRowNode.Child0 = projectNodeInput;
            context.Command.RecomputeNodeInfo(singleRowNode);
            projectNode.Child0 = singleRowNode;

            newNode = projectNode;
            return true; // subtree modified internally
        }

        #region All SingleRowOp Rules

        internal static readonly Rule[] Rules = new Rule[]
            {
                Rule_SingleRowOpOverAnything,
                Rule_SingleRowOpOverProject,
            };

        #endregion
    }
}
