// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class ThrowingMonitorTests
    {
        [Fact]
        public void Enter_throws_when_called_twice()
        {
            var monitor = new ThrowingMonitor();

            monitor.Enter();
            Assert.Equal(Strings.ConcurrentMethodInvocation,
                Assert.Throws<NotSupportedException>(() =>
            monitor.Enter()).Message);
        }

        [Fact]
        public void EnsureNotEntered_doesnt_throw_before_enter_or_after_Exit()
        {
            var monitor = new ThrowingMonitor();

            monitor.EnsureNotEntered();
            monitor.Enter();
            monitor.Exit();
            monitor.EnsureNotEntered();
        }

        [Fact]
        public void EnsureNotEntered_throws_after_enter()
        {
            var monitor = new ThrowingMonitor();

            monitor.Enter();
            Assert.Equal(Strings.ConcurrentMethodInvocation,
                Assert.Throws<NotSupportedException>(() =>
            monitor.EnsureNotEntered()).Message);
        }
    }
}
