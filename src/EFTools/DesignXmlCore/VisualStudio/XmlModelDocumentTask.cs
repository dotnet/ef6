// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using Microsoft.VisualStudio.Package;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TextManager.Interop;

    /// <summary>
    ///     This is the error task we use for open documents.  The DocumentTask will keep text ranges up to date when the buffer changes.
    /// </summary>
    internal abstract class XmlModelDocumentTask : DocumentTask, IXmlModelErrorTask
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly uint _itemID;

        protected XmlModelDocumentTask(
            IServiceProvider site, IVsTextLines buffer, MARKERTYPE markerType, TextSpan span, string document, uint itemID,
            string errorMessage, IVsHierarchy hierarchy)
            : base(site, buffer, markerType, span, document)
        {
            _serviceProvider = site;
            _itemID = itemID;
            Text = errorMessage;
            HierarchyItem = hierarchy;
        }

        public override int OnAfterMarkerChange(IVsTextMarker marker)
        {
            var val = base.OnAfterMarkerChange(marker);

            var textLineMarker = TextLineMarker;
            var span = Span;
            if (textLineMarker != null)
            {
                var spanArray = new TextSpan[1];
                NativeMethods.ThrowOnFailure(textLineMarker.GetCurrentSpan(spanArray));
                span = spanArray[0];
            }

            Line = span.iStartLine;
            Column = span.iStartIndex;

            return val;
        }

        public IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        public uint ItemID
        {
            get { return _itemID; }
        }
    }
}
