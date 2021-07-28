// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Text.RegularExpressions;

    public static class StringExtensions
    {
        public static string StripFormatting(this string argument)
        {
            return Regex.Replace(argument, @"\s", string.Empty);
        }
    }
}
