// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System.IO;
    using UnitTests.TestHelpers;
    using Xunit;

    public class DatabaseGenerationAssemblyLoaderTests
    {
        [Fact]
        public void AssemblyLoader_passed_null_project_can_find_paths_for_standard_DLLs_case_insensitive()
        {
            const string vsInstallPath = @"C:\My\Test\VS\InstallPath";
            var assemblyLoader = new DatabaseGenerationAssemblyLoader(null, vsInstallPath);
            Assert.Equal(
                Path.Combine(vsInstallPath, "EntityFramework.dll"),
                assemblyLoader.GetAssemblyPath("EntityFramework"));
            Assert.Equal(
                Path.Combine(vsInstallPath, "EntityFramework.dll"),
                assemblyLoader.GetAssemblyPath("entityframework"));
            Assert.Equal(
                Path.Combine(vsInstallPath, "EntityFramework.dll"),
                assemblyLoader.GetAssemblyPath("ENTITYFRAMEWORK"));
            Assert.Equal(
                Path.Combine(vsInstallPath, "EntityFramework.SqlServer.dll"),
                assemblyLoader.GetAssemblyPath("EntityFramework.SqlServer"));
            Assert.Equal(
                Path.Combine(vsInstallPath, "EntityFramework.SqlServer.dll"),
                assemblyLoader.GetAssemblyPath("entityframework.sqlserver"));
            Assert.Equal(
                Path.Combine(vsInstallPath, "EntityFramework.SqlServer.dll"),
                assemblyLoader.GetAssemblyPath("ENTITYFRAMEWORK.SQLSERVER"));
            Assert.Equal(
                Path.Combine(vsInstallPath, "EntityFramework.SqlServerCompact.dll"),
                assemblyLoader.GetAssemblyPath("EntityFramework.SqlServerCompact"));
            Assert.Equal(
                Path.Combine(vsInstallPath, "EntityFramework.SqlServerCompact.dll"),
                assemblyLoader.GetAssemblyPath("entityframework.sqlservercompact"));
            Assert.Equal(
                Path.Combine(vsInstallPath, "EntityFramework.SqlServerCompact.dll"),
                assemblyLoader.GetAssemblyPath("ENTITYFRAMEWORK.SQLSERVERCOMPACT"));
        }

        [Fact]
        public void AssemblyLoader_passed_non_WebsiteProject_can_find_correct_paths_to_DLLs()
        {
            const string vsInstallPath = @"C:\My\Test\VS\InstallPath";
            const string projectPath = @"C:\My\Test\ProjectPath";
            var project =
                MockDTE.CreateProject(
                    new[]
                        {
                            MockDTE.CreateReference3(
                                "EntityFramework", "6.0.0.0", "EntityFramework",
                                Path.Combine(projectPath, "EntityFramework.dll")),
                            MockDTE.CreateReference3(
                                "EntityFramework.SqlServer", "6.0.0.0", "EntityFramework.SqlServer",
                                Path.Combine(projectPath, "EntityFramework.SqlServer.dll")),
                            MockDTE.CreateReference3(
                                "EntityFramework.SqlServerCompact", "6.0.0.0",
                                "EntityFramework.SqlServerCompact",
                                Path.Combine(projectPath, "EntityFramework.SqlServerCompact.dll")),
                            MockDTE.CreateReference3(
                                "My.Project.Reference", "6.0.0.0", "My.Project.Reference",
                                Path.Combine(projectPath, "My.Project.Reference.dll"), true)
                        });
            var assemblyLoader = new DatabaseGenerationAssemblyLoader(project, vsInstallPath);

            // assert that the DLLs installed under VS are resolved there
            Assert.Equal(
                Path.Combine(vsInstallPath, "EntityFramework.dll"),
                assemblyLoader.GetAssemblyPath("EntityFramework"));
            Assert.Equal(
                Path.Combine(vsInstallPath, "EntityFramework.SqlServer.dll"),
                assemblyLoader.GetAssemblyPath("EntityFramework.SqlServer"));
            Assert.Equal(
                Path.Combine(vsInstallPath, "EntityFramework.SqlServerCompact.dll"),
                assemblyLoader.GetAssemblyPath("EntityFramework.SqlServerCompact"));

            // assert that other project references are resolved to wherever their reference points to
            Assert.Equal(
                Path.Combine(projectPath, "My.Project.Reference.dll"),
                assemblyLoader.GetAssemblyPath("My.Project.Reference"));
        }

        [Fact]
        public void AssemblyLoader_passed_WebsiteProject_can_find_correct_paths_to_DLLs()
        {
            const string vsInstallPath = @"C:\My\Test\VS\InstallPath";
            const string projectPath = @"C:\My\Test\WebsitePath";
            var project =
                MockDTE.CreateWebSite(
                    new[]
                        {
                            MockDTE.CreateAssemblyReference(
                                "EntityFramework, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                                Path.Combine(projectPath, "EntityFramework.dll")),
                            MockDTE.CreateAssemblyReference(
                                "EntityFramework.SqlServer, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                                Path.Combine(projectPath, "EntityFramework.SqlServer.dll")),
                            MockDTE.CreateAssemblyReference(
                                "EntityFramework.SqlServerCompact, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                                Path.Combine(projectPath, "EntityFramework.SqlServerCompact.dll")),
                            MockDTE.CreateAssemblyReference(
                                "My.WebsiteProject.Reference, Version=4.1.0.0, Culture=neutral, PublicKeyToken=bbbbbbbbbbbbbbbb",
                                Path.Combine(projectPath, "My.WebsiteProject.Reference.dll"))
                        });
            var assemblyLoader = new DatabaseGenerationAssemblyLoader(project, vsInstallPath);

            // assert that the DLLs installed under VS are resolved there
            Assert.Equal(
                Path.Combine(vsInstallPath, "EntityFramework.dll"),
                assemblyLoader.GetAssemblyPath("EntityFramework"));
            Assert.Equal(
                Path.Combine(vsInstallPath, "EntityFramework.SqlServer.dll"),
                assemblyLoader.GetAssemblyPath("EntityFramework.SqlServer"));
            Assert.Equal(
                Path.Combine(vsInstallPath, "EntityFramework.SqlServerCompact.dll"),
                assemblyLoader.GetAssemblyPath("EntityFramework.SqlServerCompact"));

            // assert that other project references are resolved to wherever their reference points to
            Assert.Equal(
                Path.Combine(projectPath, "My.WebsiteProject.Reference.dll"),
                assemblyLoader.GetAssemblyPath("My.WebsiteProject.Reference"));
        }
    }
}
