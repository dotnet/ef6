// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    ///     Represents a TREAT AS operation
    /// </summary>
    internal sealed class TreatOp : ScalarOp
    {
        #region private state

        private readonly bool m_isFake;

        #endregion

        #region constructors

        internal TreatOp(TypeUsage type, bool isFake)
            : base(OpType.Treat, type)
        {
            m_isFake = isFake;
        }

        private TreatOp()
            : base(OpType.Treat)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        ///     Used as patterns in transformation rules
        /// </summary>
        internal static readonly TreatOp Pattern = new TreatOp();

        /// <summary>
        ///     1 child - instance
        /// </summary>
        internal override int Arity
        {
            get { return 1; }
        }

        /// <summary>
        ///     Is this a "fake" treat?
        /// </summary>
        internal bool IsFakeTreat
        {
            get { return m_isFake; }
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
