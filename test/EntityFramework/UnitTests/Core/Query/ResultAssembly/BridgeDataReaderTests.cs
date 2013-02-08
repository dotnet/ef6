// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.ResultAssembly
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;
    using Xunit;
    using MockHelper = System.Data.Entity.Core.Objects.MockHelper;

    public class BridgeDataReaderTests
    {
        [Fact]
        public void Constructors_dont_advance_the_underlying_shaper()
        {
            var sourceEnumerable = new[] { new object[] { 1 } };

            var rootCoordinatorFactory = MockHelper.CreateCoordinatorFactory<int, RecordState>(
                depth: 0,
                stateSlot: 0,
                ordinal: 0,
                nestedCoordinators: new CoordinatorFactory[0],
                producedValues: null);

            var shaperMock = new Mock<Shaper<RecordState>>(
                Common.Internal.Materialization.MockHelper.CreateDbDataReader(sourceEnumerable),
                /*context*/ null,
                /*workspace*/ null,
                MergeOption.AppendOnly,
                /*stateCount*/ 2,
                rootCoordinatorFactory,
                /*readerOwned*/ false,
                /*useSpatialReader*/ false)
                                 {
                                     CallBase = true
                                 };

            shaperMock.Setup(m => m.GetEnumerator()).Returns(
                () =>
                    {
                        Assert.True(false);
                        return null;
                    });

            // Verify these methods don't cause initialization
            var bridgeDataReader = new BridgeDataReader(shaperMock.Object, rootCoordinatorFactory, 0, null);
            bridgeDataReader.GetEnumerator();
        }

        [Fact]
        public void Initialization_performed_lazily()
        {
            VerifyGetterInitialization(bdr => bdr[0]);
            VerifyGetterInitialization(bdr => bdr["foo"]);
            VerifyGetterInitialization(bdr => bdr.DataRecordInfo);
            VerifyGetterInitialization(bdr => bdr.Depth);
            VerifyGetterInitialization(bdr => bdr.FieldCount);
            VerifyGetterInitialization(bdr => bdr.HasRows);
            VerifyGetterInitialization(bdr => bdr.IsClosed);
            VerifyGetterInitialization(bdr => bdr.RecordsAffected);
            VerifyGetterInitialization(bdr => bdr.VisibleFieldCount);
            VerifyMethodInitialization(bdr => bdr.Close());
            VerifyMethodInitialization(bdr => bdr.CloseImplicitly());
            VerifyMethodInitialization(bdr => bdr.Dispose());
            VerifyMethodInitialization(bdr => bdr.GetBoolean(0));
            VerifyMethodInitialization(bdr => bdr.GetByte(0));
            VerifyMethodInitialization(bdr => bdr.GetBytes(0, 0, new byte[1], 0, 0));
            VerifyMethodInitialization(bdr => bdr.GetChar(0));
            VerifyMethodInitialization(bdr => bdr.GetChars(0, 0, new char[1], 0, 1));
            VerifyMethodInitialization(bdr => bdr.GetData(0));
            VerifyMethodInitialization(bdr => bdr.GetDataReader(0));
            VerifyMethodInitialization(bdr => bdr.GetDataRecord(0));
            VerifyMethodInitialization(bdr => bdr.GetDataTypeName(0));
            VerifyMethodInitialization(bdr => bdr.GetDateTime(0));
            VerifyMethodInitialization(bdr => bdr.GetDecimal(0));
            VerifyMethodInitialization(bdr => bdr.GetDouble(0));
            VerifyMethodInitialization(bdr => bdr.GetFieldType(0));
            VerifyMethodInitialization(bdr => bdr.GetFloat(0));
            VerifyMethodInitialization(bdr => bdr.GetGuid(0));
            VerifyMethodInitialization(bdr => bdr.GetInt16(0));
            VerifyMethodInitialization(bdr => bdr.GetInt32(0));
            VerifyMethodInitialization(bdr => bdr.GetInt64(0));
            VerifyMethodInitialization(bdr => bdr.GetName(0));
            VerifyMethodInitialization(bdr => bdr.GetOrdinal(""));
            VerifyMethodInitialization(bdr => bdr.GetString(0));
            VerifyMethodInitialization(bdr => bdr.GetValue(0));
            VerifyMethodInitialization(bdr => bdr.GetValues(new object[0]));
            VerifyMethodInitialization(bdr => bdr.IsDBNull(0));
            VerifyMethodInitialization(bdr => bdr.NextResult());
            VerifyMethodInitialization(bdr => bdr.Read());
            VerifyMethodInitialization(bdr => bdr.ToList<object>());

#if !NET40
            VerifyMethodInitialization(bdr => bdr.CloseImplicitlyAsync(CancellationToken.None).Wait(), async: true);
            VerifyMethodInitialization(bdr => bdr.GetFieldValue<object>(0));
            VerifyMethodInitialization(bdr => bdr.GetFieldValueAsync<object>(0).Wait(), async: true);
            VerifyMethodInitialization(bdr => bdr.GetStream(0));
            VerifyMethodInitialization(bdr => bdr.GetTextReader(0));
            VerifyMethodInitialization(bdr => bdr.IsDBNullAsync(0));
            VerifyMethodInitialization(bdr => bdr.NextResultAsync().Wait(), async: true);
            VerifyMethodInitialization(bdr => bdr.ReadAsync().Wait(), async: true);
#endif
        }

        private void VerifyGetterInitialization<TProperty>(
            Func<BridgeDataReader, TProperty> getterFunc)
        {
            Assert.NotNull(getterFunc);

            VerifyMethodInitialization(bdr => getterFunc(bdr));
        }

        private void VerifyMethodInitialization(Action<BridgeDataReader> methodToInvoke, bool async = false)
        {
            Assert.NotNull(methodToInvoke);

            var sourceEnumerable = new[] { new object[] { 1 } };

            var rootCoordinatorFactory = MockHelper.CreateCoordinatorFactory<int, RecordState>(
                depth: 0,
                stateSlot: 0,
                ordinal: 0,
                nestedCoordinators: new CoordinatorFactory[0],
                producedValues: null);

            var shaperMock = new Mock<Shaper<RecordState>>(
                Common.Internal.Materialization.MockHelper.CreateDbDataReader(sourceEnumerable),
                /*context*/ null,
                /*workspace*/ null,
                MergeOption.AppendOnly,
                /*stateCount*/ 2,
                rootCoordinatorFactory,
                /*readerOwned*/ false,
                /*useSpatialReader*/ false)
                                 {
                                     CallBase = true
                                 };

            var bridgeDataReaderMock = new Mock<BridgeDataReader>(shaperMock.Object, rootCoordinatorFactory, 0, null)
                                           {
                                               CallBase = true
                                           };

            bridgeDataReaderMock.Protected().Setup("EnsureInitialized").Verifiable();
#if !NET40
            bridgeDataReaderMock.Protected().Setup<Task>("EnsureInitializedAsync", ItExpr.IsAny<CancellationToken>()).Verifiable();
#endif

            try
            {
                methodToInvoke(bridgeDataReaderMock.Object);
            }
            catch (Exception)
            {
            }

            if (async)
            {
                bridgeDataReaderMock.Protected().Verify<Task>(
                    "EnsureInitializedAsync", Times.AtLeastOnce(), ItExpr.IsAny<CancellationToken>());
            }
            else
            {
                bridgeDataReaderMock.Protected().Verify("EnsureInitialized", Times.AtLeastOnce());
            }
        }

#if !NET40

        [Fact]
        public void NextResult_Read_and_GetFieldValue_return_data_from_underlying_reader()
        {
            var bridgeDataReader = CreateNestedBridgeDataReader();

            var actualValues = new List<int>();
            do
            {
                while (bridgeDataReader.Read())
                {
                    actualValues.Add(bridgeDataReader.GetFieldValue<int>(0));
                }
            }
            while (bridgeDataReader.NextResult());

            Assert.Equal(new[] { 1, 2, 1, 3 }.ToList(), actualValues);
        }

        [Fact]
        public void NextResultAsync_ReadAsync_and_GetFieldValueAsync_return_data_from_underlying_reader()
        {
            var bridgeDataReader = CreateNestedBridgeDataReader();

            var actualValues = new List<int>();
            do
            {
                while (bridgeDataReader.ReadAsync().Result)
                {
                    actualValues.Add(bridgeDataReader.GetFieldValueAsync<int>(0).Result);
                }
            }
            while (bridgeDataReader.NextResultAsync().Result);

            Assert.Equal(new[] { 1, 2, 1, 3 }.ToList(), actualValues);
        }

#endif

        private BridgeDataReader CreateNestedBridgeDataReader(DbDataReader dataReader = null)
        {
            var sourceEnumerable1 = new[]
                                        {
                                            new object[] { 1 },
                                            new object[] { null },
                                            new object[] { 2 },
                                            new object[] { 2 }
                                        };

            var sourceEnumerable2 = new[]
                                        {
                                            new object[] { 1 },
                                            new object[] { 3 },
                                        };

            dataReader = dataReader
                         ?? Common.Internal.Materialization.MockHelper.CreateDbDataReader(sourceEnumerable1, sourceEnumerable2);

            var rootCoordinatorFactory = MockHelper.CreateCoordinatorFactory<int, RecordState>(
                depth: 0,
                stateSlot: 0,
                ordinal: 0,
                nestedCoordinators: new CoordinatorFactory[0],
                producedValues: null);

            var shaperMock = new Mock<Shaper<RecordState>>(
                dataReader,
                /*context*/ null,
                /*workspace*/ null,
                MergeOption.AppendOnly,
                /*stateCount*/ 2,
                rootCoordinatorFactory,
                /*readerOwned*/ false,
                /*useSpatialReader*/ false)
                                 {
                                     CallBase = true
                                 };

            var rootCoordinatorFactory2 = MockHelper.CreateCoordinatorFactory<int, RecordState>(
                depth: 0,
                stateSlot: 0,
                ordinal: 0,
                nestedCoordinators: new CoordinatorFactory[0],
                producedValues: null);

            var shaperMock2 = new Mock<Shaper<RecordState>>(
                dataReader,
                /*context*/ null,
                /*workspace*/ null,
                MergeOption.AppendOnly,
                /*stateCount*/ 2,
                rootCoordinatorFactory2,
                /*readerOwned*/ false,
                /*useSpatialReader*/ false)
                                  {
                                      CallBase = true
                                  };

            var nextResultInfo = new[]
                                     {
                                         new KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>>(
                                             shaperMock2.Object, rootCoordinatorFactory2)
                                     }.ToList();

            return new BridgeDataReader(shaperMock.Object, rootCoordinatorFactory, 0, nextResultInfo.GetEnumerator());
        }

        [Fact]
        public void CloseImplicitly_consumes_reader()
        {
            var readCalled = false;
            var nextResultCalled = false;

            var dbDataReaderMock = new Mock<DbDataReader>();
            dbDataReaderMock
                .Setup(m => m.Read())
                .Returns(
                    () =>
                        {
                            if (readCalled)
                            {
                                return false;
                            }
                            else
                            {
                                readCalled = true;
                                return true;
                            }
                        });

            dbDataReaderMock
                .Setup(m => m.NextResult())
                .Returns(
                    () =>
                        {
                            if (nextResultCalled)
                            {
                                return false;
                            }
                            else
                            {
                                nextResultCalled = true;
                                return true;
                            }
                        });

            var bridgeDataReader = CreateNestedBridgeDataReader(dbDataReaderMock.Object);

            bridgeDataReader.CloseImplicitly();

            Assert.True(bridgeDataReader.IsClosed);
            Assert.True(readCalled);
            Assert.False(nextResultCalled);
        }

#if !NET40

        [Fact]
        public void CloseImplicitlyAsync_consumes_reader()
        {
            var readCalled = false;
            var nextResultCalled = false;

            var dbDataReaderMock = new Mock<DbDataReader>();

            dbDataReaderMock
                .Setup(m => m.ReadAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken ct) =>
                        {
                            if (readCalled)
                            {
                                return Task.FromResult(false);
                            }
                            else
                            {
                                readCalled = true;
                                return Task.FromResult(true);
                            }
                        });

            dbDataReaderMock
                .Setup(m => m.NextResultAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken ct) =>
                        {
                            if (nextResultCalled)
                            {
                                return Task.FromResult(false);
                            }
                            else
                            {
                                nextResultCalled = true;
                                return Task.FromResult(true);
                            }
                        });

            var bridgeDataReader = CreateNestedBridgeDataReader(dbDataReaderMock.Object);

            bridgeDataReader.CloseImplicitlyAsync(CancellationToken.None).Wait();

            Assert.True(bridgeDataReader.IsClosed);
            Assert.True(readCalled);
            Assert.False(nextResultCalled);
        }

#endif
    }
}
