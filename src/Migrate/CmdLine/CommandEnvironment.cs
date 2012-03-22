namespace CmdLine
{
    using System;

    public class CommandEnvironment : ICommandEnvironment
    {
        public string CommandLine
        {
            get { return Environment.CommandLine; }
        }

        private string[] args;

        public string[] GetCommandLineArgs()
        {
            return args ?? (args = Environment.GetCommandLineArgs());
        }

        public string Program
        {
            get { return GetCommandLineArgs()[0]; }
        }
    }
}
