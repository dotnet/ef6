namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Utilities;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref = "KeyAttribute" /> found on properties in the model.
    /// </summary>
    public sealed class KeyAttributeConvention : IConfigurationConvention<PropertyInfo, EntityTypeConfiguration>
    {
        private readonly IConfigurationConvention<PropertyInfo, EntityTypeConfiguration> _impl
            = new KeyAttributeConventionImpl();

        internal KeyAttributeConvention()
        {
        }

        void IConfigurationConvention<PropertyInfo, EntityTypeConfiguration>.Apply(
            PropertyInfo memberInfo, Func<EntityTypeConfiguration> configuration)
        {
            _impl.Apply(memberInfo, configuration);
        }

        internal sealed class KeyAttributeConventionImpl :
            AttributeConfigurationConvention<PropertyInfo, EntityTypeConfiguration, KeyAttribute>
        {
            internal override void Apply(
                PropertyInfo propertyInfo, EntityTypeConfiguration entityTypeConfiguration, KeyAttribute _)
            {
                if (propertyInfo.IsValidEdmScalarProperty())
                {
                    entityTypeConfiguration.Key(propertyInfo);
                }
            }
        }
    }
}
