// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    /// <summary>
    /// Kind of VarInfo
    /// </summary>
    internal enum VarInfoKind
    {
        /// <summary>
        /// The VarInfo is of <see cref="PrimitiveTypeVarInfo" /> type.
        /// </summary>
        PrimitiveTypeVarInfo,

        /// <summary>
        /// The VarInfo is of <see cref="StructuredVarInfo" /> type.
        /// </summary>
        StructuredTypeVarInfo,

        /// <summary>
        /// The VarInfo is of <see cref="CollectionVarInfo" /> type.
        /// </summary>
        CollectionVarInfo
    }
}
