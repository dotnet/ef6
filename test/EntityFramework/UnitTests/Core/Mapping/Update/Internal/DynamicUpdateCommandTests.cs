// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Internal;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class DynamicUpdateCommandTests
    {
        public class Execute
        {
            [Fact]
            public void Returns_rows_affected_when_there_is_no_reader()
            {
                var timeout = 43;
                var mockUpdateTranslator = new Mock<UpdateTranslator>(MockBehavior.Strict);
                mockUpdateTranslator.Setup(m => m.CommandTimeout).Returns(timeout);
                var entityConnection = new Mock<EntityConnection>().Object;
                mockUpdateTranslator.Setup(m => m.Connection).Returns(entityConnection);

                var mockDbModificationCommandTree = new Mock<DbModificationCommandTree>();
                var mockDynamicUpdateCommand = new Mock<DynamicUpdateCommand>(
                    new Mock<TableChangeProcessor>().Object, mockUpdateTranslator.Object,
                    ModificationOperator.Delete, PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0),
                    PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0), mockDbModificationCommandTree.Object,
                    /*outputIdentifiers*/ null)
                                                   {
                                                       CallBase = true
                                                   };

                var mockDbCommand = new Mock<DbCommand>();
                var timesCommandTimeoutCalled = 0;
                mockDbCommand.SetupSet(m => m.CommandTimeout = It.IsAny<int>()).Callback(
                    (int value) =>
                        {
                            timesCommandTimeoutCalled++;
                            Assert.Equal(timeout, value);
                        });

                var rowsAffected = 36;
                mockDbCommand.Setup(m => m.ExecuteNonQuery()).Returns(rowsAffected);

                var identifierValues = new Dictionary<int, object>();

                mockDynamicUpdateCommand.Protected().Setup<DbCommand>("CreateCommand", identifierValues).Returns(mockDbCommand.Object);

                var generatedValues = new List<KeyValuePair<PropagatorResult, object>>();

                var rowsAffectedResult = mockDynamicUpdateCommand.Object.Execute(identifierValues, generatedValues);

                Assert.Equal(rowsAffected, rowsAffectedResult);
                Assert.Equal(1, timesCommandTimeoutCalled);
                Assert.Equal(0, generatedValues.Count);
            }

            [Fact]
            public void Returns_rows_affected_when_there_is_a_reader()
            {
                var mockPrimitiveType = new Mock<PrimitiveType>();
                mockPrimitiveType.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.PrimitiveType);
                mockPrimitiveType.Setup(m => m.PrimitiveTypeKind).Returns(PrimitiveTypeKind.Int32);
                mockPrimitiveType.Setup(m => m.DataSpace).Returns(DataSpace.CSpace);
                var memberName = "property";
                var edmProperty = new EdmProperty(memberName, TypeUsage.Create(mockPrimitiveType.Object));

                var entityType = new EntityType("", "", DataSpace.CSpace, Enumerable.Empty<string>(), new[] { edmProperty });
                entityType.SetReadOnly();

                var mockUpdateTranslator = new Mock<UpdateTranslator>(MockBehavior.Strict);
                mockUpdateTranslator.Setup(m => m.CommandTimeout).Returns(() => null);
                var entityConnection = new Mock<EntityConnection>().Object;
                mockUpdateTranslator.Setup(m => m.Connection).Returns(entityConnection);

                var mockDbModificationCommandTree = new Mock<DbModificationCommandTree>();
                mockDbModificationCommandTree.SetupGet(m => m.HasReader).Returns(true);

                var mockDynamicUpdateCommand = new Mock<DynamicUpdateCommand>(
                    new Mock<TableChangeProcessor>().Object, mockUpdateTranslator.Object,
                    ModificationOperator.Delete, PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0),
                    PropagatorResult.CreateStructuralValue(
                        new[] { PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0) },
                        entityType,
                        isModified: false),
                    mockDbModificationCommandTree.Object, /*outputIdentifiers*/ null)
                                                   {
                                                       CallBase = true
                                                   };

                var mockDbCommand = new Mock<DbCommand>();

                var dbValue = 66;
                var mockDbDataReader = new Mock<DbDataReader>();
                mockDbDataReader.Setup(m => m.GetValue(It.IsAny<int>())).Returns(dbValue);
                var rowsToRead = 2;
                mockDbDataReader.Setup(m => m.Read()).Returns(
                    () =>
                        {
                            rowsToRead--;
                            return rowsToRead > 0;
                        });
                mockDbDataReader.Setup(m => m.FieldCount).Returns(1);
                mockDbDataReader.Setup(m => m.GetName(0)).Returns(memberName);
                mockDbCommand.Protected().Setup<DbDataReader>("ExecuteDbDataReader", CommandBehavior.SequentialAccess).Returns(
                    mockDbDataReader.Object);

                var identifierValues = new Dictionary<int, object>();

                mockDynamicUpdateCommand.Protected().Setup<DbCommand>("CreateCommand", identifierValues).Returns(mockDbCommand.Object);

                var generatedValues = new List<KeyValuePair<PropagatorResult, object>>();

                var rowsAffectedResult = mockDynamicUpdateCommand.Object.Execute(identifierValues, generatedValues);

                Assert.Equal(1, rowsAffectedResult);
                Assert.Equal(1, generatedValues.Count);
                Assert.Equal(dbValue, generatedValues[0].Value);
                Assert.Equal(0, generatedValues[0].Key.GetSimpleValue());
            }
        }

#if !NET40

        public class ExecuteAsync
        {
            [Fact]
            public void Returns_rows_affected_when_there_is_no_reader()
            {
                var timeout = 43;
                var mockUpdateTranslator = new Mock<UpdateTranslator>(MockBehavior.Strict);
                mockUpdateTranslator.Setup(m => m.CommandTimeout).Returns(timeout);
                var entityConnection = new Mock<EntityConnection>().Object;
                mockUpdateTranslator.Setup(m => m.Connection).Returns(entityConnection);

                var mockDbModificationCommandTree = new Mock<DbModificationCommandTree>();
                var mockDynamicUpdateCommand = new Mock<DynamicUpdateCommand>(
                    new Mock<TableChangeProcessor>().Object, mockUpdateTranslator.Object,
                    ModificationOperator.Delete, PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0),
                    PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0), mockDbModificationCommandTree.Object,
                    /*outputIdentifiers*/ null)
                                                   {
                                                       CallBase = true
                                                   };

                var mockDbCommand = new Mock<DbCommand>();
                var timesCommandTimeoutCalled = 0;
                mockDbCommand.SetupSet(m => m.CommandTimeout = It.IsAny<int>()).Callback(
                    (int value) =>
                        {
                            timesCommandTimeoutCalled++;
                            Assert.Equal(timeout, value);
                        });

                var rowsAffected = 36;
                mockDbCommand.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(rowsAffected));

                var identifierValues = new Dictionary<int, object>();

                mockDynamicUpdateCommand.Protected().Setup<DbCommand>("CreateCommand", identifierValues).Returns(mockDbCommand.Object);

                var generatedValues = new List<KeyValuePair<PropagatorResult, object>>();

                var rowsAffectedResult =
                    mockDynamicUpdateCommand.Object.ExecuteAsync(identifierValues, generatedValues, CancellationToken.None).Result;

                Assert.Equal(rowsAffected, rowsAffectedResult);
                Assert.Equal(1, timesCommandTimeoutCalled);
                Assert.Equal(0, generatedValues.Count);
            }

            [Fact]
            public void Returns_rows_affected_when_there_is_a_reader()
            {
                var mockPrimitiveType = new Mock<PrimitiveType>();
                mockPrimitiveType.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.PrimitiveType);
                mockPrimitiveType.Setup(m => m.PrimitiveTypeKind).Returns(PrimitiveTypeKind.Int32);
                mockPrimitiveType.Setup(m => m.DataSpace).Returns(DataSpace.CSpace);
                var memberName = "property";
                var edmProperty = new EdmProperty(memberName, TypeUsage.Create(mockPrimitiveType.Object));

                var entityType = new EntityType("", "", DataSpace.CSpace, Enumerable.Empty<string>(), new[] { edmProperty });
                entityType.SetReadOnly();

                var mockUpdateTranslator = new Mock<UpdateTranslator>(MockBehavior.Strict);
                mockUpdateTranslator.Setup(m => m.CommandTimeout).Returns(() => null);
                var entityConnection = new Mock<EntityConnection>().Object;
                mockUpdateTranslator.Setup(m => m.Connection).Returns(entityConnection);

                var mockDbModificationCommandTree = new Mock<DbModificationCommandTree>();
                mockDbModificationCommandTree.SetupGet(m => m.HasReader).Returns(true);

                var mockDynamicUpdateCommand = new Mock<DynamicUpdateCommand>(
                    new Mock<TableChangeProcessor>().Object, mockUpdateTranslator.Object,
                    ModificationOperator.Delete, PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0),
                    PropagatorResult.CreateStructuralValue(
                        new[] { PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0) },
                        entityType,
                        isModified: false),
                    mockDbModificationCommandTree.Object, /*outputIdentifiers*/ null)
                                                   {
                                                       CallBase = true
                                                   };

                var mockDbCommand = new Mock<DbCommand>();

                var dbValue = 66;
                var mockDbDataReader = new Mock<DbDataReader>();
                mockDbDataReader.Setup(m => m.GetFieldValueAsync<object>(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult<object>(dbValue));
                var rowsToRead = 2;
                mockDbDataReader.Setup(m => m.ReadAsync(It.IsAny<CancellationToken>())).Returns(
                    () =>
                        {
                            rowsToRead--;
                            return Task.FromResult(rowsToRead > 0);
                        });
                mockDbDataReader.Setup(m => m.FieldCount).Returns(1);
                mockDbDataReader.Setup(m => m.GetName(0)).Returns(memberName);
                mockDbDataReader.Setup(m => m.NextResultAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(false));

                mockDbCommand.Protected()
                    .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", CommandBehavior.SequentialAccess, It.IsAny<CancellationToken>())
                    .Returns(Task.FromResult(mockDbDataReader.Object));

                var identifierValues = new Dictionary<int, object>();

                mockDynamicUpdateCommand.Protected().Setup<DbCommand>("CreateCommand", identifierValues).Returns(mockDbCommand.Object);

                var generatedValues = new List<KeyValuePair<PropagatorResult, object>>();

                var rowsAffectedResult =
                    mockDynamicUpdateCommand.Object.ExecuteAsync(identifierValues, generatedValues, CancellationToken.None).Result;

                Assert.Equal(1, rowsAffectedResult);
                Assert.Equal(1, generatedValues.Count);
                Assert.Equal(dbValue, generatedValues[0].Value);
                Assert.Equal(0, generatedValues[0].Key.GetSimpleValue());
            }
        }

#endif
    }
}
