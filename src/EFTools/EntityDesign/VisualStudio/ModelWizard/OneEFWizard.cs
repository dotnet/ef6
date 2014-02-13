// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard
{
    using EnvDTE;
    using Microsoft.Data.Entity.Design.CodeGeneration;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.TemplateWizard;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

    /// <summary>
    /// This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class OneEFWizard : IWizard
    {
        internal static ModelBuilderSettings ModelBuilderSettings { set; private get; }
        private ConfigFileUtils _configFileUtils;

        private List<KeyValuePair<string, string>> _generatedCode;
        private string _contextFilePath;

        /// <summary>
        /// This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public OneEFWizard()
        {
        }

        // testing only
        internal OneEFWizard(ConfigFileUtils configFileUtils)
        {
            _configFileUtils = configFileUtils;
            _generatedCode = new List<KeyValuePair<string, string>>();
        }

        /// <inheritdoc />
        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        /// <inheritdoc />
        public void ProjectFinishedGenerating(Project project)
        {
        }

        /// <inheritdoc />
        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
            _contextFilePath = projectItem.get_FileNames(1);
        }

        /// <inheritdoc />
        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            RunStarted(ModelBuilderSettings, new CodeFirstModelGenerator(ModelBuilderSettings.Project), replacementsDictionary);
        }

        internal void RunStarted(ModelBuilderSettings modelBuilderSettings, CodeFirstModelGenerator codeFirstModelGenerator, Dictionary<string, string> replacementsDictionary)
        {
            var contextClassName = replacementsDictionary["$safeitemname$"];

            _generatedCode = codeFirstModelGenerator.Generate(
                modelBuilderSettings.ModelBuilderEngine != null
                        ? modelBuilderSettings.ModelBuilderEngine.Model
                        : null,
                replacementsDictionary["$rootnamespace$"],
                contextClassName,
                modelBuilderSettings.SaveConnectionStringInAppConfig 
                    ? modelBuilderSettings.AppConfigConnectionPropertyName 
                    : contextClassName).ToList();

            Debug.Assert(_generatedCode.Count > 0, "code has not been generated");

            // TODO: verify if files don't exist
            // TODO: handle exceptions from code gen

            replacementsDictionary["$contextfilecontents$"] = _generatedCode[0].Value;
        }

        /// <inheritdoc />
        public void RunFinished()
        {
            RunFinished(ModelBuilderSettings, Path.GetDirectoryName(_contextFilePath));
        }

        // internal for testing, settings parameter to allow testing without messing with the static variable
        internal void RunFinished(ModelBuilderSettings settings, string targetDirectory)
        {
            var project = settings.Project;

            foreach (var generatedItem in _generatedCode.Skip(1))
            {
                VsUtils.AddNewFile(
                    project,
                    Path.Combine(targetDirectory, generatedItem.Key),
                    generatedItem.Value);
            }

            _configFileUtils =
                _configFileUtils ??
                    new ConfigFileUtils(
                    project,
                    PackageManager.Package,
                    settings.VSApplicationType);

            UpdateConfigFile(settings);
        }

        private void UpdateConfigFile(ModelBuilderSettings settings)
        {
            if (!settings.SaveConnectionStringInAppConfig)
            {
                return;
            }

            var connectionString = ConnectionManager.InjectEFAttributesIntoConnectionString(
                settings.AppConfigConnectionString, settings.RuntimeProviderInvariantName);

            _configFileUtils.GetOrCreateConfigFile();
            var configXml = _configFileUtils.LoadConfig();
            ConnectionManager.AddConnectionStringElement(
                configXml, 
                settings.AppConfigConnectionPropertyName,
                connectionString,
                settings.RuntimeProviderInvariantName);
            _configFileUtils.SaveConfig(configXml);
        }

        /// <inheritdoc />
        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}
