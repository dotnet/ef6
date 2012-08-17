// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query
{
    using System;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Query.ResultAssembly;
    using System.Threading;
    using Moq;
    using Xunit;

    public class BridgeDataRecordTests
    {
        [Fact]
        public void GetValues_throws_for_null_argument()
        {
            var bridgeDataRecord = CreateBridgeDataRecord();

            Assert.Equal(
                "values",
                Assert.Throws<ArgumentNullException>(
                    () => bridgeDataRecord.GetValues(null)).ParamName);
        }

        [Fact]
        public void CloseExplicitly_doesnt_throw()
        {
            var bridgeDataRecord = CreateBridgeDataRecord();

            Assert.False(bridgeDataRecord.IsExplicitlyClosed);
            bridgeDataRecord.CloseExplicitly();
            Assert.True(bridgeDataRecord.IsExplicitlyClosed);
        }

#if !NET40

        [Fact]
        public void CloseExplicitlyAsync_doesnt_throw()
        {
            var bridgeDataRecord = CreateBridgeDataRecord();

            Assert.False(bridgeDataRecord.IsExplicitlyClosed);
            bridgeDataRecord.CloseExplicitlyAsync(CancellationToken.None).Wait();
            Assert.True(bridgeDataRecord.IsExplicitlyClosed);
        }

#endif
        
        [Fact]
        public void CloseImplicitly_doesnt_throw()
        {
            var bridgeDataRecord = CreateBridgeDataRecord();

            Assert.False(bridgeDataRecord.IsImplicitlyClosed);
            bridgeDataRecord.CloseImplicitly();
            Assert.True(bridgeDataRecord.IsImplicitlyClosed);
        }

#if !NET40

        [Fact]
        public void CloseImplicitlyAsync_doesnt_throw()
        {
            var bridgeDataRecord = CreateBridgeDataRecord();

            Assert.False(bridgeDataRecord.IsImplicitlyClosed);
            bridgeDataRecord.CloseImplicitlyAsync(CancellationToken.None).Wait();
            Assert.True(bridgeDataRecord.IsImplicitlyClosed);
        }

#endif
        
        private BridgeDataRecord CreateBridgeDataRecord()
        {
            var dbDataReaderMock = new Mock<DbDataReader>();

            var coordinatorFactory = Objects.MockHelper.CreateCoordinatorFactory<RecordState>(shaper => null);

            var shaperMock = new Mock<Shaper<RecordState>>(
                dbDataReaderMock.Object,
                /*context*/ null,
                /*workspace*/ null,
                MergeOption.AppendOnly,
                /*stateCount*/ 1,
                coordinatorFactory,
                /*checkPermissions*/ null,
                /*readerOwned*/ false)
                                 {
                                     CallBase = true
                                 };

            return new BridgeDataRecord(shaperMock.Object, 0);
        }
    }
}
