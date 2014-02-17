// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact
{
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.SqlServerCompact.Resources;
    using System.Data.Entity.SqlServerCompact.SqlGen;
    using System.Data.Entity.SqlServerCompact.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    // <summary>
    // The Provider Manifest for SQL Server CE
    // </summary>
    internal class SqlCeProviderManifest : DbXmlEnabledProviderManifest
    {
        #region Private and Internal Fields

        // <summary>
        // Singleton object; RDP supports all features as that of LDP.
        // So, this shouldn't be an issue anyways.
        // </summary>
        internal static readonly SqlCeProviderManifest Instance = new SqlCeProviderManifest(true);

#if SQLSERVERCOMPACT35
        internal const string ProviderInvariantName = "System.Data.SqlServerCe.3.5";
#else
        internal const string ProviderInvariantName = "System.Data.SqlServerCe.4.0";
#endif

        internal const string Token40 = "4.0";

        internal bool _isLocalProvider = true;

        // <summary>
        // maximum size of SSC unicode
        // </summary>
        private const int nvarcharMaxSize = 4000;

        private const int binaryMaxSize = 8000;

        private const string providerManifestFile =
            "System.Data.Resources.SqlServerCe.Entity.SqlCeProviderServices.ProviderManifest.xml";

        private const string storeSchemaMappingFile =
            "System.Data.Resources.SqlServerCe.Entity.SqlCeProviderServices.StoreSchemaMapping.msl";

#if SQLSERVERCOMPACT35
        private const string storeSchemaDescriptionFile =
            "System.Data.Resources.SqlServerCe.Entity.Legacy.SqlCeProviderServices.StoreSchemaDefinition.ssdl";
#else
        private const string storeSchemaDescriptionFile =
            "System.Data.Resources.SqlServerCe.Entity.SqlCeProviderServices.StoreSchemaDefinition.ssdl";
#endif
        // This SSDL is being used only at design time. And because SqlServerCe 3.5 is not supported in 
        // either VS2013 nor VS2012 we don't need a SqlServerCe 3.5 version of this file.
        private const string storeSchemaDescriptionFileForRDP =
            "Microsoft.SqlServerCe.Client.Resources.Entity.SqlCeProviderServices.StoreSchemaDefinition.ssdl";
        // '~' is the same escape character that L2S uses

        internal const char LikeEscapeChar = '~';
        internal const string LikeEscapeCharToString = "~";
        #endregion

        #region Constructors

        // <summary>
        // Constructor
        // </summary>
        public SqlCeProviderManifest(bool isLocalProvider)
            : base(GetProviderManifest())
        {
            // SSC's provider for Entity framework can connect to only one version of 
            // the database today. So there is no ambiguity. 
            // Add logic here if we change this behavior in future.
            _isLocalProvider = isLocalProvider;
        }

        #endregion

        private static XmlReader GetProviderManifest()
        {
            // DEVNOTE/CAUTION: This method could be called either from Remote or Local Provider.
            // Ensure that the code works well with the both provider types.
            //
            // NOTE: Remote Provider is loaded at runtime, if available.
            // This is done to remove hard dependency on Remote Provider and
            // it might not be present in all scenarios. All Remote Provider
            // type checks need to be done using RemoteProviderHelper class.
            //
            return GetXmlResource(providerManifestFile);
        }

        // <summary>
        // Providers should override this to return information specific to their provider.
        // This method should never return null.
        // </summary>
        // <param name="informationType"> The name of the information to be retrieved. </param>
        // <returns> An XmlReader at the begining of the information requested. </returns>
        protected override XmlReader GetDbInformation(string informationType)
        {
            if (informationType == ProviderManifest.StoreSchemaDefinition)
            {
                return GetStoreSchemaDescription();
            }

            if (informationType == ProviderManifest.StoreSchemaMapping)
            {
                return GetStoreSchemaMapping();
            }

            // Use default Conceptual Schema Definition
            if (informationType == ProviderManifest.ConceptualSchemaDefinition)
            {
                return null;
            }

            throw ADP1.ProviderIncompatible(EntityRes.GetString(EntityRes.ProviderReturnedNullForGetDbInformation, informationType));
        }

        // <summary>
        // This method takes a type and a set of facets and returns the best mapped equivalent type
        // in EDM.
        // </summary>
        // <param name="storeType"> A TypeUsage encapsulating a store type and a set of facets </param>
        // <returns> A TypeUsage encapsulating an EDM type and a set of facets </returns>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        public override TypeUsage GetEdmType(TypeUsage storeType)
        {
            Check.NotNull(storeType, "storeType");

            var storeTypeName = storeType.EdmType.Name.ToLowerInvariant();
            if (!base.StoreTypeNameToEdmPrimitiveType.ContainsKey(storeTypeName))
            {
                throw ADP1.Argument(EntityRes.GetString(EntityRes.ProviderDoesNotSupportType, storeTypeName));
            }

            var edmPrimitiveType = base.StoreTypeNameToEdmPrimitiveType[storeTypeName];

            var maxLength = 0;
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
                    return TypeUsage.CreateDefaultTypeUsage(edmPrimitiveType);

                case "nvarchar":
                    newPrimitiveTypeKind = PrimitiveTypeKind.String;
                    isUnbounded = !TypeHelpers.TryGetMaxLength(storeType, out maxLength);
                    isFixedLen = false;
                    break;

                case "nchar":
                    newPrimitiveTypeKind = PrimitiveTypeKind.String;
                    isUnbounded = !TypeHelpers.TryGetMaxLength(storeType, out maxLength);
                    isFixedLen = true;
                    break;

                case "ntext":
                    newPrimitiveTypeKind = PrimitiveTypeKind.String;
                    isUnbounded = true;
                    isFixedLen = false;
                    break;

                case "binary":
                    newPrimitiveTypeKind = PrimitiveTypeKind.Binary;
                    isUnbounded = !TypeHelpers.TryGetMaxLength(storeType, out maxLength);
                    isFixedLen = true;
                    break;

                case "varbinary":
                    newPrimitiveTypeKind = PrimitiveTypeKind.Binary;
                    isUnbounded = !TypeHelpers.TryGetMaxLength(storeType, out maxLength);
                    isFixedLen = false;
                    break;

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
                        if (TypeHelpers.TryGetPrecision(storeType, out precision)
                            && TypeHelpers.TryGetScale(storeType, out scale))
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

                case "datetime":
                    return TypeUsage.CreateDateTimeTypeUsage(edmPrimitiveType, null);

                default:
                    throw ADP1.NotSupported(EntityRes.GetString(EntityRes.ProviderDoesNotSupportType, storeTypeName));
            }

            Debug.Assert(
                newPrimitiveTypeKind == PrimitiveTypeKind.String || newPrimitiveTypeKind == PrimitiveTypeKind.Binary,
                "at this point only string and binary types should be present");

            switch (newPrimitiveTypeKind)
            {
                case PrimitiveTypeKind.String:
                    if (!isUnbounded)
                    {
                        return TypeUsage.CreateStringTypeUsage(edmPrimitiveType, /*isUnicode*/ true, isFixedLen, maxLength);
                    }
                    else
                    {
                        return TypeUsage.CreateStringTypeUsage(edmPrimitiveType, /*isUnicode*/ true, isFixedLen);
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
                    throw ADP1.NotSupported(EntityRes.GetString(EntityRes.ProviderDoesNotSupportType, storeTypeName));
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
                throw ADP1.Argument(EntityRes.GetString(EntityRes.ProviderDoesNotSupportType, edmType.EdmType));
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

                case PrimitiveTypeKind.Guid:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["uniqueidentifier"]);

                case PrimitiveTypeKind.Double:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["float"]);

                case PrimitiveTypeKind.Single:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["real"]);

                case PrimitiveTypeKind.Decimal: // decimal, numeric, money
                    {
                        byte precision;
                        if (!TypeHelpers.TryGetPrecision(edmType, out precision))
                        {
                            precision = 18;
                        }

                        byte scale;
                        if (!TypeHelpers.TryGetScale(edmType, out scale))
                        {
                            scale = 0;
                        }
                        var tu = TypeUsage.CreateDecimalTypeUsage(StoreTypeNameToStorePrimitiveType["decimal"], precision, scale);
                        return tu;
                    }

                case PrimitiveTypeKind.Binary: // binary, varbinary, image, timestamp, rowversion
                    {
                        var isFixedLength = null != facets[ProviderManifest.FixedLengthFacetName].Value
                                            && (bool)facets[ProviderManifest.FixedLengthFacetName].Value;
                        var f = facets[ProviderManifest.MaxLengthFacetName];
                        var isMaxLength = Helper.IsUnboundedFacetValue(f) || null == f.Value || (int)f.Value > binaryMaxSize;
                        var maxLength = !isMaxLength ? (int)f.Value : Int32.MinValue;

                        TypeUsage tu;
                        if (isFixedLength)
                        {
                            tu = TypeUsage.CreateBinaryTypeUsage(
                                StoreTypeNameToStorePrimitiveType["binary"], true, (isMaxLength ? binaryMaxSize : maxLength));
                        }
                        else
                        {
                            if (null == f.Value)
                            {
                                tu = TypeUsage.CreateBinaryTypeUsage(StoreTypeNameToStorePrimitiveType["varbinary"], false, binaryMaxSize);
                            }
                            else if (Helper.IsUnboundedFacetValue(f)
                                     || edmType.EdmType.Name == "image")
                            {
                                tu = TypeUsage.CreateBinaryTypeUsage(StoreTypeNameToStorePrimitiveType["image"], false);
                            }
                            else if ((int)f.Value > binaryMaxSize)
                            {
                                throw ADP1.ColumnGreaterThanMaxLengthNotSupported(edmType.EdmType.Name, binaryMaxSize);
                            }
                            else
                            {
                                tu = TypeUsage.CreateBinaryTypeUsage(StoreTypeNameToStorePrimitiveType["varbinary"], false, maxLength);
                            }
                        }
                        return tu;
                    }

                case PrimitiveTypeKind.String:
                    //char, nchar, varchar, nvarchar, ntext, text, xml
                    {
                        var isFixedLength = null != facets[ProviderManifest.FixedLengthFacetName].Value
                                            && (bool)facets[ProviderManifest.FixedLengthFacetName].Value;
                        var f = facets[ProviderManifest.MaxLengthFacetName];
                        // maxlen is true if facet value is unbounded, the value is bigger than the limited string sizes *or* the facet
                        // value is null. this is needed since functions still have maxlength facet value as null
                        var isMaxLength = Helper.IsUnboundedFacetValue(f) || null == f.Value || (int)f.Value > (nvarcharMaxSize);
                        var maxLength = !isMaxLength ? (int)f.Value : Int32.MinValue;

                        TypeUsage tu;

                        if (isFixedLength)
                        {
                            tu = TypeUsage.CreateStringTypeUsage(
                                StoreTypeNameToStorePrimitiveType["nchar"], true, true, (isMaxLength ? nvarcharMaxSize : maxLength));
                        }
                        else
                        {
                            if (null == f.Value)
                            {
                                // if it is unknown, fallback to nvarchar[4000] instead of ntext since it has limited store semantics
                                tu = TypeUsage.CreateStringTypeUsage(
                                    StoreTypeNameToStorePrimitiveType["nvarchar"], true, false, nvarcharMaxSize);
                            }
                            else if (Helper.IsUnboundedFacetValue(f)
                                     || edmType.EdmType.Name == "ntext")
                            {
                                tu = TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["ntext"], true, false);
                            }
                            else if ((int)f.Value > nvarcharMaxSize)
                            {
                                throw ADP1.ColumnGreaterThanMaxLengthNotSupported(edmType.EdmType.Name, nvarcharMaxSize);
                            }
                            else
                            {
                                tu = TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["nvarchar"], true, false, maxLength);
                            }
                        }
                        return tu;
                    }

                case PrimitiveTypeKind.DateTime:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["datetime"]);

                default:
                    throw ADP1.NotSupported(
                        EntityRes.GetString(
                            EntityRes.NoStoreTypeForEdmType, TypeHelpers.GetIdentity(edmType), primitiveType.PrimitiveTypeKind));
            }
        }

        private static XmlReader GetStoreSchemaMapping()
        {
            return GetXmlResource(storeSchemaMappingFile);
        }

        private XmlReader GetStoreSchemaDescription()
        {
            if (_isLocalProvider)
            {
                return GetXmlResource(storeSchemaDescriptionFile);
            }
            else
            {
                return GetXmlResource(storeSchemaDescriptionFileForRDP);
            }
        }

        private static XmlReader GetXmlResource(string resourceName)
        {
            return XmlReader.Create(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName));
        }

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

        // <summary>
        // Returns true, SqlCeClient supports escaping strings to be used as arguments to like
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
        // The SqlCe provider handles instances of DbInExpression.
        // </summary>
        // <returns> <c>true</c>. </returns>
        public override bool SupportsInExpression()
        {
            return true;
        }
    }
}
