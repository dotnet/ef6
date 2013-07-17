// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Used to configure a column with length facets for an entity type or complex type. This configuration functionality is exposed by the Code First Fluent API, see <see cref="DbModelBuilder"/>. 
    /// </summary>
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

        /// <summary>Configures the column to allow the maximum length supported by the database provider.</summary>
        /// <returns>The same <see cref="T:System.Data.Entity.ModelConfiguration.Configuration.LengthColumnConfiguration" /> instance so that multiple calls can be chained.</returns>
        public LengthColumnConfiguration IsMaxLength()
        {
            Configuration.IsMaxLength = true;
            Configuration.MaxLength = null;

            return this;
        }

        /// <summary>Configures the column to have the specified maximum length.</summary>
        /// <returns>The same <see cref="T:System.Data.Entity.ModelConfiguration.Configuration.LengthColumnConfiguration" /> instance so that multiple calls can be chained.</returns>
        /// <param name="value">The maximum length for the column. Setting the value to null will remove any maximum length restriction from the column and a default length will be used for the database column.</param>
        public LengthColumnConfiguration HasMaxLength(int? value)
        {
            Configuration.MaxLength = value;
            Configuration.IsMaxLength = null;

            return this;
        }

        /// <summary>Configures the column to be fixed length.</summary>
        /// <returns>The same <see cref="T:System.Data.Entity.ModelConfiguration.Configuration.LengthColumnConfiguration" /> instance so that multiple calls can be chained.</returns>
        public LengthColumnConfiguration IsFixedLength()
        {
            Configuration.IsFixedLength = true;

            return this;
        }

        /// <summary>Configures the column to be variable length.</summary>
        /// <returns>The same <see cref="T:System.Data.Entity.ModelConfiguration.Configuration.LengthColumnConfiguration" /> instance so that multiple calls can be chained.</returns>
        public LengthColumnConfiguration IsVariableLength()
        {
            Configuration.IsFixedLength = false;

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

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
