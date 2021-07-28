// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer.Utilities;
    using System.Data.SqlTypes;
    using System.IO;
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
            var mockSqlDataReader = CreateSqlDataReaderWrapper(dbGeography.ProviderValue, "sys.geography");
            var sqlSpatialDataReader = new SqlSpatialDataReader(SqlSpatialServices.Instance, mockSqlDataReader);

            var convertedDbGeography = sqlSpatialDataReader.GetGeography(0);

            Assert.Equal(dbGeography.WellKnownValue.WellKnownText, convertedDbGeography.WellKnownValue.WellKnownText);
        }

#if !NET40

        [Fact]
        public void GetGeographyAsync_roundtrips_DbGeography()
        {
            var dbGeography = DbGeography.FromText("POINT (90 50)");
            var mockSqlDataReader = CreateSqlDataReaderWrapper(dbGeography.ProviderValue, "sys.geography");
            var sqlSpatialDataReader = new SqlSpatialDataReader(SqlSpatialServices.Instance, mockSqlDataReader);

            var convertedDbGeography = sqlSpatialDataReader.GetGeographyAsync(0, CancellationToken.None).Result;

            Assert.Equal(dbGeography.WellKnownValue.WellKnownText, convertedDbGeography.WellKnownValue.WellKnownText);
        }

#endif

        [Fact]
        public void GetGeometry_roundtrips_DbGeometry()
        {
            var dbGeometry = DbGeometry.FromText("POINT (90 50)");
            var mockSqlDataReader = CreateSqlDataReaderWrapper(dbGeometry.ProviderValue, "sys.geometry");
            var sqlSpatialDataReader = new SqlSpatialDataReader(SqlSpatialServices.Instance, mockSqlDataReader);

            var convertedDbGeometry = sqlSpatialDataReader.GetGeometry(0);

            Assert.Equal(dbGeometry.WellKnownValue.WellKnownText, convertedDbGeometry.WellKnownValue.WellKnownText);
        }

#if !NET40

        [Fact]
        public void GetGeometryAsync_roundtrips_DbGeometry()
        {
            var dbGeometry = DbGeometry.FromText("POINT (90 50)");
            var sqlDataReaderWrapper = CreateSqlDataReaderWrapper(dbGeometry.ProviderValue, "sys.geometry");
            var sqlSpatialDataReader = new SqlSpatialDataReader(SqlSpatialServices.Instance, sqlDataReaderWrapper);

            var convertedDbGeometry = sqlSpatialDataReader.GetGeometryAsync(0, CancellationToken.None).Result;

            Assert.Equal(dbGeometry.WellKnownValue.WellKnownText, convertedDbGeometry.WellKnownValue.WellKnownText);
        }

#endif

        [Fact]
        public void IsGeographyColumn_returns_true_for_geography_column()
        {
            var dbGeography = DbGeography.FromText("POINT (90 50)");
            var mockSqlDataReader = CreateSqlDataReaderWrapper(dbGeography.ProviderValue, "sys.geography");
            var sqlSpatialDataReader = new SqlSpatialDataReader(SqlSpatialServices.Instance, mockSqlDataReader);

            Assert.True(sqlSpatialDataReader.IsGeographyColumn(0));
        }

        [Fact]
        public void IsGeographyColumn_returns_false_for_geometry_column()
        {
            var dbGeometry = DbGeometry.FromText("POINT (90 50)");
            var mockSqlDataReader = CreateSqlDataReaderWrapper(dbGeometry.ProviderValue, "sys.geometry");
            var sqlSpatialDataReader = new SqlSpatialDataReader(SqlSpatialServices.Instance, mockSqlDataReader);

            Assert.False(sqlSpatialDataReader.IsGeographyColumn(0));
        }

        [Fact]
        public void IsGeometryColumn_returns_true_for_geometry_column()
        {
            var dbGeometry = DbGeometry.FromText("POINT (90 50)");
            var mockSqlDataReader = CreateSqlDataReaderWrapper(dbGeometry.ProviderValue, "sys.geometry");
            var sqlSpatialDataReader = new SqlSpatialDataReader(SqlSpatialServices.Instance, mockSqlDataReader);

           Assert.True(sqlSpatialDataReader.IsGeometryColumn(0));
        }

        [Fact]
        public void IsGeometryColumn_returns_false_for_geography_column()
        {
            var dbGeography = DbGeography.FromText("POINT (90 50)");
            var mockSqlDataReader = CreateSqlDataReaderWrapper(dbGeography.ProviderValue, "sys.geography");
            var sqlSpatialDataReader = new SqlSpatialDataReader(SqlSpatialServices.Instance, mockSqlDataReader);

            Assert.False(sqlSpatialDataReader.IsGeometryColumn(0));
        }

        private SqlDataReaderWrapper CreateSqlDataReaderWrapper(object spatialProviderValueToReturn, string providerDataType)
        {
            var mockSqlDataReader = new Mock<SqlDataReaderWrapper>();

            using (var memoryStream = new MemoryStream())
            {
                var writer = new BinaryWriter(memoryStream);

                var writeMethod = spatialProviderValueToReturn.GetType().GetPublicInstanceMethod("Write", new[] { typeof(BinaryWriter) });
                writeMethod.Invoke(spatialProviderValueToReturn, new[] { writer });
                var sqlBytes = new SqlBytes(memoryStream.ToArray());

                mockSqlDataReader.Setup(m => m.GetSqlBytes(0)).Returns(sqlBytes);
#if !NET40
                mockSqlDataReader.Setup(m => m.GetFieldValueAsync<SqlBytes>(0, CancellationToken.None)).Returns(Task.FromResult(sqlBytes));
#endif
                mockSqlDataReader.Setup(m => m.GetDataTypeName(0)).Returns(providerDataType);
                mockSqlDataReader.Setup(m => m.FieldCount).Returns(1);
            }

            return mockSqlDataReader.Object;
        }
    }
}
