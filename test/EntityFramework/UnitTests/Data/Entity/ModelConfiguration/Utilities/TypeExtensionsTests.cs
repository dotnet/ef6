namespace System.Data.Entity.ModelConfiguration.Utilities.UnitTests
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Objects.DataClasses;
    using System.Reflection;
    using Moq.Protected;
    using Xunit;

    public sealed class TypeExtensionsTests
    {
        [Fact]
        public void IsValidStructuralType_should_return_false_for_invalid_types()
        {
            Assert.True(new List<Type>
                {
                    typeof(object),
                    typeof(string),
                    typeof(ComplexObject),
                    typeof(EntityObject),
                    typeof(StructuralObject),
                    typeof(EntityKey),
                    typeof(EntityReference)
                }.TrueForAll(t => !t.IsValidStructuralType()));
        }

        [Fact]
        public void IsValidStructuralType_should_return_false_for_generic_types()
        {
            var mockType = new MockType();
            mockType.SetupGet(t => t.IsGenericType).Returns(true);

            Assert.False(mockType.Object.IsValidStructuralType());
        }

        [Fact]
        public void IsValidStructuralType_should_return_false_for_generic_type_definitions()
        {
            var mockType = new MockType();
            mockType.SetupGet(t => t.IsGenericTypeDefinition).Returns(true);

            Assert.False(mockType.Object.IsValidStructuralType());
        }

        [Fact]
        public void IsValidStructuralType_should_return_false_for_arrays()
        {
            var mockType = new MockType();
            mockType.Protected().Setup<bool>("IsArrayImpl").Returns(true);

            Assert.False(mockType.Object.IsValidStructuralType());
        }

        [Fact]
        public void IsValidStructuralType_should_return_true_for_enums()
        {
            var mockType = new MockType();
            mockType.SetupGet(t => t.IsEnum).Returns(true);

            Assert.True(mockType.Object.IsValidStructuralType());
        }

        [Fact]
        public void IsValidStructuralType_should_return_false_for_value_types()
        {
            var mockType = new MockType();
            mockType.Protected().Setup<bool>("IsValueTypeImpl").Returns(true);

            Assert.False(mockType.Object.IsValidStructuralType());
        }

        [Fact]
        public void IsValidStructuralType_should_return_false_for_interfaces()
        {
            var mockType = new MockType().TypeAttributes(TypeAttributes.Interface);

            Assert.False(mockType.Object.IsValidStructuralType());
        }

        [Fact]
        public void IsValidStructuralType_should_return_false_for_primitive_types()
        {
            Assert.False(typeof(int).IsValidStructuralType());
        }

        [Fact]
        public void IsValidStructuralType_should_return_false_for_nested_types()
        {
            var mockType = new MockType();
            mockType.SetupGet(t => t.DeclaringType).Returns(typeof(object));

            Assert.False(mockType.Object.IsValidStructuralType());
        }

        [Fact]
        public void NullableType_should_correctly_detect_nullable_type_and_return_underlying_type()
        {
            Type underlyingType;
            Assert.True(typeof(int?).TryUnwrapNullableType(out underlyingType));
            Assert.Equal(typeof(int), underlyingType);
            Assert.False(typeof(int).TryUnwrapNullableType(out underlyingType));
            Assert.Equal(typeof(int), underlyingType);
        }

        [Fact]
        public void IsCollection_should_correctly_detect_collections()
        {
            Assert.True(typeof(ICollection<string>).IsCollection());
            Assert.True(typeof(IList<string>).IsCollection());
            Assert.True(typeof(List<int>).IsCollection());
            Assert.True(typeof(ICollection_should_correctly_detect_collections_fixture).IsCollection());
            Assert.False(typeof(object).IsCollection());
            Assert.False(typeof(ICollection).IsCollection());
        }

        [Fact]
        public void IsCollection_should_return_collection_element_type()
        {
            Type elementType;

            Assert.True(typeof(ICollection<string>).IsCollection(out elementType));
            Assert.Equal(typeof(string), elementType);
            Assert.True(typeof(IList<object>).IsCollection(out elementType));
            Assert.Equal(typeof(object), elementType);
            Assert.True(typeof(List<int>).IsCollection(out elementType));
            Assert.Equal(typeof(int), elementType);
            Assert.True(typeof(ICollection_should_correctly_detect_collections_fixture).IsCollection(out elementType));
            Assert.Equal(typeof(bool), elementType);
        }

        #region Test Fixtures

        private sealed class ICollection_should_correctly_detect_collections_fixture : List<bool>
        {
        }

        #endregion
    }
}