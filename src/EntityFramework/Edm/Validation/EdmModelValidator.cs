// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation
{
    using System.Data.Entity.Core.Metadata.Edm;

    internal static class EdmModelValidator
    {
        internal static void Validate(EdmModel validateRoot, EdmModelValidationContext context)
        {
            var edmModelValidationRuleSet
                = EdmModelRuleSet.CreateEdmModelRuleSet(context.ValidationContextVersion, context.ValidateSyntax);

            var modelVisitor = new EdmModelValidationVisitor(context, edmModelValidationRuleSet);

            modelVisitor.Visit(validateRoot);
        }
    }
}
