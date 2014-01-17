// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Threading;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.DatabaseGeneration;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.UI.Views.Dialogs;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.WizardFramework;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal partial class WizardPageDbGenSummary : WizardPageBase
    {
        internal WorkflowApplication _workflowInstance;
        private Label _statusLabel;
        private SynchronizationContext _synchronizationContext;
        private bool _addedDbConfigPage;
        private bool _onWorkflowCleanup;
        private string _ddlFileExtension;

        // TODO: create strongly-typed properties in an options page type to store this information
        private const string RegKeyNameDdlOverwriteWarning = "DbGenShowOverwriteDDLWarning";
        private const string RegKeyNameEdmxOverwriteWarning = "DbGenShowEdmxOverwriteWarning";
        private const string RegKeyNameCustomWorkflowWarning = "DbGenShowCustomWorkflowWarning";

        internal WizardPageDbGenSummary(ModelBuilderWizardForm wizard)
            : base(wizard)
        {
            InitializeComponent();

            Logo = Properties.Resources.PageIcon;
            Headline = Properties.Resources.DbGenSummary_Title;
            Id = "WizardPageGenerateDatabaseScriptId";
            ShowInfoPanel = false;

            HelpKeyword = null;

            _addedDbConfigPage = false;
        }

        public override bool OnActivate()
        {
            if (!Visited)
            {
                SummaryTabs.Enabled = false;
                txtSaveDdlAs.Enabled = false;
            }

            return base.OnActivate();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public override void OnActivated()
        {
            base.OnActivated();

            _onWorkflowCleanup = false;

            Debug.Assert(
                !Wizard.MovingNext || _workflowInstance == null,
                "Possible memory leak: We should have destroyed the old workflow instance when activating WizardPageDbGenSummary");

            if (_workflowInstance == null)
            {
                if (LocalDataUtil.IsSqlMobileConnectionString(Wizard.ModelBuilderSettings.DesignTimeProviderInvariantName))
                {
                    _ddlFileExtension = DatabaseGenerationEngine._sqlceFileExtension;
                }
                else
                {
                    _ddlFileExtension = DatabaseGenerationEngine._ddlFileExtension;
                }

                // Add in the DbConfig page before if we have found a connection
                if (Wizard.Mode == ModelBuilderWizardForm.WizardMode.PerformDBGenSummaryOnly
                    && _addedDbConfigPage == false)
                {
                    Wizard.InsertPageBefore(Id, new WizardPageDbConfig(Wizard));
                    _addedDbConfigPage = true;
                }

                // Display the default path for the DDL
                var artifactProjectItem = VsUtils.GetProjectItemForDocument(
                    Wizard.ModelBuilderSettings.Artifact.Uri.LocalPath, PackageManager.Package);
                if (artifactProjectItem != null)
                {
                    txtSaveDdlAs.Text = DatabaseGenerationEngine.CreateDefaultDdlFileName(artifactProjectItem) + _ddlFileExtension;
                }

                // Disable all buttons except for Previous and Cancel
                Wizard.EnableButton(ButtonType.Previous, true);
                Wizard.EnableButton(ButtonType.Next, false);
                Wizard.EnableButton(ButtonType.Finish, false);
                Wizard.EnableButton(ButtonType.Cancel, true);

                // Display a status message
                ShowStatus(Properties.Resources.DbGenSummary_StatusDeterminingDDL);

                // Extract the XML from the EDMX file and convert it into an EdmItemCollection for the workflow
                EdmItemCollection edm = null;
                using (new VsUtils.HourglassHelper())
                {
                    IList<EdmSchemaError> schemaErrors;
                    edm = DatabaseGenerationEngine.GetEdmItemCollectionFromArtifact(Wizard.ModelBuilderSettings.Artifact, out schemaErrors);

                    Debug.Assert(
                        edm != null && schemaErrors.Count == 0,
                        "EdmItemCollection schema errors found; we should have performed validation on the EdmItemCollection before instantiating the wizard.");
                }

                var existingSsdl = DatabaseGenerationEngine.GetSsdlFromArtifact(Wizard.ModelBuilderSettings.Artifact);
                var existingMsl = DatabaseGenerationEngine.GetMslFromArtifact(Wizard.ModelBuilderSettings.Artifact);

                // Attempt to get the workflow path, template path, and database schema name from the artifact. If we don't find them, we'll use defaults.
                var workflowPath = DatabaseGenerationEngine.GetWorkflowPathFromArtifact(Wizard.ModelBuilderSettings.Artifact);
                var templatePath = DatabaseGenerationEngine.GetTemplatePathFromArtifact(Wizard.ModelBuilderSettings.Artifact);
                var databaseSchemaName = DatabaseGenerationEngine.GetDatabaseSchemaNameFromArtifact(Wizard.ModelBuilderSettings.Artifact);

                // Save off the SynchronizationContext so we can post methods to the UI event queue when
                // responding to workflow events (since they are executed in a separate thread)
                _synchronizationContext = SynchronizationContext.Current;

                // Invoke the Pipeline/Workflow. The Workflow engine will automatically wrap this in a background thread
                try
                {
                    using (new VsUtils.HourglassHelper())
                    {
                        var resolvedWorkflowFileInfo = DatabaseGenerationEngine.ResolveAndValidateWorkflowPath(
                            Wizard.Project,
                            workflowPath);

                        var resolvedDefaultPath = VsUtils.ResolvePathWithMacro(
                            null, DatabaseGenerationEngine.DefaultWorkflowPath,
                            new Dictionary<string, string>
                                {
                                    { ExtensibleFileManager.EFTOOLS_USER_MACRONAME, ExtensibleFileManager.UserEFToolsDir.FullName },
                                    { ExtensibleFileManager.EFTOOLS_VS_MACRONAME, ExtensibleFileManager.VSEFToolsDir.FullName }
                                });

                        // Display a security warning if the workflow path specified is different from the default
                        if (!resolvedWorkflowFileInfo.FullName.Equals(
                            Path.GetFullPath(resolvedDefaultPath), StringComparison.OrdinalIgnoreCase))
                        {
                            var displayCustomWorkflowWarning = true;
                            try
                            {
                                var customWorkflowWarningString = EdmUtils.GetUserSetting(RegKeyNameCustomWorkflowWarning);
                                if (false == String.IsNullOrEmpty(customWorkflowWarningString)
                                    && false == Boolean.TryParse(customWorkflowWarningString, out displayCustomWorkflowWarning))
                                {
                                    displayCustomWorkflowWarning = true;
                                }
                                if (displayCustomWorkflowWarning)
                                {
                                    var cancelledDuringCustomWorkflowWarning = DismissableWarningDialog
                                        .ShowWarningDialogAndSaveDismissOption(
                                            Resources.DatabaseCreation_CustomWorkflowWarningTitle,
                                            Resources.DatabaseCreation_WarningCustomWorkflow,
                                            RegKeyNameCustomWorkflowWarning,
                                            DismissableWarningDialog.ButtonMode.OkCancel);
                                    if (cancelledDuringCustomWorkflowWarning)
                                    {
                                        HandleError(
                                            String.Format(
                                                CultureInfo.CurrentCulture, Resources.DatabaseCreation_CustomWorkflowCancelled,
                                                resolvedWorkflowFileInfo.FullName), false);
                                        return;
                                    }
                                }
                            }
                            catch (SecurityException e)
                            {
                                // We should at least alert the user of why this is failing so they can take steps to fix it.
                                VsUtils.ShowMessageBox(
                                    Services.ServiceProvider,
                                    String.Format(
                                        CultureInfo.CurrentCulture, Resources.ErrorReadingWritingUserSetting,
                                        RegKeyNameCustomWorkflowWarning, e.Message),
                                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                                    OLEMSGICON.OLEMSGICON_WARNING);
                            }
                        }

                        _workflowInstance = DatabaseGenerationEngine.CreateDatabaseScriptGenerationWorkflow(
                            _synchronizationContext,
                            Wizard.Project,
                            Wizard.ModelBuilderSettings.Artifact.Uri.LocalPath,
                            resolvedWorkflowFileInfo,
                            templatePath,
                            edm,
                            existingSsdl,
                            existingMsl,
                            databaseSchemaName,
                            Wizard.ModelBuilderSettings.InitialCatalog,
                            Wizard.ModelBuilderSettings.RuntimeProviderInvariantName,
                            Wizard.ModelBuilderSettings.AppConfigConnectionString,
                            Wizard.ModelBuilderSettings.ProviderManifestToken,
                            Wizard.ModelBuilderSettings.Artifact.SchemaVersion,
                            _workflowInstance_WorkflowCompleted,
                            _workflowInstance_UnhandledException);
                    }

                    Wizard.ModelBuilderSettings.WorkflowInstance = _workflowInstance;

                    _workflowInstance.Run();
                }
                catch (Exception e)
                {
                    HandleError(e.Message, true);

                    if (_workflowInstance != null)
                    {
                        CleanupWorkflow();
                    }
                }
            }
        }

        private UnhandledExceptionAction _workflowInstance_UnhandledException(WorkflowApplicationUnhandledExceptionEventArgs e)
        {
            _synchronizationContext.Post(
                state =>
                    {
                        if (e.UnhandledException != null)
                        {
                            HandleError(e.UnhandledException.Message, true);
                        }
                    }, null);
            return UnhandledExceptionAction.Terminate;
        }

        /// <summary>
        ///     Invoked by the VS Wizard framework when this page is exited or when the "Finish" button is clicked.
        ///     Updates ModelBuilderSettings from the GUI
        /// </summary>
        public override bool OnDeactivate()
        {
            if (Wizard.MovingPrevious)
            {
                CleanupWorkflow();
            }

            return base.OnDeactivate();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal override bool OnWizardFinish()
        {
            if (Wizard.ModelBuilderSettings.DdlStringReader != null)
            {
                // Make sure that the DDL filename is not null
                if (String.IsNullOrEmpty(txtSaveDdlAs.Text))
                {
                    VsUtils.ShowErrorDialog(Properties.Resources.ErrorDdlFileNameIsNull);
                    return false;
                }

                // Resolve the project URI
                Uri projectUri = null;
                bool projectHasFilename;
                var projectFullName = VsUtils.GetProjectPathWithName(Wizard.Project, out projectHasFilename);

                try
                {
                    if (false == Uri.TryCreate(projectFullName, UriKind.Absolute, out projectUri)
                        || projectUri == null)
                    {
                        VsUtils.ShowErrorDialog(
                            String.Format(CultureInfo.CurrentCulture, Properties.Resources.ErrorResolvingProjectFile, projectFullName));
                        return false;
                    }
                }
                catch (UriFormatException)
                {
                    VsUtils.ShowErrorDialog(
                        String.Format(CultureInfo.CurrentCulture, Properties.Resources.ErrorResolvingProjectFile, projectFullName));
                }

                // Attempt to create a URI from the DDL path, either relative to the project URI or absolute. 
                Uri ddlUri = null;
                try
                {
                    if (false == Uri.TryCreate(projectUri, txtSaveDdlAs.Text, out ddlUri)
                        || ddlUri == null)
                    {
                        VsUtils.ShowErrorDialog(
                            String.Format(
                                CultureInfo.CurrentCulture, Properties.Resources.ErrorResolvingDdlFileNameException, txtSaveDdlAs.Text));
                        return false;
                    }
                }
                catch (UriFormatException)
                {
                    VsUtils.ShowErrorDialog(
                        String.Format(
                            CultureInfo.CurrentCulture, Properties.Resources.ErrorResolvingDdlFileNameException, txtSaveDdlAs.Text));
                }

                var ddlFilePath = ddlUri.LocalPath;

                // Validate the file name
                try
                {
                    var ddlFileName = Path.GetFileName(ddlFilePath);
                    if (String.IsNullOrEmpty(ddlFileName))
                    {
                        VsUtils.ShowErrorDialog(
                            String.Format(CultureInfo.CurrentCulture, Properties.Resources.ErrorDdlPathNotFile, ddlFilePath));
                        return false;
                    }

                    if (!VsUtils.IsValidFileName(ddlFileName))
                    {
                        VsUtils.ShowErrorDialog(String.Format(CultureInfo.CurrentCulture, Resources.ErrorNonValidFileName, ddlFilePath));
                        return false;
                    }
                }
                catch (ArgumentException)
                {
                    VsUtils.ShowErrorDialog(
                        String.Format(CultureInfo.CurrentCulture, Properties.Resources.ErrorResolvingDdlFileNameException, ddlFilePath));
                    return false;
                }

                // Add ".sql" if the extension is not already .sql
                if (!Path.GetExtension(ddlFilePath).Equals(_ddlFileExtension, StringComparison.OrdinalIgnoreCase))
                {
                    ddlFilePath += _ddlFileExtension;
                }

                // Now we should have a valid, non-null filename
                Debug.Assert(
                    !String.IsNullOrEmpty(ddlFilePath),
                    "DDL filename should either be not null or we should have handled an exception before continuing...");
                if (String.IsNullOrEmpty(ddlFilePath))
                {
                    VsUtils.ShowErrorDialog(Properties.Resources.ErrorDdlFileNameIsNull);
                    return false;
                }

                // If the parent directory does not exist, then we do not proceed
                try
                {
                    var fileInfo = new FileInfo(ddlFilePath);
                    var parentDirInfo = fileInfo.Directory;
                    if (parentDirInfo != null)
                    {
                        if (false == parentDirInfo.Exists)
                        {
                            VsUtils.ShowErrorDialog(
                                String.Format(CultureInfo.CurrentCulture, Properties.Resources.ErrorNoDdlParentDir, ddlFilePath));
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    // various exceptions could occur here, such as PathTooLong or Security. In this case we will display an error.
                    VsUtils.ShowErrorDialog(
                        String.Format(
                            CultureInfo.CurrentCulture, Properties.Resources.ErrorCouldNotParseDdlFileName, ddlFilePath, e.Message));
                    return false;
                }

                // Display the DDL Overwrite Warning Dialog
                if (File.Exists(ddlFilePath))
                {
                    var displayDdlOverwriteWarning = true;
                    try
                    {
                        var ddlOverwriteWarningString = EdmUtils.GetUserSetting(RegKeyNameDdlOverwriteWarning);
                        if (false == String.IsNullOrEmpty(ddlOverwriteWarningString)
                            && false == Boolean.TryParse(ddlOverwriteWarningString, out displayDdlOverwriteWarning))
                        {
                            displayDdlOverwriteWarning = true;
                        }
                        if (displayDdlOverwriteWarning)
                        {
                            var cancelledDuringOverwriteDdl = DismissableWarningDialog.ShowWarningDialogAndSaveDismissOption(
                                Resources.DatabaseCreation_DDLOverwriteWarningTitle,
                                String.Format(CultureInfo.CurrentCulture, Resources.DatabaseCreation_WarningOverwriteDdl, ddlFilePath),
                                RegKeyNameDdlOverwriteWarning,
                                DismissableWarningDialog.ButtonMode.YesNo);
                            if (cancelledDuringOverwriteDdl)
                            {
                                return false;
                            }
                        }
                    }
                    catch (SecurityException e)
                    {
                        // We should at least alert the user of why this is failing so they can take steps to fix it.
                        VsUtils.ShowMessageBox(
                            Services.ServiceProvider,
                            String.Format(
                                CultureInfo.CurrentCulture, Resources.ErrorReadingWritingUserSetting, RegKeyNameDdlOverwriteWarning,
                                e.Message),
                            OLEMSGBUTTON.OLEMSGBUTTON_OK,
                            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                            OLEMSGICON.OLEMSGICON_WARNING);
                    }
                }

                // At this point we will save off the DDL Filename into the wizard settings
                Wizard.ModelBuilderSettings.DdlFileName = ddlFilePath;
            }

            if ((Wizard.ModelBuilderSettings.SsdlStringReader != null ||
                 Wizard.ModelBuilderSettings.MslStringReader != null)
                && (Wizard.ModelBuilderSettings.Artifact != null)
                && (Wizard.ModelBuilderSettings.Artifact.StorageModel() != null)
                && (!Wizard.ModelBuilderSettings.Artifact.StorageModel().IsEmpty))
            {
                // Display the SSDL/MSL Overwrite Warning Dialog
                var displayEdmxOverwriteWarning = true;
                try
                {
                    var edmxOverwriteWarningString = EdmUtils.GetUserSetting(RegKeyNameEdmxOverwriteWarning);
                    if (false == String.IsNullOrEmpty(edmxOverwriteWarningString)
                        && false == Boolean.TryParse(edmxOverwriteWarningString, out displayEdmxOverwriteWarning))
                    {
                        displayEdmxOverwriteWarning = true;
                    }
                    if (displayEdmxOverwriteWarning)
                    {
                        var cancelledDuringOverwriteSsdl = DismissableWarningDialog.ShowWarningDialogAndSaveDismissOption(
                            Resources.DatabaseCreation_EdmxOverwriteWarningTitle,
                            Resources.DatabaseCreation_WarningOverwriteMappings,
                            RegKeyNameEdmxOverwriteWarning,
                            DismissableWarningDialog.ButtonMode.YesNo);
                        if (cancelledDuringOverwriteSsdl)
                        {
                            return false;
                        }
                    }
                }
                catch (SecurityException e)
                {
                    // We should at least alert the user of why this is failing so they can take steps to fix it.
                    VsUtils.ShowMessageBox(
                        Services.ServiceProvider,
                        String.Format(
                            CultureInfo.CurrentCulture, Resources.ErrorReadingWritingUserSetting, RegKeyNameEdmxOverwriteWarning,
                            e.Message),
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                        OLEMSGICON.OLEMSGICON_WARNING);
                }
            }

            using (new VsUtils.HourglassHelper())
            {
                // Now we output the DDL, update app/web.config, update the edmx, and open the SQL file that gets produced
                return DatabaseGenerationEngine.UpdateEdmxAndEnvironment(Wizard.ModelBuilderSettings);
            }
        }

        internal override void OnWizardCancel()
        {
            base.OnWizardCancel();

            Wizard.EnableButton(ButtonType.Cancel, false);
            CleanupWorkflow();
        }

        private void HideStatus()
        {
            if (_statusLabel != null)
            {
                SummaryTabs.SelectedTab.Controls.Remove(_statusLabel);
                _statusLabel = null;
            }
        }

        private void ShowStatus(string message)
        {
            if (_statusLabel == null)
            {
                _statusLabel = new Label();
                _statusLabel.BackColor = txtDDL.BackColor;
                _statusLabel.Size = txtDDL.ClientSize;
                _statusLabel.Location = new Point(
                    txtDDL.Left + SystemInformation.Border3DSize.Width,
                    txtDDL.Top + SystemInformation.Border3DSize.Height);
                _statusLabel.TextAlign = ContentAlignment.MiddleCenter;
                _statusLabel.Anchor = txtDDL.Anchor;
            }

            _statusLabel.Text = message;

            var tabPage = SummaryTabs.SelectedTab;
            tabPage.Controls.Add(_statusLabel);
            tabPage.Controls.SetChildIndex(_statusLabel, 0);
        }

        private void CleanupWorkflow()
        {
            if (_workflowInstance != null)
            {
                _onWorkflowCleanup = true;
                _workflowInstance.Abort();
                _workflowInstance = null;
            }
        }

        private void HandleError(string message, bool displayErrorDialog)
        {
            // The workflow encountered a termination exception
            if (displayErrorDialog)
            {
                VsUtils.ShowErrorDialog(message);
            }
            ShowStatus(message);

            // ...Otherwise, just stay on the summary page and
            // disable all buttons except for Previous and Cancel
            Wizard.EnableButton(ButtonType.Previous, true);
            Wizard.EnableButton(ButtonType.Next, false);
            Wizard.EnableButton(ButtonType.Finish, false);
            Wizard.EnableButton(ButtonType.Cancel, true);
        }

        /// <summary>
        ///     This method gets called when the workflow completes or gets terminated
        /// </summary>
        private void _workflowInstance_WorkflowCompleted(WorkflowApplicationCompletedEventArgs e)
        {
            _synchronizationContext.Post(
                state =>
                    {
                        // If we are just cleaning up the workflow, no need to do anything
                        // except enable the appropriate buttons (we might be heading to the
                        // previous page)
                        if (_onWorkflowCleanup)
                        {
                            Wizard.EnableButton(ButtonType.Previous, false);
                            Wizard.EnableButton(ButtonType.Next, true);
                            Wizard.EnableButton(ButtonType.Finish, false);
                            Wizard.EnableButton(ButtonType.Cancel, true);
                            _onWorkflowCleanup = false;

                            return;
                        }

                        // If there was an exception, then the UnhandledException handler should have
                        // already handled it.
                        if (e.TerminationException == null)
                        {
                            // The workflow has completed successfully

                            // Hide the status message on the DDL tab
                            HideStatus();

                            SummaryTabs.Enabled = true;
                            txtSaveDdlAs.Enabled = true;

                            // Immediately enable the Finish button if we've gotten a hold of the WorkflowInstance
                            // We also enable going back to the connection page and cancelling out of the wizard completely
                            Wizard.EnableButton(ButtonType.Previous, true);
                            Wizard.EnableButton(ButtonType.Next, false);
                            Wizard.EnableButton(ButtonType.Finish, true);
                            Wizard.EnableButton(ButtonType.Cancel, true);

                            // Examine the SSDL output. Display an error if we can't find it.
                            object ssdlOutputObj;
                            var ssdlOutput = String.Empty;
                            if (e.Outputs.TryGetValue(EdmConstants.ssdlOutputName, out ssdlOutputObj)
                                && ssdlOutputObj != null
                                && !String.IsNullOrEmpty(ssdlOutput = ssdlOutputObj as string))
                            {
                                Wizard.ModelBuilderSettings.SsdlStringReader = new StringReader(ssdlOutput);
                            }

                            // Examine the MSL output. Display an error if we can't find it.
                            object mslOutputObj;
                            var mslOutput = String.Empty;
                            if (e.Outputs.TryGetValue(EdmConstants.mslOutputName, out mslOutputObj)
                                && mslOutputObj != null
                                && !String.IsNullOrEmpty(mslOutput = mslOutputObj as string))
                            {
                                Wizard.ModelBuilderSettings.MslStringReader = new StringReader(mslOutput);
                            }

                            // Examine the DDL output. Display an error if we can't find it.
                            object ddlOutputObj;
                            var ddlOutput = String.Empty;
                            if (e.Outputs.TryGetValue(EdmConstants.ddlOutputName, out ddlOutputObj)
                                && ddlOutputObj != null
                                && !String.IsNullOrEmpty(ddlOutput = ddlOutputObj as string))
                            {
                                Wizard.ModelBuilderSettings.DdlStringReader = new StringReader(ddlOutput);
                            }

                            // Display the DDL in the textbox
                            if (!String.IsNullOrEmpty(ddlOutput))
                            {
                                InferTablesAndDisplayDDL(ddlOutput);
                            }
                        }
                    }, null);
        }

        private void InferTablesAndDisplayDDL(string ddl)
        {
            // first we'll get the right TSData Extension from the provider

            // then we'll load up a ModelBuilder

            // finally, AddObjects(ddl)

            // from the ModelBuilder, we can examine the tables, etc.

            // we'll also serialize the ddl
            txtDDL.Text = ddl;
        }

        private void txtSaveDdlAs_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSaveDdlAs.Text.Trim()))
            {
                Wizard.EnableButton(ButtonType.Finish, false);
            }
            else
            {
                Wizard.EnableButton(ButtonType.Finish, true);
            }
        }
    }
}
