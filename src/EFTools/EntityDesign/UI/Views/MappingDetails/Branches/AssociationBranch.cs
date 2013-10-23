// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches
{
    using System.Collections.Generic;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Associations;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid;

    internal class AssociationBranch : HeaderBranch
    {
        private MappingAssociation _mappingAssociation;
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

            _mappingAssociation = component as MappingAssociation;
            if (_mappingAssociation != null)
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

            IBranch assocSetBranch = null;
            if (initialPopulation)
            {
                assocSetBranch = new AssociationSetBranch(_mappingAssociation, _columns);
            }
            else
            {
                var locateData = LocateObject("ASSOCIATION", ObjectStyle.TrackingObject, 0);
                assocSetBranch = locateData.Row >= 0
                                     ? GetObject(locateData.Row, 0, ObjectStyle.ExpandedBranch) as IBranch
                                     : new AssociationSetBranch(_mappingAssociation, _columns);
            }
            childBranches.Add(new ChildBranchInfo(assocSetBranch, Resources.MappingDetails_AssociationHeader, "ASSOCIATION"));

            SetHeaderInfo(childBranches.ToArray(), _columns);
        }
    }
}
