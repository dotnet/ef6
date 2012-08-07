// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration
{
    internal enum PerfType
    {
        InitialSetup = 0,
        CellCreation,
        KeyConstraint,
        ViewgenContext,
        UpdateViews,
        DisjointConstraint,
        PartitionConstraint,
        DomainConstraint,
        ForeignConstraint,
        QueryViews,
        BoolResolution,
        Unsatisfiability,
        ViewParsing,
    }
}
