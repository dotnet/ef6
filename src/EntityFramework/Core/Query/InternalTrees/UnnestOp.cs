// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Diagnostics;

    // <summary>
    // Scans a virtual extent (ie) a transient collection
    // </summary>
    internal sealed class UnnestOp : RelOp
    {
        #region private state

        private readonly Table m_table;
        private readonly Var m_var;

        #endregion

        #region constructors

        internal UnnestOp(Var v, Table t)
            : this()
        {
            m_var = v;
            m_table = t;
        }

        private UnnestOp()
            : base(OpType.Unnest)
        {
        }

        #endregion

        #region publics

        internal static readonly UnnestOp Pattern = new UnnestOp();

        // <summary>
        // The (collection-typed) Var that's being unnested
        // </summary>
        internal Var Var
        {
            get { return m_var; }
        }

        // <summary>
        // The table instance produced by this Op
        // </summary>
        internal Table Table
        {
            get { return m_table; }
        }

        // <summary>
        // Exactly 1 child
        // </summary>
        internal override int Arity
        {
            get { return 1; }
        }

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
