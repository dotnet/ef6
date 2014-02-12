// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.PlanCompiler;
    using System.Data.Entity.Infrastructure;
    using System.Xml;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.CodeGeneration;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Moq;
    using UnitTests.TestHelpers;
    using Xunit;

    public class OneEFWizardTests
    {
        [Fact]
        public void RunFinished_should_not_add_connection_string_to_config_file_if_SaveConnectionStringInAppConfig_false()
        {
            var mockConfig = new Mock<ConfigFileUtils>(Mock.Of<Project>(), Mock.Of<IServiceProvider>(), 
                VisualStudioProjectSystem.WindowsApplication, null, null);

            new OneEFWizard(mockConfig.Object)
                .RunFinished(new ModelBuilderSettings { SaveConnectionStringInAppConfig = false}, null);

            mockConfig.Verify(m => m.GetOrCreateConfigFile(), Times.Never());
            mockConfig.Verify(m => m.LoadConfig(), Times.Never());
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

            new OneEFWizard(mockConfig.Object)
                .RunFinished(settings, null);

            mockConfig.Verify(m => m.GetOrCreateConfigFile(), Times.Once());
            mockConfig.Verify(m => m.LoadConfig(), Times.Once());
            mockConfig.Verify(m => m.SaveConfig(
                It.Is<XmlDocument>(config => config.SelectSingleNode("/configuration/connectionStrings/add[@name='myConnStr']/@connectionString").Value ==
                @"data source=(localdb)\v11.0;initial catalog=App.MyContext;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework")), 
                Times.Once());
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

            new OneEFWizard()
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

            new OneEFWizard()
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

            new OneEFWizard()
                .RunStarted(modelBuilderSettings, mockCodeGenerator.Object, replacementsDictionary);

            mockCodeGenerator.Verify(g => g.Generate(It.IsAny<DbModel>(), "Project.Data", "MyContext", "MyContext"), Times.Once());
        }
    }
}