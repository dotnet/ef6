// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Common;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer.Resources;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     SqlClient specific implementation of <see cref="DbSpatialDataReader" />
    /// </summary>
    internal sealed class SqlSpatialDataReader : DbSpatialDataReader
    {
        private const string GeometrySqlType = "sys.geometry";
        private const string GeographySqlType = "sys.geography";

        private readonly DbSpatialServices _spatialServices;
        private readonly DbDataReader _reader;

        internal SqlSpatialDataReader(DbSpatialServices spatialServices, DbDataReader underlyingReader)
        {
            _spatialServices = spatialServices;
            _reader = underlyingReader;
        }

        public override DbGeography GetGeography(int ordinal)
        {
            EnsureGeographyColumn(ordinal);
            var geography = _reader.GetValue(ordinal);
            return _spatialServices.GeographyFromProviderValue(geography);
        }

#if !NET40

        public override async Task<DbGeography> GetGeographyAsync(int ordinal, CancellationToken cancellationToken)
        {
            EnsureGeographyColumn(ordinal);
            var geography = await _reader.GetFieldValueAsync<object>(ordinal, cancellationToken);
            return _spatialServices.GeographyFromProviderValue(geography);
        }

#endif

        public override DbGeometry GetGeometry(int ordinal)
        {
            EnsureGeometryColumn(ordinal);
            var geometry = _reader.GetValue(ordinal);
            return _spatialServices.GeometryFromProviderValue(geometry);
        }

#if !NET40

        public override async Task<DbGeometry> GetGeometryAsync(int ordinal, CancellationToken cancellationToken)
        {
            EnsureGeometryColumn(ordinal);
            var geometry = await _reader.GetFieldValueAsync<object>(ordinal, cancellationToken);
            return _spatialServices.GeometryFromProviderValue(geometry);
        }

#endif
        
        // test to ensure that the SQL column has the expected SQL type. Don't use the CLR type to avoid having to worry about differences in 
        // type versions between the client and the database.  
        private void EnsureGeographyColumn(int ordinal)
        {
            var fieldTypeName = _reader.GetDataTypeName(ordinal);
            if (!fieldTypeName.EndsWith(GeographySqlType, StringComparison.Ordinal))
            // Use EndsWith so that we just see the schema and type name, not the database name.
            {
                throw new InvalidDataException(Strings.SqlProvider_InvalidGeographyColumn(fieldTypeName));
            }
        }

        private void EnsureGeometryColumn(int ordinal)
        {
            var fieldTypeName = _reader.GetDataTypeName(ordinal);
            if (!fieldTypeName.EndsWith(GeometrySqlType, StringComparison.Ordinal))
            // Use EndsWith so that we just see the schema and type name, not the database name.
            {
                throw new InvalidDataException(Strings.SqlProvider_InvalidGeometryColumn(fieldTypeName));
            }
        }
    }
}
