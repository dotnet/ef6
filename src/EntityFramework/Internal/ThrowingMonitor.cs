// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>
    ///     Provides a mechanism to ensure an exception is thrown on concurrent execution of a critical section.
    /// </summary>
    internal class ThrowingMonitor
    {
        // This is field is not volatile because we need stronger guarantees than volatile provides.
        // Instead we use Thread.MemoryBarrier to ensure freshness (Interlocked methods also use it internally).
        private int _isInCriticalSection;

        /// <summary>
        ///     Acquires an exclusive lock on this instance.
        ///     Any subsequent call to Enter before a call to Exit will result in an exception.
        /// </summary>
        public void Enter()
        {
            if (Interlocked.CompareExchange(ref _isInCriticalSection, 1, 0) != 0)
            {
                throw new NotSupportedException(Strings.ConcurrentMethodInvocation);
            }
        }

        /// <summary>
        ///     Releases an exclusive lock on this instance.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "state",
            Justification = "Used in the debug build")]
        public void Exit()
        {
            var state = Interlocked.Exchange(ref _isInCriticalSection, 0);
            Debug.Assert(state == 1, "Expected to be in a critical section");
        }

        /// <summary>
        ///     Throws an exception if an exclusive lock has been acquired on this instance.
        /// </summary>
        public void EnsureNotEntered()
        {
            // Ensure the value read from _isInCriticalSection is fresh
            Thread.MemoryBarrier();
            if (_isInCriticalSection != 0)
            {
                throw new NotSupportedException(Strings.ConcurrentMethodInvocation);
            }
        }
    }
}
