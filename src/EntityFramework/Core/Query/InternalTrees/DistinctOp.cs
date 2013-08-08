// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    /// DistinctOp
    /// </summary>
    internal sealed class DistinctOp : RelOp
    {
        #region private state

        private readonly VarVec m_keys;

        #endregion

        #region constructors

        private DistinctOp()
            : base(OpType.Distinct)
        {
        }

        internal DistinctOp(VarVec keyVars)
            : this()
        {
            DebugCheck.NotNull(keyVars);
            Debug.Assert(!keyVars.IsEmpty);
            m_keys = keyVars;
        }

        #endregion

        #region public methods

        internal static readonly DistinctOp Pattern = new DistinctOp();

        /// <summary>
        /// 1 child - input
        /// </summary>
        internal override int Arity
        {
            get { return 1; }
        }

        /// <summary>
        /// Get "key" vars for the distinct
        /// </summary>
        internal VarVec Keys
        {
            get { return m_keys; }
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
