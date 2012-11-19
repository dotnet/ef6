// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    ///     Represents a set of static Type helpers operating on TypeMetadata
    /// </summary>
    internal static class TypeHelpers
    {
        /// <summary>
        ///     Asserts types are in Model space
        /// </summary>
        /// <param name="typeUsage"> </param>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "CSpace")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "PrimitiveType")]
        [Conditional("DEBUG")]
        internal static void AssertEdmType(TypeUsage typeUsage)
        {
            var type = typeUsage.EdmType;
            if (TypeSemantics.IsCollectionType(typeUsage))
            {
                AssertEdmType(GetElementTypeUsage(typeUsage));
            }
            else if (TypeSemantics.IsStructuralType(typeUsage)
                     && !Helper.IsComplexType(typeUsage.EdmType)
                     && !Helper.IsEntityType(typeUsage.EdmType))
            {
                foreach (EdmMember m in GetDeclaredStructuralMembers(typeUsage))
                {
                    AssertEdmType(m.TypeUsage);
                }
            }
            else if (TypeSemantics.IsPrimitiveType(typeUsage))
            {
                var pType = type as PrimitiveType;
                if (null != pType)
                {
                    if (pType.DataSpace
                        != DataSpace.CSpace)
                    {
                        throw new NotSupportedException(
                            String.Format(CultureInfo.InvariantCulture, "PrimitiveType must be CSpace '{0}'", typeUsage));
                    }
                }
            }
        }

        /// <summary>
        ///     Asserts querycommandtrees are in model space type terms
        /// </summary>
        /// <param name="commandTree"> </param>
        [Conditional("DEBUG")]
        internal static void AssertEdmType(DbCommandTree commandTree)
        {
            var queryCommandTree = commandTree as DbQueryCommandTree;
            if (null != queryCommandTree)
            {
                AssertEdmType(queryCommandTree.Query.ResultType);
            }
        }

        //
        // Type Semantics
        //

        /// <summary>
        ///     Determines whether a given typeUsage is valid as OrderBy sort key
        /// </summary>
        /// <param name="typeUsage"> </param>
        /// <returns> </returns>
        internal static bool IsValidSortOpKeyType(TypeUsage typeUsage)
        {
            if (TypeSemantics.IsRowType(typeUsage))
            {
                var rowType = (RowType)typeUsage.EdmType;
                foreach (var property in rowType.Properties)
                {
                    if (!IsValidSortOpKeyType(property.TypeUsage))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return TypeSemantics.IsOrderComparable(typeUsage);
            }
        }

        /// <summary>
        ///     Determines whether a given typeusage is valid as GroupBy key
        /// </summary>
        /// <param name="typeUsage"> </param>
        /// <returns> </returns>
        internal static bool IsValidGroupKeyType(TypeUsage typeUsage)
        {
            return IsSetComparableOpType(typeUsage);
        }

        /// <summary>
        ///     Determine wheter a given typeusage is valid for Distinct operator
        /// </summary>
        /// <param name="typeUsage"> </param>
        /// <returns> </returns>
        internal static bool IsValidDistinctOpType(TypeUsage typeUsage)
        {
            return IsSetComparableOpType(typeUsage);
        }

        /// <summary>
        ///     Determine wheter a given typeusage is valid for set comparison operator such as UNION, INTERSECT and EXCEPT
        /// </summary>
        /// <param name="typeUsage"> </param>
        /// <returns> </returns>
        internal static bool IsSetComparableOpType(TypeUsage typeUsage)
        {
            if (Helper.IsEntityType(typeUsage.EdmType)
                ||
                Helper.IsPrimitiveType(typeUsage.EdmType)
                ||
                Helper.IsEnumType(typeUsage.EdmType)
                ||
                Helper.IsRefType(typeUsage.EdmType))
            {
                return true;
            }
            else if (TypeSemantics.IsRowType(typeUsage))
            {
                var rowType = (RowType)typeUsage.EdmType;
                foreach (var property in rowType.Properties)
                {
                    if (!IsSetComparableOpType(property.TypeUsage))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Returns true if typeUsage type is valid for IS [NOT] NULL (expr) operator
        /// </summary>
        /// <param name="typeUsage"> </param>
        /// <returns> </returns>
        internal static bool IsValidIsNullOpType(TypeUsage typeUsage)
        {
            return TypeSemantics.IsReferenceType(typeUsage) ||
                   TypeSemantics.IsEntityType(typeUsage) ||
                   TypeSemantics.IsScalarType(typeUsage);
        }

        internal static bool IsValidInOpType(TypeUsage typeUsage)
        {
            return TypeSemantics.IsReferenceType(typeUsage) ||
                   TypeSemantics.IsEntityType(typeUsage) ||
                   TypeSemantics.IsScalarType(typeUsage);
        }

        internal static TypeUsage GetCommonTypeUsage(TypeUsage typeUsage1, TypeUsage typeUsage2)
        {
            return TypeSemantics.GetCommonType(typeUsage1, typeUsage2);
        }

        internal static TypeUsage GetCommonTypeUsage(IEnumerable<TypeUsage> types)
        {
            TypeUsage commonType = null;
            foreach (var testType in types)
            {
                if (null == testType)
                {
                    return null;
                }

                if (null == commonType)
                {
                    commonType = testType;
                }
                else
                {
                    commonType = TypeSemantics.GetCommonType(commonType, testType);
                    if (null == commonType)
                    {
                        break;
                    }
                }
            }
            return commonType;
        }

        //
        // Type property extractors
        //

        internal static bool TryGetClosestPromotableType(TypeUsage fromType, out TypeUsage promotableType)
        {
            promotableType = null;
            if (Helper.IsPrimitiveType(fromType.EdmType))
            {
                var fromPrimitiveType = (PrimitiveType)fromType.EdmType;
                IList<PrimitiveType> promotableTypes = EdmProviderManifest.Instance.GetPromotionTypes(fromPrimitiveType);
                var index = promotableTypes.IndexOf(fromPrimitiveType);
                if (-1 != index
                    && index + 1 < promotableTypes.Count)
                {
                    promotableType = TypeUsage.Create(promotableTypes[index + 1]);
                }
            }
            return (null != promotableType);
        }

        //
        // Facet Helpers
        //

        internal static bool TryGetBooleanFacetValue(TypeUsage type, string facetName, out bool boolValue)
        {
            boolValue = false;
            Facet boolFacet;
            if (type.Facets.TryGetValue(facetName, false, out boolFacet)
                && boolFacet.Value != null)
            {
                boolValue = (bool)boolFacet.Value;
                return true;
            }

            return false;
        }

        internal static bool TryGetByteFacetValue(TypeUsage type, string facetName, out byte byteValue)
        {
            byteValue = 0;
            Facet byteFacet;
            if (type.Facets.TryGetValue(facetName, false, out byteFacet)
                && byteFacet.Value != null
                && !Helper.IsUnboundedFacetValue(byteFacet))
            {
                byteValue = (byte)byteFacet.Value;
                return true;
            }

            return false;
        }

        internal static bool TryGetIntFacetValue(TypeUsage type, string facetName, out int intValue)
        {
            intValue = 0;
            Facet intFacet;
            if (type.Facets.TryGetValue(facetName, false, out intFacet)
                && intFacet.Value != null
                && !Helper.IsUnboundedFacetValue(intFacet)
                && !Helper.IsVariableFacetValue(intFacet))
            {
                intValue = (int)intFacet.Value;
                return true;
            }

            return false;
        }

        internal static bool TryGetIsFixedLength(TypeUsage type, out bool isFixedLength)
        {
            if (!TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.String)
                &&
                !TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Binary))
            {
                isFixedLength = false;
                return false;
            }

            // Binary and String MaxLength facets share the same name
            return TryGetBooleanFacetValue(type, DbProviderManifest.FixedLengthFacetName, out isFixedLength);
        }

        internal static bool TryGetIsUnicode(TypeUsage type, out bool isUnicode)
        {
            if (!TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.String))
            {
                isUnicode = false;
                return false;
            }

            return TryGetBooleanFacetValue(type, DbProviderManifest.UnicodeFacetName, out isUnicode);
        }

        internal static bool IsFacetValueConstant(TypeUsage type, string facetName)
        {
            // Binary and String FixedLength facets share the same name
            return Helper.GetFacet(((PrimitiveType)type.EdmType).FacetDescriptions, facetName).IsConstant;
        }

        internal static bool TryGetMaxLength(TypeUsage type, out int maxLength)
        {
            if (!TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.String)
                &&
                !TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Binary))
            {
                maxLength = 0;
                return false;
            }

            // Binary and String FixedLength facets share the same name
            return TryGetIntFacetValue(type, DbProviderManifest.MaxLengthFacetName, out maxLength);
        }

        internal static bool TryGetPrecision(TypeUsage type, out byte precision)
        {
            if (!TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Decimal))
            {
                precision = 0;
                return false;
            }

            return TryGetByteFacetValue(type, DbProviderManifest.PrecisionFacetName, out precision);
        }

        internal static bool TryGetScale(TypeUsage type, out byte scale)
        {
            if (!TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Decimal))
            {
                scale = 0;
                return false;
            }

            return TryGetByteFacetValue(type, DbProviderManifest.ScaleFacetName, out scale);
        }

        internal static bool TryGetPrimitiveTypeKind(TypeUsage type, out PrimitiveTypeKind typeKind)
        {
            if (type != null
                && type.EdmType != null
                && type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
            {
                typeKind = ((PrimitiveType)type.EdmType).PrimitiveTypeKind;
                return true;
            }

            typeKind = default(PrimitiveTypeKind);
            return false;
        }

        //
        // Type Constructors
        //

        internal static CollectionType CreateCollectionType(TypeUsage elementType)
        {
            return new CollectionType(elementType);
        }

        internal static TypeUsage CreateCollectionTypeUsage(TypeUsage elementType)
        {
            return TypeUsage.Create(new CollectionType(elementType));
        }

        internal static RowType CreateRowType(IEnumerable<KeyValuePair<string, TypeUsage>> columns)
        {
            return CreateRowType(columns, null);
        }

        internal static RowType CreateRowType(IEnumerable<KeyValuePair<string, TypeUsage>> columns, InitializerMetadata initializerMetadata)
        {
            var rowElements = new List<EdmProperty>();
            foreach (var kvp in columns)
            {
                rowElements.Add(new EdmProperty(kvp.Key, kvp.Value));
            }
            return new RowType(rowElements, initializerMetadata);
        }

        internal static TypeUsage CreateRowTypeUsage(IEnumerable<KeyValuePair<string, TypeUsage>> columns)
        {
            return TypeUsage.Create(CreateRowType(columns));
        }

        internal static RefType CreateReferenceType(EntityTypeBase entityType)
        {
            return new RefType((EntityType)entityType);
        }

        internal static TypeUsage CreateReferenceTypeUsage(EntityType entityType)
        {
            return TypeUsage.Create(CreateReferenceType(entityType));
        }

        /// <summary>
        ///     Creates metadata for a new row type with column names and types based on the key members of the specified Entity type
        /// </summary>
        /// <param name="entityType"> The Entity type that provides the Key members on which the column names and types of the new row type will be based </param>
        /// <returns> A new RowType info with column names and types corresponding to the Key members of the specified Entity type </returns>
        internal static RowType CreateKeyRowType(EntityTypeBase entityType)
        {
            IEnumerable<EdmMember> entityKeys = entityType.KeyMembers;
            if (null == entityKeys)
            {
                throw new ArgumentException(Strings.Cqt_Metadata_EntityTypeNullKeyMembersInvalid, "entityType");
            }

            var resultCols = new List<KeyValuePair<string, TypeUsage>>();
            //int idx = 0;
            foreach (EdmProperty keyProperty in entityKeys)
            {
                //this.CheckMember(keyProperty, "property", CommandTreeUtils.FormatIndex("entityType.KeyMembers", idx++));
                resultCols.Add(new KeyValuePair<string, TypeUsage>(keyProperty.Name, Helper.GetModelTypeUsage(keyProperty)));
            }

            if (resultCols.Count < 1)
            {
                throw new ArgumentException(Strings.Cqt_Metadata_EntityTypeEmptyKeyMembersInvalid, "entityType");
            }

            return CreateRowType(resultCols);
        }

        /// <summary>
        ///     Gets primitive type usage for <paramref name="scalarType" />.
        /// </summary>
        /// <param name="scalarType"> Primitive or enum type usage. </param>
        /// <returns>
        ///     Primitive type usage for <paramref name="scalarType" /> .
        /// </returns>
        /// <remarks>
        ///     For enum types a new type usage based on the underlying type will be created. For primitive types
        ///     the value passed to the function will be returned.
        /// </remarks>
        internal static TypeUsage GetPrimitiveTypeUsageForScalar(TypeUsage scalarType)
        {
            Debug.Assert(scalarType != null, "scalarType != null");
            Debug.Assert(TypeSemantics.IsScalarType(scalarType), "Primitive or enum type expected.");

            return TypeSemantics.IsEnumerationType(scalarType)
                       ? CreateEnumUnderlyingTypeUsage(scalarType)
                       : scalarType;
        }

        /// <summary>
        ///     Factory method for creating a type usage for underlying type of enum type usage.
        /// </summary>
        /// <param name="enumTypeUsage"> Enum type usage used to create an underlying type usage of. </param>
        /// <returns> Type usage for the underlying enum type. </returns>
        internal static TypeUsage CreateEnumUnderlyingTypeUsage(TypeUsage enumTypeUsage)
        {
            Debug.Assert(enumTypeUsage != null, "enumTypeUsage != null");
            Debug.Assert(TypeSemantics.IsEnumerationType(enumTypeUsage), "enumTypeUsage is not an enumerated type");

            return TypeUsage.Create(Helper.GetUnderlyingEdmTypeForEnumType(enumTypeUsage.EdmType), enumTypeUsage.Facets);
        }

        /// <summary>
        ///     Factory method for creating a type usage for underlying union type of spatial type usage.
        /// </summary>
        /// <param name="spatialTypeUsage"> Spatial type usage used to create a union type usage of. </param>
        /// <returns> Type usage for the spatial union type of the correct topology. </returns>
        internal static TypeUsage CreateSpatialUnionTypeUsage(TypeUsage spatialTypeUsage)
        {
            Debug.Assert(spatialTypeUsage != null, "spatialTypeUsage != null");
            Debug.Assert(TypeSemantics.IsStrongSpatialType(spatialTypeUsage), "spatialTypeUsage is not a strong spatial type");
            return TypeUsage.Create(Helper.GetSpatialNormalizedPrimitiveType(spatialTypeUsage.EdmType), spatialTypeUsage.Facets);
        }

        //
        // Type extractors
        //

        /// <summary>
        ///     Retrieves Properties and/or RelationshipEnds declared by the specified type or any base type.
        /// </summary>
        /// <param name="type"> </param>
        /// <returns> </returns>
        internal static IBaseList<EdmMember> GetAllStructuralMembers(TypeUsage type)
        {
            return GetAllStructuralMembers(type.EdmType);
        }

        internal static IBaseList<EdmMember> GetAllStructuralMembers(EdmType edmType)
        {
            Debug.Assert(edmType != null);
            switch (edmType.BuiltInTypeKind)
            {
                case BuiltInTypeKind.AssociationType:
                    return (IBaseList<EdmMember>)((AssociationType)edmType).AssociationEndMembers;
                case BuiltInTypeKind.ComplexType:
                    return (IBaseList<EdmMember>)((ComplexType)edmType).Properties;
                case BuiltInTypeKind.EntityType:
                    return (IBaseList<EdmMember>)((EntityType)edmType).Properties;
                case BuiltInTypeKind.RowType:
                    return (IBaseList<EdmMember>)((RowType)edmType).Properties;
                default:
                    return EmptyArrayEdmProperty;
            }
        }

        /// <summary>
        ///     Retrieves Properties and/or RelationshipEnds declared by (and ONLY by) the specified type.
        /// </summary>
        /// <param name="type"> </param>
        /// <returns> </returns>
        internal static IEnumerable GetDeclaredStructuralMembers(TypeUsage type)
        {
            return GetDeclaredStructuralMembers(type.EdmType);
        }

        /// <summary>
        ///     Retrieves Properties and/or RelationshipEnds declared by (and ONLY by) the specified type.
        /// </summary>
        /// <param name="edmType"> </param>
        /// <returns> </returns>
        internal static IEnumerable GetDeclaredStructuralMembers(EdmType edmType)
        {
            switch (edmType.BuiltInTypeKind)
            {
                case BuiltInTypeKind.AssociationType:
                    return ((AssociationType)edmType).GetDeclaredOnlyMembers<AssociationEndMember>();
                case BuiltInTypeKind.ComplexType:
                    return ((ComplexType)edmType).GetDeclaredOnlyMembers<EdmProperty>();
                case BuiltInTypeKind.EntityType:
                    return ((EntityType)edmType).GetDeclaredOnlyMembers<EdmProperty>();
                case BuiltInTypeKind.RowType:
                    return ((RowType)edmType).GetDeclaredOnlyMembers<EdmProperty>();
                default:
                    return EmptyArrayEdmProperty;
            }
        }

        internal static readonly ReadOnlyMetadataCollection<EdmMember> EmptyArrayEdmMember =
            new ReadOnlyMetadataCollection<EdmMember>(new MetadataCollection<EdmMember>().SetReadOnly());

        internal static readonly FilteredReadOnlyMetadataCollection<EdmProperty, EdmMember> EmptyArrayEdmProperty =
            new FilteredReadOnlyMetadataCollection<EdmProperty, EdmMember>(EmptyArrayEdmMember, null);

        internal static ReadOnlyMetadataCollection<EdmProperty> GetProperties(TypeUsage typeUsage)
        {
            return GetProperties(typeUsage.EdmType);
        }

        internal static ReadOnlyMetadataCollection<EdmProperty> GetProperties(EdmType edmType)
        {
            switch (edmType.BuiltInTypeKind)
            {
                case BuiltInTypeKind.ComplexType:
                    return ((ComplexType)edmType).Properties;
                case BuiltInTypeKind.EntityType:
                    return ((EntityType)edmType).Properties;
                case BuiltInTypeKind.RowType:
                    return ((RowType)edmType).Properties;
                default:
                    return EmptyArrayEdmProperty;
            }
        }

        internal static TypeUsage GetElementTypeUsage(TypeUsage type)
        {
            if (TypeSemantics.IsCollectionType(type))
            {
                return ((CollectionType)type.EdmType).TypeUsage;
            }
            else if (TypeSemantics.IsReferenceType(type))
            {
                return TypeUsage.Create(((RefType)type.EdmType).ElementType);
            }

            return null;
        }

        /// <summary>
        ///     Returns row type if supplied function is a tvf returning Collection(RowType), otherwise null.
        /// </summary>
        internal static RowType GetTvfReturnType(EdmFunction tvf)
        {
            if (tvf.ReturnParameter != null
                && TypeSemantics.IsCollectionType(tvf.ReturnParameter.TypeUsage))
            {
                var expectedElementTypeUsage = ((CollectionType)tvf.ReturnParameter.TypeUsage.EdmType).TypeUsage;
                if (TypeSemantics.IsRowType(expectedElementTypeUsage))
                {
                    return (RowType)expectedElementTypeUsage.EdmType;
                }
            }
            return null;
        }

        //
        // Element type
        //
        internal static bool TryGetCollectionElementType(TypeUsage type, out TypeUsage elementType)
        {
            CollectionType collectionType;
            if (TryGetEdmType(type, out collectionType))
            {
                elementType = collectionType.TypeUsage;
                return (elementType != null);
            }

            elementType = null;
            return false;
        }

        /// <summary>
        ///     If the type refered to by the TypeUsage is a RefType, extracts the EntityType and returns true,
        ///     otherwise returns false.
        /// </summary>
        /// <param name="type"> TypeUsage that may or may not refer to a RefType </param>
        /// <param name="referencedEntityType"> Non-null if the TypeUsage refers to a RefType, null otherwise </param>
        /// <returns> True if the TypeUsage refers to a RefType, false otherwise </returns>
        internal static bool TryGetRefEntityType(TypeUsage type, out EntityType referencedEntityType)
        {
            RefType refType;
            if (TryGetEdmType(type, out refType)
                &&
                Helper.IsEntityType(refType.ElementType))
            {
                referencedEntityType = (EntityType)refType.ElementType;
                return true;
            }

            referencedEntityType = null;
            return false;
        }

        internal static TEdmType GetEdmType<TEdmType>(TypeUsage typeUsage)
            where TEdmType : EdmType
        {
            return (TEdmType)typeUsage.EdmType;
        }

        internal static bool TryGetEdmType<TEdmType>(TypeUsage typeUsage, out TEdmType type)
            where TEdmType : EdmType
        {
            type = typeUsage.EdmType as TEdmType;
            return (type != null);
        }

        //
        // Misc
        //

        internal static TypeUsage GetReadOnlyType(TypeUsage type)
        {
            if (!(type.IsReadOnly))
            {
                type.SetReadOnly();
            }
            return type;
        }

        //
        // Type Description
        //

        internal static string GetFullName(string qualifier, string name)
        {
            return string.IsNullOrEmpty(qualifier)
                       ? string.Format(CultureInfo.InvariantCulture, "{0}", name)
                       : string.Format(CultureInfo.InvariantCulture, "{0}.{1}", qualifier, name);
        }

        /// <summary>
        ///     Converts the given CLR type into a DbType
        /// </summary>
        /// <param name="clrType"> The CLR type to convert </param>
        /// <returns> </returns>
        internal static DbType ConvertClrTypeToDbType(Type clrType)
        {
            switch (Type.GetTypeCode(clrType))
            {
                case TypeCode.Empty:
                    throw new ArgumentException(Strings.ADP_InvalidDataType(TypeCode.Empty.ToString()));

                case TypeCode.Object:
                    if (clrType == typeof(Byte[]))
                    {
                        return DbType.Binary;
                    }
                    if (clrType == typeof(Char[]))
                    {
                        // Always treat char and char[] as string
                        return DbType.String;
                    }
                    else if (clrType == typeof(Guid))
                    {
                        return DbType.Guid;
                    }
                    else if (clrType == typeof(TimeSpan))
                    {
                        return DbType.Time;
                    }
                    else if (clrType == typeof(DateTimeOffset))
                    {
                        return DbType.DateTimeOffset;
                    }

                    return DbType.Object;

                case TypeCode.DBNull:
                    return DbType.Object;
                case TypeCode.Boolean:
                    return DbType.Boolean;
                case TypeCode.SByte:
                    return DbType.SByte;
                case TypeCode.Byte:
                    return DbType.Byte;
                case TypeCode.Char:
                    // Always treat char and char[] as string
                    return DbType.String;
                case TypeCode.Int16:
                    return DbType.Int16;
                case TypeCode.UInt16:
                    return DbType.UInt16;
                case TypeCode.Int32:
                    return DbType.Int32;
                case TypeCode.UInt32:
                    return DbType.UInt32;
                case TypeCode.Int64:
                    return DbType.Int64;
                case TypeCode.UInt64:
                    return DbType.UInt64;
                case TypeCode.Single:
                    return DbType.Single;
                case TypeCode.Double:
                    return DbType.Double;
                case TypeCode.Decimal:
                    return DbType.Decimal;
                case TypeCode.DateTime:
                    return DbType.DateTime;
                case TypeCode.String:
                    return DbType.String;
                default:
                    throw new ArgumentException(
                        Strings.ADP_UnknownDataTypeCode(
                            ((int)Type.GetTypeCode(clrType)).ToString(CultureInfo.InvariantCulture), clrType.FullName));
            }
        }

        internal static bool IsIntegerConstant(TypeUsage valueType, object value, long expectedValue)
        {
            if (!TypeSemantics.IsIntegerNumericType(valueType))
            {
                return false;
            }

            if (null == value)
            {
                return false;
            }

            var intType = (PrimitiveType)valueType.EdmType;
            switch (intType.PrimitiveTypeKind)
            {
                case PrimitiveTypeKind.Byte:
                    return (expectedValue == (byte)value);

                case PrimitiveTypeKind.Int16:
                    return (expectedValue == (short)value);

                case PrimitiveTypeKind.Int32:
                    return (expectedValue == (int)value);

                case PrimitiveTypeKind.Int64:
                    return (expectedValue == (long)value);

                case PrimitiveTypeKind.SByte:
                    return (expectedValue == (sbyte)value);

                default:
                    {
                        Debug.Assert(false, "Integer primitive type was not one of Byte, Int16, Int32, Int64, SByte?");
                        return false;
                    }
            }
        }

        /// <summary>
        ///     returns a Typeusage
        /// </summary>
        /// <param name="primitiveTypeKind"> </param>
        /// <returns> </returns>
        internal static TypeUsage GetLiteralTypeUsage(PrimitiveTypeKind primitiveTypeKind)
        {
            // all clr strings by default are unicode
            return GetLiteralTypeUsage(primitiveTypeKind, true /* unicode */);
        }

        internal static TypeUsage GetLiteralTypeUsage(PrimitiveTypeKind primitiveTypeKind, bool isUnicode)
        {
            TypeUsage typeusage;
            var primitiveType = EdmProviderManifest.Instance.GetPrimitiveType(primitiveTypeKind);
            switch (primitiveTypeKind)
            {
                case PrimitiveTypeKind.String:
                    typeusage = TypeUsage.Create(
                        primitiveType,
                        new FacetValues
                            {
                                Unicode = isUnicode,
                                MaxLength = TypeUsage.DefaultMaxLengthFacetValue,
                                FixedLength = false,
                                Nullable = false
                            });
                    break;

                default:
                    typeusage = TypeUsage.Create(
                        primitiveType,
                        new FacetValues
                            {
                                Nullable = false
                            });
                    break;
            }
            return typeusage;
        }

        internal static bool IsCanonicalFunction(EdmFunction function)
        {
            var isCanonicalFunction = (function.DataSpace == DataSpace.CSpace && function.NamespaceName == EdmConstants.EdmNamespace);

            Debug.Assert(
                !isCanonicalFunction || (isCanonicalFunction && !function.HasUserDefinedBody),
                "Canonical function '" + function.FullName + "' can not have a user defined body");

            return isCanonicalFunction;
        }
    }
}
