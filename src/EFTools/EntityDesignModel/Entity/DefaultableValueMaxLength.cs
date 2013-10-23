// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using Microsoft.Data.Tools.XmlDesignerBase;

    /// <summary>
    ///     This is the Defaultable Value class for the Property's MaxLength Facet.
    /// </summary>
    internal class DefaultableValueMaxLength : DefaultableValueUIntOrNone
    {
        internal static readonly StringOrPrimitiveConverter<UInt32> _maxLengthConverter =
            new StringOrPrimitiveConverter<UInt32>(
                UInt32.TryParse, Property.MaxLengthMaxValue, Resources.NoneDisplayValueUsedForUX);

        internal static StringOrPrimitiveConverter<UInt32> MaxLengthConverter
        {
            get { return _maxLengthConverter; }
        }

        internal DefaultableValueMaxLength(EFElement parent, string attributeName)
            : base(parent, attributeName)
        {
        }

        protected internal override StringOrPrimitive<UInt32> ConvertStringToValue(string stringVal)
        {
            return MaxLengthConverter.ValueConverter(stringVal);
        }

        protected internal override string ConvertValueToString(StringOrPrimitive<UInt32> val)
        {
            return StringOrPrimitiveConverter<UInt32>.StringConverter(val);
        }

        internal override string AttributeName
        {
            get { return Property.AttributeMaxLength; }
        }

        public override StringOrPrimitive<uint> DefaultValue
        {
            get { return NoneValue; }
        }
    }
}
