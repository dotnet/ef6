// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.DataSourceWizardExtension
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.VSDesigner.Data.DataSourceWizard.Interface;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.TemplateWizard;

    [Export(typeof(DataSourceWizardPageBase))]
    internal class LaunchModelGenWizardPage : LaunchWizardPageBase
    {
        private const string AdoNetEntityDataModelCSharp = "AdoNetEntityDataModelCSharp.zip";
        private const string AdoNetEntityDataModelVB = "AdoNetEntityDataModelVB.zip";
        private const string AdoNetEntityDataModelAspNetCSharp = "AdoNetEntityDataModelCSharp_ASPNET.zip";
        private const string AdoNetEntityDataModelAspNetVB = "AdoNetEntityDataModelVB_ASPNET.zip";

        /// <summary>
        ///     Creates EDM Wizard launch page
        /// </summary>
        /// <param name="wizard">The wizard that will display the page</param>
        internal LaunchModelGenWizardPage(DataSourceWizardFormBase wizard)
            : base(wizard)
        {
        }

        protected override bool LaunchEntityDesignerWizard(EdmDataSourceWizardData wizardData)
        {
            var containingProject = wizardData.ContainingProject;
            var itemTemplatePath = GetEdmxItemTemplatePath(containingProject);
            var edmxFileName = GetUniqueEdmxFileNameInProject(containingProject, Resources.EdmDesignerFileNameFormat);
            try
            {
                containingProject.ProjectItems.AddFromTemplate(itemTemplatePath, edmxFileName);
            }
            catch (COMException ex)
            {
                if (ex.ErrorCode == VSConstants.OLE_E_PROMPTSAVECANCELLED)
                {
                    // replace COMException indicating that the user cancelled the Entity Designer Wizard with
                    // a WizardCancelledException
                    throw new WizardCancelledException();
                }
                else
                {
                    throw;
                }
            }

            wizardData.EDMProjectItem = VsUtils.GetProjectItemForDocument(edmxFileName, WizardForm.ServiceProvider);
            // Dismiss Data Source Wizard if a new edmx file is successfuly created.
            return (wizardData.EDMProjectItem != null);
        }

        #region Helper methods

        /// <summary>
        ///     Generate a unique project item name
        /// </summary>
        private static string GetUniqueEdmxFileNameInProject(Project project, string formatModelName)
        {
            const int numberMaxTries = 100000;

            // The edmx will always be added under root folder.
            var rootDirectory = VsUtils.GetProjectRoot(project, Services.ServiceProvider);
            var generatedModelName = String.Empty;
            for (var i = 1; i < numberMaxTries; i++)
            {
                generatedModelName = String.Format(CultureInfo.CurrentCulture, formatModelName, i);
                if (!File.Exists(rootDirectory.FullName + "\\" + generatedModelName))
                {
                    return generatedModelName;
                }
            }
            Debug.Assert(false, "Could not generated unique project item name.");
            return generatedModelName;
        }

        /// <summary>
        ///     Retrives edmx item template path.
        /// </summary>
        private static string GetEdmxItemTemplatePath(Project project)
        {
            var solution2 = project.DTE.Solution as Solution2;
            var templateName = GetItemTemplateName(project);
            return solution2.GetProjectItemTemplate(templateName, project.Kind);
        }

        /// <summary>
        ///     Get template name for a given project.
        ///     TODO: we should be able to move this code to a common place.
        /// </summary>
        private static string GetItemTemplateName(Project project)
        {
            var projectKind = VsUtils.GetProjectKind(project);

            if (projectKind == VsUtils.ProjectKind.CSharp)
            {
                return AdoNetEntityDataModelCSharp;
            }
            else if (projectKind == VsUtils.ProjectKind.VB)
            {
                return AdoNetEntityDataModelVB;
            }
            else if (projectKind == VsUtils.ProjectKind.Web)
            {
                if (VsUtils.IsWebSiteVBProject(project))
                {
                    return AdoNetEntityDataModelAspNetCSharp;
                }
                else if (VsUtils.IsWebSiteCSharpProject(project))
                {
                    return AdoNetEntityDataModelAspNetVB;
                }
            }

            Debug.Fail("Could not find item template name for: " + projectKind.ToString("g"));
            return string.Empty;
        }

        #endregion
    }
}
