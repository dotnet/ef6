// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System.ComponentModel;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters;

    internal class EFComplexPropertyDescriptor : EFPropertyDescriptorBase<ComplexConceptualProperty>
    {
        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_Type")]
        [LocDescription("PropertyWindow_Description_Type")]
        [TypeConverter(typeof(ComplexTypeConverter))]
        [MergableProperty(false)]
        public ComplexType Type
        {
            get { return TypedEFElement.ComplexType.Target; }
            set
            {
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                var changeType = new ChangeComplexPropertyTypeCommand(TypedEFElement, value);
                CommandProcessor.InvokeSingleCommand(cpc, changeType);
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

        internal override bool IsBrowsableConcurrencyMode()
        {
            return true;
        }
    }
}
