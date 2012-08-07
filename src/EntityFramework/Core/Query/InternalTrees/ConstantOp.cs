// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    ///     Represents an external constant
    /// </summary>
    internal sealed class ConstantOp : ConstantBaseOp
    {
        #region constructors

        internal ConstantOp(TypeUsage type, object value)
            : base(OpType.Constant, type, value)
        {
            Debug.Assert(value != null, "ConstantOp with a null value?");
        }

        private ConstantOp()
            : base(OpType.Constant)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        ///     Pattern for transformation rules
        /// </summary>
        internal static readonly ConstantOp Pattern = new ConstantOp();

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
