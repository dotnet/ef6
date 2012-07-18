namespace System.Data.Entity.Core.Query
{
    using System;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Query.ResultAssembly;
    using Moq;
    using Xunit;

    public class BridgeDataRecordTests
    {
        [Fact]
        public void GetValues_throws_for_null_argument()
        {
            var dbDataReaderMock = new Mock<DbDataReader>();

            var coordinatorFactory = Objects.MockHelper.CreateCoordinatorFactory<RecordState>(shaper => null);

            var shaperMock = new Mock<Shaper<RecordState>>(dbDataReaderMock.Object, /*context*/ null, /*workspace*/ null,
                MergeOption.AppendOnly, /*stateCount*/ 1, coordinatorFactory, /*checkPermissions*/ null,
                /*readerOwned*/ false) { CallBase = true };

            var bridgeDataRecord = new BridgeDataRecord(shaperMock.Object, 0);

            Assert.Equal("values",
                Assert.Throws<ArgumentNullException>(
                    () => bridgeDataRecord.GetValues(null)).ParamName);
        }
    }
}
