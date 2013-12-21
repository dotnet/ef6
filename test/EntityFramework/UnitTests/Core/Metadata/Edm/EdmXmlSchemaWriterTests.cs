// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Text;
    using System.Xml;
    using Moq;
    using Xunit;

    public class EdmXmlSchemaWriterTests
    {
        [Fact]
        public void WriteFunctionElementHeader_should_write_element_and_attributes()
        {
            var fixture = new Fixture();

            var function = new EdmFunction(
                "Foo",
                "Bar",
                DataSpace.SSpace,
                new EdmFunctionPayload
                    {
                        Schema = "dbo"
                    });

            fixture.Writer.WriteFunctionElementHeader(function);

            Assert.Equal(
                "<Function Name=\"Foo\" Aggregate=\"false\" BuiltIn=\"false\" NiladicFunction=\"false\" IsComposable=\"true\" ParameterTypeSemantics=\"AllowImplicitConversion\" Schema=\"dbo\"",
                fixture.ToString());
        }

        [Fact]
        public void WriteFunctionElementHeader_should_write_return_type_attribute_for_primitive_return_type()
        {
            var fixture = new Fixture();

            var function = new EdmFunction(
                "Foo",
                "Bar",
                DataSpace.SSpace,
                new EdmFunctionPayload
                    {
                        Schema = "dbo",
                        ReturnParameters =
                            new[]
                                {
                                    new FunctionParameter(
                                        "r",
                                        TypeUsage.CreateDefaultTypeUsage(
                                            PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                                        ParameterMode.ReturnValue)
                                }
                    });

            fixture.Writer.WriteFunctionElementHeader(function);

            Assert.Equal(
                "<Function Name=\"Foo\" Aggregate=\"false\" BuiltIn=\"false\" NiladicFunction=\"false\" IsComposable=\"true\" ParameterTypeSemantics=\"AllowImplicitConversion\" Schema=\"dbo\" ReturnType=\"Int32\"",
                fixture.ToString());
        }

        [Fact]
        public void WriteFunctionElementHeader_should_not_write_return_type_attribute_for_non_primitive_return_type()
        {
            var fixture = new Fixture();

            var returnParameterType = TypeUsage.CreateDefaultTypeUsage(new RowType());

            var function = new EdmFunction(
                "Foo",
                "Bar",
                DataSpace.SSpace,
                new EdmFunctionPayload
                    {
                        Schema = "dbo",
                        ReturnParameters =
                            new[]
                                {
                                    new FunctionParameter(
                                        "r",
                                        returnParameterType,
                                        ParameterMode.ReturnValue)
                                }
                    });

            fixture.Writer.WriteFunctionElementHeader(function);

            Assert.Equal(
                "<Function Name=\"Foo\" Aggregate=\"false\" BuiltIn=\"false\" NiladicFunction=\"false\" IsComposable=\"true\" ParameterTypeSemantics=\"AllowImplicitConversion\" Schema=\"dbo\"",
                fixture.ToString());
        }

        [Fact]
        public void WriteFunctionElementHeader_should_write_store_function_name_if_specified()
        {
            var fixture = new Fixture();

            var function = new EdmFunction(
                "Foo",
                "Bar",
                DataSpace.SSpace,
                new EdmFunctionPayload
                {
                    Schema = "dbo",
                    StoreFunctionName = "Not Foo"
                });

            fixture.Writer.WriteFunctionElementHeader(function);

            Assert.Equal(
                "<Function Name=\"Foo\" Aggregate=\"false\" BuiltIn=\"false\" NiladicFunction=\"false\" IsComposable=\"true\" ParameterTypeSemantics=\"AllowImplicitConversion\" Schema=\"dbo\" StoreFunctionName=\"Not Foo\"",
                fixture.ToString());
        }

        [Fact]
        public void WriteFunctionElementHeader_should_not_write_store_function_name_if_equal_to_edm_function_name()
        {
            var fixture = new Fixture();

            var function = new EdmFunction(
                "Foo",
                "Bar",
                DataSpace.SSpace,
                new EdmFunctionPayload
                {
                    Schema = "dbo",
                    StoreFunctionName = "Foo"
                });

            fixture.Writer.WriteFunctionElementHeader(function);

            Assert.Equal(
                "<Function Name=\"Foo\" Aggregate=\"false\" BuiltIn=\"false\" NiladicFunction=\"false\" IsComposable=\"true\" ParameterTypeSemantics=\"AllowImplicitConversion\" Schema=\"dbo\"",
                fixture.ToString());
        }

        [Fact]
        public void WriteFunctionParameterHeader_should_write_element_and_attributes()
        {
            var fixture = new Fixture();

            var typeUsage
                = TypeUsage.Create(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String),
                    new FacetValues
                        {
                            MaxLength = 123
                        });

            var functionParameter
                = new FunctionParameter(
                    "Foo",
                    typeUsage,
                    ParameterMode.In);

            fixture.Writer.WriteFunctionParameterHeader(functionParameter);

            Assert.Equal(
                "<Parameter Name=\"Foo\" Type=\"String\" Mode=\"In\" MaxLength=\"123\"",
                fixture.ToString());

            fixture = new Fixture();

            typeUsage
                = TypeUsage.Create(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Decimal),
                    new FacetValues
                        {
                            Precision = (byte?)4,
                            Scale = (byte?)8
                        });

            functionParameter
                = new FunctionParameter(
                    "Foo",
                    typeUsage,
                    ParameterMode.In);

            fixture.Writer.WriteFunctionParameterHeader(functionParameter);

            Assert.Equal(
                "<Parameter Name=\"Foo\" Type=\"Decimal\" Mode=\"In\" Precision=\"4\" Scale=\"8\"",
                fixture.ToString());
        }

        [Fact]
        public void WriteFunctionImportHeader_writes_element_and_attributes()
        {
            var fixture = new Fixture();

            var complexType = new ComplexType("CT", "N", DataSpace.CSpace);

            var returnParameter =
                new FunctionParameter(
                    "ReturnValue",
                    TypeUsage.CreateDefaultTypeUsage(complexType.GetCollectionType()),
                    ParameterMode.ReturnValue);

            var functionPayload =
                new EdmFunctionPayload
                    {
                        IsComposable = true,
                        ReturnParameters = new[] { returnParameter },
                    };

            var functionImport
                = new EdmFunction("foo", "nction", DataSpace.CSpace, functionPayload);

            fixture.Writer.WriteFunctionImportElementHeader(functionImport);

            Assert.Equal(
                "<FunctionImport Name=\"foo\" ReturnType=\"Collection(N.CT)\" IsComposable=\"true\"",
                fixture.ToString());
        }

        [Fact]
        public void WriteFunctionImportHeader_IsComposable_attribute_if_it_is_false()
        {
            var fixture = new Fixture();

            var returnParameter =
                new FunctionParameter(
                    "ReturnValue",
                    TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                    ParameterMode.ReturnValue);

            var functionPayload =
                new EdmFunctionPayload
                    {
                        IsComposable = false,
                        ReturnParameters = new[] { returnParameter },
                    };

            var functionImport
                = new EdmFunction("foo", "nction", DataSpace.CSpace, functionPayload);

            fixture.Writer.WriteFunctionImportElementHeader(functionImport);

            Assert.Equal(
                "<FunctionImport Name=\"foo\" ReturnType=\"Int32\"",
                fixture.ToString());
        }

        [Fact]
        public void WriteFunctionImportParameterElement_writes_full_element_with_attributes()
        {
            var fixture = new Fixture();

            var functionImportParameter =
                new FunctionParameter(
                    "miles",
                    TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                    ParameterMode.InOut);

            fixture.Writer.WriteFunctionImportParameterElementHeader(functionImportParameter);

            Assert.Equal(
                "<Parameter Name=\"miles\" Mode=\"InOut\" Type=\"Int32\"",
                fixture.ToString());
        }

        [Fact]
        public void WriteFunctionReturnTypeElementHeader_writes_header()
        {
            var fixture = new Fixture();

            fixture.Writer.WriteFunctionReturnTypeElementHeader();

            Assert.Equal("<ReturnType", fixture.ToString());
        }

        [Fact]
        public void WriteCollectionTypeElementHeader_writes_header()
        {
            var fixture = new Fixture();

            fixture.Writer.WriteCollectionTypeElementHeader();

            Assert.Equal("<CollectionType", fixture.ToString());
        }

        [Fact]
        public void WriteRowTypeElementHeader_writes_header()
        {
            var fixture = new Fixture();

            fixture.Writer.WriteRowTypeElementHeader();

            Assert.Equal("<RowType", fixture.ToString());
        }

        [Fact]
        public void WriteSchemaElementHeader_writes_annotation_prefix_for_UseStrongSpatialTypes_attribute()
        {
            var fixture = new Fixture();
            fixture.Writer.WriteSchemaElementHeader("test");

            Assert.Equal(@"<Schema Namespace=""test"" Alias=""Self"" annotation:UseStrongSpatialTypes=""false"" xmlns:annotation=""http://schemas.microsoft.com/ado/2009/02/edm/annotation"" xmlns:customannotation=""http://schemas.microsoft.com/ado/2013/11/edm/customannotation""", 
            fixture.ToString());
        }
        
        [Fact]
        public void WriteSchemaElementHeader_writes_annotation_namespace_declaration_if_UseStrongSpatialTypes_not_present()
        {
            var fixture = new Fixture(2.0);
            fixture.Writer.WriteSchemaElementHeader("test");

            Assert.Equal(@"<Schema Namespace=""test"" Alias=""Self"" xmlns:annotation=""http://schemas.microsoft.com/ado/2009/02/edm/annotation"" xmlns:customannotation=""http://schemas.microsoft.com/ado/2013/11/edm/customannotation""",
            fixture.ToString());
        }

        [Fact]
        public void WriteSchemaElementHeader_writes_StoreSchemaGen_namespace_declaration_if_requested()
        {
            var fixture = new Fixture(2.0);
            fixture.Writer.WriteSchemaElementHeader("Room.Store", "fakeProvider", "42", true);

            Assert.Equal(@"<Schema Namespace=""Room.Store"" Provider=""fakeProvider"" ProviderManifestToken=""42"" Alias=""Self"" xmlns:store=""http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator"" xmlns:customannotation=""http://schemas.microsoft.com/ado/2013/11/edm/customannotation""",
            fixture.ToString());
        }

        [Fact]
        public void WriteSchemaElementHeader_does_not_write_StoreSchemaGen_namespace_declaration_if_not_requested()
        {
            var fixture = new Fixture(2.0);
            fixture.Writer.WriteSchemaElementHeader("Room.Store", "fakeProvider", "42", false);

            Assert.Equal(@"<Schema Namespace=""Room.Store"" Provider=""fakeProvider"" ProviderManifestToken=""42"" Alias=""Self"" xmlns:customannotation=""http://schemas.microsoft.com/ado/2013/11/edm/customannotation""",
            fixture.ToString());
        }

        [Fact]
        public void WriteContainer_writes_annotation_namespace_when_requested()
        {
            var container = EntityContainer.Create(
                "C", DataSpace.CSpace, new EntitySetBase[0], null,
                new[]
                    {
                        new MetadataProperty(
                            "http://schemas.microsoft.com/ado/2009/02/edm/annotation:LazyLoadingEnabled",
                            TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)), "true")
                    });

            var fixture = new Fixture();

            // WriteSchemaElementHeader binds the "annotation" namespace prefix to the annotation namespace uri
            fixture.Writer.WriteSchemaElementHeader("ns");
            fixture.Writer.WriteEntityContainerElementHeader(container);

            Assert.Equal(
                @"<Schema Namespace=""ns"" Alias=""Self"" annotation:UseStrongSpatialTypes=""false"" xmlns:annotation=""http://schemas.microsoft.com/ado/2009/02/edm/annotation"" xmlns:customannotation=""http://schemas.microsoft.com/ado/2013/11/edm/customannotation"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">" +
                @"<EntityContainer Name=""C"" annotation:LazyLoadingEnabled=""true""",
            fixture.ToString());
        }

        [Fact]
        public void WriteEntityTypeElementHeader_writes_annotations()
        {
            var property = MetadataProperty.CreateAnnotation(XmlConstants.ClrTypeAnnotationWithPrefix, typeof(Random));

            var mockEntityType = new Mock<EntityType>("E", "ns", DataSpace.CSpace);
            mockEntityType.Setup(m => m.Name).Returns("E");
            mockEntityType.Setup(m => m.MetadataProperties).Returns(
                new ReadOnlyMetadataCollection<MetadataProperty>(new List<MetadataProperty>() { property }));

            var fixture = new Fixture(2.0);
            fixture.Writer.WriteEntityTypeElementHeader(mockEntityType.Object);

            Assert.Equal(
                @"<EntityType Name=""E"" p1:ClrType=""System.Random, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089""",
                fixture.ToString());
        }

        [Fact]
        public void WriteComplexTypeElementHeader_writes_annotations()
        {
            var property = MetadataProperty.CreateAnnotation(XmlConstants.ClrTypeAnnotationWithPrefix, typeof(Random));

            var mockComplexType = new Mock<ComplexType>("C", "ns", DataSpace.CSpace);
            mockComplexType.Setup(m => m.Name).Returns("C");
            mockComplexType.Setup(m => m.MetadataProperties).Returns(
                new ReadOnlyMetadataCollection<MetadataProperty>(new List<MetadataProperty>() { property }));

            var fixture = new Fixture(2.0);
            fixture.Writer.WriteComplexTypeElementHeader(mockComplexType.Object);

            Assert.Equal(
                @"<ComplexType Name=""C"" p1:ClrType=""System.Random, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089""",
                fixture.ToString());
        }

        [Fact]
        public void WriteEnumTypeElementHeader_writes_annotations()
        {
            var property = MetadataProperty.CreateAnnotation(XmlConstants.ClrTypeAnnotationWithPrefix, typeof(Random));

            var mockEnumType = new Mock<EnumType>();
            mockEnumType.Setup(m => m.Name).Returns("E");
            mockEnumType.Setup(m => m.MetadataProperties).Returns(
                new ReadOnlyMetadataCollection<MetadataProperty>(new List<MetadataProperty>() { property }));

            var fixture = new Fixture(2.0);
            fixture.Writer.WriteEnumTypeElementHeader(mockEnumType.Object);

            Assert.Equal(
                @"<EnumType Name=""E"" IsFlags=""false"" p1:ClrType=""System.Random, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"" UnderlyingType=""Int32""",
                fixture.ToString());
        }
        
        [Fact]
        public void WriteExtendedProperties_writes_annotations_using_serializer_to_serialize_if_available()
        {
            var property = MetadataProperty.CreateAnnotation(XmlConstants.ClrTypeAnnotationWithPrefix, typeof(Random));

            var mockEntityType = new Mock<EntityType>("E", "ns", DataSpace.CSpace);
            mockEntityType.Setup(m => m.MetadataProperties).Returns(
                new ReadOnlyMetadataCollection<MetadataProperty>(new List<MetadataProperty>() { property }));

            var fixture = new Fixture(2.0);
            fixture.UnderlyingWriter.WriteStartElement(XmlConstants.EntityType);
            fixture.Writer.WriteExtendedProperties(mockEntityType.Object);

            Assert.Equal(
                @"<EntityType p1:ClrType=""System.Random, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089""",
                fixture.ToString());
        }

        [Fact]
        public void WriteExtendedProperties_writes_annotations_using_ToString_if_serializer_not_available()
        {
            var property = MetadataProperty.CreateAnnotation(XmlConstants.UseClrTypesAnnotationWithPrefix, "true");

            var mockEntityType = new Mock<EntityType>("E", "ns", DataSpace.CSpace);
            mockEntityType.Setup(m => m.MetadataProperties).Returns(
                new ReadOnlyMetadataCollection<MetadataProperty>(new List<MetadataProperty>() { property }));

            var fixture = new Fixture(2.0);
            fixture.UnderlyingWriter.WriteStartElement(XmlConstants.EntityType);
            fixture.Writer.WriteExtendedProperties(mockEntityType.Object);

            Assert.Equal(@"<EntityType p1:UseClrTypes=""true""", fixture.ToString());
        }

        [Fact]
        public void WriteExtendedProperties_does_not_write_annotations_that_do_not_have_a_name_containing_a_colon()
        {
            var property = MetadataProperty.CreateAnnotation("Foo", "true");

            var mockEntityType = new Mock<EntityType>("E", "ns", DataSpace.CSpace);
            mockEntityType.Setup(m => m.MetadataProperties).Returns(
                new ReadOnlyMetadataCollection<MetadataProperty>(new List<MetadataProperty>() { property }));

            var fixture = new Fixture(2.0);
            fixture.UnderlyingWriter.WriteStartElement(XmlConstants.EntityType);
            fixture.Writer.WriteExtendedProperties(mockEntityType.Object);

            Assert.Equal(@"<EntityType", fixture.ToString());
        }

        [Fact]
        public void WriteExtendedProperties_writes_annotations_on_properties()
        {
            var annotation = MetadataProperty.CreateAnnotation(XmlConstants.CustomAnnotationPrefix + "Foo", "Goo");

            var mockProperty = new Mock<MetadataProperty>();
            mockProperty.Setup(m => m.MetadataProperties).Returns(
                new ReadOnlyMetadataCollection<MetadataProperty>(new List<MetadataProperty>() { annotation }));

            var fixture = new Fixture(2.0);
            fixture.UnderlyingWriter.WriteStartElement(XmlConstants.Property);
            fixture.Writer.WriteExtendedProperties(mockProperty.Object);

            Assert.Equal(@"<Property p1:Foo=""Goo""", fixture.ToString());
        }

        [Fact]
        public void WriteExtendedProperties_does_not_write_store_generated_annotations()
        {
            var annotation = MetadataProperty.CreateAnnotation(XmlConstants.StoreGeneratedPatternAnnotation, "Identity");

            var mockProperty = new Mock<MetadataProperty>();
            mockProperty.Setup(m => m.MetadataProperties).Returns(
                new ReadOnlyMetadataCollection<MetadataProperty>(new List<MetadataProperty>() { annotation }));

            var fixture = new Fixture(2.0);
            fixture.UnderlyingWriter.WriteStartElement(XmlConstants.Property);
            fixture.Writer.WriteExtendedProperties(mockProperty.Object);

            Assert.Equal(@"<Property", fixture.ToString());
        }

        private class Fixture
        {
            public readonly EdmXmlSchemaWriter Writer;
            public readonly XmlWriter UnderlyingWriter;

            private readonly StringBuilder _stringBuilder;

            public Fixture(double version = 3.0)
            {
                _stringBuilder = new StringBuilder();

                UnderlyingWriter = XmlWriter.Create(
                    _stringBuilder,
                    new XmlWriterSettings
                        {
                            OmitXmlDeclaration = true
                        });

                Writer = new EdmXmlSchemaWriter(UnderlyingWriter, version, false);
            }

            public override string ToString()
            {
                UnderlyingWriter.Flush();

                return _stringBuilder.ToString();
            }
        }
    }
}
