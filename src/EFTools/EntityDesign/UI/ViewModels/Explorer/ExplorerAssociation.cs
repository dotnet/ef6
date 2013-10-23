// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal abstract class ExplorerAssociation : EntityDesignExplorerEFElement
    {
        public ExplorerAssociation(EditingContext context, Association assoc, ExplorerEFElement parent)
            : base(context, assoc, parent)
        {
            // do nothing
        }

        protected override void InsertChild(EFElement efElementToInsert)
        {
            var assocEnd = efElementToInsert as AssociationEnd;
            if (assocEnd != null)
            {
                // the ViewModel does not keep track of AssociationEnd elements 
                // but it is not an error - so just return
                return;
            }

            var refConstraint = efElementToInsert as ReferentialConstraint;
            if (refConstraint != null)
            {
                // the ViewModel does not keep track of ReferentialConstraint elements 
                // but it is not an error - so just return
                return;
            }

            base.InsertChild(efElementToInsert);
        }

        protected override void LoadChildrenFromModel()
        {
            // do nothing
        }

        protected override void LoadWpfChildrenCollection()
        {
            // do nothing
        }
    }
}
