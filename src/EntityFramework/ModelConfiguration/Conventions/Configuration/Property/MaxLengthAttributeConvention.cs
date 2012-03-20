namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref = "MaxLengthAttribute" /> found on properties in the model.
    /// </summary>
    public sealed class MaxLengthAttributeConvention
        : IConfigurationConvention<PropertyInfo, LengthPropertyConfiguration>
    {
        private readonly IConfigurationConvention<PropertyInfo, LengthPropertyConfiguration> _impl
            = new MaxLengthAttributeConventionImpl();

        internal MaxLengthAttributeConvention()
        {
        }

        void IConfigurationConvention<PropertyInfo, LengthPropertyConfiguration>.Apply(
            PropertyInfo memberInfo, Func<LengthPropertyConfiguration> configuration)
        {
            _impl.Apply(memberInfo, configuration);
        }

        internal sealed class MaxLengthAttributeConventionImpl
            : AttributeConfigurationConvention<PropertyInfo, LengthPropertyConfiguration, MaxLengthAttribute>
        {
            private const int MaxLengthIndicator = -1;

            internal override void Apply(
                PropertyInfo propertyInfo, LengthPropertyConfiguration lengthPropertyConfiguration, MaxLengthAttribute maxLengthAttribute)
            {
                if ((maxLengthAttribute.Length == 0)
                    || (maxLengthAttribute.Length < MaxLengthIndicator))
                {
                    throw Error.MaxLengthAttributeConvention_InvalidMaxLength(propertyInfo.Name, propertyInfo.ReflectedType);
                }

                // Set the length if the length configuration's maxlength is not yet set
                if (lengthPropertyConfiguration.IsMaxLength == null && lengthPropertyConfiguration.MaxLength == null)
                {
                    if (maxLengthAttribute.Length == MaxLengthIndicator)
                    {
                        lengthPropertyConfiguration.IsMaxLength = true;
                    }
                    else
                    {
                        lengthPropertyConfiguration.MaxLength = maxLengthAttribute.Length;
                    }
                }
            }
        }
    }
}