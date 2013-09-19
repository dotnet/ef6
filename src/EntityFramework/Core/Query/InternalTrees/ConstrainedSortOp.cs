// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// A Constrained SortOp. Used to represent physical paging (skip, limit, skip + limit) operations.
    /// </summary>
    internal sealed class ConstrainedSortOp : SortBaseOp
    {
        #region private state

        #endregion

        #region constructors

        // Pattern constructor
        private ConstrainedSortOp()
            : base(OpType.ConstrainedSort)
        {
        }

        internal ConstrainedSortOp(List<SortKey> sortKeys, bool withTies)
            : base(OpType.ConstrainedSort, sortKeys)
        {
            WithTies = withTies;
        }

        #endregion

        #region public methods

        internal bool WithTies { get; set; }

        internal static readonly ConstrainedSortOp Pattern = new ConstrainedSortOp();

        /// <summary>
        /// 3 children - the input, a possibly NullOp limit and a possibly NullOp skip count.
        /// </summary>
        internal override int Arity
        {
            get { return 3; }
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
