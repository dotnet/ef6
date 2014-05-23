// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;
    using Microsoft.VisualStudio.Modeling.Shell;
    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.Win32;

    /// <summary>
    ///     Base class for tool windows that use the Tree Grid (VirtualTreeControl)
    /// </summary>
    internal abstract class TreeGridDesignerToolWindow : ToolWindow, ITreeGridDesignerColumnHost
    {
        private TreeGridDesignerTreeControl _treeControl;
        private ITreeGridDesignerToolWindowContainer _containerControl;
        private ITree _treeProvider;

        // Keeps track of the currently selected object.  Used to maintain selection/expansion in the tree across document window changes.
        private object _currentSelection;

        // Object we're pushing for currentSelection.  Used to restore our selection context across document window changes.
        private object _currentBrowseObject;

        private ModelingDocData _currentDocData; // DocData we are currently subscribed to. 
        private DeferredRequest _deferredExpandAllNodes;
        protected int? _previouslySelectedColumn = null;
        private VSEventBroadcaster vsEventBroadcaster;

        /// <summary>
        ///     Construct the designer tool window.
        /// </summary>
        /// <param name="serviceProvider"></param>
        protected TreeGridDesignerToolWindow(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _deferredExpandAllNodes = new DeferredRequest(ExpandAllDeferred);
        }

        /// <summary>
        ///     Dispose of resources used by the designer tool window.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
                    VSColorTheme.ThemeChanged -= VSColorTheme_ThemeChanged;

                    if (_deferredExpandAllNodes != null)
                    {
                        _deferredExpandAllNodes.Dispose();
                        _deferredExpandAllNodes = null;
                    }

                    if (_containerControl != null)
                    {
                        _containerControl.Dispose();
                        _containerControl = null;
                    }

                    if (_treeControl != null)
                    {
                        _treeControl.Dispose();
                        _treeControl = null;
                    }

                    if (vsEventBroadcaster != null)
                    {
                        vsEventBroadcaster.Dispose();
                    }
                    _treeProvider = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        ///     Overriden to create the VirtualTreeControl hosted in the tool window
        /// </summary>
        /// <returns></returns>
        protected override void OnToolWindowCreate()
        {
            Debug.Assert(_containerControl == null, "containerControl should be null at the beginning of OnCreate");

            // set up tree data
            _treeProvider = new VariableColumnTree(1);

            // set up tree control
            _treeControl = new TreeGridDesignerTreeControl(this);
            _treeControl.SelectionMode = SelectionMode.One;
            _treeControl.LabelEditSupport = VirtualTreeLabelEditActivationStyles.ImmediateMouse
                                            | VirtualTreeLabelEditActivationStyles.Explicit;
            _treeControl.HasHorizontalGridLines = true;
            _treeControl.HasVerticalGridLines = false;
            _treeControl.ShowToolTips = true;
            _treeControl.MultiColumnTree = _treeProvider as IMultiColumnTree;
            _treeControl.IsDragSource = false;
            _treeControl.MultiColumnHighlight = true;
            _treeControl.StandardCheckBoxes = true;
            _treeControl.SelectionChanged += OnTreeSelectionChanged;
            _treeControl.LabelEditControlChanged += OnLabelEditControlChanged;
            _treeControl.Site = new TreeGridDesignerSite(this);
            _treeControl.ContextMenuInvoked += OnContextMenu;

            var defaultColumnsCollection = DefaultColumns;
            if (defaultColumnsCollection != null)
            {
                _treeControl.DefaultColumns = defaultColumnsCollection;
            }

            // create container control which handles the watermark
            _containerControl = CreateContainer();
            _containerControl.SetWatermarkInfo(WatermarkInfo);
            _containerControl.WatermarkVisible = true;
            _containerControl.AccessibilityObject.Name = AccessibilityName;

            // Listen for user preference changes.  We use this to update our fonts due to high-contrast changes.
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;

            // we always want focus rectangles to be drawn
            NativeMethods.SendMessage(
                _treeControl.Handle, NativeMethods.WM_UPDATEUISTATE, (NativeMethods.UIS_CLEAR | (NativeMethods.UISF_HIDEFOCUS << 16)), 0);

            vsEventBroadcaster = new VSEventBroadcaster(this);
            vsEventBroadcaster.Initialize();
            vsEventBroadcaster.OnFontChanged += vsEventBroadcaster_OnFontChanged;

            // set the font based on VS settings
            SetFonts();

            SetTreeControlThemedColors();

            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
        }

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            SetTreeControlThemedColors();

            _containerControl.SetWatermarkThemedColors();
            _containerControl.SetToolbarThemedColors();

            // force the tree header to re-theme
            NativeMethods.PostMessage(_treeControl.Handle, e.Message, IntPtr.Zero, IntPtr.Zero);
        }

        private void SetTreeControlThemedColors()
        {
            _treeControl.SelectedItemActiveBackColor = VSColorTheme.GetThemedColor(TreeViewColors.SelectedItemActiveColorKey);
            _treeControl.SelectedItemActiveForeColor = VSColorTheme.GetThemedColor(TreeViewColors.SelectedItemActiveTextColorKey);
            _treeControl.SelectedItemInactiveBackColor = VSColorTheme.GetThemedColor(TreeViewColors.SelectedItemInactiveColorKey);
            _treeControl.SelectedItemInactiveForeColor = VSColorTheme.GetThemedColor(TreeViewColors.SelectedItemInactiveTextColorKey);
            _treeControl.DisabledItemForeColor = VSColorTheme.GetThemedColor(EnvironmentColors.SystemGrayTextColorKey);
            _treeControl.BackColor = VSColorTheme.GetThemedColor(TreeViewColors.BackgroundColorKey);
            _treeControl.ForeColor = VSColorTheme.GetThemedColor(TreeViewColors.BackgroundTextColorKey);
#if VS11
            _treeControl.GridLinesColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTabBorderColorKey);
#else
            _treeControl.GridLinesColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowContentGridColorKey);
#endif
        }

        /// <summary>
        ///     Returns the tree control hosted in the tool window.
        /// </summary>
        /// <returns></returns>
        public override IWin32Window /* WindowPane */ Window
        {
            get
            {
                // sometimes container control is null when shutting down
                IWin32Window win = null;
                if (_containerControl != null)
                {
                    win = _containerControl.Window;
                }
                return win;
            }
        }

        protected virtual ITreeGridDesignerToolWindowContainer CreateContainer()
        {
            return new TreeGridDesignerToolWindowContainer(_treeControl);
        }

        /// <summary>
        ///     Document we are tracking selection on is closing, ensure we clean everything up.
        /// </summary>
        private void OnDocumentClosing(object sender, EventArgs args)
        {
            Debug.Assert(_currentDocData != null, "null docData in DocumentClosing event");
            if (_currentDocData != null)
            {
                try
                {
                    // clear tree data, causes event handlers to be removed from our branches.
                    if (_treeProvider != null)
                    {
                        _treeProvider.Root = null;
                    }

                    // clear cached selection
                    _currentSelection = null;
                    _currentBrowseObject = null;

                    // disable column event handlers
                    Debug.Assert(_currentDocData.Store != null, "unable to remove column event handlers");
                    if (_currentDocData.Store != null)
                    {
                        _treeControl.RemoveColumnEventHandlers();
                    }

                    // NOTE: It's important to cast here, otherwise a wrong overload would be called
                    // and selection container would not be cleared leading to strange problems - e.g.
                    // properties window might call refresh on disposed store etc.
                    DoSelectionChanged((object)null);
                }
                finally
                {
                    // Unsubscribe from document closing event
                    _currentDocData.DocumentClosing -= OnDocumentClosing;
                    _currentDocData = null;
                }
            }
        }

        /// <summary>
        ///     Fired when the current document changes.
        /// </summary>
        /// <param name="oldView">Previous DocData</param>
        /// <param name="newView">Current DocData</param>
        protected override void OnDocumentWindowChanged(ModelingDocView oldView, ModelingDocView newView)
        {
            var newModelingData = newView != null ? newView.DocData : null;
            if (newModelingData != null
                && IsDocumentSupported(newModelingData))
            {
                var store = newModelingData.Store;
                if (store != null)
                {
                    if (_currentDocData != null)
                    {
                        // if we're switching stores, make sure we clear the tree.  Prevents store disposed exceptions if
                        // the currentDocData.Store is disposed before we switch back to it.
                        if (newModelingData.Store != _currentDocData.Store)
                        {
                            // clear tree data, causes event handlers to be removed from our branches.
                            if (_treeProvider != null)
                            {
                                _treeProvider.Root = null;
                            }

                            // clear cached selection
                            _currentSelection = null;
                            _currentBrowseObject = null;
                        }

                        // disable column event handlers
                        _treeControl.RemoveColumnEventHandlers();

                        // unsubscribe from document closing event
                        _currentDocData.DocumentClosing -= OnDocumentClosing;
                    }

                    // enable column event handlers
                    _treeControl.AddColumnEventHandlers();

                    // subscribe to document closing event
                    newModelingData.DocumentClosing += OnDocumentClosing;

                    // cache the doc data, so we can unsubscribe properly.  We cannot
                    // unsubscribe using oldView.DocData, because we may get an OnDocumentWindowChanged(oldModelingData, null) 
                    // just prior to a document close.  In that case, we'd unsubscribe too early, and not clean up properly
                    // in OnDocumentClosed.  Instead we wait until either we get a new supported document, or the old one closes.
                    _currentDocData = newModelingData;
                }
            }
            else
            {
                // it's possible that the oldView is not some docData we support, in that case, don't do anything
                var oldModelingData = oldView != null ? oldView.DocData : null;
                if (oldModelingData != null
                    && IsDocumentSupported(oldModelingData))
                {
                    // Null or unsupported view, clear our selection context.  Note that we leave the tree populated
                    // here so that in the common case of switching back and forth between designer and code, we 
                    // don't lose selection/expansion state in the tree.  We also clear/save the selection context,
                    // because we don't want to push anything to the property browser while the watermark is showing.
                    _currentBrowseObject = PrimarySelection;
                    SetSelectedComponents(new object[] { });
                    if (_containerControl != null)
                    {
                        _containerControl.WatermarkVisible = true;
                    }
                }
            }
        }

        /// <summary>
        ///     Fire when the font is changed in VS
        /// </summary>
        private void vsEventBroadcaster_OnFontChanged(object sender, EventArgs e)
        {
            SetFonts();
        }

        /// <summary>
        ///     Allows derived classes to specify supported Document type(s)
        /// </summary>
        protected abstract bool IsDocumentSupported(DocData document);

        /// <summary>
        ///     Called by derived classes to indicate that selection has changed.  Implements selection logic based
        ///     on attributes placed on selected objects.
        /// </summary>
        /// <returns>true if selection changed</returns>
        protected bool DoSelectionChanged(ISelectionService newSelection)
        {
            if (newSelection != null)
            {
                // operation designer only cares about the primary selection
                return DoSelectionChanged(newSelection.PrimarySelection);
            }

            return false;
        }

        /// <summary>
        ///     This will retrieve the Watermark from any derived class and force our
        ///     container to update the text.
        /// </summary>
        protected void ForceWatermarkTextChange()
        {
            if (_containerControl != null)
            {
                _containerControl.SetWatermarkInfo(WatermarkInfo);
            }
        }

        protected bool DoSelectionChanged(object selection)
        {
            return DoSelectionChanged(selection, false);
        }

        /// <summary>
        ///     Called by derived classes to indicate that selection has changed.  Implements selection logic based
        ///     on attributes placed on selected objects.
        /// </summary>
        /// <returns>true if selection changed</returns>
        protected bool DoSelectionChanged(object selection, bool force)
        {
            if (_containerControl == null)
            {
                Debug.Fail("OperationDesignerWindow must be initialized before DoSelectionChanged can be called");
                return false; // we haven't been initialized yet
            }

            if (!force
                && _currentSelection != null
                && _currentSelection == selection)
            {
                _containerControl.WatermarkVisible = false;
                if (_currentBrowseObject != null)
                {
                    try
                    {
                        // Reset selection context.  We clear this on document window switches.
                        SetSelectedComponents(new[] { _currentBrowseObject });
                    }
                    finally
                    {
                        _currentBrowseObject = null;
                    }
                }

                // nothing changed
                return false;
            }

            if (_treeControl.PopulateTree(selection))
            {
                // hide watermark
                if (_containerControl.WatermarkVisible)
                {
                    _containerControl.WatermarkVisible = false;
                }

                // cache selection
                _currentSelection = selection;

                return true;
            }
            else
            {
                // otherwise set watermark for unrecognized selection
                _containerControl.SetWatermarkInfo(WatermarkInfo);

                // we weren't previously showing the watermark, so treat this as 
                // a selection change.
                if (!_containerControl.WatermarkVisible)
                {
                    _containerControl.WatermarkVisible = true;
                    SetSelectedComponents(new object[] { }); // clear our selection context when displaying the watermark.
                    _currentSelection = null;
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected override bool CommitPendingEditForCommand(CommandID command)
        {
            // make sure we've committed any edits in the tree control
            if (_treeControl != null
                && _treeControl.InLabelEdit)
            {
                TreeGridDesignerBranch.AbortVirtualTreeEdit = false;

                try
                {
                    _treeControl.InLabelEdit = false;
                    if (TreeGridDesignerBranch.AbortVirtualTreeEdit)
                    {
                        return false;
                    }
                }
                finally
                {
                    TreeGridDesignerBranch.AbortVirtualTreeEdit = false;
                }
            }

            return true;
        }

        protected override bool PreProcessMessage(ref Message m)
        {
            //When the tree is in the process of adjust splitter and Escape key is down,
            //Pass it to treeControl.
            if (m.Msg == NativeMethods.WM_KEYDOWN)
            {
                var key = m.WParam.ToInt32();
                if (key == (int)Keys.F1)
                {
                    // do not respond to F1 Help
                    return true;
                }

                if (key == (int)Keys.F2)
                {
                    // send F2 to the tree control
                    NativeMethods.SendMessage(m.HWnd, m.Msg, key, m.LParam.ToInt32());
                    return true;
                }

                if (key == (int)Keys.Escape
                    && (_treeControl.InLabelEdit || (_treeControl.HeaderControl != null && _treeControl.HeaderControl.AdjustingColumnHeader)))
                {
                    // ensure escape is passed to the tree control if in edit or column resize mode
                    NativeMethods.SendMessage(m.HWnd, m.Msg, key, m.LParam.ToInt32());
                    return true;
                }

                if (key == (int)Keys.Tab)
                {
                    // let container handle Tab key
                    var c = _containerControl as Control;
                    return c != null && c.PreProcessMessage(ref m);
                }
                else
                {
                    var c = Control.FromHandle(m.HWnd);
                    return c != null && c.PreProcessMessage(ref m);
                }
            }

            return false;
        }

        #region Font handling

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Color)
            {
                SetColor();
            }

            SetFonts();
        }

        private void SetFonts()
        {
            var shellFont = VSHelpers.GetVSFont(this);
            // get the font used for dialogs and tool windows
            if (shellFont != null)
            {
                _containerControl.Font = shellFont;
                _treeControl.Font = shellFont;
            }
        }

        private void SetColor()
        {
            NativeMethods.SendMessage(_containerControl.Handle, NativeMethods.WM_SYSCOLORCHANGE, IntPtr.Zero, IntPtr.Zero);
        }

        #endregion

        #region event handlers

        private void OnContextMenu(object sender, ContextMenuEventArgs e)
        {
            if (ContextMenuId != null)
            {
                MenuService.ShowContextMenu(ContextMenuId, e.X, e.Y);
            }
        }

        /// <summary>
        ///     Returns the CommandID of the context menu to be shown for this tool window.
        /// </summary>
        protected abstract CommandID ContextMenuId { get; }

        /// <summary>
        /// </summary>
        /// <value></value>
        protected virtual string AccessibilityName
        {
            get { return String.Empty; }
        }

        private void OnTreeSelectionChanged(object sender, EventArgs e)
        {
            var info = _treeControl.SelectedItemInfo;
            if (info.Branch != null)
            {
                var options = 0;
                var selObj = info.Branch.GetObject(info.Row, info.Column, TreeGridDesignerBranch.BrowsingObject, ref options);
                if (selObj != null)
                {
                    var willNotUpdateSelection = SelectedElements.Contains(selObj);

                    SetSelectedComponents(new[] { selObj });

                    // SetSelectedComponents will not fire selection events if the row has not changed
                    // the column might have changed so we keep/compare state in order to fire the event
                    if ((_previouslySelectedColumn == null || TreeControl.CurrentColumn != _previouslySelectedColumn)
                        && willNotUpdateSelection)
                    {
                        OnSelectionChanged(EventArgs.Empty);
                    }

                    _previouslySelectedColumn = TreeControl.CurrentColumn;
                }
                else
                {
                    SetSelectedComponents(new object[] { });
                }
            }
        }

        private void OnLabelEditControlChanged(object sender, EventArgs e)
        {
            // set the active edit window.  this is used to route clipboard commands.
            ActiveInPlaceEditWindow = _treeControl.LabelEditControl;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gives derived classes access to the underlying tree
        /// </summary>
        protected ITree TreeProvider
        {
            get { return _treeProvider; }
        }

        /// <summary>
        ///     Returns info for the currently selected row in the tree.
        /// </summary>
        protected VirtualTreeItemInfo SelectedItemInfo
        {
            get { return _treeControl.SelectedItemInfo; }
        }

        /// <summary>
        ///     Allows to access the tree control
        /// </summary>
        internal TreeGridDesignerTreeControl TreeControl
        {
            get { return _treeControl; }
        }

        /// <summary>
        ///     Allows derived classes to access the container control
        /// </summary>
        protected ITreeGridDesignerToolWindowContainer ContainerControl
        {
            get { return _containerControl; }
        }

        protected abstract TreeGridDesignerWatermarkInfo WatermarkInfo { get; }

        /// <summary>
        ///     Allows derived classes to specify a set of default columns, which will be displayed if none can be found
        ///     via inspection of the selected object.
        /// </summary>
        protected virtual ICollection DefaultColumns
        {
            get { return null; }
        }

        /// <summary>
        ///     Returns true if the control is displaying a watermark instead of the tree.
        /// </summary>
        /// <value></value>
        internal bool WatermarkVisible
        {
            get { return _containerControl != null && _containerControl.WatermarkVisible; }
        }

        #endregion

        #region ITreeGridDesignerColumnHost

        public void /* ITreeGridDesignerColumnHost */ Invalidate(object tracking)
        {
            if (!_containerControl.WatermarkVisible)
            {
                if (tracking == null)
                {
                    _treeControl.Invalidate();
                }
                else if (TreeProvider.VisibleItemCount > 0)
                {
                    if (TreeGridDesignerBranch.InVirtualTreeEdit)
                    {
                        // a common case here is that an event handler calls this due to an edit made in the tree itself,
                        // so we look for that and don't refresh if that is the case, as it is unnecessary.
                        var itemInfo = _treeControl.SelectedItemInfo;
                        if (itemInfo.Branch != null)
                        {
                            var options = 0;
                            var currentTracking = itemInfo.Branch.GetObject(
                                itemInfo.Row, itemInfo.Column, ObjectStyle.TrackingObject, ref options);
                            if (tracking.Equals(currentTracking))
                            {
                                return;
                            }
                        }
                    }

                    // search visible rows only
                    var startIndex = _treeControl.TopIndex;

                    var itemHeight = (int)NativeMethods.SendMessage(_treeControl.Handle, NativeMethods.LB_GETITEMHEIGHT, 0, 0);
                    var endIndex = TreeProvider.VisibleItemCount;
                    var endVisibleIndex = startIndex + 1 + ((_treeControl.Height > 0) ? (_treeControl.Height / itemHeight) : 0);
                    if (endVisibleIndex < endIndex)
                    {
                        endIndex = endVisibleIndex;
                    }

                    var itemEnumerator = TreeProvider.EnumerateColumnItems(0, _treeControl.ColumnPermutation, false, startIndex, endIndex);

                    IBranch currentBranch = null;
                    while (itemEnumerator.MoveNext())
                    {
                        // just make a single call into each branch to locate the object
                        if (itemEnumerator.Branch != null
                            && itemEnumerator.Branch != currentBranch)
                        {
                            currentBranch = itemEnumerator.Branch;

                            var locateData = currentBranch.LocateObject(tracking, ObjectStyle.TrackingObject, 0);

                            if (locateData.Options == (int)TrackingObjectAction.ThisLevel)
                            {
                                // found it
                                _treeControl.InvalidateItem((itemEnumerator.RowInTree - itemEnumerator.RowInBranch) + locateData.Row);
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Retrieves the current label edit state of the tree control.
        /// </summary>
        public bool /* ITreeGridDesignerColumnHost */ InLabelEdit
        {
            get
            {
                if (_treeControl != null)
                {
                    return _treeControl.InLabelEdit;
                }

                return false;
            }
        }

        public abstract EditingContext /* ITreeGridDesignerColumnHost */ Context { get; set; }

        public void /* ITreeGridDesignerColumnHost */ ExpandAll()
        {
            if (_deferredExpandAllNodes != null
                &&
                _deferredExpandAllNodes.IsPending == false)
            {
                _deferredExpandAllNodes.Request();
            }
        }

        private int _storeCurrentIndexBeforeReload = -1;
        private int _storeCurrentColumnBeforeReload = -1;

        private void ExpandAllDeferred(object o)
        {
            for (var i = 0; i < _treeProvider.VisibleItemCount; i++)
            {
                if (_treeProvider.IsExpandable(i, 0)
                    && !_treeProvider.IsExpanded(i, 0))
                {
                    _treeProvider.ToggleExpansion(i, 0);
                }
            }

            // if we have stored off the index and column before a reload, then 
            // try and re-select that column
            IBranch currentBranch = null;
            if (_storeCurrentIndexBeforeReload >= 0
                &&
                _storeCurrentIndexBeforeReload < _treeProvider.VisibleItemCount)
            {
                _treeControl.CurrentIndex = _storeCurrentIndexBeforeReload;
                var info = _treeProvider.GetItemInfo(_storeCurrentIndexBeforeReload, 0, true);
                currentBranch = info.Branch;
            }

            if (_storeCurrentColumnBeforeReload >= 0
                &&
                currentBranch != null
                &&
                _storeCurrentColumnBeforeReload < currentBranch.VisibleItemCount)
            {
                _treeControl.CurrentColumn = _storeCurrentColumnBeforeReload;
            }

            _storeCurrentIndexBeforeReload = -1;
            _storeCurrentColumnBeforeReload = -1;
        }

        public void /* ITreeGridDesignerColumnHost */ ReloadRoot()
        {
            _storeCurrentIndexBeforeReload = _treeControl.CurrentIndex;
            _storeCurrentColumnBeforeReload = _treeControl.CurrentColumn;

            DoSelectionChanged(_currentSelection, true);
        }

        #endregion
    }
}
