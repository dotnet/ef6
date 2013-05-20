// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Utilities;

    internal abstract class TypeConventionWithHavingBase<T> : TypeConventionBase
        where T : class
    {
        private readonly Func<Type, T> _capturingPredicate;

        public TypeConventionWithHavingBase(
            IEnumerable<Func<Type, bool>> predicates,
            Func<Type, T> capturingPredicate)
            : base(predicates)
        {
            DebugCheck.NotNull(predicates);
            DebugCheck.NotNull(capturingPredicate);

            _capturingPredicate = capturingPredicate;
        }

        internal Func<Type, T> CapturingPredicate
        {
            get { return _capturingPredicate; }
        }

        protected override void ApplyCore(Type memberInfo, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(modelConfiguration);

            var value = _capturingPredicate(memberInfo);

            if (value != null)
            {
                InvokeAction(memberInfo, modelConfiguration, value);
            }
        }

        protected abstract void InvokeAction(Type memberInfo, ModelConfiguration configuration, T value);

        protected override sealed void ApplyCore(
            Type memberInfo, Func<EntityTypeConfiguration> configuration, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(modelConfiguration);

            var value = _capturingPredicate(memberInfo);

            if (value != null)
            {
                InvokeAction(memberInfo, configuration, modelConfiguration, value);
            }
        }

        protected abstract void InvokeAction(
            Type memberInfo, Func<EntityTypeConfiguration> configuration, ModelConfiguration modelConfiguration, T value);

        protected override void ApplyCore(
            Type memberInfo, Func<ComplexTypeConfiguration> configuration, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(modelConfiguration);

            var value = _capturingPredicate(memberInfo);

            if (value != null)
            {
                InvokeAction(memberInfo, configuration, modelConfiguration, value);
            }
        }

        protected abstract void InvokeAction(
            Type memberInfo, Func<ComplexTypeConfiguration> configuration, ModelConfiguration modelConfiguration, T value);
    }
}
