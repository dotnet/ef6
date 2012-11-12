// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Config;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Reflection;
    using Moq.Protected;
    using Xunit;

    public sealed class TypeExtensionsTests
    {
        public class IsValidStructuralType
        {
            [Fact]
            public void IsValidStructuralType_should_return_false_for_invalid_types()
            {
                Assert.True(
                    new List<Type>
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
        }

        public class TryUnwrapNullableType
        {
            [Fact]
            public void NullableType_should_correctly_detect_nullable_type_and_return_underlying_type()
            {
                Type underlyingType;
                Assert.True(typeof(int?).TryUnwrapNullableType(out underlyingType));
                Assert.Equal(typeof(int), underlyingType);
                Assert.False(typeof(int).TryUnwrapNullableType(out underlyingType));
                Assert.Equal(typeof(int), underlyingType);
            }
        }

        public class IsCollection
        {
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
        }

        public class TryGetElementType
        {
            [Fact]
            public void TryGetElementType_returns_element_type_for_given_interface()
            {
                Assert.Same(typeof(string), typeof(ICollection<string>).TryGetElementType(typeof(ICollection<>)));
                Assert.Same(typeof(DbContext), typeof(IDatabaseInitializer<DbContext>).TryGetElementType(typeof(IDatabaseInitializer<>)));
                Assert.Same(typeof(int), typeof(List<int>).TryGetElementType(typeof(IList<>)));
                Assert.Same(
                    typeof(DbContext), typeof(MultipleImplementor<DbContext, string>).TryGetElementType(typeof(IDatabaseInitializer<>)));
                Assert.Same(typeof(string), typeof(MultipleImplementor<DbContext, string>).TryGetElementType(typeof(IEnumerable<>)));
            }

            [Fact]
            public void TryGetElementType_returns_null_if_type_is_generic_type_definition()
            {
                Assert.Null(typeof(ICollection<>).TryGetElementType(typeof(ICollection<>)));
            }

            [Fact]
            public void TryGetElementType_returns_null_if_type_doesnt_implement_interface()
            {
                Assert.Null(typeof(ICollection<string>).TryGetElementType(typeof(IDatabaseInitializer<>)));
                Assert.Null(typeof(Random).TryGetElementType(typeof(IDatabaseInitializer<>)));
            }

            public class MultipleImplementor<TContext, TElement> : IDatabaseInitializer<TContext>, IEnumerable<TElement>
                where TContext : DbContext
            {
                public void InitializeDatabase(TContext context)
                {
                }

                public IEnumerator<TElement> GetEnumerator()
                {
                    throw new NotImplementedException();
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
            }
        }

        public class CreateInstance
        {
            [Fact]
            public void CreateInstance_throws_if_type_does_not_have_parameterless_constructor()
            {
                Assert.Equal(
                    Strings.CreateInstance_NoParameterlessConstructor(typeof(BadConstructorConfiguration)),
                    Assert.Throws<InvalidOperationException>(
                        () => typeof(BadConstructorConfiguration)
                                  .CreateInstance<DbConfiguration>()).Message);
            }

            [Fact]
            public void CreateInstance_throws_if_type_is_abstract()
            {
                Assert.Equal(
                    Strings.CreateInstance_AbstractType(typeof(AbstractConfiguration)),
                    Assert.Throws<InvalidOperationException>(
                        () => typeof(AbstractConfiguration)
                                  .CreateInstance<DbConfiguration>()).Message);
            }

            [Fact]
            public void CreateInstance_throws_if_type_is_generic_type()
            {
                Assert.Equal(
                    Strings.CreateInstance_GenericType(typeof(GenericConfiguration<>)),
                    Assert.Throws<InvalidOperationException>(
                        () => typeof(GenericConfiguration<>)
                                  .CreateInstance<DbConfiguration>()).Message);
            }

            [Fact]
            public void CreateInstance_throws_expected_message_if_type_has_bad_base_type()
            {
                Assert.Equal(
                    Strings.CreateInstance_BadSqlGeneratorType(typeof(object).ToString(), typeof(MigrationSqlGenerator).ToString()),
                    Assert.Throws<InvalidOperationException>(
                        () => typeof(object)
                                  .CreateInstance<MigrationSqlGenerator>(Strings.CreateInstance_BadSqlGeneratorType)).Message);
            }

            [Fact]
            public void CreateInstance_throws_expected_exception_type_if_type_does_not_have_parameterless_constructor()
            {
                Assert.Equal(
                    Strings.CreateInstance_NoParameterlessConstructor(typeof(BadConstructorConfiguration)),
                    Assert.Throws<MigrationsException>(
                        () => typeof(BadConstructorConfiguration)
                                  .CreateInstance<DbConfiguration>(s => new MigrationsException(s))).Message);
            }

            [Fact]
            public void CreateInstance_throws_expected_exception_type_if_type_is_abstract()
            {
                Assert.Equal(
                    Strings.CreateInstance_AbstractType(typeof(AbstractConfiguration)),
                    Assert.Throws<MigrationsException>(
                        () => typeof(AbstractConfiguration)
                                  .CreateInstance<DbConfiguration>(s => new MigrationsException(s))).Message);
            }

            [Fact]
            public void CreateInstance_throws_expected_exception_type_if_type_is_generic_type()
            {
                Assert.Equal(
                    Strings.CreateInstance_GenericType(typeof(GenericConfiguration<>)),
                    Assert.Throws<MigrationsException>(
                        () => typeof(GenericConfiguration<>)
                                  .CreateInstance<DbConfiguration>(s => new MigrationsException(s))).Message);
            }

            [Fact]
            public void CreateInstance_throws_expected_exception_type_and_expected_message_if_type_has_bad_base_type()
            {
                Assert.Equal(
                    Strings.CreateInstance_BadMigrationsConfigurationType(
                        typeof(object).ToString(),
                        typeof(DbMigrationsConfiguration).ToString()),
                    Assert.Throws<MigrationsException>(
                        () => typeof(object)
                                  .CreateInstance<DbMigrationsConfiguration>(
                                      Strings.CreateInstance_BadMigrationsConfigurationType,
                                      s => new MigrationsException(s))).Message);
            }
        }

        #region Test Fixtures

        private sealed class ICollection_should_correctly_detect_collections_fixture : List<bool>
        {
        }

        public abstract class AbstractConfiguration : DbConfiguration
        {
            public AbstractConfiguration()
            {
                // prevent code cleanup removal
                DoStuff();
            }

            private void DoStuff()
            {
            }
        }

        public class GenericConfiguration<T> : DbConfiguration
        {
        }

        public abstract class BadConstructorConfiguration : DbConfiguration
        {
            protected BadConstructorConfiguration(int _)
            {
            }
        }

        #endregion
    }
}
