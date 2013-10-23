// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System.ComponentModel;
    using Microsoft.Data.Entity.Design.Core.Controls;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters;

    internal class EFPropertyDescriptor : EFPropertyDescriptorBase<ConceptualProperty>
    {
        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_EntityKey")]
        [LocDescription("PropertyWindow_Description_EntityKey")]
        [TypeConverter(typeof(BoolConverter))]
        public bool EntityKey
        {
            get { return TypedEFElement.IsKeyProperty; }
            set
            {
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                var setKey = new SetKeyPropertyCommand(TypedEFElement, value);
                CommandProcessor.InvokeSingleCommand(cpc, setKey);
            }
        }

        public bool IsBrowsableEntityKey()
        {
            return TypedEFElement.IsEntityTypeProperty;
        }

        public bool IsReadOnlyEntityKey()
        {
            // Disallow the user to set PK on a sub type.
            var isReadOnly = false;
            if (TypedEFElement.EntityType != null)
            {
                var cet = TypedEFElement.EntityType as ConceptualEntityType;
                if (cet != null)
                {
                    isReadOnly = cet.HasResolvableBaseType && !TypedEFElement.IsKeyProperty;
                }
                else
                {
                    isReadOnly = !TypedEFElement.IsKeyProperty;
                }
            }
            return isReadOnly;
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_Type")]
        [LocDescription("PropertyWindow_Description_Type")]
        [TypeConverter(typeof(ConceptualPropertyTypeConverter))]
        public string Type
        {
            get { return TypedEFElement.TypeName; }
            set
            {
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                var changeType = new ChangePropertyTypeCommand(TypedEFElement, value);
                CommandProcessor.InvokeSingleCommand(cpc, changeType);
            }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_StoreGeneratedPattern")]
        [LocDescription("PropertyWindow_Description_StoreGeneratedPattern")]
        [TypeConverter(typeof(StoreGeneratedPatternConverter))]
        public string StoreGeneratedPattern
        {
            get { return TypedEFElement.StoreGeneratedPattern.Value; }
            set
            {
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                var cmd = new SetStoreGeneratedPatternCommand(TypedEFElement, value);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
            }
        }

        internal override bool IsBrowsableGetter()
        {
            return true;
        }

        internal override bool IsBrowsableSetter()
        {
            return true;
        }

        internal override bool IsBrowsableNullable()
        {
            return true;
        }

        internal override bool IsBrowsableDefault()
        {
            return true;
        }

        internal override bool IsBrowsableConcurrencyMode()
        {
            return true;
        }

        // facets
        internal override bool IsBrowsableMaxLength()
        {
            return ModelHelper.IsValidModelFacet(TypedEFElement.PrimitiveTypeName, Property.AttributeMaxLength);
        }

        internal override bool IsBrowsableFixedLength()
        {
            return ModelHelper.IsValidModelFacet(TypedEFElement.PrimitiveTypeName, Property.AttributeFixedLength);
        }

        internal override bool IsBrowsablePrecision()
        {
            return ModelHelper.IsValidModelFacet(TypedEFElement.PrimitiveTypeName, Property.AttributePrecision);
        }

        internal override bool IsBrowsableScale()
        {
            return ModelHelper.IsValidModelFacet(TypedEFElement.PrimitiveTypeName, Property.AttributeScale);
        }

        internal override bool IsBrowsableUnicode()
        {
            return ModelHelper.IsValidModelFacet(TypedEFElement.PrimitiveTypeName, Property.AttributeUnicode);
        }

        internal override bool IsBrowsableCollation()
        {
            return ModelHelper.IsValidModelFacet(TypedEFElement.PrimitiveTypeName, Property.AttributeCollation);
        }

        public override object GetDescriptorDefaultValue(string propertyDescriptorMethodName)
        {
            if (propertyDescriptorMethodName.Equals("EntityKey"))
            {
                return false;
            }
            else if (propertyDescriptorMethodName.Equals("Type"))
            {
                return ModelConstants.DefaultPropertyType;
            }
            else if (propertyDescriptorMethodName.Equals("StoreGeneratedPattern"))
            {
                return TypedEFElement.StoreGeneratedPattern.DefaultValue;
            }
            return base.GetDescriptorDefaultValue(propertyDescriptorMethodName);
        }
    }
}
