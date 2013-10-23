// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Validation
{
    using System;
    using System.Xml;
    using Moq;
    using Xunit;

    public class XmlReaderProxyTests
    {
        [Fact]
        public void HasLineInfo_returns_lineNumberService_HasLineInfo_lineNumberService_not_null()
        {
            var lineNumberServiceMock = new Mock<IXmlLineInfo>();
            lineNumberServiceMock
                .Setup(l => l.HasLineInfo())
                .Returns(true);

            Assert.True(
                new XmlReaderProxy(new Mock<XmlReader>().Object, new Uri("http://tempuri"), lineNumberServiceMock.Object)
                    .HasLineInfo());

            lineNumberServiceMock.Verify(l => l.HasLineInfo(), Times.Once());
        }

        [Fact]
        public void HasLineInfo_calls_into_underlying_XmlReader_HasLineInfo_if_XmlReader_implements_IXmlLineInfo_and_lineNumberService_null(
            )
        {
            var xmlReaderMock = new Mock<XmlReader>();
            var xmlLineInfoMock = xmlReaderMock.As<IXmlLineInfo>();
            xmlLineInfoMock
                .Setup(l => l.HasLineInfo())
                .Returns(true);

            Assert.True(
                new XmlReaderProxy(xmlReaderMock.Object, new Uri("http://tempuri"), null)
                    .HasLineInfo());

            xmlLineInfoMock.Verify(l => l.HasLineInfo(), Times.Once());
        }

        [Fact]
        public void HasLineInfo_returns_false_if_lineNumberService_null_and_XmlReader_is_not_IXmlLineInfo()
        {
            Assert.False(
                new XmlReaderProxy(new Mock<XmlReader>().Object, new Uri("http://tempuri"), null)
                    .HasLineInfo());
        }

        [Fact]
        public void LineNumber_returns_lineNumberService_LineNumber_lineNumberService_not_null()
        {
            var lineNumberServiceMock = new Mock<IXmlLineInfo>();
            lineNumberServiceMock
                .Setup(l => l.LineNumber)
                .Returns(42);

            Assert.Equal(
                42,
                new XmlReaderProxy(new Mock<XmlReader>().Object, new Uri("http://tempuri"), lineNumberServiceMock.Object)
                    .LineNumber);

            lineNumberServiceMock.Verify(l => l.LineNumber, Times.Once());
        }

        [Fact]
        public void LineNumber_calls_into_underlying_XmlReader_LineNumber_if_XmlReader_implements_IXmlLineInfo_and_lineNumberService_null()
        {
            var xmlReaderMock = new Mock<XmlReader>();
            var xmlLineInfoMock = xmlReaderMock.As<IXmlLineInfo>();
            xmlLineInfoMock
                .Setup(l => l.LineNumber)
                .Returns(42);

            Assert.Equal(
                42,
                new XmlReaderProxy(xmlReaderMock.Object, new Uri("http://tempuri"), null)
                    .LineNumber);

            xmlLineInfoMock.Verify(l => l.LineNumber, Times.Once());
        }

        [Fact]
        public void LineNumber_returns_0_if_lineNumberService_null_and_XmlReader_is_not_IXmlLineInfo()
        {
            Assert.Equal(
                0,
                new XmlReaderProxy(new Mock<XmlReader>().Object, new Uri("http://tempuri"), null)
                    .LineNumber);
        }

        [Fact]
        public void LinePosition_returns_LinePositionService_LinePosition_LinePositionService_not_null()
        {
            var linePositionServiceMock = new Mock<IXmlLineInfo>();
            linePositionServiceMock
                .Setup(l => l.LinePosition)
                .Returns(42);

            Assert.Equal(
                42,
                new XmlReaderProxy(new Mock<XmlReader>().Object, new Uri("http://tempuri"), linePositionServiceMock.Object)
                    .LinePosition);

            linePositionServiceMock.Verify(l => l.LinePosition, Times.Once());
        }

        [Fact]
        public void
            LinePosition_calls_into_underlying_XmlReader_LinePosition_if_XmlReader_implements_IXmlLineInfo_and_LinePositionService_null()
        {
            var xmlReaderMock = new Mock<XmlReader>();
            var xmlLineInfoMock = xmlReaderMock.As<IXmlLineInfo>();
            xmlLineInfoMock
                .Setup(l => l.LinePosition)
                .Returns(42);

            Assert.Equal(
                42,
                new XmlReaderProxy(xmlReaderMock.Object, new Uri("http://tempuri"), null)
                    .LinePosition);

            xmlLineInfoMock.Verify(l => l.LinePosition, Times.Once());
        }

        [Fact]
        public void LinePosition_returns_0_if_LinePositionService_null_and_XmlReader_is_not_IXmlLineInfo()
        {
            Assert.Equal(
                0,
                new XmlReaderProxy(new Mock<XmlReader>().Object, new Uri("http://tempuri"), null)
                    .LinePosition);
        }
    }
}
