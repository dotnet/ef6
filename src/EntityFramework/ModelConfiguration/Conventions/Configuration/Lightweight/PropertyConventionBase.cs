// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;
    using PrimitivePropertyConfiguration =
        System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration;

    internal abstract class PropertyConventionBase :
        IConfigurationConvention<PropertyInfo, PrimitivePropertyConfiguration>
    {
        private readonly IEnumerable<Func<PropertyInfo, bool>> _predicates;

        public PropertyConventionBase(IEnumerable<Func<PropertyInfo, bool>> predicates)
        {
            DebugCheck.NotNull(predicates);

            _predicates = predicates;
        }

        internal IEnumerable<Func<PropertyInfo, bool>> Predicates
        {
            get { return _predicates; }
        }

        public void Apply(
            PropertyInfo memberInfo, Func<PrimitivePropertyConfiguration> configuration, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(modelConfiguration);

            if (_predicates.All(p => p(memberInfo)))
            {
                ApplyCore(memberInfo, configuration, modelConfiguration);
            }
        }

        protected abstract void ApplyCore(
            PropertyInfo memberInfo, Func<PrimitivePropertyConfiguration> configuration, ModelConfiguration modelConfiguration);
    }
}
