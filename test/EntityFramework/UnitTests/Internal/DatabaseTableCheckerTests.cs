// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal.MockingProxies;
    using System.Data.SqlClient;
    using System.Linq;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class DatabaseTableCheckerTests : TestBase
    {
        [Fact]
        public void AnyModelTableExists_uses_ExecutionStrategy()
        {
            var connectionMock = new Mock<DbConnection>
            {
                CallBase = true
            };
            var internalContextMock = new Mock<InternalContext>();

            var mockOperations = new Mock<DatabaseOperations>();
            mockOperations.Setup(m => m.Exists(It.IsAny<ObjectContext>())).Returns(true);
            internalContextMock.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);

            var dbCommandMock = new Mock<DbCommand>();

            SetupMocksForTableChecking(dbCommandMock, connectionMock, internalContextMock);

            var executionStrategyMock = new Mock<IDbExecutionStrategy>();
            // Verify that ExecutionStrategy.Execute calls DbCommand.ExecuteDataReader
            executionStrategyMock.Setup(m => m.Execute(It.IsAny<Func<List<Tuple<string, string>>>>()))
                                 .Returns<Func<List<Tuple<string, string>>>>(
                                     f =>
                                     {
                                         dbCommandMock.Protected().Verify<DbDataReader>(
                                             "ExecuteDbDataReader", Times.Never(), It.IsAny<CommandBehavior>());
                                         var result = f();
                                         dbCommandMock.Protected().Verify<DbDataReader>(
                                             "ExecuteDbDataReader", Times.Once(), It.IsAny<CommandBehavior>());
                                         return result;
                                     });

            MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
            try
            {
                new DatabaseTableChecker().AnyModelTableExists(internalContextMock.Object);
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }

            // Finally verify that ExecutionStrategy.Execute was called
            executionStrategyMock.Verify(m => m.Execute(It.IsAny<Func<List<Tuple<string, string>>>>()), Times.Once());
        }

        [Fact]
        public void AnyModelTableExists_dispatches_to_interceptors()
        {
            var connectionMock = new Mock<DbConnection>
                {
                    CallBase = true
                };
            var internalContextMock = new Mock<InternalContext>();
            var dbCommandMock = new Mock<DbCommand>();

            var mockOperations = new Mock<DatabaseOperations>();
            mockOperations.Setup(m => m.Exists(It.IsAny<ObjectContext>())).Returns(true);
            internalContextMock.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);

            SetupMocksForTableChecking(dbCommandMock, connectionMock, internalContextMock);

            var interceptorMock = new Mock<DbCommandInterceptor>
                {
                    CallBase = true
                };
            Interception.AddInterceptor(interceptorMock.Object);
            try
            {
                new DatabaseTableChecker().AnyModelTableExists(internalContextMock.Object);
            }
            finally
            {
                Interception.RemoveInterceptor(interceptorMock.Object);
            }

            interceptorMock.Verify(
                m => m.ReaderExecuting(
                    dbCommandMock.Object,
                    It.Is<DbCommandInterceptionContext<DbDataReader>>(c => c.ObjectContexts.Contains(internalContextMock.Object.ObjectContext))));
        }

        [Fact]
        public void AnyModelTableExists_returns_false_if_database_does_not_exist()
        {
            var mockOperations = new Mock<DatabaseOperations>();
            mockOperations.Setup(m => m.Exists(It.IsAny<ObjectContext>())).Returns(false);

            var internalContextMock = new Mock<InternalContext>();
            internalContextMock.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);
            internalContextMock.Setup(m => m.CreateObjectContextForDdlOps()).Returns(new Mock<ClonedObjectContext>().Object);

            Assert.False(new DatabaseTableChecker().AnyModelTableExists(internalContextMock.Object));
        }

        private static void SetupMocksForTableChecking(
            Mock<DbCommand> dbCommandMock, Mock<DbConnection> connectionMock, Mock<InternalContext> internalContextMock)
        {
            var dataReader = Core.Common.Internal.Materialization.MockHelper.CreateDbDataReader();

            dbCommandMock.Protected().Setup<DbDataReader>(
                "ExecuteDbDataReader", It.IsAny<CommandBehavior>())
                         .Returns(dataReader);

            connectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(dbCommandMock.Object);
            connectionMock.Setup(m => m.ConnectionString).Returns("FakeConnection");
            connectionMock.Setup(m => m.DataSource).Returns("Foo");
            connectionMock.Protected().Setup<DbProviderFactory>("DbProviderFactory").Returns(SqlClientFactory.Instance);

            var entityType = new EntityType(
                "FakeEntityType", "FakeNamespace", DataSpace.CSpace, new[] { "key" }, new EdmMember[] { new EdmProperty("key") });
            var entitySet = new EntitySet("FakeSet", "FakeSchema", "FakeTable", null, entityType);

            var entityContainer = new EntityContainer("C", DataSpace.SSpace);
            entityContainer.AddEntitySetBase(entitySet);

            var mockObjectContext = Core.Objects.MockHelper.CreateMockObjectContext<object>();

            var storeItemCollectionMock =
                Mock.Get((StoreItemCollection)mockObjectContext.MetadataWorkspace.GetItemCollection(DataSpace.SSpace));
            storeItemCollectionMock
                .Setup(m => m.GetItems<EntityContainer>())
                .Returns(
                    new ReadOnlyCollection<EntityContainer>(
                        new List<EntityContainer>
                            {
                                entityContainer
                            }));

            var clonedObjectContextMock = new Mock<ClonedObjectContext>();
            clonedObjectContextMock.Setup(m => m.Connection).Returns(connectionMock.Object);
            clonedObjectContextMock.Setup(m => m.ObjectContext).Returns(new ObjectContextProxy(mockObjectContext));

            internalContextMock.Setup(m => m.CodeFirstModel).Returns(new DbCompiledModel());
            internalContextMock.Setup(m => m.ProviderName).Returns("System.Data.SqlClient");
            internalContextMock.Setup(m => m.ObjectContext).Returns(mockObjectContext);
            internalContextMock.Setup(m => m.CreateObjectContextForDdlOps()).Returns(clonedObjectContextMock.Object);
        }
    }
}
