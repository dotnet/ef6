// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.DotNet.Cli.CommandLine;

using MyResources = System.Data.Entity.Tools.Properties.Resources;

namespace System.Data.Entity.Tools.Commands
{
    internal abstract class CommandBase
    {
        public virtual void Configure(CommandLineApplication command)
        {
            var verbose = command.Option("-v|--verbose", MyResources.VerboseDescription);
            var noColor = command.Option("--no-color", MyResources.NoColorDescription);
            var prefixOutput = command.Option("--prefix-output", MyResources.PrefixDescription);

            command.HandleResponseFiles = true;

            command.OnExecute(
                () =>
                    {
                        Reporter.IsVerbose = verbose.HasValue();
                        Reporter.NoColor = noColor.HasValue();
                        Reporter.PrefixOutput = prefixOutput.HasValue();

                        Validate();

                        return Execute();
                    });
        }

        protected virtual void Validate()
        {
        }

        protected virtual int Execute()
            => 0;
    }
}
