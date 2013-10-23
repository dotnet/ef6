// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Windows.Forms.VisualStyles;
    using Accessibility;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Common;
    using Microsoft.Win32;
    using Timer = System.Windows.Forms.Timer;

    #region ModifySelectionAction enum

    /// <summary>
    ///     Used with the VirtualTreeControl.SetCurrentExtendedMultiSelectIndex to specify the
    ///     modification to make to the selection state of the new caret index.
    /// </summary>
    internal enum ModifySelectionAction
    {
        /// <summary>
        ///     Do not take any special action.
        /// </summary>
        None,

        /// <summary>
        ///     Toggle the selection state of the item
        /// </summary>
        Toggle,

        /// <summary>
        ///     The item should be selected
        /// </summary>
        Select,

        /// <summary>
        ///     The item should not be selected
        /// </summary>
        Clear,
    }

    #endregion // ModifySelectionAction enum

    #region DragEffectCombinationMode enum

    /// <summary>
    ///     Specifies how drag effects are combined when multiple
    ///     items are selected for dragging. The drag object returned
    ///     by the tree needs to decide if feedback should be provided
    ///     as a union or intersection of the effects supported by the
    ///     different drag objects.
    /// </summary>
    internal enum DragEffectCombinationMode
    {
        /// <summary>
        ///     Combine drag/drop effects with the 'binary and' operator. If
        ///     the intersection is empty, then the drag operation is aborted.
        /// </summary>
        Intersection,

        /// <summary>
        ///     Combine drag/drop effects with the 'binary or' operator. The
        ///     returned effects for a union of effects supported by all of the nodes.
        /// </summary>
        Union,
    }

    #endregion // DragEffectCombinationMode enum

    /// <summary>
    ///     A control to display ITree and IMultiColumnTree implementations
    /// </summary>
    [SuppressMessage("Whitehorse.CustomRules", "WH03:WinFormControlCatchUnhandledExceptions",
        Justification = "All but critical exceptions are caught.")]
    internal partial class VirtualTreeControl : Control
    {
        #region InPlaceEdit Control Helper Code

        private class InPlaceControlHelperImpl : VirtualTreeInPlaceControlHelper
        {
            // The message launched with (a WM_?BUTTONDOWN), or - the message to indicate we saw it again.
            // Used to determine if a control was double clicked right after launch, which is difficult
            // because the host control gets the first click, and the inplace control the second.
            private int myLaunchMessage;
            private int myLaunchMessageTime;

            public InPlaceControlHelperImpl(Control inPlaceControl)
                : base(inPlaceControl)
            {
            }

            /// <summary>
            ///     Call this method from the OnKeyDown override in
            ///     the in place control.
            /// </summary>
            /// <param name="e">The KeyEventArgs</param>
            /// <returns>Returns true if the keystroke was handled, indicating that further processing is not needed.</returns>
            public override sealed bool OnKeyDown(KeyEventArgs e)
            {
                if (Parent != null)
                {
                    if (e.KeyData == Keys.Escape)
                    {
                        Parent.DismissLabelEdit(true, true);
                        e.Handled = true;
                    }
                    else if (e.KeyData == Keys.Enter)
                    {
                        Parent.DismissLabelEdit(false, true);
                        e.Handled = true;
                    }
                    else if (0 != (Flags & VirtualTreeInPlaceControls.ForwardKeyEvents))
                    {
                        Parent.OnKeyDown(e);
                    }
                }
                return e.Handled;
            }

            /// <summary>
            ///     Call this method from the OnKeyPress override in
            ///     the in place control.
            /// </summary>
            /// <param name="e">The KeyPressEventArgs</param>
            /// <returns>Returns true if the keystroke was handled, indicating that further processing is not needed.</returns>
            public override sealed bool OnKeyPress(KeyPressEventArgs e)
            {
                if (0 != (Flags & VirtualTreeInPlaceControls.ForwardKeyEvents))
                {
                    Parent.OnKeyPress(e);
                }
                if (e.KeyChar == (char)(int)Keys.Return)
                {
                    // Eat the character, so edit control wont beep!
                    e.Handled = true;
                }
                return e.Handled;
            }

            /// <summary>
            ///     Called at the beginning of the Control.OnTextChanged override.
            /// </summary>
            public override sealed void OnTextChanged()
            {
                if (Parent != null)
                {
                    Parent.LabelEditTextChanged();
                }
            }

            /// <summary>
            ///     Called at the beginning of the Control.OnLostFocus override.
            /// </summary>
            public override sealed void OnLostFocus()
            {
                if (Parent != null)
                {
                    Parent.DismissLabelEdit(false, false);
                }
            }

            /// <summary>
            ///     Called when the Flags value is changed.
            /// </summary>
            /// <param name="oldFlags">The old value of the Flags property</param>
            protected override void OnFlagsChanged(VirtualTreeInPlaceControls oldFlags)
            {
                if (Parent != null)
                {
                    Parent.LabelEditFlagsChanged(oldFlags, Flags);
                }
            }

            /// <summary>
            ///     Indicates the windows message used to create the control. Facilitates correct
            ///     mouse handling for ImmediateMouseLabelEdits where the control is transparent.
            /// </summary>
            public override int LaunchedByMessage
            {
                get { return myLaunchMessage; }
                set
                {
                    if (NativeMethods.WM_MOUSEFIRST <= value
                        && value <= NativeMethods.WM_MOUSELAST)
                    {
                        switch (value)
                        {
                            case NativeMethods.WM_LBUTTONDOWN:
                            case NativeMethods.WM_RBUTTONDOWN:
                            case NativeMethods.WM_MBUTTONDOWN:
                            case NativeMethods.WM_XBUTTONDOWN:
                                myLaunchMessage = value;
                                myLaunchMessageTime = NativeMethods.GetMessageTime();
                                return;
                        }
                    }
                    myLaunchMessage = 0;
                }
            }

            /// <summary>
            ///     Called when a mouse message is received by the inplace control. This callback
            ///     should only be used by controls that need to support transparent edit regions.
            /// </summary>
            /// <param name="message">Message structure passed to WndProc</param>
            /// <returns>True to indicate that the message has been handled and should not be forwarded to base.WndProc</returns>
            public override bool OnMouseMessage(ref Message message)
            {
                if (Parent != null)
                {
                    var messageId = message.Msg;
                    if (NativeMethods.WM_MOUSEFIRST <= messageId
                        && messageId <= NativeMethods.WM_MOUSELAST)
                    {
                        var correspondingDownMessage = 0;
                        var correspondingDoubleClickMessage = 0;
                        switch (messageId)
                        {
                                // Don't pass through down/up as these have lots of side effects that are
                                // very invasive to work around (like constantly refreshing the selection). However,
                                // pushing through double clicks and mouse moves enables tooltips and double
                                // click actions on the underlying control.
                                // UNDONE: See if we can get drag-drop working correctly. May have to forward directly
                                // with a new helper method as LBUTTONDOWN has lots of other side effects (like grabbing focus)
                            case NativeMethods.WM_LBUTTONDBLCLK:
                            case NativeMethods.WM_MBUTTONDBLCLK:
                            case NativeMethods.WM_RBUTTONDBLCLK:
                            case NativeMethods.WM_XBUTTONDBLCLK:
                            case NativeMethods.WM_MOUSEMOVE:
                                {
                                    // These messages all have client coordinates packed into the lparam. We need to adjust the
                                    // coordinates into client coordinates of the parent, then let the parent handle the message.
                                    var loc = InPlaceControl.Location;

                                    message.Result = NativeMethods.SendMessage(
                                        Parent.Handle, messageId, (int)message.WParam,
                                        NativeMethods.MAKELONG(
                                            NativeMethods.SignedLOWORD(message.LParam) + loc.X,
                                            NativeMethods.SignedHIWORD(message.LParam) + loc.Y));
                                    return true;
                                }

                            case NativeMethods.WM_MOUSEWHEEL:
                                message.Result = NativeMethods.SendMessage(
                                    Parent.Handle, messageId, (int)message.WParam, (int)message.LParam);
                                return true;
                            case NativeMethods.WM_LBUTTONDOWN:
                            case NativeMethods.WM_RBUTTONDOWN:
                            case NativeMethods.WM_MBUTTONDOWN:
                            case NativeMethods.WM_XBUTTONDOWN:
                                if (myLaunchMessage == messageId)
                                {
                                    myLaunchMessage = -messageId;
                                }
                                else
                                {
                                    myLaunchMessage = 0;
                                }
                                break;

                            case NativeMethods.WM_LBUTTONUP:
                                correspondingDownMessage = NativeMethods.WM_LBUTTONDOWN;
                                correspondingDoubleClickMessage = NativeMethods.WM_LBUTTONDBLCLK;
                                break;
                            case NativeMethods.WM_MBUTTONUP:
                                correspondingDownMessage = NativeMethods.WM_MBUTTONDOWN;
                                correspondingDoubleClickMessage = NativeMethods.WM_MBUTTONDBLCLK;
                                break;
                            case NativeMethods.WM_RBUTTONUP:
                                correspondingDownMessage = NativeMethods.WM_RBUTTONDOWN;
                                correspondingDoubleClickMessage = NativeMethods.WM_RBUTTONDBLCLK;
                                break;
                            case NativeMethods.WM_XBUTTONUP:
                                correspondingDownMessage = NativeMethods.WM_XBUTTONDOWN;
                                correspondingDoubleClickMessage = NativeMethods.WM_XBUTTONDBLCLK;
                                break;
                        }
                        if (correspondingDownMessage != 0
                            && myLaunchMessage != 0)
                        {
                            if (myLaunchMessage == -correspondingDownMessage)
                            {
                                myLaunchMessage = 0; // Only check once
                                var testTime = NativeMethods.GetMessageTime();
                                if ((testTime > myLaunchMessageTime)
                                    && ((testTime - myLaunchMessageTime) <= SystemInformation.DoubleClickTime))
                                {
                                    var loc = InPlaceControl.Location;
                                    NativeMethods.SendMessage(
                                        Parent.Handle, correspondingDoubleClickMessage, (int)message.WParam,
                                        NativeMethods.MAKELONG(
                                            NativeMethods.SignedLOWORD(message.LParam) + loc.X,
                                            NativeMethods.SignedHIWORD(message.LParam) + loc.Y));
                                }
                            }
                            else
                            {
                                myLaunchMessage = 0; // Only check once
                            }
                        }
                    }
                }
                return false;
            }

            /// <summary>
            ///     Tied to the LabelEditDirty flag on the in-place edit control.  Gives custom controls
            ///     a way to specify which edits do/do not dirty the control.
            /// </summary>
            /// <value></value>
            public override bool Dirty
            {
                get
                {
                    if (Parent != null)
                    {
                        return Parent.GetStateFlag(VTCStateFlags.LabelEditDirty);
                    }

                    return false;
                }
                set
                {
                    if (Parent != null)
                    {
                        Parent.SetStateFlag(VTCStateFlags.LabelEditDirty, value);
                    }
                }
            }

            /// <summary>
            ///     Provides in-place controls a way to display exceptions.
            /// </summary>
            public override bool DisplayException(Exception exception)
            {
                if (Parent != null)
                {
                    return Parent.DisplayException(exception);
                }

                return false;
            }
        }

        /// <summary>
        ///     Create a helper object to facilitate custom inplace editing controls that
        ///     need to attach an IVirtualTreeInPlaceControl implementation to a VirtualTreeControl instance
        /// </summary>
        /// <param name="inPlaceControl">The inplace control implementation</param>
        /// <returns>A new helper objects</returns>
        public static VirtualTreeInPlaceControlHelper CreateInPlaceControlHelper(Control inPlaceControl)
        {
            return new InPlaceControlHelperImpl(inPlaceControl);
        }

        #endregion //InPlaceEdit Control Helper Code

        #region Search String Incrementing

        internal static class SearchString
        {
            private const int Timeout = 1000;
            private static int _lastTime = NativeMethods.GetMessageTime() - 2 * Timeout;
            private static string _current = string.Empty;
            private static int _failCount;

            public static string Increment(char ch, out bool restart)
            {
                if (ch == '\0')
                {
                    Clear();
                    restart = false;
                    return _current;
                }

                var curTime = NativeMethods.GetMessageTime();
                if (curTime - _lastTime > Timeout)
                {
                    Clear();
                }
                _lastTime = curTime;

                if (_current.Length == 1
                    && _current[0] == ch)
                {
                    // This is standard behavior placed in a tree control
                    // to enable more traditional first-letter type ahead
                    // behavior. If the user quickly types the same letter
                    // twice after a clearing pause, assume that they want
                    // to jump based on the first letter.
                    restart = true;
                }
                else
                {
                    restart = _current.Length == 0;
                    _current = _current + ch;
                }
                return _current;
            }

            public static void Clear()
            {
                _failCount = 0;
                _current = string.Empty;
            }

            public static void SignalFailure()
            {
                if (_failCount == 0)
                {
                    NativeMethods.MessageBeep(0);
                }
                ++_failCount;
            }

            public static bool IsActive
            {
                get
                {
                    if (_current.Length > 0)
                    {
                        if (NativeMethods.GetMessageTime() - _lastTime < Timeout)
                        {
                            return true;
                        }
                        Clear();
                    }
                    return false;
                }
            }
        }

        #endregion // Search String Incrementing

        #region Smooth Scrolling Code

        internal static class SmoothScroll
        {
            /// <summary>
            ///     A internal object used for synchronization.
            /// </summary>
            private static object internalSyncObject;

            /// <summary>
            ///     Gets the internal object used for synchronization.
            /// </summary>
            private static object InternalSyncObject
            {
                get
                {
                    if (internalSyncObject == null)
                    {
                        var o = new object();
                        Interlocked.CompareExchange(ref internalSyncObject, o, null);
                    }
                    return internalSyncObject;
                }
            }

            private static bool smoothScrollingEnabled;
            private static bool smoothScrollingInitialized;

            internal static bool SmoothScrollingEnabled
            {
                get
                {
                    if (!smoothScrollingInitialized)
                    {
                        lock (InternalSyncObject)
                        {
                            if (!smoothScrollingInitialized)
                            {
                                smoothScrollingInitialized = true;
                                var iSmooth = 0;
                                using (var desktopKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\DeskTop"))
                                {
                                    if (desktopKey != null)
                                    {
                                        iSmooth = (int)desktopKey.GetValue("SmoothScroll", 0);
                                    }
                                }
                                smoothScrollingEnabled = iSmooth != 0;
                            }
                        }
                    }

                    return smoothScrollingEnabled;
                }
            }

            [Flags]
            internal enum Flags
            {
                None = 0x0,
                ReturnUpdateRect = 0x1,
                NoTimeLimit = 0x00010000,
                Immediate = 0x00020000,
                IgnoreSettings = 0x00040000, // ignore system settings to turn on/off smooth scroll
            }

            private const short SUBSCROLLS = 50;

            internal struct Info
            {
                public IntPtr hWnd;
                public int xChange;
                public int yChange;
                public Rectangle srcRect;
                public Rectangle clipRect;
                public IntPtr hrgnUpdate;
                public Rectangle updateRect;
                public Flags smoothScrollFlags;
                public NativeMethods.ScrollWindowFlags scrollWindowFlags;
                public short maxScrollTime;

                public Info(IntPtr handle, int dx, int dy)
                {
                    hWnd = handle;
                    xChange = dx;
                    yChange = dy;
                    srcRect = Rectangle.Empty;
                    clipRect = Rectangle.Empty;
                    hrgnUpdate = IntPtr.Zero;
                    updateRect = Rectangle.Empty;
                    smoothScrollFlags = Flags.None;
                    scrollWindowFlags = 0;
                    maxScrollTime = (short)(SystemInformation.DoubleClickTime / 2);
                }
            }

            internal static bool ScrollWindow(object owner, ref Info ssi)
            {
                var fReturnUpdate = ((ssi.smoothScrollFlags & Flags.ReturnUpdateRect) != 0);
                var fEmptySrc = ssi.srcRect.IsEmpty;
                var fEmptyClip = ssi.clipRect.IsEmpty;
                var srcRect = new NativeMethods.RECT(ssi.srcRect);
                var clipRect = new NativeMethods.RECT(ssi.clipRect);
                var updateRect = new NativeMethods.RECT(0, 0, 0, 0);
                var fImmediate = (0 != (ssi.smoothScrollFlags & Flags.Immediate))
                                 || ((0 == (ssi.smoothScrollFlags & Flags.IgnoreSettings)) && !SmoothScrollingEnabled);
                var flagParam = 0;
                var retVal = false;

                if (fImmediate)
                {
                    flagParam = NativeMethods.ScrollWindowFlagsParam(ssi.scrollWindowFlags);
                }
                else
                {
                    var maxTime = Math.Max(ssi.maxScrollTime, SUBSCROLLS);
                    flagParam = NativeMethods.ScrollWindowFlagsParam(ssi.scrollWindowFlags, maxTime);
                }

                if ((fEmptySrc != fEmptyClip) || fEmptySrc)
                {
                    // Create boxed objects as needed to get IntPtr values
                    var srcHandle = new GCHandle();
                    var clipHandle = new GCHandle();
                    var pSrc = IntPtr.Zero;
                    var pClip = IntPtr.Zero;
                    if (!fEmptySrc)
                    {
                        srcHandle = GCHandle.Alloc(srcRect, GCHandleType.Pinned);
                        pSrc = srcHandle.AddrOfPinnedObject();
                    }
                    if (!fEmptyClip)
                    {
                        clipHandle = GCHandle.Alloc(clipRect, GCHandleType.Pinned);
                        pClip = clipHandle.AddrOfPinnedObject();
                    }
                    var srcRef = new HandleRef(owner, pSrc);
                    var clipRef = new HandleRef(owner, pClip);
                    if (fReturnUpdate)
                    {
                        retVal = NativeMethods.ScrollWindowEx(
                            ssi.hWnd, ssi.xChange, ssi.yChange, srcRef.Handle, clipRef.Handle, ssi.hrgnUpdate, out updateRect, flagParam);
                    }
                    else
                    {
                        retVal = NativeMethods.ScrollWindowEx(
                            ssi.hWnd, ssi.xChange, ssi.yChange, srcRef.Handle, clipRef.Handle, ssi.hrgnUpdate, IntPtr.Zero, flagParam);
                    }
                }
                else if (fReturnUpdate)
                {
                    retVal = NativeMethods.ScrollWindowEx(
                        ssi.hWnd, ssi.xChange, ssi.yChange, ref srcRect, ref clipRect, ssi.hrgnUpdate, out updateRect, flagParam);
                }
                else
                {
                    retVal = NativeMethods.ScrollWindowEx(
                        ssi.hWnd, ssi.xChange, ssi.yChange, ref srcRect, ref clipRect, ssi.hrgnUpdate, IntPtr.Zero, flagParam);
                }

                if (fReturnUpdate)
                {
                    ssi.updateRect = new Rectangle(updateRect.left, updateRect.top, updateRect.width, updateRect.height);
                }
                return retVal;
            }
        }

        #endregion //Smooth Scrolling Code

        #region State and Style Flags

        [Flags]
        private enum VTCStyleFlags
        {
            HasLines = 0x1,
            HasRootLines = 0x2,
            HasButtons = 0x4,
            FullCellSelect = 0x8,
            MultiSelect = 0x10,
            ExtendedMultiSelect = 0x20,
            VariableHeight = 0x40,
            ShowToolTips = 0x100,
            IsDragSource = 0x200,
            // OPEN BIT
            LeftScrollBar = 0x800,
            DisplayColumnHeaders = 0x1000,
            MultiColumnHighlight = 0x2000,
            // The label edit values should be kept aligned with VirtualTreeLabelActivationStyles
            ExplicitLabelEdits = 0x4000,
            DelayedLabelEdits = 0x8000,
            ImmediateMouseLabelEdits = 0x10000,
            ImmediateSelectionLabelEdits = 0x20000,
            LabelEditsMask = ExplicitLabelEdits | DelayedLabelEdits | ImmediateMouseLabelEdits | ImmediateSelectionLabelEdits,
            LabelEditsShift = 14,
            StandardCheckBoxes = 0x40000,
            HasRootButtons = 0x80000,
            HeaderDragDrop = 0x100000,
            HeaderButtons = 0x200000,
            HeaderFullDrag = 0x400000,
            HeaderHotTrack = 0x800000,
            AutoFillFixedColumns = 0x1000000,
            MaskHasIndentBitmaps = HasLines | HasRootLines | HasButtons | HasRootButtons,
            MaskMultiSelect = MultiSelect | ExtendedMultiSelect,
            HasHorizontalGridLines = 0x2000000,
            HasVerticalGridLines = 0x4000000,
            HasGridLines = HasHorizontalGridLines | HasVerticalGridLines,
            DistinguishFocusedColumn = 0x8000000,
            UseCompatibleTextRendering = 0x10000000,
            EnableExplorerTheme = 0x20000000
        }

        [Flags]
        private enum VTCStateFlags : uint
        {
            CallDefWndProc = 0x1,
            LabelEditActive = 0x2,
            LabelEditDirty = 0x4,
            LabelEditProcessing = 0x8,
            RedrawOff = 0x10,
            NoDismissEdit = 0x20,
            InVerticalScroll = 0x40,
            InHorizontalAdjust = 0x80,
            DragDropHighlight = 0x100, // set when we should draw background highlight on the current drop target.
            StateImageHotTracked = 0x200,
            // set when we should draw the state image as hot-tracked.  Currently only supported for standard checkboxes.
            ShowingTooltip = 0x400, // set when the tooltip is displayed in the control.
            VScrollOutOfRange = 0x800,
            ScrollWait = 0x1000,
            InWmSize = 0x2000,
            InFirstWmSize = 0x4000,
            LastToolRectClipped = 0x8000,
            ChangingColumnOrder = 0x10000,
            UpdateHeaderControl = 0x20000, // Modifier for ChangingColumnOrder, not routinely cleared
            WindowPositionChanging = 0x40000,
            LabelEditTransparent = 0x80000,
            SelChangeFromMouse = 0x100000,
            RestoringSelection = 0x200000,
            ReturnedAccessibilityObject = 0x400000,
            FullColumnHeadersDirty = 0x800000,
            FixedColumnAutoFilled = 0x1000000,
            CombineDragEffectsWithAnd = 0x2000000,
            StandardLButtonDownProcessing = 0x4000000, // Used to modify CallDefWndProc
            InBumpScroll = 0x8000000,
            BumpHScrollSent = 0x10000000,
            BumpVScrollSent = 0x20000000,
            MouseButtonDownShift = 0x40000000, // set during an mouse down processing, if message contains MK_SHIFT
            MouseButtonDownCtrl = 0x80000000, // set during an mouse down processing, if message contains MK_CONTROL
            LabelEditMask = LabelEditActive | LabelEditDirty | LabelEditProcessing | LabelEditTransparent,
        }

        private enum MouseShiftStyle
        {
            None, //Don't mouse shift
            LineDown, //On line down, always shift
            PageDown //On page down, shift only if mouse is in down button change range
        }

        private const int MAGIC_MININDENT = 5;
        private const int MAGIC_INDENT = 3;
        private const int DEFAULT_INDENTWIDTH = 18;
        private const int MAGIC_HORZLINE = 5;
        private const int DRAWCHECK_WIDTH = 13;
        private const int DRAWCHECK_HEIGHT = 13;
        // Standard amount for horizontal scroll operations.  6 seems to be the standard listbox number, 
        // not sure where they get it from, but moving 1 at a time is painfully slow.
        private const int HORIZONTAL_SCROLL_AMOUNT = 6;

        #endregion //State and Style flags

        #region Member Variables

        //private delegate void CallVoid();
        private ITree myTree;
        private IMultiColumnTree myMctree;
        private int mySelectionColumn;
        private VTCStyleFlags myStyleFlags;
        private VTCStateFlags myStateFlags;
        private Bitmap myIndentBmp;
        private Color myIndentBackgroundColor;
        // UNDONE: Make some of these shorts?
        private int myIndentWidth;
        private int myItemHeight;
        private int myImageWidth;
        private int myImageHeight;
        private int myTextHeight;
        private int myStateImageWidth;
        private short myUpdateCount;
        private Timer EditTimer;
        private Timer myDragTimer;
        private int myLastDragExpandRow = VirtualTreeConstant.NullIndex;
        private int myLastDragExpandCol;
        private ImageList myImageList;
        private ImageList myStateImageList;
        private string[] myImageDescriptions;
        private string[] myStateImageDescriptions;
        private AccessibleStates[] myStateImageAccessibleStates;
        private IVirtualTreeInPlaceControl myInPlaceControl;
        private CommitLabelEditCallback myCustomInPlaceCommit;
        private Font myBoldFont;
        private StringFormat myStringFormat;
        private int myEditIndex = VirtualTreeConstant.NullIndex;
        private int myEditColumn; // Display column for the inplace edit control
        private int myMouseOverIndex = VirtualTreeConstant.NullIndex;
        private int myRawMouseOverIndex = VirtualTreeConstant.NullIndex;
        private int myMouseOverColumn;
        private VirtualTreeHitInfo myMouseDownHitInfo;
        private ToolTipType myTipType;
        private ToolTipControl myTooltip;
        private int myPuntChars; // number of WM_CHAR's to ignore
        private int myDropRow = VirtualTreeConstant.NullIndex;
        private int myDropColumn; // native index of the current drop column
        private ListBoxStateTrackerClass myShuffleTracker;

        // Scroll Positioners
        private int myMaxItemWidth; // width of longest visible item
        private Size myLastSize = Size.Empty; // Client size at last size or hscroll change
        private int myFirstMaxIndex = VirtualTreeConstant.NullIndex; // Absolute index of top visible widest item
        private int myLastMaxIndex; // Absolute index of bottom visible widest (may be the same as iFirstMaxIndex)
        private int myFullyVisibleCount = 1; // number of items that CAN fully fit in window

        // number of items that CAN at least partly fit in window when the horizontal scrollbar is not showing
        private int myPartlyVisibleCountIgnoreHScroll = 1;

        private int myXPos
        {
            get { return HasHorizontalScrollBar ? NativeMethods.GetScrollPos(Handle, NativeMethods.ScrollBarType.Horizontal) : 0; }
        }

        private int myYPos
        {
            get { return HasVerticalScrollBar ? NativeMethods.GetScrollPos(Handle, NativeMethods.ScrollBarType.Vertical) : 0; }
        }

        //private int myTop;				// first visible item
        private int myTopStartScroll; // top item when the scroll starts
        // bump scroll information
        private const int myBumpDelayTickCount = 50; // in milliseconds
        private const int myBumpInitialDelayTickCount = 500; // in milliseconds

        #endregion //Member Variables

        #region Construct and Finalize

        private IContainer components;

        /// <summary>
        ///     Create a new VirtualTreeControl instance
        /// </summary>
        public VirtualTreeControl()
        {
            SetStyle(ControlStyles.UserPaint, false);
            SetStyle(ControlStyles.ContainerControl, true);
            //SetStyle(ControlStyles.StandardClick, false);
            SetStyle(ControlStyles.StandardDoubleClick, false);
            SetBounds(0, 0, 120, 96);

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            UseVSThemeForTreeExpander = true;

            myIndentWidth = DEFAULT_INDENTWIDTH;
            myItemHeight = 16;
            myStyleFlags =
                VTCStyleFlags.MaskHasIndentBitmaps |
                VTCStyleFlags.ShowToolTips |
                VTCStyleFlags.IsDragSource |
                VTCStyleFlags.DisplayColumnHeaders |
                VTCStyleFlags.ExplicitLabelEdits |
                VTCStyleFlags.DistinguishFocusedColumn |
                VTCStyleFlags.EnableExplorerTheme;
        }

        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                if (myInPlaceControl != null)
                {
                    myInPlaceControl.InPlaceControl.Dispose();
                }
                if (myImageList != null)
                {
                    myImageList.RecreateHandle -= ImageListRecreated;
                }
                MultiColumnHighlightFocusPen = null;
                if (myBoldFont != null)
                {
                    myBoldFont.Dispose();
                }
                myBoldFont = null;
                if (focusPen != null)
                {
                    focusPen.Dispose();
                }
                focusPen = null;
            }
            base.Dispose(disposing);
        }

        #endregion Construct and Finalize

        #region State Properties

        private bool GetStateFlag(VTCStateFlags bit)
        {
            return (myStateFlags & bit) == bit;
        }

        private void SetStateFlag(VTCStateFlags bit, bool value)
        {
            if (value)
            {
                myStateFlags |= bit;
            }
            else
            {
                myStateFlags &= ~bit;
            }
        }

        /// <summary>
        ///     Determine the redraw state. Redraw is dependent on events
        ///     coming from the current Tree object and is independent of
        ///     the BeginUpdate/EndUpdate count.
        /// </summary>
        protected bool Redraw
        {
            get { return !GetStateFlag(VTCStateFlags.RedrawOff); }
            set
            {
                //NYI: Do something when this changes. Note that this is
                //sometimes used to turn off unconditionally (like WM_SIZE)
                SetStateFlag(VTCStateFlags.RedrawOff, !value);
            }
        }

        #endregion //State Properties

        #region Style Properties

        private bool GetStyleFlag(VTCStyleFlags bit)
        {
            return (myStyleFlags & bit) == bit;
        }

        private bool GetAnyStyleFlag(VTCStyleFlags bits)
        {
            return 0 != (myStyleFlags & bits);
        }

        private void SetStyleFlag(VTCStyleFlags bit, bool value)
        {
            if (value)
            {
                myStyleFlags |= bit;
            }
            else
            {
                myStyleFlags &= ~bit;
            }
        }

        /// <summary>
        ///     Determine the types of label edit support that will be attempted on the branches.
        ///     Turning on label edit support for a given branch requires setting this property
        ///     to include the given support level, setting IBranch.Features to indicate the support
        ///     level, and then responding to IBranch.BeginLabelEdit.
        /// </summary>
        [DefaultValue(VirtualTreeLabelEditActivationStyles.Explicit)]
        public VirtualTreeLabelEditActivationStyles LabelEditSupport
        {
            get
            {
                return
                    (VirtualTreeLabelEditActivationStyles)
                    ((int)(myStyleFlags & VTCStyleFlags.LabelEditsMask) >> (int)VTCStyleFlags.LabelEditsShift);
            }
            set
            {
                var styleFlags = (int)myStyleFlags;
                styleFlags = styleFlags & (int)~VTCStyleFlags.LabelEditsMask | (int)value << (int)VTCStyleFlags.LabelEditsShift;
                myStyleFlags = (VTCStyleFlags)styleFlags;
            }
        }

        /// <summary>
        ///     Display gridlines between items.  Applies to both horizontal and vertical gridlines.
        /// </summary>
        [DefaultValue(false)]
        public bool HasGridLines
        {
            get { return GetStyleFlag(VTCStyleFlags.HasGridLines); }
            set
            {
                if (value != GetStyleFlag(VTCStyleFlags.HasGridLines))
                {
                    SetStyleFlag(VTCStyleFlags.HasGridLines, value);
                    // Gridlines are accounted for in the item height, take into account now.
                    CalcItemHeight();
                    Invalidate();
                }
            }
        }

        /// <summary>
        ///     Display horizontal gridlines between items.
        /// </summary>
        [DefaultValue(false)]
        public bool HasHorizontalGridLines
        {
            get { return GetStyleFlag(VTCStyleFlags.HasHorizontalGridLines); }
            set
            {
                if (value != GetStyleFlag(VTCStyleFlags.HasHorizontalGridLines))
                {
                    SetStyleFlag(VTCStyleFlags.HasHorizontalGridLines, value);
                    Invalidate();
                }
            }
        }

        /// <summary>
        ///     Display vertical gridlines between items.
        /// </summary>
        [DefaultValue(false)]
        public bool HasVerticalGridLines
        {
            get { return GetStyleFlag(VTCStyleFlags.HasVerticalGridLines); }
            set
            {
                if (value != GetStyleFlag(VTCStyleFlags.HasVerticalGridLines))
                {
                    SetStyleFlag(VTCStyleFlags.HasVerticalGridLines, value);
                    // Vertical gridlines are accounted for in the item height, take into account now.
                    CalcItemHeight();
                    Invalidate();
                }
            }
        }

        /// <summary>
        ///     Automatically create a standard check box image list to use with
        ///     the VirtualTreeDisplayData.StateImageIndex property. Indices for
        ///     the standard checkboxes are in the StandardCheckBoxImage enum.
        ///     Note that when this property is true, if any custom images are added
        ///     to the VirtualTreeControl.StateImageList, they should be added at the
        ///     end of the list.
        /// </summary>
        [DefaultValue(false)]
        public bool StandardCheckBoxes
        {
            get { return GetStyleFlag(VTCStyleFlags.StandardCheckBoxes); }
            set
            {
                if (value != GetStyleFlag(VTCStyleFlags.StandardCheckBoxes))
                {
                    SetStyleFlag(VTCStyleFlags.StandardCheckBoxes, value);
                    // If we're turning this off and the state image list is already
                    // created, then we end up with no image list on a refresh, which
                    // can be bad, so only refresh if we're turning it on.
                    if (myStateImageList != null)
                    {
                        // The state image list will be automatically recreated as needed.
                        myStateImageList = null;
                        myStateImageDescriptions = null;
                        myStateImageAccessibleStates = null;
                        Invalidate();
                    }
                }
            }
        }

        /// <summary>
        ///     Display tooltips for truncated items and item glyphs. Defaults to true.
        /// </summary>
        [DefaultValue(true)]
        public bool ShowToolTips
        {
            get { return GetStyleFlag(VTCStyleFlags.ShowToolTips); }
            set
            {
                if (value != GetStyleFlag(VTCStyleFlags.ShowToolTips))
                {
                    if (!value)
                    {
                        DestroyTooltipWindow();
                    }
                    SetStyleFlag(VTCStyleFlags.ShowToolTips, value);
                }
            }
        }

        /// <summary>
        ///     Branches in this control will be asked to source drag objects. Defaults to true.
        /// </summary>
        [DefaultValue(true)]
        public bool IsDragSource
        {
            get { return GetStyleFlag(VTCStyleFlags.IsDragSource); }
            set
            {
                if (value != GetStyleFlag(VTCStyleFlags.IsDragSource))
                {
                    SetStyleFlag(VTCStyleFlags.IsDragSource, value);
                }
            }
        }

        /// <summary>
        ///     Display indentation lines. Changing this property
        ///     may also adjust the HasRootLines property.
        /// </summary>
        [DefaultValue(true)]
        [RefreshProperties(RefreshProperties.All)]
        public bool HasLines
        {
            get { return GetStyleFlag(VTCStyleFlags.HasLines) && !EnableExplorerTheme; }
            set
            {
                if (value != GetStyleFlag(VTCStyleFlags.HasLines))
                {
                    var flag = VTCStyleFlags.HasLines;
                    // Turn off root lines as well
                    if (!value)
                    {
                        flag |= VTCStyleFlags.HasRootLines;
                    }
                    else if (GetStyleFlag(VTCStyleFlags.HasRootButtons))
                    {
                        flag |= VTCStyleFlags.HasRootLines;
                    }
                    SetStyleFlag(flag, value);
                    IndentBitmap = null;
                    Invalidate();
                }
            }
        }

        /// <summary>
        ///     Display lines to the left of level 0 items. Changing this property
        ///     may also adjust the HasRootButtons and HasLines properties.
        /// </summary>
        [DefaultValue(true)]
        [RefreshProperties(RefreshProperties.All)]
        public bool HasRootLines
        {
            get { return GetStyleFlag(VTCStyleFlags.HasRootLines) && !EnableExplorerTheme; }
            set
            {
                if (value != GetStyleFlag(VTCStyleFlags.HasRootLines))
                {
                    var flag = VTCStyleFlags.HasRootLines;
                    // Turn on root lines as well
                    if (value)
                    {
                        flag |= VTCStyleFlags.HasLines;
                        if (GetStyleFlag(VTCStyleFlags.HasButtons))
                        {
                            flag |= VTCStyleFlags.HasRootButtons;
                        }
                    }
                    else if (GetStyleFlag(VTCStyleFlags.HasRootButtons))
                    {
                        flag |= VTCStyleFlags.HasRootButtons;
                    }
                    SetStyleFlag(flag, value);
                    IndentBitmap = null;
                    Invalidate();
                }
            }
        }

        /// <summary>
        ///     Display expand and collapse buttons in the tree. Changing this property
        ///     may also adjust the HasRootButtons property.
        /// </summary>
        [DefaultValue(true)]
        [RefreshProperties(RefreshProperties.All)]
        public bool HasButtons
        {
            get { return GetStyleFlag(VTCStyleFlags.HasButtons); }
            set
            {
                if (value != GetStyleFlag(VTCStyleFlags.HasButtons))
                {
                    var flag = VTCStyleFlags.HasButtons;
                    // Turn off root buttons as well
                    if (!value)
                    {
                        flag |= VTCStyleFlags.HasRootButtons;
                    }
                    else if (GetStyleFlag(VTCStyleFlags.HasRootLines))
                    {
                        flag |= VTCStyleFlags.HasRootButtons;
                    }
                    SetStyleFlag(flag, value);
                    IndentBitmap = null;
                    Invalidate();
                }
            }
        }

        /// <summary>
        ///     Display buttons to the left of level 0 items. Changing this property
        ///     may also adjust the HasRootLines and HasButtons properties.
        /// </summary>
        [DefaultValue(true)]
        [RefreshProperties(RefreshProperties.All)]
        public bool HasRootButtons
        {
            get { return GetStyleFlag(VTCStyleFlags.HasRootButtons); }
            set
            {
                if (value != GetStyleFlag(VTCStyleFlags.HasRootButtons))
                {
                    var flag = VTCStyleFlags.HasRootButtons;
                    // Turn on buttons as well
                    if (value)
                    {
                        flag |= VTCStyleFlags.HasButtons;
                        if (GetStyleFlag(VTCStyleFlags.HasLines))
                        {
                            flag |= VTCStyleFlags.HasRootLines;
                        }
                    }
                    else if (GetStyleFlag(VTCStyleFlags.HasRootLines))
                    {
                        flag |= VTCStyleFlags.HasRootLines;
                    }
                    SetStyleFlag(flag, value);
                    IndentBitmap = null;
                    Invalidate();
                }
            }
        }

        /// <summary>
        ///     Draw an entire cell selected instead of just the area immediately around the item text. Acts
        ///     independently of MultiColumnHightlight, which draws selection on an entire row, not just the
        ///     selected cell in that row.
        /// </summary>
        [DefaultValue(false)]
        public bool FullCellSelect
        {
            get { return GetStyleFlag(VTCStyleFlags.FullCellSelect); }
            set
            {
                if (value != GetStyleFlag(VTCStyleFlags.FullCellSelect))
                {
                    SetStyleFlag(VTCStyleFlags.FullCellSelect, value);
                    Invalidate();
                }
            }
        }

        /// <summary>
        ///     Highlight is drawn to span all columns in a row.  The focus rectangle is still
        ///     drawn for a single column only. Defaults to false.
        /// </summary>
        [DefaultValue(false)]
        public bool MultiColumnHighlight
        {
            get { return GetStyleFlag(VTCStyleFlags.MultiColumnHighlight); }
            set
            {
                if (value != GetStyleFlag(VTCStyleFlags.MultiColumnHighlight))
                {
                    SetStyleFlag(VTCStyleFlags.MultiColumnHighlight, value);
                    Invalidate();
                }
            }
        }

        /// <summary>
        ///     Set to true by default, but only has an effect if MultiColumnHighlight is also set to true.
        ///     Causes the focused cell to be drawn using the window's background color, rather than the selection color,
        ///     so that it is easier to distinguish the focused column.  This is helpful because behaviors such as keyboard
        ///     navigation and in-place edit depend on the focused column, which can be hard to see if MultiColumnHighlight
        ///     is turned on.
        /// </summary>
        [DefaultValue(true)]
        public bool DistinguishFocusedColumn
        {
            get { return GetStyleFlag(VTCStyleFlags.DistinguishFocusedColumn); }
            set
            {
                if (value != GetStyleFlag(VTCStyleFlags.DistinguishFocusedColumn))
                {
                    SetStyleFlag(VTCStyleFlags.DistinguishFocusedColumn, value);
                    Invalidate();
                }
            }
        }

        /// <summary>
        ///     Get or set the current selection mode for the control. Defaults to single select (SelectionMode.One)
        /// </summary>
        [DefaultValue(SelectionMode.One)]
        public SelectionMode SelectionMode
        {
            get
            {
                if (GetStyleFlag(VTCStyleFlags.MultiSelect))
                {
                    return GetStyleFlag(VTCStyleFlags.ExtendedMultiSelect) ? SelectionMode.MultiExtended : SelectionMode.MultiSimple;
                }
                else
                {
                    return SelectionMode.One;
                }
            }
            set
            {
                if (!Enum.IsDefined(typeof(SelectionMode), value))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(SelectionMode));
                }

                if (value == SelectionMode.None)
                {
                    throw new ArgumentException(VirtualTreeStrings.GetString(VirtualTreeStrings.SelectionModeNoneException));
                }

                VTCStyleFlags newBits = 0;
                switch (value)
                {
                        //case SelectionMode.One:
                        //	break;
                    case SelectionMode.MultiSimple:
                        newBits = VTCStyleFlags.MultiSelect;
                        break;
                    case SelectionMode.MultiExtended:
                        newBits = VTCStyleFlags.MultiSelect | VTCStyleFlags.ExtendedMultiSelect;
                        break;
                }

                if (newBits != (myStyleFlags & VTCStyleFlags.MaskMultiSelect))
                {
                    SetStyleFlag(VTCStyleFlags.MaskMultiSelect, false);
                    if (newBits != 0)
                    {
                        SetStyleFlag(newBits, true);
                    }

                    if (IsHandleCreated)
                    {
                        if (SelectionCount > 0)
                        {
                            ClearSelection(true);
                            DoSelectionChanged();
                        }
                        AnchorIndex = VirtualTreeConstant.NullIndex;
                        if (CaretIndex > 0)
                        {
                            CaretIndex = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether to use the compatible text rendering engine (GDI+) instead of GDI.
        /// </summary>
        [DefaultValue(false)]
        public bool UseCompatibleTextRendering
        {
            get { return GetStyleFlag(VTCStyleFlags.UseCompatibleTextRendering); }
            set
            {
                if (value != GetStyleFlag(VTCStyleFlags.UseCompatibleTextRendering))
                {
                    SetStyleFlag(VTCStyleFlags.UseCompatibleTextRendering, value);
                    Invalidate();
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether to use the vista explorer style.
        ///     Changing this property may also adjust the HasLines and HasRootLines properties.
        /// </summary>
        [DefaultValue(false)]
        public bool EnableExplorerTheme
        {
            get { return GetStyleFlag(VTCStyleFlags.EnableExplorerTheme) && SupportsExplorerTheme(); }
            set
            {
                if (value != GetStyleFlag(VTCStyleFlags.EnableExplorerTheme))
                {
                    SetStyleFlag(VTCStyleFlags.EnableExplorerTheme, value);
                    IndentBitmap = null;
                    Invalidate();
                }
            }
        }

        /// <summary>
        ///     Gets or sets whether to use the VS Theme tree expander icons. Defaults to true.
        /// </summary>
        public bool UseVSThemeForTreeExpander { get; set; }

        #endregion //Style Properties

        #region Standard CheckBox ImageList Generation

        private static string[] CreateStandardCheckboxDescriptions()
        {
            // Note that the list repeats to cover flat and 3d checks
            return new string[13]
                {
                    VirtualTreeStrings.GetString(VirtualTreeStrings.UncheckedAccDesc), // Unchecked 
                    VirtualTreeStrings.GetString(VirtualTreeStrings.UncheckedAccDesc), // Unchecked, hot, same as unchecked.
                    VirtualTreeStrings.GetString(VirtualTreeStrings.InactiveUncheckedAccDesc), // Unchecked, disabled
                    VirtualTreeStrings.GetString(VirtualTreeStrings.CheckedAccDesc), // Checked
                    VirtualTreeStrings.GetString(VirtualTreeStrings.CheckedAccDesc), // Checked, hot, same as checked.
                    VirtualTreeStrings.GetString(VirtualTreeStrings.InactiveCheckedAccDesc), // Checked, disabled
                    VirtualTreeStrings.GetString(VirtualTreeStrings.PartiallyCheckedAccDesc), // Mixed
                    VirtualTreeStrings.GetString(VirtualTreeStrings.PartiallyCheckedAccDesc), // Mixed, hot, same as mixed.
                    VirtualTreeStrings.GetString(VirtualTreeStrings.InactivePartiallyCheckedAccDesc), // Mixed, disabled 
                    VirtualTreeStrings.GetString(VirtualTreeStrings.UncheckedAccDesc), // Unchecked flat
                    VirtualTreeStrings.GetString(VirtualTreeStrings.CheckedAccDesc), // Checked flat
                    VirtualTreeStrings.GetString(VirtualTreeStrings.PartiallyCheckedAccDesc), // Mixed flat
                    VirtualTreeStrings.GetString(VirtualTreeStrings.InactiveUncheckedAccDesc)
                }; // Disabled flat
        }

        private static AccessibleStates[] CreateStandardCheckboxAccessibleStates()
        {
            // Positions in the array correspond to values in the StandardCheckBoxImage enumeration
            return new AccessibleStates[13]
                {
                    AccessibleStates.None, // Unchecked
                    AccessibleStates.HotTracked, // Unchecked, hot
                    AccessibleStates.Unavailable, // Unchecked, disabled
                    AccessibleStates.Checked, // Checked
                    AccessibleStates.Checked | AccessibleStates.HotTracked, // Checked, hot
                    AccessibleStates.Checked | AccessibleStates.Unavailable, // Checked, disabled
                    AccessibleStates.Indeterminate, // Mixed
                    AccessibleStates.Indeterminate | AccessibleStates.HotTracked, // Mixed, hot
                    AccessibleStates.Indeterminate | AccessibleStates.Unavailable, // Mixed, disabled
                    AccessibleStates.None, // Unchecked flat
                    AccessibleStates.Checked, // Checked flat
                    AccessibleStates.Indeterminate, // Mixed flat
                    AccessibleStates.Unavailable
                }; // Disabled flat
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "handle")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static ImageList CreateStandardCheckboxImages(Size basedOnSize)
        {
            ImageList retVal = null;
            // Determine the image size and center of the image.
            var cxImage = basedOnSize.Width;
            var cyImage = basedOnSize.Height;
            var cxCheck = DRAWCHECK_WIDTH;
            var cyCheck = DRAWCHECK_HEIGHT;
            var cx = (cxImage - cxCheck) / 2;
            if (cx < 0)
            {
                cx = 0;
                cxCheck = cxImage;
            }
            var cy = (cyImage - cyCheck) / 2;
            if (cy < 0)
            {
                cy = 0;
                cyCheck = cyImage;
            }

            // These values must be kept in sync with the StandardCheckBoxImage enumeration
            // themedStates are for use with CheckBoxRenderer class, which will draw with XP theming if available
            var themedStates = new[]
                {
                    CheckBoxState.UncheckedNormal,
                    CheckBoxState.UncheckedHot,
                    CheckBoxState.UncheckedDisabled,
                    CheckBoxState.CheckedNormal,
                    CheckBoxState.CheckedHot,
                    CheckBoxState.CheckedDisabled,
                    CheckBoxState.MixedNormal,
                    CheckBoxState.MixedHot,
                    CheckBoxState.MixedDisabled
                };
            // Theming doesn't include the notion of flat checkboxes, so we still use ControlPaint routine to draw those.
            var flatStates = new[]
                {
                    ButtonState.Normal | ButtonState.Flat,
                    ButtonState.Checked | ButtonState.Flat,
                    ButtonState.Checked | ButtonState.Inactive | ButtonState.Flat,
                    ButtonState.Inactive | ButtonState.Flat
                };
            using (var bmp = new Bitmap((themedStates.Length + flatStates.Length) * cxImage, cyImage))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.FillRectangle(Brushes.Transparent, 0, 0, bmp.Width, bmp.Height);
                    for (var i = 0; i < themedStates.Length; ++i)
                    {
                        CheckBoxRenderer.DrawCheckBox(g, new Point(cx, cy), themedStates[i]);
                        cx += cxImage;
                    }
                    for (var i = 0; i < flatStates.Length; ++i)
                    {
                        ControlPaint.DrawCheckBox(g, cx, cy, cxCheck, cyCheck, flatStates[i]);
                        cx += cxImage;
                    }
                    retVal = new ImageList();
                    retVal.ColorDepth = ColorDepth.Depth16Bit; // hot-tracked images don't look good in the default 8-bit mode.
                    retVal.ImageSize = new Size(cxImage, cyImage);
                    retVal.Images.AddStrip(bmp);
                    // ImageList oddity. The handle must be created before the 
                    // bitmap is disposed or you end up with 0 images.
                    var handle = retVal.Handle;
                }
            }
            return retVal;
        }

        #endregion // Standard CheckBox ImageList Generation

        #region Window Interaction Functions

        /// <summary>
        ///     Control.CreateParams override
        /// </summary>
        protected override CreateParams CreateParams
        {
            [SecuritySafeCritical]
            [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            [SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                var cp = base.CreateParams;
                cp.ClassName = NativeMethods.WC_LISTBOX;

                cp.Style |= NativeMethods.WS_VSCROLL | NativeMethods.LBS_NOTIFY | NativeMethods.LBS_NODATA
                            | NativeMethods.LBS_OWNERDRAWFIXED; // | NativeMethods.LBS_WANTKEYBOARDINPUT;
                //if (scrollAlwaysVisible) cp.Style |= NativeMethods.LBS_DISABLENOSCROLL;
                //if (!integralHeight)
                cp.Style |= NativeMethods.LBS_NOINTEGRALHEIGHT;

                //				switch (borderStyle) 
                //				{
                //					case BorderStyle.Fixed3D:
                //						cp.ExStyle |= NativeMethods.WS_EX_CLIENTEDGE;
                //						break;
                //					case BorderStyle.FixedSingle:
                //						cp.Style |= NativeMethods.WS_BORDER;
                //						break;
                //				}
                //cp.ExStyle |= NativeMethods.WS_EX_CLIENTEDGE;

                //if (horizontalScrollbar) 
                cp.Style |= NativeMethods.WS_HSCROLL;

                if (GetStyleFlag(VTCStyleFlags.LeftScrollBar))
                {
                    cp.ExStyle |= NativeMethods.WS_EX_LEFTSCROLLBAR;
                }

                // We implement selection logic in this control, so we specify a style of LBS_NOSEL here.
                cp.Style |= NativeMethods.LBS_NOSEL;
                cp.Style |= NativeMethods.LBS_OWNERDRAWFIXED;

                // Can't draw transparent regions in in-place controls without turning of WM_CLIPCHILDREN
                cp.Style &= ~NativeMethods.WS_CLIPCHILDREN;
                return cp;
            }
        }

        /// <summary>
        ///     Control.WndProc override
        /// </summary>
        /// <param name="m">Message</param>
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            try
            {
                _WndProc(ref m);
            }
            catch (Exception ex)
            {
                if (CriticalException.IsCriticalException(ex))
                {
                    throw; // always throw critical exceptions.
                }

                if (!DisplayException(ex))
                {
                    throw;
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.SetScrollPos(System.IntPtr,Microsoft.Data.Entity.Design.VisualStudio.NativeMethods+ScrollBarType,System.Int32,System.Boolean)")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.InvalidateRect(System.IntPtr,System.IntPtr,System.Boolean)")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.GetClientRect(System.IntPtr,Microsoft.Data.Entity.Design.VisualStudio.NativeMethods+RECT@)")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void _WndProc(ref Message m)
        {
            int lParam;
            switch (m.Msg)
            {
                case NativeMethods.WM_ERASEBKGND:
                    // Avoid drawing in area already used by items.  We will fully erase the item area
                    // as part of the draw item message.
                    var visibleItemHeight = 0;
                    if (myTree != null)
                    {
                        visibleItemHeight = (myTree.VisibleItemCount - TopIndex) * myItemHeight;
                    }

                    // Avoid using ClientRectangle here, since this is a cached WinForms property
                    // which may not be up to date in some cases, such as during a control resize.
                    NativeMethods.RECT clientRect;
                    NativeMethods.GetClientRect(Handle, out clientRect);
                    var remainingHeight = clientRect.height - visibleItemHeight;
                    if (remainingHeight > 0)
                    {
                        using (var g = Graphics.FromHdc(m.WParam))
                        {
                            Brush backBrush = null;
                            EnsureBrush(ref backBrush, BackColor);
                            try
                            {
                                g.FillRectangle(backBrush, 0, visibleItemHeight, clientRect.width, remainingHeight);
                            }
                            finally
                            {
                                CleanBrush(ref backBrush, BackColor);
                            }
                        }
                    }

                    // Nonzero return value indicates we've done the erasing.
                    m.Result = (IntPtr)1;
                    break;
                case NativeMethods.WM_NCCALCSIZE:
                    base.WndProc(ref m);
                    if ((int)m.WParam != 0 && HeaderVisible)
                    {
                        Marshal.WriteInt32(
                            m.LParam, NativeMethods.NCCALCSIZE_PARAMS.rgrc0TopOffset,
                            HeaderHeight + Marshal.ReadInt32(m.LParam, NativeMethods.NCCALCSIZE_PARAMS.rgrc0TopOffset));
                        myBorderOffset = -1;
                    }
                    break;
                case NativeMethods.WM_LBUTTONUP:
                    {
                        var restoreSelChange = GetStateFlag(VTCStateFlags.SelChangeFromMouse);
                        if (restoreSelChange)
                        {
                            SetStateFlag(VTCStateFlags.SelChangeFromMouse, true);
                        }
                        base.WndProc(ref m);
                        if (restoreSelChange)
                        {
                            SetStateFlag(VTCStateFlags.SelChangeFromMouse, false);
                        }
                    }
                    break;
                case NativeMethods.WM_KILLFOCUS:
                case NativeMethods.WM_SETFOCUS:
                    // If we're setting focus to a fake focus control, then don't
                    // redraw now. In particular, doing this with the header control
                    // will mess up the splitter lines.
                    if (!GetStateFlag(VTCStateFlags.RedrawOff)
                        && myUpdateCount == 0
                        && (m.WParam == IntPtr.Zero || !(IsDrawWithFocusWindow(m.WParam) || IsInPlaceEditWindow(m.WParam))))
                    {
                        RedrawVisibleSelectedItems();
                    }

                    // notify accessibility clients of focus change to caret item
                    if (m.Msg == NativeMethods.WM_SETFOCUS
                        && CurrentIndex != VirtualTreeConstant.NullIndex)
                    {
                        if (VirtualTreeAccEvents.ShouldNotify(VirtualTreeAccEvents.eventObjectFocus, this))
                        {
                            VirtualTreeAccEvents.Notify(
                                VirtualTreeAccEvents.eventObjectFocus, CurrentIndex,
                                CurrentColumn, this);
                        }
                    }

                    // We don't call the base on a WM_SETFOCUS, since DefWndProc will send a focus WinEvents,
                    // which we don't want.  We still need to raise events, however.
                    if (m.Msg == NativeMethods.WM_SETFOCUS)
                    {
                        OnGotFocus(EventArgs.Empty);
                    }
                    else
                    {
                        OnLostFocus(EventArgs.Empty);
                    }
                    break;
                case NativeMethods.WM_REFLECT + NativeMethods.WM_CHARTOITEM:
                    m.Result = (IntPtr)(-1);
                    break;
                case NativeMethods.WM_REFLECT + NativeMethods.WM_DRAWITEM:
                    WmReflectDrawItem(ref m);
                    break;
                case NativeMethods.WM_LBUTTONDBLCLK:
                    SetStyle(ControlStyles.UserMouse, true);
                    SetStateFlag(VTCStateFlags.CallDefWndProc, true);
                    base.WndProc(ref m);
                    if (GetStateFlag(VTCStateFlags.CallDefWndProc))
                    {
                        DefWndProc(ref m);
                    }
                    SetStyle(ControlStyles.UserMouse, false);
                    break;
                case NativeMethods.WM_LBUTTONDOWN:
                    {
                        // Handling the mouse down is a very delicate operation that
                        // involves letting us, and winforms all having a shot
                        // at the operation. WinForms does some useful things and sets
                        // internal state indicating the the mouse is down (and gets
                        // the capture, etc), but we don't want them to call DefWndProc
                        // because that will automatically do things like change selection
                        // when we click on an expansion button, or drag select in multiselect
                        // mode when we're trying to do a drag drop. The UserMouse=true style
                        // used here stops the control from calling DefWndProc so we can decide
                        // ourselves if it needs to be called or not to do standard selection
                        // processing for the control. Note that base.WndProc call here sets
                        // the WinForms state and calls our OnMouseDown, where most of the 
                        // processing is done.
                        var startIndex = CurrentIndex;
                        var skipStandardProcessing = true;
                        var localCallDefWndProc = true;
                        SetStyle(ControlStyles.UserMouse, true);
                        SetStateFlag(VTCStateFlags.CallDefWndProc, true);
                        SetStateFlag(VTCStateFlags.SelChangeFromMouse, true);
                        SetStateFlag(VTCStateFlags.StandardLButtonDownProcessing, false);
                        // WinForms discards the modifer key information in the original message.  Capture that here for use in OnMouseDown.  
                        if (0 != ((int)m.WParam & NativeMethods.MK_CONTROL))
                        {
                            SetStateFlag(VTCStateFlags.MouseButtonDownCtrl, true);
                        }
                        if (0 != ((int)m.WParam & NativeMethods.MK_SHIFT))
                        {
                            SetStateFlag(VTCStateFlags.MouseButtonDownShift, true);
                        }
                        try
                        {
                            base.WndProc(ref m);
                            localCallDefWndProc = GetStateFlag(VTCStateFlags.CallDefWndProc);
                            if (localCallDefWndProc)
                            {
                                if (myMouseDownHitInfo.HitTarget != VirtualTreeHitTargets.NoWhere)
                                {
                                    skipStandardProcessing = false;
                                    if (RequireColumnSwitchForSelection(ref myMouseDownHitInfo))
                                    {
                                        var columnChangeOnly = startIndex == myMouseDownHitInfo.Row;
                                        SetSelectionColumn(myMouseDownHitInfo.DisplayColumn, false);
                                            // don't fire events here, we'll fire when we set selection.
                                        if (columnChangeOnly)
                                        {
                                            startIndex = -1;
                                        }
                                    }
                                }
                            }
                            else if (GetStateFlag(VTCStateFlags.StandardLButtonDownProcessing))
                            {
                                // The selection pieces have already been taken care of, to the drag-drop and
                                // label edit bits.
                                skipStandardProcessing = false;
                                startIndex = -1;
                            }
                            SetStyle(ControlStyles.UserMouse, false);
                            var checkForDrag = !skipStandardProcessing && IsDragSource;
                            var multiSelect = GetStyleFlag(VTCStyleFlags.MultiSelect);
                            var fContinue = true;
                            if (checkForDrag && multiSelect)
                            {
                                // MultiSelect drag needs to be initiated before the DoSelectionChangeFromMouse
                                // call, not after. In the single select case, the default processing
                                // will select an item. But in the multiselect case, it will deselect
                                // the item if it is already selected, so we need to check for
                                // starting the drag before doing the default processing.
                                if (IsSelected(myMouseDownHitInfo.Row))
                                {
                                    checkForDrag = false;
                                    var p = new Point((int)m.LParam);
                                    if (CheckForDragBegin(
                                        p.X, p.Y, myMouseDownHitInfo.Row, myMouseDownHitInfo.DisplayColumn, ref localCallDefWndProc))
                                    {
                                        // Code repeated below
                                        var dragData = PopulateDragData(
                                            myMouseDownHitInfo.Row, myMouseDownHitInfo.DisplayColumn, DragReason.DragDrop);
                                        if (!dragData.IsEmpty)
                                        {
                                            if (startIndex != -1
                                                && startIndex != myMouseDownHitInfo.Row)
                                            {
                                                DoSelectionChanged();
                                            }
                                            DoDragDrop(dragData.Data, dragData.AllowedEffects);
                                            fContinue = false;
                                        }
                                    }
                                }
                                else
                                {
                                    checkForDrag = localCallDefWndProc; // Nothing is going to change
                                }
                            }
                            if (fContinue && localCallDefWndProc)
                            {
                                DoSelectionChangeFromMouse(
                                    ref myMouseDownHitInfo, 0 != ((int)m.WParam & NativeMethods.MK_SHIFT),
                                    0 != ((int)m.WParam & NativeMethods.MK_CONTROL), MouseButtons.Left);
                                if (startIndex == -1)
                                {
                                    // startIndex == -1 indicates we already set column selection above, and the caret index isn't changing.
                                    // need to fire events here, since to DoSelectionChangeFromMouse, it will look like nothing changed.
                                    DoSelectionChanged();
                                }
                            }
                            if (checkForDrag
                                &&
                                ((myMouseDownHitInfo.HitTarget
                                  & (VirtualTreeHitTargets.OnItem | VirtualTreeHitTargets.OnItemRight | VirtualTreeHitTargets.OnItemLeft))
                                 != 0)
                                &&
                                (!multiSelect || IsSelected(myMouseDownHitInfo.Row)))
                            {
                                var p = new Point((int)m.LParam);
                                var dummyPending = false;
                                if (CheckForDragBegin(
                                    p.X, p.Y, myMouseDownHitInfo.Row, myMouseDownHitInfo.DisplayColumn, ref dummyPending))
                                {
                                    // Code copied from above
                                    var dragData = PopulateDragData(
                                        myMouseDownHitInfo.Row, myMouseDownHitInfo.DisplayColumn, DragReason.DragDrop);
                                    if (!dragData.IsEmpty)
                                    {
                                        if (startIndex != -1
                                            && startIndex != myMouseDownHitInfo.Row)
                                        {
                                            DoSelectionChanged();
                                        }
                                        DoDragDrop(dragData.Data, dragData.AllowedEffects);
                                        fContinue = false;
                                    }
                                }
                            }
                            if (!skipStandardProcessing
                                && fContinue
                                &&
                                (0
                                 != (myMouseDownHitInfo.HitTarget
                                     & (VirtualTreeHitTargets.OnItemLabel | VirtualTreeHitTargets.OnItemRight
                                        | VirtualTreeHitTargets.OnItemLeft))))
                            {
                                StartEditTimer(myMouseDownHitInfo.Row, m.Msg);
                            }
                        }
                        finally
                        {
                            SetStateFlag(VTCStateFlags.MouseButtonDownCtrl, false);
                            SetStateFlag(VTCStateFlags.MouseButtonDownShift, false);
                            SetStateFlag(VTCStateFlags.SelChangeFromMouse, false);
                        }
                        break;
                    }
                case NativeMethods.WM_RBUTTONDOWN:
                    // Focus doesn't happen automatically on a right
                    // mouse click. Set this up front before other
                    // mouse events start firing.
                    if (!Focused)
                    {
                        Focus();
                    }
                    // WinForms discards the modifer key information in the original message.  Capture that here for use in OnMouseDown.  
                    if (0 != ((int)m.WParam & NativeMethods.MK_CONTROL))
                    {
                        SetStateFlag(VTCStateFlags.MouseButtonDownCtrl, true);
                    }
                    if (0 != ((int)m.WParam & NativeMethods.MK_SHIFT))
                    {
                        SetStateFlag(VTCStateFlags.MouseButtonDownShift, true);
                    }
                    try
                    {
                        base.WndProc(ref m);
                    }
                    finally
                    {
                        SetStateFlag(VTCStateFlags.MouseButtonDownCtrl, false);
                        SetStateFlag(VTCStateFlags.MouseButtonDownShift, false);
                    }
                    break;
                case NativeMethods.WM_MOUSEMOVE:
                    if (myTooltip != null || StandardCheckBoxes)
                    {
                        // update tooltips and checkbox hot-track state. 
                        UpdateMouseTargets();
                        if (myTooltip != null)
                        {
                            myTooltip.Relay(m.WParam, m.LParam);
                        }
                    }
                    base.WndProc(ref m);
                    break;
                case NativeMethods.WM_NCMOUSEMOVE:
                    if (myTooltip != null || StandardCheckBoxes)
                    {
                        // update tooltips and checkbox hot-track state. 
                        UpdateMouseTargets();
                        if (myTooltip != null)
                        {
                            myTooltip.Relay(m.WParam, m.LParam);
                        }
                    }
                    base.WndProc(ref m);
                    break;
                case NativeMethods.WM_MOUSELEAVE:
                    if (myRawMouseOverIndex != -1)
                    {
                        // make sure to invalidate checkbox hot-track region if necessary when the mouse leaves the control.
                        UpdateMouseTargets();
                    }
                    base.WndProc(ref m);
                    break;
                case NativeMethods.WM_NOTIFY:
                    // NMHDR is layed out hwndFrom/idFrom/code
                    if (myTooltip != null
                        &&
                        myTooltip.IsHandleCreated
                        &&
                        Marshal.ReadIntPtr(m.LParam) == myTooltip.Handle)
                    {
                        var fContinue = false;

#if DEBUG
                        // - to not have to compile structs used only in Asserts
                        Debug.Assert(Marshal.OffsetOf(typeof(NativeMethods.NMHDR), "code").ToInt32() == 8);
                        Debug.Assert(Marshal.OffsetOf(typeof(NativeMethods.TOOLTIPTEXT), "lpszText").ToInt32() == 12);
#endif
                        var code = Marshal.ReadIntPtr(m.LParam, 8).ToInt32();
                        switch (code)
                        {
                            case NativeMethods.TTN_NEEDTEXTA:
                            case NativeMethods.TTN_NEEDTEXTW:
                                if (myMouseOverIndex == VirtualTreeConstant.NullIndex)
                                {
                                    if (!UpdateMouseTargets())
                                    {
                                        fContinue = true;
                                        break;
                                    }
                                }
                                // This gets the active selection column,
                                // need to support all columns.
                                Marshal.WriteIntPtr(
                                    m.LParam,
                                    12,
                                    myTooltip.GetTextPtr(myTree, myMouseOverIndex, myMouseOverColumn, myTipType));
                                break;
                            case NativeMethods.TTN_SHOW:
                                if (myMouseOverIndex != VirtualTreeConstant.NullIndex)
                                {
                                    SetStateFlag(VTCStateFlags.ShowingTooltip, true);
                                    myTooltip.PositionTipWindow(this);
                                    m.Result = (IntPtr)1;
                                }
                                break;
                            case NativeMethods.TTN_POP:
                                SetStateFlag(VTCStateFlags.ShowingTooltip, false);
                                fContinue = true;
                                break;
                            default:
                                fContinue = true;
                                break;
                        }
                        if (!fContinue)
                        {
                            break;
                        }
                    }
                    base.WndProc(ref m);
                    break;

                case NativeMethods.WM_WINDOWPOSCHANGING:
                    {
                        var flags = (NativeMethods.SetWindowPosFlags)Marshal.ReadInt32(m.LParam, NativeMethods.WINDOWPOS.flagsOffset);
                        if (((NativeMethods.SetWindowPosFlags.SWP_NOSIZE | NativeMethods.SetWindowPosFlags.SWP_NOMOVE)
                             != (flags & (NativeMethods.SetWindowPosFlags.SWP_NOSIZE | NativeMethods.SetWindowPosFlags.SWP_NOMOVE)))
                            || (0 != (flags & NativeMethods.SetWindowPosFlags.SWP_FRAMECHANGED)))
                        {
                            // The exclusions are a strange case, but Windows was not sending a
                            // WM_WINDOWPOSCHANGED if SetWindowPos was called with NOMOVE | NOSIZE,
                            // so this was not getting turned back on and DrawItem was blocked until
                            // the next size
                            SetStateFlag(VTCStateFlags.WindowPositionChanging, true);
                        }
                        base.WndProc(ref m);
                        break;
                    }
                case NativeMethods.WM_WINDOWPOSCHANGED:
                    {
                        var oldWidth = ClientSize.Width;
                        base.WndProc(ref m);
                        SetStateFlag(VTCStateFlags.WindowPositionChanging, false);
                        var flags = (NativeMethods.SetWindowPosFlags)Marshal.ReadInt32(m.LParam, NativeMethods.WINDOWPOS.flagsOffset);
                        var sizeChanged = 0 == (flags & NativeMethods.SetWindowPosFlags.SWP_NOSIZE)
                                          && (oldWidth != ClientSize.Width && Width > 0);
                        if (HeaderVisible && myHeaderContainer != null)
                        {
                            var visibleChanged = ((flags
                                                   & (NativeMethods.SetWindowPosFlags.SWP_HIDEWINDOW
                                                      | NativeMethods.SetWindowPosFlags.SWP_SHOWWINDOW)) != 0);
                            var frameChanged = 0 != (flags & NativeMethods.SetWindowPosFlags.SWP_FRAMECHANGED);
                            var positionChanged = 0 == (flags & NativeMethods.SetWindowPosFlags.SWP_NOMOVE);
                            var zorderChanged = 0 == (flags & NativeMethods.SetWindowPosFlags.SWP_NOZORDER);
                            if (visibleChanged)
                            {
                                myHeaderContainer.Visible = ((flags & NativeMethods.SetWindowPosFlags.SWP_SHOWWINDOW) != 0);
                            }
                            if (sizeChanged || frameChanged)
                            {
                                myHeaderContainer.UpdateHeaderControlPosition(true);
                            }
                            if (zorderChanged)
                            {
                                myHeaderContainer.UpdateHeaderControlZOrder();
                            }

                            if (positionChanged
                                || frameChanged
                                || sizeChanged)
                            {
                                RepositionHeaderContainer();
                            }
                        }
                        if (sizeChanged)
                        {
                            if (myMctree != null
                                && myUpdateCount == 0)
                            {
                                var columns = (myColumnPermutation != null) ? myColumnPermutation.VisibleColumnCount : myMctree.ColumnCount;
                                if (columns > 1)
                                {
                                    // Following are conditions where we need to refresh completely on a resize:
                                    // 1.  Variable column bounds.  These change when resize occurs.
                                    // 2.  Left scroll bar.  This may adjust column widths
                                    // 3.  No vertical gridlines.  This causes us to use ellipsis string trimming, which may require repaint if text was truncated.
                                    if ((myHeaderBounds.HasVariableBounds || LeftScrollBar || !HasVerticalGridLines)
                                        && 0 == (flags & NativeMethods.SetWindowPosFlags.SWP_NOREDRAW))
                                    {
                                        Refresh();
                                    }
                                }
                            }
                        }
                        break;
                    }
                case NativeMethods.WM_SIZE:
                    {
                        lParam = (int)m.LParam;
                        var curSize = new Size(NativeMethods.UnsignedLOWORD(lParam), NativeMethods.UnsignedHIWORD(lParam));
                        var cacheLastSize = myLastSize;
                        var checkHScrollHack =
                            !GetStateFlag(VTCStateFlags.InFirstWmSize) &&
                            (curSize.Width != cacheLastSize.Width);
                        var iPrevPos = 0;
                        var scrollRegionClean = false;
                        if (checkHScrollHack)
                        {
                            checkHScrollHack = HasHorizontalScrollBar;
                            if (checkHScrollHack)
                            {
                                iPrevPos = myXPos;
                                // There are several options here to avoid the braindead (and heavily flashing)
                                // approach of fully invalidating the window when this happens.
                                var hRgn = NativeMethods.CreateRectRgn(0, 0, 0, 0);
                                if (hRgn != IntPtr.Zero)
                                {
                                    var regType = NativeMethods.GetUpdateRgn(m.HWnd, hRgn, false);
                                    switch (regType)
                                    {
                                        case NativeMethods.RegionType.Complex:
                                        case NativeMethods.RegionType.Simple:
                                            {
                                                var testRect = new NativeMethods.RECT(0, 0, cacheLastSize.Width, cacheLastSize.Height);
                                                if (NativeMethods.RectInRegion(hRgn, ref testRect))
                                                {
                                                    scrollRegionClean = false;
                                                }
                                                break;
                                            }
                                    }
                                    NativeMethods.DeleteObject(hRgn);
                                }
                            }
                        }
                        //Debug.WriteLine("WM_SIZE Before base WndProc " + myXPos.ToString() + " " + HasHorizontalScrollBar.ToString() + cacheLastSize.Width.ToString() + "/" + cacheLastSize.Height.ToString());
                        SetStateFlag(VTCStateFlags.InWmSize, true);
                        var inFirstSize = !GetStateFlag(VTCStateFlags.InFirstWmSize);
                        if (inFirstSize)
                        {
                            Redraw = false;
                            SetStateFlag(VTCStateFlags.InFirstWmSize, true);
                        }
                        base.WndProc(ref m);
                        if (inFirstSize)
                        {
                            SetStateFlag(VTCStateFlags.InFirstWmSize, false);
                            Redraw = true;
                        }
                        if (GetStateFlag(VTCStateFlags.InWmSize))
                        {
                            // WM_SIZE can recurse as the scrollbars come and go, use
                            // the last pass, not the first, to record the new size, but
                            // always use the first pass to do the scrolling.
                            myLastSize = curSize;
                            SetStateFlag(VTCStateFlags.InWmSize, false);
                        }
                        if (checkHScrollHack)
                        {
                            if (iPrevPos != myXPos)
                            {
                                if (scrollRegionClean)
                                {
                                    var ssi = new SmoothScroll.Info(m.HWnd, myLastSize.Width - cacheLastSize.Width, 0);
                                    ssi.scrollWindowFlags = NativeMethods.ScrollWindowFlags.Invalidate
                                                            | NativeMethods.ScrollWindowFlags.Erase;
                                    ssi.smoothScrollFlags = SmoothScroll.Flags.Immediate;
                                    ssi.srcRect.Width = cacheLastSize.Width;
                                    ssi.srcRect.Height = cacheLastSize.Height;
                                    Debug.WriteLine("Before Scrolling Window");
                                    SmoothScroll.ScrollWindow(this, ref ssi);
                                    Debug.WriteLine("After Scrolling Window");
                                }
                                else
                                {
                                    NativeMethods.InvalidateRect(m.HWnd, IntPtr.Zero, true);
                                }
                            }
                        }
                        if (inFirstSize)
                        {
                            //Debug.WriteLine("WM_SIZE After base WndProc " + myXPos.ToString() + " " + HasHorizontalScrollBar.ToString() + myLastSize.Width.ToString() + "/" + myLastSize.Height.ToString());
                            SizeWnd(myLastSize.Width, myLastSize.Height);
                        }
                        break;
                    }
                case NativeMethods.WM_HSCROLL:
                    DismissLabelEdit(false, true);
                    base.WndProc(ref m);
                    if (myHeaderContainer != null)
                    {
                        if (ItemCount == 0)
                        {
                            // DefWndProc does nothing if there are no items in the tree, so we
                            // need to do it ourselves to get the scrollbar to move.
                            var si = new NativeMethods.SCROLLINFO(0);
                            si.fMask = NativeMethods.ScrollInfoFlags.All;
                            NativeMethods.GetScrollInfo(m.HWnd, NativeMethods.ScrollBarType.Horizontal, ref si);
                            var newPos = si.nPos;
                            switch ((NativeMethods.ScrollAction)NativeMethods.UnsignedLOWORD((int)m.WParam))
                            {
                                case NativeMethods.ScrollAction.LineLeft:

                                    newPos -= HORIZONTAL_SCROLL_AMOUNT;
                                    break;
                                case NativeMethods.ScrollAction.LineRight:
                                    newPos += HORIZONTAL_SCROLL_AMOUNT;
                                    break;
                                case NativeMethods.ScrollAction.PageLeft:
                                    newPos -= si.nPage;
                                    break;
                                case NativeMethods.ScrollAction.PageRight:
                                    newPos += si.nPage;
                                    break;
                                case NativeMethods.ScrollAction.ThumbPosition:
                                case NativeMethods.ScrollAction.ThumbTrack:
                                    newPos = si.nTrackPos;
                                    break;
                            }
                            if (newPos != si.nPos)
                            {
                                newPos = Math.Min(Math.Max(0, newPos), si.nMax - si.nPage + 1);
                                if (newPos != si.nPos)
                                {
                                    NativeMethods.SetScrollPos(m.HWnd, NativeMethods.ScrollBarType.Horizontal, newPos, true);
                                }
                            }
                        }
                        myHeaderContainer.UpdateHeaderControlPosition(false);
                    }
                    //	wParam = (int)m.WParam;
                    //	HorzScroll((NativeMethods.ScrollAction)NativeMethods.LOWORD(wParam), NativeMethods.HIWORD(wParam));
                    break;

                case NativeMethods.WM_VSCROLL:
                    {
                        var fEndScroll =
                            (NativeMethods.ScrollAction)NativeMethods.UnsignedLOWORD((int)m.WParam) == NativeMethods.ScrollAction.EndScroll;
                        if (!fEndScroll
                            && !GetStateFlag(VTCStateFlags.InVerticalScroll))
                        {
                            DismissLabelEdit(false, true);
                            SetStateFlag(VTCStateFlags.InVerticalScroll, true);
                            myTopStartScroll = TopIndex;
                        }
                        base.WndProc(ref m);
                        if (fEndScroll)
                        {
                            SetStateFlag(VTCStateFlags.InVerticalScroll, false);
                            //BeginInvoke(new CallVoid(VScrollCompleted));
                            VScrollCompleted();
                        }
                        //VertScroll((NativeMethods.ScrollAction)NativeMethods.LOWORD(wParam), NativeMethods.HIWORD(wParam));
                        break;
                    }
                case NativeMethods.WM_MOUSEWHEEL:
                    DismissLabelEdit(false, true);
                    base.WndProc(ref m);
                    if (myHeaderContainer != null
                        && HasHorizontalScrollBar
                        && !HasVerticalScrollBar)
                    {
                        // if there's no vertical scroll bar, mousehweel scrolls horizontally so we need to
                        // update the header.
                        myHeaderContainer.UpdateHeaderControlPosition(false);
                    }
                    break;

                    //				case NativeMethods.WM_LBUTTONUP:
                    //					// Get the mouse location
                    //					//
                    //					int x = (int)(short)m.LParam;
                    //					int y = (int)m.LParam >> 16;
                    //					Point pt = new Point(x,y);
                    //					pt = PointToScreen(pt);
                    //					bool captured = Capture;
                    //					if (captured && UnsafeNativeMethods.WindowFromPoint(pt.X, pt.Y) == Handle) 
                    //					{
                    //						if (selectedItems != null) 
                    //						{
                    //							selectedItems.Dirty();
                    //						}
                    //						
                    //						if (!doubleClickFired && !ValidationCancelled) 
                    //						{
                    //							OnClick(EventArgs.Empty);
                    //						}
                    //						else 
                    //						{
                    //							doubleClickFired = false;
                    //							// WM_COMMAND is only fired if the user double clicks an item,
                    //							// so we can't use that as a double-click substitute
                    //							if (!ValidationCancelled) 
                    //							{
                    //								OnDoubleClick(EventArgs.Empty);
                    //							}
                    //						}
                    //					}
                    //					base.WndProc(ref m);
                    //					doubleClickFired = false;
                    //					break;
                    //
                    //				case NativeMethods.WM_LBUTTONDBLCLK:
                    //					//the Listbox gets  WM_LBUTTONDOWN - WM_LBUTTONUP -WM_LBUTTONDBLCLK - WM_LBUTTONUP...
                    //					//sequence for doubleclick...
                    //					//the first WM_LBUTTONUP, resets the flag for Doubleclick
                    //					//So its necessary for us to set it again...
                    //					doubleClickFired = true;
                    //					base.WndProc(ref m);
                    //					break;
                    //				
                    //				case NativeMethods.WM_WINDOWPOSCHANGED:
                    //					base.WndProc(ref m);
                    //					if (integralHeight && fontIsChanged) 
                    //					{
                    //						Height = Math.Max(Height,ItemHeight);
                    //						fontIsChanged = false;
                    //					}
                    //					break;
                case NativeMethods.WM_CONTEXTMENU:
                    ContextMenuEventArgs e = null;
                    if (m.LParam.ToInt32() != -1)
                    {
                        e = new ContextMenuEventArgs(NativeMethods.SignedLOWORD(m.LParam), NativeMethods.SignedHIWORD(m.LParam));
                    }
                    else
                    {
                        // context menu was invoked through the keyboard.  determine location based on
                        // current selection within the tree.
                        var index = CurrentIndex;
                        Point p;
                        if (index != -1)
                        {
                            var r = GetItemRectangle(CurrentIndex, CurrentColumn, true, true, null);
                            p = PointToScreen(new Point(r.Right, r.Bottom));
                        }
                        else
                        {
                            p = PointToScreen(Point.Empty);
                        }
                        e = new ContextMenuEventArgs(p.X, p.Y);
                    }
                    OnContextMenuInvoked(e);
                    break;
                case NativeMethods.WM_GETOBJECT:
                    OnWmGetObject(ref m);
                    break;
                case NativeMethods.WM_SYSCOLORCHANGE:
                case NativeMethods.WM_THEMECHANGED:
                    // theme changed, invalidate the indent bitmaps, which may contain themed button glyphs.
                    // this will get recreated the next time it's needed for painting
                    IndentBitmap = null;
                    // Also, let the header control change its theme
                    if (myHeaderContainer != null
                        && myHeaderContainer.IsHandleCreated)
                    {
                        NativeMethods.SendMessage(myHeaderContainer.HeaderControl.Handle, m.Msg, m.WParam, m.LParam);
                    }

                    break;
                    // UNDONE : following are provided temporarily for compatibility with existing Burton clients.
                    // should be removed once those clients switch to the corresponding public APIs.
                case NativeMethods.LB_SETSEL:
                    var select = m.WParam == IntPtr.Zero ? false : true;
                    if (m.LParam == (IntPtr)(-1))
                    {
                        if (select)
                        {
                            SelectAll();
                        }
                        else
                        {
                            ClearSelection();
                        }
                    }
                    else
                    {
                        SelectRange((int)m.LParam, (int)m.LParam, select);
                    }
                    m.Result = (IntPtr)1;
                    break;
                case NativeMethods.LB_SETANCHORINDEX:
                    AnchorIndex = (int)m.WParam;
                    m.Result = (IntPtr)1;
                    break;
                case NativeMethods.LB_GETANCHORINDEX:
                    m.Result = (IntPtr)AnchorIndex;
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        /// <summary>
        ///     Control.OnHandleCreate override
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            EnsureHeaderContainer();
            CalcTextHeight();
            CalcItemHeight(true);
            base.OnHandleCreated(e);
            if (ShowToolTips)
            {
                myTooltip = new ToolTipControl(this);
            }
            AttachHeaderContainer();
            AfterHandleCreated();
        }

        private void AfterHandleCreated()
        {
            ItemCount = (myTree == null) ? 0 : myTree.VisibleItemCount;
            ScrollBarsAfterSetWidth(VirtualTreeConstant.NullIndex);
        }

        /// <summary>
        ///     Determines if a window should be treated the same way as
        ///     the main control window when it comes to drawing item focus. The
        ///     default implementation returns true for the header control.
        /// </summary>
        /// <param name="testHandle">The window to test</param>
        /// <returns>True if items should draw focused when the test window has focus</returns>
        protected virtual bool IsDrawWithFocusWindow(IntPtr testHandle)
        {
            return myHeaderContainer != null && myHeaderContainer.IsHandleCreated && myHeaderContainer.HeaderControl.Handle == testHandle;
        }

        /// <summary>
        ///     Determines if the given handle represents an the in-place edit window or one of its
        ///     children.
        /// </summary>
        private bool IsInPlaceEditWindow(IntPtr testHandle)
        {
            if (GetStateFlag(VTCStateFlags.LabelEditActive))
            {
                var labelCtl = myInPlaceControl.InPlaceControl;
                var labelHandle = labelCtl.Handle;
                if (labelCtl.HasChildren)
                {
                    while (testHandle != IntPtr.Zero)
                    {
                        if (labelHandle == testHandle)
                        {
                            return true;
                        }
                        testHandle = NativeMethods.GetParent(testHandle);
                    }
                }
                else if (testHandle == labelHandle)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Control.OnHandleDestroyed override
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnHandleDestroyed(EventArgs e)
        {
            ItemCount = 0;
            if (myTooltip != null)
            {
                DestroyTooltipWindow();
                myMouseOverIndex = myRawMouseOverIndex = -1;
            }
            StringFormat = null;
            BoldFont = null;
            base.OnHandleDestroyed(e);
        }

        /// <summary>
        ///     Control.OnSystemColorsChanged override
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnSystemColorsChanged(EventArgs e)
        {
            IndentBitmap = null;
            MultiColumnHighlightFocusPen = null;
            base.OnSystemColorsChanged(e);
        }

        private void OnWmGetObject(ref Message message)
        {
            // Get the corresponding accessible object
            var accessibleObjectId = (int)message.LParam;
            var accessibleObject = VirtualTreeAccEvents.GetObject(accessibleObjectId);

            // If an AccessibleObject was retrieved, pass it back.
            if (accessibleObject != null)
            {
                IAccessible iacc = accessibleObject;

                // Get the Lresult
                var punkAcc = Marshal.GetIUnknownForObject(iacc);
                try
                {
                    var IID_IAccessible = new Guid(NativeMethods.uuid_IAccessible);
                    message.Result = NativeMethods.LresultFromObject(
                        ref IID_IAccessible, message.WParam, new HandleRef(accessibleObject, punkAcc));
                }
                finally
                {
                    Marshal.Release(punkAcc);
                }
            }
            else
            {
                base.WndProc(ref message);
            }
        }

        private void WmReflectDrawItem(ref Message m)
        {
            if (GetStateFlag(VTCStateFlags.WindowPositionChanging)
                || !Redraw
                || myUpdateCount != 0)
            {
                // We get this occasionally, but we'll get them
                // again after the window is done sizing, so there is
                // not much point in processing them now.
                return;
            }
            var dis = (NativeMethods.DRAWITEMSTRUCT)m.GetLParam(typeof(NativeMethods.DRAWITEMSTRUCT));
            var bounds = Rectangle.FromLTRB(dis.rcItem.left, dis.rcItem.top, dis.rcItem.right, dis.rcItem.bottom);
            if (!ClientRectangle.IntersectsWith(bounds))
            {
                return;
            }
            var dc = dis.hDC;
            var oldPal = NativeMethods.SelectPalette(dc, Graphics.GetHalftonePalette(), 1);
            try
            {
                using (var g = Graphics.FromHdc(dc))
                {
                    //UNDONE
                    //					if (HorizontalScrollbar) 
                    //					{
                    //						bounds.Width = Math.Max(MaxItemWidth, bounds.Width);
                    //					}

                    var state = (DrawItemState)dis.itemState;
                    if (dis.itemId == CaretIndex)
                    {
                        state |= DrawItemState.Focus;
                    }
                    else
                    {
                        // explicitly kill focus here, since the control will always tell us the first item in the list is focused.
                        state &= ~DrawItemState.Focus;
                    }

                    Color foreColor;
                    Color backColor;
                    // listbox may pass dis.itemId == -1, in the case of drawing focus in an empty list.
                    if (dis.itemId >= 0
                        && IsSelected(dis.itemId))
                    {
                        foreColor = SelectedItemActiveForeColor; // This row is selected, so set the forecolor and backcolor accordingly.
                        backColor = SelectedItemActiveBackColor;
                        state |= DrawItemState.Selected;
                    }
                    else
                    {
                        foreColor = ForeColor;
                        backColor = BackColor;
                    }

                    // Erase the item before calling OnDrawItem.  Necessary since all erasing is done here,
                    // rather than as part of the WM_ERASEBKGND message.
                    Brush backBrush = null;
                    EnsureBrush(ref backBrush, BackColor);
                    try
                    {
                        g.FillRectangle(backBrush, bounds);
                    }
                    finally
                    {
                        CleanBrush(ref backBrush, BackColor);
                    }
                    OnDrawItem(new DrawItemEventArgs(g, Font, bounds, dis.itemId, state, foreColor, backColor));
                }
            }
            finally
            {
                if (oldPal != IntPtr.Zero)
                {
                    NativeMethods.SelectPalette(dc, oldPal, 0);
                }
            }
            m.Result = (IntPtr)1;
        }

        /// <summary>
        ///     Control.OnKeyPress override
        /// </summary>
        /// <param name="e">KeyPressEventArgs</param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (myPuntChars > 0)
            {
                --myPuntChars;
                e.Handled = true;
            }
                // Keep beeps to a minimum by keeping Control out
            else if ((ModifierKeys & Keys.Control) == 0)
            {
                if (myTree != null
                    && myTree.VisibleItemCount > 0)
                {
                    bool restart;
                    var searchString = SearchString.Increment(e.KeyChar, out restart);
                    if (searchString.Length != 0)
                    {
                        e.Handled = true;
                        string curString;
                        var foundItem = false;
                        var searchLen = searchString.Length;
                        var itemStart = CurrentIndex;
                        if (itemStart == VirtualTreeConstant.NullIndex)
                        {
                            itemStart = 0;
                        }
                        int selectionColumn;
                        int nativeSelectionColumn;
                        ResolveSelectionColumn(itemStart, out selectionColumn, out nativeSelectionColumn);
                        if (restart)
                        {
                            ++itemStart;
                            if (itemStart >= myTree.VisibleItemCount)
                            {
                                itemStart = 0;
                            }
                        }
                        var columnItems = myTree.EnumerateColumnItems(selectionColumn, myColumnPermutation, false, itemStart, itemStart - 1);
                        while (columnItems.MoveNext())
                        {
                            curString = columnItems.Branch.GetText(columnItems.RowInBranch, columnItems.ColumnInBranch);
                            if (curString != null
                                && curString.Length != 0)
                            {
                                if (curString.Length >= searchLen
                                    &&
                                    0 == String.Compare(curString, 0, searchString, 0, searchLen, true, CultureInfo.CurrentCulture))
                                    // || ((0 == String.Compare(curString, 0, altString, 0, 1, true)) && (index > caret && index < itemStart))) //UNDONE: From original code.
                                {
                                    CurrentIndex = columnItems.RowInTree;
                                    foundItem = true;
                                    break;
                                }
                            }
                        }
                        if (!foundItem)
                        {
                            if (SameChars(searchString))
                            {
                                // if they hit the same key twice in a row at the beginning of
                                // the search, and there was no item found, they likely meant to
                                // retstart the search
                                SearchString.Clear();
                                OnKeyPress(e);
                                return;
                            }
                            SearchString.SignalFailure();
                        }
                    }
                }
            }
            base.OnKeyPress(e);
        }

        // Helper function to see if all characters in the string are the same
        private static bool SameChars(string s)
        {
            var iLen = s.Length;
            if (iLen < 2)
            {
                return false;
            }
            var c = s[0];
            for (var i = 1; i < iLen; ++i)
            {
                if (c != s[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///     Control.OnKeyDown override
        /// </summary>
        /// <param name="e">KeyEventArgs</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        protected override void OnKeyDown(KeyEventArgs e)
        {
            var iNewCaret = VirtualTreeConstant.NullIndex;
            var puntChar = false;
            var navDirection = TreeNavigation.None;
            var sourceColumn = mySelectionColumn;
            var startItemCount = (myTree == null) ? 0 : myTree.VisibleItemCount;
            var preserveSelectionOnColumnShift = true;
            switch (e.KeyCode)
            {
                case Keys.Tab:
                    // Note that the IsInputKey function should block this from ever
                    // reaching here for a bad column, but IsInputKey is subject to external
                    // forces that can let the keystroke through, so we need to validate
                    // here. We also need to get an accurate row in the new column, which might
                    // be different than the current row because of blanks.
                    if (myMctree != null
                        && !e.Alt
                        && !e.Control)
                    {
                        navDirection = e.Shift ? TreeNavigation.LeftColumn : TreeNavigation.RightColumn;
                    }

                    // Always handle the character. We don't punt these because they don't get a WM_CHAR,
                    // which is what pulls it off the queue.
                    e.Handled = true;
                    //puntChar = true;
                    break;
                case Keys.Left:
                    {
                        e.Handled = true;
                        var iCaret = CurrentIndex;
                        if (e.Control)
                        {
                            if (HasHorizontalScrollBar)
                            {
                                // scroll left
                                SetLeft(myXPos - HORIZONTAL_SCROLL_AMOUNT);
                            }
                        }
                        else if (iCaret != VirtualTreeConstant.NullIndex)
                        {
                            int nativeSourceColumn;
                            ResolveSelectionColumn(iCaret, out sourceColumn, out nativeSourceColumn);
                            if (myTree.IsExpanded(iCaret, nativeSourceColumn))
                            {
                                myTree.ToggleExpansion(iCaret, nativeSourceColumn);
                            }
                            else
                            {
                                if (iCaret == 0)
                                {
                                    iCaret = CurrentIndexCheckAnchor;
                                }
                                if (iCaret == -1)
                                {
                                    if (startItemCount > 0)
                                    {
                                        iNewCaret = 0;
                                    }
                                }
                                else
                                {
                                    navDirection = TreeNavigation.Left;
                                }
                            }
                        }
                        else if (startItemCount > 0)
                        {
                            iNewCaret = 0;
                        }
                        break;
                    }
                case Keys.Multiply:
                    if (!SearchString.IsActive)
                    {
                        e.Handled = true;
                        var iCaret = CurrentIndex;
                        if (iCaret != VirtualTreeConstant.NullIndex)
                        {
                            int nativeSourceColumn;
                            ResolveSelectionColumn(iCaret, out sourceColumn, out nativeSourceColumn);
                            puntChar = true;
                            ExpandRecurse(iCaret, nativeSourceColumn);
                        }
                    }
                    break;
                case Keys.Back:
                    {
                        e.Handled = true;
                        var iCaret = CurrentIndex;
                        puntChar = true;
                        if (iCaret != VirtualTreeConstant.NullIndex)
                        {
                            int nativeSourceColumn;
                            ResolveSelectionColumn(iCaret, out sourceColumn, out nativeSourceColumn);
                            iNewCaret = myTree.GetParentIndex(iCaret, nativeSourceColumn);
                        }
                        break;
                    }
                case Keys.Space:
                    if (e.Alt)
                    {
                        break; // let Alt-Space through, so as not to conflict with the system menu.
                    }

                    // UNDONE: Handle ToggleState here in addition to MultiSelect.
                    // It might be worth while to add a flag that automatically
                    // adds CheckBoxes to each item and uses the checkboxes instead
                    // of the normal multiselect to indicate selection. The underlying
                    // branches would not be offered a ToggleState in this type of list.
                    if (GetStyleFlag(VTCStyleFlags.MultiSelect))
                    {
                        var searchStringActive = SearchString.IsActive;
                        if (GetStyleFlag(VTCStyleFlags.ExtendedMultiSelect))
                        {
                            // Toggle the current selection on a control-space
                            if (e.Control)
                            {
                                var hWnd = Handle;
                                var iCaret = CaretIndex;
                                if (iCaret != VirtualTreeConstant.NullIndex)
                                {
                                    e.Handled = true;

                                    // Determine what we need to toggle to and select it.  We also move the anchor, unless the shift key is down, to match listbox default behavior.
                                    SetSelected(iCaret, !IsSelected(iCaret));
                                    if (!e.Shift)
                                    {
                                        AnchorIndex = iCaret;
                                    }

                                    DoSelectionChanged();
                                    FireWinEventsForSelection(false, true, ModifySelectionAction.Toggle);
                                }
                            }
                            else if (myStateImageList != null
                                     && !searchStringActive)
                            {
                                // For an extended multiselect control, Ctrl-space is used to
                                // toggle the selection, which leaves the simple 'space' available
                                // to toggle the state image index.
                                if (ToggleStateAtCurrentIndex())
                                {
                                    e.Handled = true;
                                    puntChar = true;
                                        // we've handled this message, make sure to clear out the corresponding WM_CHAR if it's been transalated.
                                }
                            }
                            if (!e.Handled)
                            {
                                puntChar = true;
                                if (searchStringActive)
                                {
                                    e.Handled = true;
                                    OnKeyPress(new KeyPressEventArgs(' '));
                                }
                            }
                        }
                        else
                        {
                            // Simple MultiSelect case. Simulate WM_CHAR if a search
                            // is active, and make sure we don't beep if it isn't.
                            puntChar = true;
                            if (searchStringActive)
                            {
                                e.Handled = true;
                                OnKeyPress(new KeyPressEventArgs(' '));
                            }
                            else if (e.Control
                                     && myStateImageList != null)
                            {
                                // For simple multiselect control, space will toggle the
                                // selection (and is the primary means of doing so). This
                                // makes Ctrl-Space the natural keystroke for state toggle.
                                e.Handled = ToggleStateAtCurrentIndex();
                            }

                            if (!e.Handled)
                            {
                                // Space toggles selection in the multi-simple case.
                                var iCaret = CaretIndex;
                                if (iCaret != VirtualTreeConstant.NullIndex)
                                {
                                    e.Handled = true;

                                    // Determine what we need to toggle to and select it.  We also move the anchor here, to match listbox default behavior.
                                    SetSelected(iCaret, !IsSelected(iCaret));
                                    AnchorIndex = iCaret;

                                    DoSelectionChanged();
                                    FireWinEventsForSelection(false, true, ModifySelectionAction.Toggle);
                                }
                            }
                        }
                    }
                    else if (myStateImageList != null)
                    {
                        if (SearchString.IsActive)
                        {
                            puntChar = true;
                            e.Handled = true;
                            OnKeyPress(new KeyPressEventArgs(' '));
                        }
                        else if (ToggleStateAtCurrentIndex())
                        {
                            puntChar = true;
                            e.Handled = true;
                        }
                    }
                    break;

                case Keys.Right:
                    {
                        e.Handled = true;
                        var iCaret = CurrentIndex;
                        if (e.Control)
                        {
                            if (HasHorizontalScrollBar)
                            {
                                // scroll right
                                SetLeft(myXPos + HORIZONTAL_SCROLL_AMOUNT);
                            }
                        }
                        else if (iCaret != VirtualTreeConstant.NullIndex)
                        {
                            int nativeSourceColumn;
                            ResolveSelectionColumn(iCaret, out sourceColumn, out nativeSourceColumn);
                            if (!myTree.IsExpanded(iCaret, nativeSourceColumn)
                                && myTree.IsExpandable(iCaret, nativeSourceColumn))
                            {
                                myTree.ToggleExpansion(iCaret, nativeSourceColumn);
                            }
                            else
                            {
                                if (iCaret == 0)
                                {
                                    iCaret = CurrentIndexCheckAnchor;
                                }
                                if (iCaret == -1)
                                {
                                    if (startItemCount > 0)
                                    {
                                        iNewCaret = 0;
                                    }
                                }
                                else
                                {
                                    navDirection = TreeNavigation.Right;
                                }
                            }
                        }
                        else if (startItemCount > 0)
                        {
                            iNewCaret = 0;
                        }
                        break;
                    }
                case Keys.Add:
                case Keys.Subtract:
                    if (e.KeyCode == Keys.Add
                        && e.Modifiers == Keys.Control)
                    {
                        // handle Ctrl-Plus to auto-size the current column.  
                        // UNDONE: the standard listview handles this to auto-size all columns, but
                        // since we may have percentage-based columns, this is not necessarily a straightforward
                        // operation.  Most important thing here is the accessbility requirement, which is 
                        // satisfied by allowing resize of the current column from the keyboard.  
                        if (mySelectionColumn >= 0)
                        {
                            e.Handled = true;
                            var resizeColumn = mySelectionColumn;
                            if (myColumnPermutation != null)
                            {
                                resizeColumn = myColumnPermutation.GetNativeColumn(resizeColumn);
                            }

                            AutoSizeColumn(resizeColumn);
                        }
                    }
                    else if (!SearchString.IsActive)
                    {
                        e.Handled = true;
                        var testState = e.KeyCode == Keys.Subtract;
                        var iCaret = CurrentIndex;
                        if (iCaret != VirtualTreeConstant.NullIndex)
                        {
                            puntChar = true;
                            int nativeSourceColumn;
                            ResolveSelectionColumn(iCaret, out sourceColumn, out nativeSourceColumn);
                            if ((testState == myTree.IsExpanded(iCaret, nativeSourceColumn))
                                && myTree.IsExpandable(iCaret, nativeSourceColumn))
                            {
                                myTree.ToggleExpansion(iCaret, nativeSourceColumn);
                            }
                        }
                    }
                    break;
                case Keys.Up:
                case Keys.Down:
                    e.Handled = true;
                    preserveSelectionOnColumnShift = false;
                    if (e.Control)
                    {
                        // Either scroll or move the caret, depending on multiselect state.
                        // An extended multi-select listbox won't let you move up/down without
                        // killing the current selection. The desired behavior is to allow
                        // the caret to move, but not change any selection on the move.
                        if (GetStyleFlag(VTCStyleFlags.ExtendedMultiSelect))
                        {
                            var iCaret = CurrentIndexCheckAnchor;
                            if (iCaret == VirtualTreeConstant.NullIndex)
                            {
                                if (startItemCount > 0)
                                {
                                    iNewCaret = 0;
                                }
                            }
                            else
                            {
                                navDirection = (e.KeyCode == Keys.Up) ? TreeNavigation.Up : TreeNavigation.Down;
                            }
                        }
                        else
                        {
                            // Scroll up/down one row
                            var newTopIndex = TopIndex + ((e.KeyCode == Keys.Up) ? -1 : 1);
                            if (newTopIndex >= 0
                                && newTopIndex < startItemCount)
                            {
                                TopIndex = newTopIndex;
                            }
                        }
                    }
                    else
                    {
                        var iCaret = CurrentIndexCheckAnchor;
                        if (iCaret == VirtualTreeConstant.NullIndex)
                        {
                            if (startItemCount > 0)
                            {
                                iNewCaret = 0;
                            }
                        }
                        else
                        {
                            navDirection = (e.KeyCode == Keys.Up) ? TreeNavigation.Up : TreeNavigation.Down;
                        }
                    }
                    break;
                case Keys.PageDown:
                case Keys.End:
                    if (startItemCount > 0
                        && myMctree != null)
                    {
                        // A page down key can jump to a blank item. Make sure that it doesn't.
                        // Page down acts by moving the current item to the first row (if possible),
                        // and the selection to the last visible row. If we hit a blank and its
                        // blank expansion anchor is the parent, then we have no choice but to
                        // go further than this and move the next non-blank item to the first row,
                        // unless this item is past the end of the tree, in which case we 
                        // scroll the current item to the top and stay where we are.

                        // Page up/page down/home/end would be nice to do with GetNavigationTarget, but
                        // they require extra data (page size) and are specialized to this
                        // view on the data (there is no page down in accessibility), so we do
                        // them inline here.
                        var caretChangeCount = e.KeyCode == Keys.PageDown ? myFullyVisibleCount : startItemCount;
                        var iCaret = CurrentIndexCheckAnchor;
                        if (caretChangeCount > 1)
                        {
                            iNewCaret = iCaret + caretChangeCount - 1;
                        }
                        else
                        {
                            // Try to move at least one
                            caretChangeCount = 1;
                            iNewCaret = iCaret + 1;
                        }
                        if (iNewCaret >= startItemCount)
                        {
                            iNewCaret = startItemCount - 1;
                        }
                        if (iNewCaret == iCaret)
                        {
                            if (iCaret == -1)
                            {
                                iNewCaret = 0;
                            }
                            // Nowhere to go, stop trying to get there
                            e.Handled = true;
                            break;
                        }

                        var findNextItemWithDown = false;
                        int nativeSourceColumn;
                        ResolveSelectionColumn(iCaret, out sourceColumn, out nativeSourceColumn);
                        var expansion = myTree.GetBlankExpansion(iNewCaret, sourceColumn, myColumnPermutation);
                        if (expansion.AnchorColumn != sourceColumn)
                        {
                            var targetCoordinate = myTree.GetNavigationTarget(
                                TreeNavigation.Up, iNewCaret, sourceColumn, myColumnPermutation);
                            if (targetCoordinate.IsValid)
                            {
                                iNewCaret = targetCoordinate.Row;
                                Debug.Assert(targetCoordinate.Column == sourceColumn);
                                if (iNewCaret == iCaret)
                                {
                                    findNextItemWithDown = true;
                                }
                                else
                                {
                                    e.Handled = true;
                                    TopIndex = iCaret;
                                }
                            }
                            else
                            {
                                // Rare case but a nice backup, indicates that we were sitting on a blank
                                findNextItemWithDown = true;
                            }
                        }
                        else if (expansion.TopRow == iCaret)
                        {
                            findNextItemWithDown = true;
                        }
                        else if (expansion.BottomRow > iNewCaret)
                        {
                            // Move the current item to the top and the
                            // selection to the new caret. A second page down
                            // will go further.
                            e.Handled = true;
                            iNewCaret = expansion.TopRow;
                            TopIndex = iCaret;
                        }
                        else if (expansion.TopRow < iNewCaret)
                        {
                            e.Handled = true;
                            TopIndex = iNewCaret - caretChangeCount - 1;
                            iNewCaret = expansion.TopRow;
                        }

                        if (findNextItemWithDown)
                        {
                            // A page down from the current item is a blank belonging to the current item,
                            // we'll need to scroll further than usual to get to the next item.
                            e.Handled = true;
                            var targetCoordinate = myTree.GetNavigationTarget(
                                TreeNavigation.Down, iCaret, sourceColumn, myColumnPermutation);
                            if (!targetCoordinate.IsValid)
                            {
                                TopIndex = iCaret;
                                iNewCaret = VirtualTreeConstant.NullIndex;
                                break;
                            }
                            Debug.Assert(sourceColumn == targetCoordinate.Column); // Down needs to stay in the same column
                            iNewCaret = targetCoordinate.Row;
                            TopIndex = iNewCaret - caretChangeCount - 1;
                        }
                    }
                    break;
                case Keys.PageUp:
                case Keys.Home:
                    if (startItemCount > 0
                        && myMctree != null)
                    {
                        // Same sentiments as PageDown, just reverse the algorithm.
                        var caretChangeCount = e.KeyCode == Keys.PageUp ? myFullyVisibleCount : startItemCount;
                        var iCaret = CurrentIndexCheckAnchor;
                        if (caretChangeCount > 1)
                        {
                            iNewCaret = iCaret - caretChangeCount + 1;
                        }
                        else
                        {
                            // Try to move at least one
                            caretChangeCount = 1;
                            iNewCaret = iCaret - 1;
                        }
                        if (iNewCaret < 0)
                        {
                            iNewCaret = 0;
                        }
                        if (iNewCaret == iCaret)
                        {
                            if (iCaret == -1)
                            {
                                iNewCaret = 0;
                            }
                            // Nowhere to go, stop trying to get there
                            e.Handled = true;
                            break;
                        }

                        var findNextItemWithUp = false;
                        int nativeSourceColumn;
                        ResolveSelectionColumn(iCaret, out sourceColumn, out nativeSourceColumn);
                        preserveSelectionOnColumnShift = false;
                        var expansion = myTree.GetBlankExpansion(iNewCaret, sourceColumn, myColumnPermutation);
                        if (expansion.AnchorColumn != sourceColumn
                            || expansion.TopRow < iNewCaret)
                        {
                            var targetCoordinate = myTree.GetNavigationTarget(
                                TreeNavigation.Down, iNewCaret, sourceColumn, myColumnPermutation);
                            if (targetCoordinate.IsValid)
                            {
                                iNewCaret = targetCoordinate.Row;
                                Debug.Assert(targetCoordinate.Column == sourceColumn);
                                if (iNewCaret == iCaret)
                                {
                                    findNextItemWithUp = true;
                                }
                                else
                                {
                                    e.Handled = true;
                                    TopIndex = Math.Max(0, iCaret - caretChangeCount + 1);
                                }
                            }
                            else
                            {
                                // Rare case but a nice backup, indicates that we were sitting on a blank
                                findNextItemWithUp = true;
                            }
                        }

                        if (findNextItemWithUp)
                        {
                            // A page up from the current item is a blank belonging to the current item,
                            // we'll need to scroll further than usual to get to the next item.
                            e.Handled = true;
                            var targetCoordinate = myTree.GetNavigationTarget(
                                TreeNavigation.Up, expansion.TopRow, sourceColumn, myColumnPermutation);
                            if (!targetCoordinate.IsValid)
                            {
                                TopIndex = Math.Max(iCaret - caretChangeCount + 1, 0);
                                iNewCaret = VirtualTreeConstant.NullIndex;
                                break;
                            }
                            Debug.Assert(sourceColumn == targetCoordinate.Column);
                            TopIndex = iNewCaret = targetCoordinate.Row;
                        }
                    }
                    break;
                case Keys.Enter:
                    {
                        var iCaret = CurrentIndex;
                        if (iCaret != VirtualTreeConstant.NullIndex)
                        {
                            int nativeSourceColumn;
                            ResolveSelectionColumn(iCaret, out sourceColumn, out nativeSourceColumn);
                            if (nativeSourceColumn != VirtualTreeConstant.NullIndex
                                && myTree != null
                                && myTree.IsExpandable(iCaret, nativeSourceColumn))
                            {
                                e.Handled = true;
                                myTree.ToggleExpansion(iCaret, nativeSourceColumn);
                            }
                        }
                        break;
                    }
            }
            sourceColumn = mySelectionColumn;
            var targetColumn = sourceColumn;
            if (navDirection != TreeNavigation.None)
            {
                var iCaret = CurrentIndex;
                if (iCaret == VirtualTreeConstant.NullIndex)
                {
                    iNewCaret = VirtualTreeConstant.NullIndex;
                }
                else
                {
                    int nativeSourceColumn;
                    ResolveSelectionColumn(iCaret, out targetColumn, out nativeSourceColumn);
                    var targetCoordinate = myTree.GetNavigationTarget(navDirection, iCaret, targetColumn, myColumnPermutation);
                    if (targetCoordinate.IsValid)
                    {
                        iNewCaret = targetCoordinate.Row;
                        targetColumn = targetCoordinate.Column;
                        if (iNewCaret == iCaret
                            && targetColumn == mySelectionColumn)
                        {
                            iNewCaret = VirtualTreeConstant.NullIndex;
                        }
                    }
                    else
                    {
                        iNewCaret = VirtualTreeConstant.NullIndex;
                    }
                }
            }
            else if (GetStyleFlag(VTCStyleFlags.MultiSelect) && ExtendSelectionToAnchors) // Condition for resolving
            {
                var iCaret = CurrentIndex;
                if (iCaret != VirtualTreeConstant.NullIndex)
                {
                    int nativeSourceColumn;
                    ResolveSelectionColumn(iCaret, out targetColumn, out nativeSourceColumn);
                }
            }
            if (iNewCaret != VirtualTreeConstant.NullIndex)
            {
                Debug.Assert(iNewCaret < myTree.VisibleItemCount);
                var preserveSelection = e.Control;
                var forceSelectCaret = ModifySelectionAction.None;
                if (targetColumn != sourceColumn)
                {
                    SetSelectionColumn(targetColumn, false);
                    if (preserveSelectionOnColumnShift)
                    {
                        preserveSelection = true;
                        forceSelectCaret = ModifySelectionAction.Select;
                    }
                }
                if (GetStyleFlag(VTCStyleFlags.ExtendedMultiSelect))
                {
                    SetCurrentExtendedMultiSelectIndex(iNewCaret, e.Shift, preserveSelection, forceSelectCaret);
                }
                else
                {
                    CurrentIndex = iNewCaret;
                }
            }
            else if (targetColumn != sourceColumn)
            {
                SetSelectionColumn(targetColumn, true);
            }
            if (puntChar)
            {
                ++myPuntChars;
            }
            else if (myPuntChars > 0)
            {
                // Make sure all of the WM_CHARS are cleared out if we're not punting this
                // character so that we ignore the correct characters.
                NativeMethods.MSG msg;
                var hWnd = Handle;
                while ((myPuntChars > 0)
                       &&
                       NativeMethods.PeekMessage(
                           out msg, hWnd, NativeMethods.WM_CHAR, NativeMethods.WM_CHAR, NativeMethods.PeekMessageAction.Remove))
                {
                    --myPuntChars;
                }
                myPuntChars = 0; // Back up just in case something got off
            }
            base.OnKeyDown(e);
        }

        /// <summary>
        ///     Retrieves the index of the item at the given coordinates
        /// </summary>
        /// <param name="x">The x position, in client coordinates</param>
        /// <param name="y">The y position, in client coordinates</param>
        /// <returns>An index, or VirtualTreeConstant.NullIndex if there is no item at this location</returns>
        public int IndexFromPoint(int x, int y)
        {
            //NT4 SP6A : SendMessage Fails. So First check whether the point is in Client Co-ordinates and then
            //call Sendmessage.
            //
            NativeMethods.RECT r;
            var hr = NativeMethods.GetClientRect(Handle, out r);
            if (NativeMethods.Succeeded(hr)
                && r.left <= x
                && x < r.right
                && r.top <= y
                && y < r.bottom)
            {
                var index = (int)NativeMethods.SendMessage(Handle, NativeMethods.LB_ITEMFROMPOINT, 0, (int)NativeMethods.MAKELPARAM(x, y));
                if (NativeMethods.UnsignedHIWORD(index) == 0)
                {
                    // Inside ListBox client area			   
                    return NativeMethods.UnsignedLOWORD(index);
                }
            }

            return VirtualTreeConstant.NullIndex;
        }

        /// <summary>
        ///     Retrieves the index of the item at the given coordinates
        /// </summary>
        /// <param name="clientPoint">A point, in client coordinates</param>
        /// <returns>An index, or VirtualTreeConstant.NullIndex if there is no item at this location</returns>
        public int IndexFromPoint(Point clientPoint)
        {
            return IndexFromPoint(clientPoint.X, clientPoint.Y);
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.GetClientRect(System.IntPtr,Microsoft.Data.Entity.Design.VisualStudio.NativeMethods+RECT@)")]
        private bool SizeWnd(int cxWnd, int cyWnd)
        {
            if (cxWnd == 0
                || cyWnd == 0)
            {
                NativeMethods.RECT rc;
                NativeMethods.GetClientRect(Handle, out rc);
                cxWnd = rc.right;
                cyWnd = rc.bottom;
            }
            myFullyVisibleCount = cyWnd / myItemHeight;
            myPartlyVisibleCountIgnoreHScroll = 
                (cyWnd + 
                 (HasHorizontalScrollBar ? SystemInformation.HorizontalScrollBarHeight : 0) 
                 + myItemHeight - 1) / myItemHeight;
            CalcScrollBars();
            return true;
        }

        /// <summary>
        ///     Begin an extensive update of the control. Turns redraw off on the control. Must
        ///     be balanced by one of the EndUpdate overrides. BeginUpdate/EndUpdate requests are
        ///     counted and can be safely nested.
        /// </summary>
        public void BeginUpdate()
        {
            if (!IsHandleCreated)
            {
                return;
            }
            if (myUpdateCount == 0)
            {
                NativeMethods.SendMessage(Handle, NativeMethods.WM_SETREDRAW, 0, 0);
            }
            ++myUpdateCount;
        }

        /// <summary>
        ///     Finish an extensive update to the control and invalidate the control to
        ///     force it to redraw.
        /// </summary>
        /// <returns>True if this balances the initial BeginUpdate request.</returns>
        public bool EndUpdate()
        {
            return EndUpdate(true);
        }

        /// <summary>
        ///     Finish an extensive update to the control and optionally invalidate the control to
        ///     force it to redraw.
        /// </summary>
        /// <param name="invalidate">true to invalidate the control</param>
        /// <returns>True if this balances the initial BeginUpdate request.</returns>
        public bool EndUpdate(bool invalidate)
        {
            if (myUpdateCount > 0)
            {
                Debug.Assert(IsHandleCreated, "Handle should be created by now");
                --myUpdateCount;
                if (myUpdateCount == 0)
                {
                    NativeMethods.SendMessage(Handle, NativeMethods.WM_SETREDRAW, -1, 0);
                    if (invalidate)
                    {
                        Invalidate();
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void LimitRectToColumn(int column, ref Rectangle itemRect)
        {
            LimitRectToColumn(column, ref itemRect, false, -1, false);
        }

        private void LimitRectToColumn(int column, ref Rectangle itemRect, bool forNonClient)
        {
            LimitRectToColumn(column, ref itemRect, forNonClient, -1, false);
        }

        private void LimitRectToColumn(int column, ref Rectangle itemRect, bool forNonClient, int extendThroughColumn, bool forDragRegion)
        {
            // make multi-column adjustments
            if (extendThroughColumn < column)
            {
                extendThroughColumn = column;
            }
            var columns = ColumnCount;
            var isLastColumn = extendThroughColumn == (columns - 1);
            var adjustLeftWidthForScrollbar = false;
            var adjustRightWidthForScrollbar = false;
            var adjustLeftForScrollbar = false;
            if (forNonClient || !HasVerticalScrollBar)
            {
                if (LeftScrollBar)
                {
                    if (column == 0
                        && !forDragRegion)
                    {
                        adjustLeftWidthForScrollbar = true;
                    }
                    else
                    {
                        adjustLeftForScrollbar = true;
                    }
                }
                else if (isLastColumn)
                {
                    adjustRightWidthForScrollbar = true;
                }
            }
            var fullPercentWidth = FullPercentageHeaderWidth;
            if (!myHeaderBounds.HasHeaders)
            {
                var incr = fullPercentWidth / columns;
                itemRect.X += (incr * column);
                if (isLastColumn)
                {
                    itemRect.Width = fullPercentWidth - itemRect.X;
                }
                else if (extendThroughColumn == column)
                {
                    itemRect.Width = incr;
                }
                else
                {
                    itemRect.Width = incr * (extendThroughColumn - column + 1);
                }
            }
            else
            {
                int itemLeft;
                int itemWidth;
                myHeaderBounds.GetColumnBounds(fullPercentWidth, column, extendThroughColumn, out itemLeft, out itemWidth);
                adjustRightWidthForScrollbar = adjustRightWidthForScrollbar && myHeaderBounds.HasVariableBounds;
                itemRect.X = itemLeft;
                itemRect.Width = itemWidth;
            }
            if (adjustLeftForScrollbar)
            {
                itemRect.X += SystemInformation.VerticalScrollBarWidth;
            }
            else if (adjustLeftWidthForScrollbar || adjustRightWidthForScrollbar)
            {
                itemRect.Width += SystemInformation.VerticalScrollBarWidth;
            }
            if (forNonClient)
            {
                itemRect.X += BorderOffset;
            }
        }

        /// <summary>
        ///     Retrieves a Rectangle object which describes the bounding rectangle
        ///     around an item in the list, in client coordinates.  If the item in question is not visible,
        ///     the rectangle will be outside the visible portion of the control.
        /// </summary>
        /// <param name="row">The row coordinate</param>
        /// <param name="column">The column coordinate. Should be a display column.</param>
        /// <returns>The bounding rectangle for the item.</returns>
        public Rectangle GetItemRectangle(int row, int column)
        {
            CheckIndex(row);
            NativeMethods.RECT rect;
            NativeMethods.SendMessage(Handle, NativeMethods.LB_GETITEMRECT, row, out rect);
            var itemRect = Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom);

            if (myMctree != null)
            {
                LimitRectToColumn(column, ref itemRect);
                // account for horizontal scroll position
                itemRect.Offset(-myXPos, 0);
            }

            return itemRect;
        }

        private Rectangle GetItemRectangle(int row, int column, bool excludeIndent, bool textRectOnly, string alternateText)
        {
            var FullRect = GetItemRectangle(row, column);
            if (excludeIndent)
            {
                var checkRootLines = GetAnyStyleFlag(VTCStyleFlags.HasRootLines | VTCStyleFlags.HasRootButtons);
                if (myColumnPermutation != null)
                {
                    column = myColumnPermutation.GetNativeColumn(column);
                }
                var info = myTree.GetItemInfo(row, column, checkRootLines && column > 0);

                // Calculate where X position should start...
                var level = info.Level;
                if (checkRootLines && (column == 0 || !info.SimpleCell))
                {
                    ++level;
                }
                var xOffset = (level * myIndentWidth);

                int imageWidth;
                var stringWidth = ListItemStringWidth(ref info, alternateText, out imageWidth);
                xOffset += imageWidth;
                FullRect.X += xOffset;
                if (textRectOnly && ((stringWidth + xOffset) < FullRect.Width))
                {
                    FullRect.Width = stringWidth;
                }
                else
                {
                    FullRect.Width -= xOffset;
                }
            }
            return FullRect;
        }

        // ----------------------------------------------------------------------------
        //
        //  If the given item is visible in the client area, the rectangle that
        //  surrounds that item is invalidated
        //
        // ----------------------------------------------------------------------------
        private void InvalidateItem(int absIndex, int column, NativeMethods.RedrawWindowFlags flags)
        {
            if (Redraw)
            {
                NativeMethods.RECT rect;
                if (column == -1)
                {
                    CheckIndex(absIndex);
                    NativeMethods.SendMessage(Handle, NativeMethods.LB_GETITEMRECT, absIndex, out rect);
                }
                else
                {
                    rect = new NativeMethods.RECT(GetItemRectangle(absIndex, column));
                }
                NativeMethods.RedrawWindow(Handle, ref rect, IntPtr.Zero, flags);
            }
        }

        private void InvalidateItem(int absIndex, int column, NativeMethods.RedrawWindowFlags flags, int count)
        {
            if (Redraw)
            {
                NativeMethods.RECT rect;
                if (column == -1)
                {
                    CheckIndex(absIndex);
                    NativeMethods.SendMessage(Handle, NativeMethods.LB_GETITEMRECT, absIndex, out rect);
                }
                else
                {
                    rect = new NativeMethods.RECT(GetItemRectangle(absIndex, column));
                }
                if (count > 1)
                {
                    rect.bottom += rect.height * (count - 1);
                }
                NativeMethods.RedrawWindow(Handle, ref rect, IntPtr.Zero, flags);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.InvalidateRect(System.IntPtr,Microsoft.Data.Entity.Design.VisualStudio.NativeMethods+RECT@,System.Boolean)")]
        private void InvalidateStateImage(int absIndex, int displayColumn, int stateImageLeft)
        {
            if (Redraw)
            {
                if (stateImageLeft == -1)
                {
                    stateImageLeft = GetItemRectangle(absIndex, displayColumn).Left;
                    var itemInfo = myTree.GetItemInfo(absIndex, displayColumn, true);
                    var level = itemInfo.Level;
                    if (GetAnyStyleFlag(VTCStyleFlags.HasRootLines | VTCStyleFlags.HasRootButtons)
                        && (displayColumn == 0 || !itemInfo.SimpleCell))
                    {
                        level++;
                    }
                    stateImageLeft += (level * myIndentWidth) - myXPos;
                }

                if (myColumnPermutation != null)
                {
                    myColumnPermutation.GetNativeColumn(displayColumn);
                }

                var stateImageTop = (absIndex - TopIndex) * myItemHeight;
                if (stateImageTop >= 0
                    && stateImageTop < ClientSize.Height)
                {
                    var rect = new NativeMethods.RECT(
                        stateImageLeft, stateImageTop, stateImageLeft + myStateImageWidth, stateImageTop + myStateImageWidth);
                    NativeMethods.InvalidateRect(Handle, ref rect, false);
                }
            }
        }

        #endregion //Window Interaction Functions

        #region Component Designer generated code

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.EditTimer = new System.Windows.Forms.Timer(this.components);
            this.myDragTimer = new System.Windows.Forms.Timer(this.components);
            // 
            // EditTimer
            // 
            this.EditTimer.Tick += new System.EventHandler(this.EditTimer_Tick);
            //
            // Drag Over Expand/BumpScroll Timer
            //
            myDragTimer.Tick += new EventHandler(OnDragTimerTick);
        }

        #endregion

        #region Event Definitions and Wiring

        private static readonly object EVENT_DRAWITEM = new object();
        private static readonly object EVENT_CONTEXTMENU = new object();
        private static readonly object EVENT_SELCHANGED = new object();
        private static readonly object EVENT_DOUBLECLICK = new object();
        private static readonly object EVENT_LABELEDITCONTROLCHANGED = new object();

        /// <summary>
        ///     Receive an event after an item is drawn
        /// </summary>
        public event DrawItemEventHandler DrawItem
        {
            add { Events.AddHandler(EVENT_DRAWITEM, value); }
            remove { Events.RemoveHandler(EVENT_DRAWITEM, value); }
        }

        /// <summary>
        ///     Receive an event when the selection changes
        /// </summary>
        public event EventHandler SelectionChanged
        {
            add { Events.AddHandler(EVENT_SELCHANGED, value); }
            remove { Events.RemoveHandler(EVENT_SELCHANGED, value); }
        }

        /// <summary>
        ///     Receive an event when a context menu is requested.
        /// </summary>
        public event ContextMenuEventHandler ContextMenuInvoked
        {
            add { Events.AddHandler(EVENT_CONTEXTMENU, value); }
            remove { Events.RemoveHandler(EVENT_CONTEXTMENU, value); }
        }

        /// <summary>
        ///     Receive an event when an item is double clicked.
        /// </summary>
        public new event DoubleClickEventHandler DoubleClick
        {
            add { Events.AddHandler(EVENT_DOUBLECLICK, value); }
            remove { Events.RemoveHandler(EVENT_DOUBLECLICK, value); }
        }

        /// <summary>
        ///     Receive an event after an inplace edit control is shown or hidden.
        /// </summary>
        public event EventHandler LabelEditControlChanged
        {
            add { Events.AddHandler(EVENT_LABELEDITCONTROLCHANGED, value); }
            remove { Events.RemoveHandler(EVENT_LABELEDITCONTROLCHANGED, value); }
        }

        /// <summary>
        ///     An item is being drawn
        /// </summary>
        /// <param name="e">DrawItemEventArgs</param>
        protected virtual void OnDrawItem(DrawItemEventArgs e)
        {
            DoDrawItem(e);
            var handler = Events[EVENT_DRAWITEM] as DrawItemEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///     A context menu has been requested
        /// </summary>
        /// <param name="e">ContextMenuEventArgs</param>
        protected virtual void OnContextMenuInvoked(ContextMenuEventArgs e)
        {
            var handler = Events[EVENT_CONTEXTMENU] as ContextMenuEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///     Called when the label edit state changes. Called after an inplace
        ///     edit control is invoked or dismissed.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnLabelEditControlChanged(EventArgs e)
        {
            var handler = Events[EVENT_LABELEDITCONTROLCHANGED] as EventHandler;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private new void OnDoubleClick(EventArgs e)
        {
            // Hide this version of OnDoubleClick
        }
        
        /// <summary>
        ///     Called when an item is double-clicked
        /// </summary>
        /// <param name="e">VirtualTreeControl specific DoubleClickEventArgs</param>
        protected virtual void OnDoubleClick(DoubleClickEventArgs e)
        {
            var handler = Events[EVENT_DOUBLECLICK] as DoubleClickEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
            if (!e.Handled
                && myTree != null
                && e.Button == MouseButtons.Left)
            {
                var hitInfo = e.HitInfo;
                if (0 != (hitInfo.HitTarget & VirtualTreeHitTargets.OnItem))
                {
                    var itemInfo = e.ItemInfo;
                    if (0 != (hitInfo.HitTarget & VirtualTreeHitTargets.OnItemStateIcon))
                    {
                        // Treat a double click as two single clicks if applied directly to
                        // the state icon.
                        ToggleAndSynchronizeState(hitInfo.Row, hitInfo.NativeColumn);
                    }
                    else if (itemInfo.Expandable)
                    {
                        myTree.ToggleExpansion(hitInfo.Row, hitInfo.NativeColumn);
                    }
                    else if (myStateImageList != null)
                    {
                        // Just toggle here, don't synchronize. This behavior is consisten
                        // with the standard ListView control. The synchronization is hard
                        // for the user and code to coordinate because the single click just
                        // toggled the selection state.
                        myTree.ToggleState(hitInfo.Row, hitInfo.NativeColumn);
                    }
                }
                else if (0 != (hitInfo.HitTarget & VirtualTreeHitTargets.OnItemButton))
                {
                    // avoid calling the DefWndProc if the click was on the button, because this
                    // causes scrolling to the selected index, which may not be the one double-clicked on.
                    SetStateFlag(VTCStateFlags.CallDefWndProc, false);
                }
            }
        }

        /// <summary>
        ///     Raises the selection changed event.
        /// </summary>
        private void DoSelectionChanged()
        {
            if (!GetStateFlag(VTCStateFlags.RestoringSelection))
            {
                OnSelectionChanged(EventArgs.Empty);
            }

            // It is somewhat arbitrary if we run this before or after the selection changed
            // event is fired. However, running this before means that the Focused property
            // of the tree control may be false. This property is often checked in a selection
            // changed event to see if the selected item should be the active selection context
            // for the application.
            if (Focused)
            {
                TestStartSelectionLabelEdit();
            }
        }

        /// <summary>
        ///     Called when the selection changes. Enables the SelectionChanged even,
        ///     immediate inplace control activation, and accessibility selection events.
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected virtual void OnSelectionChanged(EventArgs e)
        {
            var handler = Events[EVENT_SELCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///     Control.OnGotFocus override. Enables immediate activation mode for label editing.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGotFocus(EventArgs e)
        {
            TestStartSelectionLabelEdit();
            base.OnGotFocus(e);
        }

        private void TestStartSelectionLabelEdit()
        {
            if (!GetStateFlag(VTCStateFlags.SelChangeFromMouse)
                && !GetStateFlag(VTCStateFlags.NoDismissEdit)
                && GetStyleFlag(VTCStyleFlags.ImmediateSelectionLabelEdits))
            {
                DismissLabelEdit(false, false);
                // Attempt to begin a label edit in response to the selection change
                myEditIndex = CurrentIndex;
                if (myEditIndex != -1)
                {
                    var immediateActivation = true;
                    StartLabelEdit(false, 0, ref immediateActivation);
                }
            }
        }

        /// <summary>
        ///     Control.IsInputKey override. Enables Tab and Shift-Tab keystrokes
        ///     to move between columns instead of moving to the next control.
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool IsInputKey(Keys keyData)
        {
            if (myMctree != null
                && (keyData & (Keys.Alt | Keys.Control)) == 0)
            {
                switch (keyData & Keys.KeyCode)
                {
                    case Keys.Tab:
                        var maxColumn = ((myColumnPermutation != null) ? myColumnPermutation.VisibleColumnCount : myMctree.ColumnCount) - 1;
                        if ((keyData & Keys.Shift) == 0)
                        {
                            if (mySelectionColumn < maxColumn)
                            {
                                // Tab will move forward one.
                                var currentRow = CurrentIndex;
                                if (currentRow != VirtualTreeConstant.NullIndex)
                                {
                                    if (myTree.GetBlankExpansion(currentRow, mySelectionColumn, myColumnPermutation).RightColumn < maxColumn)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                        else if (mySelectionColumn > 0)
                        {
                            var currentRow = CurrentIndex;
                            if (currentRow != VirtualTreeConstant.NullIndex)
                            {
                                if (myColumnPermutation == null
                                    || myTree.GetBlankExpansion(currentRow, mySelectionColumn, myColumnPermutation).LeftColumn > 0)
                                {
                                    // Shift tab will move the column back one
                                    return true;
                                }
                            }
                        }
                        break;
                }
            }
            return base.IsInputKey(keyData);
        }

        #endregion

        #region VirtualTree Events and related functions

        // Small helper structure to share code to restore the tree state
        // for both a simple and a recursive expansion.
        private class ListBoxStateTrackerClass
        {
            public ListBoxStateTracker Inner;

            public ListBoxStateTrackerClass(VirtualTreeControl ctl)
            {
                Inner = new ListBoxStateTracker(ctl);
            }
        }

        private struct ListBoxStateTracker
        {
            private const int TRACKINDEX_RestoreTop = 0;
            private const int TRACKINDEX_Caret = 1;
            private const int TRACKINDEX_Anchor = 2;
            private const int TRACKINDEX_FirstSelection = 3;
            public int startTop;
            public int restoreTop; // Tracked index
            private int restoreCaret; // Tracked index
            private bool caretMoved; // Block selection events unless the caret was moved
            private int restoreAnchorIndex; // Tracked index
            public readonly int restoreXPos;
            private readonly int restoreHExtent;
            private int[] restoreSelection; // Tracked indices
            private readonly int startingListCount;
            private int restoreColumn;
            private readonly int startColumn;

            internal ListBoxStateTracker(VirtualTreeControl ctl)
            {
                startTop = ctl.TopIndex;
                restoreSelection = null;
                restoreAnchorIndex = -1;
                restoreColumn = startColumn = ctl.CurrentColumn;
                var hWnd = ctl.Handle;
                if (ctl.GetStyleFlag(VTCStyleFlags.MultiSelect)
                    && ctl.SelectedItemCount > 0)
                {
                    restoreSelection = ctl.SelectedIndicesArray;
                    restoreAnchorIndex = ctl.AnchorIndex;
                }
                restoreTop = startTop;
                // Don't use CurrentIndex here, it doesn't notice the caret/anchor subtle distinction,
                // so we end up making a selection where there didn't used to be one.
                restoreCaret = ctl.CurrentIndexCheckAnchor;
                caretMoved = false;
                restoreXPos = ctl.myXPos;
                startingListCount = NativeMethods.SendMessage(hWnd, NativeMethods.LB_GETCOUNT, 0, 0).ToInt32();
                restoreHExtent = 0;
                if (restoreXPos != 0)
                {
                    restoreHExtent = NativeMethods.SendMessage(hWnd, NativeMethods.LB_GETHORIZONTALEXTENT, 0, 0).ToInt32();
                    NativeMethods.SendMessage(
                        hWnd, NativeMethods.WM_HSCROLL, NativeMethods.MAKELONG((int)NativeMethods.ScrollAction.ThumbPosition, 0), 0);
                    NativeMethods.SendMessage(hWnd, NativeMethods.WM_HSCROLL, (int)NativeMethods.ScrollAction.EndScroll, 0);
                    NativeMethods.SendMessage(hWnd, NativeMethods.LB_SETHORIZONTALEXTENT, 0, 0);
                }
            }

            // Get the tracker items into a PositionTracker array
            internal PositionTracker[] GetPositionTrackers()
            {
                var selCount = (restoreSelection == null) ? 0 : restoreSelection.Length;
                var retVal = new PositionTracker[TRACKINDEX_FirstSelection + selCount];
                var column = restoreColumn;
                retVal[TRACKINDEX_RestoreTop].Initialize(restoreTop, -1);
                retVal[TRACKINDEX_Caret].Initialize(restoreCaret, column);
                retVal[TRACKINDEX_Anchor].Initialize(restoreAnchorIndex, column);
                //UNDONE: Temporary hack to make it easier to test selection tracking
                //				retVal[TRACKINDEX_RestoreTop].StartPosition = -1;
                //				retVal[TRACKINDEX_Caret].StartPosition = -1;
                //				retVal[TRACKINDEX_Anchor].StartPosition = -1;
                if (selCount != 0)
                {
                    for (var i = 0; i < selCount; ++i)
                    {
                        retVal[TRACKINDEX_FirstSelection + i].Initialize(restoreSelection[i], column);
                    }
                }
                return retVal;
            }

            internal void ApplyPositionTrackerData(PositionTracker[] trackers)
            {
                restoreTop = trackers[TRACKINDEX_RestoreTop].EndRow;
                restoreCaret = trackers[TRACKINDEX_Caret].EndRow;
                restoreAnchorIndex = trackers[TRACKINDEX_Anchor].EndRow;
                var count = trackers.Length;
                if (count > TRACKINDEX_FirstSelection)
                {
                    var newCount = 0;

                    // Pass 1, figure out how many node selections survived the process.
                    for (var i = TRACKINDEX_FirstSelection; i < count; ++i)
                    {
                        if (trackers[i].EndRow != VirtualTreeConstant.NullIndex)
                        {
                            ++newCount;
                        }
                    }

                    // Pass 2, reset the selection array
                    if (newCount == 0)
                    {
                        restoreSelection = null;
                    }
                    else
                    {
                        if (newCount != restoreSelection.Length)
                        {
                            restoreSelection = new int[newCount];
                        }
                        var nextSlot = 0;
                        int endPos;
                        for (var i = TRACKINDEX_FirstSelection; i < count; ++i)
                        {
                            endPos = trackers[i].EndRow;
                            if (endPos != VirtualTreeConstant.NullIndex)
                            {
                                restoreSelection[nextSlot] = endPos;
                                if (++nextSlot == newCount)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // Apply the index change to the selection array and other fields.
            internal void ApplyChange(
                int absIndex, int column, int change, int blanksAboveChange, int totalItemCount, bool isExpansionToggle)
            {
                // if change < 0, absIndex == -1 indicates that we are removing item(s) beginning with the first.
                // this case is handled by the code below.
                if (absIndex == -1
                    && change > 0)
                {
                    return; //UNDONE: How should we handle a change at the root node
                }
                var changeCeiling = absIndex + blanksAboveChange;
                if (restoreSelection != null)
                {
                    var upper = restoreSelection.GetUpperBound(0) + 1;
                    // Run separate optimized loops for the two different cases
                    if (change > 0)
                    {
                        for (var i = 0; i < upper; ++i)
                        {
                            if (restoreSelection[i] > changeCeiling)
                            {
                                restoreSelection[i] += change;
                            }
                        }
                    }
                    else if (change < 0)
                    {
                        int testIndex;
                        for (var i = 0; i < upper; ++i)
                        {
                            testIndex = restoreSelection[i];
                            if (testIndex >= 0
                                &&
                                testIndex > changeCeiling)
                            {
                                if (testIndex + change <= changeCeiling)
                                {
                                    testIndex = -1; // Item has been deleted
                                }
                                else
                                {
                                    testIndex += change;
                                }
                                restoreSelection[i] = testIndex;
                            }
                        }
                    }
                }
                if (restoreTop > changeCeiling
                    && restoreTop > 0)
                {
                    restoreTop += change;
                    if (restoreTop <= changeCeiling)
                    {
                        // The top is in the deletion range, adjust it
                        // and make sure we always do a full repaint.
                        startTop = -1;
                        restoreTop = changeCeiling + 1;
                        if (restoreTop == totalItemCount)
                        {
                            --restoreTop;
                        }
                    }
                }
                if (restoreCaret > changeCeiling)
                {
                    restoreCaret += change;
                    if (restoreCaret <= changeCeiling)
                    {
                        // The caret was deleted, move it to the parent node
                        restoreCaret = absIndex;
                        caretMoved = true;
                        restoreColumn = column;
                    }
                }
                else if (!isExpansionToggle
                         && restoreCaret == changeCeiling
                         && change > 0)
                {
                    // this happens when item(s) are inserted at the caret position.
                    // treat this as a caret move for selection purposes, since 
                    // the caret will be positioned on the new item upon restore
                    caretMoved = true;
                }
                if (restoreAnchorIndex > changeCeiling)
                {
                    restoreAnchorIndex += change;
                    if (restoreAnchorIndex <= changeCeiling)
                    {
                        // The anchor index was deleted, move it to the parent node
                        restoreAnchorIndex = absIndex;
                    }
                }
            }

            // Apply the index change to the selection array and other fields.
            internal void ApplyChange(ref ItemMovedEventArgs e)
            {
                if (e.UpdateTrailingColumns ? restoreColumn < e.Column : restoreColumn != e.Column)
                {
                    // Nothing to do
                    return;
                }

                // The required changes are broken into two adjacent ranges. One range shifts by +/-e.ItemCount,
                // and the other shifts by e.ToRow-e.FromRow. For an item moving up, the first range is
                // on top with a positive shift. For an item moving down, the first range is on the bottom
                // with a negative shift.

                int rangeTop; // The first item in the top range
                int rangeMiddle; // The first item in the bottom range
                int rangeBottom; // The first item not in either range
                int topShift; // The shift for the top range
                int bottomShift; // The shift for the bottom range
                var fromRow = e.FromRow;
                var toRow = e.ToRow;
                var itemCount = e.ItemCount;
                if (fromRow > toRow)
                {
                    // Item is moving up
                    rangeTop = toRow;
                    rangeMiddle = fromRow;
                    rangeBottom = fromRow + itemCount;
                    topShift = itemCount;
                    bottomShift = toRow - fromRow;
                }
                else
                {
                    // Item is moving down
                    rangeTop = fromRow;
                    rangeMiddle = fromRow + itemCount;
                    rangeBottom = toRow + 1;
                    topShift = toRow - fromRow;
                    bottomShift = -itemCount;
                }
                if (restoreSelection != null)
                {
                    var upper = restoreSelection.GetUpperBound(0) + 1;
                    int testIndex;
                    for (var i = 0; i < upper; ++i)
                    {
                        testIndex = restoreSelection[i];
                        if (testIndex >= rangeTop)
                        {
                            if (testIndex < rangeMiddle)
                            {
                                restoreSelection[i] = testIndex + topShift;
                            }
                            else if (testIndex < rangeBottom)
                            {
                                restoreSelection[i] = testIndex + bottomShift;
                            }
                        }
                    }
                }
                if (restoreTop >= rangeTop)
                {
                    if (restoreTop < rangeMiddle)
                    {
                        restoreTop += topShift;
                    }
                    else if (restoreTop < rangeBottom)
                    {
                        restoreTop += bottomShift;
                    }
                }
                if (restoreCaret >= rangeTop)
                {
                    if (restoreCaret < rangeMiddle)
                    {
                        restoreCaret += topShift;
                    }
                    else if (restoreCaret < rangeBottom)
                    {
                        restoreCaret += bottomShift;
                    }
                }
                if (restoreAnchorIndex >= rangeTop)
                {
                    if (restoreAnchorIndex < rangeMiddle)
                    {
                        restoreAnchorIndex += topShift;
                    }
                    else if (restoreAnchorIndex < rangeBottom)
                    {
                        restoreAnchorIndex += bottomShift;
                    }
                }
            }

            [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.SetScrollPos(System.IntPtr,Microsoft.Data.Entity.Design.VisualStudio.NativeMethods+ScrollBarType,System.Int32,System.Boolean)")]
            internal void Restore(VirtualTreeControl ctl)
            {
                var itemCount = ctl.ItemCount;
                if (itemCount == 0)
                {
                    if (restoreSelection != null
                        || restoreCaret >= 0)
                    {
                        // fire selection change, as we cannot restore the selection or caret.
                        ctl.DoSelectionChanged();
                    }
                    return;
                }
                var hWnd = ctl.Handle;
                if (restoreSelection != null)
                {
                    // The old selection is still there, make sure we clear it before restoring
                    ctl.ClearSelection(false);
                    var upper = restoreSelection.GetUpperBound(0) + 1;
                    for (var i = 0; i < upper; ++i)
                    {
                        // UNDONE: Be smarter about this and do ranges in addition to
                        // individual items.
                        if (restoreSelection[i] != -1
                            && restoreSelection[i] < itemCount)
                        {
                            ctl.SetSelected(restoreSelection[i], true);
                        }
                    }
                }
                if (startingListCount > 0)
                {
                    if (restoreCaret >= 0
                        && restoreCaret < itemCount)
                    {
                        try
                        {
                            if (!caretMoved)
                            {
                                ctl.SetStateFlag(VTCStateFlags.RestoringSelection, true);
                            }
                            if (restoreColumn != startColumn)
                            {
                                ctl.SetSelectionColumn(restoreColumn, false);
                            }
                            if (ctl.GetStyleFlag(VTCStyleFlags.ExtendedMultiSelect))
                            {
                                // Last parameter is a little strange. If there is no anchor, then the caret
                                // should not be selected, which is the normal behavior of the routine.
                                ctl.SetCurrentExtendedMultiSelectIndex(
                                    restoreCaret, false, true,
                                    (restoreAnchorIndex == -1) ? ModifySelectionAction.Clear : ModifySelectionAction.None);
                            }
                            else
                            {
                                ctl.CurrentIndex = restoreCaret;
                            }
                        }
                        finally
                        {
                            if (!caretMoved)
                            {
                                ctl.SetStateFlag(VTCStateFlags.RestoringSelection, false);
                            }
                        }
                    }
                    ctl.TopIndex = restoreTop;
                    if (restoreXPos != 0)
                    {
                        NativeMethods.SendMessage(hWnd, NativeMethods.LB_SETHORIZONTALEXTENT, restoreHExtent, 0);
                        NativeMethods.SetScrollPos(hWnd, NativeMethods.ScrollBarType.Horizontal, restoreXPos, true);
                        NativeMethods.SendMessage(
                            hWnd, NativeMethods.WM_HSCROLL,
                            NativeMethods.MAKELONG((int)NativeMethods.ScrollAction.ThumbPosition, restoreXPos), 0);
                        NativeMethods.SendMessage(hWnd, NativeMethods.WM_HSCROLL, (int)NativeMethods.ScrollAction.EndScroll, 0);
                    }
                    if (restoreAnchorIndex >= 0
                        && restoreAnchorIndex < itemCount)
                    {
                        ctl.AnchorIndex = restoreAnchorIndex;
                    }
                }
            }
        }

        private void OnItemMoved(object sender, ItemMovedEventArgs e)
        {
            BeginUpdate();
            var stateTracker = new ListBoxStateTracker(this);
            stateTracker.ApplyChange(ref e);
            stateTracker.Restore(this);
            EndUpdate();
            // UNDONE: Do some ScrollWindowEx and ValidateRect optimizations here to avoid redrawing
            // the whole tree. Make sure the TopIndex doesn't change and that there aren't pending redraws
            // in optimized regions. This should be done along with the OnToggleExpansion optimizations
        }

        private void OnToggleExpansion(ITree tree, int absRow, int column, int change, int blanksAboveChange, SubItemColumnAdjustmentCollection subItemChanges, bool isExpansionToggle)
        {
            OnToggleExpansion(tree.VisibleItemCount, absRow, column, change, blanksAboveChange, subItemChanges, isExpansionToggle);
        }

        private void OnItemCountChanged(object sender, ItemCountChangedEventArgs e)
        {
            OnToggleExpansion(
                e.Tree.VisibleItemCount, e.AnchorRow, e.Column, e.Change, e.BlanksAfterAnchor, e.HasSubItemChanges ? e.SubItemChanges : null,
                e.IsExpansionToggle);
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.InvalidateRect(System.IntPtr,Microsoft.Data.Entity.Design.VisualStudio.NativeMethods+RECT@,System.Boolean)")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.ValidateRect(System.IntPtr,System.IntPtr)")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.ValidateRect(System.IntPtr,Microsoft.Data.Entity.Design.VisualStudio.NativeMethods+RECT@)")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.GetClientRect(System.IntPtr,Microsoft.Data.Entity.Design.VisualStudio.NativeMethods+RECT@)")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void OnToggleExpansion(
            int visibleItemCount, int absIndex, int column, int change, int blanksAboveChange,
            SubItemColumnAdjustmentCollection subItemChanges, bool isExpansionToggle)
        {
            var checkLeftVertScroll = LeftScrollBar;
            var beforeVertScroll = HasVerticalScrollBar;

            var colPerm = myColumnPermutation;
            var adjustAfter = absIndex;
            var adjustChange = change;
            var adjustBlanksAboveChange = blanksAboveChange;
            var totalChange = change;
            var fullRowChangeBegin = -1;
            if (subItemChanges != null)
            {
                SubItemColumnAdjustment columnAdjustment;
                var changeCount = subItemChanges.Count;
                var selIndex = CurrentIndex;
                int displayColumnDummy;
                var nativeSelectionColumn = -1;
                for (var i = 0; i < changeCount; ++i)
                {
                    columnAdjustment = subItemChanges[i];
                    if (Math.Abs(totalChange) < Math.Abs(columnAdjustment.Change))
                    {
                        totalChange = columnAdjustment.Change;
                    }
                    if (i == (changeCount - 1))
                    {
                        if (selIndex > absIndex
                            && selIndex <= (absIndex + columnAdjustment.ItemsBelowAnchor))
                        {
                            if (nativeSelectionColumn == -1)
                            {
                                ResolveSelectionColumn(selIndex, out displayColumnDummy, out nativeSelectionColumn);
                            }
                            if (columnAdjustment.Column != nativeSelectionColumn)
                            {
                                adjustAfter = absIndex + columnAdjustment.ItemsBelowAnchor;
                                adjustBlanksAboveChange = 0;
                            }
                            else
                            {
                                adjustChange = totalChange;
                            }
                        }
                        fullRowChangeBegin = absIndex + Math.Abs(columnAdjustment.Change - change) + 1;
                    }
                }
            }

            if (totalChange == 0)
            {
                OnDisplayDataChanged(VirtualTreeDisplayDataChanges.ItemButton, absIndex, column, 1);
                return;
            }
            var hWnd = Handle;
            NativeMethods.RECT pendingUpdateRect;
            var paintPending = NativeMethods.GetUpdateRect(hWnd, out pendingUpdateRect, false);
            BeginUpdate();

            // We need the top index for both cases to determine if it has changed.
            // A top index change indicates that the whole window needs refreshing,
            // no change indicates we only need to update the portion below absIndex.
            var stateTracker = new ListBoxStateTracker(this);
            // The window's listbox doesn't allow a mechanism for inserting
            // items in a no-data listbox, so everything vital (selection, caret,
            // and top index) must be explicitly maintained. LB_DELETESTRING does
            // work (unlike LB_INSERTSTRING), but there are performance penalties
            // with multiple WM_DELETEITEM messages due to the callbacks to the parent
            // object, so we handle deletion in a similar fashion.

            // Adjust cached settings
            stateTracker.ApplyChange(adjustAfter, column, adjustChange, adjustBlanksAboveChange, visibleItemCount, isExpansionToggle);

            try
            {
                // Change the ItemCount. This clears the selection, scroll state, etc.  Set RestoringSelection
                // flag so that we don't fire events for this, since we will restore it below.
                SetStateFlag(VTCStateFlags.RestoringSelection, true);
                ItemCount = visibleItemCount;
            }
            finally
            {
                SetStateFlag(VTCStateFlags.RestoringSelection, false);
            }

            // see if there was enough room for the expansion
            if (isExpansionToggle && change > 0)
            {
                var spaceNeeded = totalChange + blanksAboveChange - (myFullyVisibleCount - 1 - (absIndex - stateTracker.restoreTop));
                if (spaceNeeded > 0)
                {
                    var newCaret = stateTracker.restoreTop + spaceNeeded;

                    if (newCaret > absIndex)
                    {
                        newCaret = absIndex;
                    }

                    stateTracker.restoreTop = newCaret;
                }

                if (absIndex < stateTracker.restoreTop)
                {
                    stateTracker.restoreTop = absIndex;
                }
            }

            // Restore the previous state.  Only do this if we're not in the middle of a list shuffle, 
            // as in that case the shuffle will take care of this when it finishes.
            if (myTree != null
                && !myTree.ListShuffle)
            {
                stateTracker.Restore(this);
            }

            EndUpdate();

            // UNDONE: If the scrollbar has appeared/disappeared when it is on the left,
            // then we want vertical gridlines to not change. Unfortunately, the listbox
            // automatically scrolls the window for us, so column 0 is correct, and the rest
            // are shifted by a scrollbar width. This means that we should be able to validate
            // large portions of column 0.

            var invalidatePending = false;
            var subsequentSubItems = false;

            // only perfom the optimization if we arent horizontally scrolled. (bug 38565)
            if (change != 0
                && myXPos == 0)
            {
                // If the expanded item is below the top, and the top
                // hasn't moved, then we can get away with only repainting
                // a portion (possibly none) of the window. Make sure we
                // only attempt to Validate regions if there is no paint
                // event pending when this occurs.
                var afterVertScroll = HasVerticalScrollBar;
                if (absIndex >= stateTracker.startTop
                    &&
                    (!checkLeftVertScroll || afterVertScroll == beforeVertScroll)
                    &&
                    stateTracker.startTop == TopIndex)
                {
                    NativeMethods.RECT itemRect;
                    NativeMethods.RECT anchorRect;
                    NativeMethods.RECT clientRect;
                    var offsetAnchor = fullRowChangeBegin != -1;
                    NativeMethods.SendMessage(hWnd, NativeMethods.LB_GETITEMRECT, absIndex, out anchorRect);
                    itemRect = anchorRect;
                    if (offsetAnchor)
                    {
                        // The full row change begin may be after the current number of items in the control, so
                        // we can't use LB_GETITEMRECT to pick it up. Use an offset from absIndex instead.
                        var offsetBy = (fullRowChangeBegin - absIndex) * itemRect.height;
                        if (offsetBy != 0)
                        {
                            itemRect.bottom += offsetBy;
                            itemRect.top += offsetBy;
                        }
                        if (HasHorizontalGridLines)
                        {
                            // Make sure the preceding gridline is redraw
                            itemRect.top -= 1;
                        }
                    }
                    NativeMethods.GetClientRect(hWnd, out clientRect);
                    var restoreXPos = stateTracker.restoreXPos;
                    if (anchorRect.bottom >= clientRect.top
                        && anchorRect.top <= clientRect.bottom)
                    {
                        // The top of the client area doesn't change.
                        // UNDONE: This can be further optimized with ScrollWindowEx, but this
                        // is a pretty good painting optimization for a first go round.
                        clientRect.bottom = itemRect.top - 1;
                        if (clientRect.width >= restoreXPos)
                        {
                            // The scrollbar acrobatics above leave the upper right portion
                            // of the window blank, adjust the rect accordingly
                            clientRect.right -= restoreXPos;
                            // TFS 166541: We used to adjust the clientRect for the scrollbar width
                            // only. However, that did not work if the last column contained right
                            // aligned content. So, we now adjust the clientRect for the entire last
                            // column width (which includes the scrollbar width change).
                            if (afterVertScroll != beforeVertScroll)
                            {
                                var numColumns = ColumnCount;
                                var lastColumn = numColumns - 1;

                                var columnRect = Rectangle.FromLTRB(clientRect.left, clientRect.top, clientRect.right, clientRect.bottom);
                                LimitRectToColumn(lastColumn, ref columnRect);
                                clientRect.right -= columnRect.Width;
                            }
                            NativeMethods.ValidateRect(hWnd, ref clientRect);
                            invalidatePending = paintPending;
                        }
                    }
                    else
                    {
                        // None of the client area changes
                        NativeMethods.ValidateRect(hWnd, IntPtr.Zero);
                        invalidatePending = paintPending;
                    }
                }
                subsequentSubItems = true;
            }
            if (subItemChanges != null)
            {
                if (subsequentSubItems || stateTracker.startTop == TopIndex)
                {
                    // Since the total item count did not change, we don't have to worry
                    // about scrollbars coming/going here
                    if (!subsequentSubItems)
                    {
                        NativeMethods.ValidateRect(hWnd, IntPtr.Zero);
                        invalidatePending = paintPending;
                    }
                    SubItemColumnAdjustment columnAdjustment;
                    var changeCount = subItemChanges.Count;
                    for (var i = 0; i < changeCount; ++i)
                    {
                        columnAdjustment = subItemChanges[i];
                        var displayColumn = columnAdjustment.Column;
                        if (colPerm != null)
                        {
                            displayColumn = colPerm.GetPermutedColumn(displayColumn);
                            if (displayColumn == -1)
                            {
                                continue;
                            }
                        }
                        var columnRect = GetItemRectangle(absIndex, displayColumn);
                        var leftColumn = displayColumn;
                        var rightColumn = displayColumn;
                        if (colPerm != null)
                        {
                            var columnExpansion = colPerm.GetColumnExpansion(displayColumn, columnAdjustment.LastColumnOnRow);
                            if (columnExpansion.Width > 1)
                            {
                                leftColumn = columnExpansion.LeftColumn;
                                rightColumn = columnExpansion.RightColumn;
                            }
                        }
                        else if (displayColumn == columnAdjustment.LastColumnOnRow)
                        {
                            var lastColumn = myMctree.ColumnCount - 1;
                            if (displayColumn < lastColumn)
                            {
                                rightColumn = lastColumn;
                            }
                        }
                        if (leftColumn != rightColumn)
                        {
                            int columnLeft;
                            int columnWidth;
                            GetColumnBounds(leftColumn, rightColumn, out columnLeft, out columnWidth);
                            columnRect.Width = columnWidth;
                            columnRect.X = columnLeft;
                        }
                        NativeMethods.RECT rect;
                        // UNDONE: Use the Change and TrailingItems values to optimize this.
                        // Easy for now: Just redraw the rest of the column
                        var changeRows = columnAdjustment.ItemsBelowAnchor + Math.Abs(columnAdjustment.Change - change) + 1;
                        if (changeRows > 0)
                        {
                            if (changeRows > 1)
                            {
                                columnRect.Inflate(0, columnRect.Height * (changeRows - 1));
                            }
                            rect = new NativeMethods.RECT(columnRect);
                            NativeMethods.InvalidateRect(hWnd, ref rect, false);
                        }
                    }
                }
            }
            if (invalidatePending)
            {
                NativeMethods.InvalidateRect(hWnd, ref pendingUpdateRect, false);
            }

            if (totalChange > 0)
            {
                ScrollBarsAfterExpand(absIndex, totalChange);
            }
            else if (totalChange < 0)
            {
                ScrollBarsAfterCollapse(absIndex, -totalChange);
            }

            if (VirtualTreeAccEvents.ShouldNotify(VirtualTreeAccEvents.eventObjectStateChange, this))
            {
                VirtualTreeAccEvents.Notify(
                    VirtualTreeAccEvents.eventObjectStateChange, absIndex, 0,
                    this);
            }
        }

        private void DoExpandRecurse(int absIndex, int column, ListBoxStateTracker stateTracker)
        {
            if (absIndex != VirtualTreeConstant.NullIndex)
            {
                // Converted from a recursive function to iterative (TFS 235913).
                // The recursive function expanded the branches from top
                // to bottom. This iterative version expands from bottom to
                // top. Expanding a branch causes the absIndex of items below
                // the expansion to change. Expanding from bottom to top means
                // our remaining stack always contains absIndexes not affected
                // by any expansions.
                var remaining = new Stack<int>(256);
                remaining.Push(absIndex);

                while (remaining.Count > 0)
                {
                    absIndex = remaining.Pop();

                    //This will always be called with an index corresponding
                    //to an open, expanded list, so Index + 1 will always 
                    //be a valid Index.  This returns the list for
                    //descendants of this item.
                    var branch = myTree.GetExpandedBranch(absIndex, column).Branch;
                    if (0 != (branch.Features & BranchFeatures.Expansions))
                    {
                        var itemCount = branch.VisibleItemCount;
                        bool recurse;
                        int relIndex;
                        absIndex += myTree.GetSubItemCount(absIndex, column) + 1;
                        for (relIndex = 0; relIndex < itemCount; ++relIndex, ++absIndex)
                        {
                            if (branch.IsExpandable(relIndex, column))
                            {
                                if (myTree.IsExpanded(absIndex, column))
                                {
                                    recurse = true;
                                }
                                else
                                {
                                    var toggle = myTree.ToggleExpansion(absIndex, column);
                                    recurse = toggle.AllowRecursion;
                                    // UNDONE_NOW: The blank expansion is wrong. ToggleExpansionAbsolute should
                                    // return this data.
                                    stateTracker.ApplyChange(absIndex, column, toggle.Change, 0, myTree.VisibleItemCount, true);
                                }
                                if (recurse)
                                {
                                    // We used to recurse here. We now mark the branch for later expansion.
                                    remaining.Push(absIndex);
                                }
                                absIndex += myTree.GetDescendantItemCount(absIndex, column, true, false);
                            }
                            else
                            {
                                absIndex += myTree.GetSubItemCount(absIndex, column);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Recursively expand the branch at the given location.
        /// </summary>
        /// <param name="row">The row coordinate.</param>
        /// <param name="column">The native column coordinate.</param>
        public void ExpandRecurse(int row, int column)
        {
            // Turn off the event temporarily. We'll do all of the tracking in this routine instead of
            // firing multiple events.
            myTree.ItemCountChanged -= OnItemCountChanged;
            try
            {
                if (myTree.IsExpandable(row, column))
                {
                    bool allowRecursion;
                    int change;
                    if (myTree.IsExpanded(row, column))
                    {
                        allowRecursion = myTree.GetDescendantItemCount(row, column, false, false) > 0;
                        change = 0;
                    }
                    else
                    {
                        var toggle = myTree.ToggleExpansion(row, column);
                        change = toggle.Change;
                        allowRecursion = toggle.AllowRecursion;
                        if (change > 0)
                        {
                            if (!allowRecursion)
                            {
                                // Fire the event manually
                                // UNDONE_NOW: The blank expansion is wrong
                                OnToggleExpansion(myTree, row, column, change, 0, null, true);
                            }
                        }
                        else if (change == 0)
                        {
                            if (GetStyleFlag(VTCStyleFlags.HasButtons))
                            {
                                // Make sure the plus/minus changes
                                InvalidateItem(row, column, NativeMethods.RedrawWindowFlags.Invalidate);
                            }
                            allowRecursion = false;
                        }
                    }
                    if (allowRecursion)
                    {
                        var stateTracker = new ListBoxStateTracker(this);
                        if (change != 0)
                        {
                            // UNDONE_NOW: The blank expansion is wrong. We need to pass real data here.
                            stateTracker.ApplyChange(row, column, change, 0, myTree.VisibleItemCount, true);
                        }
                        DoExpandRecurse(row, column, stateTracker);
                        BeginUpdate();
                        ItemCount = myTree.VisibleItemCount;
                        stateTracker.Restore(this);
                        var iTop = TopIndex;
                        if (iTop > 0
                            && row < iTop)
                        {
                            TopIndex = row;
                        }
                        ScrollBarsAfterSetWidth(VirtualTreeConstant.NullIndex);
                        EndUpdate();
                    }
                }
            }
            finally
            {
                myTree.ItemCountChanged += OnItemCountChanged;
            }
        }

        private void OnSetRedraw(object sender, SetRedrawEventArgs args)
        {
            if (args.RedrawOn)
            {
                EndUpdate();
            }
            else
            {
                BeginUpdate();
            }
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            if (IsHandleCreated)
            {
                BeginUpdate();
                DismissLabelEdit(true, false); // cancel outstanding label edits prior to refresh
                var tree = sender as ITree;
                // use the tree's VisibleItemCount (bug 63308)
                ItemCount = tree.Root != null ? tree.VisibleItemCount : 0;
                EndUpdate();
            }
        }

        private void OnToggleState(object sender, ToggleStateEventArgs e)
        {
            if (e.StateRefreshOptions == StateRefreshChanges.Current)
            {
                // UNDONE: Does not handle cases where other display changes (glyph, bolding, etc) in
                // response to a state toggle
                OnDisplayDataChanged(VirtualTreeDisplayDataChanges.StateImage, e.Row, e.Column, 1);
            }
            else
            {
                // UNDONE: Can be much cleaner than this
                Refresh();
            }

            if (VirtualTreeAccEvents.ShouldNotify(VirtualTreeAccEvents.eventObjectStateChange, this))
            {
                var displayColumn = myColumnPermutation == null ? e.Column : myColumnPermutation.GetPermutedColumn(e.Column);
                VirtualTreeAccEvents.Notify(
                    VirtualTreeAccEvents.eventObjectStateChange, e.Row, displayColumn,
                    this);
            }
        }

        private void OnDisplayDataChanged(object sender, DisplayDataChangedEventArgs e)
        {
            OnDisplayDataChanged(e.Changes, e.StartRow, e.Column, e.Count);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void OnDisplayDataChanged(VirtualTreeDisplayDataChanges changes, int row, int column, int count)
        {
            if (IsHandleCreated)
            {
                // Normalize column
                var nativeColumn = column;
                if (column < 0)
                {
                    column = -1;
                }
                else if (myColumnPermutation != null)
                {
                    column = myColumnPermutation.GetPermutedColumn(column);
                }
                if (changes != VirtualTreeDisplayDataChanges.AccessibleValue)
                    // no need to invalidate if AccessibleValue is only flag specified.
                {
                    // UNDONE: Look at changes, only update the appropriate portions
                    InvalidateItem(row, column, NativeMethods.RedrawWindowFlags.Invalidate, count);
                }
                if (0 != (changes & VirtualTreeDisplayDataChanges.DependentUIElements))
                {
                    int testIndex;
                    if (GetStyleFlag(VTCStyleFlags.MultiSelect))
                    {
                        if (TreatAsFocused)
                        {
                            var iter = CreateSelectedItemEnumerator();
                            if (iter != null)
                            {
                                while (iter.MoveNext())
                                {
                                    if (column == -1
                                        || iter.ColumnInTree == column)
                                    {
                                        testIndex = iter.RowInTree;
                                        if ((testIndex >= row)
                                            && (testIndex < row + count))
                                        {
                                            DoSelectionChanged();
                                            // One call is enough, get out
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (((column == -1) || (column == mySelectionColumn))
                            &&
                            (-1 != (testIndex = CurrentIndex))
                            &&
                            ((testIndex >= row) && (testIndex < row + count))
                            &&
                            (TreatAsFocused))
                        {
                            DoSelectionChanged();
                        }
                    }
                }
                if (0 != (changes & VirtualTreeDisplayDataChanges.AccessibleValue)
                    && nativeColumn >= 0)
                {
                    // indicates a change in cell value, fire accessibility events.  Fire value change event for 
                    // simple cells, and name change event for outline items (because value in an outline item is the depth in the tree).
                    for (var curRow = row; curRow < (row + count); curRow++)
                    {
                        var info = Tree.GetItemInfo(curRow, nativeColumn, true);
                        if (!info.Blank)
                        {
                            if (AccTreeRoot.IsSimpleItem(this, curRow, nativeColumn - info.Column, nativeColumn))
                            {
                                if (VirtualTreeAccEvents.ShouldNotify(VirtualTreeAccEvents.eventObjectValueChange, this))
                                {
                                    VirtualTreeAccEvents.Notify(VirtualTreeAccEvents.eventObjectValueChange, curRow, column, this);
                                }
                            }
                            else
                            {
                                if (VirtualTreeAccEvents.ShouldNotify(VirtualTreeAccEvents.eventObjectNameChange, this))
                                {
                                    VirtualTreeAccEvents.Notify(VirtualTreeAccEvents.eventObjectNameChange, curRow, column, this);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ListShuffleBeginning(object sender, PositionManagerEventArgs manager)
        {
            myShuffleTracker = new ListBoxStateTrackerClass(this);
            var cache = myShuffleTracker.Inner.GetPositionTrackers();
            if (cache != null)
            {
                manager.StorePositions(cache, this, myMctree != null);
            }
        }

        private void ListShuffleEnding(object sender, PositionManagerEventArgs manager)
        {
            var lbstate = myShuffleTracker;
            myShuffleTracker = null;
            if (lbstate != null)
            {
                var cache = manager.RetrievePositions(this, myMctree != null);
                if (cache != null)
                {
                    lbstate.Inner.ApplyPositionTrackerData(cache);
                    lbstate.Inner.Restore(this);
                }
            }
        }

        #endregion //VirtualTree Events

        #region Virtual Tree Integration Functions

        private void CheckIndex(int index)
        {
            if (index < 0
                || myTree == null
                || index >= myTree.VisibleItemCount)
            {
                throw new ArgumentOutOfRangeException("index");
            }
        }

        /// <summary>
        ///     Retrieve basic information about the item at this row and column. Use HitInfoExtended
        ///     for more complete information.
        /// </summary>
        /// <param name="x">The x position, in client coordinates.</param>
        /// <param name="y">The y position, in client coordinates.</param>
        public VirtualTreeHitInfo HitInfo(int x, int y)
        {
            ExtraHitInfo extraInfo;
            return HitInfo(x, y, out extraInfo, false);
        }

        /// <summary>
        ///     Retrieve information about the item at this row and column. Includes additional
        ///     information about the location of different UI elements.
        /// </summary>
        /// <param name="x">The x position, in client coordinates.</param>
        /// <param name="y">The y position, in client coordinates.</param>
        public VirtualTreeExtendedHitInfo HitInfoExtended(int x, int y)
        {
            ExtraHitInfo extraInfo;
            var hitInfo = HitInfo(x, y, out extraInfo, true);
            return new VirtualTreeExtendedHitInfo(ref hitInfo, ref extraInfo);
        }

        /// <summary>
        ///     Determine the column for a given hit point, as well as the
        ///     left and width of the column. The values returned here are
        ///     not adjusted by the horizontal scrollbar position.
        /// </summary>
        /// <param name="x">The x offset into row (unadjusted)</param>
        /// <param name="itemLeft">The left edge of this column</param>
        /// <param name="itemWidth">The width of this column</param>
        /// <returns>The column x is in</returns>
        private int ColumnHitTest(int x, out int itemLeft, out int itemWidth)
        {
            var column = 0;
            var columns = (myColumnPermutation != null) ? myColumnPermutation.VisibleColumnCount : myMctree.ColumnCount;
            var adjustLeft = false;
            var adjustRight = false;
            if (!HasVerticalScrollBar)
            {
                if (LeftScrollBar)
                {
                    adjustLeft = true;
                }
                else if (column == (columns - 1))
                {
                    adjustRight = true;
                }
            }
            var fullPercentWidth = FullPercentageHeaderWidth;
            itemLeft = 0;
            itemWidth = 0;
            if (adjustLeft)
            {
                itemLeft = SystemInformation.VerticalScrollBarWidth;
                if (x < itemLeft)
                {
                    if (!myHeaderBounds.HasHeaders)
                    {
                        itemWidth = fullPercentWidth / columns + itemLeft;
                    }
                    else
                    {
                        var cacheLeft = itemLeft;
                        myHeaderBounds.GetColumnBounds(fullPercentWidth, 0, 0, out itemLeft, out itemWidth);
                        itemWidth += cacheLeft;
                    }
                    itemLeft = 0;
                    return 0;
                }
                else
                {
                    x -= itemLeft;
                }
            }
            int columnWidth;
            if (!myHeaderBounds.HasHeaders)
            {
                columnWidth = fullPercentWidth / columns;
                while (x > columnWidth)
                {
                    x -= columnWidth;
                    ++column;
                }
                itemLeft += column * columnWidth;
                itemWidth = columnWidth;
            }
            else
            {
                int leftIncr;
                column = myHeaderBounds.ColumnHitTest(fullPercentWidth, x, out leftIncr, out itemWidth);
                itemLeft += leftIncr;
            }
            if (adjustRight)
            {
                if (column == (columns - 1))
                {
                    itemWidth += SystemInformation.VerticalScrollBarWidth;
                }
            }
            else if (adjustLeft && column == 0)
            {
                itemLeft = 0;
                itemWidth += SystemInformation.VerticalScrollBarWidth;
            }
            return column;
        }

        /// <summary>
        ///     Get the left and width for the expanded column. This is called
        ///     after ColumnHitTest determines the column, and GetBlankExpansion
        ///     determines the column range.
        /// </summary>
        /// <param name="leftColumn">The leftmost column</param>
        /// <param name="rightColumn">The rightmost column</param>
        /// <param name="itemLeft">The left edge of this column</param>
        /// <param name="itemWidth">The width of this column</param>
        private void GetColumnBounds(int leftColumn, int rightColumn, out int itemLeft, out int itemWidth)
        {
            var columns = (myColumnPermutation != null) ? myColumnPermutation.VisibleColumnCount : myMctree.ColumnCount;
            var adjustLeft = false;
            var adjustRight = false;
            if (!HasVerticalScrollBar)
            {
                if (LeftScrollBar)
                {
                    adjustLeft = true;
                }
                else if (rightColumn == (columns - 1))
                {
                    adjustRight = true;
                }
            }
            var fullPercentWidth = FullPercentageHeaderWidth;
            itemLeft = 0;
            itemWidth = 0;
            int columnWidth;
            if (!myHeaderBounds.HasHeaders)
            {
                columnWidth = fullPercentWidth / columns;
                if (leftColumn > 0)
                {
                    itemLeft = columnWidth * leftColumn;
                }
                if (leftColumn == rightColumn)
                {
                    itemWidth = columnWidth;
                }
                else if (rightColumn == (columns - 1))
                {
                    itemWidth = fullPercentWidth - itemLeft;
                }
                else
                {
                    itemWidth = (rightColumn - leftColumn + 1) * columnWidth;
                }
            }
            else
            {
                myHeaderBounds.GetColumnBounds(fullPercentWidth, leftColumn, rightColumn, out itemLeft, out itemWidth);
            }
            if (adjustRight)
            {
                itemWidth += SystemInformation.VerticalScrollBarWidth;
            }
            else if (adjustLeft)
            {
                if (leftColumn == 0)
                {
                    itemWidth += SystemInformation.VerticalScrollBarWidth;
                }
                else
                {
                    itemLeft += SystemInformation.VerticalScrollBarWidth;
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private VirtualTreeHitInfo HitInfo(int x, int y, out ExtraHitInfo extraInfo, bool populateExtras)
        {
            extraInfo = new ExtraHitInfo();
            var target = VirtualTreeHitTargets.Uninitialized;
            var absIndex = VirtualTreeConstant.NullIndex;
            var column = 0;
            var nativeColumn = 0;
            var rawRow = absIndex;
            var rawColumn = absIndex;

            if (x < 0)
            {
                target |= VirtualTreeHitTargets.ToLeft;
            }
            else if (x > ClientSize.Width)
            {
                target |= VirtualTreeHitTargets.ToRight;
            }

            if (y < 0)
            {
                target |= VirtualTreeHitTargets.Above;
            }
            else if (y > ClientSize.Height)
            {
                target |= VirtualTreeHitTargets.Below;
            }
            if (target != VirtualTreeHitTargets.Uninitialized)
            {
                return new VirtualTreeHitInfo(absIndex, column, target);
            }

            absIndex = rawRow = IndexFromPoint(x, y);

            if ((absIndex < 0)
                ||
                (myTree == null)
                ||
                (absIndex >= myTree.VisibleItemCount))
            {
                return new VirtualTreeHitInfo(VirtualTreeConstant.NullIndex, column, VirtualTreeHitTargets.NoWhere);
            }

            // Shift by the horizontal scroll position
            var scrollShift = myXPos;
            x += scrollShift;

            // Determine the current column
            int level;
            int relIndex;
            IBranch branch;
            var multiColumn = myMctree != null;
            var labelShift = 0;
            int itemWidth;
            int itemLeft;
            if (multiColumn)
            {
                column = rawColumn = ColumnHitTest(x, out itemLeft, out itemWidth);
                int columns;
                if (myColumnPermutation == null)
                {
                    columns = myMctree.ColumnCount;
                    nativeColumn = column;
                }
                else
                {
                    columns = myColumnPermutation.VisibleColumnCount;
                    nativeColumn = myColumnPermutation.GetNativeColumn(column);
                }
                if (column >= columns)
                {
                    return new VirtualTreeHitInfo(VirtualTreeConstant.NullIndex, column, VirtualTreeHitTargets.NoWhere);
                }
            }
            else
            {
                itemLeft = 0;
                itemWidth = ClientSize.Width;
            }
            var leadingBlanksLeft = itemLeft;
            var checkRootLines = GetAnyStyleFlag(VTCStyleFlags.HasRootLines | VTCStyleFlags.HasRootButtons);
            var info = myTree.GetItemInfo(absIndex, nativeColumn, checkRootLines && nativeColumn > 0);
            VirtualTreeHitTargets blankTargetBit = 0;
            var testingBlank = info.Blank;
            if (multiColumn)
            {
                var expansion = myTree.GetBlankExpansion(absIndex, column, myColumnPermutation);
                //Debug.WriteLine(string.Format("Left/Top ({0}, {1}) Anchor ({2}, {3}) Width {4} Height {5}", expansion.LeftColumn, expansion.TopRow, expansion.AnchorColumn, expansion.TopRow, expansion.Width, expansion.Height));
                if (expansion.AnchorColumn != VirtualTreeConstant.NullIndex)
                {
                    if (testingBlank)
                    {
                        blankTargetBit = VirtualTreeHitTargets.OnBlankItem;
                        absIndex = expansion.TopRow;
                        column = expansion.AnchorColumn;
                        nativeColumn = (myColumnPermutation == null) ? column : myColumnPermutation.GetNativeColumn(column);
                        // If the expansion width is 1, then we're in a blank expansion below
                        // the last item in a column. Treat/ this the same as hovering on the
                        // main item, except that we don't acknowledge glyph or button hovers.
                        info = myTree.GetItemInfo(absIndex, nativeColumn, checkRootLines && nativeColumn > 0);
                    }
                    if (expansion.Width > 1)
                    {
                        // The item is wider than a single column. This type of item
                        // always extends the full width of the tree. Retrieve new information.
                        GetColumnBounds(expansion.LeftColumn, expansion.RightColumn, out itemLeft, out itemWidth);
                        leadingBlanksLeft = itemLeft;
                        if (expansion.LeftColumn != expansion.AnchorColumn)
                        {
                            GetColumnBounds(expansion.AnchorColumn, expansion.RightColumn, out itemLeft, out itemWidth);
                        }
                    }
                }
            }
            if (info.Blank)
            {
                return new VirtualTreeHitInfo(rawRow, rawColumn, VirtualTreeHitTargets.OnBlankItem);
            }
            else if (itemLeft != leadingBlanksLeft
                     && x < itemLeft)
            {
                return new VirtualTreeHitInfo(
                    absIndex, column, nativeColumn, rawRow, rawColumn, VirtualTreeHitTargets.OnItemLeft | blankTargetBit);
            }

            level = info.Level;
            branch = info.Branch;
            relIndex = info.Row;

            // Shift by the indent width
            var xItemStart = level * myIndentWidth + itemLeft;
            if (checkRootLines && (nativeColumn == 0 || !info.SimpleCell))
            {
                xItemStart += myIndentWidth;
            }

            if (x < xItemStart)
            {
                // We're in the indent region
                target = VirtualTreeHitTargets.OnItemIndent;

                // See if we're actually on a button, not just an indent
                if (HasButtons && ((x + myIndentWidth) > xItemStart))
                {
                    if (myTree.IsExpandable(absIndex, nativeColumn))
                    {
                        target = VirtualTreeHitTargets.OnItemButton;
                    }
                }
                //UNDONE Find the item whose expansion we're clicking on, and check whether
                //or not we're within the bounds of the button width of the item.
                return new VirtualTreeHitInfo(absIndex, column, nativeColumn, rawRow, rawColumn, target | blankTargetBit);
            }
            else
            {
                x -= xItemStart;
                var tddMasks = new VirtualTreeDisplayDataMasks(
                    VirtualTreeDisplayMasks.State, VirtualTreeDisplayStates.Bold | VirtualTreeDisplayStates.TextAlignFar);
                if (myImageWidth > 0)
                {
                    tddMasks.Mask |= VirtualTreeDisplayMasks.Image;
                }
                if (myStateImageWidth > 0)
                {
                    tddMasks.Mask |= VirtualTreeDisplayMasks.StateImage;
                }
                var tdd = branch.GetDisplayData(relIndex, info.Column, tddMasks);

                // Check if we're on the state icon.
                if (myStateImageWidth > 0)
                {
                    if (tdd.StateImageIndex >= 0)
                    {
                        if (x < myStateImageWidth)
                        {
                            target = VirtualTreeHitTargets.OnItemStateIcon;

                            if (StandardCheckBoxes
                                && tdd.StateImageList == null
                                &&
                                (tdd.StateImageIndex == (int)StandardCheckBoxImage.Unchecked
                                 || tdd.StateImageIndex == (int)StandardCheckBoxImage.Checked
                                 || tdd.StateImageIndex == (int)StandardCheckBoxImage.Indeterminate))
                            {
                                target |= VirtualTreeHitTargets.StateIconHotTracked;
                            }
                        }
                        else
                        {
                            x -= myStateImageWidth;
                        }
                        labelShift += myStateImageWidth;
                    }
                }

                // Check if we're on the image icon
                if (myImageWidth > 0)
                {
                    if (tdd.Image != -1)
                    {
                        if (x < myImageWidth)
                        {
                            if (target == VirtualTreeHitTargets.Uninitialized)
                            {
                                target = VirtualTreeHitTargets.OnItemIcon;
                            }
                        }
                        else
                        {
                            x -= myImageWidth;
                        }
                        labelShift += myImageWidth;
                    }
                }

                if (populateExtras || target == VirtualTreeHitTargets.Uninitialized)
                {
                    // See where we are on the item's label
                    var labelFont = (0 == (tdd.State & VirtualTreeDisplayStates.Bold)) ? Font : BoldFont;
                    var iWidth = ListItemStringWidth(labelFont, ref info);
                    var textAlign = GetLabelTextAlignment(ref tdd);
                    if (RightToLeft == RightToLeft.Yes)
                    {
                        // For purposes of hit test calculations, RTL switches near/far alignment. 
                        textAlign = textAlign == StringAlignment.Far ? StringAlignment.Near : StringAlignment.Far;
                    }
                    if (target == VirtualTreeHitTargets.Uninitialized)
                    {
                        if (textAlign == StringAlignment.Near)
                        {
                            target = (x < iWidth)
                                         ? VirtualTreeHitTargets.OnItemLabel
                                         : VirtualTreeHitTargets.OnItemRight;
                        }
                        else
                        {
                            // add labelShift and xItemStart because they've been subtracted from original x value above.
                            target = (x + labelShift + xItemStart > itemLeft + itemWidth - iWidth)
                                         ? VirtualTreeHitTargets.OnItemLabel
                                         : VirtualTreeHitTargets.OnItemLeft;
                        }
                    }

                    if (populateExtras)
                    {
                        var currentTopIndex = TopIndex;
                        extraInfo.IsTruncated =
                            (rawRow != absIndex && absIndex < currentTopIndex) ||
                            ((rawRow == absIndex) &&
                             (0 == (target & (VirtualTreeHitTargets.OnItemRight | VirtualTreeHitTargets.OnItemLeft))) &&
                             ((xItemStart + labelShift + SystemInformation.Border3DSize.Width + iWidth > itemLeft + itemWidth) ||
                              (labelShift + SystemInformation.Border3DSize.Width < 0)));
                        extraInfo.LabelOffset = labelShift;
                        var top = (rawRow - currentTopIndex) * myItemHeight;
                        extraInfo.ClippedItemRectangle = new Rectangle(
                            xItemStart - scrollShift,
                            top,
                            (extraInfo.IsTruncated || textAlign == StringAlignment.Far)
                                ? itemLeft + itemWidth - xItemStart
                                : labelShift + iWidth,
                            // in right align case, clipped item rectangle will always extend the full width, since text draws from the right.
                            myItemHeight);
                        extraInfo.FullLabelRectangle = new Rectangle(
                            xItemStart + labelShift - scrollShift,
                            top,
                            iWidth,
                            myItemHeight);
                        if (textAlign == StringAlignment.Far)
                        {
                            // account for alignment away from glyph.
                            extraInfo.FullLabelRectangle.X = itemLeft + itemWidth - scrollShift - iWidth;
                        }
                        else if (multiColumn
                                 && HasVerticalGridLines
                                 && (column != 0))
                        {
                            // account for vertical gridlines
                            extraInfo.FullLabelRectangle.X += 1;
                        }
                        extraInfo.LabelFont = labelFont;
                        extraInfo.LabelFormat = StringFormat;
                        extraInfo.LabelFormat.Alignment = GetLabelTextAlignment(ref tdd);
                            // get alignment again, so it's not adjusted for RTL.
                    }
                }
            }
            Debug.Assert(target != VirtualTreeHitTargets.Uninitialized);
            return new VirtualTreeHitInfo(absIndex, column, nativeColumn, rawRow, rawColumn, target | blankTargetBit);

            /*INDEX index;
            int cxState, cxImage;
            int xShift;
            ULONG Level;
            IVsLiteTreeList *ptl;
            ULONG ListIndex;
            WORD iWidth;
            VSTREEDISPLAYDATA tdd;

            xShift = Level * pTree->cxIndent;
            xShift -= pTree->xPos;

            if ((pTree->ci.style & (TVS_HASLINES | TVS_HASBUTTONS)) &&
                (pTree->ci.style &TVS_LINESATROOT))
            {
                // Subtract some more to make up for the pluses at the root
                xShift += pTree->cxIndent;
            }

            x -= xShift;

            //Get image offsets.  Not all items have an image, so
            //we have to check that as well here.
            cxState = 0;
            cxImage = pTree->cxImage;
            tdd.Mask = 0;
            tdd.StateMask = 0;
            if (cxImage) 
            {
                tdd.Mask |= TDM_IMAGE;
                tdd.hImageList = NULL;
            }
            if (pTree->himlState) 
            {
                tdd.Mask |= TDM_STATE;
                tdd.StateMask |= TDS_STATEIMAGEMASK;
                tdd.State = 0; //in case of failure
            }
            if (tdd.Mask) 
            {
                ptl->GetDisplayData(ListIndex, &tdd);
                cxState = TV_StateIndex(tdd.State) ? pTree->cxState : 0;
                if (cxImage && tdd.hImageList == NULL && tdd.Image == (USHORT)(-1)) 
                {
                    cxImage = 0;
                }
            }

            //iWidth adjusted from regular
            //treeview which caches iWidth for an item.
            iWidth = TV_ListItemStringWidth(pTree, ptl, ListIndex);
            if (x <= (int) (cxImage + cxState)) 
            {
                if (x >= 0) 
                {
                    if (pTree->himlState &&  (x < cxState)) 
                    {
                        *wHitCode = TVHT_ONITEMSTATEICON;
                    } 
                    else if (pTree->hImageList && (x < (int) cxImage + cxState)) 
                    {
                        *wHitCode = TVHT_ONITEMICON;
                    } 
                    else 
                    {
                        *wHitCode = TVHT_ONITEMLABEL;
                    }
                } 
                else 
                {
                    if ((x >= -pTree->cxIndent) && (pTree->ci.style & TVS_HASBUTTONS)) 
                    {
                        BOOL expandable = FALSE;
                        pTree->pVsTree->GetExpandableAbsolute(index, &expandable);
                        *wHitCode = expandable ? TVHT_ONITEMBUTTON : TVHT_ONITEMINDENT;
                    }
                    else 
                    {
                        *wHitCode = TVHT_ONITEMINDENT;
                    }
                }
                if (pfIsStringTruncated) 
                {
                    *pfIsStringTruncated = ((xShift + cxImage + cxState + g_cxEdge + iWidth > pTree->cxWnd) ||
                        (xShift + cxImage + cxState + g_cxEdge < 0));
                }
            }
            else 
            {
                if (x <= (int) (iWidth + cxImage + cxState)) 
                {
                    *wHitCode = TVHT_ONITEMLABEL;
                    if (pfIsStringTruncated) 
                    {
                        *pfIsStringTruncated = ((xShift + cxImage + cxState + g_cxEdge + iWidth > pTree->cxWnd) ||
                            (xShift + cxImage + cxState + g_cxEdge < 0));
                    } 
                }
                else 
                {
                    *wHitCode = TVHT_ONITEMRIGHT;
                    if (pfIsStringTruncated) 
                    {
                        *pfIsStringTruncated = false;
                    } 
                }
            }

            if (lprcItem && (*wHitCode & TVHT_ONITEM)) 
            {
                lprcItem->left = xShift;
                lprcItem->right = (pfIsStringTruncated && *pfIsStringTruncated) ? pTree->cxWnd : xShift + cxImage + cxState + iWidth;
                lprcItem->top = (index - pTree->iTop) * pTree->cyItem;
                lprcItem->bottom = lprcItem->top + pTree->cyItem;
                if (pcxShiftToLabel) 
                {
                    *pcxShiftToLabel = (LONG)(cxState + cxImage);
                }
            }

            if (ptl) 
            {
                ptl->Release();
            }
            if (pLevel) 
            {
                *pLevel = Level;
            }
            return (INDEX) index;*/
        }

        /// <summary>
        ///     Control.OnMouseDown override. Handles tree-specific mouse gestures and events.
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Clicks == 2)
            {
                var doubleClickArgs = new DoubleClickEventArgs(this, e.Button, e.X, e.Y);
                OnDoubleClick(doubleClickArgs);
                if (doubleClickArgs.Handled)
                {
                    SetStateFlag(VTCStateFlags.CallDefWndProc, false);
                }
            }
            else
            {
                var hitInfo = myMouseDownHitInfo = HitInfo(e.X, e.Y);
                // == OnItemButton is correct here. Ignore OnItemButton if OnBlankItem is also set
                if (e.Button == MouseButtons.Left
                    && 0 != (hitInfo.HitTarget & VirtualTreeHitTargets.OnItemButton))
                {
                    if (myTree.IsExpandable(hitInfo.Row, hitInfo.NativeColumn))
                    {
                        myTree.ToggleExpansion(hitInfo.Row, hitInfo.NativeColumn);
                        SetStateFlag(VTCStateFlags.CallDefWndProc, false);
                        return;
                    }
                }

                if (e.Button == MouseButtons.Left
                    && 0 != (hitInfo.HitTarget & VirtualTreeHitTargets.OnItemStateIcon))
                {
                    // the state icon was clicked
                    ToggleAndSynchronizeState(myMouseDownHitInfo.Row, myMouseDownHitInfo.NativeColumn);
                    SetStateFlag(VTCStateFlags.CallDefWndProc, false);
                    return;
                }

                if (0 != (hitInfo.HitTarget & VirtualTreeHitTargets.OnBlankItem)
                    && e.Button != MouseButtons.Right)
                {
                    if (hitInfo.HitTarget != VirtualTreeHitTargets.OnBlankItem)
                    {
                        // This is an anchored blank item, handle it specially.
                        // Hold off on shifting anchor columns until we need to.
                        if (RequireColumnSwitchForSelection(ref hitInfo))
                        {
                            SetSelectionColumn(hitInfo.DisplayColumn, false);
                        }
                        if (GetStyleFlag(VTCStyleFlags.ExtendedMultiSelect))
                        {
                            var isControlPressed = GetStateFlag(VTCStateFlags.MouseButtonDownCtrl);
                            SetCurrentExtendedMultiSelectIndex(
                                hitInfo.Row, GetStateFlag(VTCStateFlags.MouseButtonDownShift), isControlPressed,
                                isControlPressed ? ModifySelectionAction.Toggle : ModifySelectionAction.Select);
                        }
                        else
                        {
                            CurrentIndex = hitInfo.Row;
                        }
                    }
                    SetStateFlag(VTCStateFlags.CallDefWndProc, false);
                    SetStateFlag(VTCStateFlags.StandardLButtonDownProcessing, true);
                }
                    // special handling for right-clicks.  These don't select in a standard list box, we want 
                    // them to select here
                else if (e.Button == MouseButtons.Right)
                {
                    if (myMouseDownHitInfo.Row != -1)
                    {
                        DoSelectionChangeFromMouse(
                            ref hitInfo, GetStateFlag(VTCStateFlags.MouseButtonDownShift), GetStateFlag(VTCStateFlags.MouseButtonDownCtrl),
                            MouseButtons.Right);
                    }
                }
            }

            base.OnMouseDown(e);
        }

        /// <summary>
        ///     Helper routine to determine if the column needs to be switched to
        ///     correctly display the selection of an item.
        /// </summary>
        /// <param name="hitInfo">The hitInfo to test</param>
        /// <returns>true if a column change is required to honor the selection request</returns>
        private bool RequireColumnSwitchForSelection(ref VirtualTreeHitInfo hitInfo)
        {
            var retVal = false;
            var selColumn = mySelectionColumn;
            if (hitInfo.DisplayColumn != selColumn)
            {
                if (GetStyleFlag(VTCStyleFlags.MultiSelect) && ExtendSelectionToAnchors)
                {
                    if (hitInfo.DisplayColumn == hitInfo.RawColumn)
                    {
                        // Single width column, must switch
                        retVal = true;
                    }
                    else if (!((selColumn >= hitInfo.DisplayColumn && selColumn <= hitInfo.RawColumn) ||
                               (selColumn <= hitInfo.DisplayColumn && selColumn >= hitInfo.RawColumn)))
                    {
                        // The selection column is between the hit columns
                        // Note that the only time this does not work is with a blank expansion
                        // that has an offset anchor. However, even in this case, you have to click
                        // on the column, or on opposite sides of the column to make the column switch,
                        // which is still a reasonable behavior.
                        retVal = true;
                    }
                }
                else
                {
                    retVal = true;
                }
            }
            return retVal;
        }

        /// <summary>
        ///     Helper function to toggle the state at the current index
        /// </summary>
        /// <returns>True if the state changed</returns>
        private bool ToggleStateAtCurrentIndex()
        {
            var iCaret = CurrentIndex;
            var retVal = false;
            if (iCaret != VirtualTreeConstant.NullIndex)
            {
                int nativeSourceColumn;
                int sourceColumn;
                ResolveSelectionColumn(iCaret, out sourceColumn, out nativeSourceColumn);
                if (StateRefreshChanges.None != ToggleAndSynchronizeState(iCaret, nativeSourceColumn))
                {
                    retVal = true;
                }
            }
            return retVal;
        }

        /// <summary>
        ///     Helper function to call ToggleState and SynchronizeState as needed
        /// </summary>
        /// <param name="toggleRow">The row to toggle</param>
        /// <param name="toggleColumn">The (native) column to toggle</param>
        /// <returns>The changes from the initial ToggleState call</returns>
        private StateRefreshChanges ToggleAndSynchronizeState(int toggleRow, int toggleColumn)
        {
            var changes = myTree.ToggleState(toggleRow, toggleColumn);
            if ((changes != StateRefreshChanges.None)
                &&
                GetStyleFlag(VTCStyleFlags.MultiSelect)
                &&
                (IsSelected(toggleRow))
                &&
                (SelectedItemCount > 1))
            {
                // Synchronize the selection state for all other toggleable items.
                var iter = CreateSelectedItemEnumerator();
                if (iter != null)
                {
                    var info = myTree.GetItemInfo(toggleRow, toggleColumn, false);
                    myTree.SynchronizeState(iter, info.Branch, info.Row, info.Column);
                }
            }
            return changes;
        }

        /// <summary>
        ///     Control.OnRightToLeftChanged override
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRightToLeftChanged(EventArgs e)
        {
            // Recreate StringFormat
            StringFormat = null;
            if (myColumnPermutation != null)
            {
                myColumnPermutation.PreferLeftBlanks = RightToLeft == RightToLeft.Yes;
                if (GetStateFlag(VTCStateFlags.FixedColumnAutoFilled))
                {
                    // The auto-fill column switches sides, toggle the bit off
                    // so that the property refreshes the fill column.
                    SetStyleFlag(VTCStyleFlags.AutoFillFixedColumns, false);
                    AutoFillFixedColumns = true;
                }
                if (IsHandleCreated)
                {
                    Refresh();
                }
            }
            base.OnRightToLeftChanged(e);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private StringFormat StringFormat
        {
            get
            {
                if (myStringFormat == null)
                {
                    var format = new StringFormat();
                    // Adjust string format for Rtl controls
                    if (RightToLeft == RightToLeft.Yes)
                    {
                        format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                    }
                    // UNDONE: Adding StringFormatFlags.LineLimit here would allow multi
                    // line text to display cleanly, but LineLimit is very risky (you can
                    // end up showing nothing at all), so we leave it up to the caller to
                    // provide single line text. Consider putting this flag in if it can
                    // be verified that it is safe in all fonts (the danger is that a font
                    // can return a height that is too short).
                    format.FormatFlags |= StringFormatFlags.NoWrap;
                    format.LineAlignment = StringAlignment.Center;
                    format.Alignment = StringAlignment.Near;
                    format.Trimming = StringTrimming.None;
                    // Trim using ellipsis if we have multiple columns and no vertical gridlines, to improve readability.
                    if (myMctree != null)
                    {
                        var columns = (myColumnPermutation != null) ? myColumnPermutation.VisibleColumnCount : myMctree.ColumnCount;
                        if (columns > 1
                            && !HasVerticalGridLines)
                        {
                            format.Trimming = StringTrimming.EllipsisCharacter;
                        }
                    }
                    myStringFormat = format;
                }
                return myStringFormat;
            }
            set
            {
                Debug.Assert(value == null);
                if (myStringFormat != null)
                {
                    myStringFormat.Dispose();
                    myStringFormat = null;
                }
            }
        }

        private static StringAlignment GetLabelTextAlignment(ref VirtualTreeDisplayData tdd)
        {
            // TODO: center alignment
            return 0 != (tdd.State & VirtualTreeDisplayStates.TextAlignFar) ? StringAlignment.Far : StringAlignment.Near;
        }

        private void DoDrawItem(DrawItemEventArgs e)
        {
            var focusHWnd = NativeMethods.GetFocus();
            var windowFocused = false;
            var itemFocused = false;
            if (focusHWnd == Handle)
            {
                windowFocused = true;
                itemFocused = 0 != (e.State & DrawItemState.Focus);
            }
            else if (IsDrawWithFocusWindow(focusHWnd))
            {
                windowFocused = true;
                itemFocused = e.Index == CurrentIndex;
            }
            if (myMctree == null
                || e.Index == -1)
            {
                DoDrawItem(e, 0, 0, windowFocused, itemFocused, true, false, true, e.Bounds);
            }
            else
            {
                var bounds = e.Bounds;
                Rectangle columnBounds;
                var drawAnchorExtensions = ExtendSelectionToAnchors;
                if (myColumnPermutation == null)
                {
                    var columns = myMctree.ColumnCount;
                    var columnBound = columns;
                    var expansion = myTree.GetBlankExpansion(e.Index, columnBound - 1, null);
                    if (expansion.Width > 1)
                    {
                        columnBound = expansion.LeftColumn + 1;
                    }
                    var finalColumn = false;
                    for (var column = 0; column < columnBound; ++column)
                    {
                        columnBounds = bounds;
                        finalColumn = (columnBound - column) == 1;
                        LimitRectToColumn(column, ref columnBounds, false, finalColumn ? (columns - 1) : -1, false);
                        DoDrawItem(
                            e, column, column, windowFocused, itemFocused,
                            (finalColumn && drawAnchorExtensions) ? (column <= mySelectionColumn) : (column == mySelectionColumn), false,
                            finalColumn, columnBounds);
                    }
                }
                else
                {
                    var colPerm = myColumnPermutation;
                    var columns = colPerm.VisibleColumnCount;
                    var nextColumn = 0;
                    bool offsetAnchor;
                    while (nextColumn < columns)
                    {
                        var expansion = myTree.GetBlankExpansion(e.Index, nextColumn, colPerm);
                        var anchorColumn = expansion.AnchorColumn;
                        if (anchorColumn != VirtualTreeConstant.NullIndex)
                        {
                            columnBounds = bounds;
                            LimitRectToColumn(anchorColumn, ref columnBounds, false, expansion.RightColumn, false);
                            offsetAnchor = anchorColumn != expansion.LeftColumn;
                            // UNDONE: Do some drawing for the area exposed by the offset anchor. We at least need
                            // to draw the leading gridlines and paint it in the same color as the indent bitmap background.
                            DoDrawItem(
                                e, anchorColumn, colPerm.GetNativeColumn(anchorColumn), windowFocused, itemFocused,
                                drawAnchorExtensions
                                    ? (mySelectionColumn >= expansion.LeftColumn && mySelectionColumn <= expansion.RightColumn)
                                    : (anchorColumn == mySelectionColumn), offsetAnchor, expansion.RightColumn == (columns - 1),
                                columnBounds);
                        }
                        nextColumn = expansion.RightColumn + 1;
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private void DoDrawItem(
            DrawItemEventArgs e, int column, int nativeColumn, bool windowFocused, bool itemFocused, bool columnSelected,
            bool dontDrawLeadingGridline, bool trailingColumn, Rectangle itemBounds)
        {
            Debug.Assert(Redraw && myUpdateCount == 0); // Handled very early in WmReflectDrawItem
            if (e.Index == -1)
            {
                return; // UNDONE: Draw focus rect in empty list
            }

            var drawingEditCell = GetStateFlag(VTCStateFlags.LabelEditActive) && myInPlaceControl != null && e.Index == myEditIndex
                                  && (myMctree == null || columnSelected);
            var drawFullEditCell = drawingEditCell && GetStateFlag(VTCStateFlags.LabelEditTransparent);
            var ignoreSelection = !Enabled || (!columnSelected && (!MultiColumnHighlight || (drawingEditCell && !drawFullEditCell)));
            // When MultiColumnHightlight is set, the focus rect should always be drawn.  This is because there is no other
            // way to indicate the currently focused column.  Otherwise, we respect the system setting to draw the focus rectangle
            var drawFocusRect = columnSelected && itemFocused && (MultiColumnHighlight || (e.State & DrawItemState.NoFocusRect) == 0);

            var info = myTree.GetItemInfo(e.Index, nativeColumn, true);
            var expandable = HasButtons && info.Expandable;
            var expanded = expandable && info.Expanded;
            var selected = !ignoreSelection && SelectionMode != SelectionMode.None
                           && (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            // Setup text font, color, and text
            var itemBackColor = selected ? SelectedItemActiveBackColor : e.BackColor;
            var itemForeColor = selected ? SelectedItemActiveForeColor : e.ForeColor;
            var trailingRectBackColor = itemBackColor; // color used for filling trailing space in the control after the last column
            var controlBackColor = BackColor;
            Brush controlBackBrush = null;
            Brush itemBackBrush = null;
            Brush itemForeBrush = null;
            Brush trailingRectBackBrush = null;

            if (ignoreSelection && (e.State & DrawItemState.Selected) != 0)
            {
                itemBackColor = trailingRectBackColor = controlBackColor;
                itemForeColor = ForeColor;
            }
            // Draw the background of the focused cell with the control's background and foreground colors when
            // MultiColumnHighlight and DistinguishFocusedColumn are set.  This makes distinguishing the focused cell much easier.
            if (drawFocusRect
                && MultiColumnHighlight
                && DistinguishFocusedColumn)
            {
                itemBackColor = controlBackColor; // don't set trailingRectBackColor here
                itemForeColor = ForeColor;
            }
            // Don't draw with a gray brush if selected, as this can obscure the text.
            if (!Enabled
                && !selected)
            {
                itemForeColor = DisabledItemForeColor;
            }

            var branch = info.Branch;
            var text = info.Blank ? null : branch.GetText(info.Row, info.Column);
            if (text == null)
            {
                text = string.Empty;
            }

            //Debug.WriteLine(text + " " + myXPos.ToString() + " " + e.Bounds.Left.ToString());
            //			bool expandable =
            //				HasButtons &&
            //				(0 == (branch.Flags & BranchFeatures.NoExpansion)) && 
            //				branch.IsExpandable(relIndex);
            //			bool expanded = expandable && helper.IsExpanded(e.Index);

            VirtualTreeDisplayData tdd;
            var drawStateImage = false;
            if (info.Blank)
            {
                var expansion = myTree.GetBlankExpansion(e.Index, column, myColumnPermutation);
                var anchorInfo = myTree.GetItemInfo(expansion.TopRow, expansion.AnchorColumn, false);
                tdd = VirtualTreeDisplayData.Empty;
                var tddMasks = new VirtualTreeDisplayDataMasks(
                    VirtualTreeDisplayMasks.Image |
                    VirtualTreeDisplayMasks.ImageOverlays |
                    VirtualTreeDisplayMasks.State |
                    VirtualTreeDisplayMasks.SelectedImage,
                    0);
                if (expanded)
                {
                    tddMasks.StateMask |= VirtualTreeDisplayStates.Expanded;
                }
                if (selected)
                {
                    tddMasks.StateMask |= VirtualTreeDisplayStates.Selected;
                }
                drawStateImage = myStateImageList != null || GetStyleFlag(VTCStyleFlags.StandardCheckBoxes);
                if (drawStateImage)
                {
                    tddMasks.Mask |= VirtualTreeDisplayMasks.StateImage;
                }
                tdd = anchorInfo.Branch.GetDisplayData(anchorInfo.Row, anchorInfo.Column, tddMasks);
            }
            else
            {
                var tddMasks = new VirtualTreeDisplayDataMasks(
                    VirtualTreeDisplayMasks.Image |
                    VirtualTreeDisplayMasks.ImageOverlays |
                    VirtualTreeDisplayMasks.State |
                    VirtualTreeDisplayMasks.SelectedImage |
                    VirtualTreeDisplayMasks.ForceSelect |
                    VirtualTreeDisplayMasks.Color,
                    VirtualTreeDisplayStates.Cut |
                    VirtualTreeDisplayStates.GrayText |
                    VirtualTreeDisplayStates.Bold |
                    VirtualTreeDisplayStates.TextAlignFar);
                if (expanded)
                {
                    tddMasks.StateMask |= VirtualTreeDisplayStates.Expanded;
                }
                if (selected)
                {
                    tddMasks.StateMask |= VirtualTreeDisplayStates.Selected;
                }
                drawStateImage = myStateImageList != null || GetStyleFlag(VTCStyleFlags.StandardCheckBoxes);
                if (drawStateImage)
                {
                    tddMasks.Mask |= VirtualTreeDisplayMasks.StateImage;
                }
                tdd = branch.GetDisplayData(info.Row, info.Column, tddMasks);

                if (tdd.GrayText
                    && !selected)
                {
                    itemForeColor = DisabledItemForeColor;
                }
            }

            if (tdd.ImageList == null)
            {
                if (myImageList != null
                    && tdd.Image != -1)
                {
                    tdd.ImageList = myImageList;
                }
            }
            else if (myImageList == null)
            {
                ImageList = tdd.ImageList;
            }

            var font = (0 == (tdd.State & VirtualTreeDisplayStates.Bold)) ? e.Font : BoldFont;

            // Note that DrawItemEventArgs already contain SystemColors.Highlight and
            // SystemColors.HighlightText if the item is marked as selected. Otherwise,
            // we would switch to those colors here. However, DrawItemEventArgs does
            // not automatically do a 'selected but not focused' color, so we need
            // to do that explicitly at this point.
            if (selected)
            {
                if (!itemFocused
                    && !drawFullEditCell
                    &&
                    // Make sure multiselect items are drawn the correct color when they
                    // don't have the focus.
                    (!windowFocused || SelectionMode == SelectionMode.One)
                    &&
                    // Don't switch to gray if the gray color is the same as the
                    // window background color
                    SystemColors.Control.ToArgb() != SystemColors.Window.ToArgb())
                {
                    itemForeColor = SelectedItemInactiveForeColor;
                    itemBackColor = SelectedItemInactiveBackColor;
                    if (MultiColumnHighlight)
                    {
                        // trailing rect should match the item if we're drawing selection in all columns
                        trailingRectBackColor = itemBackColor;
                    }
                }
            }
            else
            {
                // Selection color overrides back color specified by the branch
                // Since colors are always paired, selection also needs to override the
                // forecolor provided by the branch.
                if (tdd.ForeColor != Color.Empty)
                {
                    itemForeColor = tdd.ForeColor;
                }

                if (tdd.BackColor != Color.Empty)
                {
                    itemBackColor = tdd.BackColor;
                }
            }

            // Highlight the current drop target.  Do this after the block above since we want to use the
            // highlight color in this case even if the window doesn't have focus (follows Windows tree control behavior).
            if (myDropRow != VirtualTreeConstant.NullIndex
                && myDropRow == e.Index
                && GetStateFlag(VTCStateFlags.DragDropHighlight)
                && (myDropColumn == nativeColumn || MultiColumnHighlight))
            {
                selected = true;
                ignoreSelection = false;
                itemBackColor = trailingRectBackColor = SelectedItemActiveBackColor;
                itemForeColor = SelectedItemActiveForeColor;
            }

            var bounds = itemBounds;
            var graphics = e.Graphics;
            var adjustedItemHeight = myItemHeight; // we adjust this value for gridlines later on
            Pen gridPen = null;
            if (HasHorizontalGridLines || HasVerticalGridLines)
            {
                var gridLineColor = GridLinesColor;
                gridPen = new Pen(gridLineColor);
                // each row is responsible for drawing the bottom horizontal gridline only.
                if (HasHorizontalGridLines)
                {
                    if (info.TrailingItem)
                    {
                        graphics.DrawLine(gridPen, bounds.X, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);

                        // draw horizontal gridlines the full width of the control.
                        if (trailingColumn && e.Bounds.Right > bounds.Right)
                        {
                            graphics.DrawLine(gridPen, bounds.Right, bounds.Bottom - 1, e.Bounds.Right, bounds.Bottom - 1);
                        }
                    }
                    --adjustedItemHeight;
                    bounds.Height -= 1;
                }

                // draw vertical gridline, except in the first column, where we never draw the vertical gridline.
                if (HasVerticalGridLines
                    && column > 0
                    && !dontDrawLeadingGridline)
                {
                    graphics.DrawLine(gridPen, bounds.X, bounds.Y - 1, bounds.X, bounds.Bottom - 1);
                    bounds.X += 1;
                    bounds.Width -= 1;
                }
            }
            var remainingWidth = bounds.Width;

            // Move the graphics object to the correct location
            var graphicsContainer = graphics.BeginContainer();
            try
            {
                graphics.TranslateTransform(bounds.Left, bounds.Top);

                //
                // Draw the item
                //

                // Split indent bitmap drawing into a helper function to prevent > 64 locals in one function (FxCop violation)
                var textLeft = 0;
                DrawIndentLines(
                    graphics, itemBackColor, itemForeColor, e.Index, nativeColumn, info, tdd, remainingWidth, adjustedItemHeight,
                    ref textLeft);

                if (textLeft != 0)
                {
                    graphics.TranslateTransform(textLeft, 0);
                    remainingWidth -= textLeft;
                }

                if (drawStateImage
                    && tdd.StateImageIndex >= 0
                    && remainingWidth > 0)
                {
                    ImageList stateImages = null;
                    if (tdd.StateImageList == null)
                    {
                        stateImages = StateImageList; // Force creation if delayed
                    }
                    else if (myStateImageList == null)
                    {
                        stateImages = tdd.StateImageList;
                        StateImageList = stateImages;
                    }

                    // Paint the state image background same as the item background.
                    EnsureBrush(ref itemBackBrush, itemBackColor);
                    graphics.FillRectangle(itemBackBrush, 0, 0, myStateImageWidth, adjustedItemHeight);

                    var xOffset = (int)graphics.Transform.OffsetX;
                    var yOffset = ((int)graphics.Transform.OffsetY) + (adjustedItemHeight - myImageHeight + 1) / 2;

                    int stateImageIndex = tdd.StateImageIndex;
                    if (myMouseOverIndex == e.Index
                        && myMouseOverColumn == nativeColumn
                        && GetStateFlag(VTCStateFlags.StateImageHotTracked)
                        && (stateImageIndex == (int)StandardCheckBoxImage.Unchecked
                            || stateImageIndex == (int)StandardCheckBoxImage.Checked
                            || stateImageIndex == (int)StandardCheckBoxImage.Indeterminate))
                    {
                        stateImageIndex++; // hot-tracked images appear immediately after the regular versions in the image list.
                    }
                    stateImages.Draw(graphics, xOffset, yOffset, stateImageIndex);
                    graphics.TranslateTransform(myStateImageWidth, 0);
                    remainingWidth -= myStateImageWidth;
                }

                if (tdd.ImageList != null
                    && remainingWidth > 0
                    && !info.Blank)
                {
                    if (tdd.Image != -1)
                    {
                        // Paint the item image background same as the item background.
                        EnsureBrush(ref itemBackBrush, itemBackColor);
                        graphics.FillRectangle(itemBackBrush, 0, 0, myImageWidth, adjustedItemHeight);

                        DoDrawImageAndOverlays(graphics, tdd, adjustedItemHeight, selected, controlBackColor);
                    }
                    graphics.TranslateTransform(myImageWidth, 0);
                    remainingWidth -= myImageWidth;
                }

                if (remainingWidth > 0)
                {
                    var textBounds = new Rectangle(0, 0, remainingWidth, bounds.Height);
                    var gridWidth = remainingWidth;
                    if (drawingEditCell && !drawFullEditCell)
                    {
                        // Just blank the rest of the item. This allows us to
                        // not worry about the width of the contents of the label edit
                        // box, which in turn enables custom input in that window.
                        EnsureBrush(ref controlBackBrush, controlBackColor);
                        graphics.FillRectangle(controlBackBrush, textBounds);
                    }
                    else
                    {
                        var border = 1;
                        //int height = Font.Height + 2 * border;

                        // Due to some sort of unpredictable painting optimization in the Windows ListBox control,
                        // we need to always paint the background rectangle for the current line.
                        EnsureBrush(ref itemBackBrush, itemBackColor);
                        EnsureBrush(ref itemForeBrush, itemForeColor);
                        var paintBackground = true;
                        if (FullCellSelect || MultiColumnHighlight)
                        {
                            paintBackground = false;
                            graphics.FillRectangle(itemBackBrush, textBounds);
                        }

                        // Draw the text
                        //
                        var stringBounds = new Rectangle(
                            textBounds.X + 1,
                            textBounds.Y + border,
                            textBounds.Width - 1,
                            textBounds.Height - border * 2);

                        var format = StringFormat;
                        format.Alignment = GetLabelTextAlignment(ref tdd);

                        var drawFocusRectForEmptyItem = false;

                        if (text.Length == 0
                            && itemFocused
                            && columnSelected)
                        {
                            // make sure to draw focus if there is no text for the item, otherwise we
                            // end up with absolutely no visual indication for the current selection
                            drawFocusRect = drawFocusRectForEmptyItem = true;
                        }

                        // Do actual drawing
                        if (paintBackground && (drawFocusRect || selected))
                        {
                            int stringWidth;
                            if (text.Length > 0)
                            {
                                // Add one so potential focus rect doesn't touch the edge of the text
                                stringWidth =
                                    StringRenderer.MeasureString(
                                        UseCompatibleTextRendering, graphics, text, font,
                                        new Rectangle(textBounds.Location, textBounds.Size), format).Width + 1;
                            }
                            else
                            {
                                stringWidth = -1;
                            }

                            Debug.Assert(textBounds.Left == 0);
                            if ((stringWidth + stringBounds.Left) < textBounds.Width)
                            {
                                // Blit the end clean and the part under the string with the background brush
                                var trailingRect = textBounds;
                                trailingRect.X = stringBounds.Left + stringWidth;
                                if (selected)
                                {
                                    EnsureBrush(ref controlBackBrush, controlBackColor);
                                    graphics.FillRectangle(controlBackBrush, trailingRect);
                                    if (text.Length > 0
                                        || (!info.Blank && GetStyleFlag(VTCStyleFlags.MultiSelect)))
                                        // avoid drawing text selection highlight if there's no text
                                    {
                                        if (trailingRect.Left > 0)
                                        {
                                            textBounds.Width = trailingRect.Left;
                                        }
                                        graphics.FillRectangle(itemBackBrush, textBounds);
                                    }
                                }
                                else
                                {
                                    graphics.FillRectangle(itemBackBrush, textBounds);
                                    if (trailingRect.Left > 0)
                                    {
                                        textBounds.Width = trailingRect.Left;
                                    }
                                }
                                paintBackground = false;
                            }
                        }

                        // Default processing for the previous block
                        if (paintBackground)
                        {
                            // Blit the whole thing
                            graphics.FillRectangle(itemBackBrush, textBounds);
                        }

                        // ensure that we use the full item width to bound the text rectangle, to avoid clipping if we're using ellipsis string trimming (VSW 371458).
                        StringRenderer.DrawString(
                            UseCompatibleTextRendering, graphics, text, font, itemForeBrush, itemForeColor,
                            new Rectangle(textBounds.Left, textBounds.Top, remainingWidth, textBounds.Height), format);

                        // Draw the focus rect if required
                        //
                        if (drawFocusRect)
                        {
                            // Don't allow the right edge of the focus rectangle to touch the edge of the client area.  If the control resizes or is scrolled,
                            // and the size of the focus rectangle changes, the right edge may not be invalidated, leaving an artifact.  This problem does not
                            // occur if the header control is present (more of the control is invalidated when header size changes).
                            if (trailingColumn
                                && (myHeaderContainer == null || !myHeaderContainer.Visible)
                                && itemBounds.Right == (ClientSize.Width + myXPos))
                            {
                                textBounds.Width++;
                            }
                            if (selected
                                && MultiColumnHighlight
                                && DistinguishFocusedColumn
                                && e.Index == CaretIndex)
                            {
                                // Draw special focus rectangle for selected, focused cells when MultiColumnHighlight is set.
                                // See VSW 399853.
                                DrawFocusRectangleForMultiColumnHighlight(graphics, textBounds);
                            }
                            else
                            {
                                ControlPaint.DrawFocusRectangle(
                                    graphics, textBounds, itemForeColor, drawFocusRectForEmptyItem ? controlBackColor : itemBackColor);
                            }
                        }
                    }

                    // If we're the last cell in a fixed width column, then there may
                    // be space to the right and we need to draw a trailing vertical gridline
                    if (trailingColumn && e.Bounds.Right > bounds.Right)
                    {
                        if (HasVerticalGridLines)
                        {
                            graphics.DrawLine(gridPen, gridWidth, -1, gridWidth, textBounds.Bottom);
                        }

                        // fill the rest of the row with the appropriate color
                        EnsureBrush(ref trailingRectBackBrush, trailingRectBackColor);
                        graphics.FillRectangle(trailingRectBackBrush, gridWidth + 1, 0, e.Bounds.Right - bounds.Right, bounds.Height);
                    }
                }
            }
            finally
            {
                graphics.EndContainer(graphicsContainer); // Put this back before deferring to base
                CleanBrush(ref controlBackBrush, controlBackColor);
                CleanBrush(ref itemBackBrush, itemBackColor);
                CleanBrush(ref itemForeBrush, itemForeColor);
                CleanBrush(ref trailingRectBackBrush, trailingRectBackColor);
            }
        }

        private Pen focusPen;

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        private Pen MultiColumnHighlightFocusPen
        {
            get
            {
                if (focusPen == null)
                {
                    var foreColor = SystemColors.WindowText;
                    var backColor = SelectedItemActiveBackColor;

                    using (var b = new Bitmap(2, 2))
                    {
                        b.SetPixel(1, 0, backColor);
                        b.SetPixel(0, 1, backColor);
                        b.SetPixel(0, 0, foreColor);
                        b.SetPixel(1, 1, foreColor);

                        using (Brush brush = new TextureBrush(b))
                        {
                            focusPen = new Pen(brush, 1);
                        }
                    }
                }
                return focusPen;
            }
            set
            {
                Debug.Assert(value == null); // Delayed generation in getter
                focusPen = null;
            }
        }

        private void DrawFocusRectangleForMultiColumnHighlight(Graphics graphics, Rectangle rectangle)
        {
            var pen = MultiColumnHighlightFocusPen;

            rectangle.Width--;
            rectangle.Height--;
            // we want the corner to be penned in the WindowText color
            var odds = (rectangle.X + rectangle.Y) % 2 == 1;
            if (odds)
            {
                pen.TranslateTransform(1, 0);
            }
            graphics.DrawRectangle(pen, rectangle);
            if (odds)
            {
                pen.ResetTransform();
            }
        }

        private void DoDrawImageAndOverlays(
            Graphics graphics, VirtualTreeDisplayData tdd, int itemHeight, bool selected, Color controlBackColor)
        {
            var xOffset = (int)graphics.Transform.OffsetX;
            var yOffset = ((int)graphics.Transform.OffsetY) + (itemHeight - myImageHeight) / 2;
            tdd.ImageList.Draw(graphics, xOffset, yOffset, selected ? tdd.SelectedImage : tdd.Image);
            int overlayIndex = tdd.OverlayIndex;
            var overlayIndices = tdd.OverlayIndices;
            if (overlayIndex != -1)
            {
                if (overlayIndices == null)
                {
                    // Use the index directly
                    tdd.ImageList.Draw(graphics, xOffset, yOffset, overlayIndex);
                }
                else if (overlayIndex != 0)
                {
                    // Use the index as a list of bits into the indices array.
                    // Do this typed if possible. It returns an IList to be CLS
                    // compliant.
                    int[] indicesArray;
                    IList<int> typedList;
                    int indicesCount;
                    int curIndex;
                    int curBit;
                    indicesCount = overlayIndices.Count;
                    curBit = 1 << (indicesCount - 1);
                    if (null != (indicesArray = overlayIndices as int[]))
                    {
                        for (curIndex = indicesCount - 1; curIndex >= 0; --curIndex)
                        {
                            if (0 != (curBit & overlayIndex))
                            {
                                tdd.ImageList.Draw(graphics, xOffset, yOffset, indicesArray[curIndex]);
                            }
                            curBit >>= 1;
                        }
                    }
                    else if (null != (typedList = overlayIndices as IList<int>))
                    {
                        for (curIndex = indicesCount - 1; curIndex >= 0; --curIndex)
                        {
                            if (0 != (curBit & overlayIndex))
                            {
                                tdd.ImageList.Draw(graphics, xOffset, yOffset, typedList[curIndex]);
                            }
                            curBit >>= 1;
                        }
                    }
                    else
                    {
                        for (curIndex = indicesCount - 1; curIndex >= 0; --curIndex)
                        {
                            if (0 != (curBit & overlayIndex))
                            {
                                tdd.ImageList.Draw(graphics, xOffset, yOffset, (int)overlayIndices[curIndex]);
                            }
                            curBit >>= 1;
                        }
                    }
                }
            }
            if (tdd.Cut)
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(128, controlBackColor)))
                {
                    graphics.FillRectangle(brush, 0, 0, myImageWidth + 1, myImageHeight + 1);
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void DrawIndentLines(
            Graphics graphics, Color backColor, Color foreColor, int absIndex, int nativeColumn, VirtualTreeItemInfo info,
            VirtualTreeDisplayData tdd, int remainingWidth, int adjustedItemHeight, ref int textLeft)
        {
            var level = info.Level;
            var expandable = HasButtons && info.Expandable;
            var expanded = expandable && info.Expanded;

            Brush brush = null;
            try
            {
                // Fill the background with the given backColor.
                EnsureBrush(ref brush, backColor);

                if (0 == (myStyleFlags & VTCStyleFlags.MaskHasIndentBitmaps))
                {
                    if (level > 0)
                    {
                        textLeft = level * myIndentWidth;
                        graphics.FillRectangle(brush, 0, 0, textLeft, adjustedItemHeight);
                    }
                }
                else
                {
                    int iRow;
                    var iColumn = 0;
                    if (!HasLines) // HasLines and HasRootLines are synchronized, only HasButtons (and possibly HasRootButtons) is set
                    {
                        if (HasRootButtons && (nativeColumn == 0 || !info.SimpleCell))
                        {
                            ++level;
                        }
                        if (level > 0)
                        {
                            textLeft = level * myIndentWidth;
                            if (info.Blank)
                            {
                                if (tdd.Image != -1)
                                {
                                    textLeft += myImageWidth;
                                }
                                if (tdd.StateImageIndex != -1)
                                {
                                    textLeft += myImageWidth;
                                }
                            }
                            // We're just looking at buttons
                            if (expandable)
                            {
                                --level;
                                iColumn = 0;
                                iRow = expanded ? 1 : 0;
                                var destRect = new Rectangle(textLeft - myIndentWidth, 0, myIndentWidth, adjustedItemHeight);
                                if (destRect.Left < remainingWidth)
                                {
                                    // Ensure indent image doesn't draw outside bounding rectangle for this column
                                    if (destRect.Right > remainingWidth)
                                    {
                                        destRect.Width = remainingWidth - destRect.X;
                                    }
                                    graphics.DrawImage(
                                        GetIndentBitmap(backColor, foreColor), destRect, 0, iRow * myItemHeight, destRect.Width,
                                        adjustedItemHeight, GraphicsUnit.Pixel);
                                }
                            }
                        }
                        if (level > 0)
                        {
                            graphics.FillRectangle(brush, 0, 0, level * myIndentWidth, adjustedItemHeight);
                        }
                    }
                    else
                    {
                        // Determine the row to blit based on expansion state
                        // The rows are laid out {no expansion, plus, minus}
                        // The columns are laid out {level bmp, middle, bottom, top, single}
                        iRow = expandable ? (expanded ? 2 : 1) : 0;

                        // Determine the column to blit based on the position in the list
                        //						if (relIndex == (branch.ItemCount - 1))
                        if (info.LastBranchItem)
                        {
                            // Last item
                            //							if (HasRootLines && (branch.ItemCount == 1))
                            //							if (info.Blank)
                            //							{
                            //								skipGlyph = true;
                            //								levelAdjust = 1;
                            //							}
                            //							else
                            if (info.FirstBranchItem)
                            {
                                iColumn = (level == 0) ? 4 : 2;
                            }
                            else
                            {
                                iColumn = 2;
                            }
                        }
                            //						else if (info.Blank)
                            //						{
                            //							levelAdjust = 1;
                            //							if (info.Expanded)
                            //							{
                            //								iColumn = 0;
                            //								iRow = 0;
                            //							}
                            //							else
                            //							{
                            //								skipGlyph = true;
                            //							}
                            //						}
                        else
                        {
                            //							if ((relIndex == 0) && HasRootLines)
                            if (info.FirstBranchItem
                                && HasRootLines
                                && (nativeColumn == 0 || !info.SimpleCell))
                            {
                                iColumn = (level == 0) ? 3 : 1;
                            }
                            else
                            {
                                iColumn = 1;
                            }
                        }

                        var iStartColumn = 0;
                        if ((absIndex & 1) == 1
                            && (myItemHeight & 1) == 1)
                        {
                            // in the odd item height case, use different bitmaps in alternating rows to keep the gridlines looking good.
                            // see comment in CreateIndentBmp() for more info.
                            iStartColumn = (HasRootLines || HasRootButtons) ? 5 : 3;
                        }
                        iColumn += iStartColumn;

                        // Blit in the level bitmaps. This is trickier than it looks because
                        // a check is required at each level to see if the parent root lines
                        // are required.
                        if (GetAnyStyleFlag(VTCStyleFlags.HasRootLines | VTCStyleFlags.HasRootButtons)
                            && (nativeColumn == 0 || !info.SimpleCell))
                        {
                            ++level;
                        }

                        if (level > 0)
                        {
                            var bmp = GetIndentBitmap(backColor, foreColor);
                            var cxSrc = myIndentWidth;
                            var cySrc = myItemHeight;
                            textLeft = level * cxSrc;
                            var skipGlyph = false;
                            var destRect = new Rectangle((level - 1) * cxSrc, 0, cxSrc, cySrc);

                            if (info.Blank)
                            {
                                if (tdd.Image != -1)
                                {
                                    textLeft += myImageWidth;
                                }
                                if (tdd.StateImageIndex != -1)
                                {
                                    textLeft += myImageWidth;
                                }
                                iColumn = iStartColumn;
                                iRow = 0;
                                if (info.LastBranchItem)
                                {
                                    skipGlyph = true;
                                }
                                if (info.Expanded)
                                {
                                    destRect.Offset(cxSrc, 0);
                                    if (destRect.Left < remainingWidth)
                                    {
                                        // Ensure indent image doesn't draw outside bounding rectangle for this column.
                                        if (destRect.Right <= remainingWidth)
                                        {
                                            graphics.DrawImage(bmp, destRect, iColumn * cxSrc, 0, cxSrc, cySrc, GraphicsUnit.Pixel);
                                        }
                                        else
                                        {
                                            graphics.DrawImage(
                                                bmp, new Rectangle(destRect.X, destRect.Y, remainingWidth - destRect.X, destRect.Height),
                                                iColumn * cxSrc, 0, remainingWidth - destRect.X, cySrc, GraphicsUnit.Pixel);
                                        }
                                    }
                                    destRect.Offset(-cxSrc, 0);
                                }
                            }

                            var xSrc = iColumn * cxSrc;
                            var ySrc = iRow * myItemHeight; //cySrc;

                            if (!skipGlyph
                                && destRect.Left < remainingWidth)
                            {
                                // Do the initial glyph.  Ensure indent image doesn't draw outside bounding rectangle for this column.
                                if (destRect.Right <= remainingWidth)
                                {
                                    graphics.DrawImage(bmp, destRect, xSrc, ySrc, cxSrc, cySrc, GraphicsUnit.Pixel);
                                }
                                else
                                {
                                    graphics.DrawImage(
                                        bmp, new Rectangle(destRect.X, destRect.Y, remainingWidth - destRect.X, destRect.Height), xSrc, ySrc,
                                        remainingWidth - destRect.X, cySrc, GraphicsUnit.Pixel);
                                }
                            }

                            if (level > 1)
                            {
                                // Move back to the parent glyph, reset cySrc and destRect to non-adjusted item height.
                                xSrc = iStartColumn * cxSrc;
                                cySrc = destRect.Height = myItemHeight;

                                //								int parentRelIndex;
                                //								int dummyLevel;
                                //								IBranch parentBranch;
                                var parentIndex = absIndex;
                                VirtualTreeItemInfo parentInfo;
                                while (--level > 0)
                                {
                                    destRect.Offset(-cxSrc, 0);
                                    parentIndex = myTree.GetParentIndex(parentIndex, nativeColumn);

                                    if (destRect.Left < remainingWidth)
                                    {
                                        parentInfo = myTree.GetItemInfo(parentIndex, nativeColumn, true);
                                        //									if ((parentRelIndex + 1) == parentBranch.ItemCount)
                                        if (parentInfo.LastBranchItem)
                                        {
                                            graphics.FillRectangle(brush, destRect.X, destRect.Y, cxSrc, adjustedItemHeight);
                                                // fill adjusted rectangle so we don't paint over gridlines.
                                        }
                                        else
                                        {
                                            // Ensure indent image doesn't draw outside bounding rectangle for this column.
                                            if (destRect.Right <= remainingWidth)
                                            {
                                                graphics.DrawImage(bmp, destRect, xSrc, ySrc, cxSrc, cySrc, GraphicsUnit.Pixel);
                                            }
                                            else
                                            {
                                                graphics.DrawImage(
                                                    bmp, new Rectangle(destRect.X, destRect.Y, remainingWidth - destRect.X, destRect.Height),
                                                    xSrc, ySrc, remainingWidth - destRect.X, cySrc, GraphicsUnit.Pixel);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                CleanBrush(ref brush, backColor);
            }
        }

        // Helper functions for managing brushes and pens
        private static void EnsureBrush(ref Brush brush, Color color)
        {
            if (brush == null)
            {
                brush = color.IsSystemColor
                            ? SystemBrushes.FromSystemColor(color)
                            : new SolidBrush(color);
            }
        }

        private static void CleanBrush(ref Brush brush, Color color)
        {
            if (brush != null)
            {
                if (!color.IsSystemColor)
                {
                    brush.Dispose();
                    brush = null;
                }
            }
        }

        private static void EnsurePen(ref Pen pen, Color color)
        {
            if (pen == null)
            {
                pen = color.IsSystemColor
                          ? SystemPens.FromSystemColor(color)
                          : new Pen(color);
            }
        }

        private static void CleanPen(ref Pen pen, Color color)
        {
            if (pen != null)
            {
                if (!color.IsSystemColor)
                {
                    pen.Dispose();
                    pen = null;
                }
            }
        }

        #endregion //Virtual Tree Integration Functions

        #region Scrolling Functions

#if SCROLLING_FINISHED
        private void ScrollItems(int itemCount, int topShownIndex, bool scrollDown)
        {
            int dx = (scrollDown ? 1 : -1) * itemCount * myItemHeight;
            SmoothScroll.Info ssi = new SmoothScroll.Info(Handle, 0, (scrollDown ? 1 : -1) * itemCount * myItemHeight);
            ssi.clipRect = ssi.srcRect = new Rectangle(new Point(0, (topShownIndex + 1) * myItemHeight), ClientSize);
            ssi.scrollWindowFlags = NativeMethods.ScrollWindowFlags.Erase | NativeMethods.ScrollWindowFlags.Invalidate;
            SmoothScroll.ScrollWindow(ref ssi);
            UpdateToolTip();
        }
        private int ScrollBelow(int index, bool redrawParent, int visibleDescendantsCount, bool scrollDown)
        {
            int retVal = 0;
            if (Redraw)
            {
                ScrollItems(visibleDescendantsCount, index - TopIndex, scrollDown);
                InvalidateItem(index, NativeMethods.RedrawWindowFlags.Invalidate);
                if (redrawParent)
                {
                    retVal = visibleDescendantsCount;
                }
            }
            else if (redrawParent)
            {
                retVal = visibleDescendantsCount;
            }
            return retVal;
        }
#endif
        // SCROLLING_FINISHED
        private int ListItemStringWidth(Graphics graphics, ref VirtualTreeItemInfo info, out int imageWidth)
        {
            return ListItemStringWidth(graphics, ref info, null, out imageWidth);
        }

        private int ListItemStringWidth(Graphics graphics, ref VirtualTreeItemInfo info, string alternateText, out int imageWidth)
        {
            imageWidth = 0;
            var tddMasks = new VirtualTreeDisplayDataMasks(VirtualTreeDisplayMasks.State, VirtualTreeDisplayStates.Bold);
            var fCheckImage = myImageWidth > 0;
            var fCheckState = myStateImageWidth > 0;
            if (fCheckImage)
            {
                tddMasks.Mask |= VirtualTreeDisplayMasks.Image;
            }
            if (fCheckState)
            {
                tddMasks.Mask |= VirtualTreeDisplayMasks.StateImage;
            }
            var tdd = info.Branch.GetDisplayData(info.Row, info.Column, tddMasks);
            var font = (0 == (tdd.State & VirtualTreeDisplayStates.Bold)) ? Font : BoldFont;

            int retVal;
            if (alternateText == null)
            {
                retVal = ListItemStringWidth(
                    graphics,
                    font,
                    ref info);
            }
            else
            {
                retVal = ListItemStringWidth(
                    graphics,
                    font,
                    alternateText);
            }
            if (fCheckImage && tdd.Image != -1)
            {
                imageWidth += myImageWidth;
            }
            if (fCheckState && tdd.StateImageIndex >= 0)
            {
                imageWidth += myStateImageWidth;
            }
            return retVal;
        }

        private int ListItemStringWidth(ref VirtualTreeItemInfo info, string alternateText, out int imageWidth)
        {
            using (var g = CreateGraphics())
            {
                return ListItemStringWidth(
                    g,
                    ref info,
                    alternateText,
                    out imageWidth);
            }
        }

        /// <summary>
        ///     Use this override if the font has already been determined via GetDisplayData
        /// </summary>
        /// <param name="font"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private int ListItemStringWidth(Font font, ref VirtualTreeItemInfo info)
        {
            using (var g = CreateGraphics())
            {
                return ListItemStringWidth(
                    g,
                    font,
                    ref info);
            }
        }

        private int ListItemStringWidth(Graphics graphics, Font font, ref VirtualTreeItemInfo info)
        {
            return ListItemStringWidth(graphics, font, info.Branch.GetText(info.Row, info.Column));
        }

        private int ListItemStringWidth(Graphics graphics, Font font, string itemText)
        {
            // Add one to remain backwards compatible
            var stringWidth = StringRenderer.MeasureString(UseCompatibleTextRendering, graphics, itemText, font, StringFormat).Width + 1;

            return stringWidth + (2 * SystemInformation.Border3DSize.Width);
        }

        // Helper function
        private bool UpdateMaxWidth(int newWidth, int newFirstMax, int newLastMax, bool alwaysResetMax)
        {
            var retVal = false;
            if (alwaysResetMax || newWidth > myMaxItemWidth)
            {
                myMaxItemWidth = newWidth;
                myFirstMaxIndex = newFirstMax;
                myLastMaxIndex = newLastMax;
                retVal = true;
            }
            else if (newWidth == myMaxItemWidth)
            {
                if (myFirstMaxIndex == VirtualTreeConstant.NullIndex)
                {
                    myFirstMaxIndex = newFirstMax;
                    myLastMaxIndex = newLastMax;
                }
                else
                {
                    myFirstMaxIndex = Math.Min(myFirstMaxIndex, newFirstMax);
                    myLastMaxIndex = Math.Max(myLastMaxIndex, newLastMax);
                }
            }
            return retVal;
        }

        //  If the expanded items are showing, update the shown (expanded) count,
        //  the max item width -- then recompute the scroll bars.
        //
        //  sets myMaxWidth
        private void ScrollBarsAfterExpand(int parentIndex, int change)
        {
            if (change == 0
                || !Redraw)
            {
                return;
            }
            var iTop = TopIndex;
            var iLastVisible = Math.Min(iTop + myPartlyVisibleCountIgnoreHScroll, myTree.VisibleItemCount) - 1;
            int iFirstToCheck, iLastToCheck;

            //myFirstMaxIndex and myLastMaxIndex needed to be adjusted.  However, there is
            //no reason to recalculate the entire visible width in most circumstances.
            Debug.Assert(myLastMaxIndex != VirtualTreeConstant.NullIndex || myFirstMaxIndex == VirtualTreeConstant.NullIndex);
                //Setting one is sufficient
            if (myFirstMaxIndex == VirtualTreeConstant.NullIndex)
            {
                iFirstToCheck = iTop;
                iLastToCheck = iLastVisible;
            }
            else if (parentIndex >= myLastMaxIndex)
            {
                //Doesn't touch maxes, just calculate the visible area
                iFirstToCheck = Math.Min(parentIndex + 1, iLastVisible);
                iLastToCheck = Math.Min(parentIndex + change, iLastVisible);
            }
            else
            {
                //The expansion may push myLastMaxIndex and/or myFirstMaxIndex
                //off the end of the visible area.  If this happens, then move 
                //up the max visible to the last visible item
                iFirstToCheck = parentIndex + 1;
                iLastToCheck = Math.Min(parentIndex + change, iLastVisible);
                if (parentIndex < myFirstMaxIndex
                    && (myFirstMaxIndex += change) > iLastVisible)
                {
                    myFirstMaxIndex = VirtualTreeConstant.NullIndex;
                    iFirstToCheck = iTop;
                    iLastToCheck = iLastVisible;
                }
                else if ((myLastMaxIndex += change) > iLastVisible)
                {
                    myLastMaxIndex = iLastVisible;
                }
            }
            //This happens fairly often and I don't know why. Investigate. For now, get by with swapping the two
            //VSASSERT(iLastToCheck+1>=iFirstToCheck, "Invalid first to last range of visible items! Please report bug with a consisent repro.");
            if (iLastToCheck + 1 < iFirstToCheck)
            {
                // swap the two
                var iTemp = iLastToCheck;
                iLastToCheck = iFirstToCheck;
                iFirstToCheck = iTemp;
            }
            int iNewFirstMax, iNewLastMax;
            var cxMax = ComputeWidthOfRange(iFirstToCheck, iLastToCheck - iFirstToCheck + 1, out iNewFirstMax, out iNewLastMax);
            UpdateMaxWidth(cxMax, iNewFirstMax, iNewLastMax, false);

            CalcScrollBars();
        }

        private bool HasVerticalScrollBar
        {
            get { return 0 != ((int)NativeMethods.GetWindowStyle(Handle) & NativeMethods.WS_VSCROLL); }
        }

        private bool HasHorizontalScrollBar
        {
            get { return 0 != ((int)NativeMethods.GetWindowStyle(Handle) & NativeMethods.WS_HSCROLL); }
        }

        //  If the collapsed items were showing, update the shown (expanded) count,
        //  the max item width -- then recompute the scroll bars.
        //
        //  sets myMaxWidth
        private void ScrollBarsAfterCollapse(int parentIndex, int change)
        {
            if (change == 0
                || !Redraw)
            {
                return;
            }

            int iLastVisible;
            int iFirstToCheck, iLastToCheck;
            var iTop = TopIndex;
            var cShowing = myTree.VisibleItemCount;
            var fAlwaysResetMax = false;

            iLastVisible = Math.Min(iTop + myPartlyVisibleCountIgnoreHScroll, cShowing) - 1;

            //myFirstMaxIndex and myLastMaxIndex needed to be adjusted.  However, there is
            //no reason to recalculate the entire visible width in most circumstances.
            Debug.Assert(myLastMaxIndex != VirtualTreeConstant.NullIndex || myFirstMaxIndex == VirtualTreeConstant.NullIndex);
                //"Setting one is sufficient");

            //Just to be safe in case we missed a case, fill in defaults here
            iFirstToCheck = iTop;
            iLastToCheck = iLastVisible;
            if (HasVerticalScrollBar
                && iTop > 0
                && iTop + myPartlyVisibleCountIgnoreHScroll > cShowing)
            {
                //We're at the end of the list, so the whole list will potentially shift down.  This would actually
                //require calculating two different ranges, the one involved in the collapse and the one being brought
                //in from above the top item.  It isn't worth the double calculation, so skip optimizations and just
                //look at all visible items. If the vertical scrollbar isn't showing, then, since the list is reducing,
                //there is no reason to check this case.
                iFirstToCheck = Math.Max(cShowing - myPartlyVisibleCountIgnoreHScroll + 1, 0);
                fAlwaysResetMax = true;
            }
            else if (myFirstMaxIndex == VirtualTreeConstant.NullIndex
                     || (iTop + myPartlyVisibleCountIgnoreHScroll > cShowing))
            {
                fAlwaysResetMax = true;
            }
            else if (parentIndex >= myLastMaxIndex)
            {
                //Collapsed below cached max range;
                iFirstToCheck = Math.Min(parentIndex + 1, iLastVisible);
            }
            else if (myFirstMaxIndex > parentIndex + change)
            {
                //Collapsed above cached max range
                if ((myFirstMaxIndex -= change) < iTop)
                {
                    if ((myLastMaxIndex -= change) < iTop)
                    {
                        myFirstMaxIndex = VirtualTreeConstant.NullIndex;
                        fAlwaysResetMax = true;
                    }
                    else
                    {
                        myFirstMaxIndex = iTop;
                        iFirstToCheck = Math.Min(myLastMaxIndex + 1, iLastVisible);
                    }
                }
                else
                {
                    myLastMaxIndex -= change;
                    iFirstToCheck = Math.Min(myLastMaxIndex + 1, iLastVisible);
                }
            }
            else if (myFirstMaxIndex > parentIndex)
            {
                //First max in collapsed branch
                if (myLastMaxIndex > parentIndex + change)
                {
                    //Last out of collapsed branch
                    myLastMaxIndex -= change;
                    myFirstMaxIndex = parentIndex + change;
                    iFirstToCheck = Math.Min(myLastMaxIndex + 1, iLastVisible);
                }
                else
                {
                    //Both collapsed
                    fAlwaysResetMax = true;
                }
            }
            else if (parentIndex + change >= myLastMaxIndex)
            {
                //UNDONE: Case not in ltscroll.cpp, report bug
                iFirstToCheck = Math.Min(myFirstMaxIndex + 1, iLastVisible);
                myLastMaxIndex = myFirstMaxIndex;
            }
            else if (parentIndex + change < myLastMaxIndex)
            {
                iFirstToCheck = Math.Min(myLastMaxIndex + 1, iLastVisible);
            }
            else if (myLastMaxIndex > iLastVisible)
            {
                iFirstToCheck = Math.Min(myFirstMaxIndex + 1, iLastVisible);
            }
            else
            {
                Debug.Assert(myFirstMaxIndex <= parentIndex && myLastMaxIndex <= parentIndex && myLastMaxIndex <= parentIndex + change);
                    //"Missed a case");
                //Last max is contained in list, but first is before
                myLastMaxIndex = parentIndex;
                iFirstToCheck = Math.Min(parentIndex + 1, iLastVisible);
            }

            //this happens fairly often and I don't know why. Investigate. For now, get by with swapping the two
            //VSASSERT(iLastToCheck+1>=iFirstToCheck, "Invalid first to last range of visible items! Please report bug with a consisent repro.");
            if (iLastToCheck + 1 < iFirstToCheck)
            {
                // swap the two
                var iTemp = iLastToCheck;
                iLastToCheck = iFirstToCheck;
                iFirstToCheck = iTemp;
            }
            int iNewFirstMax, iNewLastMax;
            var cxMax = ComputeWidthOfRange(iFirstToCheck, iLastToCheck - iFirstToCheck + 1, out iNewFirstMax, out iNewLastMax);
            UpdateMaxWidth(cxMax, iNewFirstMax, iNewLastMax, fAlwaysResetMax);

            CalcScrollBars();
        }

        //  If a single item's width changed, alter the max width if needed.
        //  If all widths changed, recompute widths and max width.
        //  Then recompute the scroll bars.
        //
        //  sets myMaxWidth
        private void ScrollBarsAfterSetWidth(int absIndex)
        {
            var fRecalc = false;
            int iMax;
            var iTop = TopIndex;
            var cShowing = ItemCount;
            if (absIndex != VirtualTreeConstant.NullIndex)
            {
                int iWidth;
                if (absIndex > iTop
                    && absIndex < iTop + myPartlyVisibleCountIgnoreHScroll)
                {
                    iWidth = ComputeWidthOfRange(absIndex, 1, out iMax, out iMax);
                    fRecalc = UpdateMaxWidth(iWidth, iMax, iMax, false);
                }
            }
            else if (ItemCount != 0)
            {
                int iNewFirstMax, iNewLastMax;
                var cxMax = ComputeWidthOfRange(
                    iTop,
                    Math.Min(iTop + myPartlyVisibleCountIgnoreHScroll, cShowing) - iTop,
                    out iNewFirstMax,
                    out iNewLastMax);
                fRecalc = UpdateMaxWidth(cxMax, iNewFirstMax, iNewLastMax, true);
            }
            else
            {
                iMax = VirtualTreeConstant.NullIndex;
                fRecalc = UpdateMaxWidth(0, iMax, iMax, true);
            }

            if (fRecalc)
            {
                CalcScrollBars();
            }
        }

        // ----------------------------------------------------------------------------
        //  Scroll window vertically as needed to make given item fully visible
        //  vertically
        // ----------------------------------------------------------------------------
        private bool ScrollVertIntoView(int absIndex)
        {
            // Do nothing if this item is not visible
            var iTop = TopIndex;
            if (absIndex < iTop)
            {
                // return SetTopItem(absIndex);
                TopIndex = absIndex;
                return true;
            }

            if (absIndex >= (iTop + myFullyVisibleCount))
            {
                //deferVerifyInView is true to make sure items move above dynamic hscroll
                // return SmoothSetTopItem(absIndex + 1 - myFullyVisibleCount, 0, false, false, true, MouseShiftStyle.None);
                // For the purposes of this calculation, assume at least one item fully visible.  Otherwise, it may go out of bounds.
                TopIndex = absIndex + 1 - (Math.Max(myFullyVisibleCount, 1));
                return true;
            }

            return false;
        }

        private bool ScrollIntoView(int absRow, int column)
        {
            return ScrollVertIntoView(absRow) || ScrollColumnIntoView(column);
        }

        private bool ScrollColumnIntoView(int column)
        {
            if (myTree != null
                && myTree.VisibleItemCount > 0)
            {
                // ensure that item is fully visible horizontally
                var xPos = myXPos;
                var itemRect = GetItemRectangle(0, column, false /* excludeIndent */, false /* textOnly */, null);
                var clientWidth = ClientSize.Width;
                if (itemRect.Width > clientWidth)
                {
                    itemRect.Width = clientWidth;
                }

                if (itemRect.Left < 0)
                {
                    return SetLeft(xPos + itemRect.Left);
                }
                else if (itemRect.Right > clientWidth)
                {
                    return SetLeft(xPos + itemRect.Right - clientWidth);
                }
            }
            return false;
        }

        //  Sets position of horizontal scroll bar and scrolls window to match that position
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.SetScrollPos(System.IntPtr,Microsoft.Data.Entity.Design.VisualStudio.NativeMethods+ScrollBarType,System.Int32,System.Boolean)")]
        private bool SetLeft(int x)
        {
            if (!HasHorizontalScrollBar)
            {
                return false;
            }

            var xPos = myXPos;
            if (x > xPos)
            {
                var hExtent = NativeMethods.SendMessage(Handle, NativeMethods.LB_GETHORIZONTALEXTENT, 0, 0).ToInt32();
                if (x > hExtent)
                {
                    x = hExtent; // don't allow scrolling past the current extent.
                }
            }

            if (x < 0)
            {
                x = 0;
            }

            if (x == xPos)
            {
                return false;
            }

            var dismissEdit = GetStateFlag(VTCStateFlags.NoDismissEdit); // enusre we don't dismiss the edit while scrolling into position
            try
            {
                SetStateFlag(VTCStateFlags.NoDismissEdit, true);
                NativeMethods.SendMessage(
                    Handle, NativeMethods.WM_HSCROLL, NativeMethods.MAKELONG((int)NativeMethods.ScrollAction.ThumbPosition, x), 0);
                NativeMethods.SendMessage(Handle, NativeMethods.WM_HSCROLL, (int)NativeMethods.ScrollAction.EndScroll, 0);
            }
            finally
            {
                SetStateFlag(VTCStateFlags.NoDismissEdit, dismissEdit);
            }

            NativeMethods.SetScrollPos(Handle, NativeMethods.ScrollBarType.Horizontal, x, true);
            UpdateToolTip();

            return true;
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.GetWindowRect(System.IntPtr,Microsoft.Data.Entity.Design.VisualStudio.NativeMethods+RECT@)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.SetScrollRange(System.IntPtr,Microsoft.Data.Entity.Design.VisualStudio.NativeMethods+ScrollBarType,System.Int32,System.Int32,System.Boolean)")]
        private void VScrollCompleted()
        {
            //Check the horizontal scrollbar.  Note that this differs
            //from the original tree sources, which cache the width for
            //every item.  With the virtual list, there is nowhere available
            //to do this, and constantly recalcing width for masses of non-visible
            //items is way too expensive.  The trade off is that we have to retest here.
            var cShowing = ItemCount;
            var newTopIndex = TopIndex;
            var oldTopIndex = myTopStartScroll;
            int iFirstToCheck, iLastToCheck;
            var iLastVisible = Math.Min(newTopIndex + myPartlyVisibleCountIgnoreHScroll, cShowing) - 1;
            var newTopIndexAdjusted = newTopIndex;
            var myXPos = 0;
            var hWnd = Handle;
            if (HasHorizontalScrollBar)
            {
                myXPos = NativeMethods.GetScrollPos(hWnd, NativeMethods.ScrollBarType.Horizontal);
            }
            var mouseShift = MouseShiftStyle.None;
            if (newTopIndex > 0
                && (newTopIndex + myPartlyVisibleCountIgnoreHScroll) > cShowing)
            {
                //We're at the end of the list, so the whole list will potentially shift down.  We need to
                //calculate based on the items which will be visible after the list shifts, not the ones which
                //are currently showing.
                newTopIndexAdjusted = Math.Max(cShowing - myPartlyVisibleCountIgnoreHScroll + 1, 0);
            }
            var prevMaxItemWidth = myMaxItemWidth;
            var fForceUpdate = false;
            //Just to be safe in case we missed a case, fill in defaults here
            iLastToCheck = iLastVisible;
            iFirstToCheck = newTopIndexAdjusted;
            if (myFirstMaxIndex == VirtualTreeConstant.NullIndex
                || Math.Abs(newTopIndexAdjusted - oldTopIndex) >= myPartlyVisibleCountIgnoreHScroll)
            {
                myFirstMaxIndex = VirtualTreeConstant.NullIndex;
                fForceUpdate = true;
            }
            else if (newTopIndex > oldTopIndex)
            {
                //Scrolled up
                if (myLastMaxIndex < newTopIndexAdjusted)
                {
                    myFirstMaxIndex = VirtualTreeConstant.NullIndex;
                    fForceUpdate = true;
                }
                else if (myFirstMaxIndex < newTopIndexAdjusted)
                {
                    myFirstMaxIndex = newTopIndexAdjusted;
                    iFirstToCheck = Math.Min(
                        iLastVisible,
                        Math.Min(iLastVisible + newTopIndexAdjusted - oldTopIndex, myLastMaxIndex + 1));
                }
                else
                {
                    //Just check the end of the list
                    iFirstToCheck = iLastVisible + newTopIndexAdjusted - oldTopIndex - 1;
                }
            }
            else
            {
                //Scrolled down
                if (myFirstMaxIndex > iLastVisible)
                {
                    myFirstMaxIndex = VirtualTreeConstant.NullIndex;
                    fForceUpdate = true;
                }
                else if (myLastMaxIndex > iLastVisible)
                {
                    myLastMaxIndex = iLastVisible;
                    iLastToCheck = myFirstMaxIndex - 1;
                }
                else
                {
                    //Just check the beginning of the list
                    iLastToCheck = oldTopIndex - 1;
                }
            }
            Debug.Assert(iFirstToCheck >= newTopIndexAdjusted && iLastToCheck <= iLastVisible); //Missed a case
            //This happens fairly often and I don't know why. Investigate. For now, get by with swapping the two
            //VSASSERT(iLastToCheck+1>=iFirstToCheck, "Invalid first to last range of visible items! Please report bug with a consisent repro.");
            if (iLastToCheck + 1 < iFirstToCheck)
            {
                // swap the two
                var iTemp = iLastToCheck;
                iLastToCheck = iFirstToCheck;
                iFirstToCheck = iTemp;
            }
            int iNewFirstMax, iNewLastMax;
            var iNewWidth = ComputeWidthOfRange(iFirstToCheck, iLastToCheck - iFirstToCheck + 1, out iNewFirstMax, out iNewLastMax);
            UpdateMaxWidth(iNewWidth, iNewFirstMax, iNewLastMax, fForceUpdate);
            if (!GetStateFlag(VTCStateFlags.InHorizontalAdjust)
                &&
                prevMaxItemWidth != myMaxItemWidth)
            {
                SetStateFlag(VTCStateFlags.InHorizontalAdjust, true);
                var cxWnd = ClientSize.Width;
                var iMouseVertShift = 0;
                var cxMaxAdjust = ((myXPos > 0) && ((myXPos + cxWnd) > myMaxItemWidth))
                                      ? cxWnd + myXPos
                                      : myMaxItemWidth;

                if (cxMaxAdjust > cxWnd)
                {
                    NativeMethods.SendMessage(hWnd, NativeMethods.LB_SETHORIZONTALEXTENT, cxMaxAdjust, 0);
                    //					if (!HasHorizontalScrollBar)
                    //					{
                    //						//pTree->fHorz = TRUE; //NYI: SetWindowLong required?
                    //						NativeMethods.SetScrollPos(hWnd, NativeMethods.ScrollBarType.Vertical, newTopIndex, true);
                    //						if (deferVerifyInView) 
                    //						{
                    //							//Vegas#27815.  Adding the scrollbar may move an item which was
                    //							//supposed to be in view out of view.  To avoid looping this routine,
                    //							//call it later through the timer.
                    //							//SetTimer(hWnd, IDT_SCROLLWAIT, GetDoubleClickTime(), NULL); //NYI: ScrollWait timer
                    //							//SetStateFlag(VTCStateFlags.ScrollWait, true);
                    //						}
                    //						if (mouseShift != MouseShiftStyle.None) 
                    //						{
                    //							iMouseVertShift = -SystemInformation.HorizontalScrollBarHeight;
                    //						}
                    //					}
                    //
                    //					NativeMethods.SCROLLINFO si = new NativeMethods.SCROLLINFO(
                    //						NativeMethods.ScrollInfoFlags.Page | NativeMethods.ScrollInfoFlags.Range,
                    //						0,
                    //						cxMaxAdjust - 1,
                    //						cxWnd,
                    //						0);
                    //
                    //					NativeMethods.SetScrollInfo(hWnd, NativeMethods.ScrollBarType.Horizontal, ref si, true);
                }
                else if (HasHorizontalScrollBar)
                {
                    NativeMethods.SendMessage(hWnd, NativeMethods.LB_SETHORIZONTALEXTENT, 0, 0);
                    NativeMethods.SetScrollRange(hWnd, NativeMethods.ScrollBarType.Horizontal, 0, 0, true);
                    //					//pTree->fHorz = FALSE; //NYI: SetWindowLong required?
                    //
                    //					NativeMethods.SetScrollPos(hWnd, NativeMethods.ScrollBarType.Vertical, newTopIndex, true);
                    //					NativeMethods.SetScrollRange(hWnd, NativeMethods.ScrollBarType.Horizontal, 0, 0, true);
                    if (mouseShift != MouseShiftStyle.None)
                    {
                        iMouseVertShift = SystemInformation.HorizontalScrollBarHeight;
                    }
                }
                if (iMouseVertShift != 0)
                {
                    //Vegas#30045, move the mouse with the down button on the scroll bar
                    NativeMethods.POINT pnt;
                    NativeMethods.GetCursorPos(out pnt);
                    if (mouseShift == MouseShiftStyle.PageDown)
                    {
                        //See if the scrollbar addition will shift the down button under the mouse
                        NativeMethods.RECT rctWindow;
                        NativeMethods.GetWindowRect(hWnd, out rctWindow);
                        //NYI: Is the edge height correct here? Should we check for non-3D borders instead?
                        if (pnt.y
                            < rctWindow.bottom - SystemInformation.Border3DSize.Height
                            - (HasHorizontalScrollBar ? 2 : 3) * SystemInformation.HorizontalScrollBarHeight)
                        {
                            iMouseVertShift = 0;
                        }
                    }
                    if (iMouseVertShift != 0)
                    {
                        pnt.y += iMouseVertShift;
                        NativeMethods.SetCursorPos(pnt.x, pnt.y);
                    }
                }
                SetStateFlag(VTCStateFlags.InHorizontalAdjust, false);
            }
        }

        //  Computes the horizontal and vertical scroll bar ranges, pages, and
        //  positions, adding or removing the scroll bars as needed.
        //
        //  sets HasHorizontal, HasVertical state
        private void CalcScrollBars()
        {
            var newExtent = 0;
            var hWnd = Handle;
            if (myMctree == null)
            {
                var xPos = 0;
                var cxWnd = ClientSize.Width;
                if (HasHorizontalScrollBar)
                {
                    xPos = NativeMethods.GetScrollPos(hWnd, NativeMethods.ScrollBarType.Horizontal);
                }
                var cxMaxAdjust = ((xPos > 0) && ((xPos + cxWnd) > myMaxItemWidth))
                                      ? cxWnd + xPos
                                      : myMaxItemWidth;
                if (cxMaxAdjust < cxWnd)
                {
                    cxMaxAdjust = 0;
                }
                newExtent = cxMaxAdjust;
            }
            if (myHeaderBounds.HasHeaders)
            {
                var fullPercentWidth = myLastSize.Width;
                if (!HasVerticalScrollBar)
                {
                    fullPercentWidth -= SystemInformation.VerticalScrollBarWidth;
                }
                newExtent = myHeaderBounds.ChangeControlWidth(fullPercentWidth);
                if (newExtent <= fullPercentWidth)
                {
                    newExtent = 0;
                }
            }
            NativeMethods.SendMessage(hWnd, NativeMethods.LB_SETHORIZONTALEXTENT, newExtent, 0);
        }

        //  Find the width of a given number of items.  With the virtual treeview,
        //  the width of the entire tree is never calculated because we care only
        //  about the visible portion of the list.
        private int ComputeWidthOfRange(int startIndex, int count, out int firstMax, out int lastMax)
        {
            // Note that we don't want to do anything here for a multi-column tree
            // because we don't care about the item width. The width is determined
            // based on the size of fixed-width columns, not as a function of items
            // in the tree. Therefore, this version of the routine is blocked if
            // there are multiple columns.
            if (myMctree != null
                && (myMctree.ColumnCount > 1))
            {
                if (myColumnPermutation != null
                    && myColumnPermutation.VisibleColumnCount == 1)
                {
                    return ComputeWidthOfRange(myColumnPermutation.GetNativeColumn(0), startIndex, count, out firstMax, out lastMax);
                }
                else
                {
                    firstMax = VirtualTreeConstant.NullIndex;
                    lastMax = VirtualTreeConstant.NullIndex;
                    return 1;
                }
            }
            return ComputeWidthOfRange(0, startIndex, count, out firstMax, out lastMax);
        }

        private int ComputeWidthOfRange(int column, int startIndex, int count, out int firstMax, out int lastMax)
        {
            firstMax = VirtualTreeConstant.NullIndex;
            lastMax = VirtualTreeConstant.NullIndex;
            if (count == 0)
            {
                return 0;
            }
            var maxWidth = 1;

            Debug.Assert(count < 0x10000000); //Something is wrong with the count of visible items. Defaulting to 1
            if (count >= 0x10000000)
            {
                count = 1;
            }

            using (var g = CreateGraphics())
            {
                var columnItems = myTree.EnumerateColumnItems(column, null, false, startIndex, startIndex + count - 1);
                var styleLevelAdjust = GetAnyStyleFlag(VTCStyleFlags.HasRootLines | VTCStyleFlags.HasRootButtons);
                var levelAdjust = 0;
                var lastLevel = -1;
                var lastIsSimple = false;
                int curWidth;
                int imageWidth;
                var baseOffset = 0;
                while (columnItems.MoveNext())
                {
                    if (lastLevel != columnItems.Level
                        || lastIsSimple != columnItems.SimpleCell)
                    {
                        lastIsSimple = columnItems.SimpleCell;
                        lastLevel = columnItems.Level;
                        if (styleLevelAdjust)
                        {
                            levelAdjust = (columnItems.ColumnInTree == 0 || !columnItems.SimpleCell) ? 1 : 0;
                        }
                        baseOffset = (lastLevel + levelAdjust) * myIndentWidth;
                    }
                    var info = new VirtualTreeItemInfo(columnItems.Branch, columnItems.RowInBranch, columnItems.ColumnInBranch, 0);
                    curWidth = ListItemStringWidth(g, ref info, out imageWidth);
                    curWidth += baseOffset + imageWidth;
                    if (maxWidth == curWidth)
                    {
                        lastMax = columnItems.RowInTree;
                    }
                    else if (maxWidth < curWidth)
                    {
                        maxWidth = curWidth;
                        firstMax = lastMax = columnItems.RowInTree;
                    }
                }
            } // using g

            return maxWidth;
        }

        /// <summary>
        ///     Compute the width required to hold all items in the column
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        protected int ComputeColumnWidth(int column)
        {
            int firstMaxDummy;
            int lastMaxDummy;
            return ComputeWidthOfRange(column, 0, myTree.VisibleItemCount, out firstMaxDummy, out lastMaxDummy);
        }

        #endregion //Scrolling Functions

        #region Various listbox and treeview properties

        /// <summary>
        ///     The ITree implementation used to get data to display in the control.
        ///     Setting the Tree property at runtime produces a single-column control.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(null)]
        public ITree Tree
        {
            get { return myTree; }
            set
            {
                SetTree(value);
                ValidateSelectionColumnAfterTreeChange();
                if (IsHandleCreated)
                {
                    AfterHandleCreated();
                }
                // Do this after the item count is reset in AfterHandleCreated
                // so we don't force a redraw.
                ValidateHeadersAfterTreeChange();
            }
        }

        /// <summary>
        ///     The IMultiColumnTree implementation used to get data to display in the control.
        ///     Setting the MultiColumnTree property at runtime produces a multi-column tree grid control.
        ///     The object that implements IMultiColumnTree must also implement ITree, and only
        ///     one of these properties should be set to assign a tree.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(null)]
        public IMultiColumnTree MultiColumnTree
        {
            get { return myMctree; }
            set
            {
                SetTree((ITree)value);
                myMctree = value;
                ValidateSelectionColumnAfterTreeChange();
                AccessibleRole = AccessibleRole.Outline;
                if (IsHandleCreated)
                {
                    AfterHandleCreated();
                }
                // Do this after the item count is reset in AfterHandleCreated
                // so we don't force a redraw.
                ValidateHeadersAfterTreeChange();
            }
        }

        private void SetTree(ITree value)
        {
            // Bulk of set code from Tree property enables turning off the tree
            // without calling IsHandleCreated, such as when an item switches
            // from a single column to a multicolumn tree.
            // Detach events from current tree
            if (myTree != null)
            {
                myTree.ItemCountChanged -= OnItemCountChanged;
                myTree.ListShuffleBeginning -= ListShuffleBeginning;
                myTree.ListShuffleEnding -= ListShuffleEnding;
                myTree.OnSetRedraw -= OnSetRedraw;
                myTree.OnRefresh -= OnRefresh;
                myTree.OnDisplayDataChanged -= OnDisplayDataChanged;
                myTree.ItemMoved -= OnItemMoved;
                myTree.StateToggled -= OnToggleState;
            }
            myTree = value;
            AccessibleRole = AccessibleRole.Outline;
            myMctree = null;

            // Attach events to new tree
            if (value != null)
            {
                myTree.ItemCountChanged += OnItemCountChanged;
                myTree.ListShuffleBeginning += ListShuffleBeginning;
                myTree.ListShuffleEnding += ListShuffleEnding;
                myTree.OnSetRedraw += OnSetRedraw;
                myTree.OnRefresh += OnRefresh;
                myTree.OnDisplayDataChanged += OnDisplayDataChanged;
                myTree.ItemMoved += OnItemMoved;
                myTree.StateToggled += OnToggleState;
            }
        }

        private void ValidateSelectionColumnAfterTreeChange()
        {
            var colPerm = myColumnPermutation;
            var supportedColumns = (myMctree == null) ? 1 : myMctree.ColumnCount;
            // This function is called before ValidateHeadersAfterTreeChange, which
            // will clear the column permutation if the column counts don't line up,
            // so we need to make sure that the we only use the column permutation
            // count if it is going to survive the tree change
            if (colPerm != null)
            {
                if (colPerm.FullColumnCount == supportedColumns)
                {
                    supportedColumns = colPerm.VisibleColumnCount;
                }
            }
            if (mySelectionColumn >= supportedColumns)
            {
                // Don't set the CurrentColumn property here. It fires selection
                // change notifications here which can cause problems because the
                // myTree/myMCTree have changed, but AfterHandleCreated (which
                // resets the item count) has not been called.
                mySelectionColumn = 0;
            }
        }

        /// <summary>
        ///     The Bold style of the current font. Should only be set to null by
        ///     derived controls, allowing the BoldFont getter to regenerate the object.
        /// </summary>
        protected Font BoldFont
        {
            get
            {
                if (myBoldFont == null)
                {
                    var font = base.Font;
                    if (font != null)
                    {
                        myBoldFont = new Font(font, FontStyle.Bold | font.Style);
                    }
                }
                return myBoldFont;
            }
            set
            {
                Debug.Assert(value == null);
                if (myBoldFont != null)
                {
                    myBoldFont.Dispose();
                    myBoldFont = null;
                }
                myBoldFont = value;
            }
        }

        /// <summary>
        ///     Control.OnFontChanged override. Resets scrollbars and item item.
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnFontChanged(EventArgs e)
        {
            BoldFont = null;
            if (myHeaderContainer != null)
            {
                myHeaderContainer.Font = Font;
            }
            CalcTextHeight();
            CalcItemHeight();
            ScrollBarsAfterSetWidth(VirtualTreeConstant.NullIndex);
            base.OnFontChanged(e);
        }

        /// <summary>
        ///     Get the current index of an item. If there is a caret but no item
        ///     anchor, then this will return -1, whereas CurrentIndex returns 0.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "hWnd")]
        protected int CurrentIndexCheckAnchor
        {
            get
            {
                var retVal = -1;
                if (IsHandleCreated && ItemCount > 0)
                {
                    var hWnd = Handle;
                    retVal = CaretIndex;
                    if (retVal == 0)
                    {
                        // A caret of zero indicates a brand new listbox if the anchor is
                        // not also set.
                        if (0 > AnchorIndex)
                        {
                            retVal = -1;
                        }
                    }
                }
                return retVal;
            }
        }

        /// <summary>
        ///     Get or set the current index of the control. Combine with CurrentColumn
        ///     to figure out the current cell coordinate.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "hWnd")]
        [Browsable(false)]
        [DefaultValue(-1)]
        public int CurrentIndex
        {
            get
            {
                if (IsHandleCreated && ItemCount > 0)
                {
                    return CaretIndex;
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                if (myTree == null
                    || value >= myTree.VisibleItemCount)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (IsHandleCreated)
                {
                    if (InLabelEdit)
                    {
                        DismissLabelEdit(false, false);
                    }
                    var hWnd = Handle;
                    if (GetStyleFlag(VTCStyleFlags.MultiSelect))
                    {
                        if (GetStyleFlag(VTCStyleFlags.ExtendedMultiSelect))
                        {
                            SetCurrentExtendedMultiSelectIndex(value, false, false, ModifySelectionAction.Select);
                        }
                        else
                        {
                            // Moving caret in simple multi select does not set the selection
                            CaretIndex = value;
                            AnchorIndex = value;
                            DoSelectionChanged();
                        }
                    }
                    else
                    {
                        // Single-select case.
                        ClearSelection(false);
                        SetSelected(value, true);
                        CaretIndex = value;
                        AnchorIndex = value;
                        DoSelectionChanged();
                        FireWinEventsForSelection(false, false, ModifySelectionAction.None);
                    }
                }
            }
        }

        /// <summary>
        ///     The current selection column. The VirtualTreeControl supports
        ///     selecting multiple rows in a single column, but not cross-column selection, so
        ///     a single value is sufficient to give the current selection column.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(0)]
        public int CurrentColumn
        {
            get { return mySelectionColumn; }
            set
            {
                if (value != mySelectionColumn)
                {
                    SetSelectionColumn(value, true);
                }
            }
        }

        /// <summary>
        ///     Returns true if a inplace control is currently active.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(false)]
        public bool InLabelEdit
        {
            get { return GetStateFlag(VTCStateFlags.LabelEditActive); }
            set
            {
                if (value != InLabelEdit)
                {
                    if (value)
                    {
                        BeginLabelEdit();
                    }
                    else
                    {
                        DismissLabelEdit(false, true);
                    }
                }
            }
        }

        /// <summary>
        ///     Retrieve the active label edit control. Use with discretion: do not
        ///     do anything hasty with this control (for example, don't close it).
        /// </summary>
        [Browsable(false)]
        public Control LabelEditControl
        {
            get
            {
                if (myInPlaceControl != null)
                {
                    return myInPlaceControl.InPlaceControl;
                }
                return null;
            }
        }

        /// <summary>
        ///     Returns true if the control has focus, or any of the other
        ///     controls directly associated with it have focus, such as the header
        ///     control or the in-place edit control and its children.
        /// </summary>
        public bool TreatAsFocused
        {
            get
            {
                var testHandle = NativeMethods.GetFocus();
                return Focused || IsDrawWithFocusWindow(testHandle) || IsInPlaceEditWindow(testHandle);
            }
        }

        /// <summary>
        ///     Displays the scrollbar on the left instead of the right.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.SetWindowLong(System.IntPtr,System.Int32,System.Int32)")]
        [DefaultValue(false)]
        public bool LeftScrollBar
        {
            get { return GetStyleFlag(VTCStyleFlags.LeftScrollBar); }
            set
            {
                if (LeftScrollBar != value)
                {
                    SetStyleFlag(VTCStyleFlags.LeftScrollBar, value);
                    if (IsHandleCreated)
                    {
                        var hWnd = Handle;
                        var styleEx = (int)NativeMethods.GetWindowLong(hWnd, NativeMethods.GWL_EXSTYLE);
                        if (value)
                        {
                            styleEx |= NativeMethods.WS_EX_LEFTSCROLLBAR;
                        }
                        else
                        {
                            styleEx &= ~NativeMethods.WS_EX_LEFTSCROLLBAR;
                        }
                        NativeMethods.SetWindowLong(hWnd, NativeMethods.GWL_EXSTYLE, styleEx);
                        NativeMethods.SetWindowPos(
                            hWnd, IntPtr.Zero, 0, 0, 0, 0,
                            NativeMethods.SetWindowPosFlags.SWP_FRAMECHANGED | NativeMethods.SetWindowPosFlags.SWP_NOACTIVATE
                            | NativeMethods.SetWindowPosFlags.SWP_NOMOVE | NativeMethods.SetWindowPosFlags.SWP_NOSIZE
                            | NativeMethods.SetWindowPosFlags.SWP_NOZORDER);
                    }
                }
            }
        }

        /// <summary>
        ///     Returns the number of items that are currently selected in the tree.
        ///     This number may be significantly larger than the total number of items
        ///     shown in the iterator returned by CreateSelectedItemEnumerator if the
        ///     control is multiselect with multiple columns.
        /// </summary>
        [Browsable(false)]
        public int SelectedItemCount
        {
            get
            {
                if (IsHandleCreated)
                {
                    return SelectionCount;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        ///     Returns a ColumnItemEnumerator that can be walked to get the set
        ///     of selected items. The returned set depends on the current value of
        ///     the ColumnSelectionTransferAction property. The returned iterator
        ///     is valid until MoveNext returns false after which points the results
        ///     are undefined.
        /// </summary>
        /// <returns>An enumerator if items are selected, null otherwise.</returns>
        public ColumnItemEnumerator CreateSelectedItemEnumerator()
        {
            if (myTree == null)
            {
                return null;
            }
            var indices = SelectedIndicesArray;
            if (indices == null)
            {
                return null;
            }
            return myTree.EnumerateColumnItems(mySelectionColumn, myColumnPermutation, ExtendSelectionToAnchors, indices, false);
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.InvalidateRect(System.IntPtr,Microsoft.Data.Entity.Design.VisualStudio.NativeMethods+RECT@,System.Boolean)")]
        private void RedrawVisibleSelectedItems()
        {
            var caretIndex = CurrentIndex; // invalidate this separately first, because the caret may exist outside selected area.
            var firstVisible = TopIndex;
            var lastVisible = firstVisible + myFullyVisibleCount;
                // There is a +1 -1 in here, +1 to get a partially visible count, -1 to make it accurate
            NativeMethods.RECT rect;
            var hWnd = Handle;

            if (caretIndex != -1)
            {
                NativeMethods.SendMessage(hWnd, NativeMethods.LB_GETITEMRECT, caretIndex, out rect);
                NativeMethods.InvalidateRect(hWnd, ref rect, false);
            }

            if (GetStyleFlag(VTCStyleFlags.MultiSelect))
            {
                using (var selEnum = new SelectedIndexEnumerator(this))
                {
                    while (selEnum.MoveNext()
                           && selEnum.Current <= lastVisible)
                    {
                        if (selEnum.Current != caretIndex
                            && selEnum.Current >= firstVisible)
                        {
                            NativeMethods.SendMessage(hWnd, NativeMethods.LB_GETITEMRECT, selEnum.Current, out rect);
                            NativeMethods.InvalidateRect(hWnd, ref rect, false);
                        }
                    }
                }
            }
        }

        private int ColumnCount
        {
            get
            {
                var columns = 1;

                if (myMctree != null)
                {
                    if (myColumnPermutation != null)
                    {
                        columns = myColumnPermutation.VisibleColumnCount;
                    }
                    else
                    {
                        columns = myMctree.ColumnCount;
                    }
                }
                else if (myHeaderBounds.HasHeaders)
                {
                    columns = myHeaderBounds.HeaderCount;
                }

                return columns;
            }
        }

        private int ItemCount
        {
            get { return (myTree == null) ? 0 : myTree.VisibleItemCount; }
            set
            {
                Debug.Assert(IsHandleCreated);
                if (myMouseOverIndex >= value)
                {
                    // hide the tooltip if the new item count makes the current index invalid
                    myMouseOverIndex = myRawMouseOverIndex = VirtualTreeConstant.NullIndex;
                    HideBubble();
                }

                NativeMethods.SendMessage(Handle, NativeMethods.LB_SETCOUNT, value, 0);

                // Clear selection and reset caret/anchor when count changes (mimics listbox behavior).  LB_SETCOUNT is not sufficient, since we track these ourselves.
                ClearSelection(false);
                if (value > 0)
                {
                    CaretIndex = AnchorIndex = 0;
                }
                else
                {
                    CaretIndex = AnchorIndex = VirtualTreeConstant.NullIndex;
                }
            }
        }

        /// <summary>
        ///     The top index displayed by this control. Returns 0 if the handle has not been
        ///     created.
        /// </summary>
        /// <value></value>
        [Browsable(false)]
        [DefaultValue(0)]
        public virtual int TopIndex
        {
            get
            {
                return IsHandleCreated
                           ? (int)NativeMethods.SendMessage(Handle, NativeMethods.LB_GETTOPINDEX, 0, 0)
                           : 0;
            }
            set
            {
                if (IsHandleCreated)
                {
                    if (value < -1
                        || myTree == null
                        || value >= myTree.VisibleItemCount)
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }
                    NativeMethods.SendMessage(Handle, NativeMethods.LB_SETTOPINDEX, value, 0);
                }
            }
        }

        /// <summary>
        ///     Control.DefaultSize override
        /// </summary>
        protected override Size DefaultSize
        {
            get { return new Size(120, 96); }
        }

        /// <summary>
        ///     The default image list for items in the control. Individual branches can easily
        ///     ignore the default the default image list by returning a custom image list in
        ///     VirtualTreeDisplayData. However, image lists from all branches should have the
        ///     same item size.
        /// </summary>
        /// <value></value>
        [DefaultValue(null)]
        public ImageList ImageList
        {
            get { return myImageList; }
            set
            {
                if (myImageList == value)
                {
                    return;
                }
                if (myImageList != null)
                {
                    myImageList.RecreateHandle -= ImageListRecreated;
                }
                SetImageList(value);
                if (value != null)
                {
                    myImageList.RecreateHandle += ImageListRecreated;
                }
            }
        }

        private void SetImageList(ImageList value)
        {
            var oldWidth = myImageWidth;
            var oldHeight = myImageHeight;
            myImageWidth = 0;
            myIndentWidth = DEFAULT_INDENTWIDTH;
            myImageHeight = 0;
            myImageList = value;
            if (value != null)
            {
                var newSize = value.ImageSize;
                myImageWidth = newSize.Width + MAGIC_INDENT;
                if (myIndentWidth < myImageWidth)
                {
                    myIndentWidth = myImageWidth;
                }
                myImageHeight = newSize.Height;
                if (oldHeight != myImageHeight)
                {
                    CalcItemHeight();
                }
                if (oldWidth != myImageWidth)
                {
                    IndentBitmap = null;
                }
            }
        }

        /// <summary>
        ///     The current indent width. This is based on the size of the image list
        ///     and cannot be set directly.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(DEFAULT_INDENTWIDTH)]
        public int IndentWidth
        {
            get { return myIndentWidth; }
            protected set { myImageWidth = value; }
        }

        /// <summary>
        ///     The default StateImageList for the control. State images are displayed before the
        ///     standard image icons. If the StandardCheckBoxes property is true then this
        ///     returns an ImageList corresponding to the StandardCheckBoxImage enum. Individual
        ///     branches can easily ignore the default state image list by returning a custom StateImageList
        ///     in VirtualTreeDisplayData.
        /// </summary>
        [DefaultValue(null)]
        public ImageList StateImageList
        {
            get
            {
                var images = myStateImageList;
                if (images == null
                    && GetStyleFlag(VTCStyleFlags.StandardCheckBoxes))
                {
                    myStateImageList =
                        images = CreateStandardCheckboxImages((myImageList == null) ? new Size(16, 16) : myImageList.ImageSize);
                    myStateImageDescriptions = CreateStandardCheckboxDescriptions();
                    myStateImageAccessibleStates = CreateStandardCheckboxAccessibleStates();
                    myStateImageWidth = images.ImageSize.Width;
                }
                return images;
            }
            set
            {
                if (myStateImageList == value)
                {
                    return;
                }
                //NYI: Do we need to track recreation/recreate the indent bitmaps like we do with the primary imagelist?
                SetStyleFlag(VTCStyleFlags.StandardCheckBoxes, false);
                myStateImageWidth = 0;
                myStateImageList = value;
                if (value != null)
                {
                    myStateImageWidth = myStateImageList.ImageSize.Width;
                }
            }
        }

        /// <summary>
        ///     Locate and select an object in this control
        /// </summary>
        /// <param name="startingBranch">The branch to start the search from, or null for the root branch.</param>
        /// <param name="target">The object to locate</param>
        /// <param name="selectStyle">The object location style (sent to IBranch.LocateObject locateStyle parameter)</param>
        /// <param name="selectData">The object location data (sent to IBranch.LocateObject locateOptions parameter)</param>
        /// <returns>true if the item was selected</returns>
        public bool SelectObject(IBranch startingBranch, object target, int selectStyle, int selectData)
        {
            // The code here would look the same as the more specific SelectObject function, except that
            // it would always set the CurrentIndex property, even in ExtendedMultiSelect cases. The
            // last three parameters match the values used inside CurrentIndex for the SetCurrentExtendedMultiSelectIndex
            // call, so just defer to the more advanced function.
            return SelectObject(startingBranch, target, selectStyle, selectData, false, false, ModifySelectionAction.Select);
        }

        /// <summary>
        ///     Locate and select an object in this control
        /// </summary>
        /// <param name="startingBranch">The branch to start the search from, or null for the root branch.</param>
        /// <param name="target">The object to locate</param>
        /// <param name="selectStyle">The object location style (sent to IBranch.LocateObject locateStyle parameter)</param>
        /// <param name="selectData">The object location data (sent to IBranch.LocateObject locateOptions parameter)</param>
        /// <param name="extendFromAnchor">Use when SelectionMode is ExtendedMultiSelect. True to extend the selection from the current anchor position</param>
        /// <param name="preserveSelection">Use when SelectionMode is ExtendedMultiSelect. True to maintain selected items outside the anchored range</param>
        /// <param name="selectCaretAction">Use when SelectionMode is ExtendedMultiSelect. Specify how the selection state should be modified</param>
        /// <returns>true if the item was selected</returns>
        public bool SelectObject(
            IBranch startingBranch, object target, int selectStyle, int selectData, bool extendFromAnchor, bool preserveSelection,
            ModifySelectionAction selectCaretAction)
        {
            if (myTree != null)
            {
                var locateTarget = myTree.LocateObject(startingBranch, target, selectStyle, selectData);
                if (locateTarget.IsValid)
                {
                    var selectionColumn = mySelectionColumn;
                    var targetColumn = locateTarget.Column;
                    if (myColumnPermutation != null)
                    {
                        targetColumn = myColumnPermutation.GetPermutedColumn(targetColumn);
                        if (targetColumn == -1)
                        {
                            // Item is not currently visible
                            return false;
                        }
                    }
                    if (GetStyleFlag(VTCStyleFlags.MultiSelect) && ExtendSelectionToAnchors)
                    {
                        // If the target column is in the same blank expansion as the
                        // current column then we leave the selection column alone.
                        if (myTree.GetBlankExpansion(locateTarget.Row, selectionColumn, myColumnPermutation).AnchorColumn == targetColumn)
                        {
                            // Block if condition below
                            selectionColumn = targetColumn;
                        }
                    }
                    if (selectionColumn != targetColumn)
                    {
                        SetSelectionColumn(locateTarget.Column, false);
                    }
                    if (GetStyleFlag(VTCStyleFlags.ExtendedMultiSelect))
                    {
                        SetCurrentExtendedMultiSelectIndex(locateTarget.Row, extendFromAnchor, preserveSelection, selectCaretAction);
                    }
                    else
                    {
                        CurrentIndex = locateTarget.Row;
                    }
                    return true;
                }
            }
            return false;
        }

        private void CalcTextHeight()
        {
            if (IsHandleCreated)
            {
                using (var g = CreateGraphics())
                {
                    var startHeight = myTextHeight;

                    myTextHeight =
                        Size.Ceiling(
                            StringRenderer.MeasureString(
                                UseCompatibleTextRendering, g, VirtualTreeStrings.GetString(VirtualTreeStrings.CalcTextHeightLetter), Font,
                                StringFormat)).Height + 2 * SystemInformation.BorderSize.Height;
                    if (HeaderVisible && (myTextHeight != startHeight))
                    {
                        RedrawHeaderFrame();
                    }
                }
            }
        }

        private void CalcItemHeight()
        {
            CalcItemHeight(false);
        }

        /// <summary>
        ///     Calculate the item height
        /// </summary>
        /// <param name="forceReset">Always reset, even if the height has not changed. Used during window recreation.</param>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.GetClientRect(System.IntPtr,Microsoft.Data.Entity.Design.VisualStudio.NativeMethods+RECT@)")]
        private void CalcItemHeight(bool forceReset)
        {
            if (IsHandleCreated)
            {
                NativeMethods.RECT rc;
                NativeMethods.GetClientRect(Handle, out rc);
                var cyWnd = rc.bottom;
                var newHeight = Math.Max(myImageHeight, myTextHeight) + (HasHorizontalGridLines ? 1 : 0);
                if (forceReset || newHeight != myItemHeight)
                {
                    myItemHeight = newHeight;
                    NativeMethods.SendMessage(Handle, NativeMethods.LB_SETITEMHEIGHT, 0, myItemHeight);
                    CalcScrollBars();
                    myFullyVisibleCount = cyWnd / myItemHeight;
                    myPartlyVisibleCountIgnoreHScroll = (cyWnd + (HasHorizontalScrollBar ? SystemInformation.HorizontalScrollBarHeight : 0)
                                                         + myItemHeight - 1) / myItemHeight;
                    IndentBitmap = null;
                }
            }
        }

        private void ImageListRecreated(object sender, EventArgs e)
        {
            Debug.Assert(sender == myImageList);
            SetImageList(myImageList);
        }

        #endregion //Various listbox and treeview properties

        #region Colors

        public override Color BackColor
        {
            get
            {
                if (overrideBackColor)
                {
                    return base.BackColor;
                }
                return SystemColors.Window;
            }
            set
            {
                base.BackColor = value;
                overrideBackColor = true;
            }
        }

        public override Color ForeColor
        {
            get
            {
                if (overrideForeColor)
                {
                    return base.ForeColor;
                }
                return SystemColors.WindowText;
            }
            set
            {
                base.ForeColor = value;
                overrideForeColor = true;
            }
        }

        public Color SelectedItemActiveBackColor
        {
            get
            {
                if (m_selectedItemActiveBackColor != Color.Empty)
                {
                    return m_selectedItemActiveBackColor;
                }
                return SystemColors.Highlight;
            }
            set { m_selectedItemActiveBackColor = value; }
        }

        public Color SelectedItemInactiveBackColor
        {
            get
            {
                if (m_selectedItemInactiveBackColor != Color.Empty)
                {
                    return m_selectedItemInactiveBackColor;
                }
                return SystemColors.Control;
            }
            set { m_selectedItemInactiveBackColor = value; }
        }

        public Color SelectedItemActiveForeColor
        {
            get
            {
                if (m_selectedItemActiveForeColor != Color.Empty)
                {
                    return m_selectedItemActiveForeColor;
                }
                return SystemColors.HighlightText;
            }
            set { m_selectedItemActiveForeColor = value; }
        }

        public Color SelectedItemInactiveForeColor
        {
            get
            {
                if (m_selectedItemInactiveForeColor != Color.Empty)
                {
                    return m_selectedItemInactiveForeColor;
                }
                return SystemColors.WindowText;
            }
            set { m_selectedItemInactiveForeColor = value; }
        }

        public Color DisabledItemForeColor
        {
            get
            {
                if (m_disabledItemForeColor != Color.Empty)
                {
                    return m_disabledItemForeColor;
                }
                return SystemColors.GrayText;
            }
            set { m_disabledItemForeColor = value; }
        }

        public Color GridLinesColor
        {
            get
            {
                if (m_gridLinesColor != Color.Empty)
                {
                    return m_gridLinesColor;
                }

                var gridLinesColor = SystemColors.ControlDark;
                if (gridLinesColor.ToArgb() == SystemColors.Window.ToArgb())
                {
                    gridLinesColor = SystemColors.GrayText;
                    if (gridLinesColor.ToArgb() == SystemColors.Window.ToArgb())
                    {
                        gridLinesColor = SystemColors.WindowText;
                    }
                }

                return gridLinesColor;
            }
            set { m_gridLinesColor = value; }
        }

        public Color InPlaceEditForeColor
        {
            get
            {
                if (m_inplaceEditForeColor != Color.Empty)
                {
                    return m_inplaceEditForeColor;
                }

                // By default, the in-place edit uses the same colors as the window
                return ForeColor;
            }
            set { m_inplaceEditForeColor = value; }
        }

        public Color InPlaceEditBackColor
        {
            get
            {
                if (m_inplaceEditBackColor != Color.Empty)
                {
                    return m_inplaceEditBackColor;
                }

                // By default, the in-place edit uses the same colors as the window
                return BackColor;
            }
            set { m_inplaceEditBackColor = value; }
        }

        private bool overrideBackColor, overrideForeColor;
        private Color m_selectedItemActiveBackColor, m_selectedItemInactiveBackColor;
        private Color m_selectedItemActiveForeColor, m_selectedItemInactiveForeColor;
        private Color m_disabledItemForeColor;
        private Color m_gridLinesColor;
        private Color m_inplaceEditForeColor, m_inplaceEditBackColor;

        #endregion //Colors

        #region DragDrop Routines

        #region Structure representing a branch and coordinate for active drag sources

        private struct DragSourceOwner
        {
            public DragSourceOwner(IBranch branch, int row, int column)
            {
                Branch = branch;
                Row = row;
                Column = column;
            }

            public void Clear()
            {
                Branch = null;
            }

            public void OnQueryContinueDrag(QueryContinueDragEventArgs args)
            {
                if (Branch != null)
                {
                    Branch.OnQueryContinueDrag(args, Row, Column);
                }
            }

            public void OnGiveFeedback(GiveFeedbackEventArgs args)
            {
                if (Branch != null)
                {
                    Branch.OnGiveFeedback(args, Row, Column);
                }
            }

            public IBranch Branch;
            public readonly int Row;
            public readonly int Column;
        }

        #endregion // Structure representing a branch and coordinate for active drag sources

        private DragSourceOwner mySingleDragSource; // Used for a single drag source
        private List<DragSourceOwner> myDragSources; // Used for multiple drag sources

        [SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges")]
        private void OnDragTimerTick(Object sender, EventArgs eventArgs)
        {
            var inBumpScroll = GetStateFlag(VTCStateFlags.InBumpScroll);
            if (inBumpScroll)
            {
                // bump scroll case
                myDragTimer.Interval = myBumpDelayTickCount;
                var mousePosition = PointToClient(MousePosition);
                var scrollSize = GetBumpScrollSize(mousePosition);
                if (scrollSize == Size.Empty)
                {
                    // Stop the timer if no scroll occurs.  This restarts the bump-scroll process.
                    StopDragTimer();
                }
                else
                {
                    if (scrollSize.Width != 0)
                    {
                        if (scrollSize.Width < 0)
                        {
                            NativeMethods.SendMessage(
                                Handle, NativeMethods.WM_HSCROLL, NativeMethods.MAKELONG((int)NativeMethods.ScrollAction.LineLeft, 0), 0);
                        }
                        else
                        {
                            NativeMethods.SendMessage(
                                Handle, NativeMethods.WM_HSCROLL, NativeMethods.MAKELONG((int)NativeMethods.ScrollAction.LineRight, 0), 0);
                        }
                        // Don't send EndScroll here.  This speeds up repeated bump-scrolls because the list view treats them as a single scroll action.
                        // Set a flag so that we send the end scroll when the timer is cancelled.
                        SetStateFlag(VTCStateFlags.BumpHScrollSent, true);
                    }
                    if (scrollSize.Height != 0)
                    {
                        if (scrollSize.Height < 0)
                        {
                            NativeMethods.SendMessage(
                                Handle, NativeMethods.WM_VSCROLL, NativeMethods.MAKELONG((int)NativeMethods.ScrollAction.LineUp, 0), 0);
                        }
                        else
                        {
                            NativeMethods.SendMessage(
                                Handle, NativeMethods.WM_VSCROLL, NativeMethods.MAKELONG((int)NativeMethods.ScrollAction.LineDown, 0), 0);
                        }
                        // Don't send EndScroll here.  This speeds up repeated bump-scrolls because the list view treats them as a single scroll action.
                        // Set a flag so that we send the end scroll when the timer is cancelled.
                        SetStateFlag(VTCStateFlags.BumpVScrollSent, true);
                    }
                }
            }
            else
            {
                // toggle expansion case
                // Stop timer before doing anything in case toggle expansion
                // is too slow or has side effects in the branch callback.
                var expandRow = myLastDragExpandRow;
                StopDragTimer();
                myTree.ToggleExpansion(expandRow, myLastDragExpandCol);
            }
        }

        private void StopDragTimer()
        {
            if (myDragTimer.Enabled)
            {
                SetStateFlag(VTCStateFlags.InBumpScroll, false);
                // send EndScroll notifications, if we bump scrolled.
                if (GetStateFlag(VTCStateFlags.BumpHScrollSent))
                {
                    SetStateFlag(VTCStateFlags.BumpHScrollSent, false);
                    NativeMethods.SendMessage(Handle, NativeMethods.WM_HSCROLL, (int)NativeMethods.ScrollAction.EndScroll, 0);
                }
                if (GetStateFlag(VTCStateFlags.BumpVScrollSent))
                {
                    SetStateFlag(VTCStateFlags.BumpVScrollSent, false);
                    NativeMethods.SendMessage(Handle, NativeMethods.WM_VSCROLL, (int)NativeMethods.ScrollAction.EndScroll, 0);
                }
                myDragTimer.Stop();
                myLastDragExpandRow = VirtualTreeConstant.NullIndex;
            }
        }

        private void DoDragOverExpand()
        {
            // see if branch is expandable, and if so, set the timer
            // check to see if we are already dragging over an item, so no need to set the timer again
            // Note that a pending bump scroll takes precendence over a drag over expand.  This is so that
            // bump scrolling doesn't get interrupted by expansion.
            if (!GetStateFlag(VTCStateFlags.InBumpScroll)
                && (myLastDragExpandRow != myDropRow || myLastDragExpandCol != myDropColumn || !myDragTimer.Enabled))
            {
                StopDragTimer();
                myLastDragExpandRow = myDropRow;
                myLastDragExpandCol = myDropColumn;

                if (myDropRow != VirtualTreeConstant.NullIndex)
                {
                    // make sure target is expandable. Do the cheaper check first (IsExpanded doesn't go to a branch)
                    if (!myTree.IsExpanded(myDropRow, myDropColumn)
                        && myTree.IsExpandable(myDropRow, myDropColumn))
                    {
                        myDragTimer.Interval = SystemInformation.DoubleClickTime * 3;
                        myDragTimer.Start();
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges")]
        private void DoDragOverBumpScroll(Point clientPoint)
        {
            // check if we're in the bump scroll region
            if (GetBumpScrollSize(clientPoint) != Size.Empty)
            {
                SetStateFlag(VTCStateFlags.InBumpScroll, true);
                myDragTimer.Interval = myBumpInitialDelayTickCount;
                myDragTimer.Start();
            }
            else
            {
                StopDragTimer();
            }
        }

        private Size GetBumpScrollSize(Point clientPoint)
        {
            var bumpScrollSize = Size.Empty;
            var clientSize = ClientSize;
            var bumpScrollRegionSize = myItemHeight / 2; // bump scroll if mouse is within half an item of the edge of the control

            // check for vertical scrolling
            if (clientPoint.Y <= bumpScrollRegionSize)
            {
                if (myYPos > 0)
                {
                    bumpScrollSize.Height = -1;
                }
            }
            else if (clientPoint.Y <= clientSize.Height
                     && clientPoint.Y >= (clientSize.Height - bumpScrollRegionSize))
            {
                if (HasVerticalScrollBar)
                {
                    var scrollInfo = new NativeMethods.SCROLLINFO();
                    scrollInfo.fMask = NativeMethods.ScrollInfoFlags.Range | NativeMethods.ScrollInfoFlags.Position;
                    NativeMethods.GetScrollInfo(Handle, NativeMethods.ScrollBarType.Vertical, ref scrollInfo);
                    if (scrollInfo.nPos < scrollInfo.nMax)
                    {
                        bumpScrollSize.Height = 1;
                    }
                }
            }

            // check for horizontal scrolling
            if (clientPoint.X <= bumpScrollRegionSize)
            {
                if (myXPos > 0)
                {
                    bumpScrollSize.Width = -1;
                }
            }
            else if (clientPoint.X <= clientSize.Width
                     && clientPoint.X >= (clientSize.Width - bumpScrollRegionSize))
            {
                if (HasHorizontalScrollBar)
                {
                    var scrollInfo = new NativeMethods.SCROLLINFO();
                    scrollInfo.fMask = NativeMethods.ScrollInfoFlags.Range | NativeMethods.ScrollInfoFlags.Position;
                    NativeMethods.GetScrollInfo(Handle, NativeMethods.ScrollBarType.Horizontal, ref scrollInfo);
                    if (scrollInfo.nPos < scrollInfo.nMax)
                    {
                        bumpScrollSize.Width = 1;
                    }
                }
            }

            return bumpScrollSize;
        }

        /// <summary>
        ///     Determines how drag effects are combined during a multiselect drag/drop operation.
        /// </summary>
        [DefaultValue(DragEffectCombinationMode.Union)]
        public DragEffectCombinationMode DragEffectCombinationMode
        {
            get
            {
                return GetStateFlag(VTCStateFlags.CombineDragEffectsWithAnd)
                           ? DragEffectCombinationMode.Intersection
                           : DragEffectCombinationMode.Union;
            }
            set
            {
                // Compare value to the current state if you decide this should
                // have an immediate impact other than switching the bits
                switch (value)
                {
                    case DragEffectCombinationMode.Intersection:
                        SetStateFlag(VTCStateFlags.CombineDragEffectsWithAnd, false);
                        break;
                    case DragEffectCombinationMode.Union:
                        SetStateFlag(VTCStateFlags.CombineDragEffectsWithAnd, true);
                        break;
                        // Ignore other cases, not worth the error message
                }
            }
        }

        /// <summary>
        ///     Clear all drag source owners
        /// </summary>
        private void ClearDragSources()
        {
            mySingleDragSource.Clear();
            if (myDragSources != null)
            {
                myDragSources.Clear();
            }
        }

        /// <summary>
        ///     Package a drag object to use with DoDragDrop or Copy/Paste operations. Use the passed in location
        ///     for a single select tree, and the current selection state for a multiselect tree. Derived
        ///     classes can override this function to add extra information to the returned drag data. Any override
        ///     should first defer to the base implementation.
        /// </summary>
        /// <param name="row">Target row in the tree</param>
        /// <param name="nativeColumn">The native column</param>
        /// <param name="dragReason">The reason for retrieving the drop object</param>
        /// <returns>A populated drag structure. IsEmpty may be true.</returns>
        protected virtual VirtualTreeStartDragData PopulateDragData(int row, int nativeColumn, DragReason dragReason)
        {
            ClearDragSources();
            if (GetStyleFlag(VTCStyleFlags.MultiSelect))
            {
                var iter = CreateSelectedItemEnumerator();
                if (iter != null)
                {
                    var finished = false;
                    try
                    {
                        List<VirtualTreeStartDragData> sourceData = null;
                        var firstDragData = new VirtualTreeStartDragData();
                        var intersectEffects = GetStateFlag(VTCStateFlags.CombineDragEffectsWithAnd);
                        var allowedEffects = DragDropEffects.None;
                        var totalCount = 0;
                        while (iter.MoveNext())
                        {
                            var dragData = iter.Branch.OnStartDrag(this, iter.RowInBranch, iter.ColumnInBranch, dragReason);
                            if (dragReason == DragReason.CanCut
                                || dragReason == DragReason.CanCopy)
                            {
                                // special handling for cut/copy queries.
                                if (dragData == VirtualTreeStartDragData.AllowCutCopy)
                                {
                                    firstDragData = dragData;
                                    totalCount = 1;
                                }
                                else
                                {
                                    // bail out, operation is not supported
                                    finished = true;
                                    return VirtualTreeStartDragData.Empty;
                                }
                            }
                            else if (!dragData.IsEmpty)
                            {
                                if (totalCount == 0)
                                {
                                    // Store the first one without using the lists, we return it directly in this case
                                    mySingleDragSource = new DragSourceOwner(iter.Branch, iter.RowInBranch, iter.ColumnInBranch);
                                    firstDragData = dragData;
                                    allowedEffects = dragData.AllowedEffects;
                                    ++totalCount;
                                }
                                else
                                {
                                    if (totalCount == 1)
                                    {
                                        // We're moving from a single item to multiple items.
                                        // Move our single drag data and source information into
                                        // lists.
                                        sourceData = new List<VirtualTreeStartDragData>();
                                        sourceData.Add(firstDragData);
                                        if (myDragSources == null)
                                        {
                                            myDragSources = new List<DragSourceOwner>();
                                        }
                                        myDragSources.Add(mySingleDragSource);
                                        mySingleDragSource.Clear();
                                    }

                                    // Add source owners before the potential cancel coming from intersectAffects
                                    myDragSources.Add(new DragSourceOwner(iter.Branch, iter.RowInBranch, iter.ColumnInBranch));
                                    if (intersectEffects)
                                    {
                                        allowedEffects &= dragData.AllowedEffects;
                                        if (allowedEffects == 0)
                                        {
                                            // The finaly clause will clear out the data sources gracefully
                                            // if we don't set the finished signal.
                                            return VirtualTreeStartDragData.Empty;
                                        }
                                    }
                                    else
                                    {
                                        allowedEffects |= dragData.AllowedEffects;
                                    }
                                    sourceData.Add(dragData);
                                    ++totalCount;
                                }
                            }
                        }
                        finished = true;
                        if (totalCount == 1)
                        {
                            return firstDragData;
                        }
                        else if (totalCount > 1)
                        {
                            // Keep this CLS compliant and just return the array of structures
                            // as returned by the branches.
                            return new VirtualTreeStartDragData(sourceData.ToArray(), allowedEffects);
                        }
                    }
                    finally
                    {
                        if (!finished)
                        {
                            ForceCancelDataSources();
                        }
                    }
                }
            }
            else
            {
                var info = myTree.GetItemInfo(row, nativeColumn, false);
                var dragData = info.Branch.OnStartDrag(this, info.Row, info.Column, dragReason);
                if (!dragData.IsEmpty)
                {
                    mySingleDragSource = new DragSourceOwner(info.Branch, info.Row, info.Column);
                    return dragData;
                }
            }
            return VirtualTreeStartDragData.Empty;
        }

        /// <summary>
        ///     Send a QueryContinueDrag Cancel notification to all data source
        ///     owners so that they can clean up their own objects.
        /// </summary>
        private void ForceCancelDataSources()
        {
            var singleSource = mySingleDragSource.Branch != null;
            var multipleSources = myDragSources != null && myDragSources.Count > 0;
            if (singleSource || multipleSources)
            {
                var args = new QueryContinueDragEventArgs(0, false, DragAction.Cancel);
                DragSourceOwner owner;
                if (singleSource)
                {
                    owner = mySingleDragSource;
                    mySingleDragSource.Clear();
                    owner.OnQueryContinueDrag(args);
                }
                if (multipleSources)
                {
                    var count = myDragSources.Count;
                    for (var i = 0; i < count; ++i)
                    {
                        owner = myDragSources[i];
                        myDragSources[i].Clear();
                        // Make sure no one refuses
                        args.Action = DragAction.Cancel;
                        owner.OnQueryContinueDrag(args);
                    }
                    myDragSources.Clear();
                }
            }
        }

        /// <summary>
        ///     Control.DragEventArgs override. Defers to the branch at the drop position, or the base object
        ///     if a branch is not displayed. Note that the standard control events are not fired if an item is
        ///     being dragged over a branch item.
        /// </summary>
        /// <param name="dragArgs">DragEventArgs</param>
        protected override void OnDragOver(DragEventArgs dragArgs)
        {
            var clientPoint = PointToClient(new Point(dragArgs.X, dragArgs.Y));
            var hitInfo = HitInfo(clientPoint.X, clientPoint.Y);
            var targetRow = hitInfo.Row;
            var targetColumn = hitInfo.NativeColumn;
            if (targetRow != myDropRow
                || targetColumn != myDropColumn)
            {
                if (myDropRow != VirtualTreeConstant.NullIndex)
                {
                    ForwardDragEvent(DragEventType.Leave, null);
                    if (GetStateFlag(VTCStateFlags.DragDropHighlight))
                    {
                        SetStateFlag(VTCStateFlags.DragDropHighlight, false);
                        InvalidateItemForDrag();
                    }
                }
                myDropRow = targetRow;
                myDropColumn = targetColumn;
                dragArgs.Effect = DragDropEffects.None;
                if (myDropRow >= 0)
                {
                    ForwardDragEvent(DragEventType.Enter, dragArgs);
                    if (0 != (dragArgs.Effect & (DragDropEffects.Copy | DragDropEffects.Link | DragDropEffects.Move)))
                    {
                        SetStateFlag(VTCStateFlags.DragDropHighlight, true);
                        InvalidateItemForDrag();
                    }
                }
            }
            else if (myDropRow >= 0)
            {
                ForwardDragEvent(DragEventType.Over, dragArgs);
                if (!GetStateFlag(VTCStateFlags.DragDropHighlight)
                    && (0 != (dragArgs.Effect & (DragDropEffects.Copy | DragDropEffects.Link | DragDropEffects.Move))))
                {
                    SetStateFlag(VTCStateFlags.DragDropHighlight, true);
                    InvalidateItemForDrag();
                }
            }
            else
            {
                base.OnDragOver(dragArgs);
            }
            DoDragOverExpand();
            if (!myDragTimer.Enabled)
            {
                DoDragOverBumpScroll(clientPoint);
            }
        }

        /// <summary>
        ///     Control.OnDragDrop override. Defers to the branch at the drop position, or the base object
        ///     if a branch is not displayed. Note that the standard control events are not fired if an item is
        ///     being dragged over a branch item.
        /// </summary>
        /// <param name="dragArgs">DragEventArgs</param>
        protected override void OnDragDrop(DragEventArgs dragArgs)
        {
            if (myDropRow != VirtualTreeConstant.NullIndex)
            {
                StopDragTimer();
                ForwardDragEvent(DragEventType.Drop, dragArgs);
                if (GetStateFlag(VTCStateFlags.DragDropHighlight))
                {
                    SetStateFlag(VTCStateFlags.DragDropHighlight, false);
                    InvalidateItemForDrag();
                }
                myDropRow = VirtualTreeConstant.NullIndex;
            }
            else
            {
                base.OnDragDrop(dragArgs);
            }
        }

        /// <summary>
        ///     Control.OnDragEnter override. Defers to the branch at the drop position, or the base object
        ///     if a branch is not displayed. Note that the standard control events are not fired if an item is
        ///     being dragged over a branch item.
        /// </summary>
        /// <param name="dragArgs">DragEventArgs</param>
        protected override void OnDragEnter(DragEventArgs dragArgs)
        {
            //Debug.Assert(myDropRow == VirtualTreeConstant.NullIndex);
            var clientPoint = PointToClient(new Point(dragArgs.X, dragArgs.Y));
            var hitInfo = HitInfo(clientPoint.X, clientPoint.Y);
            var targetRow = hitInfo.Row;
            dragArgs.Effect = DragDropEffects.None;
            if (targetRow >= 0)
            {
                myDropRow = targetRow;
                myDropColumn = hitInfo.NativeColumn;
                ForwardDragEvent(DragEventType.Enter, dragArgs);
                if (0 != (dragArgs.Effect & (DragDropEffects.Copy | DragDropEffects.Link | DragDropEffects.Move)))
                {
                    SetStateFlag(VTCStateFlags.DragDropHighlight, true);
                    InvalidateItemForDrag();
                }
            }
            else
            {
                base.OnDragEnter(dragArgs);
            }
        }

        /// <summary>
        ///     Control.OnDragLeave override. Defers to the branch at the drop position, or the base object
        ///     if a branch is not displayed. Note that the standard control events are not fired if an item is
        ///     being dragged over a branch item.
        /// </summary>
        /// <param name="args">EventArgs</param>
        protected override void OnDragLeave(EventArgs args)
        {
            if (myDropRow != VirtualTreeConstant.NullIndex)
            {
                ForwardDragEvent(DragEventType.Leave, null);
                if (GetStateFlag(VTCStateFlags.DragDropHighlight))
                {
                    SetStateFlag(VTCStateFlags.DragDropHighlight, false);
                    InvalidateItemForDrag();
                }
                myDropRow = VirtualTreeConstant.NullIndex;
            }
            else
            {
                base.OnDragLeave(args);
            }

            // cancel the pending drag expand timer if necessary
            StopDragTimer();
        }

        // During drag, the current item is highlighted.  This invalidates the appropriate portion of the tree to keep the highlight current.
        private void InvalidateItemForDrag()
        {
            // invalidate whole row if multi-column highlight is set, otherwise just the drag column.
            if (MultiColumnHighlight)
            {
                InvalidateItem(myDropRow, -1, NativeMethods.RedrawWindowFlags.Invalidate | NativeMethods.RedrawWindowFlags.Erase);
            }
            else
            {
                var column = myColumnPermutation == null ? myDropColumn : myColumnPermutation.GetPermutedColumn(myDropColumn);
                var itemRect = GetItemRectangle(myDropRow, column);
                var expansion = myTree.GetBlankExpansion(myDropRow, column, myColumnPermutation);
                if (expansion.RightColumn != column)
                {
                    itemRect.Width = (GetItemRectangle(myDropRow, expansion.RightColumn).Right - itemRect.X);
                }
                var rect = new NativeMethods.RECT(itemRect);
                NativeMethods.RedrawWindow(
                    Handle, ref rect, IntPtr.Zero, NativeMethods.RedrawWindowFlags.Invalidate | NativeMethods.RedrawWindowFlags.Erase);
            }
        }

        private void ForwardDragEvent(DragEventType dragType, DragEventArgs args)
        {
            var info = myTree.GetItemInfo(myDropRow, myDropColumn, false);
            info.Branch.OnDragEvent(this, info.Row, info.Column, dragType, args);
        }

        // Return true if the user moves a certain number of pixels in any direction
        private bool CheckForDragBegin(int x, int y, int row, int column, ref bool lButtonDownPending)
        {
            NativeMethods.MSG msg;
            var dragSize = SystemInformation.DragSize;

            if (dragSize.Width == 0)
            {
                dragSize = SystemInformation.DoubleClickSize;
            }

            var testRect = new Rectangle(
                PointToScreen(new Point(x - dragSize.Width, y - dragSize.Height)),
                new Size(dragSize.Width + dragSize.Width, dragSize.Height + dragSize.Height));

            var itemRect = GetItemRectangle(row, column);
            itemRect.Location = PointToScreen(itemRect.Location);
            testRect.Intersect(itemRect);

            var hWnd = Handle;
            // This routine does not set/release capture like the original
            // because this is done automatically by the .NET Control class.
            //NativeMethods.SetCapture(hWnd);
            var finished = false;
            var retVal = false;
            do
            {
                if (NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, NativeMethods.PeekMessageAction.Remove))
                {
                    // See if the application wants to process the message...
                    //UNDONE: Should probably provide a different message
                    if (NativeMethods.CallMsgFilter(ref msg, NativeMethods.MSGF_COMMCTRL_BEGINDRAG))
                    {
                        continue;
                    }

                    switch (msg.message)
                    {
                        case NativeMethods.WM_LBUTTONUP:
                            if (lButtonDownPending)
                            {
                                lButtonDownPending = false;
                                DoSelectionChangeFromMouse(
                                    ref myMouseDownHitInfo, GetStateFlag(VTCStateFlags.MouseButtonDownShift),
                                    GetStateFlag(VTCStateFlags.MouseButtonDownCtrl), MouseButtons.Left);
                            }
                            finished = true;
                            break;
                        case NativeMethods.WM_RBUTTONUP:
                        case NativeMethods.WM_LBUTTONDOWN:
                        case NativeMethods.WM_RBUTTONDOWN:
                            //NativeMethods.ReleaseCapture();
                            finished = true;
                            //retVal = false; // Already set
                            break;

                        case NativeMethods.WM_MOUSEMOVE:
                            if (!testRect.Contains(msg.pt_x, msg.pt_y))
                            {
                                //NativeMethods.ReleaseCapture();
                                finished = true;
                                retVal = true;
                            }
                            break;
                    }
                    if (!retVal
                        || itemRect.Contains(msg.pt_x, msg.pt_y))
                    {
                        NativeMethods.TranslateMessage(ref msg);
                        NativeMethods.DispatchMessage(ref msg);
                    }
                }

                // WM_CANCELMODE messages will unset the capture, in that
                // case I want to exit this loop
            }
            while (!finished
                   && (NativeMethods.GetCapture() == hWnd));

            return retVal;
        }

        /// <summary>
        ///     Control.OnQueryContinueDrag override. Defers to the branch being dragged. Defers to the base
        ///     if no branch is currently being dragged.
        /// </summary>
        /// <param name="args">QueryContinueDragEventArgs</param>
        protected override void OnQueryContinueDrag(QueryContinueDragEventArgs args)
        {
            if (mySingleDragSource.Branch != null)
            {
                // Process the single item
                mySingleDragSource.OnQueryContinueDrag(args);
                switch (args.Action)
                {
                    case DragAction.Cancel:
                    case DragAction.Drop:
                        mySingleDragSource.Clear();
                        break;
                }
            }
            else if (myDragSources != null
                     && myDragSources.Count > 0)
            {
                var clearSources = true;
                try
                {
                    ProcessMultipleOnQueryContinueDrag(args, myDragSources, myDragSources.Count);
                    clearSources = args.Action != DragAction.Continue;
                }
                finally
                {
                    if (clearSources)
                    {
                        myDragSources.Clear();
                    }
                }
            }
            else
            {
                base.OnQueryContinueDrag(args);
            }
        }

        /// <summary>
        ///     Helper fucntion for OnQueryContinueDrag. Allows us to reprocess
        ///     previously processed items if the DragAction changes.
        /// </summary>
        /// <param name="args">QueryContinueDragEventArgs</param>
        /// <param name="sources">The sources being changed</param>
        /// <param name="count">The count of items to process. May be less than sources.Count</param>
        private static void ProcessMultipleOnQueryContinueDrag(QueryContinueDragEventArgs args, List<DragSourceOwner> sources, int count)
        {
            var lastAction = args.Action;
            for (var i = 0; i < count; ++i)
            {
                sources[i].OnQueryContinueDrag(args);
                if (args.Action != lastAction)
                {
                    switch (lastAction)
                    {
                        case DragAction.Cancel:
                            // Ignore, can't abort a cancel
                            args.Action = DragAction.Cancel;
                            break;
                        case DragAction.Drop:
                            switch (args.Action)
                            {
                                case DragAction.Cancel:
                                    // Nothing can be done here for the previous items. You
                                    // can't cancel a drop after the drop has been notified.
                                    break;
                                case DragAction.Continue:
                                    // Ignore, if the user ignores a Cancel or Drop and
                                    // changes the DragAction they get what they deserve,
                                    // but don't affect the other objects.
                                    args.Action = DragAction.Drop;
                                    break;
                            }
                            break;
                        case DragAction.Continue:
                            // Process back from the beginning with the new drag or cancel action
                            if (i > 0)
                            {
                                ProcessMultipleOnQueryContinueDrag(args, sources, i - 1);
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Control.OnGiveFeeback override. Defers to the branch being dragged. Defers to the base
        ///     if no branch is currently being dragged.
        /// </summary>
        /// <param name="args">GiveFeedbackEventArgs</param>
        protected override void OnGiveFeedback(GiveFeedbackEventArgs args)
        {
            if (mySingleDragSource.Branch != null)
            {
                // Process the single item
                mySingleDragSource.OnGiveFeedback(args);
            }
            else if (myDragSources != null
                     && myDragSources.Count > 0)
            {
                var sources = myDragSources;
                var count = sources.Count;
                for (var i = 0; i < count; ++i)
                {
                    sources[i].OnGiveFeedback(args);
                }
            }
            else
            {
                base.OnGiveFeedback(args);
            }
        }

        #endregion // DragDrop Routines

        #region Cut/Copy/Paste Routines

        /// <summary>
        ///     Put the data object for the current selection on the clipboard
        ///     requesting data objects with DragReason.Copy
        /// </summary>
        public void Copy()
        {
            CutCopy(DragReason.Copy);
        }

        /// <summary>
        ///     Put the data object for the current selection on the clipboard
        ///     requesting data objects with DragReason.Cut. It is the responsibility
        ///     of the branches to provide OnDisplayDataChanged, OnItemDeleted, and other
        ///     events if the UI changes as a result of this request. The VirtualTreeDisplayData
        ///     should return a state of Cut in this case.
        /// </summary>
        public void Cut()
        {
            CutCopy(DragReason.Cut);
        }

        private bool CutCopy(DragReason reason)
        {
            if (myTree != null)
            {
                var index = -1;
                var selectionColumn = -1;
                var nativeSelectionColumn = -1;
                var fContinue = false;
                if (GetStyleFlag(VTCStyleFlags.MultiSelect))
                {
                    fContinue = true;
                }
                else if ((-1 != (index = CurrentIndex))
                         && (-1 != (selectionColumn = mySelectionColumn)))
                {
                    fContinue = true;
                    ResolveSelectionColumn(index, out selectionColumn, out nativeSelectionColumn);
                }
                if (fContinue)
                {
                    // Note that the position parameters are used with single select only
                    var dragData = PopulateDragData(index, nativeSelectionColumn, reason);
                    if (reason == DragReason.CanCut
                        || reason == DragReason.CanCopy)
                    {
                        return dragData == VirtualTreeStartDragData.AllowCutCopy;
                    }

                    if (!dragData.IsEmpty)
                    {
                        // Make a data object to put on the clipboard, unless the
                        // returned object is already a data object.
                        var dataObj = dragData.Data as IDataObject;
                        if (dataObj == null)
                        {
                            dataObj = new DataObject(dragData.Data);
                        }
                        Clipboard.SetDataObject(dataObj);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///     Can the current selection act as a cut source?  Queries this by calling
        ///     IBranch.OnStartDrag with DragReason.CanCopy.
        /// </summary>
        [Browsable(false)]
        public bool CanCopy
        {
            get { return CutCopy(DragReason.CanCopy); }
        }

        /// <summary>
        ///     Can the current selection act as a cut source?  Queries this by calling
        ///     IBranch.OnStartDrag with DragReason.CanCut.
        /// </summary>
        [Browsable(false)]
        public bool CanCut
        {
            get { return CutCopy(DragReason.CanCut); }
        }

        /// <summary>
        ///     Does the current object support a paste operation from the current
        ///     data object on the clipboard?
        /// </summary>
        [Browsable(false)]
        public bool CanPaste
        {
            get { return DoPaste(true); }
        }

        /// <summary>
        ///     Paste the item currently on the clipboard. Mimics a DragEnter
        ///     and DragDrop or DragLeave event on the current branch.
        /// </summary>
        public void Paste()
        {
            DoPaste(false);
        }

        /// <summary>
        ///     Helper function to do both the CanPaste and Paste operations
        /// </summary>
        /// <param name="canPasteOnly">true if this is only a test (called by CanPaste)</param>
        /// <returns>true if a paste is possible or succeeded</returns>
        private bool DoPaste(bool canPasteOnly)
        {
            int index;
            int selectionColumn;
            if (myTree != null
                && (-1 != (index = CurrentIndex))
                && (-1 != (selectionColumn = mySelectionColumn)))
            {
                IDataObject dataObject = null;
                try
                {
                    dataObject = Clipboard.GetDataObject();
                }
                catch (COMException)
                {
                    // Happens occasionally, treat like no data
                }
                if (dataObject == null)
                {
                    return false;
                }
                int nativeSelectionColumn;
                ResolveSelectionColumn(index, out selectionColumn, out nativeSelectionColumn);
                var info = myTree.GetItemInfo(
                    index, (myColumnPermutation != null) ? myColumnPermutation.GetNativeColumn(selectionColumn) : selectionColumn, false);
                var args = new DragEventArgs(
                    dataObject, 0, -1, -1, DragDropEffects.Copy | DragDropEffects.Link | DragDropEffects.Move, DragDropEffects.None);
                info.Branch.OnDragEvent(this, info.Row, info.Column, DragEventType.Enter, args);
                if ((args.Effect & args.AllowedEffect) != DragDropEffects.None)
                {
                    DragEventType closingEvent;
                    if (canPasteOnly)
                    {
                        closingEvent = DragEventType.Leave;
                        args = null;
                    }
                    else
                    {
                        closingEvent = DragEventType.Drop;
                    }

                    // Call the final event to make sure the branch can clean up
                    info.Branch.OnDragEvent(this, info.Row, info.Column, closingEvent, args);
                    return true;
                }
            }
            return false;
        }

        #endregion // Cut/Copy/Paste Routines

        #region ToolTip Routines

        private class ToolTipControl : Control
        {
            private readonly VirtualTreeControl myParent;
            [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
            private IntPtr myStringBuffer = IntPtr.Zero;
            private string myLastString;
            private RectangleF myTextRect;
            private Rectangle myFullLabelRectangle;
            private Point myActivatingMousePos;
            private StringFormat myFormat;

            protected override void OnMouseDown(MouseEventArgs e)
            {
                var newPoint = new Point(e.X, e.Y);
                newPoint = myParent.PointToClient(PointToScreen(newPoint));
                myParent.OnMouseDown(new MouseEventArgs(e.Button, e.Clicks, newPoint.X, newPoint.Y, e.Delta));
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                var newPoint = new Point(e.X, e.Y);
                newPoint = myParent.PointToClient(PointToScreen(newPoint));
                myParent.OnMouseUp(new MouseEventArgs(e.Button, e.Clicks, newPoint.X, newPoint.Y, e.Delta));
            }

            protected override void WndProc(ref Message m)
            {
                try
                {
                    switch (m.Msg)
                    {
                        case NativeMethods.WM_CONTEXTMENU:
                            var hWnd = myParent.Handle;
                            NativeMethods.SendMessage(hWnd, m.Msg, (int)hWnd, (int)m.LParam);
                            return;
                        case NativeMethods.WM_MOUSEACTIVATE:
                            m.Result = (IntPtr)NativeMethods.MA_NOACTIVATE;
                            return;
                        case NativeMethods.WM_NCHITTEST:
                            // When using v6 common controls, the native
                            // tooltip does not end up returning HTTRANSPARENT all the time, so its TTF_TRANSPARENT
                            // behavior does not work, ie. mouse events do not fall thru to controls underneath. This
                            // is due to a combination of old app-specific hacks in comctl32, functional changes between
                            // v5 and v6, and the specfic way the grid drives its tooltip. Workaround is to just
                            // force HTTRANSPARENT all the time.
                            m.Result = (IntPtr)NativeMethods.HTTRANSPARENT;
                            return;
                    }
                    base.WndProc(ref m);
                }
                catch (Exception ex)
                {
                    if (CriticalException.IsCriticalException(ex))
                    {
                        throw; // always throw critical exceptions.
                    }

                    if (!myParent.DisplayException(ex))
                    {
                        throw;
                    }
                }
            }

            public ToolTipControl(VirtualTreeControl parent)
            {
                myParent = parent;
                myLastString = String.Empty;
                SetStyle(ControlStyles.UserPaint, true);
                BackColor = SystemColors.Info;
                ForeColor = SystemColors.InfoText;
            }

            protected override void CreateHandle()
            {
                var icc = new NativeMethods.INITCOMMONCONTROLSEX();
                icc.dwICC = NativeMethods.ICC_TAB_CLASSES;
                NativeMethods.InitCommonControlsEx(icc);

                base.CreateHandle();
                if (IsHandleCreated)
                {
                    var ti = new NativeMethods.TOOLINFO();
                    ti.SetSize();
                    ti.uFlags = NativeMethods.TTF_IDISHWND | NativeMethods.TTF_TRANSPARENT;
                    ti.hwnd = myParent.Handle;
                    ti.uId = (int)ti.hwnd;
                    ti.lpszText = NativeMethods.LPSTR_TEXTCALLBACK;

                    NativeMethods.SendMessage(base.Handle, NativeMethods.TTM_ADDTOOL, 0, ref ti);
                    NativeMethods.SendMessage(base.Handle, NativeMethods.TTM_SETDELAYTIME, NativeMethods.TTDT_RESHOW, 0);
                }
            }

            protected override void OnPaint(PaintEventArgs pe)
            {
                // Fix CT 64185. Need to recreate graphic object, else tooltip does not show on long text.
                using (var g = Graphics.FromHwnd(Handle))
                {
                    base.OnPaint(pe);

                    g.Clear(BackColor);

                    var drawString = Marshal.PtrToStringAuto(myStringBuffer);
                    using (Brush foreColorBrush = new SolidBrush(ForeColor))
                    {
                        if (myFormat.Trimming != StringTrimming.None)
                        {
                            // don't allow the tooltip to trim the string at all.
                            using (var noTrimFormat = new StringFormat(myFormat))
                            {
                                noTrimFormat.Trimming = StringTrimming.None;
                                StringRenderer.DrawString(
                                    UseCompatibleTextRendering, g, drawString, Font, foreColorBrush, ForeColor, myTextRect, noTrimFormat);
                            }
                        }
                        else
                        {
                            StringRenderer.DrawString(
                                UseCompatibleTextRendering, g, drawString, Font, foreColorBrush, ForeColor, myTextRect, myFormat);
                        }
                    }
                }
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    var cp = base.CreateParams;
                    cp.Parent = myParent.Handle;
                    cp.ClassName = NativeMethods.TOOLTIPS_CLASS;
                    cp.ExStyle = NativeMethods.WS_EX_TOPMOST;
                    cp.Style = NativeMethods.TTS_NOPREFIX;
                    return cp;
                }
            }

            protected bool UseCompatibleTextRendering
            {
                get { return myParent.UseCompatibleTextRendering; }
            }

            public void Relay(IntPtr wParam, IntPtr lParam)
            {
                var msg = new NativeMethods.MSG();
                msg.lParam = lParam;
                msg.wParam = wParam;
                msg.message = NativeMethods.WM_MOUSEMOVE;
                msg.hwnd = myParent.Handle;
                NativeMethods.SendMessage(Handle, NativeMethods.TTM_RELAYEVENT, 0, ref msg);
            }

            public IntPtr GetTextPtr(ITree tree, int absRow, int column, ToolTipType tipType)
            {
                string text = null;
                var info = tree.GetItemInfo(absRow, column, false);
                text = info.Branch.GetTipText(info.Row, info.Column, tipType);
                if (text == null
                    || text.Length == 0 && tipType == ToolTipType.Default)
                {
                    text = info.Branch.GetText(info.Row, info.Column);
                }
                return SetStringBuffer(text);
            }

            [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison")]
            private IntPtr SetStringBuffer(string text)
            {
                // UNDONE: Can we ensure sufficient common control
                // version support to force UNICODE via a WM_NOTIFYFORMAT
                // message?
                if (text == null)
                {
                    text = string.Empty;
                }
                if (text == myLastString
                    || myLastString.CompareTo(text) == 0)
                {
                    return myStringBuffer;
                }
                myLastString = text;
                if (myStringBuffer != IntPtr.Zero)
                {
                    FreeStringBuffer();
                }
                if (Marshal.SystemDefaultCharSize == 1)
                {
                    myStringBuffer = Marshal.StringToCoTaskMemAnsi(text);
                }
                else
                {
                    myStringBuffer = Marshal.StringToBSTR(text);
                }
                return myStringBuffer;
            }

            private void FreeStringBuffer()
            {
                Debug.Assert(myStringBuffer != IntPtr.Zero);
                if (Marshal.SystemDefaultCharSize == 1)
                {
                    Marshal.FreeCoTaskMem(myStringBuffer);
                }
                else
                {
                    Marshal.FreeBSTR(myStringBuffer);
                }
                myStringBuffer = IntPtr.Zero;
            }

            public void PositionTipWindow(VirtualTreeControl ctl)
            {
                // measure the string, set up the window
                var tipString = Marshal.PtrToStringAuto(myStringBuffer);
                int stringWidth, stringHeight;
                using (var g = CreateGraphics())
                {
                    stringWidth = ctl.ListItemStringWidth(g, Font, tipString);
                    stringHeight = Math.Max(
                        StringRenderer.MeasureString(UseCompatibleTextRendering, g, tipString, Font, myFormat).Height, ctl.myTextHeight);
                }
                var textDimsInt = new Size();
                textDimsInt.Width = stringWidth;
                textDimsInt.Height = stringHeight;
                var windowRect = new Rectangle(ctl.PointToScreen(myFullLabelRectangle.Location), textDimsInt);

                // add padding for the client rectangle
                windowRect.Inflate(1, 1);

                // move window into screen space, clip, and set the position
                // To determine the screen to show the tip on, we could use Screen.FromRectangle(tipRect),
                // Screen.FromPoint(tipRect.Location), Screen.FromPoint(current mouse position), or
                // Screen.FromPoint(activating mouse position). I chose the last option.
                var offScreenEdge = false;
                var screenBounds = Screen.FromPoint(myActivatingMousePos).WorkingArea;
                if (windowRect.Left < screenBounds.Left)
                {
                    offScreenEdge = true;
                    windowRect.X = screenBounds.X;
                }
                else if (windowRect.Right > screenBounds.Right)
                {
                    offScreenEdge = true;
                    windowRect.X = screenBounds.X + screenBounds.Width - windowRect.Width;
                }

                if (windowRect.Bottom > screenBounds.Bottom)
                {
                    offScreenEdge = true;
                    windowRect.Y = screenBounds.Y + screenBounds.Height - windowRect.Height;
                }
                NativeMethods.SetWindowPos(
                    Handle,
                    IntPtr.Zero,
                    windowRect.Left,
                    windowRect.Top,
                    windowRect.Width,
                    windowRect.Height,
                    NativeMethods.SetWindowPosFlags.SWP_NOACTIVATE | NativeMethods.SetWindowPosFlags.SWP_NOZORDER);

                // find the point in the client rect to draw the text rect
                if (!offScreenEdge)
                {
                    myTextRect = new RectangleF(
                        PointToClient(ctl.PointToScreen(myFullLabelRectangle.Location)), new SizeF(textDimsInt.Width, textDimsInt.Height));
                }
                else
                {
                    var rect = NativeMethods.RECT.FromXYWH(windowRect.X, windowRect.Y, windowRect.Width, windowRect.Height);
                    NativeMethods.SendMessage(Handle, NativeMethods.TTM_ADJUSTRECT, 0, out rect);
                    myTextRect = new RectangleF(
                        PointToClient(new Point(rect.left, rect.top)), new SizeF(textDimsInt.Width, textDimsInt.Height));
                }
            }

            public void Pop()
            {
                if (IsHandleCreated)
                {
                    NativeMethods.SendMessage(Handle, NativeMethods.TTM_POP, 0, 0);
                }
            }

            public new void DestroyHandle()
            {
                base.DestroyHandle();
            }

            public void Activate(ref VirtualTreeHitInfo hit, ref ExtraHitInfo extraInfo, ref Point mousePos, bool immediateActivation)
            {
                var hWnd = Handle.ToInt32();
                if (hWnd == 0)
                {
                    return;
                }

                var ctrlhWnd = Handle;
                NativeMethods.SendMessage(ctrlhWnd, NativeMethods.TTM_ACTIVATE, 0, hWnd);
                if (hit.Row != VirtualTreeConstant.NullIndex)
                {
                    var ti = new NativeMethods.TOOLINFO();
                    ti.SetSize();
                    ti.uId = hWnd;
                    ti.uFlags = NativeMethods.TTF_IDISHWND;
                    ti.rect = NativeMethods.RECT.FromXYWH(
                        extraInfo.ClippedItemRectangle.X,
                        extraInfo.ClippedItemRectangle.Y,
                        extraInfo.ClippedItemRectangle.Width,
                        extraInfo.ClippedItemRectangle.Height);
                    NativeMethods.SendMessage(
                        ctrlhWnd, NativeMethods.TTM_SETDELAYTIME, NativeMethods.TTDT_INITIAL, immediateActivation ? 0 : 250);
                    NativeMethods.SendMessage(ctrlhWnd, NativeMethods.TTM_NEWTOOLRECT, 0, ref ti);
                    NativeMethods.SendMessage(ctrlhWnd, NativeMethods.TTM_ACTIVATE, 1, hWnd);
                    Font = extraInfo.LabelFont;
                    myFormat = extraInfo.LabelFormat;
                    myFullLabelRectangle = extraInfo.FullLabelRectangle;
                    myActivatingMousePos = mousePos;
                }
            }
        }

        private void DestroyTooltipWindow()
        {
            if (myTooltip != null)
            {
                HideBubble();
                myTooltip.DestroyHandle();
                myTooltip = null;
            }
        }

        private void HideBubble()
        {
            if (myTooltip != null
                && GetStateFlag(VTCStateFlags.ShowingTooltip))
            {
                myTooltip.Pop();
            }
        }

        private void UpdateToolTip()
        {
            if (myTooltip != null && Redraw)
            {
                UpdateMouseTargets();
            }
        }

        // Updates state of the control when the mouse position changes.  Currently updates
        // tooltip and checkbox hot-track states.  Return value currently indicates whether or
        // not a tooltip is showing after returning from this method.
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private bool UpdateMouseTargets()
        {
            if (!IsHandleCreated)
            {
                return false;
            }

            NativeMethods.POINT pt;
            var rectClipped = false;
            var containedInClient = true;

            NativeMethods.GetCursorPos(out pt);
            var mousePos = new Point(pt.x, pt.y);
            NativeMethods.ScreenToClient(Handle, ref pt);

            ExtraHitInfo extraInfo;
            VirtualTreeHitInfo hit;
            if (myTooltip != null)
            {
                myTipType = ToolTipType.Default;
                hit = HitInfo(
                    pt.x,
                    pt.y,
                    out extraInfo,
                    true);

                if (hit.Row != VirtualTreeConstant.NullIndex)
                {
                    if ((hit.HitTarget & VirtualTreeHitTargets.OnItemLabel) != 0)
                    {
                        var rightToLeft = 0 != (extraInfo.LabelFormat.FormatFlags & StringFormatFlags.DirectionRightToLeft);
                        if ((extraInfo.LabelFormat.Alignment == StringAlignment.Near && !rightToLeft)
                            || (extraInfo.LabelFormat.Alignment == StringAlignment.Far && rightToLeft))
                        {
                            //Don't show a custom tiptext over label to reduce interference with clicking, etc
                            //However, make sure that moving from label to icons switches to special text.
                            //Note that we only make this adjustment if label is aligned near, otherwise ClippedItemRectange
                            //will already be aligned away from the indent, glyph, etc.
                            extraInfo.ClippedItemRectangle.X += extraInfo.LabelOffset;
                            rectClipped = true;
                        }
                    }
                    else if ((hit.HitTarget & VirtualTreeHitTargets.OnItemIcon) != 0)
                    {
                        myTipType = ToolTipType.Icon;
                    }
                    else if ((hit.HitTarget & VirtualTreeHitTargets.OnItemStateIcon) != 0)
                    {
                        myTipType = ToolTipType.StateIcon;
                    }
                }
            }
            else
            {
                hit = HitInfo(
                    pt.x,
                    pt.y,
                    out extraInfo,
                    false);
            }

            // invalidate old hot-tracked region if it is no longer valid.  Do this before updating the cached indices.
            if (myMouseOverIndex != VirtualTreeConstant.NullIndex
                && GetStateFlag(VTCStateFlags.StateImageHotTracked)
                &&
                (myMouseOverIndex != hit.Row || myMouseOverColumn != hit.NativeColumn
                 || 0 == (hit.HitTarget & VirtualTreeHitTargets.StateIconHotTracked)))
            {
                SetStateFlag(VTCStateFlags.StateImageHotTracked, false);
                InvalidateStateImage(myMouseOverIndex, myMouseOverColumn, -1);
            }

            if (myTooltip != null)
            {
                // show tooltip only if on icon, state icon or the label when it is truncated
                if (0 == (hit.HitTarget & VirtualTreeHitTargets.OnItem))
                {
                    // Test for special case where we're to the right or left of a label in a blank expansion (left would only happen in cases where text is not left-aligned)
                    if (
                        !(extraInfo.IsTruncated
                          && ((hit.HitTarget & (VirtualTreeHitTargets.OnItemRight | VirtualTreeHitTargets.OnItemLeft)) != 0)))
                    {
                        hit.ClearRowData();
                    }
                }
                else if ((myTipType == ToolTipType.Default)
                         && !extraInfo.IsTruncated)
                {
                    containedInClient = (hit.RawRow != hit.Row) || ClientRectangle.Contains(extraInfo.ClippedItemRectangle);
                    if (containedInClient)
                    {
                        hit.ClearRowData();
                    }
                }

                // update the item we're showing the tooltip for...
                if (myRawMouseOverIndex != hit.RawRow
                    || GetStateFlag(VTCStateFlags.LastToolRectClipped) != rectClipped)
                {
                    SetStateFlag(VTCStateFlags.LastToolRectClipped, rectClipped);
                    myTooltip.Activate(
                        ref hit, ref extraInfo, ref mousePos,
                        myTipType == ToolTipType.Default && (!containedInClient || extraInfo.IsTruncated));
                }
            }

            myRawMouseOverIndex = hit.RawRow;
            myMouseOverIndex = hit.Row;
            myMouseOverColumn = hit.NativeColumn;

            // invalidate state image region if we're doing checkbox hot-tracking.  Do this after
            // updating the cached indices because they will be used by the draw item routine.
            if (0 != (hit.HitTarget & VirtualTreeHitTargets.StateIconHotTracked))
            {
                if (!GetStateFlag(VTCStateFlags.StateImageHotTracked))
                {
                    SetStateFlag(VTCStateFlags.StateImageHotTracked, true);
                    InvalidateStateImage(myMouseOverIndex, myMouseOverColumn, -1);
                }
            }

            return (myTooltip != null && hit.Row != VirtualTreeConstant.NullIndex);
        }

        #endregion // ToolTip Routines

        #region EditLabel Routines

        /// <summary>
        ///     Begin an edit timer at the given index, or do an immediate activation if appropriate.
        /// </summary>
        /// <param name="absIndex">The row to start the edit timer</param>
        /// <param name="message">The message id (WM_LBUTTONDOWN, etc) used to trigger the activation request.</param>
        private void StartEditTimer(int absIndex, int message)
        {
            var startTimer = false;
            if (GetAnyStyleFlag(VTCStyleFlags.ImmediateMouseLabelEdits | VTCStyleFlags.ImmediateSelectionLabelEdits))
            {
                // Attempt an immediate activation.
                myEditIndex = absIndex;
                var immediateActivationStatus = true;
                StartLabelEdit(false, message, ref immediateActivationStatus);
                startTimer = !immediateActivationStatus && GetStyleFlag(VTCStyleFlags.DelayedLabelEdits);
            }
            else if (GetStyleFlag(VTCStyleFlags.DelayedLabelEdits))
            {
                startTimer = true;
            }
            if (startTimer)
            {
                myEditIndex = absIndex;
                EditTimer.Interval = SystemInformation.DoubleClickTime;
                EditTimer.Enabled = true;
            }
        }

        private void CancelEditTimer()
        {
            EditTimer.Enabled = false;
            myEditIndex = VirtualTreeConstant.NullIndex;
        }

        private void EditTimer_Tick(object sender, EventArgs e)
        {
            if (EditTimer.Enabled)
            {
                EditTimer.Enabled = false;
                var immediateActivate = false;
                StartLabelEdit(false, 0, ref immediateActivate);
            }
        }

        /// <summary>
        ///     Begin a label edit
        /// </summary>
        /// <param name="explicitActivation">Activation is being explicitly requested by the user</param>
        /// <param name="message">The message id (WM_LBUTTONDOWN, etc) used to trigger the activation request. Use 0 for an explicit or timer-launched edit request.</param>
        /// <param name="immediateActivation">If immediateActivation is true on the way in, it will be true on the way out unless activation was deferred</param>
        private void StartLabelEdit(bool explicitActivation, int message, ref bool immediateActivation)
        {
            var requestImmediate = immediateActivation;
            int nativeSelectionColumn;
            int selectionColumn;
            ResolveSelectionColumn(myEditIndex, out selectionColumn, out nativeSelectionColumn);
            if (null == DoLabelEdit(myEditIndex, selectionColumn, message, explicitActivation, ref immediateActivation))
            {
                if (requestImmediate && !immediateActivation)
                {
                    DismissLabelEdit(false, false);
                }
            }
        }

        private void LabelEditTextChanged()
        {
            SetStateFlag(VTCStateFlags.LabelEditDirty, true);
            if (myInPlaceControl != null
                && (myInPlaceControl.Flags & VirtualTreeInPlaceControls.SizeToText) != 0)
            {
                SetEditSize();
            }
        }

        private void SetEditSize()
        {
            if (myEditIndex == VirtualTreeConstant.NullIndex)
            {
                return;
            }

            var sizeToText = (myInPlaceControl.Flags & VirtualTreeInPlaceControls.SizeToText) != 0;
            int selectionColumn;
            int nativeSelectionColumn;
            ResolveSelectionColumn(myEditIndex, out selectionColumn, out nativeSelectionColumn);
            var rcLabel = GetItemRectangle(
                myEditIndex, selectionColumn, true, sizeToText, sizeToText ? myInPlaceControl.InPlaceControl.Text : null);
            var stringWidth = rcLabel.Width;

            // get exact the text bounds (acount for borders used when drawing)
            rcLabel.Inflate(-SystemInformation.Border3DSize.Width, -SystemInformation.BorderSize.Height);

            //UNDONE: deal with this when editing is defined 
            SetEditInPlaceSize(myInPlaceControl, stringWidth, ref rcLabel);
        }

        /// <summary>
        ///     Launch an explicit label edit at the given cell. Explicit label editing
        ///     must be support by the control, the current branch Features, and the
        ///     current branches BeginLabelEdit.
        /// </summary>
        public void BeginLabelEdit()
        {
            if (GetStyleFlag(VTCStyleFlags.ExplicitLabelEdits))
            {
                var curIndex = CurrentIndex;
                if (curIndex >= 0)
                {
                    myEditIndex = curIndex;
                    var immediateActivation = false;
                    StartLabelEdit(true, 0, ref immediateActivation);
                }
            }
        }

        /// <summary>
        ///     Stop displaying an in place edit control
        /// </summary>
        /// <param name="cancel">
        ///     Set to true to close the inplace control without
        ///     calling the EndLabelEdit commit function
        /// </param>
        public void EndLabelEdit(bool cancel)
        {
            DismissLabelEdit(cancel, true);
        }

        private IVirtualTreeInPlaceControl DoLabelEdit(
            int absRow, int column, int message, bool explicitActivation, ref bool immediateActivation)
        {
            Debug.Assert(!explicitActivation || !immediateActivation);
            if (!GetAnyStyleFlag(VTCStyleFlags.LabelEditsMask))
            {
                return null;
            }

            DismissLabelEdit(false, false);

            var nativeColumn = (myColumnPermutation != null) ? myColumnPermutation.GetNativeColumn(column) : column;

            var info = myTree.GetItemInfo(absRow, nativeColumn, false);

            var flags = info.Branch.Features;
            // UNDONE: Explicit Label Edits
            if (immediateActivation
                && (0 == (flags & (BranchFeatures.ImmediateMouseLabelEdits | BranchFeatures.ImmediateSelectionLabelEdits))))
            {
                if (0 != (flags & BranchFeatures.DelayedLabelEdits))
                {
                    immediateActivation = false;
                }

                return null;
            }
            else if (explicitActivation && (0 == (flags & BranchFeatures.ExplicitLabelEdits)))
            {
                return null;
            }
            else if (!explicitActivation
                     && !immediateActivation
                     && 0 == (flags & BranchFeatures.DelayedLabelEdits))
            {
                return null;
            }

            // Begin the label editing sequence by retrieving the data from
            // from the branch and filling in unsupplied default values.
            VirtualTreeLabelEditActivationStyles activationStyle;
            if (immediateActivation)
            {
                // Distinguish between mouse and non-mouse selection if the branch supports both
                if (0 == (flags & BranchFeatures.ImmediateMouseLabelEdits))
                {
                    activationStyle = VirtualTreeLabelEditActivationStyles.ImmediateSelection;
                }
                else if (GetStateFlag(VTCStateFlags.SelChangeFromMouse))
                {
                    activationStyle = VirtualTreeLabelEditActivationStyles.ImmediateMouse;
                }
                else
                {
                    activationStyle = VirtualTreeLabelEditActivationStyles.ImmediateSelection;
                }
            }
            else if (explicitActivation)
            {
                activationStyle = VirtualTreeLabelEditActivationStyles.Explicit;
            }
            else
            {
                activationStyle = VirtualTreeLabelEditActivationStyles.Delayed;
            }
            var editData = info.Branch.BeginLabelEdit(info.Row, info.Column, activationStyle);
            if (!editData.IsValid)
            {
                return null;
            }
            else if (immediateActivation && editData.ActivationDeferred)
            {
                immediateActivation = false;
                return null;
            }
            var labelText = editData.AlternateText;
            var inPlaceEdit = editData.CustomInPlaceEdit;
            var maxTextLength = editData.MaxTextLength;

            if (labelText == null)
            {
                // Note: Don't compare to String.Empty. This allows the
                // user to return String.Empty to display an edit box with
                // nothing in it.
                labelText = info.Branch.GetText(info.Row, info.Column);
            }

            if (inPlaceEdit == null)
                // Alternate condition is not used. Without this check,
                // an invalid type will automatically throw a casting
                // exception in CreateEditInPlaceWindow, which is better
                // than silently ignoring the user setting.
                //|| !inPlaceEditType.IsSubclassOf(typeof(IVirtualTreeInPlaceControl)))
            {
                inPlaceEdit = typeof(VirtualTreeInPlaceEditControl);
            }

            ScrollIntoView(absRow, column);
            myEditColumn = column;
            myInPlaceControl = CreateEditInPlaceWindow(labelText, maxTextLength, column, message, inPlaceEdit);
            myCustomInPlaceCommit = editData.CustomCommit;
            var ctl = myInPlaceControl.InPlaceControl;

            // Set the colors of the in-place edit control
            myInPlaceControl.InPlaceControl.ForeColor = InPlaceEditForeColor;
            myInPlaceControl.InPlaceControl.BackColor = InPlaceEditBackColor;

            SetStateFlag(VTCStateFlags.LabelEditMask, false);
            SetStateFlag(VTCStateFlags.LabelEditActive, true);

            HideBubble();

            myEditIndex = absRow;
            SetEditSize();

            var invalidateEditItem = true;
            if (0 != (myInPlaceControl.Flags & VirtualTreeInPlaceControls.DrawItemText))
            {
                SetStateFlag(VTCStateFlags.LabelEditTransparent, true);
                invalidateEditItem = false;
            }

            // Show the window and set focus to it.  Do this after setting the
            // size so we don't get flicker.
            ctl.Show();

            // Changing focus causes OnLostFocus for the currently focused control.
            // Ignore the case that this triggers a DismissLabelEdit. May happen if the edit control has
            // child controls, each of which dismisses on an OnLostFocus.
            SetStateFlag(VTCStateFlags.NoDismissEdit, true);
            try
            {
                ctl.Focus();
            }
            finally
            {
                SetStateFlag(VTCStateFlags.NoDismissEdit, false);
            }

            if (invalidateEditItem)
            {
                InvalidateAreaForLabelEdit(absRow, column, ctl);
            }

            // Rescroll edit window
            myInPlaceControl.SelectAllText();

            // notify listeners of label edit state change
            OnLabelEditControlChanged(EventArgs.Empty);

            return myInPlaceControl;
        }

        /// <summary>
        ///     Invalidates proper region of the control when label edit state changes.
        /// </summary>
        private void InvalidateAreaForLabelEdit(int absRow, int column, Control labelEditControl)
        {
            // if we don't draw the selection as focused when the in-place edit is active
            // (and we don't by default), then we need to invalidate the entire selected
            // area, because it will need to be repainted as unfocused.
            var drawEditAsFocusedWindow = IsDrawWithFocusWindow(labelEditControl.Handle);
            if (!drawEditAsFocusedWindow
                && GetStyleFlag(VTCStyleFlags.MultiSelect))
            {
                var iter = CreateSelectedItemEnumerator();
                if (iter != null)
                {
                    while (iter.MoveNext())
                    {
                        // invalidate whole row if it's a multi-column highlight, otherwise just the column.
                        InvalidateItem(
                            iter.RowInTree, MultiColumnHighlight ? -1 : iter.DisplayColumn,
                            NativeMethods.RedrawWindowFlags.Invalidate | NativeMethods.RedrawWindowFlags.Erase);
                    }
                }
            }

            // Now handle the edit row.  This may not be part of the multi-selection, so we do it even if we've executed the block above.
            // Invalidate the whole row if it's a multi-column highlight and focusing the edit window changes the highlight color, otherwise just invalidate the column.
            InvalidateItem(
                absRow, (MultiColumnHighlight && !drawEditAsFocusedWindow) ? -1 : column,
                NativeMethods.RedrawWindowFlags.Invalidate | NativeMethods.RedrawWindowFlags.Erase);
        }

        private void LabelEditFlagsChanged(VirtualTreeInPlaceControls oldFlags, VirtualTreeInPlaceControls newFlags)
        {
            if (myInPlaceControl != null)
            {
                if (0 != ((newFlags & VirtualTreeInPlaceControls.DrawItemText) ^ (oldFlags & VirtualTreeInPlaceControls.DrawItemText)))
                {
                    SetStateFlag(VTCStateFlags.LabelEditTransparent, 0 != (newFlags & VirtualTreeInPlaceControls.DrawItemText));
                    InvalidateItem(
                        myEditIndex, myEditColumn, NativeMethods.RedrawWindowFlags.Invalidate | NativeMethods.RedrawWindowFlags.Erase);
                }
            }
        }

        private bool DismissLabelEdit(bool fCancel, bool fForceFocus)
        {
            if (GetStateFlag(VTCStateFlags.NoDismissEdit))
            {
                return false;
            }

            var fOkToContinue = true;

            var edit = myInPlaceControl;

            if (edit == null)
            {
                // Also make sure there are no pending edits...
                CancelEditTimer();
                return true;
            }
            var editCtl = edit.InPlaceControl;

            // Assume that if we are not visible that the window is in the
            // process of being destroyed and we should not process the
            // editing of the window...
            if (!NativeMethods.IsWindowVisible(Handle))
            {
                fCancel = true;
            }

            //
            // We are using the Window Id of the control as a BOOL to
            // state if it is dirty or not.
            Debug.Assert(GetStateFlag(VTCStateFlags.LabelEditActive));
            if (GetStateFlag(VTCStateFlags.LabelEditProcessing))
            {
                // We are in the process of processing an update now, bail out
                return true;
            }
            else if (GetStateFlag(VTCStateFlags.LabelEditDirty))
            {
                // The edit control is dirty so continue.
                SetStateFlag(VTCStateFlags.LabelEditProcessing, true);
            }
            else
            {
                // The edit control is not dirty so act like cancel.
                fCancel = true;
                SetStateFlag(VTCStateFlags.LabelEditProcessing, true);
            }

            var fCloseWindow = fCancel;
            var selectionColumn = -1;
            var iEdit = myEditIndex;
            // If we're canceling outright, then there is no reason to
            // notify the branch that the edit is being canceled.
            try
            {
                if (!fCancel)
                {
                    // Deleting items can set myEditIndex to NullIndex if the program
                    // deleted the items out from underneath us (while we are waiting
                    // for the edit timer).
                    if (iEdit != VirtualTreeConstant.NullIndex)
                    {
                        // Relocate the branch and item for the object and
                        // ask them to commit.
                        int nativeSelectionColumn;
                        ResolveSelectionColumn(iEdit, out selectionColumn, out nativeSelectionColumn);
                        var info = myTree.GetItemInfo(iEdit, nativeSelectionColumn, false);

                        var editCode = (myCustomInPlaceCommit == null)
                                           ? info.Branch.CommitLabelEdit(info.Row, info.Column, editCtl.Text)
                                           : myCustomInPlaceCommit(info, editCtl);

                        switch (editCode)
                        {
                            case LabelEditResult.AcceptEdit:
                                //NYI: Need to adjust horizontal extent
                                fCloseWindow = true;
                                break;
                            case LabelEditResult.CancelEdit:
                                fCloseWindow = true;
                                break;
                            case LabelEditResult.BlockDeactivate:
                                goto case LabelEditResult.CancelEdit;
                                //NYI: Need to get a posting mechanism here, probably through
                                //BeginInvoke, to call back and reopen the edit window at a later time.
                                //Debug.Assert(!fCloseWindow);
                                //SetStateFlag(VTCStateFlags.LabelEditProcessing, false);
                                //myInPlaceControl.SelectAllText();
                                //break;
                            default:
                                Debug.Assert(false, "Invalid Enum Value");
                                // Nothing much to do except toss it
                                fCloseWindow = true;
                                break;
                        }
                    }
                    else
                    {
                        fCloseWindow = true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (CriticalException.IsCriticalException(ex))
                {
                    fCloseWindow = false; // prevents us from doing work in finally block below in the case of a critical exception.
                    throw;
                }

                fCloseWindow = true;
                fOkToContinue = false;
                if (!DisplayException(ex))
                {
                    throw;
                }
            }
                /*       catch
            {
                fCloseWindow = true;
                fOkToContinue = false;
                throw;
            }*/
            finally
            {
                if (fCloseWindow)
                {
                    // Make sure the text redraws properly
                    if (iEdit != VirtualTreeConstant.NullIndex
                        && iEdit < ItemCount)
                    {
                        if (selectionColumn != -1)
                        {
                            int nativeSelectionColumn;
                            ResolveSelectionColumn(iEdit, out selectionColumn, out nativeSelectionColumn);
                        }
                        InvalidateAreaForLabelEdit(iEdit, selectionColumn, editCtl);
                    }
                    SetStateFlag(VTCStateFlags.NoDismissEdit, true); // this is so that we don't recurse due to killfocus
                    if (fForceFocus && !Focused)
                    {
                        Focus();
                    }
                    editCtl.Hide();
                    SetStateFlag(VTCStateFlags.NoDismissEdit, false);

                    // If we did not reenter edit mode before now reset the edit state
                    // variables to NULL
                    var disposeControl = (myInPlaceControl.Flags & VirtualTreeInPlaceControls.DisposeControl) != 0;
                    if (myInPlaceControl == edit)
                    {
                        myInPlaceControl = null;
                        myCustomInPlaceCommit = null;
                        SetStateFlag(VTCStateFlags.LabelEditMask, false);
                        myEditIndex = VirtualTreeConstant.NullIndex;
                    }

                    // done with the edit control -- if desired, we will
                    // dispose of it.  Otherwise, it's up to the branch
                    if (disposeControl)
                    {
                        // Reset this flag, Dispose can have side effects
                        SetStateFlag(VTCStateFlags.NoDismissEdit, true);
                        editCtl.Dispose();
                        SetStateFlag(VTCStateFlags.NoDismissEdit, false);
                    }
                }
            }

            // notify listeners of label edit state change
            OnLabelEditControlChanged(EventArgs.Empty);

            return fOkToContinue;
        }

        /// <summary>
        ///     Called whenever and an exception is thrown by a label edit control
        ///     or some other command. Overrides of this function should display the exception and return
        ///     true, or return false to let the exception be rethrown. Displaying without returning true is
        ///     considered bad form because display exception is likely to be called again as the message stack
        ///     unwinds and you can end up displaying the same message multiple times. The default implementation
        ///     displays a MessageBox and returns true.
        /// </summary>
        /// <param name="exception">The caught exception</param>
        /// <returns>true to indicate the user was notified of the exception, false to rethrow the exception.</returns>
        protected virtual bool DisplayException(Exception exception)
        {
            return DisplayException(Site, exception);
        }

        [SuppressMessage("Whitehorse.CustomRules", "WH01:DoNotUseMessageBoxShow",
            Justification = "[graysonm] Only using MessageBox.Show as a fallback.")]
        internal static bool DisplayException(IServiceProvider serviceProvider, Exception ex)
        {
            if (serviceProvider != null)
            {
                var uiService = serviceProvider.GetService(typeof(IUIService)) as IUIService;

                if (uiService != null)
                {
                    uiService.ShowError(ex);
                    return true;
                }
            }

            MessageBoxOptions options = 0;
            if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
            {
                options = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
            }
            MessageBox.Show(
                null, ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1,
                options);
            return true;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private IVirtualTreeInPlaceControl CreateEditInPlaceWindow(
            string labelText, int maxTextLength, int column, int message, object inPlaceEdit)
        {
            IVirtualTreeInPlaceControl retVal = null;
            if (inPlaceEdit is Type)
            {
                // If the branch has specified a type, we will create it
                retVal = (IVirtualTreeInPlaceControl)Activator.CreateInstance(inPlaceEdit as Type);
            }
            else
            {
                // otherwise assume we have an instance
                retVal = (IVirtualTreeInPlaceControl)inPlaceEdit;
            }
            var ctl = retVal.InPlaceControl;
            // Note that we used to use a Bold font if the item is drawn as bold. This was
            // done so that the edit box completely covered the text behind it. However, with
            // custom text drawing enabled, it is much more difficult to figure out the correct
            // width, so we simply blank out the item behind the edit window and avoid the issue.
            // Therefore, we always use the FontStyle of the current font for editing.
            ctl.Font = Font;
            if (maxTextLength > 0)
            {
                retVal.MaxTextLength = maxTextLength;
            }

            // Create the window with some nonzero size so margins work properly
            // The caller will do a SetEditInPlaceSize to set the real size
            // But make sure the width is huge so when an app calls SetWindowText,
            // USER won't try to scroll the window.
            ctl.Location = new Point(0, -20);
                // position outside client area prior to setting parent.  This eliminates flicker when window is repositioned.
            ctl.Size = new Size(16384, 20);
            ctl.Text = labelText;

            var accessibleName = ctl.AccessibleName;
            if ((accessibleName == null || accessibleName.Length == 0)
                && myHeaderBounds.HasHeaders)
            {
                // for accessibility set the control's name to the column name that it's in.
                ctl.AccessibleName = myHeaderBounds.Headers[column].Text;
            }

            ctl.Site = Site; // site the in-place control to allow it access to services provided by our container
            retVal.SelectionStart = 0;
            retVal.Parent = this;
            retVal.LaunchedByMessage = message;
            return retVal;
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.InvalidateRect(System.IntPtr,System.IntPtr,System.Boolean)")]
        private void SetEditInPlaceSize(IVirtualTreeInPlaceControl edit, int stringWidth, ref Rectangle boundingRect)
        {
            var editCtl = edit.InPlaceControl;
            var hwndEdit = editCtl.Handle;

            if ((edit.Flags & VirtualTreeInPlaceControls.SizeToText) != 0)
            {
                //Size stringSize = new Size(stringWidth, myItemHeight);
                var stringSize = new Size(stringWidth, myItemHeight);
                if (HasHorizontalGridLines)
                {
                    stringSize.Height -= 1;
                }

                // Minimum text box size is 1/4 icon spacing size
                stringSize.Width = Math.Max(stringSize.Width, SystemInformation.IconSpacingSize.Width / 4);

                // position the text rect based on the text rect passed in
                // if wrapping, center the edit control around the text mid point
                var textRect = new Rectangle(0, 0, stringSize.Width, stringSize.Height);
                textRect.Offset(
                    boundingRect.Left,
                    boundingRect.Top + (boundingRect.Height - textRect.Height) / 2);

                // give a little space to ease the editing of this thing
                textRect.Width += edit.ExtraEditWidth;

                //
                // Make sure that the whole edit window is always visible.
                // We should not extend it to the outside of the parent window.
                //
                var clippedRect = Rectangle.Intersect(ClientRectangle, textRect);
                if (!clippedRect.IsEmpty)
                {
                    textRect = clippedRect;
                }

                //
                // Inflate it after the clipping, because it's ok to hide border.
                //
                var rcFormat = new NativeMethods.RECT(edit.FormattingRectangle);

                // Turn the margins inside-out so we can AdjustWindowRect on them.
                rcFormat.top = -rcFormat.top;
                rcFormat.left = -rcFormat.left;
                NativeMethods.AdjustWindowRectEx(
                    ref rcFormat,
                    NativeMethods.GetWindowStyle(hwndEdit),
                    false,
                    NativeMethods.GetWindowExStyle(hwndEdit));

                textRect.Inflate(-rcFormat.left, -rcFormat.top);

                boundingRect = textRect;
            }

            NativeMethods.HideCaret(hwndEdit);
            editCtl.Size = new Size(boundingRect.Width, boundingRect.Height);
            editCtl.Location = new Point(boundingRect.Left, boundingRect.Top);
            NativeMethods.InvalidateRect(hwndEdit, IntPtr.Zero, true);
            NativeMethods.ShowCaret(hwndEdit);
        }

        #endregion //EditLabel Routines

        #region Accessibility Routines

        /// <summary>
        ///     Control.CreateAccessibilityInstance override
        /// </summary>
        /// <returns></returns>
        protected override AccessibleObject CreateAccessibilityInstance()
        {
            SetStateFlag(VTCStateFlags.ReturnedAccessibilityObject, true);
            return new AccTreeRoot(this);
        }

        private Rectangle GetAccessibilityLocation(int row, int column, int height, int width)
        {
            var rect = GetItemRectangle(row, column);
            if (height > 1
                || width > 1)
            {
                var rect2 = GetItemRectangle(row + height - 1, column + width - 1);
                rect = Rectangle.FromLTRB(rect.Left, rect.Top, rect2.Right, rect2.Bottom);
            }
            rect.Intersect(ClientRectangle);
            return rect.IsEmpty ? rect : RectangleToScreen(rect);
        }

        private void SetAccessibilityState(int row, int column, AccItemSettings settings, ref AccessibleStates state)
        {
            if (0 == (settings & AccItemSettings.BlankRow))
            {
                state |= AccessibleStates.Selectable | AccessibleStates.Focusable;
                // UNDONE: Also need to call ResolveSelectionColumn for multiselect trees
                // UNDONE: Is there any way to check IsDrawWithFocusWindow here?
                if (mySelectionColumn == column
                    && row == CurrentIndex
                    && Focused)
                {
                    state |= AccessibleStates.Focused;
                }
                // Report selection state if row is selected, and this is the current column
                // or MultiColumnHighlight is set, in which case selection will be drawn across the entire row.
                if ((mySelectionColumn == column || MultiColumnHighlight)
                    && IsSelected(row))
                {
                    state |= AccessibleStates.Selected;
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        private void DoAccessibilitySelectionChange(int row, int column, AccItemSettings settings, AccessibleSelection selectionState)
        {
            if (0 != (selectionState & AccessibleSelection.TakeSelection))
            {
                // TakeSelection may not be combined with any of these
                if (0 != (selectionState
                          & (AccessibleSelection.ExtendSelection | AccessibleSelection.AddSelection | AccessibleSelection.RemoveSelection)))
                {
                    throw new COMException(String.Empty, NativeMethods.E_INVALIDARG);
                }

                // set column appropriately
                if (mySelectionColumn != column
                    && (0 == (settings & (AccItemSettings.BlankRow | AccItemSettings.HiddenItem))))
                {
                    SetSelectionColumn(column, false);
                }

                if (GetStyleFlag(VTCStyleFlags.MultiSelect))
                {
                    ClearSelection(true);
                    SetSelected(row, true);
                    DoSelectionChanged();
                    FireWinEventsForSelection(false, false, ModifySelectionAction.None);
                    if (0 != (selectionState & AccessibleSelection.TakeFocus))
                    {
                        CaretIndex = AnchorIndex = row;
                    }
                }
                else
                {
                    // CurrentIndex setter sets selection and fires events in single select mode.
                    CurrentIndex = row;
                }
            }
            else if (0 != (selectionState & AccessibleSelection.ExtendSelection))
            {
                // ExtendSelection only valid for extended multi-select, and requires valid anchor.
                if (!GetStyleFlag(VTCStyleFlags.ExtendedMultiSelect)
                    || AnchorIndex < 0)
                {
                    throw new COMException(String.Empty, NativeMethods.E_INVALIDARG);
                }

                // set column appropriately
                if (mySelectionColumn != column
                    && (0 == (settings & (AccItemSettings.BlankRow | AccItemSettings.HiddenItem))))
                {
                    SetSelectionColumn(column, false);
                }

                // May use ExtendSelection to select or deselect a range.  If AddSelection/RemoveSelection are specified, use to determine whether we should select or deselect,
                // otherwise use current selected state of anchor index.
                bool select;
                if (0 != (selectionState & (AccessibleSelection.AddSelection | AccessibleSelection.RemoveSelection)))
                {
                    select = 0 != (selectionState & AccessibleSelection.AddSelection);
                }
                else
                {
                    select = IsSelected(AnchorIndex);
                }
                SetCurrentExtendedMultiSelectIndex(
                    row, true, false, select ? ModifySelectionAction.Select : ModifySelectionAction.Clear, select);
            }
            else if (0 != (selectionState & (AccessibleSelection.AddSelection | AccessibleSelection.RemoveSelection)))
            {
                // AddSelection/RemoveSelection only valid for multi-select, and may not be combined with any of these
                if (!GetStyleFlag(VTCStyleFlags.MultiSelect)
                    || (selectionState
                        != (selectionState
                            & (AccessibleSelection.AddSelection | AccessibleSelection.RemoveSelection | AccessibleSelection.TakeSelection))))
                {
                    throw new COMException(String.Empty, NativeMethods.E_INVALIDARG);
                }

                // set column appropriately
                if (mySelectionColumn != column
                    && (0 == (settings & (AccItemSettings.BlankRow | AccItemSettings.HiddenItem))))
                {
                    SetSelectionColumn(column, false);
                }

                var add = (0 != (selectionState & AccessibleSelection.AddSelection));
                SetSelected(row, add);
                DoSelectionChanged();
                FireWinEventsForSelection(false, true, add ? ModifySelectionAction.Select : ModifySelectionAction.Clear);

                if (0 != (selectionState & AccessibleSelection.TakeFocus))
                {
                    CaretIndex = AnchorIndex = row;
                }
            }
            else if (0 != (selectionState & AccessibleSelection.TakeFocus))
            {
                if (GetStyleFlag(VTCStyleFlags.MultiSelect))
                {
                    CaretIndex = AnchorIndex = row;
                    DoSelectionChanged();
                }
                else
                {
                    CurrentIndex = row; // TakeFocus behaves the same as TakeSelection in single-select mode.
                }
            }
            else
            {
                throw new COMException(String.Empty, NativeMethods.E_FAIL);
            }
        }

        /// <summary>
        ///     Set the value for the ImageDescriptions property if it was not set
        ///     in a constructor.
        /// </summary>
        /// <param name="descriptions">The new image descriptions</param>
        public void SetImageDescriptions(IList descriptions)
        {
            myImageDescriptions = VirtualTreeAccessibilityData.InterpretStringsValue(descriptions);
        }

        /// <summary>
        ///     A list of descriptions that describe the values in the default image list for the control.
        ///     Used to provide accessibility descriptions if you request PrimaryImageText
        ///     or PrimaryImageAndOverlays replacement fields.
        /// </summary>
        public IList ImageDescriptions
        {
            get { return myImageDescriptions; }
        }

        /// <summary>
        ///     Set the value for the StateImageDescriptions property.
        /// </summary>
        /// <param name="descriptions">The new image descriptions</param>
        public void SetStateImageDescriptions(IList descriptions)
        {
            myStateImageDescriptions = VirtualTreeAccessibilityData.InterpretStringsValue(descriptions);
        }

        /// <summary>
        ///     A list of descriptions that describe the values in the default image list for the control.
        ///     Used to provide accessibility descriptions if you request StateImageText replacement
        ///     fields.
        /// </summary>
        public IList StateImageDescriptions
        {
            get { return myStateImageDescriptions; }
        }

        /// <summary>
        ///     Set the value for the StateImageAcccessibleStates property.
        /// </summary>
        /// <param name="accessibleStates">The new accessible states</param>
        public void SetStateImageAccessibleStates(IList accessibleStates)
        {
            myStateImageAccessibleStates = VirtualTreeAccessibilityData.InterpretAccessibleStatesValue(accessibleStates);
        }

        /// <summary>
        ///     A list of AccessibleStates that correspond to the values in the default image list for the control.
        ///     These are OR'd in along with the other states the control supports natively, such as Expanded/Collapsed.
        /// </summary>
        /// <value></value>
        public IList StateImageAccessibleStates
        {
            get { return myStateImageAccessibleStates; }
        }

        private void GetAccessibilityTextFields(
            int row, int column, ref VirtualTreeItemInfo info, out string name, out string value, out string description,
            out string helpFile, out int helpId, out AccessibleStates state, out bool contentIsCheckBox)
        {
            var accData = info.Branch.GetAccessibilityData(info.Row, info.Column);
            helpFile = accData.HelpFile;
            if (helpFile == null)
            {
                helpFile = string.Empty;
            }
            helpId = accData.HelpContextId;
            AccessibilityReplacementField[] replacementFields;
            var locValue = info.Branch.GetAccessibleValue(info.Row, info.Column);
            var locName = accData.NameFormatString;
            if (locName == null
                || locName.Length == 0)
            {
                locName = info.Branch.GetAccessibleName(info.Row, info.Column);
                if (locName == null)
                {
                    locName = string.Empty;
                }
            }
            else
            {
                replacementFields = accData.NameReplacementFields as AccessibilityReplacementField[];
                if (replacementFields != null)
                {
                    locName = FormatReplacementFields(
                        row, column, ref info, locName, replacementFields, accData.ImageDescriptions as string[],
                        accData.StateImageDescriptions as string[]);
                }
            }
            var locDesc = accData.DescriptionFormatString;
            if (locDesc == null
                || locDesc.Length == 0)
            {
                locDesc = info.Branch.GetTipText(info.Row, info.Column, ToolTipType.Icon);
                if (locDesc == null
                    || locDesc.Length == 0)
                {
                    locDesc = locName;
                }
            }
            else
            {
                replacementFields = accData.DescriptionReplacementFields as AccessibilityReplacementField[];
                if (replacementFields != null)
                {
                    locDesc = FormatReplacementFields(
                        row, column, ref info, locDesc, replacementFields, accData.ImageDescriptions as string[],
                        accData.StateImageDescriptions as string[]);
                }
            }
            var locState = AccessibleStates.None;
            var displayData = info.Branch.GetDisplayData(
                info.Row, info.Column,
                new VirtualTreeDisplayDataMasks(
                    VirtualTreeDisplayMasks.Image | VirtualTreeDisplayMasks.ImageOverlays | VirtualTreeDisplayMasks.StateImage, 0));
            contentIsCheckBox = false;
            if (displayData.StateImageIndex >= 0)
            {
                var states = (displayData.StateImageList == null)
                                 ? myStateImageAccessibleStates
                                 : accData.StateImageAccessibleStates as AccessibleStates[];
                // Fixes 342626	- With Windows Eye installed, first component created throws benign "Object reference not set to an object"	
                // Added check that states is null.
                // v-matbir - 8/13/04
                if (states != null
                    && displayData.StateImageIndex < states.Length)
                {
                    locState = states[displayData.StateImageIndex];
                }

                // treat the content as a check box if the StandardCheckBoxes property is true, the state
                // image index is in the appropriate range, and there is no custom state imagelist.
                contentIsCheckBox = StandardCheckBoxes
                                    && displayData.StateImageIndex <= LastStandardCheckBoxImageIndex
                                    && displayData.StateImageList == null;
            }
            name = locName;
            value = locValue;
            description = locDesc;
            state = locState;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private string FormatReplacementFields(
            int row, int column, ref VirtualTreeItemInfo info, string formatString, AccessibilityReplacementField[] replacementFields,
            string[] imageDescriptions, string[] stateImageDescriptions)
        {
            var replacementCount = replacementFields.Length;
            var displayData = VirtualTreeDisplayData.Empty;
            var haveDisplayData = false;
            var fields = new string[replacementCount];
            var branch = info.Branch;
            var locRow = info.Row;
            var locCol = info.Column;
            StringBuilder builder = null;
            AccessibilityReplacementField curField;
            var culture = CultureInfo.CurrentUICulture;
            for (var i = 0; i < replacementCount; ++i)
            {
                curField = replacementFields[i];
                switch (curField)
                {
                    case AccessibilityReplacementField.ColumnHeader:
                        if (myHeaderBounds.HasHeaders)
                        {
                            fields[i] = myHeaderBounds.Headers[column].Text;
                        }
                        break;
                    case AccessibilityReplacementField.GlobalRow0:
                        fields[i] = row.ToString(culture);
                        break;
                    case AccessibilityReplacementField.GlobalRow1:
                        fields[i] = (row + 1).ToString(culture);
                        break;
                    case AccessibilityReplacementField.GlobalRowText0:
                        fields[i] = string.Format(culture, VirtualTreeStrings.GetString(VirtualTreeStrings.RowAccDesc), row);
                        break;
                    case AccessibilityReplacementField.GlobalRowText1:
                        fields[i] = string.Format(culture, VirtualTreeStrings.GetString(VirtualTreeStrings.RowAccDesc), row + 1);
                        break;
                    case AccessibilityReplacementField.GlobalColumn0:
                        fields[i] = column.ToString(culture);
                        break;
                    case AccessibilityReplacementField.GlobalColumn1:
                        fields[i] = (column + 1).ToString(culture);
                        break;
                    case AccessibilityReplacementField.GlobalColumnText0:
                        fields[i] = string.Format(culture, VirtualTreeStrings.GetString(VirtualTreeStrings.ColumnAccDesc), column);
                        break;
                    case AccessibilityReplacementField.GlobalColumnText1:
                        fields[i] = string.Format(culture, VirtualTreeStrings.GetString(VirtualTreeStrings.RowAccDesc), column + 1);
                        break;
                    case AccessibilityReplacementField.GlobalRowAndColumnText0:
                        fields[i] = string.Format(culture, VirtualTreeStrings.GetString(VirtualTreeStrings.RowColumnAccDesc), row, column);
                        break;
                    case AccessibilityReplacementField.GlobalRowAndColumnText1:
                        fields[i] = string.Format(
                            culture, VirtualTreeStrings.GetString(VirtualTreeStrings.RowColumnAccDesc), row + 1, column + 1);
                        break;
                    case AccessibilityReplacementField.LocalRow0:
                        fields[i] = locRow.ToString(culture);
                        break;
                    case AccessibilityReplacementField.LocalRow1:
                        fields[i] = (locRow + 1).ToString(culture);
                        break;
                    case AccessibilityReplacementField.LocalRowText0:
                        fields[i] = string.Format(culture, VirtualTreeStrings.GetString(VirtualTreeStrings.RowAccDesc), locRow);
                        break;
                    case AccessibilityReplacementField.LocalRowText1:
                        fields[i] = string.Format(culture, VirtualTreeStrings.GetString(VirtualTreeStrings.RowAccDesc), locRow + 1);
                        break;
                    case AccessibilityReplacementField.LocalRowOfTotal:
                        fields[i] = string.Format(
                            culture, VirtualTreeStrings.GetString(VirtualTreeStrings.RowOfTotalAccDesc), locRow + 1, branch.VisibleItemCount);
                        break;
                    case AccessibilityReplacementField.ChildRowCount:
                    case AccessibilityReplacementField.ChildRowCountText:
                        {
                            var descendantCount = Tree.GetDescendantItemCount(row, column, false, false);
                            if (curField == AccessibilityReplacementField.ChildRowCountText)
                            {
                                fields[i] = string.Format(
                                    culture, VirtualTreeStrings.GetString(VirtualTreeStrings.ChildRowNumberAccDesc), descendantCount);
                            }
                            else
                            {
                                fields[i] = descendantCount.ToString(culture);
                            }
                            break;
                        }
                    case AccessibilityReplacementField.ColumnCount:
                    case AccessibilityReplacementField.ColumnCountText:
                        {
                            var columnCount = 1;
                            if (myMctree != null)
                            {
                                var mcBranch = branch as IMultiColumnBranch;
                                if (mcBranch != null)
                                {
                                    // UNDONE: Column Permutation
                                    columnCount = Math.Min(
                                        myMctree.ColumnCount,
                                        (0 != (branch.Features & BranchFeatures.JaggedColumns))
                                            ? mcBranch.ColumnCount
                                            : mcBranch.GetJaggedColumnCount(locRow));
                                }
                            }
                            if (curField == AccessibilityReplacementField.ColumnCountText)
                            {
                                fields[i] = string.Format(
                                    culture, VirtualTreeStrings.GetString(VirtualTreeStrings.ColumnNumberAccDesc), columnCount);
                            }
                            else
                            {
                                fields[i] = columnCount.ToString(culture);
                            }
                            break;
                        }
                    case AccessibilityReplacementField.DisplayText:
                        fields[i] = branch.GetText(locRow, locCol);
                        break;
                    case AccessibilityReplacementField.ImageTipText:
                        fields[i] = branch.GetTipText(locRow, locCol, ToolTipType.Icon);
                        break;
                    case AccessibilityReplacementField.PrimaryImageText:
                    case AccessibilityReplacementField.PrimaryImageAndOverlaysText:
                    case AccessibilityReplacementField.StateImageText:
                        {
                            if (!haveDisplayData)
                            {
                                displayData = branch.GetDisplayData(
                                    locRow, locCol,
                                    new VirtualTreeDisplayDataMasks(
                                        VirtualTreeDisplayMasks.Image | VirtualTreeDisplayMasks.ImageOverlays
                                        | VirtualTreeDisplayMasks.StateImage, 0));
                            }
                            string[] descriptions;
                            int imageIndex;
                            int descriptionsLength;
                            switch (curField)
                            {
                                case AccessibilityReplacementField.PrimaryImageAndOverlaysText:
                                case AccessibilityReplacementField.PrimaryImageText:
                                    imageIndex = displayData.Image;
                                    if (imageIndex >= 0)
                                    {
                                        descriptions = (displayData.ImageList == null) ? myImageDescriptions : imageDescriptions;
                                        if (descriptions != null)
                                        {
                                            descriptionsLength = descriptions.Length;
                                            if (imageIndex < descriptionsLength)
                                            {
                                                fields[i] = descriptions[imageIndex];
                                            }
                                            if (curField == AccessibilityReplacementField.PrimaryImageAndOverlaysText)
                                            {
                                                imageIndex = displayData.OverlayIndex;
                                                if (imageIndex != -1)
                                                {
                                                    var separator = culture.TextInfo.ListSeparator;
                                                    var overlayIndices = displayData.OverlayIndices;
                                                    if (overlayIndices == null)
                                                    {
                                                        if (imageIndex < descriptionsLength)
                                                        {
                                                            fields[i] = string.Concat(fields[i], separator, descriptions[imageIndex]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (builder == null)
                                                        {
                                                            builder = new StringBuilder();
                                                        }
                                                        else
                                                        {
                                                            builder.Length = 0;
                                                        }
                                                        var startString = fields[i];
                                                        if (startString != null
                                                            && startString.Length != 0)
                                                        {
                                                            builder.Append(fields[i]);
                                                        }
                                                        else
                                                        {
                                                            // Keep track of this value so we can rip the leading
                                                            // separator later if the primary image had no text
                                                            startString = null;
                                                        }
                                                        // Use the index as a list of bits into the indices array.
                                                        // Do this typed if possible. It returns an IList to be CLS
                                                        // compliant. Note that this differs from the drawing routines
                                                        // for the images, which draw backwards, while this contatenates forwards.
                                                        int[] indicesArray;
                                                        IList<int> typedList;
                                                        var overlayIndex = imageIndex;
                                                        int indicesCount;
                                                        int curIndex;
                                                        int curBit;
                                                        indicesCount = overlayIndices.Count;
                                                        curBit = 1;
                                                        if (null != (indicesArray = overlayIndices as int[]))
                                                        {
                                                            for (curIndex = 0; curIndex < indicesCount; ++curIndex)
                                                            {
                                                                if (0 != (curBit & overlayIndex))
                                                                {
                                                                    imageIndex = indicesArray[curIndex];
                                                                    if (imageIndex < descriptionsLength)
                                                                    {
                                                                        builder.Append(separator);
                                                                        builder.Append(descriptions[imageIndex]);
                                                                    }
                                                                }
                                                                curBit <<= 1;
                                                            }
                                                        }
                                                        else if (null != (typedList = overlayIndices as IList<int>))
                                                        {
                                                            for (curIndex = 0; curIndex < indicesCount; ++curIndex)
                                                            {
                                                                if (0 != (curBit & overlayIndex))
                                                                {
                                                                    imageIndex = typedList[curIndex];
                                                                    if (imageIndex < descriptionsLength)
                                                                    {
                                                                        builder.Append(separator);
                                                                        builder.Append(descriptions[imageIndex]);
                                                                    }
                                                                }
                                                                curBit <<= 1;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            for (curIndex = 0; curIndex < indicesCount; ++curIndex)
                                                            {
                                                                if (0 != (curBit & overlayIndex))
                                                                {
                                                                    imageIndex = (int)overlayIndices[curIndex];
                                                                    if (imageIndex < descriptionsLength)
                                                                    {
                                                                        builder.Append(separator);
                                                                        builder.Append(descriptions[imageIndex]);
                                                                    }
                                                                }
                                                                curBit <<= 1;
                                                            }
                                                        }
                                                        if (startString == null)
                                                        {
                                                            // Strip leading separator if we had nothing to start with
                                                            var separatorLength = separator.Length;
                                                            fields[i] = builder.ToString(separatorLength, builder.Length - separatorLength);
                                                        }
                                                        else
                                                        {
                                                            fields[i] = builder.ToString();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case AccessibilityReplacementField.StateImageText:
                                    if (displayData.StateImageIndex >= 0)
                                    {
                                        descriptions = (displayData.StateImageList == null)
                                                           ? myStateImageDescriptions
                                                           : stateImageDescriptions;
                                        if (descriptions != null
                                            && displayData.StateImageIndex < descriptions.Length)
                                        {
                                            fields[i] = descriptions[displayData.StateImageIndex];
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
            return string.Format(culture, formatString, fields);
        }

        /// <summary>
        ///     Get the accessible object for this location in the control.
        /// </summary>
        /// <param name="row">The given row</param>
        /// <param name="column">The given column</param>
        /// <param name="nonPermutedColumn">
        ///     true if column is a non-permuted column value. If this
        ///     is true, then the column is always interpreted as if ColumnPermutation were not set.
        ///     However, note that the current ColumnPermutation is used to generate an accessibility
        ///     object that represents the state of the current tree.
        /// </param>
        /// <returns></returns>
        public AccessibleObject GetAccessibleObject(int row, int column, bool nonPermutedColumn)
        {
            var settings = AccItemSettings.None;
            var displayColumn = column;
            var nativeColumn = column;
            var colPerm = myColumnPermutation;
            if (myMctree != null)
            {
                if (colPerm != null)
                {
                    if (nonPermutedColumn)
                    {
                        displayColumn = colPerm.GetPermutedColumn(nativeColumn);
                        if (displayColumn == -1)
                        {
                            displayColumn = 0;
                            settings |= AccItemSettings.HiddenItem;
                        }
                    }
                    else
                    {
                        nativeColumn = colPerm.GetNativeColumn(displayColumn);
                    }
                }
                var expansion = myTree.GetBlankExpansion(row, displayColumn, colPerm);
                row = expansion.TopRow;
                displayColumn = expansion.AnchorColumn;
                if (displayColumn == VirtualTreeConstant.NullIndex)
                {
                    displayColumn = 0;
                }
                else
                {
                    nativeColumn = (colPerm != null) ? colPerm.GetNativeColumn(displayColumn) : displayColumn;
                }
            }
            var info = Tree.GetItemInfo(row, nativeColumn, true);
            Debug.Assert(!info.Blank);
            if (info.Blank)
            {
                return null;
            }
            else if (info.Column > 0)
            {
                return AccTreeRoot.GetColumnObject(this, row, nativeColumn - info.Column, info.Column, false);
            }
            else
            {
                return new AccPrimaryItem(this, row, displayColumn, nativeColumn, settings, ref info);
            }
        }

        /// <summary>
        ///     A bucket to hold settings information
        /// </summary>
        [Flags]
        private enum AccItemSettings
        {
            /// <summary>
            ///     No settings
            /// </summary>
            None = 0,

            /// <summary>
            ///     The item is representing a completely blank row
            /// </summary>
            BlankRow = 1,

            /// <summary>
            ///     The item is representing an item which is not currently visible.
            ///     This is used primarily to distinguish the 0 column when it is
            ///     displayed for structure purposes only.
            /// </summary>
            HiddenItem = 2,
        }

        /// <summary>
        ///     The top level accessibility object for the control
        /// </summary>
        private class AccTreeRoot : ControlAccessibleObject
        {
            private readonly VirtualTreeControl myCtl;

            public AccTreeRoot(VirtualTreeControl ctl)
                : base(ctl)
            {
                myCtl = ctl;
            }

            public override AccessibleObject GetChild(int index)
            {
                ITree tree;
                if (null != (tree = myCtl.Tree))
                {
                    if (null != tree.Root)
                    {
                        var row = tree.GetOffsetFromParent(-1, 0, index, false) - 1;
                        var info = tree.GetItemInfo(row, 0, true);
                        var settings = AccItemSettings.None;
                        var displayColumn = 0;
                        var colPerm = myCtl.myColumnPermutation;
                        if (colPerm != null)
                        {
                            displayColumn = colPerm.GetPermutedColumn(0);
                            if (displayColumn == -1)
                            {
                                displayColumn = 0;
                                settings |= AccItemSettings.HiddenItem;
                            }
                        }
                        return new AccPrimaryItem(
                            myCtl,
                            row,
                            displayColumn,
                            0,
                            settings,
                            ref info);
                    }
                }
                return null;
            }

            public override AccessibleObject HitTest(int x, int y)
            {
                var pointInClient = myCtl.PointToClient(new Point(x, y));
                var hitInfo = myCtl.HitInfo(pointInClient.X, pointInClient.Y);
                var onItemRegion = 0 != (hitInfo.HitTarget & VirtualTreeHitTargets.OnItemRegion);
                var onBlankItem = 0 != (hitInfo.HitTarget & VirtualTreeHitTargets.OnBlankItem);
                if (onItemRegion || onBlankItem)
                {
                    var row = hitInfo.Row;
                    var nativeColumn = hitInfo.NativeColumn;
                    var displayColumn = hitInfo.DisplayColumn;
                    var settings = AccItemSettings.None;
                    if (hitInfo.HitTarget == VirtualTreeHitTargets.OnBlankItem)
                    {
                        nativeColumn = displayColumn = 0;
                    }
                    var info = myCtl.Tree.GetItemInfo(row, nativeColumn, true);
                    if (info.Column > 0)
                    {
                        return GetColumnObject(myCtl, row, nativeColumn - info.Column, info.Column, false);
                    }
                    else
                    {
                        return new AccPrimaryItem(myCtl, row, displayColumn, nativeColumn, settings, ref info);
                    }
                }
                return this;
            }

            /// <summary>
            ///     Return either a Cell or Column object, depending on the branch settings
            /// </summary>
            /// <param name="ctl">The parent control</param>
            /// <param name="baseRow">The row coordinate</param>
            /// <param name="nativeBaseColumn">The column coordinate for the owning branch</param>
            /// <param name="localColumn">The native column offset</param>
            /// <param name="returnColumn">Return an AccColumn object if this is true, otherwise return the AccPrimaryItem directly</param>
            /// <returns>An AccSimpleCell or AccColumn</returns>
            public static AccessibleObject GetColumnObject(
                VirtualTreeControl ctl, int baseRow, int nativeBaseColumn, int localColumn, bool returnColumn)
            {
                // Note:  if this logic changes, corresponding change should be made to IsSimpleItem below.
                Debug.Assert(localColumn > 0);
                AccessibleObject retVal = null;
                var tree = ctl.Tree;
                var info = tree.GetItemInfo(baseRow, nativeBaseColumn, false);
                var mcBranch = info.Branch as IMultiColumnBranch;
                if (mcBranch != null)
                {
                    var simpleCell = true;
                    var useColInfo = false;
                    var nativeColumn = nativeBaseColumn + localColumn;
                    var displayColumn = nativeColumn;
                    var colPerm = ctl.myColumnPermutation;
                    var settings = AccItemSettings.None;
                    if (colPerm != null)
                    {
                        displayColumn = colPerm.GetPermutedColumn(nativeColumn);
                        if (displayColumn == -1)
                        {
                            displayColumn = 0;
                            settings |= AccItemSettings.HiddenItem;
                        }
                    }
                    var style = mcBranch.ColumnStyles(localColumn);
                    var colInfo = new VirtualTreeItemInfo();
                    style = mcBranch.ColumnStyles(localColumn);
                    switch (style)
                    {
                        case SubItemCellStyles.Expandable:
                            // Use a simple cell unless the item is expandable. An item
                            // in an expandable column cannot have subitems, so
                            // we will not need a column object for this purpose.
                            simpleCell = !info.Branch.IsExpandable(info.Row, localColumn);
                            break;
                        case SubItemCellStyles.Mixed:
                        case SubItemCellStyles.Complex:
                            colInfo = tree.GetItemInfo(baseRow, nativeColumn, true);
                            if (colInfo.Column == 0)
                            {
                                var columns = (0 == (info.Branch.Features & BranchFeatures.JaggedColumns))
                                                  ? mcBranch.ColumnCount
                                                  : mcBranch.GetJaggedColumnCount(info.Row);
                                // The data is provided by a different branch. The item is simple only
                                // if the branch has one unexpandable item and no sub items.
                                if (!(colInfo.Branch.VisibleItemCount == 1 &&
                                      !colInfo.Branch.IsExpandable(0, 0) &&
                                      (localColumn < columns ||
                                       (null != (colInfo.Branch as IMultiColumnBranch) &&
                                        0 == tree.GetSubItemCount(baseRow, nativeColumn)))))
                                {
                                    simpleCell = false;
                                }
                                useColInfo = true;
                            }
                            else if (style == SubItemCellStyles.Mixed)
                            {
                                goto case SubItemCellStyles.Expandable;
                            }
                            break;
                    }
                    if (simpleCell)
                    {
                        if (useColInfo)
                        {
                            retVal = new AccSimpleCell(ctl, baseRow, displayColumn, nativeColumn, settings, ref colInfo);
                        }
                        else
                        {
                            var cellInfo = tree.GetItemInfo(baseRow, nativeColumn, true);
                            if (!cellInfo.Blank)
                            {
                                retVal = new AccSimpleCell(ctl, baseRow, displayColumn, nativeColumn, settings, ref cellInfo);
                            }
                        }
                    }
                    else if (returnColumn)
                    {
                        if (useColInfo)
                        {
                            retVal = new AccColumn(ctl, baseRow, displayColumn, nativeColumn, settings, ref colInfo);
                        }
                        else
                        {
                            var cellInfo = tree.GetItemInfo(baseRow, nativeBaseColumn + localColumn, true);
                            retVal = new AccColumn(ctl, baseRow, displayColumn, nativeColumn, settings, ref cellInfo);
                        }
                    }
                    else
                    {
                        if (useColInfo)
                        {
                            retVal = new AccPrimaryItem(ctl, baseRow, displayColumn, nativeColumn, settings, ref colInfo);
                        }
                        else
                        {
                            var cellInfo = tree.GetItemInfo(baseRow, nativeBaseColumn + localColumn, true);
                            retVal = new AccPrimaryItem(ctl, baseRow, displayColumn, nativeColumn, settings, ref cellInfo);
                        }
                    }
                }
                return retVal;
            }

            /// <summary>
            ///     Helper routine to figure out if a given column object is a simple cell or a complex item.
            /// </summary>
            public static bool IsSimpleItem(VirtualTreeControl ctl, int baseRow, int nativeBaseColumn, int localColumn)
            {
                if (localColumn == 0)
                {
                    return false; // items in the first column are never simple
                }
                var simpleCell = true;
                var tree = ctl.Tree;
                var info = tree.GetItemInfo(baseRow, nativeBaseColumn, false);
                var mcBranch = info.Branch as IMultiColumnBranch;
                if (mcBranch != null)
                {
                    var nativeColumn = nativeBaseColumn + localColumn;
                    var style = mcBranch.ColumnStyles(localColumn);
                    var colInfo = new VirtualTreeItemInfo();
                    switch (style)
                    {
                        case SubItemCellStyles.Expandable:
                            // Use a simple cell unless the item is expandable. An item
                            // in an expandable column cannot have subitems, so
                            // we will not need a column object for this purpose.
                            simpleCell = !info.Branch.IsExpandable(info.Row, localColumn);
                            break;
                        case SubItemCellStyles.Mixed:
                        case SubItemCellStyles.Complex:
                            colInfo = tree.GetItemInfo(baseRow, nativeColumn, true);
                            if (colInfo.Column == 0)
                            {
                                var columns = (0 == (info.Branch.Features & BranchFeatures.JaggedColumns))
                                                  ? mcBranch.ColumnCount
                                                  : mcBranch.GetJaggedColumnCount(info.Row);
                                // The data is provided by a different branch. The item is simple only
                                // if the branch has one unexpandable item and no sub items.
                                if (!(colInfo.Branch.VisibleItemCount == 1 &&
                                      !colInfo.Branch.IsExpandable(0, 0) &&
                                      (localColumn < columns ||
                                       (null != (colInfo.Branch as IMultiColumnBranch) &&
                                        0 == tree.GetSubItemCount(baseRow, nativeColumn)))))
                                {
                                    simpleCell = false;
                                }
                            }
                            else if (style == SubItemCellStyles.Mixed)
                            {
                                goto case SubItemCellStyles.Expandable;
                            }
                            break;
                    }
                }
                return simpleCell;
            }

            public override int GetChildCount()
            {
                return myCtl.Tree.Root.VisibleItemCount;
            }
        }

        /// <summary>
        ///     The base class for creating cells and outline items. Holds row, column,
        ///     width, height, and text information. The role and children are left
        ///     up to the override.
        /// </summary>
        private abstract class AccItem : AccessibleObject
        {
            protected readonly VirtualTreeControl myCtl;
            protected readonly string myName;
            protected readonly string myValue;
            protected readonly string myDescription;
            protected readonly string myHelpFile;
            protected readonly int myHelpId;
            protected readonly int myRow;
            protected readonly int myDisplayColumn;
            protected readonly int myNativeColumn;
            protected readonly int myRowHeight;
            protected readonly int myColumnWidth;
            protected readonly int myLevel;
            protected readonly AccItemSettings mySettings;
            protected readonly AccessibleStates myState;
            protected readonly bool myCheckBoxContent;

            /// <summary>
            ///     Construct the base class of an accessibility item for this object
            /// </summary>
            /// <param name="ctl">The parent control</param>
            /// <param name="row">The current row</param>
            /// <param name="displayColumn">The display column.</param>
            /// <param name="nativeColumn">The native column.</param>
            /// <param name="settings">Miscellaneous settings for the object</param>
            /// <param name="info">The info for this cell. Gives the info for column 0 for a blank row.</param>
            public AccItem(
                VirtualTreeControl ctl,
                int row,
                int displayColumn,
                int nativeColumn,
                AccItemSettings settings,
                ref VirtualTreeItemInfo info)
            {
                // Blank row is calculated explicitly, ignore it if it is passed in
                settings = (AccItemSettings)((int)settings & ~(int)(AccItemSettings.BlankRow));

                var tree = ctl.Tree;

                // Get information about the branch and tree state at this
                // position in the tree.
                Debug.Assert(!info.Blank); // Adjust row and column before creating here to take care of blanks
                if (displayColumn < 0)
                {
                    displayColumn = 0;
                }
                if (nativeColumn < 0)
                {
                    nativeColumn = 0;
                }

                if (ctl.MultiColumnTree != null)
                {
                    // Adjust the passed in row and column so that it is on a non-blank item,
                    // and find out how far the blank range extends from that anchor point.
                    // Note that even a single-column branch can have blank columns to the
                    // right (or left, with permutations), so we always need to do GetBlankExpansion.
                    var expansion = tree.GetBlankExpansion(row, displayColumn, ctl.myColumnPermutation);
                    row = expansion.TopRow;
                    myRowHeight = expansion.Height;
                    if (expansion.AnchorColumn != -1)
                    {
                        displayColumn = expansion.AnchorColumn;
                        myColumnWidth = expansion.RightColumn - displayColumn + 1; // Ignore blanks to the left, don't use Width
                    }
                    else
                    {
                        settings |= AccItemSettings.BlankRow;
                        myColumnWidth = expansion.Width;
                    }
                }
                else
                {
                    myRowHeight = myColumnWidth = 1;
                }

                // Cache the information we need
                myCtl = ctl;
                ctl.GetAccessibilityTextFields(
                    row, displayColumn, ref info, out myName, out myValue, out myDescription, out myHelpFile, out myHelpId, out myState,
                    out myCheckBoxContent);
                myRow = row;
                myDisplayColumn = displayColumn;
                myNativeColumn = nativeColumn;
                mySettings = settings;
                myLevel = info.Level;
                if (info.Expandable)
                {
                    if (info.Expanded)
                    {
                        myState |= AccessibleStates.Expanded;
                    }
                    else
                    {
                        myState |= AccessibleStates.Collapsed;
                    }
                }
                if (0 != (settings & AccItemSettings.HiddenItem))
                {
                    myState |= AccessibleStates.Invisible;
                }
                else if (Rectangle.Empty == myCtl.GetAccessibilityLocation(myRow, myDisplayColumn, myRowHeight, myColumnWidth))
                {
                    myState |= (AccessibleStates.Invisible | AccessibleStates.Offscreen);
                }
                ctl.SetAccessibilityState(row, displayColumn, settings, ref myState);
            }

            public override string Name
            {
                get { return myName; }
            }

            public override string Description
            {
                get { return myDescription; }
            }

            public override int GetHelpTopic(out string fileName)
            {
                fileName = myHelpFile;
                return myHelpId;
            }

            public override Rectangle Bounds
            {
                get
                {
                    if (0 != (mySettings & AccItemSettings.HiddenItem))
                    {
                        return Rectangle.Empty;
                    }
                    return myCtl.GetAccessibilityLocation(myRow, myDisplayColumn, myRowHeight, myColumnWidth);
                }
            }

            public override string Value
            {
                get { return (String.IsNullOrEmpty(myValue) ? myLevel.ToString(CultureInfo.InvariantCulture) : myValue); }
            }

            public override AccessibleObject Parent
            {
                get
                {
                    var tree = myCtl.Tree;
                    var colPerm = myCtl.myColumnPermutation;
                    if (colPerm != null
                        && myDisplayColumn >= colPerm.VisibleColumnCount)
                    {
                        // Protects against stale accessibility object
                        return myCtl.AccessibilityObject;
                    }
                    var target = VirtualTreeCoordinate.Invalid;
                    var settings = AccItemSettings.None;
                    var nativeColumn = 0;
                    if (0 == (mySettings & AccItemSettings.HiddenItem))
                    {
                        target = tree.GetNavigationTarget(TreeNavigation.ComplexParent, myRow, myDisplayColumn, colPerm);
                    }
                    if (target.IsValid)
                    {
                        nativeColumn = target.Column;
                        if (colPerm != null)
                        {
                            nativeColumn = colPerm.GetNativeColumn(nativeColumn);
                        }
                    }
                    else if (colPerm != null)
                    {
                        // See if we can get a hidden parent
                        target = tree.GetNavigationTarget(TreeNavigation.ComplexParent, myRow, myNativeColumn, null);
                        if (target.IsValid)
                        {
                            settings |= AccItemSettings.HiddenItem;
                            nativeColumn = target.Column;
                            target.Column = colPerm.GetPermutedColumn(nativeColumn);
                            if (target.Column == -1)
                            {
                                target.Column = 0;
                            }
                        }
                    }

                    if (target.IsValid)
                    {
                        var info = myCtl.Tree.GetItemInfo(target.Row, nativeColumn, true);
                        return new AccPrimaryItem(myCtl, target.Row, target.Column, nativeColumn, settings, ref info);
                    }
                    else
                    {
                        return myCtl.AccessibilityObject;
                    }
                }
            }

            public override AccessibleObject HitTest(int x, int y)
            {
                return this;
            }

            public override AccessibleStates State
            {
                get { return myState; }
            }

            public override string DefaultAction
            {
                get
                {
                    if (0 != (myState & (AccessibleStates.Expanded | AccessibleStates.Collapsed)))
                    {
                        return (0 == (myState & AccessibleStates.Expanded))
                                   ? VirtualTreeStrings.GetString(VirtualTreeStrings.DefActionExpandAccDesc)
                                   : VirtualTreeStrings.GetString(VirtualTreeStrings.DefActionCollapseAccDesc);
                    }
                    else
                    {
                        return base.DefaultAction;
                    }
                }
            }

            public override void DoDefaultAction()
            {
                if (0 != (myState & (AccessibleStates.Expanded | AccessibleStates.Collapsed)))
                {
                    myCtl.Tree.ToggleExpansion(myRow, myNativeColumn);
                }
                else
                {
                    base.DoDefaultAction();
                }
            }

            public override AccessibleObject Navigate(AccessibleNavigation navdir)
            {
                TreeNavigation navAction;
                switch (navdir)
                {
                    case AccessibleNavigation.Down:
                        navAction = TreeNavigation.NextSibling;
                        break;
                    case AccessibleNavigation.FirstChild:
                        navAction = TreeNavigation.FirstChild;
                        break;
                    case AccessibleNavigation.LastChild:
                        navAction = TreeNavigation.LastChild;
                        break;
                    case AccessibleNavigation.Up:
                        navAction = TreeNavigation.PreviousSibling;
                        break;
                    case AccessibleNavigation.Left:
                        navAction = TreeNavigation.Left;
                        break;
                    case AccessibleNavigation.Next:
                        navAction = TreeNavigation.Down;
                        break;
                    case AccessibleNavigation.Previous:
                        navAction = TreeNavigation.Up;
                        break;
                    case AccessibleNavigation.Right:
                        navAction = TreeNavigation.Right;
                        break;
                    default:
                        return null;
                }
                AccessibleObject retVal = null;
                var tree = myCtl.Tree;
                var colPerm = myCtl.ColumnPermutation;
                if (colPerm != null
                    && myDisplayColumn >= colPerm.VisibleColumnCount)
                {
                    // Handle a stale object so the rest of this doesn't crash
                    return null;
                }
                var target = tree.GetNavigationTarget(navAction, myRow, myDisplayColumn, colPerm);
                if (target.IsValid)
                {
                    var nativeColumn = target.Column;
                    var settings = AccItemSettings.None;
                    if (colPerm != null)
                    {
                        nativeColumn = colPerm.GetNativeColumn(nativeColumn);
                        if (colPerm.GetPermutedColumn(nativeColumn) == -1)
                        {
                            settings |= AccItemSettings.HiddenItem;
                        }
                    }
                    var newInfo = tree.GetItemInfo(target.Row, nativeColumn, true);
                    retVal = new AccPrimaryItem(myCtl, target.Row, nativeColumn, target.Column, settings, ref newInfo);
                }
                return retVal;
            }

            public override void Select(AccessibleSelection flags)
            {
                myCtl.DoAccessibilitySelectionChange(myRow, myDisplayColumn, mySettings, flags);
            }
        }

        /// <summary>
        ///     The accessibility object for items in a column of the tree
        ///     grid that supports expansion. Used whether or not the item
        ///     itself is expandable.
        /// </summary>
        private class AccPrimaryItem : AccItem
        {
            private readonly int myChildItems;
            private readonly int myNativeChildColumns;
            private readonly int myDisplayedChildColumns;

            public AccPrimaryItem(
                VirtualTreeControl ctl, int row, int displayColumn, int nativeColumn, AccItemSettings settings, ref VirtualTreeItemInfo info)
                :
                    base(ctl, row, displayColumn, nativeColumn, settings, ref info)
            {
                if (ctl.MultiColumnTree != null)
                {
                    var mcBranch = info.Branch as IMultiColumnBranch;
                    if (mcBranch != null)
                    {
                        // If Column is not 0, then we're looking at an expandable cell. Expandable
                        // cells cannot have multiple columns.
                        if (info.Column == 0)
                        {
                            var childColumnCount = (0 == (info.Branch.Features & BranchFeatures.JaggedColumns))
                                                       ? mcBranch.ColumnCount
                                                       : mcBranch.GetJaggedColumnCount(info.Row);

                            myNativeChildColumns = myDisplayedChildColumns = childColumnCount - 1;

                            var colPerm = ctl.myColumnPermutation;
                            if (colPerm != null)
                            {
                                // We have to ask on a per-column basis if the given column is
                                // currently displayed.
                                var columnBound = nativeColumn + childColumnCount;
                                childColumnCount = 0;
                                for (var i = nativeColumn + 1; i < columnBound; ++i)
                                {
                                    if (colPerm.GetPermutedColumn(i) != -1)
                                    {
                                        ++childColumnCount;
                                    }
                                }
                                myDisplayedChildColumns = childColumnCount;
                            }
                        }
                    }
                }
                if (ctl.Tree.IsExpanded(myRow, myNativeColumn))
                {
                    myChildItems = ctl.Tree.GetExpandedBranch(myRow, myNativeColumn).Branch.VisibleItemCount;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    if (myCheckBoxContent)
                    {
                        return AccessibleRole.CheckButton;
                    }
                    else
                    {
                        return AccessibleRole.OutlineItem;
                    }
                }
            }

            public override AccessibleObject GetChild(int index)
            {
                var adjust = myDisplayedChildColumns;
                var tree = myCtl.Tree;
                if (index < adjust)
                {
                    var localColumn = index + 1;
                    var colPerm = myCtl.myColumnPermutation;
                    if (colPerm != null)
                    {
                        // With a column permutation, we want the children to be in
                        // left to right order of what is displayed, not the native order.
                        // Walk the column permutation until we find the correct index.
                        var nativeLowerBound = myNativeColumn + 1;
                        var nativeUpperBound = myNativeColumn + myNativeChildColumns;
                        var columnCount = colPerm.VisibleColumnCount;
                        var candidate = myNativeColumn + 1;
                        ;
                        if (colPerm.PreferLeftBlanks)
                        {
                            // Run the children right to left
                            for (var i = columnCount - 1; i >= 0; --i)
                            {
                                candidate = colPerm.GetNativeColumn(i);
                                if (candidate >= nativeLowerBound
                                    && candidate <= nativeUpperBound)
                                {
                                    --localColumn;
                                    if (localColumn == 0)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Run the children left to right
                            for (var i = 0; i < columnCount; ++i)
                            {
                                candidate = colPerm.GetNativeColumn(i);
                                if (candidate >= nativeLowerBound
                                    && candidate <= nativeUpperBound)
                                {
                                    --localColumn;
                                    if (localColumn == 0)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        localColumn = candidate - myNativeColumn;
                    }
                    return AccTreeRoot.GetColumnObject(myCtl, myRow, myNativeColumn, localColumn, true);
                }
                else
                {
                    var childRow = myRow + tree.GetOffsetFromParent(myRow, myNativeColumn, index - adjust, false);
                    var childInfo = tree.GetItemInfo(childRow, myNativeColumn, true);
                    return new AccPrimaryItem(myCtl, childRow, myDisplayColumn, myNativeColumn, mySettings, ref childInfo);
                }
            }

            public override int GetChildCount()
            {
                return myDisplayedChildColumns + myChildItems;
            }
        }

        /// <summary>
        ///     The accessibility object for an item in a simple secondary column.
        /// </summary>
        private class AccSimpleCell : AccItem
        {
            private readonly string value;

            public AccSimpleCell(
                VirtualTreeControl ctl, int row, int displayColumn, int nativeColumn, AccItemSettings settings, ref VirtualTreeItemInfo info)
                :
                    base(ctl, row, displayColumn, nativeColumn, settings, ref info)
            {
                value = info.Branch.GetAccessibleValue(info.Row, info.Column);
            }

            public override AccessibleRole Role
            {
                get
                {
                    if (myCheckBoxContent)
                    {
                        return AccessibleRole.CheckButton;
                    }
                    else
                    {
                        return AccessibleRole.Cell;
                    }
                }
            }

            public override string Value
            {
                get { return value; }
            }
        }

        /// <summary>
        ///     The accessibility object for any secondary column that is
        ///     not a simple cell.
        /// </summary>
        private class AccColumn : AccessibleObject
        {
            private readonly VirtualTreeControl myCtl;
            private readonly int myRow;
            private readonly int myDisplayColumn;
            private readonly int myNativeColumn;
            private readonly AccItemSettings mySettings;
            private readonly int myRowHeight;
            private readonly int myColumnWidth;
            private readonly int myChildCount;
            // row and column here are the row and column of the parent item.
            // The local column (relative to the parent branch) can be retrieved from the VirtualTreeItemInfo.
            public AccColumn(
                VirtualTreeControl ctl, int row, int displayColumn, int nativeColumn, AccItemSettings settings, ref VirtualTreeItemInfo info)
            {
                // Blank row is calculated explicitly, ignore it if it is passed in
                settings = (AccItemSettings)((int)settings & ~(int)(AccItemSettings.BlankRow));
                myCtl = ctl;
                var tree = ctl.Tree;
                var lastRowAdjust = 0;
                if (info.Column == 0)
                {
                    // Looking at a complex column
                    myChildCount = info.Branch.VisibleItemCount;
                    lastRowAdjust = -1;
                }
                else
                {
                    // This is a wrapper around a single-celled column. It can
                    // only have one item in it. Flag with a -1 to distinguish it
                    // from a complex column
                    myChildCount = -1;
                }

                // Expand column to include blanks to the bottom and right
                var expansion =
                    tree.GetBlankExpansion(
                        row + lastRowAdjust + tree.GetDescendantItemCount(row, nativeColumn, true, myChildCount != -1), displayColumn,
                        myCtl.myColumnPermutation);
                myRow = row;
                myRowHeight = expansion.BottomRow - row + 1;
                displayColumn = expansion.AnchorColumn;
                var colPerm = ctl.myColumnPermutation;
                if (displayColumn == VirtualTreeConstant.NullIndex)
                {
                    settings |= AccItemSettings.BlankRow;
                    displayColumn = 0;
                    myColumnWidth = expansion.Width;
                }
                else
                {
                    nativeColumn = (colPerm != null) ? colPerm.GetNativeColumn(displayColumn) : displayColumn;
                    myColumnWidth = expansion.RightColumn - displayColumn + 1;
                }
                myDisplayColumn = displayColumn;
                myNativeColumn = nativeColumn;
                mySettings = settings;
            }

            public override AccessibleRole Role
            {
                get { return AccessibleRole.Column; }
            }

            public override AccessibleStates State
            {
                get
                {
                    var retVal = base.State;
                    if (0 != (mySettings & AccItemSettings.HiddenItem))
                    {
                        retVal |= AccessibleStates.Invisible;
                    }
                    return retVal;
                }
            }

            public override Rectangle Bounds
            {
                get
                {
                    if (0 != (mySettings & AccItemSettings.HiddenItem))
                    {
                        return Rectangle.Empty;
                    }
                    return myCtl.GetAccessibilityLocation(myRow, myDisplayColumn, myRowHeight, myColumnWidth);
                }
            }

            public override AccessibleObject Parent
            {
                get
                {
                    var tree = myCtl.Tree;
                    var colPerm = myCtl.myColumnPermutation;
                    var target = VirtualTreeCoordinate.Invalid;
                    var settings = AccItemSettings.None;
                    var nativeColumn = 0;
                    if (0 == (mySettings & AccItemSettings.HiddenItem))
                    {
                        target = tree.GetNavigationTarget(TreeNavigation.ComplexParent, myRow, myDisplayColumn, colPerm);
                    }
                    if (target.IsValid)
                    {
                        nativeColumn = target.Column;
                        if (colPerm != null)
                        {
                            nativeColumn = colPerm.GetNativeColumn(nativeColumn);
                        }
                    }
                    else if (colPerm != null)
                    {
                        // See if we can get a hidden parent
                        target = tree.GetNavigationTarget(TreeNavigation.ComplexParent, myRow, myNativeColumn, null);
                        if (target.IsValid)
                        {
                            settings |= AccItemSettings.HiddenItem;
                            nativeColumn = target.Column;
                            target.Column = colPerm.GetPermutedColumn(nativeColumn);
                            if (target.Column == -1)
                            {
                                target.Column = 0;
                            }
                        }
                    }
                    if (target.IsValid)
                    {
                        var info = myCtl.Tree.GetItemInfo(target.Row, nativeColumn, true);
                        return new AccPrimaryItem(myCtl, target.Row, target.Column, nativeColumn, settings, ref info);
                    }
                    else
                    {
                        return myCtl.AccessibilityObject;
                    }
                }
            }

            public override AccessibleObject HitTest(int x, int y)
            {
                // Return null here to use the hit testing from the parent object
                return null;
            }

            public override AccessibleObject GetChild(int index)
            {
                var childRow = myRow;
                if (index > 0)
                {
                    childRow += myCtl.Tree.GetOffsetFromParent(myRow, myNativeColumn, index, true) - 1;
                }
                var info = myCtl.Tree.GetItemInfo(childRow, myNativeColumn, true);
                // This item will have the same native and display column, and will never be a blank row.
                return new AccPrimaryItem(myCtl, childRow, myDisplayColumn, myNativeColumn, mySettings, ref info);
            }

            public override int GetChildCount()
            {
                return (myChildCount == -1) ? 1 : myChildCount;
            }
        }

        #endregion // Accessibility Routines
    }
}
