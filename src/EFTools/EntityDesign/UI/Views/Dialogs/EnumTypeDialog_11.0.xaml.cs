// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Controls;
using Microsoft.Data.Entity.Design.UI.ViewModels;
using Microsoft.VisualStudio.PlatformUI;
using EntityDesignerResources = Microsoft.Data.Entity.Design.Resources;

namespace Microsoft.Data.Entity.Design.UI.Views.Dialogs
{
    // <summary>
    // Interaction logic for EnumTypeDialog.xaml
    // </summary>
    internal partial class EnumTypeDialog : DialogWindow
    {
        private bool _isManualEditCommit = false;
        private bool _isOkButtonClicked = false;
        private EnumTypeViewModel _enumTypeViewModel = null;

        public EnumTypeDialog(EnumTypeViewModel vm)
        {
            _enumTypeViewModel = vm;
            this.DataContext = _enumTypeViewModel;
            InitializeComponent();
            if (vm.IsNew)
            {
                this.Title = EntityDesignerResources.EnumDialog_NewEnumWindowTitle;
            }
            else
            {
                this.Title = EntityDesignerResources.EnumDialog_EditEnumWindowTitle;
            }
            this.dgEnumTypeMembers.CellEditEnding += dgEnumTypeMembers_CellEditEnding;
            this.HasHelpButton = false;
        }

        public EnumTypeViewModel EnumTypeViewModel
        {
            get
            {
                return _enumTypeViewModel;
            }
        }

        #region Event Handler

        protected override void OnClosing(CancelEventArgs e)
        {
            // When ok button is clicked:
            // - First we need to ensure all the changes in data-grid are committed.
            // - If the view model is not in valid state,  block the dialog from closing.
            if (_isOkButtonClicked)
            {
                CommitDialogControlsManually();

                EnumTypeViewModel vm = this.DataContext as EnumTypeViewModel;
                Debug.Assert(vm != null, "Dialog DataContext is not type of EnumTypeViewModel");
                if (vm != null && vm.IsValid == false)
                {
                    e.Cancel = true;
                    _isOkButtonClicked = false;
                    return;
                }
            }
            base.OnClosing(e);
        }

        private void dgEnumTypeMembers_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // This is to ensure that we commit every time the cell is edited.
            CommitDialogControlsManually();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            _isOkButtonClicked = true;
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _isOkButtonClicked = false;
            this.DialogResult = false;
        }

        private void OnTextBoxTextChanged(Object sender, TextChangedEventArgs e)
        {
            // Force binding between source and target element; this is so that validation can take place immediately.
            ForceTextBoxUpdateSource(sender);
        }

        private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            // Force binding between source and target element; this is so that validation can take place immediately.
            ForceTextBoxUpdateSource(sender);
        }

        private void OnRefrenceExternalTypeClick(Object sender, RoutedEventArgs e)
        {
            if (chkReferenceExternalType.IsChecked == true)
            {
                Keyboard.Focus(txtExternalType);
            }
            ForceTextBoxUpdateSource(txtExternalType);
        }

        #endregion

        private static void ForceTextBoxUpdateSource(object control)
        {
            TextBox textBox = control as TextBox;
            Debug.Assert(textBox != null, "parameter control is not a TextBox type");
            if (textBox != null)
            {
                textBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
        }

        private void CommitDialogControlsManually()
        {
            if (!_isManualEditCommit)
            {
                try
                {
                    _isManualEditCommit = true;
                    txtEnumTypeName.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                    txtExternalType.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                    dgEnumTypeMembers.CommitEdit(DataGridEditingUnit.Row, true);
                }
                finally
                {
                    _isManualEditCommit = false;
                }
            }
        }

        private void DgEnumTypeMembers_OnLoaded(object sender, RoutedEventArgs e)
        {
            foreach (var row in dgEnumTypeMembers.FindDescendants<DataGridRow>().Where(r => r.IsNewItem).ToList())
            {
                foreach (var cell in row.FindDescendants<DataGridCell>().ToList())
                {
                    if (cell.Column != null)
                    {
                        if (cell.Column.DisplayIndex == 0)
                        {
                            cell.SetValue(AutomationProperties.NameProperty, EntityDesignerResources.EnumDialog_EnumTypeMemberNameLabel);
                        }
                        else if (cell.Column.DisplayIndex == 1)
                        {
                            cell.SetValue(AutomationProperties.NameProperty, EntityDesignerResources.EnumDialog_EnumTypeMemberValueLabel);
                        }
                    }
                }
            }
        }

        #region Test support

        private static event EventHandler DialogActivatedTestEventStorage;

        // This should be set only in the test code!!!
        internal static event EventHandler DialogActivatedTestEvent
        {
            add
            {
                DialogActivatedTestEventStorage += value;
            }
            remove
            {
                DialogActivatedTestEventStorage -= value;
            }
        }

        // For test purposes only!!!
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (DialogActivatedTestEventStorage != null)
            {
                DialogActivatedTestEventStorage(this, EventArgs.Empty);
            }
        }
        #endregion
    }
}