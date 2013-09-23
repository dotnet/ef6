// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Data.Entity.Core.Query.InternalTrees;

    // <summary>
    // Transformation Rules for DistinctOp
    // </summary>
    internal static class DistinctOpRules
    {
        #region DistinctOpOfKeys

        internal static readonly SimpleRule Rule_DistinctOpOfKeys = new SimpleRule(OpType.Distinct, ProcessDistinctOpOfKeys);

        // <summary>
        // If the DistinctOp includes all all the keys of the input, than it is unnecessary.
        // Distinct (X, distinct_keys) -> Project( X, distinct_keys) where distinct_keys includes all keys of X.
        // </summary>
        // <param name="context"> Rule processing context </param>
        // <param name="n"> current subtree </param>
        // <param name="newNode"> transformed subtree </param>
        // <returns> transformation status </returns>
        private static bool ProcessDistinctOpOfKeys(RuleProcessingContext context, Node n, out Node newNode)
        {
            var command = context.Command;

            var nodeInfo = command.GetExtendedNodeInfo(n.Child0);

            var op = (DistinctOp)n.Op;

            //If we know the keys of the input and the list of distinct keys includes them all, omit the distinct
            if (!nodeInfo.Keys.NoKeys
                && op.Keys.Subsumes(nodeInfo.Keys.KeyVars))
            {
                var newOp = command.CreateProjectOp(op.Keys);

                //Create empty vardef list
                var varDefListOp = command.CreateVarDefListOp();
                var varDefListNode = command.CreateNode(varDefListOp);

                newNode = command.CreateNode(newOp, n.Child0, varDefListNode);
                return true;
            }

            //Otherwise return the node as is
            newNode = n;
            return false;
        }

        #endregion

        #region All DistinctOp Rules

        internal static readonly Rule[] Rules = new Rule[]
            {
                Rule_DistinctOpOfKeys,
            };

        #endregion
    }
}
