// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Linq;
    using Xunit;

    public class IsNullConditionMappingTests
    {
        [Fact]
        public void Can_create_is_null_condition_mapping_with_property()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String);
            var property = EdmProperty.CreatePrimitive("P", primitiveType);
            var mapping1 = new IsNullConditionMapping(property, true);
            var mapping2 = new IsNullConditionMapping(property, false);

            Assert.Same(property, mapping1.Property);
            Assert.Same(property, mapping2.Property);
            Assert.Null(mapping1.Column);
            Assert.Null(mapping2.Column);
            Assert.Equal(true, mapping1.IsNull);
            Assert.Equal(false, mapping2.IsNull);
            Assert.Null(mapping1.Value);
            Assert.Null(mapping2.Value);
        }

        [Fact]
        public void Can_create_is_null_condition_mapping_with_column()
        {
            var primitiveType = FakeSqlProviderServices.Instance.GetProviderManifest("2008").GetStoreTypes().First();
            var column = EdmProperty.CreatePrimitive("C", primitiveType);
            var mapping1 = new IsNullConditionMapping(column, true);
            var mapping2 = new IsNullConditionMapping(column, false);

            Assert.Same(column, mapping1.Column);
            Assert.Same(column, mapping2.Column);
            Assert.Null(mapping1.Property);
            Assert.Null(mapping2.Property);
            Assert.Equal(true, mapping1.IsNull);
            Assert.Equal(false, mapping2.IsNull);
            Assert.Null(mapping1.Value);
            Assert.Null(mapping2.Value);
        }

        [Fact]
        public void Cannot_create_is_null_condition_mapping_with_null_property_or_column()
        {
            Assert.Equal(
                "propertyOrColumn",
                Assert.Throws<ArgumentNullException>(
                    () => new IsNullConditionMapping(null, true)).ParamName);
        }
    }
}
