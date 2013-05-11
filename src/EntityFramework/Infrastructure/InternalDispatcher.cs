// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Threading.Tasks;

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

        public TResult Dispatch<TInterceptionContext, TResult>(
            TResult result,
            TInterceptionContext interceptionContext,
            Action<TInterceptor> intercept)
            where TInterceptionContext : DbInterceptionContext, IDbInterceptionContextWithResult<TResult>
        {
            if (_interceptors.Count == 0)
            {
                return result;
            }

            interceptionContext.Result = result;
            _interceptors.Each(intercept);
            return interceptionContext.Result;
        }

        public TResult Dispatch<TInterceptionContext, TResult>(
            Func<TResult> operation,
            TInterceptionContext interceptionContext,
            Action<TInterceptor> executing,
            Action<TInterceptor, TInterceptionContext> executed)
            where TInterceptionContext : DbInterceptionContext, IDbInterceptionContextWithResult<TResult>
        {
            if (_interceptors.Count == 0)
            {
                return operation();
            }

            _interceptors.Each(executing);

            TResult result;
            try
            {
                result = interceptionContext.IsResultSet ? interceptionContext.Result : operation();
            }
            catch (Exception ex)
            {
                interceptionContext = (TInterceptionContext)interceptionContext.WithException(ex);
                _interceptors.Each(i => executed(i, interceptionContext));

                throw;
            }

            interceptionContext.Result = result;
            _interceptors.Each(i => executed(i, interceptionContext));
            return interceptionContext.Result;
        }

#if !NET40
        public Task<TResult> Dispatch<TInterceptionContext, TResult>(
            Func<Task<TResult>> operation,
            TInterceptionContext interceptionContext,
            Action<TInterceptor> executing,
            Action<TInterceptor, TInterceptionContext> executed,
            Func<TInterceptionContext, Task, TInterceptionContext> updateInterceptionContext)
            where TInterceptionContext : DbInterceptionContext, IDbInterceptionContextWithResult<TResult>
        {
            if (_interceptors.Count == 0)
            {
                return operation();
            }

            _interceptors.Each(executing);

            var task = interceptionContext.IsResultSet ? Task.FromResult(interceptionContext.Result) : operation();

            // This first continuation is setup to always run the "executed" interceptors even
            // if the task fails or is canceled.
            var interceptionTask = task.ContinueWith(
                t =>
                    {
                        var result = t.IsCanceled || t.IsFaulted ? default(TResult) : t.Result;
                        
                        var contextToPropagate = updateInterceptionContext(interceptionContext, t);
                        if (t.IsFaulted)
                        {
                            contextToPropagate = (TInterceptionContext)contextToPropagate.WithException(t.Exception.InnerException);
                        }

                        contextToPropagate.Result = result;
                        _interceptors.Each(i => executed(i, contextToPropagate));
                        return contextToPropagate.Result;
                    }, TaskContinuationOptions.ExecuteSynchronously);

            // The second continuation is setup to transfer state from the original task into the
            // continuation task so that it will be visible to consumers of the task.
            var tcs = new TaskCompletionSource<TResult>();
            interceptionTask.ContinueWith(
                t =>
                    {
                        if (task.IsFaulted)
                        {
                            tcs.SetException(task.Exception.InnerException);
                        }
                        else if (task.IsCanceled)
                        {
                            tcs.SetCanceled();
                        }
                        else
                        {
                            tcs.SetResult(t.Result);
                        }
                    }, TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }
#endif
    }
}
