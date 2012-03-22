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
            ArgumentHelp = argumentHelp;
        }

        public CommandLineHelpException(CommandArgumentHelp argumentHelp, Exception inner)
            : base(argumentHelp, inner)
        {
            ArgumentHelp = argumentHelp;
        }

        #endregion
    }
}
