// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.Utilities;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref="ColumnAttribute" /> found on properties in the model
    /// </summary>
    public class ColumnAttributeConvention
        : AttributeConfigurationConvention<PropertyInfo, PrimitivePropertyConfiguration, ColumnAttribute>
    {
        public override void Apply(
            PropertyInfo memberInfo,
            PrimitivePropertyConfiguration configuration,
            ModelConfiguration modelConfiguration,
            ColumnAttribute attribute)
        {
            Check.NotNull(memberInfo, "memberInfo");
            Check.NotNull(configuration, "configuration");
            Check.NotNull(modelConfiguration, "modelConfiguration");
            Check.NotNull(attribute, "attribute");

            if (string.IsNullOrWhiteSpace(configuration.ColumnName)
                && !string.IsNullOrWhiteSpace(attribute.Name))
            {
                configuration.ColumnName = attribute.Name;
            }

            if (string.IsNullOrWhiteSpace(configuration.ColumnType)
                && !string.IsNullOrWhiteSpace(attribute.TypeName))
            {
                configuration.ColumnType = attribute.TypeName;
            }

            if ((configuration.ColumnOrder == null)
                && attribute.Order >= 0)
            {
                configuration.ColumnOrder = attribute.Order;
            }
        }
    }
}
