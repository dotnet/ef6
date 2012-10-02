// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace CmdLine.Tests
{
    extern alias migrate;

    public class ThreeRequiredPositionArgs
    {
        [migrate::CmdLine.CommandLineParameterAttribute(Name = "String 1", ParameterIndex = 1, Required = true)]
        public string S1 { get; set; }

        [migrate::CmdLine.CommandLineParameterAttribute(Name = "String 2", ParameterIndex = 2, Required = true)]
        public string S2 { get; set; }

        [migrate::CmdLine.CommandLineParameterAttribute(Name = "String 3", ParameterIndex = 3, Required = true)]
        public string S3 { get; set; }
    }
}
