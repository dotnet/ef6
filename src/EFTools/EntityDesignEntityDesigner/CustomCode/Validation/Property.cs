// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.ViewModel
{
    using System;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.VisualStudio.Modeling.Validation;

    [ValidationState(ValidationState.Disabled)]
    internal partial class Property
    {
        /// <summary>
        ///     Validate property name
        /// </summary>
        [ValidationMethod(ValidationCategories.Open | ValidationCategories.Save, CustomCategory = "OnTransactionCommitted")]
        private void ValidateName(ValidationContext context)
        {
            if (!EscherAttributeContentValidator.IsValidCsdlPropertyName(Name))
            {
                var message = String.Format(CultureInfo.CurrentCulture, Resources.Error_PropertyNameInvalid, Name);
                context.LogError(message, Properties.Resources.ErrorCode_PropertyNameInvalid, this);
            }
        }
    }
}
