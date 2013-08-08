// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.QueryCache;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;

    internal static class MockHelper
    {
        public static Translator CreateRecordStateTranslator()
        {
            var shaperFactory = new ShaperFactory<RecordState>(
                2,
                Objects.MockHelper.CreateCoordinatorFactory<object, RecordState>(
                    0, 0, 0, new CoordinatorFactory[0], new List<RecordState>()),
                new Type[0], new bool[0], MergeOption.NoTracking);

            var translatorMock = new Mock<Translator>();
            translatorMock.Setup(
                m => m.TranslateColumnMap<RecordState>(It.IsAny<ColumnMap>(), It.IsAny<MetadataWorkspace>(),
                    It.IsAny<SpanIndex>(), It.IsAny<MergeOption>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(shaperFactory);

            return translatorMock.Object;
        }

        public static Translator CreateTranslator<T>() where T : class
        {
            var shaperFactory = new ShaperFactory<T>(
                1,
                Objects.MockHelper.CreateCoordinatorFactory<object, T>(0, 0, 0, new CoordinatorFactory[0], new List<T>()),
                new Type[0], new bool[0], MergeOption.AppendOnly);

            var translatorMock = new Mock<Translator>();
            translatorMock.Setup(
                m => m.TranslateColumnMap<T>(It.IsAny<ColumnMap>(), It.IsAny<MetadataWorkspace>(),
                    It.IsAny<SpanIndex>(), It.IsAny<MergeOption>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(shaperFactory);

            return translatorMock.Object;
        }

        public static DbDataReader CreateDbDataReader(params IEnumerable<object[]>[] sourceEnumerables)
        {
            if (sourceEnumerables.Length == 0)
            {
                sourceEnumerables = new[] { new object[0][] };
            }
            var underlyingEnumerators = new IEnumerator<object[]>[sourceEnumerables.Length];
            var fieldCounts = new int[sourceEnumerables.Length];
            for (var i = 0; i < sourceEnumerables.Length; i++)
            {
                underlyingEnumerators[i] = sourceEnumerables[i].GetEnumerator();
                fieldCounts[i] = sourceEnumerables[i].Count() > 0
                    ? sourceEnumerables[i].First().Count()
                    : 0;
            }

            var currentResultSet = 0;

            var dbDataReaderMock = new Mock<DbDataReader>() { CallBase = true };
            dbDataReaderMock
                .Setup(m => m.Read())
                .Returns(
                    () => underlyingEnumerators[currentResultSet].MoveNext());
#if !NET40

            dbDataReaderMock
                .Setup(m => m.ReadAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken ct) =>
                    Task.FromResult(underlyingEnumerators[currentResultSet].MoveNext()));

#endif

            dbDataReaderMock
                .Setup(m => m.NextResult())
                .Returns(
                    () =>
                    ++currentResultSet < underlyingEnumerators.Length);

#if !NET40

            dbDataReaderMock
                .Setup(m => m.NextResultAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken ct) =>
                    Task.FromResult(++currentResultSet < underlyingEnumerators.Length));

#endif

            dbDataReaderMock
                .Setup(m => m.GetValue(It.IsAny<int>()))
                .Returns(
                    (int ordinal) =>
                    underlyingEnumerators[currentResultSet].Current[ordinal]);

            dbDataReaderMock
                .Setup(m => m.GetValues(It.IsAny<object[]>()))
                .Returns(
                    (object[] result) =>
                        {
                            int i = 0;
                            for (; i < result.Length && i < underlyingEnumerators[currentResultSet].Current.Length; i++ )
                            {
                                result[i] = underlyingEnumerators[currentResultSet].Current[i];
                            }
                            return i;
                        });

            dbDataReaderMock
                .Setup(m => m.IsDBNull(It.IsAny<int>()))
                .Returns((int ordinal) => underlyingEnumerators[currentResultSet].Current[ordinal] == null);

#if !NET40

            dbDataReaderMock
                .Setup(m => m.GetFieldValueAsync<object>(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(
                    (int ordinal, CancellationToken ct) =>
                    Task.FromResult(underlyingEnumerators[currentResultSet].Current[ordinal]));

            dbDataReaderMock
                .Setup(m => m.IsDBNullAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns((int ordinal, CancellationToken ct) => Task.FromResult(underlyingEnumerators[currentResultSet].Current[ordinal] == null));

#endif

            dbDataReaderMock
                .Setup(m => m.FieldCount)
                .Returns(() => fieldCounts[currentResultSet]);

            dbDataReaderMock
                .Setup(m => m.GetName(It.IsAny<int>()))
                .Returns((int ordinal) => "column" + ordinal);

            var closed = false;

            dbDataReaderMock
                .Setup(m => m.IsClosed)
                .Returns(() => 
                    closed);

            dbDataReaderMock
                .Setup(m => m.Close())
                .Callback(() =>
                    closed = true);

            return dbDataReaderMock.Object;
        }
    }
}
