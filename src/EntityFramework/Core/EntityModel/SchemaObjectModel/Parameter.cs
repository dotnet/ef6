// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityModel.SchemaObjectModel
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using System.Xml;
    using Som = System.Data.Entity.Core.EntityModel.SchemaObjectModel;

    /// <summary>
    ///     Summary description for StructuredProperty.
    /// </summary>
    internal class Parameter : FacetEnabledSchemaElement
    {
        #region Instance Fields

        private ParameterDirection _parameterDirection = ParameterDirection.Input;
        private CollectionKind _collectionKind = CollectionKind.None;
        private ModelFunctionTypeElement _typeSubElement;
        private bool _isRefType;

        #endregion

        #region constructor

        /// <summary>
        /// </summary>
        /// <param name="parentElement"> </param>
        internal Parameter(Function parentElement)
            : base(parentElement)
        {
            _typeUsageBuilder = new TypeUsageBuilder(this);
        }

        #endregion

        #region Public Properties

        internal ParameterDirection ParameterDirection
        {
            get { return _parameterDirection; }
        }

        internal CollectionKind CollectionKind
        {
            get { return _collectionKind; }
            set { _collectionKind = value; }
        }

        internal bool IsRefType
        {
            get { return _isRefType; }
        }

        internal override TypeUsage TypeUsage
        {
            get
            {
                if (_typeSubElement != null)
                {
                    return _typeSubElement.GetTypeUsage();
                }
                else if (base.TypeUsage == null)
                {
                    return null;
                }
                else if (CollectionKind != CollectionKind.None)
                {
                    return TypeUsage.Create(new CollectionType(base.TypeUsage));
                }
                else
                {
                    return base.TypeUsage;
                }
            }
        }

        #endregion

        internal new SchemaType Type
        {
            get { return _type; }
        }

        internal void WriteIdentity(StringBuilder builder)
        {
            builder.Append("Parameter(");
            if (!string.IsNullOrWhiteSpace(UnresolvedType))
            {
                if (_collectionKind != CollectionKind.None)
                {
                    builder.Append("Collection(" + UnresolvedType + ")");
                }
                else if (_isRefType)
                {
                    builder.Append("Ref(" + UnresolvedType + ")");
                }
                else
                {
                    builder.Append(UnresolvedType);
                }
            }
            else if (_typeSubElement != null)
            {
                _typeSubElement.WriteIdentity(builder);
            }
            builder.Append(")");
        }

        internal override SchemaElement Clone(SchemaElement parentElement)
        {
            var parameter = new Parameter((Function)parentElement);
            parameter._collectionKind = _collectionKind;
            parameter._parameterDirection = _parameterDirection;
            parameter._type = _type;
            parameter.Name = Name;
            parameter._typeUsageBuilder = _typeUsageBuilder;
            return parameter;
        }

        internal bool ResolveNestedTypeNames(
            Converter.ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
        {
            if (_typeSubElement == null)
            {
                return false;
            }
            return _typeSubElement.ResolveNameAndSetTypeUsage(convertedItemCache, newGlobalItems);
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.TypeElement))
            {
                HandleTypeAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.Mode))
            {
                HandleModeAttribute(reader);
                return true;
            }
            else if (_typeUsageBuilder.HandleAttribute(reader))
            {
                return true;
            }

            return false;
        }

        #region Private Methods

        private void HandleTypeAttribute(XmlReader reader)
        {
            DebugCheck.NotNull(reader);
            Debug.Assert(UnresolvedType == null);

            string type;
            if (!Utils.GetString(Schema, reader, out type))
            {
                return;
            }

            TypeModifier typeModifier;

            Function.RemoveTypeModifier(ref type, out typeModifier, out _isRefType);

            switch (typeModifier)
            {
                case TypeModifier.Array:
                    CollectionKind = CollectionKind.Bag;
                    break;
                default:
                    Debug.Assert(
                        typeModifier == TypeModifier.None,
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "Type is not valid for property {0}: {1}. The modifier for the type cannot be used in this context.", FQName,
                            reader.Value));
                    break;
            }

            if (!Utils.ValidateDottedName(Schema, reader, type))
            {
                return;
            }

            UnresolvedType = type;
        }

        private void HandleModeAttribute(XmlReader reader)
        {
            DebugCheck.NotNull(reader);

            var value = reader.Value;

            if (String.IsNullOrEmpty(value))
            {
                return;
            }

            value = value.Trim();

            if (!String.IsNullOrEmpty(value))
            {
                switch (value)
                {
                    case XmlConstants.In:
                        _parameterDirection = ParameterDirection.Input;
                        break;
                    case XmlConstants.Out:
                        _parameterDirection = ParameterDirection.Output;
                        if (ParentElement.IsComposable
                            && ParentElement.IsFunctionImport)
                        {
                            AddErrorBadParameterDirection(value, reader, Strings.BadParameterDirectionForComposableFunctions);
                        }
                        break;
                    case XmlConstants.InOut:
                        _parameterDirection = ParameterDirection.InputOutput;
                        if (ParentElement.IsComposable
                            && ParentElement.IsFunctionImport)
                        {
                            AddErrorBadParameterDirection(value, reader, Strings.BadParameterDirectionForComposableFunctions);
                        }
                        break;
                    default:
                        {
                            AddErrorBadParameterDirection(value, reader, Strings.BadParameterDirection);
                        }
                        break;
                }
            }
        }

        private void AddErrorBadParameterDirection(string value, XmlReader reader, Func<object, object, object, object, string> errorFunc)
        {
            // don't try to identify the parameter by any of the attributes
            // because we are still parsing attributes, and we don't know which ones
            // have been parsed yet.
            AddError(
                ErrorCode.BadParameterDirection, EdmSchemaErrorSeverity.Error, reader,
                errorFunc(
                    ParentElement.Parameters.Count, // indexed at 0 to be similar to the old exception
                    ParentElement.Name,
                    ParentElement.ParentElement.FQName,
                    value));
        }

        protected override bool HandleElement(XmlReader reader)
        {
            if (base.HandleElement(reader))
            {
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.CollectionType))
            {
                HandleCollectionTypeElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.ReferenceType))
            {
                HandleReferenceTypeElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.TypeRef))
            {
                HandleTypeRefElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.RowType))
            {
                HandleRowTypeElement(reader);
                return true;
            }
            else if (Schema.DataModel
                     == SchemaDataModelOption.EntityDataModel)
            {
                if (CanHandleElement(reader, XmlConstants.ValueAnnotation))
                {
                    // EF does not support this EDM 3.0 element, so ignore it.
                    SkipElement(reader);
                    return true;
                }
                else if (CanHandleElement(reader, XmlConstants.TypeAnnotation))
                {
                    // EF does not support this EDM 3.0 element, so ignore it.
                    SkipElement(reader);
                    return true;
                }
            }

            return false;
        }

        protected void HandleCollectionTypeElement(XmlReader reader)
        {
            DebugCheck.NotNull(reader);

            var subElement = new CollectionTypeElement(this);
            subElement.Parse(reader);
            _typeSubElement = subElement;
        }

        protected void HandleReferenceTypeElement(XmlReader reader)
        {
            DebugCheck.NotNull(reader);

            var subElement = new ReferenceTypeElement(this);
            subElement.Parse(reader);
            _typeSubElement = subElement;
        }

        protected void HandleTypeRefElement(XmlReader reader)
        {
            DebugCheck.NotNull(reader);

            var subElement = new TypeRefElement(this);
            subElement.Parse(reader);
            _typeSubElement = subElement;
        }

        protected void HandleRowTypeElement(XmlReader reader)
        {
            DebugCheck.NotNull(reader);

            var subElement = new RowTypeElement(this);
            subElement.Parse(reader);
            _typeSubElement = subElement;
        }

        #endregion

        internal override void ResolveTopLevelNames()
        {
            // If type was defined as an attribute: <ReturnType Type="int"/>
            if (_unresolvedType != null)
            {
                base.ResolveTopLevelNames();
            }

            // If type was defined as a subelement: <ReturnType><CollectionType>...</CollectionType></ReturnType>
            if (_typeSubElement != null)
            {
                _typeSubElement.ResolveTopLevelNames();
            }
        }

        internal override void Validate()
        {
            base.Validate();

            ValidationHelper.ValidateTypeDeclaration(this, _type, _typeSubElement);

            if (Schema.DataModel
                != SchemaDataModelOption.EntityDataModel)
            {
                Debug.Assert(
                    Schema.DataModel == SchemaDataModelOption.ProviderDataModel ||
                    Schema.DataModel == SchemaDataModelOption.ProviderManifestModel, "Unexpected data model");

                var collectionAllowed = ParentElement.IsAggregate;

                // Only scalar parameters are allowed for functions in s-space.
                Debug.Assert(_typeSubElement == null, "Unexpected type subelement inside <Parameter> element.");
                if (_type != null
                    && (_type is ScalarType == false || (!collectionAllowed && _collectionKind != CollectionKind.None)))
                {
                    var typeName = "";
                    if (_type != null)
                    {
                        typeName = Function.GetTypeNameForErrorMessage(_type, _collectionKind, _isRefType);
                    }
                    else if (_typeSubElement != null)
                    {
                        typeName = _typeSubElement.FQName;
                    }
                    if (Schema.DataModel
                        == SchemaDataModelOption.ProviderManifestModel)
                    {
                        AddError(
                            ErrorCode.FunctionWithNonEdmTypeNotSupported,
                            EdmSchemaErrorSeverity.Error,
                            this,
                            Strings.FunctionWithNonEdmPrimitiveTypeNotSupported(typeName, ParentElement.FQName));
                    }
                    else
                    {
                        AddError(
                            ErrorCode.FunctionWithNonPrimitiveTypeNotSupported,
                            EdmSchemaErrorSeverity.Error,
                            this,
                            Strings.FunctionWithNonPrimitiveTypeNotSupported(typeName, ParentElement.FQName));
                    }
                    return;
                }
            }

            ValidationHelper.ValidateFacets(this, _type, _typeUsageBuilder);

            if (_isRefType)
            {
                ValidationHelper.ValidateRefType(this, _type);
            }

            if (_typeSubElement != null)
            {
                _typeSubElement.Validate();
            }
        }
    }
}
