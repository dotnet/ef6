// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using Xunit;

    public class EnumTypeTests
    {
        [Fact]
        public void Can_create_enumeration_type()
        {
            var stringTypeUsage = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var metadataProperty = new MetadataProperty("MetadataProperty", stringTypeUsage, "Value");
            var underlyingType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);
            var members
                = new[]
                  {
                      EnumMember.Create("M1", 1, null),
                      EnumMember.Create("M2", 2, null)
                  };

            var enumType = EnumType.Create("EnumName", "N", underlyingType, true, members, new[] { metadataProperty });

            Assert.Equal("EnumName", enumType.Name);
            Assert.Equal("N", enumType.NamespaceName);
            Assert.True(enumType.IsFlags);
            Assert.Equal(DataSpace.CSpace, enumType.DataSpace);
            Assert.Equal(members, enumType.Members);
            Assert.Same(metadataProperty, enumType.MetadataProperties.Last());
            Assert.True(enumType.IsReadOnly);
        }

        [Fact]
        public void Create_throws_if_name_is_null_or_empty()
        {
            var underlyingType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(
                    () => EnumType.Create(null, "N", underlyingType, true, null, null))
                    .Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(
                    () => EnumType.Create(String.Empty, "N", underlyingType, true, null, null))
                    .Message);
        }

        [Fact]
        public void Create_throws_if_namespaceName_is_null_or_empty()
        {
            var underlyingType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("namespaceName"),
                Assert.Throws<ArgumentException>(
                    () => EnumType.Create("EnumName", null, underlyingType, true, null, null))
                    .Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("namespaceName"),
                Assert.Throws<ArgumentException>(
                    () => EnumType.Create("EnumName", String.Empty, underlyingType, true, null, null))
                    .Message);
        }

        [Fact]
        public void Create_throws_if_underlyingType_is_null()
        {
            Assert.Equal(
                "underlyingType",
                Assert.Throws<ArgumentNullException>(
                    () => EnumType.Create("EnumName", "N", null, true, null, null))
                    .ParamName);
        }

        [Fact]
        public void Create_throws_if_underlyingType_is_not_supported()
        {
            var underlyingType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String);

            Assert.Equal(
                new ArgumentException(Strings.InvalidEnumUnderlyingType, "underlyingType")
                    .Message,
                Assert.Throws<ArgumentException>(
                    () => EnumType.Create("EnumName", "N", underlyingType, true, null, null))
                    .Message);
        }

        [Fact]
        public void Create_throws_if_member_value_not_in_range_of_underlying_type()
        {
            var underlyingType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Byte);
            var member = EnumMember.Create("M", Int32.MaxValue, null);

            Assert.Equal(
                new ArgumentException(
                    Strings.EnumMemberValueOutOfItsUnderylingTypeRange(
                        member.Value, member.Name, underlyingType.Name), "members")
                    .Message,
                Assert.Throws<ArgumentException>(
                    () => EnumType.Create("EnumName", "N", underlyingType, true, new[] { member }, null))
                    .Message);
        }

        [Fact]
        public void Create_throws_if_member_names_are_not_unique()
        {
            var underlyingType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);
            var members
                = new[]
                  {
                      EnumMember.Create("M", 1, null),
                      EnumMember.Create("M", 2, null)
                  };

            Assert.Equal(
                new ArgumentException(Strings.ItemDuplicateIdentity("M"), "item")
                    .Message,
                Assert.Throws<ArgumentException>(
                    () => EnumType.Create("EnumName", "N", underlyingType, true, members, null))
                    .Message);
        }

        [Fact]
        public void Can_set_and_get_underlying_type()
        {
            var enumType = new EnumType();

            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int64);

            enumType.UnderlyingType = primitiveType;

            Assert.Same(primitiveType, enumType.UnderlyingType);
        }
    }
}
