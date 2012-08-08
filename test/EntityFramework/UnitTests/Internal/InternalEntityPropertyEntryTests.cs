// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using Moq;
    using Xunit;

    public class InternalEntityPropertyEntryTests
    {
        #region Helper methods

        private static void SetWrongTypeTest(Action<InternalEntityPropertyEntry> setValue)
        {
            var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.RefTypePropertyMetadata);

            Assert.Equal(
                Strings.DbPropertyValues_WrongTypeForAssignment(
                    typeof(Random).Name, "RefTypeProp", typeof(string).Name, typeof(FakeWithProps).Name),
                Assert.Throws<InvalidOperationException>(() => setValue(propEntry)).Message);
        }

        private static void SetWrongComplexTypeTest(Action<InternalEntityPropertyEntry> setValue)
        {
            var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.ComplexPropertyMetadata);

            Assert.Equal(
                Strings.DbPropertyValues_AttemptToSetValuesFromWrongObject(typeof(Random).Name, typeof(FakeWithProps).Name),
                Assert.Throws<ArgumentException>(() => setValue(propEntry)).Message);
        }

        #endregion

        public class OriginalValues
        {
            [Fact]
            public void Scalar_original_value_can_be_set()
            {
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.ValueTypePropertyMetadata);

                propEntry.OriginalValue = -1;

                Assert.Equal(-1, entityEntry.OriginalValues["ValueTypeProp"]);
            }

            [Fact]
            public void Scalar_original_value_can_be_read()
            {
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.ValueTypePropertyMetadata);

                var value = propEntry.OriginalValue;

                Assert.Equal(21, value);
            }

            [Fact]
            public void Scalar_original_value_can_be_set_to_null()
            {
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.RefTypePropertyMetadata);

                propEntry.OriginalValue = null;

                Assert.Null(entityEntry.OriginalValues["RefTypeProp"]);
            }

            [Fact]
            public void Scalar_original_value_can_be_read_when_when_it_is_null()
            {
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.RefTypePropertyMetadata);

                propEntry.OriginalValue = null;
                var value = propEntry.OriginalValue;

                Assert.Null(value);
            }

            [Fact]
            public void Original_value_for_scalar_property_cannot_be_set_to_instance_of_wrong_type()
            {
                SetWrongTypeTest(e => e.OriginalValue = new Random());
            }

            [Fact]
            public void Original_value_returned_for_complex_property_is_object_instance()
            {
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.ComplexPropertyMetadata);

                var value = (FakeWithProps)propEntry.OriginalValue;

                Assert.Equal(22, value.ValueTypeProp);
                Assert.Equal(23, value.ComplexProp.ValueTypeProp);
            }

            [Fact]
            public void Original_value_returned_for_complex_property_can_be_null()
            {
                var properties = new Dictionary<string, object>
                                     {
                                         { "ComplexProp", null }
                                     };
                var originalValues = new TestInternalPropertyValues<FakeWithProps>(properties, new[] { "ComplexProp" });
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry(null, originalValues).Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.ComplexPropertyMetadata);

                var value = propEntry.OriginalValue;

                Assert.Null(value);
            }

            [Fact]
            public void Complex_original_value_can_be_set()
            {
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.ComplexPropertyMetadata);

                propEntry.OriginalValue = new FakeWithProps
                                              {
                                                  ValueTypeProp = -2,
                                                  ComplexProp = new FakeWithProps
                                                                    {
                                                                        ValueTypeProp = -3
                                                                    }
                                              };

                Assert.Equal(21, entityEntry.OriginalValues["ValueTypeProp"]);
                Assert.Equal(-2, ((InternalPropertyValues)entityEntry.OriginalValues["ComplexProp"])["ValueTypeProp"]);
                Assert.Equal(
                    -3,
                    ((InternalPropertyValues)((InternalPropertyValues)entityEntry.OriginalValues["ComplexProp"])["ComplexProp"])[
                        "ValueTypeProp"]);
            }

            [Fact]
            public void Original_value_for_complex_property_cannot_be_set_to_null()
            {
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.ComplexPropertyMetadata);

                Assert.Equal(
                    Strings.DbPropertyValues_ComplexObjectCannotBeNull("ComplexProp", "FakeWithProps"),
                    Assert.Throws<InvalidOperationException>(() => propEntry.OriginalValue = null).Message);
            }

            [Fact]
            public void Original_value_for_complex_property_cannot_be_set_to_null_even_if_it_is_already_null()
            {
                var properties = new Dictionary<string, object>
                                     {
                                         { "ComplexProp", null }
                                     };
                var originalValues = new TestInternalPropertyValues<FakeWithProps>(properties, new[] { "ComplexProp" });
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry(null, originalValues).Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.ComplexPropertyMetadata);

                Assert.Equal(
                    Strings.DbPropertyValues_ComplexObjectCannotBeNull("ComplexProp", "FakeWithProps"),
                    Assert.Throws<InvalidOperationException>(() => propEntry.OriginalValue = null).Message);
            }

            [Fact]
            public void Original_value_for_complex_property_cannot_be_set_to_instance_with_nested_null_complex_property()
            {
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.ComplexPropertyMetadata);

                var complexObject = new FakeWithProps
                                        {
                                            ValueTypeProp = -2,
                                            ComplexProp = null
                                        };

                Assert.Equal(
                    Strings.DbPropertyValues_ComplexObjectCannotBeNull("ComplexProp", typeof(FakeWithProps).Name),
                    Assert.Throws<InvalidOperationException>(() => propEntry.OriginalValue = complexObject).Message);
            }

            [Fact]
            public void Original_value_for_complex_property_cannot_be_set_to_instance_of_wrong_type()
            {
                SetWrongComplexTypeTest(e => e.OriginalValue = new Random());
            }
        }

        public class CurrentValues
        {
            [Fact]
            public void Scalar_current_value_can_be_set()
            {
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.ValueTypePropertyMetadata);

                propEntry.CurrentValue = -1;

                Assert.Equal(-1, entityEntry.CurrentValues["ValueTypeProp"]);
            }

            [Fact]
            public void Scalar_current_value_can_be_read()
            {
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.ValueTypePropertyMetadata);

                var value = propEntry.CurrentValue;

                Assert.Equal(11, value);
            }

            [Fact]
            public void Scalar_current_value_can_be_set_to_null()
            {
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.RefTypePropertyMetadata);

                propEntry.CurrentValue = null;

                Assert.Null(entityEntry.CurrentValues["RefTypeProp"]);
            }

            [Fact]
            public void Current_value_for_scalar_property_cannot_be_set_to_instance_of_wrong_type()
            {
                SetWrongTypeTest(e => e.CurrentValue = new Random());
            }

            [Fact]
            public void Current_value_returned_for_complex_property_is_actual_complex_object_instance()
            {
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.ComplexPropertyMetadata);
                var entity = (FakeWithProps)entityEntry.Entity;

                var value = (FakeWithProps)propEntry.CurrentValue;

                Assert.Same(entity.ComplexProp, value);
                Assert.Equal(12, value.ValueTypeProp);
                Assert.Equal(13, value.ComplexProp.ValueTypeProp);
            }

            [Fact]
            public void Current_value_returned_for_complex_property_can_be_null()
            {
                var properties = new Dictionary<string, object>
                                     {
                                         { "ComplexProp", null }
                                     };
                var currentValues = new TestInternalPropertyValues<FakeWithProps>(properties, new[] { "ComplexProp" });
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry(currentValues).Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.ComplexPropertyMetadata);

                var value = propEntry.CurrentValue;

                Assert.Null(value);
            }

            [Fact]
            public void Complex_current_value_can_be_set_and_the_actual_complex_object_is_set()
            {
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.ComplexPropertyMetadata);
                var entity = (FakeWithProps)entityEntry.Entity;

                var complexObject = new FakeWithProps
                                        {
                                            ValueTypeProp = -2,
                                            ComplexProp = new FakeWithProps
                                                              {
                                                                  ValueTypeProp = -3
                                                              }
                                        };
                propEntry.CurrentValue = complexObject;

                Assert.Same(entity.ComplexProp, complexObject);
                Assert.Equal(-2, entity.ComplexProp.ValueTypeProp);
                Assert.Equal(-3, entity.ComplexProp.ComplexProp.ValueTypeProp);

                Assert.Equal(11, entityEntry.CurrentValues["ValueTypeProp"]);
                Assert.Equal(-2, ((InternalPropertyValues)entityEntry.CurrentValues["ComplexProp"])["ValueTypeProp"]);
                Assert.Equal(
                    -3,
                    ((InternalPropertyValues)((InternalPropertyValues)entityEntry.CurrentValues["ComplexProp"])["ComplexProp"])[
                        "ValueTypeProp"]);
            }

            [Fact]
            public void Current_value_for_complex_property_cannot_be_set_to_null()
            {
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.ComplexPropertyMetadata);

                Assert.Equal(
                    Strings.DbPropertyValues_ComplexObjectCannotBeNull("ComplexProp", "FakeWithProps"),
                    Assert.Throws<InvalidOperationException>(() => propEntry.CurrentValue = null).Message);
            }

            [Fact]
            public void Current_value_for_complex_property_cannot_be_set_to_null_even_if_it_is_already_null()
            {
                var properties = new Dictionary<string, object>
                                     {
                                         { "ComplexProp", null }
                                     };
                var currentValues = new TestInternalPropertyValues<FakeWithProps>(properties, new[] { "ComplexProp" });
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry(currentValues).Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.ComplexPropertyMetadata);

                Assert.Equal(
                    Strings.DbPropertyValues_ComplexObjectCannotBeNull("ComplexProp", "FakeWithProps"),
                    Assert.Throws<InvalidOperationException>(() => propEntry.CurrentValue = null).Message);
            }

            [Fact]
            public void Current_value_for_complex_property_cannot_be_set_to_instance_with_nested_null_complex_property()
            {
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.ComplexPropertyMetadata);

                var complexObject = new FakeWithProps
                                        {
                                            ValueTypeProp = -2,
                                            ComplexProp = null
                                        };

                Assert.Equal(
                    Strings.DbPropertyValues_ComplexObjectCannotBeNull("ComplexProp", typeof(FakeWithProps).Name),
                    Assert.Throws<InvalidOperationException>(() => propEntry.CurrentValue = complexObject).Message);
            }

            [Fact]
            public void Current_value_for_complex_property_cannot_be_set_to_instance_of_wrong_type()
            {
                SetWrongComplexTypeTest(e => e.CurrentValue = new Random());
            }
        }

        public class IsModified
        {
            [Fact]
            public void IsModified_returns_true_if_property_is_in_modified_list()
            {
                // Note that CreateMockInternalEntry sets ValueTypeProp as modified
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.ValueTypePropertyMetadata);

                Assert.True(propEntry.IsModified);
            }

            [Fact]
            public void IsModified_returns_false_if_property_is_not_in_modified_list()
            {
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.RefTypePropertyMetadata);

                Assert.False(propEntry.IsModified);
            }

            [Fact]
            public void IsModified_can_be_set_to_true_when_it_is_currently_false()
            {
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.RefTypePropertyMetadata);
                var mockStateEntry = Mock.Get(entityEntry.ObjectStateEntry);

                Assert.False(propEntry.IsModified);

                propEntry.IsModified = true;

                Assert.True(propEntry.IsModified);
                mockStateEntry.Verify(e => e.SetModifiedProperty("RefTypeProp"));
            }

            [Fact]
            public void IsModified_can_be_set_to_true_when_it_is_currently_true()
            {
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.ValueTypePropertyMetadata);

                Assert.True(propEntry.IsModified);

                propEntry.IsModified = true;

                Assert.True(propEntry.IsModified);
            }

            [Fact]
            public void IsModified_can_be_set_to_false_when_it_is_currently_false()
            {
                var entityEntry = FakeWithProps.CreateMockInternalEntityEntry().Object;
                var propEntry = new InternalEntityPropertyEntry(entityEntry, FakeWithProps.RefTypePropertyMetadata);

                Assert.False(propEntry.IsModified);

                propEntry.IsModified = false;

                Assert.False(propEntry.IsModified);
            }
        }

        public class Name
        {
            [Fact]
            public void Non_generic_DbPropertyEntry_Name_returns_name_of_property_from_internal_entry()
            {
                var internalEntry = new InternalEntityPropertyEntry(
                    new Mock<InternalEntityEntryForMock<FakeEntity>>().Object,
                    FakeEntity.FakeNamedFooPropertyMetadata);

                Assert.Equal("Foo", internalEntry.Name);
            }

            [Fact]
            public void Generic_DbPropertyEntry_Name_returns_name_of_property_from_internal_entry()
            {
                var internalEntry = new InternalEntityPropertyEntry(
                    new Mock<InternalEntityEntryForMock<FakeEntity>>().Object,
                    FakeEntity.FakeNamedFooPropertyMetadata);

                Assert.Equal("Foo", internalEntry.Name);
            }

            [Fact]
            public void Non_generic_DbReferenceEntry_Name_returns_name_of_property_from_internal_entry()
            {
                var internalEntry = new InternalReferenceEntry(
                    new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, FakeWithProps.ReferenceMetadata);

                Assert.Equal("Reference", internalEntry.Name);
            }

            [Fact]
            public void Generic_DbReferenceEntry_Name_returns_name_of_property_from_internal_entry()
            {
                var internalEntry = new InternalReferenceEntry(
                    new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, FakeWithProps.ReferenceMetadata);

                Assert.Equal("Reference", internalEntry.Name);
            }

            [Fact]
            public void Non_generic_DbCollectionEntry_Name_returns_name_of_property_from_internal_entry()
            {
                var internalEntry = new InternalCollectionEntry(
                    new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, FakeWithProps.CollectionMetadata);

                Assert.Equal("Collection", internalEntry.Name);
            }

            [Fact]
            public void Generic_DbCollectionEntry_Name_returns_name_of_property_from_internal_entry()
            {
                var internalEntry = new InternalCollectionEntry(
                    new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, FakeWithProps.CollectionMetadata);

                Assert.Equal("Collection", internalEntry.Name);
            }
        }
    }
}
