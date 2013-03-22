// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using Moq;
    using Xunit;

    public class DbMigrationsConfigurationTests : TestBase
    {
        private class TestMigrationsConfiguration : DbMigrationsConfiguration
        {
        }

        [Fact]
        public void Can_get_and_set_migration_context_properties()
        {
            var migrationsConfiguration = new TestMigrationsConfiguration
                {
                    AutomaticMigrationsEnabled = false,
                    CodeGenerator = new CSharpMigrationCodeGenerator(),
                    ContextType = typeof(ShopContext_v1)
                };

            Assert.False(migrationsConfiguration.AutomaticMigrationsEnabled);
            Assert.NotNull(migrationsConfiguration.CodeGenerator);
            Assert.NotNull(migrationsConfiguration.ContextType);
        }

        [Fact]
        public void Can_get_and_set_sql_generator()
        {
            var migrationsConfiguration = new TestMigrationsConfiguration();
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            migrationsConfiguration.SetSqlGenerator(DbProviders.Sql, migrationSqlGenerator);

            Assert.Same(migrationSqlGenerator, migrationsConfiguration.GetSqlGenerator(DbProviders.Sql));
        }

        [Fact]
        public void GetSqlGenerator_should_throw_when_no_generator_registered_for_provider()
        {
            var migrationsConfiguration = new TestMigrationsConfiguration();

            var exception = Assert.Throws<MigrationsException>(() => migrationsConfiguration.GetSqlGenerator("Foomatic"));

            Assert.Equal(Strings.NoSqlGeneratorForProvider("Foomatic"), exception.Message);
        }

        [Fact]
        public void Providers_are_assigned_by_default()
        {
            var migrationsConfiguration = new TestMigrationsConfiguration();

            Assert.NotNull(migrationsConfiguration.CodeGenerator);
            Assert.NotNull(migrationsConfiguration.GetSqlGenerator(DbProviders.Sql));
            Assert.NotNull(migrationsConfiguration.GetSqlGenerator(DbProviders.SqlCe));
        }

        [Fact]
        public void ContextKey_is_assigned_by_default()
        {
            var migrationsConfiguration = new TestMigrationsConfiguration();

            Assert.Equal(migrationsConfiguration.GetType().FullName, migrationsConfiguration.ContextKey);
        }

        [Fact]
        public void Can_get_and_set_context_key()
        {
            var migrationsConfiguration
                = new TestMigrationsConfiguration
                    {
                        ContextKey = "Foo"
                    };

            Assert.Equal("Foo", migrationsConfiguration.ContextKey);
        }

        [Fact]
        public void ContextKey_restricts_length_to_context_key_max_length()
        {
            var migrationsConfiguration
                = new TestMigrationsConfiguration
                    {
                        ContextKey = new string('a', 600)
                    };

            Assert.Equal(new string('a', HistoryContext.ContextKeyMaxLength), migrationsConfiguration.ContextKey);
        }

        [Fact]
        public void Properties_check_for_bad_arguments()
        {
            var config = new TestMigrationsConfiguration();

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentNullException>(() => config.CodeGenerator = null).ParamName);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("value"),
                Assert.Throws<ArgumentException>(() => config.ContextKey = null).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("value"),
                Assert.Throws<ArgumentException>(() => config.ContextKey = "").Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("value"),
                Assert.Throws<ArgumentException>(() => config.ContextKey = " ").Message);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentNullException>(() => config.ContextType = null).ParamName);

            Assert.Equal(
                Strings.DbMigrationsConfiguration_ContextType("Random"),
                Assert.Throws<ArgumentException>(() => config.ContextType = typeof(Random)).Message);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentNullException>(() => config.MigrationsAssembly = null).ParamName);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("value"),
                Assert.Throws<ArgumentException>(() => config.MigrationsDirectory = null).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("value"),
                Assert.Throws<ArgumentException>(() => config.MigrationsDirectory = "").Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("value"),
                Assert.Throws<ArgumentException>(() => config.MigrationsDirectory = " ").Message);

            Assert.Equal(
                Strings.DbMigrationsConfiguration_RootedPath(@"\Test"),
                Assert.Throws<MigrationsException>(() => config.MigrationsDirectory = @"\Test").Message);
        }

        [Fact]
        public void Can_set_MigrationsNamespace_to_null()
        {
            Assert.Null(
                new TestMigrationsConfiguration
                    {
                        MigrationsNamespace = null
                    }.MigrationsNamespace);
        }

        [Fact]
        public void Setting_HistoryContextFactory_to_null_results_in_default_factory_being_used()
        {
            var config = new TestMigrationsConfiguration();

            var factory = new Mock<IHistoryContextFactory>().Object;
            config.HistoryContextFactory = factory;
            Assert.Same(factory, config.HistoryContextFactory);

            config.HistoryContextFactory = null;
            Assert.IsType<DefaultHistoryContextFactory>(config.HistoryContextFactory);
        }

        [Fact]
        public void CommandTimeout_throws_for_negative_values()
        {
            var config = new TestMigrationsConfiguration();
            
            Assert.Equal(
                Strings.ObjectContext_InvalidCommandTimeout,
                Assert.Throws<ArgumentException>(
                    () => config.CommandTimeout = -1).Message);
        }
    }
}
