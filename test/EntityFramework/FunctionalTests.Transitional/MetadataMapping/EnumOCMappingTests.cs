// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.MetadataMapping
{
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using Xunit;

    public class EnumOCMappingTests : FunctionalTestBase
    {
        private static XDocument EnumCsdl()
        {
            return XDocument.Load(
                typeof(EnumOCMappingTests).Assembly().GetManifestResourceStream("System.Data.Entity.MetadataMapping.Enum.csdl"));
        }

        #region convention loader (POCO)

        [Fact]
        public void Verify_simple_enum_mapping_POCO()
        {
            Verify_simple_enum_mapping(true);
        }

        [Fact]
        public void Complex_type_with_enum_property_is_mapped_correctly_POCO()
        {
            Complex_type_with_enum_property_is_mapped_correctly(true);
        }

        [Fact]
        public void Enums_with_members_with_same_values_are_mapped_even_if_order_is_different_POCO()
        {
            Enums_with_members_with_same_values_are_mapped_even_if_order_is_different(true);
        }

        [Fact]
        public void Nullability_of_enum_properties_ignored_for_mapping_POCO()
        {
            Nullability_of_enum_properties_ignored_for_mapping(true);
        }

        [Fact]
        public void Can_map_enum_type_with_no_members_POCO()
        {
            Can_map_enum_type_with_no_members(true);
        }

        [Fact]
        public void Cannot_map_OSpace_enum_type_with_unsupported_underlying_type_POCO()
        {
            var exception = Assert.Throws<MetadataException>(
                    () => Cannot_map_OSpace_enum_type_with_unsupported_underlying_type(true));

            exception.ValidateMessage("Validator_OSpace_Convention_MissingOSpaceType", false, "MessageModel.MessageType");
            exception.ValidateMessage("Validator_UnsupportedEnumUnderlyingType", false, "System.UInt32");
        }

        [Fact]
        public void Cannot_map_enum_types_if_names_are_different_POCO()
        {
            var exception = Assert.Throws<MetadataException>(() => Cannot_map_enum_types_if_names_are_different(true));
            exception.ValidateMessage("Validator_OSpace_Convention_MissingOSpaceType", false, "MessageModel.PaymentMethod");
        }

        [Fact]
        public void Cannot_map_enum_types_if_underlying_types_dont_match_POCO()
        {
            var exception = Assert.Throws<MetadataException>(
                () => Cannot_map_enum_types_if_underlying_types_dont_match(true));

            exception.ValidateMessage("Validator_OSpace_Convention_MissingOSpaceType", false, "MessageModel.MessageType");
            exception.ValidateMessage("Validator_OSpace_Convention_NonMatchingUnderlyingTypes", false);
        }

        [Fact]
        public void Cannot_map_OSpace_enum_type_with_fewer_members_than_CSpace_enum_type_POCO()
        {
            var exception = Assert.Throws<MetadataException>(
                () => Cannot_map_OSpace_enum_type_with_fewer_members_than_CSpace_enum_type(true));

            exception.ValidateMessage("Validator_OSpace_Convention_MissingOSpaceType", false, "MessageModel.MessageType");
            exception.ValidateMessage("Mapping_Enum_OCMapping_MemberMismatch", false, "MessageModel.MessageType", "Ground", 2, "MessageModel.MessageType");
        }

        [Fact]
        public void Cannot_map_OSpace_enum_type_whose_member_name_does_not_match_CSpace_enum_type_member_name_POCO()
        {
            var exception = Assert.Throws<MetadataException>(
                () => Cannot_map_OSpace_enum_type_whose_member_name_does_not_match_CSpace_enum_type_member_name(true));

            exception.ValidateMessage("Validator_OSpace_Convention_MissingOSpaceType", false, "MessageModel.MessageType");
            exception.ValidateMessage("Mapping_Enum_OCMapping_MemberMismatch", false, "MessageModel.MessageType", "Ground", 2, "MessageModel.MessageType");
        }

        [Fact]
        public void Can_map_OSpace_enum_type_that_has_more_members_than_CSPace_enum_type_if_members_match_POCO()
        {
            Can_map_OSpace_enum_type_that_has_more_members_than_CSPace_enum_type_if_members_match(true);
        }

        [Fact]
        public void Can_map_CSpace_enum_type_with_no_enum_members_POCO()
        {
            Can_map_CSpace_enum_type_with_no_enum_members(true);
        }

        [Fact]
        public void Cannot_map_if_OSpace_enum_type_member_value_does_not_match_CSpace_enum_type_member_value_POCO()
        {
            var exception = Assert.Throws<MetadataException>(
                () => Cannot_map_if_OSpace_enum_type_member_value_does_not_match_CSpace_enum_type_member_value(true));

            exception.ValidateMessage("Validator_OSpace_Convention_MissingOSpaceType", false, "MessageModel.MessageType");
            exception.ValidateMessage("Mapping_Enum_OCMapping_MemberMismatch", false, "MessageModel.MessageType", "Ground", 2, "MessageModel.MessageType");
        }

        [Fact]
        public void Verify_OSpace_enum_type_is_not_mapped_to_CSpace_entity_type_with_same_name_POCO()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => OSpace_enum_type_and_CSpace_entity_type_have_the_same_name(true));

            exception.ValidateMessage("Mapping_Object_InvalidType", "Model.MessageType");
        }

        [Fact]
        public void Verify_OSpace_entity_type_is_not_mapped_to_CSpace_enum_type_with_same_name_POCO()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => OSpace_entity_type_and_CSpace_enum_type_have_the_same_name(true));

            exception.ValidateMessage("Mapping_Object_InvalidType", "Model.MessageType");
        }

        // POCO specific cases

        [Fact]
        public void Correct_CSpace_enum_type_is_mapped_if_multiple_OSpace_enum_types_exist_but_only_one_matches()
        {
            var additionalEnumType = XDocument.Parse(
                @"<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""MessageModel1"">
    <EnumType Name=""MessageType"" IsFlags=""false"" />
</Schema>");

            var workspace =
                CreateMetadataWorkspace(
                    EnumCsdl(),
                    BuildAssembly(true, EnumCsdl(), additionalEnumType),
                    true);

            Assert.Equal(
                "MessageModel.MessageType:MessageModel.MessageType",
                workspace.GetMap("MessageModel.MessageType", DataSpace.OSpace, DataSpace.OCSpace).Identity);

            Assert.Equal(
                "MessageModel.Message:MessageModel.Message",
                workspace.GetMap("MessageModel.Message", DataSpace.OSpace, DataSpace.OCSpace).Identity);
        }

        [Fact]
        public void Mapping_fails_for_multiple_OSpace_enum_types_matching_the_same_CSpace_enum_type_POCO()
        {
            var additionalMatchingEnumType = XDocument.Parse(
                @"<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""MessageModel1"">
  <EnumType Name=""MessageType"" IsFlags=""false"">
    <Member Name=""Express"" />
    <Member Name=""Priority"" />
    <Member Name=""Ground"" />
    <Member Name=""Air"" />
  </EnumType>
</Schema>");

            var assembly = BuildAssembly(true, EnumCsdl(), additionalMatchingEnumType);
            var exception = Assert.Throws<MetadataException>(
                () => CreateMetadataWorkspace(EnumCsdl(), assembly, true));

            exception.ValidateMessage(
                "Validator_OSpace_Convention_AmbiguousClrType",
                false,
                "MessageType",
                "MessageModel.MessageType",
                "MessageModel1.MessageType");
        }

        [Fact]
        public void Cannot_create_workspace_if_OSpace_enum_property_does_not_have_getter()
        {
            var oSpaceCsdl = EnumCsdl();
            oSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}Property")
                .Single(p => (string)p.Attribute("Name") == "MessageType")
                .SetAttributeValue("{MappingTestExtension}SuppressGetter", true);

            var assembly = BuildAssembly(true, oSpaceCsdl);
            var exception = Assert.Throws<MetadataException>(
                () => CreateMetadataWorkspace(EnumCsdl(), assembly, true));

            exception.ValidateMessage(
                "Validator_OSpace_Convention_ScalarPropertyMissginGetterOrSetter",
                false,
                "MessageType",
                "MessageModel.Message",
                assembly.FullName);
        }

        [Fact]
        public void Cannot_create_workspace_if_OSpace_enum_property_does_not_have_setter()
        {
            var oSpaceCsdl = EnumCsdl();
            oSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}Property")
                .Single(p => (string)p.Attribute("Name") == "MessageType")
                .SetAttributeValue("{MappingTestExtension}SuppressSetter", true);

            var assembly = BuildAssembly(true, oSpaceCsdl);
            var exception = Assert.Throws<MetadataException>(
                () => CreateMetadataWorkspace(EnumCsdl(), assembly, true));

            exception.ValidateMessage(
                "Validator_OSpace_Convention_ScalarPropertyMissginGetterOrSetter",
                false,
                "MessageType",
                "MessageModel.Message",
                assembly.FullName);
        }

        [Fact]
        public void Can_load_entity_with_property_of_enum_type_from_different_assembly()
        {
            const bool isPOCO = true;

            var enumTypeCsdl = XDocument.Parse(
                @"<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""EnumModel"">
  <EnumType Name=""Enum"" IsFlags=""false"" />
</Schema>");

            var entityTypeCsdl = XDocument.Parse(
                @"<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""EnumModel"">
  <EntityContainer Name=""EnumModelContainer"">
    <EntitySet Name=""Entity"" EntityType=""EnumModel.Entity"" />
  </EntityContainer>
  <EntityType Name=""Entity"">
    <Key>
      <PropertyRef Name=""Id"" />
    </Key>
    <Property Name=""Id"" Nullable=""false"" Type=""Int32"" />
    <Property Name=""EnumProperty"" Nullable=""false"" Type=""EnumModel.Enum"" />
  </EntityType>
</Schema>");

            var assemblyWithEnumType = BuildAssembly(isPOCO, enumTypeCsdl);
            var assemblyWithEntityType = BuildAssembly(isPOCO, entityTypeCsdl);

            EdmItemCollection edmItemCollection;
            using (var enumTypeReader = enumTypeCsdl.CreateReader())
            {
                using (var entityTypeReader = entityTypeCsdl.CreateReader())
                {
                    edmItemCollection =
                        new EdmItemCollection(
                            new[] { enumTypeReader, entityTypeReader });
                }
            }

            var objectItemCollection = new ObjectItemCollection();
            objectItemCollection.LoadFromAssembly(assemblyWithEnumType, edmItemCollection);
            objectItemCollection.LoadFromAssembly(assemblyWithEntityType, edmItemCollection);

            var workspace = new MetadataWorkspace(
                () => edmItemCollection,
                () => null,
                () => null,
                () => objectItemCollection);

            Assert.Equal(
                "EnumModel.Entity:EnumModel.Entity",
                workspace.GetMap("EnumModel.Entity", DataSpace.OSpace, DataSpace.OCSpace).Identity);
        }

        #endregion

        #region attribute loader (non-POCO)

        [Fact]
        public void Verify_simple_enum_mapping_non_POCO()
        {
            Verify_simple_enum_mapping(false);
        }

        [Fact]
        public void Complex_type_with_enum_property_is_mapped_correctly_NonPOCO()
        {
            Complex_type_with_enum_property_is_mapped_correctly(false);
        }

        [Fact]
        public void Enums_with_members_with_same_values_are_mapped_even_if_order_is_different_NonPOCO()
        {
            Enums_with_members_with_same_values_are_mapped_even_if_order_is_different(false);
        }

        [Fact]
        public void Nullability_of_enum_properties_ignored_for_mapping_NonPOCO()
        {
            Nullability_of_enum_properties_ignored_for_mapping(false);
        }

        [Fact]
        public void Can_map_enum_type_with_no_members_NonPOCO()
        {
            Can_map_enum_type_with_no_members(false);
        }

        [Fact]
        public void Cannot_map_OSpace_enum_type_with_unsupported_underlying_NonPOCO()
        {
            var exception = Assert.Throws<MetadataException>(
                    () => Cannot_map_OSpace_enum_type_with_unsupported_underlying_type(false));

            exception.ValidateMessage(
                "Validator_OSpace_ScalarPropertyNotPrimitive",
                false,
                "TypeOfMessage",
                "MessageModel.MessageTypeLookUp",
                "MessageModel.MessageType");

            exception.ValidateMessage("Validator_UnsupportedEnumUnderlyingType", false, "System.UInt32");
        }

        [Fact]
        public void Cannot_map_enum_types_if_names_are_different_NonPOCO()
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => Cannot_map_enum_types_if_names_are_different(false));

            exception.ValidateMessage("Mapping_Object_InvalidType", "MessageModel.ShippingType");
        }

        [Fact]
        public void OSpaceEnumUnderlyingTypeDoesNotMatchCSpaceEnumUnderlyingTypeName_NonPOCO()
        {
            var exception = Assert.Throws<MappingException>(
                () => Cannot_map_enum_types_if_underlying_types_dont_match(false));

            exception.ValidateMessage(
                "Mapping_Enum_OCMapping_UnderlyingTypesMismatch",
                false,
                "Int32",
                "MessageModel.MessageType",
                "Int64", 
                "MessageModel.MessageType");
        }

        [Fact]
        public void Cannot_map_OSpace_enum_type_with_fewer_members_than_CSpace_enum_type_NonPOCO()
        {
            var exception = Assert.Throws<MappingException>(
                () => Cannot_map_OSpace_enum_type_with_fewer_members_than_CSpace_enum_type(false));

            exception.ValidateMessage(
                "Mapping_Enum_OCMapping_MemberMismatch",
                "MessageModel.MessageType",
                "Ground",
                2,
                "MessageModel.MessageType");
        }

        [Fact]
        public void OSpaceEnumTypeMemberNameDoesNotMatchCSpaceEnumTypeMemberName_NonPOCO()
        {
            var exception = Assert.Throws<MappingException>(
                () => Cannot_map_OSpace_enum_type_whose_member_name_does_not_match_CSpace_enum_type_member_name(false));

            exception.ValidateMessage(
                "Mapping_Enum_OCMapping_MemberMismatch", 
                "MessageModel.MessageType", 
                "Ground", 
                2, 
                "MessageModel.MessageType");
        }

        [Fact]
        public void Can_map_OSpace_enum_type_that_has_more_members_than_CSPace_enum_type_if_members_match_NonPOCO()
        {
            Can_map_OSpace_enum_type_that_has_more_members_than_CSPace_enum_type_if_members_match(false);
        }

        [Fact]
        public void Can_map_CSpace_enum_type_with_no_enum_members_NonPOCO()
        {
            Can_map_CSpace_enum_type_with_no_enum_members(false);
        }

        [Fact]
        public void Cannot_map_if_OSpace_enum_type_member_value_does_not_match_CSpace_enum_type_member_value_NonPOCO()
        {
            var exception = Assert.Throws<MappingException>(
                () => Cannot_map_if_OSpace_enum_type_member_value_does_not_match_CSpace_enum_type_member_value(false));

            exception.ValidateMessage(
                "Mapping_Enum_OCMapping_MemberMismatch",
                "MessageModel.MessageType",
                "Ground",
                2,
                "MessageModel.MessageType");
        }

        [Fact]
        public void Cannot_map_OSpace_enum_type_to_CSpace_entity_type_with_the_same_name_NonPOCO()
        {
            var exception = Assert.Throws<MappingException>(
                () => OSpace_enum_type_and_CSpace_entity_type_have_the_same_name(false));

            exception.ValidateMessage("Mapping_EnumTypeMappingToNonEnumType", "Model.MessageType", "Model.MessageType");
        }

        [Fact]
        public void Cannot_map_OSpace_entity_type_to_CSpace_enum_type_with_the_same_name_NonPOCO()
        {
            var exception = Assert.Throws<MappingException>(
                () => OSpace_entity_type_and_CSpace_enum_type_have_the_same_name(false));

            exception.ValidateMessage("Mapping_EnumTypeMappingToNonEnumType", "Model.MessageType", "Model.MessageType");
        }

        // non-POCO specific cases

        [Fact]
        public void EnumTypeVerifiedWhenLoadingEntityWithPropertyOfThisEnumType_NonPOCO()
        {
            var oSpaceCsdl = EnumCsdl();

            oSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}Member")
                .Single(m => (string)m.Attribute("Name") == "Ground" && (string)m.Parent.Attribute("Name") == "MessageType")
                .SetAttributeValue("Value", "64");

            //var workspace = PrepareModel(oSpaceCsdl, EnumCsdl(), false);
            var exception = Assert.Throws<MappingException>(
                    () => GetMappedType(oSpaceCsdl, EnumCsdl(), "MessageModel.Message", false));

            exception.ValidateMessage(
                "Mapping_Enum_OCMapping_MemberMismatch",
                "MessageModel.MessageType",
                "Ground",
                "2",
                "MessageModel.MessageType");
        }

        [Fact]
        public void Can_use_EdmEnumType_attribute_to_map_OSpace_enum_type_to_CSpace_enum_type_with_different_name_NonPOCO()
        {
            var cSpaceCsdl = EnumCsdl();
            cSpaceCsdl
                .Element("{http://schemas.microsoft.com/ado/2009/11/edm}Schema")
                .SetAttributeValue("Namespace", "MessageModelModified");

            foreach (var attribute in cSpaceCsdl
                .Descendants()
                .Attributes()
                .Where(a => ((string)a).StartsWith("MessageModel.")))
            {
                attribute.SetValue(((string)attribute).Replace("MessageModel.", "MessageModelModified."));
            }

            cSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}EnumType")
                .Single(e => (string)e.Attribute("Name") == "MessageType")
                .SetAttributeValue("Name", "NewMessageType");

            foreach (var propertyElement in cSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}Property")
                .Where(p => (string)p.Attribute("Type") == "MessageModelModified.MessageType"))
            {
                propertyElement.SetAttributeValue("Type", "MessageModelModified.NewMessageType");
            }

            var oSpaceCsdl = EnumCsdl();

            foreach (var typeElement in oSpaceCsdl
                .Element("{http://schemas.microsoft.com/ado/2009/11/edm}Schema")
                .Elements()
                .Where(e => new[] { "EntityType", "ComplexType", "EnumType" }.Contains(e.Name.LocalName)))
            {
                if ((string)typeElement.Attribute("Name") == "MessageType")
                {
                    typeElement.SetAttributeValue("{MappingTestExtension}OSpaceTypeName", "NewMessageType");
                }
                typeElement.SetAttributeValue("{MappingTestExtension}OSpaceTypeNamespace", "MessageModelModified");
            }

            var workspace = PrepareModel(oSpaceCsdl, cSpaceCsdl, false);

            Assert.Equal(
                "MessageModel.Message:MessageModelModified.Message",
                workspace.GetMap("MessageModel.Message", DataSpace.OSpace, DataSpace.OCSpace).Identity);

            Assert.Equal(
                "MessageModel.MessageType:MessageModelModified.NewMessageType",
                workspace.GetMap("MessageModel.MessageType", DataSpace.OSpace, DataSpace.OCSpace).Identity);
        }

        [Fact]
        public void Cannot_use_OSpace_enum_type_as_property_type_if_it_does_not_have_EdmTypeAttribute()
        {
            var oSpaceCsdl = EnumCsdl();

            oSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}EnumType")
                .Single(p => (string)p.Attribute("Name") == "MessageType")
                .SetAttributeValue("{MappingTestExtension}SuppressEdmTypeAttribute", true);

            var exception = Assert.Throws<MetadataException>(() => PrepareModel(oSpaceCsdl, EnumCsdl(), false));
            exception.ValidateMessage(
                "Validator_OSpace_ScalarPropertyNotPrimitive",
                false,
                "MessageType",
                "MessageModel.Message",
                "MessageModel.MessageType");

            exception.ValidateMessage(
                "Validator_OSpace_ScalarPropertyNotPrimitive",
                false,
                "TypeOfMessage",
                "MessageModel.MessageTypeLookUp",
                "MessageModel.MessageType");
        }

        [Fact]
        public void Cannot_have_2_OSpace_enum_types_mapped_to_single_CSpace_enum_type()
        {
            var additionalMatchingEnumType = EnumCsdl();
            additionalMatchingEnumType
                .Element("{http://schemas.microsoft.com/ado/2009/11/edm}Schema")
                .Elements()
                .Where(e => (string)e.Attribute("Name") != "MessageType")
                .Remove();

            additionalMatchingEnumType
                .Element("{http://schemas.microsoft.com/ado/2009/11/edm}Schema")
                .SetAttributeValue("Namespace", "MessageModelAddendum");

            var enumTypeElement =
                additionalMatchingEnumType.
                    Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}EnumType")
                                          .Single();

            enumTypeElement.SetAttributeValue("{MappingTestExtension}OSpaceTypeName", "MessageType");
            enumTypeElement.SetAttributeValue("{MappingTestExtension}OSpaceTypeNamespace", "MessageModel");

            var exception = Assert.Throws<MappingException>(
                () => CreateMetadataWorkspace(
                    EnumCsdl(),
                    BuildAssembly(false, EnumCsdl(), additionalMatchingEnumType),
                    false));

            exception.ValidateMessage("Mapping_CannotMapCLRTypeMultipleTimes", "MessageModel.MessageType");
        }

        #endregion

        private static void Verify_simple_enum_mapping(bool isPOCO)
        {
            var workspace = PrepareModel( /* oSpaceCsdl */ EnumCsdl(), /* cSpaceCsdl */ EnumCsdl(), isPOCO);

            Assert.Equal(
                "MessageModel.MessageType:MessageModel.MessageType",
                workspace.GetMap("MessageModel.MessageType", DataSpace.OSpace, DataSpace.OCSpace).Identity);

            Assert.Equal(
                "MessageModel.Message:MessageModel.Message",
                workspace.GetMap("MessageModel.Message", DataSpace.OSpace, DataSpace.OCSpace).Identity);
        }

        private static void Complex_type_with_enum_property_is_mapped_correctly(bool isPOCO)
        {
            var complexType = XElement.Parse(
                @"<ComplexType Name=""Complex"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
  <Property Name=""MessageType"" Type=""MessageModel.MessageType"" Nullable=""false""/>
</ComplexType>");

            var csdl = EnumCsdl();

            csdl
                .Element("{http://schemas.microsoft.com/ado/2009/11/edm}Schema")
                .Add(complexType);

            csdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}EntityType")
                .Single(e => (string)e.Attribute("Name") == "Message")
                .Add(
                    new XElement(
                        "{http://schemas.microsoft.com/ado/2009/11/edm}Property",
                        new XAttribute("Name", "ComplexProperty"),
                        new XAttribute("Type", "MessageModel.Complex"),
                        new XAttribute("Nullable", "false")));

            var workspace = PrepareModel( /* oSpaceCsdl */ csdl, /* cSpaceCsdl */ csdl, isPOCO);

            Assert.Equal(
                "MessageModel.MessageType:MessageModel.MessageType",
                workspace.GetMap("MessageModel.MessageType", DataSpace.OSpace, DataSpace.OCSpace).Identity);

            Assert.Equal(
                "MessageModel.Message:MessageModel.Message",
                workspace.GetMap("MessageModel.Message", DataSpace.OSpace, DataSpace.OCSpace).Identity);

            Assert.Equal(
                "MessageModel.Complex:MessageModel.Complex",
                workspace.GetMap("MessageModel.Complex", DataSpace.OSpace, DataSpace.OCSpace).Identity);
        }

        private static void Enums_with_members_with_same_values_are_mapped_even_if_order_is_different(bool isPOCO)
        {
            var oSpaceCsdl = EnumCsdl();
            var oSpaceEnumType = oSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}EnumType")
                .Single(e => (string)e.Attribute("Name") == "ContentsType");

            oSpaceEnumType.Add(
                new XElement(
                    "{http://schemas.microsoft.com/ado/2009/11/edm}Member",
                    new XAttribute("Name", "LegacyType"),
                    new XAttribute("Value", 0)));

            var cSpaceCsdl = new XDocument(oSpaceCsdl);
            var cSpaceEnumType = cSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}EnumType")
                .Single(e => (string)e.Attribute("Name") == "ContentsType");

            var sortedMemberElements = cSpaceEnumType.Elements().OrderByDescending(e => (string)e.Attribute("Name")).ToArray();

            cSpaceEnumType.Elements().Remove();
            cSpaceEnumType.Add(sortedMemberElements);

            Assert.Equal(
                "MessageModel.ContentsType:MessageModel.ContentsType",
                GetMappedType(oSpaceCsdl, cSpaceCsdl, "MessageModel.ContentsType", isPOCO).Identity);
        }

        private static void Nullability_of_enum_properties_ignored_for_mapping(bool isPOCO)
        {
            var oSpaceCsdl = EnumCsdl();
            oSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}Property")
                .Single(e => (string)e.Attribute("Name") == "MessageType" && (string)e.Parent.Attribute("Name") == "Message")
                .SetAttributeValue("Nullable", true);

            oSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}Property")
                .Single(e => (string)e.Attribute("Name") == "ContentsType")
                .SetAttributeValue("Nullable", false);

            var cSpaceCsdl = EnumCsdl();
            cSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}Property")
                .Single(e => (string)e.Attribute("Name") == "MessageType" && (string)e.Parent.Attribute("Name") == "Message")
                .SetAttributeValue("Nullable", false);

            cSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}Property")
                .Single(e => (string)e.Attribute("Name") == "ContentsType")
                .SetAttributeValue("Nullable", true);

            Assert.Equal(
                "MessageModel.Message:MessageModel.Message",
                GetMappedType(oSpaceCsdl, cSpaceCsdl, "MessageModel.Message", isPOCO).Identity);
        }

        private static void Can_map_enum_type_with_no_members(bool isPOCO)
        {
            var enumCsdl = EnumCsdl();
            enumCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}Member")
                .Remove();

            Assert.Equal(
                "MessageModel.Message:MessageModel.Message",
                GetMappedType(enumCsdl, enumCsdl, "MessageModel.Message", isPOCO).Identity);
        }

        private static void Cannot_map_OSpace_enum_type_with_unsupported_underlying_type(bool isPOCO)
        {
            var oSpaceCsdl = EnumCsdl();
            oSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}EnumType")
                .Single(p => (string)p.Attribute("Name") == "MessageType")
                .SetAttributeValue("UnderlyingType", "UInt32");

            PrepareModel(oSpaceCsdl, EnumCsdl(), isPOCO);
        }

        private static void Cannot_map_enum_types_if_names_are_different(bool isPOCO)
        {
            var oSpaceCsdl = EnumCsdl();

            oSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}EnumType")
                .Single(e => (string)e.Attribute("Name") == "PaymentMethod")
                .SetAttributeValue("Name", "ShippingType");

            oSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}Property")
                .Single(p => (string)p.Attribute("Name") == "PaymentMethod")
                .SetAttributeValue("Type", "MessageModel.ShippingType");

            GetMappedType(oSpaceCsdl, EnumCsdl(), "MessageModel.ShippingType", isPOCO);
        }

        private static void Cannot_map_enum_types_if_underlying_types_dont_match(bool isPOCO)
        {
            var oSpaceCsdl = EnumCsdl();

            oSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}EnumType")
                .Single(e => (string)e.Attribute("Name") == "MessageType")
                .SetAttributeValue("UnderlyingType", "Int64");

            GetMappedType(oSpaceCsdl, EnumCsdl(), "MessageModel.MessageType", isPOCO);
        }

        private static void Cannot_map_OSpace_enum_type_with_fewer_members_than_CSpace_enum_type(bool isPOCO)
        {
            var oSpaceCsdl = EnumCsdl();

            oSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}Member")
                .Where(m => (string)m.Attribute("Name") == "Ground" && (string)m.Parent.Attribute("Name") == "MessageType")
                .Remove();

            GetMappedType(oSpaceCsdl, EnumCsdl(), "MessageModel.MessageType", isPOCO);
        }

        private static void Cannot_map_OSpace_enum_type_whose_member_name_does_not_match_CSpace_enum_type_member_name(bool isPOCO)
        {
            var oSpaceCsdl = EnumCsdl();

            oSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}Member")
                .Single(m => (string)m.Attribute("Name") == "Ground" && (string)m.Parent.Attribute("Name") == "MessageType")
                .SetAttributeValue("Name", "Ground1");

            GetMappedType(oSpaceCsdl, EnumCsdl(), "MessageModel.MessageType", isPOCO);
        }

        private static void Can_map_OSpace_enum_type_that_has_more_members_than_CSPace_enum_type_if_members_match(bool isPOCO)
        {
            var oSpaceCsdl = EnumCsdl();

            var enumType = oSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}EnumType")
                .Single(e => (string)e.Attribute("Name") == "MessageType");

            enumType.Add(
                new XElement(
                    "{http://schemas.microsoft.com/ado/2009/11/edm}Member",
                    new XAttribute("Name", "LegacyType"),
                    new XAttribute("Value", 0)));

            Assert.Equal(
                "MessageModel.Message:MessageModel.Message",
                GetMappedType(oSpaceCsdl, EnumCsdl(), "MessageModel.Message", isPOCO).Identity);
        }

        private static void Can_map_CSpace_enum_type_with_no_enum_members(bool isPOCO)
        {
            var cSpaceCsdl = EnumCsdl();

            cSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}EnumType")
                .Single(e => (string)e.Attribute("Name") == "MessageType")
                .Elements()
                .Remove();

            Assert.Equal(
                "MessageModel.Message:MessageModel.Message",
                GetMappedType(EnumCsdl(), cSpaceCsdl, "MessageModel.Message", isPOCO).Identity);
        }

        private static void Cannot_map_if_OSpace_enum_type_member_value_does_not_match_CSpace_enum_type_member_value(bool isPOCO)
        {
            var oSpaceCsdl = EnumCsdl();

            oSpaceCsdl
                .Descendants("{http://schemas.microsoft.com/ado/2009/11/edm}Member")
                .Single(m => (string)m.Attribute("Name") == "Ground" && (string)m.Parent.Attribute("Name") == "MessageType")
                .SetAttributeValue("Value", "64");

            GetMappedType(oSpaceCsdl, EnumCsdl(), "MessageModel.MessageType", isPOCO);
        }

        private static void OSpace_enum_type_and_CSpace_entity_type_have_the_same_name(bool isPOCO)
        {
            var oSpaceCsdl = XDocument.Parse(
                @"<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""Model"">
  <EnumType Name=""MessageType"" IsFlags=""false"" />
</Schema>");

            var cSpaceCsdl = XDocument.Parse(
                @"<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""Model"">
  <EntityType Name=""MessageType"">
    <Key>
      <PropertyRef Name=""Id"" />
    </Key>
    <Property Name=""Id"" Nullable=""false"" Type=""Int32"" />
  </EntityType>
</Schema>");

            GetMappedType(oSpaceCsdl, cSpaceCsdl, "Model.MessageType", isPOCO);
        }

        private static void OSpace_entity_type_and_CSpace_enum_type_have_the_same_name(bool isPOCO)
        {
            var oSpaceCsdl = XDocument.Parse(
                @"<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""Model"">
  <EntityType Name=""MessageType"">
    <Key>
      <PropertyRef Name=""Id"" />
    </Key>
    <Property Name=""Id"" Nullable=""false"" Type=""Int32"" />
  </EntityType>
</Schema>");

            var cSpaceCsdl = XDocument.Parse(
                @"<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""Model"">
  <EnumType Name=""MessageType"" IsFlags=""false"" />
</Schema>");

            GetMappedType(oSpaceCsdl, cSpaceCsdl, "Model.MessageType", isPOCO);
        }

        private static MappingBase GetMappedType(XDocument oSpaceCsdl, XDocument cSpaceCsdl, string oSpaceTypeName, bool isPOCO)
        {
            var workspace = PrepareModel(oSpaceCsdl, cSpaceCsdl, isPOCO);
            return workspace.GetMap(oSpaceTypeName, DataSpace.OSpace, DataSpace.OCSpace);
        }

        private static Assembly BuildAssembly(bool isPOCO, params XDocument[] oSpaceCsdl)
        {
            return
                new CsdlToClrAssemblyConverter(isPOCO, oSpaceCsdl)
                    .BuildAssembly(Guid.NewGuid().ToString());
        }

        private static MetadataWorkspace PrepareModel(XDocument oSpaceCsdl, XDocument cSpaceCsdl, bool isPOCO)
        {
            return CreateMetadataWorkspace(
                cSpaceCsdl,
                BuildAssembly(isPOCO, oSpaceCsdl),
                isPOCO);
        }

        private static MetadataWorkspace CreateMetadataWorkspace(XDocument cSpaceCsdl, Assembly assembly, bool isPOCO)
        {

            EdmItemCollection edmItemCollection;
            using (var csdlReader = cSpaceCsdl.CreateReader())
            {
                edmItemCollection = new EdmItemCollection(new[] { csdlReader });
            }

            // assembly can actually be an AssemblyBuilder. The following line ensures that we are 
            // using the actual assembly otherwise an Assert in ObjectItemAttributeAssemblyLoader.LoadType
            // will fire.
            assembly = assembly.GetTypes().First().Assembly();
            var objectItemCollection = new ObjectItemCollection();

            if (isPOCO)
            {
                objectItemCollection.LoadFromAssembly(assembly, edmItemCollection);
            }
            else
            {
                objectItemCollection.LoadFromAssembly(assembly);
            }

            var workspace = new MetadataWorkspace(
                () => edmItemCollection,
                () => null,
                () => null,
                () => objectItemCollection);

            return workspace;
        }
    }
}
