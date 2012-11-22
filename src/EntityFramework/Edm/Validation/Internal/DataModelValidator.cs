// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation.Internal
{
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.Edm.Validation.Internal.EdmModel;

    internal class DataModelValidator
    {
        public event EventHandler<DataModelErrorEventArgs> OnError;

        public void Validate(Core.Metadata.Edm.EdmModel root, bool validateSyntax)
        {
            var context = new EdmModelValidationContext(validateSyntax);

            context.OnError += OnError;

            context.Validate(root);
        }
    }
}
