// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Utilities;

    internal class TypeConventionWithHaving<T, TValue> : TypeConventionWithHavingBase<TValue>
        where T : class
        where TValue : class
    {
        private readonly Action<ConventionTypeConfiguration<T>, TValue> _entityConfigurationAction;

        public TypeConventionWithHaving(
            IEnumerable<Func<Type, bool>> predicates,
            Func<Type, TValue> capturingPredicate,
            Action<ConventionTypeConfiguration<T>, TValue> entityConfigurationAction)
            : base(predicates.Prepend(TypeConvention<T>.OfTypePredicate), capturingPredicate)
        {
            DebugCheck.NotNull(predicates);
            DebugCheck.NotNull(capturingPredicate);
            DebugCheck.NotNull(entityConfigurationAction);

            _entityConfigurationAction = entityConfigurationAction;
        }

        internal Action<ConventionTypeConfiguration<T>, TValue> EntityConfigurationAction
        {
            get { return _entityConfigurationAction; }
        }

        protected override void InvokeAction(Type memberInfo, ModelConfiguration modelConfiguration, TValue value)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(modelConfiguration);
            DebugCheck.NotNull(value);

            _entityConfigurationAction(new ConventionTypeConfiguration<T>(memberInfo, modelConfiguration), value);
        }

        protected override void InvokeAction(
            Type memberInfo, Func<EntityTypeConfiguration> configuration, ModelConfiguration modelConfiguration, TValue value)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(modelConfiguration);
            DebugCheck.NotNull(value);

            _entityConfigurationAction(new ConventionTypeConfiguration<T>(memberInfo, configuration, modelConfiguration), value);
        }

        protected override void InvokeAction(
            Type memberInfo, Func<ComplexTypeConfiguration> configuration, ModelConfiguration modelConfiguration, TValue value)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(modelConfiguration);
            DebugCheck.NotNull(value);

            _entityConfigurationAction(new ConventionTypeConfiguration<T>(memberInfo, configuration, modelConfiguration), value);
        }
    }
}
