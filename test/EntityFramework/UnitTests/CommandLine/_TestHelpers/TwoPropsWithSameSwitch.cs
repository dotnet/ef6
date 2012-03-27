namespace CmdLine.Tests
{
    public class TwoPropsWithSameSwitch
    {
        [CommandLineParameter("B")]
        public bool B1 { get; set; }

        [CommandLineParameter("B")]
        public bool B2 { get; set; }
    }
}