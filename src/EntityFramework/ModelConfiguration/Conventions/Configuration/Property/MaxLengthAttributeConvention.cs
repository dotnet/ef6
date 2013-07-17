// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Convention to process instances of <see cref="MaxLengthAttribute" /> found on properties in the model.
    /// </summary>
    public class MaxLengthAttributeConvention
        : PrimitivePropertyAttributeConfigurationConvention<MaxLengthAttribute>
    {
        private const int MaxLengthIndicator = -1;

        /// <inheritdoc/>
        public override void Apply(ConventionPrimitivePropertyConfiguration configuration, MaxLengthAttribute attribute)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(attribute, "attribute");

            var memberInfo = configuration.ClrPropertyInfo;
            if ((attribute.Length == 0)
                || (attribute.Length < MaxLengthIndicator))
            {
                throw Error.MaxLengthAttributeConvention_InvalidMaxLength(
                    memberInfo.Name, memberInfo.ReflectedType);
            }

            if (attribute.Length == MaxLengthIndicator)
            {
                configuration.IsMaxLength();
            }
            else
            {
                configuration.HasMaxLength(attribute.Length);
            }
        }
    }
}
