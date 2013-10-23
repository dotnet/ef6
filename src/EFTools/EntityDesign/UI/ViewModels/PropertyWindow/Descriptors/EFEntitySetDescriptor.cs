// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System.ComponentModel;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters;

    internal class EFEntitySetDescriptor : EFAnnotatableElementDescriptor<EntitySet>
    {
        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_EntityType")]
        public string EntityType
        {
            get { return TypedEFElement.EntityType.RefName; }
        }

        [LocCategory("PropertyWindow_Category_CodeGeneration")]
        [LocDisplayName("PropertyWindow_DisplayName_Getter")]
        [LocDescription("PropertyWindow_Description_Getter")]
        [TypeConverter(typeof(GetterSetterConverter))]
        public string GetterAccess
        {
            get
            {
                var conc = TypedEFElement as ConceptualEntitySet;
                if (conc != null)
                {
                    return conc.GetterAccess.Value;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                var conc = TypedEFElement as ConceptualEntitySet;
                if (conc != null)
                {
                    var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                    var cmd = new UpdateDefaultableValueCommand<string>(conc.GetterAccess, value);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                }
            }
        }

        public bool IsBrowsableGetterAccess()
        {
            // only show this item if this is a conceptual entity set
            return TypedEFElement.EntityModel.IsCSDL;
        }

        public override string GetComponentName()
        {
            return TypedEFElement.NormalizedNameExternal;
        }

        public override string GetClassName()
        {
            return "EntitySet";
        }

        public override object GetDescriptorDefaultValue(string propertyDescriptorMethodName)
        {
            if (propertyDescriptorMethodName.Equals("GetterAccess"))
            {
                var conc = TypedEFElement as ConceptualEntitySet;
                if (conc != null)
                {
                    return conc.GetterAccess.DefaultValue;
                }
                else
                {
                    return string.Empty;
                }
            }
            return base.GetDescriptorDefaultValue(propertyDescriptorMethodName);
        }
    }
}
