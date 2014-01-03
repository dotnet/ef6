// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using VSErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using VSTextManagerInterop = Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.Data.Entity.Design.Package
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Tools.XmlDesignerBase.Base.Util;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.EntityDesigner.CustomSerializer;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Extensibility;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.UI.Views.Explorer;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.Model;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VisualStudio.Modeling;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling.Immutability;
    using Microsoft.VisualStudio.Modeling.Shell;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using IServiceProvider = System.IServiceProvider;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal partial class MicrosoftDataEntityDesignDocData :
        IEntityDesignDocData,
        VSTextManagerInterop.IVsUndoTrackingEvents,
        IDiagramManager,
        IVsHasRelatedSaveItems
    {
        private object _underlyingBuffer;
        private string _backupFileName;
        private bool _isDiagramLoaded;
        private bool _isHandlingDocumentReloaded;

        public EFArtifact Model
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(FileName))
                {
                    var artifact = PackageManager.Package.ModelManager.GetArtifact(Utils.FileName2Uri(FileName)) as EntityDesignArtifact;
                    return artifact;
                }
                else
                {
                    return null;
                }
            }
        }

        private VSTextManagerInterop.IVsTextLines VsBuffer
        {
            get { return _underlyingBuffer as VSTextManagerInterop.IVsTextLines; }
        }

        // this indicates weather the Model Diagram has already been created or translated
        internal bool IsModelDiagramLoaded
        {
            get { return _isDiagramLoaded; }
        }

        internal bool IsHandlingDocumentReloaded
        {
            get { return _isHandlingDocumentReloaded; }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                UnregisterUndoTracking();
                DestroyBuffer();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsPersistDocData.Close")]
        private void DestroyBuffer()
        {
            var persistDocData = _underlyingBuffer as IVsPersistDocData;
            if (persistDocData != null)
            {
                persistDocData.Close();
            }
            _underlyingBuffer = null;
        }

        public string BackupFileName
        {
            get { return _backupFileName; }
        }

        internal EditingContext EditingContext
        {
            get
            {
                var uri = Utils.FileName2Uri(FileName);

                var contextManager = EditingContextManager;
                if (contextManager != null)
                {
                    return contextManager.GetNewOrExistingContext(uri);
                }
                return null;
            }
        }

        private static EditingContextManager EditingContextManager
        {
            get
            {
                if (PackageManager.Package != null
                    && PackageManager.Package.DocumentFrameMgr != null)
                {
                    return PackageManager.Package.DocumentFrameMgr.EditingContextManager;
                }

                Debug.Fail("Unable to get instance of EditingContextManager.");
                return null;
            }
        }

        /// <summary>
        ///     Creates an instance of a VSTextBuffer, sets the LanguageService SID, and uses IVsPersistDocData to fire loading of the doc data.
        ///     We also start broadcasting a FileNameChanged event if the buffer gets renamed. *note* this method also gets called on reload.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsPersistDocData.LoadDocData(System.String)")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public bool CreateAndLoadBuffer()
        {
            DestroyBuffer();

            var pkg = PackageManager.Package as Package;
            IServiceProvider serviceProvider = pkg;

            var textLinesType = typeof(VSTextManagerInterop.IVsTextLines);
            var riid = textLinesType.GUID;
            var clsid = typeof(VSTextManagerInterop.VsTextBufferClass).GUID;

            _underlyingBuffer = pkg.CreateInstance(ref clsid, ref riid, textLinesType);
            Debug.Assert(_underlyingBuffer != null, "Failure while creating buffer.");

            var buffer = _underlyingBuffer as VSTextManagerInterop.IVsTextLines;
            Debug.Assert(buffer != null, "Why does buffer not implement IVsTextLines?");

            var ows = buffer as IObjectWithSite;
            if (ows != null)
            {
                ows.SetSite(serviceProvider.GetService(typeof(IOleServiceProvider)));
            }

            // We want to set the LanguageService SID explicitly to the XML Language Service.
            // We need turn off GUID_VsBufferDetectLangSID before calling LoadDocData so that the 
            // TextBuffer does not do the work to detect the LanguageService SID from the file extension.
            var userData = buffer as VSTextManagerInterop.IVsUserData;
            if (userData != null)
            {
                var VsBufferDetectLangSID = new Guid("{17F375AC-C814-11d1-88AD-0000F87579D2}"); //GUID_VsBufferDetectLangSID;
                VSErrorHandler.ThrowOnFailure(userData.SetData(ref VsBufferDetectLangSID, false));
            }

            var langSid = CommonPackageConstants.xmlEditorLanguageService;
            VSErrorHandler.ThrowOnFailure(buffer.SetLanguageServiceID(ref langSid));

            var persistDocData = buffer as IVsPersistDocData;
            if (persistDocData != null)
            {
                persistDocData.LoadDocData(FileName);
                var artifactUri = new Uri(FileName);

                var artifact = PackageManager.Package.ModelManager.GetArtifact(artifactUri);
                if (artifact != null
                    && artifact.IsCodeGenArtifact)
                {
                    var standaloneProvider = artifact.XmlModelProvider as StandaloneXmlModelProvider;
                    if (standaloneProvider.ExtensionErrors == null
                        || standaloneProvider.ExtensionErrors.Count == 0)
                    {
                        // If there is a cached code gen artifact, it will have loaded its text buffer using extensions already.
                        // Therefore we can grab the text buffer from that artifact for our docdata buffer, and dispose the
                        // code gen artifact since it's using a XmlProvider that is standalone and won't be supported by the
                        // designer.
                        var projectItem = VsUtils.GetProjectItem(Hierarchy, ItemId);
                        if (projectItem != null)
                        {
                            if (VSHelpers.CheckOutFilesIfEditable(ServiceProvider, new[] { FileName }))
                            {
                                string artifactText = null;
                                using (var writer = new Utf8StringWriter())
                                {
                                    artifact.XDocument.Save(writer, SaveOptions.None);
                                    artifactText = writer.ToString();
                                }

                                if (!String.IsNullOrWhiteSpace(artifactText))
                                {
                                    VsUtils.SetTextForVsTextLines(VsBuffer, artifactText);
                                }
                            }
                            else
                            {
                                ErrorListHelper.LogExtensionErrors(
                                    new List<ExtensionError>
                                        {
                                            new ExtensionError(
                                                string.Format(
                                                    CultureInfo.CurrentCulture, Resources.ExtensionError_SourceControlLock,
                                                    Path.GetFileName(FileName)),
                                                ErrorCodes.ExtensionsError_BufferNotEditable,
                                                ExtensionErrorSeverity.Error)
                                        },
                                    projectItem);
                            }
                        }

                        PackageManager.Package.ModelManager.ClearArtifact(artifactUri);
                    }
                    else
                    {
                        // If the extensions ran into errors whilst loading, we'll need to re-run extensions anyway so we ignore the cache
                        PackageManager.Package.ModelManager.ClearArtifact(artifactUri);
                        DispatchLoadToExtensions();
                    }
                }
                else
                {
                    DispatchLoadToExtensions();
                }
            }

            // DSL exposes the FileNameChanged event which we subscribe to so we can update the moniker inside
            // the text buffer, which is required to update our model as well as keep the XmlModel in sync.
            FileNameChanged += OnFileNameChanged;
            RegisterUndoTracking();
            return true;
        }

        private void DispatchLoadToExtensions()
        {
            if (Hierarchy != null)
            {
                var projectItem = VsUtils.GetProjectItem(Hierarchy, ItemId);
                if (projectItem != null)
                {
                    var fileContents = VSHelpers.GetTextFromVsTextLines(VsBuffer);
                    string newBufferContents;
                    List<ExtensionError> extensionErrors;
                    if (StandaloneXmlModelProvider.TryGetBufferViaExtensions(
                        projectItem, fileContents, out newBufferContents, out extensionErrors))
                    {
                        if (VSHelpers.CheckOutFilesIfEditable(ServiceProvider, new[] { FileName }))
                        {
                            VsUtils.SetTextForVsTextLines(VsBuffer, newBufferContents);
                        }
                        else
                        {
                            ErrorListHelper.LogExtensionErrors(
                                new List<ExtensionError>
                                    {
                                        new ExtensionError(
                                            string.Format(
                                                CultureInfo.CurrentCulture, Resources.ExtensionError_SourceControlLock,
                                                Path.GetFileName(FileName)),
                                            ErrorCodes.ExtensionsError_BufferNotEditable,
                                            ExtensionErrorSeverity.Error)
                                    },
                                projectItem);
                        }
                    }
                }
            }
        }

        private string DispatchSaveToExtensions(string fileContents)
        {
            Debug.Assert(fileContents != null, "fileContents != null");

            // see if any extensions want to participate in saving
            if (Hierarchy != null)
            {
                var serializers = EscherExtensionPointManager.LoadModelTransformExtensions();
                var converters = EscherExtensionPointManager.LoadModelConversionExtensions();

                var projectItem = VsUtils.GetProjectItem(Hierarchy, ItemId);

                if ((serializers.Length > 0 || converters.Length > 0)
                    && projectItem != null
                    && VsUtils.EntityFrameworkSupportedInProject(
                        projectItem.ContainingProject, PackageManager.Package, allowMiscProject: false))
                {
                    return DispatchSaveToExtensions(PackageManager.Package, projectItem, fileContents, converters, serializers);
                }
            }

            return fileContents;
        }

        // internal for testing
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal string DispatchSaveToExtensions(
            IServiceProvider serviceProvider, ProjectItem projectItem, string fileContents,
            Lazy<IModelConversionExtension, IEntityDesignerConversionData>[] converters,
            Lazy<IModelTransformExtension>[] serializers)
        {
            Debug.Assert(projectItem != null, "projectItem != null");
            Debug.Assert(fileContents != null, "bufferText != null");
            Debug.Assert(serializers != null && converters != null, "extensions must not be null");
            Debug.Assert(serializers.Any() || converters.Any(), "at least one extension expected");

            ModelTransformContextImpl transformContext = null;
            ModelConversionContextImpl conversionContext = null;

            try
            {
                var original = XDocument.Parse(fileContents, LoadOptions.PreserveWhitespace);
                var targetSchemaVersion = EdmUtils.GetEntityFrameworkVersion(projectItem.ContainingProject, serviceProvider);
                Debug.Assert(targetSchemaVersion != null, "we should not get here for Misc projects");

                transformContext = new ModelTransformContextImpl(projectItem, targetSchemaVersion, original);

                // call the extensions that can save EDMX files first (even if we aren't going to end up in an EDMX file, let them process)
                VSArtifact.DispatchToSerializationExtensions(serializers, transformContext, loading: false);

                // get the extension of the file being loaded (might not be EDMX); this API will include the preceeding "."
                var fileInfo = new FileInfo(FileName);
                var fileExtension = fileInfo.Extension;

                // now if this is not an EDMX file, hand off to the extension who can convert it to the writable content
                if (!string.Equals(fileExtension, EntityDesignArtifact.ExtensionEdmx, StringComparison.OrdinalIgnoreCase))
                {
                    // the current document coming from the serializers becomes our original
                    conversionContext = new ModelConversionContextImpl(
                        projectItem.ContainingProject, projectItem, fileInfo, targetSchemaVersion, transformContext.CurrentDocument);

                    // we aren't loading an EDMX file, so call the extensions who can process this file extension
                    // when this finishes, then output should be a valid EDMX document
                    VSArtifact.DispatchToConversionExtensions(converters, fileExtension, conversionContext, false);

                    // we are done saving, so get bufferText from the OriginalDocument
                    // TODO use Utf8StringWriter here somehow?
                    return conversionContext.OriginalDocument;
                }
                else
                {
                    // we are saving an EDMX file, so get bufferText from the XDocument
                    using (var writer = new Utf8StringWriter())
                    {
                        transformContext.CurrentDocument.Save(writer, SaveOptions.None);
                        return writer.ToString();
                    }
                }
            }
            catch (XmlException)
            {
                // Don't do anything here. We will want to gracefully step out of the extension loading
                // and let the core designer handle this.
                return fileContents;
            }
            finally
            {
                var errorList = ErrorListHelper.GetExtensionErrorList(serviceProvider);
                errorList.Clear();

                // log any errors
                if (conversionContext != null)
                {
                    if (conversionContext.Errors.Count > 0)
                    {
                        ErrorListHelper.LogExtensionErrors(conversionContext.Errors, projectItem);
                    }
                    conversionContext.Dispose();
                }

                if (transformContext != null)
                {
                    if (transformContext.Errors.Count > 0)
                    {
                        ErrorListHelper.LogExtensionErrors(transformContext.Errors, projectItem);
                    }
                    transformContext.Dispose();
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.TextManager.Interop.IVsChangeTrackingUndoManager.AdviseTrackingClient(Microsoft.VisualStudio.TextManager.Interop.IVsUndoTrackingEvents)")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.TextManager.Interop.IVsChangeTrackingUndoManager.MarkCleanState")]
        private void RegisterUndoTracking()
        {
            IOleUndoManager undoManager = null;
            if (VSUndoManager != null)
            {
                undoManager = VSUndoManager;
            }
            // In order to track when our document returns to the clean state after an undo,
            // we need to Advise for IVsUndoTrackingEvents. This is how the "remove modified 
            // star after undo" feature works.
            var changeTrackingUndoMgr = (VSTextManagerInterop.IVsChangeTrackingUndoManager)undoManager;
            if (changeTrackingUndoMgr != null)
            {
                changeTrackingUndoMgr.MarkCleanState();
                changeTrackingUndoMgr.AdviseTrackingClient(this);
            }
        }

        /// <summary>
        ///     Implementing IVsUndoTrackingEvents
        /// </summary>
        public void OnReturnToCleanState()
        {
            var artifact = PackageManager.Package.ModelManager.GetArtifact(Utils.FileName2Uri(FileName));
            if (artifact != null)
            {
                artifact.IsDirty = false;
                SetDocDataDirty(0);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.TextManager.Interop.IVsChangeTrackingUndoManager.UnadviseTrackingClient")]
        private void UnregisterUndoTracking()
        {
            IOleUndoManager undoManager = null;
            if (VSUndoManager != null)
            {
                undoManager = VSUndoManager;
            }
            if (undoManager != null)
            {
                var changeTrackingUndoMgr = (VSTextManagerInterop.IVsChangeTrackingUndoManager)undoManager;
                if (changeTrackingUndoMgr != null)
                {
                    changeTrackingUndoMgr.UnadviseTrackingClient();
                }
            }
        }

        public string GetBufferTextForSaving()
        {
            return
                DispatchSaveToExtensions(
                    VSHelpers.GetTextFromVsTextLines(VsBuffer));
        }

        protected override void Save(string fileName)
        {
            // Make sure that we set the root element to the current active view's root element before we save.
            IServiceProvider serviceProvider = PackageManager.Package;
            var selectionService = serviceProvider.GetService(typeof(IMonitorSelectionService)) as IMonitorSelectionService;
            var dataEntityDesignDocView = selectionService.CurrentDocumentView as MicrosoftDataEntityDesignDocView;

            // if the current active view is not Entity designer or the view belongs to another document, just get the first view available.
            if (dataEntityDesignDocView == null
                || dataEntityDesignDocView.DocData != this)
            {
                dataEntityDesignDocView = DocViews.OfType<MicrosoftDataEntityDesignDocView>().FirstOrDefault();
            }

            Debug.Assert(dataEntityDesignDocView != null, "There is no active DocView associated for the DocData.");
            if (dataEntityDesignDocView != null)
            {
                SetRootElement(dataEntityDesignDocView.Diagram.ModelElement);
            }

            base.Save(fileName);

            ProcessDependentTTFiles();
        }

        protected override bool BackupFile(string backupFileName)
        {
            _backupFileName = backupFileName;
            return base.BackupFile(backupFileName);
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsQueryEditQuerySave2.OnAfterSaveUnreloadableFile(System.String,System.UInt32,Microsoft.VisualStudio.Shell.Interop.VSQEQS_FILE_ATTRIBUTE_DATA[])")]
        protected override void OnDocumentLoaded()
        {
            base.OnDocumentLoaded();

            if (Model != null
                && Model.IsDesignerSafe)
            {
                // The code below is to make sure that there at least 1 diagram created for the model.
                EnsureDiagramIsCreated(Model);
            }

            // call IVsQueryEditQuerySave2.OnAfterSaveUnreloadableFile() otherwise VS thinks the
            // underlying file is dirty and you get the "conflicting modifications detected" message
            var queryEditQuerySave2 = PackageManager.Package.GetService(typeof(SVsQueryEditQuerySave)) as IVsQueryEditQuerySave2;
            if (queryEditQuerySave2 != null)
            {
                queryEditQuerySave2.OnAfterSaveUnreloadableFile(FileName, 0, null);
            }

            // this handler notifies us when DSL has committed a transaction
            EventHandler<TransactionCommitEventArgs> eventHandler = OnTransactionCommitted;
            Store.EventManagerDirectory.TransactionCommitted.Add(eventHandler);

            // this lets us hook into the DSL mechanism and maybe stop a transaction from committing
            //this.Store.TransactionManager.AddCanCommitCallback(CanCommitTransaction);

            // flush it or else it will look like the doc is dirty right after loading
            FlushUndoManager();

            // Set the instance of this class in DiagramManagerContextItem.
            EditingContext.Items.GetValue<DiagramManagerContextItem>().SetViewManager(this);
        }

        public void EnsureDiagramIsCreated(EFArtifact artifact)
        {
            if (RootElement != null)
            {
                var modelRoot = RootElement as EntityDesignerViewModel;
                if (modelRoot != null)
                {
                    var diagram = modelRoot.GetDiagram();
                    Debug.Assert(diagram != null, "DSL Diagram should have been created by now");

                    if (diagram != null)
                    {
                        Debug.Assert(artifact.DesignerInfo() != null, "artifact.DesignerInfo should not be null");
                        Debug.Assert(artifact.DesignerInfo().Diagrams != null, "artifact.DesignerInfo.Diagrams should not be null");

                        if (artifact.DesignerInfo() != null
                            && artifact.DesignerInfo().Diagrams != null)
                        {
                            var saveDiagramDocument = false;

                            if (artifact.DesignerInfo().Diagrams.FirstDiagram == null)
                            {
                                // layout is very slow.  Only auto-layout if less than a max number of types
                                if (diagram.ModelElement.EntityTypes.Count < EntityDesignerDiagram.IMPLICIT_AUTO_LAYOUT_CEILING)
                                {
                                    diagram.AutoLayoutDiagram();
                                }

                                try
                                {
                                    EntityDesignerViewModel.RespondToModelChanges = false;
                                    EntityModelToDslModelTranslatorStrategy.CreateDefaultDiagram(modelRoot.EditingContext, diagram);

                                    // Ensure that DSL Diagram and Model Diagram are in sync:
                                    // - Update xref between 2 diagrams
                                    // - Propagetes model diagram info to DSL Diagram.
                                    var modelDiagram = artifact.DesignerInfo().Diagrams.FirstDiagram;
                                    Debug.Assert(modelDiagram != null, "modelDiagram should not be null");
                                    if (modelDiagram != null)
                                    {
                                        ModelTranslatorContextItem.GetEntityModelTranslator(EditingContext)
                                            .SynchronizeSingleDslModelElement(modelRoot, modelDiagram);
                                    }
                                }
                                finally
                                {
                                    EntityDesignerViewModel.RespondToModelChanges = true;
                                }

                                // save the file after adding Diagram element so it won't open as a dirty document
                                saveDiagramDocument = true;
                            }

                            if (saveDiagramDocument)
                            {
                                var rdt = new RunningDocumentTable(ServiceProvider);
                                rdt.SaveFileIfDirty(FileName);
                            }

                            _isDiagramLoaded = true;
                        }
                    }
                }
            }
        }

        protected override int LoadDocData(string fileName, bool isReload)
        {
            EntityDesignerViewModel.EntityShapeLocationSeed = 0;
            var ret = base.LoadDocData(fileName, isReload);

            if (UndoManager != null)
            {
                // Set the buffer's IOleUndoManager on XmlModelProvider, so each time a Tx Commits using XmlStore, an UndoUnit will be 
                // pushed onto the stack.  This is currently neccessary if you want XmlEditor to fire UndoRedoCompleted Event.
                var artifact = EditingContextManager.GetArtifact(EditingContext);
                Debug.Assert(artifact != null, "artifact should not be null");
                if (artifact != null)
                {
                    var xmlModelProvider = artifact.XmlModelProvider as VSXmlModelProvider;
                    Debug.Assert(xmlModelProvider != null, "Unexpected xml model provider type is being used with VS implementation");
                    if (xmlModelProvider != null)
                    {
                        // Attempt to get the undo manager from the buffer and disable it.
                        // This is because this undo manager is different from the undo manager that is used by the Entity Designer (ParentUndoManager)
                        // In linked transactions this can cause a problem since there are two undo managers that are getting rolled back, and the
                        // XML Model ends up thinking the parse tree is out of sync with the buffer. We should not have to explicitly roll back the buffer
                        // since modifications from our undo manager wrap XML model parse tree modifications, which when rolled back will be committed
                        // to the buffer.
                        IOleUndoManager bufferUndoMgr;
                        var hr = ((VSTextManagerInterop.IVsTextBuffer)VsBuffer).GetUndoManager(out bufferUndoMgr);

                        Debug.Assert(bufferUndoMgr != null, "Couldn't find the buffer undo manager. Undo/Redo will be disabled");
                        if (NativeMethods.Succeeded(hr) && bufferUndoMgr != null)
                        {
                            xmlModelProvider.UndoManager = new ParentUndoManager(VSUndoManager);
                            bufferUndoMgr.DiscardFrom(null);
                            bufferUndoMgr.Enable(0);
                        }

                        Store.UndoManager.UndoState = UndoState.Disabled;
                    }
                }
            }
            return ret;
        }

        protected override void OnDocumentReloading(EventArgs e)
        {
            base.OnDocumentReloading(e);

            // When document is about to be reloaded, we need to close the artifact since the underlying vs text buffer will be destroyed.
            // During document reloading, LoadDocData and LoadView methods will be called which recreate a new vs text buffer, the Escher model and the DSL model elements.
            if (EditingContext != null)
            {
                var artifactService = EditingContext.GetEFArtifactService();

                Debug.Assert(artifactService != null && artifactService.Artifact != null, "Why there is no active artifact?");
                if (artifactService != null
                    && artifactService.Artifact != null)
                {
                    var contextManager = EditingContextManager;
                    if (contextManager != null)
                    {
                        contextManager.CloseArtifact(artifactService.Artifact.Uri);
                    }
                }
            }
        }

        protected override void OnDocumentReloaded(EventArgs e)
        {
            _isHandlingDocumentReloaded = true;

            try
            {
                base.OnDocumentReloaded(e);

                var uri = Utils.FileName2Uri(FileName);

                var artifact = PackageManager.Package.ModelManager.GetArtifact(uri);
                artifact.FireArtifactReloadedEvent();

                PackageManager.Package.DocumentFrameMgr.SetItemForActiveFrame(uri);

                FlushUndoManager();
            }
            finally
            {
                _isHandlingDocumentReloaded = false;
            }
        }

        /// <summary>
        ///     DSL exposes the FileNameChanged event which we subscribe to so we can update the moniker inside
        ///     the text buffer, which is required to update our model as well as keep the XmlModel in sync.
        ///     NOTE: this function will ONLY get called when the document is open in the designer (when there is
        ///     a document data open in the running document table)
        /// </summary>
        protected void OnFileNameChanged(object sender, EventArgs e)
        {
            var buffer = _underlyingBuffer as VSTextManagerInterop.IVsTextLines;
            Debug.Assert(buffer != null, "There is no underlying buffer in order to correctly change the filename");
            if (buffer != null)
            {
                var ud = buffer as VSTextManagerInterop.IVsUserData;
                Debug.Assert(
                    ud != null, "Cannot change the moniker associated with the buffer because there is no IVsUserData associated with it");
                if (ud != null)
                {
                    var GUID_VsBufferMoniker = typeof(VSTextManagerInterop.IVsUserData).GUID;

                    // get the old filename from the buffer
                    object oldFileNameObject;
                    var hr = ud.GetData(ref GUID_VsBufferMoniker, out oldFileNameObject);
                    if (NativeMethods.Succeeded(hr))
                    {
                        // set the new filename into the buffer so the XmlModel is satisfied when it tries to RegisterAndLock
                        hr = ud.SetData(ref GUID_VsBufferMoniker, FileName);

                        // send off notification to the package so anything else can subscribe to an event from there
                        var oldFileName = oldFileNameObject as string;
                        if (PackageManager.Package != null
                            && NativeMethods.Succeeded(hr)
                            && oldFileName != null)
                        {
                            PackageManager.Package.OnFileNameChanged(oldFileName, FileName);
                        }
                    }
                }
            }
        }

        protected void OnTransactionCommitted(Object sender, TransactionCommitEventArgs e)
        {
            MicrosoftDataEntityDesignDocView dataEntityDesignDocView = null;

            // Need to figure the diagram view where the transaction originates.
            // First we look at transaction context for diagram id information.
            // If this information is not available then we are going to look at the current active window.
            var diagramId = string.Empty;
            if (e.TransactionContext.TryGetValue(EfiTransactionOriginator.TransactionOriginatorDiagramId, out diagramId))
            {
                foreach (var view in DocViews.OfType<MicrosoftDataEntityDesignDocView>())
                {
                    if (view.Diagram.DiagramId == diagramId)
                    {
                        dataEntityDesignDocView = view;
                        break;
                    }
                }
            }
            else
            {
                // look at the current selection
                // Note: must use CurrentDocumentView rather than CurrentWindow, CurrentWindow can be e.g. the MappingDetailsWindow
                IServiceProvider serviceProvider = PackageManager.Package;
                var selectionService = serviceProvider.GetService(typeof(IMonitorSelectionService)) as IMonitorSelectionService;
                dataEntityDesignDocView = selectionService.CurrentDocumentView as MicrosoftDataEntityDesignDocView;
            }

            // When a new diagram is created, the doc view is not created yet when transaction is committed.
            // In that situation, we just skip delegating the transaction to DSL view model because there is none.
            if (dataEntityDesignDocView != null
                && dataEntityDesignDocView.IsLoading == false)
            {
                var viewModel = dataEntityDesignDocView.Diagram.ModelElement as EntityDesignerViewModel;
                if (viewModel != null)
                {
                    viewModel.OnTransactionCommited(e);

                    // now set the isDirty flag on Shell's UndoManager so diagram layout changes can 
                    // also get persisted; if there isn't one of our context's in the xact, then this
                    // change didn't involve changing any items, just position or size
                    var changeContext = ViewModelChangeContext.GetExistingContext(e.Transaction);
                    if (changeContext == null)
                    {
                        if (IsHandlingDocumentReloaded == false)
                        {
                            SetDocDataDirty(1);
                        }
                    }
                    else
                    {
                        // if we get here, there are changes that originated in the designer surface
                        // so run our validation
                        if (Store != null)
                        {
                            ValidationController.ValidateCustom(Store, "OnTransactionCommited");
                        }
                    }
                }
            }
        }

        // this function is fired only for top most transactions
        //private bool CanCommitTransaction(Transaction tx)
        //{
        //    Debug.Assert(tx != null, "tx should not be null");
        //    if (tx.Context.ContextInfo.ContainsKey(EntityDesignerViewModel.TransactionCancelledGuid))
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        /// <summary>
        ///     Loads the text into the buffer. This method will allocate a VSTextManagerInterop::IVsTextLines will be created
        ///     and loaded with the model
        /// </summary>
        /// <param name="ppTextBuffer">newly created buffer loaded with the model file (in XML string form)</param>
        /// <returns>VSConstants.S_OK if operation successful</returns>
        public override int GetTextBuffer(out VSTextManagerInterop.IVsTextLines ppTextBuffer)
        {
            if (VsBuffer == null)
            {
                if (!CreateAndLoadBuffer())
                {
                    ppTextBuffer = null;
                    return VSConstants.E_FAIL;
                }
            }

            ppTextBuffer = VsBuffer;
            return VSConstants.S_OK;
        }

        public override int LockTextBuffer(int fLock)
        {
            var hr = VSConstants.S_OK;
            if (VsBuffer != null)
            {
                hr = VsBuffer.LockBuffer();
            }
            return hr;
        }

        public override int SetTextBuffer(VSTextManagerInterop.IVsTextLines pTextBuffer)
        {
            var hr = VSConstants.S_OK;
            _underlyingBuffer = pTextBuffer;
            return hr;
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Boolean.TryParse(System.String,System.Boolean@)")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void ProcessDependentTTFiles()
        {
            // check if template processing was turned off via designer options
            var processTemplates = true;
            var artifact = PackageManager.Package.ModelManager.GetArtifact(Utils.FileName2Uri(FileName));
            if (artifact != null)
            {
                DesignerInfo designerInfo;
                if (artifact.DesignerInfo().TryGetDesignerInfo(OptionsDesignerInfo.ElementName, out designerInfo))
                {
                    var optionsDesignerInfo = designerInfo as OptionsDesignerInfo;
                    Debug.Assert(
                        optionsDesignerInfo != null,
                        "DesignerInfo associated with " + OptionsDesignerInfo.ElementName + "must be of type OptionsDesignerInfo");
                    if (optionsDesignerInfo != null
                        && optionsDesignerInfo.ProcessDependentTemplatesOnSave != null
                        && optionsDesignerInfo.ProcessDependentTemplatesOnSave.ValueAttr != null)
                    {
                        bool.TryParse(optionsDesignerInfo.ProcessDependentTemplatesOnSave.ValueAttr.Value, out processTemplates);
                    }
                }
            }

            // if template processing was turned off, return
            if (processTemplates == false)
            {
                return;
            }

            // Find all .tt files in the project and invoke custom Tool 
            var fileFinder = new VSFileFinder(".tt");
            fileFinder.FindInProject(Hierarchy);
            foreach (var vsFileInfo in fileFinder.MatchingFiles)
            {
                if (IsDependentTTFile(vsFileInfo))
                {
                    var pi = VsUtils.GetProjectItem(vsFileInfo.Hierarchy, vsFileInfo.ItemId);
                    Debug.Assert(pi != null, "Couldn't find project item, but file was discovered by VSFileFinder");
                    if (pi != null)
                    {
                        try
                        {
                            VsUtils.RunCustomTool(pi);
                        }
                        catch (Exception e)
                        {
                            // Swallow exceptions here, since we don't want a custom tool to interfere with the file save.
                            var errorMsg = String.Format(
                                CultureInfo.CurrentCulture, Resources.ErrorOccurredRunningCustomTool, vsFileInfo.Path, e.Message);
                            VsUtils.LogOutputWindowPaneMessage(VSHelpers.GetProject(Hierarchy), errorMsg);
                        }
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private bool IsDependentTTFile(VSFileFinder.VSFileInfo fileInfo)
        {
            string contents = null;
            var docData = VSHelpers.GetDocData(ServiceProvider, fileInfo.Path);
            if (docData == null)
            {
                // load file from disk
                try
                {
                    contents = File.ReadAllText(fileInfo.Path);
                }
                catch (Exception e)
                {
                    var errorMsg = String.Format(CultureInfo.CurrentCulture, Resources.ErrorOccurredLoadingFile, fileInfo.Path, e.Message);
                    VsUtils.LogOutputWindowPaneMessage(VSHelpers.GetProject(Hierarchy), errorMsg);
                    contents = null;
                }
            }
            else
            {
                // read buffer contents
                var tl = VSHelpers.GetVsTextLinesFromDocData(docData);
                Debug.Assert(tl != null, "Couldn't get text lines from doc data for .tt file");
                if (tl != null)
                {
                    contents = VSHelpers.GetTextFromVsTextLines(tl);
                }
            }

            //
            // BUGBUG 592011:  replace this with something smarter.  Use T4 parser to identify all input files.
            //
            if (contents != null)
            {
                var fi = new FileInfo(FileName);
                return contents.Contains(fi.Name);
            }
            else
            {
                return false;
            }
        }

        // There is a generated DSL code that will try to lock the diagram file if exists.
        // We don't want this to happen because an exception will be thrown when trying to load the diagram in XMLModel.
        // DSL looks for a diagram file that is named this.Name + “.” + DiagramExtension.  
        // By setting the DiagramExtension value to “notused”, DSL won’t find the file and it won’t be locked.   
        // We overwrite the extension to trick the DSL to think that the diagram file does not exist.
        protected override string DiagramExtension
        {
            get { return "notused"; }
        }

        // This is queried during transations to update the dirty marker.
        public override int IsDocDataDirty(out int isDirty)
        {
            if (Model != null
                && Model.IsDirty)
            {
                isDirty = 1;
                return VSConstants.S_OK;
            }

            return base.IsDocDataDirty(out isDirty);
        }

        public void EnableDiagramEdits(bool canEdit)
        {
            var modelPartition = GetModelPartition();
            if (canEdit)
            {
                if (modelPartition != null
                    && modelPartition.GetLocks() != Locks.None)
                {
                    modelPartition.SetLocks(Locks.None);
                }
            }
            else
            {
                if (modelPartition != null
                    && modelPartition.GetLocks() != Locks.All)
                {
                    modelPartition.SetLocks(Locks.All);
                }
            }
        }

        #region IDiagramManager interface implementation

        // Currently openInNewTab flag is not used since a new diagram will alway be opened in a new tab.
        // But we would like to support open in new table vs open in existing tab scenario in the future.
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame.Show")]
        public void OpenDiagram(string diagramMoniker, bool openInNewTab)
        {
            // first check if the doc view for the diagram is available
            foreach (var docView in DocViews)
            {
                var dataEntityDesignDocView = docView as MicrosoftDataEntityDesignDocView;
                if (dataEntityDesignDocView.Diagram.DiagramId == diagramMoniker)
                {
                    dataEntityDesignDocView.Frame.Show();
                    return;
                }
            }
            OpenView(PackageConstants.guidLogicalView, diagramMoniker);
        }

        public IViewDiagram ActiveDiagram
        {
            get
            {
                var selectionService = Services.DslMonitorSelectionService;
                Debug.Assert(selectionService != null, "Could not retrieve IMonitorSelectionService from Escher package.");
                if (selectionService != null)
                {
                    return GetDiagramFromDocView(selectionService.CurrentDocumentView);
                }
                return null;
            }
        }

        public IViewDiagram FirstOpenDiagram
        {
            get
            {
                IVsUIHierarchy hier;
                uint itemId;
                IVsWindowFrame pFrame;
                if (VsShellUtilities.IsDocumentOpen(
                    Services.ServiceProvider, FileName, PackageConstants.guidLogicalView, out hier, out itemId, out pFrame))
                {
                    object docViewObj;
                    if (VSConstants.S_OK == pFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docViewObj))
                    {
                        return GetDiagramFromDocView(docViewObj);
                    }
                }
                return null;
            }
        }

        public IEnumerable<IViewDiagram> OpenDiagrams
        {
            get
            {
                foreach (var docView in DocViews.ToList())
                {
                    yield return GetDiagramFromDocView(docView);
                }
            }
        }

        private static IViewDiagram GetDiagramFromDocView(object docViewObject)
        {
            var singleDiagramDocView = docViewObject as SingleDiagramDocView;

            if (singleDiagramDocView != null)
            {
                return singleDiagramDocView.Diagram as IViewDiagram;
            }
            return null;
        }

        /// <summary>
        ///     Close window for diagram with the specific id.
        /// </summary>
        /// <param name="diagramMoniker"></param>
        public void CloseDiagram(string diagramMoniker)
        {
            foreach (var docView in DocViews)
            {
                var dataEntityDesignDocView = docView as MicrosoftDataEntityDesignDocView;
                Debug.Assert(dataEntityDesignDocView != null, "DocView is null or not typeof MicrosoftDataEntityDesignDocView");
                if (dataEntityDesignDocView != null
                    && dataEntityDesignDocView.Diagram.DiagramId == diagramMoniker)
                {
                    CloseDocViewWindow(dataEntityDesignDocView);
                    return;
                }
            }
        }

        /// <summary>
        ///     Close all doc-view windows.
        /// </summary>
        public void CloseAllDiagrams()
        {
            foreach (var docView in DocViews.ToList())
            {
                CloseDocViewWindow(docView as MicrosoftDataEntityDesignDocView);
            }
        }

        /// <summary>
        ///     Close DocView's windows frame.
        ///     Prompt the user to save the document if the last diagram is closed and doc data is dirty.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame.CloseFrame(System.UInt32)")]
        private void CloseDocViewWindow(MicrosoftDataEntityDesignDocView docView)
        {
            Debug.Assert(docView != null, "docView parameter is null");
            if (docView != null)
            {
                var isDirty = 0;
                IsDocDataDirty(out isDirty);
                if (DocViews.Count == 1
                    && isDirty == 1)
                {
                    docView.Frame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_PromptSave);
                }
                else
                {
                    docView.Frame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
                }
            }
        }

        #endregion

        int IVsHasRelatedSaveItems.GetRelatedSaveTreeItems(
            VSSAVETREEITEM saveItem, uint celt, VSSAVETREEITEM[] rgSaveTreeItems, out uint pcActual)
        {
            pcActual = 0;

            return VSConstants.S_OK;
        }
    }
}
