// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation.Internal
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Validation.Internal.EdmModel;

    internal abstract class DataModelValidationRule<TItem> : DataModelValidationRule
        where TItem : IMetadataItem
    {
        protected Action<EdmModelValidationContext, TItem> _validate;

        internal DataModelValidationRule(Action<EdmModelValidationContext, TItem> validate)
        {
            _validate = validate;
        }

        internal override Type ValidatedType
        {
            get { return typeof(TItem); }
        }

        internal override void Evaluate(EdmModelValidationContext context, IMetadataItem item)
        {
            _validate(context, (TItem)item);
        }
    }
}
