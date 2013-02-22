// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class PropertyConstraintExceptionTests : TestBase
    {
        [Fact]
        public void PropertyConstraintException_exposes_public_empty_constructor()
        {
            var ex = new PropertyConstraintException();

            Assert.Null(ex.PropertyName);

            ex = ExceptionHelpers.SerializeAndDeserialize(ex);

            Assert.Null(ex.PropertyName);
        }

        [Fact]
        public void PropertyConstraintException_exposes_public_string_constructor()
        {
            var ex = new PropertyConstraintException("Message");

            Assert.Equal("Message", ex.Message);
            Assert.Null(ex.PropertyName);

            ex = ExceptionHelpers.SerializeAndDeserialize(ex);

            Assert.Equal("Message", ex.Message);
            Assert.Null(ex.PropertyName);
        }

        [Fact]
        public void PropertyConstraintException_exposes_public_string_and_inner_exception_constructor()
        {
            var inner = new Exception("Don't look down.");
            var ex = new PropertyConstraintException("Message", inner);

            Assert.Equal("Message", ex.Message);
            Assert.Same(inner, ex.InnerException);
            Assert.Null(ex.PropertyName);

            ex = ExceptionHelpers.SerializeAndDeserialize(ex);

            Assert.Equal("Message", ex.Message);
            Assert.Equal(inner.Message, ex.InnerException.Message);
            Assert.Null(ex.PropertyName);
        }

        [Fact]
        public void PropertyConstraintException_exposes_public_string_and_property_name_constructor()
        {
            var ex = new PropertyConstraintException("Message", "Property");

            Assert.Equal("Message", ex.Message);
            Assert.Equal("Property", ex.PropertyName);

            ex = ExceptionHelpers.SerializeAndDeserialize(ex);

            Assert.Equal("Message", ex.Message);
            Assert.Equal("Property", ex.PropertyName);
        }

        [Fact]
        public void PropertyConstraintException_exposes_public_string_property_name_and_inner_exception_constructor()
        {
            var inner = new Exception("The cracks of doom!");
            var ex = new PropertyConstraintException("Message", "Property", inner);

            Assert.Equal("Message", ex.Message);
            Assert.Equal("Property", ex.PropertyName);
            Assert.Same(inner, ex.InnerException);

            ex = ExceptionHelpers.SerializeAndDeserialize(ex);

            Assert.Equal("Message", ex.Message);
            Assert.Equal("Property", ex.PropertyName);
            Assert.Equal(inner.Message, ex.InnerException.Message);
        }

        [Fact]
        public void PropertyConstraintException_string_and_property_name_constructor_throws_if_passed_null_property_name()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("propertyName"),
                Assert.Throws<ArgumentException>(() => new PropertyConstraintException("Message", (string)null)).Message);
        }

        [Fact]
        public void PropertyConstraintException_string_property_name_and_inner_exception_constructor_throws_if_passed_null_property_name()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("propertyName"),
                Assert.Throws<ArgumentException>(() => new PropertyConstraintException("Message", null, new Exception())).Message);
        }

        [Fact]
        public void PropertyConstraintException_is_marked_as_Serializable()
        {
            Assert.True(typeof(PropertyConstraintException).GetCustomAttributes(typeof(SerializableAttribute), inherit: false).Any());
        }
    }
}
