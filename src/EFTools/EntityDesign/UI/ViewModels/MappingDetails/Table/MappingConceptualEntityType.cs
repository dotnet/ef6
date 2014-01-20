// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Tables
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns;

    // <summary>
    //     This class represents the root node of the view model when we are mapping entities, it points to
    //     a c-side entity and has a list of 'tables' that the entity is mapped to.
    //     + MappingConceptualEntityType             [will resolve to 2 EntityTypeMappings]
    //     |
    //     + MappingStorageEntityType (0..*)       [will resolve to 2 MappingFragments (one in each ETM)]
    //     |
    //     + MappingCondition (0..*)             [will resolve to a Condition in either the Default ETM or the IsTypeOf ETM]
    //     + MappingColumnMappings (1..1)        [doesn't map to the model, just an extra container node]
    //     |
    //     + MappingScalarProperty (0..*)      [will resolve to a ScalarProperty in the Default ETM]
    //     The reason why this view model is tying itself to the C- &amp; S-side entity types
    //     (instead of EntityTypeMappings or MappingFragments) is because we need to be able to
    //     keep track of more than one of these per entity.
    // </summary>
    [TreeGridDesignerRootBranch(typeof(EntityTypeBranch))]
    [TreeGridDesignerColumn(typeof(ColumnNameColumn), Order = 1)]
    [TreeGridDesignerColumn(typeof(OperatorColumn), Order = 2)]
    [TreeGridDesignerColumn(typeof(ValueColumn), Order = 3)]
    internal class MappingConceptualEntityType : MappingEntityMappingRoot
    {
        private IList<MappingStorageEntityType> _storageEntityTypes;

        public MappingConceptualEntityType(EditingContext context, EntityType entityType, MappingEFElement parent)
            : base(context, entityType, parent)
        {
        }

        internal ConceptualEntityType ConceptualEntityType
        {
            get { return ModelItem as ConceptualEntityType; }
        }

        internal IList<MappingStorageEntityType> StorageEntityTypes
        {
            get
            {
                _storageEntityTypes = new List<MappingStorageEntityType>();

                if (ConceptualEntityType != null)
                {
                    // a given entity type will have multiple entity type mappings (one default, one IsTypeOf)
                    // and therefore will have a mapping fragment for each of these; this array is here
                    // so that we can built a unique list of tables across all mapping fragments
                    var tables = new List<EntityType>();

                    // loop through every EntityTypeMapping that has a dep on this c-side entity
                    // and then loop through every MappingFragment, building a de-duped list of 
                    // s-side entities (tables)
                    foreach (var etm in ConceptualEntityType.GetAntiDependenciesOfType<EntityTypeMapping>())
                    {
                        foreach (var frag in etm.MappingFragments())
                        {
                            Debug.Assert(
                                frag.StoreEntitySet.Status == BindingStatus.Known,
                                "inconsistent EntitySet binding status " + frag.StoreEntitySet.Status + ", should be " + BindingStatus.Known);

                            if (frag.StoreEntitySet.Status == BindingStatus.Known)
                            {
                                var ses = frag.StoreEntitySet.Target as StorageEntitySet;
                                Debug.Assert(
                                    ses.EntityType.Status == BindingStatus.Known,
                                    "inconsistent EntityType binding status " + ses.EntityType.Status + ", should be " + BindingStatus.Known);

                                if (ses.EntityType.Status == BindingStatus.Known)
                                {
                                    var table = ses.EntityType.Target as StorageEntityType;
                                    Debug.Assert(
                                        ses.EntityType.Target != null ? table != null : true, "EntityType is not StorageEntityType");
                                    Debug.Assert(table != null, "table should not be null");

                                    if (!tables.Contains(table))
                                    {
                                        tables.Add(table);
                                    }
                                }
                            }
                        }
                    }

                    // now that we have our list of unique 'tables', create view model entries for each
                    foreach (var table in tables)
                    {
                        var mset = (MappingStorageEntityType)ModelToMappingModelXRef.GetNewOrExisting(_context, table, this);
                        _storageEntityTypes.Add(mset);
                    }
                }

                return _storageEntityTypes;
            }
        }

        protected override void LoadChildrenCollection()
        {
            foreach (var child in StorageEntityTypes)
            {
                _children.Add(child);
            }
        }

        protected override void OnChildDeleted(MappingEFElement melem)
        {
            var child = melem as MappingStorageEntityType;
            Debug.Assert(child != null, "Unknown child being deleted");
            if (child != null)
            {
                _children.Remove(child);
                return;
            }

            base.OnChildDeleted(melem);
        }
    }
}
