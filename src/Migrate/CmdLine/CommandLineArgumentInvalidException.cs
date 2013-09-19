// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace CmdLine
{
    using System;
    using System.Data.Entity.Migrations.Console.Resources;

    [Serializable]
    internal class CommandLineArgumentInvalidException : CommandLineException
    {
        public CommandLineArgumentInvalidException(Type argumentType, CommandArgument argument)
            : base(GetInvalidArgumentString(argumentType, argument))
        {
        }

        private static CommandArgumentHelp GetInvalidArgumentString(Type argumentType, CommandArgument argument)
        {
            var cmdLine = string.Join(" ", CommandLine.Args);
            var message = argument.IsParameter()
                              ? Strings.InvalidCommandLineArgument(argument.Token, argument.ParameterIndex, cmdLine)
                              : Strings.InvalidCommandLineCommand(
                                  argument.Token, cmdLine.IndexOf(argument.Token, StringComparison.InvariantCulture), cmdLine);

            return new CommandArgumentHelp(argumentType, message);
        }
    }
}
