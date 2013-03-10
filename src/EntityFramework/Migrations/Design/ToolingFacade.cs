// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Design
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    ///     Helper class that is used by design time tools to run migrations related
    ///     commands that need to interact with an application that is being edited
    ///     in Visual Studio.
    ///     Because the application is being edited the assemblies need to
    ///     be loaded in a separate AppDomain to ensure the latest version
    ///     is always loaded.
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
        /// <param name="assemblyName"> The name of the assembly that contains the migrations configuration to be used. </param>
        /// <param name="configurationTypeName"> The namespace qualified name of migrations configuration to be used. </param>
        /// <param name="workingDirectory"> The working directory containing the compiled assemblies. </param>
        /// <param name="configurationFilePath"> The path of the config file from the startup project. </param>
        /// <param name="dataDirectory"> The path of the application data directory from the startup project. Typically the App_Data directory for web applications or the working directory for executables. </param>
        /// <param name="connectionStringInfo"> The connection to the database to be migrated. If null is supplied, the default connection for the context will be used. </param>
        [SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
        public ToolingFacade(
            string assemblyName,
            string configurationTypeName,
            string workingDirectory,
            string configurationFilePath,
            string dataDirectory,
            DbConnectionInfo connectionStringInfo)
        {
            Check.NotEmpty(assemblyName, "assemblyName");

            _assemblyName = assemblyName;
            _configurationTypeName = configurationTypeName;
            _connectionStringInfo = connectionStringInfo;

            var info = new AppDomainSetup
                {
                    ShadowCopyFiles = "true"
                };

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

        internal ToolingFacade()
        {
            // For testing
        }

        /// <summary>
        ///     Releases all unmanaged resources used by the facade.
        /// </summary>
        ~ToolingFacade()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Gets the fully qualified name of all types deriving from <see cref="DbContext" />.
        /// </summary>
        /// <returns> All context types found. </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IEnumerable<string> GetContextTypes()
        {
            var runner = new GetContextTypesRunner();
            ConfigureRunner(runner);

            Run(runner);

            return (IEnumerable<string>)_appDomain.GetData("result");
        }

        /// <summary>
        ///     Gets the fully qualified name of a type deriving from <see cref="DbContext" />.
        /// </summary>
        /// <param name="contextTypeName"> The name of the context type. If null, the single context type found in the assembly will be returned. </param>
        /// <returns> The context type found. </returns>
        public string GetContextType(string contextTypeName)
        {
            var runner = new GetContextTypeRunner
                {
                    ContextTypeName = contextTypeName
                };
            ConfigureRunner(runner);

            Run(runner);

            return (string)_appDomain.GetData("result");
        }

        /// <summary>
        ///     Gets a list of all migrations that have been applied to the database.
        /// </summary>
        /// <returns> Ids of applied migrations. </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual IEnumerable<string> GetDatabaseMigrations()
        {
            var runner = new GetDatabaseMigrationsRunner();
            ConfigureRunner(runner);

            Run(runner);

            return (IEnumerable<string>)_appDomain.GetData("result");
        }

        /// <summary>
        ///     Gets a list of all migrations that have not been applied to the database.
        /// </summary>
        /// <returns> Ids of pending migrations. </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual IEnumerable<string> GetPendingMigrations()
        {
            var runner = new GetPendingMigrationsRunner();
            ConfigureRunner(runner);

            Run(runner);

            return (IEnumerable<string>)_appDomain.GetData("result");
        }

        /// <summary>
        ///     Updates the database to the specified migration.
        /// </summary>
        /// <param name="targetMigration"> The Id of the migration to migrate to. If null is supplied, the database will be updated to the latest migration. </param>
        /// <param name="force"> Value indicating if data loss during automatic migration is acceptable. </param>
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
        /// <param name="sourceMigration"> The migration to update from. If null is supplied, a script to update the current database will be produced. </param>
        /// <param name="targetMigration"> The migration to update to. If null is supplied, a script to update to the latest migration will be produced. </param>
        /// <param name="force"> Value indicating if data loss during automatic migration is acceptable. </param>
        /// <returns> The generated SQL script. </returns>
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
        /// <param name="migrationName"> The name for the generated migration. </param>
        /// <param name="language"> The programming language of the generated migration. </param>
        /// <param name="rootNamespace"> The root namespace of the project the migration will be added to. </param>
        /// <param name="ignoreChanges"> Whether or not to include model changes. </param>
        /// <returns> The scaffolded migration. </returns>
        public virtual ScaffoldedMigration Scaffold(
            string migrationName, string language, string rootNamespace, bool ignoreChanges)
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

            return (ScaffoldedMigration)_appDomain.GetData("result");
        }

        /// <summary>
        ///     Scaffolds the initial code-based migration corresponding to a previously run database initializer.
        /// </summary>
        /// <param name="language"> The programming language of the generated migration. </param>
        /// <param name="rootNamespace"> The root namespace of the project the migration will be added to. </param>
        /// <returns> The scaffolded migration. </returns>
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

            return (ScaffoldedMigration)_appDomain.GetData("result");
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
        /// <param name="disposing">
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

        [SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
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

            [SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
            public void Run()
            {
                try
                {
                    RunCore();
                }
                catch (Exception ex)
                {
                    // Not ideal; not sure why exceptions won't just serialize straight across
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
                return new MigrationsConfigurationFinder(new TypeFinder(LoadAssembly())).FindMigrationsConfiguration(
                    null,
                    ConfigurationTypeName,
                    Error.AssemblyMigrator_NoConfiguration,
                    (assembly, types) => Error.AssemblyMigrator_MultipleConfigurations(assembly),
                    Error.AssemblyMigrator_NoConfigurationWithName,
                    Error.AssemblyMigrator_MultipleConfigurationsWithName);
            }

            protected Assembly LoadAssembly()
            {
                try
                {
                    return Assembly.Load(AssemblyName);
                }
                catch (FileNotFoundException ex)
                {
                    throw new MigrationsException(
                        Strings.ToolingFacade_AssemblyNotFound(ex.FileName),
                        ex);
                }
            }
        }

        [Serializable]
        private class GetDatabaseMigrationsRunner : BaseRunner
        {
            [SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
            protected override void RunCore()
            {
                var databaseMigrations = GetMigrator().GetDatabaseMigrations();

                AppDomain.CurrentDomain.SetData("result", databaseMigrations);
            }
        }

        [Serializable]
        private class GetPendingMigrationsRunner : BaseRunner
        {
            [SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
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

            [SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
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

            [SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
            protected override void RunCore()
            {
                var configuration = GetConfiguration();

                var scaffolder = new MigrationScaffolder(configuration);

                var @namespace = configuration.MigrationsNamespace;

                // Need to strip project namespace when generating code for VB projects 
                // (The VB compiler automatically prefixes the project namespace)
                if (Language == "vb"
                    && !string.IsNullOrWhiteSpace(RootNamespace))
                {
                    if (RootNamespace.EqualsIgnoreCase(@namespace))
                    {
                        @namespace = null;
                    }
                    else if (@namespace != null
                             && @namespace.StartsWith(RootNamespace + ".", StringComparison.OrdinalIgnoreCase))
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

                AppDomain.CurrentDomain.SetData("result", scaffoldedMigration);
            }

            protected virtual ScaffoldedMigration Scaffold(MigrationScaffolder scaffolder)
            {
                return scaffolder.Scaffold(MigrationName, IgnoreChanges);
            }

            protected override void OverrideConfiguration(DbMigrationsConfiguration configuration)
            {
                base.OverrideConfiguration(configuration);

                // If the user hasn't set their own generator and their using a VB project then switch in the default VB one
                if (Language == "vb"
                    && configuration.CodeGenerator is CSharpMigrationCodeGenerator)
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
            [SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
            protected override void RunCore()
            {
                var assembly = LoadAssembly();

                var contextTypes = assembly.GetAccessibleTypes()
                                           .Where(t => typeof(DbContext).IsAssignableFrom(t)).Select(t => t.FullName)
                                           .ToList();

                AppDomain.CurrentDomain.SetData("result", contextTypes);
            }
        }

        [Serializable]
        private class GetContextTypeRunner : BaseRunner
        {
            public string ContextTypeName { get; set; }

            [SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
            protected override void RunCore()
            {
                var contextType = new TypeFinder(LoadAssembly()).FindType(
                    typeof(DbContext),
                    ContextTypeName,
                    types => types.Where(t => !typeof(HistoryContext).IsAssignableFrom(t)),
                    Error.EnableMigrations_NoContext,
                    (assembly, types) =>
                        {
                            var message = new StringBuilder();
                            message.Append(Strings.EnableMigrations_MultipleContexts(assembly));

                            foreach (var type in types)
                            {
                                message.AppendLine();
                                message.Append(Strings.EnableMigrationsForContext(type.FullName));
                            }

                            return new MigrationsException(message.ToString());
                        },
                    Error.EnableMigrations_NoContextWithName,
                    Error.EnableMigrations_MultipleContextsWithName);

                AppDomain.CurrentDomain.SetData("result", contextType.FullName);
            }
        }
    }
}
