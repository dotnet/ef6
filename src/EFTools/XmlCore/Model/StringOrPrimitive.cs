// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;

    /// <summary>
    ///     Class that allows string values or primitive values.  Convert to and from these using StringOrPrimitiveConverter class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    internal class StringOrPrimitive<T>
    {
        private readonly T _primitiveValue;
        private readonly string _stringValue;

        internal StringOrPrimitive(T primitiveValue)
        {
            _primitiveValue = primitiveValue;
        }

        internal StringOrPrimitive(string stringVal)
        {
            _stringValue = stringVal;
        }

        internal string StringValue
        {
            get { return _stringValue; }
        }

        internal T PrimitiveValue
        {
            get { return _primitiveValue; }
        }

        public override bool Equals(object obj)
        {
            var other = obj as StringOrPrimitive<T>;
            if (null == other)
            {
                return false;
            }

            var thisStringValue = StringValue;
            var otherStringValue = other.StringValue;
            if (null != thisStringValue)
            {
                if (null == otherStringValue)
                {
                    // this is a StringValue, other is a PrimitiveValue
                    return false;
                }
                else
                {
                    // both StringValues
                    return thisStringValue.Equals(otherStringValue, StringComparison.Ordinal);
                }
            }

            if (null != otherStringValue)
            {
                // this is a PrimitiveValue, other is a StringValue
                return false;
            }

            // both PrimitiveValues
            var thisPrimitiveValue = PrimitiveValue;
            var otherPrimitiveValue = other.PrimitiveValue;
            return (thisPrimitiveValue.Equals(otherPrimitiveValue));
        }

        public override int GetHashCode()
        {
            var thisStringValue = StringValue;
            if (null != thisStringValue)
            {
                return thisStringValue.GetHashCode();
            }

            return PrimitiveValue.GetHashCode();
        }

        public override string ToString()
        {
            if (_stringValue != null)
            {
                return _stringValue;
            }
            else
            {
                return _primitiveValue.ToString();
            }
        }
    }
}
