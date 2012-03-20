namespace CmdLine
{
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    ///   Represents a command line argument
    /// </summary>
    public class CommandArgument
    {
        private const int TokenGroup = 0;

        private int parameterIndex = -1;

        private string value;

        /// <summary>
        ///   Initializes a CommandArgument from a Regex match
        /// </summary>
        /// <param name = "match"></param>
        public CommandArgument(Match match)
        {
            this.Token = GetGroupValue(match, TokenGroup);
            this.SwitchSeparator = GetGroupValue(match, CommandLine.SwitchSeperatorGroup);
            this.Command = GetGroupValue(match, CommandLine.SwitchNameGroup);
            this.SwitchOption = GetGroupValue(match, CommandLine.SwitchOptionGroup);
            this.Value = GetGroupValue(match, CommandLine.ValueGroup);
        }

        public CommandArgument(string token, int parameterIndex)
        {
            this.Token = token;
            this.ParameterIndex = parameterIndex;
        }

        public string SwitchOption { get; set; }

        public string Token { get; set; }

        public string SwitchSeparator { get; set; }

        public string Command { get; set; }

        public string Value
        {
            get
            {
                return string.IsNullOrWhiteSpace(this.value) && this.ParameterIndex != -1
                           ? this.Token
                           : this.value;
            }
            set
            {
                this.value = value;
            }
        }

        public int ParameterIndex
        {
            get
            {
                return this.parameterIndex;
            }
            set
            {
                this.parameterIndex = value;
            }
        }

        /// <summary>
        ///   Returns the value used by the property cache for the key
        /// </summary>
        /// <remarks>
        ///   If the Command property has a value use that, otherwise use the formatted position value
        /// </remarks>
        internal string Key
        {
            get
            {
                return this.IsParameter()
                           ? CommandLineParameterAttribute.GetParameterKey(this.ParameterIndex)
                           : CommandLine.CaseSensitive ? this.Command : this.Command.ToLowerInvariant();
            }
        }

        public bool IsCommand()
        {
            return !string.IsNullOrWhiteSpace(this.Command);
        }

        public bool IsParameter()
        {
            return !this.IsCommand();
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

            this.AppendProperty(stringBuilder, "Token");
            this.AppendProperty(stringBuilder, "Command");
            this.AppendProperty(stringBuilder, "SwitchSeparator");
            this.AppendProperty(stringBuilder, "SwitchOption");
            this.AppendProperty(stringBuilder, "Value");
            this.AppendProperty(stringBuilder, "ParameterIndex");

            return stringBuilder.ToString();
        }

        private void AppendProperty(StringBuilder stringBuilder, string propertyName)
        {
            var property = this.GetType().GetProperty(propertyName);
            var propValue = property.GetValue(this, null);

            stringBuilder.AppendFormat(" {0}: \"{1}\"", propertyName, propValue);
        }
    }
}