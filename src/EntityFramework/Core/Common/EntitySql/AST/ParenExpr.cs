namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    using System.Diagnostics;

    /// <summary>
    /// Represents a paren expression ast node.
    /// </summary>
    internal sealed class ParenExpr : Node
    {
        private readonly Node _expr;

        /// <summary>
        /// Initializes paren expression.
        /// </summary>
        internal ParenExpr(Node expr)
        {
            Debug.Assert(expr != null, "expr != null");
            _expr = expr;
        }

        /// <summary>
        /// Returns the parenthesized expression.
        /// </summary>
        internal Node Expr
        {
            get { return _expr; }
        }
    }
}
