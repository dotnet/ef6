// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET45

namespace CmdLine.Tests
{
    extern alias migrate;

    public class PropWithNoCommandName
    {
        [migrate::CmdLine.CommandLineParameterAttribute(Default = true)]
        public bool b1 { get; set; }
    }
}

#endif
