namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    using System;
    using System.Globalization;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

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