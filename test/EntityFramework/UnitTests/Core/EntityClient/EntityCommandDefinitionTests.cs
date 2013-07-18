// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityClient
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Data.Entity.Core.Query.ResultAssembly;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class EntityCommandDefinitionTests
    {
        public class Execute
        {
            [Fact]
            public void Exception_thrown_if_CommandBehavior_is_not_SequentialAccess()
            {
                var entityCommandDefinition = new Mock<EntityCommandDefinition>(null, null, null)
                    {
                        CallBase = true
                    }.Object;

                Assert.Equal(
                    Strings.ADP_MustUseSequentialAccess,
                    Assert.Throws<InvalidOperationException>(
                        () => entityCommandDefinition.Execute(default(EntityCommand), CommandBehavior.Default)).Message);
            }

            [Fact]
            public void Returns_null_if_nothing_was_executed_and_DbDataReader_is_null()
            {
                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(null, null, null)
                    {
                        CallBase = true
                    };
                entityCommandDefinitionMock.Setup(m => m.ExecuteStoreCommands(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>())).
                                            Returns(default(DbDataReader));

                var entityCommandDefinition = entityCommandDefinitionMock.Object;
                var result = entityCommandDefinition.Execute(default(EntityCommand), CommandBehavior.SequentialAccess);

                Assert.Equal(null, result);
            }

            [Fact]
            public void Reader_consumed_and_returned_for_query_without_a_result_type()
            {
                var dbDataReaderMock = new Mock<DbDataReader>();
                dbDataReaderMock.SetupGet(m => m.IsClosed).Returns(false);
                dbDataReaderMock.Setup(m => m.NextResult()).Returns(false);
                var dbDataReader = dbDataReaderMock.Object;

                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(null, null, null)
                    {
                        CallBase = true
                    };
                entityCommandDefinitionMock.Setup(m => m.ExecuteStoreCommands(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>())).
                                            Returns(dbDataReader);

                entityCommandDefinitionMock.Setup(m => m.CreateColumnMap(It.IsAny<DbDataReader>(), It.IsAny<int>())).
                                            Returns(default(ColumnMap));

                var entityCommandDefinition = entityCommandDefinitionMock.Object;
                var result = entityCommandDefinition.Execute(default(EntityCommand), CommandBehavior.SequentialAccess);

                Assert.Same(dbDataReader, result);
                dbDataReaderMock.VerifyGet(m => m.IsClosed, Times.Once());
                dbDataReaderMock.Verify(m => m.NextResult(), Times.Once());
            }

            [Fact]
            public void New_bridge_DataReader_created_for_query_with_result_type()
            {
                var dbDataReaderMock = new Mock<DbDataReader>();
                dbDataReaderMock.SetupGet(m => m.IsClosed).Returns(false);
                dbDataReaderMock.Setup(m => m.NextResult()).Returns(false);

                var typeUsageMock = new Mock<TypeUsage>();
                var columnMapMock = new Mock<ColumnMap>(typeUsageMock.Object, "Foo");

                var bridgeDataReader = new Mock<DbDataReader>().Object;
                var bridgeDataReaderFactoryMock = new Mock<BridgeDataReaderFactory>(MockBehavior.Loose, null);
                bridgeDataReaderFactoryMock.Setup(
                    m =>
                    m.Create(
                        It.IsAny<DbDataReader>(), It.IsAny<ColumnMap>(), It.IsAny<MetadataWorkspace>(), It.IsAny<IEnumerable<ColumnMap>>()))
                                           .
                                            Returns(bridgeDataReader);

                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(bridgeDataReaderFactoryMock.Object, null, null)
                    {
                        CallBase = true
                    };
                entityCommandDefinitionMock.Setup(m => m.ExecuteStoreCommands(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>())).
                                            Returns(dbDataReaderMock.Object);

                entityCommandDefinitionMock.Setup(m => m.CreateColumnMap(It.IsAny<DbDataReader>(), It.IsAny<int>())).
                                            Returns(columnMapMock.Object);

                var entityConnectionMock
                    = new Mock<EntityConnection>(
                        null /*workspace*/,
                        null /*connection*/,
                        true /*skipInitialization*/,
                        true /*entityConnectionOwnsStoreConnection*/,
                        null);

                var entityCommandMock = new Mock<EntityCommand>();
                entityCommandMock.SetupGet(m => m.Connection).Returns(entityConnectionMock.Object);

                var result = entityCommandDefinitionMock.Object.Execute(entityCommandMock.Object, CommandBehavior.SequentialAccess);

                Assert.Same(bridgeDataReader, result);
            }

            [Fact]
            public void StoreDataReader_is_disposed_properly_if_exception_is_thrown()
            {
                var dbDataReaderMock = new Mock<DbDataReader>();
                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(null, null, null)
                    {
                        CallBase = true
                    };
                entityCommandDefinitionMock.Setup(m => m.ExecuteStoreCommands(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>())).
                                            Returns(dbDataReaderMock.Object);
                entityCommandDefinitionMock.Setup(m => m.CreateColumnMap(It.IsAny<DbDataReader>(), It.IsAny<int>())).Throws
                    <InvalidOperationException>();

                var entityCommandDefinition = entityCommandDefinitionMock.Object;

                Assert.Throws<InvalidOperationException>(
                    () => entityCommandDefinition.Execute(default(EntityCommand), CommandBehavior.SequentialAccess));

                dbDataReaderMock.Protected().Verify("Dispose", Times.Once(), true);
            }
        }

#if !NET40

        public class ExecuteAsync
        {
            [Fact]
            public void Exception_thrown_if_CommandBehavior_is_not_SequentialAccess()
            {
                var entityCommandDefinition = new Mock<EntityCommandDefinition>(null, null, null)
                    {
                        CallBase = true
                    }.Object;

                AssertThrowsInAsyncMethod<InvalidOperationException>(
                    Strings.ADP_MustUseSequentialAccess,
                    () =>
                    entityCommandDefinition.ExecuteAsync(default(EntityCommand), CommandBehavior.Default, CancellationToken.None).Wait());
            }

            [Fact]
            public void Returns_null_if_nothing_was_executed_and_DbDataReader_is_null()
            {
                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(null, null, null)
                    {
                        CallBase = true
                    };
                entityCommandDefinitionMock.Setup(
                    m => m.ExecuteStoreCommandsAsync(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
                                           .Returns(Task.FromResult(default(DbDataReader)));

                var entityCommandDefinition = entityCommandDefinitionMock.Object;
                var result =
                    entityCommandDefinition.ExecuteAsync(default(EntityCommand), CommandBehavior.SequentialAccess, CancellationToken.None)
                                           .Result;

                Assert.Equal(null, result);
            }

            [Fact]
            public void Reader_consumed_and_returned_for_query_without_a_result_type()
            {
                var dbDataReaderMock = new Mock<DbDataReader>();
                dbDataReaderMock.SetupGet(m => m.IsClosed).Returns(false);
                dbDataReaderMock.Setup(m => m.NextResultAsync(CancellationToken.None)).Returns(Task.FromResult(false));
                var dbDataReader = dbDataReaderMock.Object;

                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(null, null, null)
                    {
                        CallBase = true
                    };
                entityCommandDefinitionMock.Setup(
                    m => m.ExecuteStoreCommandsAsync(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
                                           .Returns(Task.FromResult(dbDataReader));

                entityCommandDefinitionMock.Setup(m => m.CreateColumnMap(It.IsAny<DbDataReader>(), It.IsAny<int>())).
                                            Returns(default(ColumnMap));

                var entityCommandDefinition = entityCommandDefinitionMock.Object;
                var result =
                    entityCommandDefinition.ExecuteAsync(default(EntityCommand), CommandBehavior.SequentialAccess, CancellationToken.None)
                                           .Result;

                Assert.Same(dbDataReader, result);
                dbDataReaderMock.VerifyGet(m => m.IsClosed, Times.Once());
                dbDataReaderMock.Verify(m => m.NextResultAsync(It.IsAny<CancellationToken>()), Times.Once());
            }

            [Fact]
            public void New_bridge_DataReader_created_for_query_with_result_type()
            {
                var dbDataReaderMock = new Mock<DbDataReader>();
                dbDataReaderMock.SetupGet(m => m.IsClosed).Returns(false);
                dbDataReaderMock.Setup(m => m.NextResultAsync(CancellationToken.None)).Returns(Task.FromResult(false));
                var dbDataReader = dbDataReaderMock.Object;

                var typeUsageMock = new Mock<TypeUsage>();
                var columnMapMock = new Mock<ColumnMap>(typeUsageMock.Object, "Foo");

                var bridgeDataReader = new Mock<DbDataReader>().Object;
                var bridgeDataReaderFactoryMock = new Mock<BridgeDataReaderFactory>(MockBehavior.Loose, null);
                bridgeDataReaderFactoryMock.Setup(
                    m =>
                    m.Create(
                        It.IsAny<DbDataReader>(), It.IsAny<ColumnMap>(), It.IsAny<MetadataWorkspace>(), It.IsAny<IEnumerable<ColumnMap>>()))
                                           .
                                            Returns(bridgeDataReader);

                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(bridgeDataReaderFactoryMock.Object, null, null)
                    {
                        CallBase = true
                    };
                entityCommandDefinitionMock.Setup(
                    m => m.ExecuteStoreCommandsAsync(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
                                           .
                                            Returns(Task.FromResult(dbDataReader));

                entityCommandDefinitionMock.Setup(m => m.CreateColumnMap(It.IsAny<DbDataReader>(), It.IsAny<int>())).
                                            Returns(columnMapMock.Object);

                var metadataWorkspaceMock = new Mock<MetadataWorkspace>();

                var entityConnectionMock
                    = new Mock<EntityConnection>(
                        null /*workspace*/,
                        null /*connection*/,
                        true /*skipInitialization*/,
                        true /*entityConnectionOwnsStoreConnection*/,
                        null);

                entityConnectionMock.Setup(m => m.GetMetadataWorkspace()).Returns(metadataWorkspaceMock.Object);

                var entityCommandMock = new Mock<EntityCommand>();
                entityCommandMock.SetupGet(m => m.Connection).Returns(entityConnectionMock.Object);

                var entityCommandDefinition = entityCommandDefinitionMock.Object;
                var result =
                    entityCommandDefinition.ExecuteAsync(entityCommandMock.Object, CommandBehavior.SequentialAccess, CancellationToken.None)
                                           .Result;

                Assert.Same(bridgeDataReader, result);
            }

            [Fact]
            public void StoreDataReader_is_disposed_properly_if_exception_is_thrown()
            {
                var dbDataReaderMock = new Mock<DbDataReader>();
                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(null, null, null)
                    {
                        CallBase = true
                    };
                entityCommandDefinitionMock.Setup(
                    m => m.ExecuteStoreCommandsAsync(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
                                           .
                                            Returns(Task.FromResult(dbDataReaderMock.Object));
                entityCommandDefinitionMock.Setup(m => m.CreateColumnMap(It.IsAny<DbDataReader>(), It.IsAny<int>())).Throws
                    <InvalidOperationException>();

                var entityCommandDefinition = entityCommandDefinitionMock.Object;

                AssertThrowsInAsyncMethod<InvalidOperationException>(
                    null,
                    () =>
                    entityCommandDefinition.ExecuteAsync(default(EntityCommand), CommandBehavior.SequentialAccess, CancellationToken.None).
                                            Wait());

                dbDataReaderMock.Protected().Verify("Dispose", Times.Once(), true);
            }
        }

#endif

        public class ExecuteStoreCommands
        {
            [Fact]
            public void MARS_is_not_supported()
            {
                var entityCommandMock = new Mock<EntityCommand>();
                entityCommandMock.Setup(m => m.InterceptionContext).Returns(new DbInterceptionContext());

                var dbCommandDefinitionMock = new Mock<DbCommandDefinition>();
                var mappedCommandDefinitions = new List<DbCommandDefinition>
                    {
                        dbCommandDefinitionMock.Object,
                        dbCommandDefinitionMock.Object
                    };

                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(null, null, mappedCommandDefinitions)
                    {
                        CallBase = true
                    };
                var entityCommandDefinition = entityCommandDefinitionMock.Object;

                Assert.Equal(
                    "MARS",
                    Assert.Throws<NotSupportedException>(
                        () => entityCommandDefinition.ExecuteStoreCommands(entityCommandMock.Object, CommandBehavior.Default)).Message);
            }

            [Fact]
            public void Executes_reader_from_store_provider_command_and_returns_it()
            {
                var entityCommand = InitializeEntityCommand();

                var dbDataReader = new Mock<DbDataReader>().Object;
                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.Protected().Setup<DbDataReader>("ExecuteDbDataReader", It.IsAny<CommandBehavior>()).Returns(dbDataReader);
                var dbCommandDefinitionMock = new Mock<DbCommandDefinition>();
                dbCommandDefinitionMock.Setup(m => m.CreateCommand()).Returns(dbCommandMock.Object);
                var mappedCommandDefinitions = new List<DbCommandDefinition>
                    {
                        dbCommandDefinitionMock.Object
                    };

                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(null, null, mappedCommandDefinitions)
                    {
                        CallBase = true
                    };

                var mockInterceptor = new Mock<DbCommandInterceptor>
                    {
                        CallBase = true
                    };
                DbInterception.Add(mockInterceptor.Object);

                try
                {
                    Assert.Same(
                        dbDataReader, entityCommandDefinitionMock.Object.ExecuteStoreCommands(entityCommand, CommandBehavior.Default));
                }
                finally
                {
                    DbInterception.Remove(mockInterceptor.Object);
                }

                dbCommandMock.Protected().Verify("ExecuteDbDataReader", Times.Once(), CommandBehavior.Default);
                mockInterceptor.Verify(
                    m => m.ReaderExecuting(dbCommandMock.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()));
                mockInterceptor.Verify(
                    m => m.ReaderExecuted(dbCommandMock.Object, It.IsAny<DbCommandInterceptionContext<DbDataReader>>()));
            }

            [Fact]
            public void CommandBehavior_passed_to_ExecuteReader_is_set_to_Default_if_CommandBehavior_passed_to_Execute_is_SequentialAccess()
            {
                var entityCommand = InitializeEntityCommand();

                var dbDataReader = new Mock<DbDataReader>().Object;
                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.Protected().Setup<DbDataReader>("ExecuteDbDataReader", It.IsAny<CommandBehavior>()).Returns(dbDataReader);
                var dbCommandDefinitionMock = new Mock<DbCommandDefinition>();
                dbCommandDefinitionMock.Setup(m => m.CreateCommand()).Returns(dbCommandMock.Object);
                var mappedCommandDefinitions = new List<DbCommandDefinition>
                    {
                        dbCommandDefinitionMock.Object
                    };

                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(null, null, mappedCommandDefinitions)
                    {
                        CallBase = true
                    };
                var entityCommandDefinition = entityCommandDefinitionMock.Object;

                entityCommandDefinition.ExecuteStoreCommands(entityCommand, CommandBehavior.SequentialAccess);
                dbCommandMock.Protected().Verify("ExecuteDbDataReader", Times.Once(), CommandBehavior.Default);
            }

            [Fact]
            public void Exception_is_wrapped_properly_if_ExecuteReader_fails()
            {
                var entityCommand = InitializeEntityCommand();

                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.Protected().Setup<DbDataReader>("ExecuteDbDataReader", It.IsAny<CommandBehavior>()).Throws
                    <InvalidOperationException>();
                var dbCommandDefinitionMock = new Mock<DbCommandDefinition>();
                dbCommandDefinitionMock.Setup(m => m.CreateCommand()).Returns(dbCommandMock.Object);
                var mappedCommandDefinitions = new List<DbCommandDefinition>
                    {
                        dbCommandDefinitionMock.Object
                    };

                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(null, null, mappedCommandDefinitions)
                    {
                        CallBase = true
                    };
                var entityCommandDefinition = entityCommandDefinitionMock.Object;

                Assert.Equal(
                    Strings.EntityClient_CommandDefinitionExecutionFailed,
                    Assert.Throws<EntityCommandExecutionException>(
                        () => entityCommandDefinition.ExecuteStoreCommands(entityCommand, CommandBehavior.Default)).Message);
            }

            private static EntityCommand InitializeEntityCommand()
            {
                var entityCommandMock = new Mock<EntityCommand>();
                entityCommandMock.Setup(m => m.ValidateAndGetEntityTransaction()).Returns(default(EntityTransaction));
                entityCommandMock.SetupGet(m => m.Connection).Returns(new Mock<EntityConnection>().Object);
                entityCommandMock.SetupGet(m => m.InterceptionContext).Returns(new DbInterceptionContext());
                return entityCommandMock.Object;
            }
        }

#if !NET40

        public class ExecuteStoreCommandsAsync
        {
            [Fact]
            public void MARS_is_not_supported()
            {
                var entityCommandMock = new Mock<EntityCommand>();

                var dbCommandDefinitionMock = new Mock<DbCommandDefinition>();
                var mappedCommandDefinitions = new List<DbCommandDefinition>
                    {
                        dbCommandDefinitionMock.Object,
                        dbCommandDefinitionMock.Object
                    };

                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(null, null, mappedCommandDefinitions)
                    {
                        CallBase = true
                    };
                var entityCommandDefinition = entityCommandDefinitionMock.Object;

                AssertThrowsInAsyncMethod<NotSupportedException>(
                    "MARS", () =>
                            entityCommandDefinition.ExecuteStoreCommandsAsync(
                                entityCommandMock.Object, CommandBehavior.Default, CancellationToken.None).Wait());
            }

            [Fact]
            public void Executes_reader_from_store_provider_command_and_returns_it()
            {
                var entityCommand = InitializeEntityCommand();

                var dbDataReader = new Mock<DbDataReader>().Object;
                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.Protected().Setup<Task<DbDataReader>>(
                    "ExecuteDbDataReaderAsync", It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()).Returns(
                        Task.FromResult(dbDataReader));
                var dbCommandDefinitionMock = new Mock<DbCommandDefinition>();
                dbCommandDefinitionMock.Setup(m => m.CreateCommand()).Returns(dbCommandMock.Object);
                var mappedCommandDefinitions = new List<DbCommandDefinition>
                    {
                        dbCommandDefinitionMock.Object
                    };

                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(null, null, mappedCommandDefinitions)
                    {
                        CallBase = true
                    };
                var entityCommandDefinition = entityCommandDefinitionMock.Object;

                var result =
                    entityCommandDefinition.ExecuteStoreCommandsAsync(entityCommand, CommandBehavior.Default, CancellationToken.None).Result;

                dbCommandMock.Protected().Verify(
                    "ExecuteDbDataReaderAsync", Times.Once(), CommandBehavior.Default, It.IsAny<CancellationToken>());
                Assert.Same(dbDataReader, result);
            }

            [Fact]
            public void
                CommandBehavior_passed_to_ExecuteReader_is_set_to_Default_if_CommandBehavior_passed_to_Execute_is_SequentialAccess()
            {
                var entityCommand = InitializeEntityCommand();

                var dbDataReader = new Mock<DbDataReader>().Object;
                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.Protected().Setup<Task<DbDataReader>>(
                    "ExecuteDbDataReaderAsync", It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()).Returns(
                        Task.FromResult(dbDataReader));
                var dbCommandDefinitionMock = new Mock<DbCommandDefinition>();
                dbCommandDefinitionMock.Setup(m => m.CreateCommand()).Returns(dbCommandMock.Object);
                var mappedCommandDefinitions = new List<DbCommandDefinition>
                    {
                        dbCommandDefinitionMock.Object
                    };

                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(null, null, mappedCommandDefinitions)
                    {
                        CallBase = true
                    };
                var entityCommandDefinition = entityCommandDefinitionMock.Object;

                entityCommandDefinition.ExecuteStoreCommandsAsync(
                    entityCommand, CommandBehavior.SequentialAccess, CancellationToken.None).Wait();

                dbCommandMock.Protected().Verify(
                    "ExecuteDbDataReaderAsync", Times.Once(), CommandBehavior.Default, It.IsAny<CancellationToken>());
            }

            [Fact]
            public void Exception_is_wrapped_properly_if_ExecuteReader_fails()
            {
                var entityCommand = InitializeEntityCommand();

                var dbCommandMock = new Mock<DbCommand>();
                dbCommandMock.Protected().Setup<Task<DbDataReader>>(
                    "ExecuteDbDataReaderAsync", It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()).Throws
                    (new AggregateException(new InvalidOperationException()));
                var dbCommandDefinitionMock = new Mock<DbCommandDefinition>();
                dbCommandDefinitionMock.Setup(m => m.CreateCommand()).Returns(dbCommandMock.Object);
                var mappedCommandDefinitions = new List<DbCommandDefinition>
                    {
                        dbCommandDefinitionMock.Object
                    };

                var entityCommandDefinitionMock = new Mock<EntityCommandDefinition>(null, null, mappedCommandDefinitions)
                    {
                        CallBase = true
                    };
                var entityCommandDefinition = entityCommandDefinitionMock.Object;

                AssertThrowsInAsyncMethod<EntityCommandExecutionException>(
                    Strings.EntityClient_CommandDefinitionExecutionFailed,
                    () =>
                    entityCommandDefinition.ExecuteStoreCommandsAsync(entityCommand, CommandBehavior.Default, CancellationToken.None).Wait());
            }

            private static EntityCommand InitializeEntityCommand()
            {
                var entityCommandMock = new Mock<EntityCommand>();
                entityCommandMock.Setup(m => m.ValidateAndGetEntityTransaction()).Returns(default(EntityTransaction));
                entityCommandMock.SetupGet(m => m.Connection).Returns(new Mock<EntityConnection>().Object);
                entityCommandMock.Setup(m => m.InterceptionContext).Returns(new DbInterceptionContext());
                return entityCommandMock.Object;
            }
        }

        private static void AssertThrowsInAsyncMethod<TException>(string expectedMessage, Assert.ThrowsDelegate testCode)
            where TException : Exception
        {
            var exception = Assert.Throws<AggregateException>(testCode);
            var innerException = exception.InnerExceptions.Single();
            Assert.IsType<TException>(innerException);
            if (expectedMessage != null)
            {
                Assert.Equal(expectedMessage, innerException.Message);
            }
        }

#endif
    }
}
