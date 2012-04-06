namespace System.Data.Entity.Migrations
{
    using System.Linq;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    // TODO: SDE Merge - No CE Provider
    //[Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class SeedingScenarios : DbTestCase
    {
        private class SeedingMigrationsConfiguration : DbMigrationsConfiguration<ShopContext_v1>
        {
            public SeedingMigrationsConfiguration()
            {
                AutomaticMigrationsEnabled = true;
            }

            protected override void Seed(ShopContext_v1 context)
            {
                context.Products.Add(new MigrationsProduct { Name = "Foomatic 1000", ProductId = 123, Sku = "BAR123" });
            }
        }

        [MigrationsTheory]
        public void Can_seed_database_when_database_up_to_date()
        {
            ResetDatabase();

            var migrationsConfiguration = new SeedingMigrationsConfiguration();

            ConfigureMigrationsConfiguration(migrationsConfiguration);

            var migrator = new DbMigrator(migrationsConfiguration);

            migrator.Update();

            using (var context = CreateContext<ShopContext_v1>())
            {
                Assert.Equal(1, context.Products.Count());
            }
        }
    }
}