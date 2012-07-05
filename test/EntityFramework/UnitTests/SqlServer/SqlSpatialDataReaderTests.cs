namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer.Utilities;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Reflection;
    using Moq;
    using Xunit;

    public class SqlSpatialDataReaderTests
    {
        [Fact]
        public void GetGeography_roundtrips_DbGeography()
        {
            var dbGeography = DbGeography.FromText("POINT (90 50)");
            var mockSqlDataReader = new Mock<SqlDataReaderWrapper>();

            using (var memoryStream = new MemoryStream())
            {
                var writer = new BinaryWriter(memoryStream);

                var sqlTypes = new SqlTypesAssemblyLoader().GetSqlTypesAssembly();
                MethodInfo writeMethod = sqlTypes.SqlGeographyType.GetMethod("Write", BindingFlags.Public | BindingFlags.Instance,
                    binder: null, types: new[] { typeof(BinaryWriter) }, modifiers: null);
                writeMethod.Invoke(dbGeography.ProviderValue, new[] { writer });
                var sqlBytes = new SqlBytes(memoryStream.ToArray());

                mockSqlDataReader.Setup(m => m.GetSqlBytes(0)).Returns(sqlBytes);
                mockSqlDataReader.Setup(m => m.GetDataTypeName(0)).Returns("sys.geography");
            }

            var sqlSpatialDataReader = new SqlSpatialDataReader(SqlSpatialServices.Instance, mockSqlDataReader.Object);

            var convertedDbGeography = sqlSpatialDataReader.GetGeography(0);

            Assert.Equal(dbGeography.WellKnownValue.WellKnownText, convertedDbGeography.WellKnownValue.WellKnownText);
        }
        
        [Fact]
        public void GetGeometry_roundtrips_DbGeometry()
        {
            var dbGeometry = DbGeometry.FromText("POINT (90 50)");
            var mockSqlDataReader = new Mock<SqlDataReaderWrapper>();

            using (var memoryStream = new MemoryStream())
            {
                var writer = new BinaryWriter(memoryStream);

                var sqlTypes = new SqlTypesAssemblyLoader().GetSqlTypesAssembly();
                MethodInfo writeMethod = sqlTypes.SqlGeometryType.GetMethod("Write", BindingFlags.Public | BindingFlags.Instance,
                    binder: null, types: new[] { typeof(BinaryWriter) }, modifiers: null);
                writeMethod.Invoke(dbGeometry.ProviderValue, new[] { writer });
                var sqlBytes = new SqlBytes(memoryStream.ToArray());

                mockSqlDataReader.Setup(m => m.GetSqlBytes(0)).Returns(sqlBytes);
                mockSqlDataReader.Setup(m => m.GetDataTypeName(0)).Returns("sys.geometry");
            }

            var sqlSpatialDataReader = new SqlSpatialDataReader(SqlSpatialServices.Instance, mockSqlDataReader.Object);

            var convertedDbGeometry = sqlSpatialDataReader.GetGeometry(0);

            Assert.Equal(dbGeometry.WellKnownValue.WellKnownText, convertedDbGeometry.WellKnownValue.WellKnownText);
        }
    }
}
