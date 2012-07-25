// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Query.InternalTrees
{
    /// <summary>
    /// A SimpleRule is a rule that specifies a specific OpType to look for, and an
    /// appropriate action to take when such an Op is identified
    /// </summary>
    internal sealed class SimpleRule : Rule
    {
        #region private state

        #endregion

        #region constructors

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="opType">The OpType we're interested in</param>
        /// <param name="processDelegate">The callback to invoke when we see such an Op</param>
        internal SimpleRule(OpType opType, ProcessNodeDelegate processDelegate)
            : base(opType, processDelegate)
        {
        }

        #endregion

        #region overriden methods

        internal override bool Match(Node node)
        {
            return node.Op.OpType == RuleOpType;
        }

        #endregion
    }
}
