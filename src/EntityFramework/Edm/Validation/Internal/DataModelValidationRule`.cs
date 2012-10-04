// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation.Internal
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.Contracts;

    internal abstract class DataModelValidationRule<TContext, TItem> : DataModelValidationRule
        where TContext : DataModelValidationContext
        where TItem : IMetadataItem
    {
        protected Action<TContext, TItem> _validate;

        internal DataModelValidationRule(Action<TContext, TItem> validate)
        {
            _validate = validate;
        }

        internal override Type ValidatedType
        {
            get { return typeof(TItem); }
        }

        internal override void Evaluate(DataModelValidationContext context, IMetadataItem item)
        {
            Contract.Assert(context is TContext, "context should be " + typeof(TContext));
            Contract.Assert(item is TItem, "item should be " + typeof(TItem));

            _validate((TContext)context, (TItem)item);
        }
    }
}
