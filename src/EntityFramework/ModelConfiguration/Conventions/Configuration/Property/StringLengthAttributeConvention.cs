// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Convention to process instances of <see cref="StringLengthAttribute" /> found on properties in the model.
    /// </summary>
    public class StringLengthAttributeConvention
        : PrimitivePropertyAttributeConfigurationConvention<StringLengthAttribute>
    {
        public override void Apply(LightweightPrimitivePropertyConfiguration configuration, StringLengthAttribute attribute)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(attribute, "attribute");
            
            if (attribute.MaximumLength < 1)
            {
                var memberInfo = configuration.ClrPropertyInfo;
                throw Error.StringLengthAttributeConvention_InvalidMaximumLength(
                    memberInfo.Name, memberInfo.ReflectedType);
            }

            // Set the length if the string configuration's maxlength is not yet set
            configuration.HasMaxLength(attribute.MaximumLength);
        }
    }
}
