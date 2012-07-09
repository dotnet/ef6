namespace System.Data.Entity.Config
{
    using System.Data.Entity.Migrations.Sql;
    using Moq;
    using Xunit;

    public class MigrationsConfigurationResolverTests
    {
        [Fact]
        public void SQL_Server_and_SQL_compact_generators_are_registered_by_default()
        {
            var resolver = new MigrationsConfigurationResolver();

            Assert.IsType<SqlServerMigrationSqlGenerator>(resolver.GetService<MigrationSqlGenerator>("System.Data.SqlClient"));
            Assert.IsType<SqlCeMigrationSqlGenerator>(resolver.GetService<MigrationSqlGenerator>("System.Data.SqlServerCe.4.0"));
        }

        [Fact]
        public void A_new_SQL_generator_can_be_added()
        {
            var resolver = new MigrationsConfigurationResolver();

            var generator = new Mock<MigrationSqlGenerator>().Object;
            resolver.SetSqlGenerator("Captain.Slow", generator);

            Assert.Same(generator, resolver.GetService<MigrationSqlGenerator>("Captain.Slow"));
        }

        [Fact]
        public void An_existing_SQL_generator_can_be_replaced()
        {
            var resolver = new MigrationsConfigurationResolver();

            var generator = new Mock<MigrationSqlGenerator>().Object;
            resolver.SetSqlGenerator("System.Data.SqlServerCe.4.0", generator);

            Assert.Same(generator, resolver.GetService<MigrationSqlGenerator>("System.Data.SqlServerCe.4.0"));
        }

        [Fact]
        public void Release_does_not_throw()
        {
            new MigrationsConfigurationResolver().Release(new object());
        }
    }
}
