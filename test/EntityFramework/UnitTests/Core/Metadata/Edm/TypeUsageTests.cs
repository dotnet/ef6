// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Moq;
    using System.Data.Entity.Resources;
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
                    Assert.Throws<ArgumentException>(() => TypeUsage.CreateStringTypeUsage(primitiveTypeMock.Object, isUnicode: true, isFixedLength: false)).Message);
            }

            [Fact]
            public void Exception_thrown_when_size_is_less_than_one()
            {
                var primitiveTypeMock = new Mock<PrimitiveType>();
                primitiveTypeMock.SetupGet(m => m.PrimitiveTypeKind).Returns(PrimitiveTypeKind.String);

                Assert.True(
                    Assert.Throws<ArgumentOutOfRangeException>(() => TypeUsage.CreateStringTypeUsage(primitiveTypeMock.Object, isUnicode: true, isFixedLength: false, maxLength: 0)).Message.StartsWith(Strings.InvalidMaxLengthSize));

                Assert.True(
                    Assert.Throws<ArgumentOutOfRangeException>(() => TypeUsage.CreateStringTypeUsage(primitiveTypeMock.Object, isUnicode: true, isFixedLength: false, maxLength: -10)).Message.StartsWith(Strings.InvalidMaxLengthSize));
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
                    Assert.Throws<ArgumentException>(() => TypeUsage.CreateBinaryTypeUsage(primitiveTypeMock.Object, isFixedLength: false)).Message);
            }

            [Fact]
            public void Exception_thrown_when_size_is_less_than_one()
            {
                var primitiveTypeMock = new Mock<PrimitiveType>();
                primitiveTypeMock.SetupGet(m => m.PrimitiveTypeKind).Returns(PrimitiveTypeKind.Binary);

                Assert.True(
                    Assert.Throws<ArgumentOutOfRangeException>(() => TypeUsage.CreateBinaryTypeUsage(primitiveTypeMock.Object, isFixedLength: false, maxLength: 0)).Message.StartsWith(Strings.InvalidMaxLengthSize));

                Assert.True(
                    Assert.Throws<ArgumentOutOfRangeException>(() => TypeUsage.CreateBinaryTypeUsage(primitiveTypeMock.Object, isFixedLength: false, maxLength: -10)).Message.StartsWith(Strings.InvalidMaxLengthSize));
            }
        }
    }
}
