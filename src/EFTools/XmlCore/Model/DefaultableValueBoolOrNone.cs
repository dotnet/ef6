// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Diagnostics.CodeAnalysis;

    internal enum BoolOrNoneComparison
    {
        Equal,
        NotEqual,
        Unknown,
    }

    /// <summary>
    ///     This is the Defaultable Value class for the any _optional_ bool attribute.
    /// </summary>
    internal abstract class DefaultableValueBoolOrNone : DefaultableValue<BoolOrNone>
    {
        internal DefaultableValueBoolOrNone(EFElement parent, string attributeName)
            : base(parent, attributeName)
        {
        }

        protected internal override BoolOrNone ConvertStringToValue(string stringVal)
        {
            return BoolOrNoneConverter.ValueConverter(stringVal);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase",
            Justification = "Must call ToLowerInvariant to match XSD restriction.")]
        protected internal override string ConvertValueToString(BoolOrNone val)
        {
            // must use lower case for attribute value to match XSD restrictions
            return BoolOrNoneConverter.StringConverter(val).ToLowerInvariant();
        }

        public override BoolOrNone DefaultValue
        {
            get { return BoolOrNone.NoneValue; }
        }

        /// <returns>
        ///     null if the DefaultableValue has its StringValue (i.e. '(None)'), otherwise
        ///     returns the primitive value
        /// </returns>
        internal bool? GetAsNullableBool()
        {
            if (Value.StringValue != null)
            {
                return null; // this indicates the '(None)' value as a (bool?)
            }

            return Value.PrimitiveValue;
        }

        /// <returns>
        ///     null if the DefaultableValue has its StringValue (i.e. '(None)'), otherwise
        ///     returns the primitive value
        /// </returns>
        internal static BoolOrNone GetFromNullableBool(bool? nullableBool)
        {
            if (nullableBool == null)
            {
                return BoolOrNone.NoneValue;
            }

            return (nullableBool.Value ? BoolOrNone.TrueValue : BoolOrNone.FalseValue);
        }

        internal BoolOrNoneComparison CompareToUsingDefault(bool value)
        {
            var stringValue = Value.StringValue;
            if (null != stringValue)
            {
                // this is using stringValue i.e. is set to 'None' - do we have a primitiveValue default?
                var defaultValue = DefaultValue;
                if (null != defaultValue.StringValue)
                {
                    // even the default value is set to 'None' - so cannot compare to value
                    return BoolOrNoneComparison.Unknown;
                }

                // use default value to compare to primitive value
                var primitiveValue = defaultValue.PrimitiveValue;
                return (primitiveValue == value ? BoolOrNoneComparison.Equal : BoolOrNoneComparison.NotEqual);
            }
            else
            {
                // this is set to a primitiveValue - so compare using primitiveValue
                var primitiveValue = Value.PrimitiveValue;
                return (primitiveValue == value ? BoolOrNoneComparison.Equal : BoolOrNoneComparison.NotEqual);
            }
        }
    }
}
