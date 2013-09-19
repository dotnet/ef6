// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.Resources;
    using System.Data.Entity.Migrations.Utilities;
    using System.Data.Entity.Utilities;
    using System.IO;
    using System.Linq;

    internal class AddMigrationCommand : MigrationsDomainCommand
    {
        public AddMigrationCommand()
        {
            // Testing
        }

        public AddMigrationCommand(string name, bool force, bool ignoreChanges)
        {
            DebugCheck.NotEmpty(name);

            Execute(() => Execute(name, force, ignoreChanges));
        }

        public void Execute(string name, bool force, bool ignoreChanges)
        {
            DebugCheck.NotEmpty(name);

            using (var facade = GetFacade())
            {
                var scaffoldedMigration
                    = facade.Scaffold(
                        name, Project.GetLanguage(), Project.GetRootNamespace(), ignoreChanges);

                WriteLine(
                    !scaffoldedMigration.IsRescaffold
                        ? Strings.ScaffoldingMigration(name)
                        : Strings.RescaffoldingMigration(name));

                var userCodePath
                    = WriteMigration(name, force, scaffoldedMigration, scaffoldedMigration.IsRescaffold);

                if (!scaffoldedMigration.IsRescaffold)
                {
                    WriteWarning(Strings.SnapshotBehindWarning(name));

                    var databaseMigrations
                        = facade.GetDatabaseMigrations().Take(2).ToList();

                    var lastDatabaseMigration = databaseMigrations.FirstOrDefault();

                    if ((lastDatabaseMigration != null)
                        && string.Equals(lastDatabaseMigration.MigrationName(), name, StringComparison.Ordinal))
                    {
                        var revertTargetMigration
                            = databaseMigrations.ElementAtOrDefault(1);

                        WriteWarning(
                            Environment.NewLine
                            + Strings.DidYouMeanToRescaffold(
                                name,
                                revertTargetMigration ?? "$InitialDatabase",
                                Path.GetFileName(userCodePath)));
                    }
                }

                Project.OpenFile(userCodePath);
            }
        }

        protected virtual string WriteMigration(string name, bool force, ScaffoldedMigration scaffoldedMigration, bool rescaffolding)
        {
            return new MigrationWriter(this).Write(scaffoldedMigration, rescaffolding, force, name);
        }
    }
}
