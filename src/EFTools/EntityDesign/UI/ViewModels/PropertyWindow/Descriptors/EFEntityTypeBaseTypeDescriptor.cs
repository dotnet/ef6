// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System.ComponentModel;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class EFEntityTypeBaseTypeDescriptor : AttributeDescriptor<EntityTypeBaseType>
    {
        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_BaseType")]
        [LocDescription("PropertyWindow_Description_InheritanceBaseType")]
        [ReadOnly(true)]
        [MergableProperty(false)]
        public string BaseType
        {
            get
            {
                EntityType baseType = TypedEFAttribute.Target;
                Debug.Assert(baseType != null, "baseType should not be null");
                return baseType.LocalName.Value;
            }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_DerivedType")]
        [LocDescription("PropertyWindow_Description_InheritanceDerivedType")]
        [ReadOnly(true)]
        [MergableProperty(false)]
        public string DerivedType
        {
            get { return TypedEFAttribute.OwnerEntityType.LocalName.Value; }
        }

        public override string GetComponentName()
        {
            return "Base of " + TypedEFAttribute.NormalizedName();
        }

        public override string GetClassName()
        {
            return "Inheritance";
        }

        public override object GetDescriptorDefaultValue(string propertyDescriptorMethodName)
        {
            return null;
        }
    }
}
