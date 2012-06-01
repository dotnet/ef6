namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Metadata.Internal;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Linq.Expressions;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class ObjectContextTests
    {
        public class DelegationToInternalClass
        {
            [Fact]
            public void Properties_delegate_to_internal_class_correctly()
            {
                VerifyGetter(c => c.Connection, m => m.Connection);
                VerifyGetter(c => c.DefaultContainerName, m => m.DefaultContainerName);
                VerifySetter(c => c.DefaultContainerName = default(string), m => m.DefaultContainerName = It.IsAny<string>());
                VerifyGetter(c => c.MetadataWorkspace, m => m.MetadataWorkspace);
                VerifyGetter(c => c.ObjectStateManager, m => m.ObjectStateManager);
                VerifyGetter(c => c.Perspective, m => m.Perspective);
                VerifyGetter(c => c.CommandTimeout, m => m.CommandTimeout);
                VerifySetter(c => c.CommandTimeout = default(int?), m => m.CommandTimeout = It.IsAny<int?>());
                VerifyGetter(c => c.InMaterialization, m => m.InMaterialization);
                VerifySetter(c => c.InMaterialization = default(bool), m => m.InMaterialization = It.IsAny<bool>());
                VerifyGetter(c => c.ContextOptions, m => m.ContextOptions);
                VerifyGetter(c => c.ColumnMapBuilder, m => m.ColumnMapBuilder);
                VerifySetter(c => c.ColumnMapBuilder = default(CollectionColumnMap), m => m.ColumnMapBuilder = It.IsAny<CollectionColumnMap>());
            }

            [Fact]
            public void Methods_delegate_to_internal_class_correctly()
            {
                VerifyMethod(
                    c => c.AddObject(default(string), new object()),
                    m => m.AddObject(default(string), It.IsAny<object>()));
                var entitySetMock = new Mock<EntitySet>();
                var wrappedEntityMock = new Mock<IEntityWrapper>();
                wrappedEntityMock.SetupGet(m => m.Entity).Returns(new object());
                VerifyMethod(
                    c => c.AddSingleObject(entitySetMock.Object, wrappedEntityMock.Object, default(string)),
                    m => m.AddSingleObject(entitySetMock.Object, wrappedEntityMock.Object, It.IsAny<string>()));
                VerifyMethod(
                    c => c.LoadProperty(default(object), default(string)), 
                    m => m.LoadProperty(It.IsAny<object>(), It.IsAny<string>()));
                VerifyMethod(
                    c => c.LoadProperty(default(object), default(string), default(MergeOption)),
                    m => m.LoadProperty(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<MergeOption>()));
                VerifyMethod(
                    c => c.LoadProperty<DummyEntity>(default(DummyEntity), default(Expression<Func<DummyEntity, object>>)),
                    m => m.LoadProperty<DummyEntity>(It.IsAny<DummyEntity>(), It.IsAny<Expression<Func<DummyEntity, object>>>()));
                VerifyMethod(
                    c => c.LoadProperty<DummyEntity>(default(DummyEntity), default(Expression<Func<DummyEntity, object>>), default(MergeOption)),
                    m => m.LoadProperty<DummyEntity>(It.IsAny<DummyEntity>(), It.IsAny<Expression<Func<DummyEntity, object>>>(), It.IsAny<MergeOption>()));
                VerifyMethod(
                    c => c.ApplyPropertyChanges("Foo", new DummyEntity()),
                    m => m.ApplyCurrentValues<object>("Foo", It.IsAny<DummyEntity>()));
                VerifyMethod(
                    c => c.ApplyCurrentValues<DummyEntity>("Foo", new DummyEntity()),
                    m => m.ApplyCurrentValues<DummyEntity>("Foo", It.IsAny<DummyEntity>()));
                VerifyMethod(
                    c => c.ApplyOriginalValues<DummyEntity>(default(string), new DummyEntity()),
                    m => m.ApplyOriginalValues<DummyEntity>(default(string), It.IsAny<DummyEntity>()));
                var entityWithKeyMock = new Mock<IEntityWithKey>();
                entityWithKeyMock.Setup(m => m.EntityKey).Returns(new EntityKey());
                VerifyMethod(
                    c => c.Attach(entityWithKeyMock.Object),
                    m => m.AttachTo(default(string), entityWithKeyMock.Object));
                VerifyMethod(
                    c => c.AttachTo(default(string), new object()),
                    m => m.AttachTo(default(string), It.IsAny<object>()));
                VerifyMethod(
                    c => c.AttachSingleObject(wrappedEntityMock.Object, entitySetMock.Object),
                    m => m.AttachSingleObject(wrappedEntityMock.Object, entitySetMock.Object));
                VerifyMethod(
                    c => c.CreateEntityKey("Foo", new object()),
                    m => m.CreateEntityKey("Foo", It.IsAny<object>()));
                VerifyMethod(
                    c => c.GetEntitySetFromName(default(string)),
                    m => m.GetEntitySetFromName(default(string)));
                VerifyMethod(
                    c => c.CreateObjectSet<DummyEntity>(), 
                    m => m.CreateObjectSet<DummyEntity>());
                VerifyMethod(
                    c => c.CreateObjectSet<DummyEntity>(default(string)),
                    m => m.CreateObjectSet<DummyEntity>(default(string)));
                VerifyMethod(c => c.EnsureConnection(), m => m.EnsureConnection());
                VerifyMethod(c => c.ReleaseConnection(), m => m.ReleaseConnection());
                VerifyMethod(c => c.EnsureMetadata(), m => m.EnsureMetadata());
                VerifyMethod(
                    c => c.CreateQuery<DummyEntity>("Foo", new ObjectParameter[0]),
                    m => m.CreateQuery<DummyEntity>("Foo", It.IsAny<ObjectParameter[]>()));
                VerifyMethod(
                    c => c.DeleteObject(default(object)),
                    m => m.DeleteObject(default(object)));
                VerifyMethod(
                    c => c.DeleteObject(new object(), default(EntitySet)),
                    m => m.DeleteObject(It.IsAny<object>(), default(EntitySet)));
                VerifyMethod(
                    c => c.Detach(default(object)),
                    m => m.Detach(default(object)));
                VerifyMethod(
                    c => c.Detach(new object(), default(EntitySet)),
                    m => m.Detach(It.IsAny<object>(), default(EntitySet)));
                VerifyMethod(
                    c => c.GetEntitySet("Foo", default(string)),
                    m => m.GetEntitySet("Foo", default(string)));
                VerifyMethod(
                    c => c.GetTypeUsage(default(Type)),
                    m => m.GetTypeUsage(default(Type)));
                VerifyMethod(
                    c => c.GetObjectByKey(new EntityKey()),
                    m => m.GetObjectByKey(It.IsAny<EntityKey>()));
                VerifyMethod(
                    c => c.Refresh(default(RefreshMode), Enumerable.Empty<object>()),
                    m => m.Refresh(default(RefreshMode), Enumerable.Empty<object>()));
                VerifyMethod(
                    c => c.Refresh(default(RefreshMode), new object()),
                    m => m.Refresh(default(RefreshMode), It.IsAny<object>()));
                VerifyMethod(
                    c => c.SaveChanges(default(SaveOptions)),
                    m => m.SaveChanges(default(SaveOptions)));
                VerifyMethod(
                    c => c.ExecuteFunction<DummyEntity>("Foo", new ObjectParameter[0]),
                    m => m.ExecuteFunction<DummyEntity>("Foo", MergeOption.AppendOnly, It.IsAny<ObjectParameter[]>()));
                VerifyMethod(
                    c => c.ExecuteFunction<DummyEntity>("Foo", default(MergeOption), new ObjectParameter[0]),
                    m => m.ExecuteFunction<DummyEntity>("Foo", default(MergeOption), It.IsAny<ObjectParameter[]>()));
                VerifyMethod(
                    c => c.ExecuteFunction("Foo", new ObjectParameter[0]),
                    m => m.ExecuteFunction("Foo", It.IsAny<ObjectParameter[]>()));
                VerifyMethod(
                     c => c.MaterializedDataRecord<DummyEntity>(
                         default(EntityCommand),
                         default(DbDataReader),
                         default(int),
                         default(ReadOnlyMetadataCollection<EntitySet>),
                         default(EdmType[]),
                         default(MergeOption)),
                     m => m.MaterializedDataRecord<DummyEntity>(
                         default(EntityCommand),
                         default(DbDataReader),
                         default(int),
                         default(ReadOnlyMetadataCollection<EntitySet>),
                         default(EdmType[]),
                         default(MergeOption)));
                VerifyMethod(
                    c => c.CreateProxyTypes(default(IEnumerable<Type>)),
                    m => m.CreateProxyTypes(default(IEnumerable<Type>)));
                VerifyMethod(
                    c => c.CreateObject<DummyEntity>(),
                    m => m.CreateObject<DummyEntity>());
                VerifyMethod(
                    c => c.ExecuteStoreCommand(default(string), default(object[])),
                    m => m.ExecuteStoreCommand(default(string), default(object[])));
                VerifyMethod(
                    c => c.ExecuteStoreQuery<DummyEntity>(default(string), default(object[])),
                    m => m.ExecuteStoreQuery<DummyEntity>(default(string), default(string), MergeOption.AppendOnly, default(object[])));
                VerifyMethod(
                    c => c.ExecuteStoreQuery<DummyEntity>(default(string), "Foo", default(MergeOption), default(object[])),
                    m => m.ExecuteStoreQuery<DummyEntity>(default(string), "Foo", default(MergeOption), default(object[])));
                VerifyMethod(
                    c => c.Translate<DummyEntity>(default(DbDataReader)),
                    m => m.Translate<DummyEntity>(default(DbDataReader)));
                VerifyMethod(
                    c => c.Translate<DummyEntity>(default(DbDataReader), "Foo", default(MergeOption)),
                    m => m.Translate<DummyEntity>(default(DbDataReader), "Foo", default(MergeOption)));
                VerifyMethod(c => c.CreateDatabase(), m => m.CreateDatabase());
                VerifyMethod(c => c.DeleteDatabase(), m => m.DeleteDatabase());
                VerifyMethod(c => c.DatabaseExists(), m => m.DatabaseExists());
                VerifyMethod(c => c.CreateDatabaseScript(), m => m.CreateDatabaseScript());
            }

            [Fact]
            public void TryGetObjectByKey_delegate_to_internal_class_correctly()
            {
                var internalObjectContextMock = new Mock<InternalObjectContext>(
                    null, /*connection*/
                    false, /*isConnectionConstructor*/
                    true, /*skipInitializeConnection*/
                    true, /*skipInitializeWorkspace*/
                    true /*skipInitializeContextOptions*/);

                object value;
                object internalValue = "Foo";
                internalObjectContextMock.Setup(m => m.TryGetObjectByKey(It.IsAny<EntityKey>(), out internalValue)).Returns(true);

                var objectContext = new ObjectContext(internalObjectContextMock.Object);
                var result = objectContext.TryGetObjectByKey(default(EntityKey), out value);

                Assert.Equal(true, result);
                Assert.Equal("Foo", value);
            }

            private void VerifyGetter<TProperty>(
                Func<ObjectContext, TProperty> getterFunc,
                Expression<Func<InternalObjectContext, TProperty>> mockGetterFunc)
            {
                Assert.NotNull(getterFunc);
                Assert.NotNull(mockGetterFunc);

                var internalObjectContextMock = new Mock<InternalObjectContext>(
                    null, /*connection*/
                    false, /*isConnectionConstructor*/
                    true, /*skipInitializeConnection*/
                    true, /*skipInitializeWorkspace*/
                    true /*skipInitializeContextOptions*/);

                var objectContext = new ObjectContext(internalObjectContextMock.Object);

                getterFunc(objectContext);
                internalObjectContextMock.VerifyGet(mockGetterFunc, Times.Once());
            }

            private void VerifySetter<TProperty>(
                Func<ObjectContext, TProperty> setter,
                Action<InternalObjectContext> mockSetter)
            {
                Assert.NotNull(setter);
                Assert.NotNull(mockSetter);

                var internalObjectContextMock = new Mock<InternalObjectContext>(
                    null, /*connection*/
                    false, /*isConnectionConstructor*/
                    true, /*skipInitializeConnection*/
                    true, /*skipInitializeWorkspace*/
                    true /*skipInitializeContextOptions*/);

                var objectContext = new ObjectContext(internalObjectContextMock.Object);

                setter(objectContext);
                internalObjectContextMock.VerifySet(m => mockSetter(m), Times.Once());
            }

            private void VerifyMethod(
                Action<ObjectContext> methodInvoke,
                Expression<Action<InternalObjectContext>> mockMethodInvoke)
            {
                Assert.NotNull(methodInvoke);
                Assert.NotNull(mockMethodInvoke);

                var internalObjectContextMock = new Mock<InternalObjectContext>(
                    null, /*connection*/
                    false, /*isConnectionConstructor*/
                    true, /*skipInitializeConnection*/
                    true, /*skipInitializeWorkspace*/
                    true /*skipInitializeContextOptions*/);

                var objectContext = new ObjectContext(internalObjectContextMock.Object);

                methodInvoke(objectContext);
                internalObjectContextMock.Verify(mockMethodInvoke, Times.Once());
            }

            private class DummyEntity
            {
            }
        }

        public class SaveChanges
        {
            [Fact]
            public void Parameterless_SaveChanges_calls_SaveOption_flags_to_DetectChangesBeforeSave_and_AcceptAllChangesAfterSave()
            {
                var internalObjectContextMock = new Mock<InternalObjectContext>(
                    null /*entityConnection*/, 
                    false, /*isConnectionConstructor*/
                    true, /*skipInitializeConnection*/
                    true, /*skipInitializeWorkspace*/
                    true /*skipInitializeContextOptions*/);

                var objectContex = new ObjectContext(internalObjectContextMock.Object);

                objectContex.SaveChanges();
                var expectedSavedOptions = SaveOptions.DetectChangesBeforeSave | SaveOptions.AcceptAllChangesAfterSave;
                internalObjectContextMock.Verify(m => m.SaveChanges(expectedSavedOptions), Times.Once());
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

                var objectContext = ObjectContextInitializationForExecuteStoreCommand(dbCommandMock, dbConnectionMock, internalEntityConnectionMock);
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

                var objectContext = ObjectContextInitializationForExecuteStoreCommand(dbCommandMock, dbConnectionMock, internalEntityConnectionMock);
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

                var objectContext = ObjectContextInitializationForExecuteStoreCommand(dbCommandMock, dbConnectionMock, internalEntityConnectionMock);
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

                var objectContext = ObjectContextInitializationForExecuteStoreCommand(dbCommandMock, dbConnectionMock, internalEntityConnectionMock);
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

                var objectContext = ObjectContextInitializationForExecuteStoreCommand(dbCommandMock, dbConnectionMock, internalEntityConnectionMock);
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

                var objectContext = ObjectContextInitializationForExecuteStoreCommand(dbCommandMock, dbConnectionMock, internalEntityConnectionMock);

                Assert.Equal(
                    Strings.ObjectContext_ExecuteCommandWithMixOfDbParameterAndValues,
                    Assert.Throws<InvalidOperationException>(() => objectContext.ExecuteStoreCommand("Foo", 1, new Mock<DbParameter>().Object)).Message);
            }

            private static ObjectContext ObjectContextInitializationForExecuteStoreCommand(
                Mock<DbCommand> dbCommandMock,
                Mock<DbConnection> dbConnectionMock,
                Mock<InternalEntityConnection> internalEntityConnectionMock)
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
            var internalObjectContextMock = new Mock<InternalObjectContext>(
                entityConnection,
                true,
                false, /*skipInitializeConnection*/
                true, /*skipInitializeWorkspace*/
                true /*skipInitializeContextOptions*/)
            {
                CallBase = true
            };

            internalObjectContextMock.SetupGet(m => m.ObjectStateManager).Returns(() => new ObjectStateManager(internalObjectStateManagerMock.Object));

            var objectContext = new ObjectContext(internalObjectContextMock.Object);

            return objectContext;
        }

        private static ObjectContext BasicObjectContextInitializationWithConnectionAndMetadata(
            Mock<InternalObjectStateManager> internalObjectStateManagerMock,
            Mock<InternalEntityConnection> internalEntityConnectionMock,
            Mock<InternalMetadataWorkspace> internalMetadataWorkspace)
        {
            var entityConnection = new EntityConnection(internalEntityConnectionMock.Object);
            internalEntityConnectionMock.Setup(m => m.GetMetadataWorkspace(false)).Returns(() => new MetadataWorkspace(internalMetadataWorkspace.Object));

            var internalObjectContextMock = new Mock<InternalObjectContext>(
                entityConnection,
                true,
                false, /*skipInitializeConnection*/
                false, /*skipInitializeWorkspace*/
                true /*skipInitializeContextOptions*/)
            {
                CallBase = true
            };

            internalObjectContextMock.SetupGet(m => m.ObjectStateManager).Returns(() => new ObjectStateManager(internalObjectStateManagerMock.Object));

            var objectContext = new ObjectContext(internalObjectContextMock.Object);

            return objectContext;
        }
    }
}
