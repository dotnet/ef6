// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches
{
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Associations;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;

    // <summary>
    //     This branch shows the scalar property mappings for an assocation set end property.
    // </summary>
    internal class EndScalarPropertyBranch : TreeGridDesignerBranch
    {
        private MappingAssociationSetEnd _mappingAssociationSetEnd;

        internal EndScalarPropertyBranch(MappingAssociationSetEnd mappingAssociationSetEnd, TreeGridDesignerColumnDescriptor[] columns)
            : base(mappingAssociationSetEnd, columns)
        {
            _mappingAssociationSetEnd = mappingAssociationSetEnd;
        }

        public EndScalarPropertyBranch()
        {
        }

        public override bool Initialize(object component, TreeGridDesignerColumnDescriptor[] columns)
        {
            if (!base.Initialize(component, columns))
            {
                return false;
            }

            var mappingAssociationSetEnd = component as MappingAssociationSetEnd;
            if (mappingAssociationSetEnd != null)
            {
                _mappingAssociationSetEnd = mappingAssociationSetEnd;
            }

            return true;
        }

        internal override object GetElement(int index)
        {
            return _mappingAssociationSetEnd.Children[index];
        }

        internal override object GetCreatorElement()
        {
            return null;
        }

        internal override int GetIndexForElement(object element)
        {
            for (var i = 0; i < _mappingAssociationSetEnd.Children.Count; i++)
            {
                if (element == _mappingAssociationSetEnd.Children[i])
                {
                    return i;
                }
            }

            return -1;
        }

        internal override int ElementCount
        {
            get { return _mappingAssociationSetEnd.Children.Count; }
        }

        protected override VirtualTreeDisplayData GetDisplayData(int row, int column, VirtualTreeDisplayDataMasks requiredData)
        {
            var data = base.GetDisplayData(row, column, requiredData);
            if (column == 0)
            {
                data.Image = data.SelectedImage = MappingDetailsImages.ICONS_PROPERTY_KEY;
                data.ImageList = MappingDetailsImages.GetIconsImageList();
            }
            else if (column == 1)
            {
                data.Image = data.SelectedImage = MappingDetailsImages.ARROWS_BOTH;
                data.ImageList = MappingDetailsImages.GetArrowsImageList();
            }
            else if (column == 2)
            {
                if (_mappingAssociationSetEnd.ScalarProperties.Count > row
                    &&
                    _mappingAssociationSetEnd.ScalarProperties[row].ScalarProperty != null
                    &&
                    _mappingAssociationSetEnd.ScalarProperties[row].ScalarProperty.ColumnName.Status == BindingStatus.Known
                    &&
                    _mappingAssociationSetEnd.ScalarProperties[row].ScalarProperty.ColumnName.Target.IsKeyProperty)
                {
                    data.Image = data.SelectedImage = MappingDetailsImages.ICONS_COLUMN_KEY;
                }
                else
                {
                    data.Image = data.SelectedImage = MappingDetailsImages.ICONS_COLUMN;
                }
                data.ImageList = MappingDetailsImages.GetIconsImageList();
            }

            return data;
        }
    }
}
