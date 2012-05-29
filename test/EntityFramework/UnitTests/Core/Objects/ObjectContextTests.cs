namespace System.Data.Entity.Core.Objects
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Metadata.Internal;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class ObjectContextTests
    {
        public class SaveChanges
        {
            [Fact]
            public void Parameterless_SaveChanges_calls_SaveOption_flags_to_DetectChangesBeforeSave_and_AcceptAllChangesAfterSave()
            {
                var objectContextMock = new Mock<ObjectContextForMock>(null /*entityConnection*/);

                objectContextMock.Object.SaveChanges();
                var expectedSavedOptions = SaveOptions.DetectChangesBeforeSave | SaveOptions.AcceptAllChangesAfterSave;
                objectContextMock.Verify(m => m.SaveChanges(expectedSavedOptions), Times.Once());
            }

            [Fact]
            public void Calls_ObjectStateManager_DetectChanges_if_SaveOptions_is_set_to_DetectChangesBeforeSave()
            {
                var internalObjectStateManagerMock = new Mock<InternalObjectStateManager>();
                internalObjectStateManagerMock.Setup(m => m.SomeEntryWithConceptualNullExists()).Returns(false);
                internalObjectStateManagerMock.Setup(m => m.GetObjectStateEntriesCount(EntityState.Added | EntityState.Deleted | EntityState.Modified)).Returns(0);

                var internalEntityConnectionMock = new Mock<InternalEntityConnection>();
                internalEntityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");

                var objectContext = BasicObjectContextInitializationWithConnection(internalObjectStateManagerMock, internalEntityConnectionMock);

                objectContext.SaveChanges(SaveOptions.DetectChangesBeforeSave);

                internalObjectStateManagerMock.Verify(m => m.DetectChanges(), Times.Once());
            }

            [Fact]
            public void Exception_thrown_if_ObjectStateManager_has_entries_with_conceptual_nulls()
            {
                var internalObjectStateManagerMock = new Mock<InternalObjectStateManager>();
                internalObjectStateManagerMock.Setup(m => m.SomeEntryWithConceptualNullExists()).Returns(true);

                var internalEntityConnectionMock = new Mock<InternalEntityConnection>();
                internalEntityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");

                var objectContext = BasicObjectContextInitializationWithConnection(internalObjectStateManagerMock, internalEntityConnectionMock);

                Assert.Equal(
                    Strings.ObjectContext_CommitWithConceptualNull,
                    Assert.Throws<InvalidOperationException>(() => objectContext.SaveChanges(SaveOptions.None)).Message);
            }

            [Fact]
            public void Shortcircuits_if_no_state_changes()
            {
                var mockObjectContext = ObjectContextForMock.Create();
                var mockServiceProvider = (IServiceProvider)((EntityConnection)mockObjectContext.Connection).StoreProviderFactory;
                var entityAdapterMock = Mock.Get((IEntityAdapter)mockServiceProvider.GetService(typeof(IEntityAdapter)));
                entityAdapterMock.Setup(m => m.Update(It.IsAny<IEntityStateManager>())).Verifiable();

                int entriesAffected = mockObjectContext.SaveChanges(SaveOptions.None);

                entityAdapterMock.Verify(m => m.Update(It.IsAny<IEntityStateManager>()), Times.Never());
                Assert.Equal(0, entriesAffected);
            }

            [Fact]
            public void If_local_transaction_is_necessary_it_gets_created_commited()
            {
                var internalObjectStateManagerMock = new Mock<InternalObjectStateManager>();
                internalObjectStateManagerMock.Setup(m => m.GetObjectStateEntriesCount(EntityState.Added | EntityState.Deleted | EntityState.Modified)).Returns(1);

                var dbTransaction = new Mock<DbTransaction>();
                var internalEntityTransactionMock = new Mock<InternalEntityTransaction>(new EntityConnection(), dbTransaction.Object);
                var entityTransaction = new EntityTransaction(internalEntityTransactionMock.Object);

                var connectionState = ConnectionState.Closed;
                var internalEntityConnectionMock = new Mock<InternalEntityConnection>();
                internalEntityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                internalEntityConnectionMock.SetupGet(m => m.State).Returns(() => connectionState);
                internalEntityConnectionMock.Setup(m => m.Open()).Callback(() => connectionState = ConnectionState.Open);
                internalEntityConnectionMock.Setup(m => m.BeginDbTransaction(It.IsAny<IsolationLevel>())).Returns(() => entityTransaction);

                // first time return false to by-pass check in the constructor
                var enlistedInUserTransactionCallCount = 0;
                internalEntityConnectionMock.SetupGet(m => m.EnlistedInUserTransaction).
                    Callback(() => enlistedInUserTransactionCallCount++).
                    Returns(enlistedInUserTransactionCallCount == 1);

                var internalMetadataWorkspace = new Mock<InternalMetadataWorkspace>();
                internalMetadataWorkspace.Setup(m => m.ShallowCopy()).Returns(() => internalMetadataWorkspace.Object);
                internalMetadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.OSpace)).Returns(true);
                internalMetadataWorkspace.Setup(m => m.GetItemCollection(DataSpace.OCSpace)).Returns(default(ItemCollection));
                internalMetadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var objectContext = BasicObjectContextInitializationWithConnectionAndMetadata(internalObjectStateManagerMock, internalEntityConnectionMock, internalMetadataWorkspace);
                objectContext.SaveChanges(SaveOptions.None);

                internalEntityConnectionMock.Verify(m => m.BeginDbTransaction(It.IsAny<IsolationLevel>()), Times.Once());
                internalEntityTransactionMock.Verify(m => m.Commit(), Times.Once());
            }

            [Fact]
            public void AcceptAllChanges_called_if_SaveOptions_are_set_to_AcceptAllChangesAfterSave()
            {
                var internalObjectStateManagerMock = new Mock<InternalObjectStateManager>();
                internalObjectStateManagerMock.Setup(m => m.GetObjectStateEntriesCount(EntityState.Added | EntityState.Deleted | EntityState.Modified)).Returns(1);
                internalObjectStateManagerMock.Setup(m => m.GetObjectStateEntries(It.IsAny<EntityState>())).Returns(Enumerable.Empty<ObjectStateEntry>());

                var connectionState = ConnectionState.Closed;
                var internalEntityConnectionMock = new Mock<InternalEntityConnection>();
                internalEntityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                internalEntityConnectionMock.SetupGet(m => m.State).Returns(() => connectionState);
                internalEntityConnectionMock.Setup(m => m.Open()).Callback(() => connectionState = ConnectionState.Open);
                internalEntityConnectionMock.SetupGet(m => m.CurrentTransaction).Returns(new EntityTransaction(new Mock<InternalEntityTransaction>(null, null).Object));

                var internalMetadataWorkspace = new Mock<InternalMetadataWorkspace>();
                internalMetadataWorkspace.Setup(m => m.ShallowCopy()).Returns(() => internalMetadataWorkspace.Object);
                internalMetadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.OSpace)).Returns(true);
                internalMetadataWorkspace.Setup(m => m.GetItemCollection(DataSpace.OCSpace)).Returns(default(ItemCollection));
                internalMetadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var objectContext = BasicObjectContextInitializationWithConnectionAndMetadata(internalObjectStateManagerMock, internalEntityConnectionMock, internalMetadataWorkspace);
                objectContext.SaveChanges(SaveOptions.AcceptAllChangesAfterSave);

                internalObjectStateManagerMock.Verify(m => m.GetObjectStateEntries(It.IsAny<EntityState>()), Times.AtLeastOnce());
            }

            [Fact]
            public void Exception_thrown_during_AcceptAllChanges_is_wrapped()
            {
                var internalObjectStateManagerMock = new Mock<InternalObjectStateManager>();
                internalObjectStateManagerMock.Setup(m => m.GetObjectStateEntriesCount(EntityState.Added | EntityState.Deleted | EntityState.Modified)).Returns(1);
                internalObjectStateManagerMock.Setup(m => m.GetObjectStateEntries(It.IsAny<EntityState>())).Throws<NotSupportedException>();

                var connectionState = ConnectionState.Closed;
                var internalEntityConnectionMock = new Mock<InternalEntityConnection>();
                internalEntityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                internalEntityConnectionMock.SetupGet(m => m.State).Returns(() => connectionState);
                internalEntityConnectionMock.Setup(m => m.Open()).Callback(() => connectionState = ConnectionState.Open);
                internalEntityConnectionMock.SetupGet(m => m.CurrentTransaction).Returns(new EntityTransaction(new Mock<InternalEntityTransaction>(null, null).Object));

                var internalMetadataWorkspace = new Mock<InternalMetadataWorkspace>();
                internalMetadataWorkspace.Setup(m => m.ShallowCopy()).Returns(() => internalMetadataWorkspace.Object);
                internalMetadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.OSpace)).Returns(true);
                internalMetadataWorkspace.Setup(m => m.GetItemCollection(DataSpace.OCSpace)).Returns(default(ItemCollection));
                internalMetadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var objectContext = BasicObjectContextInitializationWithConnectionAndMetadata(internalObjectStateManagerMock, internalEntityConnectionMock, internalMetadataWorkspace);

                Assert.Equal(
                    Strings.ObjectContext_AcceptAllChangesFailure(new NotSupportedException().Message),
                    Assert.Throws<InvalidOperationException>(() => objectContext.SaveChanges(SaveOptions.AcceptAllChangesAfterSave)).Message);
            }
        }

        public class ExecuteStoreCommand
        {
            [Fact]
            public void Command_is_executed_with_correct_CommandText()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(() => dbCommandMock.Object);

                var internalEntityConnectionMock = new Mock<InternalEntityConnection>();
                internalEntityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                internalEntityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                internalEntityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnectionMock.Object);

                var objectContext = ObjectContextInitializationForExecuteStoreCommand(internalEntityConnectionMock);
                objectContext.ExecuteStoreCommand("Foo");

                dbCommandMock.VerifySet(m => m.CommandText = "Foo", Times.Once());
                dbCommandMock.Verify(m => m.ExecuteNonQuery(), Times.Once());
            }

            [Fact]
            public void CommandTimeout_is_set_on_created_DbCommand_if_it_was_set_on_ObjectContext()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(() => dbCommandMock.Object);

                var internalEntityConnectionMock = new Mock<InternalEntityConnection>();
                internalEntityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                internalEntityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                internalEntityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnectionMock.Object);

                var objectContext = ObjectContextInitializationForExecuteStoreCommand(internalEntityConnectionMock);
                objectContext.CommandTimeout = 10;
                objectContext.ExecuteStoreCommand("Foo");

                dbCommandMock.VerifySet(m => m.CommandTimeout = 10, Times.Once());
            }

            [Fact]
            public void Transaction_set_on_created_DbCommand_if_it_was_set_on_EntityConnection()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(() => dbCommandMock.Object);

                var storeTransaction = new Mock<DbTransaction>().Object;
                var internalEntityTransaction = new Mock<InternalEntityTransaction>(null, null);
                internalEntityTransaction.SetupGet(m => m.StoreTransaction).Returns(() => storeTransaction);
                var entityTransaction = new EntityTransaction(internalEntityTransaction.Object);

                var internalEntityConnectionMock = new Mock<InternalEntityConnection>();
                internalEntityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                internalEntityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                internalEntityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnectionMock.Object);
                internalEntityConnectionMock.SetupGet(m => m.CurrentTransaction).Returns(entityTransaction);

                var objectContext = ObjectContextInitializationForExecuteStoreCommand(internalEntityConnectionMock);
                objectContext.ExecuteStoreCommand("Foo");

                dbCommandMock.VerifySet(m => m.Transaction = storeTransaction, Times.Once());
            }

            [Fact]
            public void DbParameters_are_passed_correctly_to_DbCommand()
            {
                var parameter1 = new Mock<DbParameter>().Object;
                var parameter2 = new Mock<DbParameter>().Object;
                var parameter3 = new Mock<DbParameter>().Object;

                bool correctParameters = false;
                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock.Setup(m => m.AddRange(It.IsAny<DbParameter[]>())).
                    Callback((Array p) =>
                        {
                            var list = p.ToList<DbParameter>();
                            if (list.Count == 3 && list[0] == parameter1 && list[1] == parameter2 && list[2] == parameter3)
                            {
                                correctParameters = true;
                            }
                        });

                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(() => parameterCollectionMock.Object);
                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(() => dbCommandMock.Object);

                var internalEntityConnectionMock = new Mock<InternalEntityConnection>();
                internalEntityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                internalEntityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                internalEntityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnectionMock.Object);

                var objectContext = ObjectContextInitializationForExecuteStoreCommand(internalEntityConnectionMock);
                objectContext.ExecuteStoreCommand("Foo", parameter1, parameter2, parameter3);

                Assert.True(correctParameters);
            }

            [Fact]
            public void Parameter_values_are_converted_to_DbParameters_and_passed_correctly_to_DbCommand()
            {
                int createdParameterCount = 0;

                var parameterMock1 = new Mock<DbParameter>();
                var parameterMock2 = new Mock<DbParameter>();
                var parameterMock3 = new Mock<DbParameter>();
                var parameterMock4 = new Mock<DbParameter>();

                var parameterMockList = new List<Mock<DbParameter>>()
                {
                    parameterMock1, parameterMock2, parameterMock3, parameterMock4,
                };

                bool correctParameters = false;
                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock.Setup(m => m.AddRange(It.IsAny<DbParameter[]>())).
                    Callback((Array p) =>
                    {
                        var list = p.ToList<DbParameter>();
                        if (list.Count == 4 && list[0] == parameterMockList[0].Object && list[1] == parameterMockList[1].Object &&
                            list[2] == parameterMockList[2].Object && list[3] == parameterMockList[3].Object)
                        {
                            correctParameters = true;
                        }
                    });

                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.SetupGet(m => m.CommandText).Returns("{0} Foo {1} Bar {2} Baz {3}");
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(() => parameterCollectionMock.Object);
                dbCommandMock.Protected().Setup<DbParameter>("CreateDbParameter").
                    Returns(() => parameterMockList[createdParameterCount].Object).
                    Callback(() => createdParameterCount++);

                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(() => dbCommandMock.Object);

                var internalEntityConnectionMock = new Mock<InternalEntityConnection>();
                internalEntityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                internalEntityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                internalEntityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnectionMock.Object);

                var objectContext = ObjectContextInitializationForExecuteStoreCommand(internalEntityConnectionMock);
                objectContext.ExecuteStoreCommand("{0} Foo {1} Bar {2} Baz {3}", 1, null, "Bar", DBNull.Value);

                parameterMock1.VerifySet(m => m.ParameterName = "p0", Times.Once());
                parameterMock1.VerifySet(m => m.Value = 1, Times.Once());

                parameterMock2.VerifySet(m => m.ParameterName = "p1", Times.Once());
                parameterMock2.VerifySet(m => m.Value = DBNull.Value, Times.Once());

                parameterMock3.VerifySet(m => m.ParameterName = "p2", Times.Once());
                parameterMock3.VerifySet(m => m.Value = "Bar", Times.Once());

                parameterMock4.VerifySet(m => m.ParameterName = "p3", Times.Once());
                parameterMock4.VerifySet(m => m.Value = DBNull.Value, Times.Once());

                dbCommandMock.VerifySet(m => m.CommandText = "@p0 Foo @p1 Bar @p2 Baz @p3");

                Assert.True(correctParameters);
            }

            [Fact]
            public void Exception_thrown_when_parameters_are_mix_of_values_and_DbParameters()
            {
                var dbCommandMock = new Mock<DbCommand>();

                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(() => dbCommandMock.Object);

                var internalEntityConnectionMock = new Mock<InternalEntityConnection>();
                internalEntityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                internalEntityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                internalEntityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnectionMock.Object);

                var objectContext = ObjectContextInitializationForExecuteStoreCommand(internalEntityConnectionMock);

                Assert.Equal(
                    Strings.ObjectContext_ExecuteCommandWithMixOfDbParameterAndValues,
                    Assert.Throws<InvalidOperationException>(() => objectContext.ExecuteStoreCommand("Foo", 1, new Mock<DbParameter>().Object)).Message);
            }

            private static ObjectContext ObjectContextInitializationForExecuteStoreCommand(Mock<InternalEntityConnection> internalEntityConnectionMock)
            {
                var internalObjectStateManagerMock = new Mock<InternalObjectStateManager>();
                var internalMetadataWorkspace = new Mock<InternalMetadataWorkspace>();
                internalMetadataWorkspace.Setup(m => m.ShallowCopy()).Returns(() => internalMetadataWorkspace.Object);
                internalMetadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.OSpace)).Returns(true);
                internalMetadataWorkspace.Setup(m => m.GetItemCollection(DataSpace.OCSpace)).Returns(default(ItemCollection));
                internalMetadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var objectContext = BasicObjectContextInitializationWithConnectionAndMetadata(internalObjectStateManagerMock, internalEntityConnectionMock, internalMetadataWorkspace);

                return objectContext;
            }
        }

        private static ObjectContext BasicObjectContextInitializationWithConnection(
            Mock<InternalObjectStateManager> internalObjectStateManagerMock,
            Mock<InternalEntityConnection> internalEntityConnectionMock)
        {
            var entityConnection = new EntityConnection(internalEntityConnectionMock.Object);
            var objectContextMock = new Mock<ObjectContextForMock>(entityConnection)
            {
                CallBase = true
            };

            objectContextMock.SetupGet(m => m.ObjectStateManager).Returns(() => new ObjectStateManager(internalObjectStateManagerMock.Object));

            return objectContextMock.Object;
        }

        private static ObjectContext BasicObjectContextInitializationWithConnectionAndMetadata(
            Mock<InternalObjectStateManager> internalObjectStateManagerMock,
            Mock<InternalEntityConnection> internalEntityConnectionMock,
            Mock<InternalMetadataWorkspace> internalMetadataWorkspace)
        {
            var entityConnection = new EntityConnection(internalEntityConnectionMock.Object);

            var objectContextMock = new Mock<ObjectContextForMock>(entityConnection)
            {
                CallBase = true
            };

            objectContextMock.SetupGet(m => m.ObjectStateManager).Returns(() => new ObjectStateManager(internalObjectStateManagerMock.Object));
            objectContextMock.SetupGet(m => m.MetadataWorkspace).Returns(() => new MetadataWorkspace(internalMetadataWorkspace.Object));

            return objectContextMock.Object;
        }
    }
}
