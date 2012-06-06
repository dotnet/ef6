namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer.Resources;
    using System.Data.Entity.SqlServer.Utilities;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.IO;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// SqlClient specific implementation of <see cref="DbSpatialDataReader"/>
    /// </summary>
    internal sealed class SqlSpatialDataReader : DbSpatialDataReader
    {
        private readonly SqlDataReaderWrapper _reader;
        private const string geometrySqlType = "sys.geometry";
        private const string geographySqlType = "sys.geography";

        internal SqlSpatialDataReader(SqlDataReaderWrapper underlyingReader)
        {
            _reader = underlyingReader;
        }

        public override DbGeography GetGeography(int ordinal)
        {
            EnsureGeographyColumn(ordinal);
            var geogBytes = _reader.GetSqlBytes(ordinal);
            var providerValue = SqlGeographyFromBinaryReader.Value(new BinaryReader(geogBytes.Stream));
            return SqlSpatialServices.Instance.GeographyFromProviderValue(providerValue);
        }

        public override async Task<DbGeography> GetGeographyAsync(int ordinal, CancellationToken cancellationToken)
        {
            EnsureGeographyColumn(ordinal);
            var geogBytes = await _reader.GetFieldValueAsync<SqlBytes>(ordinal, cancellationToken);
            var providerValue = SqlGeographyFromBinaryReader.Value(new BinaryReader(geogBytes.Stream));
            return SqlSpatialServices.Instance.GeographyFromProviderValue(providerValue);
        }

        public override DbGeometry GetGeometry(int ordinal)
        {
            EnsureGeometryColumn(ordinal);
            var geomBytes = _reader.GetSqlBytes(ordinal);
            var providerValue = SqlGeometryFromBinaryReader.Value(new BinaryReader(geomBytes.Stream));
            return SqlSpatialServices.Instance.GeometryFromProviderValue(providerValue);
        }

        public override async Task<DbGeometry> GetGeometryAsync(int ordinal, CancellationToken cancellationToken)
        {
            EnsureGeometryColumn(ordinal);
            var geomBytes = await _reader.GetFieldValueAsync<SqlBytes>(ordinal, cancellationToken);
            var providerValue = SqlGeometryFromBinaryReader.Value(new BinaryReader(geomBytes.Stream));
            return SqlSpatialServices.Instance.GeometryFromProviderValue(providerValue);
        }

        private static readonly Lazy<Func<BinaryReader, object>> SqlGeographyFromBinaryReader =
            new Lazy<Func<BinaryReader, object>>(
                () => CreateBinaryReadDelegate(SqlProviderServices.GetSqlTypesAssembly().SqlGeographyType), isThreadSafe: true);

        private static readonly Lazy<Func<BinaryReader, object>> SqlGeometryFromBinaryReader =
            new Lazy<Func<BinaryReader, object>>(
                () => CreateBinaryReadDelegate(SqlProviderServices.GetSqlTypesAssembly().SqlGeometryType), isThreadSafe: true);

        // test to ensure that the SQL column has the expected SQL type.   Don't use the CLR type to avoid having to worry about differences in 
        // type versions between the client and the database.  
        private void EnsureGeographyColumn(int ordinal)
        {
            var fieldTypeName = _reader.GetDataTypeName(ordinal);
            if (!fieldTypeName.EndsWith(geographySqlType, StringComparison.Ordinal))
            // Use EndsWith so that we just see the schema and type name, not the database name.
            {
                throw new InvalidDataException(Strings.SqlProvider_InvalidGeographyColumn(fieldTypeName));
            }
        }

        private void EnsureGeometryColumn(int ordinal)
        {
            var fieldTypeName = _reader.GetDataTypeName(ordinal);
            if (!fieldTypeName.EndsWith(geometrySqlType, StringComparison.Ordinal))
            // Use EndsWith so that we just see the schema and type name, not the database name.
            {
                throw new InvalidDataException(Strings.SqlProvider_InvalidGeometryColumn(fieldTypeName));
            }
        }

        /// <summary>
        /// Builds and compiles the Expression equivalent of the following:
        ///   
        /// (BinaryReader r) => { var result = new SpatialType(); result.Read(r); return r; }
        ///   
        /// The construct/read pattern is preferred over casting the result of calling GetValue on the DataReader,
        /// because constructing the value directly allows client code to specify the type, rather than SqlClient using
        /// the server-specified assembly qualified type name from TDS to try to locate the correct type on the client.
        /// </summary>
        /// <param name="spatialType"></param>
        /// <returns></returns>
        private static Func<BinaryReader, object> CreateBinaryReadDelegate(Type spatialType)
        {
            Debug.Assert(spatialType != null, "Ensure spatialType is non-null before calling CreateBinaryReadDelegate");

            var readerParam = Expression.Parameter(typeof(BinaryReader));
            var binarySerializable = Expression.Variable(spatialType);
            var readMethod = spatialType.GetMethod(
                "Read", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(BinaryReader) }, null);

            var ex = Expression.Lambda<Func<BinaryReader, object>>(
                Expression.Block(
                    new[] { binarySerializable },
                    Expression.Assign(binarySerializable, Expression.New(spatialType)),
                    Expression.Call(binarySerializable, readMethod, readerParam),
                    binarySerializable
                    ),
                readerParam
                );

            var result = ex.Compile();
            return result;
        }
    }
}
