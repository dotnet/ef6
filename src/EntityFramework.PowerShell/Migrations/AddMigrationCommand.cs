// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.Resources;
    using System.Data.Entity.Migrations.Utilities;
    using System.Diagnostics.Contracts;
    using System.Linq;

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
                            var userCodePath = new MigrationWriter(this)
                                .Write(
                                    scaffoldedMigration,
                                    rescaffolding,
                                    force,
                                    name);

                            if (!rescaffolding)
                            {
                                WriteWarning(Strings.SnapshotBehindWarning(scaffoldedMigration.MigrationId));
                            }

                            Project.OpenFile(userCodePath);
                        }
                    });
        }
    }
}
