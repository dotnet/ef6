// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiUnitTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Reflection;
    using Moq;
    using Xunit;

    /// <summary>
    ///     General unit tests for DbPropertyValues and related classes/methods.
    ///     Some specific features, such as concurrency, are tested elsewhere.
    /// </summary>
    public class DbPropertyValuesTests : TestBase
    {
        #region Helper classes

        private const BindingFlags PropertyBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        /// <summary>
        ///     A type with a variety of different types of properties used for the tests in this class.
        /// </summary>
        public class FakeTypeWithProps
        {
            public FakeTypeWithProps()
            {
                PublicNonNullStringProp = "NonNullValue";
            }

            public FakeTypeWithProps(
                int id, string publicStringProp, string privateStringProp,
                int publicIntProp, int privateIntProp,
                int publicIntPropWithPrivateSetter, int publicIntPropWithPrivateGetter,
                byte[] publicBinaryProp)
                : this()
            {
                Id = id;
                PublicStringProp = publicStringProp;
                PrivateStringProp = privateStringProp;
                PublicIntProp = publicIntProp;
                PrivateIntProp = privateIntProp;
                PublicIntPropWithPrivateSetter = publicIntPropWithPrivateSetter;
                PublicIntPropWithPrivateGetter = publicIntPropWithPrivateGetter;
                PublicBinaryProp = publicBinaryProp;
            }

            public virtual int Id { get; set; }

            public virtual string PublicStringProp { get; set; }
            public virtual string PublicNonNullStringProp { get; set; }
            private string PrivateStringProp { get; set; }

            public virtual string PublicReadonlyStringProp
            {
                get { return "PublicReadonlyStringProp"; }
            }

            private string PrivateReadonlyStringProp
            {
                get { return "PrivateReadonlyStringProp"; }
            }

            public virtual string PublicWriteOnlyStringProp
            {
                set { }
            }

            private string PrivateWriteOnlyStringProp
            {
                set { }
            }

            public virtual int PublicIntProp { get; set; }
            private int PrivateIntProp { get; set; }

            public virtual int PublicIntPropWithPrivateSetter { get; private set; }
            public virtual int PublicIntPropWithPrivateGetter { private get; set; }

            public virtual byte[] PublicBinaryProp { get; set; }

            public virtual FakeTypeWithProps NestedObject { get; set; }
        }

        public class FakeDerivedTypeWithProps : FakeTypeWithProps
        {
        }

        #endregion

        #region Tests for copying values to an object

        [Fact]
        public void Non_Generic_DbPropertyValues_uses_ToObject_on_InternalPropertyValues()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 1 }
                                 };
            var values = new DbPropertyValues(new TestInternalPropertyValues<FakeTypeWithProps>(properties));

            var clone = (FakeTypeWithProps)values.ToObject();

            Assert.Equal(1, clone.Id);
        }

        [Fact]
        public void ToObject_for_entity_uses_CreateObject_to_return_instance_of_correct_type()
        {
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(null, isEntityValues: true);
            values.MockInternalContext.Setup(c => c.CreateObject(typeof(FakeTypeWithProps))).Returns(new FakeDerivedTypeWithProps());

            var clone = values.ToObject();

            Assert.IsType<FakeDerivedTypeWithProps>(clone);
        }

        [Fact]
        public void ToObject_for_complex_object_does_not_use_CreateObject_to_create_instance()
        {
            var values = new TestInternalPropertyValues<FakeTypeWithProps>();

            var clone = values.ToObject();

            values.MockInternalContext.Verify(c => c.CreateObject(typeof(FakeTypeWithProps)), Times.Never());
            Assert.IsType<FakeTypeWithProps>(clone);
        }

        [Fact]
        public void ToObject_sets_all_properties_from_the_dictionary_onto_the_object_including_those_with_private_setters()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 1 },
                                     { "PublicStringProp", "PublicStringPropValue" },
                                     { "PrivateStringProp", "PrivateStringPropValue" },
                                     { "PublicIntProp", 2 },
                                     { "PrivateIntProp", 3 },
                                     { "PublicIntPropWithPrivateSetter", 4 },
                                     { "PublicIntPropWithPrivateGetter", 5 },
                                     { "PublicBinaryProp", new byte[] { 3, 1, 4, 1, 5, 9 } },
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties);

            var clone = (FakeTypeWithProps)values.ToObject();

            Assert.Equal(1, clone.Id);
            Assert.Equal("PublicStringPropValue", clone.PublicStringProp);
            Assert.Equal(2, clone.PublicIntProp);
            Assert.Equal(
                "PrivateStringPropValue",
                typeof(FakeTypeWithProps).GetProperty("PrivateStringProp", PropertyBindingFlags).GetValue(clone, null));
            Assert.Equal(3, typeof(FakeTypeWithProps).GetProperty("PrivateIntProp", PropertyBindingFlags).GetValue(clone, null));
            Assert.Equal(4, clone.PublicIntPropWithPrivateSetter);
            Assert.Equal(
                5, typeof(FakeTypeWithProps).GetProperty("PublicIntPropWithPrivateGetter", PropertyBindingFlags).GetValue(clone, null));
            Assert.True(DbHelpers.KeyValuesEqual(new byte[] { 3, 1, 4, 1, 5, 9 }, clone.PublicBinaryProp));
        }

        [Fact]
        public void ToObject_ignores_properties_that_are_conceptually_in_shadow_state()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 1 },
                                     { "MissingProp", "MissingPropValue" }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties);

            var clone = (FakeTypeWithProps)values.ToObject();

            Assert.Equal(1, clone.Id);
        }

        [Fact]
        public void ToObject_ignores_properties_that_are_readonly()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 1 },
                                     { "PublicReadonlyStringProp", "Foo" },
                                     { "PrivateReadonlyStringProp", "Foo" }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties);

            var clone = (FakeTypeWithProps)values.ToObject();

            Assert.Equal(1, clone.Id);
            Assert.Equal("PublicReadonlyStringProp", clone.PublicReadonlyStringProp);
            Assert.Equal(
                "PrivateReadonlyStringProp",
                typeof(FakeTypeWithProps).GetProperty("PrivateReadonlyStringProp", PropertyBindingFlags).GetValue(clone, null));
        }

        [Fact]
        public void ToObject_can_set_reference_propeties_to_null()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "PublicNonNullStringProp", null }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties);

            var clone = (FakeTypeWithProps)values.ToObject();

            Assert.Null(clone.PublicNonNullStringProp);
        }

        [Fact]
        public void ToObject_returns_nested_property_dictionary_as_cloned_object()
        {
            var nestedProperties = new Dictionary<string, object>
                                       {
                                           { "Id", 1 }
                                       };
            var nestedValues = new TestInternalPropertyValues<FakeTypeWithProps>(nestedProperties);

            var properties = new Dictionary<string, object>
                                 {
                                     { "NestedObject", nestedValues }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties, new[] { "NestedObject" });

            var clone = (FakeTypeWithProps)values.ToObject();

            Assert.Equal(1, clone.NestedObject.Id);
        }

        [Fact]
        public void ToObject_ignores_null_nested_property_dictionary()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 1 },
                                     { "NestedObject", null }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties, new[] { "NestedObject" });

            var clone = (FakeTypeWithProps)values.ToObject();

            Assert.Equal(1, clone.Id);
            Assert.Null(clone.NestedObject);
        }

        [Fact]
        public void ToObject_throws_when_trying_to_set_null_values_onto_non_nullable_properties()
        {
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(
                new Dictionary<string, object>
                    {
                        { "Id", 1 }
                    });
            values["Id"] = null;

            Assert.Equal(
                Strings.DbPropertyValues_CannotSetNullValue("Id", "Int32", "FakeTypeWithProps"),
                Assert.Throws<InvalidOperationException>(() => values.ToObject()).Message);
        }

        #endregion

        #region Tests for setting values from an object

        [Fact]
        public void Attempt_to_copy_values_from_null_object_throws()
        {
            var values = new DbPropertyValues(new TestInternalPropertyValues<FakeTypeWithProps>());

            Assert.Equal("obj", Assert.Throws<ArgumentNullException>(() => values.SetValues((object)null)).ParamName);
        }

        [Fact]
        public void SetValues_copies_values_from_object_to_property_dictionary()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 0 },
                                     { "PublicStringProp", null },
                                     { "PrivateStringProp", null },
                                     { "PublicIntProp", 0 },
                                     { "PrivateIntProp", 0 },
                                     { "PublicIntPropWithPrivateSetter", 0 },
                                     { "PublicIntPropWithPrivateGetter", 0 },
                                     { "PublicBinaryProp", null },
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties);

            var obj = new FakeTypeWithProps(
                1, "PublicStringPropValue", "PrivateStringPropValue", 2, 3, 4, 5, new byte[] { 3, 1, 4, 1, 5, 9 });

            values.SetValues(obj);

            Assert.Equal(1, values["Id"]);
            Assert.Equal("PublicStringPropValue", values["PublicStringProp"]);
            Assert.Equal(2, values["PublicIntProp"]);
            Assert.Equal("PrivateStringPropValue", values["PrivateStringProp"]);
            Assert.Equal(3, values["PrivateIntProp"]);
            Assert.Equal(4, values["PublicIntPropWithPrivateSetter"]);
            Assert.Equal(5, values["PublicIntPropWithPrivateGetter"]);
            Assert.True(DbHelpers.KeyValuesEqual(new byte[] { 3, 1, 4, 1, 5, 9 }, values["PublicBinaryProp"]));
        }

        [Fact]
        public void Attempt_to_copy_values_from_object_of_differnt_type_copies_no_properties_if_no_properties_match()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "PublicStringProp", null },
                                     { "PrivateStringProp", null },
                                     { "PublicIntProp", 0 },
                                     { "PrivateIntProp", 0 },
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties);

            values.SetValues("Bang!");

            Assert.Null(values["PublicStringProp"]);
            Assert.Equal(0, values["PublicIntProp"]);
            Assert.Null(values["PrivateStringProp"]);
            Assert.Equal(0, values["PrivateIntProp"]);
        }

        public class FakeTypeWithSomeProps
        {
            public FakeTypeWithSomeProps(string publicStringProp, int privateIntProp, int someOtherIntProp)
            {
                PublicStringProp = publicStringProp;
                PrivateIntProp = privateIntProp;
                SomeOtherIntProp = someOtherIntProp;
            }

            public string PublicStringProp { get; set; }
            private int PrivateIntProp { get; set; }
            public int SomeOtherIntProp { get; set; }
        }

        [Fact]
        public void Attempt_to_copy_values_from_object_of_differnt_type_copies_only_properties_that_match()
        {
            Attempt_to_copy_values_from_object_of_differnt_type_copies_only_properties_that_match_implementation(
                new FakeTypeWithSomeProps("PublicStringPropValue", 3, 4));
        }

        [Fact]
        public void Attempt_to_copy_values_from_anonymous_object_copies_only_properties_that_match()
        {
            var obj = new
                          {
                              PublicStringProp = "PublicStringPropValue",
                              PrivateIntProp = 3,
                              SomeDifferentIntProp = 4
                          };
            Attempt_to_copy_values_from_object_of_differnt_type_copies_only_properties_that_match_implementation(obj);
        }

        private void Attempt_to_copy_values_from_object_of_differnt_type_copies_only_properties_that_match_implementation(object obj)
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 0 },
                                     { "PublicStringProp", null },
                                     { "PrivateStringProp", null },
                                     { "PublicIntProp", 0 },
                                     { "PrivateIntProp", 0 },
                                     { "PublicIntPropWithPrivateSetter", 0 },
                                     { "PublicIntPropWithPrivateGetter", 0 },
                                     { "PublicBinaryProp", null },
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties);

            values.SetValues(obj);

            Assert.Equal(0, values["Id"]);
            Assert.Equal("PublicStringPropValue", values["PublicStringProp"]);
            Assert.Equal(0, values["PublicIntProp"]);
            Assert.Null(values["PrivateStringProp"]);
            Assert.Equal(3, values["PrivateIntProp"]);
            Assert.Equal(0, values["PublicIntPropWithPrivateSetter"]);
            Assert.Equal(0, values["PublicIntPropWithPrivateGetter"]);
            Assert.Null(values["PublicBinaryProp"]);
        }

        [Fact]
        public void Non_Generic_DbPropertyValues_SetValues_works_on_the_underlying_dictionary()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 0 }
                                 };
            var values = new DbPropertyValues(new TestInternalPropertyValues<FakeTypeWithProps>(properties));

            values.SetValues(
                new FakeTypeWithProps
                    {
                        Id = 1
                    });

            Assert.Equal(1, values["Id"]);
        }

        [Fact]
        public void Calling_SetValues_with_instance_of_derived_type_works()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 0 }
                                 };
            var values = new DbPropertyValues(new TestInternalPropertyValues<FakeTypeWithProps>(properties));

            values.SetValues(
                new FakeDerivedTypeWithProps
                    {
                        Id = 1
                    });

            Assert.Equal(1, values["Id"]);
        }

        [Fact]
        public void SetValues_ignores_properties_that_are_conceptually_in_shadow_state()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 0 },
                                     { "MissingProp", "MissingPropValue" }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties);

            values.SetValues(
                new FakeTypeWithProps
                    {
                        Id = 1
                    });

            Assert.Equal(1, values["Id"]);
            Assert.Equal("MissingPropValue", values["MissingProp"]);
        }

        [Fact]
        public void SetValues_ignores_properties_that_are_write_only()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 0 },
                                     { "PublicWriteOnlyStringProp", "Foo" },
                                     { "PrivateWriteOnlyStringProp", "Bar" }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties);

            values.SetValues(
                new FakeTypeWithProps
                    {
                        Id = 1
                    });

            Assert.Equal(1, values["Id"]);
            Assert.Equal("Foo", values["PublicWriteOnlyStringProp"]);
            Assert.Equal("Bar", values["PrivateWriteOnlyStringProp"]);
        }

        [Fact]
        public void SetValues_can_set_reference_propeties_to_null()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 0 },
                                     { "PublicStringProp", "NonNull" }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties);

            values.SetValues(
                new FakeTypeWithProps
                    {
                        Id = 1
                    });

            Assert.Equal(1, values["Id"]);
            Assert.Null(values["PublicStringProp"]);
        }

        [Fact]
        public void SetValues_sets_values_from_complex_object_into_nested_property_dictionary()
        {
            var nestedProperties = new Dictionary<string, object>
                                       {
                                           { "Id", 0 }
                                       };
            var nestedValues = new TestInternalPropertyValues<FakeTypeWithProps>(nestedProperties);

            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 0 },
                                     { "NestedObject", nestedValues }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties, new[] { "NestedObject" });

            values.SetValues(
                new FakeTypeWithProps
                    {
                        Id = 1,
                        NestedObject = new FakeTypeWithProps
                                           {
                                               Id = 2
                                           }
                    });

            Assert.Equal(1, values["Id"]);
            Assert.Equal(2, nestedValues["Id"]);
        }

        [Fact]
        public void SetValues_when_complex_object_is_null_throws()
        {
            var nestedProperties = new Dictionary<string, object>
                                       {
                                           { "Id", 0 }
                                       };
            var nestedValues = new TestInternalPropertyValues<FakeTypeWithProps>(nestedProperties);

            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 0 },
                                     { "NestedObject", nestedValues }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties, new[] { "NestedObject" });

            var obj = new FakeTypeWithProps
                          {
                              Id = 1,
                              NestedObject = null
                          };

            Assert.Equal(
                Strings.DbPropertyValues_ComplexObjectCannotBeNull("NestedObject", typeof(FakeTypeWithProps).Name),
                Assert.Throws<InvalidOperationException>(() => values.SetValues(obj)).Message);
        }

        [Fact]
        public void SetValues_when_nested_property_dictionary_in_source_is_null_throws()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 0 },
                                     { "NestedObject", null }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties, new[] { "NestedObject" });

            var obj = new FakeTypeWithProps
                          {
                              Id = 1,
                              NestedObject = new FakeTypeWithProps
                                                 {
                                                     Id = 2
                                                 }
                          };

            Assert.Equal(
                Strings.DbPropertyValues_NestedPropertyValuesNull("NestedObject", typeof(FakeTypeWithProps).Name),
                Assert.Throws<InvalidOperationException>(() => values.SetValues(obj)).Message);
        }

        [Fact]
        public void SetValues_does_not_attempt_to_set_values_that_are_not_different()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "PublicStringProp", "PublicStringPropValue" },
                                     { "PublicIntProp", 2 },
                                     { "PublicBinaryProp", new byte[] { 3, 1, 4, 1, 5, 9 } },
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties);

            var obj = new FakeTypeWithProps
                          {
                              PublicStringProp = "PublicStringPropValue",
                              PublicIntProp = 2,
                              PublicBinaryProp = new byte[] { 3, 1, 4, 1, 5, 9 }
                          };

            values.SetValues(obj);

            values.GetMockItem("PublicStringProp").VerifySet(i => i.Value = It.IsAny<object>(), Times.Never());
            values.GetMockItem("PublicIntProp").VerifySet(i => i.Value = It.IsAny<object>(), Times.Never());
            values.GetMockItem("PublicBinaryProp").VerifySet(i => i.Value = It.IsAny<object>(), Times.Never());
        }

        #endregion

        #region Tests for copying values to a new dictionary

        [Fact]
        public void Non_Generic_DbPropertyValues_uses_Clone_on_InternalPropertyValues()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 1 }
                                 };
            var values = new DbPropertyValues(new TestInternalPropertyValues<FakeTypeWithProps>(properties));

            var clone = values.Clone();

            Assert.Equal(1, clone["Id"]);
        }

        [Fact]
        public void Clone_for_an_entity_returns_a_new_dictionary_that_is_also_for_an_entity()
        {
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(null, isEntityValues: true);

            var clone = values.Clone();

            Assert.True(clone.IsEntityValues);
        }

        [Fact]
        public void Clone_for_a_complex_object_returns_a_new_dictionary_that_is_also_for_an_complex_object()
        {
            var values = new TestInternalPropertyValues<FakeTypeWithProps>();

            var clone = values.Clone();

            Assert.False(clone.IsEntityValues);
        }

        [Fact]
        public void Clone_sets_all_properties_from_the_dictionary_into_the_new_values()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 1 },
                                     { "PublicStringProp", "PublicStringPropValue" },
                                     { "PrivateStringProp", "PrivateStringPropValue" },
                                     { "PublicIntProp", 2 },
                                     { "PrivateIntProp", 3 },
                                     { "PublicIntPropWithPrivateSetter", 4 },
                                     { "PublicIntPropWithPrivateGetter", 5 },
                                     { "PublicBinaryProp", new byte[] { 3, 1, 4, 1, 5, 9 } },
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties);

            var clone = values.Clone();

            Assert.Equal(1, clone["Id"]);
            Assert.Equal("PublicStringPropValue", clone["PublicStringProp"]);
            Assert.Equal(2, clone["PublicIntProp"]);
            Assert.Equal("PrivateStringPropValue", clone["PrivateStringProp"]);
            Assert.Equal(3, clone["PrivateIntProp"]);
            Assert.Equal(4, clone["PublicIntPropWithPrivateSetter"]);
            Assert.Equal(5, clone["PublicIntPropWithPrivateGetter"]);
            Assert.True(DbHelpers.KeyValuesEqual(new byte[] { 3, 1, 4, 1, 5, 9 }, clone["PublicBinaryProp"]));
        }

        [Fact]
        public void Clone_can_copy_null_properties()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "PublicNonNullStringProp", null }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties);

            var clone = values.Clone();

            Assert.Null(clone["PublicNonNullStringProp"]);
        }

        [Fact]
        public void Clone_clones_nested_property_dictionary_into_new_cloned_nested_dictionary()
        {
            var nestedProperties = new Dictionary<string, object>
                                       {
                                           { "Id", 2 }
                                       };
            var nestedValues = new TestInternalPropertyValues<FakeTypeWithProps>(nestedProperties);

            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 1 },
                                     { "NestedObject", nestedValues }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties, new[] { "NestedObject" });

            var clone = values.Clone();
            var nestedClone = (InternalPropertyValues)clone["NestedObject"];

            Assert.Equal(1, clone["Id"]);
            Assert.Equal(2, nestedClone["Id"]);

            Assert.False(clone.GetItem("Id").IsComplex);
            Assert.True(clone.GetItem("NestedObject").IsComplex);
            Assert.False(nestedClone.GetItem("Id").IsComplex);
        }

        [Fact]
        public void Clone_ignores_null_nested_property_dictionary()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 1 },
                                     { "NestedObject", null }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties, new[] { "NestedObject" });

            var clone = values.Clone();

            Assert.Equal(1, clone["Id"]);
            Assert.Null(clone["NestedObject"]);
        }

        [Fact]
        public void Modifying_properties_on_cloned_dictionary_does_not_change_properties_on_original_dictionary_and_vice_versa()
        {
            var nestedProperties = new Dictionary<string, object>
                                       {
                                           { "Id", 2 }
                                       };
            var nestedValues = new TestInternalPropertyValues<FakeTypeWithProps>(nestedProperties);

            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 1 },
                                     { "NestedObject", nestedValues }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties, new[] { "NestedObject" });

            var clone = values.Clone();
            var nestedClone = (InternalPropertyValues)clone["NestedObject"];

            values["Id"] = -1;
            nestedValues["Id"] = -2;
            clone["Id"] = -3;
            nestedClone["Id"] = -4;

            Assert.Equal(-1, values["Id"]);
            Assert.Equal(-2, nestedValues["Id"]);
            Assert.Equal(-3, clone["Id"]);
            Assert.Equal(-4, nestedClone["Id"]);
        }

        #endregion

        #region Tests for setting values from another dictionary

        [Fact]
        public void Attempt_to_copy_values_from_null_dictionary_on_non_generic_DbPropertyValues_throws()
        {
            var values = new DbPropertyValues(new TestInternalPropertyValues<FakeTypeWithProps>());

            Assert.Equal("propertyValues", Assert.Throws<ArgumentNullException>(() => values.SetValues(null)).ParamName);
        }

        [Fact]
        public void Attempt_to_copy_values_from_dictionary_of_wrong_type_throws()
        {
            var values1 = new TestInternalPropertyValues<FakeDerivedTypeWithProps>();
            var values2 = new TestInternalPropertyValues<FakeTypeWithProps>();

            Assert.Equal(
                Strings.DbPropertyValues_AttemptToSetValuesFromWrongType(
                    typeof(FakeTypeWithProps).Name, typeof(FakeDerivedTypeWithProps).Name),
                Assert.Throws<ArgumentException>(() => values1.SetValues(values2)).Message);
        }

        [Fact]
        public void SetValues_copies_values_from_one_dictionary_to_another_dictionary()
        {
            var fromValues = CreateSimpleValues();
            fromValues["Id"] = 1;
            fromValues["PublicStringProp"] = "PublicStringPropValue";
            fromValues["PrivateStringProp"] = "PrivateStringPropValue";
            fromValues["PublicIntProp"] = 2;
            fromValues["PrivateIntProp"] = 3;
            fromValues["PublicIntPropWithPrivateSetter"] = 4;
            fromValues["PublicIntPropWithPrivateGetter"] = 5;
            fromValues["PublicBinaryProp"] = new byte[] { 3, 1, 4, 1, 5, 9 };

            var toValues = CreateSimpleValues();

            toValues.SetValues(fromValues);

            Assert.Equal(1, toValues["Id"]);
            Assert.Equal("PublicStringPropValue", toValues["PublicStringProp"]);
            Assert.Equal(2, toValues["PublicIntProp"]);
            Assert.Equal("PrivateStringPropValue", toValues["PrivateStringProp"]);
            Assert.Equal(3, toValues["PrivateIntProp"]);
            Assert.Equal(4, toValues["PublicIntPropWithPrivateSetter"]);
            Assert.Equal(5, toValues["PublicIntPropWithPrivateGetter"]);
            Assert.True(DbHelpers.KeyValuesEqual(new byte[] { 3, 1, 4, 1, 5, 9 }, toValues["PublicBinaryProp"]));
        }

        private TestInternalPropertyValues<FakeTypeWithProps> CreateSimpleValues()
        {
            var nestedProperties = new Dictionary<string, object>
                                       {
                                           { "Id", 0 }
                                       };
            var nestedValues = new TestInternalPropertyValues<FakeTypeWithProps>(nestedProperties);

            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 0 },
                                     { "PublicStringProp", null },
                                     { "PrivateStringProp", null },
                                     { "PublicIntProp", 0 },
                                     { "PrivateIntProp", 0 },
                                     { "PublicIntPropWithPrivateSetter", 0 },
                                     { "PublicIntPropWithPrivateGetter", 0 },
                                     { "PublicBinaryProp", null },
                                     { "NestedObject", nestedValues },
                                 };
            return new TestInternalPropertyValues<FakeTypeWithProps>(properties, new[] { "NestedObject" });
        }

        [Fact]
        public void Non_Generic_DbPropertyValues_SetValues_with_a_dictionary_works_on_the_underlying_dictionary()
        {
            var fromValues = new DbPropertyValues(CreateSimpleValues());
            fromValues["Id"] = 1;

            var toValues = new DbPropertyValues(CreateSimpleValues());

            toValues.SetValues(fromValues);

            Assert.Equal(1, toValues["Id"]);
        }

        [Fact]
        public void Calling_SetValues_with_dictionary_of_derived_type_works()
        {
            var fromProperties = new Dictionary<string, object>
                                     {
                                         { "Id", 1 }
                                     };
            var fromValues = new DbPropertyValues(new TestInternalPropertyValues<FakeDerivedTypeWithProps>(fromProperties));

            var toProperties = new Dictionary<string, object>
                                   {
                                       { "Id", 0 }
                                   };
            var toValues = new DbPropertyValues(new TestInternalPropertyValues<FakeTypeWithProps>(toProperties));

            toValues.SetValues(fromValues);

            Assert.Equal(1, toValues["Id"]);
        }

        [Fact]
        public void SetValues_with_dictionary_works_with_null_values()
        {
            var fromValues = CreateSimpleValues();
            fromValues["Id"] = 1;
            var toValues = CreateSimpleValues();
            toValues["PublicStringProp"] = "Non-null";

            toValues.SetValues(fromValues);

            Assert.Equal(1, toValues["Id"]);
            Assert.Null(toValues["PublicStringProp"]);
        }

        [Fact]
        public void SetValues_sets_nested_values_from_the_dictionary_into_nested_property_dictionary()
        {
            var fromValues = CreateSimpleValues();
            fromValues["Id"] = 1;
            ((InternalPropertyValues)fromValues["NestedObject"])["Id"] = 2;

            var toValues = CreateSimpleValues();

            toValues.SetValues(fromValues);

            Assert.Equal(1, toValues["Id"]);
            Assert.Equal(2, ((InternalPropertyValues)toValues["NestedObject"])["Id"]);
        }

        [Fact]
        public void SetValues_when_nested_dictionary_is_for_wrong_type_throws()
        {
            var fromProperties = new Dictionary<string, object>
                                     {
                                         { "Id", 1 }
                                     };
            var fromValues = new DbPropertyValues(new TestInternalPropertyValues<FakeTypeWithProps>(fromProperties));

            var toProperties = new Dictionary<string, object>
                                   {
                                       { "Id", 0 }
                                   };
            var toValues = new DbPropertyValues(new TestInternalPropertyValues<FakeDerivedTypeWithProps>(toProperties));

            Assert.Equal(
                Strings.DbPropertyValues_AttemptToSetValuesFromWrongType(
                    typeof(FakeTypeWithProps).Name, typeof(FakeDerivedTypeWithProps).Name),
                Assert.Throws<ArgumentException>(() => toValues.SetValues(fromValues)).Message);
        }

        [Fact]
        public void SetValues_when_nested_dictionary_in_source_is_null_throws()
        {
            var nestedValues = new TestInternalPropertyValues<FakeTypeWithProps>();

            var fromProperties = new Dictionary<string, object>
                                     {
                                         { "NestedObject", null }
                                     };
            var fromValues = new TestInternalPropertyValues<FakeTypeWithProps>(fromProperties, new[] { "NestedObject" });

            var toProperties = new Dictionary<string, object>
                                   {
                                       { "NestedObject", nestedValues }
                                   };
            var toValues = new TestInternalPropertyValues<FakeTypeWithProps>(toProperties, new[] { "NestedObject" });

            Assert.Equal(
                Strings.DbPropertyValues_NestedPropertyValuesNull("NestedObject", typeof(FakeTypeWithProps).Name),
                Assert.Throws<InvalidOperationException>(() => toValues.SetValues(fromValues)).Message);
        }

        [Fact]
        public void SetValues_when_nested_dictionary_in_target_is_null_throws()
        {
            var nestedValues = new TestInternalPropertyValues<FakeTypeWithProps>();

            var fromProperties = new Dictionary<string, object>
                                     {
                                         { "NestedObject", nestedValues }
                                     };
            var fromValues = new TestInternalPropertyValues<FakeTypeWithProps>(fromProperties, new[] { "NestedObject" });

            var toProperties = new Dictionary<string, object>
                                   {
                                       { "NestedObject", null }
                                   };
            var toValues = new TestInternalPropertyValues<FakeTypeWithProps>(toProperties, new[] { "NestedObject" });

            Assert.Equal(
                Strings.DbPropertyValues_NestedPropertyValuesNull("NestedObject", typeof(FakeTypeWithProps).Name),
                Assert.Throws<InvalidOperationException>(() => toValues.SetValues(fromValues)).Message);
        }

        [Fact]
        public void SetValues_from_another_dictionary_does_not_attempt_to_set_values_that_are_not_different()
        {
            var fromValues = CreateSimpleValues();
            var toValues = CreateSimpleValues();

            toValues.SetValues(fromValues);

            toValues.GetMockItem("PublicStringProp").VerifySet(i => i.Value = It.IsAny<object>(), Times.Never());
            toValues.GetMockItem("PublicIntProp").VerifySet(i => i.Value = It.IsAny<object>(), Times.Never());
            toValues.GetMockItem("PublicBinaryProp").VerifySet(i => i.Value = It.IsAny<object>(), Times.Never());
        }

        #endregion

        #region Tests for reading values from dictionarys

        [Fact]
        public void Scalar_values_can_be_read_from_a_property_dictionary()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 1 }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties);

            Assert.Equal(1, values["Id"]);
        }

        [Fact]
        public void Complex_values_can_be_read_from_a_property_dictionary()
        {
            var nestedProperties = new Dictionary<string, object>
                                       {
                                           { "Id", 1 }
                                       };
            var nestedValues = new TestInternalPropertyValues<FakeTypeWithProps>(nestedProperties);

            var properties = new Dictionary<string, object>
                                 {
                                     { "NestedObject", nestedValues }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties, new[] { "NestedObject" });

            var readValues = (InternalPropertyValues)values["NestedObject"];
            Assert.Equal(1, readValues["Id"]);
        }

        [Fact]
        public void Reading_values_from_non_generic_DbPropertyValues_uses_the_internal_dictionary_and_returns_a_non_generic_dictionary()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 1 }
                                 };
            var values = new DbPropertyValues(new TestInternalPropertyValues<FakeTypeWithProps>(properties));

            Assert.Equal(1, values["Id"]);
        }

        [Fact]
        public void Reading_value_from_non_generic_DbPropertyValues_for_a_null_property_name_throws()
        {
            var values = new DbPropertyValues(CreateSimpleValues());

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("propertyName"),
                Assert.Throws<ArgumentException>(() => { var _ = values[null]; }).Message);
        }

        [Fact]
        public void Reading_value_from_non_generic_DbPropertyValues_for_an_empty_property_name_throws()
        {
            var values = new DbPropertyValues(CreateSimpleValues());

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => { var _ = values[""]; }).Message);
        }

        [Fact]
        public void Reading_value_from_non_generic_DbPropertyValues_for_a_whitespace_property_name_throws()
        {
            var values = new DbPropertyValues(CreateSimpleValues());

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => { var _ = values[" "]; }).Message);
        }

        [Fact]
        public void Reading_value_for_a_missing_property_name_throws()
        {
            var values = CreateSimpleValues();

            Assert.Equal(
                Strings.DbPropertyValues_PropertyDoesNotExist("NoSuchProperty", typeof(FakeTypeWithProps).Name),
                Assert.Throws<ArgumentException>(() => { var _ = values["NoSuchProperty"]; }).Message);
        }

        #endregion

        #region Tests for writing values into property values

        [Fact]
        public void Scalar_values_can_be_set_into_a_property_dictionary()
        {
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(
                new Dictionary<string, object>
                    {
                        { "Id", 0 }
                    });

            values["Id"] = 1;

            Assert.Equal(1, values["Id"]);
        }

        [Fact]
        public void Complex_values_can_be_set_at_the_complex_object_level_into_a_nested_property_dictionary()
        {
            SettingNestedValuesTest((values, nestedValues) => values["NestedObject"] = nestedValues);
        }

        [Fact]
        public void Complex_values_can_be_set_at_the_complex_object_level_into_a_non_generic_dictionary_using_a_generic_dictionary()
        {
            SettingNestedValuesTest(
                (values, nestedValues) => new DbPropertyValues(values)["NestedObject"] = new DbPropertyValues(nestedValues));
        }

        [Fact]
        public void Complex_values_can_be_set_at_the_complex_object_level_into_a_non_generic_dictionary_using_a_non_generic_dictionary()
        {
            SettingNestedValuesTest(
                (values, nestedValues) => new DbPropertyValues(values)["NestedObject"] = new DbPropertyValues(nestedValues));
        }

        private void SettingNestedValuesTest(Action<InternalPropertyValues, InternalPropertyValues> bagger)
        {
            var nestedProperties = new Dictionary<string, object>
                                       {
                                           { "Id", 0 }
                                       };
            var nestedValues = new TestInternalPropertyValues<FakeTypeWithProps>(nestedProperties);

            var properties = new Dictionary<string, object>
                                 {
                                     { "NestedObject", nestedValues }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties, new[] { "NestedObject" });

            var newNestedProperties = new Dictionary<string, object>
                                          {
                                              { "Id", 1 }
                                          };
            var newNestedValues = new TestInternalPropertyValues<FakeTypeWithProps>(newNestedProperties);

            bagger(values, newNestedValues);

            var readValues = (InternalPropertyValues)values["NestedObject"];

            Assert.Same(nestedValues, readValues); // The nested dictionary itself has not changed
            Assert.Equal(1, readValues["Id"]); // But the values in the dictionary have been copied.
        }

        [Fact]
        public void Complex_values_cannot_be_set_to_null_in_a_property_dictionary()
        {
            var values = new DbPropertyValues(CreateSimpleValues());

            Assert.Equal(
                Strings.DbPropertyValues_AttemptToSetNonValuesOnComplexProperty,
                Assert.Throws<ArgumentException>(() => values["NestedObject"] = null).Message);
        }

        [Fact]
        public void Complex_values_cannot_be_set_to_actual_complex_object_instance_in_a_property_dictionary()
        {
            var values = new DbPropertyValues(CreateSimpleValues());

            Assert.Equal(
                Strings.DbPropertyValues_AttemptToSetNonValuesOnComplexProperty,
                Assert.Throws<ArgumentException>(() => values["NestedObject"] = new FakeTypeWithProps()).Message);
        }

        [Fact]
        public void Writing_values_to_non_generic_DbPropertyValues_uses_the_internal_dictionary()
        {
            var values = new DbPropertyValues(
                new TestInternalPropertyValues<FakeTypeWithProps>(
                    new Dictionary<string, object>
                        {
                            { "Id", 0 }
                        }));

            values["Id"] = 1;

            Assert.Equal(1, values["Id"]);
        }

        [Fact]
        public void Writing_values_to_non_generic_DbPropertyValues_for_a_null_property_name_throws()
        {
            var values = new DbPropertyValues(CreateSimpleValues());

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => values[null] = 0).Message);
        }

        [Fact]
        public void Writing_values_to_non_generic_DbPropertyValues_for_an_empty_property_name_throws()
        {
            var values = new DbPropertyValues(CreateSimpleValues());

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => values[""] = 0).Message);
        }

        [Fact]
        public void Writing_values_to_non_generic_DbPropertyValues_for_a_whitespace_property_name_throws()
        {
            var values = new DbPropertyValues(CreateSimpleValues());

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => values[" "] = 0).Message);
        }

        [Fact]
        public void Writing_values_to_a_missing_property_name_throws()
        {
            var values = CreateSimpleValues();

            Assert.Equal(
                Strings.DbPropertyValues_PropertyDoesNotExist("NoSuchProperty", typeof(FakeTypeWithProps).Name),
                Assert.Throws<ArgumentException>(() => values["NoSuchProperty"] = 0).Message);
        }

        #endregion

        #region Tests for reading property names from a dictionary

        [Fact]
        public void Property_names_can_be_read_from_a_property_dictionary()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "One", 1 },
                                     { "Two", 2 },
                                     { "Three", 3 }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties);

            var names = values.PropertyNames;

            Assert.Equal(3, names.Count);
            Assert.True(names.Contains("One"));
            Assert.True(names.Contains("Two"));
            Assert.True(names.Contains("Three"));
        }

        [Fact]
        public void Property_names_are_returned_as_a_readonly_set()
        {
            var properties = new Dictionary<string, object>
                                 {
                                     { "Id", 1 }
                                 };
            var values = new TestInternalPropertyValues<FakeTypeWithProps>(properties);

            var names = values.PropertyNames;

            Assert.True(names.IsReadOnly);
        }

        [Fact]
        public void ReadOnlyHashSet_is_readonly_and_throws_for_mutating_methods()
        {
            var set = new ReadOnlySet<string>(new HashSet<string>());

            Assert.True(set.IsReadOnly);

            Assert.Equal(
                Strings.DbPropertyValues_PropertyValueNamesAreReadonly,
                Assert.Throws<NotSupportedException>(() => ((ICollection<string>)set).Add("")).Message);
            Assert.Equal(
                Strings.DbPropertyValues_PropertyValueNamesAreReadonly, Assert.Throws<NotSupportedException>(() => set.Add("")).Message);
            Assert.Equal(
                Strings.DbPropertyValues_PropertyValueNamesAreReadonly, Assert.Throws<NotSupportedException>(() => set.Clear()).Message);
            Assert.Equal(
                Strings.DbPropertyValues_PropertyValueNamesAreReadonly, Assert.Throws<NotSupportedException>(() => set.Remove("")).Message);
        }

        [Fact]
        public void ReadOnlyHashSet_calls_underlying_Set_methods()
        {
            var mockSet = new Mock<ISet<string>>();
            var mockIEnumerable = mockSet.As<IEnumerable>();
            var set = new ReadOnlySet<string>(mockSet.Object);
            var other = new HashSet<string>();

            var array = new string[0];
            set.CopyTo(array, 0);
            mockSet.Verify(s => s.CopyTo(array, 0), Times.Once());

            set.ExceptWith(other);
            mockSet.Verify(s => s.ExceptWith(other), Times.Once());

            set.IntersectWith(other);
            mockSet.Verify(s => s.IntersectWith(other), Times.Once());

            set.IsProperSubsetOf(other);
            mockSet.Verify(s => s.IsProperSubsetOf(other), Times.Once());

            set.IsProperSupersetOf(other);
            mockSet.Verify(s => s.IsProperSupersetOf(other), Times.Once());

            set.IsSubsetOf(other);
            mockSet.Verify(s => s.IsSubsetOf(other), Times.Once());

            set.IsSupersetOf(other);
            mockSet.Verify(s => s.IsSupersetOf(other), Times.Once());

            set.Overlaps(other);
            mockSet.Verify(s => s.Overlaps(other), Times.Once());

            set.SetEquals(other);
            mockSet.Verify(s => s.SetEquals(other), Times.Once());

            set.SymmetricExceptWith(other);
            mockSet.Verify(s => s.SymmetricExceptWith(other), Times.Once());

            set.UnionWith(other);
            mockSet.Verify(s => s.UnionWith(other), Times.Once());

            set.Contains("Foo");
            mockSet.Verify(s => s.Contains("Foo"), Times.Once());

            var _ = set.Count;
            mockSet.Verify(s => s.Count, Times.Exactly(1));

            mockSet.Setup(s => s.GetEnumerator()).Returns(Enumerable.Empty<string>().GetEnumerator());
            set.GetEnumerator();
            mockSet.Verify(s => s.GetEnumerator(), Times.Once());

            mockIEnumerable.Setup(s => s.GetEnumerator()).Returns(Enumerable.Empty<string>().GetEnumerator());
            ((IEnumerable)set).GetEnumerator();
            mockIEnumerable.Verify(s => s.GetEnumerator(), Times.Once());
        }

        #endregion
    }
}
