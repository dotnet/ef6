// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Console
{
    using System.Data.Entity.Migrations.Console.Resources;
    using CmdLine;

    [CommandLineArguments(
        Program = "migrate",
        TitleResourceId = EntityRes.MigrateTitle,
        DescriptionResourceId = EntityRes.MigrateDescription)]
    public class Arguments
    {
        [CommandLineParameter(
            ParameterIndex = 1,
            NameResourceId = EntityRes.AssemblyNameArgument,
            Required = true,
            DescriptionResourceId = EntityRes.AssemblyNameDescription)]
        public string AssemblyName { get; set; }

        [CommandLineParameter(
            ParameterIndex = 2,
            NameResourceId = EntityRes.ConfigurationTypeNameArgument,
            DescriptionResourceId = EntityRes.ConfigurationTypeNameDescription)]
        public string ConfigurationTypeName { get; set; }

        [CommandLineParameter(
            ParameterIndex = 3,
            NameResourceId = EntityRes.ContextAssemblyNameArgument,
            Required = false,
            DescriptionResourceId = EntityRes.ContextAssemblyNameDescription)]
        public string ContextAssemblyName { get; set; }

        [CommandLineParameter(
            Command = "targetMigration",
            DescriptionResourceId = EntityRes.TargetMigrationDescription)]
        public string TargetMigration { get; set; }

        [CommandLineParameter(
            Command = "startUpDirectory",
            DescriptionResourceId = EntityRes.WorkingDirectoryDescription)]
        public string WorkingDirectory { get; set; }

        [CommandLineParameter(
            Command = "startUpConfigurationFile",
            DescriptionResourceId = EntityRes.ConfigurationFileDescription)]
        public string ConfigurationFile { get; set; }

        [CommandLineParameter(
            Command = "startUpDataDirectory",
            DescriptionResourceId = EntityRes.DataDirectoryDescription)]
        public string DataDirectory { get; set; }

        [CommandLineParameter(
            Command = "connectionStringName",
            DescriptionResourceId = EntityRes.ConnectionStringNameDescription)]
        public string ConnectionStringName { get; set; }

        [CommandLineParameter(
            Command = "connectionString",
            DescriptionResourceId = EntityRes.ConnectionStringDescription)]
        public string ConnectionString { get; set; }

        [CommandLineParameter(
            Command = "connectionProviderName",
            DescriptionResourceId = EntityRes.ConnectionProviderNameDescription)]
        public string ConnectionProviderName { get; set; }

        [CommandLineParameter(
            Command = "force",
            DescriptionResourceId = EntityRes.ForceDescription)]
        public bool Force { get; set; }

        [CommandLineParameter(
            Command = "verbose",
            DescriptionResourceId = EntityRes.VerboseDescription)]
        public bool Verbose { get; set; }

        [CommandLineParameter(
            Command = "?",
            IsHelp = true,
            DescriptionResourceId = EntityRes.HelpDescription)]
        public bool Help { get; set; }

        internal void Validate()
        {
            if (!string.IsNullOrWhiteSpace(ConnectionStringName)
                && !string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw Error.AmbiguousConnectionString();
            }

            if (string.IsNullOrWhiteSpace(ConnectionString)
                != string.IsNullOrWhiteSpace(ConnectionProviderName))
            {
                throw Error.MissingConnectionInfo();
            }
        }

        internal void Standardize()
        {
            AssemblyName = Standardize(AssemblyName);
            ContextAssemblyName = Standardize(ContextAssemblyName);
        }

        private static string Standardize(string assemblyName)
        {
            if (assemblyName != null
                && (assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                    || assemblyName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)))
            {
                assemblyName = assemblyName.Substring(0, assemblyName.Length - 4);
            }
            return assemblyName;
        }
    }
}
