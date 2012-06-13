namespace System.Data.Entity.SqlServer.Utilities
{
    using System.Data.Entity.SqlServer.Resources;
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    /// <summary>
    ///     Code Contracts hook methods - Called when contracts fail. Here we detect the most common preconditions
    ///     so we can throw the correct exceptions. It also means that we can write preconditions using the
    ///     simplest Contract.Requires() form.
    /// </summary>
    internal static class RuntimeFailureMethods
    {
        private static readonly Regex _isNotNull = new Regex(
            @"^\s*(@?\w+)\s*\!\=\s*null\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex _isNullOrWhiteSpace = new Regex(
            @"^\s*\!\s*string\s*\.\s*IsNullOrWhiteSpace\s*\(\s*(@?[\w]+)\s*\)\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        [DebuggerStepThrough]
        public static void Requires(bool condition, string userMessage, string conditionText)
        {
            if (!condition)
            {
                Match match;

                if (((match = _isNotNull.Match(conditionText)) != null)
                    && match.Success)
                {
                    throw Error.ArgumentNull(match.Groups[1].Value);
                }

                if (((match = _isNullOrWhiteSpace.Match(conditionText)) != null)
                    && match.Success)
                {
                    throw Error.ArgumentIsNullOrWhitespace(match.Groups[1].Value);
                }

                throw Error.PreconditionFailed(conditionText, userMessage);
            }
        }
    }
}
