// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.TypeEditors
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing.Design;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Tools.XmlDesignerBase;

    /// <summary>
    ///     Provides a drop-down editor experience for properties in the Properties Window
    ///     where there is a '(None)' option as well as free-form text
    /// </summary>
    internal class NoneOptionListBoxTypeEditor : UITypeEditor
    {
        private IWindowsFormsEditorService _editorService;

        /// <summary>
        ///     Helper method to update the DefaultableValue if and only if the value has changed
        /// </summary>
        /// <param name="existingValue">
        ///     existing value of attribute (StringOrNone.NoneValue indicates attribute
        ///     currently does not exist)
        /// </param>
        /// <param name="newValue">
        ///     new value of attribute passed into setter (null indicates user did not select
        ///     anything on the drop-down and so no setting should take place, StringOrNone.NoneValue indicates user
        ///     selected '(None)' on the drop-down and so attribute should be removed if present)
        /// </param>
        /// <param name="defaultableValueToUpdate"></param>
        internal static void UpdateDefaultableValueIfValuesDiffer(
            StringOrNone existingValue, StringOrNone newValue,
            DefaultableValue<StringOrNone> defaultableValueToUpdate)
        {
            if (null == newValue)
            {
                // user exited drop-down without selecting anything
                return;
            }

            if (existingValue.Equals(newValue))
            {
                // no change in value - so just return
                return;
            }
            else
            {
                // existingValue and valueToSet are different - so update the DefaultableValue
                // if newValue is NoneValue then set valueToSet to null which will remove the attribute
                // otherwise use newValue as is
                var valueToSet = (StringOrNone.NoneValue.Equals(newValue) ? null : newValue);
                var cmd =
                    new UpdateDefaultableValueCommand<StringOrNone>(defaultableValueToUpdate, valueToSet);
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
            }
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (null == provider)
            {
                Debug.Assert(false, "NoneOptionListBoxTypeEditor.EditValue(): provider object must not be null");
                // returning value means "no change"
                return value;
            }
            else
            {
                _editorService =
                    provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                Debug.Assert(null != _editorService, "Could not find IWindowsFormsEditorService for provider " + provider);
                if (null != _editorService)
                {
                    // create and populate the list-box
                    using (var listBox = new NoneOptionListBox())
                    {
                        // if the (None) value was the incoming value then show this by pre-selecting that
                        // value in the list, otherwise leave all items unselected
                        if (StringOrNone.NoneValue.Equals(value))
                        {
                            listBox.SelectedIndex = 0;
                        }

                        // now set up the SelectedValueChanged event handler
                        listBox.SelectedValueChanged += listBox_SelectedValueChanged;

                        // display the list-box and let user choose
                        _editorService.DropDownControl(listBox);
                        var selectedItem = listBox.SelectedItem as string;
                        Debug.Assert(
                            null == selectedItem
                            || Resources.NoneDisplayValueUsedForUX.Equals(selectedItem, StringComparison.CurrentCulture),
                            typeof(NoneOptionListBox).Name + ": selectedItem should be null or NoneObject");

                        // desubscribe from the SelectedValueChanged event handler
                        listBox.SelectedValueChanged -= listBox_SelectedValueChanged;

                        // return value dependent on what user chose
                        if (Resources.NoneDisplayValueUsedForUX.Equals(selectedItem, StringComparison.CurrentCulture))
                        {
                            value = StringOrNone.NoneValue;
                        }
                        else
                        {
                            // user did not select the None option - return null indicating no selection
                            value = null;
                        }
                    }
                }
            }

            return value;
        }

        internal void listBox_SelectedValueChanged(object sender, EventArgs e)
        {
            _editorService.CloseDropDown();
        }

        internal class NoneOptionListBox : ListBox
        {
            internal NoneOptionListBox()
            {
                BorderStyle = BorderStyle.FixedSingle;
                SelectionMode = SelectionMode.One; // only allow single-select
                Items.Add(Resources.NoneDisplayValueUsedForUX);
                Height = PreferredHeight; // scale to height of single item
            }

            // override to look like other drop-downs
            protected override Padding DefaultPadding
            {
                get { return new Padding(0, 0, 0, 12); }
            }

            protected override Padding DefaultMargin
            {
                get { return new Padding(0, 0, 0, 0); }
            }
        }
    }
}
