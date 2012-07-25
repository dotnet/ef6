// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// From clause item kind.
    /// </summary>
    internal enum FromClauseItemKind
    {
        AliasedFromClause,
        JoinFromClause,
        ApplyFromClause
    }
}
