// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity.Tools.Migrations.Design;
using System.Data.Entity.Tools.Utilities;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using Microsoft.DotNet.Cli.CommandLine;

using MyResources = System.Data.Entity.Tools.Properties.Resources;

namespace System.Data.Entity.Tools.Commands
{
    internal class MigrationsEnableCommand : EFCommandBase
    {
        private CommandOption _assembly;
        private CommandOption _force;
        private CommandOption _auto;
        private CommandOption _language;
        private CommandOption _projectDir;
        private CommandOption _migrationsDir;
        private CommandOption _rootNamespace;
        private CommandOption _context;
        private CommandOption _contextAssembly;
        private CommandOption _json;
        private CommandOption _connectionStringName;
        private CommandOption _connectionString;
        private CommandOption _connectionProvider;
        private CommandOption _dataDir;
        private CommandOption _config;

        public override void Configure(CommandLineApplication command)
        {
            command.Description = MyResources.MigrationsEnableDescription;

            _assembly = command.Option("-a|--assembly <PATH>", MyResources.AssemblyDescription);
            _projectDir = command.Option("--project-dir <PATH>", MyResources.ProjectDirDescription);
            _language = command.Option("--language <LANGUAGE>", MyResources.LanguageDescription);
            _rootNamespace = command.Option("--root-namespace <NAMESPACE>", MyResources.RootNamespaceDescription);
            _auto = command.Option("--auto", MyResources.MigrationsEnableAutoDescription);
            _force = command.Option("-f|--force", MyResources.MigrationsEnableForceDescription);
            _migrationsDir = command.Option("--migrations-dir <PATH>", MyResources.MigrationsDirDescription);
            _context = command.Option("-c|--context <DBCONTEXT>", MyResources.ContextDescription);
            _contextAssembly = command.Option("--context-assembly <PATH>", MyResources.ContextAssemblyDescription);
            _json = Json.ConfigureOption(command);
            _connectionStringName = command.Option("--connection-string-name <NAME>", MyResources.ConnectionStringNameDescription);
            _connectionString = command.Option("--connection-string <STRING>", MyResources.ConnectionStringDescription);
            _connectionProvider = command.Option("--connection-provider <NAME>", MyResources.ConnectionProviderDescription);
            _dataDir = command.Option("--data-dir <PATH>", MyResources.DataDirDescription);
            _config = command.Option("--config <PATH>", MyResources.ConfigDescription);

            base.Configure(command);
        }

        protected override void Validate()
        {
            base.Validate();

            if (!_assembly.HasValue())
            {
                throw new CommandException(string.Format(MyResources.MissingOption, _assembly.LongName));
            }

            if (_connectionString.HasValue() || _connectionProvider.HasValue())
            {
                if (!_connectionString.HasValue())
                {
                    throw new CommandException(string.Format(MyResources.MissingOption, _connectionString.LongName));
                }
                if (!_connectionProvider.HasValue())
                {
                    throw new CommandException(string.Format(MyResources.MissingOption, _connectionProvider.LongName));
                }
                if (_connectionStringName.HasValue())
                {
                    throw new CommandException(
                        string.Format(
                            MyResources.MutuallyExclusiveOptions,
                            _connectionStringName.LongName,
                            _connectionString.LongName));
                }
            }
        }

        protected override int Execute()
        {
            var migrationsDirectory = _migrationsDir.Value();
            var qualifiedContextTypeName = CreateExecutor().GetContextType(
                _context.Value(),
                _contextAssembly.Value());
            var isVb = string.Equals(_language.Value(), "VB", StringComparison.OrdinalIgnoreCase);
            var fileName = isVb ? "Configuration.vb" : "Configuration.cs";
            var templateStream = typeof(MigrationsEnableCommand).Assembly.GetManifestResourceStream("System.Data.Entity.Tools.Templates." + fileName);
            Debug.Assert(templateStream != null);

            string template;
            using (var templateReader = new StreamReader(templateStream, Encoding.UTF8))
            {
                template = templateReader.ReadToEnd();
            }

            var tokens = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(migrationsDirectory))
            {
                tokens["migrationsDirectory"]
                    = "\r\n            MigrationsDirectory = "
                      + (!isVb ? "@" : null)
                      + "\"" + migrationsDirectory + "\""
                      + (!isVb ? ";" : null);
            }
            else
            {
                migrationsDirectory = "Migrations";
            }

            tokens["enableAutomaticMigrations"]
                = _auto.HasValue()
                      ? (isVb ? "True" : "true")
                      : (isVb ? "False" : "false");

            var rootNamespace = _rootNamespace.Value();
            var migrationsNamespace = migrationsDirectory.Replace("\\", ".");

            tokens["namespace"]
                = !isVb && !string.IsNullOrWhiteSpace(rootNamespace)
                      ? rootNamespace + "." + migrationsNamespace
                      : migrationsNamespace;

            if (isVb && qualifiedContextTypeName.StartsWith(rootNamespace + "."))
            {
                tokens["contextType"]
                    = qualifiedContextTypeName.Substring(rootNamespace.Length + 1).Replace('+', '.');
            }
            else
            {
                tokens["contextType"] = qualifiedContextTypeName.Replace('+', '.');
            }

            if (Path.IsPathRooted(migrationsDirectory))
            {
                throw new CommandException(string.Format(MyResources.MigrationsDirectoryParamIsRooted, migrationsDirectory));
            }

            var absoluteMigrationsDirectory = Path.Combine(_projectDir.Value(), migrationsDirectory);
            var absolutePath = Path.Combine(absoluteMigrationsDirectory, fileName);

            if (!_force.HasValue()
                && File.Exists(absolutePath))
            {
                throw new CommandException(string.Format(MyResources.MigrationsAlreadyEnabled, _assembly.Value()));
            }

            var fullMigrationsNamespace = rootNamespace + "." + migrationsNamespace;

            Reporter.WriteInformation(MyResources.EnableMigrations_BeginInitialScaffold);

            var scaffoldedMigration = CreateExecutor().ScaffoldInitialCreate(
                _connectionStringName.Value(),
                _connectionString.Value(),
                _connectionProvider.Value(),
                qualifiedContextTypeName,
                _contextAssembly.Value(),
                fullMigrationsNamespace,
                _auto.HasValue(),
                migrationsDirectory);
            if (scaffoldedMigration != null)
            {
                if (!_auto.HasValue())
                {
                    WriteMigration(scaffoldedMigration);

                    Reporter.WriteWarning(string.Format(
                        MyResources.EnableMigrations_InitialScaffold,
                        scaffoldedMigration.MigrationId));
                }

                // We found an initial create so we need to add an explicit ContextKey
                // assignment to the configuration

                tokens["contextKey"]
                    = "\r\n            ContextKey = "
                      + "\"" + qualifiedContextTypeName + "\""
                      + (!isVb ? ";" : null);
            }

            Directory.CreateDirectory(absoluteMigrationsDirectory);
            File.WriteAllText(absolutePath, new TemplateProcessor().Process(template, tokens), Encoding.UTF8);

            if (_json.HasValue())
            {
                string migrationPath = null;
                string migrationDesignerPath = null;
                string migrationResourcesPath = null;
                if (scaffoldedMigration != null)
                {
                    migrationPath = Path.Combine(
                        absoluteMigrationsDirectory,
                        scaffoldedMigration.MigrationId + "." + scaffoldedMigration.Language);
                    migrationDesignerPath = Path.Combine(
                        absoluteMigrationsDirectory,
                        scaffoldedMigration.MigrationId + ".resx");
                    migrationResourcesPath = Path.Combine(
                        absoluteMigrationsDirectory,
                        scaffoldedMigration.MigrationId + ".Designer." + scaffoldedMigration.Language);
                }

                Reporter.WriteData("{");
                Reporter.WriteData("  \"migrationsConfiguration\": " + Json.Literal(absolutePath) + ",");
                Reporter.WriteData("  \"migration\": " + Json.Literal(migrationPath) + ",");
                Reporter.WriteData("  \"migrationResources\": " + Json.Literal(migrationDesignerPath) + ",");
                Reporter.WriteData("  \"migrationDesigner\": " + Json.Literal(migrationResourcesPath));
                Reporter.WriteData("}");
            }

            return base.Execute();
        }

        private ExecutorBase CreateExecutor()
        {
            try
            {
#if NET40 || NET45
                return new AppDomainExecutor(
                    _assembly.Value(),
                    _dataDir.Value(),
                    _config.Value(),
                    _rootNamespace.Value(),
                    _language.Value());
#elif NETCOREAPP3_0
                return new ReflectionExecutor(
                _assembly.Value(),
                _dataDir.Value(),
                _config.Value(),
                _rootNamespace.Value(),
                _language.Value());
#else
#error Unexpected target framework
#endif
            }
            catch (FileNotFoundException ex)
                when (new AssemblyName(ex.FileName).Name == "EntityFramework")
            {
                throw new CommandException(
                    string.Format(
                        MyResources.EntityFrameworkNotFound,
                        Path.GetFileNameWithoutExtension(_assembly.Value())),
                    ex);
            }
        }

        private string WriteMigration(ScaffoldedMigration scaffoldedMigration)
        {
            DebugCheck.NotNull(scaffoldedMigration);

            var userCodeFileName = scaffoldedMigration.MigrationId + "." + scaffoldedMigration.Language;
            var userCodePath = Path.Combine(scaffoldedMigration.Directory, userCodeFileName);
            var absoluteUserCodePath = Path.Combine(_projectDir.Value(), userCodePath);
            var designerCodeFileName = scaffoldedMigration.MigrationId + ".Designer." + scaffoldedMigration.Language;
            var designerCodePath = Path.Combine(scaffoldedMigration.Directory, designerCodeFileName);
            var absoluteDesignerCodePath = Path.Combine(_projectDir.Value(), designerCodePath);
            var resourcesFileName = scaffoldedMigration.MigrationId + ".resx";
            var resourcesPath = Path.Combine(scaffoldedMigration.Directory, resourcesFileName);

            Directory.CreateDirectory(Path.GetDirectoryName(absoluteUserCodePath));
            File.WriteAllText(absoluteUserCodePath, scaffoldedMigration.UserCode, Encoding.UTF8);

            var absoluteResourcesPath = Path.Combine(_projectDir.Value(), resourcesPath);

            using (var writer = new ResXResourceWriter(absoluteResourcesPath))
            {
                foreach (var i in scaffoldedMigration.Resources)
                {
                    writer.AddResource(i.Key, i.Value);
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(absoluteDesignerCodePath));
            File.WriteAllText(absoluteDesignerCodePath, scaffoldedMigration.DesignerCode, Encoding.UTF8);

            return userCodePath;
        }
    }
}
