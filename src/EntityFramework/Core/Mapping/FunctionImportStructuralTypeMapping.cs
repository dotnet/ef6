// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Specifies a function import structural type mapping.
    /// </summary>
    public abstract class FunctionImportStructuralTypeMapping : MappingItem
    {
        internal readonly LineInfo LineInfo;
        internal readonly Collection<FunctionImportReturnTypePropertyMapping> ColumnsRenameList;

        internal FunctionImportStructuralTypeMapping(
            Collection<FunctionImportReturnTypePropertyMapping> columnsRenameList, LineInfo lineInfo)
        {
            ColumnsRenameList = columnsRenameList;
            LineInfo = lineInfo;
        }

        /// <summary>
        /// Gets the property mappings for the result type of a function import.
        /// </summary>
        public ReadOnlyCollection<FunctionImportReturnTypePropertyMapping> Properties
        {
            get { return new ReadOnlyCollection<FunctionImportReturnTypePropertyMapping>(ColumnsRenameList); }
        }

        internal override void SetReadOnly()
        {
            SetReadOnly(ColumnsRenameList);

            base.SetReadOnly();
        }
    }
}
