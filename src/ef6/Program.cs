// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Tools.Commands;
using System.Linq;
using System.Text;
using Microsoft.DotNet.Cli.CommandLine;

namespace System.Data.Entity.Tools
{
    internal static class Program
    {
        private static readonly string[] _knownExceptions = new[]
        {
            "System.Data.Entity.Migrations.Infrastructure.MigrationsException",
            "System.Data.Entity.Migrations.Infrastructure.AutomaticMigrationsDisabledException",
            "System.Data.Entity.Migrations.Infrastructure.AutomaticDataLossException",
            "System.Data.Entity.Migrations.Infrastructure.MigrationsPendingException"
        };

        private static int Main(string[] args)
        {
#if NET45 || NETCOREAPP
            if (Console.IsOutputRedirected)
            {
                Console.OutputEncoding = Encoding.UTF8;
            }
#elif !NET40
#error Unexpected target framework
#endif

            var app = new CommandLineApplication
            {
                Name = "ef6"
            };

            new RootCommand().Configure(app);

            try
            {
                return app.Execute(args);
            }
            catch (Exception ex)
            {
                var wrappedException = ex as WrappedException;
                if (ex is CommandException
                    || ex is CommandParsingException
                    || _knownExceptions.Contains(wrappedException?.Type))
                {
                    Reporter.WriteVerbose(ex.ToString());
                }
                else
                {
                    Reporter.WriteInformation(ex.ToString());
                }

                Reporter.WriteError(ex.Message);

                return 1;
            }
        }
    }
}
