// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;

    // ExplorerConceptualEntityType must be distinguished from 
    // ExplorerStorageEntityType in order to allow the XAML
    // to load different images for them
    internal class ExplorerConceptualEntityType : ExplorerEntityType
    {
        public ExplorerConceptualEntityType(EditingContext context, EntityType entityType, ExplorerEFElement parent)
            : base(context, entityType, parent)
        {
            // do nothing
        }

        internal override string ExplorerImageResourceKeyName
        {
            get { return "EntityTypePngIcon"; }
        }

        // the name of Conceptual Entity Types are editable inline in the Explorer
        public override bool IsEditableInline
        {
            get { return true; }
        }

        internal override void OnModelPropertyChanged(string modelPropName)
        {
            base.OnModelPropertyChanged(modelPropName);

            var xref = ModelToExplorerModelXRef.GetModelToBrowserModelXRef(_context);

            if (modelPropName == EFNameableItem.AttributeName)
            {
                // This code below makes sure that if ExplorerConceptualEntityType's name is changed we need to ensure the corresponding ExplorerEntityTypeShape's name is also updated.
                // TODO: review the code below see if we can create a more generic code in ExplorerViewModelHelper's ProcessModelChangesCommitted.
                var entityType = ModelItem as EntityType;
                foreach (var ets in entityType.GetAntiDependenciesOfType<EntityTypeShape>())
                {
                    var exploreEFElement = xref.GetExisting(ets);
                    if (exploreEFElement != null)
                    {
                        exploreEFElement.OnModelPropertyChanged(modelPropName);
                    }
                }
            }
        }
    }
}
