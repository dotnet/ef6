// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET452

namespace System.Data.Entity.CommandLine
{
    extern alias migrate;
    using Xunit;

    public class CommandLineWriteLineColorTests
    {
        [Fact] // CodePlex 1950
        public void WriteLineColor_can_write_text_with_replacement_tokens()
        {
            var noParamsText = "";
            var paramsText = "";

            migrate::CmdLine.CommandLine.WriteLineColor(
                s => noParamsText = s,
                (s, p) => paramsText = s + p[0],
                ConsoleColor.Cyan,
                "Some Text ");

            Assert.Equal("Some Text ", noParamsText);
            Assert.Equal("", paramsText);
        }

        [Fact] // CodePlex 1950
        public void WriteLineColor_can_write_text_with_replacement_tokens_when_replacement_args_exist()
        {
            var noParamsText = "";
            var paramsText = "";

            migrate::CmdLine.CommandLine.WriteLineColor(
                s => noParamsText = s,
                (s, p) => paramsText = s + p[0],
                ConsoleColor.Cyan,
                "SomeText ",
                "Hey!");

            Assert.Equal("", noParamsText);
            Assert.Equal("SomeText Hey!", paramsText);
        }
    }
}

#endif
