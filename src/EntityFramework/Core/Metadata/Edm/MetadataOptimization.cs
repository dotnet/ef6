// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Collections.Concurrent;

    internal class MetadataOptimization
    {
        private readonly MetadataWorkspace _workspace;

        private readonly IDictionary<Type, EntitySetTypePair> _entitySetMappingsCache
            = new Dictionary<Type, EntitySetTypePair>();

        private volatile AssociationType[] _csAssociationTypes;
        private volatile AssociationType[] _osAssociationTypes;
        private volatile object[] _csAssociationTypeToSets;

        internal MetadataOptimization(MetadataWorkspace workspace)
        {
            DebugCheck.NotNull(workspace);

            _workspace = workspace;
        }

        #region Entity Set Mappings

        // <summary>
        // Returns a dictionary that serves as a cache of the mapping between clr types and entity sets 
        // used by the internal context.
        // The cache's lifecycle is the same as the metadata workspace's lifecycle.
        // </summary>
        // <returns>The entity set mapping dictionary owned by the provided context type</returns>
        internal IDictionary<Type, EntitySetTypePair> GetEntitySetMappingCache()
        {
            return _entitySetMappingsCache;
        }

        // <summary>
        // Updates the cache of types to entity sets either for the first time or after potentially
        // doing some o-space loading.
        // </summary>
        // <param name="entitySetMappings">The dictionary where the mappings will be updated</param>
        internal void UpdateEntitySetMappings(IDictionary<Type, EntitySetTypePair> entitySetMappings)
        {
            var objectItemCollection = (ObjectItemCollection)_workspace.GetItemCollection(DataSpace.OSpace);
            var ospaceTypes = _workspace.GetItems<EntityType>(DataSpace.OSpace);
            var inverseHierarchy = new Stack<EntityType>();

            foreach (var ospaceType in ospaceTypes)
            {
                inverseHierarchy.Clear();
                var cspaceType = (EntityType)_workspace.GetEdmSpaceType(ospaceType);
                do
                {
                    inverseHierarchy.Push(cspaceType);
                    cspaceType = (EntityType)cspaceType.BaseType;
                }
                while (cspaceType != null);

                EntitySet entitySet = null;
                while (entitySet == null
                       && inverseHierarchy.Count > 0)
                {
                    cspaceType = inverseHierarchy.Pop();
                    foreach (var container in _workspace.GetItems<EntityContainer>(DataSpace.CSpace))
                    {
                        var entitySets = container.BaseEntitySets.Where(s => s.ElementType == cspaceType).ToList();
                        var entitySetsCount = entitySets.Count;
                        if (entitySetsCount > 1
                            || entitySetsCount == 1 && entitySet != null)
                        {
                            throw Error.DbContext_MESTNotSupported();
                        }
                        if (entitySetsCount == 1)
                        {
                            entitySet = (EntitySet)entitySets[0];
                        }
                    }
                }

                // Entity set may be null if the o-space type is a base type that is in the model but is
                // not part of any set.  For most practical purposes, this type is not in the model since
                // there is no way to query etc. for objects of this type.
                if (entitySet != null)
                {
                    var ospaceBaseType = (EntityType)_workspace.GetObjectSpaceType(cspaceType);
                    var clrType = objectItemCollection.GetClrType(ospaceType);
                    var clrBaseType = objectItemCollection.GetClrType(ospaceBaseType);
                    entitySetMappings[clrType] = new EntitySetTypePair(entitySet, clrBaseType);
                }
            }
        }

        #endregion

        #region CSpace

        internal AssociationType GetCSpaceAssociationType(AssociationType osAssociationType)
        {
            Debug.Assert(osAssociationType.Index >= 0);

            return _csAssociationTypes[osAssociationType.Index];
        }

        internal AssociationSet FindCSpaceAssociationSet(AssociationType associationType, string endName, EntitySet endEntitySet)
        {
            DebugCheck.NotNull(associationType);
            DebugCheck.NotEmpty(endName);
            DebugCheck.NotNull(endEntitySet);
            Debug.Assert(associationType.DataSpace == DataSpace.CSpace);

            var array = GetCSpaceAssociationTypeToSetsMap();
            var index = associationType.Index;

            var objectAtIndex = array[index];
            if (objectAtIndex == null)
            {
                return null;
            }

            var associationSet = objectAtIndex as AssociationSet;
            if (associationSet != null)
            {
                return associationSet.AssociationSetEnds[endName].EntitySet == endEntitySet ? associationSet : null;
            }

            var items = (AssociationSet[])objectAtIndex;
            for (var i = 0; i < items.Length; i++)
            {
                associationSet = items[i];
                if (associationSet.AssociationSetEnds[endName].EntitySet == endEntitySet)
                {
                    return associationSet;
                }
            }

            return null;
        }

        internal AssociationSet FindCSpaceAssociationSet(AssociationType associationType, string endName,
            string entitySetName, string entityContainerName, out EntitySet endEntitySet)
        {
            DebugCheck.NotNull(associationType);
            DebugCheck.NotEmpty(endName);
            DebugCheck.NotEmpty(entitySetName);
            DebugCheck.NotEmpty(entityContainerName);
            Debug.Assert(associationType.DataSpace == DataSpace.CSpace);

            var array = GetCSpaceAssociationTypeToSetsMap();
            var index = associationType.Index;

            var objectAtIndex = array[index];
            if (objectAtIndex == null)
            {
                endEntitySet = null;
                return null;
            }

            var associationSet = objectAtIndex as AssociationSet;
            if (associationSet != null)
            {
                var entitySet = associationSet.AssociationSetEnds[endName].EntitySet;
                if (entitySet.Name == entitySetName &&
                    entitySet.EntityContainer.Name == entityContainerName)
                {
                    endEntitySet = entitySet;
                    return associationSet;
                }

                endEntitySet = null;
                return null;
            }

            var items = (AssociationSet[])objectAtIndex;
            for (var i = 0; i < items.Length; i++)
            {
                associationSet = items[i];
                var entitySet = associationSet.AssociationSetEnds[endName].EntitySet;
                if (entitySet.Name == entitySetName && 
                    entitySet.EntityContainer.Name == entityContainerName)
                {
                    endEntitySet = entitySet;
                    return associationSet;
                }
            }

            endEntitySet = null;
            return null;
        }

        // Internal for testing only.
        internal AssociationType[] GetCSpaceAssociationTypes()
        {
            if (_csAssociationTypes == null)
            {
                _csAssociationTypes = IndexCSpaceAssociationTypes(_workspace.GetItemCollection(DataSpace.CSpace));
            }

            return _csAssociationTypes;
        }

        private static AssociationType[] IndexCSpaceAssociationTypes(ItemCollection itemCollection)
        {
            Debug.Assert(itemCollection.DataSpace == DataSpace.CSpace);
            Debug.Assert(itemCollection.IsReadOnly);

            var associationTypes = new List<AssociationType>();
            var count = 0;

            foreach (var associatonType in itemCollection.GetItems<AssociationType>())
            {
                associationTypes.Add(associatonType);
                associatonType.Index = count++;
            }

            return associationTypes.ToArray();
        }

        // Internal for testing only.
        internal object[] GetCSpaceAssociationTypeToSetsMap()
        {
            if (_csAssociationTypeToSets == null)
            {
                _csAssociationTypeToSets = MapCSpaceAssociationTypeToSets(
                    _workspace.GetItemCollection(DataSpace.CSpace), GetCSpaceAssociationTypes().Length);
            }

            return _csAssociationTypeToSets;
        }

        private static object[] MapCSpaceAssociationTypeToSets(ItemCollection itemCollection, int associationTypeCount)
        {
            Debug.Assert(itemCollection.DataSpace == DataSpace.CSpace);
            Debug.Assert(itemCollection.IsReadOnly);

            var associationTypeToSets = new object[associationTypeCount];

            foreach (var entityContainer in itemCollection.GetItems<EntityContainer>())
            {
                foreach (var baseEntitySet in entityContainer.BaseEntitySets)
                {
                    var associationSet = baseEntitySet as AssociationSet;
                    if (associationSet != null)
                    {
                        var j = associationSet.ElementType.Index;
                        Debug.Assert(j >= 0);

                        AddItemAtIndex(associationTypeToSets, j, associationSet);
                    }
                }
            }

            return associationTypeToSets;
        }

        #endregion

        #region OSpace

        internal AssociationType GetOSpaceAssociationType(
            AssociationType cSpaceAssociationType, Func<AssociationType> initializer)
        {
            Debug.Assert(cSpaceAssociationType.DataSpace == DataSpace.CSpace);

            var oSpaceAssociationTypes = GetOSpaceAssociationTypes();
            var index = cSpaceAssociationType.Index;

            Thread.MemoryBarrier(); 
            var oSpaceAssociationType = oSpaceAssociationTypes[index];

            if (oSpaceAssociationType == null)
            {
                oSpaceAssociationType = initializer();
                Debug.Assert(oSpaceAssociationType.DataSpace == DataSpace.OSpace);

                oSpaceAssociationType.Index = index;
                oSpaceAssociationTypes[index] = oSpaceAssociationType;
                Thread.MemoryBarrier(); 
            }

            return oSpaceAssociationType;
        }

        // Internal for testing only.
        internal AssociationType[] GetOSpaceAssociationTypes()
        {
            if (_osAssociationTypes == null)
            {
                _osAssociationTypes = new AssociationType[GetCSpaceAssociationTypes().Length];
            }

            return _osAssociationTypes;
        }

        #endregion

        #region Helper methods

        private static void AddItemAtIndex<T>(object[] array, int index, T newItem)
            where T : class
        {
            var objectAtIndex = array[index];
            if (objectAtIndex == null)
            {
                array[index] = newItem;
                return;
            }

            var item = objectAtIndex as T;
            if (item != null)
            {
                array[index] = new[] { item, newItem };
                return;
            }

            var items = (T[])objectAtIndex;
            var count = items.Length;
            Array.Resize(ref items, count + 1);
            items[count] = newItem;
            array[index] = items;
        }

        #endregion
    }
}
