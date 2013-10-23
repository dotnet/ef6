// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Moq;
    using Xunit;

    public class DesignerErrorListTests
    {
        [Fact]
        public void DesignerErrorList_creates_non_null_ErrorListProvider()
        {
            Assert.NotNull(new DesignerErrorList(new Mock<IServiceProvider>().Object).Provider);
        }

        [Fact]
        public void Can_add_clear_error_list_tasks()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(p => p.GetService(typeof(SVsTaskList))).Returns(new Mock<IVsTaskList>().Object);

            var task = new ErrorTask();
            var errorList = new DesignerErrorList(mockServiceProvider.Object);

            Assert.Empty(errorList.Provider.Tasks);
            errorList.AddItem(task);
            Assert.Equal(new[] { task }, errorList.Provider.Tasks.Cast<ErrorTask>());
            errorList.Clear();
            Assert.Empty(errorList.Provider.Tasks);
        }
    }
}
