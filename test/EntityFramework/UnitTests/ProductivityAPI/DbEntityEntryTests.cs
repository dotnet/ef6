namespace ProductivityApiUnitTests
{
    using System;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.Linq;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Moq;
    using Xunit;

    /// <summary>
    /// General unit tests for DbEntityEntry and related classes/methods.
    /// Some specific features, such as concurrency, are tested elsewhere.
    /// </summary>
    public class DbEntityEntryTests : UnitTestBase
    {
        #region Helpers

        private Mock<System.Data.Entity.Internal.IEntityStateEntry> CreateMockStateEntry<TEnity>() where TEnity : class, new()
        {
            var mockStateEntry = new Mock<System.Data.Entity.Internal.IEntityStateEntry>();
            var fakeEntity = new TEnity();
            mockStateEntry.Setup(e => e.Entity).Returns(fakeEntity);
            return mockStateEntry;
        }

        private Mock<InternalContextForMock> CreateMockInternalContextForEntry(Mock<System.Data.Entity.Internal.IEntityStateEntry> mockStateEntry)
        {
            return CreateMockInternalContextForEntries(new[] { mockStateEntry });
        }

        private Mock<InternalContextForMock> CreateMockInternalContextForEntries(Mock<System.Data.Entity.Internal.IEntityStateEntry>[] mockStateEntries)
        {
            var fakeEntity = mockStateEntries[0].Object.Entity;

            var mockInternalContext = new Mock<InternalContextForMock>();
            mockInternalContext.Setup(c => c.GetStateEntry(fakeEntity)).Returns(mockStateEntries[0].Object);
            mockInternalContext.Setup(c => c.GetStateEntries()).Returns(mockStateEntries.Select(e => e.Object));
            mockInternalContext.Setup(c => c.GetStateEntries<FakeDerivedEntity>()).Returns(mockStateEntries.Where(e => e.Object.Entity is FakeDerivedEntity).Select(e => e.Object));

            var mockContext = Mock.Get(mockInternalContext.Object.Owner);
            mockContext.Setup(c => c.InternalContext).Returns(mockInternalContext.Object);

            return mockInternalContext;
        }

        #endregion

        #region Tests for generic DbEntityEntry<> properties and methods

        [Fact]
        public void DbEntityEntry_Entity_returns_the_tracked_entity()
        {
            var mockStateEntry = CreateMockStateEntry<FakeEntity>();
            var fakeEntity = mockStateEntry.Object.Entity;

            var entityEntry = new DbEntityEntry<FakeEntity>(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, mockStateEntry.Object));

            Assert.Same(fakeEntity, entityEntry.Entity);
        }

        [Fact]
        public void DbEntityEntry_State_returns_the_entity_state()
        {
            var mockStateEntry = CreateMockStateEntry<FakeEntity>();
            mockStateEntry.Setup(e => e.State).Returns(EntityState.Detached);
            var entityEntry = new DbEntityEntry<FakeEntity>(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, mockStateEntry.Object));

            Assert.Equal(EntityState.Detached, entityEntry.State);
        }

        [Fact]
        public void DbEntityEntry_State_sets_the_entity_state()
        {
            var mockStateEntry = CreateMockStateEntry<FakeEntity>();
            var entityEntry = new DbEntityEntry<FakeEntity>(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, mockStateEntry.Object));

            entityEntry.State = EntityState.Detached;

            mockStateEntry.Verify(e => e.ChangeState(EntityState.Detached), Times.Once());
        }

        [Fact]
        public void DbEntityEntry_GetValidationResult_calls_ValidateEntity()
        {
            var mockInternalContext = new Mock<InternalContextForMock>();
            var entityEntry = new DbEntityEntry<FakeEntity>(
                new InternalEntityEntry(mockInternalContext.Object, CreateMockStateEntry<FakeEntity>().Object));

            entityEntry.GetValidationResult();

            Mock.Get(mockInternalContext.Object.Owner).Verify(c => c.CallValidateEntity(entityEntry));
        }

        [Fact]
        public void InternalEntityEntry_EntityType_returns_the_type_of_the_tracked_entity()
        {
            var mockStateEntry = CreateMockStateEntry<FakeEntity>();
            var entityEntry = new InternalEntityEntry(new Mock<InternalContextForMock>().Object, mockStateEntry.Object);

            Assert.Equal(typeof(FakeEntity), entityEntry.EntityType);
        }

        #endregion

        #region Tests for non-generic DbEntityEntry properties and methods

        [Fact]
        public void Non_generic_DbEntityEntry_Entity_returns_the_tracked_entity()
        {
            var mockStateEntry = CreateMockStateEntry<FakeEntity>();
            var fakeEntity = mockStateEntry.Object.Entity;

            var entityEntry = new DbEntityEntry(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, mockStateEntry.Object));

            Assert.Same(fakeEntity, entityEntry.Entity);
        }

        [Fact]
        public void Non_generic_DbEntityEntry_State_returns_the_entity_state()
        {
            var mockStateEntry = CreateMockStateEntry<FakeEntity>();
            mockStateEntry.Setup(e => e.State).Returns(EntityState.Detached);
            var entityEntry = new DbEntityEntry(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, mockStateEntry.Object));

            Assert.Equal(EntityState.Detached, entityEntry.State);
        }

        [Fact]
        public void Non_generic_DbEntityEntry_State_sets_the_entity_state()
        {
            var mockStateEntry = CreateMockStateEntry<FakeEntity>();
            var entityEntry = new DbEntityEntry(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, mockStateEntry.Object));

            entityEntry.State = EntityState.Detached;

            mockStateEntry.Verify(e => e.ChangeState(EntityState.Detached), Times.Once());
        }

        [Fact]
        public void Non_generic_DbEntityEntry_GetValidationResult_calls_ValidateEntity()
        {
            var mockInternalContext = new Mock<InternalContextForMock>();
            var entityEntry = new DbEntityEntry(
                new InternalEntityEntry(mockInternalContext.Object, CreateMockStateEntry<FakeEntity>().Object));

            entityEntry.GetValidationResult();

            Mock.Get(mockInternalContext.Object.Owner).Verify(c => c.CallValidateEntity(entityEntry));
        }

        [Fact]
        public void Non_generic_DbEntityEntry_can_be_converted_to_generic_version()
        {
            var mockStateEntry = CreateMockStateEntry<FakeEntity>();
            var entityEntry = new DbEntityEntry(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, mockStateEntry.Object));

            var nonGenericEntry = entityEntry.Cast<FakeEntity>();

            Assert.Same(entityEntry.Entity, nonGenericEntry.Entity);
        }

        [Fact]
        public void Non_generic_DbEntityEntry_can_be_converted_to_generic_version_of_base_type()
        {
            var mockStateEntry = new Mock<System.Data.Entity.Internal.IEntityStateEntry>();
            mockStateEntry.Setup(e => e.Entity).Returns(new FakeDerivedEntity());
            var entityEntry = new DbEntityEntry(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, mockStateEntry.Object));

            var nonGenericEntry = entityEntry.Cast<FakeEntity>();

            Assert.Same(entityEntry.Entity, nonGenericEntry.Entity);
        }

        [Fact]
        public void Non_generic_DbEntityEntry_throws_on_attempt_to_create_generic_version_for_incompatible_type()
        {
            var mockStateEntry = CreateMockStateEntry<FakeEntity>();
            var entityEntry = new DbEntityEntry(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, mockStateEntry.Object));

            Assert.Equal(Strings.DbEntity_BadTypeForCast(typeof(DbEntityEntry).Name, typeof(FakeDerivedEntity).Name, typeof(FakeEntity).Name), Assert.Throws<InvalidCastException>(() => entityEntry.Cast<FakeDerivedEntity>()).Message);
        }

        [Fact]
        public void Generic_DbEntityEntry_can_be_implicitly_converted_to_non_generic_version()
        {
            var mockStateEntry = CreateMockStateEntry<FakeEntity>();
            DbEntityEntry<FakeEntity> entityEntry = new DbEntityEntry<FakeEntity>(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, mockStateEntry.Object));

            NonGenericTestMethod(entityEntry, entityEntry.Entity);
        }

        private void NonGenericTestMethod(DbEntityEntry nonGenericEntry, object wrappedEntity)
        {
            Assert.Same(wrappedEntity, nonGenericEntry.Entity);
        }

        #endregion

        #region Tests for methods that cannot be used with detached entries

        private Mock<InternalContextForMock> CreateMockedInternalContextForDetachedEntity()
        {
            var mockSetAdapeter = new Mock<IInternalSetAdapter>();
            mockSetAdapeter.SetupGet(a => a.InternalSet).Returns(new Mock<InternalSetForMock<FakeEntity>>().Object);

            var mockInternalContext = new Mock<InternalContextForMock>();
            mockInternalContext.Setup(i => i.Set(typeof(FakeEntity))).Returns(mockSetAdapeter.Object);

            var mockContext = Mock.Get(mockInternalContext.Object.Owner);
            mockContext.Setup(c => c.InternalContext).Returns(mockInternalContext.Object);

            return mockInternalContext;
        }

        [Fact]
        public void InternalEntityEntry_CurrentValues_throws_when_used_on_stand_alone_entry()
        {
            var entityEntry = new InternalEntityEntry(CreateMockedInternalContextForDetachedEntity().Object, new FakeEntity());

            Assert.Equal(Strings.DbEntityEntry_NotSupportedForDetached("CurrentValues", "FakeEntity"), Assert.Throws<InvalidOperationException>(() => { var _ = entityEntry.CurrentValues; }).Message);
        }

        [Fact]
        public void InternalEntityEntry_OriginalValues_throws_when_used_on_stand_alone_entry()
        {
            var entityEntry = new InternalEntityEntry(CreateMockedInternalContextForDetachedEntity().Object, new FakeEntity());

            Assert.Equal(Strings.DbEntityEntry_NotSupportedForDetached("OriginalValues", "FakeEntity"), Assert.Throws<InvalidOperationException>(() => { var _ = entityEntry.OriginalValues; }).Message);
        }

        [Fact]
        public void InternalEntityEntry_GetDatabaseValues_throws_when_used_on_stand_alone_entry()
        {
            var entityEntry = new InternalEntityEntry(CreateMockedInternalContextForDetachedEntity().Object, new FakeEntity());

            Assert.Equal(Strings.DbEntityEntry_NotSupportedForDetached("GetDatabaseValues", "FakeEntity"), Assert.Throws<InvalidOperationException>(() => entityEntry.GetDatabaseValues()).Message);
        }

        [Fact]
        public void InternalEntityEntry_Reload_throws_when_used_on_stand_alone_entry()
        {
            var entityEntry = new InternalEntityEntry(CreateMockedInternalContextForDetachedEntity().Object, new FakeEntity());

            Assert.Equal(Strings.DbEntityEntry_NotSupportedForDetached("Reload", "FakeEntity"), Assert.Throws<InvalidOperationException>(() => entityEntry.Reload()).Message);
        }

        #endregion

        #region Tests for getting entity entries from a context

        [Fact]
        public void DbContext_Entry_returns_an_entry_for_a_tracked_entity()
        {
            var mockStateEntry = CreateMockStateEntry<FakeEntity>();
            var mockInternalContext = CreateMockInternalContextForEntry(mockStateEntry);
            var fakeEntity = mockStateEntry.Object.Entity;
            var context = mockInternalContext.Object.Owner;

            var entry = context.Entry(fakeEntity);

            Assert.Same(mockStateEntry.Object.Entity, entry.Entity);
        }

        [Fact]
        public void DbContext_Entry_returns_an_entry_for_a_detached_entity()
        {
            var context = CreateMockedInternalContextForDetachedEntity().Object.Owner;
            var fakeEntity = new FakeEntity();

            var entry = context.Entry((object)fakeEntity);

            Assert.NotNull(entry);
            Assert.Same(fakeEntity, entry.Entity);
        }

        [Fact]
        public void DbContext_Entry_throws_if_given_a_null_entity()
        {
            var context = CreateMockInternalContextForEntry(CreateMockStateEntry<FakeEntity>()).Object.Owner;

            Assert.Equal("entity", Assert.Throws<ArgumentNullException>(() => context.Entry(null)).ParamName);
        }

        [Fact]
        public void Generic_DbContext_Entry_returns_a_generic_entry_for_a_tracked_entity()
        {
            var mockStateEntry = CreateMockStateEntry<FakeEntity>();
            var mockInternalContext = CreateMockInternalContextForEntry(mockStateEntry);
            var fakeEntity = mockStateEntry.Object.Entity;
            var context = mockInternalContext.Object.Owner;

            var entry = context.Entry<FakeEntity>((FakeEntity)fakeEntity);

            Assert.Same(mockStateEntry.Object.Entity, entry.Entity);
        }

        [Fact]
        public void Generic_DbContext_Entry_returns_an_entry_for_a_detached_entity()
        {
            var context = CreateMockedInternalContextForDetachedEntity().Object.Owner;
            var fakeEntity = new FakeEntity();

            var entry = context.Entry<FakeEntity>(fakeEntity);

            Assert.NotNull(entry);
            Assert.Same(fakeEntity, entry.Entity);
        }

        [Fact]
        public void Generic_DbContext_Entry_throws_if_given_a_null_entity()
        {
            var context = CreateMockInternalContextForEntry(CreateMockStateEntry<FakeEntity>()).Object.Owner;

            Assert.Equal("entity", Assert.Throws<ArgumentNullException>(() => context.Entry<FakeEntity>(null)).ParamName);
        }

        [Fact]
        public void DbContext_Entries_returns_entries_for_all_types_of_tracked_entity()
        {
            var mockStateEntries = new[] { CreateMockStateEntry<FakeEntity>(), CreateMockStateEntry<FakeDerivedEntity>(), CreateMockStateEntry<FakeDerivedEntity>() };
            var mockInternalContext = CreateMockInternalContextForEntries(mockStateEntries);
            var context = mockInternalContext.Object.Owner;

            var entries = context.ChangeTracker.Entries().ToList();

            Assert.Equal(3, entries.Count);
            Assert.Same(mockStateEntries[0].Object.Entity, entries[0].Entity);
            Assert.Same(mockStateEntries[1].Object.Entity, entries[1].Entity);
            Assert.Same(mockStateEntries[2].Object.Entity, entries[2].Entity);
        }

        [Fact]
        public void Generic_DbContext_Entries_returns_entries_for_tracked_entities_of_the_given_type()
        {
            var mockStateEntries = new[] { CreateMockStateEntry<FakeEntity>(), CreateMockStateEntry<FakeDerivedEntity>(), CreateMockStateEntry<FakeDerivedEntity>() };
            var mockInternalContext = CreateMockInternalContextForEntries(mockStateEntries);
            var context = mockInternalContext.Object.Owner;

            var entries = context.ChangeTracker.Entries<FakeDerivedEntity>().ToList();

            Assert.Equal(2, entries.Count);
            Assert.Same(mockStateEntries[1].Object.Entity, entries[0].Entity);
            Assert.Same(mockStateEntries[2].Object.Entity, entries[1].Entity);
        }

        #endregion

        #region Tests for overriden Equals and HashCode

        [Fact]
        public void Generic_DbEntityEntries_for_same_entity_in_same_context_test_equal_and_return_same_hash_code()
        {
            var stateEntry = CreateMockStateEntry<FakeEntity>().Object;
            var internalContext = new Mock<InternalContextForMock>().Object;

            var entityEntry1 = new DbEntityEntry<FakeEntity>(new InternalEntityEntry(internalContext, stateEntry));
            var entityEntry2 = new DbEntityEntry<FakeEntity>(new InternalEntityEntry(internalContext, stateEntry));

            Assert.Equal(entityEntry1, entityEntry2);
            Assert.Equal(entityEntry1.GetHashCode(), entityEntry2.GetHashCode());
        }

        [Fact]
        public void Generic_DbEntityEntries_for_different_entities_in_same_context_test_not_equal()
        {
            var internalContext = new Mock<InternalContextForMock>().Object;

            var entityEntry1 = new DbEntityEntry<FakeEntity>(new InternalEntityEntry(internalContext, CreateMockStateEntry<FakeEntity>().Object));
            var entityEntry2 = new DbEntityEntry<FakeEntity>(new InternalEntityEntry(internalContext, CreateMockStateEntry<FakeEntity>().Object));

            Assert.NotEqual(entityEntry1, entityEntry2);
        }

        [Fact]
        public void Generic_DbEntityEntries_for_same_entity_in_different_contexts_test_not_equal()
        {
            var stateEntry = CreateMockStateEntry<FakeEntity>().Object;

            var entityEntry1 = new DbEntityEntry<FakeEntity>(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, stateEntry));
            var entityEntry2 = new DbEntityEntry<FakeEntity>(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, stateEntry));

            Assert.NotEqual(entityEntry1, entityEntry2);
        }

        [Fact]
        public void Generic_DbEntityEntries_for_different_entities_in_different_contexts_test_not_equal()
        {
            var entityEntry1 = new DbEntityEntry<FakeEntity>(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, CreateMockStateEntry<FakeEntity>().Object));
            var entityEntry2 = new DbEntityEntry<FakeEntity>(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, CreateMockStateEntry<FakeEntity>().Object));

            Assert.NotEqual(entityEntry1, entityEntry2);
        }

        [Fact]
        public void Generic_DbEntityEntry_tests_not_equal_to_null()
        {
            var entityEntry = new DbEntityEntry<FakeEntity>(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, CreateMockStateEntry<FakeEntity>().Object));

            Assert.False(entityEntry.Equals((object)null));
        }

        [Fact]
        public void Generic_DbEntityEntry_tests_not_equal_to_null_using_strongly_typed_method()
        {
            var entityEntry = new DbEntityEntry<FakeEntity>(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, CreateMockStateEntry<FakeEntity>().Object));

            Assert.False(entityEntry.Equals((DbEntityEntry<FakeEntity>)null));
        }

        [Fact]
        public void Generic_DbEntityEntry_tests_not_equal_to_object_of_wrong_type()
        {
            var stateEntry = CreateMockStateEntry<FakeEntity>().Object;
            var internalContext = new Mock<InternalContextForMock>().Object;

            var entityEntry1 = new DbEntityEntry<FakeEntity>(new InternalEntityEntry(internalContext, stateEntry));
            var entityEntry2 = new DbEntityEntry(new InternalEntityEntry(internalContext, stateEntry));

            Assert.False(entityEntry1.Equals(entityEntry2));
        }

        [Fact]
        public void Non_generic_DbEntityEntries_for_same_entity_in_same_context_test_equal_and_return_same_hash_code()
        {
            var stateEntry = CreateMockStateEntry<FakeEntity>().Object;
            var internalContext = new Mock<InternalContextForMock>().Object;

            var entityEntry1 = new DbEntityEntry(new InternalEntityEntry(internalContext, stateEntry));
            var entityEntry2 = new DbEntityEntry(new InternalEntityEntry(internalContext, stateEntry));

            Assert.Equal(entityEntry1, entityEntry2);
            Assert.Equal(entityEntry1.GetHashCode(), entityEntry2.GetHashCode());
        }

        [Fact]
        public void Non_generic_DbEntityEntries_for_different_entities_in_same_context_test_not_equal()
        {
            var internalContext = new Mock<InternalContextForMock>().Object;

            var entityEntry1 = new DbEntityEntry(new InternalEntityEntry(internalContext, CreateMockStateEntry<FakeEntity>().Object));
            var entityEntry2 = new DbEntityEntry(new InternalEntityEntry(internalContext, CreateMockStateEntry<FakeEntity>().Object));

            Assert.NotEqual(entityEntry1, entityEntry2);
        }

        [Fact]
        public void Non_generic_DbEntityEntries_for_same_entity_in_different_contexts_test_not_equal()
        {
            var stateEntry = CreateMockStateEntry<FakeEntity>().Object;

            var entityEntry1 = new DbEntityEntry(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, stateEntry));
            var entityEntry2 = new DbEntityEntry(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, stateEntry));

            Assert.NotEqual(entityEntry1, entityEntry2);
        }

        [Fact]
        public void Non_generic_DbEntityEntries_for_different_entities_in_different_contexts_test_not_equal()
        {
            var entityEntry1 = new DbEntityEntry(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, CreateMockStateEntry<FakeEntity>().Object));
            var entityEntry2 = new DbEntityEntry(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, CreateMockStateEntry<FakeEntity>().Object));

            Assert.NotEqual(entityEntry1, entityEntry2);
        }

        [Fact]
        public void Non_generic_DbEntityEntry_tests_not_equal_to_null()
        {
            var entityEntry = new DbEntityEntry(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, CreateMockStateEntry<FakeEntity>().Object));

            Assert.False(entityEntry.Equals((object)null));
        }

        [Fact]
        public void Non_generic_DbEntityEntry_tests_not_equal_to_null_using_strongly_typed_method()
        {
            var entityEntry = new DbEntityEntry(new InternalEntityEntry(new Mock<InternalContextForMock>().Object, CreateMockStateEntry<FakeEntity>().Object));

            Assert.False(entityEntry.Equals((DbEntityEntry)null));
        }

        [Fact]
        public void Non_generic_DbEntityEntry_tests_not_equal_to_object_of_wrong_type()
        {
            var stateEntry = CreateMockStateEntry<FakeEntity>().Object;
            var internalContext = new Mock<InternalContextForMock>().Object;

            var entityEntry1 = new DbEntityEntry(new InternalEntityEntry(internalContext, stateEntry));
            var entityEntry2 = new DbEntityEntry<FakeEntity>(new InternalEntityEntry(internalContext, stateEntry));

            Assert.False(entityEntry1.Equals((object)entityEntry2));
        }

        [Fact]
        public void Non_generic_DbEntityEntry_tests_equal_to_generic_version_due_to_implicit_cast()
        {
            var stateEntry = CreateMockStateEntry<FakeEntity>().Object;
            var internalContext = new Mock<InternalContextForMock>().Object;

            var entityEntry1 = new DbEntityEntry(new InternalEntityEntry(internalContext, stateEntry));
            var entityEntry2 = new DbEntityEntry<FakeEntity>(new InternalEntityEntry(internalContext, stateEntry));

            Assert.True(entityEntry1.Equals(entityEntry2));
        }

        #endregion
    }
}
