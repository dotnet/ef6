// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Tools
{
    internal static class AnsiConsole
    {
        public static readonly AnsiTextWriter _out = new AnsiTextWriter(Console.Out);

        public static void WriteLine(string text)
            => _out.WriteLine(text);
    }
}
