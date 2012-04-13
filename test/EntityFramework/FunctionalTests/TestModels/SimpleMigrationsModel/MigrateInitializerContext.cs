namespace FunctionalTests.SimpleMigrationsModel
{
    using System.Data.Entity;

    public class MigrateInitializerContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
    }
}
