// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using System;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Moq;
    using Moq.Protected;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using UnitTests.TestHelpers;
    using VSLangProj;
    using VsWebSite;
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
            Run_OnDeactivate_creates_and_verifies_model_path(ModelGenerationOption.EmptyModel, LangEnum.CSharp, false, ".edmx");
            Run_OnDeactivate_creates_and_verifies_model_path(ModelGenerationOption.EmptyModel, LangEnum.CSharp, true, ".edmx");
            Run_OnDeactivate_creates_and_verifies_model_path(ModelGenerationOption.EmptyModel, LangEnum.VisualBasic, false, ".edmx");
            Run_OnDeactivate_creates_and_verifies_model_path(ModelGenerationOption.EmptyModel, LangEnum.VisualBasic, true, ".edmx");

            Run_OnDeactivate_creates_and_verifies_model_path(ModelGenerationOption.GenerateFromDatabase, LangEnum.CSharp, false, ".edmx");
            Run_OnDeactivate_creates_and_verifies_model_path(ModelGenerationOption.GenerateFromDatabase, LangEnum.CSharp, true, ".edmx");
            Run_OnDeactivate_creates_and_verifies_model_path(ModelGenerationOption.GenerateFromDatabase, LangEnum.VisualBasic, false, ".edmx");
            Run_OnDeactivate_creates_and_verifies_model_path(ModelGenerationOption.GenerateFromDatabase, LangEnum.VisualBasic, true, ".edmx");

            Run_OnDeactivate_creates_and_verifies_model_path(ModelGenerationOption.EmptyModelCodeFirst, LangEnum.CSharp, false, ".cs");
            Run_OnDeactivate_creates_and_verifies_model_path(ModelGenerationOption.EmptyModelCodeFirst, LangEnum.CSharp, true, ".cs");

            Run_OnDeactivate_creates_and_verifies_model_path(ModelGenerationOption.EmptyModelCodeFirst, LangEnum.VisualBasic, false, ".vb");
            Run_OnDeactivate_creates_and_verifies_model_path(ModelGenerationOption.EmptyModelCodeFirst, LangEnum.VisualBasic, true, ".vb");

            Run_OnDeactivate_creates_and_verifies_model_path(ModelGenerationOption.CodeFirstFromDatabase, LangEnum.CSharp, false, ".cs");
            Run_OnDeactivate_creates_and_verifies_model_path(ModelGenerationOption.CodeFirstFromDatabase, LangEnum.CSharp, true, ".cs");

            Run_OnDeactivate_creates_and_verifies_model_path(ModelGenerationOption.CodeFirstFromDatabase, LangEnum.VisualBasic, false, ".vb");
            Run_OnDeactivate_creates_and_verifies_model_path(ModelGenerationOption.CodeFirstFromDatabase, LangEnum.VisualBasic, true, ".vb");
        }

        private static void Run_OnDeactivate_creates_and_verifies_model_path(
            ModelGenerationOption generationOption, LangEnum language, bool isWebSite, string expectedExtension)
        {
            var mockDte =
                new MockDTE(
                    ".NETFramework, Version=v4.5",
                    isWebSite
                        ? MockDTE.CreateWebSite(
                            properties: new Dictionary<string, object>
                            {
                                { "CurrentWebsiteLanguage", language == LangEnum.CSharp ? "C#" : "VB" }
                            }, 
                            assemblyReferences: new AssemblyReference[0])
                        : MockDTE.CreateProject(
                            kind: language == LangEnum.CSharp ? MockDTE.CSharpProjectKind : MockDTE.VBProjectKind, 
                            assemblyReferences: new Reference[0]));

            var modelBuilderSettings = new ModelBuilderSettings
            {
                NewItemFolder = @"C:\temp",
                ModelName = "myModel",
                Project = mockDte.Project,
                GenerationOption = generationOption
            };

            var mockWizardPageStart = 
                new Mock<WizardPageStart>(ModelBuilderWizardFormHelper.CreateWizard(modelBuilderSettings, mockDte.ServiceProvider), null) 
                { CallBase = true };

            mockWizardPageStart
                .Protected()
                .Setup<bool>("VerifyModelFilePath", ItExpr.IsAny<string>())
                .Returns(false);

            mockWizardPageStart.Object.OnDeactivate();

            mockWizardPageStart
                .Protected()
                .Verify("VerifyModelFilePath", Times.Once(), @"C:\temp\myModel" + expectedExtension);
        }

        [Fact]
        public void OnDeactivate_does_not_update_settings_if_model_file_already_exists()
        {
            var mockDte = new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]);

            var modelBuilderSettings = new ModelBuilderSettings
            {
                NewItemFolder = @"C:\temp",
                ModelName = "myModel",
                Project = mockDte.Project
            };

            var wizard = ModelBuilderWizardFormHelper.CreateWizard(modelBuilderSettings, mockDte.ServiceProvider);
            var mockWizardPageStart = new Mock<WizardPageStart>(wizard, null) { CallBase = true };
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
            var wizard = CreateModelBuilderWizardForm(ModelGenerationOption.EmptyModel);
            CreateMockWizardPageStart(wizard, WizardPageStart.GenerateEmptyModelIndex).Object
                .OnDeactivate();

            var modelBuilderSettings = wizard.ModelBuilderSettings;

            Assert.Equal(ModelGenerationOption.EmptyModel, modelBuilderSettings.GenerationOption);
            Assert.False(wizard.FileAlreadyExistsError);
            Assert.Equal(@"C:\temp\myModel.edmx", modelBuilderSettings.ModelPath);
            Assert.True(modelBuilderSettings.ReplacementDictionary.Any());
            Assert.Null(modelBuilderSettings.ModelBuilderEngine);
        }

        [Fact]
        public void OnDeactivate_updates_model_settings_if_model_file_does_not_exist_for_generate_from_database()
        {
            
            var wizard = CreateModelBuilderWizardForm(ModelGenerationOption.GenerateFromDatabase);

            var mockWizardPageStart = CreateMockWizardPageStart(wizard, WizardPageStart.GenerateFromDatabaseIndex);
            mockWizardPageStart
                .Protected()
                .Setup<string>("GetEdmxTemplateContent", ItExpr.IsAny<string>())
                .Returns("vstemplate contents");

            mockWizardPageStart.Object.OnDeactivate();

            var modelBuilderSettings = wizard.ModelBuilderSettings;

            Assert.Equal(ModelGenerationOption.GenerateFromDatabase, modelBuilderSettings.GenerationOption);
            Assert.False(wizard.FileAlreadyExistsError);
            Assert.Equal(@"C:\temp\myModel.edmx", modelBuilderSettings.ModelPath);
            // replacement dictionary updated lazily
            Assert.False(modelBuilderSettings.ReplacementDictionary.Any());
            Assert.IsType<EdmxModelBuilderEngine>(modelBuilderSettings.ModelBuilderEngine);

            mockWizardPageStart
                .Protected()
                .Verify("GetEdmxTemplateContent", Times.Once(), "fake.vstemplate");   
        }

        [Fact]
        public void OnDeactivate_updates_model_settings_if_model_file_does_not_exist_for_CodeFirst_empty_model()
        {
            var configXml = new XmlDocument();
            configXml.LoadXml(@"<configuration>
  <connectionStrings>    
    <add name=""myModel"" connectionString=""Data Source=(localdb)\v11.0;"" providerName=""System.Data.SqlClient"" />
  </connectionStrings>
</configuration>");

            var mockConfig = new Mock<ConfigFileUtils>(Mock.Of<Project>(), Mock.Of<IServiceProvider>(), null, Mock.Of<IVsUtils>(), null);
            mockConfig.Setup(u => u.LoadConfig()).Returns(configXml);

            var wizard = CreateModelBuilderWizardForm(ModelGenerationOption.EmptyModelCodeFirst);
            CreateMockWizardPageStart(wizard, WizardPageStart.GenerateEmptyModelCodeFirstIndex, mockConfig.Object).Object
                .OnDeactivate();

            var modelBuilderSettings = wizard.ModelBuilderSettings;
            Assert.Equal(ModelGenerationOption.EmptyModelCodeFirst, modelBuilderSettings.GenerationOption);
            Assert.False(wizard.FileAlreadyExistsError);
            Assert.Equal(@"C:\temp\myModel.cs", modelBuilderSettings.ModelPath);
            Assert.False(modelBuilderSettings.ReplacementDictionary.Any());
            Assert.Null(modelBuilderSettings.ModelBuilderEngine);
            Assert.Equal(@"myModel1", modelBuilderSettings.AppConfigConnectionPropertyName);
        }

        [Fact]
        public void OnDeactivate_updates_model_settings_if_model_file_does_not_exist_for_CodeFirst_from_database()
        {
            var wizard = CreateModelBuilderWizardForm(ModelGenerationOption.CodeFirstFromDatabase);

            CreateMockWizardPageStart(wizard, WizardPageStart.GenerateCodeFirstFromDatabaseIndex).Object
                .OnDeactivate();

            var modelBuilderSettings = wizard.ModelBuilderSettings;
            Assert.Equal(ModelGenerationOption.CodeFirstFromDatabase, modelBuilderSettings.GenerationOption);
            Assert.False(wizard.FileAlreadyExistsError);
            Assert.Equal(@"C:\temp\myModel.cs", modelBuilderSettings.ModelPath);
            Assert.False(modelBuilderSettings.ReplacementDictionary.Any());
            Assert.IsType<CodeFirstModelBuilderEngine>(modelBuilderSettings.ModelBuilderEngine);
        }

        private static ModelBuilderWizardForm CreateModelBuilderWizardForm(ModelGenerationOption modelGenerationOption)
        {
            var mockDte = new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]);

            var modelBuilderSettings = new ModelBuilderSettings
            {
                NewItemFolder = @"C:\temp",
                ModelName = "myModel",
                ReplacementDictionary = new Dictionary<string, string>(),
                TargetSchemaVersion = EntityFrameworkVersion.Version3,
                VsTemplatePath = "fake.vstemplate",
                Project = mockDte.Project,
                GenerationOption = modelGenerationOption
            };

            var wizard = ModelBuilderWizardFormHelper.CreateWizard(modelBuilderSettings, mockDte.ServiceProvider);
            return wizard;
        }

        [Fact]
        public void OnActivate_result_depends_on_FileAlreadyExistsError()
        {
            var mockDte = new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]);

            var wizard = 
                ModelBuilderWizardFormHelper.CreateWizard(project : mockDte.Project, serviceProvider: mockDte.ServiceProvider);
            var wizardPageStart = new WizardPageStart(wizard);

            wizard.FileAlreadyExistsError = true;
            Assert.False(wizardPageStart.OnActivate());

            wizard.FileAlreadyExistsError = false;
            Assert.True(wizardPageStart.OnActivate());
        }

        [Fact]
        public void listViewModelContents_DoubleClick_calls_OnFinish_if_EmptyModel_selected()
        {
            var mockDte = new MockDTE(
                ".NETFramework, Version=v4.5", 
                references: new[] { MockDTE.CreateReference("EntityFramework", "5.0.0.0") });

            var mockWizard =
                Mock.Get(
                    ModelBuilderWizardFormHelper.CreateWizard(
                        ModelGenerationOption.EmptyModel, 
                        mockDte.Project, 
                        serviceProvider: mockDte.ServiceProvider));

            var mockWizardPageStart =
                CreateMockWizardPageStart(mockWizard.Object, WizardPageStart.GenerateEmptyModelIndex);

            mockWizard.Setup(w => w.OnFinish());

            mockWizardPageStart.Object.listViewModelContents_DoubleClick(sender: null, e: null);

            mockWizard.Verify(w => w.OnFinish(), Times.Once());
        }

        [Fact]
        public void listViewModelContents_DoubleClick_calls_OnFinish_if_EmptyModelCodeFirst_selected_and_EF_not_referenced_or_EF6_referenced()
        {
            var mockDtes = new[]
            {
                new MockDTE(".NETFramework, Version=v4.5", references: new Reference[0]),
                new MockDTE(".NETFramework, Version=v4.5",
                    references: new[] { MockDTE.CreateReference("EntityFramework", "6.0.0.0") })
            };

            foreach (var mockDte in mockDtes)
            {
                var mockWizard = 
                    Mock.Get(
                        ModelBuilderWizardFormHelper.CreateWizard(
                            ModelGenerationOption.EmptyModelCodeFirst, 
                            mockDte.Project, 
                            serviceProvider: mockDte.ServiceProvider));
                    
                var mockWizardPageStart =
                    CreateMockWizardPageStart(mockWizard.Object, WizardPageStart.GenerateEmptyModelCodeFirstIndex);

                mockWizard.Setup(w => w.OnFinish());

                mockWizardPageStart.Object.listViewModelContents_DoubleClick(sender: null, e: null);

                mockWizard.Verify(w => w.OnFinish(), Times.Once());
            }
        }

        [Fact]
        public void listViewModelContents_DoubleClick_wont_call_OnFinish_if_EmptyModelCodeFirst_selected_and_EF5_referenced()
        {
            var mockDte =
                new MockDTE(
                    ".NETFramework, Version=v4.5",
                    references: new[] { MockDTE.CreateReference("EntityFramework", "5.0.0.0") });

            var mockWizard = 
                    Mock.Get(
                        ModelBuilderWizardFormHelper.CreateWizard(
                            ModelGenerationOption.EmptyModelCodeFirst, 
                            mockDte.Project, 
                            serviceProvider: mockDte.ServiceProvider));

            var mockWizardPageStart = 
                CreateMockWizardPageStart(mockWizard.Object, WizardPageStart.GenerateEmptyModelCodeFirstIndex);

            mockWizard.Setup(w => w.OnFinish());

            mockWizardPageStart.Object.listViewModelContents_DoubleClick(sender: null, e: null);

            mockWizard.Verify(w => w.OnFinish(), Times.Never());
        }

        private static Mock<WizardPageStart> CreateMockWizardPageStart(ModelBuilderWizardForm mockWizard, int itemIndex, 
            ConfigFileUtils configFileUtils = null)
        {
            var mockWizardPageStart = new Mock<WizardPageStart>(mockWizard, configFileUtils) { CallBase = true };
            mockWizardPageStart
                .Protected()
                .Setup<bool>("AnyItemSelected")
                .Returns(true);

            mockWizardPageStart
                .Protected()
                .Setup<bool>("VerifyModelFilePath", ItExpr.IsAny<string>())
                .Returns(true);

            mockWizardPageStart
                .Protected()
                .Setup<int>("GetSelectedOptionIndex")
                .Returns(itemIndex);
            return mockWizardPageStart;
        }
    }
}
