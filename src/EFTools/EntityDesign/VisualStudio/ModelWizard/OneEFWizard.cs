// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard
{
    using System.IO;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.Data.Entity.Design.CodeGeneration;
    using Microsoft.Data.Entity.Design.CodeGeneration.Generators;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.VisualStudio.Data.Framework;
    using Microsoft.VisualStudio.TemplateWizard;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Linq;
    using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

    /// <summary>
    /// This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class OneEFWizard : IWizard
    {
        internal static DbModel Model { set; private get; }

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
            _generatedCode = GenerateCode(
                (DTE2)automationObject, 
                replacementsDictionary["$rootnamespace$"], 
                replacementsDictionary["$safeitemname$"]).ToList();

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

        private IEnumerable<KeyValuePair<string, string>> GenerateCode(DTE2 dte, string codeNamespace, string contextClassName)
        {
            _project = VsUtils.GetActiveProject(dte);

            if (Model != null)
            {
                return new CodeFirstModelGenerator(
                    _project,
                    new ServiceProvider((IOleServiceProvider)dte))
                        .Generate(Model, codeNamespace);
            }
            else
            {
                if (VsUtils.GetLanguageForProject(_project) == LangEnum.VisualBasic)
                {
                    return new VBCodeFirstEmptyModelGenerator()
                        .Generate(codeNamespace, contextClassName);
                }
                else
                {
                    return new CSharpCodeFirstEmptyModelGenarator()
                        .Generate(codeNamespace, contextClassName);
                }
            }              
        }
    }
}
