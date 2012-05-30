namespace ProductivityApiUnitTests
{
    using System;
    using System.Data.Entity.Core;
    using System.Data;
    using System.Data.Entity;
    using System.Linq;
    using Xunit;

    public class PropertyConstraintExceptionTests : TestBase
    {
        #region Tests for constructors

        [Fact]
        public void PropertyConstraintException_exposes_public_empty_constructor()
        {
            var ex = new PropertyConstraintException();

            Assert.Null(ex.PropertyName);
        }

        [Fact]
        public void PropertyConstraintException_exposes_public_string_constructor()
        {
            var ex = new PropertyConstraintException("Message");

            Assert.Equal("Message", ex.Message);
            Assert.Null(ex.PropertyName);
        }

        [Fact]
        public void PropertyConstraintException_exposes_public_string_and_inner_exception_constructor()
        {
            var inner = new Exception();

            var ex = new PropertyConstraintException("Message", inner);

            Assert.Equal("Message", ex.Message);
            Assert.Same(inner, ex.InnerException);
            Assert.Null(ex.PropertyName);
        }

        [Fact]
        public void PropertyConstraintException_exposes_public_string_and_property_name_constructor()
        {
            var ex = new PropertyConstraintException("Message", "Property");

            Assert.Equal("Message", ex.Message);
            Assert.Equal("Property", ex.PropertyName);
        }

        [Fact]
        public void PropertyConstraintException_exposes_public_string_property_name_and_inner_exception_constructor()
        {
            var inner = new Exception();

            var ex = new PropertyConstraintException("Message", "Property", inner);

            Assert.Equal("Message", ex.Message);
            Assert.Equal("Property", ex.PropertyName);
            Assert.Same(inner, ex.InnerException);
        }

        [Fact]
        public void PropertyConstraintException_string_and_property_name_constructor_throws_if_passed_null_property_name()
        {
            Assert.Equal("propertyName", Assert.Throws<ArgumentNullException>(() => new PropertyConstraintException("Message", (string)null)).ParamName);
        }

        [Fact]
        public void PropertyConstraintException_string_property_name_and_inner_exception_constructor_throws_if_passed_null_property_name()
        {
            Assert.Equal("propertyName", Assert.Throws<ArgumentNullException>(() => new PropertyConstraintException("Message", (string)null, new Exception())).ParamName);
        }

        #endregion

        #region Serialization tests

        [Fact]
        public void PropertyConstraintException_is_marked_as_Serializable()
        {
            Assert.True(typeof(PropertyConstraintException).GetCustomAttributes(typeof(SerializableAttribute), inherit: false).Any());
        }

        [Fact]
        public void PropertyConstraintException_with_non_null_property_name_can_be_serialized_and_deserialized()
        {
            var ex = ExceptionHelpers.SerializeAndDeserialize(new PropertyConstraintException("Message", "Property", new InvalidOperationException("Inner")));

            Assert.Equal("Message", ex.Message);
            Assert.Equal("Property", ex.PropertyName);
            Assert.Equal("Inner", ex.InnerException.Message);
        }

        [Fact]
        public void PropertyConstraintException_with_null_property_name_can_be_serialized_and_deserialized()
        {
            var ex = ExceptionHelpers.SerializeAndDeserialize(new PropertyConstraintException());

            Assert.Null(ex.PropertyName);
            Assert.Null(ex.InnerException);
        }

        #endregion
    }
}
