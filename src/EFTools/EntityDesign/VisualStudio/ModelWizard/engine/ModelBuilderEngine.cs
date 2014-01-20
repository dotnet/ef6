// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using System.Xml;
    using System.Xml.Linq;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb;
    using Microsoft.Data.Entity.Design.VisualStudio.Data.Sql;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VisualStudio.UI;
    using Microsoft.VisualStudio.Data.Core;
    using Microsoft.VisualStudio.Data.Services;
    using Command = Microsoft.Data.Entity.Design.Model.Commands.Command;
    using Resources = Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties.Resources;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal abstract class ModelBuilderEngine
    {
        private static readonly XmlWriterSettings WriterSettings = new XmlWriterSettings();

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ModelBuilderEngine()
        {
            WriterSettings.Indent = true;
            WriterSettings.ConformanceLevel = ConformanceLevel.Fragment;
            // this is needed for correct indenting
            WriterSettings.NewLineChars += "      ";
        }

        protected abstract void AddErrors(IEnumerable<EdmSchemaError> errors);
        internal abstract IEnumerable<EdmSchemaError> Errors { get; }

        // <summary>
        //     This is the XDocument of the model in memory.  No assumptions should be made that it exists on disk.
        // </summary>
        internal abstract XDocument Model { get; }

        protected abstract void InitializeModelContents(Version targetSchemaVersion);

        // <summary>
        //     Generates EDMX file.
        // </summary>
        public void GenerateModel(ModelBuilderSettings settings)
        {
            if (settings.GenerationOption == ModelGenerationOption.GenerateFromDatabase
                && String.IsNullOrEmpty(settings.DesignTimeConnectionString))
            {
                throw new ArgumentOutOfRangeException(Resources.Engine_EmptyConnStringErrorMsg);
            }

            InitializeModelContents(settings.TargetSchemaVersion);

            GenerateModel(new EdmxHelper(Model), settings, new VSModelBuilderEngineHostContext(settings));
        }

        // internal virtual to allow mocking
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal virtual void GenerateModel(EdmxHelper edmxHelper, ModelBuilderSettings settings, ModelBuilderEngineHostContext hostContext)
        {
            var generatingModelWatch = Stopwatch.StartNew();

            // Clear out the ModelGenErrorCache before ModelGen begins
            PackageManager.Package.ModelGenErrorCache.RemoveErrors(settings.ModelPath);

            var errors = new List<EdmSchemaError>();
            try
            {
                var storeModelNamespace = GetStoreNamespace(settings);
                var model = GenerateModels(storeModelNamespace, settings, errors);

                edmxHelper.UpdateEdmxFromModel(model, storeModelNamespace, settings.ModelNamespace, errors);

                // load extensions that want to update model after the wizard has run. 
                hostContext.DispatchToModelGenerationExtensions();

                UpdateDesignerInfo(edmxHelper, settings);

                hostContext.LogMessage(
                    FormatMessage(
                    errors.Any()
                        ? Resources.Engine_ModelGenErrors
                        : Resources.Engine_ModelGenSuccess,
                    Path.GetFileName(settings.ModelPath)));

                if (errors.Any())
                {
                    PackageManager.Package.ModelGenErrorCache.AddErrors(settings.ModelPath, errors);
                }
            }
            catch (Exception e)
            {
                hostContext.LogMessage(FormatMessage(Resources.Engine_ModelGenException, e));
            }

            generatingModelWatch.Stop();

            hostContext.LogMessage(FormatMessage(Resources.LoadingDBMetadataTimeMsg, settings.LoadingDBMetatdataTime));
            hostContext.LogMessage(FormatMessage(Resources.GeneratingModelTimeMsg, generatingModelWatch.Elapsed));
        }

        // internal virtual to allow mocking
        internal virtual DbModel GenerateModels(string storeModelNamespace, ModelBuilderSettings settings, List<EdmSchemaError> errors)
        {
            return new ModelGenerator(settings, storeModelNamespace).GenerateModel(errors);
        }

        private static string FormatMessage(string resourcestringName, params object[] args)
        {
            return
                String.Format(
                    CultureInfo.CurrentCulture,
                    resourcestringName,
                    args);
        }

        private static string GetStoreNamespace(ModelBuilderSettings settings)
        {
            return
                string.IsNullOrEmpty(settings.StorageNamespace)
                    ? String.Format(
                        CultureInfo.CurrentCulture,
                        Resources.SelectTablesPage_DefaultStorageNamespaceFormat,
                        settings.ModelNamespace)
                    : settings.StorageNamespace;
        }

        protected virtual void UpdateDesignerInfo(EdmxHelper edmxHelper, ModelBuilderSettings settings)
        {
            Debug.Assert(edmxHelper != null);

            edmxHelper.UpdateDesignerOptionProperty(
                OptionsDesignerInfo.AttributeEnablePluralization, settings.UsePluralizationService);
            edmxHelper.UpdateDesignerOptionProperty(
                OptionsDesignerInfo.AttributeIncludeForeignKeysInModel, settings.IncludeForeignKeysInModel);
            edmxHelper.UpdateDesignerOptionProperty(
                OptionsDesignerInfo.AttributeUseLegacyProvider, settings.UseLegacyProvider);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static void SetupSettingsAndModeForDbPages(
            IServiceProvider serviceProvider,
            Project project,
            EFArtifact artifact,
            bool checkDatabaseConnection,
            ModelBuilderWizardForm.WizardMode noConnectionMode,
            ModelBuilderWizardForm.WizardMode existingConnectionMode,
            out ModelBuilderWizardForm.WizardMode startMode,
            out ModelBuilderSettings settings)
        {
            var conceptualEntityModel = artifact.ConceptualModel();
            Debug.Assert(conceptualEntityModel != null, "Null Conceptual Entity Model");
            var entityContainer = conceptualEntityModel.FirstEntityContainer as ConceptualEntityContainer;
            Debug.Assert(entityContainer != null, "Null Conceptual Entity Container");
            var entityContainerName = entityContainer.LocalName.Value;

            // set up ModelBuilderSettings for startMode=noConnectionMode
            startMode = noConnectionMode;
            settings = new ModelBuilderSettings();
            var appType = VsUtils.GetApplicationType(serviceProvider, project);
            settings.VSApplicationType = appType;
            settings.AppConfigConnectionPropertyName = entityContainerName;
            settings.Artifact = artifact;
            settings.UseLegacyProvider = ModelHelper.GetDesignerPropertyValueFromArtifactAsBool(
                OptionsDesignerInfo.ElementName,
                OptionsDesignerInfo.AttributeUseLegacyProvider,
                OptionsDesignerInfo.UseLegacyProviderDefault,
                artifact);
            settings.TargetSchemaVersion = artifact.SchemaVersion;
            settings.Project = project;
            settings.ModelPath = artifact.Uri.LocalPath;

            // Get the provider manifest token from the existing SSDL.
            // We don't want to attempt to get it from provider services since this requires a connection
            // which will severely impact the performance of Model First in disconnected scenarios.
            settings.ProviderManifestToken = DatabaseGenerationEngine.GetProviderManifestTokenDisconnected(artifact);

            // Change startMode and settings appropriately depending on whether there is an existing connection string and whether we can/should connect
            // to the database
            var connectionString = ConnectionManager.GetConnectionStringObject(project, entityContainerName);
            if (connectionString != null)
            {
                var ecsb = connectionString.Builder;
                var runtimeProviderName = ecsb.Provider;
                var runtimeProviderConnectionString = ecsb.ProviderConnectionString;
                var designTimeProviderConnectionString = connectionString.GetDesignTimeProviderConnectionString(project);
                var initialCatalog = String.Empty;

                if (checkDatabaseConnection)
                {
                    // This path will check to make sure that we can connect to an existing database before changing the start mode to 'existingConnection'
                    IVsDataConnection dataConnection = null;
                    try
                    {
                        var dataConnectionManager = serviceProvider.GetService(typeof(IVsDataConnectionManager)) as IVsDataConnectionManager;
                        Debug.Assert(dataConnectionManager != null, "Could not find IVsDataConnectionManager");

                        var dataProviderManager = serviceProvider.GetService(typeof(IVsDataProviderManager)) as IVsDataProviderManager;
                        Debug.Assert(dataProviderManager != null, "Could not find IVsDataProviderManager");

                        if (dataConnectionManager != null
                            && dataProviderManager != null)
                        {
                            // this will either get an existing connection or attempt to create a new one
                            dataConnection = DataConnectionUtils.GetDataConnection(
                                dataConnectionManager,
                                dataProviderManager,
                                connectionString.DesignTimeProviderInvariantName,
                                designTimeProviderConnectionString);
                            Debug.Assert(
                                dataConnection != null,
                                "Could not find the IVsDataConnection; an exception should have been thrown if this was the case");
                            if (dataConnection != null)
                            {
                                VsUtils.EnsureProvider(runtimeProviderName, settings.UseLegacyProvider, project, serviceProvider);

                                if (CanCreateAndOpenConnection(
                                    new StoreSchemaConnectionFactory(),
                                    runtimeProviderName,
                                    connectionString.DesignTimeProviderInvariantName,
                                    designTimeProviderConnectionString))
                                {
                                    startMode = existingConnectionMode;
                                    initialCatalog = DataConnectionUtils.GetInitialCatalog(dataProviderManager, dataConnection);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // do nothing - we will go to WizardPageDbConfig which is
                        // what we want if the DB connection fails
                    }
                    finally
                    {
                        // Close the IVsDataConnection
                        if (dataConnection != null)
                        {
                            try
                            {
                                dataConnection.Close();
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                else
                {
                    // This path will just parse the existing connection string in order to change the start mode. This is ideal for features
                    // that do not need a database connection -- the information in the connection string is enough.
                    startMode = existingConnectionMode;
                    initialCatalog = DataConnectionUtils.GetInitialCatalog(
                        connectionString.DesignTimeProviderInvariantName, designTimeProviderConnectionString);
                }

                if (startMode == existingConnectionMode)
                {
                    // the invariant name and connection string came from app.config, so they are "runtime" invariant names and not "design-time"
                    // (Note: it is OK for InitialCatalog to be null at this stage e.g. from a provider who do not support the concept of Initial Catalog)
                    settings.SetInvariantNamesAndConnectionStrings(
                        serviceProvider,
                        project,
                        runtimeProviderName,
                        runtimeProviderConnectionString,
                        runtimeProviderConnectionString,
                        false);
                    settings.InitialCatalog = initialCatalog;
                    settings.AppConfigConnectionPropertyName = entityContainerName;
                    settings.SaveConnectionStringInAppConfig = false;

                    VsUtils.EnsureProvider(runtimeProviderName, settings.UseLegacyProvider, project, serviceProvider);
                }
            }
        }

        // internal for testing
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static bool CanCreateAndOpenConnection(
            StoreSchemaConnectionFactory connectionFactory, string providerInvariantName, string designTimeInvariantName,
            string designTimeConnectionString)
        {
            Debug.Assert(connectionFactory != null, "connectionFactory != null");
            Debug.Assert(
                !string.IsNullOrWhiteSpace(designTimeInvariantName),
                "designTimeInvariantName must not be null or empty");
            Debug.Assert(
                !string.IsNullOrWhiteSpace(designTimeConnectionString),
                "designTimeConnectionString must not be null or empty");

            EntityConnection entityConnection = null;
            try
            {
                // attempt to create a DbConnection using the provider connection string we have. This will
                // throw an exception if the connection cannot be made, for example, if the credentials aren't
                // set. This has to be done using DDEX-based APIs since the SchemaGenerator is based off of
                // DbConnection, and DDEX will save the password whereas DbConnection will not.
                Version _;
                entityConnection = connectionFactory.Create(
                    DependencyResolver.Instance,
                    providerInvariantName,
                    designTimeConnectionString,
                    EntityFrameworkVersion.Latest,
                    out _);
                entityConnection.Open();
            }
            catch
            {
                return false;
            }
            finally
            {
                // Close the EntityConnection
                if (entityConnection != null)
                {
                    VsUtils.SafeCloseDbConnection(entityConnection, designTimeInvariantName, designTimeConnectionString);
                }
            }

            return true;
        }

        // <summary>
        //     Helper method used to show the ProgressDialog and collect return type information about sprocs
        // </summary>
        // <param name="owner">Window that owns the dialog</param>
        // <param name="newFunctionEntries">list of Functions for which we should collect information</param>
        // <param name="modelBuilderSettings">ModelBuilderSettings where collected information will be stored</param>
        internal static DialogResult ShowProgressDialog(
            IWin32Window owner, IList<EntityStoreSchemaFilterEntry> newFunctionEntries, ModelBuilderSettings modelBuilderSettings)
        {
            var args = new GatherAndReturnSprocInfo(newFunctionEntries, modelBuilderSettings);
            using (var pd = new ProgressDialog(
                Design.Resources.RetrievingSprocReturnTypeProgressDialogTitle,
                Design.Resources.RetrievingSprocReturnTypeProgressDialogDescription,
                Design.Resources.RetrievingSprocReturnTypeProgressDialogInitialStatus, GatherAndStoreSchemaProcedureInformation, args))
            {
                return pd.ShowDialog(owner);
            }
        }

        // <summary>
        //     This method run on the background thread behind a ProgressDialog.
        //     For each filter entry in newFunctionFilterEntries log on to the database, gather the return type
        //     information and store the result in ModelBuilderSettings
        // </summary>
        internal static object GatherAndStoreSchemaProcedureInformation(BackgroundWorker worker, DoWorkEventArgs e)
        {
            if (null == worker)
            {
                throw new ProgressDialogException(
                    string.Format(
                        CultureInfo.CurrentCulture, Design.Resources.RetrievingSprocReturnTypeErrorMessage, "null BackgroundWorker"));
            }

            var arg = e.Argument as GatherAndReturnSprocInfo;
            if (null == arg)
            {
                throw new ProgressDialogException(
                    string.Format(
                        CultureInfo.CurrentCulture, Design.Resources.RetrievingSprocReturnTypeErrorMessage, "null DoWorkEventArgs"));
            }

            var newFunctionFilterEntries = arg.NewFunctionEntries;
            if (null == newFunctionFilterEntries)
            {
                throw new ProgressDialogException(
                    string.Format(
                        CultureInfo.CurrentCulture, Design.Resources.RetrievingSprocReturnTypeErrorMessage, "null newFunctionFilterEntries"));
            }

            var modelBuilderSettings = arg.ModelBuilderSettings;
            if (null == modelBuilderSettings)
            {
                throw new ProgressDialogException(
                    string.Format(
                        CultureInfo.CurrentCulture, Design.Resources.RetrievingSprocReturnTypeErrorMessage, "null modelBuilderSettings"));
            }

            // clear map first (if user has clicked backwards and forwards between wizard pages this can already be populated)
            modelBuilderSettings.NewFunctionSchemaProcedures.Clear();

            // now set-up Dictionary with all EntityStoreSchemaFilterEntry keys but all pointing to null values
            // if the process is interrupted then those that still have null values represent sprocs which
            // need to be deleted
            foreach (var entry in newFunctionFilterEntries)
            {
                modelBuilderSettings.NewFunctionSchemaProcedures.Add(entry, null);
            }

            PopulateNewFunctionSchemaProcedures(
                modelBuilderSettings.NewFunctionSchemaProcedures,
                modelBuilderSettings.DesignTimeProviderInvariantName,
                modelBuilderSettings.DesignTimeConnectionString,
                e,
                worker);

            return null;
        }

        internal static void PopulateNewFunctionSchemaProcedures(
            Dictionary<EntityStoreSchemaFilterEntry, IDataSchemaProcedure> newFunctionSchemaProcedureMap,
            string designTimeProviderInvariantName,
            string designTimeProviderConnectionString,
            DoWorkEventArgs e = null,
            BackgroundWorker worker = null,
            int startingAmountOfProgressBar = 0,
            int amountOfProgressBarGiven = 100)
        {
            // set up database connection
            var dataConnectionManager = Services.ServiceProvider.GetService(typeof(IVsDataConnectionManager)) as IVsDataConnectionManager;
            Debug.Assert(dataConnectionManager != null, "Could not find IVsDataConnectionManager");

            var dataProviderManager = Services.ServiceProvider.GetService(typeof(IVsDataProviderManager)) as IVsDataProviderManager;
            Debug.Assert(dataProviderManager != null, "Could not find IVsDataProviderManager");

            IVsDataConnection dataConnection = null;
            if (null != dataConnectionManager
                && null != dataProviderManager)
            {
                dataConnection = DataConnectionUtils.GetDataConnection(
                    dataConnectionManager, dataProviderManager, designTimeProviderInvariantName, designTimeProviderConnectionString);
            }
            if (null == dataConnection)
            {
                throw new ProgressDialogException(
                    string.Format(
                        CultureInfo.CurrentCulture, Design.Resources.RetrievingSprocReturnTypeErrorMessage, "null IVsDataConnection"));
            }

            // open the database connection and collect info for each Function
            try
            {
                dataConnection.Open();
                var dataSchemaServer = new DataSchemaServer(dataConnection);

                // now loop over all entries adding return type information
                var numFunctionFilterEntries = newFunctionSchemaProcedureMap.Count;
                var numFunctionFilterEntryCurrent = 0;
                foreach (var entry in newFunctionSchemaProcedureMap.Keys.ToList())
                {
                    numFunctionFilterEntryCurrent++;
                    if (worker != null
                        && e != null
                        && worker.CancellationPending)
                    {
                        // user requested interrupt of this process
                        e.Cancel = true;
                    }
                    else
                    {
                        if (worker != null
                            && worker.WorkerReportsProgress)
                        {
                            // report progress so ProgressDialog can update its status
                            var percentCompleted = startingAmountOfProgressBar +
                                                   ((int)
                                                    (((numFunctionFilterEntryCurrent - 1) / (float)numFunctionFilterEntries)
                                                     * amountOfProgressBarGiven));
                            var userState = new ProgressDialogUserState();
                            userState.NumberIterations = numFunctionFilterEntries;
                            userState.CurrentIteration = numFunctionFilterEntryCurrent;
                            userState.CurrentStatusMessage = string.Format(
                                CultureInfo.CurrentCulture,
                                Design.Resources.RetrievingSprocReturnTypeInfoMessage,
                                numFunctionFilterEntryCurrent,
                                numFunctionFilterEntries,
                                entry.Schema,
                                entry.Name);
                            worker.ReportProgress(percentCompleted, userState);
                        }

                        // now retrieve and store the return type information
                        var schemaProcedure = dataSchemaServer.GetProcedureOrFunction(entry.Schema, entry.Name);
                        Debug.Assert(
                            null == newFunctionSchemaProcedureMap[entry],
                            "This entry has already been processed, Schema = " + entry.Schema + ", Name = " + entry.Name);
                        newFunctionSchemaProcedureMap[entry] = schemaProcedure;
                    }
                }
            }
            finally
            {
                if (null != dataConnection)
                {
                    dataConnection.Close();
                }
            }
        }

        // <summary>
        //     Processes the sproc return type information stored in newFunctionSchemaProceduresMap to
        //     add commands which create matching FunctionImports or delete Functions as necessary
        // </summary>
        // <param name="artifact"></param>
        // <param name="newFunctionSchemaProceduresMap">
        //     map of all processed EntityStoreSchemaFilterEntry for Functions to
        //     their IDataSchemaProcedure (where data was collected) or null (where data was not collected because the data
        //     collection process was interrupted)
        // </param>
        // <param name="commands">list of commands to which to add the create or delete commands</param>
        // <param name="shouldCreateComposableFunctionImports">whether to create FunctionImports for composable Functions</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        internal static void ProcessStoredProcedureReturnTypeInformation(
            EFArtifact artifact,
            Dictionary<EntityStoreSchemaFilterEntry, IDataSchemaProcedure> newFunctionSchemaProceduresMap, IList<Command> commands,
            bool shouldCreateComposableFunctionImports)
        {
            if (null == artifact)
            {
                Debug.Fail("null artifact");
                return;
            }

            if (null == newFunctionSchemaProceduresMap)
            {
                Debug.Fail("Null newFunctionSchemaProceduresMap for artifact " + artifact.Uri);
                return;
            }

            var sem = artifact.StorageModel();
            if (null == sem)
            {
                Debug.Fail("Null StorageEntityModel for artifact " + artifact.Uri);
                return;
            }

            var storageEntityContainerName = sem.FirstEntityContainer.LocalName.Value;
            if (string.IsNullOrWhiteSpace(storageEntityContainerName))
            {
                Debug.Fail("Null or whitespace StorageEntityContainerName for artifact " + artifact.Uri);
                return;
            }

            foreach (var entry in newFunctionSchemaProceduresMap.Keys)
            {
                var schemaProcedure = newFunctionSchemaProceduresMap[entry];
                Command cmd = null;
                if (null == schemaProcedure)
                {
                    // schemaProcedure information was not collected - so delete the Function
                    var dbObj = DatabaseObject.CreateFromEntityStoreSchemaFilterEntry(entry, storageEntityContainerName);
                    var func = ModelHelper.FindFunction(sem, dbObj);
                    Debug.Assert(func != null, "Could not find Function to delete matching Database Object " + dbObj.ToString());
                    if (null != func)
                    {
                        cmd = func.GetDeleteCommand();
                    }
                }
                else
                {
                    cmd = new CreateMatchingFunctionImportCommand(schemaProcedure, shouldCreateComposableFunctionImports);
                }

                if (null != cmd)
                {
                    commands.Add(cmd);
                }
            }
        }
    }

    // <summary>
    //     Helper class to pass information to background thread in ProgressDialog
    // </summary>
    internal class GatherAndReturnSprocInfo
    {
        private readonly IList<EntityStoreSchemaFilterEntry> _newFunctionEntries;
        private readonly ModelBuilderSettings _modelBuilderSettings;

        internal GatherAndReturnSprocInfo(IList<EntityStoreSchemaFilterEntry> newFunctionEntries, ModelBuilderSettings modelBuilderSettings)
        {
            Debug.Assert(null != newFunctionEntries, "newFunctionEntries should not be null");
            Debug.Assert(null != modelBuilderSettings, "modelBuilderSettings should not be null");
            _newFunctionEntries = newFunctionEntries;
            _modelBuilderSettings = modelBuilderSettings;
        }

        internal IList<EntityStoreSchemaFilterEntry> NewFunctionEntries
        {
            get { return _newFunctionEntries; }
        }

        internal ModelBuilderSettings ModelBuilderSettings
        {
            get { return _modelBuilderSettings; }
        }
    }
}
