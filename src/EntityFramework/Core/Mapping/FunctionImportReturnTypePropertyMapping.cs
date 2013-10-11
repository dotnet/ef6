// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    /// <summary>
    /// Base class for mapping a property of a function import return type.
    /// </summary>
    public abstract class FunctionImportReturnTypePropertyMapping : MappingItem
    {
        internal readonly LineInfo LineInfo;

        internal FunctionImportReturnTypePropertyMapping(LineInfo lineInfo)
        {
            LineInfo = lineInfo;
        }

        internal abstract string CMember { get; }
        internal abstract string SColumn { get; }
    }
}
