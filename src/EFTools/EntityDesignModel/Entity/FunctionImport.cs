// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Tools.XmlDesignerBase;

    internal class FunctionImport : NameableAnnotatableElement
    {
        internal static readonly string ElementName = "FunctionImport";
        internal static readonly string AttributeReturnType = "ReturnType";
        internal static readonly string AttributeEntitySet = "EntitySet";
        internal static readonly string AttributeMethodAccess = "MethodAccess";
        internal static readonly string AttributeIsComposable = "IsComposable";

        // this string is used to form ReturnType attribute value. 
        // The string is an invariable keyword in the CSDL so it should not be localized.
        internal static readonly string CollectionFormat = "Collection({0})";

        // ReturnType attribute can be either primitive type, complex Type or EntityType
        private DefaultableValue<string> _returnTypeAsPrimitiveType;
        private SingleItemBinding<EntityType> _returnTypeAsEntityType;
        private SingleItemBinding<ComplexType> _returnTypeAsComplexType;

        private SingleItemBinding<EntitySet> _entitySet;
        private DefaultableValue<string> _methodAccessAttr;
        private DefaultableValueBoolOrNone _isComposableAttr;

        private readonly List<Parameter> _parameters = new List<Parameter>();

        internal FunctionImport(EFElement parent, XElement element)
            : base(parent, element)
        {
        }

        internal Function Function
        {
            get
            {
                foreach (var fim in GetAntiDependenciesOfType<FunctionImportMapping>())
                {
                    if (fim.FunctionImportName.Target == this
                        && fim.FunctionName != null
                        && fim.FunctionName.Target != null)
                    {
                        return fim.FunctionName.Target;
                    }
                }

                return null;
            }
        }

        internal FunctionImportMapping FunctionImportMapping
        {
            get
            {
                foreach (var fim in GetAntiDependenciesOfType<FunctionImportMapping>())
                {
                    if (fim.FunctionImportName.Target == this)
                    {
                        return fim;
                    }
                }

                return null;
            }
        }

        // if the EntitySet attribute is set then the ReturnType is EntityType.
        internal bool IsReturnTypeEntityType
        {
            get
            {
                if ((EntitySet.RefName != null)
                    || (EntitySet.Target != null))
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        ///     Check whether FunctionImport return type is a complex type
        /// </summary>
        internal bool IsReturnTypeComplexType
        {
            get
            {
                // Return false if it is an entity type.
                if (IsReturnTypeEntityType)
                {
                    return false;
                }
                    // Examine the attribute, check whether the type name is a primitive type.
                else if (XElement != null)
                {
                    // ReturnType attribute will look like: ReturnType="Collection(Fully_Qualified_name)"
                    var returnTypeAttribute = GetAttribute(AttributeReturnType);
                    if (returnTypeAttribute != null)
                    {
                        var unwrapString = ModelHelper.UnwrapCollectionAroundFunctionImportReturnType(returnTypeAttribute.Value, true);
                        return (!ModelHelper.AllPrimitiveTypes(Artifact.SchemaVersion).Contains(unwrapString));
                    }
                }
                else if (_returnTypeAsComplexType != null)
                {
                    return true;
                }

                return false;
            }
        }

        internal EFAttribute ReturnType
        {
            get
            {
                if (IsReturnTypeEntityType)
                {
                    return ReturnTypeAsEntityType;
                }
                else if (IsReturnTypeComplexType)
                {
                    return ReturnTypeAsComplexType;
                }
                else
                {
                    return ReturnTypeAsPrimitiveType;
                }
            }
        }

        /// <summary>
        ///     Return display-string representation of the return type.
        /// </summary>
        internal string ReturnTypeToPrettyString
        {
            get
            {
                if (IsReturnTypeEntityType)
                {
                    // If target is known and available, return target's local name.
                    if (ReturnTypeAsEntityType.Status == BindingStatus.Known)
                    {
                        return ReturnTypeAsEntityType.Target.LocalName.Value;
                    }
                    else
                    {
                        // The attribute value format is Collection(Namespace.Name).
                        // The UnwrapCollectionAroundFunctionImportReturnType method will return the "Name" part.
                        return ModelHelper.UnwrapCollectionAroundFunctionImportReturnType(ReturnTypeAsEntityType.RefName);
                    }
                }
                else if (IsReturnTypeComplexType)
                {
                    // If target is known and available, return target's local name.
                    if (ReturnTypeAsComplexType.Status == BindingStatus.Known)
                    {
                        return ReturnTypeAsComplexType.Target.LocalName.Value;
                    }
                    else
                    {
                        // The attribute value format is Collection(Namespace.Name).
                        // The UnwrapCollectionAroundFunctionImportReturnType method will return the "Name" part.
                        return ModelHelper.UnwrapCollectionAroundFunctionImportReturnType(ReturnTypeAsComplexType.RefName);
                    }
                }
                else
                {
                    return ModelHelper.UnwrapCollectionAroundFunctionImportReturnType(ReturnTypeAsPrimitiveType.Value);
                }
            }
        }

        /// <summary>
        ///     Return normalized string representation of the return type.
        /// </summary>
        internal string ReturnTypeToNormalizedString
        {
            get
            {
                if (IsReturnTypeEntityType)
                {
                    // If target is known and available, return target's NormalizedNameExternal name.
                    if (ReturnTypeAsEntityType.Status == BindingStatus.Known)
                    {
                        return ReturnTypeAsEntityType.Target.NormalizedNameExternal;
                    }
                    else
                    {
                        // The attribute value format is Collection(Namespace.Name).
                        // The UnwrapCollectionAroundFunctionImportReturnType method will return the "Namespace.Name" part.
                        return ModelHelper.UnwrapCollectionAroundFunctionImportReturnType(ReturnTypeAsEntityType.RefName, true);
                    }
                }
                else if (IsReturnTypeComplexType)
                {
                    // If target is known and available, return target's NormalizedNameExternal name.
                    if (ReturnTypeAsComplexType.Status == BindingStatus.Known)
                    {
                        return ReturnTypeAsComplexType.Target.NormalizedNameExternal;
                    }
                    else
                    {
                        // The attribute value format is Collection(Namespace.Name).
                        // The UnwrapCollectionAroundFunctionImportReturnType method will return the "Namespace.Name" part.
                        return ModelHelper.UnwrapCollectionAroundFunctionImportReturnType(ReturnTypeAsComplexType.RefName, true);
                    }
                }
                else
                {
                    return ModelHelper.UnwrapCollectionAroundFunctionImportReturnType(ReturnTypeAsPrimitiveType.Value);
                }
            }
        }

        internal SingleItemBinding<EntityType> ReturnTypeAsEntityType
        {
            get
            {
                // if ReturnType was previously a primitive type, we need to clear this EFAttribute first
                if (_returnTypeAsPrimitiveType != null)
                {
                    ClearEFObject(_returnTypeAsPrimitiveType);
                    _returnTypeAsPrimitiveType = null;
                }

                if (_returnTypeAsComplexType != null)
                {
                    ClearEFObject(_returnTypeAsComplexType);
                    _returnTypeAsComplexType = null;
                }

                if (_returnTypeAsEntityType == null)
                {
                    _returnTypeAsEntityType = new SingleItemBinding<EntityType>(
                        this,
                        AttributeReturnType,
                        FunctionImportEntityTypeNameNormalizer);

                    //
                    // HACK HACK:  this is a total hack.  In some undo/redo scenarios, this could be unresolved unless we resolve it here.
                    // the problem is that a ReturnType could be a DefaultableValue or an ItemBinding, depending on 
                    // its contents.  We should address this at a more fundamental level.  One option is introducing a 
                    // DefaultableValue/ItemBinding "union" type which can serve this dual-role purpose
                    //
                    //  TFS BUG #524594 is tracking fixing this up.
                    //

                    if (State == EFElementState.Resolved)
                    {
                        _returnTypeAsEntityType.Rebind();
                    }
                }

                return _returnTypeAsEntityType;
            }
        }

        internal SingleItemBinding<ComplexType> ReturnTypeAsComplexType
        {
            get
            {
                // if ReturnType was previously a primitive type or a entity type, we need to clear this EFAttribute first
                if (_returnTypeAsPrimitiveType != null)
                {
                    ClearEFObject(_returnTypeAsPrimitiveType);
                    _returnTypeAsPrimitiveType = null;
                }

                if (_returnTypeAsEntityType != null)
                {
                    ClearEFObject(_returnTypeAsEntityType);
                    _returnTypeAsEntityType = null;
                }

                if (_returnTypeAsComplexType == null)
                {
                    _returnTypeAsComplexType = new SingleItemBinding<ComplexType>(
                        this,
                        AttributeReturnType,
                        FunctionImportEntityTypeNameNormalizer);

                    //
                    //Just like the code in ReturnTypeAsEntityType property, this is necessary because the binding could be unresolved in the undo scenario.
                    // TFS BUG #524594 is tracking fixing this up.
                    //
                    if (State == EFElementState.Resolved)
                    {
                        _returnTypeAsComplexType.Rebind();
                    }
                }

                return _returnTypeAsComplexType;
            }
        }

        internal DefaultableValue<string> ReturnTypeAsPrimitiveType
        {
            get
            {
                // if ReturnType was previously an EntityType or a complex type, we need to clear this EFAttribute first
                if (_returnTypeAsEntityType != null)
                {
                    ClearEFObject(_returnTypeAsEntityType);
                    _returnTypeAsEntityType = null;
                }

                if (_returnTypeAsComplexType != null)
                {
                    ClearEFObject(_returnTypeAsComplexType);
                    _returnTypeAsComplexType = null;
                }

                if (_returnTypeAsPrimitiveType == null)
                {
                    _returnTypeAsPrimitiveType = new ReturnTypeAsPrimitiveTypeDefaultableValue(this);
                }
                return _returnTypeAsPrimitiveType;
            }
        }

        internal class ReturnTypeAsPrimitiveTypeDefaultableValue : DefaultableValue<string>
        {
            internal ReturnTypeAsPrimitiveTypeDefaultableValue(EFElement parent)
                : base(parent, AttributeReturnType)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeReturnType; }
            }

            public override string DefaultValue
            {
                get { return Resources.NoneDisplayValueUsedForUX; }
            }
        }

        internal SingleItemBinding<EntitySet> EntitySet
        {
            get
            {
                if (_entitySet == null)
                {
                    _entitySet = new SingleItemBinding<EntitySet>(
                        this,
                        AttributeEntitySet,
                        FunctionImportEntitySetNameNormalizer);
                }
                return _entitySet;
            }
        }

        internal DefaultableValue<string> MethodAccess
        {
            get
            {
                if (_methodAccessAttr == null)
                {
                    _methodAccessAttr = new MethodAccessDefaultableValue(this);
                }
                return _methodAccessAttr;
            }
        }

        private class MethodAccessDefaultableValue : DefaultableValue<string>
        {
            internal MethodAccessDefaultableValue(EFElement parent)
                : base(parent, AttributeMethodAccess, SchemaManager.GetCodeGenerationNamespaceName())
            {
            }

            internal override string AttributeName
            {
                get { return AttributeMethodAccess; }
            }

            public override string DefaultValue
            {
                get { return ModelConstants.CodeGenerationAccessPublic; }
            }
        }

        /// <summary>
        ///     Manages the content of the Composable attribute
        /// </summary>
        internal DefaultableValueBoolOrNone IsComposable
        {
            get
            {
                if (_isComposableAttr == null)
                {
                    _isComposableAttr = new ComposableDefaultableValue(this);
                }
                return _isComposableAttr;
            }
        }

        private class ComposableDefaultableValue : DefaultableValueBoolOrNone
        {
            internal ComposableDefaultableValue(FunctionImport parent)
                : base(parent, AttributeIsComposable)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeIsComposable; }
            }

            public override BoolOrNone DefaultValue
            {
                get
                {
                    // Note: runtime treats NoneValue as equivalent to false
                    return BoolOrNone.NoneValue;
                }
            }
        }

        internal void AddParameter(Parameter param)
        {
            _parameters.Add(param);
        }

        internal IList<Parameter> Parameters()
        {
            return _parameters.AsReadOnly();
        }

        // we unfortunately get a warning from the compiler when we use the "base" keyword in "iterator" types generated by using the
        // "yield return" keyword.  By adding this method, I was able to get around this.  Unfortunately, I wasn't able to figure out
        // a way to implement this once and have derived classes share the implementation (since the "base" keyword is resolved at 
        // compile-time and not at runtime.
        private IEnumerable<EFObject> BaseChildren
        {
            get { return base.Children; }
        }

        internal override IEnumerable<EFObject> Children
        {
            get
            {
                foreach (var efobj in BaseChildren)
                {
                    yield return efobj;
                }
                foreach (var param in _parameters)
                {
                    yield return param;
                }

                yield return ReturnType;
                yield return EntitySet;
                yield return MethodAccess;
                yield return IsComposable;
            }
        }

        protected override void OnChildDeleted(EFContainer efContainer)
        {
            var child1 = efContainer as Parameter;
            if (child1 != null)
            {
                _parameters.Remove(child1);
                return;
            }

            base.OnChildDeleted(efContainer);
        }

        internal override DeleteEFElementCommand GetDeleteCommand()
        {
            var cmd = new DeleteFunctionImportCommand(this);
            if (cmd == null)
            {
                // shouldn't happen, just to be safe
                throw new InvalidOperationException();
            }
            return cmd;
        }

#if DEBUG
        internal override ICollection<string> MyAttributeNames()
        {
            var s = base.MyAttributeNames();
            s.Add(AttributeReturnType);
            s.Add(AttributeEntitySet);
            s.Add(AttributeMethodAccess);
            s.Add(AttributeIsComposable);
            return s;
        }
#endif

#if DEBUG
        internal override ICollection<string> MyChildElementNames()
        {
            var s = base.MyChildElementNames();
            s.Add(Parameter.ElementName);
            return s;
        }
#endif

        protected override void PreParse()
        {
            Debug.Assert(State != EFElementState.Parsed, "this object should not already be in the parsed state");

            ClearEFObject(_returnTypeAsEntityType);
            _returnTypeAsEntityType = null;
            ClearEFObject(_returnTypeAsComplexType);
            _returnTypeAsComplexType = null;
            ClearEFObject(_returnTypeAsPrimitiveType);
            _returnTypeAsPrimitiveType = null;
            ClearEFObject(_entitySet);
            _entitySet = null;
            ClearEFObject(_methodAccessAttr);
            _methodAccessAttr = null;

            ClearEFObjectCollection(_parameters);

            base.PreParse();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal override bool ParseSingleElement(ICollection<XName> unprocessedElements, XElement elem)
        {
            if (elem.Name.LocalName == Parameter.ElementName)
            {
                var param = new Parameter(this, elem);
                param.Parse(unprocessedElements);
                _parameters.Add(param);
            }
            else
            {
                return base.ParseSingleElement(unprocessedElements, elem);
            }
            return true;
        }

        internal override string GetRefNameForBinding(ItemBinding binding)
        {
            return LocalName.Value;
        }

        protected override void DoNormalize()
        {
            var normalizedName = FunctionImportNameNormalizer.NameNormalizer(this, LocalName.Value);
            Debug.Assert(null != normalizedName, "Null NormalizedName for refName " + LocalName.Value);
            NormalizedName = (normalizedName != null ? normalizedName.Symbol : Symbol.EmptySymbol);
            base.DoNormalize();
        }

        protected override void DoResolve(EFArtifactSet artifactSet)
        {
            if (EntitySet.RefName != null)
            {
                EntitySet.Rebind();
                ReturnTypeAsEntityType.Rebind();
                if (EntitySet.Status == BindingStatus.Known
                    && ReturnTypeAsEntityType.Status == BindingStatus.Known)
                {
                    State = EFElementState.Resolved;
                }
            }
            else if (IsReturnTypeComplexType)
            {
                ReturnTypeAsComplexType.Rebind();
                if (ReturnTypeAsComplexType.Status == BindingStatus.Known)
                {
                    State = EFElementState.Resolved;
                }
            }
            else
            {
                base.DoResolve(artifactSet);
            }
        }

        internal static NormalizedName FunctionImportEntitySetNameNormalizer(EFElement parent, string refName)
        {
            Debug.Assert(parent != null, "parent should not be null");

            if (refName == null)
            {
                return null;
            }

            // cast the parameter to what this really is
            var fi = parent as FunctionImport;
            Debug.Assert(fi != null, "parent should be a " + typeof(FunctionImport));

            // get the entity container name
            Symbol entityContainerName = null;
            var ec = fi.Parent as BaseEntityContainer;
            if (ec != null)
            {
                entityContainerName = ec.NormalizedName;
            }

            Symbol symbol = null;
            if (entityContainerName != null)
            {
                // the normalized name for an EntitySet is 'EntityContainerName + # + EntitySetName'
                symbol = new Symbol(entityContainerName, refName);
            }

            if (symbol == null)
            {
                symbol = new Symbol(refName);
            }

            var normalizedName = new NormalizedName(symbol, null, null, refName);
            return normalizedName;
        }

        internal static NormalizedName FunctionImportEntityTypeNameNormalizer(EFElement parent, string refName)
        {
            Debug.Assert(parent != null, "parent should not be null");

            if (refName == null)
            {
                return null;
            }

            // since the format of ReturnType is "Collection(...)", we need to unwrap the refName first
            // Make sure that  we extract a fully qualified name for refName.
            refName = ModelHelper.UnwrapCollectionAroundFunctionImportReturnType(refName, true);
            return EntityTypeNameNormalizer.NameNormalizer(parent, refName);
        }

        /// <summary>
        ///     This method will go through function-import result-column type mappings.
        ///     For each type-mapping checks whether there exists a type-mapping that involves the passed-in property.
        ///     If a match is found, the corresponding column name is returned.
        ///     If no match could be found, an empty string is returned.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        internal static string GetFunctionImportResultColumnName(FunctionImport functionImport, Property property)
        {
            if (functionImport == null)
            {
                throw new ArgumentException("functionImport is null.");
            }

            if (property == null)
            {
                throw new ArgumentException("property is null");
            }

            var mapping = functionImport.FunctionImportMapping;
            if (mapping != null
                && mapping.ResultMapping != null)
            {
                foreach (var typeMapping in mapping.ResultMapping.TypeMappings())
                {
                    var scalarProperty = typeMapping.FindScalarProperty(property);
                    if (scalarProperty != null)
                    {
                        Debug.Assert(!String.IsNullOrEmpty(scalarProperty.ColumnName.Value), "Function Import Result Column name is empty");
                        if (!String.IsNullOrEmpty(scalarProperty.ColumnName.Value))
                        {
                            return scalarProperty.ColumnName.Value;
                        }
                    }
                }
            }
            return String.Empty;
        }
    }
}
