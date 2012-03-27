namespace CmdLine.Tests
{
    public class ThreeRequiredPositionArgs
    {
        [CommandLineParameter(Name="String 1", ParameterIndex = 1, Required = true)]
        public string S1 { get; set; }

        [CommandLineParameter(Name = "String 2", ParameterIndex = 2, Required = true)]
        public string S2 { get; set; }

        [CommandLineParameter(Name = "String 3", ParameterIndex = 3, Required = true)]
        public string S3 { get; set; }
    }
}