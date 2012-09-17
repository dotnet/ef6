// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref="ConcurrencyCheckAttribute" /> found on properties in the model.
    /// </summary>
    public class ConcurrencyCheckAttributeConvention
        : AttributeConfigurationConvention<PropertyInfo, PrimitivePropertyConfiguration, ConcurrencyCheckAttribute>
    {
        public override void Apply(
            PropertyInfo memberInfo, PrimitivePropertyConfiguration configuration,
            ConcurrencyCheckAttribute attribute)
        {
            if (configuration.ConcurrencyMode == null)
            {
                configuration.ConcurrencyMode = EdmConcurrencyMode.Fixed;
            }
        }
    }
}
