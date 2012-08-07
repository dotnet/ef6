// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    ///     Represents distinct kind (none=all,all,distinct).
    /// </summary>
    internal enum DistinctKind
    {
        None,
        All,
        Distinct
    }
}
