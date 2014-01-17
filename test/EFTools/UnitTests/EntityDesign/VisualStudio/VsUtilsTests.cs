// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.SqlServer;
    using System.IO;
    using System.Linq;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper;
    using Microsoft.VisualStudio.Shell.Design;
    using Microsoft.VisualStudio.Shell.Interop;
    using Moq;
    using UnitTests.TestHelpers;
    using VSLangProj;
    using VSLangProj80;
    using VsWebSite;
    using Xunit;

    public class VsUtilsTests
    {
        [Fact]
        public void GetProjectReferenceAssemblyNames_returns_references()
        {
            var project = MockDTE.CreateProject(
                new[]
                    {
                        MockDTE.CreateReference("System.Data.Entity", "4.0.0.0"),
                        MockDTE.CreateReference("EntityFramework", "5.0.0.0")
                    });

            var referenceAssemblyNames = VsUtils.GetProjectReferenceAssemblyNames(project).ToArray();

            Assert.Equal(2, referenceAssemblyNames.Count());
            Assert.Equal("EntityFramework", referenceAssemblyNames.Last().Key);
            Assert.Equal(new Version(5, 0, 0, 0), referenceAssemblyNames.Last().Value);
        }

        [Fact]
        public void GetProjectReferenceAssemblyNames_returns_references_for_websites()
        {
            var project = MockDTE.CreateWebSite(
                new[]
                    {
                        MockDTE.CreateAssemblyReference(
                            "System.Data.Entity, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
                        MockDTE.CreateAssemblyReference(
                            "EntityFramework, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
                    });

            var referenceAssemblyNames = VsUtils.GetProjectReferenceAssemblyNames(project).ToArray();

            Assert.Equal(2, referenceAssemblyNames.Count());
            Assert.Equal("EntityFramework", referenceAssemblyNames.Last().Key);
            Assert.Equal(new Version(5, 0, 0, 0), referenceAssemblyNames.Last().Value);
        }

        [Fact]
        public void GetProjectReferenceAssemblyNames_for_websites_ignores_references_with_badly_formed_strong_names()
        {
            var project = MockDTE.CreateWebSite(
                new[]
                    {
                        // Deliberately emptying the PublicKeyToken causes a situation where
                        // creating the AssemblyName throws. This test tests that the exception
                        // is ignored and the other 2 references are still returned.
                        // (See issue 1467).
                        MockDTE.CreateAssemblyReference(
                            "AspNet.ScriptManager.jQuery.UI.Combined, Version=1.8.20.0, Culture=neutral, PublicKeyToken="),
                        MockDTE.CreateAssemblyReference(
                            "System.Data.Entity, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
                        MockDTE.CreateAssemblyReference(
                            "EntityFramework, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
                    });

            var referenceAssemblyNames = VsUtils.GetProjectReferenceAssemblyNames(project).ToArray();

            Assert.Equal(2, referenceAssemblyNames.Count());
            Assert.Equal(
                0, referenceAssemblyNames.Where(
                    ran => ran.Key == "AspNet.ScriptManager.jQuery.UI.Combined").Count());
            Assert.Equal(
                1, referenceAssemblyNames.Where(
                    ran => ran.Key == "System.Data.Entity").Count());
            Assert.Equal(
                1, referenceAssemblyNames.Where(
                    ran => ran.Key == "EntityFramework").Count());
        }

        [Fact]
        public void GetProjectReferenceAssemblyNames_handles_duplicate_references()
        {
            var project = MockDTE.CreateWebSite(
                new[]
                    {
                        MockDTE.CreateAssemblyReference(
                            "System.Web.WebPages.Deployment, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"),
                        MockDTE.CreateAssemblyReference(
                            "System.Web.WebPages.Deployment, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35")
                    });

            var referenceAssemblyNames = VsUtils.GetProjectReferenceAssemblyNames(project).ToArray();

            Assert.Equal(2, referenceAssemblyNames.Count());
            Assert.Equal("System.Web.WebPages.Deployment", referenceAssemblyNames.Last().Key);
            Assert.Equal(new Version(2, 0, 0, 0), referenceAssemblyNames.Last().Value);
        }

        [Fact]
        public void AddProjectReference_adds_reference()
        {
            var vsReferences = new Mock<References>();
            var vsProject = new Mock<VSProject2>();
            vsProject.SetupGet(p => p.References).Returns(vsReferences.Object);

            var project = MockDTE.CreateProject();
            Mock.Get(project).Setup(p => p.Object).Returns(vsProject.Object);

            VsUtils.AddProjectReference(project, "System.Data");

            vsReferences.Verify(r => r.Add("System.Data"));
        }

        [Fact]
        public void AddProjectReference_adds_reference_for_websites()
        {
            var vsAssemblyReferences = new Mock<AssemblyReferences>();
            var vsWebSite = new Mock<VSWebSite>();
            vsWebSite.SetupGet(p => p.References).Returns(vsAssemblyReferences.Object);
            var project = MockDTE.CreateWebSite();
            Mock.Get(project).Setup(p => p.Object).Returns(vsWebSite.Object);

            VsUtils.AddProjectReference(project, "System.Data");

            vsAssemblyReferences.Verify(r => r.AddFromGAC("System.Data"));
        }

        [Fact]
        public void GetInstalledEntityFrameworkAssemblyVersion_returns_null_when_none()
        {
            var project = MockDTE.CreateProject(Enumerable.Empty<Reference>());

            Assert.Null(VsUtils.GetInstalledEntityFrameworkAssemblyVersion(project));
        }

        [Fact]
        public void GetInstalledEntityFrameworkAssemblyVersion_returns_version_when_only_SDE()
        {
            var project = MockDTE.CreateProject(new[] { MockDTE.CreateReference("System.Data.Entity", "4.0.0.0") });

            Assert.Equal(new Version(4, 0, 0, 0), VsUtils.GetInstalledEntityFrameworkAssemblyVersion(project));
        }

        [Fact]
        public void GetInstalledEntityFrameworkAssemblyVersion_returns_EF_version_when_EF_and_SDE()
        {
            var project = MockDTE.CreateProject(
                new[]
                    {
                        MockDTE.CreateReference("System.Data.Entity", "4.0.0.0"),
                        MockDTE.CreateReference("EntityFramework", "5.0.0.0")
                    });

            Assert.Equal(RuntimeVersion.Version5Net45, VsUtils.GetInstalledEntityFrameworkAssemblyVersion(project));
        }

        [Fact]
        public void GetInstalledEntityFrameworkAssemblyVersion_returns_version_when_only_EF()
        {
            var project = MockDTE.CreateProject(new[] { MockDTE.CreateReference("EntityFramework", "6.0.0.0") });

            Assert.Equal(RuntimeVersion.Version6, VsUtils.GetInstalledEntityFrameworkAssemblyVersion(project));
        }

        [Fact]
        public void IsModernProviderAvailable_returns_true_for_known_providers()
        {
            Assert.True(
                VsUtils.IsModernProviderAvailable(
                    "System.Data.SqlClient",
                    Mock.Of<Project>(),
                    Mock.Of<IServiceProvider>()));
        }

        [Fact]
        public void IsMiscellaneousProject_detects_misc_files_project()
        {
            Assert.True(VsUtils.IsMiscellaneousProject(MockDTE.CreateMiscFilesProject()));
            Assert.False(VsUtils.IsMiscellaneousProject(MockDTE.CreateProject()));
        }

        [Fact]
        public void EntityFrameworkSupportedInProject_returns_true_for_applicable_projects()
        {
            var targets =
                new[]
                    {
                        ".NETFramework,Version=v4.0",
                        ".NETFramework,Version=v3.5",
                        ".NETFramework,Version=v4.5",
                    };

            foreach (var target in targets)
            {
                var monikerHelper = new MockDTE(target);

                Assert.True(
                    VsUtils.EntityFrameworkSupportedInProject(monikerHelper.Project, monikerHelper.ServiceProvider, allowMiscProject: true));
                Assert.True(
                    VsUtils.EntityFrameworkSupportedInProject(monikerHelper.Project, monikerHelper.ServiceProvider, allowMiscProject: false));
            }
        }

        [Fact]
        public void EntityFrameworkSupportedInProject_returns_true_for_Misc_project_if_allowed()
        {
            const string vsMiscFilesProjectUniqueName = "<MiscFiles>";

            var monikerHelper = new MockDTE("anytarget", vsMiscFilesProjectUniqueName);

            Assert.True(
                VsUtils.EntityFrameworkSupportedInProject(monikerHelper.Project, monikerHelper.ServiceProvider, allowMiscProject: true));
        }

        [Fact]
        public void EntityFrameworkSupportedInProject_returns_false_for_Misc_project_if_not_allowed()
        {
            const string vsMiscFilesProjectUniqueName = "<MiscFiles>";

            var monikerHelper = new MockDTE("anytarget", vsMiscFilesProjectUniqueName);

            Assert.False(
                VsUtils.EntityFrameworkSupportedInProject(monikerHelper.Project, monikerHelper.ServiceProvider, allowMiscProject: false));
        }

        [Fact]
        public void EntityFrameworkSupportedInProject_returns_false_for_project_where_EF_cannot_be_used()
        {
            var targets =
                new[]
                    {
                        ".NETFramework,Version=v3.0",
                        ".NETFramework,Version=v2.0",
                        ".XBox,Version=v4.5",
                        string.Empty,
                        null
                    };

            foreach (var target in targets)
            {
                var monikerHelper = new MockDTE(target);

                Assert.False(
                    VsUtils.EntityFrameworkSupportedInProject(monikerHelper.Project, monikerHelper.ServiceProvider, allowMiscProject: true));
                Assert.False(
                    VsUtils.EntityFrameworkSupportedInProject(monikerHelper.Project, monikerHelper.ServiceProvider, allowMiscProject: false));
            }
        }

        [Fact]
        public void SchemaVersionSupportedInProject_returns_true_for_all_versions_for_misc_project()
        {
            var serviceProvider = new Mock<IServiceProvider>().Object;

            Assert.True(
                VsUtils.SchemaVersionSupportedInProject(
                    MockDTE.CreateMiscFilesProject(), EntityFrameworkVersion.Version1, serviceProvider));

            Assert.True(
                VsUtils.SchemaVersionSupportedInProject(
                    MockDTE.CreateMiscFilesProject(), EntityFrameworkVersion.Version2, serviceProvider));

            Assert.True(
                VsUtils.SchemaVersionSupportedInProject(
                    MockDTE.CreateMiscFilesProject(), EntityFrameworkVersion.Version3, serviceProvider));
        }

        [Fact]
        public void SchemaVersionSupportedInProject_returns_true_for_v1_and_NetFramework_3_5_otherwise_false()
        {
            var mockDte =
                new MockDTE(
                    ".NETFramework, Version=v3.5",
                    references: new[] { MockDTE.CreateReference("System.Data.Entity", "3.5.0.0") });

            Assert.True(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version1, mockDte.ServiceProvider));

            Assert.False(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version2, mockDte.ServiceProvider));

            Assert.False(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version3, mockDte.ServiceProvider));
        }

        [Fact]
        public void SchemaVersionSupportedInProject_returns_true_for_v2_and_NetFramework_4_otherwise_false()
        {
            var mockDte =
                new MockDTE(
                    ".NETFramework, Version=v4.0",
                    references: new[] { MockDTE.CreateReference("System.Data.Entity", "4.0.0.0") });

            Assert.False(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version1, mockDte.ServiceProvider));

            Assert.True(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version2, mockDte.ServiceProvider));

            Assert.False(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version3, mockDte.ServiceProvider));
        }

        [Fact]
        public void SchemaVersionSupportedInProject_returns_true_for_v1_and_NetFramework_3_5_if_EF_not_referenced_otherwise_false()
        {
            var mockDte =
                new MockDTE(".NETFramework, Version=v3.5", references: new Reference[0]);

            Assert.True(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version1, mockDte.ServiceProvider));

            Assert.False(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version2, mockDte.ServiceProvider));

            Assert.False(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version3, mockDte.ServiceProvider));
        }

        [Fact]
        public void
            SchemaVersionSupportedInProject_returns_true_for_v2_and_NetFramework_4_if_EF_not_referenced_otherwise_false()
        {
            var mockDte =
                new MockDTE(".NETFramework, Version=v4.0", references: new Reference[0]);

            Assert.False(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version1, mockDte.ServiceProvider));

            Assert.True(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version2, mockDte.ServiceProvider));

            Assert.False(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version3, mockDte.ServiceProvider));
        }

        [Fact]
        public void
            SchemaVersionSupportedInProject_returns_true_for_v3_and_NetFramework_4_5_if_EF_not_referenced_otherwise_false()
        {
            var mockDte =
                new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]);

            Assert.False(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version1, mockDte.ServiceProvider));

            Assert.False(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version2, mockDte.ServiceProvider));

            Assert.True(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version3, mockDte.ServiceProvider));
        }

        [Fact]
        public void SchemaVersionSupportedInProject_returns_true_for_v2_and_NetFramework_4_and_EF5_otherwise_false()
        {
            var mockDte =
                new MockDTE(
                    ".NETFramework, Version=v4.0",
                    references:
                        new[]
                            {
                                MockDTE.CreateReference("System.Data.Entity", "4.0.0.0"),
                                MockDTE.CreateReference("EntityFramework", "4.4.0.0")
                            });

            Assert.False(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version1, mockDte.ServiceProvider));

            Assert.True(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version2, mockDte.ServiceProvider));

            Assert.False(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version3, mockDte.ServiceProvider));
        }

        [Fact]
        public void SchemaVersionSupportedInProject_returns_true_for_v2_and_v3_if_EF4_and_EF6_present()
        {
            var mockDte =
                new MockDTE(
                    ".NETFramework, Version=v4.0",
                    references:
                        new[]
                            {
                                MockDTE.CreateReference("System.Data.Entity", "4.0.0.0"),
                                MockDTE.CreateReference("EntityFramework", "6.0.0.0")
                            });

            Assert.False(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version1, mockDte.ServiceProvider));

            Assert.True(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version2, mockDte.ServiceProvider));

            Assert.True(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version3, mockDte.ServiceProvider));
        }

        [Fact]
        public void SchemaVersionSupportedInProject_returns_true_for_v3_and_EF5_on_NetFramework_4_5_otherwise_false()
        {
            var mockDte =
                new MockDTE(
                    ".NETFramework, Version=v4.5",
                    references: new[] { MockDTE.CreateReference("System.Data.Entity", "4.0.0.0") });

            Assert.False(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version1, mockDte.ServiceProvider));

            Assert.False(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version2, mockDte.ServiceProvider));

            Assert.True(
                VsUtils.SchemaVersionSupportedInProject(
                    mockDte.Project, EntityFrameworkVersion.Version3, mockDte.ServiceProvider));
        }

        [Fact]
        public void SchemaVersionSupportedInProject_returns_true_for_v3_and_EF6_otherwise_false()
        {
            var targetNetFrameworkVersions =
                new[] { ".NETFramework, Version=v4.0", ".NETFramework, Version=v4.5" };

            foreach (var targetNetFrameworkVersion in targetNetFrameworkVersions)
            {
                var mockDte =
                    new MockDTE(
                        targetNetFrameworkVersion,
                        references: new[] { MockDTE.CreateReference("EntityFramework", "6.0.0.0") });

                Assert.False(
                    VsUtils.SchemaVersionSupportedInProject(
                        mockDte.Project, EntityFrameworkVersion.Version1, mockDte.ServiceProvider));

                Assert.False(
                    VsUtils.SchemaVersionSupportedInProject(
                        mockDte.Project, EntityFrameworkVersion.Version2, mockDte.ServiceProvider));

                Assert.True(
                    VsUtils.SchemaVersionSupportedInProject(
                        mockDte.Project, EntityFrameworkVersion.Version3, mockDte.ServiceProvider));
            }
        }

        [Fact]
        public void GetProjectKind_when_csharp()
        {
            var project = MockDTE.CreateProject(kind: MockDTE.CSharpProjectKind);

            Assert.Equal(VsUtils.ProjectKind.CSharp, VsUtils.GetProjectKind(project));
        }

        [Fact]
        public void GetProjectKind_when_vb()
        {
            var project = MockDTE.CreateProject(kind: MockDTE.VBProjectKind);

            Assert.Equal(VsUtils.ProjectKind.VB, VsUtils.GetProjectKind(project));
        }

        [Fact]
        public void GetProjectKind_when_web()
        {
            Assert.Equal(VsUtils.ProjectKind.Web, VsUtils.GetProjectKind(MockDTE.CreateWebSite()));
        }

        [Fact]
        public void GetProjectKind_when_unknown()
        {
            var project = MockDTE.CreateProject(kind: null);

            Assert.Equal(VsUtils.ProjectKind.Unknown, VsUtils.GetProjectKind(project));
        }

        [Fact]
        public void IsWebSiteProject_when_web()
        {
            Assert.True(VsUtils.IsWebSiteProject(MockDTE.CreateWebSite()));
        }

        [Fact]
        public void IsWebSiteProject_when_not_web()
        {
            Assert.False(VsUtils.IsWebSiteProject(MockDTE.CreateProject()));
        }

        [Fact]
        public void GetProjectRoot_returns_solution_dir_when_misc_project()
        {
            var solution = new Mock<IVsSolution>();
            var solutionDirectory = @"C:\Path\To\Solution\";
            string temp;
            solution.Setup(s => s.GetSolutionInfo(out solutionDirectory, out temp, out temp));
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(p => p.GetService(typeof(IVsSolution))).Returns(solution.Object);

            var root = VsUtils.GetProjectRoot(MockDTE.CreateMiscFilesProject(), serviceProvider.Object);

            Assert.Equal(solutionDirectory, root.FullName);
        }

        [Fact]
        public void GetProjectRoot_returns_fullpath_when_csharp_project()
        {
            const string fullPath = @"C:\Path\To\Project\";
            var project = MockDTE.CreateProject(
                properties: new Dictionary<string, object> { { "FullPath", fullPath } });

            var root = VsUtils.GetProjectRoot(project, Mock.Of<IServiceProvider>());

            Assert.Equal(fullPath, root.FullName);
        }

        [Fact]
        public void GetProjectRoot_returns_fullpath_when_website()
        {
            const string fullPath = @"C:\Path\To\WebSite";
            var project = MockDTE.CreateWebSite(
                properties: new Dictionary<string, object> { { "FullPath", fullPath } });

            var root = VsUtils.GetProjectRoot(project, Mock.Of<IServiceProvider>());

            Assert.Equal(fullPath + Path.DirectorySeparatorChar, root.FullName);
        }

        [Fact]
        public void GetProjectTargetDir_returns_dir_when_csharp_project()
        {
            var project = MockDTE.CreateProject(
                properties: new Dictionary<string, object> { { "FullPath", @"C:\Path\To\Project\" } },
                configurationProperties: new Dictionary<string, object> { { "OutputPath", @"bin\Debug\" } });

            var targetDir = VsUtils.GetProjectTargetDir(project, Mock.Of<IServiceProvider>());

            Assert.Equal(@"C:\Path\To\Project\bin\Debug\", targetDir);
        }

        [Fact]
        public void GetProjectTargetDir_returns_dir_when_website()
        {
            var project = MockDTE.CreateWebSite(
                properties: new Dictionary<string, object> { { "FullPath", @"C:\Path\To\WebSite" } });

            var targetDir = VsUtils.GetProjectTargetDir(project, Mock.Of<IServiceProvider>());

            Assert.Equal(@"C:\Path\To\WebSite\Bin\", targetDir);
        }

        [Fact]
        public void GetProjectTargetFileName_returns_name()
        {
            var project = MockDTE.CreateProject(
                properties: new Dictionary<string, object> { { "OutputFileName", "ConsoleApplication1.exe" } });

            Assert.Equal("ConsoleApplication1.exe", VsUtils.GetProjectTargetFileName(project));
        }

        [Fact]
        public void GetProjectTargetFileName_returns_null_when_nonstring()
        {
            var project = MockDTE.CreateProject(
                properties: new Dictionary<string, object> { { "OutputFileName", 42 } });

            Assert.Null(VsUtils.GetProjectTargetFileName(project));
        }

        [Fact]
        public void GetProjectTargetFileName_returns_null_when_no_property()
        {
            var project = MockDTE.CreateProject(properties: new Dictionary<string, object>());

            Assert.Null(VsUtils.GetProjectTargetFileName(project));
        }

        [Fact]
        public void GetTypeFromProject_uses_dynamic_type_service()
        {
            var dte = new MockDTE(".NETFramework,Version=v4.5");

            var typeResolutionService = new Mock<ITypeResolutionService>();
            var dynamicTypeService = new Mock<DynamicTypeService>(MockBehavior.Strict);
            dynamicTypeService.Setup(s => s.GetTypeResolutionService(It.IsAny<IVsHierarchy>(), It.IsAny<uint>()))
                .Returns(typeResolutionService.Object);
            var serviceProvider = Mock.Get(dte.ServiceProvider);
            serviceProvider.Setup(p => p.GetService(typeof(DynamicTypeService))).Returns(dynamicTypeService.Object);

            VsUtils.GetTypeFromProject("Some.Type", dte.Project, dte.ServiceProvider);

            dynamicTypeService.Verify(s => s.GetTypeResolutionService(dte.Hierarchy, It.IsAny<uint>()));
            typeResolutionService.Verify(s => s.GetType("Some.Type"));
        }

        [Fact]
        public void EnsureProvider_unregisters_provider_when_useLegacyProvider()
        {
            DependencyResolver.RegisterProvider(typeof(SqlProviderServices), "System.Data.SqlClient");

            VsUtils.EnsureProvider("System.Data.SqlClient", true, Mock.Of<Project>(), Mock.Of<IServiceProvider>());

            Assert.IsType<LegacyDbProviderServicesWrapper>(
                DependencyResolver.GetService<DbProviderServices>("System.Data.SqlClient"));
        }

        [Fact]
        public void EnsureProvider_registers_provider_when_not_useLegacyProvider()
        {
            VsUtils.EnsureProvider("System.Data.SqlClient", false, Mock.Of<Project>(), Mock.Of<IServiceProvider>());
            try
            {
                Assert.Same(
                    SqlProviderServices.Instance,
                    DependencyResolver.GetService<DbProviderServices>("System.Data.SqlClient"));
            }
            finally
            {
                DependencyResolver.UnregisterProvider("System.Data.SqlClient");
            }
        }

        [Fact]
        public void GetProjectItemByPath_returns_item()
        {
            var projectItem = new Mock<ProjectItem>();
            var projectItems = new Mock<ProjectItems>();
            projectItems.Setup(i => i.Item("Class1.cs")).Returns(projectItem.Object);
            var project = new Mock<Project>();
            project.SetupGet(p => p.ProjectItems).Returns(projectItems.Object);

            var result = VsUtils.GetProjectItemByPath(project.Object, "Class1.cs");

            Assert.Same(projectItem.Object, result);
        }

        [Fact]
        public void GetProjectItemByPath_returns_item_when_nested()
        {
            var fileProjectItem = new Mock<ProjectItem>();
            var directoryProjectItems = new Mock<ProjectItems>();
            directoryProjectItems.Setup(i => i.Item("Class1.cs")).Returns(fileProjectItem.Object);
            var directoryProjectItem = new Mock<ProjectItem>();
            directoryProjectItem.SetupGet(i => i.ProjectItems).Returns(directoryProjectItems.Object);
            var projectItems = new Mock<ProjectItems>();
            projectItems.Setup(i => i.Item("Model")).Returns(directoryProjectItem.Object);
            var project = new Mock<Project>();
            project.SetupGet(p => p.ProjectItems).Returns(projectItems.Object);

            var result = VsUtils.GetProjectItemByPath(project.Object, @"Model\Class1.cs");

            Assert.Same(fileProjectItem.Object, result);
        }

        [Fact]
        public void GetProjectItemByPath_returns_null_when_error()
        {
            var projectItems = new Mock<ProjectItems>();
            projectItems.Setup(i => i.Item(It.IsAny<object>())).Throws<Exception>();
            var project = new Mock<Project>();
            project.SetupGet(p => p.ProjectItems).Returns(projectItems.Object);

            var result = VsUtils.GetProjectItemByPath(project.Object, "Class1.cs");

            Assert.Null(result);
        }
    }
}
