// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Config;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Linq;
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
            public void IsValidStructuralType_should_return_true_for_nested_types()
            {
                var mockType = new MockType();
                mockType.SetupGet(t => t.DeclaringType).Returns(typeof(object));

                Assert.True(mockType.Object.IsValidStructuralType());
            }
        }

        public class GetNonIndexerProperties
        {
            [Fact]
            public void Should_exclude_indexers()
            {
                Assert.Equal("Length", typeof(string).GetNonIndexerProperties().Single().Name);
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

        private sealed class ICollection_should_correctly_detect_collections_fixture : List<bool>
        {
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

            public void TryGetElementType_returns_element_type_for_given_class()
            {
                Assert.Same(typeof(string), typeof(Collection<string>).TryGetElementType(typeof(Collection<>)));
                Assert.Same(typeof(int), typeof(List<int>).TryGetElementType(typeof(List<>)));
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

            [Fact]
            public void TryGetElementType_returns_null_if_type_doesnt_implement_class()
            {
                Assert.Null(typeof(ICollection<string>).TryGetElementType(typeof(List<>)));
                Assert.Null(typeof(Random).TryGetElementType(typeof(Collection<>)));
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

        public class GetBaseTypes
        {
            [Fact]
            public void GetBaseTypes_return_all_base_types()
            {
                Assert.Equal(3, typeof(MultipleHierarchy).GetBaseTypes().Count());
                Assert.True(typeof(MultipleHierarchy).GetBaseTypes().Contains(typeof(Some)));
                Assert.True(typeof(MultipleHierarchy).GetBaseTypes().Contains(typeof(Base)));
                Assert.True(typeof(MultipleHierarchy).GetBaseTypes().Contains(typeof(object)));
            }

            [Fact]
            public void GetBaseTypes_return_empty_if_no_base_type_exists()
            {
                Assert.False(typeof(object).GetBaseTypes().Any());
            }

            public class MultipleHierarchy : Some
            {
            }

            public class Some : Base
            {
            }

            public class Base
            {
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

            public abstract class BadConstructorConfiguration : DbConfiguration
            {
                protected BadConstructorConfiguration(int _)
                {
                }
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

            [Fact]
            public void CreateInstance_throws_expected_exception_type_if_type_is_generic_type()
            {
                Assert.Equal(
                    Strings.CreateInstance_GenericType(typeof(GenericConfiguration<>)),
                    Assert.Throws<MigrationsException>(
                        () => typeof(GenericConfiguration<>)
                                  .CreateInstance<DbConfiguration>(s => new MigrationsException(s))).Message);
            }

            public class GenericConfiguration<T> : DbConfiguration
            {
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

        public class NestingNamespace
        {
            [Fact]
            public void NestingNamespace_returns_simple_namespace_for_non_nested_type()
            {
                Assert.Equal("System.Data.Entity.Utilities", typeof(TypeExtensionsTests).NestingNamespace());
            }

            [Fact]
            public void NestingNamespace_returns_correct_namespace_for_nested_type()
            {
                Assert.Equal("System.Data.Entity.Utilities.TypeExtensionsTests", typeof(NestingNamespace).NestingNamespace());
            }

            public class MoreNested
            {
            }

            [Fact]
            public void NestingNamespace_returns_correct_namespace_for_double_nested_type()
            {
                Assert.Equal("System.Data.Entity.Utilities.TypeExtensionsTests.NestingNamespace", typeof(MoreNested).NestingNamespace());
            }

            [Fact]
            public void NestingNamespace_returns_null_for_non_nested_type_not_in_a_namespace()
            {
                Assert.Null(typeof(NoNamespaceClass).NestingNamespace());
            }

            [Fact]
            public void NestingNamespace_returns_correct_namespace_for_nested_type_not_in_a_namespace()
            {
                Assert.Equal("NoNamespaceClass", typeof(NoNamespaceClass.Nested).NestingNamespace());
            }
        }

        public class FullNameWithNesting
        {
            [Fact]
            public void FullNameWithNesting_returns_simple_name_for_non_nested_type()
            {
                Assert.Equal("System.Data.Entity.Utilities.TypeExtensionsTests", typeof(TypeExtensionsTests).FullNameWithNesting());
            }

            [Fact]
            public void FullNameWithNesting_returns_correct_name_for_nested_type()
            {
                Assert.Equal(
                    "System.Data.Entity.Utilities.TypeExtensionsTests.FullNameWithNesting",
                    typeof(FullNameWithNesting).FullNameWithNesting());
            }

            public class MoreNested
            {
            }

            [Fact]
            public void FullNameWithNesting_returns_correct_name_for_double_nested_type()
            {
                Assert.Equal(
                    "System.Data.Entity.Utilities.TypeExtensionsTests.FullNameWithNesting.MoreNested",
                    typeof(MoreNested).FullNameWithNesting());
            }

            [Fact]
            public void FullNameWithNesting_returns_correct_name_for_non_nested_type_not_in_a_namespace()
            {
                Assert.Equal("NoNamespaceClass", typeof(NoNamespaceClass).FullNameWithNesting());
            }

            [Fact]
            public void FullNameWithNesting_returns_correct_name_for_nested_type_not_in_a_namespace()
            {
                Assert.Equal("NoNamespaceClass.Nested", typeof(NoNamespaceClass.Nested).FullNameWithNesting());
            }
        }

        public class OverridesEqualsOrGetHashCode
        {
            [Fact]
            public void Check_returns_false_for_object()
            {
                Assert.False(typeof(object).OverridesEqualsOrGetHashCode());
            }

            public class NoOverride
            {
            }

            [Fact]
            public void Check_returns_false_for_class_that_does_not_override_Equals_or_GetHashCode()
            {
                Assert.False(typeof(NoOverride).OverridesEqualsOrGetHashCode());
            }

            public class NewMethods
            {
                public virtual new bool Equals(object obj)
                {
                    return base.Equals(obj);
                }

                public virtual new int GetHashCode()
                {
                    return base.GetHashCode();
                }
            }

            [Fact]
            public void Check_returns_false_for_class_that_has_new_Equals_and_GetHashCode()
            {
                Assert.False(typeof(NewMethods).OverridesEqualsOrGetHashCode());
            }

            public class OverrideNewMethods : NewMethods
            {
                public override bool Equals(object obj)
                {
                    return base.Equals(obj);
                }

                public override int GetHashCode()
                {
                    return base.GetHashCode();
                }
            }

            [Fact]
            public void Check_returns_false_for_class_that_overrides_new_Equals_and_GetHashCode()
            {
                Assert.False(typeof(OverrideNewMethods).OverridesEqualsOrGetHashCode());
            }

            public class DifferentMethods : NewMethods
            {
                public bool Equals(DifferentMethods obj)
                {
                    return base.Equals(obj);
                }

                public int GetHashCode(int foo)
                {
                    return base.GetHashCode();
                }
            }

            [Fact]
            public void Check_returns_false_for_class_that_has_differnt_Equals_and_GetHashCode_signatures()
            {
                Assert.False(typeof(DifferentMethods).OverridesEqualsOrGetHashCode());
            }

#pragma warning disable 659
            public class OverridesEquals
            {
                public override bool Equals(object obj)
                {
                    return base.Equals(obj);
                }
            }
#pragma warning restore 659

            [Fact]
            public void Check_returns_true_for_class_that_overrides_Equals()
            {
                Assert.True(typeof(OverridesEquals).OverridesEqualsOrGetHashCode());
            }

            public class OverridesGetHashCode
            {
                public override int GetHashCode()
                {
                    return base.GetHashCode();
                }
            }

            [Fact]
            public void Check_returns_true_for_class_that_overrides_GetHashCode()
            {
                Assert.True(typeof(OverridesGetHashCode).OverridesEqualsOrGetHashCode());
            }

            public class HidesEquals : OverridesEquals
            {
                public new bool Equals(object obj)
                {
                    return base.Equals(obj);
                }
            }

            [Fact]
            public void Check_returns_true_for_class_that_overrides_Equals_but_then_hides_it()
            {
                Assert.True(typeof(HidesEquals).OverridesEqualsOrGetHashCode());
            }

            public class HidesGetHashCode : OverridesGetHashCode
            {
                public new int GetHashCode()
                {
                    return base.GetHashCode();
                }
            }

            [Fact]
            public void Check_returns_true_for_class_that_overrides_GetHashCode_but_then_hides_it()
            {
                Assert.True(typeof(HidesGetHashCode).OverridesEqualsOrGetHashCode());
            }
        }
    }
}

public class NoNamespaceClass
{
    public class Nested
    {
    }
}
