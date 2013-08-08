// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Represents GROUPPARTITION(expr) expression.
    /// </summary>
    internal sealed class GroupPartitionExpr : GroupAggregateExpr
    {
        private readonly Node _argExpr;

        /// <summary>
        /// Initializes GROUPPARTITION expression node.
        /// </summary>
        internal GroupPartitionExpr(DistinctKind distinctKind, Node refArgExpr)
            : base(distinctKind)
        {
            _argExpr = refArgExpr;
        }

        /// <summary>
        /// Return GROUPPARTITION argument expression.
        /// </summary>
        internal Node ArgExpr
        {
            get { return _argExpr; }
        }
    }
}
