// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Xml.Linq;
    using Xunit;

    public class SchemaElementTests
    {
        [Fact]
        public void CreateMetadataPropertyFromXmlAttribute_creates_annotation_using_serializer_if_available()
        {
            var property = new SomeElement().CreateMetadataPropertyFromXmlAttribute(
                XmlConstants.CustomAnnotationNamespace, "ClrType", typeof(Random).AssemblyQualifiedName);

            Assert.Equal(XmlConstants.ClrTypeAnnotation, property.Name);
            Assert.Same(typeof(Random), property.Value);
        }

        [Fact]
        public void CreateMetadataPropertyFromXmlAttribute_creates_annotation_with_string_if_no_serializer_available()
        {
            var property = new SomeElement().CreateMetadataPropertyFromXmlAttribute(
                XmlConstants.CustomAnnotationNamespace, "UseClrTypes", "true");

            Assert.Equal(XmlConstants.UseClrTypesAnnotation, property.Name);
            Assert.Equal("true", property.Value);
        }

        [Fact]
        public void CreateMetadataPropertyFromXmlElement_creates_annotation_containing_XElement()
        {
            var element = new XElement("SoBeautiful");
            var property = SchemaElement.CreateMetadataPropertyFromXmlElement(
                "YourFur", "IsRed", element);

            Assert.Equal("YourFur:IsRed", property.Name);
            Assert.Same(element, property.Value);
        }

        internal class SomeElement : SchemaElement
        {
            public SomeElement()
                : base(null)
            {
            }
        }
    }
}
