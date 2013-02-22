// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace CmdLine.Tests
{
    extern alias migrate;

    [migrate::CmdLine.CommandLineArgumentsAttribute(Title = TestArgsTitle, Description = TestArgsDescription)]
    public class TestArgs
    {
        private const string SArgDescription = "This is a string argument";

        public const string TestArgsTitle = "My Test Program";

        public const string TestArgsDescription = "Verifies that the command parsing works";

        public const string YArgDescription = "The Y Arg is optional";

        public const string TArgDescription = "The T value is required";

        public const string StringArgDefault = "Default S Value";

        public const bool BoolYDefault = false;

        public const bool BoolTDefault = true;

        [migrate::CmdLine.CommandLineParameterAttribute(Command = "Y", Name = "The Y Value", Description = YArgDescription)]
        public bool BoolY { get; set; }

        [migrate::CmdLine.CommandLineParameterAttribute(Command = "T", Default = true, Description = TArgDescription)]
        public bool BoolT { get; set; }

        [migrate::CmdLine.CommandLineParameterAttribute(Command = "S", Description = SArgDescription, Default = StringArgDefault)]
        public string StringArg { get; set; }

        [migrate::CmdLine.CommandLineParameterAttribute(Command = "N", Description = "An Int32 Number", Required = true, ValueExample = "13"
            )]
        public int Number { get; set; }
    }
}
