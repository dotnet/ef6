// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.DotNet.Cli.CommandLine;

using MyResources = System.Data.Entity.Tools.Properties.Resources;

namespace System.Data.Entity.Tools
{
    internal static class Json
    {
        public static CommandOption ConfigureOption(CommandLineApplication command)
            => command.Option("--json", MyResources.JsonDescription);

        public static string Literal(string text)
            => text != null
                ? "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\""
                : "null";
    }
}
