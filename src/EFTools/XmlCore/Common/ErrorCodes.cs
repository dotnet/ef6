// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Common
{
    internal class ErrorCodes
    {
        // values are exclusive
        internal static readonly int _RUNTIME_CSDL_START = 0;
        internal static readonly int _RUNTIME_CSDL_END = 999;
        internal static readonly int _RUNTIME_MSL_START = 2000;
        internal static readonly int _RUNTIME_MSL_END = 2999;

        // Note that these ErrorCodes were taken from DataEntityDesign.  The error codes here and in DataEntityDesign should not conflict.
        internal static readonly int ERROR_NUMBER_BASE = 10000;

        // Escher errors have the range from 10,000 - 15,000.  The runtime has the range from 1-9999.  We don't want conflicting error numbers, so 
        // please stay in this range.
        internal static readonly int NORMALIZE_DUPLICATE_SYMBOL_DEFINED = ERROR_NUMBER_BASE + 1;
        internal static readonly int RESOLVE_CONDITION_BOUND_TO_PROP_AND_COLUMN = ERROR_NUMBER_BASE + 2;
        internal static readonly int RESOLVE_UNRESOLVED_ALIAS = ERROR_NUMBER_BASE + 3;
        internal static readonly int LEADING_OR_TRAILING_SPACES_IN_NCNAME = ERROR_NUMBER_BASE + 4;
        internal static readonly int COLON_IN_NC_NAME = ERROR_NUMBER_BASE + 5;
        internal static readonly int BAD_FIRST_CHAR_IN_NC_NAME = ERROR_NUMBER_BASE + 6;
        internal static readonly int INVALID_NC_NAME_CHAR = ERROR_NUMBER_BASE + 7;
        internal static readonly int GENERIC_SCHEMA_ERROR = ERROR_NUMBER_BASE + 8;
        internal static readonly int TOO_MANY_REFERENTIAL_CONSTRAINTS_IN_ASSOCIATION = ERROR_NUMBER_BASE + 9;
        internal static readonly int TOO_MANY_KEY_ELEMENTS = ERROR_NUMBER_BASE + 10;
        internal static readonly int ModelParse_MutuallyExclusiveAttributeAndChildElement = ERROR_NUMBER_BASE + 11;
        internal static readonly int ModelParse_AliasElementMissingKeyAttribute = ERROR_NUMBER_BASE + 12;
        internal static readonly int ModelParse_AliasElementMissingValueAttribute = ERROR_NUMBER_BASE + 13;
        internal static readonly int INVALID_VALUE = ERROR_NUMBER_BASE + 14;
        internal static readonly int FATAL_PARSE_ERROR = ERROR_NUMBER_BASE + 15;
        internal static readonly int FATAL_RESOLVE_ERROR = ERROR_NUMBER_BASE + 16;
        internal static readonly int TOO_MANY_DOCUMENTATION_ELEMENTS = ERROR_NUMBER_BASE + 17;
        internal static readonly int TOO_MANY_ENTITY_CONTAINER_ELEMENTS = ERROR_NUMBER_BASE + 18;
        internal static readonly int DATA_SERVICES_NODE_DETECTED = ERROR_NUMBER_BASE + 19;
        internal static readonly int NON_QUALIFIED_ELEMENT = ERROR_NUMBER_BASE + 20;
        internal static readonly int DUPLICATED_ELEMENT_ENCOUNTERED = ERROR_NUMBER_BASE + 21;
        internal static readonly int UNEXPECTED_ELEMENT_ENCOUNTERED = ERROR_NUMBER_BASE + 22;
        internal static readonly int ErrorValidatingArtifact_ConceptualModelMissing = ERROR_NUMBER_BASE + 23;
        internal static readonly int ErrorValidatingArtifact_StorageModelMissing = ERROR_NUMBER_BASE + 24;
        internal static readonly int ErrorValidatingArtifact_MappingModelMissing = ERROR_NUMBER_BASE + 25;
        internal static readonly int ModelParse_GhostNodeNotSupportedByDesigner = ERROR_NUMBER_BASE + 26;
        internal static readonly int ErrorValidatingArtifact_InvalidCSDLNamespaceForTargetFrameworkVersion = ERROR_NUMBER_BASE + 27;
        internal static readonly int ErrorValidatingArtifact_InvalidSSDLNamespaceForTargetFrameworkVersion = ERROR_NUMBER_BASE + 28;
        internal static readonly int ErrorValidatingArtifact_InvalidMSLNamespaceForTargetFrameworkVersion = ERROR_NUMBER_BASE + 29;
        internal static readonly int ExtensionsError_BufferNotEditable = ERROR_NUMBER_BASE + 30;

        internal static readonly int ESCHER_VALIDATOR_ERROR_NUMBER_BASE = 11000;
        internal static readonly int ESCHER_VALIDATOR_CIRCULAR_INHERITANCE = ESCHER_VALIDATOR_ERROR_NUMBER_BASE + 1;
        internal static readonly int ESCHER_VALIDATOR_ENTITY_TYPE_WITHOUT_ENTITY_SET = ESCHER_VALIDATOR_ERROR_NUMBER_BASE + 2;
        internal static readonly int ESCHER_VALIDATOR_MULTIPE_ENTITY_SETS_PER_TYPE = ESCHER_VALIDATOR_ERROR_NUMBER_BASE + 3;
        internal static readonly int ESCHER_VALIDATOR_ASSOCIATION_WITHOUT_ASSOCIATION_SET = ESCHER_VALIDATOR_ERROR_NUMBER_BASE + 4;
        internal static readonly int ESCHER_VALIDATOR_INCLUDES_COMPLEX_TYPE = ESCHER_VALIDATOR_ERROR_NUMBER_BASE + 5;
        internal static readonly int ESCHER_VALIDATOR_INCLUDES_USING = ESCHER_VALIDATOR_ERROR_NUMBER_BASE + 6;
        internal static readonly int ESCHER_VALIDATOR_UNMAPPED_ENTITY_TYPE = ESCHER_VALIDATOR_ERROR_NUMBER_BASE + 7;
        internal static readonly int ESCHER_VALIDATOR_UNMAPPED_ASSOCIATION = ESCHER_VALIDATOR_ERROR_NUMBER_BASE + 8;
        internal static readonly int ESCHER_VALIDATOR_UNMAPPED_PROPERTY = ESCHER_VALIDATOR_ERROR_NUMBER_BASE + 9;
        internal static readonly int ESCHER_VALIDATOR_UNMAPPED_ASSOCIATION_END = ESCHER_VALIDATOR_ERROR_NUMBER_BASE + 10;
        internal static readonly int ESCHER_VALIDATOR_UNMAPPED_ASSOCIATION_END_KEY = ESCHER_VALIDATOR_ERROR_NUMBER_BASE + 11;
        internal static readonly int ESCHER_VALIDATOR_CONDITION_ON_PRIMARY_KEY = ESCHER_VALIDATOR_ERROR_NUMBER_BASE + 12;
        internal static readonly int ESCHER_VALIDATOR_CIRCULAR_COMPLEX_TYPE_DEFINITION = ESCHER_VALIDATOR_ERROR_NUMBER_BASE + 13;
        internal static readonly int ESCHER_VALIDATOR_UNDEFINED_COMPLEX_PROPERTY_TYPE = ESCHER_VALIDATOR_ERROR_NUMBER_BASE + 14;
        internal static readonly int ESCHER_VALIDATOR_ENUM_PROPERTY_WITH_STOREGENERATEDPATTERN = ESCHER_VALIDATOR_ERROR_NUMBER_BASE + 15;

        internal static readonly int RUNTIME_VALIDATION_EXCEPTION_BASE = 12000;
        internal static readonly int RUNTIME_VALIDATOR_EXCEPTION_OCCURRED_DURING_RUNTIME_VALIDATION = RUNTIME_VALIDATION_EXCEPTION_BASE + 5;

        internal static readonly int RUNTIME_VALIDATOR_UNABLE_TO_LOAD_STORE_ITEM_COLLECTION_FROM_PROVIDER_FACTORY =
            RUNTIME_VALIDATION_EXCEPTION_BASE + 6;

        internal static readonly int UPDATE_MODEL_FROM_DB_BASE = 13000;
        internal static readonly int UPDATE_MODEL_FROM_DB_CANT_INCLUDE_REF_CONSTRAINT = UPDATE_MODEL_FROM_DB_BASE + 1;

        internal static readonly int GenerateModelFromDbBase = 13100;
        internal static readonly int GenerateModelFromDbReverseEngineerStoreModelFailed = GenerateModelFromDbBase + 1;
        internal static readonly int GenerateModelFromDbInvalidConceptualModel = GenerateModelFromDbBase + 2;
    }
}
