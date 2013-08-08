// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Represents join clause item.
    /// </summary>
    internal sealed class JoinClauseItem : Node
    {
        private readonly FromClauseItem _joinLeft;
        private readonly FromClauseItem _joinRight;
        private readonly Node _onExpr;

        /// <summary>
        /// Initializes join clause item without ON expression.
        /// </summary>
        internal JoinClauseItem(FromClauseItem joinLeft, FromClauseItem joinRight, JoinKind joinKind)
            : this(joinLeft, joinRight, joinKind, null)
        {
        }

        /// <summary>
        /// Initializes join clause item with ON expression.
        /// </summary>
        internal JoinClauseItem(FromClauseItem joinLeft, FromClauseItem joinRight, JoinKind joinKind, Node onExpr)
        {
            _joinLeft = joinLeft;
            _joinRight = joinRight;
            JoinKind = joinKind;
            _onExpr = onExpr;
        }

        /// <summary>
        /// Returns join left expression.
        /// </summary>
        internal FromClauseItem LeftExpr
        {
            get { return _joinLeft; }
        }

        /// <summary>
        /// Returns join right expression.
        /// </summary>
        internal FromClauseItem RightExpr
        {
            get { return _joinRight; }
        }

        /// <summary>
        /// Join kind (cross, inner, full, left outer,right outer).
        /// </summary>
        internal JoinKind JoinKind { get; set; }

        /// <summary>
        /// Returns join on expression.
        /// </summary>
        internal Node OnExpr
        {
            get { return _onExpr; }
        }
    }
}
