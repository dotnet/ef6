// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Validation
{
    using Microsoft.Data.Tools.XmlDesignerBase;
    using Moq;
    using System;
    using Xunit;

    public class ErrorInfoTests
    {
        [Fact]
        public void Edmx_ErrorInfo_initialized_correctly()
        {
            var mockEFObject = new Mock<EFObject>();
            mockEFObject.Setup(o => o.GetLineNumber()).Returns(76);
            mockEFObject.Setup(o => o.GetColumnNumber()).Returns(12);
            mockEFObject.Setup(o => o.Uri).Returns(new Uri(@"c:\project\model.edmx"));

            var edmxErrorInfo = 
                new ErrorInfo(ErrorInfo.Severity.ERROR, "test", mockEFObject.Object, 42, ErrorClass.Runtime_CSDL);

            Assert.Equal(76, edmxErrorInfo.GetLineNumber());
            Assert.Equal(12, edmxErrorInfo.GetColumnNumber());
            Assert.True(edmxErrorInfo.IsError());
            Assert.False(edmxErrorInfo.IsWarning());
            Assert.False(edmxErrorInfo.IsInfo());
            Assert.Equal(string.Format(Resources.Error_Message_With_Error_Code_Prefix, 42, "test"), edmxErrorInfo.Message);
            Assert.Same(mockEFObject.Object, edmxErrorInfo.Item);
            Assert.Equal(@"c:\project\model.edmx", edmxErrorInfo.ItemPath);
            Assert.Equal(42, edmxErrorInfo.ErrorCode);
            Assert.Equal(ErrorClass.Runtime_CSDL, edmxErrorInfo.ErrorClass);
        }

        [Fact]
        public void Code_first_ErrorInfo_initialized_correctly()
        {
            var edmxErrorInfo = 
                new ErrorInfo(ErrorInfo.Severity.WARNING, "test", @"c:\project\model.edmx" , 17, ErrorClass.None);

            Assert.Equal(0, edmxErrorInfo.GetLineNumber());
            Assert.Equal(0, edmxErrorInfo.GetColumnNumber());
            Assert.False(edmxErrorInfo.IsError());
            Assert.True(edmxErrorInfo.IsWarning());
            Assert.False(edmxErrorInfo.IsInfo());

            Assert.Equal(string.Format(Resources.Error_Message_With_Error_Code_Prefix, 17, "test"), edmxErrorInfo.Message);
            Assert.Same(null, edmxErrorInfo.Item);
            Assert.Equal(@"c:\project\model.edmx", edmxErrorInfo.ItemPath);
            Assert.Equal(17, edmxErrorInfo.ErrorCode);
            Assert.Equal(ErrorClass.None, edmxErrorInfo.ErrorClass);
        }
    }
}
