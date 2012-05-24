namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Diagnostics;

    /// <summary>
    /// Selects out a single row from a underlying subquery. Two flavors of this Op exist.
    /// The first flavor enforces the single-row-ness (ie) an error is raised if the
    /// underlying subquery produces more than one row.
    /// The other flavor simply choses any row from the input
    /// </summary>
    internal sealed class SingleRowOp : RelOp
    {
        #region constructors

        private SingleRowOp()
            : base(OpType.SingleRow)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// Singleton instance
        /// </summary>
        internal static readonly SingleRowOp Instance = new SingleRowOp();

        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly SingleRowOp Pattern = Instance;

        /// <summary>
        /// 1 child - input
        /// </summary>
        internal override int Arity
        {
            get { return 1; }
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
