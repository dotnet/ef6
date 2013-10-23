// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Forms.Integration;
    using System.Windows.Input;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.UI.Views;
    using Microsoft.Data.Entity.Design.UI.Views.Explorer;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Cursor = System.Windows.Forms.Cursor;
    using Cursors = System.Windows.Forms.Cursors;
    using MenuItem = System.Windows.Controls.MenuItem;
    using Point = System.Drawing.Point;

    internal abstract class ExplorerWindow : ToolWindowPane, IVsWindowFrameNotify3
    {
        private ExplorerInfo _currentExplorerInfo;
        private EditingContext _editingContext;
        private readonly ElementHost _elementHost;
        private readonly IXmlDesignerPackage _package;
        private OleMenuCommandService _menuService;

        private uint _monitorSelectionCookie;
        private VSEventBroadcaster vsEventBroadcaster;

        /// <summary>
        ///     Standard constructor for the tool window.
        /// </summary>
        public ExplorerWindow(IXmlDesignerPackage package)
            : base(null)
        {
            _package = package;
            _elementHost = new ElementHost();
            _elementHost.BackColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
            VSColorTheme.ThemeChanged += OnColorThemeChanged;
            _package.ModelManager.ModelChangesCommitted += OnModelChangesCommitted;
            _package.FileNameChanged += OnFileNameChanged;
        }

        private void OnColorThemeChanged(EventArgs e)
        {
            if (_elementHost != null)
            {
                _elementHost.BackColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
            }
        }

        private int OnFileNameChanged(object sender, ModelChangeEventArgs args)
        {
            // TODO: The code could be optimized so we don't have to reload the model browser when EDMX file is renamed.
            // But this should be ok for now since renaming should not be performed frequently.
            ClearAndReload();
            return VSConstants.S_OK;
        }

        internal EditingContext Context
        {
            get { return _editingContext; }
            set
            {
                // this is not the same one we have
                if (_editingContext != value)
                {
                    // if we have one, remove its event handler
                    if (_editingContext != null)
                    {
                        _editingContext.Disposing -= OnEditingContextDisposing;
                        _editingContext.Reloaded -= OnEditingContextReloaded;
                    }

                    _editingContext = value;

                    // add our handler
                    if (_editingContext != null)
                    {
                        _editingContext.Disposing += OnEditingContextDisposing;
                        _editingContext.Reloaded += OnEditingContextReloaded;
                    }

                    UpdateView();
                }
            }
        }

        internal ExplorerInfo CurrentExplorerInfo
        {
            get { return _currentExplorerInfo; }
        }

        protected EditingContext EditingContext
        {
            get { return _editingContext; }
        }

        protected OleMenuCommandService MenuCommandService
        {
            get { return _menuService; }
        }

        private void OnEditingContextDisposing(object sender, EventArgs e)
        {
            Debug.Assert(Context == sender);
            Context = null;
        }

        private void OnEditingContextReloaded(object sender, EventArgs e)
        {
            ClearAndReload();
        }

        private void ClearAndReload()
        {
            if (_currentExplorerInfo != null
                &&
                _currentExplorerInfo._explorerFrame != null)
            {
                _elementHost.Child = null;
                _currentExplorerInfo._explorerFrame.Dispose();
                _currentExplorerInfo._explorerFrame = null;
            }

            UpdateView();
        }

        /// <summary>
        ///     Hides the tool window frame if we switch to a context that doesn't need it
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame.Hide")]
        internal void Hide()
        {
            var frame = Frame as IVsWindowFrame;
            if (frame != null)
            {
                frame.Hide();
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame.Show")]
        internal void Show()
        {
            var frame = Frame as IVsWindowFrame;
            if (frame != null)
            {
                frame.Show();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void OnContentGotFocus(object sender, EventArgs e)
        {
            if (_currentExplorerInfo != null
                &&
                _currentExplorerInfo._explorerFrame != null
                &&
                _currentExplorerInfo._lastFocusedElement != null)
            {
                Keyboard.Focus(_currentExplorerInfo._lastFocusedElement);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void OnModelChangesCommitted(object sender, EfiChangedEventArgs e)
        {
            if (_currentExplorerInfo != null
                &&
                _currentExplorerInfo._explorerFrame != null)
            {
                try
                {
                    _currentExplorerInfo._explorerFrame.OnModelChangesCommitted(sender, e);
                }
                catch (Exception ex)
                {
                    Debug.Fail("Exception caught while processing changes to the explorer", ex.Message);
                    ClearAndReload();
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsMonitorSelection.UnadviseSelectionEvents(System.UInt32)")]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    VSColorTheme.ThemeChanged -= OnColorThemeChanged;
                    _package.ModelManager.ModelChangesCommitted -= OnModelChangesCommitted;
                    _package.FileNameChanged -= OnFileNameChanged;

                    if (_monitorSelectionCookie != 0)
                    {
                        // Unregister for VS Selection Events
                        var monSel = GetService(typeof(IVsMonitorSelection)) as IVsMonitorSelection;
                        if (monSel != null)
                        {
                            monSel.UnadviseSelectionEvents(_monitorSelectionCookie);
                        }

                        _monitorSelectionCookie = 0;
                    }

                    Context = null;

                    if (_elementHost != null)
                    {
                        _elementHost.Dispose();
                    }

                    if (vsEventBroadcaster != null)
                    {
                        vsEventBroadcaster.Dispose();
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        ///     This property returns the handle to the user control that should
        ///     be hosted in the Tool Window.
        /// </summary>
        public override IWin32Window Window
        {
            get { return _elementHost; }
        }

        /// <summary>
        ///     Sets the current context on the ExplorerInfo.
        /// </summary>
        protected abstract void SetExplorerInfo();

        /// <summary>
        ///     Gets the identifier of the context menu command.
        /// </summary>
        protected abstract CommandID GetContextMenuCommandID();

        /// <summary>
        ///     This is called after our control has been created and sited.
        ///     This is a good place to initialize the control with data gathered
        ///     from Visual Studio services.
        /// </summary>
        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();

            // Set the text that will appear in the title bar of the tool window.
            // Note that because we need access to the package for localization,
            // we have to wait to do this here. If we used a constant string,
            // we could do this in the consturctor.
            Caption = _package.GetResourceString("ExplorerWindowTitle");

            IServiceProvider serviceProvider = _package;
            if (serviceProvider != null)
            {
                var packageMenuService = serviceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
                _menuService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
                if (packageMenuService != null
                    && _menuService != null)
                {
                    var cmd = packageMenuService.FindCommand(StandardCommands.Delete);
                    if (cmd != null)
                    {
                        _menuService.AddCommand(cmd);
                    }

                    cmd = packageMenuService.FindCommand(StandardCommands.Cut);
                    if (cmd != null)
                    {
                        _menuService.AddCommand(cmd);
                    }

                    cmd = packageMenuService.FindCommand(StandardCommands.Copy);
                    if (cmd != null)
                    {
                        _menuService.AddCommand(cmd);
                    }

                    cmd = packageMenuService.FindCommand(StandardCommands.Paste);
                    if (cmd != null)
                    {
                        _menuService.AddCommand(cmd);
                    }

                    DefineSearchCommands();

                    vsEventBroadcaster = new VSEventBroadcaster(serviceProvider);
                    vsEventBroadcaster.Initialize();
                    vsEventBroadcaster.OnFontChanged += vsEventBroadcaster_OnFontChanged;
                }
            }
        }

        private void DefineSearchCommands()
        {
            // standard commands Command IDs - these are defined in GUID NativeMethods.GUID_VSStandardCommandSet97
            const int cmdidFindNext = 370; // find Next Search Result
            const int cmdidFindPrev = 371; // find Previous Search Result

            // define search Next/Previous commands (defines F3/shift-F3 keyboard shortcuts)
            DefineCmd(
                NativeMethods.GUID_VSStandardCommandSet97, cmdidFindNext, frame => frame.SelectNextSearchResult.Execute(null),
                frame => frame.CanGotoNextResult);
            DefineCmd(
                NativeMethods.GUID_VSStandardCommandSet97, cmdidFindPrev, frame => frame.SelectPreviousSearchResult.Execute(null),
                frame => frame.CanGotoPreviousResult);
        }

        #region IVsWindowFrameNotify3 Members

        int IVsWindowFrameNotify3.OnClose(ref uint pgrfSaveOptions)
        {
            return NativeMethods.S_OK;
        }

        int IVsWindowFrameNotify3.OnDockableChange(int fDockable, int x, int y, int w, int h)
        {
            return NativeMethods.S_OK;
        }

        int IVsWindowFrameNotify3.OnMove(int x, int y, int w, int h)
        {
            return NativeMethods.S_OK;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        int IVsWindowFrameNotify3.OnShow(int fShow)
        {
            try
            {
                if (fShow == (int)__FRAMESHOW.FRAMESHOW_WinShown)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    UpdateView();
                }
                else if (fShow == (int)__FRAMESHOW.FRAMESHOW_Hidden)
                {
                    // XXX: This is failing ...
                    // ((IVsBatchUpdate)this).FlushPendingUpdates(0);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in ExplorerWindow.OnShow(): " + e.Message + " Inner Exception: " + e.InnerException.Message);
            }

            return NativeMethods.S_OK;
        }

        int IVsWindowFrameNotify3.OnSize(int x, int y, int w, int h)
        {
            return NativeMethods.S_OK;
        }

        #endregion

        private delegate void ExecuteCmdHandler(ExplorerFrame frame);

        private delegate bool CanExecuteCmdHandler(ExplorerFrame frame);

        private void DefineCmd(Guid guidCmdSet, int cmdid, ExecuteCmdHandler executeCmd, CanExecuteCmdHandler canExecuteCmd)
        {
            var invokeHandler =
                executeCmd == null
                    ? (EventHandler)null
                    : (sender, e) =>
                        {
                            if (_currentExplorerInfo != null)
                            {
                                executeCmd(_currentExplorerInfo._explorerFrame);
                            }
                        };

            Debug.Assert(null != invokeHandler, "DefineCmd: Could not define command - null invokeHandler");
            if (null != invokeHandler)
            {
                var menuCmd = DefineCommandHandler(invokeHandler, guidCmdSet, cmdid);

                Debug.Assert(null != menuCmd, "Unable to define OleMenuCommand for GUID " + guidCmdSet + ", cmdid " + cmdid);
                if (null != menuCmd)
                {
                    menuCmd.BeforeQueryStatus += (sender, arguments) =>
                        {
                            if (null != _currentExplorerInfo)
                            {
                                var canExecute = null != _currentExplorerInfo._explorerFrame
                                                 && canExecuteCmd(_currentExplorerInfo._explorerFrame);
                                var oleMenuCommandSender = (OleMenuCommand)sender;
                                oleMenuCommandSender.Enabled = oleMenuCommandSender.Visible = canExecute;
                            }
                        };
                }
            }
        }

        /// <summary>
        ///     Define a command handler.
        ///     When the user press the button corresponding to the CommandID
        ///     the EventHandler will be called.
        /// </summary>
        /// <returns>The menu command. This can be used to set parameter such as the default visibility once the package is loaded</returns>
        internal OleMenuCommand DefineCommandHandler(EventHandler invokeHandler, Guid guidCmdSet, int cmdid)
        {
            var commandId = new CommandID(guidCmdSet, cmdid);
            if (null != _menuService)
            {
                // Add the command handler
                var command = new OleMenuCommand(invokeHandler, commandId);
                if (null != command)
                {
                    _menuService.AddCommand(command);
                    return command;
                }
            }

            return null;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void UpdateView()
        {
            if (_currentExplorerInfo != null
                && _currentExplorerInfo._explorerFrame != null)
            {
                RemoveFrameEvents();
            }

            if (_editingContext == null)
            {
                // We set the explorer frame to be null so that we can reset it
                // whenever a model is brought into focus (see 'Setting Explorer Info:' below)
                if (_currentExplorerInfo != null)
                {
                    _currentExplorerInfo._explorerFrame = null;
                }
                _currentExplorerInfo = null;
                _elementHost.Child = null;
            }
            else
            {
                _currentExplorerInfo = _editingContext.Items.GetValue<ExplorerInfo>();
                Debug.Assert(_currentExplorerInfo != null, "Couldn't get ExplorerInfo form context");
                if (_currentExplorerInfo != null)
                {
                    // Setting Explorer Info: If the explorer frame is null, then reset the explorer info from the
                    // editing context.
                    if (_currentExplorerInfo._explorerFrame == null)
                    {
                        SetExplorerInfo();
                    }

                    AddFrameEvents();

                    try
                    {
                        _elementHost.Child = _currentExplorerInfo._explorerFrame;
                    }
                    catch (Exception e)
                    {
                        Debug.Fail(e.ToString());
                    }
                }
            }
        }

        private void AddFrameEvents()
        {
            Debug.Assert(_currentExplorerInfo != null, "In AddFrameEvents: _currentExplorerInfo is null");
            Debug.Assert(_currentExplorerInfo._explorerFrame != null, "In AddFrameEvents: _currentExplorerInfo._explorerFrame is null");
            if (_currentExplorerInfo != null
                && _currentExplorerInfo._explorerFrame != null)
            {
                _currentExplorerInfo._explorerFrame.ShowContextMenu += OnShowContextMenu;
#if VIEWSOURCE
    //_currentExplorerInfo._explorerFrame.ViewSource += new EventHandler<ViewSourceEventArgs>(OnViewSource);
#endif
                //_currentExplorerInfo._explorerFrame.ActivateDesigner += new EventHandler(OnActivateDesigner);
                //_currentExplorerInfo._explorerFrame.EFElementChanged += new EventHandler<EFElementChangedEventArgs>(OnEFElementChanged);
                _currentExplorerInfo._explorerFrame.LostKeyboardFocus += OnLostKeyboardFocus;
                _currentExplorerInfo._explorerFrame.GotKeyboardFocus += OnGotKeyboardFocus;
            }
        }

        private void RemoveFrameEvents()
        {
            Debug.Assert(_currentExplorerInfo != null, "In AddFrameEvents: _currentExplorerInfo is null");
            Debug.Assert(_currentExplorerInfo._explorerFrame != null, "In AddFrameEvents: _currentExplorerInfo._explorerFrame is null");
            if (_currentExplorerInfo != null
                && _currentExplorerInfo._explorerFrame != null)
            {
                _currentExplorerInfo._explorerFrame.ShowContextMenu -= OnShowContextMenu;
#if VIEWSOURCE
    //_currentExplorerInfo._explorerFrame.ViewSource -= new EventHandler<ViewSourceEventArgs>(OnViewSource);
#endif
                //_currentExplorerInfo._explorerFrame.ActivateDesigner -= new EventHandler(OnActivateDesigner);
                //_currentExplorerInfo._explorerFrame.EFElementChanged -= new EventHandler<EFElementChangedEventArgs>(OnEFElementChanged);
                _currentExplorerInfo._explorerFrame.LostKeyboardFocus -= OnLostKeyboardFocus;
                _currentExplorerInfo._explorerFrame.GotKeyboardFocus -= OnGotKeyboardFocus;
            }
        }

        private void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_currentExplorerInfo != null)
            {
                if (_currentExplorerInfo._explorerFrame == sender)
                {
                    if (!(e.OldFocus is MenuItem))
                    {
                        _currentExplorerInfo._lastFocusedElement = e.OldFocus;
                    }
                }
            }
        }

        private void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_currentExplorerInfo != null
                && _currentExplorerInfo._explorerFrame != null)
            {
                if (_currentExplorerInfo._explorerFrame == sender)
                {
                    _currentExplorerInfo._explorerFrame.UpdateSelection();
                }
            }
        }

        protected abstract void OnBeforeShowContextMenu();

        private void OnShowContextMenu(object sender, ShowContextMenuEventArgs e)
        {
            if (MenuCommandService != null)
            {
                OnBeforeShowContextMenu();
                var explorerContextMenu = GetContextMenuCommandID();
                var p = _elementHost.PointToScreen(new Point((int)e.Point.X, (int)e.Point.Y));
                try
                {
                    MenuCommandService.ShowContextMenu(explorerContextMenu, p.X, p.Y);
                }
                catch (COMException ex)
                {
                    // do not rethrow exception as this causes VS crash
                    Debug.Fail("Caught exception of type " + ex.GetType().FullName +
                               " with message " + ex.Message + ". Stack Trace: " + ex.StackTrace);
                }
            }
        }

        private void vsEventBroadcaster_OnFontChanged(object sender, EventArgs e)
        {
            if (_elementHost != null)
            {
                _elementHost.Font = VSHelpers.GetVSFont(_package);
            }
        }

        internal class ExplorerInfo : ContextItem
        {
            internal ExplorerFrame _explorerFrame;
            internal SelectionContainer<ExplorerSelection> _selectionContainer;
            internal IInputElement _lastFocusedElement;

            internal void SetExplorerInfo(ExplorerFrame explorerFrame, SelectionContainer<ExplorerSelection> selectionContainer)
            {
                _explorerFrame = explorerFrame;
                _selectionContainer = selectionContainer;

                var context = explorerFrame.Context;
                context.Disposing += OnContextDisposing;
                context.Items.SetValue(this);
            }

            private void OnContextDisposing(object sender, EventArgs e)
            {
                var context = (EditingContext)sender;

                if (_selectionContainer != null)
                {
                    _selectionContainer.Dispose();
                    _selectionContainer = null;
                }

                if (_explorerFrame != null)
                {
                    _explorerFrame.Dispose();
                    _explorerFrame = null;
                }

                context.Items.SetValue(new ExplorerInfo());
                context.Disposing -= OnContextDisposing;
            }

            internal override Type ItemType
            {
                get { return typeof(ExplorerInfo); }
            }
        }
    }
}
