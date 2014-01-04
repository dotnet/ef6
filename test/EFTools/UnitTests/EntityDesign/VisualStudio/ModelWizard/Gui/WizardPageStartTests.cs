// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Moq;
    using Moq.Protected;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnitTests.TestHelpers;
    using Xunit;

    public class WizardPageStartTests
    {
        static WizardPageStartTests()
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
        public void OnDeactivate_creates_and_verifies_model_path()
        {
            var mockDte = new MockDTE(".NETFramework, Version=v4.5");

            var modelBuilderSettings = new ModelBuilderSettings
            {
                NewItemFolder = @"C:\temp",
                ModelName = "myModel",
                Project = mockDte.Project
            };

            var wizard = new ModelBuilderWizardForm(modelBuilderSettings, ModelBuilderWizardForm.WizardMode.PerformAllFunctionality);
            var mockWizardPageStart = new Mock<WizardPageStart>(wizard, mockDte.ServiceProvider) { CallBase = true };
            mockWizardPageStart
                .Protected()
                .Setup<bool>("VerifyModelFilePath", ItExpr.IsAny<string>())
                .Returns(false);

            mockWizardPageStart.Object.OnDeactivate();

            mockWizardPageStart
                .Protected()
                .Verify("VerifyModelFilePath", Times.Once(), @"C:\temp\myModel.edmx");
        }

        [Fact]
        public void OnDeactivate_does_not_update_settings_if_model_file_already_exists()
        {
            var mockDte = new MockDTE(".NETFramework, Version=v4.5");

            var modelBuilderSettings = new ModelBuilderSettings
            {
                NewItemFolder = @"C:\temp",
                ModelName = "myModel",
                Project = mockDte.Project
            };

            var wizard = new ModelBuilderWizardForm(modelBuilderSettings, ModelBuilderWizardForm.WizardMode.PerformAllFunctionality);
            var mockWizardPageStart = new Mock<WizardPageStart>(wizard, mockDte.ServiceProvider) { CallBase = true };
            mockWizardPageStart
                .Protected()
                .Setup<bool>("VerifyModelFilePath", ItExpr.IsAny<string>())
                .Returns(false);

            mockWizardPageStart.Object.OnDeactivate();

            Assert.Null(modelBuilderSettings.ModelPath);
            Assert.True(wizard.FileAlreadyExistsError);
        }

        [Fact]
        public void OnDeactivate_updates_model_settings_if_model_file_does_not_exist_for_empty_model()
        {
            var mockDte = new MockDTE(".NETFramework, Version=v4.5");

            var modelBuilderSettings = new ModelBuilderSettings
            {
                NewItemFolder = @"C:\temp",
                ModelName = "myModel",
                ReplacementDictionary = new Dictionary<string, string>(),
                TargetSchemaVersion = EntityFrameworkVersion.Version3,
                Project = mockDte.Project
            };

            var wizard = new ModelBuilderWizardForm(modelBuilderSettings, ModelBuilderWizardForm.WizardMode.PerformAllFunctionality);
            var mockWizardPageStart = new Mock<WizardPageStart>(wizard, mockDte.ServiceProvider) { CallBase = true };
            mockWizardPageStart
                .Protected()
                .Setup<bool>("VerifyModelFilePath", ItExpr.IsAny<string>())
                .Returns(true);
            mockWizardPageStart
                .Protected()
                .Setup<int>("GetSelectedOptionIndex")
                .Returns(WizardPageStart.GenerateEmptyModelIndex);

            mockWizardPageStart.Object.OnDeactivate();

            Assert.Equal(ModelGenerationOption.EmptyModel, modelBuilderSettings.GenerationOption);
            Assert.False(wizard.FileAlreadyExistsError);
            Assert.Equal(@"C:\temp\myModel.edmx", modelBuilderSettings.ModelPath);
            Assert.True(modelBuilderSettings.ReplacementDictionary.Any());
            Assert.Null(modelBuilderSettings.ModelBuilderEngine);
        }

        [Fact]
        public void OnDeactivate_updates_model_settings_if_model_file_does_not_exist_for_generate_from_database()
        {
            var mockDte = new MockDTE(".NETFramework, Version=v4.5");

            var modelBuilderSettings = new ModelBuilderSettings
            {
                NewItemFolder = @"C:\temp",
                ModelName = "myModel",
                ReplacementDictionary = new Dictionary<string, string>(),
                TargetSchemaVersion = EntityFrameworkVersion.Version3,
                VsTemplatePath = "fake.vstemplate",
                Project = mockDte.Project
            };

            var wizard = new ModelBuilderWizardForm(modelBuilderSettings, ModelBuilderWizardForm.WizardMode.PerformAllFunctionality);
            var mockWizardPageStart = new Mock<WizardPageStart>(wizard, mockDte.ServiceProvider) { CallBase = true };
            mockWizardPageStart
                .Protected()
                .Setup<bool>("VerifyModelFilePath", ItExpr.IsAny<string>())
                .Returns(true);
            mockWizardPageStart
                .Protected()
                .Setup<int>("GetSelectedOptionIndex")
                .Returns(WizardPageStart.GenerateFromDatabaseIndex);
            mockWizardPageStart
                .Protected()
                .Setup<string>("GetEdmxTemplateContent", ItExpr.IsAny<string>())
                .Returns("vstemplate contents");

            mockWizardPageStart.Object.OnDeactivate();

            Assert.Equal(ModelGenerationOption.GenerateFromDatabase, modelBuilderSettings.GenerationOption);
            Assert.False(wizard.FileAlreadyExistsError);
            Assert.Equal(@"C:\temp\myModel.edmx", modelBuilderSettings.ModelPath);
            // replacement dictionary updated lazily
            Assert.False(modelBuilderSettings.ReplacementDictionary.Any());
            Assert.NotNull(modelBuilderSettings.ModelBuilderEngine);

            mockWizardPageStart
                .Protected()
                .Verify("GetEdmxTemplateContent", Times.Once(), "fake.vstemplate");   
        }

        [Fact]
        public void OnActivate_result_depends_on_FileAlreadyExistsError()
        {
            var mockDte = new MockDTE(".NETFramework, Version=v4.5");
            var wizard = new ModelBuilderWizardForm(
                new ModelBuilderSettings { Project = mockDte.Project}, 
                ModelBuilderWizardForm.WizardMode.PerformAllFunctionality);
            var wizardPageStart = new WizardPageStart(wizard, mockDte.ServiceProvider);

            wizard.FileAlreadyExistsError = true;
            Assert.False(wizardPageStart.OnActivate());

            wizard.FileAlreadyExistsError = false;
            Assert.True(wizardPageStart.OnActivate());
        }
    }
}
