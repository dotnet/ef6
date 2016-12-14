// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Threading.Tasks;

    internal class InterceptionContextMutableData
    {
        private const string LegacyUserState = "__LegacyUserState__";

        private Exception _exception;
        private bool _isSuppressed;
        private IDictionary<string, object> _userStateMap;

        public bool HasExecuted { get; set; }
        public Exception OriginalException { get; set; }
        public TaskStatus TaskStatus { get; set; }

        private IDictionary<string, object> UserStateMap
        {
            get
            {
                if (_userStateMap == null)
                {
                    _userStateMap = new Dictionary<string, object>(StringComparer.Ordinal);
                }

                return _userStateMap;
            }
        }

        [Obsolete("Not safe when multiple interceptors are in use. Use SetUserState and FindUserState instead.")]
        public object UserState
        {
            get { return FindUserState(LegacyUserState); }
            set { SetUserState(LegacyUserState, value); }
        }

        public object FindUserState(string key)
        {
            object value;
            return _userStateMap != null && UserStateMap.TryGetValue(key, out value) ? value : null;
        }

        public void SetUserState(string key, object value)
        {
            UserStateMap[key] = value;
        }

        public bool IsExecutionSuppressed
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
