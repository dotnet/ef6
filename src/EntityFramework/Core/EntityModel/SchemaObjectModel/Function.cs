// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.EntityModel.SchemaObjectModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;

    /// <summary>
    /// class representing the Schema element in the schema
    /// </summary>
    internal class Function : SchemaType
    {
        #region Instance Fields

        // if adding properties also add to InitializeObject()!
        private bool _isAggregate;
        private bool _isBuiltIn;
        private bool _isNiladicFunction;
        protected bool _isComposable = true;
        protected FunctionCommandText _commandText;
        private string _storeFunctionName;
        protected SchemaType _type;
        private string _unresolvedType;
        protected bool _isRefType;
        // both are not specified
        protected SchemaElementLookUpTable<Parameter> _parameters;
        protected List<ReturnType> _returnTypeList;
        private CollectionKind _returnTypeCollectionKind = CollectionKind.None;
        private ParameterTypeSemantics _parameterTypeSemantics;
        private string _schema;

        private string _functionStrongName;

        #endregion

        #region Static Fields

        private static readonly Regex _typeParser = new Regex(
            @"^(?<modifier>((Collection)|(Ref)))\s*\(\s*(?<typeName>\S*)\s*\)$", RegexOptions.Compiled);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static void RemoveTypeModifier(ref string type, out TypeModifier typeModifier, out bool isRefType)
        {
            isRefType = false;
            typeModifier = TypeModifier.None;

            var match = _typeParser.Match(type);
            if (match.Success)
            {
                type = match.Groups["typeName"].Value;
                switch (match.Groups["modifier"].Value)
                {
                    case "Collection":
                        typeModifier = TypeModifier.Array;
                        return;
                    case "Ref":
                        isRefType = true;
                        return;
                    default:
                        Debug.Assert(false, "Unexpected modifier: " + match.Groups["modifier"].Value);
                        break;
                }
            }
        }

        internal static string GetTypeNameForErrorMessage(SchemaType type, CollectionKind colKind, bool isRef)
        {
            var typeName = type.FQName;
            if (isRef)
            {
                typeName = "Ref(" + typeName + ")";
            }
            switch (colKind)
            {
                case CollectionKind.Bag:
                    typeName = "Collection(" + typeName + ")";
                    break;
                default:
                    Debug.Assert(colKind == CollectionKind.None, "Unexpected CollectionKind");
                    break;
            }
            return typeName;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// ctor for a schema function
        /// </summary>
        public Function(Schema parentElement)
            : base(parentElement)
        {
        }

        #endregion

        #region Public Properties

        public bool IsAggregate
        {
            get { return _isAggregate; }
            internal set { _isAggregate = value; }
        }

        public bool IsBuiltIn
        {
            get { return _isBuiltIn; }
            internal set { _isBuiltIn = value; }
        }

        public bool IsNiladicFunction
        {
            get { return _isNiladicFunction; }
            internal set { _isNiladicFunction = value; }
        }

        public bool IsComposable
        {
            get { return _isComposable; }
            internal set { _isComposable = value; }
        }

        public string CommandText
        {
            get
            {
                if (_commandText != null)
                {
                    return _commandText.CommandText;
                }
                return null;
            }
        }

        public ParameterTypeSemantics ParameterTypeSemantics
        {
            get { return _parameterTypeSemantics; }
            internal set { _parameterTypeSemantics = value; }
        }

        public string StoreFunctionName
        {
            get { return _storeFunctionName; }
            internal set
            {
                Debug.Assert(value != null, "StoreFunctionName should never be set null value");
                _storeFunctionName = value;
            }
        }

        public virtual SchemaType Type
        {
            get
            {
                if (null != _returnTypeList)
                {
                    Debug.Assert(_returnTypeList.Count == 1, "Shouldn't use Type if there could be multiple return types");
                    return _returnTypeList[0].Type;
                }
                else
                {
                    return _type;
                }
            }
        }

        public IList<ReturnType> ReturnTypeList
        {
            get { return null != _returnTypeList ? new ReadOnlyCollection<ReturnType>(_returnTypeList) : null; }
        }

        public SchemaElementLookUpTable<Parameter> Parameters
        {
            get
            {
                if (_parameters == null)
                {
                    _parameters = new SchemaElementLookUpTable<Parameter>();
                }
                return _parameters;
            }
        }

        public CollectionKind CollectionKind
        {
            get { return _returnTypeCollectionKind; }
            internal set { _returnTypeCollectionKind = value; }
        }

        public override string Identity
        {
            get
            {
                if (String.IsNullOrEmpty(_functionStrongName))
                {
                    var name = FQName;
                    var stringBuilder = new StringBuilder(name);
                    var first = true;
                    stringBuilder.Append('(');
                    foreach (var parameter in Parameters)
                    {
                        if (!first)
                        {
                            stringBuilder.Append(',');
                        }
                        else
                        {
                            first = false;
                        }
                        stringBuilder.Append(Helper.ToString(parameter.ParameterDirection));
                        stringBuilder.Append(' ');
                        // we don't include the facets in the identity, since we are *not*
                        // taking them into consideration inside the 
                        // RankFunctionParameters method of TypeResolver.cs

                        parameter.WriteIdentity(stringBuilder);
                    }
                    stringBuilder.Append(')');
                    _functionStrongName = stringBuilder.ToString();
                }
                return _functionStrongName;
            }
        }

        public bool IsReturnAttributeReftype
        {
            get { return _isRefType; }
        }

        public virtual bool IsFunctionImport
        {
            get { return false; }
        }

        public string DbSchema
        {
            get { return _schema; }
        }

        #endregion

        #region Protected Properties

        protected override bool HandleElement(XmlReader reader)
        {
            if (base.HandleElement(reader))
            {
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.CommandText))
            {
                HandleCommandTextFunctionElment(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.Parameter))
            {
                HandleParameterElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.ReturnTypeElement))
            {
                HandleReturnTypeElement(reader);
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

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.ReturnType))
            {
                HandleReturnTypeAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.AggregateAttribute))
            {
                HandleAggregateAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.BuiltInAttribute))
            {
                HandleBuiltInAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.StoreFunctionName))
            {
                HandleStoreFunctionNameAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.NiladicFunction))
            {
                HandleNiladicFunctionAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.IsComposable))
            {
                HandleIsComposableAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.ParameterTypeSemantics))
            {
                HandleParameterTypeSemanticsAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.Schema))
            {
                HandleDbSchemaAttribute(reader);
                return true;
            }

            return false;
        }

        #endregion

        #region Internal Methods

        internal override void ResolveTopLevelNames()
        {
            base.ResolveTopLevelNames();

            if (_unresolvedType != null)
            {
                Debug.Assert(
                    Schema.DataModel != SchemaDataModelOption.ProviderManifestModel,
                    "ProviderManifest cannot have ReturnType as an attribute");
                Schema.ResolveTypeName(this, UnresolvedReturnType, out _type);
            }

            if (null != _returnTypeList)
            {
                foreach (var returnType in _returnTypeList)
                {
                    returnType.ResolveTopLevelNames();
                }
            }

            foreach (var parameter in Parameters)
            {
                parameter.ResolveTopLevelNames();
            }
        }

        /// <summary>
        /// Perform local validation on function definition.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal override void Validate()
        {
            base.Validate();

            if (_type != null
                && _returnTypeList != null)
            {
                AddError(
                    ErrorCode.ReturnTypeDeclaredAsAttributeAndElement, EdmSchemaErrorSeverity.Error,
                    Strings.TypeDeclaredAsAttributeAndElement);
            }

            // only call Type if _returnTypeList is empty, to ensure that we don't it when 
            // _returnTypeList has more than one element.
            if (_returnTypeList == null
                && Type == null)
            {
                // Composable functions and function imports must declare return type.
                if (IsComposable)
                {
                    AddError(
                        ErrorCode.ComposableFunctionOrFunctionImportWithoutReturnType, EdmSchemaErrorSeverity.Error,
                        Strings.ComposableFunctionOrFunctionImportMustDeclareReturnType);
                }
            }
            else
            {
                // Non-composable functions (except function imports) must not declare a return type.
                if (!IsComposable
                    && !IsFunctionImport)
                {
                    AddError(
                        ErrorCode.NonComposableFunctionWithReturnType, EdmSchemaErrorSeverity.Error,
                        Strings.NonComposableFunctionMustNotDeclareReturnType);
                }
            }

            if (Schema.DataModel
                != SchemaDataModelOption.EntityDataModel)
            {
                if (IsAggregate)
                {
                    // Make sure that the function has exactly one parameter and that takes
                    // a collection type
                    if (Parameters.Count != 1)
                    {
                        AddError(
                            ErrorCode.InvalidNumberOfParametersForAggregateFunction,
                            EdmSchemaErrorSeverity.Error,
                            this,
                            Strings.InvalidNumberOfParametersForAggregateFunction(FQName));
                    }
                    else if (Parameters.GetElementAt(0).CollectionKind
                             == CollectionKind.None)
                    {
                        // Since we have already checked that there should be exactly one parameter, it should be safe to get the
                        // first parameter for the function
                        var param = Parameters.GetElementAt(0);

                        AddError(
                            ErrorCode.InvalidParameterTypeForAggregateFunction,
                            EdmSchemaErrorSeverity.Error,
                            this,
                            Strings.InvalidParameterTypeForAggregateFunction(param.Name, FQName));
                    }
                }

                if (!IsComposable)
                {
                    // All aggregates, built-in and niladic functions must be composable, so throw error here.
                    if (IsAggregate ||
                        IsNiladicFunction ||
                        IsBuiltIn)
                    {
                        AddError(
                            ErrorCode.NonComposableFunctionAttributesNotValid, EdmSchemaErrorSeverity.Error,
                            Strings.NonComposableFunctionHasDisallowedAttribute);
                    }
                }

                if (null != CommandText)
                {
                    // Functions with command text are not composable.
                    if (IsComposable)
                    {
                        AddError(
                            ErrorCode.ComposableFunctionWithCommandText, EdmSchemaErrorSeverity.Error,
                            Strings.CommandTextFunctionsNotComposable);
                    }

                    // Functions with command text cannot declare store function name.
                    if (null != StoreFunctionName)
                    {
                        AddError(
                            ErrorCode.FunctionDeclaresCommandTextAndStoreFunctionName, EdmSchemaErrorSeverity.Error,
                            Strings.CommandTextFunctionsCannotDeclareStoreFunctionName);
                    }
                }
            }

            if (Schema.DataModel
                == SchemaDataModelOption.ProviderDataModel)
            {
                // In SSDL function may return a primitive value or a collection of rows with scalar props.
                // It is not possible to encode "collection of rows" in the ReturnType attribute, so the only check needed here is to make sure that the type is scalar and not a collection.
                if (_type != null
                    && (_type is ScalarType == false || _returnTypeCollectionKind != CollectionKind.None))
                {
                    AddError(
                        ErrorCode.FunctionWithNonPrimitiveTypeNotSupported,
                        EdmSchemaErrorSeverity.Error,
                        this,
                        Strings.FunctionWithNonPrimitiveTypeNotSupported(
                            GetTypeNameForErrorMessage(_type, _returnTypeCollectionKind, _isRefType), FQName));
                }
            }

            if (_returnTypeList != null)
            {
                foreach (var returnType in _returnTypeList)
                {
                    // FunctiomImportElement has additional validation for return types.
                    returnType.Validate();
                }
            }

            if (_parameters != null)
            {
                foreach (var parameter in _parameters)
                {
                    parameter.Validate();
                }
            }

            if (_commandText != null)
            {
                _commandText.Validate();
            }
        }

        internal override void ResolveSecondLevelNames()
        {
            foreach (var parameter in _parameters)
            {
                parameter.ResolveSecondLevelNames();
            }
        }

        internal override SchemaElement Clone(SchemaElement parentElement)
        {
            // We only support clone for FunctionImports.
            throw Error.NotImplemented();
        }

        protected void CloneSetFunctionFields(Function clone)
        {
            clone._isAggregate = _isAggregate;
            clone._isBuiltIn = _isBuiltIn;
            clone._isNiladicFunction = _isNiladicFunction;
            clone._isComposable = _isComposable;
            clone._commandText = _commandText;
            clone._storeFunctionName = _storeFunctionName;
            clone._type = _type;
            clone._returnTypeList = _returnTypeList;
            clone._returnTypeCollectionKind = _returnTypeCollectionKind;
            clone._parameterTypeSemantics = _parameterTypeSemantics;
            clone._schema = _schema;
            clone.Name = Name;

            // Clone all the parameters
            foreach (var parameter in Parameters)
            {
                var error = clone.Parameters.TryAdd((Parameter)parameter.Clone(clone));
                Debug.Assert(error == AddErrorKind.Succeeded, "Since we are cloning a validated function, this should never fail.");
            }
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        internal string UnresolvedReturnType
        {
            get { return _unresolvedType; }
            set { _unresolvedType = value; }
        }

        #endregion //Internal Properties

        #region Private Methods

        /// <summary>
        /// The method that is called when a DbSchema attribute is encountered.
        /// </summary>
        /// <param name="reader">An XmlReader positioned at the Type attribute.</param>
        private void HandleDbSchemaAttribute(XmlReader reader)
        {
            Debug.Assert(
                Schema.DataModel == SchemaDataModelOption.ProviderDataModel, "We shouldn't see this attribute unless we are parsing ssdl");
            Debug.Assert(reader != null);

            _schema = reader.Value;
        }

        /// <summary>
        /// Handler for the Version attribute
        /// </summary>
        /// <param name="reader">xml reader currently positioned at Version attribute</param>
        private void HandleAggregateAttribute(XmlReader reader)
        {
            Debug.Assert(reader != null);
            var isAggregate = false;
            HandleBoolAttribute(reader, ref isAggregate);
            IsAggregate = isAggregate;
        }

        /// <summary>
        /// Handler for the Namespace attribute
        /// </summary>
        /// <param name="reader">xml reader currently positioned at Namespace attribute</param>
        private void HandleBuiltInAttribute(XmlReader reader)
        {
            Debug.Assert(reader != null);
            var isBuiltIn = false;
            HandleBoolAttribute(reader, ref isBuiltIn);
            IsBuiltIn = isBuiltIn;
        }

        /// <summary>
        /// Handler for the Alias attribute
        /// </summary>
        /// <param name="reader">xml reader currently positioned at Alias attribute</param>
        private void HandleStoreFunctionNameAttribute(XmlReader reader)
        {
            Debug.Assert(reader != null);
            var value = reader.Value;
            if (!String.IsNullOrEmpty(value))
            {
                value = value.Trim();
                StoreFunctionName = value;
            }
        }

        /// <summary>
        /// Handler for the NiladicFunctionAttribute attribute
        /// </summary>
        /// <param name="reader">xml reader currently positioned at Namespace attribute</param>
        private void HandleNiladicFunctionAttribute(XmlReader reader)
        {
            Debug.Assert(reader != null);
            var isNiladicFunction = false;
            HandleBoolAttribute(reader, ref isNiladicFunction);
            IsNiladicFunction = isNiladicFunction;
        }

        /// <summary>
        /// Handler for the IsComposableAttribute attribute
        /// </summary>
        /// <param name="reader">xml reader currently positioned at Namespace attribute</param>
        private void HandleIsComposableAttribute(XmlReader reader)
        {
            Debug.Assert(reader != null);
            var isComposable = true;
            HandleBoolAttribute(reader, ref isComposable);
            IsComposable = isComposable;
        }

        private void HandleCommandTextFunctionElment(XmlReader reader)
        {
            Debug.Assert(reader != null);

            var commandText = new FunctionCommandText(this);
            commandText.Parse(reader);
            _commandText = commandText;
        }

        protected virtual void HandleReturnTypeAttribute(XmlReader reader)
        {
            Debug.Assert(reader != null);
            Debug.Assert(UnresolvedReturnType == null);

            string type;
            if (!Utils.GetString(Schema, reader, out type))
            {
                return;
            }

            TypeModifier typeModifier;

            RemoveTypeModifier(ref type, out typeModifier, out _isRefType);

            switch (typeModifier)
            {
                case TypeModifier.Array:
                    CollectionKind = CollectionKind.Bag;
                    break;
                case TypeModifier.None:
                    break;
                default:
                    Debug.Assert(false, "RemoveTypeModifier already checks for this");
                    break;
            }

            if (!Utils.ValidateDottedName(Schema, reader, type))
            {
                return;
            }

            UnresolvedReturnType = type;
        }

        /// <summary>
        /// Handler for the Parameter Element
        /// </summary>
        /// <param name="reader">xml reader currently positioned at Parameter Element</param>
        protected void HandleParameterElement(XmlReader reader)
        {
            Debug.Assert(reader != null);

            var parameter = new Parameter(this);

            parameter.Parse(reader);

            Parameters.Add(parameter, true, Strings.ParameterNameAlreadyDefinedDuplicate);
        }

        /// <summary>
        /// Handler for the ReturnType element
        /// </summary>
        /// <param name="reader">xml reader currently positioned at ReturnType element</param>
        protected void HandleReturnTypeElement(XmlReader reader)
        {
            Debug.Assert(reader != null);

            var returnType = new ReturnType(this);

            returnType.Parse(reader);

            if (_returnTypeList == null)
            {
                _returnTypeList = new List<ReturnType>();
            }
            _returnTypeList.Add(returnType);
        }

        /// <summary>
        /// Handles ParameterTypeSemantics attribute
        /// </summary>
        /// <param name="reader"></param>
        private void HandleParameterTypeSemanticsAttribute(XmlReader reader)
        {
            Debug.Assert(reader != null);

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
                    case "ExactMatchOnly":
                        ParameterTypeSemantics = ParameterTypeSemantics.ExactMatchOnly;
                        break;
                    case "AllowImplicitPromotion":
                        ParameterTypeSemantics = ParameterTypeSemantics.AllowImplicitPromotion;
                        break;
                    case "AllowImplicitConversion":
                        ParameterTypeSemantics = ParameterTypeSemantics.AllowImplicitConversion;
                        break;
                    default:
                        // don't try to use the name of the function, because we are still parsing the 
                        // attributes, and we may not be to the name attribute yet.
                        AddError(
                            ErrorCode.InvalidValueForParameterTypeSemantics, EdmSchemaErrorSeverity.Error, reader,
                            Strings.InvalidValueForParameterTypeSemanticsAttribute(
                                value));

                        break;
                }
            }
        }

        #endregion
    }
}
