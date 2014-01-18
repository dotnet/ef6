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

    internal partial class ComplexTypePickerDialog : Form
    {
        internal ComplexTypePickerDialog(ConceptualEntityModel cModel)
        {
            InitializeComponent();

            // Set the default font to VS shell font.
            var vsFont = VSHelpers.GetVSFont(Services.ServiceProvider);
            if (vsFont != null)
            {
                Font = vsFont;
            }

            Debug.Assert(cModel != null, "Please specify ConceptualEntityModel");
            if (cModel != null)
            {
                var complexTypes = new List<ComplexType>(cModel.ComplexTypes());
                complexTypes.Sort(EFElement.EFElementDisplayNameComparison);
                complexTypesListBox.Items.AddRange(complexTypes.ToArray());
                ViewUtils.DisplayHScrollOnListBoxIfNecessary(complexTypesListBox);
            }

            complexTypesListBox.SelectedIndexChanged += complexTypesListBox_SelectedIndexChanged;
            complexTypesListBox.MouseDoubleClick += complexTypesListBox_MouseDoubleClick;
            okButton.Enabled = ComplexType != null;
        }

        // <summary>
        //     Use this constructor if you want to remove a ComplexType (for example currently selected) from the list
        // </summary>
        internal ComplexTypePickerDialog(ConceptualEntityModel cModel, ComplexType complexTypeToRemove)
            : this(cModel)
        {
            Debug.Assert(complexTypeToRemove != null, "Null ComplexType passed");
            if (complexTypeToRemove != null)
            {
                complexTypesListBox.Items.Remove(complexTypeToRemove);
            }
        }

        private void complexTypesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            okButton.Enabled = ComplexType != null;
        }

        private void complexTypesListBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (ComplexType != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        internal ComplexType ComplexType
        {
            get { return complexTypesListBox.SelectedItem as ComplexType; }
        }
    }
}
