// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Utilities;
    using Microsoft.WizardFramework;
    using Resources = Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties.Resources;

    // <summary>
    //     This is the page in the ModelGen VS wizard used for selecting
    //     tables/views/sprocs to add (and to refresh/delete others as
    //     appropriate) when in the Update Model from Database mode
    // </summary>
    // to view this class in the forms designer, make it temporarily derive from Microsoft.WizardFramework.WizardPage
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal partial class WizardPageUpdateFromDatabase : WizardPageBase
    {
        internal class DatabaseObjectNameFirstComparer : IComparer<DatabaseObject>
        {
            public int Compare(DatabaseObject x, DatabaseObject y)
            {
                var compareNames = CompareStringsWhichMayBeNull(x.Name, y.Name);
                if (compareNames != 0)
                {
                    return compareNames;
                }
                return CompareStringsWhichMayBeNull(x.Schema, y.Schema);
            }

            private static int CompareStringsWhichMayBeNull(string x, string y)
            {
                if (null == x
                    && null == y)
                {
                    // both null
                    return 0;
                }
                if (null == x)
                {
                    // x is null, y is not
                    return -1;
                }
                if (null == y)
                {
                    // y is null, x is not
                    return 1;
                }
                // both x & y are non-null
                return String.Compare(x, y, StringComparison.CurrentCulture);
            }
        }

        private static readonly DatabaseObjectNameFirstComparer _databaseObjectNameFirstComparer = new DatabaseObjectNameFirstComparer();

        private string _initializedDataConnection;
        private readonly BackgroundWorker _bgWorkerPopulateTree;
        private Label _statusLabel;
        private readonly Stopwatch _stopwatch;
        private Control _controlWithToolTipShown;

        internal WizardPageUpdateFromDatabase(ModelBuilderWizardForm wizard)
            : base(wizard)
        {
            InitializeComponent();

            Logo = Resources.PageIcon;
            Headline = Resources.SelectTablesPage_Title;
            Id = "WizardPageUpdateFromDatabaseId";
            ShowInfoPanel = false;

            _bgWorkerPopulateTree = new BackgroundWorker();
            _bgWorkerPopulateTree.WorkerSupportsCancellation = true;
            _bgWorkerPopulateTree.RunWorkerCompleted += bgWorkerPopulateTree_RunWorkerCompleted;
            _bgWorkerPopulateTree.DoWork += bgWorkerPopulateTree_DoWork;

            AddTabPage.Select();

            HelpKeyword = null;

            Debug.Assert(
                Wizard.Mode == ModelBuilderWizardForm.WizardMode.PerformDatabaseConfigAndSelectTables ||
                Wizard.Mode == ModelBuilderWizardForm.WizardMode.PerformSelectTablesOnly,
                "Unexpected wizard mode " + Wizard.Mode);

            // 
            //  Get the optionsDesignerInfo from the artifact
            //
            OptionsDesignerInfo optionsDesignerInfo = null;
            var artifact = Wizard.ModelBuilderSettings.Artifact;
            Debug.Assert(artifact != null, "Expected non-null artifact");
            if (artifact != null)
            {
                DesignerInfo designerInfo;
                if (artifact.DesignerInfo().TryGetDesignerInfo(OptionsDesignerInfo.ElementName, out designerInfo))
                {
                    Debug.Assert(designerInfo != null, "expected non-null designerInfo");
                    optionsDesignerInfo = designerInfo as OptionsDesignerInfo;
                    Debug.Assert(optionsDesignerInfo != null, "expected non-null optionsDesignerInfo");
                }
            }

            //
            // set up pluralization checkbox.  We only support english pluralization for this release, so default this checked in this case
            //
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "en")
            {
                // default to checked.
                toolTip.SetToolTip(chkPluralize, Resources.PluralizeCheckBoxToolTipText);
                chkPluralize.Enabled = true;
                chkPluralize.Checked = true;

                // read value from designer properties
                if (optionsDesignerInfo != null)
                {
                    chkPluralize.Checked = TryReadDesignerProperty(optionsDesignerInfo.CheckPluralizationInWizard, true);
                }
            }
            else
            {
                // even if non-english, we still want the option available, just not checked by default
                chkPluralize.Enabled = true;
                chkPluralize.Checked = false;

                // read value from designer properties
                if (optionsDesignerInfo != null)
                {
                    chkPluralize.Checked = TryReadDesignerProperty(optionsDesignerInfo.CheckPluralizationInWizard, false);
                }
                toolTip.SetToolTip(chkPluralize, Resources.PluralizeCheckBoxDisabledToolTipText);
            }

            // foreign keys are supported by any version of EF that runs on .NET Framework 4 or newer
            if (NetFrameworkVersioningHelper.TargetNetFrameworkVersion(Wizard.Project, Wizard.ServiceProvider) >=
                NetFrameworkVersioningHelper.NetFrameworkVersion4)
            {
                chkIncludeForeignKeys.Checked = true;
                chkIncludeForeignKeys.Enabled = true;
                toolTip.SetToolTip(
                    chkIncludeForeignKeys,
                    Resources.SelectTablesPage_IncludeForeignKeysToolTip);

                // try to read value from designer properties
                if (optionsDesignerInfo != null)
                {
                    chkIncludeForeignKeys.Checked =
                        TryReadDesignerProperty(optionsDesignerInfo.CheckIncludeForeignKeysInModel, true);
                }
            }
            else
            {
                toolTip.SetToolTip(chkIncludeForeignKeys, Design.Resources.DisabledFeatureTooltip);
                chkIncludeForeignKeys.Parent.MouseMove += IncludeForeignKeysArea_OnMouseMove;
                chkIncludeForeignKeys.Parent.MouseLeave += IncludeForeignKeysArea_OnMouseLeave;
                chkIncludeForeignKeys.Enabled = false;
                chkIncludeForeignKeys.Checked = false;
            }

            // assume we have no Stored Procs and so default the Create Function Imports checkbox to unchecked and not enabled
            chkCreateFunctionImports.Enabled = false;
            chkCreateFunctionImports.Checked = false;

            _stopwatch = new Stopwatch();
        }

        internal bool TreeViewsInitialized { get; set; }

        public override bool IsDataValid
        {
            get { return true; }
        }

        public override bool OnActivate()
        {
            if (!Visited)
            {
                TreeViewsInitialized = false;

                // Make tab pages not enabled and Description fields invisible
                AddUpdateDeleteTabControl.Enabled = false;
                DescriptionTextBox.Visible = false;
            }

            return base.OnActivate();
        }

        // <summary>
        //     Invoked by the VS Wizard framework when this page is entered.
        //     Starts a background thread to retrieve table information to display
        // </summary>
        public override void OnActivated()
        {
            base.OnActivated();

            Debug.Assert(Wizard.ModelBuilderSettings.DesignTimeConnectionString != null, "Unexpected null value for connection string");

            if (!TreeViewsInitialized
                || !Wizard.ModelBuilderSettings.DesignTimeConnectionString.Equals(_initializedDataConnection))
            {
                // Disable wizard navigation temporarily until the background thread completes
                Wizard.EnableButton(ButtonType.Previous, false);
                Wizard.EnableButton(ButtonType.Next, false);
                Wizard.EnableButton(ButtonType.Finish, false);

                // Clear existing database objects
                Wizard.ModelBuilderSettings.DatabaseObjectFilters = null;
                AddTreeView.TreeViewControl.Nodes.Clear();
                RefreshTreeView.TreeViewControl.Nodes.Clear();
                RefreshTreeView.TreeViewControl.CheckBoxes = false;
                VsShellUtilities.ApplyTreeViewThemeStyles(RefreshTreeView.TreeViewControl);
                // re-apply theme styles (setting Checkboxes resets them)
                DeleteTreeView.TreeViewControl.Nodes.Clear();
                DeleteTreeView.TreeViewControl.CheckBoxes = false;
                VsShellUtilities.ApplyTreeViewThemeStyles(DeleteTreeView.TreeViewControl);
                // re-apply theme styles (setting Checkboxes resets them)

                // Put up a status message
                ShowStatus(Resources.SelectTablesPage_StatusRetrievingTablesText);

                // Get database objects in a background thread
                _stopwatch.Reset();
                _stopwatch.Start();
                _bgWorkerPopulateTree.RunWorkerAsync(this);
            }
        }

        // <summary>
        //     Invoked by the VS Wizard framework when this page is exited or when the "Finish" button is clicked.
        //     Updates ModelBuilderSettings from the GUI
        // </summary>
        public override bool OnDeactivate()
        {
            if (Wizard.MovingNext
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
            UpdateSettingsFromGui();
            UpdateNewFunctionFilterEntries(); // only needs to be done once when wizard finishes

            if (Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.GenerateFromDatabase)
            {
                GenerateModel(Wizard.ModelBuilderSettings);
            }

            return true;
        }

        // <summary>
        //     DoWork event handler: Get database objects in a background thread.
        //     This method is called by background worker component on a diffferent thread than the UI thread.
        //     We use the ModelBuilderEngine to get database objects to display
        // </summary>
        private void bgWorkerPopulateTree_DoWork(object sender, DoWorkEventArgs args)
        {
            // This method will run on a thread other than the UI thread.
            // Be sure not to manipulate any Windows Forms controls created on the UI thread from this method.
            var result = new ICollection<EntityStoreSchemaFilterEntry>[3];
            try
            {
                result[0] = DatabaseMetadataQueryTool.GetTablesFilterEntries(Wizard.ModelBuilderSettings, args);
                if (args.Cancel)
                {
                    return;
                }

                result[1] = DatabaseMetadataQueryTool.GetViewFilterEntries(Wizard.ModelBuilderSettings, args);
                if (args.Cancel)
                {
                    return;
                }

                result[2] = DatabaseMetadataQueryTool.GetFunctionsFilterEntries(Wizard.ModelBuilderSettings, args);
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

        // <summary>
        //     RunWorkerCompleted event handler: Populate TreeViews here.
        //     This method is called by background worker component the same thread as the UI thread.
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
                    var errMsg = ModelBuilderWizardForm.ShowDatabaseConnectionErrorDialog(args.Error);

                    Visited = false;

                    // if database config could be at fault revert to database config page
                    // otherwise just show the error message in the wizard and enable the cancel button
                    if (ModelBuilderWizardForm.WizardMode.PerformDatabaseConfigAndSelectTables == Wizard.Mode)
                    {
                        Wizard.OnPrevious();
                    }
                    else
                    {
                        ShowStatus(errMsg);
                        Wizard.EnableButton(ButtonType.Cancel, true);
                    }
                }
                else
                {
                    // No errors, populate nodes in TreeViews
                    var result = (ICollection<EntityStoreSchemaFilterEntry>[])args.Result;
                    var tableEntries = result[0];
                    var viewEntries = result[1];
                    var sprocEntries = result[2];

                    // First find all tables, views and storedProc's which exist in the current model (before update from DB)
                    // (value is not used - but list is in sorted order to ensure they show up correctly on the wizard)
                    SortedDictionary<DatabaseObject, int> existingTables;
                    SortedDictionary<DatabaseObject, int> existingViews;
                    SortedDictionary<DatabaseObject, int> existingStoredProcs;
                    GetExistingTablesViewsAndSprocs(
                        Wizard.ModelBuilderSettings.Artifact,
                        out existingTables, out existingViews, out existingStoredProcs);

                    // now create the tree nodes
                    string storageEntityContainerName = null;
                    if (null != Wizard.ModelBuilderSettings.Artifact
                        && null != Wizard.ModelBuilderSettings.Artifact.StorageModel()
                        && null != Wizard.ModelBuilderSettings.Artifact.StorageModel().FirstEntityContainer
                        && null != Wizard.ModelBuilderSettings.Artifact.StorageModel().FirstEntityContainer.LocalName
                        && null != Wizard.ModelBuilderSettings.Artifact.StorageModel().FirstEntityContainer.LocalName.Value)
                    {
                        storageEntityContainerName =
                            Wizard.ModelBuilderSettings.Artifact.StorageModel().FirstEntityContainer.LocalName.Value;
                    }
                    CreateAddRefreshAndDeleteTreeNodes(
                        tableEntries,
                        viewEntries,
                        sprocEntries,
                        existingTables,
                        existingViews,
                        existingStoredProcs,
                        storageEntityContainerName);

                    // Hide status message
                    HideStatus();

                    // Enable tab pages and make Description fields visible
                    AddUpdateDeleteTabControl.Enabled = true;
                    DescriptionTextBox.Visible = true;

                    // Set focus to TreeView
                    var currentTreeView = CurrentTreeView;
                    if (null != currentTreeView)
                    {
                        currentTreeView.FocusAndSetFirstNodeSelected();
                    }

                    TreeViewsInitialized = true;
                    _initializedDataConnection = Wizard.ModelBuilderSettings.DesignTimeConnectionString;
                }

                // Enable wizard navigation
                Wizard.OnValidationStateChanged(this);
            }
            catch (Exception e)
            {
                ShowStatus(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        Resources.SelectTablesPage_ErrorRetrievingTablesText,
                        e.Message));
                Wizard.EnableButton(ButtonType.Cancel, true);
            }
        }

        private void CreateAddRefreshAndDeleteTreeNodes(
            ICollection<EntityStoreSchemaFilterEntry> tableEntries,
            ICollection<EntityStoreSchemaFilterEntry> viewEntries,
            ICollection<EntityStoreSchemaFilterEntry> sprocEntries,
            SortedDictionary<DatabaseObject, int> existingTables,
            SortedDictionary<DatabaseObject, int> existingViews,
            SortedDictionary<DatabaseObject, int> existingStoredProcs,
            string storageEntityContainerName)
        {
            // set up added items page
            CreateAddedItemsTreeNodes(
                tableEntries,
                viewEntries,
                sprocEntries,
                existingTables,
                existingViews,
                existingStoredProcs,
                storageEntityContainerName);

            // construct lists of all tables, views and storedProcs according to the DB
            Dictionary<DatabaseObject, EntityStoreSchemaFilterEntry> tablesFromDB;
            Dictionary<DatabaseObject, EntityStoreSchemaFilterEntry> viewsFromDB;
            Dictionary<DatabaseObject, EntityStoreSchemaFilterEntry> storedProcsFromDB;
            GetTablesViewsAndSprocsFromDB(
                tableEntries,
                viewEntries,
                sprocEntries,
                out tablesFromDB,
                out viewsFromDB,
                out storedProcsFromDB,
                storageEntityContainerName);

            // set up refreshed and deleted items pages
            CreateRefreshedAndDeletedItemsTreeNodes(existingTables,
                existingViews,
                existingStoredProcs,
                tablesFromDB,
                viewsFromDB,
                storedProcsFromDB);
        }

        private static List<EntityStoreSchemaFilterEntry> NotPreviouslyExistingNodes(
            ICollection<EntityStoreSchemaFilterEntry> entriesFromDB,
            string defaultSchemaName,
            SortedDictionary<DatabaseObject, int> existingDatabaseObjects)
        {
            var entriesFromDBThatDidNotPreviouslyExist = new List<EntityStoreSchemaFilterEntry>();

            foreach (var entry in entriesFromDB)
            {
                var dbObj = DatabaseObject.
                    CreateFromEntityStoreSchemaFilterEntry(entry, null);
                if (existingDatabaseObjects.ContainsKey(dbObj))
                {
                    // did previously exist
                    continue;
                }
                if (null == dbObj.Schema)
                {
                    // dbObj.Schema can be null with SQL CE databases which have no schema
                    // so try again using the defaultSchemaName for the schema
                    dbObj.Schema = defaultSchemaName;
                    if (!existingDatabaseObjects.ContainsKey(dbObj))
                    {
                        entriesFromDBThatDidNotPreviouslyExist.Add(entry);
                    }
                }
                else
                {
                    entriesFromDBThatDidNotPreviouslyExist.Add(entry);
                }
            }

            return entriesFromDBThatDidNotPreviouslyExist;
        }

        private void CreateAddedItemsTreeNodes(
            ICollection<EntityStoreSchemaFilterEntry> tableEntries,
            ICollection<EntityStoreSchemaFilterEntry> viewEntries,
            ICollection<EntityStoreSchemaFilterEntry> storedProcEntries,
            SortedDictionary<DatabaseObject, int> existingTables,
            SortedDictionary<DatabaseObject, int> existingViews,
            SortedDictionary<DatabaseObject, int> existingStoredProcs,
            string storageEntityContainerName)
        {
            var newTables = NotPreviouslyExistingNodes(tableEntries, storageEntityContainerName, existingTables);
            var newViews = NotPreviouslyExistingNodes(viewEntries, storageEntityContainerName, existingViews);
            var newSprocs = NotPreviouslyExistingNodes(storedProcEntries, storageEntityContainerName, existingStoredProcs);

            var addedTablesNode = DatabaseObjectTreeView.CreateRootNodeAndDescendents(
                newTables, Resources.SelectTablesPage_TablesNode, DatabaseObjectTreeView.TreeViewImage.DbTablesImage,
                DatabaseObjectTreeView.TreeViewImage.TableImage);
            var addedViewsNode = DatabaseObjectTreeView.CreateRootNodeAndDescendents(
                newViews, Resources.SelectTablesPage_ViewsNode, DatabaseObjectTreeView.TreeViewImage.DbViewsImage,
                DatabaseObjectTreeView.TreeViewImage.ViewImage);
            var addedSprocsNode = DatabaseObjectTreeView.CreateRootNodeAndDescendents(
                newSprocs, Resources.SelectTablesPage_StoredProceduresNode, DatabaseObjectTreeView.TreeViewImage.DbStoreProcsImage,
                DatabaseObjectTreeView.TreeViewImage.StoreProcImage);

            AddTreeView.TreeViewControl.Nodes.Add(addedTablesNode);
            AddTreeView.TreeViewControl.Nodes.Add(addedViewsNode);
            AddTreeView.TreeViewControl.Nodes.Add(addedSprocsNode);

            // if there are _new_ Sproc entries then enable the Create Function Imports box and by default set to checked
            if (newSprocs.Count > 0)
            {
                chkCreateFunctionImports.Enabled = true;
                chkCreateFunctionImports.Checked = true;
            }
        }

        private void CreateRefreshedAndDeletedItemsTreeNodes(
            SortedDictionary<DatabaseObject, int> existingTables,
            SortedDictionary<DatabaseObject, int> existingViews,
            SortedDictionary<DatabaseObject, int> existingStoredProcs,
            Dictionary<DatabaseObject, EntityStoreSchemaFilterEntry> tablesFromDB,
            Dictionary<DatabaseObject, EntityStoreSchemaFilterEntry> viewsFromDB,
            Dictionary<DatabaseObject, EntityStoreSchemaFilterEntry> storedProcsFromDB)
        {
            // setup top-level Tables, Views and Sprocs nodes for RefreshTree
            var refreshedTablesNode =
                DatabaseObjectTreeView.CreateTreeNode(
                    Resources.SelectTablesPage_TablesNode, false, null, Resources.SelectTablesPage_TablesNode,
                    DatabaseObjectTreeView.TreeViewImage.DbTablesImage, Resources.SelectTablesPage_TablesNode);
            var refreshedViewsNode =
                DatabaseObjectTreeView.CreateTreeNode(
                    Resources.SelectTablesPage_ViewsNode, false, null, Resources.SelectTablesPage_ViewsNode,
                    DatabaseObjectTreeView.TreeViewImage.DbViewsImage, Resources.SelectTablesPage_ViewsNode);
            var refreshedSprocsNode =
                DatabaseObjectTreeView.CreateTreeNode(
                    Resources.SelectTablesPage_StoredProceduresNode, false, null, Resources.SelectTablesPage_StoredProceduresNode,
                    DatabaseObjectTreeView.TreeViewImage.DbStoreProcsImage, Resources.SelectTablesPage_StoredProceduresNode);

            RefreshTreeView.TreeViewControl.Nodes.Add(refreshedTablesNode);
            RefreshTreeView.TreeViewControl.Nodes.Add(refreshedViewsNode);
            RefreshTreeView.TreeViewControl.Nodes.Add(refreshedSprocsNode);

            // setup top-level Tables, Views and Sprocs nodes for DeleteTree
            var deletedTablesNode =
                DatabaseObjectTreeView.CreateTreeNode(
                    Resources.SelectTablesPage_TablesNode, false, null, Resources.SelectTablesPage_TablesNode,
                    DatabaseObjectTreeView.TreeViewImage.DbTablesImage, Resources.SelectTablesPage_TablesNode);
            var deletedViewsNode =
                DatabaseObjectTreeView.CreateTreeNode(
                    Resources.SelectTablesPage_ViewsNode, false, null, Resources.SelectTablesPage_ViewsNode,
                    DatabaseObjectTreeView.TreeViewImage.DbViewsImage, Resources.SelectTablesPage_ViewsNode);
            var deletedSprocsNode =
                DatabaseObjectTreeView.CreateTreeNode(
                    Resources.SelectTablesPage_StoredProceduresNode, false, null, Resources.SelectTablesPage_StoredProceduresNode,
                    DatabaseObjectTreeView.TreeViewImage.DbStoreProcsImage, Resources.SelectTablesPage_StoredProceduresNode);

            DeleteTreeView.TreeViewControl.Nodes.Add(deletedTablesNode);
            DeleteTreeView.TreeViewControl.Nodes.Add(deletedViewsNode);
            DeleteTreeView.TreeViewControl.Nodes.Add(deletedSprocsNode);

            // any entry in existingTables which also exists in tablesFromDB is about to be refreshed
            // otherwise its about to be deleted
            foreach (var table in existingTables.Keys)
            {
                if (tablesFromDB.ContainsKey(table))
                {
                    DatabaseObjectTreeView.EnsureSchemaAndLeafNode(
                        refreshedTablesNode, table, DatabaseObjectTreeView.TreeViewImage.TableImage, tablesFromDB[table]);
                }
                else
                {
                    DatabaseObjectTreeView.EnsureSchemaAndLeafNode(
                        deletedTablesNode, table, DatabaseObjectTreeView.TreeViewImage.TableImage, null);
                }
            }

            // any entry in existingViews which also exists in viewsFromDB is about to be refreshed
            // otherwise its about to be deleted
            foreach (var view in existingViews.Keys)
            {
                if (viewsFromDB.ContainsKey(view))
                {
                    DatabaseObjectTreeView.EnsureSchemaAndLeafNode(
                        refreshedViewsNode, view, DatabaseObjectTreeView.TreeViewImage.ViewImage, viewsFromDB[view]);
                }
                else
                {
                    DatabaseObjectTreeView.EnsureSchemaAndLeafNode(
                        deletedViewsNode, view, DatabaseObjectTreeView.TreeViewImage.ViewImage, null);
                }
            }

            // similarly any entry in existingStoredProcNames which also exists in storedProcsFromDB is about to be refreshed
            foreach (var storedProc in existingStoredProcs.Keys)
            {
                if (storedProcsFromDB.ContainsKey(storedProc))
                {
                    DatabaseObjectTreeView.EnsureSchemaAndLeafNode(
                        refreshedSprocsNode, storedProc, DatabaseObjectTreeView.TreeViewImage.StoreProcImage, storedProcsFromDB[storedProc]);
                }
                else
                {
                    DatabaseObjectTreeView.EnsureSchemaAndLeafNode(
                        deletedSprocsNode, storedProc, DatabaseObjectTreeView.TreeViewImage.StoreProcImage, null);
                }
            }
        }

        private static void GetExistingTablesViewsAndSprocs(
            EFArtifact artifact,
            out SortedDictionary<DatabaseObject, int> existingTables,
            out SortedDictionary<DatabaseObject, int> existingViews,
            out SortedDictionary<DatabaseObject, int> existingStoredProcs)
        {
            existingTables = new SortedDictionary<DatabaseObject, int>(_databaseObjectNameFirstComparer);
            existingViews = new SortedDictionary<DatabaseObject, int>(_databaseObjectNameFirstComparer);
            existingStoredProcs = new SortedDictionary<DatabaseObject, int>(_databaseObjectNameFirstComparer);

            if (artifact != null
                && artifact.StorageModel() != null)
            {
                if (artifact.StorageModel().EntityTypes() != null)
                {
                    foreach (var es in artifact.StorageModel().FirstEntityContainer.EntitySets())
                    {
                        var ses = es as StorageEntitySet;
                        if (null != ses)
                        {
                            var tableOrView = DatabaseObject.CreateFromEntitySet(ses);
                            if (ses.StoreSchemaGeneratorTypeIsView)
                            {
                                existingViews.Add(tableOrView, 0);
                            }
                            else
                            {
                                existingTables.Add(tableOrView, 0);
                            }
                        }
                    }
                }

                if (artifact.StorageModel().Functions() != null)
                {
                    foreach (var f in artifact.StorageModel().Functions())
                    {
                        var ssp = DatabaseObject.CreateFromFunction(f);
                        existingStoredProcs.Add(ssp, 0);
                    }
                }
            }
        }

        private static void GetTablesViewsAndSprocsFromDB(
            ICollection<EntityStoreSchemaFilterEntry> tableEntries,
            ICollection<EntityStoreSchemaFilterEntry> viewEntries,
            ICollection<EntityStoreSchemaFilterEntry> storedProcEntries,
            out Dictionary<DatabaseObject, EntityStoreSchemaFilterEntry> tablesFromDB,
            out Dictionary<DatabaseObject, EntityStoreSchemaFilterEntry> viewsFromDB,
            out Dictionary<DatabaseObject, EntityStoreSchemaFilterEntry> storedProcsFromDB,
            string defaultSchemaName)
        {
            tablesFromDB = new Dictionary<DatabaseObject, EntityStoreSchemaFilterEntry>();
            viewsFromDB = new Dictionary<DatabaseObject, EntityStoreSchemaFilterEntry>();
            storedProcsFromDB = new Dictionary<DatabaseObject, EntityStoreSchemaFilterEntry>();

            // construct a list of all tables and views according to the DB
            foreach (var entry in tableEntries)
            {
                var table = DatabaseObject.CreateFromEntityStoreSchemaFilterEntry(entry, defaultSchemaName);
                tablesFromDB.Add(table, entry);
            }

            foreach (var entry in viewEntries)
            {
                var view = DatabaseObject.CreateFromEntityStoreSchemaFilterEntry(entry, defaultSchemaName);
                viewsFromDB.Add(view, entry);
            }

            // similarly find list of all sprocs according to the DB
            foreach (var entry in storedProcEntries)
            {
                var sproc = DatabaseObject.CreateFromEntityStoreSchemaFilterEntry(entry, defaultSchemaName);
                storedProcsFromDB.Add(sproc, entry);
            }
        }

        private ICollection<EntityStoreSchemaFilterEntry> GetSelectedFilterEntriesFromTreeView()
        {
            var mapper = new TreeViewSchemaFilterMapper();
            mapper.AddTreeView(AddTreeView.TreeViewControl, null);
            mapper.AddTreeView(RefreshTreeView.TreeViewControl, new TreeViewSchemaFilterMapperSettings { UseOnlyCheckedNodes = false });
            var filterEntryBag = mapper.CreateSchemaFilterEntryBag();

            // This should always be a byval edmx
            return filterEntryBag.CollapseAndOptimize(SchemaFilterPolicy.GetByValEdmxPolicy());
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "result")]
        private void UpdateNewFunctionFilterEntries()
        {
            var mapper = new TreeViewSchemaFilterMapper();
            mapper.AddTreeView(AddTreeView.TreeViewControl, null);
            var filterEntryBag = mapper.CreateSchemaFilterEntryBag();

            // because we only added the AddTreeView above IncludedSprocEntries is the list of selected
            // sprocs in the Add tab only
            var newFunctionEntries = filterEntryBag.IncludedSprocEntries.ToList();

            // if there are any new Function entries and if the user has selected to create matching Function Imports
            // then create and run a ProgressDialog while we are collecting the sproc return type info
            if (newFunctionEntries.Count > 0
                && chkCreateFunctionImports.Checked)
            {
                var result = ProgressDialogHelper.ShowProgressDialog(this, newFunctionEntries, Wizard.ModelBuilderSettings);
            }
        }

        // <summary>
        //     Helper to show a status message Label control on top of the client area of the TreeView control
        // </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void ShowStatus(string message)
        {
            HideStatus();

            var currentTreeView = CurrentTreeView;
            if (null != currentTreeView)
            {
                // dpi/scaling may have changed since the last time we showed a status
                using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
                {
                    _statusLabel = new Label
                    {
                        BackColor = currentTreeView.TreeViewControl.BackColor,
                        Size = currentTreeView.ClientSize,
                        Location = new Point(
                            currentTreeView.Left + SystemInformation.Border3DSize.Width,
                            currentTreeView.Top + SystemInformation.Border3DSize.Height),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Anchor = currentTreeView.Anchor,
                        Text = message
                    };

                    var tabPage = AddUpdateDeleteTabControl.SelectedTab;
                    tabPage.Controls.Add(_statusLabel);
                    tabPage.Controls.SetChildIndex(_statusLabel, 0);
                }
            }
        }

        // <summary>
        //     Helper to hide the status message Label control
        // </summary>
        private void HideStatus()
        {
            if (_statusLabel != null)
            {
                using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
                {
                    AddUpdateDeleteTabControl.SelectedTab.Controls.Remove(_statusLabel);
                    _statusLabel = null;
                }
            }
        }

        private DatabaseObjectTreeView CurrentTreeView
        {
            get
            {
                DatabaseObjectTreeView tree = null;

                var controls = AddUpdateDeleteTabControl.SelectedTab.Controls.Find("AddTreeView", true);
                Debug.Assert(controls.Length == 1, "controls.Length (" + controls.Length + ") should be 1");
                if (controls.Length > 0)
                {
                    tree = controls[0] as DatabaseObjectTreeView;
                }

                return tree;
            }
        }

        private static bool TryReadDesignerProperty(DesignerProperty prop, bool defaultValue)
        {
            if (prop != null
                && prop.ValueAttr != null)
            {
                bool v;
                if (Boolean.TryParse(prop.ValueAttr.Value, out v))
                {
                    return v;
                }
            }
            return defaultValue;
        }

        // <summary>
        //     Display the tooltip if the user hover the mouse on the complex return type controls.
        // </summary>
        private void IncludeForeignKeysArea_OnMouseMove(Object sender, MouseEventArgs e)
        {
            WizardPageSelectTables.ToolTipOnDisabledControl_OnMouseMove(
                sender, e, chkIncludeForeignKeys, toolTip, ref _controlWithToolTipShown);
        }

        private void IncludeForeignKeysArea_OnMouseLeave(object sender, EventArgs e)
        {
            WizardPageSelectTables.ToolTipOnDisabledControl_OnMouseLeave(sender, e, toolTip, ref _controlWithToolTipShown);
        }

        private void AddTabPage_Enter(object sender, EventArgs e)
        {
            DescriptionTextBox.Text = Resources.UpdateFromDatabase_AddDescription;
        }

        private void RefreshTabPage_Enter(object sender, EventArgs e)
        {
            DescriptionTextBox.Text = Resources.UpdateFromDatabase_RefreshDescription;
        }

        private void DeleteTabPage_Enter(object sender, EventArgs e)
        {
            DescriptionTextBox.Text = Resources.UpdateFromDatabase_DeleteDescription;
        }
    }
}
