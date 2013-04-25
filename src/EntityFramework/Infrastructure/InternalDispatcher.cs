// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal class InternalDispatcher<TInterceptor>
        where TInterceptor : class, IDbInterceptor
    {
        private volatile List<TInterceptor> _interceptors = new List<TInterceptor>();
        private readonly object _lock = new object();

        public void Add(IDbInterceptor interceptor)
        {
            DebugCheck.NotNull(interceptor);

            var asThisType = interceptor as TInterceptor;

            if (asThisType == null)
            {
                return;
            }

            lock (_lock)
            {
                var newList = _interceptors.ToList();
                newList.Add(asThisType);
                _interceptors = newList;
            }
        }

        public void Remove(IDbInterceptor interceptor)
        {
            DebugCheck.NotNull(interceptor);

            var asThisType = interceptor as TInterceptor;

            if (asThisType == null)
            {
                return;
            }

            lock (_lock)
            {
                var newList = _interceptors.ToList();
                newList.Remove(asThisType);
                _interceptors = newList;
            }
        }

        public TResult Dispatch<TResult>(TResult result, Func<TResult, TInterceptor, TResult> accumulator)
        {
            DebugCheck.NotNull(accumulator);

            return _interceptors.Count == 0
                       ? result
                       : _interceptors.Aggregate(result, accumulator);
        }

        public void Dispatch(Action<TInterceptor> action)
        {
            DebugCheck.NotNull(action);

            if (_interceptors.Count != 0)
            {
                _interceptors.Each(action);
            }
        }

        public TResult Dispatch<TResult>(
            Func<TResult> operation,
            Action<TInterceptor> executing,
            Func<TResult, TInterceptor, TResult> executed)
        {
            if (_interceptors.Count == 0)
            {
                return operation();
            }

            _interceptors.Each(executing);

            var result = operation();

            return _interceptors.Aggregate(result, executed);
        }
    }
}
