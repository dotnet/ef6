// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Diagnostics;

    /// <summary>
    ///     FilterOp
    /// </summary>
    internal sealed class FilterOp : RelOp
    {
        #region constructors

        private FilterOp()
            : base(OpType.Filter)
        {
        }

        #endregion

        #region public methods

        internal static readonly FilterOp Instance = new FilterOp();
        internal static readonly FilterOp Pattern = Instance;

        /// <summary>
        ///     2 children - input, pred
        /// </summary>
        internal override int Arity
        {
            get { return 2; }
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
