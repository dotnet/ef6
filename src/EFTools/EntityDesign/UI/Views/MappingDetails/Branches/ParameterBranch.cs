// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches
{
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Functions;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;

    /// <summary>
    ///     This branch shows the scalar property mappings.
    /// </summary>
    internal class ParameterBranch : TreeGridDesignerBranch
    {
        private MappingFunctionScalarProperties _mappingFunctionScalarProperties;

        internal ParameterBranch(
            MappingFunctionScalarProperties mappingFunctionScalarProperties, TreeGridDesignerColumnDescriptor[] columns)
            : base(mappingFunctionScalarProperties, columns)
        {
            _mappingFunctionScalarProperties = mappingFunctionScalarProperties;
        }

        public ParameterBranch()
        {
        }

        public override bool Initialize(object component, TreeGridDesignerColumnDescriptor[] columns)
        {
            if (!base.Initialize(component, columns))
            {
                return false;
            }

            var mappingFunctionScalarProperties = component as MappingFunctionScalarProperties;
            if (mappingFunctionScalarProperties != null)
            {
                _mappingFunctionScalarProperties = mappingFunctionScalarProperties;
            }

            return true;
        }

        internal override object GetElement(int index)
        {
            return _mappingFunctionScalarProperties.Children[index];
        }

        internal override object GetCreatorElement()
        {
            return null;
        }

        internal override int GetIndexForElement(object element)
        {
            for (var i = 0; i < _mappingFunctionScalarProperties.Children.Count; i++)
            {
                if (element == _mappingFunctionScalarProperties.Children[i])
                {
                    return i;
                }
            }

            return -1;
        }

        internal override int ElementCount
        {
            get { return _mappingFunctionScalarProperties.Children.Count; }
        }

        protected override VirtualTreeDisplayData GetDisplayData(int row, int column, VirtualTreeDisplayDataMasks requiredData)
        {
            var data = base.GetDisplayData(row, column, requiredData);

            // construct underlying parameter
            Parameter param = null;
            if (row < ElementCount)
            {
                var mfsp = GetElement(row) as MappingFunctionScalarProperty;
                if (null != mfsp)
                {
                    param = mfsp.StoreParameter;
                }
            }

            if (column == 0)
            {
                data.Image = data.SelectedImage = MappingDetailsImages.ICONS_PARAMETER;
                data.ImageList = MappingDetailsImages.GetIconsImageList();
            }
            else if (column == 1)
            {
                // direction of arrow icon depends on InOut of underlying parameter
                if (null != param
                    && Parameter.InOutMode.Out == param.InOut)
                {
                    data.Image = data.SelectedImage = MappingDetailsImages.ARROWS_RIGHT;
                }
                else if (null != param
                         && Parameter.InOutMode.InOut == param.InOut)
                {
                    data.Image = data.SelectedImage = MappingDetailsImages.ARROWS_BOTH;
                }
                else
                {
                    data.Image = data.SelectedImage = MappingDetailsImages.ARROWS_LEFT;
                }
                data.ImageList = MappingDetailsImages.GetArrowsImageList();
            }
            else if (column == 2)
            {
                // do not show icon for Out parameters
                if (null != param
                    && Parameter.InOutMode.Out != param.InOut)
                {
                    if (_mappingFunctionScalarProperties.ScalarProperties.Count > row
                        && _mappingFunctionScalarProperties.ScalarProperties[row].ScalarProperty != null
                        && _mappingFunctionScalarProperties.ScalarProperties[row].ScalarProperty.Name.Status == BindingStatus.Known
                        && _mappingFunctionScalarProperties.ScalarProperties[row].ScalarProperty.Name.Target.IsKeyProperty)
                    {
                        data.Image = data.SelectedImage = MappingDetailsImages.ICONS_PROPERTY_KEY;
                    }
                    else
                    {
                        data.Image = data.SelectedImage = MappingDetailsImages.ICONS_PROPERTY;
                    }
                    data.ImageList = MappingDetailsImages.GetIconsImageList();
                }
            }

            return data;
        }
    }
}
