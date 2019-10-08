// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.DotNet.Cli.CommandLine;

using MyResources = System.Data.Entity.Tools.Properties.Resources;

namespace System.Data.Entity.Tools.Commands
{
    internal class MigrationsCommand : HelpCommandBase
    {
        public override void Configure(CommandLineApplication command)
        {
            command.Description = MyResources.MigrationsDescription;

            command.Command("add", new MigrationsAddCommand().Configure);
            command.Command("enable", new MigrationsEnableCommand().Configure);
            command.Command("list", new MigrationsListCommand().Configure);

            base.Configure(command);
        }
    }
}
