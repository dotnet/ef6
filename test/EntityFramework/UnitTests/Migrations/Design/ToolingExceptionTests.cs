// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Design
{
    using Xunit;

    public class ToolingExceptionTests
    {
        [Fact]
        public void Constructors_allow_for_nulls()
        {
            Assert.True(new ToolingException(null).Message.Contains("'System.Data.Entity.Migrations.Design.ToolingException'"));
            Assert.True(new ToolingException(null, null).Message.Contains("'System.Data.Entity.Migrations.Design.ToolingException'"));
            Assert.Null(new ToolingException(null, null).InnerException);
            Assert.Null(new ToolingException(null, null, null).InnerType);
            Assert.Null(new ToolingException(null, null, null).InnerStackTrace);
            Assert.True(new ToolingException(null, null, null).Message.Contains("'System.Data.Entity.Migrations.Design.ToolingException'"));
        }

        [Fact]
        public void Parameterless_constructor_uses_default_message_and_sets_up_serialization()
        {
            var exception = new ToolingException();

            Assert.True(exception.Message.Contains("'System.Data.Entity.Migrations.Design.ToolingException'"));
            Assert.Null(exception.InnerType);
            Assert.Null(exception.InnerStackTrace);

            exception = ExceptionHelpers.SerializeAndDeserialize(exception);

            Assert.True(exception.Message.Contains("'System.Data.Entity.Migrations.Design.ToolingException'"));
            Assert.Null(exception.InnerType);
            Assert.Null(exception.InnerStackTrace);
        }

        [Fact]
        public void Constructor_uses_given_message_and_sets_up_serialization()
        {
            var exception = new ToolingException("It's Tool Time!");

            Assert.Equal("It's Tool Time!", exception.Message);
            Assert.Null(exception.InnerType);
            Assert.Null(exception.InnerStackTrace);

            exception = ExceptionHelpers.SerializeAndDeserialize(exception);

            Assert.Equal("It's Tool Time!", exception.Message);
            Assert.Null(exception.InnerType);
            Assert.Null(exception.InnerStackTrace);
        }

        [Fact]
        public void Constructor_uses_given_message_and_inner_exception_and_sets_up_serialization()
        {
            var innerException = new Exception("Hello? Hello?");
            var exception = new ToolingException("Can somebody let me out?", innerException);

            Assert.Equal("Can somebody let me out?", exception.Message);
            Assert.Same(innerException, exception.InnerException);
            Assert.Null(exception.InnerType);
            Assert.Null(exception.InnerStackTrace);

            exception = ExceptionHelpers.SerializeAndDeserialize(exception);

            Assert.Equal("Can somebody let me out?", exception.Message);
            Assert.Equal(innerException.Message, exception.InnerException.Message);
            Assert.Null(exception.InnerType);
            Assert.Null(exception.InnerStackTrace);
        }

        [Fact]
        public void Constructor_uses_given_detailed_information_and_sets_up_serialization()
        {
            var exception = new ToolingException("Really?", "INTP", "Where's my tracing paper?");

            Assert.Equal("Really?", exception.Message);
            Assert.Equal("INTP", exception.InnerType);
            Assert.Equal("Where's my tracing paper?", exception.InnerStackTrace);

            exception = ExceptionHelpers.SerializeAndDeserialize(exception);

            Assert.Equal("Really?", exception.Message);
            Assert.Equal("INTP", exception.InnerType);
            Assert.Equal("Where's my tracing paper?", exception.InnerStackTrace);
        }
    }
}
