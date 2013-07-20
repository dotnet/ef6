// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    internal class EdmModelValidationRule<TItem> : DataModelValidationRule<TItem>
        where TItem : class
    {
        internal EdmModelValidationRule(Action<EdmModelValidationContext, TItem> validate)
            : base(validate)
        {
        }
    }
}
