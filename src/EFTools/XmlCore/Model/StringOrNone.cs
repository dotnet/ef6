// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;

    /// <summary>
    ///     Class that represents a choice between a value indicating "not set" which will generally be
    ///     referred to as '(None)' and any string, including the string "(None)" which will be treated separately
    /// </summary>
    [Serializable]
    internal class StringOrNone
    {
        // special value passed when converting the NoneValue to a string
        // (should never be displayed so do not need to localize)
        internal static readonly string NoneUnderlyingValue = "__STRING_OR_NONE_NONE_VALUE__";

        private readonly string _value;
        private readonly bool _isNoneValue;

        internal static readonly StringOrNone NoneValue = new StringOrNone();

        private StringOrNone()
        {
            // represents the '(NoneValue)' StringOrNone
            _value = NoneUnderlyingValue;
            _isNoneValue = true;
        }

        internal StringOrNone(string stringVal)
        {
            _value = stringVal;
            _isNoneValue = false;
        }

        internal bool IsNoneValue
        {
            get { return _isNoneValue; }
        }

        public override string ToString()
        {
            return _value;
        }

        public override bool Equals(object obj)
        {
            var other = obj as StringOrNone;
            if (null == other)
            {
                return false;
            }

            if (IsNoneValue && other.IsNoneValue)
            {
                return true;
            }
            else if (!IsNoneValue
                     && !other.IsNoneValue)
            {
                return ToString().Equals(other.ToString(), StringComparison.Ordinal);
            }
            else
            {
                // this is NoneValue and other is not, or other is NoneValue and this is not
                return false;
            }
        }

        public override int GetHashCode()
        {
            if (IsNoneValue)
            {
                return -1;
            }

            return ToString().GetHashCode();
        }
    }
}
