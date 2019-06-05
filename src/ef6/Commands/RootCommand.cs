// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.DotNet.Cli.CommandLine;
using static System.Data.Entity.Tools.AnsiConstants;
using MyResources = System.Data.Entity.Tools.Properties.Resources;

namespace System.Data.Entity.Tools.Commands
{
    internal class RootCommand : HelpCommandBase
    {
        public override void Configure(CommandLineApplication command)
        {
            command.FullName = MyResources.EF6FullName;

            command.Command("database", new DatabaseCommand().Configure);
            command.Command("migrations", new MigrationsCommand().Configure);

            command.VersionOption("--version", GetVersion);

            base.Configure(command);
        }

        protected override int Execute()
        {
            Reporter.WriteInformation(
                string.Join(
                    Environment.NewLine,
                    string.Empty,
                    Reporter.Colorize(@"                     ___  ", x => x.Insert(19, Bold + Blue)),
                    Reporter.Colorize(@"                    / __| ", x => x.Insert(24, Blue).Insert(19, Cyan)),
                    Reporter.Colorize(@"         ___  ___  | |__  ", x => x.Insert(19, Bold + Green).Insert(8, Dark + Magenta)),
                    Reporter.Colorize(@"        | __|| __| |  _ \ ", x => x.Insert(23, Gray).Insert(22, Yellow).Insert(19, Bold + Gray).Insert(8, Dark + Magenta)),
                    Reporter.Colorize(@"        | _| | _|  | |_| |", x => x.Insert(19, Bold + Yellow).Insert(8, Dark + Magenta)),
                    Reporter.Colorize(@"        |___||_|    \___/ ", x => x.Insert(26, Reset).Insert(19, Bold + Red).Insert(8, Dark + Magenta)),
                    string.Empty));

            return base.Execute();
        }

        private static string GetVersion()
            => typeof(RootCommand).Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                .Cast<AssemblyInformationalVersionAttribute>().FirstOrDefault()?.InformationalVersion;
    }
}
