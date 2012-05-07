namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Represents Row contructor expression.
    /// </summary>
    internal sealed class RowConstructorExpr : Node
    {
        private readonly NodeList<AliasedExpr> _exprList;

        internal RowConstructorExpr(NodeList<AliasedExpr> exprList)
        {
            _exprList = exprList;
        }

        /// <summary>
        /// Returns list of elements as aliased expressions.
        /// </summary>
        internal NodeList<AliasedExpr> AliasedExprList
        {
            get { return _exprList; }
        }
    }
}