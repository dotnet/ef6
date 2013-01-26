// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using Xunit;
    using Xunit.Sdk;

    public class MetadataEnumTests : FunctionalTestBase, IUseFixture<MetadataEnumFixture>
    {
        private const string enumCsdl =
@"<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""EnumTestModel"">
  <EntityContainer Name=""EnumTestContainer"">
    <EntitySet Name=""EnumTestSet"" EntityType=""EnumTestModel.EnumTestEntity"" />
  </EntityContainer>
  <EntityType Name=""EnumTestEntity"">
    <Key>
      <PropertyRef Name=""Id"" />
    </Key>
    <Property Name=""Id"" Nullable=""false"" Type=""Int32"" />
    <Property Name=""Color"" Type=""EnumTestModel.Color"" Nullable=""false""/>
  </EntityType>
  <EnumType Name=""Color"" IsFlags=""false"">
    <Member Name=""Yellow"" Value=""1"" />
    <Member Name=""Green"" Value=""2"" />
    <Member Name=""Blue"" Value=""3""/>
  </EnumType>
</Schema>";

        private const string edmNamespace = "http://schemas.microsoft.com/ado/2009/11/edm";

        private XmlSchemaSet csdlSchemaSet;

        public void SetFixture(MetadataEnumFixture data)
        {
            csdlSchemaSet = data.CsdlSchemaSet;
        }

        [Fact]
        public void Empty_enum_is_valid()
        {
            var emptyEnum =
@"<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""EnumModel"">
  <EnumType Name=""Color"" IsFlags=""false"">
  </EnumType>
</Schema>";

            var edmItemCollection = new EdmItemCollection(new[] { XmlReader.Create(new StringReader(emptyEnum)) });
            var enumType = edmItemCollection.GetItems<EnumType>().Single();

            Assert.Equal(0, enumType.Members.Count);
        }

        [Fact]
        public void SchemaSource_metadata_property_is_populated()
        {
            var edmItemCollection = new EdmItemCollection(new[] { XmlReader.Create(new StringReader(enumCsdl)) });
            var enumType = edmItemCollection.GetItems<EnumType>().Single();
            
            var schemaSource = enumType.MetadataProperties["SchemaSource"];
            Assert.Equal(PropertyKind.System, schemaSource.PropertyKind);
        }

        [Fact]
        public void Documenation_is_populated_correctly_for_enum_type()
        {
            var enumWithDocumentation =
@"<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""EnumModel"">
  <EnumType Name=""Color"" IsFlags=""false"">
    <Documentation>
      <Summary>Documentation - summary</Summary>
      <LongDescription>Documentation - long description</LongDescription>
    </Documentation>
    <Member Name=""Yellow"" Value=""1"" />
  </EnumType>
</Schema>";

            var edmItemCollection = new EdmItemCollection(new[] { XmlReader.Create(new StringReader(enumWithDocumentation)) });
            var enumType = edmItemCollection.GetItems<EnumType>().Single();

            Assert.NotNull(enumType.Documentation);
            Assert.Equal("Documentation - summary", enumType.Documentation.Summary);
            Assert.Equal("Documentation - long description", enumType.Documentation.LongDescription);
        }
        
        [Fact]
        public void Documenation_is_populated_correctly_for_members_of_enum_type()
        {
            var enumWithDocumentation =
@"<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""EnumTestModel"">
  <EnumType Name=""Color"" IsFlags=""false"">
    <Member Name=""Yellow"" Value=""1"">
      <Documentation>
        <Summary>Documentation - summary</Summary>
        <LongDescription>Documentation - long description</LongDescription>
      </Documentation>
    </Member>
  </EnumType>
</Schema>";

            var edmItemCollection = new EdmItemCollection(new[] { XmlReader.Create(new StringReader(enumWithDocumentation)) });
            var enumType = edmItemCollection.GetItems<EnumType>().Single();
            var enumMemberDocumetation = enumType.Members.Single().Documentation;

            Assert.NotNull(enumMemberDocumetation);
            Assert.Equal("Documentation - summary", enumMemberDocumetation.Summary);
            Assert.Equal("Documentation - long description", enumMemberDocumetation.LongDescription);
        }

        [Fact]
        public void EnumType_contains_MetadataProperties()
        {
            var enumCsdl =
@"<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""EnumTestModel"">
  <EnumType Name=""Color"" IsFlags=""false"">
    <Member Name=""Yellow"" Value=""1"" />
    <Member Name=""Green"" Value=""2"" />
    <Member Name=""Blue"" Value=""3""/>
  </EnumType>
</Schema>";

            var edmItemCollection = new EdmItemCollection(new[] { XmlReader.Create(new StringReader(enumCsdl)) });

            var enumType = edmItemCollection.GetItems<EnumType>().Single();

            Assert.Equal("Color", enumType.MetadataProperties.Where(p => p.Name == "Name").Single().Value);
            Assert.False((bool)enumType.MetadataProperties.Where(p => p.Name == "IsFlags").Single().Value);
            Assert.Equal("Edm.Int32", enumType.MetadataProperties.Where(p => p.Name == "UnderlyingType").Single().Value.ToString());

            var membersProperty = ((IEnumerable<EnumMember>)enumType.MetadataProperties.Where(p => p.Name == "Members").Single().Value).ToList();
            
            Assert.Equal(3, membersProperty.Count());
            Assert.Equal("Yellow", membersProperty[0].Name);
            Assert.Equal(1, membersProperty[0].Value);

            Assert.Equal("Green", membersProperty[1].Name);
            Assert.Equal(2, membersProperty[1].Value);

            Assert.Equal("Blue", membersProperty[2].Name);
            Assert.Equal(3, membersProperty[2].Value);
        }

        [Fact]
        public void Clr_types_of_member_values_match_the_UnderlyingType()
        {
            var clrTypeForUndelryingType = new Dictionary<string, Type>() {
                { "Byte", typeof(byte) },
                { "SByte", typeof(sbyte) },
                { "Int16", typeof(short) },
                { "Int32", typeof(int) },
                { "Int64", typeof(long) },
                { "Edm.Byte", typeof(byte) },
                { "Edm.SByte", typeof(sbyte) },
                { "Edm.Int16", typeof(short) },
                { "Edm.Int32", typeof(int) },
                { "Edm.Int64", typeof(long) },
            };

            var enumCSDL = XDocument.Parse(enumCsdl);
            var enumTypeElement = enumCSDL.Descendants(XName.Get("EnumType", edmNamespace)).Single();

            foreach (var kvp in clrTypeForUndelryingType)
            {
                enumTypeElement.SetAttributeValue("UnderlyingType", kvp.Key);
                var edmItemCollection = new EdmItemCollection(new XmlReader[] { enumCSDL.CreateReader() });
                var enumType = edmItemCollection.GetItems<EnumType>().Single();

                Assert.False(enumType.Members.Any(m => m.Value.GetType() != kvp.Value));
            }
        }

        [Fact]
        public void Custom_MetadataProperties_are_populated_for_members_of_enum_type()
        {
            var enumCsdl =
@"<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""EnumTestModel"">
  <EnumType Name=""Color"" IsFlags=""false"">
    <Member Name=""Yellow"" Value=""1"" p3:abc=""xyz"" xmlns:p3=""http://tempuri.org"" />
    <Member Name=""Green"" Value=""2"" />
    <Member Name=""Blue"" Value=""3"" />
  </EnumType>
</Schema>";

            var edmItemCollection = new EdmItemCollection(new[] { XmlReader.Create(new StringReader(enumCsdl)) });
            var enumMember = edmItemCollection.GetItems<EnumType>().Single().Members.First();

            var customProperty = enumMember.MetadataProperties["http://tempuri.org:abc"];

            Assert.Equal("xyz", customProperty.Value);
            Assert.Equal(PropertyKind.Extended, customProperty.PropertyKind);
        }

        [Fact]
        public void Nullable_facet_on_EnumProperty_is_set_correctly()
        {
            foreach (var nullableFacetValue in new bool?[] { null, false, true })
            {
                var csdl = XDocument.Parse(enumCsdl);
                csdl.Descendants(XName.Get("Property", edmNamespace))
                    .Where(p => (string)p.Attribute("Name") == "Color")
                    .Single()
                    .SetAttributeValue("Nullable", nullableFacetValue == null ? null : nullableFacetValue.ToString().ToLower());

                var edmItemCollection = new EdmItemCollection(new XmlReader[] { csdl.CreateReader() });

                Assert.Equal(
                    nullableFacetValue ?? true,
                    edmItemCollection.GetItems<EntityType>().Single().Properties.Single(p => p.Name == "Color").Nullable);
            }
        }

        [Fact]
        public void Enum_values_with_facets_can_be_used_on_Parameter_element()
        {
            foreach (var nullableFacetValue in new bool?[] { null, false, true })
            {
                VerifyFunctionDefinitionWithEnumsValid(XElement.Parse(
@"<Function Name=""TestFunc"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
    <Parameter Name=""Input"" Type=""EnumTestModel.Color"" " + GetNullableFacetAttributeString(nullableFacetValue) + @" />
    <ReturnType Type=""Int32"" />
</Function>"),
                  (edmFunction) =>
                  {
                      Assert.Equal(BuiltInTypeKind.EnumType, edmFunction.Parameters.Single().TypeUsage.EdmType.BuiltInTypeKind);
                      Assert.Equal(
                          nullableFacetValue ?? true,
                          (bool)edmFunction.Parameters.Single().TypeUsage.Facets.Single(f => f.Name == "Nullable").Value);
                  });
            }
        }

        [Fact]
        public void Enum_ReturnTypes_with_facets_can_be_ised_on_Function_element()
        {
            foreach (var nullableFacetValue in new bool?[] { null, false, true })
            {
                VerifyFunctionDefinitionWithEnumsValid(XElement.Parse(
@"<Function Name=""TestFunc"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" ReturnType=""EnumTestModel.Color"" " + GetNullableFacetAttributeString(nullableFacetValue) + " />"),
                    (edmFunction) =>
                    {
                        Assert.Equal(BuiltInTypeKind.EnumType, edmFunction.ReturnParameter.TypeUsage.EdmType.BuiltInTypeKind);
                        Assert.Equal(
                            nullableFacetValue ?? true,
                                (bool)edmFunction.ReturnParameter.TypeUsage.Facets.Single(f => f.Name == "Nullable").Value);
                    });
            }
        }

        [Fact]
        public void Enum_values_with_facets_can_be_used_on_ReturnType_element()
        {
            foreach (var nullableFacetValue in new bool?[] { null, false, true })
            {
                VerifyFunctionDefinitionWithEnumsValid(XElement.Parse(
@"<Function Name=""TestFunc"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
    <ReturnType Type=""EnumTestModel.Color"" " + GetNullableFacetAttributeString(nullableFacetValue) + @" />
</Function>"),
                  (edmFunction) =>
                  {
                      Assert.Equal(BuiltInTypeKind.EnumType, edmFunction.ReturnParameter.TypeUsage.EdmType.BuiltInTypeKind);
                      Assert.Equal(
                          nullableFacetValue ?? true,
                            (bool)edmFunction.ReturnParameter.TypeUsage.Facets.Single(f => f.Name == "Nullable").Value);
                  });
            }
        }

        private static void VerifyFunctionDefinitionWithEnumsValid(XElement funcDefinition, Action<EdmFunction> verifyFunctionAction)
        {
            Assert.True(funcDefinition.Name.LocalName == "Function");
            Assert.True(funcDefinition.DescendantsAndSelf().Any(e => e.Attributes().Any(a => ((string)a).Contains("EnumTestModel.Color"))));

            var enumCSDL = XDocument.Parse(enumCsdl);
            enumCSDL.Root.Add(funcDefinition);

            var edmItemCollection = new EdmItemCollection(new XmlReader[] { enumCSDL.CreateReader() });
            var function = edmItemCollection.GetItems<EdmFunction>().Where(f => f.Name == (string)funcDefinition.Attribute("Name")).Single();

            verifyFunctionAction(function);
        }

        private static string GetNullableFacetAttributeString(bool? nullableFacetValue)
        {
            return
                nullableFacetValue != null ?
                    string.Format(@"Nullable=""{0}""", nullableFacetValue.ToString().ToLower()) :
                    string.Empty;
        }

        [Fact]
        public void Using_invalid_facets_on__EnumProperty_throws()
        {
            RunInvalidEnumTypeFacetTests((csdl, facetName, facetValue) =>
            {
                csdl.Descendants(XName.Get("Property", edmNamespace))
                    .Where(p => (string)p.Attribute("Name") == "Color")
                    .Single().SetAttributeValue(facetName, facetValue);
            });
        }

        [Fact]
        public void Using_invalid_facets_on_EnumFunction_param_throws()
        {
            RunInvalidEnumTypeFacetTests((csdl, facetName, facetValue) =>
            {
                csdl.Root.Add(XElement.Parse(string.Format(
@"<Function Name=""TestFunc"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" ReturnType=""String"">
    <Parameter Name=""Input"" Type=""EnumTestModel.Color"" {0}=""{1}"" />
</Function>", facetName, facetValue)));
            });
        }

        [Fact]
        public void Using_invalid_facets_on_inline_EnumFunction_ReturnType_throws()
        {
            RunInvalidEnumTypeFacetTests((csdl, facetName, facetValue) =>
            {
                csdl.Root.Add(XElement.Parse(string.Format(
                    @"<Function Name=""TestFunc"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" ReturnType=""EnumTestModel.Color"" {0}=""{1}"" />",
                    facetName, facetValue)));
            });
        }

        [Fact]
        public void Using_invalidFacetsOnEnumFunctionReturnTypeThrows()
        {
            RunInvalidEnumTypeFacetTests((csdl, facetName, facetValue) =>
            {
                csdl.Root.Add(XElement.Parse(string.Format(
@"<Function Name=""TestFunc"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
    <ReturnType Type=""EnumTestModel.Color"" {0}=""{1}""  />
</Function>", facetName, facetValue)));
            });
        }

        [Fact]
        public void Using_invalid_facets_on_Collection_of_Enum__for_function_ReturnType_throws()
        {
            RunInvalidEnumTypeFacetTests((csdl, facetName, facetValue) =>
            {
                csdl.Root.Add(XElement.Parse(string.Format(
@"<Function Name=""TestFunc"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
    <ReturnType>
        <CollectionType ElementType=""EnumTestModel.Color"" {0}=""{1}"" />
    </ReturnType>
</Function>", facetName, facetValue)));
            });
        }

        [Fact]
        public void Using_invalid_facets_on_collection_of_Ref_Enum_for_function_ReturnType_throws()
        {
            RunInvalidEnumTypeFacetTests((csdl, facetName, facetValue) =>
            {
                csdl.Root.Add(XElement.Parse(string.Format(
@"<Function Name=""TestFunc"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
    <ReturnType>
        <CollectionType>
            <TypeRef Type=""EnumTestModel.Color"" {0}=""{1}"" />
        </CollectionType>
    </ReturnType>
</Function>", facetName, facetValue)));
            });
        }

        [Fact]
        public void Using_invalid_facets_on_RowType_of_Collection_of_Enum_for_function_ReturnType_throws()
        {
            RunInvalidEnumTypeFacetTests((csdl, facetName, facetValue) =>
            {
                csdl.Root.Add(XElement.Parse(string.Format(
@"<Function Name=""TestFunc"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
    <ReturnType>
        <RowType>
            <Property Name=""EnumProperty"" Type=""EnumTestModel.Color"" {0}=""{1}"" />
        </RowType>
    </ReturnType>
</Function>", facetName, facetValue)));
            });
        }

        private static string[,] invalidEnumPropertyFacets = 
        { 
            { "Collation", "Int32" },
            { "FixedLength", "false" },
            { "MaxLength", "30" },
            { "Precision", "3" },
            { "Scale", "2" },
            { "Unicode", "true" }
        };

        private static void RunInvalidEnumTypeFacetTests(Action<XDocument, string, string> updateCsdl)
        {
            for (int i = 0; i < invalidEnumPropertyFacets.Length >> 1; i++)
            {
                var enumCSDL = XDocument.Parse(enumCsdl);
                updateCsdl(enumCSDL, invalidEnumPropertyFacets[i, 0], invalidEnumPropertyFacets[i, 1]);

                try
                {
                    new EdmItemCollection(new XmlReader[] { enumCSDL.CreateReader() });

                    throw new AssertException("Expecting exception to be thrown during EdmItemCollection construction.");
                }
                catch (MetadataException ex)
                {
                    Assert.True(ex.Message.Contains(invalidEnumPropertyFacets[i, 0] + " facet isn't allowed for properties of type EnumTestModel.Color"), "Unexpected exception\n: " + ex);
                }
            }
        }

        [Fact]
        public void Using_invalid_facets_on_FunctionImport_enum_parameter_throws()
        {
            string[,] funcImportInvalidEnumParamFacets = new string[8, 2];
            Array.Copy(invalidEnumPropertyFacets, funcImportInvalidEnumParamFacets, invalidEnumPropertyFacets.Length);

            funcImportInvalidEnumParamFacets[6, 0] = "Nullable";
            funcImportInvalidEnumParamFacets[6, 1] = "false";
            funcImportInvalidEnumParamFacets[7, 0] = "DefaultValue";
            funcImportInvalidEnumParamFacets[7, 1] = "34";

            for (int i = 0; i < funcImportInvalidEnumParamFacets.Length >> 1; i++)
            {
                var enumCSDL = XDocument.Parse(enumCsdl);
                enumCSDL.Root
                    .Descendants(XName.Get("EntityContainer", edmNamespace))
                    .Single()
                    .Add(XElement.Parse(string.Format(
@"<FunctionImport Name=""TestFunc"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
    <Parameter Name=""EnumProperty"" Type=""EnumTestModel.Color"" {0}=""{1}"" />
</FunctionImport>", funcImportInvalidEnumParamFacets[i, 0], funcImportInvalidEnumParamFacets[i, 1])));

                try
                {
                    new EdmItemCollection(new XmlReader[] { enumCSDL.CreateReader() });

                    // Should never get here - exception is expected
                    Assert.True(1 == 2);
                }
                catch (MetadataException ex)
                {
                    if (new string[] { "MaxLength", "Precision", "Scale" }.Contains(funcImportInvalidEnumParamFacets[i, 0]))
                    {
                        Assert.True(ex.Message.Contains(funcImportInvalidEnumParamFacets[i, 0] + " facet isn't allowed for properties of type EnumTestModel.Color"), "Unexpected exception\n: " + ex);
                    }
                    else if (funcImportInvalidEnumParamFacets[i, 0] == "Nullable")
                    {
                        Assert.True(ex.Message.Contains("non-nullable"), "Unexpected exception\n: " + ex);
                    }
                    else
                    {
                        Assert.True(ex.Message.Contains("The '" + funcImportInvalidEnumParamFacets[i, 0] + "' attribute is not allowed"), "Unexpected exception\n: " + ex);
                    }
                }
            }
        }

        [Fact]
        public void Collection_of_enum_values_can_be_specified_as_FunctionImport_ReturnType()
        {
            var functionImportDefinitions = new XElement[] { 
                XElement.Parse(@"<FunctionImport Name=""TestFunc"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" ReturnType=""Collection(EnumTestModel.Color)"" />"),
                XElement.Parse(
                    @"<FunctionImport Name=""TestFunc"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
                        <ReturnType Type=""Collection(EnumTestModel.Color)"" />
                    </FunctionImport>"),
                XElement.Parse(
                    @"<FunctionImport Name=""TestFunc"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
                        <ReturnType Type=""Collection(EnumTestModel.Color)"" />
                        <ReturnType Type=""Collection(EnumTestModel.Color)"" />
                    </FunctionImport>")
            };

            foreach (var functionImportDefinition in functionImportDefinitions)
            {
                var enumCSDL = XDocument.Parse(enumCsdl);
                enumCSDL.Root
                    .Descendants(XName.Get("EntityContainer", edmNamespace))
                    .Single()
                    .Add(functionImportDefinition);

                var edmItemCollection = new EdmItemCollection(new XmlReader[] { enumCSDL.CreateReader() });
                var functionImport = edmItemCollection.GetItems<EntityContainer>().Single().FunctionImports.Single(f => f.Name == "TestFunc");

                Assert.Equal(functionImportDefinition.Elements("{http://schemas.microsoft.com/ado/2009/11/edm}ReturnType").Count() + functionImportDefinition.Attributes("ReturnType").Count(),
                    functionImport.ReturnParameters.Count);

                foreach (var retParam in (IEnumerable<FunctionParameter>)functionImport.ReturnParameters ?? new FunctionParameter[] { functionImport.ReturnParameter })
                {
                    var retParamEdmType = retParam.TypeUsage.EdmType;

                    Assert.True(retParamEdmType is CollectionType);
                    Assert.Equal(((CollectionType)retParamEdmType).TypeUsage.EdmType.BuiltInTypeKind, BuiltInTypeKind.EnumType);
                }
            }
        }

        [Fact]
        public void Enum_type_with_code_generation_ExternalTypeName_attribute_is_valid()
        {
            var document = XDocument.Parse(enumCsdl);
            var enumType = document.Descendants(XName.Get("EnumType", edmNamespace)).First();
            enumType.SetAttributeValue(XName.Get("ExternalTypeName", "http://schemas.microsoft.com/ado/2006/04/codegeneration"), "abc");

            document.Validate(csdlSchemaSet, (o, e) => { throw e.Exception; });
        }

        [Fact]
        public void Code_generation_ExternamTypeName_attribute_is_added_to_metadata_properties()
        {
            var document = XDocument.Parse(enumCsdl);
            var enumType = document.Descendants(XName.Get("EnumType", edmNamespace)).First();
            enumType.SetAttributeValue(XName.Get("ExternalTypeName", "http://schemas.microsoft.com/ado/2006/04/codegeneration"), "abc");

            var itemCollection = new EdmItemCollection(new[] { document.CreateReader() });

            Assert.Equal(
                "abc",
                itemCollection.GetItems<EnumType>().Single().MetadataProperties.Single(m => m.Name == "http://schemas.microsoft.com/ado/2006/04/codegeneration:ExternalTypeName").Value);
        }

        [Fact]
        public void Validation_fails_for_enums_with_missing_EnumTypeName()
        {
            var csdl = XDocument.Parse(enumCsdl);
            var enumType = csdl.Descendants(XName.Get("EnumType", edmNamespace)).First();
            enumType.Attributes("Name").Remove();

            var exceptionMessage = string.Empty;
            csdl.Validate(csdlSchemaSet, (o, e) => exceptionMessage = e.Message);
            Assert.Equal("The required attribute 'Name' is missing.", exceptionMessage);
        }

        [Fact]
        public void Validation_fails_for_enums_with_empty_EnumTypeName()
        {
            var csdl = XDocument.Parse(enumCsdl);
            var enumType = csdl.Descendants(XName.Get("EnumType", edmNamespace)).First();
            enumType.SetAttributeValue("Name", string.Empty);

            var exceptionMessage = string.Empty;
            csdl.Validate(csdlSchemaSet, (o, e) => exceptionMessage = e.Message);
            Assert.True(exceptionMessage.Contains("Name"));
            Assert.True(exceptionMessage.Contains("http://schemas.microsoft.com/ado/2009/11/edm:TSimpleIdentifier"));
        }

        [Fact]
        public void Validation_fails_for_enums_with_empty_UnderlyingType_value()
        {
            var csdl = XDocument.Parse(enumCsdl);
            var enumType = csdl.Descendants(XName.Get("EnumType", edmNamespace)).First();
            enumType.SetAttributeValue("UnderlyingType", string.Empty);

            var exceptionMessage = string.Empty;
            csdl.Validate(csdlSchemaSet, (o, e) => exceptionMessage = e.Message);
            Assert.True(exceptionMessage.Contains("UnderlyingType"));
            Assert.True(exceptionMessage.Contains("http://schemas.microsoft.com/ado/2009/11/edm:TPropertyType"));
        }

        [Fact]
        public void Validation_fails_for_enums_with_empty_IsFlags_value()
        {
            var csdl = XDocument.Parse(enumCsdl);
            var enumType = csdl.Descendants(XName.Get("EnumType", edmNamespace)).First();
            enumType.SetAttributeValue("IsFlags", string.Empty);

            var exceptionMessage = string.Empty;
            csdl.Validate(csdlSchemaSet, (o, e) => exceptionMessage = e.Message);
            Assert.True(exceptionMessage.Contains("IsFlags"));
            Assert.True(exceptionMessage.Contains("http://www.w3.org/2001/XMLSchema:boolean"));
        }

        [Fact]
        public void Validation_fails_for_enums_with_invalid_IsFlags_value()
        {
            var csdl = XDocument.Parse(enumCsdl);
            var enumType = csdl.Descendants(XName.Get("EnumType", edmNamespace)).First();
            enumType.SetAttributeValue("IsFlags", "abc");

            var exceptionMessage = string.Empty;
            csdl.Validate(csdlSchemaSet, (o, e) => exceptionMessage = e.Message);
            Assert.True(exceptionMessage.Contains("IsFlags"));
            Assert.True(exceptionMessage.Contains("abc"));
            Assert.True(exceptionMessage.Contains("http://www.w3.org/2001/XMLSchema:boolean"));
        }

        [Fact]
        public void Validation_fails_for_enums_with_missing_enum_member_Name()
        {
            var csdl = XDocument.Parse(enumCsdl);
            var enumMember = csdl.Descendants(XName.Get("Member", edmNamespace)).First();
            enumMember.Attributes("Name").Remove();

            var exceptionMessage = string.Empty;
            csdl.Validate(csdlSchemaSet, (o, e) => exceptionMessage = e.Message);
            Assert.Equal("The required attribute 'Name' is missing.", exceptionMessage);
        }

        [Fact]
        public void Validation_fails_for_enums_with_empty_enum_member_Name()
        {
            var csdl = XDocument.Parse(enumCsdl);
            var enumMember = csdl.Descendants(XName.Get("Member", edmNamespace)).First();
            enumMember.SetAttributeValue("Name", string.Empty);

            var exceptionMessage = string.Empty;
            csdl.Validate(csdlSchemaSet, (o, e) => exceptionMessage = e.Message);
            Assert.True(exceptionMessage.Contains("Name"));
            Assert.True(exceptionMessage.Contains("http://schemas.microsoft.com/ado/2009/11/edm:TSimpleIdentifier"));
        }

        [Fact]
        public void Validation_fails_for_enums_with_empty_member_Value()
        {
            var csdl = XDocument.Parse(enumCsdl);
            var enumMember = csdl.Descendants(XName.Get("Member", edmNamespace)).First();
            enumMember.SetAttributeValue("Value", string.Empty);

            var exceptionMessage = string.Empty;
            csdl.Validate(csdlSchemaSet, (o, e) => exceptionMessage = e.Message);
            Assert.True(exceptionMessage.Contains("Value"));
            Assert.True(exceptionMessage.Contains("http://www.w3.org/2001/XMLSchema:long"));
        }

        [Fact]
        public void Validation_fails_for_enums_with_invalid_member_Value()
        {
            var csdl = XDocument.Parse(enumCsdl);
            var enumMember = csdl.Descendants(XName.Get("Member", edmNamespace)).First();
            enumMember.SetAttributeValue("Value", "abc");

            var exceptionMessage = string.Empty;
            csdl.Validate(csdlSchemaSet, (o, e) => exceptionMessage = e.Message);
            Assert.True(exceptionMessage.Contains("Value"));
            Assert.True(exceptionMessage.Contains("abc"));
            Assert.True(exceptionMessage.Contains("http://www.w3.org/2001/XMLSchema:long"));
        }

        [Fact]
        public void Validation_fails_for_enums_with_out_of_range_member_Value()
        {
            var csdl = XDocument.Parse(enumCsdl);
            var enumMember = csdl.Descendants(XName.Get("Member", edmNamespace)).First();
            enumMember.SetAttributeValue("Value", new string('9', 30));

            var exceptionMessage = string.Empty;
            csdl.Validate(csdlSchemaSet, (o, e) => exceptionMessage = e.Message);

            Assert.True(exceptionMessage.Contains("Value"));
            Assert.True(exceptionMessage.Contains("999999999999999999999999999999"));
            Assert.True(exceptionMessage.Contains("http://www.w3.org/2001/XMLSchema:long"));
        }

        [Fact]
        public void Validation_fails_for_enum_with_duplicate_member()
        {
            var csdl =
@"<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""EnumTestModel"">
  <EnumType Name=""Color"" IsFlags=""false"">
    <Member Name=""Yellow"" Value=""1"" />
    <Member Name=""Yellow"" Value=""1"" />
    <Member Name=""Green"" Value=""2"" />
  </EnumType>
</Schema>";

            var exceptionMessage = Assert.Throws<MetadataException>(() => new EdmItemCollection(new[] { XmlReader.Create(new StringReader(csdl)) })).Message;
            Assert.True(exceptionMessage.Contains(Strings.DuplicateEnumMember));
        }

        [Fact]
        public void Validation_fails_for_enums_with_incorrect_UnderlyingType_String()
        {
            IncorrectUnderlyingType("String", Strings.InvalidEnumUnderlyingType);
        }

        [Fact]
        public void Validation_fails_for_enums_with_incorrect_UnderlyingType_ModelType()
        {
            IncorrectUnderlyingType("EnumTestModel.EnumTestEntity", Strings.InvalidEnumUnderlyingType);
        }

        [Fact]
        public void Validation_fails_for_enums_with_incorrect_UnderlyingType_SameEnumType()
        {
            IncorrectUnderlyingType("EnumTestModel.Color", Strings.InvalidEnumUnderlyingType);
        }

        [Fact]
        public void Validation_fails_for_enums_with_incorrect_UnderlyingType_NonExistingType()
        {
            IncorrectUnderlyingType("EnumTestModel.NonExistingType", Strings.NotInNamespaceNoAlias("NonExistingType", "EnumTestModel"));
        }

        [Fact]
        public void Validation_fails_for_enums_with_incorrect_UnderlyingType_InvalidEdmType()
        {
            IncorrectUnderlyingType("Edm.MyType", Strings.NotInNamespaceNoAlias("MyType", "Edm"));
        }

        private void IncorrectUnderlyingType(string typeName, string expectedException)
        {
            var csdlDocument = XDocument.Parse(enumCsdl);
            var enumType = csdlDocument.Descendants(XName.Get("EnumType", edmNamespace)).Single();
            enumType.SetAttributeValue("UnderlyingType", typeName);

            var exceptionMessage = Assert.Throws<MetadataException>(() => new EdmItemCollection(new[] { csdlDocument.CreateReader() })).Message;
            Assert.True(exceptionMessage.Contains(expectedException));
        }

        [Fact]
        public void Validation_fails_for_enums_with_CalculatedValue_out_of_range()
        {
            var csdl =
@"<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""EnumTestModel"">
    <EnumType Name=""Color"" IsFlags=""false"">
    <Member Name=""Yellow"" Value=""9223372036854775807"" />
    <Member Name=""Green"" />
    </EnumType>
</Schema>";

            var exceptionMessage = Assert.Throws<MetadataException>(() => new EdmItemCollection(new[] { XmlReader.Create(new StringReader(csdl)) })).Message;
            Assert.True(exceptionMessage.Contains(Strings.CalculatedEnumValueOutOfRange));
        }

        [Fact]
        public void Validation_fails_for_enums_with_SpecifiedValue_too_large_to_fit_in_Byte()
        {
            var value = (((long)Byte.MaxValue) + 1).ToString();
            RunUnderOverflowTestForExplicitlySpecifiedValues("Byte", value, Strings.EnumMemberValueOutOfItsUnderylingTypeRange(value, "Yellow", "Byte"));
        }

        [Fact]
        public void Validation_fails_for_enums_with_SpecifiedValue_cannot_be_converted_to_Byte()
        {
            var value = (((long)Byte.MinValue) - 1).ToString();
            RunUnderOverflowTestForExplicitlySpecifiedValues("Byte", value, Strings.EnumMemberValueOutOfItsUnderylingTypeRange(value, "Yellow", "Byte"));
        }

        [Fact]
        public void Validation_fails_for_enums_with_SpecifiedValue_too_large_to_fit_in_SByte()
        {
            var value = (((long)SByte.MaxValue) + 1).ToString();
            RunUnderOverflowTestForExplicitlySpecifiedValues("SByte", value, Strings.EnumMemberValueOutOfItsUnderylingTypeRange(value, "Yellow", "SByte"));
        }

        [Fact]
        public void Validation_fails_for_enums_with_SpecifiedValue_cannot_be_converted_to_SByte()
        {
            var value = (((long)SByte.MinValue) - 1).ToString();
            RunUnderOverflowTestForExplicitlySpecifiedValues("SByte", value, Strings.EnumMemberValueOutOfItsUnderylingTypeRange(value, "Yellow", "SByte"));
        }

        [Fact]
        public void Validation_fails_for_enums_with_SpecifiedValue_too_large_to_fit_in_Int16()
        {
            var value = (((long)Int16.MaxValue) + 1).ToString();
            RunUnderOverflowTestForExplicitlySpecifiedValues("Int16", value, Strings.EnumMemberValueOutOfItsUnderylingTypeRange(value, "Yellow", "Int16"));
        }

        [Fact]
        public void Validation_fails_for_enums_with_SpecifiedValue_cannot_be_converted_to_Int16()
        {
            var value = (((long)Int16.MinValue) - 1).ToString();
            RunUnderOverflowTestForExplicitlySpecifiedValues("Int16", value, Strings.EnumMemberValueOutOfItsUnderylingTypeRange(value, "Yellow", "Int16"));
        }

        [Fact]
        public void Validation_fails_for_enums_with_SpecifiedValue_too_large_to_fit_in_Int32()
        {
            var value = (((long)Int32.MaxValue) + 1).ToString();
            RunUnderOverflowTestForExplicitlySpecifiedValues("Int32", value, Strings.EnumMemberValueOutOfItsUnderylingTypeRange(value, "Yellow", "Int32"));
        }

        [Fact]
        public void Validation_fails_for_enums_with_SpecifiedValue_cannot_be_converted_to_Int32()
        {
            var value = (((long)Int32.MinValue) - 1).ToString();
            RunUnderOverflowTestForExplicitlySpecifiedValues("Int32", value, Strings.EnumMemberValueOutOfItsUnderylingTypeRange(value, "Yellow", "Int32"));
        }

        [Fact]
        public void Validation_fails_for_enums_with_SpecifiedValue_too_large_to_fit_in_Int64()
        {
            var value = ((long)Int64.MaxValue).ToString() + "1";
            RunUnderOverflowTestForExplicitlySpecifiedValues("Int64", value, "http://www.w3.org/2001/XMLSchema:long");
        }

        [Fact]
        public void Validation_fails_for_enums_with_SpecifiedValue_cannot_be_converted_to_Int64()
        {
            var value = "-1" + ((long)Int64.MaxValue).ToString();
            RunUnderOverflowTestForExplicitlySpecifiedValues("Int64", value, "http://www.w3.org/2001/XMLSchema:long");
        }

        private static void RunUnderOverflowTestForExplicitlySpecifiedValues(string underlyingTypeName, string value, string expectedException)
        {
            var document = XDocument.Parse(enumCsdl);
            var enumType = document.Descendants(XName.Get("EnumType", edmNamespace)).Single();
            enumType.SetAttributeValue("UnderlyingType", underlyingTypeName);
            document.Descendants(XName.Get("Member", edmNamespace))
                    .First()
                    .SetAttributeValue("Value", value);

            var exceptionMessage = Assert.Throws<MetadataException>(() => new EdmItemCollection(new[] { document.CreateReader() })).Message;
            Assert.True(exceptionMessage.Contains(expectedException));
        }

        [Fact]
        public void Validation_fails_for_enums_with_CalculatedValue_too_large_to_fit_in_Byte()
        {
            var value = (long)Byte.MaxValue;
            var exceptions = new [] 
            {
                Strings.EnumMemberValueOutOfItsUnderylingTypeRange(value + 1, "Green", "Byte"),
                Strings.EnumMemberValueOutOfItsUnderylingTypeRange(value + 2, "Blue", "Byte"),
            };

            RunOverflowTestsForCalculatedValues("Byte", value, exceptions);
        }

        [Fact]
        public void Validation_fails_for_enums_with_CalculatedValue_too_large_to_fit_in_SByte()
        {
            var value = (long)SByte.MaxValue;
            var exceptions = new[] 
            {
                Strings.EnumMemberValueOutOfItsUnderylingTypeRange(value + 1, "Green", "SByte"),
                Strings.EnumMemberValueOutOfItsUnderylingTypeRange(value + 2, "Blue", "SByte"),
            };

            RunOverflowTestsForCalculatedValues("SByte", value, exceptions);
        }

        [Fact]
        public void Validation_fails_for_enums_with_CalculatedValue_too_large_to_fit_in_Int16()
        {
            var value = (long)Int16.MaxValue;
            var exceptions = new[] 
            {
                Strings.EnumMemberValueOutOfItsUnderylingTypeRange(value + 1, "Green", "Int16"),
                Strings.EnumMemberValueOutOfItsUnderylingTypeRange(value + 2, "Blue", "Int16"),
            };

            RunOverflowTestsForCalculatedValues("Int16", value, exceptions);
        }

        [Fact]
        public void Validation_fails_for_enums_with_CalculatedValue_too_large_to_fit_in_Int32()
        {
            var value = (long)Int32.MaxValue;
            var exceptions = new[] 
            {
                Strings.EnumMemberValueOutOfItsUnderylingTypeRange(value + 1, "Green", "Int32"),
                Strings.EnumMemberValueOutOfItsUnderylingTypeRange(value + 2, "Blue", "Int32"),
            };

            RunOverflowTestsForCalculatedValues("Int32", value, exceptions);
        }

        private static void RunOverflowTestsForCalculatedValues(string underlyingTypeName, long lastSpecifiedValue, string[] expectedExceptions)
        {
            var document = XDocument.Parse(enumCsdl);
            var enumType = document.Descendants(XName.Get("EnumType", edmNamespace)).Single();
            enumType.SetAttributeValue("UnderlyingType", underlyingTypeName);
            document.Descendants(XName.Get("Member", edmNamespace))
                    .First()
                    .SetAttributeValue("Value", lastSpecifiedValue);

            document.Descendants(XName.Get("Member", edmNamespace))
                    .Skip(1)
                    .Attributes("Value")
                    .Remove();

            var exceptionMessage = Assert.Throws<MetadataException>(() => new EdmItemCollection(new[] { document.CreateReader() })).Message;
            foreach (var expectedException in expectedExceptions)
            {
                Assert.True(exceptionMessage.Contains(expectedException));
            }
        }

        [Fact]
        public void DefaultValue_not_supported_for_enum_properties()
        {
            var csdl = @"<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""EnumTestModel"">
  <EntityType Name=""EnumTestEntity"">
    <Key>
      <PropertyRef Name=""Id"" />
    </Key>
    <Property Name=""Id"" Nullable=""false"" Type=""Int32"" />
    <Property Name=""Color"" Type=""EnumTestModel.Color"" Nullable=""false"" DefaultValue=""5""/>
  </EntityType>
  <EnumType Name=""Color"" IsFlags=""false"">
    <Member Name=""Yellow"" Value=""1"" />
  </EnumType>
</Schema>";

            var exceptionMessage = Assert.Throws<MetadataException>(() => new EdmItemCollection(new[] { XmlReader.Create(new StringReader(csdl)) })).Message;
            Assert.True(exceptionMessage.Contains(Strings.DefaultNotAllowed));
        }
    }

    public class MetadataEnumFixture
    {
        public XmlSchemaSet CsdlSchemaSet { get; private set; }

        public MetadataEnumFixture()
        {
            this.CsdlSchemaSet = new XmlSchemaSet();

            foreach (var schemaName in new string[] { 
                    "System.Data.Resources.CSDLSchema_3.xsd",  
                    "System.Data.Resources.CodeGenerationSchema.xsd",
                    "System.Data.Resources.AnnotationSchema.xsd"})
            {
                this.CsdlSchemaSet.Add(XmlSchema.Read(
                    typeof(EntityContainer).Assembly.GetManifestResourceStream(schemaName),
                    (o, e) => { throw new InvalidOperationException("The built-in schema is invalid", e.Exception); }));
            }

            this.CsdlSchemaSet.Compile();
        }
    }
}
