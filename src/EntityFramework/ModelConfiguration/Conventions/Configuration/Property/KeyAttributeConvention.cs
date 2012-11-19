// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Utilities;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref="KeyAttribute" /> found on properties in the model.
    /// </summary>
    public class KeyAttributeConvention :
        AttributeConfigurationConvention<PropertyInfo, EntityTypeConfiguration, KeyAttribute>
    {
        public override void Apply(
            PropertyInfo memberInfo, EntityTypeConfiguration configuration, KeyAttribute attribute)
        {
            Check.NotNull(memberInfo, "memberInfo");
            Check.NotNull(configuration, "configuration");
            Check.NotNull(attribute, "attribute");

            if (memberInfo.IsValidEdmScalarProperty())
            {
                configuration.Key(memberInfo);
            }
        }
    }
}
