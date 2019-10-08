// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.DotNet.Cli.CommandLine;
using MyResources = System.Data.Entity.Tools.Properties.Resources;

namespace System.Data.Entity.Tools.Commands
{
    internal class DatabaseCommand : HelpCommandBase
    {
        public override void Configure(CommandLineApplication command)
        {
            command.Description = MyResources.DatabaseDescription;

            command.Command("update", new DatabaseUpdateCommand().Configure);

            base.Configure(command);
        }
    }
}
