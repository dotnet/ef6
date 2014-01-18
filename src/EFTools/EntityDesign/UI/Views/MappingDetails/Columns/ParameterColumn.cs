// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Functions;

    // <summary>
    //     Based on the type of item being shown, show the correct text for the Column Name column.
    // </summary>
    internal class ParameterColumn : BaseColumn
    {
        public ParameterColumn()
            : base(Resources.MappingDetails_ParameterName)
        {
        }

        public override object /* PropertyDescriptor */ GetValue(object component)
        {
            var elem = component as MappingEFElement;
            if (elem != null)
            {
                EnsureTypeConverters(elem);
                return new MappingLovEFElement(elem, elem.Name);
            }

            return MappingEFElement.LovBlankPlaceHolder;
        }

        // <summary>
        //     Overriding this allows the list-of-values dropdowns to use
        //     the converter to convert back from a string to an object (in our
        //     case a MappingLovEFElement object)
        // </summary>
        public override Type /* PropertyDescriptor */ PropertyType
        {
            get { return typeof(ParameterColumnConverter); }
        }

        internal override bool IsDeleteSupported(object component)
        {
            if (component is MappingResultBinding
                ||
                component is MappingModificationFunctionMapping)
            {
                var mappingElement = component as MappingEFElement;

                Debug.Assert(mappingElement != null, "The component should be a MappingEFElement");
                if (mappingElement.ModelItem != null)
                {
                    return true;
                }
            }
            return false;
        }

        public override void /* PropertyDescriptor */ SetValue(object component, object value)
        {
            // the user clicked off of the drop-down without choosing a value
            if (value == null
                || MappingEFElement.LovBlankPlaceHolder == value)
            {
                return;
            }

            // if they picked on the "Empty" placeholder, ignore it
            if (MappingEFElement.LovEmptyPlaceHolder == value)
            {
                return;
            }

            // see if the incoming value is a string
            var valueAsString = value as string;
            var lovElement = value as MappingLovEFElement;
            Debug.Assert(
                lovElement != null || valueAsString != null,
                "value is not a MappingLovEFElement nor a string. Actual type is " + value.GetType().FullName);

            var mrb = component as MappingResultBinding;
            if (mrb != null)
            {
                if (mrb.ModelItem != null)
                {
                    if (string.IsNullOrEmpty(valueAsString))
                    {
                        // they cleared out the columnName field, so delete it
                        mrb.Delete(null);
                        OnValueChanged(this, new ColumnValueChangedEventArgs(TreeGridDesignerBranchChangedArgs.DeleteItemArgs));
                        return;
                    }
                    else
                    {
                        // they just changed the columnName, just update it in place
                        mrb.ColumnName = valueAsString;
                        OnValueChanged(this, ColumnValueChangedEventArgs.Default);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(valueAsString) == false)
                    {
                        // a brand new one, so create it
                        if (mrb.CreateModelItem(null, Host.Context, valueAsString))
                        {
                            // only invoke this if the model item was actually created
                            OnValueChanged(this, new ColumnValueChangedEventArgs(TreeGridDesignerBranchChangedArgs.InsertItemArgs));
                        }
                    }
                }

                return;
            }

            // the trid will sometimes send an empty string when the user drops down a list
            // and then clicks away; if this happens just leave
            if (string.IsNullOrEmpty(valueAsString))
            {
                return;
            }

            var mfm = component as MappingModificationFunctionMapping;
            if (mfm != null)
            {
                // if value is a string then we've called SetValue after an edit using keyboard
                // so lookup correct MappingLovElement and use that going forward
                lovElement = mfm.GetLovElementFromLovElementOrString(lovElement, valueAsString, ListOfValuesCollection.FirstColumn);
                if (lovElement == null)
                {
                    return;
                }

                if (MappingEFElement.LovDeletePlaceHolder == lovElement)
                {
                    // they selected the delete me row, so clear out the item's ModelItem but don't remove
                    // the node from the parent so that it can revert to its "CreateNode" text
                    mfm.DeleteModelItem(null);
                }
                else if (MappingEFElement.LovEmptyPlaceHolder == lovElement)
                {
                    return;
                }
                else
                {
                    var func = lovElement.ModelElement as Function;
                    Debug.Assert(
                        func != null,
                        "component is a MappingModificationFunctionMapping but value.ModelElement is of type " + value.GetType().FullName);

                    if (func != null)
                    {
                        if (mfm.ModelItem != null)
                        {
                            // they switched to a different one so delete the old underlying model item and then create a new one
                            var cpc = new CommandProcessorContext(
                                Host.Context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_UpdateMappingFragment);
                            mfm.SwitchModelItem(cpc, Host.Context, func, true);
                        }
                        else
                        {
                            mfm.CreateModelItem(null, Host.Context, func);
                        }
                    }
                }

                OnValueChanged(this, new ColumnValueChangedEventArgs(new TreeGridDesignerBranchChangedArgs()));
                return;
            }
        }

        internal override TreeGridDesignerValueSupportedStates GetValueSupported(object component)
        {
            _currentElement = component as MappingEFElement;

            if (component is MappingFunctionEntityType
                || component is MappingFunctionScalarProperties
                || component is MappingFunctionScalarProperty
                || component is MappingResultBindings)
            {
                return TreeGridDesignerValueSupportedStates.None;
            }
            else
            {
                return base.GetValueSupported(component);
            }
        }

        internal override void EnsureTypeConverters(MappingEFElement element)
        {
            if (_converter == null
                || _currentElement != element)
            {
                // create initial type converter
                _currentElement = element;
                if (element is MappingResultBinding)
                {
                    _converter = new NoDropDownParameterColumnConverter();
                }
                else
                {
                    _converter = new ParameterColumnConverter();
                }
            }
        }

        internal override object GetInPlaceEdit(object component, ref string alternateText)
        {
            // calling EnsureTypeConverters() ensures the right type converter
            // is in place when a drop-down is navigated to using the keyboard
            EnsureTypeConverters(component as MappingEFElement);

            var mrb = component as MappingResultBinding;
            if (mrb != null)
            {
                if (mrb.ResultBinding == null)
                {
                    alternateText = String.Empty;
                }
                return typeof(TreeGridDesignerInPlaceEdit);
            }
            else
            {
                return base.GetInPlaceEdit(component, ref alternateText);
            }
        }

        internal override bool AllowKeyDownProcessing(KeyEventArgs e, object component)
        {
            var mrb = component as MappingResultBinding;
            // if the in place edit is a regular text box and Delete key is pressed, we should disallow special key processing.
            if (mrb != null
                && e.KeyCode == Keys.Delete)
            {
                return false;
            }
            else
            {
                return base.AllowKeyDownProcessing(e, component);
            }
        }
    }

    internal class ParameterColumnConverter : BaseColumnConverter<ParameterColumn>
    {
        protected override void PopulateMapping(ITypeDescriptorContext context)
        {
            if (context != null)
            {
                _context = context;
            }

            Debug.Assert(_context != null, "Should have a context for the PopulateMapping call.");

            PopulateMappingForSelectedObject(_context.PropertyDescriptor as ParameterColumn);
        }

        protected override void PopulateMappingForSelectedObject(ParameterColumn selectedObject)
        {
            Debug.Assert(selectedObject != null, "selectedObject should not be null");

            if (selectedObject != null
                &&
                selectedObject.Element != null)
            {
                if (selectedObject.Element is MappingModificationFunctionMapping)
                {
                    var lov = selectedObject.Element.GetListOfValues(ListOfValuesCollection.FirstColumn);
                    foreach (var key in lov.Keys)
                    {
                        AddMapping(key, lov[key]);
                    }
                    return;
                }
            }
        }
    }

    // <summary>
    //     Used to override conversion for the case where the ParameterColumn is representing
    //     a MappingResultBinding
    // </summary>
    internal class NoDropDownParameterColumnConverter : ParameterColumnConverter
    {
        // needs to return false so as to provide ordinary Textbox for editing
        // (instead of drop-down) see TreeGridDesignerTreeControl.CreateTypeEditorHost()
        public override bool /* TypeConverter */ GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }
}
