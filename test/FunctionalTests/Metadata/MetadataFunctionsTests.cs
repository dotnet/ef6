// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Metadata
{
    using System;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using Xunit;

    public class MetadataFunctionsTests : FunctionalTestBase
    {
        [Fact]
        public void Exception_thrown_when_loading_ssdl_containing_functions_with_duplicate_overloads()
        {
            var ssdl =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Schema Provider=""System.Data.SqlClient"" ProviderManifestToken=""2005"" xmlns=""http://schemas.microsoft.com/ado/2006/04/edm/ssdl"" Namespace=""Dbo"" Alias=""Self"">
  <Function Name=""MyFunc"" ReturnType=""int"" BuiltIn=""true"">
    <Parameter Name=""arg1"" Type=""nchar"" Mode=""In"" />
  </Function>
  <Function Name=""MyFunc"" ReturnType=""int"" BuiltIn=""true"">
    <Parameter Name=""arg1"" Type=""varchar"" Mode=""In"" />
  </Function>
</Schema>";

            var exception = Assert.Throws<MetadataException>(() => new StoreItemCollection(new[] { XmlReader.Create(new StringReader(ssdl)) }));
            exception.ValidateMessage(
                "DuplicatedFunctionoverloads",
                false,
                "Dbo.MyFunc",
                "(In Edm.String(Nullable=True,DefaultValue=,MaxLength=,Unicode=,FixedLength=))");
        }

        [Fact]
        public void Exception_thrown_when_loading_ssdl_with_out_of_range_facet_value()
        {
            var ssdl =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Schema Provider=""System.Data.SqlClient"" ProviderManifestToken=""2005"" xmlns=""http://schemas.microsoft.com/ado/2006/04/edm/ssdl"" Namespace=""Dbo"" Alias=""Self"">
  <EntityType Name=""Product"">
    <Key>
      <PropertyRef Name=""Id"" />
    </Key>
    <Property Name=""Id"" Type=""Int32"" Nullable=""false"" />
    <Property Name=""MyProperty"" Type=""String"" MaxLength=""11111111111111111111111111111111111111111111111111111111111111111111111111111111"" />
  </EntityType>
</Schema>";

            var exceptionMessage = Assert.Throws<MetadataException>(() => new StoreItemCollection(new[] { XmlReader.Create(new StringReader(ssdl)) })).Message;

            Assert.True(exceptionMessage.Contains("MaxLength"));
            Assert.True(exceptionMessage.Contains("11111111111111111111111111111111111111111111111111111111111111111111111111111111"));
            Assert.True(exceptionMessage.Contains("http://schemas.microsoft.com/ado/2006/04/edm/ssdl:TMaxLengthFacet"));
        }

        [Fact]
        public void Exception_thrown_when_loading_csdl_with_function_having_the_same_name_as_type_in_the_model()
        {
            var functionDefinition =
@"<Function Name=""Person"" ReturnType=""Self.Person"">
    <Parameter Name=""Param1"" Type=""Edm.Int32"" />
    <DefiningExpression>esql expression</DefiningExpression>
  </Function>";

            MetadataFunctionHelper(
                functionDefinition,
                e => e.ValidateMessage("AmbiguousFunctionAndType", false, "Entities.Person", "Conceptual"));
        }

        [Fact]
        public void Exception_thrown_when_loading_csdl_with_function_with_facet_set_on_non_scalar_return_type()
        {
            var functionDefinition =
@"<Function Name=""MyFunction"" ReturnType=""Self.Person"" Nullable=""true"">
    <Parameter Name=""Param1"" Type=""Edm.Int32"" />
    <DefiningExpression>esql expression</DefiningExpression>
  </Function>";

            MetadataFunctionHelper(
                functionDefinition, 
                e => e.ValidateMessage("FacetsOnNonScalarType", false, "Entities.Person"));
        }

        [Fact]
        public void Exception_thrown_when_loading_csdl_with_function_with_facet_set_on_non_scalar_type()
        {
            var functionDefinition =
@"<Function Name=""MyFunction"" ReturnType=""Edm.Int32"">
    <Parameter Name=""Param1"" Type=""Self.Person"" Nullable=""true"" />
    <DefiningExpression>esql expression</DefiningExpression>
  </Function>";

            MetadataFunctionHelper(
                functionDefinition, 
                e => e.ValidateMessage("FacetsOnNonScalarType", false, "Entities.Person"));
        }

        [Fact]
        public void Exception_thrown_when_loading_csdl_with_function_without_return_type()
        {
            var functionDefinition =
@"<Function Name=""MyFunction"" Nullable=""true"">
    <Parameter Name=""Param1"" Type=""Edm.Int32"" />
    <DefiningExpression>esql expression</DefiningExpression>
  </Function>";

            MetadataFunctionHelper(
                functionDefinition, 
                e =>
                {
                    e.ValidateMessage("ComposableFunctionOrFunctionImportMustDeclareReturnType", false);
                    e.ValidateMessage("FacetDeclarationRequiresTypeAttribute", false);
                });
        }

        [Fact]
        public void Exception_thrown_when_loading_csdl_with_function_with_parameter_without_a_return_type()
        {
            var functionDefinition =
@"<Function Name=""MyFunction"" ReturnType=""Edm.Int32"">
    <Parameter Name=""Param1"" Nullable=""true"" />
    <DefiningExpression>esql expression</DefiningExpression>
  </Function>";

            MetadataFunctionHelper(
                functionDefinition, 
                e =>
                {
                    e.ValidateMessage("TypeMustBeDeclared", false);
                    e.ValidateMessage("FacetDeclarationRequiresTypeAttribute", false);
                });
        }

        [Fact]
        public void Exception_thrown_when_loading_csdl_with_function_with_return_type_without_type_declaration()
        {
            var functionDefinition =
@"<Function Name=""MyFunction"">
    <ReturnType Nullable=""true"" >
    </ReturnType>
    <DefiningExpression>ABC</DefiningExpression>
  </Function>";

            MetadataFunctionHelper(
                functionDefinition,
                e =>
                {
                    e.ValidateMessage("TypeMustBeDeclared", false);
                    e.ValidateMessage("FacetDeclarationRequiresTypeAttribute", false);
                });
        }

        [Fact]
        public void Exception_thrown_when_loading_csdl_with_function_with_facet_set_on_collection_of_entities_type()
        {
            var functionDefinition =
@"<Function Name=""MyFunction"">
    <ReturnType>
      <CollectionType ElementType=""Self.Person"" Nullable=""false"" />
    </ReturnType>
    <DefiningExpression>ABC</DefiningExpression>
  </Function>";

            MetadataFunctionHelper(
                functionDefinition, 
                e => e.ValidateMessage("FacetsOnNonScalarType", false, "Entities.Person"));
        }

        [Fact]
        public void Exception_thrown_when_loading_csdl_with_function_with_return_type_being_a_collection_of_unspecified_type()
        {
            var functionDefinition =
@"<Function Name=""MyFunction"">
    <ReturnType>
      <CollectionType Nullable=""false"" />
    </ReturnType>
    <DefiningExpression>ABC</DefiningExpression>
  </Function>";

            MetadataFunctionHelper(
                functionDefinition,
                e =>
                {
                    e.ValidateMessage("TypeMustBeDeclared", false);
                    e.ValidateMessage("FacetDeclarationRequiresTypeAttribute", false);
                });
        }

        [Fact]
        public void Exception_thrown_when_loading_csdl_with_function_with_return_type_being_a_collection_row_of_entity_with_facet_specified()
        {
            var functionDefinition =
@"<Function Name=""MyFunction"">
    <ReturnType>
      <CollectionType>
        <RowType>
          <Property Name=""AProperty"" Type=""Self.Person"" Nullable=""true"" />
        </RowType>
      </CollectionType>
    </ReturnType>
    <DefiningExpression>ABC</DefiningExpression>
  </Function>";

            MetadataFunctionHelper(
                functionDefinition,
                e => e.ValidateMessage("FacetsOnNonScalarType", false, "Entities.Person"));
        }

        [Fact]
        public void Exception_thrown_when_loading_csdl_with_function_with_return_type_being_a_collection_row_of_unspecified_type()
        {
            var functionDefinition =
@"<Function Name=""MyFunction"">
    <ReturnType>
      <CollectionType>
        <RowType>
          <Property Name=""AProperty"" Nullable=""true"" />
        </RowType>
      </CollectionType>
    </ReturnType>
    <DefiningExpression>ABC</DefiningExpression>
  </Function>";

            MetadataFunctionHelper(
                functionDefinition,
                e =>
                {
                    e.ValidateMessage("TypeMustBeDeclared", false);
                    e.ValidateMessage("FacetDeclarationRequiresTypeAttribute", false);
                });
        }

        [Fact]
        public void Exception_thrown_when_loading_csdl_with_function_with_return_type_declared_boh_in_attribute_and_element()
        {
            var functionDefinition =
@"<Function Name=""MyFunction"" ReturnType=""Self.Person"">
    <ReturnType>
      <CollectionType ElementType=""Self.Person"" />
    </ReturnType>
    <DefiningExpression>ABC</DefiningExpression>
  </Function>";

            MetadataFunctionHelper(
                functionDefinition, 
                e => e.ValidateMessage("TypeDeclaredAsAttributeAndElement", false));
        }

        [Fact]
        public void Exception_thrown_when_loading_csdl_with_function_with_return_type_being_row_without_properties()
        {
            var functionDefinition =
@"<Function Name=""MyFunction"" ReturnType=""Edm.Int32"">
    <Parameter Name=""Param1"">
      <RowType/>
    </Parameter>
    <DefiningExpression>esql expression</DefiningExpression>
  </Function>";

            MetadataFunctionHelper(
                functionDefinition, 
                e => e.ValidateMessage("RowTypeWithoutProperty", false));
        }

        [Fact]
        public void Exception_thrown_when_loading_csdl_with_function_with_return_type_being_reference_to_non_entity_type()
        {
            var functionDefinition =
@"<Function Name=""MyFunction"" ReturnType=""Ref(Edm.Int32)"">
    <Parameter Name=""Person"" >
      <ReferenceType Type=""Edm.Int32"" />
    </Parameter>
    <DefiningExpression>esql expression</DefiningExpression>
  </Function>";

            MetadataFunctionHelper(
                functionDefinition, 
                e => e.ValidateMessage("ReferenceToNonEntityType", false, "Edm.Int32"));
        }

        [Fact]
        public void Exception_thrown_when_loading_csdl_with_function_with_parameter_being_type_being_reference_to_non_entity_type()
        {
            var functionDefinition =
@"<Function Name=""MyFunction"" ReturnType=""Edm.Int32"">
    <Parameter Name=""Param1"">
      <RowType>
        <Property Name=""AProperty"" Type=""Ref(Edm.Int32)""/>
      </RowType>
    </Parameter>
    <DefiningExpression>esql expression</DefiningExpression>
  </Function>";

            MetadataFunctionHelper(
                functionDefinition, 
                e => e.ValidateMessage("ReferenceToNonEntityType", false, "Edm.Int32"));
        }

        [Fact]
        public void Exception_thrown_when_loading_csdl_with_ambiguous_functions()
        {
            var functionDefinition =
@"<Function Name=""MyFunction"" ReturnType=""String"">
    <Parameter Name=""p"">
      <RowType>
        <Property Name=""a"" Type=""Int32""/>
        <Property Name=""b"" Type=""Int32""/>
      </RowType>
    </Parameter>
    <DefiningExpression>
      'abc'
    </DefiningExpression>
  </Function>
  <Function Name=""MyFunction"" ReturnType=""String"">
    <Parameter Name=""p"">
      <RowType>
        <Property Name=""b"" Type=""Int32""/>
        <Property Name=""a"" Type=""Int32""/>
      </RowType>
    </Parameter>
    <DefiningExpression>
      'abc'
    </DefiningExpression>
  </Function>";

            MetadataFunctionHelper(
                functionDefinition, 
                e => e.ValidateMessage("AmbiguousFunctionOverload", false, "Entities.MyFunction", "Conceptual"));
        }

        [Fact]
        public void Exception_thrown_when_loading_csdl_with_function_with_two_arguments_that_are_the_same()
        {
            var functionDefinition =
@"<Function Name=""MyFunction"" ReturnType=""Self.Person"">
    <Parameter Name=""Param1"" Type=""Edm.Int32"" />
    <Parameter Name=""Param1"" Type=""Edm.String"" />
    <DefiningExpression>esql expression</DefiningExpression>
  </Function>";

            MetadataFunctionHelper(
                functionDefinition, 
                e => e.ValidateMessage("ParameterNameAlreadyDefinedDuplicate", false, "Param1"));
        }

        [Fact]
        public void Exception_thrown_when_loading_csdl_with_function_with_argument_being_a_row_with_two_properties_with_the_same_name()
        {
            var functionDefinition =
@"<Function Name=""MyFunction"" ReturnType=""Edm.Int32"">
    <Parameter Name=""Param1"">
      <RowType>
        <Property Name=""AProperty"" Type=""Edm.Int32""/>
        <Property Name=""AProperty"" Type=""Edm.String""/>
      </RowType>
    </Parameter>
    <DefiningExpression>esql expression</DefiningExpression>
  </Function>";

            MetadataFunctionHelper(
                functionDefinition, 
                e => e.ValidateMessage("EdmModel_Validator_Semantic_DuplicateEntityContainerMemberName", false, "AProperty"));
        }

        [Fact]
        public void Exception_thrown_when_loading_csdl_with_function_with_undefined_return_type_and_parameter_type()
        {
            var functionDefinition =
@" <Function Name=""MyFunction"" ReturnType=""Self.Undefined"">
    <Parameter Name=""Param1"" Type=""Edm.Undefined"" />
    <DefiningExpression>esql expression</DefiningExpression>
  </Function>";

            MetadataFunctionHelper(
                functionDefinition,
                e =>
                {
                    e.ValidateMessage("NotInNamespaceAlias", false, "Undefined", "Entities", "Self");
                    e.ValidateMessage("NotInNamespaceNoAlias", false, "Undefined", "Edm");
                });
        }

        [Fact]
        public void Exception_thrown_when_loading_csdl_with_function_with_reference_to_undefined_type_as_return_type_and_parameter_type()
        {
            var functionDefinition =
@" <Function Name=""MyFunction"" ReturnType=""Edm.Int32"">
    <Parameter Name=""Param1"">
      <RowType>
        <Property Name=""Prop1"">
          <CollectionType ElementType=""Self.Undefined"" Nullable=""false"" />
        </Property>
      </RowType>
    </Parameter>
    <DefiningExpression>esql expression</DefiningExpression>
  </Function>";

            MetadataFunctionHelper(
                functionDefinition, 
                e => e.ValidateMessage("NotInNamespaceAlias", false, "Undefined", "Entities", "Self"));
        }

        private void MetadataFunctionHelper(string functionDefinition, Action<Exception> verificationAction)
        {
            var csdl = string.Format(
                CultureInfo.InvariantCulture, 
@"<?xml version=""1.0"" encoding=""utf-16""?>
<Schema Alias=""Self"" Namespace=""Entities"" xmlns=""http://schemas.microsoft.com/ado/2008/09/edm"">
  <EntityType Name=""Person"">
    <Key>
      <PropertyRef Name=""Id"" />
    </Key>
    <Property Name=""Id"" Type=""Int32"" Nullable=""false"" />
  </EntityType>
  {0}
</Schema>", functionDefinition);

            var exception = Assert.Throws<MetadataException>(() => new EdmItemCollection(new[] { XmlReader.Create(new StringReader(csdl)) }));
            verificationAction(exception);
        }
    }
}
