namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Diagnostics;

    /// <summary>
    /// UnionAll (ie) no duplicate elimination
    /// </summary>
    internal sealed class UnionAllOp : SetOp
    {
        #region private state

        private readonly Var m_branchDiscriminator;

        #endregion

        #region constructors

        private UnionAllOp()
            : base(OpType.UnionAll)
        {
        }

        internal UnionAllOp(VarVec outputs, VarMap left, VarMap right, Var branchDiscriminator)
            : base(OpType.UnionAll, outputs, left, right)
        {
            m_branchDiscriminator = branchDiscriminator;
        }

        #endregion

        #region public methods

        internal static readonly UnionAllOp Pattern = new UnionAllOp();

        /// <summary>
        /// Returns the branch discriminator var for this op.  It may be null, if
        /// we haven't been through key pullup yet.
        /// </summary>
        internal Var BranchDiscriminator
        {
            get { return m_branchDiscriminator; }
        }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n)
        {
            v.Visit(this, n);
        }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n)
        {
            return v.Visit(this, n);
        }

        #endregion
    }
}