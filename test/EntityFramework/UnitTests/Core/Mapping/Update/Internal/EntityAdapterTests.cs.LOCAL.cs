namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;
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
                var entityAdapter = new EntityAdapter();
                var mockEntityStateManager = new Mock<IEntityStateManager>();

                var cacheEntriesAffected = entityAdapter.Update(mockEntityStateManager.Object);

                Assert.Equal(0, cacheEntriesAffected);
            }

            [Fact]
            public void Returns_number_of_enties_affected()
            {
                var expectedCacheEntriesAffected = 1;
                var mockUpdateTranslator = new Mock<UpdateTranslator>(MockBehavior.Strict);
                mockUpdateTranslator.Setup(m => m.Update()).Returns(expectedCacheEntriesAffected);

                var updateTranslatorFactory = new Func<IEntityStateManager, EntityAdapter, UpdateTranslator>(
                    (stateManager, adapter) => mockUpdateTranslator.Object);

                var entityAdapter = new Mock<EntityAdapter>(updateTranslatorFactory)
                                        {
                                            CallBase = true
                                        }.Object;

                var mockEntityStateManager = new Mock<IEntityStateManager>();
                var mockEntityStateEntry = new Mock<IEntityStateEntry>();
                mockEntityStateManager.Setup(m => m.GetEntityStateEntries(It.IsAny<EntityState>()))
                    .Returns(new[] { mockEntityStateEntry.Object });

                var mockInternalEntityConnection = new Mock<InternalEntityConnection>();
                mockInternalEntityConnection.Setup(m => m.State).Returns(ConnectionState.Open);
                mockInternalEntityConnection.Setup(m => m.StoreConnection)
                    .Returns(new Mock<DbConnection>().Object);
                mockInternalEntityConnection.Setup(m => m.StoreProviderFactory)
                    .Returns(EntityProviderFactory.Instance);
                entityAdapter.Connection = new EntityConnection(mockInternalEntityConnection.Object);

                var cacheEntriesAffected = entityAdapter.Update(mockEntityStateManager.Object);

                Assert.Equal(expectedCacheEntriesAffected, cacheEntriesAffected);
            }

            [Fact]
            public void Update_throws_if_no_connection_set()
            {
                var entityAdapter = new EntityAdapter();
                var mockEntityStateManager = new Mock<IEntityStateManager>();
                var mockEntityStateEntry = new Mock<IEntityStateEntry>();
                mockEntityStateManager.Setup(m => m.GetEntityStateEntries(It.IsAny<EntityState>()))
                    .Returns(new[] { mockEntityStateEntry.Object });

                Assert.Equal(
                    Strings.EntityClient_NoConnectionForAdapter,
                    Assert.Throws<InvalidOperationException>(() => entityAdapter.Update(mockEntityStateManager.Object)).Message);
            }

            [Fact]
            public void Throws_if_no_store_connection_set()
            {
                var entityAdapter = new EntityAdapter();
                var mockEntityStateManager = new Mock<IEntityStateManager>();
                var mockEntityStateEntry = new Mock<IEntityStateEntry>();
                mockEntityStateManager.Setup(m => m.GetEntityStateEntries(It.IsAny<EntityState>()))
                    .Returns(new[] { mockEntityStateEntry.Object });

                var mockInternalEntityConnection = new Mock<InternalEntityConnection>();
                mockInternalEntityConnection.Setup(m => m.StoreProviderFactory)
                    .Returns(new Mock<DbProviderFactory>().Object);
                entityAdapter.Connection = new EntityConnection(mockInternalEntityConnection.Object);

                Assert.Equal(
                    Strings.EntityClient_NoStoreConnectionForUpdate,
                    Assert.Throws<InvalidOperationException>(() => entityAdapter.Update(mockEntityStateManager.Object)).Message);
            }

            [Fact]
            public void Throws_if_no_store_provider_factory_set()
            {
                var entityAdapter = new EntityAdapter();
                var mockEntityStateManager = new Mock<IEntityStateManager>();
                var mockEntityStateEntry = new Mock<IEntityStateEntry>();
                mockEntityStateManager.Setup(m => m.GetEntityStateEntries(It.IsAny<EntityState>()))
                    .Returns(new[] { mockEntityStateEntry.Object });

                var mockInternalEntityConnection = new Mock<InternalEntityConnection>();
                mockInternalEntityConnection.Setup(m => m.StoreConnection)
                    .Returns(new Mock<DbConnection>().Object);
                entityAdapter.Connection = new EntityConnection(mockInternalEntityConnection.Object);

                Assert.Equal(
                    Strings.EntityClient_NoStoreConnectionForUpdate,
                    Assert.Throws<InvalidOperationException>(() => entityAdapter.Update(mockEntityStateManager.Object)).Message);
            }

            [Fact]
            public void Throws_if_connection_is_closed()
            {
                var entityAdapter = new EntityAdapter();
                var mockEntityStateManager = new Mock<IEntityStateManager>();
                var mockEntityStateEntry = new Mock<IEntityStateEntry>();
                mockEntityStateManager.Setup(m => m.GetEntityStateEntries(It.IsAny<EntityState>()))
                    .Returns(new[] { mockEntityStateEntry.Object });

                var mockInternalEntityConnection = new Mock<InternalEntityConnection>();
                mockInternalEntityConnection.Setup(m => m.StoreConnection)
                    .Returns(new Mock<DbConnection>().Object);
                mockInternalEntityConnection.Setup(m => m.StoreProviderFactory)
                    .Returns(new Mock<DbProviderFactory>().Object);
                entityAdapter.Connection = new EntityConnection(mockInternalEntityConnection.Object);

                Assert.Equal(
                    Strings.EntityClient_ClosedConnectionForUpdate,
                    Assert.Throws<InvalidOperationException>(() => entityAdapter.Update(mockEntityStateManager.Object)).Message);
            }
        }

        public class UpdateAsync
        {
            [Fact]
            public void Returns_0_if_no_changes()
            {
                var entityAdapter = new EntityAdapter();
                var mockEntityStateManager = new Mock<IEntityStateManager>();

                var cacheEntriesAffected = entityAdapter.UpdateAsync(mockEntityStateManager.Object, CancellationToken.None).Result;

                Assert.Equal(0, cacheEntriesAffected);
            }

            [Fact]
            public void Returns_number_of_enties_affected()
            {
                var expectedCacheEntriesAffected = 1;
                var mockUpdateTranslator = new Mock<UpdateTranslator>(MockBehavior.Strict);
                mockUpdateTranslator.Setup(m => m.UpdateAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(expectedCacheEntriesAffected));

                var updateTranslatorFactory = new Func<IEntityStateManager, EntityAdapter, UpdateTranslator>(
                    (stateManager, adapter) => mockUpdateTranslator.Object);

                var entityAdapter = new Mock<EntityAdapter>(updateTranslatorFactory)
                {
                    CallBase = true
                }.Object;

                var mockEntityStateManager = new Mock<IEntityStateManager>();
                var mockEntityStateEntry = new Mock<IEntityStateEntry>();
                mockEntityStateManager.Setup(m => m.GetEntityStateEntries(It.IsAny<EntityState>()))
                    .Returns(new[] { mockEntityStateEntry.Object });

                var mockInternalEntityConnection = new Mock<InternalEntityConnection>();
                mockInternalEntityConnection.Setup(m => m.State).Returns(ConnectionState.Open);
                mockInternalEntityConnection.Setup(m => m.StoreConnection)
                    .Returns(new Mock<DbConnection>().Object);
                mockInternalEntityConnection.Setup(m => m.StoreProviderFactory)
                    .Returns(EntityProviderFactory.Instance);
                entityAdapter.Connection = new EntityConnection(mockInternalEntityConnection.Object);

                var cacheEntriesAffected = entityAdapter.UpdateAsync(mockEntityStateManager.Object, CancellationToken.None).Result;

                Assert.Equal(expectedCacheEntriesAffected, cacheEntriesAffected);
            }

            [Fact]
            public void Update_throws_if_no_connection_set()
            {
                var entityAdapter = new EntityAdapter();
                var mockEntityStateManager = new Mock<IEntityStateManager>();
                var mockEntityStateEntry = new Mock<IEntityStateEntry>();
                mockEntityStateManager.Setup(m => m.GetEntityStateEntries(It.IsAny<EntityState>()))
                    .Returns(new[] { mockEntityStateEntry.Object });

                Assert.Equal(
                    Strings.EntityClient_NoConnectionForAdapter,
                    Assert.Throws<InvalidOperationException>(() => entityAdapter.UpdateAsync(mockEntityStateManager.Object, CancellationToken.None)).Message);
            }

            [Fact]
            public void Throws_if_no_store_connection_set()
            {
                var entityAdapter = new EntityAdapter();
                var mockEntityStateManager = new Mock<IEntityStateManager>();
                var mockEntityStateEntry = new Mock<IEntityStateEntry>();
                mockEntityStateManager.Setup(m => m.GetEntityStateEntries(It.IsAny<EntityState>()))
                    .Returns(new[] { mockEntityStateEntry.Object });

                var mockInternalEntityConnection = new Mock<InternalEntityConnection>();
                mockInternalEntityConnection.Setup(m => m.StoreProviderFactory)
                    .Returns(new Mock<DbProviderFactory>().Object);
                entityAdapter.Connection = new EntityConnection(mockInternalEntityConnection.Object);

                Assert.Equal(
                    Strings.EntityClient_NoStoreConnectionForUpdate,
                    Assert.Throws<InvalidOperationException>(() => entityAdapter.UpdateAsync(mockEntityStateManager.Object, CancellationToken.None)).Message);
            }

            [Fact]
            public void Throws_if_no_store_provider_factory_set()
            {
                var entityAdapter = new EntityAdapter();
                var mockEntityStateManager = new Mock<IEntityStateManager>();
                var mockEntityStateEntry = new Mock<IEntityStateEntry>();
                mockEntityStateManager.Setup(m => m.GetEntityStateEntries(It.IsAny<EntityState>()))
                    .Returns(new[] { mockEntityStateEntry.Object });

                var mockInternalEntityConnection = new Mock<InternalEntityConnection>();
                mockInternalEntityConnection.Setup(m => m.StoreConnection)
                    .Returns(new Mock<DbConnection>().Object);
                entityAdapter.Connection = new EntityConnection(mockInternalEntityConnection.Object);

                Assert.Equal(
                    Strings.EntityClient_NoStoreConnectionForUpdate,
                    Assert.Throws<InvalidOperationException>(() => entityAdapter.UpdateAsync(mockEntityStateManager.Object, CancellationToken.None)).Message);
            }

            [Fact]
            public void Throws_if_connection_is_closed()
            {
                var entityAdapter = new EntityAdapter();
                var mockEntityStateManager = new Mock<IEntityStateManager>();
                var mockEntityStateEntry = new Mock<IEntityStateEntry>();
                mockEntityStateManager.Setup(m => m.GetEntityStateEntries(It.IsAny<EntityState>()))
                    .Returns(new[] { mockEntityStateEntry.Object });

                var mockInternalEntityConnection = new Mock<InternalEntityConnection>();
                mockInternalEntityConnection.Setup(m => m.StoreConnection)
                    .Returns(new Mock<DbConnection>().Object);
                mockInternalEntityConnection.Setup(m => m.StoreProviderFactory)
                    .Returns(new Mock<DbProviderFactory>().Object);
                entityAdapter.Connection = new EntityConnection(mockInternalEntityConnection.Object);

                Assert.Equal(
                    Strings.EntityClient_ClosedConnectionForUpdate,
                    Assert.Throws<InvalidOperationException>(() => entityAdapter.UpdateAsync(mockEntityStateManager.Object, CancellationToken.None)).Message);
            }
        }
    }
}
