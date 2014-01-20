// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Tables;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;

    // <summary>
    //     This branch displays all of the conditions in for a table/entity mapping.  It also displays a
    //     creator node so that users can add new conditions.
    // </summary>
    internal class ConditionBranch : TreeGridDesignerBranch
    {
        private MappingStorageEntityType _mappingStorageEntityType;
        //private bool _registeredEventHandlers = false;

        internal ConditionBranch(MappingStorageEntityType mappingStorageEntityType, TreeGridDesignerColumnDescriptor[] columns)
            : base(mappingStorageEntityType, columns)
        {
            _mappingStorageEntityType = mappingStorageEntityType;
        }

        public ConditionBranch()
        {
        }

        public override bool Initialize(object component, TreeGridDesignerColumnDescriptor[] columns)
        {
            if (!base.Initialize(component, columns))
            {
                return false;
            }

            var mappingStorageEntityType = component as MappingStorageEntityType;
            if (mappingStorageEntityType != null)
            {
                _mappingStorageEntityType = mappingStorageEntityType;
            }

            return true;
        }

        internal override object GetElement(int index)
        {
            return _mappingStorageEntityType.Children[index];
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal override object GetCreatorElement()
        {
            var mc = new MappingCondition(null, null, _mappingStorageEntityType);
            return mc;
        }

        internal override int GetIndexForElement(object element)
        {
            for (var i = 0; i < _mappingStorageEntityType.Children.Count; i++)
            {
                if (element == _mappingStorageEntityType.Children[i])
                {
                    return i;
                }
            }

            return -1;
        }

        internal override int ElementCount
        {
            get { return _mappingStorageEntityType.Children.Count; }
        }

        internal override int CreatorNodeCount
        {
            get { return 1; }
        }

        protected override string GetCreatorNodeText(int index)
        {
            return Resources.MappingDetails_ConditionCreatorNode;
        }

        // Note: index is the index number of which Condition we are creating
        // i.e. if this is the n-th Condition, index will be n-1
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override LabelEditResult OnCreatorNodeEditCommitted(int index, object value, int insertIndex)
        {
            var columnName = value as string;
            if (!string.IsNullOrEmpty(columnName))
            {
                var treeGridColumns = GetColumns();
                Debug.Assert(
                    treeGridColumns != null && treeGridColumns.Length > 0, "TreeGridColumns does not have expected number of columns");

                if (treeGridColumns != null
                    && treeGridColumns.Length > 0)
                {
                    var treeGridColumn = treeGridColumns[0];
                    Debug.Assert(treeGridColumn != null, "First TreeGridColumn is null");
                    if (treeGridColumn != null)
                    {
                        var mc = new MappingCondition(_mappingStorageEntityType.Context, null, _mappingStorageEntityType);
                        var lovElement = mc.FindMappingLovElement(columnName, ListOfValuesCollection.FirstColumn);

                        // if lovElement is LovEmptyPlaceHolder then user is clicking off the 'Empty'
                        // entry in the drop-down list and there's nothing to insert
                        if (lovElement != null
                            && lovElement != MappingEFElement.LovEmptyPlaceHolder)
                        {
                            treeGridColumn.SetValue(mc, lovElement);
                            DoBranchModification(BranchModificationEventArgs.InsertItems(this, insertIndex, 1));
                            return LabelEditResult.AcceptEdit;
                        }
                    }
                }
            }

            return base.OnCreatorNodeEditCommitted(index, value, insertIndex);
        }

        protected override VirtualTreeDisplayData GetDisplayData(int row, int column, VirtualTreeDisplayDataMasks requiredData)
        {
            var data = base.GetDisplayData(row, column, requiredData);
            if (column == 0)
            {
                data.Image = data.SelectedImage = MappingDetailsImages.ICONS_CONDITION;
                data.ImageList = MappingDetailsImages.GetIconsImageList();
            }
            if (column == 2)
            {
                // if the value of condition is empty string we want to show gray text "<Emtpy String>" instead
                var mc = GetElement(row) as MappingCondition;
                if (mc != null
                    && mc.IsValueEmptyString)
                {
                    data.GrayText = true;
                }
            }

            return data;
        }
    }
}
