// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches
{
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Associations;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;

    internal class AssociationSetEndBranch : TreeGridDesignerBranch
    {
        private MappingAssociationSet _mappingAssociationSet;

        internal AssociationSetEndBranch(MappingAssociationSet mappingAssociationSet, TreeGridDesignerColumnDescriptor[] columns)
            : base(mappingAssociationSet, columns)
        {
            _mappingAssociationSet = mappingAssociationSet;
        }

        public AssociationSetEndBranch()
        {
        }

        public override bool Initialize(object component, TreeGridDesignerColumnDescriptor[] columns)
        {
            if (!base.Initialize(component, columns))
            {
                return false;
            }

            var mappingAssociationSet = component as MappingAssociationSet;
            if (mappingAssociationSet != null)
            {
                _mappingAssociationSet = mappingAssociationSet;
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
            return _mappingAssociationSet.Children[index];
        }

        internal override object GetCreatorElement()
        {
            return null;
        }

        internal override int GetIndexForElement(object element)
        {
            for (var i = 0; i < _mappingAssociationSet.Children.Count; i++)
            {
                if (element == _mappingAssociationSet.Children[i])
                {
                    return i;
                }
            }

            return -1;
        }

        internal override int ElementCount
        {
            get { return _mappingAssociationSet.Children.Count; }
        }

        protected override bool IsExpandable(int index)
        {
            return (index < ElementCount);
        }

        protected override IBranch GetExpandedBranch(int index)
        {
            if (index < ElementCount)
            {
                var mase = GetElement(index) as MappingAssociationSetEnd;
                if (mase != null)
                {
                    return new EndScalarPropertyBranch(mase, GetColumns());
                }
            }

            return null;
        }
    }
}
