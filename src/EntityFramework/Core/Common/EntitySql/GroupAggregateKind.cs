namespace System.Data.Entity.Core.Common.EntitySql
{
    internal enum GroupAggregateKind
    {
        None,

        /// <summary>
        /// Inside of an aggregate function (Max, Min, etc).
        /// All range variables originating on the defining scope of this aggregate should yield <see cref="IGroupExpressionExtendedInfo.GroupVarBasedExpression"/>.
        /// </summary>
        Function,

        /// <summary>
        /// Inside of GROUPPARTITION expression.
        /// All range variables originating on the defining scope of this aggregate should yield <see cref="IGroupExpressionExtendedInfo.GroupAggBasedExpression"/>.
        /// </summary>
        Partition,

        /// <summary>
        /// Inside of a group key definition
        /// All range variables originating on the defining scope of this aggregate should yield <see cref="ScopeEntry.GetExpression"/>.
        /// </summary>
        GroupKey
    }
}
