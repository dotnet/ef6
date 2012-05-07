namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Diagnostics;

    /// <summary>
    /// Scans a view - very similar to a ScanTable
    /// </summary>
    internal sealed class ScanViewOp : ScanTableBaseOp
    {
        #region constructors

        /// <summary>
        /// Scan constructor
        /// </summary>
        /// <param name="table"></param>
        internal ScanViewOp(Table table)
            : base(OpType.ScanView, table)
        {
        }

        private ScanViewOp()
            : base(OpType.ScanView)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// Only to be used for pattern matches
        /// </summary>
        internal static readonly ScanViewOp Pattern = new ScanViewOp();

        /// <summary>
        /// Exactly 1 child
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