// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System.ComponentModel;
    using Microsoft.Data.Entity.Design.Core.Controls;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters;

    internal class EFEnumTypeDescriptor : EFAnnotatableElementDescriptor<EnumType>
    {
        public override string GetComponentName()
        {
            return TypedEFElement.NormalizedNameExternal;
        }

        public override string GetClassName()
        {
            return "EnumType";
        }

        [LocCategory("PropertyWindow_Category_CodeGeneration")]
        [LocDisplayName("PropertyWindow_DisplayName_Access")]
        [LocDescription("PropertyWindow_Description_Access")]
        [TypeConverter(typeof(AccessConverter))]
        public string TypeAccess
        {
            get { return TypedEFElement.TypeAccess.Value; }
            set
            {
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                var cmd = new UpdateDefaultableValueCommand<string>(TypedEFElement.TypeAccess, value);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
            }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDescription("PropertyWindow_Description_EnumTypeName")]
        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_EnumUnderlyingType")]
        [LocDescription("PropertyWindow_Description_EnumUnderlyingType")]
        [TypeConverter(typeof(EnumUnderlyingTypeConverter))]
        public string UnderlyingType
        {
            get { return TypedEFElement.UnderlyingType.Value; }
            set
            {
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                CommandProcessor.InvokeSingleCommand(cpc, new ChangeEnumUnderlyingTypeCommand(TypedEFElement, value));
            }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_EnumFlagAttribute")]
        [LocDescription("EnumDialog_IsFlagLabelDescription")]
        [TypeConverter(typeof(BoolConverter))]
        public bool IsFlag
        {
            get { return TypedEFElement.IsFlags.Value; }
            set
            {
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                CommandProcessor.InvokeSingleCommand(cpc, new SetEnumFlagAttributeCommand(TypedEFElement, value));
            }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_EnumExternalTypeAttribute")]
        [LocDescription("PropertyWindow_Description_EnumExternalTypeAttribute")]
        public string ExternalTypeName
        {
            get { return TypedEFElement.ExternalTypeName.Value; }
            set
            {
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                CommandProcessor.InvokeSingleCommand(
                    cpc,
                    new SetEnumTypeFacetCommand(
                        TypedEFElement, TypedEFElement.Name.Value,
                        TypedEFElement.UnderlyingType.Value, value, TypedEFElement.IsFlags.Value));
            }
        }

        public override object GetDescriptorDefaultValue(string propertyDescriptorMethodName)
        {
            if (propertyDescriptorMethodName.Equals("TypeAccess"))
            {
                return TypedEFElement.TypeAccess.DefaultValue;
            }
            return base.GetDescriptorDefaultValue(propertyDescriptorMethodName);
        }
    }
}
