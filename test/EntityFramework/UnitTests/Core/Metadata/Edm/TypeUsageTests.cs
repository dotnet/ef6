// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class TypeUsageTests
    {
        [Fact]
        public void Can_update_existing_facet_with_shallow_copy()
        {
            var typeUsage1 = TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            Assert.Equal(5, typeUsage1.Facets.Count);
            Assert.True((bool)typeUsage1.Facets["Nullable"].Value);

            var typeUsage2 = typeUsage1.ShallowCopy(Facet.Create(MetadataItem.NullableFacetDescription, false));

            Assert.Equal(5, typeUsage1.Facets.Count);
            Assert.Equal(5, typeUsage2.Facets.Count);
            Assert.True((bool)typeUsage1.Facets["Nullable"].Value);
            Assert.False((bool)typeUsage2.Facets["Nullable"].Value);
        }

        [Fact]
        public void Can_add_new_facet_with_shallow_copy()
        {
            var typeUsage1 = TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            Assert.Equal(5, typeUsage1.Facets.Count);

            var typeUsage2 = typeUsage1.ShallowCopy(Facet.Create(Converter.ConcurrencyModeFacet, ConcurrencyMode.Fixed));

            Assert.Equal(5, typeUsage1.Facets.Count);
            Assert.Equal(6, typeUsage2.Facets.Count);
            Assert.Equal(ConcurrencyMode.Fixed, typeUsage2.Facets["ConcurrencyMode"].Value);
        }
    }
}
