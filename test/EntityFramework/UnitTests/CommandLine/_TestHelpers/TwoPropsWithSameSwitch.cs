namespace CmdLine.Tests
{
    extern alias migrate;
    using migrate::CmdLine;

    public class TwoPropsWithSameSwitch
    {
        [CommandLineParameter("B")]
        public bool B1 { get; set; }

        [CommandLineParameter("B")]
        public bool B2 { get; set; }
    }
}