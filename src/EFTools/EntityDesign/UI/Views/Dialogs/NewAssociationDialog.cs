// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Infrastructure.Pluralization;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal partial class NewAssociationDialog : Form
    {
        private bool _needsValidation;
        private readonly bool _foreignKeysSupported;
        private readonly IPluralizationService _pluralizationService;

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

        internal NewAssociationDialog(IEnumerable<EntityType> entityTypes, EntityType entity1, EntityType entity2)
        {
            Debug.Assert(entity1 != null && entity2 != null, "both entity1 and entity2 should be non-null");

            // Ensure _foreignKeysSupported is initialized before we initialize UI components.
            _foreignKeysSupported =
                EdmFeatureManager.GetForeignKeysInModelFeatureState(entity1.Artifact.SchemaVersion)
                    .IsEnabled();

            // pluralization service is based on English only for Dev10
            var pluralize = ModelHelper.GetDesignerPropertyValueFromArtifactAsBool(
                OptionsDesignerInfo.ElementName,
                OptionsDesignerInfo.AttributeEnablePluralization, OptionsDesignerInfo.EnablePluralizationDefault, entity1.Artifact);
            if (pluralize)
            {
                _pluralizationService = DependencyResolver.GetService<IPluralizationService>();
            }

            InitializeComponent();

            // Set the default font to VS shell font.
            var vsFont = VSHelpers.GetVSFont(Services.ServiceProvider);
            if (vsFont != null)
            {
                Font = vsFont;
            }

            foreach (var entityType in entityTypes)
            {
                entity1ComboBox.Items.Add(entityType);
                entity2ComboBox.Items.Add(entityType);
            }

            multiplicity1ComboBox.Items.Add(
                new MultiplicityComboBoxItem(Resources.PropertyWindow_Value_MultiplicityOne, ModelConstants.Multiplicity_One));
            multiplicity1ComboBox.Items.Add(
                new MultiplicityComboBoxItem(Resources.PropertyWindow_Value_MultiplicityZeroOrOne, ModelConstants.Multiplicity_ZeroOrOne));
            multiplicity1ComboBox.Items.Add(
                new MultiplicityComboBoxItem(Resources.PropertyWindow_Value_MultiplicityMany, ModelConstants.Multiplicity_Many));

            multiplicity2ComboBox.Items.Add(
                new MultiplicityComboBoxItem(Resources.PropertyWindow_Value_MultiplicityOne, ModelConstants.Multiplicity_One));
            multiplicity2ComboBox.Items.Add(
                new MultiplicityComboBoxItem(Resources.PropertyWindow_Value_MultiplicityZeroOrOne, ModelConstants.Multiplicity_ZeroOrOne));
            multiplicity2ComboBox.Items.Add(
                new MultiplicityComboBoxItem(Resources.PropertyWindow_Value_MultiplicityMany, ModelConstants.Multiplicity_Many));

            // set multiplicity and entity values first before adding event handlers, otherwise
            // it tries to update the ExplanationText and calculate NavProp names before all
            // of the required information is in place
            multiplicity1ComboBox.SelectedIndex = 0;
            multiplicity2ComboBox.SelectedIndex = 2;
            entity1ComboBox.SelectedItem = entity1;
            entity2ComboBox.SelectedItem = entity2;
            multiplicity1ComboBox.SelectedIndexChanged += multiplicity1ComboBox_SelectedIndexChanged;
            multiplicity2ComboBox.SelectedIndexChanged += multiplicity2ComboBox_SelectedIndexChanged;
            entity1ComboBox.SelectedIndexChanged += entity1ComboBox_SelectedIndexChanged;
            entity2ComboBox.SelectedIndexChanged += entity2ComboBox_SelectedIndexChanged;

            // now update the calculated fields
            UpdateAssociationName();
            UpdateEnd1NavigationPropertyName();
            UpdateEnd2NavigationPropertyName();
            UpdateExplanationText();
            UpdateCreateForeignKeysCheckBox();

            cancelButton.BackColor = SystemColors.Control;
            cancelButton.ForeColor = SystemColors.ControlText;
            okButton.BackColor = SystemColors.Control;
            okButton.ForeColor = SystemColors.ControlText;
        }

        internal string AssociationName
        {
            get { return associationNameTextBox.Text; }
        }

        internal ConceptualEntityType End1Entity
        {
            get
            {
                var cet = entity1ComboBox.SelectedItem as ConceptualEntityType;
                Debug.Assert(entity2ComboBox.SelectedItem is EntityType ? cet != null : true, "EntityType is not ConceptualEntityType");
                return cet;
            }
        }

        internal ConceptualEntityType End2Entity
        {
            get
            {
                var cet = entity2ComboBox.SelectedItem as ConceptualEntityType;
                Debug.Assert(
                    entity2ComboBox.SelectedItem is EntityType ? cet != null : true,
                    typeof(NewAssociationDialog).Name + ".End2Entity: EntityType is not ConceptualEntityType");
                return cet;
            }
        }

        internal string End1Multiplicity
        {
            get
            {
                var item = multiplicity1ComboBox.SelectedItem as MultiplicityComboBoxItem;
                if (item != null)
                {
                    return item.Value;
                }

                return null;
            }
        }

        internal string End2Multiplicity
        {
            get
            {
                var item = multiplicity2ComboBox.SelectedItem as MultiplicityComboBoxItem;
                if (item != null)
                {
                    return item.Value;
                }

                return null;
            }
        }

        internal string End1MultiplicityText
        {
            get
            {
                var item = multiplicity1ComboBox.SelectedItem as MultiplicityComboBoxItem;
                if (item != null)
                {
                    return item.ToString();
                }

                return String.Empty;
            }
        }

        internal string End2MultiplicityText
        {
            get
            {
                var item = multiplicity2ComboBox.SelectedItem as MultiplicityComboBoxItem;
                if (item != null)
                {
                    return item.ToString();
                }

                return String.Empty;
            }
        }

        internal string End1NavigationPropertyName
        {
            get { return navigationPropertyCheckbox.Checked ? navigationProperty1TextBox.Text : string.Empty; }
        }

        internal string End2NavigationPropertyName
        {
            get { return navigationProperty2Checkbox.Checked ? navigationProperty2TextBox.Text : string.Empty; }
        }

        internal bool CreateForeignKeyProperties
        {
            get { return (_foreignKeysSupported && createForeignKeysCheckBox.Enabled ? createForeignKeysCheckBox.Checked : false); }
        }

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            // ignore request
            hevent.Handled = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_needsValidation)
            {
                _needsValidation = false;
                string msg = null;

                if (!EscherAttributeContentValidator.IsValidCsdlAssociationName(AssociationName))
                {
                    VsUtils.ShowErrorDialog(DialogsResource.NewAssociationDialog_InvalidAssociationNameMsg);
                    e.Cancel = true;
                    associationNameTextBox.Focus();
                }
                else if (navigationPropertyCheckbox.Checked
                         && !EscherAttributeContentValidator.IsValidCsdlNavigationPropertyName(End1NavigationPropertyName))
                {
                    VsUtils.ShowErrorDialog(DialogsResource.NewAssociationDialog_InvalidNavigationPropertyNameMsg);
                    e.Cancel = true;
                    navigationProperty1TextBox.Focus();
                }
                else if (navigationProperty2Checkbox.Checked
                         && !EscherAttributeContentValidator.IsValidCsdlNavigationPropertyName(End2NavigationPropertyName))
                {
                    VsUtils.ShowErrorDialog(DialogsResource.NewAssociationDialog_InvalidNavigationPropertyNameMsg);
                    e.Cancel = true;
                    navigationProperty2TextBox.Focus();
                }
                else if (!ModelHelper.IsUniqueName(typeof(Association), End1Entity.Parent, AssociationName, false, out msg))
                {
                    VsUtils.ShowErrorDialog(DialogsResource.NewAssociationDialog_EnsureUniqueNameMsg);
                    e.Cancel = true;
                    associationNameTextBox.Focus();
                }
                else if (navigationPropertyCheckbox.Checked
                         && End1NavigationPropertyName.Equals(End1Entity.LocalName.Value, StringComparison.Ordinal))
                {
                    VsUtils.ShowErrorDialog(DialogsResource.SameEntityAndPropertyNameMsg);
                    e.Cancel = true;
                    navigationProperty1TextBox.Focus();
                }
                else if (navigationPropertyCheckbox.Checked
                         && !ModelHelper.IsUniquePropertyName(End1Entity, End1NavigationPropertyName, true))
                {
                    VsUtils.ShowErrorDialog(DialogsResource.NewAssociationDialog_EnsureUniquePropertyNameMsg);
                    e.Cancel = true;
                    navigationProperty1TextBox.Focus();
                }
                else if (End2NavigationPropertyName.Equals(End2Entity.LocalName.Value, StringComparison.Ordinal))
                {
                    VsUtils.ShowErrorDialog(DialogsResource.SameEntityAndPropertyNameMsg);
                    e.Cancel = true;
                    navigationProperty2TextBox.Focus();
                }
                else if (navigationProperty2Checkbox.Checked
                         && !ModelHelper.IsUniquePropertyName(End2Entity, End2NavigationPropertyName, true)
                         || (End1Entity == End2Entity && navigationProperty2Checkbox.Checked
                             && End2NavigationPropertyName.Equals(End1NavigationPropertyName, StringComparison.Ordinal)))
                {
                    VsUtils.ShowErrorDialog(DialogsResource.NewAssociationDialog_EnsureUniquePropertyNameMsg);
                    e.Cancel = true;
                    navigationProperty2TextBox.Focus();
                }
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            _needsValidation = true;
        }

        private void entity1ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateAssociationName();
            UpdateEnd1NavigationPropertyName();
            UpdateEnd2NavigationPropertyName();
        }

        private void entity2ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateAssociationName();
            UpdateEnd1NavigationPropertyName();
            UpdateEnd2NavigationPropertyName();
        }

        private void associationNameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!EscherAttributeContentValidator.IsValidCsdlAssociationName(AssociationName))
            {
                end1GroupBox.Enabled = false;
                end2GroupBox.Enabled = false;
            }
            else
            {
                end1GroupBox.Enabled = true;
                end2GroupBox.Enabled = true;
            }
        }

        private void navigationProperty1TextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateExplanationText();
        }

        private void navigationProperty2TextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateExplanationText();
        }

        private void multiplicity1ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateEnd2NavigationPropertyName();
            UpdateExplanationText();
        }

        private void multiplicity2ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateEnd1NavigationPropertyName();
            UpdateExplanationText();
        }

        private void navigationPropertyCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            navigationProperty1TextBox.Enabled = navigationPropertyCheckbox.Checked;
        }

        private void navigationProperty2Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            navigationProperty2TextBox.Enabled = navigationProperty2Checkbox.Checked;
        }

        private void UpdateAssociationName()
        {
            EntityType principal;
            EntityType dependent;
            TryGetPrincipalAndDependentEntities(out principal, out dependent);

            var principalName = (principal != null) ? principal.LocalName.Value : End1Entity.LocalName.Value;
            var dependentName = (dependent != null) ? dependent.LocalName.Value : End2Entity.LocalName.Value;

            associationNameTextBox.Text = ModelHelper.GetUniqueName(typeof(Association), End1Entity.Parent, principalName + dependentName);
        }

        private void UpdateEnd1NavigationPropertyName()
        {
            if (End1Entity != null
                && End2Entity != null)
            {
                var namesToAvoid = new HashSet<string>();
                if (End1Entity.Equals(End2Entity))
                {
                    namesToAvoid.Add(navigationProperty2TextBox.Text);
                }

                var proposedNavPropName = ModelHelper.ConstructProposedNavigationPropertyName(
                    _pluralizationService, End2Entity.LocalName.Value, End2Multiplicity);
                navigationProperty1TextBox.Text = ModelHelper.GetUniqueConceptualPropertyName(proposedNavPropName, End1Entity, namesToAvoid);
            }
        }

        private void UpdateEnd2NavigationPropertyName()
        {
            if (End1Entity != null
                && End2Entity != null)
            {
                var namesToAvoid = new HashSet<string>();
                if (End1Entity.Equals(End2Entity))
                {
                    namesToAvoid.Add(navigationProperty1TextBox.Text);
                }

                var proposedNavPropName = ModelHelper.ConstructProposedNavigationPropertyName(
                    _pluralizationService, End1Entity.LocalName.Value, End1Multiplicity);
                navigationProperty2TextBox.Text = ModelHelper.GetUniqueConceptualPropertyName(proposedNavPropName, End2Entity, namesToAvoid);
            }
        }

        private static bool AreForeignKeysSupportedByCardinality(string end1, string end2)
        {
            // Table for when FKs are enabled/disabled based on multiplicity
            //
            //     End1    |   End2     |   Result
            //     --------------------------------
            //       1     |      1     |   FK disabled
            //       1     |      0..1  |   FK disabled
            //       1     |      *     |   FK enabled
            //       0..1  |      1     |   FK disabled
            //       0..1  |      0..1  |   FK disabled
            //       0..1  |      *     |   FK enabled
            //       *     |      1     |   FK enabled
            //       *     |      0..1  |   FK enabled
            //       *     |      *     |   FK disabled
            //

            var supported = false;

#if DEBUG
            // verify that values are what we expect
            string[] values =
                {
                    Resources.PropertyWindow_Value_MultiplicityMany, Resources.PropertyWindow_Value_MultiplicityOne,
                    Resources.PropertyWindow_Value_MultiplicityZeroOrOne
                };
            var hashValues = new HashSet<string>(values);
            Debug.Assert(hashValues.Contains(end1), "Unexpected string value for end1:  " + end1);
            Debug.Assert(hashValues.Contains(end2), "Unexpected string value for end2:  " + end2);
#endif

            // FKs are enabled when one end is "many" and the other end is not many.  We check for that here
            if (string.Compare(end1, Resources.PropertyWindow_Value_MultiplicityMany, StringComparison.CurrentCulture) == 0)
            {
                if (string.Compare(end2, Resources.PropertyWindow_Value_MultiplicityMany, StringComparison.CurrentCulture) != 0)
                {
                    // *:1 or *:0..1 - FKs are supported
                    supported = true;
                }
            }
            else if (string.Compare(end2, Resources.PropertyWindow_Value_MultiplicityMany, StringComparison.CurrentCulture) == 0)
            {
                if (string.Compare(end1, Resources.PropertyWindow_Value_MultiplicityMany, StringComparison.CurrentCulture) != 0)
                {
                    // 1:* or 0..1:* - FKs are supported
                    supported = true;
                }
            }

            return supported;
        }

        // <summary>
        //     Attempts to determing the principal and dependent ends. Can return null for principal/dependent values.
        // </summary>
        private void TryGetPrincipalAndDependentEntities(out EntityType principal, out EntityType dependent)
        {
            // NOTE: this needs to match the logic in ModelHelper.DeterminePrincipalDependentAssociationEnds()
            principal = null;
            dependent = null;

            if (string.Equals(End1MultiplicityText, Resources.PropertyWindow_Value_MultiplicityOne, StringComparison.CurrentCulture)
                && string.Equals(End2MultiplicityText, Resources.PropertyWindow_Value_MultiplicityOne, StringComparison.CurrentCulture))
            {
                // End1 Multiplicity | End2 Multiplicity | Principal Role
                // 1                 | 1                 | just pick end1
                dependent = End2Entity;
                principal = End1Entity;
            }
            else if (string.Equals(
                End1MultiplicityText, Resources.PropertyWindow_Value_MultiplicityZeroOrOne, StringComparison.CurrentCulture)
                     && string.Equals(
                         End2MultiplicityText, Resources.PropertyWindow_Value_MultiplicityZeroOrOne, StringComparison.CurrentCulture))
            {
                // End1 Multiplicity | End2 Multiplicity | Principal Role
                // 0..1              | 0..1              | just pick end1
                dependent = End2Entity;
                principal = End1Entity;
            }
            else if (string.Equals(
                End1MultiplicityText, Resources.PropertyWindow_Value_MultiplicityZeroOrOne, StringComparison.CurrentCulture)
                     && string.Equals(
                         End2MultiplicityText, Resources.PropertyWindow_Value_MultiplicityMany, StringComparison.CurrentCulture))
            {
                // End1 Multiplicity | End2 Multiplicity | Principal Role
                // 0..1              | *                 | 0..1
                dependent = End2Entity;
                principal = End1Entity;
            }
            else if (string.Equals(
                End2MultiplicityText, Resources.PropertyWindow_Value_MultiplicityZeroOrOne, StringComparison.CurrentCulture)
                     && string.Equals(
                         End1MultiplicityText, Resources.PropertyWindow_Value_MultiplicityMany, StringComparison.CurrentCulture))
            {
                // End1 Multiplicity | End2 Multiplicity | Principal Role
                // *                 | 0..1              | 0..1
                dependent = End1Entity;
                principal = End2Entity;
            }
            else
            {
                if (string.Equals(
                    End1MultiplicityText, Resources.PropertyWindow_Value_MultiplicityOne, StringComparison.CurrentCulture)
                    && (string.Equals(
                        End2MultiplicityText, Resources.PropertyWindow_Value_MultiplicityZeroOrOne, StringComparison.CurrentCulture)
                        || string.Equals(
                            End2MultiplicityText, Resources.PropertyWindow_Value_MultiplicityMany, StringComparison.CurrentCulture)))
                {
                    // End1 Multiplicity | End2 Multiplicity | Principal Role
                    // 1                 | 0..1              | 1
                    // 1                 | *                 | 1
                    dependent = End2Entity;
                    principal = End1Entity;
                }
                else if (string.Equals(
                    End2MultiplicityText, Resources.PropertyWindow_Value_MultiplicityOne, StringComparison.CurrentCulture)
                         && (string.Equals(
                             End1MultiplicityText, Resources.PropertyWindow_Value_MultiplicityZeroOrOne, StringComparison.CurrentCulture)
                             ||
                             string.Equals(
                                 End1MultiplicityText, Resources.PropertyWindow_Value_MultiplicityMany, StringComparison.CurrentCulture)))
                {
                    // End1 Multiplicity | End2 Multiplicity | Principal Role
                    // 0..1              | 1                 | 1
                    // *                 | 1                 | 1
                    dependent = End1Entity;
                    principal = End2Entity;
                }
            }
        }

        private void UpdateCreateForeignKeysCheckBox()
        {
            var enableFKCheckbox = _foreignKeysSupported && AreForeignKeysSupportedByCardinality(End1MultiplicityText, End2MultiplicityText);

            if (enableFKCheckbox == false)
            {
                createForeignKeysCheckBox.Checked = false;
                createForeignKeysCheckBox.Enabled = false;
            }
            else
            {
                createForeignKeysCheckBox.Enabled = true;
                EntityType principal;
                EntityType dependent;
                TryGetPrincipalAndDependentEntities(out principal, out dependent);

                if (dependent != null)
                {
                    createForeignKeysCheckBox.Text = string.Format(
                        CultureInfo.CurrentCulture, DialogsResource.NewAssociationDialog_CreateForeignKeysLabel, dependent.LocalName.Value);
                }
                else
                {
                    createForeignKeysCheckBox.Text = DialogsResource.NewAssociationDialog_CreateForeignKeysLabel_Default;
                }
            }
        }

        private void UpdateExplanationText()
        {
            UpdateCreateForeignKeysCheckBox();

            string sentence1;
            string sentence2;
            var sentenceBase1 = (!string.IsNullOrEmpty(End1NavigationPropertyName))
                                    ? DialogsResource.NewAssociationDialog_ExplanationText1
                                    : DialogsResource.NewAssociationDialog_ExplanationText1EmptyNavProp;
            var sentenceBase2 = (!string.IsNullOrEmpty(End2NavigationPropertyName))
                                    ? DialogsResource.NewAssociationDialog_ExplanationText2
                                    : DialogsResource.NewAssociationDialog_ExplanationText2EmptyNavProp;

            sentenceBase1 = (!string.IsNullOrEmpty(End1NavigationPropertyName))
                                ? DialogsResource.NewAssociationDialog_ExplanationText1
                                : DialogsResource.NewAssociationDialog_ExplanationText1EmptyNavProp;
            sentenceBase2 = (!string.IsNullOrEmpty(End1NavigationPropertyName))
                                ? DialogsResource.NewAssociationDialog_ExplanationText2
                                : DialogsResource.NewAssociationDialog_ExplanationText2EmptyNavProp;

            sentence1 = String.Format(
                CultureInfo.CurrentCulture,
                (string.Equals(End2MultiplicityText, Resources.PropertyWindow_Value_MultiplicityOne, StringComparison.CurrentCulture))
                    ? sentenceBase1
                    : sentenceBase2,
                End1Entity.LocalName.Value,
                End2MultiplicityText,
                End2Entity.LocalName.Value,
                End1NavigationPropertyName);

            sentenceBase1 = (!string.IsNullOrEmpty(End2NavigationPropertyName))
                                ? DialogsResource.NewAssociationDialog_ExplanationText1
                                : DialogsResource.NewAssociationDialog_ExplanationText1EmptyNavProp;
            sentenceBase2 = (!string.IsNullOrEmpty(End2NavigationPropertyName))
                                ? DialogsResource.NewAssociationDialog_ExplanationText2
                                : DialogsResource.NewAssociationDialog_ExplanationText2EmptyNavProp;

            sentence2 = String.Format(
                CultureInfo.CurrentCulture,
                (string.Equals(End1MultiplicityText, Resources.PropertyWindow_Value_MultiplicityOne, StringComparison.CurrentCulture))
                    ? sentenceBase1
                    : sentenceBase2,
                End2Entity.LocalName.Value,
                End1MultiplicityText,
                End1Entity.LocalName.Value,
                End2NavigationPropertyName);

            explanationTextBox.Text = sentence1 + "\r\n\r\n" + sentence2;
        }

        private class MultiplicityComboBoxItem
        {
            private readonly string _text;
            private readonly string _value;

            public MultiplicityComboBoxItem(string text, string value)
            {
                Debug.Assert(!String.IsNullOrEmpty(text) && !String.IsNullOrEmpty(value), "neither text nor value should be null or empty");
                _text = text;
                _value = value;
            }

            public override string ToString()
            {
                return _text;
            }

            public string Value
            {
                get { return _value; }
            }
        }
    }
}
