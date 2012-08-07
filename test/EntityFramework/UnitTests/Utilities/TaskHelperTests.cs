// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System;
    using System.Linq;
    using Xunit;

    public class TaskHelperTests
    {
        [Fact]
        public void FromCancellation_returns_task()
        {
            var cancelledTask = TaskHelper.FromCancellation<int>();

            Assert.True(cancelledTask.IsCompleted);
            Assert.True(cancelledTask.IsCanceled);
            Assert.False(cancelledTask.IsFaulted);
            Assert.Null(cancelledTask.Exception);
        }

        [Fact]
        public void FromException_returns_task_with_exception()
        {
            var exception = new InvalidOperationException();

            var exceptionTask = TaskHelper.FromException<int>(exception);

            Assert.True(exceptionTask.IsCompleted);
            Assert.False(exceptionTask.IsCanceled);
            Assert.True(exceptionTask.IsFaulted);
            Assert.Same(exception, exceptionTask.Exception.InnerExceptions.Single());
        }
    }
}
