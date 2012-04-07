namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.Resources;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;

    internal class AddMigrationCommand : MigrationsDomainCommand
    {
        public AddMigrationCommand(string name, bool force, bool ignoreChanges)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            Execute(
                () =>
                    {
                        var project = Project;
                        var rescaffolding = false;

                        using (var facade = GetFacade())
                        {
                            var pendingMigrations = facade.GetPendingMigrations();

                            if (pendingMigrations.Any())
                            {
                                var lastMigration = pendingMigrations.Last();

                                if (!string.Equals(lastMigration, name, StringComparison.OrdinalIgnoreCase)
                                    &&
                                    !string.Equals(
                                        lastMigration.MigrationName(), name, StringComparison.OrdinalIgnoreCase))
                                {
                                    throw Error.MigrationsPendingException(pendingMigrations.Join());
                                }

                                rescaffolding = true;
                                name = lastMigration;
                            }

                            WriteLine(Strings.LoggingGenerate(name));

                            var scaffoldedMigration = facade.Scaffold(
                                name, project.GetLanguage(), project.GetRootNamespace(), ignoreChanges);

                            var userCodeFileName = scaffoldedMigration.MigrationId + "." + scaffoldedMigration.Language;
                            var userCodePath = Path.Combine(scaffoldedMigration.Directory, userCodeFileName);
                            var designerCodeFileName = scaffoldedMigration.MigrationId + ".Designer."
                                                       + scaffoldedMigration.Language;
                            var designerCodePath = Path.Combine(scaffoldedMigration.Directory, designerCodeFileName);

                            if (rescaffolding && !force)
                            {
                                var absoluteUserCodePath = Path.Combine(project.GetProjectDir(), userCodePath);

                                if (!string.Equals(scaffoldedMigration.UserCode, File.ReadAllText(absoluteUserCodePath)))
                                {
                                    WriteWarning(Strings.RescaffoldNoForce(name));
                                }
                            }
                            else
                            {
                                project.AddFile(userCodePath, scaffoldedMigration.UserCode);
                            }

                            project.AddFile(designerCodePath, scaffoldedMigration.DesignerCode);

                            if (!rescaffolding)
                            {
                                WriteWarning(Strings.SnapshotBehindWarning(scaffoldedMigration.MigrationId));
                            }

                            project.OpenFile(userCodePath);
                        }
                    });
        }
    }
}
