// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.ViewModel
{
    using System;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.EntityDesigner.Properties;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.VisualStudio.Modeling.Validation;

    [ValidationState(ValidationState.Disabled)]
    internal partial class Association
    {
        /// <summary>
        ///     Validate Association name
        /// </summary>
        /// <param name="context"></param>
        [ValidationMethod(ValidationCategories.Open | ValidationCategories.Save, CustomCategory = "OnTransactionCommitted")]
        private void ValidateName(ValidationContext context)
        {
            if (!EscherAttributeContentValidator.IsValidCsdlAssociationName(Name))
            {
                var message = String.Format(CultureInfo.CurrentCulture, Resources.Error_AssociationNameInvalid, Name);
                context.LogError(message, Resources.ErrorCode_AssociationNameInvalid, this);
            }
        }
    }
}
