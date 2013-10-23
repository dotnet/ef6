// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.ViewModel
{
    using System;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.EntityDesigner.Properties;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.VisualStudio.Modeling.Validation;

    [ValidationState(ValidationState.Disabled)]
    internal partial class EntityType
    {
        /// <summary>
        ///     Validate Entity name
        /// </summary>
        /// <param name="context"></param>
        [ValidationMethod(ValidationCategories.Open | ValidationCategories.Save, CustomCategory = "OnTransactionCommited")]
        private void ValidateName(ValidationContext context)
        {
            if (!EscherAttributeContentValidator.IsValidCsdlEntityTypeName(Name))
            {
                var message = String.Format(CultureInfo.CurrentCulture, Resources.Error_EntityNameInvalid, Name);
                context.LogError(message, Resources.ErrorCode_EntityNameInvalid, this);
            }
        }

        /// <summary>
        ///     Validate Entity key
        /// </summary>
        /// <param name="context"></param>
        [ValidationMethod(ValidationCategories.Open | ValidationCategories.Save, CustomCategory = "OnTransactionCommited")]
        private void ValidateKey(ValidationContext context)
        {
            try
            {
                var keyProperties = GetKeyProperties();
                if (keyProperties.Count == 0)
                {
                    var errorMessage = String.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Warning_EntityHasNoKeys, Name);

                    context.LogError(errorMessage, Resources.ErrorCode_EntityHasNoKeys, this);
                }
            }
            catch (InvalidOperationException)
            {
                // Circular inheritance detected, will be logged by "ValidateInheritance"
            }
        }

        /// <summary>
        ///     Validate inheritance (check for circular inheritance)
        /// </summary>
        /// <param name="context"></param>
        [ValidationMethod(ValidationCategories.Open | ValidationCategories.Save, CustomCategory = "OnTransactionCommited")]
        private void ValidateInheritance(ValidationContext context)
        {
            var circularPath = String.Empty;
            if (HasCircularInheritance(out circularPath))
            {
                var errorMessage = String.Format(
                    CultureInfo.CurrentCulture,
                    Resources.Error_CircularEntityInheritanceFound,
                    Name, circularPath);

                context.LogError(errorMessage, Resources.ErrorCode_CircularEntityInheritanceFound, this);
            }
        }
    }
}
