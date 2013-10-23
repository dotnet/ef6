// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches
{
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.FunctionImports;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;

    /// <summary>
    ///     This branch shows the scalar property mappings for a FunctionImportMapping.
    /// </summary>
    internal class FunctionImportScalarPropertyBranch : TreeGridDesignerBranch
    {
        private MappingFunctionImport _mappingFunctionImport;

        internal FunctionImportScalarPropertyBranch(MappingFunctionImport mappingFunctionImport, TreeGridDesignerColumnDescriptor[] columns)
            : base(mappingFunctionImport, columns)
        {
            _mappingFunctionImport = mappingFunctionImport;
        }

        public FunctionImportScalarPropertyBranch()
        {
        }

        public override bool Initialize(object component, TreeGridDesignerColumnDescriptor[] columns)
        {
            if (!base.Initialize(component, columns))
            {
                return false;
            }

            var mappingFunctionImport = component as MappingFunctionImport;
            if (mappingFunctionImport != null)
            {
                _mappingFunctionImport = mappingFunctionImport;
            }

            return true;
        }

        internal override object GetElement(int index)
        {
            return _mappingFunctionImport.Children[index];
        }

        internal override object GetCreatorElement()
        {
            return null;
        }

        internal override int GetIndexForElement(object element)
        {
            for (var i = 0; i < _mappingFunctionImport.Children.Count; i++)
            {
                if (element == _mappingFunctionImport.Children[i])
                {
                    return i;
                }
            }

            return -1;
        }

        internal override int ElementCount
        {
            get { return _mappingFunctionImport.Children.Count; }
        }

        protected override VirtualTreeDisplayData GetDisplayData(int row, int column, VirtualTreeDisplayDataMasks requiredData)
        {
            var data = base.GetDisplayData(row, column, requiredData);
            if (column == 0)
            {
                var scalarProperty = row < ElementCount
                                         ? GetElement(row) as MappingFunctionImportScalarProperty
                                         : null;
                if (scalarProperty != null
                    && scalarProperty.IsKeyProperty)
                {
                    data.Image = data.SelectedImage = MappingDetailsImages.ICONS_PROPERTY_KEY;
                }
                else
                {
                    data.Image = data.SelectedImage = MappingDetailsImages.ICONS_PROPERTY;
                }
                data.ImageList = MappingDetailsImages.GetIconsImageList();
            }
            else if (column == 1)
            {
                data.Image = data.SelectedImage = MappingDetailsImages.ARROWS_LEFT;
                data.ImageList = MappingDetailsImages.GetArrowsImageList();
            }
            else if (column == 2)
            {
                data.Image = data.SelectedImage = MappingDetailsImages.ICONS_COLUMN;
                data.ImageList = MappingDetailsImages.GetIconsImageList();

                var scalarProperty = row < ElementCount
                                         ? GetElement(row) as MappingFunctionImportScalarProperty
                                         : null;
                if (scalarProperty != null
                    && (scalarProperty.ModelItem == null || scalarProperty.IsComplexProperty))
                {
                    // gray out the text if it's default value or error message
                    data.GrayText = true;
                }
            }

            return data;
        }
    }
}
