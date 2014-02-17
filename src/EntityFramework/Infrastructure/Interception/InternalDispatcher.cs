// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
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
            Action<TInterceptor, TInterceptionContext> intercept)
            where TInterceptionContext : DbInterceptionContext, IDbMutableInterceptionContext<TResult>
        {
            if (_interceptors.Count == 0)
            {
                return result;
            }

            interceptionContext.MutableData.SetExecuted(result);

            foreach (var interceptor in _interceptors)
            {
                intercept(interceptor, interceptionContext);
            }

            if (interceptionContext.MutableData.Exception != null)
            {
                throw interceptionContext.MutableData.Exception;
            }

            return interceptionContext.MutableData.Result;
        }

        public void Dispatch<TTarget, TInterceptionContext>(
            TTarget target,
            Action<TTarget, TInterceptionContext> operation,
            TInterceptionContext interceptionContext,
            Action<TInterceptor, TTarget, TInterceptionContext> executing,
            Action<TInterceptor, TTarget, TInterceptionContext> executed)
            where TInterceptionContext : DbInterceptionContext, IDbMutableInterceptionContext
        {
            if (_interceptors.Count == 0)
            {
                operation(target, interceptionContext);
                return;
            }

            foreach (var interceptor in _interceptors)
            {
                executing(interceptor, target, interceptionContext);
            }

            if (!interceptionContext.MutableData.IsExecutionSuppressed)
            {
                try
                {
                    operation(target, interceptionContext);
                    interceptionContext.MutableData.HasExecuted = true;
                }
                catch (Exception ex)
                {
                    interceptionContext.MutableData.SetExceptionThrown(ex);

                    foreach (var interceptor in _interceptors)
                    {
                        executed(interceptor, target, interceptionContext);
                    }

                    if (ReferenceEquals(interceptionContext.MutableData.Exception, ex))
                    {
                        throw;
                    }
                }
            }

            if (interceptionContext.MutableData.OriginalException == null)
            {
                foreach (var interceptor in _interceptors)
                {
                    executed(interceptor, target, interceptionContext);
                }
            }

            if (interceptionContext.MutableData.Exception != null)
            {
                throw interceptionContext.MutableData.Exception;
            }
        }

        public TResult Dispatch<TTarget, TInterceptionContext, TResult>(
            TTarget target,
            Func<TTarget, TInterceptionContext, TResult> operation,
            TInterceptionContext interceptionContext,
            Action<TInterceptor, TTarget, TInterceptionContext> executing,
            Action<TInterceptor, TTarget, TInterceptionContext> executed)
            where TInterceptionContext : DbInterceptionContext, IDbMutableInterceptionContext<TResult>
        {
            if (_interceptors.Count == 0)
            {
                return operation(target, interceptionContext);
            }

            foreach (var interceptor in _interceptors)
            {
                executing(interceptor, target, interceptionContext);
            }

            if (!interceptionContext.MutableData.IsExecutionSuppressed)
            {
                try
                {
                    interceptionContext.MutableData.SetExecuted(operation(target, interceptionContext));
                }
                catch (Exception ex)
                {
                    interceptionContext.MutableData.SetExceptionThrown(ex);

                    foreach (var interceptor in _interceptors)
                    {
                        executed(interceptor, target, interceptionContext);
                    }

                    if (ReferenceEquals(interceptionContext.MutableData.Exception, ex))
                    {
                        throw;
                    }
                }
            }

            if (interceptionContext.MutableData.OriginalException == null)
            {
                foreach (var interceptor in _interceptors)
                {
                    executed(interceptor, target, interceptionContext);
                }
            }

            if (interceptionContext.MutableData.Exception != null)
            {
                throw interceptionContext.MutableData.Exception;
            }

            return interceptionContext.MutableData.Result;
        }

#if !NET40
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public Task DispatchAsync<TTarget, TInterceptionContext>(
            TTarget target,
            Func<TTarget, TInterceptionContext, CancellationToken, Task> operation,
            TInterceptionContext interceptionContext,
            Action<TInterceptor, TTarget, TInterceptionContext> executing,
            Action<TInterceptor, TTarget, TInterceptionContext> executed,
            CancellationToken cancellationToken)
            where TInterceptionContext : DbInterceptionContext, IDbMutableInterceptionContext
        {
            if (_interceptors.Count == 0)
            {
                return operation(target, interceptionContext, cancellationToken);
            }

            foreach (var interceptor in _interceptors)
            {
                executing(interceptor, target, interceptionContext);
            }

            var task = interceptionContext.MutableData.IsExecutionSuppressed
                ? Task.FromResult((object)null)
                : operation(target, interceptionContext, cancellationToken);

            var tcs = new TaskCompletionSource<object>();
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
                        interceptionContext.MutableData.HasExecuted = true;
                    }

                    try
                    {
                        foreach (var interceptor in _interceptors)
                        {
                            executed(interceptor, target, interceptionContext);
                        }
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
                        tcs.SetResult(null);
                    }
                }, TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public Task<TResult> DispatchAsync<TTarget, TInterceptionContext, TResult>(
            TTarget target,
            Func<TTarget, TInterceptionContext, CancellationToken, Task<TResult>> operation,
            TInterceptionContext interceptionContext,
            Action<TInterceptor, TTarget, TInterceptionContext> executing,
            Action<TInterceptor, TTarget, TInterceptionContext> executed,
            CancellationToken cancellationToken)
            where TInterceptionContext : DbInterceptionContext, IDbMutableInterceptionContext<TResult>
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_interceptors.Count == 0)
            {
                return operation(target, interceptionContext, cancellationToken);
            }

            foreach (var interceptor in _interceptors)
            {
                executing(interceptor, target, interceptionContext);
            }

            var task = interceptionContext.MutableData.IsExecutionSuppressed
                ? Task.FromResult(interceptionContext.MutableData.Result)
                : operation(target, interceptionContext, cancellationToken);

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
                        foreach (var interceptor in _interceptors)
                        {
                            executed(interceptor, target, interceptionContext);
                        }
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
