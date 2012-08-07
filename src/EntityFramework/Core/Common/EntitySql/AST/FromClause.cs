// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    ///     Represents from clause.
    /// </summary>
    internal sealed class FromClause : Node
    {
        private readonly NodeList<FromClauseItem> _fromClauseItems;

        /// <summary>
        ///     Initializes from clause.
        /// </summary>
        internal FromClause(NodeList<FromClauseItem> fromClauseItems)
        {
            _fromClauseItems = fromClauseItems;
        }

        /// <summary>
        ///     List of from clause items.
        /// </summary>
        internal NodeList<FromClauseItem> FromClauseItems
        {
            get { return _fromClauseItems; }
        }
    }
}
