// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    internal class DataModelValidator
    {
        public event EventHandler<DataModelErrorEventArgs> OnError;

        public void Validate(EdmModel model, bool validateSyntax)
        {
            var context = new EdmModelValidationContext(model, validateSyntax);

            context.OnError += OnError;

            var modelVisitor
                = new EdmModelValidationVisitor(
                    context,
                    EdmModelRuleSet.CreateEdmModelRuleSet(model.SchemaVersion, validateSyntax));

            modelVisitor.Visit(model);
        }
    }
}
