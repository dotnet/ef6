namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents an internally generated constant that is used to serve as a null sentinel, 
    /// i.e. to be checked whether it is null.
    /// </summary>
    internal sealed class NullSentinelOp : ConstantBaseOp
    {
        #region constructors

        internal NullSentinelOp(TypeUsage type, object value)
            : base(OpType.NullSentinel, type, value)
        {
        }

        private NullSentinelOp()
            : base(OpType.NullSentinel)
        {
        }

        #endregion

        #region public apis

        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly NullSentinelOp Pattern = new NullSentinelOp();

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
