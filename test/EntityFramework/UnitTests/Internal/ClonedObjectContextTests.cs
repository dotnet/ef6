// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
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

            new ClonedObjectContext(mockContext.Object, "Database=PinkyDinkyDo");

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

            var clonedContext = new ClonedObjectContext(mockContext.Object, "Database=PinkyDinkyDo");

            mockClonedContext.VerifySet(m => m.DefaultContainerName = It.IsAny<string>());
            Assert.Equal("Kipper", clonedContext.ObjectContext.DefaultContainerName);
        }

        [Fact]
        public void Cloning_an_ObjectContext_with_a_null_default_container_name_results_in_a_clone_without_a_default_container_name()
        {
            var mockClonedContext = new Mock<ObjectContextProxy>();
            var mockContext = CreateMockObjectContext(CreateMockConnection(), mockClonedContext);

            var clonedContext = new ClonedObjectContext(mockContext.Object, "Database=PinkyDinkyDo");

            mockClonedContext.VerifySet(m => m.DefaultContainerName = It.IsAny<string>(), Times.Never());
            Assert.Null(clonedContext.ObjectContext.DefaultContainerName);
        }

        [Fact]
        public void Cloning_an_ObjectContext_with_a_whitespace_default_container_name_results_in_a_clone_without_a_default_container_name()
        {
            var mockClonedContext = new Mock<ObjectContextProxy>();
            var mockContext = CreateMockObjectContext(CreateMockConnection(), mockClonedContext);
            mockContext.Setup(m => m.DefaultContainerName).Returns(" ");

            var clonedContext = new ClonedObjectContext(mockContext.Object, "Database=PinkyDinkyDo");

            mockClonedContext.VerifySet(m => m.DefaultContainerName = It.IsAny<string>(), Times.Never());
            Assert.Null(clonedContext.ObjectContext.DefaultContainerName);
        }

        [Fact]
        public void ClonedObjectContext_Connection_returns_the_cloned_store_connection()
        {
            var storeConnection = new SqlConnection();
            var mockContext = CreateMockObjectContext(CreateMockConnection(storeConnection));

            var clonedConnection = new ClonedObjectContext(mockContext.Object, "Database=PinkyDinkyDo").Connection;

            Assert.NotSame(storeConnection, clonedConnection);
            Assert.Equal("Database=PinkyDinkyDo", clonedConnection.ConnectionString);
            Assert.Same(storeConnection.GetType(), clonedConnection.GetType());
        }

        [Fact]
        public void ClonedObjectContext_ObjectContext_returns_the_cloned_ObjectContext()
        {
            var mockClonedContext = new Mock<ObjectContextProxy>();
            var mockContext = CreateMockObjectContext(CreateMockConnection(), mockClonedContext);

            var clonedContext = new ClonedObjectContext(mockContext.Object, "Database=PinkyDinkyDo");

            mockContext.Verify(m => m.CreateNew(It.IsAny<EntityConnectionProxy>()));
            Assert.Same(mockClonedContext.Object, clonedContext.ObjectContext);
        }

        [Fact]
        public void When_cloning_an_ObjectContext_without_transfering_assemblies_no_assemblies_are_transfered()
        {
            var mockClonedContext = new Mock<ObjectContextProxy>();
            var mockContext = CreateMockObjectContext(CreateMockConnection(), mockClonedContext);

            new ClonedObjectContext(
                mockContext.Object, "Database=PinkyDinkyDo",
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

            new ClonedObjectContext(mockContext.Object, "Database=PinkyDinkyDo");

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

            var clonedContext = new ClonedObjectContext(mockContext.Object, "Database=PinkyDinkyDo");

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

            var clonedContext = new ClonedObjectContext(mockContext.Object, "Database=PinkyDinkyDo");

            var connectionIsDisposed = false;
            clonedContext.Connection.Disposed += (_, __) => connectionIsDisposed = true;

            clonedContext.Dispose();
            connectionIsDisposed = false;

            clonedContext.Dispose();

            mockClonedContext.Verify(m => m.Dispose(), Times.Once());
            Assert.False(connectionIsDisposed);
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
