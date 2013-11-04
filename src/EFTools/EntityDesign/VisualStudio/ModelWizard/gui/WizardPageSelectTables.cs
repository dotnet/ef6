// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.WizardFramework;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Windows.Forms;
    using System.Xml;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    /// <summary>
    ///     This is the third page in the ModelGen VS wizard and is invoked if the user wants to generate the model from a database.
    ///     This page lets the user select tables that should be included in the generated model
    /// </summary>
    // to view this class in the forms designer, make it temporarily derive from Microsoft.WizardFramework.WizardPage
    internal partial class WizardPageSelectTables : WizardPageBase
    {
        // This key is used to store errors that are encountered by WizardPageSelectTables so that other pages can to refer to them.
        internal const string SELECT_TABLES_ERRORS_KEY = "WizardPageSelectTablesErrors";

        private bool _modelNamespaceInitialized;
        private string _initializedDataConnection;
        private bool _initializedUsingLegacyProvider;
        private DatabaseConnectionSsdlAggregator _ssdlAggregator;
        private readonly BackgroundWorker _bgWorkerPopulateTree;
        private readonly Stopwatch _stopwatch;
        private Control _controlWithToolTipShown;
        internal static readonly string selectTablesPageId = "WizardPageSelectTablesId";

        #region Constructors

        internal WizardPageSelectTables(ModelBuilderWizardForm wizard, IServiceProvider serviceProvider)
            : base(wizard, serviceProvider)
        {
            InitializeComponent();

            Logo = ModelWizard.Properties.Resources.PageIcon;
            Headline = ModelWizard.Properties.Resources.SelectTablesPage_Title;
            Id = selectTablesPageId;
            ShowInfoPanel = false;

            labelPrompt.Text = ModelWizard.Properties.Resources.WhichDatabaseObjectsLabel;
            labelPrompt.Font = LabelFont;

            HelpKeyword = null;

            Debug.Assert(
                Wizard.Mode == ModelBuilderWizardForm.WizardMode.PerformAllFunctionality, "Unexpected wizard mode in WizardPageSelectTables");
            InitializeModelOptions(
                chkPluralize, chkIncludeForeignKeys, chkCreateFunctionImports,
                toolTip, Wizard.Project, Wizard.ServiceProvider,
                IncludeForeignKeysArea_OnMouseMove, IncludeForeignKeysArea_OnMouseLeave);

            _stopwatch = new Stopwatch();

            _bgWorkerPopulateTree = new BackgroundWorker();
            _bgWorkerPopulateTree.WorkerSupportsCancellation = true;
            _bgWorkerPopulateTree.RunWorkerCompleted += bgWorkerPopulateTree_RunWorkerCompleted;
            _bgWorkerPopulateTree.DoWork += bgWorkerPopulateTree_DoWork;
        }

        internal static void InitializeModelOptions(
            CheckBox pluralizationCheckBox, CheckBox foreignKeysCheckBox, CheckBox createFunctionImportsCheckBox,
            ToolTip wizardToolTip, Project appProject, IServiceProvider serviceProvider,
            MouseEventHandler mouseMoveHandler, EventHandler mouseLeaveHandler)
        {
            // we only support english pluralization for this release, so default this checked in this case
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "en")
            {
                // default to checked if culture is english
                pluralizationCheckBox.Enabled = true;
                pluralizationCheckBox.Checked = true;
                wizardToolTip.SetToolTip(pluralizationCheckBox, ModelWizard.Properties.Resources.PluralizeCheckBoxToolTipText);
            }
            else
            {
                // even if non-english, we still want the option available, just not checked by default
                pluralizationCheckBox.Enabled = true;
                pluralizationCheckBox.Checked = false;
                wizardToolTip.SetToolTip(pluralizationCheckBox, ModelWizard.Properties.Resources.PluralizeCheckBoxDisabledToolTipText);
            }

            // foreign keys are supported by any version of EF that runs on .NET Framework 4 or newer
            if (NetFrameworkVersioningHelper.TargetNetFrameworkVersion(appProject, serviceProvider) >=
                NetFrameworkVersioningHelper.NetFrameworkVersion4)
            {
                foreignKeysCheckBox.Enabled = true;
                foreignKeysCheckBox.Checked = true;
                wizardToolTip.SetToolTip(
                    foreignKeysCheckBox,
                    ModelWizard.Properties.Resources.SelectTablesPage_IncludeForeignKeysToolTip);
            }
            else
            {
                wizardToolTip.SetToolTip(foreignKeysCheckBox, Resources.DisabledFeatureTooltip);
                foreignKeysCheckBox.Parent.MouseMove += mouseMoveHandler;
                foreignKeysCheckBox.Parent.MouseLeave += mouseLeaveHandler;
                foreignKeysCheckBox.Enabled = false;
                foreignKeysCheckBox.Checked = false;
            }

            if (createFunctionImportsCheckBox != null)
            {
                // assume we have no Stored Procs and so default the Create Function Imports checkbox to unchecked and not enabled
                createFunctionImportsCheckBox.Enabled = false;
                createFunctionImportsCheckBox.Checked = false;
            }
        }

        #endregion Constructors

        #region WizardPage overrides

        public override bool IsDataValid
        {
            get { return true; }
        }

        public override bool OnActivate()
        {
            if (!Visited)
            {
                _modelNamespaceInitialized = false;
            }

            return base.OnActivate();
        }

        /// <summary>
        ///     Invoked by the VS Wizard framework when this page is entered.
        ///     Starts a background thread to retrieve table information to display
        /// </summary>
        public override void OnActivated()
        {
            base.OnActivated();

            _ssdlAggregator = new DatabaseConnectionSsdlAggregator(Wizard.ModelBuilderSettings);

            using (new VsUtils.HourglassHelper())
            {
                if (!_modelNamespaceInitialized)
                {
                    var existingNamespaces = InitializeExistingNamespaces(Wizard.Project);

                    // set the storage target to the initial catalog but if we're targeting a database project
                    // then set it to the database name which is by definition unique.
                    var storageTarget = Wizard.ModelBuilderSettings.InitialCatalog;

                    // set the default name for the Model Namespace
                    string trialNamespace;
                    if (String.IsNullOrEmpty(storageTarget))
                    {
                        trialNamespace = ModelConstants.DefaultModelNamespace;
                    }
                    else
                    {
                        trialNamespace = EdmUtils.ConstructValidModelNamespace(
                            storageTarget + ModelConstants.DefaultModelNamespace,
                            ModelConstants.DefaultModelNamespace);
                    }

                    modelNamespaceTextBox.Text = EdmUtils.ConstructUniqueNamespace(trialNamespace, existingNamespaces);
                }
            } // restore cursor

            Debug.Assert(
                Wizard.ModelBuilderSettings.DesignTimeConnectionString != null,
                "Unexpected null value for connection string");

            if (Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.GenerateFromDatabase
                && (!Wizard.ModelBuilderSettings.DesignTimeConnectionString.Equals(_initializedDataConnection) ||
                    Wizard.ModelBuilderSettings.UseLegacyProvider != _initializedUsingLegacyProvider))
            {
                // Enable the treeView / labels and hide the database project label and textbox.
                databaseObjectTreeView.Visible = true;
                labelPrompt.Visible = true;

                // Disable wizard navigation temporarily until the background thread completes
                Wizard.EnableButton(ButtonType.Previous, false);
                Wizard.EnableButton(ButtonType.Next, false);
                Wizard.EnableButton(ButtonType.Finish, false);

                // disable model namespace textbox
                modelNamespaceTextBox.Enabled = false;

                // Clear existing database objects
                Wizard.ModelBuilderSettings.DatabaseObjectFilters = null;
                databaseObjectTreeView.TreeViewControl.Nodes.Clear();

                // Put up a status message
                databaseObjectTreeView.ShowStatus(ModelWizard.Properties.Resources.SelectTablesPage_StatusRetrievingTablesText);

                // Get table names in a background thread
                _stopwatch.Reset();
                _stopwatch.Start();
                _bgWorkerPopulateTree.RunWorkerAsync(this);
            }
        }

        /// <summary>
        ///     Invoked by the VS Wizard framework when this page is exited or when the "Finish" button is clicked.
        ///     Updates ModelBuilderSettings from the GUI
        /// </summary>
        public override bool OnDeactivate()
        {
            if (Wizard.MovingNext
                && !Wizard.MovingPrevious
                && !Wizard.WizardFinishing)
            {
                if (!OnWizardFinish())
                {
                    return false;
                }
            }

            UpdateSettingsFromGui();

            return base.OnDeactivate();
        }

        private void UpdateSettingsFromGui()
        {
            Wizard.ModelBuilderSettings.DatabaseObjectFilters = GetSelectedFilterEntriesFromTreeView();
            Wizard.ModelBuilderSettings.ModelNamespace = modelNamespaceTextBox.Text;
            Wizard.ModelBuilderSettings.LoadingDBMetatdataTime = _stopwatch.Elapsed;
            Wizard.ModelBuilderSettings.UsePluralizationService = chkPluralize.Checked;
            Wizard.ModelBuilderSettings.IncludeForeignKeysInModel = chkIncludeForeignKeys.Checked;
        }

        internal override void OnWizardCancel()
        {
            base.OnWizardCancel();

            if (_bgWorkerPopulateTree != null
                && _bgWorkerPopulateTree.IsBusy
                && _bgWorkerPopulateTree.CancellationPending == false)
            {
                _bgWorkerPopulateTree.CancelAsync();
                Wizard.EnableButton(ButtonType.Cancel, false);

                using (new VsUtils.HourglassHelper())
                {
                    var i = 0;
                    while (_bgWorkerPopulateTree.IsBusy)
                    {
                        Application.DoEvents();
                        if (i++ >= 16)
                        {
                            break; // guaranteed break out after 8 sec
                        }
                    }
                }
            }
        }

        internal override bool OnWizardFinish()
        {
            using (new VsUtils.HourglassHelper())
            {
                UpdateSettingsFromGui();
                UpdateModelBuilderFilterSettings(); // only needs to be done once when wizard finishes
            }

            if (!ValidateNamespace(modelNamespaceTextBox, Wizard.ModelBuilderSettings))
            {
                return false;
            }

            var hasSelectedDatabaseObject = false;
            var hasDatabaseObjects = false;
            foreach (TreeNode r in databaseObjectTreeView.TreeViewControl.Nodes)
            {
                if (r.Nodes.Count > 0)
                {
                    hasDatabaseObjects = true;
                }
                if (r.Checked)
                {
                    hasSelectedDatabaseObject = true;
                }
            }

            // Don't prompt the user about selected database objects if we're creating a new database project.
            if (!hasSelectedDatabaseObject && hasDatabaseObjects)
            {
                if (VsUtils.ShowMessageBox(
                    PackageManager.Package,
                    ModelWizard.Properties.Resources.SelectTablesPage_ConfirmNoTables,
                    OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                    OLEMSGICON.OLEMSGICON_QUERY) != DialogResult.Yes)
                {
                    return false;
                }
            }


            if (Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.GenerateFromDatabase)
            {
                GenerateModel(Wizard.ModelBuilderSettings);
            }
            
            return true;
        }

        internal static bool ValidateNamespace(TextBox modelNamespaceTextBox, ModelBuilderSettings modelBuilderSettings)
        {
            if (!EdmUtils.IsValidModelNamespace(modelNamespaceTextBox.Text))
            {
                var s = ModelWizard.Properties.Resources.ConnectionStringNonValidIdentifier;
                VsUtils.ShowErrorDialog(String.Format(CultureInfo.CurrentCulture, s, modelNamespaceTextBox.Text));
                modelNamespaceTextBox.Focus();
                return false;
            }

            // the Model Namespace and the Entity Container name must differ
            if (ModelBuilderWizardForm.ModelNamespaceAndEntityContainerNameSame(modelBuilderSettings))
            {
                var s = ModelWizard.Properties.Resources.NamespaceAndEntityContainerSame;
                VsUtils.ShowErrorDialog(String.Format(CultureInfo.CurrentCulture, s, modelBuilderSettings.AppConfigConnectionPropertyName));
                modelNamespaceTextBox.Focus();
                return false;
            }
            return true;
        }

        #endregion WizardPage overrides

        #region Methods

        /// <summary>
        ///     DoWork event handler: Get table names in a background thread.
        ///     This method is called by background worker component on a different thread than the UI thread.
        ///     We use the ModelBuilderEngine to get table names to display
        /// </summary>
        private void bgWorkerPopulateTree_DoWork(object sender, DoWorkEventArgs args)
        {
            // This method will run on a thread other than the UI thread.
            // Be sure not to manipulate any Windows Forms controls created on the UI thread from this method.
            using (new VsUtils.HourglassHelper())
            {
                var result = new ICollection<EntityStoreSchemaFilterEntry>[3];
                try
                {
                    result[0] = _ssdlAggregator.GetTableFilterEntries(args);
                    if (args.Cancel)
                    {
                        return;
                    }

                    result[1] = _ssdlAggregator.GetViewFilterEntries(args);
                    if (args.Cancel)
                    {
                        return;
                    }

                    result[2] = _ssdlAggregator.GetFunctionFilterEntries(args);
                    if (args.Cancel)
                    {
                        return;
                    }
                }
                finally
                {
                    VsUtils.SafeCloseDbConnectionOnFile(
                        Wizard.ModelBuilderSettings.DesignTimeProviderInvariantName, Wizard.ModelBuilderSettings.DesignTimeConnectionString);
                }

                args.Result = result;
            }
        }

        /// <summary>
        ///     RunWorkerCompleted event handler: Populate Treeview here.
        ///     This method is called by background worker component on the same thread as the UI thread.
        ///     ModelBuilderEngine gaves us table names to display so we add them to the TreeView
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void bgWorkerPopulateTree_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            try
            {
                _stopwatch.Stop();

                if (args.Cancelled)
                {
                    return;
                }

                if (args.Error != null)
                {
                    // show error dialog
                    ModelBuilderWizardForm.ShowDatabaseConnectionErrorDialog(args.Error);

                    Visited = false;
                    Wizard.OnPrevious();
                }
                else
                {
                    using (new VsUtils.HourglassHelper())
                    {
                        // No errors, populate nodes in TreeView - create the nodes accordingly
                        var result = (ICollection<EntityStoreSchemaFilterEntry>[])args.Result;
                        var tableEntries = result[0];
                        var viewEntries = result[1];
                        var sprocEntries = result[2];

                        CreateTreeForNewModel(tableEntries, viewEntries, sprocEntries);

                        // if there are Sproc entries then enable the Create Function Imports box and by default set to checked
                        if (sprocEntries.Count > 0)
                        {
                            chkCreateFunctionImports.Enabled = true;
                            chkCreateFunctionImports.Checked = true;
                        }

                        // Hide status message
                        databaseObjectTreeView.HideStatus();

                        // Set focus to TreeView
                        databaseObjectTreeView.TreeViewControl.SelectedNode = databaseObjectTreeView.TreeViewControl.Nodes[0];
                        databaseObjectTreeView.Focus();

                        _initializedDataConnection = Wizard.ModelBuilderSettings.DesignTimeConnectionString;
                        _initializedUsingLegacyProvider = Wizard.ModelBuilderSettings.UseLegacyProvider;
                        modelNamespaceTextBox.Enabled = true;
                    }
                }

                // Enable wizard navigation
                Wizard.OnValidationStateChanged(this);
            }
            catch (Exception e)
            {
                databaseObjectTreeView.ShowStatus(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        ModelWizard.Properties.Resources.SelectTablesPage_ErrorRetrievingTablesText, e.Message));
                Wizard.EnableButton(ButtonType.Cancel, true);
            }
        }

        private void CreateTreeForNewModel(
            ICollection<EntityStoreSchemaFilterEntry> tableEntries,
            ICollection<EntityStoreSchemaFilterEntry> viewEntries, ICollection<EntityStoreSchemaFilterEntry> sprocEntries)
        {
            databaseObjectTreeView.TreeViewControl.Nodes.Add(
                DatabaseObjectTreeView.CreateRootNodeAndDescendents(
                    tableEntries, ModelWizard.Properties.Resources.SelectTablesPage_TablesNode,
                    DatabaseObjectTreeView.TreeViewImage.DbTablesImage, DatabaseObjectTreeView.TreeViewImage.TableImage));
            databaseObjectTreeView.TreeViewControl.Nodes.Add(
                DatabaseObjectTreeView.CreateRootNodeAndDescendents(
                    viewEntries, ModelWizard.Properties.Resources.SelectTablesPage_ViewsNode,
                    DatabaseObjectTreeView.TreeViewImage.DbViewsImage, DatabaseObjectTreeView.TreeViewImage.ViewImage));
            databaseObjectTreeView.TreeViewControl.Nodes.Add(
                DatabaseObjectTreeView.CreateRootNodeAndDescendents(
                    sprocEntries, ModelWizard.Properties.Resources.SelectTablesPage_StoredProceduresNode,
                    DatabaseObjectTreeView.TreeViewImage.DbStoreProcsImage, DatabaseObjectTreeView.TreeViewImage.StoreProcImage));
        }

        private ICollection<EntityStoreSchemaFilterEntry> GetSelectedFilterEntriesFromTreeView()
        {
            var mapper = new TreeViewSchemaFilterMapper(databaseObjectTreeView.TreeViewControl);
            var filterEntryBag = mapper.CreateSchemaFilterEntryBag();

            return filterEntryBag.CollapseAndOptimize(SchemaFilterPolicy.GetByValEdmxPolicy());
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "result")]
        private void UpdateModelBuilderFilterSettings()
        {
            var mapper = new TreeViewSchemaFilterMapper(databaseObjectTreeView.TreeViewControl);
            var filterEntryBag = mapper.CreateSchemaFilterEntryBag();

            IList<EntityStoreSchemaFilterEntry> newFunctionEntries = new List<EntityStoreSchemaFilterEntry>();
            foreach (var entry in filterEntryBag.IncludedSprocEntries)
            {
                newFunctionEntries.Add(entry);
            }

            // if there are any new Function entries and if the user has selected to create matching Function Imports
            // then create and run a ProgressDialog while we are collecting the sproc return type info
            if (newFunctionEntries.Count > 0
                && chkCreateFunctionImports.Checked)
            {
                var result = ModelBuilderEngine.ShowProgressDialog(this, newFunctionEntries, Wizard.ModelBuilderSettings);
            }
        }

        internal static HashSet<string> InitializeExistingNamespaces(Project project)
        {
            var existingNamespaces = new HashSet<string>();
            if (project != null)
            {
                // find the namespace used in the CSDL section of each existing edmx file in the project
                var vsHierarchy = VsUtils.GetVsHierarchy(project, Services.ServiceProvider);
                var fileFinder = new VSFileFinder(EntityDesignArtifact.EXTENSION_EDMX);
                fileFinder.FindInProject(vsHierarchy);

                foreach (var fileInfo in fileFinder.MatchingFiles)
                {
                    try
                    {
                        var xmlDocument = EdmUtils.SafeLoadXmlFromPath(fileInfo.Path);
                        foreach (var schemaVersion in EntityFrameworkVersion.GetAllVersions())
                        {
                            var nsMgr = SchemaManager.GetEdmxNamespaceManager(xmlDocument.NameTable, schemaVersion);

                            foreach (
                                XmlElement e in xmlDocument.SelectNodes("/edmx:Edmx/edmx:Runtime/edmx:ConceptualModels/csdl:Schema", nsMgr))
                            {
                                var namespaceValue = e.GetAttribute("Namespace");
                                if (!string.IsNullOrEmpty(namespaceValue))
                                {
                                    existingNamespaces.Add(namespaceValue);
                                }
                            }
                        }
                    }
                        // swallow various exceptions that come from reading the file or parsing xml
                        // We just skip this document in the event of an exception.
                    catch (IOException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                    catch (NotSupportedException)
                    {
                    }
                    catch (SecurityException)
                    {
                    }
                    catch (XmlException)
                    {
                    }
                }
            }
            return existingNamespaces;
        }

        /// <summary>
        ///     Event handler to display a tooltip over a disabled control
        /// </summary>
        internal void IncludeForeignKeysArea_OnMouseMove(Object sender, MouseEventArgs e)
        {
            ToolTipOnDisabledControl_OnMouseMove(sender, e, chkIncludeForeignKeys, toolTip, ref _controlWithToolTipShown);
        }

        /// <summary>
        ///     Event handler to display a tooltip over a disabled control
        /// </summary>
        private void IncludeForeignKeysArea_OnMouseLeave(object sender, EventArgs e)
        {
            ToolTipOnDisabledControl_OnMouseLeave(sender, e, toolTip, ref _controlWithToolTipShown);
        }

        /// <summary>
        ///     Helper method to display a tooltip over a disabled control
        /// </summary>
        internal static void ToolTipOnDisabledControl_OnMouseMove(
            Object sender, MouseEventArgs e, Control disabledControl, ToolTip toolTip, ref Control controlWithToolTipShown)
        {
            var parent = sender as Control;
            if (parent == null)
            {
                return;
            }
            var ctrl = parent.GetChildAtPoint(e.Location);
            if (ctrl == disabledControl)
            {
                // if the user hover on control where tooltip is shown, just return.
                if (ctrl == controlWithToolTipShown)
                {
                    return;
                }
                var tipString = toolTip.GetToolTip(ctrl);
                // calculate the screen coordinate of the mouse
                toolTip.Show(tipString, ctrl, 2, ctrl.Height + 2);
                controlWithToolTipShown = ctrl;
            }
            else if (controlWithToolTipShown != null)
            {
                toolTip.Hide(controlWithToolTipShown);
                controlWithToolTipShown = null;
            }
        }

        /// <summary>
        ///     Helper method to display a tooltip over a disabled control
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "sender")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e")]
        internal static void ToolTipOnDisabledControl_OnMouseLeave(
            object sender, EventArgs e, ToolTip toolTip, ref Control controlWithToolTipShown)
        {
            if (controlWithToolTipShown != null)
            {
                toolTip.Hide(controlWithToolTipShown);
            }
            controlWithToolTipShown = null;
        }

        #endregion Methods
    }
}
