// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System;
    using System.Diagnostics;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.Data.Entity.Design.Common;
    using System.IO;

    internal class ConfigFileUtils
    {
        private const string AppConfigItemTemplateCs = "AppConfigInternal.zip";
        private const string AppConfigItemTemplateVb = "AppConfigurationInternal.zip";
        private const string WebConfigItemTemplate = "WebConfig.zip";
        private const string CsWebApplicationKind = "{349C5853-65DF-11DA-9384-00065B846F21}";
        private const string VbWebApplicationKind = "{349C5854-65DF-11DA-9384-00065B846F21}";

        private readonly IVsUtils _vsUtils;

        private readonly Project _project;
        private readonly IServiceProvider _serviceProvider;
        private readonly VisualStudioProjectSystem _applicationType;
        private readonly string _configFileName;

        public ConfigFileUtils(Project project, IServiceProvider serviceProvider, IVsUtils vsUtils = null)
        {
            Debug.Assert(project != null, "project is null");
            Debug.Assert(serviceProvider != null, "serviceProvider is null");

            _vsUtils = vsUtils ?? new VsUtilsWrapper();

            _project = project;
            _serviceProvider = serviceProvider;
            _applicationType = _vsUtils.GetApplicationType(_serviceProvider, _project);
            _configFileName = VsUtils.IsWebProject(_applicationType)
                ? VsUtils.WebConfigFileName
                : VsUtils.AppConfigFileName;
        }

        public string ConfigFileName
        {
            get { return _configFileName; }
        }

        public ProjectItem CreateConfigFile()
        {
            _project.ProjectItems.AddFromTemplate(GetConfigItempTemplatePath(), _configFileName);

            return GetConfigProjectItem();
        }

        public ProjectItem AddFromFile()
        {
            var projectDirectoryInfo = _vsUtils.GetProjectRoot(_project, _serviceProvider);
            var configFileInfo = new FileInfo(Path.Combine(projectDirectoryInfo.FullName, _configFileName));
            if (configFileInfo.Exists)
            {
                _project.ProjectItems.AddFromFile(configFileInfo.FullName);
                return GetConfigProjectItem();
            }

            return null;
        }

        public ProjectItem GetConfigProjectItem()
        {
            return _vsUtils.FindFirstProjectItemWithName(_project.ProjectItems, _configFileName);
        }

        private string GetConfigItempTemplatePath()
        {
            var projectLanguage = _vsUtils.GetLanguageForProject(_project);

            var configItemTemplate =
                VsUtils.IsWebProject(_applicationType)
                    ? WebConfigItemTemplate
                    : projectLanguage == LangEnum.VisualBasic
                        ? AppConfigItemTemplateVb 
                        : AppConfigItemTemplateCs;

            var solution2 = (Solution2)_project.DTE.Solution;

            if(_applicationType == VisualStudioProjectSystem.WebApplication)
            {
                return solution2.GetProjectItemTemplate(
                    WebConfigItemTemplate,
                    projectLanguage == LangEnum.VisualBasic ? VbWebApplicationKind : CsWebApplicationKind);
            }

            return solution2.GetProjectItemTemplate(configItemTemplate, _project.Kind);
        }
    }
}
