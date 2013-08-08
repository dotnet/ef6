// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Represents order by clause.
    /// </summary>
    internal sealed class OrderByClause : Node
    {
        private readonly NodeList<OrderByClauseItem> _orderByClauseItem;
        private readonly Node _skipExpr;
        private readonly Node _limitExpr;
        private readonly uint _methodCallCount;

        /// <summary>
        /// Initializes order by clause.
        /// </summary>
        internal OrderByClause(NodeList<OrderByClauseItem> orderByClauseItem, Node skipExpr, Node limitExpr, uint methodCallCount)
        {
            _orderByClauseItem = orderByClauseItem;
            _skipExpr = skipExpr;
            _limitExpr = limitExpr;
            _methodCallCount = methodCallCount;
        }

        /// <summary>
        /// Returns order by clause items.
        /// </summary>
        internal NodeList<OrderByClauseItem> OrderByClauseItem
        {
            get { return _orderByClauseItem; }
        }

        /// <summary>
        /// Returns skip sub clause ast node.
        /// </summary>
        internal Node SkipSubClause
        {
            get { return _skipExpr; }
        }

        /// <summary>
        /// Returns limit sub-clause ast node.
        /// </summary>
        internal Node LimitSubClause
        {
            get { return _limitExpr; }
        }

        /// <summary>
        /// True if order by has method calls.
        /// </summary>
        internal bool HasMethodCall
        {
            get { return (_methodCallCount > 0); }
        }
    }
}
