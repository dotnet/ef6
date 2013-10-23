// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Explorer
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    ///     This must be public since it needs to be accessible from a satellite resource DLL for localization
    /// </summary>
    public sealed partial class EntityDesignExplorerContent : ExplorerContent
    {
        internal override SearchComboBox SearchBox
        {
            get { return searchBox; }
        }

        internal override TextBlock SearchBarText
        {
            get { return searchBarText; }
        }

        internal override Button GoSearchButton
        {
            get { return goSearchButton; }
        }

        internal override Border SearchBar
        {
            get { return searchBar; }
        }

        internal override FrameworkElement SearchTicksTrack
        {
            get { return searchTicksTrack; }
        }

        internal override SearchAdornerDecorator SearchAdornerDecorator
        {
            get { return searchAdornerDecorator; }
        }

        internal override TreeView ExplorerTreeView
        {
            get { return explorerTreeView; }
        }

        internal override ExplorerTreeViewItem ExplorerTreeRoot
        {
            get { return explorerTreeRoot; }
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    // null out these variables so they will be free'd for GC
                    explorerTreeRoot = null;
                    explorerTreeView = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
