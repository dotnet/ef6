// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;

    /// <summary>
    ///     Convention to process instances of <see cref="ComplexTypeAttribute" /> found on types in the model.
    /// </summary>
    public sealed class ComplexTypeAttributeConvention : IConfigurationConvention<Type, ModelConfiguration>
    {
        private readonly IConfigurationConvention<Type, ModelConfiguration> _impl =
            new ComplexTypeAttributeConventionImpl();

        internal ComplexTypeAttributeConvention()
        {
        }

        void IConfigurationConvention<Type, ModelConfiguration>.Apply(
            Type memberInfo, Func<ModelConfiguration> configuration)
        {
            _impl.Apply(memberInfo, configuration);
        }

        internal sealed class ComplexTypeAttributeConventionImpl :
            AttributeConfigurationConvention<Type, ModelConfiguration, ComplexTypeAttribute>
        {
            internal override void Apply(Type type, ModelConfiguration modelConfiguration, ComplexTypeAttribute _)
            {
                modelConfiguration.ComplexType(type);
            }
        }
    }
}
