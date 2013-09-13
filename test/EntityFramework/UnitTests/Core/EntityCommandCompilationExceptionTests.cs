// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using Xunit;

    public class EntityCommandCompilationExceptionTests
    {
        [Fact]
        public void Constructors_can_be_passed_null_or_empty_message_without_throwing()
        {
            Assert.True(new EntityCommandCompilationException(null).Message.Contains(
                "System.Data.Entity.Core.EntityCommandCompilationException"));

            Assert.Equal("", new EntityCommandCompilationException("").Message);
            Assert.Equal(" ", new EntityCommandCompilationException(" ").Message);

            Assert.True(new EntityCommandCompilationException(null, new Exception()).Message.Contains(
                "System.Data.Entity.Core.EntityCommandCompilationException"));

            Assert.Equal("", new EntityCommandCompilationException("", new Exception()).Message);
            Assert.Equal(" ", new EntityCommandCompilationException(" ", new Exception()).Message);

            Assert.Null(new EntityCommandCompilationException("Foo", null).InnerException);
        }
    }
}
