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
        public class ExecuteReader
        {
            [Fact]
            public void Parameterless_ExecuteReader_calls_overload_with_CommandBehavior_Default()
            {
                var entityCommandMock = new Mock<EntityCommand> { CallBase = true };
                entityCommandMock.Setup(m => m.ExecuteReader(It.IsAny<CommandBehavior>()));
                var entityCommand = entityCommandMock.Object;
                entityCommand.ExecuteReader();

                entityCommandMock.Verify(m => m.ExecuteReader(CommandBehavior.Default), Times.Once());
            }

            [Fact]
            public void Exception_thrown_if_EntityConnection_is_not_set()
            {
                var entityCommand = new EntityCommand(string.Empty, default(EntityConnection));
                
                Assert.Equal(
                    Strings.EntityClient_NoConnectionForCommand, 
                    Assert.Throws<InvalidOperationException>(() => entityCommand.ExecuteReader(CommandBehavior.Default)).Message);
            }

            [Fact]
            public void Exception_thrown_if_EntityConnection_StoreProviderFactory_is_not_set()
            {
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.StoreProviderFactory).Returns(default(DbProviderFactory));
                var entityConnection = entityConnectionMock.Object;

                var entityCommand = new EntityCommand(string.Empty, entityConnection);

                Assert.Equal(
                    Strings.EntityClient_ConnectionStringNeededBeforeOperation,
                    Assert.Throws<InvalidOperationException>(() => entityCommand.ExecuteReader(CommandBehavior.Default)).Message);
            }

            [Fact]
            public void Exception_thrown_if_EntityConnection_StoreConnection_is_not_set()
            {
                var providerFactory = new Mock<DbProviderFactory>(MockBehavior.Strict).Object;
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.StoreProviderFactory).Returns(providerFactory);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(default(DbConnection));
                var entityConnection = entityConnectionMock.Object;

                var entityCommand = new EntityCommand(string.Empty, entityConnection);

                Assert.Equal(
                    Strings.EntityClient_ConnectionStringNeededBeforeOperation,
                    Assert.Throws<InvalidOperationException>(() => entityCommand.ExecuteReader(CommandBehavior.Default)).Message);
            }

            [Fact]
            public void Exception_thrown_if_EntityConnection_State_is_Closed()
            {
                var providerFactory = new Mock<DbProviderFactory>(MockBehavior.Strict).Object;
                var dbConnection = new Mock<DbConnection>(MockBehavior.Strict).Object;
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.StoreProviderFactory).Returns(providerFactory);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnection);
                entityConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Closed);
                var entityConnection = entityConnectionMock.Object;

                var entityCommand = new EntityCommand(string.Empty, entityConnection);

                Assert.Equal(
                    Strings.EntityClient_ExecutingOnClosedConnection(Strings.EntityClient_ConnectionStateClosed),
                    Assert.Throws<InvalidOperationException>(() => entityCommand.ExecuteReader(CommandBehavior.Default)).Message);
            }

            [Fact]
            public void Exception_thrown_if_EntityConnection_State_is_Broken()
            {
                var providerFactory = new Mock<DbProviderFactory>(MockBehavior.Strict).Object;
                var dbConnection = new Mock<DbConnection>(MockBehavior.Strict).Object;
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.StoreProviderFactory).Returns(providerFactory);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnection);
                entityConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Broken);
                var entityConnection = entityConnectionMock.Object;

                var entityCommand = new EntityCommand(string.Empty, entityConnection);

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

                var entityCommand = new EntityCommand(entityConnection, entityCommandDefinitionMock.Object);

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

                var entityDataReaderFactoryMock = new Mock<EntityCommand.EntityDataReaderFactory>();

                var entityCommand = new EntityCommand(entityConnection, entityCommandDefinitionMock.Object, entityDataReaderFactoryMock.Object);
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
                var entityDataReaderFactoryMock = new Mock<EntityCommand.EntityDataReaderFactory>();
                entityDataReaderFactoryMock.Setup(m => m.CreateEntityDataReader(It.IsAny<EntityCommand>(), It.IsAny<DbDataReader>(), It.IsAny<CommandBehavior>())).
                    Returns(entityDataReaderMock.Object);

                var entityCommand = new EntityCommand(entityConnection, entityCommandDefinition, entityDataReaderFactoryMock.Object);
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

                var entityDataReaderFactoryMock = new Mock<EntityCommand.EntityDataReaderFactory>();
                entityDataReaderFactoryMock.Setup(m => m.CreateEntityDataReader(It.IsAny<EntityCommand>(), It.IsAny<DbDataReader>(), It.IsAny<CommandBehavior>())).
                    Returns(entityDataReaderMock.Object);

                var entityCommand = new EntityCommand(entityConnection, entityCommandDefinition, entityDataReaderFactoryMock.Object);
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

                var entityDataReaderFactoryMock = new Mock<EntityCommand.EntityDataReaderFactory>();
                entityDataReaderFactoryMock.Setup(m => m.CreateEntityDataReader(It.IsAny<EntityCommand>(), It.IsAny<DbDataReader>(), It.IsAny<CommandBehavior>())).
                    Returns(entityDataReaderMock.Object);

                var entityCommand = new EntityCommand(entityConnection, entityCommandDefinition, entityDataReaderFactoryMock.Object);
                var result = entityCommand.ExecuteNonQuery();

                Assert.Equal(10, result);
            }
        }

        private static EntityConnection InitializeEntityConnection()
        {
            var providerFactory = new Mock<DbProviderFactory>(MockBehavior.Strict).Object;
            var dbConnection = new Mock<DbConnection>(MockBehavior.Strict).Object;

            var entityConnectionMock = new Mock<EntityConnection>();
            entityConnectionMock.SetupGet(m => m.StoreProviderFactory).Returns(providerFactory);
            entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnection);
            entityConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Open);
            var entityConnection = entityConnectionMock.Object;

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
