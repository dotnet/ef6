// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
#if !NET40
    using System.Threading;
    using System.Threading.Tasks;
#endif
    using Moq;
    using Xunit;

    public class ObjectQueryExecutionPlanTests
    {
        [Fact]
        public void Execute_sets_the_parameter_values_and_returns_the_result()
        {
            var entityCommandDefinitionMock = Mock.Get(EntityClient.MockHelper.CreateEntityCommandDefinition());
            entityCommandDefinitionMock.Setup(m => m.Parameters).Returns(
                new[]
                    {
                        new EntityParameter
                            {
                                ParameterName = "Par1"
                            }
                    });

            entityCommandDefinitionMock.Setup(m => m.ExecuteStoreCommands(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>()))
                                       .Returns(
                                           (EntityCommand ec, CommandBehavior cb) =>
                                           {
                                               Assert.Equal(1, ec.Parameters.Count);
                                               Assert.Equal(2, ec.Parameters[0].Value);
                                               Assert.Equal(3, ec.CommandTimeout);
                                               return Common.Internal.Materialization.MockHelper.CreateDbDataReader(
                                                   new[] { new object[] { "Bar" } });
                                           });

            var shaperFactory = new ShaperFactory<string>(
                1,
                Objects.MockHelper.CreateCoordinatorFactory(shaper => (string)shaper.Reader.GetValue(0)),
                null,
                MergeOption.AppendOnly);

            var edmTypeMock = new Mock<EdmType>();
            edmTypeMock.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.SimpleType);

            var objectQueryExecutionPlan = new ObjectQueryExecutionPlan(
                entityCommandDefinitionMock.Object,
                shaperFactory, TypeUsage.Create(edmTypeMock.Object), MergeOption.AppendOnly, false, null, null);

            var objectParameterCollectionMock = new Mock<ObjectParameterCollection>(new ClrPerspective(new MetadataWorkspace()));

            objectParameterCollectionMock
                .Setup(m => m.GetEnumerator())
                .Returns(((IEnumerable<ObjectParameter>)new[] { new ObjectParameter("Par1", 2) }).GetEnumerator());

            var objectContextMock = Mock.Get(Objects.MockHelper.CreateMockObjectContext<string>());
            objectContextMock.Setup(m => m.CommandTimeout).Returns(3);

            var result = objectQueryExecutionPlan.Execute<string>(objectContextMock.Object, objectParameterCollectionMock.Object);

            Assert.Equal("Bar", result.Single());
        }

        [Fact]
        public void Execute_with_streaming_set_to_false_doesnt_stream()
        {
            Execute_with_streaming(false);
        }

        [Fact]
        public void Execute_with_streaming_set_to_true_streams()
        {
            Execute_with_streaming(true);
        }

        private void Execute_with_streaming(bool streaming)
        {
            var entityCommandDefinitionMock = Mock.Get(EntityClient.MockHelper.CreateEntityCommandDefinition());

            DbDataReader reader = null;
            entityCommandDefinitionMock.Setup(m => m.ExecuteStoreCommands(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>()))
                                       .Returns(
                                           (EntityCommand ec, CommandBehavior cb) =>
                                           {
                                               reader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(
                                                   new[] { new object[] { "Bar" } });
                                               Assert.Equal(streaming ? CommandBehavior.Default : CommandBehavior.SequentialAccess, cb);
                                               return reader;
                                           });

            var shaperFactory = new ShaperFactory<string>(
                1,
                Objects.MockHelper.CreateCoordinatorFactory(shaper => (string)shaper.Reader.GetValue(0)),
                null,
                MergeOption.AppendOnly);

            var edmTypeMock = new Mock<EdmType>();
            edmTypeMock.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.SimpleType);

            var objectQueryExecutionPlan = new ObjectQueryExecutionPlan(
                entityCommandDefinitionMock.Object,
                shaperFactory, TypeUsage.Create(edmTypeMock.Object), MergeOption.AppendOnly, streaming, null, null);

            var objectParameterCollectionMock = new Mock<ObjectParameterCollection>(new ClrPerspective(new MetadataWorkspace()));

            objectParameterCollectionMock
                .Setup(m => m.GetEnumerator())
                .Returns(((IEnumerable<ObjectParameter>)new ObjectParameter[0]).GetEnumerator());

            var objectContextMock = Mock.Get(Objects.MockHelper.CreateMockObjectContext<string>());
            objectContextMock.Setup(m => m.CommandTimeout).Returns(3);

            var result = objectQueryExecutionPlan.Execute<string>(objectContextMock.Object, objectParameterCollectionMock.Object);

            Assert.Equal(!streaming, reader.IsClosed);
        }

        [Fact]
        public void Execute_with_streaming_set_to_false_disposes_the_reader_on_exception()
        {
            Execute_disposes_the_reader_on_exception(false);
        }

        [Fact]
        public void Execute_with_streaming_set_to_true_disposes_the_reader_on_exception()
        {
            Execute_disposes_the_reader_on_exception(true);
        }

        private void Execute_disposes_the_reader_on_exception(bool streaming)
        {
            var entityCommandDefinitionMock = Mock.Get(EntityClient.MockHelper.CreateEntityCommandDefinition());

            DbDataReader reader = null;
            entityCommandDefinitionMock.Setup(m => m.ExecuteStoreCommands(It.IsAny<EntityCommand>(), It.IsAny<CommandBehavior>()))
                                       .Returns(
                                           (EntityCommand ec, CommandBehavior cb) =>
                                           {
                                               reader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(
                                                   new[] { new object[] { "Bar" } });
                                               return reader;
                                           });

            var shaperFactory = new ShaperFactory<string>(
                1,
                Objects.MockHelper.CreateCoordinatorFactory(shaper => (string)shaper.Reader.GetValue(0)),
                null,
                MergeOption.AppendOnly);

            var edmTypeMock = new Mock<EdmType>();
            edmTypeMock.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.SimpleType);

            var objectQueryExecutionPlan = new ObjectQueryExecutionPlan(
                entityCommandDefinitionMock.Object,
                shaperFactory, TypeUsage.Create(edmTypeMock.Object), MergeOption.AppendOnly, streaming, null, null);

            var objectParameterCollectionMock = new Mock<ObjectParameterCollection>(new ClrPerspective(new MetadataWorkspace()));

            objectParameterCollectionMock
                .Setup(m => m.GetEnumerator())
                .Returns(((IEnumerable<ObjectParameter>)new ObjectParameter[0]).GetEnumerator());

            var objectContextMock = Mock.Get(Objects.MockHelper.CreateMockObjectContext<string>());
            objectContextMock.Setup(m => m.CommandTimeout).Returns(3);
            objectContextMock.Setup(m => m.MetadataWorkspace).Returns(() => { throw new NotImplementedException(); });

            Assert.Throws<NotImplementedException>(() =>
                objectQueryExecutionPlan.Execute<string>(objectContextMock.Object, objectParameterCollectionMock.Object));

            Assert.Equal(true, reader.IsClosed);
            var readerMock = Mock.Get(reader);
            readerMock.Verify(m => m.GetValues(It.IsAny<object[]>()), streaming ? Times.Never() : Times.Once());
        }

#if !NET40

        [Fact]
        public void ExecuteAsync_sets_the_parameter_values_and_returns_the_result()
        {
            var entityCommandDefinitionMock = Mock.Get(EntityClient.MockHelper.CreateEntityCommandDefinition());
            entityCommandDefinitionMock.Setup(m => m.Parameters).Returns(
                new[]
                    {
                        new EntityParameter
                            {
                                ParameterName = "Par1"
                            }
                    });

            entityCommandDefinitionMock.Setup(
                m => m.ExecuteStoreCommandsAsync(
                    It.IsAny<EntityCommand>(),
                    It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
                                       .Returns(
                                           (EntityCommand ec, CommandBehavior cb, CancellationToken ct) =>
                                           {
                                               Assert.Equal(1, ec.Parameters.Count);
                                               Assert.Equal(2, ec.Parameters[0].Value);
                                               Assert.Equal(3, ec.CommandTimeout);
                                               return Task.FromResult(
                                                   Common.Internal.Materialization.MockHelper.CreateDbDataReader(
                                                       new[] { new object[] { "Bar" } }));
                                           });

            var shaperFactory = new ShaperFactory<string>(
                1,
                Objects.MockHelper.CreateCoordinatorFactory(shaper => (string)shaper.Reader.GetValue(0)),
                null,
                MergeOption.AppendOnly);

            var edmTypeMock = new Mock<EdmType>();
            edmTypeMock.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.SimpleType);

            var objectQueryExecutionPlan = new ObjectQueryExecutionPlan(
                entityCommandDefinitionMock.Object,
                shaperFactory, TypeUsage.Create(edmTypeMock.Object), MergeOption.AppendOnly, false, null, null);

            var objectParameterCollectionMock = new Mock<ObjectParameterCollection>(new ClrPerspective(new MetadataWorkspace()));

            objectParameterCollectionMock
                .Setup(m => m.GetEnumerator())
                .Returns(((IEnumerable<ObjectParameter>)new[] { new ObjectParameter("Par1", 2) }).GetEnumerator());

            var objectContextMock = Mock.Get(Objects.MockHelper.CreateMockObjectContext<string>());
            objectContextMock.Setup(m => m.CommandTimeout).Returns(3);

            var result = objectQueryExecutionPlan.ExecuteAsync<string>(
                objectContextMock.Object,
                objectParameterCollectionMock.Object, CancellationToken.None).Result;

            Assert.Equal("Bar", result.SingleAsync().Result);
        }

        [Fact]
        public void ExecuteAsync_with_streaming_set_to_false_doesnt_stream()
        {
            ExecuteAsync_with_streaming(false);
        }

        [Fact]
        public void ExecuteAsync_with_streaming_set_to_true_streams()
        {
            ExecuteAsync_with_streaming(true);
        }

        private void ExecuteAsync_with_streaming(bool streaming)
        {
            var entityCommandDefinitionMock = Mock.Get(EntityClient.MockHelper.CreateEntityCommandDefinition());

            DbDataReader reader = null;
            entityCommandDefinitionMock.Setup(
                m => m.ExecuteStoreCommandsAsync(
                    It.IsAny<EntityCommand>(),
                    It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
                                       .Returns(
                                           (EntityCommand ec, CommandBehavior cb, CancellationToken ct) =>
                                           {
                                               reader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(
                                                       new[] { new object[] { "Bar" } });
                                               Assert.Equal(streaming ? CommandBehavior.Default : CommandBehavior.SequentialAccess, cb);
                                               return Task.FromResult(reader);
                                           });

            var shaperFactory = new ShaperFactory<string>(
                1,
                Objects.MockHelper.CreateCoordinatorFactory(shaper => (string)shaper.Reader.GetValue(0)),
                null,
                MergeOption.AppendOnly);

            var edmTypeMock = new Mock<EdmType>();
            edmTypeMock.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.SimpleType);

            var objectQueryExecutionPlan = new ObjectQueryExecutionPlan(
                entityCommandDefinitionMock.Object,
                shaperFactory, TypeUsage.Create(edmTypeMock.Object), MergeOption.AppendOnly, streaming, null, null);

            var objectParameterCollectionMock = new Mock<ObjectParameterCollection>(new ClrPerspective(new MetadataWorkspace()));

            objectParameterCollectionMock
                .Setup(m => m.GetEnumerator())
                .Returns(((IEnumerable<ObjectParameter>)new[] { new ObjectParameter("Par1", 2) }).GetEnumerator());

            var objectContextMock = Mock.Get(Objects.MockHelper.CreateMockObjectContext<string>());
            objectContextMock.Setup(m => m.CommandTimeout).Returns(3);

            var result = objectQueryExecutionPlan.ExecuteAsync<string>(
                objectContextMock.Object,
                objectParameterCollectionMock.Object, CancellationToken.None).Result;

            Assert.Equal(!streaming, reader.IsClosed);
        }

        [Fact]
        public void ExecuteAsync_with_streaming_set_to_false_disposes_the_reader_on_exception()
        {
            ExecuteAsync_disposes_the_reader_on_exception(false);
        }

        [Fact]
        public void ExecuteAsync_with_streaming_set_to_true_disposes_the_reader_on_exception()
        {
            ExecuteAsync_disposes_the_reader_on_exception(true);
        }

        private void ExecuteAsync_disposes_the_reader_on_exception(bool streaming)
        {
            var entityCommandDefinitionMock = Mock.Get(EntityClient.MockHelper.CreateEntityCommandDefinition());

            DbDataReader reader = null;
            entityCommandDefinitionMock.Setup(
                m => m.ExecuteStoreCommandsAsync(
                    It.IsAny<EntityCommand>(),
                    It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
                                       .Returns(
                                           (EntityCommand ec, CommandBehavior cb, CancellationToken ct) =>
                                           {
                                               reader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(
                                                       new[] { new object[] { "Bar" } });
                                               return Task.FromResult(reader);
                                           });

            var shaperFactory = new ShaperFactory<string>(
                1,
                Objects.MockHelper.CreateCoordinatorFactory(shaper => (string)shaper.Reader.GetValue(0)),
                null,
                MergeOption.AppendOnly);

            var edmTypeMock = new Mock<EdmType>();
            edmTypeMock.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.SimpleType);

            var objectQueryExecutionPlan = new ObjectQueryExecutionPlan(
                entityCommandDefinitionMock.Object,
                shaperFactory, TypeUsage.Create(edmTypeMock.Object), MergeOption.AppendOnly, streaming, null, null);

            var objectParameterCollectionMock = new Mock<ObjectParameterCollection>(new ClrPerspective(new MetadataWorkspace()));

            objectParameterCollectionMock
                .Setup(m => m.GetEnumerator())
                .Returns(((IEnumerable<ObjectParameter>)new[] { new ObjectParameter("Par1", 2) }).GetEnumerator());

            var objectContextMock = Mock.Get(Objects.MockHelper.CreateMockObjectContext<string>());
            objectContextMock.Setup(m => m.CommandTimeout).Returns(3);
            objectContextMock.Setup(m => m.MetadataWorkspace).Returns(() => { throw new NotImplementedException(); });

            Assert.Throws<NotImplementedException>(() =>
                ExceptionHelpers.UnwrapAggregateExceptions(() => objectQueryExecutionPlan.ExecuteAsync<string>(
                    objectContextMock.Object,
                    objectParameterCollectionMock.Object, CancellationToken.None).Result));

            Assert.Equal(true, reader.IsClosed);
            var readerMock = Mock.Get(reader);
            readerMock.Verify(m => m.GetFieldValueAsync<object>(It.IsAny<int>(), It.IsAny<CancellationToken>()), streaming ? Times.Never() : Times.Once());
        }

#endif
    }
}
