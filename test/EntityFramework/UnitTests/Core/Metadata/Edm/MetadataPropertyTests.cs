// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class MetadataPropertyTests
    {
        [Fact]
        public void Can_get_and_set_value_property()
        {
            var metadataProperty
                = new MetadataProperty
                      {
                          Value = "Foo"
                      };

            Assert.Equal("Foo", metadataProperty.Value);
        }

        [Fact]
        public void Create_sets_properties_and_seals_MetadataProperty()
        {
            var typeUsage = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var metadataProperty = MetadataProperty.Create("property", typeUsage, "value");

            Assert.Equal("property", metadataProperty.Name);
            Assert.Same(typeUsage, metadataProperty.TypeUsage);
            Assert.Equal("value", metadataProperty.Value);
            Assert.True(metadataProperty.IsReadOnly);
        }
    }
}
