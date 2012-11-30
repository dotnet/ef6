// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Reflection;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class DelegateFactoryTests
    {
        public class CreateConstructor
        {
            [Fact]
            public void CreateConstructor_creates_constructor_delegates_for_reference_types_with_parameterless_contructors()
            {
                Assert.IsType<object>(DelegateFactory.CreateConstructor(typeof(object))());
                Assert.IsType<DbContext>(DelegateFactory.CreateConstructor(typeof(DbContext))());
                Assert.IsType<PrivateNestedClass>(DelegateFactory.CreateConstructor(typeof(PrivateNestedClass))());
                Assert.IsType<InternalClass>(DelegateFactory.CreateConstructor(typeof(InternalClass))());

                var anonType = new
                                   {
                                   }.GetType();
                Assert.IsType(anonType, DelegateFactory.CreateConstructor(anonType)());
            }

            [Fact]
            public void CreateConstructor_throws_for_value_types_or_types_with_no_parameterless_constructor()
            {
                Assert.Equal(
                    Strings.CodeGen_ConstructorNoParameterless(typeof(int).FullName),
                    Assert.Throws<InvalidOperationException>(() => DelegateFactory.CreateConstructor(typeof(int))).Message);

                Assert.Equal(
                    Strings.CodeGen_ConstructorNoParameterless(typeof(DateTime).FullName),
                    Assert.Throws<InvalidOperationException>(() => DelegateFactory.CreateConstructor(typeof(DateTime))).Message);
            }
        }

        public class CreateNavigationPropertySetter
        {
            [Fact]
            public void CreateNavigationPropertySetter_creates_a_setter_delegate_for_public_reference_property()
            {
                var target = new PublicClass2();
                var entity = new PublicClass1();

                DelegateFactory.CreateNavigationPropertySetter(
                    typeof(PublicClass1), typeof(PublicClass1).GetProperty("PublicNavProperty"))(entity, target);

                Assert.Same(target, entity.PublicNavProperty);
            }

            [Fact]
            public void CreateNavigationPropertySetter_creates_a_setter_delegate_for_public_reference_property_and_accepts_derived_types()
            {
                var target = new PublicClass2d();
                var entity = new PublicClass1();

                DelegateFactory.CreateNavigationPropertySetter(
                    typeof(PublicClass1), typeof(PublicClass1).GetProperty("PublicNavProperty"))(entity, target);

                Assert.Same(target, entity.PublicNavProperty);
            }

            [Fact]
            public void CreateNavigationPropertySetter_creates_a_setter_delegate_for_non_public_reference_property()
            {
                var target = new InternalClass();
                var entity = new PublicClass1();

                DelegateFactory.CreateNavigationPropertySetter(
                    typeof(PublicClass1),
                    typeof(PublicClass1).GetProperty("InternalNavProperty", BindingFlags.Instance | BindingFlags.NonPublic))(entity, target);

                Assert.Same(target, entity.InternalNavProperty);
            }

            [Fact]
            public void CreateNavigationPropertySetter_throws_for_static_value_type_and_properties_without_setters()
            {
                Assert.Equal(
                    Strings.CodeGen_PropertyIsStatic,
                    Assert.Throws<InvalidOperationException>(
                        () => DelegateFactory.CreateNavigationPropertySetter(
                            typeof(PublicClass1), typeof(PublicClass1).GetProperty("StaticProp"))).Message);

                Assert.Equal(
                    Strings.CodeGen_PropertyDeclaringTypeIsValueType,
                    Assert.Throws<InvalidOperationException>(
                        () => DelegateFactory.CreateNavigationPropertySetter(
                            typeof(ValueType), typeof(ValueType).GetProperty("ValueTypeProp"))).Message);

                Assert.Equal(
                    Strings.CodeGen_PropertyNoSetter,
                    Assert.Throws<InvalidOperationException>(
                        () => DelegateFactory.CreateNavigationPropertySetter(
                            typeof(PublicClass1), typeof(PublicClass1).GetProperty("NoSetterProp"))).Message);
            }
        }

        public class CreatePropertyGetter
        {
            [Fact]
            public void CreatePropertyGetter_throws_for_unsupported_property_types_and_properties_without_getters()
            {
                Assert.Equal(
                    Strings.CodeGen_PropertyNoGetter,
                    Assert.Throws<InvalidOperationException>(
                        () => DelegateFactory.CreatePropertyGetter(
                            typeof(PublicClass1), typeof(PublicClass1).GetProperty("NoGetterProp"))).Message);

                Assert.Equal(
                    Strings.CodeGen_PropertyIsStatic,
                    Assert.Throws<InvalidOperationException>(
                        () => DelegateFactory.CreatePropertyGetter(
                            typeof(PublicClass1), typeof(PublicClass1).GetProperty("StaticProp"))).Message);

                Assert.Equal(
                    Strings.CodeGen_PropertyDeclaringTypeIsValueType,
                    Assert.Throws<InvalidOperationException>(
                        () => DelegateFactory.CreatePropertyGetter(
                            typeof(ValueType), typeof(ValueType).GetProperty("ValueTypeProp"))).Message);

                Assert.Equal(
                    Strings.CodeGen_PropertyIsIndexed,
                    Assert.Throws<InvalidOperationException>(
                        () => DelegateFactory.CreatePropertyGetter(
                            typeof(PublicClass1), typeof(PublicClass1).GetProperties().First(p => p.GetIndexParameters().Any()))).Message);
            }

            [Fact]
            public void CreatePropertyGetter_throws_for_pointer_types()
            {
                var pointerType = new MockType();
                pointerType.Protected().Setup<bool>("IsPointerImpl").Returns(true);
                var property = new MockPropertyInfo(pointerType.Object, "PointerProp");
                var getter = new Mock<MethodInfo>();
                getter.Setup(m => m.ReturnType).Returns(pointerType.Object);
                getter.Setup(m => m.DeclaringType).Returns(new MockType().Object);
                property.Setup(m => m.GetGetMethod(true)).Returns(getter.Object);

                Assert.Equal(
                    Strings.CodeGen_PropertyUnsupportedType,
                    Assert.Throws<InvalidOperationException>(
                        () => DelegateFactory.CreatePropertyGetter(new MockType(), property.Object)).Message);
            }

            [Fact]
            public void CreatePropertyGetter_creates_getter_delegate_for_public_reference_type_property()
            {
                var target = new PublicClass2();

                Assert.Same(
                    target,
                    DelegateFactory.CreatePropertyGetter(
                        typeof(PublicClass1),
                        typeof(PublicClass1).GetProperty("PublicNavProperty"))(
                            new PublicClass1
                                {
                                    PublicNavProperty = target
                                }));
            }

            [Fact]
            public void CreatePropertyGetter_creates_getter_delegate_for_non_public_reference_type_property()
            {
                var target = new InternalClass();

                Assert.Same(
                    target,
                    DelegateFactory.CreatePropertyGetter(
                        typeof(PublicClass1),
                        typeof(PublicClass1).GetProperty("InternalNavProperty", BindingFlags.NonPublic | BindingFlags.Instance))(
                            new PublicClass1
                                {
                                    InternalNavProperty = target
                                }));
            }

            [Fact]
            public void CreatePropertyGetter_creates_getter_delegate_for_value_type_property()
            {
                Assert.Equal(
                    1,
                    DelegateFactory.CreatePropertyGetter(
                        typeof(PublicClass1),
                        typeof(PublicClass1).GetProperty("InternalValueTypeProperty", BindingFlags.NonPublic | BindingFlags.Instance))(
                            new PublicClass1
                                {
                                    InternalValueTypeProperty = 1
                                }));
            }

            [Fact]
            public void CreatePropertyGetter_creates_getter_delegate_for_Nullable_property_that_returns_the_value_if_it_exists()
            {
                Assert.Equal(
                    1,
                    DelegateFactory.CreatePropertyGetter(
                        typeof(PublicClass1),
                        typeof(PublicClass1).GetProperty("NullableProperty", BindingFlags.NonPublic | BindingFlags.Instance))(
                            new PublicClass1
                                {
                                    NullableProperty = 1
                                }));
            }

            [Fact]
            public void CreatePropertyGetter_creates_getter_delegate_for_Nullable_property_that_returns_null_if_the_Nullable_as_no_value()
            {
                Assert.Null(
                    DelegateFactory.CreatePropertyGetter(
                        typeof(PublicClass1),
                        typeof(PublicClass1).GetProperty("NullableProperty", BindingFlags.NonPublic | BindingFlags.Instance))(
                            new PublicClass1
                                {
                                    NullableProperty = null
                                }));
            }
        }

        public class CreatePropertySetter
        {
            [Fact]
            public void CreatePropertySetter_throws_for_unsupported_property_types_and_properties_without_setters()
            {
                Assert.Equal(
                    Strings.CodeGen_PropertyNoSetter,
                    Assert.Throws<InvalidOperationException>(
                        () => DelegateFactory.CreatePropertySetter(
                            typeof(PublicClass1), typeof(PublicClass1).GetProperty("NoSetterProp"), true)).Message);

                Assert.Equal(
                    Strings.CodeGen_PropertyIsStatic,
                    Assert.Throws<InvalidOperationException>(
                        () => DelegateFactory.CreatePropertySetter(
                            typeof(PublicClass1), typeof(PublicClass1).GetProperty("StaticProp"), true)).Message);

                Assert.Equal(
                    Strings.CodeGen_PropertyDeclaringTypeIsValueType,
                    Assert.Throws<InvalidOperationException>(
                        () => DelegateFactory.CreatePropertySetter(
                            typeof(ValueType), typeof(ValueType).GetProperty("ValueTypeProp"), true)).Message);

                Assert.Equal(
                    Strings.CodeGen_PropertyIsIndexed,
                    Assert.Throws<InvalidOperationException>(
                        () => DelegateFactory.CreatePropertySetter(
                            typeof(PublicClass1), typeof(PublicClass1).GetProperties().First(p => p.GetIndexParameters().Any()), true)).
                        Message);
            }

            [Fact]
            public void CreatePropertySetter_throws_for_pointer_types()
            {
                var pointerType = new MockType();
                pointerType.Protected().Setup<bool>("IsPointerImpl").Returns(true);
                var property = new MockPropertyInfo(pointerType.Object, "PointerProp");
                var setter = new Mock<MethodInfo>();
                setter.Setup(m => m.ReturnType).Returns(pointerType.Object);
                setter.Setup(m => m.DeclaringType).Returns(new MockType().Object);
                property.Setup(m => m.GetSetMethod(true)).Returns(setter.Object);

                Assert.Equal(
                    Strings.CodeGen_PropertyUnsupportedType,
                    Assert.Throws<InvalidOperationException>(
                        () => DelegateFactory.CreatePropertySetter(new MockType(), property.Object, true)).Message);
            }

            [Fact]
            public void CreatePropertySetter_creates_a_setter_delegate_for_a_non_public_property()
            {
                var target = new InternalClass();
                var entity = new PublicClass1();

                DelegateFactory.CreatePropertySetter(
                    typeof(PublicClass1),
                    typeof(PublicClass1).GetProperty("InternalNavProperty", BindingFlags.Instance | BindingFlags.NonPublic),
                    allowNull: true)(entity, target);

                Assert.Same(target, entity.InternalNavProperty);
            }

            [Fact]
            public void CreatePropertySetter_can_create_a_delegate_for_a_reference_type()
            {
                var target = new PublicClass2();
                var entity = new PublicClass1();

                DelegateFactory.CreatePropertySetter(
                    typeof(PublicClass1),
                    typeof(PublicClass1).GetProperty("PublicNavProperty"),
                    allowNull: true)(entity, target);

                Assert.Same(target, entity.PublicNavProperty);
            }

            [Fact]
            public void CreatePropertySetter_can_create_a_delegate_for_a_reference_type_that_allows_a_derived_type_to_be_set()
            {
                var target = new PublicClass2d();
                var entity = new PublicClass1();

                DelegateFactory.CreatePropertySetter(
                    typeof(PublicClass1),
                    typeof(PublicClass1).GetProperty("PublicNavProperty"),
                    allowNull: true)(entity, target);

                Assert.Same(target, entity.PublicNavProperty);
            }

            [Fact]
            public void CreatePropertySetter_can_create_a_delegate_for_a_reference_type_that_allows_null_to_be_set()
            {
                var entity = new PublicClass1();

                DelegateFactory.CreatePropertySetter(
                    typeof(PublicClass1),
                    typeof(PublicClass1).GetProperty("PublicNavProperty"),
                    allowNull: true)(entity, null);

                Assert.Null(entity.PublicNavProperty);
            }

            [Fact]
            public void CreatePropertySetter_can_create_a_delegate_for_a_reference_type_that_throws_if_null_is_set()
            {
                Assert.Equal(
                    Strings.Materializer_SetInvalidValue(
                        typeof(PublicClass2).Name,
                        typeof(PublicClass1).Name,
                        "PublicNavProperty",
                        "null"),
                    Assert.Throws<ConstraintException>(
                        () =>
                        DelegateFactory.CreatePropertySetter(
                            typeof(PublicClass1),
                            typeof(PublicClass1).GetProperty("PublicNavProperty"),
                            allowNull: false)(new PublicClass1(), null)).Message);
            }

            [Fact]
            public void CreatePropertySetter_can_create_a_delegate_for_a_reference_type_that_throws_if_value_is_of_wrong_type()
            {
                Assert.Equal(
                    Strings.Materializer_SetInvalidValue(
                        typeof(PublicClass2).Name,
                        typeof(PublicClass1).Name,
                        "PublicNavProperty",
                        typeof(InternalClass).Name),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        DelegateFactory.CreatePropertySetter(
                            typeof(PublicClass1),
                            typeof(PublicClass1).GetProperty("PublicNavProperty"),
                            allowNull: true)(new PublicClass1(), new InternalClass())).Message);
            }

            [Fact]
            public void CreatePropertySetter_can_create_a_delegate_for_a_value_type()
            {
                var entity = new PublicClass1();

                DelegateFactory.CreatePropertySetter(
                    typeof(PublicClass1),
                    typeof(PublicClass1).GetProperty("InternalValueTypeProperty", BindingFlags.NonPublic | BindingFlags.Instance),
                    allowNull: false)(entity, 7);

                Assert.Equal(7, entity.InternalValueTypeProperty);
            }

            [Fact]
            public void CreatePropertySetter_can_create_a_delegate_for_a_value_type_that_throws_if_null_is_set()
            {
                Assert.Equal(
                    Strings.Materializer_SetInvalidValue(
                        typeof(int).Name,
                        typeof(PublicClass1).Name,
                        "InternalValueTypeProperty",
                        "null"),
                    Assert.Throws<ConstraintException>(
                        () =>
                        DelegateFactory.CreatePropertySetter(
                            typeof(PublicClass1),
                            typeof(PublicClass1).GetProperty("InternalValueTypeProperty", BindingFlags.NonPublic | BindingFlags.Instance),
                            allowNull: false)(new PublicClass1(), null)).Message);
            }

            [Fact]
            public void CreatePropertySetter_can_create_a_delegate_for_a_value_type_that_throws_if_null_is_set_even_if_allowNull_is_true()
            {
                Assert.Equal(
                    Strings.Materializer_SetInvalidValue(
                        typeof(int).Name,
                        typeof(PublicClass1).Name,
                        "InternalValueTypeProperty",
                        "null"),
                    Assert.Throws<ConstraintException>(
                        () =>
                        DelegateFactory.CreatePropertySetter(
                            typeof(PublicClass1),
                            typeof(PublicClass1).GetProperty("InternalValueTypeProperty", BindingFlags.NonPublic | BindingFlags.Instance),
                            allowNull: true)(new PublicClass1(), null)).Message);
            }

            [Fact]
            public void CreatePropertySetter_can_create_a_delegate_for_a_value_type_that_throws_if_value_is_of_wrong_type()
            {
                Assert.Equal(
                    Strings.Materializer_SetInvalidValue(
                        typeof(int).Name,
                        typeof(PublicClass1).Name,
                        "InternalValueTypeProperty",
                        typeof(DateTime).Name),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        DelegateFactory.CreatePropertySetter(
                            typeof(PublicClass1),
                            typeof(PublicClass1).GetProperty("InternalValueTypeProperty", BindingFlags.NonPublic | BindingFlags.Instance),
                            allowNull: false)(new PublicClass1(), new DateTime())).Message);
            }

            [Fact]
            public void CreatePropertySetter_can_create_a_delegate_for_a_nullable_type()
            {
                var entity = new PublicClass1();

                DelegateFactory.CreatePropertySetter(
                    typeof(PublicClass1),
                    typeof(PublicClass1).GetProperty("NullableProperty", BindingFlags.NonPublic | BindingFlags.Instance),
                    allowNull: true)(entity, 7);

                Assert.Equal(7, entity.NullableProperty);
            }

            [Fact]
            public void CreatePropertySetter_can_create_a_delegate_for_a_nullable_type_that_allows_null_to_be_set()
            {
                var entity = new PublicClass1();

                DelegateFactory.CreatePropertySetter(
                    typeof(PublicClass1),
                    typeof(PublicClass1).GetProperty("NullableProperty", BindingFlags.NonPublic | BindingFlags.Instance),
                    allowNull: true)(entity, null);

                Assert.Null(entity.NullableProperty);
            }

            [Fact]
            public void CreatePropertySetter_can_create_a_delegate_for_a_nullable_type_that_throws_if_null_is_set()
            {
                Assert.Equal(
                    Strings.Materializer_SetInvalidValue(
                        typeof(int).Name,
                        typeof(PublicClass1).Name,
                        "NullableProperty",
                        "null"),
                    Assert.Throws<ConstraintException>(
                        () =>
                        DelegateFactory.CreatePropertySetter(
                            typeof(PublicClass1),
                            typeof(PublicClass1).GetProperty("NullableProperty", BindingFlags.NonPublic | BindingFlags.Instance),
                            allowNull: false)(new PublicClass1(), null)).Message);
            }

            [Fact]
            public void CreatePropertySetter_can_create_a_delegate_for_a_nullable_type_that_throws_if_value_is_of_wrong_type()
            {
                Assert.Equal(
                    Strings.Materializer_SetInvalidValue(
                        typeof(int).Name,
                        typeof(PublicClass1).Name,
                        "NullableProperty",
                        typeof(DateTime).Name),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        DelegateFactory.CreatePropertySetter(
                            typeof(PublicClass1),
                            typeof(PublicClass1).GetProperty("NullableProperty", BindingFlags.NonPublic | BindingFlags.Instance),
                            allowNull: true)(new PublicClass1(), new DateTime())).Message);
            }
        }

        private class PrivateNestedClass
        {
        }
    }

    public class PublicClass1
    {
        public PublicClass2 PublicNavProperty { get; set; }
        internal InternalClass InternalNavProperty { get; set; }
        public static PublicClass2 StaticProp { get; set; }
        internal int InternalValueTypeProperty { get; set; }
        internal int? NullableProperty { get; set; }

        public PublicClass2 NoSetterProp
        {
            get { throw new NotImplementedException(); }
        }

        public PublicClass2 NoGetterProp
        {
            set { throw new NotImplementedException(); }
        }

        public PublicClass2 this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }

    public class PublicClass2
    {
    }

    public class PublicClass2d : PublicClass2
    {
    }

    internal class InternalClass
    {
    }

    public struct ValueType
    {
        public PublicClass2 ValueTypeProp { get; set; }
    }
}
