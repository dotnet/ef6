namespace System.Data.Entity.Migrations
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Resources;
    using System.Data.Entity.SqlServer;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.CSharp;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    public class ToolingScenarios : DbTestCase, IUseFixture<ToolingFixture>
    {
        private string _projectDir;

        [MigrationsTheory]
        public void Can_update()
        {
            ResetDatabase();

            var logBuilder = new StringBuilder();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
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

        [MigrationsTheory]
        public void Can_script_update()
        {
            ResetDatabase();

            var logBuilder = new StringBuilder();
            string sql;

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
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

        [MigrationsTheory]
        public void Can_get_context_types()
        {
            ResetDatabase();

            var logBuilder = new StringBuilder();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
                configurationTypeName: null,
                workingDirectory: _projectDir,
                configurationFilePath: null,
                dataDirectory: null,
                connectionStringInfo: null))
            {
                var result = facade.GetContextTypes();

                Assert.Equal(1, result.Count());
                Assert.Equal("ClassLibrary1.Context", result.Single());
            }
        }

        [MigrationsTheory]
        public void Can_get_context_type()
        {
            ResetDatabase();

            var logBuilder = new StringBuilder();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
                configurationTypeName: null,
                workingDirectory: _projectDir,
                configurationFilePath: null,
                dataDirectory: null,
                connectionStringInfo: null))
            {
                var result = facade.GetContextType(null);

                Assert.Equal("ClassLibrary1.Context", result);
            }
        }

        [MigrationsTheory]
        public void Can_get_context_type_by_name()
        {
            ResetDatabase();

            var logBuilder = new StringBuilder();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
                configurationTypeName: null,
                workingDirectory: _projectDir,
                configurationFilePath: null,
                dataDirectory: null,
                connectionStringInfo: null))
            {
                var result = facade.GetContextType("Context");

                Assert.Equal("ClassLibrary1.Context", result);
            }
        }

        [MigrationsTheory]
        public void Can_get_context_type_by_qualified_name()
        {
            ResetDatabase();

            var logBuilder = new StringBuilder();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
                configurationTypeName: null,
                workingDirectory: _projectDir,
                configurationFilePath: null,
                dataDirectory: null,
                connectionStringInfo: null))
            {
                var result = facade.GetContextType("ClassLibrary1.Context");

                Assert.Equal("ClassLibrary1.Context", result);
            }
        }

        [MigrationsTheory]
        public void Throws_when_context_type_not_found()
        {
            ResetDatabase();

            var logBuilder = new StringBuilder();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
                configurationTypeName: null,
                workingDirectory: _projectDir,
                configurationFilePath: null,
                dataDirectory: null,
                connectionStringInfo: null))
            {
                var ex = Assert.Throws<ToolingException>(
                    () => facade.GetContextType("MissingContext"));
            }
        }

        [MigrationsTheory]
        public void Can_scaffold_initial_create()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            var initialCreate = new MigrationScaffolder(migrator.Configuration).Scaffold("InitialCreate");

            migrator = CreateMigrator<ShopContext_v1>(scaffoldedMigrations: initialCreate);

            migrator.Update();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
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

        [MigrationsTheory]
        public void Can_scaffold_empty()
        {
            using (var facade = new ToolingFacade(
                "ClassLibrary1",
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

        [MigrationsTheory]
        public void Can_scaffold()
        {
            ResetDatabase();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
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

        [MigrationsTheory]
        public void Can_scaffold_vb()
        {
            ResetDatabase();

            using (var facade = new ToolingFacade(
                "ClassLibrary1",
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

        [MigrationsTheory]
        public void Wraps_assembly_not_found_exceptions()
        {
            const string unknownAssemblyName = "UnknownAssembly";

            using (var facade = new ToolingFacade(
                    unknownAssemblyName,
                    "ClassLibrary1.Configuration",
                    _projectDir,
                    Path.Combine(_projectDir, "App.config"),
                    null,
                    null))
            {
                var ex = Assert.Throws<ToolingException>(() => facade.GetDatabaseMigrations());
                Assert.Equal(Strings.ToolingFacade_AssemblyNotFound(unknownAssemblyName), ex.Message);
            }
        }

        public void SetFixture(ToolingFixture data)
        {
            _projectDir = data.ProjectDir;
        }
    }

    public class ToolingFixture : IDisposable
    {
        public string ProjectDir { get; private set; }

        public ToolingFixture()
        {
            var targetDir = IOHelpers.GetTempDirName();
            var targetName = "ClassLibrary1";
            var targetFileName = targetName + ".dll";
            var targetPath = Path.Combine(targetDir, targetFileName);

            var entityFrameworkPath = new Uri(typeof(DbContext).Assembly.CodeBase).LocalPath;
            IOHelpers.CopyToDir(entityFrameworkPath, targetDir);

            var entityFrameworkSqlServerPath = new Uri(typeof(SqlProviderServices).Assembly.CodeBase).LocalPath;
            IOHelpers.CopyToDir(entityFrameworkSqlServerPath, targetDir);

            using (var compiler = new CSharpCodeProvider())
            {
                var results
                    = compiler.CompileAssemblyFromSource(
                        new CompilerParameters(
                            new[]
                                    {
                                        "System.dll",
                                        "System.Core.dll",
                                        "System.Data.Entity.dll",
                                        entityFrameworkPath
                                    },
                            targetPath),
                        @"namespace ClassLibrary1
{
    using System.Data.Entity;
    using System.Data.Entity.Migrations;

    public class Configuration : DbMigrationsConfiguration<Context>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }
    }

    public class Context : DbContext
    {
        public Context()
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

                if (results.Errors.HasErrors)
                {
                    throw new InvalidOperationException(results.Errors.Cast<CompilerError>().First(e => !e.IsWarning).ToString());
                }
            }

            var configurationFile = Path.Combine(targetDir, "App.config");

            File.WriteAllText(
                configurationFile,
                @"<?xml version='1.0' encoding='utf-8' ?>
<configuration>
  <connectionStrings>
    <add name='ClassLibrary1' connectionString='" +
        DatabaseProviderFixture.InitializeTestDatabase(DatabaseProvider.SqlClient, DatabaseProviderFixture.DefaultDatabaseName).ConnectionString +
                                                  @"' providerName='System.Data.SqlClient' />
  </connectionStrings>
</configuration>");

            ProjectDir = targetDir;
        }

        public void Dispose()
        {
            if (ProjectDir != null
                && Directory.Exists(ProjectDir))
            {
                Directory.Delete(ProjectDir, true);
            }
        }
    }
}