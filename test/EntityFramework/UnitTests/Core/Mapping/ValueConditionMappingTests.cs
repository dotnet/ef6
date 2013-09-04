// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Linq;
    using Xunit;

    public class ValueConditionMappingTests
    {
        [Fact]
        public void Can_create_value_condition_mapping_with_property()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String);
            var property = EdmProperty.CreatePrimitive("P", primitiveType);
            var mapping = new ValueConditionMapping(property, 42);

            Assert.Same(property, mapping.Property);
            Assert.Null(mapping.Column);
            Assert.Equal(42, mapping.Value);
            Assert.Null(mapping.IsNull);
        }

        [Fact]
        public void Can_create_value_condition_mapping_with_column()
        {
            var primitiveType = FakeSqlProviderServices.Instance.GetProviderManifest("2008").GetStoreTypes().First();
            var column = EdmProperty.CreatePrimitive("C", primitiveType);
            var mapping = new ValueConditionMapping(column, 42);

            Assert.Same(column, mapping.Column);
            Assert.Null(mapping.Property);
            Assert.Equal(42, mapping.Value);
            Assert.Null(mapping.IsNull);
        }

        [Fact]
        public void Cannot_create_value_condition_mapping_with_null_property_or_column()
        {
            Assert.Equal(
                "propertyOrColumn",
                Assert.Throws<ArgumentNullException>(
                    () => new ValueConditionMapping(null, 42)).ParamName);
        }

        [Fact]
        public void Cannot_create_value_condition_mapping_with_null_value()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String);
            var property = EdmProperty.CreatePrimitive("P", primitiveType);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentNullException>(
                    () => new ValueConditionMapping(property, null)).ParamName);
        }
    }
}
