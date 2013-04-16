// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Serialization
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Text;
    using System.Xml;
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
        public void WriteFunctionParameterHeader_should_write_element_and_attributes()
        {
            var fixture = new Fixture();

            var functionParameter
                = new FunctionParameter(
                    "Foo",
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                    ParameterMode.In);

            fixture.Writer.WriteFunctionParameterHeader(functionParameter);

            Assert.Equal(
                "<Parameter Name=\"Foo\" Type=\"String\" Mode=\"In\"",
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
                new EdmFunctionPayload()
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
                new EdmFunctionPayload()
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

            Assert.Equal(@"<Schema Namespace=""test"" Alias=""Self"" annotation:UseStrongSpatialTypes=""false"" xmlns:annotation=""http://schemas.microsoft.com/ado/2009/02/edm/annotation""", 
            fixture.ToString());
        }
        
        [Fact]
        public void WriteSchemaElementHeader_writes_annotation_namespace_declaration_if_UseStrongSpatialTypes_not_present()
        {
            var fixture = new Fixture(2.0);
            fixture.Writer.WriteSchemaElementHeader("test");

            Assert.Equal(@"<Schema Namespace=""test"" Alias=""Self"" xmlns:annotation=""http://schemas.microsoft.com/ado/2009/02/edm/annotation""",
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
                @"<Schema Namespace=""ns"" Alias=""Self"" annotation:UseStrongSpatialTypes=""false"" xmlns:annotation=""http://schemas.microsoft.com/ado/2009/02/edm/annotation"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">" + 
                @"<EntityContainer Name=""C"" annotation:LazyLoadingEnabled=""true""",
            fixture.ToString());
        }

        private class Fixture
        {
            public readonly EdmXmlSchemaWriter Writer;

            private readonly StringBuilder _stringBuilder;
            private readonly XmlWriter _xmlWriter;

            public Fixture(double version = 3.0)
            {
                _stringBuilder = new StringBuilder();

                _xmlWriter = XmlWriter.Create(
                    _stringBuilder,
                    new XmlWriterSettings
                        {
                            OmitXmlDeclaration = true
                        });

                Writer = new EdmXmlSchemaWriter(_xmlWriter, version, false);
            }

            public override string ToString()
            {
                _xmlWriter.Flush();

                return _stringBuilder.ToString();
            }
        }
    }
}
