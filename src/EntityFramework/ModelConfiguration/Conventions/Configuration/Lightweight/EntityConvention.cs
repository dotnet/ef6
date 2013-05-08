// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Utilities;

    internal class EntityConvention : EntityConventionBase
    {
        private readonly Action<LightweightEntityConfiguration> _entityConfigurationAction;

        public EntityConvention(
            IEnumerable<Func<Type, bool>> predicates,
            Action<LightweightEntityConfiguration> entityConfigurationAction)
            : base(predicates)
        {
            DebugCheck.NotNull(predicates);
            DebugCheck.NotNull(entityConfigurationAction);

            _entityConfigurationAction = entityConfigurationAction;
        }

        internal Action<LightweightEntityConfiguration> EntityConfigurationAction
        {
            get { return _entityConfigurationAction; }
        }

        protected override void ApplyCore(
            Type memberInfo, Func<EntityTypeConfiguration> configuration, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(modelConfiguration);

            _entityConfigurationAction(new LightweightEntityConfiguration(memberInfo, configuration));
        }
    }
}
