// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.SqlClient;
    using Moq;
    using Xunit;

    public class DatabaseCreatorTests
    {
        public class CreateDatabase : TestBase
        {
            [Fact]
            public void CreateDatabase_uses_core_provider_when_not_in_code_first_mode()
            {
                var mockOperations = new Mock<DatabaseOperations>();
                var mockContext = CreateMockContextForMigrator(mockOperations, codeFirst: false);

                new DatabaseCreator().CreateDatabase(
                    mockContext.Object,
                    (config, context) =>
                        {
                            Assert.True(false);
                            return null;
                        },
                    null);

                mockOperations.Verify(m => m.Create(null), Times.Once());
                mockContext.Verify(m => m.SaveMetadataToDatabase(), Times.Once());
                mockContext.Verify(m => m.MarkDatabaseInitialized(), Times.Once());
            }

            [Fact]
            public void CreateDatabase_uses_core_provider_when_provider_is_not_SQL_Server_or_SQL_CE()
            {
                var mockOperations = new Mock<DatabaseOperations>();
                var mockContext = CreateMockContextForMigrator(mockOperations);
                mockContext.Setup(m => m.ProviderName).Returns("Some.Other.Provider");

                new DatabaseCreator().CreateDatabase(
                    mockContext.Object,
                    (config, context) =>
                        {
                            Assert.True(false);
                            return null;
                        },
                    null);

                mockOperations.Verify(m => m.Create(null), Times.Once());
                mockContext.Verify(m => m.SaveMetadataToDatabase(), Times.Once());
                mockContext.Verify(m => m.MarkDatabaseInitialized(), Times.Once());
            }

            [Fact]
            public void CreateDatabase_uses_Migrations_when_provider_is_SQL_Server()
            {
                CreateDatabase_uses_Migrations_when_provider_is_known("System.Data.SqlClient");
            }

            [Fact]
            public void CreateDatabase_uses_Migrations_when_provider_is_SQL_CE()
            {
                CreateDatabase_uses_Migrations_when_provider_is_known("System.Data.SqlServerCe.4.0");
            }

            [Fact]
            public void CreateDatabase_uses_Migrations_when_provider_when_SQL_generator_has_been_registered()
            {
                var mockMigrationsResolver = new Mock<IDbDependencyResolver>();
                mockMigrationsResolver
                    .Setup(m => m.GetService(typeof(Func<MigrationSqlGenerator>), "FooClient"))
                    .Returns(() => (Func<MigrationSqlGenerator>)(() => new Mock<MigrationSqlGenerator>().Object));

                CreateDatabase_uses_Migrations_when_provider_is_known("FooClient", mockMigrationsResolver.Object);
            }

            private void CreateDatabase_uses_Migrations_when_provider_is_known(
                string provider, IDbDependencyResolver resolver = null, bool migrationsConfigured = false)
            {
                var mockOperations = new Mock<DatabaseOperations>();
                var mockContext = CreateMockContextForMigrator(mockOperations);
                mockContext.Setup(m => m.ProviderName).Returns(provider);
                mockContext.Setup(m => m.MigrationsConfigurationDiscovered).Returns(migrationsConfigured);

                Mock<MigratorBase> mockMigrator = null;

                new DatabaseCreator(resolver ?? DbConfiguration.DependencyResolver)
                    .CreateDatabase(
                        mockContext.Object,
                        (config, context) => (mockMigrator = new Mock<MigratorBase>(null)).Object,
                        null);

                mockMigrator.Verify(m => m.Update(null), Times.Once());
                mockOperations.Verify(m => m.Create(null), Times.Never());
                mockContext.Verify(m => m.SaveMetadataToDatabase(), Times.Never());
                mockContext.Verify(m => m.MarkDatabaseInitialized(), Times.Once());
            }

            [Fact] // CodePlex 1192
            public void CreateDatabase_uses_Migrations_when_Migrations_is_configured()
            {
                var mockMigrationsResolver = new Mock<IDbDependencyResolver>();
                mockMigrationsResolver
                    .Setup(m => m.GetService(typeof(Func<MigrationSqlGenerator>), "FooClient"))
                    .Returns(() => (Func<MigrationSqlGenerator>)(() => new Mock<MigrationSqlGenerator>().Object));

                CreateDatabase_uses_Migrations_when_provider_is_known(
                    "FooClient", mockMigrationsResolver.Object, migrationsConfigured: true);
            }

            [Fact]
            public void CreateDatabase_using_Migrations_configures_a_migrator_appropriately()
            {
                DbMigrationsConfiguration configuration = null;

                new DatabaseCreator().CreateDatabase(
                    CreateMockContextForMigrator().Object,
                    (config, context) =>
                        {
                            configuration = config;
                            return new Mock<DbMigrator>(config, context, DatabaseExistenceState.Unknown, true).Object;
                        },
                    null);

                Assert.True(typeof(FakeContext).IsAssignableFrom(configuration.ContextType));
                Assert.True(configuration.AutomaticMigrationsEnabled);
                Assert.Equal(
                    "Database=Foo",
                    configuration.TargetDatabase.GetConnectionString(AppConfig.DefaultInstance).ConnectionString);
                Assert.Equal("System.Data.Entity.Internal.DatabaseCreatorTests+FakeContext", configuration.ContextKey);
            }

            [Fact]
            public void CreateDatabase_using_Migrations_calls_Update_on_a_migrator_and_marks_database_as_initialized()
            {
                var mockContext = CreateMockContextForMigrator();
                Mock<DbMigrator> mockMigrator = null;

                new DatabaseCreator().CreateDatabase(
                    mockContext.Object,
                    (config, context) => (mockMigrator = new Mock<DbMigrator>(config, context, DatabaseExistenceState.Unknown, true)).Object,
                    null);

                mockMigrator.Verify(m => m.Update(null), Times.Once());
                mockContext.Verify(m => m.MarkDatabaseInitialized(), Times.Once());
            }

            [Fact]
            public void CreateDatabase_using_Migrations_does_not_suppress_initializers()
            {
                var internalContext = CreateMockContextForMigrator().Object;

                DbContextInfo.CurrentInfo = null;
                
                new DatabaseCreator().CreateDatabase(
                    internalContext,
                    (config, context) => new Mock<DbMigrator>(config, context, DatabaseExistenceState.Unknown, true).Object,
                    null);

                Assert.Null(DbContextInfo.CurrentInfo);
            }

            [Fact]
            public void CreateDatabase_using_Migrations_does_not_dispose_users_context()
            {
                var mockContext = CreateMockContextForMigrator();

                new DatabaseCreator().CreateDatabase(
                    mockContext.Object,
                    (config, context) => new Mock<DbMigrator>(config, context, DatabaseExistenceState.Unknown, true).Object,
                    null);

                Assert.False(mockContext.Object.IsDisposed);
            }

            private static Mock<InternalContextForMock<FakeContext>> CreateMockContextForMigrator(
                Mock<DatabaseOperations> mockOperations = null, bool codeFirst = true)
            {
                var mockCompiledModel = new Mock<DbCompiledModel>();
                mockCompiledModel.Setup(m => m.CachedModelBuilder).Returns(new DbModelBuilder());

                var mockContext = new Mock<InternalContextForMock<FakeContext>>();
                mockContext.Setup(m => m.CodeFirstModel).Returns(codeFirst ? mockCompiledModel.Object : null);
                mockContext.Setup(m => m.OriginalConnectionString).Returns("Database=Foo");
                mockContext.Setup(m => m.ProviderName).Returns("System.Data.SqlClient");
                mockContext.Setup(m => m.ModelProviderInfo).Returns(ProviderRegistry.Sql2008_ProviderInfo);
                mockContext.Setup(m => m.Connection).Returns(new SqlConnection());
                mockContext.Setup(m => m.OwnerShortTypeName).Returns(typeof(FakeContext).ToString());

                if (mockOperations != null)
                {
                    mockContext.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);
                }

                return mockContext;
            }
        }

        public class FakeContext : DbContext
        {
            static FakeContext()
            {
                Database.SetInitializer<FakeContext>(null);
            }
        }
    }
}
