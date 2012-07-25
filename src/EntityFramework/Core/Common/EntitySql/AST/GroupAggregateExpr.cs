// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Base class for <see cref="MethodExpr"/> and <see cref="GroupPartitionExpr"/>.
    /// </summary>
    internal abstract class GroupAggregateExpr : Node
    {
        internal GroupAggregateExpr(DistinctKind distinctKind)
        {
            DistinctKind = distinctKind;
        }

        /// <summary>
        /// True if it is a "distinct" aggregate.
        /// </summary>
        internal readonly DistinctKind DistinctKind;

        internal GroupAggregateInfo AggregateInfo;
    }
}
