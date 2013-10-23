// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Associations
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns;

    [TreeGridDesignerRootBranch(typeof(AssociationBranch))]
    [TreeGridDesignerColumn(typeof(PropertyColumn), Order = 1)]
    [TreeGridDesignerColumn(typeof(OperatorColumn), Order = 2)]
    [TreeGridDesignerColumn(typeof(ColumnNameColumn), Order = 3)]
    internal class MappingAssociation : MappingAssociationMappingRoot
    {
        private MappingAssociationSet _assocSet;

        public MappingAssociation(EditingContext context, Association assoc, MappingEFElement parent)
            : base(context, assoc, parent)
        {
            Debug.Assert(assoc != null, "MappingAssociation cannot accept a null Association");
            Debug.Assert(
                assoc.AssociationSet != null,
                "MappingAssociation cannot accept an Association " + assoc.ToPrettyString() + " with a null AssociationSet");
        }

        internal Association Association
        {
            get { return ModelItem as Association; }
        }

        protected override void LoadChildrenCollection()
        {
            _assocSet = ModelToMappingModelXRef.GetNewOrExisting(_context, Association.AssociationSet, this) as MappingAssociationSet;
            _children.Add(_assocSet);
        }

        protected override void OnChildDeleted(MappingEFElement melem)
        {
            if (_assocSet == melem)
            {
                _assocSet = null;
            }
        }
    }
}
