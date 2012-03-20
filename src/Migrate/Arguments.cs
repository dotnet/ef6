namespace System.Data.Entity.Migrations.Console
{
    using CmdLine;

    [CommandLineArguments(
        Program = "migrate",
        Title = "Code First Migrations Command Line Utility",
        Description = "Applies any pending migrations to the database.")]
    public class Arguments
    {
        [CommandLineParameter(
            ParameterIndex = 1,
            Name = "assembly",
            Required = true,
            Description = "Specifies the name of the assembly that contains the migrations configuration type.")]
        public string AssemblyName { get; set; }

        [CommandLineParameter(
            ParameterIndex = 2,
            Name = "configurationType",
            Description = "Specifies the name of the migrations configuration type. If omitted, Code First Migrations will attempt to locate a single migrations configuration type in the specified assembly.")]
        public string ConfigurationTypeName { get; set; }

        [CommandLineParameter(
            Command = "targetMigration",
            Description = "Specifies the name of a particular migration to update the database to. If omitted, the current model will be used.")]
        public string TargetMigration { get; set; }

        [CommandLineParameter(
            Command = "StartUpDirectory",
            Description = "Specifies the working directory of your application.")]
        public string WorkingDirectory { get; set; }

        [CommandLineParameter(
            Command = "startUpConfigurationFile",
            Description = "Specifies the Web.config or App.config file of your application.")]
        public string ConfigurationFile { get; set; }

        [CommandLineParameter(
            Command = "startUpDataDirectory",
            Description = "Specifies the directory to use when resolving connection strings containing the |DataDirectory| substitution string.")]
        public string DataDirectory { get; set; }

        [CommandLineParameter(
            Command = "connectionStringName",
            Description = "Specifies the name of the connection string to use from the specified configuration file. If omitted, the context's default connection will be used.")]
        public string ConnectionStringName { get; set; }

        [CommandLineParameter(
            Command = "connectionString",
            Description = "Specifies the the connection string to use. If omitted, the context's default connection will be used.")]
        public string ConnectionString { get; set; }

        [CommandLineParameter(
            Command = "connectionProviderName",
            Description = "Specifies the provider invariant name of the connection string.")]
        public string ConnectionProviderName { get; set; }

        [CommandLineParameter(
            Command = "force",
            Description = "Indicates that automatic migrations which might incur data loss should be allowed.")]
        public bool Force { get; set; }

        [CommandLineParameter(
            Command = "verbose",
            Description = "Indicates that the executing SQL and additional diagnostic information should be output to the console window.")]
        public bool Verbose { get; set; }

        [CommandLineParameter(
            Command = "?",
            IsHelp = true,
            Description = "Display this help message")]
        public bool Help { get; set; }

        internal void Validate()
        {
            if (!string.IsNullOrWhiteSpace(ConnectionStringName) && !string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new CommandLineException("Only one of /connectionStringName or /connectionString can be specified.");
            }

            if (string.IsNullOrWhiteSpace(ConnectionString) != string.IsNullOrWhiteSpace(ConnectionProviderName))
            {
                throw new CommandLineException("/connectionString and /connectionProviderName must be specified together.");
            }
        }

        internal void Standardize()
        {
            if (AssemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                || AssemblyName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                AssemblyName = AssemblyName.Substring(0, AssemblyName.Length - 4);
            }
        }
    }
}