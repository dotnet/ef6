// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.ObjectModel;

    internal abstract class FunctionImportStructuralTypeMapping
    {
        internal readonly LineInfo LineInfo;
        internal readonly Collection<FunctionImportReturnTypePropertyMapping> ColumnsRenameList;

        internal FunctionImportStructuralTypeMapping(
            Collection<FunctionImportReturnTypePropertyMapping> columnsRenameList, LineInfo lineInfo)
        {
            ColumnsRenameList = columnsRenameList;
            LineInfo = lineInfo;
        }
    }
}
