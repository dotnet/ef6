// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40
#if SQLSERVER
namespace System.Data.Entity.SqlServer.Utilities
#else
namespace System.Data.Entity.Utilities
#endif
{
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class TaskExtensions
    {
        public static CultureAwaiter<T> WithCurrentCulture<T>(this Task<T> task)
        {
            return new CultureAwaiter<T>(task);
        }

        public static CultureAwaiter WithCurrentCulture(this Task task)
        {
            return new CultureAwaiter(task);
        }

        public struct CultureAwaiter<T> : ICriticalNotifyCompletion
        {
            private readonly Task<T> _task;

            public CultureAwaiter(Task<T> task)
            {
                _task = task;
            }

            public CultureAwaiter<T> GetAwaiter()
            {
                return this;
            }

            public bool IsCompleted
            {
                get { return _task.IsCompleted; }
            }

            public T GetResult()
            {
                return _task.GetAwaiter().GetResult();
            }

            public void OnCompleted(Action continuation)
            {
                // The compiler will never call this method
                throw new NotImplementedException();
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                var currentCulture = Thread.CurrentThread.CurrentCulture;
                _task.ConfigureAwait(false).GetAwaiter().UnsafeOnCompleted(() =>
                {
                    var originalCulture = Thread.CurrentThread.CurrentCulture;
                    Thread.CurrentThread.CurrentCulture = currentCulture;
                    try
                    {
                        continuation();
                    }
                    finally
                    {
                        Thread.CurrentThread.CurrentCulture = originalCulture;
                    }
                });
            }
        }

        public struct CultureAwaiter : ICriticalNotifyCompletion
        {
            private readonly Task _task;

            public CultureAwaiter(Task task)
            {
                _task = task;
            }

            public CultureAwaiter GetAwaiter()
            {
                return this;
            }

            public bool IsCompleted
            {
                get { return _task.IsCompleted; }
            }

            public void GetResult()
            {
                _task.GetAwaiter().GetResult();
            }

            public void OnCompleted(Action continuation)
            {
                // The compiler will never call this method
                throw new NotImplementedException();
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                var currentCulture = Thread.CurrentThread.CurrentCulture;
                _task.ConfigureAwait(false).GetAwaiter().UnsafeOnCompleted(() =>
                {
                    var originalCulture = Thread.CurrentThread.CurrentCulture;
                    Thread.CurrentThread.CurrentCulture = currentCulture;
                    try
                    {
                        continuation();
                    }
                    finally
                    {
                        Thread.CurrentThread.CurrentCulture = originalCulture;
                    }
                });
            }
        }
    }
}
#endif