// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;

    // Represents Property info stored in Clipboard
    [Serializable]
    internal class PropertyClipboardFormat : PropertyBaseClipboardFormat
    {
        private readonly string _propertyType;
        private bool? _isNullable;
        private bool _isKeyProperty;
        private readonly StringOrNone _defaultValue;
        private readonly string _concurrencyMode;
        private readonly bool _isConceptualProperty;
        private readonly bool _isComplexProperty;

        // facets
        private readonly StringOrPrimitive<UInt32> _maxLength;
        private readonly bool? _fixedLength;
        private readonly StringOrPrimitive<UInt32> _precision;
        private readonly StringOrPrimitive<UInt32> _scale;
        private readonly bool? _unicode;
        private readonly StringOrNone _collation;

        // annotations
        private string _storeGeneratedPattern;

        internal PropertyClipboardFormat(Property property)
            : base(property)
        {
            var complexProperty = property as ComplexConceptualProperty;
            if (complexProperty != null)
            {
                _propertyType = complexProperty.ComplexType.RefName;
                _isComplexProperty = true;
            }
            else
            {
                _propertyType = property.TypeName;
                _isComplexProperty = false;
            }
            _isNullable = (property.Nullable.Value.StringValue == null ? property.Nullable.Value.PrimitiveValue : (bool?)null);
            _isKeyProperty = property.IsKeyProperty;
            _defaultValue = property.DefaultValue.Value;
            _concurrencyMode = property.ConcurrencyMode.IsDefaulted ? null : property.ConcurrencyMode.Value;
            _maxLength = property.MaxLength.IsDefaulted ? null : property.MaxLength.Value;
            _fixedLength = property.FixedLength.GetAsNullableBool();
            _precision = property.Precision.Value;
            _scale = property.Scale.Value;
            _unicode = property.Unicode.GetAsNullableBool();
            _collation = property.Collation.Value;

            var conceptualProp = property as ConceptualProperty;
            if (conceptualProp != null)
            {
                _isConceptualProperty = true;
                _storeGeneratedPattern = conceptualProp.StoreGeneratedPattern.IsDefaulted
                                             ? null
                                             : conceptualProp.StoreGeneratedPattern.Value;
            }
            else
            {
                _isConceptualProperty = false;
                _storeGeneratedPattern = null;
            }
        }

        internal string TraceString()
        {
            return "[" + typeof(PropertyClipboardFormat).Name +
                   " name=" + PropertyName +
                   ", getter=" + GetterAccessModifier +
                   ", setter=" + SetterAccessModifier +
                   ", propertyType=" + _propertyType +
                   ", isNullable=" + _isNullable +
                   ", isKeyProperty=" + _isKeyProperty +
                   ", defaultValue=" + _defaultValue +
                   ", concurrencyMode=" + _concurrencyMode +
                   ", isConceptualProperty=" + _isConceptualProperty +
                   ", isComplexProperty=" + _isComplexProperty +
                   ", maxLength=" + _maxLength +
                   ", fixedLength=" + _fixedLength +
                   ", precision=" + _precision +
                   ", scale =" + _scale +
                   ", unicode=" + _unicode +
                   ", collation=" + _collation +
                   ", storeGeneratedPattern=" + _storeGeneratedPattern +
                   "]";
        }

        internal bool IsConceptualProperty
        {
            get { return _isConceptualProperty; }
        }

        internal bool IsComplexProperty
        {
            get { return _isComplexProperty; }
        }

        [ClipboardPropertyMap(Property.AttributeType)]
        internal string PropertyType
        {
            get { return _propertyType; }
        }

        [ClipboardPropertyMap(Property.AttributeNullable)]
        internal bool? IsNullable
        {
            get { return _isNullable; }
            set { _isNullable = value; }
        }

        internal bool IsKeyProperty
        {
            get { return _isKeyProperty; }
            set { _isKeyProperty = value; }
        }

        [ClipboardPropertyMap(Property.AttributeDefaultValue)]
        internal StringOrNone Default
        {
            get { return _defaultValue; }
        }

        [ClipboardPropertyMap(Property.AttributeMaxLength)]
        internal StringOrPrimitive<UInt32> MaxLength
        {
            get { return _maxLength; }
        }

        [ClipboardPropertyMap(Property.AttributeFixedLength)]
        internal bool? FixedLength
        {
            get { return _fixedLength; }
        }

        [ClipboardPropertyMap(Property.AttributePrecision)]
        internal StringOrPrimitive<UInt32> Precision
        {
            get { return _precision; }
        }

        [ClipboardPropertyMap(Property.AttributeScale)]
        internal StringOrPrimitive<UInt32> Scale
        {
            get { return _scale; }
        }

        [ClipboardPropertyMap(Property.AttributeUnicode)]
        internal bool? Unicode
        {
            get { return _unicode; }
        }

        [ClipboardPropertyMap(Property.AttributeCollation)]
        internal StringOrNone Collation
        {
            get { return _collation; }
        }

        [ClipboardPropertyMap(Property.AttributeConcurrencyMode)]
        internal string ConcurrencyMode
        {
            get { return _concurrencyMode; }
        }

        [ClipboardPropertyMap(StoreGeneratedPatternForCsdlDefaultableValue.AttributeStoreGeneratedPattern)]
        internal string StoreGeneratedPattern
        {
            get { return _storeGeneratedPattern; }
            set { _storeGeneratedPattern = value; }
        }
    }
}
