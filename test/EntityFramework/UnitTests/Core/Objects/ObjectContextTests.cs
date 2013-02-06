// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Core.Common.QueryCache;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
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

        public class SaveChanges : TestBase
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

                var objectContext = CreateObjectContext(entityConnectionMock, objectStateManagerMock);

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
                    = new Mock<ObjectContext>(null, null, null, mockCommandInterceptor.Object, mockEntityAdapter.Object)
                          {
                              CallBase = true
                          };
                mockObjectContext.Setup(oc => oc.ObjectStateManager).Returns(mockObjectStateManager.Object);

                var storeConnectionMock = new Mock<DbConnection>();
                storeConnectionMock.Setup(m => m.DataSource).Returns("foo");
                mockObjectContext.Setup(oc => oc.Connection).Returns(storeConnectionMock.Object);

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

                var objectContext = CreateObjectContext(entityConnectionMock, objectStateManagerMock);

                Assert.Equal(
                    Strings.ObjectContext_CommitWithConceptualNull,
                    Assert.Throws<InvalidOperationException>(() => objectContext.SaveChanges(SaveOptions.None)).Message);
            }

            [Fact]
            public void Shortcircuits_if_no_state_changes()
            {
                var mockObjectContext = Mock.Get(MockHelper.CreateMockObjectContext<DbDataRecord>());
                mockObjectContext.CallBase = true;

                var mockServiceProvider = (IServiceProvider)((EntityConnection)mockObjectContext.Object.Connection).StoreProviderFactory;
                var entityAdapterMock = Mock.Get((IEntityAdapter)mockServiceProvider.GetService(typeof(IEntityAdapter)));
                entityAdapterMock.Setup(m => m.Update(It.IsAny<IEntityStateManager>(), true)).Verifiable();

                var entriesAffected = mockObjectContext.Object.SaveChanges(SaveOptions.None);

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
                var storeConnectionMock = new Mock<DbConnection>();
                storeConnectionMock.Setup(m => m.DataSource).Returns("foo");
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => connectionState);
                entityConnectionMock.Setup(m => m.Open()).Callback(() => connectionState = ConnectionState.Open);
                entityConnectionMock.Setup(m => m.BeginTransaction()).Returns(() => entityTransaction);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(storeConnectionMock.Object);

                // first time return false to by-pass check in the constructor
                var enlistedInUserTransactionCallCount = 0;
                entityConnectionMock.SetupGet(m => m.EnlistedInUserTransaction).
                                     Callback(() => enlistedInUserTransactionCallCount++).
                                     Returns(enlistedInUserTransactionCallCount == 1);

                var objectContext = CreateObjectContext(entityConnectionMock, objectStateManagerMock);
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
                var storeConnectionMock = new Mock<DbConnection>();
                storeConnectionMock.Setup(m => m.DataSource).Returns("foo");
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => connectionState);
                entityConnectionMock.Setup(m => m.Open()).Callback(() => connectionState = ConnectionState.Open);
                entityConnectionMock.SetupGet(m => m.CurrentTransaction).Returns(new Mock<EntityTransaction>().Object);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(storeConnectionMock.Object);

                var objectContext = CreateObjectContext(entityConnectionMock, objectStateManagerMock);
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
                var storeConnectionMock = new Mock<DbConnection>();
                storeConnectionMock.Setup(m => m.DataSource).Returns("foo");
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => connectionState);
                entityConnectionMock.Setup(m => m.Open()).Callback(() => connectionState = ConnectionState.Open);
                entityConnectionMock.SetupGet(m => m.CurrentTransaction).Returns(new Mock<EntityTransaction>().Object);
                entityConnectionMock.Setup(m => m.StoreConnection).Returns(storeConnectionMock.Object);

                var objectContext = CreateObjectContext(entityConnectionMock, objectStateManagerMock);

                Assert.Equal(
                    Strings.ObjectContext_AcceptAllChangesFailure(new NotSupportedException().Message),
                    Assert.Throws<InvalidOperationException>(() => objectContext.SaveChanges(SaveOptions.AcceptAllChangesAfterSave)).Message);
            }

            [Fact]
            public void OnSavingChanges_event_gets_called()
            {
                var mockObjectContext = Mock.Get(MockHelper.CreateMockObjectContext<DbDataRecord>());
                mockObjectContext.CallBase = true;

                var callCount = 0;
                EventHandler saveChangesDelegate = delegate { callCount++; };
                mockObjectContext.Object.SavingChanges += saveChangesDelegate;

                var entriesAffected = mockObjectContext.Object.SaveChanges(SaveOptions.None);

                Assert.Equal(1, callCount);
                Assert.Equal(0, entriesAffected);

                //Ensure that event does not get called when removed
                callCount = 0;
                mockObjectContext.Object.SavingChanges -= saveChangesDelegate;

                entriesAffected = mockObjectContext.Object.SaveChanges(SaveOptions.None);

                Assert.Equal(0, callCount);
                Assert.Equal(0, entriesAffected);
            }

            [Fact]
            public void Raises_expected_exception_from_OnSavingChanges_event()
            {
                var mockObjectContext = Mock.Get(MockHelper.CreateMockObjectContext<DbDataRecord>());
                mockObjectContext.CallBase = true;

                EventHandler saveChangesDelegate = delegate { throw new InvalidOperationException(); };
                mockObjectContext.Object.SavingChanges += saveChangesDelegate;

                Assert.Throws<InvalidOperationException>(
                    () =>
                    mockObjectContext.Object.SaveChanges(SaveOptions.None));
            }

            [Fact]
            public void Uses_ExecutionStrategy()
            {
                var objectStateManagerMock = new Mock<ObjectStateManager>();
                objectStateManagerMock.Setup(
                    m => m.GetObjectStateEntriesCount(EntityState.Added | EntityState.Deleted | EntityState.Modified)).Returns(1);

                var storeConnectionMock = new Mock<DbConnection>();
                storeConnectionMock.Setup(m => m.DataSource).Returns("foo");
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(storeConnectionMock.Object);

                var objectContext = CreateObjectContext(entityConnectionMock, objectStateManagerMock);

                var executionStrategyMock = new Mock<IExecutionStrategy>();
                executionStrategyMock.Setup(m => m.Execute(It.IsAny<Func<int>>())).Returns<Func<int>>(f => 2);

                MutableResolver.AddResolver<IExecutionStrategy>(key => executionStrategyMock.Object);
                try
                {
                    Assert.Equal(2, objectContext.SaveChanges());
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }

                executionStrategyMock.Verify(m => m.Execute(It.IsAny<Func<int>>()), Times.Once());
            }
        }

        public class EnsureConnection
        {
            [Fact]
            public void Calls_EnsureMetadata_if_connection_open()
            {
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                var objectContextMock = new Mock<ObjectContextForMock>(entityConnectionMock.Object)
                                            {
                                                CallBase = true
                                            };
                objectContextMock.Setup(m => m.EnsureMetadata()).Verifiable();

                objectContextMock.Object.EnsureConnection();

                objectContextMock.Verify(m => m.EnsureMetadata(), Times.Once());
            }

            [Fact]
            public void Releases_connection_when_exception_caught()
            {
                var connectionMock = new Mock<EntityConnection>();
                connectionMock.Setup(m => m.State).Returns(ConnectionState.Open);
                var objectContextMock = new Mock<ObjectContextForMock>(connectionMock.Object)
                                            {
                                                CallBase = true
                                            };
                objectContextMock.Setup(m => m.EnsureMetadata()).Throws(new MetadataException());

                try
                {
                    objectContextMock.Object.EnsureConnection();
                }
                catch (MetadataException)
                {
                    objectContextMock.Verify(m => m.ReleaseConnection(), Times.Once());
                }
            }

            [Fact]
            public void Calls_Open_if_connection_is_closed_or_broken()
            {
                var state = ConnectionState.Broken;
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.Setup(m => m.Close()).Callback(() => { state = ConnectionState.Closed; });
                entityConnectionMock.Setup(m => m.Open()).Callback(
                    () =>
                        {
                            entityConnectionMock.Verify(m => m.Close(), Times.Once());
                            state = ConnectionState.Open;
                        });
                entityConnectionMock.SetupGet(m => m.State).Returns(() => state);

                var objectContextMock = new Mock<ObjectContextForMock>(entityConnectionMock.Object)
                                            {
                                                CallBase = true
                                            };
                objectContextMock.Setup(m => m.EnsureMetadata());

                objectContextMock.Object.EnsureConnection();

                entityConnectionMock.Verify(m => m.Open(), Times.Once());
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

                var objectContext = CreateObjectContext(entityConnectionMock);
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

                var objectContext = CreateObjectContext(entityConnectionMock);
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

                var objectContext = CreateObjectContext(entityConnectionMock);
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
                                                    if (list.Count == 3
                                                        && list[0] == parameter1
                                                        && list[1] == parameter2
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

                var objectContext = CreateObjectContext(entityConnectionMock);
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
                                                    if (list.Count == 4
                                                        && list[0] == parameterMockList[0].Object
                                                        && list[1] == parameterMockList[1].Object
                                                        &&
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

                var objectContext = CreateObjectContext(entityConnectionMock);
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

                var objectContext = CreateObjectContext(entityConnectionMock);

                Assert.Equal(
                    Strings.ObjectContext_ExecuteCommandWithMixOfDbParameterAndValues,
                    Assert.Throws<InvalidOperationException>(
                        () => objectContext.ExecuteStoreCommand("Foo", 1, new Mock<DbParameter>().Object)).Message);
            }
        }

        public class ExecuteStoreQuery
        {
            [Fact]
            public void Command_is_executed_with_correct_CommandText_and_parameters()
            {
                var dbCommandMock = new Mock<DbCommand>();

                var parameterMock = new Mock<DbParameter>();
                var correctParameters = false;
                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock.Setup(m => m.AddRange(It.IsAny<DbParameter[]>())).
                                        Callback(
                                            (Array p) =>
                                                {
                                                    var list = p.ToList<DbParameter>();
                                                    if (list.Count == 1
                                                        && list[0] == parameterMock.Object)
                                                    {
                                                        correctParameters = true;
                                                    }
                                                });

                dbCommandMock.Protected().Setup<DbDataReader>("ExecuteDbDataReader", It.IsAny<CommandBehavior>()).Returns(
                    Common.Internal.Materialization.MockHelper.CreateDbDataReader(new[] { new[] { new object() } }));
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                objectContext.ExecuteStoreQuery<object>("{0} Foo", parameterMock.Object);

                dbCommandMock.VerifySet(m => m.CommandText = "{0} Foo", Times.Once());
                dbCommandMock.Protected().Verify("ExecuteDbDataReader", Times.Once(), It.IsAny<CommandBehavior>());
                Assert.True(correctParameters);
            }

            [Fact]
            public void Command_is_executed_with_correct_CommandText_and_parameters_with_streaming()
            {
                var dbCommandMock = new Mock<DbCommand>();

                var parameterMock = new Mock<DbParameter>();
                var correctParameters = false;
                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock.Setup(m => m.AddRange(It.IsAny<DbParameter[]>())).
                                        Callback(
                                            (Array p) =>
                                                {
                                                    var list = p.ToList<DbParameter>();
                                                    if (list.Count == 1
                                                        && list[0] == parameterMock.Object)
                                                    {
                                                        correctParameters = true;
                                                    }
                                                });

                var dataReader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(new[] { new[] { new object() } });
                // This reader will throw if buffering is on
                Mock.Get(dataReader).Setup(m => m.Read()).Throws(new NotImplementedException());
                dbCommandMock.Protected().Setup<DbDataReader>("ExecuteDbDataReader", It.IsAny<CommandBehavior>()).Returns(dataReader);
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                objectContext.ExecuteStoreQuery<object>("{0} Foo", new ExecutionOptions(MergeOption.AppendOnly, true), parameterMock.Object);

                dbCommandMock.VerifySet(m => m.CommandText = "{0} Foo", Times.Once());
                dbCommandMock.Protected().Verify("ExecuteDbDataReader", Times.Once(), It.IsAny<CommandBehavior>());
                Assert.True(correctParameters);
            }

            [Fact]
            public void DbDataReader_is_buffered_by_default()
            {
                var dbCommandMock = new Mock<DbCommand>();

                dbCommandMock.Protected().Setup<DbDataReader>("ExecuteDbDataReader", It.IsAny<CommandBehavior>()).Returns(
                    new Mock<DbDataReader>().Object);
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => new Mock<DbParameterCollection>().Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                objectContext.ExecuteStoreQuery<object>("{0} Foo");
            }

            [Fact]
            public void Connection_is_released_after_reader_exception()
            {
                var dbCommandMock = new Mock<DbCommand>();

                dbCommandMock.Protected().Setup<DbDataReader>("ExecuteDbDataReader", It.IsAny<CommandBehavior>())
                             .Throws(new InvalidOperationException("Foo"));
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => new Mock<DbParameterCollection>().Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                Assert.Equal(
                    "Foo",
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        objectContext.ExecuteStoreQuery<object>("Bar")).Message);

                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Once());
            }

            [Fact]
            public void Connection_is_released_after_translator_exception()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var dataReader = Common.Internal.Materialization.MockHelper.CreateDbDataReader();

                dbCommandMock.Protected().Setup<DbDataReader>("ExecuteDbDataReader", It.IsAny<CommandBehavior>())
                             .Returns(dataReader);
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => new Mock<DbParameterCollection>().Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                Mock.Get(objectContext.MetadataWorkspace).Setup(m => m.GetQueryCacheManager())
                    .Throws(new InvalidOperationException("Foo"));

                Assert.Equal(
                    "Foo",
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        objectContext.ExecuteStoreQuery<object>("Foo")).Message);

                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Once());
                Mock.Get(dataReader).Protected().Verify("Dispose", Times.Once(), true);
            }
        }

        public class ExecuteFunction
        {
            [Fact]
            public void Command_is_executed_with_streaming()
            {
                var dataReader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(new[] { new[] { new object() } });
                // This reader will throw if buffering is on
                Mock.Get(dataReader).Setup(m => m.Read()).Throws(new NotImplementedException());

                var entityCommandDefinition = EntityClient.MockHelper.CreateEntityCommandDefinition(
                    dataReader, new List<EntityParameter>());
                FakeSqlProviderServices.Instance.EntityCommandDefinition = entityCommandDefinition;

                var entityCommandMock = Mock.Get((EntityCommand)entityCommandDefinition.CreateCommand());

                var objectContext = CreateObjectContext(entityCommandMock.Object);

                var entityConnectionMock = Mock.Get((EntityConnection)objectContext.Connection);
                entityConnectionMock.Setup(m => m.StoreProviderFactory).Returns(new FakeSqlProviderFactory());

                var entityTypeMock = new Mock<EntityType>
                                         {
                                             CallBase = true
                                         };
                var collectionTypeMock = new Mock<CollectionType>(entityTypeMock.Object)
                                             {
                                                 CallBase = true
                                             };
                var entityType = (EdmType)entityTypeMock.Object;
                var metadataWorkspaceMock = Mock.Get(objectContext.MetadataWorkspace);
                metadataWorkspaceMock.Setup(m => m.TryDetermineCSpaceModelType(It.IsAny<Type>(), out entityType))
                                     .Returns(true);

                var entityContainer = new EntityContainer("Bar", DataSpace.CSpace);
                metadataWorkspaceMock.Setup(m => m.TryGetEntityContainer(It.IsAny<string>(), It.IsAny<DataSpace>(), out entityContainer))
                                     .Returns(true);
                var functionImport = new EdmFunction(
                    "Foo", "Bar", DataSpace.CSpace,
                    new EdmFunctionPayload
                        {
                            IsComposable = false,
                            IsFunctionImport = true,
                            ReturnParameters = new[]
                                                   {
                                                       new FunctionParameter(
                                                           EdmConstants.ReturnType,
                                                           TypeUsage.Create(collectionTypeMock.Object),
                                                           ParameterMode.ReturnValue),
                                                   }
                        });
                entityContainer.AddFunctionImport(functionImport);

                var edmItemCollection = (EdmItemCollection)objectContext.MetadataWorkspace.GetItemCollection(DataSpace.CSpace);
                var storeItemCollection = (StoreItemCollection)objectContext.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
                var containerMappingMock = new Mock<StorageEntityContainerMapping>(entityContainer);
                FunctionImportMapping targetFunctionMapping = new FunctionImportMappingNonComposable(
                    functionImport, functionImport, new List<List<FunctionImportStructuralTypeMapping>>(), edmItemCollection);
                containerMappingMock.Setup(
                    m => m.TryGetFunctionImportMapping(
                        It.IsAny<EdmFunction>(), out targetFunctionMapping)).Returns(true);

                var storageMappingItemCollection = new Mock<StorageMappingItemCollection>(
                    edmItemCollection, storeItemCollection, new string[0])
                                                       {
                                                           CallBase = true
                                                       };
                storageMappingItemCollection.Setup(m => m.GetItems<StorageEntityContainerMapping>())
                                            .Returns(
                                                new ReadOnlyCollection<StorageEntityContainerMapping>(
                                                    new List<StorageEntityContainerMapping>
                                                        {
                                                            containerMappingMock.Object
                                                        }));

                metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.CSSpace, It.IsAny<bool>()))
                                     .Returns(storageMappingItemCollection.Object);

                objectContext.ExecuteFunction<object>("Foo", new ExecutionOptions(MergeOption.AppendOnly, streaming: true));

                entityCommandMock.Protected().Verify("ExecuteDbDataReader", Times.Once(), It.IsAny<CommandBehavior>());

                FakeSqlProviderServices.Instance.EntityCommandDefinition = null;
            }
        }

        public class Dispose
        {
            [Fact]
            public void ObjectContext_disposes_underlying_EntityConnection_if_contextOwnConnection_flag_is_set()
            {
                var entityConnectionMock = new Mock<EntityConnection>(MockBehavior.Strict, null, null, true, true);
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Fake connection string");
                entityConnectionMock.Setup(m => m.GetMetadataWorkspace(false)).Returns(new Mock<MetadataWorkspace>().Object);
                entityConnectionMock.Protected().Setup("Dispose", true).Verifiable();

                var objectContext = new ObjectContext(entityConnectionMock.Object, true);
                objectContext.Dispose();

                entityConnectionMock.Protected().Verify("Dispose", Times.Once(), true);
            }

            [Fact]
            public void ObjectContext_does_not_dispose_underlying_EntityConnection_if_contextOwnConnection_flag_is_not_set()
            {
                var entityConnectionMock = new Mock<EntityConnection>(MockBehavior.Strict, null, null, true, true);
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Fake connection string");
                entityConnectionMock.Setup(m => m.GetMetadataWorkspace(false)).Returns(new Mock<MetadataWorkspace>().Object);
                entityConnectionMock.Protected().Setup("Dispose", false).Verifiable();

                var objectContext = new ObjectContext(entityConnectionMock.Object, false);
                objectContext.Dispose();

                entityConnectionMock.Protected().Verify("Dispose", Times.Never(), true);
            }

            [Fact]
            public void ObjectContext_does_not_dispose_underlying_EntityConnection_if_contextOwnConnection_flag_is_not_specified()
            {
                var entityConnectionMock = new Mock<EntityConnection>(MockBehavior.Strict, null, null, true, true);
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Fake connection string");
                entityConnectionMock.Setup(m => m.GetMetadataWorkspace(false)).Returns(new Mock<MetadataWorkspace>().Object);
                entityConnectionMock.Protected().Setup("Dispose", true).Verifiable();

                var objectContext = new ObjectContext(entityConnectionMock.Object);
                objectContext.Dispose();

                entityConnectionMock.Protected().Verify("Dispose", Times.Never(), true);
            }
        }

#if !NET40

        public class SaveChangesAsync : TestBase
        {
            [Fact]
            public void Parameterless_SaveChangesAsync_calls_SaveOption_flags_to_DetectChangesBeforeSave_and_AcceptAllChangesAfterSave()
            {
                var objectContextMock = new Mock<ObjectContextForMock>(null /*entityConnection*/);
                objectContextMock.Setup(m => m.SaveChangesAsync(It.IsAny<SaveOptions>(), It.IsAny<CancellationToken>()))
                                 .Returns(Task.FromResult(0));

                objectContextMock.Object.SaveChangesAsync().Wait();

                objectContextMock.Verify(
                    m =>
                    m.SaveChangesAsync(SaveOptions.DetectChangesBeforeSave | SaveOptions.AcceptAllChangesAfterSave, CancellationToken.None),
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

                var objectContext = CreateObjectContext(entityConnectionMock, objectStateManagerMock);

                objectContext.SaveChangesAsync(SaveOptions.DetectChangesBeforeSave).Wait();

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
                mockEntityAdapter.Setup(m => m.UpdateAsync(mockObjectStateManager.Object, CancellationToken.None))
                                 .Returns(Task.FromResult(1));

                var mockObjectContext
                    = new Mock<ObjectContext>(null, null, null, mockCommandInterceptor.Object, mockEntityAdapter.Object)
                          {
                              CallBase = true
                          };

                var storeConnectionMock = new Mock<DbConnection>();
                storeConnectionMock.Setup(m => m.DataSource).Returns("foo");
                mockObjectContext.Setup(oc => oc.ObjectStateManager).Returns(mockObjectStateManager.Object);
                mockObjectContext.Setup(oc => oc.Connection).Returns(storeConnectionMock.Object);

                mockObjectContext.Object.SaveChangesAsync(SaveOptions.None).Wait();

                mockObjectContext.Verify(oc => oc.EnsureConnectionAsync(CancellationToken.None), Times.Never());
                mockEntityAdapter.Verify(ea => ea.UpdateAsync(mockObjectStateManager.Object, CancellationToken.None));
            }

            [Fact]
            public void Exception_thrown_if_ObjectStateManager_has_entries_with_conceptual_nulls()
            {
                var objectStateManagerMock = new Mock<ObjectStateManager>();
                objectStateManagerMock.Setup(m => m.SomeEntryWithConceptualNullExists()).Returns(true);

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");

                var objectContext = CreateObjectContext(entityConnectionMock, objectStateManagerMock);

                Assert.Equal(
                    Strings.ObjectContext_CommitWithConceptualNull,
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        ExceptionHelpers.UnwrapAggregateExceptions(
                            () =>
                            objectContext.SaveChangesAsync(SaveOptions.None).Wait()))
                          .Message);
            }

            [Fact]
            public void Shortcircuits_if_no_state_changes()
            {
                var mockObjectContext = Mock.Get(MockHelper.CreateMockObjectContext<DbDataRecord>());
                mockObjectContext.CallBase = true;

                var mockServiceProvider = (IServiceProvider)((EntityConnection)mockObjectContext.Object.Connection).StoreProviderFactory;
                var entityAdapterMock = Mock.Get((IEntityAdapter)mockServiceProvider.GetService(typeof(IEntityAdapter)));
                entityAdapterMock.Setup(m => m.UpdateAsync(It.IsAny<IEntityStateManager>(), It.IsAny<CancellationToken>())).Verifiable();

                var entriesAffected = mockObjectContext.Object.SaveChangesAsync(SaveOptions.None, CancellationToken.None).Result;

                entityAdapterMock.Verify(m => m.UpdateAsync(It.IsAny<IEntityStateManager>(), It.IsAny<CancellationToken>()), Times.Never());
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
                var storeConnectionMock = new Mock<DbConnection>();
                storeConnectionMock.Setup(m => m.DataSource).Returns("foo");
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => connectionState);
                entityConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Returns(
                    () =>
                        {
                            connectionState = ConnectionState.Open;
                            return Task.FromResult<object>(null);
                        });
                entityConnectionMock.Setup(m => m.BeginTransaction()).Returns(() => entityTransaction);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(storeConnectionMock.Object);

                // first time return false to by-pass check in the constructor
                var enlistedInUserTransactionCallCount = 0;
                entityConnectionMock.SetupGet(m => m.EnlistedInUserTransaction).
                                     Callback(() => enlistedInUserTransactionCallCount++).
                                     Returns(enlistedInUserTransactionCallCount == 1);

                var objectContext = CreateObjectContext(entityConnectionMock, objectStateManagerMock);
                objectContext.SaveChangesAsync(SaveOptions.None).Wait();

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
                var storeConnectionMock = new Mock<DbConnection>();
                storeConnectionMock.Setup(m => m.DataSource).Returns("foo");
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => connectionState);
                entityConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Returns(
                    () =>
                        {
                            connectionState = ConnectionState.Open;
                            return Task.FromResult<object>(null);
                        });
                entityConnectionMock.SetupGet(m => m.CurrentTransaction).Returns(new Mock<EntityTransaction>().Object);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(storeConnectionMock.Object);

                var metadataWorkspace = new Mock<MetadataWorkspace>();
                metadataWorkspace.Setup(m => m.ShallowCopy()).Returns(() => metadataWorkspace.Object);
                metadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.OSpace)).Returns(true);
                metadataWorkspace.Setup(m => m.GetItemCollection(DataSpace.OCSpace)).Returns(default(ItemCollection));
                metadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var objectContext = CreateObjectContext(entityConnectionMock, objectStateManagerMock, metadataWorkspace);
                objectContext.SaveChangesAsync(SaveOptions.AcceptAllChangesAfterSave).Wait();

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
                var storeConnectionMock = new Mock<DbConnection>();
                storeConnectionMock.Setup(m => m.DataSource).Returns("foo");
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => connectionState);
                entityConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Returns(
                    () =>
                        {
                            connectionState = ConnectionState.Open;
                            return Task.FromResult<object>(null);
                        });
                entityConnectionMock.SetupGet(m => m.CurrentTransaction).Returns(new Mock<EntityTransaction>().Object);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(storeConnectionMock.Object);

                var objectContext = CreateObjectContext(entityConnectionMock, objectStateManagerMock);

                Assert.Equal(
                    Strings.ObjectContext_AcceptAllChangesFailure(new NotSupportedException().Message),
                    Assert.Throws<InvalidOperationException>(
                        () => ExceptionHelpers.UnwrapAggregateExceptions(
                            () => objectContext.SaveChangesAsync(SaveOptions.AcceptAllChangesAfterSave).Result)).Message);
            }

            [Fact]
            public void OnSavingChanges_event_gets_called()
            {
                var mockObjectContext = Mock.Get(MockHelper.CreateMockObjectContext<DbDataRecord>());
                mockObjectContext.CallBase = true;

                var callCount = 0;
                EventHandler saveChangesDelegate = delegate { callCount++; };
                mockObjectContext.Object.SavingChanges += saveChangesDelegate;

                var entriesAffected = mockObjectContext.Object.SaveChanges(SaveOptions.None);

                Assert.Equal(1, callCount);
                Assert.Equal(0, entriesAffected);

                //Ensure that event does not get called when removed
                callCount = 0;
                mockObjectContext.Object.SavingChanges -= saveChangesDelegate;

                entriesAffected = mockObjectContext.Object.SaveChangesAsync(SaveOptions.None).Result;

                Assert.Equal(0, callCount);
                Assert.Equal(0, entriesAffected);
            }

            [Fact]
            public void Raises_expected_exception_from_OnSavingChanges_event()
            {
                var mockObjectContext = Mock.Get(MockHelper.CreateMockObjectContext<DbDataRecord>());
                mockObjectContext.CallBase = true;

                EventHandler saveChangesDelegate = delegate { throw new InvalidOperationException(); };
                mockObjectContext.Object.SavingChanges += saveChangesDelegate;

                Assert.Throws<InvalidOperationException>(
                    () =>
                    ExceptionHelpers.UnwrapAggregateExceptions(
                        () =>
                        mockObjectContext.Object.SaveChangesAsync(SaveOptions.None).Wait()));
            }

            [Fact]
            public void Uses_ExecutionStrategy()
            {
                var objectStateManagerMock = new Mock<ObjectStateManager>();
                objectStateManagerMock.Setup(
                    m => m.GetObjectStateEntriesCount(EntityState.Added | EntityState.Deleted | EntityState.Modified)).Returns(1);

                var storeConnectionMock = new Mock<DbConnection>();
                storeConnectionMock.Setup(m => m.DataSource).Returns("foo");
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(storeConnectionMock.Object);

                var objectContext = CreateObjectContext(entityConnectionMock, objectStateManagerMock);

                var executionStrategyMock = new Mock<IExecutionStrategy>();
                executionStrategyMock.Setup(m => m.ExecuteAsync(It.IsAny<Func<Task<int>>>())).Returns<Func<Task<int>>>(
                    f => Task.FromResult(2));

                MutableResolver.AddResolver<IExecutionStrategy>(key => executionStrategyMock.Object);
                try
                {
                    Assert.Equal(2, objectContext.SaveChangesAsync(SaveOptions.AcceptAllChangesAfterSave).Result);
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }

                executionStrategyMock.Verify(m => m.ExecuteAsync(It.IsAny<Func<Task<int>>>()), Times.Once());
            }
        }

        public class EnsureConnectionAsync
        {
            [Fact]
            public void Calls_EnsureMetadata_if_connection_open()
            {
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                var objectContextMock = new Mock<ObjectContextForMock>(entityConnectionMock.Object)
                                            {
                                                CallBase = true
                                            };
                objectContextMock.Setup(m => m.EnsureMetadata()).Verifiable();

                objectContextMock.Object.EnsureConnectionAsync(CancellationToken.None).Wait();

                objectContextMock.Verify(m => m.EnsureMetadata(), Times.Once());
            }

            [Fact]
            public void Releases_connection_when_exception_caught()
            {
                var connectionMock = new Mock<EntityConnection>();
                connectionMock.Setup(m => m.State).Returns(ConnectionState.Open);
                var objectContextMock = new Mock<ObjectContextForMock>(connectionMock.Object)
                                            {
                                                CallBase = true
                                            };
                objectContextMock.Setup(m => m.EnsureMetadata()).Throws(new MetadataException());

                try
                {
                    objectContextMock.Object.EnsureConnectionAsync(CancellationToken.None).Wait();
                }
                catch (AggregateException)
                {
                    objectContextMock.Verify(m => m.ReleaseConnection(), Times.Once());
                }
            }

            [Fact]
            public void Calls_Open_if_connection_is_closed_or_broken()
            {
                var state = ConnectionState.Broken;
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.Setup(m => m.Close()).Callback(() => { state = ConnectionState.Closed; });
                entityConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Returns(
                    () =>
                    {
                        entityConnectionMock.Verify(m => m.Close(), Times.Once());
                        state = ConnectionState.Open;
                        return Task.FromResult(true);
                    });
                entityConnectionMock.SetupGet(m => m.State).Returns(() => state);

                var objectContextMock = new Mock<ObjectContextForMock>(entityConnectionMock.Object)
                {
                    CallBase = true
                };
                objectContextMock.Setup(m => m.EnsureMetadata());

                objectContextMock.Object.EnsureConnectionAsync(CancellationToken.None).Wait();

                entityConnectionMock.Verify(m => m.OpenAsync(It.IsAny<CancellationToken>()), Times.Once());
            }
        }

        public class ExecuteStoreCommandAsync
        {
            [Fact]
            public void Command_is_executed_with_correct_CommandText()
            {
                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));
                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(() => dbCommandMock.Object);

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnectionMock.Object);

                var objectContext = CreateObjectContext(entityConnectionMock);
                objectContext.ExecuteStoreCommandAsync("Foo").Wait();

                dbCommandMock.VerifySet(m => m.CommandText = "Foo", Times.Once());
                dbCommandMock.Verify(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Once());
            }

            [Fact]
            public void CommandTimeout_is_set_on_created_DbCommand_if_it_was_set_on_ObjectContext()
            {
                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));
                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(() => dbCommandMock.Object);

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnectionMock.Object);

                var objectContext = CreateObjectContext(entityConnectionMock);
                objectContext.CommandTimeout = 10;
                objectContext.ExecuteStoreCommandAsync("Foo").Wait();

                dbCommandMock.VerifySet(m => m.CommandTimeout = 10, Times.Once());
            }

            [Fact]
            public void Transaction_set_on_created_DbCommand_if_it_was_set_on_EntityConnection()
            {
                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));
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

                var objectContext = CreateObjectContext(entityConnectionMock);
                objectContext.ExecuteStoreCommandAsync("Foo").Wait();

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
                                                    if (list.Count == 3
                                                        && list[0] == parameter1
                                                        && list[1] == parameter2
                                                        && list[2] == parameter3)
                                                    {
                                                        correctParameters = true;
                                                    }
                                                });

                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);
                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(() => dbCommandMock.Object);

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnectionMock.Object);

                var objectContext = CreateObjectContext(entityConnectionMock);
                objectContext.ExecuteStoreCommandAsync("Foo", parameter1, parameter2, parameter3).Wait();

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
                                                    if (list.Count == 4
                                                        && list[0] == parameterMockList[0].Object
                                                        && list[1] == parameterMockList[1].Object
                                                        &&
                                                        list[2] == parameterMockList[2].Object
                                                        && list[3] == parameterMockList[3].Object)
                                                    {
                                                        correctParameters = true;
                                                    }
                                                });

                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));
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

                var objectContext = CreateObjectContext(entityConnectionMock);
                objectContext.ExecuteStoreCommandAsync("{0} Foo {1} Bar {2} Baz {3}", 1, null, "Bar", DBNull.Value).Wait();

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
                dbCommandMock.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));
                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(() => dbCommandMock.Object);

                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Foo");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnectionMock.Object);

                var objectContext = CreateObjectContext(entityConnectionMock);

                Assert.Equal(
                    Strings.ObjectContext_ExecuteCommandWithMixOfDbParameterAndValues,
                    Assert.Throws<InvalidOperationException>(
                        () => ExceptionHelpers.UnwrapAggregateExceptions(
                            () => objectContext.ExecuteStoreCommandAsync(
                                "Foo", 1,
                                new Mock<DbParameter>().Object).Result)).Message);
            }
        }

        public class ExecuteStoreQueryAsync
        {
            [Fact]
            public void Command_is_executed_with_correct_CommandText_and_parameters()
            {
                var dbCommandMock = new Mock<DbCommand>();

                var parameterMock = new Mock<DbParameter>();
                var correctParameters = false;
                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock.Setup(m => m.AddRange(It.IsAny<DbParameter[]>())).
                                        Callback(
                                            (Array p) =>
                                                {
                                                    var list = p.ToList<DbParameter>();
                                                    if (list.Count == 1
                                                        && list[0] == parameterMock.Object)
                                                    {
                                                        correctParameters = true;
                                                    }
                                                });

                dbCommandMock.Protected().Setup<Task<DbDataReader>>(
                    "ExecuteDbDataReaderAsync", It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>())
                             .Returns(
                                 Task.FromResult(
                                     Common.Internal.Materialization.MockHelper.CreateDbDataReader(new[] { new[] { new object() } })));
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                objectContext.ExecuteStoreQueryAsync<object>("{0} Foo", parameterMock.Object).Wait();

                dbCommandMock.VerifySet(m => m.CommandText = "{0} Foo", Times.Once());
                dbCommandMock.Protected().Verify(
                    "ExecuteDbDataReaderAsync", Times.Once(), It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>());
                Assert.True(correctParameters);
            }

            [Fact]
            public void Command_is_executed_with_correct_CommandText_and_parameters_with_streaming()
            {
                var dbCommandMock = new Mock<DbCommand>();

                var parameterMock = new Mock<DbParameter>();
                var correctParameters = false;
                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock.Setup(m => m.AddRange(It.IsAny<DbParameter[]>())).
                                        Callback(
                                            (Array p) =>
                                                {
                                                    var list = p.ToList<DbParameter>();
                                                    if (list.Count == 1
                                                        && list[0] == parameterMock.Object)
                                                    {
                                                        correctParameters = true;
                                                    }
                                                });

                var dataReader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(new[] { new[] { new object() } });
                // This reader will throw if buffering is on
                Mock.Get(dataReader).Setup(m => m.ReadAsync(It.IsAny<CancellationToken>())).Throws(new NotImplementedException());
                dbCommandMock.Protected().Setup<Task<DbDataReader>>(
                    "ExecuteDbDataReaderAsync", It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>())
                             .Returns(Task.FromResult(dataReader));
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                objectContext.ExecuteStoreQueryAsync<object>(
                    "{0} Foo", new ExecutionOptions(MergeOption.AppendOnly, true),
                    CancellationToken.None, parameterMock.Object).Wait();

                dbCommandMock.VerifySet(m => m.CommandText = "{0} Foo", Times.Once());
                dbCommandMock.Protected().Verify(
                    "ExecuteDbDataReaderAsync", Times.Once(), It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>());
                Assert.True(correctParameters);
            }

            [Fact]
            public void Connection_is_released_after_reader_exception()
            {
                var dbCommandMock = new Mock<DbCommand>();

                dbCommandMock.Protected().Setup<Task<DbDataReader>>(
                    "ExecuteDbDataReaderAsync", It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>())
                             .Throws(new InvalidOperationException("Foo"));
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => new Mock<DbParameterCollection>().Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                Assert.Equal(
                    "Foo",
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        ExceptionHelpers.UnwrapAggregateExceptions(
                            () =>
                            objectContext.ExecuteStoreQueryAsync<object>("Bar").Wait())).Message);

                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Once());
            }

            [Fact]
            public void Connection_is_released_after_translator_exception()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var dataReader = Common.Internal.Materialization.MockHelper.CreateDbDataReader();

                dbCommandMock.Protected().Setup<Task<DbDataReader>>(
                    "ExecuteDbDataReaderAsync", It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>())
                             .Returns(Task.FromResult(dataReader));
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => new Mock<DbParameterCollection>().Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                Mock.Get(objectContext.MetadataWorkspace).Setup(m => m.GetQueryCacheManager())
                    .Throws(new InvalidOperationException("Foo"));

                Assert.Equal(
                    "Foo",
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        ExceptionHelpers.UnwrapAggregateExceptions(
                            () =>
                            objectContext.ExecuteStoreQueryAsync<object>("Foo").Wait())).Message);

                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Once());
                Mock.Get(dataReader).Protected().Verify("Dispose", Times.Once(), true);
            }
        }

#endif

        private static ObjectContext CreateObjectContext(DbCommand dbCommand)
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(() => dbCommand);

            var metadataWorkspaceMock = new Mock<MetadataWorkspace>
                                            {
                                                CallBase = true
                                            };
            metadataWorkspaceMock.Setup(m => m.ShallowCopy()).Returns(() => metadataWorkspaceMock.Object);
            var edmItemCollection = new EdmItemCollection();
            var providerManifestMock = new Mock<DbProviderManifest>();
            providerManifestMock.Setup(m => m.GetStoreTypes()).Returns(new ReadOnlyCollection<PrimitiveType>(new List<PrimitiveType>()));
            providerManifestMock.Setup(m => m.GetStoreFunctions()).Returns(new ReadOnlyCollection<EdmFunction>(new List<EdmFunction>()));

            var storeItemCollection = new StoreItemCollection(
                FakeSqlProviderFactory.Instance, providerManifestMock.Object, "providerInvariantName", "token");

            metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.OSpace, It.IsAny<bool>()))
                                 .Returns(new ObjectItemCollection());
            metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.OCSpace, It.IsAny<bool>()))
                                 .Returns(new ObjectItemCollection());
            metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.CSpace, It.IsAny<bool>()))
                                 .Returns(edmItemCollection);
            metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.SSpace, It.IsAny<bool>()))
                                 .Returns(storeItemCollection);
            metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.CSSpace, It.IsAny<bool>()))
                                 .Returns(new StorageMappingItemCollection(edmItemCollection, storeItemCollection));
            metadataWorkspaceMock.Setup(m => m.GetQueryCacheManager()).Returns(QueryCacheManager.Create());

            var entityConnectionMock = new Mock<EntityConnection>();
            entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Bar");
            entityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
            entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnectionMock.Object);
            entityConnectionMock.Setup(m => m.GetMetadataWorkspace()).Returns(metadataWorkspaceMock.Object);

            var translator = Common.Internal.Materialization.MockHelper.CreateTranslator<object>();

            var edmTypeMock = new Mock<EdmType>();
            edmTypeMock.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.SimpleType);

            var collectionColumnMap = new SimpleCollectionColumnMap(
                TypeUsage.Create(edmTypeMock.Object), "",
                new ScalarColumnMap(TypeUsage.Create(edmTypeMock.Object), "", 0, 0), null, null);

            var objectStateManagerMock = new Mock<ObjectStateManager>();

            var columnMapFactoryMock = new Mock<ColumnMapFactory>();
            columnMapFactoryMock.Setup(
                m => m.CreateColumnMapFromReaderAndType(
                    It.IsAny<DbDataReader>(), It.IsAny<EdmType>(), It.IsAny<EntitySet>(),
                    It.IsAny<Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping>>()))
                                .Returns(collectionColumnMap);

            columnMapFactoryMock.Setup(
                m => m.CreateColumnMapFromReaderAndClrType(It.IsAny<DbDataReader>(), It.IsAny<Type>(), It.IsAny<MetadataWorkspace>()))
                                .Returns(collectionColumnMap);

            var objectContext = CreateObjectContext(
                entityConnectionMock, objectStateManagerMock, metadataWorkspace: metadataWorkspaceMock, translator: translator,
                columnMapFactory: columnMapFactoryMock.Object);

            return objectContext;
        }

        private static ObjectContext CreateObjectContext(
            Mock<EntityConnection> entityConnectionMock, Mock<ObjectStateManager> objectStateManagerMock = null,
            Mock<MetadataWorkspace> metadataWorkspace = null, Translator translator = null, ColumnMapFactory columnMapFactory = null)
        {
            if (objectStateManagerMock == null)
            {
                objectStateManagerMock = new Mock<ObjectStateManager>();
            }

            if (metadataWorkspace == null)
            {
                metadataWorkspace = new Mock<MetadataWorkspace>();
                metadataWorkspace.Setup(m => m.ShallowCopy()).Returns(() => metadataWorkspace.Object);
                metadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.OSpace)).Returns(true);
                metadataWorkspace.Setup(m => m.GetItemCollection(DataSpace.OCSpace)).Returns(default(ItemCollection));
                metadataWorkspace.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
            }

            var objectContextMock = new Mock<ObjectContext>(new ObjectQueryExecutionPlanFactory(), translator, columnMapFactory, null, null)
                                        {
                                            CallBase = true
                                        };

            objectContextMock.Setup(m => m.Connection).Returns(entityConnectionMock.Object);
            objectContextMock.Setup(m => m.ObjectStateManager).Returns(() => objectStateManagerMock.Object);
            objectContextMock.Setup(m => m.MetadataWorkspace).Returns(() => metadataWorkspace.Object);
            objectContextMock.Setup(m => m.DefaultContainerName).Returns("Bar");

            return objectContextMock.Object;
        }
    }
}
