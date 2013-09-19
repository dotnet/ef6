// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Xml;

    internal sealed class IntegerFacetDescriptionElement : FacetDescriptionElement
    {
        public IntegerFacetDescriptionElement(TypeElement type, string name)
            : base(type, name)
        {
        }

        public override EdmType FacetType
        {
            get { return MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Int32); }
        }

        /////////////////////////////////////////////////////////////////////
        // Attribute Handlers

        /// <summary>
        /// Handler for the Default attribute
        /// </summary>
        /// <param name="reader"> xml reader currently positioned at Default attribute </param>
        protected override void HandleDefaultAttribute(XmlReader reader)
        {
            var value = -1;
            if (HandleIntAttribute(reader, ref value))
            {
                DefaultValue = value;
            }
        }
    }
}
