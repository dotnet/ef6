// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref = "DatabaseGeneratedAttribute" /> found on properties in the model.
    /// </summary>
    public sealed class DatabaseGeneratedAttributeConvention
        : IConfigurationConvention<PropertyInfo, PrimitivePropertyConfiguration>
    {
        private readonly IConfigurationConvention<PropertyInfo, PrimitivePropertyConfiguration> _impl
            = new DatabaseGeneratedAttributeConventionImpl();

        internal DatabaseGeneratedAttributeConvention()
        {
        }

        void IConfigurationConvention<PropertyInfo, PrimitivePropertyConfiguration>.Apply(
            PropertyInfo memberInfo, Func<PrimitivePropertyConfiguration> configuration)
        {
            _impl.Apply(memberInfo, configuration);
        }

        internal sealed class DatabaseGeneratedAttributeConventionImpl
            : AttributeConfigurationConvention<PropertyInfo, PrimitivePropertyConfiguration, DatabaseGeneratedAttribute>
        {
            internal override void Apply(
                PropertyInfo propertyInfo,
                PrimitivePropertyConfiguration primitivePropertyConfiguration,
                DatabaseGeneratedAttribute databaseGeneratedAttribute)
            {
                if (primitivePropertyConfiguration.DatabaseGeneratedOption == null)
                {
                    primitivePropertyConfiguration.DatabaseGeneratedOption =
                        databaseGeneratedAttribute.DatabaseGeneratedOption;
                }
            }
        }
    }
}
