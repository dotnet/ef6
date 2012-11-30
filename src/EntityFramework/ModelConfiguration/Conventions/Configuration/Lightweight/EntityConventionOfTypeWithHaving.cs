// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Utilities;

    internal class EntityConventionOfTypeWithHaving<T, TValue> : EntityConventionWithHavingBase<TValue>
        where T : class
        where TValue : class
    {
        private readonly Action<LightweightEntityConfiguration<T>, TValue> _entityConfigurationAction;

        public EntityConventionOfTypeWithHaving(
           IEnumerable<Func<Type, bool>> predicates,
            Func<Type, TValue> capturingPredicate,
            Action<LightweightEntityConfiguration<T>, TValue> entityConfigurationAction)
            : base(predicates.Prepend(EntityConventionOfType<T>.OfTypePredicate), capturingPredicate)
        {
            DebugCheck.NotNull(predicates);
            DebugCheck.NotNull(capturingPredicate);
            DebugCheck.NotNull(entityConfigurationAction);

            _entityConfigurationAction = entityConfigurationAction;
        }

        internal Action<LightweightEntityConfiguration<T>, TValue> EntityConfigurationAction
        {
            get { return _entityConfigurationAction; }
        }

        protected override void InvokeAction(Type memberInfo, Func<EntityTypeConfiguration> configuration, TValue value)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(value);

            _entityConfigurationAction(new LightweightEntityConfiguration<T>(memberInfo, configuration), value);
        }
    }
}
