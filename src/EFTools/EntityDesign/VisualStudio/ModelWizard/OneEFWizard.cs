// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard
{
    using System;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.CodeGeneration;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.TemplateWizard;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
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
        private readonly IVsUtils _vsUtils;
        private readonly IErrorListHelper _errorListHelper;
        private readonly ModelGenErrorCache _errorCache;

        /// <summary>
        /// This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public OneEFWizard()
        {
            _vsUtils = new VsUtilsWrapper();
            _errorListHelper = new ErrorListHelperWrapper();
            _errorCache = PackageManager.Package.ModelGenErrorCache;
        }

        // testing only
        internal OneEFWizard(ConfigFileUtils configFileUtils = null, IVsUtils vsUtils = null, IErrorListHelper errorListHelper = null, ModelGenErrorCache errorCache = null, 
            List<KeyValuePair<string, string>> generatedCode = null)
        {
            _configFileUtils = configFileUtils;
            _generatedCode = generatedCode ?? new List<KeyValuePair<string, string>>();
            _vsUtils = vsUtils;
            _errorListHelper = errorListHelper;
            _errorCache = errorCache;
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

            AddErrors(projectItem);
        }

        private void AddErrors(ProjectItem projectItem)
        {
            var edmSchemaErrors = _errorCache.GetErrors(_contextFilePath);

            if(edmSchemaErrors != null && edmSchemaErrors.Any())
            {
                using (var serviceProvider = new ServiceProvider((IOleServiceProvider)projectItem.ContainingProject.DTE))
                {
                    var hierarchy = _vsUtils.GetVsHierarchy(projectItem.ContainingProject, serviceProvider);
                    var itemId = _vsUtils.GetProjectItemId(hierarchy, projectItem);

                    _errorListHelper.AddErrorInfosToErrorList(
                        edmSchemaErrors.Select(
                            e => new ErrorInfo(
                                e.Severity == EdmSchemaErrorSeverity.Error ? ErrorInfo.Severity.ERROR : ErrorInfo.Severity.WARNING,
                                e.Message,
                                _contextFilePath,
                                e.ErrorCode,
                                ErrorClass.Runtime_All)).ToList(),
                        hierarchy,
                        itemId);
                }
            }
        }

        /// <inheritdoc />
        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            RunStarted(ModelBuilderSettings, new CodeFirstModelGenerator(ModelBuilderSettings.Project), replacementsDictionary);
        }

        internal void RunStarted(ModelBuilderSettings modelBuilderSettings, CodeFirstModelGenerator codeFirstModelGenerator, Dictionary<string, string> replacementsDictionary)
        {
            try
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

                replacementsDictionary["$contextfilecontents$"] = _generatedCode[0].Value;
            }
            catch (CodeFirstModelGenerationException e)
            {
                _vsUtils.ShowErrorDialog(
                    string.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", e.Message, Environment.NewLine, e.InnerException));
            }
        }

        /// <inheritdoc />
        public void RunFinished()
        {
            RunFinished(ModelBuilderSettings, Path.GetDirectoryName(_contextFilePath));
        }

        // internal for testing, settings parameter to allow testing without messing with the static variable
        internal void RunFinished(ModelBuilderSettings settings, string targetDirectory)
        {
            // null indicates an error when generating code
            if (_generatedCode == null)
            {
                return;
            }

            var project = settings.Project;

            var filesToSave = _generatedCode.Skip(1)
                .ToDictionary(kvp => Path.Combine(targetDirectory, kvp.Key), kvp => (object)kvp.Value);

            try
            {
                _vsUtils.WriteCheckoutTextFilesInProject(filesToSave);
            }
            finally
            {
                // even if saving fails we actually might have created some files
                // and we should add them to the project
                AddFilesToProject(project, filesToSave.Keys);
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
            var existingConnectionStrings = ConnectionManager.GetExistingConnectionStrings(_configFileUtils);
            string existingConnectionString;
            if (existingConnectionStrings.TryGetValue(settings.AppConfigConnectionPropertyName, out existingConnectionString)
                && string.Equals(existingConnectionString, connectionString))
            {
                // An element with the same name and connectionString already exists - no need to update.
                // This can happen if the user chooses an existing connection and connection name on the WizardPageDbConfig page.
                return;
            }

            var configXml = _configFileUtils.LoadConfig();
            ConnectionManager.AddConnectionStringElement(
                configXml, 
                settings.AppConfigConnectionPropertyName,
                connectionString,
                settings.RuntimeProviderInvariantName);
            _configFileUtils.SaveConfig(configXml);
        }

        private static void AddFilesToProject(Project project, IEnumerable<string> paths)
        {
            foreach (var path in paths.Where(File.Exists))
            {
                project.ProjectItems.AddFromFile(path);
            }
        }

        /// <inheritdoc />
        public bool ShouldAddProjectItem(string filePath)
        {
            return _generatedCode != null && _generatedCode.Count > 0;
        }
    }
}
