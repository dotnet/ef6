// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.Resources;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref="StringLengthAttribute" /> found on properties in the model.
    /// </summary>
    public sealed class StringLengthAttributeConvention
        : IConfigurationConvention<PropertyInfo, StringPropertyConfiguration>
    {
        private readonly IConfigurationConvention<PropertyInfo, StringPropertyConfiguration> _impl
            = new StringLengthAttributeConventionImpl();

        internal StringLengthAttributeConvention()
        {
        }

        void IConfigurationConvention<PropertyInfo, StringPropertyConfiguration>.Apply(
            PropertyInfo memberInfo, Func<StringPropertyConfiguration> configuration)
        {
            _impl.Apply(memberInfo, configuration);
        }

        internal sealed class StringLengthAttributeConventionImpl
            : AttributeConfigurationConvention<PropertyInfo, StringPropertyConfiguration, StringLengthAttribute>
        {
            internal override void Apply(
                PropertyInfo propertyInfo,
                StringPropertyConfiguration stringPropertyConfiguration,
                StringLengthAttribute stringLengthAttribute)
            {
                if (stringLengthAttribute.MaximumLength < -1
                    || stringLengthAttribute.MaximumLength == 0)
                {
                    throw Error.StringLengthAttributeConvention_InvalidMaximumLength(
                        propertyInfo.Name, propertyInfo.ReflectedType);
                }

                // Set the length if the string configuration's maxlength is not yet set
                if (stringPropertyConfiguration.IsMaxLength == null
                    && stringPropertyConfiguration.MaxLength == null)
                {
                    stringPropertyConfiguration.MaxLength = stringLengthAttribute.MaximumLength;
                }
            }
        }
    }
}
