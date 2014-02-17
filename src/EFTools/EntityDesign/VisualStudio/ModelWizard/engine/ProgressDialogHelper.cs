// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb;
    using Microsoft.Data.Entity.Design.VisualStudio.Data.Sql;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VisualStudio.UI;
    using Microsoft.VisualStudio.Data.Core;
    using Microsoft.VisualStudio.Data.Services;

    internal static class ProgressDialogHelper
    {
        // <summary>
        //     Helper method used to show the ProgressDialog and collect return type information about sprocs
        // </summary>
        // <param name="owner">Window that owns the dialog</param>
        // <param name="newFunctionEntries">list of Functions for which we should collect information</param>
        // <param name="modelBuilderSettings">ModelBuilderSettings where collected information will be stored</param>
        public static DialogResult ShowProgressDialog(
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
        private static object GatherAndStoreSchemaProcedureInformation(BackgroundWorker worker, DoWorkEventArgs e)
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
        private static void PopulateNewFunctionSchemaProcedures(
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
}