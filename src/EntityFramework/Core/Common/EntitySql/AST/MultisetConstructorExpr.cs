// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Represents multiset constructor expression.
    /// </summary>
    internal sealed class MultisetConstructorExpr : Node
    {
        private readonly NodeList<Node> _exprList;

        internal MultisetConstructorExpr(NodeList<Node> exprList)
        {
            _exprList = exprList;
        }

        /// <summary>
        /// Returns list of elements as alias expressions.
        /// </summary>
        internal NodeList<Node> ExprList
        {
            get { return _exprList; }
        }
    }
}
