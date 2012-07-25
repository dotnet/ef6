// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core
{
    using Xunit;

    public class EntityExceptionTests
    {
        [Fact]
        public void Constructors_can_be_passed_null_or_empty_message_without_throwing()
        {
            Assert.Equal(
                "Exception of type 'System.Data.Entity.Core.EntityException' was thrown.",
                new EntityException(null).Message);

            Assert.Equal("", new EntityException("").Message);
            Assert.Equal(" ", new EntityException(" ").Message);

            Assert.Equal(
                "Exception of type 'System.Data.Entity.Core.EntityException' was thrown.",
                new EntityException(null, new Exception()).Message);

            Assert.Equal("", new EntityException("", new Exception()).Message);
            Assert.Equal(" ", new EntityException(" ", new Exception()).Message);

            Assert.Null(new EntityException("Foo", null).InnerException);
        }
    }
}