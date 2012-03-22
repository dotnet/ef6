namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Diagnostics.Contracts;

    public class PrimitiveColumnConfiguration
    {
        private readonly Properties.Primitive.PrimitivePropertyConfiguration _configuration;

        internal PrimitiveColumnConfiguration(Properties.Primitive.PrimitivePropertyConfiguration configuration)
        {
            Contract.Requires(configuration != null);

            _configuration = configuration;
        }

        internal Properties.Primitive.PrimitivePropertyConfiguration Configuration
        {
            get { return _configuration; }
        }

        public PrimitiveColumnConfiguration IsOptional()
        {
            Configuration.IsNullable = true;

            return this;
        }

        public PrimitiveColumnConfiguration IsRequired()
        {
            Configuration.IsNullable = false;

            return this;
        }

        public PrimitiveColumnConfiguration HasColumnType(string columnType)
        {
            Configuration.ColumnType = columnType;

            return this;
        }

        public PrimitiveColumnConfiguration HasColumnOrder(int? columnOrder)
        {
            if (!(columnOrder == null || columnOrder.Value >= 0))
            {
                throw new ArgumentOutOfRangeException("columnOrder");
            }

            Configuration.ColumnOrder = columnOrder;

            return this;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
