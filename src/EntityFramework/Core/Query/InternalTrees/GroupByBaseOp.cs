// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Diagnostics;

    /// <summary>
    /// GroupByBaseOp
    /// </summary>
    internal abstract class GroupByBaseOp : RelOp
    {
        #region private state

        private readonly VarVec m_keys;
        private readonly VarVec m_outputs;

        #endregion

        #region constructors

        protected GroupByBaseOp(OpType opType)
            : base(opType)
        {
            Debug.Assert(opType == OpType.GroupBy || opType == OpType.GroupByInto, "GroupByBaseOp OpType must be GroupBy or GroupByInto");
        }

        internal GroupByBaseOp(OpType opType, VarVec keys, VarVec outputs)
            : this(opType)
        {
            m_keys = keys;
            m_outputs = outputs;
        }

        #endregion

        #region public methods

        /// <summary>
        /// GroupBy keys
        /// </summary>
        internal VarVec Keys
        {
            get { return m_keys; }
        }

        /// <summary>
        /// All outputs of this Op - includes keys and aggregates
        /// </summary>
        internal VarVec Outputs
        {
            get { return m_outputs; }
        }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v"> The BasicOpVisitor that is visiting this Op </param>
        /// <param name="n"> The Node that references this Op </param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n)
        {
            v.Visit(this, n);
        }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v"> The visitor </param>
        /// <param name="n"> The node in question </param>
        /// <returns> An instance of TResultType </returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n)
        {
            return v.Visit(this, n);
        }

        #endregion
    }
}
