// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class CommitFailedExceptionTests
    {
        [Fact]
        public void Constructors_can_be_passed_null_or_empty_message_without_throwing()
        {
            Assert.Contains(
                "System.Data.Entity.Infrastructure.CommitFailedException",
                new CommitFailedException(null).Message);

            Assert.Equal("", new CommitFailedException("").Message);
            Assert.Equal(" ", new CommitFailedException(" ").Message);

            Assert.Contains(
                "System.Data.Entity.Infrastructure.CommitFailedException",
                new CommitFailedException(null, new Exception()).Message);

            Assert.Equal("", new CommitFailedException("", new Exception()).Message);
            Assert.Equal(" ", new CommitFailedException(" ", new Exception()).Message);

            Assert.Null(new CommitFailedException("Foo", null).InnerException);
        }

        [Fact]
        public void Default_constructor_uses_default_message()
        {
            Assert.Equal(Strings.CommitFailed, new CommitFailedException().Message);
        }
    }
}
