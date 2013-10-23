// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views
{
    using System;
    using System.Windows;

    internal class ShowContextMenuEventArgs : EventArgs
    {
        private readonly Point _point;

        public Point Point
        {
            get { return _point; }
        }

        internal ShowContextMenuEventArgs(Point point)
        {
            _point = point;
        }
    }
}
