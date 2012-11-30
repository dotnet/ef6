// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.EntitySql.AST;
    using System.Data.Entity.Utilities;

    internal sealed class GroupPartitionInfo : GroupAggregateInfo
    {
        internal GroupPartitionInfo(
            GroupPartitionExpr groupPartitionExpr, ErrorContext errCtx, GroupAggregateInfo containingAggregate,
            ScopeRegion definingScopeRegion)
            : base(GroupAggregateKind.Partition, groupPartitionExpr, errCtx, containingAggregate, definingScopeRegion)
        {
            DebugCheck.NotNull(groupPartitionExpr);
        }

        internal void AttachToAstNode(string aggregateName, DbExpression aggregateDefinition)
        {
            DebugCheck.NotNull(aggregateDefinition);
            base.AttachToAstNode(aggregateName, aggregateDefinition.ResultType);
            AggregateDefinition = aggregateDefinition;
        }

        internal DbExpression AggregateDefinition;
    }
}
