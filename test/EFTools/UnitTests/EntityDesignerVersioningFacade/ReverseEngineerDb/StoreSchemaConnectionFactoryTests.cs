// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using SystemDataCommon = System.Data.Common;

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb
{
    using System;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.SqlServer;
    using System.IO;
    using System.Security;
    using System.Xml;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class StoreSchemaConnectionFactoryTests
    {
        [Fact]
        public void Create_creates_valid_EntityConnection_and_returns_EF_version()
        {
            var mockProviderServices = SetupMockProviderServices();
            mockProviderServices
                .Protected()
                .Setup<DbProviderManifest>("GetDbProviderManifest", ItExpr.IsAny<string>())
                .Returns(SqlProviderServices.Instance.GetProviderManifest("2008"));

            var mockResolver = SetupMockResolver(mockProviderServices);

            foreach (var targetEFVersion in EntityFrameworkVersion.GetAllVersions())
            {
                Version actualEFVersion;

                var entityConnection =
                    new StoreSchemaConnectionFactory().Create(
                        mockResolver.Object,
                        "System.Data.SqlClient",
                        "Server=test",
                        targetEFVersion,
                        out actualEFVersion);

                Assert.NotNull(entityConnection);
                var expectedVersion =
                    targetEFVersion == EntityFrameworkVersion.Version2
                        ? EntityFrameworkVersion.Version1
                        : targetEFVersion;

                Assert.Equal(expectedVersion, actualEFVersion);
            }
        }

        [Fact]
        public void Create_throws_ArgumentException_for_unrecognized_porvider_invariant_name()
        {
            const string providerInvariantName = "abc";

            Assert.Equal(
                string.Format(Resources_VersioningFacade.EntityClient_InvalidStoreProvider, providerInvariantName),
                Assert.Throws<ArgumentException>(
                    () => new StoreSchemaConnectionFactory().Create(
                        new Mock<IDbDependencyResolver>().Object,
                        providerInvariantName,
                        "connectionString",
                        new Version(1, 0, 0, 0))).Message);
        }

        [Fact]
        public void Create_creates_valid_EntityConnection()
        {
            var mockProviderServices = SetupMockProviderServices();
            mockProviderServices
                .Protected()
                .Setup<DbProviderManifest>("GetDbProviderManifest", ItExpr.IsAny<string>())
                .Returns(SqlProviderServices.Instance.GetProviderManifest("2008"));

            var mockResolver = SetupMockResolver(mockProviderServices);

            foreach (var efVersion in EntityFrameworkVersion.GetAllVersions())
            {
                var entityConnection =
                    new StoreSchemaConnectionFactory()
                        .Create(
                            mockResolver.Object,
                            "System.Data.SqlClient",
                            "Server=test",
                            efVersion);

                Assert.NotNull(entityConnection);
            }
        }

        [Fact]
        public void Create_throws_ProviderIncompatibleException_for_invalid_schema_Ssdl()
        {
            var mockProviderServices = SetupMockProviderServices();
            var mockProviderManifest = new Mock<DbProviderManifest>();
            mockProviderManifest
                .Protected()
                .Setup<XmlReader>("GetDbInformation", ItExpr.Is<string>(s => s == DbProviderManifest.StoreSchemaDefinitionVersion3))
                .Returns(XmlReader.Create(new StringReader("<root />")));

            mockProviderServices
                .Protected()
                .Setup<DbProviderManifest>("GetDbProviderManifest", ItExpr.IsAny<string>())
                .Returns(mockProviderManifest.Object);

            var mockResolver = SetupMockResolver(mockProviderServices);

            Assert.True(
                Assert.Throws<ProviderIncompatibleException>(
                    () => new StoreSchemaConnectionFactory().Create(
                        mockResolver.Object,
                        "System.Data.SqlClient",
                        "Server=test",
                        EntityFrameworkVersion.Version3)).Message.StartsWith("Schema specified is not valid. Errors"));
        }

        [Fact]
        public void Create_throws_ProviderIncompatibleException_for_invalid_schema_Msl()
        {
            var mockProviderServices = SetupMockProviderServices();
            var mockProviderManifest = new Mock<DbProviderManifest>();

            mockProviderManifest
                .Protected()
                .Setup<XmlReader>("GetDbInformation", ItExpr.Is<string>(s => s != DbProviderManifest.StoreSchemaMappingVersion3))
                .Returns(
                    SqlProviderServices.Instance.GetProviderManifest("2008").GetInformation(DbProviderManifest.StoreSchemaMappingVersion3));

            mockProviderManifest
                .Protected()
                .Setup<XmlReader>("GetDbInformation", ItExpr.Is<string>(s => s == DbProviderManifest.StoreSchemaMappingVersion3))
                .Returns(XmlReader.Create(new StringReader("<root />")));

            mockProviderServices
                .Protected()
                .Setup<DbProviderManifest>("GetDbProviderManifest", ItExpr.IsAny<string>())
                .Returns(mockProviderManifest.Object);

            var mockResolver = SetupMockResolver(mockProviderServices);

            Assert.True(
                Assert.Throws<ProviderIncompatibleException>(
                    () => new StoreSchemaConnectionFactory().Create(
                        mockResolver.Object,
                        "System.Data.SqlClient",
                        "Server=test",
                        EntityFrameworkVersion.Version3)).Message.StartsWith("Schema specified is not valid. Errors"));
        }

        private static Mock<DbProviderServices> SetupMockProviderServices()
        {
            var mockProviderServices = new Mock<DbProviderServices>();
            mockProviderServices
                .Protected()
                .Setup<string>("GetDbProviderManifestToken", ItExpr.IsAny<SystemDataCommon.DbConnection>())
                .Returns("2008");

            return mockProviderServices;
        }

        private static Mock<IDbDependencyResolver> SetupMockResolver(Mock<DbProviderServices> mockProviderServices)
        {
            var mockResolver = new Mock<IDbDependencyResolver>();
            mockResolver
                .Setup(
                    r => r.GetService(
                        It.Is<Type>(t => t == typeof(DbProviderServices)),
                        It.IsAny<string>()))
                .Returns(mockProviderServices.Object);
            return mockResolver;
        }

        [Fact]
        public void IsCatchableExceptionType_filters_exceptions_correctly()
        {
            Assert.False(StoreSchemaConnectionFactory.IsCatchableExceptionType(new StackOverflowException()));
            Assert.False(StoreSchemaConnectionFactory.IsCatchableExceptionType(new OutOfMemoryException()));
            Assert.False(StoreSchemaConnectionFactory.IsCatchableExceptionType(new NullReferenceException()));
            Assert.False(StoreSchemaConnectionFactory.IsCatchableExceptionType(new AccessViolationException()));
            Assert.False(StoreSchemaConnectionFactory.IsCatchableExceptionType(new SecurityException()));

            Assert.True(StoreSchemaConnectionFactory.IsCatchableExceptionType(new Exception()));
            Assert.True(StoreSchemaConnectionFactory.IsCatchableExceptionType(new InvalidOperationException()));
        }
    }
}
