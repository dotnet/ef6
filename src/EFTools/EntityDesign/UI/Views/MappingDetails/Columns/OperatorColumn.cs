// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Associations;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.FunctionImports;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Functions;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Tables;

    // <summary>
    //     Based on the type of item being shown, show the correct text for the Operator column.
    // </summary>
    internal class OperatorColumn : BaseColumn
    {
        internal static readonly MappingLovEFElement ArrowIconPlaceholder = new MappingLovEFElement(" ");

        public OperatorColumn()
            : base(Resources.MappingDetails_Operator)
        {
        }

        protected override float GetWidthPercentage()
        {
            // constant size for the Operator column
            return 0.10f;
        }

        public override object /* PropertyDescriptor */ GetValue(object component)
        {
            var mc = component as MappingCondition;
            if (mc != null)
            {
                EnsureTypeConverters(mc);
                return mc.Operator;
            }

            var msp = component as MappingScalarProperty;
            if (msp != null)
            {
                EnsureTypeConverters(msp);
                return ArrowIconPlaceholder;
            }

            var mesp = component as MappingEndScalarProperty;
            if (mesp != null)
            {
                EnsureTypeConverters(mesp);
                return ArrowIconPlaceholder;
            }

            var mfsp = component as MappingFunctionScalarProperty;
            if (mfsp != null)
            {
                EnsureTypeConverters(mfsp);
                return ArrowIconPlaceholder;
            }

            var mrb = component as MappingResultBinding;
            if (mrb != null)
            {
                EnsureTypeConverters(mrb);
                return ArrowIconPlaceholder;
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
            get { return typeof(OperatorColumnConverter); }
        }

        internal override bool IsDeleteSupported(object component)
        {
            return false;
        }

        // This method receives changes as MappingLovElements (user used the mouse)
        // or as strings (user used the keyboard)
        public override void /* PropertyDescriptor */ SetValue(object component, object value)
        {
            // if they picked on the "Empty" placeholder, ignore it
            if (MappingEFElement.LovEmptyPlaceHolder == value)
            {
                return;
            }

            // if we get a blank, that is never valid
            if (value == null
                || MappingEFElement.LovBlankPlaceHolder == value)
            {
                return;
            }

            var lovElement = value as MappingLovEFElement;
            var valueAsString = value as string;

            Debug.Assert(
                lovElement != null || valueAsString != null,
                "value is not a MappingLovEFElement nor a string. Actual type is " + value.GetType().FullName);
            if (lovElement == null
                && string.IsNullOrEmpty(valueAsString))
            {
                // Both null is an error condition and should not happen.
                // But the trid will sometimes send an empty string when the user drops down a list
                // and then clicks away; in both cases just return without doing anything.
                return;
            }

            var mc = component as MappingCondition;
            if (mc != null)
            {
                Debug.Assert(mc.ModelItem != null, "MappingCondition should not have null ModelItem");
                lovElement = mc.GetLovElementFromLovElementOrString(lovElement, valueAsString, ListOfValuesCollection.SecondColumn);
                if (lovElement != null)
                {
                    mc.Operator = lovElement;
                    OnValueChanged(this, ColumnValueChangedEventArgs.Default);
                }
                return;
            }
        }

        internal override TreeGridDesignerValueSupportedStates GetValueSupported(object component)
        {
            if (component is MappingColumnMappings
                || component is MappingConceptualEntityType
                || component is MappingStorageEntityType
                || component is MappingScalarProperty
                || component is MappingAssociationSet
                || component is MappingAssociationSetEnd
                || component is MappingEndScalarProperty
                || component is MappingFunctionEntityType
                || component is MappingModificationFunctionMapping
                || component is MappingFunctionScalarProperties
                || component is MappingFunctionScalarProperty
                || component is MappingResultBindings
                || component is MappingResultBinding
                || component is MappingFunctionImport
                || component is MappingFunctionImportScalarProperty)
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
                _converter = new OperatorColumnConverter();
            }
        }
    }

    internal class OperatorColumnConverter : BaseColumnConverter<OperatorColumn>
    {
        protected override void PopulateMapping(ITypeDescriptorContext context)
        {
            if (context != null)
            {
                _context = context;
            }

            Debug.Assert(_context != null, "Should have a context for the PopulateMapping call.");

            PopulateMappingForSelectedObject(_context.PropertyDescriptor as OperatorColumn);
        }

        protected override void PopulateMappingForSelectedObject(OperatorColumn selectedObject)
        {
            Debug.Assert(selectedObject != null, "selectedObject should not be null");

            if (selectedObject != null
                &&
                selectedObject.Element != null)
            {
                if (selectedObject.Element is MappingCondition)
                {
                    var lov = selectedObject.Element.GetListOfValues(ListOfValuesCollection.SecondColumn);
                    foreach (var key in lov.Keys)
                    {
                        AddMapping(key, lov[key]);
                    }
                    return;
                }
            }
        }
    }
}
