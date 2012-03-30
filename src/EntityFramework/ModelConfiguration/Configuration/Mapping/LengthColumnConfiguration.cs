namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    public abstract class LengthColumnConfiguration : PrimitiveColumnConfiguration
    {
        internal LengthColumnConfiguration(Properties.Primitive.LengthPropertyConfiguration configuration)
            : base(configuration)
        {
        }

        internal new Properties.Primitive.LengthPropertyConfiguration Configuration
        {
            get { return (Properties.Primitive.LengthPropertyConfiguration)base.Configuration; }
        }

        public LengthColumnConfiguration IsMaxLength()
        {
            Configuration.IsMaxLength = true;
            Configuration.MaxLength = null;

            return this;
        }

        public LengthColumnConfiguration HasMaxLength(int? value)
        {
            Configuration.MaxLength = value;
            Configuration.IsMaxLength = null;

            return this;
        }

        public LengthColumnConfiguration IsFixedLength()
        {
            Configuration.IsFixedLength = true;

            return this;
        }

        public LengthColumnConfiguration IsVariableLength()
        {
            Configuration.IsFixedLength = false;

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

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
