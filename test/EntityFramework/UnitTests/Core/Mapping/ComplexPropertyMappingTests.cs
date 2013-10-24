// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using Xunit;

    public class ComplexPropertyMappingTests
    {
        [Fact]
        public void Can_create_mapping_and_get_property()
        {
            var complexType = ComplexType.Create("CT", "NS", DataSpace.CSpace, new EdmMember[0], null);
            var property = EdmProperty.CreateComplex("P", complexType);
            var mapping = new ComplexPropertyMapping(property);

            Assert.Same(property, mapping.Property);
        }

        [Fact]
        public void Can_add_get_remove_type_mappings()
        {
            var complexType = ComplexType.Create("CT", "NS", DataSpace.CSpace, new EdmMember[0], null);
            var property = EdmProperty.CreateComplex("P", complexType);
            var mapping = new ComplexPropertyMapping(property);

            Assert.Equal(0, mapping.TypeMappings.Count);

            var typeMapping = new ComplexTypeMapping(isPartial: false);
            mapping.AddTypeMapping(typeMapping);

            Assert.Equal(1, mapping.TypeMappings.Count);
            Assert.Same(typeMapping, mapping.TypeMappings[0]);

            mapping.RemoveTypeMapping(typeMapping);

            Assert.Equal(0, mapping.TypeMappings.Count);
        }

        [Fact]
        public void Cannot_create_mapping_with_null_property()
        {
            Assert.Equal(
                "property",
                Assert.Throws<ArgumentNullException>(
                    () => new ComplexPropertyMapping(null)).ParamName);
        }

        [Fact]
        public void Cannot_create_mapping_for_non_complex_property()
        {
            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var exception = 
                new ArgumentException(
                    Strings.StorageComplexPropertyMapping_OnlyComplexPropertyAllowed, 
                    "property");

            Assert.Equal(
                exception.Message,
                Assert.Throws<ArgumentException>(
                    () => new ComplexPropertyMapping(property)).Message);
        }

        [Fact]
        public void Cannot_add_type_mapping_when_read_only()
        {
            var complexType = ComplexType.Create("CT", "NS", DataSpace.CSpace, new EdmMember[0], null);
            var property = EdmProperty.CreateComplex("P", complexType);
            var mapping = new ComplexPropertyMapping(property);

            mapping.SetReadOnly();

            Assert.True(mapping.IsReadOnly);

            var typeMapping = new ComplexTypeMapping(isPartial: false);

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => mapping.AddTypeMapping(typeMapping)).Message);
        }

        [Fact]
        public void Cannot_remove_type_mapping_when_read_only()
        {
            var complexType = ComplexType.Create("CT", "NS", DataSpace.CSpace, new EdmMember[0], null);
            var property = EdmProperty.CreateComplex("P", complexType);
            var mapping = new ComplexPropertyMapping(property);

            var typeMapping = new ComplexTypeMapping(isPartial: false);
            mapping.AddTypeMapping(typeMapping);
            mapping.SetReadOnly();

            Assert.True(mapping.IsReadOnly);

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => mapping.RemoveTypeMapping(typeMapping)).Message);
        }

        [Fact]
        public void SetReadOnly_is_called_on_child_mapping_items()
        {
            var complexType = ComplexType.Create("CT", "NS", DataSpace.CSpace, new EdmMember[0], null);
            var property = EdmProperty.CreateComplex("P", complexType);
            var mapping = new ComplexPropertyMapping(property);

            var typeMapping = new ComplexTypeMapping(isPartial: false);
            mapping.AddTypeMapping(typeMapping);

            Assert.False(typeMapping.IsReadOnly);
            mapping.SetReadOnly();
            Assert.True(typeMapping.IsReadOnly);
        }
    }
}
