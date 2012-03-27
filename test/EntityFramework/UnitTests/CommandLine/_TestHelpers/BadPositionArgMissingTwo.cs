namespace CmdLine.Tests
{
    public class BadPositionArgMissingTwo
    {
        [CommandLineParameter(Name = "source", ParameterIndex = 1, Required = true, Description = "Specifies the file(s) to copy.")]
        public string Source { get; set; }

        // Note: There is no position 2

        [CommandLineParameter(Name = "destination", ParameterIndex = 3, Description = "Specifies the file(s) to copy.")]
        public string Destination { get; set; }

    }
}