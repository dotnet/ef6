// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using Xunit;

    public class ExecutionOptionsTests
    {
        [Fact]
        public void Streaming_is_null_by_default()
        {
            var executionOptions = new ExecutionOptions(MergeOption.OverwriteChanges);

            Assert.Equal(MergeOption.OverwriteChanges, executionOptions.MergeOption);
            Assert.Null(executionOptions.UserSpecifiedStreaming);
        }

        [Fact]
        public void Equals_returns_correct_results()
        {
            TestEquals(
                (left, right) => ReferenceEquals(left, null)
                                     ? ReferenceEquals(right, null)
                                     : left.Equals(right));
        }

        [Fact]
        public void Equality_returns_correct_results()
        {
            TestEquals((left, right) => (ExecutionOptions)left == (ExecutionOptions)right);
        }

        [Fact]
        public void Inequality_returns_correct_results()
        {
            TestEquals((left, right) => !((ExecutionOptions)left != (ExecutionOptions)right));
        }

        private void TestEquals(Func<object, object, bool> equals)
        {
            var sameInstace = new ExecutionOptions(MergeOption.AppendOnly, streaming: false);
            Assert.True(
                equals(
                    sameInstace,
                    sameInstace));
            Assert.True(
                equals(
                    new ExecutionOptions(MergeOption.AppendOnly, false),
                    new ExecutionOptions(MergeOption.AppendOnly, false)));
            Assert.False(
                equals(
                    new ExecutionOptions(MergeOption.AppendOnly, false),
                    new ExecutionOptions(MergeOption.AppendOnly, true)));
            Assert.False(
                equals(
                    new ExecutionOptions(MergeOption.OverwriteChanges, false),
                    new ExecutionOptions(MergeOption.AppendOnly, false)));
            Assert.True(
                equals(
                    null,
                    null));
            Assert.False(
                equals(
                    null,
                    new ExecutionOptions(MergeOption.AppendOnly, true)));
            Assert.False(
                equals(
                    new ExecutionOptions(MergeOption.AppendOnly, false),
                    null));
        }

        [Fact]
        public void GetHashCode_returns_correct_results()
        {
            Assert.Equal(
                new ExecutionOptions(MergeOption.AppendOnly, false).GetHashCode(),
                new ExecutionOptions(MergeOption.AppendOnly, false).GetHashCode());
            Assert.NotEqual(
                new ExecutionOptions(MergeOption.AppendOnly, false).GetHashCode(),
                new ExecutionOptions(MergeOption.NoTracking, false).GetHashCode());
            Assert.NotEqual(
                new ExecutionOptions(MergeOption.AppendOnly, false).GetHashCode(),
                new ExecutionOptions(MergeOption.AppendOnly, true).GetHashCode());
        }
    }
}
