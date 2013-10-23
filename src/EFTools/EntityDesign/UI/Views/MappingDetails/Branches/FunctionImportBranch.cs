// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches
{
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.FunctionImports;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;

    internal class FunctionImportBranch : TreeGridDesignerBranch
    {
        private MappingFunctionImport _mappingFunctionImport;

        public override bool Initialize(object component, TreeGridDesignerColumnDescriptor[] columns)
        {
            if (!base.Initialize(component, columns))
            {
                return false;
            }

            _mappingFunctionImport = component as MappingFunctionImport;
            if (_mappingFunctionImport != null)
            {
                return true;
            }

            return false;
        }

        internal override int ElementCount
        {
            get { return 1; }
        }

        internal override object GetCreatorElement()
        {
            return null;
        }

        internal override object GetElement(int index)
        {
            return _mappingFunctionImport;
        }

        internal override int GetIndexForElement(object element)
        {
            return 0;
        }

        protected override bool IsExpandable(int index)
        {
            return (index < ElementCount);
        }

        protected override IBranch GetExpandedBranch(int index)
        {
            if (index == 0)
            {
                return new FunctionImportScalarPropertyBranch(_mappingFunctionImport, GetColumns());
            }

            return null;
        }
    }
}
