// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Dialogs
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Resources = Microsoft.Data.Tools.XmlDesignerBase.Resources;

    internal partial class NewEntityDialog : Form
    {
        private readonly ConceptualEntityModel _model;
        private bool _needsValidation;

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

        internal NewEntityDialog(ConceptualEntityModel model)
        {
            Debug.Assert(model != null, "model should not be null");
            _model = model;

            InitializeComponent();
            // Set the default font to VS shell font.
            var vsFont = VSHelpers.GetVSFont(Services.ServiceProvider);
            if (vsFont != null)
            {
                Font = vsFont;
            }
            keyPropertyCheckBox.Checked = true;

            propertyTypeComboBox.Items.AddRange(ModelHelper.AllPrimitiveTypesSorted(_model.Artifact.SchemaVersion));
            propertyTypeComboBox.SelectedItem = ModelConstants.Int32PropertyType;

            baseTypeComboBox.Items.Add(Resources.NoneDisplayValueUsedForUX);
            baseTypeComboBox.Items.AddRange(model.EntityTypes().ToArray());
            if (baseTypeComboBox.Items.Count > 0)
            {
                baseTypeComboBox.SelectedIndex = 0;
            }

            entityNameTextBox.Text = ModelHelper.GetUniqueNameWithNumber(
                typeof(EntityType), model, Model.Resources.Model_DefaultEntityTypeName);
            propertyNameTextBox.Text = Model.Resources.Model_IdPropertyName;

            cancelButton.BackColor = SystemColors.Control;
            cancelButton.ForeColor = SystemColors.ControlText;
            okButton.BackColor = SystemColors.Control;
            okButton.ForeColor = SystemColors.ControlText;
        }

        internal string EntityName
        {
            get { return entityNameTextBox.Text; }
        }

        internal string EntitySetName
        {
            get { return entitySetTextBox.Text; }
        }

        internal bool CreateKeyProperty
        {
            get { return keyPropertyCheckBox.Enabled && keyPropertyCheckBox.Checked; }
        }

        internal string KeyPropertyName
        {
            get { return propertyNameTextBox.Text; }
        }

        internal string KeyPropertyType
        {
            get { return propertyTypeComboBox.SelectedItem as string; }
        }

        internal ConceptualEntityType BaseEntityType
        {
            get
            {
                var cet = baseTypeComboBox.SelectedItem as ConceptualEntityType;
#if DEBUG
                var et = baseTypeComboBox.SelectedItem as EntityType;
                Debug.Assert(et != null ? cet != null : true, "EntityType is not ConceptualEntityType");
#endif
                return cet;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_needsValidation)
            {
                _needsValidation = false;

                if (!EscherAttributeContentValidator.IsValidCsdlEntityTypeName(entityNameTextBox.Text))
                {
                    VsUtils.ShowErrorDialog(DialogsResource.NewEntityDialog_InvalidEntityNameMsg);
                    e.Cancel = true;
                    entityNameTextBox.Focus();
                }
                else
                {
                    string msg;
                    if (!ModelHelper.IsUniqueName(typeof(EntityType), _model, entityNameTextBox.Text, false, out msg))
                    {
                        VsUtils.ShowErrorDialog(DialogsResource.NewEntityDialog_EnsureUniqueNameMsg);
                        e.Cancel = true;
                        entityNameTextBox.Focus();
                        return;
                    }

                    if (entitySetTextBox.Enabled)
                    {
                        if (!EscherAttributeContentValidator.IsValidCsdlEntitySetName(EntitySetName))
                        {
                            VsUtils.ShowErrorDialog(DialogsResource.NewEntityDialog_InvalidEntitySetMsg);
                            e.Cancel = true;
                            entitySetTextBox.Focus();
                            return;
                        }

                        if (!ModelHelper.IsUniqueName(typeof(EntitySet), _model.FirstEntityContainer, EntitySetName, false, out msg))
                        {
                            VsUtils.ShowErrorDialog(DialogsResource.NewEntityDialog_EnsureUniqueSetNameMsg);
                            e.Cancel = true;
                            entitySetTextBox.Focus();
                            return;
                        }
                    }

                    if (propertyNameTextBox.Enabled)
                    {
                        if (!EscherAttributeContentValidator.IsValidCsdlPropertyName(propertyNameTextBox.Text))
                        {
                            VsUtils.ShowErrorDialog(DialogsResource.NewEntityDialog_InvalidKeyPropertyNameMsg);
                            e.Cancel = true;
                            propertyNameTextBox.Focus();
                            return;
                        }
                        else if (propertyNameTextBox.Text.Equals(EntityName, StringComparison.Ordinal))
                        {
                            VsUtils.ShowErrorDialog(DialogsResource.SameEntityAndPropertyNameMsg);
                            e.Cancel = true;
                            propertyNameTextBox.Focus();
                            return;
                        }
                    }
                }
            }
        }

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            // ignore request
            hevent.Handled = true;
        }

        private void baseTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSettingsFromGui();
        }

        private void entityNameTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateSettingsFromGui();
        }

        private void UpdateSettingsFromGui()
        {
            if (!EscherAttributeContentValidator.IsValidCsdlEntityTypeName(entityNameTextBox.Text))
            {
                baseTypeComboBox.Enabled = false;
                entitySetTextBox.Enabled = false;
                groupBox2.Enabled = false;
            }
            else
            {
                groupBox2.Enabled = true;
                baseTypeComboBox.Enabled = true;

                if (BaseEntityType == null)
                {
                    entitySetTextBox.Enabled = true;
                    groupBox2.Enabled = true;
                    var proposedEntitySetName = ModelHelper.ConstructProposedEntitySetName(_model.Artifact, EntityName);
                    entitySetTextBox.Text = ModelHelper.GetUniqueName(typeof(EntitySet), _model.FirstEntityContainer, proposedEntitySetName);
                    keyPropertyCheckBox.Checked = true;
                }
                else
                {
                    keyPropertyCheckBox.Checked = false;
                    entitySetTextBox.Enabled = false;
                    entitySetTextBox.Text = BaseEntityType.EntitySet.LocalName.Value;
                    groupBox2.Enabled = false;
                }
            }
        }

        private void keyPropertyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (keyPropertyCheckBox.Checked)
            {
                propertyNameTextBox.Enabled = true;
                propertyNameLabel.Enabled = true;
                propertyTypeComboBox.Enabled = true;
                propertyTypeLabel.Enabled = true;
            }
            else
            {
                propertyNameTextBox.Enabled = false;
                propertyNameLabel.Enabled = false;
                propertyTypeComboBox.Enabled = false;
                propertyTypeLabel.Enabled = false;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            _needsValidation = true;
        }
    }
}
