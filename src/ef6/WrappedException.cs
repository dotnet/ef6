// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Tools
{
    internal class WrappedException : Exception
    {
        private readonly string _stackTrace;

        public WrappedException(string type, string message, string stackTrace)
            : base(message)
        {
            Type = type;
            _stackTrace = stackTrace;
        }

        public string Type { get; }

        public override string ToString()
            => _stackTrace;
    }
}
