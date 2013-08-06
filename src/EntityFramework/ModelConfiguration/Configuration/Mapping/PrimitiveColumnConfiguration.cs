// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Configures a primitive column from an entity type.
    /// </summary>
    public class PrimitiveColumnConfiguration
    {
        private readonly Properties.Primitive.PrimitivePropertyConfiguration _configuration;

        internal PrimitiveColumnConfiguration(Properties.Primitive.PrimitivePropertyConfiguration configuration)
        {
            DebugCheck.NotNull(configuration);

            _configuration = configuration;
        }

        internal Properties.Primitive.PrimitivePropertyConfiguration Configuration
        {
            get { return _configuration; }
        }

        /// <summary>Configures the primitive column to be optional.</summary>
        /// <returns>The same <see cref="T:System.Data.Entity.ModelConfiguration.Configuration.PrimitiveColumnConfiguration" /> instance so that multiple calls can be chained.</returns>
        public PrimitiveColumnConfiguration IsOptional()
        {
            Configuration.IsNullable = true;

            return this;
        }

        /// <summary>Configures the primitive column to be required.</summary>
        /// <returns>The same <see cref="T:System.Data.Entity.ModelConfiguration.Configuration.PrimitiveColumnConfiguration" /> instance so that multiple calls can be chained.</returns>
        public PrimitiveColumnConfiguration IsRequired()
        {
            Configuration.IsNullable = false;

            return this;
        }

        /// <summary>Configures the data type of the primitive column used to store the property.</summary>
        /// <returns>The same <see cref="T:System.Data.Entity.ModelConfiguration.Configuration.PrimitiveColumnConfiguration" /> instance so that multiple calls can be chained.</returns>
        /// <param name="columnType">The name of the database provider specific data type.</param>
        public PrimitiveColumnConfiguration HasColumnType(string columnType)
        {
            Configuration.ColumnType = columnType;

            return this;
        }

        /// <summary>Configures the order of the primitive column used to store the property. This method is also used to specify key ordering when an entity type has a composite key.</summary>
        /// <returns>The same <see cref="T:System.Data.Entity.ModelConfiguration.Configuration.PrimitiveColumnConfiguration" /> instance so that multiple calls can be chained.</returns>
        /// <param name="columnOrder">The order that this column should appear in the database table.</param>
        public PrimitiveColumnConfiguration HasColumnOrder(int? columnOrder)
        {
            if (!(columnOrder == null || columnOrder.Value >= 0))
            {
                throw new ArgumentOutOfRangeException("columnOrder");
            }

            Configuration.ColumnOrder = columnOrder;

            return this;
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Gets the <see cref="Type" /> of the current instance.
        /// </summary>
        /// <returns>The exact runtime type of the current instance.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
