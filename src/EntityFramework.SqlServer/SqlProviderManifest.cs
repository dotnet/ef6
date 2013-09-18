// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.SqlServer.Resources;
    using System.Data.Entity.SqlServer.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    // <summary>
    // The Provider Manifest for SQL Server
    // </summary>
    internal class SqlProviderManifest : DbXmlEnabledProviderManifest
    {
        internal const string TokenSql8 = "2000";
        internal const string TokenSql9 = "2005";
        internal const string TokenSql10 = "2008";
        internal const string TokenSql11 = "2012";
        internal const string TokenAzure11 = "2012.Azure";

        // '~' is the same escape character that L2S uses
        internal const char LikeEscapeChar = '~';
        internal const string LikeEscapeCharToString = "~";

        #region Private Fields

        // Default to SQL Server 2005 (9.0)
        private readonly SqlVersion _version = SqlVersion.Sql9;

        // <summary>
        // Maximum size of SQL Server unicode
        // </summary>
        private const int varcharMaxSize = 8000;

        private const int nvarcharMaxSize = 4000;
        private const int binaryMaxSize = 8000;

        private ReadOnlyCollection<PrimitiveType> _primitiveTypes;
        private ReadOnlyCollection<EdmFunction> _functions;

        #endregion

        #region Constructors

        // <summary>
        // Initializes a new instance of the <see cref="SqlProviderManifest" /> class.
        // </summary>
        // <param name="manifestToken"> A token used to infer the capabilities of the store. </param>
        public SqlProviderManifest(string manifestToken)
            : base(GetProviderManifest())
        {
            // GetSqlVersion will throw ArgumentException if manifestToken is null, empty, or not recognized.
            _version = SqlVersionUtils.GetSqlVersion(manifestToken);
        }

        #endregion

        #region Properties

        internal SqlVersion SqlVersion
        {
            get { return _version; }
        }

        #endregion

        #region Private Methods

        private static XmlReader GetXmlResource(string resourceName)
        {
            return XmlReader.Create(typeof(SqlProviderManifest).Assembly().GetManifestResourceStream(resourceName), null, resourceName);
        }

        internal static XmlReader GetProviderManifest()
        {
            return GetXmlResource("System.Data.Resources.SqlClient.SqlProviderServices.ProviderManifest.xml");
        }

        internal static XmlReader GetStoreSchemaMapping(string mslName)
        {
            return GetXmlResource("System.Data.Resources.SqlClient.SqlProviderServices." + mslName + ".msl");
        }

        internal XmlReader GetStoreSchemaDescription(string ssdlName)
        {
            if (_version == SqlVersion.Sql8)
            {
                return GetXmlResource("System.Data.Resources.SqlClient.SqlProviderServices." + ssdlName + "_Sql8.ssdl");
            }

            return GetXmlResource("System.Data.Resources.SqlClient.SqlProviderServices." + ssdlName + ".ssdl");
        }

        #endregion

        #region Internal Methods

        // <summary>
        // Function to detect wildcard characters %, _, [ and ^ and escape them with a preceding ~
        // This escaping is used when StartsWith, EndsWith and Contains canonical and CLR functions
        // are translated to their equivalent LIKE expression
        // NOTE: This code has been copied from LinqToSql
        // </summary>
        // <param name="text"> Original input as specified by the user </param>
        // <param name="alwaysEscapeEscapeChar"> escape the escape character ~ regardless whether wildcard characters were encountered </param>
        // <param name="usedEscapeChar"> true if the escaping was performed, false if no escaping was required </param>
        // <returns> The escaped string that can be used as pattern in a LIKE expression </returns>
        internal static string EscapeLikeText(string text, bool alwaysEscapeEscapeChar, out bool usedEscapeChar)
        {
            DebugCheck.NotNull(text);

            usedEscapeChar = false;
            if (!(text.Contains("%") || text.Contains("_") || text.Contains("[")
                  || text.Contains("^") || alwaysEscapeEscapeChar && text.Contains(LikeEscapeCharToString)))
            {
                return text;
            }
            var sb = new StringBuilder(text.Length);
            foreach (var c in text)
            {
                if (c == '%'
                    || c == '_'
                    || c == '['
                    || c == '^'
                    || c == LikeEscapeChar)
                {
                    sb.Append(LikeEscapeChar);
                    usedEscapeChar = true;
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        #endregion

        #region Overrides

        // <summary>
        // Providers should override this to return information specific to their provider.
        // This method should never return null.
        // </summary>
        // <param name="informationType"> The name of the information to be retrieved. </param>
        // <returns> An XmlReader at the begining of the information requested. </returns>
        protected override XmlReader GetDbInformation(string informationType)
        {
            if (informationType == StoreSchemaDefinitionVersion3
                || informationType == StoreSchemaDefinition)
            {
                return GetStoreSchemaDescription(informationType);
            }

            if (informationType == StoreSchemaMappingVersion3
                || informationType == StoreSchemaMapping)
            {
                return GetStoreSchemaMapping(informationType);
            }

            // Use default Conceptual Schema Definition
            if (informationType == ConceptualSchemaDefinitionVersion3
                || informationType == ConceptualSchemaDefinition)
            {
                return null;
            }

            throw new ProviderIncompatibleException(Strings.ProviderReturnedNullForGetDbInformation(informationType));
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        public override ReadOnlyCollection<PrimitiveType> GetStoreTypes()
        {
            if (_primitiveTypes == null)
            {
                if (_version == SqlVersion.Sql10
                    ||
                    _version == SqlVersion.Sql11)
                {
                    _primitiveTypes = base.GetStoreTypes();
                }
                else
                {
                    var primitiveTypes = new List<PrimitiveType>(base.GetStoreTypes());
                    Debug.Assert(
                        (_version == SqlVersion.Sql8) || (_version == SqlVersion.Sql9),
                        "Found version other than SQL 8, 9, 10 or 11.");
                    //Remove the Katmai types for both SQL 8 and SQL 9
                    primitiveTypes.RemoveAll(
                        delegate(PrimitiveType primitiveType)
                        {
                            var name = primitiveType.Name.ToLowerInvariant();
                            return name.Equals("time", StringComparison.Ordinal) ||
                                   name.Equals("date", StringComparison.Ordinal) ||
                                   name.Equals("datetime2", StringComparison.Ordinal) ||
                                   name.Equals("datetimeoffset", StringComparison.Ordinal) ||
                                   name.Equals("geography", StringComparison.Ordinal) ||
                                   name.Equals("geometry", StringComparison.Ordinal);
                        }
                        );
                    //Remove the types that won't work in SQL 8
                    if (_version == SqlVersion.Sql8)
                    {
                        // SQLBUDT 550667 and 551271: Remove xml and 'max' types for SQL Server 2000
                        primitiveTypes.RemoveAll(
                            delegate(PrimitiveType primitiveType)
                            {
                                var name = primitiveType.Name.ToLowerInvariant();
                                return name.Equals("xml", StringComparison.Ordinal) || name.EndsWith("(max)", StringComparison.Ordinal);
                            }
                            );
                    }
                    _primitiveTypes = new ReadOnlyCollection<PrimitiveType>(primitiveTypes);
                }
            }

            return _primitiveTypes;
        }

        public override ReadOnlyCollection<EdmFunction> GetStoreFunctions()
        {
            if (_functions == null)
            {
                if (_version == SqlVersion.Sql10
                    ||
                    _version == SqlVersion.Sql11)
                {
                    _functions = base.GetStoreFunctions();
                }
                else
                {
                    //Remove the functions over katmai types from both SQL 9 and SQL 8.
                    var functions = base.GetStoreFunctions().Where(f => !IsKatmaiOrNewer(f));
                    if (_version == SqlVersion.Sql8)
                    {
                        // SQLBUDT 550998: Remove unsupported overloads from Provider Manifest on SQL 8.0
                        functions = functions.Where(f => !IsYukonOrNewer(f));
                    }
                    _functions = new ReadOnlyCollection<EdmFunction>(functions.ToList());
                }
            }

            return _functions;
        }

        private static bool IsKatmaiOrNewer(EdmFunction edmFunction)
        {
            // Spatial types are only supported from Katmai onward; any functions using them must therefore also be Katmai or newer.
            if ((edmFunction.ReturnParameter != null && edmFunction.ReturnParameter.TypeUsage.IsSpatialType())
                || edmFunction.Parameters.Any(p => p.TypeUsage.IsSpatialType()))
            {
                return true;
            }

            var funParams = edmFunction.Parameters;
            switch (edmFunction.Name.ToUpperInvariant())
            {
                case "COUNT":
                case "COUNT_BIG":
                case "MAX":
                case "MIN":
                    {
                        var name = ((CollectionType)funParams[0].TypeUsage.EdmType).TypeUsage.EdmType.Name;
                        return ((name.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase)) ||
                                (name.Equals("Time", StringComparison.OrdinalIgnoreCase)));
                    }
                case "DAY":
                case "MONTH":
                case "YEAR":
                case "DATALENGTH":
                case "CHECKSUM":
                    {
                        var name = funParams[0].TypeUsage.EdmType.Name;
                        return ((name.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase)) ||
                                (name.Equals("Time", StringComparison.OrdinalIgnoreCase)));
                    }
                case "DATEADD":
                case "DATEDIFF":
                    {
                        var param1Name = funParams[1].TypeUsage.EdmType.Name;
                        var param2Name = funParams[2].TypeUsage.EdmType.Name;
                        return ((param1Name.Equals("Time", StringComparison.OrdinalIgnoreCase)) ||
                                (param2Name.Equals("Time", StringComparison.OrdinalIgnoreCase)) ||
                                (param1Name.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase)) ||
                                (param2Name.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase)));
                    }
                case "DATENAME":
                case "DATEPART":
                    {
                        var name = funParams[1].TypeUsage.EdmType.Name;
                        return ((name.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase)) ||
                                (name.Equals("Time", StringComparison.OrdinalIgnoreCase)));
                    }
                case "SYSUTCDATETIME":
                case "SYSDATETIME":
                case "SYSDATETIMEOFFSET":
                    return true;
                default:
                    break;
            }

            return false;
        }

        private static bool IsYukonOrNewer(EdmFunction edmFunction)
        {
            var funParams = edmFunction.Parameters;
            if (funParams == null
                || funParams.Count == 0)
            {
                return false;
            }

            switch (edmFunction.Name.ToUpperInvariant())
            {
                case "COUNT":
                case "COUNT_BIG":
                    {
                        var name = ((CollectionType)funParams[0].TypeUsage.EdmType).TypeUsage.EdmType.Name;
                        return name.Equals("Guid", StringComparison.OrdinalIgnoreCase);
                    }

                case "CHARINDEX":
                    {
                        foreach (var funParam in funParams)
                        {
                            if (funParam.TypeUsage.EdmType.Name.Equals("Int64", StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                    break;

                default:
                    break;
            }

            return false;
        }

        // <summary>
        // This method takes a type and a set of facets and returns the best mapped equivalent type
        // in EDM.
        // </summary>
        // <param name="storeType"> A TypeUsage encapsulating a store type and a set of facets </param>
        // <returns> A TypeUsage encapsulating an EDM type and a set of facets </returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        public override TypeUsage GetEdmType(TypeUsage storeType)
        {
            Check.NotNull(storeType, "storeType");

            var storeTypeName = storeType.EdmType.Name.ToLowerInvariant();
            if (!base.StoreTypeNameToEdmPrimitiveType.ContainsKey(storeTypeName))
            {
                throw new ArgumentException(Strings.ProviderDoesNotSupportType(storeTypeName));
            }

            var edmPrimitiveType = base.StoreTypeNameToEdmPrimitiveType[storeTypeName];

            var maxLength = 0;
            var isUnicode = true;
            var isFixedLen = false;
            var isUnbounded = true;

            PrimitiveTypeKind newPrimitiveTypeKind;

            switch (storeTypeName)
            {
                // for some types we just go with simple type usage with no facets
                case "tinyint":
                case "smallint":
                case "bigint":
                case "bit":
                case "uniqueidentifier":
                case "int":
                case "geography":
                case "geometry":
                    return TypeUsage.CreateDefaultTypeUsage(edmPrimitiveType);

                case "varchar":
                    newPrimitiveTypeKind = PrimitiveTypeKind.String;
                    isUnbounded = !storeType.TryGetMaxLength(out maxLength);
                    isUnicode = false;
                    isFixedLen = false;
                    break;

                case "char":
                    newPrimitiveTypeKind = PrimitiveTypeKind.String;
                    isUnbounded = !storeType.TryGetMaxLength(out maxLength);
                    isUnicode = false;
                    isFixedLen = true;
                    break;

                case "nvarchar":
                    newPrimitiveTypeKind = PrimitiveTypeKind.String;
                    isUnbounded = !storeType.TryGetMaxLength(out maxLength);
                    isUnicode = true;
                    isFixedLen = false;
                    break;

                case "nchar":
                    newPrimitiveTypeKind = PrimitiveTypeKind.String;
                    isUnbounded = !storeType.TryGetMaxLength(out maxLength);
                    isUnicode = true;
                    isFixedLen = true;
                    break;

                case "varchar(max)":
                case "text":
                    newPrimitiveTypeKind = PrimitiveTypeKind.String;
                    isUnbounded = true;
                    isUnicode = false;
                    isFixedLen = false;
                    break;

                case "nvarchar(max)":
                case "ntext":
                case "xml":
                    newPrimitiveTypeKind = PrimitiveTypeKind.String;
                    isUnbounded = true;
                    isUnicode = true;
                    isFixedLen = false;
                    break;

                case "binary":
                    newPrimitiveTypeKind = PrimitiveTypeKind.Binary;
                    isUnbounded = !storeType.TryGetMaxLength(out maxLength);
                    isFixedLen = true;
                    break;

                case "varbinary":
                    newPrimitiveTypeKind = PrimitiveTypeKind.Binary;
                    isUnbounded = !storeType.TryGetMaxLength(out maxLength);
                    isFixedLen = false;
                    break;

                case "varbinary(max)":
                case "image":
                    newPrimitiveTypeKind = PrimitiveTypeKind.Binary;
                    isUnbounded = true;
                    isFixedLen = false;
                    break;

                case "timestamp":
                case "rowversion":
                    return TypeUsage.CreateBinaryTypeUsage(edmPrimitiveType, true, 8);

                case "float":
                case "real":
                    return TypeUsage.CreateDefaultTypeUsage(edmPrimitiveType);

                case "decimal":
                case "numeric":
                    {
                        byte precision;
                        byte scale;
                        if (storeType.TryGetPrecision(out precision)
                            && storeType.TryGetScale(out scale))
                        {
                            return TypeUsage.CreateDecimalTypeUsage(edmPrimitiveType, precision, scale);
                        }
                        else
                        {
                            return TypeUsage.CreateDecimalTypeUsage(edmPrimitiveType);
                        }
                    }

                case "money":
                    return TypeUsage.CreateDecimalTypeUsage(edmPrimitiveType, 19, 4);

                case "smallmoney":
                    return TypeUsage.CreateDecimalTypeUsage(edmPrimitiveType, 10, 4);

                case "datetime":
                case "datetime2":
                case "smalldatetime":
                    return TypeUsage.CreateDateTimeTypeUsage(edmPrimitiveType, null);
                case "date":
                    return TypeUsage.CreateDefaultTypeUsage(edmPrimitiveType);
                case "time":
                    return TypeUsage.CreateTimeTypeUsage(edmPrimitiveType, null);
                case "datetimeoffset":
                    return TypeUsage.CreateDateTimeOffsetTypeUsage(edmPrimitiveType, null);

                default:
                    throw new NotSupportedException(Strings.ProviderDoesNotSupportType(storeTypeName));
            }

            Debug.Assert(
                newPrimitiveTypeKind == PrimitiveTypeKind.String || newPrimitiveTypeKind == PrimitiveTypeKind.Binary,
                "at this point only string and binary types should be present");

            switch (newPrimitiveTypeKind)
            {
                case PrimitiveTypeKind.String:
                    if (!isUnbounded)
                    {
                        return TypeUsage.CreateStringTypeUsage(edmPrimitiveType, isUnicode, isFixedLen, maxLength);
                    }
                    else
                    {
                        return TypeUsage.CreateStringTypeUsage(edmPrimitiveType, isUnicode, isFixedLen);
                    }
                case PrimitiveTypeKind.Binary:
                    if (!isUnbounded)
                    {
                        return TypeUsage.CreateBinaryTypeUsage(edmPrimitiveType, isFixedLen, maxLength);
                    }
                    else
                    {
                        return TypeUsage.CreateBinaryTypeUsage(edmPrimitiveType, isFixedLen);
                    }
                default:
                    throw new NotSupportedException(Strings.ProviderDoesNotSupportType(storeTypeName));
            }
        }

        // <summary>
        // This method takes a type and a set of facets and returns the best mapped equivalent type
        // in SQL Server, taking the store version into consideration.
        // </summary>
        // <param name="edmType"> A TypeUsage encapsulating an EDM type and a set of facets </param>
        // <returns> A TypeUsage encapsulating a store type and a set of facets </returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public override TypeUsage GetStoreType(TypeUsage edmType)
        {
            Check.NotNull(edmType, "edmType");
            Debug.Assert(edmType.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType);

            var primitiveType = edmType.EdmType as PrimitiveType;
            if (primitiveType == null)
            {
                throw new ArgumentException(Strings.ProviderDoesNotSupportType(edmType.EdmType.Name));
            }

            var facets = edmType.Facets;

            switch (primitiveType.PrimitiveTypeKind)
            {
                case PrimitiveTypeKind.Boolean:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["bit"]);

                case PrimitiveTypeKind.Byte:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["tinyint"]);

                case PrimitiveTypeKind.Int16:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["smallint"]);

                case PrimitiveTypeKind.Int32:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["int"]);

                case PrimitiveTypeKind.Int64:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["bigint"]);

                case PrimitiveTypeKind.Geography:
                case PrimitiveTypeKind.GeographyPoint:
                case PrimitiveTypeKind.GeographyLineString:
                case PrimitiveTypeKind.GeographyPolygon:
                case PrimitiveTypeKind.GeographyMultiPoint:
                case PrimitiveTypeKind.GeographyMultiLineString:
                case PrimitiveTypeKind.GeographyMultiPolygon:
                case PrimitiveTypeKind.GeographyCollection:
                    return GetStorePrimitiveTypeIfPostSql9("geography", edmType.EdmType.Name, primitiveType.PrimitiveTypeKind);

                case PrimitiveTypeKind.Geometry:
                case PrimitiveTypeKind.GeometryPoint:
                case PrimitiveTypeKind.GeometryLineString:
                case PrimitiveTypeKind.GeometryPolygon:
                case PrimitiveTypeKind.GeometryMultiPoint:
                case PrimitiveTypeKind.GeometryMultiLineString:
                case PrimitiveTypeKind.GeometryMultiPolygon:
                case PrimitiveTypeKind.GeometryCollection:
                    return GetStorePrimitiveTypeIfPostSql9("geometry", edmType.EdmType.Name, primitiveType.PrimitiveTypeKind);

                case PrimitiveTypeKind.Guid:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["uniqueidentifier"]);

                case PrimitiveTypeKind.Double:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["float"]);

                case PrimitiveTypeKind.Single:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["real"]);

                case PrimitiveTypeKind.Decimal: // decimal, numeric, smallmoney, money
                    {
                        byte precision;
                        if (!edmType.TryGetPrecision(out precision))
                        {
                            precision = 18;
                        }

                        byte scale;
                        if (!edmType.TryGetScale(out scale))
                        {
                            scale = 0;
                        }
                        var tu = TypeUsage.CreateDecimalTypeUsage(StoreTypeNameToStorePrimitiveType["decimal"], precision, scale);
                        return tu;
                    }

                case PrimitiveTypeKind.Binary: // binary, varbinary, varbinary(max), image, timestamp, rowversion
                    {
                        var isFixedLength = null != facets[FixedLengthFacetName].Value && (bool)facets[FixedLengthFacetName].Value;
                        var f = facets[MaxLengthFacetName];
                        var isMaxLength = f.IsUnbounded || null == f.Value || (int)f.Value > binaryMaxSize;
                        var maxLength = !isMaxLength ? (int)f.Value : Int32.MinValue;

                        TypeUsage tu;
                        if (isFixedLength)
                        {
                            tu = TypeUsage.CreateBinaryTypeUsage(
                                StoreTypeNameToStorePrimitiveType["binary"], true, (isMaxLength ? binaryMaxSize : maxLength));
                        }
                        else
                        {
                            if (isMaxLength)
                            {
                                if (_version != SqlVersion.Sql8)
                                {
                                    tu = TypeUsage.CreateBinaryTypeUsage(StoreTypeNameToStorePrimitiveType["varbinary(max)"], false);
                                    Debug.Assert(tu.Facets[MaxLengthFacetName].Description.IsConstant, "varbinary(max) is not constant!");
                                }
                                else
                                {
                                    tu = TypeUsage.CreateBinaryTypeUsage(
                                        StoreTypeNameToStorePrimitiveType["varbinary"], false, binaryMaxSize);
                                }
                            }
                            else
                            {
                                tu = TypeUsage.CreateBinaryTypeUsage(StoreTypeNameToStorePrimitiveType["varbinary"], false, maxLength);
                            }
                        }
                        return tu;
                    }

                case PrimitiveTypeKind.String:
                    // char, nchar, varchar, nvarchar, varchar(max), nvarchar(max), ntext, text, xml
                    {
                        var isUnicode = null == facets[UnicodeFacetName].Value || (bool)facets[UnicodeFacetName].Value;
                        var isFixedLength = null != facets[FixedLengthFacetName].Value && (bool)facets[FixedLengthFacetName].Value;
                        var f = facets[MaxLengthFacetName];
                        // maxlen is true if facet value is unbounded, the value is bigger than the limited string sizes *or* the facet
                        // value is null. this is needed since functions still have maxlength facet value as null
                        var isMaxLength = f.IsUnbounded || null == f.Value
                                          || (int)f.Value > (isUnicode ? nvarcharMaxSize : varcharMaxSize);
                        var maxLength = !isMaxLength ? (int)f.Value : Int32.MinValue;

                        TypeUsage tu;

                        if (isUnicode)
                        {
                            if (isFixedLength)
                            {
                                tu = TypeUsage.CreateStringTypeUsage(
                                    StoreTypeNameToStorePrimitiveType["nchar"], true, true, (isMaxLength ? nvarcharMaxSize : maxLength));
                            }
                            else
                            {
                                if (isMaxLength)
                                {
                                    // nvarchar(max) (SQL 9) or ntext (SQL 8)
                                    if (_version != SqlVersion.Sql8)
                                    {
                                        tu = TypeUsage.CreateStringTypeUsage(
                                            StoreTypeNameToStorePrimitiveType["nvarchar(max)"], true, false);
                                        Debug.Assert(tu.Facets[MaxLengthFacetName].Description.IsConstant, "NVarchar(max) is not constant!");
                                    }
                                    else
                                    {
                                        // if it is unknown, fallback to nvarchar[4000] instead of ntext since it has limited store semantics
                                        tu = TypeUsage.CreateStringTypeUsage(
                                            StoreTypeNameToStorePrimitiveType["nvarchar"], true, false, nvarcharMaxSize);
                                    }
                                }
                                else
                                {
                                    tu = TypeUsage.CreateStringTypeUsage(
                                        StoreTypeNameToStorePrimitiveType["nvarchar"], true, false, maxLength);
                                }
                            }
                        }
                        else // !isUnicode
                        {
                            if (isFixedLength)
                            {
                                tu = TypeUsage.CreateStringTypeUsage(
                                    StoreTypeNameToStorePrimitiveType["char"], false, true,
                                    (isMaxLength ? varcharMaxSize : maxLength));
                            }
                            else
                            {
                                if (isMaxLength)
                                {
                                    // nvarchar(max) (SQL 9) or ntext (SQL 8)
                                    if (_version != SqlVersion.Sql8)
                                    {
                                        tu = TypeUsage.CreateStringTypeUsage(
                                            StoreTypeNameToStorePrimitiveType["varchar(max)"], false, false);
                                        Debug.Assert(tu.Facets[MaxLengthFacetName].Description.IsConstant, "varchar(max) is not constant!");
                                    }
                                    else
                                    {
                                        // if it is unknown, fallback to varchar[8000] instead of text since it has limited store semantics
                                        tu = TypeUsage.CreateStringTypeUsage(
                                            StoreTypeNameToStorePrimitiveType["varchar"], false, false, varcharMaxSize);
                                    }
                                }
                                else
                                {
                                    tu = TypeUsage.CreateStringTypeUsage(
                                        StoreTypeNameToStorePrimitiveType["varchar"], false, false, maxLength);
                                }
                            }
                        }
                        return tu;
                    }

                case PrimitiveTypeKind.DateTime:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["datetime"]);
                case PrimitiveTypeKind.DateTimeOffset:
                    return GetStorePrimitiveTypeIfPostSql9("datetimeoffset", edmType.EdmType.Name, primitiveType.PrimitiveTypeKind);
                case PrimitiveTypeKind.Time:
                    return GetStorePrimitiveTypeIfPostSql9("time", edmType.EdmType.Name, primitiveType.PrimitiveTypeKind);

                default:
                    throw new NotSupportedException(Strings.NoStoreTypeForEdmType(edmType.EdmType.Name, primitiveType.PrimitiveTypeKind));
            }
        }

        private TypeUsage GetStorePrimitiveTypeIfPostSql9(
            string storeTypeName, string nameForException, PrimitiveTypeKind primitiveTypeKind)
        {
            if ((SqlVersion != SqlVersion.Sql8)
                && (SqlVersion != SqlVersion.Sql9))
            {
                return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType[storeTypeName]);
            }
            else
            {
                throw new NotSupportedException(Strings.NoStoreTypeForEdmType(nameForException, primitiveTypeKind));
            }
        }

        // <summary>
        // Returns true, SqlClient supports escaping strings to be used as arguments to like
        // The escape character is '~'
        // </summary>
        // <param name="escapeCharacter"> The character '~' </param>
        // <returns> True </returns>
        public override bool SupportsEscapingLikeArgument(out char escapeCharacter)
        {
            escapeCharacter = LikeEscapeChar;
            return true;
        }

        // <summary>
        // Escapes the wildcard characters and the escape character in the given argument.
        // </summary>
        // <returns> Equivalent to the argument, with the wildcard characters and the escape character escaped </returns>
        public override string EscapeLikeArgument(string argument)
        {
            Check.NotNull(argument, "argument");

            bool usedEscapeCharacter;
            return EscapeLikeText(argument, true, out usedEscapeCharacter);
        }

        // <summary>
        // Returns a boolean that specifies whether the corresponding provider can handle expression trees 
        // containing instances of DbInExpression.
        // The Sql provider handles instances of DbInExpression.
        // </summary>
        // <returns> <c>true</c>. </returns>
        public override bool SupportsInExpression()
        {
            return true;
        }

        #endregion
    }
}
