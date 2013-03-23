// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class DatabaseTableCheckerTests : TestBase
    {
        public class FakeContext : DbContext
        {
            private readonly InternalContext _internalContext;

            internal FakeContext(DbConnection connection, InternalContext internalContext)
                : base(connection, contextOwnsConnection: false)
            {
                _internalContext = internalContext;
            }

            internal override InternalContext InternalContext
            {
                get { return _internalContext; }
            }
        }

        [Fact]
        public void AnyModelTableExists_uses_ExecutionStrategy()
        {
            var dbCommandMock = new Mock<DbCommand>();
            var dataReader = Core.Common.Internal.Materialization.MockHelper.CreateDbDataReader();

            dbCommandMock.Protected().Setup<DbDataReader>(
                "ExecuteDbDataReader", It.IsAny<CommandBehavior>())
                         .Returns(dataReader);

            var executionStrategyMock = new Mock<IExecutionStrategy>();

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

            var connectionMock = new Mock<DbConnection>
                                     {
                                         CallBase = true
                                     };
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

            var internalContextMock = new Mock<InternalContext>();
            internalContextMock.Setup(m => m.CodeFirstModel).Returns(new DbCompiledModel());
            internalContextMock.Setup(m => m.ProviderName).Returns("System.Data.SqlClient");
            internalContextMock.Setup(m => m.ObjectContext).Returns(mockObjectContext);
            internalContextMock.Setup(m => m.CreateObjectContextForDdlOps()).Returns(clonedObjectContextMock.Object);

            MutableResolver.AddResolver<Func<IExecutionStrategy>>(key => (Func<IExecutionStrategy>)(() => executionStrategyMock.Object));
            try
            {
                new DatabaseTableChecker().AnyModelTableExists(new FakeContext(connectionMock.Object, internalContextMock.Object));
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }

            // Finally verify that ExecutionStrategy.Execute was called
            executionStrategyMock.Verify(m => m.Execute(It.IsAny<Func<List<Tuple<string, string>>>>()), Times.Once());
        }
    }
}
