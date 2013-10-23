// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Designer;

    // Represents multiple EntityTypes (and relationships) info stored in Clipboard
    [Serializable]
    internal class EntitiesClipboardFormat
    {
        private readonly List<EntityTypeClipboardFormat> _entities = new List<EntityTypeClipboardFormat>();
        private readonly List<AssociationClipboardFormat> _associations = new List<AssociationClipboardFormat>();
        // "Derived-type-to-base-type" map which contains all the inheritance relationships among entity-types in the clipboard.
        // This is to ensure that the inheritance relationships are copied over.
        private readonly Dictionary<EntityTypeClipboardFormat, EntityTypeClipboardFormat> _inheritances =
            new Dictionary<EntityTypeClipboardFormat, EntityTypeClipboardFormat>();

        internal EntitiesClipboardFormat(
            ICollection<EntityType> entities, ICollection<Association> associations, IDictionary<EntityType, EntityType> inheritances)
        {
            var entitiesMap = new Dictionary<EntityType, EntityTypeClipboardFormat>(entities.Count);
            foreach (var entity in entities)
            {
                // Check if the entity has been added. Copying a self association could cause the same entities to be added twice in the list.
                if (entitiesMap.ContainsKey(entity) == false)
                {
                    var clipboardEntity = new EntityTypeClipboardFormat(entity);
                    _entities.Add(clipboardEntity);
                    entitiesMap.Add(entity, clipboardEntity);
                }
            }
            InitializeAssociationAndInheritanceClipboardObjects(entitiesMap, associations, inheritances);
        }

        // Given the list of EntityTypeShapes, create the following in objects:
        // - List of the EntityClipboardFormat that represents the shape's entities and shape's property (for example: FillColor).
        // - List of the AssociationClipboardFormat that represent the associations among the shape's entities.
        // - List of the ClipboardFormat object that represent the inheritance among the shape's entities.
        internal EntitiesClipboardFormat(ICollection<EntityTypeShape> entityTypeShapes)
        {
            // Get a list of distinct entity-types from the entity-type-shape collection.
            var entitiesMap = new Dictionary<EntityType, EntityTypeClipboardFormat>(entityTypeShapes.Count);
            foreach (var ets in entityTypeShapes)
            {
                var et = ets.EntityType.Target;
                if (et == null
                    || entitiesMap.ContainsKey(et))
                {
                    continue;
                }

                var clipboardEntity = new EntityTypeClipboardFormat(ets);
                _entities.Add(clipboardEntity);
                entitiesMap.Add(et, clipboardEntity);
            }

            // Figure out the association and the inheritance between the selected entities.
            IList<EntityType> selectedEntityTypes = entitiesMap.Keys.ToList();
            var associations = new HashSet<Association>();
            var inheritances = new Dictionary<EntityType, EntityType>();

            foreach (var et in selectedEntityTypes)
            {
                // Add inheritance if both base and derived type is in the list.
                var cet = et as ConceptualEntityType;
                if (cet != null
                    && selectedEntityTypes.Contains(cet.BaseType.Target)
                    && inheritances.ContainsKey(cet) == false)
                {
                    inheritances.Add(cet, cet.BaseType.Target);
                }

                var participatingAssociations = Association.GetAssociationsForEntityType(et);
                foreach (var assoc in participatingAssociations)
                {
                    if (associations.Contains(assoc))
                    {
                        break;
                    }

                    var entityTypesInAssociation = assoc.AssociationEnds().Select(ae => ae.Type.Target).ToList();
                    // if both participating entity-types are in the list, add the association.
                    if (entityTypesInAssociation.Except(selectedEntityTypes).Count() == 0)
                    {
                        associations.Add(assoc);
                    }
                }
            }
            InitializeAssociationAndInheritanceClipboardObjects(entitiesMap, associations, inheritances);
        }

        internal List<EntityTypeClipboardFormat> ClipboardEntities
        {
            get { return _entities; }
        }

        internal List<AssociationClipboardFormat> ClipboardAssociations
        {
            get { return _associations; }
        }

        internal Dictionary<EntityTypeClipboardFormat, EntityTypeClipboardFormat> ClipboardInheritances
        {
            get { return _inheritances; }
        }

        private void InitializeAssociationAndInheritanceClipboardObjects(
            Dictionary<EntityType, EntityTypeClipboardFormat> entitiesMap, ICollection<Association> associations,
            IDictionary<EntityType, EntityType> inheritances)
        {
            foreach (var association in associations)
            {
                var associationEnds = association.AssociationEnds();
                if (associationEnds.Count == 2
                    && associationEnds[0].Type.Status == BindingStatus.Known
                    && associationEnds[1].Type.Status == BindingStatus.Known)
                {
                    // add contained associations only
                    EntityTypeClipboardFormat clipboardEntity1, clipboardEntity2;
                    if (entitiesMap.TryGetValue(associationEnds[0].Type.Target, out clipboardEntity1)
                        && entitiesMap.TryGetValue(associationEnds[1].Type.Target, out clipboardEntity2))
                    {
                        _associations.Add(new AssociationClipboardFormat(association, clipboardEntity1, clipboardEntity2));
                    }
                }
            }

            foreach (var inheritance in inheritances)
            {
                // add contained inheritances only
                EntityTypeClipboardFormat clipboardEntity1, clipboardEntity2;
                if (entitiesMap.TryGetValue(inheritance.Key, out clipboardEntity1)
                    && entitiesMap.TryGetValue(inheritance.Value, out clipboardEntity2))
                {
                    _inheritances.Add(clipboardEntity1, clipboardEntity2);
                }
            }
        }
    }
}
