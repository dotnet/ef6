// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Diagnostics;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    ///     This is the error task we use when a document is not opened.
    /// </summary>
    internal abstract class XmlModelErrorTask : ErrorTask, IXmlModelErrorTask, IDisposable
    {
        private ServiceProvider _serviceProvider;
        private readonly uint _itemID;

        protected XmlModelErrorTask(
            string document, string errorMessage, int lineNumber, int columnNumber, TaskErrorCategory category, IVsHierarchy hierarchy,
            uint itemID)
        {
            ErrorCategory = category;
            HierarchyItem = hierarchy;
            _itemID = itemID;

            IOleServiceProvider oleSP = null;
            var hr = hierarchy.GetSite(out oleSP);
            if (NativeMethods.Succeeded(hr))
            {
                _serviceProvider = new ServiceProvider(oleSP);
            }

            Debug.Assert(!String.IsNullOrEmpty(document), "document is null or empty");
            Debug.Assert(!String.IsNullOrEmpty(errorMessage), "errorMessage is null or empty");
            Debug.Assert(hierarchy != null, "hierarchy is null");

            Document = document;
            Text = errorMessage;
            Line = lineNumber;
            Column = columnNumber;
        }

        ~XmlModelErrorTask()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_serviceProvider != null)
                {
                    _serviceProvider.Dispose();
                    _serviceProvider = null;
                }
            }
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
