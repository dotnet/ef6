// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Query.InternalTrees
{
    /// <summary>
    /// All rule pattern operators - Leaf, Tree
    /// </summary>
    internal abstract class RulePatternOp : Op
    {
        #region constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="opType">kind of Op</param>
        internal RulePatternOp(OpType opType)
            : base(opType)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// RulePatternOp
        /// </summary>
        internal override bool IsRulePatternOp
        {
            get { return true; }
        }

        #endregion
    }
}
