// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
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
            where TInterceptionContext : DbInterceptionContext, IDbMutableInterceptionContext<TResult>
        {
            if (_interceptors.Count == 0)
            {
                return result;
            }

            interceptionContext.MutableData.SetExecuted(result);

            _interceptors.Each(intercept);

            if (interceptionContext.MutableData.Exception != null)
            {
                throw interceptionContext.MutableData.Exception;
            }

            return interceptionContext.MutableData.Result;
        }

        public TResult Dispatch<TInterceptionContext, TResult>(
            Func<TResult> operation,
            TInterceptionContext interceptionContext,
            Action<TInterceptor> executing,
            Action<TInterceptor> executed)
            where TInterceptionContext : DbInterceptionContext, IDbMutableInterceptionContext<TResult>
        {
            if (_interceptors.Count == 0)
            {
                return operation();
            }

            _interceptors.Each(executing);

            if (!interceptionContext.MutableData.IsExecutionSuppressed)
            {
                try
                {
                    interceptionContext.MutableData.SetExecuted(operation());
                }
                catch (Exception ex)
                {
                    interceptionContext.MutableData.SetExceptionThrown(ex);

                    _interceptors.Each(executed);

                    if (ReferenceEquals(interceptionContext.MutableData.Exception, ex))
                    {
                        throw;
                    }
                }
            }

            if (interceptionContext.MutableData.OriginalException == null)
            {
                _interceptors.Each(executed);
            }

            if (interceptionContext.MutableData.Exception != null)
            {
                throw interceptionContext.MutableData.Exception;
            }

            return interceptionContext.MutableData.Result;
        }

#if !NET40
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public Task<TResult> DispatchAsync<TInterceptionContext, TResult>(
            Func<Task<TResult>> operation,
            TInterceptionContext interceptionContext,
            Action<TInterceptor> executing,
            Action<TInterceptor> executed)
            where TInterceptionContext : DbInterceptionContext, IDbMutableInterceptionContext<TResult>
        {
            if (_interceptors.Count == 0)
            {
                return operation();
            }

            _interceptors.Each(executing);

            var task = interceptionContext.MutableData.IsExecutionSuppressed 
                ? Task.FromResult(interceptionContext.MutableData.Result) 
                : operation();

            var tcs = new TaskCompletionSource<TResult>();
            task.ContinueWith(
                t =>
                    {
                        interceptionContext.MutableData.TaskStatus = t.Status;

                        if (t.IsFaulted)
                        {
                            interceptionContext.MutableData.SetExceptionThrown(t.Exception.InnerException);
                        }
                        else if (!interceptionContext.MutableData.IsExecutionSuppressed)
                        {
                            interceptionContext.MutableData.SetExecuted(t.IsCanceled || t.IsFaulted ? default(TResult) : t.Result);
                        }

                        try
                        {
                            _interceptors.Each(executed);
                        }
                        catch (Exception ex)
                        {
                            interceptionContext.MutableData.Exception = ex;
                        }

                        if (interceptionContext.MutableData.Exception != null)
                        {
                            tcs.SetException(interceptionContext.MutableData.Exception);
                        }
                        else if (t.IsCanceled)
                        {
                            tcs.SetCanceled();
                        }
                        else
                        {
                            tcs.SetResult(interceptionContext.MutableData.Result);
                        }
                    }, TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }
#endif
    }
}
