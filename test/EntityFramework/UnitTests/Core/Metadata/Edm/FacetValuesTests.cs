// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm.Provider;
    using Xunit;

    public class FacetValuesTests
    {
        [Fact]
        public void Create_copies_value_from_NullableFacet()
        {
            Test_create_copies_value(DbProviderManifest.NullableFacetName, value: true);
        }

        [Fact]
        public void Create_copies_value_from_MaxLengthFacet()
        {
            Test_create_copies_value(DbProviderManifest.MaxLengthFacetName, value: 1);
        }

        [Fact]
        public void Create_copies_value_from_UnicodeFacet()
        {
            Test_create_copies_value(DbProviderManifest.UnicodeFacetName, value: true);
        }

        [Fact]
        public void Create_copies_value_from_FixedLengthFacet()
        {
            Test_create_copies_value(DbProviderManifest.FixedLengthFacetName, value: true);
        }

        [Fact]
        public void Create_copies_value_from_PrecisionFacet()
        {
            Test_create_copies_value(DbProviderManifest.PrecisionFacetName, value: (byte)1);
        }

        [Fact]
        public void Create_copies_value_from_ScaleFacet()
        {
            Test_create_copies_value(DbProviderManifest.ScaleFacetName, value: (byte)1);
        }

        [Fact]
        public void Create_copies_value_from_DefaultValueFacet()
        {
            Test_create_copies_value(DbProviderManifest.DefaultValueFacetName, value: 0.1);
        }

        [Fact]
        public void Create_copies_value_from_CollationFacet()
        {
            Test_create_copies_value(DbProviderManifest.CollationFacetName, value: "foo");
        }

        [Fact]
        public void Create_copies_value_from_SridFacet()
        {
            Test_create_copies_value(DbProviderManifest.SridFacetName, value: 1);
        }

        [Fact]
        public void Create_copies_value_from_IsStrictFacet()
        {
            Test_create_copies_value(DbProviderManifest.IsStrictFacetName, value: true);
        }

        [Fact]
        public void Create_copies_value_from_StoreGeneratedPatternFacet()
        {
            Test_create_copies_value(EdmProviderManifest.StoreGeneratedPatternFacetName, value: StoreGeneratedPattern.Computed);
        }

        [Fact]
        public void Create_copies_value_from_ConcurrencyModeFacet()
        {
            Test_create_copies_value(EdmProviderManifest.ConcurrencyModeFacetName, value: ConcurrencyMode.Fixed);
        }

        [Fact]
        public void Create_copies_value_from_CollectionKindFacet()
        {
            Test_create_copies_value(EdmConstants.CollectionKind, value: CollectionKind.List);
        }

        private static void Test_create_copies_value<T>(string facetName, T value)
        {
            var description = GetFacetDescription<T>(facetName);
            var facet = Facet.Create(description, value: value);

            var values = FacetValues.Create(new List<Facet> { facet });

            Facet returnedFacet = null;
            Assert.True(values.TryGetFacet(description, out returnedFacet));
            Assert.Equal(facet.Value, returnedFacet.Value);
        }

        [Fact]
        public void NullFacetValues_does_not_have_NullableFacet()
        {
            Test_NullFacetValues_has_value_set_to_null<bool>(DbProviderManifest.NullableFacetName, shouldBePresent: false);
        }

        [Fact]
        public void NullFacetValues_has_MaxLengthFacet_set_to_null()
        {
            Test_NullFacetValues_has_value_set_to_null<int>(DbProviderManifest.MaxLengthFacetName);
        }

        [Fact]
        public void NullFacetValues_has_UnicodeFacet_set_to_null()
        {
            Test_NullFacetValues_has_value_set_to_null<bool>(DbProviderManifest.UnicodeFacetName);
        }

        [Fact]
        public void NullFacetValues_has_FixedLengthFacet_set_to_null()
        {
            Test_NullFacetValues_has_value_set_to_null<bool>(DbProviderManifest.FixedLengthFacetName);
        }

        [Fact]
        public void NullFacetValues_has_PrecisionFacet_set_to_null()
        {
            Test_NullFacetValues_has_value_set_to_null<byte>(DbProviderManifest.PrecisionFacetName);
        }

        [Fact]
        public void NullFacetValues_has_ScaleFacet_set_to_null()
        {
            Test_NullFacetValues_has_value_set_to_null<byte>(DbProviderManifest.ScaleFacetName);
        }

        [Fact]
        public void NullFacetValues_does_not_have_DefaultValueFacet()
        {
            Test_NullFacetValues_has_value_set_to_null<decimal>(DbProviderManifest.DefaultValueFacetName, shouldBePresent: false);
        }

        [Fact]
        public void NullFacetValues_has_CollationFacet_set_to_null()
        {
            Test_NullFacetValues_has_value_set_to_null<string>(DbProviderManifest.CollationFacetName);
        }

        [Fact]
        public void NullFacetValues_has_SridFacet_set_to_null()
        {
            Test_NullFacetValues_has_value_set_to_null<int>(DbProviderManifest.SridFacetName);
        }

        [Fact]
        public void NullFacetValues_has_IsStrictFacet_set_to_null()
        {
            Test_NullFacetValues_has_value_set_to_null<bool>(DbProviderManifest.IsStrictFacetName);
        }

        [Fact]
        public void NullFacetValues_has_StoreGeneratedPatternFacet_set_to_null()
        {
            Test_NullFacetValues_has_value_set_to_null<StoreGeneratedPattern>(EdmProviderManifest.StoreGeneratedPatternFacetName);
        }

        [Fact]
        public void NullFacetValues_has_ConcurrencyModeFacet_set_to_null()
        {
            Test_NullFacetValues_has_value_set_to_null<ConcurrencyMode>(EdmProviderManifest.ConcurrencyModeFacetName);
        }

        [Fact]
        public void NullFacetValues_has_CollectionKindFacet_set_to_null()
        {
            Test_NullFacetValues_has_value_set_to_null<CollectionKind>(EdmConstants.CollectionKind);
        }

        private static void Test_NullFacetValues_has_value_set_to_null<T>(string facetName, bool shouldBePresent = true)
        {
            var description = GetFacetDescription<T>(facetName);

            var values = FacetValues.NullFacetValues;

            Facet returnedFacet = null;
            if (shouldBePresent)
            {
                Assert.True(values.TryGetFacet(description, out returnedFacet));
                Assert.Null(returnedFacet.Value);
            }
            else
            {
                Assert.False(values.TryGetFacet(description, out returnedFacet));
                Assert.Null(returnedFacet);
            }
        }

        private static FacetDescription GetFacetDescription<T>(string facetName)
        {
            var type = typeof(T);
            EdmType edmType;
            PrimitiveTypeKind primitiveTypeKind;
            if (ClrProviderManifest.TryGetPrimitiveTypeKind(type, out primitiveTypeKind))
            {
                edmType = new PrimitiveType { PrimitiveTypeKind = primitiveTypeKind };
            }
            else
            {
                edmType = new EnumType(type);
            }

            return new FacetDescription(facetName, edmType, null, null, null);
        }
    }
}
