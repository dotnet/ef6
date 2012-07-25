// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Edm.Parsing.Xml.Internal.Csdl
{
    /// <summary>
    ///     Constants for CSDL XML.
    /// </summary>
    internal static class CsdlConstants
    {
        internal const string FileExtension = ".csdl";

        internal const double VersionUndefined = 0.0;

        // v3.5 of .net framework
        internal const double Version1 = 1.0;
        internal const string Version1Namespace = "http://schemas.microsoft.com/ado/2006/04/edm";
        internal const string Version1Xsd = "System.Data.Resources.CSDLSchema_1.xsd";

        internal const double Version1_1 = 1.1;
        internal const string Version1_1Namespace = "http://schemas.microsoft.com/ado/2007/05/edm";
        internal const string Version1_1Xsd = "System.Data.Resources.CSDLSchema_1_1.xsd";

        // v4 of .net framework
        internal const double Version2 = 2.0;
        internal const string Version2Namespace = "http://schemas.microsoft.com/ado/2008/09/edm";
        internal const string Version2Xsd = "System.Data.Resources.CSDLSchema_2.xsd";

        // v4 next of .net framework
        internal const double Version3 = 3.0;
        internal const string Version3Namespace = "http://schemas.microsoft.com/ado/2009/11/edm";
        internal const string Version3Xsd = "System.Data.Resources.CSDLSchema_3.xsd";

        internal const double VersionLatest = Version3;

        internal const string AnnotationNamespace = "http://schemas.microsoft.com/ado/2009/02/edm/annotation";

        internal const string Attribute_Abstract = "Abstract";
        internal const string Attribute_Action = "Action";
        internal const string Attribute_Alias = "Alias";
        internal const string Attribute_Association = "Association";
        internal const string Attribute_BaseType = "BaseType";
        internal const string Attribute_CollectionKind = "CollectionKind";
        internal const string Attribute_ConcurrencyMode = "ConcurrencyMode";
        internal const string Attribute_DefaultValue = "DefaultValue";
        internal const string Attribute_FromRole = "FromRole";
        internal const string Attribute_EntityType = "EntityType";
        internal const string Attribute_EntitySet = "EntitySet";
        internal const string Attribute_FixedLength = "FixedLength";
        internal const string Attribute_IsFlags = "IsFlags";
        internal const string Attribute_UnderlyingType = "UnderlyingType";
        internal const string Attribute_MaxLength = "MaxLength";
        internal const string Attribute_Multiplicity = "Multiplicity";
        internal const string Attribute_Name = "Name";
        internal const string Attribute_Namespace = "Namespace";
        internal const string Attribute_Nullable = "Nullable";
        internal const string Attribute_Precision = "Precision";
        internal const string Attribute_Relationship = "Relationship";
        internal const string Attribute_Role = "Role";
        internal const string Attribute_Scale = "Scale";
        internal const string Attribute_ToRole = "ToRole";
        internal const string Attribute_Type = "Type";
        internal const string Attribute_Unicode = "Unicode";
        internal const string Attribute_ReturnType = "ReturnType";
        internal const string Attribute_ResultEnd = "ResultEnd";
        internal const string Attribute_Value = "Value";
        internal const string Attribute_UseStrongSpatialTypes = "UseStrongSpatialTypes";

        internal const string Element_Association = "Association";
        internal const string Element_AssociationSet = "AssociationSet";
        internal const string Element_ComplexType = "ComplexType";
        internal const string Element_Dependent = "Dependent";
        internal const string Element_Documentation = "Documentation";
        internal const string Element_End = "End";
        internal const string Element_EntityContainer = "EntityContainer";
        internal const string Element_EntitySet = "EntitySet";
        internal const string Element_EntityType = "EntityType";
        internal const string Element_EnumType = "EnumType";
        internal const string Element_EnumTypeMember = "Member";
        internal const string Element_Key = "Key";
        internal const string Element_LongDescription = "LongDescription";
        internal const string Element_NavigationProperty = "NavigationProperty";
        internal const string Element_OnDelete = "OnDelete";
        internal const string Element_Principal = "Principal";
        internal const string Element_Property = "Property";
        internal const string Element_PropertyRef = "PropertyRef";
        internal const string Element_ReferentialConstraint = "ReferentialConstraint";
        internal const string Element_Schema = "Schema";
        internal const string Element_Summary = "Summary";
        internal const string Element_Using = "Using";

        internal const string Property_ElementType = "ElementType";
        internal const string Property_TargetSet = "TargetSet";
        internal const string Property_SourceSet = "SourceSet";

        internal const string Value_Bag = "Bag";
        internal const string Value_EndMany = "*";
        internal const string Value_EndOptional = "0..1";
        internal const string Value_EndRequired = "1";
        internal const string Value_Fixed = "Fixed";
        internal const string Value_List = "List";
        internal const string Value_Max = "Max";
        internal const string Value_None = "None";
        internal const string Value_Self = "Self";
        internal const string Value_True = "true";
        internal const string Value_False = "false";
        internal const string Value_Computed = "Computed";
        internal const string Value_Identity = "Identity";

        internal const bool Default_Abstract = false;
        internal const bool Default_Nullable = true;

        // The following constants will be used when Function/FunctionImport/CollectionKind support is added:

        //// Const node names in the CDM schema xml
        //internal const string DefiningExpression = "DefiningExpression";
        //internal const string FunctionImport = "FunctionImport";
        //internal const string SampleValue = "SampleValue";

        //// constants used for codegen hints
        //internal const string TypeAccess = "TypeAccess";
        //internal const string MethodAccess = "MethodAccess";
        //internal const string SetterAccess = "SetterAccess";
        //internal const string GetterAccess = "GetterAccess";

        //// const attribute names in the CDM schema XML
        //internal const string Extends = "Extends";
        //internal const string Table = "Table";
        //internal const string ElementType = "ElementType";

        //// facet values
        //internal const string In = "In";
        //internal const string Out = "Out";
        //internal const string InOut = "InOut";
    }
}
