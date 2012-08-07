// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace CmdLine
{
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    ///     Represents a command line argument
    /// </summary>
    public class CommandArgument
    {
        private const int TokenGroup = 0;

        private int parameterIndex = -1;

        private string value;

        /// <summary>
        ///     Initializes a CommandArgument from a Regex match
        /// </summary>
        /// <param name="match"> </param>
        public CommandArgument(Match match)
        {
            Token = GetGroupValue(match, TokenGroup);
            SwitchSeparator = GetGroupValue(match, CommandLine.SwitchSeparatorGroup);
            Command = GetGroupValue(match, CommandLine.SwitchNameGroup);
            SwitchOption = GetGroupValue(match, CommandLine.SwitchOptionGroup);
            Value = GetGroupValue(match, CommandLine.ValueGroup);
        }

        public CommandArgument(string token, int parameterIndex)
        {
            Token = token;
            ParameterIndex = parameterIndex;
        }

        public string SwitchOption { get; set; }

        public string Token { get; set; }

        public string SwitchSeparator { get; set; }

        public string Command { get; set; }

        public string Value
        {
            get
            {
                return string.IsNullOrWhiteSpace(value) && ParameterIndex != -1
                           ? Token
                           : value;
            }
            set { this.value = value; }
        }

        public int ParameterIndex
        {
            get { return parameterIndex; }
            set { parameterIndex = value; }
        }

        /// <summary>
        ///     Returns the value used by the property cache for the key
        /// </summary>
        /// <remarks>
        ///     If the Command property has a value use that, otherwise use the formatted position value
        /// </remarks>
        internal string Key
        {
            get
            {
                return IsParameter()
                           ? CommandLineParameterAttribute.GetParameterKey(ParameterIndex)
                           : CommandLine.CaseSensitive ? Command : Command.ToLowerInvariant();
            }
        }

        public bool IsCommand()
        {
            return !string.IsNullOrWhiteSpace(Command);
        }

        public bool IsParameter()
        {
            return !IsCommand();
        }

        private static string GetGroupValue(Match match, string group)
        {
            return match.Groups[group].Success
                       ? match.Groups[group].Value.Trim()
                       : null;
        }

        private static string GetGroupValue(Match match, int group)
        {
            return match.Groups[group].Success
                       ? match.Groups[group].Value.Trim()
                       : null;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder("CommandArgument");

            AppendProperty(stringBuilder, "Token");
            AppendProperty(stringBuilder, "Command");
            AppendProperty(stringBuilder, "SwitchSeparator");
            AppendProperty(stringBuilder, "SwitchOption");
            AppendProperty(stringBuilder, "Value");
            AppendProperty(stringBuilder, "ParameterIndex");

            return stringBuilder.ToString();
        }

        private void AppendProperty(StringBuilder stringBuilder, string propertyName)
        {
            var property = GetType().GetProperty(propertyName);
            var propValue = property.GetValue(this, null);

            stringBuilder.AppendFormat(" {0}: \"{1}\"", propertyName, propValue);
        }
    }
}
