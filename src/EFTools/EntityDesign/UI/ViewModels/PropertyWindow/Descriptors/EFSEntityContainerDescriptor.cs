// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class EFSEntityContainerDescriptor : EFAnnotatableElementDescriptor<StorageEntityContainer>
    {
        internal override bool IsReadOnlyName()
        {
            return true;
        }

        public override string GetComponentName()
        {
            return TypedEFElement.NormalizedNameExternal;
        }

        public override string GetClassName()
        {
            return "StorageEntityContainer";
        }
    }
}
