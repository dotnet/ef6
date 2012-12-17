// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation
{
    internal class EdmModelValidationRule<TItem> : DataModelValidationRule<TItem>
    {
        internal EdmModelValidationRule(Action<EdmModelValidationContext, TItem> validate)
            : base(validate)
        {
        }
    }
}
