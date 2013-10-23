// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views
{
#if VIEWSOURCE
    internal class ViewSourceEventArgs : EventArgs
    {
        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
        }

        private XObject _node;
        public XObject Node
        {
            get { return _node; }
        }

        internal ViewSourceEventArgs(string filePath, XObject node)
        {
            _filePath = filePath;
            _node = node;
        }
    }
#endif
}
