// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class EFSEntityModelDescriptor : EFAnnotatableElementDescriptor<StorageEntityModel>
    {
        internal override bool IsReadOnlyName()
        {
            return true;
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_Alias")]
        public string Alias
        {
            get { return TypedEFElement.Alias.Value; }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_Namespace")]
        public string Namespace
        {
            get { return TypedEFElement.Namespace.Value; }
        }

        public override string GetComponentName()
        {
            return TypedEFElement.NormalizedNameExternal;
        }

        public override string GetClassName()
        {
            return "StorageEntityModel";
        }
    }
}
