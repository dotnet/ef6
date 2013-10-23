// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Validation
{
    using System;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Moq;
    using Xunit;

    public class XNodeReaderLineNumberServiceTests
    {
        [Fact]
        public void HasLineInfo_returns_true()
        {
            // we use reflection inside XNodeReaderLineNumberService. If this test fails it means that the class 
            // we reflect on changed and the XNodeReaderLineNumberService needs to be updated accordingly.

            var mockModelProvider = new Mock<XmlModelProvider>();
            using (var reader = new XElement("dummy").CreateReader())
            {
                using (var modelProvider = mockModelProvider.Object)
                {
                    Assert.True(
                        new XNodeReaderLineNumberService(modelProvider, reader, new Uri("urn:abc")).HasLineInfo());
                }
            }
        }

        [Fact]
        public void LineNumber_and_LinePosition_return_line_and_column_number_for_element_if_XNodeReader_positioned_on_element()
        {
            var inputXml = new XElement("dummy", "value");

            var mockModelProvider = new Mock<XmlModelProvider>();
            mockModelProvider
                .Setup(m => m.GetTextSpanForXObject(It.Is<XObject>(x => x == inputXml), It.IsAny<Uri>()))
                .Returns(new TextSpan { iStartLine = 21, iStartIndex = 42, });

            using (var reader = inputXml.CreateReader())
            {
                reader.Read();
                Assert.Equal(XmlNodeType.Element, reader.NodeType);
                Assert.Equal("dummy", reader.Name);

                using (var modelProvider = mockModelProvider.Object)
                {
                    Assert.Equal(
                        21,
                        new XNodeReaderLineNumberService(modelProvider, reader, new Uri("urn:abc")).LineNumber);

                    Assert.Equal(
                        42,
                        new XNodeReaderLineNumberService(modelProvider, reader, new Uri("urn:abc")).LinePosition);
                }
            }
        }

        [Fact]
        public void LineNumber_and_LinePosition_return_line_and_column_number_for_text_if_XNodeReader_positioned_on_text_node()
        {
            var inputXml = new XElement("dummy", "value");

            var mockModelProvider = new Mock<XmlModelProvider>();
            mockModelProvider
                .Setup(m => m.GetTextSpanForXObject(It.Is<XObject>(x => x == inputXml.FirstNode), It.IsAny<Uri>()))
                .Returns(new TextSpan { iStartLine = 21, iStartIndex = 42, });

            using (var reader = inputXml.CreateReader())
            {
                reader.Read();
                reader.Read();
                Assert.Equal(XmlNodeType.Text, reader.NodeType);
                Assert.Equal("value", reader.Value);

                using (var modelProvider = mockModelProvider.Object)
                {
                    Assert.Equal(
                        21,
                        new XNodeReaderLineNumberService(modelProvider, reader, new Uri("urn:abc")).LineNumber);

                    Assert.Equal(
                        42,
                        new XNodeReaderLineNumberService(modelProvider, reader, new Uri("urn:abc")).LinePosition);
                }
            }
        }
    }
}
