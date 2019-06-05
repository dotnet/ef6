// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.DotNet.Cli.CommandLine;

namespace System.Data.Entity.Tools.Commands
{
    internal abstract class EFCommandBase : CommandBase
    {
        public override void Configure(CommandLineApplication command)
        {
            command.HelpOption("-h|--help");

            base.Configure(command);
        }
    }
}
