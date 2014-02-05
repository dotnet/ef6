// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.Data.Entity.Design.Common;
    using Moq;
    using System;
    using Xunit;

    public class ConfigFilesUtilsTests
    {
        [Fact]
        public void ConfigFile_name_set_correctly()
        {
            var cases = new[]
            {
                new
                {
                    ApplicationType = VisualStudioProjectSystem.WebApplication,
                    ConfigFileName = "Web.Config"
                },
                new
                {
                    ApplicationType = VisualStudioProjectSystem.Website,
                    ConfigFileName = "Web.Config"
                },
                new
                {
                    ApplicationType = VisualStudioProjectSystem.WindowsApplication,
                    ConfigFileName = "App.Config"
                }
            };

            foreach (var testCase in cases)
            {
                var mockVsUtils = new Mock<IVsUtils>();
                mockVsUtils
                    .Setup(u => u.GetApplicationType(It.IsAny<IServiceProvider>(), It.IsAny<Project>()))
                    .Returns(testCase.ApplicationType);

                var configFileUtils = 
                    new ConfigFileUtils(Mock.Of<Project>(), Mock.Of<IServiceProvider>(), mockVsUtils.Object);

                Assert.Equal(testCase.ConfigFileName, configFileUtils.ConfigFileName);
            }
        }

        [Fact]
        public void GetConfigProjectItem_calls_into_FindFirstProjectItemWithName()
        {
            var mockVsUtils = new Mock<IVsUtils>();

            new ConfigFileUtils(Mock.Of<Project>(), Mock.Of<IServiceProvider>(), mockVsUtils.Object)
                .GetConfigProjectItem();

            mockVsUtils.Verify(
                u => u.FindFirstProjectItemWithName(It.IsAny<ProjectItems>(), It.IsAny<string>()), 
                Times.Once());
        }

        [Fact]
        public void CreateConfigFile_uses_correct_item_template_to_add_config_file()
        {
            var testCases = new[]
            {
                new
                {
                    ApplicationType = VisualStudioProjectSystem.WebApplication,
                    ProjectLanguage = LangEnum.CSharp,
                    ExpectedConfigItemTemplate = "WebConfig.zip",
                    ExpectedLanguage = "{349C5853-65DF-11DA-9384-00065B846F21}"
                },
                new
                {
                    ApplicationType = VisualStudioProjectSystem.WebApplication,
                    ProjectLanguage = LangEnum.VisualBasic,
                    ExpectedConfigItemTemplate = "WebConfig.zip",
                    ExpectedLanguage = "{349C5854-65DF-11DA-9384-00065B846F21}"
                },

                new
                {
                    ApplicationType = VisualStudioProjectSystem.Website,
                    ProjectLanguage = LangEnum.CSharp,
                    ExpectedConfigItemTemplate = "WebConfig.zip",
                    ExpectedLanguage = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}" // PrjKind.prjKindCSharpProject
                },
                new
                {
                    ApplicationType = VisualStudioProjectSystem.Website,
                    ProjectLanguage = LangEnum.VisualBasic,
                    ExpectedConfigItemTemplate = "WebConfig.zip",
                    ExpectedLanguage = "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}" // PrjKind.prjKindVBProject
                },
                new
                {
                    ApplicationType = VisualStudioProjectSystem.WindowsApplication,
                    ProjectLanguage = LangEnum.CSharp,
                    ExpectedConfigItemTemplate = "AppConfigInternal.zip",
                    ExpectedLanguage = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}" // PrjKind.prjKindCSharpProject
                },
                new
                {
                    ApplicationType = VisualStudioProjectSystem.WindowsApplication,
                    ProjectLanguage = LangEnum.VisualBasic,
                    ExpectedConfigItemTemplate = "AppConfigurationInternal.zip",
                    ExpectedLanguage = "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}" // PrjKind.prjKindVBProject
                },
            };

            foreach (var testCase in testCases)
            {
                var mockVsUtils = CreateMockVsUtils(testCase.ProjectLanguage, testCase.ApplicationType);
                var project = CreateMockProject(testCase.ProjectLanguage).Object;

                var configFileUtils =
                    new ConfigFileUtils(project, Mock.Of<IServiceProvider>(), mockVsUtils.Object);

                configFileUtils.CreateConfigFile();

                Mock.Get((Solution2)project.DTE.Solution)
                    .Verify(s => s.GetProjectItemTemplate(testCase.ExpectedConfigItemTemplate, testCase.ExpectedLanguage), Times.Once());
            }
        }

        private static Mock<IVsUtils> CreateMockVsUtils(LangEnum projectLanguage, VisualStudioProjectSystem applicationType)
        {
            var mockVsUtils = new Mock<IVsUtils>();
            mockVsUtils
                .Setup(u => u.GetApplicationType(It.IsAny<IServiceProvider>(), It.IsAny<Project>()))
                .Returns(applicationType);
            mockVsUtils
                .Setup(u => u.GetLanguageForProject(It.IsAny<Project>()))
                .Returns(projectLanguage);
            return mockVsUtils;
        }

        private static Mock<Project> CreateMockProject(LangEnum projectLanguage)
        {
            var mockSolution = new Mock<Solution2>();
            var mockDte = new Mock<DTE>();
            mockDte.Setup(d => d.Solution).Returns(mockSolution.As<Solution>().Object);
            var mockProjectItems = new Mock<ProjectItems>();
            var mockProject = new Mock<Project>();
            mockProject.Setup(p => p.DTE).Returns(mockDte.Object);
            mockProject.Setup(p => p.ProjectItems).Returns(mockProjectItems.Object);
            mockProject.Setup(p => p.Kind)
                .Returns(
                    projectLanguage == LangEnum.CSharp
                        ? "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"
                        : "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}");
            return mockProject;
        }
    }
}
