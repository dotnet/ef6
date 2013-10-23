// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System;
    using System.ComponentModel;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters;

    internal class EFParameterDescriptor : EFAnnotatableElementDescriptor<Parameter>
    {
        internal override bool IsReadOnlyName()
        {
            return true;
        }

        internal static bool CanResetType()
        {
            // cannot call Reset on Type attribute as this passes null to the Type setter which
            // removes the attribute which is a schema violation
            return false;
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_Type")]
        [TypeConverter(typeof(ScalarTypeConverter<EFParameterDescriptor, Parameter>))]
        public string Type
        {
            get { return TypedEFElement.Type.Value; }
            set
            {
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                var cmd = new UpdateDefaultableValueCommand<string>(TypedEFElement.Type, value);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
            }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_Parameter")]
        [LocDescription("PropertyWindow_Description_Parameter")]
        public string Parameter
        {
            get
            {
                if (TypedEFElement.Parent != null)
                {
                    var functionImport = TypedEFElement.Parent as FunctionImport;
                    if (functionImport != null)
                    {
                        if (functionImport.Function != null)
                        {
                            foreach (var efsParameter in functionImport.Function.Parameters())
                            {
                                if (efsParameter.LocalName.Value == TypedEFElement.LocalName.Value)
                                {
                                    return efsParameter.LocalName.Value;
                                }
                            }
                        }
                    }
                }
                return String.Empty;
            }
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
