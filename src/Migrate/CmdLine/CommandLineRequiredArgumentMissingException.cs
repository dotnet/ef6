// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace CmdLine
{
    using System;
    using System.Data.Entity.Migrations.Console.Resources;

    [Serializable]
    public class CommandLineRequiredArgumentMissingException : CommandLineException
    {
        public CommandLineRequiredArgumentMissingException(Type argumentType, string argumentName, int parameterIndex)
            : base(new CommandArgumentHelp(argumentType, FormatMessage(argumentName, parameterIndex)))
        {
        }

        private static string FormatMessage(string argumentName, int parameterIndex)
        {
            return parameterIndex == -1
                       ? Strings.MissingCommandLineParameter("", argumentName, CommandLine.Text)
                       : Strings.MissingCommandLineParameter(parameterIndex, argumentName, string.Join(" ", CommandLine.Args));
        }
    }
}
