// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.DotNet.Cli.CommandLine;

namespace System.Data.Entity.Tools.Commands
{
    internal class HelpCommandBase : EFCommandBase
    {
        private CommandLineApplication _command;

        public override void Configure(CommandLineApplication command)
        {
            _command = command;

            base.Configure(command);
        }

        protected override int Execute()
        {
            _command.ShowHelp();

            return 0;
        }
    }
}
