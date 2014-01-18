// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard
{
    using System.Collections.Generic;
    using System.Globalization;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties;
    using Microsoft.VisualStudio.TemplateWizard;

    /// <summary>
    /// This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class OneEFWizard : IWizard
    {
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
        }

        /// <inheritdoc />
        public void RunFinished()
        {
        }

        /// <inheritdoc />
        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            AddReplacements(VsUtils.GetActiveProject((DTE2)automationObject), replacementsDictionary);
        }

        /// <inheritdoc />
        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        private static void AddReplacements(Project project, Dictionary<string, string> replacementsDictionary)
        {
            string ctorCommentTemplate;
            string dbSetCommentTemplate;

            if (VsUtils.GetLanguageForProject(project) == LangEnum.VisualBasic)
            {
                ctorCommentTemplate = Resources.CodeFirstCodeFile_CtorComment_VB;
                dbSetCommentTemplate = Resources.CodeFirstCodeFile_DbSetComment_VB;
            }
            else
            {
                ctorCommentTemplate = Resources.CodeFirstCodeFile_CtorComment_CS;
                dbSetCommentTemplate = Resources.CodeFirstCodeFile_DbSetComment_CS;
            }

            // the item names used to get replacements must match names in the code file templates
            replacementsDictionary["$ctorcomment$"] =
                string.Format(
                    CultureInfo.CurrentCulture,
                    ctorCommentTemplate,
                    replacementsDictionary["$safeitemname$"],
                    replacementsDictionary["$rootnamespace$"]);

            replacementsDictionary["$dbsetcomment$"] = dbSetCommentTemplate;
        }
    }
}
