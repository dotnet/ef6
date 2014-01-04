// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Resources;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class EntityAdapterTests
    {
        public class Update
        {
            [Fact]
            public void Returns_0_if_no_changes()
            {
                var entityStateManagerMock = new Mock<ObjectStateManager>();
                var mockContext = new Mock<ObjectContext>(null, null, null, null);
                mockContext.Setup(m => m.ObjectStateManager).Returns(entityStateManagerMock.Object);

                var entityAdapter = new EntityAdapter(mockContext.Object);

                Assert.Equal(0, entityAdapter.Update());
            }

            [Fact]
            public void Returns_number_of_enties_affected()
            {
                var entityStateManagerMock = new Mock<ObjectStateManager>();
                entityStateManagerMock.Setup(m => m.HasChanges()).Returns(true);
                var entityStateEntryMock = new Mock<ObjectStateEntry>();
                entityStateManagerMock.Setup(m => m.GetObjectStateEntriesInternal(It.IsAny<EntityState>()))
                    .Returns(new[] { entityStateEntryMock.Object });

                var mockContext = new Mock<ObjectContext>(null, null, null, null);
                mockContext.Setup(m => m.ObjectStateManager).Returns(entityStateManagerMock.Object);

                var mockUpdateTranslator = new Mock<UpdateTranslator>(MockBehavior.Strict);
                mockUpdateTranslator.Setup(m => m.Update()).Returns(1);

                var updateTranslatorFactory = new Func<EntityAdapter, UpdateTranslator>(adapter => mockUpdateTranslator.Object);

                var entityAdapter = new Mock<EntityAdapter>(mockContext.Object, updateTranslatorFactory)
                                        {
                                            CallBase = true
                                        }.Object;

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.Setup(m => m.State).Returns(ConnectionState.Open);
                entityConnectionMock.Setup(m => m.StoreConnection)
                    .Returns(new Mock<DbConnection>().Object);
                entityConnectionMock.Setup(m => m.StoreProviderFactory)
                    .Returns(EntityProviderFactory.Instance);
                entityAdapter.Connection = entityConnectionMock.Object;

                var cacheEntriesAffected = entityAdapter.Update();

                Assert.Equal(1, cacheEntriesAffected);
            }

            [Fact]
            public void Update_throws_if_no_connection_set()
            {
                var entityStateManagerMock = new Mock<ObjectStateManager>();
                entityStateManagerMock.Setup(m => m.HasChanges()).Returns(true);
                var entityStateEntryMock = new Mock<ObjectStateEntry>();
                entityStateManagerMock.Setup(m => m.GetObjectStateEntriesInternal(It.IsAny<EntityState>()))
                    .Returns(new[] { entityStateEntryMock.Object });

                var mockContext = new Mock<ObjectContext>(null, null, null, null);
                mockContext.Setup(m => m.ObjectStateManager).Returns(entityStateManagerMock.Object);

                var entityAdapter = new EntityAdapter(mockContext.Object);

                Assert.Equal(
                    Strings.EntityClient_NoConnectionForAdapter,
                    Assert.Throws<InvalidOperationException>(() => entityAdapter.Update()).Message);
            }

            [Fact]
            public void Throws_if_no_store_connection_set()
            {
                var entityStateManagerMock = new Mock<ObjectStateManager>();
                entityStateManagerMock.Setup(m => m.HasChanges()).Returns(true);
                var entityStateEntryMock = new Mock<ObjectStateEntry>();
                entityStateManagerMock.Setup(m => m.GetObjectStateEntriesInternal(It.IsAny<EntityState>()))
                    .Returns(new[] { entityStateEntryMock.Object });

                var mockContext = new Mock<ObjectContext>(null, null, null, null);
                mockContext.Setup(m => m.ObjectStateManager).Returns(entityStateManagerMock.Object);

                var entityAdapter = new EntityAdapter(mockContext.Object);

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.Setup(m => m.StoreProviderFactory)
                    .Returns(new Mock<DbProviderFactory>().Object);
                entityAdapter.Connection = entityConnectionMock.Object;

                Assert.Equal(
                    Strings.EntityClient_NoStoreConnectionForUpdate,
                    Assert.Throws<InvalidOperationException>(() => entityAdapter.Update()).Message);
            }

            [Fact]
            public void Throws_if_no_store_provider_factory_set()
            {
                var entityStateManagerMock = new Mock<ObjectStateManager>();
                entityStateManagerMock.Setup(m => m.HasChanges()).Returns(true);
                var entityStateEntryMock = new Mock<ObjectStateEntry>();
                entityStateManagerMock.Setup(m => m.GetObjectStateEntriesInternal(It.IsAny<EntityState>()))
                    .Returns(new[] { entityStateEntryMock.Object });

                var mockContext = new Mock<ObjectContext>(null, null, null, null);
                mockContext.Setup(m => m.ObjectStateManager).Returns(entityStateManagerMock.Object);

                var entityAdapter = new EntityAdapter(mockContext.Object);

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.Setup(m => m.StoreConnection)
                    .Returns(new Mock<DbConnection>().Object);
                entityAdapter.Connection = entityConnectionMock.Object;

                Assert.Equal(
                    Strings.EntityClient_NoStoreConnectionForUpdate,
                    Assert.Throws<InvalidOperationException>(() => entityAdapter.Update()).Message);
            }

            [Fact]
            public void Throws_if_connection_is_closed()
            {
                var entityStateManagerMock = new Mock<ObjectStateManager>();
                entityStateManagerMock.Setup(m => m.HasChanges()).Returns(true);
                var entityStateEntryMock = new Mock<ObjectStateEntry>();
                entityStateManagerMock.Setup(m => m.GetObjectStateEntriesInternal(It.IsAny<EntityState>()))
                    .Returns(new[] { entityStateEntryMock.Object });

                var mockContext = new Mock<ObjectContext>(null, null, null, null);
                mockContext.Setup(m => m.ObjectStateManager).Returns(entityStateManagerMock.Object);
                var entityAdapter = new EntityAdapter(mockContext.Object);

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.Setup(m => m.StoreConnection)
                    .Returns(new Mock<DbConnection>().Object);
                entityConnectionMock.Setup(m => m.StoreProviderFactory)
                    .Returns(new Mock<DbProviderFactory>().Object);
                entityAdapter.Connection = entityConnectionMock.Object;

                Assert.Equal(
                    Strings.EntityClient_ClosedConnectionForUpdate,
                    Assert.Throws<InvalidOperationException>(() => entityAdapter.Update()).Message);
            }
        }

#if !NET40

        public class UpdateAsync
        {
            [Fact]
            public void Returns_0_if_no_changes()
            {
                var entityStateManagerMock = new Mock<ObjectStateManager>();

                var mockContext = new Mock<ObjectContext>(null, null, null, null);
                mockContext.Setup(m => m.ObjectStateManager).Returns(entityStateManagerMock.Object);

                var entityAdapter = new EntityAdapter(mockContext.Object);

                var cacheEntriesAffected = entityAdapter.UpdateAsync(CancellationToken.None).Result;

                Assert.Equal(0, cacheEntriesAffected);
            }

            [Fact]
            public void Returns_number_of_enties_affected()
            {
                var entityStateManagerMock = new Mock<ObjectStateManager>();
                entityStateManagerMock.Setup(m => m.HasChanges()).Returns(true);
                var entityStateEntryMock = new Mock<ObjectStateEntry>();
                entityStateManagerMock.Setup(m => m.GetObjectStateEntriesInternal(It.IsAny<EntityState>()))
                    .Returns(new[] { entityStateEntryMock.Object });

                var mockContext = new Mock<ObjectContext>(null, null, null, null);
                mockContext.Setup(m => m.ObjectStateManager).Returns(entityStateManagerMock.Object);

                var mockUpdateTranslator = new Mock<UpdateTranslator>(MockBehavior.Strict);
                mockUpdateTranslator.Setup(m => m.UpdateAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(1));

                var updateTranslatorFactory = new Func<EntityAdapter, UpdateTranslator>(adapter => mockUpdateTranslator.Object);

                var entityAdapter = new Mock<EntityAdapter>(mockContext.Object, updateTranslatorFactory)
                                        {
                                            CallBase = true
                                        }.Object;

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.Setup(m => m.State).Returns(ConnectionState.Open);
                entityConnectionMock.Setup(m => m.StoreConnection)
                    .Returns(new Mock<DbConnection>().Object);
                entityConnectionMock.Setup(m => m.StoreProviderFactory)
                    .Returns(EntityProviderFactory.Instance);
                entityAdapter.Connection = entityConnectionMock.Object;

                var cacheEntriesAffected = entityAdapter.UpdateAsync(CancellationToken.None).Result;

                Assert.Equal(1, cacheEntriesAffected);
            }

            [Fact]
            public void Update_throws_if_no_connection_set()
            {
                var entityStateManagerMock = new Mock<ObjectStateManager>();
                entityStateManagerMock.Setup(m => m.HasChanges()).Returns(true);
                var entityStateEntryMock = new Mock<ObjectStateEntry>();
                entityStateManagerMock.Setup(m => m.GetObjectStateEntriesInternal(It.IsAny<EntityState>()))
                    .Returns(new[] { entityStateEntryMock.Object });

                var mockContext = new Mock<ObjectContext>(null, null, null, null);
                mockContext.Setup(m => m.ObjectStateManager).Returns(entityStateManagerMock.Object);

                var entityAdapter = new EntityAdapter(mockContext.Object);

                Assert.Equal(
                    Strings.EntityClient_NoConnectionForAdapter,
                    Assert.Throws<InvalidOperationException>(
                        () => entityAdapter.UpdateAsync(CancellationToken.None)).Message);
            }

            [Fact]
            public void Throws_if_no_store_connection_set()
            {
                var entityStateManagerMock = new Mock<ObjectStateManager>();
                entityStateManagerMock.Setup(m => m.HasChanges()).Returns(true);
                var entityStateEntryMock = new Mock<ObjectStateEntry>();
                entityStateManagerMock.Setup(m => m.GetObjectStateEntriesInternal(It.IsAny<EntityState>()))
                    .Returns(new[] { entityStateEntryMock.Object });

                var mockContext = new Mock<ObjectContext>(null, null, null, null);
                mockContext.Setup(m => m.ObjectStateManager).Returns(entityStateManagerMock.Object);

                var entityAdapter = new EntityAdapter(mockContext.Object);

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.Setup(m => m.StoreProviderFactory)
                    .Returns(new Mock<DbProviderFactory>().Object);
                entityAdapter.Connection = entityConnectionMock.Object;

                Assert.Equal(
                    Strings.EntityClient_NoStoreConnectionForUpdate,
                    Assert.Throws<InvalidOperationException>(
                        () => entityAdapter.UpdateAsync(CancellationToken.None)).Message);
            }

            [Fact]
            public void Throws_if_no_store_provider_factory_set()
            {
                var entityStateManagerMock = new Mock<ObjectStateManager>();
                entityStateManagerMock.Setup(m => m.HasChanges()).Returns(true);
                var entityStateEntryMock = new Mock<ObjectStateEntry>();
                entityStateManagerMock.Setup(m => m.GetObjectStateEntriesInternal(It.IsAny<EntityState>()))
                    .Returns(new[] { entityStateEntryMock.Object });

                var mockContext = new Mock<ObjectContext>(null, null, null, null);
                mockContext.Setup(m => m.ObjectStateManager).Returns(entityStateManagerMock.Object);

                var entityAdapter = new EntityAdapter(mockContext.Object);

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.Setup(m => m.StoreConnection)
                    .Returns(new Mock<DbConnection>().Object);
                entityAdapter.Connection = entityConnectionMock.Object;

                Assert.Equal(
                    Strings.EntityClient_NoStoreConnectionForUpdate,
                    Assert.Throws<InvalidOperationException>(
                        () => entityAdapter.UpdateAsync(CancellationToken.None)).Message);
            }

            [Fact]
            public void Throws_if_connection_is_closed()
            {
                var entityStateManagerMock = new Mock<ObjectStateManager>();
                entityStateManagerMock.Setup(m => m.HasChanges()).Returns(true);
                var entityStateEntryMock = new Mock<ObjectStateEntry>();
                entityStateManagerMock.Setup(m => m.GetObjectStateEntriesInternal(It.IsAny<EntityState>()))
                    .Returns(new[] { entityStateEntryMock.Object });

                var mockContext = new Mock<ObjectContext>(null, null, null, null);
                mockContext.Setup(m => m.ObjectStateManager).Returns(entityStateManagerMock.Object);

                var entityAdapter = new EntityAdapter(mockContext.Object);

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.Setup(m => m.StoreConnection)
                    .Returns(new Mock<DbConnection>().Object);
                entityConnectionMock.Setup(m => m.StoreProviderFactory)
                    .Returns(new Mock<DbProviderFactory>().Object);
                entityAdapter.Connection = entityConnectionMock.Object;

                Assert.Equal(
                    Strings.EntityClient_ClosedConnectionForUpdate,
                    Assert.Throws<InvalidOperationException>(
                        () => entityAdapter.UpdateAsync(CancellationToken.None)).Message);
            }
        }

#endif
    }
}
