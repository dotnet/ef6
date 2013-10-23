// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Commands;

    [DebuggerDisplay("{Parent.ToString(),nq}.{LocalName.Value,nq}")]
    internal abstract class Property : PropertyBase
    {
        // is used when creating default names for Scalar Properties - always the same is every language so not localized 
        internal const string DefaultPropertyName = "Property";

        internal const string ElementName = "Property";

        internal const string AttributeType = "Type";
        internal const string AttributeConcurrencyMode = "ConcurrencyMode";

        // facets attributes
        internal const string AttributeNullable = "Nullable";
        internal const string AttributeDefaultValue = "DefaultValue";
        internal const string AttributeMaxLength = "MaxLength";
        internal const string AttributeFixedLength = "FixedLength";
        internal const string AttributePrecision = "Precision";
        internal const string AttributeScale = "Scale";
        internal const string AttributeUnicode = "Unicode";
        internal const string AttributeCollation = "Collation";

        internal static readonly string MaxLengthMaxValue = "Max";
        internal static readonly StringOrPrimitive<UInt32> MaxLengthMaxValueObject = new StringOrPrimitive<UInt32>(MaxLengthMaxValue);

        // general (including Name from base class)
        private DefaultableValue<string> _concurrencyModeAttr;
        private DefaultableValue<string> _storeGeneratedPatternAttr;

        // facets
        private DefaultableValueBoolOrNone _nullableAttr;
        private DefaultableValueStringOrNone _defaultAttr;
        private DefaultableValue<StringOrPrimitive<UInt32>> _maxLengthAttr;
        private DefaultableValueBoolOrNone _fixedLengthAttr;
        private DefaultableValueUIntOrNone _precisionAttr;
        private DefaultableValueUIntOrNone _scaleAttr;
        private DefaultableValueBoolOrNone _unicodeAttr;
        private DefaultableValueStringOrNone _collationAttr;

        protected Property(EFElement parent, XElement element)
            : this(parent, element, null)
        {
            // nothing
        }

        /// <summary>
        ///     Create a property at the a specified position.
        /// </summary>
        /// <param name="parent">Property's Parent.</param>
        /// <param name="element">Property's XElement</param>
        /// <param name="insertPosition">Information where the property should be inserted to. If the parameter is null, the property will be placed as the last property of the entity.</param>
        protected Property(EFElement parent, XElement element, InsertPropertyPosition insertPosition)
            : base(parent, element, insertPosition)
        {
            // nothing
        }

        internal override string EFTypeName
        {
            get { return ElementName; }
        }

        internal bool IsEntityTypeProperty
        {
            get { return Parent is EntityType; }
        }

        internal bool IsComplexTypeProperty
        {
            get { return Parent is ComplexType; }
        }

        internal bool IsKeyProperty
        {
            get
            {
                var et = EntityType;
                if (et != null
                    && et.Key != null)
                {
                    return et.Key.GetPropertyRef(this) != null;
                }

                return false;
            }
        }

        internal abstract string TypeName { get; }

        protected class TypeDefaultableValue : DefaultableValue<string>
        {
            internal TypeDefaultableValue(Property parent)
                : base(parent, AttributeType)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeType; }
            }

            public override string DefaultValue
            {
                get { return ModelConstants.DefaultPropertyType; }
            }
        }

        /// <summary>
        ///     Manages the content of the Nullable attribute
        /// </summary>
        internal DefaultableValueBoolOrNone Nullable
        {
            get
            {
                if (_nullableAttr == null)
                {
                    _nullableAttr = new NullableDefaultableValue(this);
                }
                return _nullableAttr;
            }
        }

        private class NullableDefaultableValue : DefaultableValueBoolOrNone
        {
            internal NullableDefaultableValue(Property parent)
                : base(parent, AttributeNullable)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeNullable; }
            }
        }

        /// <summary>
        ///     Manages the content of the DefaultValue attribute
        /// </summary>
        internal DefaultableValueStringOrNone DefaultValue
        {
            get
            {
                if (_defaultAttr == null)
                {
                    _defaultAttr = new DefaultValueDefaultableValue(this);
                }
                return _defaultAttr;
            }
        }

        private class DefaultValueDefaultableValue : DefaultableValueStringOrNone
        {
            internal DefaultValueDefaultableValue(Property parent)
                : base(parent, AttributeDefaultValue)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeDefaultValue; }
            }
        }

        /// <summary>
        ///     Manages the content of the ConcurrencyMode attribute
        /// </summary>
        internal DefaultableValue<string> ConcurrencyMode
        {
            get
            {
                if (_concurrencyModeAttr == null)
                {
                    _concurrencyModeAttr = new ConcurrencyModeDefaultableValue(this);
                }
                return _concurrencyModeAttr;
            }
        }

        private class ConcurrencyModeDefaultableValue : DefaultableValue<string>
        {
            internal ConcurrencyModeDefaultableValue(Property parent)
                : base(parent, AttributeConcurrencyMode)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeConcurrencyMode; }
            }

            public override string DefaultValue
            {
                get { return ModelConstants.ConcurrencyModeNone; }
            }
        }

        /// <summary>
        ///     Manages the content of the MaxLength attribute
        /// </summary>
        internal DefaultableValue<StringOrPrimitive<UInt32>> MaxLength
        {
            get
            {
                if (_maxLengthAttr == null)
                {
                    _maxLengthAttr = new DefaultableValueMaxLength(this, AttributeMaxLength);
                }
                return _maxLengthAttr;
            }
        }

        internal static StringOrPrimitive<UInt32> GetMaxLengthDefault()
        {
            return new StringOrPrimitive<UInt32>(GetMaxLengthDefaultUInt32());
        }

        internal static UInt32 GetMaxLengthDefaultUInt32()
        {
            return int.MaxValue;
        }

        /// <summary>
        ///     Manages the content of the FixedLength attribute
        /// </summary>
        internal DefaultableValueBoolOrNone FixedLength
        {
            get
            {
                if (_fixedLengthAttr == null)
                {
                    _fixedLengthAttr = new FixedLengthDefaultableValue(this);
                }
                return _fixedLengthAttr;
            }
        }

        private class FixedLengthDefaultableValue : DefaultableValueBoolOrNone
        {
            internal FixedLengthDefaultableValue(Property parent)
                : base(parent, AttributeFixedLength)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeFixedLength; }
            }

            public override BoolOrNone DefaultValue
            {
                get { return ((Property)Parent).GetFixedLengthDefault(); }
            }
        }

        protected virtual BoolOrNone GetFixedLengthDefault()
        {
            return BoolOrNone.NoneValue;
        }

        /// <summary>
        ///     Manages the content of the Precision attribute
        /// </summary>
        internal DefaultableValueUIntOrNone Precision
        {
            get
            {
                if (_precisionAttr == null)
                {
                    _precisionAttr = new PrecisionDefaultableValue(this);
                }
                return _precisionAttr;
            }
        }

        private class PrecisionDefaultableValue : DefaultableValueUIntOrNone
        {
            internal PrecisionDefaultableValue(Property parent)
                : base(parent, AttributePrecision)
            {
            }

            internal override string AttributeName
            {
                get { return AttributePrecision; }
            }

            public override StringOrPrimitive<uint> DefaultValue
            {
                get
                {
                    var precisionDefault = ((Property)Parent).GetPrecisionDefault();
                    return precisionDefault;
                }
            }
        }

        protected virtual StringOrPrimitive<uint> GetPrecisionDefault()
        {
            return DefaultableValueUIntOrNone.NoneValue;
        }

        /// <summary>
        ///     Manages the content of the Scale attribute
        /// </summary>
        internal DefaultableValueUIntOrNone Scale
        {
            get
            {
                if (_scaleAttr == null)
                {
                    _scaleAttr = new ScaleDefaultableValue(this);
                }
                return _scaleAttr;
            }
        }

        private class ScaleDefaultableValue : DefaultableValueUIntOrNone
        {
            internal ScaleDefaultableValue(Property parent)
                : base(parent, AttributeScale)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeScale; }
            }

            public override StringOrPrimitive<uint> DefaultValue
            {
                get
                {
                    var scaleDefault = ((Property)Parent).GetScaleDefault();
                    return scaleDefault;
                }
            }
        }

        protected virtual StringOrPrimitive<uint> GetScaleDefault()
        {
            return DefaultableValueUIntOrNone.NoneValue;
        }

        /// <summary>
        ///     Manages the content of the Unicode attribute
        /// </summary>
        internal DefaultableValueBoolOrNone Unicode
        {
            get
            {
                if (_unicodeAttr == null)
                {
                    _unicodeAttr = new UnicodeDefaultableValue(this);
                }
                return _unicodeAttr;
            }
        }

        private class UnicodeDefaultableValue : DefaultableValueBoolOrNone
        {
            internal UnicodeDefaultableValue(Property parent)
                : base(parent, AttributeUnicode)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeUnicode; }
            }

            public override BoolOrNone DefaultValue
            {
                get { return ((Property)Parent).GetUnicodeDefault(); }
            }
        }

        protected virtual BoolOrNone GetUnicodeDefault()
        {
            return BoolOrNone.NoneValue;
        }

        /// <summary>
        ///     Manages the content of the Collation attribute
        /// </summary>
        internal DefaultableValueStringOrNone Collation
        {
            get
            {
                if (_collationAttr == null)
                {
                    _collationAttr = new CollationDefaultableValue(this);
                }
                return _collationAttr;
            }
        }

        protected virtual StringOrNone GetCollationDefault()
        {
            return StringOrNone.NoneValue;
        }

        private class CollationDefaultableValue : DefaultableValueStringOrNone
        {
            internal CollationDefaultableValue(Property parent)
                : base(parent, AttributeCollation)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeCollation; }
            }

            public override StringOrNone DefaultValue
            {
                get { return ((Property)Parent).GetCollationDefault(); }
            }
        }

        /// <summary>
        ///     Manages the content of the StoreGeneratedPattern attribute
        /// </summary>
        internal DefaultableValue<string> StoreGeneratedPattern
        {
            get
            {
                if (_storeGeneratedPatternAttr == null)
                {
                    _storeGeneratedPatternAttr = GetStoreGeneratedPatternDefaultableValue();
                }
                return _storeGeneratedPatternAttr;
            }
        }

        protected abstract DefaultableValue<string> GetStoreGeneratedPatternDefaultableValue();

        /// <summary>
        ///     Remove all facets except Nullable (i.e. the setting of Nullable remains as is).
        /// </summary>
        internal void RemoveAllFacetsExceptNullable()
        {
            if (null != DefaultValue)
            {
                DefaultValue.Delete();
                _defaultAttr = null;
            }
            if (null != MaxLength)
            {
                MaxLength.Delete();
                _maxLengthAttr = null;
            }
            if (null != FixedLength)
            {
                FixedLength.Delete();
                _fixedLengthAttr = null;
            }
            if (null != Precision)
            {
                Precision.Delete();
                _precisionAttr = null;
            }
            if (null != Scale)
            {
                Scale.Delete();
                _scaleAttr = null;
            }
            if (null != Unicode)
            {
                Unicode.Delete();
                _unicodeAttr = null;
            }
            if (null != Collation)
            {
                Collation.Delete();
                _collationAttr = null;
            }
        }

        internal BaseEntityModel EntityModel
        {
            get
            {
                var model = Parent.Parent as BaseEntityModel;
                Debug.Assert(model != null, "this.Parent.Parent should be a BaseEntityModel");
                return model;
            }
        }

#if DEBUG
        internal override ICollection<string> MyAttributeNames()
        {
            var s = base.MyAttributeNames();
            s.Add(AttributeType);
            s.Add(AttributeNullable);
            s.Add(AttributeDefaultValue);
            s.Add(AttributeConcurrencyMode);
            s.Add(AttributeMaxLength);
            s.Add(AttributeFixedLength);
            s.Add(AttributePrecision);
            s.Add(AttributeScale);
            s.Add(AttributeUnicode);
            s.Add(AttributeCollation);
            return s;
        }
#endif

        protected override void PreParse()
        {
            Debug.Assert(State != EFElementState.Parsed, "this object should not already be in the parsed state");

            ClearEFObject(_nullableAttr);
            _nullableAttr = null;

            ClearEFObject(_defaultAttr);
            _defaultAttr = null;

            ClearEFObject(_concurrencyModeAttr);
            _concurrencyModeAttr = null;

            ClearEFObject(_maxLengthAttr);
            _maxLengthAttr = null;

            ClearEFObject(_fixedLengthAttr);
            _fixedLengthAttr = null;

            ClearEFObject(_precisionAttr);
            _precisionAttr = null;

            ClearEFObject(_scaleAttr);
            _scaleAttr = null;

            ClearEFObject(_unicodeAttr);
            _unicodeAttr = null;

            ClearEFObject(_collationAttr);
            _collationAttr = null;

            ClearEFObject(_storeGeneratedPatternAttr);
            _storeGeneratedPatternAttr = null;

            base.PreParse();
        }

        /// <summary>
        ///     A property's normalized name is [model namespace].[entitytype].[property name]
        /// </summary>
        protected override void DoNormalize()
        {
            var normalizedName = PropertyNameNormalizer.NameNormalizer(this, LocalName.Value);
            Debug.Assert(null != normalizedName, "Null NormalizedName for refName " + LocalName.Value);
            NormalizedName = (normalizedName != null ? normalizedName.Symbol : Symbol.EmptySymbol);
            base.DoNormalize();
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
                yield return Nullable;
                yield return DefaultValue;
                yield return ConcurrencyMode;

                yield return MaxLength;
                yield return FixedLength;
                yield return Precision;
                yield return Scale;
                yield return Unicode;
                yield return Collation;

                yield return StoreGeneratedPattern;
            }
        }

        /// <summary>
        ///     A property is always referred to by its bare Name.  This is because a property is always
        ///     referred to in the context of an EntityType.
        /// </summary>
        internal override string GetRefNameForBinding(ItemBinding binding)
        {
            return LocalName.Value;
        }

        internal override DeleteEFElementCommand GetDeleteCommand()
        {
            DeleteEFElementCommand cmd = new DeletePropertyCommand(this);
            if (cmd == null)
            {
                // shouldn't happen, just to be safe
                throw new InvalidOperationException();
            }
            return cmd;
        }
    }
}
