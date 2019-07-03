// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    // <summary>
    // Represents a query expression ast node.
    // </summary>
    internal sealed class QueryExpr : Node
    {
        private readonly SelectClause _selectClause;
        private readonly FromClause _fromClause;
        private readonly Node _whereClause;
        private readonly GroupByClause _groupByClause;
        private readonly HavingClause _havingClause;
        private readonly OrderByClause _orderByClause;

        // <summary>
        // Initializes a query expression ast node.
        // </summary>
        // <param name="selectClause"> select clause </param>
        // <param name="fromClause"> from clause </param>
        // <param name="whereClause"> optional where clause </param>
        // <param name="groupByClause"> optional group by clause </param>
        // <param name="havingClause"> optional having clause </param>
        // <param name="orderByClause"> optional order by clause </param>
        internal QueryExpr(
            SelectClause selectClause,
            FromClause fromClause,
            Node whereClause,
            GroupByClause groupByClause,
            HavingClause havingClause,
            OrderByClause orderByClause)
        {
            _selectClause = selectClause;
            _fromClause = fromClause;
            _whereClause = whereClause;
            _groupByClause = groupByClause;
            _havingClause = havingClause;
            _orderByClause = orderByClause;
        }

        // <summary>
        // Returns select clause.
        // </summary>
        internal SelectClause SelectClause
        {
            get { return _selectClause; }
        }

        // <summary>
        // Returns from clause.
        // </summary>
        internal FromClause FromClause
        {
            get { return _fromClause; }
        }

        // <summary>
        // Returns optional where clause (expr).
        // </summary>
        internal Node WhereClause
        {
            get { return _whereClause; }
        }

        // <summary>
        // Returns optional group by clause.
        // </summary>
        internal GroupByClause GroupByClause
        {
            get { return _groupByClause; }
        }

        // <summary>
        // Returns optional having clause (expr).
        // </summary>
        internal HavingClause HavingClause
        {
            get { return _havingClause; }
        }

        // <summary>
        // Returns optional order by clause.
        // </summary>
        internal OrderByClause OrderByClause
        {
            get { return _orderByClause; }
        }

        // <summary>
        // Returns true if method calls are present.
        // </summary>
        internal bool HasMethodCall
        {
            get
            {
                return _selectClause.HasMethodCall ||
                       (null != _havingClause && _havingClause.HasMethodCall) ||
                       (null != _orderByClause && _orderByClause.HasMethodCall);
            }
        }
    }
}
