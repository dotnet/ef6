// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Data.Entity.Functionals.Utilities;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.SqlServer;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.CSharp;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    public class ToolingScenarios : DbTestCase, IClassFixture<ToolingFixture>
    {
        private string _projectDir;
        private string _contextDir;

        [MigrationsTheory(SlowGroup = TestGroup.MigrationsTests, Skip = "Fails when delay signed")]
        public void Can_update()
        {
            ResetDatabase();

            var logBuilder = new StringBuilder();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
                "ContextLibrary1",
                "ClassLibrary1.Configuration",
                _projectDir,
                Path.Combine(_projectDir, "App.config"),
                null,
                null))
            {
                facade.LogInfoDelegate = m => logBuilder.AppendLine("INFO: " + m);
                facade.LogVerboseDelegate = s => logBuilder.AppendLine("SQL: " + s);

                facade.Update(null, false);
            }

            var log = logBuilder.ToString();
            Assert.True(log.Contains("INFO: Applying automatic migration"));
            Assert.True(log.Contains("SQL: CREATE TABLE [dbo].[Entities]"));

            Assert.True(DatabaseExists());
            Assert.True(TableExists("Entities"));
        }

        [MigrationsTheory(SlowGroup = TestGroup.MigrationsTests, Skip = "Fails when delay signed")]
        public void Can_script_update()
        {
            ResetDatabase();

            var logBuilder = new StringBuilder();
            string sql;

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
                "ContextLibrary1",
                "ClassLibrary1.Configuration",
                _projectDir,
                Path.Combine(_projectDir, "App.config"),
                null,
                null))
            {
                facade.LogInfoDelegate = m => logBuilder.AppendLine("INFO: " + m);

                sql = facade.ScriptUpdate(null, null, false);
            }

            Assert.True(sql.Contains("CREATE TABLE [dbo].[Entities]"));

            var log = logBuilder.ToString();
            Assert.True(log.Contains("INFO: Applying automatic migration"));
        }

        [MigrationsTheory(Skip = "Fails when delay signed")]
        public void Can_get_context_types()
        {
            ResetDatabase();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
                "ContextLibrary1",
                configurationTypeName: null,
                workingDirectory: _contextDir,
                configurationFilePath: null,
                dataDirectory: null,
                connectionStringInfo: null))
            {
                var result = facade.GetContextTypes();

                Assert.Equal(1, result.Count());
                Assert.Equal("ContextLibrary1.Context", result.First());
            }
        }

        [MigrationsTheory(Skip = "Fails when delay signed")]
        public void Can_get_context_type()
        {
            ResetDatabase();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
                "ContextLibrary1",
                configurationTypeName: null,
                workingDirectory: _contextDir,
                configurationFilePath: null,
                dataDirectory: null,
                connectionStringInfo: null))
            {
                var result = facade.GetContextType(null);

                Assert.Equal("ContextLibrary1.Context", result);
            }
        }

        [MigrationsTheory(Skip = "Fails when delay signed")]
        public void Can_get_context_type_by_name()
        {
            ResetDatabase();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
                "ContextLibrary1",
                configurationTypeName: null,
                workingDirectory: _contextDir,
                configurationFilePath: null,
                dataDirectory: null,
                connectionStringInfo: null))
            {
                var result = facade.GetContextType("Context");

                Assert.Equal("ContextLibrary1.Context", result);
            }
        }

        [MigrationsTheory(Skip = "Fails when delay signed")]
        public void Can_get_context_type_by_qualified_name()
        {
            ResetDatabase();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
                "ContextLibrary1",
                configurationTypeName: null,
                workingDirectory: _contextDir,
                configurationFilePath: null,
                dataDirectory: null,
                connectionStringInfo: null))
            {
                var result = facade.GetContextType("ContextLibrary1.Context");

                Assert.Equal("ContextLibrary1.Context", result);
            }
        }

        [MigrationsTheory(Skip = "Fails when delay signed")]
        public void Throws_when_context_type_not_found()
        {
            ResetDatabase();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
                "ContextLibrary1",
                configurationTypeName: null,
                workingDirectory: _contextDir,
                configurationFilePath: null,
                dataDirectory: null,
                connectionStringInfo: null))
            {
                Assert.Throws<ToolingException>(() => facade.GetContextType("MissingContext"));
            }
        }

        [MigrationsTheory(Skip = "Fails when delay signed")]
        public void Can_still_scaffold_generic_context_by_specifying_name_directly()
        {
            ResetDatabase();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
                "ContextLibrary1",
                configurationTypeName: null,
                workingDirectory: _contextDir,
                configurationFilePath: null,
                dataDirectory: null,
                connectionStringInfo: null))
            {
                var result = facade.GetContextType("GenericContext`1");
                Assert.Equal("ContextLibrary1.GenericContext`1", result);
            }
        }

        [MigrationsTheory(Skip = "Fails when delay signed")]
        public void Can_still_scaffold_abstract_context_by_specifying_name_directly()
        {
            ResetDatabase();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
                "ContextLibrary1",
                configurationTypeName: null,
                workingDirectory: _contextDir,
                configurationFilePath: null,
                dataDirectory: null,
                connectionStringInfo: null))
            {
                var result = facade.GetContextType("AbstractContext");
                Assert.Equal("ContextLibrary1.AbstractContext", result);
            }
        }

        [MigrationsTheory(SlowGroup = TestGroup.MigrationsTests, Skip = "Fails when delay signed")]
        public void Can_scaffold_initial_create()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v2>();

            var initialCreate = new MigrationScaffolder(migrator.Configuration).Scaffold("InitialCreate");

            migrator = CreateMigrator<ShopContext_v2>(scaffoldedMigrations: initialCreate, contextKey: "ContextLibrary1.Context");

            migrator.Update();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
                "ContextLibrary1",
                "ClassLibrary1.Configuration",
                _projectDir,
                Path.Combine(_projectDir, "App.config"),
                null,
                null))
            {
                var scaffoldedMigration = facade.ScaffoldInitialCreate("cs", "ClassLibrary1");

                Assert.True(scaffoldedMigration.DesignerCode.Length > 500);
                Assert.Equal("cs", scaffoldedMigration.Language);
                Assert.True(scaffoldedMigration.MigrationId.EndsWith("_InitialCreate"));
                Assert.True(scaffoldedMigration.UserCode.Length > 500);
            }
        }

        [MigrationsTheory(SlowGroup = TestGroup.MigrationsTests, Skip = "Fails when delay signed")]
        public void Can_scaffold_empty()
        {
            using (var facade = new ToolingFacade(
                "ClassLibrary1",
                "ContextLibrary1",
                "ClassLibrary1.Configuration",
                _projectDir,
                Path.Combine(_projectDir, "App.config"),
                null,
                null))
            {
                var scaffoldedMigration = facade.Scaffold("Create", "cs", "ClassLibrary1", ignoreChanges: true);

                Assert.True(scaffoldedMigration.DesignerCode.Length > 500);
                Assert.Equal("cs", scaffoldedMigration.Language);
                Assert.True(scaffoldedMigration.MigrationId.EndsWith("_Create"));
                Assert.True(scaffoldedMigration.UserCode.Length < 300);
            }
        }

        [MigrationsTheory(Skip = "Fails when delay signed")]
        public void Can_scaffold()
        {
            ResetDatabase();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
                "ContextLibrary1",
                "ClassLibrary1.Configuration",
                _projectDir,
                Path.Combine(_projectDir, "App.config"),
                null,
                null))
            {
                var scaffoldedMigration = facade.Scaffold("Create", "cs", "ClassLibrary1", ignoreChanges: false);

                Assert.True(scaffoldedMigration.DesignerCode.Length > 500);
                Assert.Equal("cs", scaffoldedMigration.Language);
                Assert.True(scaffoldedMigration.MigrationId.EndsWith("_Create"));
                Assert.True(scaffoldedMigration.UserCode.Length > 500);
            }
        }

        [MigrationsTheory(SlowGroup = TestGroup.MigrationsTests, Skip = "Fails when delay signed")]
        public void Can_scaffold_vb()
        {
            ResetDatabase();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
                "ContextLibrary1",
                "ClassLibrary1.Configuration",
                _projectDir,
                Path.Combine(_projectDir, "App.config"),
                null,
                null))
            {
                var scaffoldedMigration = facade.Scaffold("Create", "vb", "ClassLibrary1", ignoreChanges: false);

                Assert.True(scaffoldedMigration.DesignerCode.Length > 500);
                Assert.Equal("vb", scaffoldedMigration.Language);
                Assert.True(scaffoldedMigration.MigrationId.EndsWith("_Create"));
                Assert.True(scaffoldedMigration.UserCode.Length > 500);
            }
        }

        [MigrationsTheory(Skip = "Fails when delay signed")]
        public void Wraps_assembly_not_found_exceptions()
        {
            const string unknownAssemblyName = "UnknownAssembly";

            using (var facade = new ToolingFacade(
                unknownAssemblyName,
                unknownAssemblyName,
                "ClassLibrary1.Configuration",
                _projectDir,
                Path.Combine(_projectDir, "App.config"),
                null,
                null))
            {
                Assert.Throws<ToolingException>(() => facade.GetDatabaseMigrations())
                      .ValidateMessage("ToolingFacade_AssemblyNotFound", unknownAssemblyName);
            }
        }

        public ToolingScenarios(DatabaseProviderFixture databaseProviderFixture, ToolingFixture data)
            : base(databaseProviderFixture)
        {
            _projectDir = data.ProjectDir;
            _contextDir = data.ContextDir;
        }
    }

    public class ToolingFixture : IDisposable
    {
        public string ProjectDir { get; private set; }
        public string ContextDir { get; private set; }

        public ToolingFixture()
        {
            var contextDir = IOHelpers.GetTempDirName();
            CreateContextProject(contextDir);

            var targetDir = IOHelpers.GetTempDirName();
            CreateMigrationsProject(targetDir, contextDir);
            AddAppConfig(targetDir);

            ProjectDir = targetDir;
            ContextDir = contextDir;
        }

        private static void AddAppConfig(string targetDir)
        {
            var configurationFile = Path.Combine(targetDir, "App.config");

            File.WriteAllText(
                configurationFile,
                @"<?xml version='1.0' encoding='utf-8' ?>
<configuration>
  <connectionStrings>
    <add name='ClassLibrary1' connectionString='" +
                DatabaseProviderFixture.InitializeTestDatabase(DatabaseProvider.SqlClient, DatabaseProviderFixture.DefaultDatabaseName).
                                        ConnectionString +
                @"' providerName='System.Data.SqlClient' />
  </connectionStrings>
</configuration>");
        }

        private static void CreateContextProject(string targetDir)
        {
            CreateProject(
                targetDir, "ContextLibrary1", new List<string>(),
                @"namespace ContextLibrary1
{
    using System.Data.Entity;

    public class Context : DbContext
    {
        public Context()
            : base(""Name=ClassLibrary1"")
        {
        }

        public DbSet<Entity> Entities { get; set; }
    }

    public class GenericContext<TEntity> : DbContext where TEntity : class
    {
        public GenericContext()
            : base(""Name=ClassLibrary1"")
        {
        }

        public DbSet<TEntity> Entities { get; set; }
    }

    public abstract class AbstractContext : DbContext
    {
        public AbstractContext()
            : base(""Name=ClassLibrary1"")
        {
        }

        public DbSet<Entity> Entities { get; set; }
    } 

    public class Entity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}");
        }

        private static void CreateMigrationsProject(string targetDir, string contextDir)
        {
            var contextPath = Path.Combine(contextDir, "ContextLibrary1.dll");
            IOHelpers.CopyToDir(contextPath, targetDir);

            CreateProject(
                targetDir, "ClassLibrary1", new List<string> { contextPath },
                @"namespace ClassLibrary1
{
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.History;

    public class Configuration : DbMigrationsConfiguration<ContextLibrary1.Context>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }
    }

    public class CustomHistoryContext : HistoryContext
    {
        public CustomHistoryContext(DbConnection existingConnection, string defaultSchema)
            : base(existingConnection, defaultSchema)
        {
        }
    }
}");
        }

        private static void CreateProject(string targetDir, string targetName, List<string> additionalAssemblies, string code)
        {
            var targetFileName = targetName + ".dll";
            var targetPath = Path.Combine(targetDir, targetFileName);

            var entityFrameworkPath = new Uri(typeof(DbContext).Assembly().CodeBase).LocalPath;
            IOHelpers.CopyToDir(entityFrameworkPath, targetDir);

            var entityFrameworkSqlServerPath = new Uri(typeof(SqlProviderServices).Assembly().CodeBase).LocalPath;
            IOHelpers.CopyToDir(entityFrameworkSqlServerPath, targetDir);

            using (var compiler = new CSharpCodeProvider())
            {
                additionalAssemblies.AddRange(
                    new List<string>
                        {
                            "System.dll",
                            "System.Data.dll",
                            "System.Core.dll",
                            "System.Data.Entity.dll",
                            entityFrameworkPath
                        });

                var results = compiler.CompileAssemblyFromSource(new CompilerParameters(additionalAssemblies.ToArray(), targetPath), code);

                if (results.Errors.HasErrors)
                {
                    throw new InvalidOperationException(results.Errors.Cast<CompilerError>().First(e => !e.IsWarning).ToString());
                }
            }
        }

        public void Dispose()
        {
            if (ProjectDir != null
                && Directory.Exists(ProjectDir))
            {
                Directory.Delete(ProjectDir, true);
            }

            if (ContextDir != null
                && Directory.Exists(ContextDir))
            {
                Directory.Delete(ContextDir, true);
            }
        }
    }
}
