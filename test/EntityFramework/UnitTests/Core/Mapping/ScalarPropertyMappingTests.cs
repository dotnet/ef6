// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using Xunit;

    public class ScalarPropertyMappingTests
    {
        [Fact]
        public void Can_get_and_set_column_property()
        {
            var column = new EdmProperty("C");
            var scalarPropertyMapping = new ScalarPropertyMapping(new EdmProperty("P"), column);

            Assert.Same(column, scalarPropertyMapping.Column);
        
            column = new EdmProperty("C'");

            scalarPropertyMapping.Column = column;

            Assert.Same(column, scalarPropertyMapping.Column);
        }

        [Fact]
        public void Cannot_create_mapping_with_null_property()
        {
            Assert.Equal(
                "property",
                Assert.Throws<ArgumentNullException>(
                    () => new ScalarPropertyMapping(
                              null,
                              EdmProperty.CreatePrimitive("p", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)))).ParamName);
        }

        [Fact]
        public void Cannot_create_mapping_with_null_column()
        {
            Assert.Equal(
                "column",
                Assert.Throws<ArgumentNullException>(
                    () => new ScalarPropertyMapping(
                              EdmProperty.CreatePrimitive("p", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                              null)).ParamName);
        }

        [Fact]
        public void Cannot_create_mapping_for_non_primitive_or_non_enum_property()
        {
            var modelProperty = EdmProperty.CreateComplex("p", new ComplexType());
            var storeColumn = EdmProperty.CreatePrimitive("p", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));

            Assert.Equal(
                Strings.StorageScalarPropertyMapping_OnlyScalarPropertiesAllowed,
                Assert.Throws<ArgumentException>(
                    () => new ScalarPropertyMapping(modelProperty, storeColumn)).Message);
        }

        [Fact]
        public void Cannot_create_mapping_for_non_primitive_store_column()
        {
            var modelProperty = EdmProperty.CreatePrimitive("p", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            var storeColumn = new EdmProperty("p", TypeUsage.CreateDefaultTypeUsage(new RowType()));

            Assert.Equal(
                Strings.StorageScalarPropertyMapping_OnlyScalarPropertiesAllowed,
                Assert.Throws<ArgumentException>(
                    () => new ScalarPropertyMapping(modelProperty, storeColumn)).Message);
        }
    }
}