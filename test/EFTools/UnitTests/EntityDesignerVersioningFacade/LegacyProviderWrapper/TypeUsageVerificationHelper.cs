// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using LegacyMetadata = System.Data.Metadata.Edm;
using LegacySpatial = System.Data.Spatial;

namespace Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Spatial;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    internal class TypeUsageVerificationHelper
    {
        public static void VerifyEdmTypesEquivalent(LegacyMetadata.EdmType legacyEdmType, EdmType edmType)
        {
            Assert.Equal(legacyEdmType.FullName, edmType.FullName);

            Assert.True(
                (legacyEdmType.BaseType == null && edmType.BaseType == null) ||
                legacyEdmType.BaseType.FullName == edmType.BaseType.FullName);
            Assert.Equal(legacyEdmType.BuiltInTypeKind.ToString(), edmType.BuiltInTypeKind.ToString());
            Assert.Equal(
                ((LegacyMetadata.DataSpace)typeof(LegacyMetadata.EdmType)
                                               .GetProperty("DataSpace", BindingFlags.Instance | BindingFlags.NonPublic)
                                               .GetValue(legacyEdmType)).ToString(),
                ((DataSpace)typeof(EdmType)
                                .GetProperty("DataSpace", BindingFlags.Instance | BindingFlags.NonPublic)
                                .GetValue(edmType)).ToString());

            if (edmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveTypeKind)
            {
                var primitiveEdmType = (PrimitiveType)edmType;
                var legacyPrimitiveEdmType = (LegacyMetadata.PrimitiveType)legacyEdmType;

                // EF5 geospatial types should be converted to EF6 spatial types
                var expectedClrEquivalentType =
                    legacyPrimitiveEdmType.ClrEquivalentType == typeof(LegacySpatial.DbGeography)
                        ? typeof(DbGeography)
                        : legacyPrimitiveEdmType.ClrEquivalentType == typeof(LegacySpatial.DbGeometry)
                              ? typeof(DbGeometry)
                              : legacyPrimitiveEdmType.ClrEquivalentType;

                Assert.Equal(expectedClrEquivalentType, primitiveEdmType.ClrEquivalentType);
                Assert.Equal(legacyPrimitiveEdmType.GetEdmPrimitiveType().FullName, primitiveEdmType.GetEdmPrimitiveType().FullName);
            }
        }

        public static void VerifyTypeUsagesEquivalent(LegacyMetadata.TypeUsage legacyTypeUsage, TypeUsage typeUsage)
        {
            if (typeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType)
            {
                VerifyTypeUsagesEquivalent(
                    ((LegacyMetadata.CollectionType)legacyTypeUsage.EdmType).TypeUsage,
                    ((CollectionType)typeUsage.EdmType).TypeUsage);
            }
            else
            {
                VerifyEdmTypesEquivalent(legacyTypeUsage.EdmType, typeUsage.EdmType);
            }

            var legacyTypeFacets = legacyTypeUsage.Facets.OrderBy(f => f.Name).ToArray();
            var typeFacets = typeUsage.Facets.OrderBy(f => f.Name).ToArray();

            Assert.Equal(legacyTypeFacets.Length, typeFacets.Length);
            for (var i = 0; i < legacyTypeFacets.Length; i++)
            {
                VerifyFacetsEquivalent(legacyTypeFacets[i], typeFacets[i]);
            }
        }

        public static void VerifyFacetsEquivalent(LegacyMetadata.Facet legacyFacet, Facet facet)
        {
            Assert.Equal(legacyFacet.Name, facet.Name);
            Assert.Equal(legacyFacet.FacetType.FullName, facet.FacetType.FullName);

            // Specialcase Variable, Max and Identity facet values - they are internal singleton objects.
            if (legacyFacet.Value != null
                && (new[] { "Max", "Variable", "Identity" }.Contains(legacyFacet.Value.ToString())
                    || facet.Name == "ConcurrencyMode"))
            {
                // this is to make sure we did not stick EF6 Max/Variable/Identity on legacy facet as the value
                Assert.Equal(typeof(LegacyMetadata.EdmType).Assembly, legacyFacet.Value.GetType().Assembly);

                Assert.NotNull(facet.Value);
                Assert.Equal(legacyFacet.Value.ToString(), facet.Value.ToString());
            }
            else
            {
                Assert.Equal(legacyFacet.Value, facet.Value);
            }

            Assert.Equal(legacyFacet.IsUnbounded, facet.IsUnbounded);
            Assert.Equal(LegacyMetadata.BuiltInTypeKind.Facet, legacyFacet.BuiltInTypeKind);
            Assert.Equal(BuiltInTypeKind.Facet, facet.BuiltInTypeKind);
        }

        public static void VerifyFacetDescriptionsEquivalent(
            FacetDescription facetDescription, LegacyMetadata.FacetDescription legacyFacetDescription)
        {
            Assert.Equal(facetDescription.FacetName, legacyFacetDescription.FacetName);
            VerifyEdmTypesEquivalent(legacyFacetDescription.FacetType, facetDescription.FacetType);
            Assert.True(
                // .ToString makes it easier to compare default values like "Variable"
                facetDescription.DefaultValue.ToString() == legacyFacetDescription.DefaultValue.ToString() ||
                facetDescription.DefaultValue == legacyFacetDescription.DefaultValue);
            Assert.Equal(facetDescription.IsConstant, legacyFacetDescription.IsConstant);
            Assert.Equal(facetDescription.IsRequired, legacyFacetDescription.IsRequired);
            Assert.Equal(facetDescription.MaxValue, legacyFacetDescription.MaxValue);
            Assert.Equal(facetDescription.MinValue, legacyFacetDescription.MinValue);
        }
    }
}
