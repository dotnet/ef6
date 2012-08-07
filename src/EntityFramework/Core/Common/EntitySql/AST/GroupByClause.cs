// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    ///     Represents group by clause.
    /// </summary>
    internal sealed class GroupByClause : Node
    {
        private readonly NodeList<AliasedExpr> _groupItems;

        /// <summary>
        ///     Initializes GROUP BY clause
        /// </summary>
        internal GroupByClause(NodeList<AliasedExpr> groupItems)
        {
            _groupItems = groupItems;
        }

        /// <summary>
        ///     Group items.
        /// </summary>
        internal NodeList<AliasedExpr> GroupItems
        {
            get { return _groupItems; }
        }
    }
}
