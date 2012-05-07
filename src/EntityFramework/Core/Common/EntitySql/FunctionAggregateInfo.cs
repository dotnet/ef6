namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.EntitySql.AST;
    using System.Diagnostics;

    internal sealed class FunctionAggregateInfo : GroupAggregateInfo
    {
        internal FunctionAggregateInfo(
            MethodExpr methodExpr, ErrorContext errCtx, GroupAggregateInfo containingAggregate, ScopeRegion definingScopeRegion)
            : base(GroupAggregateKind.Function, methodExpr, errCtx, containingAggregate, definingScopeRegion)
        {
            Debug.Assert(methodExpr != null, "methodExpr != null");
        }

        internal void AttachToAstNode(string aggregateName, DbAggregate aggregateDefinition)
        {
            Debug.Assert(aggregateDefinition != null, "aggregateDefinition != null");
            base.AttachToAstNode(aggregateName, aggregateDefinition.ResultType);
            AggregateDefinition = aggregateDefinition;
        }

        internal DbAggregate AggregateDefinition;
    }
}