// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Validation
{
    using System;
    using Xunit;

    public class EscherAttributeContentValidatorTests
    {
        [Fact]
        public void IsValidAttributeValue_returns_false_if_the_value_contains_invalid_xml_characters()
        {
            Assert.False(EscherAttributeContentValidator.IsValidXmlAttributeValue("\u0000"));
        }

        [Fact]
        public void IsValidAttributeValue_returns_true_if_the_value_contains_only_valid_xml_characters()
        {
            Assert.True(EscherAttributeContentValidator.IsValidXmlAttributeValue("<>&AAA"));
        }

        [Fact]
        public void IsValidCsdlNamespaceName_returns_true_for_valid_Csdl_namespace()
        {
            Assert.True(EscherAttributeContentValidator.IsValidCsdlNamespaceName("Model1.Namespace.Edm"));
            Assert.True(EscherAttributeContentValidator.IsValidCsdlNamespaceName("Model1NamespaceEdm"));
            Assert.True(EscherAttributeContentValidator.IsValidCsdlNamespaceName(new string('a', 512)));
        }

        [Fact]
        public void IsValidCsdlNamespaceName_returns_false_for_invalid_Csdl_namespace()
        {
            Assert.False(EscherAttributeContentValidator.IsValidCsdlNamespaceName(new string('a', 513)));
            Assert.False(EscherAttributeContentValidator.IsValidCsdlNamespaceName("Name\u0000space"));
            Assert.False(EscherAttributeContentValidator.IsValidCsdlNamespaceName(""));
            Assert.False(EscherAttributeContentValidator.IsValidCsdlNamespaceName(".Namespace"));
            Assert.False(EscherAttributeContentValidator.IsValidCsdlNamespaceName("Namespace."));
        }

        [Fact]
        public void IsValidCsdlEntityContainerName_returns_true_for_valid_container_name()
        {
            NameVerificationReturnsTrueForFunction(
                EscherAttributeContentValidator.IsValidCsdlEntityContainerName);
        }

        [Fact]
        public void IsValidCsdlEntityContainerName_returns_false_for_invalid_container_name()
        {
            NameVerificationReturnsFalseForFunction(
                EscherAttributeContentValidator.IsValidCsdlEntityContainerName);
        }

        [Fact]
        public void IsValidCsdlEntitySetName_returns_true_for_valid_entityset_name()
        {
            NameVerificationReturnsTrueForFunction(
                EscherAttributeContentValidator.IsValidCsdlEntitySetName);
        }

        [Fact]
        public void IsValidCsdlEntitySetName_returns_false_for_invalid_entityset_name()
        {
            NameVerificationReturnsFalseForFunction(
                EscherAttributeContentValidator.IsValidCsdlEntitySetName);
        }

        [Fact]
        public void IsValidCsdlEntityTypeName_returns_true_for_valid_entity_type_name()
        {
            NameVerificationReturnsTrueForFunction(
                EscherAttributeContentValidator.IsValidCsdlEntityTypeName);
        }

        [Fact]
        public void IsValidCsdlEntityTypeName_returns_false_for_invalid_entity_type_name()
        {
            NameVerificationReturnsFalseForFunction(
                EscherAttributeContentValidator.IsValidCsdlEntityTypeName);
        }

        [Fact]
        public void IsValidCsdlComplexTypeName_returns_true_for_valid_complex_type_name()
        {
            NameVerificationReturnsTrueForFunction(
                EscherAttributeContentValidator.IsValidCsdlComplexTypeName);
        }

        [Fact]
        public void IsValidCsdlComplexTypeName_returns_false_for_invalid_complex_type_name()
        {
            NameVerificationReturnsFalseForFunction(
                EscherAttributeContentValidator.IsValidCsdlComplexTypeName);
        }

        [Fact]
        public void IsValidCsdlEnumTypeName_returns_true_for_valid_enum_type_name()
        {
            NameVerificationReturnsTrueForFunction(
                EscherAttributeContentValidator.IsValidCsdlEnumTypeName);
        }

        [Fact]
        public void IsValidCsdlEnumTypeName_returns_false_for_invalid_enum_type_name()
        {
            NameVerificationReturnsFalseForFunction(
                EscherAttributeContentValidator.IsValidCsdlEnumTypeName);
        }

        [Fact]
        public void IsValidCsdlEnumMemberName_returns_true_for_valid_enum_member_name()
        {
            NameVerificationReturnsTrueForFunction(
                EscherAttributeContentValidator.IsValidCsdlEnumMemberName);
        }

        [Fact]
        public void IsValidCsdlEnumMemberName_returns_false_for_invalid_enum_member_name()
        {
            NameVerificationReturnsFalseForFunction(
                EscherAttributeContentValidator.IsValidCsdlEnumMemberName);
        }

        [Fact]
        public void IsValidCsdlPropertyName_returns_true_for_valid_property_name()
        {
            NameVerificationReturnsTrueForFunction(
                EscherAttributeContentValidator.IsValidCsdlPropertyName);
        }

        [Fact]
        public void IsValidCsdlPropertyName_returns_false_for_invalid_property_name()
        {
            NameVerificationReturnsFalseForFunction(
                EscherAttributeContentValidator.IsValidCsdlPropertyName);
        }

        [Fact]
        public void IsValidCsdlNavigationPropertyName_returns_true_for_valid_navigation_property_name()
        {
            NameVerificationReturnsTrueForFunction(
                EscherAttributeContentValidator.IsValidCsdlNavigationPropertyName);
        }

        [Fact]
        public void IsValidCsdlNavigationPropertyName_returns_false_for_invalid_navigation_property_name()
        {
            NameVerificationReturnsFalseForFunction(
                EscherAttributeContentValidator.IsValidCsdlNavigationPropertyName);
        }

        [Fact]
        public void IsValidCsdlAssociationName_returns_true_for_valid_association_name()
        {
            NameVerificationReturnsTrueForFunction(
                EscherAttributeContentValidator.IsValidCsdlAssociationName);
        }

        [Fact]
        public void IsValidCsdlAssociationName_returns_false_for_invalid_association_name()
        {
            NameVerificationReturnsFalseForFunction(
                EscherAttributeContentValidator.IsValidCsdlAssociationName);
        }

        [Fact]
        public void IsValidCsdlFunctionImportName_returns_true_for_valid_function_import_name()
        {
            NameVerificationReturnsTrueForFunction(
                EscherAttributeContentValidator.IsValidCsdlFunctionImportName);
        }

        [Fact]
        public void IsValidCsdlFunctionImportName_returns_false_for_invalid_function_import_name()
        {
            NameVerificationReturnsFalseForFunction(
                EscherAttributeContentValidator.IsValidCsdlFunctionImportName);
        }

        private static void NameVerificationReturnsTrueForFunction(Func<string, bool> nameVerificationFunc)
        {
            Assert.True(nameVerificationFunc(new string('c', 480)));
        }

        private static void NameVerificationReturnsFalseForFunction(Func<string, bool> nameVerificationFunc)
        {
            Assert.False(nameVerificationFunc(string.Empty));
            Assert.False(nameVerificationFunc(new string('c', 481)));
            Assert.False(nameVerificationFunc("na\0000me"));
            Assert.False(nameVerificationFunc(".name"));
            Assert.False(nameVerificationFunc("na.me"));
        }
    }
}
