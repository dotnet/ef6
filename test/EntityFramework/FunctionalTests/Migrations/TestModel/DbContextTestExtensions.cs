namespace System.Data.Entity.Migrations
{
    using System.Data.SqlServerCe;

    public static class DbContextTestExtensions
    {
        public static void IgnoreSpatialTypesOnSqlCe(this DbContext context, DbModelBuilder modelBuilder)
        {
            if (context.Database.Connection is SqlCeConnection)
            {
                modelBuilder.Entity<MigrationsStore>().Ignore(e => e.Location);
                modelBuilder.Entity<MigrationsStore>().Ignore(e => e.FloorPlan);
            }
        }
    }
}
