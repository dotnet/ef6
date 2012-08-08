// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class InternalCollectionEntryTests
    {
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
