// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal class Function : EFNameableItem
    {
        internal static readonly string ElementName = "Function";
        internal static readonly string AttributeReturnType = "ReturnType";
        internal static readonly string AttributeAggregate = "Aggregate";
        internal static readonly string AttributeBuiltIn = "BuiltIn";
        internal static readonly string AttributeStoreFunctionName = "StoreFunctionName";
        internal static readonly string AttributeNiladicFunction = "NiladicFunction";
        internal static readonly string AttributeIsComposable = "IsComposable";
        internal static readonly string AttributeParameterTypeSemantics = "ParameterTypeSemantics";
        internal static readonly string AttributeSchema = "Schema";

        private DefaultableValue<string> _returnTypeAttr;
        private DefaultableValue<bool> _aggregateAttr;
        private DefaultableValue<bool> _builtInAttr;
        private DefaultableValue<string> _storeFunctionNameAttr;
        private DefaultableValue<bool> _niladicFunctionAttr;
        private DefaultableValue<bool> _isComposableAttr;
        private DefaultableValue<string> _parameterTypeSemanticsAttr;
        private DefaultableValue<string> _schemaAttr;
        private DefaultableValue<string> _storeSchemaGenSchemaAttr;
        private DefaultableValue<string> _storeSchemaGenNameAttr;

        private CommandText _commandText;
        private readonly List<Parameter> _parameters = new List<Parameter>();

        internal Function(EFElement parent, XElement element)
            : base(parent, element)
        {
        }

        internal StorageEntityModel EntityModel
        {
            get
            {
                var baseType = Parent as StorageEntityModel;
                Debug.Assert(baseType != null, "this.Parent should be a StorageEntityModel");
                return baseType;
            }
        }

        internal CommandText CommandText
        {
            get { return _commandText; }
        }

        internal DefaultableValue<string> ReturnType
        {
            get
            {
                if (_returnTypeAttr == null)
                {
                    _returnTypeAttr = new ReturnTypeDefaultableValue(this);
                }
                return _returnTypeAttr;
            }
        }

        private class ReturnTypeDefaultableValue : DefaultableValue<string>
        {
            internal ReturnTypeDefaultableValue(EFElement parent)
                : base(parent, AttributeReturnType)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeReturnType; }
            }

            public override string DefaultValue
            {
                get { return null; }
            }
        }

        internal DefaultableValue<bool> Aggregate
        {
            get
            {
                if (_aggregateAttr == null)
                {
                    _aggregateAttr = new AggregateDefaultableValue(this);
                }
                return _aggregateAttr;
            }
        }

        private class AggregateDefaultableValue : DefaultableValue<bool>
        {
            internal AggregateDefaultableValue(EFElement parent)
                : base(parent, AttributeAggregate)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeAggregate; }
            }

            public override bool DefaultValue
            {
                get { return false; }
            }
        }

        internal DefaultableValue<bool> BuiltIn
        {
            get
            {
                if (_builtInAttr == null)
                {
                    _builtInAttr = new BuiltInDefaultableValue(this);
                }
                return _builtInAttr;
            }
        }

        private class BuiltInDefaultableValue : DefaultableValue<bool>
        {
            internal BuiltInDefaultableValue(EFElement parent)
                : base(parent, AttributeBuiltIn)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeBuiltIn; }
            }

            public override bool DefaultValue
            {
                get { return false; }
            }
        }

        internal DefaultableValue<string> StoreFunctionName
        {
            get
            {
                if (_storeFunctionNameAttr == null)
                {
                    _storeFunctionNameAttr = new StoreFunctionNameDefaultableValue(this);
                }
                return _storeFunctionNameAttr;
            }
        }

        private class StoreFunctionNameDefaultableValue : DefaultableValue<string>
        {
            internal StoreFunctionNameDefaultableValue(EFElement parent)
                : base(parent, AttributeStoreFunctionName)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeStoreFunctionName; }
            }

            public override string DefaultValue
            {
                get { return null; }
            }
        }

        internal DefaultableValue<bool> NiladicFunction
        {
            get
            {
                if (_niladicFunctionAttr == null)
                {
                    _niladicFunctionAttr = new NiladicFunctionDefaultableValue(this);
                }
                return _niladicFunctionAttr;
            }
        }

        private class NiladicFunctionDefaultableValue : DefaultableValue<bool>
        {
            internal NiladicFunctionDefaultableValue(EFElement parent)
                : base(parent, AttributeNiladicFunction)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeNiladicFunction; }
            }

            public override bool DefaultValue
            {
                get { return false; }
            }
        }

        internal DefaultableValue<bool> IsComposable
        {
            get
            {
                if (_isComposableAttr == null)
                {
                    _isComposableAttr = new IsComposableDefaultableValue(this);
                }
                return _isComposableAttr;
            }
        }

        private class IsComposableDefaultableValue : DefaultableValue<bool>
        {
            internal IsComposableDefaultableValue(EFElement parent)
                : base(parent, AttributeIsComposable)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeIsComposable; }
            }

            public override bool DefaultValue
            {
                get { return true; }
            }
        }

        internal DefaultableValue<string> ParameterTypeSemantic
        {
            get
            {
                if (_parameterTypeSemanticsAttr == null)
                {
                    _parameterTypeSemanticsAttr = new ParameterTypeSemanticDefaultableValue(this);
                }
                return _parameterTypeSemanticsAttr;
            }
        }

        private class ParameterTypeSemanticDefaultableValue : DefaultableValue<string>
        {
            internal ParameterTypeSemanticDefaultableValue(EFElement parent)
                : base(parent, AttributeParameterTypeSemantics)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeParameterTypeSemantics; }
            }

            public override string DefaultValue
            {
                get { return null; }
            }
        }

        internal DefaultableValue<string> Schema
        {
            get
            {
                if (_schemaAttr == null)
                {
                    _schemaAttr = new SchemaDefaultableValue(this);
                }
                return _schemaAttr;
            }
        }

        private class SchemaDefaultableValue : DefaultableValue<string>
        {
            internal SchemaDefaultableValue(EFElement parent)
                : base(parent, AttributeSchema, string.Empty)
            {
                // note: added the string.Empty namespace to distinguish from the
                // StoreSchemaGenerator Schema attribute
            }

            internal override string AttributeName
            {
                get { return AttributeSchema; }
            }

            public override string DefaultValue
            {
                get { return null; }
            }
        }

        /// <summary>
        ///     Manages the content of the StoreSchemaGenerator Schema attribute.
        ///     This is a special attribute put on by the EntityStoreSchemaGenerator.
        ///     It's optional. If present it overrides all other attributes to define the
        ///     schema of the database object (needed for cases where the Function element has
        ///     a child CommandText as in that case the standard SSDL Schema attribute
        ///     (see Schema method above) is not present).
        /// </summary>
        internal DefaultableValue<string> StoreSchemaGeneratorSchema
        {
            get
            {
                if (_storeSchemaGenSchemaAttr == null)
                {
                    _storeSchemaGenSchemaAttr = new StoreSchemaGeneratorSchemaDefaultableValue(this);
                }

                return _storeSchemaGenSchemaAttr;
            }
        }

        private class StoreSchemaGeneratorSchemaDefaultableValue : DefaultableValue<string>
        {
            internal StoreSchemaGeneratorSchemaDefaultableValue(EFElement parent)
                : base(parent, ModelConstants.StoreSchemaGeneratorSchemaAttributeName
                    , SchemaManager.GetEntityStoreSchemaGeneratorNamespaceName())
            {
            }

            internal override string AttributeName
            {
                get { return ModelConstants.StoreSchemaGeneratorSchemaAttributeName; }
            }

            public override string DefaultValue
            {
                get { return null; }
            }
        }

        /// <summary>
        ///     Manages the content of the StoreSchemaGenerator Name attribute.
        ///     This is a special attribute put on by the EntityStoreSchemaGenerator.
        ///     It's optional. If present it overrides all other attributes to define the
        ///     name of the database object (needed for cases where the Function element has
        ///     a child CommandText as in that case the standard SSDL StoreFunctionName attribute
        ///     (see StoreFunctionName method above) may not be present).
        /// </summary>
        internal DefaultableValue<string> StoreSchemaGeneratorName
        {
            get
            {
                if (_storeSchemaGenNameAttr == null)
                {
                    _storeSchemaGenNameAttr = new StoreSchemaGeneratorNameDefaultableValue(this);
                }

                return _storeSchemaGenNameAttr;
            }
        }

        private class StoreSchemaGeneratorNameDefaultableValue : DefaultableValue<string>
        {
            internal StoreSchemaGeneratorNameDefaultableValue(EFElement parent)
                : base(parent, ModelConstants.StoreSchemaGeneratorNameAttributeName
                    , SchemaManager.GetEntityStoreSchemaGeneratorNamespaceName())
            {
            }

            internal override string AttributeName
            {
                get { return ModelConstants.StoreSchemaGeneratorNameAttributeName; }
            }

            public override string DefaultValue
            {
                get { return null; }
            }
        }

        internal IList<Parameter> Parameters()
        {
            return _parameters.AsReadOnly();
        }

        internal void AddParameter(Parameter parameter)
        {
            _parameters.Add(parameter);
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
                foreach (var param in Parameters())
                {
                    yield return param;
                }

                if (_commandText != null)
                {
                    yield return _commandText;
                }

                // add attributes
                yield return ReturnType;
                yield return Aggregate;
                yield return BuiltIn;
                yield return StoreFunctionName;
                yield return NiladicFunction;
                yield return IsComposable;
                yield return ParameterTypeSemantic;
                yield return Schema;
                yield return StoreSchemaGeneratorSchema;
                yield return StoreSchemaGeneratorName;
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

            var child2 = efContainer as CommandText;
            if (child2 != null)
            {
                _commandText = null;
                return;
            }

            base.OnChildDeleted(efContainer);
        }

#if DEBUG
        internal override ICollection<string> MyAttributeNames()
        {
            var s = base.MyAttributeNames();
            s.Add(AttributeAggregate);
            s.Add(AttributeBuiltIn);
            s.Add(AttributeIsComposable);
            s.Add(AttributeNiladicFunction);
            s.Add(AttributeParameterTypeSemantics);
            s.Add(AttributeReturnType);
            s.Add(AttributeSchema);
            s.Add(AttributeStoreFunctionName);
            s.Add(ModelConstants.StoreSchemaGeneratorSchemaAttributeName);
            s.Add(ModelConstants.StoreSchemaGeneratorNameAttributeName);
            return s;
        }
#endif

#if DEBUG
        internal override ICollection<string> MyChildElementNames()
        {
            var s = base.MyChildElementNames();
            s.Add(CommandText.ElementName);
            s.Add(Parameter.ElementName);
            return s;
        }
#endif

        protected override void PreParse()
        {
            Debug.Assert(State != EFElementState.Parsed, "this object should not already be in the parsed state");

            ClearEFObject(_commandText);
            _commandText = null;

            ClearEFObject(_returnTypeAttr);
            _returnTypeAttr = null;
            ClearEFObject(_aggregateAttr);
            _aggregateAttr = null;
            ClearEFObject(_builtInAttr);
            _builtInAttr = null;
            ClearEFObject(_storeFunctionNameAttr);
            _storeFunctionNameAttr = null;
            ClearEFObject(_niladicFunctionAttr);
            _niladicFunctionAttr = null;
            ClearEFObject(_isComposableAttr);
            _isComposableAttr = null;
            ClearEFObject(_parameterTypeSemanticsAttr);
            _parameterTypeSemanticsAttr = null;
            ClearEFObject(_schemaAttr);
            _schemaAttr = null;
            ClearEFObject(_storeSchemaGenSchemaAttr);
            _storeSchemaGenSchemaAttr = null;
            ClearEFObject(_storeSchemaGenNameAttr);
            _storeSchemaGenNameAttr = null;

            ClearEFObjectCollection(_parameters);

            base.PreParse();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal override bool ParseSingleElement(ICollection<XName> unprocessedElements, XElement elem)
        {
            if (elem.Name.LocalName == CommandText.ElementName)
            {
                if (_commandText != null)
                {
                    // multiple CommandText elements
                    var msg = String.Format(CultureInfo.CurrentCulture, Resources.DuplicatedElementMsg, elem.Name.LocalName);
                    Artifact.AddParseErrorForObject(this, msg, ErrorCodes.DUPLICATED_ELEMENT_ENCOUNTERED);
                }
                else
                {
                    _commandText = new CommandText(this, elem);
                    _commandText.Parse(unprocessedElements);
                }
            }
            else if (elem.Name.LocalName == Parameter.ElementName)
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

        protected override void DoNormalize()
        {
            var normalizedName = FunctionNameNormalizer.NameNormalizer(this, LocalName.Value);
            Debug.Assert(null != normalizedName, "Null NormalizedName for refName " + LocalName.Value);
            NormalizedName = (normalizedName != null ? normalizedName.Symbol : Symbol.EmptySymbol);
            base.DoNormalize();
        }

        /// <summary>
        ///     Returns the name of the schema on the underlying database that
        ///     this Function represents
        /// </summary>
        internal string DatabaseSchemaName
        {
            get
            {
                var schemaNameAttr = StoreSchemaGeneratorSchema;
                if (null != schemaNameAttr
                    && !string.IsNullOrEmpty(schemaNameAttr.Value))
                {
                    // if the store:Schema attribute is present then that 
                    // overrides any other attribute and defines the schema name
                    return schemaNameAttr.Value;
                }
                else
                {
                    schemaNameAttr = Schema;
                    if (null != schemaNameAttr
                        && !string.IsNullOrEmpty(schemaNameAttr.Value))
                    {
                        // otherwise if the Schema attribute is present 
                        // then that defines the schema name
                        return schemaNameAttr.Value;
                    }
                    else
                    {
                        // if neither of the above attributes are present then 
                        // the schema name (as defined by the runtime) is the 
                        // value of the Namespace attribute of the parent Schema element
                        var sem = Parent as StorageEntityModel;
                        Debug.Assert(
                            sem != null,
                            "Parent of Function should be a StorageEntityModel. Actual parent has type "
                            + (Parent == null ? "NULL" : Parent.GetType().FullName));
                        if (null == sem)
                        {
                            return null;
                        }
                        else
                        {
                            return sem.Namespace.Value;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Returns the name of the function on the underlying database that
        ///     this Function represents
        /// </summary>
        internal string DatabaseFunctionName
        {
            get
            {
                var funcName = StoreSchemaGeneratorName;
                if (null != funcName
                    && !string.IsNullOrEmpty(funcName.Value))
                {
                    // if the store:Name attribute is present then that 
                    // overrides any other attribute and defines the function name
                    return funcName.Value;
                }
                else
                {
                    funcName = StoreFunctionName;
                    if (null != funcName
                        && !string.IsNullOrEmpty(funcName.Value))
                    {
                        // otherwise if the StoreFunctionName attribute is 
                        // present then that defines the function name
                        return funcName.Value;
                    }
                    else
                    {
                        // if neither of the above attributes are present 
                        // then the function name is defined by the name 
                        // of the Function itself
                        return LocalName.Value;
                    }
                }
            }
        }

        internal FunctionImport FunctionImport
        {
            get
            {
                foreach (var fim in GetAntiDependenciesOfType<FunctionImportMapping>())
                {
                    if (fim.FunctionName.Target == this
                        && fim.FunctionImportName != null
                        && fim.FunctionImportName.Target != null)
                    {
                        return fim.FunctionImportName.Target;
                    }
                }

                return null;
            }
        }
    }
}
