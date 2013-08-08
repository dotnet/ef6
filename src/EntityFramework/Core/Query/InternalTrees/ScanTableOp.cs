// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Diagnostics;

    /// <summary>
    /// Scans a table
    /// </summary>
    internal sealed class ScanTableOp : ScanTableBaseOp
    {
        #region constructors

        /// <summary>
        /// Scan constructor
        /// </summary>
        internal ScanTableOp(Table table)
            : base(OpType.ScanTable, table)
        {
        }

        private ScanTableOp()
            : base(OpType.ScanTable)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// Only to be used for pattern matches
        /// </summary>
        internal static readonly ScanTableOp Pattern = new ScanTableOp();

        /// <summary>
        /// No children
        /// </summary>
        internal override int Arity
        {
            get { return 0; }
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
