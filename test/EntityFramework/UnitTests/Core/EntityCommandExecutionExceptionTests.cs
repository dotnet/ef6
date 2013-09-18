// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using Xunit;

    public class EntityCommandExecutionExceptionTests
    {
        [Fact]
        public void Constructors_can_be_passed_null_or_empty_message_without_throwing()
        {
            Assert.Contains(
                "System.Data.Entity.Core.EntityCommandExecutionException",
                new EntityCommandExecutionException(null).Message);

            Assert.Equal("", new EntityCommandExecutionException("").Message);
            Assert.Equal(" ", new EntityCommandExecutionException(" ").Message);

            Assert.Contains(
                "System.Data.Entity.Core.EntityCommandExecutionException", 
                new EntityCommandExecutionException(null, new Exception()).Message);

            Assert.Equal("", new EntityCommandExecutionException("", new Exception()).Message);
            Assert.Equal(" ", new EntityCommandExecutionException(" ", new Exception()).Message);

            Assert.Null(new EntityCommandExecutionException("Foo", null).InnerException);
        }
    }
}
