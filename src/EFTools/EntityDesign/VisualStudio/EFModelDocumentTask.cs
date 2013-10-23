// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TextManager.Interop;

    /// <summary>
    ///     This is the error task we use for open documents.  The DocumentTask will keep text ranges up to date when the buffer changes.
    /// </summary>
    internal class EFModelDocumentTask : XmlModelDocumentTask
    {
        internal EFModelDocumentTask(
            IServiceProvider site, IVsTextLines buffer, MARKERTYPE markerType, TextSpan span, string document, uint itemID,
            string errorMessage, IVsHierarchy hierarchy)
            : base(site, buffer, markerType, span, document, itemID, errorMessage, hierarchy)
        {
        }

        protected override void OnNavigate(EventArgs e)
        {
            EFModelErrorTaskNavigator.NavigateTo(this, e);
        }
    }
}
