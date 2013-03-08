// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Services
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Moq;
    using Xunit;

    public class StructuralTypeMappingGeneratorTests
    {
        [Fact] // CodePlex 881
        public void Generate_retains_store_facets_returned_by_the_provider_manifest()
        {
            var storeType = new PrimitiveType
                {
                    Name = "number",
                    DataSpace = DataSpace.SSpace
                };

            var typeUsage =
                TypeUsage.Create(
                    storeType,
                    new[]
                        {
                            CreateConstFacet("Precision", PrimitiveTypeKind.Byte, (byte)11),
                            CreateConstFacet("Scale", PrimitiveTypeKind.Byte, (byte)0)
                        });

            var mockManifest = new Mock<DbProviderManifest>();
            mockManifest.Setup(m => m.GetStoreType(It.IsAny<TypeUsage>())).Returns(typeUsage);

            var storeProperty = new TestMappingGenerator(mockManifest.Object)
                .MapTableColumn(EdmProperty.Primitive("P1", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)));

            Assert.Equal("number", storeProperty.TypeUsage.EdmType.Name);

            var facets = storeProperty.TypeUsage.Facets;
            Assert.Equal((byte)11, facets.Where(f => f.Name == "Precision").Select(f => f.Value).Single());
            Assert.Equal((byte)0, facets.Where(f => f.Name == "Scale").Select(f => f.Value).Single());
        }

        internal class TestMappingGenerator : StructuralTypeMappingGenerator
        {
            public TestMappingGenerator(DbProviderManifest providerManifest)
                : base(providerManifest)
            {
            }

            public EdmProperty MapTableColumn(EdmProperty property)
            {
                return MapTableColumn(property, "Test", isInstancePropertyOnDerivedType: false);
            }
        }

        private static Facet CreateConstFacet(string facetName, PrimitiveTypeKind facetTypeKind, object value)
        {
            return Facet.Create(
                new FacetDescription(
                    facetName, PrimitiveType.GetEdmPrimitiveType(facetTypeKind),
                    null, null, value, true, null), value);
        }
    }
}
