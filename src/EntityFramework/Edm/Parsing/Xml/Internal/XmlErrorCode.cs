namespace System.Data.Entity.Edm.Parsing.Xml.Internal
{
    /// <summary>
    ///     The acceptable range for this enum is 0000 - 0999; the range 10,000-15,000 is reserved for tools.
    /// </summary>
    internal static class XmlErrorCode
    {
        /// <summary>
        /// </summary>
        internal const int InvalidErrorCodeValue = 0;

        // unused 1,
        /// <summary>
        /// </summary>
        internal const int SecurityError = 2;

        // unused 3,
        /// <summary>
        /// </summary>
        internal const int IOException = 4;

        /// <summary>
        /// </summary>
        internal const int XmlError = 5;

        /// <summary>
        /// </summary>
        internal const int TooManyErrors = 6;

        /// <summary>
        /// </summary>
        internal const int MalformedXml = 7;

        /// <summary>
        /// </summary>
        internal const int UnexpectedXmlNodeType = 8;

        /// <summary>
        /// </summary>
        internal const int UnexpectedXmlAttribute = 9;

        /// <summary>
        /// </summary>
        internal const int UnexpectedXmlElement = 10;

        /// <summary>
        /// </summary>
        internal const int TextNotAllowed = 11;

        /// <summary>
        /// </summary>
        internal const int EmptyFile = 12;

        /// <summary>
        /// </summary>
        internal const int XsdError = 13;

        /// <summary>
        /// </summary>
        internal const int InvalidAlias = 14;

        /// <summary>
        /// </summary>
        internal const int MissingAttribute = 15;

        /// <summary>
        /// </summary>
        internal const int IntegerExpected = 16;

        /// <summary>
        /// </summary>
        internal const int InvalidName = 17;

        // unused 18,
        /// <summary>
        /// </summary>
        internal const int AlreadyDefined = 19;

        /// <summary>
        /// </summary>
        internal const int ElementNotInSchema = 20;

        // unused 21,
        /// <summary>
        /// </summary>
        internal const int InvalidBaseType = 22;

        /// <summary>
        /// </summary>
        internal const int NoConcreteDescendants = 23;

        /// <summary>
        /// </summary>
        internal const int CycleInTypeHierarchy = 24;

        /// <summary>
        /// </summary>
        internal const int InvalidVersionNumber = 25;

        /// <summary>
        /// </summary>
        internal const int InvalidSize = 26;

        /// <summary>
        /// </summary>
        internal const int InvalidBoolean = 27;

        // unused 28,
        /// <summary>
        /// </summary>
        internal const int BadType = 29;

        // unused 30,
        // unused 31,
        /// <summary>
        /// </summary>
        internal const int InvalidVersioningClass = 32;

        /// <summary>
        /// </summary>
        internal const int InvalidVersionIntroduced = 33;

        /// <summary>
        /// </summary>
        internal const int BadNamespace = 34;

        // unused 35,
        // unused 36,
        // unused 37,
        /// <summary>
        /// </summary>
        internal const int UnresolvedReferenceSchema = 38;

        // unused 39,
        /// <summary>
        /// </summary>
        internal const int NotInNamespace = 40;

        /// <summary>
        /// </summary>
        internal const int NotUnnestedType = 41;

        /// <summary>
        /// </summary>
        internal const int BadProperty = 42;

        /// <summary>
        /// </summary>
        internal const int UndefinedProperty = 43;

        /// <summary>
        /// </summary>
        internal const int InvalidPropertyType = 44;

        /// <summary>
        /// </summary>
        internal const int InvalidAsNestedType = 45;

        /// <summary>
        /// </summary>
        internal const int InvalidChangeUnit = 46;

        /// <summary>
        /// </summary>
        internal const int UnauthorizedAccessException = 47;

        // unused 48,
        // unused 49,
        // unused 50,
        /// <summary>
        ///     Precision out of range
        /// </summary>
        internal const int PrecisionOutOfRange = 51;

        /// <summary>
        ///     Scale out of range
        /// </summary>
        internal const int ScaleOutOfRange = 52;

        /// <summary>
        /// </summary>
        internal const int DefaultNotAllowed = 53;

        /// <summary>
        /// </summary>
        internal const int InvalidDefault = 54;

        /// <summary>
        ///     One of the required facets is missing
        /// </summary>
        internal const int RequiredFacetMissing = 55;

        /// <summary>
        /// </summary>
        internal const int BadImageFormatException = 56;

        /// <summary>
        /// </summary>
        internal const int MissingSchemaXml = 57;

        /// <summary>
        /// </summary>
        internal const int BadPrecisionAndScale = 58;

        /// <summary>
        /// </summary>
        internal const int InvalidChangeUnitUsage = 59;

        /// <summary>
        /// </summary>
        internal const int NameTooLong = 60;

        /// <summary>
        /// </summary>
        internal const int CircularlyDefinedType = 61;

        /// <summary>
        /// </summary>
        internal const int InvalidAssociation = 62;

        /// <summary>
        ///     The facet isn't allow by the property type.
        /// </summary>
        internal const int FacetNotAllowedByType = 63;

        /// <summary>
        ///     This facet value is constant and is specified in the schema
        /// </summary>
        internal const int ConstantFacetSpecifiedInSchema = 64;

        // unused 65,
        // unused 66,
        // unused 67,
        // unused 68,
        // unused 69,
        // unused 70,
        // unused 71,
        // unused 72,
        // unused 73,
        /// <summary>
        /// </summary>
        internal const int BadNavigationProperty = 74;

        /// <summary>
        /// </summary>
        internal const int InvalidKey = 75;

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
        /// <summary>
        ///     Multiplicity value was malformed
        /// </summary>
        internal const int InvalidMultiplicity = 92;

        // unused 93,
        // unused 94,
        // unused 95,
        /// <summary>
        ///     The value for the Action attribute is invalid or not allowed in the current context
        /// </summary>
        internal const int InvalidAction = 96;

        /// <summary>
        ///     An error occurred processing the On&lt;Operation&gt; elements
        /// </summary>
        internal const int InvalidOperation = 97;

        // unused 98,
        /// <summary>
        ///     Ends were given for the Property element of a EntityContainer that is not a RelationshipSet
        /// </summary>
        internal const int InvalidContainerTypeForEnd = 99;

        /// <summary>
        ///     The extent name used in the EntittyContainerType End does not match the name of any of the EntityContainerProperties in the containing EntityContainer
        /// </summary>
        internal const int InvalidEndEntitySet = 100;

        /// <summary>
        ///     An end element was not given, and cannot be inferred because too many EntityContainerEntitySet elements that are good possibilities.
        /// </summary>
        internal const int AmbiguousEntityContainerEnd = 101;

        /// <summary>
        ///     An end element was not given, and cannot be inferred because there is no EntityContainerEntitySets that are the correct type to be used as an EntitySet.
        /// </summary>
        internal const int MissingExtentEntityContainerEnd = 102;

        // unused 103,
        // unused 104,
        // unused 105,
        /// <summary>
        ///     Not a valid parameter direction for the parameter in a function
        /// </summary>
        internal const int BadParameterDirection = 106;

        /// <summary>
        ///     Unable to infer an optional schema part, to resolve this; be more explicit
        /// </summary>
        internal const int FailedInference = 107;

        // unused = 108,
        /// <summary>
        ///     Invalid facet attribute(s) specified in provider manifest
        /// </summary>
        internal const int InvalidFacetInProviderManifest = 109;

        /// <summary>
        ///     Invalid role value in the relationship constraint
        /// </summary>
        internal const int InvalidRoleInRelationshipConstraint = 110;

        /// <summary>
        ///     Invalid Property in relationship constraint
        /// </summary>
        internal const int InvalidPropertyInRelationshipConstraint = 111;

        /// <summary>
        ///     Type mismatch between ToProperty and FromProperty in the relationship constraint
        /// </summary>
        internal const int TypeMismatchRelationshipConstraint = 112;

        /// <summary>
        ///     Invalid multiplicity in FromRole in the relationship constraint
        /// </summary>
        internal const int InvalidMultiplicityInRoleInRelationshipConstraint = 113;

        /// <summary>
        ///     The number of properties in the FromProperty and ToProperty in the relationship constraint must be identical
        /// </summary>
        internal const int MismatchNumberOfPropertiesInRelationshipConstraint = 114;

        /// <summary>
        ///     No Properties defined in either FromProperty or ToProperty in the relationship constraint
        /// </summary>
        internal const int MissingPropertyInRelationshipConstraint = 115;

        /// <summary>
        ///     Missing constraint in relationship type in ssdl
        /// </summary>
        internal const int MissingConstraintOnRelationshipType = 116;

        // unused 117,
        // unused 118,
        /// <summary>
        ///     Same role referred in the ToRole and FromRole of a referential constraint
        /// </summary>
        internal const int SameRoleReferredInReferentialConstraint = 119;

        /// <summary>
        ///     Invalid value for attribute ParameterTypeSemantics
        /// </summary>
        internal const int InvalidValueForParameterTypeSemantics = 120;

        /// <summary>
        ///     Invalid type used for a Relationship End Type
        /// </summary>
        internal const int InvalidRelationshipEndType = 121;

        /// <summary>
        ///     Invalid PrimitiveTypeKind
        /// </summary>
        internal const int InvalidPrimitiveTypeKind = 122;

        // unused 123,
        /// <summary>
        ///     Invalid TypeConversion DestinationType
        /// </summary>
        internal const int InvalidTypeConversionDestinationType = 124;

        /// <summary>
        ///     Expected a integer value between 0 - 255
        /// </summary>
        internal const int ByteValueExpected = 125;

        /// <summary>
        ///     Invalid Type specified in function
        /// </summary>
        internal const int FunctionWithNonScalarTypeNotSupported = 126;

        /// <summary>
        ///     Precision must not be greater than 28
        /// </summary>
        internal const int PrecisionMoreThanAllowedMax = 127;

        /// <summary>
        ///     Properties that are part of entity key must be of scalar type
        /// </summary>
        internal const int EntityKeyMustBeScalar = 128;

        /// <summary>
        ///     Binary type properties which are part of entity key are currently not supported
        /// </summary>
        internal const int BinaryEntityKeyCurrentlyNotSupported = 129;

        /// <summary>
        ///     The primitive type kind does not have a preferred mapping
        /// </summary>
        internal const int NoPreferredMappingForPrimitiveTypeKind = 130;

        /// <summary>
        ///     More than one PreferredMapping for a PrimitiveTypeKind
        /// </summary>
        internal const int TooManyPreferredMappingsForPrimitiveTypeKind = 131;

        /// <summary>
        ///     End with * multiplicity cannot have operations specified
        /// </summary>
        internal const int EndWithManyMultiplicityCannotHaveOperationsSpecified = 132;

        /// <summary>
        ///     EntitySet type has no keys
        /// </summary>
        internal const int EntitySetTypeHasNoKeys = 133;

        /// <summary>
        ///     InvalidNumberOfParametersForAggregateFunction
        /// </summary>
        internal const int InvalidNumberOfParametersForAggregateFunction = 134;

        /// <summary>
        ///     InvalidParameterTypeForAggregateFunction
        /// </summary>
        internal const int InvalidParameterTypeForAggregateFunction = 135;

        /// <summary>
        ///     Composable functions must declare a return type.
        /// </summary>
        internal const int ComposableFunctionWithoutReturnType = 136;

        /// <summary>
        ///     Non-composable functions must not declare a return type.
        /// </summary>
        internal const int NonComposableFunctionWithReturnType = 137;

        /// <summary>
        ///     Non-composable functions do not permit the aggregate; niladic; or built-in attributes.
        /// </summary>
        internal const int NonComposableFunctionAttributesNotValid = 138;

        /// <summary>
        ///     Composable functions can not include command text attribute.
        /// </summary>
        internal const int ComposableFunctionWithCommandText = 139;

        /// <summary>
        ///     Functions should not declare both a store name and command text (only one or the other can be used).
        /// </summary>
        internal const int FunctionDeclaresCommandTextAndStoreFunctionName = 140;

        /// <summary>
        ///     SystemNamespace
        /// </summary>
        internal const int SystemNamespace = 141;

        /// <summary>
        ///     Empty DefiningQuery text
        /// </summary>
        internal const int EmptyDefiningQuery = 142;

        /// <summary>
        ///     Schema, Table and DefiningQuery are all specified, and are mutually exclusive
        /// </summary>
        internal const int TableAndSchemaAreMutuallyExclusiveWithDefiningQuery = 143;

        /// <summary>
        ///     ConcurrencyMode value was malformed
        /// </summary>
        internal const int InvalidConcurrencyMode = 144;

        /// <summary>
        ///     Concurrency can't change for any sub types of an EntitySet type.
        /// </summary>
        internal const int ConcurrencyRedefinedOnSubTypeOfEntitySetType = 145;

        /// <summary>
        ///     Function import return type must be either empty, a collection of entities, or a singleton scalar.
        /// </summary>
        internal const int FunctionImportUnsupportedReturnType = 146;

        /// <summary>
        ///     Function import specifies a non-existent entity set.
        /// </summary>
        internal const int FunctionImportUnknownEntitySet = 147;

        /// <summary>
        ///     Function import specifies entity type return but no entity set.
        /// </summary>
        internal const int FunctionImportReturnsEntitiesButDoesNotSpecifyEntitySet = 148;

        /// <summary>
        ///     Function import specifies entity type that does not derive from element type of entity set.
        /// </summary>
        internal const int FunctionImportEntityTypeDoesNotMatchEntitySet = 149;

        /// <summary>
        ///     Function import specifies a binding to an entity set but does not return entities.
        /// </summary>
        internal const int FunctionImportSpecifiesEntitySetButDoesNotReturnEntityType = 150;

        /// <summary>
        ///     InternalError
        /// </summary>
        internal const int InternalError = 152;

        /// <summary>
        ///     Same Entity Set Taking part in the same role of the relationship set in two different relationship sets
        /// </summary>
        internal const int SimilarRelationshipEnd = 153;

        /// <summary>
        ///     Entity key refers to the same property twice
        /// </summary>
        internal const int DuplicatePropertySpecifiedInEntityKey = 154;

        /// <summary>
        ///     Function declares a ReturnType attribute and element
        /// </summary>
        internal const int AmbiguousFunctionReturnType = 156;

        /// <summary>
        ///     Nullable Complex Type not supported in Edm V1
        /// </summary>
        internal const int NullableComplexType = 157;

        /// <summary>
        ///     Only Complex Collections supported in Edm V1.1
        /// </summary>
        internal const int NonComplexCollections = 158;

        /// <summary>
        ///     No Key defined on Entity Type
        /// </summary>
        internal const int KeyMissingOnEntityType = 159;

        /// <summary>
        ///     Invalid namespace specified in using element
        /// </summary>
        internal const int InvalidNamespaceInUsing = 160;

        /// <summary>
        ///     Need not specify system namespace in using
        /// </summary>
        internal const int NeedNotUseSystemNamespaceInUsing = 161;

        /// <summary>
        ///     Cannot use a reserved/system namespace as alias
        /// </summary>
        internal const int CannotUseSystemNamespaceAsAlias = 162;

        /// <summary>
        ///     Invalid qualification specified for type
        /// </summary>
        internal const int InvalidNamespaceName = 163;

        /// <summary>
        ///     Invalid Entity Container Name in extends attribute
        /// </summary>
        internal const int InvalidEntityContainerNameInExtends = 164;

        /// <summary>
        ///     Invalid CollectionKind value in property CollectionKind attribute
        /// </summary>
        internal const int InvalidCollectionKind = 165;

        /// <summary>
        ///     Must specify namespace or alias of the schema in which this type is defined
        /// </summary>
        internal const int InvalidNamespaceOrAliasSpecified = 166;

        /// <summary>
        ///     Entity Container cannot extend itself
        /// </summary>
        internal const int EntityContainerCannotExtendItself = 167;

        /// <summary>
        ///     Failed to retrieve provider manifest
        /// </summary>
        internal const int FailedToRetrieveProviderManifest = 168;

        /// <summary>
        ///     Mismatched Provider Manifest token values in SSDL artifacts
        /// </summary>
        internal const int ProviderManifestTokenMismatch = 169;

        /// <summary>
        ///     Missing Provider Manifest token value in SSDL artifact(s)
        /// </summary>
        internal const int ProviderManifestTokenNotFound = 170;

        /// <summary>
        ///     Empty CommandText element
        /// </summary>
        internal const int EmptyCommandText = 171;

        /// <summary>
        ///     Inconsistent Provider values in SSDL artifacts
        /// </summary>
        internal const int InconsistentProvider = 172;

        /// <summary>
        ///     Inconsistent Provider Manifest token values in SSDL artifacts
        /// </summary>
        internal const int InconsistentProviderManifestToken = 173;

        /// <summary>
        ///     Duplicated Function overloads
        /// </summary>
        internal const int DuplicatedFunctionoverloads = 174;

        /// <summary>
        ///     InvalidProvider
        /// </summary>
        internal const int InvalidProvider = 175;

        /// <summary>
        ///     FunctionWithNonEdmTypeNotSupported
        /// </summary>
        internal const int FunctionWithNonEdmTypeNotSupported = 176;

        /// <summary>
        ///     ComplexTypeAsReturnTypeAndDefinedEntitySet
        /// </summary>
        internal const int ComplexTypeAsReturnTypeAndDefinedEntitySet = 177;

        /// <summary>
        ///     ComplexTypeAsReturnTypeAndDefinedEntitySet
        /// </summary>
        internal const int ComplexTypeAsReturnTypeAndNestedComplexProperty = 178;

        /// unused 179,
        /// unused 180,
        /// unused 181,
        /// <summary>
        ///     In model functions facet attribute is allowed only on ScalarTypes
        /// </summary>
        internal const int FacetOnNonScalarType = 182;

        /// <summary>
        ///     Captures several conditions where facets are placed on element where it should not exist.
        /// </summary>
        internal const int IncorrectlyPlacedFacet = 183;

        /// <summary>
        ///     Return type has not been declared
        /// </summary>
        internal const int ReturnTypeNotDeclared = 184;

        internal const int TypeNotDeclared = 185;
        internal const int RowTypeWithoutProperty = 186;
        internal const int ReturnTypeDeclaredAsAttributeAndElement = 187;
        internal const int TypeDeclaredAsAttributeAndElement = 188;
        internal const int ReferenceToNonEntityType = 189;

        /// <summary>
        ///     Invalid value in the EnumTypeOption
        /// </summary>
        internal const int InvalidValueInEnumOption = 190;

        internal const int IncompatibleSchemaVersion = 191;

        /// <summary>
        ///     The structural annotation cannot use codegen namespaces
        /// </summary>
        internal const int NoCodeGenNamespaceInStructuralAnnotation = 192;

        /// <summary>
        ///     Function and type cannot have the same fully qualified name
        /// </summary>
        internal const int AmbiguousFunctionAndType = 193;

        /// <summary>
        ///     Cannot load different version of schema in the same ItemCollection
        /// </summary>
        internal const int CannotLoadDifferentVersionOfSchemaInTheSameItemCollection = 194;

        /// <summary>
        ///     Expected bool value
        /// </summary>
        internal const int BoolValueExpected = 195;

        /// <summary>
        ///     End without Multiplicity specified
        /// </summary>
        internal const int EndWithoutMultiplicity = 196;

        /// <summary>
        ///     In SSDL, if composable function returns a collection of rows (TVF), all row properties must be of scalar types.
        /// </summary>
        internal const int TVFReturnTypeRowHasNonScalarProperty = 197;

        /// <summary>
        ///     The name of NamedEdmItem must not be empty or white space only
        /// </summary>
        internal const int EdmModel_NameMustNotBeEmptyOrWhiteSpace = 198;

        /// <summary>
        ///     EdmTypeReference is empty
        /// </summary>
        /// Unused 199;
        internal const int EdmAssociationType_AssocationEndMustNotBeNull = 200;

        internal const int EdmAssociationConstraint_DependentEndMustNotBeNull = 201;
        internal const int EdmAssociationConstraint_DependentPropertiesMustNotBeEmpty = 202;
        internal const int EdmNavigationProperty_AssocationMustNotBeNull = 203;
        internal const int EdmNavigationProperty_ResultEndMustNotBeNull = 204;
        internal const int EdmAssociationEnd_EntityTypeMustNotBeNull = 205;
        internal const int EdmEntitySet_ElementTypeMustNotBeNull = 206;
        internal const int EdmAssociationSet_ElementTypeMustNotBeNull = 207;
        internal const int EdmAssociationSet_SourceSetMustNotBeNull = 208;
        internal const int EdmAssociationSet_TargetSetMustNotBeNull = 209;
        internal const int EdmFunctionImport_ReturnTypeMustBeCollectionType = 210;
        internal const int EdmModel_NameIsNotAllowed = 211;
        internal const int EdmTypeReferenceNotValid = 212;
        internal const int EdmFunctionNotExistsInV1 = 213;
        internal const int EdmFunctionNotExistsInV1_1 = 214;
        internal const int Serializer_OneNamespaceAndOneContainer = 215;
        internal const int EdmModel_Validator_Semantic_InvalidEdmTypeReference = 216;
        internal const int EdmModel_Validator_TypeNameAlreadyDefinedDuplicate = 217;
        internal const int EdmModel_Validator_DuplicateEntityContainerMemberName = 218;
        internal const int EdmFunction_UnsupportedParameterType = 219;
        internal const int EdmModel_Validator_InvalidAbstractComplexType = 220;
        internal const int EdmModel_Validator_InvalidPolymorphicComplexType = 221;
    }
}
