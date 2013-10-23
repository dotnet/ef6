// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing.Design;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.TypeEditors;
    using Resources = Microsoft.Data.Tools.XmlDesignerBase.Resources;

    internal class EFFunctionImportDescriptor : EFAnnotatableElementDescriptor<FunctionImport>
    {
        [MergableProperty(false)]
        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_StoredProcedureName")]
        [TypeConverter(typeof(FuncImpSprocConverter))]
        public Function StoredProcedureName
        {
            get { return TypedEFElement.Function; }
            set
            {
                if (value == TypedEFElement.Function)
                {
                    return;
                }
                var cModel = TypedEFElement.RuntimeModelRoot() as ConceptualEntityModel;
                Debug.Assert(cModel != null, "Why isn't there a conceptual entity model for this function import?");
                if (cModel != null)
                {
                    var cmdFuncImpSproc = new ChangeFunctionImportCommand(
                        cModel.FirstEntityContainer as ConceptualEntityContainer,
                        TypedEFElement,
                        value,
                        TypedEFElement.LocalName.Value,
                        TypedEFElement.IsComposable.Value,
                        false,
                        null);
                    var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                    CommandProcessor.InvokeSingleCommand(cpc, cmdFuncImpSproc);
                }
            }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_ReturnType")]
        [Editor(typeof(FunctionImportReturnTypeTypeEditor), typeof(UITypeEditor))]
        public string ReturnType
        {
            get
            {
                // Return FunctionImport's return type display name.
                // If the return type is an entity type, the name will be the entity type's local name.
                // if the return type is a complex type, the name will be the complex type local name.
                // if the return type is a primitive type or "None", the property will return the primitive type name or "None".
                return TypedEFElement.ReturnTypeToPrettyString;
            }
            // do not define setter - this ensures that the return type is only editable through
            // the FunctionImportReturnTypeTypeEditor editor
        }

        [LocCategory("PropertyWindow_Category_CodeGeneration")]
        [LocDisplayName("PropertyWindow_DisplayName_Access")]
        [LocDescription("PropertyWindow_Description_Access")]
        [TypeConverter(typeof(GetterSetterConverter))]
        public string MethodAccess
        {
            get { return TypedEFElement.MethodAccess.Value; }
            set
            {
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                var cmd = new UpdateDefaultableValueCommand<string>(TypedEFElement.MethodAccess, value);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
            }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_Composable")]
        [LocDescription("PropertyWindow_Description_Composable")]
        [TypeConverter(typeof(BoolOrNoneTypeConverter))]
        public BoolOrNone Composable
        {
            get { return TypedEFElement.IsComposable.Value; }
        }

        public override string GetComponentName()
        {
            return TypedEFElement.NormalizedNameExternal;
        }

        public override string GetClassName()
        {
            return "FunctionImport";
        }

        public override object GetDescriptorDefaultValue(string propertyDescriptorMethodName)
        {
            if (propertyDescriptorMethodName.Equals("ReturnType"))
            {
                return Resources.NoneDisplayValueUsedForUX;
            }
            else if (propertyDescriptorMethodName.Equals("MethodAccess"))
            {
                return TypedEFElement.MethodAccess.DefaultValue;
            }
            return base.GetDescriptorDefaultValue(propertyDescriptorMethodName);
        }
    }
}
