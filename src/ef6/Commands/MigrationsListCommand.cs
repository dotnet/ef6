// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.DotNet.Cli.CommandLine;

using MyResources = System.Data.Entity.Tools.Properties.Resources;

namespace System.Data.Entity.Tools.Commands
{
    internal class MigrationsListCommand : MigrationsCommandBase
    {
        public override void Configure(CommandLineApplication command)
        {
            command.Description = MyResources.MigrationsListDescription;

            base.Configure(command);
        }

        protected override int Execute()
        {
            Reporter.WriteInformation(MyResources.GetMigrationsCommand_Intro);

            var migrations = CreateExecutor().GetDatabaseMigrations(
                ConnectionStringName.Value(),
                ConnectionString.Value(),
                ConnectionProvider.Value(),
                MigrationsConfig.Value());

            if (migrations.Any())
            {
                foreach (var migration in migrations)
                {
                    Reporter.WriteData(migration);
                }
            }
            else
            {
                Reporter.WriteInformation(MyResources.GetMigrationsCommand_NoHistory);
            }

            return base.Execute();
        }
    }
}
