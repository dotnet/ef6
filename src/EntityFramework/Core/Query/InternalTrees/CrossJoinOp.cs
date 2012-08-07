// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Diagnostics;

    /// <summary>
    ///     A CrossJoin (n-way)
    /// </summary>
    internal sealed class CrossJoinOp : JoinBaseOp
    {
        #region constructors

        private CrossJoinOp()
            : base(OpType.CrossJoin)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        ///     Singleton instance
        /// </summary>
        internal static readonly CrossJoinOp Instance = new CrossJoinOp();

        internal static readonly CrossJoinOp Pattern = Instance;

        /// <summary>
        ///     varying number of children (but usually greater than 1)
        /// </summary>
        internal override int Arity
        {
            get { return ArityVarying; }
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
