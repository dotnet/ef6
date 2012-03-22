namespace CmdLine
{
    using System;

    public class CommandLineRequiredArgumentMissingException : CommandLineException
    {
        public CommandLineRequiredArgumentMissingException(Type argumentType, string argumentName, int parameterIndex)
            : base(new CommandArgumentHelp(argumentType, FormatMessage(argumentName, parameterIndex)))
        {
        }

        private static string FormatMessage(string argumentName, int parameterIndex)
        {
            return parameterIndex == -1
                       ? FormatMessage(argumentName)
                       : string.Format(
                           "Missing required parameter {0} \"{1}\" in command line \"{2}\"", parameterIndex,
                           argumentName, string.Join(" ", CommandLine.Args));
        }

        private static string FormatMessage(string argumentName)
        {
            return string.Format(
                "Missing required parameter \"{0}\" in command line \"{1}\"", argumentName, CommandLine.Text);
        }
    }
}
