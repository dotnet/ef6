// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class EFSParameterDescriptor : EFAnnotatableElementDescriptor<Parameter>
    {
        internal override bool IsReadOnlyName()
        {
            return true;
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_Type")]
        public string Type
        {
            get { return TypedEFElement.Type.Value; }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_Mode")]
        public string Mode
        {
            get { return TypedEFElement.Mode.Value; }
        }

        public override string GetComponentName()
        {
            return TypedEFElement.NormalizedNameExternal;
        }

        public override string GetClassName()
        {
            return "Parameter";
        }
    }
}
