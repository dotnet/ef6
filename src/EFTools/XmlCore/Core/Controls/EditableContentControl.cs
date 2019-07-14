// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Core.Controls
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public enum PerformEditResult
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        NotAttempted,

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        Success, //Successful edit

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        FailRetry, //Failed Edit, however we will let the consumer to retry

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        FailAbort //Failed Edit and we won't let the consumer to retry
    };

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class EndEditFromLostFocusEventArgs : EventArgs
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="newFocusElement">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        public EndEditFromLostFocusEventArgs(IInputElement newFocusElement)
        {
            NewFocusElement = newFocusElement;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public IInputElement NewFocusElement { get; private set; }
    }

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class EditableContentControl : UserControl
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(EditableContentControl), new PropertyMetadata(TextChangedCallback));

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent(
            "TextChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EditableContentControl));

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public static readonly DependencyProperty IsInEditModeProperty = DependencyProperty.Register(
            "IsInEditMode", typeof(bool), typeof(EditableContentControl), new PropertyMetadata(IsInEditModeChangedCallback));

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public static readonly DependencyProperty IsEditModeAllowedProperty = DependencyProperty.Register(
            "IsEditModeAllowed", typeof(bool), typeof(EditableContentControl), new PropertyMetadata(IsEditModeAllowedChangedCallback));

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public Style TextBoxStyle
        {
            get { return (Style)GetValue(TextBoxStyleProperty); }
            set { SetValue(TextBoxStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextBoxStyle.  This enables animation, styling, binding, etc...
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public static readonly DependencyProperty TextBoxStyleProperty =
            DependencyProperty.Register(
                "TextBoxStyle", typeof(Style), typeof(EditableContentControl), new PropertyMetadata(TxtStyleChangedCallback));

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public PerformEditResult EditResult
        {
            get { return (PerformEditResult)GetValue(EditResultProperty); }
            set { SetValue(EditResultProperty, value); }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public static readonly DependencyProperty EditResultProperty =
            DependencyProperty.Register(
                "EditResult", typeof(PerformEditResult), typeof(EditableContentControl),
                new UIPropertyMetadata(PerformEditResult.NotAttempted));

        private UIElement _focusableAncestor;
        private ControlTemplate _savedOriginalTemplate;
        private ScrollViewer _scrollViewer;
        private bool _isEditing;
        private bool _shouldCommit;

        private static string editModeTemplateKey = "EditModeTemplate";
        private static string textBoxName = "textBox";

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public EditableContentControl()
        {
            // create the EditModeTemplate definition; there is a limitation
            // that prevents doing this in xaml (child content cannot have the x:Name
            // property set for user controls that are partially defined using xaml)

            //<UserControl.Resources>
            //    <ControlTemplate x:Key="EditModeTemplate" 
            //                     TargetType="{x:Type UserControl}">
            //        <TextBox x:Name="textBox" 
            //                 Margin="0,0,0,0" 
            //                 LostKeyboardFocus="textBox_LostKeyboardFocus"
            //                 PreviewKeyDown="textBox_PreviewKeyDown">
            //        </TextBox>
            //    </ControlTemplate>
            //</UserControl.Resources>

            var factory = new FrameworkElementFactory(typeof(TextBox));
            factory.SetValue(NameProperty, textBoxName);
            factory.AddHandler(LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(textBox_LostKeyboardFocus));
            factory.AddHandler(PreviewKeyDownEvent, new KeyEventHandler(textBox_PreviewKeyDown));
            var editModeTemplate = new ControlTemplate(typeof(EditableContentControl));
            editModeTemplate.VisualTree = factory;
            editModeTemplate.Seal();
            Resources.Add(editModeTemplateKey, editModeTemplate);
        }

        /***** Text property *****/

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        protected UIElement FocusableAncestor
        {
            get { return _focusableAncestor; }
        }

        internal static void TextChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((EditableContentControl)d).RaiseTextChangedEvent();
            }
        }

        internal static void TxtStyleChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                var edtCont = (EditableContentControl)d;
                if (edtCont.HasContent)
                {
                    var txtBox = edtCont.GetTextBox();
                    if (txtBox != null)
                    {
                        txtBox.Style = (Style)e.NewValue;
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public event RoutedEventHandler TextChanged
        {
            add { AddHandler(TextChangedEvent, value); }
            remove { RemoveHandler(TextChangedEvent, value); }
        }

        private void RaiseTextChangedEvent()
        {
            var newEventArgs = new RoutedEventArgs(TextChangedEvent);
            RaiseEvent(newEventArgs);
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public event EventHandler<EndEditFromLostFocusEventArgs> EndEditFromLostFocus;

        /***** IsInEditMode property *****/

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public bool IsInEditMode
        {
            get { return (bool)GetValue(IsInEditModeProperty); }
            set
            {
                // only put into edit mode if edit mode is allowed
                if (false == value
                    ||
                    (value && IsEditModeAllowed))
                {
                    SetValue(IsInEditModeProperty, value);
                }
            }
        }

        internal static void IsInEditModeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                var editableContentControl = d as EditableContentControl;
                if (editableContentControl != null)
                {
                    var inEditMode = (bool)e.NewValue;
                    if (inEditMode)
                    {
                        editableContentControl.BeginEdit();
                    }
                    else
                    {
                        editableContentControl.OnEndEdit();
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public bool IsEditModeAllowed
        {
            get { return (bool)GetValue(IsEditModeAllowedProperty); }
            set { SetValue(IsEditModeAllowedProperty, value); }
        }

        internal static void IsEditModeAllowedChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="e">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="sender">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="e">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            // find first focusable ancestor for handling F2
            _focusableAncestor = GetFirstFocusableAncestor(this);
            if (_focusableAncestor != null)
            {
                _focusableAncestor.KeyDown += focusableAncestor_KeyDown;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="sender">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="e">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        protected virtual void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_focusableAncestor != null)
            {
                _focusableAncestor.KeyDown -= focusableAncestor_KeyDown;
            }
        }

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

        private void BeginEdit()
        {
            if (!_isEditing)
            {
                if (_savedOriginalTemplate == null)
                {
                    _savedOriginalTemplate = Template;
                }
                Template = Resources[editModeTemplateKey] as ControlTemplate;
                ApplyTemplate();

                var textBox = GetTextBox();
                if (textBox != null)
                {
                    textBox.Text = Text;
                    textBox.SelectAll();

                    Keyboard.Focus(textBox);

                    textBox.MinWidth = 30;
                    textBox.MaxWidth = CalculateTextBoxMaxWidth();
                }

                _isEditing = true;
                _shouldCommit = true;
            }
        }

        private double CalculateTextBoxMaxWidth()
        {
            FindScrollViewer();

            if (_scrollViewer != null)
            {
                // calculate maxWidth so that resizing of the textbox doesn't cause horizontal scrolling
                var maxWidth = _scrollViewer.ViewportWidth;
                var x = TranslatePoint(new Point(0, 0), _scrollViewer).X;
                if (x < 0)
                {
                    x = 0;
                }
                maxWidth -= x;
                if (maxWidth < MinWidth)
                {
                    maxWidth = MinWidth;
                }
                return maxWidth;
            }

            return double.PositiveInfinity;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void FindScrollViewer()
        {
            if (_scrollViewer == null)
            {
                DependencyObject uiElement = this;
                while (uiElement != null
                       && !(uiElement is ScrollViewer))
                {
                    uiElement = VisualTreeHelper.GetParent(uiElement);
                }
                _scrollViewer = uiElement as ScrollViewer;
            }
        }

        // this method would revert back to old text and cancel all errors
        // used for programmatic reversal of edit mode irrespective of error
        internal void RevertAndEndEdit()
        {
            if (_isEditing)
            {
                // set edit mode flag to false to avoid recursion
                _isEditing = false;
                EditResult = PerformEditResult.NotAttempted;
                _shouldCommit = false;

                // rollback the template and edit mode
                Template = _savedOriginalTemplate;
                IsInEditMode = false;
                SetFocusToAncestor();
            }
        }

        private void SetFocusToAncestor()
        {
            if (_focusableAncestor != null)
            {
                Keyboard.Focus(_focusableAncestor);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public void EndEdit()
        {
            OnEndEdit(continueOnError: false);
        }

        private void OnEndEdit(bool continueOnError = true)
        {
            if (_isEditing)
            {
                // set edit mode flag to false to avoid recursion
                _isEditing = false;
                if (_shouldCommit)
                {
                    var textBox = GetTextBox();
                    Text = textBox.Text;
                    var bd = GetBindingExpression(TextProperty);
                    if (bd != null)
                    {
                        // make sure target reflects new value (which may be different
                        // from text entered in the textbox)
                        bd.UpdateTarget();
                    }
                }
                // ensure that if it does not need to be committed, there should not be an error
                Debug.Assert(
                    (_shouldCommit)
                    || (!_shouldCommit && (EditResult == PerformEditResult.Success || EditResult == PerformEditResult.NotAttempted)),
                    "Should not have error if commit is not required");
                // check the error condition and choose to keep in edit mode or go to text mode
                if (EditResult == PerformEditResult.FailRetry && continueOnError)
                {
                    // keep editing
                    IsInEditMode = true;
                    _isEditing = true;
                }
                else
                {
                    // if there is no error, rollback the template and edit mode
                    Template = _savedOriginalTemplate;
                    IsInEditMode = false;
                    SetFocusToAncestor();
                }
            }
        }

        private TextBox GetTextBox()
        {
            var textBox = VisualTreeHelper.GetChild(this, 0) as TextBox;
            Debug.Assert(
                textBox != null && textBox.Name == textBoxName,
                "GetTextBox should only be called if EditModeTemplate is the active control template.");
            textBox.Style = TextBoxStyle;
            return textBox;
        }

        private void textBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var ctMenu = e.NewFocus as ContextMenu;
            if (ctMenu == null
                || ctMenu.PlacementTarget != sender)
            {
                OnEndEdit();

                if (EndEditFromLostFocus != null)
                {
                    EndEditFromLostFocus(this, new EndEditFromLostFocusEventArgs(e.NewFocus));
                }
            }
        }

        private void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        e.Handled = true;
                        break;

                    case Key.Escape:
                        _shouldCommit = false;
                        EditResult = PerformEditResult.NotAttempted;
                        e.Handled = true;
                        break;
                }
                if (e.Handled)
                {
                    // end editing if enter/esc
                    OnEndEdit();
                }
            }
        }

        private void focusableAncestor_KeyDown(object sender, KeyEventArgs e)
        {
            // only handle the event if edit mode is allowed
            if (IsEditModeAllowed
                && !e.Handled
                && e.Key == Key.F2)
            {
                IsInEditMode = true;
                e.Handled = true;
            }
        }
    }
}
