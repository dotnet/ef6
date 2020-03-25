// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Interception;
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
            mockOperations.Setup(m => m.Exists(It.IsAny<DbConnection>(), It.IsAny<int?>(), It.IsAny<Lazy<StoreItemCollection>>())).Returns(true);
            internalContextMock.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);

            var dbCommandMock = new Mock<DbCommand>();
            
            SetupMocksForTableChecking(dbCommandMock, connectionMock, internalContextMock);

            var executionStrategyMock = new Mock<IDbExecutionStrategy>();
            // Verify that ExecutionStrategy.Execute calls DbCommand.ExecuteDataReader
            executionStrategyMock.Setup(m => m.Execute(It.IsAny < Func<bool>>()))
                                 .Returns<Func<bool>>(
                                     f =>
                                     {
                                         dbCommandMock.Verify(m => m.ExecuteScalar(), Times.Never());
                                         var result = f();
                                         dbCommandMock.Verify(m => m.ExecuteScalar(), Times.Once());
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
            executionStrategyMock.Verify(m => m.Execute(It.IsAny<Func<bool>>()), Times.Once());
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
            mockOperations.Setup(m => m.Exists(It.IsAny<DbConnection>(), It.IsAny<int?>(), It.IsAny<Lazy<StoreItemCollection>>())).Returns(true);
            internalContextMock.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);

            SetupMocksForTableChecking(dbCommandMock, connectionMock, internalContextMock);

            var interceptorMock = new Mock<DbCommandInterceptor>
                {
                    CallBase = true
                };
            DbInterception.Add(interceptorMock.Object);
            try
            {
                new DatabaseTableChecker().AnyModelTableExists(internalContextMock.Object);
            }
            finally
            {
                DbInterception.Remove(interceptorMock.Object);
            }

            interceptorMock.Verify(
                m => m.ScalarExecuting(
                    dbCommandMock.Object,
                    It.Is<DbCommandInterceptionContext<object>>(c => c.ObjectContexts.Contains(internalContextMock.Object.ObjectContext))));
        }

        [Fact]
        public void AnyModelTableExists_returns_DoesNotExist_if_database_does_not_exist()
        {
            var mockOperations = new Mock<DatabaseOperations>();
            mockOperations.Setup(m => m.Exists(It.IsAny<DbConnection>(), It.IsAny<int?>(), It.IsAny<Lazy<StoreItemCollection>>())).Returns(false);

            var internalContextMock = new Mock<InternalContext>();
            internalContextMock.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);
            internalContextMock.Setup(m => m.CreateObjectContextForDdlOps()).Returns(new Mock<ClonedObjectContext>().Object);

            Assert.Equal(DatabaseExistenceState.DoesNotExist, new DatabaseTableChecker().AnyModelTableExists(internalContextMock.Object));
        }

        [Fact]
        public void AnyModelTableExists_returns_Exists_if_database_exists_and_not_Code_First()
        {
            var mockOperations = new Mock<DatabaseOperations>();
            mockOperations.Setup(m => m.Exists(It.IsAny<DbConnection>(), It.IsAny<int?>(), It.IsAny<Lazy<StoreItemCollection>>())).Returns(true);

            var internalContextMock = new Mock<InternalContext>();
            internalContextMock.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);
            internalContextMock.Setup(m => m.CreateObjectContextForDdlOps()).Returns(new Mock<ClonedObjectContext>().Object);

            Assert.Equal(DatabaseExistenceState.Exists, new DatabaseTableChecker().AnyModelTableExists(internalContextMock.Object));
        }

        [Fact]
        public void AnyModelTableExists_returns_Exists_if_provider_doesnt_support_table_checking()
        {
            var mockOperations = new Mock<DatabaseOperations>();
            mockOperations.Setup(m => m.Exists(It.IsAny<DbConnection>(), It.IsAny<int?>(), It.IsAny<Lazy<StoreItemCollection>>())).Returns(true);

            var internalContextMock = new Mock<InternalContext>();
            internalContextMock.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);
            internalContextMock.Setup(m => m.CreateObjectContextForDdlOps()).Returns(new Mock<ClonedObjectContext>().Object);
            internalContextMock.Setup(m => m.CodeFirstModel).Returns(new DbCompiledModel());
            internalContextMock.Setup(m => m.ProviderName).Returns("Access");

            Assert.Equal(DatabaseExistenceState.Exists, new DatabaseTableChecker().AnyModelTableExists(internalContextMock.Object));
        }

        [Fact]
        public void AnyModelTableExists_returns_Exists_if_model_is_empty()
        {
            var mockOperations = new Mock<DatabaseOperations>();
            mockOperations.Setup(m => m.Exists(It.IsAny<DbConnection>(), It.IsAny<int?>(), It.IsAny<Lazy<StoreItemCollection>>())).Returns(true);

            var internalContextMock = new Mock<InternalContext>();
            internalContextMock.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);
            internalContextMock.Setup(m => m.CreateObjectContextForDdlOps()).Returns(new Mock<ClonedObjectContext>().Object);
            internalContextMock.Setup(m => m.CodeFirstModel).Returns(new DbCompiledModel());
            internalContextMock.Setup(m => m.ProviderName).Returns("System.Data.SqlClient");

            var mockTableChecker = new Mock<DatabaseTableChecker> { CallBase = true };
            mockTableChecker.Setup(m => m.GetModelTables(It.IsAny<InternalContext>())).Returns(Enumerable.Empty<EntitySet>());

            Assert.Equal(DatabaseExistenceState.Exists, mockTableChecker.Object.AnyModelTableExists(internalContextMock.Object));
        }

        [Fact]
        public void AnyModelTableExists_returns_Exists_if_any_model_table_exists()
        {
            var mockOperations = new Mock<DatabaseOperations>();
            mockOperations.Setup(m => m.Exists(It.IsAny<DbConnection>(), It.IsAny<int?>(), It.IsAny<Lazy<StoreItemCollection>>())).Returns(true);

            var internalContextMock = new Mock<InternalContext>();
            internalContextMock.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);
            internalContextMock.Setup(m => m.CreateObjectContextForDdlOps()).Returns(new Mock<ClonedObjectContext>().Object);
            internalContextMock.Setup(m => m.CodeFirstModel).Returns(new DbCompiledModel());
            internalContextMock.Setup(m => m.ProviderName).Returns("System.Data.SqlClient");

            var mockTableChecker = new Mock<DatabaseTableChecker> { CallBase = true };
            mockTableChecker.Setup(m => m.GetModelTables(It.IsAny<InternalContext>())).Returns(new[] { new EntitySet() });
            mockTableChecker.Setup(
                m => m.QueryForTableExistence(
                    It.IsAny<TableExistenceChecker>(),
                    It.IsAny<ClonedObjectContext>(),
                    It.IsAny<List<EntitySet>>())).Returns(true);

            Assert.Equal(DatabaseExistenceState.Exists, mockTableChecker.Object.AnyModelTableExists(internalContextMock.Object));
        }

        [Fact]
        public void AnyModelTableExists_returns_Exists_if_history_table_with_entry_exists()
        {
            var mockOperations = new Mock<DatabaseOperations>();
            mockOperations.Setup(m => m.Exists(It.IsAny<DbConnection>(), It.IsAny<int?>(), It.IsAny<Lazy<StoreItemCollection>>())).Returns(true);

            var internalContextMock = new Mock<InternalContext>();
            internalContextMock.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);
            internalContextMock.Setup(m => m.CreateObjectContextForDdlOps()).Returns(new Mock<ClonedObjectContext>().Object);
            internalContextMock.Setup(m => m.CodeFirstModel).Returns(new DbCompiledModel());
            internalContextMock.Setup(m => m.ProviderName).Returns("System.Data.SqlClient");
            internalContextMock.Setup(m => m.HasHistoryTableEntry()).Returns(true);

            var mockTableChecker = new Mock<DatabaseTableChecker> { CallBase = true };
            mockTableChecker.Setup(m => m.GetModelTables(It.IsAny<InternalContext>())).Returns(new[] { new EntitySet() });
            mockTableChecker.Setup(
                m => m.QueryForTableExistence(
                    It.IsAny<TableExistenceChecker>(),
                    It.IsAny<ClonedObjectContext>(),
                    It.IsAny<List<EntitySet>>())).Returns(false);

            Assert.Equal(DatabaseExistenceState.Exists, mockTableChecker.Object.AnyModelTableExists(internalContextMock.Object));
        }

        [Fact]
        public void AnyModelTableExists_returns_ExistsConsideredEmpty_if_no_tables_and_no_history()
        {
            var mockOperations = new Mock<DatabaseOperations>();
            mockOperations.Setup(m => m.Exists(It.IsAny<DbConnection>(), It.IsAny<int?>(), It.IsAny<Lazy<StoreItemCollection>>())).Returns(true);

            var internalContextMock = new Mock<InternalContext>();
            internalContextMock.Setup(m => m.DatabaseOperations).Returns(mockOperations.Object);
            internalContextMock.Setup(m => m.CreateObjectContextForDdlOps()).Returns(new Mock<ClonedObjectContext>().Object);
            internalContextMock.Setup(m => m.CodeFirstModel).Returns(new DbCompiledModel());
            internalContextMock.Setup(m => m.ProviderName).Returns("System.Data.SqlClient");
            internalContextMock.Setup(m => m.HasHistoryTableEntry()).Returns(false);

            var mockTableChecker = new Mock<DatabaseTableChecker> { CallBase = true };
            mockTableChecker.Setup(m => m.GetModelTables(It.IsAny<InternalContext>())).Returns(new[] { new EntitySet() });
            mockTableChecker.Setup(
                m => m.QueryForTableExistence(
                    It.IsAny<TableExistenceChecker>(),
                    It.IsAny<ClonedObjectContext>(),
                    It.IsAny<List<EntitySet>>())).Returns(false);

            Assert.Equal(DatabaseExistenceState.ExistsConsideredEmpty, mockTableChecker.Object.AnyModelTableExists(internalContextMock.Object));
        }

        private static void SetupMocksForTableChecking(
            Mock<DbCommand> dbCommandMock, Mock<DbConnection> connectionMock, Mock<InternalContext> internalContextMock)
        {
            var dataReader = Core.Common.Internal.Materialization.MockHelper.CreateDbDataReader();

            dbCommandMock.Protected().Setup<DbParameter>("CreateDbParameter")
                .Returns(new Mock<DbParameter>().Object);
            dbCommandMock.Protected().Setup<DbParameterCollection>("DbParameterCollection")
                .Returns(new Mock<DbParameterCollection>().Object);
            dbCommandMock.Setup(m => m.ExecuteScalar()).Returns(0);
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
