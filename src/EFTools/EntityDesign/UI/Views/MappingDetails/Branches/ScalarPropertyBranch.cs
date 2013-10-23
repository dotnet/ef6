// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches
{
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Tables;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;

    /// <summary>
    ///     This branch shows the scalar property mappings.
    /// </summary>
    internal class ScalarPropertyBranch : TreeGridDesignerBranch
    {
        private MappingColumnMappings _mappingColumnMappings;

        internal ScalarPropertyBranch(MappingColumnMappings mappingColumnMappings, TreeGridDesignerColumnDescriptor[] columns)
            : base(mappingColumnMappings, columns)
        {
            _mappingColumnMappings = mappingColumnMappings;
        }

        public ScalarPropertyBranch()
        {
        }

        public override bool Initialize(object component, TreeGridDesignerColumnDescriptor[] columns)
        {
            if (!base.Initialize(component, columns))
            {
                return false;
            }

            var mappingColumnMappings = component as MappingColumnMappings;
            if (mappingColumnMappings != null)
            {
                _mappingColumnMappings = mappingColumnMappings;
            }

            return true;
        }

        internal override object GetElement(int index)
        {
            return _mappingColumnMappings.Children[index];
        }

        internal override object GetCreatorElement()
        {
            return null;
        }

        internal override int GetIndexForElement(object element)
        {
            for (var i = 0; i < _mappingColumnMappings.Children.Count; i++)
            {
                if (element == _mappingColumnMappings.Children[i])
                {
                    return i;
                }
            }

            return -1;
        }

        internal override int ElementCount
        {
            get { return _mappingColumnMappings.Children.Count; }
        }

        protected override VirtualTreeDisplayData GetDisplayData(int row, int column, VirtualTreeDisplayDataMasks requiredData)
        {
            var data = base.GetDisplayData(row, column, requiredData);
            if (column == 0)
            {
                if (_mappingColumnMappings.ScalarProperties.Count > row
                    && _mappingColumnMappings.ScalarProperties[row].IsKeyColumn)
                {
                    data.Image = data.SelectedImage = MappingDetailsImages.ICONS_COLUMN_KEY;
                }
                else
                {
                    data.Image = data.SelectedImage = MappingDetailsImages.ICONS_COLUMN;
                }
                data.ImageList = MappingDetailsImages.GetIconsImageList();
            }
            else if (column == 1)
            {
                data.Image = data.SelectedImage = MappingDetailsImages.ARROWS_BOTH;
                data.ImageList = MappingDetailsImages.GetArrowsImageList();
            }
            else if (column == 2)
            {
                // cache off the count
                var propertyCount = _mappingColumnMappings.ScalarProperties.Count;

                // cache off the property reference (if we have one)
                Property property = null;
                if (propertyCount > row
                    && _mappingColumnMappings.ScalarProperties[row].ScalarProperty != null
                    && _mappingColumnMappings.ScalarProperties[row].ScalarProperty.Name.Status == BindingStatus.Known)
                {
                    property = _mappingColumnMappings.ScalarProperties[row].ScalarProperty.Name.Target;
                }

                if (property != null
                    && property.IsKeyProperty)
                {
                    // if we have a valid property and its a key, show the key icon
                    data.Image = data.SelectedImage = MappingDetailsImages.ICONS_PROPERTY_KEY;
                }
                else if (property != null
                         && property.IsComplexTypeProperty)
                {
                    // if we have a valid property and its a complex property, show that one
                    data.Image = data.SelectedImage = MappingDetailsImages.ICONS_COMPLEX_PROPERTY;
                }
                else
                {
                    // in all other cases, show the default property icon
                    data.Image = data.SelectedImage = MappingDetailsImages.ICONS_PROPERTY;
                }

                // regardless, we use the same image list
                data.ImageList = MappingDetailsImages.GetIconsImageList();
            }

            return data;
        }
    }
}
