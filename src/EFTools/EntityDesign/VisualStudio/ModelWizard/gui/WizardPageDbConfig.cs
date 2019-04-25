// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Data.Core;
    using Microsoft.VisualStudio.Data.Services;
    using Microsoft.VisualStudio.DataTools.Interop;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VSDesigner.Data;
    using Microsoft.VSDesigner.VSDesignerPackage;
    using Microsoft.WizardFramework;
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Windows.Forms;
    using Resources = Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties.Resources;

    // <summary>
    //     This is the second page in the ModelGen VS wizard and is invoked if the user wants to generate the model from a database.
    //     In this page the user can:
    //     - select the server and database to generate the model from
    //     - choose whether to save the connection string in App.Config
    // </summary>
    // to view this class in the forms designer, make it temporarily derive from Microsoft.WizardFramework.WizardPage
    internal partial class WizardPageDbConfig : WizardPageBase
    {
        private readonly ConfigFileUtils _configFileUtils;
        private readonly CodeIdentifierUtils _identifierUtil;
        private bool _isInitialized;

        //  DDEX Services we use
        private IVsDataConnectionManager _dataConnectionManager;
        private IVsDataExplorerConnectionManager _dataExplorerConnectionManager;
        private IVsDataProviderManager _dataProviderManager;
        private IVsDataConnection _dataConnection;

        // will be set to true if the focus should not be changed on page activation
        private bool _isFocusSet;
        
        internal WizardPageDbConfig(ModelBuilderWizardForm wizard)
            : base(wizard)
        {
            InitializeComponent();

            _configFileUtils =
                new ConfigFileUtils(wizard.Project, wizard.ServiceProvider, wizard.ModelBuilderSettings.VSApplicationType);

            _identifierUtil =
                new CodeIdentifierUtils(wizard.ModelBuilderSettings.VSApplicationType, VsUtils.GetLanguageForProject(wizard.Project));

            Headline = Resources.DbConfigPage_Title;
            Logo = Resources.PageIcon;
            Id = "WizardPageDbConfigId";
            ShowInfoPanel = false;

            sensitiveInfoTextBox.Text = Resources.SensitiveDataInfoText;
            disallowSensitiveInfoButton.Text = Resources.DisallowSensitiveDataInfoText;
            allowSensitiveInfoButton.Text = Resources.AllowSensitiveDataInfoText;
            if (VsUtils.IsWebProject(wizard.ModelBuilderSettings.VSApplicationType))
            {
                checkBoxSaveInAppConfig.Text = Resources.SaveConnectionLabelASP;
            }
            else
            {
                checkBoxSaveInAppConfig.Text = Resources.SaveConnectionLabel;
            }

            // make the App/Web.Config connection name entry non-editable for 'Update Model' and 'Generate Database' scenarios
            if (ModelBuilderWizardForm.WizardMode.PerformDatabaseConfigAndSelectTables == Wizard.Mode
                || ModelBuilderWizardForm.WizardMode.PerformDatabaseConfigAndDBGenSummary == Wizard.Mode
                || ModelBuilderWizardForm.WizardMode.PerformDBGenSummaryOnly == Wizard.Mode)
            {
                textBoxAppConfigConnectionName.Enabled = false;
            }

            newDBConnectionButton.Text = Resources.NewDatabaseConnectionBtn;
            newDBConnectionButton.BackColor = SystemColors.Control;
            newDBConnectionButton.ForeColor = SystemColors.ControlText;
            lblEntityConnectionString.Text = Resources.ConnectionStringLabel;
            lblPagePrompt.Text = Resources.WhichDataConnectionLabel;
            lblPagePrompt.Font = LabelFont;

            sensitiveInfoTextBox.Enabled = false;
            disallowSensitiveInfoButton.Enabled = false;
            allowSensitiveInfoButton.Enabled = false;
            HelpKeyword = null;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Init()
        {
            if (_isInitialized)
            {
                // need to reset settings on the dialog to avoid displaying stale information
                // especially showing EntityConnectionString for CodeFirst from Database
                NewDataSourceSelected();
                return;
            }

            using (new VsUtils.HourglassHelper())
            {
                // 
                //  look up the DDEX Service Providers we use in this wizard
                //
                _dataConnectionManager = ServiceProvider.GetService(typeof(IVsDataConnectionManager)) as IVsDataConnectionManager;
                _dataExplorerConnectionManager =
                    ServiceProvider.GetService(typeof(IVsDataExplorerConnectionManager)) as IVsDataExplorerConnectionManager;
                _dataProviderManager = ServiceProvider.GetService(typeof(IVsDataProviderManager)) as IVsDataProviderManager;
                
                var providerMapper = ServiceProvider.GetService(typeof(IDTAdoDotNetProviderMapper)) as IDTAdoDotNetProviderMapper;
                Debug.Assert(providerMapper != null, "providerMapper == null");

                // populate the combo box with project connections first
                var globalConnectionService = ServiceProvider.GetService(typeof(IGlobalConnectionService)) as IGlobalConnectionService;

                var dataConnections = new Dictionary<string, DataConnection>();
                if (null != globalConnectionService)
                {
                    try
                    {
                        var connections = globalConnectionService.GetConnections(ServiceProvider, Wizard.Project);
                        foreach (var connection in connections)
                        {
                            if (connection.Location == ConnectionLocation.SettingsFile
                                || connection.Location == ConnectionLocation.Both)
                            {
                                var providerGuid = providerMapper.MapInvariantNameToGuid(
                                    connection.ProviderName, connection.DesignTimeConnectionString, false);
                                if (DataConnectionUtils.HasEntityFrameworkProvider(
                                    _dataProviderManager, providerGuid, Wizard.Project, ServiceProvider)
                                    && DataProviderProjectControl.IsProjectSupported(providerGuid, Wizard.Project))
                                {
                                    dataSourceComboBox.Items.Add(
                                        new DataSourceComboBoxItem(
                                            connection.Name + " (Settings)", providerGuid, connection.DesignTimeConnectionString, false));
                                    dataConnections.Add(connection.DesignTimeConnectionString, connection);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // there is a bug in the VSDesigner; it throws an exception when attempting to call GetConnections if there is a bad connection
                        // if there is a problem, the only thing we can do is just add all connections, and the project connections won't be sorted in order.
                    }
                }

                // populate the combo box with connection names from server explorer
                foreach (var connection in _dataExplorerConnectionManager.Connections.Values)
                {
                    if (DataConnectionUtils.HasEntityFrameworkProvider(
                        _dataProviderManager, connection.Provider, Wizard.Project, ServiceProvider)
                        && DataProviderProjectControl.IsProjectSupported(connection.Provider, Wizard.Project)
                        && !dataConnections.ContainsKey(DataConnectionUtils.DecryptConnectionString(connection.Connection)))
                    {
                        dataSourceComboBox.Items.Add(new DataSourceComboBoxItem(connection.DisplayName, connection.Connection));
                    }
                }

                // highlight the first one in the list
                if (dataSourceComboBox.Items.Count > 0)
                {
                    dataSourceComboBox.SelectedIndex = 0;
                }

                // set a minimum height for the connection string text box
                if (textBoxConnectionString.Height < textBoxConnectionString.Font.Height)
                {
                    textBoxConnectionString.Height = textBoxConnectionString.Font.Height;
                }

                // mark as initialized
                _isInitialized = true;
            } // restore cursor
        }

        // this will be called by Wizard to check if the Next button can be enabled
        public override bool IsDataValid
        {
            get
            {
                if (_dataConnection == null
                    || (ContainsSensitiveData(_dataConnection) &&
                        allowSensitiveInfoButton.Checked == false && disallowSensitiveInfoButton.Checked == false))
                {
                    return false;
                }

                return true;
            }
        }

        public override bool OnActivate()
        {
            // don't allow activating this page if the name provided by the user
            // conflicts with an existing file because the initialization steps
            // we perform on activation will fail due to model name not being set
            return base.OnActivate() && !Wizard.FileAlreadyExistsError;
        }

        public override void OnActivated()
        {
            base.OnActivated();

            Init();

            if (_isFocusSet)
            {
                _isFocusSet = false;
            }
            else
            {
                dataSourceComboBox.Focus();
            }

            // We shouldn't be allowed to 'Finish'
            if (ModelBuilderWizardForm.WizardMode.PerformDatabaseConfigAndDBGenSummary == Wizard.Mode
                || ModelBuilderWizardForm.WizardMode.PerformDBGenSummaryOnly == Wizard.Mode)
            {
                Wizard.EnableButton(ButtonType.Finish, false);
            }
        }

        // <summary>
        //     Invoked by the VS Wizard framework when this page is exited or when the "Finish" button is clicked on this page
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

            // if database is local check if we should copy it into the project
            if (ModelBuilderWizardForm.WizardMode.PerformAllFunctionality == Wizard.Mode
                && Wizard.MovingNext)
            {
                if (!PromptConvertLocalConnection())
                {
                    return false;
                }
            }

            return base.OnDeactivate();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal override bool OnWizardFinish()
        {
            using (new VsUtils.HourglassHelper())
            {
                UpdateSettingsFromGui();
            }

            //
            // validate app config connection name
            //
            if (checkBoxSaveInAppConfig.Checked)
            {
                var id = textBoxAppConfigConnectionName.Text;
                if (!EscherAttributeContentValidator.IsValidCsdlEntityContainerName(id)
                    || !_identifierUtil.IsValidIdentifier(id))
                {
                    VsUtils.ShowErrorDialog(
                        string.Format(CultureInfo.CurrentCulture, Resources.ConnectionStringNonValidIdentifier, id));
                    textBoxAppConfigConnectionName.Focus();
                    _isFocusSet = true;
                    return false;
                }

                // only check that the connection string name is new if started in
                // 'PerformAllFunctionality' mode
                if (ModelBuilderWizardForm.WizardMode.PerformAllFunctionality == Wizard.Mode)
                {
                    string connectionString;
                    if (ConnectionManager.GetExistingConnectionStrings(_configFileUtils).TryGetValue(id, out connectionString)
                        && !string.Equals(textBoxConnectionString.Text, connectionString, StringComparison.Ordinal))
                    {
                        VsUtils.ShowErrorDialog(
                            string.Format(CultureInfo.CurrentCulture, Resources.ConnectionStringDuplicateIdentifer, id));
                        textBoxAppConfigConnectionName.Focus();
                        _isFocusSet = true;
                        return false;
                    }
                }
            }

            // the Model Namespace and the Entity Container name must differ
            if (ModelBuilderWizardForm.ModelNamespaceAndEntityContainerNameSame(Wizard.ModelBuilderSettings))
            {
                var s = Resources.NamespaceAndEntityContainerSame;
                VsUtils.ShowErrorDialog(
                    String.Format(CultureInfo.CurrentCulture, s, Wizard.ModelBuilderSettings.AppConfigConnectionPropertyName));
                textBoxAppConfigConnectionName.Focus();
                _isFocusSet = true;
                return false;
            }

            try
            {
                // this might cause dataConnection to include some sensitive data into connectionString
                // the Open function also can cause DDEX to put up a prompt for the username/password for an existing connection
                // that does not have any saved password information.
                _dataConnection.Open();

                if (!IsDataValid)
                {
                    sensitiveInfoTextBox.Enabled = true;
                    allowSensitiveInfoButton.Checked = false;
                    allowSensitiveInfoButton.Enabled = true;
                    disallowSensitiveInfoButton.Checked = false;
                    disallowSensitiveInfoButton.Enabled = true;

                    var result = VsUtils.ShowMessageBox(
                        PackageManager.Package,
                        Resources.SensitiveDataInfoText,
                        OLEMSGBUTTON.OLEMSGBUTTON_YESNOCANCEL,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_THIRD,
                        OLEMSGICON.OLEMSGICON_QUERY);

                    switch (result)
                    {
                        case DialogResult.Yes:
                            allowSensitiveInfoButton.Checked = true;
                            break;
                        case DialogResult.No:
                            disallowSensitiveInfoButton.Checked = true;
                            break;
                        default:
                            Wizard.OnValidationStateChanged(this);
                            return false;
                    }
                }
            }
            catch (DataConnectionOpenCanceledException)
            {
                return false;
            }
            catch (Exception e)
            {
                // show error dialog
                ModelBuilderWizardForm.ShowDatabaseConnectionErrorDialog(e);

                return false;
            }
            finally
            {
                if (_dataConnection.State != DataConnectionState.Closed)
                {
                    _dataConnection.Close();
                }
            }

            return true;
        }

        // <summary>
        //     Prompts the user to copy the database file into the project if appropriate
        // </summary>
        // <returns>whether wizard should continue to next page</returns>
        private bool PromptConvertLocalConnection()
        {
            // first check whether connection is Local
            if (!LocalDataUtil.IsLocalDbFileConnectionString(
                Wizard.ModelBuilderSettings.DesignTimeProviderInvariantName,
                Wizard.ModelBuilderSettings.DesignTimeConnectionString))
            {
                return true;
            }

            // if connection is Local but has already been copied into the project should not prompt
            var filePath = LocalDataUtil.GetLocalDbFilePath(
                Wizard.ModelBuilderSettings.DesignTimeProviderInvariantName,
                Wizard.ModelBuilderSettings.DesignTimeConnectionString);
            if (!string.IsNullOrEmpty(filePath))
            {
                var projectItemCollection = LocalDataUtil.GetDefaultCollectionForLocalDataFile(Wizard.ServiceProvider, Wizard.Project);
                var targetPath = VsUtils.ConstructTargetPathForDatabaseFile(
                    Wizard.Project, projectItemCollection, Path.GetFileName(filePath));
                if (File.Exists(targetPath))
                {
                    return true;
                }
            }

            // Special case -- Check if this is a SQL Mobile Device
            if (LocalDataUtil.IsSqlMobileDeviceConnectionString(
                Wizard.ModelBuilderSettings.DesignTimeProviderInvariantName,
                Wizard.ModelBuilderSettings.DesignTimeConnectionString))
            {
                // For mobile devices, if the connection starts with 'Mobile Device' it means that the connection
                // refers to a location on the device itself. Do not perform conversion.
                return true;
            }

            // ask user if they want to copy
            var result = VsUtils.ShowMessageBox(
                Wizard.ServiceProvider,
                Resources.LocalDataConvertConnectionText,
                OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                OLEMSGICON.OLEMSGICON_QUERY);

            if (DialogResult.Yes != result)
            {
                return true;
            }

            try
            {
                return CopyFileIntoProjectAndConvertConnectionString(filePath);
            }
            catch (FileCopyException e)
            {
                var errMsgWithInnerExceptions = VsUtils.ConstructInnerExceptionErrorMessage(e);
                var errMsg = string.Format(
                    CultureInfo.CurrentCulture, Resources.LocalDataExceptionCopyingFile, e.GetType().FullName, filePath,
                    errMsgWithInnerExceptions);
                VsUtils.ShowErrorDialog(errMsg);
                return false;
            }
        }

        // <summary>
        //     Copies the local file into the project
        // </summary>
        // <param name="filePath">Path to file to copy</param>
        // <returns>whether the copy is successful</returns>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsTrackProjectDocuments3.HandsOffFiles(System.UInt32,System.Int32,System.String[])")]
        private bool CopyFileIntoProjectAndConvertConnectionString(string filePath)
        {
            try
            {
                // File is not in the project and user wants to convert it.
                var serviceProvider = Wizard.ServiceProvider;
                var project = Wizard.Project;
                var vsTrackProjectDocuments = serviceProvider.GetService(typeof(SVsTrackProjectDocuments)) as IVsTrackProjectDocuments3;

                if (null == vsTrackProjectDocuments)
                {
                    Debug.Fail("Could not get IVsTrackProjectDocuments3 from service provider.");
                }
                else
                {
                    // releases any read locks on this file so we can read it
                    vsTrackProjectDocuments.HandsOffFiles((uint)__HANDSOFFMODE.HANDSOFFMODE_ReadAccess, 1, new[] { filePath });
                }

                ProjectItem dbProjectItem = null;
                var targetProjectItemCollection = LocalDataUtil.GetDefaultCollectionForLocalDataFile(Wizard.ServiceProvider, project);
                dbProjectItem = VsUtils.BringDatabaseFileIntoProject(serviceProvider, project, targetProjectItemCollection, filePath);

                if (dbProjectItem == null)
                {
                    var errmsg = string.Format(CultureInfo.CurrentCulture, Resources.LocalDataErrorAddingFile, filePath, project.UniqueName);
                    throw new FileCopyException(errmsg);
                }

                var newFilePath = (string)dbProjectItem.Properties.Item("FullPath").Value;

                // now re-target the connection string at the copied file
                return RetargetConnectionString(newFilePath);
            }
            catch (COMException ex)
            {
                if (ex.ErrorCode == VSConstants.OLE_E_PROMPTSAVECANCELLED)
                {
                    // User canceled a checkout prompt.
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        // <summary>
        //     Changes connection string to point to new file path
        // </summary>
        // <param name="newFilePath">New file path</param>
        // <returns>whether successful</returns>
        private bool RetargetConnectionString(string newFilePath)
        {
            // compute the Design and Runtime Connection String values based on the new file path
            // Note: newAppConfigConnectionString should use the |DataDirectory| macro because
            // that is what's displayed on screen and interpreted at runtime, but newConnectionString
            // should not use the macro as it will be used by the wizard at the next step to create
            // a working DB connection
            var filePathKey = LocalDataUtil.GetFilePathKey(
                Wizard.ModelBuilderSettings.DesignTimeProviderInvariantName,
                Wizard.ModelBuilderSettings.DesignTimeConnectionString);
            var newConnectionString = ConvertConnectionStringToNewPath(
                Wizard.ModelBuilderSettings.DesignTimeProviderInvariantName, filePathKey,
                Wizard.ModelBuilderSettings.DesignTimeConnectionString, newFilePath, false);
            var newAppConfigConnectionString = ConvertConnectionStringToNewPath(
                Wizard.ModelBuilderSettings.DesignTimeProviderInvariantName, filePathKey,
                Wizard.ModelBuilderSettings.AppConfigConnectionString, newFilePath, true);

            // update the DataConnection itself with the new path
            var oldProvider = DataConnectionUtils.GetVsProvider(_dataProviderManager, _dataConnection);
            if (null == oldProvider)
            {
                Debug.Fail("Could not find VS Provider for DataConnection " + _dataConnection.DisplayConnectionString);
                return false;
            }

            if (null == _dataConnectionManager)
            {
                Debug.Fail("this._dataConnectionManager is null - cannot construct new DataConnection");
                return false;
            }

            var dataConnection = _dataConnectionManager.GetConnection(oldProvider.Guid, newConnectionString, false);
            if (null == dataConnection)
            {
                Debug.Fail(
                    "DataConnectionManager could not get a connection for provider " + oldProvider.Guid + ", newConnectionString "
                    + newConnectionString);
                return false;
            }

            // update this WizardPage's DataConnection and also set the DataSourceComboBoxItem's 
            // dataConnection to this so that this connection is used if the user re-selects this
            // item from the drop-down
            SetDataConnection(dataConnection);
            var currentDataSourceComboBoxItem = dataSourceComboBox.SelectedItem as DataSourceComboBoxItem;
            Debug.Assert(null != currentDataSourceComboBoxItem, "Currently selected should not be null");
            if (null != currentDataSourceComboBoxItem)
            {
                currentDataSourceComboBoxItem.ResetDataConnection(dataConnection);
            }

            // update the Design and Runtime Connection String values stored in ModelBuilderSettings
            // these connection strings & invariant names are coming from the ddex provider, so these are "design-time"
            Wizard.ModelBuilderSettings.SetInvariantNamesAndConnectionStrings(ServiceProvider,
                Wizard.Project, Wizard.ModelBuilderSettings.DesignTimeProviderInvariantName, newConnectionString,
                newAppConfigConnectionString, true);
            return true;
        }

        private const string DataDirectoryMacro = "|DataDirectory|";

        private string ConvertConnectionStringToNewPath(
            string providerInvariantName, string filePathKey, string oldConnectionString, string newFilePath, bool useDataDirectoryMacro)
        {
            if (string.IsNullOrEmpty(filePathKey))
            {
                Debug.Fail("requires non-null, non-empty filePathKey");
                return oldConnectionString;
            }

            if (LocalDataUtil.IsSqlMobileConnectionString(providerInvariantName))
            {
                // DbConnectionString does not support SQL Mobile
                return GenerateNewSqlMobileConnectionString(oldConnectionString, newFilePath, useDataDirectoryMacro);
            }

            var dbConnectionStringBuilder = new DbConnectionStringBuilder();
            dbConnectionStringBuilder.ConnectionString = oldConnectionString;
            object filePathObject;
            dbConnectionStringBuilder.TryGetValue(filePathKey, out filePathObject);
            var filePath = filePathObject as string;
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.Fail("could not find filePath for filePathKey=" + filePathKey);
                return oldConnectionString;
            }

            // replace old path with new one
            dbConnectionStringBuilder.Remove(filePathKey);
            if (useDataDirectoryMacro)
            {
                dbConnectionStringBuilder.Add(filePathKey, DataDirectoryMacro + Path.DirectorySeparatorChar + Path.GetFileName(newFilePath));
            }
            else
            {
                dbConnectionStringBuilder.Add(filePathKey, newFilePath);
            }

            return dbConnectionStringBuilder.ConnectionString;
        }

        internal string GenerateNewSqlMobileConnectionString(string oldConnectionString, string newFilePath, bool useDataDirectoryMacro)
        {
            var connPropertiesSvc = DataConnectionUtils.GetConnectionProperties(_dataProviderManager, _dataConnection);
            if (connPropertiesSvc == null)
            {
                Debug.Fail("Could not get connection properties service for Data Connection " + _dataConnection.DisplayConnectionString);
                return oldConnectionString;
            }
            connPropertiesSvc.Parse(oldConnectionString);

            // We need to ensure a path to the device is not converted to a desktop path
            object dataSourceObject;
            connPropertiesSvc.TryGetValue(LocalDataUtil.CONNECTION_PROPERTY_DATA_SOURCE, out dataSourceObject);
            var dataSource = dataSourceObject as string;
            if (!string.IsNullOrEmpty(dataSource)
                && dataSource.StartsWith(LocalDataUtil.SQL_MOBILE_DEVICE, StringComparison.OrdinalIgnoreCase))
            {
                return oldConnectionString;
            }

            // Update the database file path
            if (useDataDirectoryMacro)
            {
                connPropertiesSvc[LocalDataUtil.CONNECTION_PROPERTY_DATA_SOURCE] = DataDirectoryMacro + Path.DirectorySeparatorChar
                                                                                   + Path.GetFileName(newFilePath);
            }
            else
            {
                connPropertiesSvc[LocalDataUtil.CONNECTION_PROPERTY_DATA_SOURCE] = newFilePath;
            }

            return connPropertiesSvc.ToString();
        }

        // <summary>
        //     Helper to update ModelBuilderSettings from GUI
        //     TODO: validate settings on this page are correct
        // </summary>
        private void UpdateSettingsFromGui()
        {
            // don't bother saving anything if there isn't a connection; the user isn't allowed to proceed forward in the wizard anyways
            if (_dataConnection == null)
            {
                return;
            }

            var decryptedConnectionString = DataConnectionUtils.DecryptConnectionString(_dataConnection);
            var appConfigConnectionString = String.Empty;

            if (checkBoxSaveInAppConfig.Enabled
                && checkBoxSaveInAppConfig.Checked)
            {
                Wizard.ModelBuilderSettings.SaveConnectionStringInAppConfig = true;
                Wizard.ModelBuilderSettings.AppConfigConnectionPropertyName = textBoxAppConfigConnectionName.Text;

                if (allowSensitiveInfoButton.Checked)
                {
                    appConfigConnectionString = decryptedConnectionString;
                }
                else
                {
                    appConfigConnectionString = _dataConnection.SafeConnectionString;
                }
            }
            else
            {
                Wizard.ModelBuilderSettings.SaveConnectionStringInAppConfig = false;
                Wizard.ModelBuilderSettings.AppConfigConnectionPropertyName = string.Empty;
            }

            // save the initial catalog
            Wizard.ModelBuilderSettings.InitialCatalog = DataConnectionUtils.GetInitialCatalog(_dataProviderManager, _dataConnection);

            // these connection strings & invariant names are coming from the ddex provider, so these are "design-time"
            var invariantName = DataConnectionUtils.GetProviderInvariantName(_dataProviderManager, _dataConnection.Provider);
            Wizard.ModelBuilderSettings.SetInvariantNamesAndConnectionStrings(ServiceProvider,
                Wizard.Project, invariantName, decryptedConnectionString, appConfigConnectionString, true);
        }

        // internal for testing
        internal string GetTextBoxConnectionStringValue(IVsDataProviderManager dataProviderManager, Guid providerGuid, string maskedConnectionString)
        {
            // providerGuid will be the design-time provider guid from DDEX, so we need to translate to the runtime provider invariant name 
            var designTimeProviderInvariantName = DataConnectionUtils.GetProviderInvariantName(dataProviderManager, providerGuid);
            var runtimeProviderInvariantName = ConnectionManager.TranslateInvariantName(
                ServiceProvider, designTimeProviderInvariantName, maskedConnectionString, true);

            var runtimeConnectionString = ConnectionManager.TranslateConnectionString(ServiceProvider,
                Wizard.Project, designTimeProviderInvariantName, maskedConnectionString, true);

            if (Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.GenerateFromDatabase
                || Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.GenerateDatabaseScript)
            {
                var metadataFiles = ConnectionManager.GetMetadataFileNamesFromArtifactFileName(
                    Wizard.Project, Wizard.ModelBuilderSettings.ModelPath, ServiceProvider);

                return ConnectionManager.CreateEntityConnectionString(
                    Wizard.Project,
                    Wizard.ModelBuilderSettings.VSApplicationType,
                    metadataFiles,
                    runtimeConnectionString,
                    runtimeProviderInvariantName).Text;
            }
            else
            {
                Debug.Assert(
                    Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.CodeFirstFromDatabase,
                    "Unexpected model generation option");

                return ConnectionManager.InjectEFAttributesIntoConnectionString(
                    runtimeConnectionString, runtimeProviderInvariantName);
            }
        }

        private static bool ContainsSensitiveData(IVsDataConnection dataConnection)
        {
            if (dataConnection == null)
            {
                return false;
            }
                // need to compare ignoring case as DecryptConnectionString() can turn e.g. 'Integrated Security' into 'integrated security'
            else if (dataConnection.DisplayConnectionString.Equals(
                DataConnectionUtils.DecryptConnectionString(dataConnection), StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void newDBConnectionButton_Click(object sender, EventArgs e)
        {
            try
            {
                var dialog = new EntityDataConnectionDialog(Wizard.Project);
                dialog.ShowDialog();

                // the below values may be null if e.g. the user hit Cancel on the dialog
                // without selecting a connection
                if (dialog.SelectedConnection != null
                    && dialog.SelectedExplorerConnection != null)
                {
                    var index =
                        dataSourceComboBox.Items.Add(
                            new DataSourceComboBoxItem(dialog.SelectedExplorerConnection.DisplayName, dialog.SelectedConnection));

                    // changing the selection in the combo box will cause the NewDataSourceSelected() to be called
                    dataSourceComboBox.SelectedIndex = index;
                }
            }
            catch (Exception ex)
            {
                VsUtils.ShowErrorDialog(ex.Message);
            }
        }

        private string GetDefaultAppConfigEntryName(IVsDataConnection dataConnection)
        {
            // get the initial catalog string from the connection
            var initialCatalog = DataConnectionUtils.GetInitialCatalog(_dataProviderManager, dataConnection);

            Wizard.ModelBuilderSettings.InitialCatalog = initialCatalog;

            // compute the connection string name
            return ConnectionManager.GetUniqueConnectionStringName(
                _configFileUtils,
                Wizard.ModelBuilderSettings.GenerationOption == ModelGenerationOption.CodeFirstFromDatabase
                    ? Wizard.ModelBuilderSettings.ModelName
                    : GetBaseEdmxConnectionStringName(initialCatalog));
        }

        private string GetBaseEdmxConnectionStringName(string baseName)
        {
            if (String.IsNullOrEmpty(baseName))
            {
                baseName = ModelConstants.DefaultEntityContainerName;
            }
            else
            {
                // replace spaces with underscores
                baseName = baseName.Replace(" ", "_");
                baseName = baseName.Replace("\t", "_");
                baseName = baseName + ModelConstants.DefaultEntityContainerName;

                if (!_identifierUtil.IsValidIdentifier(baseName))
                {
                    baseName = ModelConstants.DefaultEntityContainerName;
                }
            }

            return baseName;
        }

        private void disallowSensitiveInfoButton_CheckedChanged(object sender, EventArgs e)
        {
            AllowSensitiveInfoChanged(!disallowSensitiveInfoButton.Checked);
        }

        private void allowSensitiveInfoButton_CheckedChanged(object sender, EventArgs e)
        {
            AllowSensitiveInfoChanged(allowSensitiveInfoButton.Checked);
        }

        private void AllowSensitiveInfoChanged(bool allowSensitiveInfo)
        {
            var maskedConnectionString = _dataConnection.DisplayConnectionString;
            if (!allowSensitiveInfo)
            {
                maskedConnectionString = _dataConnection.SafeConnectionString;
            }

            textBoxConnectionString.Text = GetTextBoxConnectionStringValue(_dataProviderManager, _dataConnection.Provider, maskedConnectionString);

            Wizard.OnValidationStateChanged(this);
        }

        private void SetDataConnection(IVsDataConnection dataConnection)
        {
            Debug.Assert(dataConnection != null, "should not be called with null dataConnection");
            _dataConnection = dataConnection;

            textBoxConnectionString.Text = GetTextBoxConnectionStringValue(
                _dataProviderManager, _dataConnection.Provider, _dataConnection.DisplayConnectionString);

            textBoxAppConfigConnectionName.Text = ModelBuilderWizardForm.WizardMode.PerformAllFunctionality == Wizard.Mode
                ? GetDefaultAppConfigEntryName(_dataConnection)
                : Wizard.ModelBuilderSettings.AppConfigConnectionPropertyName;

            if (ContainsSensitiveData(_dataConnection))
            {
                sensitiveInfoTextBox.Enabled = true;
                allowSensitiveInfoButton.Checked = false;
                allowSensitiveInfoButton.Enabled = true;
                disallowSensitiveInfoButton.Checked = false;
                disallowSensitiveInfoButton.Enabled = true;
            }
            else
            {
                sensitiveInfoTextBox.Enabled = false;
                allowSensitiveInfoButton.Checked = false;
                allowSensitiveInfoButton.Enabled = false;
                disallowSensitiveInfoButton.Checked = false;
                disallowSensitiveInfoButton.Enabled = false;
            }
        }

        private void NewDataSourceSelected()
        {
            var selectedItem = (DataSourceComboBoxItem)dataSourceComboBox.SelectedItem;

            // If the SelectedIndexChanged event is fired but there are no items in the combobox 
            // the selected index is -1 and the selected item is null. To avoid the NRE and VS crash 
            // we just don't do anything in this case. Another way to trigger the NRE was to go back to 
            // the first page and return to the db config page when the combobox was empty
            if (selectedItem == null)
            {
                return;
            }

            SetDataConnection(selectedItem.GetDataConnection(_dataConnectionManager));

            // new data source has been selected, we should invalidate following pages
            Wizard.InvalidateFollowingPages();

            Wizard.OnValidationStateChanged(this);
        }

        private void checkBoxSaveInAppConfig_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSaveInAppConfig.Enabled
                && checkBoxSaveInAppConfig.Checked
                && ModelBuilderWizardForm.WizardMode.PerformDatabaseConfigAndSelectTables != Wizard.Mode
                && ModelBuilderWizardForm.WizardMode.PerformDatabaseConfigAndDBGenSummary != Wizard.Mode
                && ModelBuilderWizardForm.WizardMode.PerformDBGenSummaryOnly != Wizard.Mode)
            {
                textBoxAppConfigConnectionName.Enabled = true;
            }
            else
            {
                textBoxAppConfigConnectionName.Enabled = false;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void dataSourceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                NewDataSourceSelected();
            }
            catch (Exception ex)
            {
                VsUtils.ShowErrorDialog(ex.Message);
            }
        }

        private class DataSourceComboBoxItem
        {
            private IVsDataConnection _dataConnection;
            private readonly Guid _provider;
            private readonly string _connectionString;
            private readonly bool _isConnectionStringEncrypted;
            private readonly string _displayName;

            public DataSourceComboBoxItem(string displayName, IVsDataConnection connection)
            {
                Debug.Assert(null != connection, "connection should not be null");

                _displayName = displayName;
                _dataConnection = connection;
            }

            public DataSourceComboBoxItem(string displayName, Guid provider, string connectionString, bool encrypted)
            {
                _displayName = displayName;
                _provider = provider;
                _connectionString = connectionString;
                _isConnectionStringEncrypted = encrypted;
            }

            public IVsDataConnection GetDataConnection(IVsDataConnectionManager dataConnectionManager)
            {
                if (null == _dataConnection)
                {
                    Debug.Assert(null != dataConnectionManager, "dataConnectionManager should not be null");

                    _dataConnection = dataConnectionManager.GetConnection(_provider, _connectionString, _isConnectionStringEncrypted);
                }

                return _dataConnection;
            }

            public void ResetDataConnection(IVsDataConnection dataConnection)
            {
                // do not need to reset _provider or _isConnectionStringEncrypted since these are the same
                // as original connection
                Debug.Assert(null != dataConnection, "dataConnection should not be null");
                _dataConnection = dataConnection;
            }

            public override string ToString()
            {
                return _displayName;
            }
        }

        private void WizardPageDbConfig_Resize(object sender, EventArgs e)
        {
            dataSourceComboBox.Width = Math.Max(newDBConnectionButton.Left - dataSourceComboBox.Left - 7, 200);
            if (textBoxConnectionString.Top + textBoxConnectionString.Height + textBoxConnectionString.Margin.Bottom >
                checkBoxSaveInAppConfig.Top - checkBoxSaveInAppConfig.Margin.Top)
            {
                textBoxConnectionString.Height = checkBoxSaveInAppConfig.Top - checkBoxSaveInAppConfig.Margin.Top
                                                 - textBoxConnectionString.Top - textBoxConnectionString.Margin.Top;
            }
        }
    }

    [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
    [Serializable]
    internal class FileCopyException : Exception
    {
        internal FileCopyException(string msg)
            : base(msg)
        {
        }

        protected FileCopyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
