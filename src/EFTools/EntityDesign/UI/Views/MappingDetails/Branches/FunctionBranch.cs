// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches
{
    using System.Collections.Generic;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Functions;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;

    /// <summary>
    ///     This is the header branch for the selected Function to EntityType.  It displays the
    ///     functions that are mapped to this entity.
    /// </summary>
    internal class FunctionBranch : HeaderBranch
    {
        private MappingFunctionEntityType _mappingFunctionTypeMapping;
        private TreeGridDesignerColumnDescriptor[] _columns;

        /// <summary>
        ///     ITreeGridDesignerInitializeBranch
        /// </summary>
        public override bool Initialize(object component, TreeGridDesignerColumnDescriptor[] columns)
        {
            if (!base.Initialize(component, columns))
            {
                return false;
            }

            _mappingFunctionTypeMapping = component as MappingFunctionEntityType;
            if (_mappingFunctionTypeMapping != null)
            {
                _columns = columns;
                PopulateHeaders(true);
                return true;
            }

            return false;
        }

        private void PopulateHeaders(bool initialPopulation)
        {
            var childBranches = new List<ChildBranchInfo>(1);

            IBranch modificationFunctionBranch = null;
            if (initialPopulation)
            {
                modificationFunctionBranch = new ModificationFunctionBranch(_mappingFunctionTypeMapping, _columns);
            }
            else
            {
                var locateData = LocateObject("FUNCTION", ObjectStyle.TrackingObject, 0);
                modificationFunctionBranch = locateData.Row >= 0
                                                 ? GetObject(locateData.Row, 0, ObjectStyle.ExpandedBranch) as IBranch
                                                 : new ModificationFunctionBranch(_mappingFunctionTypeMapping, _columns);
            }
            childBranches.Add(new ChildBranchInfo(modificationFunctionBranch, Resources.MappingDetails_FunctionsHeader, "FUNCTION"));

            SetHeaderInfo(childBranches.ToArray(), _columns);
        }
    }
}
