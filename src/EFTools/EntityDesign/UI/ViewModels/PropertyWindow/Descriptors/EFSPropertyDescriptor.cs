// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class EFSPropertyDescriptor : EFPropertyDescriptorBase<StorageProperty>
    {
        private static bool IsReadOnly
        {
            get { return true; }
        }

        internal override bool IsReadOnlyName()
        {
            return IsReadOnly;
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_EntityKey")]
        public bool EntityKey
        {
            get { return TypedEFElement.IsKeyProperty; }
        }

        internal static bool IsReadOnlyEntityKey()
        {
            return IsReadOnly;
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_Type")]
        public string Type
        {
            get { return TypedEFElement.Type.Value; }
        }

        internal static bool IsReadOnlyType()
        {
            return IsReadOnly;
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_StoreGeneratedPattern")]
        public string StoreGeneratedPattern
        {
            get { return TypedEFElement.StoreGeneratedPattern.Value; }
        }

        internal static bool IsReadOnlyStoreGeneratedPattern()
        {
            return IsReadOnly;
        }

        internal override bool IsReadOnlyNullable()
        {
            return IsReadOnly;
        }

        internal override bool IsBrowsableMaxLength()
        {
            if (TypedEFElement == null
                || TypedEFElement.EntityType == null
                || TypedEFElement.EntityType.EntityModel == null
                || TypedEFElement.Type == null
                || TypedEFElement.Type.Value == null)
            {
                return false;
            }
            else
            {
                return ModelHelper.IsValidStorageFacet(
                    (StorageEntityModel)TypedEFElement.EntityType.EntityModel, TypedEFElement.Type.Value, Property.AttributeMaxLength);
            }
        }

        internal override bool IsReadOnlyMaxLength()
        {
            return IsReadOnly;
        }

        internal override bool IsBrowsableFixedLength()
        {
            if (TypedEFElement == null
                || TypedEFElement.EntityType == null
                || TypedEFElement.EntityType.EntityModel == null
                || TypedEFElement.Type == null
                || TypedEFElement.Type.Value == null)
            {
                return false;
            }
            else
            {
                return ModelHelper.IsValidStorageFacet(
                    (StorageEntityModel)TypedEFElement.EntityType.EntityModel, TypedEFElement.Type.Value, Property.AttributeFixedLength);
            }
        }

        internal override bool IsReadOnlyFixedLength()
        {
            return IsReadOnly;
        }

        internal override bool IsBrowsablePrecision()
        {
            if (TypedEFElement == null
                || TypedEFElement.EntityType == null
                || TypedEFElement.EntityType.EntityModel == null
                || TypedEFElement.Type == null
                || TypedEFElement.Type.Value == null)
            {
                return false;
            }
            else
            {
                return ModelHelper.IsValidStorageFacet(
                    (StorageEntityModel)TypedEFElement.EntityType.EntityModel, TypedEFElement.Type.Value, Property.AttributePrecision);
            }
        }

        internal override bool IsReadOnlyPrecision()
        {
            return IsReadOnly;
        }

        internal override bool IsBrowsableScale()
        {
            if (TypedEFElement == null
                || TypedEFElement.EntityType == null
                || TypedEFElement.EntityType.EntityModel == null
                || TypedEFElement.Type == null
                || TypedEFElement.Type.Value == null)
            {
                return false;
            }
            else
            {
                return ModelHelper.IsValidStorageFacet(
                    (StorageEntityModel)TypedEFElement.EntityType.EntityModel, TypedEFElement.Type.Value, Property.AttributeScale);
            }
        }

        internal override bool IsReadOnlyScale()
        {
            return IsReadOnly;
        }

        internal override bool IsBrowsableUnicode()
        {
            if (TypedEFElement == null
                || TypedEFElement.EntityType == null
                || TypedEFElement.EntityType.EntityModel == null
                || TypedEFElement.Type == null
                || TypedEFElement.Type.Value == null)
            {
                return false;
            }
            else
            {
                return ModelHelper.IsValidStorageFacet(
                    (StorageEntityModel)TypedEFElement.EntityType.EntityModel, TypedEFElement.Type.Value, Property.AttributeUnicode);
            }
        }

        internal override bool IsReadOnlyUnicode()
        {
            return IsReadOnly;
        }

        internal override bool IsBrowsableCollation()
        {
            if (TypedEFElement == null
                || TypedEFElement.EntityType == null
                || TypedEFElement.EntityType.EntityModel == null
                || TypedEFElement.Type == null
                || TypedEFElement.Type.Value == null)
            {
                return false;
            }
            else
            {
                return ModelHelper.IsValidStorageFacet(
                    (StorageEntityModel)TypedEFElement.EntityType.EntityModel, TypedEFElement.Type.Value, Property.AttributeCollation);
            }
        }

        internal override bool IsReadOnlyCollation()
        {
            return IsReadOnly;
        }

        public override string GetComponentName()
        {
            return TypedEFElement.NormalizedNameExternal;
        }

        public override string GetClassName()
        {
            return "Property";
        }
    }
}
