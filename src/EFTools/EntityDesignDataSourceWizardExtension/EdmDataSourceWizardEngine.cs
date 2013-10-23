// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.Data.Entity.Design.DataSourceWizardExtension
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Drawing;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.VSDesigner.Data.DataSourceWizard.Interface;
    using Microsoft.VisualStudio.Shell;

    [Export(typeof(IDataSourceWizardEngine))]
    internal class EdmDataSourceWizardEngine : IDataSourceWizardEngine
    {
        private Bitmap _wizardIcon;
        private readonly DataSourceWizardPageBase[] _wizardPages = new DataSourceWizardPageBase[2];

        private const string SqlServerCompactFileExtension = ".sdf";
        private const string LocalDbFileExtension = ".mdf";
        private const string EdmxFileExtension = ".edmx";

        #region IDisplayable

        /// <summary>
        ///     Wizard Item display name.
        /// </summary>
        string IDisplayable.DisplayName
        {
            get { return Resources.WizardEngineDisplayName; }
        }

        string IDisplayable.Description
        {
            get
            {
                // TODO: update the description text in the resource string table.
                return Resources.WizardEngineDescription;
            }
        }

        Image IDisplayable.Image
        {
            get
            {
                if (_wizardIcon == null)
                {
                    _wizardIcon = Resources.EdmWizardIcon;
                }

                return _wizardIcon;
            }
        }

        #endregion

        #region IDataSourceWizardEngine Interface

        /// <summary>
        ///     This Engine should only be displayed under Database group.
        /// </summary>
        DataSourceGroup IDataSourceWizardEngine.DataSourceGroup
        {
            get { return DataSourceGroup.Database; }
        }

        /// <summary>
        ///     Return true if the wizard engine is supported for the project.
        /// </summary>
        bool IDataSourceWizardEngine.CanWorkInProject(Project project)
        {
            if (project != null)
            {
                var projectKind = VsUtils.GetProjectKind(project);
                // Currently, only VB, C# and Web projects are supported.
                if (projectKind == VsUtils.ProjectKind.CSharp
                    || projectKind == VsUtils.ProjectKind.Web
                    || projectKind == VsUtils.ProjectKind.VB)
                {
                    using (var serviceProvider =
                        new ServiceProvider((IOleServiceProvider)project.DTE))
                    {
                        return VsUtils
                            .EntityFrameworkSupportedInProject(project, serviceProvider, false);
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///     Return true if the wizard engine to reconfigure project item.
        /// </summary>
        bool IDataSourceWizardEngine.CanConfigureExisting(ProjectItem targetItem)
        {
            // this method is called frequently to determine whether wizard item should be displayed or not.
            // So we should not do heavy processing here.
            if (targetItem != null
                && !String.IsNullOrEmpty(targetItem.Name)
                && targetItem.Name.EndsWith(EdmxFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Return true if the engine could generate a new item based on a specified item.
        /// </summary>
        /// <param name="referencedItem"></param>
        /// <returns></returns>
        bool IDataSourceWizardEngine.CanConfigureNew(ProjectItem referencedItem)
        {
            // Currently EDM  supports being generated from local database file and service-based database file.
            if (referencedItem == null)
            {
                return true;
            }
            else if (!String.IsNullOrEmpty(referencedItem.Name)
                     && (referencedItem.Name.EndsWith(SqlServerCompactFileExtension, StringComparison.OrdinalIgnoreCase)
                         || referencedItem.Name.EndsWith(LocalDbFileExtension, StringComparison.OrdinalIgnoreCase))
                )
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Create a data bucket to hold data collected by the wizard engine.
        ///     This is called when the user selects Add New Data Source menu item
        /// </summary>
        /// <param name="wizardForm"></param>
        /// <param name="referencedItem"></param>
        /// <param name="targetCollection"></param>
        /// <returns></returns>
        IDataSourceWizardData IDataSourceWizardEngine.CreateData(
            DataSourceWizardFormBase wizardForm, ProjectItem referencedItem, ProjectItems targetCollection)
        {
            Debug.Assert(wizardForm != null, "Wizard form is null!");
            Debug.Assert(wizardForm.Project != null, "Could not determine the current project");

            var data = new EdmDataSourceWizardData
                {
                    ContainingProject = wizardForm.Project,
                };

            return data;
        }

        /// <summary>
        ///     Create a data bucket to hold data collected by the wizard engine.
        ///     This is called when the user selects Update Data Source menu item.
        /// </summary>
        /// <param name="wizardForm"></param>
        /// <param name="targetItem"></param>
        /// <returns></returns>
        IDataSourceWizardData IDataSourceWizardEngine.CreateData(DataSourceWizardFormBase wizardForm, ProjectItem targetItem)
        {
            Debug.Assert(wizardForm != null, "Wizard form is null");
            Debug.Assert(wizardForm.Project != null, "Could not determine the current project");
            Debug.Assert(targetItem != null, "the passed in targetItem is null");

            var data = new EdmDataSourceWizardData
                {
                    ContainingProject = wizardForm.Project,
                    EDMProjectItem = targetItem
                };
            return data;
        }

        /// <summary>
        ///     Clean data bucket
        /// </summary>
        /// <param name="wizardData"></param>
        void IDataSourceWizardEngine.CleanData(IDataSourceWizardData wizardData)
        {
            // Do nothing
        }

        /// <summary>
        ///     Initialize wizard pages.
        /// </summary>
        /// <param name="wizardForm"></param>
        void IDataSourceWizardEngine.InitializePages(DataSourceWizardFormBase wizardForm)
        {
            _wizardPages[0] = new LaunchModelGenWizardPage(wizardForm);
            _wizardPages[1] = new LaunchUpdateModelWizardPage(wizardForm);
        }

        DataSourceWizardPageBase IDataSourceWizardEngine.GetFirstPage(IDataSourceWizardData wizardData)
        {
            var edmWizardData = wizardData as EdmDataSourceWizardData;
            Debug.Assert(edmWizardData != null, "wizardData parameter is not of EdmDataSourceWizardData type");

            if (edmWizardData != null)
            {
                if (edmWizardData.EDMProjectItem != null)
                {
                    // Re-entrant mode, launch UpdateModelFromDatabase wizard
                    return _wizardPages[1];
                }
                else
                {
                    // Launch ModelGen wizard.
                    return _wizardPages[0];
                }
            }
            return null;
        }

        /// <summary>
        ///     Determine if the engine has collected enough information to perform work.
        /// </summary>
        bool IDataSourceWizardEngine.CanPerformWork(IDataSourceWizardData wizardData)
        {
            var edmWizardData = wizardData as EdmDataSourceWizardData;
            Debug.Assert(edmWizardData != null, "The passed in wizardData parameter is not of EdmDataSourceWizardData type");
            return (edmWizardData != null && edmWizardData.EDMProjectItem != null && !edmWizardData.IsCancelled);
        }

        ProjectItem[] IDataSourceWizardEngine.PerformWork(IDataSourceWizardData wizardData)
        {
            var edmWizardData = wizardData as EdmDataSourceWizardData;
            Debug.Assert(edmWizardData != null, "The passed in wizardData parameter is not of EdmDataSourceWizardData type");
            if (edmWizardData != null
                && edmWizardData.EDMProjectItem != null
                && !edmWizardData.IsCancelled)
            {
                // Running custom tool so code behind is generated correctly; for some reason currently this is not the case until we manually run custom tool.
                VsUtils.RunCustomTool(edmWizardData.EDMProjectItem);
                return new[] { edmWizardData.EDMProjectItem };
            }
            return null;
        }

        #endregion
    }
}
