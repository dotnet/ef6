// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Entity.Design.Model.Visitor;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;

    internal class EntityDesignArtifact : EFArtifact
    {
        #region Fields

        internal static readonly string ExtensionMsl = ".msl";
        internal static readonly string ExtensionCsdl = ".csdl";
        internal static readonly string ExtensionSsdl = ".ssdl";
        internal static readonly string ExtensionEdmx = ".edmx";
        internal static readonly string ExtensionDiagram = ".diagram";

        // designer models
        private EFDesignerInfoRoot _designerInfoRoot;

        // runtime models
        private MappingModel _mappingModel;
        private ConceptualEntityModel _conceptualEntityModel;
        private StorageEntityModel _storageEntityModel;

        // this will be set to true if the file contains DataServices element
        private bool _dataServicesNodePresent;

        /// <summary>
        ///     True if this artifact is free from errors that prevent it from being used in the designer
        /// </summary>
        private bool _isStructurallySafe;

        /// <summary>
        ///     True if this artifact's XMLNS values are consistent with the desireed EDMX version.
        /// </summary>
        private bool _isVersionSafe;

        #endregion  Fields

        #region Properties

        internal virtual EditingContext EditingContext
        {
            get { return null; }
        }

        // virtual to allow mocking
        internal virtual EFDesignerInfoRoot DesignerInfo
        {
            get { return _designerInfoRoot; }
        }

        // virtual to allow mocking
        internal virtual ConceptualEntityModel ConceptualModel
        {
            get { return _conceptualEntityModel; }
            set { _conceptualEntityModel = value; }
        }

        // virtual to allow mocking
        internal virtual StorageEntityModel StorageModel
        {
            get { return _storageEntityModel; }
            set { _storageEntityModel = value; }
        }

        // virtual to allow mocking
        internal virtual MappingModel MappingModel
        {
            get { return _mappingModel; }
            set { _mappingModel = value; }
        }

        internal override IEnumerable<EFObject> Children
        {
            get
            {
                if (_designerInfoRoot != null)
                {
                    yield return _designerInfoRoot;
                }

                if (_mappingModel != null)
                {
                    yield return _mappingModel;
                }

                if (_conceptualEntityModel != null)
                {
                    yield return _conceptualEntityModel;
                }

                if (_storageEntityModel != null)
                {
                    yield return _storageEntityModel;
                }
            }
        }

        internal bool DataServicesNodePresent
        {
            get { return _dataServicesNodePresent; }
        }

        internal override Version SchemaVersion
        {
            get { return SchemaManager.GetSchemaVersion(GetRootNamespace()); }
        }

        internal bool IsStructurallySafe
        {
            get { return _isStructurallySafe; }
        }

        internal bool IsVersionSafe
        {
            get { return _isVersionSafe; }
            set { _isVersionSafe = value; }
        }

        /// <summary>
        ///     If Diagram information is stored in a separate file, the information will be stored in DiagramArtifact.
        /// </summary>
        internal DiagramArtifact DiagramArtifact { get; set; }

        #endregion  Properties

        /// <summary>
        ///     Constructs an EntityDesignArtifact for the passed in URI
        /// </summary>
        /// <param name="modelManager">A reference of ModelManager</param>
        /// <param name="uri">The URI to the EDMX file that this artifact will load</param>
        /// <param name="xmlModelProvider">If you pass null, then you must derive from this class and implement CreateModelProvider().</param>
        internal EntityDesignArtifact(ModelManager modelManager, Uri uri, XmlModelProvider xmlModelProvider)
            : base(modelManager, uri, xmlModelProvider)
        {
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_designerInfoRoot != null)
                    {
                        _designerInfoRoot.Dispose();
                        _designerInfoRoot = null;
                    }

                    if (_mappingModel != null)
                    {
                        _mappingModel.Dispose();
                        _mappingModel = null;
                    }

                    if (_conceptualEntityModel != null)
                    {
                        _conceptualEntityModel.Dispose();
                        _conceptualEntityModel = null;
                    }

                    if (_storageEntityModel != null)
                    {
                        _storageEntityModel.Dispose();
                        _storageEntityModel = null;
                    }

                    if (DiagramArtifact != null)
                    {
                        ModelManager.ClearArtifact(DiagramArtifact.Uri);
                        DiagramArtifact = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        internal override void Parse(ICollection<XName> unprocessedElements)
        {
            State = EFElementState.ParseAttempted;

            var path = Uri.LocalPath;
            var lastdot = path.LastIndexOf('.');
            var extension = Uri.LocalPath.Substring(lastdot, path.Length - lastdot);

            if (extension.EndsWith(ExtensionMsl, StringComparison.OrdinalIgnoreCase))
            {
                _mappingModel = new MappingModel(this, XDocument.Root);
            }
            else if (extension.EndsWith(ExtensionCsdl, StringComparison.OrdinalIgnoreCase))
            {
                _conceptualEntityModel = new ConceptualEntityModel(this, XDocument.Root);
            }
            else if (extension.EndsWith(ExtensionSsdl, StringComparison.OrdinalIgnoreCase))
            {
                _storageEntityModel = new StorageEntityModel(this, XDocument.Root);
            }
            else if (GetFileExtensions().Contains(extension))
            {
                if (_designerInfoRoot != null)
                {
                    _designerInfoRoot.Dispose();
                    _designerInfoRoot = null;
                }

                if (_storageEntityModel != null)
                {
                    _storageEntityModel.Dispose();
                    _storageEntityModel = null;
                }

                if (_mappingModel != null)
                {
                    _mappingModel.Dispose();
                    _mappingModel = null;
                }

                if (_conceptualEntityModel != null)
                {
                    _conceptualEntityModel.Dispose();
                    _conceptualEntityModel = null;
                }

                // convert the xlinq tree to our model
                foreach (var elem in XObject.Document.Elements())
                {
                    ParseSingleElement(unprocessedElements, elem);
                }
            }

            if (_designerInfoRoot != null)
            {
                _designerInfoRoot.Parse(unprocessedElements);
            }

            if (_conceptualEntityModel != null)
            {
                _conceptualEntityModel.Parse(unprocessedElements);
            }

            if (_storageEntityModel != null)
            {
                _storageEntityModel.Parse(unprocessedElements);
            }

            if (_mappingModel != null)
            {
                _mappingModel.Parse(unprocessedElements);
            }

            State = EFElementState.Parsed;
        }

        /// <summary>
        ///     Reload the EntityDesignArtifact and DiagramArtifact (if available).
        /// </summary>
        internal override void ReloadArtifact()
        {
            try
            {
                IsArtifactReloading = true;

                // clear out the artifact set of our information
                ArtifactSet.RemoveArtifact(this);
                ArtifactSet.Add(this);

                // Reparse the artifact.
                State = EFElementState.None;
                Parse(new List<XName>());
                if (State == EFElementState.Parsed)
                {
                    XmlModelHelper.NormalizeAndResolve(this);
                }

                // NOTE: DiagramArtifact must be reloaded after EntityDesignArtifact finishes reloading but before we fire artifact reloaded event.
                if (DiagramArtifact != null)
                {
                    DiagramArtifact.ReloadArtifact();
                }

                // this will do some analysis to determine if the artifact is safe for the designer, or should be displayed in the xml editor
                DetermineIfArtifactIsDesignerSafe();

                FireArtifactReloadedEvent();

                RequireDelayedReload = false;
                IsDirty = false;
            }
            finally
            {
                IsArtifactReloading = false;
            }
        }

        private bool CheckForCorrectNamespace(XElement element, string[] expectedNamespace)
        {
            return CheckForCorrectNamespace(element, expectedNamespace, true);
        }

        private bool CheckForCorrectNamespace(XElement element, string[] expectedNamespace, bool addParseErrorOnFailure)
        {
            if (element == null
                || element.Name == null
                || element.Name.Namespace == null)
            {
                return false;
            }

            var foundMatch = false;
            foreach (var namespaceName in expectedNamespace)
            {
                if (element.Name.NamespaceName == namespaceName)
                {
                    foundMatch = true;
                    break;
                }
            }

            if (!foundMatch)
            {
                // add an error & return false. 
                if (addParseErrorOnFailure)
                {
                    var msg = String.Format(CultureInfo.CurrentCulture, Resources.ModelParse_NonQualifiedElement, element.Name.LocalName);
                    var error = new ErrorInfo(ErrorInfo.Severity.ERROR, msg, this, ErrorCodes.NON_QUALIFIED_ELEMENT, ErrorClass.ParseError);
                    AddParseErrorForObject(this, error);
                }
                return false;
            }

            return true;
        }

        internal override bool ParseSingleElement(ICollection<XName> unprocessedElements, XElement elem)
        {
            if (elem.Name.LocalName == "Edmx")
            {
                if (!CheckForCorrectNamespace(elem, SchemaManager.GetEDMXNamespaceNames()))
                {
                    return false;
                }

                var runtimeElementProcessed = false;
                var designerElementProcessed = false;
                foreach (var elem2 in elem.Elements())
                {
                    if (elem2.Name.LocalName == "Runtime")
                    {
                        if (!CheckForCorrectNamespace(elem2, SchemaManager.GetEDMXNamespaceNames()))
                        {
                            continue;
                        }

                        if (runtimeElementProcessed)
                        {
                            var msg = String.Format(CultureInfo.CurrentCulture, Resources.DuplicatedElementMsg, elem2.Name.LocalName);
                            var error = new ErrorInfo(
                                ErrorInfo.Severity.ERROR, msg, this, ErrorCodes.DUPLICATED_ELEMENT_ENCOUNTERED, ErrorClass.ParseError);
                            AddParseErrorForObject(this, error);
                            continue;
                        }
                        runtimeElementProcessed = true;

                        var conceptualModelsProcessed = false;
                        var storageModelsProcessed = false;
                        var mappingsProcessed = false;
                        foreach (var elem3 in elem2.Elements())
                        {
                            ParseSingleEntityModelElement(
                                elem3, ref conceptualModelsProcessed, ref storageModelsProcessed, ref mappingsProcessed);
                        }
                    }
                    else if (elem2.Name.LocalName == "Designer")
                    {
                        if (!CheckForCorrectNamespace(elem2, SchemaManager.GetEDMXNamespaceNames()))
                        {
                            continue;
                        }

                        if (designerElementProcessed)
                        {
                            var msg = String.Format(CultureInfo.CurrentCulture, Resources.DuplicatedElementMsg, elem2.Name.LocalName);
                            var error = new ErrorInfo(
                                ErrorInfo.Severity.ERROR, msg, this, ErrorCodes.DUPLICATED_ELEMENT_ENCOUNTERED, ErrorClass.ParseError);
                            AddParseErrorForObject(this, error);
                            continue;
                        }
                        designerElementProcessed = true;

                        ParseDesignerInfoRoot(elem2);
                    }
                    else if (elem2.Name.LocalName == "DataServices")
                    {
                        if (!CheckForCorrectNamespace(elem2, SchemaManager.GetEDMXNamespaceNames()))
                        {
                            continue;
                        }

                        _dataServicesNodePresent = true;
                        var error = new ErrorInfo(
                            ErrorInfo.Severity.WARNING, Resources.DataServicesNodeWarning, this, ErrorCodes.DATA_SERVICES_NODE_DETECTED,
                            ErrorClass.ParseError);
                        AddParseErrorForObject(this, error);
                    }
                    else
                    {
                        var msg = String.Format(CultureInfo.CurrentCulture, Resources.UnexpectedElementMsg, elem2.Name.LocalName);
                        var error = new ErrorInfo(
                            ErrorInfo.Severity.ERROR, msg, this, ErrorCodes.UNEXPECTED_ELEMENT_ENCOUNTERED, ErrorClass.ParseError);
                        AddParseErrorForObject(this, error);
                        continue;
                    }
                }
                return true;
            }
            else
            {
                var msg = String.Format(CultureInfo.CurrentCulture, Resources.UnexpectedElementMsg, elem.Name.LocalName);
                var error = new ErrorInfo(
                    ErrorInfo.Severity.ERROR, msg, this, ErrorCodes.UNEXPECTED_ELEMENT_ENCOUNTERED, ErrorClass.ParseError);
                AddParseErrorForObject(this, error);
                return false;
            }
        }

        internal override bool ReparseSingleElement(ICollection<XName> unprocessedElements, XElement element)
        {
            if (element.Name.LocalName == BaseEntityModel.ElementName
                || element.Name.LocalName == MappingModel.ElementName)
            {
                // Update Model sometimes just replaces the Schema elements; if we undo this, we'll have to
                // parse a new Schema element, so find the entity model that contains it and parse it

                var entityModelXObj = element.Parent;
                Debug.Assert(
                    entityModelXObj != null
                    && (entityModelXObj.Name.LocalName == "ConceptualModels" || entityModelXObj.Name.LocalName == "StorageModels"
                        || entityModelXObj.Name.LocalName == "Mappings"),
                    "How could a Schema element be added underneath a parent that is not a ConceptualModel, StorageModel, or Mapping?");

                var conceptualModelsProcessed = false;
                var storageModelsProcessed = false;
                var mappingsProcessed = false;

                ParseSingleEntityModelElement(
                    entityModelXObj, ref conceptualModelsProcessed, ref storageModelsProcessed, ref mappingsProcessed);

                if (conceptualModelsProcessed)
                {
                    Debug.Assert(ConceptualModel != null, "If the conceptual model root was created, why isn't it in this artifact?");
                    ConceptualModel.Parse(new List<XName>());
                }
                else if (storageModelsProcessed)
                {
                    Debug.Assert(StorageModel != null, "If the storage model root was created, why isn't it in this artifact?");
                    StorageModel.Parse(new List<XName>());
                }
                else if (mappingsProcessed)
                {
                    Debug.Assert(MappingModel != null, "If the mappings root was created, why isn't it in this artifact?");
                    MappingModel.Parse(new List<XName>());
                }
                return true;
            }
            else
            {
                return base.ReparseSingleElement(unprocessedElements, element);
            }
        }

        internal void ParseSingleEntityModelElement(
            XElement entityModelXElement, ref bool conceptualModelsProcessed, ref bool storageModelsProcessed, ref bool mappingsProcessed)
        {
            if (!CheckForCorrectNamespace(entityModelXElement, SchemaManager.GetEDMXNamespaceNames()))
            {
                return;
            }

            if (entityModelXElement.Name.LocalName == "ConceptualModels")
            {
                if (conceptualModelsProcessed)
                {
                    var msg = String.Format(CultureInfo.CurrentCulture, Resources.DuplicatedElementMsg, entityModelXElement.Name.LocalName);
                    var error = new ErrorInfo(
                        ErrorInfo.Severity.ERROR, msg, this, ErrorCodes.DUPLICATED_ELEMENT_ENCOUNTERED, ErrorClass.ParseError);
                    AddParseErrorForObject(this, error);
                    return;
                }
                conceptualModelsProcessed = true;

                ParseConceptualModels(entityModelXElement);
            }
            else if (entityModelXElement.Name.LocalName == "StorageModels")
            {
                if (storageModelsProcessed)
                {
                    var msg = String.Format(CultureInfo.CurrentCulture, Resources.DuplicatedElementMsg, entityModelXElement.Name.LocalName);
                    var error = new ErrorInfo(
                        ErrorInfo.Severity.ERROR, msg, this, ErrorCodes.DUPLICATED_ELEMENT_ENCOUNTERED, ErrorClass.ParseError);
                    AddParseErrorForObject(this, error);
                    return;
                }
                storageModelsProcessed = true;

                ParseStorageModels(entityModelXElement);
            }
            else if (entityModelXElement.Name.LocalName == "Mappings")
            {
                if (mappingsProcessed)
                {
                    var msg = String.Format(CultureInfo.CurrentCulture, Resources.DuplicatedElementMsg, entityModelXElement.Name.LocalName);
                    var error = new ErrorInfo(
                        ErrorInfo.Severity.ERROR, msg, this, ErrorCodes.DUPLICATED_ELEMENT_ENCOUNTERED, ErrorClass.ParseError);
                    AddParseErrorForObject(this, error);
                    return;
                }
                mappingsProcessed = true;

                ParseMappings(entityModelXElement);
            }
            else
            {
                var msg = String.Format(CultureInfo.CurrentCulture, Resources.UnexpectedElementMsg, entityModelXElement.Name.LocalName);
                var error = new ErrorInfo(
                    ErrorInfo.Severity.ERROR, msg, this, ErrorCodes.UNEXPECTED_ELEMENT_ENCOUNTERED, ErrorClass.ParseError);
                AddParseErrorForObject(this, error);
                return;
            }
        }

        internal EFRuntimeModelRoot CreateRuntimeModelRoot(XElement runtimeModelRoot)
        {
            if (runtimeModelRoot.Name.LocalName == BaseEntityModel.ElementName
                && CheckForCorrectNamespace(runtimeModelRoot, SchemaManager.GetCSDLNamespaceNames(), false))
            {
                if (_conceptualEntityModel != null)
                {
                    _conceptualEntityModel.Dispose();
                }
                _conceptualEntityModel = new ConceptualEntityModel(this, runtimeModelRoot);
                return _conceptualEntityModel;
            }
            else if (runtimeModelRoot.Name.LocalName == BaseEntityModel.ElementName
                     && CheckForCorrectNamespace(runtimeModelRoot, SchemaManager.GetSSDLNamespaceNames(), false))
            {
                if (_storageEntityModel != null)
                {
                    _storageEntityModel.Dispose();
                }
                _storageEntityModel = new StorageEntityModel(this, runtimeModelRoot);
                return _storageEntityModel;
            }
            else if (runtimeModelRoot.Name.LocalName == MappingModel.ElementName
                     && CheckForCorrectNamespace(runtimeModelRoot, SchemaManager.GetMSLNamespaceNames(), false))
            {
                if (_mappingModel != null)
                {
                    _mappingModel.Dispose();
                }
                _mappingModel = new MappingModel(this, runtimeModelRoot);
                return _mappingModel;
            }
            else
            {
                //Debug.Fail("Unexpected runtime model root");
                return null;
            }
        }

        private void ParseStorageModels(XElement storageModelsElement)
        {
            foreach (var elem in storageModelsElement.Elements())
            {
                if (elem.Name.LocalName == BaseEntityModel.ElementName)
                {
                    if (!CheckForCorrectNamespace(elem, SchemaManager.GetSSDLNamespaceNames()))
                    {
                        continue;
                    }

                    if (_storageEntityModel != null)
                    {
                        var msg = String.Format(CultureInfo.CurrentCulture, Resources.DuplicatedElementMsg, elem.Name.LocalName);
                        var error = new ErrorInfo(
                            ErrorInfo.Severity.ERROR, msg, this, ErrorCodes.DUPLICATED_ELEMENT_ENCOUNTERED, ErrorClass.ParseError);
                        AddParseErrorForObject(this, error);
                    }
                    else
                    {
                        CreateRuntimeModelRoot(elem);
                    }
                }
                else
                {
                    var msg = String.Format(CultureInfo.CurrentCulture, Resources.UnexpectedElementMsg, elem.Name.LocalName);
                    var error = new ErrorInfo(
                        ErrorInfo.Severity.ERROR, msg, this, ErrorCodes.UNEXPECTED_ELEMENT_ENCOUNTERED, ErrorClass.ParseError);
                    AddParseErrorForObject(this, error);
                }
            }
        }

        private void ParseDesignerInfoRoot(XElement designerInfoElement)
        {
            if (_designerInfoRoot != null)
            {
                _designerInfoRoot.Dispose();
            }
            _designerInfoRoot = new EFDesignerInfoRoot(this, designerInfoElement);
        }

        private void ParseMappings(XElement mappingsElement)
        {
            foreach (var elem in mappingsElement.Elements())
            {
                if (elem.Name.LocalName == MappingModel.ElementName)
                {
                    if (!CheckForCorrectNamespace(elem, SchemaManager.GetMSLNamespaceNames()))
                    {
                        continue;
                    }

                    if (_mappingModel != null)
                    {
                        var msg = String.Format(CultureInfo.CurrentCulture, Resources.DuplicatedElementMsg, elem.Name.LocalName);
                        var error = new ErrorInfo(
                            ErrorInfo.Severity.ERROR, msg, this, ErrorCodes.DUPLICATED_ELEMENT_ENCOUNTERED, ErrorClass.ParseError);
                        AddParseErrorForObject(this, error);
                    }
                    else
                    {
                        CreateRuntimeModelRoot(elem);
                    }
                }
                else
                {
                    var msg = String.Format(CultureInfo.CurrentCulture, Resources.UnexpectedElementMsg, elem.Name.LocalName);
                    var error = new ErrorInfo(
                        ErrorInfo.Severity.ERROR, msg, this, ErrorCodes.UNEXPECTED_ELEMENT_ENCOUNTERED, ErrorClass.ParseError);
                    AddParseErrorForObject(this, error);
                }
            }
        }

        private void ParseConceptualModels(XElement conceptualModelsElement)
        {
            foreach (var elem in conceptualModelsElement.Elements())
            {
                if (elem.Name.LocalName == BaseEntityModel.ElementName)
                {
                    if (!CheckForCorrectNamespace(elem, SchemaManager.GetCSDLNamespaceNames()))
                    {
                        continue;
                    }

                    if (_conceptualEntityModel != null)
                    {
                        var msg = String.Format(CultureInfo.CurrentCulture, Resources.DuplicatedElementMsg, elem.Name.LocalName);
                        var error = new ErrorInfo(
                            ErrorInfo.Severity.ERROR, msg, this, ErrorCodes.DUPLICATED_ELEMENT_ENCOUNTERED, ErrorClass.ParseError);
                        AddParseErrorForObject(this, error);
                    }
                    else
                    {
                        CreateRuntimeModelRoot(elem);
                    }
                }
                else
                {
                    var msg = String.Format(CultureInfo.CurrentCulture, Resources.UnexpectedElementMsg, elem.Name.LocalName);
                    var error = new ErrorInfo(
                        ErrorInfo.Severity.ERROR, msg, this, ErrorCodes.UNEXPECTED_ELEMENT_ENCOUNTERED, ErrorClass.ParseError);
                    AddParseErrorForObject(this, error);
                }
            }
        }

        protected override void OnChildDeleted(EFContainer efContainer)
        {
            if (efContainer == _designerInfoRoot)
            {
                _designerInfoRoot = null;
            }
            else if (efContainer == _mappingModel)
            {
                _mappingModel = null;
            }
            else if (efContainer == _conceptualEntityModel)
            {
                _conceptualEntityModel = null;
            }
            else if (efContainer == _storageEntityModel)
            {
                _storageEntityModel = null;
            }

            base.OnChildDeleted(efContainer);
        }

#if DEBUG
        protected override VerifyModelIntegrityVisitor GetVerifyModelIntegrityVisitor()
        {
            return new VerifyEscherModelIntegrityVisitor();
        }

        internal override VerifyModelIntegrityVisitor GetVerifyModelIntegrityVisitor(
            bool checkDisposed, bool checkUnresolved, bool checkXObject, bool checkAnnotations, bool checkBindingIntegrity)
        {
            return new VerifyEscherModelIntegrityVisitor(
                checkDisposed, checkUnresolved, checkXObject, checkAnnotations, checkBindingIntegrity);
        }
#endif

        /// <summary>
        ///     Determines if the designer is safe to be viewed and also if it's safe to have edits made to it. By reference artifacts aren't safe to
        ///     edit when their references are invalid since they wouldn't be synced together. Note this is different from the EFArtifact.CanEditArtifact()
        ///     method which is about checking whether the artifact is locked in source code control
        /// </summary>
        internal bool IsDesignerSafeAndEditSafe()
        {
            return IsDesignerSafe;
        }

        /// <summary>
        ///     This will do analysis to determine if a document should be opened
        ///     only in the XmlEditor.
        /// </summary>
        internal override void DetermineIfArtifactIsDesignerSafe()
        {
            DetermineIfArtifactIsVersionSafe();
            DetermineIfArtifactIsStructurallySafe();
            IsDesignerSafe = IsVersionSafe && IsStructurallySafe;
        }

        /// <summary>
        ///     This will do analysis to determine if a document should be opened
        ///     only in the XmlEditor.
        /// </summary>
        internal void DetermineIfArtifactIsStructurallySafe()
        {
            // XSD prevents an EDMX from having both a DataServices node and a Runtime node
            // don't validate DataServices EDMX files
            if (DataServicesNodePresent)
            {
                _isStructurallySafe = false;
                return;
            }

            // reset the value to true first - then determine if not safe below.
            _isStructurallySafe = true;
            var artifactSet = (EntityDesignArtifactSet)ArtifactSet;

            // Do escher validation.  Doing this lets us determine if we want to keep MSL validation errors
            // in the artifact set, or just ignore them.
            EscherModelValidator.ValidateEscherModel(artifactSet, true);

            var shouldValidateMappings = artifactSet.ShouldDoRuntimeMappingValidation();

            // now do runtime validation.  Always pass true to do mapping validation since we need these errors to determine safe-mode
            // use the schema version of this artifact as the target Entity Framework version
            new RuntimeMetadataValidator(ModelManager, SchemaVersion, DependencyResolver.Instance)
                .Validate(artifactSet);

            // scan all of the validation errors to see if we need to go into safe mode
            foreach (var ei in artifactSet.GetAllErrors())
            {
                if (EscherModelValidator.IsOpenInEditorError(ei)
                    || RuntimeMetadataValidator.IsOpenInEditorError(ei, this))
                {
                    _isStructurallySafe = false;
                }
            }

            // the XML editor will "fix-up" parser errors for us, so we parse again just to be sure that there are none
            if (_isStructurallySafe && IsXmlValid() == false)
            {
                _isStructurallySafe = false;
            }
            else if (DesignerInfo == null
                     || DesignerInfo.Diagrams == null)
            {
                // if the edmx file is missing Designer or Diagrams element it's not designer safe
                _isStructurallySafe = false;
            }
            else if (ConceptualModel == null
                     || ConceptualModel.EntityContainerCount > 1)
            {
                // if the edmx file is missing ConceptualModel or it has multiple EntityContainers it's not designer safe
                _isStructurallySafe = false;
            }
            else if (MappingModel == null
                     || StorageModel == null)
            {
                // if the edmx file is missing MappingModel or StorageModel it's not designer safe
                _isStructurallySafe = false;
            }

            //
            // now we decide if we need to clear out the MSL errors from runtime validation above
            //
            if (shouldValidateMappings == false && _isStructurallySafe)
            {
                artifactSet.ClearErrors(ErrorClass.Runtime_MSL);
                artifactSet.ClearErrors(ErrorClass.Runtime_ViewGen);
            }
        }

        internal virtual void DetermineIfArtifactIsVersionSafe()
        {
            // make sure that the XML namespace of the EDMX, Csdl, ssdl & msl nodes match the expected XML namespaces for the schema version 
            _isVersionSafe = GetRootNamespace() == SchemaManager.GetEDMXNamespaceName(SchemaVersion)
                             && CompareNamespaces(ConceptualModel, SchemaManager.GetCSDLNamespaceName(SchemaVersion))
                             && CompareNamespaces(StorageModel, SchemaManager.GetSSDLNamespaceName(SchemaVersion))
                             && CompareNamespaces(MappingModel, SchemaManager.GetMSLNamespaceName(SchemaVersion));
        }

        internal virtual bool IsXmlValid()
        {
            // since the xml editor will fix-up parser errors, we can't detect if the xml will parse.  This method  is 
            // overriden in VSArtifact to see if the xml will parse.
            return true;
        }

        private static bool CompareNamespaces(EFRuntimeModelRoot rootNode, XNamespace xnamespace)
        {
            Debug.Assert(xnamespace != null, "xnamespace != null");
            Debug.Assert(rootNode == null || rootNode.XNamespace != null, "invalid model root namespace");

            return rootNode == null || rootNode.XNamespace == xnamespace;
        }

        /// <summary>
        ///     Retrives the namespace of the root elemnt of the document.  This should be either an EDMX, CSDL, SSDL or MSL namespace URI.
        /// </summary>
        /// <returns></returns>
        internal XNamespace GetRootNamespace()
        {
            return XDocument.Root.Name.Namespace;
        }

        internal virtual HashSet<string> GetFileExtensions()
        {
            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            extensions.Add(ExtensionEdmx);
            return extensions;
        }

        protected internal override bool ExpectEFObjectForXObject(XObject xobject)
        {
            var xe = xobject as XElement;
            if (xe != null)
            {
                foreach (var n in SchemaManager.GetEDMXNamespaceNames())
                {
                    // see if this element is the "Edmx" element.  We don't exepct a EFObject for this element
                    if (xe.Name.NamespaceName.Equals(n, StringComparison.OrdinalIgnoreCase))
                    {
                        if (xe.Name.LocalName.Equals("Edmx", StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }
                }

                // now look for items that are in the EF set of namespaces (ie, csdl/ssdl/msl/edmx namespaces.
                // If something isn't in this namespace, we return false below (since it won't have a model element)
                // if something is in this namespace, we'll base this to the base class, which will return true.
                //
                // the reason this is here is there will be asserts after undo/redo operations if we can't find
                // a model element for the xnode that is part of the undo/redo.  Returning false from this method
                // tells us not to expect the model element.
                //
                foreach (var v in EntityFrameworkVersion.GetAllVersions())
                {
                    foreach (var s in SchemaManager.GetAllNamespacesForVersion(v))
                    {
                        if (xe.Name.NamespaceName.Equals(s, StringComparison.OrdinalIgnoreCase))
                        {
                            // we only expect EFObjects for items in the Entity Framework namespaces
                            return base.ExpectEFObjectForXObject(xobject);
                        }
                    }
                }
            }

            return false;
        }

        internal virtual List<EdmSchemaError> GetModelGenErrors()
        {
            return null;
        }
    }
}
