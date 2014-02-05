// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Xml;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.Data.Entity.Design.Common;
    using System.IO;
    using Microsoft.VisualStudio.TextManager.Interop;
    using System.Globalization;

    internal class ConfigFileUtils
    {
        private const string AppConfigItemTemplateCs = "AppConfigInternal.zip";
        private const string AppConfigItemTemplateVb = "AppConfigurationInternal.zip";
        private const string WebConfigItemTemplate = "WebConfig.zip";
        private const string CsWebApplicationKind = "{349C5853-65DF-11DA-9384-00065B846F21}";
        private const string VbWebApplicationKind = "{349C5854-65DF-11DA-9384-00065B846F21}";

        private readonly IVsUtils _vsUtils;
        private readonly IVsHelpers _vsHelpers;

        private readonly Project _project;
        private readonly IServiceProvider _serviceProvider;
        private readonly VisualStudioProjectSystem _applicationType;
        private readonly string _configFileName;

        public ConfigFileUtils(Project project, IServiceProvider serviceProvider, IVsUtils vsUtils = null, IVsHelpers vsHelpers = null)
        {
            Debug.Assert(project != null, "project is null");
            Debug.Assert(serviceProvider != null, "serviceProvider is null");

            _vsUtils = vsUtils ?? new VsUtilsWrapper();
            _vsHelpers = vsHelpers ?? new VsHelpersWrapper();

            _project = project;
            _serviceProvider = serviceProvider;
            _applicationType = _vsUtils.GetApplicationType(_serviceProvider, _project);
            _configFileName = VsUtils.IsWebProject(_applicationType)
                ? VsUtils.WebConfigFileName
                : VsUtils.AppConfigFileName;
        }

        public ProjectItem GetOrCreateConfigFile()
        {
            if (!ConfigFileExists())
            {
                CreateConfigFile();
            }

            return GetConfigProjectItem();
        }

        public string ConfigFileName
        {
            get { return _configFileName; }
        }

        // virtual for testing
        public virtual ProjectItem CreateConfigFile()
        {
            Debug.Assert(GetConfigProjectItem() == null, "Config file already exists");

            var configProjectItem = AddFromFile();
            if (configProjectItem != null)
            {
                return configProjectItem;
            }

            _project.ProjectItems.AddFromTemplate(GetConfigItempTemplatePath(), _configFileName);
            return GetConfigProjectItem();
        }

        private ProjectItem AddFromFile()
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

        // virtual for testing
        public virtual XmlDocument LoadConfig()
        {
            var configFilePath = GetConfigPath();
            if (configFilePath != null)
            {
                // attempt to construct the config xml from the doc data if it is available
                try
                {
                    var textLines = _vsHelpers.GetDocData(_serviceProvider, configFilePath) as IVsTextLines;
                    return textLines != null
                        ? EdmUtils.SafeLoadXmlFromString(
                            VSHelpers.GetTextFromVsTextLines(textLines), preserveWhitespace: true)
                        : EdmUtils.SafeLoadXmlFromPath(configFilePath);
                }
                catch (XmlException e)
                {
                    VsUtils.LogStandardError(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            Resources.VSUtils_ExceptionParsingXml,
                            configFilePath,
                            e.Message),
                        configFilePath,
                        e.LineNumber,
                        e.LinePosition);

                    throw;
                }
            }

            return null;
        }

        // virtual for testing
        public virtual void SaveConfig(XmlDocument contents)
        {
            Debug.Assert(contents != null, "contents is null");

            var configFilePath = GetConfigPath();

            Debug.Assert(configFilePath != null, "config project item does not exist");

            try
            {
                _vsUtils.WriteCheckoutXmlFilesInProject(new Dictionary<string, object> { { configFilePath, contents } });
            }
            catch (Exception e)
            {
                VsUtils.LogOutputWindowPaneMessage(
                    _project,
                    string.Format(
                        CultureInfo.CurrentCulture, 
                        Resources.ConnectionManager_SaveXmlError, 
                        configFilePath, 
                        e.Message));
                throw;
            }
        }

        public bool ConfigFileExists()
        {
            return GetConfigProjectItem() != null;
        }

        public ProjectItem GetConfigProjectItem()
        {
            return _vsUtils.FindFirstProjectItemWithName(_project.ProjectItems, _configFileName);
        }

        public string GetConfigPath()
        {
            var configProjectItem = GetConfigProjectItem();
            return configProjectItem != null ? configProjectItem.FileNames[1] : null;
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
