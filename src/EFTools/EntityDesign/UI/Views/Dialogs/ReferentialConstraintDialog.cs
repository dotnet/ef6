// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.Shell.Interop;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal partial class ReferentialConstraintDialog : Form
    {
        private bool _needsValidation;
        private bool _shouldDeleteOnly;
        private readonly Association _association;
        private readonly AssociationEnd _end1;
        private readonly AssociationEnd _end2;

        private AssociationEnd _principal;
        private AssociationEnd _dependent;

        private readonly Dictionary<AssociationEnd, RoleListItem> _roleListItems = new Dictionary<AssociationEnd, RoleListItem>();
        private readonly RoleListItem _blankRoleListItem = new RoleListItem(null, false);

        private readonly Dictionary<Symbol, MappingListItem> _mappingListItems = new Dictionary<Symbol, MappingListItem>();

        private readonly Dictionary<Symbol, KeyListItem> _dependentListItems = new Dictionary<Symbol, KeyListItem>();
        private readonly KeyListItem _blankDependentKeyListItem = new KeyListItem(null);

        private bool _handlingSelection;

        internal static IEnumerable<Command> LaunchReferentialConstraintDialog(Association association)
        {
            var commands = new List<Command>();
            if (association != null)
            {
                if (association.ReferentialConstraint == null
                    ||
                    (association.ReferentialConstraint != null &&
                     association.ReferentialConstraint.Principal != null &&
                     association.ReferentialConstraint.Principal.Role.Target != null &&
                     association.ReferentialConstraint.Principal.Role.Target.Type.Target != null &&
                     association.ReferentialConstraint.Dependent != null &&
                     association.ReferentialConstraint.Dependent.Role.Target != null &&
                     association.ReferentialConstraint.Dependent.Role.Target.Type.Target != null
                    )
                    )
                {
                    using (var dlg = new ReferentialConstraintDialog(association))
                    {
                        var result = dlg.ShowDialog();
                        if (result != DialogResult.Cancel
                            && dlg.Principal != null
                            && dlg.Dependent != null)
                        {
                            if (association.ReferentialConstraint != null)
                            {
                                // first, enqueue the delete command (always)
                                commands.Add(association.ReferentialConstraint.GetDeleteCommand());
                            }

                            if (dlg.ShouldDeleteOnly == false)
                            {
                                var principalProps = new List<Property>();
                                var dependentProps = new List<Property>();

                                var keys = GetKeysForType(dlg.Principal.Type.Target);

                                foreach (var mli in dlg.MappingList)
                                {
                                    if (mli.IsValidPrincipalKey)
                                    {
                                        // try to resolve the symbol into a property
                                        Property p = null;
                                        Property d = null;
                                        if (mli.PrincipalKey != null)
                                        {
                                            p = GetKeyForType(mli.PrincipalKey, dlg.Principal.Type.Target, keys);
                                        }

                                        if (mli.DependentProperty != null)
                                        {
                                            d = dlg.Dependent.Artifact.ArtifactSet.LookupSymbol(mli.DependentProperty) as Property;
                                        }

                                        if (p != null
                                            && d != null)
                                        {
                                            principalProps.Add(p);
                                            dependentProps.Add(d);
                                        }
                                    }
                                }

                                // now enqueue the command to create a new one if the user didn't click Delete
                                Debug.Assert(
                                    principalProps.Count == dependentProps.Count,
                                    "principal (" + principalProps.Count + ") & dependent (" + dependentProps.Count
                                    + ") property counts must match!");
                                if (principalProps.Count > 0)
                                {
                                    Command cmd = new CreateReferentialConstraintCommand(
                                        dlg.Principal,
                                        dlg.Dependent,
                                        principalProps,
                                        dependentProps);
                                    commands.Add(cmd);
                                }
                            }
                        }
                    }
                }
                else
                {
                    VsUtils.ShowMessageBox(
                        PackageManager.Package,
                        Resources.Error_CannotEditRefConstraint,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                        OLEMSGICON.OLEMSGICON_WARNING);
                }
            }
            return commands;
        }

        internal ReferentialConstraintDialog(Association association)
        {
            if (association == null)
            {
                // this should never be null on current code-paths.
                throw new ArgumentNullException("association");
            }

            _association = association;
            _end1 = _association.AssociationEnds()[0];
            _end2 = _association.AssociationEnds()[1];

            var selfAssociation = (_end1.Type.Target == _end2.Type.Target);

            _roleListItems.Add(_end1, new RoleListItem(_end1, selfAssociation));
            _roleListItems.Add(_end2, new RoleListItem(_end2, selfAssociation));

            InitializeComponent();

            // enable tool tips
            listMappings.ShowItemToolTips = true;

            if (EdmFeatureManager.GetForeignKeysInModelFeatureState(association.Artifact.SchemaVersion).IsEnabled())
            {
                hdrDependent.Text = DialogsResource.RefConstraintDialog_DependentKeyHeader_SupportFKs;
            }

            // Set the default font to VS shell font.
            var vsFont = VSHelpers.GetVSFont(Services.ServiceProvider);
            if (vsFont != null)
            {
                Font = vsFont;
            }

            // load list of roles
            cmdPrincipalRole.Items.Add(_blankRoleListItem);
            cmdPrincipalRole.Items.Add(_roleListItems[_end1]);
            cmdPrincipalRole.Items.Add(_roleListItems[_end2]);
        }

        private void ReferentialConstraintDialog_Load(object sender, EventArgs e)
        {
            // calculate evenly spaced columns
            var columnWidth = (listMappings.ClientRectangle.Width / listMappings.Columns.Count);

            // make sure this is at least a certain width (125 should fit most column name), if so, set them all
            if (columnWidth > 125)
            {
                foreach (ColumnHeader ch in listMappings.Columns)
                {
                    ch.Width = columnWidth;
                }
            }

            // select the one we have (if one)
            if (_association.ReferentialConstraint != null)
            {
                var principal = _association.ReferentialConstraint.Principal.Role.Target;

                if (principal == _end1
                    || principal == _end2)
                {
                    cmdPrincipalRole.SelectedItem = _roleListItems[principal];
                }
                else
                {
                    Debug.Fail("unexpected principal end doesn't match the ends on the association");
                    cmdPrincipalRole.SelectedItem = _blankRoleListItem;
                }
                btnDelete.Enabled = true;
            }
            else
            {
                cmdPrincipalRole.SelectedItem = _blankRoleListItem;
                btnDelete.Enabled = false;
                cmbDependentKey.Enabled = false;
            }

            if (_association.EntityModel.IsCSDL == false)
            {
                cmdPrincipalRole.Enabled = false;
                listMappings.Enabled = false;
                btnOK.Enabled = false;
                btnDelete.Enabled = false;
            }

            ChangeDependencyKeyComboBoxVisibility(false);

            Debug.Assert(
                listMappings.GetType() == typeof(ReferentialConstraintListView),
                "VS has replaced the code in *.Designer.cs; you'll have to change it back");
        }

        internal AssociationEnd Principal
        {
            get { return _principal; }
        }

        internal AssociationEnd Dependent
        {
            get { return _dependent; }
        }

        internal IEnumerable<MappingListItem> MappingList
        {
            get
            {
                foreach (var mli in _mappingListItems.Values)
                {
                    yield return mli;
                }
            }
        }

        internal List<Symbol> PrincipalProperties
        {
            get
            {
                var props = new List<Symbol>();
                if (_principal != null)
                {
                    foreach (var item in _mappingListItems)
                    {
                        if (item.Value.DependentProperty != null)
                        {
                            props.Add(item.Value.PrincipalKey);
                        }
                    }
                }
                return props;
            }
        }

        internal List<Symbol> DependentProperties
        {
            get
            {
                var props = new List<Symbol>();
                if (_dependent != null)
                {
                    foreach (var item in _mappingListItems)
                    {
                        if (item.Value.DependentProperty != null)
                        {
                            props.Add(item.Value.DependentProperty);
                        }
                    }
                }
                return props;
            }
        }

        internal bool ShouldDeleteOnly
        {
            get { return _shouldDeleteOnly; }
        }

        private void cmdPrincipalRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            AssociationEnd principal = null;
            AssociationEnd dependent = null;
            if (cmdPrincipalRole.SelectedItem == _roleListItems[_end1])
            {
                principal = _end1;
                dependent = _end2;
            }
            else if (cmdPrincipalRole.SelectedItem == _roleListItems[_end2])
            {
                principal = _end2;
                dependent = _end1;
            }

            if (principal == _principal)
            {
                return; // no change
            }

            // remember new choice
            _principal = principal;
            _dependent = dependent;

            if (_dependent == null)
            {
                txtDependentRole.Text = string.Empty;
            }
            else
            {
                txtDependentRole.Text = _dependent.Role.Value;
            }

            // clear our lists
            listMappings.Items.Clear();
            _mappingListItems.Clear();

            cmbDependentKey.Items.Clear();
            _dependentListItems.Clear();

            // user might have chosen the blank row
            if (_principal != null
                && _dependent != null)
            {
                PopulateMappingListItems();
                PopulateListView();
                listMappings.Enabled = true;

                // load dependent role keys into the combo box
                if (_dependent.Type.Target != null)
                {
                    cmbDependentKey.Items.Add(_blankDependentKeyListItem);

                    foreach (var key in GetMappableDependentProperties())
                    {
                        var item = new KeyListItem(key.NormalizedName);
                        _dependentListItems.Add(key.NormalizedName, item);

                        cmbDependentKey.Items.Add(item);
                    }

                    // in the SSDL, ref constraints can be to non-key columns
                    if (_association.EntityModel.IsCSDL == false)
                    {
                        foreach (var prop in _dependent.Type.Target.Properties())
                        {
                            if (prop.IsKeyProperty)
                            {
                                continue;
                            }

                            var item = new KeyListItem(prop.NormalizedName);
                            _dependentListItems.Add(prop.NormalizedName, item);

                            cmbDependentKey.Items.Add(item);
                        }
                    }
                }
                cmbDependentKey.Enabled = true;

                // select the first row
                if (listMappings.Items.Count > 0)
                {
                    listMappings.Focus();

                    var lvItem = listMappings.Items[0];
                    lvItem.Selected = true;
                    ShowDependencyKeyComboBox(lvItem);
                }
            }
            else
            {
                listMappings.Enabled = false;
                cmbDependentKey.Enabled = false;
                ChangeDependencyKeyComboBoxVisibility(false);
            }
        }

        private IEnumerable<Property> GetMappableDependentProperties()
        {
            IEnumerable<Property> rtrn;
            if (_dependent == null)
            {
                rtrn = new Property[0];
            }
            else if (_association.EntityModel.IsCSDL == false)
            {
                // in the SSDL, ref constraints can be to non-key columns
                rtrn = _dependent.Type.Target.Properties();
            }
            else if (EdmFeatureManager.GetForeignKeysInModelFeatureState(_association.Artifact.SchemaVersion).IsEnabled())
            {
                // targeting netfx 4.0 or greater, so allow all properties on the dependent end
                var l = new List<Property>();
                var t = _dependent.Type.Target as ConceptualEntityType;
                Debug.Assert(_dependent.Type.Target == null || t != null, "EntityType is not ConceptualEntityType");
                while (t != null)
                {
                    foreach (var p in t.Properties())
                    {
                        if ((p is ComplexConceptualProperty) == false)
                        {
                            l.Add(p);
                        }
                    }
                    t = t.BaseType.Target;
                }
                rtrn = l;
            }
            else
            {
                // targeting netfx 3.5, so allow only allow keys on the dependent end
                if (_dependent.Type.Target != null)
                {
                    var cet = _dependent.Type.Target as ConceptualEntityType;
                    Debug.Assert(cet != null, "entity type is not a conceptual entity type");
                    rtrn = cet.ResolvableTopMostBaseType.ResolvableKeys;
                }
                else
                {
                    rtrn = new Property[0];
                }
            }
            return rtrn;
        }

        private void listMappings_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_handlingSelection)
            {
                return;
            }

            var mappingListItem = GetSelectedMappingListItem();
            if (mappingListItem == null)
            {
                EnableRightPane(false);
                cmbDependentKey.SelectedItem = _blankDependentKeyListItem;
            }
            else
            {
                EnableRightPane(true);

                if (mappingListItem.DependentProperty == null)
                {
                    cmbDependentKey.SelectedItem = _blankDependentKeyListItem;
                }
                    // In EF V1, if the DependentProperty is not part of the key, it is not included in the _dependentListItems.
                    // See: GetMappableDependentProperties() method.
                else if (_dependentListItems.ContainsKey(mappingListItem.DependentProperty))
                {
                    cmbDependentKey.SelectedItem = _dependentListItems[mappingListItem.DependentProperty];
                }
            }
        }

        private void listMappings_MouseUp(object sender, MouseEventArgs e)
        {
            // get the item on the row that is clicked
            var lvItem = listMappings.GetItemAt(e.X, e.Y);

            // make sure that an item is clicked
            if (lvItem != null)
            {
                ShowDependencyKeyComboBox(lvItem);
            }
        }

        private void listMappings_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == (int)Keys.F2)
            {
                var lvItem = GetSelectedListViewItem();
                if (lvItem != null)
                {
                    ShowDependencyKeyComboBox(lvItem);
                }
            }
        }

        private void ShowDependencyKeyComboBox(ListViewItem lvItem)
        {
            var mli = lvItem.Tag as MappingListItem;
            if (mli != null)
            {
                if (mli.IsValidPrincipalKey == false)
                {
                    // if this is not a valid principal key, we don't show the combo box.
                    return;
                }
            }

            // get the bounds of the item that is clicked
            var clickedItemRect = lvItem.Bounds;

            // see if the column is completely scrolled off to the left
            if ((clickedItemRect.Left + listMappings.Columns[0].Width + listMappings.Columns[1].Width) < 0)
            {
                // if the column is out of view to the left, do nothing
                return;
            }

            // see if the column is partially scrolled off to the left
            if (clickedItemRect.Left + listMappings.Columns[0].Width < 0)
            {
                // determine if column extends beyond right side of ListView
                if ((clickedItemRect.Left + listMappings.Columns[0].Width + listMappings.Columns[1].Width) > listMappings.Width)
                {
                    // set width of column to match width of ListView
                    clickedItemRect.Width = listMappings.Width;
                    clickedItemRect.X = 0;
                }
                else
                {
                    // the right side of cell is in view, restrict scope (remember clickedItemRect.Left is negative)
                    clickedItemRect.Width = listMappings.Columns[0].Width + listMappings.Columns[1].Width + clickedItemRect.Left;
                    clickedItemRect.X = 2;
                }
            }
            else if (listMappings.Columns[0].Width + listMappings.Columns[1].Width > listMappings.Width)
            {
                // left side is in view, but right side extends beyond the ListView
                clickedItemRect.Width = listMappings.Width - listMappings.Columns[0].Width - 2;
                clickedItemRect.X = 2 + listMappings.Columns[0].Width;
            }
            else
            {
                // both edges are in view
                clickedItemRect.Width = listMappings.Columns[1].Width;
                clickedItemRect.X = 2 + listMappings.Columns[0].Width;
            }

            // adjust the top to account for the location of the ListView
            clickedItemRect.Y += listMappings.Top + flowLayoutPanel1.Top;
            clickedItemRect.X += listMappings.Left + flowLayoutPanel1.Left;

            // assign calculated bounds to the ComboBox
            cmbDependentKey.Bounds = clickedItemRect;

            if (listMappings.IsVerticalScrollbarVisible)
            {
                // If combo overlays with vertical scrollbar, check whether we could display
                // the combo with size (clickedItemRect.Width - VerticalScrollbar.Width) 
                // because we do not want to combo overlays with vertical scrollbar UI
                if (clickedItemRect.Left < listMappings.Width
                    &&
                    clickedItemRect.Right > (listMappings.Width - SystemInformation.VerticalScrollBarWidth))
                {
                    var bounds = clickedItemRect;
                    bounds.Width -= SystemInformation.VerticalScrollBarWidth;

                    if (bounds.Right <= 0)
                    {
                        return;
                    }

                    cmbDependentKey.Bounds = bounds;

                    // Set dropdown width to the original calculated value to make sure
                    // we could see all text within the UI bound in the drop-down list
                    if (clickedItemRect.Width > 0)
                    {
                        cmbDependentKey.DropDownWidth = clickedItemRect.Width;
                    }
                }
            }

            // set default text for ComboBox to match the item that is clicked
            if (lvItem.SubItems.Count == 1)
            {
                cmbDependentKey.Text = string.Empty;
            }
            else
            {
                cmbDependentKey.Text = lvItem.SubItems[1].Text;
            }

            // display the ComboBox, and make sure that it is on top with focus
            ChangeDependencyKeyComboBoxVisibility(true);
            cmbDependentKey.BringToFront();
            cmbDependentKey.Focus();
        }

        private void ChangeDependencyKeyComboBoxVisibility(bool visible)
        {
            if (visible)
            {
                AcceptButton = null;
            }
            else
            {
                AcceptButton = btnOK;
            }
            cmbDependentKey.Visible = visible;
        }

        private void cmbDependentKey_KeyPress(object sender, KeyPressEventArgs e)
        {
            var mappingListItem = GetSelectedMappingListItem();

            // see if the user presses ESC
            switch (e.KeyChar)
            {
                case (char)(int)Keys.Escape:
                    {
                        // reset the original text value, and then hide the ComboBox
                        if (mappingListItem != null)
                        {
                            if (mappingListItem.DependentProperty == null)
                            {
                                cmbDependentKey.Text = string.Empty;
                            }
                            else
                            {
                                cmbDependentKey.Text = mappingListItem.DependentProperty.GetLocalName();
                            }
                        }
                        ChangeDependencyKeyComboBoxVisibility(false);
                        break;
                    }

                case (char)(int)Keys.Enter:
                    {
                        // hide the ComboBox
                        ChangeDependencyKeyComboBoxVisibility(false);
                        break;
                    }
            }
        }

        private void cmbDependentKey_SelectionChangeCommitted(object sender, EventArgs e)
        {
            AssignDependentKeySelection();
        }

        private void cmbDependentKey_Leave(object sender, EventArgs e)
        {
            AssignDependentKeySelection();
        }

        private void AssignDependentKeySelection()
        {
            var mappingListItem = GetSelectedMappingListItem();

            // if we have a list view selection
            if (mappingListItem != null)
            {
                if (cmbDependentKey.SelectedItem != _blankDependentKeyListItem)
                {
                    // user chose nothing, or else they chose the existing one - just return
                    var keyListItem = cmbDependentKey.SelectedItem as KeyListItem;
                    if (keyListItem == null
                        ||
                        keyListItem.Key.Equals(mappingListItem.DependentProperty))
                    {
                        ChangeDependencyKeyComboBoxVisibility(false);
                        return;
                    }

                    // set the new dependent key
                    mappingListItem.DependentProperty = keyListItem.Key;
                }
                else
                {
                    // user chose the blank row, but the key is already clear - just leave
                    if (mappingListItem.DependentProperty == null)
                    {
                        ChangeDependencyKeyComboBoxVisibility(false);
                        return;
                    }

                    // user chose the blank row, clear it out
                    mappingListItem.DependentProperty = null;
                }

                // reload the list view
                PopulateListView();

                // reselect the current item
                _handlingSelection = true;
                listMappings.Items[mappingListItem.CurrentIndex].Selected = true;
                _handlingSelection = false;
            }

            ChangeDependencyKeyComboBoxVisibility(false);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            _needsValidation = true;
            _shouldDeleteOnly = false;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            _needsValidation = true;
            _shouldDeleteOnly = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _needsValidation = false;
            _shouldDeleteOnly = false;
        }

        // <summary>
        //     shows a warning dialog if a dependent property is used more than once
        // </summary>
        // <returns>whether all dependent properties are used at most once</returns>
        private bool CheckDepPropMappedOnlyOnce()
        {
            var depPropsAlreadyUsed = new HashSet<Symbol>();
            var dupeProps = new HashSet<Symbol>(); // used to identify duplicated properties in error message

            foreach (var mli in MappingList)
            {
                if (mli.IsValidPrincipalKey)
                {
                    if (mli.DependentProperty != null)
                    {
                        if (depPropsAlreadyUsed.Contains(mli.DependentProperty))
                        {
                            if (!dupeProps.Contains(mli.DependentProperty))
                            {
                                // if property is duplicated it may be duplicated more than once
                                // only add to this list once
                                dupeProps.Add(mli.DependentProperty);
                            }
                        }
                        else
                        {
                            depPropsAlreadyUsed.Add(mli.DependentProperty);
                        }
                    }
                }
            }

            // dupeProps contains the list of dependent properties mapped more than once
            if (0 == dupeProps.Count)
            {
                return true;
            }
            else
            {
                // if dupeProps contains any properties display them in the warning and return false

                // construct list of duplicated Properties
                var listOfDupeProps = new StringBuilder();
                var isFirst = true;
                foreach (var prop in dupeProps)
                {
                    if (!isFirst)
                    {
                        listOfDupeProps.Append(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
                    }
                    isFirst = false;

                    listOfDupeProps.Append(prop.ToDisplayString());
                }

                // display warning dialog
                VsUtils.ShowMessageBox(
                    PackageManager.Package,
                    string.Format(
                        CultureInfo.CurrentCulture, DialogsResource.RefConstraintDialog_DependentPropMappedMultipleTimes, listOfDupeProps),
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                    OLEMSGICON.OLEMSGICON_WARNING);

                return false;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // if user pressed the OK button check if a dependent
            // property is mapped more than once
            if (DialogResult.OK == DialogResult
                &&
                false == CheckDepPropMappedOnlyOnce())
            {
                // if it is mapped more than once then we
                // insist the user fix this before closing
                e.Cancel = true;
                return;
            }

            // validate if the flag is set
            if (_needsValidation)
            {
                _needsValidation = false;

                // only validate if we aren't deleting
                if (!_shouldDeleteOnly)
                {
                    // check to see if nothing has been selected
                    if (Principal == null
                        || Dependent == null)
                    {
                        // if nothing selected treat this as a delete
                        _shouldDeleteOnly = true;
                    }
                }
            }
        }

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            // ignore request
            hevent.Handled = true;
        }

        private void PopulateMappingListItems()
        {
            _mappingListItems.Clear();

            // load existing mappings if we have a ref constraint and the user 
            // hasn't changed the principal role
            if (_association.ReferentialConstraint != null
                &&
                _association.ReferentialConstraint.Principal.Role.Target == _principal)
            {
                var principalType = _principal.Type.Target;
                var principalKeys = GetKeysForType(principalType);

                var pnum = _association.ReferentialConstraint.Principal.PropertyRefs.GetEnumerator();
                var dnum = _association.ReferentialConstraint.Dependent.PropertyRefs.GetEnumerator();

                // always move to the next principal ref
                while (pnum.MoveNext())
                {
                    var psym = pnum.Current.Name.NormalizedName();
                    Symbol dsym = null;
                    if (dnum.MoveNext())
                    {
                        dsym = dnum.Current.Name.NormalizedName();
                    }

                    var item = new MappingListItem(psym, dsym, IsValidPrincipalKey(psym, principalKeys));
                    _mappingListItems.Add(item.PrincipalKey, item);
                }
            }

            // add any remaining principal keys that aren't mapped yet in a 
            // ref constraint (which means all if no ref constraint exists)
            var principalEntityType = _principal.Type.Target as ConceptualEntityType;
            Debug.Assert(principalEntityType != null, "EntityType is not ConceptualEntityType");
            foreach (var key in principalEntityType.ResolvableTopMostBaseType.ResolvableKeys)
            {
                if (_mappingListItems.ContainsKey(key.NormalizedName) == false)
                {
                    var item = new MappingListItem(key.NormalizedName, null, true);
                    _mappingListItems.Add(item.PrincipalKey, item);
                }
            }

            // attempt to auto-map the keys if we don't have a ref constraint
            if (_association.ReferentialConstraint == null
                &&
                principalEntityType != null
                &&
                _dependent.Type.Target != null)
            {
                // load a list of dependent properties
                var dependentProperties = new List<Symbol>();

                foreach (var dprop in GetMappableDependentProperties())
                {
                    dependentProperties.Add(dprop.NormalizedName);
                }

                // process each principal key first by name
                foreach (var pkey in principalEntityType.ResolvableTopMostBaseType.ResolvableKeys)
                {
                    var item = _mappingListItems[pkey.NormalizedName];

                    // attempt a unique 1:1 name match between the principal key and any dependent property
                    if (item.DependentProperty == null)
                    {
                        item.DependentProperty = dependentProperties.Where(p => p.GetLocalName() == pkey.LocalName.Value).FirstOrDefault();
                        if (item.DependentProperty != null)
                        {
                            dependentProperties.Remove(item.DependentProperty);
                        }
                    }
                }

                // process any unmapped primary keys now ordinally
                foreach (var pkey in principalEntityType.ResolvableTopMostBaseType.ResolvableKeys)
                {
                    var item = _mappingListItems[pkey.NormalizedName];
                    if (item.DependentProperty == null)
                    {
                        // are there any unmapped dependent keys?
                        if (dependentProperties.Count > 0)
                        {
                            // there are, pick the first one
                            item.DependentProperty = dependentProperties[0];
                            dependentProperties.Remove(item.DependentProperty);
                        }
                    }
                }
            }
        }

        private void PopulateListView()
        {
            listMappings.Items.Clear();

            foreach (var pair in _mappingListItems)
            {
                var item = pair.Value;
                ListViewItem lvi = null;

                if (item.IsValidPrincipalKey == false)
                {
                    // principal property isn't a valid key
                    var propName = String.Format(
                        CultureInfo.CurrentCulture, Resources.RefConstraintDialog_ErrorInRCPrincipalProperty,
                        item.PrincipalKey.GetLocalName());
                    lvi = new ListViewItem(propName);
                    lvi.Font = new Font(lvi.Font, FontStyle.Italic);
                }
                else
                {
                    lvi = new ListViewItem(item.PrincipalKey.GetLocalName());
                }

                if (item.DependentProperty != null)
                {
                    ListViewItem.ListViewSubItem subItem = null;
                    var depProperty = _dependent.Artifact.ArtifactSet.LookupSymbol(item.DependentProperty) as Property;
                    if (depProperty != null)
                    {
                        subItem = new ListViewItem.ListViewSubItem(lvi, item.DependentProperty.GetLocalName());
                    }
                    else
                    {
                        // dependent property doesn't exist
                        var msg = String.Format(
                            CultureInfo.CurrentCulture, Resources.RefConstraintDialog_ErrorInRCDependentProperty,
                            item.DependentProperty.GetLocalName());
                        subItem = new ListViewItem.ListViewSubItem(lvi, msg);
                        subItem.Font = new Font(subItem.Font, FontStyle.Italic);
                    }
                    lvi.SubItems.Add(subItem);
                }

                lvi.Tag = item;
                listMappings.Items.Add(lvi);
            }
        }

        // return true if the given symbol is for a valid key on the principal end
        private bool IsValidPrincipalKey(Symbol key, HashSet<Property> principalKeys)
        {
            IEnumerable<EFElement> symbols = _association.Artifact.ArtifactSet.GetSymbolList(key);
            foreach (var el in symbols)
            {
                var p = el as Property;
                if (p != null)
                {
                    if (principalKeys.Contains(p))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private ListViewItem GetSelectedListViewItem()
        {
            var selected = listMappings.SelectedItems;
            Debug.Assert(selected.Count <= 1, "selected.Count (" + selected.Count + ") should be <= 1");

            if (selected.Count == 1)
            {
                return selected[0];
            }

            return null;
        }

        private MappingListItem GetSelectedMappingListItem()
        {
            var selected = listMappings.SelectedItems;
            Debug.Assert(selected.Count <= 1, "selected.Count (" + selected.Count + ") should be <= 1");

            if (selected.Count == 1)
            {
                var mappingListItem = selected[0].Tag as MappingListItem;
                mappingListItem.CurrentIndex = selected[0].Index;
                return mappingListItem;
            }

            return null;
        }

        // given a symbol and an entity type, return the property that matches a key for that entity type
        private static Property GetKeyForType(Symbol symbol, EntityType entityType, HashSet<Property> keys)
        {
            IEnumerable<EFElement> elements = entityType.Artifact.ArtifactSet.GetSymbolList(symbol);
            foreach (var e in elements)
            {
                var p = e as Property;
                if (keys.Contains(p))
                {
                    return p;
                }
            }
            return null;
        }

        private static HashSet<Property> GetKeysForType(EntityType entityType)
        {
            var principalKeys = new HashSet<Property>();
            if (entityType != null)
            {
                var cet = entityType as ConceptualEntityType;
                if (cet != null)
                {
                    foreach (var c in cet.SafeSelfAndBaseTypes)
                    {
                        foreach (var k in c.ResolvableKeys)
                        {
                            principalKeys.Add(k);
                        }
                    }
                }
                else
                {
                    foreach (var p in entityType.ResolvableKeys)
                    {
                        principalKeys.Add(p);
                    }
                }
            }
            return principalKeys;
        }

        private void EnableRightPane(bool enable)
        {
            cmbDependentKey.Enabled = enable;
        }

        protected override void OnLoad(EventArgs e)
        {
            MinimumSize = Size;
            base.OnLoad(e);
        }
    }

    internal class RoleListItem
    {
        private readonly AssociationEnd _end;
        private readonly bool _useRoleName;

        internal RoleListItem(AssociationEnd end, bool useRoleName)
        {
            _end = end;
            _useRoleName = useRoleName;
        }

        internal AssociationEnd End
        {
            get { return _end; }
        }

        public override string ToString()
        {
            if (_end != null && _useRoleName)
            {
                return _end.Role.Value;
            }
            else if (_end != null
                     && _end.Type.Target != null
                     && _useRoleName == false)
            {
                return _end.Type.Target.LocalName.Value;
            }

            return string.Empty;
        }
    }

    internal class MappingListItem
    {
        private readonly Symbol _principalSymbol;

        internal bool IsValidPrincipalKey { get; private set; }

        internal MappingListItem(Symbol principalSymbol, Symbol dependentSymbol, bool isValidPrincipalKey)
        {
            _principalSymbol = principalSymbol;
            DependentProperty = dependentSymbol;
            IsValidPrincipalKey = isValidPrincipalKey;
        }

        internal Symbol PrincipalKey
        {
            get { return _principalSymbol; }
        }

        internal Symbol DependentProperty { get; set; }

        internal int CurrentIndex { get; set; }

        public override string ToString()
        {
            if (_principalSymbol != null)
            {
                return _principalSymbol.GetLocalName();
            }
            return string.Empty;
        }
    }

    internal class KeyListItem
    {
        private readonly Symbol _key;

        internal KeyListItem(Symbol key)
        {
            _key = key;
        }

        internal Symbol Key
        {
            get { return _key; }
        }

        public override string ToString()
        {
            if (_key != null)
            {
                return _key.GetLocalName();
            }
            return string.Empty;
        }
    }
}
