// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches
{
    using System.Collections;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Functions;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;

    // <summary>
    //     This branch shows 3 lines, one each for an Insert, Update and Delete function.
    // </summary>
    internal class ModificationFunctionBranch : TreeGridDesignerBranch
    {
        private MappingFunctionEntityType _mappingFunctionTypeMapping;
        private readonly IBranch[] _expandedBranches = new IBranch[3];

        internal ModificationFunctionBranch(
            MappingFunctionEntityType mappingFunctionTypeMapping, TreeGridDesignerColumnDescriptor[] columns)
            : base(mappingFunctionTypeMapping, columns)
        {
            _mappingFunctionTypeMapping = mappingFunctionTypeMapping;
        }

        public ModificationFunctionBranch()
        {
        }

        public override bool Initialize(object component, TreeGridDesignerColumnDescriptor[] columns)
        {
            if (!base.Initialize(component, columns))
            {
                return false;
            }

            var mappingFunctionTypeMapping = component as MappingFunctionEntityType;
            if (mappingFunctionTypeMapping != null)
            {
                _mappingFunctionTypeMapping = mappingFunctionTypeMapping;
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
                return string.Empty;
            }
        }

        internal override object GetElement(int index)
        {
            return _mappingFunctionTypeMapping.Children[index];
        }

        internal override object GetCreatorElement()
        {
            return null;
        }

        internal override int GetIndexForElement(object element)
        {
            for (var i = 0; i < _mappingFunctionTypeMapping.Children.Count; i++)
            {
                if (element == _mappingFunctionTypeMapping.Children[i])
                {
                    return i;
                }
            }

            return -1;
        }

        internal override int ElementCount
        {
            get { return _mappingFunctionTypeMapping.Children.Count; }
        }

        protected override bool IsExpandable(int index)
        {
            if (index < ElementCount)
            {
                var mfm = GetElement(index) as MappingModificationFunctionMapping;
                if (mfm != null
                    && (mfm.MappingFunctionScalarProperties != null || mfm.MappingResultBindings != null))
                {
                    return true;
                }
            }
            return false;
        }

        protected override IBranch GetExpandedBranch(int index)
        {
            if (index < ElementCount)
            {
                var mfm = GetElement(index) as MappingModificationFunctionMapping;
                if (mfm != null)
                {
                    if (mfm.ModificationFunctionType == ModificationFunctionType.Delete)
                    {
                        _expandedBranches[index] = new ParametersBranch(mfm, GetColumns());
                    }
                    else
                    {
                        var branchList = new ArrayList(2);
                        branchList.Add(new ParametersBranch(mfm, GetColumns()));
                        branchList.Add(new ResultBindingsBranch(mfm, GetColumns()));
                        _expandedBranches[index] = new AggregateBranch(branchList, 0);
                    }
                    return _expandedBranches[index];
                }
            }

            return null;
        }

        protected override VirtualTreeDisplayData GetDisplayData(int row, int column, VirtualTreeDisplayDataMasks requiredData)
        {
            var data = base.GetDisplayData(row, column, requiredData);
            if (column == 0)
            {
                data.Image = data.SelectedImage = MappingDetailsImages.ICONS_FUNCTION;
                data.ImageList = MappingDetailsImages.GetIconsImageList();
            }

            return data;
        }

        public override void OnColumnValueChanged(TreeGridDesignerBranchChangedArgs args)
        {
            Debug.Assert(
                args.Row < _expandedBranches.Length,
                "args.Row (" + args.Row + ") should be < _expandedBranches.Length (" + _expandedBranches.Length + ")");
            if (args.Row < _expandedBranches.Length
                && _expandedBranches[args.Row] != null)
            {
                DoBranchModification(BranchModificationEventArgs.RemoveBranch(_expandedBranches[args.Row]));
                _expandedBranches[args.Row] = null;
            }
            base.OnColumnValueChanged(args);
        }
    }
}
