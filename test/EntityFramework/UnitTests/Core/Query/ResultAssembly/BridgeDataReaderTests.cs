namespace System.Data.Entity.Core.Query.ResultAssembly
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    using Moq;
    using Xunit;

    public class BridgeDataReaderTests
    {
        [Fact]
        public void Read_and_GetFieldValue_return_data_from_underlying_reader()
        {
            var sourceEnumerable = new[] {
                                           new object[] { 1 },
                                           new object[] { null },
                                           new object[] { 2 },
                                           new object[] { 2 }
                                       };

            var underlyingEnumerator = ((IEnumerable<object[]>)sourceEnumerable).GetEnumerator();

            var dbDataReaderMock = new Mock<DbDataReader>();
            dbDataReaderMock.Setup(m => m.Read()).Returns(underlyingEnumerator.MoveNext);
            dbDataReaderMock.Setup(m => m.GetValue(It.IsAny<int>())).Returns((int ordinal) => underlyingEnumerator.Current[ordinal]);
            dbDataReaderMock.Setup(m => m.IsDBNull(It.IsAny<int>())).Returns((int ordinal) => underlyingEnumerator.Current[ordinal] == null);

            var rootCoordinatorFactory = Objects.MockHelper.CreateCoordinatorFactory<int, RecordState>(
                depth: 0,
                stateSlot: 0,
                ordinal: 0,
                nestedCoordinators: new CoordinatorFactory[0],
                producedValues: null);

            var shaperMock = new Mock<Shaper<RecordState>>(dbDataReaderMock.Object, /*context*/ null, /*workspace*/ null,
                MergeOption.AppendOnly, /*stateCount*/ 2, rootCoordinatorFactory, /*checkPermissions*/ null,
                /*readerOwned*/ false) { CallBase = true };

            var bridgeDataReader = new BridgeDataReader(shaperMock.Object, rootCoordinatorFactory, 0, null);

            var actualValues = new List<int>();
            while (bridgeDataReader.Read())
            {
                actualValues.Add(bridgeDataReader.GetFieldValue<int>(0));
            }

            Assert.Equal(new [] { 1, 2 }.ToList(), actualValues);

        }
    }
}