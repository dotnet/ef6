// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Diagnostics;

    /// <summary>
    ///     GroupByOp
    /// </summary>
    internal sealed class GroupByOp : GroupByBaseOp
    {
        #region constructors

        private GroupByOp()
            : base(OpType.GroupBy)
        {
        }

        internal GroupByOp(VarVec keys, VarVec outputs)
            : base(OpType.GroupBy, keys, outputs)
        {
        }

        #endregion

        #region public methods

        internal static readonly GroupByOp Pattern = new GroupByOp();

        /// <summary>
        ///     3 children - input, keys (vardeflist), aggregates (vardeflist)
        /// </summary>
        internal override int Arity
        {
            get { return 3; }
        }

        /// <summary>
        ///     Visitor pattern method
        /// </summary>
        /// <param name="v"> The BasicOpVisitor that is visiting this Op </param>
        /// <param name="n"> The Node that references this Op </param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n)
        {
            v.Visit(this, n);
        }

        /// <summary>
        ///     Visitor pattern method for visitors with a return value
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
