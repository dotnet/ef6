// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Common;

    #region IVirtualTreeInPlaceControl interface

    /// <summary>
    ///     The coordination interface that needs to be implemented
    ///     by a control that wants to be in-place activated in the
    ///     tree control. Controls should implement this interface
    ///     by defering to the implementation of the members on the
    ///     IInPlaceControlDefer interface to the VirtualTreeControl.InPlaceControlHelper,
    ///     and implementing the remaining methods themselves class.
    /// </summary>
    internal interface IVirtualTreeInPlaceControl : IVirtualTreeInPlaceControlDefer
    {
        /// <summary>
        ///     Contains control-specific code to select all text in the window.
        /// </summary>
        void SelectAllText();

        /// <summary>
        ///     Contains control-specific code to set the SelectionStart property
        ///     on the control.
        /// </summary>
        int SelectionStart { get; set; }

        /// <summary>
        ///     Contains control-specific code to set the MaxTextLength property.
        /// </summary>
        int MaxTextLength { get; set; }

        /// <summary>
        ///     Get the formatting rectangle of the text. See EM_GETRECT for the definition
        ///     of a formatting rectangle;
        /// </summary>
        Rectangle FormattingRectangle { get; }

        /// <summary>
        ///     Get the amount of extra space that the control should show to the right of the
        ///     text for editing. This can be used to hold a cursor in a live edit box, or a dropdown
        ///     button in a combo box.
        /// </summary>
        int ExtraEditWidth { get; }
    }

    #endregion

    #region IVirtualTreeInPlaceControlDefer

    /// <summary>
    ///     The portion of the IInPlaceControl interface that is
    ///     implemented on the VirtualTreeControl.InPlaceControlHelper class.
    /// </summary>
    internal interface IVirtualTreeInPlaceControlDefer
    {
        /// <summary>
        ///     The tree control that activates this object.
        /// </summary>
        VirtualTreeControl Parent { get; set; }

        /// <summary>
        ///     The control that implements this interface
        /// </summary>
        Control InPlaceControl { get; }

        /// <summary>
        ///     Flags governing control appearance/behavior
        /// </summary>
        VirtualTreeInPlaceControls Flags { get; set; }

        /// <summary>
        ///     The windows message (WM_LBUTTONDOWN) that launched the edit
        ///     control. Use 0 for for a timer, selection change, or explicit
        ///     launch. Used to support mouse behavior while a control is in-place
        ///     activating in response to a mouse click.
        /// </summary>
        int LaunchedByMessage { get; set; }

        /// <summary>
        ///     Value indicates whether the in-place edit control is currently dirty.
        ///     At commit time, only a dirty in-place edit control generates calls
        ///     to IBranch.CommitLabelEdit or a custom commit delegate.
        /// </summary>
        bool Dirty { get; set; }
    }

    #endregion

    #region InPlaceControl flags

    /// <summary>
    ///     Flags that control how the tree control interacts with an
    ///     in-place activated control.
    /// </summary>
    [Flags]
    internal enum VirtualTreeInPlaceControls
    {
        /// <summary>
        ///     Set this flag to specify that the control should be sized based
        ///     on the label text. By default, the control is sized based on the
        ///     size of the activating cell.
        /// </summary>
        SizeToText = 1,

        /// <summary>
        ///     Set this flag to specify that the VirtualTreeControl should call Dispose()
        ///     on the in place control when it is no longer in use. If it is not set, then
        ///     it is up to the branch to dispose the control. Clearing this flag allows the
        ///     branch to reuse a single control instance.
        /// </summary>
        DisposeControl = 2,

        /// <summary>
        ///     By default, the text behind an in-place activated control is not drawn when the
        ///     control is painted. This enables the control to be smaller than the text without
        ///     having strange drawing happening in the background. Setting this flag forces the
        ///     text to draw, enabling transparent controls. For example, this enables a dropdown
        ///     control to show a button without providing an edit field.
        /// </summary>
        DrawItemText = 4,

        /// <summary>
        ///     If the in-place control does not need keystrokes itself, then it can set this flag
        ///     to force the OnKeyDown and OnKeyPress callbacks in the inplace helper to forward
        ///     the keystrokes to the tree control.
        /// </summary>
        ForwardKeyEvents = 8,
    }

    #endregion

    /// <summary>
    ///     The default class to use an inplace label edit.
    /// </summary>
    [SuppressMessage("Whitehorse.CustomRules", "WH03:WinFormControlCatchUnhandledExceptions",
        Justification = "All but critical exceptions are caught.")]
    internal class VirtualTreeInPlaceEditControl : TextBox, IVirtualTreeInPlaceControl
    {
        #region Boilerplate InPlaceControl code

        private readonly VirtualTreeInPlaceControlHelper myInPlaceHelper;

        VirtualTreeControl IVirtualTreeInPlaceControlDefer.Parent
        {
            get { return VirtualTreeInPlaceControlDeferParent; }
            set { VirtualTreeInPlaceControlDeferParent = value; }
        }

        /// <summary>
        ///     The tree control that activates this object.
        /// </summary>
        protected VirtualTreeControl VirtualTreeInPlaceControlDeferParent
        {
            get { return myInPlaceHelper.Parent; }
            set { myInPlaceHelper.Parent = value; }
        }

        Control IVirtualTreeInPlaceControlDefer.InPlaceControl
        {
            get { return InPlaceControl; }
        }

        /// <summary>
        ///     The control that implements this interface
        /// </summary>
        protected Control InPlaceControl
        {
            get { return myInPlaceHelper.InPlaceControl; }
        }

        VirtualTreeInPlaceControls IVirtualTreeInPlaceControlDefer.Flags
        {
            get { return Flags; }
            set { Flags = value; }
        }

        /// <summary>
        ///     Flags governing control appearance/behavior
        /// </summary>
        protected VirtualTreeInPlaceControls Flags
        {
            get { return myInPlaceHelper.Flags; }
            set { myInPlaceHelper.Flags = value; }
        }

        /// <summary>
        ///     Windows message used to launch the control. Defers to helper.
        /// </summary>
        int IVirtualTreeInPlaceControlDefer.LaunchedByMessage
        {
            get { return LaunchedByMessage; }
            set { LaunchedByMessage = value; }
        }

        /// <summary>
        ///     The windows message (WM_LBUTTONDOWN) that launched the edit
        ///     control. Use 0 for for a timer, selection change, or explicit
        ///     launch. Used to support mouse behavior while a control is in-place
        ///     activating in response to a mouse click.
        /// </summary>
        protected int LaunchedByMessage
        {
            get { return myInPlaceHelper.LaunchedByMessage; }
            set { myInPlaceHelper.LaunchedByMessage = value; }
        }

        bool IVirtualTreeInPlaceControlDefer.Dirty
        {
            get { return Dirty; }
            set { Dirty = value; }
        }

        /// <summary>
        ///     Dirty state of the in-place control.  Defers to helper.
        /// </summary>
        protected bool Dirty
        {
            get { return myInPlaceHelper.Dirty; }
            set { myInPlaceHelper.Dirty = value; }
        }

        /// <summary>
        ///     Notify the inplace helper that a key is down if
        ///     the event was not handled.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!e.Handled)
            {
                e.Handled = myInPlaceHelper.OnKeyDown(e);
            }
        }

        /// <summary>
        ///     Notify the inplace helper that a key was pressed if the
        ///     key was not handled.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (!e.Handled)
            {
                e.Handled = myInPlaceHelper.OnKeyPress(e);
            }
        }

        /// <summary>
        ///     Notify the inplace helper that the text changed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTextChanged(EventArgs e)
        {
            myInPlaceHelper.OnTextChanged();
            base.OnTextChanged(e);
        }

        /// <summary>
        ///     Notify the inplace helper that focus was lost
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostFocus(EventArgs e)
        {
            myInPlaceHelper.OnLostFocus();
            base.OnLostFocus(e);
        }

        #endregion // Boilerplace InPlaceControl code

        #region Custom InPlaceControl callbacks

        void IVirtualTreeInPlaceControl.SelectAllText()
        {
            SelectAllText();
        }

        /// <summary>
        ///     Contains control-specific code to select all text in the window.
        /// </summary>
        protected void SelectAllText()
        {
            // Note that myEdit.SelectAll() is not quite the same as the following code
            var hwndEdit = Handle;
            NativeMethods.SendMessage(hwndEdit, NativeMethods.EM_SETSEL, -1, -1); // move to the end
            NativeMethods.SendMessage(hwndEdit, NativeMethods.EM_SETSEL, 0, -1); // select all text
        }

        int IVirtualTreeInPlaceControl.SelectionStart
        {
            get { return SelectionStart; }
            set { SelectionStart = value; }
        }

        int IVirtualTreeInPlaceControl.MaxTextLength
        {
            get { return MaxTextLength; }
            set { MaxTextLength = value; }
        }

        /// <summary>
        ///     Contains control-specific code to set the MaxTextLength property.
        /// </summary>
        protected int MaxTextLength
        {
            get { return MaxLength; }
            set { MaxLength = value; }
        }

        Rectangle IVirtualTreeInPlaceControl.FormattingRectangle
        {
            get { return FormattingRectangle; }
        }

        /// <summary>
        ///     Get the formatting rectangle of the text. See EM_GETRECT for the definition
        ///     of a formatting rectangle;
        /// </summary>
        protected Rectangle FormattingRectangle
        {
            get
            {
                // EM_GETRECT already takes EM_GETMARGINS into account, so don't use both.
                NativeMethods.RECT rcFormat;
                var hwndEdit = Handle;
                NativeMethods.SendMessage(hwndEdit, NativeMethods.EM_GETRECT, 0, out rcFormat);
                return new Rectangle(rcFormat.left, rcFormat.top, rcFormat.width, rcFormat.height);
            }
        }

        int IVirtualTreeInPlaceControl.ExtraEditWidth
        {
            get { return ExtraEditWidth; }
        }

        /// <summary>
        ///     Implementation of IVirtualTreeInPlaceControl.ExtraEditWidth
        /// </summary>
        protected static int ExtraEditWidth
        {
            get { return VirtualTreeInPlaceControlHelper.DefaultExtraEditWidth; }
        }

        #endregion // Custom InPlaceControl callbacks

        #region Required WndProc override

        /// <summary>
        ///     WndProc override
        /// </summary>
        /// <param name="m"></param>
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            try
            {
                switch (m.Msg)
                {
                        // The tree grid will dispose this control on a MouseWheel
                        // event, but the base control class does not handle the case
                        // of the disappearing control correctly. The DefWndProc call
                        // here will forward the call back up the parent chain. Uncomment
                        // the following code to get to the OnMouseWheel override and/or
                        // fire the event, or handle MOUSEWHEEL code in line at this point.
                    case NativeMethods.WM_MOUSEWHEEL:
                        DefWndProc(ref m);
                        //if (!IsDisposed)
                        //{
                        //	Point p = new Point ((int)(short)m.LParam, (int)m.LParam >> 16);
                        //	p = PointToClient(p);
                        //	OnMouseWheel(new MouseEventArgs(MouseButtons.None, 0, p.X, p.Y, (int)m.WParam >> 16));
                        //}
                        break;
                    default:
                        base.WndProc(ref m);
                        break;
                }
            }
            catch (Exception e)
            {
                if (CriticalException.IsCriticalException(e))
                {
                    throw;
                }

                myInPlaceHelper.DisplayException(e);
            }
        }

        #endregion // Required WndProc override

        /// <summary>
        ///     Create a new inplace edit control
        /// </summary>
        public VirtualTreeInPlaceEditControl()
        {
            myInPlaceHelper = VirtualTreeControl.CreateInPlaceControlHelper(this);
            BorderStyle = BorderStyle.FixedSingle;
            TextAlign = HorizontalAlignment.Left;
        }

        /// <summary>
        ///     Changes default CreateParams. Turns off ES_AUTOVSCROLL and turns on ES_AUTOHSCROLL.
        /// </summary>
        protected override CreateParams CreateParams
        {
            [SecuritySafeCritical]
            [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            [SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                var cp = base.CreateParams;
                cp.Style |= NativeMethods.ES_AUTOHSCROLL;
                cp.Style &= ~NativeMethods.ES_AUTOVSCROLL;
                return cp;
            }
        }
    }

    #region VirtualTreeInPlaceControlHelper class

    /// <summary>
    ///     A helper class used by external (and internal) inplace controls.
    ///     Implements the IVirtualTreeInPlaceControlDefer interface, allowing
    ///     custom inplace control implementations an implementation to defer to
    ///     for most of the IVirtualTreeInPlaceControl methods.
    /// </summary>
    internal abstract class VirtualTreeInPlaceControlHelper : IVirtualTreeInPlaceControlDefer
    {
        private VirtualTreeControl myParent;
        private readonly Control myInPlaceControl;
        private VirtualTreeInPlaceControls myFlags;

        /// <summary>
        ///     The default value for the extra width placed in an inplace edit
        ///     box to allow extra space for typing.
        /// </summary>
        public static readonly int DefaultExtraEditWidth = SystemInformation.Border3DSize.Width * 5;

        private VirtualTreeInPlaceControlHelper()
        {
        }

        /// <summary>
        ///     A constructor for derived classes.
        /// </summary>
        /// <param name="inPlaceControl">The control to inplace activate</param>
        protected VirtualTreeInPlaceControlHelper(Control inPlaceControl)
        {
            myInPlaceControl = inPlaceControl;

            // default flags
            myFlags = VirtualTreeInPlaceControls.SizeToText | VirtualTreeInPlaceControls.DisposeControl;
        }

        /// <summary>
        ///     The tree control we're current in-place active in.
        /// </summary>
        public VirtualTreeControl Parent
        {
            get { return myParent; }
            set
            {
                myParent = value;
                myInPlaceControl.Parent = value;
            }
        }

        /// <summary>
        ///     The inplace control associated with this helper object
        /// </summary>
        public Control InPlaceControl
        {
            get { return myInPlaceControl; }
        }

        /// <summary>
        ///     Settings indicating how the inplace control interacts with the tree control. Defaults
        ///     to SizeToText | DisposeControl.
        /// </summary>
        public VirtualTreeInPlaceControls Flags
        {
            get { return myFlags; }
            set
            {
                var oldValue = myFlags;
                if (oldValue != value)
                {
                    myFlags = value;
                    OnFlagsChanged(oldValue);
                }
            }
        }

        int IVirtualTreeInPlaceControlDefer.LaunchedByMessage
        {
            get { return LaunchedByMessage; }
            set { LaunchedByMessage = value; }
        }

        /// <summary>
        ///     Indicates the windows message used to create the control. Facilitates correct
        ///     mouse handling for ImmediateMouseLabelEdits where the control is transparent.
        /// </summary>
        public abstract int LaunchedByMessage { get; set; }

        /// <summary>
        ///     Value indicates whether the in-place edit control is currently dirty.
        ///     At commit time, only a dirty in-place edit control generates calls
        ///     to IBranch.CommitLabelEdit or a custom commit delegate.
        /// </summary>
        public abstract bool Dirty { get; set; }

        /// <summary>
        ///     Call this method from the OnKeyDown override in
        ///     the in place control.
        /// </summary>
        /// <param name="e">The KeyEventArgs</param>
        /// <returns>Returns true if the keystroke was handled, indicating that further processing is not needed.</returns>
        public abstract bool OnKeyDown(KeyEventArgs e);

        /// <summary>
        ///     Call this method from the OnKeyPress override in
        ///     the in place control.
        /// </summary>
        /// <param name="e">The KeyPressEventArgs</param>
        /// <returns>Returns true if the keystroke was handled, indicating that further processing is not needed.</returns>
        public abstract bool OnKeyPress(KeyPressEventArgs e);

        /// <summary>
        ///     Called at the beginning of the Control.OnTextChanged override.
        /// </summary>
        public abstract void OnTextChanged();

        /// <summary>
        ///     Called at the beginning of the Control.OnLostFocus override.
        /// </summary>
        public abstract void OnLostFocus();

        /// <summary>
        ///     Called when the Flags value is changed.
        /// </summary>
        /// <param name="oldFlags">The old value of the Flags property</param>
        protected abstract void OnFlagsChanged(VirtualTreeInPlaceControls oldFlags);

        /// <summary>
        ///     Called when a mouse message is received by the inplace control. This callback
        ///     should only be used by controls that need to support transparent edit regions.
        /// </summary>
        /// <param name="message">Message structure passed to WndProc</param>
        /// <returns>True to indicate that the message has been handled and should not be forwarded to base.WndProc</returns>
        public abstract bool OnMouseMessage(ref Message message);

        /// <summary>
        ///     Allows in-place controls to call back on the tree to display an error.
        /// </summary>
        /// <param name="exception">Exception containing information to be displayed.</param>
        /// <returns>
        ///     True if the exception is displayed to the user, false otherwise.  Callers should generally rethrow
        ///     the exception if the return value is false.
        /// </returns>
        public abstract bool DisplayException(Exception exception);
    }

    #endregion
}
