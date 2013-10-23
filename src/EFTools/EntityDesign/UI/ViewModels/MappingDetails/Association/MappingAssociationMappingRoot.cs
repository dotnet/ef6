// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Associations
{
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;

    internal class MappingAssociationMappingRoot : MappingEFElement
    {
        public MappingAssociationMappingRoot(EditingContext context, EFElement modelItem, MappingEFElement parent)
            : base(context, modelItem, parent)
        {
        }

        internal MappingAssociation MappingAssociation
        {
            get { return GetParentOfType(typeof(MappingAssociation)) as MappingAssociation; }
        }

        internal MappingAssociationSet MappingAssociationSet
        {
            get { return GetParentOfType(typeof(MappingAssociationSet)) as MappingAssociationSet; }
        }

        internal MappingAssociationSetEnd MappingAssociationSetEnd
        {
            get { return GetParentOfType(typeof(MappingAssociationSetEnd)) as MappingAssociationSetEnd; }
        }
    }
}
