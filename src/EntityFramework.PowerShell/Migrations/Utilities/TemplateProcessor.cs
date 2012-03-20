namespace System.Data.Entity.Migrations.Utilities
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Helper class to replace tokens in template files
    /// </summary>
    internal class TemplateProcessor
    {
        private static readonly Regex _tokenRegex = new Regex(@"\$(?<tokenName>\w+)\$");

        /// <summary>
        /// Calculate the result of applying tokens to a template
        /// </summary>
        /// <param name="input">Template to be processed</param>
        /// <param name="tokens">Values to be used for tokens</param>
        /// <returns>Template with tokens replaced</returns>
        public string Process(string input, IDictionary<string, string> tokens)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(input));
            Contract.Requires(tokens != null);

            return _tokenRegex.Replace(
                input,
                match =>
                {
                    var tokenName = match.Groups["tokenName"].Value;
                    string value = string.Empty;

                    tokens.TryGetValue(tokenName, out value);

                    return value;
                });
        }
    }
}