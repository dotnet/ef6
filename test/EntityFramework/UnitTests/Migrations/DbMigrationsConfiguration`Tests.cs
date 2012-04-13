namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Sql;
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
    }
}