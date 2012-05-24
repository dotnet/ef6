namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// An internal cast operation. (Softly) Convert a type instance into an instance of another type
    /// 
    /// This Op is intended to capture "promotion" semantics. (ie) int16 promotes to an int32; Customer promotes to Person
    /// etc. This Op is intended to shield the PlanCompiler from having to reason about 
    /// the promotion semantics; and is intended to make the query tree very 
    /// explicit
    /// 
    /// </summary>
    internal sealed class SoftCastOp : ScalarOp
    {
        #region constructors

        internal SoftCastOp(TypeUsage type)
            : base(OpType.SoftCast, type)
        {
        }

        private SoftCastOp()
            : base(OpType.SoftCast)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly SoftCastOp Pattern = new SoftCastOp();

        /// <summary>
        /// 1 child - input expression
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
