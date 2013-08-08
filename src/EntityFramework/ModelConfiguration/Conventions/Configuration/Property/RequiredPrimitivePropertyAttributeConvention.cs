// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Convention to process instances of <see cref="RequiredAttribute" /> found on primitive properties in the model.
    /// </summary>
    public class RequiredPrimitivePropertyAttributeConvention
        : PrimitivePropertyAttributeConfigurationConvention<RequiredAttribute>
    {
        /// <inheritdoc/>
        public override void Apply(ConventionPrimitivePropertyConfiguration configuration, RequiredAttribute attribute)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(attribute, "attribute");

            configuration.IsRequired();
        }
    }
}
