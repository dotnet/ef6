namespace System.Data.Entity.Migrations.Design
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Utilities;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Helper class that is used by design time tools to run migrations related  
    ///     commands that need to interact with an application that is being edited
    ///     in Visual Studio.
    /// 
    ///     Because the application is being edited the assemblies need to
    ///     be loaded in a separate AppDomain to ensure the latest version
    ///     is always loaded.
    /// 
    ///     The App/Web.config file from the startup project is also copied
    ///     to ensure that any configuration is applied.
    /// </summary>
    public class ToolingFacade : IDisposable
    {
        private readonly string _assemblyName;
        private readonly string _configurationTypeName;
        private readonly string _configurationFile;
        private readonly DbConnectionInfo _connectionStringInfo;

        private AppDomain _appDomain;

        /// <summary>
        ///     Gets or sets an action to be run to log information.
        /// </summary>
        public Action<string> LogInfoDelegate { get; set; }

        /// <summary>
        ///     Gets or sets an action to be run to log warnings.
        /// </summary>
        public Action<string> LogWarningDelegate { get; set; }

        /// <summary>
        ///     Gets or sets an action to be run to log verbose information.
        /// </summary>
        public Action<string> LogVerboseDelegate { get; set; }

        /// <summary>
        ///     Initializes a new instance of the ToolingFacade class.
        /// </summary>
        /// <param name = "assemblyName">
        ///     The name of the assembly that contains the migrations configuration to be used.
        /// </param>
        /// <param name = "configurationTypeName">
        ///     The namespace qualified name of migrations configuration to be used.
        /// </param>
        /// <param name = "workingDirectory">
        ///     The working directory containing the compiled assemblies.
        /// </param>
        /// <param name = "configurationFilePath">
        ///     The path of the config file from the startup project.
        /// </param>
        /// <param name = "dataDirectory">
        ///     The path of the application data directory from the startup project.
        ///     Typically the App_Data directory for web applications or the working directory for executables.
        /// </param>
        /// <param name = "connectionStringInfo">
        ///     The connection to the database to be migrated.
        ///     If null is supplied, the default connection for the context will be used.
        /// </param>
        public ToolingFacade(
            string assemblyName,
            string configurationTypeName,
            string workingDirectory,
            string configurationFilePath,
            string dataDirectory,
            DbConnectionInfo connectionStringInfo)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(assemblyName));

            _assemblyName = assemblyName;
            _configurationTypeName = configurationTypeName;
            _connectionStringInfo = connectionStringInfo;

            var info = new AppDomainSetup { ShadowCopyFiles = "true" };

            if (!string.IsNullOrWhiteSpace(workingDirectory))
            {
                info.ApplicationBase = workingDirectory;
            }

            _configurationFile = new ConfigurationFileUpdater().Update(configurationFilePath);
            info.ConfigurationFile = _configurationFile;

            var friendlyName = "MigrationsToolingFacade" + Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            _appDomain = AppDomain.CreateDomain(friendlyName, null, info);

            if (!string.IsNullOrWhiteSpace(dataDirectory))
            {
                _appDomain.SetData("DataDirectory", dataDirectory);
            }
        }

        /// <summary>
        ///     Releases all unmanaged resources used by the facade.
        /// </summary>
        ~ToolingFacade()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Gets the fully qualified name of all types deriving from <see cref = "DbContext" />.
        /// </summary>
        /// <returns>All context types found.</returns>
        public IEnumerable<string> GetContextTypes()
        {
            var runner = new GetContextTypesRunner();
            ConfigureRunner(runner);

            Run(runner);

            return (IEnumerable<string>)_appDomain.GetData("result");
        }

        /// <summary>
        ///     Gets a list of all migrations that have been applied to the database.
        /// </summary>
        /// <returns>Ids of applied migrations.</returns>
        public IEnumerable<string> GetDatabaseMigrations()
        {
            var runner = new GetDatabaseMigrationsRunner();
            ConfigureRunner(runner);

            Run(runner);

            return (IEnumerable<string>)_appDomain.GetData("result");
        }

        /// <summary>
        ///     Gets a list of all migrations that have not been applied to the database.
        /// </summary>
        /// <returns>Ids of pending migrations.</returns>
        public IEnumerable<string> GetPendingMigrations()
        {
            var runner = new GetPendingMigrationsRunner();
            ConfigureRunner(runner);

            Run(runner);

            return (IEnumerable<string>)_appDomain.GetData("result");
        }

        /// <summary>
        ///     Updates the database to the specified migration.
        /// </summary>
        /// <param name = "targetMigration">
        ///     The Id of the migration to migrate to.
        ///     If null is supplied, the database will be updated to the latest migration.
        /// </param>
        /// <param name = "force">Value indicating if data loss during automatic migration is acceptable.</param>
        public void Update(string targetMigration, bool force)
        {
            var runner = new UpdateRunner
                {
                    TargetMigration = targetMigration,
                    Force = force
                };
            ConfigureRunner(runner);

            Run(runner);
        }

        /// <summary>
        ///     Generates a SQL script to migrate between two migrations.
        /// </summary>
        /// <param name = "sourceMigration">
        ///     The migration to update from. 
        ///     If null is supplied, a script to update the current database will be produced.
        /// </param>
        /// <param name = "targetMigration">
        ///     The migration to update to.
        ///     If null is supplied, a script to update to the latest migration will be produced.
        /// </param>
        /// <param name = "force">Value indicating if data loss during automatic migration is acceptable.</param>
        /// <returns>The generated SQL script.</returns>
        public string ScriptUpdate(string sourceMigration, string targetMigration, bool force)
        {
            var runner
                = new ScriptUpdateRunner
                    {
                        SourceMigration = sourceMigration,
                        TargetMigration = targetMigration,
                        Force = force
                    };
            ConfigureRunner(runner);

            Run(runner);

            return (string)_appDomain.GetData("result");
        }

        /// <summary>
        ///     Scaffolds a code-based migration to apply any pending model changes.
        /// </summary>
        /// <param name = "migrationName">The name for the generated migration.</param>
        /// <param name = "language">The programming language of the generated migration.</param>
        /// <param name = "rootNamespace">The root namespace of the project the migration will be added to.</param>
        /// <param name = "ignoreChanges">Whether or not to include model changes.</param>
        /// <returns>The scaffolded migration.</returns>
        public ScaffoldedMigration Scaffold(string migrationName, string language, string rootNamespace, bool ignoreChanges)
        {
            var runner
                = new ScaffoldRunner
                    {
                        MigrationName = migrationName,
                        Language = language,
                        RootNamespace = rootNamespace,
                        IgnoreChanges = ignoreChanges
                    };
            ConfigureRunner(runner);

            Run(runner);

            return HydrateScaffoldedMigration();
        }

        /// <summary>
        ///     Scaffolds the initial code-based migration corresponding to a previously run database initializer.
        /// </summary>
        /// <param name = "language">The programming language of the generated migration.</param>
        /// <param name = "rootNamespace">The root namespace of the project the migration will be added to.</param>
        /// <returns>The scaffolded migration.</returns>
        public ScaffoldedMigration ScaffoldInitialCreate(string language, string rootNamespace)
        {
            var runner
                = new InitialCreateScaffoldRunner
                    {
                        Language = language,
                        RootNamespace = rootNamespace
                    };

            ConfigureRunner(runner);

            Run(runner);

            return HydrateScaffoldedMigration();
        }

        private ScaffoldedMigration HydrateScaffoldedMigration()
        {
            if ((bool)_appDomain.GetData("result.IsNull"))
            {
                return null;
            }

            return new ScaffoldedMigration
                {
                    DesignerCode = (string)_appDomain.GetData("result.DesignerCode"),
                    Language = (string)_appDomain.GetData("result.Language"),
                    MigrationId = (string)_appDomain.GetData("result.MigrationId"),
                    UserCode = (string)_appDomain.GetData("result.UserCode"),
                    Directory = (string)_appDomain.GetData("result.Folder")
                };
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases all resources used by the facade.
        /// </summary>
        /// <param name = "disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _appDomain != null)
            {
                AppDomain.Unload(_appDomain);
                _appDomain = null;
            }

            if (_configurationFile != null)
            {
                File.Delete(_configurationFile);
            }
        }

        private void ConfigureRunner(BaseRunner runner)
        {
            runner.AssemblyName = _assemblyName;
            runner.ConfigurationTypeName = _configurationTypeName;
            runner.ConnectionStringInfo = _connectionStringInfo;
            runner.Log = new ToolLogger(this);
        }

        private void Run(BaseRunner runner)
        {
            _appDomain.SetData("error", null);
            _appDomain.SetData("typeName", null);
            _appDomain.SetData("stackTrace", null);

            _appDomain.DoCallBack(runner.Run);

            var error = (string)_appDomain.GetData("error");

            if (error != null)
            {
                var typeName = (string)_appDomain.GetData("typeName");
                var stackTrace = (string)_appDomain.GetData("stackTrace");

                throw new ToolingException(error, typeName, stackTrace);
            }
        }

        private class ToolLogger : MigrationsLogger
        {
            private readonly ToolingFacade _facade;

            public ToolLogger(ToolingFacade facade)
            {
                _facade = facade;
            }

            public override void Info(string message)
            {
                if (_facade.LogInfoDelegate != null)
                {
                    _facade.LogInfoDelegate(message);
                }
            }

            public override void Warning(string message)
            {
                if (_facade.LogWarningDelegate != null)
                {
                    _facade.LogWarningDelegate(message);
                }
            }

            public override void Verbose(string sql)
            {
                if (_facade.LogVerboseDelegate != null)
                {
                    _facade.LogVerboseDelegate(sql);
                }
            }
        }

        [Serializable]
        private abstract class BaseRunner
        {
            public string AssemblyName { get; set; }
            public string ConfigurationTypeName { get; set; }
            public DbConnectionInfo ConnectionStringInfo { get; set; }
            public ToolLogger Log { get; set; }

            public void Run()
            {
                try
                {
                    RunCore();
                }
                catch (Exception ex)
                {
                    // HACK: Not sure why exceptions won't just serialize straight across
                    AppDomain.CurrentDomain.SetData("error", ex.Message);
                    AppDomain.CurrentDomain.SetData("typeName", ex.GetType().FullName);
                    AppDomain.CurrentDomain.SetData("stackTrace", ex.ToString());
                }
            }

            protected abstract void RunCore();

            protected MigratorBase GetMigrator()
            {
                return DecorateMigrator(new DbMigrator(GetConfiguration()));
            }

            protected DbMigrationsConfiguration GetConfiguration()
            {
                var configuration = FindConfiguration();
                OverrideConfiguration(configuration);

                return configuration;
            }

            protected virtual void OverrideConfiguration(DbMigrationsConfiguration configuration)
            {
                if (ConnectionStringInfo != null)
                {
                    configuration.TargetDatabase = ConnectionStringInfo;
                }
            }

            private MigratorBase DecorateMigrator(DbMigrator migrator)
            {
                return new MigratorLoggingDecorator(migrator, Log);
            }

            private DbMigrationsConfiguration FindConfiguration()
            {
                var configurationTypeNameSpecified = !string.IsNullOrWhiteSpace(ConfigurationTypeName);
                var assembly = Assembly.Load(AssemblyName);

                Type configurationType = null;

                // Try for a fully-qualified match
                if (configurationTypeNameSpecified)
                {
                    configurationType = assembly.GetType(ConfigurationTypeName);
                }

                // Otherwise, search for it
                if (configurationType == null)
                {
                    var assemblyName = assembly.GetName().Name;
                    var configurationTypes
                        = assembly.GetTypes()
                            .Where(t => typeof(DbMigrationsConfiguration).IsAssignableFrom(t));

                    if (configurationTypeNameSpecified)
                    {
                        configurationTypes
                            = configurationTypes
                                .Where(t => string.Equals(t.Name, ConfigurationTypeName, StringComparison.OrdinalIgnoreCase))
                                .ToList();

                        // Disambiguate using case
                        if (configurationTypes.Count() > 1)
                        {
                            configurationTypes
                                = configurationTypes
                                    .Where(t => string.Equals(t.Name, ConfigurationTypeName, StringComparison.Ordinal))
                                    .ToList();
                        }

                        if (!configurationTypes.Any())
                        {
                            throw Error.AssemblyMigrator_NoConfigurationWithName(ConfigurationTypeName, assemblyName);
                        }

                        if (configurationTypes.Count() > 1)
                        {
                            throw Error.AssemblyMigrator_MultipleConfigurationsWithName(ConfigurationTypeName, assemblyName);
                        }
                    }
                    else
                    {
                        // Filter out unusable types
                        configurationTypes
                            = configurationTypes
                                .Where(
                                    t => t.GetConstructor(Type.EmptyTypes) != null
                                         && !t.IsAbstract
                                         && !t.IsGenericType)
                                .ToList();

                        if (!configurationTypes.Any())
                        {
                            throw Error.AssemblyMigrator_NoConfiguration(assemblyName);
                        }

                        if (configurationTypes.Count() > 1)
                        {
                            throw Error.AssemblyMigrator_MultipleConfigurations(assemblyName);
                        }
                    }

                    Contract.Assert(configurationTypes.Count() == 1);
                    configurationType = configurationTypes.Single();
                }

                Contract.Assert(configurationType != null);

                return CreateConfiguration(configurationType);
            }

            private static DbMigrationsConfiguration CreateConfiguration(Type configurationType)
            {
                var configurationTypeName = configurationType.Name;

                if (!typeof(DbMigrationsConfiguration).IsAssignableFrom(configurationType))
                {
                    throw Error.AssemblyMigrator_NonConfigurationType(configurationTypeName);
                }

                if (configurationType.GetConstructor(Type.EmptyTypes) == null)
                {
                    throw Error.AssemblyMigrator_NoDefaultConstructor(configurationTypeName);
                }

                if (configurationType.IsAbstract)
                {
                    throw Error.AssemblyMigrator_AbstractConfigurationType(configurationTypeName);
                }

                if (configurationType.IsGenericType)
                {
                    throw Error.AssemblyMigrator_GenericConfigurationType(configurationTypeName);
                }

                return (DbMigrationsConfiguration)Activator.CreateInstance(configurationType);
            }
        }

        [Serializable]
        private class GetDatabaseMigrationsRunner : BaseRunner
        {
            protected override void RunCore()
            {
                var databaseMigrations = GetMigrator().GetDatabaseMigrations();

                AppDomain.CurrentDomain.SetData("result", databaseMigrations);
            }
        }

        [Serializable]
        private class GetPendingMigrationsRunner : BaseRunner
        {
            protected override void RunCore()
            {
                var pendingMigrations = GetMigrator().GetPendingMigrations();

                AppDomain.CurrentDomain.SetData("result", pendingMigrations);
            }
        }

        [Serializable]
        private class UpdateRunner : BaseRunner
        {
            public string TargetMigration { get; set; }
            public bool Force { get; set; }

            protected override void RunCore()
            {
                GetMigrator().Update(TargetMigration);
            }

            protected override void OverrideConfiguration(DbMigrationsConfiguration configuration)
            {
                base.OverrideConfiguration(configuration);

                if (Force)
                {
                    configuration.AutomaticMigrationDataLossAllowed = true;
                }
            }
        }

        [Serializable]
        private class ScriptUpdateRunner : BaseRunner
        {
            public string SourceMigration { get; set; }
            public string TargetMigration { get; set; }
            public bool Force { get; set; }

            protected override void RunCore()
            {
                var migrator = GetMigrator();

                var script
                    = new MigratorScriptingDecorator(migrator)
                        .ScriptUpdate(SourceMigration, TargetMigration);

                AppDomain.CurrentDomain.SetData("result", script);
            }

            protected override void OverrideConfiguration(DbMigrationsConfiguration configuration)
            {
                base.OverrideConfiguration(configuration);

                if (Force)
                {
                    configuration.AutomaticMigrationDataLossAllowed = true;
                }
            }
        }

        [Serializable]
        private class ScaffoldRunner : BaseRunner
        {
            public string MigrationName { get; set; }
            public string Language { get; set; }
            public string RootNamespace { get; set; }
            public bool IgnoreChanges { get; set; }

            protected override void RunCore()
            {
                var configuration = GetConfiguration();

                var scaffolder = new MigrationScaffolder(configuration);

                var @namespace = configuration.MigrationsNamespace;

                // Need to strip project namespace when generating code for VB projects 
                // (The VB compiler automatically prefixes the project namespace)
                if (Language == "vb" && !string.IsNullOrWhiteSpace(RootNamespace))
                {
                    if (RootNamespace.EqualsIgnoreCase(@namespace))
                    {
                        @namespace = null;
                    }
                    else if (@namespace != null && @namespace.StartsWith(RootNamespace + ".", StringComparison.OrdinalIgnoreCase))
                    {
                        @namespace = @namespace.Substring(RootNamespace.Length + 1);
                    }
                    else
                    {
                        throw Error.MigrationsNamespaceNotUnderRootNamespace(@namespace, RootNamespace);
                    }
                }

                scaffolder.Namespace = @namespace;

                var scaffoldedMigration = Scaffold(scaffolder);

                // UNDONE: Not sure why this won't just serialize straight across
                // AppDomain.CurrentDomain.SetData("result", scaffoldedMigration);

                if (scaffoldedMigration == null)
                {
                    AppDomain.CurrentDomain.SetData("result.IsNull", true);
                }
                else
                {
                    AppDomain.CurrentDomain.SetData("result.IsNull", false);
                    AppDomain.CurrentDomain.SetData("result.DesignerCode", scaffoldedMigration.DesignerCode);
                    AppDomain.CurrentDomain.SetData("result.Language", scaffoldedMigration.Language);
                    AppDomain.CurrentDomain.SetData("result.MigrationId", scaffoldedMigration.MigrationId);
                    AppDomain.CurrentDomain.SetData("result.UserCode", scaffoldedMigration.UserCode);
                    AppDomain.CurrentDomain.SetData("result.Folder", scaffoldedMigration.Directory);
                }
            }

            protected virtual ScaffoldedMigration Scaffold(MigrationScaffolder scaffolder)
            {
                return scaffolder.Scaffold(MigrationName, IgnoreChanges);
            }

            protected override void OverrideConfiguration(DbMigrationsConfiguration configuration)
            {
                base.OverrideConfiguration(configuration);

                // If the user hasn't set their own generator and their using a VB project then switch in the default VB one
                if (Language == "vb" && configuration.CodeGenerator is CSharpMigrationCodeGenerator)
                {
                    configuration.CodeGenerator = new VisualBasicMigrationCodeGenerator();
                }
            }
        }

        [Serializable]
        private class InitialCreateScaffoldRunner : ScaffoldRunner
        {
            protected override ScaffoldedMigration Scaffold(MigrationScaffolder scaffolder)
            {
                return scaffolder.ScaffoldInitialCreate();
            }
        }

        [Serializable]
        private class GetContextTypesRunner : BaseRunner
        {
            protected override void RunCore()
            {
                var assembly = Assembly.Load(AssemblyName);

                var contextTypes = assembly.GetTypes()
                    .Where(t => typeof(DbContext).IsAssignableFrom(t)).Select(t => t.FullName)
                    .ToList();

                AppDomain.CurrentDomain.SetData("result", contextTypes);
            }
        }
    }
}