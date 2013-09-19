// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting
{
    internal enum TileOpKind
    {
        Union,
        Join,
        AntiSemiJoin,
        // Project,
        Named
    }
}
