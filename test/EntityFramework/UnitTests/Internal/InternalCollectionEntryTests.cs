// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Resources;
    using System.Threading;
    using Xunit;

    public class InternalCollectionEntryTests
    {
        public class CurrentValue
        {
            [Fact]
            public void InternalCollectionEntry_gets_current_value_from_entity_if_property_exists()
            {
                InternalCollectionEntry_gets_current_value_from_entity_if_property_exists_implementation(isDetached: false);
            }

            [Fact]
            public void InternalCollectionEntry_gets_current_value_from_entity_even_when_detached_if_property_exists()
            {
                InternalCollectionEntry_gets_current_value_from_entity_if_property_exists_implementation(isDetached: true);
            }

            private void InternalCollectionEntry_gets_current_value_from_entity_if_property_exists_implementation(bool isDetached)
            {
                var relatedCollection = new List<FakeEntity>();
                var entity = new FakeWithProps
                                 {
                                     Collection = relatedCollection
                                 };
                var internalEntry = new InternalCollectionEntry(
                    MockHelper.CreateMockInternalEntityEntry(
                        entity, isDetached).Object, FakeWithProps.CollectionMetadata);

                var propValue = internalEntry.CurrentValue;

                Assert.Same(relatedCollection, propValue);
            }

            [Fact]
            public void InternalCollectionEntry_sets_current_value_onto_entity_if_property_exists()
            {
                InternalCollectionEntry_sets_current_value_onto_entity_if_property_exists_implementation(isDetached: false);
            }

            [Fact]
            public void InternalCollectionEntry_sets_current_value_onto_entity_even_when_detached_if_property_exists()
            {
                InternalCollectionEntry_sets_current_value_onto_entity_if_property_exists_implementation(isDetached: true);
            }

            private void InternalCollectionEntry_sets_current_value_onto_entity_if_property_exists_implementation(bool isDetached)
            {
                var entity = new FakeWithProps
                                 {
                                     Collection = new List<FakeEntity>()
                                 };
                var internalEntry = new InternalCollectionEntry(
                    MockHelper.CreateMockInternalEntityEntry(
                        entity, isDetached).Object, FakeWithProps.CollectionMetadata);

                var relatedCollection = new List<FakeEntity>();
                internalEntry.CurrentValue = relatedCollection;

                Assert.Same(relatedCollection, entity.Collection);
            }

            [Fact]
            public void
                InternalCollectionEntry_gets_current_value_that_is_the_RelatedEnd_if_navigation_property_has_been_removed_from_entity()
            {
                var relatedCollection = new EntityCollection<FakeEntity>();
                var internalEntry =
                    new InternalCollectionEntry(
                        MockHelper.CreateMockInternalEntityEntry(new FakeEntity(), entityCollection: relatedCollection).Object,
                        FakeWithProps.CollectionMetadata);

                var propValue = internalEntry.CurrentValue;

                Assert.Same(relatedCollection, propValue);
            }

            [Fact]
            public void
                InternalCollectionEntry_does_nothing_if_attempting_to_set_the_actual_EntityCollection_as_a_current_value_when_navigation_property_has_been_removed_from_entity
                ()
            {
                var relatedCollection = new EntityCollection<FakeEntity>();
                var internalEntry =
                    new InternalCollectionEntry(
                        MockHelper.CreateMockInternalEntityEntry(new FakeEntity(), entityCollection: relatedCollection).Object,
                        FakeWithProps.CollectionMetadata);

                internalEntry.CurrentValue = relatedCollection; // Test that it doesn't throw
            }

            [Fact]
            public void
                InternalCollectionEntry_throws_when_attempting_to_set_a_new_collection_when_navigation_property_has_been_removed_from_entity
                ()
            {
                var internalEntry =
                    new InternalCollectionEntry(
                        MockHelper.CreateMockInternalEntityEntry(new FakeEntity(), entityCollection: new EntityCollection<FakeEntity>()).
                            Object, FakeWithProps.CollectionMetadata);

                Assert.Equal(
                    Strings.DbCollectionEntry_CannotSetCollectionProp("Collection", typeof(FakeEntity).ToString()),
                    Assert.Throws<NotSupportedException>(() => internalEntry.CurrentValue = new List<FakeEntity>()).Message);
            }

            [Fact]
            public void InternalCollectionEntry_throws_getting_current_value_from_detached_entity_if_navigation_property_has_been_removed()
            {
                var mockInternalEntry = MockHelper.CreateMockInternalEntityEntry(
                    new FakeEntity(), isDetached: true);
                var internalEntry = new InternalCollectionEntry(mockInternalEntry.Object, FakeWithProps.CollectionMetadata);

                Assert.Equal(
                    Strings.DbPropertyEntry_NotSupportedForDetached("CurrentValue", "Collection", "FakeEntity"),
                    Assert.Throws<InvalidOperationException>(() => { var _ = internalEntry.CurrentValue; }).Message);
            }

            [Fact]
            public void InternalCollectionEntry_throws_setting_current_value_from_detached_entity_if_navigation_property_has_been_removed()
            {
                var mockInternalEntry = MockHelper.CreateMockInternalEntityEntry(
                    new FakeEntity(), isDetached: true);
                var internalEntry = new InternalCollectionEntry(mockInternalEntry.Object, FakeWithProps.CollectionMetadata);

                Assert.Equal(
                    Strings.DbCollectionEntry_CannotSetCollectionProp("Collection", typeof(FakeEntity).ToString()),
                    Assert.Throws<NotSupportedException>(() => { internalEntry.CurrentValue = null; }).Message);
            }
        }

        public class Load
        {
            [Fact]
            public void InternalCollectionEntry_Load_throws_if_used_with_Detached_entity()
            {
                var mockInternalEntry = MockHelper.CreateMockInternalEntityEntry(
                    new FakeEntity(), isDetached: true);
                var internalEntry = new InternalCollectionEntry(mockInternalEntry.Object, FakeWithProps.CollectionMetadata);

                Assert.Equal(
                    Strings.DbPropertyEntry_NotSupportedForDetached("Load", "Collection", "FakeEntity"),
                    Assert.Throws<InvalidOperationException>(() => internalEntry.Load()).Message);
            }
        }

#if !NET40

        public class LoadAsync
        {
            [Fact]
            public void InternalCollectionEntry_LoadAsync_throws_if_used_with_Detached_entity()
            {
                var mockInternalEntry = MockHelper.CreateMockInternalEntityEntry(
                    new FakeEntity(), isDetached: true);
                var internalEntry = new InternalCollectionEntry(mockInternalEntry.Object, FakeWithProps.CollectionMetadata);

                Assert.Equal(
                    Strings.DbPropertyEntry_NotSupportedForDetached("LoadAsync", "Collection", "FakeEntity"),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        internalEntry.LoadAsync(CancellationToken.None)).Message);
            }
        }

#endif

        public class IsLoaded
        {
            [Fact]
            public void InternalCollectionEntry_IsLoaded_throws_if_used_with_Detached_entity()
            {
                var mockInternalEntry = MockHelper.CreateMockInternalEntityEntry(
                    new FakeEntity(), isDetached: true);
                var internalEntry = new InternalCollectionEntry(mockInternalEntry.Object, FakeWithProps.CollectionMetadata);

                Assert.Equal(
                    Strings.DbPropertyEntry_NotSupportedForDetached("IsLoaded", "Collection", "FakeEntity"),
                    Assert.Throws<InvalidOperationException>(() => { var _ = internalEntry.IsLoaded; }).Message);

                Assert.Equal(
                    Strings.DbPropertyEntry_NotSupportedForDetached("IsLoaded", "Collection", "FakeEntity"),
                    Assert.Throws<InvalidOperationException>(() => { internalEntry.IsLoaded = true; }).Message);
            }
        }

        public class Name
        {
            [Fact]
            public void InternalCollectionEntry_Name_works_even_when_used_with_Detached_entity()
            {
                var mockInternalEntry = MockHelper.CreateMockInternalEntityEntry(
                    new FakeEntity(), isDetached: true);
                var internalEntry = new InternalCollectionEntry(mockInternalEntry.Object, FakeWithProps.CollectionMetadata);

                Assert.Equal("Collection", internalEntry.Name);
            }
        }

        public class Query
        {
            [Fact]
            public void InternalCollectionEntry_Query_throws_if_used_with_Detached_entity()
            {
                var mockInternalEntry = MockHelper.CreateMockInternalEntityEntry(
                    new FakeEntity(), isDetached: true);
                var internalEntry = new InternalCollectionEntry(mockInternalEntry.Object, FakeWithProps.CollectionMetadata);

                Assert.Equal(
                    Strings.DbPropertyEntry_NotSupportedForDetached("Query", "Collection", "FakeEntity"),
                    Assert.Throws<InvalidOperationException>(() => internalEntry.Query()).Message);
            }
        }
    }
}
