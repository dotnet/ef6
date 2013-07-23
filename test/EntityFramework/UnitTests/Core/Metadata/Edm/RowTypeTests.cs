// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Linq;
    using Xunit;

    public class RowTypeTests
    {
        [Fact]
        public void Create_factory_method_sets_properties_and_seals_the_type()
        {
            var rowType = RowType.Create(
                new[]
                    {
                        EdmProperty.CreatePrimitive("foo", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                        EdmProperty.CreatePrimitive("bar", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int64))
                    },
                new[]
                    {
                        new MetadataProperty(
                            "TestProperty", TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                            "baz")
                    }
                );

            Assert.NotNull(rowType);
            Assert.Equal(new[] {"foo", "bar"}, rowType.Properties.Select(p => p.Name));

            var metadataProperty = rowType.MetadataProperties.SingleOrDefault(p => p.Name == "TestProperty");
            Assert.NotNull(metadataProperty);
            Assert.Equal("baz", metadataProperty.Value);

            Assert.True(rowType.IsReadOnly);
        }

        [Fact]
        public void Can_get_list_of_declared_properties()
        {
            var rowType = new RowType();

            Assert.Empty(rowType.DeclaredProperties);

            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            rowType.AddMember(property);

            Assert.Equal(1, rowType.DeclaredProperties.Count);

            rowType.RemoveMember(property);
        }
    }
}
