namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Configures a database column used to store a string values.
    ///     This configuration functionality is available via the Code First Fluent API, see <see cref = "DbModelBuilder" />.
    /// </summary>
    public class StringColumnConfiguration : LengthColumnConfiguration
    {
        internal StringColumnConfiguration(Properties.Primitive.StringPropertyConfiguration configuration)
            : base(configuration)
        {
        }

        internal new Properties.Primitive.StringPropertyConfiguration Configuration
        {
            get { return (Properties.Primitive.StringPropertyConfiguration)base.Configuration; }
        }

        /// <summary>
        ///     Configures the column to allow the maximum length supported by the database provider.
        /// </summary>
        /// <returns>The same StringColumnConfiguration instance so that multiple calls can be chained.</returns>
        public new StringColumnConfiguration IsMaxLength()
        {
            base.IsMaxLength();

            return this;
        }

        /// <summary>
        ///     Configures the property to have the specified maximum length.
        /// </summary>
        /// <param name = "value">
        ///     The maximum length for the property.
        ///     Setting 'null' will result in a default length being used for the column.
        ///     <returns>The same StringColumnConfiguration instance so that multiple calls can be chained.</returns>
        public new StringColumnConfiguration HasMaxLength(int? value)
        {
            base.HasMaxLength(value);

            return this;
        }

        /// <summary>
        ///     Configures the column to be fixed length.
        ///     Use HasMaxLength to set the length that the property is fixed to.
        /// </summary>
        /// <returns>The same StringColumnConfiguration instance so that multiple calls can be chained.</returns>
        public new StringColumnConfiguration IsFixedLength()
        {
            base.IsFixedLength();

            return this;
        }

        /// <summary>
        ///     Configures the column to be variable length.
        ///     Columns are variable length by default.
        /// </summary>
        /// <returns>The same StringColumnConfiguration instance so that multiple calls can be chained.</returns>
        public new StringColumnConfiguration IsVariableLength()
        {
            base.IsVariableLength();

            return this;
        }

        /// <summary>
        ///     Configures the column to be optional.
        /// </summary>
        /// <returns>The same StringColumnConfiguration instance so that multiple calls can be chained.</returns>
        public new StringColumnConfiguration IsOptional()
        {
            base.IsOptional();

            return this;
        }

        /// <summary>
        ///     Configures the column to be required.
        /// </summary>
        /// <returns>The same StringColumnConfiguration instance so that multiple calls can be chained.</returns>
        public new StringColumnConfiguration IsRequired()
        {
            base.IsRequired();

            return this;
        }

        /// <summary>
        ///     Configures the data type of the database column.
        /// </summary>
        /// <param name = "columnType">Name of the database provider specific data type.</param>
        /// <returns>The same StringColumnConfiguration instance so that multiple calls can be chained.</returns>
        public new StringColumnConfiguration HasColumnType(string columnType)
        {
            base.HasColumnType(columnType);

            return this;
        }

        /// <summary>
        ///     Configures the order of the database column.
        /// </summary>
        /// <param name = "columnOrder">The order that this column should appear in the database table.</param>
        /// <returns>The same StringColumnConfiguration instance so that multiple calls can be chained.</returns>
        public new StringColumnConfiguration HasColumnOrder(int? columnOrder)
        {
            base.HasColumnOrder(columnOrder);

            return this;
        }

        /// <summary>
        ///     Configures the column to support Unicode string content.
        /// </summary>
        /// <returns>The same StringColumnConfiguration instance so that multiple calls can be chained.</returns>
        public StringColumnConfiguration IsUnicode()
        {
            IsUnicode(true);

            return this;
        }

        /// <summary>
        ///     Configures whether or not the column supports Unicode string content.
        /// </summary>
        /// <param name = "unicode">
        ///     Value indicating if the column supports Unicode string content or not.
        ///     Specifying 'null' will remove the Unicode facet from the column.
        ///     Specifying 'null' will cause the same runtime behavior as specifying 'false'.
        /// </param>
        /// <returns>The same StringColumnConfiguration instance so that multiple calls can be chained.</returns>
        public StringColumnConfiguration IsUnicode(bool? unicode)
        {
            Configuration.IsUnicode = unicode;

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
