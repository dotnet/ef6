// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class EntityCollectionTests
    {
        public class Load
        {
            [Fact]
            public void Calls_collection_override_passing_in_null_for_all_merge_options()
            {
                foreach (MergeOption option in Enum.GetValues(typeof(MergeOption)))
                {
                    Calls_collection_override_passing_in_null(option);
                }
            }

            private void Calls_collection_override_passing_in_null(MergeOption mergeOption)
            {
                var entityCollectionMock = MockHelper.CreateMockEntityCollection<object>(null);

                var timesLoadCalled = 0;
                entityCollectionMock.Setup(m => m.Load(It.IsAny<List<IEntityWrapper>>(), It.IsAny<MergeOption>()))
                    .Callback(
                        (IEnumerable<object> actualCollection, MergeOption actualMergeOption) =>
                            {
                                timesLoadCalled++;
                                Assert.Equal(null, actualCollection);
                                Assert.Equal(mergeOption, actualMergeOption);
                            });

                var timesCheckOwnerNullCalled = 0;
                entityCollectionMock.Setup(m => m.CheckOwnerNull())
                    .Callback(() => { timesCheckOwnerNullCalled++; });

                entityCollectionMock.Object.Load(mergeOption);

                entityCollectionMock.Verify(m => m.Load(It.IsAny<List<IEntityWrapper>>(), It.IsAny<MergeOption>()));

                Assert.True(1 == timesLoadCalled, "Expected Load to be called once for MergeOption." + mergeOption);
                Assert.True(1 == timesCheckOwnerNullCalled, "Expected CheckOwnerNull to be called once for MergeOption." + mergeOption);
            }

            [Fact]
            public void Calls_merge_passing_in_expected_values_for_all_merge_options()
            {
                foreach (MergeOption option in Enum.GetValues(typeof(MergeOption)))
                {
                    Calls_merge_passing_in_expected_values(null, option, null);
                    Calls_merge_passing_in_expected_values(null, option, new object());
                    Calls_merge_passing_in_expected_values(new[] { new Mock<IEntityWrapper>().Object }.ToList(), option, null);
                }
            }

            private void Calls_merge_passing_in_expected_values(
                List<IEntityWrapper> collection, MergeOption mergeOption, object refreshedValue)
            {
                var entityCollectionMock = MockHelper.CreateMockEntityCollection(refreshedValue);

                var timesMergeCalled = 0;
                if (collection == null)
                {
                    entityCollectionMock.Setup(m => m.Merge(It.IsAny<IEnumerable<object>>(), It.IsAny<MergeOption>(), true))
                        .Callback(
                            (IEnumerable<object> actualCollection, MergeOption actualMergeOption, bool setIsLoaded) =>
                                {
                                    timesMergeCalled++;

                                    Assert.Equal(
                                        refreshedValue == null ? Enumerable.Empty<object>() : new[] { refreshedValue }, actualCollection);
                                    Assert.Equal(mergeOption, actualMergeOption);
                                });
                }
                else
                {
                    entityCollectionMock.Setup(m => m.Merge<object>(It.IsAny<List<IEntityWrapper>>(), It.IsAny<MergeOption>(), true))
                        .Callback(
                            (List<IEntityWrapper> actualCollection, MergeOption actualMergeOption, bool setIsLoaded) =>
                                {
                                    timesMergeCalled++;

                                    Assert.Equal(collection, actualCollection);
                                    Assert.Equal(mergeOption, actualMergeOption);
                                });
                }

                entityCollectionMock.Object.Load(collection, mergeOption);

                Assert.True(
                    1 == timesMergeCalled,
                    string.Format(
                        "Expected Merge to be called once for MergeOption.{0}, {1} collection and {2} refreshed value.",
                        mergeOption,
                        collection == null ? "null" : "not null",
                        refreshedValue == null ? "null" : "not null"));
            }

            [Fact]
            public void Add_generic_throws_for_null_argument()
            {
                Assert.Throws<ArgumentNullException>(
                    () => MockHelper.CreateMockEntityCollection<string>(null).Object.Add(null));
            }

            [Fact]
            public void Add_object_throws_for_null_argument()
            {
                Assert.Throws<ArgumentNullException>(
                    () => ((IRelatedEnd)MockHelper.CreateMockEntityCollection<string>(null).Object).Add((object)null));
            }

            [Fact]
            public void Add_IEntityWithRelationships_throws_for_null_argument()
            {
                Assert.Throws<ArgumentNullException>(
                    () => ((IRelatedEnd)MockHelper.CreateMockEntityCollection<string>(null).Object).Add(null));
            }

            [Fact]
            public void Remove_throws_for_null_argument()
            {
                Assert.Throws<ArgumentNullException>(
                    () => MockHelper.CreateMockEntityCollection<object>(null).Object.Remove(null));
            }
        }

        public class CheckIfNavigationPropertyContainsEntity
        {
            [Fact]
            public void Check_returns_false_if_nav_property_does_not_exist()
            {
                var mockOwner = new Mock<IEntityWrapper>();
                mockOwner.Setup(m => m.Entity).Returns(new object());

                var collection = new EntityCollection<object>(
                    mockOwner.Object, 
                    new RelationshipNavigation(
                        "relationship", "from", "to", new NavigationPropertyAccessor(null), new NavigationPropertyAccessor(null)),
                    new Mock<IRelationshipFixer>().Object);

                Assert.False(collection.CheckIfNavigationPropertyContainsEntity(new Mock<IEntityWrapper>().Object));
            }

            [Fact]
            public void Check_returns_false_if_nav_property_is_null()
            {
                var mockTarget = new Mock<IEntityWrapper>();
                var collection = CreateCollectionForContains(null, new object(), mockTarget);

                Assert.False(collection.CheckIfNavigationPropertyContainsEntity(mockTarget.Object));
            }

            [Fact]
            public void Check_throws_for_non_collection_nav_props()
            {
                var mockTarget = new Mock<IEntityWrapper>();
                var collection = CreateCollectionForContains(new object(), new object(), mockTarget);

                Assert.Equal(
                    Strings.ObjectStateEntry_UnableToEnumerateCollection("to", "System.Object"),
                    Assert.Throws<EntityException>(() => collection.CheckIfNavigationPropertyContainsEntity(mockTarget.Object)).Message);
            }

            [Fact]
            public void Check_returns_false_if_nav_property_does_not_contain_entity()
            {
                var mockTarget = new Mock<IEntityWrapper>();
                var collection = CreateCollectionForContains(new HashSet<object>(), new object(), mockTarget);

                Assert.False(collection.CheckIfNavigationPropertyContainsEntity(mockTarget.Object));
            }

            [Fact]
            public void Check_returns_true_if_nav_property_contains_entity()
            {
                var containedEntity = new object();
                var navigationCollection = new HashSet<object> { containedEntity };
                var mockTarget = new Mock<IEntityWrapper>();

                var collection = CreateCollectionForContains(navigationCollection, containedEntity, mockTarget);

                Assert.True(collection.CheckIfNavigationPropertyContainsEntity(mockTarget.Object));
            }

            public class MyHashSet<T> : HashSet<T>, ICollection<T>
            {
                public MyHashSet()
                {
                }

                public MyHashSet(IEqualityComparer<T> comparer)
                    : base(comparer)
                {
                }

                public virtual new bool Contains(T obj)
                {
                    return false;
                }
            }

            [Fact]
            public void Check_uses_Contains_if_target_object_does_not_override_GetHashCode_or_Equals()
            {
                var mockCollection = new Mock<ICollection<object>>();

                var mockTarget = new Mock<IEntityWrapper>();
                var collection = CreateCollectionForContains(mockCollection.Object, new object(), mockTarget);

                collection.CheckIfNavigationPropertyContainsEntity(mockTarget.Object);

                mockCollection.Verify(m => m.Contains(It.IsAny<object>()));
            }

            [Fact]
            public void Check_does_not_use_Contains_if_target_object_overrides_GetHashCode_or_Equals()
            {
                var mockCollection = new Mock<MyHashSet<object>>();

                var mockTarget = new Mock<IEntityWrapper>();
                var collection = CreateCollectionForContains(mockCollection.Object, new object(), mockTarget, true);

                collection.CheckIfNavigationPropertyContainsEntity(mockTarget.Object);

                mockCollection.Verify(m => m.Contains(It.IsAny<object>()), Times.Never());
            }

            [Fact]
            public void Check_uses_Contains_if_ObjectReferenceEqualityComparer_is_used_even_if_GetHashCode_or_Equals_overridden()
            {
                var mockCollection = new Mock<MyHashSet<object>>(new ObjectReferenceEqualityComparer());

                var mockTarget = new Mock<IEntityWrapper>();
                var collection = CreateCollectionForContains(mockCollection.Object, new object(), mockTarget, true);

                collection.CheckIfNavigationPropertyContainsEntity(mockTarget.Object);

                mockCollection.Verify(m => m.Contains(It.IsAny<object>()));
            }

            private static EntityCollection<object> CreateCollectionForContains(
                object navigationCollection, object containedEntity, Mock<IEntityWrapper> mockTarget, bool overridesEquals = false)
            {
                mockTarget.Setup(m => m.Entity).Returns(containedEntity);
                mockTarget.Setup(m => m.OverridesEqualsOrGetHashCode).Returns(overridesEquals);

                var mockOwner = new Mock<IEntityWrapper>();
                mockOwner.Setup(m => m.Entity).Returns(new object());
                mockOwner.Setup(m => m.GetNavigationPropertyValue(It.IsAny<RelatedEnd>())).Returns(navigationCollection);

                var collection = new EntityCollection<object>(
                    mockOwner.Object, 
                    new RelationshipNavigation(
                        "relationship", "from", "to", new NavigationPropertyAccessor("from"), new NavigationPropertyAccessor("to")),
                    new Mock<IRelationshipFixer>().Object);

                return collection;
            }
        }

#if !NET40

        public class LoadAsync
        {
            [Fact]
            public void Calls_collection_override_passing_in_null_for_all_merge_options()
            {
                foreach (MergeOption option in Enum.GetValues(typeof(MergeOption)))
                {
                    Calls_collection_override_passing_in_null(option);
                }
            }

            private void Calls_collection_override_passing_in_null(MergeOption mergeOption)
            {
                var entityCollectionMock = MockHelper.CreateMockEntityCollection<object>(null);

                var timesLoadCalled = 0;
                entityCollectionMock.Setup(
                    m => m.LoadAsync(It.IsAny<List<IEntityWrapper>>(), It.IsAny<MergeOption>(), It.IsAny<CancellationToken>()))
                    .Returns(
                        (IEnumerable<object> actualCollection, MergeOption actualMergeOption, CancellationToken cancellationToken) =>
                            {
                                timesLoadCalled++;
                                Assert.Equal(null, actualCollection);
                                Assert.Equal(mergeOption, actualMergeOption);
                                return Task.FromResult<object>(null);
                            });

                var timesCheckOwnerNullCalled = 0;
                entityCollectionMock.Setup(m => m.CheckOwnerNull())
                    .Callback(() => { timesCheckOwnerNullCalled++; });

                entityCollectionMock.Object.LoadAsync(mergeOption, CancellationToken.None).Wait();

                entityCollectionMock.Verify(
                    m => m.LoadAsync(It.IsAny<List<IEntityWrapper>>(), It.IsAny<MergeOption>(), It.IsAny<CancellationToken>()));

                Assert.True(1 == timesLoadCalled, "Expected Load to be called once for MergeOption." + mergeOption);
                Assert.True(1 == timesCheckOwnerNullCalled, "Expected CheckOwnerNull to be called once for MergeOption." + mergeOption);
            }

            [Fact]
            public void Calls_merge_passing_in_expected_values_for_all_merge_options()
            {
                foreach (MergeOption option in Enum.GetValues(typeof(MergeOption)))
                {
                    Calls_merge_passing_in_expected_values(null, option, null);
                    Calls_merge_passing_in_expected_values(null, option, new object());
                    Calls_merge_passing_in_expected_values(new[] { new Mock<IEntityWrapper>().Object }.ToList(), option, null);
                }
            }

            private void Calls_merge_passing_in_expected_values(
                List<IEntityWrapper> collection, MergeOption mergeOption, object refreshedValue)
            {
                var entityCollectionMock = MockHelper.CreateMockEntityCollection(refreshedValue);

                var timesMergeCalled = 0;
                if (collection == null)
                {
                    entityCollectionMock.Setup(m => m.Merge(It.IsAny<IEnumerable<object>>(), It.IsAny<MergeOption>(), true))
                        .Callback(
                            (IEnumerable<object> actualCollection, MergeOption actualMergeOption, bool setIsLoaded) =>
                                {
                                    timesMergeCalled++;

                                    Assert.Equal(
                                        refreshedValue == null ? Enumerable.Empty<object>() : new[] { refreshedValue }, actualCollection);
                                    Assert.Equal(mergeOption, actualMergeOption);
                                });
                }
                else
                {
                    entityCollectionMock.Setup(m => m.Merge<object>(It.IsAny<List<IEntityWrapper>>(), It.IsAny<MergeOption>(), true))
                        .Callback(
                            (List<IEntityWrapper> actualCollection, MergeOption actualMergeOption, bool setIsLoaded) =>
                                {
                                    timesMergeCalled++;

                                    Assert.Equal(collection, actualCollection);
                                    Assert.Equal(mergeOption, actualMergeOption);
                                });
                }

                entityCollectionMock.Object.LoadAsync(collection, mergeOption, CancellationToken.None);

                Assert.True(
                    1 == timesMergeCalled,
                    string.Format(
                        "Expected Merge to be called once for MergeOption.{0}, {1} collection and {2} refreshed value.",
                        mergeOption,
                        collection == null ? "null" : "not null",
                        refreshedValue == null ? "null" : "not null"));
            }

            [Fact]
            public void Add_generic_throws_for_null_argument()
            {
                Assert.Throws<ArgumentNullException>(
                    () => MockHelper.CreateMockEntityCollection<string>(null).Object.Add(null));
            }

            [Fact]
            public void Add_object_throws_for_null_argument()
            {
                Assert.Throws<ArgumentNullException>(
                    () => ((IRelatedEnd)MockHelper.CreateMockEntityCollection<string>(null).Object).Add((object)null));
            }

            [Fact]
            public void Add_IEntityWithRelationships_throws_for_null_argument()
            {
                Assert.Throws<ArgumentNullException>(
                    () => ((IRelatedEnd)MockHelper.CreateMockEntityCollection<string>(null).Object).Add(null));
            }

            [Fact]
            public void Remove_throws_for_null_argument()
            {
                Assert.Throws<ArgumentNullException>(
                    () => MockHelper.CreateMockEntityCollection<object>(null).Object.Remove(null));
            }
        }

#endif
    }
}
