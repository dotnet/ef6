// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Xml.Linq;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Moq;
    using Moq.Protected;
    using UnitTests.TestHelpers;
    using VSLangProj;
    using Xunit;

    public class VSArtifactTests
    {
        [Fact]
        public void GetModelGenErrors_returns_errors_stored_in_ModelGenCache()
        {
            var modelGenCache = new ModelGenErrorCache();
            var errors = new List<EdmSchemaError>(new[] { new EdmSchemaError("test", 42, EdmSchemaErrorSeverity.Error) });
            modelGenCache.AddErrors(@"C:\temp.edmx", errors);

            var mockPackage = new Mock<IEdmPackage>();
            mockPackage.Setup(p => p.ModelGenErrorCache).Returns(modelGenCache);
            PackageManager.Package = mockPackage.Object;

            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            using (var vsArtifact = new VSArtifact(modelManager, new Uri(@"C:\temp.edmx"), modelProvider))
            {
                Assert.Same(errors, vsArtifact.GetModelGenErrors());
            }
        }

        [Fact]
        public void
            DetermineIfArtifactIsVersionSafe_sets_IsVersionSafe_to_true_if_schema_is_the_latest_version_supported_by_referenced_runtime()
        {
            var mockDte = new MockDTE(
                ".NETFramework, Version=v4.5.1",
                references: new[] { MockDTE.CreateReference("EntityFramework", "6.0.0.0") });

            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var mockVsArtifact =
                new Mock<VSArtifact>(modelManager, new Uri("urn:dummy"), modelProvider) { CallBase = true };
            mockVsArtifact.Protected().Setup<IServiceProvider>("ServiceProvider").Returns(mockDte.ServiceProvider);
            mockVsArtifact.Protected().Setup<Project>("GetProject").Returns(mockDte.Project);
            mockVsArtifact.Setup(a => a.XDocument).Returns(
                new XDocument(
                    new XElement(
                        XName.Get(
                            "Edmx",
                            SchemaManager.GetEDMXNamespaceName(EntityFrameworkVersion.Version3)))));

            var artifact = mockVsArtifact.Object;

            artifact.DetermineIfArtifactIsVersionSafe();
            Assert.True(artifact.IsVersionSafe);
        }

        [Fact]
        public void DetermineIfArtifactIsVersionSafe_sets_IsVersionSafe_to_true_for_Misc_project()
        {
            var mockDte = new MockDTE(
                ".NETFramework, Version=v4.5.1",
                Constants.vsMiscFilesProjectUniqueName);

            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var mockVsArtifact =
                new Mock<VSArtifact>(modelManager, new Uri("urn:dummy"), modelProvider) { CallBase = true };
            mockVsArtifact.Protected().Setup<IServiceProvider>("ServiceProvider").Returns(mockDte.ServiceProvider);
            mockVsArtifact.Protected().Setup<Project>("GetProject").Returns(mockDte.Project);
            mockVsArtifact.Setup(a => a.XDocument).Returns(
                new XDocument(
                    new XElement(
                        XName.Get(
                            "Edmx",
                            SchemaManager.GetEDMXNamespaceName(EntityFrameworkVersion.Version3)))));

            var artifact = mockVsArtifact.Object;
            artifact.DetermineIfArtifactIsVersionSafe();
            Assert.True(artifact.IsVersionSafe);
        }

        [Fact]
        public void DetermineIfArtifactIsVersionSafe_sets_IsVersionSafe_to_false_if_project_does_not_support_EF()
        {
            var mockDte = new MockDTE(
                ".NETFramework, Version=v2.0",
                references: new Reference[0]);

            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var mockVsArtifact =
                new Mock<VSArtifact>(modelManager, new Uri("urn:dummy"), modelProvider) { CallBase = true };
            mockVsArtifact.Protected().Setup<IServiceProvider>("ServiceProvider").Returns(mockDte.ServiceProvider);
            mockVsArtifact.Protected().Setup<Project>("GetProject").Returns(mockDte.Project);
            mockVsArtifact.Setup(a => a.XDocument).Returns(
                new XDocument(
                    new XElement(
                        XName.Get(
                            "Edmx",
                            SchemaManager.GetEDMXNamespaceName(EntityFrameworkVersion.Version2)))));

            var artifact = mockVsArtifact.Object;

            artifact.DetermineIfArtifactIsVersionSafe();
            Assert.False(artifact.IsVersionSafe);
        }

        [Fact]
        public void
            DetermineIfArtifactIsVersionSafe_sets_IsVersionSafe_to_false_if_schema_is_not_the_latest_version_supported_by_referenced_runtime
            ()
        {
            var mockDte = new MockDTE(
                ".NETFramework, Version=v4.5.1",
                references: new[] { MockDTE.CreateReference("EntityFramework", "6.0.0.0") });

            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var mockVsArtifact =
                new Mock<VSArtifact>(modelManager, new Uri("urn:dummy"), modelProvider) { CallBase = true };
            mockVsArtifact.Protected().Setup<IServiceProvider>("ServiceProvider").Returns(mockDte.ServiceProvider);
            mockVsArtifact.Protected().Setup<Project>("GetProject").Returns(mockDte.Project);
            mockVsArtifact.Setup(a => a.XDocument).Returns(
                new XDocument(
                    new XElement(
                        XName.Get(
                            "Edmx",
                            SchemaManager.GetEDMXNamespaceName(EntityFrameworkVersion.Version2)))));

            var artifact = mockVsArtifact.Object;

            artifact.DetermineIfArtifactIsVersionSafe();
            Assert.False(artifact.IsVersionSafe);
        }

        [Fact]
        public void DetermineIfArtifactIsVersionSafe_sets_IsVersionSafe_to_false_if_versions_dont_match()
        {
            var mockDte = new MockDTE(
                ".NETFramework, Version=v4.5.1",
                Constants.vsMiscFilesProjectUniqueName);

            var modelManager = new Mock<ModelManager>(null, null).Object;
            var modelProvider = new Mock<XmlModelProvider>().Object;
            var mockVsArtifact =
                new Mock<VSArtifact>(modelManager, new Uri("urn:dummy"), modelProvider) { CallBase = true };
            mockVsArtifact.Protected().Setup<IServiceProvider>("ServiceProvider").Returns(mockDte.ServiceProvider);
            mockVsArtifact.Protected().Setup<Project>("GetProject").Returns(mockDte.Project);
            mockVsArtifact.Setup(a => a.XDocument).Returns(
                new XDocument(
                    new XElement(
                        XName.Get(
                            "Edmx",
                            SchemaManager.GetEDMXNamespaceName(EntityFrameworkVersion.Version3)))));

            var mockConceptualModel =
                new Mock<ConceptualEntityModel>(
                    mockVsArtifact.Object,
                    new XElement(XName.Get("Schema", SchemaManager.GetCSDLNamespaceName(EntityFrameworkVersion.Version2))));

            mockConceptualModel
                .Setup(m => m.XNamespace)
                .Returns(SchemaManager.GetCSDLNamespaceName(EntityFrameworkVersion.Version2));

            mockVsArtifact
                .Setup(m => m.ConceptualModel)
                .Returns(mockConceptualModel.Object);

            var artifact = mockVsArtifact.Object;
            artifact.DetermineIfArtifactIsVersionSafe();
            Assert.False(artifact.IsVersionSafe);
        }
    }
}
