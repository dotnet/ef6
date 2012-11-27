// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Utilities;

    internal class EntityConventionWithHaving<T> : EntityConventionBase
        where T : class
    {
        private readonly Func<Type, T> _capturingPredicate;
        private readonly Action<LightweightEntityConfiguration, T> _entityConfigurationAction;

        public EntityConventionWithHaving(
            IEnumerable<Func<Type, bool>> predicates,
            Func<Type, T> capturingPredicate,
            Action<LightweightEntityConfiguration, T> entityConfigurationAction)
            : base(predicates)
        {
            DebugCheck.NotNull(predicates);
            DebugCheck.NotNull(capturingPredicate);
            DebugCheck.NotNull(entityConfigurationAction);

            _capturingPredicate = capturingPredicate;
            _entityConfigurationAction = entityConfigurationAction;
        }

        internal Func<Type, T> CapturingPredicate
        {
            get { return _capturingPredicate; }
        }

        internal Action<LightweightEntityConfiguration, T> EntityConfigurationAction
        {
            get { return _entityConfigurationAction; }
        }

        protected override void ApplyCore(Type memberInfo, Func<EntityTypeConfiguration> configuration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);

            var value = _capturingPredicate(memberInfo);

            if (value != null)
            {
                _entityConfigurationAction(new LightweightEntityConfiguration(memberInfo, configuration), value);
            }
        }
    }
}
