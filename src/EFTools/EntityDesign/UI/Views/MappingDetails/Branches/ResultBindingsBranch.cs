// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches
{
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Functions;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;

    /// <summary>
    ///     The purpose of this class is to create the container node for the parameters.  So,
    ///     there is only ever one item, one row.
    /// </summary>
    internal class ResultBindingsBranch : TreeGridDesignerBranch
    {
        private MappingModificationFunctionMapping _mappingModificationFunctionMapping;

        internal ResultBindingsBranch(
            MappingModificationFunctionMapping mappingModificationFunctionMapping, TreeGridDesignerColumnDescriptor[] columns)
            : base(mappingModificationFunctionMapping, columns)
        {
            _mappingModificationFunctionMapping = mappingModificationFunctionMapping;
        }

        public ResultBindingsBranch()
        {
        }

        public override bool Initialize(object component, TreeGridDesignerColumnDescriptor[] columns)
        {
            if (!base.Initialize(component, columns))
            {
                return false;
            }

            var mappingModificationFunctionMapping = component as MappingModificationFunctionMapping;
            if (mappingModificationFunctionMapping != null)
            {
                _mappingModificationFunctionMapping = mappingModificationFunctionMapping;
            }

            return true;
        }

        protected override string GetText(int row, int column)
        {
            if (column == 0)
            {
                return base.GetText(row, column);
            }
            else
            {
                // this branch just displays text in the first column
                return string.Empty;
            }
        }

        internal override object GetElement(int index)
        {
            return _mappingModificationFunctionMapping.MappingResultBindings;
        }

        internal override object GetCreatorElement()
        {
            return null;
        }

        internal override int GetIndexForElement(object element)
        {
            return 0;
        }

        internal override int ElementCount
        {
            get { return _mappingModificationFunctionMapping.MappingResultBindings == null ? 0 : 1; }
        }

        protected override bool IsExpandable(int index)
        {
            return (index < ElementCount);
        }

        protected override IBranch GetExpandedBranch(int index)
        {
            if (index < ElementCount)
            {
                var mrb = GetElement(index) as MappingResultBindings;
                if (mrb != null)
                {
                    return new ResultBindingBranch(mrb, GetColumns());
                }
            }

            return null;
        }

        protected override VirtualTreeDisplayData GetDisplayData(int row, int column, VirtualTreeDisplayDataMasks requiredData)
        {
            var data = base.GetDisplayData(row, column, requiredData);
            if (column == 0)
            {
                data.Image = data.SelectedImage = MappingDetailsImages.ICONS_FOLDER;
                data.ImageList = MappingDetailsImages.GetIconsImageList();
            }

            return data;
        }
    }
}
