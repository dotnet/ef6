// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    public class PrimitiveColumnConfiguration
    {
        private readonly PrimitivePropertyConfiguration _configuration;

        internal PrimitiveColumnConfiguration(PrimitivePropertyConfiguration configuration)
        {
            Contract.Requires(configuration != null);

            _configuration = configuration;
        }

        internal PrimitivePropertyConfiguration Configuration
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

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
