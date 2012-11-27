// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.EntitySql.AST;
    using System.Data.Entity.Utilities;

    internal sealed class FunctionAggregateInfo : GroupAggregateInfo
    {
        internal FunctionAggregateInfo(
            MethodExpr methodExpr, ErrorContext errCtx, GroupAggregateInfo containingAggregate, ScopeRegion definingScopeRegion)
            : base(GroupAggregateKind.Function, methodExpr, errCtx, containingAggregate, definingScopeRegion)
        {
            DebugCheck.NotNull(methodExpr);
        }

        internal void AttachToAstNode(string aggregateName, DbAggregate aggregateDefinition)
        {
            DebugCheck.NotNull(aggregateDefinition);
            base.AttachToAstNode(aggregateName, aggregateDefinition.ResultType);
            AggregateDefinition = aggregateDefinition;
        }

        internal DbAggregate AggregateDefinition;
    }
}
