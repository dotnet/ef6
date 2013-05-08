// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.Utilities;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref="ConcurrencyCheckAttribute" /> found on properties in the model.
    /// </summary>
    public class ConcurrencyCheckAttributeConvention
        : AttributeConfigurationConvention<PropertyInfo, PrimitivePropertyConfiguration, ConcurrencyCheckAttribute>
    {
        public override void Apply(
            PropertyInfo memberInfo,
            PrimitivePropertyConfiguration configuration,
            ModelConfiguration modelConfiguration,
            ConcurrencyCheckAttribute attribute)
        {
            Check.NotNull(memberInfo, "memberInfo");
            Check.NotNull(configuration, "configuration");
            Check.NotNull(modelConfiguration, "modelConfiguration");
            Check.NotNull(attribute, "attribute");

            if (configuration.ConcurrencyMode == null)
            {
                configuration.ConcurrencyMode = ConcurrencyMode.Fixed;
            }
        }
    }
}
