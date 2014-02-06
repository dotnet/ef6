// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.VisualStudio.TextManager.Interop;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using Xunit;

    public class ConfigFileUtilsTests
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
                var configFileUtils =
                    new ConfigFileUtils(Mock.Of<Project>(), Mock.Of<IServiceProvider>(), testCase.ApplicationType);

                Assert.Equal(testCase.ConfigFileName, configFileUtils.ConfigFileName);
            }
        }

        [Fact]
        public void GetConfigProjectItem_calls_into_FindFirstProjectItemWithName()
        {
            var mockVsUtils = new Mock<IVsUtils>();

            new ConfigFileUtils(Mock.Of<Project>(), Mock.Of<IServiceProvider>(), vsUtils: mockVsUtils.Object)
                .GetConfigProjectItem();

            mockVsUtils.Verify(
                u => u.FindFirstProjectItemWithName(It.IsAny<ProjectItems>(), It.IsAny<string>()), 
                Times.Once());
        }

        [Fact]
        public void GetOrCreateConfigFile_does_not_create_config_file_if_config_already_exists()
        {
            var projectItem = Mock.Of<ProjectItem>();

            var mockVsUtils = CreateMockVsUtils(LangEnum.CSharp, VisualStudioProjectSystem.WebApplication);
            mockVsUtils
                .Setup(u => u.FindFirstProjectItemWithName(It.IsAny<ProjectItems>(), It.IsAny<string>()))
                .Returns(projectItem);

            var mockConfigFileUtils =
                new Mock<ConfigFileUtils>(Mock.Of<Project>(), Mock.Of<IServiceProvider>(), null, mockVsUtils.Object, null) 
                { CallBase = true };

            Assert.Same(
                projectItem,
                mockConfigFileUtils.Object.GetOrCreateConfigFile());

            mockConfigFileUtils.Verify(u => u.CreateConfigFile(), Times.Never());
        }

        [Fact]
        public void GetOrCreateConfigFile_creates_config_file_if_config_does_not_exists()
        {
            var projectItem = Mock.Of<ProjectItem>();

            var mockVsUtils = CreateMockVsUtils(LangEnum.CSharp, VisualStudioProjectSystem.WebApplication);
            mockVsUtils
                .Setup(u => u.FindFirstProjectItemWithName(It.IsAny<ProjectItems>(), It.IsAny<string>()))
                .Returns(projectItem);

            var mockConfigFileUtils =
                new Mock<ConfigFileUtils>(Mock.Of<Project>(), Mock.Of<IServiceProvider>(), null, mockVsUtils.Object, null) 
                { CallBase = true };

            mockConfigFileUtils
                .Setup(u => u.CreateConfigFile())
                .Returns(projectItem);

            Assert.Same(
                projectItem,
                mockConfigFileUtils.Object.GetOrCreateConfigFile());

            mockConfigFileUtils.Verify(u => u.CreateConfigFile(), Times.Never());
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
                    new ConfigFileUtils(project, Mock.Of<IServiceProvider>(), vsUtils: mockVsUtils.Object);

                configFileUtils.CreateConfigFile();

                Mock.Get((Solution2)project.DTE.Solution)
                    .Verify(s => s.GetProjectItemTemplate(testCase.ExpectedConfigItemTemplate, testCase.ExpectedLanguage), Times.Once());
            }
        }

        [Fact]
        public void LoadConfig_returns_null_if_config_does_not_exist()
        {
            var configFileUtils = new ConfigFileUtils(
                CreateMockProject(LangEnum.CSharp).Object,
                Mock.Of<IServiceProvider>(),
                vsUtils: CreateMockVsUtils(LangEnum.CSharp, VisualStudioProjectSystem.WebApplication).Object);

            Assert.Null(configFileUtils.LoadConfig());
        }

        [Fact]
        public void LoadConfig_loads_config_from_IVsTextLines()
        {
            var configContents = "<config />";
            const string configFilePath = "configfilepath";
            var mockTextLines = new Mock<IVsTextLines>();
            mockTextLines.Setup(
                l => l.GetLineText(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), out configContents));

            var mockVsHelpers = new Mock<IVsHelpers>();
            mockVsHelpers
                .Setup(h => h.GetDocData(It.IsAny<IServiceProvider>(), It.IsAny<string>()))
                .Returns(mockTextLines.Object);

            var mockConfigProjectItem = new Mock<ProjectItem>();
            mockConfigProjectItem.Setup(i => i.get_FileNames(It.IsAny<short>())).Returns(configFilePath);

            var mockVsUtils = CreateMockVsUtils(LangEnum.CSharp, VisualStudioProjectSystem.WebApplication);
            mockVsUtils
                .Setup(u => u.FindFirstProjectItemWithName(It.IsAny<ProjectItems>(), It.IsAny<string>()))
                .Returns(mockConfigProjectItem.Object);

            var configFileUtils = new ConfigFileUtils(
                CreateMockProject(LangEnum.CSharp).Object,
                Mock.Of<IServiceProvider>(),
                null,
                mockVsUtils.Object,
                mockVsHelpers.Object);

            Assert.Equal(configContents, configFileUtils.LoadConfig().OuterXml);
            mockVsHelpers.Verify(h => h.GetDocData(It.IsAny<IServiceProvider>(), configFilePath), Times.Once());
        }

        [Fact]
        public void SaveConfig_invokes_WriteCheckoutXmlFilesInProject()
        {
            const string configFilePath = "configfilepath";
            var mockConfigProjectItem = new Mock<ProjectItem>();
            mockConfigProjectItem.Setup(i => i.get_FileNames(It.IsAny<short>())).Returns(configFilePath);

            var mockVsUtils = CreateMockVsUtils(LangEnum.CSharp, VisualStudioProjectSystem.WebApplication);
            mockVsUtils
                .Setup(u => u.FindFirstProjectItemWithName(It.IsAny<ProjectItems>(), It.IsAny<string>()))
                .Returns(mockConfigProjectItem.Object);

            var configXml = new XmlDocument();
            configXml.LoadXml("<config />");

            new ConfigFileUtils(
                CreateMockProject(LangEnum.CSharp).Object,
                Mock.Of<IServiceProvider>(),
                vsUtils: mockVsUtils.Object).SaveConfig(configXml);

            mockVsUtils.Verify(
                u => u.WriteCheckoutXmlFilesInProject(
                    It.Is<IDictionary<string, object>>(
                        d => 
                            d.Count == 1 && 
                            d.ContainsKey(configFilePath) && 
                            ReferenceEquals(d[configFilePath], configXml))), Times.Once());
        }

        [Fact]
        public void ConfigFileExists_returns_true_if_config_project_item_exisits()
        {
            var mockVsUtils = CreateMockVsUtils(LangEnum.CSharp, VisualStudioProjectSystem.WebApplication);
            mockVsUtils
                .Setup(u => u.FindFirstProjectItemWithName(It.IsAny<ProjectItems>(), It.IsAny<string>()))
                .Returns(Mock.Of<ProjectItem>());

            Assert.True(
                new ConfigFileUtils(Mock.Of<Project>(), Mock.Of<IServiceProvider>(), vsUtils: mockVsUtils.Object)
                    .ConfigFileExists());
        }

        [Fact]
        public void ConfigFileExists_returns_false_if_config_project_item_does_not_exisit()
        {
            Assert.False(
                new ConfigFileUtils(Mock.Of<Project>(), Mock.Of<IServiceProvider>(), vsUtils: Mock.Of<IVsUtils>())
                    .ConfigFileExists());
        }

        [Fact]
        public void GetConfigPath_returns_path_if_config_project_item_exisit()
        {
            const string configFilePath = "configfilepath";
            var mockConfigProjectItem = new Mock<ProjectItem>();
            mockConfigProjectItem.Setup(i => i.get_FileNames(It.IsAny<short>())).Returns(configFilePath);

            var mockVsUtils = CreateMockVsUtils(LangEnum.CSharp, VisualStudioProjectSystem.WebApplication);
            mockVsUtils
                .Setup(u => u.FindFirstProjectItemWithName(It.IsAny<ProjectItems>(), It.IsAny<string>()))
                .Returns(mockConfigProjectItem.Object);

            Assert.Equal(
                "configfilepath",
                new ConfigFileUtils(Mock.Of<Project>(), Mock.Of<IServiceProvider>(), vsUtils: mockVsUtils.Object)
                    .GetConfigPath());
        }

        [Fact]
        public void GetConfigPath_returns_null_if_config_project_item_does_not_exisit()
        {
            Assert.Null(
                new ConfigFileUtils(Mock.Of<Project>(), Mock.Of<IServiceProvider>(), vsUtils: Mock.Of<IVsUtils>())
                    .GetConfigPath());
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
            mockVsUtils
                .Setup(u => u.GetProjectRoot(It.IsAny<Project>(), It.IsAny<IServiceProvider>()))
                .Returns(new DirectoryInfo(Directory.GetCurrentDirectory()));
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
