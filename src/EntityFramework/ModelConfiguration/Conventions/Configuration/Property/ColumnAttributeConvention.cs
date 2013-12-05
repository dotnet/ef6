// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Convention to process instances of <see cref="ColumnAttribute" /> found on properties in the model
    /// </summary>
    public class ColumnAttributeConvention
        : PrimitivePropertyAttributeConfigurationConvention<ColumnAttribute>
    {
        /// <inheritdoc/>
        public override void Apply(ConventionPrimitivePropertyConfiguration configuration, ColumnAttribute attribute)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(attribute, "attribute");

            if (!string.IsNullOrWhiteSpace(attribute.Name))
            {
                configuration.HasColumnName(attribute.Name);
            }

            if (!string.IsNullOrWhiteSpace(attribute.TypeName))
            {
                configuration.HasColumnType(attribute.TypeName);
            }

            if (attribute.Order >= 0)
            {
                configuration.HasColumnOrder(attribute.Order);
            }
        }
    }
}
