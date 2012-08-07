// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Internal
{
    using System.Collections.Generic;

    internal class EdmModelParentMap
    {
        private readonly EdmModel model;

        private readonly Dictionary<EdmNamespaceItem, EdmNamespace> itemToNamespaceMap =
            new Dictionary<EdmNamespaceItem, EdmNamespace>();

        private readonly Dictionary<EdmEntityContainerItem, EdmEntityContainer> itemToContainerMap =
            new Dictionary<EdmEntityContainerItem, EdmEntityContainer>();

        internal EdmModelParentMap(EdmModel edmModel)
        {
            model = edmModel;
        }

        internal void Compute()
        {
            itemToNamespaceMap.Clear();
            if (model.HasNamespaces)
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
            if (model.HasContainers)
            {
                foreach (var modelContainer in model.Containers)
                {
                    foreach (var item in modelContainer.ContainerItems)
                    {
                        if (item != null)
                        {
                            itemToContainerMap[item] = modelContainer;
                        }
                    }
                }
            }
        }

        internal IEnumerable<EdmNamespaceItem> NamespaceItems
        {
            get { return itemToNamespaceMap.Keys; }
        }

        internal bool TryGetEntityContainer(EdmEntityContainerItem item, out EdmEntityContainer container)
        {
            if (item != null)
            {
                return itemToContainerMap.TryGetValue(item, out container);
            }
            container = null;
            return false;
        }

        internal bool TryGetNamespace(EdmNamespaceItem item, out EdmNamespace itemNamespace)
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
