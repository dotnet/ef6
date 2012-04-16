using System;
using System.Collections.Generic;
using System.Data.Spatial;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SampleEntityFrameworkProvider
{
    internal sealed class SqlSpatialDataReader : DbSpatialDataReader
    {
        private readonly SqlDataReader reader;
        
        static SqlSpatialDataReader()
        {
        }

        public SqlSpatialDataReader(SqlDataReader underlyingReader)
        {
            this.reader = underlyingReader;
        }

        public override DbGeography GetGeography(int ordinal)
        {
            EnsureGeographyColumn(ordinal);

            var geographyBytes = this.reader.GetSqlBytes(ordinal);
            dynamic geography = Activator.CreateInstance(SqlTypes.SqlGeographyType);
            geography.Read(new BinaryReader(geographyBytes.Stream));

            return SpatialServices.Instance.GeographyFromProviderValue(geography);
        }

        public override DbGeometry GetGeometry(int ordinal)
        {
            EnsureGeometryColumn(ordinal);

            var geometryBytes = this.reader.GetSqlBytes(ordinal);
            dynamic geometry = Activator.CreateInstance(SqlTypes.SqlGeometryType);
            geometry.Read(new BinaryReader(geometryBytes.Stream));

            return SpatialServices.Instance.GeometryFromProviderValue(geometry);
        }

        private void EnsureGeographyColumn(int ordinal)
        {
            string fieldTypeName = this.reader.GetDataTypeName(ordinal);
            if (!fieldTypeName.EndsWith("sys.geography", StringComparison.Ordinal)) // Use EndsWith so that we just see the schema and type name, not the database name.
            {
                throw new InvalidOperationException(
                       string.Format(
                           "Expected a geography value, found a value of type {0}.",
                           fieldTypeName));
            }
        }

        private void EnsureGeometryColumn(int ordinal)
        {
            string fieldTypeName = this.reader.GetDataTypeName(ordinal);
            if (!fieldTypeName.EndsWith("sys.geometry", StringComparison.Ordinal)) // Use EndsWith so that we just see the schema and type name, not the database name.
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Expected a geometry value, found a value of type {0}.",
                        fieldTypeName));
            }
        }
    }
}
