// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges;
    using Microsoft.Data.Entity.Design.EntityDesigner.Utils;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling;

    internal partial class EntityType : IContainRelatedElementsToEmphasizeWhenSelected
    {
        /// <summary>
        ///     Returns all the Association related to the entity-type.
        /// </summary>
        public IEnumerable<ModelElement> RelatedElementsToEmphasizeOnSelected
        {
            get
            {
                var entityType = EntityDesignerViewModel.ModelXRef.GetExisting(this) as ConceptualEntityType;
                Debug.Assert(entityType != null, "Unable to find model EntityType for DSL EntityType:" + Name);
                if (entityType != null)
                {
                    foreach (var modelAssociation in Model.Entity.Association.GetAssociationsForEntityType(entityType))
                    {
                        var viewAssociation = EntityDesignerViewModel.ModelXRef.GetExisting(modelAssociation) as Association;
                        if (viewAssociation != null
                            && viewAssociation.IsDeleted == false)
                        {
                            yield return viewAssociation;
                        }
                    }
                }
            }
        }

        protected override bool CanMerge(ProtoElementBase rootElement, ElementGroupPrototype elementGroupPrototype)
        {
            if (rootElement != null
                && rootElement.ElementId == Guid.Empty)
            {
                var rootElementDomainInfo = Partition.DomainDataDirectory.GetDomainClass(rootElement.DomainClassId);

                if (rootElementDomainInfo.IsDerivedFrom(NavigationProperty.DomainClassId))
                {
                    return false;
                }
            }
            return base.CanMerge(rootElement, elementGroupPrototype);
        }

        protected override void OnDeleting()
        {
            base.OnDeleting();

            if (EntityDesignerViewModel != null
                && EntityDesignerViewModel.Reloading == false)
            {
                var tx = ModelUtils.GetCurrentTx(Store);
                Debug.Assert(tx != null);
                if (tx != null
                    && !tx.IsSerializing)
                {
                    ViewModelChangeContext.GetNewOrExistingContext(tx).ViewModelChanges.Add(new EntityTypeDelete(this));
                }
            }
        }
    }
}
