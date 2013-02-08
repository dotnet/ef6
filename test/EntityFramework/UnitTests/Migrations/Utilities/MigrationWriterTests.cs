// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Utilities
{
    using System.Collections;
    using System.Data.Entity.Migrations.Design;
    using System.IO;
    using System.Linq;
    using System.Resources;
    using EnvDTE;
    using Moq;
    using Xunit;

    public class MigrationWriterTests : IDisposable
    {
        private const string MigrationName = "InitialCreate";
        private const string MigrationId = "201206072128312_" + MigrationName;
        private const string Language = "cs";
        private const string MigrationsDirectory = "Migrations";
        private const string UserCodeFileName = MigrationId + "." + Language;
        private const string DesignerCodeFileName = MigrationId + ".Designer." + Language;
        private const string ResourcesFileName = MigrationId + ".resx";
        private const string UserCodePath = MigrationsDirectory + @"\" + UserCodeFileName;
        private const string DesignerCodePath = MigrationsDirectory + @"\" + DesignerCodeFileName;
        private const string ResourcesPath = MigrationsDirectory + @"\" + ResourcesFileName;
        private const string ResourceName = "MyResource";

        private readonly string _projectDir = IOHelpers.GetTempDirName();

        [Fact]
        public void Write_writes_artifacts()
        {
            TestWrite(
                (writer, scaffoldedMigration) =>
                writer.Write(scaffoldedMigration));
        }

        [Fact]
        public void Write_overwrites_designerCode_and_resources_when_rescaffolding()
        {
            CreateProject(
                "The user code.",
                "The old designer code.",
                "The old resource.");

            TestWrite(
                (writer, scaffoldedMigration) =>
                writer.Write(scaffoldedMigration, rescaffolding: true));
        }

        [Fact]
        public void Write_doesnt_overwrite_userCode_when_rescaffolding()
        {
            CreateProject(
                "The edited user code.",
                "The old designer code.",
                "The old resource.");

            TestWrite(
                (writer, scaffoldedMigration) =>
                writer.Write(scaffoldedMigration, rescaffolding: true, name: MigrationName),
                skipUserCodeVerification: true);

            var userCodePath = Path.Combine(_projectDir, UserCodePath);
            Assert.Equal("The edited user code.", File.ReadAllText(userCodePath));
        }

        [Fact]
        public void Write_overwrites_artifacts_when_rescaffolding_with_force()
        {
            CreateProject(
                "The edited user code.",
                "The old designer code.",
                "The old resource.");

            TestWrite(
                (writer, scaffoldedMigration) =>
                writer.Write(scaffoldedMigration, rescaffolding: true, force: true));
        }

        public void Dispose()
        {
            Directory.Delete(_projectDir, recursive: true);
        }

        private void CreateProject(string userCode, string designerCode, string resource)
        {
            Directory.CreateDirectory(Path.Combine(_projectDir, MigrationsDirectory));

            var userCodePath = Path.Combine(_projectDir, UserCodePath);
            File.WriteAllText(userCodePath, userCode);

            var designerCodePath = Path.Combine(_projectDir, DesignerCodePath);
            File.WriteAllText(designerCodePath, designerCode);

            var resourcesPath = Path.Combine(_projectDir, ResourcesPath);

            using (var resourceWriter = new ResXResourceWriter(resourcesPath))
            {
                resourceWriter.AddResource(ResourceName, resource);
            }
        }

        private void TestWrite(
            Func<System.Data.Entity.Migrations.Utilities.MigrationWriter, ScaffoldedMigration, string> action,
            bool skipUserCodeVerification = false)
        {
            var command = CreateCommand(_projectDir);
            var writer = new System.Data.Entity.Migrations.Utilities.MigrationWriter(command);
            var scaffoldedMigration = new ScaffoldedMigration
                                          {
                                              MigrationId = MigrationId,
                                              Language = Language,
                                              Directory = MigrationsDirectory,
                                              UserCode = "The user code.",
                                              DesignerCode = "The designer code.",
                                              Resources =
                                                  {
                                                      { ResourceName, "The resource." }
                                                  }
                                          };

            var relativeUserCodePath = action(writer, scaffoldedMigration);

            Assert.Equal(UserCodePath, relativeUserCodePath);

            if (!skipUserCodeVerification)
            {
                var userCodePath = Path.Combine(_projectDir, UserCodePath);
                Assert.Equal("The user code.", File.ReadAllText(userCodePath));
            }

            var designerCodePath = Path.Combine(_projectDir, DesignerCodePath);
            Assert.Equal("The designer code.", File.ReadAllText(designerCodePath));

            var resourcesPath = Path.Combine(_projectDir, ResourcesPath);

            using (var reader = new ResXResourceReader(resourcesPath))
            {
                var resources = reader.Cast<DictionaryEntry>();

                Assert.Equal(1, resources.Count());
                Assert.Contains(new DictionaryEntry(ResourceName, "The resource."), resources);
            }
        }

        private static System.Data.Entity.Migrations.MigrationsDomainCommand CreateCommand(string projectDir)
        {
            var fullPathProperty = new Mock<Property>();
            fullPathProperty.SetupGet(p => p.Value).Returns(projectDir);

            var properties = new Mock<Properties>();
            properties.Setup(p => p.Item("FullPath")).Returns(fullPathProperty.Object);

            var dte = new Mock<DTE>();

            var projectItems = new Mock<ProjectItems>();
            projectItems.SetupGet(pi => pi.Kind).Returns(
                System.Data.Entity.Migrations.Extensions.ProjectExtensions.VsProjectItemKindPhysicalFolder);
            projectItems.Setup(pi => pi.AddFromDirectory(It.IsAny<string>())).Returns(
                () =>
                    {
                        var dirProjectItems = new Mock<ProjectItems>();

                        var dirProjectItem = new Mock<ProjectItem>();
                        dirProjectItem.SetupGet(pi => pi.ProjectItems).Returns(dirProjectItems.Object);

                        return dirProjectItem.Object;
                    });

            var project = new Mock<Project>();
            projectItems.SetupGet(pi => pi.Parent).Returns(() => project.Object);
            project.SetupGet(p => p.Properties).Returns(properties.Object);
            project.SetupGet(p => p.DTE).Returns(dte.Object);
            project.SetupGet(p => p.ProjectItems).Returns(projectItems.Object);

            var command = new Mock<System.Data.Entity.Migrations.MigrationsDomainCommand>();
            command.SetupGet(c => c.Project).Returns(project.Object);
            command.Setup(c => c.WriteWarning(It.IsAny<string>())).Callback(() => { });

            return command.Object;
        }
    }
}
