// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    ///     Represents a order by clause item.
    /// </summary>
    internal sealed class OrderByClauseItem : Node
    {
        private readonly Node _orderExpr;
        private readonly OrderKind _orderKind;
        private readonly Identifier _optCollationIdentifier;

        /// <summary>
        ///     Initializes non-collated order by clause item.
        /// </summary>
        internal OrderByClauseItem(Node orderExpr, OrderKind orderKind)
            : this(orderExpr, orderKind, null)
        {
        }

        /// <summary>
        ///     Initializes collated order by clause item.
        /// </summary>
        /// <param name="optCollationIdentifier"> optional Collation identifier </param>
        internal OrderByClauseItem(Node orderExpr, OrderKind orderKind, Identifier optCollationIdentifier)
        {
            _orderExpr = orderExpr;
            _orderKind = orderKind;
            _optCollationIdentifier = optCollationIdentifier;
        }

        /// <summary>
        ///     Oeturns order expression.
        /// </summary>
        internal Node OrderExpr
        {
            get { return _orderExpr; }
        }

        /// <summary>
        ///     Returns order kind (none,asc,desc).
        /// </summary>
        internal OrderKind OrderKind
        {
            get { return _orderKind; }
        }

        /// <summary>
        ///     Returns collattion identifier if one exists.
        /// </summary>
        internal Identifier Collation
        {
            get { return _optCollationIdentifier; }
        }
    }
}
