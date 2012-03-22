namespace CmdLine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    [Serializable]
    public class CommandArgumentHelp
    {
        #region Constants and Fields

        private readonly List<CommandLineParameterAttribute> validArguments = new List<CommandLineParameterAttribute>();

        #endregion

        #region Constructors and Destructors

        public CommandArgumentHelp(PropertyInfo property, string message)
            : this(property.DeclaringType, message)
        {
        }

        public CommandArgumentHelp(Type argumentClassType, string message)
        {
            Message = message;
            var cmdLineAttribute = CommandLineArgumentsAttribute.Get(argumentClassType);

            if (cmdLineAttribute != null)
            {
                Title = cmdLineAttribute.Title;
                Description = cmdLineAttribute.Description;
                Program = cmdLineAttribute.Program;
            }

            foreach (var parameterAttribute in CommandLineParameterAttribute.GetAllPropertyParameters(argumentClassType)
                )
            {
                validArguments.Add(parameterAttribute);
            }
        }

        public CommandArgumentHelp(Type argumentClassType)
            : this(argumentClassType, string.Empty)
        {
        }

        #endregion

        #region Properties

        public string CommandLine
        {
            get { return CmdLine.CommandLine.Text; }
        }

        public string Description { get; set; }

        public CommandArgument InvalidArgument { get; set; }

        public int InvalidPosition { get; set; }

        public string Message { get; set; }

        public string Program { get; set; }

        public string Title { get; set; }

        public IEnumerable<CommandLineParameterAttribute> ValidArguments
        {
            get { return validArguments; }
        }

        #endregion

        #region Public Methods

        public string GetHelpText(int maxWidth, int margin = 5)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Title);
            sb.AppendLine(Description);
            sb.AppendLine();
            var maxParameterWidth = AppendCommandLineExample(sb, maxWidth, margin);

            AppendParameters(sb, maxWidth, maxParameterWidth, margin);
            return sb.ToString();
        }

        #endregion

        #region Methods

        private static void AppendLines(
            StringBuilder sb, int maxParameterWidth, IList<string> firstColum, IList<string> secondColumn)
        {
            var format = string.Format("{{0,-{0}}}{{1}}", maxParameterWidth + 1);
            for (var i = 0; i < firstColum.Count || i < secondColumn.Count; i++)
            {
                sb.AppendLine(
                    string.Format(
                        format, i < firstColum.Count ? firstColum[i] : string.Empty,
                        i < secondColumn.Count ? secondColumn[i] : string.Empty));
            }
        }

        private static string CommandName(CommandLineParameterAttribute parameter)
        {
            return parameter.Command;
        }

        private static string FormatCommandArgument(CommandLineParameterAttribute parameter)
        {
            return string.Format(
                parameter.Required ? "{0}{1} " : "[{0}{1}] ", CmdLine.CommandLine.CommandSeparators.First(),
                parameter.Command);
        }

        private static string FormatCommandLineParameter(CommandLineParameterAttribute parameter)
        {
            return parameter.IsCommand() ? FormatCommandArgument(parameter) : FormatParameterArgument(parameter);
        }

        private static string FormatParameterArgument(CommandLineParameterAttribute parameter)
        {
            return string.Format(parameter.Required ? "{0} " : "[{0}] ", parameter.Name);
        }

        private static bool IsCommand(CommandLineParameterAttribute parameter)
        {
            return parameter.IsCommand();
        }

        private static bool IsParameter(CommandLineParameterAttribute parameter)
        {
            return parameter.IsParameter();
        }

        private static int ParameterIndex(CommandLineParameterAttribute parameter)
        {
            return parameter.ParameterIndex;
        }

        private static IList<string> WrapText(string text, int width)
        {
            // (.{1,<width>})(\s+|$\n?)
            var format = string.Format(@"(.{{1,{0}}})(\s+|$\n?)", width);
            var matches = Regex.Matches(text, format);
            var result = new List<string>(matches.Count);
            result.AddRange(
                from object match in matches
                select match.ToString());
            return result;
        }

        private int AppendCommandLineExample(StringBuilder sb, int maxWidth, int margin)
        {
            var paramSb = new StringBuilder();

            var maxParameterWidth = 0;
            foreach (var parameterName in ValidArguments.OrderBy(IsCommand).Select(FormatCommandLineParameter))
            {
                paramSb.AppendFormat("{0} ", parameterName);
                if (parameterName.Length > maxParameterWidth)
                {
                    maxParameterWidth = parameterName.Length;
                }
            }
            var parms = paramSb.ToString();
            var width = Program.Length;
            var nameLines = WrapText(Program, width);
            var descriptionLines = WrapText(parms, maxWidth - width - margin);
            AppendLines(sb, width, nameLines, descriptionLines);

            sb.AppendLine();
            return maxParameterWidth;
        }

        private void AppendParameter(
            StringBuilder sb, CommandLineParameterAttribute parameter, int maxWidth, int maxParameterWidth, int margin)
        {
            var nameLines = WrapText(FormatCommandLineParameter(parameter), maxParameterWidth + margin);
            var descriptionLines = WrapText(parameter.Description, maxWidth - maxParameterWidth - margin);

            AppendLines(sb, maxParameterWidth, nameLines, descriptionLines);
        }

        private void AppendParameters(StringBuilder sb, int maxWidth, int maxParameterWidth, int margin)
        {
            foreach (var parameter in ValidArguments.Where(IsParameter).OrderBy(ParameterIndex))
            {
                AppendParameter(sb, parameter, maxWidth, maxParameterWidth, margin);
            }
            foreach (var parameter in ValidArguments.Where(IsCommand).OrderBy(CommandName))
            {
                AppendParameter(sb, parameter, maxWidth, maxParameterWidth, margin);
            }
        }

        #endregion
    }
}
