// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Edm.Validation.Internal.EdmModel
{
    using System.Data.Entity.Edm.Common;

    internal class EdmModelValidationRule<TItem> : DataModelValidationRule<EdmModelValidationContext, TItem>
        where TItem : DataModelItem
    {
        internal EdmModelValidationRule(Action<EdmModelValidationContext, TItem> validate)
            :
                base(validate)
        {
        }
    }
}
