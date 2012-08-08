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
                var mockInternalEntityEntry = new Mock<InternalEntityEntryForMock<object>>() { CallBase = true };
                mockInternalEntityEntry.Setup(e => e.OriginalValues).Returns(() => new TestInternalPropertyValues<object>());
                mockInternalEntityEntry.Setup(e => e.IsDetached).Returns(true);

                Assert.Equal(Strings.DbEntityEntry_NotSupportedForDetached("GetDatabaseValues", "Object"),
                    Assert.Throws<InvalidOperationException>(() =>
                        mockInternalEntityEntry.Object.GetDatabaseValues()).Message);
            }

            [Fact]
            public void On_an_added_entry_throws()
            {
                var mockInternalEntityEntry = new Mock<InternalEntityEntryForMock<object>>() { CallBase = true };
                mockInternalEntityEntry.Setup(e => e.OriginalValues).Returns(() => new TestInternalPropertyValues<object>());
                mockInternalEntityEntry.Setup(e => e.State).Returns(EntityState.Added);

                Assert.Equal(Strings.DbPropertyValues_CannotGetValuesForState("GetDatabaseValues", EntityState.Added),
                    Assert.Throws<InvalidOperationException>(() =>
                        mockInternalEntityEntry.Object.GetDatabaseValues()).Message);
            }
        }

        public class GetDatabaseValuesAsync
        {
            [Fact]
            public void On_a_detached_entry_throws()
            {
                var mockInternalEntityEntry = new Mock<InternalEntityEntryForMock<object>>() { CallBase = true };
                mockInternalEntityEntry.Setup(e => e.OriginalValues).Returns(() => new TestInternalPropertyValues<object>());
                mockInternalEntityEntry.Setup(e => e.IsDetached).Returns(true);

                Assert.Equal(Strings.DbEntityEntry_NotSupportedForDetached("GetDatabaseValuesAsync", "Object"),
                    Assert.Throws<InvalidOperationException>(() =>
                        ExceptionHelpers.UnwrapAggregateExceptions(() =>
                            mockInternalEntityEntry.Object.GetDatabaseValuesAsync(CancellationToken.None).Result)).Message);
            }

            [Fact]
            public void On_an_added_entry_throws()
            {
                var mockInternalEntityEntry = new Mock<InternalEntityEntryForMock<object>>() { CallBase = true };
                mockInternalEntityEntry.Setup(e => e.OriginalValues).Returns(() => new TestInternalPropertyValues<object>());
                mockInternalEntityEntry.Setup(e => e.State).Returns(EntityState.Added);

                Assert.Equal(Strings.DbPropertyValues_CannotGetValuesForState("GetDatabaseValuesAsync", EntityState.Added),
                    Assert.Throws<InvalidOperationException>(() =>
                        ExceptionHelpers.UnwrapAggregateExceptions(() =>
                            mockInternalEntityEntry.Object.GetDatabaseValuesAsync(CancellationToken.None).Result)).Message);
            }
        }
    }
}
