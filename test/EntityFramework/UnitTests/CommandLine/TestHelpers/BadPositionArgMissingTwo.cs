// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace CmdLine.Tests
{
    extern alias migrate;

    public class BadPositionArgMissingTwo
    {
        [migrate::CmdLine.CommandLineParameterAttribute(Name = "source", ParameterIndex = 1, Required = true,
            Description = "Specifies the file(s) to copy.")]
        public string Source { get; set; }

        // Note: There is no position 2

        [migrate::CmdLine.CommandLineParameterAttribute(Name = "destination", ParameterIndex = 3,
            Description = "Specifies the file(s) to copy.")]
        public string Destination { get; set; }
    }
}
