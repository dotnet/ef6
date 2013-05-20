// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal abstract class TypeConventionBase : IConfigurationConvention<Type, EntityTypeConfiguration>,
                                                   IConfigurationConvention<Type, ComplexTypeConfiguration>,
                                                   IConfigurationConvention<Type>
    {
        private readonly IEnumerable<Func<Type, bool>> _predicates;

        protected TypeConventionBase(IEnumerable<Func<Type, bool>> predicates)
        {
            DebugCheck.NotNull(predicates);

            _predicates = predicates;
        }

        internal IEnumerable<Func<Type, bool>> Predicates
        {
            get { return _predicates; }
        }

        public void Apply(Type memberInfo, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(modelConfiguration);

            if (_predicates.All(p => p(memberInfo)))
            {
                ApplyCore(memberInfo, modelConfiguration);
            }
        }

        protected abstract void ApplyCore(Type memberInfo, ModelConfiguration modelConfiguration);

        public void Apply(Type memberInfo, Func<EntityTypeConfiguration> configuration, ModelConfiguration modelConfiguration)
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
            Type memberInfo, Func<EntityTypeConfiguration> configuration, ModelConfiguration modelConfiguration);

        public void Apply(Type memberInfo, Func<ComplexTypeConfiguration> configuration, ModelConfiguration modelConfiguration)
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
            Type memberInfo, Func<ComplexTypeConfiguration> configuration, ModelConfiguration modelConfiguration);
    }
}
