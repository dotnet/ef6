// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;

    internal class EdmModelParentMap
    {
        private readonly EdmModel model;

        private readonly Dictionary<EdmType, EdmNamespace> itemToNamespaceMap =
            new Dictionary<EdmType, EdmNamespace>();

        private readonly Dictionary<EntitySetBase, EntityContainer> itemToContainerMap =
            new Dictionary<EntitySetBase, EntityContainer>();

        internal EdmModelParentMap(EdmModel edmModel)
        {
            model = edmModel;
        }

        internal void Compute()
        {
            itemToNamespaceMap.Clear();
            if (model.Namespaces.Any())
            {
                foreach (var modelNamespace in model.Namespaces)
                {
                    foreach (var item in modelNamespace.NamespaceItems)
                    {
                        if (item != null)
                        {
                            itemToNamespaceMap[item] = modelNamespace;
                        }
                    }
                }
            }

            itemToContainerMap.Clear();
            if (model.Containers.Any())
            {
                foreach (var modelContainer in model.Containers)
                {
                    foreach (var item in modelContainer.BaseEntitySets)
                    {
                        if (item != null)
                        {
                            itemToContainerMap[item] = modelContainer;
                        }
                    }
                }
            }
        }

        internal IEnumerable<EdmType> NamespaceItems
        {
            get { return itemToNamespaceMap.Keys; }
        }

        internal bool TryGetEntityContainer(EntitySetBase item, out EntityContainer container)
        {
            if (item != null)
            {
                return itemToContainerMap.TryGetValue(item, out container);
            }
            container = null;
            return false;
        }

        internal bool TryGetNamespace(EdmType item, out EdmNamespace itemNamespace)
        {
            if (item != null)
            {
                return itemToNamespaceMap.TryGetValue(item, out itemNamespace);
            }
            itemNamespace = null;
            return false;
        }
    }
}
