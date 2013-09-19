// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.ModelConfiguration.Configuration.Properties;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.Utilities;
    using System.Reflection;

    public partial class ConventionsConfiguration
    {
        private class PropertyConfigurationConventionDispatcher
        {
            private readonly IConvention _convention;
            private readonly Type _propertyConfigurationType;
            private readonly PropertyInfo _propertyInfo;
            private readonly Func<PropertyConfiguration> _propertyConfiguration;
            private readonly ModelConfiguration _modelConfiguration;

            public PropertyConfigurationConventionDispatcher(
                IConvention convention,
                Type propertyConfigurationType,
                PropertyInfo propertyInfo,
                Func<PropertyConfiguration> propertyConfiguration,
                ModelConfiguration modelConfiguration)
            {
                Check.NotNull(convention, "convention");
                Check.NotNull(propertyConfigurationType, "propertyConfigurationType");
                Check.NotNull(propertyInfo, "propertyInfo");
                Check.NotNull(propertyConfiguration, "propertyConfiguration");

                _convention = convention;
                _propertyConfigurationType = propertyConfigurationType;
                _propertyInfo = propertyInfo;
                _propertyConfiguration = propertyConfiguration;
                _modelConfiguration = modelConfiguration;
            }

            public void Dispatch()
            {
                Dispatch<PropertyConfiguration>();
                Dispatch<Properties.Primitive.PrimitivePropertyConfiguration>();
                Dispatch<Properties.Primitive.LengthPropertyConfiguration>();
                Dispatch<Properties.Primitive.DateTimePropertyConfiguration>();
                Dispatch<Properties.Primitive.DecimalPropertyConfiguration>();
                Dispatch<Properties.Primitive.StringPropertyConfiguration>();
                Dispatch<Properties.Primitive.BinaryPropertyConfiguration>();
                Dispatch<NavigationPropertyConfiguration>();
            }

            private void Dispatch<TPropertyConfiguration>()
                where TPropertyConfiguration : PropertyConfiguration
            {
                var propertyConfigurationConvention
                    = _convention as IConfigurationConvention<PropertyInfo, TPropertyConfiguration>;

                if ((propertyConfigurationConvention != null)
                    && typeof(TPropertyConfiguration).IsAssignableFrom(_propertyConfigurationType))
                {
                    propertyConfigurationConvention.Apply(
                        _propertyInfo, () => (TPropertyConfiguration)_propertyConfiguration(), _modelConfiguration);
                }
            }
        }
    }
}
