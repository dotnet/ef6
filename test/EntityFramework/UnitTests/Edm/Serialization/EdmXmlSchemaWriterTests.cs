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

        private class Fixture
        {
            public readonly EdmXmlSchemaWriter Writer;

            private readonly StringBuilder _stringBuilder;
            private readonly XmlWriter _xmlWriter;

            public Fixture()
            {
                _stringBuilder = new StringBuilder();

                _xmlWriter = XmlWriter.Create(
                    _stringBuilder,
                    new XmlWriterSettings
                        {
                            OmitXmlDeclaration = true
                        });

                Writer = new EdmXmlSchemaWriter(_xmlWriter, 3.0, false);
            }

            public override string ToString()
            {
                _xmlWriter.Flush();

                return _stringBuilder.ToString();
            }
        }
    }
}
