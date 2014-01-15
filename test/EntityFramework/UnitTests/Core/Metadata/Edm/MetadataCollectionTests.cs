// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using Xunit;

    public class MetadataCollectionTests
    {
        [Fact]
        public void Constructor_throws_ArgumentException_for_null_collection_item()
        {
            var items = new[]
            {
                new EntityType("E0", "N", DataSpace.CSpace),
                null,
                new EntityType("E2", "N", DataSpace.CSpace)
            };

            Assert.Equal(
                Strings.ADP_CollectionParameterElementIsNull("items"),
                Assert.Throws<ArgumentException>(() => new MetadataCollection<MetadataItem>(items)).Message);
        }

        [Fact]
        public void Identity_dictionaries_are_created_lazily_once()
        {
            var collection = CreateEntityTypeCollection(MetadataCollection<EntityType>.UseDictionaryCrossover + 1);

            Assert.False(collection.HasCaseSensitiveDictionary);
            Assert.False(collection.HasCaseInsensitiveDictionary);

            var dictionary1 = collection.GetCaseSensitiveDictionary();
            var dictionary2 = collection.GetCaseSensitiveDictionary();

            Assert.Same(dictionary1, dictionary2);
            Assert.True(collection.HasCaseSensitiveDictionary);
            Assert.False(collection.HasCaseInsensitiveDictionary);

            var dictionary3 = collection.GetCaseInsensitiveDictionary();
            var dictionary4 = collection.GetCaseInsensitiveDictionary();

            Assert.Same(dictionary3, dictionary4);
            Assert.True(collection.HasCaseSensitiveDictionary);
            Assert.True(collection.HasCaseInsensitiveDictionary);
        }

        [Fact]
        public void Add_throws_ArgumentException_for_duplicate_identity()
        {
            var metadataCollection = new MetadataCollection<EntityType>();
            var item = new EntityType("E", "N", DataSpace.CSpace);
            metadataCollection.Add(item);
            Assert.Equal(1, metadataCollection.Count);

            Assert.Contains(
                Strings.ItemDuplicateIdentity(item.Identity),
                Assert.Throws<ArgumentException>(
                    () => metadataCollection.Add(item)).Message);

            Assert.Contains(
                Strings.ItemDuplicateIdentity(item.Identity),
                Assert.Throws<ArgumentException>(
                    () => metadataCollection.Add(
                        new EntityType("E", "N", DataSpace.CSpace))).Message);
        }

        [Fact]
        public void AddRange_can_add_items_to_readonly_collection()
        {
            var metadataCollection = new MetadataCollection<EntityType>();
            var item = new EntityType("E", "N", DataSpace.CSpace);
            metadataCollection.Add(item);
            Assert.Equal(1, metadataCollection.Count);

            metadataCollection.SetReadOnly();
            Assert.True(metadataCollection.IsReadOnly);

            metadataCollection.AddRange(
                new List<EntityType>
                {
                    new EntityType("F", "N", DataSpace.CSpace),
                    new EntityType("G", "N", DataSpace.CSpace)
                });

            Assert.Equal(3, metadataCollection.Count);
            Assert.True(metadataCollection.IsReadOnly);
        }

        [Fact]
        public void AddRange_throws_ArgumentNullException_for_null_collection_item()
        {
            var metadataCollection = new MetadataCollection<MetadataItem>();

            Assert.Equal(
                Strings.ADP_CollectionParameterElementIsNull("items"),
                Assert.Throws<ArgumentException>(
                    () => metadataCollection.AddRange(
                        new List<MetadataItem>
                            {
                                null
                            })).Message);
        }

        [Fact]
        public void AddRange_throws_ArgumentException_for_duplicate_identity()
        {
            var metadataCollection = new MetadataCollection<EntityType>();
            var item = new EntityType("E", "N", DataSpace.CSpace);
            metadataCollection.Add(item);
            Assert.Equal(1, metadataCollection.Count);

            Assert.Contains(
                Strings.ItemDuplicateIdentity(item.Identity),
                Assert.Throws<ArgumentException>(
                    () => metadataCollection.AddRange(
                        new List<EntityType>
                            {
                                item
                            })).Message);

            Assert.Contains(
                Strings.ItemDuplicateIdentity(item.Identity),
                Assert.Throws<ArgumentException>(
                    () => metadataCollection.AddRange(
                        new List<EntityType>
                            {
                                new EntityType("E", "N", DataSpace.CSpace)
                            })).Message);
        }

        [Fact]
        public void AddRange_throws_ArgumentNullException_for_null_identity()
        {
            var metadataCollection = new MetadataCollection<EntityType>();

            Assert.Equal(
                new ArgumentNullException("name").Message,
                Assert.Throws<ArgumentNullException>(
                    () => metadataCollection.AddRange(
                        new List<EntityType>
                        {
                            new EntityType(null, null, DataSpace.CSpace)
                        })).Message);
        }

        [Fact]
        public void Add_throws_ArgumentException_for_null_identity()
        {
            var metadataCollection = new MetadataCollection<EntityType>();

            Assert.Equal(
                new ArgumentNullException("name").Message,
                Assert.Throws<ArgumentNullException>(
                    () => metadataCollection.Add(
                        new EntityType(null, null, DataSpace.CSpace))).Message);
        }

        [Fact]
        public void Modifiers_throw_when_collection_is_readonly()
        {
            var metadataCollection = new MetadataCollection<EntityType>();
            var item = new EntityType("E", "N", DataSpace.CSpace);
            metadataCollection.Add(item);
            Assert.Equal(1, metadataCollection.Count);

            metadataCollection.SetReadOnly();
            Assert.True(metadataCollection.IsReadOnly);

            Assert.Equal(
                Strings.OperationOnReadOnlyCollection,
                Assert.Throws<InvalidOperationException>(
                    () => metadataCollection.Add(
                        new EntityType("E", "N", DataSpace.CSpace))).Message);

            Assert.Equal(
                Strings.OperationOnReadOnlyCollection,
                Assert.Throws<InvalidOperationException>(
                    () => metadataCollection.Remove(item)).Message);

            Assert.Equal(
                Strings.OperationOnReadOnlyCollection,
                Assert.Throws<InvalidOperationException>(
                    () => metadataCollection[0] = item).Message);
        }

        [Fact]
        public void Can_get_IndexOf_item()
        {
            var items = new []
            {
                new EntityType("E0", "N", DataSpace.CSpace),
                new EntityType("E1", "N", DataSpace.CSpace),
                new EntityType("E2", "N", DataSpace.CSpace),
                new EntityType("E3", "N", DataSpace.CSpace),
                new EntityType("E4", "N", DataSpace.CSpace)
            };

            var collection = new MetadataCollection<EntityType>(items);

            for (var i = 0; i < items.Length; i++)
            {
                Assert.Equal(i, collection.IndexOf(items[i]));
            }
        }

        [Fact]
        public void Can_retrieve_item_by_index_from_list()
        {
            var collection = CreateEntityTypeCollection(MetadataCollection<EntityType>.UseDictionaryCrossover + 1);

            for (var i = 0; i < collection.Count; i++)
            {
                Assert.Equal("N.E" + i, collection[i].Identity);
            }

            Assert.False(collection.HasCaseSensitiveDictionary);
            Assert.False(collection.HasCaseInsensitiveDictionary);
        }

        [Fact]
        public void Can_retrieve_item_by_case_sensitive_identity_from_list()
        {
            var collection = CreateEntityTypeCollection(MetadataCollection<EntityType>.UseDictionaryCrossover);

            for (var i = 0; i < collection.Count; i++)
            {
                var identity = "N.E" + i;
                EntityType item;

                Assert.Same(collection[i], collection[identity]);
                Assert.Same(collection[i], collection.GetValue(identity, false));
                Assert.True(collection.TryGetValue(identity, false, out item));
                Assert.Same(collection[i], item);
            }

            Assert.False(collection.HasCaseSensitiveDictionary);
            Assert.False(collection.HasCaseInsensitiveDictionary);
        }

        [Fact]
        public void Can_retrieve_item_by_case_insensitive_identity_from_list()
        {
            var collection = CreateEntityTypeCollection(MetadataCollection<EntityType>.UseDictionaryCrossover);

            for (var i = 0; i < collection.Count; i++)
            {
                var identity = "n.e" + i;
                EntityType item;

                Assert.Same(collection[i], collection.GetValue(identity, true));
                Assert.True(collection.TryGetValue(identity, true, out item));
                Assert.Same(collection[i], item);
            }

            Assert.False(collection.HasCaseSensitiveDictionary);
            Assert.False(collection.HasCaseInsensitiveDictionary);
        }

        [Fact]
        public void Can_retrieve_item_by_case_sensitive_identity_from_dictionary()
        {
            var collection = CreateEntityTypeCollection(MetadataCollection<EntityType>.UseDictionaryCrossover + 1);

            for (var i = 0; i < collection.Count; i++)
            {
                var identity = "N.E" + i;
                EntityType item;

                Assert.Same(collection[i], collection[identity]);
                Assert.Same(collection[i], collection.GetValue(identity, false));
                Assert.True(collection.TryGetValue(identity, false, out item));
                Assert.Same(collection[i], item);
            }

            Assert.True(collection.HasCaseSensitiveDictionary);
            Assert.False(collection.HasCaseInsensitiveDictionary);
        }

        [Fact]
        public void Can_retrieve_item_by_case_insensitive_identity_from_dictionary()
        {
            var collection = CreateEntityTypeCollection(MetadataCollection<EntityType>.UseDictionaryCrossover + 1);

            for (var i = 0; i < collection.Count; i++)
            {
                var identity = "n.e" + i;
                EntityType item;

                Assert.Same(collection[i], collection.GetValue(identity, true));
                Assert.True(collection.TryGetValue(identity, true, out item));
                Assert.Same(collection[i], item);
            }

            Assert.False(collection.HasCaseSensitiveDictionary);
            Assert.True(collection.HasCaseInsensitiveDictionary);
        }

        [Fact]
        public void Retrieving_item_by_identity_throws_if_not_found()
        {
            var collection = CreateEntityTypeCollection(MetadataCollection<EntityType>.UseDictionaryCrossover);

            var identity = "no found";

            Assert.Equal(
                new ArgumentException(Strings.ItemInvalidIdentity(identity), "identity").Message,
                Assert.Throws<ArgumentException>(() => collection[identity]).Message);

            Assert.Equal(
                new ArgumentException(Strings.ItemInvalidIdentity(identity), "identity").Message,
                Assert.Throws<ArgumentException>(() => collection.GetValue(identity, false)).Message);

            Assert.Equal(
                new ArgumentException(Strings.ItemInvalidIdentity(identity), "identity").Message,
                Assert.Throws<ArgumentException>(() => collection.GetValue(identity, true)).Message);

            Assert.False(collection.HasCaseSensitiveDictionary);
            Assert.False(collection.HasCaseInsensitiveDictionary);

            collection.Add(new EntityType("E", "N", DataSpace.CSpace));

            Assert.Equal(
                new ArgumentException(Strings.ItemInvalidIdentity(identity), "identity").Message,
                Assert.Throws<ArgumentException>(() => collection[identity]).Message);

            Assert.Equal(
                new ArgumentException(Strings.ItemInvalidIdentity(identity), "identity").Message,
                Assert.Throws<ArgumentException>(() => collection.GetValue(identity, false)).Message);

            Assert.Equal(
                new ArgumentException(Strings.ItemInvalidIdentity(identity), "identity").Message,
                Assert.Throws<ArgumentException>(() => collection.GetValue(identity, true)).Message);

            Assert.True(collection.HasCaseSensitiveDictionary);
            Assert.True(collection.HasCaseInsensitiveDictionary);
        }

        [Fact]
        public void TryGetValue_returns_false_if_identity_not_found()
        {
            var collection = CreateEntityTypeCollection(MetadataCollection<EntityType>.UseDictionaryCrossover);

            var identity = "no found";

            EntityType item;
            Assert.False(collection.TryGetValue(identity, false, out item));
            Assert.False(collection.TryGetValue(identity, true, out item));

            Assert.False(collection.HasCaseSensitiveDictionary);
            Assert.False(collection.HasCaseInsensitiveDictionary);

            collection.Add(new EntityType("E", "N", DataSpace.CSpace));
            Assert.False(collection.TryGetValue(identity, false, out item));
            Assert.False(collection.TryGetValue(identity, true, out item));

            Assert.True(collection.HasCaseSensitiveDictionary);
            Assert.True(collection.HasCaseInsensitiveDictionary);            
        }

        [Fact]
        public void Can_check_if_collection_contains_item()
        {
            var collection = CreateEntityTypeCollection(MetadataCollection<EntityType>.UseDictionaryCrossover);

            foreach (var item in collection)
            {
                Assert.True(collection.Contains(item));
            }

            Assert.False(collection.Contains(new EntityType("E0", "N", DataSpace.CSpace)));

            var newItem = new EntityType("F", "N", DataSpace.CSpace);

            Assert.False(collection.Contains(newItem));

            collection.Add(newItem);

            Assert.False(collection.HasCaseSensitiveDictionary);
            Assert.False(collection.HasCaseInsensitiveDictionary);

            Assert.False(collection.Contains(new EntityType("E0", "N", DataSpace.CSpace)));
            Assert.True(collection.Contains(newItem));

            Assert.True(collection.HasCaseSensitiveDictionary);
            Assert.False(collection.HasCaseInsensitiveDictionary);
        }

        [Fact]
        public void Can_check_if_collection_contains_item_with_specified_identity()
        {
            var items = new[]
            {
                new EntityType("E0", "N", DataSpace.CSpace),
                new EntityType("E1", "N", DataSpace.CSpace),
                new EntityType("E2", "N", DataSpace.CSpace),
                new EntityType("E3", "N", DataSpace.CSpace),
                new EntityType("E4", "N", DataSpace.CSpace)
            };

            var collection = new MetadataCollection<EntityType>(items);

            for (var i = 0; i < collection.Count; i++)
            {
                Assert.True(collection.ContainsIdentity("N.E" + i));
            }

            Assert.False(collection.ContainsIdentity("missing"));
        }

        [Fact]
        public void Can_set_item_at_index()
        {
            var collection = CreateEntityTypeCollection(MetadataCollection<EntityType>.UseDictionaryCrossover + 1);

            for (var i = 0; i < collection.Count; i++)
            {
                var entityType = new EntityType("F" + i, "N", DataSpace.CSpace);
                collection[i] = entityType;

                Assert.Same(entityType, collection[i]);
            }

            Assert.False(collection.HasCaseSensitiveDictionary);
            Assert.False(collection.HasCaseInsensitiveDictionary);
        }

        [Fact]
        public void Dictionaries_are_updated_when_setting_item_at_index()
        {
            var collection = CreateEntityTypeCollection(MetadataCollection<EntityType>.UseDictionaryCrossover + 1);

            collection.GetValue(collection[0].Identity, false);
            collection.GetValue(collection[0].Identity, true);

            Assert.True(collection.HasCaseSensitiveDictionary);
            Assert.True(collection.HasCaseInsensitiveDictionary);

            var entityType = new EntityType("F3", "N", DataSpace.CSpace);
            collection[3] = entityType;

            Assert.True(collection.HasCaseSensitiveDictionary);
            Assert.False(collection.HasCaseInsensitiveDictionary);

            Assert.Same(entityType, collection[entityType.Identity]);
        }

        [Fact]
        public void Can_remove_item_from_collection()
        {
            var metadataCollection = new MetadataCollection<EntityType>();

            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            metadataCollection.Add(entityType);

            Assert.Equal(1, metadataCollection.Count);

            metadataCollection.Remove(entityType);

            Assert.Empty(metadataCollection);
        }

        [Fact]
        public void Dictionaries_are_updated_when_removing_item()
        {
            var collection = CreateEntityTypeCollection(MetadataCollection<EntityType>.UseDictionaryCrossover + 2);

            collection.GetValue(collection[0].Identity, false);
            collection.GetValue(collection[0].Identity, true);

            Assert.True(collection.HasCaseSensitiveDictionary);
            Assert.True(collection.HasCaseInsensitiveDictionary);

            var item = collection[3];
            Assert.True(collection.Remove(item));

            Assert.True(collection.HasCaseSensitiveDictionary);
            Assert.False(collection.HasCaseInsensitiveDictionary);

            Assert.False(collection.TryGetValue(item.Identity, true, out item));
        }

        [Fact]
        public void Dictionaries_are_updated_when_HandleIdentityChange_is_called()
        {
            var members = new List<EdmProperty>();

            for (var i = 0; i < MetadataCollection<EntityType>.UseDictionaryCrossover + 1; i++)
            {
                members.Add(EdmProperty.CreatePrimitive("P" + i, PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)));
            }

            var collection = new MetadataCollection<EdmProperty>(members);
            Assert.Equal(members.Count, collection.Count);

            collection.GetValue(collection[0].Identity, false);
            collection.GetValue(collection[0].Identity, true);

            Assert.True(collection.HasCaseSensitiveDictionary);
            Assert.True(collection.HasCaseInsensitiveDictionary);

            var item = collection[3];
            item.Name = "R3";
            collection.HandleIdentityChange(item, "P3");

            Assert.True(collection.HasCaseSensitiveDictionary);
            Assert.False(collection.HasCaseInsensitiveDictionary);

            Assert.Same(item, collection["R3"]);
        }

        private static MetadataCollection<EntityType> CreateEntityTypeCollection(int count)
        {
            var entityTypes = new List<EntityType>();

            for (var i = 0; i < count; i++)
            {
                entityTypes.Add(new EntityType("E" + i, "N", DataSpace.CSpace));
            }

            var collection = MetadataCollection<EntityType>.Wrap(entityTypes);
            Assert.Equal(count, collection.Count);

            return collection;
        }
    }
}
