// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Utilities;

    internal class TypeConventionOfType<T> : TypeConventionBase
        where T : class
    {
        private static readonly Func<Type, bool> _ofTypePredicate = t => typeof(T).IsAssignableFrom(t);
        private readonly Action<LightweightTypeConfiguration<T>> _entityConfigurationAction;

        public TypeConventionOfType(
            IEnumerable<Func<Type, bool>> predicates,
            Action<LightweightTypeConfiguration<T>> entityConfigurationAction)
            : base(predicates.Prepend(_ofTypePredicate))
        {
            DebugCheck.NotNull(predicates);
            DebugCheck.NotNull(entityConfigurationAction);

            _entityConfigurationAction = entityConfigurationAction;
        }

        internal Action<LightweightTypeConfiguration<T>> EntityConfigurationAction
        {
            get { return _entityConfigurationAction; }
        }

        internal static Func<Type, bool> OfTypePredicate
        {
            get { return _ofTypePredicate; }
        }

        protected override void ApplyCore(Type memberInfo, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(modelConfiguration);

            _entityConfigurationAction(new LightweightTypeConfiguration<T>(memberInfo, modelConfiguration));
        }

        protected override void ApplyCore(
            Type memberInfo, Func<EntityTypeConfiguration> configuration, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(modelConfiguration);

            _entityConfigurationAction(new LightweightTypeConfiguration<T>(memberInfo, configuration, modelConfiguration));
        }

        protected override void ApplyCore(
            Type memberInfo, Func<ComplexTypeConfiguration> configuration, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(modelConfiguration);

            _entityConfigurationAction(new LightweightTypeConfiguration<T>(memberInfo, configuration, modelConfiguration));
        }
    }
}
