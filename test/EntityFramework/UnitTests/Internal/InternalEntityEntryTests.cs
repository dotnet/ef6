// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Resources;
    using System.Threading;
    using Moq;
    using Xunit;

    public class InternalEntityEntryTests
    {
        public class GetDatabaseValues
        {
            [Fact]
            public void On_a_detached_entry_throws()
            {
                var mockInternalEntityEntry = new Mock<InternalEntityEntryForMock<object>>
                    {
                        CallBase = true
                    };
                mockInternalEntityEntry.Setup(e => e.OriginalValues).Returns(() => new TestInternalPropertyValues<object>());
                mockInternalEntityEntry.Setup(e => e.IsDetached).Returns(true);

                Assert.Equal(
                    Strings.DbEntityEntry_NotSupportedForDetached("GetDatabaseValues", "Object"),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        mockInternalEntityEntry.Object.GetDatabaseValues()).Message);
            }

            [Fact]
            public void On_an_added_entry_throws()
            {
                var mockInternalEntityEntry = new Mock<InternalEntityEntryForMock<object>>
                    {
                        CallBase = true
                    };
                mockInternalEntityEntry.Setup(e => e.OriginalValues).Returns(() => new TestInternalPropertyValues<object>());
                mockInternalEntityEntry.Setup(e => e.State).Returns(EntityState.Added);

                Assert.Equal(
                    Strings.DbPropertyValues_CannotGetValuesForState("GetDatabaseValues", EntityState.Added),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        mockInternalEntityEntry.Object.GetDatabaseValues()).Message);
            }
        }

#if !NET40

        public class GetDatabaseValuesAsync
        {
            [Fact]
            public void On_a_detached_entry_throws()
            {
                var mockInternalEntityEntry = new Mock<InternalEntityEntryForMock<object>>
                    {
                        CallBase = true
                    };
                mockInternalEntityEntry.Setup(e => e.OriginalValues).Returns(() => new TestInternalPropertyValues<object>());
                mockInternalEntityEntry.Setup(e => e.IsDetached).Returns(true);

                Assert.Equal(
                    Strings.DbEntityEntry_NotSupportedForDetached("GetDatabaseValuesAsync", "Object"),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        ExceptionHelpers.UnwrapAggregateExceptions(
                            () =>
                            mockInternalEntityEntry.Object.GetDatabaseValuesAsync(CancellationToken.None).Result)).Message);
            }

            [Fact]
            public void On_an_added_entry_throws()
            {
                var mockInternalEntityEntry = new Mock<InternalEntityEntryForMock<object>>
                    {
                        CallBase = true
                    };
                mockInternalEntityEntry.Setup(e => e.OriginalValues).Returns(() => new TestInternalPropertyValues<object>());
                mockInternalEntityEntry.Setup(e => e.State).Returns(EntityState.Added);

                Assert.Equal(
                    Strings.DbPropertyValues_CannotGetValuesForState("GetDatabaseValuesAsync", EntityState.Added),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        ExceptionHelpers.UnwrapAggregateExceptions(
                            () =>
                            mockInternalEntityEntry.Object.GetDatabaseValuesAsync(CancellationToken.None).Result)).Message);
            }

            [Fact]
            public void GetDatabaseValuesAsync_throws_OperationCanceledException_if_task_is_cancelled()
            {
                var internalEntityEntry = new Mock<InternalEntityEntryForMock<object>> { CallBase = true }.Object;

                Assert.Throws<OperationCanceledException>(
                    () => internalEntityEntry.GetDatabaseValuesAsync(new CancellationToken(canceled: true))
                        .GetAwaiter().GetResult());
            }
        }

        public class ReloadAsync
        {
            [Fact]
            public void ReloadAsync_throws_OperationCanceledException_if_task_is_cancelled()
            {
                var internalEntityEntry = new Mock<InternalEntityEntryForMock<object>> { CallBase = true }.Object;

                Assert.Throws<OperationCanceledException>(
                    () => internalEntityEntry.ReloadAsync(new CancellationToken(canceled: true))
                        .GetAwaiter().GetResult());
            }
        }

        public new class GetHashCode
        {
            [Fact]
            public void Doesnt_call_GetHashCode_on_entity_object()
            {
                var mockInternalEntityEntry1 = new InternalEntityEntry(
                    new Mock<InternalContextForMock>
                        {
                            CallBase = true
                        }.Object, MockHelper.CreateMockStateEntry<GetHashCodeEntity>().Object);

                var mockInternalEntityEntry2 = new InternalEntityEntry(
                    new Mock<InternalContextForMock>
                        {
                            CallBase = true
                        }.Object, MockHelper.CreateMockStateEntry<GetHashCodeEntity>().Object);

                Assert.NotEqual(0, mockInternalEntityEntry1.GetHashCode());
                Assert.NotEqual(0, mockInternalEntityEntry2.GetHashCode());

                Assert.NotEqual(mockInternalEntityEntry1.GetHashCode(), mockInternalEntityEntry2.GetHashCode());
            }
        }

        public new class Equals
        {
            [Fact]
            public void Doesnt_call_Equals_on_entity_object()
            {
                var mockInternalEntityEntry1 = new InternalEntityEntry(
                    new Mock<InternalContextForMock>
                    {
                        CallBase = true
                    }.Object, MockHelper.CreateMockStateEntry<GetHashCodeEntity>().Object);

                var mockInternalEntityEntry2 = new InternalEntityEntry(
                    new Mock<InternalContextForMock>
                    {
                        CallBase = true
                    }.Object, MockHelper.CreateMockStateEntry<GetHashCodeEntity>().Object);

                Assert.False(mockInternalEntityEntry1.Equals(mockInternalEntityEntry2));
            }
        }

        public class GetHashCodeEntity
        {
            public override bool Equals(object obj)
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
        }

#endif
    }
}
