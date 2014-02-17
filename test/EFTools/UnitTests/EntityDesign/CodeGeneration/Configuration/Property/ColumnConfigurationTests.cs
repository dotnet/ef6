// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using Xunit;

    public class ColumnConfigurationTests
    {
        [Fact]
        public void GetAttributeBody_returns_body_when_name()
        {
            var configuration = new ColumnConfiguration { Name = "Id" };
            var code = new CSharpCodeHelper();

            Assert.Equal("Column(\"Id\")", configuration.GetAttributeBody(code));
        }

        [Fact]
        public void GetAttributeBody_returns_body_when_order()
        {
            var configuration = new ColumnConfiguration { Order = 0 };
            var code = new CSharpCodeHelper();

            Assert.Equal("Column(Order = 0)", configuration.GetAttributeBody(code));
        }

        [Fact]
        public void GetAttributeBody_returns_body_when_type()
        {
            var configuration = new ColumnConfiguration { TypeName = "int" };
            var code = new CSharpCodeHelper();

            Assert.Equal("Column(TypeName = \"int\")", configuration.GetAttributeBody(code));
        }

        [Fact]
        public void GetAttributeBody_returns_body_when_order_and_type()
        {
            var configuration = new ColumnConfiguration { Order = 0, TypeName = "int" };
            var code = new CSharpCodeHelper();

            Assert.Equal("Column(Order = 0, TypeName = \"int\")", configuration.GetAttributeBody(code));
        }

        [Fact]
        public void GetAttributeBody_returns_body_when_order_and_type_and_vb()
        {
            var configuration = new ColumnConfiguration { Order = 0, TypeName = "int" };
            var code = new VBCodeHelper();

            Assert.Equal("Column(Order:=0, TypeName:=\"int\")", configuration.GetAttributeBody(code));
        }

        [Fact]
        public void GetAttributeBody_returns_body_when_all()
        {
            var configuration = new ColumnConfiguration { Name = "Id", Order = 0, TypeName = "int" };
            var code = new CSharpCodeHelper();

            Assert.Equal("Column(\"Id\", Order = 0, TypeName = \"int\")", configuration.GetAttributeBody(code));
        }

        [Fact]
        public void GetMethodChain_returns_body_when_name()
        {
            var configuration = new ColumnConfiguration { Name = "Id" };
            var code = new CSharpCodeHelper();

            Assert.Equal(".HasColumnName(\"Id\")", configuration.GetMethodChain(code));
        }

        [Fact]
        public void GetMethodChain_returns_body_when_order()
        {
            var configuration = new ColumnConfiguration { Order = 0 };
            var code = new CSharpCodeHelper();

            Assert.Equal(".HasColumnOrder(0)", configuration.GetMethodChain(code));
        }

        [Fact]
        public void GetMethodChain_returns_body_when_type()
        {
            var configuration = new ColumnConfiguration { TypeName = "int" };
            var code = new CSharpCodeHelper();

            Assert.Equal(".HasColumnType(\"int\")", configuration.GetMethodChain(code));
        }

        [Fact]
        public void GetMethodChain_returns_body_when_all()
        {
            var configuration = new ColumnConfiguration { Name = "Id", Order = 0, TypeName = "int" };
            var code = new CSharpCodeHelper();

            Assert.Equal(
                ".HasColumnName(\"Id\").HasColumnOrder(0).HasColumnType(\"int\")",
                configuration.GetMethodChain(code));
        }
    }
}
