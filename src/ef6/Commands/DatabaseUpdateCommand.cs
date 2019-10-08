// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.DotNet.Cli.CommandLine;
using MyResources = System.Data.Entity.Tools.Properties.Resources;

namespace System.Data.Entity.Tools.Commands
{
    internal class DatabaseUpdateCommand : MigrationsCommandBase
    {
        private CommandOption _sourceMigration;
        private CommandOption _targetMigration;
        private CommandOption _script;
        private CommandOption _force;

        public override void Configure(CommandLineApplication command)
        {
            command.Description = MyResources.DatabaseUpdateDescription;

            _sourceMigration = command.Option("--source <MIGRATION>", MyResources.DatabaseUpdateSourceDescription);
            _targetMigration = command.Option("--target <MIGRATION>", MyResources.DatabaseUpdateTargetDescription);
            _script = command.Option("--script", MyResources.DatabaseUpdateScriptDescription);
            _force = command.Option("-f|--force", MyResources.DatabaseUpdateForceDescription);

            base.Configure(command);
        }

        protected override int Execute()
        {
            if (_script.HasValue())
            {
                var sql = CreateExecutor().ScriptUpdate(
                    _sourceMigration.Value(),
                    _targetMigration.Value(),
                    _force.HasValue(),
                    ConnectionStringName.Value(),
                    ConnectionString.Value(),
                    ConnectionProvider.Value(),
                    MigrationsConfig.Value());

                Reporter.WriteData(sql);
            }
            else
            {
                Reporter.WriteInformation(MyResources.UpdateDatabaseCommand_VerboseInstructions);

                try
                {
                    CreateExecutor().Update(
                        _targetMigration.Value(),
                        _force.HasValue(),
                        ConnectionStringName.Value(),
                        ConnectionString.Value(),
                        ConnectionProvider.Value(),
                        MigrationsConfig.Value());
                }
                catch (WrappedException ex)
                {
                    if (ex.Type == "System.Data.Entity.Migrations.Infrastructure.AutomaticMigrationsDisabledException")
                    {
                        Reporter.WriteWarning(ex.Message);
                        Reporter.WriteWarning(MyResources.AutomaticMigrationDisabledInfo);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return base.Execute();
        }
    }
}
