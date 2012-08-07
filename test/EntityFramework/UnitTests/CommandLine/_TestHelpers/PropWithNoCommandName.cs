// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace CmdLine.Tests
{
    extern alias migrate;
    using migrate::CmdLine;

    public class PropWithNoCommandName
    {
        [CommandLineParameter(Default = true)]
        public bool b1 { get; set; }
    }
}
