namespace $namespace$
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<$contextType$>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = $enableAutomaticMigrations$;$migrationsDirectory$$contextKey$
        }

        protected override void Seed($contextType$ context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
        }
    }
}
