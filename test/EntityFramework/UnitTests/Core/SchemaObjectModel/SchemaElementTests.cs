// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class SchemaElementTests
    {
        [Fact]
        public void CreateMetadataPropertyFromOtherNamespaceXmlArtifact_creates_annotation_using_serializer_if_available()
        {
            var property = new SomeElement().CreateMetadataPropertyFromOtherNamespaceXmlArtifact(
                XmlConstants.CustomAnnotationNamespace, "ClrType", typeof(Random).AssemblyQualifiedName);

            Assert.Equal(XmlConstants.ClrTypeAnnotation, property.Name);
            Assert.Same(typeof(Random), property.Value);
        }

        [Fact]
        public void CreateMetadataPropertyFromOtherNamespaceXmlArtifact_creates_annotation_with_string_if_no_serializer_available()
        {
            var property = new SomeElement().CreateMetadataPropertyFromOtherNamespaceXmlArtifact(
                XmlConstants.CustomAnnotationNamespace, "UseClrTypes", "true");

            Assert.Equal(XmlConstants.UseClrTypesAnnotation, property.Name);
            Assert.Equal("true", property.Value);
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
