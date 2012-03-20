namespace CmdLine
{
    using System;

    public class CommandLineArgumentInvalidException : CommandLineException
    {
        public CommandLineArgumentInvalidException(Type argumentType, CommandArgument argument)
            : base(GetInvalidArgumentString(argumentType, argument))
        {
        }

        private static CommandArgumentHelp GetInvalidArgumentString(Type argumentType, CommandArgument argument)
        {
            var cmdLine = string.Join(" ", CommandLine.Args);
            var message = argument.IsParameter()
                              ? string.Format("Invalid argument \"{0}\" at parameter index {1} in command line \"{2}\"", argument.Token, argument.ParameterIndex, cmdLine)
                              : string.Format("Invalid command \"{0}\" at index {1} in command line \"{2}\"", argument.Token, cmdLine.IndexOf(argument.Token), cmdLine);

            return new CommandArgumentHelp(argumentType, message);
        }
    }
}