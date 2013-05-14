// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    internal sealed class RefOp : ScalarOp
    {
        #region private state

        private readonly EntitySet m_entitySet;

        #endregion

        #region constructors

        internal RefOp(EntitySet entitySet, TypeUsage type)
            : base(OpType.Ref, type)
        {
            m_entitySet = entitySet;
        }

        private RefOp()
            : base(OpType.Ref)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        ///     Pattern for transformation rules
        /// </summary>
        internal static readonly RefOp Pattern = new RefOp();

        /// <summary>
        ///     1 child - key
        /// </summary>
        internal override int Arity
        {
            get { return 1; }
        }

        /// <summary>
        ///     The EntitySet to which the reference refers
        /// </summary>
        internal EntitySet EntitySet
        {
            get { return m_entitySet; }
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
