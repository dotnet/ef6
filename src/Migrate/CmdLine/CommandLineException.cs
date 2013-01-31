// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace CmdLine
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors",
        Justification = "SerializeObjectState used instead")]
    [Serializable]
    public class CommandLineException : Exception
    {
        [NonSerialized]
        private CommandLineExceptionState _state;

        public CommandLineException(string message)
            : base(message)
        {
            SubscribeToSerializeObjectState();
        }

        public CommandLineException(CommandArgumentHelp argumentHelp)
            : base(CheckNotNull(argumentHelp).Message)
        {
            ArgumentHelp = argumentHelp;

            SubscribeToSerializeObjectState();
        }

        public CommandLineException(CommandArgumentHelp argumentHelp, Exception inner)
            : base(CheckNotNull(argumentHelp).Message, inner)
        {
            ArgumentHelp = argumentHelp;

            SubscribeToSerializeObjectState();
        }

        public CommandArgumentHelp ArgumentHelp
        {
            get { return _state.ArgumentHelp; }
            set { _state.ArgumentHelp = value; }
        }

        private static CommandArgumentHelp CheckNotNull(CommandArgumentHelp argumentHelp)
        {
            if (argumentHelp == null)
            {
                throw new ArgumentNullException("argumentHelp");
            }
            return argumentHelp;
        }

        private void SubscribeToSerializeObjectState()
        {
            SerializeObjectState += (_, a) => a.AddSerializedState(_state);
        }

        [Serializable]
        private struct CommandLineExceptionState : ISafeSerializationData
        {
            public CommandArgumentHelp ArgumentHelp { get; set; }

            public void CompleteDeserialization(object deserialized)
            {
                ((CommandLineException)deserialized)._state = this;
            }
        }
    }
}
