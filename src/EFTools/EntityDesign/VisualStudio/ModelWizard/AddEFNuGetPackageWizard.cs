// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TemplateWizard;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Nu", Justification = "This is how you spell NuGet.")]
    public class AddEFNuGetPackageWizard : IWizard
    {
        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem)
        {
            throw new System.NotImplementedException();
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {
            throw new System.NotImplementedException();
        }

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem)
        {
            throw new System.NotImplementedException();
        }

        public void RunFinished()
        {
            throw new System.NotImplementedException();
        }

        public void RunStarted(object automationObject, System.Collections.Generic.Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            throw new System.NotImplementedException();
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            throw new System.NotImplementedException();
        }
    }
}
