// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Explorer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.UI.Commands;
    using Microsoft.Data.Entity.Design.UI.ViewModels.Explorer;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal abstract class ExplorerFrame : DockPanel, INotifyPropertyChanged, IDisposable
    {
        private bool _isDisposed;

        private EditingContext _context;
        private ExplorerContent _frameContent;
        private TreeView _frameTreeView;
        private ExplorerViewModelHelper _viewModelHelper;
        private bool _changingSelection;
        private ExplorerEFElement _selectedExplorerEFElement;

        // used to compare with _selectedExplorerEFElement to see if user is selecting same thing more than once
        protected ExplorerEFElement _previousSelectedExplorerEFElement = null;

        // Property that returns the AdornerLayer that wraps the TreeView control
        private AdornerLayer _treeViewAdornerLayer;

        /// <summary>
        ///     Cached vertical scrollbar.
        /// </summary>
        /// <remarks>
        ///     This variable is *ONLY* used to cache the scrollbar to be able to unsubscribe from events.
        ///     You can get the scrollbar by calling <see cref="ExplorerFrame.GetVerticalScrollBar()" /> method.
        /// </remarks>
        private ScrollBar _vScrollBar;

        private ScrollViewer _scrollViewer;

        private readonly ICommand _searchCommand;
        private readonly ICommand _resetSearchCommand;
        private readonly ICommand _selectNextSearchResult;
        private readonly ICommand _selectPreviousSearchResult;

        private bool _searchIsActive;
        // _searchExpansionInProgress indicates that an expansion from an initial search is
        //  in progress so that the TreeViewItem expand event will not attempt to fix up adorners all
        //  over again ...
        private bool _searchExpansionInProgress;
        private bool _nextOrPreviousInProgress;
        private bool _modelChangesCommittingInProgress;

        private DeferredRequest _deferredExpansionAndCalculateAdorners;
        private DeferredRequest _deferredUpdateNextAndPreviousSearchResults;

        private bool _isTreeViewMouseDown;
        private bool _isDragDrop;

        #region Explorer events

        internal event EventHandler<ShowContextMenuEventArgs> ShowContextMenu;
        internal event EventHandler SearchCompleted;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected ExplorerFrame(EditingContext context)
        {
            EditingContext = context;

            // search text commands
            _searchCommand = new DelegateCommand(OnSearchCommand);
            _resetSearchCommand = new DelegateCommand(OnResetSearchCommand);

            // navigate search commands
            _selectPreviousSearchResult = new DelegateCommand(OnSelectPreviousSearchResult);
            _selectNextSearchResult = new DelegateCommand(OnSelectNextSearchResult);

            _deferredExpansionAndCalculateAdorners = new DeferredRequest(OnExpanded);
            _deferredUpdateNextAndPreviousSearchResults = new DeferredRequest(OnUpdateNextAndPreviousResults);
        }

        public ICommand SearchCommand
        {
            get { return _searchCommand; }
        }

        public ICommand ResetSearchCommand
        {
            get { return _resetSearchCommand; }
        }

        public ICommand SelectNextSearchResult
        {
            get { return _selectNextSearchResult; }
        }

        public ICommand SelectPreviousSearchResult
        {
            get { return _selectPreviousSearchResult; }
        }

        public bool CanGoToNextSearchResult
        {
            get
            {
                var context = _context;
                if (context != null)
                {
                    var explorerSearchResults = ExplorerSearchResults.GetExplorerSearchResults(context);
                    return explorerSearchResults.CanGoToNextSearchResult;
                }
                return false;
            }
        }

        public bool CanGoToPreviousSearchResult
        {
            get
            {
                var context = _context;
                if (context != null)
                {
                    var explorerSearchResults = ExplorerSearchResults.GetExplorerSearchResults(context);
                    return explorerSearchResults.CanGoToPreviousSearchResult;
                }
                return false;
            }
        }

        #region Properties

        public EditingContext Context
        {
            get { return _context; }
        }

        internal ExplorerViewModelHelper ExplorerViewModelHelper
        {
            get { return _viewModelHelper; }
        }

        // _searchIsActive indicates that there is an active search that many have zero
        //  or more search results.  The TreeViewItem Expand and Collapse events use this
        //  indicator to know that adorners need to be fixed up when an expand or collapse
        //  is done on a TreeViewItem.
        public bool SearchIsActive
        {
            get { return _searchIsActive; }
            set
            {
                _searchIsActive = value;
                SearchBar.Visibility = Visibility.Collapsed;
                SearchTicksTrack.Visibility = Visibility.Collapsed;
                if (_searchIsActive)
                {
                    SearchBar.Visibility = Visibility.Visible;
                    SearchTicksTrack.Visibility = Visibility.Visible;
                }
            }
        }

        #endregion

        #region Public methods

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                OnResetSearchCommand();
                EditingContext = null;

                _deferredExpansionAndCalculateAdorners.Dispose();
                _deferredExpansionAndCalculateAdorners = null;
                _deferredUpdateNextAndPreviousSearchResults.Dispose();
                _deferredUpdateNextAndPreviousSearchResults = null;

                UnhookScrollBarEvents(_vScrollBar);
                _vScrollBar = null;

                if (_frameTreeView != null)
                {
                    _frameTreeView.ContextMenuOpening -= OnContextMenuOpening;
                    _frameTreeView.PreviewMouseDown -= OnTreeViewPreviewMouseDown;
                    _frameTreeView.PreviewMouseMove -= OnTreeViewPreviewMouseMove;
                    _frameTreeView.PreviewMouseUp -= OnTreeViewPreviewMouseUp;
                    _frameTreeView = null;
                }

                if (_frameContent != null)
                {
                    _frameContent.Dispose();
                    _frameContent = null;
                }
            }
        }

        #endregion

        #region Implementation

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (Children.Count == 0)
            {
                try
                {
                    Loaded += OnExplorerFrameLoaded;

                    _frameContent = InitializeExplorerContent();

                    Children.Add(_frameContent);

                    // Hook up to the Explorer TreeView to Selection
                    ExplorerTreeView.SelectedItemChanged += OnTreeViewSelectionChanged;

                    // Bind the overall Explorer frame context to this class
                    _frameContent.DataContext = this;

                    // Bind the TreeView control to the ViewModel root
                    ReloadViewModel();

                    // Select the root node and update selection
                    ExplorerTreeRoot.IsSelected = true;
                    UpdateSelection();
                }
                catch (Exception ex)
                {
                    Debug.Fail("Failed to initialize ExplorerFrame. Exception message: " + ex.Message);
                }
            }
        }

        private void ReloadViewModel()
        {
            Debug.Assert(_context != null, "ExplorerFrame was disposed");
            // if _viewModelHelper doesn't exist then create and subscribe to ViewModelChanged event
            if (_viewModelHelper == null)
            {
                _viewModelHelper = GetNewExplorerViewModelHelper();
                _viewModelHelper.ExplorerViewModelChanged += OnViewModelChange;
            }

            // create a new ViewModel and assign it to the ViewModelHelper - this
            // will cause ViewModelChanged event to be fired and we will update
            // the DataContext in OnViewModelChange() below
            _viewModelHelper.CreateViewModel(_context);
        }

        // refresh the DataContext of the tree view if the underlying view model changes
        private void OnViewModelChange(object sender, ExplorerViewModelHelper.ExplorerViewModelChangedEventArgs args)
        {
            var viewModel = args.NewViewModel;
            Debug.Assert(viewModel != null, "Null ViewModel in ExplorerFrame.OnViewModelChange()");
            Debug.Assert(viewModel.RootNode != null, "Null ViewModel.RootNode in ExplorerFrame.OnViewModelChange()");
            ExplorerTreeView.DataContext = viewModel.RootNode;
        }

        protected EditingContext EditingContext
        {
            get { return _context; }
            set
            {
                // unregister from old context
                if (_context != value)
                {
                    if (_context != null)
                    {
                        _context.Items.Unsubscribe<ExplorerSelection>(OnSelectionChanged);
                        _context = null;
                    }

                    // register to new context
                    _context = value;
                }
            }
        }

        internal void UpdateSelection()
        {
            Debug.Assert(_context != null, "ExplorerFrame was disposed");
            if (ExplorerTreeView.SelectedItem == null)
            {
                Selection.Clear<ExplorerSelection>(_context);
                _selectedExplorerEFElement = null;
            }
            else
            {
                ExplorerEFElement brItem;
                var selectedItem = ExplorerTreeView.SelectedItem;
                if (selectedItem == ExplorerTreeRoot)
                {
                    brItem = ExplorerTreeRoot.DataContext as ExplorerEFElement;
                }
                else
                {
                    brItem = selectedItem as ExplorerEFElement;
                }

                _selectedExplorerEFElement = brItem;

                if (brItem != null)
                {
                    var newSelection = new HashSet<EFElement>();
                    if (brItem.ModelItem != null)
                    {
                        newSelection.Add(brItem.ModelItem);
                    }
                    _context.Items.SetValue(new ExplorerSelection(newSelection));
                }
            }
        }

        internal ExplorerTreeViewItem GetTreeViewItem(ExplorerEFElement explorerElement, bool returnAncestorTreeViewItemIfNotAvailable)
        {
            if (explorerElement == null)
            {
                return null;
            }

            var treeViewItem = ExplorerTreeRoot;

            // need to Skip() the ExplorerRootNode because it is not the DataContext for any UI Element in the tree
            foreach (var item in explorerElement.SelfAndAncestors().Reverse().Skip(1))
            {
                //Note: if the ItemContainerGenerator status is NotStarted, the UI Element is not created yet.
                //This causes ContainerFromItem call returns NULL.
                if (treeViewItem.ItemContainerGenerator.Status == GeneratorStatus.NotStarted)
                {
                    treeViewItem.UpdateLayout();
                }

                var childTreeViewItem = (ExplorerTreeViewItem)treeViewItem.ItemContainerGenerator.ContainerFromItem(item);
                if (childTreeViewItem == null)
                {
                    if (returnAncestorTreeViewItemIfNotAvailable)
                    {
                        return treeViewItem;
                    }
                    else
                    {
                        treeViewItem = null;
                        break;
                    }
                }
                else
                {
                    treeViewItem = childTreeViewItem;
                }
            }

            Debug.Assert(treeViewItem == null || treeViewItem.DataContext == explorerElement);

            return treeViewItem;
        }

        #endregion

        #region Event Handlers

        private void OnExplorerFrameLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnExplorerFrameLoaded;
        }

        private void OnSelectionChanged(ExplorerSelection selection)
        {
            UpdateSelection();
        }

        /// <summary>
        ///     this is called by the *real* handler in ExplorerWindow so that we can catch
        ///     exceptions and reload the UI if needed - don't call this from anywhere else
        /// </summary>
        internal void OnModelChangesCommitted(object sender, EfiChangedEventArgs e)
        {
            Debug.Assert(_viewModelHelper != null, "Null _viewModelHelper in ExplorerFrame.OnModelChangesCommitted()");
            Debug.Assert(_context != null, "ExplorerFrame was disposed");
            _viewModelHelper.ProcessModelChangesCommitted(_context, e);

            // search results may have had elements removed/renamed
            // this updates adorners and Next & Previous in background    
            _modelChangesCommittingInProgress = true;
            WaitForExpansionThenProcessTreeView();
        }

        private void OnTreeViewSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!_changingSelection)
            {
                try
                {
                    _changingSelection = true;
                    _previousSelectedExplorerEFElement = _selectedExplorerEFElement; // store off previous selection
                    UpdateSelection();
                    UpdateNextAndPreviousResults(true);
                }
                finally
                {
                    _changingSelection = false;
                }
            }
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var source = e.OriginalSource as UIElement;
            if (source != null)
            {
                if (ShowContextMenu != null)
                {
                    var selectedExplorerEFElement = GetSelectedExplorerEFElement();
                    if (selectedExplorerEFElement != null)
                    {
                        Point p;
                        if (e.CursorLeft == -1
                            && e.CursorTop == -1)
                        {
                            p = new Point(0, 0);
                            var translatedPoint = source.TranslatePoint(p, this);
                            ShowContextMenu(sender, new ShowContextMenuEventArgs(translatedPoint));
                            e.Handled = true;
                        }
                        else
                        {
                            // show context menu invoked by mouse right-click
                            p = new Point(e.CursorLeft, e.CursorTop);
                            var translatedPoint = source.TranslatePoint(p, this);
                            ShowContextMenu(sender, new ShowContextMenuEventArgs(translatedPoint));
                            e.Handled = true;
                        }
                    }
                }
            }
        }

        // Change selection for right-mouse button as well as left
        private void OnTreeViewPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed
                || e.RightButton == MouseButtonState.Pressed)
            {
                ExplorerTreeViewItem treeViewItem = null;
                var item = e.OriginalSource as UIElement;
                while (item != null)
                {
                    var toggleButton = item as ToggleButton;
                    if (toggleButton != null
                        && toggleButton.Name == "Expander")
                    {
                        if (e.LeftButton == MouseButtonState.Pressed)
                        {
                            // have hit the expand/collapse button - so 
                            // return without handling event here
                            return;
                        }
                    }
                    treeViewItem = item as ExplorerTreeViewItem;
                    if (treeViewItem != null)
                    {
                        break;
                    }
                    if (item.Focusable)
                    {
                        break;
                    }
                    item = VisualTreeHelper.GetParent(item) as UIElement;
                }

                if (treeViewItem != null)
                {
                    treeViewItem.IsSelected = true;
                    Keyboard.Focus(treeViewItem);
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        _isTreeViewMouseDown = true;
                    }
                }
            }
        }

        /// <summary>
        ///     Handler for the event when the mouse is hovered on top of this element.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTreeViewPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Debug.Assert(ExplorerTreeView != null, "Why ExplorerTreeView is null");
            if (ExplorerTreeView != null
                && _isTreeViewMouseDown
                && !_isDragDrop
                && !ExplorerTreeView.IsMouseCaptured)
            {
                try
                {
                    // capture the mouse event so no other elements got event notification until we release it.
                    ExplorerTreeView.CaptureMouse();
                    if (_selectedExplorerEFElement != null
                        && _selectedExplorerEFElement.ModelItem != null)
                    {
                        var dataObject = GetClipboardObjectForExplorerItem(_selectedExplorerEFElement);
                        if (dataObject != null)
                        {
                            _isDragDrop = true;
                            DragDrop.DoDragDrop(ExplorerTreeView, dataObject, DragDropEffects.All);
                        }
                    }
                }
                finally
                {
                    _isDragDrop = false;
                    _isTreeViewMouseDown = false;
                    ExplorerTreeView.ReleaseMouseCapture();
                }
            }
        }

        /// <summary>
        ///     Handler for the event when the any mouse button is release while the mouse pointer is over this element.
        ///     This is just to ensure that we release mouse capture and reset _isTreeViewMouseDown flag.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTreeViewPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Debug.Assert(ExplorerTreeView != null, "Why ExplorerTreeView is null?");
            if (ExplorerTreeView != null)
            {
                try
                {
                    ExplorerTreeView.ReleaseMouseCapture();
                }
                finally
                {
                    _isTreeViewMouseDown = false;
                }
            }
        }

        /// <summary>
        ///     Get Serializable object that represents the EFElement.
        /// </summary>
        /// <param name="efElement"></param>
        /// <returns></returns>
        protected virtual DataObject GetClipboardObjectForExplorerItem(ExplorerEFElement efElement)
        {
            return null;
        }

        #endregion

        #region ExplorerFrame Control Accessor Properties

        internal ExplorerTreeViewItem ExplorerTreeRoot
        {
            get { return (_frameContent == null ? null : _frameContent.ExplorerTreeRoot); }
        }

        // Property that returns the Search Text Box control
        private ComboBox SearchComboBox
        {
            get { return (_frameContent == null ? null : _frameContent.SearchBox); }
        }

        private Border SearchBar
        {
            get { return (_frameContent == null ? null : _frameContent.SearchBar); }
        }

        private FrameworkElement SearchTicksTrack
        {
            get { return (_frameContent == null ? null : _frameContent.SearchTicksTrack); }
        }

        private SearchAdornerDecorator SearchAdornerDecorator
        {
            get { return (_frameContent == null ? null : _frameContent.SearchAdornerDecorator); }
        }

        private AdornerLayer TreeViewAdornerLayer
        {
            get
            {
                if (_treeViewAdornerLayer == null)
                {
                    _treeViewAdornerLayer = AdornerLayer.GetAdornerLayer(_frameContent.ExplorerTreeView);
                    Debug.Assert(_treeViewAdornerLayer != null, "Unable to Locate TreeView AdornerLayer");
                }
                return _treeViewAdornerLayer;
            }
        }

        internal ScrollViewer ScrollViewer
        {
            get
            {
                if (_scrollViewer == null)
                {
                    AdornerDecorator decorator = SearchAdornerDecorator;
                    // This AdornerDecorator wraps the TreeView so we can use it to find the TreeView's ScrollViewer
                    _scrollViewer = (ScrollViewer)ExplorerUtility.GetTypeDescendents(decorator, typeof(ScrollViewer)).FirstOrDefault();
                }
                return _scrollViewer;
            }
        }

        private TreeView ExplorerTreeView
        {
            get
            {
                if (_frameTreeView == null
                    && _frameContent != null)
                {
                    _frameTreeView = _frameContent.ExplorerTreeView;
                    _frameTreeView.ContextMenu = new ContextMenu();
                    _frameTreeView.ContextMenuOpening += OnContextMenuOpening;
                    _frameTreeView.PreviewMouseDown += OnTreeViewPreviewMouseDown;
                    // Listen to preview mouse mouve and mouse up events for drag and drop support.
                    _frameTreeView.PreviewMouseMove += OnTreeViewPreviewMouseMove;
                    _frameTreeView.PreviewMouseUp += OnTreeViewPreviewMouseUp;
                }
                return _frameTreeView;
            }
        }

        internal ExplorerEFElement GetSelectedExplorerEFElement()
        {
            return _selectedExplorerEFElement;
        }

        internal ExplorerEFElement GetPreviousSelectedExplorerEFElement()
        {
            return _previousSelectedExplorerEFElement;
        }

        #endregion

        protected abstract ExplorerViewModelHelper GetNewExplorerViewModelHelper();
        protected abstract ExplorerContent InitializeExplorerContent();

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Bug746319 opened to remove this in future")]
        private static UIElement GetFirstFocusableAncestor(UIElement focusableAncestor)
        {
            while (focusableAncestor != null)
            {
                focusableAncestor = VisualTreeHelper.GetParent(focusableAncestor) as UIElement;
                if (focusableAncestor != null
                    && focusableAncestor.Focusable)
                {
                    break;
                }
            }
            return focusableAncestor;
        }

        private ExplorerSearchResults ExpandViewModelAndTreeViewItems(ModelSearchResults modelSearchResults)
        {
            _searchExpansionInProgress = true;
            return _viewModelHelper.ExpandViewModelToDisplaySearchResults(modelSearchResults);
        }

        private void ResetAdorners()
        {
            // If there is no vertical scrollbar track then there are no adorners to reset
            if (SearchTicksTrack != null)
            {
                var adorners = TreeViewAdornerLayer.GetAdorners(SearchTicksTrack);
                if (adorners != null)
                {
                    foreach (var adorner in adorners.OfType<SearchTickAdorner>())
                    {
                        TreeViewAdornerLayer.Remove(adorner);
                    }
                }
            }
            SearchAdornerDecorator.ResetAdorners();
        }

        // Invoked when the user clicks the Search Go Button
        private void OnSearchCommand()
        {
            var previousCursor = _frameContent.Cursor;
            try
            {
                _frameContent.Cursor = Cursors.AppStarting;
                SearchComboBox.IsHitTestVisible = false;
                _frameContent.UpdateLayout();

                var searchCriteria = SearchComboBox.Text.Trim();
                if (string.IsNullOrEmpty(searchCriteria))
                {
                    // textbox could have whitespace in it - if so do not search but need to set 
                    // textbox text back to empty to re-enable search button
                    SearchComboBox.Text = string.Empty;

                    // Note: without below search combo box is not enabled after first call
                    SearchComboBox.IsHitTestVisible = true;
                }
                else
                {
                    // Do the actual search and put the results into a SearchResults (which is an editing context item)
                    var searchResults = ExplorerViewModelHelper.SearchModelByDisplayName(searchCriteria);
                    if (!SearchComboBox.Items.Contains(searchCriteria))
                    {
                        SearchComboBox.Items.Insert(0, searchCriteria);
                    }

                    DisplaySearchResults(searchResults, false);
                }
            }
            finally
            {
                _frameContent.Cursor = previousCursor;
            }
        }

        private void OnResetSearchCommand()
        {
            ResetPreviousSearchResults(true);

            ResetAdorners();

            if (_deferredExpansionAndCalculateAdorners != null)
            {
                _deferredExpansionAndCalculateAdorners.Cancel();
            }

            SearchIsActive = false;
        }

        /// <summary>
        ///     This method will be called back once the search results expansion is complete.
        /// </summary>
        private void OnExpanded(object o)
        {
            try
            {
                // Reset adorners since they will be redrawn in next step
                ResetAdorners();

                ProcessTreeViewItemsInSearchResults();

                if (_modelChangesCommittingInProgress)
                {
                    _modelChangesCommittingInProgress = false;
                }
                if (_searchExpansionInProgress)
                {
                    _searchExpansionInProgress = false;
                    OnSearchCompleted();
                }
            }
            finally
            {
                _frameContent.Cursor = null;
                // Note: without below search combo box is not enabled after first call
                SearchComboBox.IsHitTestVisible = true;
            }
        }

        private void OnSearchCompleted()
        {
            if (SearchCompleted != null)
            {
                SearchCompleted(this, EventArgs.Empty);
            }
        }

        internal void OnMouseDownAncestorOfSearchResultItemStyle(object sender)
        {
            var treeViewItem = ExplorerUtility.FindVisualAncestorOfType<ExplorerTreeViewItem>(sender as FrameworkElement);
            Debug.Assert(treeViewItem != null);
            if (treeViewItem != null)
            {
                FocusTreeViewItem(treeViewItem);
                WaitUIUpdate();
                UpdateNextAndPreviousResults(true);
                WaitUIUpdate();
                OnSelectNextSearchResult();
            }
        }

        public bool CanGotoNextResult
        {
            get
            {
                var context = _context;
                if (context != null)
                {
                    var explorerSearchResults = ExplorerSearchResults.GetExplorerSearchResults(context);
                    return explorerSearchResults.CanGoToNextSearchResult;
                }
                return false;
            }
        }

        public bool CanGotoPreviousResult
        {
            get
            {
                var context = _context;
                if (context != null)
                {
                    var explorerSearchResults = ExplorerSearchResults.GetExplorerSearchResults(context);
                    return explorerSearchResults.CanGoToPreviousSearchResult;
                }
                return false;
            }
        }

        private void UpdateNextAndPreviousResults(bool relativeToSelection)
        {
            if (_deferredUpdateNextAndPreviousSearchResults != null)
            {
                _deferredUpdateNextAndPreviousSearchResults.Request(relativeToSelection);
            }
        }

        // Argument must be an object to allow it to be called via DeferredRequest
        private void OnUpdateNextAndPreviousResults(object arg)
        {
            if (SearchIsActive)
            {
                Debug.Assert(_context != null, "ExplorerFrame was disposed");
                // only recalculate Next & Previous if the user selected a different tree
                // item (if they just hit Next or Previous Search Result then this will
                // be calculated automatically)
                if (!_nextOrPreviousInProgress)
                {
                    // relativeToItem is the current selection (or the root node if no selection)
                    ExplorerEFElement relativeToItem = null;
                    var selectedItem = _selectedExplorerEFElement;
                    var relativeToSelection = (bool)arg;
                    if (relativeToSelection)
                    {
                        relativeToItem = selectedItem;
                    }
                    if (relativeToItem == null)
                    {
                        relativeToItem = ExplorerViewModelHelper.ViewModel.RootNode;
                    }

                    var explorerSearchResults = ExplorerSearchResults.GetExplorerSearchResults(_context);
                    explorerSearchResults.RecalculateNextAndPrevious(relativeToItem);
                }

                // update whether Next and Previous buttons are enabled
                OnPropertyChanged("CanGotoNextResult");
                OnPropertyChanged("CanGotoPreviousResult");
            }
        }

        private void OnSelectNextSearchResult()
        {
            Debug.Assert(_context != null, "ExplorerFrame was disposed");
            try
            {
                _nextOrPreviousInProgress = true;

                var explorerSearchResults = ExplorerSearchResults.GetExplorerSearchResults(_context);
                var nextSearchResultItem = explorerSearchResults.SelectNextSearchResult();
                if (nextSearchResultItem != null)
                {
                    nextSearchResultItem.ExpandTreeViewToMe();
                    FocusExplorerEFElement(nextSearchResultItem, true);
                    WaitUIUpdate();
                }
            }
            finally
            {
                _nextOrPreviousInProgress = false;
            }
        }

        private void OnSelectPreviousSearchResult()
        {
            Debug.Assert(_context != null, "ExplorerFrame was disposed");
            try
            {
                _nextOrPreviousInProgress = true;

                var explorerSearchResults = ExplorerSearchResults.GetExplorerSearchResults(_context);
                var previousSearchResultItem = explorerSearchResults.SelectPreviousSearchResult();
                if (previousSearchResultItem != null)
                {
                    previousSearchResultItem.ExpandTreeViewToMe();
                    FocusExplorerEFElement(previousSearchResultItem, true);
                    WaitUIUpdate();
                }
            }
            finally
            {
                _nextOrPreviousInProgress = false;
            }
        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        internal void OnTreeViewItemCollapsed()
        {
            if (_searchIsActive && !_searchExpansionInProgress)
            {
                WaitForExpansionThenProcessTreeView();
            }
        }

        internal void OnTreeViewItemExpanded()
        {
            // If a search is active and we are not in the initial search
            //  expansion then handle the expand event for this TreeViewItem
            if (_searchIsActive && !_searchExpansionInProgress)
            {
                WaitForExpansionThenProcessTreeView();
            }
        }

        private void DisplaySearchResults(ModelSearchResults searchResults, bool clearTextSearch)
        {
            ResetPreviousSearchResults(clearTextSearch);

            // Attempt to make sure the the Search Results items are showing
            var explorerSearchResults = ExpandViewModelAndTreeViewItems(searchResults);

            var numResultsFound = explorerSearchResults.Count;
            if (numResultsFound > 0)
            {
                // Assure that the root TreeViewItem node is expanded
                ExplorerTreeRoot.IsExpanded = true;
            }

            SetSearchBarText(searchResults.Action, searchResults.SearchCriteria, numResultsFound);
            SearchIsActive = true;

            WaitForExpansionThenProcessTreeView();
        }

        private void ResetPreviousSearchResults(bool clearTextSearch)
        {
            Debug.Assert(_context != null, "ExplorerFrame was disposed");
            if (clearTextSearch)
            {
                SearchComboBox.Text = string.Empty;
            }

            // now reset the ExplorerSearchResults which will clear all the 
            // IsInSearchResults settings in the tree
            var explorerSearchResults = ExplorerSearchResults.GetExplorerSearchResults(_context);
            explorerSearchResults.Reset();
        }

        private void SetSearchBarText(string action, string searchCriteria, int found)
        {
            // first set Search Bar text itself
            SetSearchBarText(_frameContent.SearchBarText, action, searchCriteria, found);

            // next use same method to define Tooltip
            var tooltip = new TextBlock();
            SetSearchBarText(tooltip, action, searchCriteria, found);
            _frameContent.SearchBarText.ToolTip = tooltip;
        }

        private static void SetSearchBarText(TextBlock textBlock, string action, string searchCriteria, int found)
        {
            textBlock.Inlines.Clear();

            if (null != action)
            {
                var actionTokens = action.Split(new[] { "{0}" }, StringSplitOptions.None);
                Debug.Assert(
                    actionTokens.Length == 2, "Could not split search bar action text into 2 tokens - action = +++" + action + "+++");

                if (actionTokens.Length == 2)
                {
                    textBlock.Inlines.Add(new Run(actionTokens[0]));
                    if (searchCriteria != null)
                    {
                        textBlock.Inlines.Add(new Bold(new Run(searchCriteria)));
                    }
                    textBlock.Inlines.Add(new Run(actionTokens[1]));
                }
            }
            textBlock.Inlines.Add(
                new Run(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Data.Tools.XmlDesignerBase.Resources.NumOfSearchResultsFound, found)));
        }

        internal double GetY(ExplorerTreeViewItem treeViewItem)
        {
            UIElement uiElement = treeViewItem;
            while (uiElement != null
                   && !uiElement.IsVisible)
            {
                uiElement = VisualTreeHelper.GetParent(uiElement) as UIElement;
            }
            if (uiElement == null)
            {
                return 0;
            }
            else
            {
                return uiElement.TranslatePoint(new Point(0, 0), _frameContent.ExplorerTreeRoot).Y;
            }
        }

        private void ProcessTreeViewItemsInSearchResults()
        {
            Debug.Assert(_context != null, "ExplorerFrame was disposed");

            var scrollBar = GetVerticalScrollBar();

            var addAdorners = scrollBar != null && scrollBar.IsVisible;
            var explorerSearchResults = ExplorerSearchResults.GetExplorerSearchResults(_context);

            if (addAdorners)
            {
                foreach (var explorerElement in explorerSearchResults.Results)
                {
                    SearchAdornerDecorator.AddAdorner(
                        TreeViewAdornerLayer, SearchTicksTrack, explorerElement, this);
                }
            }

            if (_searchExpansionInProgress || _modelChangesCommittingInProgress)
            {
                // update Next & Previous whether tree is being expanded because
                // search was initiated by user or because model changes are being committed
                OnUpdateNextAndPreviousResults(true);

                // only select the Next or Previous result if search was initiated by user
                if (_searchExpansionInProgress)
                {
                    if (explorerSearchResults.CanGoToNextSearchResult)
                    {
                        OnSelectNextSearchResult();
                    }
                    else if (explorerSearchResults.CanGoToPreviousSearchResult)
                    {
                        OnSelectPreviousSearchResult();
                    }
                    else
                    {
                        // this has the side-effect of calling Focus() on the selected item
                        FocusExplorerEFElement(GetSelectedExplorerEFElement(), true);
                    }
                }
            }
        }

        private void WaitForExpansionThenProcessTreeView()
        {
            if (_deferredExpansionAndCalculateAdorners != null)
            {
                _deferredExpansionAndCalculateAdorners.Request();
            }
        }

        private ScrollBar GetVerticalScrollBar()
        {
            // wire up events for the scrollbar
            var explorerTreeView = ExplorerTreeView;
            var scrollBar = (explorerTreeView == null ? null : ExplorerUtility.FindFirstVerticalScrollBar(explorerTreeView));

            // Is cached scrollbar the same as the one in the visual tree?
            if (!ReferenceEquals(_vScrollBar, scrollBar))
            {
                if (_vScrollBar != null)
                {
                    UnhookScrollBarEvents(_vScrollBar);
                    _vScrollBar = null;
                    _scrollViewer = null;
                }

                // wire up events for the scrollbar
                _vScrollBar = scrollBar;
                if (_vScrollBar != null)
                {
                    _vScrollBar.SizeChanged += VScrollBar_SizeChanged;
                    _vScrollBar.IsVisibleChanged += VScrollBar_IsVisibleChanged;
                    _vScrollBar.Unloaded += VScrollBar_Unloaded;
                }
            }

            return _vScrollBar;
        }

        private void UnhookScrollBarEvents(ScrollBar scrollBar)
        {
            if (scrollBar != null)
            {
                scrollBar.SizeChanged -= VScrollBar_SizeChanged;
                scrollBar.IsVisibleChanged -= VScrollBar_IsVisibleChanged;
                scrollBar.Unloaded -= VScrollBar_Unloaded;
            }
        }

        /// <summary>
        ///     If the Vertical Scrollbar changes in size, if there is an active search,
        ///     the adorners need to be redrawn.
        /// </summary>
        private void VScrollBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var paddingTop = SystemParameters.VerticalScrollBarButtonHeight;
            var paddingBottom = paddingTop;
            if (ScrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible)
            {
                paddingBottom += SystemParameters.HorizontalScrollBarHeight;
            }

            double marginRight = 0; // SystemParameters.VerticalScrollBarWidth;
            SearchTicksTrack.Margin = new Thickness(0, 0, marginRight, 0);
            SearchTicksTrack.SetValue(Border.PaddingProperty, new Thickness(0, paddingTop, 0, paddingBottom));

            if (_searchIsActive && !_searchExpansionInProgress)
            {
                ResetAdorners();
                WaitForExpansionThenProcessTreeView();
            }
        }

        /// <summary>
        ///     If the Vertical ScrollBar visibility changed we need to fix up the Search Tick
        ///     adorners if a search is active.  Either the ScrollBar is becoming visible or being
        ///     hidden.  In either case reset (remove) the adorners.  If the scrollbar is becoming
        ///     visible then the adorners need to be repainted.
        /// </summary>
        private void VScrollBar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_context == null)
            {
                return;
            }
            if (_searchIsActive && !_searchExpansionInProgress)
            {
                ResetAdorners();
                if (GetVerticalScrollBar().Visibility == Visibility.Visible)
                {
                    WaitForExpansionThenProcessTreeView();
                }
            }
        }

        private void VScrollBar_Unloaded(object sender, RoutedEventArgs e)
        {
            UnhookScrollBarEvents((ScrollBar)sender);
        }

        internal bool SelectTreeViewItem(ExplorerEFElement explorerElement)
        {
            if (explorerElement != null)
            {
                explorerElement.ExpandTreeViewToMe();
                ExplorerTreeView.UpdateLayout();
                var tvi = FocusExplorerEFElement(explorerElement, false);
                Debug.Assert(null != tvi, "Could not find TreeViewItem for ExplorerEFElement " + explorerElement.Name);
                return (null != tvi);
            }

            return false;
        }

        internal void FocusTreeViewItem(ExplorerTreeViewItem treeViewItem)
        {
            if (null == treeViewItem)
            {
                Debug.Assert(false, "FocusTreeViewItem: should not be passed null ExplorerTreeViewItem");
                return;
            }

            ScrollToMakeVisible(treeViewItem);

            // having scrolled to the correct position, do the actual focus
            Keyboard.Focus(treeViewItem);
        }

        internal ExplorerTreeViewItem FocusExplorerEFElement(ExplorerEFElement explorerElement, bool focusAncestorIfNotAvailable)
        {
            if (null == explorerElement)
            {
                Debug.Assert(false, "FocusExplorerEFElement needs non-null explorerElement");
                return null;
            }

            var treeViewItem = GetTreeViewItem(explorerElement, focusAncestorIfNotAvailable);
            if (null != treeViewItem)
            {
                explorerElement.ExpandTreeViewToMe();
                ScrollToMakeVisible(treeViewItem);

                // having scrolled to the correct position, do the actual focus
                Keyboard.Focus(treeViewItem);

                return treeViewItem;
            }
            else
            {
                Debug.Assert(
                    false, "FocusExplorerEFElement: Could not find ExplorerTreeViewItem for ExplorerEFElement " + explorerElement.Name);
                return null;
            }
        }

        internal void ScrollToMakeVisible(ExplorerTreeViewItem treeViewItem)
        {
            if (null == treeViewItem)
            {
                Debug.Assert(false, "ScrollToMakeVisible: should not be passed null ExplorerTreeViewItem");
                return;
            }

            // select item and scroll so that it is visible
            treeViewItem.IsSelected = true;
            if (ScrollViewer != null)
            {
                var y = GetY(treeViewItem);
                var viewPortHeight = ScrollViewer.ViewportHeight;
                if (Math.Abs(ScrollViewer.VerticalOffset + viewPortHeight / 2 - y) > viewPortHeight / 2)
                {
                    // bring to view and center vertically
                    ScrollViewer.ScrollToVerticalOffset(y - viewPortHeight / 2);
                }

                var partHeader = ExplorerUtility.GetTreeViewItemPartHeader(treeViewItem);
                if (partHeader != null)
                {
                    // bring to view horizontally and maximize visible width
                    var x = partHeader.TranslatePoint(new Point(0, 0), ExplorerTreeRoot).X;
                    var width = partHeader.DesiredSize.Width;
                    var viewPortWidth = ScrollViewer.ViewportWidth;
                    var horizontalOffset = ScrollViewer.HorizontalOffset;
                    if ((horizontalOffset > x)
                        || ((x + width) > (horizontalOffset + viewPortWidth)))
                    {
                        ScrollViewer.ScrollToHorizontalOffset(x);
                    }
                }
            }
        }

        private static void WaitUIUpdate()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background, new DispatcherOperationCallback(o => frame.Continue = false), null);
            Dispatcher.PushFrame(frame);
        }
    }
}
