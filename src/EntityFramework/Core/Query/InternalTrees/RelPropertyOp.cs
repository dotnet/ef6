// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Almost identical to a PropertyOp - the only difference being that we're dealing with an
    /// "extended" property (a rel property) this time
    /// </summary>
    internal sealed class RelPropertyOp : ScalarOp
    {
        #region private state

        private readonly RelProperty m_property;

        #endregion

        #region constructors

        private RelPropertyOp()
            : base(OpType.RelProperty)
        {
        }

        internal RelPropertyOp(TypeUsage type, RelProperty property)
            : base(OpType.RelProperty, type)
        {
            m_property = property;
        }

        #endregion

        #region public APIs

        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly RelPropertyOp Pattern = new RelPropertyOp();

        /// <summary>
        /// 1 child - the entity instance
        /// </summary>
        internal override int Arity
        {
            get { return 1; }
        }

        /// <summary>
        /// Get the property metadata
        /// </summary>
        public RelProperty PropertyInfo
        {
            get { return m_property; }
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
