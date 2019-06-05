// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.DotNet.Cli.CommandLine;

using MyResources = System.Data.Entity.Tools.Properties.Resources;

namespace System.Data.Entity.Tools.Commands
{
    internal abstract class MigrationsCommandBase : ProjectCommandBase
    {
        protected CommandOption MigrationsConfig { get; private set; }

        public override void Configure(CommandLineApplication command)
        {
            MigrationsConfig = command.Option("--migrations-config <TYPE>", MyResources.MigrationsConfigDescription);

            base.Configure(command);
        }
    }
}
