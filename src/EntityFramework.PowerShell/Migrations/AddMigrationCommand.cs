namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Resources;

    internal class AddMigrationCommand : MigrationsDomainCommand
    {
        public AddMigrationCommand(string name, bool force, bool ignoreChanges)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            Execute(
                () =>
                {
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
                            name, Project.GetLanguage(), Project.GetRootNamespace(), ignoreChanges);

                        var userCodeFileName = scaffoldedMigration.MigrationId + "." + scaffoldedMigration.Language;
                        var userCodePath = Path.Combine(scaffoldedMigration.Directory, userCodeFileName);
                        var designerCodeFileName = scaffoldedMigration.MigrationId + ".Designer."
                            + scaffoldedMigration.Language;
                        var designerCodePath = Path.Combine(scaffoldedMigration.Directory, designerCodeFileName);
                        var resourcesFileName = scaffoldedMigration.MigrationId + ".resx";
                        var resourcesPath = Path.Combine(scaffoldedMigration.Directory, resourcesFileName);

                        if (rescaffolding && !force)
                        {
                            var absoluteUserCodePath = Path.Combine(Project.GetProjectDir(), userCodePath);

                            if (!string.Equals(scaffoldedMigration.UserCode, File.ReadAllText(absoluteUserCodePath)))
                            {
                                WriteWarning(Strings.RescaffoldNoForce(name));
                            }
                        }
                        else
                        {
                            Project.AddFile(userCodePath, scaffoldedMigration.UserCode);
                        }

                        WriteResources(userCodePath, resourcesPath, scaffoldedMigration.Resources);
                        Project.AddFile(designerCodePath, scaffoldedMigration.DesignerCode);

                        if (!rescaffolding)
                        {
                            WriteWarning(Strings.SnapshotBehindWarning(scaffoldedMigration.MigrationId));
                        }

                        Project.OpenFile(userCodePath);
                    }
                });
        }

        private void WriteResources(string userCodePath, string resourcesPath, IDictionary<string, object> resources)
        {
            Contract.Requires(!string.IsNullOrEmpty(userCodePath));
            Contract.Requires(!string.IsNullOrEmpty(resourcesPath));
            Contract.Requires(resources != null);

            var absoluteResourcesPath = Path.Combine(Project.GetProjectDir(), resourcesPath);

            Project.EditFile(resourcesPath);

            using (var writer = new ResXResourceWriter(absoluteResourcesPath))
            {
                resources.Each(i => writer.AddResource(i.Key, i.Value));
            }

            Project.AddFile(resourcesPath);
        }
    }
}