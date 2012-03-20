namespace System.Data.Entity.Migrations.Console
{
    using System;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Design;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Reflection;
    using CmdLine;

    internal class Program
    {
        private Arguments _arguments;

        public Program(Arguments arguments)
        {
            _arguments = arguments;
        }

        public static void Main(string[] args)
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
            }
            catch (CommandLineException ex)
            {
                WriteError(ex.ArgumentHelp.Message);
                WriteLine(ex.ArgumentHelp.GetHelpText(Console.BufferWidth));
            }
            catch (Exception ex)
            {
                if (arguments != null
                    && arguments.Verbose)
                {
                    WriteVerbose(ex.ToString());
                }

                WriteError(ex.Message);
            }
        }

        public void Run()
        {
            using (var facade = CreateFacade())
            {
                facade.Update(_arguments.TargetMigration, _arguments.Force);
            }
        }

        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            if (new AssemblyName(args.Name).Name == "EntityFramework")
            {
                var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\lib\net45\EntityFramework.dll");

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
            CommandLine.WriteLineColor(ConsoleColor.Red, "ERROR: " + message);
        }

        private static void WriteWarning(string message)
        {
            CommandLine.WriteLineColor(ConsoleColor.Yellow, "WARNING: " + message);
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
                Contract.Assert(string.IsNullOrWhiteSpace(_arguments.ConnectionString));
                Contract.Assert(string.IsNullOrWhiteSpace(_arguments.ConnectionProviderName));

                connectionStringInfo = new DbConnectionInfo(_arguments.ConnectionStringName);
            }
            else if (!string.IsNullOrWhiteSpace(_arguments.ConnectionString))
            {
                Contract.Assert(string.IsNullOrWhiteSpace(_arguments.ConnectionStringName));
                Contract.Assert(!string.IsNullOrWhiteSpace(_arguments.ConnectionProviderName));

                connectionStringInfo = new DbConnectionInfo(_arguments.ConnectionString, _arguments.ConnectionProviderName);
            }

            var facade
                = new ToolingFacade(
                    _arguments.AssemblyName,
                    _arguments.ConfigurationTypeName,
                    _arguments.WorkingDirectory,
                    _arguments.ConfigurationFile,
                    _arguments.DataDirectory,
                    connectionStringInfo);

            facade.LogInfoDelegate = WriteLine;
            facade.LogWarningDelegate = WriteWarning;

            if (_arguments.Verbose)
            {
                facade.LogVerboseDelegate = sql => WriteVerbose("VERBOSE: " + sql);
            }

            return facade;
        }
    }
}