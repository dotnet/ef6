// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Utilities;

    internal class EntityConventionWithHaving<T> : EntityConventionWithHavingBase<T>
        where T : class
    {
        private readonly Action<LightweightEntityConfiguration, T> _entityConfigurationAction;

        public EntityConventionWithHaving(
            IEnumerable<Func<Type, bool>> predicates,
            Func<Type, T> capturingPredicate,
            Action<LightweightEntityConfiguration, T> entityConfigurationAction)
            : base(predicates, capturingPredicate)
        {
            DebugCheck.NotNull(predicates);
            DebugCheck.NotNull(capturingPredicate);
            DebugCheck.NotNull(entityConfigurationAction);

            _entityConfigurationAction = entityConfigurationAction;
        }

        internal Action<LightweightEntityConfiguration, T> EntityConfigurationAction
        {
            get { return _entityConfigurationAction; }
        }

        protected override void InvokeAction(
            Type memberInfo, ModelConfiguration modelConfiguration, T value)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(modelConfiguration);
            DebugCheck.NotNull(value);

            _entityConfigurationAction(new LightweightEntityConfiguration(memberInfo, modelConfiguration), value);
        }

        protected override void InvokeAction(
            Type memberInfo, Func<EntityTypeConfiguration> configuration, ModelConfiguration modelConfiguration, T value)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(modelConfiguration);
            DebugCheck.NotNull(value);

            _entityConfigurationAction(new LightweightEntityConfiguration(memberInfo, configuration, modelConfiguration), value);
        }

        protected override void InvokeAction(
            Type memberInfo, Func<ComplexTypeConfiguration> configuration, ModelConfiguration modelConfiguration, T value)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(value);

            _entityConfigurationAction(new LightweightEntityConfiguration(memberInfo, configuration, modelConfiguration), value);
        }
    }
}
