// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Provides access to all dispatchers through the the <see cref="Interception.Dispatch" /> fluent API.
    /// </summary>
    public class Dispatchers
    {
        private readonly DbCommandTreeDispatcher _commandTreeDispatcher = new DbCommandTreeDispatcher();
        private readonly DbCommandDispatcher _commandDispatcher = new DbCommandDispatcher();
        private readonly EntityConnectionDispatcher _entityConnectionDispatcher = new EntityConnectionDispatcher();
        private readonly CancelableDbCommandDispatcher _cancelableCommandDispatcher = new CancelableDbCommandDispatcher();

        internal Dispatchers(IDbDependencyResolver resolver = null)
        {
            (resolver ?? DbConfiguration.DependencyResolver).GetServices<IDbInterceptor>().Each(AddInterceptor);
        }

        internal virtual DbCommandTreeDispatcher CommandTree
        {
            get { return _commandTreeDispatcher; }
        }

        /// <summary>
        ///     Provides methods for dispatching to <see cref="IDbCommandInterceptor" /> interceptors for
        ///     interception of methods on <see cref="DbCommand" />.
        /// </summary>
        public virtual DbCommandDispatcher Command
        {
            get { return _commandDispatcher; }
        }

        internal virtual EntityConnectionDispatcher EntityConnection
        {
            get { return _entityConnectionDispatcher; }
        }

        internal virtual CancelableDbCommandDispatcher CancelableCommand
        {
            get { return _cancelableCommandDispatcher; }
        }

        internal virtual void AddInterceptor(IDbInterceptor interceptor)
        {
            DebugCheck.NotNull(interceptor);

            _commandTreeDispatcher.InternalDispatcher.Add(interceptor);
            _commandDispatcher.InternalDispatcher.Add(interceptor);
            _entityConnectionDispatcher.InternalDispatcher.Add(interceptor);
            _cancelableCommandDispatcher.InternalDispatcher.Add(interceptor);
        }

        internal virtual void RemoveInterceptor(IDbInterceptor interceptor)
        {
            DebugCheck.NotNull(interceptor);

            _commandTreeDispatcher.InternalDispatcher.Remove(interceptor);
            _commandDispatcher.InternalDispatcher.Remove(interceptor);
            _entityConnectionDispatcher.InternalDispatcher.Remove(interceptor);
            _cancelableCommandDispatcher.InternalDispatcher.Remove(interceptor);
        }
    }
}
