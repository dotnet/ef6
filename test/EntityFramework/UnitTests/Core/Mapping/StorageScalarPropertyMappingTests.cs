// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using Xunit;

    public class StorageScalarPropertyMappingTests
    {
        [Fact]
        public void Can_get_and_set_column_property()
        {
            var column = new EdmProperty("C");
            var scalarPropertyMapping = new StorageScalarPropertyMapping(new EdmProperty("P"), column);

            Assert.Same(column, scalarPropertyMapping.ColumnProperty);
        
            column = new EdmProperty("C'");

            scalarPropertyMapping.ColumnProperty = column;

            Assert.Same(column, scalarPropertyMapping.ColumnProperty);
        }

        [Fact]
        public void Cannot_set_null_property()
        {
            Assert.Equal(
                "member",
                Assert.Throws<ArgumentNullException>(
                    () => new StorageScalarPropertyMapping(
                              null,
                              EdmProperty.CreatePrimitive("p", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)))).ParamName);
        }

        [Fact]
        public void Cannot_create_mapping_for_null_store_property()
        {
            Assert.Equal(
                "columnMember",
                Assert.Throws<ArgumentNullException>(
                    () => new StorageScalarPropertyMapping(
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
                    () => new StorageScalarPropertyMapping(modelProperty, storeColumn)).Message);
        }

        [Fact]
        public void Cannot_create_mapping_for_non_primitive_store_column()
        {
            var modelProperty = EdmProperty.CreatePrimitive("p", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            var storeColumn = new EdmProperty("p", TypeUsage.CreateDefaultTypeUsage(new RowType()));

            Assert.Equal(
                Strings.StorageScalarPropertyMapping_OnlyScalarPropertiesAllowed,
                Assert.Throws<ArgumentException>(
                    () => new StorageScalarPropertyMapping(modelProperty, storeColumn)).Message);
        }
    }
}