// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace CmdLine.Tests
{
    extern alias migrate;
    using migrate::CmdLine;

    public class BadPositionArgNoOne
    {
        // Note: There is no position 1
        [CommandLineParameter(Name = "source", ParameterIndex = 2, Required = true, Description = "Specifies the file(s) to copy.")]
        public string Source { get; set; }
    }
}