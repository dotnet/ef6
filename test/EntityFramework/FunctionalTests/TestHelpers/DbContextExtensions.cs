namespace System.Data.Entity
{
    using System.Data.Entity.Migrations;
    using System.Data.SqlServerCe;

    public static class DbContextExtensions
    {
        public static TypeAssertion<TStructuralType> Assert<TStructuralType>(this DbContext context)
            where TStructuralType : class
        {
            return new TypeAssertion<TStructuralType>(context);
        }

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