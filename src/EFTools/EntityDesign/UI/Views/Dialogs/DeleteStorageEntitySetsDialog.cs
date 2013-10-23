// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.VisualStudio;

    internal partial class DeleteStorageEntitySetsDialog : Form
    {
        #region TEST HOOKS

        // For test purposes only!!!
        private static event EventHandler DialogActivatedTestEventStorage;

        // This should be set only in the test code!!!
        internal static event EventHandler DialogActivatedTestEvent
        {
            add { DialogActivatedTestEventStorage += value; }
            remove { DialogActivatedTestEventStorage -= value; }
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

        internal DeleteStorageEntitySetsDialog(ICollection<StorageEntitySet> storageEntitySets)
        {
            InitializeComponent();

            // Set the default font to VS shell font.
            var vsFont = VSHelpers.GetVSFont(Services.ServiceProvider);
            if (vsFont != null)
            {
                Font = vsFont;
                DescriptionLabel.Font = vsFont;
            }

            // default result is to cancel
            DialogResult = DialogResult.Cancel;

            // display StorageEntitySets ordered by name
            Debug.Assert(null != storageEntitySets, "Constructor requires a Collection of StorageEntitySets");
            if (null != storageEntitySets)
            {
                var entitySets = new List<StorageEntitySet>(storageEntitySets);
                entitySets.Sort(EFElement.EFElementDisplayNameComparison);
                StorageEntitySetsListBox.Items.AddRange(entitySets.ToArray());
                ViewUtils.DisplayHScrollOnListBoxIfNecessary(StorageEntitySetsListBox);
            }
        }

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            // ignore request
            hevent.Handled = true;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void NoButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }

        private void YesButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }
    }
}
