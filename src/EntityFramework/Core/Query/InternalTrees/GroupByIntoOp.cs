// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Diagnostics;

    /// <summary>
    /// GroupByIntoOp
    /// </summary>
    internal sealed class GroupByIntoOp : GroupByBaseOp
    {
        #region private state

        private readonly VarVec m_inputs;

        #endregion

        #region constructors

        private GroupByIntoOp()
            : base(OpType.GroupByInto)
        {
        }

        internal GroupByIntoOp(VarVec keys, VarVec inputs, VarVec outputs)
            : base(OpType.GroupByInto, keys, outputs)
        {
            m_inputs = inputs;
        }

        #endregion

        #region public methods

        /// <summary>
        /// GroupBy keys
        /// </summary>
        internal VarVec Inputs
        {
            get { return m_inputs; }
        }

        internal static readonly GroupByIntoOp Pattern = new GroupByIntoOp();

        /// <summary>
        /// 4 children - input, keys (vardeflist), aggregates (vardeflist), groupaggregates (vardeflist)
        /// </summary>
        internal override int Arity
        {
            get { return 4; }
        }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n)
        {
            v.Visit(this, n);
        }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n)
        {
            return v.Visit(this, n);
        }

        #endregion
    }
}
