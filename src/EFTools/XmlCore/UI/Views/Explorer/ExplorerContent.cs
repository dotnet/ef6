// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Explorer
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public abstract class ExplorerContent : Grid, IDisposable
    {
        private ExplorerFrame _explorerFrame;

        internal abstract SearchComboBox SearchBox { get; }
        internal abstract TextBlock SearchBarText { get; }
        internal abstract Button GoSearchButton { get; }
        internal abstract Border SearchBar { get; }
        internal abstract FrameworkElement SearchTicksTrack { get; }
        internal abstract SearchAdornerDecorator SearchAdornerDecorator { get; }
        internal abstract TreeView ExplorerTreeView { get; }
        internal abstract ExplorerTreeViewItem ExplorerTreeRoot { get; }

        internal void OnTreeViewItemCollapsed(object sender, RoutedEventArgs e)
        {
            var explorerFrame = GetExplorerFrame();
            if (null != explorerFrame)
            {
                explorerFrame.OnTreeViewItemCollapsed();
            }
        }

        internal void OnTreeViewItemExpanded(object sender, RoutedEventArgs e)
        {
            var explorerFrame = GetExplorerFrame();
            if (null != explorerFrame)
            {
                explorerFrame.OnTreeViewItemExpanded();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "DownLoaded",
            Justification = "It is DropDown Loaded, not Drop Downloaded")]
        internal void OnDropDownLoaded(object sender, RoutedEventArgs e)
        {
            var searchTextBox = ExplorerUtility.GetTypeDescendents(SearchBox, typeof(TextBox)).FirstOrDefault() as TextBox;
            if (searchTextBox != null)
            {
                searchTextBox.MaxLength = 1000;
            }
        }

        internal void OnDropDownOpened(object sender, EventArgs e)
        {
            // if search text already appears in combo box, select it
            var text = SearchBox.Text;
            SearchBox.SelectedValue = text;
            SearchBox.Text = text;
            if (SearchBox.Items.Count == 0)
            {
                SearchBox.IsDropDownOpen = false;
            }
        }

        internal void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            // If user presses 'Return' then invoke the same code as if the user
            // had pressed the 'Go Search' button
            if (e.Key == Key.Return)
            {
                if (!string.IsNullOrEmpty(SearchBox.Text))
                {
                    var searchCommand = GoSearchButton.Command;
                    if (searchCommand.CanExecute(null))
                    {
                        searchCommand.Execute(null);
                    }
                }
            }
        }

        internal void OnMouseDownAncestorOfSearchResultItemStyle(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var frame = GetExplorerFrame();
                if (null != frame)
                {
                    frame.OnMouseDownAncestorOfSearchResultItemStyle(sender);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public static bool HasLoadException
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="disposing">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _explorerFrame = null;
            }
        }

        private ExplorerFrame GetExplorerFrame()
        {
            if (_explorerFrame == null)
            {
                _explorerFrame = ExplorerUtility.FindLogicalAncestorOfType<ExplorerFrame>(this);
            }

            return _explorerFrame;
        }
    }
}
