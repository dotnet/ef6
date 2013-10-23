// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class EFSFunctionDescriptor : EFAnnotatableElementDescriptor<Function>
    {
        public override string GetComponentName()
        {
            return TypedEFElement.NormalizedNameExternal;
        }

        public override string GetClassName()
        {
            return "Function";
        }

        private static bool IsReadOnly
        {
            get { return true; /*always read only*/ }
        }

        internal override bool IsReadOnlyName()
        {
            return IsReadOnly;
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_ReturnType")]
        public string ReturnType
        {
            get { return TypedEFElement.ReturnType.Value; }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_Aggregate")]
        public bool Aggregate
        {
            get { return TypedEFElement.Aggregate.Value; }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_BuiltIn")]
        public bool BuiltIn
        {
            get { return TypedEFElement.BuiltIn.Value; }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_StoreFunctionName")]
        public string StoreFunctionName
        {
            get { return TypedEFElement.StoreFunctionName.Value; }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_Niladic")]
        public bool Niladic
        {
            get { return TypedEFElement.NiladicFunction.Value; }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_Composable")]
        public bool Composable
        {
            get { return TypedEFElement.IsComposable.Value; }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_CommandText")]
        public string CommandText
        {
            get { return (null == TypedEFElement.CommandText) ? null : TypedEFElement.CommandText.Command; }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_ParameterTypeSemantics")]
        public string ParameterTypeSemantics
        {
            get { return TypedEFElement.ParameterTypeSemantic.Value; }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_DatabaseName")]
        [LocDescription("PropertyWindow_Description_DatabaseName")]
        public string DatabaseName
        {
            get
            {
                var func = TypedEFElement;
                if (null != func)
                {
                    return func.DatabaseFunctionName;
                }

                return string.Empty;
            }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_DatabaseSchema")]
        [LocDescription("PropertyWindow_Description_DatabaseSchema")]
        public string DatabaseSchema
        {
            get
            {
                var func = TypedEFElement;
                if (null != func)
                {
                    return func.DatabaseSchemaName;
                }

                return string.Empty;
            }
        }
    }
}
