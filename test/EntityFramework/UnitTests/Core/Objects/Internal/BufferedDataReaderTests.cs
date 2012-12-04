// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Reflection;
    using System.Threading;
    using Moq;
    using Xunit;

    public class BufferedDataReaderTests
    {
        [Fact]
        public void Metadata_methods_return_expected_results_sync()
        {
            Metadata_methods_return_expected_results(false);
        }

        [Fact]
        public void Metadata_methods_return_expected_results_async()
        {
            Metadata_methods_return_expected_results(true);
        }

        private void Metadata_methods_return_expected_results(bool async)
        {
            var reader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(new[] { new[] { new object() } });
            var readerMock = Mock.Get(reader);
            readerMock.Setup(m => m.RecordsAffected).Returns(2);
            readerMock.Setup(m => m.GetOrdinal(It.IsAny<string>())).Returns(3);
            readerMock.Setup(m => m.GetDataTypeName(It.IsAny<int>())).Returns("dataTypeName");
            readerMock.Setup(m => m.GetFieldType(It.IsAny<int>())).Returns(typeof(DBNull));
            readerMock.Setup(m => m.GetName(It.IsAny<int>())).Returns("columnName");

            var bufferedDataReader = new BufferedDataReader(reader);
            if (async)
            {
                bufferedDataReader.InitializeAsync(CancellationToken.None).Wait();
            }
            else
            {
                bufferedDataReader.Initialize();
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
            bufferedDataReader.Initialize();

            bufferedDataReader.Close();

            Assert.Throws<InvalidOperationException>(() => bufferedDataReader.FieldCount)
                .ValidateMessage("ADP_ClosedDataReaderError");
            Assert.Throws<InvalidOperationException>(() => bufferedDataReader.GetOrdinal("columnName"))
                .ValidateMessage("ADP_ClosedDataReaderError");
            Assert.Throws<InvalidOperationException>(() => bufferedDataReader.GetDataTypeName(0))
                .ValidateMessage("ADP_ClosedDataReaderError");
            Assert.Throws<InvalidOperationException>(() => bufferedDataReader.GetFieldType(0))
                .ValidateMessage("ADP_ClosedDataReaderError");
            Assert.Throws<InvalidOperationException>(() => bufferedDataReader.GetName(0))
                .ValidateMessage("ADP_ClosedDataReaderError");
        }

        [Fact]
        public void Manipulation_methods_perform_expected_actions_sync()
        {
            Manipulation_methods_perform_expected_actions(false);
        }

        [Fact]
        public void Manipulation_methods_perform_expected_actions_async()
        {
            Manipulation_methods_perform_expected_actions(true);
        }

        private void Manipulation_methods_perform_expected_actions(bool async)
        {
            var reader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(
                new[] { new object[] { 1, "a" } }, new object[0][]);

            var bufferedDataReader = new BufferedDataReader(reader);

            Assert.False(bufferedDataReader.IsClosed);
            if (async)
            {
                bufferedDataReader.InitializeAsync(CancellationToken.None).Wait();
            }
            else
            {
                bufferedDataReader.Initialize();
            }
            Assert.False(bufferedDataReader.IsClosed);

            Assert.True(bufferedDataReader.HasRows);

            if (async)
            {
                Assert.True(bufferedDataReader.ReadAsync().Result);
                Assert.False(bufferedDataReader.ReadAsync().Result);
            }
            else
            {
                Assert.True(bufferedDataReader.Read());
                Assert.False(bufferedDataReader.Read());
            }

            Assert.True(bufferedDataReader.HasRows);

            if (async)
            {
                Assert.True(bufferedDataReader.NextResultAsync().Result);
            }
            else
            {
                Assert.True(bufferedDataReader.NextResult());
            }

            Assert.False(bufferedDataReader.HasRows);

            if (async)
            {
                Assert.False(bufferedDataReader.ReadAsync().Result);
                Assert.False(bufferedDataReader.NextResultAsync().Result);
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
            bufferedReader.Initialize();

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
            bufferedReader.Initialize();
            Assert.True(reader.IsClosed);
            bufferedReader.Initialize();
        }

        [Fact]
        public void InitializeAsync_is_idempotent()
        {
            var reader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(new[] { new[] { new object() } });
            var bufferedReader = new BufferedDataReader(reader);

            Assert.False(reader.IsClosed);
            bufferedReader.InitializeAsync(CancellationToken.None);
            Assert.True(reader.IsClosed);
            bufferedReader.InitializeAsync(CancellationToken.None);
        }

        [Fact]
        public void Data_methods_return_expected_results_sync()
        {
            Data_methods_return_expected_results(false);
        }

        [Fact]
        public void Data_methods_return_expected_results_async()
        {
            Data_methods_return_expected_results(true);
        }

        private void Data_methods_return_expected_results(bool async)
        {
            Verify_get_method_returns_supplied_value(true, async);
            Verify_get_method_returns_supplied_value((byte)1, async);
            Verify_get_method_returns_supplied_value((short)1, async);
            Verify_get_method_returns_supplied_value(1, async);
            Verify_get_method_returns_supplied_value(1L, async);
            Verify_get_method_returns_supplied_value(1F, async);
            Verify_get_method_returns_supplied_value(1D, async);
            Verify_get_method_returns_supplied_value(1M, async);
            Verify_get_method_returns_supplied_value('a', async);
            Verify_get_method_returns_supplied_value("a", async);
            Verify_get_method_returns_supplied_value(DateTime.Now, async);
            Verify_get_method_returns_supplied_value(Guid.NewGuid(), async);
            var obj = new object();
            Verify_method_result(r => r[0], async, obj, new[] { new[] { obj } });
            Verify_method_result(r => r["column0"], async, obj, new[] { new[] { obj } });
            Verify_method_result(r => r.GetValue(0), async, obj, new[] { new[] { obj } });
#if !NET40
            Verify_method_result(r => r.GetFieldValue<object>(0), async, obj, new[] { new[] { obj } });
            Verify_method_result(r => r.GetFieldValueAsync<object>(0).Result, async, obj, new[] { new[] { obj } });
#endif
            Verify_method_result(r => r.IsDBNull(0), async, true, new[] { new object[] { DBNull.Value } });
            Verify_method_result(r => r.IsDBNull(0), async, false, new[] { new[] { new object() } });
#if !NET40
            Verify_method_result(r => r.IsDBNullAsync(0).Result, async, true, new[] { new object[] { DBNull.Value } });
            Verify_method_result(r => r.IsDBNullAsync(0).Result, async, false, new[] { new[] { new object() } });
#endif
            Assert.Throws<NotSupportedException>(
                () =>
                Verify_method_result(r => r.GetBytes(0, 0, new byte[0], 0, 0), async, 0, new[] { new[] { obj } }));
            Assert.Throws<NotSupportedException>(
                () =>
                Verify_method_result(r => r.GetChars(0, 0, new char[0], 0, 0), async, 0, new[] { new[] { obj } }));
        }

        private void Verify_method_result<T>(
            Func<BufferedDataReader, T> method, bool async, T expectedResult, params IEnumerable<object[]>[] dataReaderContents)
        {
            var reader = Common.Internal.Materialization.MockHelper.CreateDbDataReader(dataReaderContents);

            var bufferedReader = new BufferedDataReader(reader);
            if (async)
            {
                bufferedReader.InitializeAsync(CancellationToken.None).Wait();
                Assert.True(bufferedReader.ReadAsync().Result);
            }
            else
            {
                bufferedReader.Initialize();
                Assert.True(bufferedReader.Read());
            }

            Assert.Equal(expectedResult, method(bufferedReader));
        }

        private void Verify_get_method_returns_supplied_value<T>(T value, bool async)
        {
            // use the specific reader.GetXXX method
            var readerMethod = GetReaderMethod(typeof(T));
            Verify_method_result(r => (T)readerMethod.Invoke(r, new object[] { 0 }), async, value, new[] { new object[] { value } });
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
            Verify_method_throws_when_no_data(r => r[0], new[] { new[] { obj } });
            Verify_method_throws_when_no_data(r => r["column0"], new[] { new[] { obj } });
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
            bufferedReader.Initialize();

            Assert.Throws<InvalidOperationException>(() => method(bufferedReader)).ValidateMessage("ADP_NoData");
        }
    }
}
