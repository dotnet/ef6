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

    // <summary>
    //     This is the third page in the ModelGen VS wizard and is invoked if the user wants to generate the model from a database.
    //     This page lets the user select tables that should be included in the generated model
    // </summary>
    // to view this class in the forms designer, make it temporarily derive from Microsoft.WizardFramework.WizardPage
    internal partial class WizardPageSelectTables : WizardPageBase
    {
        private readonly bool _isNetFx35;
        private string _initializedDataConnection;
        private bool _initializedUsingLegacyProvider;
        // -1 ensures we set the state of the controls when OnActivate is called the first time
        private ModelGenerationOption _initializedGenerationOption = (ModelGenerationOption)(-1);
        private readonly BackgroundWorker _bgWorkerPopulateTree;
        private readonly Stopwatch _stopwatch;
        private Control _controlWithToolTipShown;
        
        internal WizardPageSelectTables(ModelBuilderWizardForm wizard)
            : base(wizard)
        {
            InitializeComponent();

            Logo = ModelWizard.Properties.Resources.PageIcon;
            Headline = ModelWizard.Properties.Resources.SelectTablesPage_Title;
            Id = "WizardPageSelectTablesId";
            ShowInfoPanel = false;

            labelPrompt.Text = ModelWizard.Properties.Resources.WhichDatabaseObjectsLabel;
            labelPrompt.Font = LabelFont;

            HelpKeyword = null;

            Debug.Assert(
                Wizard.Mode == ModelBuilderWizardForm.WizardMode.PerformAllFunctionality, 
                "Unexpected wizard mode in WizardPageSelectTables");

            _stopwatch = new Stopwatch();

            _isNetFx35 = NetFrameworkVersioningHelper.TargetNetFrameworkVersion(Wizard.Project, ServiceProvider) <
                NetFrameworkVersioningHelper.NetFrameworkVersion4;

            InitializeModelOptions();

            _bgWorkerPopulateTree = new BackgroundWorker();
            _bgWorkerPopulateTree.WorkerSupportsCancellation = true;
            _bgWorkerPopulateTree.RunWorkerCompleted += bgWorkerPopulateTree_RunWorkerCompleted;
            _bgWorkerPopulateTree.DoWork += bgWorkerPopulateTree_DoWork;
        }

        private void InitializeModelOptions()
        {
            // we only support english pluralization for this release, so default this checked in this case
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "en")
            {
                // default to checked if culture is english
                chkPluralize.Enabled = true;
                chkPluralize.Checked = true;
                toolTip.SetToolTip(chkPluralize, ModelWizard.Properties.Resources.PluralizeCheckBoxToolTipText);
            }
            else
            {
                // even if non-english, we still want the option available, just not checked by default
                chkPluralize.Enabled = true;
                chkPluralize.Checked = false;
                toolTip.SetToolTip(chkPluralize, ModelWizard.Properties.Resources.PluralizeCheckBoxDisabledToolTipText);
            }

            if (_isNetFx35)
            {
                toolTip.SetToolTip(chkIncludeForeignKeys, Resources.DisabledFeatureTooltip);
                chkIncludeForeignKeys.Parent.MouseMove += IncludeForeignKeysArea_OnMouseMove;
                chkIncludeForeignKeys.Parent.MouseLeave += IncludeForeignKeysArea_OnMouseLeave;
            }
            else
            {
                toolTip.SetToolTip(
                    chkIncludeForeignKeys,
                    ModelWizard.Properties.Resources.SelectTablesPage_IncludeForeignKeysToolTip);
            }

            // assume we have no Stored Procs and so default the Create Function Imports checkbox to unchecked and not enabled
            chkCreateFunctionImports.Enabled = false;
            chkCreateFunctionImports.Checked = false;
        }

        public override bool IsDataValid
        {
            get { return true; }
        }

        // <summary>
        //     Invoked by the VS Wizard framework when this page is entered.
        //     Starts a background thread to retrieve table information to display
        // </summary>
        public override void OnActivated()
        {
            base.OnActivated();

            SetControlState(Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.CodeFirstFromDatabase);
           
            Debug.Assert(
                Wizard.ModelBuilderSettings.DesignTimeConnectionString != null,
                "Unexpected null value for connection string");

            if ((Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.GenerateFromDatabase ||
                Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.CodeFirstFromDatabase)
                && (!Wizard.ModelBuilderSettings.DesignTimeConnectionString.Equals(_initializedDataConnection) ||
                    Wizard.ModelBuilderSettings.UseLegacyProvider != _initializedUsingLegacyProvider ||
                    Wizard.ModelBuilderSettings.GenerationOption != _initializedGenerationOption))
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

        private void SetControlState(bool isCodeFirstFlow)
        {
            modelNamespaceTextBox.Text = GetUniqueModelNamespace();
            modelNamespaceTextBox.Visible = modelNamespaceLabel.Visible = !isCodeFirstFlow;

            if (isCodeFirstFlow)
            {
                // for models created with CodeFirst from database we always want foreign keys
                // (note that the code generation templates CodeFirst from database don't support IAs)
                chkIncludeForeignKeys.Checked = true;
                chkIncludeForeignKeys.Enabled = false;

                // We don't support functions/function imports in Code First
                chkCreateFunctionImports.Checked = false;
                chkCreateFunctionImports.Enabled = false;
            }
            else if(_initializedGenerationOption != Wizard.ModelBuilderSettings.GenerationOption)
            {

                if (!_isNetFx35)
                {
                    chkIncludeForeignKeys.Enabled = true;
                    chkIncludeForeignKeys.Checked = true;
                }
                else
                {
                    // EF1 did not support foreign keys
                    chkIncludeForeignKeys.Enabled = false;
                    chkIncludeForeignKeys.Checked = false;
                }
            }
        }

        private string GetUniqueModelNamespace()
        {
            using (new VsUtils.HourglassHelper())
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

                return EdmUtils.ConstructUniqueNamespace(trialNamespace, existingNamespaces);

            } // restore cursor
        }

        // <summary>
        //     Invoked by the VS Wizard framework when this page is exited or when the "Finish" button is clicked.
        //     Updates ModelBuilderSettings from the GUI
        // </summary>
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
            Wizard.ModelBuilderSettings.LoadingDBMetadataTime = _stopwatch.Elapsed;
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


            if (Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.GenerateFromDatabase ||
                Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.CodeFirstFromDatabase)
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

        // <summary>
        //     DoWork event handler: Get table names in a background thread.
        //     This method is called by background worker component on a different thread than the UI thread.
        //     We use the ModelBuilderEngine to get table names to display
        // </summary>
        private void bgWorkerPopulateTree_DoWork(object sender, DoWorkEventArgs args)
        {
            // This method will run on a thread other than the UI thread.
            // Be sure not to manipulate any Windows Forms controls created on the UI thread from this method.
            using (new VsUtils.HourglassHelper())
            {
                var ssdlAggregator = new DatabaseConnectionSsdlAggregator(Wizard.ModelBuilderSettings);

                var result = new ICollection<EntityStoreSchemaFilterEntry>[3];
                try
                {
                    result[0] = ssdlAggregator.GetTableFilterEntries(args);
                    if (args.Cancel)
                    {
                        return;
                    }

                    result[1] = ssdlAggregator.GetViewFilterEntries(args);
                    if (args.Cancel)
                    {
                        return;
                    }

                    // we don't support function imports in CodeFirst so don't have to retrieve them
                    if (Wizard.ModelBuilderSettings.GenerationOption != ModelGenerationOption.CodeFirstFromDatabase)
                    {
                        result[2] = ssdlAggregator.GetFunctionFilterEntries(args);
                    }

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

        // <summary>
        //     RunWorkerCompleted event handler: Populate Treeview here.
        //     This method is called by background worker component on the same thread as the UI thread.
        //     ModelBuilderEngine gaves us table names to display so we add them to the TreeView
        // </summary>
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
                        if (sprocEntries != null && sprocEntries.Count > 0)
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
                        _initializedGenerationOption = Wizard.ModelBuilderSettings.GenerationOption;
                        modelNamespaceTextBox.Enabled = Wizard.ModelBuilderSettings.GenerationOption != ModelGenerationOption.CodeFirstFromDatabase;
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
            ICollection<EntityStoreSchemaFilterEntry> viewEntries, 
            ICollection<EntityStoreSchemaFilterEntry> sprocEntries)
        {
            databaseObjectTreeView.TreeViewControl.Nodes.Add(
                DatabaseObjectTreeView.CreateRootNodeAndDescendents(
                    tableEntries, ModelWizard.Properties.Resources.SelectTablesPage_TablesNode,
                    DatabaseObjectTreeView.TreeViewImage.DbTablesImage, DatabaseObjectTreeView.TreeViewImage.TableImage));
            databaseObjectTreeView.TreeViewControl.Nodes.Add(
                DatabaseObjectTreeView.CreateRootNodeAndDescendents(
                    viewEntries, ModelWizard.Properties.Resources.SelectTablesPage_ViewsNode,
                    DatabaseObjectTreeView.TreeViewImage.DbViewsImage, DatabaseObjectTreeView.TreeViewImage.ViewImage));

            // sprocEntries will be null for CodeFirst from database
            if (sprocEntries != null)
            {
                databaseObjectTreeView.TreeViewControl.Nodes.Add(
                    DatabaseObjectTreeView.CreateRootNodeAndDescendents(
                        sprocEntries, ModelWizard.Properties.Resources.SelectTablesPage_StoredProceduresNode,
                        DatabaseObjectTreeView.TreeViewImage.DbStoreProcsImage, DatabaseObjectTreeView.TreeViewImage.StoreProcImage));
            }
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
                var result = ProgressDialogHelper.ShowProgressDialog(this, newFunctionEntries, Wizard.ModelBuilderSettings);
            }
        }

        internal static HashSet<string> InitializeExistingNamespaces(Project project)
        {
            var existingNamespaces = new HashSet<string>();
            if (project != null)
            {
                // find the namespace used in the CSDL section of each existing edmx file in the project
                var vsHierarchy = VsUtils.GetVsHierarchy(project, Services.ServiceProvider);
                var fileFinder = new VSFileFinder(EntityDesignArtifact.ExtensionEdmx);
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

        // <summary>
        //     Event handler to display a tooltip over a disabled control
        // </summary>
        internal void IncludeForeignKeysArea_OnMouseMove(Object sender, MouseEventArgs e)
        {
            ToolTipOnDisabledControl_OnMouseMove(sender, e, chkIncludeForeignKeys, toolTip, ref _controlWithToolTipShown);
        }

        // <summary>
        //     Event handler to display a tooltip over a disabled control
        // </summary>
        private void IncludeForeignKeysArea_OnMouseLeave(object sender, EventArgs e)
        {
            ToolTipOnDisabledControl_OnMouseLeave(sender, e, toolTip, ref _controlWithToolTipShown);
        }

        // <summary>
        //     Helper method to display a tooltip over a disabled control
        // </summary>
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

        // <summary>
        //     Helper method to display a tooltip over a disabled control
        // </summary>
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
    }
}
