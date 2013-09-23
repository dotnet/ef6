// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    // <summary>
    // ANSI switched Case expression.
    // </summary>
    internal sealed class CaseOp : ScalarOp
    {
        #region constructors

        internal CaseOp(TypeUsage type)
            : base(OpType.Case, type)
        {
        }

        private CaseOp()
            : base(OpType.Case)
        {
        }

        #endregion

        #region public methods

        // <summary>
        // Pattern for use in transformation rules
        // </summary>
        internal static readonly CaseOp Pattern = new CaseOp();

        // <summary>
        // Visitor pattern method
        // </summary>
        // <param name="v"> The BasicOpVisitor that is visiting this Op </param>
        // <param name="n"> The Node that references this Op </param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n)
        {
            v.Visit(this, n);
        }

        // <summary>
        // Visitor pattern method for visitors with a return value
        // </summary>
        // <param name="v"> The visitor </param>
        // <param name="n"> The node in question </param>
        // <returns> An instance of TResultType </returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n)
        {
            return v.Visit(this, n);
        }

        #endregion
    }
}
