// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
            ArgumentHelp = argumentHelp;
        }

        public CommandLineException(CommandArgumentHelp argumentHelp, Exception inner)
            : base(argumentHelp.Message, inner)
        {
            ArgumentHelp = argumentHelp;
        }

        protected CommandLineException(SerializationInfo info, StreamingContext context)
        {
            ArgumentHelp = (CommandArgumentHelp)info.GetValue("ArgumentHelp", typeof(CommandArgumentHelp));
        }

        public CommandArgumentHelp ArgumentHelp { get; set; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ArgumentHelp", ArgumentHelp);
        }
    }
}
