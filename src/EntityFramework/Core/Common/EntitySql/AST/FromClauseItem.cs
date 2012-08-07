// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    ///     Represents single from clause item.
    /// </summary>
    internal sealed class FromClauseItem : Node
    {
        private readonly Node _fromClauseItemExpr;
        private readonly FromClauseItemKind _fromClauseItemKind;

        /// <summary>
        ///     Initializes as 'simple' aliased expression.
        /// </summary>
        internal FromClauseItem(AliasedExpr aliasExpr)
        {
            _fromClauseItemExpr = aliasExpr;
            _fromClauseItemKind = FromClauseItemKind.AliasedFromClause;
        }

        /// <summary>
        ///     Initializes as join clause item.
        /// </summary>
        internal FromClauseItem(JoinClauseItem joinClauseItem)
        {
            _fromClauseItemExpr = joinClauseItem;
            _fromClauseItemKind = FromClauseItemKind.JoinFromClause;
        }

        /// <summary>
        ///     Initializes as apply clause item.
        /// </summary>
        internal FromClauseItem(ApplyClauseItem applyClauseItem)
        {
            _fromClauseItemExpr = applyClauseItem;
            _fromClauseItemKind = FromClauseItemKind.ApplyFromClause;
        }

        /// <summary>
        ///     From clause item expression.
        /// </summary>
        internal Node FromExpr
        {
            get { return _fromClauseItemExpr; }
        }

        /// <summary>
        ///     From clause item kind (alias,join,apply).
        /// </summary>
        internal FromClauseItemKind FromClauseItemKind
        {
            get { return _fromClauseItemKind; }
        }
    }
}
