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
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using System.Data.Entity.SqlServer;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using Moq;
    using Moq.Protected;
    using Xunit;
    using System.Collections;

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

        [Fact]
        public void FactMethodName()
        {
            var ops = new MigrationOperation[]
                          {
                              new AddColumnOperation("dbo.__MigrationHistory", 
                                  new ColumnModel(PrimitiveTypeKind.String)), 
                                  new DropPrimaryKeyOperation(), 
                                  new AddPrimaryKeyOperation()
                          };

            Assert.True(
                ops.Select(o => o.GetType()).SequenceEqual(
                    new[]
                        {
                            typeof(AddColumnOperation),
                            typeof(DropPrimaryKeyOperation),
                            typeof(AddPrimaryKeyOperation)
                        }));
        }

        public class SaveChanges : TestBase
        {
            [Fact]
            public void Parameterless_SaveChanges_calls_SaveOption_flags_to_DetectChangesBeforeSave_and_AcceptAllChangesAfterSave()
            {
                var objectContextMock = new Mock<ObjectContextForMock>(null /*entityConnection*/, null /*entityAdapter*/) { CallBase = true };

                objectContextMock.Setup(
                    m => m.SaveChanges(SaveOptions.DetectChangesBeforeSave | SaveOptions.AcceptAllChangesAfterSave))
                    .Returns(1);

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
                var entityAdapterMock = new Mock<IEntityAdapter>();
                var mockObjectContext = Mock.Get(MockHelper.CreateMockObjectContext<DbDataRecord>(entityAdapterMock.Object));
                mockObjectContext.CallBase = true;

                entityAdapterMock.Setup(m => m.Update(true)).Verifiable();

                var entriesAffected = mockObjectContext.Object.SaveChanges(SaveOptions.None);

                entityAdapterMock.Verify(m => m.Update(true), Times.Never());
                Assert.Equal(0, entriesAffected);
            }

            [Fact]
            public void If_local_transaction_is_necessary_it_gets_created_commited()
            {
                var objectStateManagerMock = new Mock<ObjectStateManager>();
                var hasChangesCount = 0;
                objectStateManagerMock
                    .Setup(m => m.HasChanges())
                    .Callback(() => hasChangesCount++)
                    .Returns(() => hasChangesCount == 1);

                var dbTransaction = new Mock<DbTransaction>();
                var entityTransactionMock = new Mock<EntityTransaction>(
                    new EntityConnection(), dbTransaction.Object);
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
                var hasChangesCount = 0;
                objectStateManagerMock
                    .Setup(m => m.HasChanges())
                    .Callback(() => hasChangesCount++)
                    .Returns(() => hasChangesCount == 1);

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

                objectStateManagerMock.Verify(m => m.GetObjectStateEntries(It.IsAny<EntityState>()), Times.AtLeastOnce());
            }

            [Fact]
            public void Exception_thrown_during_AcceptAllChanges_is_wrapped()
            {
                var objectStateManagerMock = new Mock<ObjectStateManager>();
                var hasChangesCount = 0;
                objectStateManagerMock
                    .Setup(m => m.HasChanges())
                    .Callback(() => hasChangesCount++)
                    .Returns(() => hasChangesCount == 1);
                objectStateManagerMock.Setup(m => m.GetObjectStateEntries(It.IsAny<EntityState>())).Throws<NotSupportedException>();

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
                objectStateManagerMock.Setup(m => m.HasChanges()).Returns(true);
                objectStateManagerMock.Setup(
                    m => m.GetObjectStateEntriesCount(EntityState.Added | EntityState.Deleted | EntityState.Modified)).Returns(1);

                var storeConnectionMock = new Mock<DbConnection>();
                storeConnectionMock.Setup(m => m.DataSource).Returns("foo");
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(storeConnectionMock.Object);

                var objectContext = CreateObjectContext(entityConnectionMock, objectStateManagerMock);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.Execute(It.IsAny<Func<int>>())).Returns<Func<int>>(f => 2);

                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
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

        public class Refresh : TestBase
        {
            [Fact]
            public void Executes_in_a_transaction_using_ExecutionStrategy()
            {
                var shaperMock = MockHelper.CreateShaperMock<object>();
                shaperMock.Setup(m => m.GetEnumerator()).Returns(
                    () => new DbEnumeratorShim<object>(((IEnumerable<object>)new[] { new object() }).GetEnumerator()));

                var objectResultMock = new Mock<ObjectResult<object>>(shaperMock.Object, null, null)
                    {
                        CallBase = true
                    };

                var entityType = new EntityType(
                    "FakeEntityType", "FakeNamespace", DataSpace.CSpace, new[] { "key" }, new EdmMember[] { new EdmProperty("key") });
                var entitySet = new EntitySet("FakeSet", "FakeSchema", "FakeTable", null, entityType);

                var entityContainer = new EntityContainer("FakeContainer", DataSpace.CSpace);
                entityContainer.AddEntitySetBase(entitySet);

                entitySet.ChangeEntityContainerWithoutCollectionFixup(entityContainer);
                entitySet.SetReadOnly();

                var model = new EdmModel(DataSpace.CSpace);
                var omicMock = new Mock<DefaultObjectMappingItemCollection>(new EdmItemCollection(model), new ObjectItemCollection())
                    {
                        CallBase = true
                    };
                var objectTypeMapping = new ObjectTypeMapping(entityType, entityType);
                objectTypeMapping.AddMemberMap(
                    new ObjectPropertyMapping((EdmProperty)entityType.Members.First(), (EdmProperty)entityType.Members.First()));
                omicMock.Setup(m => m.GetMap(It.IsAny<GlobalItem>())).Returns(objectTypeMapping);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>
                    {
                        CallBase = true
                    };
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.OSpace)).Returns(true);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                metadataWorkspaceMock.Setup(m => m.GetEntityContainer(It.IsAny<string>(), It.IsAny<DataSpace>()))
                                     .Returns(entityContainer);
                metadataWorkspaceMock.Setup(m => m.TryGetEntityContainer(It.IsAny<string>(), It.IsAny<DataSpace>(), out entityContainer))
                                     .Returns(true);
                var storeItemCollection = new StoreItemCollection(
                    GenericProviderFactory<DbProviderFactory>.Instance, new SqlProviderManifest("2008"), "System.Data.FakeSqlClient", "2008");
                metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.SSpace)).Returns(storeItemCollection);
#pragma warning disable 618
                metadataWorkspaceMock.Object.RegisterItemCollection(omicMock.Object);
#pragma warning restore 618

                var objectStateManagerMock = new Mock<ObjectStateManager>(metadataWorkspaceMock.Object)
                    {
                        CallBase = true
                    };
                var entityKey = new EntityKey(entitySet, "keyValue");
                var entityEntry = objectStateManagerMock.Object.AddKeyEntry(entityKey, entitySet);

                var objectContextMock = Mock.Get(
                    CreateObjectContext(
                        metadataWorkspaceMock: metadataWorkspaceMock,
                        objectStateManagerMock: objectStateManagerMock));

                var entityWrapperMock = new Mock<IEntityWrapper>();
                entityWrapperMock.Setup(m => m.EntityKey).Returns(entityKey);
                var entityWrapperFactoryMock = new Mock<EntityWrapperFactory>();
                entityWrapperFactoryMock.Setup(
                    m => m.WrapEntityUsingStateManagerGettingEntry(
                        It.IsAny<object>(), It.IsAny<ObjectStateManager>(), out entityEntry))
                                        .Returns(entityWrapperMock.Object);
                objectContextMock.Setup(m => m.EntityWrapperFactory).Returns(entityWrapperFactoryMock.Object);

                var executionPlanMock = new Mock<ObjectQueryExecutionPlan>(
                    MockBehavior.Loose, null, null, null, MergeOption.NoTracking, false, null, null);
                executionPlanMock.Setup(
                    m => m.Execute<object>(It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>()))
                                 .Returns(objectResultMock.Object);
                objectContextMock.Setup(
                    m => m.PrepareRefreshQuery(It.IsAny<RefreshMode>(), It.IsAny<EntitySet>(), It.IsAny<List<EntityKey>>(), It.IsAny<int>()))
                                 .Returns(new Tuple<ObjectQueryExecutionPlan, int>(executionPlanMock.Object, 1));

                // Verify that ExecuteInTransaction calls ObjectQueryExecutionPlan.Execute
                objectContextMock.Setup(
                    m =>
                    m.ExecuteInTransaction(It.IsAny<Func<ObjectResult<object>>>(), It.IsAny<IDbExecutionStrategy>(), It.IsAny<bool>(), It.IsAny<bool>()))
                                 .Returns<Func<ObjectResult<object>>, IDbExecutionStrategy, bool, bool>(
                                     (f, t, s, r) =>
                                         {
                                             executionPlanMock.Verify(
                                                 m =>
                                                 m.Execute<object>(It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>()),
                                                 Times.Never());
                                             var result = f();
                                             executionPlanMock.Verify(
                                                 m =>
                                                 m.Execute<object>(It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>()),
                                                 Times.Once());
                                             return result;
                                         });

                // Verify that ExecutionStrategy.Execute calls ExecuteInTransaction
                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.Execute(It.IsAny<Func<ObjectResult<object>>>()))
                                     .Returns<Func<ObjectResult<object>>>(
                                         f =>
                                             {
                                                 objectContextMock.Verify(
                                                     m =>
                                                     m.ExecuteInTransaction(
                                                         It.IsAny<Func<ObjectResult<object>>>(), It.IsAny<IDbExecutionStrategy>(), false, true),
                                                     Times.Never());
                                                 var result = f();
                                                 objectContextMock.Verify(
                                                     m =>
                                                     m.ExecuteInTransaction(
                                                         It.IsAny<Func<ObjectResult<object>>>(), It.IsAny<IDbExecutionStrategy>(), false, true),
                                                     Times.Once());
                                                 return result;
                                             });

                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
                try
                {
                    objectContextMock.Object.Refresh(RefreshMode.StoreWins, new object());
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }

                // Finally verify that ExecutionStrategy.Execute was called
                executionStrategyMock.Verify(m => m.Execute(It.IsAny<Func<ObjectResult<object>>>()), Times.Once());
            }
        }

        public class EnsureConnection
        {
            [Fact]
            public void Releases_connection_when_exception_caught()
            {
                var connectionMock = new Mock<EntityConnection>();
                connectionMock.Setup(m => m.State).Returns(ConnectionState.Open);
                connectionMock.Setup(m => m.EnlistTransaction(It.IsAny<Transaction>())).Throws(new NotImplementedException());
                var objectContextMock = new Mock<ObjectContextForMock>(connectionMock.Object, null /*entityAdapter*/)
                    {
                        CallBase = true
                    };
                using (new TransactionScope())
                {
                    Assert.Throws<NotImplementedException>(() => objectContextMock.Object.EnsureConnection());
                }

                objectContextMock.Verify(m => m.ReleaseConnection(), Times.Once());
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

                var objectContextMock = new Mock<ObjectContextForMock>(entityConnectionMock.Object, null /*entityAdapter*/)
                    {
                        CallBase = true
                    };
                objectContextMock.Object.EnsureConnection();

                entityConnectionMock.Verify(m => m.Open(), Times.Once());
            }
        }

        public class ExecuteStoreCommand : TestBase
        {
            [Fact]
            public void Command_is_executed_with_correct_CommandText()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(() => dbCommandMock.Object);
                dbConnectionMock.Setup(m => m.DataSource).Returns("fake");

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
                dbConnectionMock.Setup(m => m.DataSource).Returns("fake");

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
                dbConnectionMock.Setup(m => m.DataSource).Returns("fake");

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
                
                parameterCollectionMock
                   .Setup(m => m.GetEnumerator())
                   .Returns(new List<DbParameter>().GetEnumerator());

                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);
                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(() => dbCommandMock.Object);
                dbConnectionMock.Setup(m => m.DataSource).Returns("fake");

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
                parameterCollectionMock
                   .Setup(m => m.GetEnumerator())
                   .Returns(new List<DbParameter>().GetEnumerator());

                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);
                dbCommandMock.Protected().Setup<DbParameter>("CreateDbParameter").
                              Returns(() => parameterMockList[createdParameterCount].Object).
                              Callback(() => createdParameterCount++);

                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(() => dbCommandMock.Object);
                dbConnectionMock.Setup(m => m.DataSource).Returns("fake");

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
                dbConnectionMock.Setup(m => m.DataSource).Returns("fake");

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

            [Fact]
            public void Executes_in_a_transaction_using_ExecutionStrategy()
            {
                Executes_in_a_transaction_using_ExecutionStrategy_implementation(startTransaction: true);
            }

            [Fact]
            public void Executes_without_a_transaction_using_ExecutionStrategy_when_calling_with_DoNotBeginTransaction()
            {
                Executes_in_a_transaction_using_ExecutionStrategy_implementation(startTransaction: false);
            }

            private void Executes_in_a_transaction_using_ExecutionStrategy_implementation(bool startTransaction)
            {
                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.Setup(m => m.ExecuteNonQuery()).Returns(1);

                var objectContextMock = Mock.Get(CreateObjectContext(dbCommandMock.Object));

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();

                // Verify that ExecuteInTransaction calls DbCommand.ExecuteNonQuery
                objectContextMock.Setup(
                    m => m.ExecuteInTransaction(It.IsAny<Func<int>>(), It.IsAny<IDbExecutionStrategy>(), It.IsAny<bool>(), It.IsAny<bool>()))
                                 .Returns<Func<int>, IDbExecutionStrategy, bool, bool>(
                                     (f, t, s, r) =>
                                         {
                                             dbCommandMock.Verify(m => m.ExecuteNonQuery(), Times.Never());
                                             var result = f();
                                             dbCommandMock.Verify(m => m.ExecuteNonQuery(), Times.Once());
                                             return result;
                                         });

                // Verify that ExecutionStrategy.Execute calls ExecuteInTransaction
                executionStrategyMock.Setup(m => m.Execute(It.IsAny<Func<int>>()))
                                     .Returns<Func<int>>(
                                         f =>
                                             {
                                                 objectContextMock.Verify(
                                                     m =>
                                                     m.ExecuteInTransaction(
                                                         It.IsAny<Func<int>>(), It.IsAny<IDbExecutionStrategy>(), It.IsAny<bool>(), It.IsAny<bool>()),
                                                     Times.Never());
                                                 var result = f();
                                                 objectContextMock.Verify(
                                                     m =>
                                                     m.ExecuteInTransaction(It.IsAny<Func<int>>(), It.IsAny<IDbExecutionStrategy>(), startTransaction, true),
                                                     Times.Once());
                                                 return result;
                                             });

                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
                try
                {
                    objectContextMock.Object.ExecuteStoreCommand(
                        startTransaction
                            ? TransactionalBehavior.EnsureTransaction
                            : TransactionalBehavior.DoNotEnsureTransaction, "foo");
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }

                // Finally verify that ExecutionStrategy.Execute was called
                executionStrategyMock.Verify(m => m.Execute(It.IsAny<Func<int>>()), Times.Once());
            }
        }

        public class ExecuteStoreQuery : TestBase
        {
            [Fact]
            public void Command_is_executed_with_correct_CommandText_and_parameters()
            {
                var dbCommandMock = new Mock<DbCommand>();

                var parameterMock = new Mock<DbParameter>();
                var correctParameters = false;
                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock
                    .Setup(m => m.AddRange(It.IsAny<DbParameter[]>()))
                    .Callback(
                                            (Array p) =>
                                                {
                                                    var list = p.ToList<DbParameter>();
                                                    if (list.Count == 1
                                                        && list[0] == parameterMock.Object)
                                                    {
                                                        correctParameters = true;
                                                    }
                                                });
                parameterCollectionMock
                    .Setup(m => m.GetEnumerator())
                    .Returns(new List<DbParameter>().GetEnumerator());

                dbCommandMock.Protected().Setup<DbDataReader>("ExecuteDbDataReader", ItExpr.IsAny<CommandBehavior>()).Returns(
                    Common.Internal.Materialization.MockHelper.CreateDbDataReader(new[] { new[] { new object() } }));
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var result = objectContext.ExecuteStoreQuery<object>("{0} Foo", new ExecutionOptions(MergeOption.AppendOnly, streaming: false), parameterMock.Object);

                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Once());

                dbCommandMock.VerifySet(m => m.CommandText = "{0} Foo", Times.Once());
                dbCommandMock.Protected().Verify("ExecuteDbDataReader", Times.Once(), CommandBehavior.SequentialAccess);
                Assert.True(correctParameters);

                result.Dispose();

                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Once());
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
                parameterCollectionMock
                   .Setup(m => m.GetEnumerator())
                   .Returns(new List<DbParameter>().GetEnumerator());

                var dataReader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(new[] { new[] { new object() } });
                // This reader will throw if buffering is on
                Mock.Get(dataReader).Setup(m => m.Read()).Throws(new NotImplementedException());
                dbCommandMock.Protected().Setup<DbDataReader>("ExecuteDbDataReader", ItExpr.IsAny<CommandBehavior>()).Returns(dataReader);
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var result = objectContext.ExecuteStoreQuery<object>("{0} Foo", new ExecutionOptions(MergeOption.AppendOnly, true), parameterMock.Object);

                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Never());

                dbCommandMock.VerifySet(m => m.CommandText = "{0} Foo", Times.Once());
                dbCommandMock.Protected().Verify("ExecuteDbDataReader", Times.Once(), CommandBehavior.Default);
                Assert.True(correctParameters);

                result.Dispose();

                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Once());
            }

            [Fact]
            public void DbDataReader_is_streaming_by_default()
            {
                var dbCommandMock = new Mock<DbCommand>();

                var storeDataReaderMock = new Mock<DbDataReader>();
                dbCommandMock.Protected().Setup<DbDataReader>("ExecuteDbDataReader", ItExpr.IsAny<CommandBehavior>()).Returns(
                    storeDataReaderMock.Object);

                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock
                   .Setup(m => m.GetEnumerator())
                   .Returns(new List<DbParameter>().GetEnumerator());

                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                objectContext.ExecuteStoreQuery<object>("{0} Foo");

                dbCommandMock.Protected().Verify<DbDataReader>("ExecuteDbDataReader", Times.Once(), CommandBehavior.Default);
                storeDataReaderMock.Verify(m => m.Read(), Times.Never());
                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Never());
            }

            [Fact]
            public void DbDataReader_is_buffered_if_execution_strategy_is_used()
            {
                var dbCommandMock = new Mock<DbCommand>();

                var storeDataReaderMock = new Mock<DbDataReader>();
                dbCommandMock.Protected().Setup<DbDataReader>("ExecuteDbDataReader", ItExpr.IsAny<CommandBehavior>()).Returns(
                    storeDataReaderMock.Object);

                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock
                   .Setup(m => m.GetEnumerator())
                   .Returns(new List<DbParameter>().GetEnumerator());

                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);
                executionStrategyMock.Setup(m => m.Execute(It.IsAny<Func<ObjectResult<object>>>()))
                     .Returns<Func<ObjectResult<object>>>(f =>f());

                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
                try
                {
                    objectContext.ExecuteStoreQuery<object>("{0} Foo");
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }

                dbCommandMock.Protected().Verify<DbDataReader>("ExecuteDbDataReader", Times.Once(), CommandBehavior.SequentialAccess);
                storeDataReaderMock.Verify(m => m.Read(), Times.Once());
                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Once());
            }

            [Fact]
            public void With_entitySet_DbDataReader_is_buffered_if_execution_strategy_is_used()
            {
                var dbCommandMock = new Mock<DbCommand>();

                var storeDataReaderMock = new Mock<DbDataReader>();
                dbCommandMock.Protected().Setup<DbDataReader>("ExecuteDbDataReader", ItExpr.IsAny<CommandBehavior>()).Returns(
                    storeDataReaderMock.Object);

                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock
                   .Setup(m => m.GetEnumerator())
                   .Returns(new List<DbParameter>().GetEnumerator());

                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);
                executionStrategyMock.Setup(m => m.Execute(It.IsAny<Func<ObjectResult<object>>>()))
                     .Returns<Func<ObjectResult<object>>>(f => f());

                SetupFooFunction(objectContext.MetadataWorkspace);

                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
                try
                {
                    objectContext.ExecuteStoreQuery<object>("{0} Foo", "Bar.Foo", MergeOption.AppendOnly);
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }

                dbCommandMock.Protected().Verify<DbDataReader>("ExecuteDbDataReader", Times.Once(), CommandBehavior.SequentialAccess);
                storeDataReaderMock.Verify(m => m.Read(), Times.Once());
                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Once());
            }

            [Fact]
            public void Throws_on_streaming_with_retrying_strategy()
            {
                var objectContext = CreateObjectContext(new Mock<DbCommand>().Object);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);

                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
                try
                {
                    Assert.Equal(
                        Strings.ExecutionStrategy_StreamingNotSupported(executionStrategyMock.Object.GetType().Name),
                        Assert.Throws<InvalidOperationException>(
                            () =>
                            objectContext.ExecuteStoreQuery<object>(
                                "{0} Foo", new ExecutionOptions(MergeOption.AppendOnly, streaming: true))).Message);
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }
            }

            [Fact]
            public void Connection_is_released_after_reader_exception()
            {
                var dbCommandMock = new Mock<DbCommand>();

                dbCommandMock.Protected().Setup<DbDataReader>("ExecuteDbDataReader", ItExpr.IsAny<CommandBehavior>())
                             .Throws(new InvalidOperationException("Foo"));

                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock
                   .Setup(m => m.GetEnumerator())
                   .Returns(new List<DbParameter>().GetEnumerator());

                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                Assert.Equal(
                    "Foo",
                    Assert.Throws<InvalidOperationException>(
                        () => objectContext.ExecuteStoreQuery<object>("Bar")).Message);

                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Once());
            }

            [Fact]
            public void Connection_is_released_after_translator_exception()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var dataReader = Common.Internal.Materialization.MockHelper.CreateDbDataReader();

                dbCommandMock.Protected().Setup<DbDataReader>("ExecuteDbDataReader", ItExpr.IsAny<CommandBehavior>())
                             .Returns(dataReader);
                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock
                   .Setup(m => m.GetEnumerator())
                   .Returns(new List<DbParameter>().GetEnumerator());

                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                Assert.Equal(
                    new ArgumentOutOfRangeException(
                        typeof(MergeOption).Name, Strings.ADP_InvalidEnumerationValue(typeof(MergeOption).Name, 10)).Message,
                    Assert.Throws<ArgumentOutOfRangeException>(
                        () => objectContext.ExecuteStoreQuery<object>("Foo", new ExecutionOptions((MergeOption)10, streaming: false)))
                          .Message);

                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Once());
                Mock.Get(dataReader).Protected().Verify("Dispose", Times.Once(), true);
            }

            [Fact]
            public void Executes_in_a_transaction_using_ExecutionStrategy()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var dataReader = Common.Internal.Materialization.MockHelper.CreateDbDataReader();

                dbCommandMock.Protected().Setup<DbDataReader>(
                    "ExecuteDbDataReader", ItExpr.IsAny<CommandBehavior>())
                             .Returns(dataReader);

                var objectContextMock = Mock.Get(CreateObjectContext(dbCommandMock.Object));

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();

                // Verify that ExecuteInTransaction calls DbCommand.ExecuteDataReader
                objectContextMock.Setup(
                    m =>
                    m.ExecuteInTransaction(It.IsAny<Func<ObjectResult<object>>>(), It.IsAny<IDbExecutionStrategy>(), It.IsAny<bool>(), It.IsAny<bool>()))
                                 .Returns<Func<ObjectResult<object>>, IDbExecutionStrategy, bool, bool>(
                                     (f, t, s, r) =>
                                         {
                                             dbCommandMock.Protected().Verify<DbDataReader>(
                                                 "ExecuteDbDataReader", Times.Never(), CommandBehavior.Default);
                                             var result = f();
                                             dbCommandMock.Protected().Verify<DbDataReader>(
                                                 "ExecuteDbDataReader", Times.Once(), CommandBehavior.Default);
                                             return result;
                                         });

                // Verify that ExecutionStrategy.Execute calls ExecuteInTransaction
                executionStrategyMock.Setup(m => m.Execute(It.IsAny<Func<ObjectResult<object>>>()))
                                     .Returns<Func<ObjectResult<object>>>(
                                         f =>
                                             {
                                                 objectContextMock.Verify(
                                                     m =>
                                                     m.ExecuteInTransaction(
                                                         It.IsAny<Func<ObjectResult<object>>>(), It.IsAny<IDbExecutionStrategy>(), false, It.IsAny<bool>()),
                                                     Times.Never());
                                                 var result = f();
                                                 objectContextMock.Verify(
                                                     m =>
                                                     m.ExecuteInTransaction(
                                                         It.IsAny<Func<ObjectResult<object>>>(), It.IsAny<IDbExecutionStrategy>(), false, It.IsAny<bool>()),
                                                     Times.Once());
                                                 return result;
                                             });

                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
                try
                {
                    objectContextMock.Object.ExecuteStoreQuery<object>("Foo");
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }

                // Finally verify that ExecutionStrategy.Execute was called
                executionStrategyMock.Verify(m => m.Execute(It.IsAny<Func<ObjectResult<object>>>()), Times.Once());
            }
        }

        public class ExecuteInTransaction : TestBase
        {
            [Fact]
            public void Throws_on_an_existing_local_transaction_when_throwOnExistingTransaction_is_true()
            {
                var dbCommandMock = new Mock<DbCommand>();

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var entityConnectionMock = Mock.Get((EntityConnection)objectContext.Connection);
                entityConnectionMock.Setup(m => m.CurrentTransaction).Returns(new Mock<EntityTransaction>().Object);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);

                var executed = false;
                Assert.Equal(
                    Strings.ExecutionStrategy_ExistingTransaction(executionStrategyMock.Object.GetType().Name),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        objectContext.ExecuteInTransaction(
                            () =>
                                {
                                    executed = true;
                                    return executed;
                                }, executionStrategyMock.Object, startLocalTransaction: false, releaseConnectionOnSuccess: false))
                          .Message);

                Assert.False(executed);
            }

            [Fact]
            public void Throws_on_a_connection_enlisted_in_a_user_transaction_when_throwOnExistingTransaction_is_true()
            {
                var dbCommandMock = new Mock<DbCommand>();

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var entityConnectionMock = Mock.Get((EntityConnection)objectContext.Connection);
                entityConnectionMock.Setup(m => m.EnlistedInUserTransaction).Returns(true);
                
                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);

                var executed = false;
                Assert.Equal(
                    Strings.ExecutionStrategy_ExistingTransaction(executionStrategyMock.Object.GetType().Name),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        objectContext.ExecuteInTransaction(
                            () =>
                                {
                                    executed = true;
                                    return executed;
                                }, executionStrategyMock.Object, startLocalTransaction: false, releaseConnectionOnSuccess: false))
                          .Message);

                Assert.False(executed);
            }

            [Fact]
            public void Throws_on_a_new_ambient_transaction_when_throwOnExistingTransaction_is_true()
            {
                var dbCommandMock = new Mock<DbCommand>();

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);

                var executed = false;
                using (new TransactionScope())
                {
                    Assert.Equal(
                        Strings.ExecutionStrategy_ExistingTransaction(executionStrategyMock.Object.GetType().Name),
                        Assert.Throws<InvalidOperationException>(
                            () =>
                            objectContext.ExecuteInTransaction(
                                () =>
                                    {
                                        executed = true;
                                        return executed;
                                    }, executionStrategyMock.Object, startLocalTransaction: false, releaseConnectionOnSuccess: false))
                              .Message);
                }

                Assert.False(executed);
            }

            [Fact]
            public void Executes_in_an_existing_local_transaction_when_throwOnExistingTransaction_is_false()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var entityConnectionMock = Mock.Get((EntityConnection)objectContext.Connection);
                entityConnectionMock.Setup(m => m.CurrentTransaction).Returns(new Mock<EntityTransaction>().Object);
                
                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(false);

                var executed = false;
                Assert.True(
                    objectContext.ExecuteInTransaction(
                        () =>
                            {
                                executed = true;
                                return executed;
                            }, executionStrategyMock.Object, startLocalTransaction: true, releaseConnectionOnSuccess: false));

                Assert.True(executed);
                entityConnectionMock.Verify(m => m.BeginTransaction(), Times.Never());
            }

            [Fact]
            public void Executes_on_a_connection_enlisted_in_a_user_transaction_when_throwOnExistingTransaction_is_true()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var entityConnectionMock = Mock.Get((EntityConnection)objectContext.Connection);
                entityConnectionMock.Setup(m => m.EnlistedInUserTransaction).Returns(true);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(false);

                var executed = false;
                Assert.True(
                    objectContext.ExecuteInTransaction(
                        () =>
                            {
                                executed = true;
                                return executed;
                            }, executionStrategyMock.Object, startLocalTransaction: true, releaseConnectionOnSuccess: false));

                Assert.True(executed);
                entityConnectionMock.Verify(m => m.BeginTransaction(), Times.Never());
            }

            [Fact]
            public void Executes_in_a_new_ambient_transaction_when_throwOnExistingTransaction_is_true()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var entityConnectionMock = Mock.Get((EntityConnection)objectContext.Connection);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(false);

                var executed = false;
                using (new TransactionScope())
                {
                    Assert.True(
                        objectContext.ExecuteInTransaction(
                            () =>
                                {
                                    executed = true;
                                    return executed;
                                }, executionStrategyMock.Object, startLocalTransaction: true, releaseConnectionOnSuccess: false));
                }

                Assert.True(executed);
                entityConnectionMock.Verify(m => m.BeginTransaction(), Times.Never());
            }

            [Fact]
            public void Starts_a_new_local_transaction_when_throwOnExistingTransaction_is_true()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var objectContext = CreateObjectContext(dbCommandMock.Object);
                var objectContextMock = Mock.Get(objectContext);

                var entityConnectionMock = Mock.Get((EntityConnection)objectContext.Connection);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);

                var executed = false;
                Assert.True(
                    objectContext.ExecuteInTransaction(
                        () =>
                            {
                                objectContextMock.Verify(m => m.EnsureConnection(), Times.Once());
                                executed = true;
                                return executed;
                            }, executionStrategyMock.Object, startLocalTransaction: true, releaseConnectionOnSuccess: false));

                Assert.True(executed);
                entityConnectionMock.Verify(m => m.BeginTransaction(), Times.Once());
                objectContextMock.Verify(m => m.ReleaseConnection(), Times.Never());
            }

            [Fact]
            public void Starts_a_new_local_transaction_when_throwOnExistingTransaction_is_false()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var objectContext = CreateObjectContext(dbCommandMock.Object);
                var objectContextMock = Mock.Get(objectContext);

                var entityConnectionMock = Mock.Get((EntityConnection)objectContext.Connection);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);

                var executed = false;
                Assert.True(
                    objectContext.ExecuteInTransaction(
                        () =>
                            {
                                objectContextMock.Verify(m => m.EnsureConnection(), Times.Once());
                                executed = true;
                                return executed;
                            }, executionStrategyMock.Object, startLocalTransaction: true, releaseConnectionOnSuccess: false));

                Assert.True(executed);
                entityConnectionMock.Verify(m => m.BeginTransaction(), Times.Once());
                objectContextMock.Verify(m => m.ReleaseConnection(), Times.Never());
            }

            [Fact]
            public void Doesnt_start_a_new_local_transaction_when_startLocalTransaction_is_false()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var objectContext = CreateObjectContext(dbCommandMock.Object);
                var objectContextMock = Mock.Get(objectContext);

                var entityConnectionMock = Mock.Get((EntityConnection)objectContext.Connection);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(false);

                var executed = false;
                Assert.True(
                    objectContext.ExecuteInTransaction(
                        () =>
                            {
                                objectContextMock.Verify(m => m.EnsureConnection(), Times.Once());
                                executed = true;
                                return executed;
                            }, executionStrategyMock.Object, startLocalTransaction: false, releaseConnectionOnSuccess: false));

                Assert.True(executed);
                entityConnectionMock.Verify(m => m.BeginTransaction(), Times.Never());
                objectContextMock.Verify(m => m.ReleaseConnection(), Times.Never());
            }

            [Fact]
            public void The_connection_is_released_if_an_exception_is_thrown_and_releaseConnectionOnSuccess_set_to_true()
            {
                The_connection_is_released_if_an_exception_is_thrown(true);
            }

            [Fact]
            public void The_connection_is_released_if_an_exception_is_thrown_and_releaseConnectionOnSuccess_set_to_false()
            {
                The_connection_is_released_if_an_exception_is_thrown(false);
            }

            private void The_connection_is_released_if_an_exception_is_thrown(bool releaseConnectionOnSuccess)
            {
                var dbCommandMock = new Mock<DbCommand>();
                var objectContext = CreateObjectContext(dbCommandMock.Object);
                var objectContextMock = Mock.Get(objectContext);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(false);

                var executed = false;
                Assert.Throws<NotImplementedException>(
                    () =>
                    objectContext.ExecuteInTransaction<object>(
                        () =>
                            {
                                objectContextMock.Verify(m => m.EnsureConnection(), Times.Once());
                                executed = true;
                                throw new NotImplementedException();
                            }, executionStrategyMock.Object, /*startLocalTransaction:*/ false, releaseConnectionOnSuccess));

                Assert.True(executed);
                objectContextMock.Verify(m => m.ReleaseConnection(), Times.Once());
            }

            [Fact]
            public void The_connection_is_released_if_no_exception_is_thrown_and_releaseConnectionOnSuccess_set_to_true()
            {
                The_connection_is_released_if_no_exception_is_thrown(true);
            }

            [Fact]
            public void The_connection_is_not_released_if_no_exception_is_thrown_and_releaseConnectionOnSuccess_set_to_false()
            {
                The_connection_is_released_if_no_exception_is_thrown(false);
            }

            private void The_connection_is_released_if_no_exception_is_thrown(bool releaseConnectionOnSuccess)
            {
                var dbCommandMock = new Mock<DbCommand>();
                var objectContext = CreateObjectContext(dbCommandMock.Object);
                var objectContextMock = Mock.Get(objectContext);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(false);

                var executed = false;
                objectContext.ExecuteInTransaction<object>(
                    () =>
                        {
                            objectContextMock.Verify(m => m.EnsureConnection(), Times.Once());
                            executed = true;
                            return null;
                        }, executionStrategyMock.Object, /*startLocalTransaction:*/ false, releaseConnectionOnSuccess);

                Assert.True(executed);
                objectContextMock.Verify(m => m.ReleaseConnection(), releaseConnectionOnSuccess ? Times.Once() : Times.Never());
            }
        }

        public class ExecuteFunction : TestBase
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

                try
                {
                    var entityCommandMock = Mock.Get((EntityCommand)entityCommandDefinition.CreateCommand());

                    var objectContext = CreateObjectContext(entityCommandMock.Object);
                    SetupFooFunction(objectContext.MetadataWorkspace);

                    var entityConnectionMock = Mock.Get((EntityConnection)objectContext.Connection);
                    entityConnectionMock.Setup(m => m.StoreProviderFactory).Returns(new FakeSqlProviderFactory());
                    entityConnectionMock.Setup(m => m.BeginTransaction()).Verifiable();

                    objectContext.ExecuteFunction<object>("Foo", new ExecutionOptions(MergeOption.AppendOnly, streaming: true));

                    entityConnectionMock.Verify(m => m.BeginTransaction(), Times.Never());
                    entityCommandMock.Protected().Verify("ExecuteDbDataReader", Times.Once(), CommandBehavior.Default);
                }
                finally
                {
                    FakeSqlProviderServices.Instance.EntityCommandDefinition = null;
                }
            }

            [Fact]
            public void Throws_on_streaming_with_retrying_strategy()
            {
                var objectContext = CreateObjectContext(new Mock<DbCommand>().Object);
                SetupFooFunction(objectContext.MetadataWorkspace);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);

                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
                try
                {
                    Assert.Equal(
                        Strings.ExecutionStrategy_StreamingNotSupported(executionStrategyMock.Object.GetType().Name),
                        Assert.Throws<InvalidOperationException>(
                            () =>
                            objectContext.ExecuteFunction<object>(
                                "Foo",
                                new ExecutionOptions(MergeOption.AppendOnly, streaming: true))).Message);
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }
            }

            [Fact]
            public void Generic_Executes_in_a_transaction_using_ExecutionStrategy()
            {
                var dataReaderMock = Mock.Get(Common.Internal.Materialization.MockHelper.CreateDbDataReader());
                dataReaderMock.Setup(m => m.FieldCount).Returns(1);
                dataReaderMock.Setup(m => m.GetName(0)).Returns("key");
                var entityCommandDefinition = EntityClient.MockHelper.CreateEntityCommandDefinition(
                    dataReaderMock.Object, new List<EntityParameter>());

                var entityCommandMock = Mock.Get((EntityCommand)entityCommandDefinition.CreateCommand());
                entityCommandMock.Protected().Setup<DbDataReader>(
                    "ExecuteDbDataReader", ItExpr.IsAny<CommandBehavior>())
                                 .Returns(dataReaderMock.Object);

                var objectContextMock = Mock.Get(CreateObjectContext(entityCommandMock.Object));

                SetupFooFunction(objectContextMock.Object.MetadataWorkspace);
                var entityConnectionMock = Mock.Get((EntityConnection)objectContextMock.Object.Connection);
                entityConnectionMock.Setup(m => m.StoreProviderFactory).Returns(new FakeSqlProviderFactory());

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);

                // Verify that ExecuteInTransaction calls DbCommand.ExecuteDataReader
                objectContextMock.Setup(
                    m =>
                    m.ExecuteInTransaction(It.IsAny<Func<ObjectResult<object>>>(), It.IsAny<IDbExecutionStrategy>(), It.IsAny<bool>(), It.IsAny<bool>()))
                                 .Returns<Func<ObjectResult<object>>, IDbExecutionStrategy, bool, bool>(
                                     (f, t, s, r) =>
                                         {
                                             entityCommandMock.Protected().Verify<DbDataReader>(
                                                 "ExecuteDbDataReader", Times.Never(), CommandBehavior.SequentialAccess);
                                             var result = f();
                                             entityCommandMock.Protected().Verify<DbDataReader>(
                                                 "ExecuteDbDataReader", Times.Once(), CommandBehavior.SequentialAccess);
                                             return result;
                                         });

                // Verify that ExecutionStrategy.Execute calls ExecuteInTransaction
                executionStrategyMock.Setup(m => m.Execute(It.IsAny<Func<ObjectResult<object>>>()))
                                     .Returns<Func<ObjectResult<object>>>(
                                         f =>
                                             {
                                                 objectContextMock.Verify(
                                                     m =>
                                                     m.ExecuteInTransaction(
                                                         It.IsAny<Func<ObjectResult<object>>>(), It.IsAny<IDbExecutionStrategy>(), true, true),
                                                     Times.Never());
                                                 var result = f();
                                                 objectContextMock.Verify(
                                                     m =>
                                                     m.ExecuteInTransaction(
                                                         It.IsAny<Func<ObjectResult<object>>>(), It.IsAny<IDbExecutionStrategy>(), true, true),
                                                     Times.Once());
                                                 return result;
                                             });

                FakeSqlProviderServices.Instance.EntityCommandDefinition = entityCommandDefinition;
                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
                try
                {
                    objectContextMock.Object.ExecuteFunction<object>("Foo");
                }
                finally
                {
                    FakeSqlProviderServices.Instance.EntityCommandDefinition = null;
                    MutableResolver.ClearResolvers();
                }

                // Finally verify that ExecutionStrategy.Execute was called
                executionStrategyMock.Verify(m => m.Execute(It.IsAny<Func<ObjectResult<object>>>()), Times.Once());
            }

            [Fact]
            public void NonGeneric_Executes_in_a_transaction_using_ExecutionStrategy()
            {
                var dataReaderMock = Mock.Get(Common.Internal.Materialization.MockHelper.CreateDbDataReader());
                dataReaderMock.Setup(m => m.FieldCount).Returns(1);
                var entityCommandDefinition = EntityClient.MockHelper.CreateEntityCommandDefinition(
                    dataReaderMock.Object, new List<EntityParameter>());

                var objectContextMock = Mock.Get(CreateObjectContext(entityCommandDefinition.CreateCommand()));

                SetupFooFunction(objectContextMock.Object.MetadataWorkspace);
                var entityConnectionMock = Mock.Get((EntityConnection)objectContextMock.Object.Connection);
                entityConnectionMock.Setup(m => m.StoreProviderFactory).Returns(new FakeSqlProviderFactory());

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();

                // Verify that ExecuteInTransaction calls DbCommand.ExecuteNonQuery
                objectContextMock.Setup(
                    m => m.ExecuteInTransaction(It.IsAny<Func<int>>(), It.IsAny<IDbExecutionStrategy>(), It.IsAny<bool>(), It.IsAny<bool>()))
                                 .Returns<Func<int>, IDbExecutionStrategy, bool, bool>(
                                     (f, t, s, r) =>
                                         {
                                             dataReaderMock.Verify(m => m.Read(), Times.Never());
                                             var result = f();
                                             dataReaderMock.Verify(m => m.Read(), Times.Once());
                                             return result;
                                         });

                // Verify that ExecutionStrategy.Execute calls ExecuteInTransaction
                executionStrategyMock.Setup(m => m.Execute(It.IsAny<Func<int>>()))
                                     .Returns<Func<int>>(
                                         f =>
                                             {
                                                 objectContextMock.Verify(
                                                     m => m.ExecuteInTransaction(It.IsAny<Func<int>>(), It.IsAny<IDbExecutionStrategy>(), true, true),
                                                     Times.Never());
                                                 var result = f();
                                                 objectContextMock.Verify(
                                                     m => m.ExecuteInTransaction(It.IsAny<Func<int>>(), It.IsAny<IDbExecutionStrategy>(), true, true),
                                                     Times.Once());
                                                 return result;
                                             });

                FakeSqlProviderServices.Instance.EntityCommandDefinition = entityCommandDefinition;
                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
                try
                {
                    objectContextMock.Object.ExecuteFunction("Foo");
                }
                finally
                {
                    FakeSqlProviderServices.Instance.EntityCommandDefinition = null;
                    MutableResolver.ClearResolvers();
                }

                // Finally verify that ExecutionStrategy.Execute was called
                executionStrategyMock.Verify(m => m.Execute(It.IsAny<Func<int>>()), Times.Once());
            }
        }

        public class Dispose
        {
            [Fact]
            public void ObjectContext_disposes_underlying_EntityConnection_if_contextOwnConnection_flag_is_set()
            {
                var entityConnectionMock = new Mock<EntityConnection>(null, null, true, true, null);
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Fake connection string");
                entityConnectionMock.Setup(m => m.GetMetadataWorkspace()).Returns(new Mock<MetadataWorkspace>().Object);
                entityConnectionMock.Protected().Setup("Dispose", true).Verifiable();

                var objectContext = new ObjectContext(entityConnectionMock.Object, true);

                objectContext.Dispose();

                entityConnectionMock.Protected().Verify("Dispose", Times.Once(), true);
            }

            [Fact]
            public void ObjectContext_does_not_dispose_underlying_EntityConnection_if_contextOwnConnection_flag_is_not_set()
            {
                var entityConnectionMock = new Mock<EntityConnection>(null, null, true, true, null);
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Fake connection string");
                entityConnectionMock.Setup(m => m.GetMetadataWorkspace()).Returns(new Mock<MetadataWorkspace>().Object);
                entityConnectionMock.Protected().Setup("Dispose", false).Verifiable();

                var objectContext = new ObjectContext(entityConnectionMock.Object, false);
                objectContext.Dispose();

                entityConnectionMock.Protected().Verify("Dispose", Times.Never(), true);
            }

            [Fact]
            public void ObjectContext_does_not_dispose_underlying_EntityConnection_if_contextOwnConnection_flag_is_not_specified()
            {
                var entityConnectionMock = new Mock<EntityConnection>(null, null, true, true, null);
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("Fake connection string");
                entityConnectionMock.Setup(m => m.GetMetadataWorkspace()).Returns(new Mock<MetadataWorkspace>().Object);
                entityConnectionMock.Protected().Setup("Dispose", true).Verifiable();

                var objectContext = new ObjectContext(entityConnectionMock.Object);
                objectContext.Dispose();

                entityConnectionMock.Protected().Verify("Dispose", Times.Never(), true);
            }
        }

        public class InterceptionContext
        {
            [Fact]
            public void InterceptionContext_has_ObjectContext_by_default()
            {
                var objectContext = new ObjectContext();
                Assert.Equal(new [] { objectContext }, objectContext.InterceptionContext.ObjectContexts);
            }
        }

#if !NET40

        public class SaveChangesAsync : TestBase
        {
            [Fact]
            public void Parameterless_SaveChangesAsync_calls_SaveOption_flags_to_DetectChangesBeforeSave_and_AcceptAllChangesAfterSave()
            {
                var objectContextMock = new Mock<ObjectContextForMock>(null /*entityConnection*/, null /*entityAdapter*/) { CallBase = true };
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
                        () => ExceptionHelpers.UnwrapAggregateExceptions(
                            () => objectContext.SaveChangesAsync(SaveOptions.None).Wait()))
                          .Message);
            }

            [Fact]
            public void Shortcircuits_if_no_state_changes()
            {
                var entityAdapterMock = new Mock<IEntityAdapter>();
                var mockObjectContext = Mock.Get(MockHelper.CreateMockObjectContext<DbDataRecord>(entityAdapterMock.Object));
                mockObjectContext.CallBase = true;

                entityAdapterMock.Setup(m => m.UpdateAsync(It.IsAny<CancellationToken>())).Verifiable();

                var entriesAffected = mockObjectContext.Object.SaveChangesAsync(SaveOptions.None, CancellationToken.None).Result;

                entityAdapterMock.Verify(m => m.UpdateAsync(It.IsAny<CancellationToken>()), Times.Never());
                Assert.Equal(0, entriesAffected);
            }

            [Fact]
            public void If_local_transaction_is_necessary_it_gets_created_commited()
            {
                var objectStateManagerMock = new Mock<ObjectStateManager>();
                var hasChangesCount = 0;
                objectStateManagerMock
                    .Setup(m => m.HasChanges())
                    .Callback(() => hasChangesCount++)
                    .Returns(() => hasChangesCount == 1);

                var dbTransaction = new Mock<DbTransaction>();
                var entityTransactionMock = new Mock<EntityTransaction>(
                    new EntityConnection(), dbTransaction.Object);
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
                var hasChangesCount = 0;
                objectStateManagerMock
                    .Setup(m => m.HasChanges())
                    .Callback(() => hasChangesCount++)
                    .Returns(() => hasChangesCount == 1);

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

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>();
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.OSpace)).Returns(true);
                metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.OCSpace)).Returns(default(ItemCollection));
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                var storeItemCollection = new StoreItemCollection(
                    GenericProviderFactory<DbProviderFactory>.Instance, new SqlProviderManifest("2008"), "System.Data.FakeSqlClient", "2008");
                metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.SSpace)).Returns(storeItemCollection);

                var objectContext = CreateObjectContext(entityConnectionMock, objectStateManagerMock, metadataWorkspaceMock);
                objectContext.SaveChangesAsync(SaveOptions.AcceptAllChangesAfterSave).Wait();

                objectStateManagerMock.Verify(m => m.GetObjectStateEntries(It.IsAny<EntityState>()), Times.AtLeastOnce());
            }

            [Fact]
            public void Exception_thrown_during_AcceptAllChanges_is_wrapped()
            {
                var objectStateManagerMock = new Mock<ObjectStateManager>();
                var hasChangesCount = 0;
                objectStateManagerMock
                    .Setup(m => m.HasChanges())
                    .Callback(() => hasChangesCount++)
                    .Returns(() => hasChangesCount == 1);
                objectStateManagerMock.Setup(m => m.GetObjectStateEntries(It.IsAny<EntityState>())).Throws<NotSupportedException>();

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
                objectStateManagerMock.Setup(m => m.HasChanges()).Returns(true);
                objectStateManagerMock.Setup(
                    m => m.GetObjectStateEntriesCount(EntityState.Added | EntityState.Deleted | EntityState.Modified)).Returns(1);

                var storeConnectionMock = new Mock<DbConnection>();
                storeConnectionMock.Setup(m => m.DataSource).Returns("foo");
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(storeConnectionMock.Object);

                var objectContext = CreateObjectContext(entityConnectionMock, objectStateManagerMock);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.ExecuteAsync(It.IsAny<Func<Task<int>>>(), It.IsAny<CancellationToken>()))
                                     .Returns<Func<Task<int>>, CancellationToken>(
                                         (f, c) => Task.FromResult(2));

                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
                try
                {
                    Assert.Equal(2, objectContext.SaveChangesAsync(SaveOptions.AcceptAllChangesAfterSave).Result);
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }

                executionStrategyMock.Verify(m => m.ExecuteAsync(It.IsAny<Func<Task<int>>>(), It.IsAny<CancellationToken>()), Times.Once());
            }

            [Fact]
            public void OperationCanceledException_thrown_before_saving_changes_if_task_is_cancelled()
            {
                var objectContext = new Mock<ObjectContextForMock>(new Mock<EntityConnection>().Object, /*entityAdapter*/ null)
                {
                    CallBase = true
                }.Object;

                objectContext.AsyncMonitor.Enter();

                Assert.Throws<OperationCanceledException>(
                    () => objectContext.SaveChangesAsync(new CancellationToken(canceled: true))
                        .GetAwaiter().GetResult());

                Assert.Throws<OperationCanceledException>(
                    () => objectContext.SaveChangesAsync(SaveOptions.None, new CancellationToken(canceled: true))
                        .GetAwaiter().GetResult());
            }
        }

        public class RefreshAsync : TestBase
        {
            [Fact]
            public void Executes_in_a_transaction_using_ExecutionStrategy()
            {
                var shaperMock = MockHelper.CreateShaperMock<object>();
                shaperMock.Setup(m => m.GetEnumerator()).Returns(
                    () => new DbEnumeratorShim<object>(((IEnumerable<object>)new[] { new object() }).GetEnumerator()));

                var objectResultMock = new Mock<ObjectResult<object>>(shaperMock.Object, null, null)
                    {
                        CallBase = true
                    };

                var entityType = new EntityType(
                    "FakeEntityType", "FakeNamespace", DataSpace.CSpace, new[] { "key" }, new EdmMember[] { new EdmProperty("key") });
                var entitySet = new EntitySet("FakeSet", "FakeSchema", "FakeTable", null, entityType);

                var entityContainer = new EntityContainer("FakeContainer", DataSpace.CSpace);
                entityContainer.AddEntitySetBase(entitySet);

                entitySet.ChangeEntityContainerWithoutCollectionFixup(entityContainer);
                entitySet.SetReadOnly();

                var model = new EdmModel(DataSpace.CSpace);
                var omicMock = new Mock<DefaultObjectMappingItemCollection>(new EdmItemCollection(model), new ObjectItemCollection())
                    {
                        CallBase = true
                    };
                var objectTypeMapping = new ObjectTypeMapping(entityType, entityType);
                objectTypeMapping.AddMemberMap(
                    new ObjectPropertyMapping((EdmProperty)entityType.Members.First(), (EdmProperty)entityType.Members.First()));
                omicMock.Setup(m => m.GetMap(It.IsAny<GlobalItem>())).Returns(objectTypeMapping);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>
                    {
                        CallBase = true
                    };
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.OSpace)).Returns(true);
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);
                metadataWorkspaceMock.Setup(m => m.GetEntityContainer(It.IsAny<string>(), It.IsAny<DataSpace>()))
                                     .Returns(entityContainer);
                metadataWorkspaceMock.Setup(m => m.TryGetEntityContainer(It.IsAny<string>(), It.IsAny<DataSpace>(), out entityContainer))
                                     .Returns(true);
                var storeItemCollection = new StoreItemCollection(
                    GenericProviderFactory<DbProviderFactory>.Instance, new SqlProviderManifest("2008"), "System.Data.FakeSqlClient", "2008");
                metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.SSpace)).Returns(storeItemCollection);
#pragma warning disable 618
                metadataWorkspaceMock.Object.RegisterItemCollection(omicMock.Object);
#pragma warning restore 618

                var objectStateManagerMock = new Mock<ObjectStateManager>(metadataWorkspaceMock.Object)
                    {
                        CallBase = true
                    };
                var entityKey = new EntityKey(entitySet, "keyValue");
                var entityEntry = objectStateManagerMock.Object.AddKeyEntry(entityKey, entitySet);

                var objectContextMock = Mock.Get(
                    CreateObjectContext(
                        metadataWorkspaceMock: metadataWorkspaceMock,
                        objectStateManagerMock: objectStateManagerMock));

                var entityWrapperMock = new Mock<IEntityWrapper>();
                entityWrapperMock.Setup(m => m.EntityKey).Returns(entityKey);
                var entityWrapperFactoryMock = new Mock<EntityWrapperFactory>();
                entityWrapperFactoryMock.Setup(
                    m => m.WrapEntityUsingStateManagerGettingEntry(
                        It.IsAny<object>(), It.IsAny<ObjectStateManager>(), out entityEntry))
                                        .Returns(entityWrapperMock.Object);
                objectContextMock.Setup(m => m.EntityWrapperFactory).Returns(entityWrapperFactoryMock.Object);

                var executionPlanMock = new Mock<ObjectQueryExecutionPlan>(
                    MockBehavior.Loose, null, null, null, MergeOption.NoTracking, false, null, null);
                executionPlanMock.Setup(
                    m =>
                    m.ExecuteAsync<object>(It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>(), It.IsAny<CancellationToken>()))
                                 .Returns(Task.FromResult(objectResultMock.Object));
                objectContextMock.Setup(
                    m => m.PrepareRefreshQuery(It.IsAny<RefreshMode>(), It.IsAny<EntitySet>(), It.IsAny<List<EntityKey>>(), It.IsAny<int>()))
                                 .Returns(new Tuple<ObjectQueryExecutionPlan, int>(executionPlanMock.Object, 1));

                // Verify that ExecuteInTransaction calls ObjectQueryExecutionPlan.Execute
                objectContextMock.Setup(
                    m =>
                    m.ExecuteInTransactionAsync(
                        It.IsAny<Func<Task<ObjectResult<object>>>>(), It.IsAny<IDbExecutionStrategy>(), It.IsAny<bool>(), It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                                 .Returns<Func<Task<ObjectResult<object>>>, IDbExecutionStrategy, bool, bool, CancellationToken>(
                                     (f, t, s, r, ct) =>
                                         {
                                             executionPlanMock.Verify(
                                                 m =>
                                                 m.ExecuteAsync<object>(
                                                     It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>(),
                                                     It.IsAny<CancellationToken>()),
                                                 Times.Never());
                                             var result = f().Result;
                                             executionPlanMock.Verify(
                                                 m =>
                                                 m.ExecuteAsync<object>(
                                                     It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>(),
                                                     It.IsAny<CancellationToken>()),
                                                 Times.Once());
                                             return Task.FromResult(result);
                                         });

                // Verify that ExecutionStrategy.Execute calls ExecuteInTransaction
                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(
                    m => m.ExecuteAsync(It.IsAny<Func<Task<ObjectResult<object>>>>(), It.IsAny<CancellationToken>()))
                                     .Returns<Func<Task<ObjectResult<object>>>, CancellationToken>(
                                         (f, ct) =>
                                             {
                                                 objectContextMock.Verify(
                                                     m =>
                                                     m.ExecuteInTransactionAsync(
                                                         It.IsAny<Func<Task<ObjectResult<object>>>>(), It.IsAny<IDbExecutionStrategy>(), false, true,
                                                         It.IsAny<CancellationToken>()),
                                                     Times.Never());
                                                 var result = f().Result;
                                                 objectContextMock.Verify(
                                                     m =>
                                                     m.ExecuteInTransactionAsync(
                                                         It.IsAny<Func<Task<ObjectResult<object>>>>(), It.IsAny<IDbExecutionStrategy>(), false, true,
                                                         It.IsAny<CancellationToken>()),
                                                     Times.Once());
                                                 return Task.FromResult(result);
                                             });

                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
                try
                {
                    objectContextMock.Object.RefreshAsync(RefreshMode.StoreWins, new object()).Wait();
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }

                // Finally verify that ExecutionStrategy.Execute was called
                executionStrategyMock.Verify(
                    m => m.ExecuteAsync(It.IsAny<Func<Task<ObjectResult<object>>>>(), It.IsAny<CancellationToken>()), Times.Once());
            }

            [Fact]
            public void Parameters_checked_before_checking_for_cancellation()
            {
                var objectContext = new Mock<ObjectContextForMock>(new Mock<EntityConnection>().Object, /*entityAdapter*/ null)
                {
                    CallBase = true
                }.Object;

                objectContext.AsyncMonitor.Enter();

                Assert.Equal(
                    "collection",
                    Assert.Throws<ArgumentNullException>(
                        () => objectContext.RefreshAsync(RefreshMode.StoreWins, (IEnumerable)null, new CancellationToken(canceled: true))
                            .GetAwaiter().GetResult()).ParamName);

                Assert.Equal(
                    "entity",
                    Assert.Throws<ArgumentNullException>(
                        () => objectContext.RefreshAsync(RefreshMode.StoreWins, (object)null, new CancellationToken(canceled: true))
                            .GetAwaiter().GetResult()).ParamName);
            }

            [Fact]
            public void OperationCanceledException_thrown_before_executing_if_task_is_cancelled()
            {
                var objectContext = new Mock<ObjectContextForMock>(new Mock<EntityConnection>().Object, /*entityAdapter*/ null)
                {
                    CallBase = true
                }.Object;

                objectContext.AsyncMonitor.Enter();

                Assert.Throws<OperationCanceledException>(
                    () => objectContext.RefreshAsync(RefreshMode.StoreWins, new object[0], new CancellationToken(canceled: true))
                        .GetAwaiter().GetResult());

                Assert.Throws<OperationCanceledException>(
                    () => objectContext.RefreshAsync(RefreshMode.StoreWins, new object(), new CancellationToken(canceled: true))
                        .GetAwaiter().GetResult());
            }
        }

        public class EnsureConnectionAsync
        {
            [Fact]
            public void Releases_connection_when_exception_caught()
            {
                var connectionMock = new Mock<EntityConnection>();
                connectionMock.Setup(m => m.State).Returns(ConnectionState.Open);
                connectionMock.Setup(m => m.EnlistTransaction(It.IsAny<Transaction>())).Throws(new NotImplementedException());
                var objectContextMock = new Mock<ObjectContextForMock>(connectionMock.Object, null /*entityAdapter*/)
                    {
                        CallBase = true
                    };

                using (new TransactionScope())
                {
                    Assert.Throws<AggregateException>(() => objectContextMock.Object.EnsureConnectionAsync(CancellationToken.None).Wait());
                }

                objectContextMock.Verify(m => m.ReleaseConnection(), Times.Once());
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

                var objectContextMock = new Mock<ObjectContextForMock>(entityConnectionMock.Object, null /*entityAdapter*/)
                    {
                        CallBase = true
                    };
                objectContextMock.Object.EnsureConnectionAsync(CancellationToken.None).Wait();

                entityConnectionMock.Verify(m => m.OpenAsync(It.IsAny<CancellationToken>()), Times.Once());
            }

            [Fact]
            public void OperationCanceledException_thrown_before_touching_connection_if_task_is_cancelled()
            {
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.Setup(c => c.State).Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

                var objectContextMock = new Mock<ObjectContextForMock>(entityConnectionMock.Object, /*entityAdapter*/ null)
                {
                    CallBase = true
                };

                Assert.Throws<OperationCanceledException>(
                    () => objectContextMock.Object.EnsureConnectionAsync(new CancellationToken(canceled: true))
                        .GetAwaiter().GetResult());
            }
        }

        public class ExecuteStoreCommandAsync : TestBase
        {
            [Fact]
            public void Command_is_executed_with_correct_CommandText()
            {
                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));

                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock
                   .Setup(m => m.GetEnumerator())
                   .Returns(new List<DbParameter>().GetEnumerator());

                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);

                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(() => dbCommandMock.Object);
                dbConnectionMock.Setup(m => m.DataSource).Returns("fake");

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
                dbConnectionMock.Setup(m => m.DataSource).Returns("fake");

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
                dbConnectionMock.Setup(m => m.DataSource).Returns("fake");

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

                parameterCollectionMock
                   .Setup(m => m.GetEnumerator())
                   .Returns(new List<DbParameter>().GetEnumerator());

                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);
                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(() => dbCommandMock.Object);
                dbConnectionMock.Setup(m => m.DataSource).Returns("fake");

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
                                                        && list[2] == parameterMockList[2].Object
                                                        && list[3] == parameterMockList[3].Object)
                                                    {
                                                        correctParameters = true;
                                                    }
                                                });

                parameterCollectionMock
                   .Setup(m => m.GetEnumerator())
                   .Returns(new List<DbParameter>().GetEnumerator());

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
                dbConnectionMock.Setup(m => m.DataSource).Returns("fake");

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
                dbConnectionMock.Setup(m => m.DataSource).Returns("fake");

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

            [Fact]
            public void Executes_in_a_transaction_using_ExecutionStrategy()
            {
                Executes_in_a_transaction_using_ExecutionStrategy_implementation(startTransaction: true);
            }

            [Fact]
            public void Executes_without_a_transaction_using_ExecutionStrategy_when_calling_with_DoNotBeginTransaction()
            {
                Executes_in_a_transaction_using_ExecutionStrategy_implementation(startTransaction: false);
            }

            private static void Executes_in_a_transaction_using_ExecutionStrategy_implementation(bool startTransaction)
            {
                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));

                var objectContextMock = Mock.Get(CreateObjectContext(dbCommandMock.Object));

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();

                // Verify that ExecuteInTransactionAsync calls DbCommand.ExecuteNonQueryAsync
                objectContextMock.Setup(
                    m => m.ExecuteInTransactionAsync(
                        It.IsAny<Func<Task<int>>>(),
                        It.IsAny<IDbExecutionStrategy>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                                 .Returns<Func<Task<int>>, IDbExecutionStrategy, bool, bool, CancellationToken>(
                                     (f, t, s, r, c) =>
                                         {
                                             dbCommandMock.Verify(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Never());
                                             var result = f();
                                             dbCommandMock.Verify(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Once());
                                             return result;
                                         });

                // Verify that ExecutionStrategy.ExecuteAsync calls ExecuteInTransactionAsync
                executionStrategyMock.Setup(
                    m => m.ExecuteAsync(It.IsAny<Func<Task<int>>>(), It.IsAny<CancellationToken>()))
                                     .Returns<Func<Task<int>>, CancellationToken>(
                                         (f, c) =>
                                             {
                                                 objectContextMock.Verify(
                                                     m =>
                                                     m.ExecuteInTransactionAsync(
                                                         It.IsAny<Func<Task<int>>>(), It.IsAny<IDbExecutionStrategy>(), It.IsAny<bool>(), It.IsAny<bool>(),
                                                         It.IsAny<CancellationToken>()),
                                                     Times.Never());
                                                 var result = f().Result;
                                                 objectContextMock.Verify(
                                                     m =>
                                                     m.ExecuteInTransactionAsync(
                                                         It.IsAny<Func<Task<int>>>(), It.IsAny<IDbExecutionStrategy>(), startTransaction, true,
                                                         It.IsAny<CancellationToken>()),
                                                     Times.Once());
                                                 return Task.FromResult(result);
                                             });

                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
                try
                {
                    Assert.NotNull(
                        objectContextMock.Object.ExecuteStoreCommandAsync(
                            startTransaction
                                ? TransactionalBehavior.EnsureTransaction
                                : TransactionalBehavior.DoNotEnsureTransaction, "foo").Result);
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }

                // Finally verify that ExecutionStrategy.ExecuteAsync was called
                executionStrategyMock.Verify(
                    m => m.ExecuteAsync(It.IsAny<Func<Task<int>>>(), It.IsAny<CancellationToken>()), Times.Once());
            }

            [Fact]
            public void OperationCanceledException_thrown_before_executing_store_command_if_task_is_cancelled()
            {
                var objectContext = new Mock<ObjectContextForMock>(new Mock<EntityConnection>().Object, /*entityAdapter*/ null)
                {
                    CallBase = true
                }.Object;

                objectContext.AsyncMonitor.Enter();

                Assert.Throws<OperationCanceledException>(
                    () => objectContext.ExecuteStoreCommandAsync(
                        "SELECT CORRECT DATA FROM STORE",
                        new CancellationToken(canceled: true))
                        .GetAwaiter().GetResult());

                Assert.Throws<OperationCanceledException>(
                    () => objectContext.ExecuteStoreCommandAsync(
                        TransactionalBehavior.EnsureTransaction,
                        "SELECT CORRECT DATA FROM STORE",
                        new CancellationToken(canceled: true))
                        .GetAwaiter().GetResult());
            }
        }

        public class ExecuteStoreQueryAsync : TestBase
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
                parameterCollectionMock
                   .Setup(m => m.GetEnumerator())
                   .Returns(new List<DbParameter>().GetEnumerator());
                dbCommandMock.Protected().Setup<Task<DbDataReader>>(
                    "ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                             .Returns(
                                 Task.FromResult(
                                     Common.Internal.Materialization.MockHelper.CreateDbDataReader(new[] { new[] { new object() } })));
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var result = objectContext.ExecuteStoreQueryAsync<object>(
                        "{0} Foo", new ExecutionOptions(MergeOption.AppendOnly, streaming: false), parameterMock.Object).Result;

                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Once());

                dbCommandMock.VerifySet(m => m.CommandText = "{0} Foo", Times.Once());
                dbCommandMock.Protected().Verify(
                    "ExecuteDbDataReaderAsync", Times.Once(), CommandBehavior.SequentialAccess, ItExpr.IsAny<CancellationToken>());
                Assert.True(correctParameters);

                result.Dispose();

                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Once());
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
                parameterCollectionMock
                   .Setup(m => m.GetEnumerator())
                   .Returns(new List<DbParameter>().GetEnumerator());
                var dataReader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(new[] { new[] { new object() } });
                // This reader will throw if buffering is on
                Mock.Get(dataReader).Setup(m => m.ReadAsync(It.IsAny<CancellationToken>())).Throws(new NotImplementedException());
                dbCommandMock.Protected().Setup<Task<DbDataReader>>(
                    "ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                             .Returns(Task.FromResult(dataReader));
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var result = objectContext.ExecuteStoreQueryAsync<object>(
                    "{0} Foo", new ExecutionOptions(MergeOption.AppendOnly, true),
                    CancellationToken.None, parameterMock.Object).Result;

                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Never());

                dbCommandMock.VerifySet(m => m.CommandText = "{0} Foo", Times.Once());
                dbCommandMock.Protected().Verify(
                    "ExecuteDbDataReaderAsync", Times.Once(), CommandBehavior.Default, It.IsAny<CancellationToken>());
                Assert.True(correctParameters);

                result.Dispose();

                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Once());
            }

            [Fact]
            public void DbDataReader_is_streaming_by_default()
            {
                var dbCommandMock = new Mock<DbCommand>();

                var storeDataReaderMock = new Mock<DbDataReader>();
                dbCommandMock.Protected()
                             .Setup<Task<DbDataReader>>(
                                 "ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                             .Returns(Task.FromResult(storeDataReaderMock.Object));

                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock
                    .Setup(m => m.GetEnumerator())
                    .Returns(new List<DbParameter>().GetEnumerator());

                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                objectContext.ExecuteStoreQueryAsync<object>("{0} Foo").Wait();

                dbCommandMock.Protected().Verify<Task<DbDataReader>>("ExecuteDbDataReaderAsync", Times.Once(), CommandBehavior.Default, It.IsAny<CancellationToken>());
                storeDataReaderMock.Verify(m => m.ReadAsync(It.IsAny<CancellationToken>()), Times.Never());
                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Never());
            }

            [Fact]
            public void DbDataReader_is_buffered_if_execution_strategy_is_used()
            {
                var dbCommandMock = new Mock<DbCommand>();

                var storeDataReaderMock = new Mock<DbDataReader>();
                storeDataReaderMock.Setup(m => m.ReadAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(false));
                storeDataReaderMock.Setup(m => m.NextResultAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(false));
                
                dbCommandMock.Protected()
                             .Setup<Task<DbDataReader>>(
                             "ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                             .Returns(Task.FromResult(storeDataReaderMock.Object));

                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock
                    .Setup(m => m.GetEnumerator())
                    .Returns(new List<DbParameter>().GetEnumerator());

                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);
                executionStrategyMock.Setup(
                    m => m.ExecuteAsync(It.IsAny<Func<Task<ObjectResult<object>>>>(), It.IsAny<CancellationToken>()))
                    .Returns<Func<Task<ObjectResult<object>>>, CancellationToken>((f, ct) => f());

                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
                try
                {
                    objectContext.ExecuteStoreQueryAsync<object>("{0} Foo").Wait();
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }

                dbCommandMock.Protected().Verify<Task<DbDataReader>>("ExecuteDbDataReaderAsync", Times.Once(), CommandBehavior.SequentialAccess, It.IsAny<CancellationToken>());
                storeDataReaderMock.Verify(m => m.ReadAsync(It.IsAny<CancellationToken>()), Times.Once());
                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Once());
            }

            [Fact]
            public void With_entitySet_DbDataReader_is_buffered_if_execution_strategy_is_used()
            {
                var dbCommandMock = new Mock<DbCommand>();

                var storeDataReaderMock = new Mock<DbDataReader>();
                storeDataReaderMock.Setup(m => m.ReadAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(false));
                storeDataReaderMock.Setup(m => m.NextResultAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(false));

                dbCommandMock.Protected()
                             .Setup<Task<DbDataReader>>(
                                 "ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                             .Returns(Task.FromResult(storeDataReaderMock.Object));

                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock
                    .Setup(m => m.GetEnumerator())
                    .Returns(new List<DbParameter>().GetEnumerator());

                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);
                executionStrategyMock.Setup(
                    m => m.ExecuteAsync(It.IsAny<Func<Task<ObjectResult<object>>>>(), It.IsAny<CancellationToken>()))
                    .Returns<Func<Task<ObjectResult<object>>>, CancellationToken>((f, ct) => f());

                SetupFooFunction(objectContext.MetadataWorkspace);

                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
                try
                {
                    objectContext.ExecuteStoreQueryAsync<object>("{0} Foo", "Bar.Foo", ExecutionOptions.Default).Wait();
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }

                dbCommandMock.Protected().Verify<Task<DbDataReader>>("ExecuteDbDataReaderAsync", Times.Once(), CommandBehavior.SequentialAccess, It.IsAny<CancellationToken>());
                storeDataReaderMock.Verify(m => m.ReadAsync(It.IsAny<CancellationToken>()), Times.Once());
                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Once());
            }

            [Fact]
            public void Throws_on_streaming_with_retrying_strategy()
            {
                var objectContext = CreateObjectContext(new Mock<DbCommand>().Object);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);

                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
                try
                {
                    Assert.Equal(
                        Strings.ExecutionStrategy_StreamingNotSupported(executionStrategyMock.Object.GetType().Name),
                        Assert.Throws<InvalidOperationException>(
                            () =>
                            objectContext.ExecuteStoreQueryAsync<object>(
                                "{0} Foo",
                                new ExecutionOptions(MergeOption.AppendOnly, streaming: true))).Message);
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }
            }

            [Fact]
            public void Connection_is_released_after_reader_exception()
            {
                var dbCommandMock = new Mock<DbCommand>();

                dbCommandMock.Protected().Setup<Task<DbDataReader>>(
                    "ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                             .Throws(new InvalidOperationException("Foo"));

                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock
                   .Setup(m => m.GetEnumerator())
                   .Returns(new List<DbParameter>().GetEnumerator());

                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                Assert.Equal(
                    "Foo",
                    Assert.Throws<InvalidOperationException>(
                        () => ExceptionHelpers.UnwrapAggregateExceptions(
                            () => objectContext.ExecuteStoreQueryAsync<object>("Bar").Wait())).Message);

                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Once());
            }

            [Fact]
            public void Connection_is_released_after_translator_exception()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var dataReader = Common.Internal.Materialization.MockHelper.CreateDbDataReader();

                dbCommandMock.Protected().Setup<Task<DbDataReader>>(
                    "ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                             .Returns(Task.FromResult(dataReader));

                var parameterCollectionMock = new Mock<DbParameterCollection>();
                parameterCollectionMock
                   .Setup(m => m.GetEnumerator())
                   .Returns(new List<DbParameter>().GetEnumerator());
                
                dbCommandMock.Protected().SetupGet<DbParameterCollection>("DbParameterCollection").Returns(
                    () => parameterCollectionMock.Object);

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                Assert.Equal(
                    new ArgumentOutOfRangeException(
                        typeof(MergeOption).Name, Strings.ADP_InvalidEnumerationValue(typeof(MergeOption).Name, 10)).Message,
                    Assert.Throws<ArgumentOutOfRangeException>(
                        () => ExceptionHelpers.UnwrapAggregateExceptions(
                            () =>
                            objectContext.ExecuteStoreQueryAsync<object>("Foo", new ExecutionOptions((MergeOption)10, streaming: false))
                                         .Wait())).Message);

                Mock.Get(objectContext).Verify(m => m.ReleaseConnection(), Times.Once());
                Mock.Get(dataReader).Protected().Verify("Dispose", Times.Once(), true);
            }

            [Fact]
            public void Executes_in_a_transaction_using_ExecutionStrategy()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var dataReader = Common.Internal.Materialization.MockHelper.CreateDbDataReader();

                dbCommandMock.Protected().Setup<Task<DbDataReader>>(
                    "ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                             .Returns(Task.FromResult(dataReader));

                var objectContextMock = Mock.Get(CreateObjectContext(dbCommandMock.Object));

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();

                // Verify that ExecuteInTransaction calls DbCommand.ExecuteDataReader
                objectContextMock.Setup(
                    m => m.ExecuteInTransactionAsync(
                        It.IsAny<Func<Task<ObjectResult<object>>>>(),
                        It.IsAny<IDbExecutionStrategy>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                                 .Returns<Func<Task<ObjectResult<object>>>, IDbExecutionStrategy, bool, bool, CancellationToken>(
                                     (f, t, s, r, c) =>
                                         {
                                             dbCommandMock.Protected().Verify<Task<DbDataReader>>(
                                                 "ExecuteDbDataReaderAsync", Times.Never(),
                                                 CommandBehavior.Default, ItExpr.IsAny<CancellationToken>());
                                             var result = f();
                                             dbCommandMock.Protected().Verify<Task<DbDataReader>>(
                                                 "ExecuteDbDataReaderAsync", Times.Once(),
                                                 CommandBehavior.Default, ItExpr.IsAny<CancellationToken>());
                                             return result;
                                         });

                // Verify that ExecutionStrategy.Execute calls ExecuteInTransaction
                executionStrategyMock.Setup(
                    m => m.ExecuteAsync(It.IsAny<Func<Task<ObjectResult<object>>>>(), It.IsAny<CancellationToken>()))
                                     .Returns<Func<Task<ObjectResult<object>>>, CancellationToken>(
                                         (f, c) =>
                                             {
                                                 objectContextMock.Verify(
                                                     m =>
                                                     m.ExecuteInTransactionAsync(
                                                         It.IsAny<Func<Task<ObjectResult<object>>>>(), It.IsAny<IDbExecutionStrategy>(), false,
                                                         It.IsAny<bool>(),
                                                         It.IsAny<CancellationToken>()),
                                                     Times.Never());
                                                 var result = f().Result;
                                                 objectContextMock.Verify(
                                                     m =>
                                                     m.ExecuteInTransactionAsync(
                                                         It.IsAny<Func<Task<ObjectResult<object>>>>(), It.IsAny<IDbExecutionStrategy>(), false,
                                                         It.IsAny<bool>(),
                                                         It.IsAny<CancellationToken>()),
                                                     Times.Once());
                                                 return Task.FromResult(result);
                                             });

                MutableResolver.AddResolver<Func<IDbExecutionStrategy>>(key => (Func<IDbExecutionStrategy>)(() => executionStrategyMock.Object));
                try
                {
                    Assert.NotNull(objectContextMock.Object.ExecuteStoreQueryAsync<object>("Foo").Result);
                }
                finally
                {
                    MutableResolver.ClearResolvers();
                }

                // Finally verify that ExecutionStrategy.Execute was called
                executionStrategyMock.Verify(
                    m => m.ExecuteAsync(It.IsAny<Func<Task<ObjectResult<object>>>>(), It.IsAny<CancellationToken>()), Times.Once());
            }
        }

        public class ExecuteInTransactionAsync : TestBase
        {
            [Fact]
            public void Throws_on_an_existing_local_transaction_when_throwOnExistingTransaction_is_true()
            {
                var dbCommandMock = new Mock<DbCommand>();

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var entityConnectionMock = Mock.Get((EntityConnection)objectContext.Connection);
                entityConnectionMock.Setup(m => m.CurrentTransaction).Returns(new Mock<EntityTransaction>().Object);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);

                var executed = false;
                Assert.Equal(
                    Strings.ExecutionStrategy_ExistingTransaction(executionStrategyMock.Object.GetType().Name),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        ExceptionHelpers.UnwrapAggregateExceptions(
                            () =>
                            objectContext.ExecuteInTransactionAsync(
                                () =>
                                    {
                                        executed = true;
                                        return Task.FromResult(executed);
                                    }, executionStrategyMock.Object, /*startLocalTransaction:*/ false,
                                /*releaseConnectionOnSuccess:*/ false, CancellationToken.None).Result)).Message);

                Assert.False(executed);
            }

            [Fact]
            public void Throws_on_a_connection_enlisted_in_a_user_transaction_when_throwOnExistingTransaction_is_true()
            {
                var dbCommandMock = new Mock<DbCommand>();

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var entityConnectionMock = Mock.Get((EntityConnection)objectContext.Connection);
                entityConnectionMock.Setup(m => m.EnlistedInUserTransaction).Returns(true);
                
                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);

                var executed = false;
                Assert.Equal(
                    Strings.ExecutionStrategy_ExistingTransaction(executionStrategyMock.Object.GetType().Name),
                    Assert.Throws<InvalidOperationException>(
                        () => ExceptionHelpers.UnwrapAggregateExceptions(
                            () =>
                            objectContext.ExecuteInTransactionAsync(
                                () =>
                                    {
                                        executed = true;
                                        return Task.FromResult(executed);
                                    }, executionStrategyMock.Object, /*startLocalTransaction:*/ false,
                                /*releaseConnectionOnSuccess:*/ false, CancellationToken.None).Result)).Message);

                Assert.False(executed);
            }

            [Fact]
            public void Throws_on_a_new_ambient_transaction_when_throwOnExistingTransaction_is_true()
            {
                var dbCommandMock = new Mock<DbCommand>();

                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);

                var executed = false;
                using (new TransactionScope())
                {
                    Assert.Equal(
                        Strings.ExecutionStrategy_ExistingTransaction(executionStrategyMock.Object.GetType().Name),
                        Assert.Throws<InvalidOperationException>(
                            () =>
                            ExceptionHelpers.UnwrapAggregateExceptions(
                                () =>
                                objectContext.ExecuteInTransactionAsync(
                                    () =>
                                        {
                                            executed = true;
                                            return Task.FromResult(executed);
                                        }, executionStrategyMock.Object, /*startLocalTransaction:*/ false,
                                    /*releaseConnectionOnSuccess:*/ false, CancellationToken.None).Result)).Message);
                }

                Assert.False(executed);
            }

            [Fact]
            public void Executes_in_an_existing_local_transaction_when_throwOnExistingTransaction_is_false()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var entityConnectionMock = Mock.Get((EntityConnection)objectContext.Connection);
                entityConnectionMock.Setup(m => m.CurrentTransaction).Returns(new Mock<EntityTransaction>().Object);
                
                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(false);

                var executed = false;

                Assert.True(
                    objectContext.ExecuteInTransactionAsync(
                        () =>
                            {
                                executed = true;
                                return Task.FromResult(executed);
                            }, executionStrategyMock.Object, /*startLocalTransaction:*/ true, false, CancellationToken.None).Result);

                Assert.True(executed);
                entityConnectionMock.Verify(m => m.BeginTransaction(), Times.Never());
            }

            [Fact]
            public void Executes_on_a_connection_enlisted_in_a_user_transaction_when_throwOnExistingTransaction_is_true()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var entityConnectionMock = Mock.Get((EntityConnection)objectContext.Connection);
                entityConnectionMock.Setup(m => m.EnlistedInUserTransaction).Returns(true);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(false);

                var executed = false;

                Assert.True(
                    objectContext.ExecuteInTransactionAsync(
                        () =>
                            {
                                executed = true;
                                return Task.FromResult(executed);
                            }, executionStrategyMock.Object, /*startLocalTransaction:*/ true, false, CancellationToken.None).Result
                    );

                Assert.True(executed);
                entityConnectionMock.Verify(m => m.BeginTransaction(), Times.Never());
            }

            [Fact]
            public void Executes_in_a_new_ambient_transaction_when_throwOnExistingTransaction_is_true()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var objectContext = CreateObjectContext(dbCommandMock.Object);

                var entityConnectionMock = Mock.Get((EntityConnection)objectContext.Connection);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(false);

                var executed = false;
                using (new TransactionScope())
                {
                    Assert.True(
                        objectContext.ExecuteInTransactionAsync(
                            () =>
                                {
                                    executed = true;
                                    return Task.FromResult(executed);
                                }, executionStrategyMock.Object, /*startLocalTransaction:*/ true, false, CancellationToken.None)
                                     .Result);
                }

                Assert.True(executed);
                entityConnectionMock.Verify(m => m.BeginTransaction(), Times.Never());
            }

            [Fact]
            public void Starts_a_new_local_transaction_when_throwOnExistingTransaction_is_true()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var objectContext = CreateObjectContext(dbCommandMock.Object);
                var objectContextMock = Mock.Get(objectContext);

                var entityConnectionMock = Mock.Get((EntityConnection)objectContext.Connection);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(true);

                var executed = false;
                Assert.True(
                    objectContext.ExecuteInTransactionAsync(
                        () =>
                            {
                                objectContextMock.Verify(m => m.EnsureConnectionAsync(It.IsAny<CancellationToken>()), Times.Once());
                                executed = true;
                                return Task.FromResult(executed);
                            }, executionStrategyMock.Object, /*startLocalTransaction:*/ true, false, CancellationToken.None).Result);

                Assert.True(executed);
                entityConnectionMock.Verify(m => m.BeginTransaction(), Times.Once());
                objectContextMock.Verify(m => m.ReleaseConnection(), Times.Never());
            }

            [Fact]
            public void Starts_a_new_local_transaction_when_throwOnExistingTransaction_is_false()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var objectContext = CreateObjectContext(dbCommandMock.Object);
                var objectContextMock = Mock.Get(objectContext);

                var entityConnectionMock = Mock.Get((EntityConnection)objectContext.Connection);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(false);

                var executed = false;
                Assert.True(
                    objectContext.ExecuteInTransactionAsync(
                        () =>
                            {
                                objectContextMock.Verify(m => m.EnsureConnectionAsync(It.IsAny<CancellationToken>()), Times.Once());
                                executed = true;
                                return Task.FromResult(executed);
                            }, executionStrategyMock.Object, /*startLocalTransaction:*/ true, false, CancellationToken.None).Result);

                Assert.True(executed);
                entityConnectionMock.Verify(m => m.BeginTransaction(), Times.Once());
                objectContextMock.Verify(m => m.ReleaseConnection(), Times.Never());
            }

            [Fact]
            public void Doesnt_start_a_new_local_transaction_when_startLocalTransaction_is_false()
            {
                var dbCommandMock = new Mock<DbCommand>();
                var objectContext = CreateObjectContext(dbCommandMock.Object);
                var objectContextMock = Mock.Get(objectContext);

                var entityConnectionMock = Mock.Get((EntityConnection)objectContext.Connection);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(false);

                var executed = false;
                Assert.True(
                    objectContext.ExecuteInTransactionAsync(
                        () =>
                            {
                                objectContextMock.Verify(m => m.EnsureConnectionAsync(It.IsAny<CancellationToken>()), Times.Once());
                                executed = true;
                                return Task.FromResult(executed);
                            }, executionStrategyMock.Object, /*startLocalTransaction:*/ false, false, CancellationToken.None)
                                 .Result);

                Assert.True(executed);
                entityConnectionMock.Verify(m => m.BeginTransaction(), Times.Never());
                objectContextMock.Verify(m => m.ReleaseConnection(), Times.Never());
            }

            [Fact]
            public void The_connection_is_released_if_an_exception_is_thrown_and_releaseConnectionOnSuccess_set_to_true()
            {
                The_connection_is_released_if_an_exception_is_thrown(true);
            }

            [Fact]
            public void The_connection_is_released_if_an_exception_is_thrown_and_releaseConnectionOnSuccess_set_to_false()
            {
                The_connection_is_released_if_an_exception_is_thrown(false);
            }

            private void The_connection_is_released_if_an_exception_is_thrown(bool releaseConnectionOnSuccess)
            {
                var dbCommandMock = new Mock<DbCommand>();
                var objectContext = CreateObjectContext(dbCommandMock.Object);
                var objectContextMock = Mock.Get(objectContext);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(false);

                var executed = false;
                Assert.Throws<NotImplementedException>(
                    () => ExceptionHelpers.UnwrapAggregateExceptions(
                        () =>
                        objectContext.ExecuteInTransactionAsync<object>(
                            () =>
                                {
                                    objectContextMock.Verify(m => m.EnsureConnectionAsync(It.IsAny<CancellationToken>()), Times.Once());
                                    executed = true;
                                    throw new NotImplementedException();
                                }, executionStrategyMock.Object, /*startLocalTransaction:*/ false, releaseConnectionOnSuccess,
                            CancellationToken.None).Result));

                Assert.True(executed);
                objectContextMock.Verify(m => m.ReleaseConnection(), Times.Once());
            }

            [Fact]
            public void The_connection_is_released_if_no_exception_is_thrown_and_releaseConnectionOnSuccess_set_to_true()
            {
                The_connection_is_released_if_no_exception_is_thrown(true);
            }

            [Fact]
            public void The_connection_is_not_released_if_no_exception_is_thrown_and_releaseConnectionOnSuccess_set_to_false()
            {
                The_connection_is_released_if_no_exception_is_thrown(false);
            }

            private void The_connection_is_released_if_no_exception_is_thrown(bool releaseConnectionOnSuccess)
            {
                var dbCommandMock = new Mock<DbCommand>();
                var objectContext = CreateObjectContext(dbCommandMock.Object);
                var objectContextMock = Mock.Get(objectContext);

                var executionStrategyMock = new Mock<IDbExecutionStrategy>();
                executionStrategyMock.Setup(m => m.RetriesOnFailure).Returns(false);

                var executed = false;
                objectContext.ExecuteInTransactionAsync(
                    () =>
                        {
                            objectContextMock.Verify(m => m.EnsureConnectionAsync(It.IsAny<CancellationToken>()), Times.Once());
                            executed = true;
                            return Task.FromResult<object>(null);
                        }, executionStrategyMock.Object, /*startLocalTransaction:*/ false, releaseConnectionOnSuccess,
                    CancellationToken.None).Wait();

                Assert.True(executed);
                objectContextMock.Verify(m => m.ReleaseConnection(), releaseConnectionOnSuccess ? Times.Once() : Times.Never());
            }
        }

#endif

        private static ObjectContext CreateObjectContext(DbCommand dbCommand = null)
        {
            var metadataWorkspaceMock = new Mock<MetadataWorkspace>
                {
                    CallBase = true
                };
            var edmItemCollection = new EdmItemCollection();
            var providerManifestMock = new Mock<DbProviderManifest>();
            providerManifestMock.Setup(m => m.GetStoreTypes()).Returns(new ReadOnlyCollection<PrimitiveType>(new List<PrimitiveType>()));
            providerManifestMock.Setup(m => m.GetStoreFunctions()).Returns(new ReadOnlyCollection<EdmFunction>(new List<EdmFunction>()));

            var storeItemCollection = new StoreItemCollection(
                FakeSqlProviderFactory.Instance, providerManifestMock.Object,
                GenericProviderFactory<DbProviderFactory>.Instance.InvariantProviderName, "2008");

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

            return CreateObjectContext(
                objectStateManagerMock: objectStateManagerMock, metadataWorkspaceMock: metadataWorkspaceMock, translator: translator,
                columnMapFactory: columnMapFactoryMock.Object, dbCommand: dbCommand);
        }

        private static ObjectContext CreateObjectContext(
            Mock<EntityConnection> entityConnectionMock = null, Mock<ObjectStateManager> objectStateManagerMock = null,
            Mock<MetadataWorkspace> metadataWorkspaceMock = null, ObjectQueryExecutionPlanFactory objectQueryExecutionPlanFactory = null,
            Translator translator = null, ColumnMapFactory columnMapFactory = null, DbCommand dbCommand = null)
        {
            if (objectStateManagerMock == null)
            {
                objectStateManagerMock = new Mock<ObjectStateManager>();
            }

            if (metadataWorkspaceMock == null)
            {
                metadataWorkspaceMock = new Mock<MetadataWorkspace>();
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.OSpace)).Returns(true);
                metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.OCSpace)).Returns(default(ItemCollection));
                metadataWorkspaceMock.Setup(m => m.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)).Returns(true);

                var storeItemCollection = new StoreItemCollection(
                    GenericProviderFactory<DbProviderFactory>.Instance, new SqlProviderManifest("2008"), "System.Data.FakeSqlClient", "2008");
                metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.SSpace)).Returns(storeItemCollection);
            }

            if (entityConnectionMock == null)
            {
                var dbConnectionMock = new Mock<DbConnection>();
                if (dbCommand != null)
                {
                    dbConnectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(() => dbCommand);
                }
                dbConnectionMock.Setup(m => m.DataSource).Returns("fakeDb");

                entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.ConnectionString).Returns("BarConnection");
                entityConnectionMock.SetupGet(m => m.State).Returns(() => ConnectionState.Open);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnectionMock.Object);
                entityConnectionMock.Setup(m => m.GetMetadataWorkspace()).Returns(metadataWorkspaceMock.Object);
            }

            var objectContextMock = new Mock<ObjectContext>(objectQueryExecutionPlanFactory, translator, columnMapFactory, null)
                {
                    CallBase = true
                };

            objectContextMock.Setup(m => m.Connection).Returns(entityConnectionMock.Object);
            objectContextMock.Setup(m => m.ObjectStateManager).Returns(() => objectStateManagerMock.Object);
            objectContextMock.Setup(m => m.MetadataWorkspace).Returns(() => metadataWorkspaceMock.Object);
            objectContextMock.Setup(m => m.DefaultContainerName).Returns("Bar");

            return objectContextMock.Object;
        }

        private static void SetupFooFunction(MetadataWorkspace metadataWorkspace)
        {
            var metadataWorkspaceMock = Mock.Get(metadataWorkspace);
            var entityType = (EdmType)new EntityType(
                                          "ReturnedEntity", "FooNamespace", DataSpace.CSpace,
                                          new[] { "key" }, new EdmMember[] { new EdmProperty("key") });
            var collectionTypeMock = new Mock<CollectionType>(entityType)
            {
                CallBase = true
            };
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

            entityContainer.AddEntitySetBase(new EntitySet("Foo", "", "", "", (EntityType)entityType));

            var edmItemCollection = (EdmItemCollection)metadataWorkspace.GetItemCollection(DataSpace.CSpace);
            var storeItemCollection = (StoreItemCollection)metadataWorkspace.GetItemCollection(DataSpace.SSpace);
            var containerMappingMock = new Mock<EntityContainerMapping>(entityContainer);
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
            storageMappingItemCollection.Setup(m => m.GetItems<EntityContainerMapping>())
                                        .Returns(
                                            new ReadOnlyCollection<EntityContainerMapping>(
                                                new List<EntityContainerMapping>
                                                        {
                                                            containerMappingMock.Object
                                                        }));

            metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.CSSpace, It.IsAny<bool>()))
                                 .Returns(storageMappingItemCollection.Object);
        }
    }
}
