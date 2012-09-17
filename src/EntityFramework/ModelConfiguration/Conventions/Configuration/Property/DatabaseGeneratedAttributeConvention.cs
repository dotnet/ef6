// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref="DatabaseGeneratedAttribute" /> found on properties in the model.
    /// </summary>
    public class DatabaseGeneratedAttributeConvention
        : AttributeConfigurationConvention<PropertyInfo, PrimitivePropertyConfiguration, DatabaseGeneratedAttribute>
    {
        public override void Apply(
            PropertyInfo memberInfo,
            PrimitivePropertyConfiguration configuration,
            DatabaseGeneratedAttribute attribute)
        {
            if (configuration.DatabaseGeneratedOption == null)
            {
                configuration.DatabaseGeneratedOption =
                    attribute.DatabaseGeneratedOption;
            }
        }
    }
}
