// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Config;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.SqlServerCompact;
    using DaFunc;
    using Moq;
    using Xunit;

    public class DbMigrationsConfigurationOfTTests
    {
        [Fact]
        public void Can_get_and_set_migration_context_properties()
        {
            var migrationsConfiguration = new DbMigrationsConfiguration
                                              {
                                                  AutomaticMigrationsEnabled = false,
                                                  ContextType = typeof(ShopContext_v1),
                                                  CodeGenerator = new Mock<MigrationCodeGenerator>().Object
                                              };

            Assert.False(migrationsConfiguration.AutomaticMigrationsEnabled);
            Assert.NotNull(migrationsConfiguration.CodeGenerator);
            Assert.Equal(typeof(ShopContext_v1), migrationsConfiguration.ContextType);
        }

        [Fact]
        public void Can_add_and_get_sql_generator()
        {
            var migrationsConfiguration = new DbMigrationsConfiguration();
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            migrationsConfiguration.SetSqlGenerator(DbProviders.Sql, migrationSqlGenerator);

            Assert.Same(migrationSqlGenerator, migrationsConfiguration.GetSqlGenerator(DbProviders.Sql));
        }

        [Fact]
        public void SQL_generator_is_obtained_from_migrations_configuration_if_set_in_migrations_configuration()
        {
            SetSqlGeneratorTest(setGenerator: true);
        }

        [Fact]
        public void SQL_generator_is_obtained_from_DbConfiguration_if_not_set_in_migrations_configuration()
        {
            SetSqlGeneratorTest(setGenerator: false);
        }

        private static void SetSqlGeneratorTest(bool setGenerator)
        {
            var generator = new Mock<MigrationSqlGenerator>().Object;
            var mockResolver = new Mock<IDbDependencyResolver>();
            mockResolver.Setup(m => m.GetService(typeof(MigrationSqlGenerator), "Gu.Hu.Ha")).Returns(generator);

            var migrationsConfiguration = new DbMigrationsConfiguration(new Lazy<IDbDependencyResolver>(() => mockResolver.Object));

            if (setGenerator)
            {
                generator = new Mock<MigrationSqlGenerator>().Object;
                migrationsConfiguration.SetSqlGenerator("Gu.Hu.Ha", generator);
            }

            Assert.Same(generator, migrationsConfiguration.GetSqlGenerator("Gu.Hu.Ha"));
        }

        [Fact]
        public void Setting_SQL_generator_does_not_change_generator_set_in_DbConfiguration()
        {
            new DbMigrationsConfiguration().SetSqlGenerator(DbProviders.SqlCe, new SqlServerMigrationSqlGenerator());

            Assert.IsType<SqlCeMigrationSqlGenerator>(
                DbConfiguration.GetService<MigrationSqlGenerator>(DbProviders.SqlCe));
        }

        private class TestMigrationsConfiguration<TContext> : DbMigrationsConfiguration<TContext>
            where TContext : DbContext
        {
        }

        [Fact]
        public void ContextKey_is_assigned_to_short_full_name_by_default()
        {
            var migrationsConfiguration
                = new TestMigrationsConfiguration<GT<NT, NT>.GenericFuncy<GT<GT<NT, NT>, NT>, NT>>();

            Assert.Equal(migrationsConfiguration.GetType().ToString(), migrationsConfiguration.ContextKey);
        }
    }
}
