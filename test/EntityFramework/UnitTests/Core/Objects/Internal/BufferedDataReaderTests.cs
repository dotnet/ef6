// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;
    using Xunit;
#if !NET40

#endif

    public class BufferedDataReaderTests : TestBase
    {
        [Fact]
        public void Metadata_methods_return_expected_results_sync()
        {
            Metadata_methods_return_expected_results(false, false);
        }

#if !NET40
        [Fact]
        public void Metadata_methods_return_expected_results_async()
        {
            Metadata_methods_return_expected_results(true, false);
        }
#endif

        [Fact]
        public void Metadata_methods_return_expected_results_with_shape_sync()
        {
            Metadata_methods_return_expected_results(false, true);
        }

#if !NET40
        [Fact]
        public void Metadata_methods_return_expected_results_with_shape_async()
        {
            Metadata_methods_return_expected_results(true, true);
        }
#endif

        private void Metadata_methods_return_expected_results(bool async, bool shaped)
        {
            var reader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(new[] { new[] { new object() } });
            var readerMock = Mock.Get(reader);
            readerMock.Setup(m => m.RecordsAffected).Returns(2);
            readerMock.Setup(m => m.GetOrdinal(It.IsAny<string>())).Returns(3);
            readerMock.Setup(m => m.GetDataTypeName(It.IsAny<int>())).Returns("dataTypeName");
            readerMock.Setup(m => m.GetFieldType(It.IsAny<int>())).Returns(typeof(DBNull));
            readerMock.Setup(m => m.GetName(It.IsAny<int>())).Returns("columnName");

            var columnTypes = new[] { typeof(object) };
            var nullableColumns = new[] { true };
            var bufferedDataReader = new BufferedDataReader(reader);
            if (async)
            {
#if !NET40
                if (shaped)
                {
                    bufferedDataReader.InitializeAsync(
                        "2008", FakeSqlProviderServices.Instance, columnTypes, nullableColumns, CancellationToken.None).Wait();
                }
                else
                {
                    bufferedDataReader.InitializeAsync("2008", FakeSqlProviderServices.Instance, null, null, CancellationToken.None).Wait();
                }
#endif
            }
            else
            {
                if (shaped)
                {
                    bufferedDataReader.Initialize("2008", FakeSqlProviderServices.Instance, columnTypes, nullableColumns);
                }
                else
                {
                    bufferedDataReader.Initialize("2008", FakeSqlProviderServices.Instance, null, null);
                }
            }

            Assert.Equal(1, bufferedDataReader.FieldCount);
            Assert.Equal(0, bufferedDataReader.GetOrdinal("columnName"));
            Assert.Equal("dataTypeName", bufferedDataReader.GetDataTypeName(0));
            Assert.Equal(typeof(DBNull), bufferedDataReader.GetFieldType(0));
            Assert.Equal("columnName", bufferedDataReader.GetName(0));
            Assert.Throws<NotSupportedException>(() => bufferedDataReader.Depth);
            Assert.Throws<NotSupportedException>(() => bufferedDataReader.GetSchemaTable());

            bufferedDataReader.Close();
            Assert.Equal(2, bufferedDataReader.RecordsAffected);
        }

        [Fact]
        public void Metadata_methods_throw_if_reader_is_closed()
        {
            var reader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(new[] { new[] { new object() } });

            var bufferedDataReader = new BufferedDataReader(reader);
            bufferedDataReader.Initialize("2008", FakeSqlProviderServices.Instance, null, null);

            bufferedDataReader.Close();
            Assert.Equal(
                Strings.ADP_ClosedDataReaderError,
                Assert.Throws<InvalidOperationException>(() => bufferedDataReader.FieldCount).Message);
            Assert.Equal(
                Strings.ADP_ClosedDataReaderError,
                Assert.Throws<InvalidOperationException>(() => bufferedDataReader.GetOrdinal("columnName")).Message);
            Assert.Equal(
                Strings.ADP_ClosedDataReaderError,
                Assert.Throws<InvalidOperationException>(() => bufferedDataReader.GetDataTypeName(0)).Message);
            Assert.Equal(
                Strings.ADP_ClosedDataReaderError,
                Assert.Throws<InvalidOperationException>(() => bufferedDataReader.GetFieldType(0)).Message);
            Assert.Equal(
                Strings.ADP_ClosedDataReaderError,
                Assert.Throws<InvalidOperationException>(() => bufferedDataReader.GetName(0)).Message);
        }

        [Fact]
        public void Manipulation_methods_perform_expected_actions_sync()
        {
            Manipulation_methods_perform_expected_actions(spatial: false, async: false, shaped: false);
        }

        [Fact]
        public void Manipulation_methods_perform_expected_actions_with_spatial_sync()
        {
            Manipulation_methods_perform_expected_actions(spatial: true, async: false, shaped: false);
        }

        [Fact]
        public void Manipulation_methods_perform_expected_actions_with_shape_sync()
        {
            Manipulation_methods_perform_expected_actions(spatial: false, async: false, shaped: true);
        }

        [Fact]
        public void Manipulation_methods_perform_expected_actions_with_shape_and_spatial_sync()
        {
            Manipulation_methods_perform_expected_actions(spatial: true, async: false, shaped: true);
        }

#if !NET40
        [Fact]
        public void Manipulation_methods_perform_expected_actions_async()
        {
            Manipulation_methods_perform_expected_actions(spatial: false, async: true, shaped: false);
        }

        [Fact]
        public void Manipulation_methods_perform_expected_actions_with_spatial_async()
        {
            Manipulation_methods_perform_expected_actions(spatial: true, async: true, shaped: false);
        }

        [Fact]
        public void Manipulation_methods_perform_expected_actions_with_shape_async()
        {
            Manipulation_methods_perform_expected_actions(spatial: false, async: true, shaped: true);
        }

        [Fact]
        public void Manipulation_methods_perform_expected_actions_with_shape_and_spatial_async()
        {
            Manipulation_methods_perform_expected_actions(spatial: true, async: true, shaped: true);
        }
#endif

        private void Manipulation_methods_perform_expected_actions(bool spatial, bool async, bool shaped)
        {
            var reader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(
                new[] { new object[] { 1, "a" } }, new object[0][]);

            var bufferedDataReader = new BufferedDataReader(reader);

            var columnTypes = new[] { typeof(int), typeof(object) };
            var nullableColumns = new[] { false, true };

            var spatialDataReaderMock = new Mock<DbSpatialDataReader>();
            var providerServicesMock = new Mock<DbProviderServices>();
            if (spatial)
            {
                columnTypes = new[] { typeof(DbGeography), typeof(DbGeometry) };
                nullableColumns = new[] { true, true };

                spatialDataReaderMock.Setup(m => m.IsGeographyColumn(0)).Returns(true);
                spatialDataReaderMock.Setup(m => m.IsGeometryColumn(1)).Returns(true);
#if !NET40
                if (async)
                {
                    spatialDataReaderMock.Setup(m => m.GetGeographyAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                        .Returns(() => Task.FromResult((DbGeography)null));
                    spatialDataReaderMock.Setup(m => m.GetGeometryAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                        .Returns(() => Task.FromResult((DbGeometry)null));
                }
#endif
                providerServicesMock.Protected()
                    .Setup<DbSpatialDataReader>("GetDbSpatialDataReader", reader, "2008")
                    .Returns(spatialDataReaderMock.Object);
            }
            try
            {
                Assert.False(bufferedDataReader.IsClosed);
                if (async)
                {
#if !NET40
                    if (shaped)
                    {
                        bufferedDataReader.InitializeAsync(
                            "2008", providerServicesMock.Object, columnTypes, nullableColumns, CancellationToken.None).Wait();
                    }
                    else
                    {
                        bufferedDataReader.InitializeAsync("2008", providerServicesMock.Object, null, null, CancellationToken.None).Wait();
                    }
#endif
                }
                else
                {
                    if (shaped)
                    {
                        bufferedDataReader.Initialize("2008", providerServicesMock.Object, columnTypes, nullableColumns);
                    }
                    else
                    {
                        bufferedDataReader.Initialize("2008", providerServicesMock.Object, null, null);
                    }
                }
                Assert.False(bufferedDataReader.IsClosed);
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }

            if (spatial)
            {
                if (async)
                {
#if !NET40
                    spatialDataReaderMock.Verify(m => m.GetGeographyAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once());
                    spatialDataReaderMock.Verify(m => m.GetGeometryAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once());
#endif
                }
                else
                {
                    spatialDataReaderMock.Verify(m => m.GetGeography(It.IsAny<int>()), Times.Once());
                    spatialDataReaderMock.Verify(m => m.GetGeometry(It.IsAny<int>()), Times.Once());
                }
            }
            Assert.True(bufferedDataReader.HasRows);

            if (async)
            {
#if !NET40
                Assert.True(bufferedDataReader.ReadAsync().Result);
                Assert.False(bufferedDataReader.ReadAsync().Result);
#endif
            }
            else
            {
                Assert.True(bufferedDataReader.Read());
                Assert.False(bufferedDataReader.Read());
            }

            Assert.True(bufferedDataReader.HasRows);

            if (async)
            {
#if !NET40
                Assert.True(bufferedDataReader.NextResultAsync().Result);
#endif
            }
            else
            {
                Assert.True(bufferedDataReader.NextResult());
            }

            Assert.False(bufferedDataReader.HasRows);

            if (async)
            {
#if !NET40
                Assert.False(bufferedDataReader.ReadAsync().Result);
                Assert.False(bufferedDataReader.NextResultAsync().Result);
#endif
            }
            else
            {
                Assert.False(bufferedDataReader.Read());
                Assert.False(bufferedDataReader.NextResult());
            }

            Assert.False(bufferedDataReader.IsClosed);
            bufferedDataReader.Close();
            Assert.True(bufferedDataReader.IsClosed);
        }

        [Fact]
        public void GetEnumerator_returns_rows_from_the_current_result_set()
        {
            var reader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(
                new[] { new object[] { 1, "a" }, new object[] { 2, "b" } },
                new[] { new object[] { 3, "c" } });

            var bufferedReader = new BufferedDataReader(reader);
            bufferedReader.Initialize("2008", FakeSqlProviderServices.Instance, null, null);

            var enumerator = bufferedReader.GetEnumerator();

            var list = new List<object>();
            do
            {
                while (enumerator.MoveNext())
                {
                    var dataRecord = (DbDataRecord)enumerator.Current;
                    for (var i = 0; i < dataRecord.FieldCount; i++)
                    {
                        list.Add(dataRecord.GetValue(i));
                    }
                }
            }
            while (bufferedReader.NextResult());

            Assert.Equal(new object[] { 1, "a", 2, "b", 3, "c" }, list);
        }

        [Fact]
        public void Close_disposes_underlying_reader()
        {
            var reader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(new[] { new[] { new object() } });
            var bufferedReader = new BufferedDataReader(reader);

            Assert.False(reader.IsClosed);
            bufferedReader.Close();
            Assert.True(reader.IsClosed);
        }

        [Fact]
        public void Initialize_is_idempotent()
        {
            var reader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(new[] { new[] { new object() } });
            var bufferedReader = new BufferedDataReader(reader);

            Assert.False(reader.IsClosed);
            bufferedReader.Initialize("2008", FakeSqlProviderServices.Instance, null, null);
            Assert.True(reader.IsClosed);
            bufferedReader.Initialize("2008", FakeSqlProviderServices.Instance, null, null);
        }

#if !NET40
        [Fact]
        public void InitializeAsync_is_idempotent()
        {
            var reader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(new[] { new[] { new object() } });
            var bufferedReader = new BufferedDataReader(reader);

            Assert.False(reader.IsClosed);
            bufferedReader.InitializeAsync("2008", FakeSqlProviderServices.Instance, null, null, CancellationToken.None).Wait();
            Assert.True(reader.IsClosed);
            bufferedReader.InitializeAsync("2008", FakeSqlProviderServices.Instance, null, null, CancellationToken.None).Wait();
        }
#endif

        [Fact]
        public void Data_methods_return_expected_results_sync()
        {
            Data_methods_return_expected_results(async: false, shaped: false);
        }

        [Fact]
        public void Data_methods_return_expected_results_with_shape_sync()
        {
            Data_methods_return_expected_results(async: false, shaped: true);
        }

#if !NET40
        [Fact]
        public void Data_methods_return_expected_results_async()
        {
            Data_methods_return_expected_results(async: true, shaped: false);
        }

        [Fact]
        public void Data_methods_return_expected_results_with_shape__async()
        {
            Data_methods_return_expected_results(async: true, shaped: true);
        }
#endif

        private void Data_methods_return_expected_results(bool async, bool shaped)
        {
            Verify_get_method_returns_supplied_value(true, async, shaped);
            Verify_get_method_returns_supplied_value((byte)1, async, shaped);
            Verify_get_method_returns_supplied_value((short)1, async, shaped);
            Verify_get_method_returns_supplied_value(1, async, shaped);
            Verify_get_method_returns_supplied_value(1L, async, shaped);
            Verify_get_method_returns_supplied_value(1F, async, shaped);
            Verify_get_method_returns_supplied_value(1D, async, shaped);
            Verify_get_method_returns_supplied_value(1M, async, shaped);
            Verify_get_method_returns_supplied_value('a', async, shaped);
            Verify_get_method_returns_supplied_value("a", async, shaped);
            Verify_get_method_returns_supplied_value(DateTime.Now, async, shaped);
            Verify_get_method_returns_supplied_value(Guid.NewGuid(), async, shaped);
            var obj = new object();
            Verify_method_result(r => r.GetValue(0), async, shaped, obj, new[] { new[] { obj } });
#if !NET40
            Verify_method_result(r => r.GetFieldValue<object>(0), async, shaped, obj, new[] { new[] { obj } });
            Verify_method_result(r => r.GetFieldValueAsync<object>(0).Result, async, shaped, obj, new[] { new[] { obj } });
#endif
            Verify_method_result(r => r.IsDBNull(0), async, shaped, true, new[] { new object[] { DBNull.Value } });
            Verify_method_result(r => r.IsDBNull(0), async, shaped, false, new[] { new object[] { true } });
#if !NET40
            Verify_method_result(r => r.IsDBNullAsync(0).Result, async, shaped, true, new[] { new object[] { DBNull.Value } });
            Verify_method_result(r => r.IsDBNullAsync(0).Result, async, shaped, false, new[] { new object[] { true } });
#endif
            Assert.Throws<NotSupportedException>(
                () =>
                Verify_method_result(r => r.GetBytes(0, 0, new byte[0], 0, 0), async, shaped, 0, new[] { new object[] { 1L } }));
            Assert.Throws<NotSupportedException>(
                () =>
                Verify_method_result(r => r.GetChars(0, 0, new char[0], 0, 0), async, shaped, 0, new[] { new object[] { 1L } }));
        }

        private void Verify_method_result<T>(
            Func<BufferedDataReader, T> method, bool async, bool shaped, T expectedResult, params IEnumerable<object[]>[] dataReaderContents)
        {
            var reader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(dataReaderContents);

            var columnTypes = new[] { typeof(T) };
            var nullableColumns = new[] { true };
            var bufferedReader = new BufferedDataReader(reader);
            if (async)
            {
#if !NET40
                if (shaped)
                {
                    bufferedReader.InitializeAsync(
                        "2008", FakeSqlProviderServices.Instance, columnTypes, nullableColumns, CancellationToken.None).Wait();
                }
                else
                {
                    bufferedReader.InitializeAsync("2008", FakeSqlProviderServices.Instance, null, null, CancellationToken.None).Wait();
                }
                Assert.True(bufferedReader.ReadAsync().Result);
#endif
            }
            else
            {
                if (shaped)
                {
                    bufferedReader.Initialize("2008", FakeSqlProviderServices.Instance, columnTypes, nullableColumns);
                }
                else
                {
                    bufferedReader.Initialize("2008", FakeSqlProviderServices.Instance, null, null);
                }
                Assert.True(bufferedReader.Read());
            }

            Assert.Equal(expectedResult, method(bufferedReader));
        }

        private void Verify_get_method_returns_supplied_value<T>(T value, bool async, bool shaped)
        {
            // use the specific reader.GetXXX method
            var readerMethod = GetReaderMethod(typeof(T));
            Verify_method_result(r => (T)readerMethod.Invoke(r, new object[] { 0 }), async, shaped, value, new[] { new object[] { value } });
        }

        private static MethodInfo GetReaderMethod(Type type)
        {
            if (type == typeof(char))
            {
                return typeof(DbDataReader).GetMethod("GetChar");
            }
            bool isNullable;
            return CodeGenEmitter.GetReaderMethod(type, out isNullable);
        }

        [Fact]
        public void Data_methods_throw_exception_if_no_data_present()
        {
            Verify_method_throws_when_no_data(r => r.GetBoolean(0), new[] { new object[] { true } });
            Verify_method_throws_when_no_data(r => r.GetByte(0), new[] { new object[] { (byte)1 } });
            Verify_method_throws_when_no_data(r => r.GetInt16(0), new[] { new object[] { (short)1 } });
            Verify_method_throws_when_no_data(r => r.GetInt32(0), new[] { new object[] { 1 } });
            Verify_method_throws_when_no_data(r => r.GetInt64(0), new[] { new object[] { 1L } });
            Verify_method_throws_when_no_data(r => r.GetFloat(0), new[] { new object[] { 1F } });
            Verify_method_throws_when_no_data(r => r.GetDouble(0), new[] { new object[] { 1D } });
            Verify_method_throws_when_no_data(r => r.GetDecimal(0), new[] { new object[] { 1M } });
            Verify_method_throws_when_no_data(r => r.GetChar(0), new[] { new object[] { 'a' } });
            Verify_method_throws_when_no_data(r => r.GetString(0), new[] { new object[] { "a" } });
            Verify_method_throws_when_no_data(r => r.GetDateTime(0), new[] { new object[] { DateTime.Now } });
            Verify_method_throws_when_no_data(r => r.GetGuid(0), new[] { new object[] { Guid.NewGuid() } });

            var obj = new object();
            Verify_method_throws_when_no_data(r => r.GetValue(0), new[] { new[] { obj } });
#if !NET40
            Verify_method_throws_when_no_data(r => r.GetFieldValue<object>(0), new[] { new[] { obj } });
            Verify_method_throws_when_no_data(r => r.GetFieldValueAsync<object>(0).Result, new[] { new[] { obj } });
#endif
            Verify_method_throws_when_no_data(r => r.IsDBNull(0), new[] { new object[] { DBNull.Value } });
            Verify_method_throws_when_no_data(r => r.IsDBNull(0), new[] { new[] { new object() } });
#if !NET40
            Verify_method_throws_when_no_data(r => r.IsDBNullAsync(0).Result, new[] { new object[] { DBNull.Value } });
            Verify_method_throws_when_no_data(r => r.IsDBNullAsync(0).Result, new[] { new[] { new object() } });
#endif
        }

        private void Verify_method_throws_when_no_data<T>(
            Func<BufferedDataReader, T> method, params IEnumerable<object[]>[] dataReaderContents)
        {
            var reader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(dataReaderContents);

            var bufferedReader = new BufferedDataReader(reader);
            bufferedReader.Initialize("2008", FakeSqlProviderServices.Instance, null, null);

            Assert.Equal(
                Strings.ADP_NoData,
                Assert.Throws<InvalidOperationException>(() => method(bufferedReader)).Message);
        }
    }
}
