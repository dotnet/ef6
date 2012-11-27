// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Collections.Generic;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer.Resources;
    using System.Data.Entity.SqlServer.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime.Serialization;

    [Serializable]
    internal sealed class SqlSpatialServices : DbSpatialServices, ISerializable
    {
        internal static readonly SqlSpatialServices Instance
            = new SqlSpatialServices(new SqlTypesAssemblyLoader(), l => l.GetSqlTypesAssembly());

        private static Dictionary<string, SqlSpatialServices> _otherSpatialServices;

        [NonSerialized]
        private readonly Lazy<SqlTypesAssembly> _sqlTypesAssemblySingleton;

        [NonSerialized]
        private readonly SqlTypesAssemblyLoader _sqlTypesAssemblyLoader;

        public SqlSpatialServices()
        {
        }

        public SqlSpatialServices(SqlTypesAssemblyLoader sqlTypesAssemblyLoader, Func<SqlTypesAssemblyLoader, SqlTypesAssembly> getSqlTypes)
        {
            DebugCheck.NotNull(getSqlTypes);
            DebugCheck.NotNull(sqlTypesAssemblyLoader);

            _sqlTypesAssemblyLoader = sqlTypesAssemblyLoader;
            _sqlTypesAssemblySingleton = new Lazy<SqlTypesAssembly>(() => getSqlTypes(sqlTypesAssemblyLoader), isThreadSafe: true);

            // Create Singletons that will delay-initialize the MethodInfo and PropertyInfo instances
            // used to invoke SqlGeography/SqlGeometry methods via reflection.
            InitializeMemberInfo();
        }

        public SqlSpatialServices(SerializationInfo info, StreamingContext context)
        {
            _sqlTypesAssemblyLoader = Instance._sqlTypesAssemblyLoader;
            _sqlTypesAssemblySingleton = Instance._sqlTypesAssemblySingleton;
            InitializeMemberInfo(Instance);
        }

        /// <inheritdoc />
        public override bool NativeTypesAvailable
        {
            get { return _sqlTypesAssemblyLoader.TryGetSqlTypesAssembly() != null; }
        }

        // Given an assembly purportedly containing SqlServerTypes for spatial values, attempt to 
        // create a corresponding SQL specific DbSpatialServices value backed by types from that assembly.
        // Uses a dictionary to ensure that there is at most db spatial service per assembly.   It's important that
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
                        var loader = new SqlTypesAssemblyLoader();
                        if (loader.TryGetSqlTypesAssembly(assembly, out sqlAssembly))
                        {
                            if (_otherSpatialServices == null)
                            {
                                _otherSpatialServices = new Dictionary<string, SqlSpatialServices>(1);
                            }
                            services = new SqlSpatialServices(loader, l => sqlAssembly);
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

        private SqlTypesAssembly SqlTypes
        {
            get { return _sqlTypesAssemblySingleton.Value; }
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

        // Ensure that provider values are from the expected version of the Sql types assembly.   If they aren't try to 
        // convert them so that they are.
        // 
        // Normally when we obtain values from the store, we try to use the appropriate SqlSpatialDataReader.   This will make sure that 
        // any spatial values are instantiated with the provider type from the appropriate SqlServerTypes assembly.   However, 
        // in one case (output parameter values) we don't have an opportunity to make this happen.    There we get whatever value 
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
                    else // expectedSpatialType == this.SqlTypes.SqlGeometryType
                    {
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

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // no need to serialize anything, on de-serialization we reinitialize all fields to match the 
            // those of Instance.
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

        #region API used by generated spatial implementation methods

        #region Reflection - remove if SqlSpatialServices uses compiled expressions instead of reflection to invoke SqlGeography/SqlGeometry methods

        private MethodInfo FindSqlGeographyMethod(string methodName, params Type[] argTypes)
        {
            return SqlTypes.SqlGeographyType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, argTypes, null);
        }

        private MethodInfo FindSqlGeographyStaticMethod(string methodName, params Type[] argTypes)
        {
            return SqlTypes.SqlGeographyType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, argTypes, null);
        }

        private PropertyInfo FindSqlGeographyProperty(string propertyName)
        {
            return SqlTypes.SqlGeographyType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        }

        private MethodInfo FindSqlGeometryStaticMethod(string methodName, params Type[] argTypes)
        {
            return SqlTypes.SqlGeometryType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, argTypes, null);
        }

        private MethodInfo FindSqlGeometryMethod(string methodName, params Type[] argTypes)
        {
            return SqlTypes.SqlGeometryType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, argTypes, null);
        }

        private PropertyInfo FindSqlGeometryProperty(string propertyName)
        {
            return SqlTypes.SqlGeometryType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        }

        #endregion

        #region Argument Conversion (conversion to SQL Server Types)

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

        #endregion

        #region Return Value Conversion (conversion from SQL Server types)

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

        #endregion

        #endregion

        public override DbGeography GeographyFromText(string geographyText)
        {
            var sqlGeographyText = ConvertToSqlString(geographyText, "geographyText");
            var result = smi_SqlGeography_Parse.Value.Invoke(null, new[] { sqlGeographyText });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyFromText(string geographyText, int srid)
        {
            var sqlGeographyText = ConvertToSqlChars(geographyText, "geographyText");
            var result = smi_SqlGeography_STGeomFromText.Value.Invoke(null, new[] { sqlGeographyText, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyPointFromText(string pointText, int srid)
        {
            var sqlPointText = ConvertToSqlChars(pointText, "pointText");
            var result = smi_SqlGeography_STPointFromText.Value.Invoke(null, new[] { sqlPointText, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyLineFromText(string lineText, int srid)
        {
            var sqlLineText = ConvertToSqlChars(lineText, "lineText");
            var result = smi_SqlGeography_STLineFromText.Value.Invoke(null, new[] { sqlLineText, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyPolygonFromText(string polygonText, int srid)
        {
            var sqlPolygonText = ConvertToSqlChars(polygonText, "polygonText");
            var result = smi_SqlGeography_STPolyFromText.Value.Invoke(null, new[] { sqlPolygonText, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyMultiPointFromText(string multiPointText, int srid)
        {
            var sqlMultiPointText = ConvertToSqlChars(multiPointText, "multiPointText");
            var result = smi_SqlGeography_STMPointFromText.Value.Invoke(null, new[] { sqlMultiPointText, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyMultiLineFromText(string multiLineText, int srid)
        {
            var sqlMultiLineText = ConvertToSqlChars(multiLineText, "multiLineText");
            var result = smi_SqlGeography_STMLineFromText.Value.Invoke(null, new[] { sqlMultiLineText, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyMultiPolygonFromText(string multiPolygonText, int srid)
        {
            var sqlMultiPolygonText = ConvertToSqlChars(multiPolygonText, "multiPolygonText");
            var result = smi_SqlGeography_STMPolyFromText.Value.Invoke(null, new[] { sqlMultiPolygonText, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyCollectionFromText(string geographyCollectionText, int srid)
        {
            var sqlGeographyCollectionText = ConvertToSqlChars(geographyCollectionText, "geographyCollectionText");
            var result = smi_SqlGeography_STGeomCollFromText.Value.Invoke(null, new[] { sqlGeographyCollectionText, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyFromBinary(byte[] geographyBytes, int srid)
        {
            var sqlGeographyBytes = ConvertToSqlBytes(geographyBytes, "geographyBytes");
            var result = smi_SqlGeography_STGeomFromWKB.Value.Invoke(null, new[] { sqlGeographyBytes, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyFromBinary(byte[] geographyBytes)
        {
            var sqlGeographyBytes = ConvertToSqlBytes(geographyBytes, "geographyBytes");
            var result = smi_SqlGeography_STGeomFromWKB.Value.Invoke(null, new[] { sqlGeographyBytes, 4326 });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyPointFromBinary(byte[] pointBytes, int srid)
        {
            var sqlPointBytes = ConvertToSqlBytes(pointBytes, "pointBytes");
            var result = smi_SqlGeography_STPointFromWKB.Value.Invoke(null, new[] { sqlPointBytes, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyLineFromBinary(byte[] lineBytes, int srid)
        {
            var sqlLineBytes = ConvertToSqlBytes(lineBytes, "lineBytes");
            var result = smi_SqlGeography_STLineFromWKB.Value.Invoke(null, new[] { sqlLineBytes, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyPolygonFromBinary(byte[] polygonBytes, int srid)
        {
            var sqlPolygonBytes = ConvertToSqlBytes(polygonBytes, "polygonBytes");
            var result = smi_SqlGeography_STPolyFromWKB.Value.Invoke(null, new[] { sqlPolygonBytes, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyMultiPointFromBinary(byte[] multiPointBytes, int srid)
        {
            var sqlMultiPointBytes = ConvertToSqlBytes(multiPointBytes, "multiPointBytes");
            var result = smi_SqlGeography_STMPointFromWKB.Value.Invoke(null, new[] { sqlMultiPointBytes, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyMultiLineFromBinary(byte[] multiLineBytes, int srid)
        {
            var sqlMultiLineBytes = ConvertToSqlBytes(multiLineBytes, "multiLineBytes");
            var result = smi_SqlGeography_STMLineFromWKB.Value.Invoke(null, new[] { sqlMultiLineBytes, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyMultiPolygonFromBinary(byte[] multiPolygonBytes, int srid)
        {
            var sqlMultiPolygonBytes = ConvertToSqlBytes(multiPolygonBytes, "multiPolygonBytes");
            var result = smi_SqlGeography_STMPolyFromWKB.Value.Invoke(null, new[] { sqlMultiPolygonBytes, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyCollectionFromBinary(byte[] geographyCollectionBytes, int srid)
        {
            var sqlGeographyCollectionBytes = ConvertToSqlBytes(geographyCollectionBytes, "geographyCollectionBytes");
            var result = smi_SqlGeography_STGeomCollFromWKB.Value.Invoke(null, new[] { sqlGeographyCollectionBytes, srid });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyFromGml(string geographyGml)
        {
            var sqlGeographyGml = ConvertToSqlXml(geographyGml, "geographyGml");
            var result = smi_SqlGeography_GeomFromGml.Value.Invoke(null, new[] { sqlGeographyGml, 4326 });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GeographyFromGml(string geographyGml, int srid)
        {
            var sqlGeographyGml = ConvertToSqlXml(geographyGml, "geographyGml");
            var result = smi_SqlGeography_GeomFromGml.Value.Invoke(null, new[] { sqlGeographyGml, srid });
            return GeographyFromProviderValue(result);
        }

        public override int GetCoordinateSystemId(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = ipi_SqlGeography_STSrid.Value.GetValue(sqlGeographyValue, null);
            return ConvertSqlInt32ToInt(result);
        }

        public override string GetSpatialTypeName(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = imi_SqlGeography_STGeometryType.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlStringToString(result);
        }

        public override int GetDimension(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = imi_SqlGeography_STDimension.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlInt32ToInt(result);
        }

        public override byte[] AsBinary(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = imi_SqlGeography_STAsBinary.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlBytesToBinary(result);
        }

        public override string AsGml(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = imi_SqlGeography_AsGml.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlXmlToString(result);
        }

        public override string AsText(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = imi_SqlGeography_STAsText.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlCharsToString(result);
        }

        public override bool GetIsEmpty(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = imi_SqlGeography_STIsEmpty.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool SpatialEquals(DbGeography geographyValue, DbGeography otherGeography)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlgeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var sqlotherGeography = ConvertToSqlValue(otherGeography, "otherGeography");
            var result = imi_SqlGeography_STEquals.Value.Invoke(sqlgeographyValue, new[] { sqlotherGeography });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Disjoint(DbGeography geographyValue, DbGeography otherGeography)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlgeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var sqlotherGeography = ConvertToSqlValue(otherGeography, "otherGeography");
            var result = imi_SqlGeography_STDisjoint.Value.Invoke(sqlgeographyValue, new[] { sqlotherGeography });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Intersects(DbGeography geographyValue, DbGeography otherGeography)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlgeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var sqlotherGeography = ConvertToSqlValue(otherGeography, "otherGeography");
            var result = imi_SqlGeography_STIntersects.Value.Invoke(sqlgeographyValue, new[] { sqlotherGeography });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override DbGeography Buffer(DbGeography geographyValue, double distance)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = imi_SqlGeography_STBuffer.Value.Invoke(sqlGeographyValue, new object[] { distance });
            return GeographyFromProviderValue(result);
        }

        public override double Distance(DbGeography geographyValue, DbGeography otherGeography)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlgeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var sqlotherGeography = ConvertToSqlValue(otherGeography, "otherGeography");
            var result = imi_SqlGeography_STDistance.Value.Invoke(sqlgeographyValue, new[] { sqlotherGeography });
            return ConvertSqlDoubleToDouble(result);
        }

        public override DbGeography Intersection(DbGeography geographyValue, DbGeography otherGeography)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlgeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var sqlotherGeography = ConvertToSqlValue(otherGeography, "otherGeography");
            var result = imi_SqlGeography_STIntersection.Value.Invoke(sqlgeographyValue, new[] { sqlotherGeography });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography Union(DbGeography geographyValue, DbGeography otherGeography)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlgeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var sqlotherGeography = ConvertToSqlValue(otherGeography, "otherGeography");
            var result = imi_SqlGeography_STUnion.Value.Invoke(sqlgeographyValue, new[] { sqlotherGeography });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography Difference(DbGeography geographyValue, DbGeography otherGeography)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlgeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var sqlotherGeography = ConvertToSqlValue(otherGeography, "otherGeography");
            var result = imi_SqlGeography_STDifference.Value.Invoke(sqlgeographyValue, new[] { sqlotherGeography });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography SymmetricDifference(DbGeography geographyValue, DbGeography otherGeography)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlgeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var sqlotherGeography = ConvertToSqlValue(otherGeography, "otherGeography");
            var result = imi_SqlGeography_STSymDifference.Value.Invoke(sqlgeographyValue, new[] { sqlotherGeography });
            return GeographyFromProviderValue(result);
        }

        public override int? GetElementCount(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = imi_SqlGeography_STNumGeometries.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlInt32ToNullableInt(result);
        }

        public override DbGeography ElementAt(DbGeography geographyValue, int nValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = imi_SqlGeography_STGeometryN.Value.Invoke(sqlGeographyValue, new object[] { nValue });
            return GeographyFromProviderValue(result);
        }

        public override double? GetLatitude(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = ipi_SqlGeography_Lat.Value.GetValue(sqlGeographyValue, null);
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override double? GetLongitude(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = ipi_SqlGeography_Long.Value.GetValue(sqlGeographyValue, null);
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override double? GetElevation(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = ipi_SqlGeography_Z.Value.GetValue(sqlGeographyValue, null);
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override double? GetMeasure(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = ipi_SqlGeography_M.Value.GetValue(sqlGeographyValue, null);
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override double? GetLength(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = imi_SqlGeography_STLength.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override DbGeography GetStartPoint(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = imi_SqlGeography_STStartPoint.Value.Invoke(sqlGeographyValue, new object[] { });
            return GeographyFromProviderValue(result);
        }

        public override DbGeography GetEndPoint(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = imi_SqlGeography_STEndPoint.Value.Invoke(sqlGeographyValue, new object[] { });
            return GeographyFromProviderValue(result);
        }

        public override bool? GetIsClosed(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = imi_SqlGeography_STIsClosed.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlBooleanToNullableBoolean(result);
        }

        public override int? GetPointCount(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = imi_SqlGeography_STNumPoints.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlInt32ToNullableInt(result);
        }

        public override DbGeography PointAt(DbGeography geographyValue, int nValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = imi_SqlGeography_STPointN.Value.Invoke(sqlGeographyValue, new object[] { nValue });
            return GeographyFromProviderValue(result);
        }

        public override double? GetArea(DbGeography geographyValue)
        {
            Check.NotNull(geographyValue, "geographyValue");

            var sqlGeographyValue = ConvertToSqlValue(geographyValue, "geographyValue");
            var result = imi_SqlGeography_STArea.Value.Invoke(sqlGeographyValue, new object[] { });
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override DbGeometry GeometryFromText(string geometryText)
        {
            var sqlGeometryText = ConvertToSqlString(geometryText, "geometryText");
            var result = smi_SqlGeometry_Parse.Value.Invoke(null, new[] { sqlGeometryText });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryFromText(string geometryText, int srid)
        {
            var sqlGeometryText = ConvertToSqlChars(geometryText, "geometryText");
            var result = smi_SqlGeometry_STGeomFromText.Value.Invoke(null, new[] { sqlGeometryText, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryPointFromText(string pointText, int srid)
        {
            var sqlPointText = ConvertToSqlChars(pointText, "pointText");
            var result = smi_SqlGeometry_STPointFromText.Value.Invoke(null, new[] { sqlPointText, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryLineFromText(string lineText, int srid)
        {
            var sqlLineText = ConvertToSqlChars(lineText, "lineText");
            var result = smi_SqlGeometry_STLineFromText.Value.Invoke(null, new[] { sqlLineText, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryPolygonFromText(string polygonText, int srid)
        {
            var sqlPolygonText = ConvertToSqlChars(polygonText, "polygonText");
            var result = smi_SqlGeometry_STPolyFromText.Value.Invoke(null, new[] { sqlPolygonText, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryMultiPointFromText(string multiPointText, int srid)
        {
            var sqlMultiPointText = ConvertToSqlChars(multiPointText, "multiPointText");
            var result = smi_SqlGeometry_STMPointFromText.Value.Invoke(null, new[] { sqlMultiPointText, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryMultiLineFromText(string multiLineText, int srid)
        {
            var sqlMultiLineText = ConvertToSqlChars(multiLineText, "multiLineText");
            var result = smi_SqlGeometry_STMLineFromText.Value.Invoke(null, new[] { sqlMultiLineText, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryMultiPolygonFromText(string multiPolygonText, int srid)
        {
            var sqlMultiPolygonText = ConvertToSqlChars(multiPolygonText, "multiPolygonText");
            var result = smi_SqlGeometry_STMPolyFromText.Value.Invoke(null, new[] { sqlMultiPolygonText, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryCollectionFromText(string geometryCollectionText, int srid)
        {
            var sqlGeometryCollectionText = ConvertToSqlChars(geometryCollectionText, "geometryCollectionText");
            var result = smi_SqlGeometry_STGeomCollFromText.Value.Invoke(null, new[] { sqlGeometryCollectionText, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryFromBinary(byte[] geometryBytes)
        {
            var sqlGeometryBytes = ConvertToSqlBytes(geometryBytes, "geometryBytes");
            var result = smi_SqlGeometry_STGeomFromWKB.Value.Invoke(null, new[] { sqlGeometryBytes, 0 });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryFromBinary(byte[] geometryBytes, int srid)
        {
            var sqlGeometryBytes = ConvertToSqlBytes(geometryBytes, "geometryBytes");
            var result = smi_SqlGeometry_STGeomFromWKB.Value.Invoke(null, new[] { sqlGeometryBytes, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryPointFromBinary(byte[] pointBytes, int srid)
        {
            var sqlPointBytes = ConvertToSqlBytes(pointBytes, "pointBytes");
            var result = smi_SqlGeometry_STPointFromWKB.Value.Invoke(null, new[] { sqlPointBytes, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryLineFromBinary(byte[] lineBytes, int srid)
        {
            var sqlLineBytes = ConvertToSqlBytes(lineBytes, "lineBytes");
            var result = smi_SqlGeometry_STLineFromWKB.Value.Invoke(null, new[] { sqlLineBytes, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryPolygonFromBinary(byte[] polygonBytes, int srid)
        {
            var sqlPolygonBytes = ConvertToSqlBytes(polygonBytes, "polygonBytes");
            var result = smi_SqlGeometry_STPolyFromWKB.Value.Invoke(null, new[] { sqlPolygonBytes, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryMultiPointFromBinary(byte[] multiPointBytes, int srid)
        {
            var sqlMultiPointBytes = ConvertToSqlBytes(multiPointBytes, "multiPointBytes");
            var result = smi_SqlGeometry_STMPointFromWKB.Value.Invoke(null, new[] { sqlMultiPointBytes, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryMultiLineFromBinary(byte[] multiLineBytes, int srid)
        {
            var sqlMultiLineBytes = ConvertToSqlBytes(multiLineBytes, "multiLineBytes");
            var result = smi_SqlGeometry_STMLineFromWKB.Value.Invoke(null, new[] { sqlMultiLineBytes, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryMultiPolygonFromBinary(byte[] multiPolygonBytes, int srid)
        {
            var sqlMultiPolygonBytes = ConvertToSqlBytes(multiPolygonBytes, "multiPolygonBytes");
            var result = smi_SqlGeometry_STMPolyFromWKB.Value.Invoke(null, new[] { sqlMultiPolygonBytes, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryCollectionFromBinary(byte[] geometryCollectionBytes, int srid)
        {
            var sqlGeometryCollectionBytes = ConvertToSqlBytes(geometryCollectionBytes, "geometryCollectionBytes");
            var result = smi_SqlGeometry_STGeomCollFromWKB.Value.Invoke(null, new[] { sqlGeometryCollectionBytes, srid });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryFromGml(string geometryGml)
        {
            var sqlGeometryGml = ConvertToSqlXml(geometryGml, "geometryGml");
            var result = smi_SqlGeometry_GeomFromGml.Value.Invoke(null, new[] { sqlGeometryGml, 0 });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GeometryFromGml(string geometryGml, int srid)
        {
            var sqlGeometryGml = ConvertToSqlXml(geometryGml, "geometryGml");
            var result = smi_SqlGeometry_GeomFromGml.Value.Invoke(null, new[] { sqlGeometryGml, srid });
            return GeometryFromProviderValue(result);
        }

        public override int GetCoordinateSystemId(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = ipi_SqlGeometry_STSrid.Value.GetValue(sqlGeometryValue, null);
            return ConvertSqlInt32ToInt(result);
        }

        public override string GetSpatialTypeName(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STGeometryType.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlStringToString(result);
        }

        public override int GetDimension(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STDimension.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlInt32ToInt(result);
        }

        public override DbGeometry GetEnvelope(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STEnvelope.Value.Invoke(sqlGeometryValue, new object[] { });
            return GeometryFromProviderValue(result);
        }

        public override byte[] AsBinary(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STAsBinary.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlBytesToBinary(result);
        }

        public override string AsGml(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_AsGml.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlXmlToString(result);
        }

        public override string AsText(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STAsText.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlCharsToString(result);
        }

        public override bool GetIsEmpty(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STIsEmpty.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool GetIsSimple(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STIsSimple.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override DbGeometry GetBoundary(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");
            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STBoundary.Value.Invoke(sqlGeometryValue, new object[] { });
            return GeometryFromProviderValue(result);
        }

        public override bool GetIsValid(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STIsValid.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool SpatialEquals(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = imi_SqlGeometry_STEquals.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Disjoint(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = imi_SqlGeometry_STDisjoint.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Intersects(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = imi_SqlGeometry_STIntersects.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Touches(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = imi_SqlGeometry_STTouches.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Crosses(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = imi_SqlGeometry_STCrosses.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Within(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = imi_SqlGeometry_STWithin.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Contains(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = imi_SqlGeometry_STContains.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Overlaps(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");
            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = imi_SqlGeometry_STOverlaps.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override bool Relate(DbGeometry geometryValue, DbGeometry otherGeometry, string matrix)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = imi_SqlGeometry_STRelate.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry, matrix });
            return ConvertSqlBooleanToBoolean(result);
        }

        public override DbGeometry Buffer(DbGeometry geometryValue, double distance)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STBuffer.Value.Invoke(sqlGeometryValue, new object[] { distance });
            return GeometryFromProviderValue(result);
        }

        public override double Distance(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = imi_SqlGeometry_STDistance.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return ConvertSqlDoubleToDouble(result);
        }

        public override DbGeometry GetConvexHull(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STConvexHull.Value.Invoke(sqlGeometryValue, new object[] { });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry Intersection(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = imi_SqlGeometry_STIntersection.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry Union(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = imi_SqlGeometry_STUnion.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry Difference(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = imi_SqlGeometry_STDifference.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry SymmetricDifference(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlgeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var sqlotherGeometry = ConvertToSqlValue(otherGeometry, "otherGeometry");
            var result = imi_SqlGeometry_STSymDifference.Value.Invoke(sqlgeometryValue, new[] { sqlotherGeometry });
            return GeometryFromProviderValue(result);
        }

        public override int? GetElementCount(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STNumGeometries.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlInt32ToNullableInt(result);
        }

        public override DbGeometry ElementAt(DbGeometry geometryValue, int nValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STGeometryN.Value.Invoke(sqlGeometryValue, new object[] { nValue });
            return GeometryFromProviderValue(result);
        }

        public override double? GetXCoordinate(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = ipi_SqlGeometry_STX.Value.GetValue(sqlGeometryValue, null);
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override double? GetYCoordinate(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = ipi_SqlGeometry_STY.Value.GetValue(sqlGeometryValue, null);
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override double? GetElevation(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = ipi_SqlGeometry_Z.Value.GetValue(sqlGeometryValue, null);
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override double? GetMeasure(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = ipi_SqlGeometry_M.Value.GetValue(sqlGeometryValue, null);
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override double? GetLength(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STLength.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override DbGeometry GetStartPoint(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STStartPoint.Value.Invoke(sqlGeometryValue, new object[] { });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GetEndPoint(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STEndPoint.Value.Invoke(sqlGeometryValue, new object[] { });
            return GeometryFromProviderValue(result);
        }

        public override bool? GetIsClosed(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STIsClosed.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlBooleanToNullableBoolean(result);
        }

        public override bool? GetIsRing(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STIsRing.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlBooleanToNullableBoolean(result);
        }

        public override int? GetPointCount(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STNumPoints.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlInt32ToNullableInt(result);
        }

        public override DbGeometry PointAt(DbGeometry geometryValue, int nValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STPointN.Value.Invoke(sqlGeometryValue, new object[] { nValue });
            return GeometryFromProviderValue(result);
        }

        public override double? GetArea(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STArea.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlDoubleToNullableDouble(result);
        }

        public override DbGeometry GetCentroid(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STCentroid.Value.Invoke(sqlGeometryValue, new object[] { });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GetPointOnSurface(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STPointOnSurface.Value.Invoke(sqlGeometryValue, new object[] { });
            return GeometryFromProviderValue(result);
        }

        public override DbGeometry GetExteriorRing(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STExteriorRing.Value.Invoke(sqlGeometryValue, new object[] { });
            return GeometryFromProviderValue(result);
        }

        public override int? GetInteriorRingCount(DbGeometry geometryValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STNumInteriorRing.Value.Invoke(sqlGeometryValue, new object[] { });
            return ConvertSqlInt32ToNullableInt(result);
        }

        public override DbGeometry InteriorRingAt(DbGeometry geometryValue, int nValue)
        {
            Check.NotNull(geometryValue, "geometryValue");

            var sqlGeometryValue = ConvertToSqlValue(geometryValue, "geometryValue");
            var result = imi_SqlGeometry_STInteriorRingN.Value.Invoke(sqlGeometryValue, new object[] { nValue });
            return GeometryFromProviderValue(result);
        }

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeography_Parse;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeography_STGeomFromText;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeography_STPointFromText;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeography_STLineFromText;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeography_STPolyFromText;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeography_STMPointFromText;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeography_STMLineFromText;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeography_STMPolyFromText;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeography_STGeomCollFromText;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeography_STGeomFromWKB;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeography_STPointFromWKB;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeography_STLineFromWKB;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeography_STPolyFromWKB;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeography_STMPointFromWKB;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeography_STMLineFromWKB;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeography_STMPolyFromWKB;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeography_STGeomCollFromWKB;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeography_GeomFromGml;

        [NonSerialized]
        private Lazy<PropertyInfo> ipi_SqlGeography_STSrid;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STGeometryType;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STDimension;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STAsBinary;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_AsGml;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STAsText;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STIsEmpty;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STEquals;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STDisjoint;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STIntersects;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STBuffer;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STDistance;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STIntersection;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STUnion;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STDifference;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STSymDifference;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STNumGeometries;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STGeometryN;

        [NonSerialized]
        private Lazy<PropertyInfo> ipi_SqlGeography_Lat;

        [NonSerialized]
        private Lazy<PropertyInfo> ipi_SqlGeography_Long;

        [NonSerialized]
        private Lazy<PropertyInfo> ipi_SqlGeography_Z;

        [NonSerialized]
        private Lazy<PropertyInfo> ipi_SqlGeography_M;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STLength;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STStartPoint;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STEndPoint;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STIsClosed;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STNumPoints;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STPointN;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeography_STArea;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeometry_Parse;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeometry_STGeomFromText;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeometry_STPointFromText;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeometry_STLineFromText;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeometry_STPolyFromText;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeometry_STMPointFromText;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeometry_STMLineFromText;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeometry_STMPolyFromText;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeometry_STGeomCollFromText;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeometry_STGeomFromWKB;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeometry_STPointFromWKB;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeometry_STLineFromWKB;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeometry_STPolyFromWKB;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeometry_STMPointFromWKB;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeometry_STMLineFromWKB;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeometry_STMPolyFromWKB;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeometry_STGeomCollFromWKB;

        [NonSerialized]
        private Lazy<MethodInfo> smi_SqlGeometry_GeomFromGml;

        [NonSerialized]
        private Lazy<PropertyInfo> ipi_SqlGeometry_STSrid;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STGeometryType;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STDimension;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STEnvelope;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STAsBinary;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_AsGml;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STAsText;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STIsEmpty;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STIsSimple;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STBoundary;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STIsValid;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STEquals;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STDisjoint;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STIntersects;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STTouches;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STCrosses;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STWithin;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STContains;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STOverlaps;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STRelate;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STBuffer;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STDistance;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STConvexHull;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STIntersection;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STUnion;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STDifference;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STSymDifference;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STNumGeometries;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STGeometryN;

        [NonSerialized]
        private Lazy<PropertyInfo> ipi_SqlGeometry_STX;

        [NonSerialized]
        private Lazy<PropertyInfo> ipi_SqlGeometry_STY;

        [NonSerialized]
        private Lazy<PropertyInfo> ipi_SqlGeometry_Z;

        [NonSerialized]
        private Lazy<PropertyInfo> ipi_SqlGeometry_M;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STLength;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STStartPoint;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STEndPoint;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STIsClosed;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STIsRing;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STNumPoints;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STPointN;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STArea;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STCentroid;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STPointOnSurface;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STExteriorRing;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STNumInteriorRing;

        [NonSerialized]
        private Lazy<MethodInfo> imi_SqlGeometry_STInteriorRingN;

        private void InitializeMemberInfo()
        {
            smi_SqlGeography_Parse = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("Parse", SqlTypes.SqlStringType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member Parse");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeography_STGeomFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STGeomFromText", SqlTypes.SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STGeomFromText");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeography_STPointFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STPointFromText", SqlTypes.SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STPointFromText");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeography_STLineFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STLineFromText", SqlTypes.SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STLineFromText");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeography_STPolyFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STPolyFromText", SqlTypes.SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STPolyFromText");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeography_STMPointFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STMPointFromText", SqlTypes.SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STMPointFromText");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeography_STMLineFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STMLineFromText", SqlTypes.SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STMLineFromText");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeography_STMPolyFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STMPolyFromText", SqlTypes.SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STMPolyFromText");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeography_STGeomCollFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STGeomCollFromText", SqlTypes.SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STGeomCollFromText");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeography_STGeomFromWKB = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STGeomFromWKB", SqlTypes.SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STGeomFromWKB");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeography_STPointFromWKB = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STPointFromWKB", SqlTypes.SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STPointFromWKB");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeography_STLineFromWKB = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STLineFromWKB", SqlTypes.SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STLineFromWKB");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeography_STPolyFromWKB = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STPolyFromWKB", SqlTypes.SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STPolyFromWKB");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeography_STMPointFromWKB = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STMPointFromWKB", SqlTypes.SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STMPointFromWKB");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeography_STMLineFromWKB = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STMLineFromWKB", SqlTypes.SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STMLineFromWKB");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeography_STMPolyFromWKB = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STMPolyFromWKB", SqlTypes.SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STMPolyFromWKB");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeography_STGeomCollFromWKB = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STGeomCollFromWKB", SqlTypes.SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STGeomCollFromWKB");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeography_GeomFromGml = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("GeomFromGml", SqlTypes.SqlXmlType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member GeomFromGml");
                        return result;
                    }, isThreadSafe: true);
            ipi_SqlGeography_STSrid = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeographyProperty("STSrid");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeography member STSrid");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STGeometryType = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STGeometryType");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STGeometryType");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STDimension = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STDimension");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STDimension");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STAsBinary = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STAsBinary");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STAsBinary");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_AsGml = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("AsGml");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member AsGml");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STAsText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STAsText");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STAsText");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STIsEmpty = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STIsEmpty");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STIsEmpty");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STEquals = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STEquals", SqlTypes.SqlGeographyType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STEquals");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STDisjoint = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STDisjoint", SqlTypes.SqlGeographyType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STDisjoint");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STIntersects = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STIntersects", SqlTypes.SqlGeographyType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STIntersects");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STBuffer = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STBuffer", typeof(double));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STBuffer");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STDistance = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STDistance", SqlTypes.SqlGeographyType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STDistance");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STIntersection = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STIntersection", SqlTypes.SqlGeographyType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STIntersection");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STUnion = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STUnion", SqlTypes.SqlGeographyType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STUnion");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STDifference = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STDifference", SqlTypes.SqlGeographyType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STDifference");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STSymDifference = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STSymDifference", SqlTypes.SqlGeographyType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STSymDifference");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STNumGeometries = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STNumGeometries");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STNumGeometries");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STGeometryN = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STGeometryN", typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STGeometryN");
                        return result;
                    }, isThreadSafe: true);
            ipi_SqlGeography_Lat = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeographyProperty("Lat");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeography member Lat");
                        return result;
                    }, isThreadSafe: true);
            ipi_SqlGeography_Long = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeographyProperty("Long");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeography member Long");
                        return result;
                    }, isThreadSafe: true);
            ipi_SqlGeography_Z = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeographyProperty("Z");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeography member Z");
                        return result;
                    }, isThreadSafe: true);
            ipi_SqlGeography_M = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeographyProperty("M");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeography member M");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STLength = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STLength");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STLength");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STStartPoint = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STStartPoint");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STStartPoint");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STEndPoint = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STEndPoint");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STEndPoint");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STIsClosed = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STIsClosed");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STIsClosed");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STNumPoints = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STNumPoints");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STNumPoints");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STPointN = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STPointN", typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STPointN");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeography_STArea = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STArea");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STArea");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeometry_Parse = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("Parse", SqlTypes.SqlStringType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member Parse");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeometry_STGeomFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STGeomFromText", SqlTypes.SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STGeomFromText");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeometry_STPointFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STPointFromText", SqlTypes.SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STPointFromText");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeometry_STLineFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STLineFromText", SqlTypes.SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STLineFromText");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeometry_STPolyFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STPolyFromText", SqlTypes.SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STPolyFromText");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeometry_STMPointFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STMPointFromText", SqlTypes.SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STMPointFromText");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeometry_STMLineFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STMLineFromText", SqlTypes.SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STMLineFromText");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeometry_STMPolyFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STMPolyFromText", SqlTypes.SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STMPolyFromText");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeometry_STGeomCollFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STGeomCollFromText", SqlTypes.SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STGeomCollFromText");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeometry_STGeomFromWKB = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STGeomFromWKB", SqlTypes.SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STGeomFromWKB");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeometry_STPointFromWKB = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STPointFromWKB", SqlTypes.SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STPointFromWKB");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeometry_STLineFromWKB = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STLineFromWKB", SqlTypes.SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STLineFromWKB");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeometry_STPolyFromWKB = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STPolyFromWKB", SqlTypes.SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STPolyFromWKB");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeometry_STMPointFromWKB = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STMPointFromWKB", SqlTypes.SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STMPointFromWKB");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeometry_STMLineFromWKB = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STMLineFromWKB", SqlTypes.SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STMLineFromWKB");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeometry_STMPolyFromWKB = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STMPolyFromWKB", SqlTypes.SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STMPolyFromWKB");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeometry_STGeomCollFromWKB = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STGeomCollFromWKB", SqlTypes.SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STGeomCollFromWKB");
                        return result;
                    }, isThreadSafe: true);
            smi_SqlGeometry_GeomFromGml = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("GeomFromGml", SqlTypes.SqlXmlType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member GeomFromGml");
                        return result;
                    }, isThreadSafe: true);
            ipi_SqlGeometry_STSrid = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeometryProperty("STSrid");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeometry member STSrid");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STGeometryType = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STGeometryType");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STGeometryType");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STDimension = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STDimension");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STDimension");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STEnvelope = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STEnvelope");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STEnvelope");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STAsBinary = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STAsBinary");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STAsBinary");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_AsGml = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("AsGml");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member AsGml");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STAsText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STAsText");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STAsText");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STIsEmpty = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STIsEmpty");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STIsEmpty");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STIsSimple = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STIsSimple");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STIsSimple");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STBoundary = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STBoundary");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STBoundary");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STIsValid = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STIsValid");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STIsValid");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STEquals = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STEquals", SqlTypes.SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STEquals");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STDisjoint = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STDisjoint", SqlTypes.SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STDisjoint");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STIntersects = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STIntersects", SqlTypes.SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STIntersects");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STTouches = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STTouches", SqlTypes.SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STTouches");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STCrosses = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STCrosses", SqlTypes.SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STCrosses");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STWithin = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STWithin", SqlTypes.SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STWithin");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STContains = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STContains", SqlTypes.SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STContains");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STOverlaps = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STOverlaps", SqlTypes.SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STOverlaps");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STRelate = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STRelate", SqlTypes.SqlGeometryType, typeof(string));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STRelate");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STBuffer = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STBuffer", typeof(double));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STBuffer");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STDistance = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STDistance", SqlTypes.SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STDistance");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STConvexHull = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STConvexHull");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STConvexHull");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STIntersection = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STIntersection", SqlTypes.SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STIntersection");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STUnion = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STUnion", SqlTypes.SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STUnion");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STDifference = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STDifference", SqlTypes.SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STDifference");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STSymDifference = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STSymDifference", SqlTypes.SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STSymDifference");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STNumGeometries = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STNumGeometries");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STNumGeometries");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STGeometryN = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STGeometryN", typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STGeometryN");
                        return result;
                    }, isThreadSafe: true);
            ipi_SqlGeometry_STX = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeometryProperty("STX");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeometry member STX");
                        return result;
                    }, isThreadSafe: true);
            ipi_SqlGeometry_STY = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeometryProperty("STY");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeometry member STY");
                        return result;
                    }, isThreadSafe: true);
            ipi_SqlGeometry_Z = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeometryProperty("Z");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeometry member Z");
                        return result;
                    }, isThreadSafe: true);
            ipi_SqlGeometry_M = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeometryProperty("M");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeometry member M");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STLength = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STLength");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STLength");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STStartPoint = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STStartPoint");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STStartPoint");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STEndPoint = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STEndPoint");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STEndPoint");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STIsClosed = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STIsClosed");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STIsClosed");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STIsRing = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STIsRing");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STIsRing");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STNumPoints = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STNumPoints");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STNumPoints");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STPointN = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STPointN", typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STPointN");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STArea = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STArea");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STArea");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STCentroid = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STCentroid");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STCentroid");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STPointOnSurface = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STPointOnSurface");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STPointOnSurface");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STExteriorRing = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STExteriorRing");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STExteriorRing");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STNumInteriorRing = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STNumInteriorRing");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STNumInteriorRing");
                        return result;
                    }, isThreadSafe: true);
            imi_SqlGeometry_STInteriorRingN = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STInteriorRingN", typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STInteriorRingN");
                        return result;
                    }, isThreadSafe: true);
        }

        private void InitializeMemberInfo(SqlSpatialServices from)
        {
            smi_SqlGeography_Parse = @from.smi_SqlGeography_Parse;
            smi_SqlGeography_STGeomFromText = @from.smi_SqlGeography_STGeomFromText;
            smi_SqlGeography_STPointFromText = @from.smi_SqlGeography_STPointFromText;
            smi_SqlGeography_STLineFromText = @from.smi_SqlGeography_STLineFromText;
            smi_SqlGeography_STPolyFromText = @from.smi_SqlGeography_STPolyFromText;
            smi_SqlGeography_STMPointFromText = @from.smi_SqlGeography_STMPointFromText;
            smi_SqlGeography_STMLineFromText = @from.smi_SqlGeography_STMLineFromText;
            smi_SqlGeography_STMPolyFromText = @from.smi_SqlGeography_STMPolyFromText;
            smi_SqlGeography_STGeomCollFromText = @from.smi_SqlGeography_STGeomCollFromText;
            smi_SqlGeography_STGeomFromWKB = @from.smi_SqlGeography_STGeomFromWKB;
            smi_SqlGeography_STPointFromWKB = @from.smi_SqlGeography_STPointFromWKB;
            smi_SqlGeography_STLineFromWKB = @from.smi_SqlGeography_STLineFromWKB;
            smi_SqlGeography_STPolyFromWKB = @from.smi_SqlGeography_STPolyFromWKB;
            smi_SqlGeography_STMPointFromWKB = @from.smi_SqlGeography_STMPointFromWKB;
            smi_SqlGeography_STMLineFromWKB = @from.smi_SqlGeography_STMLineFromWKB;
            smi_SqlGeography_STMPolyFromWKB = @from.smi_SqlGeography_STMPolyFromWKB;
            smi_SqlGeography_STGeomCollFromWKB = @from.smi_SqlGeography_STGeomCollFromWKB;
            smi_SqlGeography_GeomFromGml = @from.smi_SqlGeography_GeomFromGml;
            ipi_SqlGeography_STSrid = @from.ipi_SqlGeography_STSrid;
            imi_SqlGeography_STGeometryType = @from.imi_SqlGeography_STGeometryType;
            imi_SqlGeography_STDimension = @from.imi_SqlGeography_STDimension;
            imi_SqlGeography_STAsBinary = @from.imi_SqlGeography_STAsBinary;
            imi_SqlGeography_AsGml = @from.imi_SqlGeography_AsGml;
            imi_SqlGeography_STAsText = @from.imi_SqlGeography_STAsText;
            imi_SqlGeography_STIsEmpty = @from.imi_SqlGeography_STIsEmpty;
            imi_SqlGeography_STEquals = @from.imi_SqlGeography_STEquals;
            imi_SqlGeography_STDisjoint = @from.imi_SqlGeography_STDisjoint;
            imi_SqlGeography_STIntersects = @from.imi_SqlGeography_STIntersects;
            imi_SqlGeography_STBuffer = @from.imi_SqlGeography_STBuffer;
            imi_SqlGeography_STDistance = @from.imi_SqlGeography_STDistance;
            imi_SqlGeography_STIntersection = @from.imi_SqlGeography_STIntersection;
            imi_SqlGeography_STUnion = @from.imi_SqlGeography_STUnion;
            imi_SqlGeography_STDifference = @from.imi_SqlGeography_STDifference;
            imi_SqlGeography_STSymDifference = @from.imi_SqlGeography_STSymDifference;
            imi_SqlGeography_STNumGeometries = @from.imi_SqlGeography_STNumGeometries;
            imi_SqlGeography_STGeometryN = @from.imi_SqlGeography_STGeometryN;
            ipi_SqlGeography_Lat = @from.ipi_SqlGeography_Lat;
            ipi_SqlGeography_Long = @from.ipi_SqlGeography_Long;
            ipi_SqlGeography_Z = @from.ipi_SqlGeography_Z;
            ipi_SqlGeography_M = @from.ipi_SqlGeography_M;
            imi_SqlGeography_STLength = @from.imi_SqlGeography_STLength;
            imi_SqlGeography_STStartPoint = @from.imi_SqlGeography_STStartPoint;
            imi_SqlGeography_STEndPoint = @from.imi_SqlGeography_STEndPoint;
            imi_SqlGeography_STIsClosed = @from.imi_SqlGeography_STIsClosed;
            imi_SqlGeography_STNumPoints = @from.imi_SqlGeography_STNumPoints;
            imi_SqlGeography_STPointN = @from.imi_SqlGeography_STPointN;
            imi_SqlGeography_STArea = @from.imi_SqlGeography_STArea;
            smi_SqlGeometry_Parse = @from.smi_SqlGeometry_Parse;
            smi_SqlGeometry_STGeomFromText = @from.smi_SqlGeometry_STGeomFromText;
            smi_SqlGeometry_STPointFromText = @from.smi_SqlGeometry_STPointFromText;
            smi_SqlGeometry_STLineFromText = @from.smi_SqlGeometry_STLineFromText;
            smi_SqlGeometry_STPolyFromText = @from.smi_SqlGeometry_STPolyFromText;
            smi_SqlGeometry_STMPointFromText = @from.smi_SqlGeometry_STMPointFromText;
            smi_SqlGeometry_STMLineFromText = @from.smi_SqlGeometry_STMLineFromText;
            smi_SqlGeometry_STMPolyFromText = @from.smi_SqlGeometry_STMPolyFromText;
            smi_SqlGeometry_STGeomCollFromText = @from.smi_SqlGeometry_STGeomCollFromText;
            smi_SqlGeometry_STGeomFromWKB = @from.smi_SqlGeometry_STGeomFromWKB;
            smi_SqlGeometry_STPointFromWKB = @from.smi_SqlGeometry_STPointFromWKB;
            smi_SqlGeometry_STLineFromWKB = @from.smi_SqlGeometry_STLineFromWKB;
            smi_SqlGeometry_STPolyFromWKB = @from.smi_SqlGeometry_STPolyFromWKB;
            smi_SqlGeometry_STMPointFromWKB = @from.smi_SqlGeometry_STMPointFromWKB;
            smi_SqlGeometry_STMLineFromWKB = @from.smi_SqlGeometry_STMLineFromWKB;
            smi_SqlGeometry_STMPolyFromWKB = @from.smi_SqlGeometry_STMPolyFromWKB;
            smi_SqlGeometry_STGeomCollFromWKB = @from.smi_SqlGeometry_STGeomCollFromWKB;
            smi_SqlGeometry_GeomFromGml = @from.smi_SqlGeometry_GeomFromGml;
            ipi_SqlGeometry_STSrid = @from.ipi_SqlGeometry_STSrid;
            imi_SqlGeometry_STGeometryType = @from.imi_SqlGeometry_STGeometryType;
            imi_SqlGeometry_STDimension = @from.imi_SqlGeometry_STDimension;
            imi_SqlGeometry_STEnvelope = @from.imi_SqlGeometry_STEnvelope;
            imi_SqlGeometry_STAsBinary = @from.imi_SqlGeometry_STAsBinary;
            imi_SqlGeometry_AsGml = @from.imi_SqlGeometry_AsGml;
            imi_SqlGeometry_STAsText = @from.imi_SqlGeometry_STAsText;
            imi_SqlGeometry_STIsEmpty = @from.imi_SqlGeometry_STIsEmpty;
            imi_SqlGeometry_STIsSimple = @from.imi_SqlGeometry_STIsSimple;
            imi_SqlGeometry_STBoundary = @from.imi_SqlGeometry_STBoundary;
            imi_SqlGeometry_STIsValid = @from.imi_SqlGeometry_STIsValid;
            imi_SqlGeometry_STEquals = @from.imi_SqlGeometry_STEquals;
            imi_SqlGeometry_STDisjoint = @from.imi_SqlGeometry_STDisjoint;
            imi_SqlGeometry_STIntersects = @from.imi_SqlGeometry_STIntersects;
            imi_SqlGeometry_STTouches = @from.imi_SqlGeometry_STTouches;
            imi_SqlGeometry_STCrosses = @from.imi_SqlGeometry_STCrosses;
            imi_SqlGeometry_STWithin = @from.imi_SqlGeometry_STWithin;
            imi_SqlGeometry_STContains = @from.imi_SqlGeometry_STContains;
            imi_SqlGeometry_STOverlaps = @from.imi_SqlGeometry_STOverlaps;
            imi_SqlGeometry_STRelate = @from.imi_SqlGeometry_STRelate;
            imi_SqlGeometry_STBuffer = @from.imi_SqlGeometry_STBuffer;
            imi_SqlGeometry_STDistance = @from.imi_SqlGeometry_STDistance;
            imi_SqlGeometry_STConvexHull = @from.imi_SqlGeometry_STConvexHull;
            imi_SqlGeometry_STIntersection = @from.imi_SqlGeometry_STIntersection;
            imi_SqlGeometry_STUnion = @from.imi_SqlGeometry_STUnion;
            imi_SqlGeometry_STDifference = @from.imi_SqlGeometry_STDifference;
            imi_SqlGeometry_STSymDifference = @from.imi_SqlGeometry_STSymDifference;
            imi_SqlGeometry_STNumGeometries = @from.imi_SqlGeometry_STNumGeometries;
            imi_SqlGeometry_STGeometryN = @from.imi_SqlGeometry_STGeometryN;
            ipi_SqlGeometry_STX = @from.ipi_SqlGeometry_STX;
            ipi_SqlGeometry_STY = @from.ipi_SqlGeometry_STY;
            ipi_SqlGeometry_Z = @from.ipi_SqlGeometry_Z;
            ipi_SqlGeometry_M = @from.ipi_SqlGeometry_M;
            imi_SqlGeometry_STLength = @from.imi_SqlGeometry_STLength;
            imi_SqlGeometry_STStartPoint = @from.imi_SqlGeometry_STStartPoint;
            imi_SqlGeometry_STEndPoint = @from.imi_SqlGeometry_STEndPoint;
            imi_SqlGeometry_STIsClosed = @from.imi_SqlGeometry_STIsClosed;
            imi_SqlGeometry_STIsRing = @from.imi_SqlGeometry_STIsRing;
            imi_SqlGeometry_STNumPoints = @from.imi_SqlGeometry_STNumPoints;
            imi_SqlGeometry_STPointN = @from.imi_SqlGeometry_STPointN;
            imi_SqlGeometry_STArea = @from.imi_SqlGeometry_STArea;
            imi_SqlGeometry_STCentroid = @from.imi_SqlGeometry_STCentroid;
            imi_SqlGeometry_STPointOnSurface = @from.imi_SqlGeometry_STPointOnSurface;
            imi_SqlGeometry_STExteriorRing = @from.imi_SqlGeometry_STExteriorRing;
            imi_SqlGeometry_STNumInteriorRing = @from.imi_SqlGeometry_STNumInteriorRing;
            imi_SqlGeometry_STInteriorRingN = @from.imi_SqlGeometry_STInteriorRingN;
        }
    }
}
