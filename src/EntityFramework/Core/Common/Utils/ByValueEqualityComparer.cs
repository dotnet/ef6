// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils
{
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    ///     An implementation of IEqualityComparer&lt;object&gt; that compares byte[] instances by value, and
    ///     delegates all other equality comparisons to a specified IEqualityComparer. In the default case,
    ///     this provides by-value comparison for instances of the CLR equivalents of all EDM primitive types.
    /// </summary>
    internal sealed class ByValueEqualityComparer : IEqualityComparer<object>
    {
        /// <summary>
        ///     Provides by-value comparison for instances of the CLR equivalents of all EDM primitive types.
        /// </summary>
        internal static readonly ByValueEqualityComparer Default = new ByValueEqualityComparer();

        private ByValueEqualityComparer()
        {
        }

        public new bool Equals(object x, object y)
        {
            if (object.Equals(x, y))
            {
                return true;
            }

            // If x and y are both non-null byte arrays, then perform a by-value comparison
            // based on length and element values, otherwise defer to the default comparison.
            //
            var xBytes = x as byte[];
            var yBytes = y as byte[];
            if (xBytes != null
                && yBytes != null)
            {
                return CompareBinaryValues(xBytes, yBytes);
            }

            return false;
        }

        public int GetHashCode(object obj)
        {
            if (obj != null)
            {
                var bytes = obj as byte[];
                if (bytes != null)
                {
                    return ComputeBinaryHashCode(bytes);
                }
            }
            else
            {
                return 0;
            }

            return obj.GetHashCode();
        }

        internal static int ComputeBinaryHashCode(byte[] bytes)
        {
            Debug.Assert(bytes != null, "Byte array cannot be null");
            var hashCode = 0;
            for (int i = 0, n = Math.Min(bytes.Length, 7); i < n; i++)
            {
                hashCode = ((hashCode << 5) ^ bytes[i]);
            }
            return hashCode;
        }

        internal static bool CompareBinaryValues(byte[] first, byte[] second)
        {
            Debug.Assert(first != null && second != null, "Arguments cannot be null");

            if (first.Length
                != second.Length)
            {
                return false;
            }

            for (var i = 0; i < first.Length; i++)
            {
                if (first[i]
                    != second[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
