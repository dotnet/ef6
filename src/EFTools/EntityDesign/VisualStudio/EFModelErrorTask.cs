// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    // <summary>
    //     This is the error task we use when a document is not opened.
    // </summary>
    internal class EFModelErrorTask : XmlModelErrorTask
    {
        internal EFModelErrorTask(
            string document, string errorMessage, int lineNumber, int columnNumber, TaskErrorCategory category, IVsHierarchy hierarchy,
            uint itemID)
            : base(document, errorMessage, lineNumber, columnNumber, category, hierarchy, itemID)
        {
            Navigate += EFModelErrorTaskNavigator.NavigateTo;
        }
    }
}
