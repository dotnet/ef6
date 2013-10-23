// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;

    /// <summary>
    ///     This class will perform the conversions to/from strings to StringOrNone
    /// </summary>
    internal static class StringOrNoneConverter
    {
        internal static StringOrNone ValueConverter(string stringVal)
        {
            if (StringOrNone.NoneUnderlyingValue.Equals(stringVal, StringComparison.Ordinal))
            {
                return StringOrNone.NoneValue;
            }
            else
            {
                return new StringOrNone(stringVal);
            }
        }

        internal static string StringConverter(StringOrNone val)
        {
            // for NoneValue this will return StringOrNone.NoneUnderlyingValue
            return val.ToString();
        }
    }
}
