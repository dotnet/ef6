// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Utilities;

    internal class TypeConvention<T> : TypeConventionBase
        where T : class
    {
        private static readonly Func<Type, bool> _ofTypePredicate = t => typeof(T).IsAssignableFrom(t);
        private readonly Action<ConventionTypeConfiguration<T>> _entityConfigurationAction;

        public TypeConvention(
            IEnumerable<Func<Type, bool>> predicates,
            Action<ConventionTypeConfiguration<T>> entityConfigurationAction)
            : base(predicates.Prepend(_ofTypePredicate))
        {
            DebugCheck.NotNull(predicates);
            DebugCheck.NotNull(entityConfigurationAction);

            _entityConfigurationAction = entityConfigurationAction;
        }

        internal Action<ConventionTypeConfiguration<T>> EntityConfigurationAction
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

            _entityConfigurationAction(new ConventionTypeConfiguration<T>(memberInfo, modelConfiguration));
        }

        protected override void ApplyCore(
            Type memberInfo, Func<EntityTypeConfiguration> configuration, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(modelConfiguration);

            _entityConfigurationAction(new ConventionTypeConfiguration<T>(memberInfo, configuration, modelConfiguration));
        }

        protected override void ApplyCore(
            Type memberInfo, Func<ComplexTypeConfiguration> configuration, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(modelConfiguration);

            _entityConfigurationAction(new ConventionTypeConfiguration<T>(memberInfo, configuration, modelConfiguration));
        }
    }
}
