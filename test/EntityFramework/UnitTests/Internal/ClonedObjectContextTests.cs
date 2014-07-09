// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Internal.MockingProxies;
    using System.Data.Entity.Utilities;
    using System.Data.SqlClient;
    using System.Reflection;
    using Moq;
    using SimpleModel;
    using Xunit;

    public class ClonedObjectContextTests : TestBase
    {
        [Fact]
        public void Creating_a_cloned_ObjectContext_causes_the_store_and_entity_connection_to_be_cloned_and_given_connection_string_applied(

            )
        {
            var storeConnection = new SqlConnection();
            var mockConnection = CreateMockConnection(storeConnection);
            var mockContext = CreateMockObjectContext(mockConnection);

            new ClonedObjectContext(mockContext.Object, null, "Database=PinkyDinkyDo");

            mockConnection.Verify(
                m =>
                    m.CreateNew(
                        It.Is<SqlConnection>(
                            p =>
                                p != null && !ReferenceEquals(p, storeConnection) &&
                                p.ConnectionString == "Database=PinkyDinkyDo")));
        }

        [Fact]
        public void Cloning_an_ObjectContext_with_a_default_container_name_copies_that_container_name()
        {
            var mockClonedContext = new Mock<ObjectContextProxy>();
            var mockContext = CreateMockObjectContext(CreateMockConnection(), mockClonedContext);
            mockContext.Setup(m => m.DefaultContainerName).Returns("Kipper");

            var clonedContext = new ClonedObjectContext(mockContext.Object, null, "Database=PinkyDinkyDo");

            mockClonedContext.VerifySet(m => m.DefaultContainerName = It.IsAny<string>());
            Assert.Equal("Kipper", clonedContext.ObjectContext.DefaultContainerName);
        }

        [Fact]
        public void Cloning_an_ObjectContext_with_a_null_default_container_name_results_in_a_clone_without_a_default_container_name()
        {
            var mockClonedContext = new Mock<ObjectContextProxy>();
            var mockContext = CreateMockObjectContext(CreateMockConnection(), mockClonedContext);

            var clonedContext = new ClonedObjectContext(mockContext.Object, null, "Database=PinkyDinkyDo");

            mockClonedContext.VerifySet(m => m.DefaultContainerName = It.IsAny<string>(), Times.Never());
            Assert.Null(clonedContext.ObjectContext.DefaultContainerName);
        }

        [Fact]
        public void Cloning_an_ObjectContext_with_a_whitespace_default_container_name_results_in_a_clone_without_a_default_container_name()
        {
            var mockClonedContext = new Mock<ObjectContextProxy>();
            var mockContext = CreateMockObjectContext(CreateMockConnection(), mockClonedContext);
            mockContext.Setup(m => m.DefaultContainerName).Returns(" ");

            var clonedContext = new ClonedObjectContext(mockContext.Object, null, "Database=PinkyDinkyDo");

            mockClonedContext.VerifySet(m => m.DefaultContainerName = It.IsAny<string>(), Times.Never());
            Assert.Null(clonedContext.ObjectContext.DefaultContainerName);
        }

        [Fact]
        public void ClonedObjectContext_Connection_returns_the_cloned_store_connection()
        {
            var storeConnection = new SqlConnection();
            var mockContext = CreateMockObjectContext(CreateMockConnection(storeConnection));

            var clonedConnection = new ClonedObjectContext(mockContext.Object, null, "Database=PinkyDinkyDo").Connection;

            Assert.NotSame(storeConnection, clonedConnection);
            Assert.Equal("Database=PinkyDinkyDo", clonedConnection.ConnectionString);
            Assert.Same(storeConnection.GetType(), clonedConnection.GetType());
        }

        [Fact]
        public void ClonedObjectContext_ObjectContext_returns_the_cloned_ObjectContext()
        {
            var mockClonedContext = new Mock<ObjectContextProxy>();
            var mockContext = CreateMockObjectContext(CreateMockConnection(), mockClonedContext);

            var clonedContext = new ClonedObjectContext(mockContext.Object, null, "Database=PinkyDinkyDo");

            mockContext.Verify(m => m.CreateNew(It.IsAny<EntityConnectionProxy>()));
            Assert.Same(mockClonedContext.Object, clonedContext.ObjectContext);
        }

        [Fact]
        public void When_cloning_an_ObjectContext_interception_is_used()
        {
            var mockClonedContext = new Mock<ObjectContextProxy>();
            var mockContext = CreateMockObjectContext(CreateMockConnection(), mockClonedContext);

            var dbConnectionInterceptorMock = new Mock<IDbConnectionInterceptor>();
            DbInterception.Add(dbConnectionInterceptorMock.Object);
            try
            {
                new ClonedObjectContext(
                    mockContext.Object, null, "Database=PinkyDinkyDo",
                    transferLoadedAssemblies: false);
            }
            finally
            {
                DbInterception.Remove(dbConnectionInterceptorMock.Object);
            }
        }

        [Fact]
        public void When_cloning_an_ObjectContext_without_transfering_assemblies_no_assemblies_are_transfered()
        {
            var mockClonedContext = new Mock<ObjectContextProxy>();
            var mockContext = CreateMockObjectContext(CreateMockConnection(), mockClonedContext);

            new ClonedObjectContext(
                mockContext.Object, null, "Database=PinkyDinkyDo",
                transferLoadedAssemblies: false);

            mockContext.Verify(m => m.GetObjectItemCollection(), Times.Never());
            mockClonedContext.Verify(m => m.LoadFromAssembly(It.IsAny<Assembly>()), Times.Never());
        }

        [Fact]
        public void
            When_cloning_an_ObjectContext_with_assemblies_enum_complex_and_entity_type_assemblies_are_transfered_but_others_are_ignored()
        {
            var mockClonedContext = new Mock<ObjectContextProxy>();
            var mockContext = CreateMockObjectContext(CreateMockConnection(), mockClonedContext);

            new ClonedObjectContext(mockContext.Object, null, "Database=PinkyDinkyDo");

            mockContext.Verify(m => m.GetObjectItemCollection(), Times.Once());
            mockClonedContext.Verify(m => m.LoadFromAssembly(typeof(object).Assembly()));
            mockClonedContext.Verify(m => m.LoadFromAssembly(GetType().Assembly()));
            mockClonedContext.Verify(m => m.LoadFromAssembly(typeof(ExtraEntity).Assembly()));
        }

        [Fact]
        public void Disposing_a_cloned_ObjectContext_disposes_both_the_context_and_the_connection()
        {
            var mockClonedContext = new Mock<ObjectContextProxy>();
            var mockContext = CreateMockObjectContext(CreateMockConnection(), mockClonedContext);

            var clonedContext = new ClonedObjectContext(mockContext.Object, null, "Database=PinkyDinkyDo");

            var connectionIsDisposed = false;
            clonedContext.Connection.Disposed += (_, __) => connectionIsDisposed = true;

            clonedContext.Dispose();

            mockClonedContext.Verify(m => m.Dispose());
            Assert.True(connectionIsDisposed);
        }

        [Fact]
        public void Calling_Dispose_on_an_already_disposed_cloned_ObjectContext_does_nothing()
        {
            var mockClonedContext = new Mock<ObjectContextProxy>();
            var mockContext = CreateMockObjectContext(CreateMockConnection(), mockClonedContext);

            var clonedContext = new ClonedObjectContext(mockContext.Object, null, "Database=PinkyDinkyDo");

            var connectionIsDisposed = false;
            clonedContext.Connection.Disposed += (_, __) => connectionIsDisposed = true;

            clonedContext.Dispose();
            connectionIsDisposed = false;

            clonedContext.Dispose();

            mockClonedContext.Verify(m => m.Dispose(), Times.Once());
            Assert.False(connectionIsDisposed);
        }

        [Fact]
        public void ClonedObjectContext_disposes_of_connections_in_correct_order()
        {
            var myConnectionInterceptor = new ConnectionDisposingInterceptor();
            DbInterception.Add(myConnectionInterceptor);
            try
            {
                var mockClonedContext = new Mock<ObjectContextProxy>();

                var storeConnection = new SqlConnection();
                var mockEntityConnection = new Mock<EntityConnectionProxy>();
                mockEntityConnection.Setup(m => m.StoreConnection).Returns(storeConnection);
                
                mockEntityConnection.Setup(m => m.CreateNew(It.IsAny<SqlConnection>())).Returns<SqlConnection>(
                    c =>
                    {
                        var mockClonedConnection = new Mock<EntityConnectionProxy>();
                        mockClonedConnection.Setup(cc => cc.StoreConnection).Returns(c);
                        mockClonedConnection.Setup(m => m.Dispose()).Callback(() => myConnectionInterceptor.IsClonedEntityConnectionDisposed = true);

                        return mockClonedConnection.Object;
                    });

                var mockContext = CreateMockObjectContext(mockEntityConnection, mockClonedContext);
                var clonedContext = new ClonedObjectContext(mockContext.Object, null, "Database=PinkyDinkyDo");

                clonedContext.Dispose();
            }
            finally
            {
                DbInterception.Remove(myConnectionInterceptor);
            }
        }

        private class ConnectionDisposingInterceptor : IDbConnectionInterceptor
        {
            public bool IsClonedEntityConnectionDisposed { get; set; }

            public void BeginningTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
            {
            }

            public void BeganTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
            {
            }

            public void Closing(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
            }

            public void Closed(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
            }

            public void ConnectionStringGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void ConnectionStringGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void ConnectionStringSetting(DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext)
            {
            }

            public void ConnectionStringSet(DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext)
            {
            }

            public void ConnectionTimeoutGetting(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext)
            {
            }

            public void ConnectionTimeoutGot(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext)
            {
            }

            public void DatabaseGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void DatabaseGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void DataSourceGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void DataSourceGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void Disposing(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
                Assert.True(IsClonedEntityConnectionDisposed, "EntityConnection should be disposed of before underlying store connection.");
            }

            public void Disposed(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
            }

            public void EnlistingTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext)
            {
            }

            public void EnlistedTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext)
            {
            }

            public void Opening(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
            }

            public void Opened(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
            }

            public void ServerVersionGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void ServerVersionGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void StateGetting(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext)
            {
            }

            public void StateGot(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext)
            {
            }
        }

        private Mock<EntityConnectionProxy> CreateMockConnection(SqlConnection storeConnection = null)
        {
            storeConnection = storeConnection ?? new SqlConnection();

            var mockConnection = new Mock<EntityConnectionProxy>();
            mockConnection.Setup(m => m.StoreConnection).Returns(storeConnection);

            mockConnection.Setup(m => m.CreateNew(It.IsAny<SqlConnection>())).Returns<SqlConnection>(
                c =>
                {
                    var mockClonedConnection = new Mock<EntityConnectionProxy>();
                    mockClonedConnection.Setup(cc => cc.StoreConnection).Returns(c);
                    return mockClonedConnection.Object;
                });

            return mockConnection;
        }

        private Mock<ObjectContextProxy> CreateMockObjectContext(
            Mock<EntityConnectionProxy> mockConnection = null, Mock<ObjectContextProxy> mockClonedContext = null)
        {
            mockConnection = mockConnection ?? CreateMockConnection();

            mockClonedContext = mockClonedContext ?? new Mock<ObjectContextProxy>();
            mockClonedContext.SetupProperty(m => m.DefaultContainerName);

            var mockContext = new Mock<ObjectContextProxy>();
            mockContext.Setup(m => m.Connection).Returns(mockConnection.Object);

            mockContext.Setup(m => m.GetObjectItemCollection()).Returns(
                new List<GlobalItem>
                {
                    CreateFakeComplexType(),
                    CreateFakeEntityType(),
                    CreateFakeEnumType(),
                    CreateFakePrimitiveType(),
                });

            mockContext.Setup(m => m.GetClrType(It.IsAny<ComplexType>())).Returns(typeof(object));
            mockContext.Setup(m => m.GetClrType(It.IsAny<EntityType>())).Returns(GetType());
            mockContext.Setup(m => m.GetClrType(It.IsAny<EnumType>())).Returns(typeof(ExtraEntity));

            mockContext.Setup(m => m.CreateNew(It.IsAny<EntityConnectionProxy>())).Returns<EntityConnectionProxy>(
                c =>
                {
                    mockClonedContext.Setup(cc => cc.Connection).Returns(c);
                    return mockClonedContext.Object;
                });

            return mockContext;
        }

        private ComplexType CreateFakeComplexType()
        {
            return (ComplexType)Activator.CreateInstance(typeof(ComplexType), nonPublic: true);
        }

        private EnumType CreateFakeEnumType()
        {
            return (EnumType)Activator.CreateInstance(typeof(EnumType), nonPublic: true);
        }

        private PrimitiveType CreateFakePrimitiveType()
        {
            return (PrimitiveType)Activator.CreateInstance(typeof(PrimitiveType), nonPublic: true);
        }

        private EntityType CreateFakeEntityType()
        {
            const BindingFlags bindingFlags = BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Instance;
            return (EntityType)Activator.CreateInstance(
                typeof(EntityType),
                bindingFlags,
                null,
                new object[] { "FakeEntity", "FakeNamespace", DataSpace.OSpace },
                null);
        }

        /// <summary>
        /// Used because Moq cannot mock Assembly.
        /// </summary>
        public class FakeAssembly : Assembly
        {
            private readonly string _name;

            public FakeAssembly(string name)
            {
                _name = name;
            }

            public override string FullName
            {
                get { return _name; }
            }
        }
    }
}
