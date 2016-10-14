// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Console
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Console.Resources;
    using System.Data.Entity.Migrations.Design;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using CmdLine;
    using Console = System.Console;

    internal class Program
    {
        private const int ExitCodeSuccess = 0;
        private const int ExitCodeError = 1;

        private readonly Arguments _arguments;

        public Program(Arguments arguments)
        {
            _arguments = arguments;
        }

        public static int Main(string[] args)
        {
            Arguments arguments = null;

            try
            {
                arguments = CommandLine.Parse<Arguments>();
                arguments.Validate();
                arguments.Standardize();

                AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

                new Program(arguments).Run();
            }
            catch (CommandLineHelpException ex)
            {
                WriteLine(ex.ArgumentHelp.GetHelpText(Console.BufferWidth));

                return ExitCodeSuccess;
            }
            catch (CommandLineException ex)
            {
                if (ex.ArgumentHelp != null)
                {
                    WriteError(ex.ArgumentHelp.Message);
                    WriteLine(ex.ArgumentHelp.GetHelpText(Console.BufferWidth));
                }
                else
                {
                    WriteError(ex.Message);
                }

                return ExitCodeError;
            }
            catch (Exception ex)
            {
                if (arguments != null
                    && arguments.Verbose)
                {
                    WriteVerbose(ex.ToString());
                }

                WriteError(ex.Message);

                return ExitCodeError;
            }

            return ExitCodeSuccess;
        }

        public void Run()
        {
            using (var facade = CreateFacade())
            {
                if (!String.IsNullOrEmpty(_arguments.ScriptFile)) 
                {
                    ScriptUpdate(facade, _arguments);
                } 
                else 
                {
                    facade.Update(_arguments.TargetMigration, _arguments.Force);
                }
            }
        }

        private static void ScriptUpdate(ToolingFacade facade, Arguments arguments)
        {
            string scriptContents = facade.ScriptUpdate(arguments.SourceMigration, arguments.TargetMigration, arguments.Force);

            File.WriteAllText(arguments.ScriptFile, scriptContents, Encoding.UTF8);
        }

        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            if (new AssemblyName(args.Name).Name == "EntityFramework")
            {
                var assemblyPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, @"..\lib\net45\EntityFramework.dll");

                if (File.Exists(assemblyPath))
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
            }

            return null;
        }

        private static void WriteLine(string message)
        {
            Console.WriteLine(message);
        }

        private static void WriteError(string message)
        {
            CommandLine.WriteLineColor(ConsoleColor.Red, Strings.ErrorMessage(message));
        }

        private static void WriteWarning(string message)
        {
            CommandLine.WriteLineColor(ConsoleColor.Yellow, Strings.WarningMessage(message));
        }

        private static void WriteVerbose(string message)
        {
            CommandLine.WriteLineColor(ConsoleColor.DarkGray, message);
        }

        private ToolingFacade CreateFacade()
        {
            DbConnectionInfo connectionStringInfo = null;

            if (!string.IsNullOrWhiteSpace(_arguments.ConnectionStringName))
            {
                Debug.Assert(string.IsNullOrWhiteSpace(_arguments.ConnectionString));
                Debug.Assert(string.IsNullOrWhiteSpace(_arguments.ConnectionProviderName));

                connectionStringInfo = new DbConnectionInfo(_arguments.ConnectionStringName);
            }
            else if (!string.IsNullOrWhiteSpace(_arguments.ConnectionString))
            {
                Debug.Assert(string.IsNullOrWhiteSpace(_arguments.ConnectionStringName));
                Debug.Assert(!string.IsNullOrWhiteSpace(_arguments.ConnectionProviderName));

                connectionStringInfo = new DbConnectionInfo(
                    _arguments.ConnectionString, _arguments.ConnectionProviderName);
            }

            var facade
                = new ToolingFacade(
                    _arguments.AssemblyName,
                    string.IsNullOrWhiteSpace(_arguments.ContextAssemblyName) ? _arguments.AssemblyName : _arguments.ContextAssemblyName,
                    _arguments.ConfigurationTypeName,
                    _arguments.WorkingDirectory,
                    _arguments.ConfigurationFile,
                    _arguments.DataDirectory,
                    connectionStringInfo);

            facade.LogInfoDelegate = WriteLine;
            facade.LogWarningDelegate = WriteWarning;

            if (_arguments.Verbose)
            {
                facade.LogVerboseDelegate = sql => WriteVerbose(Strings.VerboseMessage(sql));
            }

            return facade;
        }
    }
}
