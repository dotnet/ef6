// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref="RequiredAttribute" /> found on primitive properties in the model.
    /// </summary>
    public sealed class RequiredPrimitivePropertyAttributeConvention
        : IConfigurationConvention<PropertyInfo, PrimitivePropertyConfiguration>
    {
        private readonly IConfigurationConvention<PropertyInfo, PrimitivePropertyConfiguration> _impl
            = new RequiredPrimitivePropertyAttributeConventionImpl();

        internal RequiredPrimitivePropertyAttributeConvention()
        {
        }

        void IConfigurationConvention<PropertyInfo, PrimitivePropertyConfiguration>.Apply(
            PropertyInfo memberInfo, Func<PrimitivePropertyConfiguration> configuration)
        {
            _impl.Apply(memberInfo, configuration);
        }

        internal sealed class RequiredPrimitivePropertyAttributeConventionImpl
            : AttributeConfigurationConvention<PropertyInfo, PrimitivePropertyConfiguration, RequiredAttribute>
        {
            internal override void Apply(
                PropertyInfo memberInfo, PrimitivePropertyConfiguration primitivePropertyConfiguration,
                RequiredAttribute attribute)
            {
                if (primitivePropertyConfiguration.IsNullable == null)
                {
                    primitivePropertyConfiguration.IsNullable = false;
                }
            }
        }
    }
}
