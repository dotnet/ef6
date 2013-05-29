// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     This is the registration point for <see cref="IDbInterceptor" /> interceptors. Interceptors
    ///     receive notifications when EF performs certain operations such as executing commands against
    ///     the database. For example, see <see cref="IDbCommandInterceptor" />.
    /// </summary>
    public static class Interception
    {
        private static readonly Lazy<Dispatchers> _dispatchers = new Lazy<Dispatchers>(() => new Dispatchers());

        /// <summary>
        ///     Registers a new <see cref="IDbInterceptor" /> to receive notifications. Note that the interceptor
        ///     must implement some interface that extends from <see cref="IDbInterceptor" /> to be useful.
        /// </summary>
        /// <param name="interceptor">The interceptor to add.</param>
        public static void AddInterceptor(IDbInterceptor interceptor)
        {
            Check.NotNull(interceptor, "interceptor");

            _dispatchers.Value.AddInterceptor(interceptor);
        }

        /// <summary>
        ///     Removes a registered <see cref="IDbInterceptor" /> so that it will no longer receive notifications.
        ///     If the given interceptor is not registered, then this is a no-op.
        /// </summary>
        /// <param name="interceptor">The interceptor to remove.</param>
        public static void RemoveInterceptor(IDbInterceptor interceptor)
        {
            Check.NotNull(interceptor, "interceptor");

            _dispatchers.Value.RemoveInterceptor(interceptor);
        }

        /// <summary>
        ///     This is the entry point for dispatching to interceptors. This is usually only used internally by
        ///     Entity Framework but it is provided publicly so that other code can make sure that registered
        ///     interceptors are called when operations are performed on behalf of EF. For example, EF providers
        ///     a may make use of this when executing commands.
        /// </summary>
        public static Dispatchers Dispatch
        {
            get { return _dispatchers.Value; }
        }
    }
}
