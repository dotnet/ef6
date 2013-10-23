// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     Deletes any Storage EntitySets which are not mapped to
    ///     anything in the MSL. Then deletes any Storage EntityTypes
    ///     which are no longer referenced by the EntitySets
    /// </summary>
    internal class DeleteUnmappedStorageEntitySetsCommand : Command
    {
        private readonly ICollection<StorageEntitySet> _entitySets;

        internal DeleteUnmappedStorageEntitySetsCommand(ICollection<StorageEntitySet> entitySets)
        {
            _entitySets = entitySets;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(null != _entitySets, "Null entitySets in DeleteUnmappedStorageEntitySetsCommand.InvokeInternal()");
            if (null == _entitySets)
            {
                return;
            }

            // loop over the unmapped StorageEntitySets deleting them, also create
            // a list of any StorageEntityTypes referenced by these StorageEntitySets
            // Note: have to convert to array first to prevent exceptions due to
            // editing the collection while iterating over it
            var entitySets = _entitySets.ToArray();
            var entityTypesList = new List<StorageEntityType>();
            foreach (var entitySet in entitySets)
            {
                // find any EntityType which is referenced by this EntitySet
                if (null != entitySet.EntityType
                    && null != entitySet.EntityType.Target)
                {
                    var et = entitySet.EntityType.Target as StorageEntityType;
                    if (null != et)
                    {
                        entityTypesList.Add(et);
                    }
                }

                DeleteEFElementCommand.DeleteInTransaction(cpc, entitySet);
            }

            // delete all StorageEntityTypes found above
            var entityTypes = entityTypesList.ToArray();
            foreach (var entityType in entityTypes)
            {
                DeleteEFElementCommand.DeleteInTransaction(cpc, entityType);
            }
        }

        /// <summary>
        ///     constructs the list of StorageEntitySet objects that will be unmapped if the
        ///     conceptual EntityTypes are deleted (including all StorageEntitySet objects
        ///     which were already unmapped)
        /// </summary>
        /// <param name="elementsToBeDeleted">collection of conceptual EFElements being considered for deletion</param>
        /// <returns>
        ///     the list of StorageEntitySet objects that will be unmapped if the
        ///     conceptual EntityType is deleted
        /// </returns>
        internal static ICollection<StorageEntitySet> UnmappedStorageEntitySetsIfDelete(ICollection<EFElement> elementsToBeDeleted)
        {
            var unmappedStorageEntitySets = new HashSet<StorageEntitySet>();
            if (null == elementsToBeDeleted)
            {
                Debug.Fail("elementsToBeDeleted should not be null");
                return unmappedStorageEntitySets;
            }

            var allCSideObjectsThatWouldBeDeleted = new HashSet<EFObject>();
            var storageEntitySetsAtRiskOfDeletion = new HashSet<StorageEntitySet>();
            foreach (var elementToBeDeleted in elementsToBeDeleted)
            {
                // construct the list of objects on the C-side that would be deleted if cetToBeDeleted was deleted
                // and add them to the set of all C-side objects that would be deleted
                var cSideObjectsThatWouldBeDeleted = ConceptualObjectsToBeDeleted(elementToBeDeleted);
                allCSideObjectsThatWouldBeDeleted.UnionWith(cSideObjectsThatWouldBeDeleted);

                // find set of StorageEntitySets mapped to those C-side objects
                storageEntitySetsAtRiskOfDeletion.UnionWith(
                    FindMappedStorageEntitySets(cSideObjectsThatWouldBeDeleted));
            }

            // loop over the StorageEntitySets in storageEntitySetsAtRiskOfDeletion,
            // these EntitySets will be in the unmappedStorageEntitySets set unless they are _also_
            // mapped to some C-side object not in cSideObjectsThatWouldBeDeleted above
            foreach (var ses in storageEntitySetsAtRiskOfDeletion)
            {
                // find list of C-side EntityTypes, EntitySets, Associations and AssociationSets
                // which are mapped to the StorageEntitySet
                var cSideObjectsMappedToStorageEntitySetAtRiskOfDeletion =
                    FindMappedConceptualObjects(ses);

                // if all these C-side objects would be deleted then the StorageEntitySet
                // could also be safely deleted and so is added to unmappedStorageEntitySets
                var allCSideObjectsWouldBeDeleted = true;
                foreach (var cSideObject in cSideObjectsMappedToStorageEntitySetAtRiskOfDeletion)
                {
                    if (!allCSideObjectsThatWouldBeDeleted.Contains(cSideObject))
                    {
                        allCSideObjectsWouldBeDeleted = false;
                        break;
                    }
                }

                if (allCSideObjectsWouldBeDeleted)
                {
                    unmappedStorageEntitySets.Add(ses);
                }
            }

            return unmappedStorageEntitySets;
        }

        // construct the list of objects on the C-side that would be deleted if 
        // elementToBeDeleted were to be deleted (C-side EntityTypes and Associations only)
        private static IEnumerable<EFObject> ConceptualObjectsToBeDeleted(EFElement elementToBeDeleted)
        {
            var cSideObjectsThatWouldBeDeleted = new HashSet<EFObject>();

            var cetToBeDeleted = elementToBeDeleted as ConceptualEntityType;
            var assocToBeDeleted = elementToBeDeleted as Association;

            // 
            if (null != cetToBeDeleted)
            {
                // add the C-side EntityType itself
                cSideObjectsThatWouldBeDeleted.Add(cetToBeDeleted);

                // add the C-side EntitySet that references cetToBeDeleted (if any)
                cSideObjectsThatWouldBeDeleted.UnionWith(cetToBeDeleted.GetAntiDependenciesOfType<ConceptualEntitySet>());

                // add any Associations referred to by cetToBeDeleted
                var assocsToBeDeleted = DeleteEntityTypeCommand.GetListOfAssociationsToBeDeleted(cetToBeDeleted);
                cSideObjectsThatWouldBeDeleted.UnionWith(assocsToBeDeleted);

                // add any AssociationSets referred to by the Associations
                foreach (var assoc in assocsToBeDeleted)
                {
                    cSideObjectsThatWouldBeDeleted.UnionWith(assoc.GetAntiDependenciesOfType<AssociationSet>());
                }
            }

            if (null != assocToBeDeleted
                && assocToBeDeleted.EntityModel.IsCSDL)
            {
                // add the C-side Association itself
                cSideObjectsThatWouldBeDeleted.Add(assocToBeDeleted);

                // add any AssociationSets referred to by the Association
                cSideObjectsThatWouldBeDeleted.UnionWith(assocToBeDeleted.GetAntiDependenciesOfType<AssociationSet>());
            }

            return cSideObjectsThatWouldBeDeleted;
        }

        // find the list of S-side EntitySets that are mapped to the given
        // C-side objects through MappingFragments and AssociationSetMappings
        private static HashSet<StorageEntitySet> FindMappedStorageEntitySets(
            IEnumerable<EFObject> cSideObjectsThatWouldBeDeleted)
        {
            var storageEntitySetsAtRiskOfDeletion = new HashSet<StorageEntitySet>();
            if (null == cSideObjectsThatWouldBeDeleted)
            {
                Debug.Fail("cSideObjectsThatWouldBeDeleted must not be null");
                return storageEntitySetsAtRiskOfDeletion;
            }

            // find the mappings for the C-side EntityType & Associations & AssociationSets
            var mappingFragments = new HashSet<MappingFragment>();
            var assocSetMappings = new HashSet<AssociationSetMapping>();
            foreach (var cSideObject in cSideObjectsThatWouldBeDeleted)
            {
                var cet = cSideObject as ConceptualEntityType;
                var assoc = cSideObject as Association;
                var assocSet = cSideObject as AssociationSet;
                if (null != cet)
                {
                    foreach (var entityTypeMapping in cet.GetAntiDependenciesOfType<EntityTypeMapping>())
                    {
                        mappingFragments.UnionWith(entityTypeMapping.MappingFragments());
                    }
                }
                else if (null != assoc)
                {
                    assocSetMappings.UnionWith(assoc.GetAntiDependenciesOfType<AssociationSetMapping>());
                }
                else if (null != assocSet)
                {
                    assocSetMappings.UnionWith(assocSet.GetAntiDependenciesOfType<AssociationSetMapping>());
                }
            }

            // now loop over the MappingFragments & AssociationSetMappings and add
            // the StorageEntitySets to the return set
            foreach (var mf in mappingFragments)
            {
                if (null != mf.StoreEntitySet
                    && null != mf.StoreEntitySet.Target)
                {
                    var ses = mf.StoreEntitySet.Target as StorageEntitySet;
                    if (null != ses)
                    {
                        storageEntitySetsAtRiskOfDeletion.Add(ses);
                    }
                }
            }
            foreach (var asm in assocSetMappings)
            {
                if (null != asm.StoreEntitySet
                    && null != asm.StoreEntitySet.Target)
                {
                    var ses = asm.StoreEntitySet.Target as StorageEntitySet;
                    if (null != ses)
                    {
                        storageEntitySetsAtRiskOfDeletion.Add(ses);
                    }
                }
            }

            return storageEntitySetsAtRiskOfDeletion;
        }

        // finds the list of C-side objects mapped through MappingFragments or AssociationSetMappings
        // to the passed in StorageEntitySet
        private static HashSet<EFObject> FindMappedConceptualObjects(StorageEntitySet storageEntitySet)
        {
            var mappedConceptualObjects = new HashSet<EFObject>();
            if (null == storageEntitySet)
            {
                Debug.Fail("storageEntitySet must not be null");
                return mappedConceptualObjects;
            }

            // for each MappingFragment find the EntityTypeMapping parent and
            // add each EntityType referenced. Also add any EntitySet which has an
            // anti-dependency on that EntityType.
            foreach (var mf in storageEntitySet.GetAntiDependenciesOfType<MappingFragment>())
            {
                var etm = mf.EntityTypeMapping;
                if (null != etm
                    && null != etm.TypeName
                    && null != etm.TypeName.Bindings)
                {
                    foreach (var binding in etm.TypeName.Bindings)
                    {
                        var et = binding.Target as ConceptualEntityType;
                        if (null != et)
                        {
                            mappedConceptualObjects.Add(et);
                            foreach (var es in et.GetAntiDependenciesOfType<ConceptualEntitySet>())
                            {
                                mappedConceptualObjects.Add(es);
                            }
                        }
                    }
                }
            }

            // for each AssociationSetMapping add any referenced C-side AssociationSet and 
            // also any referenced C-side Association
            foreach (var asm in storageEntitySet.GetAntiDependenciesOfType<AssociationSetMapping>())
            {
                if (null != asm.Name
                    && null != asm.Name.Target)
                {
                    var assocSet = asm.Name.Target;
                    mappedConceptualObjects.Add(assocSet);
                    if (null != assocSet.Association
                        && null != assocSet.Association.Target)
                    {
                        mappedConceptualObjects.Add(assocSet.Association.Target);
                    }
                }

                if (null != asm.TypeName
                    && null != asm.TypeName.Target)
                {
                    mappedConceptualObjects.Add(asm.TypeName.Target);
                }
            }

            return mappedConceptualObjects;
        }
    }
}
