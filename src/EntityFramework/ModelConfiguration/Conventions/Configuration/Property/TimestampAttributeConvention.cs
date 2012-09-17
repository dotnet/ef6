// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref="TimestampAttribute" /> found on properties in the model.
    /// </summary>
    public class TimestampAttributeConvention
        : AttributeConfigurationConvention<PropertyInfo, BinaryPropertyConfiguration, TimestampAttribute>
    {
        public override void Apply(
            PropertyInfo memberInfo, BinaryPropertyConfiguration configuration, TimestampAttribute attribute)
        {
            if (configuration.IsRowVersion == null)
            {
                configuration.IsRowVersion = true;
            }
        }
    }
}
