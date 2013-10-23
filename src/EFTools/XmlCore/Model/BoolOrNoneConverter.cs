// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Data.Tools.XmlDesignerBase;

    /// <summary>
    ///     This class will perform the conversions between strings to BoolOrNone, or from a BoolOrNone to a string.
    /// </summary>
    internal static class BoolOrNoneConverter
    {
        private static readonly Dictionary<string, BoolOrNone> _validStrings =
            new Dictionary<string, BoolOrNone>(StringComparer.OrdinalIgnoreCase)
                {
                    // note: accepts versions of true and false with initial letter either capitalized or not capitalized
                    {Resources.NoneDisplayValueUsedForUX, BoolOrNone.NoneValue},
                    {true.ToString(), BoolOrNone.TrueValue},
                    {false.ToString(), BoolOrNone.FalseValue}
                };

        internal static BoolOrNone ValueConverter(string stringVal)
        {
            if (_validStrings.ContainsKey(stringVal))
            {
                return _validStrings[stringVal];
            }

            return null;
        }

        internal static BoolOrNone ValueConverterForBool(bool? boolVal)
        {
            if (boolVal == true)
            {
                return BoolOrNone.TrueValue;
            }
            if (boolVal == false)
            {
                return BoolOrNone.FalseValue;
            }
            return BoolOrNone.NoneValue;
        }

        internal static string StringConverter(BoolOrNone val)
        {
            if (val.StringValue != null)
            {
                // returns None value
                return val.StringValue;
            }
            else
            {
                // returns true and false values as strings
                return "" + val.PrimitiveValue.ToString();
            }
        }
    }
}
