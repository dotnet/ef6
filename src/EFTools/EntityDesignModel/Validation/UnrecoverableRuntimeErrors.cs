// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Validation
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.SchemaObjectModel;

    internal static class UnrecoverableRuntimeErrors
    {
        /// <summary>
        ///     Schema Object Model error codes that should cause a document to be opened without designer support.
        ///     Note: the error code is commented because:
        ///     - The error is also commented in the runtime.
        ///     or
        ///     - The error can be fixed in the designer (marked by: // Designer --) so we don't want to trigger safe mode.
        /// </summary>
        public static readonly ErrorCode[] SchemaObjectModelErrorCodes
            = new[]
                {
                    // Designer -- ErrorCode.InvalidErrorCodeValue, 
                    ErrorCode.SecurityError,
                    ErrorCode.IOException,
                    ErrorCode.XmlError, 
                    // Designer -- ErrorCode.TooManyErrors, 
                    ErrorCode.MalformedXml,
                    ErrorCode.UnexpectedXmlNodeType,
                    ErrorCode.UnexpectedXmlAttribute,
                    ErrorCode.UnexpectedXmlElement,
                    ErrorCode.TextNotAllowed,
                    ErrorCode.EmptyFile,
                    ErrorCode.XsdError,
                    ErrorCode.InvalidAlias,
                    ErrorCode.IntegerExpected,
                    ErrorCode.InvalidName,
                    ErrorCode.ElementNotInSchema,
                    ErrorCode.InvalidBaseType,
                    ErrorCode.InvalidVersionNumber,
                    ErrorCode.InvalidSize,
                    ErrorCode.InvalidBoolean,
                    ErrorCode.BadType,
                    ErrorCode.InvalidVersioningClass,
                    ErrorCode.InvalidVersionIntroduced,
                    ErrorCode.BadNamespace,
                    ErrorCode.UnresolvedReferenceSchema,
                    ErrorCode.NotInNamespace,
                    ErrorCode.NotUnnestedType, 
                    // Designer -- ErrorCode.BadProperty, 
                    ErrorCode.UndefinedProperty,
                    ErrorCode.InvalidPropertyType,
                    ErrorCode.InvalidAsNestedType,
                    ErrorCode.InvalidChangeUnit,
                    ErrorCode.UnauthorizedAccessException, 
                    // Designer -- ErrorCode.PrecisionOutOfRange, 
                    // Designer -- ErrorCode.ScaleOutOfRange, 
                    ErrorCode.DefaultNotAllowed, 
                    // Designer -- ErrorCode.InvalidDefault, 
                    // Designer -- ErrorCode.RequiredFacetMissing, 
                    ErrorCode.BadImageFormatException,
                    ErrorCode.MissingSchemaXml, 
                    // Designer -- ErrorCode.BadPrecisionAndScale, 
                    // Designer -- ErrorCode.InvalidChangeUnitUsage, 
                    ErrorCode.NameTooLong, 
                    // Designer -- ErrorCode.CircularlyDefinedType, 

                    // Association has more than 2 Ends.
                    ErrorCode.InvalidAssociation,

                    // The facet isn't allowed by the property type.
                    ErrorCode.FacetNotAllowedByType, 
                    // This facet value is constant and is specified in the schema
                    ErrorCode.ConstantFacetSpecifiedInSchema,
                    ErrorCode.BadNavigationProperty, 
            
                    // This error is ambiguous, so err on the friendly side
                    // Designer -- ErrorCode.InvalidKey, 
            
                    // Multiplicity value was malformed
                    ErrorCode.InvalidMultiplicity, 
                    // The value for the Action attribute is not valid or not allowed in the current context
                    ErrorCode.InvalidAction, 
                    // An error occurred processing the On&lt;Operation&gt; elements
                    // Designer -- ErrorCode.InvalidOperation, 
                    // Ends were given for the Property element of a EntityContainer that is not a RelationshipSet
                    ErrorCode.InvalidContainerTypeForEnd, 
                    //  The extent name used in the EntityContainerType End does not match the name of any of the EntityContainerProperties in the containing EntityContainer
                    ErrorCode.InvalidEndEntitySet, 
                    //  An end element was not given, and cannot be inferred because too many EntityContainerEntitySet elements that are good possibilities.
                    ErrorCode.AmbiguousEntityContainerEnd, 
                    //  An end element was not given, and cannot be inferred because there is no EntityContainerEntitySets that are the correct type to be used as an EntitySet.
                    ErrorCode.MissingExtentEntityContainerEnd, 
                    //  Not a valid parameter direction for the parameter in a function
                    ErrorCode.BadParameterDirection, 
                    //  Unable to infer an optional schema part, to resolve this, be more explicit
                    ErrorCode.FailedInference, 
            
                    //  non-valid facet attribute(s) specified in provider manifest
                    // Designer -- ErrorCode.InvalidFacetInProviderManifest,             
                    ErrorCode.InvalidRoleInRelationshipConstraint, 
                    //ErrorCode.InvalidPropertyInRelationshipConstraint,  - we can fix this in the RC editor.
            
                    //  Type mismatch between ToProperty and FromProperty in the relationship constraint
                    //ErrorCode.TypeMismatchRelationshipConstaint, 

                    //ErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint, 
            
                    //  The number of properties in the FromProperty and ToProperty in the relationship constraint must be identical
                    //ErrorCode.MismatchNumberOfPropertiesInRelationshipConstraint, 
            
                    //  No Properties defined in either FromProperty or ToProperty in the relationship constraint
                    //ErrorCode.MissingPropertyInRelationshipConstraint, 

                    //  Missing constraint in relationship type in ssdl
                    ErrorCode.MissingConstraintOnRelationshipType, 
                    //  Same role referred in the ToRole and FromRole of a referential constraint 
                    ErrorCode.SameRoleReferredInReferentialConstraint, 
                    // Designer -- ErrorCode.InvalidValueForParameterTypeSemantics, 
                    ErrorCode.InvalidRelationshipEndType, 
                    // Designer -- ErrorCode.InvalidPrimitiveTypeKind, 
                    // Designer -- ErrorCode.InvalidDateTimeKind, 
                    // Designer -- ErrorCode.InvalidTypeConversionDestinationType, 
            
                    //  Expected a integer value between 0 - 255
                    // Designer -- ErrorCode.ByteValueExpected, 
            
                    //  non-valid Type specified in function
                    // Designer -- ErrorCode.FunctionWithNonScalarTypeNotSupported, 
            
                    //  Precision must not be greater than 28 
                    // Designer -- ErrorCode.PrecisionMoreThan29, 
            
                    //  Properties that are part of entity key must be of scalar type
                    // Designer -- ErrorCode.EntityKeyMustBeScalar, 

                    //  Binary type properties which are part of entity key are currently not supported 
                    // Designer -- ErrorCode.BinaryEntityKeyCurrentlyNotSupported, 
            
                    //  The primitive type kind does not have a preferred mapping
                    // Designer -- ErrorCode.NoPreferredMappingForPrimitiveTypeKind,
            
                    //  More than one PreferredMapping for a PrimitiveTypeKind
                    // Designer -- ErrorCode.TooManyPreferredMappingsForPrimitiveTypeKind, 
            
                    //  End with * multiplicity cannot have operations specified
                    // Designer -- ErrorCode.EndWithManyMultiplicityCannotHaveOperationsSpecified, 

                    //  EntitySet type has no keys
                    // Designer -- ErrorCode.EntitySetTypeHasNoKeys,
            
                    //  InvalidNumberOfParametersForAggregateFunction
                    // Designer -- ErrorCode.InvalidNumberOfParametersForAggregateFunction,
            
                    //  InvalidParameterTypeForAggregateFunction
                    // Designer -- ErrorCode.InvalidParameterTypeForAggregateFunction,
            
                    //  Composable functions must declare a return type.
                    // Designer -- ErrorCode.ComposableFunctionWithoutReturnType,

                    //  Non-composable functions must not declare a return type.
                    // Designer -- ErrorCode.NonComposableFunctionWithReturnType,
            
                    //  Non-composable functions do not permit the aggregate, niladic, or 
                    /// built-in attributes.
                    // Designer -- ErrorCode.NonComposableFunctionAttributesNotValid,
            
                    //  Composable functions can not include command text attribute.
                    ErrorCode.ComposableFunctionWithCommandText,
            
                    //  Functions should not declare both a store name and command text
                    /// (only one or the other can be used).
                    ErrorCode.FunctionDeclaresCommandTextAndStoreFunctionName,
            
                    // Designer -- ErrorCode.SystemNamespace,
            
                    //  Empty DefiningQuery text
                    ErrorCode.EmptyDefiningQuery,
            
                    //  Schema, Table and DefiningQuery are all specified, and are mutually exclusive
                    ErrorCode.TableAndSchemaAreMutuallyExclusiveWithDefiningQuery,
            
                    //  Provider manifest does not allow a type to explicitly promote to itself, this is an implicit assumption.
                    // Designer -- ErrorCode.ProviderManifestExplicitPromotionToSelf,
            
                    //  Concurrency can't change for any sub types of an EntitySet type.
                    // Designer -- ErrorCode.ConcurrencyRedefinedOnSubTypeOfEntitySetType,
            
                    //  Function import return type must be either empty, a collection of entities, or a singleton scalar.
                    // Designer -- ErrorCode.FunctionImportUnsupportedReturnType,
            
                    //  Function import specifies a non-existent entity set.
                    // Designer -- ErrorCode.FunctionImportUnknownEntitySet,

                    ///Designer Function import specifies entity type return but no entity set.
                    // Designer -- ErrorCode.FunctionImportReturnsEntitiesButDoesNotSpecifyEntitySet,
            
                    //  Function import specifies entity type that does not derive from element type of entity set.
                    // Designer -- ErrorCode.FunctionImportEntityTypeDoesNotMatchEntitySet,
            
                    //  Function import specifies a binding to an entity set but does not return entities.
                    // Designer -- ErrorCode.FunctionImportSpecifiesEntitySetButDoesNotReturnEntityType,
            
                    //  InternalError
                    // Designer -- ErrorCode.InternalError,

                    //  Same Entity Set Taking part in the same role of the relationship set in two different relationship sets
                    ErrorCode.SimilarRelationshipEnd,
                    //  Entity key refers to the same property twice
                    ErrorCode.DuplicatePropertySpecifiedInEntityKey,
                    //  Function declares a ReturnType attribute and element
                    ErrorCode.AmbiguousFunctionReturnType,
                    //  Nullable Complex Type not supported in Edm V1
                    ErrorCode.NullableComplexType,
                    //  Only Complex Collections supported in Edm V1.1
                    ErrorCode.NonComplexCollections,

                    //  No Key defined on Entity Type 
                    // Designer -- ErrorCode.KeyMissingOnEntityType,
                    ErrorCode.InvalidNamespaceInUsing,
                    //  Need not specify system namespace in using
                    ErrorCode.NeedNotUseSystemNamespaceInUsing,
                    //  Cannot use a reserved/system namespace as alias
                    // Designer -- ErrorCode.CannotUseSystemNamespaceAsAlias,
                    //  non-valid qualification specified for type
                    ErrorCode.InvalidNamespaceName,
                    ErrorCode.InvalidEntityContainerNameInExtends,
            
                    //  Must specify namespace or alias of the schema in which this type is defined
                    // Designer -- ErrorCode.InvalidNamespaceOrAliasSpecified,
            
                    //  Entity Container cannot extend itself
                    ErrorCode.EntityContainerCannotExtendItself,
            
                    //  Failed to retrieve provider manifest
                    // Designer -- ErrorCode.FailedToRetrieveProviderManifest,

                    //  Mismatched Provider Manifest token values in SSDL artifacts
                    // Designer -- ErrorCode.ProviderManifestTokenMismatch,
            
                    //  Missing Provider Manifest token value in SSDL artifact(s)
                    // Designer -- ErrorCode.ProviderManifestTokenNotFound,
            
                    //  Empty CommandText element
                    ErrorCode.EmptyCommandText,
            
                    /// Inconsistent Provider Manifest token values in SSDL artifacts
                    ErrorCode.InconsistentProviderManifestToken,

                    //  Duplicated Function overloads
                    ErrorCode.DuplicatedFunctionoverloads,

                    // TODO: Review error codes below and disable error codes that can be fixed from designer.

                    ErrorCode.InvalidProvider,
                    ErrorCode.FunctionWithNonEdmTypeNotSupported,
                    ErrorCode.ComplexTypeAsReturnTypeAndDefinedEntitySet,
                    ErrorCode.ComplexTypeAsReturnTypeAndNestedComplexProperty,
        
                    // In model functions facet attribute is allowed only on ScalarTypes
                    ErrorCode.FacetOnNonScalarType,

                    // Captures several conditions where facets are placed on element where it should not exist.
                    ErrorCode.IncorrectlyPlacedFacet,

                    // Return type has not been declared
                    ErrorCode.ReturnTypeNotDeclared,
                    ErrorCode.TypeNotDeclared,
                    ErrorCode.RowTypeWithoutProperty,
                    ErrorCode.ReturnTypeDeclaredAsAttributeAndElement,
                    ErrorCode.TypeDeclaredAsAttributeAndElement,
                    ErrorCode.ReferenceToNonEntityType,
                    ErrorCode.IncompatibleSchemaVersion,
            
                    // The structural annotation cannot use codegen namespaces
                    ErrorCode.NoCodeGenNamespaceInStructuralAnnotation,

                    // Function and type cannot have the same fully qualified name</summary>
                    ErrorCode.AmbiguousFunctionAndType,

                    // Cannot load different version of schema in the same ItemCollection</summary>
                    ErrorCode.CannotLoadDifferentVersionOfSchemaInTheSameItemCollection,

                    // Expected bool value</summary>
                    ErrorCode.BoolValueExpected,

                    // End without Multiplicity specified
                    ErrorCode.EndWithoutMultiplicity,

                    // In SSDL, if composable function returns a collection of rows (TVF), all row properties must be of scalar types.
                    ErrorCode.TVFReturnTypeRowHasNonScalarProperty,

                    // FunctionUnknownEntityContainer,
                    // FunctionEntityContainerMustBeSpecified,
                    // FunctionUnknownEntitySet,
                    // FunctionEntitySetMustBeSpecified,

                    // Defining expression and entity set can not be specified at the same time.
                    ErrorCode.FunctionWithDefiningExpressionAndEntitySetNotAllowed,

                    // Function specifies return type that does not derive from element type of entity set.
                    ErrorCode.FunctionEntityTypeScopeDoesNotMatchReturnType,

                    // The specified type cannot be used as the underlying type of Enum type.
                    ErrorCode.InvalidEnumUnderlyingType,

                    // Duplicate enumeration member.
                    ErrorCode.DuplicateEnumMember,

                    // The calculated value for an enum member is ouf of Int64 range.
                    ErrorCode.CalculatedEnumValueOutOfRange, 

                    // The enumeration value for an enum member is out of its underlying type range.
                    ErrorCode.EnumMemberValueOutOfItsUnderylingTypeRange,
                };

        /// Mapping Error Codes that should cause a document to be opened without designer support.  
        /// 
        /// Note: the error code is commented because:
        /// - The error is also commented in the runtime.
        /// or
        /// - The error can be fixed in the designer (marked by: // Designer --) so we don't want to trigger safe mode.
        /// </summary>
        public static readonly MappingErrorCode[] StorageMappingErrorCodes
            = new[]
                {
                    // TableMappingFragment element expected
                    MappingErrorCode.TableMappingFragmentExpected,
            
                    // SetMappingFragment element expected
                    MappingErrorCode.SetMappingExpected,
            
                    // Designer Duplicate Conditions for a member
                    // Designer -- MappingErrorCode.DuplicateCondition,

                    MappingErrorCode.DuplicateSetMapping,
                    MappingErrorCode.DuplicateTypeMapping,
                    MappingErrorCode.ConditionError,
                    // MISSING IN RUNTIME - MappingErrorCode.InvalidXmlNamespace. Is in spec but error code doesn't exist

                    MappingErrorCode.RootMappingElementMissing,
                    // Designer -- MappingErrorCode.IncompatibleMemberMapping,

                    // MISSING IN RUNTIME -  MappingErrorCode.MappingSchemaValidationError.  Is in spec but error code doesn't exist             
                    // MISSING IN RUNTIME -  MappingErrorCode.InvalidSchemaExtension.  Is in spec but error code doesn't exist 
            
                    // non-valid Enum Value
                    MappingErrorCode.InvalidEnumValue,

                    // Xml Schema Validation error
                    MappingErrorCode.XmlSchemaValidationError,
            
                    // Designer Ambiguous Function Mapping For AssociationSet
                    // Designer -- MappingErrorCode.AmbiguousFunctionMappingForAssociationSet,
            
                    // Designer Missing Set Closure In FunctionMapping
                    // Designer -- MappingErrorCode.MissingSetClosureInFunctionMapping,
            
                    // Designer Missing Function Mapping For Entity Type
                    // Designer -- MappingErrorCode.MissingFunctionMappingForEntityType,

                    MappingErrorCode.InvalidTableNameAttributeWithModificationFunctionMapping,
                    MappingErrorCode.InvalidModificationFunctionMappingForMultipleTypes,

                    // Designer Ambiguous Result Binding In Function Mapping
                    // Designer -- MappingErrorCode.AmbiguousResultBindingInFunctionMapping,
                    // Designer -- MappingErrorCode.InvalidAssociationSetRoleInFunctionMapping,
                    // Designer -- MappingErrorCode.InvalidAssociationSetCardinalityInFunctionMapping,

                    MappingErrorCode.RedundantEntityTypeMappingInModificationFunctionMapping,
                    MappingErrorCode.MissingVersionInModificationFunctionMapping,
                    MappingErrorCode.InvalidVersionInModificationFunctionMapping,
                    MappingErrorCode.ParameterBoundTwiceInModificationFunctionMapping,
            
                    // Designer
                    // MappingErrorCode.CSpaceMemberMappedToMultipleSSpaceMemberWithDifferentTypes,
            
                    // Designer No store type found for the given CSpace type (these error message is for primitive type with no facets)
                    // Designer -- MappingErrorCode.NoEquivalentStorePrimitiveTypeFound,

                    // Designer No Store type found for the given CSpace type with the given set of facets
                    // Designer -- MappingErrorCode.NoEquivalentStorePrimitiveTypeWithFacetsFound,
            
                    // Designer While mapping functions, if the property type is not compatible with the function parameter
                    // Designer -- MappingErrorCode.InvalidFunctionMappingPropertyParameterTypeMismatch,
            
                    // XML While mapping functions, if more than one end of association is mapped
                    MappingErrorCode.InvalidModificationFunctionMappingMultipleEndsOfAssociationMapped,
                    MappingErrorCode.InvalidModificationFunctionMappingUnknownFunction,
                    MappingErrorCode.InvalidModificationFunctionMappingAmbiguousFunction,
                        
                    // Designer While mapping functions, if we find a non-valid function parameter
                    // Designer -- MappingErrorCode.InvalidFunctionMappingNotValidFunctionParameter,
            
                    // Association set function mappings are not consistently defined for different operations
                    // Designer -- MappingErrorCode.InvalidFunctionMappingAssociationSetNotMappedForOperation,
            
                    //XML Entity type function mapping includes association end but the type is not part of the association
                    MappingErrorCode
                        .InvalidModificationFunctionMappingAssociationEndMappingInvalidForEntityType,

                    // XML Function import mapping references non-existent store function
                    MappingErrorCode.MappingFunctionImportStoreFunctionDoesNotExist,
            
                    // XML Function import mapping references store function with overloads (overload resolution is not possible)
                    MappingErrorCode.MappingFunctionImportStoreFunctionAmbiguous,
            
                    // XML Function import mapping reference non-existent import
                    MappingErrorCode.MappingFunctionImportFunctionImportDoesNotExist,
            
                    // XML Function import mapping is mapped in several locations
                    MappingErrorCode.MappingFunctionImportFunctionImportMappedMultipleTimes,

                    // Designer Attempting to map composable function
                    // Designer -- MappingErrorCode.MappingFunctionImportTargetFunctionMustBeComposable,
            
                    // Designer No parameter on import side corresponding to target parameter
                    // Designer -- MappingErrorCode.MappingFunctionImportTargetParameterHasNoCorrespondingImportParameter,
            
                    // Designer No parameter on target side corresponding to import parameter
                    // Designer -- MappingErrorCode.MappingFunctionImportImportParameterHasNoCorrespondingTargetParameter,
            
                    // Designer Parameter directions are different
                    // Designer -- MappingErrorCode.MappingFunctionImportIncompatibleParameterMode,

                    // Designer Parameter types are different
                    // Designer -- MappingErrorCode.MappingFunctionImportIncompatibleParameterType,
            
                    // XML Rows affected parameter does not exist on mapped function
                    MappingErrorCode.MappingFunctionImportRowsAffectedParameterDoesNotExist,
            
                    // XML Rows affected parameter does not Int32
                    MappingErrorCode.MappingFunctionImportRowsAffectedParameterHasWrongType,
            
                    // XML Rows affected does not have 'out' mode
                    MappingErrorCode.MappingFunctionImportRowsAffectedParameterHasWrongMode,

                    // Designer Empty Container Mapping
                    // Designer -- MappingErrorCode.EmptyContainerMapping,
            
                    // Designer Empty Set Mapping
                    //StorageMappingErrorCode.EmptySetMapping,
            
                    // Both TableName Attribute on Set Mapping and QueryView specified
                    MappingErrorCode.TableNameAttributeWithQueryView,
            
                    // Empty Query View
                    MappingErrorCode.EmptyQueryView,

                    // Both Query View and Property Maps specified for EntitySet
                    MappingErrorCode.PropertyMapsWithQueryView,
            
                    // Some sets in the graph missing Query Views
                    MappingErrorCode.MissingSetClosureInQueryViews,
                    MappingErrorCode.InvalidQueryView,
                    MappingErrorCode.InvalidQueryViewResultType,
            
                    // Designer Item with same name exists both in CSpace and SSpace
                    // Designer -- MappingErrorCode.ItemWithSameNameExistsBothInCSpaceAndSSpace,
            
                    // XML Unsupported expression kind in query view
                    MappingErrorCode.MappingUnsupportedExpressionKindQueryView,
            
                    // XML Non S-space target in query view
                    MappingErrorCode.MappingUnsupportedScanTargetQueryView,
            
                    // Non structural property referenced in query view
                    MappingErrorCode.MappingUnsupportedPropertyKindQueryView,

                    // Initialization non-target type in query view
                    MappingErrorCode.MappingUnsupportedInitializationQueryView,

                    // Designer EntityType mapping for non-entity set function
                    // Designer -- MappingErrorCode.MappingFunctionImportEntityTypeMappingForFunctionNotReturningEntitySet,
            
                    // XML FunctionImport ambiguous type mappings
                    MappingErrorCode.MappingFunctionImportAmbiguousTypeConditions,

                    // XML FunctionImport includes multiple conditions for single column
            
                    // XML Abstract Entity Type being mapped
                    // 20080425 - code 2078 MappingErrorCode.EntityType does not have concrete descendants. no longer requires XML-Editor only mode (see bug 629341)
                    // MappingErrorCode.MappingForAbstractEntityType,
            
                    // XML Storage EntityContainer Name mismatch while specifying partial mapping
                    MappingErrorCode.StorageEntityContainerNameMismatchWhileSpecifyingPartialMapping,
            
                    // XML TypeName attribute specified for First QueryView
                    MappingErrorCode.TypeNameForFirstQueryView,

                    // Designer No TypeName attribute is specified for type-specific QueryViews
                    // Designer -- MappingErrorCode.NoTypeNameForTypeSpecificQueryView,
            
                    // Designer Multiple (optype/oftypeonly) QueryViews have been defined for the same EntitySet/EntityType
                    // Designer -- MappingErrorCode.QueryViewExistsForEntitySetAndType,
            
                    // Designer TypeName Contains Multiple Types For QueryView
                    // Designer -- MappingErrorCode.TypeNameContainsMultipleTypesForQueryView,
            
                    // Designer IsTypeOf QueryView is specified for base type
                    // Designer -- MappingErrorCode.IsTypeOfQueryViewForBaseType,

                    // XML ScalarProperty Element contains non-valid type
                    MappingErrorCode.InvalidTypeInScalarProperty,
            
                    // XML Already Mapped Storage Container
                    MappingErrorCode.AlreadyMappedStorageEntityContainer,
            
                    // XML No query view is allowed at compile time in EntityContainerMapping
                    MappingErrorCode.UnsupportedQueryViewInEntityContainerMapping,
            
                    // XML EntityContainerMapping only contains query view
                    MappingErrorCode.MappingAllQueryViewAtCompileTime,

                    // TODO: Review error codes below and disable error codes that can be fixed from designer.

                    // No views can be generated since all of the EntityContainerMapping contain query view
                    MappingErrorCode.MappingNoViewsCanBeGenerated, 

                    // The store provider returns null EdmType for the given targetParameter's type
                    MappingErrorCode.MappingStoreProviderReturnsNullEdmType, 
                    //MappingFunctionImportInvalidMemberName,

                    // Multiple mappings of the same Member or Property inside the same mapping fragment.
                    MappingErrorCode.DuplicateMemberMapping,
            
                    // MappingErrorCode.MappingFunctionImportPartialRenameMapping,

                    // Entity type mapping for a function import that does not return a collection of entity type.
                    MappingErrorCode.MappingFunctionImportUnexpectedEntityTypeMapping,

                    // Complex type mapping for a function import that does not return a collection of complex type.
                    MappingErrorCode.MappingFunctionImportUnexpectedComplexTypeMapping,

                    // Distinct flag can only be placed in a container that is not read-write
                    MappingErrorCode.DistinctFragmentInReadWriteContainer,

                    // The EntitySet used in creating the Ref and the EntitySet declared in AssociationSetEnd do not match
                    MappingErrorCode.EntitySetMismatchOnAssociationSetEnd,

                    // FKs not permitted for function association ends.
                    MappingErrorCode.InvalidModificationFunctionMappingAssociationEndForeignKey,

                    // EdmItemCollectionVersionIncompatible,
                    // StoreItemCollectionVersionIncompatible,

                    // Cannot load different version of schemas in the same ItemCollection
                    MappingErrorCode.CannotLoadDifferentVersionOfSchemaInTheSameItemCollection,
                    MappingErrorCode.MappingDifferentMappingEdmStoreVersion,
                    MappingErrorCode.MappingDifferentEdmStoreVersion,

                    // All function imports must be mapped.
                    MappingErrorCode.UnmappedFunctionImport,
    
                    // Invalid function import result mapping: return type property not mapped.
                    MappingErrorCode.MappingFunctionImportReturnTypePropertyNotMapped,

                    // MappingErrorCode.AmbiguousFunction,

                    // Unresolvable Type Name
                    MappingErrorCode.InvalidType,

                    // MappingErrorCode.FunctionResultMappingTypeMismatch,

                    // TVF expected on the store side.
                    MappingErrorCode.MappingFunctionImportTVFExpected,

                    // Collection(Scalar) function import return type is not compatible with the TVF column type.
                    MappingErrorCode.MappingFunctionImportScalarMappingTypeMismatch,

                    // Collection(Scalar) function import must be mapped to a TVF returning a single column.
                    MappingErrorCode.MappingFunctionImportScalarMappingToMulticolumnTVF,

                    // Attempting to map composable function import to a non-composable function.
                    MappingErrorCode.MappingFunctionImportTargetFunctionMustBeComposable,

                    // Non-s-space function call in query view.
                    MappingErrorCode.UnsupportedFunctionCallInQueryView,
    
                    // Invalid function result mapping: result mapping count doesn't match result type count.
                    MappingErrorCode.FunctionResultMappingCountMismatch,
                };
    }
}
