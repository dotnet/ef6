// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using Microsoft.Data.Tools.XmlDesignerBase;

    /// <summary>
    ///     This is the Defaultable Value class for the Property's MaxLength Facet.
    /// </summary>
    internal abstract class DefaultableValueUIntOrNone : DefaultableValue<StringOrPrimitive<UInt32>>
    {
        internal static readonly StringOrPrimitive<UInt32> NoneValue = new StringOrPrimitive<UInt32>(Resources.NoneDisplayValueUsedForUX);

        internal static readonly StringOrPrimitiveConverter<UInt32> _uintOrNoneConverter =
            new StringOrPrimitiveConverter<UInt32>(UInt32.TryParse, Resources.NoneDisplayValueUsedForUX);

        internal static StringOrPrimitiveConverter<UInt32> Converter
        {
            get { return _uintOrNoneConverter; }
        }

        internal DefaultableValueUIntOrNone(EFElement parent, string attributeName)
            : base(parent, attributeName)
        {
        }

        protected internal override StringOrPrimitive<UInt32> ConvertStringToValue(string stringVal)
        {
            return Converter.ValueConverter(stringVal);
        }

        protected internal override string ConvertValueToString(StringOrPrimitive<UInt32> val)
        {
            return StringOrPrimitiveConverter<UInt32>.StringConverter(val);
        }

        public override StringOrPrimitive<uint> DefaultValue
        {
            get { return NoneValue; }
        }

        /// <returns>
        ///     null if the DefaultableValue has its StringValue (i.e. '(None)'), otherwise
        ///     returns the primitive value
        /// </returns>
        internal uint? GetAsNullableUInt()
        {
            if (Value.StringValue != null)
            {
                return null; // this indicates the '(None)' value as a (uint?)
            }

            return Value.PrimitiveValue;
        }

        /// <returns>
        ///     null if the DefaultableValue has its StringValue (i.e. '(None)'), otherwise
        ///     returns the primitive value
        /// </returns>
        internal static StringOrPrimitive<uint> GetFromNullableUInt(uint? nullableUInt)
        {
            if (nullableUInt == null)
            {
                return NoneValue;
            }

            return new StringOrPrimitive<uint>((uint)nullableUInt);
        }
    }
}
