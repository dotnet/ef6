// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Functions
{
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;

    internal class MappingFunctionMappingRoot : MappingEFElement
    {
        public MappingFunctionMappingRoot(EditingContext context, EFElement modelItem, MappingEFElement parent)
            : base(context, modelItem, parent)
        {
        }

        internal MappingFunctionEntityType MappingFunctionEntityType
        {
            get { return GetParentOfType(typeof(MappingFunctionEntityType)) as MappingFunctionEntityType; }
        }

        internal MappingModificationFunctionMapping MappingModificationFunctionMapping
        {
            get { return GetParentOfType(typeof(MappingModificationFunctionMapping)) as MappingModificationFunctionMapping; }
        }
    }
}
