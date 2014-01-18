// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Activities;
    using System.Activities.Hosting;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Xaml;
    using System.Xml;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.DatabaseGeneration;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal static class DatabaseGenerationEngine
    {
        private static string _defaultWorkflowPath;
        private static string _defaultTemplatePath;
        private static ExtensibleFileManager _workflowFileManager;
        private static ExtensibleFileManager _templateFileManager;
        private const string XamlExtension = ".xaml";
        private const string TtExtension = ".tt";
        private const string DefaultWorkflowName = "TablePerTypeStrategy.xaml";
        private const string DefaultTemplateName = "SSDLToSQL10.tt";
        private const string DefaultDatabaseSchema = "dbo";

        internal static readonly string _dbGenFolderName = "DBGen";
        internal static readonly string _ddlFileExtension = ".sql";
        internal static readonly string _sqlceFileExtension = ".sqlce";

        internal static string DefaultWorkflowPath
        {
            get
            {
                if (_defaultWorkflowPath == null)
                {
                    _defaultWorkflowPath = Path.Combine(
                        Path.Combine(ExtensibleFileManager.VSEFToolsMacro, _dbGenFolderName), DefaultWorkflowName);
                }
                return _defaultWorkflowPath;
            }
        }

        internal static string DefaultTemplatePath
        {
            get
            {
                if (_defaultTemplatePath == null)
                {
                    _defaultTemplatePath = Path.Combine(
                        Path.Combine(ExtensibleFileManager.VSEFToolsMacro, _dbGenFolderName), DefaultTemplateName);
                }
                return _defaultTemplatePath;
            }
        }

        internal static ExtensibleFileManager WorkflowFileManager
        {
            get
            {
                if (_workflowFileManager == null)
                {
                    _workflowFileManager = new ExtensibleFileManager(_dbGenFolderName, XamlExtension);
                }
                return _workflowFileManager;
            }
        }

        internal static ExtensibleFileManager TemplateFileManager
        {
            get
            {
                if (_templateFileManager == null)
                {
                    _templateFileManager = new ExtensibleFileManager(_dbGenFolderName, TtExtension);
                }
                return _templateFileManager;
            }
        }

        internal static string DefaultDatabaseSchemaName
        {
            get { return DefaultDatabaseSchema; }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static void GenerateDatabaseScriptFromModel(EntityDesignArtifact artifact)
        {
            VsUtils.EnsureProvider(artifact);

            var project = VSHelpers.GetProjectForDocument(artifact.Uri.LocalPath, PackageManager.Package);
            var sp = Services.ServiceProvider;
            ModelBuilderWizardForm form;
            ModelBuilderWizardForm.WizardMode startMode;
            ModelBuilderSettings settings;

            // Start the hourglass, especially because we'll be incurring a perf hit from validating
            using (new VsUtils.HourglassHelper())
            {
                // Before running the Generate Database wizard, we have to make sure that the C-Side validates
                VisualStudioEdmxValidator.LoadAndValidateFiles(artifact.Uri);

                if (
                    artifact.ArtifactSet.GetAllErrors()
                        .Count(ei => ei.ErrorClass == ErrorClass.Runtime_CSDL || ei.ErrorClass == ErrorClass.Escher_CSDL) > 0)
                {
                    VsUtils.ShowErrorDialog(Resources.DatabaseCreation_ValidationFailed);
                    return;
                }

                // set up ModelBuilderSettings
                ModelBuilderEngine.SetupSettingsAndModeForDbPages(
                    sp, project, artifact, false,
                    ModelBuilderWizardForm.WizardMode.PerformDatabaseConfigAndDBGenSummary,
                    ModelBuilderWizardForm.WizardMode.PerformDBGenSummaryOnly, out startMode, out settings);

                form = new ModelBuilderWizardForm(sp, settings, startMode);
            }

            var originalSchemaVersion = settings.TargetSchemaVersion;

            try
            {
                // start the ModelBuilderWizardForm; this will start the workflow in another thread.
                form.Start();
            }
            catch (Exception e)
            {
                VsUtils.ShowErrorDialog(
                    String.Format(
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

            // If database was configured, add DbContext templates
            if (startMode == ModelBuilderWizardForm.WizardMode.PerformDatabaseConfigAndDBGenSummary)
            {
                var edmxItem = VsUtils.GetProjectItemForDocument(artifact.Uri.LocalPath, sp);
                new DbContextCodeGenerator().AddDbContextTemplates(edmxItem, settings.UseLegacyProvider);

                // We need to reload the artifact if we updated the edmx as part of generating
                // model from the database.
                if (settings.TargetSchemaVersion != originalSchemaVersion)
                {
                    artifact.ReloadArtifact();
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal static bool UpdateEdmxAndEnvironment(ModelBuilderSettings settings)
        {
            var artifact = settings.Artifact as EntityDesignArtifact;
            if (artifact == null)
            {
                Debug.Fail("In trying to UpdateEdmxAndEnvironment(), No Artifact was found in the ModelBuilderSettings");
                return false;
            }

            // Update the app. or web.config, register build providers etc
            ConfigFileUtils.UpdateConfig(settings);

            if (settings.SsdlStringReader != null
                && settings.MslStringReader != null)
            {
                // Create the XmlReaders for the ssdl and msl text
                var ssdlXmlReader = XmlReader.Create(settings.SsdlStringReader);
                var mslXmlReader = XmlReader.Create(settings.MslStringReader);

                // Set up our post event to clear out the error list
                var cmd = new ReplaceSsdlAndMslCommand(ssdlXmlReader, mslXmlReader);
                cmd.PostInvokeEvent += (o, e) =>
                    {
                        var errorList = ErrorListHelper.GetSingleDocErrorList(e.CommandProcessorContext.Artifact.Uri);
                        if (errorList != null)
                        {
                            errorList.Clear();
                        }
                    };

                // Update the model (all inside 1 transaction so we don't get multiple undo's/redo's)
                var editingContext =
                    PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(settings.Artifact.Uri);
                var cpc = new CommandProcessorContext(
                    editingContext,
                    EfiTransactionOriginator.GenerateDatabaseScriptFromModelId, Resources.Tx_GenerateDatabaseScriptFromModel);
                var cp = new CommandProcessor(cpc, cmd);
                var addUseLegacyProviderCommand = ModelHelper.CreateSetDesignerPropertyValueCommandFromArtifact(
                    cpc.Artifact,
                    OptionsDesignerInfo.ElementName,
                    OptionsDesignerInfo.AttributeUseLegacyProvider,
                    settings.UseLegacyProvider.ToString());
                if (addUseLegacyProviderCommand != null)
                {
                    cp.EnqueueCommand(addUseLegacyProviderCommand);
                }

                // When the user had a v2 edmx file (it can happen when creating a new empty model in a project targeting 
                // .NET Framework 4 and the project does not have refereces to any of EF dlls) and selected EF6 in 
                // the "create database from model" wizard we need to update the artifact to use v3 schemas otherwise
                // there will be a watermark saying that the edmx is not correct for the EF version and needs to be updated.
                // We only want to run this command if the version really changed to avoid the overhead.
                if (artifact.SchemaVersion != settings.TargetSchemaVersion)
                {
                    cp.EnqueueCommand(new RetargetXmlNamespaceCommand(artifact, settings.TargetSchemaVersion));
                }

                cp.Invoke();
            }

            // First let's get the canonical file path since DTE needs this
            if (!string.IsNullOrEmpty(settings.DdlFileName)
                && settings.DdlStringReader != null)
            {
                var canonicalFilePath = string.Empty;
                try
                {
                    var fi = new FileInfo(settings.DdlFileName);
                    canonicalFilePath = fi.FullName;
                }
                catch (Exception e)
                {
                    Debug.Fail(
                        "We should have caught this exception '" + e.Message + "' immediately after the user clicked the 'Finish' button");
                    VsUtils.ShowErrorDialog(
                        String.Format(
                            CultureInfo.CurrentCulture, ModelWizard.Properties.Resources.ErrorCouldNotParseDdlFileName, settings.DdlFileName,
                            e.Message));
                    return false;
                }

                // Output the DDL file, catch any Exceptions, display them, and revert
                // back to the last page of the wizard.
                try
                {
                    OutputDdl(canonicalFilePath, settings.DdlStringReader);
                }
                catch (Exception e)
                {
                    if (e.InnerException == null)
                    {
                        VsUtils.ShowErrorDialog(
                            String.Format(
                                CultureInfo.CurrentCulture, Resources.DatabaseCreation_ErrorWritingDdl, canonicalFilePath, e.Message));
                    }
                    else
                    {
                        VsUtils.ShowErrorDialog(
                            String.Format(
                                CultureInfo.CurrentCulture, Resources.DatabaseCreation_ErrorWritingDdlWithInner, canonicalFilePath,
                                e.Message, e.InnerException.Message));
                    }
                    return false;
                }

                // Add DDL file to the project if it is inside the project
                string relativePath;
                if (VsUtils.TryGetRelativePathInProject(settings.Project, canonicalFilePath, out relativePath))
                {
                    AddDDLFileToProject(settings.Project, canonicalFilePath);
                }

                // Open the DDL file if it is not already open
                IVsUIHierarchy hier;
                uint itemId;
                IVsWindowFrame frame;
                if (VsShellUtilities.IsDocumentOpen(
                    Services.ServiceProvider, canonicalFilePath, Guid.Empty, out hier, out itemId, out frame) == false)
                {
                    VsShellUtilities.OpenDocument(Services.ServiceProvider, canonicalFilePath);
                }
            }

            return true;
        }

        private static void OutputDdl(string ddlFileName, StringReader ddlStringReader)
        {
            var fileInfo = new FileInfo(ddlFileName);
            if (false == fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            var ddlText = ddlStringReader.ReadToEnd();
            IDictionary<string, object> map = new Dictionary<string, object> { { ddlFileName, ddlText } };
            VsUtils.WriteCheckoutTextFilesInProject(map);
        }

        internal static string CreateDefaultDdlFileName(ProjectItem artifactProjectItem)
        {
            // Starting with the artifact's ProjectItem, walk up the parents building a relative path
            // and finally append to the project. This builds up the "project path" to the file so we can
            // work with linked files.
            var relativeArtifactPath = artifactProjectItem.Name;
            var parentItem = artifactProjectItem.Collection.Parent as ProjectItem;
            while (parentItem != null)
            {
                relativeArtifactPath = Path.Combine(parentItem.Name, relativeArtifactPath);
                parentItem = parentItem.Collection.Parent as ProjectItem;
            }
            return relativeArtifactPath;
        }

        internal static void AddDDLFileToProject(Project project, string ddlFileName)
        {
#if DEBUG
            var fileInfo = new FileInfo(ddlFileName);
            Debug.Assert(
                String.Equals(fileInfo.FullName, ddlFileName, StringComparison.OrdinalIgnoreCase),
                "We should have passed the canonical name into this method since DTE does not accept paths with consecutive slashes");
#endif
            var ddlProjectItem = VsUtils.GetProjectItemForDocument(ddlFileName, Services.ServiceProvider);
            if (ddlProjectItem == null)
            {
                project.ProjectItems.AddFromFile(ddlFileName);
            }
        }

        internal static string GetProviderManifestTokenDisconnected(EFArtifact artifact)
        {
            var storageModel = artifact.StorageModel();
            if (storageModel != null
                && storageModel.ProviderManifestToken != null)
            {
                return storageModel.ProviderManifestToken.Value;
            }

            Debug.Fail("Unable to determine the provider manifest token for the SSDL");

            return String.Empty;
        }

        internal static string GetProviderManifestTokenConnected(
            IDbDependencyResolver resolver, string providerInvariantName, string providerConnectionString)
        {
            DbConnection connection = null;
            try
            {
                var factory = DbProviderFactories.GetFactory(providerInvariantName);
                Debug.Assert(factory != null, "failed because DbProviderFactory is null");

                connection = factory.CreateConnection();

                Debug.Assert(connection != null, "failed because DbConnection is null");
                connection.ConnectionString = providerConnectionString;

                var providerServices = resolver.GetService<DbProviderServices>(providerInvariantName);
                Debug.Assert(providerServices != null, "failed because DbProviderServices is null");

                return providerServices.GetProviderManifestToken(connection);
            }
            finally
            {
                VsUtils.SafeCloseDbConnection(connection, providerInvariantName, providerConnectionString);
            }
        }

        private static string GetSchemaFromRuntimeModelRoot(EFRuntimeModelRoot modelRoot)
        {
            Debug.Assert(modelRoot != null, "EFRuntimeModelROot is null ");
            var sb = new StringBuilder();

            Debug.Assert(
                modelRoot != null && modelRoot.XElement != null,
                "Could not find the runtime model root or its XElement in GetSchemaFromRuntimeModelRoot");
            if (modelRoot != null
                && modelRoot.XElement != null)
            {
                using (var writer = new StringWriter(sb, CultureInfo.CurrentCulture))
                {
                    modelRoot.XElement.Save(writer);
                }
            }

            return sb.ToString();
        }

        internal static string GetSsdlFromArtifact(EFArtifact artifact)
        {
            Debug.Assert(artifact != null, "Artifact is null ");
            if (artifact != null)
            {
                return GetSchemaFromRuntimeModelRoot(artifact.StorageModel());
            }

            return String.Empty;
        }

        internal static string GetMslFromArtifact(EFArtifact artifact)
        {
            Debug.Assert(artifact != null, "Artifact is null ");
            if (artifact != null)
            {
                return GetSchemaFromRuntimeModelRoot(artifact.MappingModel());
            }

            return String.Empty;
        }

        internal static EdmItemCollection GetEdmItemCollectionFromArtifact(EFArtifact artifact, out IList<EdmSchemaError> schemaErrors)
        {
            Debug.Assert(artifact != null, "Artifact is null ");
            EdmItemCollection edmItemCollection = null;
            schemaErrors = new List<EdmSchemaError>();

            var conceptualModel = artifact.ConceptualModel();
            Debug.Assert(
                conceptualModel != null && conceptualModel.XElement != null,
                "Could not find the conceptual model or its XElement in GetEdmItemCollectionFromArtifact");
            if (conceptualModel != null
                && conceptualModel.XElement != null)
            {
                using (var xmlReader = conceptualModel.XElement.CreateReader())
                {
                    edmItemCollection = EdmItemCollection.Create(new[] { xmlReader }, null, out schemaErrors);
                }
            }

            return edmItemCollection;
        }

        internal static string GetWorkflowPathFromArtifact(EFArtifact artifact)
        {
            var workflowPath = ModelHelper.GetDesignerPropertyValueFromArtifact(
                OptionsDesignerInfo.ElementName, OptionsDesignerInfo.AttributeDatabaseGenerationWorkflow, artifact);
            if (String.IsNullOrEmpty(workflowPath))
            {
                // There is probably not a DesignerProperty under the DesignerInfoPropertySet or there may not even
                // be a DesignerInfoPropertySet under the DesignerInfo. In this case, we will just use the default value
                // of the workflow path.
                workflowPath = DefaultWorkflowPath;
            }
            return workflowPath;
        }

        internal static string GetTemplatePathFromArtifact(EFArtifact artifact)
        {
            var templatePath = ModelHelper.GetDesignerPropertyValueFromArtifact(
                OptionsDesignerInfo.ElementName, OptionsDesignerInfo.AttributeDDLGenerationTemplate, artifact);
            if (String.IsNullOrEmpty(templatePath))
            {
                // There is probably not a DesignerProperty under the DesignerInfoPropertySet or there may not even
                // be a DesignerInfoPropertySet under the DesignerInfo. In this case, we will just use the default value
                // of the workflow path.
                templatePath = DefaultTemplatePath;
            }
            return templatePath;
        }

        internal static string GetDatabaseSchemaNameFromArtifact(EFArtifact artifact)
        {
            var databaseSchemaName = ModelHelper.GetDesignerPropertyValueFromArtifact(
                OptionsDesignerInfo.ElementName, OptionsDesignerInfo.AttributeDatabaseSchemaName, artifact);
            if (String.IsNullOrEmpty(databaseSchemaName))
            {
                // There is probably not a DesignerProperty under the DesignerInfoPropertySet or there may not even
                // be a DesignerInfoPropertySet under the DesignerInfo. In this case, we will just use the default value
                // of the database schema name
                databaseSchemaName = DefaultDatabaseSchemaName;
            }
            return databaseSchemaName;
        }

        internal class PathValidationErrorMessages
        {
            private string _nullFile;
            private string _nonValid;
            private string _parseError;
            private string _nonFile;
            private string _notInProject;
            private string _nonExistant;

            // <summary>
            //     Used when the resolved file path is null or empty. Requires 0 FormatItems.
            // </summary>
            internal string NullFile
            {
                get { return _nullFile; }
                set
                {
#if DEBUG
                    ValidateFormat(value, 0);
#endif
                    _nullFile = value;
                }
            }

            // <summary>
            //     Used if we can't resolve the absolute URI created from the resolved file path or the relative URI,
            //     relative to the project. Requires 1 FormatItem: the resolved workflow file path.
            // </summary>
            internal string NonValid
            {
                get { return _nonValid; }
                set
                {
#if DEBUG
                    ValidateFormat(value, 1);
#endif
                    _nonValid = value;
                }
            }

            // <summary>
            //     Used when an exception occurs while creating the URIs. Requires 2 FormatItems: the unresolved
            //     file path and the exception message.
            // </summary>
            internal string ParseError
            {
                get { return _parseError; }
                set
                {
#if DEBUG
                    ValidateFormat(value, 2);
#endif
                    _parseError = value;
                }
            }

            // <summary>
            //     Used if the given file is a UNC path or one without a file:// scheme. Requires 1 FormatItem:
            //     the resolved file path.
            // </summary>
            internal string NonFile
            {
                get { return _nonFile; }
                set
                {
#if DEBUG
                    ValidateFormat(value, 1);
#endif
                    _nonFile = value;
                }
            }

            // <summary>
            //     Used if the file has not been included in the project. Requires 1 FormatItem: the resolved
            //     file path
            // </summary>
            internal string NotInProject
            {
                get { return _notInProject; }
                set
                {
#if DEBUG
                    ValidateFormat(value, 1);
#endif
                    _notInProject = value;
                }
            }

            // <summary>
            //     Used if the resolved file path does not exist. Requires 1 FormatItem: the resolved file path
            // </summary>
            internal string NonExistant
            {
                get { return _nonExistant; }
                set
                {
#if DEBUG
                    ValidateFormat(value, 1);
#endif
                    _nonExistant = value;
                }
            }

#if DEBUG
            private readonly Regex _argumentsRegex = new Regex(@"\{\d+\}");

            private void ValidateFormat(string value, int numArguments)
            {
                if (_argumentsRegex.Matches(value).Count != numArguments)
                {
                    Debug.Fail("There should be " + numArguments + " FormatItems in the error message " + value);
                }
            }

            internal void ValidateAllProperties()
            {
                Debug.Assert(false == String.IsNullOrEmpty(NonExistant), "NonExistant property not set");
                Debug.Assert(false == String.IsNullOrEmpty(NonFile), "NonFile property not set");
                Debug.Assert(false == String.IsNullOrEmpty(NonValid), "NonValid property not set");
                Debug.Assert(false == String.IsNullOrEmpty(NotInProject), "NotInProject property not set");
                Debug.Assert(false == String.IsNullOrEmpty(NullFile), "NullFile property not set");
                Debug.Assert(false == String.IsNullOrEmpty(ParseError), "ParseError property not set");
            }
#endif
        }

        internal static FileInfo ResolveAndValidateWorkflowPath(Project project, string unresolvedPath)
        {
            // Resolve and validate the workflow file path
            var errorMessages = new PathValidationErrorMessages
                {
                    NullFile = Resources.DatabaseCreation_ErrorWorkflowPathNotSet,
                    NonValid = Resources.DatabaseCreation_ErrorNonValidWorkflowUri,
                    ParseError = Resources.DatabaseCreation_ExceptionParsingWorkflowFilePath,
                    NonFile = Resources.DatabaseCreation_NonFileWorkflow,
                    NotInProject = Resources.DatabaseCreation_ErrorWorkflowFileNotInProject,
                    NonExistant = Resources.DatabaseCreation_WorkflowFileNotExists
                };

            return ResolveAndValidatePath(
                project,
                unresolvedPath,
                errorMessages);
        }

        // <summary>
        //     Create the WF workflow application used by the Database Script Generation wizard:
        //     1. Deserialize the XAML file specified by the user
        //     2. Add inputs
        //     3. Add parameters to the EdmParameterBag, added to the workflow via an extension
        // </summary>
        // <param name="syncContext">SynchronizationContext of VS's UI thread that can be used by the workflow to spawn UIs</param>
        // <param name="project"></param>
        // <param name="artifactPath"></param>
        // <param name="workflowFileInfo"></param>
        // <param name="templatePath">DDL template path. We will resolve/validate this at runtime within the appropriate TemplateActivity</param>
        // <param name="edmItemCollection"></param>
        // <param name="existingSsdl"></param>
        // <param name="existingMsl"></param>
        // <param name="databaseSchemaName"></param>
        // <param name="databaseName"></param>
        // <param name="providerInvariantName"></param>
        // <param name="providerConnectionString"></param>
        // <param name="providerManifestToken"></param>
        // <param name="targetVersion"></param>
        // <param name="workflowCompletedHandler"></param>
        // <param name="unhandledExceptionHandler"></param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        internal static WorkflowApplication CreateDatabaseScriptGenerationWorkflow(
            SynchronizationContext syncContext,
            Project project,
            string artifactPath,
            FileInfo workflowFileInfo,
            string templatePath,
            EdmItemCollection edmItemCollection,
            string existingSsdl,
            string existingMsl,
            string databaseSchemaName,
            string databaseName,
            string providerInvariantName,
            string providerConnectionString,
            string providerManifestToken,
            Version targetVersion,
            Action<WorkflowApplicationCompletedEventArgs> workflowCompletedHandler,
            Func<WorkflowApplicationUnhandledExceptionEventArgs, UnhandledExceptionAction> unhandledExceptionHandler)
        {
            // Inputs are specific to the workflow. No need to provide a strongly typed bag here
            // because the workflow has to define these.
            var inputs = new Dictionary<string, object>
                {
                    { EdmConstants.csdlInputName, edmItemCollection },
                    { EdmConstants.existingSsdlInputName, existingSsdl },
                    { EdmConstants.existingMslInputName, existingMsl }
                };

            // Initialize the AssemblyLoader. This will cache project/website references and proffer assembly
            // references to the XamlSchemaContext as well as the OutputGeneratorActivities
            var assemblyLoader = new DatabaseGenerationAssemblyLoader(project, VsUtils.GetVisualStudioInstallDir());

            // Parameters can be used throughout the workflow. These are more ubiquitous than inputs and
            // so they do not need to be defined ahead of time in the workflow.
            var edmWorkflowSymbolResolver = new EdmParameterBag(
                syncContext, assemblyLoader, targetVersion, providerInvariantName, providerManifestToken, providerConnectionString,
                databaseSchemaName, databaseName, templatePath, artifactPath);

            // Deserialize the XAML file into a Activity
            Activity modelFirstWorkflowElement;
            using (var stream = workflowFileInfo.OpenRead())
            {
                using (
                    var xamlXmlReader = new XamlXmlReader(XmlReader.Create(stream), new DatabaseGenerationXamlSchemaContext(assemblyLoader))
                    )
                {
                    modelFirstWorkflowElement = ActivityXamlServices.Load(xamlXmlReader);
                }
            }

            // Create a WorkflowInstance from the WorkflowElement and pass in the inputs
            var workflowInstance = new WorkflowApplication(modelFirstWorkflowElement, inputs);

            // Attach a SymbolResolver for external parameters; this is like the ParameterBag
            var symbolResolver = new SymbolResolver();
            symbolResolver.Add(typeof(EdmParameterBag).Name, edmWorkflowSymbolResolver);

            workflowInstance.Extensions.Add(symbolResolver);
            workflowInstance.Completed = workflowCompletedHandler;
            workflowInstance.OnUnhandledException = unhandledExceptionHandler;

            return workflowInstance;
        }

        // <summary>
        //     Resolves the given 'path' with the given project's macros and validates it based on these rules:
        //     1. The resolved path should not be null or empty
        //     2. We should be able to create a absolute URI from the resolved path OR
        //     2.1 We should be able to create a relative URI, relative to the project from the resolved path
        //     3. If the path is a custom path, it should be included in the project and should be relative to the project.
        //     4. The file must exist.
        // </summary>
        internal static FileInfo ResolveAndValidatePath(Project project, string path, PathValidationErrorMessages errorMessages)
        {
#if DEBUG
            errorMessages.ValidateAllProperties();
#endif
            var resolvedPath = VsUtils.ResolvePathWithMacro(
                null, path,
                new Dictionary<string, string>
                    {
                        { ExtensibleFileManager.EFTOOLS_USER_MACRONAME, ExtensibleFileManager.UserEFToolsDir.FullName },
                        { ExtensibleFileManager.EFTOOLS_VS_MACRONAME, ExtensibleFileManager.VSEFToolsDir.FullName }
                    });

            // First check null
            if (String.IsNullOrEmpty(resolvedPath))
            {
                throw new InvalidOperationException(errorMessages.NullFile);
            }

            // resolved path should be a resolvable full path, so try creating a URI. We will need the URI's local path for the next step. This process
            // will strip out any levels of indirection in the path ('..\'). If this is a relative path pointing to a file in the project then this step will
            // be a no-op.
            Uri resolvedUri;
            if (Uri.TryCreate(resolvedPath, UriKind.Absolute, out resolvedUri))
            {
                resolvedPath = resolvedUri.LocalPath;
            }

            // We need to determine if this is a custom path; that is, something the user has typed in that is different from
            // the files in the 'user' and 'vs' directories.
            string temporaryRelativePath;
            if (VsUtils.TryGetRelativePathInParentPath(
                Path.Combine(ExtensibleFileManager.UserEFToolsDir.FullName, _dbGenFolderName), resolvedPath, out temporaryRelativePath)
                == false
                &&
                VsUtils.TryGetRelativePathInParentPath(
                    Path.Combine(ExtensibleFileManager.VSEFToolsDir.FullName, _dbGenFolderName), resolvedPath, out temporaryRelativePath)
                == false)
            {
                // Attempt to resolve to the project
                try
                {
                    bool projectHasFilename;
                    var projectPath = VsUtils.GetProjectPathWithName(project, out projectHasFilename);
                    var projectDirUri = new Uri(projectPath);
                    Debug.Assert(projectDirUri != null, "The project directory URI is null; why wasn't an exception thrown?");

                    if (projectDirUri != null)
                    {
                        if (false == Uri.TryCreate(projectDirUri, resolvedPath, out resolvedUri))
                        {
                            throw new InvalidOperationException(
                                String.Format(CultureInfo.CurrentCulture, errorMessages.NonValid, resolvedPath));
                        }
                        resolvedPath = resolvedUri.LocalPath;
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, errorMessages.ParseError, path, e.Message), e);
                }

                // Now check if the file has been added to the project using the resolved path. We do not allow otherwise.
                if (VsUtils.GetProjectItemForDocument(resolvedPath, Services.ServiceProvider) == null)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, errorMessages.NotInProject, resolvedPath));
                }
            }

            // If the specified file is not installed or does not exist, then we cannot continue
            var pathFileInfo = new FileInfo(resolvedPath);
            if (false == pathFileInfo.Exists)
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, errorMessages.NonExistant, pathFileInfo.FullName));
            }

            return pathFileInfo;
        }
    }
}
