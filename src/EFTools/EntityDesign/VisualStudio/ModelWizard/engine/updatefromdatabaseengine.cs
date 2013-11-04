// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml.Linq;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Extensibility;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.UI.Views.Explorer;
    using Microsoft.Data.Entity.Design.VisualStudio.Model;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.XmlDesignerBase.Model.StandAlone;
    using Command = Microsoft.Data.Entity.Design.Model.Commands.Command;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal static class UpdateFromDatabaseEngine
    {
        /// <summary>
        ///     Updates the EDMX file based on the Database changes
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal static void UpdateModelFromDatabase(EntityDesignArtifact artifact)
        {
            VsUtils.EnsureProvider(artifact);

            var project = VSHelpers.GetProjectForDocument(artifact.Uri.LocalPath, PackageManager.Package);
            var serviceProvider = Services.ServiceProvider;

            // set up ModelBuilderSettings for startMode=PerformDatabaseConfigAndSelectTables
            ModelBuilderWizardForm.WizardMode startMode;
            ModelBuilderSettings settings;
            ModelBuilderEngine.SetupSettingsAndModeForDbPages(
                serviceProvider, project, artifact, true,
                ModelBuilderWizardForm.WizardMode.PerformDatabaseConfigAndSelectTables,
                ModelBuilderWizardForm.WizardMode.PerformSelectTablesOnly, out startMode, out settings);
            settings.WizardKind = WizardKind.UpdateModel;

            // use existing storage namespace as new storage namespace
            if (null != artifact.StorageModel()
                && null != artifact.StorageModel().Namespace
                && !string.IsNullOrEmpty(artifact.StorageModel().Namespace.Value))
            {
                settings.StorageNamespace = artifact.StorageModel().Namespace.Value;
            }

            // use existing model namespace as new model namespace (this only affects the temporary
            // artifact but there is a situation where the C-side EntityContainer has been given the
            // same name as the default model namespace where not setting this causes the temporary
            // artifact to be unreadable because of symbol clashes)
            if (null != artifact.ConceptualModel()
                && null != artifact.ConceptualModel().Namespace
                && !string.IsNullOrEmpty(artifact.ConceptualModel().Namespace.Value))
            {
                settings.ModelNamespace = artifact.ConceptualModel().Namespace.Value;
            }
            
            settings.ModelBuilderEngine = new UpdateModelFromDatabaseModelBuilderEngine();

            // call the ModelBuilderWizardForm
            var form = new ModelBuilderWizardForm(serviceProvider, settings, startMode);

            try
            {
                form.Start();
            }
            catch (Exception e)
            {
                VsUtils.ShowErrorDialog(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.ModelObjectItemWizard_UnexpectedExceptionHasOccurred,
                        e.Message));
                return;
            }

            // if Wizard was cancelled or the user hit 'X' to close window
            // no need for any further action
            if (form.WizardCancelled
                || !form.WizardFinished)
            {
                return;
            }

            // Update the app. or web.config, register build providers etc
            ConfigFileUtils.UpdateConfig(settings);

            // clear all previous errors for this document first
            ErrorListHelper.ClearErrorsForDocAcrossLists(settings.Artifact.Uri);

            // log any errors that occurred during model-gen to the error list
            ErrorListHelper.LogUpdateModelWizardErrors(settings.ModelBuilderEngine.Errors, settings.Artifact.Uri.LocalPath);

            // use form.ModelBuilderSettings to look at accumulated info and
            // take appropriate action
            var editingContext = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(artifact.Uri);
            var shouldReloadArtifact = ProcessAccumulatedInfo(editingContext, artifact, settings);

            // If database was configured, add DbContext templates
            if (startMode == ModelBuilderWizardForm.WizardMode.PerformDatabaseConfigAndSelectTables)
            {
                var edmxItem = VsUtils.GetProjectItemForDocument(artifact.Uri.LocalPath, serviceProvider);
                new DbContextCodeGenerator().AddDbContextTemplates(edmxItem, settings.UseLegacyProvider);
            }

            // We can reload only after we added EF references to the project otherwise we would get a watermark
            // saying that the schema version does not match the referenced EF version which would not be true.
            // If we reload becuase there was an extension that potentially modified the artifact then it does not matter.
            if (shouldReloadArtifact)
            {
                artifact.ReloadArtifact();
                artifact.IsDirty = true;
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static bool ProcessAccumulatedInfo(
            EditingContext editingContext, EntityDesignArtifact existingArtifact, ModelBuilderSettings settings)
        {
            var schemaVersionChanged = existingArtifact.SchemaVersion != settings.TargetSchemaVersion;

            EntityDesignModelManager tempModelManager = null;
            FileInfo tempEdmxFile = null;
            Uri tempEdmxFileUri = null;

            try
            {
                // set up new temporary ModelManager
                // NOTE:  use an EFArtifact with VSArtifactSet.  This is so we get the in-vs behavior of artifact sets, 
                // but don't rely on VS loading the model into the RDT, etc..
                tempModelManager = new EntityDesignModelManager(new EFArtifactFactory(), new VSArtifactSetFactory());

                tempEdmxFile = ConstructTempEdmxFile(settings);
                tempEdmxFileUri = new Uri(tempEdmxFile.FullName, UriKind.Absolute);
                var tempArtifactBasedOnDatabase = tempModelManager.GetNewOrExistingArtifact(tempEdmxFileUri, new VanillaXmlModelProvider());

                // if a model generation extension has changed the model, ensure that it is 
                // valid before we start to process it
                if (settings.HasExtensionChangedModel)
                {
                    ValidateArtifact(tempModelManager, tempArtifactBasedOnDatabase, WizardKind.Generate);
                }

                // Note: later we want the diagram shapes and connectors to be created in the current active diagram
                // so set TransactionContext appropriately.
                EfiTransactionContext transactionContext = null;
                var contextItem = editingContext.Items.GetValue<DiagramManagerContextItem>();
                if (contextItem != null
                    && contextItem.DiagramManager != null)
                {
                    var activeDiagram = contextItem.DiagramManager.ActiveDiagram;
                    if (activeDiagram != null)
                    {
                        transactionContext = new EfiTransactionContext();
                        transactionContext.Add(
                            EfiTransactionOriginator.TransactionOriginatorDiagramId, new DiagramContextItem(activeDiagram.DiagramId));
                    }
                }

                // clear search if active (Note: in Model.Tests it is OK for below to be null)
                var explorerInfo = editingContext.Items.GetValue<ExplorerWindow.ExplorerInfo>();
                if (explorerInfo != null)
                {
                    var explorerFrame = explorerInfo._explorerFrame;
                    if (explorerFrame != null)
                    {
                        if (explorerFrame.SearchIsActive)
                        {
                            explorerFrame.ResetSearchCommand.Execute(null);
                        }
                    }
                }

                var cpc = new CommandProcessorContext(
                    editingContext, EfiTransactionOriginator.UpdateModelFromDatabaseId,
                    Resources.Tx_UpdateModelFromDatabase, null, transactionContext);

                if (schemaVersionChanged)
                {
                    // changing namespaces must be done in a separate transaction otherwise XmlEditor
                    // will not pick-up changes made to xml after namespaces are changed 
                    CommandProcessor.InvokeSingleCommand(
                        cpc,
                        new RetargetXmlNamespaceCommand(existingArtifact, settings.TargetSchemaVersion));
                }

                // Update the existing artifact based on tempArtifactBasedOnDatabase
                var commands = new List<Command>();
                var cmd = new UpdateModelFromDatabaseCommand(tempArtifactBasedOnDatabase);
                commands.Add(cmd);

                // set up our post event to clear out the error list
                cmd.PostInvokeEvent +=
                    (o, e) =>
                        {
                            var errorList =
                                ErrorListHelper.GetSingleDocErrorList(e.CommandProcessorContext.Artifact.Uri);
                            if (errorList != null)
                            {
                                errorList.Clear();
                            }
                        };

                DesignerInfo designerInfo;
                if (existingArtifact.DesignerInfo().TryGetDesignerInfo(OptionsDesignerInfo.ElementName, out designerInfo))
                {
                    var optionsDesignerInfo = designerInfo as OptionsDesignerInfo;
                    Debug.Assert(optionsDesignerInfo != null, "expected non-null optionsDesignerInfo");
                    if (optionsDesignerInfo != null)
                    {
                        // pluralization checkbox
                        AddUpdateDesignerPropertyCommand(
                            optionsDesignerInfo.CheckPluralizationInWizard,
                            OptionsDesignerInfo.AttributeEnablePluralization,
                            settings.UsePluralizationService, optionsDesignerInfo, commands);

                        // include FKs in model checkbox
                        AddUpdateDesignerPropertyCommand(
                            optionsDesignerInfo.CheckIncludeForeignKeysInModel,
                            OptionsDesignerInfo.AttributeIncludeForeignKeysInModel,
                            settings.IncludeForeignKeysInModel, optionsDesignerInfo, commands);

                        // ensure UseLegacyProvider is set
                        AddUpdateDesignerPropertyCommand(
                            optionsDesignerInfo.UseLegacyProvider,
                            OptionsDesignerInfo.AttributeUseLegacyProvider,
                            settings.UseLegacyProvider, optionsDesignerInfo, commands);
                    }
                }

                // create a new FunctionImport for every new Function created (whether composable or not)
                // (or delete Functions if ProgressDialog did not finish successfully)
                // Note: this must take place as a DelegateCommand as ProcessStoredProcedureReturnTypeInformation()
                // can depend on finding the existing Functions to delete. And it won't find them until the
                // ReplaceSsdlCommand within UpdateModelFromDatabaseCommand has executed.
                var createMatchingFunctionImportsDelegateCommand = new DelegateCommand(
                    () =>
                        {
                            var functionImportCommands = new List<Command>();
                            ModelBuilderEngine.ProcessStoredProcedureReturnTypeInformation(
                                existingArtifact, settings.NewFunctionSchemaProcedures, functionImportCommands, true);

                            if (functionImportCommands.Count > 0)
                            {
                                new CommandProcessor(cpc, functionImportCommands)
                                    .Invoke();
                            }
                        });
                commands.Add(createMatchingFunctionImportsDelegateCommand);

                // if needed, create a command to dispatch any extensions
                if (EscherExtensionPointManager.LoadModelGenerationExtensions().Length > 0)
                {
                    var dispatchCommand = new DispatchToExtensionsCommand(settings);
                    commands.Add(dispatchCommand);
                }

                // do all the work here in one transaction
                new CommandProcessor(cpc, commands)
                    .Invoke();

                // if an extension has changed the model, do a full reload
                if (schemaVersionChanged || settings.HasExtensionChangedModel)
                {
                    return true;
                }
                else
                {
                    // reset the is-designer safe flag - this can be set incorrectly when the document is reloaded after ssdl has been updated,
                    // but csdl & msl haven't.  Here, the model is correct, but we need to get views to refresh themselves after we
                    // reset the is-designer-safe flag.
                    // Perf note: reloading the artifact can take some time - so only reload if IsDesignerSafe has changed
                    var isDesignerSafeBefore = existingArtifact.IsDesignerSafe;
                    existingArtifact.DetermineIfArtifactIsDesignerSafe();
                    var isDesignerSafeAfter = existingArtifact.IsDesignerSafe;
                    if (isDesignerSafeAfter != isDesignerSafeBefore)
                    {
                        existingArtifact.FireArtifactReloadedEvent();
                    }
                }
            }
            finally
            {
                // remove tempArtifactBasedOnDatabase to dispose EFObject's properly
                if (tempEdmxFileUri != null
                    && tempModelManager != null)
                {
                    tempModelManager.ClearArtifact(tempEdmxFileUri);
                }

                // dispose of our temp model manager
                if (tempModelManager != null)
                {
                    tempModelManager.Dispose();
                }

                // delete temporary file
                if (tempEdmxFile != null
                    && tempEdmxFile.Exists)
                {
                    try
                    {
                        tempEdmxFile.Delete();
                    }
                    catch (IOException)
                    {
                        // do nothing if delete fails
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Will validate the passed in artifact and throw an exception if validation fails
        ///     TODO: figure out what to do with the actual errors (write to a log?)
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static void ValidateArtifact(EntityDesignModelManager modelManager, EFArtifact artifact, WizardKind kind)
        {
            var errorsFound = false;
            Exception caughtException = null;

            try
            {
                VsUtils.EnsureProvider(artifact);
                var artifactSet = (EntityDesignArtifactSet)modelManager.GetArtifactSet(artifact.Uri);
                modelManager.ValidateAndCompileMappings(artifactSet, false); // just run the runtime's validation
                var errors = artifactSet.GetAllErrorsForArtifact(artifact);
                if (errors != null
                    && errors.Count > 0)
                {
                    foreach (var error in errors)
                    {
                        if (error.IsError())
                        {
                            errorsFound = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                caughtException = e;
            }

            if (errorsFound || caughtException != null)
            {
                var message = string.Empty;
                if (kind == WizardKind.Generate)
                {
                    message = Resources.Extensibility_ExtensionMadeBadModel;
                }
                else if (kind == WizardKind.UpdateModel)
                {
                    message = Resources.Extensibility_ExtensionMadeBadModel_Update;
                }

                if (caughtException == null)
                {
                    throw new InvalidOperationException(message);
                }
                else
                {
                    throw new InvalidOperationException(message, caughtException);
                }
            }
        }

        /// <summary>
        ///     Returns a Command to update a value of a designer property
        /// </summary>
        private static void AddUpdateDesignerPropertyCommand(
            DesignerProperty property, string propertyName, bool checkBoxValue, OptionsDesignerInfo optionsDesignerInfo,
            List<Command> commands)
        {
            if (property == null
                || property.ValueAttr == null
                || checkBoxValue != bool.Parse(property.ValueAttr.Value))
            {
                var value = checkBoxValue ? Boolean.TrueString : Boolean.FalseString;
                var cmd = new ChangeDesignerPropertyCommand(propertyName, value, optionsDesignerInfo);
                commands.Add(cmd);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private static FileInfo ConstructTempEdmxFile(ModelBuilderSettings settings)
        {
            var tempFilePath = Path.GetTempFileName().Replace(".tmp", EntityDesignArtifact.EXTENSION_EDMX);
            using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
            {
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.Write(settings.ModelBuilderEngine.Model.ToString());
                }
            }
            return new FileInfo(tempFilePath);
        }

        private class DispatchToExtensionsCommand : Command
        {
            private readonly EFArtifact _artifact;
            private readonly XDocument _fromDbDocument;
            private readonly XDocument _originalDocument;
            private readonly ProjectItem _projectItem;
            private readonly ModelBuilderSettings _settings;

            internal DispatchToExtensionsCommand(ModelBuilderSettings settings)
            {
                _settings = settings;
                _artifact = settings.Artifact;
                _fromDbDocument = settings.ModelBuilderEngine.Model;
                _projectItem = VsUtils.GetProjectItemForDocument(_artifact.Uri.LocalPath, PackageManager.Package);

                // make a copy of the artifact before any chagnes are done to it.  This is the "original document" that will be passed to extensions
                _originalDocument = XDocument.Parse(_artifact.XDocument.ToString(), LoadOptions.PreserveWhitespace);
            }

            protected override void InvokeInternal(CommandProcessorContext cpc)
            {
                // make a copy of the artifact in its current state.  This is the "update model document".  That is, the document after we've run update-model logic on it.
                var updateModelDocument = XDocument.Parse(_artifact.XDocument.ToString(), LoadOptions.PreserveWhitespace);

                var dispatcher = new UpdateModelFromDBExtensionDispatcher(
                    WizardKind.UpdateModel, _fromDbDocument, _artifact.XDocument, _projectItem, _originalDocument, updateModelDocument);
                dispatcher.Dispatch();

                _settings.HasExtensionChangedModel = dispatcher.HasCurrentChanged;
            }

            protected override void PostInvoke(CommandProcessorContext cpc)
            {
                base.PostInvoke(cpc);

                if (_settings.HasExtensionChangedModel)
                {
                    //
                    // we validate here in the command so that we can throw to abort the transaction on errors
                    //

                    //
                    // since the xml editor transaction hasn't been committed, the model state isn't totally valid and we get
                    // a bunch of assertions if we use the model backed by the xml editor.  For example, if we try to validate, 
                    // we get assertions about being unable to find line numbers.  Because of this, we create a temporary artifact
                    // on the updated model and validate that.
                    //
                    EntityDesignModelManager tempModelManager = null;
                    EFArtifact tempArtifact = null;
                    InMemoryXmlModelProvider modelProvider = null;

                    try
                    {
                        var uri = _artifact.Uri;
                        modelProvider = new InMemoryXmlModelProvider(uri, _artifact.XDocument.ToString());
                        // NOTE:  use an EFArtifact with VSArtifactSet.  This is so we get the in-vs behavior of artifact sets, but don't rely on VS loading the model into the RDT, etc..
                        // We passed in instance of EFArtifactFactory to the model manager because we don't want the DiagramArtifact is instantiated and loaded because:
                        // - We are only interested in Model validation; there is no need to load diagram.
                        // - InMemoryXmlModelProvider will throw when we request to load diagram model since diagram model URI is different from entity model URI.
                        tempModelManager = new EntityDesignModelManager(new EFArtifactFactory(), new VSArtifactSetFactory());
                        tempArtifact = tempModelManager.GetNewOrExistingArtifact(uri, modelProvider);
                        ValidateArtifact(tempModelManager, tempArtifact, WizardKind.UpdateModel);
                    }
                    finally
                    {
                        if (tempArtifact != null)
                        {
                            tempArtifact.Dispose();
                        }
                        if (tempModelManager != null)
                        {
                            tempModelManager.Dispose();
                        }
                        if (modelProvider != null)
                        {
                            modelProvider.Dispose();
                        }
                    }
                }
            }
        }
    }
}
