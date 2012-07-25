// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.EntityModel.SchemaObjectModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Xml;

    internal sealed class SridFacetDescriptionElement : FacetDescriptionElement
    {
        public SridFacetDescriptionElement(TypeElement type, string name)
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
        /// <param name="reader">xml reader currently positioned at Default attribute</param>
        protected override void HandleDefaultAttribute(XmlReader reader)
        {
            var value = reader.Value;
            if (value.Trim()
                == XmlConstants.Variable)
            {
                DefaultValue = EdmConstants.VariableValue;
                return;
            }

            var intValue = -1;
            if (HandleIntAttribute(reader, ref intValue))
            {
                DefaultValue = intValue;
            }
        }
    }
}
