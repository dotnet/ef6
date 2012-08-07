// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Parsing.Xml.Internal.Msl
{
    /// <summary>
    ///     Constants for C-S MSL XML.
    /// </summary>
    internal static class MslConstants
    {
        internal const string FileExtension = ".msl";

        // v3.5 of .net framework
        internal const double Version1 = 1.0;
        internal const string Version1Namespace = "urn:schemas-microsoft-com:windows:storage:mapping:CS";
        internal const string Version1Xsd = "System.Data.Resources.CSMSL_1.xsd";

        // v4 of .net framework
        internal const double Version2 = 2.0;
        internal const string Version2Namespace = "http://schemas.microsoft.com/ado/2008/09/mapping/cs";
        internal const string Version2Xsd = "System.Data.Resources.CSMSL_2.xsd";

        // v4 next of .net framework
        internal const double Version3 = 3.0;
        internal const string Version3Namespace = "http://schemas.microsoft.com/ado/2009/11/mapping/cs";
        internal const string Version3Xsd = "System.Data.Resources.CSMSL_3.xsd";

        internal const double VersionLatest = Version3;

        internal const string Element_Mapping = "Mapping";
        internal const string Element_EntityContainerMapping = "EntityContainerMapping";
        internal const string Element_EntitySetMapping = "EntitySetMapping";
        internal const string Element_AssociationSetMapping = "AssociationSetMapping";
        internal const string Element_EndProperty = "EndProperty";
        internal const string Element_EntityTypeMapping = "EntityTypeMapping";
        internal const string Element_QueryView = "QueryView";
        internal const string Element_MappingFragment = "MappingFragment";
        internal const string Element_ScalarProperty = "ScalarProperty";
        internal const string Element_ComplexProperty = "ComplexProperty";
        internal const string Element_Condition = "Condition";

        internal const string Attribute_Space = "Space";
        internal const string Attribute_CDMEntityContainer = "CdmEntityContainer";
        internal const string Attribute_StorageEntityContainer = "StorageEntityContainer";
        internal const string Attribute_Name = "Name";
        internal const string Attribute_TypeName = "TypeName";
        internal const string Attribute_StoreEntitySet = "StoreEntitySet";
        internal const string Attribute_ColumnName = "ColumnName";
        internal const string Attribute_Value = "Value";
        internal const string Attribute_IsNull = "IsNull";

        internal const string Value_IsTypeOf = "IsTypeOf(";
        internal const string Value_IsTypeOfTerminal = ")";
        internal const string Value_Space = "C-S";

        //internal const string AliasElement = "Alias";
        //internal const string AliasKeyAttribute = "Key";
        //internal const string AliasValueAttribute = "Value";
        //internal const string QueryViewElement = "QueryView";
        //internal const string GenerateUpdateViews = "GenerateUpdateViews";
        //internal const string AssociationSetMappingElement = "AssociationSetMapping";
        //internal const string AssociationSetMappingNameAttribute = "Name";
        //internal const string AssociationSetMappingTypeNameAttribute = "TypeName";
        //internal const string AssociationSetMappingStoreEntitySetAttribute = "StoreEntitySet";
        //internal const string EndPropertyMappingElement = "EndProperty";
        //internal const string EndPropertyMappingNameAttribute = "Name";
        //internal const string CompositionSetMappingNameAttribute = "Name";
        //internal const string CompositionSetMappingTypeNameAttribute = "TypeName";
        //internal const string CompositionSetMappingStoreEntitySetAttribute = "StoreEntitySet";
        //internal const string FunctionImportMappingElement = "FunctionImportMapping";
        //internal const string FunctionImportMappingFunctionNameAttribute = "FunctionName";
        //internal const string FunctionImportMappingFunctionImportNameAttribute = "FunctionImportName";
        //internal const string CompositionSetParentEndName = "Parent";
        //internal const string CompositionSetChildEndName = "Child";
        //internal const string MappingFragmentMakeColumnsDistinctAttribute = "MakeColumnsDistinct";
        //internal const string ComplexPropertyElement = "ComplexProperty";
        //internal const string AssociationEndElement = "AssociationEnd";
        //internal const string ComplexPropertyNameAttribute = "Name";
        //internal const string ComplexPropertyTypeNameAttribute = "TypeName";
        //internal const string ComplexPropertyIsPartialAttribute = "IsPartial";
        //internal const string ComplexTypeMappingElement = "ComplexTypeMapping";
        //internal const string ComplexTypeMappingTypeNameAttribute = "TypeName";
        //internal const string CollectionPropertyNameAttribute = "Name";
        //internal const string CollectionPropertyIsPartialAttribute = "IsPartial";
        //internal const string IsTypeOfOnly = "IsTypeOfOnly(";
        //internal const string IsTypeOfOnlyTerminal = ")";
        //internal const string ModificationFunctionMappingElement = "ModificationFunctionMapping";
        //internal const string DeleteFunctionElement = "DeleteFunction";
        //internal const string InsertFunctionElement = "InsertFunction";
        //internal const string UpdateFunctionElement = "UpdateFunction";
        //internal const string FunctionNameAttribute = "FunctionName";
        //internal const string RowsAffectedParameterAttribute = "RowsAffectedParameter";
        //internal const string ParameterNameAttribute = "ParameterName";
        //internal const string ParameterVersionAttribute = "Version";
        //internal const string ParameterVersionAttributeCurrentValue = "Current";
        //internal const string AssociationSetAttribute = "AssociationSet";
        //internal const string FromAttribute = "From";
        //internal const string ToAttribute = "To";
        //internal const string ResultBindingElement = "ResultBinding";
        //internal const string ResultBindingPropertyNameAttribute = "Name";
        //internal const string ResultBindingColumnNameAttribute = "ColumnName";
        //internal const char TypeNameSperator = ';';
        //internal const char IdentitySeperator = ':';
        //internal const string EntityViewGenerationTypeName = "Edm_EntityMappingGeneratedViews.ViewsForBaseEntitySets";
        //internal const string FunctionImportMappingResultMapping = "ResultMapping";
    }
}
