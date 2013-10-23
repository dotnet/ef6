// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.DataSourceWizardExtension
{
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using EnvDTE;
    using Microsoft.Data.Tools.XmlDesignerBase.Base.Util;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VSDesigner.Data.DataSourceWizard.Interface;

    [Export(typeof(DataSourceWizardPageBase))]
    internal class LaunchUpdateModelWizardPage : LaunchWizardPageBase
    {
        /// <summary>
        ///     Creates EDM Wizard launch page
        /// </summary>
        /// <param name="wizard">The wizard that will display the page</param>
        internal LaunchUpdateModelWizardPage(DataSourceWizardFormBase wizard)
            : base(wizard)
        {
        }

        protected override bool LaunchEntityDesignerWizard(EdmDataSourceWizardData wizardData)
        {
            Debug.Assert(wizardData.EDMProjectItem != null, "EDMProjectItem is null");
            if (wizardData.EDMProjectItem != null)
            {
                // We need to ensure that the edmx is opened in the designer.
                var window = wizardData.EDMProjectItem.Open(Constants.vsViewKindPrimary);
                Debug.Assert(window != null, "Unable to get window for created edmx file");
                if (window != null)
                {
                    window.Activate();
                }
                DoLaunchUpdateModelFromDatabaseWizard(wizardData.EDMProjectItem);
            }
            // always dismiss the DataSource Wizard.
            return true;
        }

        #region Helper function

        private static void DoLaunchUpdateModelFromDatabaseWizard(ProjectItem projectItem)
        {
            var uri = Utils.FileName2Uri(projectItem.get_FileNames(1));
            ModelManager modelManager = PackageManager.Package.ModelManager;
            var efArtifactSet = modelManager.GetArtifactSet(uri);

            Debug.Assert(efArtifactSet != null, "Could not find artifact for the following file:" + uri.AbsolutePath);

            if (efArtifactSet != null)
            {
                var entityDesignArtifact = efArtifactSet.GetEntityDesignArtifact();

                Debug.Assert(
                    entityDesignArtifact != null,
                    "In LaunchUpdateModelWizardPage: DoLaunchUpdateModelFromDatabaseWizard, entityDesignArtifact is null");

                if (entityDesignArtifact != null)
                {
                    UpdateFromDatabaseEngine.UpdateModelFromDatabase(entityDesignArtifact);
                }
            }
        }

        #endregion
    }
}
