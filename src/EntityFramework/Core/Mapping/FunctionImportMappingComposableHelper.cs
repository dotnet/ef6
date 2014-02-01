// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Xml;

    internal class FunctionImportMappingComposableHelper
    {
        private readonly EntityContainerMapping _entityContainerMapping;
        private readonly string m_sourceLocation;
        private readonly List<EdmSchemaError> m_parsingErrors;        

        internal FunctionImportMappingComposableHelper(
            EntityContainerMapping entityContainerMapping,
            string sourceLocation, 
            List<EdmSchemaError> parsingErrors)
        {
            _entityContainerMapping = entityContainerMapping;
            m_sourceLocation = sourceLocation;
            m_parsingErrors = parsingErrors;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal bool TryCreateFunctionImportMappingComposableWithStructuralResult(
            EdmFunction functionImport,
            EdmFunction cTypeTargetFunction,
            List<FunctionImportStructuralTypeMapping> typeMappings,
            RowType cTypeTvfElementType,
            RowType sTypeTvfElementType,
            IXmlLineInfo lineInfo,
            out FunctionImportMappingComposable mapping)
        {
            mapping = null;

            // If it is an implicit structural type mapping, add a type mapping fragment for the return type of the function import,
            // unless it is an abstract type.
            if (typeMappings.Count == 0)
            {
                StructuralType resultType;
                if (MetadataHelper.TryGetFunctionImportReturnType(functionImport, 0, out resultType))
                {
                    if (resultType.Abstract)
                    {
                        AddToSchemaErrorWithMemberAndStructure(
                            Strings.Mapping_FunctionImport_ImplicitMappingForAbstractReturnType,
                            resultType.FullName, functionImport.Identity,
                            MappingErrorCode.MappingOfAbstractType, m_sourceLocation, lineInfo, m_parsingErrors);
                        return false;
                    }
                    if (resultType.BuiltInTypeKind
                        == BuiltInTypeKind.EntityType)
                    {
                        typeMappings.Add(
                            new FunctionImportEntityTypeMapping(
                                Enumerable.Empty<EntityType>(),
                                new[] { (EntityType)resultType },
                                Enumerable.Empty<FunctionImportEntityTypeMappingCondition>(),
                                new Collection<FunctionImportReturnTypePropertyMapping>(),
                                new LineInfo(lineInfo)));
                    }
                    else
                    {
                        Debug.Assert(
                            resultType.BuiltInTypeKind == BuiltInTypeKind.ComplexType,
                            "resultType.BuiltInTypeKind == BuiltInTypeKind.ComplexType");
                        typeMappings.Add(
                            new FunctionImportComplexTypeMapping(
                                (ComplexType)resultType,
                                new Collection<FunctionImportReturnTypePropertyMapping>(),
                                new LineInfo(lineInfo)));
                    }
                }
            }


            // when this method is invoked when a CodeFirst model is being built (e.g. from a custom convention) the
            // StorageMappingItemCollection will be null. In this case we can provide an empty EdmItemCollection which
            // will allow inferring implicit result mapping
            var edmItemCollection =
                _entityContainerMapping.StorageMappingItemCollection != null
                    ? _entityContainerMapping.StorageMappingItemCollection.EdmItemCollection
                    : new EdmItemCollection(new EdmModel(DataSpace.CSpace));

            // Validate and convert FunctionImportEntityTypeMapping elements into structure suitable for composable function import mapping.
            var functionImportKB = new FunctionImportStructuralTypeMappingKB(typeMappings, edmItemCollection);

            var structuralTypeMappings =
                new List<Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>>>();
            EdmProperty[] targetFunctionKeys = null;
            if (functionImportKB.MappedEntityTypes.Count > 0)
            {
                // Validate TPH ambiguity.
                if (!functionImportKB.ValidateTypeConditions( /*validateAmbiguity: */true, m_parsingErrors, m_sourceLocation))
                {
                    return false;
                }

                // For each mapped entity type, prepare list of conditions and list of property mappings.
                for (var i = 0; i < functionImportKB.MappedEntityTypes.Count; ++i)
                {
                    List<ConditionPropertyMapping> typeConditions;
                    List<PropertyMapping> propertyMappings;
                    if (TryConvertToEntityTypeConditionsAndPropertyMappings(
                        functionImport,
                        functionImportKB,
                        i,
                        cTypeTvfElementType,
                        sTypeTvfElementType,
                        lineInfo, out typeConditions, out propertyMappings))
                    {
                        structuralTypeMappings.Add(
                            Tuple.Create((StructuralType)functionImportKB.MappedEntityTypes[i], typeConditions, propertyMappings));
                    }
                }
                if (structuralTypeMappings.Count
                    < functionImportKB.MappedEntityTypes.Count)
                {
                    // Some of the entity types produced errors during conversion, exit.
                    return false;
                }

                // Infer target function keys based on the c-space entity types.
                if (!TryInferTVFKeys(structuralTypeMappings, out targetFunctionKeys))
                {
                    AddToSchemaErrorsWithMemberInfo(
                        Strings.Mapping_FunctionImport_CannotInferTargetFunctionKeys, functionImport.Identity,
                        MappingErrorCode.MappingFunctionImportCannotInferTargetFunctionKeys, m_sourceLocation, lineInfo,
                        m_parsingErrors);
                    return false;
                }
            }
            else
            {
                ComplexType resultComplexType;
                if (MetadataHelper.TryGetFunctionImportReturnType(functionImport, 0, out resultComplexType))
                {
                    // Gather and validate complex type property mappings.
                    List<PropertyMapping> propertyMappings;
                    if (
                        !TryConvertToPropertyMappings(
                            resultComplexType, cTypeTvfElementType, sTypeTvfElementType, functionImport, functionImportKB, lineInfo,
                            out propertyMappings))
                    {
                        return false;
                    }
                    structuralTypeMappings.Add(
                        Tuple.Create((StructuralType)resultComplexType, new List<ConditionPropertyMapping>(), propertyMappings));
                }
                else
                {
                    Debug.Fail("Function import return type is expected to be a collection of complex type.");
                }
            }

            mapping = new FunctionImportMappingComposable(
                functionImport,
                cTypeTargetFunction,
                structuralTypeMappings,
                targetFunctionKeys,
                _entityContainerMapping);
            return true;
        }

        internal bool TryCreateFunctionImportMappingComposableWithScalarResult(
            EdmFunction functionImport,
            EdmFunction cTypeTargetFunction,
            EdmFunction sTypeTargetFunction,
            EdmType scalarResultType,
            RowType cTypeTvfElementType,
            IXmlLineInfo lineInfo,
            out FunctionImportMappingComposable mapping)
        {
            mapping = null;

            // Make sure that TVF returns exactly one column
            if (cTypeTvfElementType.Properties.Count > 1)
            {
                AddToSchemaErrors(
                    Strings.Mapping_FunctionImport_ScalarMappingToMulticolumnTVF(functionImport.Identity, sTypeTargetFunction.Identity),
                    MappingErrorCode.MappingFunctionImportScalarMappingToMulticolumnTVF, m_sourceLocation, lineInfo, m_parsingErrors);
                return false;
            }

            // Make sure that scalarResultType agrees with the column type.
            if (
                !ValidateFunctionImportMappingResultTypeCompatibility(
                    TypeUsage.Create(scalarResultType), cTypeTvfElementType.Properties[0].TypeUsage))
            {
                AddToSchemaErrors(
                    Strings.Mapping_FunctionImport_ScalarMappingTypeMismatch(
                        functionImport.ReturnParameter.TypeUsage.EdmType.FullName,
                        functionImport.Identity,
                        sTypeTargetFunction.ReturnParameter.TypeUsage.EdmType.FullName,
                        sTypeTargetFunction.Identity),
                    MappingErrorCode.MappingFunctionImportScalarMappingTypeMismatch, m_sourceLocation, lineInfo, m_parsingErrors);
                return false;
            }

            mapping = new FunctionImportMappingComposable(
                functionImport,
                cTypeTargetFunction,
                null,
                null,
                _entityContainerMapping);
            return true;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private bool TryConvertToEntityTypeConditionsAndPropertyMappings(
            EdmFunction functionImport,
            FunctionImportStructuralTypeMappingKB functionImportKB,
            int typeID,
            RowType cTypeTvfElementType,
            RowType sTypeTvfElementType,
            IXmlLineInfo navLineInfo,
            out List<ConditionPropertyMapping> typeConditions,
            out List<PropertyMapping> propertyMappings)
        {
            var entityType = functionImportKB.MappedEntityTypes[typeID];
            typeConditions = new List<ConditionPropertyMapping>();

            var errorFound = false;

            // Gather and validate entity type conditions from the type-producing fragments.
            foreach (var entityTypeMapping in functionImportKB.NormalizedEntityTypeMappings.Where(f => f.ImpliedEntityTypes[typeID]))
            {
                foreach (var condition in entityTypeMapping.ColumnConditions.Where(c => c != null))
                {
                    EdmProperty column;
                    if (sTypeTvfElementType.Properties.TryGetValue(condition.ColumnName, false, out column))
                    {
                        object value;
                        bool? isNull;
                        if (condition.ConditionValue.IsSentinel)
                        {
                            value = null;
                            if (condition.ConditionValue
                                == ValueCondition.IsNull)
                            {
                                isNull = true;
                            }
                            else
                            {
                                Debug.Assert(
                                    condition.ConditionValue == ValueCondition.IsNotNull,
                                    "Only IsNull or IsNotNull condition values are expected.");
                                isNull = false;
                            }
                        }
                        else
                        {
                            var cTypeColumn = cTypeTvfElementType.Properties[column.Name];
                            Debug.Assert(cTypeColumn != null, "cTypeColumn != null");
                            Debug.Assert(
                                Helper.IsPrimitiveType(cTypeColumn.TypeUsage.EdmType),
                                "S-space columns are expected to be of a primitive type.");
                            var cPrimitiveType = (PrimitiveType)cTypeColumn.TypeUsage.EdmType;
                            Debug.Assert(cPrimitiveType.ClrEquivalentType != null, "Scalar Types should have associated clr type");
                            Debug.Assert(
                                condition is FunctionImportEntityTypeMappingConditionValue,
                                "Non-sentinel condition is expected to be of type FunctionImportEntityTypeMappingConditionValue.");
                            value = ((FunctionImportEntityTypeMappingConditionValue)condition).GetConditionValue(
                                cPrimitiveType.ClrEquivalentType,
                                handleTypeNotComparable: () =>
                                {
                                    AddToSchemaErrorWithMemberAndStructure(
                                        Strings.
                                            Mapping_InvalidContent_ConditionMapping_InvalidPrimitiveTypeKind,
                                        column.Name, column.TypeUsage.EdmType.FullName,
                                        MappingErrorCode.ConditionError,
                                        m_sourceLocation, condition.LineInfo, m_parsingErrors);
                                },
                                handleInvalidConditionValue: () =>
                                {
                                    AddToSchemaErrors(
                                        Strings.Mapping_ConditionValueTypeMismatch,
                                        MappingErrorCode.ConditionError,
                                        m_sourceLocation, condition.LineInfo, m_parsingErrors);
                                });
                            if (value == null)
                            {
                                errorFound = true;
                                continue;
                            }
                            isNull = null;
                        }
                        typeConditions.Add(new ConditionPropertyMapping(null, column, value, isNull));
                    }
                    else
                    {
                        AddToSchemaErrorsWithMemberInfo(
                            Strings.Mapping_InvalidContent_Column, condition.ColumnName,
                            MappingErrorCode.InvalidStorageMember,
                            m_sourceLocation, condition.LineInfo, m_parsingErrors);
                    }
                }
            }

            // Gather and validate entity type property mappings.
            errorFound |=
                !TryConvertToPropertyMappings(
                    entityType, cTypeTvfElementType, sTypeTvfElementType, functionImport, functionImportKB, navLineInfo,
                    out propertyMappings);

            return !errorFound;
        }

        private bool TryConvertToPropertyMappings(
            StructuralType structuralType,
            RowType cTypeTvfElementType,
            RowType sTypeTvfElementType,
            EdmFunction functionImport,
            FunctionImportStructuralTypeMappingKB functionImportKB,
            IXmlLineInfo navLineInfo,
            out List<PropertyMapping> propertyMappings)
        {
            propertyMappings = new List<PropertyMapping>();

            // Gather and validate structuralType property mappings.
            var errorFound = false;
            foreach (EdmProperty property in TypeHelpers.GetAllStructuralMembers(structuralType))
            {
                // Only scalar property mappings are supported at the moment.
                if (!Helper.IsScalarType(property.TypeUsage.EdmType))
                {
                    var error = new EdmSchemaError(
                        Strings.Mapping_Invalid_CSide_ScalarProperty(property.Name),
                        (int)MappingErrorCode.InvalidTypeInScalarProperty,
                        EdmSchemaErrorSeverity.Error,
                        m_sourceLocation, navLineInfo.LineNumber, navLineInfo.LinePosition);
                    m_parsingErrors.Add(error);
                    errorFound = true;
                    continue;
                }

                string columnName = null;
                IXmlLineInfo columnMappingLineInfo = null;
                FunctionImportReturnTypeStructuralTypeColumnRenameMapping columnRenameMapping;
                bool explicitPropertyMapping;
                if (functionImportKB.ReturnTypeColumnsRenameMapping.TryGetValue(property.Name, out columnRenameMapping))
                {
                    explicitPropertyMapping = true;
                    columnName = columnRenameMapping.GetRename(structuralType, out columnMappingLineInfo);
                }
                else
                {
                    explicitPropertyMapping = false;
                    columnName = property.Name;
                }
                columnMappingLineInfo = columnMappingLineInfo != null && columnMappingLineInfo.HasLineInfo()
                                            ? columnMappingLineInfo
                                            : navLineInfo;

                EdmProperty column;
                if (sTypeTvfElementType.Properties.TryGetValue(columnName, false, out column))
                {
                    Debug.Assert(cTypeTvfElementType.Properties.Contains(columnName), "cTypeTvfElementType.Properties.Contains(columnName)");
                    var cTypeColumn = cTypeTvfElementType.Properties[columnName];
                    if (ValidateFunctionImportMappingResultTypeCompatibility(property.TypeUsage, cTypeColumn.TypeUsage))
                    {
                        propertyMappings.Add(new ScalarPropertyMapping(property, column));
                    }
                    else
                    {
                        var error = new EdmSchemaError(
                            GetInvalidMemberMappingErrorMessage(property, column),
                            (int)MappingErrorCode.IncompatibleMemberMapping,
                            EdmSchemaErrorSeverity.Error,
                            m_sourceLocation, columnMappingLineInfo.LineNumber, columnMappingLineInfo.LinePosition);
                        m_parsingErrors.Add(error);
                        errorFound = true;
                    }
                }
                else
                {
                    if (explicitPropertyMapping)
                    {
                        AddToSchemaErrorsWithMemberInfo(
                            Strings.Mapping_InvalidContent_Column, columnName,
                            MappingErrorCode.InvalidStorageMember,
                            m_sourceLocation, columnMappingLineInfo, m_parsingErrors);
                        errorFound = true;
                    }
                    else
                    {
                        var error = new EdmSchemaError(
                            Strings.Mapping_FunctionImport_PropertyNotMapped(
                                property.Name, structuralType.FullName, functionImport.Identity),
                            (int)MappingErrorCode.MappingFunctionImportReturnTypePropertyNotMapped,
                            EdmSchemaErrorSeverity.Error,
                            m_sourceLocation, columnMappingLineInfo.LineNumber, columnMappingLineInfo.LinePosition);
                        m_parsingErrors.Add(error);
                        errorFound = true;
                    }
                }
            }

            // Make sure that propertyMappings is in the order of properties of the structuredType.
            // The rest of the code depends on it.
            Debug.Assert(
                errorFound ||
                TypeHelpers.GetAllStructuralMembers(structuralType).Count == propertyMappings.Count &&
                TypeHelpers.GetAllStructuralMembers(structuralType).Cast<EdmMember>().Zip(propertyMappings)
                           .All(ppm => ppm.Key.EdmEquals(ppm.Value.Property)),
                "propertyMappings order does not correspond to the order of properties in the structuredType.");

            return !errorFound;
        }

        // <summary>
        // Attempts to infer key columns of the target function based on the function import mapping.
        // </summary>
        private static bool TryInferTVFKeys(
            List<Tuple<StructuralType, List<ConditionPropertyMapping>, List<PropertyMapping>>> structuralTypeMappings,
            out EdmProperty[] keys)
        {
            keys = null;
            Debug.Assert(structuralTypeMappings.Count > 0, "Function import returning entities must have non-empty structuralTypeMappings.");
            foreach (var typeMapping in structuralTypeMappings)
            {
                EdmProperty[] currentKeys;
                if (!TryInferTVFKeysForEntityType((EntityType)typeMapping.Item1, typeMapping.Item3, out currentKeys))
                {
                    keys = null;
                    return false;
                }
                if (keys == null)
                {
                    keys = currentKeys;
                }
                else
                {
                    // Make sure all keys are mapped to the same columns.
                    Debug.Assert(keys.Length == currentKeys.Length, "All subtypes must have the same number of keys.");
                    for (var i = 0; i < keys.Length; ++i)
                    {
                        if (!keys[i].EdmEquals(currentKeys[i]))
                        {
                            keys = null;
                            return false;
                        }
                    }
                }
            }
            // Make sure columns are non-nullable, otherwise it shouldn't be considered a key.
            for (var i = 0; i < keys.Length; ++i)
            {
                if (keys[i].Nullable)
                {
                    keys = null;
                    return false;
                }
            }
            return true;
        }

        private static bool TryInferTVFKeysForEntityType(
            EntityType entityType, List<PropertyMapping> propertyMappings, out EdmProperty[] keys)
        {
            keys = new EdmProperty[entityType.KeyMembers.Count];
            for (var i = 0; i < keys.Length; ++i)
            {
                var mapping =
                    propertyMappings[entityType.Properties.IndexOf((EdmProperty)entityType.KeyMembers[i])] as ScalarPropertyMapping;
                if (mapping == null)
                {
                    keys = null;
                    return false;
                }
                keys[i] = mapping.Column;
            }
            return true;
        }

        private static bool ValidateFunctionImportMappingResultTypeCompatibility(TypeUsage cSpaceMemberType, TypeUsage sSpaceMemberType)
        {
            // Function result data flows from S-side to C-side.
            var fromType = sSpaceMemberType;
            var toType = ResolveTypeUsageForEnums(cSpaceMemberType);

            var directlyPromotable = TypeSemantics.IsStructurallyEqualOrPromotableTo(fromType, toType);
            var inverselyPromotable = TypeSemantics.IsStructurallyEqualOrPromotableTo(toType, fromType);

            // We are quite lax here. We only require that values belong to the same class (can flow in one or the other direction).
            // We could require precisely s-type to be promotable to c-type, but in this case it won't be possible to reuse the same 
            // c-types for mapped functions and entity sets, because entity sets (read-write) require c-types to be promotable to s-types.
            return directlyPromotable || inverselyPromotable;
        }

        private static TypeUsage ResolveTypeUsageForEnums(TypeUsage typeUsage)
        {
            return MappingItemLoader.ResolveTypeUsageForEnums(typeUsage);
        }

        private static void AddToSchemaErrors(
            string message, MappingErrorCode errorCode, string location, IXmlLineInfo lineInfo, IList<EdmSchemaError> parsingErrors)
        {
            MappingItemLoader.AddToSchemaErrors(message, errorCode, location, lineInfo, parsingErrors);
        }

        private static void AddToSchemaErrorsWithMemberInfo(
            Func<object, string> messageFormat, string errorMember, MappingErrorCode errorCode, string location,
            IXmlLineInfo lineInfo, IList<EdmSchemaError> parsingErrors)
        {
            MappingItemLoader.AddToSchemaErrorsWithMemberInfo(
                messageFormat, errorMember, errorCode, location, lineInfo, parsingErrors);
        }

        private static void AddToSchemaErrorWithMemberAndStructure(
            Func<object, object, string> messageFormat, string errorMember,
            string errorStructure, MappingErrorCode errorCode, string location, IXmlLineInfo lineInfo,
            IList<EdmSchemaError> parsingErrors)
        {
            MappingItemLoader.AddToSchemaErrorWithMemberAndStructure(
                messageFormat, errorMember, errorStructure, errorCode, location, lineInfo, parsingErrors);
        }

        private static string GetInvalidMemberMappingErrorMessage(EdmMember cSpaceMember, EdmMember sSpaceMember)
        {
            return MappingItemLoader.GetInvalidMemberMappingErrorMessage(cSpaceMember, sSpaceMember);
        }
    }
}
