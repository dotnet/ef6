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

    /// <summary>
    /// SqlClient specific implementation of <see cref="DbSpatialServices"/>
    /// </summary>
    [Serializable]
    internal sealed partial class SqlSpatialServices : DbSpatialServices, ISerializable
    {
        /// <summary>
        /// Do not allow instantiation
        /// </summary>
        internal static readonly SqlSpatialServices Instance = new SqlSpatialServices(SqlProviderServices.GetSqlTypesAssembly);

        private static Dictionary<string, SqlSpatialServices> otherSpatialServices;

        [NonSerialized]
        private readonly Lazy<SqlTypesAssembly> _sqlTypesAssemblySingleton;

        internal SqlSpatialServices()
        {
        }

        private SqlSpatialServices(Func<SqlTypesAssembly> getSqlTypes)
        {
            Debug.Assert(getSqlTypes != null, "Validate SqlTypes assembly delegate before constructing SqlSpatialServiceS");
            _sqlTypesAssemblySingleton = new Lazy<SqlTypesAssembly>(getSqlTypes, isThreadSafe: true);

            // Create Singletons that will delay-initialize the MethodInfo and PropertyInfo instances used to invoke SqlGeography/SqlGeometry methods via reflection.
            InitializeMemberInfo();
        }

        private SqlSpatialServices(SerializationInfo info, StreamingContext context)
        {
            var instance = Instance;
            _sqlTypesAssemblySingleton = instance._sqlTypesAssemblySingleton;
            InitializeMemberInfo(instance);
        }

        // Given an assembly purportedly containing SqlServerTypes for spatial values, attempt to 
        // create a corersponding Sql spefic DbSpatialServices value backed by types from that assembly.
        // Uses a dictionary to ensure that there is at most db spatial service per assembly.   It's important that
        // this be done in a way that ensures that the underlying SqlTypesAssembly value is also atomized,
        // since that's caching compilation.
        // Relies on SqlTypesAssembly to verify that the assembly is appropriate.
        private static bool TryGetSpatialServiceFromAssembly(Assembly assembly, out SqlSpatialServices services)
        {
            if (otherSpatialServices == null
                || !otherSpatialServices.TryGetValue(assembly.FullName, out services))
            {
                lock (Instance)
                {
                    if (otherSpatialServices == null
                        || !otherSpatialServices.TryGetValue(assembly.FullName, out services))
                    {
                        SqlTypesAssembly sqlAssembly;
                        if (SqlTypesAssembly.TryGetSqlTypesAssembly(assembly, out sqlAssembly))
                        {
                            if (otherSpatialServices == null)
                            {
                                otherSpatialServices = new Dictionary<string, SqlSpatialServices>(1);
                            }
                            services = new SqlSpatialServices(() => sqlAssembly);
                            otherSpatialServices.Add(assembly.FullName, services);
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
            // Cannot use Contract.Requires here because this is an override and the contract always
            // gets compiled out in release builds.
            Throw.IfNull(wellKnownValue, "wellKnownValue");

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
            // Cannot use Contract.Requires here because this is an override and the contract always
            // gets compiled out in release builds.
            Throw.IfNull(providerValue, "providerValue");

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

                throw new ArgumentException(Strings.SqlSpatialServices_ProviderValueNotSqlType(expectedSpatialType.AssemblyQualifiedName), "providerValue");
            }

            return providerValue;
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        public override DbGeographyWellKnownValue CreateWellKnownValue(DbGeography geographyValue)
        {
            // Cannot use Contract.Requires here because this is an override and the contract always
            // gets compiled out in release builds.
            Throw.IfNull(geographyValue, "geographyValue");

            var spatialValue = geographyValue.AsSpatialValue();

            var result = CreateWellKnownValue(
                spatialValue,
                () => (Exception)new ArgumentException(Strings.SqlSpatialservices_CouldNotCreateWellKnownGeographyValueNoSrid, "geographyValue"),
                () => (Exception)new ArgumentException(Strings.SqlSpatialservices_CouldNotCreateWellKnownGeographyValueNoWkbOrWkt, "geographyValue"),
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
            // Cannot use Contract.Requires here because this is an override and the contract always
            // gets compiled out in release builds.
            Throw.IfNull(wellKnownValue, "wellKnownValue");

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
            // Cannot use Contract.Requires here because this is an override and the contract always
            // gets compiled out in release builds.
            Throw.IfNull(providerValue, "providerValue");

            var normalizedProviderValue = NormalizeProviderValue(providerValue, SqlTypes.SqlGeometryType);
            return SqlTypes.IsSqlGeometryNull(normalizedProviderValue) ? null : CreateGeometry(this, normalizedProviderValue);
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        public override DbGeometryWellKnownValue CreateWellKnownValue(DbGeometry geometryValue)
        {
            // Cannot use Contract.Requires here because this is an override and the contract always
            // gets compiled out in release builds.
            Throw.IfNull(geometryValue, "geometryValue");

            var spatialValue = geometryValue.AsSpatialValue();

            var result = CreateWellKnownValue(
                spatialValue,
                () => (Exception)new ArgumentException(Strings.SqlSpatialservices_CouldNotCreateWellKnownGeometryValueNoSrid, "geometryValue"),
                () => (Exception)new ArgumentException(Strings.SqlSpatialservices_CouldNotCreateWellKnownGeometryValueNoWkbOrWkt, "geometryValue"),
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
            return SqlTypes.GeographyAsTextZM(geographyValue);
        }

        public override string AsTextIncludingElevationAndMeasure(DbGeometry geometryValue)
        {
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
    }
}
