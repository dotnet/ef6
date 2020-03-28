// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Linq;
    using Xunit;

    public class EntitySetTests
    {
        [Fact]
        public void Create_factory_method_sets_properties_and_seals_the_type()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            var entitySet =
                EntitySet.Create(
                    "EntitySet",
                    "dbo",
                    "tblEntities",
                    "definingQuery",
                    entityType,
                    new[]
                        {
                            new MetadataProperty(
                                "TestProperty",
                                TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                                "value")
                        });

            Assert.Equal("EntitySet", entitySet.Name);
            Assert.Equal("dbo", entitySet.Schema);
            Assert.Equal("tblEntities", entitySet.Table);
            Assert.Equal("definingQuery", entitySet.DefiningQuery);
            Assert.Same(entityType, entitySet.ElementType);

            var metadataProperty = entitySet.MetadataProperties.SingleOrDefault(p => p.Name == "TestProperty");
            Assert.NotNull(metadataProperty);
            Assert.Equal("value", metadataProperty.Value);

            Assert.True(entitySet.IsReadOnly);
        }
    }
}
