namespace CmdLine.Tests
{
    extern alias migrate;
    using migrate::CmdLine;

    public class PropWithNoCommandName
    {
        [CommandLineParameter(Default=true)]
        public bool b1 { get; set; }
    }
}