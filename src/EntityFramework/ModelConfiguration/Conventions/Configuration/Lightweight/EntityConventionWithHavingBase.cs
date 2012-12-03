// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Utilities;

    internal abstract class EntityConventionWithHavingBase<T> : EntityConventionBase
        where T : class
    {
        private readonly Func<Type, T> _capturingPredicate;

        public EntityConventionWithHavingBase(
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

        protected override sealed void ApplyCore(Type memberInfo, Func<EntityTypeConfiguration> configuration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);

            var value = _capturingPredicate(memberInfo);

            if (value != null)
            {
                InvokeAction(memberInfo, configuration, value);
            }
        }

        protected abstract void InvokeAction(Type memberInfo, Func<EntityTypeConfiguration> configuration, T value);
    }
}
