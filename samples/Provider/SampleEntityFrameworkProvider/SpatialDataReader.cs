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
        private static readonly Type sqlGeographyType;
        private static readonly Type sqlGeometryType;

        private readonly SqlDataReader reader;
        
        static SqlSpatialDataReader()
        {
            // find the latest version of Microsoft.SqlServer.Types assembly that contains Sql spatial types
            var preferredSqlTypesAssemblies = new[] 
            {                
                "Microsoft.SqlServer.Types, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91",
                "Microsoft.SqlServer.Types, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91",
            };

            Assembly sqlTypesAssembly = null;
            foreach (string assemblyFullName in preferredSqlTypesAssemblies)
            {
                AssemblyName asmName = new AssemblyName(assemblyFullName);
                try
                {
                    sqlTypesAssembly = Assembly.Load(asmName);
                    break;
                }
                catch (FileNotFoundException)
                {
                }
                catch (FileLoadException)
                {
                }
            }

            if (sqlTypesAssembly == null)
            {
                throw new InvalidOperationException("Microsoft.SqlServer.Types assembly not found");
            }

            sqlGeographyType = sqlTypesAssembly.GetType("Microsoft.SqlServer.Types.SqlGeography", throwOnError: true);
            sqlGeometryType = sqlTypesAssembly.GetType("Microsoft.SqlServer.Types.SqlGeometry", throwOnError: true);
        }

        public SqlSpatialDataReader(SqlDataReader underlyingReader)
        {
            this.reader = underlyingReader;
        }

        public override DbGeography GetGeography(int ordinal)
        {
            EnsureGeographyColumn(ordinal);

            SqlBytes geographyBytes = this.reader.GetSqlBytes(ordinal);
            dynamic geography = Activator.CreateInstance(sqlGeographyType);
            geography.Read(new BinaryReader(geographyBytes.Stream));

            return SpatialServices.Instance.GeographyFromProviderValue(geography);
        }

        public override DbGeometry GetGeometry(int ordinal)
        {
            throw new NotImplementedException();
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
