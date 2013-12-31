// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using Xunit;
    using Xunit.Extensions;

    public class TableConfigurationTests
    {
        [Fact]
        public void GetAttributeBody_returns_body()
        {
            var configuration = new TableConfiguration { Table = "Entities" };
            var code = new CSharpCodeHelper();

            Assert.Equal("Table(\"Entities\")", configuration.GetAttributeBody(code));
        }

        [Fact]
        public void GetMethodChain_returns_body()
        {
            var configuration = new TableConfiguration { Table = "Entities" };
            var code = new CSharpCodeHelper();

            Assert.Equal(".ToTable(\"Entities\")", configuration.GetMethodChain(code));
        }

        [Theory]
        [InlineData(null, "One", "One")]
        [InlineData("One", "Two", "One.Two")]
        [InlineData(null, "One.Two", "[One.Two]")]
        [InlineData("One.Two", "Three", "[One.Two].Three")]
        [InlineData("One", "Two.Three", "One.[Two.Three]")]
        [InlineData("One.Two", "Three.Four", "[One.Two].[Three.Four]")]
        public void GetName_escapes_parts_when_dot(string schema, string table, string expected)
        {
            var configuration = new TableConfiguration { Schema = schema, Table = table };

            Assert.Equal(expected, configuration.GetName());
        }
    }
}
