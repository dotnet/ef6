// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Diagnostics;

    /// <summary>
    ///     Represents a table with a single row
    /// </summary>
    internal sealed class SingleRowTableOp : RelOp
    {
        #region constructors

        private SingleRowTableOp()
            : base(OpType.SingleRowTable)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        ///     Singleton instance
        /// </summary>
        internal static readonly SingleRowTableOp Instance = new SingleRowTableOp();

        /// <summary>
        ///     Pattern for transformation rules
        /// </summary>
        internal static readonly SingleRowTableOp Pattern = Instance;

        /// <summary>
        ///     0 children
        /// </summary>
        internal override int Arity
        {
            get { return 0; }
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
