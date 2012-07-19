namespace System.Data.Entity.SqlServer
{
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
            var mockSqlDataReader = CreateSqlDataReaderWrapper(dbGeography.ProviderValue, "sys.geography");
            var sqlSpatialDataReader = new SqlSpatialDataReader(SqlSpatialServices.Instance, mockSqlDataReader);

            var convertedDbGeography = sqlSpatialDataReader.GetGeography(0);

            Assert.Equal(dbGeography.WellKnownValue.WellKnownText, convertedDbGeography.WellKnownValue.WellKnownText);
        }

        [Fact]
        public void GetGeographyAsync_roundtrips_DbGeography()
        {
            var dbGeography = DbGeography.FromText("POINT (90 50)");
            var mockSqlDataReader = CreateSqlDataReaderWrapper(dbGeography.ProviderValue, "sys.geography");
            var sqlSpatialDataReader = new SqlSpatialDataReader(SqlSpatialServices.Instance, mockSqlDataReader);

            var convertedDbGeography = sqlSpatialDataReader.GetGeographyAsync(0).Result;

            Assert.Equal(dbGeography.WellKnownValue.WellKnownText, convertedDbGeography.WellKnownValue.WellKnownText);
        }

        [Fact]
        public void GetGeometry_roundtrips_DbGeometry()
        {
            var dbGeometry = DbGeometry.FromText("POINT (90 50)");
            var mockSqlDataReader = CreateSqlDataReaderWrapper(dbGeometry.ProviderValue, "sys.geometry");
            var sqlSpatialDataReader = new SqlSpatialDataReader(SqlSpatialServices.Instance, mockSqlDataReader);

            var convertedDbGeometry = sqlSpatialDataReader.GetGeometry(0);

            Assert.Equal(dbGeometry.WellKnownValue.WellKnownText, convertedDbGeometry.WellKnownValue.WellKnownText);
        }

        [Fact]
        public void GetGeometryAsync_roundtrips_DbGeometry()
        {
            var dbGeometry = DbGeometry.FromText("POINT (90 50)");
            var sqlDataReaderWrapper = CreateSqlDataReaderWrapper(dbGeometry.ProviderValue, "sys.geometry");
            var sqlSpatialDataReader = new SqlSpatialDataReader(SqlSpatialServices.Instance, sqlDataReaderWrapper);

            var convertedDbGeometry = sqlSpatialDataReader.GetGeometryAsync(0).Result;

            Assert.Equal(dbGeometry.WellKnownValue.WellKnownText, convertedDbGeometry.WellKnownValue.WellKnownText);
        }

        private SqlDataReaderWrapper CreateSqlDataReaderWrapper(object spatialProviderValueToReturn, string providerDataType)
        {
            var mockSqlDataReader = new Mock<SqlDataReaderWrapper>();

            using (var memoryStream = new MemoryStream())
            {
                var writer = new BinaryWriter(memoryStream);

                MethodInfo writeMethod = spatialProviderValueToReturn.GetType().GetMethod("Write", BindingFlags.Public | BindingFlags.Instance,
                    binder: null, types: new[] { typeof(BinaryWriter) }, modifiers: null);
                writeMethod.Invoke(spatialProviderValueToReturn, new[] { writer });
                var sqlBytes = new SqlBytes(memoryStream.ToArray());

                mockSqlDataReader.Setup(m => m.GetSqlBytes(0)).Returns(sqlBytes);
                mockSqlDataReader.Setup(m => m.GetFieldValueAsync<SqlBytes>(0, CancellationToken.None)).Returns(Task.FromResult(sqlBytes));
                mockSqlDataReader.Setup(m => m.GetDataTypeName(0)).Returns(providerDataType);
            }

            return mockSqlDataReader.Object;
        }
    }
}
