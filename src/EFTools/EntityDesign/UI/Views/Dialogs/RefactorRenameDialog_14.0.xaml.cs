// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Windows;
using Microsoft.Data.Entity.Design.Common;
using Microsoft.Data.Entity.Design.Model;
using Microsoft.Data.Entity.Design.Model.Entity;
using Microsoft.Data.Entity.Design.Model.Validation;
using Microsoft.Data.Entity.Design.VisualStudio;
using Microsoft.Data.Entity.Design.VisualStudio.Package;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.Data.Entity.Design.UI.Views.Dialogs
{
    internal partial class RefactorRenameDialog : DialogWindow
    {
        EFNormalizableItem _objectToRename = null;

        internal RefactorRenameDialog(EFNormalizableItem objectToRename)
        {
            ArgumentValidation.CheckForNullReference(objectToRename, "objectToRename");

            InitializeComponent();
            _objectToRename = objectToRename;
            this.newNameTextBox.Text = objectToRename.Name.Value;
            this.Loaded += OnLoaded;
        }

        internal string NewName { get { return this.newNameTextBox.Text; } }
        internal bool? ShowPreview { get { return this.previewCheckBox.IsChecked; } }

        protected override void InvokeDialogHelp()
        {
            // do nothing
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= OnLoaded;
            this.locationTextBox.Text = Path.GetFileName(_objectToRename.Artifact.Uri.LocalPath);
            this.newNameTextBox.SelectAll();
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            string errorMessage;

            if (ValidateName(out errorMessage))
            {
                this.DialogResult = true;
            }
            else
            {
                VsUtils.ShowError(PackageManager.Package, errorMessage);
            }
        }

        private void OnNewNameTextBoxTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Note: we're doing ordinal and not ignore case because the user might want to rename to change the casing only.
            if (_objectToRename.Name.Value.Equals(this.NewName, StringComparison.Ordinal))
            {
                this.okButton.IsEnabled = false;
            }
            else
            {
                this.okButton.IsEnabled = true;
            }
        }

        private bool ValidateName(out string errorMessage)
        {
            // Unlike C# and TSql refactoring, we won't allow the user to proceed with a rename that causes a name conflict (C# and TSql
            // give a warning and allow you to proceed). This is because our rename command will fail in the case of name conflicts, so
            // we need return a hard error in the rename dialog.
            EFAttribute attr = _objectToRename.GetNameAttribute();
            errorMessage = null;

            AttributeContentValidator contentValidator = _objectToRename.Artifact.ModelManager.GetAttributeContentValidator(_objectToRename.Artifact);
            if (!contentValidator.IsValidAttributeValue(this.NewName, attr))
            {
                // not valid content
                errorMessage = Microsoft.Data.Entity.Design.Resources.RefactorRename_InvalidName;
                return false;
            }

            Property property = _objectToRename as Property;
            if (_objectToRename is EntityType)
            {
                if (!ModelHelper.IsUniqueNameForExistingItem(_objectToRename, this.NewName, true, out errorMessage))
                {
                    return false;
                }
            }
            else if (property != null)
            {
                if (!ModelHelper.IsUniqueNameForExistingItem(property, this.NewName, true, out errorMessage))
                {
                    errorMessage = string.Format(CultureInfo.CurrentCulture, Microsoft.Data.Entity.Design.Model.Resources.NAME_NOT_UNIQUE, this.NewName);
                    return false;
                }
            }
            else if (_objectToRename is Association)
            {
                if (!ModelHelper.IsUniqueNameForExistingItem(_objectToRename, this.NewName, true, out errorMessage))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
