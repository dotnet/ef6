// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref="NotMappedAttribute" /> found on properties in the model.
    /// </summary>
    public sealed class NotMappedPropertyAttributeConvention
        : IConfigurationConvention<PropertyInfo, StructuralTypeConfiguration>
    {
        private readonly IConfigurationConvention<PropertyInfo, StructuralTypeConfiguration> _impl
            = new NotMappedPropertyAttributeConventionImpl();

        internal NotMappedPropertyAttributeConvention()
        {
        }

        void IConfigurationConvention<PropertyInfo, StructuralTypeConfiguration>.Apply(
            PropertyInfo memberInfo, Func<StructuralTypeConfiguration> configuration)
        {
            _impl.Apply(memberInfo, configuration);
        }

        internal sealed class NotMappedPropertyAttributeConventionImpl
            : AttributeConfigurationConvention<PropertyInfo, StructuralTypeConfiguration, NotMappedAttribute>
        {
            internal override void Apply(
                PropertyInfo propertyInfo, StructuralTypeConfiguration structuralTypeConfiguration, NotMappedAttribute _)
            {
                structuralTypeConfiguration.Ignore(propertyInfo);
            }
        }
    }
}
