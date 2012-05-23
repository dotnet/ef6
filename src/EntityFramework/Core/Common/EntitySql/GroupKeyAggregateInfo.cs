namespace System.Data.Entity.Core.Common.EntitySql
{
    internal sealed class GroupKeyAggregateInfo : GroupAggregateInfo
    {
        internal GroupKeyAggregateInfo(
            GroupAggregateKind aggregateKind, ErrorContext errCtx, GroupAggregateInfo containingAggregate, ScopeRegion definingScopeRegion)
            : base(
                aggregateKind, null /* there is no AST.GroupAggregateExpression corresponding to the group key */, errCtx,
                containingAggregate, definingScopeRegion)
        {
        }
    }
}
