// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.Mapping
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public sealed class StorageEntityTypeMappingExtensionsTests
    {
        [Fact]
        public void GetPropertyMapping_should_return_mapping_with_path()
        {
            var entityTypeMapping = new StorageEntityTypeMapping(null);
            var propertyFoo = EdmProperty.CreateComplex("Foo", new ComplexType());
            var propertyBar = EdmProperty.CreatePrimitive("Bar", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            var entityPropertyMapping
                = new ColumnMappingBuilder(
                    EdmProperty.CreatePrimitive("C", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                    new[]
                        {
                            propertyFoo,
                            propertyBar,
                        });

            var entityTypeMappingFragment
                = new StorageMappingFragment(new EntitySet(), entityTypeMapping, false);

            entityTypeMappingFragment.AddColumnMapping(entityPropertyMapping);
            entityTypeMapping.AddFragment(entityTypeMappingFragment);

            Assert.Same(entityPropertyMapping, entityTypeMapping.GetPropertyMapping(propertyFoo, propertyBar));
        }
    }
}
