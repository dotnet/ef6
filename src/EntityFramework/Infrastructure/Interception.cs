// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    public class Interception : IDisposable
    {
        internal static readonly Interception Instance = new Interception();

        public static void AddInterceptor(IDbInterceptor interceptor)
        {
            Check.NotNull(interceptor, "interceptor");

            Instance.Add(interceptor);
        }

        public static void RemoveInterceptor(IDbInterceptor interceptor)
        {
            Check.NotNull(interceptor, "interceptor");

            Instance.Remove(interceptor);
        }

        public static void AddInterceptor(DbContext context, IDbInterceptor interceptor)
        {
            Check.NotNull(context, "context");
            Check.NotNull(interceptor, "interceptor");

            Instance.Add(context, interceptor);
        }

        public static void RemoveInterceptor(DbContext context, IDbInterceptor interceptor)
        {
            Check.NotNull(context, "context");
            Check.NotNull(interceptor, "interceptor");

            Instance.Remove(context, interceptor);
        }

        private readonly List<IDbInterceptor> _globalInterceptors
            = new List<IDbInterceptor>();

        private readonly ConcurrentDictionary<DbContext, IList<IDbInterceptor>> _perContextInterceptors
            = new ConcurrentDictionary<DbContext, IList<IDbInterceptor>>();

        private readonly ThreadLocal<DbContext> _activeContext = new ThreadLocal<DbContext>(() => null);

        internal Interception()
        {
        }

        public virtual void Add(IDbInterceptor interceptor)
        {
            Check.NotNull(interceptor, "interceptor");

            lock (_globalInterceptors)
            {
                Debug.Assert(!_globalInterceptors.Contains(interceptor));

                _globalInterceptors.Add(interceptor);
            }
        }

        public virtual void Remove(IDbInterceptor interceptor)
        {
            Check.NotNull(interceptor, "interceptor");

            lock (_globalInterceptors)
            {
                Debug.Assert(_globalInterceptors.Contains(interceptor));

                _globalInterceptors.Remove(interceptor);
            }
        }

        public virtual void Add(DbContext context, IDbInterceptor interceptor)
        {
            Check.NotNull(context, "context");
            Check.NotNull(interceptor, "interceptor");

            var interceptors
                = _perContextInterceptors
                    .GetOrAdd(context, new List<IDbInterceptor>());

            lock (interceptors)
            {
                Debug.Assert(!interceptors.Contains(interceptor));

                interceptors.Add(interceptor);
            }
        }

        public virtual void Remove(DbContext context, IDbInterceptor interceptor)
        {
            Check.NotNull(context, "context");
            Check.NotNull(interceptor, "interceptor");

            IList<IDbInterceptor> interceptors;

            if (_perContextInterceptors.TryGetValue(context, out interceptors))
            {
                lock (interceptors)
                {
                    Debug.Assert(interceptors.Contains(interceptor));

                    interceptors.Remove(interceptor);
                }
            }
        }

        internal virtual void SetActiveContext(DbContext context)
        {
            _activeContext.Value = context;
        }

        internal virtual bool Dispatch(DbCommand command)
        {
            DebugCheck.NotNull(command);

            return Dispatch(true, (b, i) => i.CommandExecuting(command) && b);
        }

        internal virtual DbCommandTree Dispatch(DbCommandTree commandTree)
        {
            DebugCheck.NotNull(commandTree);

            return Dispatch(commandTree, (r, i) => i.CommandTreeCreated(r));
        }

        internal virtual bool Dispatch(EntityConnection entityConnection)
        {
            DebugCheck.NotNull(entityConnection);

            return Dispatch(true, (b, i) => i.ConnectionOpening(entityConnection) && b);
        }

        private TResult Dispatch<TResult>(
            TResult result,
            Func<TResult, IDbInterceptor, TResult> accumulator)
        {
            DebugCheck.NotNull(accumulator);

            if (_activeContext.Value != null)
            {
                IList<IDbInterceptor> interceptors;

                if (_perContextInterceptors.TryGetValue(_activeContext.Value, out interceptors))
                {
                    IList<IDbInterceptor> interceptorsCopy;

                    lock (interceptors)
                    {
                        interceptorsCopy = interceptors.ToList();
                    }

                    result = interceptorsCopy.Aggregate(result, accumulator);
                }
            }

            IEnumerable<IDbInterceptor> globalInterceptors;

            lock (_globalInterceptors)
            {
                globalInterceptors = _globalInterceptors.ToList();
            }

            return globalInterceptors.Aggregate(result, accumulator);
        }

        ~Interception()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _activeContext.Dispose();
            }
        }
    }
}
