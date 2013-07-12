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
            var description = new FacetDescription(
                DbProviderManifest.NullableFacetName, new PrimitiveType { PrimitiveTypeKind = PrimitiveTypeKind.Boolean }, null, null, null);
            var facet = Facet.Create(description, value: true);

            var values = FacetValues.Create(new List<Facet> { facet });

            Facet returnedFacet = null;
            Assert.True(values.TryGetFacet(description, out returnedFacet));
            Assert.Equal(facet.Value, returnedFacet.Value);
        }

        [Fact]
        public void Create_copies_value_from_MaxLengthFacet()
        {
            var description = new FacetDescription(
                DbProviderManifest.MaxLengthFacetName, new PrimitiveType { PrimitiveTypeKind = PrimitiveTypeKind.Int32 }, null, null, null);
            var facet = Facet.Create(description, value: 1);

            var values = FacetValues.Create(new List<Facet> { facet });

            Facet returnedFacet = null;
            Assert.True(values.TryGetFacet(description, out returnedFacet));
            Assert.Equal(facet.Value, returnedFacet.Value);
        }

        [Fact]
        public void Create_copies_value_from_UnicodeFacet()
        {
            var description = new FacetDescription(
                DbProviderManifest.UnicodeFacetName, new PrimitiveType { PrimitiveTypeKind = PrimitiveTypeKind.Boolean }, null, null, null);
            var facet = Facet.Create(description, value: true);

            var values = FacetValues.Create(new List<Facet> { facet });

            Facet returnedFacet = null;
            Assert.True(values.TryGetFacet(description, out returnedFacet));
            Assert.Equal(facet.Value, returnedFacet.Value);
        }

        [Fact]
        public void Create_copies_value_from_FixedLengthFacet()
        {
            var description = new FacetDescription(
                DbProviderManifest.FixedLengthFacetName, new PrimitiveType { PrimitiveTypeKind = PrimitiveTypeKind.Boolean }, null, null,
                null);
            var facet = Facet.Create(description, value: true);

            var values = FacetValues.Create(new List<Facet> { facet });

            Facet returnedFacet = null;
            Assert.True(values.TryGetFacet(description, out returnedFacet));
            Assert.Equal(facet.Value, returnedFacet.Value);
        }

        [Fact]
        public void Create_copies_value_from_PrecisionFacet()
        {
            var description = new FacetDescription(
                DbProviderManifest.PrecisionFacetName, new PrimitiveType { PrimitiveTypeKind = PrimitiveTypeKind.Byte }, null, null, null);
            var facet = Facet.Create(description, value: (byte)1);

            var values = FacetValues.Create(new List<Facet> { facet });

            Facet returnedFacet = null;
            Assert.True(values.TryGetFacet(description, out returnedFacet));
            Assert.Equal(facet.Value, returnedFacet.Value);
        }

        [Fact]
        public void Create_copies_value_from_ScaleFacet()
        {
            var description = new FacetDescription(
                DbProviderManifest.ScaleFacetName, new PrimitiveType { PrimitiveTypeKind = PrimitiveTypeKind.Byte }, null, null, null);
            var facet = Facet.Create(description, value: (byte)1);

            var values = FacetValues.Create(new List<Facet> { facet });

            Facet returnedFacet = null;
            Assert.True(values.TryGetFacet(description, out returnedFacet));
            Assert.Equal(facet.Value, returnedFacet.Value);
        }

        [Fact]
        public void Create_copies_value_from_DefaultValueFacet()
        {
            var description = new FacetDescription(
                DbProviderManifest.DefaultValueFacetName, new PrimitiveType { PrimitiveTypeKind = PrimitiveTypeKind.Double }, null, null,
                null);
            var facet = Facet.Create(description, value: 0.1);

            var values = FacetValues.Create(new List<Facet> { facet });

            Facet returnedFacet = null;
            Assert.True(values.TryGetFacet(description, out returnedFacet));
            Assert.Equal(facet.Value, returnedFacet.Value);
        }

        [Fact]
        public void Create_copies_value_from_CollationFacet()
        {
            var description = new FacetDescription(
                DbProviderManifest.CollationFacetName, new PrimitiveType { PrimitiveTypeKind = PrimitiveTypeKind.Boolean }, null, null, null);
            var facet = Facet.Create(description, value: true);

            var values = FacetValues.Create(new List<Facet> { facet });

            Facet returnedFacet = null;
            Assert.True(values.TryGetFacet(description, out returnedFacet));
            Assert.Equal(facet.Value, returnedFacet.Value);
        }

        [Fact]
        public void Create_copies_value_from_SridFacet()
        {
            var description = new FacetDescription(
                DbProviderManifest.SridFacetName, new PrimitiveType { PrimitiveTypeKind = PrimitiveTypeKind.Int32 }, null, null, null);
            var facet = Facet.Create(description, value: 1);

            var values = FacetValues.Create(new List<Facet> { facet });

            Facet returnedFacet = null;
            Assert.True(values.TryGetFacet(description, out returnedFacet));
            Assert.Equal(facet.Value, returnedFacet.Value);
        }

        [Fact]
        public void Create_copies_value_from_IsStrictFacet()
        {
            var description = new FacetDescription(
                DbProviderManifest.IsStrictFacetName, new PrimitiveType { PrimitiveTypeKind = PrimitiveTypeKind.Boolean }, null, null, null);
            var facet = Facet.Create(description, value: true);

            var values = FacetValues.Create(new List<Facet> { facet });

            Facet returnedFacet = null;
            Assert.True(values.TryGetFacet(description, out returnedFacet));
            Assert.Equal(facet.Value, returnedFacet.Value);
        }

        [Fact]
        public void Create_copies_value_from_StoreGeneratedPatternFacet()
        {
            var description = new FacetDescription(
                EdmProviderManifest.StoreGeneratedPatternFacetName, new EnumType(typeof(StoreGeneratedPattern)), null, null, null);
            var facet = Facet.Create(description, value: StoreGeneratedPattern.Computed);

            var values = FacetValues.Create(new List<Facet> { facet });

            Facet returnedFacet = null;
            Assert.True(values.TryGetFacet(description, out returnedFacet));
            Assert.Equal(facet.Value, returnedFacet.Value);
        }

        [Fact]
        public void Create_copies_value_from_ConcurrencyModeFacet()
        {
            var description = new FacetDescription(
                EdmProviderManifest.ConcurrencyModeFacetName, new EnumType(typeof(ConcurrencyMode)), null, null, null);
            var facet = Facet.Create(description, value: ConcurrencyMode.Fixed);

            var values = FacetValues.Create(new List<Facet> { facet });

            Facet returnedFacet = null;
            Assert.True(values.TryGetFacet(description, out returnedFacet));
            Assert.Equal(facet.Value, returnedFacet.Value);
        }
    }
}
