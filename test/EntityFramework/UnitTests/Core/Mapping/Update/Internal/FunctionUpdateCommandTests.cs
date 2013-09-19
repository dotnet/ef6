// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class FunctionUpdateCommandTests
    {
        public class Execute
        {
            [Fact]
            public void Returns_rows_affected_when_there_are_no_result_columns()
            {
                var stateEntries = new ReadOnlyCollection<IEntityStateEntry>(new List<IEntityStateEntry>());
                var stateEntry = new ExtractedStateEntry(
                    EntityState.Unchanged, PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0),
                    PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0),
                    new Mock<IEntityStateEntry>().Object);

                var timeout = 43;
                var updateTranslatorMock = new Mock<UpdateTranslator>();
                updateTranslatorMock.Setup(m => m.CommandTimeout).Returns(timeout);
                var entityConnection = new Mock<EntityConnection>().Object;
                updateTranslatorMock.Setup(m => m.Connection).Returns(entityConnection);
                updateTranslatorMock.Setup(m => m.InterceptionContext).Returns(new DbInterceptionContext());

                var dbCommandMock = new Mock<DbCommand>();

                var mockFunctionUpdateCommand = new Mock<FunctionUpdateCommand>(
                    updateTranslatorMock.Object, stateEntries, stateEntry, dbCommandMock.Object)
                                                    {
                                                        CallBase = true
                                                    };

                var timesCommandTimeoutCalled = 0;
                dbCommandMock.SetupSet(m => m.CommandTimeout = It.IsAny<int>()).Callback(
                    (int value) =>
                        {
                            timesCommandTimeoutCalled++;
                            Assert.Equal(timeout, value);
                        });

                var rowsAffected = 36;
                dbCommandMock.Setup(m => m.ExecuteNonQuery()).Returns(rowsAffected);

                var timesSetInputIdentifiers = 0;
                var identifierValues = new Dictionary<int, object>();
                mockFunctionUpdateCommand.Setup(m => m.SetInputIdentifiers(It.IsAny<Dictionary<int, object>>()))
                    .Callback<Dictionary<int, object>>(
                        identifierValuesPassed =>
                            {
                                timesSetInputIdentifiers++;
                                Assert.Same(identifierValues, identifierValuesPassed);
                            });

                var generatedValues = new List<KeyValuePair<PropagatorResult, object>>();

                var rowsAffectedResult = mockFunctionUpdateCommand.Object.Execute(identifierValues, generatedValues);

                Assert.Equal(rowsAffected, rowsAffectedResult);
                Assert.Equal(1, timesCommandTimeoutCalled);
                Assert.Equal(1, timesSetInputIdentifiers);
                Assert.Equal(0, generatedValues.Count);
            }

            [Fact]
            public void Returns_rows_affected_when_there_are_result_columns()
            {
                var mockPrimitiveType = new Mock<PrimitiveType>();
                mockPrimitiveType.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.PrimitiveType);
                mockPrimitiveType.Setup(m => m.PrimitiveTypeKind).Returns(PrimitiveTypeKind.Int32);
                mockPrimitiveType.Setup(m => m.DataSpace).Returns(DataSpace.CSpace);
                var edmProperty = new EdmProperty("property", TypeUsage.Create(mockPrimitiveType.Object));

                var entityType = new EntityType("", "", DataSpace.CSpace, Enumerable.Empty<string>(), new[] { edmProperty });
                entityType.SetReadOnly();

                var stateEntry = new ExtractedStateEntry(
                    EntityState.Unchanged,
                    PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0),
                    PropagatorResult.CreateStructuralValue(
                        new[] { PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0) },
                        entityType,
                        isModified: false),
                    new Mock<IEntityStateEntry>().Object);

                var updateTranslatorMock = new Mock<UpdateTranslator>();
                updateTranslatorMock.Setup(m => m.CommandTimeout).Returns(() => null);
                var entityConnection = new Mock<EntityConnection>().Object;
                updateTranslatorMock.Setup(m => m.Connection).Returns(entityConnection);
                updateTranslatorMock.Setup(m => m.InterceptionContext).Returns(new DbInterceptionContext());

                var dbCommandMock = new Mock<DbCommand>();
                var stateEntries = new ReadOnlyCollection<IEntityStateEntry>(new List<IEntityStateEntry>());

                var mockFunctionUpdateCommand = new Mock<FunctionUpdateCommand>(
                    updateTranslatorMock.Object, stateEntries, stateEntry, dbCommandMock.Object)
                                                    {
                                                        CallBase = true
                                                    };

                var dbValue = 66;
                var dbDataReaderMock = new Mock<DbDataReader>();
                dbDataReaderMock.Setup(m => m.GetValue(It.IsAny<int>())).Returns(dbValue);
                var rowsToRead = 2;
                dbDataReaderMock.Setup(m => m.Read()).Returns(
                    () =>
                        {
                            rowsToRead--;
                            return rowsToRead > 0;
                        });
                dbCommandMock.Protected().Setup<DbDataReader>("ExecuteDbDataReader", CommandBehavior.SequentialAccess).Returns(
                    dbDataReaderMock.Object);

                var timesSetInputIdentifiers = 0;
                var identifierValues = new Dictionary<int, object>();
                mockFunctionUpdateCommand.Setup(m => m.SetInputIdentifiers(It.IsAny<Dictionary<int, object>>()))
                    .Callback<Dictionary<int, object>>(
                        identifierValuesPassed =>
                            {
                                timesSetInputIdentifiers++;
                                Assert.Same(identifierValues, identifierValuesPassed);
                            });

                var generatedValues = new List<KeyValuePair<PropagatorResult, object>>();
                var mockObjectStateManager = new Mock<ObjectStateManager>();
                var objectStateEntryMock = new Mock<ObjectStateEntry>(mockObjectStateManager.Object, null, EntityState.Unchanged);
                var currentValueRecordMock = new Mock<CurrentValueRecord>(objectStateEntryMock.Object);

                var idColumn = new KeyValuePair<string, PropagatorResult>(
                    "ID",
                    PropagatorResult.CreateServerGenSimpleValue(
                        PropagatorFlags.NoFlags, /*value:*/ 0, currentValueRecordMock.Object, recordOrdinal: 0));
                mockFunctionUpdateCommand.Protected().Setup<List<KeyValuePair<string, PropagatorResult>>>("ResultColumns")
                    .Returns((new[] { idColumn }).ToList());

                var rowsAffectedResult = mockFunctionUpdateCommand.Object.Execute(identifierValues, generatedValues);

                Assert.Equal(1, rowsAffectedResult);
                Assert.Equal(1, timesSetInputIdentifiers);
                Assert.Equal(1, generatedValues.Count);
                Assert.Same(idColumn.Value, generatedValues[0].Key);
                Assert.Equal(dbValue, generatedValues[0].Value);
            }
        }

#if !NET40

        public class ExecuteAsync
        {
            [Fact]
            public void Returns_rows_affected_when_there_are_no_result_columns()
            {
                var stateEntries = new ReadOnlyCollection<IEntityStateEntry>(new List<IEntityStateEntry>());
                var stateEntry = new ExtractedStateEntry(
                    EntityState.Unchanged, PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0),
                    PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0),
                    new Mock<IEntityStateEntry>().Object);

                var timeout = 43;
                var updateTranslatorMock = new Mock<UpdateTranslator>();
                updateTranslatorMock.Setup(m => m.CommandTimeout).Returns(timeout);
                var entityConnection = new Mock<EntityConnection>().Object;
                updateTranslatorMock.Setup(m => m.Connection).Returns(entityConnection);
                updateTranslatorMock.Setup(m => m.InterceptionContext).Returns(new DbInterceptionContext());

                var dbCommandMock = new Mock<DbCommand>();

                var mockFunctionUpdateCommand = new Mock<FunctionUpdateCommand>(
                    updateTranslatorMock.Object, stateEntries, stateEntry, dbCommandMock.Object)
                                                    {
                                                        CallBase = true
                                                    };

                var timesCommandTimeoutCalled = 0;
                dbCommandMock.SetupSet(m => m.CommandTimeout = It.IsAny<int>()).Callback(
                    (int value) =>
                        {
                            timesCommandTimeoutCalled++;
                            Assert.Equal(timeout, value);
                        });

                var rowsAffected = 36;
                dbCommandMock.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(rowsAffected));

                var timesSetInputIdentifiers = 0;
                var identifierValues = new Dictionary<int, object>();
                mockFunctionUpdateCommand.Setup(m => m.SetInputIdentifiers(It.IsAny<Dictionary<int, object>>()))
                    .Callback<Dictionary<int, object>>(
                        identifierValuesPassed =>
                            {
                                timesSetInputIdentifiers++;
                                Assert.Same(identifierValues, identifierValuesPassed);
                            });

                var generatedValues = new List<KeyValuePair<PropagatorResult, object>>();

                var rowsAffectedResult =
                    mockFunctionUpdateCommand.Object.ExecuteAsync(identifierValues, generatedValues, CancellationToken.None).Result;

                Assert.Equal(rowsAffected, rowsAffectedResult);
                Assert.Equal(1, timesCommandTimeoutCalled);
                Assert.Equal(1, timesSetInputIdentifiers);
                Assert.Equal(0, generatedValues.Count);
            }

            [Fact]
            public void Returns_rows_affected_when_there_are_result_columns()
            {
                var mockPrimitiveType = new Mock<PrimitiveType>();
                mockPrimitiveType.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.PrimitiveType);
                mockPrimitiveType.Setup(m => m.PrimitiveTypeKind).Returns(PrimitiveTypeKind.Int32);
                mockPrimitiveType.Setup(m => m.DataSpace).Returns(DataSpace.CSpace);
                var edmProperty = new EdmProperty("property", TypeUsage.Create(mockPrimitiveType.Object));

                var entityType = new EntityType("", "", DataSpace.CSpace, Enumerable.Empty<string>(), new[] { edmProperty });
                entityType.SetReadOnly();

                var stateEntry = new ExtractedStateEntry(
                    EntityState.Unchanged,
                    PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0),
                    PropagatorResult.CreateStructuralValue(
                        new[] { PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value: 0) },
                        entityType,
                        isModified: false),
                    new Mock<IEntityStateEntry>().Object);

                var updateTranslatorMock = new Mock<UpdateTranslator>();
                updateTranslatorMock.Setup(m => m.CommandTimeout).Returns(() => null);
                var entityConnection = new Mock<EntityConnection>().Object;
                updateTranslatorMock.Setup(m => m.Connection).Returns(entityConnection);
                updateTranslatorMock.Setup(m => m.InterceptionContext).Returns(new DbInterceptionContext());

                var dbCommandMock = new Mock<DbCommand>();
                var stateEntries = new ReadOnlyCollection<IEntityStateEntry>(new List<IEntityStateEntry>());

                var mockFunctionUpdateCommand = new Mock<FunctionUpdateCommand>(
                    updateTranslatorMock.Object, stateEntries, stateEntry, dbCommandMock.Object)
                                                    {
                                                        CallBase = true
                                                    };

                var dbValue = 66;
                var dbDataReaderMock = new Mock<DbDataReader>();
                dbDataReaderMock.Setup(m => m.GetFieldValueAsync<object>(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult<object>(dbValue));
                var rowsToRead = 2;
                dbDataReaderMock.Setup(m => m.ReadAsync(It.IsAny<CancellationToken>())).Returns(
                    () =>
                        {
                            rowsToRead--;
                            return Task.FromResult(rowsToRead > 0);
                        });
                dbDataReaderMock.Setup(m => m.NextResultAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(false));

                dbCommandMock.Protected()
                    .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", CommandBehavior.SequentialAccess, It.IsAny<CancellationToken>())
                    .Returns(Task.FromResult(dbDataReaderMock.Object));

                var timesSetInputIdentifiers = 0;
                var identifierValues = new Dictionary<int, object>();
                mockFunctionUpdateCommand.Setup(m => m.SetInputIdentifiers(It.IsAny<Dictionary<int, object>>()))
                    .Callback<Dictionary<int, object>>(
                        identifierValuesPassed =>
                            {
                                timesSetInputIdentifiers++;
                                Assert.Same(identifierValues, identifierValuesPassed);
                            });

                var generatedValues = new List<KeyValuePair<PropagatorResult, object>>();
                var objectStateManagerMock = new Mock<ObjectStateManager>();
                var objectStateEntryMock = new Mock<ObjectStateEntry>(objectStateManagerMock.Object, null, EntityState.Unchanged);
                var currentValueRecordMock = new Mock<CurrentValueRecord>(objectStateEntryMock.Object);

                var idColumn = new KeyValuePair<string, PropagatorResult>(
                    "ID",
                    PropagatorResult.CreateServerGenSimpleValue(
                        PropagatorFlags.NoFlags, /*value:*/ 0, currentValueRecordMock.Object, recordOrdinal: 0));
                mockFunctionUpdateCommand.Protected().Setup<List<KeyValuePair<string, PropagatorResult>>>("ResultColumns")
                    .Returns((new[] { idColumn }).ToList());

                var rowsAffectedResult =
                    mockFunctionUpdateCommand.Object.ExecuteAsync(identifierValues, generatedValues, CancellationToken.None).Result;

                Assert.Equal(1, rowsAffectedResult);
                Assert.Equal(1, timesSetInputIdentifiers);
                Assert.Equal(1, generatedValues.Count);
                Assert.Same(idColumn.Value, generatedValues[0].Key);
                Assert.Equal(dbValue, generatedValues[0].Value);
            }
        }

#endif
    }
}
