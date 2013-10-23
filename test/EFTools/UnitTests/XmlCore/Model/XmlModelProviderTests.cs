// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.XmlDesignerBase.Model
{
    using System;
    using System.Xml.Linq;
    using Moq;
    using Xunit;

    public class XmlModelProviderTests
    {
        [Fact]
        public void GetTextSpanForXObject_calls_XmlModel_GetTextSpan_if_xobject_not_null()
        {
            var textSpan = new TextSpan { iStartLine = 42, iStartIndex = 43, iEndLine = 44, iEndIndex = 45 };

            var xmlModelMock = new Mock<XmlModel>();
            xmlModelMock
                .Setup(m => m.GetTextSpan(It.IsAny<XObject>()))
                .Returns(textSpan);

            var modelProviderMock = new Mock<XmlModelProvider> { CallBase = true };
            modelProviderMock
                .Setup(m => m.GetXmlModel(It.IsAny<Uri>()))
                .Returns(xmlModelMock.Object);

            Assert.Equal(
                textSpan,
                modelProviderMock.Object.GetTextSpanForXObject(new XText("2.71828"), new Uri("http://tempuri")));
        }

        [Fact]
        public void GetTextSpanForXObject_creates_empty_TextSpan_if_xobject_null()
        {
            var textSpan = new Mock<XmlModelProvider> { CallBase = true }.Object
                .GetTextSpanForXObject(null, new Uri("http://tempuri"));

            Assert.Equal(0, textSpan.iStartLine);
            Assert.Equal(0, textSpan.iStartIndex);
            Assert.Equal(0, textSpan.iEndLine);
            Assert.Equal(0, textSpan.iEndIndex);
        }
    }
}
