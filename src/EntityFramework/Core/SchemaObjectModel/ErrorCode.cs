// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    // This file contains an enum for the errors generated by Metadata Loading (SOM)
    //
    // There is almost a one-to-one correspondence between these error codes
    // and the resource strings - so if you need more insight into what the
    // error code means, please see the code that uses the particular enum
    // AND the corresponding resource string
    //
    // error numbers end up being hard coded in test cases; they can be removed, but should not be changed.
    // reusing error numbers is probably OK, but not recommended.
    //
    // The acceptable range for this enum is
    // 0000 - 0999
    //
    // The Range 10,000-15,000 is reserved for tools
    //
    // <summary>
    // Summary description for ErrorCode.
    // </summary>
    internal enum ErrorCode
    {
        InvalidErrorCodeValue = 0,
        // unused 1,
        SecurityError = 2,
        // unused 3,
        IOException = 4,
        XmlError = 5,
        TooManyErrors = 6,
        MalformedXml = 7,
        UnexpectedXmlNodeType = 8,
        UnexpectedXmlAttribute = 9,
        UnexpectedXmlElement = 10,
        TextNotAllowed = 11,
        EmptyFile = 12,
        XsdError = 13,
        InvalidAlias = 14,
        // unused 15,
        IntegerExpected = 16,
        InvalidName = 17,
        // unused 18,
        AlreadyDefined = 19,
        ElementNotInSchema = 20,
        // unused 21,
        InvalidBaseType = 22,
        NoConcreteDescendants = 23,
        CycleInTypeHierarchy = 24,
        InvalidVersionNumber = 25,
        InvalidSize = 26,
        InvalidBoolean = 27,
        // unused 28,
        BadType = 29,
        // unused 30,
        // unused 31,
        InvalidVersioningClass = 32,
        InvalidVersionIntroduced = 33,
        BadNamespace = 34,
        // unused 35,
        // unused 36,
        // unused 37,
        UnresolvedReferenceSchema = 38,
        // unused 39,
        NotInNamespace = 40,
        NotUnnestedType = 41,
        BadProperty = 42,
        UndefinedProperty = 43,
        InvalidPropertyType = 44,
        InvalidAsNestedType = 45,
        InvalidChangeUnit = 46,
        UnauthorizedAccessException = 47,
        // unused 48,
        // unused 49,

        // <summary>
        // Namespace attribute must be specified.
        // </summary>
        MissingNamespaceAttribute = 50,

        // <summary>
        // Precision out of range
        // </summary>
        PrecisionOutOfRange = 51,

        // <summary>
        // Scale out of range
        // </summary>
        ScaleOutOfRange = 52,

        DefaultNotAllowed = 53,
        InvalidDefault = 54,

        // <summary>
        // One of the required facets is missing
        // </summary>
        RequiredFacetMissing = 55,

        BadImageFormatException = 56,
        MissingSchemaXml = 57,
        BadPrecisionAndScale = 58,
        InvalidChangeUnitUsage = 59,
        NameTooLong = 60,
        CircularlyDefinedType = 61,
        InvalidAssociation = 62,

        // <summary>
        // The facet isn't allow by the property type.
        // </summary>
        FacetNotAllowedByType = 63,

        // <summary>
        // This facet value is constant and is specified in the schema
        // </summary>
        ConstantFacetSpecifiedInSchema = 64,

        // unused 65,
        // unused 66,
        // unused 67,
        // unused 68,
        // unused 69,
        // unused 70,
        // unused 71,
        // unused 72,
        // unused 73,
        BadNavigationProperty = 74,
        InvalidKey = 75,
        // unused 76,
        // unused 77,
        // unused 78,
        // unused 79,
        // unused 80,
        // unused 81,
        // unused 82,
        // unused 83,
        // unused 84,
        // unused 85,
        // unused 86,
        // unused 87,
        // unused 88,
        // unused 89,
        // unused 90,
        // unused 91,

        // <summary>
        // Multiplicity value was malformed
        // </summary>
        InvalidMultiplicity = 92,

        // unused 93,
        // unused 94,
        // unused 95,

        // <summary>
        // The value for the Action attribute is invalid or not allowed in the current context
        // </summary>
        InvalidAction = 96,

        // <summary>
        // An error occurred processing the On&lt;Operation&gt;  elements
        // </summary>
        InvalidOperation = 97,

        // unused 98,

        // <summary>
        // Ends were given for the Property element of a EntityContainer that is not a RelationshipSet
        // </summary>
        InvalidContainerTypeForEnd = 99,

        // <summary>
        // The extent name used in the EntityContainerType End does not match the name of any of the EntityContainerProperties in the containing EntityContainer
        // </summary>
        InvalidEndEntitySet = 100,

        // <summary>
        // An end element was not given, and cannot be inferred because too many EntityContainerEntitySet elements that are good possibilities.
        // </summary>
        AmbiguousEntityContainerEnd = 101,

        // <summary>
        // An end element was not given, and cannot be inferred because there is no EntityContainerEntitySets that are the correct type to be used as an EntitySet.
        // </summary>
        MissingExtentEntityContainerEnd = 102,

        // unused 103,
        // unused 104,
        // unused 105,

        // <summary>
        // Not a valid parameter direction for the parameter in a function
        // </summary>
        BadParameterDirection = 106,

        // <summary>
        // Unable to infer an optional schema part, to resolve this, be more explicit
        // </summary>
        FailedInference = 107,

        // unused = 108,

        // <summary>
        // Invalid facet attribute(s) specified in provider manifest
        // </summary>
        InvalidFacetInProviderManifest = 109,

        // <summary>
        // Invalid role value in the relationship constraint
        // </summary>
        InvalidRoleInRelationshipConstraint = 110,

        // <summary>
        // Invalid Property in relationship constraint
        // </summary>
        InvalidPropertyInRelationshipConstraint = 111,

        // <summary>
        // Type mismatch between ToProperty and FromProperty in the relationship constraint
        // </summary>
        TypeMismatchRelationshipConstraint = 112,

        // <summary>
        // Invalid multiplicty in FromRole in the relationship constraint
        // </summary>
        InvalidMultiplicityInRoleInRelationshipConstraint = 113,

        // <summary>
        // The number of properties in the FromProperty and ToProperty in the relationship constraint must be identical
        // </summary>
        MismatchNumberOfPropertiesInRelationshipConstraint = 114,

        // <summary>
        // No Properties defined in either FromProperty or ToProperty in the relationship constraint
        // </summary>
        MissingPropertyInRelationshipConstraint = 115,

        // <summary>
        // Missing constraint in relationship type in ssdl
        // </summary>
        MissingConstraintOnRelationshipType = 116,

        // unused 117,
        // unused 118,

        // <summary>
        // Same role referred in the ToRole and FromRole of a referential constraint
        // </summary>
        SameRoleReferredInReferentialConstraint = 119,

        // <summary>
        // Invalid value for attribute ParameterTypeSemantics
        // </summary>
        InvalidValueForParameterTypeSemantics = 120,

        // <summary>
        // Invalid type used for a Relationship End Type
        // </summary>
        InvalidRelationshipEndType = 121,

        // <summary>
        // Invalid PrimitiveTypeKind
        // </summary>
        InvalidPrimitiveTypeKind = 122,

        // unused 123,

        // <summary>
        // Invalid TypeConversion DestinationType
        // </summary>
        InvalidTypeConversionDestinationType = 124,

        // <summary>
        // Expected a integer value between 0 - 255
        // </summary>
        ByteValueExpected = 125,

        // <summary>
        // Invalid Type specified in function
        // </summary>
        FunctionWithNonPrimitiveTypeNotSupported = 126,

        // <summary>
        // Precision must not be greater than 28
        // </summary>
        PrecisionMoreThanAllowedMax = 127,

        // <summary>
        // Properties that are part of entity key must be of scalar type
        // </summary>
        EntityKeyMustBeScalar = 128,

        // <summary>
        // Binary and spatial type properties which are part of entity key are currently not supported
        // </summary>
        EntityKeyTypeCurrentlyNotSupported = 129,

        // <summary>
        // The primitive type kind does not have a prefered mapping
        // </summary>
        NoPreferredMappingForPrimitiveTypeKind = 130,

        // <summary>
        // More than one PreferredMapping for a PrimitiveTypeKind
        // </summary>
        TooManyPreferredMappingsForPrimitiveTypeKind = 131,

        // <summary>
        // End with * multiplicity cannot have operations specified
        // </summary>
        EndWithManyMultiplicityCannotHaveOperationsSpecified = 132,

        // <summary>
        // EntitySet type has no keys
        // </summary>
        EntitySetTypeHasNoKeys = 133,

        // <summary>
        // InvalidNumberOfParametersForAggregateFunction
        // </summary>
        InvalidNumberOfParametersForAggregateFunction = 134,

        // <summary>
        // InvalidParameterTypeForAggregateFunction
        // </summary>
        InvalidParameterTypeForAggregateFunction = 135,

        // <summary>
        // Composable functions and function imports must declare a return type.
        // </summary>
        ComposableFunctionOrFunctionImportWithoutReturnType = 136,

        // <summary>
        // Non-composable functions must not declare a return type.
        // </summary>
        NonComposableFunctionWithReturnType = 137,

        // <summary>
        // Non-composable functions do not permit the aggregate, niladic, or built-in attributes.
        // </summary>
        NonComposableFunctionAttributesNotValid = 138,

        // <summary>
        // Composable functions can not include command text attribute.
        // </summary>
        ComposableFunctionWithCommandText = 139,

        // <summary>
        // Functions should not declare both a store name and command text (only one or the other
        // can be used).
        // </summary>
        FunctionDeclaresCommandTextAndStoreFunctionName = 140,

        // <summary>
        // SystemNamespace
        // </summary>
        SystemNamespace = 141,

        // <summary>
        // Empty DefiningQuery text
        // </summary>
        EmptyDefiningQuery = 142,

        // <summary>
        // Schema, Table and DefiningQuery are all specified, and are mutually exclusive
        // </summary>
        TableAndSchemaAreMutuallyExclusiveWithDefiningQuery = 143,

        // unused 144,

        // <summary>
        // Concurrency can't change for any sub types of an EntitySet type.
        // </summary>
        ConcurrencyRedefinedOnSubTypeOfEntitySetType = 145,

        // <summary>
        // Function import return type must be either empty, a collection of entities, or a singleton scalar.
        // </summary>
        FunctionImportUnsupportedReturnType = 146,

        // <summary>
        // Function import specifies a non-existent entity set.
        // </summary>
        FunctionImportUnknownEntitySet = 147,

        // <summary>
        // Function import specifies entity type return but no entity set.
        // </summary>
        FunctionImportReturnsEntitiesButDoesNotSpecifyEntitySet = 148,

        // <summary>
        // Function import specifies entity type that does not derive from element type of entity set.
        // </summary>
        FunctionImportEntityTypeDoesNotMatchEntitySet = 149,

        // <summary>
        // Function import specifies a binding to an entity set but does not return entities.
        // </summary>
        FunctionImportSpecifiesEntitySetButDoesNotReturnEntityType = 150,

        // <summary>
        // InternalError
        // </summary>
        InternalError = 152,

        // <summary>
        // Same Entity Set Taking part in the same role of the relationship set in two different relationship sets
        // </summary>
        SimilarRelationshipEnd = 153,

        // <summary>
        // Entity key refers to the same property twice
        // </summary>
        DuplicatePropertySpecifiedInEntityKey = 154,

        // <summary>
        // Function declares a ReturnType attribute and element
        // </summary>
        AmbiguousFunctionReturnType = 156,

        // <summary>
        // Nullable Complex Type not supported in Edm V1
        // </summary>
        NullableComplexType = 157,

        // <summary>
        // Only Complex Collections supported in Edm V1.1
        // </summary>
        NonComplexCollections = 158,

        // <summary>
        // No Key defined on Entity Type
        // </summary>
        KeyMissingOnEntityType = 159,

        // <summary>
        // Invalid namespace specified in using element
        // </summary>
        InvalidNamespaceInUsing = 160,

        // <summary>
        // Need not specify system namespace in using
        // </summary>
        NeedNotUseSystemNamespaceInUsing = 161,

        // <summary>
        // Cannot use a reserved/system namespace as alias
        // </summary>
        CannotUseSystemNamespaceAsAlias = 162,

        // <summary>
        // Invalid qualification specified for type
        // </summary>
        InvalidNamespaceName = 163,

        // <summary>
        // Invalid Entity Container Name in extends attribute
        // </summary>
        InvalidEntityContainerNameInExtends = 164,

        // unused 165,

        // <summary>
        // Must specify namespace or alias of the schema in which this type is defined
        // </summary>
        InvalidNamespaceOrAliasSpecified = 166,

        // <summary>
        // Entity Container cannot extend itself
        // </summary>
        EntityContainerCannotExtendItself = 167,

        // <summary>
        // Failed to retrieve provider manifest
        // </summary>
        FailedToRetrieveProviderManifest = 168,

        // <summary>
        // Mismatched Provider Manifest token values in SSDL artifacts
        // </summary>
        ProviderManifestTokenMismatch = 169,

        // <summary>
        // Missing Provider Manifest token value in SSDL artifact(s)
        // </summary>
        ProviderManifestTokenNotFound = 170,

        // <summary>
        // Empty CommandText element
        // </summary>
        EmptyCommandText = 171,

        // <summary>
        // Inconsistent Provider values in SSDL artifacts
        // </summary>
        InconsistentProvider = 172,

        // <summary>
        // Inconsistent Provider Manifest token values in SSDL artifacts
        // </summary>
        InconsistentProviderManifestToken = 173,

        // <summary>
        // Duplicated Function overloads
        // </summary>
        DuplicatedFunctionoverloads = 174,

        // <summary>
        // InvalidProvider
        // </summary>
        InvalidProvider = 175,

        // <summary>
        // FunctionWithNonEdmTypeNotSupported
        // </summary>
        FunctionWithNonEdmTypeNotSupported = 176,

        // <summary>
        // ComplexTypeAsReturnTypeAndDefinedEntitySet
        // </summary>
        ComplexTypeAsReturnTypeAndDefinedEntitySet = 177,

        // <summary>
        // ComplexTypeAsReturnTypeAndDefinedEntitySet
        // </summary>
        ComplexTypeAsReturnTypeAndNestedComplexProperty = 178,

        // unused = 179,

        // <summary>
        // A function import can be either composable or side-effecting, but not both.
        // </summary>
        FunctionImportComposableAndSideEffectingNotAllowed = 180,

        // <summary>
        // A function import can specify an entity set or an entity set path, but not both.
        // </summary>
        FunctionImportEntitySetAndEntitySetPathDeclared = 181,

        // <summary>
        // In model functions facet attribute is allowed only on ScalarTypes
        // </summary>
        FacetOnNonScalarType = 182,

        // <summary>
        // Captures several conditions where facets are placed on element where it should not exist.
        // </summary>
        IncorrectlyPlacedFacet = 183,

        // <summary>
        // Return type has not been declared
        // </summary>
        ReturnTypeNotDeclared = 184,

        TypeNotDeclared = 185,
        RowTypeWithoutProperty = 186,
        ReturnTypeDeclaredAsAttributeAndElement = 187,
        TypeDeclaredAsAttributeAndElement = 188,
        ReferenceToNonEntityType = 189,

        // <summary>
        // Collection and reference type parameters are not allowed in function imports.
        // </summary>
        FunctionImportCollectionAndRefParametersNotAllowed = 190,

        IncompatibleSchemaVersion = 191,

        // <summary>
        // The structural annotation cannot use codegen namespaces
        // </summary>
        NoCodeGenNamespaceInStructuralAnnotation = 192,

        // <summary>
        // Function and type cannot have the same fully qualified name
        // </summary>
        AmbiguousFunctionAndType = 193,

        // <summary>
        // Cannot load different version of schema in the same ItemCollection
        // </summary>
        CannotLoadDifferentVersionOfSchemaInTheSameItemCollection = 194,

        // <summary>
        // Expected bool value
        // </summary>
        BoolValueExpected = 195,

        // <summary>
        // End without Multiplicity specified
        // </summary>
        EndWithoutMultiplicity = 196,

        // <summary>
        // In SSDL, if composable function returns a collection of rows (TVF), all row properties must be of scalar types.
        // </summary>
        TVFReturnTypeRowHasNonScalarProperty = 197,

        // FunctionUnknownEntityContainer = 198,
        // FunctionEntityContainerMustBeSpecified = 199,
        // FunctionUnknownEntitySet = 200,

        // <summary>
        // Only nullable parameters are supported in function imports.
        // </summary>
        FunctionImportNonNullableParametersNotAllowed = 201,

        // <summary>
        // Defining expression and entity set can not be specified at the same time.
        // </summary>
        FunctionWithDefiningExpressionAndEntitySetNotAllowed = 202,

        // <summary>
        // Function specifies return type that does not derive from element type of entity set.
        // </summary>
        FunctionEntityTypeScopeDoesNotMatchReturnType = 203,

        // <summary>
        // The specified type cannot be used as the underlying type of Enum type.
        // </summary>
        InvalidEnumUnderlyingType = 204,

        // <summary>
        // Duplicate enumeration member.
        // </summary>
        DuplicateEnumMember = 205,

        // <summary>
        // The calculated value for an enum member is ouf of Int64 range.
        // </summary>
        CalculatedEnumValueOutOfRange = 206,

        // <summary>
        // The enumeration value for an enum member is out of its underlying type range.
        // </summary>
        EnumMemberValueOutOfItsUnderylingTypeRange = 207,

        // <summary>
        // The Srid value is out of range.
        // </summary>
        InvalidSystemReferenceId = 208,

        // <summary>
        // A CSDL spatial type in a file without the UseSpatialUnionType annotation
        // </summary>
        UnexpectedSpatialType = 209,
    }
}
