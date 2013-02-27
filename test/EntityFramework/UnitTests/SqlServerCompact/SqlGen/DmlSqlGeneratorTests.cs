// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact.SqlGen
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Text;
    using Moq;
    using Xunit;

    public class DmlSqlGeneratorTests
    {
        [Fact]
        public void HandlIdentity_adds_cast_for_identity_int_columns()
        {
            Assert.Equal("CAST (@@IDENTITY AS int)", HandlIdentity_adds_cast_for_identity_columns(PrimitiveTypeKind.Int32));
        }

        [Fact]
        public void HandlIdentity_adds_cast_for_identity_bigint_columns()
        {
            Assert.Equal("CAST (@@IDENTITY AS bigint)", HandlIdentity_adds_cast_for_identity_columns(PrimitiveTypeKind.Int64));
        }

        private string HandlIdentity_adds_cast_for_identity_columns(PrimitiveTypeKind primitiveTypeKind)
        {
            var typeUsage = ProviderRegistry.SqlCe4_ProviderManifest.GetStoreType(
                TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(primitiveTypeKind)));

            var mockMember = new Mock<EdmMember>();
            mockMember.Setup(m => m.TypeUsage).Returns(typeUsage);

            var mockTransalator = new Mock<DmlSqlGenerator.ExpressionTranslator>();
            mockTransalator.Setup(m => m.MemberValues).Returns(new Dictionary<EdmMember, DbParameter>());

            var builder = new StringBuilder();
            DmlSqlGenerator.HandleIdentity(builder, mockTransalator.Object, mockMember.Object, false, new Mock<EntitySetBase>().Object);

            return builder.ToString();
        }

        [Fact]
        public void HandlIdentity_does_not_add_cast_if_member_value_exists()
        {
            var typeUsage = ProviderRegistry.SqlCe4_ProviderManifest.GetStoreType(
                TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)));

            var mockMember = new Mock<EdmMember>();
            mockMember.Setup(m => m.TypeUsage).Returns(typeUsage);

            var mockParameter = new Mock<DbParameter>();
            mockParameter.Setup(m => m.ParameterName).Returns("A Compact Unicorn");

            var mockTransalator = new Mock<DmlSqlGenerator.ExpressionTranslator>();
            var members = new Dictionary<EdmMember, DbParameter>
                {
                    { mockMember.Object, mockParameter.Object }
                };

            mockTransalator.Setup(m => m.MemberValues).Returns(members);

            var builder = new StringBuilder();
            DmlSqlGenerator.HandleIdentity(builder, mockTransalator.Object, mockMember.Object, false, new Mock<EntitySetBase>().Object);

            Assert.Equal("A Compact Unicorn", builder.ToString());
        }

        [Fact]
        public void HandlIdentity_throws_for_unsupported_identity_type()
        {
            var typeUsage = ProviderRegistry.SqlCe4_ProviderManifest.GetStoreType(
                TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Guid)));

            var mockMember = new Mock<EdmMember>();
            mockMember.Setup(m => m.TypeUsage).Returns(typeUsage);
            mockMember.Setup(m => m.Name).Returns("Cheese");

            var mockTransalator = new Mock<DmlSqlGenerator.ExpressionTranslator>();
            mockTransalator.Setup(m => m.MemberValues).Returns(new Dictionary<EdmMember, DbParameter>());

            var builder = new StringBuilder();

            Assert.Equal(
                ADP1.Update_NotSupportedIdentityType("Cheese", "SqlServerCe.uniqueidentifier"),
                Assert.Throws<InvalidOperationException>(
                    () => DmlSqlGenerator.HandleIdentity(
                        builder, mockTransalator.Object, mockMember.Object, false, new Mock<EntitySetBase>().Object)).Message);
        }

        [Fact]
        public void HandlIdentity_throws_for_multiple_identity_type()
        {
            var typeUsage = ProviderRegistry.SqlCe4_ProviderManifest.GetStoreType(
                TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)));

            var mockMember = new Mock<EdmMember>();
            mockMember.Setup(m => m.TypeUsage).Returns(typeUsage);

            var mockTransalator = new Mock<DmlSqlGenerator.ExpressionTranslator>();
            mockTransalator.Setup(m => m.MemberValues).Returns(new Dictionary<EdmMember, DbParameter>());

            var builder = new StringBuilder();

            var mockSet = new Mock<EntitySetBase>();
            mockSet.Setup(m => m.Name).Returns("Pickle");

            Assert.Equal(
                ADP1.Update_NotSupportedServerGenKey("Pickle"),
                Assert.Throws<NotSupportedException>(
                    () => DmlSqlGenerator.HandleIdentity(
                        builder, mockTransalator.Object, mockMember.Object, true, mockSet.Object)).Message);
        }
    }
}
