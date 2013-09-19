// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.Utilities
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Moq;
    using Xunit;

    public class TypeUsageExtensionsTests
    {
        public class GetPrecision
        {
            [Fact]
            public void GetPrecision_returns_the_Precision_facet()
            {
                Assert.Equal(
                    77,
                    TypeUsage.Create(new Mock<EdmType>().Object, CreateFacetList(DbProviderManifest.PrecisionFacetName, (byte)77))
                        .GetPrecision());
            }
        }

        public class GetScale
        {
            [Fact]
            public void GetScale_returns_the_Scale_facet()
            {
                Assert.Equal(
                    88,
                    TypeUsage.Create(new Mock<EdmType>().Object, CreateFacetList(DbProviderManifest.ScaleFacetName, (byte)88))
                        .GetScale());
            }
        }

        public class GetMaxLength
        {
            [Fact]
            public void GetMaxLength_returns_the_MaxLength_facet()
            {
                Assert.Equal(
                    12345,
                    TypeUsage.Create(new Mock<EdmType>().Object, CreateFacetList(DbProviderManifest.MaxLengthFacetName, 12345))
                        .GetMaxLength());
            }
        }

        public class GetFacetValue
        {
            [Fact]
            public void GetFacetValue_returns_the_named_facet_value()
            {
                Assert.Equal(
                    "Unicorn",
                    TypeUsage.Create(new Mock<EdmType>().Object, CreateFacetList("Magic", "Unicorn"))
                        .GetFacetValue<string>("Magic"));
            }
        }

        public class IsFixedLength
        {
            [Fact]
            public void IsFixedLength_returns_false_for_types_that_dont_have_the_fixed_length_facet()
            {
                Assert.False(
                    TypeUsage.Create(new Mock<EdmType>().Object, CreateFacetList("Magic", "Unicorn"))
                        .IsFixedLength());
            }

            [Fact]
            public void IsFixedLength_returns_false_for_types_that_have_null_fixed_length_facet()
            {
                Assert.False(
                    TypeUsage.Create(new Mock<EdmType>().Object, CreateFacetList(DbProviderManifest.FixedLengthFacetName, null))
                        .IsFixedLength());
            }

            [Fact]
            public void IsFixedLength_returns_false_for_types_that_have_false_fixed_length_facet()
            {
                Assert.False(
                    TypeUsage.Create(new Mock<EdmType>().Object, CreateFacetList(DbProviderManifest.FixedLengthFacetName, false))
                        .IsFixedLength());
            }

            [Fact]
            public void IsFixedLength_returns_true_for_types_that_have_true_fixed_length_facet()
            {
                Assert.True(
                    TypeUsage.Create(new Mock<EdmType>().Object, CreateFacetList(DbProviderManifest.FixedLengthFacetName, true))
                        .IsFixedLength());
            }
        }

        public class TryGetPrecision
        {
            [Fact]
            public void TryGetPrecision_returns_false_for_non_decimal_types()
            {
                byte _;
                Assert.False(
                    TypeUsage.Create(CreateMockPrimitiveType(PrimitiveTypeKind.Double).Object, Enumerable.Empty<Facet>())
                        .TryGetPrecision(out _));
            }

            [Fact]
            public void TryGetPrecision_returns_false_for_decimal_type_with_no_precision_facet()
            {
                byte _;
                Assert.False(
                    TypeUsage.Create(CreateMockPrimitiveType(PrimitiveTypeKind.Decimal).Object, Enumerable.Empty<Facet>())
                        .TryGetPrecision(out _));
            }

            [Fact]
            public void TryGetPrecision_returns_false_for_decimal_type_with_null_precision_facet()
            {
                byte _;
                Assert.False(
                    TypeUsage.Create(
                        CreateMockPrimitiveType(PrimitiveTypeKind.Decimal).Object,
                        CreateFacetList(DbProviderManifest.PrecisionFacetName, null))
                        .TryGetPrecision(out _));
            }

            [Fact]
            public void TryGetPrecision_returns_true_and_value_for_decimal_type_with_precision_facet()
            {
                byte precision;
                Assert.True(
                    TypeUsage.Create(
                        CreateMockPrimitiveType(PrimitiveTypeKind.Decimal).Object,
                        CreateFacetList(DbProviderManifest.PrecisionFacetName, (byte)33))
                        .TryGetPrecision(out precision));
                Assert.Equal(33, precision);
            }
        }

        public class TryGetScale
        {
            [Fact]
            public void TryGetScale_returns_false_for_non_decimal_types()
            {
                byte _;
                Assert.False(
                    TypeUsage.Create(CreateMockPrimitiveType(PrimitiveTypeKind.Double).Object, Enumerable.Empty<Facet>())
                        .TryGetScale(out _));
            }

            [Fact]
            public void TryGetScale_returns_false_for_decimal_type_with_no_scale_facet()
            {
                byte _;
                Assert.False(
                    TypeUsage.Create(CreateMockPrimitiveType(PrimitiveTypeKind.Decimal).Object, Enumerable.Empty<Facet>())
                        .TryGetScale(out _));
            }

            [Fact]
            public void TryGetScale_returns_false_for_decimal_type_with_null_scale_facet()
            {
                byte _;
                Assert.False(
                    TypeUsage.Create(
                        CreateMockPrimitiveType(PrimitiveTypeKind.Decimal).Object,
                        CreateFacetList(DbProviderManifest.ScaleFacetName, null))
                        .TryGetScale(out _));
            }

            [Fact]
            public void TryGetScale_returns_true_and_value_for_decimal_type_with_scale_facet()
            {
                byte scale;
                Assert.True(
                    TypeUsage.Create(
                        CreateMockPrimitiveType(PrimitiveTypeKind.Decimal).Object,
                        CreateFacetList(DbProviderManifest.ScaleFacetName, (byte)33))
                        .TryGetScale(out scale));
                Assert.Equal(33, scale);
            }
        }

        public class TryGetFacetValue
        {
            [Fact]
            public void TryGetFacetValue_returns_false_for_type_with_no_given_facet()
            {
                bool _;
                Assert.False(
                    TypeUsage.Create(new Mock<EdmType>().Object, Enumerable.Empty<Facet>())
                        .TryGetFacetValue("Magic", out _));
            }

            [Fact]
            public void TryGetFacetValue_returns_false_for_type_with_null_given_facet()
            {
                bool _;
                Assert.False(
                    TypeUsage.Create(new Mock<EdmType>().Object, CreateFacetList("Magic", null))
                        .TryGetFacetValue("Magic", out _));
            }

            [Fact]
            public void TryGetFacetValue_returns_false_for_type_with_UnboundedValue_facet_value()
            {
                bool _;
                Assert.False(
                    TypeUsage.Create(new Mock<EdmType>().Object, CreateFacetList("Magic", EdmConstants.UnboundedValue))
                        .TryGetFacetValue("Magic", out _));
            }

            [Fact]
            public void TryGetFacetValue_returns_false_for_type_with_VariableValue_facet_value()
            {
                bool _;
                Assert.False(
                    TypeUsage.Create(new Mock<EdmType>().Object, CreateFacetList("Magic", EdmConstants.VariableValue))
                        .TryGetFacetValue("Magic", out _));
            }

            [Fact]
            public void TryGetFacetValue_returns_true_and_value_for_type_with_given_facet()
            {
                int result;
                Assert.True(
                    TypeUsage.Create(new Mock<EdmType>().Object, CreateFacetList("Magic", 68000))
                        .TryGetFacetValue("Magic", out result));
                Assert.Equal(68000, result);
            }
        }

        public class TryGetPrimitiveTypeKind
        {
            [Fact]
            public void TryGetPrimitiveTypeKind_returns_type_kind_for_primitive_type()
            {
                Assert.Equal(
                    PrimitiveTypeKind.DateTime,
                    TypeUsage.Create(CreateMockPrimitiveType(PrimitiveTypeKind.DateTime).Object).GetPrimitiveTypeKind());
            }
        }

        public class IsPrimitiveType
        {
            [Fact]
            public void IsPrimitiveType_returns_false_for_null_type()
            {
                Assert.False(TypeUsageExtensions.IsPrimitiveType(null));
            }

            [Fact]
            public void IsPrimitiveType_returns_false_for_non_primitive_type()
            {
                var mockEdmType = new Mock<EdmType>();
                mockEdmType.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.ComplexType);

                Assert.False(TypeUsage.Create(mockEdmType.Object).IsPrimitiveType());
            }

            [Fact]
            public void IsPrimitiveType_with_out_returns_false_for_null_type()
            {
                Assert.False(TypeUsageExtensions.IsPrimitiveType(null, PrimitiveTypeKind.Binary));
            }

            [Fact]
            public void IsPrimitiveType_with_out_returns_false_for_non_primitive_type()
            {
                var mockEdmType = new Mock<EdmType>();
                mockEdmType.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.ComplexType);

                Assert.False(TypeUsage.Create(mockEdmType.Object).IsPrimitiveType(PrimitiveTypeKind.Binary));
            }

            [Fact]
            public void IsPrimitiveType_returns_false_for_different_primitive_type()
            {
                Assert.False(
                    TypeUsage.Create(CreateMockPrimitiveType(PrimitiveTypeKind.DateTime).Object).IsPrimitiveType(PrimitiveTypeKind.Binary));
            }

            [Fact]
            public void IsPrimitiveType_returns_true_for_matching_primitive_type()
            {
                Assert.True(
                    TypeUsage.Create(CreateMockPrimitiveType(PrimitiveTypeKind.DateTime).Object).IsPrimitiveType(PrimitiveTypeKind.DateTime));
            }
        }

        public class IsNullable
        {
            [Fact]
            public void IsNullable_returns_false_for_types_that_dont_have_the_fixed_length_facet()
            {
                Assert.False(
                    TypeUsage.Create(new Mock<EdmType>().Object, CreateFacetList("Magic", "Unicorn"))
                        .IsNullable());
            }

            [Fact]
            public void IsNullable_returns_false_for_types_that_have_null_fixed_length_facet()
            {
                Assert.False(
                    TypeUsage.Create(new Mock<EdmType>().Object, CreateFacetList(DbProviderManifest.NullableFacetName, null))
                        .IsNullable());
            }

            [Fact]
            public void IsNullable_returns_false_for_types_that_have_false_fixed_length_facet()
            {
                Assert.False(
                    TypeUsage.Create(new Mock<EdmType>().Object, CreateFacetList(DbProviderManifest.NullableFacetName, false))
                        .IsNullable());
            }

            [Fact]
            public void IsNullable_returns_true_for_types_that_have_true_fixed_length_facet()
            {
                Assert.True(
                    TypeUsage.Create(new Mock<EdmType>().Object, CreateFacetList(DbProviderManifest.NullableFacetName, true))
                        .IsNullable());
            }
        }

        public class TryGetIsUnicode
        {
            [Fact]
            public void TryGetIsUnicode_returns_false_for_non_string_types()
            {
                bool _;
                Assert.False(
                    TypeUsage.Create(CreateMockPrimitiveType(PrimitiveTypeKind.Double).Object, Enumerable.Empty<Facet>())
                        .TryGetIsUnicode(out _));
            }

            [Fact]
            public void TryGetIsUnicode_returns_false_for_string_type_with_no_Unicode_facet()
            {
                bool _;
                Assert.False(
                    TypeUsage.Create(CreateMockPrimitiveType(PrimitiveTypeKind.String).Object, Enumerable.Empty<Facet>())
                        .TryGetIsUnicode(out _));
            }

            [Fact]
            public void TryGetIsUnicode_returns_false_for_string_type_with_null_Unicode_facet()
            {
                bool _;
                Assert.False(
                    TypeUsage.Create(
                        CreateMockPrimitiveType(PrimitiveTypeKind.String).Object,
                        CreateFacetList(DbProviderManifest.UnicodeFacetName, null))
                        .TryGetIsUnicode(out _));
            }

            [Fact]
            public void TryGetIsUnicode_returns_true_and_sets_true_value_for_string_type_with_Unicode_facet_set_to_true()
            {
                bool isUnicode;
                Assert.True(
                    TypeUsage.Create(
                        CreateMockPrimitiveType(PrimitiveTypeKind.String).Object,
                        CreateFacetList(DbProviderManifest.UnicodeFacetName, true))
                        .TryGetIsUnicode(out isUnicode));
                Assert.True(isUnicode);
            }

            [Fact]
            public void TryGetIsUnicode_returns_true_and_sets_false_value_for_string_type_with_Unicode_facet_set_to_false()
            {
                bool isUnicode;
                Assert.True(
                    TypeUsage.Create(
                        CreateMockPrimitiveType(PrimitiveTypeKind.String).Object,
                        CreateFacetList(DbProviderManifest.UnicodeFacetName, false))
                        .TryGetIsUnicode(out isUnicode));
                Assert.False(isUnicode);
            }
        }

        public class TryGetMaxLength
        {
            [Fact]
            public void TryGetMaxLength_returns_false_for_types_that_are_not_string_or_binary()
            {
                int _;
                Assert.False(
                    TypeUsage.Create(CreateMockPrimitiveType(PrimitiveTypeKind.Double).Object, Enumerable.Empty<Facet>())
                        .TryGetMaxLength(out _));
            }

            [Fact]
            public void TryGetMaxLength_returns_false_for_matching_type_with_no_MaxLength_facet()
            {
                int _;
                Assert.False(
                    TypeUsage.Create(CreateMockPrimitiveType(PrimitiveTypeKind.Binary).Object, Enumerable.Empty<Facet>())
                        .TryGetMaxLength(out _));
            }

            [Fact]
            public void TryGetMaxLength_returns_false_for_matching_type_with_null_MaxLength_facet()
            {
                int _;
                Assert.False(
                    TypeUsage.Create(
                        CreateMockPrimitiveType(PrimitiveTypeKind.String).Object,
                        CreateFacetList(DbProviderManifest.MaxLengthFacetName, null))
                        .TryGetMaxLength(out _));
            }

            [Fact]
            public void TryGetMaxLength_returns_false_for_matching_type_with_UnboundedValue_MaxLength_facet()
            {
                int _;
                Assert.False(
                    TypeUsage.Create(
                        CreateMockPrimitiveType(PrimitiveTypeKind.Binary).Object,
                        CreateFacetList(DbProviderManifest.MaxLengthFacetName, EdmConstants.UnboundedValue))
                        .TryGetMaxLength(out _));
            }

            [Fact]
            public void TryGetMaxLength_returns_false_for_matching_type_with_VariableValue_MaxLength_facet()
            {
                int _;
                Assert.False(
                    TypeUsage.Create(
                        CreateMockPrimitiveType(PrimitiveTypeKind.String).Object,
                        CreateFacetList(DbProviderManifest.MaxLengthFacetName, EdmConstants.VariableValue))
                        .TryGetMaxLength(out _));
            }

            [Fact]
            public void TryGetMaxLength_returns_true_and_value_for_string_type_with_MaxLength_facet()
            {
                int maxLength;
                Assert.True(
                    TypeUsage.Create(
                        CreateMockPrimitiveType(PrimitiveTypeKind.String).Object,
                        CreateFacetList(DbProviderManifest.MaxLengthFacetName, 68020))
                        .TryGetMaxLength(out maxLength));
                Assert.Equal(68020, maxLength);
            }

            [Fact]
            public void TryGetMaxLength_returns_true_and_value_for_binary_type_with_MaxLength_facet()
            {
                int maxLength;
                Assert.True(
                    TypeUsage.Create(
                        CreateMockPrimitiveType(PrimitiveTypeKind.Binary).Object,
                        CreateFacetList(DbProviderManifest.MaxLengthFacetName, 68020))
                        .TryGetMaxLength(out maxLength));
                Assert.Equal(68020, maxLength);
            }
        }

        public class GetProperties
        {
            [Fact]
            public void GetProperties_returns_complex_type_properties()
            {
                var properties = new ReadOnlyMetadataCollection<EdmProperty>();

                var mockEdmType = new Mock<ComplexType>();
                mockEdmType.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.ComplexType);
                mockEdmType.Setup(m => m.Properties).Returns(properties);

                Assert.Same(properties, TypeUsage.Create(mockEdmType.Object).GetProperties());
            }

            [Fact]
            public void GetProperties_returns_entity_type_properties()
            {
                var properties = new ReadOnlyMetadataCollection<EdmProperty>();

                var mockEdmType = new Mock<EntityType>("E", "N", DataSpace.CSpace);
                mockEdmType.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.EntityType);
                mockEdmType.Setup(m => m.Properties).Returns(properties);

                Assert.Same(properties, TypeUsage.Create(mockEdmType.Object).GetProperties());
            }

            [Fact]
            public void GetProperties_returns_row_type_properties()
            {
                var properties = new ReadOnlyMetadataCollection<EdmProperty>();

                var mockEdmType = new Mock<RowType>();
                mockEdmType.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.RowType);
                mockEdmType.Setup(m => m.Properties).Returns(properties);

                Assert.Same(properties, TypeUsage.Create(mockEdmType.Object).GetProperties());
            }

            [Fact]
            public void GetProperties_returns_no_properties_for_types_that_dont_have_properties()
            {
                var mockEdmType = new Mock<PrimitiveType>();
                mockEdmType.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.PrimitiveType);

                Assert.Empty(TypeUsage.Create(mockEdmType.Object).GetProperties());
            }
        }

        public class GetElementTypeUsage
        {
            [Fact]
            public void GetElementTypeUsage_returns_collection_element_usage_for_collection_type()
            {
                var typeUsage = new TypeUsage();

                var mockEdmType = new Mock<CollectionType>();
                mockEdmType.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.CollectionType);
                mockEdmType.Setup(m => m.TypeUsage).Returns(typeUsage);

                Assert.Same(typeUsage, TypeUsage.Create(mockEdmType.Object).GetElementTypeUsage());
            }

            [Fact]
            public void GetElementTypeUsage_returns_reference_element_usage_for_reference_type()
            {
                var refType = new Mock<EntityType>("E", "N", DataSpace.CSpace).Object;

                var mockEdmType = new Mock<RefType>();
                mockEdmType.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.RefType);
                mockEdmType.Setup(m => m.ElementType).Returns(refType);

                Assert.Same(refType, TypeUsage.Create(mockEdmType.Object).GetElementTypeUsage().EdmType);
            }

            [Fact]
            public void GetElementTypeUsage_returns_null_for_non_collection_or_reference_type()
            {
                Assert.Null(TypeUsage.Create(CreateMockPrimitiveType(PrimitiveTypeKind.DateTime).Object).GetElementTypeUsage());
            }
        }

        public class MustFacetBeConstant
        {
            [Fact]
            public void MustFacetBeConstant_returns_true_for_a_facet_description_marked_as_constant()
            {
                MustFacetBeConstant_returns_IsConstant_for_a_facet_description_marked_as_constant(isConstant: true);
            }

            [Fact]
            public void MustFacetBeConstant_returns_false_for_a_facet_description_marked_as_not_constant()
            {
                MustFacetBeConstant_returns_IsConstant_for_a_facet_description_marked_as_constant(isConstant: false);
            }

            private void MustFacetBeConstant_returns_IsConstant_for_a_facet_description_marked_as_constant(bool isConstant)
            {
                var mockFacetDescription = new Mock<FacetDescription>();
                mockFacetDescription.Setup(m => m.FacetName).Returns("Magic");
                mockFacetDescription.Setup(m => m.IsConstant).Returns(isConstant);

                var mockType = CreateMockPrimitiveType(PrimitiveTypeKind.Byte);
                mockType.Setup(m => m.FacetDescriptions).Returns(
                    new ReadOnlyCollection<FacetDescription>(new[] { mockFacetDescription.Object }));

                Assert.Equal(isConstant, TypeUsage.Create(mockType.Object).MustFacetBeConstant("Magic"));
            }
        }

        public class IsSpatialType
        {
            [Fact]
            public void IsSpatialType_returns_false_for_non_primitive_type()
            {
                var mockEdmType = new Mock<EntityType>("E", "N", DataSpace.CSpace);
                mockEdmType.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.EntityType);

                Assert.False(TypeUsage.Create(mockEdmType.Object).IsSpatialType());
            }

            [Fact]
            public void IsSpatialType_returns_false_for_prmitive_but_non_spatial_type()
            {
                Assert.False(TypeUsage.Create(CreateMockPrimitiveType(PrimitiveTypeKind.DateTime).Object).IsSpatialType());
            }

            [Fact]
            public void IsSpatialType_returns_true_for_spatial_type()
            {
                Assert.True(TypeUsage.Create(CreateMockPrimitiveType(PrimitiveTypeKind.GeographyMultiLineString).Object).IsSpatialType());
            }

            [Fact]
            public void IsSpatialType_with_out_returns_false_for_non_primitive_type()
            {
                var mockEdmType = new Mock<EntityType>("E", "N", DataSpace.CSpace);
                mockEdmType.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.EntityType);

                PrimitiveTypeKind _;
                Assert.False(TypeUsage.Create(mockEdmType.Object).IsSpatialType(out _));
            }

            [Fact]
            public void IsSpatialType_with_out_returns_false_for_prmitive_but_non_spatial_type()
            {
                PrimitiveTypeKind _;
                Assert.False(TypeUsage.Create(CreateMockPrimitiveType(PrimitiveTypeKind.DateTime).Object).IsSpatialType(out _));
            }

            [Fact]
            public void IsSpatialType_with_out_returns_true_for_spatial_type()
            {
                PrimitiveTypeKind result;
                Assert.True(
                    TypeUsage.Create(CreateMockPrimitiveType(PrimitiveTypeKind.GeographyMultiLineString).Object).IsSpatialType(out result));
                Assert.Equal(PrimitiveTypeKind.GeographyMultiLineString, result);
            }
        }

        public class ForceNonUnicode
        {
            public void ForceNonUnicode_converts_a_unicode_string_type_usage_to_a_non_unicode_string_type_usage()
            {
                var stringUsage = TypeUsage.CreateStringTypeUsage(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String),
                    isUnicode: true,
                    isFixedLength: false,
                    maxLength: 256);

                var facets = stringUsage.Facets.Select(
                    f => new
                             {
                                 f.Name,
                                 f.Value
                             }).ToList();

                var nonUnicode = stringUsage.ForceNonUnicode();
                Assert.False((bool)nonUnicode.Facets.Single(f => f.Name == "Unicode").Value);

                foreach (var facet in facets)
                {
                    if (facet.Name != "Unicode")
                    {
                        Assert.Equal(facet.Value, nonUnicode.Facets.Single(f => f.Name == facet.Name).Value);
                    }
                }
            }
        }

        private static Mock<PrimitiveType> CreateMockPrimitiveType(PrimitiveTypeKind kind)
        {
            var mockEdmType = new Mock<PrimitiveType>();
            mockEdmType.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.PrimitiveType);
            mockEdmType.Setup(m => m.PrimitiveTypeKind).Returns(kind);

            return mockEdmType;
        }

        private static IEnumerable<Facet> CreateFacetList(string name, object value)
        {
            return
                new[]
                    {
                        CreateMockFacet("Foo", "fooValue").Object,
                        CreateMockFacet(name, value).Object,
                        CreateMockFacet("Bar", "barValue").Object
                    };
        }

        private static Mock<Facet> CreateMockFacet(string name, object value)
        {
            var mockFacet = new Mock<Facet>();
            mockFacet.Setup(m => m.Name).Returns(name);
            mockFacet.Setup(m => m.Identity).Returns(name);
            mockFacet.Setup(m => m.Value).Returns(value);

            return mockFacet;
        }
    }
}
