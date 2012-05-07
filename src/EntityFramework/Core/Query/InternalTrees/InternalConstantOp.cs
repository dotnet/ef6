namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents internally generated constants
    /// </summary>
    internal sealed class InternalConstantOp : ConstantBaseOp
    {
        #region constructors

        internal InternalConstantOp(TypeUsage type, object value)
            : base(OpType.InternalConstant, type, value)
        {
            Debug.Assert(value != null, "InternalConstantOp with a null value?");
        }

        private InternalConstantOp()
            : base(OpType.InternalConstant)
        {
        }

        #endregion

        #region public apis

        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly InternalConstantOp Pattern = new InternalConstantOp();

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