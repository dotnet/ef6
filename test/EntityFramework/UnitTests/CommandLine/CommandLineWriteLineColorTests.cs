// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace CmdLine
{
    extern alias migrate;
    using Xunit;

    public class CommandLineWriteLineColorTests
    {
        [Fact] // CodePlex 1950
        public void WriteLineColor_can_write_text_with_replacement_tokens()
        {
            string logText = "insert into temptable value ('{0} test')";

            Assert.DoesNotThrow(() => { migrate::CmdLine.CommandLine.WriteLineColor(System.ConsoleColor.Cyan, logText); });
        }
    }
}
