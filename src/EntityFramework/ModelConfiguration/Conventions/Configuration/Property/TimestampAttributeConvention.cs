// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.Utilities;
    using System.Reflection;
    using BinaryPropertyConfiguration = System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.BinaryPropertyConfiguration;

    /// <summary>
    ///     Convention to process instances of <see cref="TimestampAttribute" /> found on properties in the model.
    /// </summary>
    public class TimestampAttributeConvention
        : AttributeConfigurationConvention<PropertyInfo, BinaryPropertyConfiguration, TimestampAttribute>
    {
        public override void Apply(
            PropertyInfo memberInfo, BinaryPropertyConfiguration configuration, ModelConfiguration modelConfiguration,
            TimestampAttribute attribute)
        {
            Check.NotNull(memberInfo, "memberInfo");
            Check.NotNull(configuration, "configuration");
            Check.NotNull(modelConfiguration, "modelConfiguration");
            Check.NotNull(attribute, "attribute");

            if (configuration.IsRowVersion == null)
            {
                configuration.IsRowVersion = true;
            }
        }
    }
}
