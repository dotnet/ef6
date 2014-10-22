// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using System.Xml;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.Data.Core;
    using Moq;
    using System;
    using System.Collections.Generic;
    using UnitTests.TestHelpers;
    using VSLangProj;
    using Xunit;

    public class WizardPageDbConfigTests
    {
        static WizardPageDbConfigTests()
        {
            // The code below is required to avoid test failures due to:
            // Due to limitations in CLR, DynamicProxy was unable to successfully replicate non-inheritable attribute 
            // System.Security.Permissions.UIPermissionAttribute on 
            // Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui.WizardPageStart.ProcessDialogChar. 
            // To avoid this error you can chose not to replicate this attribute type by calling
            // 'Castle.DynamicProxy.Generators.AttributesToAvoidReplicating.Add(typeof(System.Security.Permissions.UIPermissionAttribute))'.
            // 
            // Note that the same pattern need to be used when creating tests for other wizard pages to avoid 
            // issues related to the order the tests are run. Alternatively we could have code that is always being run
            // before any tests (e.g. a ctor of a class all test classes would derive from) where we would do that
            Castle.DynamicProxy.Generators
                .AttributesToAvoidReplicating
                .Add(typeof(System.Security.Permissions.UIPermissionAttribute));
        }

        [Fact]
        public void OnActivate_result_depends_on_FileAlreadyExistsError()
        {
            var wizard = ModelBuilderWizardFormHelper.CreateWizard();
            var wizardPageDbConfig = new WizardPageDbConfig(wizard);

            wizard.FileAlreadyExistsError = true;
            Assert.False(wizardPageDbConfig.OnActivate());

            wizard.FileAlreadyExistsError = false;
            Assert.True(wizardPageDbConfig.OnActivate());
        }

        [Fact]
        public void GetTextBoxConnectionStringValue_returns_entity_connection_string_for_EDMX_DatabaseFirst()
        {
            var guid = new Guid("42424242-4242-4242-4242-424242424242");

            var mockDte = new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]);
            mockDte.SetProjectProperties(new Dictionary<string, object> { { "FullPath", @"C:\Project" } });
            var mockParentProjectItem = new Mock<ProjectItem>();
            mockParentProjectItem.Setup(p => p.Collection).Returns(Mock.Of<ProjectItems>());
            mockParentProjectItem.Setup(p => p.Name).Returns("Folder");

            var mockModelProjectItem = new Mock<ProjectItem>();
            var mockCollection = new Mock<ProjectItems>();
            mockCollection.Setup(p => p.Parent).Returns(mockParentProjectItem.Object);
            mockModelProjectItem.Setup(p => p.Collection).Returns(mockCollection.Object);

            var wizardPageDbConfig =
                new WizardPageDbConfig(
                    ModelBuilderWizardFormHelper.CreateWizard(ModelGenerationOption.GenerateFromDatabase, mockDte.Project, @"C:\Project\myModel.edmx"));

            Assert.Equal(
                "metadata=res://*/myModel.csdl|res://*/myModel.ssdl|res://*/myModel.msl;provider=System.Data.SqlClient;" +
                "provider connection string=\"integrated security=SSPI;MultipleActiveResultSets=True;App=EntityFramework\"",
                wizardPageDbConfig.GetTextBoxConnectionStringValue(
                    CreateDataProviderManager(guid),
                    guid,
                    "Integrated Security=SSPI"));
        }

        [Fact]
        public void GetTextBoxConnectionStringValue_returns_entity_connection_string_for_EDMX_ModelFirst()
        {
            var guid = new Guid("42424242-4242-4242-4242-424242424242");

            var mockDte = new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]);
            mockDte.SetProjectProperties(new Dictionary<string, object> { { "FullPath", @"C:\Project" } });
            var mockParentProjectItem = new Mock<ProjectItem>();
            mockParentProjectItem.Setup(p => p.Collection).Returns(Mock.Of<ProjectItems>());
            mockParentProjectItem.Setup(p => p.Name).Returns("Folder");

            var mockModelProjectItem = new Mock<ProjectItem>();
            var mockCollection = new Mock<ProjectItems>();
            mockCollection.Setup(p => p.Parent).Returns(mockParentProjectItem.Object);
            mockModelProjectItem.Setup(p => p.Collection).Returns(mockCollection.Object);

            var wizardPageDbConfig =
                new WizardPageDbConfig(
                    ModelBuilderWizardFormHelper.CreateWizard(ModelGenerationOption.GenerateDatabaseScript, mockDte.Project, @"C:\Project\myModel.edmx"));

            Assert.Equal(
                "metadata=res://*/myModel.csdl|res://*/myModel.ssdl|res://*/myModel.msl;provider=System.Data.SqlClient;" +
                "provider connection string=\"integrated security=SSPI;MultipleActiveResultSets=True;App=EntityFramework\"",
                wizardPageDbConfig.GetTextBoxConnectionStringValue(
                    CreateDataProviderManager(guid),
                    guid,
                    "Integrated Security=SSPI"));
        }

        [Fact]
        public void GetTextBoxConnectionStringValue_returns_regular_connection_string_for_CodeFirst_from_Database()
        {
            var guid = new Guid("42424242-4242-4242-4242-424242424242");
            var wizardPageDbConfig = new WizardPageDbConfig(
                ModelBuilderWizardFormHelper.CreateWizard(ModelGenerationOption.CodeFirstFromDatabase));

            Assert.Equal(
                "integrated security=SSPI;MultipleActiveResultSets=True;App=EntityFramework",
                wizardPageDbConfig.GetTextBoxConnectionStringValue(
                    CreateDataProviderManager(guid),
                    guid, 
                    "Integrated Security=SSPI"));
        }

        private static IVsDataProviderManager CreateDataProviderManager(Guid vsDataProviderGuid)
        {
            var mockDataProvider = new Mock<IVsDataProvider>();
            mockDataProvider
                .Setup(p => p.GetProperty("InvariantName"))
                .Returns("System.Data.SqlClient");

            var mockProviderManager = new Mock<IVsDataProviderManager>();
            mockProviderManager
                .Setup(m => m.Providers)
                .Returns(new Dictionary<Guid, IVsDataProvider> { { vsDataProviderGuid, mockDataProvider.Object } });

            return mockProviderManager.Object;
        }
    }
}
