// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Tables
{
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;

    internal class MappingEntityMappingRoot : MappingEFElement
    {
        public MappingEntityMappingRoot(EditingContext context, EFElement modelItem, MappingEFElement parent)
            : base(context, modelItem, parent)
        {
        }

        internal MappingConceptualEntityType MappingConceptualEntityType
        {
            get { return GetParentOfType(typeof(MappingConceptualEntityType)) as MappingConceptualEntityType; }
        }

        internal MappingStorageEntityType MappingStorageEntityType
        {
            get { return GetParentOfType(typeof(MappingStorageEntityType)) as MappingStorageEntityType; }
        }
    }
}
