// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Convention to process instances of <see cref="ConcurrencyCheckAttribute" /> found on properties in the model.
    /// </summary>
    public class ConcurrencyCheckAttributeConvention
        : PrimitivePropertyAttributeConfigurationConvention<ConcurrencyCheckAttribute>
    {
        public override void Apply(LightweightPrimitivePropertyConfiguration configuration, ConcurrencyCheckAttribute attribute)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(attribute, "attribute");

            configuration.IsConcurrencyToken();
        }
    }
}
