// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects.Internal;
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

                entityCollectionMock.Object.LoadAsync(mergeOption).Wait();

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
