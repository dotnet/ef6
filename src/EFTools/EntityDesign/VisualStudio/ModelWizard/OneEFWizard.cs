// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard
{
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.Data.Entity.Design.CodeGeneration;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.VisualStudio.TemplateWizard;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class OneEFWizard : IWizard
    {
        internal static ModelBuilderSettings ModelBuilderSettings { set; private get; }

        private List<KeyValuePair<string, string>> _generatedCode;
        private Project _project;
        private string _contextFilePath;

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
            var dte = (DTE2)automationObject;
            _project = VsUtils.GetActiveProject(dte);

            _generatedCode = GenerateCode(
                replacementsDictionary["$rootnamespace$"],
                replacementsDictionary["$safeitemname$"],
                ModelBuilderSettings.AppConfigConnectionPropertyName).ToList();

            Debug.Assert(_generatedCode.Count > 0, "code has not been generated");

            // TODO: verify if files don't exist
            // TODO: handle exceptions from code gen

            replacementsDictionary["$contextfilecontents$"] = _generatedCode[0].Value;
        }

        /// <inheritdoc />
        public void RunFinished()
        {
            var targetDirectory = Path.GetDirectoryName(_contextFilePath);

            foreach (var generatedItem in _generatedCode.Skip(1))
            {
                VsUtils.AddNewFile(
                    _project,
                    Path.Combine(targetDirectory, generatedItem.Key),
                    generatedItem.Value);
            }
        }

        /// <inheritdoc />
        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        private IEnumerable<KeyValuePair<string, string>> GenerateCode(string codeNamespace, string contextClassName, string connectionStringName)
        {
            return new CodeFirstModelGenerator(_project)
                .Generate(
                    ModelBuilderSettings.ModelBuilderEngine != null
                        ? ModelBuilderSettings.ModelBuilderEngine.Model
                        : null,
                    codeNamespace,
                    contextClassName,
                    connectionStringName);
        }
    }
}
