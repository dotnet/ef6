// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using Moq;
    using Xunit;

    public class EntityKeyTests
    {
        public class Constructors
        {
            [Fact]
            public void Constructors_validate_arguments_are_not_null_or_empty()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("qualifiedEntitySetName"),
                    Assert.Throws<ArgumentException>(() => new EntityKey(null, (IEnumerable<KeyValuePair<string, object>>)null)).Message);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("qualifiedEntitySetName"),
                    Assert.Throws<ArgumentException>(() => new EntityKey("", (IEnumerable<KeyValuePair<string, object>>)null)).Message);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("qualifiedEntitySetName"),
                    Assert.Throws<ArgumentException>(() => new EntityKey(" ", (IEnumerable<KeyValuePair<string, object>>)null)).Message);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("qualifiedEntitySetName"),
                    Assert.Throws<ArgumentException>(() => new EntityKey(null, (IEnumerable<EntityKeyMember>)null)).Message);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("qualifiedEntitySetName"),
                    Assert.Throws<ArgumentException>(() => new EntityKey("", (IEnumerable<EntityKeyMember>)null)).Message);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("qualifiedEntitySetName"),
                    Assert.Throws<ArgumentException>(() => new EntityKey(" ", (IEnumerable<EntityKeyMember>)null)).Message);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("qualifiedEntitySetName"),
                    Assert.Throws<ArgumentException>(() => new EntityKey(null, "Foo", 1)).Message);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("qualifiedEntitySetName"),
                    Assert.Throws<ArgumentException>(() => new EntityKey("", "Foo", 1)).Message);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("qualifiedEntitySetName"),
                    Assert.Throws<ArgumentException>(() => new EntityKey(" ", "Foo", 1)).Message);

                Assert.Equal(
                    "entityKeyValues",
                    Assert.Throws<ArgumentNullException>(() => new EntityKey("A.B", (IEnumerable<KeyValuePair<string, object>>)null)).
                        ParamName);

                Assert.Equal(
                    "entityKeyValues",
                    Assert.Throws<ArgumentNullException>(() => new EntityKey("A.B", (IEnumerable<EntityKeyMember>)null)).ParamName);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("keyName"),
                    Assert.Throws<ArgumentException>(() => new EntityKey("A.B", null, 1)).Message);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("keyName"),
                    Assert.Throws<ArgumentException>(() => new EntityKey("A.B", "", 1)).Message);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("keyName"),
                    Assert.Throws<ArgumentException>(() => new EntityKey("A.B", " ", 1)).Message);

                Assert.Equal(
                    "keyValue",
                    Assert.Throws<ArgumentNullException>(() => new EntityKey("A.B", "Foo", null)).ParamName);
            }

            [Fact]
            public void InitializeEntitySetName_validates_name_has_two_non_empty_parts()
            {
                Assert.True(
                    Assert.Throws<ArgumentException>(() => new EntityKey().InitializeEntitySetName("A")).Message.StartsWith(
                        Strings.EntityKey_InvalidQualifiedEntitySetName));

                Assert.True(
                    Assert.Throws<ArgumentException>(() => new EntityKey().InitializeEntitySetName("A.")).Message.StartsWith(
                        Strings.EntityKey_InvalidQualifiedEntitySetName));

                Assert.True(
                    Assert.Throws<ArgumentException>(() => new EntityKey().InitializeEntitySetName("A. ")).Message.StartsWith(
                        Strings.EntityKey_InvalidQualifiedEntitySetName));

                Assert.True(
                    Assert.Throws<ArgumentException>(() => new EntityKey().InitializeEntitySetName(".B")).Message.StartsWith(
                        Strings.EntityKey_InvalidQualifiedEntitySetName));

                Assert.True(
                    Assert.Throws<ArgumentException>(() => new EntityKey().InitializeEntitySetName(" .B")).Message.StartsWith(
                        Strings.EntityKey_InvalidQualifiedEntitySetName));
            }

            [Fact]
            public void InitializeEntitySetName_validates_both_parts_of_name_are_valid_EDM_identifiers()
            {
                Assert.Equal(
                    Strings.EntityKey_InvalidName("♋"),
                    Assert.Throws<ArgumentException>(() => new EntityKey().InitializeEntitySetName("♋.B")).Message);

                Assert.Equal(
                    Strings.EntityKey_InvalidName("☢"),
                    Assert.Throws<ArgumentException>(() => new EntityKey().InitializeEntitySetName("A.☢")).Message);
            }

            [Fact]
            public void InitializeEntitySetName_sets_the_container_name_and_entity_set_name_into_the_key_instance()
            {
                var entityKey = new EntityKey();
                entityKey.InitializeEntitySetName("Container.Set");

                Assert.Equal("Container", entityKey.EntityContainerName);
                Assert.Equal("Set", entityKey.EntitySetName);
            }

            [Fact]
            public void KeyValues_constructor_initializes_entity_set_and_key_values()
            {
                var key = new EntityKey("Container.Set", new[] { new KeyValuePair<string, object>("Name", 1), });

                Assert.Equal("Set", key.EntitySetName);
                Assert.Equal("Container", key.EntityContainerName);
                Assert.Equal("Name", key.EntityKeyValues.Single().Key);
                Assert.Equal(1, key.EntityKeyValues.Single().Value);
            }

            [Fact]
            public void Strings_constructor_initializes_entity_set_and_key_values()
            {
                var key = new EntityKey("Container.Set", "Name", 1);

                Assert.Equal("Set", key.EntitySetName);
                Assert.Equal("Container", key.EntityContainerName);
                Assert.Equal("Name", key.EntityKeyValues.Single().Key);
                Assert.Equal(1, key.EntityKeyValues.Single().Value);
            }

            [Fact]
            public void Strings_constructor_throws_for_bad_key_name()
            {
                Assert.Equal(
                    Strings.EntityKey_InvalidName("✄"),
                    Assert.Throws<ArgumentException>(() => new EntityKey("Container.Set", "✄", 1)).Message);
            }

            [Fact]
            public void KeyValues_constructor_initializes_entity_set_and_key_values_for_composite_keys()
            {
                var key = new EntityKey(
                    "Container.Set",
                    new[]
                        {
                            new KeyValuePair<string, object>("Name1", 1),
                            new KeyValuePair<string, object>("Name2", "Foo"),
                        });

                Assert.Equal("Set", key.EntitySetName);
                Assert.Equal("Container", key.EntityContainerName);

                Assert.Equal("Name1", key.EntityKeyValues.First().Key);
                Assert.Equal(1, key.EntityKeyValues.First().Value);

                Assert.Equal("Name2", key.EntityKeyValues.Skip(1).Single().Key);
                Assert.Equal("Foo", key.EntityKeyValues.Skip(1).Single().Value);
            }

            [Fact]
            public void EntityKeyMembers_constructor_initializes_entity_set_and_key_values()
            {
                var key = new EntityKey("Container.Set", new[] { new EntityKeyMember("Name", 1), });

                Assert.Equal("Set", key.EntitySetName);
                Assert.Equal("Container", key.EntityContainerName);
                Assert.Equal("Name", key.EntityKeyValues.Single().Key);
                Assert.Equal(1, key.EntityKeyValues.Single().Value);
            }

            [Fact]
            public void EntityKeyMembers_constructor_initializes_entity_set_and_key_values_for_composite_keys()
            {
                var key = new EntityKey(
                    "Container.Set",
                    new[]
                        {
                            new EntityKeyMember("Name1", 1),
                            new EntityKeyMember("Name2", "Foo"),
                        });

                Assert.Equal("Set", key.EntitySetName);
                Assert.Equal("Container", key.EntityContainerName);

                Assert.Equal("Name1", key.EntityKeyValues.First().Key);
                Assert.Equal(1, key.EntityKeyValues.First().Value);

                Assert.Equal("Name2", key.EntityKeyValues.Skip(1).Single().Key);
                Assert.Equal("Foo", key.EntityKeyValues.Skip(1).Single().Value);
            }

            [Fact]
            public void InitializeKeyValues_validates_key_names_and_values_for_single_property_keys()
            {
                InitializeKeyValues_validates_key_names_and_values_for_single_property_keys_impl(null, 1);
                InitializeKeyValues_validates_key_names_and_values_for_single_property_keys_impl("", 1);
                InitializeKeyValues_validates_key_names_and_values_for_single_property_keys_impl(" ", 1);
                InitializeKeyValues_validates_key_names_and_values_for_single_property_keys_impl("A", null);
            }

            private void InitializeKeyValues_validates_key_names_and_values_for_single_property_keys_impl(string keyName, object keyValue)
            {
                var exception = Assert.Throws<ArgumentException>(
                    () => new EntityKey().InitializeKeyValues(new[] { new KeyValuePair<string, object>(keyName, keyValue), }));

                Assert.True(exception.Message.StartsWith(Strings.EntityKey_NoNullsAllowedInKeyValuePairs));
                Assert.Equal("entityKeyValues", exception.ParamName);
            }

            [Fact]
            public void InitializeKeyValues_validates_key_names_and_values_for_composite_keys()
            {
                InitializeKeyValues_validates_key_names_and_values_for_composite_keys_impl(null, 1);
                InitializeKeyValues_validates_key_names_and_values_for_composite_keys_impl("", 1);
                InitializeKeyValues_validates_key_names_and_values_for_composite_keys_impl(" ", 1);
                InitializeKeyValues_validates_key_names_and_values_for_composite_keys_impl("A", null);
            }

            private void InitializeKeyValues_validates_key_names_and_values_for_composite_keys_impl(string keyName, object keyValue)
            {
                var exception = Assert.Throws<ArgumentException>(
                    () => new EntityKey().InitializeKeyValues(
                        new[]
                            {
                                new KeyValuePair<string, object>("A", 1),
                                new KeyValuePair<string, object>(keyName, keyValue),
                            }));

                Assert.True(exception.Message.StartsWith(Strings.EntityKey_NoNullsAllowedInKeyValuePairs));
                Assert.Equal("entityKeyValues", exception.ParamName);
            }

            [Fact]
            public void InitializeKeyValues_throws_if_key_name_is_not_valid_identifier()
            {
                Assert.Equal(
                    Strings.EntityKey_InvalidName("✄"),
                    Assert.Throws<ArgumentException>(
                        () => new EntityKey().InitializeKeyValues(new[] { new KeyValuePair<string, object>("✄", 1), })).Message);

                Assert.Equal(
                    Strings.EntityKey_InvalidName("✄"),
                    Assert.Throws<ArgumentException>(
                        () => new EntityKey().InitializeKeyValues(
                            new[]
                                {
                                    new KeyValuePair<string, object>("A", 1),
                                    new KeyValuePair<string, object>("✄", 2),
                                })).Message);
            }

            [Fact]
            public void DataRecord_constructor_initializes_entity_set_and_key_values()
            {
                var mockEntityType = CreateMockEntityType("Name");
                var mockDataRecord = CreateMockDataRecord(mockEntityType, Tuple.Create("Name", (object)1));
                var mockEntitySet = CreateMockEntitySet(mockEntityType);

                var key = new EntityKey(mockEntitySet.Object, mockDataRecord.Object);

                Assert.Equal("Set", key.EntitySetName);
                Assert.Equal("Container", key.EntityContainerName);
                Assert.Equal("Name", key.EntityKeyValues.Single().Key);
                Assert.Equal(1, key.EntityKeyValues.Single().Value);
            }

            [Fact]
            public void DataRecord_constructor_initializes_entity_set_and_key_values_for_composite_keys()
            {
                var mockEntityType = CreateMockEntityType("Name1", "Name2");

                var mockDataRecord = CreateMockDataRecord(
                    mockEntityType,
                    Tuple.Create("Name1", (object)1),
                    Tuple.Create("Name2", (object)"Foo"));

                var mockEntitySet = CreateMockEntitySet(mockEntityType);

                var key = new EntityKey(mockEntitySet.Object, mockDataRecord.Object);

                Assert.Equal("Set", key.EntitySetName);
                Assert.Equal("Container", key.EntityContainerName);

                Assert.Equal("Name1", key.EntityKeyValues.First().Key);
                Assert.Equal(1, key.EntityKeyValues.First().Value);

                Assert.Equal("Name2", key.EntityKeyValues.Skip(1).Single().Key);
                Assert.Equal("Foo", key.EntityKeyValues.Skip(1).Single().Value);
            }
            
            [Fact]
            public void ValidateEntityKey_handles_unsigned_integers()
            {
                var entityType = new EntityType(
                    "FakeEntityType",
                    "FakeNamespace",
                    DataSpace.CSpace,
                    new[] { "key" },
                    new EdmMember[]
                    { new EdmProperty("key", TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int64))) });
                var entitySet = new EntitySet("FakeSet", "FakeSchema", "FakeTable", null, entityType);
                var entityContainer = new EntityContainer("FakeContainer", DataSpace.CSpace);
                entityContainer.AddEntitySetBase(entitySet);
                entitySet.ChangeEntityContainerWithoutCollectionFixup(entityContainer);
                var entityKey = new EntityKey(entitySet, 1U);

                entityKey.ValidateEntityKey(new Mock<MetadataWorkspace>().Object, entitySet);
            }

            [Fact]
            public void ValidateEntityKey_throws_for_incompatible_types()
            {
                var entityType = new EntityType(
                    "FakeEntityType",
                    "FakeNamespace",
                    DataSpace.CSpace,
                    new[] { "key" },
                    new EdmMember[] { new EdmProperty("key", TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32))) });
                var entitySet = new EntitySet("FakeSet", "FakeSchema", "FakeTable", null, entityType);
                var entityContainer = new EntityContainer("FakeContainer", DataSpace.CSpace);
                entityContainer.AddEntitySetBase(entitySet);
                entitySet.ChangeEntityContainerWithoutCollectionFixup(entityContainer);
                var entityKey = new EntityKey(entitySet, 1U);

                Assert.Equal(
                    Strings.EntityKey_IncorrectValueType("key", typeof(int).FullName, typeof(uint).FullName),
                    Assert.Throws<InvalidOperationException>(() =>
                            entityKey.ValidateEntityKey(new Mock<MetadataWorkspace>().Object, entitySet)).Message);
            }

            private static Mock<EntityType> CreateMockEntityType(params string[] keyNames)
            {
                var metadataCollection = new ReadOnlyMetadataCollection<EdmMember>(
                    keyNames.Select(
                        k =>
                            {
                                var mockEdmMember1 = new Mock<EdmMember>();
                                mockEdmMember1.Setup(m => m.Name).Returns(k);
                                mockEdmMember1.Setup(m => m.Identity).Returns(k);
                                return mockEdmMember1.Object;
                            }).ToList());

                var mockEntityType = new Mock<EntityType>("E", "N", DataSpace.CSpace);
                mockEntityType.Setup(m => m.KeyMembers).Returns(metadataCollection);
                mockEntityType.Setup(m => m.IsAssignableFrom(mockEntityType.Object)).Returns(true);
                mockEntityType.Setup(m => m.KeyMemberNames).Returns(keyNames.Select(k => k).ToArray());

                return mockEntityType;
            }

            private static Mock<IExtendedDataRecord> CreateMockDataRecord(
                Mock<EntityType> mockEntityType,
                params Tuple<string, object>[] keyValues)
            {
                var mockTypeUsage = new Mock<TypeUsage>();
                mockTypeUsage.Setup(m => m.EdmType).Returns(mockEntityType.Object);

                var mockDataRecordInfo = new Mock<DataRecordInfo>();
                mockDataRecordInfo.Setup(m => m.RecordType).Returns(mockTypeUsage.Object);

                var mockDataRecord = new Mock<IExtendedDataRecord>();
                mockDataRecord.Setup(m => m.DataRecordInfo).Returns(mockDataRecordInfo.Object);
                keyValues.Each(k => mockDataRecord.Setup(m => m[k.Item1]).Returns(k.Item2));

                return mockDataRecord;
            }

            private static Mock<EntitySet> CreateMockEntitySet(Mock<EntityType> mockEntityType)
            {
                var mockContainer = new Mock<EntityContainer>("C", DataSpace.CSpace);
                mockContainer.Setup(m => m.Name).Returns("Container");

                var mockEntitySet = new Mock<EntitySet>();
                mockEntitySet.Setup(m => m.ElementType).Returns(mockEntityType.Object);
                mockEntitySet.Setup(m => m.Name).Returns("Set");
                mockEntitySet.Setup(m => m.EntityContainer).Returns(mockContainer.Object);

                return mockEntitySet;
            }
        }

        public class NameProperties
        {
            [Fact]
            public void EntitySetName_can_be_set_to_null_if_already_null()
            {
                var entityKey = new EntityKey();

                entityKey.EntitySetName = null;
                Assert.Null(entityKey.EntitySetName);

                entityKey.EntitySetName = null;
                Assert.Null(entityKey.EntitySetName);
            }

            [Fact]
            public void EntityContainerName_can_be_set_to_null_if_already_null()
            {
                var entityKey = new EntityKey();

                entityKey.EntityContainerName = null;
                Assert.Null(entityKey.EntityContainerName);

                entityKey.EntityContainerName = null;
                Assert.Null(entityKey.EntityContainerName);
            }

            [Fact]
            public void EntitySetName_cannot_be_changed_once_set()
            {
                var entityKey = new EntityKey();

                entityKey.EntitySetName = "Set";
                Assert.Equal("Set", entityKey.EntitySetName);

                Assert.Equal(
                    Strings.EntityKey_CannotChangeKey,
                    Assert.Throws<InvalidOperationException>(() => entityKey.EntitySetName = "SetAgain").Message);

                Assert.Equal("Set", entityKey.EntitySetName);
            }

            [Fact]
            public void EntityContainerName_cannot_be_changed_once_set()
            {
                var entityKey = new EntityKey();

                entityKey.EntityContainerName = "Set";
                Assert.Equal("Set", entityKey.EntityContainerName);

                Assert.Equal(
                    Strings.EntityKey_CannotChangeKey,
                    Assert.Throws<InvalidOperationException>(() => entityKey.EntityContainerName = "SetAgain").Message);

                Assert.Equal("Set", entityKey.EntityContainerName);
            }
        }

        public class LookupSingletonName
        {
            [Fact]
            public void LookupSingletonName_returns_null_when_given_null()
            {
                Assert.Null(EntityKey.LookupSingletonName(null));
            }

            [Fact]
            public void LookupSingletonName_returns_null_when_given_an_empty_string()
            {
                Assert.Null(EntityKey.LookupSingletonName(""));
            }

            [Fact]
            public void LookupSingletonName_returns_reference_to_stored_object_when_given_stored_object()
            {
                const string foo = "foo";
                Assert.Same(foo, EntityKey.LookupSingletonName(foo));
                Assert.Same(foo, EntityKey.LookupSingletonName(foo));
            }

            [Fact]
            public void LookupSingletonName_returns_reference_to_stored_object_when_given_non_stored_but_equal_string()
            {
                const string foo = "foo";
                Assert.Same(foo, EntityKey.LookupSingletonName(foo));

                var differentFoo = "FOO".ToLower();
                Assert.NotSame(foo, differentFoo);
                Assert.Same(foo, EntityKey.LookupSingletonName(differentFoo));
            }
        }
    }
}
