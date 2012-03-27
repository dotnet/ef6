namespace CmdLine.Tests
{
    using CmdLine;

    [CommandLineArguments(Title = TestArgsTitle, Description = TestArgsDescription)]
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

        [CommandLineParameter(Command = "Y", Name="The Y Value", Description = YArgDescription)]

        public bool BoolY { get; set; }

        [CommandLineParameter(Command = "T", Default = true, Description = TArgDescription)]
        public bool BoolT { get; set; }

        [CommandLineParameter(Command = "S", Description = SArgDescription, Default = StringArgDefault)]
        public string StringArg { get; set; }

        [CommandLineParameter(Command = "N", Description = "An Int32 Number", Required = true, ValueExample = "13")]
        public int Number { get; set; }

    }
}