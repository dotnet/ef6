namespace System.Data.Entity.Core.EntityClient
{
    using System.Collections;
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using Moq;
    using Xunit;

    public class EntityDataReaderTests
    {
        public class DelegationToStoreReader
        {
            [Fact]
            public void Properties_delegate_to_underlying_store_reader_correctly()
            {
                VerifyGetter(r => r.Depth, m => m.Depth);
                VerifyGetter(r => r.FieldCount, m => m.FieldCount);
                VerifyGetter(r => r.HasRows, m => m.HasRows);
                VerifyGetter(r => r.IsClosed, m => m.IsClosed);
                VerifyGetter(r => r.RecordsAffected, m => m.RecordsAffected);
                VerifyGetter(r => r.VisibleFieldCount, m => m.VisibleFieldCount);
            }

            [Fact]
            public void Methods_delegate_to_underlying_store_reader_correctly()
            {
                VerifyMethod(r => r.GetBoolean(default(int)), m => m.GetBoolean(It.IsAny<int>()));
                VerifyMethod(
                    r => r.GetBytes(default(int), default(long), default(byte[]), default(int), default(int)),
                    m => m.GetBytes(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()));
                VerifyMethod(r => r.GetChar(default(int)), m => m.GetChar(It.IsAny<int>()));
                VerifyMethod(
                    r => r.GetChars(default(int), default(long), default(char[]), default(int), default(int)),
                    m => m.GetChars(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<char[]>(), It.IsAny<int>(), It.IsAny<int>()));
                VerifyMethod(r => r.GetDataTypeName(default(int)), m => m.GetDataTypeName(It.IsAny<int>()));
                VerifyMethod(r => r.GetDateTime(default(int)), m => m.GetDateTime(It.IsAny<int>()));
                VerifyMethod(r => r.GetDecimal(default(int)), m => m.GetDecimal(It.IsAny<int>()));
                VerifyMethod(r => r.GetDouble(default(int)), m => m.GetDouble(It.IsAny<int>()));
                VerifyMethod(r => r.GetFieldType(default(int)), m => m.GetFieldType(It.IsAny<int>()));
                VerifyMethod(r => r.GetFloat(default(int)), m => m.GetFloat(It.IsAny<int>()));
                VerifyMethod(r => r.GetGuid(default(int)), m => m.GetGuid(It.IsAny<int>()));
                VerifyMethod(r => r.GetInt16(default(int)), m => m.GetInt16(It.IsAny<int>()));
                VerifyMethod(r => r.GetInt32(default(int)), m => m.GetInt32(It.IsAny<int>()));
                VerifyMethod(r => r.GetInt64(default(int)), m => m.GetInt64(It.IsAny<int>()));
                VerifyMethod(r => r.GetName(default(int)), m => m.GetName(It.IsAny<int>()));
                VerifyMethod(r => r.GetOrdinal("Foo"), m => m.GetOrdinal(It.IsAny<string>()));
                VerifyMethod(r => r.GetProviderSpecificFieldType(default(int)), m => m.GetProviderSpecificFieldType(It.IsAny<int>()));
                VerifyMethod(r => r.GetProviderSpecificValue(default(int)), m => m.GetProviderSpecificValue(It.IsAny<int>()));
                VerifyMethod(r => r.GetProviderSpecificValues(default(object[])), m => m.GetProviderSpecificValues(It.IsAny<object[]>()));
                VerifyMethod(r => r.GetString(default(int)), m => m.GetString(It.IsAny<int>()));
                VerifyMethod(r => r.GetValue(default(int)), m => m.GetValue(It.IsAny<int>()));
                VerifyMethod(r => r.GetValues(Enumerable.Empty<object>().ToArray()), m => m.GetValues(It.IsAny<object[]>()));
                VerifyMethod(r => r.IsDBNull(default(int)), m => m.IsDBNull(It.IsAny<int>()));
                VerifyMethod(r => r.NextResult(), m => m.NextResult());
                VerifyMethod(r => r.NextResultAsync(), m => m.NextResultAsync(It.IsAny<CancellationToken>()));
                VerifyMethod(r => r.NextResultAsync(default(CancellationToken)), m => m.NextResultAsync(It.IsAny<CancellationToken>()));
                VerifyMethod(r => r.Read(), m => m.Read());
                VerifyMethod(r => r.ReadAsync(), m => m.ReadAsync(It.IsAny<CancellationToken>()));
                VerifyMethod(r => r.ReadAsync(default(CancellationToken)), m => m.ReadAsync(It.IsAny<CancellationToken>()));
                VerifyMethod(r => r.GetEnumerator(), m => m.GetEnumerator());
            }

            private void VerifyGetter<TProperty>(
                Func<EntityDataReader, TProperty> getterFunc,
                Expression<Func<DbDataReader, TProperty>> mockGetterFunc)
            {
                Assert.NotNull(getterFunc);
                Assert.NotNull(mockGetterFunc);

                var dbDataReaderMock = new Mock<DbDataReader>();
                var entityDataReader = new EntityDataReader(new EntityCommand(), dbDataReaderMock.Object, CommandBehavior.Default);

                getterFunc(entityDataReader);
                dbDataReaderMock.VerifyGet(mockGetterFunc, Times.Once());
            }

            private void VerifyMethod(Action<EntityDataReader> methodInvoke, Expression<Action<DbDataReader>> mockMethodInvoke)
            {
                Assert.NotNull(methodInvoke);
                Assert.NotNull(mockMethodInvoke);

                var dbDataReaderMock = new Mock<DbDataReader>();
                dbDataReaderMock.Setup(m => m.FieldCount).Returns(1);
                dbDataReaderMock.Setup(m => m.GetEnumerator()).Returns(new Mock<IEnumerator>().Object);
                var entityDataReader = new EntityDataReader(new EntityCommand(), dbDataReaderMock.Object, CommandBehavior.Default);

                methodInvoke(entityDataReader);
                dbDataReaderMock.Verify(mockMethodInvoke, Times.Once());
            }
        }

        [Fact]
        public void NextResult_wraps_exception_into_EntityCommandExecutionException_if_one_was_thrown()
        {
            var dbDataReaderMock = new Mock<DbDataReader>();
            dbDataReaderMock.Setup(m => m.NextResult()).Throws<InvalidOperationException>();
            var entityDataReader = new EntityDataReader(new EntityCommand(), dbDataReaderMock.Object, CommandBehavior.Default);

            Assert.Equal(
                Strings.EntityClient_StoreReaderFailed,
                Assert.Throws<EntityCommandExecutionException>(() => entityDataReader.NextResult()).Message);
        }

        [Fact]
        public void NextResultAsync_wraps_exception_into_EntityCommandExecutionException_if_one_was_thrown()
        {
            var dbDataReaderMock = new Mock<DbDataReader>();
            dbDataReaderMock.Setup(m => m.NextResultAsync(It.IsAny<CancellationToken>())).Throws<InvalidOperationException>();
            var entityDataReader = new EntityDataReader(new EntityCommand(), dbDataReaderMock.Object, CommandBehavior.Default);

            AssertThrowsInAsyncMethod<EntityCommandExecutionException>(
                Strings.EntityClient_StoreReaderFailed,
                () => entityDataReader.NextResultAsync().Wait());
        }

        private static void AssertThrowsInAsyncMethod<TException>(string expectedMessage, Xunit.Assert.ThrowsDelegate testCode)
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
    }
}
