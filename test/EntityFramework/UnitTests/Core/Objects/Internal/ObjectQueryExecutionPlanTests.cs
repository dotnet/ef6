// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;
#if !NET40
#endif

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
                shaperFactory, TypeUsage.Create(edmTypeMock.Object), MergeOption.AppendOnly, null, null);

            var objectParameterCollectionMock = new Mock<ObjectParameterCollection>(new ClrPerspective(new MetadataWorkspace()));

            objectParameterCollectionMock
                .Setup(m => m.GetEnumerator())
                .Returns(((IEnumerable<ObjectParameter>)new[] { new ObjectParameter("Par1", 2) }).GetEnumerator());

            var objectContextMock = Mock.Get(Objects.MockHelper.CreateMockObjectContext<string>());
            objectContextMock.Setup(m => m.CommandTimeout).Returns(3);

            var result = objectQueryExecutionPlan.Execute<string>(objectContextMock.Object, objectParameterCollectionMock.Object);

            Assert.Equal("Bar", result.Single());
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
                shaperFactory, TypeUsage.Create(edmTypeMock.Object), MergeOption.AppendOnly, null, null);

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

#endif
    }
}
