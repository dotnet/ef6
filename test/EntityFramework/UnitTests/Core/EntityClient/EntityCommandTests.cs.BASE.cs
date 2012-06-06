namespace System.Data.Entity.Core.EntityClient
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Linq.Expressions;
    using Moq;
    using Xunit;

    public class EntityCommandTests
    {
        public class DelegationToInternalClass
        {
            [Fact]
            public void Properties_delegate_to_internal_class_correctly()
            {
                VerifyGetter(c => c.Connection, m => m.Connection);
                VerifySetter(c => c.Connection = default(EntityConnection), m => m.Connection = It.IsAny<EntityConnection>());

                VerifyGetter(c => c.CommandText, m => m.CommandText);
                VerifySetter(c => c.CommandText = default(string), m => m.CommandText = It.IsAny<string>());

                VerifyGetter(c => c.CommandTree, m => m.CommandTree);
                VerifySetter(c => c.CommandTree = default(DbCommandTree), m => m.CommandTree = It.IsAny<DbCommandTree>());

                VerifyGetter(c => c.CommandTimeout, m => m.CommandTimeout);
                VerifySetter(c => c.CommandTimeout = default(int), m => m.CommandTimeout = It.IsAny<int>());

                VerifyGetter(c => c.CommandType, m => m.CommandType);
                VerifySetter(c => c.CommandType = default(CommandType), m => m.CommandType = It.IsAny<CommandType>());

                VerifyGetter(c => c.Parameters, m => m.Parameters);

                VerifyGetter(c => c.Transaction, m => m.Transaction);
                VerifySetter(c => c.Transaction = default(EntityTransaction), m => m.Transaction = It.IsAny<EntityTransaction>());

                VerifyGetter(c => c.UpdatedRowSource, m => m.UpdatedRowSource);
                VerifySetter(c => c.UpdatedRowSource = default(UpdateRowSource), m => m.UpdatedRowSource = It.IsAny<UpdateRowSource>());

                VerifyGetter(c => c.DesignTimeVisible, m => m.DesignTimeVisible);
                VerifySetter(c => c.DesignTimeVisible = default(bool), m => m.DesignTimeVisible = It.IsAny<bool>());

                VerifyGetter(c => c.EnablePlanCaching, m => m.EnablePlanCaching);
                VerifySetter(c => c.EnablePlanCaching = default(bool), m => m.EnablePlanCaching = It.IsAny<bool>());
            }

            [Fact]
            public void Methods_delegate_to_internal_class_correctly()
            {
                VerifyMethod(c => c.ExecuteReader(default(CommandBehavior)), m => m.ExecuteReader(It.IsAny<CommandBehavior>()));
                VerifyMethod(c => c.ExecuteNonQuery(), m => m.ExecuteNonQuery());
                VerifyMethod(c => c.ExecuteScalar(), m => m.ExecuteScalar());
                VerifyMethod(c => c.Unprepare(), m => m.Unprepare());
                VerifyMethod(c => c.Prepare(), m => m.Prepare());
                VerifyMethod(c => c.GetCommandDefinition(), m => m.GetCommandDefinition());
                VerifyMethod(c => c.ToTraceString(), m => m.ToTraceString());
                VerifyMethod(c => c.GetParameterTypeUsage(), m => m.GetParameterTypeUsage());
                VerifyMethod(c => c.NotifyDataReaderClosing(), m => m.NotifyDataReaderClosing());
                VerifyMethod(c => c.SetStoreProviderCommand(default(DbCommand)), m => m.SetStoreProviderCommand(It.IsAny<DbCommand>()));
            }

            private void VerifyGetter<TProperty>(
                Func<EntityCommand, TProperty> getterFunc,
                Expression<Func<InternalEntityCommand, TProperty>> mockGetterFunc)
            {
                Assert.NotNull(getterFunc);
                Assert.NotNull(mockGetterFunc);

                var internalCommandMock = new Mock<InternalEntityCommand>(null);
                var command = new EntityCommand(internalCommandMock.Object);

                getterFunc(command);
                internalCommandMock.VerifyGet(mockGetterFunc, Times.Once());
            }

            private void VerifySetter<TProperty>(
                Func<EntityCommand, TProperty> setter,
                Action<InternalEntityCommand> mockSetter)
            {
                Assert.NotNull(setter);
                Assert.NotNull(mockSetter);

                var internalCommandMock = new Mock<InternalEntityCommand>(null);
                var command = new EntityCommand(internalCommandMock.Object);

                setter(command);
                internalCommandMock.VerifySet(m => mockSetter(m), Times.Once());
            }

            private void VerifyMethod(
                Action<EntityCommand> methodInvoke,
                Expression<Action<InternalEntityCommand>> mockMethodInvoke)
            {
                Assert.NotNull(methodInvoke);
                Assert.NotNull(mockMethodInvoke);

                var internalCommandMock = new Mock<InternalEntityCommand>(null);
                var command = new EntityCommand(internalCommandMock.Object);

                methodInvoke(command);
                internalCommandMock.Verify(mockMethodInvoke, Times.Once());
            }
        }

        public class ExecuteReader
        {
            [Fact]
            public void Parameterless_ExecuteReader_calls_overload_with_CommandBehavior_Default()
            {
                var internalEntityCommandMock = new Mock<InternalEntityCommand>(null);
                var entityCommand = new EntityCommand(internalEntityCommandMock.Object);
                entityCommand.ExecuteReader();

                internalEntityCommandMock.Verify(m => m.ExecuteReader(CommandBehavior.Default), Times.Once());
            }

            [Fact]
            public void Exception_thrown_if_EntityConnection_is_not_set()
            {
                var internalEntityCommand = new InternalEntityCommand(string.Empty, default(EntityConnection));
                var entityCommand = new EntityCommand(internalEntityCommand);
                
                Assert.Equal(
                    Strings.EntityClient_NoConnectionForCommand, 
                    Assert.Throws<InvalidOperationException>(() => entityCommand.ExecuteReader(CommandBehavior.Default)).Message);
            }

            [Fact]
            public void Exception_thrown_if_EntityConnection_StoreProviderFactory_is_not_set()
            {
                var internalEntityConnectionMock = new Mock<InternalEntityConnection>();
                internalEntityConnectionMock.SetupGet(m => m.StoreProviderFactory).Returns(default(DbProviderFactory));
                var entityConnection = new EntityConnection(internalEntityConnectionMock.Object);

                var internalEntityCommand = new InternalEntityCommand(string.Empty, entityConnection);
                var entityCommand = new EntityCommand(internalEntityCommand);

                Assert.Equal(
                    Strings.EntityClient_ConnectionStringNeededBeforeOperation,
                    Assert.Throws<InvalidOperationException>(() => entityCommand.ExecuteReader(CommandBehavior.Default)).Message);
            }

            [Fact]
            public void Exception_thrown_if_EntityConnection_StoreConnection_is_not_set()
            {
                var providerFactory = new Mock<DbProviderFactory>(MockBehavior.Strict).Object;
                var internalEntityConnectionMock = new Mock<InternalEntityConnection>();
                internalEntityConnectionMock.SetupGet(m => m.StoreProviderFactory).Returns(providerFactory);
                internalEntityConnectionMock.SetupGet(m => m.StoreConnection).Returns(default(DbConnection));
                var entityConnection = new EntityConnection(internalEntityConnectionMock.Object);

                var internalEntityCommand = new InternalEntityCommand(string.Empty, entityConnection);
                var entityCommand = new EntityCommand(internalEntityCommand);

                Assert.Equal(
                    Strings.EntityClient_ConnectionStringNeededBeforeOperation,
                    Assert.Throws<InvalidOperationException>(() => entityCommand.ExecuteReader(CommandBehavior.Default)).Message);
            }

            [Fact]
            public void Exception_thrown_if_EntityConnection_State_is_Closed()
            {
                var providerFactory = new Mock<DbProviderFactory>(MockBehavior.Strict).Object;
                var dbConnection = new Mock<DbConnection>(MockBehavior.Strict).Object;
                var internalEntityConnectionMock = new Mock<InternalEntityConnection>();
                internalEntityConnectionMock.SetupGet(m => m.StoreProviderFactory).Returns(providerFactory);
                internalEntityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnection);
                internalEntityConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Closed);
                var entityConnection = new EntityConnection(internalEntityConnectionMock.Object);

                var internalEntityCommand = new InternalEntityCommand(string.Empty, entityConnection);
                var entityCommand = new EntityCommand(internalEntityCommand);

                Assert.Equal(
                    Strings.EntityClient_ExecutingOnClosedConnection(Strings.EntityClient_ConnectionStateClosed),
                    Assert.Throws<InvalidOperationException>(() => entityCommand.ExecuteReader(CommandBehavior.Default)).Message);
            }

            [Fact]
            public void Exception_thrown_if_EntityConnection_State_is_Broken()
            {
                var providerFactory = new Mock<DbProviderFactory>(MockBehavior.Strict).Object;
                var dbConnection = new Mock<DbConnection>(MockBehavior.Strict).Object;
                var internalEntityConnectionMock = new Mock<InternalEntityConnection>();
                internalEntityConnectionMock.SetupGet(m => m.StoreProviderFactory).Returns(providerFactory);
                internalEntityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnection);
                internalEntityConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Broken);
                var entityConnection = new EntityConnection(internalEntityConnectionMock.Object);

                var internalEntityCommand = new InternalEntityCommand(string.Empty, entityConnection);
                var entityCommand = new EntityCommand(internalEntityCommand);

                Assert.Equal(
                    Strings.EntityClient_ExecutingOnClosedConnection(Strings.EntityClient_ConnectionStateBroken),
                    Assert.Throws<InvalidOperationException>(() => entityCommand.ExecuteReader(CommandBehavior.Default)).Message);
            }

            [Fact]
            public void EntityCommandDefinition_is_executed_with_correct_EntityCommand_and_CommandBehavior()
            {
                var entityConnection = InitializeEntityConnection();
                var passedEntityCommand = default(EntityCommand);
                var passedCommandbehavior = default(CommandBehavior);
                var storeDataReader = new Mock<DbDataReader>().Object;

                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(MockBehavior.Strict);
                entityCommandDefinitionMock.SetupGet(m => m.Parameters).Returns(Enumerable.Empty<EntityParameter>());
                entityCommandDefinitionMock.Setup(m => m.Execute(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>())).
                    Returns(storeDataReader).
                    Callback((EntityCommand ec, CommandBehavior cb) =>
                        {
                            passedEntityCommand = ec;
                            passedCommandbehavior = cb;
                        });

                var internalEntityCommand = new InternalEntityCommand(entityConnection, entityCommandDefinitionMock.Object);
                var entityCommand = new EntityCommand(internalEntityCommand);

                var commandBehavior = CommandBehavior.SequentialAccess;
                entityCommand.ExecuteReader(commandBehavior);

                Assert.Same(entityCommand, passedEntityCommand);
                Assert.Equal(passedCommandbehavior, commandBehavior);
                entityCommandDefinitionMock.Verify(m => m.Execute(entityCommand, commandBehavior), Times.Once());
            }

            [Fact]
            public void EntityDataReader_is_created_with_correct_EntityCommand_DbDataReader_and_CommandBehavior()
            {
                var entityConnection = InitializeEntityConnection();
                var passedEntityCommand = default(EntityCommand);
                var passedCommandbehavior = default(CommandBehavior);
                var storeDataReader = new Mock<DbDataReader>().Object;

                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(MockBehavior.Strict);
                entityCommandDefinitionMock.SetupGet(m => m.Parameters).Returns(Enumerable.Empty<EntityParameter>());
                entityCommandDefinitionMock.Setup(m => m.Execute(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>())).
                    Returns(storeDataReader).
                    Callback((EntityCommand ec, CommandBehavior cb) =>
                    {
                        passedEntityCommand = ec;
                        passedCommandbehavior = cb;
                    });

                var entityDataReaderFactoryMock = new Mock<System.Data.Entity.Core.EntityClient.Internal.InternalEntityCommand.EntityDataReaderFactory>();

                var internalEntityCommand = new InternalEntityCommand(entityConnection, entityCommandDefinitionMock.Object, entityDataReaderFactoryMock.Object);
                var entityCommand = new EntityCommand(internalEntityCommand);
                var commandBehavior = CommandBehavior.SequentialAccess;

                var entityDataReader = new EntityDataReader(entityCommand, storeDataReader, commandBehavior);
                entityDataReaderFactoryMock.Setup(m => m.CreateEntityDataReader(It.IsAny<EntityCommand>(), It.IsAny<DbDataReader>(), It.IsAny<CommandBehavior>())).
                    Returns(entityDataReader);

                var returnedReader = entityCommand.ExecuteReader(commandBehavior);

                Assert.Same(entityDataReader, returnedReader);
                entityDataReaderFactoryMock.Verify(m => m.CreateEntityDataReader(entityCommand, storeDataReader, commandBehavior), Times.Once());
            }
        }

        public class ExecuteNonQuery
        {
            [Fact]
            public void Calls_ExecuteReader_with_CommandBehavior_set_to_SequentialAccess()
            {
                var entityConnection = InitializeEntityConnection();
                var entityCommandDefinition = InitializeEntityCommandDefinition();

                var entityDataReaderMock = new Mock<EntityDataReader>();
                var entityDataReaderFactoryMock = new Mock<System.Data.Entity.Core.EntityClient.Internal.InternalEntityCommand.EntityDataReaderFactory>();
                entityDataReaderFactoryMock.Setup(m => m.CreateEntityDataReader(It.IsAny<EntityCommand>(), It.IsAny<DbDataReader>(), It.IsAny<CommandBehavior>())).
                    Returns(entityDataReaderMock.Object);

                var internalEntityCommand = new InternalEntityCommand(entityConnection, entityCommandDefinition, entityDataReaderFactoryMock.Object);
                var entityCommand = new EntityCommand(internalEntityCommand);
                entityCommand.ExecuteNonQuery();
            }

            [Fact]
            public void Iterates_over_all_results_of_EntityDataReader_without_touching_individual_rows()
            {
                var entityConnection = InitializeEntityConnection();
                var entityCommandDefinition = InitializeEntityCommandDefinition();

                int readNextCount = 0;

                var entityDataReaderMock = new Mock<EntityDataReader>();
                entityDataReaderMock.Setup(m => m.NextResult()).Callback(() => readNextCount++).Returns(() => readNextCount < 5);

                var entityDataReaderFactoryMock = new Mock<System.Data.Entity.Core.EntityClient.Internal.InternalEntityCommand.EntityDataReaderFactory>();
                entityDataReaderFactoryMock.Setup(m => m.CreateEntityDataReader(It.IsAny<EntityCommand>(), It.IsAny<DbDataReader>(), It.IsAny<CommandBehavior>())).
                    Returns(entityDataReaderMock.Object);

                var internalEntityCommand = new InternalEntityCommand(entityConnection, entityCommandDefinition, entityDataReaderFactoryMock.Object);
                var entityCommand = new EntityCommand(internalEntityCommand);
                entityCommand.ExecuteNonQuery();

                entityDataReaderMock.Verify(m => m.NextResult(), Times.Exactly(5));
            }

            [Fact]
            public void Returns_EntityDataReader_RecordsAffected_as_a_result()
            {
                var entityConnection = InitializeEntityConnection();
                var entityCommandDefinition = InitializeEntityCommandDefinition();

                int readNextCount = 0;

                var entityDataReaderMock = new Mock<EntityDataReader>();
                entityDataReaderMock.Setup(m => m.NextResult()).Callback(() => readNextCount++).Returns(() => readNextCount < 5);
                entityDataReaderMock.SetupGet(m => m.RecordsAffected).Returns(10);

                var entityDataReaderFactoryMock = new Mock<System.Data.Entity.Core.EntityClient.Internal.InternalEntityCommand.EntityDataReaderFactory>();
                entityDataReaderFactoryMock.Setup(m => m.CreateEntityDataReader(It.IsAny<EntityCommand>(), It.IsAny<DbDataReader>(), It.IsAny<CommandBehavior>())).
                    Returns(entityDataReaderMock.Object);

                var internalEntityCommand = new InternalEntityCommand(entityConnection, entityCommandDefinition, entityDataReaderFactoryMock.Object);
                var entityCommand = new EntityCommand(internalEntityCommand);
                var result = entityCommand.ExecuteNonQuery();

                Assert.Equal(10, result);
            }
        }

        private static EntityConnection InitializeEntityConnection()
        {
            var providerFactory = new Mock<DbProviderFactory>(MockBehavior.Strict).Object;
            var dbConnection = new Mock<DbConnection>(MockBehavior.Strict).Object;

            var internalEntityConnectionMock = new Mock<InternalEntityConnection>();
            internalEntityConnectionMock.SetupGet(m => m.StoreProviderFactory).Returns(providerFactory);
            internalEntityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnection);
            internalEntityConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);
            var entityConnection = new EntityConnection(internalEntityConnectionMock.Object);

            return entityConnection;
        }

        private static EntityCommandDefinition InitializeEntityCommandDefinition()
        {
            var storeDataReader = new Mock<DbDataReader>().Object;
            var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(MockBehavior.Strict);
            entityCommandDefinitionMock.SetupGet(m => m.Parameters).Returns(Enumerable.Empty<EntityParameter>());
            entityCommandDefinitionMock.Setup(m => m.Execute(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>())).
                Returns(storeDataReader);

            return entityCommandDefinitionMock.Object;
        }
    }
}
