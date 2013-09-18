// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Xml;

    // <summary>
    // SqlTypesAssembly allows for late binding to the capabilities of a specific version of the Microsoft.SqlServer.Types assembly
    // </summary>
    internal class SqlTypesAssembly
    {
        // <summary>
        // For testing.
        // </summary>
        public SqlTypesAssembly()
        {
        }

        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public SqlTypesAssembly(Assembly sqlSpatialAssembly)
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
            var asTextMethod = SqlGeometryType.GetPublicInstanceMethod("STAsText");
            SqlCharsType = asTextMethod.ReturnType;
            SqlStringType = SqlCharsType.Assembly().GetType("System.Data.SqlTypes.SqlString", throwOnError: true);
            SqlBooleanType = SqlCharsType.Assembly().GetType("System.Data.SqlTypes.SqlBoolean", throwOnError: true);
            SqlBytesType = SqlCharsType.Assembly().GetType("System.Data.SqlTypes.SqlBytes", throwOnError: true);
            SqlDoubleType = SqlCharsType.Assembly().GetType("System.Data.SqlTypes.SqlDouble", throwOnError: true);
            SqlInt32Type = SqlCharsType.Assembly().GetType("System.Data.SqlTypes.SqlInt32", throwOnError: true);
            SqlXmlType = SqlCharsType.Assembly().GetType("System.Data.SqlTypes.SqlXml", throwOnError: true);

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
                                              .Else(sqlDoubleVal.ConvertTo(SqlDoubleType).ConvertTo<double>().ConvertTo<double?>()))
                                                   .Compile();

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

            _smiSqlGeographyParse = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("Parse", SqlStringType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member Parse");
                        return result;
                    }, isThreadSafe: true);

            _smiSqlGeographyStGeomFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STGeomFromText", SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STGeomFromText");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeographyStPointFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STPointFromText", SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STPointFromText");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeographyStLineFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STLineFromText", SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STLineFromText");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeographyStPolyFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STPolyFromText", SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STPolyFromText");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeographyStmPointFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STMPointFromText", SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STMPointFromText");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeographyStmLineFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STMLineFromText", SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STMLineFromText");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeographyStmPolyFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STMPolyFromText", SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STMPolyFromText");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeographyStGeomCollFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STGeomCollFromText", SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STGeomCollFromText");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeographyStGeomFromWkb = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STGeomFromWKB", SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STGeomFromWKB");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeographyStPointFromWkb = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STPointFromWKB", SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STPointFromWKB");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeographyStLineFromWkb = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STLineFromWKB", SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STLineFromWKB");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeographyStPolyFromWkb = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STPolyFromWKB", SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STPolyFromWKB");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeographyStmPointFromWkb = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STMPointFromWKB", SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STMPointFromWKB");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeographyStmLineFromWkb = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STMLineFromWKB", SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STMLineFromWKB");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeographyStmPolyFromWkb = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STMPolyFromWKB", SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STMPolyFromWKB");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeographyStGeomCollFromWkb = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("STGeomCollFromWKB", SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STGeomCollFromWKB");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeographyGeomFromGml = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyStaticMethod("GeomFromGml", SqlXmlType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member GeomFromGml");
                        return result;
                    }, isThreadSafe: true);
            _ipiSqlGeographyStSrid = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeographyProperty("STSrid");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeography member STSrid");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStGeometryType = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STGeometryType");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STGeometryType");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStDimension = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STDimension");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STDimension");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStAsBinary = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STAsBinary");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STAsBinary");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyAsGml = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("AsGml");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member AsGml");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStAsText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STAsText");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STAsText");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStIsEmpty = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STIsEmpty");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STIsEmpty");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStEquals = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STEquals", SqlGeographyType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STEquals");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStDisjoint = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STDisjoint", SqlGeographyType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STDisjoint");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStIntersects = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STIntersects", SqlGeographyType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STIntersects");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStBuffer = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STBuffer", typeof(double));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STBuffer");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStDistance = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STDistance", SqlGeographyType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STDistance");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStIntersection = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STIntersection", SqlGeographyType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STIntersection");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStUnion = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STUnion", SqlGeographyType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STUnion");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStDifference = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STDifference", SqlGeographyType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STDifference");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStSymDifference = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STSymDifference", SqlGeographyType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STSymDifference");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStNumGeometries = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STNumGeometries");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STNumGeometries");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStGeometryN = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STGeometryN", typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STGeometryN");
                        return result;
                    }, isThreadSafe: true);
            _ipiSqlGeographyLat = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeographyProperty("Lat");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeography member Lat");
                        return result;
                    }, isThreadSafe: true);
            _ipiSqlGeographyLong = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeographyProperty("Long");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeography member Long");
                        return result;
                    }, isThreadSafe: true);
            _ipiSqlGeographyZ = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeographyProperty("Z");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeography member Z");
                        return result;
                    }, isThreadSafe: true);
            _ipiSqlGeographyM = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeographyProperty("M");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeography member M");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStLength = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STLength");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STLength");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStStartPoint = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STStartPoint");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STStartPoint");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStEndPoint = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STEndPoint");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STEndPoint");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStIsClosed = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STIsClosed");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STIsClosed");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStNumPoints = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STNumPoints");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STNumPoints");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStPointN = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STPointN", typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STPointN");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeographyStArea = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeographyMethod("STArea");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeography member STArea");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeometryParse = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("Parse", SqlStringType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member Parse");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeometryStGeomFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STGeomFromText", SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STGeomFromText");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeometryStPointFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STPointFromText", SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STPointFromText");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeometryStLineFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STLineFromText", SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STLineFromText");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeometryStPolyFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STPolyFromText", SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STPolyFromText");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeometryStmPointFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STMPointFromText", SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STMPointFromText");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeometryStmLineFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STMLineFromText", SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STMLineFromText");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeometryStmPolyFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STMPolyFromText", SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STMPolyFromText");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeometryStGeomCollFromText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STGeomCollFromText", SqlCharsType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STGeomCollFromText");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeometryStGeomFromWkb = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STGeomFromWKB", SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STGeomFromWKB");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeometryStPointFromWkb = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STPointFromWKB", SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STPointFromWKB");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeometryStLineFromWkb = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STLineFromWKB", SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STLineFromWKB");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeometryStPolyFromWkb = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STPolyFromWKB", SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STPolyFromWKB");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeometryStmPointFromWkb = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STMPointFromWKB", SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STMPointFromWKB");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeometryStmLineFromWkb = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STMLineFromWKB", SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STMLineFromWKB");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeometryStmPolyFromWkb = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STMPolyFromWKB", SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STMPolyFromWKB");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeometryStGeomCollFromWkb = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("STGeomCollFromWKB", SqlBytesType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STGeomCollFromWKB");
                        return result;
                    }, isThreadSafe: true);
            _smiSqlGeometryGeomFromGml = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryStaticMethod("GeomFromGml", SqlXmlType, typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member GeomFromGml");
                        return result;
                    }, isThreadSafe: true);
            _ipiSqlGeometryStSrid = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeometryProperty("STSrid");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeometry member STSrid");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStGeometryType = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STGeometryType");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STGeometryType");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStDimension = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STDimension");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STDimension");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStEnvelope = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STEnvelope");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STEnvelope");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStAsBinary = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STAsBinary");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STAsBinary");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryAsGml = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("AsGml");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member AsGml");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStAsText = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STAsText");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STAsText");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStIsEmpty = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STIsEmpty");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STIsEmpty");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStIsSimple = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STIsSimple");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STIsSimple");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStBoundary = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STBoundary");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STBoundary");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStIsValid = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STIsValid");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STIsValid");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStEquals = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STEquals", SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STEquals");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStDisjoint = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STDisjoint", SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STDisjoint");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStIntersects = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STIntersects", SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STIntersects");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStTouches = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STTouches", SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STTouches");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStCrosses = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STCrosses", SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STCrosses");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStWithin = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STWithin", SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STWithin");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStContains = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STContains", SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STContains");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStOverlaps = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STOverlaps", SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STOverlaps");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStRelate = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STRelate", SqlGeometryType, typeof(string));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STRelate");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStBuffer = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STBuffer", typeof(double));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STBuffer");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStDistance = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STDistance", SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STDistance");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStConvexHull = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STConvexHull");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STConvexHull");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStIntersection = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STIntersection", SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STIntersection");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStUnion = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STUnion", SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STUnion");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStDifference = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STDifference", SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STDifference");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStSymDifference = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STSymDifference", SqlGeometryType);
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STSymDifference");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStNumGeometries = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STNumGeometries");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STNumGeometries");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStGeometryN = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STGeometryN", typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STGeometryN");
                        return result;
                    }, isThreadSafe: true);
            _ipiSqlGeometryStx = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeometryProperty("STX");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeometry member STX");
                        return result;
                    }, isThreadSafe: true);
            _ipiSqlGeometrySty = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeometryProperty("STY");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeometry member STY");
                        return result;
                    }, isThreadSafe: true);
            _ipiSqlGeometryZ = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeometryProperty("Z");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeometry member Z");
                        return result;
                    }, isThreadSafe: true);
            _ipiSqlGeometryM = new Lazy<PropertyInfo>(
                () =>
                    {
                        var result = FindSqlGeometryProperty("M");
                        Debug.Assert(result != null, "Could not retrieve PropertyInfo for SqlGeometry member M");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStLength = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STLength");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STLength");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStStartPoint = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STStartPoint");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STStartPoint");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStEndPoint = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STEndPoint");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STEndPoint");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStIsClosed = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STIsClosed");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STIsClosed");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStIsRing = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STIsRing");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STIsRing");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStNumPoints = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STNumPoints");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STNumPoints");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStPointN = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STPointN", typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STPointN");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStArea = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STArea");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STArea");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStCentroid = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STCentroid");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STCentroid");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStPointOnSurface = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STPointOnSurface");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STPointOnSurface");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStExteriorRing = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STExteriorRing");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STExteriorRing");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStNumInteriorRing = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STNumInteriorRing");
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STNumInteriorRing");
                        return result;
                    }, isThreadSafe: true);
            _imiSqlGeometryStInteriorRingN = new Lazy<MethodInfo>(
                () =>
                    {
                        var result = FindSqlGeometryMethod("STInteriorRingN", typeof(int));
                        Debug.Assert(result != null, "Could not retrieve MethodInfo for SqlGeometry member STInteriorRingN");
                        return result;
                    }, isThreadSafe: true);
        }

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

        internal Type SqlGeographyType { get; private set; }
        internal Type SqlGeometryType { get; private set; }

        internal object ConvertToSqlTypesGeography(DbGeography geographyValue)
        {
            DebugCheck.NotNull(geographyValue);
            return GetSqlTypesSpatialValue(geographyValue.AsSpatialValue(), SqlGeographyType);
        }

        internal object SqlTypesGeographyFromBinary(byte[] wellKnownBinary, int srid)
        {
            DebugCheck.NotNull(wellKnownBinary);
            return sqlGeographyFromWKBByteArray(wellKnownBinary, srid);
        }

        internal object SqlTypesGeographyFromText(string wellKnownText, int srid)
        {
            DebugCheck.NotNull(wellKnownText);
            return sqlGeographyFromWKTString(wellKnownText, srid);
        }

        internal object ConvertToSqlTypesGeometry(DbGeometry geometryValue)
        {
            DebugCheck.NotNull(geometryValue);
            return GetSqlTypesSpatialValue(geometryValue.AsSpatialValue(), SqlGeometryType);
        }

        internal object SqlTypesGeometryFromBinary(byte[] wellKnownBinary, int srid)
        {
            DebugCheck.NotNull(wellKnownBinary);
            return sqlGeometryFromWKBByteArray(wellKnownBinary, srid);
        }

        internal object SqlTypesGeometryFromText(string wellKnownText, int srid)
        {
            DebugCheck.NotNull(wellKnownText);
            return sqlGeometryFromWKTString(wellKnownText, srid);
        }

        private object GetSqlTypesSpatialValue(IDbSpatialValue spatialValue, Type requiredProviderValueType)
        {
            DebugCheck.NotNull(spatialValue);

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

        private static Func<TArg, int, object> CreateStaticConstructorDelegate<TArg>(Type spatialType, string methodName)
        {
            DebugCheck.NotNull(spatialType);
            var dataParam = Expression.Parameter(typeof(TArg));
            var sridParam = Expression.Parameter(typeof(int));
            var staticCtorMethod = spatialType.GetOnlyDeclaredMethod(methodName);
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
            var byteArrayCtor = sqlBytesType.GetDeclaredConstructor(toConvert.Type);
            Debug.Assert(byteArrayCtor != null, "SqlXml(System.IO.Stream) constructor not found?");
            Expression result = Expression.New(byteArrayCtor, toConvert);
            return result;
        }

        private static Expression BuildConvertToSqlChars(Expression toConvert, Type sqlCharsType)
        {
            // dataParam:String => new SqlChars(new SqlString(dataParam))
            Debug.Assert(sqlCharsType.Name == "SqlChars", "String argument used with non-SqlChars static constructor method?");
            var sqlString = sqlCharsType.Assembly().GetType("System.Data.SqlTypes.SqlString", throwOnError: true);
            var sqlCharsFromSqlStringCtor = sqlCharsType.GetDeclaredConstructor(sqlString);
            Debug.Assert(sqlCharsFromSqlStringCtor != null, "SqlXml(System.IO.Stream) constructor not found?");
            var sqlStringFromStringCtor = sqlString.GetDeclaredConstructor(typeof(string));
            Expression result = Expression.New(sqlCharsFromSqlStringCtor, Expression.New(sqlStringFromStringCtor, toConvert));
            return result;
        }

        private static Expression BuildConvertToSqlString(Expression toConvert, Type sqlStringType)
        {
            // dataParam:String => new SqlString(dataParam)
            Debug.Assert(sqlStringType.Name == "SqlString", "String argument used with non-SqlString static constructor method?");
            var sqlStringFromStringCtor = sqlStringType.GetDeclaredConstructor(typeof(string));
            Debug.Assert(sqlStringFromStringCtor != null);
            Expression result = Expression.Convert(Expression.New(sqlStringFromStringCtor, toConvert), typeof(object));
            return result;
        }

        private static Expression BuildConvertToSqlXml(Expression toConvert, Type sqlXmlType)
        {
            // dataParam:Stream => new SqlXml(dataParam)
            Debug.Assert(sqlXmlType.Name == "SqlXml", "Stream argument used with non-SqlXml static constructor method?");
            var readerCtor = sqlXmlType.GetDeclaredConstructor(toConvert.Type);
            Debug.Assert(readerCtor != null, "SqlXml(System.Xml.XmlReader) constructor not found?");
            Expression result = Expression.New(readerCtor, toConvert);
            return result;
        }

        public Lazy<MethodInfo> SmiSqlGeographyParse
        {
            get { return _smiSqlGeographyParse; }
        }

        public Lazy<MethodInfo> SmiSqlGeographyStGeomFromText
        {
            get { return _smiSqlGeographyStGeomFromText; }
        }

        public Lazy<MethodInfo> SmiSqlGeographyStPointFromText
        {
            get { return _smiSqlGeographyStPointFromText; }
        }

        public Lazy<MethodInfo> SmiSqlGeographyStLineFromText
        {
            get { return _smiSqlGeographyStLineFromText; }
        }

        public Lazy<MethodInfo> SmiSqlGeographyStPolyFromText
        {
            get { return _smiSqlGeographyStPolyFromText; }
        }

        public Lazy<MethodInfo> SmiSqlGeographyStmPointFromText
        {
            get { return _smiSqlGeographyStmPointFromText; }
        }

        public Lazy<MethodInfo> SmiSqlGeographyStmLineFromText
        {
            get { return _smiSqlGeographyStmLineFromText; }
        }

        public Lazy<MethodInfo> SmiSqlGeographyStmPolyFromText
        {
            get { return _smiSqlGeographyStmPolyFromText; }
        }

        public Lazy<MethodInfo> SmiSqlGeographyStGeomCollFromText
        {
            get { return _smiSqlGeographyStGeomCollFromText; }
        }

        public Lazy<MethodInfo> SmiSqlGeographyStGeomFromWkb
        {
            get { return _smiSqlGeographyStGeomFromWkb; }
        }

        public Lazy<MethodInfo> SmiSqlGeographyStPointFromWkb
        {
            get { return _smiSqlGeographyStPointFromWkb; }
        }

        public Lazy<MethodInfo> SmiSqlGeographyStLineFromWkb
        {
            get { return _smiSqlGeographyStLineFromWkb; }
        }

        public Lazy<MethodInfo> SmiSqlGeographyStPolyFromWkb
        {
            get { return _smiSqlGeographyStPolyFromWkb; }
        }

        public Lazy<MethodInfo> SmiSqlGeographyStmPointFromWkb
        {
            get { return _smiSqlGeographyStmPointFromWkb; }
        }

        public Lazy<MethodInfo> SmiSqlGeographyStmLineFromWkb
        {
            get { return _smiSqlGeographyStmLineFromWkb; }
        }

        public Lazy<MethodInfo> SmiSqlGeographyStmPolyFromWkb
        {
            get { return _smiSqlGeographyStmPolyFromWkb; }
        }

        public Lazy<MethodInfo> SmiSqlGeographyStGeomCollFromWkb
        {
            get { return _smiSqlGeographyStGeomCollFromWkb; }
        }

        public Lazy<MethodInfo> SmiSqlGeographyGeomFromGml
        {
            get { return _smiSqlGeographyGeomFromGml; }
        }

        public Lazy<PropertyInfo> IpiSqlGeographyStSrid
        {
            get { return _ipiSqlGeographyStSrid; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStGeometryType
        {
            get { return _imiSqlGeographyStGeometryType; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStDimension
        {
            get { return _imiSqlGeographyStDimension; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStAsBinary
        {
            get { return _imiSqlGeographyStAsBinary; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyAsGml
        {
            get { return _imiSqlGeographyAsGml; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStAsText
        {
            get { return _imiSqlGeographyStAsText; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStIsEmpty
        {
            get { return _imiSqlGeographyStIsEmpty; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStEquals
        {
            get { return _imiSqlGeographyStEquals; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStDisjoint
        {
            get { return _imiSqlGeographyStDisjoint; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStIntersects
        {
            get { return _imiSqlGeographyStIntersects; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStBuffer
        {
            get { return _imiSqlGeographyStBuffer; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStDistance
        {
            get { return _imiSqlGeographyStDistance; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStIntersection
        {
            get { return _imiSqlGeographyStIntersection; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStUnion
        {
            get { return _imiSqlGeographyStUnion; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStDifference
        {
            get { return _imiSqlGeographyStDifference; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStSymDifference
        {
            get { return _imiSqlGeographyStSymDifference; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStNumGeometries
        {
            get { return _imiSqlGeographyStNumGeometries; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStGeometryN
        {
            get { return _imiSqlGeographyStGeometryN; }
        }

        public Lazy<PropertyInfo> IpiSqlGeographyLat
        {
            get { return _ipiSqlGeographyLat; }
        }

        public Lazy<PropertyInfo> IpiSqlGeographyLong
        {
            get { return _ipiSqlGeographyLong; }
        }

        public Lazy<PropertyInfo> IpiSqlGeographyZ
        {
            get { return _ipiSqlGeographyZ; }
        }

        public Lazy<PropertyInfo> IpiSqlGeographyM
        {
            get { return _ipiSqlGeographyM; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStLength
        {
            get { return _imiSqlGeographyStLength; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStStartPoint
        {
            get { return _imiSqlGeographyStStartPoint; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStEndPoint
        {
            get { return _imiSqlGeographyStEndPoint; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStIsClosed
        {
            get { return _imiSqlGeographyStIsClosed; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStNumPoints
        {
            get { return _imiSqlGeographyStNumPoints; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStPointN
        {
            get { return _imiSqlGeographyStPointN; }
        }

        public Lazy<MethodInfo> ImiSqlGeographyStArea
        {
            get { return _imiSqlGeographyStArea; }
        }

        public Lazy<MethodInfo> SmiSqlGeometryParse
        {
            get { return _smiSqlGeometryParse; }
        }

        public Lazy<MethodInfo> SmiSqlGeometryStGeomFromText
        {
            get { return _smiSqlGeometryStGeomFromText; }
        }

        public Lazy<MethodInfo> SmiSqlGeometryStPointFromText
        {
            get { return _smiSqlGeometryStPointFromText; }
        }

        public Lazy<MethodInfo> SmiSqlGeometryStLineFromText
        {
            get { return _smiSqlGeometryStLineFromText; }
        }

        public Lazy<MethodInfo> SmiSqlGeometryStPolyFromText
        {
            get { return _smiSqlGeometryStPolyFromText; }
        }

        public Lazy<MethodInfo> SmiSqlGeometryStmPointFromText
        {
            get { return _smiSqlGeometryStmPointFromText; }
        }

        public Lazy<MethodInfo> SmiSqlGeometryStmLineFromText
        {
            get { return _smiSqlGeometryStmLineFromText; }
        }

        public Lazy<MethodInfo> SmiSqlGeometryStmPolyFromText
        {
            get { return _smiSqlGeometryStmPolyFromText; }
        }

        public Lazy<MethodInfo> SmiSqlGeometryStGeomCollFromText
        {
            get { return _smiSqlGeometryStGeomCollFromText; }
        }

        public Lazy<MethodInfo> SmiSqlGeometryStGeomFromWkb
        {
            get { return _smiSqlGeometryStGeomFromWkb; }
        }

        public Lazy<MethodInfo> SmiSqlGeometryStPointFromWkb
        {
            get { return _smiSqlGeometryStPointFromWkb; }
        }

        public Lazy<MethodInfo> SmiSqlGeometryStLineFromWkb
        {
            get { return _smiSqlGeometryStLineFromWkb; }
        }

        public Lazy<MethodInfo> SmiSqlGeometryStPolyFromWkb
        {
            get { return _smiSqlGeometryStPolyFromWkb; }
        }

        public Lazy<MethodInfo> SmiSqlGeometryStmPointFromWkb
        {
            get { return _smiSqlGeometryStmPointFromWkb; }
        }

        public Lazy<MethodInfo> SmiSqlGeometryStmLineFromWkb
        {
            get { return _smiSqlGeometryStmLineFromWkb; }
        }

        public Lazy<MethodInfo> SmiSqlGeometryStmPolyFromWkb
        {
            get { return _smiSqlGeometryStmPolyFromWkb; }
        }

        public Lazy<MethodInfo> SmiSqlGeometryStGeomCollFromWkb
        {
            get { return _smiSqlGeometryStGeomCollFromWkb; }
        }

        public Lazy<MethodInfo> SmiSqlGeometryGeomFromGml
        {
            get { return _smiSqlGeometryGeomFromGml; }
        }

        public Lazy<PropertyInfo> IpiSqlGeometryStSrid
        {
            get { return _ipiSqlGeometryStSrid; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStGeometryType
        {
            get { return _imiSqlGeometryStGeometryType; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStDimension
        {
            get { return _imiSqlGeometryStDimension; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStEnvelope
        {
            get { return _imiSqlGeometryStEnvelope; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStAsBinary
        {
            get { return _imiSqlGeometryStAsBinary; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryAsGml
        {
            get { return _imiSqlGeometryAsGml; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStAsText
        {
            get { return _imiSqlGeometryStAsText; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStIsEmpty
        {
            get { return _imiSqlGeometryStIsEmpty; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStIsSimple
        {
            get { return _imiSqlGeometryStIsSimple; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStBoundary
        {
            get { return _imiSqlGeometryStBoundary; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStIsValid
        {
            get { return _imiSqlGeometryStIsValid; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStEquals
        {
            get { return _imiSqlGeometryStEquals; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStDisjoint
        {
            get { return _imiSqlGeometryStDisjoint; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStIntersects
        {
            get { return _imiSqlGeometryStIntersects; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStTouches
        {
            get { return _imiSqlGeometryStTouches; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStCrosses
        {
            get { return _imiSqlGeometryStCrosses; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStWithin
        {
            get { return _imiSqlGeometryStWithin; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStContains
        {
            get { return _imiSqlGeometryStContains; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStOverlaps
        {
            get { return _imiSqlGeometryStOverlaps; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStRelate
        {
            get { return _imiSqlGeometryStRelate; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStBuffer
        {
            get { return _imiSqlGeometryStBuffer; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStDistance
        {
            get { return _imiSqlGeometryStDistance; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStConvexHull
        {
            get { return _imiSqlGeometryStConvexHull; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStIntersection
        {
            get { return _imiSqlGeometryStIntersection; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStUnion
        {
            get { return _imiSqlGeometryStUnion; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStDifference
        {
            get { return _imiSqlGeometryStDifference; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStSymDifference
        {
            get { return _imiSqlGeometryStSymDifference; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStNumGeometries
        {
            get { return _imiSqlGeometryStNumGeometries; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStGeometryN
        {
            get { return _imiSqlGeometryStGeometryN; }
        }

        public Lazy<PropertyInfo> IpiSqlGeometryStx
        {
            get { return _ipiSqlGeometryStx; }
        }

        public Lazy<PropertyInfo> IpiSqlGeometrySty
        {
            get { return _ipiSqlGeometrySty; }
        }

        public Lazy<PropertyInfo> IpiSqlGeometryZ
        {
            get { return _ipiSqlGeometryZ; }
        }

        public Lazy<PropertyInfo> IpiSqlGeometryM
        {
            get { return _ipiSqlGeometryM; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStLength
        {
            get { return _imiSqlGeometryStLength; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStStartPoint
        {
            get { return _imiSqlGeometryStStartPoint; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStEndPoint
        {
            get { return _imiSqlGeometryStEndPoint; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStIsClosed
        {
            get { return _imiSqlGeometryStIsClosed; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStIsRing
        {
            get { return _imiSqlGeometryStIsRing; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStNumPoints
        {
            get { return _imiSqlGeometryStNumPoints; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStPointN
        {
            get { return _imiSqlGeometryStPointN; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStArea
        {
            get { return _imiSqlGeometryStArea; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStCentroid
        {
            get { return _imiSqlGeometryStCentroid; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStPointOnSurface
        {
            get { return _imiSqlGeometryStPointOnSurface; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStExteriorRing
        {
            get { return _imiSqlGeometryStExteriorRing; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStNumInteriorRing
        {
            get { return _imiSqlGeometryStNumInteriorRing; }
        }

        public Lazy<MethodInfo> ImiSqlGeometryStInteriorRingN
        {
            get { return _imiSqlGeometryStInteriorRingN; }
        }

        private readonly Lazy<MethodInfo> _smiSqlGeographyParse;
        private readonly Lazy<MethodInfo> _smiSqlGeographyStGeomFromText;
        private readonly Lazy<MethodInfo> _smiSqlGeographyStPointFromText;
        private readonly Lazy<MethodInfo> _smiSqlGeographyStLineFromText;
        private readonly Lazy<MethodInfo> _smiSqlGeographyStPolyFromText;
        private readonly Lazy<MethodInfo> _smiSqlGeographyStmPointFromText;
        private readonly Lazy<MethodInfo> _smiSqlGeographyStmLineFromText;
        private readonly Lazy<MethodInfo> _smiSqlGeographyStmPolyFromText;
        private readonly Lazy<MethodInfo> _smiSqlGeographyStGeomCollFromText;
        private readonly Lazy<MethodInfo> _smiSqlGeographyStGeomFromWkb;
        private readonly Lazy<MethodInfo> _smiSqlGeographyStPointFromWkb;
        private readonly Lazy<MethodInfo> _smiSqlGeographyStLineFromWkb;
        private readonly Lazy<MethodInfo> _smiSqlGeographyStPolyFromWkb;
        private readonly Lazy<MethodInfo> _smiSqlGeographyStmPointFromWkb;
        private readonly Lazy<MethodInfo> _smiSqlGeographyStmLineFromWkb;
        private readonly Lazy<MethodInfo> _smiSqlGeographyStmPolyFromWkb;
        private readonly Lazy<MethodInfo> _smiSqlGeographyStGeomCollFromWkb;
        private readonly Lazy<MethodInfo> _smiSqlGeographyGeomFromGml;
        private readonly Lazy<PropertyInfo> _ipiSqlGeographyStSrid;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStGeometryType;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStDimension;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStAsBinary;
        private readonly Lazy<MethodInfo> _imiSqlGeographyAsGml;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStAsText;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStIsEmpty;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStEquals;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStDisjoint;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStIntersects;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStBuffer;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStDistance;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStIntersection;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStUnion;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStDifference;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStSymDifference;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStNumGeometries;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStGeometryN;
        private readonly Lazy<PropertyInfo> _ipiSqlGeographyLat;
        private readonly Lazy<PropertyInfo> _ipiSqlGeographyLong;
        private readonly Lazy<PropertyInfo> _ipiSqlGeographyZ;
        private readonly Lazy<PropertyInfo> _ipiSqlGeographyM;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStLength;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStStartPoint;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStEndPoint;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStIsClosed;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStNumPoints;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStPointN;
        private readonly Lazy<MethodInfo> _imiSqlGeographyStArea;
        private readonly Lazy<MethodInfo> _smiSqlGeometryParse;
        private readonly Lazy<MethodInfo> _smiSqlGeometryStGeomFromText;
        private readonly Lazy<MethodInfo> _smiSqlGeometryStPointFromText;
        private readonly Lazy<MethodInfo> _smiSqlGeometryStLineFromText;
        private readonly Lazy<MethodInfo> _smiSqlGeometryStPolyFromText;
        private readonly Lazy<MethodInfo> _smiSqlGeometryStmPointFromText;
        private readonly Lazy<MethodInfo> _smiSqlGeometryStmLineFromText;
        private readonly Lazy<MethodInfo> _smiSqlGeometryStmPolyFromText;
        private readonly Lazy<MethodInfo> _smiSqlGeometryStGeomCollFromText;
        private readonly Lazy<MethodInfo> _smiSqlGeometryStGeomFromWkb;
        private readonly Lazy<MethodInfo> _smiSqlGeometryStPointFromWkb;
        private readonly Lazy<MethodInfo> _smiSqlGeometryStLineFromWkb;
        private readonly Lazy<MethodInfo> _smiSqlGeometryStPolyFromWkb;
        private readonly Lazy<MethodInfo> _smiSqlGeometryStmPointFromWkb;
        private readonly Lazy<MethodInfo> _smiSqlGeometryStmLineFromWkb;
        private readonly Lazy<MethodInfo> _smiSqlGeometryStmPolyFromWkb;
        private readonly Lazy<MethodInfo> _smiSqlGeometryStGeomCollFromWkb;
        private readonly Lazy<MethodInfo> _smiSqlGeometryGeomFromGml;
        private readonly Lazy<PropertyInfo> _ipiSqlGeometryStSrid;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStGeometryType;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStDimension;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStEnvelope;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStAsBinary;
        private readonly Lazy<MethodInfo> _imiSqlGeometryAsGml;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStAsText;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStIsEmpty;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStIsSimple;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStBoundary;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStIsValid;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStEquals;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStDisjoint;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStIntersects;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStTouches;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStCrosses;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStWithin;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStContains;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStOverlaps;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStRelate;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStBuffer;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStDistance;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStConvexHull;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStIntersection;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStUnion;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStDifference;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStSymDifference;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStNumGeometries;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStGeometryN;
        private readonly Lazy<PropertyInfo> _ipiSqlGeometryStx;
        private readonly Lazy<PropertyInfo> _ipiSqlGeometrySty;
        private readonly Lazy<PropertyInfo> _ipiSqlGeometryZ;
        private readonly Lazy<PropertyInfo> _ipiSqlGeometryM;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStLength;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStStartPoint;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStEndPoint;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStIsClosed;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStIsRing;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStNumPoints;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStPointN;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStArea;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStCentroid;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStPointOnSurface;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStExteriorRing;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStNumInteriorRing;
        private readonly Lazy<MethodInfo> _imiSqlGeometryStInteriorRingN;

        private MethodInfo FindSqlGeographyMethod(string methodName, params Type[] argTypes)
        {
            return SqlGeographyType.GetDeclaredMethod(methodName, argTypes);
        }

        private MethodInfo FindSqlGeographyStaticMethod(string methodName, params Type[] argTypes)
        {
            return SqlGeographyType.GetDeclaredMethod(methodName, argTypes);
        }

        private PropertyInfo FindSqlGeographyProperty(string propertyName)
        {
            return SqlGeographyType.GetRuntimeProperty(propertyName);
        }

        private MethodInfo FindSqlGeometryStaticMethod(string methodName, params Type[] argTypes)
        {
            return SqlGeometryType.GetDeclaredMethod(methodName, argTypes);
        }

        private MethodInfo FindSqlGeometryMethod(string methodName, params Type[] argTypes)
        {
            return SqlGeometryType.GetDeclaredMethod(methodName, argTypes);
        }

        private PropertyInfo FindSqlGeometryProperty(string propertyName)
        {
            return SqlGeometryType.GetRuntimeProperty(propertyName);
        }
    }
}
