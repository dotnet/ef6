// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Extensibility;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Microsoft.VisualStudio.Shell;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal class VSArtifact : EntityDesignArtifact
    {
        private HashSet<string> _namespaces;
        private LayerManager _layerManager;

        internal event EventHandler<EventArgs> AfterLoadedArtifact;

        internal override EditingContext EditingContext
        {
            get { return PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(Uri); }
        }

        internal LayerManager LayerManager
        {
            get { return _layerManager; }
        }

        /// <summary>
        ///     Creates an instance of an EFArtifact for use inside Visual Studio
        /// </summary>
        /// <param name="modelManager">A reference of ModelManager</param>
        /// <param name="uri">The URI to the EDMX file that this artifact will load</param>
        /// <param name="xmlModelProvider">We are ignoring this parameter, sending null to base class so that it will call CreateModelProvider()</param>
        internal VSArtifact(ModelManager modelManager, Uri uri, XmlModelProvider xmlModelProvider)
            : base(modelManager, uri, xmlModelProvider)
        {
        }

        internal override void Init()
        {
            base.Init();
            _layerManager = new LayerManager(this);
            AddEventHandler();
        }

        internal override void FireArtifactReloadedEvent()
        {
            var context = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(Uri);
            Debug.Assert(context != null, "context should not be null");
            if (context != null)
            {
                context.OnReloaded(EventArgs.Empty);
            }
        }

        internal override void OnLoaded()
        {
            base.OnLoaded();

            // Register the artifact into the ModelBus if it is resolved.
            if (State == EFElementState.Resolved)
            {
                if (AfterLoadedArtifact != null)
                {
                    AfterLoadedArtifact(this, null);
                }
            }
        }

        protected override void OnBeforeHandleXmlModelTransactionCompleted(object sender, XmlTransactionEventArgs args)
        {
            base.OnBeforeHandleXmlModelTransactionCompleted(sender, args);

#if DEBUG
            var rDT = new RunningDocumentTable(PackageManager.Package);
            uint cookie = 0;
            rDT.FindDocument(Uri.LocalPath, out cookie);

            var info = rDT.GetDocumentInfo(cookie);
            Debug.Print(
                string.Format(
                    CultureInfo.CurrentCulture, "There are now {0} Edit Locks, and {1} Read Locks.", info.EditLocks, info.ReadLocks));
#endif
        }

        protected internal override HashSet<string> GetNamespaces()
        {
            if (_namespaces == null)
            {
                _namespaces = new HashSet<string>();
                foreach (var n in SchemaManager.GetAllNamespacesForVersion(SchemaVersion))
                {
                    _namespaces.Add(n);
                }
            }

            return _namespaces;
        }

        /// <summary>
        ///     Ensure we have the correct provider loaded before we reload
        /// </summary>
        internal override void ReloadArtifact()
        {
            VsUtils.EnsureProvider(this);
            base.ReloadArtifact();
        }

        /// <summary>
        ///     This will do analysis to determine if a document should be opened
        ///     only in the XmlEditor.
        /// </summary>
        internal override void DetermineIfArtifactIsDesignerSafe()
        {
            VsUtils.EnsureProvider(this);

            base.DetermineIfArtifactIsDesignerSafe();
            //
            // TODO:  we need to figure out how to deal with errors from the wizard. 
            //        when we clear the error list below, we lose errors that we put into the error
            //        list when running the wizard.  
            // 

            //
            // Now update the VS error list with all of the errors we want to display, which are now in the EFArtifactSet.  
            //
            var errorInfos = ArtifactSet.GetAllErrors();
            if (errorInfos.Count > 0)
            {
                var currentProject = VSHelpers.GetProjectForDocument(Uri.LocalPath, PackageManager.Package);
                if (currentProject != null)
                {
                    var hierarchy = VsUtils.GetVsHierarchy(currentProject, Services.ServiceProvider);
                    if (hierarchy != null)
                    {
                        var fileFinder = new VSFileFinder(Uri.LocalPath);
                        fileFinder.FindInProject(hierarchy);

                        Debug.Assert(fileFinder.MatchingFiles.Count <= 1, "Unexpected count of matching files in project");

                        // if the EDMX file is not part of the project.
                        if (fileFinder.MatchingFiles.Count == 0)
                        {
                            var docData = VSHelpers.GetDocData(PackageManager.Package, Uri.LocalPath) as IEntityDesignDocData;
                            ErrorListHelper.AddErrorInfosToErrorList(errorInfos, docData.Hierarchy, docData.ItemId);
                        }
                        else
                        {
                            foreach (var vsFileInfo in fileFinder.MatchingFiles)
                            {
                                if (vsFileInfo.Hierarchy == VsUtils.GetVsHierarchy(currentProject, Services.ServiceProvider))
                                {
                                    var errorList = ErrorListHelper.GetSingleDocErrorList(vsFileInfo.Hierarchy, vsFileInfo.ItemId);
                                    if (errorList != null)
                                    {
                                        errorList.Clear();
                                        ErrorListHelper.AddErrorInfosToErrorList(errorInfos, vsFileInfo.Hierarchy, vsFileInfo.ItemId);
                                    }
                                    else
                                    {
                                        Debug.Fail("errorList is null!");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal override bool IsXmlValid()
        {
            // If there is a VSXmlModelProvider, we should be able to find a docdata for it.
            // In any other case, it doesn't matter whether there is document data or not.
            var docData = VSHelpers.GetDocData(PackageManager.Package, Uri.LocalPath);
            Debug.Assert(
                !(XmlModelProvider is VSXmlModelProvider) || docData != null, "Using a VSXmlModelProvider but docData is null for Artifact!");

            try
            {
                XmlDocument xmldoc;
                if (docData != null)
                {
                    var textLines = VSHelpers.GetVsTextLinesFromDocData(docData);
                    Debug.Assert(textLines != null, "Failed to get IVSTextLines from docdata");

                    xmldoc = EdmUtils.SafeLoadXmlFromString(VSHelpers.GetTextFromVsTextLines(textLines));
                }
                else
                {
                    // If there is no docdata then attempt to create the XmlDocument from the internal
                    // XLinq tree in the artifact
                    xmldoc = new XmlDocument();
                    xmldoc.Load(XDocument.CreateReader());
                }
                // For the most part, the Edmx schema version of an artifact should be in sync with the schema version 
                // that is compatible with the project's target framework; except when the user adds an existing edmx to a project (the version could be different).
                // For all cases, we always want to validate using the XSD's version that matches the artifact's version.
                var documentSchemaVersion = base.SchemaVersion;
                Debug.Assert(
                    EntityFrameworkVersion.IsValidVersion(documentSchemaVersion),
                    "The EF Schema Version is not valid. Value:"
                    + (documentSchemaVersion != null ? documentSchemaVersion.ToString() : "null"));

                // does the XML parse? If not, the load call below will throw
                if (EntityFrameworkVersion.IsValidVersion(documentSchemaVersion))
                {
                    var nsMgr = SchemaManager.GetEdmxNamespaceManager(xmldoc.NameTable, documentSchemaVersion);
                    // Do XSD validation on the document.
                    xmldoc.Schemas = EscherAttributeContentValidator.GetInstance(documentSchemaVersion).EdmxSchemaSet;
                    var svec = new SchemaValidationErrorCollector();

                    // remove runtime specific lines
                    // find the ConceptualModel Schema node
                    RemoveRunTimeNode(xmldoc, "/edmx:Edmx/edmx:Configurations", nsMgr);
                    RemoveRunTimeNode(xmldoc, "/edmx:Edmx/edmx:Runtime/edmx:ConceptualModels", nsMgr);
                    RemoveRunTimeNode(xmldoc, "/edmx:Edmx/edmx:Runtime/edmx:StorageModels", nsMgr);
                    RemoveRunTimeNode(xmldoc, "/edmx:Edmx/edmx:Runtime/edmx:Mappings", nsMgr);

                    xmldoc.Validate(svec.ValidationCallBack);

                    return svec.ErrorCount == 0;
                }
            }
            catch
            {
            }

            return false;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    RemoveEventHandler();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        internal override LangEnum LanguageForCodeGeneration
        {
            get
            {
                var project = VSHelpers.GetProjectForDocument(Uri.LocalPath, PackageManager.Package);
                return VsUtils.GetLanguageForProject(project);
            }
        }

        internal override void DetermineIfArtifactIsVersionSafe()
        {
            // We want to move the user to the latest possible schemas - so if a user opens a v2 edmx
            // file in a project that has a reference to an EF assembly that can handle v3 schema we 
            // won't display the model but will show a watermark saying "please upgrade your schema"
            // There are two exceptions to this rule:
            // - a user opens an edmx without a project (a.k.a. Misc project) in that case we always show
            //   the model
            // - a user is targeting .NET Framework 4 and has references to both System.Data.Entity.dll 
            //   and EF6 EntityFramework.dll in which case we allow opening both v2 and v3 edmx files

            var project = GetProject();
            Debug.Assert(project != null);

            IsVersionSafe =
                VsUtils.EntityFrameworkSupportedInProject(project, ServiceProvider, allowMiscProject: true) &&
                VsUtils.SchemaVersionSupportedInProject(project, SchemaVersion, ServiceProvider);

            if (IsVersionSafe)
            {
                base.DetermineIfArtifactIsVersionSafe();
            }
        }

        // needed for mocking
        protected virtual IServiceProvider ServiceProvider
        {
            get { return PackageManager.Package; }
        }

        // needed for mocking
        protected virtual Project GetProject()
        {
            return VSHelpers.GetProjectForDocument(Uri.LocalPath, ServiceProvider);
        }

        private static void RemoveRunTimeNode(XmlDocument xmlDoc, string xpath, XmlNamespaceManager xmlNsm)
        {
            try
            {
                var runtimeNode = (XmlElement)xmlDoc.SelectSingleNode(xpath, xmlNsm);
                if (runtimeNode != null
                    && runtimeNode.ParentNode != null)
                {
                    runtimeNode.ParentNode.RemoveChild(runtimeNode);
                }
            }
            catch (XPathException)
            {
                Debug.Fail("The XPath expression contains a prefix which is not defined in the XmlNamespaceManager.");
            }
            catch (ArgumentException)
            {
                Debug.Fail("The oldChild is not a child of this node. Or this node is read-only. ");
            }
        }

        private EventHandler<XObjectChangeEventArgs> _beforeEvent;

        private EventHandler<XObjectChangeEventArgs> BeforeEventHandler
        {
            get
            {
                if (_beforeEvent == null)
                {
                    _beforeEvent = OnBeforeChange;
                }
                return _beforeEvent;
            }
        }

        private void OnBeforeChange(object sender, XObjectChangeEventArgs e)
        {
            if (XmlModelProvider.CurrentTransaction == null)
            {
                //throw new InvalidOperationException(Resources.ChangingModelOutsideTransaction);
            }
        }

        protected override void OnAfterHandleXmlModelTransactionCompleted(
            object sender, XmlTransactionEventArgs xmlTransactionEventArgs, EfiChangeGroup changeGroup)
        {
            base.OnAfterHandleXmlModelTransactionCompleted(sender, xmlTransactionEventArgs, changeGroup);

            Debug.Assert(_layerManager != null, "LayerManager must not be null");
            if (_layerManager != null)
            {
                var changes = from ixc in xmlTransactionEventArgs.Transaction.Changes()
                              select new Tuple<XObject, XObjectChange>(ixc.Node, ixc.Action);
                _layerManager.OnAfterTransactionCommitted(changes);
            }
        }

        private void AddEventHandler()
        {
            if (XDocument != null)
            {
                XDocument.Changing += BeforeEventHandler;
            }
        }

        private void RemoveEventHandler()
        {
            if (XDocument != null)
            {
                XDocument.Changing -= BeforeEventHandler;
            }
        }

        internal override HashSet<string> GetFileExtensions()
        {
            return GetVSArtifactFileExtensions();
        }

        private static HashSet<string> _artifactFileExtensions;

        internal static HashSet<string> GetVSArtifactFileExtensions()
        {
            if (_artifactFileExtensions == null)
            {
                _artifactFileExtensions =
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    { EXTENSION_EDMX, EXTENSION_DIAGRAM };

                // add any other extensions registered by converters
                foreach (var exportInfo in EscherExtensionPointManager.LoadModelConversionExtensions())
                {
                    var fileExtension = exportInfo.Metadata.FileExtension;

                    // ensure that the extension starts with a '.'
                    if (fileExtension.StartsWith(".", StringComparison.Ordinal) == false)
                    {
                        fileExtension = "." + fileExtension;
                    }

                    // add it if it isn't already in the list (duplicates will be checked for during load/save)
                    if (_artifactFileExtensions.Contains(fileExtension) == false)
                    {
                        _artifactFileExtensions.Add(fileExtension);
                    }
                }
            }
            return _artifactFileExtensions;
        }

        internal static void DispatchToSerializationExtensions(
            ICollection<Lazy<IModelTransformExtension>> exports, ModelTransformExtensionContext context, bool loading)
        {
            if (exports != null)
            {
                foreach (var exportInfo in exports)
                {
                    var extension = exportInfo.Value;
                    if (loading)
                    {
                        extension.OnAfterModelLoaded(context);
                    }
                    else
                    {
                        extension.OnBeforeModelSaved(context);
                    }
                }
            }
        }

        internal static void DispatchToConversionExtensions(
            ICollection<Lazy<IModelConversionExtension, IEntityDesignerConversionData>> exports, string fileExtension,
            ModelConversionExtensionContext context, bool loading)
        {
            var converters = new List<string>();

            if (exports != null)
            {
                foreach (var exportInfo in exports)
                {
                    var converterFileExtension = exportInfo.Metadata.FileExtension;
                    if (converterFileExtension != null
                        && !converterFileExtension.StartsWith(".", StringComparison.Ordinal))
                    {
                        converterFileExtension = "." + converterFileExtension;
                    }

                    if (string.Equals(fileExtension, converterFileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        if (converters.Count > 0)
                        {
                            break;
                        }

                        var extension = exportInfo.Value;
                        if (loading)
                        {
                            extension.OnAfterFileLoaded(context);
                        }
                        else
                        {
                            extension.OnBeforeFileSaved(context);
                        }

                        converters.Add(extension.GetType().FullName);
                    }
                }
            }

            if (converters.Count == 0)
            {
                throw new InvalidOperationException(Resources.Extensibility_NoConverterForExtension);
            }
            else if (converters.Count > 1)
            {
                var convs = string.Empty;
                for (var i = 0; i < converters.Count; i++)
                {
                    if (i != 0)
                    {
                        convs += ", ";
                    }
                    convs += converters[i];
                }

                var message = string.Format(CultureInfo.CurrentCulture, Resources.Extensibility_TooManyConverters, convs);
                throw new InvalidOperationException(message);
            }
        }

        internal override List<EdmSchemaError> GetModelGenErrors()
        {
            return PackageManager.Package.ModelGenErrorCache.GetErrors(Uri.LocalPath);
        }
    }
}
