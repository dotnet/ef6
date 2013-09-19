// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class ColumnMappingBuilderTests
    {
        [Fact]
        public void Can_initialize_column_mapping_builder()
        {
            var columnProperty = new EdmProperty("C");
            var property = new EdmProperty("P");

            var columnMappingBuilder = new ColumnMappingBuilder(columnProperty, new[] { property });

            Assert.Same(columnProperty, columnMappingBuilder.ColumnProperty);
            Assert.Same(property, columnMappingBuilder.PropertyPath.Single());
        }

        [Fact]
        public void Cannot_initialize_column_mapping_builder_to_null_values()
        {
            var property = new EdmProperty("X");

            Assert.Equal(
                "columnProperty",
                Assert.Throws<ArgumentNullException>(
                    () => new ColumnMappingBuilder(null, new[] { property })).ParamName);

            Assert.Equal(
                "propertyPath",
                Assert.Throws<ArgumentNullException>(
                    () => new ColumnMappingBuilder(property, null)).ParamName);
        }

        [Fact]
        public void Setting_column_should_update_property_mapping()
        {
            var columnProperty1 = new EdmProperty("C1");
            var property = new EdmProperty("P");
            var columnMappingBuilder = new ColumnMappingBuilder(columnProperty1, new[] { property });
            var scalarPropertyMapping = new ScalarPropertyMapping(property, columnProperty1);

            columnMappingBuilder.SetTarget(scalarPropertyMapping);

            var columnProperty2 = new EdmProperty("C2");

            columnMappingBuilder.ColumnProperty = columnProperty2;

            Assert.Same(columnProperty2, columnMappingBuilder.ColumnProperty);
            Assert.Same(columnProperty2, scalarPropertyMapping.ColumnProperty);
        }
    }
}
