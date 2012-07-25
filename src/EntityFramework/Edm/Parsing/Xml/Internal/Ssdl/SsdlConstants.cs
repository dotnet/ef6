// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Edm.Parsing.Xml.Internal.Ssdl
{
    /// <summary>
    ///     Constants for SSDL XML.
    /// </summary>
    internal static class SsdlConstants
    {
        internal const string FileExtension = ".ssdl";

        internal const double VersionUndefined = 0.0;

        // v3.5 of .net framework
        internal const double Version1 = 1.0;
        internal const string Version1Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl";
        internal const string Version1Xsd = "System.Data.Resources.SSDLSchema.xsd";

        //// v4 of .net framework
        internal const double Version2 = 2.0;
        internal const string Version2Namespace = "http://schemas.microsoft.com/ado/2009/02/edm/ssdl";
        internal const string Version2Xsd = "System.Data.Resources.SSDLSchema_2.xsd";

        //// v4 next of .net framework
        internal const double Version3 = 3.0;
        internal const string Version3Namespace = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl";
        internal const string Version3Xsd = "System.Data.Resources.SSDLSchema_3.xsd";

        internal const double VersionLatest = Version3;

        // Const node names in the CDM schema xml
        internal const string Element_Association = "Association";
        internal const string Element_AssociationSet = "AssociationSet";
        internal const string Element_ComplexType = "ComplexType";
        internal const string Element_DefiningQuery = "DefiningQuery";
        internal const string Element_DefiningExpression = "DefiningExpression";
        internal const string Element_Documentation = "Documentation";
        internal const string Element_DependentRole = "Dependent";
        internal const string Element_End = "End";
        internal const string Element_EntityType = "EntityType";
        internal const string Element_EntityContainer = "EntityContainer";
        internal const string Element_FunctionImport = "FunctionImport";
        internal const string Element_Key = "Key";
        internal const string Element_NavigationProperty = "NavigationProperty";
        internal const string Element_OnDelete = "OnDelete";
        internal const string Element_PrincipalRole = "Principal";
        internal const string Element_Property = "Property";
        internal const string Element_PropertyRef = "PropertyRef";
        internal const string Element_ReferentialConstraint = "ReferentialConstraint";
        internal const string Element_Role = "Role";
        internal const string Element_Schema = "Schema";
        internal const string Element_Summary = "Summary";
        internal const string Element_LongDescription = "LongDescription";
        internal const string Element_SampleValue = "SampleValue";
        internal const string Element_EntitySet = "EntitySet";

        internal const string Element_Using = "Using";

        //// constants used for codegen hints
        //internal const string TypeAccess = "TypeAccess";
        //internal const string MethodAccess = "MethodAccess";
        //internal const string SetterAccess = "SetterAccess";
        //internal const string GetterAccess = "GetterAccess";

        //// const attribute names in the CDM schema XML
        internal const string Attribute_Provider = "Provider";
        internal const string Attribute_ProviderManifestToken = "ProviderManifestToken";
        internal const string Attribute_EntityType = "EntityType";
        internal const string Attribute_Schema = "Schema";
        internal const string Attribute_Association = "Association";
        internal const string Attribute_Abstract = "Abstract";
        internal const string Attribute_Action = "Action";
        internal const string Attribute_BaseType = "BaseType";
        internal const string Attribute_EntitySet = "EntitySet";
        internal const string Attribute_Extends = "Extends";
        internal const string Attribute_FromRole = "FromRole";
        internal const string Attribute_Multiplicity = "Multiplicity";
        internal const string Attribute_Name = "Name";
        internal const string Attribute_Namespace = "Namespace";
        internal const string Attribute_Table = "Table";
        internal const string Attribute_Role = "Role";
        internal const string Attribute_ToRole = "ToRole";
        internal const string Attribute_Relationship = "Relationship";
        internal const string Attribute_ElementType = "ElementType";
        internal const string Attribute_StoreGeneratedPattern = "StoreGeneratedPattern";
        internal const string Attribute_Type = "Type";
        internal const string Attribute_Alias = "Alias";
        // facet values
        internal const string Attribute_Scale = "Scale";
        internal const string Attribute_Precision = "Precision";
        internal const string Attribute_MaxLength = "MaxLength";
        internal const string Attribute_Unicode = "Unicode";
        internal const string Attribute_FixedLength = "FixedLength";
        internal const string Attribute_Nullable = "Nullable";
        internal const string Attribute_Srid = "Srid";
        internal const string Attribute_IsStrict = "IsStrict";

        // Value
        internal const string Value_Max = "Max";
        internal const string Value_Identity = "Identity";
        internal const string Value_Computed = "Computed";
        internal const string Value_Fixed = "Fixed";
        internal const string Value_Self = "Self";

        //// const attribute values in the CDM schema xml
        //internal const string True = "true";

        //// xml constants used in provider manifest
        //internal const string Function = "Function";
        //internal const string ReturnType = "ReturnType";
        //internal const string Parameter = "Parameter";
        //internal const string Mode = "Mode";
        //internal const string StoreFunctionName = "StoreFunctionName";

        //internal const string ProviderManifestElement = "ProviderManifest";
        //internal const string TypesElement = "Types";
        //internal const string FunctionsElement = "Functions";
        //internal const string TypeElement = "Type";
        //internal const string FunctionElement = "Function";
        //internal const string ScaleElement = "Scale";
        //internal const string PrecisionElement = "Precision";
        //internal const string MaxLengthElement = "MaxLength";
        //internal const string FacetDescriptionsElement = "FacetDescriptions";
        //internal const string UnicodeElement = "Unicode";
        //internal const string FixedLengthElement = "FixedLength";
        //internal const string ReturnTypeElement = "ReturnType";
        //internal const string TypeAttribute = "Type";

        //internal const string MinimumAttribute = "Minimum";
        //internal const string MaximumAttribute = "Maximum";
        //internal const string NamespaceAttribute = "Namespace";
        //internal const string DefaultValueAttribute = "DefaultValue";
        //internal const string ConstantAttribute = "Constant";
        //internal const string DestinationTypeAttribute = "DestinationType";
        //internal const string PrimitiveTypeKindAttribute = "PrimitiveTypeKind";
        //internal const string AggregateAttribute = "Aggregate";
        //internal const string BuiltInAttribute = "BuiltIn";
        //internal const string NameAttribute = "Name";
        //internal const string IgnoreFacetsAttribute = "IgnoreFacets";
        //internal const string NiladicFunction = "NiladicFunction";
        //internal const string IsComposable = "IsComposable";
        //internal const string CommandText = "CommandText";
        //internal const string ParameterTypeSemantics = "ParameterTypeSemantics";
        //internal const string CollectionType = "CollectionType";
        //internal const string ReferenceType = "ReferenceType";
        //internal const string RowType = "RowType";
        //internal const string TypeRef = "TypeRef";

        //internal const string XmlCommentStartString = "<!--";
        //internal const string XmlCommentEndString = "-->";

        //#endregion // CDM Schema Xml NodeNames
    }
}
