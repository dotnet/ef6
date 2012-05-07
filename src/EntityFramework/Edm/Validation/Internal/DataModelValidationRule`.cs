namespace System.Data.Entity.Edm.Validation.Internal
{
    using System.Data.Entity.Edm.Common;
    using System.Diagnostics.Contracts;

    internal abstract class DataModelValidationRule<TContext, TItem> : DataModelValidationRule
        where TContext : DataModelValidationContext
        where TItem : DataModelItem
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

        internal override void Evaluate(DataModelValidationContext context, DataModelItem item)
        {
            Contract.Assert(context is TContext, "context should be " + typeof(TContext));
            Contract.Assert(item is TItem, "item should be " + typeof(TItem));

            _validate((TContext)context, (TItem)item);
        }
    }
}