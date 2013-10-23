// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.UpdateFromDatabase
{
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal delegate void AddCSideEntityTypeToEntityTypeIdentityMapping(EntityType et, DatabaseObject dbObj);

    internal static class UpdateModelFromDatabaseUtils
    {
        internal static void ConstructEntityMappings(
            EntityContainerMapping ecm,
            AddCSideEntityTypeToEntityTypeIdentityMapping addMappingDelegate)
        {
            foreach (var esm in ecm.EntitySetMappings())
            {
                foreach (var etm in esm.EntityTypeMappings())
                {
                    foreach (var mf in etm.MappingFragments())
                    {
                        var sSideEntitySet = mf.StoreEntitySet.Target as StorageEntitySet;
                        if (null != sSideEntitySet)
                        {
                            var dbObj = DatabaseObject.CreateFromEntitySet(sSideEntitySet);

                            // record mappings to C-side entity types
                            foreach (var entry in etm.TypeName.Entries)
                            {
                                if (null != entry.EntityType)
                                {
                                    // now that we've identified EntityType/DatabaseObject pair 
                                    // call the delegate that was passed in to actually do the mapping
                                    addMappingDelegate(entry.EntityType, dbObj);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
