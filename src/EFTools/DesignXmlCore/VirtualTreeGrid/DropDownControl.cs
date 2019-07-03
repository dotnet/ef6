// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Common;
    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.Win32;

    /// <summary>
    ///     Values passed to TypeEditorHost.Create to indicate the style of the edit
    ///     box to show in the text area.
    /// </summary>
    internal enum TypeEditorHostEditControlStyle
    {
        /// <summary>
        ///     Displays a live edit box for the given value.
        /// </summary>
        Editable = 0,

        /// <summary>
        ///     Leaves the region where a live edit box would normally be transparent
        ///     to let the backend control draw the text region.
        /// </summary>
        TransparentEditRegion = 1,

        /// <summary>
        ///     Places an instruction label with GrayText in the area the text region would
        ///     normally be.
        /// </summary>
        InstructionLabel = 2,

        /// <summary>
        ///     Displays a read-only edit box for the given value.
        /// </summary>
        ReadOnlyEdit = 3,
    }

    /// <summary>
    ///     Class used to drop down an arbitrary Control, or display a modal dialog editor
    ///     akin to what's being done by the property browser
    /// </summary>
    [SuppressMessage("Whitehorse.CustomRules", "WH03:WinFormControlCatchUnhandledExceptions",
        Justification = "All but critical exceptions are caught.")]
    internal class TypeEditorHost : Control, IWindowsFormsEditorService, ITypeDescriptorContext, IVirtualTreeInPlaceControl
    {
        private TypeEditorHostTextBox _edit;
        private Control _instructionLabel;
        private Control _dropDown;
        private DropDownButton _button;
        private DropDownHolder _dropDownHolder;
        private object _instance; // instance object currently being edited
        private PropertyDescriptor _propertyDescriptor; // property descriptor describing instance object property being edited
        private readonly UITypeEditor _uiTypeEditor; // cached uiTypeEditor
        private bool _inShowDialog; // flag set to true if we are showing a dialog.

        private DialogResult _dialogResult;
                             // cached DialogResult from ShowDialog call.  Used in OpenDropDown to determine whether to dismiss the label edit control if commitOnClose is set.

        // cache these because we won't use them until handle creation.
        private TypeEditorHostEditControlStyle _editControlStyle;
        private readonly UITypeEditorEditStyle _editStyle;

        private const int DROP_DOWN_DEFAULT_HEIGHT = 100;

        /// <summary>
        ///     Fired after the dropdown closes
        /// </summary>
        public event EventHandler DropDownClosed;

        /// <summary>
        ///     Creates a new TypeEditorHost to display the given UITypeEditor
        /// </summary>
        /// <param name="editor">The UITypeEditor instance to host</param>
        /// <param name="propertyDescriptor">Property descriptor used to get/set values in the drop-down.</param>
        /// <param name="instance">Instance object used to get/set values in the drop-down.</param>
        protected TypeEditorHost(UITypeEditor editor, PropertyDescriptor propertyDescriptor, object instance)
            :
                this(UITypeEditorEditStyle.DropDown, propertyDescriptor, instance, TypeEditorHostEditControlStyle.Editable)
        {
            _uiTypeEditor = editor;
            if (editor != null)
            {
                _editStyle = editor.GetEditStyle(this);
            }
        }

        /// <summary>
        ///     Creates a new TypeEditorHost to display the given UITypeEditor
        /// </summary>
        /// <param name="editor">The UITypeEditor instance to host</param>
        /// <param name="editControlStyle">The type of control to show in the edit area.</param>
        /// <param name="propertyDescriptor">Property descriptor used to get/set values in the drop-down.</param>
        /// <param name="instance">Instance object used to get/set values in the drop-down.</param>
        protected TypeEditorHost(
            UITypeEditor editor, PropertyDescriptor propertyDescriptor, object instance, TypeEditorHostEditControlStyle editControlStyle)
            :
                this(UITypeEditorEditStyle.DropDown, propertyDescriptor, instance, editControlStyle)
        {
            _uiTypeEditor = editor;
            if (editor != null)
            {
                _editStyle = editor.GetEditStyle(this);
            }
        }

        /// <summary>
        ///     Creates a new TypeEditorHost with the given editStyle.
        /// </summary>
        /// <param name="editStyle">Style of editor to create.</param>
        /// ///
        /// <param name="propertyDescriptor">Property descriptor used to get/set values in the drop-down.</param>
        /// <param name="instance">Instance object used to get/set values in the drop-down.</param>
        /// <param name="editControlStyle">Style of text box to create.</param>
        protected TypeEditorHost(
            UITypeEditorEditStyle editStyle, PropertyDescriptor propertyDescriptor, object instance,
            TypeEditorHostEditControlStyle editControlStyle)
        {
            if (m_inPlaceHelper != null)
            {
                return; // only allow initialization once
            }

            _editStyle = editStyle;
            _editControlStyle = editControlStyle;
            CurrentPropertyDescriptor = propertyDescriptor;
            CurrentInstance = instance;

            // initialize VirtualTree in-place edit stuff
            m_inPlaceHelper = VirtualTreeControl.CreateInPlaceControlHelper(this);
            m_inPlaceHelper.Flags &= ~VirtualTreeInPlaceControls.SizeToText; // size to full item width by default

            // set accessible role of the parent control of the text box/button to combo box, 
            // so accessibility clients have a clue they're dealing with a combo box-like control.
            AccessibleRole = AccessibleRole.ComboBox;
            TabStop = false;
        }

        /// <summary>
        ///     Factory method for creating the appropriate drop-down control based on the given property descriptor
        /// </summary>
        /// <param name="propertyDescriptor">A property descriptor describing the property being set</param>
        /// <param name="instance">The object instance being edited</param>
        /// <returns>A TypeEditorHost instance if the given property descriptor supports it, null otherwise.</returns>
        public static TypeEditorHost Create(PropertyDescriptor propertyDescriptor, object instance)
        {
            TypeEditorHost dropDown = null;

            if (propertyDescriptor != null)
            {
                var uiTypeEditor = propertyDescriptor.GetEditor(typeof(UITypeEditor)) as UITypeEditor;
                if (uiTypeEditor != null) // UITypeEditor case
                {
                    dropDown = new TypeEditorHost(uiTypeEditor, propertyDescriptor, instance);
                }
                else
                {
                    var converter = propertyDescriptor.Converter;
                    if (converter != null
                        && converter.GetStandardValuesSupported(null)) // converter case
                    {
                        dropDown = new TypeEditorHostListBox(converter, propertyDescriptor, instance);
                    }
                }
            }

            return dropDown;
        }

        /// <summary>
        ///     Factory method for creating the appropriate drop-down control based on the given property descriptor.
        ///     If the property descriptor supports a UITypeEditor, a TypeEditorHost will be created with that editor.
        ///     If not, and the TypeConverter attached to the PropertyDescriptor supports standard values, a
        ///     TypeEditorHostListBox will be created with this TypeConverter.
        /// </summary>
        /// <param name="propertyDescriptor">A property descriptor describing the property being set</param>
        /// <param name="instance">The object instance being edited</param>
        /// <param name="editControlStyle">The type of control to show in the edit area.</param>
        /// <returns>A TypeEditorHost instance if the given property descriptor supports it, null otherwise.</returns>
        public static TypeEditorHost Create(
            PropertyDescriptor propertyDescriptor, object instance, TypeEditorHostEditControlStyle editControlStyle)
        {
            TypeEditorHost dropDown = null;

            if (propertyDescriptor != null)
            {
                var uiTypeEditor = propertyDescriptor.GetEditor(typeof(UITypeEditor)) as UITypeEditor;
                if (uiTypeEditor != null) // UITypeEditor case
                {
                    dropDown = new TypeEditorHost(uiTypeEditor, propertyDescriptor, instance, editControlStyle);
                }
                else
                {
                    var converter = propertyDescriptor.Converter;
                    if (converter != null
                        && converter.GetStandardValuesSupported(null)) // converter case
                    {
                        dropDown = new TypeEditorHostListBox(converter, propertyDescriptor, instance, editControlStyle);
                    }
                }
            }

            return dropDown;
        }

        /// <summary>
        ///     Create the edit/drop-down controls when our handle is created.
        /// </summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // create the edit box.  done before creating the button because
            // DockStyle.Right controls should be added first.
            switch (_editControlStyle)
            {
                case TypeEditorHostEditControlStyle.Editable:
                    InitializeEdit();
                    break;

                case TypeEditorHostEditControlStyle.ReadOnlyEdit:
                    InitializeEdit();
                    _edit.ReadOnly = true;
                    break;

                case TypeEditorHostEditControlStyle.InstructionLabel:
                    InitializeInstructionLabel();
                    UpdateInstructionLabelText();
                    break;
            }

            // create the drop-down button
            if (EditStyle != UITypeEditorEditStyle.None)
            {
                _button = new DropDownButton();
                _button.Dock = DockStyle.Right;
                _button.FlatStyle = FlatStyle.Flat;
                _button.FlatAppearance.BorderSize = 0;
                _button.FlatAppearance.MouseDownBackColor
                    = VSColorTheme.GetThemedColor(EnvironmentColors.ComboBoxButtonMouseDownBackgroundColorKey);
                _button.FlatAppearance.MouseOverBackColor
                    = VSColorTheme.GetThemedColor(EnvironmentColors.ComboBoxButtonMouseOverBackgroundColorKey);

                // only allow focus to go to the drop-down button if we don't have an edit control.
                // if we have an edit control, we want this to act like a combo box, where only the edit control
                // is focusable.  If there's no edit control, though, we need to make sure the button can be 
                // focused, to prevent WinForms from forwarding focus somewhere else.
                _button.TabStop = _edit == null;

                if (_editStyle == UITypeEditorEditStyle.DropDown)
                {
                    _button.Image = CreateArrowBitmap();
                    // set button name for accessibility purposes.
                    _button.AccessibleName = VirtualTreeStrings.GetString(VirtualTreeStrings.DropDownButtonAccessibleName);
                }
                else if (_editStyle == UITypeEditorEditStyle.Modal)
                {
                    _button.Image = CreateDotDotDotBitmap();

                    // set button name for accessibility purposes.
                    _button.AccessibleName = VirtualTreeStrings.GetString(VirtualTreeStrings.BrowseButtonAccessibleName);
                }
                // Bug 17449 (Currituck). Use the system prescribed one, property grid uses this approach in 
                // ndp\fx\src\winforms\managed\system\winforms\propertygridinternal\propertygridview.cs 
                _button.Size = new Size(SystemInformation.VerticalScrollBarArrowHeight, Font.Height);

                _button.BackColor = VSColorTheme.GetThemedColor(EnvironmentColors.ComboBoxBackgroundColorKey);

                Controls.Add(_button);
                _button.Click += OnDropDownButtonClick;
                _button.LostFocus += OnContainedControlLostFocus;
            }

            // Create the drop down control container.  Check for null here because this
            // may have already been created via a call to SetComponent.
            if (_dropDownHolder == null
                && EditStyle == UITypeEditorEditStyle.DropDown)
            {
                _dropDownHolder = new DropDownHolder(this);
                _dropDownHolder.Font = Font;
            }
        }

        private void InitializeEdit()
        {
            _edit = CreateTextBox();
            _edit.BorderStyle = BorderStyle.None;
            _edit.AutoSize = false; // with no border, AutoSize causes some overlap with the grid lines on the tree control, so we remove it.
            _edit.Dock = DockStyle.Fill;
            _edit.Text = base.Text; // we store text locally prior to handle creation, so we set it here.
            _edit.KeyDown += OnEditKeyDown;
            _edit.KeyPress += OnEditKeyPress;
            _edit.LostFocus += OnContainedControlLostFocus;
            _edit.TextChanged += OnEditTextChanged;
            Controls.Add(_edit);
        }

        /// <summary>
        ///     Create the edit control.  Derived classes may override to customize the edit control.
        /// </summary>
        protected virtual TypeEditorHostTextBox CreateTextBox()
        {
            return new TypeEditorHostTextBox(this);
        }

        private void InitializeInstructionLabel()
        {
            _instructionLabel = CreateInstructionLabel();
            _instructionLabel.Dock = DockStyle.Fill;
            Controls.Add(_instructionLabel);
        }

        /// <summary>
        ///     Create the instruction label control.  Derived classes may override this to customize the instruction label.
        /// </summary>
        /// <returns>A new instruction label</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected virtual Label CreateInstructionLabel()
        {
            var label = new Label();
            label.UseMnemonic = false;
            return label;
        }

        /// <summary>
        ///     Determine the TypeEditorHostEditControlStyle settings for this control
        /// </summary>
        public TypeEditorHostEditControlStyle EditControlStyle
        {
            get { return _editControlStyle; }
            set
            {
                if (value != _editControlStyle)
                {
                    _editControlStyle = value;
                    // Whatever control is currently visible has to go.
                    if (_edit != null)
                    {
                        _edit.Dispose();
                        _edit = null;
                    }
                    if (_instructionLabel != null)
                    {
                        _instructionLabel.Dispose();
                        _instructionLabel = null;
                    }
                    switch (value)
                    {
                        case TypeEditorHostEditControlStyle.Editable:
                            InitializeEdit();
                            break;
                        case TypeEditorHostEditControlStyle.ReadOnlyEdit:
                            InitializeEdit();
                            _edit.ReadOnly = true;
                            break;
                        case TypeEditorHostEditControlStyle.InstructionLabel:
                            InitializeInstructionLabel();
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     UITypeEditorEditStyle used by this TypeEditorHost.
        /// </summary>
        public UITypeEditorEditStyle EditStyle
        {
            get { return _editStyle; }
        }

        /// <summary>
        ///     Get/set the current property descriptor
        /// </summary>
        public PropertyDescriptor CurrentPropertyDescriptor
        {
            get { return _propertyDescriptor; }
            set
            {
                _propertyDescriptor = value;
                UpdateInstructionLabelText();
            }
        }

        /// <summary>
        ///     Get/set the current instance object
        /// </summary>
        public object CurrentInstance
        {
            get { return _instance; }
            set
            {
                _instance = value;
                UpdateInstructionLabelText();
            }
        }

        /// <summary>
        ///     Specifies whether the label edit control should remain active when the drop-down closes.
        ///     Note that this only applies to cases where the value changes, if the value does not change
        ///     or the user cancels the edit, the edit control will remain active regardless of the value of
        ///     this property.
        ///     The default value is false, which means that the label edit control remains active.
        /// </summary>
        /// <value></value>
        public bool DismissLabelEditOnDropDownClose { get; set; }

        /// <summary>
        ///     The instruction label text is based on the value returned from the current
        ///     property descriptor. This function should be called when the CurrentPropertyDescriptor
        ///     and Instance properties change to update the text.
        /// </summary>
        protected void UpdateInstructionLabelText()
        {
            if (_instructionLabel != null
                && _propertyDescriptor != null
                && _instance != null)
            {
                _instructionLabel.Text = Text;
            }
        }

        /// <summary>
        ///     Specifies window creation flags.
        /// </summary>
        protected override CreateParams CreateParams
        {
            [SecuritySafeCritical]
            [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            [SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                var cp = base.CreateParams;

                // remove WS_CLIPCHILDREN.  Presence of this style causes update regions of our child controls
                // not to be invalidated when the parent control is resized, which causes painting issues.
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
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.ValidateRect(System.IntPtr,Microsoft.Data.Entity.Design.VisualStudio.NativeMethods+Rectangle@)")]
        protected override void WndProc(ref Message m)
        {
            try
            {
                // Do special processing if the control is simply in pass through mode
                if (_edit == null)
                {
                    switch (m.Msg)
                    {
                        case NativeMethods.WM_PAINT:
                            if (_instructionLabel == null
                                && m.WParam == IntPtr.Zero)
                            {
                                var rect = new NativeMethods.Rectangle(ClientRectangle);

                                if (_button != null)
                                {
                                    rect.Right = _button.Left;
                                }

                                NativeMethods.ValidateRect(m.HWnd, ref rect);
                                if (!NativeMethods.GetUpdateRect(m.HWnd, IntPtr.Zero, false))
                                {
                                    return;
                                }
                            }

                            break;
                        default:
                            if (m.Msg >= NativeMethods.WM_MOUSEFIRST
                                && m.Msg <= NativeMethods.WM_MOUSELAST)
                            {
                                if (m_inPlaceHelper.OnMouseMessage(ref m) || IsDisposed)
                                {
                                    return;
                                }
                            }
                            break;
                    }
                }
                else
                {
                    // if we have an edit control, forward clipboard commands to it.
                    if (m.Msg == NativeMethods.WM_CUT
                        || m.Msg == NativeMethods.WM_COPY
                        || m.Msg == NativeMethods.WM_PASTE)
                    {
                        // forward these to the underlying edit control
                        m.Result = NativeMethods.SendMessage(_edit.Handle, m.Msg, (int)m.WParam, (int)m.LParam);
                        return;
                    }
                }

                base.WndProc(ref m);
            }
            catch (Exception e)
            {
                if (CriticalException.IsCriticalException(e))
                {
                    throw;
                }

                m_inPlaceHelper.DisplayException(e);
            }
        }

        /// <summary>
        ///     Control.IsInputKey override
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns>true if the parent control will need the keys for navigation</returns>
        protected override bool IsInputKey(Keys keyData)
        {
            if (_edit == null)
            {
                switch (keyData & Keys.KeyCode)
                {
                    case Keys.Left:
                    case Keys.Right:
                    case Keys.Up:
                    case Keys.Down:
                        // Key these to the message loop, where we can forward them to the parent control
                        return true;
                }
            }
            return base.IsInputKey(keyData);
        }

        /// <summary>
        ///     Control.OnKeyDown override. First tries to open dropdown, then defers
        ///     to the helper, and finally the control
        /// </summary>
        /// <param name="e">KeyEventArgs</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!e.Handled)
            {
                switch (e.KeyCode)
                {
                    case Keys.Down:
                        if (e.Alt
                            && !e.Control)
                        {
                            OpenDropDown();
                            e.Handled = true;
                            return;
                        }
                        break;
                }
            }
            // pass on to the outer control, for further handling
            if (!m_inPlaceHelper.OnKeyDown(e))
            {
                base.OnKeyDown(e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_edit != null)
                {
                    _edit.Dispose();
                }

                if (_instructionLabel != null)
                {
                    _instructionLabel.Dispose();
                }

                if (_button != null)
                {
                    _button.Dispose();
                }

                if (_dropDown != null)
                {
                    _dropDown.Dispose();
                }

                if (_dropDownHolder != null)
                {
                    _dropDownHolder.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        ///     Specify the control to display in the drop-down
        /// </summary>
        /// <param name="control">control to display</param>
        public void SetComponent(Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }

            // create the drop-down holder, if necessary
            if (_dropDownHolder == null)
            {
                _dropDownHolder = new DropDownHolder(this);
                _dropDownHolder.Font = Font;
            }

            if (_dropDown != null)
            {
                _dropDown.KeyDown -= OnDropDownKeyDown;
                _dropDown.Dispose();
            }

            _dropDown = control;

            if (_dropDown != null)
            {
                // site the control, to allow it access to services from our container
                control.Site = Site;
                control.KeyDown += OnDropDownKeyDown;
                _dropDownHolder.SetComponent(_dropDown, (_uiTypeEditor == null) ? Resizable : _uiTypeEditor.IsDropDownResizable);
            }
        }

        /// <summary>
        ///     Override to make the dropdown resizable. This property is ignored if a UITypeEditor is
        ///     provided.
        /// </summary>
        protected virtual bool Resizable
        {
            get { return false; }
        }

        protected TextBox Edit
        {
            get { return _edit; }
        }

        /// <summary>
        ///     Allow clients to customize the edit control accessible name.
        /// </summary>
        public string EditAccessibleName
        {
            get { return _edit.AccessibleName; }
            set { _edit.AccessibleName = value; }
        }

        /// <summary>
        ///     Allow clients to customize the edit control accessible description.
        /// </summary>
        public string EditAccessibleDescription
        {
            get { return _edit.AccessibleDescription; }
            set { _edit.AccessibleDescription = value; }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            // size button
            if (_button != null)
            {
                _button.Height = Height;
                // Part of Bug 17449 (Currituck). Button width setting removed here to match VS guidelines, use SystemInformation.VerticalScrollBarArrowHeight instead.
            }

            base.OnLayout(e);

            /*if(edit != null)
            {
                // center text box vertically
                edit.Top += ((Height - edit.Height) / 2);
            }*/
        }

        /// <summary>
        ///     Update the drop-down container font when our font changes.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFontChanged(EventArgs e)
        {
            // update the font of the drop down container
            if (_dropDownHolder != null)
            {
                _dropDownHolder.Font = Font;
            }

            base.OnFontChanged(e);
        }

        /// <summary>
        ///     Returns true iff the drop down currently contains the focus
        /// </summary>
        public bool DropDownContainsFocus
        {
            get { return _dropDownHolder != null && _dropDownHolder.ContainsFocus; }
        }

        protected Control DropDown
        {
            get { return _dropDown; }
        }

        public override string Text
        {
            get
            {
                if (_edit != null)
                {
                    return _edit.Text;
                }
                else if (!IsHandleCreated
                         && (EditControlStyle == TypeEditorHostEditControlStyle.Editable
                             || EditControlStyle == TypeEditorHostEditControlStyle.ReadOnlyEdit))
                {
                    // prior to handle creation, the edit control will not be created, so we store text
                    // locally in this control.
                    return base.Text;
                }
                else if (_instance != null
                         && _propertyDescriptor != null)
                {
                    var converter = _propertyDescriptor.Converter;
                    var value = _propertyDescriptor.GetValue(_instance);
                    if (converter != null
                        && converter.CanConvertTo(this, typeof(string)))
                    {
                        return converter.ConvertToString(this, CultureInfo.CurrentUICulture, value);
                    }
                    return value.ToString();
                }
                return String.Empty;
            }
            set
            {
                if (_edit != null)
                {
                    _edit.Text = value;
                    OnTextChanged(EventArgs.Empty);
                }
                else if (!IsHandleCreated
                         && (EditControlStyle == TypeEditorHostEditControlStyle.Editable
                             || EditControlStyle == TypeEditorHostEditControlStyle.ReadOnlyEdit))
                {
                    base.Text = value;
                }
            }
        }

        /// <summary>
        ///     Return the height of the control contained in the drop down control.
        ///     The default implementation returns the width of the contained drop down control.
        /// </summary>
        protected virtual int DropDownHeight
        {
            get { return (_dropDownHolder != null) ? _dropDownHolder.Component.Height : DROP_DOWN_DEFAULT_HEIGHT; }
        }

        /// <summary>
        ///     Returns the width of the control contained in the dropdown. The default implementation
        ///     returns the width of the drop down control, if one is provided.
        ///     This override is not used if IgnoreDropDownWidth returns true.
        /// </summary>
        protected virtual int DropDownWidth
        {
            get { return (_dropDownHolder != null) ? _dropDownHolder.Component.Width : Width; }
        }

        /// <summary>
        ///     Override this property and return true to always give the dropdown
        ///     the same initial size as the dropdown host control. If this property
        ///     is set, the DropDownWidth property will not be called.
        /// </summary>
        protected virtual bool IgnoreDropDownWidth
        {
            get { return false; }
        }

        /// <summary>
        ///     Fired before the drop down is opened.  Clients may sink this event if they need to perform
        ///     some initialization before the drop-down control is shown.
        /// </summary>
        public event EventHandler OpeningDropDown;

        /// <summary>
        ///     Fired after the property descriptor's SetValue method is called and before
        ///     the OnTextChanged method is called to (potentially) resize the control. Responding to
        ///     this event allows the use the change the EditControlStyle property for different values
        /// </summary>
        public event EventHandler PropertyDescriptorValueChanged;

        /// <summary>
        ///     Notify derived classes that the drop-down is opening
        /// </summary>
        protected virtual void OnOpeningDropDown(EventArgs e)
        {
            // inform listeners that the drop-down is about to open
            if (OpeningDropDown != null)
            {
                OpeningDropDown(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///     Notify derived classes that CurrentPropertyDescriptor.SetValue has been called.
        /// </summary>
        protected virtual void OnPropertyDescriptorValueChanged(EventArgs e)
        {
            // inform listeners that the value is changing
            if (PropertyDescriptorValueChanged != null)
            {
                PropertyDescriptorValueChanged(this, EventArgs.Empty);
            }
        }

        private void DisplayDropDown()
        {
            Debug.Assert(_dropDownHolder != null, "OpenDropDown called with no control to drop down");
            OnOpeningDropDown(EventArgs.Empty);

            // Get drop down height
            var dropHeight = DropDownHeight + 2 * DropDownHolder.DropDownHolderBorder;
            var dropWidth = IgnoreDropDownWidth ? Width : DropDownWidth + 2 * DropDownHolder.DropDownHolderBorder;

            // position drop-down under the arrow
            var location = new Point(Width - dropWidth, Height);
            location = PointToScreen(location);

            var bounds = new Rectangle(location, new Size(dropWidth, dropHeight));
            var currentScreen = Screen.FromControl(this);
            if (currentScreen != null
                && bounds.Bottom > currentScreen.WorkingArea.Bottom)
            {
                // open the drop-down upwards if it will go off the bottom of the screen
                bounds.Y -= (dropHeight + Height);
                _dropDownHolder.ResizeUp = true;
            }
            else
            {
                _dropDownHolder.ResizeUp = false;
            }

            _dropDownHolder.Bounds = bounds;
            // display drop-down
            _dropDownHolder.Visible = true;

            _dropDownHolder.Focus();

            _dropDownHolder.HookMouseDown = true;
            _dropDownHolder.DoModalLoop();
            _dropDownHolder.HookMouseDown = false;
        }

        internal void CloseDropDown(bool accept)
        {
            Debug.Assert(_dropDownHolder != null, "CloseDropDown called with no drop-down");
            _dropDownHolder.Visible = false;
            _dropDownHolder.HookMouseDown = false;

            if (accept && (0 != String.Compare(Text, _dropDown.Text, false, CultureInfo.CurrentCulture)))
            {
                Text = _dropDown.Text;
            }

            if (_edit != null)
            {
                _edit.Focus();
            }
            else
            {
                Focus();
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static Bitmap CreateArrowBitmap()
        {
            Bitmap bitmap = null;
            Icon icon = null;
            try
            {
                icon = new Icon(typeof(TypeEditorHost), "arrow.ico");
                using (var original = icon.ToBitmap())
                {
                    bitmap = BitmapFromImageReplaceColor(original);
                }
            }
            catch
            {
                bitmap = new Bitmap(16, 16);
                throw;
            }
            finally
            {
                if (icon != null)
                {
                    icon.Dispose();
                }
            }
            return bitmap;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static Bitmap CreateDotDotDotBitmap()
        {
            Bitmap bitmap = null;
            Icon icon = null;
            try
            {
                icon = new Icon(typeof(TypeEditorHost), "dotdotdot.ico");
                using (var original = icon.ToBitmap())
                {
                    bitmap = BitmapFromImageReplaceColor(original);
                }
            }
            catch
            {
                bitmap = new Bitmap(16, 16);
                throw;
            }
            finally
            {
                if (icon != null)
                {
                    icon.Dispose();
                }
            }
            return bitmap;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static Bitmap BitmapFromImageReplaceColor(Image original)
        {
            var newBitmap = new Bitmap(original.Width, original.Height, original.PixelFormat);

            using (var g = Graphics.FromImage(newBitmap))
            {
                using (var attrs = new ImageAttributes())
                {
                    var cm = new ColorMap();

                    cm.OldColor = Color.Black;
                    // Bug 17449 (Currituck). Use the system prescribed one, property grid uses this approach in 
                    // ndp\fx\src\winforms\managed\system\winforms\propertygridinternal\propertygridview.cs 
                    cm.NewColor = VSColorTheme.GetThemedColor(EnvironmentColors.ComboBoxGlyphColorKey);
                    attrs.SetRemapTable(new[] { cm }, ColorAdjustType.Bitmap);
                    g.DrawImage(
                        original, new Rectangle(0, 0, original.Width, original.Height), 0, 0, original.Width, original.Height,
                        GraphicsUnit.Pixel, attrs, null, IntPtr.Zero);
                }
            }

            return newBitmap;
        }

        private void OnDropDownButtonClick(object sender, EventArgs e)
        {
            OpenDropDown();
        }

        /// <summary>
        ///     Open the dropdown
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void OpenDropDown()
        {
            if (_dropDown == null
                || !_dropDown.Visible)
            {
                try
                {
                    if (m_inPlaceHelper.Dirty)
                    {
                        // Handle currently dirty editor.  Follow the property grid here and commit the dirty value before
                        // displaying the drop-down.  Note that this is one case where we take a label edit and convert it
                        // before calling SetValue.  Don't really have much choice, though, since we can't force a CommitLabelEdit
                        // here.
                        Debug.Assert(Edit != null, "how did we become dirty without an in-place edit control?");
                        m_inPlaceHelper.Dirty = false;
                        _propertyDescriptor.SetValue(_instance, ConvertFromString(Text));
                        if (m_inPlaceHelper.Parent == null
                            || m_inPlaceHelper.Parent.LabelEditControl != this)
                        {
                            return; // bail out if we're no longer editing
                        }
                    }

                    // Get the value directly from the property descriptor so that the
                    // EditValue call deals with a raw value, not one that has gone through
                    // type converters, etc.
                    var value = _propertyDescriptor.GetValue(_instance);

                    if (_uiTypeEditor != null)
                    {
                        _dialogResult = DialogResult.None;
                        // we have a UITypeEditor, use it.  UITypeEditor usually calls back on ShowDialog or OpenDropDown
                        // via the IWindowsFormsEditorService, so in most cases this pushes a modal loop.
                        var newValue = _uiTypeEditor.EditValue(this, this, value);
                        if (m_inPlaceHelper.Parent == null
                            || m_inPlaceHelper.Parent.LabelEditControl != this)
                        {
                            // when control is dismissed, one need to call setvalue to prevent loss of user data
                            // See bug VSW 328713
                            // when control is dismissed, one must *not* call setvalue if the new value is the same as the old one
                            // See bug VSW 383162
                            if (value != newValue)
                            {
                                m_inPlaceHelper.Dirty = false;

                                _propertyDescriptor.SetValue(_instance, newValue);
                                OnPropertyDescriptorValueChanged(EventArgs.Empty);
                            }
                        }
                        else
                        {
                            if (value != newValue)
                            {
                                m_inPlaceHelper.OnTextChanged();
                                if (DismissLabelEditOnDropDownClose)
                                {
                                    // dismiss the edit control if DismissLabelEditOnDropDownClose is set.
                                    m_inPlaceHelper.Parent.EndLabelEdit(true /* cancel */);
                                }
                                else
                                {
                                    // user has made an edit, update the text displayed
                                    Text = ConvertToString(newValue);
                                    SelectAllText();
                                    // inform the VirtualTreeControl that the edit should not dirty the in-place edit window.
                                    // All required data store changes should be made as part of the SetValue call, so
                                    // we do not want to generate a CommitLabelEdit call when the in-place edit is committed.
                                    // we make this call prior to the call to SetValue in case that call triggers the commit.
                                    m_inPlaceHelper.Dirty = false;
                                }

                                _propertyDescriptor.SetValue(_instance, newValue);
                                OnPropertyDescriptorValueChanged(EventArgs.Empty);
                            }
                            else
                            {
                                if (DismissLabelEditOnDropDownClose)
                                {
                                    // dismiss the edit control if DismissLabelEditOnDropDownClose is set.
                                    // if a dialog editor was used and OK was pressed, we do this even if 
                                    // value == newValue.  This enables editors that want to internally handle 
                                    // updates in EditValue rather than through a separate SetValue call.
                                    m_inPlaceHelper.Parent.EndLabelEdit(true /* cancel */);
                                }
                                else
                                {
                                    // the property browser refreshes after EditValue returns, no matter what the return value is.
                                    // To mimic this, we'll update the value from the property descriptor if no edit is made.
                                    // This enables editors that want to internally handle updates in EditValue rather than through
                                    // a separate SetValue call.
                                    var refreshValue = _propertyDescriptor.GetValue(_instance);
                                    if (refreshValue != null)
                                    {
                                        Text = ConvertToString(refreshValue);
                                        SelectAllText();
                                        m_inPlaceHelper.Dirty = false;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // non-UITypeEditor case.  May be a TypeEditorHostListBox that uses a TypeConverter,
                        // or a derived class that doesn't use a UITypeEditor.

                        // Open the drop down.  This pushes a modal loop.
                        DisplayDropDown();
                        if (m_inPlaceHelper.Parent == null
                            || m_inPlaceHelper.Parent.LabelEditControl != this
                            || !Dirty)
                        {
                            return; // bail out if we're no longer editing
                        }

                        // use our text to determine the new value.  This will be set 
                        // in CloseDropDown, if the user makes an edit
                        var newValue = (Edit != null) ? ConvertFromString(Text) : null;
                        if ((value != null && !value.Equals(newValue))
                            || (value == null && newValue != null))
                        {
                            m_inPlaceHelper.OnTextChanged();
                            if (DismissLabelEditOnDropDownClose)
                            {
                                // dismiss the edit control if DismissLabelEditOnDropDownClose is set.
                                m_inPlaceHelper.Parent.EndLabelEdit(true /* cancel */);
                            }
                            else
                            {
                                m_inPlaceHelper.Dirty = false; // see comment above
                            }

                            _propertyDescriptor.SetValue(_instance, newValue);
                            OnPropertyDescriptorValueChanged(EventArgs.Empty);
                        }
                    }

                    OnDropDownClosed(EventArgs.Empty);
                }
                catch (Exception e)
                {
                    if (CriticalException.IsCriticalException(e))
                    {
                        throw;
                    }

                    // display exceptions thrown during drop-down open to the user
                    if (!m_inPlaceHelper.DisplayException(e))
                    {
                        throw;
                    }
                }
            }
            else
            {
                CloseDropDown(false);
            }
        }

        private object ConvertFromString(string value)
        {
            if (_propertyDescriptor.PropertyType == typeof(string))
            {
                return value;
            }

            var converter = _propertyDescriptor.Converter;
            if (converter != null
                && converter.CanConvertFrom(this, typeof(string)))
            {
                try
                {
                    return converter.ConvertFromString(this, CultureInfo.CurrentCulture, value);
                }
                catch (Exception e)
                {
                    if (CriticalException.IsCriticalException(e))
                    {
                        // We throw if this is a critical exception.
                        throw;
                    }
                    // If the string cannot be converted by the TypeConverter, we return null.
                    // This change was made for VSW Bug # 448059. When a boolean value had
                    // not been initialized to true or false, and the user hit the drop-down
                    // control and dismissed it without selecting an item, we got an empty
                    // string that we tried to convert to a boolean. Ideally we would be passed
                    // a different type-converter for a boolean that can exist in uninitialized
                    // state, but that would have been a bigger code change towards the end of Beta2.
                    // We put this fix in instead so that if the client passes us a string that
                    // their TypeConverter cannot handle, then we return null. We hope that if
                    // the client passed us a string and a TypeConverter that are incompatible,
                    // then their PropertyDescriptor can handle setting a value to null.
                    return null;
                }
            }

            return null;
        }

        private string ConvertToString(object value)
        {
            var stringValue = value as string;
            if (stringValue != null)
            {
                return stringValue;
            }

            var converter = _propertyDescriptor.Converter;
            if (converter != null
                && converter.CanConvertTo(this, typeof(string)))
            {
                return converter.ConvertToString(this, CultureInfo.CurrentCulture, value);
            }

            return null;
        }

        /// <summary>
        ///     The dropdown has been closed
        /// </summary>
        protected virtual void OnDropDownClosed(EventArgs e)
        {
            if (DropDownClosed != null)
            {
                DropDownClosed(this, e);
            }
        }

        private void OnEditKeyDown(object sender, KeyEventArgs e)
        {
            OnEditKeyDown(e);
        }

        /// <summary>
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnEditKeyDown(KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        private void OnEditKeyPress(object sender, KeyPressEventArgs e)
        {
            OnEditKeyPress(e);
        }

        /// <summary>
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnEditKeyPress(KeyPressEventArgs e)
        {
            // pass on to the outer control, for further handling
            OnKeyPress(e);
        }

        private void OnContainedControlLostFocus(object sender, EventArgs e)
        {
            // pass on to the outer control, for further handling
            OnLostFocus(e);
        }

        private void OnEditTextChanged(object sender, EventArgs e)
        {
            // this causes the Text property of this control to 
            // change as well, so treat it as such
            OnTextChanged(e);
        }

        /// <summary>
        ///     Update drop-down button image when the system colors change.
        ///     Required for high-contrast mode support.
        /// </summary>
        protected override void OnSystemColorsChanged(EventArgs e)
        {
            // colors actually haven't been updated at this point.
            // wait for the System event to let us know they have
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Color)
            {
                SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;

                // recreate button image, required to support high contrast
                var editStyle = UITypeEditorEditStyle.DropDown;

                if (_uiTypeEditor != null)
                {
                    editStyle = _uiTypeEditor.GetEditStyle();
                }

                var oldImage = _button.Image;

                if (oldImage != null)
                {
                    try
                    {
                        if (editStyle == UITypeEditorEditStyle.DropDown)
                        {
                            _button.Image = CreateArrowBitmap();
                        }
                        else if (editStyle == UITypeEditorEditStyle.Modal)
                        {
                            _button.Image = CreateDotDotDotBitmap();
                        }
                    }
                    finally
                    {
                        oldImage.Dispose();
                    }
                }
            }
        }

        private void OnDropDownKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    CloseDropDown(false);
                    e.Handled = true;
                    break;
                case Keys.Enter:
                    CloseDropDown(true);
                    e.Handled = true;
                    break;
            }

            if (!e.Handled)
            {
                // pass this on to the outer control, for further handling
                OnKeyDown(e);
            }
        }

        internal static bool HandleContains(IntPtr parentHandle, IntPtr childHandle)
        {
            if (parentHandle == IntPtr.Zero)
            {
                return false;
            }
            while (childHandle != IntPtr.Zero)
            {
                if (childHandle == parentHandle)
                {
                    return true;
                }
                childHandle = NativeMethods.GetParent(childHandle);
            }
            return false;
        }

        #region ITypeDescriptorContext Members

        /// <summary>
        ///     ITypeDescriptorContext.OnComponentChanged
        /// </summary>
        public void /*ITypeDescriptorContext*/ OnComponentChanged()
        {
            // don't support IComponentChangeService
        }

        IContainer ITypeDescriptorContext.Container
        {
            get { return TypeDescriptorContextContainer; }
        }

        /// <summary>
        ///     Gets the IContainer that contains the Component.
        /// </summary>
        /// <value></value>
        protected static IContainer TypeDescriptorContextContainer
        {
            get
            {
                // we don't support containers
                return null;
            }
        }

        /// <summary>
        ///     ITypeDescriptorContext.OnComponentChanging implementation.
        /// </summary>
        /// <returns>true</returns>
        public bool /*ITypeDescriptorContext*/ OnComponentChanging()
        {
            // Don't really support IComponentChangeService
            // but must return true to allow component update.
            // However, framework will not fire ComponentChanged event.
            return true;
        }

        /// <summary>
        ///     ITypeDescriptorContext.Instance implementation
        /// </summary>
        public object /*ITypeDescriptorContext*/ Instance
        {
            get { return _instance; }
        }

        /// <summary>
        ///     ITypeDescriptorContext.PropertyDescriptor implementation
        /// </summary>
        public PropertyDescriptor /*ITypeDescriptorContext*/ PropertyDescriptor
        {
            get { return _propertyDescriptor; }
        }

        #endregion

        #region IServiceProvider Members

        object IServiceProvider.GetService(Type serviceType)
        {
            return ServiceProviderGetService(serviceType);
        }

        /// <summary>
        ///     Gets the service provided by this TypeEditorHost.
        /// </summary>
        /// <param name="serviceType">type of service being requested.</param>
        /// <returns></returns>
        protected object ServiceProviderGetService(Type serviceType)
        {
            // services we support
            if (serviceType == typeof(IWindowsFormsEditorService)
                || serviceType == typeof(ITypeDescriptorContext))
            {
                return this;
            }

            // delegate to our site for other services
            if (Site != null)
            {
                return Site.GetService(serviceType);
            }

            return null;
        }

        #endregion

        #region IWindowsFormsEditorService Members

        void IWindowsFormsEditorService.DropDownControl(Control control)
        {
            DropDownControl(control);
        }

        /// <summary>
        ///     Display the given control in a dropdown. Implements IWindowsFormsEditorService.DropDownControl
        /// </summary>
        /// <param name="control">The control to display</param>
        protected void DropDownControl(Control control)
        {
            if (_dropDown != control)
            {
                SetComponent(control);
            }

            DisplayDropDown();
        }

        void IWindowsFormsEditorService.CloseDropDown()
        {
            CloseDropDown();
        }

        /// <summary>
        ///     Implements IWindowsFormsEditorService.CloseDropDown
        /// </summary>
        protected void CloseDropDown()
        {
            CloseDropDown(false);
        }

        DialogResult IWindowsFormsEditorService.ShowDialog(Form dialog)
        {
            return ShowDialog(dialog);
        }

        /// <summary>
        ///     Show the type editor dialog. Implements IWindowsFormsEditorService.ShowDialog.
        /// </summary>
        /// <param name="dialog"></param>
        /// <returns></returns>
        protected DialogResult ShowDialog(Form dialog)
        {
            _dialogResult = DialogResult.None;

            // try to shift down if sitting right on top of existing owner.
            if (dialog.StartPosition == FormStartPosition.CenterScreen)
            {
                Control topControl = this;
                if (topControl != null)
                {
                    while (topControl.Parent != null)
                    {
                        topControl = topControl.Parent;
                    }
                    if (topControl.Size.Equals(dialog.Size))
                    {
                        dialog.StartPosition = FormStartPosition.Manual;
                        var location = topControl.Location;
                        // CONSIDER what constant to get here?
                        location.Offset(25, 25);
                        dialog.Location = location;
                    }
                }
            }

            var service = (IUIService)((IServiceProvider)this).GetService(typeof(IUIService));
            try
            {
                _inShowDialog = true;
                if (service != null)
                {
                    _dialogResult = service.ShowDialog(dialog);
                }
                else
                {
                    _dialogResult = dialog.ShowDialog(this);
                }
            }
            finally
            {
                _inShowDialog = false;
            }

            // give focus back to the text box
            if (_edit != null)
            {
                _edit.Focus();
            }
            else
            {
                Focus();
            }

            return _dialogResult;
        }

        #endregion

        #region Implementation of IInPlaceControl

        /// <summary>
        ///     Select all text in the edit area
        /// </summary>
        public void SelectAllText()
        {
            // force handle creation here, because we need the edit control.
            if (!IsHandleCreated)
            {
                CreateHandle();
            }

            // Note that m_edit.SelectAll() is not quite the same as the following code
            if (Edit != null)
            {
                Edit.Focus();
                var hwndEdit = Edit.Handle;
                NativeMethods.SendMessage(hwndEdit, NativeMethods.EM_SETSEL, -1, -1); // move to the end
                NativeMethods.SendMessage(hwndEdit, NativeMethods.EM_SETSEL, 0, -1); // select all text
            }
        }

        /// <summary>
        ///     Return the position of the current selection start
        /// </summary>
        public int SelectionStart
        {
            get { return (Edit == null) ? 0 : Edit.SelectionStart; }
            set
            {
                // force handle creation here, because we need the edit control.
                if (!IsHandleCreated)
                {
                    CreateHandle();
                }

                if (Edit != null)
                {
                    Edit.SelectionStart = value;
                }
            }
        }

        /// <summary>
        ///     Return maximum length of text in the edit field
        /// </summary>
        public int MaxTextLength
        {
            get { return (Edit == null) ? 0 : Edit.MaxLength; }
            set
            {
                // force handle creation here, because we need the edit control.
                if (!IsHandleCreated)
                {
                    CreateHandle();
                }

                if (Edit != null)
                {
                    Edit.MaxLength = value;
                }
            }
        }

        /// <summary>
        /// </summary>
        public Rectangle FormattingRectangle
        {
            get
            {
                // UNDONE: Untrue statement
                // not necessary since we're always sizing this
                // based on cell size, not text size
                return Rectangle.Empty;
            }
        }

        /// <summary>
        /// </summary>
        int IVirtualTreeInPlaceControl.ExtraEditWidth
        {
            get { return ExtraEditWidth; }
        }

        /// <summary>
        ///     Implementation of IVirtualTreeInPlaceControl.ExtraEditWidth
        /// </summary>
        protected int ExtraEditWidth
        {
            get
            {
                var retVal = 0;

                if (EditControlStyle == TypeEditorHostEditControlStyle.Editable
                    || EditControlStyle == TypeEditorHostEditControlStyle.ReadOnlyEdit)
                {
                    retVal += VirtualTreeInPlaceControlHelper.DefaultExtraEditWidth;
                }

                if (EditStyle != UITypeEditorEditStyle.None)
                {
                    retVal += 16; // bitmap size for button
                }

                return retVal;
            }
        }

        #endregion

        #region Boilerplate InPlaceControl code

        private readonly VirtualTreeInPlaceControlHelper m_inPlaceHelper;

        /// <summary>
        ///     Parent VirtualTreeControl
        /// </summary>
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
            get { return m_inPlaceHelper.Parent; }
            set { m_inPlaceHelper.Parent = value; }
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
        ///     control. Use 0 for a timer, selection change, or explicit
        ///     launch. Used to support mouse behavior while a control is in-place
        ///     activating in response to a mouse click.
        /// </summary>
        protected int LaunchedByMessage
        {
            get { return m_inPlaceHelper.LaunchedByMessage; }
            set { m_inPlaceHelper.LaunchedByMessage = value; }
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
            get { return m_inPlaceHelper.Dirty; }
            set { m_inPlaceHelper.Dirty = value; }
        }

        /// <summary>
        ///     Returns the in-place control itself (this)
        /// </summary>
        public Control InPlaceControl
        {
            get { return m_inPlaceHelper.InPlaceControl; }
        }

        /// <summary>
        ///     Settings indicating how the inplace control interacts with the tree control. Defaults
        ///     to SizeToText | DisposeControl.
        /// </summary>
        public VirtualTreeInPlaceControls Flags
        {
            get { return m_inPlaceHelper.Flags; }
            set { m_inPlaceHelper.Flags = value; }
        }

        /// <summary>
        ///     Pass KeyPress events on to the parent control for special handling
        /// </summary>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (!e.Handled)
            {
                e.Handled = m_inPlaceHelper.OnKeyPress(e);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            m_inPlaceHelper.OnTextChanged();
            base.OnTextChanged(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            if (!_inShowDialog
                && !ContainsFocus
                && !DropDownContainsFocus)
            {
                m_inPlaceHelper.OnLostFocus();
            }
            base.OnLostFocus(e);
        }

        #endregion // Boilerplace InPlaceControl code

        /// <summary>
        ///     Derived class so we can customize the accessibility keyboard shortcut
        /// </summary>
        [SuppressMessage("Whitehorse.CustomRules", "WH03:WinFormControlCatchUnhandledExceptions",
            Justification = "All but critical exceptions are caught.")]
        private class DropDownButton : Button
        {
            protected override AccessibleObject CreateAccessibilityInstance()
            {
                return new DropDownButtonAccessibleObject(this);
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
                    base.WndProc(ref m);
                }
                catch (Exception e)
                {
                    if (CriticalException.IsCriticalException(e))
                    {
                        throw;
                    }

                    VirtualTreeControl.DisplayException(Parent.Site, e);
                }
            }
        }

        private class DropDownButtonAccessibleObject : ControlAccessibleObject
        {
            public DropDownButtonAccessibleObject(Control inner)
                : base(inner)
            {
            }

            /// <summary>
            ///     return Alt+Down as our keyboard shortcut
            /// </summary>
            public override string KeyboardShortcut
            {
                get { return VirtualTreeStrings.GetString(VirtualTreeStrings.DropDownAccessibleShortcut); }
            }
        }
    }

    internal interface IMouseHookClient
    {
        // return true if the click is handled, false
        // to pass it on
        bool OnClickHooked();
    }

    /// <summary>
    ///     Borrowed from property browser.  Installs a mouse hook so we can close the drop down if the
    ///     user clicks the mouse while it's open
    /// </summary>
    internal class MouseHooker : IDisposable
    {
        private readonly Control _control;
        private readonly IMouseHookClient _client;

        internal int ThisProcessId = 0;
        private GCHandle _mouseHookRoot;
        [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
        private IntPtr _mouseHookHandle = IntPtr.Zero;

        private const bool HookDisable = false;

        private bool _processing;

        public MouseHooker(Control control, IMouseHookClient client)
        {
            _control = control;
            _client = client;
        }

        public virtual bool HookMouseDown
        {
            get { return _mouseHookHandle != IntPtr.Zero; }
            set
            {
                if (value && !HookDisable)
                {
                    HookMouse();
                }
                else
                {
                    UnhookMouse();
                }
            }
        }

        #region IDisposable

        ~MouseHooker()
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
                UnhookMouse();
            }
        }

        #endregion

        /// <devdoc>
        ///     Sets up the needed windows hooks to catch messages.
        /// </devdoc>
        /// <internalonly />
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.GetWindowThreadProcessId(System.Runtime.InteropServices.HandleRef,System.Int32@)")]
        private void HookMouse()
        {
            lock (this)
            {
                if (_mouseHookHandle != IntPtr.Zero)
                {
                    return;
                }

                if (ThisProcessId == 0)
                {
                    NativeMethods.GetWindowThreadProcessId(new HandleRef(_control, _control.Handle), out ThisProcessId);
                }

                NativeMethods.HookProc hook = new MouseHookObject(this).Callback;
                _mouseHookRoot = GCHandle.Alloc(hook);

                _mouseHookHandle = NativeMethods.SetWindowsHookEx(
                    NativeMethods.WH_MOUSE,
                    hook,
                    NativeMethods.NullHandleRef,
                    NativeMethods.GetCurrentThreadId());
                Debug.Assert(_mouseHookHandle != IntPtr.Zero, "Failed to install mouse hook");
            }
        }

        /// <devdoc>
        ///     HookProc used for catch mouse messages.
        /// </devdoc>
        /// <internalonly />
        private IntPtr MouseHookProc(int nCode, IntPtr wparam, IntPtr lparam)
        {
            if (nCode == NativeMethods.HC_ACTION)
            {
                var mhs = (NativeMethods.MouseHookStruct)Marshal.PtrToStructure(lparam, typeof(NativeMethods.MouseHookStruct));
                if (mhs != null)
                {
                    switch ((int)wparam)
                    {
                        case NativeMethods.WM_LBUTTONDOWN:
                        case NativeMethods.WM_MBUTTONDOWN:
                        case NativeMethods.WM_RBUTTONDOWN:
                        case NativeMethods.WM_NCLBUTTONDOWN:
                        case NativeMethods.WM_NCMBUTTONDOWN:
                        case NativeMethods.WM_NCRBUTTONDOWN:
                        case NativeMethods.WM_MOUSEACTIVATE:
                            if (ProcessMouseDown(mhs.handle))
                            {
                                return (IntPtr)1;
                            }
                            break;
                    }
                }
            }

            return NativeMethods.CallNextHookEx(new HandleRef(this, _mouseHookHandle), nCode, wparam, lparam);
        }

        /// <devdoc>
        ///     Removes the windowshook that was installed.
        /// </devdoc>
        /// <internalonly />
        private void UnhookMouse()
        {
            lock (this)
            {
                if (_mouseHookHandle != IntPtr.Zero)
                {
                    NativeMethods.UnhookWindowsHookEx(new HandleRef(this, _mouseHookHandle));
                    _mouseHookRoot.Free();
                    _mouseHookHandle = IntPtr.Zero;
                }
            }
        }

        private static MouseButtons GetAsyncMouseState()
        {
            MouseButtons buttons = 0;

            // SECURITYNOTE : only let state of MouseButtons out...
            //
            if (NativeMethods.GetKeyState((int)Keys.LButton) < 0)
            {
                buttons |= MouseButtons.Left;
            }
            if (NativeMethods.GetKeyState((int)Keys.RButton) < 0)
            {
                buttons |= MouseButtons.Right;
            }
            if (NativeMethods.GetKeyState((int)Keys.MButton) < 0)
            {
                buttons |= MouseButtons.Middle;
            }
            if (NativeMethods.GetKeyState((int)Keys.XButton1) < 0)
            {
                buttons |= MouseButtons.XButton1;
            }
            if (NativeMethods.GetKeyState((int)Keys.XButton2) < 0)
            {
                buttons |= MouseButtons.XButton2;
            }
            return buttons;
        }

        //
        // Here is where we force validation on any clicks outside the control
        //
        private bool ProcessMouseDown(IntPtr hWnd)
        {
            // com+ 12678
            // if we put up the "invalid" message box, it appears this 
            // method is getting called re-entrantly when it shouldn't be.
            // this prevents us from recursing.
            //
            if (_processing)
            {
                return false;
            }

            var hWndAtPoint = hWnd;
            var handle = _control.Handle;
            var ctrlAtPoint = Control.FromHandle(hWndAtPoint);

            // if it's us or one of our children, just process as normal
            //
            if (hWndAtPoint != handle
                && !_control.Contains(ctrlAtPoint)
                && !TypeEditorHost.HandleContains(_control.Handle, handle))
            {
                Debug.Assert(ThisProcessId != 0, "Didn't get our process id!");

                // make sure the window is in our process
                int pid;
                var hr = NativeMethods.GetWindowThreadProcessId(new HandleRef(null, hWndAtPoint), out pid);

                // if this isn't our process, unhook the mouse.
                if (!NativeMethods.Succeeded(hr) || pid != ThisProcessId)
                {
                    HookMouseDown = false;
                    return false;
                }

                // if this a sibling control (e.g. the drop down or buttons), just forward the message and skip the commit
                var needCommit = ctrlAtPoint == null || !IsSiblingControl(_control, ctrlAtPoint);
                try
                {
                    _processing = true;
                    if (needCommit)
                    {
                        if (_client.OnClickHooked())
                        {
                            return true; // there was an error, so eat the mouse
                        }
                        else
                        {
                            // Returning false lets the message go to its destination.  Only
                            // return false if there is still a mouse button down.  That might not be the
                            // case if committing the entry opened a modal dialog.
                            var state = GetAsyncMouseState();

                            return (int)state == 0;
                        }
                    }
                }
                finally
                {
                    _processing = false;
                }

                // cancel our hook at this point
                HookMouseDown = false;
            }
            return false;
        }

        private static bool IsSiblingControl(Control c1, Control c2)
        {
            var parent1 = c1.Parent;
            var parent2 = c2.Parent;

            while (parent2 != null)
            {
                if (parent1 == parent2)
                {
                    return true;
                }

                parent2 = parent2.Parent;
            }
            return false;
        }

        /// <devdoc>
        ///     Forwards messageHook calls to ToolTip.messageHookProc
        /// </devdoc>
        /// <internalonly />
        private class MouseHookObject
        {
            internal readonly WeakReference reference;

            public MouseHookObject(MouseHooker parent)
            {
                reference = new WeakReference(parent, false);
            }

            public virtual IntPtr Callback(int nCode, IntPtr wparam, IntPtr lparam)
            {
                var ret = IntPtr.Zero;
                // try 
                // {
                var control = (MouseHooker)reference.Target;
                if (control != null)
                {
                    ret = control.MouseHookProc(nCode, wparam, lparam);
                }
                // }
                // catch (Exception) 
                // {
                // ignore
                // }
                return ret;
            }
        }
    }

    /// <summary>
    ///     Borrowed from the property browser.  Control to contain a child control which can be used
    ///     to do editing
    /// </summary>
    [SuppressMessage("Whitehorse.CustomRules", "WH03:WinFormControlCatchUnhandledExceptions",
        Justification = "All but critical exceptions are caught.")]
    internal class DropDownHolder : Form, IMouseHookClient
    {
        internal Control CurrentControl = null;
        internal const int Border = 1;
        private readonly TypeEditorHost _dropDownParent;

        // we use this to hook mouse downs, etc. to know when to close the dropdown.
        private readonly MouseHooker _mouseHooker;

        // all the resizing goo...
        private bool _resizable = true; // true if we're showing the resize widget.
        private bool _resizing; // true if we're in the middle of a resize operation.
        private bool _resizeUp; // true if the dropdown is above the grid row, which means the resize widget is at the top.
        private Point _dragStart = Point.Empty; // the point at which the drag started to compute the delta
        private Rectangle _dragBaseRect = Rectangle.Empty; // the original bounds of our control.
        private int _currentMoveType = MoveTypeNone; // what type of move are we processing? left, bottom, or both?

        // the width of the vertical resize area at the bottom
        private static readonly int _resizeBorderSize = SystemInformation.HorizontalScrollBarHeight / 2;

        // the minimum size for the control
        private static readonly Size _minDropDownSize = 
            new Size(SystemInformation.VerticalScrollBarWidth * 4, SystemInformation.HorizontalScrollBarHeight * 4);

        // our cached size grip glyph.  Control paint only does right bottom glyphs, so we cache a mirrored one.
        // See GetSizeGripGlyph
        private Bitmap _sizeGripGlyph;

        internal const int DropDownHolderBorder = 1;
        private const int MoveTypeNone = 0x0;
        private const int MoveTypeBottom = 0x1;
        private const int MoveTypeLeft = 0x2;
        private const int MoveTypeTop = 0x4;

        internal DropDownHolder(TypeEditorHost dropDownParent)
        {
            ShowInTaskbar = false;
            ControlBox = false;
            MinimizeBox = false;
            MaximizeBox = false;
            Text = String.Empty;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual; // set this to avoid being moved when our handle is created.
            _mouseHooker = new MouseHooker(this, this);
            Visible = false;
            BackColor = VSColorTheme.GetThemedColor(EnvironmentColors.ComboBoxBackgroundColorKey);
            _dropDownParent = dropDownParent;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= NativeMethods.WS_EX_TOOLWINDOW;
                cp.Style |= NativeMethods.WS_POPUP | NativeMethods.WS_BORDER;
                if (OSFeature.IsPresent(SystemParameter.DropShadow))
                {
                    cp.ClassStyle |= NativeMethods.CS_DROPSHADOW;
                }

                if (_dropDownParent != null)
                {
                    cp.Parent = _dropDownParent.Handle;
                }
                return cp;
            }
        }

        public virtual bool HookMouseDown
        {
            get { return _mouseHooker.HookMouseDown; }
            set { _mouseHooker.HookMouseDown = value; }
        }

        /// <devdoc>
        ///     This gets set to true if there isn't enough space below the currently selected
        ///     row for the drop down, so it appears above the row.  In this case, we make the resize
        ///     grip appear at the top left.
        /// </devdoc>
        public bool ResizeUp
        {
            get { return _resizeUp; }
            set
            {
                if (_resizeUp != value)
                {
                    // clear the glyph so we regenerate it.
                    //
                    _sizeGripGlyph = null;
                    _resizeUp = value;
                    if (_resizable)
                    {
                        DockPadding.Bottom = 0;
                        DockPadding.Top = 0;
                        if (value)
                        {
                            DockPadding.Top = SystemInformation.HorizontalScrollBarHeight;
                        }
                        else
                        {
                            DockPadding.Bottom = SystemInformation.HorizontalScrollBarHeight;
                        }
                    }
                }
            }
        }

        protected override void DestroyHandle()
        {
            _mouseHooker.Dispose();
            base.DestroyHandle();
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Data.Entity.Design.VisualStudio.NativeMethods.MsgWaitForMultipleObjects(System.Int32,System.Int32,System.Boolean,System.Int32,System.Int32)")]
        public void DoModalLoop()
        {
            // Push a modal loop.  This kind of stinks, but I think it is a
            // better user model than returning from DropDownControl immediately.
            //  
            while (Visible)
            {
                Application.DoEvents();
                NativeMethods.MsgWaitForMultipleObjects(1, 0, true, 250, NativeMethods.QS_ALLINPUT);
            }
        }

        public virtual Control Component
        {
            get { return CurrentControl; }
        }

        /// <devdoc>
        ///     Get a glyph for sizing the lower left hand grip.  The code in ControlPaint only does lower-right glyphs
        ///     so we do some GDI+ magic to take that glyph and mirror it.  That way we can still share the code (in case it changes for theming, etc),
        ///     not have any special cases, and possibly solve world hunger.
        /// </devdoc>
        private Bitmap GetSizeGripGlyph(Graphics g)
        {
            if (_sizeGripGlyph != null)
            {
                return _sizeGripGlyph;
            }

            var scrollBarWidth = SystemInformation.VerticalScrollBarWidth;
            var scrollBarHeight = SystemInformation.HorizontalScrollBarHeight;

            // create our drawing surface based on the current graphics context.
            //
            _sizeGripGlyph = new Bitmap(scrollBarWidth, scrollBarHeight, g);
            using (var glyphGraphics = Graphics.FromImage(_sizeGripGlyph))
            {
                // mirror the image around the x-axis to get a gripper handle that works
                // for the lower left.
                using (var m = new Matrix())
                {

                    // basically, mirroring is just scaling by -1 on the X-axis.  So any point that's like (10, 10) goes to (-10, 10). 
                    // that mirrors it, but also moves everything to the negative axis, so we just bump the whole thing over by it's width.
                    // 
                    // the +1 is because things at (0,0) stay at (0,0) since [0 * -1 = 0] so we want to get them over to the other side too.
                    //
                    // resizeUp causes the image to also be mirrored vertically so the grip can be used as a top-left grip instead of bottom-left.
                    //
                    m.Translate(scrollBarWidth + 1, (_resizeUp ? scrollBarHeight + 1 : 0));
                    m.Scale(-1, (ResizeUp ? -1 : 1));
                    glyphGraphics.Transform = m;
                    ControlPaint.DrawSizeGrip(glyphGraphics, BackColor, 0, 0, scrollBarWidth, scrollBarHeight);
                    glyphGraphics.ResetTransform();
                }
            }

            _sizeGripGlyph.MakeTransparent(BackColor);
            return _sizeGripGlyph;
        }

        public virtual bool GetUsed()
        {
            return (CurrentControl != null);
        }

        public virtual void FocusComponent()
        {
            if (CurrentControl != null && Visible)
            {
                CurrentControl.Focus();
            }
        }

        bool IMouseHookClient.OnClickHooked()
        {
            _dropDownParent.CloseDropDown(true);
            return false;
        }

        private void OnCurrentControlResize(object o, EventArgs e)
        {
            if (CurrentControl != null
                && !_resizing
                && !CurrentControl.Disposing)
            {
                var oldWidth = Width;
                var newSize = new Size(2 * DropDownHolderBorder + CurrentControl.Width, 2 * DropDownHolderBorder + CurrentControl.Height);

                if (_resizable)
                {
                    newSize.Height += SystemInformation.HorizontalScrollBarHeight;
                }

                try
                {
                    _resizing = true;
                    SuspendLayout();
                    Size = newSize;
                }
                finally
                {
                    _resizing = false;
                    ResumeLayout(false);
                }
                Left -= (Width - oldWidth);
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            try
            {
                _resizing = true;
                base.OnLayout(levent);
            }
            finally
            {
                _resizing = false;
            }
        }

        /// <devdoc>
        ///     Just figure out what kind of sizing we would do at a given drag location.
        /// </devdoc>
        private int MoveTypeFromPoint(int x, int y)
        {
            var scrollBarWidth = SystemInformation.VerticalScrollBarWidth;
            var scrollBarHeight = SystemInformation.HorizontalScrollBarHeight;
            var bRect = new Rectangle(0, Height - scrollBarHeight, scrollBarWidth, scrollBarHeight);
            var tRect = new Rectangle(0, 0, scrollBarWidth, scrollBarHeight);

            if (!ResizeUp
                && bRect.Contains(x, y))
            {
                return MoveTypeLeft | MoveTypeBottom;
            }
            else if (ResizeUp && tRect.Contains(x, y))
            {
                return MoveTypeLeft | MoveTypeTop;
            }
            else if (!ResizeUp
                     && Math.Abs(Height - y) < _resizeBorderSize)
            {
                return MoveTypeBottom;
            }
            else if (ResizeUp && Math.Abs(y) < _resizeBorderSize)
            {
                return MoveTypeTop;
            }

            return MoveTypeNone;
        }

        /// <devdoc>
        ///     Decide if we're going to be sizing at the given point, and if so, Capture and safe our current state.
        /// </devdoc>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _currentMoveType = MoveTypeFromPoint(e.X, e.Y);
                if (_currentMoveType != MoveTypeNone)
                {
                    _dragStart = PointToScreen(new Point(e.X, e.Y));
                    _dragBaseRect = Bounds;
                    Capture = true;
                }
                else
                {
                    _dropDownParent.CloseDropDown(true);
                }
            }

            base.OnMouseDown(e);
        }

        /// <devdoc>
        ///     Either set the cursor or do a move, depending on what our currentMoveType is/
        /// </devdoc>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (! _resizable)
            {
                // don't do any resizing ops
            }
            else
                // not moving so just set the cursor.
                //
                if (_currentMoveType == MoveTypeNone)
                {
                    var cursorMoveType = MoveTypeFromPoint(e.X, e.Y);

                    switch (cursorMoveType)
                    {
                        case (MoveTypeLeft | MoveTypeBottom):
                            Cursor = Cursors.SizeNESW;
                            break;

                        case MoveTypeBottom:
                        case MoveTypeTop:
                            Cursor = Cursors.SizeNS;
                            break;

                        case MoveTypeTop | MoveTypeLeft:
                            Cursor = Cursors.SizeNWSE;
                            break;

                        default:
                            Cursor = null;
                            break;
                    }
                }
                else
                {
                    var dragPoint = PointToScreen(new Point(e.X, e.Y));
                    var newBounds = Bounds;

                    // we're in a move operation, so do the resize.
                    //
                    if ((_currentMoveType & MoveTypeBottom) == MoveTypeBottom)
                    {
                        newBounds.Height = Math.Max(_minDropDownSize.Height, _dragBaseRect.Height + (dragPoint.Y - _dragStart.Y));
                    }

                    // for left and top moves, we actually have to resize and move the form simultaneously.
                    // do to that, we compute the xdelta, and apply that to the base rectangle if it's not going to
                    // make the form smaller than the minimum.
                    //
                    if ((_currentMoveType & MoveTypeTop) == MoveTypeTop)
                    {
                        var delta = dragPoint.Y - _dragStart.Y;

                        if ((_dragBaseRect.Height - delta) > _minDropDownSize.Height)
                        {
                            newBounds.Y = _dragBaseRect.Top + delta;
                            newBounds.Height = _dragBaseRect.Height - delta;
                        }
                    }

                    if ((_currentMoveType & MoveTypeLeft) == MoveTypeLeft)
                    {
                        var delta = dragPoint.X - _dragStart.X;

                        if ((_dragBaseRect.Width - delta) > _minDropDownSize.Width)
                        {
                            newBounds.X = _dragBaseRect.Left + delta;
                            newBounds.Width = _dragBaseRect.Width - delta;
                        }
                    }

                    if (newBounds != Bounds)
                    {
                        try
                        {
                            _resizing = true;
                            Bounds = newBounds;
                        }
                        finally
                        {
                            _resizing = false;
                        }
                    }

                    // Redraw!
                    //
                    var scrollBarHeight = SystemInformation.HorizontalScrollBarHeight;

                    Invalidate(new Rectangle(0, Height - scrollBarHeight, Width, scrollBarHeight));
                }

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            // just clear the cursor back to the default.
            //
            Cursor = null;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
            {
                // reset the world.
                //
                _currentMoveType = MoveTypeNone;
                _dragStart = Point.Empty;
                _dragBaseRect = Rectangle.Empty;
                Capture = false;
            }
        }

        /// <devdoc>
        ///     Just paint and draw our glyph.
        /// </devdoc>
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            if (_resizable)
            {
                var scrollBarWidth = SystemInformation.VerticalScrollBarWidth;
                var scrollBarHeight = SystemInformation.HorizontalScrollBarHeight;
                var lRect = new Rectangle(0, ResizeUp ? 0 : Height - scrollBarHeight, scrollBarWidth, scrollBarHeight);

                pe.Graphics.DrawImage(GetSizeGripGlyph(pe.Graphics), lRect);
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            try
            {
                if ((keyData & (Keys.Shift | Keys.Control | Keys.Alt)) == 0)
                {
                    var accept = false;
                    var doClose = false;
                    switch (keyData & Keys.KeyCode)
                    {
                        case Keys.Escape:
                            doClose = true;
                            //accept = false; // default value
                            break;
                        case Keys.Enter:
                            doClose = accept = true;
                            break;
                    }
                    if (doClose)
                    {
                        _dropDownParent.CloseDropDown(accept);
                        return true;
                    }
                }
                return base.ProcessDialogKey(keyData);
            }
            catch (Exception e)
            {
                if (CriticalException.IsCriticalException(e))
                {
                    throw;
                }

                VirtualTreeControl.DisplayException(_dropDownParent.Site, e);
                return false;
            }
        }

        public void SetComponent(Control ctl, bool resizable)
        {
            _resizable = resizable;

            // clear any existing control we have
            //
            if (CurrentControl != null)
            {
                CurrentControl.Resize -= OnCurrentControlResize;
                Controls.Remove(CurrentControl);
                CurrentControl = null;
            }

            // now set up the new control, top to bottom
            //
            if (ctl != null)
            {
                var sz = new Size(2 * DropDownHolderBorder + ctl.Width, 2 * DropDownHolderBorder + ctl.Height);

                // set the size stuff.
                //
                try
                {
                    SuspendLayout();
                    // if we're resizable, add the space for the widget. Make sure
                    // this happens with layout off or you get the side effect of shrinking
                    // the contained control.
                    if (resizable)
                    {
                        var hscrollHeight = SystemInformation.HorizontalScrollBarHeight;
                        sz.Height += hscrollHeight + hscrollHeight;
                            // UNDONE: This is bizarre. But if we don't double the height adjustment, we lose that much each time the control is shown

                        // we use dockpadding to save space to draw the widget.
                        //
                        if (ResizeUp)
                        {
                            DockPadding.Top = hscrollHeight;
                        }
                        else
                        {
                            DockPadding.Bottom = hscrollHeight;
                        }
                    }
                    Size = sz;
                    ctl.Dock = DockStyle.Fill;
                    ctl.Visible = true;
                    Controls.Add(ctl);
                }
                finally
                {
                    ResumeLayout(true);
                }
                CurrentControl = ctl;

                // hook the resize event.
                //
                CurrentControl.Resize += OnCurrentControlResize;
            }

            Enabled = CurrentControl != null;
        }

        /// <summary>
        ///     Control.WndProc override
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            try
            {
                if (m.Msg == NativeMethods.WM_ACTIVATE)
                {
                    // SetState(STATE_MODAL, true);
                    var activatedControl = FromHandle(m.LParam);
                    if (Visible
                        && NativeMethods.UnsignedLOWORD(m.WParam) == NativeMethods.WA_INACTIVE
                        && (activatedControl == null || (!Contains(activatedControl) && !TypeEditorHost.HandleContains(Handle, m.LParam))))
                    {
                        // Notification occurs when the drop-down holder
                        // is de-activated via a click outside the drop-down area.
                        // call CloseDropDown(true) to commit any changes.
                        _dropDownParent.CloseDropDown(true);
                        return;
                    }
                }
                else if (m.Msg == NativeMethods.WM_CLOSE)
                {
                    // don't let an ALT-F4 get you down
                    //
                    if (Visible)
                    {
                        _dropDownParent.CloseDropDown(false);
                    }
                    return;
                }

                base.WndProc(ref m);
            }
            catch (Exception e)
            {
                if (CriticalException.IsCriticalException(e))
                {
                    throw;
                }

                VirtualTreeControl.DisplayException(_dropDownParent.Site, e);
            }
        }
    }

    /// <summary>
    ///     Edit control displayed in the TypeEditorHost.  Just a TextBox with some additional
    ///     key message processing for opening the drop down.
    /// </summary>
    [SuppressMessage("Whitehorse.CustomRules", "WH03:WinFormControlCatchUnhandledExceptions",
        Justification = "All but critical exceptions are caught.")]
    internal class TypeEditorHostTextBox : TextBox
    {
        private readonly TypeEditorHost _dropDownParent;

        /// <summary>
        ///     Edit control displayed in the TypeEditorHost.  Just a TextBox with some additional
        ///     key message processing for opening the drop down.
        /// </summary>
        public TypeEditorHostTextBox(TypeEditorHost dropDownParent)
        {
            _dropDownParent = dropDownParent;

            BackColor = VSColorTheme.GetThemedColor(EnvironmentColors.ComboBoxBackgroundColorKey);
            ForeColor = VSColorTheme.GetThemedColor(EnvironmentColors.ComboBoxTextColorKey);
        }

        /// <summary>
        ///     Key processing.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase",
            Justification =
                "[rakeshna] Because the rest of our Whitehorse code is only run in a full-trusted environment in Whidbey, we do not gain any security benefit from applying the LinkDemand attribute."
            )]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Z: // ensure that the edit control handles undo if the text box is dirty.
                    if (((IVirtualTreeInPlaceControlDefer)_dropDownParent).Dirty
                        && ((keyData & Keys.Control) != 0)
                        && ((keyData & Keys.Shift) == 0)
                        && ((keyData & Keys.Alt) == 0))
                    {
                        Undo();
                        return true;
                    }

                    break;

                case Keys.A: // ensure that the edit control handles select all.
                    if (((keyData & Keys.Control) != 0)
                        && ((keyData & Keys.Shift) == 0)
                        && ((keyData & Keys.Alt) == 0))
                    {
                        SelectAll();
                        return true;
                    }

                    break;

                case Keys.F4: // F4 opens the drop down
                    if ((keyData & (Keys.Shift | Keys.Control | Keys.Alt)) == 0)
                    {
                        _dropDownParent.OpenDropDown();
                        return true;
                    }

                    break;

                case Keys.Down: // Alt-Down opens the drop down
                    if (((keyData & Keys.Alt) != 0)
                        && ((keyData & Keys.Control) == 0))
                    {
                        _dropDownParent.OpenDropDown();
                        return true;
                    }

                    break;

                case Keys.Delete:
                    NativeMethods.SendMessage(Handle, msg.Msg, msg.WParam.ToInt32(), msg.LParam.ToInt32());
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        ///     Key processing.
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            // for some reason when the Alt key is pressed,
            // WM_KEYDOWN messages aren't going through the 
            // TranslateAccelerator/PreProcessMessage loop.
            // So we handle Alt-Down here.
            switch (e.KeyCode)
            {
                case Keys.Down: // Alt-Down opens the drop down
                    if (((e.KeyData & Keys.Alt) != 0)
                        && ((e.KeyData & Keys.Control) == 0))
                    {
                        _dropDownParent.OpenDropDown();
                        e.Handled = true;
                        return;
                    }

                    break;
            }

            base.OnKeyDown(e);
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
                base.WndProc(ref m);
            }
            catch (Exception e)
            {
                if (CriticalException.IsCriticalException(e))
                {
                    throw;
                }

                VirtualTreeControl.DisplayException(_dropDownParent.Site, e);
            }
        }
    }

    /// <summary>
    ///     Specializes the TypeEditorHost to use a ListBox as the control to drop down,
    ///     making this essentially a ComboBox.
    /// </summary>
    internal class TypeEditorHostListBox : TypeEditorHost
    {
        private readonly ListBox _listBox;
        private TypeConverter _typeConverter;

        /// <summary>
        ///     Creates a new drop-down control to display the given TypeConverter
        /// </summary>
        /// <param name="typeConverter">The TypeConverter instance to retrieve drop-down values from</param>
        /// <param name="propertyDescriptor">Property descriptor used to get/set values in the drop-down.</param>
        /// <param name="instance">Instance object used to get/set values in the drop-down.</param>
        protected internal TypeEditorHostListBox(TypeConverter typeConverter, PropertyDescriptor propertyDescriptor, object instance)
            :
                this(
                typeConverter, propertyDescriptor, instance,
                (typeConverter != null && typeConverter.GetStandardValuesExclusive())
                    ? TypeEditorHostEditControlStyle.ReadOnlyEdit
                    : TypeEditorHostEditControlStyle.Editable)
        {
        }

        /// <summary>
        ///     Creates a new drop-down list to display the given type converter.
        ///     The type converter must support a standard values collection.
        /// </summary>
        /// <param name="typeConverter">The TypeConverter instance to retrieve drop-down values from</param>
        /// <param name="propertyDescriptor">Property descriptor used to get/set values in the drop-down.</param>
        /// <param name="instance">Instance object used to get/set values in the drop-down.</param>
        /// <param name="editControlStyle">Edit control style.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected internal TypeEditorHostListBox(
            TypeConverter typeConverter, PropertyDescriptor propertyDescriptor, object instance,
            TypeEditorHostEditControlStyle editControlStyle)
            :
                base(UITypeEditorEditStyle.DropDown, propertyDescriptor, instance, editControlStyle)
        {
            _typeConverter = typeConverter;

            // UNDONE: currently, this class only supports exclusive values.

            // create the list box
            _listBox = new ListBox { BorderStyle = BorderStyle.None };
            _listBox.MouseUp += OnDropDownMouseUp;

            _listBox.BackColor = VSColorTheme.GetThemedColor(EnvironmentColors.ComboBoxBackgroundColorKey);
            _listBox.ForeColor = VSColorTheme.GetThemedColor(EnvironmentColors.ComboBoxTextColorKey);

            if (_typeConverter != null
                && _typeConverter.GetStandardValuesSupported(this))
            {
                // populate it with values from the type converter
                foreach (var value in _typeConverter.GetStandardValues())
                {
                    _listBox.Items.Add(value);
                }
            }

            // set list box as the drop control
            SetComponent(_listBox);
        }

        private void OnDropDownMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                CloseDropDown(true);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_listBox != null)
                {
                    _listBox.MouseUp -= OnDropDownMouseUp;
                    _listBox.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        protected override int DropDownHeight
        {
            get
            {
                var itemCount = _listBox.Items.Count;
                return itemCount < 8 ? (itemCount * _listBox.ItemHeight) + (_listBox.ItemHeight / 2) : 8 * _listBox.ItemHeight;
            }
        }

        /// <summary>
        ///     Do not call the DropDownWidth override
        /// </summary>
        protected override bool IgnoreDropDownWidth
        {
            get { return true; }
        }

        protected override void OnEditKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    if (_listBox.SelectedIndex > 0)
                    {
                        _listBox.SelectedIndex--;
                        Text = _listBox.Text;
                        if (Edit != null)
                        {
                            Edit.SelectionStart = 0;
                            Edit.SelectionLength = Edit.Text.Length;
                        }
                    }
                    e.Handled = true;
                    break;
                case Keys.Down:
                    if (_listBox.SelectedIndex < _listBox.Items.Count - 1)
                    {
                        _listBox.SelectedIndex++;
                        Text = _listBox.Text;
                        if (Edit != null)
                        {
                            Edit.SelectionStart = 0;
                            Edit.SelectionLength = Edit.Text.Length;
                        }
                    }
                    e.Handled = true;
                    break;
            }
            base.OnEditKeyDown(e);
        }

        protected override void OnEditKeyPress(KeyPressEventArgs e)
        {
            base.OnEditKeyPress(e);

            if (!e.Handled)
            {
                var caret = (Edit == null) ? 0 : Edit.SelectionStart;

                string currentString = null;
                if (caret == 0)
                {
                    currentString = new string(e.KeyChar, 1);
                }
                else if (caret > 0)
                {
                    currentString = Edit.Lines[0].Substring(0, caret) + e.KeyChar;
                }

                if (currentString != null
                    && currentString.Length > 0)
                {
                    var index = _listBox.SelectedIndex;

                    if (index == -1)
                    {
                        index = 0;
                    }

                    var endIndex = index;

                    var foundMatch = false;

                    while (true)
                    {
                        // TODO : refine this algorithm.  Should we assume the
                        // list is sorted?
                        var itemText = _listBox.Items[index].ToString();

                        if (currentString.Length <= itemText.Length
                            && String.Compare(itemText, 0, currentString, 0, currentString.Length, true, CultureInfo.CurrentUICulture) == 0)
                        {
                            foundMatch = true;
                            break;
                        }

                        index++;

                        if (index == _listBox.Items.Count)
                        {
                            index = 0;
                        }

                        if (index == endIndex)
                        {
                            break;
                        }
                    }

                    if (foundMatch)
                    {
                        _listBox.SelectedIndex = index;
                        Text = _listBox.Text;
                        if (Edit != null)
                        {
                            Edit.SelectionStart = currentString.Length;
                        }
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        ///     Overridden to set up the list index based on current text
        /// </summary>
        protected override void OnOpeningDropDown(EventArgs e)
        {
            // make sure list box is hardened appropriately against exceptions.
            if (!(_listBox.WindowTarget is SafeWindowTarget))
            {
                _listBox.WindowTarget = new SafeWindowTarget(Site, _listBox.WindowTarget);
            }
            // if we have a type converter, populate with values
            if (_listBox.Items.Count > 0)
            {
                SetListBoxIndexForCurrentText();
            }

            base.OnOpeningDropDown(e);
        }

        /// <summary>
        ///     Overridden to set up the list index based on current text
        /// </summary>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            // ensure that list box index is set appropriately
            if (_listBox.Items.Count > 0
                && (_listBox.SelectedIndex == -1
                    || String.Compare(_listBox.Items[_listBox.SelectedIndex].ToString(), Text, true, CultureInfo.CurrentUICulture) != 0))
            {
                SetListBoxIndexForCurrentText();
            }
        }

        private void SetListBoxIndexForCurrentText()
        {
            var selectedIndex = 0;
            var found = false;

            for (var i = 0; i < _listBox.Items.Count; i++)
            {
                var itemText = _listBox.Items[i].ToString();

                if (String.Compare(itemText, Text, true, CultureInfo.CurrentUICulture) == 0)
                {
                    selectedIndex = i;
                    found = true;
                    break;
                }
            }

            if (found)
            {
                _listBox.SelectedIndex = selectedIndex;
            }
        }

        public ListBox.ObjectCollection Items
        {
            get { return _listBox.Items; }
        }
    }
}
