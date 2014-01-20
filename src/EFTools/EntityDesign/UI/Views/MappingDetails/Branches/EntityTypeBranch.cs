// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches
{
    using System.Collections.Generic;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Tables;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;

    // <summary>
    //     This is the header branch for the selected Entity Type Mapping.  It displays the
    //     tables that are mapped to this entity.
    // </summary>
    internal class EntityTypeBranch : HeaderBranch
    {
        private MappingConceptualEntityType _mappingConceptualTypeMapping;
        private TreeGridDesignerColumnDescriptor[] _columns;

        // <summary>
        //     ITreeGridDesignerInitializeBranch
        // </summary>
        public override bool Initialize(object component, TreeGridDesignerColumnDescriptor[] columns)
        {
            if (!base.Initialize(component, columns))
            {
                return false;
            }

            _mappingConceptualTypeMapping = component as MappingConceptualEntityType;
            if (_mappingConceptualTypeMapping != null)
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

            IBranch tableBranch = null;
            if (initialPopulation)
            {
                tableBranch = new TableBranch(_mappingConceptualTypeMapping, _columns);
            }
            else
            {
                var locateData = LocateObject("TABLE", ObjectStyle.TrackingObject, 0);
                tableBranch = locateData.Row >= 0
                                  ? GetObject(locateData.Row, 0, ObjectStyle.ExpandedBranch) as IBranch
                                  : new TableBranch(_mappingConceptualTypeMapping, _columns);
            }
            childBranches.Add(new ChildBranchInfo(tableBranch, Resources.MappingDetails_TablesHeader, "TABLE"));

            SetHeaderInfo(childBranches.ToArray(), _columns);
        }
    }
}
