namespace CmdLine.Tests
{
    public class PropWithNoCommandName
    {
        [CommandLineParameter(Default=true)]
        public bool b1 { get; set; }
    }
}