// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    internal sealed class EdmModelValidationContext
    {
        public event EventHandler<DataModelErrorEventArgs> OnError;

        public EdmModelValidationContext(bool validateSyntax)
        {
            ValidateSyntax = validateSyntax;
        }

        public bool ValidateSyntax { get; set; }
        public double ValidationContextVersion { get; set; }

        public EdmModelParentMap ModelParentMap { get; private set; }

        public string GetQualifiedPrefix(EdmType item)
        {
            Debug.Assert(ModelParentMap != null);

            string qualifiedPrefix = null;
            EdmNamespace parentNamespace;
            if (ModelParentMap.TryGetNamespace(item, out parentNamespace))
            {
                qualifiedPrefix = parentNamespace.Name;
            }

            return qualifiedPrefix;
        }

        public string GetQualifiedPrefix(EntitySetBase item)
        {
            Debug.Assert(ModelParentMap != null);

            string qualifiedPrefix = null;
            EntityContainer parentContainer;

            if (ModelParentMap.TryGetEntityContainer(item, out parentContainer))
            {
                qualifiedPrefix = parentContainer.Name;
            }

            return qualifiedPrefix;
        }

        public void RaiseDataModelValidationEvent(DataModelErrorEventArgs error)
        {
            if (OnError != null)
            {
                OnError(this, error);
            }
        }

        public void Validate(EdmModel root)
        {
            DebugCheck.NotNull(root);

            ModelParentMap = new EdmModelParentMap(root);
            ModelParentMap.Compute();

            ValidationContextVersion = root.Version;

            EdmModelValidator.Validate(root, this);
        }

        public void AddError(IMetadataItem item, string propertyName, string errorMessage)
        {
            RaiseDataModelValidationEvent(
                new DataModelErrorEventArgs
                    {
                        ErrorMessage = errorMessage,
                        Item = item,
                        PropertyName = propertyName,
                    }
                );
        }

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
}
