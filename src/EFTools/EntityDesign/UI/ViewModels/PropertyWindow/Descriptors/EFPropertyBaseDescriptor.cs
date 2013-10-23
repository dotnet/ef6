// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System.ComponentModel;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters;

    internal class EFPropertyBaseDescriptor<T> :
        EFAnnotatableElementDescriptor<T>, IAnnotatableDocumentableDescriptor
        where T : PropertyBase
    {
        [LocCategory("PropertyWindow_Category_CodeGeneration")]
        [LocDisplayName("PropertyWindow_DisplayName_Getter")]
        [LocDescription("PropertyWindow_Description_Getter")]
        [TypeConverter(typeof(GetterSetterConverter))]
        public string Getter
        {
            get { return TypedEFElement.Getter.Value; }
            set
            {
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                var cmd = new UpdateDefaultableValueCommand<string>(TypedEFElement.Getter, value);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
            }
        }

        internal virtual bool IsBrowsableGetter()
        {
            return false;
        }

        internal virtual bool IsReadOnlyGetter()
        {
            return false;
        }

        [LocCategory("PropertyWindow_Category_CodeGeneration")]
        [LocDisplayName("PropertyWindow_DisplayName_Setter")]
        [LocDescription("PropertyWindow_Description_Setter")]
        [TypeConverter(typeof(GetterSetterConverter))]
        public string Setter
        {
            get { return TypedEFElement.Setter.Value; }
            set
            {
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                var cmd = new UpdateDefaultableValueCommand<string>(TypedEFElement.Setter, value);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
            }
        }

        internal virtual bool IsBrowsableSetter()
        {
            return false;
        }

        internal virtual bool IsReadOnlySetter()
        {
            return false;
        }

        public override object GetDescriptorDefaultValue(string propertyDescriptorMethodName)
        {
            if (propertyDescriptorMethodName.Equals("Getter"))
            {
                return TypedEFElement.Getter.DefaultValue;
            }
            else if (propertyDescriptorMethodName.Equals("Setter"))
            {
                return TypedEFElement.Setter.DefaultValue;
            }
            return base.GetDescriptorDefaultValue(propertyDescriptorMethodName);
        }
    }
}
