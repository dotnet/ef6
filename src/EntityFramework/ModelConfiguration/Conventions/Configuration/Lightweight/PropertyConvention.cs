// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.Utilities;
    using System.Reflection;

    internal class PropertyConvention : PropertyConventionBase
    {
        private readonly Action<LightweightPrimitivePropertyConfiguration> _propertyConfigurationAction;

        public PropertyConvention(
            IEnumerable<Func<PropertyInfo, bool>> predicates,
            Action<LightweightPrimitivePropertyConfiguration> propertyConfigurationAction)
            : base(predicates)
        {
            DebugCheck.NotNull(predicates);
            DebugCheck.NotNull(propertyConfigurationAction);

            _propertyConfigurationAction = propertyConfigurationAction;
        }

        internal Action<LightweightPrimitivePropertyConfiguration> PropertyConfigurationAction
        {
            get { return _propertyConfigurationAction; }
        }

        protected override void ApplyCore(
            PropertyInfo memberInfo, Func<PrimitivePropertyConfiguration> configuration, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(modelConfiguration);

            _propertyConfigurationAction(new LightweightPrimitivePropertyConfiguration(memberInfo, configuration));
        }
    }
}
