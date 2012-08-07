// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
