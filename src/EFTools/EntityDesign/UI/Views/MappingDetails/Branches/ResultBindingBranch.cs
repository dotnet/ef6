// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Functions;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    // <summary>
    //     This branch displays all of the conditions in for a table/entity mapping.  It also displays a
    //     creator node so that users can add new conditions.
    // </summary>
    internal class ResultBindingBranch : TreeGridDesignerBranch
    {
        private MappingResultBindings _mappingResultBindings;

        internal ResultBindingBranch(MappingResultBindings mappingResultBindings, TreeGridDesignerColumnDescriptor[] columns)
            : base(mappingResultBindings, columns)
        {
            _mappingResultBindings = mappingResultBindings;
        }

        public ResultBindingBranch()
        {
        }

        public override bool Initialize(object component, TreeGridDesignerColumnDescriptor[] columns)
        {
            if (!base.Initialize(component, columns))
            {
                return false;
            }

            var mappingResultBindings = component as MappingResultBindings;
            if (mappingResultBindings != null)
            {
                _mappingResultBindings = mappingResultBindings;
            }

            return true;
        }

        internal override object GetElement(int index)
        {
            return _mappingResultBindings.Children[index];
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal override object GetCreatorElement()
        {
            var mrb = new MappingResultBinding(null, null, _mappingResultBindings);
            return mrb;
        }

        internal override int GetIndexForElement(object element)
        {
            for (var i = 0; i < _mappingResultBindings.Children.Count; i++)
            {
                if (element == _mappingResultBindings.Children[i])
                {
                    return i;
                }
            }

            return -1;
        }

        internal override int ElementCount
        {
            get { return _mappingResultBindings.Children.Count; }
        }

        internal override int CreatorNodeCount
        {
            get { return 1; }
        }

        protected override string GetCreatorNodeText(int index)
        {
            return Resources.MappingDetails_ResultBindingCreatorNode;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override LabelEditResult OnCreatorNodeEditCommitted(int index, object value, int insertIndex)
        {
            var columnName = value as string;
            if (!string.IsNullOrEmpty(columnName))
            {
                var mrb = new MappingResultBinding(null, null, _mappingResultBindings);
                if (mrb.CreateModelItem(null, _mappingResultBindings.Context, columnName))
                {
                    DoBranchModification(BranchModificationEventArgs.InsertItems(this, insertIndex, 1));
                    return LabelEditResult.AcceptEdit;
                }
                else
                {
                    // attempt to create model item failed (no properties to map to)
                    return LabelEditResult.CancelEdit;
                }
            }
            else
            {
                return LabelEditResult.CancelEdit;
            }
        }

        protected override VirtualTreeDisplayData GetDisplayData(int row, int column, VirtualTreeDisplayDataMasks requiredData)
        {
            var data = base.GetDisplayData(row, column, requiredData);
            if (column == 0)
            {
                data.Image = data.SelectedImage = MappingDetailsImages.ICONS_RESULT_BINDING;
                data.ImageList = MappingDetailsImages.GetIconsImageList();
            }
            else if (column == 1)
            {
                data.Image = data.SelectedImage = MappingDetailsImages.ARROWS_RIGHT;
                data.ImageList = MappingDetailsImages.GetArrowsImageList();
            }
            else if (column == 2)
            {
                if (_mappingResultBindings.ResultBindings.Count > row
                    && _mappingResultBindings.ResultBindings[row].ResultBinding != null
                    && _mappingResultBindings.ResultBindings[row].ResultBinding.Name.Status == BindingStatus.Known
                    && _mappingResultBindings.ResultBindings[row].ResultBinding.Name.Target.IsKeyProperty)
                {
                    data.Image = data.SelectedImage = MappingDetailsImages.ICONS_PROPERTY_KEY;
                }
                else
                {
                    data.Image = data.SelectedImage = MappingDetailsImages.ICONS_PROPERTY;
                }
                data.ImageList = MappingDetailsImages.GetIconsImageList();
            }

            return data;
        }
    }
}
