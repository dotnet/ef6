// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    /// <summary>
    ///     This is the Defaultable Value class for the any _optional_ string attribute.
    /// </summary>
    internal abstract class DefaultableValueStringOrNone : DefaultableValue<StringOrNone>
    {
        internal DefaultableValueStringOrNone(EFElement parent, string attributeName)
            : base(parent, attributeName)
        {
        }

        protected internal override StringOrNone ConvertStringToValue(string stringVal)
        {
            return StringOrNoneConverter.ValueConverter(stringVal);
        }

        protected internal override string ConvertValueToString(StringOrNone val)
        {
            return StringOrNoneConverter.StringConverter(val);
        }

        public override StringOrNone DefaultValue
        {
            get { return StringOrNone.NoneValue; }
        }
    }
}
