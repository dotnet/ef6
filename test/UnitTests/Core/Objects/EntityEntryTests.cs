// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Moq;
    using Xunit;

    public class EntityEntryTests
    {
        public class ApplyCurrentValues : TestBase
        {
            [Fact]
            public void ApplyCurrentValues_throws_for_null_argument()
            {
                Assert.Throws<ArgumentNullException>(
                    () => new EntityEntry().ApplyCurrentValues(null));
            }
        }

        public class ApplyOriginalValues : TestBase
        {
            [Fact]
            public void ApplyOriginalValues_throws_for_null_argument()
            {
                Assert.Throws<ArgumentNullException>(
                    () => new EntityEntry().ApplyOriginalValues(null));
            }
        }

        public class SetOriginalEntityValue : TestBase
        {
            [Fact]
            public void Adds_original_value_for_scalar_property()
            {
                var mockMemberMetadata = new Mock<StateManagerMemberMetadata>();
                mockMemberMetadata.Setup(m => m.IsComplex).Returns(false);

                var mockMetadata = new Mock<StateManagerTypeMetadata>();
                mockMetadata.Setup(m => m.Member(3)).Returns(mockMemberMetadata.Object);

                var entry = new EntityEntry();

                var userObject = new object();
                var value = new object();

                entry.SetOriginalEntityValue(mockMetadata.Object, 3, userObject, value);

                Assert.Equal(0, entry.FindOriginalValueIndex(mockMemberMetadata.Object, userObject));
                Assert.Same(
                    value,
                    entry.GetOriginalEntityValue(mockMetadata.Object, 3, userObject, ObjectStateValueRecord.OriginalReadonly));
            }

            [Fact]
            public void Adds_original_value_for_scalar_property_when_some_other_original_values_exist()
            {
                var mockMemberMetadata1 = new Mock<StateManagerMemberMetadata>();
                mockMemberMetadata1.Setup(m => m.IsComplex).Returns(false);
                var mockMemberMetadata2 = new Mock<StateManagerMemberMetadata>();
                mockMemberMetadata2.Setup(m => m.IsComplex).Returns(false);

                var mockMetadata = new Mock<StateManagerTypeMetadata>();
                mockMetadata.Setup(m => m.Member(1)).Returns(mockMemberMetadata1.Object);
                mockMetadata.Setup(m => m.Member(2)).Returns(mockMemberMetadata2.Object);

                var entry = new EntityEntry();

                entry.SetOriginalEntityValue(mockMetadata.Object, 2, new object(), 7);

                var userObject = new object();
                var value = new object();

                entry.SetOriginalEntityValue(mockMetadata.Object, 1, userObject, value);

                Assert.Equal(1, entry.FindOriginalValueIndex(mockMemberMetadata1.Object, userObject));
                Assert.Same(
                    value,
                    entry.GetOriginalEntityValue(mockMetadata.Object, 1, userObject, ObjectStateValueRecord.OriginalReadonly));
            }

            [Fact]
            public void Updates_original_value_for_scalar_property()
            {
                var mockMemberMetadata = new Mock<StateManagerMemberMetadata>();
                mockMemberMetadata.Setup(m => m.IsComplex).Returns(false);

                var mockMetadata = new Mock<StateManagerTypeMetadata>();
                mockMetadata.Setup(m => m.Member(3)).Returns(mockMemberMetadata.Object);

                var entry = new EntityEntry();

                var userObject = new object();

                entry.SetOriginalEntityValue(mockMetadata.Object, 3, userObject, new object());

                var value = new object();
                entry.SetOriginalEntityValue(mockMetadata.Object, 3, userObject, value);

                Assert.Equal(0, entry.FindOriginalValueIndex(mockMemberMetadata.Object, userObject));
                Assert.Same(
                    value,
                    entry.GetOriginalEntityValue(mockMetadata.Object, 3, userObject, ObjectStateValueRecord.OriginalReadonly));
            }

            [Fact]
            public void Adds_original_value_for_complex_property()
            {
                var userObject = new object();
                var oldComplexObject = new object();
                var newComplexObject = new object();
                var newValue = new object();

                var complexMetadata = CreateComplexMetadata(userObject, oldComplexObject);
                var scalarMetadata = CreateNestedScalarMetadata(newComplexObject, newValue);

                var mockStateManager = new Mock<ObjectStateManager>();
                mockStateManager
                    .Setup(m => m.GetOrAddStateManagerTypeMetadata(It.IsAny<EdmType>()))
                    .Returns(scalarMetadata);

                var entry = new EntityEntry(mockStateManager.Object);

                entry.SetOriginalEntityValue(complexMetadata, 3, userObject, newComplexObject);

                Assert.Equal(0, entry.FindOriginalValueIndex(scalarMetadata.Member(0), oldComplexObject));
                Assert.Same(
                    newValue,
                    entry.GetOriginalEntityValue(scalarMetadata, 0, oldComplexObject, ObjectStateValueRecord.OriginalReadonly));
            }

            [Fact]
            public void Adds_original_value_for_complex_property_when_some_other_original_values_exist()
            {
                var mockMemberMetadata1 = new Mock<StateManagerMemberMetadata>();
                mockMemberMetadata1.Setup(m => m.IsComplex).Returns(false);
                var mockMemberMetadata2 = new Mock<StateManagerMemberMetadata>();
                mockMemberMetadata2.Setup(m => m.IsComplex).Returns(false);

                var mockMetadata = new Mock<StateManagerTypeMetadata>();
                mockMetadata.Setup(m => m.Member(1)).Returns(mockMemberMetadata1.Object);
                mockMetadata.Setup(m => m.Member(2)).Returns(mockMemberMetadata2.Object);

                var userObject = new object();
                var oldComplexObject = new object();
                var newComplexObject = new object();
                var newValue = new object();

                var complexMetadata = CreateComplexMetadata(userObject, oldComplexObject);
                var scalarMetadata = CreateNestedScalarMetadata(newComplexObject, newValue);

                var mockStateManager = new Mock<ObjectStateManager>();
                mockStateManager
                    .Setup(m => m.GetOrAddStateManagerTypeMetadata(It.IsAny<EdmType>()))
                    .Returns(scalarMetadata);

                var entry = new EntityEntry(mockStateManager.Object);
                entry.SetOriginalEntityValue(mockMetadata.Object, 2, new object(), 7);
                entry.SetOriginalEntityValue(mockMetadata.Object, 1, new object(), 0);

                entry.SetOriginalEntityValue(complexMetadata, 3, userObject, newComplexObject);

                Assert.Equal(2, entry.FindOriginalValueIndex(scalarMetadata.Member(0), oldComplexObject));
                Assert.Same(
                    newValue,
                    entry.GetOriginalEntityValue(scalarMetadata, 0, oldComplexObject, ObjectStateValueRecord.OriginalReadonly));
            }

            [Fact]
            public void Updates_original_value_for_complex_property()
            {
                var userObject = new object();
                var oldComplexObject = new object();
                var newComplexObject = new object();
                var newValue = new object();

                var complexMetadata = CreateComplexMetadata(userObject, oldComplexObject);
                var scalarMetadata = CreateNestedScalarMetadata(newComplexObject, newValue);

                var mockStateManager = new Mock<ObjectStateManager>();
                mockStateManager
                    .Setup(m => m.GetOrAddStateManagerTypeMetadata(It.IsAny<EdmType>()))
                    .Returns(scalarMetadata);

                var entry = new EntityEntry(mockStateManager.Object);

                entry.SetOriginalEntityValue(complexMetadata, 3, userObject, newComplexObject);

                var newValue2 = new object();
                Mock.Get(scalarMetadata.Member(0)).Setup(m => m.GetValue(newComplexObject)).Returns(newValue2);

                entry.SetOriginalEntityValue(complexMetadata, 3, userObject, newComplexObject);

                Assert.Equal(0, entry.FindOriginalValueIndex(scalarMetadata.Member(0), oldComplexObject));
                Assert.Same(
                    newValue,
                    entry.GetOriginalEntityValue(scalarMetadata, 0, oldComplexObject, ObjectStateValueRecord.OriginalReadonly));
            }
        }

        public class GetOriginalEntityValue : TestBase
        {
            [Fact]
            public void Gets_DBNull_for_null_original_value()
            {
                var mockMemberMetadata = new Mock<StateManagerMemberMetadata>();
                mockMemberMetadata.Setup(m => m.IsComplex).Returns(false);

                var mockMetadata = new Mock<StateManagerTypeMetadata>();
                mockMetadata.Setup(m => m.Member(3)).Returns(mockMemberMetadata.Object);

                var entry = new EntityEntry();

                var userObject = new object();

                entry.SetOriginalEntityValue(mockMetadata.Object, 3, userObject, null);

                Assert.Same(
                    DBNull.Value,
                    entry.GetOriginalEntityValue(mockMetadata.Object, 3, userObject, ObjectStateValueRecord.OriginalReadonly));
            }

            [Fact]
            public void Gets_current_value_if_original_value_not_set()
            {
                var userObject = new object();
                var currentValue = new object();

                var mockMemberMetadata = new Mock<StateManagerMemberMetadata>();
                mockMemberMetadata.Setup(m => m.IsComplex).Returns(false);
                mockMemberMetadata.Setup(m => m.GetValue(userObject)).Returns(currentValue);

                var mockMetadata = new Mock<StateManagerTypeMetadata>();
                mockMetadata.Setup(m => m.Member(3)).Returns(mockMemberMetadata.Object);

                var entry = new EntityEntry();

                Assert.Same(
                    currentValue,
                    entry.GetOriginalEntityValue(mockMetadata.Object, 3, userObject, ObjectStateValueRecord.OriginalReadonly));
            }
        }

        public class ExpandComplexTypeAndAddValues : TestBase
        {
            [Fact]
            public void Traverses_through_multiple_levels_of_complex_properties()
            {
                var userObject = new object();
                var oldComplexObject1 = new object();
                var oldComplexObject2 = new object();
                var newComplexObject = new object();
                var newValue = new object();

                var complexType1 = new Mock<EdmType>().Object;
                var complexMetadata1 = CreateComplexMetadata(userObject, oldComplexObject1, complexType1);

                var complexType2 = new Mock<EdmType>().Object;
                var complexMetadata2 = CreateComplexMetadata(newComplexObject, oldComplexObject2, complexType2, oldComplexObject1, 0);

                var scalarMetadata = CreateNestedScalarMetadata(oldComplexObject2, newValue);

                var mockStateManager = new Mock<ObjectStateManager>();
                mockStateManager
                    .Setup(m => m.GetOrAddStateManagerTypeMetadata(complexType1))
                    .Returns(complexMetadata2);
                mockStateManager
                    .Setup(m => m.GetOrAddStateManagerTypeMetadata(complexType2))
                    .Returns(scalarMetadata);

                var entry = new EntityEntry(mockStateManager.Object);

                entry.ExpandComplexTypeAndAddValues(complexMetadata1.Member(3), oldComplexObject1, newComplexObject, true);

                Assert.Equal(0, entry.FindOriginalValueIndex(scalarMetadata.Member(0), oldComplexObject2));
                Assert.Same(
                    newValue,
                    entry.GetOriginalEntityValue(scalarMetadata, 0, oldComplexObject2, ObjectStateValueRecord.OriginalReadonly));
            }

            [Fact]
            public void Traverses_through_multiple_levels_of_complex_properties_for_changed_case()
            {
                var userObject = new object();
                var oldComplexObject1 = new object();
                var oldComplexObject2 = new object();
                var newComplexObject = new object();
                var newValue = new object();
                var newValue2 = new object();

                var complexType1 = new Mock<EdmType>().Object;
                var complexMetadata1 = CreateComplexMetadata(userObject, oldComplexObject1, complexType1);

                var complexType2 = new Mock<EdmType>().Object;
                var complexMetadata2 = CreateComplexMetadata(newComplexObject, oldComplexObject2, complexType2, oldComplexObject1, 0);

                var scalarMetadata = CreateNestedScalarMetadata(oldComplexObject2, newValue);

                var mockStateManager = new Mock<ObjectStateManager>();
                mockStateManager
                    .Setup(m => m.GetOrAddStateManagerTypeMetadata(complexType1))
                    .Returns(complexMetadata2);
                mockStateManager
                    .Setup(m => m.GetOrAddStateManagerTypeMetadata(complexType2))
                    .Returns(scalarMetadata);

                var entry = new EntityEntry(mockStateManager.Object);

                entry.AddOriginalValueAt(-1, scalarMetadata.Member(0), oldComplexObject2, newValue2);

                entry.ExpandComplexTypeAndAddValues(complexMetadata1.Member(3), oldComplexObject1, newComplexObject, false);

                Assert.Equal(0, entry.FindOriginalValueIndex(scalarMetadata.Member(0), oldComplexObject2));
                Assert.Same(
                    newValue2,
                    entry.GetOriginalEntityValue(scalarMetadata, 0, oldComplexObject2, ObjectStateValueRecord.OriginalReadonly));
            }
        }

        public class AddOriginalValueAt : TestBase
        {
            [Fact]
            public void Adds_new_values_to_list_if_not_found_index_is_passed_otherwise_updates_current_entry()
            {
                var entry = new EntityEntry();

                var userObject1 = new object();
                var metadata1 = new StateManagerMemberMetadata();
                entry.AddOriginalValueAt(-1, metadata1, userObject1, 1);

                var userObject2 = new object();
                var metadata2 = new StateManagerMemberMetadata();
                entry.AddOriginalValueAt(-1, metadata2, userObject2, 2);

                var userObject3 = new object();
                var metadata3 = new StateManagerMemberMetadata();
                entry.AddOriginalValueAt(-1, metadata3, userObject3, 3);

                Assert.Equal(0, entry.FindOriginalValueIndex(metadata1, userObject1));
                Assert.Equal(1, entry.FindOriginalValueIndex(metadata2, userObject2));
                Assert.Equal(2, entry.FindOriginalValueIndex(metadata3, userObject3));

                Assert.Equal(1, entry.GetOriginalEntityValue(null, metadata1, 0, userObject1, ObjectStateValueRecord.OriginalReadonly, 0));
                Assert.Equal(2, entry.GetOriginalEntityValue(null, metadata2, 0, userObject2, ObjectStateValueRecord.OriginalReadonly, 0));
                Assert.Equal(3, entry.GetOriginalEntityValue(null, metadata3, 0, userObject3, ObjectStateValueRecord.OriginalReadonly, 0));

                entry.AddOriginalValueAt(1, metadata2, userObject2, 7);

                Assert.Equal(1, entry.FindOriginalValueIndex(metadata2, userObject2));
                Assert.Equal(7, entry.GetOriginalEntityValue(null, metadata2, 0, userObject2, ObjectStateValueRecord.OriginalReadonly, 0));
            }
        }

        public class FindOriginalValueIndex : TestBase
        {
            [Fact]
            public void Returns_not_found_if_no_original_values_exist()
            {
                var entry = new EntityEntry();

                var userObject = new object();
                var metadata = new StateManagerMemberMetadata();
                entry.AddOriginalValueAt(-1, metadata, userObject, 1);

                Assert.Equal(-1, entry.FindOriginalValueIndex(new StateManagerMemberMetadata(), userObject));
                Assert.Equal(-1, entry.FindOriginalValueIndex(metadata, new object()));
                Assert.Equal(0, entry.FindOriginalValueIndex(metadata, userObject));
            }

            [Fact]
            public void Only_matches_entries_with_both_matching_metadata_and_user_object()
            {
                var entry = new EntityEntry();

                Assert.Equal(-1, entry.FindOriginalValueIndex(new StateManagerMemberMetadata(), new object()));
            }
        }

        private static StateManagerTypeMetadata CreateNestedScalarMetadata(object complexObject, object value)
        {
            var mockScalarMetadata = new Mock<StateManagerMemberMetadata>();
            mockScalarMetadata.Setup(m => m.IsComplex).Returns(false);
            mockScalarMetadata.Setup(m => m.GetValue(complexObject)).Returns(value);

            var mockNestedMetadata = new Mock<StateManagerTypeMetadata>();
            mockNestedMetadata.Setup(m => m.Member(0)).Returns(mockScalarMetadata.Object);
            mockNestedMetadata.Setup(m => m.FieldCount).Returns(1);

            return mockNestedMetadata.Object;
        }

        private static StateManagerTypeMetadata CreateComplexMetadata(
            object owner, object complexObject, EdmType edmType = null, object complexObject2 = null, int index = 3)
        {
            var mockUsage = new Mock<TypeUsage>();
            mockUsage.Setup(m => m.EdmType).Returns(edmType ?? new Mock<EdmType>().Object);

            var mockMember = new Mock<EdmProperty>("Foo");
            mockMember.Setup(m => m.TypeUsage).Returns(mockUsage.Object);

            var mockComplexMetadata = new Mock<StateManagerMemberMetadata>();
            mockComplexMetadata.Setup(m => m.IsComplex).Returns(true);
            mockComplexMetadata.Setup(m => m.GetValue(owner)).Returns(complexObject);
            mockComplexMetadata.Setup(m => m.GetValue(complexObject2)).Returns(complexObject);
            mockComplexMetadata.Setup(m => m.CdmMetadata).Returns(mockMember.Object);

            var mockTopMetadata = new Mock<StateManagerTypeMetadata>();
            mockTopMetadata.Setup(m => m.Member(index)).Returns(mockComplexMetadata.Object);
            mockTopMetadata.Setup(m => m.FieldCount).Returns(1);

            return mockTopMetadata.Object;
        }
    }
}
