namespace CmdLine
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    public class CommandLineException : Exception
    {
        public CommandLineException(string message)
            : base(message)
        {
        }

        public CommandLineException(CommandArgumentHelp argumentHelp)
            : base(argumentHelp.Message)
        {
            this.ArgumentHelp = argumentHelp;
        }

        public CommandLineException(CommandArgumentHelp argumentHelp, Exception inner)
            : base(argumentHelp.Message, inner)
        {
            this.ArgumentHelp = argumentHelp;
        }

        protected CommandLineException(SerializationInfo info, StreamingContext context)
        {
            this.ArgumentHelp = (CommandArgumentHelp)info.GetValue("ArgumentHelp", typeof(CommandArgumentHelp));
        }

        public CommandArgumentHelp ArgumentHelp { get; set; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ArgumentHelp", this.ArgumentHelp);
        }
    }
}