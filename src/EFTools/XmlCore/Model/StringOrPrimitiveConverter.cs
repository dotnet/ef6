// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System.Collections.Generic;
    using System.Globalization;
    using Microsoft.Data.Tools.XmlDesignerBase;

    /// <summary>
    ///     This class will perform the conversions between strings to StringOrPrimitive, or from a StringOrPrimitive to a string.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class StringOrPrimitiveConverter<T>
    {
        internal delegate bool TryParse(string s, out T result);

        private readonly TryParse _tryParse;
        private readonly HashSet<string> _validStrings;

        internal StringOrPrimitiveConverter(TryParse tryParse, params string[] validStrings)
        {
            _tryParse = tryParse;
            _validStrings = new HashSet<string>();
            if (validStrings != null)
            {
                foreach (var s in validStrings)
                {
                    _validStrings.Add(s);
                }
            }
        }

        internal virtual bool IsLegalString(string stringVal)
        {
            return _validStrings.Contains(stringVal);
        }

        internal StringOrPrimitive<T> ValueConverter(string stringVal)
        {
            StringOrPrimitive<T> stringOrPrimitive = null;

            if (IsLegalString(stringVal))
            {
                stringOrPrimitive = new StringOrPrimitive<T>(stringVal);
            }
            else
            {
                T value;

                if (_tryParse(stringVal, out value))
                {
                    stringOrPrimitive = new StringOrPrimitive<T>(value);
                }
                else
                {
                    var message = string.Format(CultureInfo.CurrentCulture, Resources.ConversionExceptionMessage, value);
                    throw new ConversionException(message);
                }
            }
            return stringOrPrimitive;
        }

        internal static string StringConverter(StringOrPrimitive<T> val)
        {
            if (val.StringValue != null)
            {
                return val.StringValue;
            }
            else
            {
                return "" + val.PrimitiveValue;
            }
        }
    }
}
