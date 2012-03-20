namespace CmdLine
{
    using System;

    public class CommandLineHelpException : CommandLineException
    {
        #region Constructors and Destructors

        public CommandLineHelpException(string message)
            : base(message)
        {
        }

        public CommandLineHelpException(CommandArgumentHelp argumentHelp)
            : base(argumentHelp.Message)
        {
            this.ArgumentHelp = argumentHelp;
        }

        public CommandLineHelpException(CommandArgumentHelp argumentHelp, Exception inner)
            : base(argumentHelp, inner)
        {
            this.ArgumentHelp = argumentHelp;
        }

        #endregion
    }
}