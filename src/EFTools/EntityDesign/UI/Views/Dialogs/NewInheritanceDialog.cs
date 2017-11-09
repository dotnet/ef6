// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.VisualStudio;

    internal partial class NewInheritanceDialog : Form
    {
        private readonly ISet<ConceptualEntityType> _entityTypes;

        #region TESTS

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

        internal NewInheritanceDialog(ConceptualEntityType baseType, IEnumerable<ConceptualEntityType> entityTypes)
        {
            InitializeComponent();
            // Set the default font to VS shell font.
            var vsFont = VSHelpers.GetVSFont(Services.ServiceProvider);
            if (vsFont != null)
            {
                Font = vsFont;
            }

            _entityTypes = new SortedSet<ConceptualEntityType>(new EFNameableItemComparer());

            foreach (var et in entityTypes)
            {
                _entityTypes.Add(et);
            }

            baseEntityComboBox.Items.AddRange(_entityTypes.ToArray());
            if (baseType != null)
            {
                baseEntityComboBox.SelectedItem = baseType;
            }
            CheckOkButtonEnabled();

            cancelButton.BackColor = SystemColors.Control;
            cancelButton.ForeColor = SystemColors.ControlText;
            okButton.BackColor = SystemColors.Control;
            okButton.ForeColor = SystemColors.ControlText;
        }

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            // ignore request
            hevent.Handled = true;
        }

        internal ConceptualEntityType BaseEntityType
        {
            get { return baseEntityComboBox.SelectedItem as ConceptualEntityType; }
        }

        internal EntityType DerivedEntityType
        {
            get { return derivedEntityComboBox.SelectedItem as EntityType; }
        }

        private void baseEntityComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            derivedEntityComboBox.Items.Clear();
            foreach (var entityType in _entityTypes)
            {
                if (entityType != BaseEntityType
                    && entityType.BaseType.Target == null)
                {
                    if (BaseEntityType == null
                        || (BaseEntityType != null && !BaseEntityType.IsDerivedFrom(entityType)))
                    {
                        derivedEntityComboBox.Items.Add(entityType);
                    }
                }
            }
            if (derivedEntityComboBox.Items.Count > 0)
            {
                derivedEntityComboBox.SelectedIndex = 0;
            }

            CheckOkButtonEnabled();
        }

        private void CheckOkButtonEnabled()
        {
            if (BaseEntityType == null
                || DerivedEntityType == null)
            {
                okButton.Enabled = false;
            }
            else
            {
                okButton.Enabled = true;
            }
        }
    }
}
