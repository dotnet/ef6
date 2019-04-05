// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET452

namespace CmdLine.Tests
{
    extern alias migrate;

    public class TwoPropsWithSameSwitch
    {
        [migrate::CmdLine.CommandLineParameterAttribute("B")]
        public bool B1 { get; set; }

        [migrate::CmdLine.CommandLineParameterAttribute("B")]
        public bool B2 { get; set; }
    }
}

#endif
