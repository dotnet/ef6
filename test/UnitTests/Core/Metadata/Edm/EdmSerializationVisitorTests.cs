// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Moq;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm.Provider;
    using System.Data.Entity.Resources;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using Xunit;

    public class EdmSerializationVisitorTests
    {
        [Fact]
        public void EdmSerializationVisitor_writes_extended_annotations_for_entity_set()
        {
            var entityType = new EntityType("MyEntity", "Model", DataSpace.SSpace);
            var entitySet = new EntitySet("Entities", null, "Entities", null, entityType);
            entitySet.AddMetadataProperties(
                CreateMetadataProperties(new[] { new KeyValuePair<string, string>("http://tempuri.org:extended-property", "value") }));

            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                new EdmSerializationVisitor(writer, 3.0).VisitEdmEntitySet(entitySet);
            }

            var xml = XDocument.Parse(sb.ToString());
            Assert.Equal(
                "value",
                (string)xml.Root.Attribute("{http://tempuri.org}extended-property"));
        }

        [Fact]
        public void StoreSchemaGen_attributes_written_using_store_namespace_prefix_and_without_namespace_declaration_on_the_element()
        {
            var entityType = new EntityType("MyEntity", "Model", DataSpace.SSpace);
            var entitySet = new EntitySet("Entities", null, "Entities", null, entityType);
            entitySet.AddMetadataProperties(
                CreateMetadataProperties(
                    new[] { new KeyValuePair<string, string>(XmlConstants.EntityStoreSchemaGeneratorNamespace + ":property", "value") }));

            var container = new EntityContainer("Container.Store", DataSpace.SSpace);
            container.AddEntitySetBase(entitySet);
            var model = EdmModel.CreateStoreModel(container, null, null);


            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                new EdmSerializationVisitor(writer, 3.0).Visit(model, "ns", "A", "b");
            }

            Assert.Contains(
                "<EntitySet Name=\"Entities\" EntityType=\"Self.MyEntity\" Table=\"Entities\" store:property=\"value\" />",
                sb.ToString());
        }

        [Fact]
        public void EdmSerializationVisitor_does_not_write_incorrectly_named_extended_properties()
        {
            var incorrectNames =
                new[]
                    {
                        "extended-property-without-namespace",
                        ":extended-property-starts-with-colon",
                        "extended-property-ends-with-colon:"
                    };

            var entityType = new EntityType("MyEntity", "Model", DataSpace.SSpace);
            var entitySet = new EntitySet("Entities", null, "Entities", null, entityType);

            entitySet.AddMetadataProperties(
                CreateMetadataProperties(
                    incorrectNames.Select(n => new KeyValuePair<string, string>(n, ""))));

            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                new EdmSerializationVisitor(writer, 3.0).VisitEdmEntitySet(entitySet);
            }

            var xml = XDocument.Parse(sb.ToString());
            Assert.False(xml.Root.Attributes().Any(a => incorrectNames.Contains(a.Name.LocalName)));
            Assert.False(xml.Root.Attributes().Any(a => a.Name.Namespace != XNamespace.None));
        }

        private static List<MetadataProperty> CreateMetadataProperties(IEnumerable<KeyValuePair<string, string>> nameValuePairs)
        {
            var edmString = EdmProviderManifest.Instance.GetPrimitiveType(PrimitiveTypeKind.String);
            var metadataProperties = new List<MetadataProperty>();
            foreach (var nameValuePair in nameValuePairs)
            {
                metadataProperties.Add(
                    new MetadataProperty(
                        nameValuePair.Key,
                        TypeUsage.CreateDefaultTypeUsage(edmString),
                        nameValuePair.Value));
            }
            return metadataProperties;
        }

        [Fact]
        public void Visit_should_write_store_schema_generator_namespace_on_Schema_if_entity_sets_use_it()
        {
            var entityType = new EntityType("MyEntity", "Model", DataSpace.SSpace);
            var entitySet = new EntitySet("Entities", null, "Entities", null, entityType);
            entitySet.AddMetadataProperties(
                CreateMetadataProperties(
                    new[] { new KeyValuePair<string, string>(XmlConstants.EntityStoreSchemaGeneratorNamespace + ":property", "value") }));

            var container = new EntityContainer("Container.Store", DataSpace.SSpace);
            container.AddEntitySetBase(entitySet);
            var model = EdmModel.CreateStoreModel(container, null, null);

            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>();

            new EdmSerializationVisitor(schemaWriterMock.Object).Visit(model, "fakeProvider", "42");

            schemaWriterMock.Verify(sw => sw.WriteSchemaElementHeader(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true), Times.Once());
            schemaWriterMock.Verify(sw => sw.WriteSchemaElementHeader(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), false), Times.Never());
        }

        [Fact]
        public void Visit_should_not_write_store_schema_generator_namespace_on_Schema_if_entity_sets_do_not_use_it()
        {
            var entityType = new EntityType("MyEntity", "Model", DataSpace.SSpace);
            var entitySet = new EntitySet("Entities", null, "Entities", null, entityType);
            var container = new EntityContainer("Container.Store", DataSpace.SSpace);
            container.AddEntitySetBase(entitySet);
            var model = EdmModel.CreateStoreModel(container, null, null);

            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>();

            new EdmSerializationVisitor(schemaWriterMock.Object).Visit(model, "fakeProvider", "42");

            schemaWriterMock.Verify(sw => sw.WriteSchemaElementHeader(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true), Times.Never());
            schemaWriterMock.Verify(sw => sw.WriteSchemaElementHeader(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), false), Times.Once());
        }

        [Fact]
        public void EdmSerializationVisitor_writes_defining_query_for_entity_set()
        {
            var entityType = new EntityType("MyEntity", "Model", DataSpace.SSpace);
            var entitySet = new EntitySet("Entities", null, "Entities", "Defining Query", entityType);

            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                new EdmSerializationVisitor(writer, 3.0).VisitEdmEntitySet(entitySet);
            }

            var xml = XDocument.Parse(sb.ToString());
            Assert.Equal(
                "Defining Query",
                (string)xml.Root.Element("DefiningQuery"));
        }

        [Fact]
        public void VisitEdmFunction_should_write_start_and_end_elements()
        {
            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>();
            var function = new EdmFunction("F", "N", DataSpace.SSpace);

            new EdmSerializationVisitor(schemaWriterMock.Object).VisitEdmFunction(function);

            schemaWriterMock.Verify(sw => sw.WriteFunctionElementHeader(function), Times.Once());
            schemaWriterMock.Verify(sw => sw.WriteEndElement(), Times.Once());
        }

        [Fact]
        public void VisitFunctionParameter_should_write_start_and_end_elements()
        {
            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>();
            var functionParameter = new FunctionParameter();

            new EdmSerializationVisitor(schemaWriterMock.Object).VisitFunctionParameter(functionParameter);

            schemaWriterMock.Verify(sw => sw.WriteFunctionParameterHeader(functionParameter), Times.Once());
            schemaWriterMock.Verify(sw => sw.WriteEndElement(), Times.Once());
        }

        [Fact]
        public void VisitFunctionImport_writes_start_and_end_elements()
        {
            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>();
            var functionImport = new EdmFunction("foo", "bar", DataSpace.CSpace);

            new EdmSerializationVisitor(schemaWriterMock.Object).VisitFunctionImport(functionImport);

            schemaWriterMock.Verify(sw => sw.WriteFunctionImportElementHeader(functionImport), Times.Once());
            schemaWriterMock.Verify(sw => sw.WriteEndElement(), Times.Once());
        }

        [Fact]
        public void VisitFunctionImportParameter_writes_function_parameter()
        {
            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>();
            var functionImport = new FunctionParameter();

            new EdmSerializationVisitor(schemaWriterMock.Object).VisitFunctionImportParameter(functionImport);

            schemaWriterMock.Verify(sw => sw.WriteFunctionImportParameterElementHeader(functionImport), Times.Once());
            schemaWriterMock.Verify(sw => sw.WriteEndElement(), Times.Once());
        }

        [Fact]
        public void VisitFunctionImport_writes_inline_return_type_if_one_return_parameter()
        {
            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>();

            var functionImport =
                new EdmFunction(
                    "f",
                    "entityModel",
                    DataSpace.CSpace,
                    new EdmFunctionPayload
                    {
                        IsComposable = true,
                        EntitySets = new[] { (EntitySet)null },
                        ReturnParameters =
                            new[]
                                    {
                                        new FunctionParameter(
                                            "ReturnType", 
                                            TypeUsage.CreateDefaultTypeUsage(new ComplexType()), 
                                            ParameterMode.ReturnValue)
                                    }
                    });

            new EdmSerializationVisitor(schemaWriterMock.Object).VisitFunctionImport(functionImport);

            schemaWriterMock.Verify(sw => sw.WriteFunctionImportElementHeader(functionImport), Times.Once());
            schemaWriterMock.Verify(
                sw => sw.WriteFunctionImportReturnTypeAttributes(functionImport.ReturnParameter, null, true), Times.Once());
            schemaWriterMock.Verify(sw => sw.WriteFunctionReturnTypeElementHeader(), Times.Never());
            schemaWriterMock.Verify(
                sw => sw.WriteFunctionImportReturnTypeAttributes(It.IsAny<FunctionParameter>(), It.IsAny<EntitySet>(), false), Times.Never());
        }

        [Fact]
        public void VisitFunctionImport_writes_multiple_return_parameters()
        {
            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>();

            var functionImport =
                new EdmFunction(
                    "f",
                    "entityModel",
                    DataSpace.CSpace,
                    new EdmFunctionPayload
                    {
                        IsComposable = true,
                        EntitySets = new[] { null, new EntitySet { Name = "ES"} },
                        ReturnParameters =
                            new[]
                                    {
                                        new FunctionParameter(
                                            "ReturnType1", 
                                            TypeUsage.CreateDefaultTypeUsage(new ComplexType()), 
                                            ParameterMode.ReturnValue),
                                        new FunctionParameter(
                                            "ReturnType2", 
                                            TypeUsage.CreateDefaultTypeUsage(new EntityType("ET", "NS", DataSpace.CSpace)), 
                                            ParameterMode.ReturnValue)
                                    }
                    });

            new EdmSerializationVisitor(schemaWriterMock.Object).VisitFunctionImport(functionImport);

            schemaWriterMock.Verify(sw => sw.WriteFunctionImportElementHeader(functionImport), Times.Once());
            schemaWriterMock.Verify(
                sw => sw.WriteFunctionImportReturnTypeAttributes(functionImport.ReturnParameters[0], null, false), Times.Once());
            schemaWriterMock.Verify(
                sw => sw.WriteFunctionImportReturnTypeAttributes(functionImport.ReturnParameters[1], functionImport.EntitySets[1], false), Times.Once());
            schemaWriterMock.Verify(sw => sw.WriteFunctionReturnTypeElementHeader(), Times.Exactly(2));
            schemaWriterMock.Verify(
                sw => sw.WriteFunctionImportReturnTypeAttributes(It.IsAny<FunctionParameter>(), It.IsAny<EntitySet>(), true), Times.Never());
        }

        [Fact]
        public void VisitFunctionReturnParameter_writes_return_and_collection_header_for_rowtype()
        {
            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>();

            var returnParameter =
                new FunctionParameter(
                    "r", 
                    TypeUsage.CreateDefaultTypeUsage(new RowType().GetCollectionType()), 
                    ParameterMode.ReturnValue);

            new EdmSerializationVisitor(schemaWriterMock.Object).VisitFunctionReturnParameter(returnParameter);

            schemaWriterMock.Verify(sw => sw.WriteFunctionReturnTypeElementHeader(), Times.Once());
            schemaWriterMock.Verify(sw => sw.WriteCollectionTypeElementHeader(), Times.Once());
            schemaWriterMock.Verify(sw => sw.WriteRowTypeElementHeader(), Times.Once());
            schemaWriterMock.Verify(sw => sw.WriteEndElement(), Times.Exactly(3));
        }

        [Fact]
        public void VisitFunctionReturnParameter_writes_return_and_collection_header_for_primitive_return_type()
        {
            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>();
            var returnParameter =
                new FunctionParameter(
                    "r", 
                    TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)), 
                    ParameterMode.ReturnValue);

            new EdmSerializationVisitor(schemaWriterMock.Object).VisitFunctionReturnParameter(returnParameter);

            schemaWriterMock.Verify(sw => sw.WriteFunctionReturnTypeElementHeader(), Times.Never());
            schemaWriterMock.Verify(sw => sw.WriteCollectionTypeElementHeader(), Times.Never());
            schemaWriterMock.Verify(sw => sw.WriteRowTypeElementHeader(), Times.Never());
            schemaWriterMock.Verify(sw => sw.WriteEndElement(), Times.Never());
        }

        [Fact]
        public void Visit_writes_Empty_model_namespace_for_empty_model_if_custom_namespace_not_provided()
        {
            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>(new Mock<XmlWriter>().Object, 3.0, false, null);

            new EdmSerializationVisitor(schemaWriterMock.Object)
                .Visit(new EdmModel(DataSpace.CSpace), null);

            schemaWriterMock.Verify(sw => sw.WriteSchemaElementHeader("Empty"), Times.Once());
        }

        [Fact]
        public void Visit_writes_custom_model_namespace_if_provided()
        {
            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>(new Mock<XmlWriter>().Object, 3.0, false, null);

            new EdmSerializationVisitor(schemaWriterMock.Object)
                .Visit(new EdmModel(DataSpace.CSpace), "NS");

            schemaWriterMock.Verify(sw => sw.WriteSchemaElementHeader("NS"), Times.Once());
        }

        [Fact]
        public void Custom_namespace_overrides_inferred_namespace()
        {
            var model = new EdmModel(DataSpace.CSpace);
            model.AddItem(new ComplexType("foo", "namespace", DataSpace.CSpace));

            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>(new Mock<XmlWriter>().Object, 3.0, false, null);

            new EdmSerializationVisitor(schemaWriterMock.Object)
                .Visit(new EdmModel(DataSpace.CSpace), "NS");

            schemaWriterMock.Verify(sw => sw.WriteSchemaElementHeader("NS"), Times.Once());
        }

        [Fact]
        public void VisitRowType_writes_rowtype()
        {
            var schemaWriterMock = new Mock<EdmXmlSchemaWriter>();
            new EdmSerializationVisitor(schemaWriterMock.Object).VisitRowType(new RowType());

            schemaWriterMock.Verify(sw => sw.WriteRowTypeElementHeader(), Times.Once());
            schemaWriterMock.Verify(sw => sw.WriteEndElement(), Times.Once());
        }

        [Fact]
        public static void VisitEdmEntityType_writes_comment_including_errors_followed_by_valid_entity_type()
        {
            // Need to specify the generic type explicitly to avoid build break on .NET Framework 4
            // ReSharper disable RedundantTypeArgumentsOfMethod
            EdmSerializationVisitor_writes_expected_xml<EntityType>(
                constructor: () => new EntityType("AName", "ANamespace", DataSpace.CSpace),
                invalid: false,
                visitAction: (visitor, item) => visitor.VisitEdmEntityType(item),
                expectedFormat: @"<!--{0}--><EntityType Name=""AName"" />");
            // ReSharper restore RedundantTypeArgumentsOfMethod
        }

        [Fact]
        public static void VisitEdmAssociationType_writes_comment_including_errors_followed_by_valid_association_type()
        {
            // Need to specify the generic type explicitly to avoid build break on .NET Framework 4
            // ReSharper disable RedundantTypeArgumentsOfMethod
            EdmSerializationVisitor_writes_expected_xml<AssociationType>(
                constructor: () => new AssociationType("AName", "ANamespace", false, DataSpace.CSpace),
                invalid: false,
                visitAction: (visitor, item) => visitor.VisitEdmAssociationType(item),
                expectedFormat: @"<!--{0}--><Association Name=""AName"" />");
            // ReSharper restore RedundantTypeArgumentsOfMethod
        }

        [Fact]
        public static void VisitEdmEntityType_writes_comment_including_errors_and_invalid_entity_type()
        {
            // Need to specify the generic type to avoid build break on .NET Framework 4
            // ReSharper disable RedundantTypeArgumentsOfMethod
            EdmSerializationVisitor_writes_expected_xml<EntityType>(
                constructor: () => new EntityType("AName", "ANamespace", DataSpace.CSpace),
                invalid: true,
                visitAction: (visitor, item) => visitor.VisitEdmEntityType(item),
                expectedFormat: @"<!--{0}<EntityType Name=""AName"" />-->");
            // ReSharper restore RedundantTypeArgumentsOfMethod
        }

        [Fact]
        public static void VisitEdmAssociationType_writes_comment_including_errors_and_invalid_association_type()
        {
            // Need to specify the generic type to avoid build break on .NET Framework 4
            // ReSharper disable RedundantTypeArgumentsOfMethod
            EdmSerializationVisitor_writes_expected_xml<AssociationType>(
                constructor: () => new AssociationType("AName", "ANamespace", false, DataSpace.CSpace),
                invalid: true,
                visitAction: (visitor, item) => visitor.VisitEdmAssociationType(item),
                expectedFormat: @"<!--{0}<Association Name=""AName"" />-->");
            // ReSharper restore RedundantTypeArgumentsOfMethods
        }

        private static void EdmSerializationVisitor_writes_expected_xml<T>(
            Func<T> constructor,
            bool invalid,
            Action<EdmSerializationVisitor, T> visitAction,
            string expectedFormat)
            where T : MetadataItem
        {
            var errors = new List<EdmSchemaError>() {
                new EdmSchemaError("Message1.", 1, EdmSchemaErrorSeverity.Error),
                new EdmSchemaError("Message2.", 2, EdmSchemaErrorSeverity.Warning)
            };

            var typeUsage1 = TypeUsage.CreateDefaultTypeUsage(
                PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Boolean));
            var property1 = MetadataProperty.Create(
                MetadataItemHelper.SchemaInvalidMetadataPropertyName, typeUsage1, invalid);

            var typeUsage2 = TypeUsage.CreateDefaultTypeUsage(
                PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String).GetCollectionType());
            var property2 = MetadataProperty.Create(
                MetadataItemHelper.SchemaErrorsMetadataPropertyName, typeUsage2, errors);

            var item = constructor();
            item.AddMetadataProperties(new List<MetadataProperty>() { property1, property2 });

            var builder = new StringBuilder();
            var settings = new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment };

            using (var writer = XmlWriter.Create(builder, settings))
            {
                visitAction(new EdmSerializationVisitor(writer, 3.0), item);
            }

            var errorsString = String.Concat(
                Strings.MetadataItemErrorsFoundDuringGeneration,
                errors[0].ToString(),
                errors[1].ToString());

            var expectedXml = String.Format(
                CultureInfo.InvariantCulture,
                expectedFormat,
                errorsString);

            AssertEqual(expectedXml, builder.ToString());
        }

        private static void AssertEqual(string expected, string actual)
        {
            Assert.Equal(Regex.Replace(expected, @"\s", ""), Regex.Replace(actual, @"\s", ""));
        }
    }
}