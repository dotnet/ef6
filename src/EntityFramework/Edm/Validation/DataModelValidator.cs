// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation
{
    using System.Data.Entity.Core.Metadata.Edm;

    internal class DataModelValidator
    {
        public event EventHandler<DataModelErrorEventArgs> OnError;

        public void Validate(EdmModel root, bool validateSyntax)
        {
            var context = new EdmModelValidationContext(validateSyntax);

            context.OnError += OnError;

            context.Validate(root);
        }
    }
}
