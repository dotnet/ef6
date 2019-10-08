// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
    internal abstract class ProjectCommandBase : EFCommandBase
    {
        protected CommandOption Assembly { get; private set; }
        protected CommandOption Config { get; private set; }
        protected CommandOption ConnectionProvider { get; private set; }
        protected CommandOption ConnectionString { get; private set; }
        protected CommandOption ConnectionStringName { get; private set; }
        protected CommandOption DataDir { get; private set; }
        protected CommandOption Language { get; private set; }
        protected CommandOption ProjectDir { get; private set; }
        protected CommandOption RootNamespace { get; private set; }

        public override void Configure(CommandLineApplication command)
        {
            Assembly = command.Option("-a|--assembly <PATH>", MyResources.AssemblyDescription);
            ProjectDir = command.Option("--project-dir <PATH>", MyResources.ProjectDirDescription);
            Language = command.Option("--language <LANGUAGE>", MyResources.LanguageDescription);
            RootNamespace = command.Option("--root-namespace <NAMESPACE>", MyResources.RootNamespaceDescription);
            DataDir = command.Option("--data-dir <PATH>", MyResources.DataDirDescription);
            Config = command.Option("--config <PATH>", MyResources.ConfigDescription);
            ConnectionStringName = command.Option("--connection-string-name <NAME>", MyResources.ConnectionStringNameDescription);
            ConnectionString = command.Option("--connection-string <STRING>", MyResources.ConnectionStringDescription);
            ConnectionProvider = command.Option("--connection-provider <NAME>", MyResources.ConnectionProviderDescription);

            base.Configure(command);
        }

        protected override void Validate()
        {
            base.Validate();

            if (!Assembly.HasValue())
            {
                throw new CommandException(string.Format(MyResources.MissingOption, Assembly.LongName));
            }

            if (ConnectionString.HasValue() || ConnectionProvider.HasValue())
            {
                if (!ConnectionString.HasValue())
                {
                    throw new CommandException(string.Format(MyResources.MissingOption, ConnectionString.LongName));
                }
                if (!ConnectionProvider.HasValue())
                {
                    throw new CommandException(string.Format(MyResources.MissingOption, ConnectionProvider.LongName));
                }
                if (ConnectionStringName.HasValue())
                {
                    throw new CommandException(
                        string.Format(
                            MyResources.MutuallyExclusiveOptions,
                            ConnectionStringName.LongName,
                            ConnectionString.LongName));
                }
            }
        }

        protected ExecutorBase CreateExecutor()
        {
            try
            {
#if NET40 || NET45
                return new AppDomainExecutor(
                    Assembly.Value(),
                    DataDir.Value(),
                    Config.Value(),
                    RootNamespace.Value(),
                    Language.Value());
#elif NETCOREAPP
                return new ReflectionExecutor(
                Assembly.Value(),
                DataDir.Value(),
                Config.Value(),
                RootNamespace.Value(),
                Language.Value());
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
                        Path.GetFileNameWithoutExtension(Assembly.Value())),
                    ex);
            }
        }

        protected string WriteMigration(
            ScaffoldedMigration scaffoldedMigration,
            bool rescaffolding = false,
            bool force = false,
            string name = null)
        {
            DebugCheck.NotNull(scaffoldedMigration);

            var userCodeFileName = scaffoldedMigration.MigrationId + "." + scaffoldedMigration.Language;
            var userCodePath = Path.Combine(scaffoldedMigration.Directory, userCodeFileName);
            var absoluteUserCodePath = Path.Combine(ProjectDir.Value(), userCodePath);
            var designerCodeFileName = scaffoldedMigration.MigrationId + ".Designer." + scaffoldedMigration.Language;
            var designerCodePath = Path.Combine(scaffoldedMigration.Directory, designerCodeFileName);
            var absoluteDesignerCodePath = Path.Combine(ProjectDir.Value(), designerCodePath);
            var resourcesFileName = scaffoldedMigration.MigrationId + ".resx";
            var resourcesPath = Path.Combine(scaffoldedMigration.Directory, resourcesFileName);

            if (rescaffolding && !force)
            {
                if (!string.Equals(scaffoldedMigration.UserCode, File.ReadAllText(absoluteUserCodePath)))
                {
                    Debug.Assert(!string.IsNullOrWhiteSpace(name));

                    Reporter.WriteWarning(string.Format(MyResources.RescaffoldNoForce, name));
                }
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(absoluteUserCodePath));
                File.WriteAllText(absoluteUserCodePath, scaffoldedMigration.UserCode, Encoding.UTF8);
            }

            var absoluteResourcesPath = Path.Combine(ProjectDir.Value(), resourcesPath);

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
