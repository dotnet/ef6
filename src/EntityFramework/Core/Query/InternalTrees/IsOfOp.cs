// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// An IS OF operation
    /// </summary>
    internal sealed class IsOfOp : ScalarOp
    {
        #region private state

        private readonly TypeUsage m_isOfType;
        private readonly bool m_isOfOnly;

        #endregion

        #region constructors

        internal IsOfOp(TypeUsage isOfType, bool isOfOnly, TypeUsage type)
            : base(OpType.IsOf, type)
        {
            m_isOfType = isOfType;
            m_isOfOnly = isOfOnly;
        }

        private IsOfOp()
            : base(OpType.IsOf)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// Pattern used for transformation rules
        /// </summary>
        internal static readonly IsOfOp Pattern = new IsOfOp();

        /// <summary>
        /// 1 child - instance
        /// </summary>
        internal override int Arity
        {
            get { return 1; }
        }

        /// <summary>
        /// The type being checked for
        /// </summary>
        internal TypeUsage IsOfType
        {
            get { return m_isOfType; }
        }

        internal bool IsOfOnly
        {
            get { return m_isOfOnly; }
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
