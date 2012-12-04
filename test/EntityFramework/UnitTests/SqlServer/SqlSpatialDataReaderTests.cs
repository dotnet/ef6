// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Common;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer.Utilities;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class SqlSpatialDataReaderTests
    {
        [Fact]
        public void GetGeography_roundtrips_DbGeography()
        {
            var dbGeography = DbGeography.FromText("POINT (90 50)");
            var mockSqlDataReader = CreateMockDbDataReader(dbGeography.ProviderValue, "sys.geography");
            var sqlSpatialDataReader = new SqlSpatialDataReader(SqlSpatialServices.Instance, mockSqlDataReader);

            var convertedDbGeography = sqlSpatialDataReader.GetGeography(0);

            Assert.Equal(dbGeography.WellKnownValue.WellKnownText, convertedDbGeography.WellKnownValue.WellKnownText);
        }

#if !NET40

        [Fact]
        public void GetGeographyAsync_roundtrips_DbGeography()
        {
            var dbGeography = DbGeography.FromText("POINT (90 50)");
            var mockSqlDataReader = CreateMockDbDataReader(dbGeography.ProviderValue, "sys.geography");
            var sqlSpatialDataReader = new SqlSpatialDataReader(SqlSpatialServices.Instance, mockSqlDataReader);

            var convertedDbGeography = sqlSpatialDataReader.GetGeographyAsync(0).Result;

            Assert.Equal(dbGeography.WellKnownValue.WellKnownText, convertedDbGeography.WellKnownValue.WellKnownText);
        }

#endif

        [Fact]
        public void GetGeometry_roundtrips_DbGeometry()
        {
            var dbGeometry = DbGeometry.FromText("POINT (90 50)");
            var mockSqlDataReader = CreateMockDbDataReader(dbGeometry.ProviderValue, "sys.geometry");
            var sqlSpatialDataReader = new SqlSpatialDataReader(SqlSpatialServices.Instance, mockSqlDataReader);

            var convertedDbGeometry = sqlSpatialDataReader.GetGeometry(0);

            Assert.Equal(dbGeometry.WellKnownValue.WellKnownText, convertedDbGeometry.WellKnownValue.WellKnownText);
        }

#if !NET40

        [Fact]
        public void GetGeometryAsync_roundtrips_DbGeometry()
        {
            var dbGeometry = DbGeometry.FromText("POINT (90 50)");
            var sqlDataReaderWrapper = CreateMockDbDataReader(dbGeometry.ProviderValue, "sys.geometry");
            var sqlSpatialDataReader = new SqlSpatialDataReader(SqlSpatialServices.Instance, sqlDataReaderWrapper);

            var convertedDbGeometry = sqlSpatialDataReader.GetGeometryAsync(0).Result;

            Assert.Equal(dbGeometry.WellKnownValue.WellKnownText, convertedDbGeometry.WellKnownValue.WellKnownText);
        }

#endif

        private DbDataReader CreateMockDbDataReader(object spatialProviderValueToReturn, string providerDataType)
        {
            var mockSqlDataReader = new Mock<DbDataReader>();

                mockSqlDataReader.Setup(m => m.GetValue(0)).Returns(spatialProviderValueToReturn);
#if !NET40
                mockSqlDataReader.Setup(m => m.GetFieldValueAsync<object>(0, CancellationToken.None)).Returns(Task.FromResult(spatialProviderValueToReturn));
#endif
                mockSqlDataReader.Setup(m => m.GetDataTypeName(0)).Returns(providerDataType);

            return mockSqlDataReader.Object;
        }
    }
}
