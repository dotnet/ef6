// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal abstract class EntityConventionBase : IConfigurationConvention<Type, EntityTypeConfiguration>
    {
        private readonly IEnumerable<Func<Type, bool>> _predicates;

        protected EntityConventionBase(IEnumerable<Func<Type, bool>> predicates)
        {
            DebugCheck.NotNull(predicates);

            _predicates = predicates;
        }

        internal IEnumerable<Func<Type, bool>> Predicates
        {
            get { return _predicates; }
        }

        public void Apply(Type memberInfo, Func<EntityTypeConfiguration> configuration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);

            if (_predicates.All(p => p(memberInfo)))
            {
                ApplyCore(memberInfo, configuration);
            }
        }

        protected abstract void ApplyCore(Type memberInfo, Func<EntityTypeConfiguration> configuration);
    }
}
