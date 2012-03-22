namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Resources;
    using System.Linq;

    internal class GetMigrationsCommand : MigrationsDomainCommand
    {
        public GetMigrationsCommand()
        {
            Execute(
                () =>
                    {
                        using (var facade = GetFacade())
                        {
                            WriteLine(Strings.GetMigrationsCommand_Intro);

                            var migrations = facade.GetDatabaseMigrations();

                            if (migrations.Any())
                            {
                                foreach (var migration in migrations)
                                {
                                    WriteLine(migration);
                                }
                            }
                            else
                            {
                                WriteLine(Strings.GetMigrationsCommand_NoHistory);
                            }
                        }
                    });
        }
    }
}
