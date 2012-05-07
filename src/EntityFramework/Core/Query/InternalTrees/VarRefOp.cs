namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Diagnostics;

    /// <summary>
    /// A reference to an existing variable
    /// </summary>
    internal sealed class VarRefOp : ScalarOp
    {
        #region private state

        private readonly Var m_var;

        #endregion

        #region constructors

        internal VarRefOp(Var v)
            : base(OpType.VarRef, v.Type)
        {
            m_var = v;
        }

        private VarRefOp()
            : base(OpType.VarRef)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// Singleton used for pattern matching
        /// </summary>
        internal static readonly VarRefOp Pattern = new VarRefOp();

        /// <summary>
        /// 0 children
        /// </summary>
        internal override int Arity
        {
            get { return 0; }
        }

        /// <summary>
        /// Two VarRefOps are equivalent, if they reference the same Var
        /// </summary>
        /// <param name="other">the other Op</param>
        /// <returns>true, if these are equivalent</returns>
        internal override bool IsEquivalent(Op other)
        {
            var otherVarRef = other as VarRefOp;
            return (otherVarRef != null && otherVarRef.Var.Equals(Var));
        }

        /// <summary>
        /// The Var that this Op is referencing
        /// </summary>
        internal Var Var
        {
            get { return m_var; }
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