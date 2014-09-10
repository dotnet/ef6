// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Diagnostics;

    internal abstract class DataModelValidationRule<TItem> : DataModelValidationRule
        where TItem : class
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

        internal override void Evaluate(EdmModelValidationContext context, MetadataItem item)
        {
            var tItem = item as TItem;
            Debug.Assert(tItem != null);
            _validate(context, tItem);
        }
    }
}
