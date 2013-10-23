// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.Linq;

    internal abstract class DefaultableValue : EFAttribute
    {
        /// <summary>
        ///     A special decimal constant that will be interpreted as the string
        ///     UnboundedString when parsing decimals.
        /// </summary>
        internal static readonly decimal UnboundedDecimal = -1;

        /// <summary>
        ///     A special string constant that will be interpreted as the decimal
        ///     UnboundedDecimal when parsing decimals.
        /// </summary>
        internal static readonly string UnboundedString = "unbounded";

        /// <summary>
        ///     Returns the name of the property of the parent object that is
        ///     represented by this defaultable value.
        /// </summary>
        internal abstract string PropertyName { get; }

        /// <summary>
        ///     Returns the value of the field, which is either the supplied value
        ///     or the default value if no value was supplied.
        /// </summary>
        internal abstract object ObjectValue { get; }

        /// <summary>
        ///     Indicates that no value was supplied, and thus the Value is the
        ///     default value.
        /// </summary>
        public abstract bool IsDefaulted { get; }

        /// <summary>
        ///     Creates a new DefaultableValue that wraps an XAttribute
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="xattribute"></param>
        protected DefaultableValue(EFElement parent, XAttribute xattribute)
            : base(parent, xattribute)
        {
        }

        /// <summary>
        ///     This method converts the passed in value to a string, using the XmlConvert class.
        ///     NOTE: If this is string, do not encode it.
        /// </summary>
        /// <param name="value">The value that we want to convert to a string</param>
        /// <returns></returns>
        protected static string GetXmlString(object value)
        {
            if (value is int)
            {
                return XmlConvert.ToString((int)value);
            }
            if (value is uint)
            {
                return XmlConvert.ToString((uint)value);
            }
            else if (value is double)
            {
                return XmlConvert.ToString((double)value);
            }
            else if (value is float)
            {
                return XmlConvert.ToString((float)value);
            }
            else if (value is decimal)
            {
                var decimalValue = (decimal)value;
                if (decimalValue == UnboundedDecimal)
                {
                    return UnboundedString;
                }
                else
                {
                    return XmlConvert.ToString(decimalValue);
                }
            }
            else if (value is bool)
            {
                var val = (bool)value;
                return XmlConvert.ToString(val);
            }
            else if (value is string)
            {
                return value.ToString();
            }
            else if (value is Enum)
            {
                return value.ToString();
            }
            else if (value is XNamespace)
            {
                return value.ToString();
            }
            else
            {
                Debug.Assert(false);
                return value.ToString();
            }
        }

        /// <summary>
        ///     This method converts the contents of the XAttribute into an object of the passed
        ///     in type.  This is mainly done using the casting capabilities of the XAttribute class.
        /// </summary>
        /// <param name="attrValue">The string passed here should be decoded as needed.</param>
        /// <param name="attribute">The XAttribute to read</param>
        /// <param name="objectType">The type of object to return</param>
        /// <returns></returns>
        protected static object ConvertToType(string stringVal, XAttribute attribute, Type objectType)
        {
            object result = null;
            if (objectType == typeof(int))
            {
                result = (int)attribute;
            }
            else if (objectType == typeof(uint))
            {
                result = (uint)attribute;
            }
            else if (objectType == typeof(uint?))
            {
                result = (uint?)attribute;
            }
            else if (objectType == typeof(double))
            {
                result = (double)attribute;
            }
            else if (objectType == typeof(float))
            {
                result = (float)attribute;
            }
            else if (objectType == typeof(decimal))
            {
                if (stringVal == UnboundedString)
                {
                    result = UnboundedDecimal;
                }
                else
                {
                    result = (decimal)attribute;
                }
            }
            else if (objectType == typeof(bool))
            {
                result = (bool)attribute;
            }
            else if (objectType == typeof(bool?))
            {
                result = (bool?)attribute;
            }
            else if (typeof(Enum).IsAssignableFrom(objectType))
            {
                foreach (var enumVal in Enum.GetValues(objectType))
                {
                    if (string.Compare(stringVal, Enum.GetName(objectType, enumVal), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        result = enumVal;
                        break;
                    }
                }
            }
            else if (objectType == typeof(XNamespace))
            {
                XNamespace ns = null;
                if (stringVal != null)
                {
                    ns = XNamespace.Get(stringVal);
                }
                return ns;
            }
            else
            {
                result = stringVal;
            }

            return result;
        }
    }
}
