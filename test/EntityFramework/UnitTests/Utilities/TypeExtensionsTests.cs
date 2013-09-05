// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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

        public class IsPublic
        {
            [Fact]
            public void Returns_true_for_normal_public_types_and_false_for_internal_types()
            {
                Assert.True(typeof(NormalPublicClass).IsPublic());
                Assert.False(typeof(NormalInternalClass).IsPublic());
            }

            [Fact]
            public void Returns_true_for_public_types_nested_to_any_level_in_public_types()
            {
                Assert.True(typeof(NormalPublicClass.NestedPublicClass).IsPublic());
                Assert.True(typeof(NormalPublicClass.NestedPublicClass.DoubleNestedPublicClass).IsPublic());
            }

            [Fact]
            public void Returns_false_for_internal_or_pseudo_public_types_nested_at_some_level_in_an_internal_type()
            {
                Assert.False(typeof(NormalInternalClass.NestedPseudoPublicClass).IsPublic());
                Assert.False(typeof(NormalInternalClass.NestedPseudoPublicClass.DoubleNestedPseudoPublicClass).IsPublic());
                Assert.False(typeof(NormalInternalClass.NestedPseudoPublicClass.NestedInternalClass).IsPublic());
            }
        }

        public class GetMethodInfo
        {
            private static readonly Type[] _params1 = new[] { typeof(Random) };
            private static readonly Type[] _params2 = new[] { typeof(Random), typeof(int) };
            private static readonly Type[] _params3 = new[] { typeof(int), typeof(Random) };
            private static readonly object[] _args1 = new object[] { new Random() };
            private static readonly object[] _args2 = new object[] { new Random(), 1 };
            private static readonly object[] _args3 = new object[] { 1, new Random() };

            [Fact]
            public void Types_method_finds_public_static_method_only_with_matching_parameters()
            {
                Assert.Equal(1, typeof(Queen).GetDeclaredMethod("Brian", Type.EmptyTypes).Invoke(null, null));
                Assert.Equal(2, typeof(Queen).GetDeclaredMethod("Brian", _params1).Invoke(null, _args1));
                Assert.Equal(3, typeof(Queen).GetDeclaredMethod("Brian", _params2).Invoke(null, _args2));
                Assert.Equal(4, typeof(Queen).GetDeclaredMethod("Brian", _params3).Invoke(null, _args3));
            }

            [Fact]
            public void Types_method_finds_non_public_static_method_only_with_matching_parameters()
            {
                Assert.Equal(5, typeof(Queen).GetDeclaredMethod("Freddie", Type.EmptyTypes).Invoke(null, null));
                Assert.Equal(6, typeof(Queen).GetDeclaredMethod("Freddie", _params1).Invoke(null, _args1));
                Assert.Equal(7, typeof(Queen).GetDeclaredMethod("Freddie", _params2).Invoke(null, _args2));
                Assert.Equal(8, typeof(Queen).GetDeclaredMethod("Freddie", _params3).Invoke(null, _args3));
            }

            [Fact]
            public void Types_method_finds_public_instance_method_only_with_matching_parameters()
            {
                Assert.Equal(9, typeof(Queen).GetDeclaredMethod("John", Type.EmptyTypes).Invoke(new Queen(), null));
                Assert.Equal(10, typeof(Queen).GetDeclaredMethod("John", _params1).Invoke(new Queen(), _args1));
                Assert.Equal(11, typeof(Queen).GetDeclaredMethod("John", _params2).Invoke(new Queen(), _args2));
                Assert.Equal(12, typeof(Queen).GetDeclaredMethod("John", _params3).Invoke(new Queen(), _args3));
            }

            [Fact]
            public void Types_method_finds_non_public_instance_method_only_with_matching_parameters()
            {
                Assert.Equal(13, typeof(Queen).GetDeclaredMethod("Roger", Type.EmptyTypes).Invoke(new Queen(), null));
                Assert.Equal(14, typeof(Queen).GetDeclaredMethod("Roger", _params1).Invoke(new Queen(), _args1));
                Assert.Equal(15, typeof(Queen).GetDeclaredMethod("Roger", _params2).Invoke(new Queen(), _args2));
                Assert.Equal(16, typeof(Queen).GetDeclaredMethod("Roger", _params3).Invoke(new Queen(), _args3));
            }

            [Fact]
            public void Types_method_returns_null_for_method_that_is_not_found()
            {
                Assert.Null(typeof(Queen).GetDeclaredMethod("Brian", new[] { typeof(int) }));
            }

            public class Queen
            {
                public static int Brian()
                {
                    return 1;
                }

                public static int Brian(Random may)
                {
                    return 2;
                }

                public static int Brian(Random harold, int may)
                {
                    return 3;
                }

                public static int Brian(int harold, Random may)
                {
                    return 4;
                }

                private static int Freddie()
                {
                    return 5;
                }

                private static int Freddie(Random mercury)
                {
                    return 6;
                }

                private static int Freddie(Random bulsara, int mercury)
                {
                    return 7;
                }

                private static int Freddie(int bulsara, Random mercury)
                {
                    return 8;
                }

                public int John()
                {
                    return 9;
                }

                public int John(Random deacon)
                {
                    return 10;
                }

                public int John(Random richard, int deacon)
                {
                    return 11;
                }

                public int John(int richard, Random deacon)
                {
                    return 12;
                }

                private int Roger()
                {
                    return 13;
                }

                private int Roger(Random taylor)
                {
                    return 14;
                }

                private int Roger(Random meddows, int taylor)
                {
                    return 15;
                }

                private int Roger(int meddows, Random taylor)
                {
                    return 16;
                }
            }

            [Fact]
            public void Name_only_method_finds_only_public_static_method_with_name()
            {
                Assert.Equal(1, typeof(Beatles).GetDeclaredMethod("George").Invoke(null, null));
            }

            [Fact]
            public void Name_only_method_finds_only_non_public_static_method_with_name()
            {
                Assert.Equal(2, typeof(Beatles).GetDeclaredMethod("Ringo").Invoke(null, _args1));
            }

            [Fact]
            public void Name_only_method_finds_only_public_instance_method_with_name()
            {
                Assert.Equal(3, typeof(Beatles).GetDeclaredMethod("John").Invoke(new Beatles(), _args2));
            }

            [Fact]
            public void Name_only_method_finds_only_non_public_instance_method_with_name()
            {
                Assert.Equal(4, typeof(Beatles).GetDeclaredMethod("James").Invoke(new Beatles(), _args3));
            }

            [Fact]
            public void Name_only_method_returns_null_for_method_that_is_not_found()
            {
                Assert.Null(typeof(Beatles).GetDeclaredMethod("Pete"));
            }

            public class Beatles
            {
                public static int George()
                {
                    return 1;
                }

                private static int Ringo(Random starr)
                {
                    return 2;
                }

                public int John(Random winston, int lennon)
                {
                    return 3;
                }

                private int James(int paul, Random mcCartney)
                {
                    return 4;
                }
            }

            [Fact]
            public void Public_instance_only_method_does_not_find_public_static_methods()
            {
                Assert.Null(typeof(Queen).GetPublicInstanceMethod("Brian", Type.EmptyTypes));
                Assert.Null(typeof(Queen).GetPublicInstanceMethod("Brian", _params1));
                Assert.Null(typeof(Queen).GetPublicInstanceMethod("Brian", _params2));
                Assert.Null(typeof(Queen).GetPublicInstanceMethod("Brian", _params3));
            }

            [Fact]
            public void Public_instance_only_method_does_not_find_non_public_static_methods()
            {
                Assert.Null(typeof(Queen).GetPublicInstanceMethod("Freddie", Type.EmptyTypes));
                Assert.Null(typeof(Queen).GetPublicInstanceMethod("Freddie", _params1));
                Assert.Null(typeof(Queen).GetPublicInstanceMethod("Freddie", _params2));
                Assert.Null(typeof(Queen).GetPublicInstanceMethod("Freddie", _params3));
            }

            [Fact]
            public void Public_instance_only_method_finds_public_instance_method_only_with_matching_parameters()
            {
                Assert.Equal(9, typeof(Queen).GetPublicInstanceMethod("John", Type.EmptyTypes).Invoke(new Queen(), null));
                Assert.Equal(10, typeof(Queen).GetPublicInstanceMethod("John", _params1).Invoke(new Queen(), _args1));
                Assert.Equal(11, typeof(Queen).GetPublicInstanceMethod("John", _params2).Invoke(new Queen(), _args2));
                Assert.Equal(12, typeof(Queen).GetPublicInstanceMethod("John", _params3).Invoke(new Queen(), _args3));
            }

            [Fact]
            public void Public_instance_only_method_does_not_finds_public_instance_methods()
            {
                Assert.Null(typeof(Queen).GetPublicInstanceMethod("Roger", Type.EmptyTypes));
                Assert.Null(typeof(Queen).GetPublicInstanceMethod("Roger", _params1));
                Assert.Null(typeof(Queen).GetPublicInstanceMethod("Roger", _params2));
                Assert.Null(typeof(Queen).GetPublicInstanceMethod("Roger", _params3));
            }

            [Fact]
            public void Public_instance_only_method_handles_inherited_overridden_and_new_methods()
            {
                Assert.Equal(1, typeof(Deep).GetPublicInstanceMethod("Gillan", Type.EmptyTypes).Invoke(new Deep(), null));
                Assert.Equal(2, typeof(Deep).GetPublicInstanceMethod("Paice", Type.EmptyTypes).Invoke(new Deep(), null));
                Assert.Equal(4, typeof(Deep).GetPublicInstanceMethod("Blackmore", Type.EmptyTypes).Invoke(new Deep(), null));
                Assert.Equal(7, typeof(Deep).GetPublicInstanceMethod("Lord", Type.EmptyTypes).Invoke(new Deep(), null));
            }

            [Fact]
            public void Types_method_only_returns_declared_methods()
            {
                Assert.Equal(1, typeof(Deep).GetDeclaredMethod("Gillan", Type.EmptyTypes).Invoke(new Deep(), null));
                Assert.Equal(2, typeof(Deep).GetDeclaredMethod("Paice", Type.EmptyTypes).Invoke(new Deep(), null));
                Assert.Equal(3, typeof(Deep).GetDeclaredMethod("Glover", Type.EmptyTypes).Invoke(new Deep(), null));
                Assert.Null(typeof(Deep).GetDeclaredMethod("Blackmore", Type.EmptyTypes));
                Assert.Null(typeof(Deep).GetDeclaredMethod("Lord", Type.EmptyTypes));

                Assert.Equal(1, typeof(Purple).GetDeclaredMethod("Gillan", Type.EmptyTypes).Invoke(new Deep(), null));
                Assert.Equal(6, typeof(Purple).GetDeclaredMethod("Paice", Type.EmptyTypes).Invoke(new Deep(), null));
                Assert.Equal(8, typeof(Purple).GetDeclaredMethod("Glover", Type.EmptyTypes).Invoke(new Deep(), null));
                Assert.Equal(4, typeof(Purple).GetDeclaredMethod("Blackmore", Type.EmptyTypes).Invoke(new Deep(), null));
                Assert.Equal(7, typeof(Purple).GetDeclaredMethod("Lord", Type.EmptyTypes).Invoke(new Deep(), null));

                Assert.Equal(5, typeof(Purple).GetDeclaredMethod("Gillan", Type.EmptyTypes).Invoke(new Purple(), null));
                Assert.Equal(6, typeof(Purple).GetDeclaredMethod("Paice", Type.EmptyTypes).Invoke(new Purple(), null));
                Assert.Equal(8, typeof(Purple).GetDeclaredMethod("Glover", Type.EmptyTypes).Invoke(new Purple(), null));
                Assert.Equal(4, typeof(Purple).GetDeclaredMethod("Blackmore", Type.EmptyTypes).Invoke(new Purple(), null));
                Assert.Equal(7, typeof(Purple).GetDeclaredMethod("Lord", Type.EmptyTypes).Invoke(new Purple(), null));
            }

            [Fact]
            public void Name_only_method_only_returns_declared_methods()
            {
                Assert.Equal(1, typeof(Deep).GetDeclaredMethod("Gillan").Invoke(new Deep(), null));
                Assert.Equal(2, typeof(Deep).GetDeclaredMethod("Paice").Invoke(new Deep(), null));
                Assert.Equal(3, typeof(Deep).GetDeclaredMethod("Glover").Invoke(new Deep(), null));
                Assert.Null(typeof(Deep).GetDeclaredMethod("Blackmore"));
                Assert.Null(typeof(Deep).GetDeclaredMethod("Lord"));

                Assert.Equal(1, typeof(Purple).GetDeclaredMethod("Gillan").Invoke(new Deep(), null));
                Assert.Equal(6, typeof(Purple).GetDeclaredMethod("Paice").Invoke(new Deep(), null));
                Assert.Equal(8, typeof(Purple).GetDeclaredMethod("Glover").Invoke(new Deep(), null));
                Assert.Equal(4, typeof(Purple).GetDeclaredMethod("Blackmore").Invoke(new Deep(), null));
                Assert.Equal(7, typeof(Purple).GetDeclaredMethod("Lord").Invoke(new Deep(), null));

                Assert.Equal(5, typeof(Purple).GetDeclaredMethod("Gillan").Invoke(new Purple(), null));
                Assert.Equal(6, typeof(Purple).GetDeclaredMethod("Paice").Invoke(new Purple(), null));
                Assert.Equal(8, typeof(Purple).GetDeclaredMethod("Glover").Invoke(new Purple(), null));
                Assert.Equal(4, typeof(Purple).GetDeclaredMethod("Blackmore").Invoke(new Purple(), null));
                Assert.Equal(7, typeof(Purple).GetDeclaredMethod("Lord").Invoke(new Purple(), null));
            }

            public class Deep : Purple
            {
                public override int Gillan()
                {
                    return 1;
                }

                public new int Paice()
                {
                    return 2;
                }

                private int Glover()
                {
                    return 3;
                }
            }

            public class Purple
            {
                public virtual int Blackmore()
                {
                    return 4;
                }

                public virtual int Gillan()
                {
                    return 5;
                }

                public virtual int Paice()
                {
                    return 6;
                }

                public int Lord()
                {
                    return 7;
                }

                private int Glover()
                {
                    return 8;
                }
            }
        }
    }

    public class NormalPublicClass
    {
        public class NestedPublicClass
        {
            public class DoubleNestedPublicClass
            {
            }
        }
    }

    internal class NormalInternalClass
    {
        public class NestedPseudoPublicClass
        {
            public class DoubleNestedPseudoPublicClass
            {
            }

            internal class NestedInternalClass
            {
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
