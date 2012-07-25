// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Data.Entity.Core.Query.InternalTrees;

    /// <summary>
    /// Transformation Rules for SortOp
    /// </summary>
    internal static class SortOpRules
    {
        #region SortOpOverAtMostOneRow

        internal static readonly SimpleRule Rule_SortOpOverAtMostOneRow = new SimpleRule(OpType.Sort, ProcessSortOpOverAtMostOneRow);

        /// <summary>
        /// If the SortOp's input is guaranteed to produce at most 1 row, remove the node with the SortOp:
        ///  Sort(X) => X, if X is guaranteed to produce no more than 1 row
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="n">current subtree</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>transformation status</returns>
        private static bool ProcessSortOpOverAtMostOneRow(RuleProcessingContext context, Node n, out Node newNode)
        {
            var nodeInfo = (context).Command.GetExtendedNodeInfo(n.Child0);

            //If the input has at most one row, omit the SortOp
            if (nodeInfo.MaxRows == RowCount.Zero
                || nodeInfo.MaxRows == RowCount.One)
            {
                newNode = n.Child0;
                return true;
            }

            //Otherwise return the node as is
            newNode = n;
            return false;
        }

        #endregion

        #region All SortOp Rules

        internal static readonly Rule[] Rules = new Rule[]
            {
                Rule_SortOpOverAtMostOneRow,
            };

        #endregion
    }
}
