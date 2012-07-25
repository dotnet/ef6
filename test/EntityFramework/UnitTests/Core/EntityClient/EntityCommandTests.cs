// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.EntityClient
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Query.ResultAssembly;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
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
                var entityConnection = MockHelper.InitializeEntityConnection();
                var passedEntityCommand = default(EntityCommand);
                var passedCommandbehavior = default(CommandBehavior);
                var storeDataReader = new Mock<DbDataReader>().Object;

                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(MockBehavior.Strict, null, null);
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
                var entityConnection = MockHelper.InitializeEntityConnection();
                var passedEntityCommand = default(EntityCommand);
                var passedCommandbehavior = default(CommandBehavior);
                var storeDataReader = new Mock<DbDataReader>().Object;

                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(MockBehavior.Strict, null, null);
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

        public class ExecuteReaderAsync
        {
            [Fact]
            public void Parameterless_ExecuteReader_calls_overload_with_CommandBehavior_Default()
            {
                var entityCommandMock = new Mock<EntityCommand>();
                entityCommandMock.Setup(m => m.ExecuteReaderAsync(CommandBehavior.Default, It.IsAny<CancellationToken>())).Returns(Task.FromResult(default(EntityDataReader)));
                entityCommandMock.Object.ExecuteReaderAsync().Wait();

                entityCommandMock.Verify(m => m.ExecuteReaderAsync(CommandBehavior.Default, It.IsAny<CancellationToken>()), Times.Once());
            }

            [Fact]
            public void Exception_thrown_if_EntityConnection_is_not_set()
            {
                var entityCommand = new EntityCommand(string.Empty, default(EntityConnection));

                AssertThrowsInAsyncMethod<InvalidOperationException>(
                    Strings.EntityClient_NoConnectionForCommand,
                    () => entityCommand.ExecuteReaderAsync(CommandBehavior.Default).Wait());
            }

            [Fact]
            public void Exception_thrown_if_EntityConnection_StoreProviderFactory_is_not_set()
            {
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.StoreProviderFactory).Returns(default(DbProviderFactory));

                var entityCommand = new EntityCommand(string.Empty, entityConnectionMock.Object);

                AssertThrowsInAsyncMethod<InvalidOperationException>(
                    Strings.EntityClient_ConnectionStringNeededBeforeOperation,
                    () => entityCommand.ExecuteReaderAsync(CommandBehavior.Default).Wait());
            }

            [Fact]
            public void Exception_thrown_if_EntityConnection_StoreConnection_is_not_set()
            {
                var providerFactory = new Mock<DbProviderFactory>(MockBehavior.Strict).Object;
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.StoreProviderFactory).Returns(providerFactory);
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(default(DbConnection));

                var entityCommand = new EntityCommand(string.Empty, entityConnectionMock.Object);

                AssertThrowsInAsyncMethod<InvalidOperationException>(
                    Strings.EntityClient_ConnectionStringNeededBeforeOperation,
                    () => entityCommand.ExecuteReaderAsync(CommandBehavior.Default).Wait());
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

                var entityCommand = new EntityCommand(string.Empty, entityConnectionMock.Object);

                AssertThrowsInAsyncMethod<InvalidOperationException>(
                    Strings.EntityClient_ExecutingOnClosedConnection(Strings.EntityClient_ConnectionStateClosed),
                    () => entityCommand.ExecuteReaderAsync(CommandBehavior.Default).Wait());
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

                var entityCommand = new EntityCommand(string.Empty, entityConnectionMock.Object);

                AssertThrowsInAsyncMethod<InvalidOperationException>(
                    Strings.EntityClient_ExecutingOnClosedConnection(Strings.EntityClient_ConnectionStateBroken),
                    () => entityCommand.ExecuteReaderAsync(CommandBehavior.Default).Wait());
            }

            [Fact]
            public void EntityCommandDefinition_is_executed_with_correct_EntityCommand_and_CommandBehavior()
            {
                var entityConnection = InitializeEntityConnection();
                var passedEntityCommand = default(EntityCommand);
                var passedCommandbehavior = default(CommandBehavior);
                var storeDataReader = new Mock<DbDataReader>().Object;

                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(MockBehavior.Strict, null, null);
                entityCommandDefinitionMock.SetupGet(m => m.Parameters).Returns(Enumerable.Empty<EntityParameter>());
                entityCommandDefinitionMock.Setup(m => m.ExecuteAsync(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>())).
                    Returns(Task.FromResult(storeDataReader)).
                    Callback((EntityCommand ec, CommandBehavior cb, CancellationToken ct) =>
                    {
                        passedEntityCommand = ec;
                        passedCommandbehavior = cb;
                    });

                var entityCommand = new EntityCommand(entityConnection, entityCommandDefinitionMock.Object);

                var commandBehavior = CommandBehavior.SequentialAccess;
                entityCommand.ExecuteReaderAsync(commandBehavior).Wait();

                Assert.Same(entityCommand, passedEntityCommand);
                Assert.Equal(passedCommandbehavior, commandBehavior);
                entityCommandDefinitionMock.Verify(m => m.ExecuteAsync(entityCommand, commandBehavior, CancellationToken.None), Times.Once());
            }

            [Fact]
            public void EntityDataReader_is_created_with_correct_EntityCommand_DbDataReader_and_CommandBehavior()
            {
                var entityConnection = InitializeEntityConnection();
                var passedEntityCommand = default(EntityCommand);
                var passedCommandbehavior = default(CommandBehavior);
                var storeDataReader = new Mock<DbDataReader>().Object;

                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(MockBehavior.Strict, null, null);
                entityCommandDefinitionMock.SetupGet(m => m.Parameters).Returns(Enumerable.Empty<EntityParameter>());
                entityCommandDefinitionMock.Setup(m => m.ExecuteAsync(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>())).
                    Returns(Task.FromResult(storeDataReader)).
                    Callback((EntityCommand ec, CommandBehavior cb, CancellationToken ct) =>
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

                var returnedReader = entityCommand.ExecuteReaderAsync(commandBehavior).Result;

                Assert.Same(entityDataReader, returnedReader);
                entityDataReaderFactoryMock.Verify(m => m.CreateEntityDataReader(entityCommand, storeDataReader, commandBehavior), Times.Once());
            }
        }

        public class ExecuteNonQuery
        {
            [Fact]
            public void Calls_ExecuteReader_with_CommandBehavior_set_to_SequentialAccess()
            {
                var entityConnection = MockHelper.InitializeEntityConnection();
                var entityCommandDefinition = MockHelper.InitializeEntityCommandDefinition();

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
                var entityConnection = MockHelper.InitializeEntityConnection();
                var entityCommandDefinition = MockHelper.InitializeEntityCommandDefinition();

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
                var entityConnection = MockHelper.InitializeEntityConnection();
                var entityCommandDefinition = MockHelper.InitializeEntityCommandDefinition();

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

        public class ExecuteNonQueryAsync
        {
            [Fact]
            public async void Calls_ExecuteReaderAsync_with_CommandBehavior_set_to_SequentialAccess()
            {
                var entityConnection = InitializeEntityConnection();
                var entityCommandDefinition = InitializeEntityCommandDefinition();

                int expectedRecordsAffected = 1;
                var entityDataReaderMock = new Mock<EntityDataReader>();
                entityDataReaderMock.Setup(m => m.NextResultAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(false));
                entityDataReaderMock.Setup(m => m.RecordsAffected).Returns(expectedRecordsAffected);
                var entityDataReaderFactoryMock = new Mock<EntityCommand.EntityDataReaderFactory>();
                entityDataReaderFactoryMock.Setup(m => m.CreateEntityDataReader(It.IsAny<EntityCommand>(), It.IsAny<DbDataReader>(), It.IsAny<CommandBehavior>())).
                    Returns(entityDataReaderMock.Object);

                var entityCommandMock = new Mock<EntityCommand>(
                    entityConnection, entityCommandDefinition, entityDataReaderFactoryMock.Object)
                                            {
                                                CallBase = true
                                            };

                int actualRecordsAffected = await entityCommandMock.Object.ExecuteNonQueryAsync();

                Assert.Equal(expectedRecordsAffected, actualRecordsAffected);
            }

            [Fact]
            public async void Iterates_over_all_results_of_EntityDataReader_without_touching_individual_rows()
            {
                var entityConnection = InitializeEntityConnection();
                var entityCommandDefinition = InitializeEntityCommandDefinition();

                int readNextCount = 0;

                var entityDataReaderMock = new Mock<EntityDataReader>();
                entityDataReaderMock.Setup(m => m.NextResultAsync(It.IsAny<CancellationToken>())).Callback((CancellationToken ct) => readNextCount++).Returns(() => Task.FromResult(readNextCount < 5));

                var entityDataReaderFactoryMock = new Mock<EntityCommand.EntityDataReaderFactory>();
                entityDataReaderFactoryMock.Setup(m => m.CreateEntityDataReader(It.IsAny<EntityCommand>(), It.IsAny<DbDataReader>(), It.IsAny<CommandBehavior>())).
                    Returns(entityDataReaderMock.Object);

                var entityCommand = new EntityCommand(entityConnection, entityCommandDefinition, entityDataReaderFactoryMock.Object);
                await entityCommand.ExecuteNonQueryAsync();

                entityDataReaderMock.Verify(m => m.NextResultAsync(It.IsAny<CancellationToken>()), Times.Exactly(5));
            }

            [Fact]
            public async void Returns_EntityDataReader_RecordsAffected_as_a_result()
            {
                var entityConnection = InitializeEntityConnection();
                var entityCommandDefinition = InitializeEntityCommandDefinition();

                int readNextCount = 0;

                var entityDataReaderMock = new Mock<EntityDataReader>();
                entityDataReaderMock.Setup(m => m.NextResultAsync(It.IsAny<CancellationToken>())).Callback(() => readNextCount++).Returns(() => Task.FromResult(readNextCount < 5));
                entityDataReaderMock.SetupGet(m => m.RecordsAffected).Returns(10);

                var entityDataReaderFactoryMock = new Mock<EntityCommand.EntityDataReaderFactory>();
                entityDataReaderFactoryMock.Setup(m => m.CreateEntityDataReader(It.IsAny<EntityCommand>(), It.IsAny<DbDataReader>(), It.IsAny<CommandBehavior>())).
                    Returns(entityDataReaderMock.Object);

                var entityCommand = new EntityCommand(entityConnection, entityCommandDefinition, entityDataReaderFactoryMock.Object);
                var result = await entityCommand.ExecuteNonQueryAsync();

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
            var storeDataReaderMock = new Mock<DbDataReader>();
            storeDataReaderMock.Setup(m => m.NextResultAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(false));
            var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(MockBehavior.Strict, null, null);
            entityCommandDefinitionMock.SetupGet(m => m.Parameters).Returns(Enumerable.Empty<EntityParameter>());
            entityCommandDefinitionMock.Setup(m => m.Execute(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>())).
                Returns(storeDataReaderMock.Object);
            entityCommandDefinitionMock.Setup(m => m.ExecuteAsync(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>())).
                Returns((EntityCommand ec, CommandBehavior cb, CancellationToken ct) => Task.FromResult(storeDataReaderMock.Object));

            return entityCommandDefinitionMock.Object;
        }

        private static void AssertThrowsInAsyncMethod<TException>(string expectedMessage, Xunit.Assert.ThrowsDelegate testCode)
            where TException : Exception
        {
            var exception = Assert.Throws<AggregateException>(testCode);
            var innerException = exception.InnerExceptions.Single();
            Assert.IsType<TException>(innerException);
            Assert.Equal(expectedMessage, innerException.Message);
        }
    }
}
