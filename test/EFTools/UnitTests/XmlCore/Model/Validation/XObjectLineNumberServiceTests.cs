// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Validation
{
    using System;
    using System.Xml.Linq;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Moq;
    using Xunit;

    public class XObjectLineNumberServiceTests
    {
        [Fact]
        public void GetLineNumber_returns_line_number_from_text_span()
        {
            var mockModelProvider = new Mock<XmlModelProvider>();
            mockModelProvider
                .Setup(m => m.GetTextSpanForXObject(It.IsAny<XObject>(), It.IsAny<Uri>()))
                .Returns(new TextSpan { iStartLine = 42 });

            using (var mockModel = mockModelProvider.Object)
            {
                Assert.Equal(
                    42,
                    new XObjectLineNumberService(mockModel).GetLineNumber(null, null));
            }
        }

        [Fact]
        public void GetColumnNumber_returns_column_number_from_text_span()
        {
            var mockModelProvider = new Mock<XmlModelProvider>();
            mockModelProvider
                .Setup(m => m.GetTextSpanForXObject(It.IsAny<XObject>(), It.IsAny<Uri>()))
                .Returns(new TextSpan { iStartIndex = 42 });

            using (var mockModel = mockModelProvider.Object)
            {
                Assert.Equal(
                    42,
                    new XObjectLineNumberService(mockModel).GetColumnNumber(null, null));
            }
        }
    }
}
