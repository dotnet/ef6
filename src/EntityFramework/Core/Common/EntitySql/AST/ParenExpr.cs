// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Represents a paren expression ast node.
    /// </summary>
    internal sealed class ParenExpr : Node
    {
        private readonly Node _expr;

        /// <summary>
        ///     Initializes paren expression.
        /// </summary>
        internal ParenExpr(Node expr)
        {
            DebugCheck.NotNull(expr);
            _expr = expr;
        }

        /// <summary>
        ///     Returns the parenthesized expression.
        /// </summary>
        internal Node Expr
        {
            get { return _expr; }
        }
    }
}
