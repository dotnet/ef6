// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer.Resources;
    using System.Data.Entity.SqlServer.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    [Serializable]
    [DbProviderName(SqlProviderServices.ProviderInvariantName)]
    internal class SqlSpatialServices : DbSpatialServices
    {
        internal static readonly SqlSpatialServices Instance = new SqlSpatialServices();

        private static Dictionary<string, SqlSpatialServices> _otherSpatialServices;

        [NonSerialized]
        private readonly SqlTypesAssemblyLoader _loader;

        public SqlSpatialServices()
        {
        }

        public SqlSpatialServices(SqlTypesAssemblyLoader loader)
        {
            _loader = loader;
        }

        /// <inheritdoc />
        public override bool NativeTypesAvailable
        {
            get { return (_loader ?? SqlTypesAssemblyLoader.DefaultInstance).TryGetSqlTypesAssembly() != null; }
        }

        // Given an assembly purportedly containing SqlServerTypes for spatial values, attempt to 
        // create a corresponding SQL specific DbSpatialServices value backed by types from that assembly.
        // Uses a dictionary to ensure that there is at most one db spatial service per assembly.   It's important that
        // this be done in a way that ensures that the underlying SqlTypesAssembly value is also atomized,
        // since that's caching compilation.
        // Relies on SqlTypesAssembly to verify that the assembly is appropriate.
        private static bool TryGetSpatialServiceFromAssembly(Assembly assembly, out SqlSpatialServices services)
        {
            if (_otherSpatialServices == null
                || !_otherSpatialServices.TryGetValue(assembly.FullName, out services))
            {
                lock (Instance)
                {
                    if (_otherSpatialServices == null
                        || !_otherSpatialServices.TryGetValue(assembly.FullName, out services))
                    {
                        SqlTypesAssembly sqlAssembly;
                        if (SqlTypesAssemblyLoader.DefaultInstance.TryGetSqlTypesAssembly(assembly, out sqlAssembly))
                        {
                            if (_otherSpatialServices == null)
                            {
                                _otherSpatialServices = new Dictionary<string, SqlSpatialServices>(1);
                            }
                            services = new SqlSpatialServices(new SqlTypesAssemblyLoader(sqlAssembly));
                            _otherSpatialServices.Add(assembly.FullName, services);
                        }
                        else
                        {
                            services = null;
                        }
                    }
                }
            }
            return services != null;
        }

        public SqlTypesAssembly SqlTypes
        {
            get { return (_loader ?? SqlTypesAssemblyLoader.DefaultInstance).GetSqlTypesAssembly(); }
        }

        public override object CreateProviderValue(DbGeographyWellKnownValue wellKnownValue)
        {
            Check.NotNull(wellKnownValue, "wellKnownValue");

            object result = null;
            if (wellKnownValue.WellKnownText != null)
            {
                result = SqlTypes.SqlTypesGeographyFromText(wellKnownValue.WellKnownText, wellKnownValue.CoordinateSystemId);
            }
            else if (wellKnownValue.WellKnownBinary != null)
            {
                result = SqlTypes.SqlTypesGeographyFromBinary(wellKnownValue.WellKnownBinary, wellKnownValue.CoordinateSystemId);
            }
            else
            {
                throw new ArgumentException(Strings.Spatial_WellKnownGeographyValueNotValid, "wellKnownValue");
            }

            return result;
        }

        public override DbGeography GeographyFromProviderValue(object providerValue)
        {
            Check.NotNull(providerValue, "providerValue");

            var normalizedProviderValue = NormalizeProviderValue(providerValue, SqlTypes.SqlGeographyType);
            return SqlTypes.IsSqlGeographyNull(normalizedProviderValue) ? null : CreateGeography(this, normalizedProviderValue);
        }

        // Ensure that provider values are from the expected version of the Sql types assembly. If they aren't try to 
        // convert them so that they are.
        // 
        // Normally when we obtain values from the store, we try to use the appropriate SqlSpatialDataReader. This will make sure that 
        // any spatial values are instantiated with the provider type from the appropriate SqlServerTypes assembly. However, 
        // in one case (output parameter values) we don't have an opportunity to make this happen. There we get whatever value 
        // the underlying SqlDataReader produces which doesn't necessarily produce values from the assembly we expect.
        private object NormalizeProviderValue(object providerValue, Type expectedSpatialType)
        {
            Debug.Assert(expectedSpatialType == SqlTypes.SqlGeographyType || expectedSpatialType == SqlTypes.SqlGeometryType);
            var providerValueType = providerValue.GetType();
            if (providerValueType != expectedSpatialType)
            {
                SqlSpatialServices otherServices;
                if (TryGetSpatialServiceFromAssembly(providerValue.GetType().Assembly, out otherServices))
                {
                    if (expectedSpatialType == SqlTypes.SqlGeographyType)
                    {
                        if (providerValueType == otherServices.SqlTypes.SqlGeographyType)
                        {
                            return ConvertToSqlValue(otherServices.GeographyFromProviderValue(providerValue), "providerValue");
                        }
                    }
                    else
                    {
                        Debug.Assert(expectedSpatialType == SqlTypes.SqlGeometryType);
                        if (providerValueType == otherServices.SqlTypes.SqlGeometryType)
                        {
                            return ConvertToSqlValue(otherServices.GeometryFromProviderValue(providerValue), "providerValue");
                        }
                    }
                }

                throw new ArgumentException(
                    Strings.SqlSpatialServices_ProviderValueNotSqlType(expectedSpatialType.AssemblyQualifiedName), "providerValue");
            }

            return providerValue;
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        public override DbGeographyWellKnownValue CreateWellKnownValue(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var spatialValue = geographyValue.AsSpatialValue();

            var result = CreateWellKnownValue(
                spatialValue,
                () =>
                (Exception)new ArgumentException(Strings.SqlSpatialservices_CouldNotCreateWellKnownGeographyValueNoSrid, "geographyValue"),
                () =>
                (Exception)
                new ArgumentException(Strings.SqlSpatialservices_CouldNotCreateWellKnownGeographyValueNoWkbOrWkt, "geographyValue"),
                (srid, wkb, wkt) => new DbGeographyWellKnownValue
                                        {
                                            CoordinateSystemId = srid,
                                            WellKnownBinary = wkb,
                                            WellKnownText = wkt
                                        });

            return result;
        }

        public override object CreateProviderValue(DbGeometryWellKnownValue wellKnownValue)
        {
            Check.NotNull(wellKnownValue, "wellKnownValue");

            object result = null;
            if (wellKnownValue.WellKnownText != null)
            {
                result = SqlTypes.SqlTypesGeometryFromText(wellKnownValue.WellKnownText, wellKnownValue.CoordinateSystemId);
            }
            else if (wellKnownValue.WellKnownBinary != null)
            {
                result = SqlTypes.SqlTypesGeometryFromBinary(wellKnownValue.WellKnownBinary, wellKnownValue.CoordinateSystemId);
            }
            else
            {
                throw new ArgumentException(Strings.Spatial_WellKnownGeometryValueNotValid, "wellKnownValue");
            }

            return result;
        }

        public override DbGeometry GeometryFromProviderValue(object providerValue)
        {
            Check.NotNull(providerValue, "providerValue");

            var normalizedProviderValue = NormalizeProviderValue(providerValue, SqlTypes.SqlGeometryType);
            return SqlTypes.IsSqlGeometryNull(normalizedProviderValue) ? null : CreateGeometry(this, normalizedProviderValue);
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        public override DbGeometryWellKnownValue CreateWellKnownValue(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var spatialValue = geometryValue.AsSpatialValue();

            var result = CreateWellKnownValue(
                spatialValue,
                () =>
                (Exception)new ArgumentException(Strings.SqlSpatialservices_CouldNotCreateWellKnownGeometryValueNoSrid, "geometryValue"),
                () =>
                (Exception)new ArgumentException(Strings.SqlSpatialservices_CouldNotCreateWellKnownGeometryValueNoWkbOrWkt, "geometryValue"),
                (srid, wkb, wkt) => new DbGeometryWellKnownValue
                                        {
                                            CoordinateSystemId = srid,
                                            WellKnownBinary = wkb,
                                            WellKnownText = wkt
                                        });

            return result;
        }

        private static TValue CreateWellKnownValue<TValue>(
            IDbSpatialValue spatialValue, Func<Exception> onMissingSrid, Func<Exception> onMissingWkbAndWkt,
            Func<int, byte[], string, TValue> onValidValue)
        {
            var srid = spatialValue.CoordinateSystemId;

            if (!srid.HasValue)
            {
                throw onMissingSrid();
            }

            var wkt = spatialValue.WellKnownText;
            if (wkt != null)
            {
                return onValidValue(srid.Value, null, wkt);
            }
            else
            {
                var wkb = spatialValue.WellKnownBinary;
                if (wkb != null)
                {
                    return onValidValue(srid.Value, wkb, null);
                }
            }

            throw onMissingWkbAndWkt();
        }

        public override string AsTextIncludingElevationAndMeasure(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            return SqlTypes.GeographyAsTextZM(geographyValue);
        }

        public override string AsTextIncludingElevationAndMeasure(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            return SqlTypes.GeometryAsTextZM(geometryValue);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "argumentName")]
        private object ConvertToSqlValue(DbGeography geographyValue, string argumentName)
        {
            if (geographyValue == null)
            {
                return null;
            }

            return SqlTypes.ConvertToSqlTypesGeography(geographyValue);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "argumentName")]
        private object ConvertToSqlValue(DbGeometry geometryValue, string argumentName)
        {
            if (geometryValue == null)
            {
                return null;
            }

            return SqlTypes.ConvertToSqlTypesGeometry(geometryValue);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "argumentName")]
        private object ConvertToSqlBytes(byte[] binaryValue, string argumentName)
        {
            if (binaryValue == null)
            {
                return null;
            }

            return SqlTypes.SqlBytesFromByteArray(binaryValue);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "argumentName")]
        private object ConvertToSqlChars(string stringValue, string argumentName)
        {
            if (stringValue == null)
            {
                return null;
            }

            return SqlTypes.SqlCharsFromString(stringValue);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "argumentName")]
        private object ConvertToSqlString(string stringValue, string argumentName)
        {
            if (stringValue == null)
            {
                return null;
            }

            return SqlTypes.SqlStringFromString(stringValue);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "argumentName")]
        private object ConvertToSqlXml(string stringValue, string argumentName)
        {
            if (stringValue == null)
            {
                return null;
            }

            return SqlTypes.SqlXmlFromString(stringValue);
        }

        private bool ConvertSqlBooleanToBoolean(object sqlBoolean)
        {
            return SqlTypes.SqlBooleanToBoolean(sqlBoolean);
        }

        private bool? ConvertSqlBooleanToNullableBoolean(object sqlBoolean)
        {
            return SqlTypes.SqlBooleanToNullableBoolean(sqlBoolean);
        }

        private byte[] ConvertSqlBytesToBinary(object sqlBytes)
        {
            return SqlTypes.SqlBytesToByteArray(sqlBytes);
        }

        private string ConvertSqlCharsToString(object sqlCharsValue)
        {
            return SqlTypes.SqlCharsToString(sqlCharsValue);
        }

        private string ConvertSqlStringToString(object sqlCharsValue)
        {
            return SqlTypes.SqlStringToString(sqlCharsValue);
        }

        private double ConvertSqlDoubleToDouble(object sqlDoubleValue)
        {
            return SqlTypes.SqlDoubleToDouble(sqlDoubleValue);
        }

        private double? ConvertSqlDoubleToNullableDouble(object sqlDoubleValue)
        {
            return SqlTypes.SqlDoubleToNullableDouble(sqlDoubleValue);
        }

        private int ConvertSqlInt32ToInt(object sqlInt32Value)
        {
            return SqlTypes.SqlInt32ToInt(sqlInt32Value);
        }

        private int? ConvertSqlInt32ToNullableInt(object sqlInt32Value)
        {
            return SqlTypes.SqlInt32ToNullableInt(sqlInt32Value);
        }

        private string ConvertSqlXmlToString(object sqlXmlValue)
        {
            return SqlTypes.SqlXmlToString(sqlXmlValue);
        }

        public override DbGeography GeographyFromText(string geographyText)
        {
            var sqlGeographyText = ConvertToSqlString(geographyText, "geographyText");
            var result = SqlTypes.SmiSqlGeographyParse.Value.Invoke(null, new[] { sqlGeographyText });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyFromText(string geographyText, int srid)
        {
            var sqlGeographyText = ConvertToSqlChars(geographyText, "geographyText");
            var result = SqlTypes.SmiSqlGeographyStGeomFromText.Value.Invoke(null, new[] { sqlGeographyText, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyPointFromText(string pointText, int srid)
        {
            var sqlPointText = ConvertToSqlChars(pointText, "pointText");
            var result = SqlTypes.SmiSqlGeographyStPointFromText.Value.Invoke(null, new[] { sqlPointText, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyLineFromText(string lineText, int srid)
        {
            var sqlLineText = ConvertToSqlChars(lineText, "lineText");
            var result = SqlTypes.SmiSqlGeographyStLineFromText.Value.Invoke(null, new[] { sqlLineText, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyPolygonFromText(string polygonText, int srid)
        {
            var sqlPolygonText = ConvertToSqlChars(polygonText, "polygonText");
            var result = SqlTypes.SmiSqlGeographyStPolyFromText.Value.Invoke(null, new[] { sqlPolygonText, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyMultiPointFromText(string multiPointText, int srid)
        {
            var sqlMultiPointText = ConvertToSqlChars(multiPointText, "multiPointText");
            var result = SqlTypes.SmiSqlGeographyStmPointFromText.Value.Invoke(null, new[] { sqlMultiPointText, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyMultiLineFromText(string multiLineText, int srid)
        {
            var sqlMultiLineText = ConvertToSqlChars(multiLineText, "multiLineText");
            var result = SqlTypes.SmiSqlGeographyStmLineFromText.Value.Invoke(null, new[] { sqlMultiLineText, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyMultiPolygonFromText(string multiPolygonText, int srid)
        {
            var sqlMultiPolygonText = ConvertToSqlChars(multiPolygonText, "multiPolygonText");
            var result = SqlTypes.SmiSqlGeographyStmPolyFromText.Value.Invoke(null, new[] { sqlMultiPolygonText, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyCollectionFromText(string geographyCollectionText, int srid)
        {
            var sqlGeographyCollectionText = ConvertToSqlChars(geographyCollectionText, "geographyCollectionText");
            var result = SqlTypes.SmiSqlGeographyStGeomCollFromText.Value.Invoke(null, new[] { sqlGeographyCollectionText, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyFromBinary(byte[] geographyBytes, int srid)
        {
            var sqlGeographyBytes = ConvertToSqlBytes(geographyBytes, "geographyBytes");
            var result = SqlTypes.SmiSqlGeographyStGeomFromWkb.Value.Invoke(null, new[] { sqlGeographyBytes, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyFromBinary(byte[] geographyBytes)
        {
            var sqlGeographyBytes = ConvertToSqlBytes(geographyBytes, "geographyBytes");
            var result = SqlTypes.SmiSqlGeographyStGeomFromWkb.Value.Invoke(null, new[] { sqlGeographyBytes, 4326 });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyPointFromBinary(byte[] pointBytes, int srid)
        {
            var sqlPointBytes = ConvertToSqlBytes(pointBytes, "pointBytes");
            var result = SqlTypes.SmiSqlGeographyStPointFromWkb.Value.Invoke(null, new[] { sqlPointBytes, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyLineFromBinary(byte[] lineBytes, int srid)
        {
            var sqlLineBytes = ConvertToSqlBytes(lineBytes, "lineBytes");
            var result = SqlTypes.SmiSqlGeographyStLineFromWkb.Value.Invoke(null, new[] { sqlLineBytes, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyPolygonFromBinary(byte[] polygonBytes, int srid)
        {
            var sqlPolygonBytes = ConvertToSqlBytes(polygonBytes, "polygonBytes");
            var result = SqlTypes.SmiSqlGeographyStPolyFromWkb.Value.Invoke(null, new[] { sqlPolygonBytes, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyMultiPointFromBinary(byte[] multiPointBytes, int srid)
        {
            var sqlMultiPointBytes = ConvertToSqlBytes(multiPointBytes, "multiPointBytes");
            var result = SqlTypes.SmiSqlGeographyStmPointFromWkb.Value.Invoke(null, new[] { sqlMultiPointBytes, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyMultiLineFromBinary(byte[] multiLineBytes, int srid)
        {
            var sqlMultiLineBytes = ConvertToSqlBytes(multiLineBytes, "multiLineBytes");
            var result = SqlTypes.SmiSqlGeographyStmLineFromWkb.Value.Invoke(null, new[] { sqlMultiLineBytes, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyMultiPolygonFromBinary(byte[] multiPolygonBytes, int srid)
        {
            var sqlMultiPolygonBytes = ConvertToSqlBytes(multiPolygonBytes, "multiPolygonBytes");
            var result = SqlTypes.SmiSqlGeographyStmPolyFromWkb.Value.Invoke(null, new[] { sqlMultiPolygonBytes, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyCollectionFromBinary(byte[] geographyCollectionBytes, int srid)
        {
            var sqlGeographyCollectionBytes = ConvertToSqlBytes(geographyCollectionBytes, "geographyCollectionBytes");
            var result = SqlTypes.SmiSqlGeographyStGeomCollFromWkb.Value.Invoke(null, new[] { sqlGeographyCollectionBytes, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyFromGml(string geographyGml)
        {
            var sqlGeographyGml = ConvertToSqlXml(geographyGml, "geographyGml");
            var result = SqlTypes.SmiSqlGeographyGeomFromGml.Value.Invoke(null, new[] { sqlGeographyGml, 4326 });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyFromGml(string geographyGml, int srid)
        {
            var sqlGeographyGml = ConvertToSqlXml(geographyGml, "geographyGml");
            var result = SqlTypes.SmiSqlGeographyGeomFromGml.Value.Invoke(null, new[] { sqlGeographyGml, srid });
            return GeographyFromProviderValue(result);
        }

        public override int GetCoordinateSystemId(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.IpiSqlGeographyStSrid.Value.GetValue(sqlGeographyValue, null);
            return ConvertSqlInt32ToInt(result);
        }

        public override string GetSpatialTypeName(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.ImiSqlGeographyStGeometryType.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlStringToString(result);
        }

        public override int GetDimension(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.ImiSqlGeographyStDimension.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlInt32ToInt(result);
        }

        public override byte[] AsBinary(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.ImiSqlGeographyStAsBinary.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlBytesToBinary(result);
        }

        public override string AsGml(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.ImiSqlGeographyAsGml.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlXmlToString(result);
        }

        public override string AsText(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.ImiSqlGeographyStAsText.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlCharsToString(result);
        }

        public override bool GetIsEmpty(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.ImiSqlGeographyStIsEmpty.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool SpatialEquals(DbGeography geographyValue, DbGeography otherGeography)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlgeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var sqlotherGeography = ConvertToSqlValue(otherGeography, "otherGeography");
            var result = SqlTypes.ImiSqlGeographyStEquals.Value.Invoke(sqlgeographyValue, new[] { sqlotherGeography });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Disjoint(DbGeography geographyValue, DbGeography otherGeography)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlgeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var sqlotherGeography = ConvertToSqlValue(otherGeography, "otherGeography");
            var result = SqlTypes.ImiSqlGeographyStDisjoint.Value.Invoke(sqlgeographyValue, new[] { sqlotherGeography });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Intersects(DbGeography geographyValue, DbGeography otherGeography)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlgeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var sqlotherGeography = ConvertToSqlValue(otherGeography, "otherGeography");
            var result = SqlTypes.ImiSqlGeographyStIntersects.Value.Invoke(sqlgeographyValue, new[] { sqlotherGeography });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override DbGeography Buffer(DbGeography geographyValue, double distance)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.ImiSqlGeographyStBuffer.Value.Invoke(sqlGeographyValue, new object[] { distance });
            return GeographyFromProviderValue(result);
        }

        public override double Distance(DbGeography geographyValue, DbGeography otherGeography)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlgeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var sqlotherGeography = ConvertToSqlValue(otherGeography, "otherGeography");
            var result = SqlTypes.ImiSqlGeographyStDistance.Value.Invoke(sqlgeographyValue, new[] { sqlotherGeography });
            return ConvertSqlDoubleToDouble(result);
        }

        public override DbGeography Intersection(DbGeography geographyValue, DbGeography otherGeography)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlgeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var sqlotherGeography = ConvertToSqlValue(otherGeography, "otherGeography");
            var result = SqlTypes.ImiSqlGeographyStIntersection.Value.Invoke(sqlgeographyValue, new[] { sqlotherGeography });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography Union(DbGeography geographyValue, DbGeography otherGeography)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlgeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var sqlotherGeography = ConvertToSqlValue(otherGeography, "otherGeography");
            var result = SqlTypes.ImiSqlGeographyStUnion.Value.Invoke(sqlgeographyValue, new[] { sqlotherGeography });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography Difference(DbGeography geographyValue, DbGeography otherGeography)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlgeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var sqlotherGeography = ConvertToSqlValue(otherGeography, "otherGeography");
            var result = SqlTypes.ImiSqlGeographyStDifference.Value.Invoke(sqlgeographyValue, new[] { sqlotherGeography });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography SymmetricDifference(DbGeography geographyValue, DbGeography otherGeography)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlgeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var sqlotherGeography = ConvertToSqlValue(otherGeography, "otherGeography");
            var result = SqlTypes.ImiSqlGeographyStSymDifference.Value.Invoke(sqlgeographyValue, new[] { sqlotherGeography });
            return GeographyFromProviderValue(result);
        }

        public override int? GetElementCount(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.ImiSqlGeographyStNumGeometries.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlInt32ToNullableInt(result);
        }

        public override DbGeography ElementAt(DbGeography geographyValue, int nValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.ImiSqlGeographyStGeometryN.Value.Invoke(sqlGeographyValue, new object[] { nValue });
            return GeographyFromProviderValue(result);
        }

        public override double? GetLatitude(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.IpiSqlGeographyLat.Value.GetValue(sqlGeographyValue, null);
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override double? GetLongitude(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.IpiSqlGeographyLong.Value.GetValue(sqlGeographyValue, null);
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override double? GetElevation(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.IpiSqlGeographyZ.Value.GetValue(sqlGeographyValue, null);
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override double? GetMeasure(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.IpiSqlGeographyM.Value.GetValue(sqlGeographyValue, null);
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override double? GetLength(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.ImiSqlGeographyStLength.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override DbGeography GetStartPoint(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.ImiSqlGeographyStStartPoint.Value.Invoke(sqlGeographyValue, new object[] { });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GetEndPoint(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.ImiSqlGeographyStEndPoint.Value.Invoke(sqlGeographyValue, new object[] { });
            return GeographyFromProviderValue(result);
        }

        public override bool? GetIsClosed(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.ImiSqlGeographyStIsClosed.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlBooleanToNullableBoolean(result);
        }

        public override int? GetPointCount(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.ImiSqlGeographyStNumPoints.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlInt32ToNullableInt(result);
        }

        public override DbGeography PointAt(DbGeography geographyValue, int nValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.ImiSqlGeographyStPointN.Value.Invoke(sqlGeographyValue, new object[] { nValue });
            return GeographyFromProviderValue(result);
        }

        public override double? GetArea(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = SqlTypes.ImiSqlGeographyStArea.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override DbGeometry GeometryFromText(string geometryText)
        {
            var sqlGeometryText = ConvertToSqlString(geometryText, "geometryText");
            var result = SqlTypes.SmiSqlGeometryParse.Value.Invoke(null, new[] { sqlGeometryText });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryFromText(string geometryText, int srid)
        {
            var sqlGeometryText = ConvertToSqlChars(geometryText, "geometryText");
            var result = SqlTypes.SmiSqlGeometryStGeomFromText.Value.Invoke(null, new[] { sqlGeometryText, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryPointFromText(string pointText, int srid)
        {
            var sqlPointText = ConvertToSqlChars(pointText, "pointText");
            var result = SqlTypes.SmiSqlGeometryStPointFromText.Value.Invoke(null, new[] { sqlPointText, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryLineFromText(string lineText, int srid)
        {
            var sqlLineText = ConvertToSqlChars(lineText, "lineText");
            var result = SqlTypes.SmiSqlGeometryStLineFromText.Value.Invoke(null, new[] { sqlLineText, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryPolygonFromText(string polygonText, int srid)
        {
            var sqlPolygonText = ConvertToSqlChars(polygonText, "polygonText");
            var result = SqlTypes.SmiSqlGeometryStPolyFromText.Value.Invoke(null, new[] { sqlPolygonText, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryMultiPointFromText(string multiPointText, int srid)
        {
            var sqlMultiPointText = ConvertToSqlChars(multiPointText, "multiPointText");
            var result = SqlTypes.SmiSqlGeometryStmPointFromText.Value.Invoke(null, new[] { sqlMultiPointText, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryMultiLineFromText(string multiLineText, int srid)
        {
            var sqlMultiLineText = ConvertToSqlChars(multiLineText, "multiLineText");
            var result = SqlTypes.SmiSqlGeometryStmLineFromText.Value.Invoke(null, new[] { sqlMultiLineText, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryMultiPolygonFromText(string multiPolygonText, int srid)
        {
            var sqlMultiPolygonText = ConvertToSqlChars(multiPolygonText, "multiPolygonText");
            var result = SqlTypes.SmiSqlGeometryStmPolyFromText.Value.Invoke(null, new[] { sqlMultiPolygonText, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryCollectionFromText(string geometryCollectionText, int srid)
        {
            var sqlGeometryCollectionText = ConvertToSqlChars(geometryCollectionText, "geometryCollectionText");
            var result = SqlTypes.SmiSqlGeometryStGeomCollFromText.Value.Invoke(null, new[] { sqlGeometryCollectionText, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryFromBinary(byte[] geometryBytes)
        {
            var sqlGeometryBytes = ConvertToSqlBytes(geometryBytes, "geometryBytes");
            var result = SqlTypes.SmiSqlGeometryStGeomFromWkb.Value.Invoke(null, new[] { sqlGeometryBytes, 0 });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryFromBinary(byte[] geometryBytes, int srid)
        {
            var sqlGeometryBytes = ConvertToSqlBytes(geometryBytes, "geometryBytes");
            var result = SqlTypes.SmiSqlGeometryStGeomFromWkb.Value.Invoke(null, new[] { sqlGeometryBytes, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryPointFromBinary(byte[] pointBytes, int srid)
        {
            var sqlPointBytes = ConvertToSqlBytes(pointBytes, "pointBytes");
            var result = SqlTypes.SmiSqlGeometryStPointFromWkb.Value.Invoke(null, new[] { sqlPointBytes, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryLineFromBinary(byte[] lineBytes, int srid)
        {
            var sqlLineBytes = ConvertToSqlBytes(lineBytes, "lineBytes");
            var result = SqlTypes.SmiSqlGeometryStLineFromWkb.Value.Invoke(null, new[] { sqlLineBytes, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryPolygonFromBinary(byte[] polygonBytes, int srid)
        {
            var sqlPolygonBytes = ConvertToSqlBytes(polygonBytes, "polygonBytes");
            var result = SqlTypes.SmiSqlGeometryStPolyFromWkb.Value.Invoke(null, new[] { sqlPolygonBytes, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryMultiPointFromBinary(byte[] multiPointBytes, int srid)
        {
            var sqlMultiPointBytes = ConvertToSqlBytes(multiPointBytes, "multiPointBytes");
            var result = SqlTypes.SmiSqlGeometryStmPointFromWkb.Value.Invoke(null, new[] { sqlMultiPointBytes, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryMultiLineFromBinary(byte[] multiLineBytes, int srid)
        {
            var sqlMultiLineBytes = ConvertToSqlBytes(multiLineBytes, "multiLineBytes");
            var result = SqlTypes.SmiSqlGeometryStmLineFromWkb.Value.Invoke(null, new[] { sqlMultiLineBytes, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryMultiPolygonFromBinary(byte[] multiPolygonBytes, int srid)
        {
            var sqlMultiPolygonBytes = ConvertToSqlBytes(multiPolygonBytes, "multiPolygonBytes");
            var result = SqlTypes.SmiSqlGeometryStmPolyFromWkb.Value.Invoke(null, new[] { sqlMultiPolygonBytes, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryCollectionFromBinary(byte[] geometryCollectionBytes, int srid)
        {
            var sqlGeometryCollectionBytes = ConvertToSqlBytes(geometryCollectionBytes, "geometryCollectionBytes");
            var result = SqlTypes.SmiSqlGeometryStGeomCollFromWkb.Value.Invoke(null, new[] { sqlGeometryCollectionBytes, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryFromGml(string geometryGml)
        {
            var sqlGeometryGml = ConvertToSqlXml(geometryGml, "geometryGml");
            var result = SqlTypes.SmiSqlGeometryGeomFromGml.Value.Invoke(null, new[] { sqlGeometryGml, 0 });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryFromGml(string geometryGml, int srid)
        {
            var sqlGeometryGml = ConvertToSqlXml(geometryGml, "geometryGml");
            var result = SqlTypes.SmiSqlGeometryGeomFromGml.Value.Invoke(null, new[] { sqlGeometryGml, srid });
            return GeometryFromProviderValue(result);
        }

        public override int GetCoordinateSystemId(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.IpiSqlGeometryStSrid.Value.GetValue(sqlGeometryValue, null);
            return ConvertSqlInt32ToInt(result);
        }

        public override string GetSpatialTypeName(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStGeometryType.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlStringToString(result);
        }

        public override int GetDimension(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStDimension.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlInt32ToInt(result);
        }

        public override DbGeometry GetEnvelope(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStEnvelope.Value.Invoke(sqlGeometryValue, new object[] { });
            return GeometryFromProviderValue(result);
        }

        public override byte[] AsBinary(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStAsBinary.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlBytesToBinary(result);
        }

        public override string AsGml(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryAsGml.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlXmlToString(result);
        }

        public override string AsText(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStAsText.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlCharsToString(result);
        }

        public override bool GetIsEmpty(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStIsEmpty.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool GetIsSimple(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStIsSimple.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override DbGeometry GetBoundary(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStBoundary.Value.Invoke(sqlGeometryValue, new object[] { });
            return GeometryFromProviderValue(result);
        }

        public override bool GetIsValid(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStIsValid.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool SpatialEquals(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = SqlTypes.ImiSqlGeometryStEquals.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Disjoint(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = SqlTypes.ImiSqlGeometryStDisjoint.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Intersects(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = SqlTypes.ImiSqlGeometryStIntersects.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Touches(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = SqlTypes.ImiSqlGeometryStTouches.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Crosses(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = SqlTypes.ImiSqlGeometryStCrosses.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Within(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = SqlTypes.ImiSqlGeometryStWithin.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Contains(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = SqlTypes.ImiSqlGeometryStContains.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Overlaps(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");
            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = SqlTypes.ImiSqlGeometryStOverlaps.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Relate(DbGeometry geometryValue, DbGeometry otherGeometry, string matrix)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = SqlTypes.ImiSqlGeometryStRelate.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry, matrix });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override DbGeometry Buffer(DbGeometry geometryValue, double distance)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStBuffer.Value.Invoke(sqlGeometryValue, new object[] { distance });
            return GeometryFromProviderValue(result);
        }

        public override double Distance(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = SqlTypes.ImiSqlGeometryStDistance.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return ConvertSqlDoubleToDouble(result);
        }

        public override DbGeometry GetConvexHull(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStConvexHull.Value.Invoke(sqlGeometryValue, new object[] { });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry Intersection(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = SqlTypes.ImiSqlGeometryStIntersection.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry Union(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = SqlTypes.ImiSqlGeometryStUnion.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry Difference(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = SqlTypes.ImiSqlGeometryStDifference.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry SymmetricDifference(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = SqlTypes.ImiSqlGeometryStSymDifference.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return GeometryFromProviderValue(result);
        }

        public override int? GetElementCount(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStNumGeometries.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlInt32ToNullableInt(result);
        }

        public override DbGeometry ElementAt(DbGeometry geometryValue, int nValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStGeometryN.Value.Invoke(sqlGeometryValue, new object[] { nValue });
            return GeometryFromProviderValue(result);
        }

        public override double? GetXCoordinate(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.IpiSqlGeometryStx.Value.GetValue(sqlGeometryValue, null);
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override double? GetYCoordinate(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.IpiSqlGeometrySty.Value.GetValue(sqlGeometryValue, null);
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override double? GetElevation(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.IpiSqlGeometryZ.Value.GetValue(sqlGeometryValue, null);
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override double? GetMeasure(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.IpiSqlGeometryM.Value.GetValue(sqlGeometryValue, null);
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override double? GetLength(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStLength.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override DbGeometry GetStartPoint(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStStartPoint.Value.Invoke(sqlGeometryValue, new object[] { });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GetEndPoint(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStEndPoint.Value.Invoke(sqlGeometryValue, new object[] { });
            return GeometryFromProviderValue(result);
        }

        public override bool? GetIsClosed(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStIsClosed.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlBooleanToNullableBoolean(result);
        }

        public override bool? GetIsRing(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStIsRing.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlBooleanToNullableBoolean(result);
        }

        public override int? GetPointCount(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStNumPoints.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlInt32ToNullableInt(result);
        }

        public override DbGeometry PointAt(DbGeometry geometryValue, int nValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStPointN.Value.Invoke(sqlGeometryValue, new object[] { nValue });
            return GeometryFromProviderValue(result);
        }

        public override double? GetArea(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStArea.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override DbGeometry GetCentroid(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStCentroid.Value.Invoke(sqlGeometryValue, new object[] { });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GetPointOnSurface(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStPointOnSurface.Value.Invoke(sqlGeometryValue, new object[] { });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GetExteriorRing(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStExteriorRing.Value.Invoke(sqlGeometryValue, new object[] { });
            return GeometryFromProviderValue(result);
        }

        public override int? GetInteriorRingCount(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStNumInteriorRing.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlInt32ToNullableInt(result);
        }

        public override DbGeometry InteriorRingAt(DbGeometry geometryValue, int nValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = SqlTypes.ImiSqlGeometryStInteriorRingN.Value.Invoke(sqlGeometryValue, new object[] { nValue });
            return GeometryFromProviderValue(result);
        }
    }
}
