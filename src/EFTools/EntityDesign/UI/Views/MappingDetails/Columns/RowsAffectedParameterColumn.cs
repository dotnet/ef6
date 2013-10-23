// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns
{
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Functions;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;

    internal class RowsAffectedParameterColumn : BaseColumn
    {
        public RowsAffectedParameterColumn()
            : base(Resources.MappingDetails_RowsAffectedParameterColumn)
        {
        }

        protected override float GetWidthPercentage()
        {
            // constant size for this column, we are just a checkbox
            return 0.15f;
        }

        internal override bool ColumnIsCheckBox
        {
            get { return true; }
        }

        internal override CheckBoxState GetCheckBoxValue(object component)
        {
            var mfsp = component as MappingFunctionScalarProperty;

            // check we are within a ModificationFunctionMapping, that the
            // ModificationFunction can be reached and that the StoreParameter exists
            if (null != mfsp
                && null != mfsp.MappingModificationFunctionMapping
                && null != mfsp.MappingModificationFunctionMapping.ModificationFunction
                && null != mfsp.StoreParameter)
            {
                if (!mfsp.StoreParameter.CanBeUsedAsRowsAffectedParameter())
                {
                    // parameter is not suitable to use as RowsAffectedParameter
                    return CheckBoxState.UncheckedDisabled;
                }

                var rowsAffectedParameter = mfsp.MappingModificationFunctionMapping.ModificationFunction.RowsAffectedParameter.Target;
                if (null != rowsAffectedParameter
                    && rowsAffectedParameter == mfsp.StoreParameter)
                {
                    return CheckBoxState.Checked;
                }
                else
                {
                    return CheckBoxState.Unchecked;
                }
            }

            return CheckBoxState.Unsupported;
        }

        internal override StateRefreshChanges ToggleCheckBoxValue(object component)
        {
            var mfsp = component as MappingFunctionScalarProperty;
            if (null != mfsp
                && null != mfsp.MappingModificationFunctionMapping
                && null != mfsp.StoreParameter)
            {
                var checkBoxState = GetCheckBoxValue(component);
                if (CheckBoxState.UncheckedDisabled == checkBoxState
                    || CheckBoxState.Unsupported == checkBoxState)
                {
                    // checkbox not enabled or invisible - no updates
                    return StateRefreshChanges.None;
                }

                // if checkbox was previously checked then we are unchecking, so
                // set RowsAffectedParameter to this Parameter,
                // otherwise remove RowsAffectedParameter
                if (CheckBoxState.Checked == checkBoxState)
                {
                    mfsp.MappingModificationFunctionMapping.RowsAffectedParameter = null;
                }
                else
                {
                    mfsp.MappingModificationFunctionMapping.RowsAffectedParameter = mfsp.StoreParameter;
                }

                return StateRefreshChanges.Parents | StateRefreshChanges.Current;
            }

            return StateRefreshChanges.None;
        }

        internal override object GetInPlaceEdit(object component, ref string alternateText)
        {
            return null;
        }

        internal override bool IsDeleteSupported(object component)
        {
            return false;
        }

        public override object /* PropertyDescriptor */ GetValue(object component)
        {
            return string.Empty;
        }

        public override void /* PropertyDescriptor */ SetValue(object component, object value)
        {
        }

        internal override TreeGridDesignerValueSupportedStates GetValueSupported(object component)
        {
            if (component is MappingFunctionEntityType
                || component is MappingModificationFunctionMapping
                || component is MappingFunctionScalarProperties
                || component is MappingResultBindings
                || component is MappingResultBinding)
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
                _currentElement = element;
                _converter = null;
            }
        }
    }
}
