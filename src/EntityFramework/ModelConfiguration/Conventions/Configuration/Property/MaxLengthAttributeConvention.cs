// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.Resources;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref="MaxLengthAttribute" /> found on properties in the model.
    /// </summary>
    public class MaxLengthAttributeConvention
        : AttributeConfigurationConvention<PropertyInfo, LengthPropertyConfiguration, MaxLengthAttribute>
    {
        private const int MaxLengthIndicator = -1;

        public override void Apply(
            PropertyInfo memberInfo, LengthPropertyConfiguration configuration,
            MaxLengthAttribute attribute)
        {
            if ((attribute.Length == 0)
                || (attribute.Length < MaxLengthIndicator))
            {
                throw Error.MaxLengthAttributeConvention_InvalidMaxLength(
                    memberInfo.Name, memberInfo.ReflectedType);
            }

            // Set the length if the length configuration's maxlength is not yet set
            if (configuration.IsMaxLength == null
                && configuration.MaxLength == null)
            {
                if (attribute.Length == MaxLengthIndicator)
                {
                    configuration.IsMaxLength = true;
                }
                else
                {
                    configuration.MaxLength = attribute.Length;
                }
            }
        }
    }
}
