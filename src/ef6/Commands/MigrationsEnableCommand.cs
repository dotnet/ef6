// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity.Tools.Utilities;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.DotNet.Cli.CommandLine;

using MyResources = System.Data.Entity.Tools.Properties.Resources;

namespace System.Data.Entity.Tools.Commands
{
    internal class MigrationsEnableCommand : ProjectCommandBase
    {
        private CommandOption _force;
        private CommandOption _auto;
        private CommandOption _migrationsDir;
        private CommandOption _context;
        private CommandOption _contextAssembly;
        private CommandOption _json;

        public override void Configure(CommandLineApplication command)
        {
            command.Description = MyResources.MigrationsEnableDescription;

            _auto = command.Option("--auto", MyResources.MigrationsEnableAutoDescription);
            _force = command.Option("-f|--force", MyResources.MigrationsEnableForceDescription);
            _migrationsDir = command.Option("--migrations-dir <PATH>", MyResources.MigrationsDirDescription);
            _context = command.Option("-c|--context <DBCONTEXT>", MyResources.ContextDescription);
            _contextAssembly = command.Option("--context-assembly <PATH>", MyResources.ContextAssemblyDescription);
            _json = Json.ConfigureOption(command);

            base.Configure(command);
        }

        protected override int Execute()
        {
            var migrationsDirectory = _migrationsDir.Value();
            var qualifiedContextTypeName = CreateExecutor().GetContextType(
                _context.Value(),
                _contextAssembly.Value());
            var isVb = string.Equals(Language.Value(), "VB", StringComparison.OrdinalIgnoreCase);
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

            var rootNamespace = RootNamespace.Value();
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

            var absoluteMigrationsDirectory = Path.Combine(ProjectDir.Value(), migrationsDirectory);
            var absolutePath = Path.Combine(absoluteMigrationsDirectory, fileName);

            if (!_force.HasValue()
                && File.Exists(absolutePath))
            {
                throw new CommandException(string.Format(MyResources.MigrationsAlreadyEnabled, Assembly.Value()));
            }

            var fullMigrationsNamespace = rootNamespace + "." + migrationsNamespace;

            Reporter.WriteInformation(MyResources.EnableMigrations_BeginInitialScaffold);

            var scaffoldedMigration = CreateExecutor().ScaffoldInitialCreate(
                ConnectionStringName.Value(),
                ConnectionString.Value(),
                ConnectionProvider.Value(),
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
    }
}
