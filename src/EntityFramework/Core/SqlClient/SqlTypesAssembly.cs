namespace System.Data.Entity.Core.SqlClient
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Spatial;
    using System.Data.Entity.Core.SqlClient.Internal;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Xml;

    internal static class Expressions
    {
        internal static Expression Null<TNullType>()
        {
            return Expression.Constant(null, typeof(TNullType));
        }

        internal static Expression Null(Type nullType)
        {
            return Expression.Constant(null, nullType);
        }

        internal static Expression<Func<TArg, TResult>> Lambda<TArg, TResult>(
            string argumentName, Func<ParameterExpression, Expression> createLambdaBodyGivenParameter)
        {
            var argParam = Expression.Parameter(typeof(TArg), argumentName);
            var lambdaBody = createLambdaBodyGivenParameter(argParam);
            return Expression.Lambda<Func<TArg, TResult>>(lambdaBody, argParam);
        }

        internal static Expression Call(this Expression exp, string methodName)
        {
            return Expression.Call(exp, methodName, Type.EmptyTypes);
        }

        internal static Expression ConvertTo(this Expression exp, Type convertToType)
        {
            return Expression.Convert(exp, convertToType);
        }

        internal static Expression ConvertTo<TConvertToType>(this Expression exp)
        {
            return Expression.Convert(exp, typeof(TConvertToType));
        }

        internal sealed class ConditionalExpressionBuilder
        {
            private readonly Expression condition;
            private readonly Expression ifTrueThen;

            internal ConditionalExpressionBuilder(Expression conditionExpression, Expression ifTrueExpression)
            {
                condition = conditionExpression;
                ifTrueThen = ifTrueExpression;
            }

            internal Expression Else(Expression resultIfFalse)
            {
                return Expression.Condition(condition, ifTrueThen, resultIfFalse);
            }
        }

        internal static ConditionalExpressionBuilder IfTrueThen(this Expression conditionExp, Expression resultIfTrue)
        {
            return new ConditionalExpressionBuilder(conditionExp, resultIfTrue);
        }

        internal static Expression Property<TPropertyType>(this Expression exp, string propertyName)
        {
            var prop = exp.Type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            Debug.Assert(
                prop != null,
                "Type '" + exp.Type.FullName + "' does not declare a public instance property with the name '" + propertyName + "'");

            return Expression.Property(exp, prop);
        }
    }

    /// <summary>
    /// SqlTypesAssembly allows for late binding to the capabilities of a specific version of the Microsoft.SqlServer.Types assembly
    /// </summary>
    internal sealed class SqlTypesAssembly
    {
        private static readonly ReadOnlyCollection<string> preferredSqlTypesAssemblies = new List<string>
                                                                                             {
                                                                                                 "Microsoft.SqlServer.Types, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91",
                                                                                                 "Microsoft.SqlServer.Types, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91",
                                                                                             }.AsReadOnly();

        private static SqlTypesAssembly BindToLatest()
        {
            Assembly sqlTypesAssembly = null;
            foreach (var assemblyFullName in preferredSqlTypesAssemblies)
            {
                var asmName = new AssemblyName(assemblyFullName);
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

            if (sqlTypesAssembly != null)
            {
                return new SqlTypesAssembly(sqlTypesAssembly);
            }
            return null;
        }

        internal static bool TryGetSqlTypesAssembly(Assembly assembly, out SqlTypesAssembly sqlAssembly)
        {
            if (IsKnownAssembly(assembly))
            {
                sqlAssembly = new SqlTypesAssembly(assembly);
                return true;
            }
            sqlAssembly = null;
            return false;
        }

        private static bool IsKnownAssembly(Assembly assembly)
        {
            foreach (var knownAssemblyFullName in preferredSqlTypesAssemblies)
            {
                if (EntityUtil.AssemblyNamesMatch(assembly.FullName, new AssemblyName(knownAssemblyFullName)))
                {
                    return true;
                }
            }
            return false;
        }

        private static readonly Singleton<SqlTypesAssembly> latestVersion = new Singleton<SqlTypesAssembly>(BindToLatest);

        /// <summary>
        /// Returns the highest available version of the Microsoft.SqlServer.Types assembly that could be located using Assembly.Load; may return <c>null</c> if no version of the assembly could be found.
        /// </summary>
        internal static SqlTypesAssembly Latest
        {
            get { return latestVersion.Value; }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private SqlTypesAssembly(Assembly sqlSpatialAssembly)
        {
            // Retrieve SQL Server spatial types and static constructor methods
            var sqlGeog = sqlSpatialAssembly.GetType("Microsoft.SqlServer.Types.SqlGeography", throwOnError: true);
            var sqlGeom = sqlSpatialAssembly.GetType("Microsoft.SqlServer.Types.SqlGeometry", throwOnError: true);

            Debug.Assert(sqlGeog != null, "SqlGeography type was not properly retrieved?");
            Debug.Assert(sqlGeom != null, "SqlGeometry type was not properly retrieved?");

            SqlGeographyType = sqlGeog;
            sqlGeographyFromWKTString = CreateStaticConstructorDelegate<string>(sqlGeog, "STGeomFromText");
            sqlGeographyFromWKBByteArray = CreateStaticConstructorDelegate<byte[]>(sqlGeog, "STGeomFromWKB");
            sqlGeographyFromGMLReader = CreateStaticConstructorDelegate<XmlReader>(sqlGeog, "GeomFromGml");

            SqlGeometryType = sqlGeom;
            sqlGeometryFromWKTString = CreateStaticConstructorDelegate<string>(sqlGeom, "STGeomFromText");
            sqlGeometryFromWKBByteArray = CreateStaticConstructorDelegate<byte[]>(sqlGeom, "STGeomFromWKB");
            sqlGeometryFromGMLReader = CreateStaticConstructorDelegate<XmlReader>(sqlGeom, "GeomFromGml");

            // Retrieve SQL Server specific primitive types
            var asTextMethod = SqlGeometryType.GetMethod(
                "STAsText", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            SqlCharsType = asTextMethod.ReturnType;
            SqlStringType = SqlCharsType.Assembly.GetType("System.Data.SqlTypes.SqlString", throwOnError: true);
            SqlBooleanType = SqlCharsType.Assembly.GetType("System.Data.SqlTypes.SqlBoolean", throwOnError: true);
            SqlBytesType = SqlCharsType.Assembly.GetType("System.Data.SqlTypes.SqlBytes", throwOnError: true);
            SqlDoubleType = SqlCharsType.Assembly.GetType("System.Data.SqlTypes.SqlDouble", throwOnError: true);
            SqlInt32Type = SqlCharsType.Assembly.GetType("System.Data.SqlTypes.SqlInt32", throwOnError: true);
            SqlXmlType = SqlCharsType.Assembly.GetType("System.Data.SqlTypes.SqlXml", throwOnError: true);

            // Create type conversion delegates to SQL Server types
            sqlBytesFromByteArray =
                Expressions.Lambda<byte[], object>("binaryValue", bytesVal => BuildConvertToSqlBytes(bytesVal, SqlBytesType)).Compile();
            sqlStringFromString =
                Expressions.Lambda<string, object>("stringValue", stringVal => BuildConvertToSqlString(stringVal, SqlStringType)).Compile();
            sqlCharsFromString =
                Expressions.Lambda<string, object>("stringValue", stringVal => BuildConvertToSqlChars(stringVal, SqlCharsType)).Compile();
            sqlXmlFromXmlReader =
                Expressions.Lambda<XmlReader, object>("readerVaue", readerVal => BuildConvertToSqlXml(readerVal, SqlXmlType)).Compile();

            // Create type conversion delegates from SQL Server types; all arguments are typed as 'object' and require Expression.Convert before use of members of the SQL Server type.

            // Explicit cast from SqlBoolean to bool
            sqlBooleanToBoolean =
                Expressions.Lambda<object, bool>("sqlBooleanValue", sqlBoolVal => sqlBoolVal.ConvertTo(SqlBooleanType).ConvertTo<bool>()).
                    Compile();

            // Explicit cast from SqlBoolean to bool? for non-Null values; otherwise null
            sqlBooleanToNullableBoolean = Expressions.Lambda<object, bool?>(
                "sqlBooleanValue", sqlBoolVal =>
                                   sqlBoolVal.ConvertTo(SqlBooleanType).Property<bool>("IsNull")
                                       .IfTrueThen(Expressions.Null<bool?>())
                                       .Else(sqlBoolVal.ConvertTo(SqlBooleanType).ConvertTo<bool>().ConvertTo<bool?>())).Compile();

            // SqlBytes has instance byte[] property 'Value'
            sqlBytesToByteArray =
                Expressions.Lambda<object, byte[]>(
                    "sqlBytesValue", sqlBytesVal => sqlBytesVal.ConvertTo(SqlBytesType).Property<byte[]>("Value")).Compile();

            // SqlChars -> SqlString, SqlString has instance string property 'Value'
            sqlCharsToString =
                Expressions.Lambda<object, string>(
                    "sqlCharsValue", sqlCharsVal => sqlCharsVal.ConvertTo(SqlCharsType).Call("ToSqlString").Property<string>("Value")).
                    Compile();

            // Explicit cast from SqlString to string
            sqlStringToString =
                Expressions.Lambda<object, string>(
                    "sqlStringValue", sqlStringVal => sqlStringVal.ConvertTo(SqlStringType).Property<string>("Value")).Compile();

            // Explicit cast from SqlDouble to double
            sqlDoubleToDouble =
                Expressions.Lambda<object, double>(
                    "sqlDoubleValue", sqlDoubleVal => sqlDoubleVal.ConvertTo(SqlDoubleType).ConvertTo<double>()).Compile();

            // Explicit cast from SqlDouble to double? for non-Null values; otherwise null
            sqlDoubleToNullableDouble = Expressions.Lambda<object, double?>(
                "sqlDoubleValue", sqlDoubleVal =>
                                  sqlDoubleVal.ConvertTo(SqlDoubleType).Property<bool>("IsNull")
                                      .IfTrueThen(Expressions.Null<double?>())
                                      .Else(sqlDoubleVal.ConvertTo(SqlDoubleType).ConvertTo<double>().ConvertTo<double?>())).Compile();

            // Explicit cast from SqlInt32 to int
            sqlInt32ToInt =
                Expressions.Lambda<object, int>("sqlInt32Value", sqlInt32Val => sqlInt32Val.ConvertTo(SqlInt32Type).ConvertTo<int>()).
                    Compile();

            // Explicit cast from SqlInt32 to int? for non-Null values; otherwise null
            sqlInt32ToNullableInt = Expressions.Lambda<object, int?>(
                "sqlInt32Value", sqlInt32Val =>
                                 sqlInt32Val.ConvertTo(SqlInt32Type).Property<bool>("IsNull")
                                     .IfTrueThen(Expressions.Null<int?>())
                                     .Else(sqlInt32Val.ConvertTo(SqlInt32Type).ConvertTo<int>().ConvertTo<int?>())).Compile();

            // SqlXml has instance string property 'Value'
            sqlXmlToString =
                Expressions.Lambda<object, string>("sqlXmlValue", sqlXmlVal => sqlXmlVal.ConvertTo(SqlXmlType).Property<string>("Value")).
                    Compile();

            isSqlGeographyNull =
                Expressions.Lambda<object, bool>(
                    "sqlGeographyValue", sqlGeographyValue => sqlGeographyValue.ConvertTo(SqlGeographyType).Property<bool>("IsNull")).
                    Compile();
            isSqlGeometryNull =
                Expressions.Lambda<object, bool>(
                    "sqlGeometryValue", sqlGeometryValue => sqlGeometryValue.ConvertTo(SqlGeometryType).Property<bool>("IsNull")).Compile();

            geographyAsTextZMAsSqlChars =
                Expressions.Lambda<object, object>(
                    "sqlGeographyValue", sqlGeographyValue => sqlGeographyValue.ConvertTo(SqlGeographyType).Call("AsTextZM")).Compile();
            geometryAsTextZMAsSqlChars =
                Expressions.Lambda<object, object>(
                    "sqlGeometryValue", sqlGeometryValue => sqlGeometryValue.ConvertTo(SqlGeometryType).Call("AsTextZM")).Compile();
        }

        #region 'Public' API

        #region Primitive Types and Type Conversions

        internal Type SqlBooleanType { get; private set; }
        internal Type SqlBytesType { get; private set; }
        internal Type SqlCharsType { get; private set; }
        internal Type SqlStringType { get; private set; }
        internal Type SqlDoubleType { get; private set; }
        internal Type SqlInt32Type { get; private set; }
        internal Type SqlXmlType { get; private set; }

        private readonly Func<object, bool> sqlBooleanToBoolean;

        internal bool SqlBooleanToBoolean(object sqlBooleanValue)
        {
            return sqlBooleanToBoolean(sqlBooleanValue);
        }

        private readonly Func<object, bool?> sqlBooleanToNullableBoolean;

        internal bool? SqlBooleanToNullableBoolean(object sqlBooleanValue)
        {
            if (sqlBooleanToBoolean == null)
            {
                return null;
            }

            return sqlBooleanToNullableBoolean(sqlBooleanValue);
        }

        private readonly Func<byte[], object> sqlBytesFromByteArray;

        internal object SqlBytesFromByteArray(byte[] binaryValue)
        {
            return sqlBytesFromByteArray(binaryValue);
        }

        private readonly Func<object, byte[]> sqlBytesToByteArray;

        internal byte[] SqlBytesToByteArray(object sqlBytesValue)
        {
            if (sqlBytesValue == null)
            {
                return null;
            }

            return sqlBytesToByteArray(sqlBytesValue);
        }

        private readonly Func<string, object> sqlStringFromString;

        internal object SqlStringFromString(string stringValue)
        {
            return sqlStringFromString(stringValue);
        }

        private readonly Func<string, object> sqlCharsFromString;

        internal object SqlCharsFromString(string stringValue)
        {
            return sqlCharsFromString(stringValue);
        }

        private readonly Func<object, string> sqlCharsToString;

        internal string SqlCharsToString(object sqlCharsValue)
        {
            if (sqlCharsValue == null)
            {
                return null;
            }
            return sqlCharsToString(sqlCharsValue);
        }

        private readonly Func<object, string> sqlStringToString;

        internal string SqlStringToString(object sqlStringValue)
        {
            if (sqlStringValue == null)
            {
                return null;
            }
            return sqlStringToString(sqlStringValue);
        }

        private readonly Func<object, double> sqlDoubleToDouble;

        internal double SqlDoubleToDouble(object sqlDoubleValue)
        {
            return sqlDoubleToDouble(sqlDoubleValue);
        }

        private readonly Func<object, double?> sqlDoubleToNullableDouble;

        internal double? SqlDoubleToNullableDouble(object sqlDoubleValue)
        {
            if (sqlDoubleValue == null)
            {
                return null;
            }
            return sqlDoubleToNullableDouble(sqlDoubleValue);
        }

        private readonly Func<object, int> sqlInt32ToInt;

        internal int SqlInt32ToInt(object sqlInt32Value)
        {
            return sqlInt32ToInt(sqlInt32Value);
        }

        private readonly Func<object, int?> sqlInt32ToNullableInt;

        internal int? SqlInt32ToNullableInt(object sqlInt32Value)
        {
            if (sqlInt32Value == null)
            {
                return null;
            }
            return sqlInt32ToNullableInt(sqlInt32Value);
        }

        private readonly Func<XmlReader, object> sqlXmlFromXmlReader;

        internal object SqlXmlFromString(string stringValue)
        {
            var xmlReader = XmlReaderFromString(stringValue);
            return sqlXmlFromXmlReader(xmlReader);
        }

        private readonly Func<object, string> sqlXmlToString;

        internal string SqlXmlToString(object sqlXmlValue)
        {
            if (sqlXmlValue == null)
            {
                return null;
            }

            return sqlXmlToString(sqlXmlValue);
        }

        private readonly Func<object, bool> isSqlGeographyNull;

        internal bool IsSqlGeographyNull(object sqlGeographyValue)
        {
            if (sqlGeographyValue == null)
            {
                return true;
            }

            return isSqlGeographyNull(sqlGeographyValue);
        }

        private readonly Func<object, bool> isSqlGeometryNull;

        internal bool IsSqlGeometryNull(object sqlGeometryValue)
        {
            if (sqlGeometryValue == null)
            {
                return true;
            }

            return isSqlGeometryNull(sqlGeometryValue);
        }

        private readonly Func<object, object> geographyAsTextZMAsSqlChars;

        internal string GeographyAsTextZM(DbGeography geographyValue)
        {
            if (geographyValue == null)
            {
                return null;
            }

            var sqlGeographyValue = ConvertToSqlTypesGeography(geographyValue);
            var chars = geographyAsTextZMAsSqlChars(sqlGeographyValue);
            return SqlCharsToString(chars);
        }

        private readonly Func<object, object> geometryAsTextZMAsSqlChars;

        internal string GeometryAsTextZM(DbGeometry geometryValue)
        {
            if (geometryValue == null)
            {
                return null;
            }

            var sqlGeometryValue = ConvertToSqlTypesGeometry(geometryValue);
            var chars = geometryAsTextZMAsSqlChars(sqlGeometryValue);
            return SqlCharsToString(chars);
        }

        #endregion

        #region Spatial Types and Spatial Factory Methods

        internal Type SqlGeographyType { get; private set; }
        internal Type SqlGeometryType { get; private set; }

        internal object ConvertToSqlTypesGeography(DbGeography geographyValue)
        {
            geographyValue.CheckNull("geographyValue");
            var result = GetSqlTypesSpatialValue(geographyValue.AsSpatialValue(), SqlGeographyType);
            return result;
        }

        internal object SqlTypesGeographyFromBinary(byte[] wellKnownBinary, int srid)
        {
            Debug.Assert(wellKnownBinary != null, "Validate WKB before calling SqlTypesGeographyFromBinary");
            return sqlGeographyFromWKBByteArray(wellKnownBinary, srid);
        }

        internal object SqlTypesGeographyFromText(string wellKnownText, int srid)
        {
            Debug.Assert(wellKnownText != null, "Validate WKT before calling SqlTypesGeographyFromText");
            return sqlGeographyFromWKTString(wellKnownText, srid);
        }

        internal object ConvertToSqlTypesGeometry(DbGeometry geometryValue)
        {
            geometryValue.CheckNull("geometryValue");
            var result = GetSqlTypesSpatialValue(geometryValue.AsSpatialValue(), SqlGeometryType);
            return result;
        }

        internal object SqlTypesGeometryFromBinary(byte[] wellKnownBinary, int srid)
        {
            Debug.Assert(wellKnownBinary != null, "Validate WKB before calling SqlTypesGeometryFromBinary");
            return sqlGeometryFromWKBByteArray(wellKnownBinary, srid);
        }

        internal object SqlTypesGeometryFromText(string wellKnownText, int srid)
        {
            Debug.Assert(wellKnownText != null, "Validate WKT before calling SqlTypesGeometryFromText");
            return sqlGeometryFromWKTString(wellKnownText, srid);
        }

        #endregion

        #endregion

        private object GetSqlTypesSpatialValue(IDbSpatialValue spatialValue, Type requiredProviderValueType)
        {
            Debug.Assert(spatialValue != null, "Ensure spatial value is non-null before calling GetSqlTypesSpatialValue");

            // If the specified value was created by this spatial services implementation, its underlying Microsoft.SqlServer.Types.SqlGeography value is available via the ProviderValue property.
            var providerValue = spatialValue.ProviderValue;
            if (providerValue != null
                && providerValue.GetType() == requiredProviderValueType)
            {
                return providerValue;
            }

            // Otherwise, attempt to retrieve a Well Known Binary, Well Known Text or GML (in descending order of preference) representation of the value that can be used to create an appropriate Microsoft.SqlServer.Types.SqlGeography/SqlGeometry value

            var srid = spatialValue.CoordinateSystemId;
            if (srid.HasValue)
            {
                // Well Known Binary (WKB)
                var binaryValue = spatialValue.WellKnownBinary;
                if (binaryValue != null)
                {
                    return (spatialValue.IsGeography
                                ? sqlGeographyFromWKBByteArray(binaryValue, srid.Value)
                                : sqlGeometryFromWKBByteArray(binaryValue, srid.Value));
                }

                // Well Known Text (WKT)
                var textValue = spatialValue.WellKnownText;
                if (textValue != null)
                {
                    return (spatialValue.IsGeography
                                ? sqlGeographyFromWKTString(textValue, srid.Value)
                                : sqlGeometryFromWKTString(textValue, srid.Value));
                }

                // Geography Markup Language (GML), as a string
                var gmlValue = spatialValue.GmlString;
                if (gmlValue != null)
                {
                    var xmlReader = XmlReaderFromString(gmlValue);
                    return (spatialValue.IsGeography
                                ? sqlGeographyFromGMLReader(xmlReader, srid.Value)
                                : sqlGeometryFromGMLReader(xmlReader, srid.Value));
                }
            }

            throw spatialValue.NotSqlCompatible();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static XmlReader XmlReaderFromString(string stringValue)
        {
            return XmlReader.Create(new StringReader(stringValue));
        }

        private readonly Func<string, int, object> sqlGeographyFromWKTString;
        private readonly Func<byte[], int, object> sqlGeographyFromWKBByteArray;
        private readonly Func<XmlReader, int, object> sqlGeographyFromGMLReader;

        private readonly Func<string, int, object> sqlGeometryFromWKTString;
        private readonly Func<byte[], int, object> sqlGeometryFromWKBByteArray;
        private readonly Func<XmlReader, int, object> sqlGeometryFromGMLReader;

        #region Expression Compilation Helpers

        private static Func<TArg, int, object> CreateStaticConstructorDelegate<TArg>(Type spatialType, string methodName)
        {
            Debug.Assert(spatialType != null, "Ensure spatialType is non-null before calling CreateStaticConstructorDelegate");
            var dataParam = Expression.Parameter(typeof(TArg));
            var sridParam = Expression.Parameter(typeof(int));
            var staticCtorMethod = spatialType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            Debug.Assert(staticCtorMethod != null, "Could not find method '" + methodName + "' on type '" + spatialType.FullName + "'");
            Debug.Assert(
                staticCtorMethod.GetParameters().Length == 2 && staticCtorMethod.GetParameters()[1].ParameterType == typeof(int),
                "Static constructor method on '" + spatialType.FullName + "' does not match static constructor pattern?");

            var sqlData = BuildConvertToSqlType(dataParam, staticCtorMethod.GetParameters()[0].ParameterType);

            var ex = Expression.Lambda<Func<TArg, int, object>>(
                Expression.Call(null, staticCtorMethod, sqlData, sridParam), dataParam, sridParam);
            var result = ex.Compile();
            return result;
        }

        private static Expression BuildConvertToSqlType(Expression toConvert, Type convertTo)
        {
            if (toConvert.Type
                == typeof(byte[]))
            {
                return BuildConvertToSqlBytes(toConvert, convertTo);
            }
            else if (toConvert.Type
                     == typeof(string))
            {
                if (convertTo.Name == "SqlString")
                {
                    return BuildConvertToSqlString(toConvert, convertTo);
                }
                else
                {
                    return BuildConvertToSqlChars(toConvert, convertTo);
                }
            }
            else
            {
                Debug.Assert(
                    toConvert.Type == typeof(XmlReader), "Argument to static constructor method was not byte[], string or XmlReader?");
                if (toConvert.Type
                    == typeof(XmlReader))
                {
                    return BuildConvertToSqlXml(toConvert, convertTo);
                }
            }

            return toConvert;
        }

        private static Expression BuildConvertToSqlBytes(Expression toConvert, Type sqlBytesType)
        {
            // dataParam:byte[] => new SqlBytes(dataParam)
            Debug.Assert(sqlBytesType.Name == "SqlBytes", "byte[] argument used with non-SqlBytes static constructor method?");
            var byteArrayCtor = sqlBytesType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public, null, new[] { toConvert.Type }, null);
            Debug.Assert(byteArrayCtor != null, "SqlXml(System.IO.Stream) constructor not found?");
            Expression result = Expression.New(byteArrayCtor, toConvert);
            return result;
        }

        private static Expression BuildConvertToSqlChars(Expression toConvert, Type sqlCharsType)
        {
            // dataParam:String => new SqlChars(new SqlString(dataParam))
            Debug.Assert(sqlCharsType.Name == "SqlChars", "String argument used with non-SqlChars static constructor method?");
            var sqlString = sqlCharsType.Assembly.GetType("System.Data.SqlTypes.SqlString", throwOnError: true);
            var sqlCharsFromSqlStringCtor = sqlCharsType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public, null, new[] { sqlString }, null);
            Debug.Assert(sqlCharsFromSqlStringCtor != null, "SqlXml(System.IO.Stream) constructor not found?");
            var sqlStringFromStringCtor = sqlString.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(string) }, null);
            Expression result = Expression.New(sqlCharsFromSqlStringCtor, Expression.New(sqlStringFromStringCtor, toConvert));
            return result;
        }

        private static Expression BuildConvertToSqlString(Expression toConvert, Type sqlStringType)
        {
            // dataParam:String => new SqlString(dataParam)
            Debug.Assert(sqlStringType.Name == "SqlString", "String argument used with non-SqlString static constructor method?");
            var sqlStringFromStringCtor = sqlStringType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(string) }, null);
            Debug.Assert(sqlStringFromStringCtor != null);
            Expression result = Expression.Convert(Expression.New(sqlStringFromStringCtor, toConvert), typeof(object));
            return result;
        }

        private static Expression BuildConvertToSqlXml(Expression toConvert, Type sqlXmlType)
        {
            // dataParam:Stream => new SqlXml(dataParam)
            Debug.Assert(sqlXmlType.Name == "SqlXml", "Stream argument used with non-SqlXml static constructor method?");
            var readerCtor = sqlXmlType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new[] { toConvert.Type }, null);
            Debug.Assert(readerCtor != null, "SqlXml(System.Xml.XmlReader) constructor not found?");
            Expression result = Expression.New(readerCtor, toConvert);
            return result;
        }

        #endregion
    }
}
