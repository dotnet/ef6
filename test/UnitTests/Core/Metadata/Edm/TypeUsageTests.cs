// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm.Provider;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Moq;
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

        public class CreateStringTypeUsage
        {
            [Fact]
            public void Exception_thrown_when_primitive_type_is_not_string()
            {
                var primitiveTypeMock = new Mock<PrimitiveType>();
                primitiveTypeMock.SetupGet(m => m.PrimitiveTypeKind).Returns(PrimitiveTypeKind.Binary);

                Assert.Equal(
                    Strings.NotStringTypeForTypeUsage,
                    Assert.Throws<ArgumentException>(
                        () => TypeUsage.CreateStringTypeUsage(primitiveTypeMock.Object, isUnicode: true, isFixedLength: false)).Message);
            }

            [Fact]
            public void Exception_thrown_when_size_is_less_than_one()
            {
                var primitiveTypeMock = new Mock<PrimitiveType>();
                primitiveTypeMock.SetupGet(m => m.PrimitiveTypeKind).Returns(PrimitiveTypeKind.String);

                Assert.True(
                    Assert.Throws<ArgumentOutOfRangeException>(
                        () =>
                        TypeUsage.CreateStringTypeUsage(primitiveTypeMock.Object, isUnicode: true, isFixedLength: false, maxLength: 0))
                          .Message.StartsWith(Strings.InvalidMaxLengthSize));

                Assert.True(
                    Assert.Throws<ArgumentOutOfRangeException>(
                        () =>
                        TypeUsage.CreateStringTypeUsage(primitiveTypeMock.Object, isUnicode: true, isFixedLength: false, maxLength: -10))
                          .Message.StartsWith(Strings.InvalidMaxLengthSize));
            }
        }

        public class CreateBinaryTypeUsage
        {
            [Fact]
            public void Exception_thrown_when_primitive_type_is_not_binary()
            {
                var primitiveTypeMock = new Mock<PrimitiveType>();
                primitiveTypeMock.SetupGet(m => m.PrimitiveTypeKind).Returns(PrimitiveTypeKind.String);

                Assert.Equal(
                    Strings.NotBinaryTypeForTypeUsage,
                    Assert.Throws<ArgumentException>(() => TypeUsage.CreateBinaryTypeUsage(primitiveTypeMock.Object, isFixedLength: false))
                          .Message);
            }

            [Fact]
            public void Exception_thrown_when_size_is_less_than_one()
            {
                var primitiveTypeMock = new Mock<PrimitiveType>();
                primitiveTypeMock.SetupGet(m => m.PrimitiveTypeKind).Returns(PrimitiveTypeKind.Binary);

                Assert.True(
                    Assert.Throws<ArgumentOutOfRangeException>(
                        () => TypeUsage.CreateBinaryTypeUsage(primitiveTypeMock.Object, isFixedLength: false, maxLength: 0))
                          .Message.StartsWith(Strings.InvalidMaxLengthSize));

                Assert.True(
                    Assert.Throws<ArgumentOutOfRangeException>(
                        () => TypeUsage.CreateBinaryTypeUsage(primitiveTypeMock.Object, isFixedLength: false, maxLength: -10))
                          .Message.StartsWith(Strings.InvalidMaxLengthSize));
            }
        }

        public class GetModelTypeUsage
        {
            [Fact]
            public void This_returned_for_CSPace_type()
            {
                var typeUsage = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
                Assert.Same(typeUsage, typeUsage.ModelTypeUsage);
            }

            [Fact]
            public void This_returned_for_OSPace_type()
            {
                var enumType = new ClrEnumType(typeof(System.DayOfWeek), "foo", "DayOfWeek");
                var typeUsage = TypeUsage.Create(enumType);

                Assert.Same(typeUsage, typeUsage.ModelTypeUsage);
            }

            [Fact]
            public void Non_nullable_CSpace_primitive_type_returned_for_non_nullable_SSpace_primitive_type()
            {
                var sSpaceTypeUsage =
                    FakeSqlProviderServices
                        .Instance.GetProviderManifest("2008")
                        .GetStoreType(
                            TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)));

                var nonNullableSSpaceTypeUsage =
                    sSpaceTypeUsage.ShallowCopy(
                        new FacetValues
                            {
                                Nullable = false
                            });

                var cSpaceTypeUsage = nonNullableSSpaceTypeUsage.ModelTypeUsage;

                Assert.Equal(DataSpace.CSpace, cSpaceTypeUsage.EdmType.GetDataSpace());
                Assert.Equal(
                    ((PrimitiveType)sSpaceTypeUsage.EdmType).PrimitiveTypeKind,
                    ((PrimitiveType)cSpaceTypeUsage.EdmType).PrimitiveTypeKind);
                Assert.False((bool)cSpaceTypeUsage.Facets["Nullable"].Value);
            }

            [Fact]
            public void Nullable_CSpace_primitive_type_returned_for_non_nullable_SSpace_primitive_type()
            {
                var sSpaceTypeUsage =
                    FakeSqlProviderServices
                    .Instance.GetProviderManifest("2008")
                    .GetStoreType(TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)));

                var cSpaceTypeUsage = sSpaceTypeUsage.ModelTypeUsage;

                Assert.Equal(DataSpace.CSpace, cSpaceTypeUsage.EdmType.GetDataSpace());
                Assert.Equal(
                    ((PrimitiveType)sSpaceTypeUsage.EdmType).PrimitiveTypeKind,
                    ((PrimitiveType)cSpaceTypeUsage.EdmType).PrimitiveTypeKind);
                Assert.True((bool)sSpaceTypeUsage.Facets["Nullable"].Value);
            }

            [Fact]
            public void ProviderIncompatibleException_thrown_for_invalid_primitive_type()
            {
                var mockProviderManifest = new Mock<DbProviderManifest>();
                var fakeStorePrimitiveTypeUsage =
                    TypeUsage.Create(
                        new PrimitiveType(
                            "foo",
                            "bar",
                            DataSpace.SSpace,
                            EdmProviderManifest.Instance.GetPrimitiveType(PrimitiveTypeKind.Int32),
                            mockProviderManifest.Object));

                Assert.Equal(
                    Strings.Mapping_ProviderReturnsNullType("bar.foo"),
                    Assert.Throws<ProviderIncompatibleException>(() => fakeStorePrimitiveTypeUsage.ModelTypeUsage).Message);
            }

            [Fact]
            public void CSpace_CollectionType_returned_for_SSpace_CollectionType()
            {
                var sSpaceCollectionTypeUsage =
                    TypeUsage.Create(
                        FakeSqlProviderServices.Instance.GetProviderManifest("2008")
                                               .GetStoreTypes()
                                               .First(t => t.PrimitiveTypeKind == PrimitiveTypeKind.Geometry)
                                               .GetCollectionType());

                var cSpaceCollectionType = sSpaceCollectionTypeUsage.ModelTypeUsage.EdmType;

                Assert.Equal(DataSpace.CSpace, cSpaceCollectionType.GetDataSpace());
                Assert.Equal(BuiltInTypeKind.CollectionType, cSpaceCollectionType.BuiltInTypeKind);

                var elementType = ((CollectionType)cSpaceCollectionType).TypeUsage.EdmType;
                Assert.Equal(DataSpace.CSpace, elementType.GetDataSpace());
                Assert.Equal(PrimitiveTypeKind.Geometry, ((PrimitiveType)elementType).PrimitiveTypeKind);
            }

            [Fact]
            public void CSpace_RowType_returned_for_SSpace_RowType()
            {
                var sSpaceTypeUsage =
                    FakeSqlProviderServices
                        .Instance.GetProviderManifest("2008")
                        .GetStoreType(TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)));

                var sSpaceRowTypeUsage = 
                    TypeUsage.CreateDefaultTypeUsage(RowType.Create(new[] { EdmProperty.Create("foo", sSpaceTypeUsage) }, null));

                var cSpaceRowType = (RowType)sSpaceRowTypeUsage.ModelTypeUsage.EdmType;
                
                Assert.Equal(DataSpace.CSpace, cSpaceRowType.GetDataSpace());
                Assert.Equal(1, cSpaceRowType.Properties.Count);
                Assert.Equal(DataSpace.CSpace, cSpaceRowType.Properties.Single().TypeUsage.EdmType.GetDataSpace());
                Assert.Equal("foo", cSpaceRowType.Properties.Single().Name);
                Assert.Equal(
                    PrimitiveTypeKind.String,
                    ((PrimitiveType)cSpaceRowType.Properties.Single().TypeUsage.EdmType).PrimitiveTypeKind);
            }
        }
    }
}
