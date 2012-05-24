namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Represents the when then sub expression.
    /// </summary>
    internal class WhenThenExpr : Node
    {
        private readonly Node _whenExpr;
        private readonly Node _thenExpr;

        /// <summary>
        /// Initializes WhenThen sub-expression.
        /// </summary>
        /// <param name="whenExpr">When expression</param>
        /// <param name="thenExpr">Then expression</param>
        internal WhenThenExpr(Node whenExpr, Node thenExpr)
        {
            _whenExpr = whenExpr;
            _thenExpr = thenExpr;
        }

        /// <summary>
        /// Returns When expression.
        /// </summary>
        internal Node WhenExpr
        {
            get { return _whenExpr; }
        }

        /// <summary>
        /// Returns Then Expression.
        /// </summary>
        internal Node ThenExpr
        {
            get { return _thenExpr; }
        }
    }
}
