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

        [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "Only cast twice in debug mode.")]
        internal override void Evaluate(EdmModelValidationContext context, MetadataItem item)
        {
            Debug.Assert(item is TItem);
            _validate(context, item as TItem);
        }
    }
}
