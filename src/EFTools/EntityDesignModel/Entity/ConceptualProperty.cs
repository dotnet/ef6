// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Xml.Linq;

    internal class ConceptualProperty : Property
    {
        private DefaultableValue<string> _typeAttrForPrimitiveType;
        private SingleItemBinding<EnumType> _typeAttrForEnumType;

        public ConceptualProperty(EFElement parent, XElement element)
            : this(parent, element, null)
        {
        }

        /// <summary>
        ///     Create a Conceptual property at a specified position.
        /// </summary>
        /// <param name="parent">Property's Parent. The value is either ConceptualEntityTYpe or a ComplexType.</param>
        /// <param name="element">Property's XElement</param>
        /// <param name="insertPosition">Information where the property should be inserted to. If the parameter is null, the property will be placed as the last property of the entity.</param>
        public ConceptualProperty(EFElement parent, XElement element, InsertPropertyPosition insertPosition)
            : base(parent, element, insertPosition)
        {
            Debug.Assert(
                parent is ConceptualEntityType || parent is ComplexType,
                "Parent of ConceptualProperty should be either ConceptualEntityType or ComplexType");
        }

        public string PrimitiveTypeName
        {
            get
            {
                Debug.Assert(
                    _typeAttrForPrimitiveType != null || _typeAttrForEnumType != null,
                    typeof(ConceptualProperty).Name + ".PrimitiveTypeName: Type attribute is not initialized.");

                if (_typeAttrForEnumType != null
                    && _typeAttrForEnumType.Status == BindingStatus.Known)
                {
                    return _typeAttrForEnumType.Target.UnderlyingType.Value;
                }
                else if (_typeAttrForPrimitiveType != null)
                {
                    return _typeAttrForPrimitiveType.Value;
                }
                return null;
            }
        }

        public bool IsEnumType
        {
            get { return (_typeAttrForEnumType != null); }
        }

        /// <summary>
        ///     Change the property type to a primitive type.
        /// </summary>
        /// <param name="typeName"></param>
        public void ChangePropertyType(string typeName)
        {
            if (_typeAttrForEnumType != null)
            {
                _typeAttrForEnumType.Unbind();
                ClearEFObject(_typeAttrForEnumType);
                _typeAttrForEnumType = null;
            }

            PrimitiveTypeAttribute.Value = typeName;
        }

        /// <summary>
        ///     Change the property type to an enum type.
        /// </summary>
        /// <param name="typeName"></param>
        public void ChangePropertyType(EnumType enumType)
        {
            Debug.Assert(enumType != null, "typeName parameter value is null");
            if (enumType != null)
            {
                ClearEFObject(_typeAttrForPrimitiveType);
                _typeAttrForPrimitiveType = null;

                EnumTypeAttribute.SetRefName(enumType);
                EnumTypeAttribute.Rebind();
            }
        }

        public void UnbindEnumType()
        {
            Debug.Assert(IsEnumType, typeof(ConceptualEntityType).Name + ".UnbindEnumType: the property type is not an enum.");

            if (IsEnumType)
            {
                _typeAttrForEnumType.Unbind();
            }
        }

        internal override string TypeName
        {
            get
            {
                EnsureTypeAttributeInitialized();

                if (IsEnumType)
                {
                    if (_typeAttrForEnumType.Status == BindingStatus.Known)
                    {
                        return _typeAttrForEnumType.Target.LocalName.Value;
                    }
                    else
                    {
                        // only show the last part of the name (without namespace)
                        return
                            _typeAttrForEnumType.RefName.Substring(
                                _typeAttrForEnumType.RefName.LastIndexOf(Symbol.VALID_RUNTIME_SEPARATOR) + 1);
                    }
                }
                return _typeAttrForPrimitiveType.Value;
            }
        }

        protected override DefaultableValue<string> GetStoreGeneratedPatternDefaultableValue()
        {
            return new StoreGeneratedPatternForCsdlDefaultableValue(this);
        }

        protected override void PreParse()
        {
            Debug.Assert(State != EFElementState.Parsed, "this object should not already be in the parsed state");

            ClearEFObject(_typeAttrForPrimitiveType);
            _typeAttrForPrimitiveType = null;

            ClearEFObject(_typeAttrForEnumType);
            _typeAttrForEnumType = null;

            base.PreParse();
        }

        protected override void DoResolve(EFArtifactSet artifactSet)
        {
            if (_typeAttrForEnumType != null)
            {
                _typeAttrForEnumType.Rebind();
                if (_typeAttrForEnumType.Status == BindingStatus.Known)
                {
                    State = EFElementState.Resolved;
                }
                return;
            }
            base.DoResolve(artifactSet);
        }

        private void EnsureTypeAttributeInitialized()
        {
            if (_typeAttrForPrimitiveType == null
                && _typeAttrForEnumType == null)
            {
                ChangePropertyType(GetTypeAttributeValue());
            }
        }

        private DefaultableValue<string> PrimitiveTypeAttribute
        {
            get
            {
                if (_typeAttrForPrimitiveType == null)
                {
                    _typeAttrForPrimitiveType = new TypeDefaultableValue(this);
                }
                return _typeAttrForPrimitiveType;
            }
        }

        private SingleItemBinding<EnumType> EnumTypeAttribute
        {
            get
            {
                if (_typeAttrForEnumType == null)
                {
                    _typeAttrForEnumType = new SingleItemBinding<EnumType>(
                        this,
                        AttributeType,
                        EFNormalizableItemDefaults.DefaultNameNormalizerForEDM);
                }
                return _typeAttrForEnumType;
            }
        }

        private string GetTypeAttributeValue()
        {
            string attrValue = null;
            var attr = XElement.FirstAttribute;
            while (attr != null)
            {
                if (attr.Name.LocalName == AttributeType)
                {
                    attrValue = attr.Value;
                    break;
                }
                attr = attr.NextAttribute;
            }

            // attrValue is null if there is no type attribute, obviously a mal-formed document, but possible
            if (attrValue != null)
            {
                return attrValue;
            }

            return String.Empty;
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
                if (IsEnumType)
                {
                    yield return EnumTypeAttribute;
                }
                else
                {
                    yield return PrimitiveTypeAttribute;
                }
            }
        }
    }
}
