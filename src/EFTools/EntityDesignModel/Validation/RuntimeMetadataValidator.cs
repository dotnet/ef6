// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.SchemaObjectModel;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.Data.Tools.XmlDesignerBase.Base.Util;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using NavigationProperty = Microsoft.Data.Entity.Design.Model.Entity.NavigationProperty;
    using ReferentialConstraint = Microsoft.Data.Entity.Design.Model.Entity.ReferentialConstraint;
    using AssociationSetMapping = Microsoft.Data.Entity.Design.Model.Mapping.AssociationSetMapping;
    using ModificationFunctionMapping = Microsoft.Data.Entity.Design.Model.Mapping.ModificationFunctionMapping;

    internal class RuntimeMetadataValidator
    {
        private readonly ModelManager _modelManager;
        private readonly Version _targetEntityFrameworkRuntimeVersion;
        private readonly IDbDependencyResolver _dependencyResolver;

        // TODO: targetEntityFrameworkRuntimeVersion is not correct and can be based on the target .NET Fx version or the namespace of the edmx file
        internal RuntimeMetadataValidator(
            ModelManager modelManager, Version targetEntityFrameworkRuntimeVersion, IDbDependencyResolver dependencyResolver)
        {
            Debug.Assert(modelManager != null, "modelManager != null");
            Debug.Assert(targetEntityFrameworkRuntimeVersion != null, "targetEntityFrameworkRuntimeVersion != null");

            _modelManager = modelManager;
            _targetEntityFrameworkRuntimeVersion = targetEntityFrameworkRuntimeVersion;
            _dependencyResolver = dependencyResolver;
        }

        public void Validate(EFArtifactSet artifactSet)
        {
            ValidateArtifactSet(artifactSet, /*forceValidation*/ true, /*validateMsl*/ true, /*runViewGen*/ false);
        }

        public void ValidateAndCompileMappings(EFArtifactSet artifactSet, bool validateMapping)
        {
            // validate each artifact set
            ValidateArtifactSet(artifactSet, /*forceValidation*/ false, /*validateMsl*/ validateMapping, /*runViewGen*/ validateMapping);
        }

        private static XmlReader CreateXmlReader(EFArtifact artifact, XElement xobject)
        {
            var baseReader = xobject.CreateReader();
            var lineNumberService = new XNodeReaderLineNumberService(artifact.XmlModelProvider, baseReader, artifact.Uri);
            return new XmlReaderProxy(baseReader, artifact.Uri, lineNumberService);
        }

        // internal for testing, virtual for mocking
        internal virtual void ValidateArtifactSet(EFArtifactSet artifactSet, bool forceValidation, bool validateMsl, bool runViewGen)
        {
            Debug.Assert(artifactSet != null, "artifactSet != null");
            Debug.Assert(!runViewGen || validateMsl, "ViewGen validation can only be performed if msl validation is requested");

            // First determine if we need to do validation
            if (!artifactSet.IsValidityDirtyForErrorClass(ErrorClass.Runtime_All)
                && !forceValidation)
            {
                return;
            }

            // If we need to do validation, first clear out any existing errors on the artifact set.
            artifactSet.ClearErrors(ErrorClass.Runtime_All);

            var designArtifact = artifactSet.GetEntityDesignArtifact();
            if (designArtifact != null)
            {
                var storeItemCollection = ValidateStoreModel(designArtifact);
                var edmItemCollection = ValidateConceptualModel(designArtifact);

                if (edmItemCollection != null
                    && storeItemCollection != null
                    && validateMsl)
                {
                    var mappingItemCollection = ValidateMapping(designArtifact, edmItemCollection, storeItemCollection);
                    if (mappingItemCollection != null && runViewGen)
                    {
                        ValidateWithViewGen(mappingItemCollection, designArtifact);
                    }
                }
            }
        }

        private EdmItemCollection ValidateConceptualModel(EntityDesignArtifact designArtifact)
        {
            Debug.Assert(designArtifact != null, "designArtifact != null");

            var artifactSet = designArtifact.ArtifactSet;

            if (designArtifact.ConceptualModel == null)
            {
                artifactSet.AddError(
                    new ErrorInfo(
                        ErrorInfo.Severity.ERROR,
                        Resources.ErrorValidatingArtifact_ConceptualModelMissing,
                        designArtifact,
                        ErrorCodes.ErrorValidatingArtifact_ConceptualModelMissing,
                        ErrorClass.Runtime_CSDL));

                return null;
            }

            if (SchemaManager.GetSchemaVersion(designArtifact.ConceptualModel.XElement.Name.Namespace)
                > _targetEntityFrameworkRuntimeVersion)
            {
                artifactSet.AddError(
                    new ErrorInfo(
                        ErrorInfo.Severity.ERROR,
                        Resources.ErrorValidatingArtifact_InvalidCSDLNamespaceForTargetFrameworkVersion,
                        designArtifact.ConceptualModel,
                        ErrorCodes.ErrorValidatingArtifact_InvalidCSDLNamespaceForTargetFrameworkVersion,
                        ErrorClass.Runtime_CSDL));

                return null;
            }

            using (var reader = CreateXmlReader(designArtifact, designArtifact.ConceptualModel.XElement))
            {
                IList<EdmSchemaError> modelErrors;
                var edmItemCollection = EdmItemCollection.Create(new[] { reader }, null, out modelErrors);

                Debug.Assert(modelErrors != null);

                ProcessErrors(modelErrors, designArtifact, ErrorClass.Runtime_CSDL);
                return edmItemCollection;
            }
        }

        private StoreItemCollection ValidateStoreModel(EntityDesignArtifact designArtifact)
        {
            Debug.Assert(designArtifact != null, "designArtifact != null");

            var artifactSet = designArtifact.ArtifactSet;

            if (designArtifact.StorageModel == null)
            {
                artifactSet.AddError(
                    new ErrorInfo(
                        ErrorInfo.Severity.ERROR,
                        Resources.ErrorValidatingArtifact_StorageModelMissing,
                        designArtifact,
                        ErrorCodes.ErrorValidatingArtifact_StorageModelMissing,
                        ErrorClass.Runtime_SSDL));

                return null;
            }

            if (SchemaManager.GetSchemaVersion(designArtifact.StorageModel.XElement.Name.Namespace) > _targetEntityFrameworkRuntimeVersion)
            {
                // the xml namespace of the ssdl Schema node is for a later version of the runtime than we are validating against
                artifactSet.AddError(
                    new ErrorInfo(
                        ErrorInfo.Severity.ERROR,
                        Resources.ErrorValidatingArtifact_InvalidSSDLNamespaceForTargetFrameworkVersion,
                        designArtifact.StorageModel,
                        ErrorCodes.ErrorValidatingArtifact_InvalidSSDLNamespaceForTargetFrameworkVersion,
                        ErrorClass.Runtime_CSDL));

                return null;
            }

            using (var reader = CreateXmlReader(designArtifact, designArtifact.StorageModel.XElement))
            {
                IList<EdmSchemaError> storeErrors;
                var storeItemCollection =
                    StoreItemCollection.Create(new[] { reader }, null, _dependencyResolver, out storeErrors);

                Debug.Assert(storeErrors != null);

                // also process cached errors and warnings (if any) from reverse engineering db
                ProcessErrors(
                    storeErrors.Concat(designArtifact.GetModelGenErrors() ?? Enumerable.Empty<EdmSchemaError>()),
                    designArtifact, ErrorClass.Runtime_SSDL);

                return storeItemCollection;
            }
        }

        private StorageMappingItemCollection ValidateMapping(
            EntityDesignArtifact designArtifact, EdmItemCollection edmItemCollection,
            StoreItemCollection storeItemCollection)
        {
            Debug.Assert(designArtifact != null, "designArtifact != null");
            Debug.Assert(edmItemCollection != null, "edmItemCollection != null");
            Debug.Assert(storeItemCollection != null, "storeItemCollection != null");

            var artifactSet = designArtifact.ArtifactSet;

            if (designArtifact.MappingModel == null)
            {
                artifactSet.AddError(
                    new ErrorInfo(
                        ErrorInfo.Severity.ERROR,
                        Resources.ErrorValidatingArtifact_MappingModelMissing,
                        designArtifact,
                        ErrorCodes.ErrorValidatingArtifact_MappingModelMissing,
                        ErrorClass.Runtime_MSL));

                return null;
            }

            if (SchemaManager.GetSchemaVersion(designArtifact.MappingModel.XElement.Name.Namespace) >
                _targetEntityFrameworkRuntimeVersion)
            {
                // the xml namespace of the mapping node is for a later version of the runtime than we are validating against

                artifactSet.AddError(
                    new ErrorInfo(
                        ErrorInfo.Severity.ERROR,
                        Resources.ErrorValidatingArtifact_InvalidMSLNamespaceForTargetFrameworkVersion,
                        designArtifact.MappingModel,
                        ErrorCodes.ErrorValidatingArtifact_InvalidMSLNamespaceForTargetFrameworkVersion,
                        ErrorClass.Runtime_MSL));

                return null;
            }

            using (var reader = CreateXmlReader(designArtifact, designArtifact.MappingModel.XElement))
            {
                IList<EdmSchemaError> mappingErrors;

                var mappingItemCollection =
                    StorageMappingItemCollection.Create(
                        edmItemCollection, storeItemCollection, new[] { reader }, null, out mappingErrors);

                Debug.Assert(mappingErrors != null);

                ProcessErrors(mappingErrors, designArtifact, ErrorClass.Runtime_MSL);
                return mappingItemCollection;
            }
        }

        private void ValidateWithViewGen(StorageMappingItemCollection mappingItemCollection, EntityDesignArtifact designArtifact)
        {
            Debug.Assert(mappingItemCollection != null, "mappingItemCollection != null");
            Debug.Assert(designArtifact != null, "designArtifact != null");

            var errors = new List<EdmSchemaError>();
            mappingItemCollection.GenerateViews(errors);

            ProcessErrors(errors, designArtifact, ErrorClass.Runtime_ViewGen);
        }

        // internal for testing
        internal void ProcessErrors(IEnumerable<EdmSchemaError> errors, EntityDesignArtifact defaultArtifactForError, ErrorClass errorClass)
        {
            Debug.Assert(errors != null, "errors != null");
            Debug.Assert(defaultArtifactForError != null, "defaultArtifactForError != null");

            var artifactSet = defaultArtifactForError.ArtifactSet;

            foreach (var error in errors)
            {
                var efObject = EdmSchemaError2EFObject(error, defaultArtifactForError);
                if (error.ErrorCode == (int)ErrorCode.NotInNamespace)
                {
                    // we want to replace runtime error for missing complex property type with ours. This
                    // is classified as a Runtime_CSDL error even though we are using an Escher error code
                    // since we are basically re-interpreting a runtime error.
                    var property = efObject as ComplexConceptualProperty;
                    if (property != null
                        && property.ComplexType.RefName == Resources.ComplexPropertyUndefinedType)
                    {
                        artifactSet.AddError(
                            new ErrorInfo(
                                ErrorInfo.Severity.ERROR,
                                string.Format(
                                    CultureInfo.CurrentCulture, Resources.EscherValidation_UndefinedComplexPropertyType,
                                    property.LocalName.Value),
                                property,
                                ErrorCodes.ESCHER_VALIDATOR_UNDEFINED_COMPLEX_PROPERTY_TYPE,
                                ErrorClass.Runtime_CSDL));
                        continue;
                    }
                }
                else if (error.ErrorCode == (int)MappingErrorCode.InvalidAssociationSet
                         && error.Severity == EdmSchemaErrorSeverity.Warning)
                {
                    // this is a warning about AssociationSetMappings on fk associations for pk-to-pk associations being ignored.
                    var associationSetMapping = efObject as AssociationSetMapping;
                    Debug.Assert(associationSetMapping != null, "Warning 2005 reported on EFObject other than Association Set Mapping");
                    if (associationSetMapping != null)
                    {
                        artifactSet.AddError(
                            new ErrorInfo(
                                GetErrorInfoSeverity(error),
                                string.Format(
                                    CultureInfo.CurrentCulture, Resources.EscherValidation_IgnoreMappedFKAssociation,
                                    associationSetMapping.Name.RefName),
                                efObject,
                                error.ErrorCode,
                                errorClass));
                        continue;
                    }
                }

                var severity =
                    error.ErrorCode == (int)MappingErrorCode.EmptyContainerMapping
                    && ValidationHelper.IsStorageModelEmpty(defaultArtifactForError)
                        ? ErrorInfo.Severity.WARNING
                        : GetErrorInfoSeverity(error);

                artifactSet.AddError(new ErrorInfo(severity, error.Message, efObject, error.ErrorCode, errorClass));
            }

            defaultArtifactForError.SetValidityDirtyForErrorClass(errorClass, false);
        }

        private static ErrorInfo.Severity GetErrorInfoSeverity(EdmSchemaError error)
        {
            switch (error.Severity)
            {
                case EdmSchemaErrorSeverity.Error:
                    return ErrorInfo.Severity.ERROR;
                case EdmSchemaErrorSeverity.Warning:
                    return ErrorInfo.Severity.WARNING;
                default:
                    Debug.Fail("Unexpected value for EdmSchemaErrorSeverity");
                    return ErrorInfo.Severity.ERROR;
            }
        }

        private EFObject EdmSchemaError2EFObject(EdmSchemaError error, EFArtifact defaultArtifactForError)
        {
            EFArtifact a = null;
            if (error.SchemaLocation != null)
            {
                a = _modelManager.GetArtifact(Utils.FileName2Uri(error.SchemaLocation));
            }

            if (a == null)
            {
                a = defaultArtifactForError;
            }

            return a.FindEFObjectForLineAndColumn(error.Line, error.Column);
        }

        /// <summary>
        ///     This method returns true if the passed-in error code is one that requires the document to be opened in the XML editor,
        ///     with no designer.
        /// </summary>
        internal static bool IsOpenInEditorError(ErrorInfo errorInfo, EFArtifact artifact)
        {
            if ((errorInfo.ErrorClass & ErrorClass.Runtime_All) == 0)
            {
                return false;
            }

            var o = artifact.FindEFObjectForLineAndColumn(errorInfo.GetLineNumber(), errorInfo.GetColumnNumber());

            if (errorInfo.ErrorCode == (int)MappingErrorCode.XmlSchemaValidationError
                && o is ModificationFunctionMapping)
            {
                // we don't trigger safe-mode for XSD errors on ModificationFunctionMapping, since these can be fixed in the designer
                return false;
            }

            if (errorInfo.ErrorCode == (int)ErrorCode.XmlError
                && o is ReferentialConstraintRole)
            {
                // we don't trigger safe-mode for XSD errors on referential constraint roles.  This is so we can leave the RC 
                // and the Role, even if it has no properties.
                return false;
            }

            if (errorInfo.ErrorCode == (int)MappingErrorCode.ConditionError
                && o is Condition)
            {
                // don't trigger safe-mode for errors based around condition validation
                return false;
            }

            if (errorInfo.ErrorCode == (int)ErrorCode.InvalidPropertyInRelationshipConstraint
                && o is ReferentialConstraint)
            {
                // don't trigger safe-mode for error about Principal not being exactly identical to the EntityType key,
                // since we can run into this problem just by adding new key Property to the EntityType that is Principal Role for some Referential Constraint
                return false;
            }

            if (errorInfo.ErrorCode == ErrorCodes.ESCHER_VALIDATOR_UNDEFINED_COMPLEX_PROPERTY_TYPE
                || (errorInfo.ErrorCode == (int)ErrorCode.NotInNamespace && o is ComplexConceptualProperty))
            {
                // don't trigger safe-mode for underfined/deleted complex property types, the user should be able to fix the problem in the designer.
                return false;
            }

            if (errorInfo.ErrorCode == (int)ErrorCode.XmlError)
            {
                var navigationProperty = o as NavigationProperty;
                if (navigationProperty != null)
                {
                    // we allow the user to have navigation properties not bound to any association via an AssociationEnd if the Name property is defined as it's still required.
                    return
                        string.IsNullOrEmpty(navigationProperty.LocalName.Value) /* invalid name */||
                        navigationProperty.Relationship.Status != BindingStatus.Undefined ||
                        navigationProperty.ToRole.Status != BindingStatus.Undefined ||
                        navigationProperty.FromRole.Status != BindingStatus.Undefined;
                }
            }

            return
                UnrecoverableRuntimeErrors.SchemaObjectModelErrorCodes.Any(c => c == (ErrorCode)errorInfo.ErrorCode) ||
                UnrecoverableRuntimeErrors.StorageMappingErrorCodes.Any(c => c == (MappingErrorCode)errorInfo.ErrorCode);
        }
    }
}
