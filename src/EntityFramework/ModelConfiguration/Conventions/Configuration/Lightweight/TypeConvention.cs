// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Utilities;

    internal class TypeConvention : TypeConventionBase
    {
        private readonly Action<ConventionTypeConfiguration> _entityConfigurationAction;

        public TypeConvention(
            IEnumerable<Func<Type, bool>> predicates,
            Action<ConventionTypeConfiguration> entityConfigurationAction)
            : base(predicates)
        {
            DebugCheck.NotNull(predicates);
            DebugCheck.NotNull(entityConfigurationAction);

            _entityConfigurationAction = entityConfigurationAction;
        }

        internal Action<ConventionTypeConfiguration> EntityConfigurationAction
        {
            get { return _entityConfigurationAction; }
        }

        protected override void ApplyCore(Type memberInfo, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(modelConfiguration);

            _entityConfigurationAction(new ConventionTypeConfiguration(memberInfo, modelConfiguration));
        }

        protected override void ApplyCore(
            Type memberInfo, Func<EntityTypeConfiguration> configuration, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(modelConfiguration);

            _entityConfigurationAction(new ConventionTypeConfiguration(memberInfo, configuration, modelConfiguration));
        }

        protected override void ApplyCore(
            Type memberInfo, Func<ComplexTypeConfiguration> configuration, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(modelConfiguration);

            _entityConfigurationAction(new ConventionTypeConfiguration(memberInfo, configuration, modelConfiguration));
        }
    }
}
