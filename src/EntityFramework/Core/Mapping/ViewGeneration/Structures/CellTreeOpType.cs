// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures
{
    // This enum identifies for which side we are generating the view

    // Different operations that are used in the CellTreeNode nodes
    internal enum CellTreeOpType
    {
        Leaf, // Leaf Node
        Union, // union all
        FOJ, // full outerjoin
        LOJ, // left outerjoin
        IJ, // inner join
        LASJ // left antisemijoin
    }
}
