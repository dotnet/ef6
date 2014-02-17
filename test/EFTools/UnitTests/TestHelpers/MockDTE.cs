// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace UnitTests.TestHelpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using EnvDTE;
    using Microsoft.VisualStudio.Shell.Interop;
    using Moq;
    using VSLangProj;
    using VSLangProj80;
    using VsWebSite;
    using Constants = EnvDTE.Constants;

    internal class MockDTE
    {
        public const string CSharpProjectKind = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
        public const string VBProjectKind = "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}";
        public const string WebSiteProjectKind = "{E24C65DC-7377-472b-9ABA-BC803B73C61A}";

        private const int S_OK = 0;
        private const uint VSITEMID_ROOT = unchecked((uint)-2);
        private const int VSHPROPID_TargetFrameworkMoniker = -2102;
        private const int VSHPROPID_ExtObject = -2027;

        public readonly object TargetFrameworkMoniker;
        public readonly IVsHierarchy Hierarchy;
        public readonly IVsSolution Solution;
        public readonly Project Project;
        public readonly IServiceProvider ServiceProvider;

        private uint _projectItemId;

        public static Project CreateProject(
            IEnumerable<Reference> assemblyReferences = null,
            string kind = CSharpProjectKind,
            IDictionary<string, object> properties = null,
            IDictionary<string, object> configurationProperties = null)
        {
            return CreateProject(
                null,
                CreateVsProject2(assemblyReferences),
                kind,
                properties,
                configurationProperties);
        }

        public static Project CreateMiscFilesProject()
        {
            return CreateProject(Constants.vsMiscFilesProjectUniqueName, null, null, null, null);
        }

        public static Project CreateWebSite(
            IEnumerable<AssemblyReference> assemblyReferences = null,
            IDictionary<string, object> properties = null,
            IDictionary<string, object> configurationProperties = null)
        {
            return CreateProject(
                null,
                CreateVsWebsite(assemblyReferences),
                WebSiteProjectKind,
                properties,
                configurationProperties);
        }

        public static Reference CreateReference(string name, string version)
        {
            var reference = new Mock<Reference>();
            reference.SetupGet(r => r.Name).Returns(name);
            reference.SetupGet(r => r.Version).Returns(version);

            return reference.Object;
        }

        public static Reference3 CreateReference3(
            string name, string version, string identity = null, string path = null, bool isResolved = false)
        {
            var reference = new Mock<Reference3>();
            reference.SetupGet(r => r.Name).Returns(name);
            reference.SetupGet(r => r.Version).Returns(version);
            if (identity != null)
            {
                reference.SetupGet(r => r.Identity).Returns(identity);
            }
            if (path != null)
            {
                reference.SetupGet(r => r.Path).Returns(path);
            }
            reference.SetupGet(r => r.Resolved).Returns(isResolved);

            return reference.Object;
        }

        public static AssemblyReference CreateAssemblyReference(string strongName, string path = null)
        {
            var assemblyReference = new Mock<AssemblyReference>();
            assemblyReference.SetupGet(r => r.StrongName).Returns(strongName);
            if (path != null)
            {
                assemblyReference.SetupGet(r => r.FullPath).Returns(path);
            }

            return assemblyReference.Object;
        }

        public static Mock<ProjectItem> CreateProjectItem(string path)
        {
            var mockProjectItem = new Mock<ProjectItem>();

            var mockProperties = new Mock<Properties>() { CallBase = true};

            mockProjectItem.Setup(p => p.Properties).Returns(mockProperties.Object);

            mockProjectItem
                .Setup(p => p.get_FileNames(1))
                .Returns(path);

            return mockProjectItem;
        }

        public MockDTE(
            object targetFrameworkMoniker, string projectUniqueName = null, IEnumerable<Reference> references = null)
            : this(targetFrameworkMoniker, CreateProject(projectUniqueName, CreateVsProject2(references), null, null, null))
        {
        }

        public MockDTE(object targetFrameworkMoniker, Project project)
        {
            TargetFrameworkMoniker = targetFrameworkMoniker;

            Project = project;
            Hierarchy = CreateVsHierarchy(targetFrameworkMoniker, Project);
            Solution = CreateVsSolution(Hierarchy);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(m => m.GetService(typeof(IVsSolution)))
                .Returns(Solution);

            mockServiceProvider
                .Setup(m => m.GetService(typeof(SVsSolution)))
                .Returns(Solution);

            ServiceProvider = mockServiceProvider.Object;
        }

        public uint AddProjectItem(Mock<ProjectItem> mockProjectItem)
        {
            ++_projectItemId;

            object projectItem = mockProjectItem.Object;

            var mockHierarchy = Mock.Get(Hierarchy);
            mockHierarchy.
                Setup(
                    m => m.GetProperty(
                        _projectItemId,
                        VSHPROPID_ExtObject,
                        out projectItem)).
                Returns(S_OK);

            mockProjectItem.Setup(i => i.ContainingProject).Returns(Project);

            return _projectItemId;
        }

        public void SetProjectProperties(IDictionary<string, object> properties)
        {
            SetProjectProperties(Mock.Get(Project), properties);
        }

        private static IVsHierarchy CreateVsHierarchy(object targetFrameworkMoniker, object project)
        {
            var mockHierarchy = new Mock<IVsHierarchy>();
            mockHierarchy
                .Setup(m => m.GetProperty(VSITEMID_ROOT, VSHPROPID_TargetFrameworkMoniker, out targetFrameworkMoniker))
                .Returns(S_OK);

            mockHierarchy.
                Setup(
                    m => m.GetProperty(
                        It.Is<uint>(itemId => itemId == VSITEMID_ROOT),
                        It.Is<int>(propertyId => propertyId == VSHPROPID_ExtObject),
                        out project)).
                Returns(S_OK);

            return mockHierarchy.Object;
        }

        private static IVsSolution CreateVsSolution(IVsHierarchy hierarchy)
        {
            var mockSolution = new Mock<IVsSolution>();
            mockSolution
                .Setup(m => m.GetProjectOfUniqueName(It.IsAny<string>(), out hierarchy))
                .Returns(S_OK);

            return mockSolution.Object;
        }

        private static Project CreateProject(
            string projectUniqueName,
            object vsProject,
            string kind,
            IDictionary<string, object> properties,
            IDictionary<string, object> configurationProperties)
        {
            var mockProject = new Mock<Project>();
            mockProject.Setup(m => m.UniqueName).Returns(projectUniqueName);
            mockProject.SetupGet(p => p.Object).Returns(vsProject);
            mockProject.SetupGet(p => p.Kind).Returns(kind);

            SetProjectProperties(mockProject, properties);

            if (configurationProperties != null)
            {
                var configurationPropertiesList = configurationProperties.Select(p => CreateProperty(p.Key, p.Value))
                    .ToList();

                var activeConfigurationProperties = new Mock<Properties>();
                activeConfigurationProperties.As<IEnumerable>().Setup(p => p.GetEnumerator())
                    .Returns(() => configurationPropertiesList.GetEnumerator());

                var configuration = new Mock<Configuration>();
                configuration.SetupGet(c => c.Properties).Returns(activeConfigurationProperties.Object);

                var configurationManager = new Mock<ConfigurationManager>();
                configurationManager.SetupGet(m => m.ActiveConfiguration).Returns(configuration.Object);

                mockProject.SetupGet(p => p.ConfigurationManager).Returns(configurationManager.Object);
            }

            return mockProject.Object;
        }


        private static VSProject2 CreateVsProject2(IEnumerable<Reference> references)
        {
            if (references == null)
            {
                return null;
            }

            var mockReferences = new Mock<References>();
            mockReferences.Setup(r => r.GetEnumerator())
                .Returns(() => references.GetEnumerator());
            mockReferences.As<IEnumerable>().Setup(r => r.GetEnumerator())
                .Returns(() => references.GetEnumerator());

            var mockVsProject2 = new Mock<VSProject2>();
            mockVsProject2.SetupGet(p => p.References).Returns(mockReferences.Object);

            return mockVsProject2.Object;
        }

        private static VSWebSite CreateVsWebsite(IEnumerable<AssemblyReference> references)
        {
            if (references == null)
            {
                return null;
            }

            var vsWebSite = new Mock<VSWebSite>();
            var vsAssemblyReferences = new Mock<AssemblyReferences>();
            vsAssemblyReferences.As<IEnumerable>()
                .Setup(r => r.GetEnumerator())
                .Returns(references.GetEnumerator());

            vsWebSite.SetupGet(p => p.References).Returns(vsAssemblyReferences.Object);

            return vsWebSite.Object;
        }

        private static void SetProjectProperties(Mock<Project> mockProject, IDictionary<string, object> properties)
        {
            if (properties != null)
            {
                var propertyList = properties.Select(p => CreateProperty(p.Key, p.Value)).ToList();

                var projectProperties = new Mock<Properties>();
                projectProperties.As<IEnumerable>().Setup(p => p.GetEnumerator())
                    .Returns(() => propertyList.GetEnumerator());
                projectProperties.Setup(p => p.Item(It.IsAny<string>()))
                    .Returns<string>(n => propertyList.FirstOrDefault(p => p.Name == n));

                mockProject.SetupGet(p => p.Properties).Returns(projectProperties.Object);
            }
        }

        private static Property CreateProperty(string name, object value)
        {
            var property = new Mock<Property>();
            property.SetupGet(p => p.Name).Returns(name);
            property.SetupGet(p => p.Value).Returns(value);

            return property.Object;
        }
    }
}
