// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     AST node for an aliased expression.
    /// </summary>
    internal sealed class AliasedExpr : Node
    {
        private readonly Node _expr;
        private readonly Identifier _alias;

        /// <summary>
        ///     Constructs an aliased expression node.
        /// </summary>
        internal AliasedExpr(Node expr, Identifier alias)
        {
            DebugCheck.NotNull(expr);
            DebugCheck.NotNull(alias);

            if (String.IsNullOrEmpty(alias.Name))
            {
                var errCtx = alias.ErrCtx;
                var message = Strings.InvalidEmptyIdentifier;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            _expr = expr;
            _alias = alias;
        }

        /// <summary>
        ///     Constructs an aliased expression node with null alias.
        /// </summary>
        internal AliasedExpr(Node expr)
        {
            DebugCheck.NotNull(expr);

            _expr = expr;
        }

        internal Node Expr
        {
            get { return _expr; }
        }

        /// <summary>
        ///     Returns expression alias identifier, or null if not aliased.
        /// </summary>
        internal Identifier Alias
        {
            get { return _alias; }
        }
    }
}
