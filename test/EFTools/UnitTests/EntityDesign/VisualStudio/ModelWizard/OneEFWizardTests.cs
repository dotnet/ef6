// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard
{
    using System.Data.Entity.Infrastructure;
    using System.IO;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.CodeGeneration;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.XmlDesignerBase;
    using Microsoft.VisualStudio.Shell.Interop;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using System.Xml;
    using UnitTests.TestHelpers;
    using Xunit;
    using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
    using System.Reflection;

    public class OneEFWizardTests
    {
        [Fact]
        public void RunFinished_should_not_add_connection_string_to_config_file_if_SaveConnectionStringInAppConfig_false()
        {
            var mockConfig = new Mock<ConfigFileUtils>(Mock.Of<Project>(), Mock.Of<IServiceProvider>(), 
                VisualStudioProjectSystem.WindowsApplication, null, null);

            new OneEFWizard(mockConfig.Object, Mock.Of<IVsUtils>())
                .RunFinished(new ModelBuilderSettings { SaveConnectionStringInAppConfig = false}, null);

            mockConfig.Verify(m => m.GetOrCreateConfigFile(), Times.Never());
            mockConfig.Verify(m => m.LoadConfig(), Times.Never());
            mockConfig.Verify(m => m.SaveConfig(It.IsAny<XmlDocument>()), Times.Never());
        }

        [Fact]
        public void RunFinished_should_not_add_connection_string_to_config_file_if_config_file_already_contains_same_connection()
        {
            var mockConfig = new Mock<ConfigFileUtils>(Mock.Of<Project>(), Mock.Of<IServiceProvider>(),
                VisualStudioProjectSystem.WindowsApplication, null, null);

            var configXml = new XmlDocument();
            configXml.LoadXml(
                @"<configuration>" + Environment.NewLine +
                @"  <connectionStrings>" + Environment.NewLine +
                @"    <add name=""myConnStr"" connectionString=""data source=(localdb)\v11.0;initial catalog=App.MyContext;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework"" providerName=""System.Data.SqlClient"" />" + Environment.NewLine +
                @"  </connectionStrings>" + Environment.NewLine +
                @"</configuration>"
            );
            mockConfig.Setup(u => u.LoadConfig()).Returns(configXml);

            var mockSettings = new Mock<ModelBuilderSettings> { CallBase = true };
            mockSettings
                .Setup(s => s.AppConfigConnectionString)
                .Returns(@"data source=(localdb)\v11.0;initial catalog=App.MyContext;integrated security=True");
            mockSettings.Setup(s => s.RuntimeProviderInvariantName).Returns("System.Data.SqlClient");

            var settings = mockSettings.Object;
            settings.SaveConnectionStringInAppConfig = true;
            settings.AppConfigConnectionPropertyName = "myConnStr";

            new OneEFWizard(mockConfig.Object, new Mock<IVsUtils>().Object)
                .RunFinished(settings, null);

            mockConfig.Verify(m => m.GetOrCreateConfigFile(), Times.Once());
            mockConfig.Verify(m => m.LoadConfig(), Times.Once());
            mockConfig.Verify(m => m.SaveConfig(It.IsAny<XmlDocument>()), Times.Never());
        }

        [Fact]
        public void RunFinished_adds_EF_attributes_and_saves_connection_string_to_config_file()
        {
            var mockConfig = new Mock<ConfigFileUtils>(Mock.Of<Project>(), Mock.Of<IServiceProvider>(),
                VisualStudioProjectSystem.WindowsApplication, null, null);

            var configXml = new XmlDocument();
            configXml.LoadXml("<configuration />");
            mockConfig.Setup(u => u.LoadConfig()).Returns(configXml);

            var mockSettings = new Mock<ModelBuilderSettings> { CallBase = true };
            mockSettings
                .Setup(s => s.AppConfigConnectionString)
                .Returns(@"data source=(localdb)\v11.0;initial catalog=App.MyContext;integrated security=True");
            mockSettings.Setup(s => s.RuntimeProviderInvariantName).Returns("System.Data.SqlClient");

            var settings = mockSettings.Object;
            settings.SaveConnectionStringInAppConfig = true;
            settings.AppConfigConnectionPropertyName = "myConnStr";

            new OneEFWizard(mockConfig.Object, Mock.Of<IVsUtils>())
                .RunFinished(settings, null);

            mockConfig.Verify(m => m.GetOrCreateConfigFile(), Times.Once());
            mockConfig.Verify(m => m.LoadConfig(), Times.Exactly(2));
            mockConfig.Verify(m => m.SaveConfig(
                It.Is<XmlDocument>(config => config.SelectSingleNode("/configuration/connectionStrings/add[@name='myConnStr']/@connectionString").Value ==
                @"data source=(localdb)\v11.0;initial catalog=App.MyContext;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework")), 
                Times.Once());
        }

        [Fact]
        public void RunFinished_checks_out_files_and_creates_project_items()
        {
            var mockProjectItems = new Mock<ProjectItems>();
            var mockProject = new Mock<Project>();
            mockProject.Setup(p => p.ProjectItems).Returns(mockProjectItems.Object);

            var mockVsUtils = new Mock<IVsUtils>();
            var settings = new ModelBuilderSettings {Project = mockProject.Object };
            var mockConfig = new Mock<ConfigFileUtils>(Mock.Of<Project>(), Mock.Of<IServiceProvider>(),
                VisualStudioProjectSystem.WindowsApplication, null, null);

            var generatedCode = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("context", string.Empty),
                new KeyValuePair<string, string>(Path.GetFileName(Assembly.GetExecutingAssembly().Location), string.Empty),
                new KeyValuePair<string, string>(Path.GetRandomFileName(), string.Empty)
            };

            new OneEFWizard(configFileUtils: mockConfig.Object, vsUtils: mockVsUtils.Object, generatedCode: generatedCode)
                .RunFinished(settings, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            // need to Skip(1) since the first item is the DbContext file which is being added as a project item
            mockVsUtils.Verify(
                u => u.WriteCheckoutTextFilesInProject(
                    It.Is<Dictionary<string, object>>(
                        fileMap => fileMap.Keys.Select(Path.GetFileName)
                            .SequenceEqual(generatedCode.Skip(1).Select(i => i.Key)))));

            var existingFilePath = Assembly.GetExecutingAssembly().Location;
            mockProjectItems.Verify(i => i.AddFromFile(existingFilePath), Times.Once());

            // verify we only added the file that existed 
            mockProjectItems.Verify(i => i.AddFromFile(It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public void RunStarted_saves_context_generated_code_replacementsDictionary_as_contextfilecontents()
        {
            var mockCodeGenerator = new Mock<CodeFirstModelGenerator>(MockDTE.CreateProject());
            mockCodeGenerator
                .Setup(g => g.Generate(It.IsAny<DbModel>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new[] { new KeyValuePair<string, string>("MyContext", "context code") });

            var modelBuilderSettings =
                new ModelBuilderSettings { SaveConnectionStringInAppConfig = true, AppConfigConnectionPropertyName = "ConnString" };

            var replacementsDictionary =
                new Dictionary<string, string>
                {
                    { "$safeitemname$", "MyContext" },
                    { "$rootnamespace$", "Project.Data" }
                };

            new OneEFWizard(vsUtils:Mock.Of<IVsUtils>())
                .RunStarted(modelBuilderSettings, mockCodeGenerator.Object, replacementsDictionary);

            Assert.Equal("context code", replacementsDictionary["$contextfilecontents$"]);
        }

        [Fact]
        public void RunStarted_uses_AppConfigConnectionPropertyName_if_SaveConnectionStringInAppConfig_true()
        {
            var mockCodeGenerator = new Mock<CodeFirstModelGenerator>(MockDTE.CreateProject());
            mockCodeGenerator
                .Setup(g => g.Generate(It.IsAny<DbModel>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new[] { new KeyValuePair<string, string>() });

            var modelBuilderSettings = 
                new ModelBuilderSettings { SaveConnectionStringInAppConfig = true, AppConfigConnectionPropertyName = "ConnString"};

            var replacementsDictionary =
                new Dictionary<string, string>
                {
                    { "$safeitemname$", "MyContext" },
                    { "$rootnamespace$", "Project.Data" }
                };

            new OneEFWizard(vsUtils: Mock.Of<IVsUtils>())
                .RunStarted(modelBuilderSettings, mockCodeGenerator.Object, replacementsDictionary);

            mockCodeGenerator.Verify(g => g.Generate(It.IsAny<DbModel>(), "Project.Data", "MyContext", "ConnString"), Times.Once());
        }

        [Fact]
        public void RunStarted_uses_context_class_name_if_SaveConnectionStringInAppConfig_false()
        {
            var mockCodeGenerator = new Mock<CodeFirstModelGenerator>(MockDTE.CreateProject());
            mockCodeGenerator
                .Setup(g => g.Generate(It.IsAny<DbModel>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new[] { new KeyValuePair<string, string>() });

            var modelBuilderSettings =
                new ModelBuilderSettings { SaveConnectionStringInAppConfig = false, AppConfigConnectionPropertyName = "ConnString" };

            var replacementsDictionary =
                new Dictionary<string, string>
                {
                    { "$safeitemname$", "MyContext" },
                    { "$rootnamespace$", "Project.Data" }
                };

            new OneEFWizard(vsUtils: Mock.Of<IVsUtils>())
                .RunStarted(modelBuilderSettings, mockCodeGenerator.Object, replacementsDictionary);

            mockCodeGenerator.Verify(g => g.Generate(It.IsAny<DbModel>(), "Project.Data", "MyContext", "MyContext"), Times.Once());
        }

        [Fact]
        public void ProjectItemFinishedGenerating_adds_errors_to_error_pane_if_any()
        {
            Mock<IErrorListHelper> mockErrorListHelper;
            Mock<ProjectItem> mockProjectItem;

            const string itemPath = @"C:\Project\MyContext.cs";

            CreateOneEFWizard(
                itemPath, 
                new[]
                {
                    new EdmSchemaError("error", 20, EdmSchemaErrorSeverity.Error),
                    new EdmSchemaError("warning", 10, EdmSchemaErrorSeverity.Warning)
                },
                out mockErrorListHelper, 
                out mockProjectItem)
                    .ProjectItemFinishedGenerating(mockProjectItem.Object);

            Func<ICollection<ErrorInfo>, bool> errorInfoCollectionVerification = c =>
            {
                if (c.Count != 2)
                {
                    return false;
                }

                var first = c.First();
                var second = c.Last();

                return c.All(i => i.ItemPath == itemPath && i.ErrorClass == ErrorClass.Runtime_All) &&
                    first.IsError() && first.Message == string.Format(Resources.Error_Message_With_Error_Code_Prefix, 20, "error") && first.ErrorCode == 20 &&
                    second.IsWarning() && second.Message == string.Format(Resources.Error_Message_With_Error_Code_Prefix, 10, "warning") && second.ErrorCode == 10;
            };

            mockErrorListHelper.Verify(
                h => h.AddErrorInfosToErrorList(
                    It.Is<ICollection<ErrorInfo>>(c => errorInfoCollectionVerification(c)),
                    It.IsAny<IVsHierarchy>(),
                    It.IsAny<uint>(),
                    false),
                Times.Once());
        }

        [Fact]
        public void ProjectItemFinishedGenerating_does_not_add_errors_to_error_pane_if_no_errors()
        {
            const string itemPath = @"C:\Project\MyContext.cs";

            Mock<IErrorListHelper> mockErrorListHelper;
            Mock<ProjectItem> mockProjectItem;

            CreateOneEFWizard(itemPath, null, out mockErrorListHelper, out mockProjectItem)
                .ProjectItemFinishedGenerating(mockProjectItem.Object);

            mockErrorListHelper.Verify(
                h => h.AddErrorInfosToErrorList(
                    It.IsAny<ICollection<ErrorInfo>>(),
                    It.IsAny<IVsHierarchy>(),
                    It.IsAny<uint>(),
                    false),
                Times.Never());
        }

        private static OneEFWizard CreateOneEFWizard(string itemPath, IEnumerable<EdmSchemaError> edmSchemaErrors, out Mock<IErrorListHelper> mockErrorListHelper, out Mock<ProjectItem> mockProjectItem)
        {
            var mockDte = new Mock<DTE>();
            mockDte.As<IOleServiceProvider>();
            var mockProject = new Mock<Project>();
            mockProject.Setup(p => p.DTE).Returns(mockDte.Object);

            var mockVsUtils = new Mock<IVsUtils>();
            mockErrorListHelper = new Mock<IErrorListHelper>();
            var errorCache = new ModelGenErrorCache();

            if (edmSchemaErrors != null)
            {
                errorCache.AddErrors(
                    itemPath,
                    edmSchemaErrors.ToList());
            }

            mockProjectItem = new Mock<ProjectItem>();
            mockProjectItem.Setup(p => p.get_FileNames(1)).Returns(itemPath);
            mockProjectItem.Setup(p => p.ContainingProject).Returns(mockProject.Object);

            return new OneEFWizard(
                vsUtils: mockVsUtils.Object, errorListHelper: mockErrorListHelper.Object, errorCache: errorCache);
        }

        [Fact]
        public void RunStarted_handles_CodeFirstModelGenerationException_and_shows_error_dialog()
        {
            var project = MockDTE.CreateProject();
            var mockCodeGenerator = new Mock<CodeFirstModelGenerator>(project);

            var innerException = new Exception("InnerException", new InvalidOperationException("nested InnerException"));

            mockCodeGenerator
                .Setup(g => g.Generate(It.IsAny<DbModel>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new CodeFirstModelGenerationException("Failed generating code.", innerException));

            var mockVsUtils = new Mock<IVsUtils>();

            var replacementsDictionary = new Dictionary<string, string>
            {
                { "$safeitemname$", "context.cs" },
                { "$rootnamespace$", "My.Namespace" }
            };

            new OneEFWizard(vsUtils: mockVsUtils.Object)
                .RunStarted(new ModelBuilderSettings(), mockCodeGenerator.Object, replacementsDictionary);

            mockVsUtils.Verify(u => u.ShowErrorDialog("Failed generating code.\r\n" + innerException), Times.Once());
        }

        [Fact]
        public void ShouldAddProjectItem_returns_false_if_code_could_not_be_generated()
        {
            Assert.False(
                new OneEFWizard(vsUtils: Mock.Of<IVsUtils>(), generatedCode: null)
                    .ShouldAddProjectItem(string.Empty));

            Assert.False(
                new OneEFWizard(vsUtils: Mock.Of<IVsUtils>(), generatedCode: new List<KeyValuePair<string, string>>())
                    .ShouldAddProjectItem(string.Empty));
        }

        [Fact]
        public void ShouldAddProjectItem_returns_true_if_code_could_be_generated()
        {
            Assert.True(
                new OneEFWizard(
                    vsUtils: Mock.Of<IVsUtils>(),
                    generatedCode: new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(string.Empty, string.Empty) })
                    .ShouldAddProjectItem(string.Empty));
        }

        [Fact]
        public void RunStarted_passes_safeitemname_as_context_name_if_valid_identifier()
        {
            const string contextName = "MyContext";

            var mockVsUtils = new Mock<IVsUtils>();
            var wizard = new OneEFWizard(vsUtils: mockVsUtils.Object);

            var mockCodeGenerator = new Mock<CodeFirstModelGenerator>(MockDTE.CreateProject());
            mockCodeGenerator
                .Setup(g => g.Generate(It.IsAny<DbModel>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new[] { new KeyValuePair<string, string>(string.Empty, string.Empty) });

            var settings = new ModelBuilderSettings { VSApplicationType = VisualStudioProjectSystem.WebApplication };
            var replacenentsDictionary = new Dictionary<string, string>
            {
                { "$safeitemname$", contextName },
                { "$rootnamespace$", "Project" }
            };

            wizard.RunStarted(settings, mockCodeGenerator.Object, replacenentsDictionary);

            mockCodeGenerator.Verify(
                g => g.Generate(
                    It.IsAny<DbModel>(),
                    "Project",
                    /*contextClassName*/ It.Is<string>(v => ReferenceEquals(v, contextName)),
                    /*connectionStringName*/ It.Is<string>(v => ReferenceEquals(v, contextName))),
            Times.Once());
        }

        [Fact]
        public void RunStarted_creates_valid_context_name_if_safeitemname_is_not_valid_identifier()
        {
            var mockVsUtils = new Mock<IVsUtils>();
            var wizard = new OneEFWizard(vsUtils: mockVsUtils.Object);

            var mockCodeGenerator = new Mock<CodeFirstModelGenerator>(MockDTE.CreateProject());
            mockCodeGenerator
                .Setup(g => g.Generate(It.IsAny<DbModel>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new[] { new KeyValuePair<string, string>(string.Empty, string.Empty) });

            var settings = new ModelBuilderSettings { VSApplicationType = VisualStudioProjectSystem.WebApplication };
            var replacenentsDictionary = new Dictionary<string, string>
            {
                { "$safeitemname$", "3My.Con text" },
                { "$rootnamespace$", "Project" }
            };

            wizard.RunStarted(settings, mockCodeGenerator.Object, replacenentsDictionary);

            mockCodeGenerator.Verify(
                g => g.Generate(
                    It.IsAny<DbModel>(), 
                    "Project",
                    /*contextClassName*/ "_3MyContext",
                    /*connectionStringName*/ "_3MyContext"),
                Times.Once());
        }
    }
}