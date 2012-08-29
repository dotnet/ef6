// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class ObjectContextTests
    {
        public class IObjectContextAdapter_ObjectContext
        {
            [Fact]
            public void ObjectContext_property_returns_self()
            {
                var context = new ObjectContextForMock(null);

                Assert.Same(context, ((IObjectContextAdapter)context).ObjectContext);
            }
        }

        public class SaveChanges
        {
            [Fact]
            public void Parameterless_SaveChanges_calls_SaveOption_flags_to_DetectChangesBeforeSave_and_AcceptAllChangesAfterSave()
            {
                var objectContextMock = new Mock<ObjectContextForMock>(null /*entityConnection*/);

                objectContextMock.Object.SaveChanges();

                objectContextMock.Verify(
                    m => m.SaveChanges(SaveOptions.DetectChangesBeforeSave | SaveOptions.AcceptAllChangesAfterSave),
                    Times.Once());
            }

            [Fact]
            public void Calls_ObjectStateManager_DetectChanges_if_SaveOptions_is_set_to_DetectChangesBeforeSave()
            {
                var objectStateManagerMock = new Mock<ObjectStateManager>();
                objectStateManagerMock.Setup(m => m.SomeEntryWithConceptualNullExists()).Returns(false);
                objectStateManagerMock.Setup(
                    m => m.GetObjectStateEntriesCount(EntityState.Added | EntityState.Deleted | EntityState.Modified)).Returns(0);

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");

                var objectContext = BasicObjectContextInitializationWithConnection(objectStateManagerMock, entityConnectionMock);

                objectContext.SaveChanges(SaveOptions.DetectChangesBeforeSave);

                objectStateManagerMock.Verify(m => m.DetectChanges(), Times.Once());
            }

            [Fact]
            public void Does_not_ensure_connection_when_intercepting()
            {
                var mockObjectStateManager = new Mock<ObjectStateManager>();
                mockObjectStateManager.Setup(osm => osm.GetObjectStateEntriesCount(It.IsAny<EntityState>())).Returns(1);

                var mockCommandInterceptor = new Mock<IDbCommandInterceptor>();
                mockCommandInterceptor.SetupGet(ci => ci.IsEnabled).Returns(true);

                var mockEntityAdapter = new Mock<IEntityAdapter>();

                var mockObjectContext
                    = new Mock<ObjectContext>(
                        mockObjectStateManager.Object, mockCommandInterceptor.Object, mockEntityAdapter.Object)
                          {
                              CallBase = true
                          };
                mockObjectContext.SetupGet(oc => oc.Connection).Returns(new Mock<EntityConnection>().Object);

                mockObjectContext.Object.SaveChanges(SaveOptions.None);

                mockObjectContext.Verify(oc => oc.EnsureConnection(), Times.Never());
                mockEntityAdapter.Verify(ea => ea.Update(mockObjectStateManager.Object, false));
            }

            [Fact]
            public void Exception_thrown_if_ObjectStateManager_has_entries_with_conceptual_nulls()
            {
                var objectStateManagerMock = new Mock<ObjectStateManager>();
                objectStateManagerMock.Setup(m => m.SomeEntryWithConceptualNullExists()).Returns(true);

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");

                var objectContext = BasicObjectContextInitializationWithConnection(objectStateManagerMock, entityConnectionMock);

                Assert.Equal(
                    Strings.ObjectContext_CommitWithConceptualNull,
                    Assert.Throws<InvalidOperationException>(() => objectContext.SaveChanges(SaveOptions.None)).Message);
            }

            [Fact]
            public void Shortcircuits_if_no_state_changes()
            {
                var mockObjectContext = MockHelper.CreateMockObjectContext<DbDataRecord>();
                var mockServiceProvider = (IServiceProvider)((EntityConnection)mockObjectContext.Connection).StoreProviderFactory;
                var entityAdapterMock = Mock.Get((IEntityAdapter)mockServiceProvider.GetService(typeof(IEntityAdapter)));
                entityAdapterMock.Setup(m => m.Update(It.IsAny<IEntityStateManager>(), true)).Verifiable();

                var entriesAffected = mockObjectContext.SaveChanges(SaveOptions.None);

                entityAdapterMock.Verify(m => m.Update(It.IsAny<IEntityStateManager>(), true), Times.Never());
                Assert.Equal(0, entriesAffected);
            }

            [Fact]
            public void If_local_transaction_is_necessary_it_gets_created_commited()
            {
                var objectStateManagerMock = new Mock<ObjectStateManager>();
                objectStateManagerMock.Setup(
                    m => m.GetObjectStateEntriesCount(EntityState.Added | EntityState.Deleted | EntityState.Modified)).Returns(1);

                var dbTransaction = new Mock<DbTransaction>();
                var entityTransactionMock = new Mock<EntityTransaction>(new EntityConnection(), dbTransaction.Object);
                var entityTransaction = entityTransactionMock.Object;

                var connectionState = ConnectionState.Closed;
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => connectionState);
                entityConnectionMock.Setup(m => m.Open()).Callback(() => connectionState = ConnectionState.Open);
                entityConnectionMock.Setup(m => m.BeginTransaction()).Returns(() => entityTransaction);

                // first time return false to by-pass check in the constructor
                var enlistedInUserTransactionCallCount = 0;
                entityConnectionMock.SetupGet(m => m.EnlistedInUserTransaction).
                    Callback(() => enlistedInUserTransactionCallCount++).
                    Returns(enlistedInUserTransactionCallCount == 1);

                var metadataWorkspace = new Mock<MetadataWorkspace>();
                metadataWorkspace.Setup(m => m.ShallowCopy()).Returns(() => metadataWorkspace.Object);
                metadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.OSpace)).Returns(true);
                metadataWorkspace.Setup(m => m.GetItemCollection(DataSpace.OCSpace)).Returns(default(ItemCollection));
                metadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var objectContext = BasicObjectContextInitializationWithConnectionAndMetadata(
                    objectStateManagerMock, entityConnectionMock, metadataWorkspace);
                objectContext.SaveChanges(SaveOptions.None);

                entityConnectionMock.Verify(m => m.BeginTransaction(), Times.Once());
                entityTransactionMock.Verify(m => m.Commit(), Times.Once());
            }

            [Fact]
            public void AcceptAllChanges_called_if_SaveOptions_are_set_to_AcceptAllChangesAfterSave()
            {
                var objectStateManagerMock = new Mock<ObjectStateManager>();
                objectStateManagerMock.Setup(
                    m => m.GetObjectStateEntriesCount(EntityState.Added | EntityState.Deleted | EntityState.Modified)).Returns(1);
                objectStateManagerMock.Setup(m => m.GetObjectStateEntriesInternal(It.IsAny<EntityState>())).Returns(
                    Enumerable.Empty<ObjectStateEntry>());

                var connectionState = ConnectionState.Closed;
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => connectionState);
                entityConnectionMock.Setup(m => m.Open()).Callback(() => connectionState = ConnectionState.Open);
                entityConnectionMock.SetupGet(m => m.CurrentTransaction).Returns(new Mock<EntityTransaction>().Object);

                var metadataWorkspace = new Mock<MetadataWorkspace>();
                metadataWorkspace.Setup(m => m.ShallowCopy()).Returns(() => metadataWorkspace.Object);
                metadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.OSpace)).Returns(true);
                metadataWorkspace.Setup(m => m.GetItemCollection(DataSpace.OCSpace)).Returns(default(ItemCollection));
                metadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var objectContext = BasicObjectContextInitializationWithConnectionAndMetadata(
                    objectStateManagerMock, entityConnectionMock, metadataWorkspace);
                objectContext.SaveChanges(SaveOptions.AcceptAllChangesAfterSave);

                objectStateManagerMock.Verify(m => m.GetObjectStateEntriesInternal(It.IsAny<EntityState>()), Times.AtLeastOnce());
            }

            [Fact]
            public void Exception_thrown_during_AcceptAllChanges_is_wrapped()
            {
                var objectStateManagerMock = new Mock<ObjectStateManager>();
                objectStateManagerMock.Setup(
                    m => m.GetObjectStateEntriesCount(EntityState.Added | EntityState.Deleted | EntityState.Modified)).Returns(1);
                objectStateManagerMock.Setup(m => m.GetObjectStateEntries(It.IsAny<EntityState>())).Throws<NotSupportedException>();
                objectStateManagerMock.Setup(m => m.GetObjectStateEntriesInternal(It.IsAny<EntityState>())).Returns(
                    Enumerable.Empty<ObjectStateEntry>());

                var connectionState = ConnectionState.Closed;
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => connectionState);
                entityConnectionMock.Setup(m => m.Open()).Callback(() => connectionState = ConnectionState.Open);
                entityConnectionMock.SetupGet(m => m.CurrentTransaction).Returns(new Mock<EntityTransaction>().Object);

                var metadataWorkspace = new Mock<MetadataWorkspace>();
                metadataWorkspace.Setup(m => m.ShallowCopy()).Returns(() => metadataWorkspace.Object);
                metadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.OSpace)).Returns(true);
                metadataWorkspace.Setup(m => m.GetItemCollection(DataSpace.OCSpace)).Returns(default(ItemCollection));
                metadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var objectContext = BasicObjectContextInitializationWithConnectionAndMetadata(
                    objectStateManagerMock, entityConnectionMock, metadataWorkspace);

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

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnectionMock.Object);

                var objectContext = ObjectContextInitializationForExecuteStoreCommand(entityConnectionMock);
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

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnectionMock.Object);

                var objectContext = ObjectContextInitializationForExecuteStoreCommand(entityConnectionMock);
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
                var entityTransactionMock = new Mock<EntityTransaction>();
                entityTransactionMock.SetupGet(m => m.StoreTransaction).Returns(() => storeTransaction);
                var entityTransaction = entityTransactionMock.Object;

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnectionMock.Object);
                entityConnectionMock.SetupGet(m => m.CurrentTransaction).Returns(entityTransaction);

                var objectContext = ObjectContextInitializationForExecuteStoreCommand(entityConnectionMock);
                objectContext.ExecuteStoreCommand("Foo");

                dbCommandMock.VerifySet(m => m.Transaction = storeTransaction, Times.Once());
            }

            [Fact]
            public void DbParameters_are_passed_correctly_to_DbCommand()
            {
                var parameter1 = new Mock<DbParameter>().Object;
                var parameter2 = new Mock<DbParameter>().Object;
                var parameter3 = new Mock<DbParameter>().Object;

                var correctParameters = false;
                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock.Setup(m => m.AddRange(It.IsAny<DbParameter[]>())).
                    Callback(
                        (Array p) =>
                            {
                                var list = p.ToList<DbParameter>();
                                if (list.Count == 3 && list[0] == parameter1 && list[1] == parameter2
                                    && list[2] == parameter3)
                                {
                                    correctParameters = true;
                                }
                            });

                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);
                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(() => dbCommandMock.Object);

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnectionMock.Object);

                var objectContext = ObjectContextInitializationForExecuteStoreCommand(entityConnectionMock);
                objectContext.ExecuteStoreCommand("Foo", parameter1, parameter2, parameter3);

                Assert.True(correctParameters);
            }

            [Fact]
            public void Parameter_values_are_converted_to_DbParameters_and_passed_correctly_to_DbCommand()
            {
                var createdParameterCount = 0;

                var parameterMock1 = new Mock<DbParameter>();
                var parameterMock2 = new Mock<DbParameter>();
                var parameterMock3 = new Mock<DbParameter>();
                var parameterMock4 = new Mock<DbParameter>();

                var parameterMockList = new List<Mock<DbParameter>>
                                            {
                                                parameterMock1,
                                                parameterMock2,
                                                parameterMock3,
                                                parameterMock4,
                                            };

                var correctParameters = false;
                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock.Setup(m => m.AddRange(It.IsAny<DbParameter[]>())).
                    Callback(
                        (Array p) =>
                            {
                                var list = p.ToList<DbParameter>();
                                if (list.Count == 4 && list[0] == parameterMockList[0].Object && list[1] == parameterMockList[1].Object &&
                                    list[2] == parameterMockList[2].Object
                                    && list[3] == parameterMockList[3].Object)
                                {
                                    correctParameters = true;
                                }
                            });

                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.SetupGet(m => m.CommandText).Returns("{0} Foo {1} Bar {2} Baz {3}");
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);
                dbCommandMock.Protected().Setup<DbParameter>("CreateDbParameter").
                    Returns(() => parameterMockList[createdParameterCount].Object).
                    Callback(() => createdParameterCount++);

                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(() => dbCommandMock.Object);

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnectionMock.Object);

                var objectContext = ObjectContextInitializationForExecuteStoreCommand(entityConnectionMock);
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

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnectionMock.Object);

                var objectContext = ObjectContextInitializationForExecuteStoreCommand(entityConnectionMock);

                Assert.Equal(
                    Strings.ObjectContext_ExecuteCommandWithMixOfDbParameterAndValues,
                    Assert.Throws<InvalidOperationException>(
                        () => objectContext.ExecuteStoreCommand("Foo", 1, new Mock<DbParameter>().Object)).Message);
            }

            private static ObjectContext ObjectContextInitializationForExecuteStoreCommand(Mock<EntityConnection> entityConnectionMock)
            {
                var objectStateManagerMock = new Mock<ObjectStateManager>();
                var metadataWorkspace = new Mock<MetadataWorkspace>();
                metadataWorkspace.Setup(m => m.ShallowCopy()).Returns(() => metadataWorkspace.Object);
                metadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.OSpace)).Returns(true);
                metadataWorkspace.Setup(m => m.GetItemCollection(DataSpace.OCSpace)).Returns(default(ItemCollection));
                metadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var objectContext = BasicObjectContextInitializationWithConnectionAndMetadata(
                    objectStateManagerMock, entityConnectionMock, metadataWorkspace);

                return objectContext;
            }
        }

        private static ObjectContext BasicObjectContextInitializationWithConnection(
            Mock<ObjectStateManager> objectStateManagerMock,
            Mock<EntityConnection> entityConnectionMock)
        {
            var entityConnection = entityConnectionMock.Object;
            var objectContextMock = new Mock<ObjectContextForMock>(entityConnection)
                                        {
                                            CallBase = true
                                        };

            objectContextMock.SetupGet(m => m.ObjectStateManager).Returns(() => objectStateManagerMock.Object);

            return objectContextMock.Object;
        }

        private static ObjectContext BasicObjectContextInitializationWithConnectionAndMetadata(
            Mock<ObjectStateManager> objectStateManagerMock,
            Mock<EntityConnection> entityConnectionMock,
            Mock<MetadataWorkspace> metadataWorkspace)
        {
            var entityConnection = entityConnectionMock.Object;

            var objectContextMock = new Mock<ObjectContextForMock>(entityConnection)
                                        {
                                            CallBase = true
                                        };

            objectContextMock.SetupGet(m => m.ObjectStateManager).Returns(() => objectStateManagerMock.Object);
            objectContextMock.SetupGet(m => m.MetadataWorkspace).Returns(() => metadataWorkspace.Object);

            return objectContextMock.Object;
        }
    }
}
