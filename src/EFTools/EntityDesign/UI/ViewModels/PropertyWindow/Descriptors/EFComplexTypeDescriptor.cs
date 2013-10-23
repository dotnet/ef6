// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System.ComponentModel;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters;

    internal class EFComplexTypeDescriptor : EFAnnotatableElementDescriptor<ComplexType>
    {
        [LocDescription("PropertyWindow_Description_ComplexTypeName")]
        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }

        public override string GetComponentName()
        {
            return TypedEFElement.NormalizedNameExternal;
        }

        public override string GetClassName()
        {
            return "ComplexType";
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
