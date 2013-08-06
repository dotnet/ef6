// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer.Resources;
    using System.Data.Entity.SqlServer.Utilities;
    using System.IO;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    ///     SqlClient specific implementation of <see cref="DbSpatialDataReader" />
    /// </summary>
    internal sealed class SqlSpatialDataReader : DbSpatialDataReader
    {
        private static readonly Lazy<Func<BinaryReader, object>> _sqlGeographyFromBinaryReader =
            new Lazy<Func<BinaryReader, object>>(
                () => CreateBinaryReadDelegate(SqlTypesAssemblyLoader.DefaultInstance.GetSqlTypesAssembly().SqlGeographyType), isThreadSafe: true);

        private static readonly Lazy<Func<BinaryReader, object>> _sqlGeometryFromBinaryReader =
            new Lazy<Func<BinaryReader, object>>(
                () => CreateBinaryReadDelegate(SqlTypesAssemblyLoader.DefaultInstance.GetSqlTypesAssembly().SqlGeometryType), isThreadSafe: true);

        private const string GeometrySqlType = "sys.geometry";
        private const string GeographySqlType = "sys.geography";

        private readonly DbSpatialServices _spatialServices;
        private readonly SqlDataReaderWrapper _reader;

        private readonly bool[] _geographyColumns;
        private readonly bool[] _geometryColumns;

        internal SqlSpatialDataReader(DbSpatialServices spatialServices, SqlDataReaderWrapper underlyingReader)
        {
            _spatialServices = spatialServices;
            _reader = underlyingReader;

            var fieldCount = _reader.FieldCount;
            _geographyColumns = new bool[fieldCount];
            _geometryColumns = new bool[fieldCount];
            for (var i = 0; i < _reader.FieldCount; i++)
            {
                var fieldTypeName = _reader.GetDataTypeName(i);
                // Use EndsWith so that we just see the schema and type name, not the database name.
                if (fieldTypeName.EndsWith(GeographySqlType, StringComparison.Ordinal))
                {
                    _geographyColumns[i] = true;
                }
                else if (fieldTypeName.EndsWith(GeometrySqlType, StringComparison.Ordinal))
                {
                    _geometryColumns[i] = true;
                }
            }
        }

        /// <inheritdoc/>
        public override DbGeography GetGeography(int ordinal)
        {
            EnsureGeographyColumn(ordinal);
            var geogBytes = _reader.GetSqlBytes(ordinal);
            var providerValue = _sqlGeographyFromBinaryReader.Value(new BinaryReader(geogBytes.Stream));
            return _spatialServices.GeographyFromProviderValue(providerValue);
        }

        /// <inheritdoc/>
        public override DbGeometry GetGeometry(int ordinal)
        {
            EnsureGeometryColumn(ordinal);
            var geomBytes = _reader.GetSqlBytes(ordinal);
            var providerValue = _sqlGeometryFromBinaryReader.Value(new BinaryReader(geomBytes.Stream));
            return _spatialServices.GeometryFromProviderValue(providerValue);
        }

        /// <inheritdoc/>
        public override bool IsGeographyColumn(int ordinal)
        {
            return _geographyColumns[ordinal];
        }

        /// <inheritdoc/>
        public override bool IsGeometryColumn(int ordinal)
        {
            return _geometryColumns[ordinal];
        }

        private void EnsureGeographyColumn(int ordinal)
        {
            if (!IsGeographyColumn(ordinal))
            {
                throw new InvalidDataException(Strings.SqlProvider_InvalidGeographyColumn(_reader.GetDataTypeName(ordinal)));
            }
        }

        private void EnsureGeometryColumn(int ordinal)
        {
            if (!IsGeometryColumn(ordinal))
            {
                throw new InvalidDataException(Strings.SqlProvider_InvalidGeometryColumn(_reader.GetDataTypeName(ordinal)));
            }
        }

        /// <summary>
        ///     Builds and compiles the Expression equivalent of the following:
        ///     (BinaryReader r) => { var result = new SpatialType(); result.Read(r); return r; }
        ///     The construct/read pattern is preferred over casting the result of calling GetValue on the DataReader,
        ///     because constructing the value directly allows client code to specify the type, rather than SqlClient using
        ///     the server-specified assembly qualified type name from TDS to try to locate the correct type on the client.
        /// </summary>
        private static Func<BinaryReader, object> CreateBinaryReadDelegate(Type spatialType)
        {
            DebugCheck.NotNull(spatialType);

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
