// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    internal class InterceptionContextMutableData<TResult> : InterceptionContextMutableData
    {
        private TResult _result;

        public TResult OriginalResult { get; set; }

        public TResult Result
        {
            get { return _result; }
            set
            {
                if (!HasExecuted)
                {
                    SuppressExecution();
                }
                _result = value;
            }
        }

        public void SetExecuted(TResult result)
        {
            HasExecuted = true;

            OriginalResult = result;
            Result = result;
        }
    }
}
