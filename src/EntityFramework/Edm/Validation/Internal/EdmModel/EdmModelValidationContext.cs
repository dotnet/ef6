// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation.Internal.EdmModel
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.Edm.Internal;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

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
    }
}
