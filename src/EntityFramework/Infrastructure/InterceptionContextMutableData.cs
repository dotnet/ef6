// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Resources;
    using System.Threading.Tasks;

    internal class InterceptionContextMutableData
    {
        private Exception _exception;
        private bool _isSuppressed;

        public bool HasExecuted { get; set; }
        public Exception OriginalException { get; set; }
        public TaskStatus TaskStatus { get; set; }

        public bool IsSuppressed
        {
            get { return _isSuppressed; }
        }

        public void SuppressExecution()
        {
            if (!_isSuppressed && HasExecuted)
            {
                throw new InvalidOperationException(Strings.SuppressionAfterExecution);
            }
            _isSuppressed = true;
        }

        public Exception Exception
        {
            get { return _exception; }
            set
            {
                if (!HasExecuted)
                {
                    SuppressExecution();
                }
                _exception = value;
            }
        }

        public void SetExceptionThrown(Exception exception)
        {
            HasExecuted = true;

            OriginalException = exception;
            Exception = exception;
        }
    }
}
