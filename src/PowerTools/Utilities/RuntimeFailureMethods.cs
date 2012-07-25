// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Utilities
{
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using Microsoft.DbContextPackage.Resources;

    /// <summary>
    /// Code Contracts hook methods - Called when contracts fail. Here we detect the
    /// most common preconditions so we can throw the correct exceptions. It also
    /// means that we can write preconditions using the simplest Contract.Requires()
    /// form.
    /// </summary>
    internal static class RuntimeFailureMethods
    {
        public static readonly Regex IsNotNull = new Regex(
            @"^\s*(@?\w+)\s*\!\=\s*null\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex IsNullOrWhiteSpace = new Regex(
            @"^\s*\!\s*string\s*\.\s*IsNullOrWhiteSpace\s*\(\s*(@?[\w]+)\s*\)\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        [DebuggerStepThrough]
        public static void Requires(bool condition, string userMessage, string conditionText)
        {
            if (!condition)
            {
                Match match;

                if (((match = IsNotNull.Match(conditionText)) != null) && match.Success)
                {
                    throw Error.ArgumentNull(match.Groups[1].Value);
                }

                if (((match = IsNullOrWhiteSpace.Match(conditionText)) != null) && match.Success)
                {
                    throw Error.ArgumentIsNullOrWhitespace(match.Groups[1].Value);
                }

                throw Error.PreconditionFailed(conditionText, userMessage);
            }
        }
    }
}