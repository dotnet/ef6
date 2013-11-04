// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Xml;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Extensibility;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Entity.Design.VisualStudio.Model;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Data.Services;
    using Microsoft.VisualStudio.Modeling.Shell;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.TemplateWizard;
    using Command = Microsoft.Data.Entity.Design.Model.Commands.Command;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    /// <summary>
    ///     Visual Studio invokes this wizard when a new item of type "ADO.NET Entity Data Model" is added
    ///     to an existing project.  This wizard is registered in the .vstemplate file item template.
    ///     The files added by this item template are:
    ///     +- modelName.edmx
    ///     |
    ///     +- modelName.Designer.cs [or vb] => code generator output
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class ModelObjectItemWizard : IWizard
    {
        // NOTE: This is not localized because it ends up in the CSDL (and hence the code)
        //       and must be a valid identifier
        private const string EntityContainerNameSuffix = "Container";

        private ProjectItem _edmxItem;
        private Project _activeSolutionProject;
        internal const string EntityDeployBuildActionName = "EntityDeploy";
        internal const string ItemTypePropertyName = "ItemType";

        private ModelBuilderSettings _modelBuilderSettings;

        /// <summary>
        ///     This method is called before opening any item that has the OpenInEditor attribute
        ///     This lets us run custom wizard logic before opening an item in the template
        /// </summary>
        /// <param name="projectItem">The project item that will be opened</param>
        public void BeforeOpeningFile(ProjectItem projectItem)
        {
            // nothing to do
        }

        /// <summary>
        ///     This method is called after the project has been created
        ///     This lets us run custom wizard logic when a project has finished generating
        /// </summary>
        public void ProjectFinishedGenerating(Project project)
        {
            // nothing to do since we are an item template
        }

        /// <summary>
        ///     This method is only called for item templates, not for project templates
        /// </summary>
        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
            Debug.Assert(
                VSArtifact.GetVSArtifactFileExtensions().Contains(Path.GetExtension(projectItem.Name)),
                "Unexpected file extension for project item");
            if (string.Equals(Path.GetExtension(projectItem.Name), EntityDesignArtifact.EXTENSION_EDMX, StringComparison.OrdinalIgnoreCase))
            {
                _edmxItem = projectItem;
            }
        }

        /// <summary>
        ///     This method is called at the beginning of a template wizard run
        ///     This lets us run custom wizard logic before anything is created and is a good place, for example,
        ///     to collect user input that will alter the run in some way
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void RunStarted(
            object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind,
            object[] customParams)
        {
            // the dte is the handle into the VS environment
            var dte = (DTE2)automationObject;
            var serviceProvider = new ServiceProvider((IOleServiceProvider)dte);

            // get the current project that the wizard is running in
            _activeSolutionProject = VsUtils.GetActiveProject(dte);
            Debug.Assert(_activeSolutionProject != null, "Unable to retrieve ActiveSolutionProject from DTE");

            EnsureCanStartWizard(serviceProvider);

            // get file name the user chose 
            string modelName;
            replacementsDictionary.TryGetValue("$rootname$", out modelName);
            Debug.Assert(modelName != null, "Unable to get $rootname$ from replacementsDictionary");

            PopluateReplacementDictionary(replacementsDictionary, modelName);

            _modelBuilderSettings = new ModelBuilderSettings
            {
                VSApplicationType = VsUtils.GetApplicationType(serviceProvider, _activeSolutionProject),
                WizardKind = WizardKind.Generate,
                TargetSchemaVersion =
                    EdmUtils.GetEntityFrameworkVersion(_activeSolutionProject, serviceProvider, useLatestIfNoEF: false),
                NewItemFolder = GetFolderNameForNewItems(dte, _activeSolutionProject),
                Project = _activeSolutionProject,
                ModelName = modelName,
                VsTemplatePath = customParams[0] as string, 
                ReplacementDictionary =  replacementsDictionary
            };

            var form = new ModelBuilderWizardForm(
                serviceProvider,
                _modelBuilderSettings,
                ModelBuilderWizardForm.WizardMode.PerformAllFunctionality)
            {
                FileAlreadyExistsError = false
            };

            try
            {
                form.Start();
            }
            catch (Exception ex)
            {
                VsUtils.ShowErrorDialog(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.ModelObjectItemWizard_UnexpectedExceptionHasOccurred,
                        ex.Message));

                ClearErrors();

                throw new WizardCancelledException();
            }


            // the form.FileAlreadyExistsError flag is set in the WizardPageStart. We do it because if we 
            // threw this exception directly from the WizardPageStart it would be swallowed and the
            // "Add New Item" dialog would not show up. Throwing the exception from here will make
            // the "Add New Item" dialog re-appear which allows the user to enter a different model name.
            if (form.FileAlreadyExistsError)
            {
                Marshal.ThrowExceptionForHR(VSConstants.E_ABORT);   
            }

            // if they cancelled or they didn't cancel, and we didn't log that Finish was pressed, 
            // they must have hit the X so cancel
            if (form.WizardCancelled
                || !form.WizardFinished)
            {
                ClearErrors();
                throw new WizardCancelledException();
            }

            Debug.Assert(ReferenceEquals(_modelBuilderSettings, form.ModelBuilderSettings));
        }

        private void ClearErrors()
        {
            if (_modelBuilderSettings.ModelPath != null)
            {
                PackageManager.Package.ModelGenErrorCache.RemoveErrors(_modelBuilderSettings.ModelPath);
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.VsUtils.ShowErrorDialog(System.String)")]
        private void EnsureCanStartWizard(IServiceProvider serviceProvider)
        {
            // make sure we can access the data package           
            if (serviceProvider.GetService(typeof(IVsDataConnectionManager)) == null)
            {
                VsUtils.ShowErrorDialog(Resources.LoadDataPackageError);
                throw new WizardCancelledException();
            }

            // make sure that our package is loaded
            try
            {
                PackageManager.LoadEDMPackage(serviceProvider);
            }
            catch (Exception ex)
            {
                // an exception occurred loading our package, so raise an error dialog, and cancel the wizard
                var message = Resources.LoadOurPackageError;
#if DEBUG
                message += " " + ex;
#else
                message += " " + ex.Message;
#endif
                VsUtils.ShowErrorDialog(message);
                throw new WizardCancelledException(Resources.LoadOurPackageError, ex);
            }

            if (!VsUtils.EntityFrameworkSupportedInProject(
                _activeSolutionProject, serviceProvider, allowMiscProject: false))
            {
                VsUtils.ShowErrorDialog(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.EdmUtils_NotValidTargetFramework));
                Marshal.ThrowExceptionForHR(VSConstants.E_ABORT);
            }
        }

        private void PopluateReplacementDictionary(Dictionary<string, string> replacementsDictionary, string modelName)
        {
            // create a "fixed" version that removes non-valid characters and leading underscores
            var fixedModelName = XmlConvert.EncodeName(modelName).TrimStart('_');

            //  make sure that the model name is a valid xml attribute value
            if (!EscherAttributeContentValidator.IsValidXmlAttributeValue(fixedModelName))
            {
                VsUtils.ShowErrorDialog(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.ModelObjectItemWizard_NonValidXmlAttributeValue,
                        modelName));
                Marshal.ThrowExceptionForHR(VSConstants.E_ABORT);
            }

            // set the value to be used for namespace in blank models
            replacementsDictionary.Add("$namespace$", fixedModelName);

            // set the value to be used for EntityContainerName in blank models
            var entityContainerName = PackageManager.Package.ConnectionManager.ConstructUniqueEntityContainerName(
                fixedModelName + EntityContainerNameSuffix,
                _activeSolutionProject);
            replacementsDictionary.Add("$conceptualEntityContainerName$", entityContainerName);

            // set default value of the EnablePluralization flag dependent on current culture
            var pluralizationDefault = (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "en").ToString();
            replacementsDictionary.Add("$enablePluralization$", pluralizationDefault);
        }

        /// <summary>
        ///     This method is called at the end of a template wizard run
        ///     This lets us run custom wizard logic when the wizard has completed all tasks
        ///     We set up project item dependencies here for files added by the .vstemplate
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void RunFinished()
        {
            if (_edmxItem == null)
            {
                return;
            }

            var fileExtension = Path.GetExtension(_edmxItem.FileNames[1]);

            Debug.Assert(
                _activeSolutionProject.Equals(_edmxItem.ContainingProject),
                "ActiveSolutionProject is not the EDMX file's containing project");
            using (new VsUtils.HourglassHelper())
            {
                var package = PackageManager.Package;
                Window window = null;

                try
                {
                    ConfigFileUtils.UpdateConfig(_modelBuilderSettings);

                    // save the model generated in the wizard UI.
                    if (_modelBuilderSettings.GenerationOption == ModelGenerationOption.GenerateFromDatabase)
                    {
                        var writingModelWatch = new Stopwatch();
                        writingModelWatch.Start();
                        var mbe = _modelBuilderSettings.ModelBuilderEngine;

                        if (!string.Equals(fileExtension, EntityDesignArtifact.EXTENSION_EDMX, StringComparison.OrdinalIgnoreCase))
                        {
                            // convert the file if this isn't EDMX
                            var edmxFileInfo = new FileInfo(_edmxItem.FileNames[1]);
                            var conversionContext = new ModelConversionContextImpl(
                                _edmxItem.ContainingProject,
                                _edmxItem,
                                edmxFileInfo,
                                _modelBuilderSettings.TargetSchemaVersion,
                                mbe.Model);
                            VSArtifact.DispatchToConversionExtensions(
                                EscherExtensionPointManager.LoadModelConversionExtensions(),
                                fileExtension,
                                conversionContext,
                                loading: false);
                            File.WriteAllText(edmxFileInfo.FullName, conversionContext.OriginalDocument);
                        }
                        else
                        {
                            // we need to use XmlWriter to output so that XmlDeclaration is preserved.
                            using (var modelWriter = XmlWriter.Create(
                                _edmxItem.FileNames[1],
                                new XmlWriterSettings { Indent = true }))
                            {
                                mbe.Model.WriteTo(modelWriter);
                            }
                        }

                        writingModelWatch.Stop();
                        VsUtils.LogOutputWindowPaneMessage(
                            _edmxItem.ContainingProject,
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Properties.Resources.WritingModelTimeMsg,
                                writingModelWatch.Elapsed));

                        // now add errors
                        ErrorListHelper.LogWizardErrors(mbe.Errors, _edmxItem);
                    }

                    // set the ItemType for the generated .edmx file
                    if (_modelBuilderSettings.VSApplicationType != VisualStudioProjectSystem.Website
                        && string.Equals(
                            fileExtension,
                            EntityDesignArtifact.EXTENSION_EDMX,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        _edmxItem.Properties.Item(ItemTypePropertyName).Value = EntityDeployBuildActionName;
                    }

                    // now open created file in VS using default viewer
                    try
                    {
                        window = _edmxItem.Open(Constants.vsViewKindPrimary);
                        Debug.Assert(window != null, "Unable to get window for created edmx file");
                    }
                    catch (ObjectDisposedException)
                    {
                        PackageManager.Package.ModelGenErrorCache.RemoveErrors(_edmxItem.get_FileNames(1));
                    }
                }
                finally
                {
                    package.ModelGenErrorCache.RemoveErrors(_edmxItem.FileNames[1]);
                }

                // Construct an editing context and make all final edits that require the file is opened.
                var edmxFileUri = new Uri(_edmxItem.FileNames[1]);
                var designArtifact =
                    package.ModelManager.GetNewOrExistingArtifact(
                        edmxFileUri, new VSXmlModelProvider(package, package)) as EntityDesignArtifact;
                Debug.Assert(
                    designArtifact != null,
                    "artifact should be of type EntityDesignArtifact but received type " + designArtifact.GetType().FullName);
                Debug.Assert(
                    designArtifact.StorageModel != null, "designArtifact StorageModel cannot be null for Uri " + edmxFileUri.AbsolutePath);
                Debug.Assert(
                    designArtifact.ConceptualModel != null,
                    "designArtifact ConceptualModel cannot be null for Uri " + edmxFileUri.AbsolutePath);

                if (designArtifact != null
                    && designArtifact.StorageModel != null
                    && designArtifact.ConceptualModel != null)
                {
                    var designerSafeBeforeAddingTemplates = designArtifact.IsDesignerSafe;

                    var editingContext =
                        package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(designArtifact.Uri);
                    Debug.Assert(editingContext != null, "Null EditingContext for artifact " + edmxFileUri.AbsolutePath);
                    if (editingContext != null)
                    {
                        // Add DbContext templates when generation is GenerateFromDatabase. (connection is configured)
                        if (_modelBuilderSettings.GenerationOption == ModelGenerationOption.GenerateFromDatabase)
                        {
                            new DbContextCodeGenerator().AddDbContextTemplates(
                                _edmxItem,
                                _modelBuilderSettings.UseLegacyProvider);
                        }

                        // Create FunctionImports for every new Function
                        var cp = PrepareCommandsAndIntegrityChecks(_modelBuilderSettings, editingContext, designArtifact);

                        if (DbContextCodeGenerator.TemplateSupported(_edmxItem.ContainingProject, package))
                        {
                            // Add command setting CodeGenerationStrategy to "None" for EmptyModel. (connection is not yet configured)
                            // NOTE: For EmptyModel, the templates will be added after the connection is configured.
                            //       (i.e. during "Generate Database from Model" or "Refresh from Database")
                            if (_modelBuilderSettings.GenerationOption == ModelGenerationOption.EmptyModel)
                            {
                                var cmd = EdmUtils.SetCodeGenStrategyToNoneCommand(designArtifact);
                                if (cmd != null)
                                {
                                    if (cp == null)
                                    {
                                        var cpc = new CommandProcessorContext(
                                            editingContext,
                                            EfiTransactionOriginator.CreateNewModelId,
                                            Resources.Tx_SetCodeGenerationStrategy);
                                        cp = new CommandProcessor(cpc, cmd);
                                    }
                                    else
                                    {
                                        cp.EnqueueCommand(cmd);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Templates not supported, add reference to SDE. (.NET Framework 3.5)
                            VsUtils.AddProjectReference(_edmxItem.ContainingProject, "System.Data.Entity");
                        }

                        if (cp != null)
                        {
                            cp.Invoke();
                        }

                        // save the artifact to make it look as though updates were part of creation
                        _edmxItem.Save();

                        if (_modelBuilderSettings.GenerationOption == ModelGenerationOption.GenerateFromDatabase
                            && !designerSafeBeforeAddingTemplates)
                        {
                            // If the artifact became safe after adding references we need to reload it (this can happen 
                            // on .NET Framework 4 where we would originally create a v3 edmx if the user selected EF6 - 
                            // the artifact will be flagged as invalid since there is no runtime which could handle v3 
                            // but after we added references to EF6 the artifacts becomes valid and need to be reloaded). 
                            designArtifact.DetermineIfArtifactIsDesignerSafe();
                            if (designArtifact.IsDesignerSafe)
                            {
                                Debug.Assert(!designArtifact.IsDirty, "Reloading dirty artifact - changes will be lost.");

                                // Since the artifact was originally not valid we did not create the diagram for it. 
                                // Using ReloadDocData will cause the diagram to be recreated. Note we don't need to 
                                // reload the artifact itself since it has not changed.
                                ((DocData)
                                 VSHelpers.GetDocData(package, designArtifact.Uri.LocalPath)).ReloadDocData(0);
                            }
                        }
                    }

                    if (window != null)
                    {
                        window.Activate();
                    }
                }
            }
        }

        // internal static to make it more testable
        internal static CommandProcessor PrepareCommandsAndIntegrityChecks(
            ModelBuilderSettings modelBuilderSettings,
            EditingContext editingContext,
            EntityDesignArtifact designArtifact)
        {
            Debug.Assert(modelBuilderSettings != null, "modelBuilderSettings != null");
            Debug.Assert(editingContext != null, "editingContext != null");
            Debug.Assert(designArtifact != null, "artifact != null");

            var commands = new List<Command>();
            if (modelBuilderSettings.NewFunctionSchemaProcedures != null
                && modelBuilderSettings.NewFunctionSchemaProcedures.Count > 0)
            {
                // user selected to create new FunctionImports, but don't create the composable ones as these have already been created by the runtime
                ModelBuilderEngine.ProcessStoredProcedureReturnTypeInformation(
                    designArtifact,
                    modelBuilderSettings.NewFunctionSchemaProcedures,
                    commands,
                    shouldCreateComposableFunctionImports: false);
            }
            else
            {
                commands.AddRange(CreateRemoveFunctionImportCommands(designArtifact));
            }

            // for SqlServer and SqlServerCe we need to add integrity checks - see the comment below
            if (commands.Count > 0
                || designArtifact.IsSqlFamilyProvider())
            {
                // set up CommandProcessorContext
                var cpc = new CommandProcessorContext(
                    editingContext,
                    EfiTransactionOriginator.CreateNewModelId,
                    Resources.Tx_CreateFunctionImport);

                // We propagate facets by default only for Sql Server or Sql Server CE since for other providers facets in C-Space might be intentionally
                // out of sync with facets from S-Space and we should not break this. For Sql Server and Sql Server CE facets should be in sync in most cases.
                if (designArtifact.IsSqlFamilyProvider())
                {
                    // Add integrity check to enforce synchronizing C-side Property facets to S-side values
                    PropagateStoragePropertyFacetsToConceptualModel.AddRule(cpc, designArtifact);
                }

                return new CommandProcessor(cpc, commands);
            }

            // no commands or integrity checks to run
            return null;
        }

        /// <summary>
        ///     Creates commands to remove function imports and complex types corresponding to results if ones exist.
        /// </summary>
        /// <param name="designArtifact">Artifact.</param>
        /// <returns>IEnumerable of commands for deleting function imports and corresponding complex types.</returns>
        /// <remarks>
        ///     This function should be called only from RunFinished() method as we don't check whether complex types we
        ///     are removing are not used by other function imports or entities.
        /// </remarks>
        private static IEnumerable<Command> CreateRemoveFunctionImportCommands(EntityDesignArtifact designArtifact)
        {
            // we were instructed not to create FunctionImports - but runtime has created them automatically so actually need to delete any which have been created
            var model = designArtifact.ConceptualModel;
            var cec = (ConceptualEntityContainer)model.FirstEntityContainer;
            foreach (var fi in cec.FunctionImports())
            {
                yield return fi.GetDeleteCommand();

                if (fi.IsReturnTypeComplexType)
                {
                    var complexType = model.ComplexTypes().SingleOrDefault(
                        c => ReferenceEquals(c, fi.ReturnTypeAsComplexType.Target));

                    Debug.Assert(
                        complexType != null,
                        string.Format("Complex type {0} for FunctionImport {1} does not exist", complexType.Name, fi.Name));

                    if (complexType != null)
                    {
                        yield return complexType.GetDeleteCommand();
                    }
                }
            }
        }

        /// <summary>
        ///     Indicates whether the specified project item should be added to the project
        ///     We always return true
        /// </summary>
        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        /// <summary>
        ///     Returns the folder path for the selected item (project or folder) that we are adding new items into
        /// </summary>
        private static string GetFolderNameForNewItems(DTE2 dte, Project activeProject)
        {
            var selectedItems = dte.SelectedItems;

            // if more than one folder is selected, the wizard framework adds items into the first (index 1) 
            // path in the list.
            var selectedItem = selectedItems.Item(1);

            string path;
            if (selectedItem.Project != null)
            {
                var di = VsUtils.GetProjectRoot(selectedItem.Project, Services.ServiceProvider);
                path = di.FullName;
            }
            else if (selectedItem.ProjectItem != null)
            {
                var pi = selectedItem.ProjectItem;
                path = pi.FileNames[1];
                var di = new DirectoryInfo(path);
                while ((di.Attributes & FileAttributes.Directory) == 0)
                {
                    di = di.Parent;
                }
                path = di.FullName;
            }
            else
            {
                var di = VsUtils.GetProjectRoot(activeProject, Services.ServiceProvider);
                path = di.FullName;
            }

            // if this is a website project, then the path must start with App_Code.  If not, then we will strip 
            // off all other paths and replace them with App_Code.  This is because the wizard will place EDMX files
            // into the root of App_Code if you try to add them in another folder.  For example, if you try to add an EDMX 
            // files into "projectRoot\XXX\YYY", the wizard will pop a dialog and add it into App_Code.
            if (VsUtils.IsWebSiteProject(activeProject))
            {
                var di = new DirectoryInfo(
                    Path.Combine(
                        VsUtils.GetProjectRoot(activeProject, Services.ServiceProvider).FullName,
                        EdmUtils.AppCodeFolderName));
                if (!path.StartsWith(di.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    path = di.FullName;
                }
            }

            return path;
        }
    }
}
