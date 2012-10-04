// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.Mapping.UnitTests
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Db.Mapping;
    using Xunit;

    public sealed class DbEntityTypeMappingExtensions
    {
        [Fact]
        public void GetPropertyMapping_should_return_mapping_with_path()
        {
            var entityTypeMapping = new DbEntityTypeMapping();
            var propertyFoo = EdmProperty.Primitive("Foo", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var propertyBar = EdmProperty.Primitive("Bar", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var entityPropertyMapping = new DbEdmPropertyMapping
                                            {
                                                PropertyPath = new[]
                                                                   {
                                                                       propertyFoo,
                                                                       propertyBar,
                                                                   }
                                            };

            var entityTypeMappingFragment = new DbEntityTypeMappingFragment();
            entityTypeMappingFragment.PropertyMappings.Add(entityPropertyMapping);
            entityTypeMapping.TypeMappingFragments.Add(entityTypeMappingFragment);

            Assert.Same(entityPropertyMapping, entityTypeMapping.GetPropertyMapping(propertyFoo, propertyBar));
        }
    }
}
