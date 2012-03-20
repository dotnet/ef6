namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref = "TimestampAttribute" /> found on properties in the model.
    /// </summary>
    public sealed class TimestampAttributeConvention
        : IConfigurationConvention<PropertyInfo, BinaryPropertyConfiguration>
    {
        private readonly IConfigurationConvention<PropertyInfo, BinaryPropertyConfiguration> _impl
            = new TimestampAttributeConventionImpl();

        internal TimestampAttributeConvention()
        {
        }

        void IConfigurationConvention<PropertyInfo, BinaryPropertyConfiguration>.Apply(
            PropertyInfo memberInfo, Func<BinaryPropertyConfiguration> configuration)
        {
            _impl.Apply(memberInfo, configuration);
        }

        internal sealed class TimestampAttributeConventionImpl
            : AttributeConfigurationConvention<PropertyInfo, BinaryPropertyConfiguration, TimestampAttribute>
        {
            internal override void Apply(
                PropertyInfo propertyInfo, BinaryPropertyConfiguration binaryPropertyConfiguration, TimestampAttribute _)
            {
                if (binaryPropertyConfiguration.IsRowVersion == null)
                {
                    binaryPropertyConfiguration.IsRowVersion = true;
                }
            }
        }
    }
}