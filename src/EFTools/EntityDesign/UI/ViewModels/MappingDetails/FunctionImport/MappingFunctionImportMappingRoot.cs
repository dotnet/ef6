// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.FunctionImports
{
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;

    internal class MappingFunctionImportMappingRoot : MappingEFElement
    {
        public MappingFunctionImportMappingRoot(EditingContext context, EFElement modelItem, MappingEFElement parent)
            : base(context, modelItem, parent)
        {
        }

        internal MappingFunctionImport MappingFunctionImport
        {
            get { return GetParentOfType(typeof(MappingFunctionImport)) as MappingFunctionImport; }
        }
    }
}
