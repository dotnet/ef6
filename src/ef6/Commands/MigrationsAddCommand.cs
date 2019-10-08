// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using Microsoft.DotNet.Cli.CommandLine;

using MyResources = System.Data.Entity.Tools.Properties.Resources;

namespace System.Data.Entity.Tools.Commands
{
    internal class MigrationsAddCommand : MigrationsCommandBase
    {
        private CommandArgument _name;
        private CommandOption _force;
        private CommandOption _ignoreChanges;
        private CommandOption _json;

        public override void Configure(CommandLineApplication command)
        {
            command.Description = MyResources.MigrationsAddDescription;

            _name = command.Argument("<NAME>", MyResources.MigrationNameDescription);
            _force = command.Option("-f|--force", MyResources.MigrationsAddForceDescription);
            _ignoreChanges = command.Option("--ignore-changes", MyResources.IgnoreChangesDescription);
            _json = Json.ConfigureOption(command);

            base.Configure(command);
        }

        protected override void Validate()
        {
            base.Validate();

            if (string.IsNullOrEmpty(_name.Value))
            {
                throw new CommandException(string.Format(MyResources.MissingArgument, _name.Name));
            }
        }

        protected override int Execute()
        {
            var scaffoldedMigration = CreateExecutor().Scaffold(
                _name.Value,
                ConnectionStringName.Value(),
                ConnectionString.Value(),
                ConnectionProvider.Value(),
                MigrationsConfig.Value(),
                _ignoreChanges.HasValue());

            Reporter.WriteInformation(
                string.Format(
                    !scaffoldedMigration.IsRescaffold
                        ? MyResources.ScaffoldingMigration
                        : MyResources.RescaffoldingMigration,
                    _name.Value));

            var userCodePath
                = WriteMigration(scaffoldedMigration, scaffoldedMigration.IsRescaffold, _force.HasValue(), _name.Value);

            if (!scaffoldedMigration.IsRescaffold)
            {
                Reporter.WriteWarning(string.Format(MyResources.SnapshotBehindWarning, _name.Value));

                var databaseMigrations
                    = CreateExecutor().GetDatabaseMigrations(
                            ConnectionStringName.Value(),
                            ConnectionString.Value(),
                            ConnectionProvider.Value(),
                            MigrationsConfig.Value())
                        .Take(2).ToList();

                var lastDatabaseMigration = databaseMigrations.FirstOrDefault();

                if ((lastDatabaseMigration != null)
                    && string.Equals(lastDatabaseMigration.MigrationName(), _name.Value, StringComparison.Ordinal))
                {
                    var revertTargetMigration
                        = databaseMigrations.ElementAtOrDefault(1);

                    Reporter.WriteWarning(
                        string.Format(
                            MyResources.DidYouMeanToRescaffold,
                            _name.Value,
                            revertTargetMigration ?? "$InitialDatabase",
                            Path.GetFileName(userCodePath)));
                }
            }

            if (_json.HasValue())
            {
                string migrationPath = null;
                string migrationDesignerPath = null;
                string migrationResourcesPath = null;
                if (scaffoldedMigration != null)
                {
                    var absoluteMigrationsDirectory = Path.Combine(ProjectDir.Value(), scaffoldedMigration.Directory);

                    migrationPath = Path.Combine(
                        absoluteMigrationsDirectory,
                        scaffoldedMigration.MigrationId + "." + scaffoldedMigration.Language);
                    migrationDesignerPath = Path.Combine(
                        absoluteMigrationsDirectory,
                        scaffoldedMigration.MigrationId + ".resx");
                    migrationResourcesPath = Path.Combine(
                        absoluteMigrationsDirectory,
                        scaffoldedMigration.MigrationId + ".Designer." + scaffoldedMigration.Language);
                }

                Reporter.WriteData("{");
                Reporter.WriteData("  \"migration\": " + Json.Literal(migrationPath) + ",");
                Reporter.WriteData("  \"migrationResources\": " + Json.Literal(migrationDesignerPath) + ",");
                Reporter.WriteData("  \"migrationDesigner\": " + Json.Literal(migrationResourcesPath));
                Reporter.WriteData("}");
            }

            return base.Execute();
        }
    }
}
