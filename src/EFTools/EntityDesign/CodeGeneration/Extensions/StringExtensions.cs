// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    internal static class StringExtensions
    {
        public static bool EqualsIgnoreCase(this string a, string b)
        {
            Debug.Assert(!string.IsNullOrEmpty(a), "a is null or empty.");
            Debug.Assert(!string.IsNullOrEmpty(b), "b is null or empty.");

            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        public static bool ContainsIgnoreCase(this IEnumerable<string> source, string value)
        {
            Debug.Assert(source != null, "source is null.");
            Debug.Assert(!string.IsNullOrEmpty(value), "value is null or empty.");

            return source.Contains(value, StringComparer.OrdinalIgnoreCase);
        }
    }
}
