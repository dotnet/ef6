// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class EnumMemberTests
    {
        [Fact]
        public void Can_create_enumeration_member()
        {
            var stringTypeUsage = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var metadataProperty = new MetadataProperty("MetadataProperty", stringTypeUsage, "Value");

            var member = EnumMember.Create("MemberName", (sbyte)5, new[] { metadataProperty });

            Assert.Equal("MemberName", member.Name);
            Assert.Equal(5, (sbyte)member.Value);
            Assert.Same(metadataProperty, member.MetadataProperties.Last());
            Assert.True(member.IsReadOnly);

            member = EnumMember.Create("MemberName", (byte)5, new[] { metadataProperty });

            Assert.Equal("MemberName", member.Name);
            Assert.Equal(5, (byte)member.Value);
            Assert.Same(metadataProperty, member.MetadataProperties.Last());
            Assert.True(member.IsReadOnly);

            member = EnumMember.Create("MemberName", (short)5, new[] { metadataProperty });

            Assert.Equal("MemberName", member.Name);
            Assert.Equal(5, (short)member.Value);
            Assert.Same(metadataProperty, member.MetadataProperties.Last());
            Assert.True(member.IsReadOnly);

            member = EnumMember.Create("MemberName", (int)5, new[] { metadataProperty });

            Assert.Equal("MemberName", member.Name);
            Assert.Equal(5, (int)member.Value);
            Assert.Same(metadataProperty, member.MetadataProperties.Last());
            Assert.True(member.IsReadOnly);

            member = EnumMember.Create("MemberName", (long)5, new[] { metadataProperty });

            Assert.Equal("MemberName", member.Name);
            Assert.Equal(5, (long)member.Value);
            Assert.Same(metadataProperty, member.MetadataProperties.Last());
            Assert.True(member.IsReadOnly);
        }

        [Fact]
        public void Create_throws_if_name_is_null_or_empty()
        {
            var stringTypeUsage = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var metadataProperty = new MetadataProperty("MetadataProperty", stringTypeUsage, "Value");

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"), 
                Assert.Throws<ArgumentException>(
                    () => EnumMember.Create(null, (sbyte)5, new [] { metadataProperty }))
                    .Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(
                    () => EnumMember.Create(String.Empty, (sbyte)5, null))
                    .Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(
                    () => EnumMember.Create(null, (byte)5, null))
                    .Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(
                    () => EnumMember.Create(String.Empty, (byte)5, new[] { metadataProperty }))
                    .Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(
                    () => EnumMember.Create(null, (short)5, null))
                    .Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(
                    () => EnumMember.Create(String.Empty, (short)5, null))
                    .Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(
                    () => EnumMember.Create(null, (int)5, null))
                    .Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(
                    () => EnumMember.Create(String.Empty, (int)5, new[] { metadataProperty }))
                    .Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(
                    () => EnumMember.Create(null, (long)5, null))
                    .Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(
                    () => EnumMember.Create(String.Empty, (long)5, null))
                    .Message);
        }
    }
}
