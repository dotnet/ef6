// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Data.Entity.Core.Common.EntitySql;
    using System.Data.Entity.Resources;
    using System.Reflection;
    using Xunit;

    public class EntitySqlExceptionTests
    {
        private static readonly PropertyInfo _hResultProperty = typeof(Exception).GetProperty(
            "HResult", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        private const int HResultInvalidQuery = -2146232006;

        [Fact]
        public void Constructors_allow_for_null_message_and_inner_exception()
        {
            Assert.True(new EntitySqlException(null).Message.Contains("'System.Data.Entity.Core.EntitySqlException'"));
            Assert.True(new EntitySqlException(null, null).Message.Contains("'System.Data.Entity.Core.EntitySqlException'"));
            Assert.Null(new EntitySqlException(null, null).InnerException);
        }

        [Fact]
        public void Parameterless_constructor_uses_general_message_and_sets_up_serialization()
        {
            var exception = new EntitySqlException();

            Assert.Equal(Strings.GeneralQueryError, exception.Message);
            Assert.Equal(HResultInvalidQuery, GetHResult(exception));
            Assert.Equal("", exception.ErrorContext);
            Assert.Equal("", exception.ErrorDescription);
            Assert.Equal(0, exception.Line);
            Assert.Equal(0, exception.Column);

            exception = ExceptionHelpers.SerializeAndDeserialize(exception);

            Assert.Equal(Strings.GeneralQueryError, exception.Message);
            Assert.Equal(HResultInvalidQuery, GetHResult(exception));
            Assert.Equal("", exception.ErrorContext);
            Assert.Equal("", exception.ErrorDescription);
            Assert.Equal(0, exception.Line);
            Assert.Equal(0, exception.Column);
        }

        [Fact]
        public void Constructor_uses_given_message_and_sets_up_serialization()
        {
            var exception = new EntitySqlException("What is this eSQL of which you speak?");

            Assert.Equal("What is this eSQL of which you speak?", exception.Message);
            Assert.Equal(HResultInvalidQuery, GetHResult(exception));
            Assert.Equal("", exception.ErrorContext);
            Assert.Equal("", exception.ErrorDescription);
            Assert.Equal(0, exception.Line);
            Assert.Equal(0, exception.Column);

            exception = ExceptionHelpers.SerializeAndDeserialize(exception);

            Assert.Equal("What is this eSQL of which you speak?", exception.Message);
            Assert.Equal(HResultInvalidQuery, GetHResult(exception));
            Assert.Equal("", exception.ErrorContext);
            Assert.Equal("", exception.ErrorDescription);
            Assert.Equal(0, exception.Line);
            Assert.Equal(0, exception.Column);
        }

        [Fact]
        public void Constructor_uses_given_message_and_inner_exception_and_sets_up_serialization()
        {
            var innerException = new Exception("I'm in here.");
            var exception = new EntitySqlException("I knoweth not, good sir.", innerException);

            Assert.Equal("I knoweth not, good sir.", exception.Message);
            Assert.Same(innerException, exception.InnerException);
            Assert.Equal(HResultInvalidQuery, GetHResult(exception));
            Assert.Equal("", exception.ErrorContext);
            Assert.Equal("", exception.ErrorDescription);
            Assert.Equal(0, exception.Line);
            Assert.Equal(0, exception.Column);

            exception = ExceptionHelpers.SerializeAndDeserialize(exception);

            Assert.Equal("I knoweth not, good sir.", exception.Message);
            Assert.Equal(innerException.Message, exception.InnerException.Message);
            Assert.Equal(HResultInvalidQuery, GetHResult(exception));
            Assert.Equal("", exception.ErrorContext);
            Assert.Equal("", exception.ErrorDescription);
            Assert.Equal(0, exception.Line);
            Assert.Equal(0, exception.Column);
        }

        [Fact]
        public void Create_uses_given_message_inner_exception_and_error_context_and_sets_up_serialization()
        {
            var innerException = new Exception("Hello");
            var exception = EntitySqlException.Create(
                new ErrorContext
                    {
                        CommandText = "select redHook\n from\n breweries",
                        ErrorContextInfo = "Hubcap emotional barometer is peaking",
                        InputPosition = 22,
                        UseContextInfoAsResourceIdentifier = false
                    },
                "Why not use LINQ like everyone else?",
                innerException);

            Assert.True(exception.Message.StartsWith("Why not use LINQ like everyone else?"));
            Assert.True(exception.Message.Contains("Hubcap emotional barometer is peaking"));
            Assert.Same(innerException, exception.InnerException);
            Assert.Equal(HResultInvalidQuery, GetHResult(exception));
            Assert.True(exception.ErrorContext.StartsWith("Hubcap emotional barometer is peaking"));
            Assert.Equal("Why not use LINQ like everyone else?", exception.ErrorDescription);
            Assert.Equal(3, exception.Line);
            Assert.Equal(2, exception.Column);

            exception = ExceptionHelpers.SerializeAndDeserialize(exception);

            Assert.True(exception.Message.StartsWith("Why not use LINQ like everyone else?"));
            Assert.True(exception.Message.Contains("Hubcap emotional barometer is peaking"));
            Assert.Equal(innerException.Message, exception.InnerException.Message);
            Assert.Equal(HResultInvalidQuery, GetHResult(exception));
            Assert.True(exception.ErrorContext.StartsWith("Hubcap emotional barometer is peaking"));
            Assert.Equal("Why not use LINQ like everyone else?", exception.ErrorDescription);
            Assert.Equal(3, exception.Line);
            Assert.Equal(2, exception.Column);
        }

        [Fact]
        public void Create_allows_all_state_to_be_set_and_sets_up_serialization()
        {
            var innerException = new Exception("Green Giant");
            var exception = EntitySqlException.Create(
                commandText: "select brandy\n from\n spirits",
                errorDescription: "This isn't the vodka I ordered.",
                errorPosition: 17,
                errorContextInfo: "It's Polish plum brandy, dude!",
                loadErrorContextInfoFromResource: true,
                innerException: innerException);

            Assert.True(exception.Message.StartsWith("This isn't the vodka I ordered."));
            Assert.Same(innerException, exception.InnerException);
            Assert.Equal(HResultInvalidQuery, GetHResult(exception));
            Assert.Equal("This isn't the vodka I ordered.", exception.ErrorDescription);
            Assert.Equal(2, exception.Line);
            Assert.Equal(4, exception.Column);

            exception = ExceptionHelpers.SerializeAndDeserialize(exception);

            Assert.True(exception.Message.StartsWith("This isn't the vodka I ordered."));
            Assert.Equal(innerException.Message, exception.InnerException.Message);
            Assert.Equal(HResultInvalidQuery, GetHResult(exception));
            Assert.Equal("This isn't the vodka I ordered.", exception.ErrorDescription);
            Assert.Equal(2, exception.Line);
            Assert.Equal(4, exception.Column);
        }

        [Fact] // CodePlex 1107
        public void Deserialized_exception_can_be_serialized_and_deserialized_again()
        {
            var exception = ExceptionHelpers.SerializeAndDeserialize(
                ExceptionHelpers.SerializeAndDeserialize(
                    new EntitySqlException("What is this eSQL of which you speak?")));

            Assert.Equal("What is this eSQL of which you speak?", exception.Message);
            Assert.Equal(HResultInvalidQuery, GetHResult(exception));
        }

        private static int GetHResult(Exception ex)
        {
            return (int)_hResultProperty.GetValue(ex, null);
        }
    }
}
