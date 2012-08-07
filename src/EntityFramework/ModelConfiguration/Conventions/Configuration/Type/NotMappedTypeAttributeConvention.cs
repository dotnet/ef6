// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;

    /// <summary>
    ///     Convention to process instances of <see cref="NotMappedAttribute" /> found on types in the model.
    /// </summary>
    public sealed class NotMappedTypeAttributeConvention : IConfigurationConvention<Type, ModelConfiguration>
    {
        private readonly IConfigurationConvention<Type, ModelConfiguration> _impl =
            new NotMappedTypeAttributeConventionImpl();

        internal NotMappedTypeAttributeConvention()
        {
        }

        void IConfigurationConvention<Type, ModelConfiguration>.Apply(
            Type memberInfo, Func<ModelConfiguration> configuration)
        {
            _impl.Apply(memberInfo, configuration);
        }

        internal sealed class NotMappedTypeAttributeConventionImpl :
            AttributeConfigurationConvention<Type, ModelConfiguration, NotMappedAttribute>
        {
            internal override void Apply(Type type, ModelConfiguration modelConfiguration, NotMappedAttribute _)
            {
                modelConfiguration.Ignore(type);
            }
        }
    }
}
