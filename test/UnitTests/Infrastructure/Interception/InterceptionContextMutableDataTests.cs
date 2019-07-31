// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Entity.Resources;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Extensions;

    public class InterceptionContextMutableDataTests : TestBase
    {
        [Fact]
        public void HasExecuted_can_be_changed()
        {
            var data = new InterceptionContextMutableData();

            Assert.False(data.HasExecuted);

            data.HasExecuted = true;

            Assert.True(data.HasExecuted);
        }

        [Fact]
        public void TaskStatus_can_be_changed()
        {
            var data = new InterceptionContextMutableData();

            Assert.Equal((TaskStatus)0, data.TaskStatus);

            data.TaskStatus = TaskStatus.Running;

            Assert.Equal(TaskStatus.Running, data.TaskStatus);
        }

        [Fact]
        public void SuppressExecution_can_be_called_before_execution()
        {
            var data = new InterceptionContextMutableData();

            Assert.False(data.IsExecutionSuppressed);

            data.SuppressExecution();

            Assert.True(data.IsExecutionSuppressed);
        }

        [Fact]
        public void SuppressExecution_throws_if_called_after_execution()
        {
            var data = new InterceptionContextMutableData { HasExecuted = true };

            Assert.Equal(
                Strings.SuppressionAfterExecution,
                Assert.Throws<InvalidOperationException>(() => data.SuppressExecution()).Message);

            Assert.False(data.IsExecutionSuppressed);
        }

        [Fact]
        public void OriginalException_can_be_changed()
        {
            var data = new InterceptionContextMutableData();

            Assert.Null(data.OriginalException);

            var exception = new Exception();
            data.OriginalException = exception;

            Assert.Same(exception, data.OriginalException);
        }

        [Fact]
        public void Exception_can_be_changed_after_execution_without_setting_IsExecutionSuppressed()
        {
            var data = new InterceptionContextMutableData { HasExecuted = true };

            Assert.Null(data.Exception);

            var exception = new Exception();
            data.Exception = exception;

            Assert.Same(exception, data.Exception);
            Assert.False(data.IsExecutionSuppressed);
        }

        [Fact]
        public void Setting_Exception_before_execution_causes_IsExecutionSuppressed_to_be_set()
        {
            var data = new InterceptionContextMutableData();

            Assert.Null(data.Exception);

            var exception = new Exception();
            data.Exception = exception;

            Assert.Same(exception, data.Exception);
            Assert.True(data.IsExecutionSuppressed);
        }

        [Fact]
        public void SetExceptionThrown_sets_Exception_and_OriginalException_and_marks_operation_as_executed()
        {
            var data = new InterceptionContextMutableData();
            var exception = new Exception();
            data.SetExceptionThrown(exception);

            Assert.Same(exception, data.OriginalException);
            Assert.Same(exception, data.Exception);
            Assert.True(data.HasExecuted);
            Assert.False(data.IsExecutionSuppressed);
        }

        [Fact]
        public void OriginalResult_can_be_changed()
        {
            var data = new InterceptionContextMutableData<string>();

            Assert.Null(data.OriginalResult);

            data.OriginalResult = "Wensleydale";

            Assert.Equal("Wensleydale", data.OriginalResult);
        }

        [Fact]
        public void Result_can_be_changed_after_execution_without_setting_IsExecutionSuppressed()
        {
            var data = new InterceptionContextMutableData<string> { HasExecuted = true };

            Assert.Null(data.Result);

            data.Result = "Wensleydale";

            Assert.Equal("Wensleydale", data.Result);
            Assert.False(data.IsExecutionSuppressed);
        }

        [Fact]
        public void Setting_Result_before_execution_causes_IsExecutionSuppressed_to_be_set()
        {
            var data = new InterceptionContextMutableData<string>();

            Assert.Null(data.Result);

            data.Result = "Wensleydale";

            Assert.Equal("Wensleydale", data.Result);
            Assert.True(data.IsExecutionSuppressed);
        }

        [Fact]
        public void SetExecuted_sets_Exception_and_OriginalException_and_marks_operation_as_executed()
        {
            var data = new InterceptionContextMutableData<string>();
            data.SetExecuted("Wensleydale");

            Assert.Equal("Wensleydale", data.OriginalResult);
            Assert.Equal("Wensleydale", data.Result);
            Assert.True(data.HasExecuted);
            Assert.False(data.IsExecutionSuppressed);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UserState_can_be_changed(bool useObsoleteState)
        {
            var data = new InterceptionContextMutableData();

            if (useObsoleteState)
            {
#pragma warning disable 618
                Assert.Null(data.UserState);
#pragma warning restore 618
            }
            else
            {
                Assert.Null(data.FindUserState("A"));
                Assert.Null(data.FindUserState("B"));
            }

            if (useObsoleteState)
            {
#pragma warning disable 618
                data.UserState = "Cheddar";
#pragma warning restore 618
            }
            else
            {
                data.SetUserState("A", "AState");
                data.SetUserState("B", "BState");
            }

            if (useObsoleteState)
            {
#pragma warning disable 618
                Assert.Equal("Cheddar", data.UserState);
#pragma warning restore 618
            }
            else
            {
                Assert.Equal("AState", data.FindUserState("A"));
                Assert.Equal("BState", data.FindUserState("B"));
                Assert.Null(data.FindUserState("C"));
            }

            if (useObsoleteState)
            {
#pragma warning disable 618
                data.UserState = "Cheddar2";
#pragma warning restore 618
            }
            else
            {
                data.SetUserState("A", "AState2");
                data.SetUserState("B", "BState2");
            }

            if (useObsoleteState)
            {
#pragma warning disable 618
                Assert.Equal("Cheddar2", data.UserState);
#pragma warning restore 618
            }
            else
            {
                Assert.Equal("AState2", data.FindUserState("A"));
                Assert.Equal("BState2", data.FindUserState("B"));
                Assert.Null(data.FindUserState("C"));
            }
        }
    }
}
