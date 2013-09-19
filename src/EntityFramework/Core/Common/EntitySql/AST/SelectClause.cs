// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Represents select clause.
    /// </summary>
    internal sealed class SelectClause : Node
    {
        private readonly NodeList<AliasedExpr> _selectClauseItems;
        private readonly SelectKind _selectKind;
        private readonly DistinctKind _distinctKind;
        private readonly Node _topExpr;
        private readonly uint _methodCallCount;

        /// <summary>
        /// Initialize SelectKind.SelectRow clause.
        /// </summary>
        internal SelectClause(
            NodeList<AliasedExpr> items, SelectKind selectKind, DistinctKind distinctKind, Node topExpr, uint methodCallCount)
        {
            _selectKind = selectKind;
            _selectClauseItems = items;
            _distinctKind = distinctKind;
            _topExpr = topExpr;
            _methodCallCount = methodCallCount;
        }

        /// <summary>
        /// Projection list.
        /// </summary>
        internal NodeList<AliasedExpr> Items
        {
            get { return _selectClauseItems; }
        }

        /// <summary>
        /// Select kind (row or value).
        /// </summary>
        internal SelectKind SelectKind
        {
            get { return _selectKind; }
        }

        /// <summary>
        /// Distinct kind (none,all,distinct).
        /// </summary>
        internal DistinctKind DistinctKind
        {
            get { return _distinctKind; }
        }

        /// <summary>
        /// Optional top expression.
        /// </summary>
        internal Node TopExpr
        {
            get { return _topExpr; }
        }

        /// <summary>
        /// True if select list has method calls.
        /// </summary>
        internal bool HasMethodCall
        {
            get { return (_methodCallCount > 0); }
        }
    }
}
