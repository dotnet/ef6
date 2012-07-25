// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using Xunit;

    public class DbMigrationsConfigurationTests
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
    }
}