// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Tables;

    // <summary>
    //     Based on the type of item being shown, show the correct text for the Value column.
    // </summary>
    internal class ValueColumn : BaseColumn
    {
        public ValueColumn()
            : base(Resources.MappingDetails_Value)
        {
        }

        internal override object GetInPlaceEdit(object component, ref string alternateText)
        {
            EnsureTypeConverters(component as MappingEFElement);
            var mc = component as MappingCondition;
            if (mc != null
                && mc.IsValueEmptyString)
            {
                // this will clear the "<Empty String>" gray text from the column when it enters an edit mode
                alternateText = String.Empty;
            }
            return base.GetInPlaceEdit(component, ref alternateText);
        }

        public override object /* PropertyDescriptor */ GetValue(object component)
        {
            var mc = component as MappingCondition;
            if (mc != null)
            {
                EnsureTypeConverters(mc);
                return new MappingLovEFElement(mc, mc.Value);
            }

            var msp = component as MappingScalarProperty;
            if (msp != null)
            {
                EnsureTypeConverters(msp);
                return new MappingLovEFElement(msp, msp.Value);
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
            get { return typeof(ValueColumnConverter); }
        }

        internal override bool IsDeleteSupported(object component)
        {
            if (component is MappingScalarProperty)
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

            var lovElement = value as MappingLovEFElement;
            var valueAsString = value as string;
            Debug.Assert(
                lovElement != null || valueAsString != null,
                "value is not a MappingLovEFElement nor a string. Actual type is " + value.GetType().FullName);

            if (lovElement == null
                && valueAsString == null)
            {
                return;
            }

            var mc = component as MappingCondition;
            if (mc != null)
            {
                Debug.Assert(mc.ModelItem != null, "MappingCondition should not have null ModelItem");

                if (mc.Operator == MappingCondition.LovOperatorIsPlaceHolder)
                {
                    // if the Operator is 'Is' and we still have text, must be 
                    // because user chose 'Null' or 'NotNull' using keyboard
                    lovElement = mc.GetLovElementFromLovElementOrString(lovElement, valueAsString, ListOfValuesCollection.ThirdColumn);
                    if (MappingEFElement.LovEmptyPlaceHolder == lovElement)
                    {
                        return;
                    }
                    else if (lovElement != null)
                    {
                        mc.Value = lovElement.DisplayName;
                    }
                }
                else
                {
                    // if the Operator is not 'Is' then user must enter text
                    // into the TextBox
                    Debug.Assert(valueAsString != null, "valueAsString null for ValuColumn.SetValue() for MappingCondition");
                    if (valueAsString != null)
                    {
                        mc.Value = valueAsString;
                    }
                }

                OnValueChanged(this, ColumnValueChangedEventArgs.Default);
                return;
            }

            // the trid will sometimes send an empty string when the user drops down a list
            // and then clicks away; if this happens just leave
            if (string.IsNullOrEmpty(valueAsString))
            {
                return;
            }

            var msp = component as MappingScalarProperty;
            if (msp != null)
            {
                lovElement = msp.GetLovElementFromLovElementOrString(lovElement, valueAsString, ListOfValuesCollection.ThirdColumn);
                if (lovElement == null)
                {
                    return;
                }

                if (MappingEFElement.LovDeletePlaceHolder == lovElement)
                {
                    // they selected the delete me row, so clear out the item
                    // don't delete the view model row since we want one for every table column
                    msp.DeleteModelItem(null);
                }
                else if (MappingEFElement.LovEmptyPlaceHolder == lovElement)
                {
                    return;
                }
                else
                {
                    var propertiesChain = lovElement.Object as List<Property>;

                    Debug.Assert(
                        propertiesChain != null,
                        "component is a " + component.GetType().Name + " but value.ModelElement is of type "
                        + lovElement.Object.GetType().FullName);

                    if (propertiesChain != null)
                    {
                        // create or update scalar property
                        msp.CreateOrUpdateModelItem(Host.Context, propertiesChain);
                    }
                }

                OnValueChanged(this, ColumnValueChangedEventArgs.Default);
                return;
            }
        }

        internal override TreeGridDesignerValueSupportedStates GetValueSupported(object component)
        {
            if (component is MappingColumnMappings
                || component is MappingConceptualEntityType
                || component is MappingStorageEntityType)
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
            var mappingCondition = element as MappingCondition;
            if (mappingCondition != null)
            {
                // if element is a MappingCondition ensure TypeConverter is
                // appropriate to Operation
                if (mappingCondition.Operator != MappingCondition.LovOperatorIsPlaceHolder)
                {
                    if (_converter == null
                        || _converter.GetType() != typeof(EqualsConditionValueColumnConverter)
                        || _currentElement != element)
                    {
                        _converter = new EqualsConditionValueColumnConverter();
                    }
                }
                else
                {
                    if (_converter == null
                        || _converter.GetType() != typeof(ValueColumnConverter)
                        || _currentElement != element)
                    {
                        _converter = new ValueColumnConverter();
                    }
                }

                _currentElement = element;
            }
            else if (_converter == null
                     || _currentElement != element)
            {
                // if not MappingCondition then only swap converter if
                // element has changed
                _currentElement = element;
                _converter = new ValueColumnConverter();
            }
        }

        internal override bool AllowKeyDownProcessing(KeyEventArgs e, object component)
        {
            EnsureTypeConverters(component as MappingEFElement);
            var mc = component as MappingCondition;
            // if the in place edit is a regular text box and Delete key is pressed, we should disallow special key processing.
            if (mc != null
                && Converter is EqualsConditionValueColumnConverter
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

    internal class ValueColumnConverter : BaseColumnConverter<ValueColumn>
    {
        protected override void PopulateMapping(ITypeDescriptorContext context)
        {
            if (context != null)
            {
                _context = context;
            }

            Debug.Assert(_context != null, "Should have a context for the PopulateMapping call.");

            PopulateMappingForSelectedObject(_context.PropertyDescriptor as ValueColumn);
        }

        protected override void PopulateMappingForSelectedObject(ValueColumn selectedObject)
        {
            Debug.Assert(selectedObject != null, "selectedObject should not be null");

            if (selectedObject != null
                && selectedObject.Element != null)
            {
                if (selectedObject.Element is MappingCondition
                    || selectedObject.Element is MappingScalarProperty)
                {
                    var lov = selectedObject.Element.GetListOfValues(ListOfValuesCollection.ThirdColumn);
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
    //     Used to override conversion for the case where the ValueColumn is representing
    //     the value for a MappingCondition with Operation '='
    // </summary>
    internal class EqualsConditionValueColumnConverter : ValueColumnConverter
    {
        // needs to return false so as to provide ordinary Textbox for editing
        // see TreeGridDesignerTreeControl.CreateTypeEditorHost()
        public override bool /* TypeConverter */ GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }
}
