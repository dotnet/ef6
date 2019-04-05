// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET452

namespace CmdLine
{
    extern alias migrate;
    using System.Data.Entity;
    using Xunit;

    public class CommandLineArgumentInvalidExceptionTests
    {
        [Fact] // CodePlex 1107
        public void Deserialized_exception_can_be_serialized_and_deserialized_again()
        {
            var commandArg = new migrate::CmdLine.CommandArgument("/N:345", 7);
            commandArg.Command = "N";

            var ex = new migrate::CmdLine.CommandLineArgumentInvalidException(
                typeof(CommandLineExceptionTests.SomeCommandLineClass), commandArg);
            
            Assert.Contains("/N:345", ex.Message);

            Assert.Contains(
                "/N:345", 
                ExceptionHelpers.SerializeAndDeserialize(ExceptionHelpers.SerializeAndDeserialize(ex)).Message);
        }
    }
}

#endif
