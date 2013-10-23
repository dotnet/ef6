// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns
{
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Functions;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;

    internal class UseOriginalValueColumn : BaseColumn
    {
        public UseOriginalValueColumn()
            : base(Resources.MappingDetails_UseOriginalValueColumn)
        {
        }

        protected override float GetWidthPercentage()
        {
            // constant size for this column, we are just a checkbox
            return 0.12f;
        }

        internal override bool ColumnIsCheckBox
        {
            get { return true; }
        }

        internal override CheckBoxState GetCheckBoxValue(object component)
        {
            var mfsp = component as MappingFunctionScalarProperty;
            if (mfsp != null
                && mfsp.MappingModificationFunctionMapping != null
                && mfsp.MappingModificationFunctionMapping.ModificationFunctionType == ModificationFunctionType.Update)
            {
                if (mfsp.ModelItem != null)
                {
                    return (mfsp.UseOriginalValue ? CheckBoxState.Checked : CheckBoxState.Unchecked);
                }
                else
                {
                    return CheckBoxState.UncheckedDisabled;
                }
            }

            return CheckBoxState.Unsupported;
        }

        internal override StateRefreshChanges ToggleCheckBoxValue(object component)
        {
            var mfsp = component as MappingFunctionScalarProperty;
            if (mfsp != null
                &&
                mfsp.ModelItem != null)
            {
                mfsp.UseOriginalValue = !mfsp.UseOriginalValue;
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
