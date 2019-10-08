// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.ViewModel
{
    using Microsoft.VisualStudio.Modeling.Validation;

    [ValidationState(ValidationState.Disabled)]
    internal partial class EntityDesignerViewModel
    {
#if false
        Temporarily keep this as a reference for when we remove DSL validation.  
        This will get validated by the runtime.

        /// <summary>
        /// Validate model Namespace
        /// </summary>
        /// <param name="context"></param>
        [ValidationMethod(ValidationCategories.Open | ValidationCategories.Save, CustomCategory = "OnTransactionCommitted")]
        private void ValidateNamespace(ValidationContext context)
        {
            if (String.IsNullOrEmpty(this.Namespace))
            {
                string message = String.Format(CultureInfo.CurrentCulture, Properties.Resources.Error_ModelNamespaceEmpty);
                context.LogError(message, Properties.Resources.ErrorCode_ModelNamespaceEmpty, this);
            }
            else if (EscherAttributeContentValidator.IsValidCSDLNamespaceName(this.Namespace) == false)
            {
                string message = String.Format(CultureInfo.CurrentCulture, Properties.Resources.Error_ModelNamespaceInvalid, this.Namespace);
                context.LogError(message, Properties.Resources.ErrorCode_ModelNamespaceInvalid, this);
            }
        }
#endif
    }
}
