// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using Moq;
    using Moq.Protected;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Migrations.History;
    using System.IO;
    using Xunit;
    using System.Threading;

    public class LazyInternalContextTests : TestBase
    {
        [Fact]
        public void CreateModel_uses_DbCompiledModel_from_ModelStore_when_available()
        {
            var store = new Mock<DbModelStore>();

            var dbCompiledModelInStore = new DbCompiledModel();
            store.Setup(c => c.TryLoad(It.IsAny<Type>())).Returns(dbCompiledModelInStore);
            store.Setup(c => c.Save(It.IsAny<Type>(), It.IsAny<DbModel>()));

            try
            {
                var dependencyResolver = new SingletonDependencyResolver<DbModelStore>(store.Object);
                MutableResolver.AddResolver<DbModelStore>(dependencyResolver);

                var mockContext = new Mock<LazyInternalContext>(
                    new Mock<DbContext>().Object, new Mock<IInternalConnection>().Object, null, null, null, null, null)
                {
                    CallBase = true
                };

                var model = LazyInternalContext.CreateModel(mockContext.Object);
             
                Assert.Same(dbCompiledModelInStore, model);

                store.Verify(c => c.TryLoad(It.IsAny<Type>()), Times.Once(),
                    "should load existing model");

                store.Verify(c => c.Save(It.IsAny<Type>(), It.IsAny<DbModel>()), Times.Never(),
                    "should not call Save when loading model from store");
            }
            finally //clean up
            {
                MutableResolver.ClearResolvers();
            }
        }

        [Fact]
        public void CreateModel_does_not_use_ModelStore_for_HistoryContext()
        {
            var store = new Mock<DbModelStore>();

            var dbCompiledModelInStore = new DbCompiledModel();
            store.Setup(c => c.TryLoad(It.IsAny<Type>())).Returns(dbCompiledModelInStore);
            store.Setup(c => c.Save(It.IsAny<Type>(), It.IsAny<DbModel>()));

            try
            {
                var dependencyResolver = new SingletonDependencyResolver<DbModelStore>(store.Object);
                MutableResolver.AddResolver<DbModelStore>(dependencyResolver);

                var mockContext = new Mock<LazyInternalContext>(
                    new MockHistoryContext(), new Mock<IInternalConnection>().Object, null, null, null, null, null)
                {
                    CallBase = true
                };
                mockContext.Object.ModelProviderInfo = ProviderRegistry.Sql2008_ProviderInfo;

                var model = LazyInternalContext.CreateModel(mockContext.Object);

                Assert.NotSame(dbCompiledModelInStore, model);

                store.Verify(c => c.TryLoad(It.IsAny<Type>()), Times.Never(),
                    "should not call store for HistoryContext");

                store.Verify(c => c.Save(It.IsAny<Type>(), It.IsAny<DbModel>()), Times.Never(),
                    "should not call store for HistoryContext");
            }
            finally //clean up
            {
                MutableResolver.ClearResolvers();
            }
        }

        public class MockHistoryContext : HistoryContext
        {
        }

        [Fact]
        public void CreateModel_saves_DbCompiledModel_into_ModelStore_when_not_yet_stored()
        {
            var store = new Mock<DbModelStore>();

            store.Setup(c => c.TryLoad(It.IsAny<Type>())).Returns((DbCompiledModel)null); //no file exists yet
            store.Setup(c => c.Save(It.IsAny<Type>(), It.IsAny<DbModel>()));

            try
            {
                var dependencyResolver = new SingletonDependencyResolver<DbModelStore>(store.Object);
                MutableResolver.AddResolver<DbModelStore>(dependencyResolver);

                var mockContext = new Mock<LazyInternalContext>(
                    new Mock<DbContext>().Object, new Mock<IInternalConnection>().Object, null, null, null, null, null)
                {
                    CallBase = true
                };
                mockContext.Object.ModelProviderInfo = ProviderRegistry.Sql2008_ProviderInfo;

                var model = LazyInternalContext.CreateModel(mockContext.Object);
                
                Assert.NotNull(model);

                store.Verify(c => c.TryLoad(It.IsAny<Type>()), Times.Once(),
                    "should try to load existing model");

                store.Verify(c => c.Save(It.IsAny<Type>(), It.IsAny<DbModel>()), Times.Once(),
                    "should Save after creating model when store did not contain exist yet");
            }
            finally //clean up
            {
                MutableResolver.ClearResolvers();
            }
        }

        [Fact]
        public void CommandTimeout_is_obtained_from_and_set_in_ObjectContext_if_ObjectContext_exists()
        {
            var mockContext = new Mock<LazyInternalContext>(
                new Mock<DbContext>().Object, new Mock<IInternalConnection>().Object, null, null, null, null, null)
                {
                    CallBase = true
                };
            var objectContext = new ObjectContext
                {
                    CommandTimeout = 77
                };
            mockContext.Setup(m => m.ObjectContextInUse).Returns(objectContext);

            Assert.Equal(77, mockContext.Object.CommandTimeout);

            mockContext.Object.CommandTimeout = 88;

            Assert.Equal(88, objectContext.CommandTimeout);
        }

        [Fact]
        public void CommandTimeout_is_obtained_from_and_set_in_field_if_ObjectContext_does_not_exist()
        {
            var mockContext = new Mock<LazyInternalContext>(
                new Mock<DbContext>().Object, new Mock<IInternalConnection>().Object, null, null, null, null, null)
                {
                    CallBase = true
                };
            mockContext.Setup(m => m.ObjectContextInUse).Returns((ObjectContext)null);

            Assert.Null(mockContext.Object.CommandTimeout);

            mockContext.Object.CommandTimeout = 88;

            Assert.Equal(88, mockContext.Object.CommandTimeout);
        }

        [Fact]
        public void Dispose_calls_EntityConnection_dispose_before_InternalConnection_dispose()
        {
            var results = new List<string>();

            var underlyingConnection = new Mock<DbConnection>();
            
            var lazyInternalConnectionMock = new Mock<LazyInternalConnection>("fake");
            lazyInternalConnectionMock.SetupGet(ic => ic.Connection).Returns(underlyingConnection.Object);
            lazyInternalConnectionMock.Setup(ic => ic.Dispose()).Callback(() => results.Add("LazyInternalConnection Dispose() called"));

            var metadataWorkspaceMock = new Mock<MetadataWorkspace>();
            metadataWorkspaceMock.Setup(mw => mw.GetItemCollection(DataSpace.CSSpace)).Returns(() => null);
            
            var dbContext = new TestDbContext();

            var entityConnectionMock = new Mock<EntityConnection>();
            entityConnectionMock.SetupGet(ec => ec.ConnectionString).Returns("fake");
            entityConnectionMock.Setup(ec => ec.GetMetadataWorkspace()).Returns(metadataWorkspaceMock.Object);
            entityConnectionMock.Protected().Setup("Dispose", ItExpr.IsAny<bool>()).
                Callback<bool>(b => results.Add("EntityConnection Dispose() called"));

            var objectContextMock = new Mock<ObjectContext>(entityConnectionMock.Object, true)
            {
                CallBase = true
            };
            objectContextMock.SetupGet(oc => oc.MetadataWorkspace).Returns(metadataWorkspaceMock.Object);

            var lazyInternalContext = new LazyInternalContext(
                dbContext, lazyInternalConnectionMock.Object, null, null, null, null, objectContextMock.Object);
            
            lazyInternalContext.DisposeContext(true);

            Assert.Equal(2, results.Count);
            Assert.Same("EntityConnection Dispose() called", results[0]);
            Assert.Same("LazyInternalConnection Dispose() called", results[1]);
        }

        public class TestDbContext : DbContext
        {
            internal override void InitializeLazyInternalContext(IInternalConnection internalConnection, DbCompiledModel model = null)
            {
                // do nothing - just placeholder object
            }
        }

#if !NET40
        [Fact]
        public void SaveChangesAsync_throws_OperationCanceledException_if_task_is_cancelled()
        {
            var lazyInternalContext = new Mock<LazyInternalContext>(
                new Mock<DbContext>().Object, new Mock<IInternalConnection>().Object, null, null, null, null, null)
            {
                CallBase = true
            }.Object;

            Assert.Throws<OperationCanceledException>(
                () => lazyInternalContext.SaveChangesAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

#endif

    }
}
