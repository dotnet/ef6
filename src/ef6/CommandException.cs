// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Tools
{
    internal class CommandException : Exception
    {
        public CommandException(string message)
            : base(message)
        {
        }

        public CommandException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
